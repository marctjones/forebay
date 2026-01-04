---
id: 1
title: Authentication Implementation Guide
created: '2026-01-04T06:18:39.594997Z'
updated: '2026-01-04T06:18:39.595008Z'
---
# Authentication Implementation Guide

## Overview
Forebay uses Google OAuth + session tokens for secure, user-friendly authentication.

## Architecture

### Authentication Flow
```
1. User: forebay login
2. CLI: Opens browser → Google OAuth consent
3. CLI: Runs local server at localhost:8080
4. Google: Redirects to localhost:8080/callback with auth code
5. CLI: Exchanges code for ID token (PKCE)
6. CLI: Sends ID token to Worker POST /auth/login
7. Worker: Validates token, checks ALLOWED_EMAILS
8. Worker: Creates session token, stores in KV
9. Worker: Returns session token to CLI
10. CLI: Saves session token to ~/.config/forebay/config.toml
```

### Daily Usage Flow
```
1. User: forebay enqueue work/tasks "item"
2. CLI: Reads session token from config.toml
3. CLI: Sends request with Authorization: Bearer <session>
4. Worker: Validates session token in KV
5. Worker: Processes request
```

## Worker Implementation

### Environment Variables
```bash
# Required
ALLOWED_EMAILS=user@gmail.com,teammate@company.com
GOOGLE_CLIENT_ID=123456.apps.googleusercontent.com

# Optional
SESSION_TTL_DAYS=30
```

### KV Namespace
```bash
# Create KV namespace
wrangler kv:namespace create SESSION_TOKENS

# wrangler.toml
[[kv_namespaces]]
binding = "SESSION_TOKENS"
id = "your-kv-namespace-id"
```

### Login Endpoint
```typescript
// POST /auth/login
// Body: { "id_token": "google-jwt" }

import { OAuth2Client } from 'google-auth-library';

async function handleLogin(request: Request, env: Env): Promise<Response> {
  const { id_token } = await request.json();
  
  // Validate Google ID token
  const client = new OAuth2Client(env.GOOGLE_CLIENT_ID);
  const ticket = await client.verifyIdToken({
    idToken: id_token,
    audience: env.GOOGLE_CLIENT_ID,
  });
  
  const payload = ticket.getPayload();
  const email = payload.email;
  
  // Check allowlist
  const allowedEmails = env.ALLOWED_EMAILS.split(',');
  if (!allowedEmails.includes(email)) {
    return new Response('Unauthorized email', { status: 403 });
  }
  
  // Generate session token
  const sessionToken = crypto.randomUUID();
  const sessionData = {
    email: email,
    created_at: Date.now(),
    device_name: request.headers.get('User-Agent') || 'unknown'
  };
  
  // Store in KV with 30-day TTL
  const ttlSeconds = (env.SESSION_TTL_DAYS || 30) * 86400;
  await env.SESSION_TOKENS.put(
    sessionToken,
    JSON.stringify(sessionData),
    { expirationTtl: ttlSeconds }
  );
  
  return new Response(JSON.stringify({ 
    session_token: sessionToken,
    email: email,
    expires_in: ttlSeconds
  }), {
    headers: { 'Content-Type': 'application/json' }
  });
}
```

### Auth Middleware
```typescript
async function authenticate(request: Request, env: Env): Promise<string | null> {
  // Extract token
  const authHeader = request.headers.get('Authorization');
  if (!authHeader?.startsWith('Bearer ')) {
    return null;
  }
  
  const token = authHeader.substring(7);
  
  // Check static tokens (CI/CD)
  if (env.STATIC_AUTH_TOKENS?.split(',').includes(token)) {
    return 'static-token-user';
  }
  
  // Validate session token
  const sessionData = await env.SESSION_TOKENS.get(token);
  if (!sessionData) {
    return null;
  }
  
  const session = JSON.parse(sessionData);
  return session.email;
}

// Use in request handler
export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    // Public endpoints
    if (request.url.endsWith('/auth/login')) {
      return handleLogin(request, env);
    }
    
    // Require auth
    const email = await authenticate(request, env);
    if (!email) {
      return new Response('Unauthorized', { status: 401 });
    }
    
    // Process authenticated request
    // (email available for logging/audit)
    return handleQueueOperation(request, env, email);
  }
}
```

## CLI Implementation

