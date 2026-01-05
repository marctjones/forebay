---
id: 58
title: Implement config file management for CLI
state: open
created: '2026-01-04T23:56:11.367936Z'
labels:
- cli
- csharp
- configuration
priority: high
---
Implement cross-platform configuration file for storing session tokens.

**Implementation:**
1. Config file location:
   - Linux/macOS: `~/.config/forebay/config.json`
   - Windows: `%APPDATA%\\forebay\\config.json`
2. Store: worker_url, session_token
3. File permissions: 0600 (owner read/write only)
4. Implement save/load/clear methods

**Acceptance Criteria:**
- [ ] Cross-platform path resolution
- [ ] Secure file permissions
- [ ] JSON serialization
- [ ] Config directory created automatically
- [ ] Unit tests passing
