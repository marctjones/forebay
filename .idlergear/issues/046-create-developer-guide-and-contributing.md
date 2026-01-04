---
id: 46
title: Create developer guide and contributing documentation
state: open
created: '2026-01-04T09:25:28.478079Z'
labels:
- documentation
- developer-guide
priority: medium
---
Write comprehensive developer documentation for contributors.

**Documentation Files to Create:**

## 1. CONTRIBUTING.md

**Sections:**
- Welcome message
- Code of conduct link
- How to contribute
  - Reporting bugs
  - Suggesting features
  - Submitting pull requests
- Development setup
  - Prerequisites
  - Clone and build
  - Running tests
  - Local Worker development
- Code style
  - Rust: cargo fmt, clippy
  - C#: EditorConfig, StyleCop
- Commit message conventions
- PR guidelines
- Review process

## 2. docs/DEVELOPMENT.md

**Sections:**

### Project Structure
```
forebay/
├── worker/           # Rust Cloudflare Worker
│   ├── src/
│   │   ├── auth.rs   # Authentication
│   │   ├── queue.rs  # Queue operations
│   │   └── lib.rs    # Router
│   └── Cargo.toml
├── client/           # C# Client
│   ├── Forebay.Core/       # Core library
│   ├── Forebay.Cli/        # CLI app
│   └── Forebay.Tests/      # Tests
└── docs/
```

### Development Workflow
1. Create feature branch
2. Write tests first (TDD)
3. Implement feature
4. Run all tests
5. Submit PR

### Testing Strategy
- Unit tests: Pure logic
- Integration tests: Deployed Worker
- E2E tests: CLI + Worker
- Coverage target: 80%+

### Local Development

**Rust Worker:**
```bash
cd worker
cargo test              # Run tests
wrangler dev           # Local development server
wrangler deploy --env preview  # Deploy to preview
```

**C# Client:**
```bash
cd client
dotnet test            # Run tests
dotnet run --project Forebay.Cli  # Run CLI
```

### Debugging
- Worker logs: `wrangler tail`
- CLI debugging: Visual Studio / VS Code
- KV inspection: Cloudflare dashboard

### Release Process
1. Update version numbers
2. Update CHANGELOG.md
3. Create git tag `v1.0.0`
4. Push tag → triggers CI/CD
5. Verify release artifacts

## 3. docs/ARCHITECTURE.md

**Sections:**

### System Overview
- Component diagram
- Data flow
- Technology stack

### Worker Architecture
- Router pattern
- Authentication middleware
- KV storage patterns
- Error handling

### Client Architecture
- Command pattern
- Configuration management
- HTTP client abstraction

### Design Decisions
- Why Rust for Worker
- Why C# for Client
- Why KV over D1
- FIFO queue implementation

### Security Considerations
- OAuth 2.0 PKCE flow
- Session token storage
- Email allowlist
- KV access patterns

### Performance
- Worker cold start: <200ms
- KV latency: <50ms P95
- Queue throughput: 100+ ops/sec

### Future Enhancements
- Queue subscriptions (WebSocket)
- Message filtering
- Dead letter queues
- Queue analytics

**Acceptance Criteria:**
- [ ] All docs files created
- [ ] Clear contribution process
- [ ] Development setup documented
- [ ] Architecture explained
- [ ] Diagrams included
- [ ] Code examples provided
- [ ] Reviewed by project maintainers
