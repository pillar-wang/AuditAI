using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using Google.Protobuf;
using Newtonsoft.Json;
using Auditai.DTO;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

/// <summary>项目归档元信息</summary>
public class ProjectArchiveMetadata
{
    [JsonProperty("projectName")]
    public string ProjectName { get; set; } = "";

    [JsonProperty("projectNumber")]
    public string ProjectNumber { get; set; } = "";

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

    [JsonProperty("auditee")]
    public string Auditee { get; set; } = "";
}

/// <summary>项目归档读写核心逻辑（.auditai 格式）</summary>
public static class ProjectArchive
{
    /// <summary>当前软件支持的最高项目数据库 Schema 版本</summary>
    private const int CurrentSchemaVersion = 44;

    private const string AppVersion = "1.0.0";

    private const string EntryProjectDb = "project.db";
    private const string EntryMetadata = "metadata.json";
    /// <summary>.auditai 中 FileCache 条目前缀：解压后为 FileCache/{fileId}.bin</summary>
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
    public static ProjectArchiveMetadata ReadMetadata(string archivePath)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException("归档文件不存在", archivePath);

        using (var archive = ZipFile.OpenRead(archivePath))
        {
            var entry = archive.GetEntry(EntryMetadata);
            if (entry == null)
                throw new InvalidDataException("无效的归档文件：缺少 metadata.json");

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                string json = reader.ReadToEnd();
                var metadata = JsonConvert.DeserializeObject<ProjectArchiveMetadata>(json);
                if (metadata == null)
                    throw new InvalidDataException("metadata.json 解析失败");
                return metadata;
            }
        }
    }

    /// <summary>导出项目为 .auditai 归档文件</summary>
    /// <param name="projectInfo">主数据库中的项目 DTO（提供名称、编号、类别等元信息）</param>
    /// <param name="outputPath">输出文件路径</param>
    public static void Export(Auditai.DTO.Project projectInfo, string outputPath)
    {
        Guid projectId = projectInfo.Id;
        string dbPath = MainForm.GetDbPathByGuid(projectId);
        if (!File.Exists(dbPath))
            throw new FileNotFoundException("项目数据库文件不存在: " + dbPath, dbPath);

        // 从项目 .db 中读取补充信息（CreateTime 等）
        Project dbProject = null;
        try
        {
            var dal = new ProjectDAL(dbPath);
            dbProject = dal.GetProject();
        }
        catch { /* 读取失败时使用主数据库的信息 */ }

        var metadata = new ProjectArchiveMetadata
        {
            ProjectName = projectInfo.Name ?? dbProject?.Name ?? "",
            ProjectNumber = projectInfo.Number ?? dbProject?.Number ?? "",
            Category = projectInfo.Category ?? dbProject?.Category ?? "",
            Note = projectInfo.Note ?? dbProject?.Note ?? "",
            Auditee = projectInfo.Auditee ?? "",
            CreateTime =
                // 优先使用主库中的 CreateTime；项目 .db 中如果是 2000-01-01 占位值则跳过
                !IsPlaceholderCreateTime(projectInfo.CreateTime) ? projectInfo.CreateTime :
                !IsPlaceholderCreateTime(dbProject?.CreateTime ?? default) ? dbProject.CreateTime :
                DateTime.Now,
            ExportTime = DateTime.Now,
            ExportedBy = Auditai.Model.User.Current?.Name ?? Auditai.Model.User.Current?.UserName ?? "",
            SchemaVersion = CurrentSchemaVersion,
            AppVersion = AppVersion
        };

        // 收集该项目所有需要打包的 FileId（Pdf / Image / 单元格附件）
        HashSet<Guid> fileIds;
        try { fileIds = CollectFileIdsFromDb(dbPath); }
        catch { fileIds = new HashSet<Guid>(); }

        // 本地 FileCache 目录：data/{userId}/{projectId}/FileCache
        long userId = Auditai.Model.User.Current?.Id ?? 0;
        string fileCacheDir = Path.Combine("data", userId.ToString(), projectId.ToString(), "FileCache");
        bool fileCacheDirExists = Directory.Exists(fileCacheDir);

        // 写入临时文件，成功后再移动到目标路径（避免产生不完整的导出文件）
        string tempPath = outputPath + ".tmp";
        try
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            using (var archive = ZipFile.Open(tempPath, ZipArchiveMode.Create))
            {
                // 添加 project.db（最优压缩）
                archive.CreateEntryFromFile(dbPath, EntryProjectDb, CompressionLevel.Optimal);

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

    /// <summary>从 .auditai 归档导入项目，返回新项目 DTO</summary>
    public static async Task<Auditai.DTO.Project> ImportAsync(string archivePath)
    {
        if (!File.Exists(archivePath))
            throw new FileNotFoundException("归档文件不存在", archivePath);

        // 1. 读取并验证元信息
        var metadata = ReadMetadata(archivePath);
        if (metadata.SchemaVersion > CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"项目文件版本过高（v{metadata.SchemaVersion}），当前软件支持的最高版本为 v{CurrentSchemaVersion}，请升级软件后导入。");
        }

        // 2. 解压 .db 到临时目录
        Guid newProjectId = Guid.NewGuid();
        string tempDir = Path.Combine(Path.GetTempPath(), "auditai_import_" + newProjectId.ToString("N"));
        Directory.CreateDirectory(tempDir);
        // 归档中的 FileCache 临时存放位置
        string tempFileCacheDir = Path.Combine(tempDir, "FileCache");

        try
        {
            string tempDbPath = Path.Combine(tempDir, EntryProjectDb);
            List<string> tempFileCacheFiles = new List<string>();

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var dbEntry = archive.GetEntry(EntryProjectDb);
                if (dbEntry == null)
                    throw new InvalidDataException("无效的归档文件：缺少 project.db");

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
                        throw new InvalidDataException($"归档文件包含非法的 FileCache 条目名：{entry.FullName}");
                    }
                    Directory.CreateDirectory(tempFileCacheDir);
                    string dest = Path.Combine(tempFileCacheDir, fileName);
                    // 第三层：规范化最终路径并强校验其必须位于安全临时目录内
                    string fullDest = Path.GetFullPath(dest);
                    if (!fullDest.StartsWith(safeFileCacheRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException($"归档条目解析后路径跳出允许的目录：{fullDest}");
                    }
                    // 以上三层校验覆盖了 fileName → dest → fullDest 的每一次流转，任何一次变形都无法绕过 StartsWith 目录边界
#pragma warning disable SCS0018 // destinationFileName 的每一段来源都经过上述白名单/黑名单+目录边界验证
                    entry.ExtractToFile(fullDest, overwrite: true);
#pragma warning restore SCS0018
                    tempFileCacheFiles.Add(fullDest);
                }
            }

            // 3. 检查 .db 的实际 Schema 版本
            int dbSchemaVersion = ReadDbSchemaVersion(tempDbPath);
            if (dbSchemaVersion > CurrentSchemaVersion)
            {
                throw new InvalidOperationException(
                    $"项目数据库版本过高（v{dbSchemaVersion}），请升级软件后导入。");
            }

            // 4. 用 ProjectDAL 打开（构造函数自动执行 UpdateSchema 升级），并清除模板关联
            Project projectDto;
            var dal = new ProjectDAL(tempDbPath);
            projectDto = dal.GetProject();

            if (projectDto != null)
            {
                projectDto.Id = newProjectId;
                projectDto.TemplateId = null;
                dal.SaveProject(projectDto);
            }
            else
            {
                // 极端情况：.db 中没有 Project 记录，使用元信息构造
                projectDto = new Project
                {
                    Id = newProjectId,
                    Name = metadata.ProjectName,
                    Number = metadata.ProjectNumber,
                    Category = metadata.Category,
                    Note = metadata.Note,
                    Auditee = metadata.Auditee,
                    CreateTime = metadata.CreateTime
                };
                dal.SaveProject(projectDto);
            }

            long localUserId = Auditai.Model.User.Current?.Id ?? 1;

            // 5. 分支：本地模式直接复制 .db 到本地 + 注册主库；服务端模式上传到服务器
            if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
            {
                // 本地模式：复制 .db 到 data/{userId}/{projectId}.db + 本地主库注册
                string userDir = Path.Combine("data", localUserId.ToString());
                Directory.CreateDirectory(userDir);
                string targetDbPath = Path.Combine(userDir, $"{newProjectId}.db");
                File.Copy(tempDbPath, targetDbPath, overwrite: true);

                // 本地模式：复制归档内的 FileCache 到 data/{userId}/{newProjectId}/FileCache/
                if (tempFileCacheFiles.Count > 0)
                {
                    string newCacheDir = Path.Combine(userDir, newProjectId.ToString(), "FileCache");
                    Directory.CreateDirectory(newCacheDir);
                    foreach (string f in tempFileCacheFiles)
                    {
                        try { File.Copy(f, Path.Combine(newCacheDir, Path.GetFileName(f)), overwrite: true); }
                        catch { /* 单个文件复制失败不影响整体导入 */ }
                    }
                }

                var newProject = new Auditai.DTO.Project
                {
                    Id = newProjectId,
                    Name = projectDto.Name,
                    Number = projectDto.Number,
                    Category = projectDto.Category,
                    Auditee = projectDto.Auditee,
                    Note = projectDto.Note,
                    Type = ProjectType.Project,
                    Version = 1,
                    CreateTime =
                        !IsPlaceholderCreateTime(projectDto.CreateTime) ? projectDto.CreateTime :
                        !IsPlaceholderCreateTime(metadata.CreateTime) ? metadata.CreateTime :
                        DateTime.Now,
                    ParentId = null,
                    TemplateId = null
                };

                Auditai.LocalDataStore.LocalDataStore.RegisterImportedProject(newProject);
                return newProject;
            }
            else
            {
                // 服务端模式：上传 .db 流到服务端，服务端创建主库记录 + 落盘 .db 文件
                using var dbStream = File.OpenRead(tempDbPath);

                DateTime createTimeForServer =
                    !IsPlaceholderCreateTime(projectDto.CreateTime) ? projectDto.CreateTime :
                    !IsPlaceholderCreateTime(metadata.CreateTime) ? metadata.CreateTime :
                    metadata.ExportTime != default ? metadata.ExportTime :
                    DateTime.Now;

                var created = await Auditai.Util.WebApiClient.ImportProject(
                    dbStream,
                    name: projectDto.Name ?? metadata.ProjectName,
                    number: projectDto.Number ?? metadata.ProjectNumber,
                    category: projectDto.Category ?? metadata.Category,
                    note: projectDto.Note ?? metadata.Note,
                    auditee: projectDto.Auditee ?? metadata.Auditee,
                    createTime: createTimeForServer,
                    schemaVersion: Math.Max(metadata.SchemaVersion, dbSchemaVersion));

                // 服务端模式：逐个上传归档内的 FileCache 文件到服务器
                // —— WebApiClient.UploadFile 会按 FileId 写入服务端 Files/{projectId}/{fileId}.bin
                Guid createdProjectId = created?.Id ?? newProjectId;
                foreach (string f in tempFileCacheFiles)
                {
                    try
                    {
                        string rawName = Path.GetFileNameWithoutExtension(f); // fileId.bin -> fileId
                        if (Guid.TryParse(rawName, out Guid fid))
                        {
                            using var fs = File.OpenRead(f);
                            // QueryString 传 ProjectId（服务端用它做路径分组 + 校验）
                            await Auditai.Util.WebApiClient.UploadFile(fid, fs, createdProjectId, $"{fid}.bin");
                        }
                    }
                    catch
                    {
                        // 单个文件上传失败不阻塞整体导入（缺失的文件打开时会自动从云端再下载）
                    }
                }

                return created;
            }
        }
        finally
        {
            // 清理临时目录
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    /// <summary>
    /// 兼容旧调用方的同步包装：本地模式可直接调用，服务端模式会阻塞等待 WebApiClient 完成。
    /// 新代码请使用 <see cref="ImportAsync"/>。
    /// </summary>
    public static Auditai.DTO.Project Import(string archivePath)
    {
        return ImportAsync(archivePath).GetAwaiter().GetResult();
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
    /// 从项目 .db 中收集所有二进制附件的 FileId（Pdf 表 / Image 表 / CellProp 单元格附件）。
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

