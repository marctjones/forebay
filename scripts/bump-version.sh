#!/bin/bash
set -e

# Version bump script for Forebay
# Updates version numbers in all project files

if [ -z "$1" ]; then
  echo "Usage: $0 <new-version>"
  echo "Example: $0 1.1.0"
  exit 1
fi

NEW_VERSION=$1

# Validate semantic version format
if ! [[ $NEW_VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
  echo "Error: Version must be in format MAJOR.MINOR.PATCH (e.g., 1.1.0)"
  exit 1
fi

echo "Bumping version to $NEW_VERSION..."

# Get project root (script is in scripts/ directory)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$PROJECT_ROOT"

# Update Rust Worker Cargo.toml
echo "Updating worker/Cargo.toml..."
if [ -f "worker/Cargo.toml" ]; then
  sed -i.bak "s/^version = \".*\"/version = \"$NEW_VERSION\"/" worker/Cargo.toml
  rm worker/Cargo.toml.bak
  echo "  ✓ Updated worker/Cargo.toml"
else
  echo "  ⚠ worker/Cargo.toml not found"
fi

# Update C# Client projects
echo "Updating C# project files..."

if [ -f "client/Forebay.Core/Forebay.Core.csproj" ]; then
  sed -i.bak "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" \
    client/Forebay.Core/Forebay.Core.csproj
  rm client/Forebay.Core/Forebay.Core.csproj.bak
  echo "  ✓ Updated client/Forebay.Core/Forebay.Core.csproj"
else
  echo "  ⚠ client/Forebay.Core/Forebay.Core.csproj not found"
fi

if [ -f "client/Forebay.Cli/Forebay.Cli.csproj" ]; then
  sed -i.bak "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" \
    client/Forebay.Cli/Forebay.Cli.csproj
  rm client/Forebay.Cli/Forebay.Cli.csproj.bak
  echo "  ✓ Updated client/Forebay.Cli/Forebay.Cli.csproj"
else
  echo "  ⚠ client/Forebay.Cli/Forebay.Cli.csproj not found"
fi

echo ""
echo "✓ Version updated to $NEW_VERSION in all project files"
echo ""
echo "Next steps:"
echo "  1. Update CHANGELOG.md with release notes"
echo "  2. Review changes: git diff"
echo "  3. Commit: git add . && git commit -m 'chore: bump version to $NEW_VERSION'"
echo "  4. Tag: git tag -a v$NEW_VERSION -m 'Release $NEW_VERSION'"
echo "  5. Push: git push origin main --tags"
echo ""
echo "Don't forget to update CHANGELOG.md!"
