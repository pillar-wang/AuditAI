$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'
$validTableId = 5699421601793

function Invoke-Multi($calls) {
    # calls: array of @{ name=...; args=@{...} }
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

"=== Testing run_validation with real validation point ==="
""
"--- Step 1: Add validation point with proper formula ---"
# Use a real formula that should evaluate (sum of column 0)
$raw = Invoke-Multi @(
    @{ name = 'add_validation_point'; args = @{ table_node_id = $validTableId; left_expr = '0'; operator = '=='; right_expr = '0'; note = 'equality test' } }
)

if ($raw) {
    $lines = $raw -split "`n"
    $resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
    if ($resp) {
        $obj = $resp | ConvertFrom-Json
        $text = $obj.result.content[0].text
        "Add response: $text"
        $inner = $text | ConvertFrom-Json
        $vid = $inner.validation_id
        "Extracted validation_id: $vid"
    }
}

""
"--- Step 2: Run validation (separate session to ensure state is saved) ---"
if ($vid) {
    $raw2 = Invoke-Multi @(
        @{ name = 'run_validation'; args = @{ validation_id = [long]$vid } }
    )
    if ($raw2) {
        $lines = $raw2 -split "`n"
        $resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
        if ($resp) {
            $obj = $resp | ConvertFrom-Json
            $text = $obj.result.content[0].text
            "Run validation response:"
            $text
        } else {
            "NO RESPONSE for run_validation"
            "--- Last 5 lines of stdout ---"
            $lines | Select-Object -Last 5
        }
    }
}

""
"--- Step 3: Cleanup - remove the validation point ---"
if ($vid) {
    $raw3 = Invoke-Multi @(
        @{ name = 'remove_validation_point'; args = @{ validation_id = [long]$vid } }
    )
    if ($raw3) {
        $lines = $raw3 -split "`n"
        $resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
        if ($resp) {
            $obj = $resp | ConvertFrom-Json
            $text = $obj.result.content[0].text
            "Cleanup: $text"
        }
    }
}
