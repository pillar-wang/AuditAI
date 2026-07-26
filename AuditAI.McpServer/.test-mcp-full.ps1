$ErrorActionPreference = 'Stop'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$templatePath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $templatePath -replace '\\','\\'

$requests = @()
$requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
$requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
$requests += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}"
$requests += '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"get_project_info","arguments":{}}}'
$requests += '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"get_project_tree","arguments":{}}}'

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

$out | Out-File -FilePath 'e:\lq\AuditAI\AuditAI.McpServer\.test-output-full.txt' -Encoding UTF8
$err | Out-File -FilePath 'e:\lq\AuditAI\AuditAI.McpServer\.test-stderr-full.txt' -Encoding UTF8

Write-Host "===== STDOUT (first 5000 chars) ====="
if ($out.Length -gt 5000) { Write-Host $out.Substring(0, 5000) } else { Write-Host $out }
Write-Host ""
Write-Host "===== STDOUT (last 3000 chars) ====="
if ($out.Length -gt 8000) { Write-Host $out.Substring($out.Length - 3000) } else { Write-Host "(see above)" }
Write-Host ""
Write-Host "===== STDERR (last 30 lines) ====="
$err -split "`n" | Select-Object -Last 30 | ForEach-Object { Write-Host $_ }
