---
id: 62
title: Configure custom domain for Cloudflare Worker
state: open
created: '2026-01-05T00:06:32.856859Z'
labels:
- deployment
- cloudflare
- infrastructure
- configuration
- documentation
priority: medium
---
Set up custom domain routing for Forebay Worker instead of using default workers.dev subdomain.

**Project-Specific Setup:**
- **Domain:** `skpt.cl` (already managed by Cloudflare)
- **Recommended subdomain:** `forebay.skpt.cl`
- **Production API:** `https://forebay.skpt.cl`

**Use Cases:**
1. **Production with custom domain:** `forebay.skpt.cl`
2. **Multiple domains in account:** Specify `zone_name = "skpt.cl"`
3. **Branding:** Professional URL instead of `*.workers.dev`

**Prerequisites:**
- ✅ Domain `skpt.cl` managed by Cloudflare DNS (already done)
- ✅ Cloudflare account with domain access
- Worker deployed to production

**Configuration for skpt.cl:**

### Step 1: Add Custom Domain in Cloudflare Dashboard
1. Go to Cloudflare Dashboard → Workers & Pages → forebay-worker
2. Click "Triggers" tab
3. Click "Add Custom Domain"
4. Enter: `forebay.skpt.cl`
5. Click "Add Custom Domain"
6. Cloudflare automatically:
   - Creates DNS CNAME record
   - Provisions SSL certificate
   - Routes traffic to Worker

### Step 2: Update wrangler.toml
```toml
name = "forebay-worker"
main = "build/worker/shim.mjs"
compatibility_date = "2024-01-01"

# Production environment with skpt.cl domain
[env.production]
routes = [
  { pattern = "forebay.skpt.cl/*", zone_name = "skpt.cl" }
]

[[env.production.kv_namespaces]]
binding = "SESSION_TOKENS"
id = "prod_session_namespace_id"

[[env.production.kv_namespaces]]
binding = "QUEUE_DATA"
id = "prod_queue_namespace_id"

[env.production.vars]
ALLOWED_EMAILS = "your@email.com"
```

**Why `zone_name = "skpt.cl"` is needed:**
- Specifies which domain to use if Cloudflare account has multiple domains
- Without it, wrangler might be ambiguous about which domain zone to deploy to
- Makes deployment configuration explicit

### Step 3: Deploy
```bash
# Deploy to production with custom domain
wrangler deploy --env production

# Verify deployment
curl https://forebay.skpt.cl/health
```

**DNS Verification:**
After deployment, verify DNS record was created:
```bash
dig forebay.skpt.cl
# Should show CNAME or A record pointing to Cloudflare Workers
```

### Alternative Subdomain Options
If `forebay.skpt.cl` doesn't suit your needs, consider:
- `api.skpt.cl` - Generic API endpoint
- `queue.skpt.cl` - Descriptive of functionality
- `fb.skpt.cl` - Abbreviated version

### CLI Configuration
Update CLI default Worker URL to use custom domain:

**client/Forebay.Cli/appsettings.json:**
```json
{
  "WorkerUrl": "https://forebay.skpt.cl"
}
```

Or users can configure manually:
```bash
# User configuration
forebay config set worker-url https://forebay.skpt.cl
```

### Test Environment
For test/staging, use workers.dev subdomain:
```toml
[env.test]
# No custom domain, uses forebay-test.yourname.workers.dev
```

This keeps test separate and doesn't consume a subdomain on skpt.cl.

**Free Tier Compatibility:**
- ✅ Custom domains work on free tier
- ✅ SSL certificates automatically provisioned
- ✅ No additional cost for custom domains
- ✅ skpt.cl already on Cloudflare DNS (perfect setup)
- ✅ Unlimited subdomains (can add more later if needed)

**Production URLs:**
- **Worker API:** `https://forebay.skpt.cl`
- **Health check:** `https://forebay.skpt.cl/health`
- **Auth:** `https://forebay.skpt.cl/auth/login`
- **Queues:** `https://forebay.skpt.cl/queues/{name}/push`

**Test URLs:**
- **Worker API:** `https://forebay-test.yourname.workers.dev`
- Keeps test clearly separated from production

**Tasks:**
- [ ] Verify skpt.cl domain access in Cloudflare dashboard
- [ ] Decide on subdomain (forebay.skpt.cl recommended)
- [ ] Add custom domain via Cloudflare dashboard
- [ ] Update wrangler.toml with route and zone_name
- [ ] Deploy to production
- [ ] Verify DNS resolution
- [ ] Update CLI default Worker URL
- [ ] Test API endpoints on custom domain
- [ ] Update documentation with production URL
- [ ] Add SSL certificate verification test

**Acceptance Criteria:**
- [ ] Production Worker accessible via `https://forebay.skpt.cl`
- [ ] SSL certificate valid and trusted
- [ ] DNS resolves correctly
- [ ] CLI configured with skpt.cl URL by default
- [ ] Documentation reflects skpt.cl domain
- [ ] zone_name specified for multi-domain account

**Documentation Needed:**
- Update README.md with `forebay.skpt.cl` as production URL
- Add domain setup steps to DEPLOYMENT.md
- Document how to verify SSL certificate
- Add troubleshooting for DNS issues

**Example CLI Usage:**
```bash
# After deployment to forebay.skpt.cl
forebay login  # Opens https://forebay.skpt.cl for OAuth
echo '{"task":"deploy"}' | forebay push work/tasks
forebay pull work/tasks
```

**References:**
- https://developers.cloudflare.com/workers/configuration/routing/routes/
- https://developers.cloudflare.com/workers/configuration/routing/custom-domains/
