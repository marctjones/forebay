---
id: 7
title: Implement TypeScript test Worker
state: closed
created: '2026-01-04T06:21:25.467077Z'
labels:
- phase-0
- worker
- typescript
priority: high
---
Create baseline TypeScript Worker implementation.

**Requirements:**
- Use Hono framework (fast, Worker-optimized)
- Implement all test endpoints (health, kv-write, kv-read, queue-push, queue-pull)
- Configure wrangler.toml with KV binding
- Keep it simple - this is just for testing
- Add basic error handling
- Return timing information in responses

**Endpoints:**
```typescript
GET  /health              → { status: "ok", worker: "typescript" }
POST /kv-write            → body: { key, value } → store in KV
GET  /kv-read/:key        → read from KV
POST /queue-push/:name    → body: { item } → append to queue in KV
GET  /queue-pull/:name    → atomic read + delete first item
```

**Note:** Queue is simulated using KV with array serialization (good enough for testing)
