---
id: 18
title: Implement Rust Worker queue endpoints
state: open
created: '2026-01-04T08:32:49.596464Z'
labels:
- phase-1
- rust
- worker
- queue
priority: high
---
Implement queue operations in the Rust Worker.

**Implementation tasks:**

1. **POST /q/:name/push**
   - Receive item payload (JSON)
   - Append to queue in QUEUE_DATA KV
   - Key format: `queue:{email}:{name}`
   - Value: JSON array of items with timestamps
   - Return success + queue length

2. **GET /q/:name/pull**
   - Atomic read + delete first item
   - Use KV get + put in transaction
   - Return item + timestamp + remaining count
   - Return 404 if queue empty

3. **GET /q/:name/stats**
   - Read queue metadata
   - Return length, oldest item timestamp
   - Don't return actual items

4. **GET /queues**
   - List all queues for authenticated user
   - Scan KV with prefix `queue:{email}:`
   - Return queue names + lengths

5. **DELETE /q/:name**
   - Delete queue from KV
   - Return success

**Data model:**
```rust
struct QueueItem {
    payload: serde_json::Value,
    timestamp: i64,
    id: String,
}
```

**Tests:**
- Push/pull operations
- Empty queue handling
- Queue isolation (users can't access each other's queues)
