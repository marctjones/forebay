---
id: 59
title: Implement OAuth flow with local HTTP callback server
state: open
created: '2026-01-04T23:56:11.701280Z'
labels:
- auth
- cli
- csharp
- oauth2
priority: high
---
Implement Google OAuth flow with PKCE and local callback server.

**Dependencies:** #6 (Google Cloud OAuth setup)

**Implementation:**
1. Generate PKCE code challenge
2. Open browser to Google consent URL
3. Start local HTTP server on localhost:8080
4. Handle OAuth callback with authorization code
5. Exchange code for ID token
6. Send ID token to Worker `/auth/login`
7. Save session token to config file

**Acceptance Criteria:**
- [ ] PKCE flow implemented
- [ ] Browser opens automatically
- [ ] Local server handles callback
- [ ] ID token exchanged
- [ ] Session token saved
- [ ] Works on Linux and Windows
