---
id: 19
title: Implement C# client authentication
state: open
created: '2026-01-04T08:32:49.921581Z'
labels:
- phase-1
- csharp
- client
- auth
priority: high
---
Implement OAuth login flow in the C# CLI.

**Implementation tasks:**

1. **forebay login command**
   - Start local HTTP server on localhost:8080
   - Generate PKCE challenge (code_verifier + code_challenge)
   - Open browser to Google OAuth consent URL
   - Wait for callback with authorization code
   - Exchange code for ID token
   - Send ID token to Worker /auth/login
   - Save session token to ~/.config/forebay/config.toml
   - Set file permissions (Unix: 600)

2. **Config file management**
   - Read/write TOML config
   - Store session token + expiry
   - Store Worker URL

3. **forebay whoami command**
   - Call Worker /auth/whoami
   - Display email + expiry

4. **forebay logout command**
   - Call Worker /auth/logout
   - Delete local config

**Dependencies:**
- Add OAuth library (e.g., IdentityModel.OidcClient)
- Add TOML library (e.g., Tomlyn)
- Add library to open browser

**Reference:** Authentication Implementation Guide in wiki
