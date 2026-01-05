---
id: 67
title: Add CORS configuration for browser-based testing
state: open
created: '2026-01-05T00:43:24.683755Z'
labels:
- worker
- rust
- infrastructure
priority: low
---
Add CORS headers to Worker to enable browser-based testing and future web UI.

## Implementation

### Configuration
```toml
[env.production.vars]
CORS_ALLOWED_ORIGINS = "*"  # Or specific: "https://forebay.skpt.cl,http://localhost:3000"
```

### Response Headers
```rust
response.headers_mut().set("Access-Control-Allow-Origin", origin)?;
response.headers_mut().set("Access-Control-Allow-Methods", "GET, POST, DELETE, OPTIONS")?;
response.headers_mut().set("Access-Control-Allow-Headers", "Content-Type, Authorization")?;
response.headers_mut().set("Access-Control-Max-Age", "86400")?;
```

### OPTIONS Preflight
```rust
if req.method() == Method::Options {
    return Response::empty()
        .with_cors_headers()
        .with_status(204);
}
```

## Files
- worker/src/cors.rs
- worker/src/lib.rs

## Acceptance Criteria
- [ ] CORS headers on all responses
- [ ] OPTIONS preflight handled
- [ ] Configurable origins
- [ ] Works with browser fetch/XHR

**Priority:** Low (v1.0), High (if building web UI)
**Milestone:** v1.0 - Core Backend
