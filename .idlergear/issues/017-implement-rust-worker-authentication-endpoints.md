---
id: 17
title: Implement Rust Worker authentication endpoints
state: open
created: '2026-01-04T08:32:49.282316Z'
labels:
- phase-1
- rust
- worker
- auth
priority: high
---
Implement authentication in the Rust Worker.

**Implementation tasks:**

1. **POST /auth/login**
   - Receive Google ID token from client
   - Verify token signature using Google's public keys
   - Extract email from token claims
   - Check email against ALLOWED_EMAILS env var
   - Generate session token (UUID v4)
   - Store session in SESSION_TOKENS KV (30-day TTL)
   - Return session token + expiry

2. **GET /auth/whoami**
   - Read session token from Authorization header
   - Look up session in KV
   - Return user email + expiry

3. **POST /auth/logout**
   - Delete session from KV
   - Return success

4. **Middleware: require_auth()**
   - Check Authorization header
   - Validate session token exists in KV
   - Attach email to request context
   - Return 401 if invalid

**Dependencies:**
- Add `jsonwebtoken` crate for JWT validation
- Add `reqwest` for fetching Google public keys

**Tests:**
- Unit tests for token validation
- Integration tests with mock KV
