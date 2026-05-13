# Forebay

[![Tests](https://github.com/marctjones/forebay/workflows/CI/badge.svg)](https://github.com/marctjones/forebay/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.0.0-green.svg)](https://github.com/marctjones/forebay/releases)

> Cross-machine message queues and document storage on Cloudflare's edge

Forebay is a universal transport abstraction: push messages to named FIFO queues, store JSON documents under arbitrary keys, and retrieve either from any machine. A Rust Cloudflare Worker handles the backend; a .NET CLI and Avalonia/TUI reference apps consume it.

## Quick Start

```bash
# Authenticate with your API key (one-time setup)
forebay login <your-api-key>

# Push a message to a queue
echo '{"task": "process_data"}' | forebay push work/tasks

# Pull the message on any machine
forebay pull work/tasks

# Store a JSON document under a named key
forebay put config/app-settings '{"theme": "dark"}'
forebay get config/app-settings
forebay list-docs --prefix config

# Inspect queues
forebay stats work/tasks
forebay list
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
- [Document Storage](#document-storage)
- [Reference Apps](#reference-apps)
- [Advanced Usage](#advanced-usage)
- [Architecture](#architecture)
- [Development](#development)
- [Self-Hosting](#self-hosting)
- [Troubleshooting](#troubleshooting)
- [License](#license)

## Features

- **Cross-platform CLI**: Linux, Windows, macOS (.NET 9)
- **API-key authentication**: Static keys mapped per-user via the worker's `API_KEYS` env var — no OAuth dance, no session expiry
- **FIFO queues**: First-In-First-Out message ordering with per-queue stats
- **Document storage**: PUT/GET/DELETE/list JSON documents by key, with prefix filtering
- **JSON payloads**: Structured data for both messages and documents
- **High performance**: Rust-powered Cloudflare Worker backend (~107ms cold start, ~88ms P95 latency)
- **Reference apps**: Avalonia desktop and terminal-UI task managers showing end-to-end use of queues + storage
- **Scriptable**: Drop-in for shell scripts and CI/CD pipelines

## Installation

### Binary Downloads

Download the latest release for your platform:

**Linux (x64)**
```bash
wget https://github.com/marctjones/forebay/releases/latest/download/forebay-linux-x64
chmod +x forebay-linux-x64
sudo mv forebay-linux-x64 /usr/local/bin/forebay
```

**Windows (x64)**
```powershell
# Download from https://github.com/marctjones/forebay/releases/latest/download/forebay-win-x64.exe
# Add to PATH or run directly
```

**macOS (x64)**
```bash
wget https://github.com/marctjones/forebay/releases/latest/download/forebay-macos-x64
chmod +x forebay-macos-x64
sudo mv forebay-macos-x64 /usr/local/bin/forebay
```

**macOS (ARM64)**
```bash
wget https://github.com/marctjones/forebay/releases/latest/download/forebay-macos-arm64
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
git clone https://github.com/marctjones/forebay.git
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

Forebay uses static API keys. The worker validates the `Authorization: Bearer <key>` header against the `API_KEYS` environment variable (a comma-separated `key:email` list — see [Self-Hosting](#self-hosting)).

To set up the client:

```bash
forebay login <your-api-key>
```

This saves the key to `~/.config/forebay/config.toml` (Linux/macOS) or `%APPDATA%\forebay\config.toml` (Windows). The key is sent on every request and used by the worker to scope your queues and documents to your email.

**Check who you're authenticated as:**
```bash
forebay whoami
```

**Clear the saved key:**
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
| `forebay login <api-key>` | Save the API key to the config file |
| `forebay whoami` | Show the email associated with the saved key |
| `forebay logout` | Clear the saved API key |

### Queue Commands

| Command | Description |
|---------|-------------|
| `forebay push <queue> [message]` | Add message to queue (reads from stdin if message omitted) |
| `forebay pull <queue>` | Retrieve and remove first message from queue |
| `forebay stats <queue>` | Show queue statistics (size, total pushed/pulled) |
| `forebay list` | List all queues |
| `forebay delete <queue>` | Delete a queue and all its messages |

### Document Storage Commands

| Command | Description |
|---------|-------------|
| `forebay put <key> <json>` | Store a JSON document under the given key (creates or replaces) |
| `forebay get <key> [--pretty]` | Retrieve the document stored at the given key |
| `forebay delete-doc <key>` | Delete a stored document |
| `forebay list-docs [--prefix <p>]` | List documents, optionally filtered by key prefix |

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

## Document Storage

Forebay also exposes a small key/value document store alongside the queue API. Use it for persistent state — task lists, configuration, notes — anywhere you'd reach for a tiny shared database.

```bash
# Store a JSON document
forebay put tasks/todo-list '{"tasks":[{"id":1,"text":"Review PRs","done":false}]}'

# Retrieve it
forebay get tasks/todo-list --pretty

# List all documents
forebay list-docs

# Filter by key prefix
forebay list-docs --prefix tasks

# Delete one
forebay delete-doc tasks/todo-list
```

Documents are stored in Cloudflare KV under the authenticated user's namespace. Keys can contain slashes for hierarchical grouping (`config/app-settings`, `notes/2026/standup`), and `list-docs --prefix` is the discovery mechanism. The value must be valid JSON; size limits mirror Cloudflare KV's per-value cap.

Endpoints (for direct API use):

| Method | Path | Action |
|---|---|---|
| `PUT` | `/store/<key>` | Create or replace document |
| `GET` | `/store/<key>` | Read document |
| `DELETE` | `/store/<key>` | Delete document |
| `GET` | `/store` | List documents (supports `?prefix=`) |

## Reference Apps

`apps/` contains two reference applications that exercise the queue + storage API end-to-end. They share state via the worker, so the same task list shows up in both:

- **`Forebay.TaskManager.Avalonia`** — cross-platform desktop GUI (Linux/Windows/macOS) built with Avalonia
- **`Forebay.TaskManager.Tui`** — terminal-UI version for SSH/headless workflows

Run either against the same worker and they'll see each other's writes:

```bash
# Desktop GUI
dotnet run --project apps/Forebay.TaskManager.Avalonia

# Terminal UI
dotnet run --project apps/Forebay.TaskManager.Tui

# Verify both see the same shared state
./apps/test-shared-storage.sh
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
┌──────────────────┐                            ┌─────────────────────┐
│  CLI (.NET 9)    │           HTTPS            │                     │
│  TaskManager.    │  ────────────────────────► │  Cloudflare Worker  │
│    Avalonia      │   Authorization:           │  (Rust + worker-rs) │
│  TaskManager.Tui │   Bearer <api-key>         │                     │
└──────────────────┘                            └──────────┬──────────┘
        │                                                  │
        ▼                                                  ▼
~/.config/forebay/                                 ┌──────────────────┐
config.toml                                        │  Cloudflare KV   │
(API key)                                          │  - QUEUES        │
                                                   │  - documents     │
                                                   └──────────────────┘
```

The worker is a single Rust crate (`worker/`) compiled to WASM and deployed via Wrangler. Each request is authenticated by validating the bearer token against the `API_KEYS` env var (a `key:email` map), then routed to either the queue module (`/queues/...`) or the storage module (`/store/...`). All persistence is Cloudflare KV.

### Technology Stack

- **Backend**: Rust Cloudflare Worker (worker-rs)
  - KV storage for queues and documents
  - Static API-key auth (no JWT, no OAuth)
  - Routes: `/queues/...`, `/store/...`, `/whoami`

- **Client**: .NET 9.0
  - `Forebay.Cli` — System.CommandLine
  - `Forebay.Core` — shared `ForebayClient` HTTP wrapper, Tomlyn-based config
  - `Forebay.Tests` — unit tests

- **Reference apps** (`apps/`):
  - `Forebay.TaskManager.Avalonia` — desktop GUI (MVVM, Avalonia 11)
  - `Forebay.TaskManager.Tui` — terminal UI

- **Performance** (Phase 0 benchmark, may have shifted since):
  - Worker cold start: ~107ms
  - Worker warm latency: ~88ms P95
  - KV read/write: <50ms P95

### Why This Stack?

Forebay went through Phase 0 viability testing across TypeScript, Rust, and Python workers. Rust was selected for performance (107ms cold start vs 195ms TypeScript, 824ms Python), latency (88ms P95 vs 142ms / 1489ms), and WASM support. .NET was selected for the client to get a single cross-platform CLI plus Avalonia desktop binaries from the same codebase.

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

### 2. Create KV Namespace

```bash
wrangler kv:namespace create forebay-queues
```

(A single namespace holds both queues and documents.)

### 3. Configure wrangler.toml

```toml
name = "forebay-worker"
main = "src/lib.rs"
compatibility_date = "2024-01-01"

[[kv_namespaces]]
binding = "QUEUES"
id = "your-queues-namespace-id"

[vars]
# Comma-separated key:email pairs. Each API key maps to a user
# email for per-user queue/document isolation.
API_KEYS = "key1:alice@example.com,key2:bob@example.com"
```

Generate strong keys however you like (`openssl rand -hex 32`, `uuidgen`, etc.). Treat the `API_KEYS` value as a secret — set it via `wrangler secret put API_KEYS` in production rather than committing it.

### 4. Deploy

```bash
cd worker
wrangler deploy
```

### 5. Configure CLI

```bash
forebay config set worker-url https://your-worker.workers.dev
forebay login <your-api-key>
```

## Troubleshooting

### Authentication Issues

**Problem**: `Unauthorized` / `Invalid API key` responses

**Solutions**:
- Verify the key is present in the worker's `API_KEYS` env var
- Re-run `forebay login <api-key>` to overwrite the saved value
- Inspect the saved config: `cat ~/.config/forebay/config.toml`
- Clear it and start over: `rm ~/.config/forebay/config.toml && forebay login <api-key>`

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


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [Cloudflare Workers](https://workers.cloudflare.com/) for global edge computing
- Rust Worker powered by [worker-rs](https://github.com/cloudflare/workers-rs)
- C# CLI built with [System.CommandLine](https://github.com/dotnet/command-line-api)
- Phase 0 viability testing informed architecture decisions
- Inspired by the need for simple, reliable cross-machine message transport

---

**Project Status**: Phase 1 in progress — queues + storage shipped, reference apps in early state

- ✅ Phase 0: Worker viability testing complete (Rust selected)
- ✅ Rust worker with FIFO queues, document storage, KV-indexed queue listing
- ✅ Static API-key authentication (Google OAuth scaffolding removed)
- ✅ .NET 9 client: `Forebay.Cli` (queue + storage commands) and `Forebay.Core` (HTTP wrapper, TOML config)
- ✅ Reference apps: `apps/Forebay.TaskManager.Avalonia` (desktop GUI) and `apps/Forebay.TaskManager.Tui` (terminal UI)
- ✅ Interactive demo script (`demo.sh`) covering all CLI features
- 🚧 90% test-coverage push
- ⏳ Integration tests and CI/CD pipeline
- ⏳ Binary release builds and signed installers

For detailed task breakdown, dependencies, and priorities, see:
- [Comprehensive Testing Plan](.idlergear/wiki/comprehensive-testing-plan-for-90-coverage.md)
- [Project Organization and Implementation Roadmap](.idlergear/wiki/project-organization-and-implementation-roadmap.md)

For more information, visit the [documentation](docs/) or [open an issue](https://github.com/marctjones/forebay/issues).
