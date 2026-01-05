---
id: 61
title: Configure multi-environment deployment (dev/test/prod)
state: open
created: '2026-01-05T00:06:32.599477Z'
labels:
- deployment
- cloudflare
- infrastructure
- configuration
priority: high
---
Set up separate Cloudflare Worker environments for development, testing, and production.

**Environments Needed:**

### 1. Development (Local)
- `wrangler dev` with local KV simulation
- `.dev.vars` file for local secrets
- Mock Google OAuth for testing
- Fast iteration without deploying

### 2. Test/Staging
- Deployed Worker: `forebay-test.yourname.workers.dev`
- Separate KV namespaces (test data isolation)
- Test OAuth credentials
- Used by CI/CD pipeline
- Free tier compatible

### 3. Production
- Deployed Worker: `forebay.yourname.workers.dev` (or custom domain)
- Production KV namespaces
- Production OAuth credentials
- Production email allowlist
- Free tier: 100,000 requests/day

**Implementation:**

### wrangler.toml Environments
```toml
name = "forebay-worker"
main = "build/worker/shim.mjs"
compatibility_date = "2024-01-01"

# Development (local only, no deployment)
[env.dev]
# Uses .dev.vars for secrets

# Test/Staging environment
[env.test]
[[env.test.kv_namespaces]]
binding = "SESSION_TOKENS"
id = "test_session_namespace_id"

[[env.test.kv_namespaces]]
binding = "QUEUE_DATA"  
id = "test_queue_namespace_id"

[env.test.vars]
ALLOWED_EMAILS = "test@example.com"

# Production environment  
[env.production]
[[env.production.kv_namespaces]]
binding = "SESSION_TOKENS"
id = "prod_session_namespace_id"

[[env.production.kv_namespaces]]
binding = "QUEUE_DATA"
id = "prod_queue_namespace_id"

[env.production.vars]
ALLOWED_EMAILS = "your@email.com"
```

### Deployment Commands
```bash
# Local development
wrangler dev

# Deploy to test
wrangler deploy --env test

# Deploy to production
wrangler deploy --env production
```

### Free Tier Considerations
**Limits per account:**
- 100,000 requests/day (across all workers)
- 10ms CPU time per request
- KV: 100,000 reads/day, 1,000 writes/day
- Unlimited KV storage (up to 1GB per namespace)

**Recommendations:**
- Use test environment sparingly
- Delete test data periodically to stay within KV limits
- Monitor usage via Cloudflare dashboard
- Consider upgrading to paid plan ($5/month) if needed

**Tasks:**
- [ ] Create separate KV namespaces for test and prod
- [ ] Configure wrangler.toml with environments
- [ ] Create .dev.vars.example file
- [ ] Document environment setup in DEPLOYMENT.md
- [ ] Add deployment scripts for each environment
- [ ] Test deployment to all three environments
- [ ] Document free tier usage monitoring

**Acceptance Criteria:**
- [ ] `wrangler dev` works locally
- [ ] Can deploy to test independently
- [ ] Can deploy to production independently  
- [ ] Each environment has isolated data
- [ ] Free tier limits documented
- [ ] Usage monitoring instructions provided

**Dependencies:** #20 (wrangler.toml), #21 (KV namespaces)
