---
id: 1
title: API Specification
created: '2026-01-04T08:37:04.011156Z'
updated: '2026-01-04T08:37:04.011170Z'
---
# Forebay Worker REST API Specification

**Version:** 1.0  
**Base URL:** `https://forebay-worker.{account}.workers.dev`  
**Authentication:** Bearer token in `Authorization` header

---

## Authentication Endpoints

### POST /auth/login

Authenticate with Google OAuth and receive a session token.

**Authentication:** None (public endpoint)

**Request Body:**
```json
{
  "id_token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjE..."
}
```

**Response (200 OK):**
```json
{
  "session_token": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "expires_at": 1704412800000
}
```

**Error Responses:**
- `400 Bad Request` - Missing or invalid id_token
- `401 Unauthorized` - Invalid Google token or email not allowed
- `500 Internal Server Error` - Server error

**Notes:**
- ID token must be a valid Google ID token (JWT)
- Email must be in ALLOWED_EMAILS environment variable
- Session token valid for 30 days (configurable)
- Token stored in SESSION_TOKENS KV namespace

---

### GET /auth/whoami

Get information about the current authenticated session.

**Authentication:** Required (session token)

**Request Body:** None

**Response (200 OK):**
```json
{
  "email": "user@example.com",
  "expires_at": 1704412800000
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid session token

---

### POST /auth/logout

Invalidate the current session token.

**Authentication:** Required (session token)

**Request Body:** None

**Response (200 OK):**
```json
{
  "success": true
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid session token

---

## Queue Endpoints

All queue endpoints require authentication via session token.

### POST /q/:name/push

Push an item to a queue.

**Authentication:** Required

**URL Parameters:**
- `name` - Queue name (alphanumeric, hyphens, underscores)

**Request Body:**
```json
{
  "payload": { 
    "any": "json",
    "data": "here"
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "queue": "work/tasks",
  "length": 5,
  "item_id": "01HQXYZ123456789ABCDEF",
  "timestamp": 1704412800000
}
```

**Error Responses:**
- `400 Bad Request` - Missing or invalid payload
- `401 Unauthorized` - Missing or invalid session token
- `413 Payload Too Large` - Payload exceeds size limit
- `500 Internal Server Error` - Server error

**Notes:**
- Queues are created automatically on first push
- Queue key format: `queue:{email}:{name}`
- Maximum payload size: 25MB (KV limit)
- Items stored with timestamp and unique ID

---

### GET /q/:name/pull

Pull (dequeue) the oldest item from a queue.

**Authentication:** Required

**URL Parameters:**
- `name` - Queue name

**Request Body:** None

**Response (200 OK):**
```json
{
  "item_id": "01HQXYZ123456789ABCDEF",
  "payload": {
    "any": "json",
    "data": "here"
  },
  "timestamp": 1704412800000,
  "remaining": 4
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid session token
- `404 Not Found` - Queue not found or empty
- `500 Internal Server Error` - Server error

**Notes:**
- Operation is atomic (read + delete)
- FIFO ordering guaranteed
- Returns 404 if queue is empty

---

### GET /q/:name/stats

Get statistics about a queue without modifying it.

**Authentication:** Required

**URL Parameters:**
- `name` - Queue name

**Request Body:** None

**Response (200 OK):**
```json
{
  "queue": "work/tasks",
  "length": 5,
  "oldest_timestamp": 1704412800000,
  "newest_timestamp": 1704413000000,
  "total_size_bytes": 1024
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid session token
- `404 Not Found` - Queue not found
- `500 Internal Server Error` - Server error

**Notes:**
- Does not dequeue items
- Oldest timestamp is from first item in queue
- Newest timestamp is from last item in queue

---

### GET /q/:name/subscribe

Long-polling endpoint for queue subscriptions (future implementation).

**Authentication:** Required

**URL Parameters:**
- `name` - Queue name

**Query Parameters:**
- `timeout` - Max wait time in seconds (default: 30, max: 60)

**Request Body:** None

**Response (200 OK):**
```json
{
  "item_id": "01HQXYZ123456789ABCDEF",
  "payload": {
    "any": "json",
    "data": "here"
  },
  "timestamp": 1704412800000
}
```

**Response (204 No Content):**
Returned if timeout reached with no new items.

**Error Responses:**
- `401 Unauthorized` - Missing or invalid session token
- `404 Not Found` - Queue not found
- `500 Internal Server Error` - Server error

**Notes:**
- Waits for new items to arrive
- Returns immediately if items already in queue
- Client should reconnect on success or timeout
- Phase 1 may use simple polling instead

---

### DELETE /q/:name

Delete a queue and all its items.

**Authentication:** Required

**URL Parameters:**
- `name` - Queue name

**Request Body:** None

**Response (200 OK):**
```json
{
  "success": true,
  "queue": "work/tasks",
  "deleted_items": 5
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid session token
- `404 Not Found` - Queue not found
- `500 Internal Server Error` - Server error

**Notes:**
- Deletes all items in the queue
- Operation is irreversible

---

### GET /queues

List all queues for the authenticated user.

**Authentication:** Required

**Request Body:** None

**Response (200 OK):**
```json
{
  "queues": [
    {
      "name": "work/tasks",
      "length": 5,
      "oldest_timestamp": 1704412800000
    },
    {
      "name": "notifications",
      "length": 2,
      "oldest_timestamp": 1704412900000
    }
  ]
}
```

**Error Responses:**
- `401 Unauthorized` - Missing or invalid session token
- `500 Internal Server Error` - Server error

**Notes:**
- Only returns queues owned by authenticated user
- Empty array if no queues exist
- Results may be paginated in future versions

---

## Health & Utility Endpoints

### GET /health

Health check endpoint (no authentication required).

**Authentication:** None

**Request Body:** None

**Response (200 OK):**
```json
{
  "status": "ok",
  "version": "1.0.0",
  "timestamp": 1704412800000
}
```

---

## Common Error Response Format

All error responses follow this format:

```json
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Invalid session token",
    "details": "Token expired or not found"
  }
}
```

**Error Codes:**
- `UNAUTHORIZED` - Authentication failure
- `FORBIDDEN` - Not allowed to access resource
- `NOT_FOUND` - Resource not found
- `BAD_REQUEST` - Invalid request data
- `PAYLOAD_TOO_LARGE` - Request too large
- `INTERNAL_ERROR` - Server error

---

## Authentication Header Format

All authenticated requests must include:

```
Authorization: Bearer {session_token}
```

Example:
```
Authorization: Bearer 550e8400-e29b-41d4-a716-446655440000
```

---

## Rate Limiting

**Phase 1:** No rate limiting implemented

**Future:** Rate limiting per user:
- 1000 requests per minute
- 10,000 requests per hour
- Headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`

