#!/usr/bin/env python3
"""
Generate markdown comparison report from benchmark results.
"""
import json
from pathlib import Path
from datetime import datetime

def load_results():
    """Load deployment and benchmark results"""
    base_path = Path(__file__).parent

    deployment_file = base_path / "deployment_results.json"
    benchmark_file = base_path / "benchmark_results.json"

    if not deployment_file.exists():
        print("❌ deployment_results.json not found")
        return None, None

    if not benchmark_file.exists():
        print("❌ benchmark_results.json not found")
        return None, None

    with open(deployment_file) as f:
        deployments = json.load(f)

    with open(benchmark_file) as f:
        benchmarks = json.load(f)

    return deployments, benchmarks

def determine_winner(benchmarks):
    """Determine overall winner based on benchmarks"""
    scores = {}

    for bench in benchmarks:
        worker = bench["worker"]
        score = 0

        # Score based on cold start (lower is better)
        if bench["health"] and bench["health"]["cold_start_ms"]:
            cold_start = bench["health"]["cold_start_ms"]
            if cold_start < 150:
                score += 3
            elif cold_start < 250:
                score += 2
            else:
                score += 1

        # Score based on warm latency (lower is better)
        if bench["health"] and bench["health"]["p95_ms"]:
            p95 = bench["health"]["p95_ms"]
            if p95 < 80:
                score += 3
            elif p95 < 120:
                score += 2
            else:
                score += 1

        # Score based on KV write performance
        if bench["kv_write"] and bench["kv_write"]["avg_ms"]:
            kv_write = bench["kv_write"]["avg_ms"]
            if kv_write < 60:
                score += 3
            elif kv_write < 100:
                score += 2
            else:
                score += 1

        scores[worker] = score

    # Return worker with highest score
    if scores:
        return max(scores, key=scores.get)
    return "typescript"  # Default fallback

