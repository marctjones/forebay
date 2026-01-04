---
id: 35
title: Create KV namespaces in Cloudflare dashboard
state: open
created: '2026-01-04T09:22:39.356743Z'
labels:
- deployment
- configuration
- cloudflare
priority: high
---
Set up Cloudflare KV namespaces for session storage and queue data.

**KV Namespaces to Create:**

1. **SESSION_TOKENS** (Production)
   - Purpose: Store user session tokens with TTL
   - Typical size: <1MB per user
   - Access pattern: High read, low write
   - TTL: 30 days automatic expiration

2. **SESSION_TOKENS_PREVIEW** (Development)
   - Purpose: Testing environment for development
   - Same structure as production

3. **QUEUES** (Production)
   - Purpose: Store queue data for all users
   - Key format: `queue:{queue_name}`
   - Typical size: Variable based on queue usage
   - Access pattern: High read/write

4. **QUEUES_PREVIEW** (Development)
   - Purpose: Testing environment for development
   - Same structure as production

**Tasks:**
- [ ] Log into Cloudflare dashboard
- [ ] Navigate to Workers & Pages → KV
- [ ] Create SESSION_TOKENS namespace (production)
- [ ] Create SESSION_TOKENS_PREVIEW namespace (preview)
- [ ] Create QUEUES namespace (production)
- [ ] Create QUEUES_PREVIEW namespace (preview)
- [ ] Copy namespace IDs to wrangler.toml
- [ ] Document namespace IDs in deployment docs

**Acceptance Criteria:**
- All 4 namespaces created
- IDs recorded in wrangler.toml
- Preview namespaces bound to preview environment
- Production namespaces bound to production environment

**Reference:**
- https://developers.cloudflare.com/kv/get-started/
