param(
    [Parameter(Mandatory=$true)][string]$ToolName,
    [string]$ArgumentsJson = '{}'
)
$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'

# Build requests as raw JSON strings (using PowerShell escape sequences for quotes)
$requests = @()
$requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
$requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
$requests += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}"

# Parse and re-serialize the arguments to ensure compact JSON
$argObj = $ArgumentsJson | ConvertFrom-Json
$argPart = $argObj | ConvertTo-Json -Compress -Depth 10
$toolReq = "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"$ToolName`",`"arguments`":$argPart}}"
$requests += $toolReq

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

# Start async reads to avoid deadlock
$stdoutTask = $proc.StandardOutput.ReadToEndAsync()
$stderrTask = $proc.StandardError.ReadToEndAsync()

$proc.StandardInput.Write($inputText)
$proc.StandardInput.Flush()
$proc.StandardInput.Close()

# Wait up to 30 seconds for exit
$waited = $proc.WaitForExit(30000)
if (-not $waited) {
    try { $proc.Kill() } catch {}
    "TIMEOUT (killed after 30s): $ToolName"
    exit 2
}

# Give async tasks a moment to complete
try { $stdoutTask.Wait(2000) | Out-Null } catch {}
try { $stderrTask.Wait(2000) | Out-Null } catch {}
$out = $stdoutTask.Result
$err = $stderrTask.Result

# Get the response for id:3
$lines = $out -split "`n"
$resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
if (-not $resp) {
    "NO RESPONSE for $ToolName (output has $($lines.Count) lines)"
    "--- Last 5 lines of stdout ---"
    $lines | Select-Object -Last 5 | ForEach-Object { "  $_" }
    "--- Last 5 lines of stderr ---"
    ($err -split "`n") | Select-Object -Last 5 | ForEach-Object { "  $_" }
    exit 3
}

# Parse the response
try {
    $obj = $resp | ConvertFrom-Json
    if ($obj.error) {
        "ERROR: $($obj.error.message)"
        exit 1
    }
    if ($obj.result -and $obj.result.content) {
        $text = ($obj.result.content | Where-Object { $_.type -eq "text" } | Select-Object -First 1).text
        try {
            $inner = $text | ConvertFrom-Json
            if ($inner.success -eq $false) {
                "FAIL: $($inner.error)"
                exit 1
            }
            "OK: $($text.Substring(0, [Math]::Min(300, $text.Length)))"
            exit 0
        } catch {
            "OK (non-JSON): $($text.Substring(0, [Math]::Min(300, $text.Length)))"
            exit 0
        }
    }
    "UNKNOWN: $resp"
    exit 4
} catch {
    "PARSE ERROR: $_"
    exit 5
}
