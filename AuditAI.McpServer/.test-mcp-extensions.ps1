# Test new fine-grained extension MCP tools
# Server reads newline-delimited JSON via Console.ReadLine()
# Verifies: AdvancedTableTools / NodeManagementTools / CellFormatTools / DocumentExtensionTools / ProjectExtensionTools
# Note: script uses only ASCII to avoid PowerShell.exe GBK encoding issues with UTF-8 source files

$ErrorActionPreference = 'Stop'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$srcDb = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$tmpDb = 'e:\lq\AuditAI\AuditAI.McpServer\.test_tmp.db'
$backupZip = 'e:\lq\AuditAI\AuditAI.McpServer\.test_backup.zip'

if (-not (Test-Path $exe)) { Write-Error "exe not found: $exe"; exit 1 }
if (-not (Test-Path $srcDb)) { Write-Error "source db not found: $srcDb"; exit 1 }

# Copy template to temp db to avoid modifying the original
Copy-Item -Path $srcDb -Destination $tmpDb -Force
$dbPath = ($tmpDb -replace '\\','/')
Write-Host "Using temp db: $dbPath" -ForegroundColor DarkGray

# Build request list (placeholders will be replaced in phase 2)
$requests = @()
$requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
$requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
$requests += '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"open_project","arguments":{"path":"' + $dbPath + '"}}}'
$requests += '{"jsonrpc":"2.0","id":3,"method":"tools/list","params":{}}'
$requests += '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"get_project_tree","arguments":{}}}'

# AdvancedTableTools (use first table node id)
$requests += '{"jsonrpc":"2.0","id":10,"method":"tools/call","params":{"name":"get_table_info","arguments":{"table_node_id":__TABLE_ID__}}}'
$requests += '{"jsonrpc":"2.0","id":11,"method":"tools/call","params":{"name":"set_column_width","arguments":{"table_node_id":__TABLE_ID__,"col":0,"width":150}}}'
$requests += '{"jsonrpc":"2.0","id":12,"method":"tools/call","params":{"name":"set_row_role","arguments":{"table_node_id":__TABLE_ID__,"row":0,"role":"Header"}}}'
$requests += '{"jsonrpc":"2.0","id":13,"method":"tools/call","params":{"name":"set_frozen_columns","arguments":{"table_node_id":__TABLE_ID__,"frozen_count":1}}}'
$requests += '{"jsonrpc":"2.0","id":14,"method":"tools/call","params":{"name":"get_control_formula","arguments":{"table_node_id":__TABLE_ID__}}}'

# CellFormatTools
$requests += '{"jsonrpc":"2.0","id":20,"method":"tools/call","params":{"name":"get_cell_style","arguments":{"table_node_id":__TABLE_ID__,"row":0,"col":0}}}'
$requests += '{"jsonrpc":"2.0","id":21,"method":"tools/call","params":{"name":"set_cell_comment","arguments":{"table_node_id":__TABLE_ID__,"row":0,"col":0,"comment":"test-comment-audit"}}}'
$requests += '{"jsonrpc":"2.0","id":22,"method":"tools/call","params":{"name":"get_cell_comment","arguments":{"table_node_id":__TABLE_ID__,"row":0,"col":0}}}'
$requests += '{"jsonrpc":"2.0","id":23,"method":"tools/call","params":{"name":"set_cell_data_format","arguments":{"table_node_id":__TABLE_ID__,"row":0,"col":0,"format_type":"Number","decimal_length":2}}}'

# NodeManagementTools (use directory node id)
$requests += '{"jsonrpc":"2.0","id":30,"method":"tools/call","params":{"name":"get_node_info","arguments":{"node_id":__DIR_ID__}}}'
$requests += '{"jsonrpc":"2.0","id":31,"method":"tools/call","params":{"name":"set_node_number","arguments":{"node_id":__DIR_ID__,"number":"1.1"}}}'
$requests += '{"jsonrpc":"2.0","id":32,"method":"tools/call","params":{"name":"get_node_path_by_id","arguments":{"node_id":__DIR_ID__}}}'
$requests += '{"jsonrpc":"2.0","id":33,"method":"tools/call","params":{"name":"list_nodes_by_type","arguments":{"node_type":"table"}}}'

