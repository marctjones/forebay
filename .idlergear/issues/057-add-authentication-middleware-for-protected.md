---
id: 57
title: Add authentication middleware for protected endpoints
state: open
created: '2026-01-04T23:56:11.074196Z'
labels:
- auth
- worker
- rust
- security
priority: high
---
Create middleware to protect all queue endpoints with session authentication.

**Dependencies:** #55 (KV session storage)

**Implementation:**
1. Extract Bearer token from Authorization header
2. Validate session exists in KV
3. Check session not expired
4. Attach user email to request context
5. Return 401 for invalid/missing/expired sessions

**Acceptance Criteria:**
- [ ] All protected endpoints check authentication
- [ ] Invalid token returns 401
- [ ] Expired session returns 401
- [ ] User email available in handlers
- [ ] Integration tests passing
