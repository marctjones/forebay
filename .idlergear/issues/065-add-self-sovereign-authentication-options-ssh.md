---
id: 65
title: Add self-sovereign authentication options (SSH keys, API keys, passkeys)
state: open
created: '2026-01-05T00:29:17.071855Z'
labels:
- auth
- enhancement
- security
- privacy
priority: medium
---
Enable authentication methods that don't require third-party OAuth providers.

**Problem:** OAuth (Google, GitHub, Microsoft) creates dependency on big tech:
- Requires internet to their services
- Privacy concerns (tracking)
- Service outages affect authentication
- Not suitable for air-gapped environments

**Use Cases:**
1. **Privacy-focused users:** No tracking by Google/Microsoft
2. **Air-gapped deployments:** No internet to OAuth providers
3. **Server-to-server:** Scripts/automation don't need browser OAuth
4. **Developer workflow:** SSH key already on machine
5. **Enterprise:** Existing PKI infrastructure

## Authentication Options (No OAuth Required)

### 1. SSH Key Authentication ⭐ Recommended

**How it works:**
```bash
# One-time setup: Register your SSH public key
forebay register-key ~/.ssh/id_ed25519.pub

# Every request: Sign with private key
forebay push work/tasks '{"data":"..."}' --sign
# CLI signs request with SSH key, Worker verifies signature
```

**Benefits:**
- ✅ No OAuth dependency
- ✅ Developers already have SSH keys
- ✅ Strong cryptographic authentication
- ✅ Works offline (no OAuth roundtrip)
- ✅ Scriptable (no browser needed)
- ✅ Battle-tested (SSH protocol)

**Implementation:**
```rust
// Worker verifies Ed25519 signature
pub struct SshKeyAuth {
    public_key: String,  // ssh-ed25519 AAAA...
    user_id: String,     // email or username
}

// Storage: public_key:{fingerprint} -> user_id
// Allowlist: ALLOWED_KEY_FINGERPRINTS
```

**CLI:**
```bash
# Register key (one-time)
forebay register-key ~/.ssh/id_ed25519.pub
# Sends public key to Worker, stores in KV

# Every request includes signature
forebay push work/tasks '{"data":"..."}'
# CLI signs: HMAC-SHA256(timestamp + endpoint + body, private_key)
# Worker verifies using stored public key
```

**Pros:**
- Zero OAuth dependencies
- Fast (no network roundtrip)
- Strong security (Ed25519)
- Familiar to developers

**Cons:**
- Initial key registration still needs auth (chicken-egg)
- Public key storage in KV

### 2. Static API Keys (Like GitHub Personal Access Tokens)

**How it works:**
```bash
# Generate API key via Worker
forebay token create --name "my-server" --expires 90d
# Returns: fbk_1234567890abcdef...

# Use API key for all requests
export FOREBAY_API_KEY=fbk_1234567890abcdef...
forebay push work/tasks '{"data":"..."}'
```

**Benefits:**
- ✅ No OAuth needed after initial setup
- ✅ Perfect for scripts/automation
- ✅ Can create multiple keys (dev, prod, CI/CD)
- ✅ Revocable per-key
- ✅ Expiration dates

**Implementation:**
```rust
pub struct ApiKey {
    key: String,           // fbk_ prefix (Forebay Key)
    user_id: String,
    name: String,          // "Production Server"
    created_at: i64,
    expires_at: Option<i64>,
    scopes: Vec<String>,   // ["queue:push", "queue:pull"]
}

// Storage: api_key:{hash} -> ApiKey
```

**Security:**
```
Key format: fbk_random_32_bytes
Storage: HMAC-SHA256(key) -> user data
Never store plaintext keys
```

**Pros:**
- Simple to implement
- Familiar pattern (like GitHub PATs)
- Great for automation
- Per-key permissions possible

**Cons:**
- Static secret (if leaked, permanent access until revoked)
- Initial creation still needs OAuth

### 3. WebAuthn/Passkeys (FIDO2)

**How it works:**
```bash
# Register hardware key (YubiKey, TouchID, Windows Hello)
forebay register-passkey
# Prompts for biometric/PIN, stores credential

# Login with passkey
forebay login
# Prompts for biometric/PIN, no OAuth needed
```

**Benefits:**
- ✅ Phishing-resistant
- ✅ No passwords/OAuth needed
- ✅ Backed by device hardware
- ✅ Modern security standard
- ✅ Works with YubiKey, TouchID, Windows Hello

**Implementation Challenges:**
- ⚠️ Complex protocol (WebAuthn)
- ⚠️ Requires HTTPS (works with Cloudflare ✅)
- ⚠️ Initial registration needs existing auth
- ⚠️ Browser/platform support varies

**Best for:**
- High-security environments
- Hardware token users (YubiKey)
- Modern enterprises

### 4. Time-based One-Time Passwords (TOTP)

**How it works:**
```bash
# Setup (one-time)
forebay setup-totp
# Shows QR code, scan with authenticator app

# Login
forebay login
# Prompts for 6-digit code from app
```

**Benefits:**
- ✅ No OAuth needed
- ✅ Works offline
- ✅ Familiar (like 2FA)
- ✅ App-based (Google Authenticator, Authy)

**Cons:**
- ❌ Still needs initial secret exchange (QR code)
- ❌ Codes expire quickly (30s)
- ❌ Not as convenient for CLI

### 5. mTLS (Mutual TLS Client Certificates)

