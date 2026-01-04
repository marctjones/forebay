# Development Guide

This guide covers the development workflow, architecture, and best practices for Forebay contributors and maintainers.

## Table of Contents

- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Testing Strategy](#testing-strategy)
- [Local Development](#local-development)
- [Debugging](#debugging)
- [Release Process](#release-process)
- [Architecture Deep Dive](#architecture-deep-dive)

## Project Structure

```
forebay/
├── worker/                   # Rust Cloudflare Worker
│   ├── src/
│   │   ├── lib.rs           # Router and main entry point
│   │   ├── auth.rs          # Authentication (Google OAuth, sessions)
│   │   ├── queue.rs         # Queue operations (push, pull, stats, etc.)
│   │   ├── models.rs        # Data models and serialization
│   │   └── error.rs         # Error handling and responses
│   ├── Cargo.toml           # Rust dependencies
│   └── wrangler.toml        # Cloudflare Worker configuration
│
├── client/                  # C# Client
│   ├── Forebay.Core/        # Core library (HttpClient wrapper)
│   │   ├── ForebayClient.cs         # Main API client
│   │   ├── Models/
│   │   │   ├── AuthenticationModels.cs  # Auth data models
│   │   │   └── QueueModels.cs           # Queue data models
│   │   └── Exceptions/
│   │       └── ForebayApiException.cs   # API exceptions
│   │
│   ├── Forebay.Cli/         # CLI application
│   │   ├── Program.cs       # Entry point
│   │   ├── Commands/
│   │   │   ├── AuthCommands.cs  # login, logout, whoami
│   │   │   └── QueueCommands.cs # push, pull, stats, list, delete
│   │   └── Configuration/
│   │       └── ConfigManager.cs # Config file management
│   │
│   └── Forebay.Tests/       # Test suite
│       ├── Models/
│       │   ├── AuthenticationModelsTests.cs
│       │   └── QueueModelsTests.cs
│       └── Client/
│           ├── ForebayAuthClientTests.cs
│           └── ForebayQueueClientTests.cs
│
├── docs/                    # Documentation
│   ├── api/                 # API documentation
│   │   ├── openapi.yaml     # OpenAPI 3.0 specification
│   │   └── README.md        # API usage guide
│   ├── DEVELOPMENT.md       # This file
│   └── ARCHITECTURE.md      # Architecture documentation
│
├── scripts/                 # Build and deployment scripts
│   ├── test-all.sh          # Run all tests
│   └── bump-version.sh      # Version bumping script
│
├── .github/                 # GitHub configuration
│   └── workflows/
│       ├── ci.yml           # Continuous integration
│       ├── deploy.yml       # Deployment workflow
│       └── release.yml      # Release automation
│
├── README.md                # Project overview
├── CONTRIBUTING.md          # Contribution guidelines
├── CHANGELOG.md             # Version history
└── LICENSE                  # MIT License
```

### Component Responsibilities

**Worker (Rust)**
- HTTP request routing
- Google OAuth JWT verification
- Session management in KV
- Queue CRUD operations
- FIFO ordering enforcement
- Error handling and responses

**Forebay.Core (C#)**
- HTTP client abstraction
- Request/response serialization
- Session token management
- Type-safe API wrapper
- Exception handling

**Forebay.Cli (C#)**
- Command-line interface
- User interaction
- Configuration management
- Input/output formatting
- Error display

## Development Workflow

### 1. Feature Development (TDD Approach)

```bash
# 1. Create feature branch
git checkout -b feature/queue-subscriptions

# 2. Write failing tests FIRST
# worker/src/queue.rs (add test)
#[test]
fn test_subscribe_to_queue() {
    // Test implementation
}

# 3. Run tests (should fail)
cargo test test_subscribe_to_queue

# 4. Implement feature
# Add subscription logic to queue.rs

# 5. Run tests (should pass)
cargo test test_subscribe_to_queue

# 6. Refactor if needed
# Clean up code while keeping tests green

# 7. Run all tests
cargo test
cd ../client && dotnet test

# 8. Format and lint
cargo fmt && cargo clippy
dotnet format

# 9. Commit
git commit -m "feat(queue): add queue subscription support"
```

### 2. Bug Fix Workflow

```bash
# 1. Create bug fix branch
git checkout -b fix/queue-fifo-ordering

# 2. Write test that reproduces bug
#[test]
fn test_fifo_ordering_preserved() {
    // Test that demonstrates the bug
}

# 3. Run test (should fail)
cargo test test_fifo_ordering_preserved

# 4. Fix the bug
# Modify queue.rs

# 5. Run test (should pass)
cargo test test_fifo_ordering_preserved

# 6. Run regression tests
cargo test

# 7. Commit with issue reference
git commit -m "fix(queue): correct FIFO ordering

Fixes #38"
```

### 3. Refactoring Workflow

```bash
# 1. Ensure all tests pass first
cargo test && dotnet test

# 2. Make refactoring changes
# Extract method, rename variables, etc.

# 3. Run tests after each change
cargo test

# 4. Commit when tests pass
git commit -m "refactor(auth): extract token verification logic"
```

## Testing Strategy

### Unit Tests

Test individual functions and modules in isolation.

**Rust Unit Tests:**
- Located in same file as code: `#[cfg(test)] mod tests`
- Test pure logic (no I/O, no Worker bindings)
- Mock external dependencies
- Goal: Fast, isolated tests

**C# Unit Tests:**
- Located in `Forebay.Tests/` project
- Use xUnit framework
- Mock HttpClient with Moq
- Use FluentAssertions for readable assertions

### Integration Tests

Test against deployed Worker endpoints.

**Location:** `client/Forebay.Tests/Integration/`

**Setup:**
```csharp
[Collection("Integration")]
public class WorkerIntegrationTests : IClassFixture<WorkerFixture>
{
    private readonly ForebayClient _client;

    public WorkerIntegrationTests(WorkerFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task EndToEnd_PushAndPull_Works()
    {
        // Arrange
        await _client.LoginAsync(fixture.TestIdToken);
        var payload = JsonDocument.Parse("{\"test\":true}").RootElement;

        // Act
        var pushResult = await _client.PushAsync("test-queue", payload);
        var pullResult = await _client.PullAsync("test-queue");

        // Assert
        pullResult.Id.Should().Be(pushResult.Id);
        pullResult.Payload.GetProperty("test").GetBoolean().Should().BeTrue();
    }
}
```

### End-to-End Tests

Test CLI + Worker together.

**Location:** `client/Forebay.Tests/E2E/`

**Example:**
```bash
#!/bin/bash
# scripts/e2e-test.sh

# Set up test environment
export FOREBAY_WORKER_URL="https://test-worker.workers.dev"

# Login
forebay login --test-mode

# Push message
echo '{"test": "data"}' | forebay push test-queue

# Pull message
result=$(forebay pull test-queue)

# Verify result
if [[ "$result" == *"test"* ]]; then
  echo "✓ E2E test passed"
else
  echo "✗ E2E test failed"
  exit 1
fi

# Cleanup
forebay delete test-queue
forebay logout
```

### Coverage Goals

- **Overall**: 80%+ coverage
- **Critical paths**: 90%+ coverage (auth, queue operations)
- **Error handling**: All error paths tested
- **Edge cases**: Boundary conditions tested

**Generate coverage:**
```bash
# Rust
cargo tarpaulin --out Html --output-dir coverage/

# C#
dotnet test /p:CollectCoverage=true \
            /p:CoverletOutputFormat=html \
            /p:CoverletOutput=coverage/
```

## Local Development

### Running Worker Locally

```bash
cd worker

# Start development server
wrangler dev

# Worker available at http://localhost:8787

# In another terminal, test endpoints
curl http://localhost:8787/health
```

**Using local KV:**
```bash
# wrangler.toml
[env.local]
kv_namespaces = [
  { binding = "SESSIONS", id = "local-sessions", preview_id = "local-sessions" },
  { binding = "QUEUES", id = "local-queues", preview_id = "local-queues" }
]

# Start with local environment
wrangler dev --env local
```

### Running CLI Against Local Worker

```bash
cd client

# Set worker URL to localhost
export FOREBAY_WORKER_URL=http://localhost:8787

# Or use CLI flag
dotnet run --project Forebay.Cli -- --worker-url http://localhost:8787 whoami

# Or create local config
cat > ~/.config/forebay/config.local.json <<EOF
{
  "workerUrl": "http://localhost:8787",
  "sessionToken": "test-token"
}
EOF

dotnet run --project Forebay.Cli -- --config config.local.json whoami
```

### Hot Reload Development

**Rust Worker:**
```bash
# Install cargo-watch
cargo install cargo-watch

# Watch for changes and restart
cargo watch -x 'test' -x 'run'
```

**C# CLI:**
```bash
# Built-in watch mode
dotnet watch --project Forebay.Cli run -- --help

# Watch tests
dotnet watch test --project Forebay.Tests
```

## Debugging

### Debugging Rust Worker

**Local debugging:**
```bash
# Add debug prints
println!("Debug: queue_name = {}", queue_name);

# Run with output
wrangler dev

# View console output in terminal
```

**Production debugging:**
```bash
# Tail logs from deployed worker
wrangler tail

# Tail with pretty formatting
wrangler tail --format pretty

# Filter logs
wrangler tail --status=error
```

**Using console.log in Worker:**
```rust
use worker::console_log;

console_log!("Request received: {:?}", req);
```

### Debugging C# CLI

**Visual Studio Code:**
```json
// .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (CLI)",
      "type": "coreclr",
      "request": "launch",
      "program": "${workspaceFolder}/client/Forebay.Cli/bin/Debug/net9.0/Forebay.Cli.dll",
      "args": ["push", "test-queue", "{\"test\":true}"],
      "cwd": "${workspaceFolder}/client/Forebay.Cli",
      "stopAtEntry": false,
      "console": "internalConsole"
    }
  ]
}
```

**Command line:**
```bash
# Run with debugger attached (VS Code)
F5 in VS Code

# Or use dotnet CLI with attach
dotnet run --project Forebay.Cli -- push test-queue '{"test":true}'
```

### Debugging Tests

**Rust:**
```bash
# Run single test with output
cargo test test_name -- --nocapture

# Run with backtrace on panic
RUST_BACKTRACE=1 cargo test

# Run with full backtrace
RUST_BACKTRACE=full cargo test
```

**C#:**
```bash
# Run with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~PushAsync_ValidRequest_ReturnsResponse"

# Debug test in VS Code
# Set breakpoint, use Test Explorer "Debug Test"
```

### Common Debugging Scenarios

**401 Unauthorized:**
```bash
# Check session token
cat ~/.config/forebay/config.json

# Test token manually
curl -H "Authorization: Bearer $(jq -r .sessionToken ~/.config/forebay/config.json)" \
     https://worker.url/auth/whoami
```

**Queue not found:**
```bash
# List all queues
forebay list

# Check KV directly (if you have access)
wrangler kv:key get --binding=QUEUES "queue:work/tasks"
```

**FIFO ordering issues:**
```rust
// Add debug logging in queue.rs
console_log!("Queue items before pull: {:?}", queue_data.items);
```

## Release Process

### 1. Version Bumping

```bash
# Update version numbers
./scripts/bump-version.sh 1.1.0

# This updates:
# - worker/Cargo.toml
# - client/Forebay.Cli/Forebay.Cli.csproj
# - client/Forebay.Core/Forebay.Core.csproj
```

### 2. Update CHANGELOG

```markdown
## [1.1.0] - 2026-01-15

### Added
- Queue subscription support via WebSockets
- Batch push operations

### Changed
- Improved error messages

### Fixed
- FIFO ordering bug in queue pull (#38)
```

### 3. Create Release

```bash
# Commit changes
git add .
git commit -m "chore: bump version to 1.1.0"

# Create annotated tag
git tag -a v1.1.0 -m "Release 1.1.0"

# Push tag
git push origin v1.1.0

# GitHub Actions will:
# - Run tests
# - Build Worker and CLI
# - Create GitHub Release
# - Upload binaries
```

### 4. Post-Release

```bash
# Verify release
gh release view v1.1.0

# Test binaries
wget https://github.com/user/forebay/releases/download/v1.1.0/forebay-linux-x64
chmod +x forebay-linux-x64
./forebay-linux-x64 --version
```

## Architecture Deep Dive

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation covering:

- System overview and component diagram
- Worker architecture (routing, auth, storage)
- Client architecture (commands, configuration)
- Design decisions and rationale
- Security considerations
- Performance characteristics
- Future enhancements

## Performance Optimization

### Worker Performance

**KV Storage:**
```rust
// Cache frequently accessed data
// Use list operations to reduce KV calls

// Bad: Multiple KV reads
let session = kv.get("session:token1").await?;
let queue1 = kv.get("queue:q1").await?;
let queue2 = kv.get("queue:q2").await?;

// Good: Batch reads if possible
// (Note: Workers KV doesn't support batch reads, so minimize calls)
```

**Reduce Allocations:**
```rust
// Use &str instead of String when possible
fn process_queue(name: &str) -> Result<()> {
    // name is borrowed, no allocation
}
```

**Async Efficiency:**
```rust
// Use join! for concurrent operations
use futures::join;

let (sessions, queues) = join!(
    kv_sessions.list().execute(),
    kv_queues.list().execute()
);
```

### CLI Performance

**Reduce HTTP Calls:**
```csharp
// Cache session token, don't validate on every command
// Only re-authenticate on 401
```

**Streaming Large Payloads:**
```csharp
// For future large message support
using var stream = await httpClient.GetStreamAsync(url);
using var reader = new StreamReader(stream);
```

## Troubleshooting Development Issues

### Cargo Build Fails

```bash
# Clear cache and rebuild
cargo clean
cargo build

# Update dependencies
cargo update
```

### .NET Build Fails

```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Wrangler Login Issues

```bash
# Re-authenticate
wrangler logout
wrangler login

# Check auth status
wrangler whoami
```

### KV Namespace Issues

```bash
# List namespaces
wrangler kv:namespace list

# Create if missing
wrangler kv:namespace create forebay-sessions
wrangler kv:namespace create forebay-queues

# Update wrangler.toml with IDs
```

## Additional Resources

- [Cloudflare Workers Docs](https://developers.cloudflare.com/workers/)
- [worker-rs Documentation](https://github.com/cloudflare/workers-rs)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Keep a Changelog](https://keepachangelog.com/)

---

For questions or issues, open a GitHub Discussion or Issue.
