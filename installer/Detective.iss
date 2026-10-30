; Inno Setup script for the Windows installer: Temnorid-Detective-Setup.exe
; Build the player first (Unity: Temnorid > Detective > Build Windows Player), then compile this with Inno Setup:
;   "ISCC.exe" installer\Detective.iss
; The installer is written to Builds\Installer.

#define AppName "Temnorid Detective"
#define AppVersion "0.1.0"
#define AppExe "TemnoridDetective.exe"

[Setup]
AppId={{9A3C5E71-4B28-4D6F-8E10-7C2B9D4A6F33}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Temnorid
AppPublisherURL=https://detective.temnorid.com
DefaultDirName={autopf}\Temnorid Detective
DefaultGroupName=Temnorid Detective
DisableProgramGroupPage=yes
OutputDir=..\Builds\Installer
OutputBaseFilename=Temnorid-Detective-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#AppExe}
WizardStyle=modern

[Files]
Source: "..\Builds\Windows\TemnoridDetective\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Run]
Filename: "{app}\{#AppExe}"; Description: "Start {#AppName}"; Flags: nowait postinstall skipifsilent
