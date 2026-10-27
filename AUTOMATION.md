# HOST-KT200 Windows automation

This adds a Windows desktop control panel without replacing the existing `LocalHost` or `HOST-Server` components.

## Desktop panel

The panel can:
- start and stop the packaged `LocalHost` process;
- select a port and check the HTTP `/status` endpoint;
- activate/deactivate supported tools in the LocalHost registry and inspect `/tools` (activation records a version; it does not itself install the tool);
- detect Python, Node.js, Java, Go and Rust in `PATH`;
- install the selected tool via `winget` after explicit confirmation when it is not bundled (Rust still requires this path and internet access);
- inspect configuration and log index, and run diagnostics for the packaged server, writable data directory, port availability and HTTP health;
- show server stdout/stderr and API responses;
- select and launch an already-installed KT200 executable from the same desktop panel, starting the LocalHost management service first and checking its health.

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

The installer downloads the current official Python, Node.js LTS, Eclipse Temurin JDK 21 LTS, and Go Windows installers during the Windows CI build, checks that each file has a valid Authenticode signature, and includes them as optional components. Selecting these components installs them during setup without requiring a network connection on the target PC. Versions are recorded in `tools/VERSIONS.txt`; the bundle is refreshed at build time rather than pinned to a fixed release. Rust is not bundled because the official rustup bootstrapper downloads toolchain components; Rust installation through winget still requires internet access.

The launcher starts the LocalHost management service and then launches the user-selected KT200 executable. It deliberately does not rewrite DNS/hosts/proxy settings, intercept TLS, or redirect license/activation traffic. The existing `HOST-Server` contains simulated license/activation responses and is build-checked but neither bundled nor invoked by the launcher. LocalHost tool activation is a metadata registry, not a replacement for a vendor license system.
