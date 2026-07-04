$asm = [System.Reflection.Assembly]::LoadFrom('e:\lq\AuditAI\AuditAI\bin\Debug\net48\AuditAI.exe')
$stream = $asm.GetManifestResourceStream('Auditai.UI.Platform.Properties.Resources.resources')
if ($null -eq $stream) {
    Write-Output 'ERROR: stream is null'
    return
}
Write-Output ('Stream size: ' + $stream.Length + ' bytes')
$reader = New-Object System.Resources.ResourceReader($stream)
$count = 0
$hasUserlogin = $false
$keys = @()
foreach ($entry in $reader) {
    $count++
    $keys += $entry.Key
    if ($entry.Key -eq 'userlogin') { $hasUserlogin = $true }
}
Write-Output ('Total entries: ' + $count)
Write-Output ('Has userlogin: ' + $hasUserlogin)
Write-Output 'First 20 keys:'
$keys | Select-Object -First 20 | ForEach-Object { Write-Output ('  ' + $_) }
