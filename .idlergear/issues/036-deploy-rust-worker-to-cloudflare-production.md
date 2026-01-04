---
id: 36
title: Deploy Rust Worker to Cloudflare production
state: open
created: '2026-01-04T09:22:39.540692Z'
labels:
- deployment
- worker
- production
priority: high
---
Deploy the Forebay Rust Worker to Cloudflare Workers production environment.

**Prerequisites:**
- [ ] wrangler.toml configured (task #34)
- [ ] KV namespaces created (task #35)
- [ ] Google OAuth client set up (task #5)
- [ ] ALLOWED_EMAILS configured
- [ ] All tests passing

**Deployment Steps:**

1. **Pre-deployment Checks:**
   - Run `cargo test` - all tests must pass
   - Run `cargo build --release --target wasm32-unknown-unknown`
   - Verify wrangler.toml configuration
   - Check KV namespace bindings

2. **Set Environment Variables:**
   ```bash
   wrangler secret put GOOGLE_CLIENT_ID
   wrangler secret put ALLOWED_EMAILS
   ```

3. **Deploy:**
   ```bash
   wrangler deploy
   ```

4. **Post-deployment Verification:**
   - Test health endpoint: `curl https://forebay-worker.<account>.workers.dev/health`
   - Test authentication flow (manual)
   - Check worker logs for errors
   - Verify KV access working

5. **Record Deployment:**
   - Save worker URL to .env or config
   - Update client default worker URL
   - Document deployment date/version

**Acceptance Criteria:**
- Worker deployed successfully
- Health endpoint returns 200 OK
- Authentication endpoints accessible
- Queue endpoints accessible (with auth)
- No errors in worker logs
- KV operations working
- Worker URL documented

**Rollback Plan:**
- Keep previous deployment available
- Use `wrangler rollback` if issues found
- Monitor error rates for 24 hours