# DocumentExtensionTools (need to create a document first; id=99 will be the placeholder for new doc)
$requests += '{"jsonrpc":"2.0","id":98,"method":"tools/call","params":{"name":"create_document_node","arguments":{"parent_id":__DIR_ID__,"name":"test-audit-doc"}}}'
$requests += '{"jsonrpc":"2.0","id":40,"method":"tools/call","params":{"name":"get_document_info","arguments":{"document_node_id":__DOC_ID__}}}'
$requests += '{"jsonrpc":"2.0","id":41,"method":"tools/call","params":{"name":"insert_paragraph_at","arguments":{"document_node_id":__DOC_ID__,"position":0,"text":"test-paragraph-audit-note"}}}'
$requests += '{"jsonrpc":"2.0","id":42,"method":"tools/call","params":{"name":"set_paragraph_comment","arguments":{"document_node_id":__DOC_ID__,"paragraph_index":0,"comment":"key-paragraph"}}}'
$requests += '{"jsonrpc":"2.0","id":43,"method":"tools/call","params":{"name":"get_paragraph_info","arguments":{"document_node_id":__DOC_ID__,"paragraph_index":0}}}'

# ProjectExtensionTools (no delete_project to avoid breaking test data)
$requests += '{"jsonrpc":"2.0","id":50,"method":"tools/call","params":{"name":"get_project_properties","arguments":{"project_id":"__PROJECT_ID__"}}}'
$requests += '{"jsonrpc":"2.0","id":51,"method":"tools/call","params":{"name":"get_project_stats","arguments":{"project_id":"__PROJECT_ID__"}}}'
$requests += '{"jsonrpc":"2.0","id":52,"method":"tools/call","params":{"name":"set_project_properties","arguments":{"project_id":"__PROJECT_ID__","properties":{"note":"mcp-test-update"}}}}'
$requests += '{"jsonrpc":"2.0","id":53,"method":"tools/call","params":{"name":"backup_project","arguments":{"project_id":"__PROJECT_ID__","output_path":"' + ($backupZip -replace '\\','/') + '","include_attachments":false}}}'

# Phase 1: init + open project + list tools + get tree
$phase1Requests = $requests | Where-Object { $_ -notmatch '__' }
$inputText1 = ($phase1Requests -join "`n") + "`n"

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $exe
$psi.RedirectStandardInput = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true
$psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
$psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8

Write-Host "=== Phase 1: init + open + tools/list + tree ===" -ForegroundColor Cyan
$proc = [System.Diagnostics.Process]::Start($psi)
$proc.StandardInput.Write($inputText1)
$proc.StandardInput.Flush()
$proc.StandardInput.Close()
$out1 = $proc.StandardOutput.ReadToEnd()
$err1 = $proc.StandardError.ReadToEnd()
if (-not $proc.HasExited) { try { $proc.Kill() } catch {} }

# Parse responses
$lines1 = $out1 -split "`n" | Where-Object { $_.Trim() -ne '' }
$responses1 = @{}
foreach ($line in $lines1) {
    try {
        $obj = $line | ConvertFrom-Json
        if ($obj.id) { $responses1[[int]$obj.id] = $obj }
    } catch {}
}

Write-Host "Got $($responses1.Count) responses" -ForegroundColor Yellow
$projectId = $null
if ($responses1[2]) {
    $openResult = $responses1[2].result.content[0].text | ConvertFrom-Json
    Write-Host "Project: $($openResult.name)"
    Write-Host "Project ID: $($openResult.project_id)"
    Write-Host "Groups: $($openResult.tree_group_count), Nodes: $($openResult.node_count)"
    $projectId = $openResult.project_id
}

if ($responses1[3]) {
    $tools = $responses1[3].result.tools
    Write-Host "Total registered tools: $($tools.Count)" -ForegroundColor Yellow

    $newTools = @(
        'split_cells', 'move_table_row', 'move_table_column',
        'set_row_height', 'set_column_width', 'set_row_visible', 'set_column_visible',
        'set_row_role', 'rename_column', 'set_column_formula', 'get_column_formula',
        'lock_table', 'set_frozen_columns', 'sort_table_by_column',
        'set_control_formula', 'get_control_formula', 'get_table_info',
        'move_node_up', 'move_node_down', 'set_node_visible', 'set_node_number',
        'set_node_permissions', 'get_node_info', 'list_nodes_by_type', 'get_node_path_by_id',
        'set_cell_data_format', 'set_cell_zero_format', 'set_cell_comment', 'get_cell_comment',
        'get_cell_style', 'copy_cell_style', 'set_column_style', 'batch_set_cell_style',
        'set_paragraph_comment', 'insert_paragraph_at', 'delete_paragraph',
        'get_paragraph_info', 'get_document_info',
        'delete_project', 'get_project_properties', 'set_project_properties',
        'backup_project', 'get_project_stats'
    )
    $toolNames = $tools | ForEach-Object { $_.name }
    $missing = @($newTools | Where-Object { $toolNames -notcontains $_ })
    if ($missing.Count -eq 0) {
        Write-Host "[OK] All $($newTools.Count) new tools registered" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] Missing $($missing.Count) tools:" -ForegroundColor Red
        $missing | ForEach-Object { Write-Host "  - $_" }
    }
}

