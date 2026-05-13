---
id: 71
title: Update test suite to remove OAuth references
state: open
created: '2026-01-05T02:20:51.892019Z'
labels:
- tech-debt
- testing
priority: low
---
The C# test suite has obsolete tests referencing the old OAuth authentication system that was replaced with API keys.

**Affected Test Files:**
1. `AuthenticationModelsTests.cs` - References LoginRequest, LoginResponse, LogoutResponse
2. `ForebayAuthClientTests.cs` - References LoginAsync(), LogoutAsync(), WhoamiResponse.ExpiresAt
3. `ConfigManagerTests.cs` - References SessionToken, ExpiresAt, Email, IsSessionValid()

**Action Required:**
Either update or remove obsolete tests:
- Remove tests for LoginAsync/LogoutAsync (no longer exist)
- Update ConfigManagerTests for ApiKey instead of SessionToken
- Update WhoamiResponse tests to remove ExpiresAt field assertions
- Add new tests for API key authentication flow

**Files to modify:**
- client/Forebay.Tests/AuthenticationModelsTests.cs
- client/Forebay.Tests/ForebayAuthClientTests.cs
- client/Forebay.Tests/ConfigManagerTests.cs

**Acceptance Criteria:**
- All tests pass with `dotnet test`
- Test coverage maintained or improved
- API key authentication flow tested

**Priority:** Low (doesn't block production deployment, but needed for CI/CD)
