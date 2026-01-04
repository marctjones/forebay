---
id: 42
title: Write end-to-end CLI integration tests
state: open
created: '2026-01-04T09:24:17.191224Z'
labels:
- testing
- e2e-test
- cli
priority: high
---
Create end-to-end tests that exercise the CLI against a real deployed Worker.

**Test Approach:**

Use C# xUnit to orchestrate actual CLI binary execution:
- Execute `forebay` CLI commands as separate processes
- Capture stdout/stderr for assertions
- Test against real deployed Worker
- Verify complete user workflows

**Test Scenarios:**

### 1. Complete Authentication Flow
```csharp
[Fact]
public async Task LoginFlow_CompletesSuccessfully()
{
    // Note: Requires manual OAuth interaction for now
    // Future: Mock OAuth server or use test credentials
    
    // 1. Run forebay login (manual step)
    // 2. Verify config file created
    // 3. Run forebay whoami
    // 4. Assert user email shown
    // 5. Run forebay logout
    // 6. Verify config cleared
}
```

### 2. Queue Operations End-to-End
```csharp
[Fact]
public async Task QueueWorkflow_PushAndPull()
{
    var queueName = $"test-{Guid.NewGuid()}";
    
    // Push item
    var pushResult = ExecuteCli($"push {queueName} \"test data\"");
    Assert.Equal(0, pushResult.ExitCode);
    
    // Stats check
    var statsResult = ExecuteCli($"stats {queueName}");
    Assert.Contains("Length: 1", statsResult.StdOut);
    
    // Pull item
    var pullResult = ExecuteCli($"pull {queueName}");
    Assert.Equal("test data", pullResult.StdOut.Trim());
    
    // Verify empty
    var emptyPull = ExecuteCli($"pull {queueName}");
    Assert.Equal(1, emptyPull.ExitCode);
    
    // Cleanup
    ExecuteCli($"delete {queueName} --force");
}
```

### 3. Piping and Stdin/Stdout
```csharp
[Fact]
public async Task Piping_WorksCorrectly()
{
    var queueName = $"test-{Guid.NewGuid()}";
    var testData = "{\"message\":\"hello\"}";
    
    // Echo into push
    var pushResult = ExecuteCli($"push {queueName}", stdin: testData);
    Assert.Equal(0, pushResult.ExitCode);
    
    // Pull and pipe to jq (if available)
    var pullResult = ExecuteCli($"pull {queueName}");
    var json = JsonDocument.Parse(pullResult.StdOut);
    Assert.Equal("hello", json.RootElement.GetProperty("message").GetString());
}
```

### 4. Error Handling
```csharp
[Fact]
public async Task NoAuth_ShowsHelpfulError()
{
    // Clear config
    DeleteConfig();
    
    // Try to push without auth
    var result = ExecuteCli("push test-queue \"data\"");
    Assert.Equal(1, result.ExitCode);
    Assert.Contains("not logged in", result.StdErr.ToLower());
    Assert.Contains("forebay login", result.StdErr);
}
```

**Test Infrastructure:**

```csharp
public class CliTestHelper
{
    public ProcessResult ExecuteCli(string args, string stdin = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = GetCliBinaryPath(),
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = stdin != null,
            UseShellExecute = false
        };
        
        var process = Process.Start(psi);
        
        if (stdin != null)
        {
            process.StandardInput.Write(stdin);
            process.StandardInput.Close();
        }
        
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        
        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StdOut = stdout,
            StdErr = stderr
        };
    }
}
```

**Acceptance Criteria:**
- [ ] All major CLI workflows tested
- [ ] Tests run actual CLI binary (not in-process)
- [ ] Tests verify exit codes
- [ ] Tests verify stdout/stderr output
- [ ] Tests clean up after themselves
- [ ] Can run in CI/CD
- [ ] Manual OAuth step documented

**Challenges:**
- OAuth requires interactive browser flow
  - Solution: Document manual test run process
  - Future: Mock OAuth or use test tokens
- Tests need real Worker deployment
  - Solution: Use staging environment
