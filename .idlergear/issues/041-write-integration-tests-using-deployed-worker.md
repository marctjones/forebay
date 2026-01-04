---
id: 41
title: Write integration tests using deployed Worker
state: open
created: '2026-01-04T09:24:16.754235Z'
labels:
- testing
- integration-test
- worker
priority: high
---
Create comprehensive integration tests that test the deployed Worker endpoints.

**Test Environment Setup:**

1. **Test Configuration:**
   - Use deployed Worker URL (production or staging)
   - Test user accounts with Google OAuth
   - Dedicated test KV namespaces (optional)
   - Clean state before/after test runs

2. **Test Framework:**
   - Use C# xUnit for test orchestration
   - RestSharp or HttpClient for API calls
   - Test against real deployed Worker
   - Automated cleanup after tests

**Integration Test Scenarios:**

### Authentication Tests
- [ ] POST /auth/login with valid Google ID token succeeds
- [ ] POST /auth/login with invalid token returns 401
- [ ] POST /auth/login with unauthorized email returns 401
- [ ] GET /auth/whoami with valid session returns user info
- [ ] GET /auth/whoami with invalid session returns 401
- [ ] POST /auth/logout invalidates session
- [ ] Session expires after TTL period

### Queue Operation Tests
- [ ] POST /queues/:queue/push creates queue and adds item
- [ ] POST /queues/:queue/push requires authentication
- [ ] POST /queues/:queue/pull returns items in FIFO order
- [ ] POST /queues/:queue/pull on empty queue returns 404
- [ ] GET /queues/:queue/stats returns correct queue info
- [ ] DELETE /queues/:queue removes all items
- [ ] GET /queues lists all user's queues

### Cross-Feature Tests
- [ ] User A cannot access User B's queues
- [ ] Multiple concurrent pushes maintain consistency
- [ ] Large payload (near KV limit) works correctly
- [ ] Queue names with special characters work
- [ ] Session works across multiple queue operations

**Test Data Management:**
- Generate unique queue names per test run
- Clean up queues after tests
- Use test email accounts
- Handle flaky network conditions

**Acceptance Criteria:**
- [ ] All critical API paths tested
- [ ] Tests run against deployed Worker
- [ ] Tests are reliable (not flaky)
- [ ] Tests clean up after themselves
- [ ] Can run in CI/CD pipeline
- [ ] Clear test output and error messages

**Implementation:**
- Create `Forebay.IntegrationTests` project
- Add test helpers for auth flow
- Add fixtures for test data cleanup
- Document how to run tests locally
