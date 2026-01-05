---
id: 45
title: Document API specification and create OpenAPI/Swagger spec
state: closed
created: '2026-01-04T09:25:28.121332Z'
labels:
- documentation
- api
priority: medium
---
Create comprehensive API documentation and OpenAPI specification for the Worker REST API.

**Deliverables:**

## 1. OpenAPI 3.0 Specification (`docs/api/openapi.yaml`)

```yaml
openapi: 3.0.0
info:
  title: Forebay Queue API
  version: 1.0.0
  description: REST API for Forebay message queue system

servers:
  - url: https://forebay-worker.{account}.workers.dev
    description: Production Cloudflare Worker

paths:
  /health:
    get:
      summary: Health check
      responses:
        '200':
          description: Service is healthy
          
  /auth/login:
    post:
      summary: Authenticate with Google OAuth
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              properties:
                id_token:
                  type: string
                  description: Google ID token from OAuth flow
      responses:
        '200':
          description: Login successful
          content:
            application/json:
              schema:
                type: object
                properties:
                  session_token:
                    type: string
                  email:
                    type: string
                  expires_at:
                    type: integer
        '401':
          description: Invalid token or unauthorized email

  # ... (all other endpoints)
```

## 2. API Documentation (`docs/api/README.md`)

**Sections:**
- Authentication
  - OAuth flow
  - Session tokens
  - Authorization header format
- Endpoints reference
  - Request/response examples
  - Error codes
  - Rate limits (if any)
- Data models
  - Queue item structure
  - Error response format
- Best practices
  - Error handling
  - Retry logic
  - Pagination (future)

## 3. Code Examples (`docs/api/examples/`)

Create example code in multiple languages:

**cURL Examples:**
```bash
# Login
curl -X POST https://worker.url/auth/login \
  -H "Content-Type: application/json" \
  -d '{"id_token": "..."}'

# Push to queue
curl -X POST https://worker.url/queues/work/push \
  -H "Authorization: Bearer session-token" \
  -H "Content-Type: application/json" \
  -d '{"payload": {"task": "process"}}'
```

**JavaScript/TypeScript:**
```typescript
// Using fetch API
const response = await fetch('https://worker.url/queues/work/push', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${sessionToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    payload: { task: 'process' }
  })
});
```

**Python:**
```python
import requests

response = requests.post(
    'https://worker.url/queues/work/push',
    headers={'Authorization': f'Bearer {session_token}'},
    json={'payload': {'task': 'process'}}
)
```

## 4. Interactive API Documentation

Generate interactive docs using:
- Swagger UI from OpenAPI spec
- Redoc alternative
- Host on GitHub Pages or docs site

**Acceptance Criteria:**
- [ ] Complete OpenAPI 3.0 spec
- [ ] All endpoints documented
- [ ] Request/response schemas defined
- [ ] Error responses documented
- [ ] Code examples for common operations
- [ ] Interactive docs hosted
- [ ] Validation of OpenAPI spec passes
- [ ] Examples tested and working
