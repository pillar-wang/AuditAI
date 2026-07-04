$dllPath = 'e:\lq\AuditAI\AuditAI\bin\Debug\net48\System.Resources.Extensions.dll'
$info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath)
Write-Output ('File Version: ' + $info.FileVersion)
Write-Output ('Product Version: ' + $info.ProductVersion)
$asm = [System.Reflection.AssemblyName]::GetAssemblyName($dllPath)
Write-Output ('Assembly Version: ' + $asm.Version)
Write-Output ('Public Key Token: ' + ([System.BitConverter]::ToString($asm.GetPublicKeyToken()).Replace('-','')))
