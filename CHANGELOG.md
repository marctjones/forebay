# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Placeholder for unreleased changes

## [1.0.0] - 2026-01-15

### Added
- Initial production release
- Google OAuth 2.0 authentication with JWT verification
- Session management with 30-day TTL
- FIFO queue operations (push, pull, stats, delete, list)
- Cross-platform CLI for Linux, Windows, and macOS
- Rust Cloudflare Worker backend with worker-rs
- C# client library (Forebay.Core) with HttpClient wrapper
- C# CLI application (Forebay.Cli) with System.CommandLine
- Comprehensive test suite (51 tests total)
  - Rust Worker: 25 unit tests
  - C# Client: 26 unit tests
- API documentation with OpenAPI 3.0 specification
- Developer documentation (CONTRIBUTING.md, DEVELOPMENT.md, ARCHITECTURE.md)
- Email allowlist for access control

### Technical Details
- Rust Worker performance: 107ms cold start, 88ms P95 latency
- Cloudflare KV storage for sessions and queues
- RESTful API with JSON payloads
- Bearer token authentication
- UUID-based session tokens
- Queue naming with hierarchical structure support
- Error handling with standardized error responses

### Documentation
- Comprehensive README.md with installation and usage
- OpenAPI 3.0 specification (docs/api/openapi.yaml)
- API documentation with examples (docs/api/README.md)
- Contributing guidelines (CONTRIBUTING.md)
- Development guide (docs/DEVELOPMENT.md)
- Architecture documentation (docs/ARCHITECTURE.md)

## [0.1.0] - 2026-01-04

### Added
- Phase 0: Language viability testing framework
- Rust Worker implementation for benchmarking
- TypeScript Worker implementation for benchmarking
- Python Worker implementation for benchmarking
- Performance benchmarking suite
- Automated deployment and testing scripts
- Results documentation (worker-viability-tests/RESULTS_FINAL.md)

### Research
- Evaluated Rust, TypeScript, Python, and .NET for Worker backend
- Evaluated Rust, Go, C#, and TypeScript for CLI client
- Selected Rust for Worker (best performance: 107ms cold start)
- Selected C# for Client (cross-platform support, future GUI)

### Decisions
- Cloudflare Workers as serverless platform
- Cloudflare KV for persistent storage
- Google OAuth for authentication
- REST API communication between client and worker
- Test-driven development with 80%+ coverage target

[Unreleased]: https://github.com/yourusername/forebay/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/yourusername/forebay/releases/tag/v1.0.0
[0.1.0]: https://github.com/yourusername/forebay/releases/tag/v0.1.0
