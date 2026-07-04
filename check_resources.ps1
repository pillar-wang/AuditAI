$asm = [System.Reflection.Assembly]::LoadFrom('e:\lq\AuditAI\AuditAI\bin\Debug\net48\AuditAI.exe')
$names = $asm.GetManifestResourceNames()
Write-Output "嵌入资源清单名 (共 $($names.Count) 个):"
$names | Sort-Object | ForEach-Object { Write-Output "  $_" }
Write-Output ""
Write-Output "查找 userlogin 相关资源:"
$names | Where-Object { $_ -match 'userlogin|Properties.Resources' } | ForEach-Object { Write-Output "  匹配: $_" }
