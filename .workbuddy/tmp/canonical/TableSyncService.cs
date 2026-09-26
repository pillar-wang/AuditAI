using Auditai.DTO;
using AuditApiServer.Data;
using AuditApiServer.Hubs;
using AuditApiServer.Models;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace AuditApiServer.Services;

/// <summary>
    /// 表格同步业务服务。阶段 6 Task 6.5 实现。
    /// 解析 PushTable Protobuf 字节流，按 Action（New=1/Modify=2/Delete=3）拆解各 BLOB 字段；
    /// 递增 Version；写入 VersionHistory 快照；
    /// PullTable 差异计算：基于客户端 Version 比对，返回增量（MVP：返回完整表）。
    /// </summary>
    /// <remarks>
    /// 并发与版本号安全修复（Critical/High）：
    /// 1. 乐观锁：SaveWithOptimisticLockAsync 在 UPDATE 时附加 AND Version = @expectedVersion，
    ///    affected rows = 0 则重试（最多 MaxVersionRetries 次），防止版本号覆盖。
    ///    移除 SemaphoreSlim 串行化：异步环境下 SemaphoreSlim.WaitAsync/Release 可能导致死锁，
    ///    完全依赖乐观锁即可保证版本号安全。
    /// 2. Protobuf 解析 try/catch：解析失败抛 ArgumentException（客户端错误），避免 500。
    /// 3. Revert 操作写 ChangeType='Revert' 审计记录。
    /// 4. 子元素 Action 字段校验：必须在 {1=New, 2=Modify, 3=Delete} 范围内。
    /// </remarks>
public class TableSyncService
{
    private readonly TableRepository _tableRepo;
    private readonly VersionHistoryService _historySvc;
    private readonly ProjectDbManager _projectDbManager;
    private readonly SqliteStorage _db;
    private readonly ILogger<TableSyncService> _logger;
    private readonly IHubContext<ChatHub> _hubContext;

    private const int MaxVersionRetries = 5;

    public TableSyncService(
        TableRepository tableRepo,
        VersionHistoryService historySvc,
        ProjectDbManager projectDbManager,
        SqliteStorage db,
        ILogger<TableSyncService> logger,
        IHubContext<ChatHub> hubContext)
    {
        _tableRepo = tableRepo;
        _historySvc = historySvc;
        _projectDbManager = projectDbManager;
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 快速推送：客户端直接 POST PushTable Protobuf 字节流（cells.Count ≤ 1000）。
    /// 端点层（Program.cs）已完成 ParseFrom，此处直接处理业务逻辑，避免重复解析。
    /// </summary>
    public async Task<object> PushTableQuickAsync(PushTable pushTable, long userId)
    {
        ValidatePushTableActions(pushTable);
        return await SavePushTableAsync(pushTable, userId);
    }

    /// <summary>
    /// 异步推送：客户端先上传文件，再调用 GET /api/Project/PushTable?taskId=&projectId=&tableId=&version=。
    /// 此方法从 TaskService 缓存文件读取 PushTable Protobuf 字节流，调用 SavePushTableAsync 写入项目 .db。
    /// 返回 JObject { Result }。
    /// </summary>
    public async Task<object> PushTableAsync(long taskId, Guid projectId, Guid tableId, int version, long userId, Stream cacheStream)
    {
        using var ms = new MemoryStream();
        await cacheStream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        PushTable pushTable;
        try
        {
            pushTable = PushTable.Parser.ParseFrom(bytes);
        }
        catch (InvalidProtocolBufferException ex)
        {
            // 诊断日志：记录前 32 字节 hex 便于排查上传文件是否损坏
            byte[] head = bytes.Length >= 32 ? bytes.AsSpan(0, 32).ToArray() : bytes;
            _logger.LogWarning(ex,
                "PushTableAsync Protobuf 解析失败: TaskId={TaskId}, BytesLength={Len}, HeadHex={Head}",
                taskId, bytes.Length, Convert.ToHexString(head));
            throw new ArgumentException("无效的 Protobuf 数据: " + ex.Message, ex);
        }

        ValidatePushTableActions(pushTable);
        var result = await SavePushTableAsync(pushTable, userId);
        _logger.LogInformation("PushTable 异步任务完成: TaskId={TaskId} TableId={TableId}", taskId, tableId);
        return result;
    }

    /// <summary>
    /// 拉取表格：客户端 POST { projectId, tableId, version } JSON。
    /// 返回 PullTable Protobuf 字节流。从项目 .db 的 Table/Column/Row/Cell/Merge/CellStyle 表读取。
    /// Result 取值约定（与客户端 Syncer.Pull 对齐）：
    ///   - "NotExist"   ：表格不存在（TreeNode/Table 行缺失）
    ///   - "Latest"     ：客户端 Version 与服务端 TreeNode.Version 一致，无需更新，返回空 PullTable
    ///   - "NeedUpdate" ：版本不一致，返回包含完整数据的 PullTable，客户端 Merge 后写入本地
    /// </summary>
    public async Task<(byte[] bytes, bool isEmpty)> PullTableAsync(Guid projectId, Guid tableId, int clientVersion)
    {
        var longTableId = BitConverter.ToInt64(tableId.ToByteArray(), 0);
        var teamId = await GetTeamIdAsync(projectId);

        using var conn = _projectDbManager.OpenProjectDb(teamId, projectId);

        // 检查 Table 是否存在
        using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT Id FROM `Table` WHERE Id=@id";
            checkCmd.Parameters.AddWithValue("@id", longTableId);
            var exists = await checkCmd.ExecuteScalarAsync();
            if (exists == null || exists == DBNull.Value)
            {
                // 与客户端 Syncer.Pull 对齐：表格不存在返回 "NotExist"
                var empty = new PullTable { Result = "NotExist", Version = 0 };
                return (empty.ToByteArray(), true);
            }
        }

        // 读取 TreeNode.Version（表格版本存储在 TreeNode 表）
        int version = 0;
        using (var verCmd = conn.CreateCommand())
        {
            verCmd.CommandText = "SELECT Version FROM `TreeNode` WHERE Id=@id";
            verCmd.Parameters.AddWithValue("@id", longTableId);
            var verResult = await verCmd.ExecuteScalarAsync();
            if (verResult != null && verResult != DBNull.Value)
                version = Convert.ToInt32(verResult);
        }

        if (version == clientVersion)
        {
            // 版本一致：客户端 Syncer.Pull 据此返回 PullResult.AlreadyLatest，无需 Merge
            var noChange = new PullTable { Result = "Latest", Version = version };
            return (noChange.ToByteArray(), true);
        }

        // 版本不一致：客户端 Syncer.Pull 据此调用 Merge 并更新 TreeNode.Version
        var pullTable = new PullTable { Result = "NeedUpdate", Version = version };

        // 表级字段
        await FillTableLevelFieldsAsync(conn, pullTable, longTableId);

        // Columns
        using (var colCmd = conn.CreateCommand())
        {
            colCmd.CommandText = "SELECT `Id`,`Caption`,`Index`,`Width`,`Visible`,`StyleId`,`CaptionStyle`,`ConsolidateAttribs`,`Formula`,`SubtotalAttribs`,`Permissions`,`CaptionFormula`,`CrossAttributes` FROM `Column` WHERE TableId=@tid";
            colCmd.Parameters.AddWithValue("@tid", longTableId);
            using var reader = await colCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                pullTable.NewColumns.Add(ReadPullColumn(reader));
        }

        // Rows
        using (var rowCmd = conn.CreateCommand())
        {
            rowCmd.CommandText = "SELECT `Id`,`Index`,`Height`,`Visible`,`Locked`,`Role`,`Permissions`,`Creator` FROM `Row` WHERE TableId=@tid";
            rowCmd.Parameters.AddWithValue("@tid", longTableId);
            using var reader = await rowCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                pullTable.NewRows.Add(ReadPullRow(reader));
        }

        // Cells（join Row 以按 TableId 过滤）
        using (var cellCmd = conn.CreateCommand())
        {
            cellCmd.CommandText = "SELECT c.`Id`,c.`ColumnId`,c.`RowId`,c.`Value`,c.`Formula`,c.`StyleId`,c.`CollectSource`,c.`HeaderFormula` FROM `Cell` c INNER JOIN `Row` r ON c.RowId=r.Id WHERE r.TableId=@tid";
            cellCmd.Parameters.AddWithValue("@tid", longTableId);
            using var reader = await cellCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                pullTable.NewCells.Add(ReadPullCell(reader));
        }

        // Merges
        using (var mergeCmd = conn.CreateCommand())
        {
            mergeCmd.CommandText = "SELECT `Id`,`TopLeft`,`BottomRight` FROM `Merge` WHERE TableId=@tid";
            mergeCmd.Parameters.AddWithValue("@tid", longTableId);
            using var reader = await mergeCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                pullTable.NewMerges.Add(ReadPullMerge(reader));
        }

        // CellStyles
        using (var styleCmd = conn.CreateCommand())
        {
            styleCmd.CommandText = "SELECT `Id`,`FontFamily`,`FontSize`,`ForeColor`,`BackColor`,`Align`,`Margin`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`DefaultValue`,`Comment` FROM `CellStyle` WHERE TableId=@tid";
            styleCmd.Parameters.AddWithValue("@tid", longTableId);
            using var reader = await styleCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                pullTable.CellStyles.Add(ReadPullCellStyle(reader));
        }

        return (pullTable.ToByteArray(), false);
    }

