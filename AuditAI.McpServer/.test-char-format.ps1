$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'
$nl = [char]10

function Invoke-Session($inputText) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $exe
    $psi.RedirectStandardInput = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8
    $proc = [System.Diagnostics.Process]::Start($psi)
    $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
    $proc.StandardInput.Write($inputText)
    $proc.StandardInput.Flush()
    $proc.StandardInput.Close()
    $proc.WaitForExit(60000) | Out-Null
    try { $stdoutTask.Wait(5000) | Out-Null } catch {}
    return $stdoutTask.Result
}

function Get-Resp($raw, $targetId) {
    if (-not $raw) { return $null }
    $lines = $raw -split "`n"
    $pattern = '"id":\s*' + $targetId + '[\s,\}]'
    $resp = $lines | Where-Object { $_ -match $pattern } | Select-Object -Last 1
    if (-not $resp) { return $null }
    $obj = $resp | ConvertFrom-Json
    if ($obj.result -and $obj.result.content) {
        return $obj.result.content[0].text
    }
    if ($obj.error) {
        return "{`"success`":false,`"error`":`"$($obj.error.message)`"}"
    }
    return $null
}

# 第一轮：获取项目树
$input1 = ''
$input1 += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$input1 += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$input1 += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl
$input1 += '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"get_project_tree","arguments":{}}}' + $nl

$raw1 = Invoke-Session $input1
$treeText = Get-Resp $raw1 3
$treeObj = $treeText | ConvertFrom-Json

$groupId = $null
if ($treeObj.success -and $treeObj.tree_groups) {
    $firstGroup = $treeObj.tree_groups | Select-Object -First 1
    $groupId = [long]$firstGroup.id
    Write-Host "Found group: id=$groupId name=$($firstGroup.name)" -ForegroundColor Cyan
} else {
    Write-Host "No groups found! treeText=$treeText" -ForegroundColor Red
    exit 1
}

# 第二轮：创建文档 + 添加段落 + 保存项目
$createDocArgs = @{ parent_id = $groupId; name = '字符格式测试文档' } | ConvertTo-Json -Compress -Depth 5
$input2 = ''
$input2 += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$input2 += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$input2 += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl
$input2 += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"create_document_node`",`"arguments`":$createDocArgs}}" + $nl
$input2 += '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"list_nodes_by_type","arguments":{"node_type":"document"}}}' + $nl

$raw2 = Invoke-Session $input2
$createText = Get-Resp $raw2 3
$createObj = $createText | ConvertFrom-Json
Write-Host "Create document: success=$($createObj.success) message=$($createObj.message) error=$($createObj.error)" -ForegroundColor Cyan

$listText = Get-Resp $raw2 4
$listObj = $listText | ConvertFrom-Json
$docNode = $null
if ($listObj.success -and $listObj.nodes) {
    $docNode = $listObj.nodes | Select-Object -Last 1
    Write-Host "Found document: id=$($docNode.id) name=$($docNode.name)" -ForegroundColor Cyan
}

if (-not $docNode) {
    Write-Host "No document found - cannot test char format tools" -ForegroundColor Red
    Write-Host "listText: $listText" -ForegroundColor DarkGray
    exit 1
}

$docId = [long]$docNode.id

# 第三轮之前：先用第四轮添加段落并保存
$addParaArgs = @{ document_node_id = $docId; text = '测试段落' } | ConvertTo-Json -Compress -Depth 5
$input2b = ''
$input2b += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$input2b += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$input2b += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl
$input2b += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"add_paragraph`",`"arguments`":$addParaArgs}}" + $nl
$input2b += '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"save_project","arguments":{}}}' + $nl

$raw2b = Invoke-Session $input2b
$addParaText = Get-Resp $raw2b 3
$addParaObj = $addParaText | ConvertFrom-Json
Write-Host "Add paragraph: success=$($addParaObj.success) message=$($addParaObj.message) error=$($addParaObj.error)" -ForegroundColor Cyan

$saveText = Get-Resp $raw2b 4
$saveObj = $saveText | ConvertFrom-Json
Write-Host "Save project: success=$($saveObj.success) message=$($saveObj.message) error=$($saveObj.error)" -ForegroundColor Cyan

# 第三轮：测试 get/set/get char format
$getArgs = @{ document_node_id = $docId; paragraph_index = 0 } | ConvertTo-Json -Compress -Depth 5
$setArgs = @{ document_node_id = $docId; paragraph_index = 0; bold = $true; font_size = 12; fore_color = 'FF0000'; font_family = '宋体' } | ConvertTo-Json -Compress -Depth 5

$input3 = ''
$input3 += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$input3 += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$input3 += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl
$input3 += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"get_char_format`",`"arguments`":$getArgs}}" + $nl
$input3 += "{`"jsonrpc`":`"2.0`",`"id`":4,`"method`":`"tools/call`",`"params`":{`"name`":`"set_char_format`",`"arguments`":$setArgs}}" + $nl
$input3 += "{`"jsonrpc`":`"2.0`",`"id`":5,`"method`":`"tools/call`",`"params`":{`"name`":`"get_char_format`",`"arguments`":$getArgs}}" + $nl

$raw3 = Invoke-Session $input3

Write-Host ""
Write-Host "=== get_char_format (initial) ===" -ForegroundColor Cyan
$getText = Get-Resp $raw3 3
Write-Host $getText
if ($getText) { $getObject = $getText | ConvertFrom-Json } else { $getObject = $null }

Write-Host ""
Write-Host "=== set_char_format (bold=true, font_size=12, color=FF0000) ===" -ForegroundColor Cyan
$setText = Get-Resp $raw3 4
Write-Host $setText
if ($setText) { $setObject = $setText | ConvertFrom-Json } else { $setObject = $null }

Write-Host ""
Write-Host "=== get_char_format (after set) ===" -ForegroundColor Cyan
$afterText = Get-Resp $raw3 5
Write-Host $afterText
if ($afterText) { $afterObject = $afterText | ConvertFrom-Json } else { $afterObject = $null }

Write-Host ""
Write-Host "=== 测试结果 ==="
if ($getObject -and $getObject.success) { Write-Host "PASS: get_char_format (initial)" -ForegroundColor Green } else { Write-Host "FAIL: get_char_format (initial)" -ForegroundColor Red }
if ($setObject -and $setObject.success) { Write-Host "PASS: set_char_format" -ForegroundColor Green } else { Write-Host "FAIL: set_char_format" -ForegroundColor Red }
if ($afterObject -and $afterObject.success) { Write-Host "PASS: get_char_format (after set)" -ForegroundColor Green } else { Write-Host "FAIL: get_char_format (after set)" -ForegroundColor Red }

if ($afterObject) {
    if ($afterObject.bold -eq $true) {
        Write-Host "  -> bold persisted" -ForegroundColor Green
    } else {
        Write-Host "  -> bold NOT persisted (got $($afterObject.bold))" -ForegroundColor Yellow
    }

    if ($afterObject.font_size -eq 12) {
        Write-Host "  -> font_size persisted" -ForegroundColor Green
    } else {
        Write-Host "  -> font_size NOT persisted (got $($afterObject.font_size))" -ForegroundColor Yellow
    }
}
