---
id: 15
title: Set up Rust Worker project structure
state: open
created: '2026-01-04T08:32:48.694013Z'
labels:
- phase-1
- rust
- worker
- setup
priority: high
---
Initialize the production Rust Worker for Forebay.

**Project structure:**
```
worker/
├── src/
│   ├── lib.rs          # Entry point, router
│   ├── auth.rs         # Authentication logic
│   ├── queue.rs        # Queue operations
│   ├── models.rs       # Data models (Request/Response types)
│   └── error.rs        # Error handling
├── Cargo.toml          # Dependencies
├── wrangler.toml       # Cloudflare configuration
└── tests/
    └── integration.rs  # Integration tests
```

**Dependencies needed:**
- `worker = "0.7.2"` - Cloudflare Workers runtime
- `serde = { version = "1.0", features = ["derive"] }`
- `serde_json = "1.0"`
- `chrono = "0.4"` - Timestamps
- `uuid = { version = "1.0", features = ["v4"] }` - Session tokens

**Configuration:**
- Worker name: `forebay-worker`
- KV namespaces: `QUEUE_DATA`, `SESSION_TOKENS`
- Environment variables: `ALLOWED_EMAILS`, `GOOGLE_CLIENT_ID`

**Initial implementation:**
- Basic router with /health endpoint
- Verify deployment works
- Add wrangler dev for local testing
