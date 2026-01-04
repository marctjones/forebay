---
id: 29
title: Write unit tests for C# CLI commands
state: open
created: '2026-01-04T08:57:11.724360Z'
labels:
- testing
- unit-test
- csharp
- client
- tdd
priority: medium
---
Create comprehensive unit tests for CLI command implementations.

**Priority:** MEDIUM
**Type:** Unit Tests
**Component:** client/Forebay.Cli/Commands

**Note:** Commands not yet implemented - create tests as part of TDD.

**Scenarios to Test:**

**LoginCommand:**
- Starts local HTTP server on port 8080
- Generates PKCE challenge correctly
- Opens browser to correct OAuth URL
- Handles OAuth callback
- Exchanges code for ID token
- Calls worker /auth/login endpoint
- Saves session token to config
- Sets correct file permissions
- Handles errors gracefully (port in use, browser not found, etc.)

**PushCommand:**
- Reads message from argument
- Reads message from stdin if no argument
- Calls worker /q/:name/push endpoint
- Displays success message with queue length
- Handles authentication errors
- Handles network errors

**PullCommand:**
- Calls worker /q/:name/pull endpoint
- Outputs message to stdout
- Exit code 0 on success, 1 on empty queue
- Handles authentication errors

**WhoamiCommand:**
- Calls worker /auth/whoami endpoint
- Displays email and expiry
- Handles missing config (not logged in)

**LogoutCommand:**
- Calls worker /auth/logout endpoint
- Deletes local config file
- Confirms logout to user

**ListCommand:**
- Calls worker /queues endpoint
- Displays queue names and lengths
- Handles empty queue list

**Acceptance Criteria:**
- All commands tested in isolation
- Mock IQueueClient for testing
- stdin/stdout handling tested
- Exit codes verified
- Error messages user-friendly
- Help text tested

**Test Framework:**
- Use xUnit
- Mock IQueueClient
- Capture console output for verification
