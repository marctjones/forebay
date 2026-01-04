---
id: 27
title: Write integration tests for Rust Worker queue endpoints
state: open
created: '2026-01-04T08:57:10.508924Z'
labels:
- testing
- integration-test
- rust
- worker
priority: high
---
Create integration tests for queue API endpoints.

**Priority:** HIGH (core functionality)
**Type:** Integration Tests
**Component:** worker/src/lib.rs + queue.rs

**Scenarios to Test:**

**POST /q/:name/push:**
- Authenticated user can push items
- Returns item_id, queue length, timestamp
- Creates queue on first push
- Unauthenticated request returns 401
- Invalid payload returns 400
- Large payload (>25MB) returns 413

**GET /q/:name/pull:**
- Authenticated user can pull items
- Returns oldest item first (FIFO)
- Decrements queue length
- Empty queue returns 404
- Unauthenticated request returns 401
- User can only pull from own queues

**GET /q/:name/stats:**
- Returns queue statistics
- Does not modify queue
- Shows correct length and timestamps
- Unauthenticated request returns 401

**DELETE /q/:name:**
- Deletes queue and all items
- Returns deleted item count
- Queue no longer accessible after deletion
- Unauthenticated request returns 401

**GET /queues:**
- Lists all user's queues
- Shows length and oldest timestamp per queue
- Empty list for new user
- Unauthenticated request returns 401

**Cross-endpoint flows:**
- Push 3 items → pull 3 items → verify FIFO order
- Push → stats → verify count → pull → stats → verify count decreased
- Multiple users can't access each other's queues
- Queue name with slashes works (e.g., "work/tasks")

**Acceptance Criteria:**
- All endpoints tested with authentication
- Queue isolation verified
- FIFO ordering verified
- HTTP status codes match API spec
- Response JSON matches API spec
- Concurrent access handled correctly
