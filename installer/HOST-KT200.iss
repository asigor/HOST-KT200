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

[Icons]
Name: "{autoprograms}\HOST-KT200 Automation"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\HOST-KT200 Automation"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Utwórz skrót na pulpicie"; GroupDescription: "Dodatkowe skróty:"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Uruchom HOST-KT200 Automation"; Flags: postinstall nowait skipifsilent
