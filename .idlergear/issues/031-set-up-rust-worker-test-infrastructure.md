---
id: 31
title: Set up Rust Worker test infrastructure
state: open
created: '2026-01-04T08:57:12.848787Z'
labels:
- testing
- infrastructure
- rust
- worker
priority: high
---
Create test infrastructure and utilities for Rust Worker testing.

**Priority:** HIGH (blocking other test tasks)
**Type:** Test Infrastructure
**Component:** worker/tests

**Tasks:**

**1. Create test utilities:**
- Mock KV store implementation
- Mock Google OAuth/JWKS responses
- Test JWT token generator
- Test session factory
- Request/Response builders for testing

**2. Set up integration test environment:**
- Configure miniflare or wrangler dev for local testing
- Set up test KV namespaces
- Configure test environment variables
- Create test fixtures

**3. Add test dependencies:**
```toml
[dev-dependencies]
worker-test = "*"  # If available
mockall = "0.12"   # For mocking
tokio-test = "0.4" # For async testing
```

**4. Create test helpers:**
```rust
// tests/common/mod.rs
pub mod fixtures {
    pub fn create_test_session() -> SessionData { ... }
    pub fn create_test_jwt() -> String { ... }
    pub fn mock_kv_store() -> MockKvStore { ... }
}
```

**5. Document testing approach:**
- How to run tests
- How to mock external dependencies
- How to test async code
- How to test Worker routes

**Acceptance Criteria:**
- Test utilities are reusable across all test files
- Integration tests can run locally without real Cloudflare account
- Clear documentation for adding new tests
- CI/CD can run all tests

**Reference:**
- worker-rs testing examples
- Cloudflare Workers testing docs
