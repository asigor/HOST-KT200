# HOST-KT200 Windows automation

This adds a Windows desktop control panel without replacing the existing `LocalHost` or `HOST-Server` components.

## Desktop panel

The panel can:
- start and stop the packaged `LocalHost` process;
- select a port and check the HTTP `/status` endpoint;
- activate a tool through the existing `/activate?tool=...&version=...` endpoint;
- run diagnostics for the packaged server, writable data directory, port availability and HTTP health;
- show server stdout/stderr and API responses.

Data is stored in `%LOCALAPPDATA%\HOST-KT200`. The server binds to loopback, as in the existing implementation.

## Build locally (Windows with .NET 8 SDK)

```powershell
dotnet build LocalHost/LocalHost.csproj -c Release
dotnet build Automation/HOST-KT200.Automation.csproj -c Release
dotnet publish Automation/HOST-KT200.Automation.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o artifacts/payload/app
dotnet publish LocalHost/LocalHost.csproj -c Release -r win-x64 --self-contained true -o artifacts/payload/server
```

Install Inno Setup 6, then run:

```powershell
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" installer/HOST-KT200.iss
```

The installer is written to `artifacts/HOST-KT200-Setup-win-x64.exe`.

## Automated verification

GitHub Actions builds both .NET projects, starts the published server in a temporary data directory, checks `/status`, `/tools`, `/config` and `/activate`, then packages the installer. The installer is uploaded as a workflow artifact.

The first CI run is the authoritative build/runtime verification. This development environment does not have the .NET SDK installed, so the Windows build and smoke tests cannot be honestly reported as passed until that workflow completes.
