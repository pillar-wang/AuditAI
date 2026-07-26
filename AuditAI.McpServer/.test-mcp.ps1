$ErrorActionPreference = 'Stop'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$tmp = 'e:\lq\AuditAI\AuditAI.McpServer\.test-output.txt'
$requests = @(
    '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
    '{"jsonrpc":"2.0","method":"notifications/initialized"}'
    '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
    '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"list_projects","arguments":{}}}'
)
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
$proc.StandardInput.Write($inputText)
$proc.StandardInput.Flush()
$proc.StandardInput.Close()
$out = $proc.StandardOutput.ReadToEnd()
$err = $proc.StandardError.ReadToEnd()
if (-not $proc.HasExited) { try { $proc.Kill() } catch {} }
"=== STDOUT ===" | Out-File -FilePath $tmp -Encoding UTF8
$out | Out-File -FilePath $tmp -Encoding UTF8 -Append
"`n=== STDERR ===" | Out-File -FilePath $tmp -Encoding UTF8 -Append
$err | Out-File -FilePath $tmp -Encoding UTF8 -Append
Write-Host "===== STDOUT ====="
Write-Host $out
Write-Host "===== STDERR (last 20 lines) ====="
$err -split "`n" | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