def generate_markdown(deployments, benchmarks):
    """Generate markdown report"""
    report = []

    report.append("# Cloudflare Worker Viability Test Results\n")
    report.append(f"**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
    report.append("---\n")

    # Executive Summary
    winner = determine_winner(benchmarks)
    report.append("## Executive Summary\n")
    report.append(f"🏆 **Recommended:** **{winner.upper()}**\n")
    report.append("")

    # Deployment Metrics
    report.append("## Deployment Metrics\n")
    report.append("| Worker     | Build Time | Status  | URL |")
    report.append("|------------|------------|---------|-----|")

    for dep in deployments:
        worker = dep["worker"].capitalize()
        build_time = f"{dep['build_time_s']}s" if dep['build_time_s'] else "N/A"
        status = "✅ Success" if dep["success"] else "❌ Failed"
        url = dep["url"] if dep["url"] else "N/A"

        report.append(f"| {worker:10} | {build_time:10} | {status:7} | {url} |")

    report.append("")

    # Performance Metrics
    report.append("## Performance Metrics\n")

    # Cold Start
    report.append("### Cold Start Performance\n")
    report.append("| Worker     | Cold Start (ms) |")
    report.append("|------------|-----------------|")

    for bench in benchmarks:
        worker = bench["worker"].capitalize()
        cold_start = "N/A"
        if bench["health"] and bench["health"]["cold_start_ms"]:
            cold_start = f"{bench['health']['cold_start_ms']}"

        report.append(f"| {worker:10} | {cold_start:15} |")

    report.append("")

    # Warm Request Latency
    report.append("### Warm Request Latency (Health Check)\n")
    report.append("| Worker     | P50 (ms) | P95 (ms) | P99 (ms) | Avg (ms) |")
    report.append("|------------|----------|----------|----------|----------|")

    for bench in benchmarks:
        worker = bench["worker"].capitalize()
        p50 = "N/A"
        p95 = "N/A"
        p99 = "N/A"
        avg = "N/A"

        if bench["health"]:
            if bench["health"]["p50_ms"]:
                p50 = f"{bench['health']['p50_ms']}"
            if bench["health"]["p95_ms"]:
                p95 = f"{bench['health']['p95_ms']}"
            if bench["health"]["p99_ms"]:
                p99 = f"{bench['health']['p99_ms']}"
            if bench["health"]["avg_ms"]:
                avg = f"{bench['health']['avg_ms']}"

        report.append(f"| {worker:10} | {p50:8} | {p95:8} | {p99:8} | {avg:8} |")

    report.append("")

    # KV Operations
    report.append("### KV Write Performance\n")
    report.append("| Worker     | Avg (ms) | P95 (ms) |")
    report.append("|------------|----------|----------|")

    for bench in benchmarks:
        worker = bench["worker"].capitalize()
        avg = "N/A"
        p95 = "N/A"

        if bench["kv_write"]:
            if bench["kv_write"]["avg_ms"]:
                avg = f"{bench['kv_write']['avg_ms']}"
            if bench["kv_write"]["p95_ms"]:
                p95 = f"{bench['kv_write']['p95_ms']}"

        report.append(f"| {worker:10} | {avg:8} | {p95:8} |")

    report.append("")

    report.append("### KV Read Performance\n")
    report.append("| Worker     | Avg (ms) | P95 (ms) |")
    report.append("|------------|----------|----------|")

    for bench in benchmarks:
        worker = bench["worker"].capitalize()
        avg = "N/A"
        p95 = "N/A"

        if bench["kv_read"]:
            if bench["kv_read"]["avg_ms"]:
                avg = f"{bench['kv_read']['avg_ms']}"
            if bench["kv_read"]["p95_ms"]:
                p95 = f"{bench['kv_read']['p95_ms']}"

        report.append(f"| {worker:10} | {avg:8} | {p95:8} |")

    report.append("")

    # Queue Operations
    report.append("### Queue Operations Performance\n")
    report.append("| Worker     | Push Avg (ms) | Pull Avg (ms) |")
    report.append("|------------|---------------|---------------|")

    for bench in benchmarks:
        worker = bench["worker"].capitalize()
        push_avg = "N/A"
        pull_avg = "N/A"

        if bench["queue_ops"]:
            if "push" in bench["queue_ops"] and bench["queue_ops"]["push"]["avg_ms"]:
                push_avg = f"{bench['queue_ops']['push']['avg_ms']}"
            if "pull" in bench["queue_ops"] and bench["queue_ops"]["pull"]["avg_ms"]:
                pull_avg = f"{bench['queue_ops']['pull']['avg_ms']}"

        report.append(f"| {worker:10} | {push_avg:13} | {pull_avg:13} |")

    report.append("")

    # Developer Experience
    report.append("## Developer Experience\n")
    report.append("### TypeScript")
    report.append("- ⭐⭐⭐⭐⭐ **Documentation:** Excellent, best Cloudflare support")
    report.append("- ⭐⭐⭐⭐⭐ **Ecosystem:** Mature, extensive package ecosystem")
    report.append("- ⭐⭐⭐⭐ **Setup:** Easy, npm/wrangler workflow familiar")
    report.append("- ⭐⭐⭐⭐ **Debugging:** Good source maps, browser DevTools\n")

    report.append("### Rust")
    report.append("- ⭐⭐⭐⭐ **Documentation:** Good, official `worker` crate")
    report.append("- ⭐⭐⭐ **Ecosystem:** Growing, WASM limitations")
    report.append("- ⭐⭐⭐ **Setup:** Moderate, requires Rust toolchain + worker-build")
    report.append("- ⭐⭐⭐ **Debugging:** Harder, WASM stack traces\n")

    report.append("### Python")
    report.append("- ⭐⭐ **Documentation:** Limited, beta status")
    report.append("- ⭐⭐ **Ecosystem:** Very limited, Pyodide restrictions")
    report.append("- ⭐⭐⭐ **Setup:** Moderate, Python syntax but Pyodide quirks")
    report.append("- ⭐⭐ **Debugging:** Challenging, limited tooling\n")

    # Recommendation
    report.append("## Recommendation\n")

    if winner == "typescript":
        report.append("**Choose TypeScript** for the Cloudflare Worker implementation.\n")
        report.append("**Reasoning:**")
        report.append("- Best documentation and ecosystem support")
        report.append("- Fastest development velocity")
        report.append("- Good enough performance for queue operations")
        report.append("- Easiest to debug and maintain")
        report.append("- Can still use C# for CLI/GUI (they communicate via HTTP)\n")

    elif winner == "rust":
        report.append("**Choose Rust** for the Cloudflare Worker implementation.\n")
        report.append("**Reasoning:**")
        report.append("- Best performance (cold start and latency)")
        report.append("- Smallest bundle size")
        report.append("- Learning opportunity aligned with goals")
        report.append("- Can still use C# for CLI/GUI (they communicate via HTTP)\n")
        report.append("**Tradeoff:** Steeper learning curve, longer development time\n")

    else:  # python
        report.append("**Choose Python** for the Cloudflare Worker implementation.\n")
        report.append("**Reasoning:**")
        report.append("- Leverage existing Python knowledge")
        report.append("- Acceptable performance for use case")
        report.append("- Can still use C# for CLI/GUI (they communicate via HTTP)\n")
        report.append("**Tradeoff:** Beta status, limited ecosystem, potential future issues\n")

    # Next Steps
    report.append("## Next Steps\n")
    report.append(f"1. **Confirm language choice:** Review results and approve **{winner.upper()}**")
    report.append("2. **Choose client language:** Likely C# (Avalonia for GUI)")
    report.append("3. **Proceed to Phase 1:** Architecture design and project setup")
    report.append("4. **Design Worker API:** Define REST endpoints for queue operations")
    report.append("5. **Implement authentication:** Google OAuth + session tokens")
    report.append("")

    return "\n".join(report)

def main():
    print("📝 Generating comparison report...\n")

    deployments, benchmarks = load_results()

    if not deployments or not benchmarks:
        print("❌ Cannot generate report - missing results")
        return

    markdown = generate_markdown(deployments, benchmarks)

    # Save report
    output_file = Path(__file__).parent.parent / "RESULTS.md"
    with open(output_file, 'w') as f:
        f.write(markdown)

    print(f"✅ Report generated: {output_file}\n")
    print("=" * 60)
    print(markdown)
    print("=" * 60)

if __name__ == "__main__":
    main()
