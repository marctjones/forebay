---
id: 28
title: Write unit tests for C# Core library
state: closed
created: '2026-01-04T08:57:11.078386Z'
labels:
- testing
- unit-test
- csharp
- client
- tdd
priority: high
---
Create comprehensive unit tests for Forebay.Core library.

**Priority:** HIGH
**Type:** Unit Tests  
**Component:** client/Forebay.Core

**Note:** Core library not yet implemented - create tests as part of TDD.

**Scenarios to Test:**

**IQueueClient interface:**
- Interface contract is clear
- All methods documented

**CloudflareClient HTTP client:**
- Base URL configuration
- Authorization header added to requests
- GET requests work correctly
- POST requests with JSON body work
- Error responses parsed correctly
- Network errors handled gracefully
- Timeout handling
- Retry logic (if implemented)

**Model serialization:**
- AuthResponse deserializes correctly
- QueueItem deserializes correctly
- PushRequest serializes correctly
- Error responses parsed correctly
- Null handling for optional fields

**Configuration management:**
- Config file read/write
- Session token storage
- Worker URL configuration
- File permissions set correctly (Unix)

**Acceptance Criteria:**
- 80%+ code coverage of Core library
- All HTTP operations mocked/stubbed
- Success and error paths tested
- JSON serialization/deserialization verified
- Configuration handling tested

**Test Framework:**
- Use xUnit
- Use Moq for mocking HttpClient
- Use FluentAssertions for readable assertions
