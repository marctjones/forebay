# Forebay API Documentation

This directory contains the complete API specification for the Forebay message queue system.

## OpenAPI Specification

The complete OpenAPI 3.0 specification is available in [`openapi.yaml`](openapi.yaml).

### Viewing the Spec

**Online viewers:**
- [Swagger Editor](https://editor.swagger.io/) - Paste the YAML content
- [Redoc](https://redocly.github.io/redoc/) - Beautiful API documentation

**Local viewing:**
```bash
# Using npx and Swagger UI
npx @redocly/cli preview-docs docs/api/openapi.yaml
```

## Authentication

All API endpoints (except `/health` and `/auth/login`) require authentication.

### OAuth Flow

1. **Obtain Google ID Token**: Use Google OAuth 2.0 to get an ID token
2. **Login**: POST to `/auth/login` with the ID token
3. **Receive Session Token**: Save the returned session token
4. **Make Requests**: Include session token in Authorization header

### Authorization Header Format

```http
Authorization: Bearer <session-token>
```

Example:
```http
GET /auth/whoami HTTP/1.1
Host: forebay-worker.example.workers.dev
Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000
```

### Session Expiration

- Sessions expire after 30 days
- Re-authenticate when receiving 401 Unauthorized responses

## API Endpoints

### Health

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/health` | Service health check | No |

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/login` | Login with Google ID token | No |
| GET | `/auth/whoami` | Get current user info | Yes |
| POST | `/auth/logout` | End current session | Yes |

### Queue Operations

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/queues/{queue}/push` | Add message to queue | Yes |
| POST | `/queues/{queue}/pull` | Retrieve message from queue | Yes |
| GET | `/queues/{queue}/stats` | Get queue statistics | Yes |
| DELETE | `/queues/{queue}` | Delete queue | Yes |
| GET | `/queues` | List all queues | Yes |

## Data Models

### Queue Item

Messages in the queue have the following structure:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "payload": {
    "task": "process_data",
    "priority": "high"
  },
  "pushed_at": 1609459200000
}
```

### Queue Metadata

Queue statistics include:

```json
{
  "queue_name": "work/tasks",
  "size": 42,
  "total_pushed": 1000,
  "total_pulled": 958
}
```

### Error Response Format

All errors follow this structure:

```json
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Invalid or expired session token",
    "details": "Session token not found in KV store"
  }
}
```

**Error Codes:**
- `UNAUTHORIZED` - Invalid or expired session token
- `BAD_REQUEST` - Invalid request format or parameters
- `NOT_FOUND` - Resource not found (queue, message)
- `INTERNAL_ERROR` - Server error

## Request/Response Examples

### 1. Health Check

**Request:**
```bash
curl https://forebay-worker.example.workers.dev/health
```

**Response:**
```json
{
  "status": "ok",
  "timestamp": 1609459200000
}
```

### 2. Login

**Request:**
```bash
curl -X POST https://forebay-worker.example.workers.dev/auth/login \
  -H "Content-Type: application/json" \
  -d '{"id_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjFlOWdkazcifQ..."}'
```

**Response:**
```json
{
  "session_token": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "expires_at": 1612137600000
}
```

### 3. Get Current User

**Request:**
```bash
curl https://forebay-worker.example.workers.dev/auth/whoami \
  -H "Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "email": "user@example.com",
  "created_at": 1609459200000,
  "expires_at": 1612137600000
}
```

### 4. Push to Queue

**Request:**
```bash
curl -X POST https://forebay-worker.example.workers.dev/queues/work/tasks/push \
  -H "Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000" \
  -H "Content-Type: application/json" \
  -d '{"payload": {"task": "process_data", "priority": "high"}}'
```

**Response:**
```json
{
  "id": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
  "queue_name": "work/tasks"
}
```

### 5. Pull from Queue

**Request:**
```bash
curl -X POST https://forebay-worker.example.workers.dev/queues/work/tasks/pull \
  -H "Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "id": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
  "payload": {
    "task": "process_data",
    "priority": "high"
  },
  "pushed_at": 1609459200000
}
```

**Empty Queue Response (404):**
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Queue not found or empty"
  }
}
```

### 6. Get Queue Stats

**Request:**
```bash
curl https://forebay-worker.example.workers.dev/queues/work/tasks/stats \
  -H "Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "queue_name": "work/tasks",
  "size": 42,
  "total_pushed": 1000,
  "total_pulled": 958
}
```

### 7. List Queues

**Request:**
```bash
curl https://forebay-worker.example.workers.dev/queues \
  -H "Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "queues": [
    {
      "name": "work/tasks",
      "size": 42
    },
    {
      "name": "notifications/urgent",
      "size": 3
    },
    {
      "name": "batch/processing",
      "size": 0
    }
  ]
}
```

### 8. Delete Queue

**Request:**
```bash
curl -X DELETE https://forebay-worker.example.workers.dev/queues/work/tasks \
  -H "Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "queue_name": "work/tasks",
  "message": "Queue deleted successfully"
}
```

### 9. Logout

**Request:**
```bash
curl -X POST https://forebay-worker.example.workers.dev/auth/logout \
  -H "Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "message": "Logged out successfully"
}
```

## Client Libraries

### JavaScript/TypeScript

```typescript
// Using fetch API
const WORKER_URL = 'https://forebay-worker.example.workers.dev';
let sessionToken: string;

