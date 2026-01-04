---
id: 30
title: Write end-to-end integration tests
state: open
created: '2026-01-04T08:57:12.272185Z'
labels:
- testing
- e2e-test
- integration-test
priority: high
---
Create end-to-end integration tests for complete system.

**Priority:** HIGH
**Type:** Integration/E2E Tests
**Component:** Full system (Worker + Client)

**Scenarios to Test:**

**Complete authentication flow:**
1. User runs `forebay login`
2. OAuth flow completes
3. Session token saved
4. `forebay whoami` shows user info
5. `forebay logout` ends session
6. `forebay whoami` fails after logout

**Complete queue flow:**
1. User logs in
2. `echo "test" | forebay push work` succeeds
3. `forebay pull work` returns "test"
4. `forebay pull work` returns 404 (empty queue)
5. Push 3 messages, pull 3 times, verify FIFO order
6. `forebay list` shows queue with correct count
7. `forebay stats work` shows correct stats

**Multi-user isolation:**
1. User A logs in and pushes to queue "private"
2. User B logs in and tries to pull from "private"
3. User B gets 404 or empty result
4. User B can push/pull from own queues

**Error handling:**
1. Commands fail gracefully without authentication
2. Network errors show helpful messages
3. Invalid queue names rejected
4. Large payloads handled correctly

**Performance:**
1. Push 100 items in sequence
2. Pull 100 items in sequence
3. Measure total time
4. Verify no data loss

**Acceptance Criteria:**
- All critical user journeys tested
- Tests run against real deployed Worker
- Tests clean up after themselves
- Can run in CI/CD pipeline
- Tests are idempotent (can run multiple times)

**Test Setup:**
- Deploy Worker to test environment
- Use test Google OAuth client
- Clean KV namespaces before/after tests
- Use separate test user accounts

**Test Framework:**
- C# xUnit for orchestration
- Actual CLI binary execution
- Real HTTP calls to Worker
- Test data cleanup automated