$tableId = $null
$docId = $null
$dirId = $null
if ($responses1[4]) {
    $treeResult = $responses1[4].result.content[0].text | ConvertFrom-Json
    Write-Host "Tree nodes: $($treeResult.node_count)" -ForegroundColor Yellow

    function Find-Nodes($nodes, [ref]$tableId, [ref]$docId, [ref]$dirId) {
        foreach ($n in $nodes) {
            if ($n.type -eq 'table' -and -not $tableId.Value) { $tableId.Value = [long]$n.id }
            if ($n.type -eq 'document' -and -not $docId.Value) { $docId.Value = [long]$n.id }
            if ($n.type -eq 'directory' -and -not $dirId.Value) { $dirId.Value = [long]$n.id }
            if ($n.children) { Find-Nodes $n.children $tableId $docId $dirId }
        }
    }
    foreach ($group in $treeResult.tree_groups) {
        if ($group.children) {
            Find-Nodes $group.children ([ref]$tableId) ([ref]$docId) ([ref]$dirId)
        }
    }
    Write-Host "Found - table: $tableId, doc: $docId, dir: $dirId"
}

# Phase 2: run all tests in a SINGLE session
# Send create_document_node first, then all other tests
# The document will be in memory for the same session (no need to persist)
Write-Host ""
Write-Host "=== Phase 2: test new tools ===" -ForegroundColor Cyan

if (-not $projectId -or -not $tableId -or -not $dirId) {
    Write-Host "[FAIL] missing node IDs, skip phase 2" -ForegroundColor Red
    exit 1
}

# Start a single long-lived process
$proc2 = [System.Diagnostics.Process]::Start($psi)
$proc2.StandardInput.WriteLine('{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}')
$proc2.StandardInput.WriteLine('{"jsonrpc":"2.0","method":"notifications/initialized"}')
$proc2.StandardInput.WriteLine('{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"open_project","arguments":{"path":"' + $dbPath + '"}}}')
$createDocReq = '{"jsonrpc":"2.0","id":98,"method":"tools/call","params":{"name":"create_document_node","arguments":{"parent_id":' + $dirId + ',"name":"test-audit-doc"}}}'
$proc2.StandardInput.WriteLine($createDocReq)
$proc2.StandardInput.Flush()

# Read responses synchronously until we get id=98 (create_document_node result)
$docId = $null
$readAttempts = 0
$maxAttempts = 50
while ($readAttempts -lt $maxAttempts) {
    $line = $proc2.StandardOutput.ReadLine()
    if ($line -eq $null) { break }
    $readAttempts++
    if ($line.Trim() -eq '') { continue }
    try {
        $obj = $line | ConvertFrom-Json
        if ($obj.id -eq 98) {
            $createResult = $obj.result.content[0].text | ConvertFrom-Json
            if ($createResult.success -and $createResult.node_id) {
                $docId = [long]$createResult.node_id
                Write-Host "Created document node: $docId" -ForegroundColor Green
            } else {
                Write-Host "[FAIL] create_document_node failed: $($createResult.error)" -ForegroundColor Red
            }
            break
        }
    } catch {}
}

if (-not $docId) {
    Write-Host "[WARN] no document id, document tests will be skipped" -ForegroundColor Yellow
}

# Build remaining test requests (exclude phase 1 ids and create_document_node id=98)
$phase2Requests = $requests | Where-Object {
    $req = $_
    if ($req -match '"id":1\b') { return $false }
    if ($req -match '"id":2\b') { return $false }
    if ($req -match '"id":3\b') { return $false }
    if ($req -match '"id":4\b') { return $false }
    if ($req -match 'notifications/initialized') { return $false }
    if ($req -match '"id":98\b') { return $false }
    return $true
}
$phase2Requests = $phase2Requests | ForEach-Object {
    if (-not $docId -and $_ -match '__DOC_ID__') { return $null }
    $r = $_ -replace '__TABLE_ID__', $tableId `
              -replace '__DIR_ID__', $dirId `
              -replace '__PROJECT_ID__', $projectId
    if ($docId) { $r = $r -replace '__DOC_ID__', $docId }
    return $r
} | Where-Object { $_ }

