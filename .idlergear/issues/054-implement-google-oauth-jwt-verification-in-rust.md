---
id: 54
title: Implement Google OAuth JWT verification in Rust Worker
state: open
created: '2026-01-04T23:54:56.463402Z'
labels:
- auth
- rust
- worker
- security
priority: high
---
Implement JWT signature verification for Google ID tokens using Google's JWKS endpoint.

**Dependencies:** #6 (Google Cloud OAuth client setup)

**Implementation:**
1. Fetch Google's public keys from https://www.googleapis.com/oauth2/v3/certs
2. Cache JWKS keys to minimize external requests
3. Verify JWT signature using RS256 algorithm
4. Validate token claims:
   - `iss`: Must be `accounts.google.com` or `https://accounts.google.com`
   - `aud`: Must match `GOOGLE_CLIENT_ID` environment variable
   - `exp`: Token not expired
   - `email_verified`: Must be true
5. Extract email from token claims

**Crates to use:**
- `jsonwebtoken` for JWT verification
- `reqwest` or `worker::Fetch` for JWKS retrieval

**Testing:**
- Unit tests with mock JWKS responses
- Test valid/invalid/expired tokens
- Test signature verification

**Files to modify:**
- `worker/src/auth.rs`

**Acceptance Criteria:**
- [ ] JWKS keys fetched and cached
- [ ] JWT signature verified correctly
- [ ] All token claims validated
- [ ] Email extracted from valid tokens
- [ ] Invalid tokens rejected with 401
- [ ] Unit tests passing
