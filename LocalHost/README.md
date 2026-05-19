# LOCAL HOST SERVER

A lightweight, offline local hosting server with integrated tool activation support.

## Features

✅ **Offline First** - Works completely offline, no internet required  
✅ **Tool Activation** - Manage and activate programming tools  
✅ **File Server** - Serve files from local directory  
✅ **Web Interface** - Access via simple HTTP endpoints  
✅ **Configuration** - JSON-based configuration system  
✅ **Cross-Platform** - Windows, macOS, Linux support  

## Quick Start

### Build
```bash
cd LocalHost
dotnet build
```

### Run
```bash
dotnet run -- --port 8080 --root ~/.localhost
```

### API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/status` | GET | Server status |
| `/tools` | GET | List active tools |
| `/activate?tool=NAME&version=VER` | GET | Activate a tool |
| `/config` | GET | Show configuration |
| `/files` | GET | Browse files |
| `/logs` | GET | View logs |

## Usage Examples

### Check Server Status
```bash
curl http://localhost:8080/status
```

### List Active Tools
```bash
curl http://localhost:8080/tools
```

### Activate Python 3.12
```bash
curl "http://localhost:8080/activate?tool=python&version=3.12"
```

### Browse Files
```bash
curl http://localhost:8080/files
```

## Directory Structure

```
~/.localhost/
├── tools/          # Tool definitions and metadata
├── data/           # Data storage
├── logs/           # Server logs
└── config.json     # Configuration file
```

## Configuration

Edit `~/.localhost/config.json`:

```json
{
  "port": 8080,
  "mode": "offline",
  "tools": ["python", "nodejs", "java", "go", "rust"]
}
```

## System Requirements

- .NET 8.0 or later
- 100 MB disk space
- 512 MB RAM minimum

## Supported Tools

- Python (3.11, 3.12)
- Node.js (18, 20)
- Java (17, 21)
- Go (1.22)
- Rust (1.75)

## License

MIT
