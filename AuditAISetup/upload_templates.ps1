# 本地模板上传到云端脚本
# 用法: .\upload_templates.ps1 -UserName admin -Password 123456 -TemplatesDir "E:\lq\AuditAI\AuditAI\Data\Templates"

param(
    [Parameter(Mandatory=$true)][string]$UserName,
    [Parameter(Mandatory=$true)][string]$Password,
    [Parameter(Mandatory=$true)][string]$TemplatesDir,
    [string]$ApiBase = "http://82.156.108.218:8957/api/"
)

$ErrorActionPreference = "Stop"

# 加载 System.Data.SQLite
$sqlitePath = "E:\lq\AuditAI\Libs\System.Data.SQLite.dll"
if (-not (Test-Path $sqlitePath)) {
    Write-Error "找不到 System.Data.SQLite.dll: $sqlitePath"
    exit 1
}
Add-Type -Path $sqlitePath

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [string]$Token = $null
    )

    $fullUrl = if ($Url.StartsWith("http")) { $Url } else { $ApiBase + $Url }
    $headers = @{}
    if ($Token) { $headers["Token"] = $Token }

    try {
        if ($Body -ne $null) {
            $jsonBody = $Body | ConvertTo-Json -Depth 100
            $response = Invoke-WebRequest -Method $Method -Uri $fullUrl -Headers $headers `
                -Body $jsonBody -ContentType "application/json" -UseBasicParsing
        } else {
            $response = Invoke-WebRequest -Method $Method -Uri $fullUrl -Headers $headers `
                -UseBasicParsing
        }
        return $response.Content | ConvertFrom-Json
    } catch {
        Write-Host "API 错误: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $errorBody = $reader.ReadToEnd()
            Write-Host "响应体: $errorBody" -ForegroundColor Red
        }
        return $null
    }
}

