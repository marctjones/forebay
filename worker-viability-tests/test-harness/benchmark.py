#!/usr/bin/env python3
"""
Benchmark all deployed Workers and measure performance.
"""
import requests
import time
import json
import statistics
from pathlib import Path

def percentile(data, p):
    """Calculate percentile manually (avoiding numpy dependency if possible)"""
    sorted_data = sorted(data)
    k = (len(sorted_data) - 1) * p
    f = int(k)
    c = int(k) + 1
    if f == c:
        return sorted_data[f]
    d0 = sorted_data[f] * (c - k)
    d1 = sorted_data[c] * (k - f)
    return d0 + d1

def test_health_check(url, num_requests=100):
    """Test health endpoint latency"""
    print(f"  Testing health check ({num_requests} requests)...")

    latencies = []

    for i in range(num_requests):
        start = time.time()
        try:
            response = requests.get(f"{url}/health", timeout=10)
            latency_ms = (time.time() - start) * 1000

            if response.status_code == 200:
                latencies.append(latency_ms)
            else:
                print(f"    ⚠️  Request {i+1} failed with status {response.status_code}")

        except Exception as e:
            print(f"    ❌ Request {i+1} error: {e}")

        # Brief delay between requests
        if i < num_requests - 1:
            time.sleep(0.05)

    if not latencies:
        return None

    return {
        "count": len(latencies),
        "cold_start_ms": round(latencies[0], 2),
        "p50_ms": round(percentile(latencies[1:], 0.50), 2) if len(latencies) > 1 else None,
        "p95_ms": round(percentile(latencies[1:], 0.95), 2) if len(latencies) > 1 else None,
        "p99_ms": round(percentile(latencies[1:], 0.99), 2) if len(latencies) > 1 else None,
        "avg_ms": round(statistics.mean(latencies[1:]), 2) if len(latencies) > 1 else None,
        "min_ms": round(min(latencies[1:]), 2) if len(latencies) > 1 else None,
        "max_ms": round(max(latencies[1:]), 2) if len(latencies) > 1 else None,
    }

def test_kv_write(url, num_writes=100):
    """Test KV write performance"""
    print(f"  Testing KV writes ({num_writes} operations)...")

    latencies = []

    for i in range(num_writes):
        key = f"test_key_{i}"
        value = f"test_value_{i}" * 10  # ~100 bytes

        try:
            start = time.time()
            response = requests.post(
                f"{url}/kv-write",
                json={"key": key, "value": value},
                timeout=10
            )
            latency_ms = (time.time() - start) * 1000

            if response.status_code == 200:
                latencies.append(latency_ms)

        except Exception as e:
            print(f"    ❌ Write {i+1} error: {e}")

        time.sleep(0.05)

    if not latencies:
        return None

    return {
        "count": len(latencies),
        "avg_ms": round(statistics.mean(latencies), 2),
        "p50_ms": round(percentile(latencies, 0.50), 2),
        "p95_ms": round(percentile(latencies, 0.95), 2),
        "min_ms": round(min(latencies), 2),
        "max_ms": round(max(latencies), 2),
    }

def test_kv_read(url, num_reads=100):
    """Test KV read performance"""
    print(f"  Testing KV reads ({num_reads} operations)...")

    # First, write some test data
    for i in range(10):
        key = f"read_test_{i}"
        value = f"value_{i}" * 10
        try:
            requests.post(f"{url}/kv-write", json={"key": key, "value": value}, timeout=10)
        except:
            pass

    time.sleep(1)  # Wait for KV to propagate

    latencies = []

    for i in range(num_reads):
        key = f"read_test_{i % 10}"

        try:
            start = time.time()
            response = requests.get(f"{url}/kv-read/{key}", timeout=10)
            latency_ms = (time.time() - start) * 1000

            if response.status_code == 200:
                latencies.append(latency_ms)

        except Exception as e:
            print(f"    ❌ Read {i+1} error: {e}")

        time.sleep(0.05)

    if not latencies:
        return None

    return {
        "count": len(latencies),
        "avg_ms": round(statistics.mean(latencies), 2),
        "p50_ms": round(percentile(latencies, 0.50), 2),
        "p95_ms": round(percentile(latencies, 0.95), 2),
        "min_ms": round(min(latencies), 2),
        "max_ms": round(max(latencies), 2),
    }

