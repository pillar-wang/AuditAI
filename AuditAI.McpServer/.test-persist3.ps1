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

"=== Before set: get page setup ==="
$raw = Invoke-Multi @(
    @{ name = 'get_page_setup'; args = @{ table_node_id = $validTableId } }
)
$text = Get-Response-Text $raw 3
$before = $text | ConvertFrom-Json
"Before: left_margin=$($before.left_margin), right_margin=$($before.right_margin), paper_kind=$($before.paper_kind)"

""
"=== Set margins (left=5.5, right=5.5) and paper_kind=A3 ==="
$raw = Invoke-Multi @(
    @{ name = 'set_margins'; args = @{ table_node_id = $validTableId; left = 5.5; right = 5.5 } }
)
$text = Get-Response-Text $raw 3
"Set response: $text"

$raw = Invoke-Multi @(
    @{ name = 'set_paper_size'; args = @{ table_node_id = $validTableId; paper_kind = 'A3' } }
)
$text = Get-Response-Text $raw 3
"Set paper response: $text"

""
"=== After reopen: get page setup ==="
$raw = Invoke-Multi @(
    @{ name = 'get_page_setup'; args = @{ table_node_id = $validTableId } }
)
$text = Get-Response-Text $raw 3
$after = $text | ConvertFrom-Json
"After: left_margin=$($after.left_margin), right_margin=$($after.right_margin), paper_kind=$($after.paper_kind)"

if ($after.left_margin -eq 5.5 -and $after.right_margin -eq 5.5) {
    "  -> Margins ARE persisted"
} else {
    "  -> Margins NOT persisted (expected 5.5/5.5, got $($after.left_margin)/$($after.right_margin))"
}

if ($after.paper_kind -eq 'A3') {
    "  -> PaperKind IS persisted"
} else {
    "  -> PaperKind NOT persisted (expected A3, got $($after.paper_kind))"
}

""
"=== Validation test ==="
$raw = Invoke-Multi @(
    @{ name = 'list_validation_points'; args = @{} }
)
$text = Get-Response-Text $raw 3
$before = $text | ConvertFrom-Json
"Before: validation count = $($before.total)"

$raw = Invoke-Multi @(
    @{ name = 'add_validation_point'; args = @{ table_node_id = $validTableId; left_expr = '0'; operator = '=='; right_expr = '0'; note = 'persist3 test' } }
)
$text = Get-Response-Text $raw 3
$added = $text | ConvertFrom-Json
"Added validation_id: $($added.validation_id)"

$raw = Invoke-Multi @(
    @{ name = 'list_validation_points'; args = @{} }
)
$text = Get-Response-Text $raw 3
$after = $text | ConvertFrom-Json
"After (same session): validation count = $($after.total)"

$raw = Invoke-Multi @(
    @{ name = 'list_validation_points'; args = @{} }
)
$text = Get-Response-Text $raw 3
$afterReopen = $text | ConvertFrom-Json
"After reopen: validation count = $($afterReopen.total)"

if ($afterReopen.total -gt $before.total) {
    "  -> Validation IS persisted"
} else {
    "  -> Validation NOT persisted (before=$($before.total), after reopen=$($afterReopen.total))"
}
