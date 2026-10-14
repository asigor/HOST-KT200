# HOST-KT200 Windows automation

This adds a Windows desktop control panel without replacing the existing `LocalHost` or `HOST-Server` components.

## Desktop panel

The panel can:
- start and stop the packaged `LocalHost` process;
- select a port and check the HTTP `/status` endpoint;
- activate/deactivate supported tools in the LocalHost registry and inspect `/tools` (activation records a version; it does not itself install the tool);
- detect Python, Node.js, Java, Go and Rust in `PATH`;
- optionally install the selected tool via `winget` after explicit confirmation (this step needs network access and an available Windows App Installer);
- inspect configuration and log index, and run diagnostics for the packaged server, writable data directory, port availability and HTTP health;
- show server stdout/stderr and API responses.

Data is stored in `%USERPROFILE%\\.localhost`, reusing the existing LocalHost data directory. The server binds to loopback.

## Build locally (Windows with .NET 8 SDK)

```powershell
dotnet build Automation/HOST-KT200.Automation.csproj -c Release
dotnet build LocalHost/LocalHost.csproj -c Release
dotnet build HOST-Server/HOST-Server.csproj -c Release
dotnet publish Automation/HOST-KT200.Automation.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts/payload/app
dotnet publish LocalHost/LocalHost.csproj -c Release -r win-x64 --self-contained true -p:UseAppHost=true -o artifacts/payload/server
```

Install Inno Setup 6, then run:

```powershell
& "${env:ProgramFiles(x86)}\\Inno Setup 6\\ISCC.exe" installer/HOST-KT200.iss
```

The installer is written to `artifacts/HOST-KT200-Setup-win-x64.exe`.

## Automated verification

The Windows GitHub Actions workflow builds the desktop app and both existing server projects, starts the packaged LocalHost in a temporary data directory, and checks `/status`, `/tools`, `/config`, `/activate`, `/deactivate`, rejection of unsupported tools and rejection of path traversal. It then builds the Inno Setup installer and uploads it as an artifact.

The KT200 protocol server is build-checked but intentionally not bundled or invoked by this automation. Tool installation through winget requires network access; the LocalHost API's activation registry itself does not install runtimes.
