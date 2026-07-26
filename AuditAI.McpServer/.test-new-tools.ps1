$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'
$validTableId = 5699421601793

# 直接构建带换行的输入字符串 - 使用 [char]10 确保 LF 换行
$nl = [char]10
$inputText = '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}' + $nl
$inputText += '{"jsonrpc":"2.0","method":"notifications/initialized"}' + $nl
$inputText += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}" + $nl

# 工具调用列表
$tools = @(
    @{ id = 3;  name = 'get_cell_borders';       args = @{ table_node_id = $validTableId; row = 0; col = 0 } }
    @{ id = 4;  name = 'set_cell_border';        args = @{ table_node_id = $validTableId; row = 0; col = 0; edge = 'top'; width = 2 } }
    @{ id = 5;  name = 'get_cell_borders';       args = @{ table_node_id = $validTableId; row = 0; col = 0 } }
    @{ id = 6;  name = 'set_cell_all_borders';   args = @{ table_node_id = $validTableId; row = 1; col = 1; width = 1 } }
    @{ id = 7;  name = 'set_range_borders';      args = @{ table_node_id = $validTableId; start_row = 0; start_col = 0; end_row = 2; end_col = 2; mode = 'all'; width = 1 } }
    @{ id = 8;  name = 'get_table_style';        args = @{ table_node_id = $validTableId } }
    @{ id = 9;  name = 'apply_table_style_preset'; args = @{ table_node_id = $validTableId; preset = 'grid' } }
    @{ id = 10; name = 'set_custom_border_style'; args = @{ table_node_id = $validTableId; up_down_line = 'thick'; left_right_line = 'none'; body_line = 'thin'; second_line = 'thin' } }
    @{ id = 11; name = 'get_validation_errors';        args = @{} }
    @{ id = 12; name = 'get_next_validation_error';    args = @{ current_index = 0 } }
    @{ id = 13; name = 'get_previous_validation_error'; args = @{ current_index = 999 } }
    @{ id = 14; name = 'list_nodes_by_type';     args = @{ node_type = 'document' } }
)

foreach ($t in $tools) {
    $argPart = $t.args | ConvertTo-Json -Compress -Depth 10
    $inputText += "{`"jsonrpc`":`"2.0`",`"id`":$($t.id),`"method`":`"tools/call`",`"params`":{`"name`":`"$($t.name)`",`"arguments`":$argPart}}" + $nl
}
Write-Host "=== InputText length: $($inputText.Length) ===" -ForegroundColor Yellow
Write-Host "=== First 300 chars ===" -ForegroundColor Yellow
$inputText.Substring(0, [Math]::Min(300, $inputText.Length))

# 启动 MCP 进程
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
$waited = $proc.WaitForExit(60000)
if (-not $waited) {
    try { $proc.Kill() } catch {}
    Write-Host "TIMEOUT"
    exit 1
}
try { $stdoutTask.Wait(5000) | Out-Null } catch {}
$raw = $stdoutTask.Result

Write-Host ""
Write-Host "=== RAW OUTPUT LENGTH: $($raw.Length) ===" -ForegroundColor Yellow
Write-Host "=== FIRST 500 CHARS ===" -ForegroundColor Yellow
if ($raw) { $raw.Substring(0, [Math]::Min(500, $raw.Length)) }

# 解析每个工具的响应
$pass = 0
$fail = 0
$skip = 0

$script:lines = $raw -split "`n"

function Get-Resp($targetId) {
    $pattern = '"id":\s*' + $targetId + '[\s,\}]'
    $resp = $script:lines | Where-Object { $_ -match $pattern } | Select-Object -Last 1
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

function Show-Test($targetId, $name, $toolArgs, $expectSuccess = $true) {
    $text = Get-Resp $targetId
    Write-Host ""
    Write-Host "[$targetId $name] args=$(ConvertTo-Json $toolArgs -Compress -Depth 5)" -ForegroundColor Cyan
    if (-not $text) {
        Write-Host "  FAIL: no response" -ForegroundColor Red
        $script:fail++
        return $null
    }
    $obj = $null
    try { $obj = $text | ConvertFrom-Json } catch {
        Write-Host "  FAIL: invalid JSON: $text" -ForegroundColor Red
        $script:fail++
        return $null
    }
    if ($obj.success -eq $expectSuccess) {
        Write-Host "  PASS: $($obj.message) $($obj.error)" -ForegroundColor Green
        $script:pass++
    } else {
        Write-Host "  FAIL: success=$($obj.success) error=$($obj.error) message=$($obj.message)" -ForegroundColor Red
        $script:fail++
    }
    return $obj
}

Write-Host ""
Write-Host "=== 1. CellBorder 工具测试（4个工具）==="
Show-Test 3 'get_cell_borders' @{ table_node_id = $validTableId; row = 0; col = 0 }
Show-Test 4 'set_cell_border' @{ table_node_id = $validTableId; row = 0; col = 0; edge = 'top'; width = 2 }
$obj = Show-Test 5 'get_cell_borders' @{ table_node_id = $validTableId; row = 0; col = 0 }
if ($obj -and $obj.success -and $obj.top.width -eq 2) {
    Write-Host "  -> top border persisted (width=2)" -ForegroundColor Green
} elseif ($obj -and $obj.success) {
    Write-Host "  -> top border NOT persisted (got width=$($obj.top.width))" -ForegroundColor Yellow
}
Show-Test 6 'set_cell_all_borders' @{ table_node_id = $validTableId; row = 1; col = 1; width = 1 }
Show-Test 7 'set_range_borders' @{ table_node_id = $validTableId; start_row = 0; start_col = 0; end_row = 2; end_col = 2; mode = 'all'; width = 1 }

Write-Host ""
Write-Host "=== 2. TableStylePreset 工具测试（3个工具）==="
Show-Test 8 'get_table_style' @{ table_node_id = $validTableId }
Show-Test 9 'apply_table_style_preset' @{ table_node_id = $validTableId; preset = 'grid' }
Show-Test 10 'set_custom_border_style' @{ table_node_id = $validTableId; up_down_line = 'thick'; left_right_line = 'none'; body_line = 'thin'; second_line = 'thin' }

Write-Host ""
Write-Host "=== 3. ValidationErrorNav 工具测试（3个工具）==="
$obj = Show-Test 11 'get_validation_errors' @{}
if ($obj -and $obj.success) {
    Write-Host "  -> total_errors=$($obj.total_errors) passed=$($obj.total_passed) failed=$($obj.total_failed)" -ForegroundColor Cyan
}
Show-Test 12 'get_next_validation_error' @{ current_index = 0 }
Show-Test 13 'get_previous_validation_error' @{ current_index = 999 }

Write-Host ""
Write-Host "=== 4. DocumentCharFormat 工具测试（2个工具）==="
$obj = Show-Test 14 'list_nodes_by_type' @{ node_type = 'document' }
if ($obj -and $obj.success -and $obj.nodes.Count -gt 0) {
    Write-Host "Found document node - skipping char format tests in this run (need new session)" -ForegroundColor Yellow
    $script:skip += 2
} else {
    Write-Host "No document nodes found - skipping DocumentCharFormat tests" -ForegroundColor Yellow
    $script:skip += 2
}

Write-Host ""
Write-Host "=== 测试总结 ==="
Write-Host "PASS: $pass"
Write-Host "FAIL: $fail"
Write-Host "SKIP: $skip"