    /// <summary>
    /// 回滚表格到指定版本：从 VersionHistory 读取快照，解析为 PushTable，
    /// 复用 SavePushTableAsync 写回项目 .db 各表，递增 TreeNode.Version。
    /// 返回 { taskId: <long> }，客户端据此等待任务完成。
    /// V2-C-01 修复：改为操作项目 .db 而非主库 TableSchemas。
    /// </summary>
    public async Task<object> RevertTableAsync(Guid projectId, Guid tableId, int targetVersion, long userId, TaskService taskSvc)
    {
        var snapshot = await _historySvc.GetSnapshotAsync(projectId, tableId, "Table", targetVersion);
        if (snapshot == null)
            return new { error = $"版本 {targetVersion} 的快照不存在" };

        if (snapshot.Snapshot == null || snapshot.Snapshot.Length == 0)
            return new { error = $"版本 {targetVersion} 的快照数据为空" };

        var taskId = await taskSvc.GenerateTaskIdAsync(userId, "RevertTable");
        taskSvc.StartBackgroundTask(taskId, async (progress, ct) =>
        {
            progress.Report(0.1, "正在加载快照");

            // 快照存储的是 PushTable 序列化字节，解析为 PushTable
            PushTable pushTable;
            try
            {
                pushTable = PushTable.Parser.ParseFrom(snapshot.Snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RevertTable 快照解析失败: TableId={TableId} Version={Version}", tableId, targetVersion);
                throw new InvalidOperationException($"快照解析失败: {ex.Message}", ex);
            }

            // 旧版增量快照（含 Modify/Delete 动作）重放到当前状态会产生混合损坏数据，拒绝回滚
            if (IsIncrementalTableSnapshot(pushTable))
            {
                _logger.LogWarning(
                    "RevertTable 拒绝：版本 {Version} 的快照为旧版增量格式，无法回滚。TableId={TableId}",
                    targetVersion, tableId);
                throw new InvalidOperationException($"版本 {targetVersion} 的快照为旧版增量格式，无法安全回滚（该版本产生于快照机制升级前）");
            }

            progress.Report(0.5, "正在覆盖数据");

            // Version=0 跳过乐观锁版本校验（快照中的 Version 是历史值，必然落后于当前服务端版本）
            pushTable.Version = 0;

            // 整体覆盖模式写回项目 .db 各表（先清空现有子数据再按全量快照重建），递增 TreeNode.Version
            // SavePushTableAsync 内部已处理乐观锁重试和版本快照写入（changeType="Revert"）
            await SavePushTableAsync(pushTable, userId, "Revert", replaceExisting: true);

            _logger.LogInformation(
                "RevertTable 完成: TableId={TableId} TargetVersion={Target} UserId={UserId}",
                tableId, targetVersion, userId);
            progress.Report(1.0, "完成");
        });

        return new { taskId };
    }

    /// <summary>
    /// 获取回滚前后差异（PullTable 字节流）。
    /// 返回 { taskId, url } 供客户端等待任务并下载结果。
    /// MVP：同步计算并直接返回 Protobuf 字节流（不需要任务等待）。
    /// V2-C-03 修复：快照存储的是 PushTable 字节，用 PushTable 解析后转换为 PullTable 返回。
    /// </summary>
    public async Task<(byte[] bytes, long taskId, string url)> GetTableRevertDiffAsync(Guid projectId, Guid tableId, int targetVersion, long userId, TaskService taskSvc)
    {
        var taskId = await taskSvc.GenerateTaskIdAsync(userId, "GetTableRevertDiff");
        var url = $"/api/ServerTask/DownloadTaskResult?taskId={taskId}";

        // MVP：同步计算差异并直接返回（实际生产环境应写入文件供客户端下载）
        var snapshot = await _historySvc.GetSnapshotAsync(projectId, tableId, "Table", targetVersion);
        // 无快照时返回 "Latest"（无差异可预览，客户端保持当前状态）；有快照时由 ConvertPushTableToPullTable 返回 "NeedUpdate"
        var pullTable = new PullTable { Result = "Latest", Version = targetVersion };
        if (snapshot?.Snapshot != null)
        {
            try
            {
                // 快照存储的是 PushTable 序列化字节，需用 PushTable 解析后转换为 PullTable
                var pushTable = PushTable.Parser.ParseFrom(snapshot.Snapshot);
                if (IsIncrementalTableSnapshot(pushTable))
                {
                    // 旧版增量快照无法转换为全量差异预览（会误导客户端按全量 Merge 损坏本地表），
                    // 返回 "Latest" 让客户端保持当前状态
                    _logger.LogWarning(
                        "GetTableRevertDiff：版本 {Version} 的快照为旧版增量格式，无法预览差异。TableId={TableId}",
                        targetVersion, tableId);
                }
                else
                {
                    pullTable = ConvertPushTableToPullTable(pushTable, targetVersion);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "快照解析失败: TableId={TableId} Version={Version}", tableId, targetVersion);
            }
        }

        // 标记任务为完成（MVP：立即完成）
        taskSvc.StartBackgroundTask(taskId, (progress, ct) =>
        {
            progress.Report(1.0, "完成");
            return Task.CompletedTask;
        });

        return (pullTable.ToByteArray(), taskId, url);
    }

    /// <summary>
    /// 获取表格版本时间线（按时间倒序）。
    /// 返回 JArray，包含 { id, version, changeType, createdAt, createdBy }。
    /// </summary>
    public async Task<List<object>> GetTableTimelineAsync(Guid projectId, Guid tableId)
    {
        var timeline = await _historySvc.ListTimelineAsync(projectId, tableId, "Table");
        return timeline.Select(t => (object)new
        {
            id = t.Id,
            version = t.Version,
            changeType = t.ChangeType,
            createdAt = t.CreatedAt,
            createdBy = t.CreatedBy
        }).ToList();
    }

    /// <summary>
    /// 查询表格版本列表（轻量字段，不含 Snapshot）。
    /// 返回 JArray，包含 { version, changeType, createdAt }。
    /// </summary>
    public async Task<List<object>> QueryTableVersionsAsync(Guid projectId, Guid tableId)
    {
        var versions = await _historySvc.ListVersionsAsync(projectId, tableId, "Table");
        return versions.Select(v => (object)new
        {
            version = v.Version,
            changeType = v.ChangeType,
            createdAt = v.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// 获取表格列信息。返回 UTF-8 编码的 JArray JSON 字节流。
    /// V2-H-04 修复：原实现调用 _tableRepo.GetColumnsAsync 读主库 TableSchemas.ColumnsData，
    /// 但 PushTable 重构后不再写入主库，数据存储在项目 .db 的 Column 表中。
    /// 现改为打开项目 .db 直接读取 Column 表并序列化为 JSON 返回。
    /// </summary>
    public async Task<byte[]?> GetTableColumnsAsync(Guid projectId, Guid tableId)
    {
        var longTableId = BitConverter.ToInt64(tableId.ToByteArray(), 0);
        var teamId = await GetTeamIdAsync(projectId);

        using var conn = _projectDbManager.OpenProjectDb(teamId, projectId);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT `Id`,`Caption`,`Index`,`Width`,`Visible`,`StyleId`,`CaptionStyle`,`ConsolidateAttribs`,`Formula`,`SubtotalAttribs`,`Permissions`,`CaptionFormula` FROM `Column` WHERE TableId=@tid ORDER BY `Index`";
        cmd.Parameters.AddWithValue("@tid", longTableId);
        using var reader = await cmd.ExecuteReaderAsync();

        var arr = new JArray();
        while (await reader.ReadAsync())
        {
            arr.Add(new JObject
            {
                ["id"] = reader.GetInt64(0),
                ["caption"] = reader.IsDBNull(1) ? null : reader.GetString(1),
                ["index"] = reader.GetInt32(2),
                ["width"] = reader.GetInt32(3),
                ["visible"] = reader.GetInt32(4) != 0,
                ["styleId"] = reader.IsDBNull(5) ? (long?)null : reader.GetInt64(5),
                ["captionStyle"] = reader.IsDBNull(6) ? null : reader.GetString(6),
                ["consolidateAttribs"] = reader.IsDBNull(7) ? null : reader.GetString(7),
                ["formula"] = reader.IsDBNull(8) ? null : reader.GetString(8),
                ["subtotalAttribs"] = reader.GetInt32(9),
                ["permissions"] = reader.IsDBNull(10) ? null : reader.GetString(10),
                ["captionFormula"] = reader.IsDBNull(11) ? null : reader.GetString(11),
            });
        }

        return System.Text.Encoding.UTF8.GetBytes(arr.ToString());
    }

    /// <summary>
    /// 内部：保存 PushTable 到项目 .db（Table/Column/Row/Cell/Merge/CellStyle）+ 写入版本快照。
    /// 使用乐观锁重试保证 TreeNode.Version 原子递增。所有写操作在单个事务内执行。
    /// changeType: "Push"（默认）或 "Revert"（回滚操作复用此方法时传入）。
    /// </summary>
    /// <param name="replaceExisting">
    /// 整体覆盖模式（仅 Revert 使用）：先清空该表现有全部子数据再应用快照内容，
    /// 保证回滚结果是完整的快照状态而非"旧增量重放到最新状态"的混合数据。
    /// </param>
    private async Task<object> SavePushTableAsync(PushTable pushTable, long userId, string changeType = "Push", bool replaceExisting = false)
    {
        var projectId = new Guid(pushTable.ProjectId.ToByteArray());
        var tableId = pushTable.Id; // long，项目 .db 中 Table.Id / TreeNode.Id 均为 long
        var teamId = await GetTeamIdAsync(projectId);

        using var conn = _projectDbManager.OpenProjectDb(teamId, projectId);

        for (var attempt = 0; attempt < MaxVersionRetries; attempt++)
        {
            // 读取当前 TreeNode 版本
            int expectedVersion;
            bool hasTreeNode;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "SELECT Version FROM `TreeNode` WHERE Id=@id";
                readCmd.Parameters.AddWithValue("@id", tableId);
                var result = await readCmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                {
                    hasTreeNode = false;
                    expectedVersion = 0;
                }
                else
                {
                    hasTreeNode = true;
                    expectedVersion = Convert.ToInt32(result);
                }
            }

            // 乐观锁检查：客户端版本落后于服务端说明他人已推送过，拒绝本次 Push 并返回最新版本，
            // 客户端 PullAndRetryPush 会 Pull 合并后重试。clientVersion=0（新表首次推送）跳过校验。
            // V2 修复：原实现完全忽略客户端版本，多人并发编辑时静默后写覆盖（丢失先写者修改）。
            if (!replaceExisting && pushTable.Version > 0 && pushTable.Version < expectedVersion)
            {
                _logger.LogWarning(
                    "PushTable 拒绝：版本落后（客户端={ClientVersion} 服务端={ServerVersion}）TableId={TableId} UserId={UserId}",
                    pushTable.Version, expectedVersion, tableId, userId);
                return new { Result = "OutOfDate", Version = expectedVersion };
            }

            using var transaction = conn.BeginTransaction();
            try
            {
                // 节点级强锁：事务内校验当前 Locker 是否被其他用户持有且未过期（30 分钟）。
                // 原检查在事务外存在 TOCTOU 窗口，现移入事务内复检，缩小竞态窗口。
                // 同 userId 多端允许（兼容用户多设备登录）；锁过期则视为已释放。
                long currentLocker = 0;
                string? lockerAcquiredAt = null;
                using (var lockReadCmd = conn.CreateCommand())
                {
                    lockReadCmd.Transaction = transaction;
                    lockReadCmd.CommandText = "SELECT `Locker`, `LockerAcquiredAt` FROM `Table` WHERE Id=@id";
                    lockReadCmd.Parameters.AddWithValue("@id", tableId);
                    using var lockReader = await lockReadCmd.ExecuteReaderAsync();
                    if (await lockReader.ReadAsync())
                    {
                        currentLocker = lockReader.IsDBNull(0) ? 0 : lockReader.GetInt64(0);
                        lockerAcquiredAt = lockReader.IsDBNull(1) ? null : lockReader.GetString(1);
                    }
                }

                if (currentLocker != 0 && currentLocker != userId && !IsLockExpired(lockerAcquiredAt))
                {
                    transaction.Rollback();
                    _logger.LogWarning("PushTable 拒绝：TableId={TableId} 被用户 {Locker} 锁定，当前用户 {UserId}",
                        tableId, currentLocker, userId);
                    return new { Result = "Locked", Locker = currentLocker };
                }

                // 回滚（整体覆盖模式）：保留当前锁归属（快照中的 Locker 是历史值），清空子数据后整体重建
                if (replaceExisting)
                {
                    pushTable.Locker = currentLocker;
                    await ClearTableChildrenAsync(conn, transaction, tableId);
                }

                // 1. 写入/更新 Table 表级字段
                await UpsertTableAsync(conn, transaction, pushTable);

                // 2. Columns：Action=1/2 upsert，Action=3 delete（级联清理关联 Cell/CellProp/Merge，防孤儿数据）
                foreach (var col in pushTable.Columns)
                {
                    if (col.Action == 3)
                        await DeleteColumnCascadeAsync(conn, transaction, tableId, col.Id);
                    else
                        await UpsertColumnAsync(conn, transaction, tableId, col);
                }

                // 3. Rows
                foreach (var row in pushTable.Rows)
                {
                    if (row.Action == 3)
                        await DeleteRowCascadeAsync(conn, transaction, tableId, row.Id);
                    else
                        await UpsertRowAsync(conn, transaction, tableId, row);
                }

                // 4. Cells
                foreach (var cell in pushTable.Cells)
                {
                    if (cell.Action == 3)
                    {
                        await DeleteEntityAsync(conn, transaction, "`Cell`", cell.Id);
                        await DeleteCellPropAsync(conn, transaction, tableId, cell.Id);
                    }
                    else
                        await UpsertCellAsync(conn, transaction, cell);
                }

                // 5. Merges
                foreach (var merge in pushTable.Merges)
                {
                    if (merge.Action == 3)
                        await DeleteEntityAsync(conn, transaction, "`Merge`", merge.Id);
                    else
                        await UpsertMergeAsync(conn, transaction, tableId, merge);
                }

                // 5b. 清理孤儿 Merge：删除行列后，跨行列的合并（TopLeft/BottomRight 不在其行列上）
                //     的引用单元格已被级联删除，Merge 自身也应移除，否则 Pull 端需依赖客户端兜底清理
                await DeleteOrphanMergesAsync(conn, transaction, tableId);

                // 6. CellStyles（PushCellStyle 无 Action 字段，统一 upsert）
                foreach (var style in pushTable.CellStyles)
                    await UpsertCellStyleAsync(conn, transaction, tableId, style);

                // 7. CellAttachments：Action=3 删除 / else upsert 写入 CellProp.Attachments 字段
                //    V2-H-05 修复：原实现校验了 CellAttachments 的 Action 字段但未实际写入。
                foreach (var att in pushTable.CellAttachments)
                {
                    if (att.Action == 3)
                        await DeleteCellAttachmentAsync(conn, transaction, tableId, att.CellId);
                    else
                        await UpsertCellAttachmentAsync(conn, transaction, tableId, att);
                }

                // 8. 递增 TreeNode.Version（乐观锁）
                // V2-M-03 修复：移除 Math.Max 中的客户端版本依赖，服务端版本号严格由数据库自增。
                int newVersion = expectedVersion + 1;
                int affected = 1;
                if (hasTreeNode)
                {
                    using var verCmd = conn.CreateCommand();
                    verCmd.Transaction = transaction;
                    verCmd.CommandText = "UPDATE `TreeNode` SET Version=@newVersion WHERE Id=@id AND Version=@expected";
                    verCmd.Parameters.AddWithValue("@newVersion", newVersion);
                    verCmd.Parameters.AddWithValue("@id", tableId);
                    verCmd.Parameters.AddWithValue("@expected", expectedVersion);
                    affected = await verCmd.ExecuteNonQueryAsync();
                }
                else
                {
                    // V2-M-05 修复：hasTreeNode=false 时 INSERT 新 TreeNode 记录，
                    // 避免 PullTableAsync 读取不到 TreeNode.Version（默认 0）导致客户端拉不到新表数据。
                    // Type=1 对应 TreeTableNode（与客户端 GetCode() 约定一致）。
                    // GroupId 取首个 TreeGroup Id，无分组时回退到 1（与 CreateDefaultTreeGroup 默认 Id 一致）。
                    long groupId = 1L;
                    using (var groupCmd = conn.CreateCommand())
                    {
                        groupCmd.Transaction = transaction;
                        groupCmd.CommandText = "SELECT Id FROM `TreeGroup` ORDER BY Id LIMIT 1";
                        var groupResult = await groupCmd.ExecuteScalarAsync();
                        if (groupResult != null && groupResult != DBNull.Value)
                            groupId = Convert.ToInt64(groupResult);
                    }

                    using var insertNodeCmd = conn.CreateCommand();
                    insertNodeCmd.Transaction = transaction;
                    // 并发修复：多个客户端几乎同时首推同一张新表时，两边都会读到 hasTreeNode=false。
                    // 直接 INSERT 会让后到者命中 TreeNode.Id 主键约束抛 SqliteException（不是 affected=0），
                    // 被外层 catch 转成 500 失败。改为"不存在才插入"，冲突时 affected=0 走统一的
                    // Rollback + 重试分支，重试时读到 hasTreeNode=true 自然转入 UPDATE 路径。
                    insertNodeCmd.CommandText = @"INSERT INTO `TreeNode`(
`Id`,`GroupId`,`ParentId`,`Name`,`Status`,`Dirty`,`Index`,`ServerIndex`,`Type`,`Level`,`Version`)
SELECT @Id,@GroupId,@ParentId,@Name,@Status,@Dirty,@Index,@ServerIndex,@Type,@Level,@Version
WHERE NOT EXISTS (SELECT 1 FROM `TreeNode` WHERE Id=@Id)";
                    insertNodeCmd.Parameters.AddWithValue("@Id", tableId);
                    insertNodeCmd.Parameters.AddWithValue("@GroupId", groupId);
                    insertNodeCmd.Parameters.AddWithValue("@ParentId", DBNull.Value);
                    insertNodeCmd.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(pushTable.Title) ? "Table" : pushTable.Title);
                    insertNodeCmd.Parameters.AddWithValue("@Status", 0);
                    insertNodeCmd.Parameters.AddWithValue("@Dirty", 0);
                    insertNodeCmd.Parameters.AddWithValue("@Index", 0);
                    insertNodeCmd.Parameters.AddWithValue("@ServerIndex", 0);
                    insertNodeCmd.Parameters.AddWithValue("@Type", 1);
                    insertNodeCmd.Parameters.AddWithValue("@Level", 0);
                    insertNodeCmd.Parameters.AddWithValue("@Version", newVersion);
                    affected = await insertNodeCmd.ExecuteNonQueryAsync();
                }

                if (affected == 0)
                {
                    transaction.Rollback();
                    _logger.LogWarning("PushTableQuick 乐观锁冲突，重试: TableId={TableId} Attempt={Attempt}",
                        tableId, attempt + 1);
                    continue;
                }

                transaction.Commit();

                // 尽力合并 WAL，避免 -wal 文件长期残留
                try
                {
                    using var ckptCmd = conn.CreateCommand();
                    ckptCmd.CommandText = "PRAGMA wal_checkpoint(PASSIVE);";
                    await ckptCmd.ExecuteNonQueryAsync();
                }
                catch { /* PASSIVE checkpoint 失败不阻断业务 */ }

                var guidTableId = LongToGuid(tableId);
                // 全量快照：提交后从数据库重建该表完整状态再存入 VersionHistory。
                // 修复：原实现直接存客户端发来的增量 PushTable，回滚时把旧增量重放到最新状态
                // 会复活已删除行列、旧值覆盖新值，产生混合损坏数据。
                byte[] fullSnapshot = await BuildFullTableSnapshotAsync(conn, tableId, projectId);
                await _historySvc.CreateSnapshotAsync(projectId, guidTableId, "Table", newVersion, changeType, fullSnapshot, userId);
                _logger.LogInformation("PushTableQuick 完成: TableId={TableId} Version={Version} Cells={CellCount} Attempt={Attempt}",
                    tableId, newVersion, pushTable.Cells.Count, attempt + 1);

                // Broadcast PeerTableChanged to all clients in the project group
                var projectIdStr = projectId.ToString();
                var groupName = $"project_{projectIdStr}";
                try
                {
                    await _hubContext.Clients.Group(groupName).SendAsync("PeerTableChanged",
                        projectIdStr, pushTable.Id.ToString(), newVersion.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PeerTableChanged 广播失败: ProjectId={Pid} TableId={Tid}", projectIdStr, pushTable.Id);
                }

                return new { Result = "Success", Version = newVersion };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        throw new InvalidOperationException($"PushTableQuick 版本号冲突，重试 {MaxVersionRetries} 次后失败");
    }

    /// <summary>
    /// 查询项目所属团队 Id（从主库 Projects 表读取）。
    /// 系统模板（TeamId=NULL）返回 Guid.Empty，调用方需通过 ProjectDbManager 的 _System 路径 fallback 处理。
    /// 不抛异常，避免系统模板编辑流程中断（系统模板的访问权限已由 CheckProjectAccessAsync 校验）。
    /// </summary>
    private async Task<Guid> GetTeamIdAsync(Guid projectId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TeamId FROM Projects WHERE Id=@pid";
        cmd.Parameters.AddWithValue("@pid", projectId.ToString());
        var teamIdStr = (string?)await cmd.ExecuteScalarAsync();
        if (string.IsNullOrEmpty(teamIdStr))
        {
            // 系统模板（TeamId=NULL）：返回 Guid.Empty，由 OpenProjectDb 路径 fallback 到 _System/Templates/ 处理
            return Guid.Empty;
        }
        return Guid.Parse(teamIdStr);
    }

    /// <summary>
    /// 节点级强锁：判断锁是否已过期（超过 30 分钟视为过期，允许其他用户抢占）。
    /// lockerAcquiredAt 为 null 或空字符串时视为过期（无获取时间记录）。
    /// 解析失败也视为过期（容错，避免脏数据导致死锁）。
    /// </summary>
    private static bool IsLockExpired(string? lockerAcquiredAt)
    {
        if (string.IsNullOrWhiteSpace(lockerAcquiredAt)) return true;
        if (!DateTime.TryParse(lockerAcquiredAt, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var acquired))
            return true;
        return (DateTime.UtcNow - acquired).TotalMinutes > 30;
    }

    /// <summary>
    /// 写入/更新 Table 表级字段。
    /// V2-M-06 修复：Mod 操作（Mask != -1）改用动态 UPDATE 只更新 Mask 标记的 dirty 字段，
    /// 避免 INSERT OR REPLACE 用 Protobuf 默认值（空字符串/0/null）覆盖未修改字段导致数据丢失。
    /// New 操作（Mask == -1）保持原 INSERT OR REPLACE 整体覆盖逻辑。
    /// Mask 位定义参考 Auditai.DTO.TableDirtyMask。
    /// </summary>
    private static async Task UpsertTableAsync(SqliteConnection conn, SqliteTransaction tx, PushTable t)
    {
        if (t.Mask != -1)
        {
            if (t.Mask == 0) return; // 表级字段无修改（仅子元素 dirty）
            var setClauses = new List<string>();
            using var modCmd = conn.CreateCommand();
            modCmd.Transaction = tx;
            int mask = t.Mask;
            // TableDirtyMask 位定义（Bit 11 Maker / Bit 12 Checker 在 PushTable 中无对应字段，忽略）
            if ((mask & 1) != 0) { setClauses.Add("`Title`=@Title"); modCmd.Parameters.AddWithValue("@Title", t.Title ?? ""); }
            if ((mask & 2) != 0) { setClauses.Add("`Note`=@Note"); modCmd.Parameters.AddWithValue("@Note", t.Note ?? ""); }
            if ((mask & 4) != 0) { setClauses.Add("`HeaderHeights`=@HeaderHeights"); modCmd.Parameters.AddWithValue("@HeaderHeights", (object?)t.HeaderHeights ?? DBNull.Value); }
            if ((mask & 8) != 0) { setClauses.Add("`DefaultStyleId`=@DefaultStyleId"); modCmd.Parameters.AddWithValue("@DefaultStyleId", t.DefaultStyleId); }
            if ((mask & 16) != 0) { setClauses.Add("`PageSetup`=@PageSetup"); modCmd.Parameters.AddWithValue("@PageSetup", t.PageSetup ?? ""); }
            if ((mask & 32) != 0) { setClauses.Add("`ConsolidateSettings`=@ConsolidateSettings"); modCmd.Parameters.AddWithValue("@ConsolidateSettings", (object?)t.ConsolidateSettings ?? DBNull.Value); }
            if ((mask & 64) != 0) { setClauses.Add("`BorderStyle`=@BorderStyle"); modCmd.Parameters.AddWithValue("@BorderStyle", t.BorderStyle); }
            if ((mask & 128) != 0) { setClauses.Add("`FrozenCols`=@FrozenCols"); modCmd.Parameters.AddWithValue("@FrozenCols", t.FrozenCols); }
            if ((mask & 256) != 0) { setClauses.Add("`HeaderMode`=@HeaderMode"); modCmd.Parameters.AddWithValue("@HeaderMode", t.HeaderMode); }
            if ((mask & 512) != 0) { setClauses.Add("`CollectSource`=@CollectSource"); modCmd.Parameters.AddWithValue("@CollectSource", (object?)t.CollectSource ?? DBNull.Value); }
            if ((mask & 1024) != 0)
            {
                setClauses.Add("`Locker`=@Locker");
                modCmd.Parameters.AddWithValue("@Locker", t.Locker);
                // 修复：Mod 路径同步刷新锁获取时间，避免换手持锁后 LockerAcquiredAt 残留旧值被立即判过期
                setClauses.Add("`LockerAcquiredAt`=@LockerAcquiredAt");
                modCmd.Parameters.AddWithValue("@LockerAcquiredAt", t.Locker != 0
                    ? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    : (object)DBNull.Value);
            }
            if ((mask & 8192) != 0) { setClauses.Add("`FilterInfo`=@FilterInfo"); modCmd.Parameters.AddWithValue("@FilterInfo", (object?)t.FilterInfo ?? DBNull.Value); }
            if ((mask & 16384) != 0) { setClauses.Add("`Foot`=@Foot"); modCmd.Parameters.AddWithValue("@Foot", t.Foot ?? ""); }
            if ((mask & 32768) != 0) { setClauses.Add("`RowOwnerExclusive`=@RowOwnerExclusive"); modCmd.Parameters.AddWithValue("@RowOwnerExclusive", t.RowOwnerExclusive ? 1 : 0); }
            if ((mask & 65536) != 0) { setClauses.Add("`RowOwnerLoad`=@RowOwnerLoad"); modCmd.Parameters.AddWithValue("@RowOwnerLoad", t.RowOwnerLoad ? 1 : 0); }
            if ((mask & 131072) != 0) { setClauses.Add("`RowOwnerLoadShare`=@RowOwnerLoadShare"); modCmd.Parameters.AddWithValue("@RowOwnerLoadShare", t.RowOwnerLoadShare.ToByteArray()); }
            if ((mask & 262144) != 0) { setClauses.Add("`Ticket`=@Ticket"); modCmd.Parameters.AddWithValue("@Ticket", t.Ticket ?? ""); }
            if ((mask & 524288) != 0) { setClauses.Add("`ControlFormula`=@ControlFormula"); modCmd.Parameters.AddWithValue("@ControlFormula", t.ControlFormula ?? ""); }
            if (setClauses.Count == 0) return;
            modCmd.CommandText = $"UPDATE `Table` SET {string.Join(", ", setClauses)} WHERE Id=@Id";
            modCmd.Parameters.AddWithValue("@Id", t.Id);
            await modCmd.ExecuteNonQueryAsync();
            return;
        }

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `Table`(
`Id`,`Title`,`PageSetup`,`Dirty`,`Note`,`HeaderHeights`,`DefaultStyleId`,
`ConsolidateSettings`,`BorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,
`Locker`,`LockerAcquiredAt`,`FilterInfo`,`Foot`,`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,
`Ticket`,`ControlFormula`,`CustomBorderStyle`)
VALUES (@Id,@Title,@PageSetup,@Dirty,@Note,@HeaderHeights,@DefaultStyleId,
@ConsolidateSettings,@BorderStyle,@FrozenCols,@HeaderMode,@CollectSource,
@Locker,@LockerAcquiredAt,@FilterInfo,@Foot,@RowOwnerExclusive,@RowOwnerLoad,@RowOwnerLoadShare,
@Ticket,@ControlFormula,@CustomBorderStyle)";
        cmd.Parameters.AddWithValue("@Id", t.Id);
        cmd.Parameters.AddWithValue("@Title", t.Title ?? "");
        cmd.Parameters.AddWithValue("@PageSetup", t.PageSetup ?? "");
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.Parameters.AddWithValue("@Note", t.Note ?? "");
        cmd.Parameters.AddWithValue("@HeaderHeights", (object?)t.HeaderHeights ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DefaultStyleId", t.DefaultStyleId);
        cmd.Parameters.AddWithValue("@ConsolidateSettings", (object?)t.ConsolidateSettings ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BorderStyle", t.BorderStyle);
        cmd.Parameters.AddWithValue("@FrozenCols", t.FrozenCols);
        cmd.Parameters.AddWithValue("@HeaderMode", t.HeaderMode);
        cmd.Parameters.AddWithValue("@CollectSource", (object?)t.CollectSource ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Locker", t.Locker);
        // 节点级强锁：New 操作整体覆盖时同步设置 LockerAcquiredAt。
        // t.Locker != 0 视为本次 Push 同时获取锁 → CURRENT_TIMESTAMP；否则 NULL。
        cmd.Parameters.AddWithValue("@LockerAcquiredAt", t.Locker != 0
            ? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            : (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FilterInfo", (object?)t.FilterInfo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Foot", t.Foot ?? "");
        cmd.Parameters.AddWithValue("@RowOwnerExclusive", t.RowOwnerExclusive ? 1 : 0);
        cmd.Parameters.AddWithValue("@RowOwnerLoad", t.RowOwnerLoad ? 1 : 0);
        cmd.Parameters.AddWithValue("@RowOwnerLoadShare", t.RowOwnerLoadShare.ToByteArray());
        cmd.Parameters.AddWithValue("@Ticket", t.Ticket ?? "");
        cmd.Parameters.AddWithValue("@ControlFormula", t.ControlFormula ?? "");
        cmd.Parameters.AddWithValue("@CustomBorderStyle", DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 写入/更新 Column。
    /// V2-M-06 修复：Mod 操作（Action=2 且 Mask!=-1）改用动态 UPDATE 只更新 dirty 字段。
    /// New 操作（Action=1 或 Mask=-1）保持原 INSERT OR REPLACE 整体覆盖逻辑。
    /// Mask 位定义参考 Auditai.DTO.ColumnDirtyMask。
    /// </summary>
    private static async Task UpsertColumnAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, PushColumn col)
    {
        if (col.Action == 2 && col.Mask != -1)
        {
            if (col.Mask == 0) return;
            var setClauses = new List<string>();
            using var modCmd = conn.CreateCommand();
            modCmd.Transaction = tx;
            int mask = col.Mask;
            // ColumnDirtyMask 位定义
            if ((mask & 1) != 0) { setClauses.Add("`Width`=@Width"); modCmd.Parameters.AddWithValue("@Width", col.Width); }
            if ((mask & 2) != 0) { setClauses.Add("`Caption`=@Caption"); modCmd.Parameters.AddWithValue("@Caption", col.Caption ?? ""); }
            if ((mask & 4) != 0) { setClauses.Add("`Visible`=@Visible"); modCmd.Parameters.AddWithValue("@Visible", col.Visible ? 1 : 0); }
            if ((mask & 8) != 0) { setClauses.Add("`Formula`=@Formula"); modCmd.Parameters.AddWithValue("@Formula", (object?)col.Formula ?? DBNull.Value); }
            if ((mask & 16) != 0) { setClauses.Add("`StyleId`=@StyleId"); modCmd.Parameters.AddWithValue("@StyleId", col.StyleId != null ? (object)col.StyleId.Value : DBNull.Value); }
            if ((mask & 32) != 0) { setClauses.Add("`CaptionStyle`=@CaptionStyle"); modCmd.Parameters.AddWithValue("@CaptionStyle", col.CaptionStyle ?? ""); }
            if ((mask & 64) != 0) { setClauses.Add("`ConsolidateAttribs`=@ConsolidateAttribs"); modCmd.Parameters.AddWithValue("@ConsolidateAttribs", (object?)col.ConsolidateAttribs ?? DBNull.Value); }
            if ((mask & 128) != 0) { setClauses.Add("`SubtotalAttribs`=@SubtotalAttribs"); modCmd.Parameters.AddWithValue("@SubtotalAttribs", col.SubtotalAttribs); }
            if ((mask & 256) != 0) { setClauses.Add("`Permissions`=@Permissions"); modCmd.Parameters.AddWithValue("@Permissions", col.Permissions ?? ""); }
            if ((mask & 512) != 0) { setClauses.Add("`CaptionFormula`=@CaptionFormula"); modCmd.Parameters.AddWithValue("@CaptionFormula", col.CaptionFormula ?? ""); }
            if ((mask & 1024) != 0) { setClauses.Add("`Index`=@Index"); modCmd.Parameters.AddWithValue("@Index", col.Index); }
            if ((mask & 2048) != 0) { setClauses.Add("`CrossAttributes`=@CrossAttributes"); modCmd.Parameters.AddWithValue("@CrossAttributes", col.CrossAttributes.ToByteArray()); }
            if (setClauses.Count == 0) return;
            modCmd.CommandText = $"UPDATE `Column` SET {string.Join(", ", setClauses)} WHERE Id=@Id";
            modCmd.Parameters.AddWithValue("@Id", col.Id);
            await modCmd.ExecuteNonQueryAsync();
            return;
        }

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `Column`(
`Id`,`TableId`,`Index`,`ServerIndex`,`Caption`,`CaptionStyle`,`Width`,`Visible`,
`Dirty`,`Status`,`ConsolidateAttribs`,`SubtotalAttribs`,`Formula`,`StyleId`,
`Permissions`,`CaptionFormula`,`CrossAttributes`)
VALUES (@Id,@TableId,@Index,@ServerIndex,@Caption,@CaptionStyle,@Width,@Visible,
@Dirty,@Status,@ConsolidateAttribs,@SubtotalAttribs,@Formula,@StyleId,
@Permissions,@CaptionFormula,@CrossAttributes)";
        cmd.Parameters.AddWithValue("@Id", col.Id);
        cmd.Parameters.AddWithValue("@TableId", tableId);
        cmd.Parameters.AddWithValue("@Index", col.Index);
        cmd.Parameters.AddWithValue("@ServerIndex", 0);
        cmd.Parameters.AddWithValue("@Caption", col.Caption ?? "");
        cmd.Parameters.AddWithValue("@CaptionStyle", col.CaptionStyle ?? "");
        cmd.Parameters.AddWithValue("@Width", col.Width);
        cmd.Parameters.AddWithValue("@Visible", col.Visible ? 1 : 0);
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.Parameters.AddWithValue("@Status", 0);
        cmd.Parameters.AddWithValue("@ConsolidateAttribs", (object?)col.ConsolidateAttribs ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SubtotalAttribs", col.SubtotalAttribs);
        cmd.Parameters.AddWithValue("@Formula", (object?)col.Formula ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StyleId", col.StyleId != null ? (object)col.StyleId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@Permissions", col.Permissions ?? "");
        cmd.Parameters.AddWithValue("@CaptionFormula", col.CaptionFormula ?? "");
        cmd.Parameters.AddWithValue("@CrossAttributes", col.CrossAttributes.ToByteArray());
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 写入/更新 Row。
    /// V2-M-06 修复：Mod 操作（Action=2 且 Mask!=-1）改用动态 UPDATE 只更新 dirty 字段。
    /// New 操作（Action=1 或 Mask=-1）保持原 INSERT OR REPLACE 整体覆盖逻辑。
    /// Mask 位定义参考 Auditai.DTO.RowDirtyMask。
    /// </summary>
    private static async Task UpsertRowAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, PushRow row)
    {
        if (row.Action == 2 && row.Mask != -1)
        {
            if (row.Mask == 0) return;
            var setClauses = new List<string>();
            using var modCmd = conn.CreateCommand();
            modCmd.Transaction = tx;
            int mask = row.Mask;
            // RowDirtyMask 位定义（注意：DB 列名 Locked 对应 PushRow.Locker）
            if ((mask & 1) != 0) { setClauses.Add("`Height`=@Height"); modCmd.Parameters.AddWithValue("@Height", row.Height); }
            if ((mask & 2) != 0) { setClauses.Add("`Visible`=@Visible"); modCmd.Parameters.AddWithValue("@Visible", row.Visible ? 1 : 0); }
            if ((mask & 4) != 0) { setClauses.Add("`Role`=@Role"); modCmd.Parameters.AddWithValue("@Role", row.Role); }
            if ((mask & 8) != 0) { setClauses.Add("`Locked`=@Locked"); modCmd.Parameters.AddWithValue("@Locked", row.Locker); }
            if ((mask & 16) != 0) { setClauses.Add("`Permissions`=@Permissions"); modCmd.Parameters.AddWithValue("@Permissions", row.Permissions ?? ""); }
            if ((mask & 32) != 0) { setClauses.Add("`Creator`=@Creator"); modCmd.Parameters.AddWithValue("@Creator", row.Creator); }
            if ((mask & 64) != 0) { setClauses.Add("`Index`=@Index"); modCmd.Parameters.AddWithValue("@Index", row.Index); }
            if (setClauses.Count == 0) return;
            modCmd.CommandText = $"UPDATE `Row` SET {string.Join(", ", setClauses)} WHERE Id=@Id";
            modCmd.Parameters.AddWithValue("@Id", row.Id);
            await modCmd.ExecuteNonQueryAsync();
            return;
        }

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `Row`(
`Id`,`TableId`,`Index`,`ServerIndex`,`Height`,`Visible`,`Dirty`,`Status`,
`Locked`,`Role`,`Permissions`,`Creator`)
VALUES (@Id,@TableId,@Index,@ServerIndex,@Height,@Visible,@Dirty,@Status,
@Locked,@Role,@Permissions,@Creator)";
        cmd.Parameters.AddWithValue("@Id", row.Id);
        cmd.Parameters.AddWithValue("@TableId", tableId);
        cmd.Parameters.AddWithValue("@Index", row.Index);
        cmd.Parameters.AddWithValue("@ServerIndex", 0);
        cmd.Parameters.AddWithValue("@Height", row.Height);
        cmd.Parameters.AddWithValue("@Visible", row.Visible ? 1 : 0);
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.Parameters.AddWithValue("@Status", 0);
        cmd.Parameters.AddWithValue("@Locked", row.Locker);
        cmd.Parameters.AddWithValue("@Role", row.Role);
        cmd.Parameters.AddWithValue("@Permissions", row.Permissions ?? "");
        cmd.Parameters.AddWithValue("@Creator", row.Creator);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 写入/更新 Cell。
    /// V2-M-06 修复：Mod 操作（Action=2 且 Mask!=-1）改用动态 UPDATE 只更新 dirty 字段。
    /// New 操作（Action=1 或 Mask=-1）保持原 INSERT OR REPLACE 整体覆盖逻辑。
    /// Mask 位定义参考 Auditai.DTO.CellDirtyMask。
    /// </summary>
    private static async Task UpsertCellAsync(SqliteConnection conn, SqliteTransaction tx, PushCell cell)
    {
        if (cell.Action == 2 && cell.Mask != -1)
        {
            if (cell.Mask == 0) return;
            var setClauses = new List<string>();
            using var modCmd = conn.CreateCommand();
            modCmd.Transaction = tx;
            int mask = cell.Mask;
            // CellDirtyMask 位定义
            if ((mask & 1) != 0) { setClauses.Add("`Value`=@Value"); modCmd.Parameters.AddWithValue("@Value", cell.Value.ToByteArray()); }
            if ((mask & 2) != 0) { setClauses.Add("`Formula`=@Formula"); modCmd.Parameters.AddWithValue("@Formula", (object?)cell.Formula ?? DBNull.Value); }
            if ((mask & 4) != 0) { setClauses.Add("`StyleId`=@StyleId"); modCmd.Parameters.AddWithValue("@StyleId", cell.StyleId != null ? (object)cell.StyleId.Value : DBNull.Value); }
            if ((mask & 8) != 0) { setClauses.Add("`CollectSource`=@CollectSource"); modCmd.Parameters.AddWithValue("@CollectSource", (object?)cell.CollectSource ?? DBNull.Value); }
            if ((mask & 16) != 0) { setClauses.Add("`HeaderFormula`=@HeaderFormula"); modCmd.Parameters.AddWithValue("@HeaderFormula", cell.HeaderFormula ?? ""); }
            if (setClauses.Count == 0) return;
            modCmd.CommandText = $"UPDATE `Cell` SET {string.Join(", ", setClauses)} WHERE Id=@Id";
            modCmd.Parameters.AddWithValue("@Id", cell.Id);
            await modCmd.ExecuteNonQueryAsync();
            return;
        }

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `Cell`(
`Id`,`RowId`,`ColumnId`,`Value`,`StyleId`,`Dirty`,`Status`,`Formula`,
`CollectSource`,`HeaderFormula`)
VALUES (@Id,@RowId,@ColumnId,@Value,@StyleId,@Dirty,@Status,@Formula,
@CollectSource,@HeaderFormula)";
        cmd.Parameters.AddWithValue("@Id", cell.Id);
        cmd.Parameters.AddWithValue("@RowId", cell.RId);
        cmd.Parameters.AddWithValue("@ColumnId", cell.CId);
        cmd.Parameters.AddWithValue("@Value", cell.Value.ToByteArray());
        cmd.Parameters.AddWithValue("@StyleId", cell.StyleId != null ? (object)cell.StyleId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.Parameters.AddWithValue("@Status", 0);
        cmd.Parameters.AddWithValue("@Formula", (object?)cell.Formula ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CollectSource", (object?)cell.CollectSource ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HeaderFormula", cell.HeaderFormula ?? "");
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task UpsertMergeAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, PushMerge merge)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `Merge`(`Id`,`TableId`,`TopLeft`,`BottomRight`,`Status`)
VALUES (@Id,@TableId,@TopLeft,@BottomRight,@Status)";
        cmd.Parameters.AddWithValue("@Id", merge.Id);
        cmd.Parameters.AddWithValue("@TableId", tableId);
        cmd.Parameters.AddWithValue("@TopLeft", merge.TopLeft);
        cmd.Parameters.AddWithValue("@BottomRight", merge.BottomRight);
        cmd.Parameters.AddWithValue("@Status", 0);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task UpsertCellStyleAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, PushCellStyle s)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `CellStyle`(
`Id`,`TableId`,`FontFamily`,`FontSize`,`ForeColor`,`BackColor`,`Align`,`Margin`,
`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Status`,`Locked`,`DefaultValue`,`Comment`)
VALUES (@Id,@TableId,@FontFamily,@FontSize,@ForeColor,@BackColor,@Align,@Margin,
@Bold,@Italic,@Underline,@DataType,@Format,@Status,@Locked,@DefaultValue,@Comment)";
        cmd.Parameters.AddWithValue("@Id", s.Id);
        cmd.Parameters.AddWithValue("@TableId", tableId);
        cmd.Parameters.AddWithValue("@FontFamily", (object?)s.FontFamily ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FontSize", s.FontSize);
        cmd.Parameters.AddWithValue("@ForeColor", s.ForeColor);
        cmd.Parameters.AddWithValue("@BackColor", s.BackColor);
        cmd.Parameters.AddWithValue("@Align", s.Align);
        cmd.Parameters.AddWithValue("@Margin", s.Margin);
        cmd.Parameters.AddWithValue("@Bold", s.Bold ? 1 : 0);
        cmd.Parameters.AddWithValue("@Italic", s.Italic ? 1 : 0);
        cmd.Parameters.AddWithValue("@Underline", s.Underline ? 1 : 0);
        cmd.Parameters.AddWithValue("@DataType", s.DataType);
        cmd.Parameters.AddWithValue("@Format", (object?)s.Format ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Status", 0);
        cmd.Parameters.AddWithValue("@Locked", s.Locker);
        cmd.Parameters.AddWithValue("@DefaultValue", (object?)s.DefaultValue ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Comment", (object?)s.Comment ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 写入单元格附件信息到 CellProp 表的 Attachments 字段。
    /// V2-H-05 修复：使用 ON CONFLICT 保留原有 Status 字段，仅更新 Attachments 和 Dirty。
    /// </summary>
    private static async Task UpsertCellAttachmentAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, PushCellAttachment att)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT INTO `CellProp`(`TableId`,`CellId`,`Dirty`,`Status`,`Attachments`)
VALUES (@TableId,@CellId,0,0,@Attachments)
ON CONFLICT(`TableId`,`CellId`) DO UPDATE SET
    `Attachments`=excluded.`Attachments`,
    `Dirty`=0";
        cmd.Parameters.AddWithValue("@TableId", tableId);
        cmd.Parameters.AddWithValue("@CellId", att.CellId);
        cmd.Parameters.AddWithValue("@Attachments", att.Attachments.ToByteArray());
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 删除单元格附件记录（Action=3）。
    /// V2-H-05 修复：CellProp 主键为 (TableId, CellId)。
    /// </summary>
    private static async Task DeleteCellAttachmentAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, long cellId)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "DELETE FROM `CellProp` WHERE TableId=@tid AND CellId=@cid";
        cmd.Parameters.AddWithValue("@tid", tableId);
        cmd.Parameters.AddWithValue("@cid", cellId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task DeleteEntityAsync(SqliteConnection conn, SqliteTransaction tx, string quotedTable, long id)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = $"DELETE FROM {quotedTable} WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 删除单元格附件记录（按 TableId + CellId）。
    /// </summary>
    private static async Task DeleteCellPropAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, long cellId)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "DELETE FROM `CellProp` WHERE TableId=@tid AND CellId=@cid";
        cmd.Parameters.AddWithValue("@tid", tableId);
        cmd.Parameters.AddWithValue("@cid", cellId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 级联删除行：同时清理该行关联的附件、合并与单元格，防止孤儿数据残留
    /// （原实现只删 Row 行，Cell/CellProp/Merge 残留在库中，随 Pull Join 被过滤但持续污染存储）。
    /// </summary>
    private static async Task DeleteRowCascadeAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, long rowId)
    {
        string[] sqls =
        {
            "DELETE FROM `CellProp` WHERE TableId=@tid AND CellId IN (SELECT Id FROM `Cell` WHERE RowId=@id)",
            "DELETE FROM `Merge` WHERE TableId=@tid AND (`TopLeft` IN (SELECT Id FROM `Cell` WHERE RowId=@id) OR `BottomRight` IN (SELECT Id FROM `Cell` WHERE RowId=@id))",
            "DELETE FROM `Cell` WHERE RowId=@id",
            "DELETE FROM `Row` WHERE Id=@id"
        };
        foreach (var sql in sqls)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@tid", tableId);
            cmd.Parameters.AddWithValue("@id", rowId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// 级联删除列：同时清理该列关联的附件、合并与单元格。
    /// </summary>
    private static async Task DeleteColumnCascadeAsync(SqliteConnection conn, SqliteTransaction tx, long tableId, long columnId)
    {
        string[] sqls =
        {
            "DELETE FROM `CellProp` WHERE TableId=@tid AND CellId IN (SELECT Id FROM `Cell` WHERE ColumnId=@id)",
            "DELETE FROM `Merge` WHERE TableId=@tid AND (`TopLeft` IN (SELECT Id FROM `Cell` WHERE ColumnId=@id) OR `BottomRight` IN (SELECT Id FROM `Cell` WHERE ColumnId=@id))",
            "DELETE FROM `Cell` WHERE ColumnId=@id",
            "DELETE FROM `Column` WHERE Id=@id"
        };
        foreach (var sql in sqls)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@tid", tableId);
            cmd.Parameters.AddWithValue("@id", columnId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// 清理孤儿 Merge：引用的 TopLeft/BottomRight 单元格已不存在（跨行列合并的中间单元格
    /// 被级行/列删除后，TopLeft/BottomRight 可能落在其他行上）。
    /// </summary>
    private static async Task DeleteOrphanMergesAsync(SqliteConnection conn, SqliteTransaction tx, long tableId)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"DELETE FROM `Merge` WHERE TableId=@tid AND (
`TopLeft` NOT IN (SELECT Id FROM `Cell` WHERE RowId IN (SELECT Id FROM `Row` WHERE TableId=@tid))
OR `BottomRight` NOT IN (SELECT Id FROM `Cell` WHERE RowId IN (SELECT Id FROM `Row` WHERE TableId=@tid)))";
        cmd.Parameters.AddWithValue("@tid", tableId);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 整体覆盖模式（Revert）：清空该表全部子数据（附件/合并/单元格/样式/行/列），
    /// 随后由调用方按全量快照重建，保证回滚结果是完整快照状态。
    /// </summary>
    private static async Task ClearTableChildrenAsync(SqliteConnection conn, SqliteTransaction tx, long tableId)
    {
        string[] sqls =
        {
            "DELETE FROM `CellProp` WHERE TableId=@tid",
            "DELETE FROM `Merge` WHERE TableId=@tid",
            "DELETE FROM `Cell` WHERE RowId IN (SELECT Id FROM `Row` WHERE TableId=@tid)",
            "DELETE FROM `CellStyle` WHERE TableId=@tid",
            "DELETE FROM `Row` WHERE TableId=@tid",
            "DELETE FROM `Column` WHERE TableId=@tid"
        };
        foreach (var sql in sqls)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@tid", tableId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// 检测快照是否为旧版增量格式（含 Modify/Delete 动作即为增量）。
    /// 旧版快照存的是客户端当次推送的增量 PushTable，不能作为回滚数据源（重放会产生混合损坏数据）。
    /// </summary>
    private static bool IsIncrementalTableSnapshot(PushTable t)
    {
        return t.Columns.Any(c => c.Action == 2 || c.Action == 3)
            || t.Rows.Any(r => r.Action == 2 || r.Action == 3)
            || t.Cells.Any(c => c.Action == 2 || c.Action == 3)
            || t.Merges.Any(m => m.Action == 2 || m.Action == 3)
            || t.CellAttachments.Any(a => a.Action == 2 || a.Action == 3);
    }

    /// <summary>
    /// 从项目 .db 重建该表完整状态，序列化为全量 PushTable（Mask=-1，子元素全部 Action=1）作为版本快照。
    /// 该快照可直接经 SavePushTableAsync(replaceExisting:true) 应用，实现正确的版本回滚。
    /// </summary>
    private static async Task<byte[]> BuildFullTableSnapshotAsync(SqliteConnection conn, long tableId, Guid projectId)
    {
        var pt = new PushTable
        {
            Id = tableId,
            ProjectId = ByteString.CopyFrom(projectId.ToByteArray()),
            Version = 0,
            Mask = -1
        };

        // 表级字段
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"SELECT `Title`,`Note`,`PageSetup`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,
`BorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,
`RowOwnerExclusive`,`RowOwnerLoad`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula`
FROM `Table` WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", tableId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                pt.Title = reader.IsDBNull(0) ? "" : reader.GetString(0);
                pt.Note = reader.IsDBNull(1) ? "" : reader.GetString(1);
                pt.PageSetup = reader.IsDBNull(2) ? "" : reader.GetString(2);
                pt.HeaderHeights = reader.IsDBNull(3) ? "" : reader.GetString(3);
                pt.DefaultStyleId = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                pt.ConsolidateSettings = reader.IsDBNull(5) ? "" : reader.GetString(5);
                pt.BorderStyle = reader.GetInt32(6);
                pt.FrozenCols = reader.GetInt32(7);
                pt.HeaderMode = reader.GetInt32(8);
                pt.CollectSource = reader.IsDBNull(9) ? "" : reader.GetString(9);
                pt.Locker = reader.IsDBNull(10) ? 0 : reader.GetInt64(10);
                pt.FilterInfo = reader.IsDBNull(11) ? "" : reader.GetString(11);
                pt.Foot = reader.IsDBNull(12) ? "" : reader.GetString(12);
                pt.RowOwnerExclusive = reader.GetInt32(13) != 0;
                pt.RowOwnerLoad = reader.GetInt32(14) != 0;
                pt.RowOwnerLoadShare = reader.IsDBNull(15) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[15]);
                pt.Ticket = reader.IsDBNull(16) ? "" : reader.GetString(16);
                pt.ControlFormula = reader.IsDBNull(17) ? "" : reader.GetString(17);
            }
        }

        // Columns（全量 Action=1）
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT `Id`,`Caption`,`Index`,`Width`,`Visible`,`StyleId`,`CaptionStyle`,`ConsolidateAttribs`,`Formula`,`SubtotalAttribs`,`Permissions`,`CaptionFormula`,`CrossAttributes` FROM `Column` WHERE TableId=@tid";
            cmd.Parameters.AddWithValue("@tid", tableId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var pc = new PushColumn
                {
                    Action = 1,
                    Id = reader.GetInt64(0),
                    Caption = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Index = reader.GetInt32(2),
                    Width = reader.GetInt32(3),
                    Visible = reader.GetInt32(4) != 0,
                    StyleId = reader.IsDBNull(5) ? null : new Int64Value { Value = reader.GetInt64(5) },
                    CaptionStyle = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    ConsolidateAttribs = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    Formula = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    SubtotalAttribs = reader.GetInt32(9),
                    Permissions = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    CaptionFormula = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    CrossAttributes = reader.IsDBNull(12) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[12]),
                    Mask = -1
                };
                pt.Columns.Add(pc);
            }
        }

        // Rows
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT `Id`,`Index`,`Height`,`Visible`,`Locked`,`Role`,`Permissions`,`Creator` FROM `Row` WHERE TableId=@tid";
            cmd.Parameters.AddWithValue("@tid", tableId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pt.Rows.Add(new PushRow
                {
                    Action = 1,
                    Id = reader.GetInt64(0),
                    Index = reader.GetInt32(1),
                    Height = reader.GetInt32(2),
                    Visible = reader.GetInt32(3) != 0,
                    Locker = reader.IsDBNull(4) ? 0 : reader.GetInt64(4),
                    Role = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    Permissions = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Creator = reader.IsDBNull(7) ? 0 : reader.GetInt64(7),
                    Mask = -1
                });
            }
        }

        // Cells
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT c.`Id`,c.`ColumnId`,c.`RowId`,c.`Value`,c.`Formula`,c.`StyleId`,c.`CollectSource`,c.`HeaderFormula` FROM `Cell` c INNER JOIN `Row` r ON c.RowId=r.Id WHERE r.TableId=@tid";
            cmd.Parameters.AddWithValue("@tid", tableId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pt.Cells.Add(new PushCell
                {
                    Action = 1,
                    Id = reader.GetInt64(0),
                    CId = reader.GetInt64(1),
                    RId = reader.GetInt64(2),
                    Value = reader.IsDBNull(3) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[3]),
                    Formula = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    StyleId = reader.IsDBNull(5) ? null : new Int64Value { Value = reader.GetInt64(5) },
                    CollectSource = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    HeaderFormula = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    Mask = -1
                });
            }
        }

        // Merges
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT `Id`,`TopLeft`,`BottomRight` FROM `Merge` WHERE TableId=@tid";
            cmd.Parameters.AddWithValue("@tid", tableId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pt.Merges.Add(new PushMerge
                {
                    Action = 1,
                    Id = reader.GetInt64(0),
                    TopLeft = reader.GetInt64(1),
                    BottomRight = reader.GetInt64(2)
                });
            }
        }

        // CellStyles
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT `Id`,`FontFamily`,`FontSize`,`ForeColor`,`BackColor`,`Align`,`Margin`,`Bold`,`Italic`,`Underline`,`DataType`,`Format`,`Locked`,`DefaultValue`,`Comment` FROM `CellStyle` WHERE TableId=@tid";
            cmd.Parameters.AddWithValue("@tid", tableId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pt.CellStyles.Add(new PushCellStyle
                {
                    Id = reader.GetInt64(0),
                    FontFamily = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    FontSize = reader.GetFloat(2),
                    ForeColor = reader.GetInt32(3),
                    BackColor = reader.GetInt32(4),
                    Align = reader.GetInt32(5),
                    Margin = reader.GetInt32(6),
                    Bold = reader.GetInt32(7) != 0,
                    Italic = reader.GetInt32(8) != 0,
                    Underline = reader.GetInt32(9) != 0,
                    DataType = reader.GetInt32(10),
                    Format = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    Locker = reader.IsDBNull(12) ? 0 : reader.GetInt64(12),
                    DefaultValue = reader.IsDBNull(13) ? "" : reader.GetString(13),
                    Comment = reader.IsDBNull(14) ? "" : reader.GetString(14)
                });
            }
        }

        // CellProps（单元格附件）
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT `CellId`,`Attachments` FROM `CellProp` WHERE TableId=@tid";
            cmd.Parameters.AddWithValue("@tid", tableId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pt.CellAttachments.Add(new PushCellAttachment
                {
                    Action = 1,
                    TableId = tableId,
                    CellId = reader.GetInt64(0),
                    Attachments = reader.IsDBNull(1) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[1])
                });
            }
        }

        return pt.ToByteArray();
    }

    private static async Task FillTableLevelFieldsAsync(SqliteConnection conn, PullTable pullTable, long tableId)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT `Title`,`PageSetup`,`Note`,`HeaderHeights`,`DefaultStyleId`,`ConsolidateSettings`,`BorderStyle`,`FrozenCols`,`HeaderMode`,`CollectSource`,`Locker`,`FilterInfo`,`Foot`,`RowOwnerLoadShare`,`Ticket`,`ControlFormula` FROM `Table` WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", tableId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return;
        pullTable.Title = new OptionalString { Value = reader.IsDBNull(0) ? "" : reader.GetString(0) };
        pullTable.PageSetup = new OptionalString { Value = reader.IsDBNull(1) ? "" : reader.GetString(1) };
        pullTable.Note = new OptionalString { Value = reader.IsDBNull(2) ? "" : reader.GetString(2) };
        pullTable.HeaderHeights = new OptionalString { Value = reader.IsDBNull(3) ? "" : reader.GetString(3) };
        pullTable.DefaultStyleId = reader.IsDBNull(4) ? new NullableInt64 { IsNull = true } : new NullableInt64 { Value = reader.GetInt64(4) };
        pullTable.ConsolidateSettings = new OptionalString { Value = reader.IsDBNull(5) ? "" : reader.GetString(5) };
        pullTable.BorderStyle = new OptionalInt32 { Value = reader.GetInt32(6) };
        pullTable.FrozenCols = new OptionalInt32 { Value = reader.GetInt32(7) };
        pullTable.HeaderMode = new OptionalInt32 { Value = reader.GetInt32(8) };
        pullTable.CollectSource = new OptionalString { Value = reader.IsDBNull(9) ? "" : reader.GetString(9) };
        pullTable.Locker = new OptionalInt64 { Value = reader.IsDBNull(10) ? 0 : reader.GetInt64(10) };
        pullTable.FilterInfo = new OptionalString { Value = reader.IsDBNull(11) ? "" : reader.GetString(11) };
        pullTable.Foot = new OptionalString { Value = reader.IsDBNull(12) ? "" : reader.GetString(12) };
        pullTable.RowOwnerLoadShare = new OptionalBytes { Value = reader.IsDBNull(13) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[13]) };
        pullTable.Ticket = new OptionalString { Value = reader.IsDBNull(14) ? "" : reader.GetString(14) };
        pullTable.ControlFormula = new OptionalString { Value = reader.IsDBNull(15) ? "" : reader.GetString(15) };
    }

    private static PullColumn ReadPullColumn(SqliteDataReader reader)
    {
        var pc = new PullColumn { Id = reader.GetInt64(0) };
        pc.Caption = new OptionalString { Value = reader.IsDBNull(1) ? "" : reader.GetString(1) };
        pc.Index = new OptionalInt32 { Value = reader.GetInt32(2) };
        pc.Width = new OptionalInt32 { Value = reader.GetInt32(3) };
        pc.Visible = new OptionalBool { Value = reader.GetInt32(4) != 0 };
        pc.StyleId = reader.IsDBNull(5) ? new NullableInt64 { IsNull = true } : new NullableInt64 { Value = reader.GetInt64(5) };
        pc.CaptionStyle = new OptionalString { Value = reader.IsDBNull(6) ? "" : reader.GetString(6) };
        pc.ConsolidateAttribs = new OptionalString { Value = reader.IsDBNull(7) ? "" : reader.GetString(7) };
        pc.Formula = new OptionalString { Value = reader.IsDBNull(8) ? "" : reader.GetString(8) };
        pc.SubtotalAttribs = new OptionalInt32 { Value = reader.GetInt32(9) };
        pc.Permissions = new OptionalString { Value = reader.IsDBNull(10) ? "" : reader.GetString(10) };
        pc.CaptionFormula = new OptionalString { Value = reader.IsDBNull(11) ? "" : reader.GetString(11) };
        pc.CrossAttributes = new OptionalBytes { Value = reader.IsDBNull(12) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[12]) };
        return pc;
    }

    private static PullRow ReadPullRow(SqliteDataReader reader)
    {
        var pr = new PullRow { Id = reader.GetInt64(0) };
        pr.Index = new OptionalInt32 { Value = reader.GetInt32(1) };
        pr.Height = new OptionalInt32 { Value = reader.GetInt32(2) };
        pr.Visible = new OptionalBool { Value = reader.GetInt32(3) != 0 };
        pr.Locker = new OptionalInt64 { Value = reader.IsDBNull(4) ? 0 : reader.GetInt64(4) };
        pr.Role = new OptionalInt32 { Value = reader.IsDBNull(5) ? 0 : reader.GetInt32(5) };
        pr.Permissions = new OptionalString { Value = reader.IsDBNull(6) ? "" : reader.GetString(6) };
        pr.Creator = new OptionalInt64 { Value = reader.GetInt64(7) };
        return pr;
    }

    private static PullCell ReadPullCell(SqliteDataReader reader)
    {
        var pc = new PullCell { Id = reader.GetInt64(0) };
        pc.CId = new OptionalInt64 { Value = reader.GetInt64(1) };
        pc.RId = new OptionalInt64 { Value = reader.GetInt64(2) };
        pc.Value = new OptionalBytes { Value = reader.IsDBNull(3) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[3]) };
        pc.Formula = new OptionalString { Value = reader.IsDBNull(4) ? "" : reader.GetString(4) };
        pc.Style = reader.IsDBNull(5) ? new NullableInt64 { IsNull = true } : new NullableInt64 { Value = reader.GetInt64(5) };
        pc.CollectSource = new OptionalString { Value = reader.IsDBNull(6) ? "" : reader.GetString(6) };
        pc.HeaderFormula = new OptionalString { Value = reader.IsDBNull(7) ? "" : reader.GetString(7) };
        return pc;
    }

