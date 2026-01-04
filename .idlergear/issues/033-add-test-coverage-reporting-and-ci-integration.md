---
id: 33
title: Add test coverage reporting and CI integration
state: open
created: '2026-01-04T08:57:14.014279Z'
labels:
- testing
- ci-cd
- infrastructure
priority: medium
---
Set up code coverage measurement and CI/CD integration for tests.

**Priority:** MEDIUM
**Type:** Test Infrastructure
**Component:** Project-wide

**Tasks:**

**1. Rust coverage:**
- Add tarpaulin or llvm-cov for coverage
- Configure coverage thresholds (80% minimum)
- Generate coverage reports
```bash
cargo tarpaulin --out Html --output-dir coverage
```

**2. C# coverage:**
- Add coverlet for coverage
- Configure coverage settings
```xml
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**3. GitHub Actions workflow:**
```yaml
name: Tests
on: [push, pull_request]
jobs:
  rust-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run Rust tests
        run: cd worker && cargo test
      - name: Generate coverage
        run: cargo tarpaulin
        
  csharp-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
      - name: Run tests
        run: dotnet test --collect:"XPlat Code Coverage"
```

**4. Coverage badges:**
- Add coverage badges to README
- Integrate with codecov.io or coveralls

**5. Pre-commit hooks:**
- Run tests before commit
- Fail if coverage drops below threshold

**Acceptance Criteria:**
- CI runs all tests on every PR
- Coverage reports generated automatically
- Coverage displayed in PR comments
- Tests must pass before merge
- Coverage threshold enforced (80%+)

**Documentation:**
- How to view coverage locally
- How to run specific test suites
- Coverage requirements explained
