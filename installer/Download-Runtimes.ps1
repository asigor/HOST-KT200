param(
    [Parameter(Mandatory = $true)]
    [string]$Destination
)

$ErrorActionPreference = "Stop"
New-Item -ItemType Directory -Path $Destination -Force | Out-Null

function Save-SignedInstaller {
    param([string]$Uri, [string]$Path, [string]$NamePattern, [string]$ExpectedPublisher)

    Write-Host "Downloading $NamePattern from official HTTPS source..."
    Invoke-WebRequest -Uri $Uri -OutFile $Path -MaximumRedirection 10
    $signature = Get-AuthenticodeSignature -FilePath $Path
    if ($signature.Status -ne "Valid") {
        throw "Authenticode signature is not valid for $Path (status: $($signature.Status))."
    }
    $subject = $signature.SignerCertificate.Subject
    if ($subject -notmatch $ExpectedPublisher) {
        throw "Unexpected signer for $NamePattern. Expected /$ExpectedPublisher/, got: $subject"
    }
    Write-Host "Verified Authenticode signature: $subject"
}

# Python: latest stable Windows x64 installer from python.org.
$pythonIndex = (Invoke-WebRequest -Uri "https://www.python.org/ftp/python/" -UseBasicParsing).Content
$pythonVersions = [regex]::Matches($pythonIndex, 'href="(3\.\d+\.\d+)/"') |
    ForEach-Object { $_.Groups[1].Value } |
    Sort-Object { [version]$_ } -Descending
$pythonVersion = $null
$pythonUri = $null
foreach ($version in $pythonVersions) {
    $candidate = "https://www.python.org/ftp/python/$version/python-$version-amd64.exe"
    try {
        $head = Invoke-WebRequest -Uri $candidate -Method Head -UseBasicParsing
        if ($head.StatusCode -eq 200) { $pythonVersion = $version; $pythonUri = $candidate; break }
    } catch { }
}
if (-not $pythonUri) { throw "Could not find a stable Python x64 installer on python.org." }
Save-SignedInstaller -Uri $pythonUri -Path (Join-Path $Destination "python-installer.exe") -NamePattern "Python $pythonVersion" -ExpectedPublisher "Python Software Foundation"
Write-Host "Bundled Python version: $pythonVersion"

# Node.js: current LTS Windows x64 MSI from nodejs.org.
$nodeEntries = Invoke-RestMethod -Uri "https://nodejs.org/dist/index.json"
$node = $nodeEntries | Where-Object { $_.lts -and $_.files -contains "win-x64-msi" } | Select-Object -First 1
if (-not $node) { throw "Could not find a Node.js LTS Windows x64 MSI." }
$nodeUri = "https://nodejs.org/dist/$($node.version)/node-$($node.version)-x64.msi"
Save-SignedInstaller -Uri $nodeUri -Path (Join-Path $Destination "node-installer.msi") -NamePattern "Node.js $($node.version)" -ExpectedPublisher "OpenJS Foundation|Node.js Foundation"
Write-Host "Bundled Node.js version: $($node.version)"

# Temurin JDK 21 LTS MSI from Eclipse Adoptium.
$javaUri = "https://api.adoptium.net/v3/installer/latest/21/ga/windows/x64/jdk/hotspot/normal/eclipse?project=jdk"
Save-SignedInstaller -Uri $javaUri -Path (Join-Path $Destination "java-installer.msi") -NamePattern "Eclipse Temurin JDK 21 LTS" -ExpectedPublisher "Eclipse Foundation"

# Go: latest stable Windows amd64 MSI from go.dev.
$goEntries = Invoke-RestMethod -Uri "https://go.dev/dl/?mode=json"
$go = $goEntries | Where-Object { $_.stable } | Where-Object { @($_.files | Where-Object { $_.os -eq "windows" -and $_.arch -eq "amd64" -and $_.kind -eq "installer" }).Count -gt 0 } | Select-Object -First 1
if (-not $go) { throw "Could not find a stable Go Windows amd64 MSI." }
$goFile = $go.files | Where-Object { $_.os -eq "windows" -and $_.arch -eq "amd64" -and $_.kind -eq "installer" } | Select-Object -First 1
$goUri = "https://go.dev/dl/$($goFile.filename)"
Save-SignedInstaller -Uri $goUri -Path (Join-Path $Destination "go-installer.msi") -NamePattern "$($go.version)" -ExpectedPublisher "Go Authors|Google LLC"
Write-Host "Bundled Go version: $($go.version)"

@"
Python=$pythonVersion
Node.js=$($node.version)
Java=Temurin JDK 21 LTS (latest at build time)
Go=$($go.version)
Rust=not bundled (official rustup requires downloading toolchain components)
"@ | Set-Content -Path (Join-Path $Destination "VERSIONS.txt") -Encoding utf8