function Read-TemplateInfo {
    param([string]$DbPath)

    try {
        $connStr = "Data Source=$DbPath;Version=3;Read Only=True;"
        $conn = New-Object System.Data.SQLite.SQLiteConnection($connStr)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT * FROM Project LIMIT 1"
        $reader = $cmd.ExecuteReader()
        if ($reader.Read()) {
            $result = [PSCustomObject]@{
                Id = [Guid]$reader["Id"].ToString()
                Name = $reader["Name"].ToString()
                Number = if ($reader["Number"] -ne [DBNull]::Value) { $reader["Number"].ToString() } else { "" }
                Category = if ($reader["Category"] -ne [DBNull]::Value) { $reader["Category"].ToString() } else { "" }
                Note = if ($reader["Note"] -ne [DBNull]::Value) { $reader["Note"].ToString() } else { "" }
                Version = [int]$reader["Version"]
            }
            $reader.Close()
            $conn.Close()
            return $result
        }
        $reader.Close()
        $conn.Close()
    } catch {
        Write-Host "  读取模板信息失败: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    return $null
}

function Read-TreeGroups {
    param([string]$DbPath)

    $groups = @()
    try {
        $connStr = "Data Source=$DbPath;Version=3;Read Only=True;"
        $conn = New-Object System.Data.SQLite.SQLiteConnection($connStr)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT Id, Name, `Index` FROM TreeGroup WHERE Status < 2 ORDER BY `Index`"
        $reader = $cmd.ExecuteReader()
        while ($reader.Read()) {
            $groups += [PSCustomObject]@{
                Id = [Int64]$reader["Id"]
                Name = $reader["Name"].ToString()
                Index = [int]$reader["Index"]
            }
        }
        $reader.Close()
        $conn.Close()
    } catch {
        Write-Host "  读取分组失败: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    return $groups
}

function Read-TreeNodes {
    param([string]$DbPath)

    $nodes = @()
    try {
        $connStr = "Data Source=$DbPath;Version=3;Read Only=True;"
        $conn = New-Object System.Data.SQLite.SQLiteConnection($connStr)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT Id, GroupId, ParentId, Name, `Index`, Type, Level, Number, Visible, RowWrite, RowRead FROM TreeNode WHERE Status < 2 ORDER BY GroupId, ParentId, `Index`"
        $reader = $cmd.ExecuteReader()
        while ($reader.Read()) {
            $nodes += [PSCustomObject]@{
                Id = [Int64]$reader["Id"]
                GroupId = [Int64]$reader["GroupId"]
                ParentId = if ($reader["ParentId"] -ne [DBNull]::Value) { [Int64]$reader["ParentId"] } else { $null }
                Name = $reader["Name"].ToString()
                Index = [int]$reader["Index"]
                Type = [int]$reader["Type"]
                Level = [int]$reader["Level"]
                Number = if ($reader["Number"] -ne [DBNull]::Value) { $reader["Number"].ToString() } else { "" }
                Visible = [bool]$reader["Visible"]
                RowWrite = [bool]$reader["RowWrite"]
                RowRead = [bool]$reader["RowRead"]
            }
        }
        $reader.Close()
        $conn.Close()
    } catch {
        Write-Host "  读取节点失败: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    return $nodes
}

function Get-TableCount {
    param([string]$DbPath)

    try {
        $connStr = "Data Source=$DbPath;Version=3;Read Only=True;"
        $conn = New-Object System.Data.SQLite.SQLiteConnection($connStr)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM `Table`"
        $count = [int]$cmd.ExecuteScalar()
        $conn.Close()
        return $count
    } catch {
        return 0
    }
}

function Get-DocCount {
    param([string]$DbPath)

    try {
        $connStr = "Data Source=$DbPath;Version=3;Read Only=True;"
        $conn = New-Object System.Data.SQLite.SQLiteConnection($connStr)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM Document"
        $count = [int]$cmd.ExecuteScalar()
        $conn.Close()
        return $count
    } catch {
        return 0
    }
}

Write-Host "===== 本地模板上传工具 =====" -ForegroundColor Cyan
Write-Host "API 地址: $ApiBase"
Write-Host "用户名: $UserName"
Write-Host "模板目录: $TemplatesDir"
Write-Host ""

# 1. 登录
Write-Host "正在登录..." -ForegroundColor Yellow
$loginBody = @{ UserName = $UserName; Password = $Password }
$loginResult = Invoke-Api -Method Post -Url "User/AccountLogin" -Body $loginBody

if (-not $loginResult -or -not $loginResult[0]) {
    Write-Error "登录失败!"
    exit 1
}

$token = $loginResult[0].TokenValue
$userId = $loginResult[1].Id
Write-Host "登录成功! 用户ID: $userId" -ForegroundColor Green
Write-Host ""

# 2. 获取团队
Write-Host "正在获取团队列表..." -ForegroundColor Yellow
$teams = Invoke-Api -Method Get -Url "Project/GetUserTeams" -Token $token

if (-not $teams -or -not $teams.teams -or $teams.teams.Count -eq 0) {
    Write-Error "未找到团队!"
    exit 1
}

$teamId = $teams.teams[0].TeamId
$teamName = $teams.teams[0].TeamName
Write-Host "当前团队: $teamName ($teamId)" -ForegroundColor Green
Write-Host ""

# 3. 切换团队
Invoke-Api -Method Post -Url "Project/UpdateCurrentTeam" -Body @{ TeamId = $teamId } -Token $token | Out-Null

# 4. 获取现有模板
Write-Host "正在获取云端现有模板..." -ForegroundColor Yellow
$existingTemplates = Invoke-Api -Method Get -Url "Project/GetTemplates" -Token $token
$existingNames = @{}
if ($existingTemplates) {
    foreach ($t in $existingTemplates) {
        $existingNames[$t.Name] = $true
    }
}
Write-Host "云端现有模板: $($existingNames.Count) 个" -ForegroundColor Green
Write-Host ""

# 5. 扫描本地模板
if (-not (Test-Path $TemplatesDir)) {
    Write-Error "模板目录不存在: $TemplatesDir"
    exit 1
}

$dbFiles = Get-ChildItem -Path $TemplatesDir -Filter "*.db"
Write-Host "找到本地模板: $($dbFiles.Count) 个" -ForegroundColor Green
Write-Host ""

$success = 0
$failed = 0
$skipped = 0
$index = 0

foreach ($dbFile in $dbFiles) {
    $index++
    $fileName = $dbFile.Name
    Write-Host "[$index/$($dbFiles.Count)] $fileName" -ForegroundColor Cyan

    try {
        # 读取模板信息
        $templateInfo = Read-TemplateInfo -DbPath $dbFile.FullName
        if (-not $templateInfo) {
            Write-Host "  跳过: 无法读取模板信息" -ForegroundColor Yellow
            $skipped++
            continue
        }

        # 检查是否已存在
        if ($existingNames.ContainsKey($templateInfo.Name)) {
            Write-Host "  跳过: 云端已存在同名模板 '$($templateInfo.Name)'" -ForegroundColor Yellow
            $skipped++
            continue
        }

        # 读取结构数据
        $groups = Read-TreeGroups -DbPath $dbFile.FullName
        $nodes = Read-TreeNodes -DbPath $dbFile.FullName
        $tableCount = Get-TableCount -DbPath $dbFile.FullName
        $docCount = Get-DocCount -DbPath $dbFile.FullName

        Write-Host "  名称: $($templateInfo.Name)"
        Write-Host "  分组: $($groups.Count), 节点: $($nodes.Count), 表格: $tableCount, 文档: $docCount"

        # 创建模板项目
        Write-Host "  创建模板项目..." -NoNewline
        $newProjectId = [Guid]::NewGuid().ToString()
        $createBody = @{
            Id = $newProjectId
            Name = $templateInfo.Name
            Number = $templateInfo.Number
            Category = $templateInfo.Category
            Note = $templateInfo.Note
            Type = 1  # ProjectType.Template
            Users = @()
        }
        $createResult = Invoke-Api -Method Post -Url "Project/CreateProject" -Body $createBody -Token $token

        if (-not $createResult) {
            Write-Host " 失败!" -ForegroundColor Red
            $failed++
            continue
        }
        Write-Host " OK" -ForegroundColor Green

        # 构建 PushProject 数据
        Write-Host "  推送项目结构..." -NoNewline

        $groupsArr = @()
        foreach ($g in $groups) {
            $groupsArr += @{
                Action = "New"
                Id = $g.Id
                Name = $g.Name
                Index = $g.Index
            }
        }

        $nodesArr = @()
        foreach ($n in $nodes) {
            $nodeObj = @{
                Action = "New"
                Id = $n.Id
                GroupId = $n.GroupId
                Name = $n.Name
                Index = $n.Index
                Type = $n.Type
                Level = $n.Level
                Number = $n.Number
                Visible = $n.Visible
                RowWrite = $n.RowWrite
                RowRead = $n.RowRead
                Permissions = ""
            }
            if ($n.ParentId) { $nodeObj["ParentId"] = $n.ParentId }
            $nodesArr += $nodeObj
        }

        $pushBody = @{
            Action = "PushProject"
            Id = $newProjectId
            Version = 0
            Groups = $groupsArr
            Nodes = $nodesArr
            DataRefs = @()
            VFs = @()
        }

        $pushResult = Invoke-Api -Method Post -Url "Project/PushProjectQuick" -Body $pushBody -Token $token

        if (-not $pushResult -or ($pushResult.Result -ne "Success" -and $pushResult.Result -ne "NoContent")) {
            Write-Host " 失败! Result: $($pushResult.Result)" -ForegroundColor Red
            $failed++
            continue
        }
        Write-Host " OK" -ForegroundColor Green

        Write-Host "  ✓ 上传成功 (结构)" -ForegroundColor Green
        $success++
        $existingNames[$templateInfo.Name] = $true

        if ($tableCount -gt 0 -or $docCount -gt 0) {
            Write-Host "  ⚠ 注意: 表格($tableCount)和文档($docCount)的内容暂未上传，仅上传了结构" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  异常: $($_.Exception.Message)" -ForegroundColor Red
        $failed++
    }

    Write-Host ""
}

Write-Host "================================" -ForegroundColor Cyan
Write-Host "上传完成!" -ForegroundColor Green
Write-Host "  成功: $success" -ForegroundColor Green
Write-Host "  失败: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
Write-Host "  跳过: $skipped" -ForegroundColor Yellow
Write-Host "  总计: $($dbFiles.Count) 个本地模板"
