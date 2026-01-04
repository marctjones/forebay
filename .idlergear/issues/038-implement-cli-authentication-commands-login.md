---
id: 38
title: Implement CLI authentication commands (login, logout, whoami)
state: open
created: '2026-01-04T09:23:20.306301Z'
labels:
- cli
- auth
- csharp
priority: high
---
Implement authentication commands for the Forebay CLI using Google OAuth PKCE flow.

**Commands to Implement:**

### 1. `forebay login`

**Flow:**
1. Check if already logged in (valid session in config)
2. Start local HTTP server on `http://localhost:8080`
3. Generate PKCE code_verifier and code_challenge
4. Open browser to Google OAuth consent URL:
   ```
   https://accounts.google.com/o/oauth2/v2/auth
     ?client_id={GOOGLE_CLIENT_ID}
     &redirect_uri=http://localhost:8080/callback
     &response_type=code
     &scope=openid email profile
     &code_challenge={challenge}
     &code_challenge_method=S256
   ```
5. Wait for callback with authorization code
6. Exchange code for ID token at Google token endpoint
7. Send ID token to Worker `POST /auth/login`
8. Save session_token to config file
9. Display success message with user email

**Implementation:**
- Use `System.Diagnostics.Process.Start()` to open browser
- Use `HttpListener` for local callback server
- PKCE challenge: SHA256(code_verifier) base64url encoded
- Timeout after 5 minutes if no callback received

### 2. `forebay logout`

**Flow:**
1. Read session token from config
2. Call Worker `POST /auth/logout`
3. Delete config file (or clear auth section)
4. Display success message

### 3. `forebay whoami`

**Flow:**
1. Read session token from config
2. Call Worker `GET /auth/whoami`
3. Display user email and session expiry
4. Show warning if session expires soon (<7 days)

**Error Handling:**
- No config file → prompt to run `forebay login`
- Expired session → prompt to run `forebay login` again
- Network errors → show helpful message
- Invalid OAuth state → security error message

**Acceptance Criteria:**
- [ ] OAuth PKCE flow works end-to-end
- [ ] Browser opens automatically
- [ ] Session token saved securely (600 permissions on Unix)
- [ ] Login shows user email on success
- [ ] Logout clears session
- [ ] Whoami shows current user info
- [ ] All error cases handled gracefully
- [ ] Config file format documented

**Dependencies:**
- Google OAuth 2.0 (no library needed, direct HTTP)
- System.Diagnostics for browser launch
- HttpListener for local server

**Testing:**
- Manual testing with real Google account
- Test expired session handling
- Test network error scenarios
