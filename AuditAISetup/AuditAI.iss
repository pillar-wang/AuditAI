; -- AuditAI 安装脚本
; 使用 Inno Setup 6 编译

#define MyAppName "AuditAI"
#define MyAppVersion "1.3.0.0"
#define MyAppPublisher "AuditAI"
#define MyAppExeName "AuditAILauncher.exe"
#define MyAppMainExe "AuditAI.exe"
#define MySourceDir "..\AuditAI\bin\Release\net48"
#define MyLauncherSourceDir "..\AuditAILauncher\bin\Release\net40"
#define MyMcpServerSourceDir "..\AuditAI.McpServer\bin\Release\net462"
#define MyOutputDir "Output"
#define DotNetInstaller "ndp48-x86-x64-all-os-enu.exe"

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
Name: "mcp_register"; Description: "为 Claude Desktop / Cursor / VS Code 注册 MCP 服务器"; GroupDescription: "AI Agent 配置:"; Flags: unchecked

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.log;*.txt;logs\*;Data\1\*;Data\Projects\*;Data\auditai_audit.db;AuditAI采数器\Logs\*"
Source: "{#MyLauncherSourceDir}\AuditAILauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyMcpServerSourceDir}\*"; DestDir: "{app}\McpServer"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#DotNetInstaller}"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNet48Installed

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\app.ico"; Tasks: quicklaunchicon
Name: "{group}\AuditAI MCP 注册工具"; Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\McpServer\Register-MCP.ps1"""; IconFilename: "{app}\app.ico"

[Run]
Filename: "{tmp}\{#DotNetInstaller}"; Parameters: "/q /norestart"; StatusMsg: "正在安装 .NET Framework 4.8..."; Flags: runhidden; Check: not IsDotNet48Installed
Filename: "{app}\{#MyAppExeName}"; Description: "启动 {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
{ ============================================================
  全局常量
============================================================ }
const
  MCP_SERVER_DIR  = 'McpServer';
  MCP_SERVER_EXE  = 'AuditAI.McpServer.exe';

{ ============================================================
  辅助函数：获取路径
============================================================ }
function McpServerDir: string;
begin
  Result := ExpandConstant('{app}\' + MCP_SERVER_DIR);
end;

function McpServerExe: string;
begin
  Result := McpServerDir + '\' + MCP_SERVER_EXE;
end;

{ ============================================================
  辅助函数：以 UTF-8 BOM 编码写入文件
  PowerShell 5.1 的 ParseFile 在无 BOM 时使用系统默认编码（中文系统为 GBK），
  会导致 UTF-8 中文注释被误解析。写入 BOM 后 PS 能正确识别为 UTF-8。
============================================================ }

procedure SaveUtf8FileWithBom(const FilePath: string; const Content: string);
var
  Lines: TArrayOfString;
begin
  SetLength(Lines, 1);
  Lines[0] := Content;
  SaveStringsToUTF8File(FilePath, Lines, False);
end;

{ ============================================================
  辅助函数：JSON 字符串转义
============================================================ }

function JsonEscape(s: string): string;
begin
  Result := s;
  StringChange(Result, '\', '\\');
  StringChange(Result, '"', '\"');
end;

{ ============================================================
  辅助函数：PowerShell 字符串转义（用于 -Command 参数中的双引号字符串）
  PowerShell 单引号字符串中，要表示一个单引号需要用两个单引号
============================================================ }

function PsSingleQuoteEscape(s: string): string;
begin
  Result := s;
  StringChange(Result, '''', '''''');
end;

{ ============================================================
  辅助函数：调用 Register-MCP.ps1 注册单个 Agent
  统一委托给 PowerShell 处理 JSON 合并，避免 Pascal Script 的字符串拼接错误
============================================================ }

function RegisterMcpConfig(const ConfigFile: string; const TopKey: string): Boolean;
var
  ResultCode: Integer;
  Ps1Content: string;
begin
  Result := False;
  if not FileExists(McpServerDir + '\Register-MCP.ps1') then
  begin
    Log('MCP: Register-MCP.ps1 不存在，跳过注册 ' + ConfigFile);
    Exit;
  end;

  { 调用 PS1 中的 Register-McpServer 函数，传入配置文件路径和顶层 key }
  { 使用单引号包裹参数，避免路径中的空格/特殊字符问题 }
  Ps1Content := '-ExecutionPolicy Bypass -NoProfile -Command "' +
    '. ''' + PsSingleQuoteEscape(McpServerDir + '\Register-MCP.ps1') + '''; ' +
    'Register-McpServer -ConfigFile ''' + PsSingleQuoteEscape(ConfigFile) + ''' -TopKey ''' + TopKey + '''' +
    '"';

  if Exec('powershell.exe', Ps1Content, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Result := (ResultCode = 0);
    if Result then
      Log('MCP: 已注册 ' + ConfigFile + ' (TopKey=' + TopKey + ')')
    else
      Log('MCP: 注册失败 ' + ConfigFile + ' (exit=' + IntToStr(ResultCode) + ')');
  end
  else
    Log('MCP: 无法启动 PowerShell 注册 ' + ConfigFile);
end;

{ ============================================================
  MCP 注册入口函数（由 Tasks 调用）
============================================================ }

function RegisterMcpToAllAgents: Boolean;
var
  ClaudePath: string;
  CursorPath: string;
  VscodePath: string;
  SuccessCount: Integer;
begin
  ClaudePath := ExpandConstant('{userappdata}\Claude\claude_desktop_config.json');
  CursorPath := ExpandConstant('{%USERPROFILE}\.cursor\mcp.json');
  VscodePath := ExpandConstant('{userappdata}\Code\User\mcp.json');

  Log('MCP: 开始注册 AI Agent MCP 服务器');

  SuccessCount := 0;
  if RegisterMcpConfig(ClaudePath, 'mcpServers') then Inc(SuccessCount);
  if RegisterMcpConfig(CursorPath, 'mcpServers') then Inc(SuccessCount);
  if RegisterMcpConfig(VscodePath, 'servers') then Inc(SuccessCount);

  Log('MCP: 注册完成，成功 ' + IntToStr(SuccessCount) + '/3，请重启 AI Agent 客户端');
  Result := True;
end;

{ ============================================================
  生成 Register-MCP.ps1 PowerShell 脚本
  用户可手动运行，提供更高的可控性
  注意：脚本运行时从 $PSScriptRoot 推断安装路径，避免硬编码 Program Files
============================================================ }

procedure GenerateMcpPowershellScript;
var
  ScriptContent: string;
  PSCode: string;
begin
  { PowerShell 脚本运行时从 $PSScriptRoot 推断 McpExe/McpDir，无需硬编码路径 }
  PSCode := '# AuditAI MCP 服务器注册工具' + #13#10 +
    '# 自动为 Claude Desktop / Cursor / VS Code 配置 MCP 服务器' + #13#10 +
    '$ErrorActionPreference = "Stop"' + #13#10 +
    '' + #13#10 +
    '{0} 从脚本所在目录推断 MCP 服务器路径' + #13#10 +
    '$McpDir = $PSScriptRoot' + #13#10 +
    '$McpExe = Join-Path $McpDir "AuditAI.McpServer.exe"' + #13#10 +
    '' + #13#10 +
    'if (-not (Test-Path $McpExe)) {' + #13#10 +
    '    Write-Host "[ERROR] 未找到 MCP 服务器: $McpExe" -ForegroundColor Red' + #13#10 +
    '    exit 1' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    '{0} auditai MCP 服务器条目' + #13#10 +
    '$AuditAiEntry = @{' + #13#10 +
    '    command = $McpExe' + #13#10 +
    '    args     = @()' + #13#10 +
    '    cwd      = $McpDir' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'function Register-McpServer {' + #13#10 +
    '    param(' + #13#10 +
    '        [Parameter(Mandatory=$true)][string]$ConfigFile,' + #13#10 +
    '        [Parameter(Mandatory=$true)][string]$TopKey' + #13#10 +
    '    )' + #13#10 +
    '' + #13#10 +
    '    {0} 确保目录存在' + #13#10 +
    '    $Dir = Split-Path $ConfigFile -Parent' + #13#10 +
    '    if (-not (Test-Path $Dir)) {' + #13#10 +
    '        New-Item -ItemType Directory -Path $Dir -Force | Out-Null' + #13#10 +
    '    }' + #13#10 +
    '' + #13#10 +
    '    {0} 如果文件不存在，创建新配置' + #13#10 +
    '    if (-not (Test-Path $ConfigFile)) {' + #13#10 +
    '        $NewConfig = [ordered]@{}' + #13#10 +
    '        $NewConfig[$TopKey] = @{ auditai = $AuditAiEntry }' + #13#10 +
    '        $NewConfig | ConvertTo-Json -Depth 10 | Out-File -FilePath $ConfigFile -Encoding UTF8' + #13#10 +
    '        Write-Host "[OK] 已创建 $ConfigFile" -ForegroundColor Green' + #13#10 +
    '        return $true' + #13#10 +
    '    }' + #13#10 +
    '' + #13#10 +
    '    {0} 读取并解析现有配置' + #13#10 +
    '    try {' + #13#10 +
    '        $Raw = Get-Content $ConfigFile -Raw' + #13#10 +
    '        $Existing = $Raw | ConvertFrom-Json' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Host "[ERROR] $ConfigFile 解析失败: $_" -ForegroundColor Red' + #13#10 +
    '        return $false' + #13#10 +
    '    }' + #13#10 +
    '' + #13#10 +
    '    {0} 检查是否已有 auditai 条目' + #13#10 +
    '    $TopObj = $Existing.$TopKey' + #13#10 +
    '    if ($TopObj -and $TopObj.PSObject.Properties[''auditai'']) {' + #13#10 +
    '        Write-Host "[SKIP] $ConfigFile 已有 auditai 条目" -ForegroundColor Yellow' + #13#10 +
    '        return $true' + #13#10 +
    '    }' + #13#10 +
    '' + #13#10 +
    '    {0} 合并 auditai 条目' + #13#10 +
    '    if (-not $TopObj) {' + #13#10 +
    '        {0} 顶层 key 不存在，创建新的' + #13#10 +
    '        $Existing | Add-Member -NotePropertyName $TopKey -NotePropertyValue ([ordered]@{ auditai = $AuditAiEntry }) -Force' + #13#10 +
    '    } else {' + #13#10 +
    '        $TopObj | Add-Member -NotePropertyName ''auditai'' -NotePropertyValue $AuditAiEntry -Force' + #13#10 +
    '    }' + #13#10 +
    '' + #13#10 +
    '    $Existing | ConvertTo-Json -Depth 10 | Out-File -FilePath $ConfigFile -Encoding UTF8' + #13#10 +
    '    Write-Host "[OK] 已写入 $ConfigFile" -ForegroundColor Green' + #13#10 +
    '    return $true' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    '{0} 卸载时清理 auditai 条目（供 Pascal Script 卸载流程调用）' + #13#10 +
    'function Unregister-McpServer {' + #13#10 +
    '    param(' + #13#10 +
    '        [Parameter(Mandatory=$true)][string]$ConfigFile' + #13#10 +
    '    )' + #13#10 +
    '' + #13#10 +
    '    if (-not (Test-Path $ConfigFile)) { return $true }' + #13#10 +
    '' + #13#10 +
    '    try {' + #13#10 +
    '        $Existing = Get-Content $ConfigFile -Raw | ConvertFrom-Json' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Host "[WARN] $ConfigFile 解析失败，跳过清理" -ForegroundColor Yellow' + #13#10 +
    '        return $false' + #13#10 +
    '    }' + #13#10 +
    '' + #13#10 +
    '    $Modified = $false' + #13#10 +
    '    foreach ($Prop in @($Existing.PSObject.Properties)) {' + #13#10 +
    '        if ($Prop.Value -and $Prop.Value.PSObject.Properties[''auditai'']) {' + #13#10 +
    '            $Prop.Value.PSObject.Properties.Remove(''auditai'') | Out-Null' + #13#10 +
    '            $Modified = $true' + #13#10 +
    '        }' + #13#10 +
    '    }' + #13#10 +
    '' + #13#10 +
    '    {0} 删除空的顶层 key' + #13#10 +
    '    if ($Modified) {' + #13#10 +
    '        $EmptyKeys = @($Existing.PSObject.Properties | Where-Object {' + #13#10 +
    '            $_.Value -is [System.Management.Automation.PSCustomObject] -and' + #13#10 +
    '            -not ($_.Value.PSObject.Properties | Select-Object -First 1)' + #13#10 +
    '        } | ForEach-Object { $_.Name })' + #13#10 +
    '        foreach ($K in $EmptyKeys) { $Existing.PSObject.Properties.Remove($K) | Out-Null }' + #13#10 +
    '' + #13#10 +
    '        if (-not ($Existing.PSObject.Properties | Select-Object -First 1)) {' + #13#10 +
    '            Remove-Item $ConfigFile -Force' + #13#10 +
    '            Write-Host "[OK] 已删除空配置文件 $ConfigFile" -ForegroundColor Green' + #13#10 +
    '        } else {' + #13#10 +
    '            $Existing | ConvertTo-Json -Depth 10 | Out-File -FilePath $ConfigFile -Encoding UTF8' + #13#10 +
    '            Write-Host "[OK] 已清理 $ConfigFile" -ForegroundColor Green' + #13#10 +
    '        }' + #13#10 +
    '    } else {' + #13#10 +
    '        Write-Host "[SKIP] $ConfigFile 无 auditai 条目" -ForegroundColor Gray' + #13#10 +
    '    }' + #13#10 +
    '    return $true' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'Write-Host "=============================================" -ForegroundColor Cyan' + #13#10 +
    'Write-Host "  AuditAI MCP 服务器注册工具" -ForegroundColor Cyan' + #13#10 +
    'Write-Host "=============================================" -ForegroundColor Cyan' + #13#10 +
    'Write-Host ""' + #13#10 +
    'Write-Host "MCP 服务器: $McpExe" -ForegroundColor Gray' + #13#10 +
    'Write-Host ""' + #13#10 +
    'Write-Host "正在配置 AI Agent MCP 服务器..." -ForegroundColor Yellow' + #13#10 +
    'Write-Host ""' + #13#10 +
    '' + #13#10 +
    '$Targets = @(' + #13#10 +
    '    @{ File = "$env:APPDATA\Claude\claude_desktop_config.json"; Key = "mcpServers" }' + #13#10 +
    '    @{ File = "$env:USERPROFILE\.cursor\mcp.json"; Key = "mcpServers" }' + #13#10 +
    '    @{ File = "$env:APPDATA\Code\User\mcp.json"; Key = "servers" }' + #13#10 +
    ')' + #13#10 +
    '' + #13#10 +
    '$Success = 0' + #13#10 +
    'foreach ($T in $Targets) {' + #13#10 +
    '    if (Register-McpServer -ConfigFile $T.File -TopKey $T.Key) { $Success++ }' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'Write-Host ""' + #13#10 +
    'Write-Host "=============================================" -ForegroundColor Cyan' + #13#10 +
    'Write-Host "注册完成 ($Success/$($Targets.Count))！请重启 Claude Desktop / Cursor / VS Code" -ForegroundColor Green' + #13#10 +
    'Write-Host "=============================================" -ForegroundColor Cyan' + #13#10 +
    'Write-Host ""' + #13#10 +
    'Write-Host "按任意键退出..." -ForegroundColor Gray' + #13#10 +
    '[void][System.Console]::ReadKey($true)';

  { 将占位符替换为 PowerShell 注释符 #，避免 Pascal Script 中转义 # 麻烦 }
  ScriptContent := PSCode;
  StringChange(ScriptContent, '{0}', Chr(35));

  { 使用 UTF-8 BOM 写入，确保 PowerShell 5.1 正确识别编码 }
  { （无 BOM 时 PS 5.1 会用系统 ANSI/GBK 编码读取，导致中文注释解析错误） }
  SaveUtf8FileWithBom(McpServerDir + '\Register-MCP.ps1', ScriptContent);
  Log('MCP: 已生成 Register-MCP.ps1');
end;

{ ============================================================
  生成 MCP_SETUP_README.txt 用户说明
============================================================ }

procedure GenerateMcpSetupReadme;
var
  ReadmePath: string;
  ReadmeContent: string;
begin
  ReadmeContent :=
    '========================================'#13#10 +
    '  AuditAI MCP 服务器配置说明'#13#10 +
    '========================================'#13#10 +
    ''#13#10 +
    'AuditAI 已安装 MCP 服务器，允许 AI Agent（Claude Desktop、'#13#10 +
    'Cursor、VS Code、Trae 等）直接调用审计工具。'#13#10 +
    ''#13#10 +
    'MCP 服务器位置：'#13#10 +
    '{MCP_EXE}'#13#10 +
    ''#13#10 +
    '【方法一】一键注册（推荐）'#13#10 +
    ''#13#10 +
    '1. 右键点击桌面上的 "AuditAI MCP 注册工具" 快捷方式'#13#10 +
    '2. 选择 "以 PowerShell 运行"'#13#10 +
    '3. 脚本会自动为 Claude Desktop / Cursor / VS Code 配置 MCP'#13#10 +
    '4. 重启各 AI Agent 客户端'#13#10 +
    ''#13#10 +
    '【方法二】手动配置'#13#10 +
    ''#13#10 +
    'Claude Desktop：'#13#10 +
    '  编辑 %APPDATA%\Claude\claude_desktop_config.json'#13#10 +
    '  添加如下内容：'#13#10 +
    ''#13#10 +
    '  {"mcpServers": {"auditai": {'#13#10 +
    '    "command": "{MCP_EXE}",'#13#10 +
    '    "args": [],'#13#10 +
    '    "cwd": "{MCP_DIR}"'#13#10 +
    '  }}}'#13#10 +
    ''#13#10 +
    'Cursor / VS Code：'#13#10 +
    '  在用户设置或项目根目录创建 mcp.json，内容同上，'#13#10 +
    '  但顶层 key 用 "mcpServers"（Cursor）或 "servers"（VS Code）。'#13#10 +
    ''#13#10 +
    '配置完成后重启 AI Agent 客户端，即可在对话中使用'#13#10 +
    '审计工具（如 list_projects、open_project 等）。'#13#10 +
    ''#13#10 +
    '========================================'#13#10 +
    'AuditAI v{VERSION}'#13#10 +
    '========================================'#13#10;

  StringChange(ReadmeContent, '{MCP_EXE}', McpServerExe);
  StringChange(ReadmeContent, '{MCP_DIR}', McpServerDir);
  StringChange(ReadmeContent, '{VERSION}', '{#MyAppVersion}');

  ReadmePath := McpServerDir + '\MCP_SETUP_README.txt';
  SaveStringToFile(ReadmePath, ReadmeContent, False);
  Log('MCP: 已生成 MCP_SETUP_README.txt');
end;

{ ============================================================
  安装后执行：生成脚本 + 可选注册
============================================================ }

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    { 始终生成 PowerShell 脚本和说明文档 }
    GenerateMcpPowershellScript;
    GenerateMcpSetupReadme;

    { 如果用户勾选了 mcp_register task，则自动注册 }
    if WizardIsTaskSelected('mcp_register') then
    begin
      RegisterMcpToAllAgents;
    end;
  end;
end;

{ ============================================================
  卸载时清理 MCP 配置中的 auditai 条目
  注意：卸载时安装目录的 Register-MCP.ps1 可能已被删除，
  因此这里生成一个临时 PS1 文件执行清理，避免依赖安装目录的脚本
============================================================ }

procedure UnregisterMcpFromConfig(const ConfigFile: string);
var
  Content: AnsiString;
  TempPs1: string;
  Ps1Content: string;
  ResultCode: Integer;
begin
  if not FileExists(ConfigFile) then
    Exit;

  LoadStringFromFile(ConfigFile, Content);
  if Pos('auditai', Content) = 0 then
    Exit;

  { 生成临时 PS1 文件，避免内联 -Command 的引号转义问题 }
  TempPs1 := GetEnv('TEMP') + '\AuditAI_Unregister_MCP.ps1';
  Ps1Content :=
    '$ErrorActionPreference = "Stop"' + #13#10 +
    '$ConfigFile = [System.IO.Path]::GetFullPath($args[0])' + #13#10 +
    'if (-not (Test-Path $ConfigFile)) { exit 0 }' + #13#10 +
    'try {' + #13#10 +
    '    $Existing = Get-Content $ConfigFile -Raw | ConvertFrom-Json' + #13#10 +
    '} catch {' + #13#10 +
    '    exit 0' + #13#10 +
    '}' + #13#10 +
    '$Modified = $false' + #13#10 +
    'foreach ($Prop in @($Existing.PSObject.Properties)) {' + #13#10 +
    '    if ($Prop.Value -and $Prop.Value.PSObject.Properties[''auditai'']) {' + #13#10 +
    '        $Prop.Value.PSObject.Properties.Remove(''auditai'') | Out-Null' + #13#10 +
    '        $Modified = $true' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 +
    'if ($Modified) {' + #13#10 +
    '    $EmptyKeys = @($Existing.PSObject.Properties | Where-Object {' + #13#10 +
    '        $_.Value -is [System.Management.Automation.PSCustomObject] -and' + #13#10 +
    '        -not ($_.Value.PSObject.Properties | Select-Object -First 1)' + #13#10 +
    '    } | ForEach-Object { $_.Name })' + #13#10 +
    '    foreach ($K in $EmptyKeys) { $Existing.PSObject.Properties.Remove($K) | Out-Null }' + #13#10 +
    '    if (-not ($Existing.PSObject.Properties | Select-Object -First 1)) {' + #13#10 +
    '        Remove-Item $ConfigFile -Force' + #13#10 +
    '    } else {' + #13#10 +
    '        $Existing | ConvertTo-Json -Depth 10 | Out-File -FilePath $ConfigFile -Encoding UTF8' + #13#10 +
    '    }' + #13#10 +
    '}';

  SaveUtf8FileWithBom(TempPs1, Ps1Content);

  { 调用临时 PS1，通过 $args[0] 传递配置文件路径，避免引号转义 }
  if Exec('powershell.exe',
    '-ExecutionPolicy Bypass -NoProfile -File "' + TempPs1 + '" "' + ConfigFile + '"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('MCP: 已清理 ' + ConfigFile + ' (exit=' + IntToStr(ResultCode) + ')');
  end
  else
    Log('MCP: 清理失败 ' + ConfigFile);

  DeleteFile(TempPs1);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ClaudePath: string;
  CursorPath: string;
  VscodePath: string;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    ClaudePath := ExpandConstant('{userappdata}\Claude\claude_desktop_config.json');
    CursorPath := ExpandConstant('{%USERPROFILE}\.cursor\mcp.json');
    VscodePath := ExpandConstant('{userappdata}\Code\User\mcp.json');

    UnregisterMcpFromConfig(ClaudePath);
    UnregisterMcpFromConfig(CursorPath);
    UnregisterMcpFromConfig(VscodePath);

    Log('MCP: 卸载时 MCP 配置清理完成');
  end;
end;

{ ============================================================
  .NET 4.8 检测
============================================================ }

function IsDotNet48Installed: Boolean;
var
  Release: Cardinal;
begin
  Result := False;
  if RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) then
  begin
    if Release >= 528040 then
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
  if not IsDotNet48Installed then
  begin
    if MsgBox('运行 {#MyAppName} 需要 Microsoft .NET Framework 4.8。' + #13#10 + #13#10 +
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

[UninstallDelete]
Type: files; Name: "{app}\*.log"
Type: dirifempty; Name: "{app}\Data"
Type: dirifempty; Name: "{app}\config"
Type: dirifempty; Name: "{app}\McpServer"
