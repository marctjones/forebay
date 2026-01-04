---
id: 2
title: Implement Google OAuth login flow in CLI
state: open
created: '2026-01-04T06:18:39.969950Z'
labels:
- enhancement
- auth
priority: high
---
Implement OAuth 2.0 PKCE flow for Google authentication:

- [ ] Add oauth2 crate dependency
- [ ] Implement `forebay login` command
- [ ] Start local HTTP server on localhost:8080
- [ ] Generate PKCE challenge
- [ ] Open browser to Google consent page
- [ ] Handle callback with auth code
- [ ] Exchange code for ID token
- [ ] Send ID token to Worker /auth/login endpoint
- [ ] Save session token to config.toml
- [ ] Set proper file permissions (chmod 600)

Dependencies needed:
- oauth2
- tiny_http or axum (for local server)
- open (to open browser)

Reference: See "Authentication Implementation Guide" wiki page
