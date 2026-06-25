# PowerShell script to fix C1 theme XML Name attributes in the resx file
# Changes Name="leqi_XXX" to Name="auditai_XXX" in the base64-encoded data

$resxPath = "e:\lq\AuditAI\ThemeResource\Auditai.ThemeResource.Properties.Resource1.resx"
$backupPath = "e:\lq\AuditAI\ThemeResource\Auditai.ThemeResource.Properties.Resource1.resx.bak"

# Backup original
Copy-Item $resxPath $backupPath -Force
Write-Output "Backup created: $backupPath"

# Load the resx file
[xml]$resx = Get-Content $resxPath -Encoding UTF8

# Define the mapping of resource names
$themeNames = @{
    "auditai_MacBlue" = "MacBlue"
    "auditai_MacSilver" = "MacSilver"
    "auditai_Office2013Green" = "Office2013Green"
    "auditai_Office2013LightGray" = "Office2013LightGray"
    "auditai_Office2013Red" = "Office2013Red"
    "auditai_Office2016Blue" = "Office2016Blue"
    "auditai_Office2016Green" = "Office2016Green"
    "auditai_Office2016Red" = "Office2016Red"
    "auditai_VS2013Blue" = "VS2013Blue"
    "auditai_VS2013Green" = "VS2013Green"
    "auditai_VS2013Light" = "VS2013Light"
    "auditai_VS2013Purple" = "VS2013Purple"
    "auditai_VS2013Red" = "VS2013Red"
    "auditai_VS2013Tan" = "VS2013Tan"
}

$fixedCount = 0

foreach ($dataNode in $resx.root.data) {
    $name = $dataNode.name
    if ($themeNames.ContainsKey($name)) {
        $suffix = $themeNames[$name]
        $oldNameInXml = "leqi_$suffix"
        $newNameInXml = "auditai_$suffix"
        
        if ($dataNode.value -ne $null) {
            $base64 = $dataNode.value.'#text'
            if ($base64 -ne $null -and $base64.Length -gt 0) {
                # Decode base64 to bytes
                $bytes = [Convert]::FromBase64String($base64)
                
                # Convert bytes to XML string (UTF-8)
                $xmlString = [System.Text.Encoding]::UTF8.GetString($bytes)
                
                # Check if it contains the old name
                if ($xmlString.Contains($oldNameInXml)) {
                    # Replace old name with new name
                    $xmlString = $xmlString.Replace($oldNameInXml, $newNameInXml)
                    
                    # Convert back to bytes and base64
                    $newBytes = [System.Text.Encoding]::UTF8.GetBytes($xmlString)
                    $newBase64 = [Convert]::ToBase64String($newBytes)
                    
                    # Update the resx node
                    $dataNode.value.'#text' = $newBase64
                    $fixedCount++
                    Write-Output "Fixed: $name ($oldNameInXml -> $newNameInXml)"
                } else {
                    Write-Output "Skipped (no old name): $name"
                }
            }
        }
    }
}

Write-Output "`nFixed $fixedCount theme entries."

# Save the modified resx
$resx.Save($resxPath)
Write-Output "Resx saved to: $resxPath"