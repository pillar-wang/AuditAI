; -- AuditAI 安装脚本
; 使用 Inno Setup 6 编译

#define MyAppName "AuditAI"
#define MyAppVersion "1.2.0.0"
#define MyAppPublisher "AuditAI"
#define MyAppExeName "AuditAILauncher.exe"
#define MyAppMainExe "AuditAI.exe"
#define MySourceDir "..\AuditAI\bin\Release\net462"
#define MyOutputDir "Output"
#define DotNetInstaller "NDP462-KB3151800-x86-x64-AllOS-ENU.exe"

[Setup]
AppId={{440e6709-195f-4388-a4e8-5dec42eefca3}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir={#MyOutputDir}
OutputBaseFilename=AuditAI-Setup-{#MyAppVersion}
SetupIconFile=app.ico
Compression=lzma2/ultra
SolidCompression=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x86 x64compatible
WizardStyle=modern
WizardImageStretch=no
UninstallDisplayIcon={app}\app.ico
UninstallDisplayName={#MyAppName}
MinVersion=6.1sp1

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"; Flags: unchecked
Name: "quicklaunchicon"; Description: "创建快速启动栏快捷方式"; GroupDescription: "附加图标:"; Flags: unchecked

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.log,*.txt,logs\*,AuditAI采数器\Logs\*,Data\1\*,Data\Projects\*,Data\auditai_audit.db"
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#DotNetInstaller}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNet462Installed

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: quicklaunchicon

[Run]
Filename: "{tmp}\{#DotNetInstaller}"; Parameters: "/q /norestart"; StatusMsg: "正在安装 .NET Framework 4.6.2..."; Flags: runhidden; Check: not IsDotNet462Installed
Filename: "{app}\{#MyAppExeName}"; Description: "启动 {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\*.log"
Type: dirifempty; Name: "{app}\Data"
Type: dirifempty; Name: "{app}\config"

[Code]
function IsDotNet462Installed: Boolean;
var
  Release: Cardinal;
begin
  Result := False;
  if RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
  begin
    if Release >= 394802 then
      Result := True;
  end;
  if not Result then
  begin
    if RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
    begin
      if Release >= 394802 then
        Result := True;
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  if not IsDotNet462Installed then
  begin
    if MsgBox('运行 {#MyAppName} 需要 Microsoft .NET Framework 4.6.2。' + #13#10 + #13#10 +
              '是否自动安装？（安装包已内置）', mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;

function NeedRestart: Boolean;
begin
  Result := False;
end;
