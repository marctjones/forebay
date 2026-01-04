---
id: 14
title: Design Worker REST API specification
state: closed
created: '2026-01-04T08:32:48.399202Z'
labels:
- phase-1
- design
- documentation
priority: high
---
Design the REST API contract for the Rust Worker before implementation.

**Endpoints to define:**

```
POST   /auth/login           - Google OAuth login
GET    /auth/whoami          - Check session
POST   /auth/logout          - Invalidate session
POST   /q/:name/push         - Enqueue item
GET    /q/:name/pull         - Dequeue item (atomic read+delete)
GET    /q/:name/subscribe    - Long-polling or WebSocket
GET    /q/:name/stats        - Queue statistics (length, oldest item)
DELETE /q/:name              - Delete queue
GET    /queues               - List all queues for user
```

**For each endpoint, document:**
- HTTP method and path
- Request body schema (JSON)
- Response body schema (JSON)
- Status codes (200, 400, 401, 404, 500)
- Authentication requirements
- Error responses

**Save as:** `.idlergear/wiki/api-specification.md`

**Note:** This API contract will be used by both Rust Worker and C# Client implementations.
