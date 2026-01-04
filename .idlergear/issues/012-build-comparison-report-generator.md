---
id: 12
title: Build comparison report generator
state: open
created: '2026-01-04T06:21:27.577428Z'
labels:
- phase-0
- testing
- documentation
priority: medium
---
Create script to generate markdown report from benchmark results.

**Input:** JSON benchmark results

**Output:** Markdown report with:

1. **Executive Summary**
   - Winner in each category
   - Overall recommendation

2. **Deployment Metrics Table**
   - Build time
   - Bundle size
   - Deployment success

3. **Performance Tables**
   - Cold start comparison
   - Request latency (p50/p95/p99)
   - KV operations
   - Queue operations

4. **Developer Experience Notes**
   - Ease of setup
   - Documentation quality
   - Ecosystem maturity

5. **Recommendation**
   - Data-driven choice
   - Tradeoffs explained
   - Justification

**Format:**
```markdown
# Cloudflare Worker Viability Test Results

## Executive Summary
🏆 **Recommended:** [Language] based on [reasons]

## Deployment
| Worker     | Build Time | Bundle Size | Status |
|------------|------------|-------------|--------|
| TypeScript | 2.1s       | 850KB       | ✅     |
...

## Performance
...

## Recommendation
Based on testing, we recommend **[Language]** because...
```

**Save to:** `worker-viability-tests/RESULTS.md`
