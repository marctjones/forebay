# Forebay

[![Tests](https://github.com/yourusername/forebay/workflows/CI/badge.svg)](https://github.com/yourusername/forebay/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)](https://github.com/yourusername/forebay/releases)

> A cross-platform message queue transport system for seamless data flow across machines

Forebay is a universal message transport abstraction that lets you push messages to named queues and retrieve them later across different machines. Think of it as a forebay that buffers and delivers your messages wherever you need them.

## Quick Start

```bash
# Authenticate with Google
forebay login

# Push a message to a queue
echo '{"task": "process_data"}' | forebay push work/tasks

# Pull the message on any machine
forebay pull work/tasks

# Check queue statistics
forebay stats work/tasks
```

## Table of Contents

- [Features](#features)
- [Installation](#installation)
  - [Binary Downloads](#binary-downloads)
  - [Build from Source](#build-from-source)
- [Getting Started](#getting-started)
  - [Authentication](#authentication)
  - [Basic Usage](#basic-usage)
- [Commands](#commands)
- [Queue Operations](#queue-operations)
- [Advanced Usage](#advanced-usage)
- [Architecture](#architecture)
- [Development](#development)
- [Self-Hosting](#self-hosting)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## Features

- **Cross-platform CLI**: Works on Linux, Windows, and macOS
- **Simple authentication**: Google OAuth with 30-day sessions
- **FIFO queues**: First-In-First-Out message ordering
- **JSON payloads**: Structured data support
- **High performance**: Rust-powered Cloudflare Worker backend (107ms cold start, 88ms P95 latency)
- **Secure**: OAuth 2.0 authentication with email allowlist
- **Scriptable**: Easy integration with shell scripts and CI/CD pipelines

## Installation

### Binary Downloads

Download the latest release for your platform:

**Linux (x64)**
```bash
wget https://github.com/yourusername/forebay/releases/latest/download/forebay-linux-x64
chmod +x forebay-linux-x64
sudo mv forebay-linux-x64 /usr/local/bin/forebay
```

**Windows (x64)**
```powershell
# Download from https://github.com/yourusername/forebay/releases/latest/download/forebay-win-x64.exe
# Add to PATH or run directly
```

**macOS (x64)**
```bash
wget https://github.com/yourusername/forebay/releases/latest/download/forebay-macos-x64
chmod +x forebay-macos-x64
sudo mv forebay-macos-x64 /usr/local/bin/forebay
```

**macOS (ARM64)**
```bash
wget https://github.com/yourusername/forebay/releases/latest/download/forebay-macos-arm64
chmod +x forebay-macos-arm64
sudo mv forebay-macos-arm64 /usr/local/bin/forebay
```

### Build from Source

**Prerequisites:**
- [Rust](https://rustup.rs/) (for Worker development)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Wrangler CLI](https://developers.cloudflare.com/workers/wrangler/install-and-update/) (for Worker deployment)

**Clone and Build:**
```bash
# Clone the repository
git clone https://github.com/yourusername/forebay.git
cd forebay

# Build the CLI
cd client
dotnet build
dotnet test

# Build the Worker
cd ../worker
cargo build
cargo test
```

**Run CLI locally:**
```bash
cd client
dotnet run --project Forebay.Cli -- --help
```

## Getting Started

### Authentication

Forebay uses Google OAuth for authentication. On your first use:

```bash
forebay login
```

This will:
1. Open your browser for Google authentication
2. Save a session token to `~/.config/forebay/config.json` (Linux/macOS) or `%APPDATA%\forebay\config.json` (Windows)
3. Keep you logged in for 30 days

**Check who you're logged in as:**
```bash
forebay whoami
```

**Log out:**
```bash
forebay logout
```

### Basic Usage

**Push a message to a queue:**
```bash
# Direct message
forebay push work/tasks '{"task": "process_data", "priority": "high"}'

# From stdin
echo '{"task": "backup_db"}' | forebay push work/tasks

# From file
cat message.json | forebay push work/tasks
```

**Pull a message from a queue:**
```bash
forebay pull work/tasks
```

**View queue statistics:**
```bash
forebay stats work/tasks
```

**List all queues:**
```bash
forebay list
```

**Delete a queue:**
```bash
forebay delete work/tasks
```

## Commands

### Authentication Commands

| Command | Description |
|---------|-------------|
| `forebay login` | Authenticate with Google OAuth |
| `forebay whoami` | Show current user information |
| `forebay logout` | End current session |

### Queue Commands

| Command | Description |
|---------|-------------|
| `forebay push <queue> [message]` | Add message to queue (reads from stdin if message omitted) |
| `forebay pull <queue>` | Retrieve and remove first message from queue |
| `forebay stats <queue>` | Show queue statistics (size, total pushed/pulled) |
| `forebay list` | List all queues |
| `forebay delete <queue>` | Delete a queue and all its messages |

## Queue Operations

### FIFO Ordering

Forebay queues use First-In-First-Out (FIFO) ordering. Messages are returned in the exact order they were pushed:

```bash
forebay push jobs '{"id": 1}'
forebay push jobs '{"id": 2}'
forebay push jobs '{"id": 3}'

forebay pull jobs  # Returns {"id": 1}
forebay pull jobs  # Returns {"id": 2}
forebay pull jobs  # Returns {"id": 3}
```

### Queue Naming

Queue names can contain letters, numbers, slashes, hyphens, and underscores:

```bash
# Valid queue names
forebay push work/tasks '...'
forebay push users/alice/notifications '...'
forebay push batch-jobs '...'
forebay push queue_123 '...'
```

Maximum queue name length: 256 characters

### JSON Payloads

All messages must be valid JSON:

```bash
# Simple object
forebay push queue '{"key": "value"}'

# Array
forebay push queue '[1, 2, 3]'

# Complex nested structure
forebay push queue '{
  "user": "alice",
  "action": "process",
  "data": {
    "files": ["a.txt", "b.txt"],
    "priority": 5
  }
}'
```

## Advanced Usage

### Piping Data Between Queues

```bash
# Move messages from one queue to another
forebay pull source-queue | forebay push dest-queue
```

### Batch Processing

```bash
# Process all messages in a queue
while msg=$(forebay pull work/tasks 2>/dev/null); do
  echo "Processing: $msg"
  # Your processing logic here
done
```

### Using in Scripts

```bash
#!/bin/bash
# Send job to queue
echo '{"job": "backup", "timestamp": "'$(date -Iseconds)'"}' | forebay push jobs/backup

# Monitor queue in background
while true; do
  msg=$(forebay pull jobs/backup 2>/dev/null)
  if [ -n "$msg" ]; then
    echo "Received job: $msg"
    # Process the job
  fi
  sleep 5
done
```

### CI/CD Integration

```yaml
# GitHub Actions example
- name: Queue deployment notification
  run: |
    echo '{"service": "api", "version": "${{ github.sha }}", "env": "production"}' | \
    forebay push deployments/notifications
```

## Architecture

```
┌─────────────┐         HTTPS          ┌──────────────────┐
│             │  ◄──────────────────►  │                  │
│  Forebay    │      REST API          │  Cloudflare      │
│  CLI (C#)   │                        │  Worker (Rust)   │
│             │   Authorization:       │                  │
└─────────────┘   Bearer <token>       └──────────────────┘
                                                │
      Local                                     │
   ~/.config/forebay/                           ▼
   config.json                         ┌──────────────────┐
   (session token)                     │  Cloudflare KV   │
                                       │  - Sessions      │
                                       │  - Queues        │
                                       └──────────────────┘
```

### Technology Stack

- **Backend**: Rust-powered Cloudflare Worker
  - Worker-rs framework
  - KV storage for sessions and queues
  - Google OAuth JWT verification
  - 25 unit tests

- **Client**: C# .NET 9.0
  - System.CommandLine for CLI
  - HttpClient for REST API
  - Cross-platform (Linux, Windows, macOS)
  - 26 unit tests

- **Performance**:
  - Worker cold start: ~107ms
  - Worker warm latency: ~88ms P95
  - KV read/write: <50ms P95

### Why This Stack?

Forebay went through comprehensive Phase 0 viability testing across TypeScript, Rust, and Python workers. Rust was selected for:
- Best performance (107ms cold start vs 195ms TypeScript, 824ms Python)
- Lowest latency (88ms P95 vs 142ms TypeScript, 1489ms Python)
- Type safety and reliability
- Excellent WASM support

C# was selected for the client for:
- Excellent cross-platform CLI support
- Future GUI development with Avalonia
- Strong typing and developer experience
- Easy distribution as single binary

See `worker-viability-tests/RESULTS_FINAL.md` for detailed benchmarks.

## Development

### Running Tests

**Rust Worker:**
```bash
cd worker
cargo test
cargo fmt --check
cargo clippy
```

**C# Client:**
```bash
cd client
dotnet test
dotnet format --verify-no-changes
```

**Coverage:**
- Current test coverage: ~60% (goal: 90%)
- Worker: 25 unit tests
- Client: 37 unit tests (26 ForebayClient + 11 ConfigManager)
- See [Comprehensive Testing Plan](.idlergear/wiki/comprehensive-testing-plan-for-90-coverage.md) for coverage roadmap

### Local Development

**Run Worker locally:**
```bash
cd worker
wrangler dev
```

**Run CLI against local Worker:**
```bash
cd client
dotnet run --project Forebay.Cli -- --worker-url http://localhost:8787 whoami
```

### Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

**Quick contribution workflow:**
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Write tests first (TDD)
4. Implement your feature
5. Ensure all tests pass: `cargo test && dotnet test`
6. Submit a pull request

## Self-Hosting

You can deploy your own Forebay Worker to Cloudflare:

### 1. Create Cloudflare Account

Sign up at [cloudflare.com](https://www.cloudflare.com/) and install [Wrangler CLI](https://developers.cloudflare.com/workers/wrangler/).

### 2. Create KV Namespaces

```bash
wrangler kv:namespace create forebay-sessions
wrangler kv:namespace create forebay-queues
```

### 3. Configure wrangler.toml

```toml
name = "forebay-worker"
main = "src/lib.rs"
compatibility_date = "2024-01-01"

[[kv_namespaces]]
binding = "SESSIONS"
id = "your-sessions-namespace-id"

[[kv_namespaces]]
binding = "QUEUES"
id = "your-queues-namespace-id"

[vars]
GOOGLE_CLIENT_ID = "your-google-oauth-client-id"
ALLOWED_EMAILS = "user1@example.com,user2@example.com"
```

### 4. Deploy

```bash
cd worker
wrangler deploy
```

### 5. Configure CLI

```bash
forebay config set worker-url https://your-worker.workers.dev
```

## Troubleshooting

### Authentication Issues

**Problem**: `forebay login` fails or hangs

**Solutions**:
- Ensure you have a stable internet connection
- Check that your browser isn't blocking popups
- Try logging out and back in: `forebay logout && forebay login`
- Clear config file: `rm ~/.config/forebay/config.json`

### Network Errors

**Problem**: `Connection refused` or timeout errors

**Solutions**:
- Check your internet connection
- Verify Worker URL: `forebay config get worker-url`
- Check Cloudflare Workers status: https://www.cloudflarestatus.com/

### Queue Not Found

**Problem**: `Queue not found` error when pulling

**Solutions**:
- Queue may be empty - check stats: `forebay stats <queue>`
- Queue name is case-sensitive
- Use `forebay list` to see all available queues

### Invalid JSON

**Problem**: `Invalid request body` error when pushing

**Solutions**:
- Ensure message is valid JSON
- Use single quotes around JSON in bash: `forebay push queue '{"key": "value"}'`
- Validate JSON with `jq`: `echo '{"key": "value"}' | jq .`

### Session Expired

**Problem**: `Unauthorized` or `Invalid session token` errors

**Solutions**:
- Sessions expire after 30 days
- Re-authenticate: `forebay login`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [Cloudflare Workers](https://workers.cloudflare.com/) for global edge computing
- Rust Worker powered by [worker-rs](https://github.com/cloudflare/workers-rs)
- C# CLI built with [System.CommandLine](https://github.com/dotnet/command-line-api)
- Phase 0 viability testing informed architecture decisions
- Inspired by the need for simple, reliable cross-machine message transport

---

**Project Status**: Phase 0 Complete - Core Development in Progress

- ✅ Phase 0: Worker viability testing complete (Rust selected)
- ✅ 62 unit tests passing (25 Rust worker + 37 C# client)
- ✅ Google OAuth authentication implemented
- ✅ FIFO queue operations implemented
- ✅ Cross-platform config management
- 🚧 CLI building (System.CommandLine API compatibility issue)
- 🚧 90% test coverage goal (currently ~60%)
- ⏳ Integration tests (planned)
- ⏳ CI/CD pipeline (planned)
- ⏳ GUI applications (future)

### Current Development Status

**What Works:**
- ✅ Rust Worker with auth, queue, KV storage
- ✅ C# Core library with ForebayClient
- ✅ Configuration management (TOML, cross-platform)
- ✅ Test suite: 25 Rust + 37 C# tests passing

**In Progress:**
- 🚧 CLI commands (blocked by System.CommandLine 2.0.1 API changes)
- 🚧 Comprehensive test coverage (targeting 90%)
- 🚧 OAuth PKCE flow implementation
- 🚧 GitHub Actions CI/CD setup

**Next Steps:**
See [Project Organization and Implementation Roadmap](.idlergear/wiki/project-organization-and-implementation-roadmap.md) for detailed milestones and timeline.

### Development Roadmap

The project is organized into 6 milestones leading to v1.0.0:

1. **Core Backend Completion** (v1.0.0-alpha.1) - 2 weeks
   - Fix CLI build issues
   - Implement OAuth PKCE flow
   - Add comprehensive error handling

2. **Comprehensive Testing** (v1.0.0-alpha.2) - 2 weeks
   - Achieve 90% code coverage
   - Add integration tests
   - Set up mock OAuth server

3. **CI/CD Infrastructure** (v1.0.0-beta.1) - 1 week
   - GitHub Actions workflows
   - Automated testing and deployment

4. **Production Hardening** (v1.0.0-rc.1) - 2 weeks
   - Security audit
   - Performance optimization
   - Production monitoring

5. **Documentation & Release** (v1.0.0) - 1 week
   - Complete user documentation
   - API reference
   - Migration guides

6. **GUI Applications** (v1.1.0) - Future
   - Avalonia desktop app
   - System tray integration

For detailed task breakdown, dependencies, and priorities, see:
- [Comprehensive Testing Plan](.idlergear/wiki/comprehensive-testing-plan-for-90-coverage.md)
- [Project Organization and Implementation Roadmap](.idlergear/wiki/project-organization-and-implementation-roadmap.md)

For more information, visit the [documentation](docs/) or [open an issue](https://github.com/yourusername/forebay/issues).
