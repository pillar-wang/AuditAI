$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'

$validTableId = 5699421601793   # "报表 抬头信息表"

function Invoke-Tool($toolName, $argsObj) {
    $argPart = $argsObj | ConvertTo-Json -Compress -Depth 10
    $requests = @()
    $requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
    $requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
    $requests += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}"
    $requests += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"$toolName`",`"arguments`":$argPart}}"

    $inputText = ($requests -join "`n") + "`n"
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
    $stderrTask = $proc.StandardError.ReadToEndAsync()

    $proc.StandardInput.Write($inputText)
    $proc.StandardInput.Flush()
    $proc.StandardInput.Close()

    $waited = $proc.WaitForExit(30000)
    if (-not $waited) {
        try { $proc.Kill() } catch {}
        return @{ status = "TIMEOUT"; msg = "killed after 30s" }
    }

    try { $stdoutTask.Wait(2000) | Out-Null } catch {}
    try { $stderrTask.Wait(2000) | Out-Null } catch {}
    $out = $stdoutTask.Result

    $lines = $out -split "`n"
    $resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
    if (-not $resp) {
        return @{ status = "NO_RESP"; msg = "stdout_lines=$($lines.Count)" }
    }

    try {
        $obj = $resp | ConvertFrom-Json
        if ($obj.error) { return @{ status = "RPC_ERROR"; msg = $obj.error.message } }
        if ($obj.result -and $obj.result.content) {
            $text = ($obj.result.content | Where-Object { $_.type -eq "text" } | Select-Object -First 1).text
            try {
                $inner = $text | ConvertFrom-Json
                if ($inner.success -eq $false) { return @{ status = "FAIL"; msg = $inner.error } }
                $snippet = if ($text.Length -gt 200) { $text.Substring(0, 200) + "..." } else { $text }
                return @{ status = "OK"; msg = $snippet }
            } catch {
                $snippet = if ($text.Length -gt 200) { $text.Substring(0, 200) + "..." } else { $text }
                return @{ status = "OK_NONJSON"; msg = $snippet }
            }
        }
        return @{ status = "UNKNOWN"; msg = $resp.Substring(0, [Math]::Min(120, $resp.Length)) }
    } catch {
        return @{ status = "PARSE_ERR"; msg = $_.ToString() }
    }
}

# Also a helper to extract a created validation_id from add_validation_point response
function Invoke-Tool-Raw($toolName, $argsObj) {
    $argPart = $argsObj | ConvertTo-Json -Compress -Depth 10
    $requests = @()
    $requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
    $requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
    $requests += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}"
    $requests += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"$toolName`",`"arguments`":$argPart}}"

    $inputText = ($requests -join "`n") + "`n"
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
    $stderrTask = $proc.StandardError.ReadToEndAsync()

    $proc.StandardInput.Write($inputText)
    $proc.StandardInput.Flush()
    $proc.StandardInput.Close()

    $waited = $proc.WaitForExit(30000)
    if (-not $waited) {
        try { $proc.Kill() } catch {}
        return $null
    }

    try { $stdoutTask.Wait(2000) | Out-Null } catch {}
    return $stdoutTask.Result
}

"=== Retesting tools with corrected arguments ==="
""

# Test 1: add_validation_point with correct 'operator' parameter
"--- Test: add_validation_point (operator='==') ---"
$r = Invoke-Tool 'add_validation_point' @{ table_node_id = $validTableId; left_expr = '[0,0]'; operator = '=='; right_expr = '0'; note = 'test' }
"[$($r.status)] $($r.msg)"
""

# Extract validation_id from response and use it
"--- Extracting validation_id from response ---"
$raw = Invoke-Tool-Raw 'add_validation_point' @{ table_node_id = $validTableId; left_expr = '[0,0]'; operator = '=='; right_expr = '0'; note = 'test2' }
$vid = $null
if ($raw) {
    $lines = $raw -split "`n"
    $resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
    if ($resp) {
        $obj = $resp | ConvertFrom-Json
        $text = $obj.result.content[0].text
        $inner = $text | ConvertFrom-Json
        $vid = $inner.validation_id
        "Extracted validation_id: $vid"
    }
}
""

# Test 2: update_validation_point with real ID
if ($vid) {
    "--- Test: update_validation_point (validation_id=$vid) ---"
    $r = Invoke-Tool 'update_validation_point' @{ validation_id = [long]$vid; note = 'updated-note' }
    "[$($r.status)] $($r.msg)"
    ""
}

# Test 3: run_validation with real ID
if ($vid) {
    "--- Test: run_validation (validation_id=$vid) ---"
    $r = Invoke-Tool 'run_validation' @{ validation_id = [long]$vid }
    "[$($r.status)] $($r.msg)"
    ""
}

# Test 4: remove_validation_point with real ID
if ($vid) {
    "--- Test: remove_validation_point (validation_id=$vid) ---"
    $r = Invoke-Tool 'remove_validation_point' @{ validation_id = [long]$vid }
    "[$($r.status)] $($r.msg)"
    ""
}

# Test 5: get_consolidate_config with table_node_id
"--- Test: get_consolidate_config (table_node_id=$validTableId) ---"
$r = Invoke-Tool 'get_consolidate_config' @{ table_node_id = $validTableId }
"[$($r.status)] $($r.msg)"
""

# Test 6: execute_consolidate with table_node_id
"--- Test: execute_consolidate (table_node_id=$validTableId) ---"
$r = Invoke-Tool 'execute_consolidate' @{ table_node_id = $validTableId; full_refresh = $false }
"[$($r.status)] $($r.msg)"
""

# Test 7: one_click_collect with table_node_id
"--- Test: one_click_collect (table_node_id=$validTableId) ---"
$r = Invoke-Tool 'one_click_collect' @{ table_node_id = $validTableId }
"[$($r.status)] $($r.msg)"
""

# Test 8: collect_cells with proper cell_indices (row=0,col=0 -> 0; row=1,col=0 -> 10000)
"--- Test: collect_cells (cell_indices=[0, 10000]) ---"
$r = Invoke-Tool 'collect_cells' @{ table_node_id = $validTableId; cell_indices = @(0, 10000); collect_key = 'test' }
"[$($r.status)] $($r.msg)"
""

# Test 9: collect_by_column with col_index (singular)
"--- Test: collect_by_column (col_index=0) ---"
$r = Invoke-Tool 'collect_by_column' @{ table_node_id = $validTableId; col_index = 0 }
"[$($r.status)] $($r.msg)"
""

# Test 10: create_snapshot then delete it
"--- Test: create_snapshot then delete it ---"
$raw = Invoke-Tool-Raw 'create_snapshot' @{ tree_node_id = $validTableId }
if ($raw) {
    $lines = $raw -split "`n"
    $resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
    if ($resp) {
        $obj = $resp | ConvertFrom-Json
        # Look for snapshot_id in response
        $text = $obj.result.content[0].text
        "Created snapshot response (truncated): $($text.Substring(0, [Math]::Min(200, $text.Length)))..."
    }
}
""

# Test 11: delete_snapshot with snapshot_id=1 (created earlier)
"--- Test: delete_snapshot (snapshot_id=1) ---"
$r = Invoke-Tool 'delete_snapshot' @{ snapshot_id = 1 }
"[$($r.status)] $($r.msg)"
""
