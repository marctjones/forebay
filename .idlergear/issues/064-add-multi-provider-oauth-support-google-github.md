---
id: 64
title: Add multi-provider OAuth support (Google, GitHub, Microsoft)
state: open
created: '2026-01-05T00:27:04.734194Z'
labels:
- auth
- enhancement
- oauth2
- configuration
priority: medium
---
Enable configurable OAuth providers instead of hardcoded Google-only authentication.

**Problem:** Currently hardcoded to Google OAuth. Users may want:
1. Different OAuth provider (GitHub, Microsoft, custom OIDC)
2. Multiple providers simultaneously (Google OR GitHub)
3. Self-hosted OAuth (Keycloak, Auth0, Okta)

**Use Cases:**
- **Personal use:** Prefer GitHub login over Google
- **Enterprise:** Use Microsoft Entra ID (Azure AD)
- **Privacy-focused:** Self-hosted Keycloak
- **Flexibility:** Allow Google OR GitHub login

## Implementation Options

### Option 1: Single Provider (Configurable)
**Simplest approach** - Choose one provider at deployment time

**Configuration:**
```toml
# wrangler.toml
[env.production.vars]
OAUTH_PROVIDER = "google"  # or "github", "microsoft", "oidc"
OAUTH_CLIENT_ID = "your-client-id"
OAUTH_ISSUER = "https://accounts.google.com"  # For OIDC discovery
ALLOWED_EMAILS = "user@example.com"
```

**Supported Providers:**
1. **Google** (current)
   - JWKS: `https://www.googleapis.com/oauth2/v3/certs`
   - Issuer: `accounts.google.com`
   - User ID: email

2. **GitHub**
   - JWKS: GitHub doesn't use JWT, uses access tokens
   - API: `https://api.github.com/user`
   - User ID: username or email

3. **Microsoft**
   - JWKS: `https://login.microsoftonline.com/common/discovery/v2.0/keys`
   - Issuer: `https://login.microsoftonline.com/{tenant}/v2.0`
   - User ID: email or UPN

4. **Generic OIDC**
   - JWKS: Auto-discovered from issuer
   - Issuer: Configurable
   - User ID: Configurable claim (email, sub, preferred_username)

### Option 2: Multi-Provider Support
**More complex** - Allow multiple providers simultaneously

**Configuration:**
```toml
[env.production.vars]
OAUTH_PROVIDERS = "google,github"  # Comma-separated
GOOGLE_CLIENT_ID = "..."
GITHUB_CLIENT_ID = "..."
ALLOWED_USERS = "user@gmail.com,githubuser,user@company.com"
```

**Login Flow:**
```bash
forebay login --provider google   # Explicit provider
forebay login --provider github
forebay login                      # Show provider menu
```

## Architecture Changes

### Current (Google-only):
```rust
async fn verify_google_token(token: &str) -> Result<Claims> {
    let jwks = fetch_google_jwks().await?;
    // Hardcoded Google logic
}
```

### Proposed (Provider-agnostic):
```rust
trait OAuthProvider {
    async fn verify_token(&self, token: &str) -> Result<UserInfo>;
    fn get_jwks_url(&self) -> &str;
    fn get_issuer(&self) -> &str;
}

struct GoogleProvider { ... }
struct GitHubProvider { ... }
struct MicrosoftProvider { ... }
struct OidcProvider { issuer: String }

async fn verify_token(token: &str, env: &Env) -> Result<UserInfo> {
    let provider = get_configured_provider(env)?;
    provider.verify_token(token).await
}
```

### User Identification

**Current:** Email-based allowlist
```
ALLOWED_EMAILS = "user@gmail.com,admin@company.com"
```

**Proposed:** Provider-specific user IDs
```
ALLOWED_USERS = "google:user@gmail.com,github:octocat,microsoft:user@company.com"
```

Or simpler: Just emails for all providers that support them
```
ALLOWED_EMAILS = "user@gmail.com,user@company.com"
```

## Configuration Examples

### Google-only (current):
```toml
[env.production.vars]
OAUTH_PROVIDER = "google"
GOOGLE_CLIENT_ID = "xxx.apps.googleusercontent.com"
ALLOWED_EMAILS = "user@gmail.com"
```

