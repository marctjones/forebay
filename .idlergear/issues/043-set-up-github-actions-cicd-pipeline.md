---
id: 43
title: Set up GitHub Actions CI/CD pipeline
state: open
created: '2026-01-04T09:24:17.561958Z'
labels:
- ci-cd
- automation
- github-actions
priority: high
---
Create automated CI/CD pipeline using GitHub Actions for testing, building, and deploying.

**Workflows to Create:**

### 1. **CI Workflow** (`.github/workflows/ci.yml`)

**Triggers:**
- Push to main branch
- Pull requests
- Manual workflow dispatch

**Jobs:**

#### Rust Worker Tests
```yaml
jobs:
  test-worker:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions-rust-lang/setup-rust-toolchain@v1
      - name: Run tests
        run: cd worker && cargo test
      - name: Check formatting
        run: cd worker && cargo fmt --check
      - name: Run clippy
        run: cd worker && cargo clippy -- -D warnings
```

#### C# Client Tests
```yaml
  test-client:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - name: Restore dependencies
        run: dotnet restore client/
      - name: Build
        run: dotnet build client/ --no-restore
      - name: Run tests
        run: dotnet test client/ --no-build --verbosity normal
```

#### Test Coverage
```yaml
  coverage:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Generate Rust coverage
        run: |
          cargo install cargo-tarpaulin
          cd worker && cargo tarpaulin --out Xml
      - name: Generate C# coverage
        run: |
          cd client
          dotnet test --collect:"XPlat Code Coverage"
      - name: Upload to Codecov
        uses: codecov/codecov-action@v4
```

### 2. **Deploy Workflow** (`.github/workflows/deploy.yml`)

**Triggers:**
- Manual workflow dispatch
- Tags matching `v*.*.*`

**Jobs:**

#### Deploy Worker to Cloudflare
```yaml
jobs:
  deploy-worker:
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/checkout@v4
      - uses: actions-rust-lang/setup-rust-toolchain@v1
        with:
          target: wasm32-unknown-unknown
      - name: Deploy to Cloudflare
        env:
          CLOUDFLARE_API_TOKEN: ${{ secrets.CLOUDFLARE_API_TOKEN }}
        run: |
          npm install -g wrangler
          cd worker
          wrangler deploy
```

#### Build and Release CLI
```yaml
  build-cli:
    strategy:
      matrix:
        include:
          - os: ubuntu-latest
            target: linux-x64
          - os: windows-latest
            target: win-x64
          - os: macos-latest
            target: osx-x64
          - os: macos-latest
            target: osx-arm64
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - name: Build CLI
        run: |
          dotnet publish client/Forebay.Cli \
            -c Release \
            -r ${{ matrix.target }} \
            --self-contained \
            -p:PublishSingleFile=true \
            -o dist/${{ matrix.target }}
      - name: Create archive
        run: |
          cd dist/${{ matrix.target }}
          tar -czf ../forebay-${{ matrix.target }}.tar.gz forebay
      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: forebay-${{ matrix.target }}
          path: dist/forebay-${{ matrix.target }}.tar.gz
```

### 3. **Release Workflow** (`.github/workflows/release.yml`)

**Triggers:**
- Tags matching `v*.*.*`

**Jobs:**
- Build all platform binaries
- Generate checksums
- Create GitHub Release
- Upload artifacts to release

**Secrets Required:**
- `CLOUDFLARE_API_TOKEN` - For Worker deployment
- `CLOUDFLARE_ACCOUNT_ID` - Account ID

**Acceptance Criteria:**
- [ ] CI runs on all PRs
- [ ] All tests must pass before merge
- [ ] Deployment requires manual approval
- [ ] Build artifacts uploaded for releases
- [ ] Code coverage tracking enabled
- [ ] Workflows documented in README
- [ ] Status badges added to README

**Future Enhancements:**
- Staging environment deployment on PRs
- Integration test run in CI
- Automated changelog generation
- Dependency updates (Dependabot)
