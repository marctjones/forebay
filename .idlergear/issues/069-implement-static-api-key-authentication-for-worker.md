---
id: 69
title: Implement static API key authentication for Worker
state: open
created: '2026-01-05T01:33:50.610049Z'
labels:
- enhancement
- auth
- worker
priority: high
---
Replace Google OAuth with simple static API key authentication for faster development.

Implementation:
- [ ] Add API_KEYS environment variable to wrangler.toml
- [ ] Implement extract_bearer_token() function
- [ ] Implement validate_api_key() function that maps keys to user emails
- [ ] Add authentication middleware to protect endpoints
- [ ] Update auth.rs to use static keys instead of JWT
- [ ] Add basic auth endpoints: /auth/whoami
- [ ] Update CLI to use api_key instead of session_token in config
- [ ] Write tests for API key validation

Environment variable format:
```
API_KEYS="dev-alice:alice@example.com,dev-bob:bob@example.com"
```

This unblocks development - OAuth can be added later as enhancement.
