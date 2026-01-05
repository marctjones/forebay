---
id: 55
title: Implement KV session storage with 30-day TTL
state: open
created: '2026-01-04T23:56:10.721487Z'
labels:
- auth
- worker
- rust
- cloudflare
priority: high
---
Implement session storage in Cloudflare KV with automatic expiration.

**Dependencies:** #54 (JWT verification)

**Implementation:**
1. Store session data in KV with key pattern: `session:{uuid}`
2. Set TTL to 30 days (2592000 seconds)
3. Implement session creation after successful OAuth
4. Implement session lookup for protected endpoints
5. Implement session deletion for logout

**Acceptance Criteria:**
- [ ] Sessions stored with 30-day TTL
- [ ] Session lookup by token
- [ ] Session deletion on logout
- [ ] Automatic expiration via KV TTL
- [ ] Unit tests passing
