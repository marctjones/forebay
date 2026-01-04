---
id: 5
title: Set up Google Cloud OAuth client
state: open
created: '2026-01-04T06:18:41.092929Z'
labels:
- setup
- auth
priority: high
---
Create OAuth 2.0 client in Google Cloud Console:

Steps:
1. Create project in Google Cloud Console
2. Enable Google+ API (or Google Identity)
3. Go to Credentials → Create OAuth 2.0 Client ID
4. Application type: Desktop app
5. Authorized redirect URIs: http://localhost:8080/callback
6. Copy Client ID
7. Hardcode Client ID in CLI (it's public, not secret)
8. Document Client ID in README setup section

Note: No client secret needed for PKCE flow

Reference: See "Authentication Implementation Guide" wiki page