    private static PullMerge ReadPullMerge(SqliteDataReader reader)
    {
        var pm = new PullMerge { Id = reader.GetInt64(0) };
        pm.TopLeft = new OptionalInt64 { Value = reader.GetInt64(1) };
        pm.BottomRight = new OptionalInt64 { Value = reader.GetInt64(2) };
        return pm;
    }

    private static PullCellStyle ReadPullCellStyle(SqliteDataReader reader)
    {
        var s = new PullCellStyle { Id = reader.GetInt64(0) };
        s.FontFamily = reader.IsDBNull(1) ? new NullableString { IsNull = true } : new NullableString { Value = reader.GetString(1) };
        s.FontSize = reader.IsDBNull(2) ? new NullableFloat { IsNull = true } : new NullableFloat { Value = reader.GetFloat(2) };
        s.ForeColor = reader.IsDBNull(3) ? new NullableInt32 { IsNull = true } : new NullableInt32 { Value = reader.GetInt32(3) };
        s.BackColor = reader.IsDBNull(4) ? new NullableInt32 { IsNull = true } : new NullableInt32 { Value = reader.GetInt32(4) };
        s.Align = reader.IsDBNull(5) ? new NullableInt32 { IsNull = true } : new NullableInt32 { Value = reader.GetInt32(5) };
        s.Margin = reader.IsDBNull(6) ? new NullableInt32 { IsNull = true } : new NullableInt32 { Value = reader.GetInt32(6) };
        s.Bold = reader.IsDBNull(7) ? new NullableBool { IsNull = true } : new NullableBool { Value = reader.GetInt32(7) != 0 };
        s.Italic = reader.IsDBNull(8) ? new NullableBool { IsNull = true } : new NullableBool { Value = reader.GetInt32(8) != 0 };
        s.Underline = reader.IsDBNull(9) ? new NullableBool { IsNull = true } : new NullableBool { Value = reader.GetInt32(9) != 0 };
        s.DataType = reader.IsDBNull(10) ? new NullableInt32 { IsNull = true } : new NullableInt32 { Value = reader.GetInt32(10) };
        s.Format = reader.IsDBNull(11) ? new NullableString { IsNull = true } : new NullableString { Value = reader.GetString(11) };
        s.Locker = reader.IsDBNull(12) ? new NullableInt64 { IsNull = true } : new NullableInt64 { Value = reader.GetInt64(12) };
        s.DefaultValue = reader.IsDBNull(13) ? new NullableString { IsNull = true } : new NullableString { Value = reader.GetString(13) };
        s.Comment = reader.IsDBNull(14) ? new NullableString { IsNull = true } : new NullableString { Value = reader.GetString(14) };
        return s;
    }

