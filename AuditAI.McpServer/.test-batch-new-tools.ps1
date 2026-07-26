$ErrorActionPreference = 'Continue'
$exe = 'e:\lq\AuditAI\AuditAI.McpServer\bin\Debug\net462\AuditAI.McpServer.exe'
$projPath = 'e:\lq\AuditAI\AuditAI.McpServer\test_template.db'
$escapedPath = $projPath -replace '\\','\\'
$outFile = 'e:\lq\AuditAI\AuditAI.McpServer\.test-batch-results.txt'

# Valid table node ID from test_template.db project tree
$validTableId = 5699421601793   # "报表 抬头信息表"
$validTableId2 = 214748364801   # "资产负债表（未审）"

function Invoke-Tool($toolName, $argsObj) {
    $argPart = $argsObj | ConvertTo-Json -Compress -Depth 10
    $requests = @()
    $requests += '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-11-25","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}'
    $requests += '{"jsonrpc":"2.0","method":"notifications/initialized"}'
    $requests += "{`"jsonrpc`":`"2.0`",`"id`":2,`"method`":`"tools/call`",`"params`":{`"name`":`"open_project`",`"arguments`":{`"path`":`"$escapedPath`"}}}"
    $requests += "{`"jsonrpc`":`"2.0`",`"id`":3,`"method`":`"tools/call`",`"params`":{`"name`":`"$toolName`",`"arguments`":$argPart}}"

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

    $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
    $stderrTask = $proc.StandardError.ReadToEndAsync()

    $proc.StandardInput.Write($inputText)
    $proc.StandardInput.Flush()
    $proc.StandardInput.Close()

    $waited = $proc.WaitForExit(30000)
    if (-not $waited) {
        try { $proc.Kill() } catch {}
        return @{ status = "TIMEOUT"; msg = "killed after 30s" }
    }

    try { $stdoutTask.Wait(2000) | Out-Null } catch {}
    try { $stderrTask.Wait(2000) | Out-Null } catch {}
    $out = $stdoutTask.Result
    $err = $stderrTask.Result

    $lines = $out -split "`n"
    $resp = $lines | Where-Object { $_ -match '"id"\s*:\s*3' } | Select-Object -Last 1
    if (-not $resp) {
        return @{ status = "NO_RESP"; msg = "stdout_lines=$($lines.Count)" }
    }

    try {
        $obj = $resp | ConvertFrom-Json
        if ($obj.error) {
            return @{ status = "RPC_ERROR"; msg = $obj.error.message }
        }
        if ($obj.result -and $obj.result.content) {
            $text = ($obj.result.content | Where-Object { $_.type -eq "text" } | Select-Object -First 1).text
            try {
                $inner = $text | ConvertFrom-Json
                if ($inner.success -eq $false) {
                    return @{ status = "FAIL"; msg = $inner.error }
                }
                $snippet = if ($text.Length -gt 120) { $text.Substring(0, 120) + "..." } else { $text }
                return @{ status = "OK"; msg = $snippet }
            } catch {
                $snippet = if ($text.Length -gt 120) { $text.Substring(0, 120) + "..." } else { $text }
                return @{ status = "OK_NONJSON"; msg = $snippet }
            }
        }
        return @{ status = "UNKNOWN"; msg = $resp.Substring(0, [Math]::Min(120, $resp.Length)) }
    } catch {
        return @{ status = "PARSE_ERR"; msg = $_.ToString() }
    }
}

