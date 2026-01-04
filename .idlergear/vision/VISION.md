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

## Architecture Strategy
**Client-Server Model:**
- Client: CLI/GUI on user's machine (language TBD)
- Server: Cloudflare Worker (language TBD - testing phase)
- Storage: KV or D1
- Communication: HTTPS REST API

**Why Worker is necessary:**
- Atomic operations (read+delete for pull)
- Queue ordering (FIFO)
- Subscriptions (long-polling/WebSocket)
- Complex queue semantics (TTLs, priorities)

## Primary Backend
Cloudflare (via MCP) - free tier friendly, serverless

## Design Principles
- Abstract transport layer cleanly
- Easy to add new backends
- Not coupled to Cloudflare
- Test-driven development (80%+ coverage)
- Boring is a feature (skeptical engineering)
- Separate core library from interfaces
- Support Worker in different language than CLI

## Platform Targets
- CLI: Ubuntu, Windows 11, Android (future)
- GUI: Ubuntu, Windows 11 (Avalonia), Android (MAUI/Avalonia Mobile)
- Single binary distribution preferred

## Current Phase
**Phase 0: Worker Viability Testing**
Build automated test harness to compare TypeScript, Rust, and Python Workers before committing to language stack.

## Development Practices
- TDD: Write tests first, high coverage
- Git: Frequent atomic commits, conventional messages
- Idlergear: GitHub Issues/Projects/Wiki integration
- Data-driven decisions