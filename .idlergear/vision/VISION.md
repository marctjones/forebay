# Forebay - Cross-Platform Message Queue Transport System

## Vision
Create a universal message transport abstraction with pluggable backends - a "forebay" that buffers and delivers messages across machines and platforms.

## Core Value Proposition
A command-line tool (+ future GUIs) that lets users pipe messages to named queues and retrieve them later across different machines. Think universal message transport layer.

## Key Usage Pattern
```bash
echo "payload" | forebay push work/tasks
forebay pull work/tasks
forebay subscribe work/tasks | while read -r msg; do process "$msg"; done
```

## Architecture Decision ✅

**Phase 0 Complete:** Comprehensive viability testing performed across TypeScript, Rust, Python (multiple APIs), and .NET research.

**Selected Stack:**
- **Backend (Cloudflare Worker):** **Rust** - Best performance (107ms cold start, 88ms P95 latency)
- **Client (CLI/GUI/Mobile):** **C# with Avalonia** - Excellent cross-platform support
- **Communication:** HTTPS REST API between client and worker
- **Storage:** Cloudflare KV (queues) + D1 (metadata)

**Why This Works:**
- Perfect separation of concerns via REST API
- Use best language for each component
- Rust performance + C# developer experience
- Aligns with learning goals (Rust + .NET)

## Test Results Summary

| Worker     | Cold Start | P95 Latency | Status |
|------------|------------|-------------|--------|
| **Rust**   | **107ms** 🏆 | **88ms** 🏆 | ✅ Production Ready |
| TypeScript | 195ms      | 142ms       | ✅ Good Alternative |
| Python     | 824ms      | 1489ms      | ❌ Not Viable (beta) |
| .NET       | N/A        | N/A         | ❌ No Workers API |

See `worker-viability-tests/RESULTS_FINAL.md` for complete analysis.

## Design Principles
- Abstract transport layer cleanly
- Easy to add new backends
- Not coupled to Cloudflare
- Test-driven development (80%+ coverage)
- Boring is a feature (skeptical engineering)
- Separate core library from interfaces
- Data-driven decisions

## Platform Targets
- CLI: Ubuntu, Windows 11, Android (future)
- GUI: Ubuntu, Windows 11 (Avalonia), Android (MAUI/Avalonia Mobile)
- Single binary distribution preferred

## Current Phase
**Phase 1: Architecture Design and Implementation**
- Design Worker REST API endpoints
- Implement Rust Worker with authentication
- Create C# client library (Forebay.Core)
- Build CLI application (Forebay.Cli)
- Prepare for GUI (Forebay.Desktop with Avalonia)

## Development Practices
- TDD: Write tests first, high coverage
- Git: Frequent atomic commits, conventional messages
- Idlergear: GitHub Issues/Projects/Wiki integration
- Data-driven decisions