**How it works:**
```bash
# Generate client certificate
forebay gen-cert --output ~/.forebay/client.pem

# Register certificate
forebay register-cert ~/.forebay/client.pem

# All requests use client cert for auth
forebay push work/tasks '{"data":"..."}'
# TLS handshake includes client cert
```

**Benefits:**
- ✅ Enterprise PKI-friendly
- ✅ Automatic with TLS connection
- ✅ Very strong security
- ✅ No additional auth headers needed

**Challenges:**
- ⚠️ Cloudflare Workers support for mTLS client certs
- ⚠️ Certificate management complexity
- ⚠️ Requires PKI infrastructure

## Recommended Approach

### Phase 1 (v1.0): OAuth-based
- Google OAuth (current plan)
- Simple to implement
- Works for most users

### Phase 2 (v1.1): Add API Keys
- Static API keys for automation
- `forebay token create` command
- Great for scripts/CI-CD
- No OAuth after initial setup

### Phase 3 (v1.2): Add SSH Key Auth
- `forebay register-key` command
- Sign requests with SSH key
- Best for developers
- Zero OAuth dependency after setup

### Phase 4 (v1.3+): Advanced Options
- WebAuthn/Passkeys (hardware keys)
- TOTP (authenticator apps)
- mTLS (enterprise PKI)

## Hybrid Approach: Bootstrap Problem Solution

**Problem:** How to register SSH key/API key without OAuth?

**Solutions:**

### Option A: OAuth Bootstrap (Pragmatic)
```bash
# One-time OAuth login to register key
forebay login                        # Google OAuth
forebay register-key ~/.ssh/id_ed25519.pub
forebay logout

# Forever after: Use SSH key (no OAuth)
forebay push work/tasks '{"data":"..."}'  # SSH signature
```

### Option B: Admin Pre-registration
```bash
# Admin adds public keys via wrangler secrets
wrangler secret put ALLOWED_SSH_KEYS
# Paste: ssh-ed25519 AAAA... user@example.com

# User can immediately use SSH key
forebay push work/tasks --key ~/.ssh/id_ed25519
```

### Option C: Self-Registration URL
```
# Admin generates registration token
wrangler secret put REGISTRATION_TOKEN

# User registers via Web UI (no OAuth)
https://forebay.skpt.cl/register?token=...
# Paste SSH public key, submit
```

## Configuration Examples

### API Key Auth:
```toml
[env.production.vars]
AUTH_METHODS = "oauth,apikey"  # Multiple methods allowed
ALLOWED_EMAILS = "admin@example.com"  # For OAuth
```

### SSH Key Auth:
```toml
[env.production.vars]
AUTH_METHODS = "oauth,sshkey"
ALLOWED_SSH_KEY_FINGERPRINTS = "SHA256:abc123...,SHA256:def456..."
```

### API Key Only (No OAuth):
```toml
[env.production.vars]
AUTH_METHODS = "apikey"
# Pre-register keys via admin interface
```

## Security Comparison

| Method | Dependency | Scriptable | Phishing-Resistant | Revocable |
|--------|------------|------------|-------------------|-----------|
| OAuth (Google) | Google.com | ❌ | ✅ | ✅ |
| SSH Keys | None | ✅ | ✅ | ✅ |
| API Keys | None | ✅ | ❌ | ✅ |
| Passkeys | None | ❌ | ✅ | ✅ |
| TOTP | None | ⚠️ | ❌ | ✅ |
| mTLS | PKI | ✅ | ✅ | ✅ |

**Best for privacy:** SSH keys or mTLS
**Best for automation:** API keys or SSH keys
**Best for security:** Passkeys or mTLS
**Easiest to implement:** API keys

## Recommendation

**v1.0:** OAuth (Google) - Get to production fast
**v1.1:** Add API keys (#65) - Enable automation
**v1.2:** Add SSH key auth (#66) - Zero big-tech dependency
**v1.3+:** Add passkeys/TOTP - Advanced security

This gives you a migration path from OAuth-dependent to fully self-sovereign authentication while shipping v1.0 quickly.

## Tasks

**v1.1 - API Keys:**
- [ ] Create ApiKey model
- [ ] Implement `POST /auth/tokens` (create API key)
- [ ] Implement `GET /auth/tokens` (list keys)
- [ ] Implement `DELETE /auth/tokens/{id}` (revoke)
- [ ] Add `forebay token create` CLI command
- [ ] Add `forebay token list` CLI command
- [ ] Add `forebay token revoke` CLI command
- [ ] Support `FOREBAY_API_KEY` environment variable
- [ ] Key storage and hashing
- [ ] Key expiration

**v1.2 - SSH Key Auth:**
- [ ] Create SshKey model
- [ ] Implement `POST /auth/ssh-keys` (register key)
- [ ] Implement request signature verification
- [ ] Add `forebay register-key` CLI command
- [ ] Add SSH signature to all requests
- [ ] Support `--key` flag for key selection
- [ ] Admin pre-registration support

**v1.3+ - Advanced:**
- [ ] WebAuthn/FIDO2 support
- [ ] TOTP setup and verification
- [ ] mTLS client certificate support

## References

- **SSH Keys:** https://en.wikipedia.org/wiki/Secure_Shell
- **WebAuthn:** https://webauthn.guide/
- **TOTP:** https://datatracker.ietf.org/doc/html/rfc6238
- **mTLS:** https://developers.cloudflare.com/cloudflare-one/identity/devices/mutual-tls-authentication/
