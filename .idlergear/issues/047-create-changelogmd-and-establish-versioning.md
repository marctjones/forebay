---
id: 47
title: Create CHANGELOG.md and establish versioning strategy
state: closed
created: '2026-01-04T09:25:28.842828Z'
labels:
- documentation
- versioning
- release
priority: medium
---
Set up changelog tracking and establish semantic versioning strategy for the project.

**Versioning Strategy:**

## Semantic Versioning (SemVer)

Format: `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking API changes
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, backward compatible

**Version Synchronization:**
- Worker and CLI versioned together
- Both share same version number
- Version compatibility documented

## CHANGELOG.md Structure

Follow Keep a Changelog format:

```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- New features that are unreleased

### Changed
- Changes in existing functionality

### Deprecated
- Soon-to-be removed features

### Removed
- Now removed features

### Fixed
- Bug fixes

### Security
- Security improvements

## [1.0.0] - 2026-01-15

### Added
- Initial release
- Google OAuth authentication
- FIFO queue operations (push, pull, stats, delete, list)
- Cross-platform CLI (Linux, Windows, macOS)
- Rust Cloudflare Worker backend
- C# client library
- Comprehensive test suite (51 tests)
- CI/CD pipeline with GitHub Actions

### Technical
- Rust Worker with 25 unit tests
- C# Client with 26 unit tests
- KV-based storage
- 30-day session TTL
- RESTful API

## [0.1.0] - 2026-01-04 (Phase 0 Complete)

### Added
- Language viability testing framework
- Rust, TypeScript, Python Worker comparisons
- Performance benchmarking
- Results documentation

[Unreleased]: https://github.com/user/forebay/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/user/forebay/releases/tag/v1.0.0
[0.1.0]: https://github.com/user/forebay/releases/tag/v0.1.0
```

## Version Tracking

**File Locations:**
- `worker/Cargo.toml`: `version = "1.0.0"`
- `client/Forebay.Cli/Forebay.Cli.csproj`: `<Version>1.0.0</Version>`
- `client/Forebay.Core/Forebay.Core.csproj`: `<Version>1.0.0</Version>`

**Automation:**
Create `scripts/bump-version.sh`:
```bash
#!/bin/bash
NEW_VERSION=$1

# Update Cargo.toml
sed -i "s/^version = .*/version = \"$NEW_VERSION\"/" worker/Cargo.toml

# Update csproj files
sed -i "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/" \
  client/Forebay.Cli/Forebay.Cli.csproj \
  client/Forebay.Core/Forebay.Core.csproj

echo "Updated version to $NEW_VERSION"
echo "Don't forget to update CHANGELOG.md!"
```

## Release Checklist

**Pre-release:**
- [ ] All tests passing
- [ ] CHANGELOG.md updated
- [ ] Version bumped in all files
- [ ] Documentation updated
- [ ] Breaking changes noted
- [ ] Migration guide (if needed)

**Release:**
- [ ] Create git tag: `git tag -a v1.0.0 -m "Release 1.0.0"`
- [ ] Push tag: `git push origin v1.0.0`
- [ ] CI/CD builds and deploys
- [ ] GitHub Release created
- [ ] Binaries uploaded
- [ ] Release notes from CHANGELOG

**Post-release:**
- [ ] Announce release
- [ ] Update documentation sites
- [ ] Monitor for issues

**Acceptance Criteria:**
- [ ] CHANGELOG.md created
- [ ] Version bump script works
- [ ] Release process documented
- [ ] Version numbers synchronized
- [ ] First release entry written
