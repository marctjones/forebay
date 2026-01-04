---
id: 23
title: Write unit tests for Rust Worker error module
state: closed
created: '2026-01-04T08:57:08.254158Z'
labels:
- testing
- unit-test
- rust
- worker
priority: medium
---
Create comprehensive unit tests for the error handling module (error.rs).

**Priority:** MEDIUM
**Type:** Unit Tests
**Component:** worker/src/error.rs

**Scenarios to Test:**
- ErrorResponse::unauthorized() creates correct error structure
- ErrorResponse::bad_request() creates correct error structure
- ErrorResponse::not_found() creates correct error structure
- ErrorResponse::internal_error() creates correct error structure
- to_response() returns Response with correct status code
- to_response() returns valid JSON with error format
- Error codes match API specification ("UNAUTHORIZED", "BAD_REQUEST", etc.)

**Acceptance Criteria:**
- All error constructors tested
- Response serialization verified
- Status codes verified (401, 400, 404, 500)
- JSON structure matches API spec format

**Example Test:**
```rust
#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_unauthorized_error() {
        let err = ErrorResponse::unauthorized("test message");
        assert_eq!(err.error.code, "UNAUTHORIZED");
        assert_eq!(err.error.message, "test message");
    }

    #[test]
    fn test_to_response_status() {
        let err = ErrorResponse::unauthorized("test");
        let response = err.to_response(401).unwrap();
        assert_eq!(response.status_code(), 401);
    }
}
```
