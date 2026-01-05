---
id: 68
title: Set up monitoring and error logging for production Worker
state: open
created: '2026-01-05T00:43:24.958735Z'
labels:
- deployment
- infrastructure
- monitoring
priority: medium
---
Configure monitoring, error logging, and alerting for production Worker.

## Cloudflare Analytics

### Built-in Metrics (Free)
- Request count
- Error rate
- Latency (P50, P95, P99)
- CPU time
- KV operations

**Access:** Cloudflare Dashboard → Workers & Pages → forebay-worker → Metrics

### Logpush (Paid - $5/month)
- Stream logs to external service
- Integrate with Datadog, Splunk, etc.

## Error Tracking

### Option 1: Sentry (Recommended)
```rust
use sentry_rust::Sentry;

// Initialize in Worker
let sentry = Sentry::new(env.var("SENTRY_DSN")?);

// Capture errors
sentry.capture_error(&error);
```

**Benefits:**
- Error aggregation
- Stack traces
- User context
- Free tier available

### Option 2: Cloudflare Tail (Development)
```bash
wrangler tail forebay-worker --env production
# Real-time log streaming
```

## Structured Logging

```rust
console_log!(
    "{{\"level\":\"error\",\"message\":\"{}\",\"user\":\"{}\",\"queue\":\"{}\"}}",
    error,
    email,
    queue_name
);
```

## Health Monitoring

### Uptime Monitoring
- UptimeRobot (free)
- Pingdom
- Cloudflare Health Checks

**Monitor:** `GET https://forebay.skpt.cl/health`

### Alerts
- Email on downtime
- Slack notifications
- PagerDuty for critical

## Files
- worker/src/logging.rs
- worker/src/monitoring.rs

## Acceptance Criteria
- [ ] Error tracking configured
- [ ] Uptime monitoring setup
- [ ] Alerts configured
- [ ] Dashboard accessible

**Priority:** Medium (important for production)
**Milestone:** v1.0 - Deployment
