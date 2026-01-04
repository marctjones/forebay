---
id: 24
title: Write unit tests for Rust Worker authentication module
state: closed
created: '2026-01-04T08:57:08.793902Z'
labels:
- testing
- unit-test
- rust
- worker
- security
priority: high
---
Create comprehensive unit tests for authentication logic (auth.rs).

**Priority:** HIGH (security-critical)
**Type:** Unit Tests
**Component:** worker/src/auth.rs

**Scenarios to Test:**

**extract_token() function:**
- Extracts token from valid "Bearer {token}" header
- Returns None for missing Authorization header
- Returns None for malformed Authorization header (no "Bearer " prefix)
- Returns None for empty token after "Bearer "

**Session validation:**
- Valid session returns SessionData
- Expired session returns 401 error
- Non-existent session returns 401 error
- Missing token returns 401 error

**Google token verification:**
- Valid token with verified email succeeds
- Token with unverified email fails
- Token with invalid signature fails
- Token with wrong audience fails
- Expired token fails

**Email allowlist:**
- Allowed email passes check
- Disallowed email fails with 401
- Empty allowlist allows all emails
- Multiple emails in allowlist handled correctly

**Acceptance Criteria:**
- 80%+ code coverage of auth.rs
- All security-critical paths tested
- Mock/stub external dependencies (Google JWKS fetch)
- Test both success and failure cases

**Test Implementation Notes:**
- Use mock KV store for testing
- Mock HTTP client for JWKS fetching
- Use test JWT tokens with known keys
