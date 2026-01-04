# Worker Viability Testing - Phase 0

This directory contains automated tests to compare TypeScript, Rust, and Python implementations of Cloudflare Workers before committing to a language stack for the Forebay project.

## Purpose

Make a **data-driven decision** about which language to use for the Cloudflare Worker backend by empirically measuring:
- Deployment metrics (build time, bundle size)
- Performance metrics (cold start, latency, KV operations)
- Developer experience

## Structure

```
worker-viability-tests/
├── typescript-worker/    # TypeScript implementation (Hono framework)
├── rust-worker/          # Rust implementation (worker crate)
├── python-worker/        # Python implementation (Pyodide)
└── test-harness/         # Automated testing scripts
    ├── deploy.py         # Deploy all workers
    ├── benchmark.py      # Run performance tests
    └── report.py         # Generate comparison report
```

## Prerequisites

1. **Node.js and npm** (for TypeScript worker and wrangler)
   ```bash
   node --version  # Should be v18+
   npm --version
   ```

2. **Rust** (for Rust worker)
   ```bash
   rustc --version  # Should be 1.70+
   cargo --version
   ```

3. **Python 3** (for test scripts)
   ```bash
   python3 --version  # Should be 3.8+
   ```

4. **Wrangler CLI** (Cloudflare's deployment tool)
   ```bash
   npm install -g wrangler
   wrangler login  # Authenticate with Cloudflare
   ```

5. **Python dependencies** (for test harness)
   ```bash
   cd test-harness
   pip install -r requirements.txt
   ```

## Running the Tests

### Step 1: Deploy Workers

```bash
cd test-harness
python3 deploy.py
```

This will:
- Install dependencies for each worker
- Build and deploy all three workers to Cloudflare
- Record build times and deployment URLs
- Save results to `deployment_results.json`

**Expected output:**
```
🚀 Cloudflare Worker Viability Test - Deployment

✅ Using wrangler: 3.x.x

📦 Deploying typescript Worker...
  Installing dependencies for typescript...
  Running wrangler deploy...
  ✅ Deployed successfully
     Build time: 2.1s
     URL: https://typescript-viability-test.yourname.workers.dev

... (rust and python deployments) ...

✅ All workers deployed successfully!
```

### Step 2: Run Benchmarks

```bash
python3 benchmark.py
```

This will:
- Test health check latency (100 requests per worker)
- Test KV write performance (50 operations per worker)
- Test KV read performance (50 operations per worker)
- Test queue push/pull operations (25 each per worker)
- Calculate percentiles (p50, p95, p99)
- Save results to `benchmark_results.json`

**Expected duration:** ~10-15 minutes

### Step 3: Generate Report

```bash
python3 report.py
```

This will:
- Load deployment and benchmark results
- Calculate overall scores
- Determine recommended language
- Generate markdown report
- Save to `../RESULTS.md`

## Interpreting Results

The generated `RESULTS.md` will include:

1. **Executive Summary** - Overall recommendation
2. **Deployment Metrics** - Build times and status
3. **Performance Metrics** - Latency, cold start, KV performance
4. **Developer Experience** - Qualitative assessment
5. **Recommendation** - Data-driven language choice with reasoning

## Test Endpoints

Each worker implements identical REST API:

- `GET /health` - Health check
- `POST /kv-write` - Write to KV (body: `{key, value}`)
- `GET /kv-read/:key` - Read from KV
- `POST /queue-push/:name` - Push item to queue (body: `{item}`)
- `GET /queue-pull/:name` - Pull (read+delete) from queue

## Troubleshooting

### "wrangler not found"
```bash
npm install -g wrangler
wrangler login
```

### TypeScript deployment fails
```bash
cd typescript-worker
npm install
npx wrangler deploy
```

### Rust deployment fails
```bash
cd rust-worker
cargo install worker-build
npx wrangler deploy
```

### Python deployment fails
Python Workers are in beta - check Cloudflare docs for current status.

### Benchmark timeouts
- Workers may need time to "warm up" after deployment
- Wait 30-60 seconds before running benchmarks
- Check worker URLs are accessible in browser

## Next Steps

After reviewing `RESULTS.md`:

1. **Approve language choice** - Based on test results
2. **Proceed to Phase 1** - Architecture design
3. **Choose client language** - Likely C# for CLI/GUI
4. **Design Worker API** - Define REST endpoints
5. **Implement authentication** - Google OAuth + sessions

## Notes

- Workers communicate via HTTP, so Worker and Client can be different languages
- These are minimal test implementations - production will be more sophisticated
- Tests run sequentially to avoid rate limiting and ensure fair comparison
- Cold start is measured as the first request after deployment wait period
