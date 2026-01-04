---
id: 4
title: Add token management commands
state: open
created: '2026-01-04T06:18:40.702970Z'
labels:
- enhancement
- auth
priority: medium
---
Implement CLI commands for token management:

Commands to add:
- `forebay whoami` - Show current authenticated email
- `forebay logout` - Delete local session token
- `forebay token create` - Generate static token (admin only)

Implementation:
- Read session token from config.toml
- Query Worker for session info (GET /auth/whoami)
- Display email and expiry
- For logout, just delete from config file
- For token create, add Worker endpoint that generates UUID and returns it

Reference: See "Authentication Implementation Guide" wiki page