### GitHub-only:
```toml
[env.production.vars]
OAUTH_PROVIDER = "github"
GITHUB_CLIENT_ID = "Iv1.xxxxxxxxx"
ALLOWED_USERS = "octocat,defunkt"  # GitHub usernames
```

### Microsoft Entra ID:
```toml
[env.production.vars]
OAUTH_PROVIDER = "microsoft"
MICROSOFT_TENANT_ID = "common"  # or specific tenant
MICROSOFT_CLIENT_ID = "xxx"
ALLOWED_EMAILS = "user@company.com"
```

### Self-hosted OIDC (Keycloak):
```toml
[env.production.vars]
OAUTH_PROVIDER = "oidc"
OIDC_ISSUER = "https://keycloak.company.com/realms/main"
OIDC_CLIENT_ID = "forebay"
ALLOWED_EMAILS = "user@company.com"
```

### Multi-provider:
```toml
[env.production.vars]
OAUTH_PROVIDERS = "google,github"
GOOGLE_CLIENT_ID = "xxx"
GITHUB_CLIENT_ID = "yyy"
ALLOWED_USERS = "google:user@gmail.com,github:octocat"
```

## CLI Changes

### Single Provider:
```bash
forebay login
# Auto-detects provider from Worker config
# Opens correct OAuth URL
```

### Multi-Provider:
```bash
forebay login
# Shows menu:
# 1. Google
# 2. GitHub
# Choose provider: 

forebay login --provider github
# Explicit provider selection
```

## Migration Path

**Phase 1 (v1.0):** Google-only (current plan)
- Simplest to implement
- Most users have Google accounts
- ✅ Already designed

**Phase 2 (v1.1):** Single configurable provider
- Add GitHub, Microsoft, OIDC support
- Choose one at deployment time
- Backward compatible (Google is default)

**Phase 3 (v1.2+):** Multi-provider support
- Allow multiple providers
- More complex but maximum flexibility

## Recommendation

**For v1.0:** Keep Google-only
- Simpler implementation
- Most users have Google
- Can add providers later without breaking changes

**For v1.1:** Add configurable single provider
- Support GitHub (developers)
- Support Microsoft (enterprises)
- Support generic OIDC (self-hosted)

**User ID Configuration:**
- Keep email-based allowlist for all providers
- Most providers support email in claims
- Simpler than provider-specific IDs

## Tasks

**Phase 1 (v1.0 - Google only):**
- [x] Design Google OAuth flow
- [ ] Implement Google JWT verification (#36)
- [ ] Email allowlist validation

**Phase 2 (v1.1 - Multi-provider):**
- [ ] Create OAuthProvider trait
- [ ] Implement GoogleProvider
- [ ] Implement GitHubProvider
- [ ] Implement MicrosoftProvider
- [ ] Implement OidcProvider (generic)
- [ ] Add provider configuration to wrangler.toml
- [ ] Update CLI to detect provider
- [ ] Provider selection UI (if multi-provider)
- [ ] Update documentation with provider setup
- [ ] Migration guide from Google-only to multi-provider

## Acceptance Criteria

**v1.0:**
- [ ] Google OAuth works with email allowlist

**v1.1:**
- [ ] Can configure GitHub as OAuth provider
- [ ] Can configure Microsoft as OAuth provider
- [ ] Can configure generic OIDC provider
- [ ] CLI auto-detects configured provider
- [ ] Email-based allowlist works for all providers
- [ ] Documentation covers all supported providers
- [ ] Migration from v1.0 works seamlessly

## Security Considerations

- ✅ Each provider verified via JWKS/JWT
- ✅ Email-based allowlist consistent across providers
- ✅ No hardcoded secrets (client IDs are public)
- ✅ TLS enforced for all OAuth flows
- ⚠️ GitHub uses access tokens, not JWTs (different verification)
- ⚠️ Provider configuration must be validated at startup

## References

- **Google:** https://developers.google.com/identity/protocols/oauth2
- **GitHub:** https://docs.github.com/en/apps/oauth-apps
- **Microsoft:** https://learn.microsoft.com/en-us/entra/identity-platform/v2-protocols-oidc
- **OIDC:** https://openid.net/specs/openid-connect-core-1_0.html

**Milestone:** v1.1.0 (Post-production)
**Priority:** Medium (nice-to-have, not required for v1.0)