---

## Data Limits

- **Maximum payload size:** 25MB (Cloudflare KV limit)
- **Maximum queue name length:** 256 characters
- **Maximum queues per user:** 1000
- **Maximum items per queue:** 10,000 (soft limit)
- **Session token TTL:** 30 days (configurable)

---

## Security Considerations

1. **HTTPS Only:** All requests must use HTTPS
2. **CORS:** Disabled by default (API not intended for browser use)
3. **Session Tokens:** UUIDv4, stored in KV with TTL
4. **Email Allowlist:** ALLOWED_EMAILS environment variable
5. **Queue Isolation:** Users can only access their own queues
6. **No Public Queues:** All queues are private to owner

---

## Implementation Notes

### Queue Storage Format

Queues stored in KV with key: `queue:{email}:{name}`

Value format:
```json
{
  "items": [
    {
      "id": "01HQXYZ123456789ABCDEF",
      "payload": { ... },
      "timestamp": 1704412800000
    }
  ],
  "metadata": {
    "created_at": 1704412000000,
    "total_pushed": 150,
    "total_pulled": 145
  }
}
```

### Session Storage Format

Sessions stored in KV with key: `session:{token}`

Value format:
```json
{
  "email": "user@example.com",
  "created_at": 1704412800000,
  "expires_at": 1707004800000
}
```

---

## Versioning

**Current Version:** 1.0

**Future Versions:**
- Version specified in URL path: `/v2/q/:name/push`
- Current version remains default if no version specified
- Breaking changes require new version
