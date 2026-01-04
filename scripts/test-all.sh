#!/bin/bash
set -e

# Run all tests for Forebay project

# Get project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$PROJECT_ROOT"

echo "========================================="
echo "Running Forebay Test Suite"
echo "========================================="
echo ""

# Track test results
RUST_TESTS_PASSED=false
CSHARP_TESTS_PASSED=false

# Run Rust Worker tests
echo "1. Testing Rust Worker..."
echo "-----------------------------------------"
if [ -d "worker" ]; then
  cd worker
  if cargo test; then
    RUST_TESTS_PASSED=true
    echo "✓ Rust Worker tests passed"
  else
    echo "✗ Rust Worker tests failed"
  fi
  cd "$PROJECT_ROOT"
else
  echo "⚠ worker/ directory not found"
fi

echo ""

# Run C# Client tests
echo "2. Testing C# Client..."
echo "-----------------------------------------"
if [ -d "client" ]; then
  cd client
  if dotnet test; then
    CSHARP_TESTS_PASSED=true
    echo "✓ C# Client tests passed"
  else
    echo "✗ C# Client tests failed"
  fi
  cd "$PROJECT_ROOT"
else
  echo "⚠ client/ directory not found"
fi

echo ""
echo "========================================="
echo "Test Summary"
echo "========================================="

if [ "$RUST_TESTS_PASSED" = true ]; then
  echo "✓ Rust Worker: PASSED"
else
  echo "✗ Rust Worker: FAILED"
fi

if [ "$CSHARP_TESTS_PASSED" = true ]; then
  echo "✓ C# Client: PASSED"
else
  echo "✗ C# Client: FAILED"
fi

echo "========================================="

# Exit with error if any tests failed
if [ "$RUST_TESTS_PASSED" = true ] && [ "$CSHARP_TESTS_PASSED" = true ]; then
  echo ""
  echo "All tests passed! ✓"
  exit 0
else
  echo ""
  echo "Some tests failed. Please fix before committing."
  exit 1
fi
