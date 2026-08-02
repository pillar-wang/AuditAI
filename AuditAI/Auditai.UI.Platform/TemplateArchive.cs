using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using Newtonsoft.Json;
using Auditai.DTO;

namespace Auditai.UI.Platform;

/// <summary>模板归档元信息（.auditaitemplate 文件中的 metadata.json）</summary>
public class TemplateArchiveMetadata
{
    [JsonProperty("templateName")]
    public string TemplateName { get; set; } = "";

    [JsonProperty("templateNumber")]
    public string TemplateNumber { get; set; } = "";

    [JsonProperty("category")]
    public string Category { get; set; } = "";

    [JsonProperty("note")]
    public string Note { get; set; } = "";

    [JsonProperty("createTime")]
    public DateTime CreateTime { get; set; }

    [JsonProperty("exportTime")]
    public DateTime ExportTime { get; set; }

    [JsonProperty("exportedBy")]
    public string ExportedBy { get; set; } = "";

    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonProperty("appVersion")]
    public string AppVersion { get; set; } = "";

    /// <summary>导出时模板所属团队 Id（跨团队导入追溯用，可为空）</summary>
    [JsonProperty("sourceTeamId")]
    public string SourceTeamId { get; set; } = "";
}

/// <summary>
/// 模板归档解压后的中间上下文：
/// 持有临时 .db 路径、临时 FileCache 文件列表、元信息和 .db 中的 Project DTO，
/// 供 UI 在 dlgTemplateEditor 编辑后再调用 StorageRouter.ImportTemplate 完成注册。
/// </summary>
public class TemplateImportContext : IDisposable
{
    public Guid NewTemplateId;
    public string TempDbPath;
    public List<string> TempFileCacheFiles;
    public TemplateArchiveMetadata Metadata;
    /// <summary>.db 中的 Project 记录，用于预填 dlgTemplateEditor</summary>
    public Auditai.DTO.Project ProjectDto;
    public string TempDir;

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (!string.IsNullOrEmpty(TempDir) && Directory.Exists(TempDir))
        {
            try { Directory.Delete(TempDir, recursive: true); } catch { /* 清理失败忽略 */ }
        }
    }
}

/// <summary>模板归档读写核心逻辑（.auditaitemplate 格式）</summary>
public static class TemplateArchive
{
    /// <summary>当前软件支持的最高模板数据库 Schema 版本（与 ProjectArchive 保持一致）</summary>
    private const int CurrentSchemaVersion = 44;

    private const string AppVersion = "1.0.0";

    private const string EntryTemplateDb = "template.db";
    private const string EntryMetadata = "metadata.json";
    /// <summary>.auditaitemplate 中 FileCache 条目前缀：解压后为 FileCache/{fileId}.bin</summary>
    private const string EntryFileCachePrefix = "FileCache/";

    /// <summary>老版本 ProjectDAL 迁移时给 CreateTime 设定的占位默认值；导出/导入时应视为"未设置"。</summary>
    private static readonly DateTime PlaceholderCreateTime = new DateTime(2000, 1, 1);

    /// <summary>判断一个 DateTime 是否为"未赋值"的占位值（default 或 2000-01-01）。</summary>
    private static bool IsPlaceholderCreateTime(DateTime t)
    {
        if (t == default) return true;
        // 2000-01-01 00:00:00 到 2000-01-02 之间都视为占位（兼容不同精度写入）
        return t >= PlaceholderCreateTime && t < PlaceholderCreateTime.AddDays(1);
    }

    /// <summary>仅读取归档文件中的元信息（不解压 .db），用于导入前预览</summary>
    public static TemplateArchiveMetadata ReadMetadata(string archivePath)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException("模板归档文件不存在", archivePath);

