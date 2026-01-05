---
id: 63
title: Implement session management and listing
state: open
created: '2026-01-05T00:17:04.324117Z'
labels:
- auth
- security
- cli
- worker
- enhancement
priority: high
---
Add ability to view and manage active sessions across devices/apps.

**Problem:** Users need visibility into:
- Which devices/apps are logged in
- When sessions were created
- Which sessions are still active
- Ability to revoke compromised sessions

**Use Cases:**
1. **List active sessions:** See all logged-in devices
2. **Revoke specific session:** Remote logout from lost device
3. **Revoke all sessions:** Security incident response
4. **Session metadata:** Device name, last used, creation time

## Implementation

### Enhanced Session Storage

**Current:**
```json
{
  "email": "user@example.com",
  "created_at": 1609459200000,
  "expires_at": 1612137600000
}
```

**Enhanced:**
```json
{
  "email": "user@example.com",
  "created_at": 1609459200000,
  "expires_at": 1612137600000,
  "device_name": "MacBook Pro",
  "device_id": "uuid-device-fingerprint",
  "last_used_at": 1609459300000,
  "ip_address": "192.168.1.1",
  "user_agent": "forebay-cli/1.0.0"
}
```

### API Endpoints

**GET /auth/sessions**
- List all active sessions for authenticated user
- Returns array of session metadata
- Requires valid session token

**DELETE /auth/sessions/{session_id}**
- Revoke specific session by ID
- Can't revoke own session (use logout instead)
- Requires valid session token

**DELETE /auth/sessions/all**
- Revoke all sessions except current
- Security feature for compromised account
- Requires valid session token

### CLI Commands

```bash
# List active sessions
forebay sessions list
# Output:
# DEVICE           CREATED        LAST USED    STATUS
# MacBook Pro      2 days ago     5 min ago    current
# Ubuntu Server    1 week ago     1 hour ago   active
# Windows Desktop  2 weeks ago    3 days ago   active

# Revoke specific session
forebay sessions revoke <session-id>

# Revoke all other sessions (keep current)
forebay sessions revoke-all

# Show current session details
forebay whoami --verbose
# Output:
# Email: user@example.com
# Session ID: 550e8400-e29b-41d4-a716-446655440000
# Device: MacBook Pro
# Created: 2 days ago
# Expires: 28 days from now
# Last Used: 5 minutes ago
```

### Device Identification

**During login, CLI captures:**
```csharp
var deviceInfo = new DeviceInfo
{
    Name = Environment.MachineName,  // "MacBook-Pro"
    Platform = RuntimeInformation.OSDescription,  // "macOS 14.0"
    CliVersion = "1.0.0",
    DeviceId = GetOrCreateDeviceId()  // Persistent UUID per device
};
```

**Device ID storage:** `~/.config/forebay/device_id` (persists across logins)

### Worker Implementation

**Session Storage Pattern:**
```
Key: session:{uuid}
Value: SessionData (with device info)

Secondary Index:
Key: user_sessions:{email}
Value: [session_id1, session_id2, ...]
```

**List sessions:**
1. Get user_sessions:{email} → array of session IDs
2. Batch fetch each session:{uuid}
3. Filter expired sessions
4. Return metadata array

**Revoke session:**
1. Delete session:{uuid} from KV
2. Remove from user_sessions:{email} array
3. Return success

### Security Considerations

**Best Practices:**
- ✅ Session tokens never logged or displayed (only first 8 chars)
- ✅ Device names are user-visible metadata (not secret)
- ✅ IP addresses stored for audit trail
- ✅ Last-used timestamp updated on each request
- ✅ Expired sessions automatically cleaned by KV TTL

**Privacy:**
- Device info stored only for user's own sessions
- Only user can view their own sessions
- No admin access to user sessions (privacy-first)

**Rate Limiting:**
- Revoke operations limited to prevent DoS
- Sessions list cached briefly to reduce KV reads

## Tasks

- [ ] Update SessionData model with device metadata
- [ ] Implement GET /auth/sessions endpoint
- [ ] Implement DELETE /auth/sessions/{id} endpoint
- [ ] Implement DELETE /auth/sessions/all endpoint
- [ ] Add secondary index for user sessions
- [ ] Implement device fingerprinting in CLI
- [ ] Add `forebay sessions list` command
- [ ] Add `forebay sessions revoke` command
- [ ] Add `forebay sessions revoke-all` command
- [ ] Update `forebay whoami` with --verbose flag
- [ ] Add session metadata to login flow
- [ ] Update last_used_at on each request
- [ ] Write unit tests for session management
- [ ] Write integration tests for revoke operations
- [ ] Document session management in README

## Acceptance Criteria

- [ ] Users can list all active sessions
- [ ] Users can revoke specific sessions
- [ ] Users can revoke all sessions (emergency)
- [ ] Device names visible in session list
- [ ] Last-used timestamp updates correctly
- [ ] Expired sessions don't appear in list
- [ ] Can't revoke someone else's sessions
- [ ] Session revoke is immediate (not cached)
- [ ] Documentation includes session management guide

## User Experience

**Simple & Seamless:**
- Login once per device → works forever (30 days)
- Auto-renewal: Session extends on each use
- No re-authentication needed for normal usage

**Security:**
- Full visibility into active sessions
- Quick revoke if device lost/stolen
- Emergency "revoke all" for security incidents

**Files:** 
- worker/src/auth.rs
- worker/src/models.rs  
- client/Forebay.Cli/Commands/SessionsCommand.cs
