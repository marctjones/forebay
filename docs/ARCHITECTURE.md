# Forebay Architecture

This document describes the architecture, design decisions, and technical details of the Forebay message queue system.

## Table of Contents

- [System Overview](#system-overview)
- [Component Architecture](#component-architecture)
- [Data Flow](#data-flow)
- [Technology Stack](#technology-stack)
- [Design Decisions](#design-decisions)
- [Security](#security)
- [Performance](#performance)
- [Future Enhancements](#future-enhancements)

## System Overview

Forebay is a cross-platform message queue system consisting of:

1. **Rust Cloudflare Worker**: Serverless backend handling HTTP requests, authentication, and queue operations
2. **C# CLI Client**: Cross-platform command-line interface for queue interactions
3. **Cloudflare KV**: Persistent storage for sessions and queue data

```
┌─────────────────────────────────────────────────────────────┐
│                         User                                │
│                           │                                 │
│                           ▼                                 │
│                    ┌─────────────┐                         │
│                    │ Forebay CLI │  (C# .NET 9.0)          │
│                    │  (Client)   │                         │
│                    └─────────────┘                         │
│                           │                                 │
│                           │ HTTPS REST API                  │
│                           │ (JSON)                          │
│                           ▼                                 │
│              ┌───────────────────────┐                     │
│              │  Cloudflare Worker    │  (Rust + WASM)      │
│              │  ┌─────────────────┐  │                     │
│              │  │     Router      │  │                     │
│              │  └────────┬────────┘  │                     │
│              │           │            │                     │
│              │  ┌────────┴────────┐  │                     │
│              │  │                 │  │                     │
│              │  │  ┌──────────┐  ┌┴──────────┐           │
│              │  │  │   Auth   │  │   Queue   │           │
│              │  │  │ Module   │  │  Module   │           │
│              │  │  └──────────┘  └───────────┘           │
│              │  │                                         │
│              │  └───────┬─────────────────────┬──────────┘│
│              └──────────┼─────────────────────┼───────────┘
│                         │                     │             │
│                         ▼                     ▼             │
│              ┌──────────────────┐  ┌──────────────────┐   │
│              │ Cloudflare KV    │  │ Cloudflare KV    │   │
│              │  (SESSIONS)      │  │   (QUEUES)       │   │
│              │                  │  │                  │   │
│              │ session:uuid →   │  │ queue:name →     │   │
│              │   SessionData    │  │   QueueData      │   │
│              └──────────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Component Architecture

### Worker (Rust)

#### Router (`lib.rs`)

Main entry point that routes HTTP requests to appropriate handlers:

```rust
Router::new()
    .get("/health", |_, _| Response::ok("OK"))
    .post_async("/auth/login", auth::handle_login)
    .get_async("/auth/whoami", auth::handle_whoami)
    .post_async("/auth/logout", auth::handle_logout)
    .post_async("/queues/:queue/push", queue::handle_push)
    .post_async("/queues/:queue/pull", queue::handle_pull)
    .get_async("/queues/:queue/stats", queue::handle_stats)
    .delete_async("/queues/:queue", queue::handle_delete)
    .get_async("/queues", queue::handle_list)
```

**Responsibilities:**
- Parse incoming HTTP requests
- Route to appropriate handler
- CORS handling (if needed)
- Global error handling

#### Authentication Module (`auth.rs`)

Handles Google OAuth authentication and session management:

```rust
// Key Functions:
- handle_login(req, ctx)      // Verify Google ID token, create session
- handle_whoami(req, ctx)     // Return current user info
- handle_logout(req, ctx)     // Delete session
- get_session(req, env)       // Validate session token
- extract_token(req)          // Extract Bearer token from header
- verify_google_token(token)  // Verify Google JWT signature
```

**Session Management:**
```rust
// Session stored in KV as:
Key: "session:{uuid}"
Value: {
    "email": "user@example.com",
    "created_at": 1609459200000,
    "expires_at": 1612137600000
}
TTL: 30 days (2592000 seconds)
```

**Authentication Flow:**
1. Client obtains Google ID token via OAuth
2. Client POSTs token to `/auth/login`
3. Worker verifies JWT signature using Google JWKS
4. Worker checks email against allowlist
5. Worker generates UUID session token
6. Worker stores session in KV with 30-day TTL
7. Worker returns session token to client

#### Queue Module (`queue.rs`)

Implements FIFO queue operations:

```rust
// Key Functions:
- handle_push(req, ctx)   // Add item to queue
- handle_pull(req, ctx)   // Remove and return first item
- handle_stats(req, ctx)  // Return queue statistics
- handle_delete(req, ctx) // Delete entire queue
- handle_list(req, ctx)   // List all queues (stub)
```

**Queue Data Structure:**
```rust
// Queue stored in KV as:
Key: "queue:{name}"
Value: {
    "items": [
        {
            "id": "uuid-1",
            "payload": {"task": "process"},
            "pushed_at": 1609459200000
        },
        {
            "id": "uuid-2",
            "payload": {"task": "backup"},
            "pushed_at": 1609459201000
        }
    ],
    "total_pushed": 1000,
    "total_pulled": 958
}
```

**FIFO Implementation:**
- Items stored as Vec in JSON
- Push: Append to end (`items.push()`)
- Pull: Remove from start (`items.remove(0)`)
- Guarantees ordering preservation

#### Models (`models.rs`)

Data structures for API requests/responses:

```rust
// Authentication
LoginRequest { id_token: String }
LoginResponse { session_token, email, expires_at }
WhoamiResponse { email, created_at, expires_at }

// Queue Operations
PushRequest { payload: Value }
PushResponse { id, queue_name }
PullResponse { id, payload, pushed_at }
StatsResponse { queue_name, size, total_pushed, total_pulled }

// Internal
SessionData { email, created_at, expires_at }
QueueData { items: Vec<QueueItem>, total_pushed, total_pulled }
QueueItem { id, payload, pushed_at }
```

#### Error Handling (`error.rs`)

Standardized error responses:

```rust
ErrorResponse {
    error: ErrorDetail {
        code: String,     // UNAUTHORIZED, BAD_REQUEST, NOT_FOUND, etc.
        message: String,  // Human-readable message
        details: Option<String>  // Additional context
    }
}

// Helper methods:
ErrorResponse::unauthorized("Invalid session")
ErrorResponse::bad_request("Invalid JSON")
ErrorResponse::not_found("Queue not found")
ErrorResponse::internal_error("KV error")
```

### Client (C#)

#### Forebay.Core Library

**ForebayClient (`ForebayClient.cs`)**

Main API client wrapping HTTP operations:

```csharp
public class ForebayClient
{
    private readonly HttpClient _httpClient;
    private string? _sessionToken;

    // Authentication
    public Task<LoginResponse> LoginAsync(string idToken)
    public Task<WhoamiResponse> WhoAmIAsync()
    public Task LogoutAsync()

    // Queue Operations
    public Task<PushResponse> PushAsync(string queue, JsonElement payload)
    public Task<PullResponse> PullAsync(string queue)
    public Task<StatsResponse> StatsAsync(string queue)
    public Task<DeleteResponse> DeleteQueueAsync(string queue)
    public Task<ListQueuesResponse> ListQueuesAsync()

    // Session Management
    public void SetSessionToken(string token)
    public string? GetSessionToken()
}
```

**Error Handling:**
```csharp
// Custom exception for API errors
public class ForebayApiException : Exception
{
    public string Code { get; }
    public string? Details { get; }
}

// Thrown when:
// - HTTP request fails
// - API returns error response
// - Deserialization fails
```

#### Forebay.Cli Application

**Command Structure:**

```csharp
// Program.cs - Entry point
RootCommand rootCommand = new("Forebay CLI");

// Auth commands
Command loginCmd = new("login", "Authenticate with Google");
Command logoutCmd = new("logout", "End current session");
Command whoamiCmd = new("whoami", "Show current user");

// Queue commands
Command pushCmd = new("push", "Add message to queue");
Command pullCmd = new("pull", "Retrieve message from queue");
Command statsCmd = new("stats", "Show queue statistics");
Command listCmd = new("list", "List all queues");
Command deleteCmd = new("delete", "Delete queue");

rootCommand.AddCommand(loginCmd);
// ... etc
```

**Configuration Management:**

```csharp
// ConfigManager.cs
public class ConfigManager
{
    // Config file: ~/.config/forebay/config.json (Linux/macOS)
    //             %APPDATA%\forebay\config.json (Windows)

    public string? WorkerUrl { get; set; }
    public string? SessionToken { get; set; }

    public void Save()
    public static ConfigManager Load()
}
```

**Stored as JSON:**
```json
{
  "workerUrl": "https://forebay-worker.example.workers.dev",
  "sessionToken": "550e8400-e29b-41d4-a716-446655440000"
}
```

## Data Flow

### Push Message Flow

```
1. User runs: forebay push work/tasks '{"task":"process"}'

2. CLI (Forebay.Cli)
   ├─ Parse command arguments
   ├─ Load config (session token)
   └─ Call ForebayClient.PushAsync()

3. Client (Forebay.Core)
   ├─ Create PushRequest JSON
   ├─ Add Authorization header
   ├─ POST /queues/work/tasks/push
   └─ Parse PushResponse

4. Worker (Rust)
   ├─ Router: POST /queues/:queue/push → queue::handle_push
   ├─ Auth: Extract and validate session token
   ├─ Queue: Parse request body
   ├─ Queue: Load queue from KV
   ├─ Queue: Append new item with UUID
   ├─ Queue: Update metadata (total_pushed++)
   ├─ Queue: Save to KV
   └─ Return: PushResponse { id, queue_name }

5. KV Storage
   ├─ Key: "queue:work/tasks"
   └─ Value: Updated QueueData with new item

6. Response propagates back to user
   ├─ Worker → Client → CLI
   └─ CLI displays: "Pushed message {id} to work/tasks"
```

### Pull Message Flow

```
1. User runs: forebay pull work/tasks

2. CLI → Client → Worker
   ├─ POST /queues/work/tasks/pull
   └─ Authorization: Bearer {token}

3. Worker (queue::handle_pull)
   ├─ Validate session
   ├─ Load queue from KV
   ├─ Check if queue exists and has items
   │  ├─ If empty: Return 404 NOT_FOUND
   │  └─ If has items: Continue
   ├─ Remove first item (FIFO)
   ├─ Update metadata (total_pulled++)
   ├─ Save updated queue to KV
   └─ Return: PullResponse { id, payload, pushed_at }

4. Client deserializes response

5. CLI displays payload to stdout
```

## Technology Stack

### Backend (Cloudflare Worker)

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| Language | Rust | Best performance, type safety, WASM support |
| Framework | worker-rs | Official Cloudflare Workers SDK for Rust |
| Runtime | V8 (via WASM) | Cloudflare Workers runtime |
| Storage | KV | Globally distributed key-value store |
| Auth | Google OAuth 2.0 | Leverages existing Google accounts |
| Serialization | serde_json | Industry standard JSON library |

**Key Dependencies:**
```toml
[dependencies]
worker = "0.0.18"
serde = { version = "1.0", features = ["derive"] }
serde_json = "1.0"
uuid = { version = "1.0", features = ["v4"] }
```

### Frontend (CLI Client)

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| Language | C# | Cross-platform, excellent CLI support |
| Framework | .NET 9.0 | Latest LTS, single-file publishing |
| CLI | System.CommandLine | Modern, type-safe CLI framework |
| HTTP | HttpClient | Built-in, async, robust |
| Serialization | System.Text.Json | High performance, built-in |

**Key Dependencies:**
```xml
<PackageReference Include="System.CommandLine" Version="2.0.0" />
<PackageReference Include="System.Text.Json" Version="9.0.0" />
```

## Design Decisions

### Why Rust for the Worker?

**Evaluated: TypeScript, Rust, Python, .NET**

**Phase 0 Benchmark Results:**

| Metric | TypeScript | Rust | Python |
|--------|-----------|------|--------|
| Cold Start | 195ms | **107ms** | 824ms |
| P95 Latency | 142ms | **88ms** | 1489ms |
| Developer Experience | Excellent | Good | Excellent |

**Decision: Rust**

Reasons:
- ✅ Best performance (50% faster cold start than TypeScript)
- ✅ Lowest latency (40% better P95 than TypeScript)
- ✅ Type safety prevents runtime errors
- ✅ Excellent WASM compilation
- ✅ Small binary size
- ❌ Steeper learning curve (acceptable tradeoff)

### Why C# for the Client?

**Evaluated: Rust, Go, C#, TypeScript**

**Decision: C# with .NET 9.0**

Reasons:
- ✅ Excellent cross-platform support (Linux, Windows, macOS, Android)
- ✅ Single-file binary publishing
- ✅ Future GUI support with Avalonia
- ✅ System.CommandLine for modern CLI
- ✅ Strong typing and IntelliSense
- ✅ Aligns with learning goals
- ❌ Larger binary than Go/Rust (acceptable)

### Why KV over D1 (SQLite)?

**Evaluated: Cloudflare KV, Cloudflare D1, External DB**

**Decision: KV for sessions and queues**

Reasons for KV:
- ✅ Simpler API (get/put/delete)
- ✅ Eventually consistent is acceptable for queues
- ✅ No schema migrations needed
- ✅ Lower cost for small data
- ✅ Built-in TTL for sessions
- ❌ No complex queries (not needed for queues)

Future: May add D1 for metadata/analytics if needed

### FIFO Implementation with Vec

**Alternative: Use KV list operations**

**Decision: Store entire queue as Vec in JSON**

Reasons:
- ✅ Simple implementation
- ✅ Guaranteed ordering
- ✅ Single KV read/write per operation
- ✅ Works for queue sizes < 10,000 items
- ❌ Not optimized for very large queues (future enhancement)

Future: Implement chunked storage for large queues

### Session Token: UUID vs JWT

**Evaluated: JWT, UUID + KV lookup**

**Decision: UUID stored in KV**

Reasons:
- ✅ Can invalidate sessions (logout)
- ✅ Simpler client implementation
- ✅ Smaller tokens
- ✅ Can update session metadata without re-issuing
- ❌ Extra KV read on each request (acceptable latency)

## Security

### Authentication

**Google OAuth 2.0 with JWT Verification:**

1. Client obtains ID token from Google
2. Worker verifies JWT signature using Google's JWKS
3. Worker validates token claims:
   - `iss`: Must be `accounts.google.com` or `https://accounts.google.com`
   - `aud`: Must match configured client ID
   - `exp`: Token not expired
   - `email_verified`: Must be true
4. Worker checks email against allowlist
5. Worker creates session token (UUID v4)

**Session Storage:**
- Session tokens: Random UUIDv4 (122 bits entropy)
- Stored in KV with 30-day TTL
- Automatic expiration via KV TTL

### Authorization

**Bearer Token Authentication:**

```http
Authorization: Bearer <session-token>
```

All endpoints except `/health` and `/auth/login` require valid session token.

**Authorization Flow:**
1. Extract token from Authorization header
2. Look up session in KV
3. Check if session exists and not expired
4. Return 401 if invalid/expired

### Email Allowlist

Environment variable controls access:

```toml
# wrangler.toml
[vars]
ALLOWED_EMAILS = "user1@example.com,user2@example.com,admin@example.com"
```

Only users with allowed emails can create sessions.

### Data Isolation

Queues are not user-scoped in current implementation:
- All authenticated users can access all queues
- Future: Add user-scoped queues (`user:{email}:queue:{name}`)

### Transport Security

- All communication over HTTPS
- TLS 1.3 enforced by Cloudflare
- No sensitive data in URLs

## Performance

### Benchmarks

**Worker Performance (from Phase 0):**
- Cold start: 107ms (P50)
- Warm latency: 88ms (P95)
- KV read: <20ms (P95)
- KV write: <50ms (P95)

**Total Request Latency:**
```
User Request → Worker (88ms) → KV (20ms) → Worker (10ms) → Response
Total: ~120ms P95
```

### Optimization Techniques

**Worker:**
- Minimize KV operations (single read/write per request)
- Use async/await properly
- Avoid unnecessary allocations
- Cache Google JWKS keys (TODO)

**Client:**
- Reuse HttpClient instance
- Connection pooling
- Keep session token in memory
- Minimal config file I/O

### Scalability

**Current Limitations:**
- Queue size: Practical limit ~10,000 items (JSON serialization)
- Concurrent requests: Limited by Worker concurrent request limit
- KV operations: 1,000 reads/sec, 1,000 writes/sec per key

**Future Improvements:**
- Chunked queue storage for large queues
- Queue sharding for high throughput
- Caching layer for frequently accessed queues

## Future Enhancements

### Phase 2: Real-time Subscriptions

Add WebSocket support for queue subscriptions:

```typescript
// Future API
const ws = new WebSocket('wss://worker.url/subscribe?queue=work/tasks&token=...');

ws.onmessage = (event) => {
  const message = JSON.parse(event.data);
  console.log('New message:', message);
};
```

**Implementation:**
- Use Cloudflare Durable Objects for WebSocket state
- Pub/sub pattern for queue notifications
- CLI: `forebay subscribe work/tasks | while read msg; do process; done`

### Phase 3: GUI Applications

Avalonia-based desktop and mobile apps:

- Ubuntu Desktop (GNOME)
- Windows 11 Desktop
- Android (Avalonia Mobile or MAUI)

**Shared Core:**
- Forebay.Core library (same as CLI)
- Shared business logic
- Platform-specific UI

### Phase 4: Advanced Queue Features

**Priority Queues:**
```json
{
  "payload": {...},
  "priority": 10  // Higher = processed first
}
```

**Message Filtering:**
```bash
forebay pull work/tasks --filter '.priority > 5'
```

**Dead Letter Queues:**
- Auto-retry failed messages
- Move to DLQ after N failures

**Queue Analytics:**
- Message throughput graphs
- Latency histograms
- Queue depth over time

### Phase 5: Additional Backends

Make transport layer pluggable:

```csharp
// Future: Plugin architecture
IQueueBackend backend = config.Backend switch {
    "cloudflare" => new CloudflareBackend(),
    "aws-sqs" => new SqsBackend(),
    "rabbitmq" => new RabbitMqBackend(),
    "redis" => new RedisBackend(),
    _ => throw new NotSupportedException()
};
```

Backends to support:
- AWS SQS
- Google Cloud Pub/Sub
- Azure Service Bus
- RabbitMQ
- Redis Streams

### Phase 6: Multi-tenancy

User-scoped queues:

```
Key: "queue:user:{email}:name"
```

Shared queues with ACLs:
```json
{
  "queue": "shared/notifications",
  "acl": {
    "readers": ["user1@example.com"],
    "writers": ["user2@example.com"],
    "admins": ["admin@example.com"]
  }
}
```

## Testing Architecture

See [DEVELOPMENT.md](DEVELOPMENT.md#testing-strategy) for full testing strategy.

**Current Coverage:**
- Rust Worker: 25 unit tests (models, auth, queue logic)
- C# Client: 26 unit tests (models, client methods)
- Integration: Planned (task #41)
- E2E: Planned (task #42)

**Test Pyramid:**
```
         ┌───────────┐
         │  E2E (2)  │  Full system, real Worker
         └───────────┘
       ┌───────────────┐
       │ Integration   │  HTTP tests against deployed Worker
       │     (10)      │
       └───────────────┘
    ┌────────────────────┐
    │   Unit Tests       │  Pure logic, mocked dependencies
    │      (51)          │
    └────────────────────┘
```

---

For questions or clarifications, open a GitHub Issue or Discussion.
