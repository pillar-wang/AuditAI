using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Data.SQLite;
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

/// <summary>项目归档读写核心逻辑（.lqaudit 格式）</summary>
public static class ProjectArchive
{
    /// <summary>当前软件支持的最高项目数据库 Schema 版本</summary>
    private const int CurrentSchemaVersion = 43;

    private const string AppVersion = "1.0.0";

    private const string EntryProjectDb = "project.db";
    private const string EntryMetadata = "metadata.json";

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

    /// <summary>导出项目为 .lqaudit 归档文件</summary>
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
            CreateTime = dbProject?.CreateTime ?? projectInfo.CreateTime,
            ExportTime = DateTime.Now,
            ExportedBy = Auditai.Model.User.Current?.Name ?? Auditai.Model.User.Current?.UserName ?? "",
            SchemaVersion = CurrentSchemaVersion,
            AppVersion = AppVersion
        };

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

    /// <summary>从 .lqaudit 归档导入项目，返回新项目 DTO</summary>
    public static Auditai.DTO.Project Import(string archivePath)
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

        // 2. 生成新项目 ID
        Guid newProjectId = Guid.NewGuid();

        // 3. 解压到临时目录
        string tempDir = Path.Combine(Path.GetTempPath(), "lqaudit_import_" + newProjectId.ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string tempDbPath = Path.Combine(tempDir, EntryProjectDb);

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var dbEntry = archive.GetEntry(EntryProjectDb);
                if (dbEntry == null)
                    throw new InvalidDataException("无效的归档文件：缺少 project.db");

                dbEntry.ExtractToFile(tempDbPath, overwrite: true);
            }

            // 4. 检查 .db 的实际 Schema 版本
            int dbSchemaVersion = ReadDbSchemaVersion(tempDbPath);
            if (dbSchemaVersion > CurrentSchemaVersion)
            {
                throw new InvalidOperationException(
                    $"项目数据库版本过高（v{dbSchemaVersion}），请升级软件后导入。");
            }

            // 5. 用 ProjectDAL 打开（构造函数自动执行 UpdateSchema 升级）
            Project projectDto;
            var dal = new ProjectDAL(tempDbPath);
            projectDto = dal.GetProject();

            // 6. 更新项目信息：新 ID、清除模板关联
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

            // 7. 复制 .db 到目标位置
            long userId = Auditai.Model.User.Current?.Id ?? 1;
            string userDir = Path.Combine("data", userId.ToString());
            Directory.CreateDirectory(userDir);
            string targetDbPath = Path.Combine(userDir, $"{newProjectId}.db");

            File.Copy(tempDbPath, targetDbPath, overwrite: true);

            // 8. 在主数据库注册新项目
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
                CreateTime = DateTime.Now,
                ParentId = null,
                TemplateId = null
            };

            Auditai.LocalDataStore.LocalDataStore.RegisterImportedProject(newProject);

            return newProject;
        }
        finally
        {
            // 清理临时目录
            try { Directory.Delete(tempDir, recursive: true); } catch { }
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
}
