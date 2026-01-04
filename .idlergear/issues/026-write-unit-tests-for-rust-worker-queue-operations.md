---
id: 26
title: Write unit tests for Rust Worker queue operations
state: closed
created: '2026-01-04T08:57:09.900763Z'
labels:
- testing
- unit-test
- rust
- worker
- tdd
priority: high
---
Create comprehensive unit tests for queue module (queue.rs).

**Priority:** HIGH (core functionality)
**Type:** Unit Tests
**Component:** worker/src/queue.rs

**Note:** queue.rs is not yet implemented - create tests as part of TDD approach.

**Scenarios to Test:**

**Queue push:**
- Push item creates QueueItem with ID and timestamp
- Item added to queue array
- Queue metadata updated (total_pushed incremented)
- Returns correct queue length
- Handles JSON payload of any type
- Respects 25MB KV size limit

**Queue pull:**
- Pull removes first item (FIFO)
- Returns item with payload and timestamp
- Updates metadata (total_pulled incremented)
- Returns 404 for empty queue
- Returns correct remaining count
- Atomic operation (no race conditions)

**Queue stats:**
- Returns correct queue length
- Returns oldest item timestamp
- Returns newest item timestamp
- Returns total size estimation
- Works for empty queue

**Queue deletion:**
- Deletes all items
- Returns deleted item count
- Queue no longer exists after deletion

**Queue listing:**
- Lists all queues for user
- Only shows user's own queues
- Returns correct metadata per queue

**Acceptance Criteria:**
- 80%+ code coverage
- All edge cases tested (empty queue, single item, many items)
- FIFO ordering verified
- Queue isolation between users verified
