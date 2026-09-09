﻿using Auditai.DTO;
using AuditApiServer.Data;
using AuditApiServer.Hubs;
using AuditApiServer.Models;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

// 注意：System.Linq（Any 扩展）由隐式 usings（ImplicitUsings）提供

/// <summary>
/// 文档同步业务服务。阶段 6 Task 6.6 实现。
/// 同 TableSyncService 逻辑，负责 PushDocument/PullDocument/RevertDocument 等。
/// </summary>
/// <remarks>
/// 并发与版本号安全修复（Critical/High）：
/// 1. 乐观锁：SavePushDocumentAsync 在 UPDATE Document.Version 时附加 AND Version = @expectedVersion，
///    affected rows = 0 则重试（最多 MaxVersionRetries 次），防止版本号覆盖。
///    V2-H-23 修复：移除原静态 SemaphoreSlim 全局锁（跨所有项目/团队串行化，瓶颈严重），
///    改为与 TableSyncService 一致的乐观锁策略，依赖 UPDATE ... WHERE Version=@expected 保证原子递增。
/// 2. Protobuf 解析 try/catch：解析失败抛 ArgumentException（客户端错误）。
/// 3. Revert 操作写 ChangeType='Revert' 审计记录。
/// 4. 子元素 Action 字段校验：必须在 {1=New, 2=Modify, 3=Delete} 范围内。
/// </remarks>
public class DocumentSyncService
{
    private readonly DocumentRepository _documentRepo;
    private readonly VersionHistoryService _historySvc;
    private readonly ProjectDbManager _projectDbManager;
    private readonly SqliteStorage _db;
    private readonly ILogger<DocumentSyncService> _logger;
    private readonly IHubContext<ChatHub> _hubContext;

    /// <summary>
    /// 乐观锁最大重试次数。
    /// </summary>
    private const int MaxVersionRetries = 5;