// Login
async function login(idToken: string) {
  const response = await fetch(`${WORKER_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id_token: idToken })
  });
  const data = await response.json();
  sessionToken = data.session_token;
  return data;
}

// Push to queue
async function push(queue: string, payload: any) {
  const response = await fetch(`${WORKER_URL}/queues/${queue}/push`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${sessionToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ payload })
  });
  return response.json();
}

// Pull from queue
async function pull(queue: string) {
  const response = await fetch(`${WORKER_URL}/queues/${queue}/pull`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${sessionToken}` }
  });

  if (!response.ok) {
    throw new Error(`Pull failed: ${response.status}`);
  }

  return response.json();
}

// Usage
await login('google-id-token');
await push('work/tasks', { task: 'process', priority: 'high' });
const message = await pull('work/tasks');
console.log(message.payload);
```

### Python

```python
import requests
import json

WORKER_URL = 'https://forebay-worker.example.workers.dev'
session_token = None

# Login
def login(id_token):
    global session_token
    response = requests.post(
        f'{WORKER_URL}/auth/login',
        json={'id_token': id_token}
    )
    response.raise_for_status()
    data = response.json()
    session_token = data['session_token']
    return data

# Push to queue
def push(queue, payload):
    response = requests.post(
        f'{WORKER_URL}/queues/{queue}/push',
        headers={'Authorization': f'Bearer {session_token}'},
        json={'payload': payload}
    )
    response.raise_for_status()
    return response.json()

# Pull from queue
def pull(queue):
    response = requests.post(
        f'{WORKER_URL}/queues/{queue}/pull',
        headers={'Authorization': f'Bearer {session_token}'}
    )

    if response.status_code == 404:
        return None  # Queue empty

    response.raise_for_status()
    return response.json()

# Usage
login('google-id-token')
push('work/tasks', {'task': 'process', 'priority': 'high'})
message = pull('work/tasks')
if message:
    print(message['payload'])
```

### C# (using Forebay.Core)

```csharp
using Forebay.Core;
using System.Text.Json;

var client = new ForebayClient("https://forebay-worker.example.workers.dev");

// Login
var loginResponse = await client.LoginAsync("google-id-token");
Console.WriteLine($"Logged in as: {loginResponse.Email}");

// Push to queue
var payload = JsonDocument.Parse("{\"task\": \"process\", \"priority\": \"high\"}").RootElement;
var pushResponse = await client.PushAsync("work/tasks", payload);
Console.WriteLine($"Pushed message: {pushResponse.Id}");

// Pull from queue
var pullResponse = await client.PullAsync("work/tasks");
Console.WriteLine($"Pulled: {pullResponse.Payload}");

// Get stats
var stats = await client.StatsAsync("work/tasks");
Console.WriteLine($"Queue size: {stats.Size}, Total pushed: {stats.TotalPushed}");

// Logout
await client.LogoutAsync();
```

## Best Practices

### Error Handling

Always check HTTP status codes and handle errors appropriately:

```typescript
try {
  const response = await fetch(url, options);

  if (!response.ok) {
    const error = await response.json();
    console.error(`API Error: ${error.error.code} - ${error.error.message}`);

    if (response.status === 401) {
      // Re-authenticate
      await login();
    }

    throw new Error(error.error.message);
  }

  return await response.json();
} catch (err) {
  console.error('Request failed:', err);
  throw err;
}
```

### Retry Logic

Implement exponential backoff for transient failures:

```typescript
async function pullWithRetry(queue: string, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await pull(queue);
    } catch (err) {
      if (i === maxRetries - 1) throw err;

      // Exponential backoff: 1s, 2s, 4s
      await new Promise(r => setTimeout(r, 1000 * Math.pow(2, i)));
    }
  }
}
```

### Session Token Storage

Store session tokens securely:

- **CLI**: Store in `~/.config/forebay/config.json` with 0600 permissions
- **Web**: Use httpOnly cookies or secure localStorage
- **Mobile**: Use platform-specific secure storage (Keychain, KeyStore)

### Queue Naming

Follow these conventions for queue names:

- Use hierarchical naming: `domain/subdomain/queue`
- Examples:
  - `work/tasks` - Work-related tasks
  - `users/alice/notifications` - User-specific queues
  - `batch/processing/high-priority` - Batch jobs with priority
- Maximum length: 256 characters
- Allowed characters: `a-z A-Z 0-9 / - _`

### Payload Size

- Keep payloads reasonably sized (< 1MB recommended)
- For large data, store in external storage and pass references
- Example:
  ```json
  {
    "type": "large_dataset",
    "s3_url": "s3://bucket/data.csv",
    "size_bytes": 52428800
  }
  ```

### Rate Limiting

While not currently enforced, follow these guidelines:

- Maximum 100 requests per second per user
- Batch operations when possible
- Use queue stats to avoid unnecessary pulls

## Pagination

Currently not implemented, but planned for future versions:

```json
{
  "queues": [...],
  "pagination": {
    "total": 150,
    "page": 1,
    "per_page": 50,
    "next_cursor": "abc123"
  }
}
```

## WebSocket Support

Planned for future versions - real-time queue subscriptions:

```typescript
// Future API (not yet implemented)
const ws = new WebSocket('wss://forebay-worker.example.workers.dev/subscribe?queue=work/tasks&token=...');

ws.onmessage = (event) => {
  const message = JSON.parse(event.data);
  console.log('New message:', message.payload);
};
```

## Changelog

See [CHANGELOG.md](../../CHANGELOG.md) for API version history and breaking changes.

## Support

- GitHub Issues: https://github.com/yourusername/forebay/issues
- Documentation: https://github.com/yourusername/forebay/tree/main/docs
- OpenAPI Spec: [openapi.yaml](openapi.yaml)
