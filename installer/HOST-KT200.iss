#define AppName "HOST-KT200 Automation"
#define AppVersion "1.0.0"
#define AppPublisher "HOST-KT200"
#define AppExeName "HOST-KT200.Automation.exe"

[Setup]
AppId={{C861BDE1-8B5A-4E52-9AF0-6C53EACB67F1}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\HOST-KT200
DefaultGroupName=HOST-KT200
OutputDir=..\artifacts
OutputBaseFilename=HOST-KT200-Setup-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppExeName}

[Files]
Source: "..\artifacts\payload\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\artifacts\payload\server\*"; DestDir: "{app}\server"; Flags: ignoreversion recursesubdirs createallsubdirs

Source: "..\artifacts\payload\installers\python-installer.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Components: python
Source: "..\artifacts\payload\installers\node-installer.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Components: node
Source: "..\artifacts\payload\installers\java-installer.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Components: java
Source: "..\artifacts\payload\installers\go-installer.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Components: go
Source: "..\artifacts\payload\installers\VERSIONS.txt"; DestDir: "{app}\tools"; Flags: ignoreversion

[Components]
Name: "python"; Description: "Python (instalator offline)"; Types: full
Name: "node"; Description: "Node.js LTS (instalator offline)"; Types: full
Name: "java"; Description: "Eclipse Temurin JDK 21 LTS (instalator offline)"; Types: full
Name: "go"; Description: "Go (instalator offline)"; Types: full

[Icons]
Name: "{autoprograms}\HOST-KT200 Automation"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\HOST-KT200 Automation"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Utwórz skrót na pulpicie"; GroupDescription: "Dodatkowe skróty:"

[Run]
Filename: "{tmp}\python-installer.exe"; Parameters: "/quiet InstallAllUsers=1 PrependPath=1 Include_test=0"; StatusMsg: "Instalowanie Python..."; Components: python; Flags: runhidden waituntilterminated
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\node-installer.msi"" /qn /norestart"; StatusMsg: "Instalowanie Node.js..."; Components: node; Flags: runhidden waituntilterminated
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\java-installer.msi"" /qn /norestart ADDLOCAL=FeatureMain,FeatureEnvironment,FeatureJarFileRunWith,FeatureJavaHome"; StatusMsg: "Instalowanie Java JDK..."; Components: java; Flags: runhidden waituntilterminated
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\go-installer.msi"" /qn /norestart"; StatusMsg: "Instalowanie Go..."; Components: go; Flags: runhidden waituntilterminated
Filename: "{app}\{#AppExeName}"; Description: "Uruchom HOST-KT200 Automation"; Flags: postinstall nowait skipifsilent