# Send all remaining requests
foreach ($req in $phase2Requests) {
    $proc2.StandardInput.WriteLine($req)
}
$proc2.StandardInput.Flush()
$proc2.StandardInput.Close()

# Read all remaining responses
$out2 = $proc2.StandardOutput.ReadToEnd()
$err2 = $proc2.StandardError.ReadToEnd()
if (-not $proc2.HasExited) { try { $proc2.Kill() } catch {} }

# Parse phase 2 responses (exclude ids 1, 2, 98 which were already handled)
$lines2 = $out2 -split "`n" | Where-Object { $_.Trim() -ne '' }
$responses2 = @{}
foreach ($line in $lines2) {
    try {
        $obj = $line | ConvertFrom-Json
        if ($obj.id) {
            $idVal = [int]$obj.id
            if ($idVal -ne 1 -and $idVal -ne 2 -and $idVal -ne 98) {
                $responses2[$idVal] = $obj
            }
        }
    } catch {}
}

function Show-Result($id, $label, $expectedField = $null) {
    if ($responses2[$id]) {
        $txt = $responses2[$id].result.content[0].text
        try { $r = $txt | ConvertFrom-Json } catch { $r = $null }
        if ($r) {
            $success = $r.success
            if ($success -eq $null) { $success = $true }
            $color = if ($success) { 'Green' } else { 'Red' }
            $mark = if ($success) { '[OK]' } else { '[FAIL]' }
            $extra = ''
            if ($expectedField -and $r.$expectedField) { $extra = " ($expectedField=$($r.$expectedField))" }
            elseif ($r.message) { $extra = " ($($r.message))" }
            elseif ($r.error) { $extra = " ($($r.error))" }
            Write-Host ("  {0} [{1}] {2}{3}" -f $mark, $id, $label, $extra) -ForegroundColor $color
            if (-not $success -and $txt.Length -lt 500) {
                Write-Host "        detail: $txt" -ForegroundColor DarkRed
            }
        } else {
            Write-Host "  [?] [$id] $label (parse error: $txt)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  [?] [$id] $label (no response)" -ForegroundColor Yellow
    }
}

Write-Host "--- AdvancedTableTools ---" -ForegroundColor Cyan
Show-Result 10 "get_table_info"
Show-Result 11 "set_column_width"
Show-Result 12 "set_row_role"
Show-Result 13 "set_frozen_columns"
Show-Result 14 "get_control_formula"

Write-Host "--- CellFormatTools ---" -ForegroundColor Cyan
Show-Result 20 "get_cell_style"
Show-Result 21 "set_cell_comment"
Show-Result 22 "get_cell_comment" "comment"
Show-Result 23 "set_cell_data_format"

Write-Host "--- NodeManagementTools ---" -ForegroundColor Cyan
Show-Result 30 "get_node_info"
Show-Result 31 "set_node_number"
Show-Result 32 "get_node_path_by_id" "path"
Show-Result 33 "list_nodes_by_type" "total"

Write-Host "--- DocumentExtensionTools ---" -ForegroundColor Cyan
if ($docId) {
    Show-Result 40 "get_document_info"
    Show-Result 41 "insert_paragraph_at"
    Show-Result 42 "set_paragraph_comment"
    Show-Result 43 "get_paragraph_info" "text"
} else {
    Write-Host "  [SKIP] no document node available" -ForegroundColor Yellow
}

Write-Host "--- ProjectExtensionTools ---" -ForegroundColor Cyan
Show-Result 50 "get_project_properties"
Show-Result 51 "get_project_stats"
Show-Result 52 "set_project_properties"
Show-Result 53 "backup_project"

if (Test-Path $backupZip) {
    $size = (Get-Item $backupZip).Length
    Write-Host "  [OK] backup file created: $size bytes" -ForegroundColor Green
    Remove-Item $backupZip -Force
}

Write-Host ""
Write-Host "=== Test complete ===" -ForegroundColor Green

if ($err2) {
    Write-Host ""
    Write-Host "=== STDERR (last 10 lines) ===" -ForegroundColor DarkGray
    $err2 -split "`n" | Select-Object -Last 10 | ForEach-Object { Write-Host "  $_" }
}

# Cleanup temp db
if (Test-Path $tmpDb) { Remove-Item $tmpDb -Force }
$tmpDbShm = $tmpDb -replace '\.db$','-shm'
$tmpDbWal = $tmpDb -replace '\.db$','-wal'
if (Test-Path $tmpDbShm) { Remove-Item $tmpDbShm -Force }
if (Test-Path $tmpDbWal) { Remove-Item $tmpDbWal -Force }
