---
id: 40
title: Build and package CLI for cross-platform distribution
state: open
created: '2026-01-04T09:23:20.920441Z'
labels:
- cli
- build
- packaging
priority: medium
---
Create build and packaging configuration for distributing the Forebay CLI as standalone binaries.

**Target Platforms:**
1. Linux x64 (Ubuntu 22.04+)
2. Windows x64 (Windows 10+)
3. macOS x64 (Intel)
4. macOS arm64 (Apple Silicon)

**Build Configurations:**

### Self-Contained Builds

Use .NET's single-file publish feature:

```bash
# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o dist/linux-x64

# Windows x64
dotnet publish -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o dist/win-x64

# macOS x64
dotnet publish -c Release -r osx-x64 --self-contained \
  -p:PublishSingleFile=true \
  -o dist/osx-x64

# macOS arm64
dotnet publish -c Release -r osx-arm64 --self-contained \
  -p:PublishSingleFile=true \
  -o dist/osx-arm64
```

**Build Script:**

Create `build.sh` and `build.ps1` scripts:
- Build all platforms
- Create archives (tar.gz for Unix, zip for Windows)
- Calculate SHA256 checksums
- Output to `dist/` directory

**Acceptance Criteria:**
- [ ] Single binary per platform
- [ ] No .NET runtime required
- [ ] Binaries under 50MB each
- [ ] Build scripts work on Linux and Windows
- [ ] Archives include README and license
- [ ] Checksums generated for verification
- [ ] Test binaries on all target platforms

**Future Enhancements:**
- Code signing for Windows/macOS
- Package managers (apt, brew, chocolatey)
- Auto-update mechanism
