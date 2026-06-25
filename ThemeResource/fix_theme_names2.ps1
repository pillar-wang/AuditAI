# PowerShell script to fix C1 theme XML Name attributes in the resx file
# Uses ResXResourceReader/Writer for proper resx handling

Add-Type -AssemblyName System.Windows.Forms

$resxPath = "e:\lq\AuditAI\ThemeResource\Auditai.ThemeResource.Properties.Resource1.resx"
$backupPath = "e:\lq\AuditAI\ThemeResource\Auditai.ThemeResource.Properties.Resource1.resx.bak"

# Backup original
Copy-Item $resxPath $backupPath -Force
Write-Output "Backup created: $backupPath"

# Read and process the resx file
$reader = New-Object System.Resources.ResXResourceReader $resxPath
$reader.BasePath = Split-Path $resxPath -Parent

# Create temp resx for writing
$tempResx = [System.IO.Path]::GetTempFileName() + ".resx"
$writer = New-Object System.Resources.ResXResourceWriter $tempResx

$fixedCount = 0
$totalCount = 0

foreach ($entry in $reader.GetEnumerator()) {
    $key = $entry.Key
    $value = $entry.Value
    
    if ($value -is [byte[]]) {
        $totalCount++
        
        # Check if the bytes contain a C1 theme (starts with XML BOM)
        if ($value.Length -gt 10 -and $value[0] -eq 0xEF -and $value[1] -eq 0xBB -and $value[2] -eq 0xBF) {
            # Decode to string
            $xmlString = [System.Text.Encoding]::UTF8.GetString($value)
            
            # Check if it contains "leqi_" in Name attribute
            if ($xmlString -match 'Name="leqi_') {
                Write-Output "Found old name in: $key"
                
                # Replace all Name="leqi_XXX" with Name="auditai_XXX"
                $newXml = $xmlString -replace 'Name="leqi_', 'Name="auditai_'
                
                if ($newXml -ne $xmlString) {
                    # Convert back to bytes
                    $newBytes = [System.Text.Encoding]::UTF8.GetBytes($newXml)
                    $writer.AddResource($key, $newBytes)
                    $fixedCount++
                    Write-Output "  Fixed: $key"
                    continue
                }
            }
        }
    }
    
    # Add unchanged
    $writer.AddResource($key, $value)
}

$writer.Close()
$reader.Close()

Write-Output "Total byte[] entries: $totalCount"
Write-Output "Fixed: $fixedCount"

# Replace original with temp
Copy-Item $tempResx $resxPath -Force
Remove-Item $tempResx -Force

Write-Output "Resx saved to: $resxPath"