def test_queue_ops(url, num_ops=50):
    """Test queue push and pull operations"""
    print(f"  Testing queue operations ({num_ops} push + {num_ops} pull)...")

    push_latencies = []
    pull_latencies = []

    queue_name = "benchmark_queue"

    # Push operations
    for i in range(num_ops):
        item = {"id": i, "data": f"item_{i}"}

        try:
            start = time.time()
            response = requests.post(
                f"{url}/queue-push/{queue_name}",
                json={"item": item},
                timeout=10
            )
            latency_ms = (time.time() - start) * 1000

            if response.status_code == 200:
                push_latencies.append(latency_ms)

        except Exception as e:
            print(f"    ❌ Push {i+1} error: {e}")

        time.sleep(0.05)

    # Pull operations
    for i in range(num_ops):
        try:
            start = time.time()
            response = requests.get(f"{url}/queue-pull/{queue_name}", timeout=10)
            latency_ms = (time.time() - start) * 1000

            if response.status_code == 200:
                pull_latencies.append(latency_ms)

        except Exception as e:
            # Expected to fail when queue is empty
            pass

        time.sleep(0.05)

    results = {}

    if push_latencies:
        results["push"] = {
            "count": len(push_latencies),
            "avg_ms": round(statistics.mean(push_latencies), 2),
            "p95_ms": round(percentile(push_latencies, 0.95), 2),
        }

    if pull_latencies:
        results["pull"] = {
            "count": len(pull_latencies),
            "avg_ms": round(statistics.mean(pull_latencies), 2),
            "p95_ms": round(percentile(pull_latencies, 0.95), 2),
        }

    return results if results else None

def benchmark_worker(worker_type, url):
    """Run all benchmarks for a single worker"""
    print(f"\n📊 Benchmarking {worker_type.upper()} Worker")
    print(f"   URL: {url}\n")

    results = {
        "worker": worker_type,
        "url": url,
        "health": None,
        "kv_write": None,
        "kv_read": None,
        "queue_ops": None,
    }

    # Wait for worker to be ready
    print("  ⏳ Waiting for worker to be ready...")
    time.sleep(10)

    try:
        # Health check test
        results["health"] = test_health_check(url)

        # KV write test
        results["kv_write"] = test_kv_write(url, num_writes=50)

        # KV read test
        results["kv_read"] = test_kv_read(url, num_reads=50)

        # Queue operations test
        results["queue_ops"] = test_queue_ops(url, num_ops=25)

        print(f"\n  ✅ Benchmark complete for {worker_type}")

    except Exception as e:
        print(f"  ❌ Benchmark failed: {e}")

    return results

def main():
    print("⚡ Cloudflare Worker Viability Test - Benchmarking\n")
    print("=" * 60)

    # Load deployment results
    deployment_file = Path(__file__).parent / "deployment_results.json"

    if not deployment_file.exists():
        print("❌ No deployment results found!")
        print("   Run deploy.py first to deploy the workers")
        return

    with open(deployment_file) as f:
        deployments = json.load(f)

    # Filter successful deployments
    successful_workers = [d for d in deployments if d["success"] and d["url"]]

    if not successful_workers:
        print("❌ No successfully deployed workers found!")
        return

    print(f"Found {len(successful_workers)} deployed workers\n")

    # Benchmark each worker
    all_results = []

    for deployment in successful_workers:
        result = benchmark_worker(deployment["worker"], deployment["url"])
        all_results.append(result)

    # Save results
    output_file = Path(__file__).parent / "benchmark_results.json"
    with open(output_file, 'w') as f:
        json.dump(all_results, f, indent=2)

    print("\n" + "=" * 60)
    print(f"✅ Benchmark results saved to: {output_file}")
    print("\n👉 Next step: Run report.py to generate comparison report")

if __name__ == "__main__":
    main()
