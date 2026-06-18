# View all bookmarks in template database Paragraph.Stream
param(
    [string]$DbPath = "e:\lq\LeqiAudit-Decompiled\LeqiAudit\Data\Templates\X004模板.db"
)

# Load SQLite assembly
$sqliteDll = "e:\lq\LeqiAudit-Decompiled\Libs\System.Data.SQLite.dll"
$interopX64 = "e:\lq\LeqiAudit\x64\SQLite.Interop.dll"
$interopX86 = "e:\lq\LeqiAudit\x86\SQLite.Interop.dll"

# Ensure SQLite.Interop.dll is reachable (copy to script directory)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (Test-Path $interopX64) {
    Copy-Item -Path $interopX64 -Destination (Join-Path $scriptDir "SQLite.Interop.dll") -Force
    Write-Host ("[OK] Copied SQLite.Interop.dll (x64)") -ForegroundColor DarkGray
} elseif (Test-Path $interopX86) {
    Copy-Item -Path $interopX86 -Destination (Join-Path $scriptDir "SQLite.Interop.dll") -Force
    Write-Host ("[OK] Copied SQLite.Interop.dll (x86)") -ForegroundColor DarkGray
}

if (Test-Path $sqliteDll) {
    Add-Type -Path $sqliteDll
    Write-Host ("[OK] Loaded: " + $sqliteDll) -ForegroundColor DarkGray
} else {
    Add-Type -AssemblyName "System.Data.SQLite" -ErrorAction SilentlyContinue
}
if (-not ("System.Data.SQLite.SQLiteConnection" -as [type])) {
    Write-Error "System.Data.SQLite not found"
    exit 1
}

$connStr = "Data Source=$DbPath;Version=3;"
$conn = New-Object System.Data.SQLite.SQLiteConnection($connStr)
$conn.Open()

# List all objects to find correct name
$listCmd = New-Object System.Data.SQLite.SQLiteCommand("SELECT type, name, sql FROM sqlite_master ORDER BY type, name", $conn)
$listReader = $listCmd.ExecuteReader()
$tableNames = @()
Write-Host "All objects (type, name):" -ForegroundColor DarkGray
while ($listReader.Read()) {
    $line = "  [" + $listReader["type"] + "] " + $listReader["name"]
    Write-Host $line -ForegroundColor DarkGray
    if ($listReader["type"] -eq "table") { $tableNames += $listReader["name"] }
}
$listReader.Close()
Write-Host ("Total tables: " + $tableNames.Count) -ForegroundColor DarkGray

# Find paragraph table (case-insensitive)
$paraTable = $tableNames | Where-Object { $_ -ieq "paragraph" } | Select-Object -First 1
if (-not $paraTable) {
    Write-Error "No paragraph table found"
    exit 1
}
Write-Host ("Using table: " + $paraTable) -ForegroundColor DarkGray

$query = "SELECT [Id], [DocumentId], [Index], [Stream] FROM [$paraTable] WHERE [Status]<2 ORDER BY [DocumentId], [Index]"
$cmd = New-Object System.Data.SQLite.SQLiteCommand($query, $conn)
$reader = $cmd.ExecuteReader()

$bookmarkCount = 0
$noTableIdCount = 0
$withTableIdCount = 0

Write-Host ""
Write-Host "=== Bookmark Analysis Report ===" -ForegroundColor Cyan
Write-Host ("DB: " + $DbPath)
Write-Host ""

while ($reader.Read()) {
    $id = $reader["Id"]
    $docId = $reader["DocumentId"]
    $idx = $reader["Index"]
    $streamBytes = $reader["Stream"]

    if ($streamBytes -eq $null -or $streamBytes.Length -eq 0) { continue }

    try {
        $inputMs = New-Object System.IO.MemoryStream(,$streamBytes)
        $deflate = New-Object System.IO.Compression.DeflateStream($inputMs, [System.IO.Compression.CompressionMode]::Decompress)
        $outputMs = New-Object System.IO.MemoryStream
        $deflate.CopyTo($outputMs)
        $deflate.Close()
        $xml = [System.Text.Encoding]::UTF8.GetString($outputMs.ToArray())

        $pattern = '<w:bookmarkStart[^>]*w:name=' + [char]34 + '([^' + [char]34 + ']+)' + [char]34
        $regex = New-Object System.Text.RegularExpressions.Regex($pattern)
        $matches = $regex.Matches($xml)

        foreach ($m in $matches) {
            $name = $m.Groups[1].Value
            if (-not $name.StartsWith("lsbm")) { continue }
            $bookmarkCount++

            $separator = "@"
            if ($name.Contains("_")) { $separator = "_" }

            $parts = $name.Split($separator.ToCharArray(), [System.StringSplitOptions]::RemoveEmptyEntries)

            $hasTableId = $false
            $tableIdField = ""
            for ($i = 0; $i -lt $parts.Length - 1; $i++) {
                if ($parts[$i] -eq "2") {
                    $tableIdField = $parts[$i + 1]
                    if (-not [string]::IsNullOrEmpty($tableIdField)) {
                        $hasTableId = $true
                    }
                    break
                }
            }

            $line = ("Doc=" + $docId + " Idx=" + $idx + " | " + $name)
            if ($hasTableId) {
                $withTableIdCount++
                $line = $line + " | TableId=" + $tableIdField
                Write-Host $line -ForegroundColor Green
            } else {
                $noTableIdCount++
                $line = $line + " | (NO TableId)"
                Write-Host $line -ForegroundColor Yellow
            }
        }
    } catch {
        Write-Warning ("Decompress failed Doc=" + $docId + " Idx=" + $idx + " : " + $_.Exception.Message)
    }
}

$reader.Close()
$conn.Close()

Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host ("Total bookmarks: " + $bookmarkCount)
Write-Host ("With TableId:   " + $withTableIdCount) -ForegroundColor Green
Write-Host ("Without TableId:" + $noTableIdCount) -ForegroundColor Yellow
Write-Host ""
