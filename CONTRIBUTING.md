# Contributing to Forebay

Thank you for your interest in contributing to Forebay! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
- [Development Setup](#development-setup)
- [Code Style](#code-style)
- [Testing](#testing)
- [Commit Messages](#commit-messages)
- [Pull Request Guidelines](#pull-request-guidelines)
- [Review Process](#review-process)

## Code of Conduct

This project adheres to a code of conduct that all contributors are expected to follow. Please be respectful, inclusive, and considerate in all interactions.

**Expected Behavior:**
- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Gracefully accept constructive criticism
- Focus on what is best for the community
- Show empathy towards other community members

**Unacceptable Behavior:**
- Harassment, trolling, or derogatory comments
- Personal or political attacks
- Publishing others' private information without permission
- Any conduct that could reasonably be considered inappropriate in a professional setting

## How to Contribute

### Reporting Bugs

If you find a bug:

1. **Check existing issues** to see if it's already reported
2. **Create a new issue** with:
   - Clear, descriptive title
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (OS, .NET version, Rust version)
   - Error messages and logs

**Bug Report Template:**
```markdown
**Description**
A clear description of the bug

**To Reproduce**
1. Run command '...'
2. See error

**Expected Behavior**
What you expected to happen

**Actual Behavior**
What actually happened

**Environment**
- OS: Ubuntu 22.04
- Forebay CLI: v1.0.0
- .NET: 9.0.0

**Logs**
```
Error output here
```
```

### Suggesting Features

We welcome feature suggestions!

1. **Check existing issues** for similar requests
2. **Create a new issue** with:
   - Clear use case
   - Proposed solution
   - Alternatives considered
   - Examples of how it would work

**Feature Request Template:**
```markdown
**Problem**
What problem does this solve?

**Proposed Solution**
How should it work?

**Alternatives**
What other approaches did you consider?

**Examples**
```bash
forebay new-feature --option value
```
```

### Submitting Pull Requests

Ready to contribute code?

1. **Fork the repository**
2. **Create a feature branch** from `main`:
   ```bash
   git checkout -b feature/my-awesome-feature
   ```
3. **Write tests first** (TDD approach)
4. **Implement your changes**
5. **Ensure tests pass**
6. **Commit with clear messages** (see [Commit Messages](#commit-messages))
7. **Push to your fork**
8. **Open a Pull Request**

## Development Setup

### Prerequisites

- **Rust** (latest stable): https://rustup.rs/
- **.NET 9.0 SDK**: https://dotnet.microsoft.com/download/dotnet/9.0
- **Wrangler CLI**: https://developers.cloudflare.com/workers/wrangler/install-and-update/
- **Git**: https://git-scm.com/

### Clone and Build

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/forebay.git
cd forebay

# Add upstream remote
git remote add upstream https://github.com/original-owner/forebay.git

# Rust Worker setup
cd worker
cargo build
cargo test

# C# Client setup
cd ../client
dotnet restore
dotnet build
dotnet test
```

### Running Tests

**All tests:**
```bash
# From project root
./scripts/test-all.sh
```

**Rust Worker tests:**
```bash
cd worker
cargo test
cargo test -- --nocapture  # Show output
cargo test test_name       # Run specific test
```

**C# Client tests:**
```bash
cd client
dotnet test
dotnet test --verbosity detailed
dotnet test --filter "FullyQualifiedName~ForebayClient"
```

### Local Development

**Run Worker locally:**
```bash
cd worker
wrangler dev

# Worker will be available at http://localhost:8787
```

**Run CLI against local Worker:**
```bash
cd client

# Set environment variable for local development
export FOREBAY_WORKER_URL=http://localhost:8787

# Run CLI
dotnet run --project Forebay.Cli -- --help
dotnet run --project Forebay.Cli -- login
```

**Watch mode for development:**
```bash
# Rust (requires cargo-watch)
cargo install cargo-watch
cd worker
cargo watch -x test

# C# (built into dotnet)
cd client
dotnet watch test --project Forebay.Tests
```

## Code Style

### Rust Code Style

We follow standard Rust conventions:

```bash
# Format code
cargo fmt

# Check formatting without changing files
cargo fmt --check

# Lint with Clippy
cargo clippy
cargo clippy -- -D warnings  # Treat warnings as errors
```

**Rust Style Guidelines:**
- Use `rustfmt` with default settings
- Follow Clippy suggestions
- Prefer `&str` over `String` for function parameters
- Use `Result<T, E>` for error handling
- Document public APIs with `///` doc comments
- Keep functions small and focused

**Example:**
```rust
/// Verifies a Google ID token and extracts the email.
///
/// # Arguments
/// * `token` - The JWT token string from Google OAuth
///
/// # Returns
/// * `Ok(email)` - The verified email address
/// * `Err(error)` - Invalid token or verification failure
pub async fn verify_google_token(token: &str) -> Result<String, Error> {
    // Implementation
}
```

### C# Code Style

We follow .NET conventions with EditorConfig:

```bash
# Format code
dotnet format

# Check formatting without changing files
dotnet format --verify-no-changes
```

**C# Style Guidelines:**
- Use PascalCase for public members
- Use camelCase for private fields
- Prefer `var` when type is obvious
- Use async/await for I/O operations
- Document public APIs with XML comments
- Follow .NET naming conventions

**.editorconfig Example:**
```ini
[*.cs]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true
```

**Example:**
```csharp
/// <summary>
/// Pushes a message to the specified queue.
/// </summary>
/// <param name="queueName">The name of the queue.</param>
/// <param name="payload">The JSON payload to push.</param>
/// <returns>Push response containing message ID.</returns>
public async Task<PushResponse> PushAsync(string queueName, JsonElement payload)
{
    // Implementation
}
```

## Testing

We aim for **80%+ test coverage** and follow Test-Driven Development (TDD).

### Testing Strategy

1. **Unit Tests**: Test individual functions and logic
2. **Integration Tests**: Test against deployed Worker
3. **E2E Tests**: Test CLI + Worker together

### Writing Tests

**Rust Unit Tests:**
```rust
#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_session_data_serialization() {
        let session = SessionData {
            email: "test@example.com".to_string(),
            created_at: 1609459200000,
            expires_at: 1612137600000,
        };

        let json = serde_json::to_string(&session).unwrap();
        assert!(json.contains("\"email\":\"test@example.com\""));

        let deserialized: SessionData = serde_json::from_str(&json).unwrap();
        assert_eq!(deserialized.email, "test@example.com");
    }
}
```

**C# Unit Tests (xUnit):**
```csharp
public class ForebayClientTests
{
    [Fact]
    public async Task PushAsync_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("/queues/test/push")
            .Respond("application/json", "{\"id\":\"123\",\"queue_name\":\"test\"}");

        var client = new ForebayClient("http://test", mockHandler.ToHttpClient());
        client.SetSessionToken("token");

        var payload = JsonDocument.Parse("{\"test\":true}").RootElement;

        // Act
        var result = await client.PushAsync("test", payload);

        // Assert
        result.Id.Should().Be("123");
        result.QueueName.Should().Be("test");
    }
}
```

### Test Coverage

**Generate coverage reports:**
```bash
# Rust (using tarpaulin)
cargo install cargo-tarpaulin
cd worker
cargo tarpaulin --out Html

# C# (using coverlet)
cd client
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=html
```

## Commit Messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

**Format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Build process, tooling, etc.

**Examples:**
```bash
feat(cli): add queue stats command

Implement 'forebay stats <queue>' command to display queue statistics
including size, total pushed, and total pulled.

Closes #42

---

fix(worker): correct FIFO ordering in queue pull

Queue was returning messages in reverse order. Fixed by using
pop() instead of remove(0) on the items vector.

Fixes #38

---

docs(api): add OpenAPI specification

Add complete OpenAPI 3.0 spec for all API endpoints with examples
and data models.

---

test(auth): add unit tests for session management

Add tests for session creation, validation, and expiration.
Coverage increased from 65% to 82%.
```

**Guidelines:**
- Use imperative mood ("add" not "added")
- Keep subject line under 72 characters
- Separate subject from body with blank line
- Wrap body at 72 characters
- Reference issues and PRs in footer

## Pull Request Guidelines

### Before Submitting

- [ ] Code follows project style guidelines
- [ ] All tests pass locally
- [ ] New tests added for new features
- [ ] Documentation updated (README, API docs, etc.)
- [ ] Commit messages follow conventions
- [ ] No merge conflicts with main branch
- [ ] PR description clearly explains changes

### PR Template

When opening a PR, include:

```markdown
## Description
Brief description of what this PR does

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to change)
- [ ] Documentation update

## Testing
How was this tested?

- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review performed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings
- [ ] Tests added and passing
- [ ] Dependent changes merged

## Related Issues
Closes #123
```

### PR Size

Keep PRs focused and reasonably sized:

- **Small** (< 200 lines): Ideal, quick to review
- **Medium** (200-500 lines): Acceptable
- **Large** (> 500 lines): Consider splitting into multiple PRs

## Review Process

### What to Expect

1. **Initial Review**: Maintainers review within 2-3 business days
2. **Feedback**: Reviewers may request changes
3. **Iteration**: Address feedback, push updates
4. **Approval**: Requires 1+ approvals from maintainers
5. **Merge**: Maintainer merges after approval

### Review Criteria

Reviewers check for:

- **Correctness**: Does it work as intended?
- **Tests**: Adequate test coverage?
- **Style**: Follows code style guidelines?
- **Documentation**: Clear comments and docs?
- **Design**: Good architectural decisions?
- **Performance**: No obvious performance issues?
- **Security**: No vulnerabilities introduced?

### Responding to Feedback

- Address all comments
- Ask questions if unclear
- Mark conversations as resolved
- Update PR description if scope changes
- Be respectful and professional

## Development Workflow

### Typical Workflow

```bash
# 1. Sync with upstream
git checkout main
git pull upstream main

# 2. Create feature branch
git checkout -b feature/my-feature

# 3. Write tests (TDD)
# Edit test files first

# 4. Run tests (should fail)
cargo test  # or dotnet test

# 5. Implement feature
# Edit source files

# 6. Run tests (should pass)
cargo test  # or dotnet test

# 7. Format code
cargo fmt
dotnet format

# 8. Commit changes
git add .
git commit -m "feat: add my feature"

# 9. Push to fork
git push origin feature/my-feature

# 10. Open Pull Request
# Use GitHub web interface
```

### Keeping Fork Updated

```bash
# Fetch upstream changes
git fetch upstream

# Merge into your main
git checkout main
git merge upstream/main

# Rebase feature branch
git checkout feature/my-feature
git rebase main

# Force push if already pushed
git push origin feature/my-feature --force-with-lease
```

## Release Process

For maintainers releasing new versions:

1. Update version numbers (see scripts/bump-version.sh)
2. Update CHANGELOG.md
3. Create git tag: `git tag -a v1.0.0 -m "Release 1.0.0"`
4. Push tag: `git push origin v1.0.0`
5. CI/CD builds and deploys automatically
6. Create GitHub Release with notes from CHANGELOG

## Getting Help

- **Questions**: Open a GitHub Discussion
- **Bugs**: Open a GitHub Issue
- **Security**: Email security@example.com (do not open public issue)
- **Chat**: Join our Discord/Slack (if available)

## Recognition

All contributors are recognized in:
- GitHub contributors page
- CHANGELOG.md release notes
- README.md acknowledgments section

Thank you for contributing to Forebay!
