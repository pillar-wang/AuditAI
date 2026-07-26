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

# Step 1: Get baseline count
"=== Step 1: Baseline count ==="
$raw = Invoke-Multi @(
    @{ name = 'list_validation_points'; args = @{} }
)
$text = Get-Response-Text $raw 3
if ($text) {
    $inner = $text | ConvertFrom-Json
    "Baseline validation count: $($inner.total)"
} else {
    "Failed to get baseline"
    exit 1
}

# Step 2: Add validation in same session, then list
""
"=== Step 2: Add validation and list in same session ==="
$raw = Invoke-Multi @(
    @{ name = 'add_validation_point'; args = @{ table_node_id = $validTableId; left_expr = '0'; operator = '=='; right_expr = '0'; note = 'persist test' } }
    @{ name = 'list_validation_points'; args = @{} }
)
$addText = Get-Response-Text $raw 3
$listText = Get-Response-Text $raw 4
if ($addText) {
    $addObj = $addText | ConvertFrom-Json
    "Added validation_id: $($addObj.validation_id)"
    $addedId = $addObj.validation_id
}
if ($listText) {
    $listObj = $listText | ConvertFrom-Json
    "After add (same session), count: $($listObj.total)"
    $found = $listObj.validation_points | Where-Object { $_.id -eq $addedId }
    if ($found) {
        "  -> New validation IS in list (in-memory works)"
    } else {
        "  -> New validation NOT in list (even in-memory doesn't work!)"
    }
}

# Step 3: New session, check if persisted
""
"=== Step 3: New session - check persistence ==="
$raw = Invoke-Multi @(
    @{ name = 'list_validation_points'; args = @{} }
)
$text = Get-Response-Text $raw 3
if ($text) {
    $inner = $text | ConvertFrom-Json
    "After reopen, count: $($inner.total)"
    $found = $inner.validation_points | Where-Object { $_.id -eq $addedId }
    if ($found) {
        "  -> Validation IS persisted to disk"
    } else {
        "  -> Validation NOT persisted to disk (save failed silently)"
    }
}
