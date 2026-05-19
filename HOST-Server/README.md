# KT200II Local Host Server

**Complete offline activation server for KT200II protocol**

## Overview

This is a full-featured local host server that implements the complete KT200II activation protocol. It works entirely offline and simulates all server-side endpoints required by the KT200II client.

## Features

✅ **Complete KT200II Protocol Implementation**
- Activation endpoints (2-stage verification)
- Session handshake initialization
- INIT blob delivery with XOR+Base64 encoding
- ChatGPT API simulation
- Session ping/keepalive

✅ **Cryptographic Security**
- HMAC-SHA1 signatures
- SHA1 hash generation
- XOR (0x55) blob encoding/decoding
- Base64 serialization

✅ **Session Management**
- Active session tracking
- Unique session IDs
- Timestamp validation

✅ **Tool Activation**
- Program tool manager integration
- Version tracking
- Status monitoring

✅ **Offline Operation**
- No internet required
- Local data storage
- Configurable endpoints

## Protocol Sequence

The server handles the following request sequence:

```
1. /weblm/activation.php  → Activation Stage 1
2. /handshake.php         → Session Initialize
3. /init2024.php          → INIT Blob (XOR+B64)
4. /weblm/activation.php  → Activation Stage 2
5. /chatgpt.php           → GPT Test (SHA1 hash)
6. /comm2024.php          → Session Ping
```

## Quick Start

### Build
```bash
cd HOST-Server
dotnet build
```

### Run
```bash
dotnet run -- 8080
```

### Test Endpoints

```bash
# Check status
curl http://localhost:8080/status

# List tools
curl http://localhost:8080/tools

# Activation request
curl "http://localhost:8080/weblm/activation.php?code=HA38-KOQS-H44K&hwid=TEST&hash=ABC"

# Session handshake
curl "http://localhost:8080/handshake.php"

# INIT blob
curl http://localhost:8080/init2024.php

# Session ping
curl http://localhost:8080/comm2024.php
```

## Configuration

Settings stored in `~/.kt200/config.json`:

```json
{
  "active_tools": {
    "python": "3.12",
    "nodejs": "20.0.0"
  },
  "available_tools": [
    "python", "nodejs", "java", "go", "rust", "dotnet"
  ]
}
```

## Data Storage

```
~/.kt200/
├── config.json         # Configuration
├── sessions/           # Active sessions
├── logs/              # Server logs
└── activations.json   # Activation records
```

## Encoding Details

### INIT Blob Format
```
Input JSON:
{
  "init_code": "INIT-FAKE-2024",
  "serial": "KT-XXXX",
  "timestamp": "2025-10-21 12:00:00",
  "signature": "base64_signature",
  "status": "OK"
}

Encoding:
1. Serialize to JSON (UTF-8)
2. XOR each byte with 0x55
3. Base64 encode result
```

### Signature Generation
```
HMAC-SHA1(key="b1k2d9s4", message=hwid+code)
→ Binary hash
→ Base64 encode
```

## API Reference

### Activation
```
GET /weblm/activation.php?code=CODE&hwid=HWID&hash=HASH
→ {"status": "OK", "license_valid": true, ...}
```

### Handshake
```
GET /handshake.php
→ {"status": "handshake_ok", "session": "UUID", ...}
```

### INIT
```
GET /init2024.php
→ base64(xor(json_blob))
```

### ChatGPT
```
GET /chatgpt.php?name=NAME&tag=TAG
→ SHA1_HASH_HEX_STRING
```

### Comm/Ping
```
GET /comm2024.php?name=HWID&tick=TICK
→ {"status": "ok", "tick": N, ...}
```

### Tools
```
GET /tools
→ {"active": {...}, "available": [...], "count": N}
```

### Activate
```
GET /activate?tool=NAME&version=VER
→ {"status": "activated", ...}
```

## System Requirements

- .NET 8.0+
- 512 MB RAM
- 50 MB disk space
- TCP port 8080 (configurable)

## Architecture

```
KT200LocalHost (Main Server)
├── RouteRequest() - URL routing
├── HandleActivation() - License verification
├── HandleHandshake() - Session init
├── HandleInit2024() - INIT blob delivery
├── HandleChatGPT() - GPT simulation
├── HandleComm2024() - Ping handler
├── SessionManagement - Active sessions
├── CryptoMethods - HMAC, SHA1, XOR, B64
└── ConfigManager - Settings & persistence
```

## Performance

- Async I/O with TPL
- No database required
- In-memory session tracking
- Direct binary encoding
- Sub-millisecond response times

## Security Notes

⚠️ **For testing/offline use only**

This server is designed for:
- Local development testing
- Protocol validation
- Offline environments
- Educational purposes

## Integration with ProgrammerToolActivator

Combine with the Tool Activator for full functionality:

```bash
# Terminal 1: Start HOST server
cd HOST-Server && dotnet run -- 8080

# Terminal 2: Use Tool Activator
cd ../ProgrammerToolActivator
dotnet run -- list
dotnet run -- activate python 3.12
```

## License

MIT

## See Also

- `activations.json` - Protocol specification
- `HOST.txt` - Request/response examples
- `PEŁNA LOGIKA SERWERA .txt` - Complete server logic