    public DocumentSyncService(
        DocumentRepository documentRepo,
        VersionHistoryService historySvc,
        ProjectDbManager projectDbManager,
        SqliteStorage db,
        ILogger<DocumentSyncService> logger,
        IHubContext<ChatHub> hubContext)
    {
        _documentRepo = documentRepo;
        _historySvc = historySvc;
        _projectDbManager = projectDbManager;
        _db = db;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 快速推送：客户端直接 POST PushDocument Protobuf 字节流（calculateSize ≤ 1MB）。
    /// 解析后整体覆盖 ParagraphsData，递增 Version，写入版本快照。
    /// V2-H-23 修复：移除全局 SemaphoreSlim，依赖 SavePushDocumentAsync 内的乐观锁保证版本号安全。
    /// </summary>
    public async Task<object> PushDocumentQuickAsync(byte[] protobufBytes, long userId)
    {
        PushDocument pushDoc;
        try
        {
            pushDoc = PushDocument.Parser.ParseFrom(protobufBytes);
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogWarning(ex, "PushDocument Protobuf 解析失败: UserId={UserId}", userId);
            throw new ArgumentException("无效的 Protobuf 数据: " + ex.Message, ex);
        }

        ValidatePushDocumentActions(pushDoc);

        return await SavePushDocumentAsync(pushDoc, userId);
    }

    /// <summary>
    /// 异步推送：客户端先上传文件，再调用 GET /api/Project/PushDocument?taskId=&projectId=&documentId=&version=。
    /// 从缓存文件读取 PushDocument Protobuf 字节流，调用 SavePushDocumentAsync 写入项目 .db。
    /// V2-H-23 修复：移除全局 SemaphoreSlim，依赖 SavePushDocumentAsync 内的乐观锁保证版本号安全。
    /// </summary>
    public async Task<object> PushDocumentAsync(long taskId, Guid projectId, Guid documentId, int version, long userId, Stream cacheStream)
    {
        using var ms = new MemoryStream();
        await cacheStream.CopyToAsync(ms);
        var bytes = ms.ToArray();

        PushDocument pushDoc;
        try
        {
            pushDoc = PushDocument.Parser.ParseFrom(bytes);
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogWarning(ex, "PushDocumentAsync Protobuf 解析失败: TaskId={TaskId}", taskId);
            throw new ArgumentException("无效的 Protobuf 数据: " + ex.Message, ex);
        }

        ValidatePushDocumentActions(pushDoc);

        var result = await SavePushDocumentAsync(pushDoc, userId);
        _logger.LogInformation("PushDocument 异步任务完成: TaskId={TaskId} DocumentId={DocumentId}", taskId, documentId);
        return result;
    }

    /// <summary>
    /// 拉取文档：客户端 POST { projectId, documentId, version } JSON。
    /// 返回 PullDocument Protobuf 字节流。从项目 .db 的 Document/Paragraph 表读取。
    /// V2-C-02 修复：读取 Document.Version 实现版本比对，clientVersion >= serverVersion 时返回空响应。
    /// </summary>
    public async Task<(byte[] bytes, bool isEmpty)> PullDocumentAsync(Guid projectId, Guid documentId, int clientVersion)
    {
        var longDocumentId = BitConverter.ToInt64(documentId.ToByteArray(), 0);
        var teamId = await GetTeamIdAsync(projectId);

        using var conn = _projectDbManager.OpenProjectDb(teamId, projectId);

        // 检查 Document 是否存在并读取 Version
        int version = 0;
        using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT Version FROM `Document` WHERE Id=@id";
            checkCmd.Parameters.AddWithValue("@id", longDocumentId);
            var verResult = await checkCmd.ExecuteScalarAsync();
            if (verResult == null || verResult == DBNull.Value)
            {
                // 与客户端 Syncer.Pull(Document) 对齐：文档不存在返回 "NotExist"
                var empty = new PullDocument { Result = "NotExist", Version = 0 };
                return (empty.ToByteArray(), true);
            }
            version = Convert.ToInt32(verResult);
        }

        // 版本比对：客户端版本等于服务端版本，返回空响应表示无更新（与 PullTable 一致使用 "Latest"）
        if (clientVersion == version)
        {
            var noChange = new PullDocument { Result = "Latest", Version = version };
            return (noChange.ToByteArray(), true);
        }

        // 版本不一致：返回完整文档数据，客户端据此 Merge（与 PullTable 一致使用 "NeedUpdate"）
        var pullDoc = new PullDocument { Result = "NeedUpdate", Version = version };

        // 表级字段
        using (var docCmd = conn.CreateCommand())
        {
            docCmd.CommandText = "SELECT `Locker`,`SectPr`,`MergeTable` FROM `Document` WHERE Id=@id";
            docCmd.Parameters.AddWithValue("@id", longDocumentId);
            using var reader = await docCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                pullDoc.Locker = new OptionalInt64 { Value = reader.IsDBNull(0) ? 0 : reader.GetInt64(0) };
                pullDoc.SectPr = new OptionalString { Value = reader.IsDBNull(1) ? "" : reader.GetString(1) };
                pullDoc.MergeTable = new OptionalInt64 { Value = reader.IsDBNull(2) ? 0 : reader.GetInt64(2) };
            }
        }

        // Paragraphs：全部放入 NewParagraphs（返回完整文档）
        using (var paraCmd = conn.CreateCommand())
        {
            paraCmd.CommandText = "SELECT `Id`,`Index`,`Stream`,`Section`,`Comment` FROM `Paragraph` WHERE DocumentId=@did ORDER BY `Index`";
            paraCmd.Parameters.AddWithValue("@did", longDocumentId);
            using var reader = await paraCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                pullDoc.NewParagraphs.Add(ReadPullParagraph(reader));
        }

