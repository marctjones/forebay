#!/usr/bin/env python3
"""
Deploy all three Workers and measure deployment metrics.
Uses wrangler CLI for deployment.
"""
import subprocess
import time
import json
import os
from pathlib import Path

WORKERS_DIR = Path(__file__).parent.parent
WORKERS = {
    "typescript": WORKERS_DIR / "typescript-worker",
    "rust": WORKERS_DIR / "rust-worker",
    "python": WORKERS_DIR / "python-worker",
}

def run_command(cmd, cwd=None):
    """Run shell command and return output"""
    result = subprocess.run(
        cmd,
        shell=True,
        cwd=cwd,
        capture_output=True,
        text=True
    )
    return result.returncode, result.stdout, result.stderr

def get_bundle_size(worker_path):
    """Estimate bundle size from build output (rough estimate)"""
    # This is a placeholder - actual bundle size would need wrangler output parsing
    return "N/A"

def install_dependencies(worker_type, worker_path):
    """Install dependencies for a worker"""
    print(f"  Installing dependencies for {worker_type}...")

    if worker_type == "typescript":
        code, stdout, stderr = run_command("npm install", cwd=worker_path)
        if code != 0:
            print(f"    ❌ Failed to install npm dependencies: {stderr}")
            return False

    elif worker_type == "rust":
        # Rust dependencies are handled by cargo during build
        pass

    elif worker_type == "python":
        # Python Workers use Pyodide, no pip install needed
        pass

    return True

def deploy_worker(worker_type, worker_path):
    """Deploy a single worker and return metrics"""
    print(f"\n📦 Deploying {worker_type} Worker...")

    metrics = {
        "worker": worker_type,
        "build_time_s": None,
        "bundle_size": None,
        "success": False,
        "url": None,
        "error": None
    }

    # Install dependencies first
    if not install_dependencies(worker_type, worker_path):
        metrics["error"] = "Dependency installation failed"
        return metrics

    # Deploy with wrangler
    print(f"  Running wrangler deploy...")
    start_time = time.time()

    code, stdout, stderr = run_command("npx wrangler deploy", cwd=worker_path)

    build_time = time.time() - start_time
    metrics["build_time_s"] = round(build_time, 2)

    if code != 0:
        print(f"  ❌ Deployment failed")
        print(f"     Error: {stderr}")
        metrics["error"] = stderr
        return metrics

    # Parse deployment output for URL
    for line in stdout.split('\n'):
        if 'https://' in line and 'workers.dev' in line:
            # Extract URL
            parts = line.split('https://')
            if len(parts) > 1:
                url = 'https://' + parts[1].split()[0]
                metrics["url"] = url.rstrip('/')
                break

    metrics["success"] = True
    metrics["bundle_size"] = get_bundle_size(worker_path)

    print(f"  ✅ Deployed successfully")
    print(f"     Build time: {metrics['build_time_s']}s")
    print(f"     URL: {metrics['url']}")

    return metrics

def save_deployment_results(results):
    """Save deployment results to JSON"""
    output_file = Path(__file__).parent / "deployment_results.json"

    with open(output_file, 'w') as f:
        json.dump(results, f, indent=2)

    print(f"\n📊 Deployment results saved to: {output_file}")

def main():
    print("🚀 Cloudflare Worker Viability Test - Deployment\n")
    print("=" * 60)

    all_results = []

    # Check if wrangler is installed
    code, stdout, stderr = run_command("npx wrangler --version")
    if code != 0:
        print("❌ wrangler not found. Please install it:")
        print("   npm install -g wrangler")
        return

    print(f"✅ Using wrangler: {stdout.strip()}\n")

    # Deploy each worker
    for worker_type, worker_path in WORKERS.items():
        result = deploy_worker(worker_type, worker_path)
        all_results.append(result)

        # Wait between deployments
        if worker_type != "python":
            print("  ⏳ Waiting 5s before next deployment...")
            time.sleep(5)

    # Save results
    save_deployment_results(all_results)

    # Summary
    print("\n" + "=" * 60)
    print("📋 DEPLOYMENT SUMMARY\n")

    for result in all_results:
        status = "✅" if result["success"] else "❌"
        print(f"{status} {result['worker'].capitalize()}")
        print(f"   Build time: {result['build_time_s']}s")
        if result['url']:
            print(f"   URL: {result['url']}")
        if result['error']:
            print(f"   Error: {result['error'][:100]}...")
        print()

    # Check if all succeeded
    all_success = all(r["success"] for r in all_results)
    if all_success:
        print("✅ All workers deployed successfully!")
        print("\n👉 Next step: Run benchmark.py to test performance")
    else:
        print("⚠️  Some workers failed to deploy")
        print("   Check errors above and fix before benchmarking")

if __name__ == "__main__":
    main()
