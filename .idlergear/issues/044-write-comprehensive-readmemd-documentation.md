---
id: 44
title: Write comprehensive README.md documentation
state: open
created: '2026-01-04T09:25:27.753604Z'
labels:
- documentation
- readme
priority: high
---
Create user-facing documentation in README.md covering setup, usage, and architecture.

**README Structure:**

## 1. Header & Badges
- Project title and tagline
- Build status badge (GitHub Actions)
- Test coverage badge (Codecov)
- License badge
- Version badge

## 2. Overview
- What is Forebay?
- Key features
- Use cases
- Quick example

```bash
# Quick start
forebay login
echo "task data" | forebay push work/tasks
forebay pull work/tasks
```

## 3. Installation

### Binary Downloads
- Links to latest releases for each platform
- SHA256 checksums
- Installation instructions per platform

### Build from Source
- Prerequisites (Rust, .NET 9.0)
- Clone and build instructions
- Running tests

## 4. Getting Started

### Initial Setup
1. Install Forebay CLI
2. Run `forebay login` (Google OAuth)
3. Configure worker URL (if custom)

### Basic Usage
- Authenticate: `forebay login`
- Push to queue: `forebay push <queue> <message>`
- Pull from queue: `forebay pull <queue>`
- View stats: `forebay stats <queue>`
- List queues: `forebay list`
- Delete queue: `forebay delete <queue>`

## 5. Authentication
- Google OAuth flow explanation
- Session token storage (~/.config/forebay/)
- Token expiration (30 days)
- Multiple accounts

## 6. Queue Operations
- FIFO ordering
- Queue naming conventions
- Payload size limits
- JSON payloads

## 7. Advanced Usage
- Piping data
- Batch operations
- Using with scripts
- CI/CD integration

## 8. Architecture
- High-level diagram
- Cloudflare Worker backend (Rust)
- KV storage
- REST API
- C# CLI client

## 9. Development
- Running tests
- Local development setup
- Contributing guidelines
- Code of conduct

## 10. Deployment (for self-hosting)
- Cloudflare Worker setup
- KV namespace creation
- Environment variables
- Custom deployment

## 11. Troubleshooting
- Common issues and solutions
- Authentication problems
- Network errors
- Queue not found

## 12. License
- MIT or Apache 2.0

## 13. Acknowledgments
- Dependencies
- Contributors
- Phase 0 viability testing

**Acceptance Criteria:**
- [ ] Clear and concise
- [ ] All commands documented
- [ ] Installation instructions for all platforms
- [ ] Examples throughout
- [ ] Badges working
- [ ] Table of contents
- [ ] Links verified
- [ ] Screenshots/diagrams (optional)

**Assets to Create:**
- Architecture diagram
- CLI usage GIF (optional)
- Logo (optional)
