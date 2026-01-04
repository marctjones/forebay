---
id: 34
title: Configure Cloudflare Worker deployment with wrangler.toml
state: open
created: '2026-01-04T09:22:08.677869Z'
labels:
- deployment
- configuration
- worker
priority: high
---
Create production-ready wrangler.toml configuration for deploying the Rust Worker.

**Configuration Required:**

1. **Basic Settings:**
   - name = "forebay-worker"
   - main = "build/worker/shim.mjs"
   - compatibility_date = "2024-01-01"
   - account_id (from Cloudflare dashboard)

2. **KV Namespaces:**
   - SESSION_TOKENS namespace for auth sessions
   - QUEUES namespace for queue data
   - Preview bindings for development
   - Production bindings for deployment

3. **Environment Variables:**
   - GOOGLE_CLIENT_ID (OAuth client)
   - ALLOWED_EMAILS (comma-separated whitelist)
   - SESSION_TTL_DAYS (default: 30)

4. **Build Configuration:**
   - [build] command for cargo build
   - [build.upload] format = "modules"
   - wasm-bindgen integration

**Tasks:**
- [ ] Create wrangler.toml in worker/ directory
- [ ] Configure KV namespace bindings
- [ ] Set up environment variable placeholders
- [ ] Add .dev.vars for local development
- [ ] Document required secrets in README
- [ ] Test local deployment with `wrangler dev`
- [ ] Create production deployment script

**Acceptance Criteria:**
- `wrangler dev` runs locally with mock KV
- Environment variables documented
- KV namespaces properly bound
- Build command works correctly
- Ready for `wrangler deploy`

**Reference:**
- https://developers.cloudflare.com/workers/wrangler/configuration/
