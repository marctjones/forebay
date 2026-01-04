---
id: 21
title: Write integration tests for Worker + Client
state: open
created: '2026-01-04T08:32:50.713611Z'
labels:
- phase-1
- testing
- integration
priority: medium
---
Create end-to-end integration tests for the full system.

**Test scenarios:**

1. **Authentication flow**
   - Login with valid Google account
   - Verify session token works
   - Logout and verify token invalidated

2. **Queue operations**
   - Push item to queue
   - Pull item from queue
   - Verify FIFO ordering
   - Verify queue isolation between users

3. **Error handling**
   - Unauthorized access (missing/invalid token)
   - Queue not found
   - Empty queue pull
   - Invalid request payloads

4. **Performance**
   - Push/pull 100 items
   - Measure latency
   - Verify no data loss

**Test setup:**
- Deploy Worker to test environment
- Use real KV namespaces (test-prefixed)
- C# integration tests use real HTTP client
- Cleanup test data after runs

**CI/CD:**
- GitHub Actions workflow
- Run integration tests on PR
- Deploy to staging on merge to main
