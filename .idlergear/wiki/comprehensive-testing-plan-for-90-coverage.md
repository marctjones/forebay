---
id: 1
title: Comprehensive Testing Plan for 90% Coverage
created: '2026-01-05T01:07:45.715285Z'
updated: '2026-01-05T01:07:45.715310Z'
---
# Forebay Comprehensive Testing Plan

## Executive Summary

This testing plan outlines the strategy to achieve **90% code coverage** across the Forebay project, combining unit tests and integration tests. The plan prioritizes critical security and data integrity paths while ensuring comprehensive coverage of all components.

**Current Status**: 62 tests passing (25 Rust + 37 C#)  
**Target**: 90% line coverage across all components  
**Timeline**: 3 implementation phases

---

## 1. Current Coverage Analysis

### 1.1 Rust Worker Coverage (~60% estimated)

**Covered** (`auth.rs`):
- ✅ Session data serialization/deserialization
- ✅ Session TTL calculation  
- ✅ Session expiry logic
- ✅ Session key formatting
- ✅ Google JWKS URL constant

**NOT Covered** (`auth.rs:34-241`):
- ❌ `handle_login()` HTTP handler - lines 35-93
- ❌ `handle_whoami()` HTTP handler - lines 95-108
- ❌ `handle_logout()` HTTP handler - lines 110-126
- ❌ `get_session()` middleware - lines 128-164 (CRITICAL - auth bypass risk)
- ❌ `extract_token()` helper - lines 166-174
- ❌ `verify_google_token()` - lines 176-219 (CRITICAL - security)
- ❌ `fetch_google_jwks()` - lines 221-241

**Covered** (`queue.rs`):
- ✅ Queue name validation logic
- ✅ FIFO ordering behavior
- ✅ Queue data initialization
- ✅ Metadata counter logic
- ✅ Queue item creation

**NOT Covered** (`queue.rs:14-260`):
- ❌ `handle_push()` - lines 14-92 (CRITICAL - data integrity)
- ❌ `handle_pull()` - lines 94-148 (CRITICAL - FIFO guarantee)
- ❌ `handle_stats()` - lines 150-203
- ❌ `handle_delete()` - lines 205-242
- ❌ `handle_list()` - lines 244-260

**Covered** (`error.rs`):
- ✅ All error constructors (100% coverage)
- ✅ Error serialization
- ⚠️ `to_response()` not testable in unit tests (requires WASM)

**Covered** (`models.rs`):
- ✅ All model serialization/deserialization (100% coverage)

**Covered** (`lib.rs`):
- ❌ Router setup and main handler (0% coverage)

### 1.2 C# Client Library Coverage (~85% estimated)

**Covered** (`Forebay.Core/ForebayClient.cs`):
- ✅ All public API methods with success paths
- ✅ Authentication header setting
- ✅ Error response parsing
- ✅ Session token management

**NOT Covered**:
- ❌ CancellationToken handling (lines 36-168)
- ❌ JsonException fallback in error parsing (line 193)
- ❌ HTTP error fallback (lines 198-200)
- ❌ `ClearSessionToken()` method (lines 28-32)
- ❌ Constructor with HttpClient parameter edge cases

**Covered** (`Forebay.Core/Configuration/`):
- ✅ Config save/load/delete (100%)
- ✅ Session validation (100%)
- ✅ Platform-specific paths (100%)
- ⚠️ chmod error handling (line 148) not tested

### 1.3 C# CLI Coverage (0% - not building)

**Files to Test**:
- `Program.cs` - Command routing and global options
- `Commands/AuthCommands.cs` - Login, logout, whoami
- `Commands/QueueCommands.cs` - Push, pull, stats, list, delete

**Critical Paths**:
- stdin redirection detection (`QueueCommands.cs:30`)
- JSON parsing with fallback (`QueueCommands.cs:51-59`)
- User confirmation prompt (`QueueCommands.cs:216-218`)
- Error exit codes

---

## 2. Unit Test Strategy

### 2.1 Rust Worker Unit Tests

#### Priority 1: Security-Critical (Target: 100%)

**Test: JWT Verification**
```rust
// Location: auth.rs tests
#[test]
fn test_verify_google_token_invalid_kid() {
    // Mock JWKS response without matching kid
    // Assert: Returns Err("No matching key found")
}

#[test]
fn test_verify_google_token_unverified_email() {
    // Mock token with email_verified: false
    // Assert: Returns Err("Email not verified")
}

#[test]
fn test_verify_google_token_expired() {
    // Mock token with exp in past
    // Assert: Token validation fails
}

#[test]
fn test_verify_google_token_wrong_audience() {
    // Mock token with different aud
    // Assert: Token validation fails
}
```

**Test: Session Middleware**
```rust
#[test]
fn test_extract_token_with_bearer() {
    let req = /* mock Request with "Bearer token123" */;
    assert_eq!(extract_token(&req), Some("token123".to_string()));
}

#[test]
fn test_extract_token_without_bearer() {
    let req = /* mock Request with "token123" (no Bearer) */;
    assert_eq!(extract_token(&req), None);
}

#[test]
fn test_extract_token_no_auth_header() {
    let req = /* mock Request without Authorization */;
    assert_eq!(extract_token(&req), None);
}

#[test]
fn test_session_expired_detection() {
    let session = SessionData {
        email: "test@example.com".to_string(),
        created_at: 1000,
        expires_at: 2000,
    };
    let now = 3000i64;
    assert!(now > session.expires_at, "Should detect expiry");
}
```

**Test: Email Allowlist**
```rust
#[test]
fn test_email_allowlist_enforcement() {
    let allowed = "user1@example.com,user2@example.com";
    let email_list: Vec<&str> = allowed.split(',').collect();
    
    assert!(email_list.contains(&"user1@example.com"));
    assert!(!email_list.contains(&"user3@example.com"));
}

#[test]
fn test_empty_allowlist_allows_all() {
    let allowed = String::new();
    let email_list: Vec<&str> = allowed.split(',').collect();
    
    // When allowlist is empty, any email should be allowed
    assert!(email_list.is_empty() || email_list.contains(&"any@example.com"));
}
```

#### Priority 2: Data Integrity (Target: 95%)

**Test: Queue FIFO with Edge Cases**
```rust
#[test]
fn test_queue_concurrent_push_order() {
    // Simulate multiple pushes
    // Verify items maintain insertion order
}

#[test]
fn test_queue_empty_pull_behavior() {
    let queue = QueueData { items: vec![], ... };
    // Verify cannot pull from empty queue
}

#[test]
fn test_queue_name_max_length_boundary() {
    let valid = "a".repeat(64);
    assert!(valid.len() <= MAX_QUEUE_NAME_LENGTH);
    
    let invalid = "a".repeat(65);
    assert!(invalid.len() > MAX_QUEUE_NAME_LENGTH);
}

#[test]
fn test_queue_metadata_persistence() {
    let mut data = QueueData { ... };
    data.metadata.total_pushed += 1;
    data.metadata.total_pulled += 1;
    
    let json = serde_json::to_string(&data).unwrap();
    let parsed: QueueData = serde_json::from_str(&json).unwrap();
    
    assert_eq!(parsed.metadata.total_pushed, 1);
    assert_eq!(parsed.metadata.total_pulled, 1);
}
```

**Test: Queue Size Calculation**
```rust
#[test]
fn test_stats_size_calculation_accuracy() {
    let queue = QueueData {
        items: vec![
            QueueItem { payload: json!({"data": "a".repeat(100)}), ... },
            QueueItem { payload: json!({"data": "b".repeat(50)}), ... },
        ],
        ...
    };
    
    let size = serde_json::to_string(&queue).unwrap().len();
    assert!(size > 150); // At least payload size
}
```

#### Priority 3: Error Handling (Target: 90%)

**Test: KV Deserialization Errors**
```rust
#[test]
fn test_queue_corrupted_data_recovery() {
    let corrupted_json = "{invalid json}";
    let result = serde_json::from_str::<QueueData>(corrupted_json);
    assert!(result.is_err());
    
    // Test fallback creates new queue
    let queue = result.unwrap_or_else(|_| QueueData {
        items: vec![],
        metadata: QueueMetadata { ... },
    });
    assert_eq!(queue.items.len(), 0);
}
```

### 2.2 C# Client Unit Tests

#### Add Missing Tests

**Test: CancellationToken Handling**
```csharp
[Fact]
public async Task LoginAsync_WhenCancelled_ThrowsTaskCanceledException()
{
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    
    var act = async () => await _client.LoginAsync("token", cts.Token);
    
    await act.Should().ThrowAsync<TaskCanceledException>();
}
```

**Test: ClearSessionToken**
```csharp
[Fact]
public async Task ClearSessionToken_RemovesAuthorizationHeader()
{
    _client.SetSessionToken("test-token");
    _client.ClearSessionToken();
    
    var act = async () => await _client.WhoAmIAsync();
    
    await act.Should().ThrowAsync<InvalidOperationException>();
}
```

**Test: Error Parsing Fallback**
```csharp
[Fact]
public async Task LoginAsync_WithMalformedErrorResponse_ThrowsGenericException()
{
    _mockHttpHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(...)
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("Not JSON")
        });
    
    var act = async () => await _client.LoginAsync("token");
    
    await act.Should().ThrowAsync<ForebayApiException>()
        .Where(ex => ex.Code == "HTTP_ERROR");
}
```

**Test: Config chmod Error Handling**
```csharp
[Fact]
public void Save_WhenChmodFails_StillSavesFile()
{
    // This test verifies that chmod failure doesn't prevent save
    // Mock Process.Start to throw
    // Verify file still exists
}
```

### 2.3 C# CLI Unit Tests

**Strategy**: Use `StringWriter` to capture stdout/stderr

**Test: Stdin Detection**
```csharp
[Fact]
public async Task PushCommand_WithStdinRedirected_ReadsFromStdin()
{
    var input = new StringReader("{\"message\":\"hello\"}");
    Console.SetIn(input);
    
    // Mock Console.IsInputRedirected = true
    // Execute push command
    // Verify message read from stdin
}
```

**Test: JSON Fallback**
```csharp
[Fact]
public async Task PushCommand_WithNonJsonInput_WrapsInMessageField()
{
    // Input: "plain text"
    // Expected payload: {"message": "plain text"}
}
```

**Test: User Confirmation**
```csharp
[Fact]
public async Task DeleteCommand_WithoutForce_PromptsConfirmation()
{
    var input = new StringReader("n\n");
    Console.SetIn(input);
    
    var output = new StringWriter();
    Console.SetOut(output);
    
    // Execute delete command
    
    output.ToString().Should().Contain("Delete queue");
    // Verify queue NOT deleted
}
```

**Test: Exit Codes**
```csharp
[Fact]
public async Task Command_WithError_SetsNonZeroExitCode()
{
    // Mock API error
    // Execute command
    // Verify context.ExitCode == 1
}
```

---

## 3. Integration Test Strategy

### 3.1 Rust Worker Integration Tests

**Setup**: Use `wrangler dev` with test KV namespaces

**Test Environment**:
```toml
# wrangler.test.toml
kv_namespaces = [
  { binding = "SESSION_TOKENS", id = "test-sessions", preview_id = "test-sessions-preview" },
  { binding = "QUEUES", id = "test-queues", preview_id = "test-queues-preview" }
]

[vars]
GOOGLE_CLIENT_ID = "test-client-id"
ALLOWED_EMAILS = "test@example.com"
```

**Integration Test Suite** (`worker/tests/integration_tests.rs`):

```rust
#[tokio::test]
async fn test_health_endpoint() {
    let response = reqwest::get("http://localhost:8787/health").await.unwrap();
    assert_eq!(response.status(), 200);
    
    let body: serde_json::Value = response.json().await.unwrap();
    assert_eq!(body["status"], "ok");
}

#[tokio::test]
async fn test_auth_flow_end_to_end() {
    // 1. Login with mock JWT
    let login_response = client
        .post("http://localhost:8787/auth/login")
        .json(&json!({"id_token": MOCK_VALID_JWT}))
        .send()
        .await
        .unwrap();
    
    assert_eq!(login_response.status(), 200);
    let session_token = login_response.json::<LoginResponse>()
        .await
        .unwrap()
        .session_token;
    
    // 2. Whoami
    let whoami_response = client
        .get("http://localhost:8787/auth/whoami")
        .header("Authorization", format!("Bearer {}", session_token))
        .send()
        .await
        .unwrap();
    
    assert_eq!(whoami_response.status(), 200);
    
    // 3. Logout
    let logout_response = client
        .post("http://localhost:8787/auth/logout")
        .header("Authorization", format!("Bearer {}", session_token))
        .send()
        .await
        .unwrap();
    
    assert_eq!(logout_response.status(), 200);
    
    // 4. Verify session invalidated
    let whoami_after_logout = client
        .get("http://localhost:8787/auth/whoami")
        .header("Authorization", format!("Bearer {}", session_token))
        .send()
        .await
        .unwrap();
    
    assert_eq!(whoami_after_logout.status(), 401);
}

#[tokio::test]
async fn test_queue_fifo_guarantee() {
    let session_token = setup_authenticated_session().await;
    
    // Push 3 items
    for i in 1..=3 {
        client
            .post("http://localhost:8787/queues/test-fifo/push")
            .header("Authorization", format!("Bearer {}", session_token))
            .json(&json!({"payload": {"order": i}}))
            .send()
            .await
            .unwrap();
    }
    
    // Pull 3 items and verify order
    for i in 1..=3 {
        let response = client
            .post("http://localhost:8787/queues/test-fifo/pull")
            .header("Authorization", format!("Bearer {}", session_token))
            .send()
            .await
            .unwrap();
        
        let payload = response.json::<PullResponse>().await.unwrap();
        assert_eq!(payload.payload["order"], i);
    }
}

#[tokio::test]
async fn test_auth_middleware_blocks_unauthenticated() {
    let response = client
        .post("http://localhost:8787/queues/test/push")
        .json(&json!({"payload": {}}))
        .send()
        .await
        .unwrap();
    
    assert_eq!(response.status(), 401);
}

#[tokio::test]
async fn test_queue_stats_accuracy() {
    let session_token = setup_authenticated_session().await;
    
    // Push 5 items
    for _ in 0..5 {
        client
            .post("http://localhost:8787/queues/test-stats/push")
            .header("Authorization", format!("Bearer {}", session_token))
            .json(&json!({"payload": {"data": "test"}}))
            .send()
            .await
            .unwrap();
    }
    
    // Get stats
    let response = client
        .get("http://localhost:8787/queues/test-stats/stats")
        .header("Authorization", format!("Bearer {}", session_token))
        .send()
        .await
        .unwrap();
    
    let stats = response.json::<StatsResponse>().await.unwrap();
    assert_eq!(stats.length, 5);
    assert!(stats.oldest_timestamp.is_some());
    assert!(stats.newest_timestamp.is_some());
    assert!(stats.total_size_bytes > 0);
}
```

### 3.2 C# CLI Integration Tests

**Setup**: Build CLI binary and execute as process

**Test Suite** (`client/Forebay.Tests.Integration/CliIntegrationTests.cs`):

```csharp
public class CliIntegrationTests : IDisposable
{
    private readonly string _cliBinaryPath;
    private readonly string _testConfigPath;
    
    [Fact]
    public async Task Cli_PushAndPull_WorksEndToEnd()
    {
        // 1. Setup session token in config
        CreateTestConfig();
        
        // 2. Push message
        var pushResult = await RunCli("push", "test-queue", "{\"message\":\"hello\"}");
        pushResult.ExitCode.Should().Be(0);
        pushResult.StdOut.Should().Contain("Pushed to test-queue");
        
        // 3. Pull message
        var pullResult = await RunCli("pull", "test-queue");
        pullResult.ExitCode.Should().Be(0);
        pullResult.StdOut.Should().Contain("\"message\":\"hello\"");
        pullResult.StdErr.Should().Contain("Remaining: 0");
    }
    
    [Fact]
    public async Task Cli_StdinPipe_Works()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _cliBinaryPath,
                Arguments = "push test-queue",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        
        process.Start();
        await process.StandardInput.WriteLineAsync("{\"message\":\"from stdin\"}");
        process.StandardInput.Close();
        await process.WaitForExitAsync();
        
        process.ExitCode.Should().Be(0);
        var output = await process.StandardOutput.ReadToEndAsync();
        output.Should().Contain("Pushed to test-queue");
    }
    
    [Fact]
    public async Task Cli_WithoutLogin_ShowsErrorMessage()
    {
        DeleteTestConfig();
        
        var result = await RunCli("push", "test-queue", "{}");
        result.ExitCode.Should().Be(1);
        result.StdErr.Should().Contain("Not logged in");
    }
    
    [Fact]
    public async Task Cli_DeleteWithConfirmation_WorksCorrectly()
    {
        var process = new Process { ... };
        process.Start();
        
        // Wait for prompt
        var output = await process.StandardOutput.ReadLineAsync();
        output.Should().Contain("Delete queue");
        
        // Send 'y'
        await process.StandardInput.WriteLineAsync("y");
        await process.WaitForExitAsync();
        
        process.ExitCode.Should().Be(0);
    }
    
    private async Task<CliResult> RunCli(params string[] args)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = _cliBinaryPath,
            Arguments = string.Join(" ", args),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
        
        await process.WaitForExitAsync();
        
        return new CliResult
        {
            ExitCode = process.ExitCode,
            StdOut = await process.StandardOutput.ReadToEndAsync(),
            StdErr = await process.StandardError.ReadToEndAsync(),
        };
    }
}
```

### 3.3 End-to-End Tests

**Full Workflow Test** (`e2e/tests/full_workflow.rs`):

```rust
#[tokio::test]
async fn test_complete_user_workflow() {
    // 1. User logs in via CLI
    let login_output = Command::new("forebay")
        .args(&["login", "--id-token", MOCK_JWT])
        .output()
        .await
        .unwrap();
    
    assert!(login_output.status.success());
    
    // 2. Push messages via CLI
    for i in 1..=10 {
        let output = Command::new("forebay")
            .args(&["push", "my-queue"])
            .stdin(Stdio::piped())
            .stdout(Stdio::piped())
            .spawn()
            .unwrap();
        
        output.stdin.unwrap()
            .write_all(format!(r#"{{"id":{}}}"#, i).as_bytes())
            .await
            .unwrap();
        
        let result = output.wait_with_output().await.unwrap();
        assert!(result.status.success());
    }
    
    // 3. Check stats
    let stats_output = Command::new("forebay")
        .args(&["stats", "my-queue"])
        .output()
        .await
        .unwrap();
    
    let stats_str = String::from_utf8(stats_output.stdout).unwrap();
    assert!(stats_str.contains("Length: 10"));
    
    // 4. Pull all messages in order
    for i in 1..=10 {
        let output = Command::new("forebay")
            .args(&["pull", "my-queue"])
            .output()
            .await
            .unwrap();
        
        let payload = String::from_utf8(output.stdout).unwrap();
        assert!(payload.contains(&format!(r#""id":{}"#, i)));
    }
    
    // 5. Verify queue empty
    let empty_pull = Command::new("forebay")
        .args(&["pull", "my-queue"])
        .output()
        .await
        .unwrap();
    
    assert!(!empty_pull.status.success());
    let stderr = String::from_utf8(empty_pull.stderr).unwrap();
    assert!(stderr.contains("Queue is empty"));
}
```

---

## 4. Test Infrastructure

### 4.1 Code Coverage Tools

**Rust**:
```bash
# Install cargo-tarpaulin
cargo install cargo-tarpaulin

# Generate coverage report
cargo tarpaulin --out Html --output-dir coverage

# CI: Upload to codecov
cargo tarpaulin --out Xml
bash <(curl -s https://codecov.io/bash)
```

**C#**:
```bash
# Install coverlet
dotnet add package coverlet.msbuild

# Generate coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage
```

### 4.2 CI/CD Integration

**GitHub Actions** (`.github/workflows/test.yml`):

```yaml
name: Test

on: [push, pull_request]

jobs:
  test-rust:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: dtolnay/rust-toolchain@stable
      - name: Run unit tests
        run: cd worker && cargo test
      
      - name: Run coverage
        run: |
          cargo install cargo-tarpaulin
          cd worker && cargo tarpaulin --out Xml
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: worker/cobertura.xml
          flags: rust-worker
  
  test-csharp:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Run unit tests
        run: |
          cd client
          dotnet test --collect:"XPlat Code Coverage"
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: client/**/coverage.cobertura.xml
          flags: csharp-client
  
  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Start wrangler dev
        run: |
          cd worker
          npx wrangler dev &
          sleep 5
      
      - name: Run integration tests
        run: cd worker && cargo test --test integration_tests
```

### 4.3 Mock OAuth Provider

**Strategy**: Create local OAuth mock server for tests

```rust
// tests/mock_oauth_server.rs
use axum::{Router, Json};
use serde_json::json;

async fn mock_jwks_handler() -> Json<Value> {
    Json(json!({
        "keys": [{
            "kid": "test-key-id",
            "n": "...", // Test RSA public key
            "e": "AQAB"
        }]
    }))
}

pub async fn start_mock_oauth_server() {
    let app = Router::new()
        .route("/oauth2/v3/certs", get(mock_jwks_handler));
    
    axum::Server::bind(&"127.0.0.1:8080".parse().unwrap())
        .serve(app.into_make_service())
        .await
        .unwrap();
}
```

**Usage in Tests**:
```rust
#[tokio::test]
async fn test_with_mock_oauth() {
    // Start mock server
    tokio::spawn(start_mock_oauth_server());
    
    // Override GOOGLE_CERTS_URL in test env
    std::env::set_var("GOOGLE_CERTS_URL", "http://localhost:8080/oauth2/v3/certs");
    
    // Run tests
}
```

### 4.4 Test Data Management

**KV Cleanup**:
```rust
// tests/helpers.rs
pub async fn cleanup_test_kv(kv: &KvStore, prefix: &str) {
    // Note: KV doesn't have list, so maintain test key tracking
    let test_keys = vec![
        format!("{}test-session-1", prefix),
        format!("{}test-session-2", prefix),
        // ...
    ];
    
    for key in test_keys {
        let _ = kv.delete(&key).await;
    }
}
```

**Test Isolation**:
```rust
#[tokio::test]
async fn test_with_isolation() {
    let test_id = Uuid::new_v4();
    let queue_name = format!("test-queue-{}", test_id);
    
    // Use unique queue name
    // Auto-cleanup via test_id
}
```

---

## 5. Coverage Targets by Component

### 5.1 Critical Components (95-100%)

**Security & Auth**:
- `auth.rs::verify_google_token()` - **100%** (JWT verification)
- `auth.rs::get_session()` - **100%** (auth middleware)
- `auth.rs::extract_token()` - **100%** (token parsing)
- `auth.rs::handle_login()` - **95%** (allowlist checking)

**Queue Data Integrity**:
- `queue.rs::handle_push()` - **95%** (FIFO guarantee)
- `queue.rs::handle_pull()` - **95%** (FIFO enforcement)
- `queue.rs` queue name validation - **100%**

### 5.2 Core Components (90-95%)

**HTTP Handlers**:
- `auth.rs::handle_whoami()` - **90%**
- `auth.rs::handle_logout()` - **90%**
- `queue.rs::handle_stats()` - **90%**
- `queue.rs::handle_delete()` - **90%**
- `queue.rs::handle_list()` - **90%**

**Client Library**:
- `ForebayClient.cs` - **90%**
- `ConfigManager.cs` - **95%** (already high)

### 5.3 Models & DTOs (80-90%)

**Serialization**:
- `models.rs` - **100%** (already covered)
- `error.rs` - **100%** (already covered)
- C# models - **85%**

### 5.4 CLI (85-90%)

**Command Handlers**:
- `AuthCommands.cs` - **85%**
- `QueueCommands.cs` - **90%** (critical stdin/stdout)
- `Program.cs` - **80%**

### 5.5 Acceptable Exceptions (<80%)

**Platform-Specific Code**:
- chmod error handling (line 148 in `ForebayConfig.cs`) - **0%** OK
- Platform detection logic (covered by manual testing)

**Unreachable Code**:
- Some error fallbacks that cannot occur in practice

---

## 6. Implementation Priority

### Phase 1: Security Critical (Week 1)

**Priority: HIGHEST**

1. **Auth Middleware Tests** (Risk: Auth bypass)
   - `get_session()` comprehensive tests
   - Session expiry detection
   - Missing/invalid token handling
   - KV lookup failures

2. **JWT Verification Tests** (Risk: Unauthorized access)
   - Invalid kid
   - Unverified email
   - Expired tokens
   - Wrong audience
   - Malformed JWT

3. **Integration: Auth Flow** (Risk: Session management)
   - Login → Whoami → Logout
   - Session invalidation
   - Concurrent sessions

**Deliverable**: 100% coverage on auth paths

### Phase 2: Data Integrity (Week 2)

**Priority: HIGH**

1. **Queue Handler Integration Tests**
   - FIFO ordering guarantee
   - Concurrent push/pull
   - Queue empty edge case
   - Stats accuracy
   - Delete behavior

2. **CLI Integration Tests**
   - stdin/stdout piping
   - JSON parsing fallback
   - Error exit codes
   - Config file management

3. **Error Handling Tests**
   - KV deserialization errors
   - HTTP error responses
   - Cancellation token handling

**Deliverable**: 90% coverage on queue operations and CLI

### Phase 3: Comprehensive Coverage (Week 3)

**Priority: MEDIUM**

1. **Remaining Unit Tests**
   - ClearSessionToken
   - Error parsing fallbacks
   - Platform-specific edge cases

2. **E2E Workflow Tests**
   - Complete user workflows
   - Multi-step operations
   - Cleanup verification

3. **Coverage Report Analysis**
   - Identify remaining gaps
   - Add tests for uncovered branches
   - Document exceptions

**Deliverable**: 90%+ coverage across all components

---

## 7. Specific Test Cases to Add

### High-Priority Test Cases

1. **Auth bypass attempt**
   - Test: Auth middleware rejects requests with expired session tokens
   - File: `worker/tests/auth_integration_tests.rs`
   - Criticality: CRITICAL

2. **FIFO guarantee under concurrency**
   - Test: Queue push maintains FIFO order with concurrent pushes
   - File: `worker/tests/queue_integration_tests.rs`
   - Criticality: CRITICAL

3. **CLI stdin handling**
   - Test: CLI handles stdin pipe correctly for multi-line JSON
   - File: `client/Forebay.Tests.Integration/CliStdinTests.cs`
   - Criticality: HIGH

4. **JWT with unverified email**
   - Test: Login rejects JWT where email_verified=false
   - File: `worker/src/auth.rs` tests
   - Criticality: CRITICAL

5. **Queue name validation boundary**
   - Test: Queue names at exactly 64 chars accepted, 65 rejected
   - File: `worker/src/queue.rs` tests
   - Criticality: MEDIUM

6. **Session expiry during request**
   - Test: Request with session that expires mid-flight
   - File: `worker/tests/auth_integration_tests.rs`
   - Criticality: HIGH

7. **KV corrupted data recovery**
   - Test: Queue data deserialize failure creates new queue
   - File: `worker/src/queue.rs` tests
   - Criticality: MEDIUM

8. **CLI error exit codes**
   - Test: All error conditions set exit code 1
   - File: `client/Forebay.Tests.Integration/CliErrorTests.cs`
   - Criticality: MEDIUM

---

## 8. Success Criteria

### Quantitative Metrics

- ✅ **90%** line coverage across entire codebase
- ✅ **100%** coverage on security-critical paths
- ✅ **95%** coverage on queue operations
- ✅ **Zero** critical paths untested
- ✅ **All** integration tests passing
- ✅ **CI** runs tests automatically on every PR

### Qualitative Metrics

- ✅ Every test has clear purpose and documentation
- ✅ Tests are deterministic and fast (<5 min total)
- ✅ Mock services are realistic and maintainable
- ✅ Test failures clearly indicate root cause
- ✅ Coverage reports generated automatically
- ✅ No flaky tests in CI

### Deliverables

1. **62+ additional unit tests** (targeting ~150 total)
2. **15+ integration tests** for Worker
3. **10+ integration tests** for CLI
4. **5+ end-to-end workflow tests**
5. **CI/CD pipeline** with coverage reporting
6. **Mock OAuth server** for testing
7. **Coverage badges** in README
8. **Test documentation** for maintainers

---

## 9. Next Steps

### Immediate Actions

1. **Fix CLI build** - Resolve System.CommandLine API compatibility
2. **Set up cargo-tarpaulin** - Install and run baseline coverage
3. **Set up coverlet** - Install and run C# coverage
4. **Create test branches** - One per phase for parallel work

### Development Workflow

1. Write test first (TDD)
2. Run test locally and verify failure
3. Implement feature/fix
4. Verify test passes
5. Check coverage delta
6. PR with coverage report

### Review Schedule

- **Daily**: Coverage metrics review
- **Weekly**: Phase completion checkpoint
- **End of Phase 3**: Final coverage audit and documentation

---

## Appendix A: Test Naming Conventions

### Rust
```rust
#[test]
fn test_{component}_{scenario}_{expected_result}()
```

Examples:
- `test_auth_expired_session_returns_401()`
- `test_queue_fifo_maintains_insertion_order()`

### C#
```csharp
[Fact]
public async Task {Method}_{Scenario}_{ExpectedResult}()
```

Examples:
- `LoginAsync_WithInvalidToken_ThrowsForebayApiException()`
- `PushCommand_WithStdinInput_ParsesJsonCorrectly()`

## Appendix B: Coverage Commands

```bash
# Rust - Generate HTML report
cd worker
cargo tarpaulin --out Html --output-dir coverage
open coverage/index.html

# C# - Generate HTML report
cd client
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
open coverage/index.html

# Combined coverage report
./scripts/generate_coverage_report.sh
```

---

**Plan Version**: 1.0  
**Last Updated**: 2026-01-04  
**Estimated Effort**: 3 weeks (1 developer)  
**Target Completion**: End of January 2026
