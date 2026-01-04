---
id: 25
title: Write integration tests for Rust Worker authentication endpoints
state: open
created: '2026-01-04T08:57:09.341644Z'
labels:
- testing
- integration-test
- rust
- worker
- security
priority: high
---
Create integration tests for authentication API endpoints.

**Priority:** HIGH (security-critical)
**Type:** Integration Tests
**Component:** worker/src/lib.rs + auth.rs

**Scenarios to Test:**

**POST /auth/login:**
- Valid Google ID token creates session
- Returns session_token, email, expires_at
- Stores session in SESSION_TOKENS KV
- Invalid ID token returns 401
- Missing id_token field returns 400
- Unauthorized email returns 401

**GET /auth/whoami:**
- Valid session token returns user info
- Missing Authorization header returns 401
- Invalid session token returns 401
- Expired session returns 401
- Returns correct email and expiry time

**POST /auth/logout:**
- Valid session token deletes session from KV
- Returns success: true
- Session no longer valid after logout
- Missing token returns 401

**Cross-endpoint flows:**
- Login → whoami → logout → whoami fails
- Multiple logins create separate sessions
- Session TTL is 30 days from creation

**Acceptance Criteria:**
- All endpoints tested with real Router
- KV operations verified (mock or test KV)
- HTTP status codes match API spec
- Response JSON matches API spec
- Session lifecycle fully tested

**Test Setup:**
- Use wrangler dev or miniflare for local testing
- Mock Google OAuth for test tokens
- Clean KV state between tests
