$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'
$validTableId = 5699421601793

function Invoke-Multi($calls) {
    $requests = @()
    $requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
    $requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
    $requests += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}"
    $id = 3
    foreach ($c in $calls) {
        $argPart = $c.args | ConvertTo-Json -Compress -Depth 10
        $requests += "{`"jsonrpc`":`"2.0`",`"id`":$id,`"method`":`"tools/call`",`"params`":{`"name`":`"$($c.name)`",`"arguments`":$argPart}}"
        $id++
    }

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

function Get-Response-Text($raw, $id) {
    if (-not $raw) { return $null }
    $lines = $raw -split "`n"
    $pattern = '"id":\s*' + $id
    $resp = $lines | Where-Object { $_ -match $pattern } | Select-Object -Last 1
    if (-not $resp) { return $null }
    $obj = $resp | ConvertFrom-Json
    if ($obj.result -and $obj.result.content) {
        return $obj.result.content[0].text
    }
    return $null
}

"=== Test 1: Page setup persistence ==="
"--- Get current page setup ---"
$raw = Invoke-Multi @(
    @{ name = 'get_page_setup'; args = @{ table_node_id = $validTableId } }
)
$text = Get-Response-Text $raw 3
if ($text) {
    $inner = $text | ConvertFrom-Json
    "Current margins: left=$($inner.margin_left), right=$($inner.margin_right)"
    "Current paper_kind: $($inner.paper_kind)"
}

""
"--- Set margins (left=5.5, right=5.5) ---"
$raw = Invoke-Multi @(
    @{ name = 'set_margins'; args = @{ table_node_id = $validTableId; left = 5.5; right = 5.5 } }
)
$text = Get-Response-Text $raw 3
if ($text) { "Set margins response: $text" }

""
"--- Reopen and check margins ---"
$raw = Invoke-Multi @(
    @{ name = 'get_page_setup'; args = @{ table_node_id = $validTableId } }
)
$text = Get-Response-Text $raw 3
if ($text) {
    $inner = $text | ConvertFrom-Json
    "After reopen, margins: left=$($inner.margin_left), right=$($inner.margin_right)"
    if ($inner.margin_left -eq 5.5) {
        "  -> Page setup IS persisted"
    } else {
        "  -> Page setup NOT persisted (save failed)"
    }
}

""
"=== Test 2: Snapshot persistence ==="
"--- Create snapshot ---"
$raw = Invoke-Multi @(
    @{ name = 'create_snapshot'; args = @{ tree_node_id = $validTableId } }
)
$text = Get-Response-Text $raw 3
if ($text) { "Create snapshot response: $($text.Substring(0, [Math]::Min(150, $text.Length)))" }

""
"--- Reopen and list snapshots ---"
$raw = Invoke-Multi @(
    @{ name = 'list_snapshots'; args = @{ tree_node_id = $validTableId } }
)
$text = Get-Response-Text $raw 3
if ($text) {
    $inner = $text | ConvertFrom-Json
    "After reopen, snapshot count: $($inner.total)"
    if ($inner.total -gt 0) {
        "  -> Snapshots ARE persisted"
    } else {
        "  -> Snapshots NOT persisted (save failed)"
    }
}

""
"=== Test 3: Validation persistence ==="
"--- Add validation ---"
$raw = Invoke-Multi @(
    @{ name = 'add_validation_point'; args = @{ table_node_id = $validTableId; left_expr = '0'; operator = '=='; right_expr = '0'; note = 'persist test' } }
)
$text = Get-Response-Text $raw 3
if ($text) {
    $inner = $text | ConvertFrom-Json
    "Added validation_id: $($inner.validation_id)"
}

""
"--- Reopen and list validations ---"
$raw = Invoke-Multi @(
    @{ name = 'list_validation_points'; args = @{} }
)
$text = Get-Response-Text $raw 3
if ($text) {
    $inner = $text | ConvertFrom-Json
    "After reopen, validation count: $($inner.total)"
    if ($inner.total -gt 19) {
        "  -> Validations ARE persisted"
    } else {
        "  -> Validations NOT persisted (save failed)"
    }
}
