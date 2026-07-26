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

# 第一轮：获取项目树找分组
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
    Write-Host "No groups found!" -ForegroundColor Red
    exit 1
}

# 第二轮：创建文档 + 添加段落
$createDocArgs = @{ parent_id = $groupId; name = 'InsertTestDoc' } | ConvertTo-Json -Compress -Depth 5
$input2 = ''
$input2 += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$input2 += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$input2 += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl
$input2 += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"create_document_node`",`"arguments`":$createDocArgs}}" + $nl
$input2 += '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"list_nodes_by_type","arguments":{"node_type":"document"}}}' + $nl

$raw2 = Invoke-Session $input2
$createText = Get-Resp $raw2 3
$createObj = $createText | ConvertFrom-Json
Write-Host "Create document: success=$($createObj.success) message=$($createObj.message)" -ForegroundColor Cyan

$listText = Get-Resp $raw2 4
$listObj = $listText | ConvertFrom-Json
$docNode = $null
if ($listObj.success -and $listObj.nodes) {
    $docNode = $listObj.nodes | Select-Object -Last 1
    Write-Host "Found document: id=$($docNode.id) name=$($docNode.name)" -ForegroundColor Cyan
}

if (-not $docNode) {
    Write-Host "No document found - cannot test insert tools" -ForegroundColor Red
    exit 1
}

$docId = [long]$docNode.id

# 添加段落（使用 add_paragraph）- 手工构造 JSON 避免 ConvertTo-Json 失败
$addParaArgs = "{`"document_node_id`":$docId,`"text`":`"Paragraph0`"}"
Write-Host "addParaArgs: $addParaArgs" -ForegroundColor Yellow
$input2b = ''
$input2b += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$input2b += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$input2b += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl
$input2b += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"add_paragraph`",`"arguments`":$addParaArgs}}" + $nl

$raw2b = Invoke-Session $input2b
$addParaText = Get-Resp $raw2b 3
Write-Host "AddParaText: $addParaText" -ForegroundColor Yellow
if ($addParaText) {
    $addParaObj = $addParaText | ConvertFrom-Json
    Write-Host "Add paragraph: success=$($addParaObj.success)" -ForegroundColor Cyan
} else {
    Write-Host "Add paragraph: no response" -ForegroundColor Red
    exit 1
}

# 第三轮：测试所有8个 insert 工具
$tools = @(
    @{ id = 3; name = 'insert_page_break';  args = @{ document_node_id = $docId; paragraph_index = 0 } }
    @{ id = 4; name = 'insert_section_break'; args = @{ document_node_id = $docId; paragraph_index = 0; break_type = 'continuous' } }
    @{ id = 5; name = 'insert_symbol'; args = @{ document_node_id = $docId; paragraph_index = 0; symbol = 'c'; font_family = 'Arial' } }
    @{ id = 6; name = 'insert_text_frame'; args = @{ document_node_id = $docId; paragraph_index = 0; text = 'TestTextFrame'; width = 3000; height = 1500 } }
    @{ id = 7; name = 'insert_header'; args = @{ document_node_id = $docId; text = 'AuditReport'; header_type = 'default' } }
    @{ id = 8; name = 'insert_footer'; args = @{ document_node_id = $docId; text = 'Page X'; footer_type = 'default' } }
    @{ id = 9; name = 'insert_image'; args = @{ document_node_id = $docId; paragraph_index = 0; image_path = 'e:\test\logo.png'; width = 300; height = 200 } }
    @{ id = 10; name = 'insert_table_into_document'; args = @{ document_node_id = $docId; paragraph_index = 0; rows = 3; cols = 4; border_style = 'single' } }
)

$input3 = ''
$input3 += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$input3 += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$input3 += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl

foreach ($t in $tools) {
    $argPart = $t.args | ConvertTo-Json -Compress -Depth 10
    $input3 += "{`"jsonrpc`":`"2.0`",`"id`":$($t.id),`"method`":`"tools/call`",`"params`":{`"name`":`"$($t.name)`",`"arguments`":$argPart}}" + $nl
}

$raw3 = Invoke-Session $input3

$pass = 0
$fail = 0

foreach ($t in $tools) {
    $text = Get-Resp $raw3 $t.id
    Write-Host ""
    Write-Host "[$($t.id) $($t.name)]" -ForegroundColor Cyan
    if (-not $text) {
        Write-Host "  FAIL: no response" -ForegroundColor Red
        $fail++
        continue
    }
    Write-Host "  $text" -ForegroundColor DarkGray
    try {
        $obj = $text | ConvertFrom-Json
        if ($obj.success) {
            Write-Host "  PASS: $($obj.message)" -ForegroundColor Green
            $pass++
        } else {
            Write-Host "  FAIL: $($obj.error)" -ForegroundColor Red
            $fail++
        }
    } catch {
        Write-Host "  FAIL: invalid JSON" -ForegroundColor Red
        $fail++
    }
}

Write-Host ""
Write-Host "=== 测试结果 ==="
Write-Host "PASS: $pass / $($tools.Count)"
Write-Host "FAIL: $fail / $($tools.Count)"