        using (var archive = ZipFile.OpenRead(archivePath))
        {
            var entry = archive.GetEntry(EntryMetadata);
            if (entry == null)
                throw new InvalidDataException("无效的模板归档文件：缺少 metadata.json");

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                var metadata = JsonConvert.DeserializeObject<TemplateArchiveMetadata>(json);
                if (metadata == null)
                    throw new InvalidDataException("metadata.json 解析失败");
                return metadata;
            }
        }
    }

    /// <summary>导出模板为 .auditaitemplate 归档文件</summary>
    /// <param name="templateInfo">主数据库中的模板 DTO（提供名称、编号、类别等元信息）</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <remarks>
    /// 调用方需在服务端模式下先调用 <see cref="MainForm.OpenProjectDb_DownloadIfNotExist"/> 下载 .db 到本地缓存，
    /// 否则本地无 .db 文件可供打包。
    /// </remarks>
    public static void Export(Auditai.DTO.Project templateInfo, string outputPath)
    {
        Guid templateId = templateInfo.Id;

        // 获取模板 .db 路径：
        // - 本地模式：Data\Templates\{templateId}.db（通过 LocalDataStore.GetTemplateDbPath 查找）
        // - 服务端模式：调用方应已通过 OpenProjectDb_DownloadIfNotExist 下载到 GetDbPathByGuid 路径
        string dbPath;
        if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
        {
            dbPath = Auditai.LocalDataStore.LocalDataStore.GetTemplateDbPath(templateId);
            if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
            {
                throw new FileNotFoundException("本地模板数据库文件不存在，模板 Id: " + templateId);
            }
        }
        else
        {
            dbPath = MainForm.GetDbPathByGuid(templateId);
            if (!File.Exists(dbPath))
                throw new FileNotFoundException("模板数据库文件不存在: " + dbPath, dbPath);
        }

        // 从模板 .db 中读取补充信息（CreateTime 等）
        // using：确保 Dispose 释放 EXCLUSIVE 锁并执行 CheckpointAndClose，
        // 避免后续 CreateEntryFromFile(dbPath) 读取 .db 时因锁占用或 WAL 未合并导致数据不完整
        Auditai.DTO.Project dbProject = null;
        try
        {
            using (var dal = new ProjectDAL(dbPath))
            {
                dbProject = dal.GetProject();
            }
        }
        catch { /* 读取失败时使用主数据库的信息 */ }

        var metadata = new TemplateArchiveMetadata
        {
            TemplateName = templateInfo.Name ?? dbProject?.Name ?? "",
            TemplateNumber = templateInfo.Number ?? dbProject?.Number ?? "",
            Category = templateInfo.Category ?? dbProject?.Category ?? "",
            Note = templateInfo.Note ?? dbProject?.Note ?? "",
            CreateTime =
                !IsPlaceholderCreateTime(templateInfo.CreateTime) ? templateInfo.CreateTime :
                !IsPlaceholderCreateTime(dbProject?.CreateTime ?? default) ? dbProject.CreateTime :
                DateTime.Now,
            ExportTime = DateTime.Now,
            ExportedBy = Auditai.Model.User.Current?.Name ?? Auditai.Model.User.Current?.UserName ?? "",
            SchemaVersion = CurrentSchemaVersion,
            AppVersion = AppVersion,
            SourceTeamId = Auditai.Model.UserTeam.Current?.Id.ToString() ?? ""
        };

        // 收集该模板所有需要打包的 FileId（Pdf / Image / 单元格附件）
        HashSet<Guid> fileIds;
        try { fileIds = CollectFileIdsFromDb(dbPath); }
        catch { fileIds = new HashSet<Guid>(); }

        // 本地 FileCache 目录：data/{userId}/{templateId}/FileCache
        long userId = Auditai.Model.User.Current?.Id ?? 0;
        string fileCacheDir = Path.Combine("data", userId.ToString(), templateId.ToString(), "FileCache");
        bool fileCacheDirExists = Directory.Exists(fileCacheDir);

        // 写入临时文件，成功后再移动到目标路径（避免产生不完整的导出文件）
        string tempPath = outputPath + ".tmp";
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            using (var archive = ZipFile.Open(tempPath, ZipArchiveMode.Create))
            {
                // 添加 template.db（最优压缩）
                archive.CreateEntryFromFile(dbPath, EntryTemplateDb, CompressionLevel.Optimal);

                // 添加 metadata.json
                var metaEntry = archive.CreateEntry(EntryMetadata, CompressionLevel.Optimal);
                using (var stream = metaEntry.Open())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(JsonConvert.SerializeObject(metadata, Formatting.Indented));
                }

                // 添加 FileCache/*.bin（PDF / 图片 / 单元格附件的二进制内容）
                // ——缺少的文件（例如还未从云端下载到本地的）自动跳过，不影响归档完整性
                foreach (Guid fid in fileIds)
                {
                    string srcPath = fileCacheDirExists ? Path.Combine(fileCacheDir, $"{fid}.bin") : null;
                    if (string.IsNullOrEmpty(srcPath) || !File.Exists(srcPath))
                        continue;
                    archive.CreateEntryFromFile(srcPath, EntryFileCachePrefix + $"{fid}.bin", CompressionLevel.Optimal);
                }
            }

            // 原子移动
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            File.Move(tempPath, outputPath);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
            throw;
        }
    }

    /// <summary>
    /// 解压 .auditaitemplate 归档到临时目录，返回中间上下文。
    /// 不执行实际注册（不写入 Data\Templates 也不上传服务端），
    /// 由 UI 在 dlgTemplateEditor 编辑模板信息后再调用 <see cref="ApplyEditorResult"/> + <see cref="Auditai.LocalDataStore.StorageRouter.ImportTemplate"/>。
    /// </summary>
    public static async Task<TemplateImportContext> ExtractAsync(string archivePath)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException("模板归档文件不存在", archivePath);

        // 1. 读取并验证元信息
        var metadata = ReadMetadata(archivePath);
        if (metadata.SchemaVersion > CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"模板文件版本过高（v{metadata.SchemaVersion}），当前软件支持的最高版本为 v{CurrentSchemaVersion}，请升级软件后导入。");
        }

        // 2. 解压 .db 到临时目录
        Guid newTemplateId = Guid.NewGuid();
        string tempDir = Path.Combine(Path.GetTempPath(), "auditai_template_import_" + newTemplateId.ToString("N"));
        Directory.CreateDirectory(tempDir);
        string tempFileCacheDir = Path.Combine(tempDir, "FileCache");

        var ctx = new TemplateImportContext
        {
            NewTemplateId = newTemplateId,
            TempDir = tempDir,
            Metadata = metadata,
            TempFileCacheFiles = new List<string>()
        };

        try
        {
            string tempDbPath = Path.Combine(tempDir, EntryTemplateDb);

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var dbEntry = archive.GetEntry(EntryTemplateDb);
                if (dbEntry == null)
                    throw new InvalidDataException("无效的模板归档文件：缺少 template.db");

                dbEntry.ExtractToFile(tempDbPath, overwrite: true);

                // 同时解压 FileCache 中的所有条目到 tempFileCacheDir
                string safeFileCacheRoot = Path.GetFullPath(tempFileCacheDir) + Path.DirectorySeparatorChar;
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.FullName) || !entry.FullName.StartsWith(EntryFileCachePrefix, StringComparison.Ordinal))
                        continue;
                    // ===== SCS0018 多层防御 =====
                    // 第一层：仅取文件名部分，丢弃 entry.FullName 里的任何目录片段（防御 ../../etc/passwd 式构造）
                    string fileName = Path.GetFileName(entry.FullName);
                    // 第二层：非空 + 非法字符/分隔符/相对路径段黑名单
                    if (string.IsNullOrEmpty(fileName) ||
                        fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                        fileName.Contains(".."))
                    {
                        throw new InvalidDataException($"模板归档文件包含非法的 FileCache 条目名：{entry.FullName}");
                    }
                    Directory.CreateDirectory(tempFileCacheDir);
                    string dest = Path.Combine(tempFileCacheDir, fileName);
                    // 第三层：规范化最终路径并强校验其必须位于安全临时目录内
                    string fullDest = Path.GetFullPath(dest);
                    if (!fullDest.StartsWith(safeFileCacheRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException($"归档条目解析后路径跳出允许的目录：{fullDest}");
                    }
#pragma warning disable SCS0018 // destinationFileName 的每一段来源都经过上述白名单/黑名单+目录边界验证
                    entry.ExtractToFile(fullDest, overwrite: true);
#pragma warning restore SCS0018
                    ctx.TempFileCacheFiles.Add(fullDest);
                }
            }

            ctx.TempDbPath = tempDbPath;

            // 3. 检查 .db 的实际 Schema 版本
            int dbSchemaVersion = ReadDbSchemaVersion(tempDbPath);
            if (dbSchemaVersion > CurrentSchemaVersion)
            {
                throw new InvalidOperationException(
                    $"模板数据库版本过高（v{dbSchemaVersion}），请升级软件后导入。");
            }

            // 4. 用 ProjectDAL 打开（构造函数自动执行 UpdateSchema 升级），读取模板 Project 记录
            // using：ProjectDAL.Dispose 会执行 CheckpointAndClose，确保 SaveProject 写入的 WAL 数据
            // 合并到主 .db 文件，避免后续 File.OpenRead(ctx.TempDbPath) 只读主文件而丢失 WAL 中的最新数据
            using (var dal = new ProjectDAL(tempDbPath))
            {
            Auditai.DTO.Project projectDto = dal.GetProject();

            if (projectDto != null)
            {
                // 重置 Id 为新模板 Id，清除 TemplateId 关联（避免与原模板冲突）
                projectDto.Id = newTemplateId;
                projectDto.TemplateId = null;
                dal.SaveProject(projectDto);
            }
            else
            {
                // 极端情况：.db 中没有 Project 记录，使用元信息构造
                projectDto = new Auditai.DTO.Project
                {
                    Id = newTemplateId,
                    Name = metadata.TemplateName,
                    Number = metadata.TemplateNumber,
                    Category = metadata.Category,
                    Note = metadata.Note,
                    CreateTime = metadata.CreateTime
                };
                dal.SaveProject(projectDto);
            }

            ctx.ProjectDto = projectDto;
            } // 关闭 using (dal)，触发 CheckpointAndClose 合并 WAL 数据到主 .db

            // 异步等待一下，与 ProjectArchive.ImportAsync 风格保持一致
            await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
            return ctx;
        }
        catch
        {
            // 解压失败时清理临时目录
            ctx.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 将 dlgTemplateEditor 编辑后的模板信息写入临时 .db：
    /// 重置 Id / Name / Number / Category / Note / TeamVisible / CreateTime + 同步状态。
    /// </summary>
    public static void ApplyEditorResult(TemplateImportContext ctx, Auditai.DTO.Project editedTemplate)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));
        if (editedTemplate == null) throw new ArgumentNullException(nameof(editedTemplate));
        if (string.IsNullOrEmpty(ctx.TempDbPath) || !File.Exists(ctx.TempDbPath))
            throw new InvalidOperationException("临时模板数据库不存在");

        // using：ProjectDAL.Dispose 会执行 CheckpointAndClose，确保 SaveProject/Execute 写入的 WAL 数据
        // 合并到主 .db 文件，避免后续 File.OpenRead(ctx.TempDbPath) 只读主文件而丢失 WAL 中的最新数据
        using (var dal = new ProjectDAL(ctx.TempDbPath))
        {
        var dbProject = dal.GetProject() ?? ctx.ProjectDto ?? new Auditai.DTO.Project();
        dbProject.Id = ctx.NewTemplateId;
        dbProject.Name = editedTemplate.Name ?? "";
        dbProject.Number = editedTemplate.Number ?? "";
        dbProject.Category = editedTemplate.Category ?? "";
        dbProject.Note = editedTemplate.Note ?? "";
        dbProject.TemplateId = null;
        dbProject.CreateTime =
            !IsPlaceholderCreateTime(editedTemplate.CreateTime) ? editedTemplate.CreateTime :
            !IsPlaceholderCreateTime(ctx.Metadata.CreateTime) ? ctx.Metadata.CreateTime :
            DateTime.Now;
        dal.SaveProject(dbProject);

        // 重置同步状态（与 LocalDataStore.SaveProjectAsTemplate 一致）
        // 使用 dal.Execute 复用 ProjectDAL 的连接管理，避免直接 SQLiteConnection 与 EXCLUSIVE 锁冲突
        try
        {
            dal.Execute(@"
                UPDATE TreeNode SET Version=0, Dirty=0, ServerIndex=0;
                UPDATE TreeGroup SET Dirty=0, ServerIndex=0;
                UPDATE [Table] SET Dirty=0;
                UPDATE Document SET Dirty=0;");
        }
        catch
        {
            // 表可能不存在（老版本 .db），忽略
        }
        } // using 块结束触发 Dispose → CheckpointAndClose，确保 WAL 数据合并到主 .db
    }

    /// <summary>
    /// 执行模板导入注册（在 dlgTemplateEditor 编辑完成后调用）：
    /// - 本地模式：复制 .db 到 Data\Templates\{templateId}.db + 注册本地主库 + 复制 FileCache
    /// - 服务端模式：上传 .db 流到 WebApiClient.ImportTemplate + 上传 FileCache 附件
    /// 返回最终创建的模板 DTO（服务端模式返回服务端创建的 DTO，本地模式返回传入的 newTemplate）。
    /// </summary>
    public static async Task<Auditai.DTO.Project> ImportAsync(TemplateImportContext ctx, Auditai.DTO.Project newTemplate)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));
        if (newTemplate == null) throw new ArgumentNullException(nameof(newTemplate));
        if (string.IsNullOrEmpty(ctx.TempDbPath) || !File.Exists(ctx.TempDbPath))
            throw new InvalidOperationException("临时模板数据库不存在");

        if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
        {
            // 本地模式：委托 LocalDataStore 完成 .db 复制 + 主库注册 + FileCache 复制
            return await Auditai.LocalDataStore.LocalDataStore.RegisterImportedTemplate(
                ctx.TempDbPath, ctx.TempFileCacheFiles, newTemplate);
        }
        else
        {
            // 服务端模式：上传 .db 流到服务端，服务端创建主库记录 + 落盘到 Templates/ 目录 + seed 主库
            using var dbStream = File.OpenRead(ctx.TempDbPath);

            DateTime createTimeForServer =
                !IsPlaceholderCreateTime(newTemplate.CreateTime) ? newTemplate.CreateTime :
                !IsPlaceholderCreateTime(ctx.Metadata.CreateTime) ? ctx.Metadata.CreateTime :
                ctx.Metadata.ExportTime != default ? ctx.Metadata.ExportTime :
                DateTime.Now;

            int schemaVersion = Math.Max(ctx.Metadata.SchemaVersion, CurrentSchemaVersion);

            var created = await Auditai.Util.WebApiClient.ImportTemplate(
                dbStream,
                name: newTemplate.Name ?? ctx.Metadata.TemplateName,
                number: newTemplate.Number ?? ctx.Metadata.TemplateNumber,
                category: newTemplate.Category ?? ctx.Metadata.Category,
                note: newTemplate.Note ?? ctx.Metadata.Note,
                createTime: createTimeForServer,
                schemaVersion: schemaVersion,
                teamVisible: newTemplate.TeamVisible);

            // 服务端模式：逐个上传归档内的 FileCache 文件到服务器
            // —— WebApiClient.UploadFile 会按 FileId 写入服务端 Files/{templateId}/{fileId}.bin
            Guid createdTemplateId = created?.Id ?? newTemplate.Id;
            foreach (string f in ctx.TempFileCacheFiles)
            {
                try
                {
                    string rawName = Path.GetFileNameWithoutExtension(f); // fileId.bin -> fileId
                    if (Guid.TryParse(rawName, out Guid fid))
                    {
                        using var fs = File.OpenRead(f);
                        await Auditai.Util.WebApiClient.UploadFile(fid, fs, createdTemplateId, $"{fid}.bin");
                    }
                }
                catch
                {
                    // 单个文件上传失败不阻塞整体导入（缺失的文件打开时会自动从云端再下载）
                }
            }

            // 上传 dlgTemplateEditor 编辑的成员列表到服务端
            // —— ImportTemplate 路由只创建 Projects 主库记录，不写入 ProjectMembers；
            //    需额外调用 UpdateProjectMembers 全量写入成员（包含 TeamVisible 控制）
            if (created != null && newTemplate.Users != null && newTemplate.Users.Any())
            {
                try
                {
                    created.Users = newTemplate.Users;
                    await Auditai.Util.WebApiClient.UpdateProjectMembers(created);
                }
                catch
                {
                    // 成员更新失败不阻塞整体导入（用户可在"修改模板"中事后调整成员）
                }
            }

            return created ?? newTemplate;
        }
    }

    /// <summary>读取 SQLite 数据库的 PRAGMA user_version（不依赖 ProjectDAL）</summary>
    private static int ReadDbSchemaVersion(string dbPath)
    {
        string connStr = $"Data Source={dbPath};Version=3;Read Only=True;";
        using (var conn = new SQLiteConnection(connStr))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA user_version;";
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }
    }

    /// <summary>
    /// 从模板 .db 中收集所有二进制附件的 FileId（Pdf 表 / Image 表 / CellProp 单元格附件）。
    /// 仅使用轻量 SQL 查询，不加载整个 DAL；任一表缺失时自动跳过，向前兼容老版本归档。
    /// </summary>
    private static HashSet<Guid> CollectFileIdsFromDb(string dbPath)
    {
        var set = new HashSet<Guid>();
        string connStr = $"Data Source={dbPath};Version=3;Read Only=True;";

        using (var conn = new SQLiteConnection(connStr))
        {
            conn.Open();

            // 1. Pdf 表
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT `FileId` FROM `Pdf` WHERE `FileId` IS NOT NULL;";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    object v = r.GetValue(0);
                    if (v == null || v is DBNull) continue;
                    if (Guid.TryParse(v.ToString(), out var g)) set.Add(g);
                }
            }
            catch { /* 老版本 .db 可能没有 Pdf 表，忽略 */ }

            // 2. Image 表
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT `FileId` FROM `Image` WHERE `FileId` IS NOT NULL;";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    object v = r.GetValue(0);
                    if (v == null || v is DBNull) continue;
                    if (Guid.TryParse(v.ToString(), out var g)) set.Add(g);
                }
            }
            catch { /* 老版本 .db 可能没有 Image 表，忽略 */ }

            // 3. CellProp.Attachments（protobuf 字节，内有多个 CellAttachmentEntry）
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT `Attachments` FROM `CellProp` WHERE `Attachments` IS NOT NULL AND length(`Attachments`) > 0;";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    if (r.IsDBNull(0)) continue;
                    byte[] blob = (byte[])r.GetValue(0);
                    if (blob == null || blob.Length == 0) continue;
                    try
                    {
                        var cellAtts = Auditai.DTO.CellAttachments.Parser.ParseFrom(blob);
                        if (cellAtts == null) continue;
                        foreach (var entry in cellAtts.Entries)
                        {
                            if (entry?.Id == null) continue;
                            if (entry.Id.Length != 16) continue; // ByteString to Guid = 16 bytes
                            try { set.Add(new Guid(entry.Id.ToByteArray())); }
                            catch { /* 无效 guid 跳过 */ }
                        }
                    }
                    catch { /* 单个 Attachments 解析失败不阻塞整体 */ }
                }
            }
            catch { /* 老版本 .db 可能没有 CellProp 表，忽略 */ }
        }

        return set;
    }
}