# Test list - use valid IDs from the project tree
$tests = @(
    # TicketTools - use a valid table
    @{ cat = "TicketTools"; name = "get_ticket_info"; args = @{ table_node_id = $validTableId } }
    @{ cat = "TicketTools"; name = "get_ticket_records"; args = @{ table_node_id = $validTableId; offset = 0; limit = 5 } }
    @{ cat = "TicketTools"; name = "set_ticket_kind"; args = @{ table_node_id = $validTableId; kind = "FixedOneRow" } }
    @{ cat = "TicketTools"; name = "set_ticket_level"; args = @{ table_node_id = $validTableId; level = "Receipt" } }
    @{ cat = "TicketTools"; name = "set_ticket_data_row_height"; args = @{ table_node_id = $validTableId; height = 30 } }
    @{ cat = "TicketTools"; name = "set_ticket_frozen"; args = @{ table_node_id = $validTableId; rows_frozen = 1; cols_frozen = 1 } }
    # CollectConsolidateTools
    @{ cat = "CollectConsolidateTools"; name = "get_collect_config"; args = @{ table_node_id = $validTableId } }
    @{ cat = "CollectConsolidateTools"; name = "collect_cells"; args = @{ table_node_id = $validTableId; row_indices = @(0,1); col_indices = @(0,1) } }
    @{ cat = "CollectConsolidateTools"; name = "collect_by_column"; args = @{ table_node_id = $validTableId; col_indices = @(0) } }
    @{ cat = "CollectConsolidateTools"; name = "get_consolidate_config"; args = @{} }
    @{ cat = "CollectConsolidateTools"; name = "execute_consolidate"; args = @{} }
    @{ cat = "CollectConsolidateTools"; name = "one_click_collect"; args = @{} }
    # SnapshotTools
    @{ cat = "SnapshotTools"; name = "list_snapshots"; args = @{ tree_node_id = $validTableId } }
    @{ cat = "SnapshotTools"; name = "create_snapshot"; args = @{ tree_node_id = $validTableId } }
    @{ cat = "SnapshotTools"; name = "restore_snapshot"; args = @{ snapshot_id = 1; tree_node_id = $validTableId } }
    @{ cat = "SnapshotTools"; name = "delete_snapshot"; args = @{ snapshot_id = 999999 } }
    @{ cat = "SnapshotTools"; name = "list_recycled_nodes"; args = @{} }
    @{ cat = "SnapshotTools"; name = "restore_recycled_node"; args = @{ snapshot_id = 999999; parent_node_id = $validTableId } }
    @{ cat = "SnapshotTools"; name = "purge_recycled_node"; args = @{ snapshot_id = 999999 } }
    # ValidationPointTools
    @{ cat = "ValidationPointTools"; name = "list_validation_points"; args = @{} }
    @{ cat = "ValidationPointTools"; name = "add_validation_point"; args = @{ table_node_id = $validTableId; left_expr = "[0,0]"; op = "=="; right_expr = "0"; note = "test" } }
    @{ cat = "ValidationPointTools"; name = "update_validation_point"; args = @{ validation_id = 999999; note = "updated" } }
    @{ cat = "ValidationPointTools"; name = "remove_validation_point"; args = @{ validation_id = 999999 } }
    @{ cat = "ValidationPointTools"; name = "run_validation"; args = @{ validation_id = 999999 } }
    # DocumentFormatTools - no document in test_template.db, will test with id=1 (will get error)
    @{ cat = "DocumentFormatTools"; name = "get_paragraph_format"; args = @{ document_node_id = 1; paragraph_index = 0 } }
    @{ cat = "DocumentFormatTools"; name = "set_paragraph_alignment"; args = @{ document_node_id = 1; paragraph_index = 0; alignment = "left" } }
    @{ cat = "DocumentFormatTools"; name = "set_paragraph_spacing"; args = @{ document_node_id = 1; paragraph_index = 0; line_spacing = 1.5 } }
    @{ cat = "DocumentFormatTools"; name = "set_paragraph_indent"; args = @{ document_node_id = 1; paragraph_index = 0; indent_first_line = 2.0 } }
    # BatchOperationTools
    @{ cat = "BatchOperationTools"; name = "batch_duplicate_column"; args = @{ table_node_id = $validTableId; col_indices = @(0); name_suffix = "_copy" } }
    @{ cat = "BatchOperationTools"; name = "batch_remove_column"; args = @{ table_node_id = $validTableId; col_indices = @(999) } }
    @{ cat = "BatchOperationTools"; name = "batch_rename_column"; args = @{ table_node_id = $validTableId; renames = @(@{ col_index = 0; new_name = "Test" }) } }
    @{ cat = "BatchOperationTools"; name = "find_in_table"; args = @{ table_node_id = $validTableId; keyword = "test" } }
    @{ cat = "BatchOperationTools"; name = "find_in_all_tables"; args = @{ keyword = "test" } }
    @{ cat = "BatchOperationTools"; name = "find_in_document"; args = @{ document_node_id = 1; keyword = "test" } }
    # PageSetupTools
    @{ cat = "PageSetupTools"; name = "get_page_setup"; args = @{ table_node_id = $validTableId } }
    @{ cat = "PageSetupTools"; name = "set_paper_size"; args = @{ table_node_id = $validTableId; paper_kind = "A4" } }
    @{ cat = "PageSetupTools"; name = "set_page_orientation"; args = @{ table_node_id = $validTableId; direction = "Portrait" } }
    @{ cat = "PageSetupTools"; name = "set_margins"; args = @{ table_node_id = $validTableId; left = 2.0; right = 2.0 } }
    @{ cat = "PageSetupTools"; name = "set_header_footer"; args = @{ table_node_id = $validTableId; position = "left"; content = "Test"; is_header = $true } }
    @{ cat = "PageSetupTools"; name = "set_print_scale"; args = @{ table_node_id = $validTableId; horizontal_zoom = 1.0 } }
    @{ cat = "PageSetupTools"; name = "set_print_options"; args = @{ table_node_id = $validTableId; print_copies = 1 } }
)

$results = @()
$pass = 0; $fail = 0; $timeout = 0; $norpc = 0
"=== Testing $($tests.Count) new MCP tools ==="
"" | Out-File -FilePath $outFile -Encoding UTF8
for ($i = 0; $i -lt $tests.Count; $i++) {
    $t = $tests[$i]
    Write-Host -NoNewline "[$($i+1)/$($tests.Count)] $($t.name) ... "
    $r = Invoke-Tool $t.name $t.args
    $results += @{ cat = $t.cat; name = $t.name; status = $r.status; msg = $r.msg }
    $line = "[$($r.status)] $($t.name): $($r.msg)"
    $line | Out-File -FilePath $outFile -Encoding UTF8 -Append
    Write-Host $line
    switch ($r.status) {
        "OK" { $pass++ }
        "OK_NONJSON" { $pass++ }
        "TIMEOUT" { $timeout++ }
        "NO_RESP" { $norpc++ }
        default { $fail++ }
    }
}

""
"=== Summary ==="
"Total: $($tests.Count)"
"Pass (OK/OK_NONJSON): $pass"
"Fail (FAIL/RPC_ERROR/etc): $fail"
"Timeout: $timeout"
"NoResponse: $norpc"
"Results saved to: $outFile"