### OAuth PKCE Flow
```rust
// src/auth.rs
use oauth2::{
    AuthUrl, ClientId, CsrfToken, PkceCodeChallenge, RedirectUrl,
    Scope, TokenUrl, AuthorizationCode, PkceCodeVerifier
};
use tiny_http::{Server, Response};

pub async fn login(config: &mut Config) -> Result<()> {
    let client_id = ClientId::new(GOOGLE_CLIENT_ID.to_string());
    
    // PKCE challenge
    let (pkce_challenge, pkce_verifier) = PkceCodeChallenge::new_random_sha256();
    
    // OAuth URLs
    let auth_url = AuthUrl::new("https://accounts.google.com/o/oauth2/v2/auth".to_string())?;
    let token_url = TokenUrl::new("https://oauth2.googleapis.com/token".to_string())?;
    let redirect_url = RedirectUrl::new("http://localhost:8080/callback".to_string())?;
    
    let client = BasicClient::new(client_id, None, auth_url, Some(token_url))
        .set_redirect_uri(redirect_url);
    
    // Generate auth URL
    let (auth_url, csrf_token) = client
        .authorize_url(CsrfToken::new_random)
        .add_scope(Scope::new("openid".to_string()))
        .add_scope(Scope::new("email".to_string()))
        .set_pkce_challenge(pkce_challenge)
        .url();
    
    // Open browser
    println!("Opening browser for Google login...");
    open::that(auth_url.to_string())?;
    
    // Start local server
    let server = Server::http("localhost:8080").unwrap();
    println!("Waiting for callback...");
    
    for request in server.incoming_requests() {
        if request.url().starts_with("/callback") {
            // Extract code from query params
            let url = format!("http://localhost:8080{}", request.url());
            let url = Url::parse(&url)?;
            let code = url.query_pairs()
                .find(|(key, _)| key == "code")
                .map(|(_, value)| value.to_string())
                .ok_or("No code in callback")?;
            
            // Exchange code for ID token
            let token_result = client
                .exchange_code(AuthorizationCode::new(code))
                .set_pkce_verifier(pkce_verifier)
                .request_async(async_http_client)
                .await?;
            
            let id_token = token_result.extra_fields().id_token()
                .ok_or("No ID token")?;
            
            // Send to Worker
            let session_token = exchange_id_token_for_session(
                &config.worker_url,
                id_token.as_str()
            ).await?;
            
            // Save to config
            config.session_token = Some(session_token.clone());
            config.save()?;
            
            // Respond to browser
            request.respond(Response::from_string("✓ Login successful! You can close this window."))?;
            
            println!("✓ Logged in successfully!");
            break;
        }
    }
    
    Ok(())
}

async fn exchange_id_token_for_session(worker_url: &str, id_token: &str) -> Result<String> {
    let client = reqwest::Client::new();
    let res = client
        .post(format!("{}/auth/login", worker_url))
        .json(&serde_json::json!({ "id_token": id_token }))
        .send()
        .await?;
    
    if !res.status().is_success() {
        return Err(format!("Login failed: {}", res.status()).into());
    }
    
    let body: serde_json::Value = res.json().await?;
    let session_token = body["session_token"]
        .as_str()
        .ok_or("No session token in response")?
        .to_string();
    
    Ok(session_token)
}
```

### Config Structure
```rust
// ~/.config/forebay/config.toml
#[derive(Serialize, Deserialize)]
pub struct Config {
    pub worker_url: String,
    pub session_token: Option<String>,
    pub email: Option<String>,
}
```

### Authenticated Requests
```rust
pub async fn enqueue(queue: &str, item: &str, config: &Config) -> Result<()> {
    let session_token = config.session_token
        .as_ref()
        .ok_or("Not logged in. Run: forebay login")?;
    
    let client = reqwest::Client::new();
    let res = client
        .post(format!("{}/q/{}/enqueue", config.worker_url, queue))
        .header("Authorization", format!("Bearer {}", session_token))
        .json(&serde_json::json!({ "item": item }))
        .send()
        .await?;
    
    if res.status() == 401 {
        return Err("Session expired. Please run: forebay login".into());
    }
    
    res.error_for_status()?;
    Ok(())
}
```

### Token Management Commands
```bash
# Login
forebay login

# Check current session
forebay whoami
# Output: Logged in as user@gmail.com

# Logout (delete local session)
forebay logout

# Create static token (for CI/CD)
forebay token create --name "github-actions"
# Output: static token (add to Worker env STATIC_AUTH_TOKENS)
```

## Google Cloud Setup

### Create OAuth Client
1. Go to Google Cloud Console
2. Create project or select existing
3. Enable Google+ API
4. Credentials → Create OAuth 2.0 Client ID
5. Application type: Desktop app
6. Authorized redirect URIs: `http://localhost:8080/callback`
7. Copy Client ID

### Store Client ID
```rust
// Hardcode in CLI (public, not secret)
const GOOGLE_CLIENT_ID: &str = "123456.apps.googleusercontent.com";
```

## Security Considerations

### Session Token Security
- 30-day expiration (configurable)
- Stored in KV (encrypted at rest)
- Revocable (delete from KV)
- Per-device (each login creates new session)

### Google OAuth Security
- PKCE prevents auth code interception
- No client secret needed (native app flow)
- Google handles 2FA, breach detection
- User can revoke via Google account settings

### Email Allowlist
- Checked on every login
- Easy to update (Worker env var)
- Supports multiple emails (comma-separated)

### Static Tokens
- For CI/CD only
- Generate via admin command
- Store securely in CI secrets
- Clearly marked in audit logs

## Testing

### Manual Test Flow
```bash
# 1. Deploy Worker with test email
wrangler secret put ALLOWED_EMAILS
# Enter: test@gmail.com

wrangler secret put GOOGLE_CLIENT_ID
# Enter: your-client-id

# 2. Test login
forebay login
# Should open browser, redirect to localhost, succeed

# 3. Test authenticated request
forebay enqueue test/queue "hello"
# Should succeed

# 4. Test expiry (manually delete from KV)
wrangler kv:key delete --binding SESSION_TOKENS "<session-token>"
forebay enqueue test/queue "world"
# Should fail with 401
```

## Troubleshooting

### "Unauthorized email"
- Email not in ALLOWED_EMAILS
- Check Worker env var: `wrangler secret list`

### "Session expired"
- Session token deleted or expired (30 days)
- Run `forebay login` again

### "Failed to open browser"
- Copy URL from terminal
- Open manually in browser
- Continue with flow

### Localhost callback fails
- Port 8080 may be in use
- CLI should try ports 8080-8089

## Future Enhancements

### Token Rotation
- Auto-refresh sessions before expiry
- Background token refresh

### MFA
- Require Google 2FA
- Check via Google ID token claims

### Audit Logging
- Log all operations with email
- Store in D1 for compliance
- Query: "Show all ops by user@gmail.com"

### Team Management
- Store allowed emails in D1
- Admin UI for managing access
- Invite flow

### Device Management
- List active sessions
- Revoke specific devices
- Name devices during login
