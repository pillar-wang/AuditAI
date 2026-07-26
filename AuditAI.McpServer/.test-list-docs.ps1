$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'

$requests = @()
$requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
$requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
$requests += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}"
$requests += '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"list_nodes_by_type","arguments":{"node_type":"document"}}}'
$requests += '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"list_nodes_by_type","arguments":{"node_type":"table"}}}'

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
$proc.StandardInput.Write($inputText)
$proc.StandardInput.Flush()
$proc.StandardInput.Close()
$proc.WaitForExit(15000) | Out-Null
try { $stdoutTask.Wait(2000) | Out-Null } catch {}
$raw = $stdoutTask.Result

"=== RAW OUTPUT ==="
$raw
"=== END ==="

"=== Parse id=3 (document) ==="
$lines = $raw -split "`n"
$resp3 = $lines | Where-Object { $_ -match '"id":\s*3' } | Select-Object -Last 1
if ($resp3) {
    $obj = $resp3 | ConvertFrom-Json
    if ($obj.result.content) {
        "Got content: $($obj.result.content[0].text)"
    } elseif ($obj.error) {
        "Error: $($obj.error | ConvertTo-Json -Compress)"
    } else {
        "Unknown: $resp3"
    }
} else {
    "No response for id=3"
}

"=== Parse id=4 (table) ==="
$resp4 = $lines | Where-Object { $_ -match '"id":\s*4' } | Select-Object -Last 1
if ($resp4) {
    $obj = $resp4 | ConvertFrom-Json
    if ($obj.result.content) {
        $tobj = $obj.result.content[0].text | ConvertFrom-Json
        "Got table count: $($tobj.total)"
        if ($tobj.nodes) {
            "First 3 tables:"
            $tobj.nodes | Select-Object -First 3 | ForEach-Object { "  id=$($_.id) name=$($_.name) type=$($_.type)" }
        }
    }
}
