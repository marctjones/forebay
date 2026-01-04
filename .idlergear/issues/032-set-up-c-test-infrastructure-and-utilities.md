---
id: 32
title: Set up C# test infrastructure and utilities
state: open
created: '2026-01-04T08:57:13.432803Z'
labels:
- testing
- infrastructure
- csharp
- client
priority: high
---
Create test infrastructure for C# client testing.

**Priority:** HIGH (blocking other test tasks)
**Type:** Test Infrastructure
**Component:** client/Forebay.Tests

**Tasks:**

**1. Add test dependencies:**
```xml
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
<PackageReference Include="Moq" Version="4.20.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
```

**2. Create test utilities:**
```csharp
// TestHelpers/MockHttpClient.cs
public class MockHttpClient {
    public static HttpClient CreateWithResponses(...)
}

// TestHelpers/TestData.cs
public static class TestData {
    public static AuthResponse ValidAuthResponse => ...
    public static QueueItem SampleQueueItem => ...
}

// TestHelpers/ConfigHelper.cs
public static class ConfigHelper {
    public static string CreateTempConfig(...)
    public static void CleanupConfig(...)
}
```

**3. Create base test classes:**
```csharp
public abstract class ClientTestBase : IDisposable {
    protected Mock<IHttpClientFactory> HttpFactory;
    protected IQueueClient Client;
    // Common setup/teardown
}
```

**4. Set up test configuration:**
- appsettings.Test.json for test settings
- Test config file location
- Mock OAuth credentials

**5. Document testing approach:**
- How to run tests (dotnet test)
- How to mock HTTP calls
- How to test file I/O
- How to test console output

**Acceptance Criteria:**
- All test dependencies installed
- Helpers make tests easy to write
- Tests don't require real network calls
- Tests don't modify user's actual config
- CI/CD can run tests

**Test Organization:**
```
Forebay.Tests/
├── Core/
│   ├── CloudflareClientTests.cs
│   ├── ConfigurationTests.cs
│   └── ModelTests.cs
├── Cli/
│   ├── LoginCommandTests.cs
│   ├── PushCommandTests.cs
│   └── PullCommandTests.cs
└── TestHelpers/
    ├── MockHttpClient.cs
    ├── TestData.cs
    └── ConfigHelper.cs
```
