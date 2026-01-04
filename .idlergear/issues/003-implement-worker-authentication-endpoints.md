---
id: 3
title: Implement Worker authentication endpoints
state: open
created: '2026-01-04T06:18:40.309117Z'
labels:
- enhancement
- auth
priority: high
---
Add authentication to Cloudflare Worker:

- [ ] Add google-auth-library to Worker
- [ ] Create POST /auth/login endpoint
- [ ] Validate Google ID token signature
- [ ] Check email against ALLOWED_EMAILS env var
- [ ] Generate session token (crypto.randomUUID())
- [ ] Store session in KV with 30-day TTL
- [ ] Implement auth middleware for all endpoints
- [ ] Support static tokens for CI/CD (STATIC_AUTH_TOKENS env)
- [ ] Return 401 for missing/invalid tokens
- [ ] Add email to operation logs for audit

Environment variables:
- ALLOWED_EMAILS
- GOOGLE_CLIENT_ID
- SESSION_TTL_DAYS (optional, default 30)
- STATIC_AUTH_TOKENS (optional, comma-separated)

KV namespace:
- SESSION_TOKENS

Reference: See "Authentication Implementation Guide" wiki page
