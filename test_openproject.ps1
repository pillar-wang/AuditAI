# Test OpenProject fix - build succeeded to original directory
$serverPath = "E:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe"
$workingDir = "E:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462"

$projectId = "69cf6ff2-2c54-4eae-b048-afcfe87736ec"
$projectPath = "data\1\$projectId.db"
$projectFullPath = "$workingDir\data\1\$projectId.db"

Write-Host "=== MCP Server Test ===" -ForegroundColor White
Write-Host "Server: $serverPath"
Write-Host "Project exists: $(Test-Path $projectFullPath)"

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $serverPath
$psi.RedirectStandardInput = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true
$psi.WorkingDirectory = $workingDir

$proc = [System.Diagnostics.Process]::Start($psi)
Start-Sleep -Milliseconds 2000

if ($proc.HasExited) {
    Write-Host "FAIL: Process exited immediately! Code=$($proc.ExitCode)" -ForegroundColor Red
    $err = $proc.StandardError.ReadToEnd()
    if ($err) { Write-Host "STDERR: $err" -ForegroundColor Yellow }
    exit
}
Write-Host "OK: Process running PID=$($proc.Id)" -ForegroundColor Green

# initialize
$proc.StandardInput.WriteLine('{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}')
Start-Sleep -Milliseconds 1000
$resp = ""
$timer = [System.Diagnostics.Stopwatch]::StartNew()
while ($timer.Elapsed.TotalSeconds -lt 5 -and -not $proc.StandardOutput.EndOfStream) {
    $line = $proc.StandardOutput.ReadLine()
    $resp += $line
    if ($line.Trim().EndsWith('}')) { break }
    Start-Sleep -Milliseconds 100
}
$timer.Stop()
Write-Host "OK: Initialize response received" -ForegroundColor Green

# initialized notification
$proc.StandardInput.WriteLine('{"jsonrpc":"2.0","method":"notifications/initialized"}')
Start-Sleep -Milliseconds 500

# open_project
$pathEsc = $projectPath.Replace('\', '\\')
$openReq = '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"open_project","arguments":{"path":"' + $pathEsc + '"}}}'
$proc.StandardInput.WriteLine($openReq)

Start-Sleep -Milliseconds 5000

$resp = ""
$timer = [System.Diagnostics.Stopwatch]::StartNew()
while ($timer.Elapsed.TotalSeconds -lt 15 -and -not $proc.StandardOutput.EndOfStream) {
    $line = $proc.StandardOutput.ReadLine()
    $resp += $line + "`n"
    if ($line.Trim().EndsWith('}')) { break }
    Start-Sleep -Milliseconds 200
}
$timer.Stop()

if ($resp -match '"success"\s*:\s*true') {
    Write-Host "PASS: open_project succeeded!" -ForegroundColor Green
    Write-Host $resp
} else {
    Write-Host "FAIL: open_project failed!" -ForegroundColor Red
    Write-Host $resp
}

# get_project_info
$proc.StandardInput.WriteLine('{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"get_project_info","arguments":{}}}')
Start-Sleep -Milliseconds 3000
$resp = ""
$timer = [System.Diagnostics.Stopwatch]::StartNew()
while ($timer.Elapsed.TotalSeconds -lt 5 -and -not $proc.StandardOutput.EndOfStream) {
    $line = $proc.StandardOutput.ReadLine()
    $resp += $line + "`n"
    if ($line.Trim().EndsWith('}')) { break }
    Start-Sleep -Milliseconds 100
}
$timer.Stop()
if ($resp -match '"success"') {
    Write-Host "OK: get_project_info response:" -ForegroundColor Cyan
    Write-Host $resp
}

# stderr
Start-Sleep -Milliseconds 500
$err = $proc.StandardError.ReadToEnd()
if ($err) { Write-Host "STDERR: $err" -ForegroundColor Yellow }

$proc.Kill()
$proc.Dispose()
Write-Host "=== DONE ===" -ForegroundColor White