        return (pullDoc.ToByteArray(), false);
    }

    /// <summary>
    /// 回滚文档到指定版本：从 VersionHistory 读取快照，解析为 PushDocument，
    /// 复用 SavePushDocumentAsync 写回项目 .db 各表，递增 Document.Version。
    /// 返回 { taskId, url } 供客户端等待任务并下载结果。
    /// V2-C-01 修复：改为操作项目 .db 而非主库 Documents。
    /// </summary>
    public async Task<object> RevertDocumentAsync(Guid projectId, Guid documentId, int targetVersion, long userId, TaskService taskSvc)
    {
        var snapshot = await _historySvc.GetSnapshotAsync(projectId, documentId, "Document", targetVersion);
        if (snapshot == null)
            return new { error = $"版本 {targetVersion} 的快照不存在" };

        if (snapshot.Snapshot == null || snapshot.Snapshot.Length == 0)
            return new { error = $"版本 {targetVersion} 的快照数据为空" };

        var taskId = await taskSvc.GenerateTaskIdAsync(userId, "RevertDocument");
        var url = $"/api/ServerTask/DownloadTaskResult?taskId={taskId}";

        taskSvc.StartBackgroundTask(taskId, async (progress, ct) =>
        {
            progress.Report(0.1, "正在加载快照");

            // V2-H-23 修复：移除全局 _pushLock，依赖 SavePushDocumentAsync 内的乐观锁保证版本号安全
            // 快照存储的是 PushDocument 序列化字节，解析为 PushDocument
            PushDocument pushDoc;
            try
            {
                pushDoc = PushDocument.Parser.ParseFrom(snapshot.Snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RevertDocument 快照解析失败: DocumentId={DocumentId} Version={Version}", documentId, targetVersion);
                throw new InvalidOperationException($"快照解析失败: {ex.Message}", ex);
            }

            // 旧版增量快照（含 Modify/Delete 动作的段落）重放到当前状态会产生混合损坏数据，拒绝回滚
            if (pushDoc.Paragraphs.Any(p => p.Action == 2 || p.Action == 3))
            {
                _logger.LogWarning(
                    "RevertDocument 拒绝：版本 {Version} 的快照为旧版增量格式，无法回滚。DocumentId={DocumentId}",
                    targetVersion, documentId);
                throw new InvalidOperationException($"版本 {targetVersion} 的快照为旧版增量格式，无法安全回滚（该版本产生于快照机制升级前）");
            }

            progress.Report(0.5, "正在覆盖数据");

            // Version=0 跳过乐观锁版本校验（快照中的 Version 是历史值，必然落后于当前服务端版本）
            pushDoc.Version = 0;

            // 整体覆盖模式写回项目 .db（先清空现有段落后按全量快照重建），递增 Document.Version
            // SavePushDocumentAsync 内部已处理版本快照写入（changeType="Revert"）
            await SavePushDocumentAsync(pushDoc, userId, "Revert", replaceExisting: true);

            _logger.LogInformation(
                "RevertDocument 完成: DocumentId={DocumentId} TargetVersion={Target} UserId={UserId}",
                documentId, targetVersion, userId);
            progress.Report(1.0, "完成");
        });

        return new { taskId, url };
    }

    /// <summary>
    /// 获取回滚前后差异（PullDocument 字节流）。
    /// V2-C-03 修复：快照存储的是 PushDocument 字节，用 PushDocument 解析后转换为 PullDocument 返回。
    /// </summary>
    public async Task<(byte[] bytes, long taskId, string url)> GetDocumentRevertDiffAsync(Guid projectId, Guid documentId, int targetVersion, long userId, TaskService taskSvc)
    {
        var taskId = await taskSvc.GenerateTaskIdAsync(userId, "GetDocumentRevertDiff");
        var url = $"/api/ServerTask/DownloadTaskResult?taskId={taskId}";

        var snapshot = await _historySvc.GetSnapshotAsync(projectId, documentId, "Document", targetVersion);
        // 无快照时返回 "Latest"（无差异可预览）；有快照时由 ConvertPushDocumentToPullDocument 返回 "NeedUpdate"
        var pullDoc = new PullDocument { Result = "Latest", Version = targetVersion };
        if (snapshot?.Snapshot != null)
        {
            try
            {
                // 快照存储的是 PushDocument 序列化字节，需用 PushDocument 解析后转换为 PullDocument
                var pushDoc = PushDocument.Parser.ParseFrom(snapshot.Snapshot);
                if (pushDoc.Paragraphs.Any(p => p.Action == 2 || p.Action == 3))
                {
                    // 旧版增量快照无法转换为全量差异预览（会误导客户端按全量 Merge 损坏本地文档），
                    // 返回 "Latest" 让客户端保持当前状态
                    _logger.LogWarning(
                        "GetDocumentRevertDiff：版本 {Version} 的快照为旧版增量格式，无法预览差异。DocumentId={DocumentId}",
                        targetVersion, documentId);
                }
                else
                {
                    pullDoc = ConvertPushDocumentToPullDocument(pushDoc, targetVersion);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "快照解析失败: DocumentId={DocumentId} Version={Version}", documentId, targetVersion);
            }
        }

        taskSvc.StartBackgroundTask(taskId, (progress, ct) =>
        {
            progress.Report(1.0, "完成");
            return Task.CompletedTask;
        });

        return (pullDoc.ToByteArray(), taskId, url);
    }

    /// <summary>
    /// 获取文档版本时间线。
    /// </summary>
    public async Task<List<object>> GetDocumentTimelineAsync(Guid projectId, Guid documentId)
    {
        var timeline = await _historySvc.ListTimelineAsync(projectId, documentId, "Document");
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
    /// 查询文档版本列表。
    /// </summary>
    public async Task<List<object>> QueryDocumentVersionsAsync(Guid projectId, Guid documentId)
    {
        var versions = await _historySvc.ListVersionsAsync(projectId, documentId, "Document");
        return versions.Select(v => (object)new
        {
            version = v.Version,
            changeType = v.ChangeType,
            createdAt = v.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// 内部：保存 PushDocument 到项目 .db（Document/Paragraph）+ 写入版本快照。
    /// V2-H-23 修复：使用乐观锁重试保证 Document.Version 原子递增，替代原全局 SemaphoreSlim。
    /// 所有写操作在单个事务内执行；UPDATE Document.Version 时附加 AND Version=@expected，
    /// affected=0 则回滚重试。changeType: "Push"（默认）或 "Revert"。
    /// </summary>
    /// <param name="replaceExisting">
    /// 整体覆盖模式（仅 Revert 使用）：先清空该文档现有段落再应用快照内容，
    /// 保证回滚结果是完整的快照状态而非"旧增量重放到最新状态"的混合数据。
    /// </param>
    private async Task<object> SavePushDocumentAsync(PushDocument pushDoc, long userId, string changeType = "Push", bool replaceExisting = false)
    {
        var projectId = new Guid(pushDoc.ProjectId.ToByteArray());
        var documentId = pushDoc.Id; // long，项目 .db 中 Document.Id 为 long
        var teamId = await GetTeamIdAsync(projectId);

        using var conn = _projectDbManager.OpenProjectDb(teamId, projectId);

        for (var attempt = 0; attempt < MaxVersionRetries; attempt++)
        {
            // 0. 读取当前 Document.Version（V2-C-02 修复）
            int expectedVersion;
            bool hasDocument;
            using (var readCmd = conn.CreateCommand())
            {
                readCmd.CommandText = "SELECT Version FROM `Document` WHERE Id=@id";
                readCmd.Parameters.AddWithValue("@id", documentId);
                var verResult = await readCmd.ExecuteScalarAsync();
                if (verResult == null || verResult == DBNull.Value)
                {
                    hasDocument = false;
                    expectedVersion = 0;
                }
                else
                {
                    hasDocument = true;
                    expectedVersion = Convert.ToInt32(verResult);
                }
            }
            int newVersion = expectedVersion + 1;

            // 乐观锁检查：客户端版本落后于服务端说明他人已推送过，拒绝本次 Push 并返回最新版本，
            // 客户端 PullAndRetryPush 会 Pull 合并后重试。clientVersion=0（新文档首次推送）跳过校验。
            // 修复：原实现完全忽略客户端版本，多人并发编辑时静默后写覆盖（丢失先写者修改）。
            if (!replaceExisting && pushDoc.Version > 0 && pushDoc.Version < expectedVersion)
            {
                _logger.LogWarning(
                    "PushDocument 拒绝：版本落后（客户端={ClientVersion} 服务端={ServerVersion}）DocumentId={DocumentId} UserId={UserId}",
                    pushDoc.Version, expectedVersion, documentId, userId);
                return new { Result = "OutOfDate", Version = expectedVersion };
            }

            using var transaction = conn.BeginTransaction();
            try
            {
                // 回滚（整体覆盖模式）：保留当前 Locker（快照中的 Locker 是历史值），清空现有段落后整体重建
                if (replaceExisting)
                {
                    using var lockerCmd = conn.CreateCommand();
                    lockerCmd.Transaction = transaction;
                    lockerCmd.CommandText = "SELECT `Locker` FROM `Document` WHERE Id=@id";
                    lockerCmd.Parameters.AddWithValue("@id", documentId);
                    var lockerResult = await lockerCmd.ExecuteScalarAsync();
                    if (lockerResult != null && lockerResult != DBNull.Value)
                        pushDoc.Locker = Convert.ToInt64(lockerResult);

                    using var clearCmd = conn.CreateCommand();
                    clearCmd.Transaction = transaction;
                    clearCmd.CommandText = "DELETE FROM `Paragraph` WHERE DocumentId=@did";
                    clearCmd.Parameters.AddWithValue("@did", documentId);
                    await clearCmd.ExecuteNonQueryAsync();
                }

                // 1. 写入/更新 Document 表级字段（V2-H-23：使用 ON CONFLICT 不重置 Version 列）
                await UpsertDocumentAsync(conn, transaction, pushDoc);

                // 1b. 乐观锁更新 Document.Version：
                //     - 已有文档：UPDATE WHERE Version=@expected，并发冲突时 affected=0 触发重试
                //     - 新文档：INSERT 时 Version 默认为 0（=expectedVersion），直接 UPDATE 设置为新版本号
                int affected;
                using (var verCmd = conn.CreateCommand())
                {
                    verCmd.Transaction = transaction;
                    if (hasDocument)
                    {
                        verCmd.CommandText = "UPDATE `Document` SET Version=@version WHERE Id=@id AND Version=@expected";
                        verCmd.Parameters.AddWithValue("@expected", expectedVersion);
                    }
                    else
                    {
                        verCmd.CommandText = "UPDATE `Document` SET Version=@version WHERE Id=@id";
                    }
                    verCmd.Parameters.AddWithValue("@version", newVersion);
                    verCmd.Parameters.AddWithValue("@id", documentId);
                    affected = await verCmd.ExecuteNonQueryAsync();
                }

                if (affected == 0)
                {
                    transaction.Rollback();
                    _logger.LogWarning("PushDocument 乐观锁冲突，重试: DocumentId={DocumentId} Attempt={Attempt}",
                        documentId, attempt + 1);
                    continue;
                }

                // 2. Paragraphs：Action=1/2 upsert，Action=3 delete
                foreach (var para in pushDoc.Paragraphs)
                {
                    if (para.Action == 3)
                        await DeleteEntityAsync(conn, transaction, "`Paragraph`", para.Id);
                    else
                        await UpsertParagraphAsync(conn, transaction, documentId, para);
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

                // 使用递增后的版本号写入快照（V2-C-02 修复：不再硬编码 0）
                // 全量快照：提交后从数据库重建文档完整状态再存入 VersionHistory。
                // 修复：原实现直接存客户端发来的增量 PushDocument，回滚时把旧增量重放到最新状态会产生混合损坏数据。
                var guidDocumentId = LongToGuid(documentId);
                byte[] fullSnapshot = await BuildFullDocumentSnapshotAsync(conn, documentId, projectId);
                await _historySvc.CreateSnapshotAsync(projectId, guidDocumentId, "Document", newVersion, changeType, fullSnapshot, userId);
                _logger.LogInformation("PushDocument 完成: DocumentId={DocumentId} Paragraphs={ParagraphCount} Version={Version} Attempt={Attempt}",
                    documentId, pushDoc.Paragraphs.Count, newVersion, attempt + 1);

                // Broadcast PeerDocumentChanged to all clients in the project group
                var projectIdStr = projectId.ToString();
                var groupName = $"project_{projectIdStr}";
                try
                {
                    await _hubContext.Clients.Group(groupName).SendAsync("PeerDocumentChanged",
                        projectIdStr, documentId.ToString(), newVersion.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "PeerDocumentChanged 广播失败: ProjectId={Pid} DocumentId={Did}", projectIdStr, documentId);
                }

                return new { Result = "Success", Version = newVersion };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        throw new InvalidOperationException($"PushDocument 版本号冲突，重试 {MaxVersionRetries} 次后失败");
    }

    /// <summary>
    /// 从项目 .db 重建该文档完整状态，序列化为全量 PushDocument（Mask=-1，段落全部 Action=1）作为版本快照。
    /// 该快照可直接经 SavePushDocumentAsync(replaceExisting:true) 应用，实现正确的版本回滚。
    /// </summary>
    private static async Task<byte[]> BuildFullDocumentSnapshotAsync(SqliteConnection conn, long documentId, Guid projectId)
    {
        var pd = new PushDocument
        {
            Id = documentId,
            ProjectId = ByteString.CopyFrom(projectId.ToByteArray()),
            Version = 0,
            Mask = -1
        };

        // 文档表级字段
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT `Locker`,`SectPr`,`MergeTable` FROM `Document` WHERE Id=@id";
            cmd.Parameters.AddWithValue("@id", documentId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                pd.Locker = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                pd.SectPr = reader.IsDBNull(1) ? "" : reader.GetString(1);
                pd.MergeTable = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
            }
        }

        // 段落（全量 Action=1）
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT `Id`,`Index`,`Stream`,`Section`,`Comment` FROM `Paragraph` WHERE DocumentId=@did ORDER BY `Index`";
            cmd.Parameters.AddWithValue("@did", documentId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pd.Paragraphs.Add(new PushParagraph
                {
                    Id = reader.GetInt64(0),
                    Action = 1,
                    Mask = -1,
                    Index = reader.GetInt32(1),
                    Stream = reader.IsDBNull(2) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[2]),
                    Section = reader.IsDBNull(3) ? null : new BytesValue { Value = ByteString.CopyFrom((byte[])reader[3]) },
                    Comment = reader.IsDBNull(4) ? "" : reader.GetString(4)
                });
            }
        }

        return pd.ToByteArray();
    }

    /// <summary>
    /// 查询项目所属团队 Id（从主库 Projects 表读取）。
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
            throw new InvalidOperationException($"项目 {projectId} 不存在或未关联团队");
        return Guid.Parse(teamIdStr);
    }

    /// <summary>
    /// 写入/更新 Document 表级字段。
    /// V2-M-06 修复：Mod 操作（Mask != -1）改用动态 UPDATE 只更新 dirty 字段，
    /// 避免 ON CONFLICT DO UPDATE 用 Protobuf 默认值覆盖未修改字段导致数据丢失。
    /// New 操作（Mask == -1）保持原 INSERT ... ON CONFLICT DO UPDATE 整体覆盖逻辑。
    /// Mask 位定义参考 Auditai.DTO.DocumentDirtyMask。
    /// </summary>
    private static async Task UpsertDocumentAsync(SqliteConnection conn, SqliteTransaction tx, PushDocument d)
    {
        if (d.Mask != -1)
        {
            if (d.Mask == 0) return; // 表级字段无修改（仅段落 dirty）
            var setClauses = new List<string>();
            using var modCmd = conn.CreateCommand();
            modCmd.Transaction = tx;
            int mask = d.Mask;
            // DocumentDirtyMask 位定义
            if ((mask & 1) != 0) { setClauses.Add("`Locker`=@Locker"); modCmd.Parameters.AddWithValue("@Locker", d.Locker); }
            if ((mask & 2) != 0) { setClauses.Add("`SectPr`=@SectPr"); modCmd.Parameters.AddWithValue("@SectPr", (object?)d.SectPr ?? DBNull.Value); }
            if ((mask & 4) != 0) { setClauses.Add("`MergeTable`=@MergeTable"); modCmd.Parameters.AddWithValue("@MergeTable", d.MergeTable); }
            if (setClauses.Count == 0) return;
            modCmd.CommandText = $"UPDATE `Document` SET {string.Join(", ", setClauses)} WHERE Id=@Id";
            modCmd.Parameters.AddWithValue("@Id", d.Id);
            await modCmd.ExecuteNonQueryAsync();
            return;
        }

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // V2-H-23 修复：原 INSERT OR REPLACE 会重置 Version 为默认值 0，破坏乐观锁的版本号比对。
        // 改为 INSERT ... ON CONFLICT DO UPDATE，仅更新业务字段，保留 Version 列由后续 UPDATE 显式设置。
        cmd.CommandText = @"INSERT INTO `Document`(`Id`,`Locker`,`SectPr`,`MergeTable`,`Dirty`)
VALUES (@Id,@Locker,@SectPr,@MergeTable,0)
ON CONFLICT(`Id`) DO UPDATE SET
    `Locker`=excluded.`Locker`,
    `SectPr`=excluded.`SectPr`,
    `MergeTable`=excluded.`MergeTable`,
    `Dirty`=0";
        cmd.Parameters.AddWithValue("@Id", d.Id);
        cmd.Parameters.AddWithValue("@Locker", d.Locker);
        cmd.Parameters.AddWithValue("@SectPr", (object?)d.SectPr ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MergeTable", d.MergeTable);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// 写入/更新 Paragraph。
    /// V2-M-06 修复：Mod 操作（Action=2 且 Mask!=-1）改用动态 UPDATE 只更新 dirty 字段。
    /// New 操作（Action=1 或 Mask=-1）保持原 INSERT OR REPLACE 整体覆盖逻辑。
    /// Mask 位定义参考 Auditai.DTO.ParagraphDirtyMask。
    /// </summary>
    private static async Task UpsertParagraphAsync(SqliteConnection conn, SqliteTransaction tx, long documentId, PushParagraph p)
    {
        if (p.Action == 2 && p.Mask != -1)
        {
            if (p.Mask == 0) return;
            var setClauses = new List<string>();
            using var modCmd = conn.CreateCommand();
            modCmd.Transaction = tx;
            int mask = p.Mask;
            // ParagraphDirtyMask 位定义
            if ((mask & 1) != 0) { setClauses.Add("`Stream`=@Stream"); modCmd.Parameters.AddWithValue("@Stream", p.Stream.ToByteArray()); }
            if ((mask & 2) != 0) { setClauses.Add("`Comment`=@Comment"); modCmd.Parameters.AddWithValue("@Comment", (object?)p.Comment ?? DBNull.Value); }
            if ((mask & 4) != 0) { setClauses.Add("`Index`=@Index"); modCmd.Parameters.AddWithValue("@Index", p.Index); }
            if (setClauses.Count == 0) return;
            modCmd.CommandText = $"UPDATE `Paragraph` SET {string.Join(", ", setClauses)} WHERE Id=@Id";
            modCmd.Parameters.AddWithValue("@Id", p.Id);
            await modCmd.ExecuteNonQueryAsync();
            return;
        }

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"INSERT OR REPLACE INTO `Paragraph`(
`Id`,`DocumentId`,`Index`,`Stream`,`ServerIndex`,`Dirty`,`Status`,`Section`,`Comment`)
VALUES (@Id,@DocumentId,@Index,@Stream,@ServerIndex,@Dirty,@Status,@Section,@Comment)";
        cmd.Parameters.AddWithValue("@Id", p.Id);
        cmd.Parameters.AddWithValue("@DocumentId", documentId);
        cmd.Parameters.AddWithValue("@Index", p.Index);
        cmd.Parameters.AddWithValue("@Stream", p.Stream.ToByteArray());
        cmd.Parameters.AddWithValue("@ServerIndex", 0);
        cmd.Parameters.AddWithValue("@Dirty", 0);
        cmd.Parameters.AddWithValue("@Status", 0);
        cmd.Parameters.AddWithValue("@Section", p.Section != null ? (object)p.Section.Value.ToByteArray() : DBNull.Value);
        cmd.Parameters.AddWithValue("@Comment", (object?)p.Comment ?? DBNull.Value);
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

    private static PullParagraph ReadPullParagraph(SqliteDataReader reader)
    {
        var pp = new PullParagraph { Id = reader.GetInt64(0) };
        pp.Index = new OptionalInt32 { Value = reader.GetInt32(1) };
        pp.Stream = new OptionalBytes { Value = reader.IsDBNull(2) ? ByteString.Empty : ByteString.CopyFrom((byte[])reader[2]) };
        pp.Section = reader.IsDBNull(3) ? new NullableBytes { IsNull = true } : new NullableBytes { Value = ByteString.CopyFrom((byte[])reader[3]) };
        pp.Comment = new OptionalString { Value = reader.IsDBNull(4) ? "" : reader.GetString(4) };
        return pp;
    }

    /// <summary>
    /// 将 PushDocument 转换为 PullDocument 格式（V2-C-03 修复）。
    /// 快照存储的是 PushDocument 序列化字节，差异预览需转换为 PullDocument 返回客户端。
    /// 转换逻辑参考 PullDocumentAsync 方法中从项目 .db 读取数据构建 PullDocument 的代码。
    /// 所有 Push Paragraphs 放入 PullDocument 的 NewParagraphs 集合（表示完整状态而非增量）。
    /// </summary>
    private static PullDocument ConvertPushDocumentToPullDocument(PushDocument pushDoc, int version)
    {
        // 转换结果包含完整数据，客户端据此 Merge；与 PullDocumentAsync 数据分支一致使用 "NeedUpdate"
        var pullDoc = new PullDocument { Result = "NeedUpdate", Version = version };

        // 表级字段
        pullDoc.Locker = new OptionalInt64 { Value = pushDoc.Locker };
        pullDoc.SectPr = new OptionalString { Value = pushDoc.SectPr ?? "" };
        pullDoc.MergeTable = new OptionalInt64 { Value = pushDoc.MergeTable };

        // Paragraphs → NewParagraphs
        foreach (var p in pushDoc.Paragraphs)
        {
            var pp = new PullParagraph { Id = p.Id };
            pp.Index = new OptionalInt32 { Value = p.Index };
            pp.Stream = new OptionalBytes { Value = p.Stream };
            pp.Section = p.Section != null
                ? new NullableBytes { Value = p.Section.Value }
                : new NullableBytes { IsNull = true };
            pp.Comment = new OptionalString { Value = p.Comment ?? "" };
            pullDoc.NewParagraphs.Add(pp);
        }

        return pullDoc;
    }

    /// <summary>
    /// 校验 PushDocument 子元素 Action 字段在 {1=New, 2=Modify, 3=Delete} 范围内。
    /// PushDocument 本身无 Action 字段；Action 位于 Paragraphs 子元素。
    /// </summary>
    private static void ValidatePushDocumentActions(PushDocument pushDoc)
    {
        SyncValidationHelper.ValidateActions(pushDoc.Paragraphs, "Paragraph", p => p.Action, p => p.Id);
    }

    private static Guid LongToGuid(long value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