    /// <summary>
    /// 将 PushTable 转换为 PullTable 格式（V2-C-03 修复）。
    /// 快照存储的是 PushTable 序列化字节，差异预览需转换为 PullTable 返回客户端。
    /// 转换逻辑参考 PullTableAsync 方法中从项目 .db 读取数据构建 PullTable 的代码。
    /// 所有 Push 子元素放入 PullTable 的 New* 集合（表示完整状态而非增量）。
    /// </summary>
    private static PullTable ConvertPushTableToPullTable(PushTable pushTable, int version)
    {
        // 转换结果包含完整数据，客户端据此 Merge；与 PullTableAsync 数据分支一致使用 "NeedUpdate"
        var pullTable = new PullTable { Result = "NeedUpdate", Version = version };

        // 表级字段
        pullTable.Title = new OptionalString { Value = pushTable.Title ?? "" };
        pullTable.Note = new OptionalString { Value = pushTable.Note ?? "" };
        pullTable.HeaderHeights = new OptionalString { Value = pushTable.HeaderHeights ?? "" };
        pullTable.DefaultStyleId = pushTable.DefaultStyleId != 0
            ? new NullableInt64 { Value = pushTable.DefaultStyleId }
            : new NullableInt64 { IsNull = true };
        pullTable.PageSetup = new OptionalString { Value = pushTable.PageSetup ?? "" };
        pullTable.ConsolidateSettings = new OptionalString { Value = pushTable.ConsolidateSettings ?? "" };
        pullTable.BorderStyle = new OptionalInt32 { Value = pushTable.BorderStyle };
        pullTable.FrozenCols = new OptionalInt32 { Value = pushTable.FrozenCols };
        pullTable.HeaderMode = new OptionalInt32 { Value = pushTable.HeaderMode };
        pullTable.CollectSource = new OptionalString { Value = pushTable.CollectSource ?? "" };
        pullTable.Locker = new OptionalInt64 { Value = pushTable.Locker };
        pullTable.FilterInfo = new OptionalString { Value = pushTable.FilterInfo ?? "" };
        pullTable.Foot = new OptionalString { Value = pushTable.Foot ?? "" };
        pullTable.RowOwnerLoadShare = new OptionalBytes { Value = pushTable.RowOwnerLoadShare };
        pullTable.Ticket = new OptionalString { Value = pushTable.Ticket ?? "" };
        pullTable.ControlFormula = new OptionalString { Value = pushTable.ControlFormula ?? "" };

        // Columns → NewColumns
        foreach (var col in pushTable.Columns)
        {
            var pc = new PullColumn { Id = col.Id };
            pc.Caption = new OptionalString { Value = col.Caption ?? "" };
            pc.Index = new OptionalInt32 { Value = col.Index };
            pc.Width = new OptionalInt32 { Value = col.Width };
            pc.Visible = new OptionalBool { Value = col.Visible };
            pc.StyleId = col.StyleId != null
                ? new NullableInt64 { Value = col.StyleId.Value }
                : new NullableInt64 { IsNull = true };
            pc.CaptionStyle = new OptionalString { Value = col.CaptionStyle ?? "" };
            pc.ConsolidateAttribs = new OptionalString { Value = col.ConsolidateAttribs ?? "" };
            pc.Formula = new OptionalString { Value = col.Formula ?? "" };
            pc.SubtotalAttribs = new OptionalInt32 { Value = col.SubtotalAttribs };
            pc.Permissions = new OptionalString { Value = col.Permissions ?? "" };
            pc.CaptionFormula = new OptionalString { Value = col.CaptionFormula ?? "" };
            pc.CrossAttributes = new OptionalBytes { Value = col.CrossAttributes };
            pullTable.NewColumns.Add(pc);
        }

        // Rows → NewRows
        foreach (var row in pushTable.Rows)
        {
            var pr = new PullRow { Id = row.Id };
            pr.Index = new OptionalInt32 { Value = row.Index };
            pr.Height = new OptionalInt32 { Value = row.Height };
            pr.Visible = new OptionalBool { Value = row.Visible };
            pr.Locker = new OptionalInt64 { Value = row.Locker };
            pr.Role = new OptionalInt32 { Value = row.Role };
            pr.Permissions = new OptionalString { Value = row.Permissions ?? "" };
            pr.Creator = new OptionalInt64 { Value = row.Creator };
            pullTable.NewRows.Add(pr);
        }

        // Cells → NewCells
        foreach (var cell in pushTable.Cells)
        {
            var pc = new PullCell { Id = cell.Id };
            pc.CId = new OptionalInt64 { Value = cell.CId };
            pc.RId = new OptionalInt64 { Value = cell.RId };
            pc.Value = new OptionalBytes { Value = cell.Value };
            pc.Formula = new OptionalString { Value = cell.Formula ?? "" };
            pc.Style = cell.StyleId != null
                ? new NullableInt64 { Value = cell.StyleId.Value }
                : new NullableInt64 { IsNull = true };
            pc.CollectSource = new OptionalString { Value = cell.CollectSource ?? "" };
            pc.HeaderFormula = new OptionalString { Value = cell.HeaderFormula ?? "" };
            pullTable.NewCells.Add(pc);
        }

        // Merges → NewMerges
        foreach (var merge in pushTable.Merges)
        {
            var pm = new PullMerge { Id = merge.Id };
            pm.TopLeft = new OptionalInt64 { Value = merge.TopLeft };
            pm.BottomRight = new OptionalInt64 { Value = merge.BottomRight };
            pullTable.NewMerges.Add(pm);
        }

        // CellStyles（PushCellStyle 无 Action 字段，统一转换）
        foreach (var s in pushTable.CellStyles)
        {
            var pcs = new PullCellStyle { Id = s.Id };
            pcs.FontFamily = new NullableString { Value = s.FontFamily ?? "" };
            pcs.FontSize = new NullableFloat { Value = s.FontSize };
            pcs.ForeColor = new NullableInt32 { Value = s.ForeColor };
            pcs.BackColor = new NullableInt32 { Value = s.BackColor };
            pcs.Align = new NullableInt32 { Value = s.Align };
            pcs.Margin = new NullableInt32 { Value = s.Margin };
            pcs.Bold = new NullableBool { Value = s.Bold };
            pcs.Italic = new NullableBool { Value = s.Italic };
            pcs.Underline = new NullableBool { Value = s.Underline };
            pcs.DataType = new NullableInt32 { Value = s.DataType };
            pcs.Format = new NullableString { Value = s.Format ?? "" };
            pcs.Locker = new NullableInt64 { Value = s.Locker };
            pcs.DefaultValue = new NullableString { Value = s.DefaultValue ?? "" };
            pcs.Comment = new NullableString { Value = s.Comment ?? "" };
            pullTable.CellStyles.Add(pcs);
        }

        return pullTable;
    }

    /// <summary>
    /// 校验 PushTable 子元素 Action 字段在 {1=New, 2=Modify, 3=Delete} 范围内。
    /// PushTable 本身无 Action 字段；Action 位于 Columns/Rows/Cells/Merges/CellAttachments 子元素。
    /// </summary>
    private static void ValidatePushTableActions(PushTable pushTable)
    {
        SyncValidationHelper.ValidateActions(pushTable.Columns, "Column", c => c.Action, c => c.Id);
        SyncValidationHelper.ValidateActions(pushTable.Rows, "Row", r => r.Action, r => r.Id);
        SyncValidationHelper.ValidateActions(pushTable.Cells, "Cell", c => c.Action, c => c.Id);
        SyncValidationHelper.ValidateActions(pushTable.Merges, "Merge", m => m.Action, m => m.Id);
        SyncValidationHelper.ValidateActions(pushTable.CellAttachments, "CellAttachment",
            a => a.Action, a => $"TableId={a.TableId}, CellId={a.CellId}");
    }

    /// <summary>
    /// long 转 Guid（确定性映射）。
    /// </summary>
    private static Guid LongToGuid(long value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
