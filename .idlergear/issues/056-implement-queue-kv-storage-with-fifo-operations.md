---
id: 56
title: Implement queue KV storage with FIFO operations
state: open
created: '2026-01-04T23:56:10.900229Z'
labels:
- queue
- worker
- rust
- cloudflare
priority: high
---
Implement FIFO queue storage using Cloudflare KV.

**Implementation:**
1. Store queue data in KV with key pattern: `queue:{name}`
2. Implement push operation (append to array)
3. Implement pull operation (remove from front)
4. Track metadata (total_pushed, total_pulled)
5. Handle empty queue gracefully

**Acceptance Criteria:**
- [ ] FIFO ordering preserved
- [ ] Push adds to end of array
- [ ] Pull removes from front
- [ ] Metadata counters updated
- [ ] Empty queue returns 404
- [ ] Unit tests passing
