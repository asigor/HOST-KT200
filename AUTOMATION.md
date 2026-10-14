# HOST-KT200 Windows automation

This adds a Windows desktop control panel without replacing the existing `LocalHost` or `HOST-Server` components.

## Desktop panel

The panel can:
- start and stop the packaged `LocalHost` process;
- select a port and check the HTTP `/status` endpoint;
- activate/deactivate supported tools in the LocalHost registry and inspect `/tools` (activation records a version; it does not itself install the tool);\n- detect Python, Node.js, Java, Go and Rust in `PATH`;\n- optionally install the selected tool via `winget` after an explicit confirmation (this step needs network access and an available Windows App Installer);
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

GitHub Actions builds the desktop app and both existing server projects, runs API smoke tests for LocalHost (including rejection of unsupported tools and path traversal), and packages the desktop app plus LocalHost server. The KT200 protocol server is build-checked but intentionally not bundled or invoked by this automation. The installer artifact is available only after a fully successful workflow.
