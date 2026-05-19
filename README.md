# HOST-KT200

**Local Offline Development Environment & Tool Activation System**

## 🎯 Overview

HOST-KT200 is a comprehensive offline-first development toolkit designed for local environments where internet connectivity may be limited or unavailable. It combines a **Local Host Server** with **Programmer Tool Activation** capabilities.

## 📦 Components

### 1. **Local Host Server** (`LocalHost/`)

A lightweight HTTP server that runs offline with:
- ✅ Tool activation and management
- ✅ File serving capabilities
- ✅ JSON-based API
- ✅ Configuration management
- ✅ System status monitoring

**Quick Start:**
```bash
cd LocalHost
dotnet build
dotnet run -- --port 8080
```

**Access:** `http://localhost:8080`

### 2. **Programmer Tool Activator** (`ProgrammerToolActivator/`)

A CLI tool for managing programming tool environments:
- ✅ Activate/deactivate tools
- ✅ Manage versions
- ✅ View configurations
- ✅ Environment tracking

**Quick Start:**
```bash
cd ProgrammerToolActivator
dotnet build
dotnet run -- list
dotnet run -- activate python 3.12
```

## 🛠️ Technology Stack

- **Language:** C# (99.3%) + Python (0.7%)
- **Runtime:** .NET 8.0
- **Platform:** Windows, macOS, Linux
- **Mode:** Fully Offline

## 🚀 Features

- 🔌 **No Internet Required** - Complete offline operation
- 🎛️ **Tool Management** - Activate multiple tools simultaneously
- 📁 **File Server** - Serve local files over HTTP
- ⚙️ **Configuration** - JSON-based settings
- 📊 **Status Monitoring** - Real-time server status
- 🔒 **Isolated Environment** - Dedicated local directory

## 📋 API Endpoints

```
GET  /status          → Server status
GET  /tools           → Active tools list
GET  /activate        → Activate tool
GET  /config          → Configuration
GET  /files           → Browse files
GET  /logs            → Server logs
```

## 💾 Data Storage

All data stored locally in `~/.localhost/`:
```
~/.localhost/
├── tools/          # Tool metadata
├── data/           # User data
├── logs/           # Server logs
└── config.json     # Configuration
```

## 🎓 Learning Resources

Both components are designed as learning projects:
- Clear C# best practices
- Async/await patterns
- HTTP server implementation
- CLI application development
- Configuration management

## 📄 License

MIT

## 🤝 Contributing

Fork → Branch → Commit → Push → PR

---

**Status:** ✅ Ready to use  
**Mode:** 🔌 Offline  
**Version:** 1.0.0
