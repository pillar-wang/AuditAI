using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auditai.DTO;
using Auditai.LocalDataStore;

namespace Auditai.UI.Platform;

/// <summary>
/// 项目元数据查询服务，统一管理项目名称和表名称的查询与缓存。
/// 消除 frmCrossProjectDataRef 中4个重复方法的代码，提供带缓存的异步 API。
/// </summary>
public class ProjectMetadataService
{
    private static ProjectMetadataService _instance;

    /// <summary>
    /// 单例实例，共享缓存
    /// </summary>
    public static ProjectMetadataService Instance =>
        _instance ?? (_instance = new ProjectMetadataService());

    // 缓存：项目名称
    private readonly Dictionary<Guid, CacheEntry<string>> _projectNameCache = new Dictionary<Guid, CacheEntry<string>>();
    // 缓存：表名称
    private readonly Dictionary<(Guid, long), CacheEntry<string>> _tableNameCache = new Dictionary<(Guid, long), CacheEntry<string>>();

    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ErrorCacheDuration = TimeSpan.FromMinutes(5);

    private class CacheEntry<T>
    {
        public T Value { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsExpired => DateTime.Now > ExpiresAt;
    }

    /// <summary>
    /// 异步获取项目名称（带缓存）
    /// </summary>
    public async Task<string> GetProjectNameAsync(Guid projectId)
    {
        // 检查缓存
        if (_projectNameCache.TryGetValue(projectId, out var entry) && !entry.IsExpired)
            return entry.Value;

        string name;
        try
        {
            // 优先从当前项目列表中查询
            var projects = await StorageRouter.GetProjects();
            var project = projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null)
            {
                name = project.Name ?? projectId.ToString();
            }
            else
            {
                // 从数据库查询
                name = LoadProjectNameFromDb(projectId);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取项目名称失败: {ex.Message}");
            name = projectId.ToString();
            // 错误结果缓存时间较短
            _projectNameCache[projectId] = new CacheEntry<string> { Value = name, ExpiresAt = DateTime.Now + ErrorCacheDuration };
            return name;
        }

        _projectNameCache[projectId] = new CacheEntry<string> { Value = name, ExpiresAt = DateTime.Now + DefaultCacheDuration };
        return name;
    }

    /// <summary>
    /// 批量获取项目名称（优化性能，减少数据库访问次数）
    /// </summary>
    public async Task<Dictionary<Guid, string>> GetProjectNamesAsync(IEnumerable<Guid> projectIds)
    {
        var result = new Dictionary<Guid, string>();
        var uncachedIds = new List<Guid>();

        // 首先从缓存中获取
        foreach (var projectId in projectIds.Distinct())
        {
            if (_projectNameCache.TryGetValue(projectId, out var entry) && !entry.IsExpired)
            {
                result[projectId] = entry.Value;
            }
            else
            {
                uncachedIds.Add(projectId);
            }
        }

        // 批量查询未缓存的项目：一次获取所有项目列表
        if (uncachedIds.Count > 0)
        {
            Dictionary<Guid, string> projectListMap = null;
            try
            {
                var projects = await StorageRouter.GetProjects();
                projectListMap = projects.ToDictionary(p => p.Id, p => p.Name ?? p.Id.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"批量获取项目列表失败: {ex.Message}");
                projectListMap = new Dictionary<Guid, string>();
            }

            foreach (var projectId in uncachedIds)
            {
                string name;
                if (projectListMap.TryGetValue(projectId, out var listName))
                {
                    name = listName;
                }
                else
                {
                    // 项目列表中没有，尝试从数据库查询
                    name = LoadProjectNameFromDb(projectId);
                }

                result[projectId] = name;
                _projectNameCache[projectId] = new CacheEntry<string> { Value = name, ExpiresAt = DateTime.Now + DefaultCacheDuration };
            }
        }

        return result;
    }

    /// <summary>
    /// 异步获取表名称（带缓存）
    /// </summary>
    public async Task<string> GetTableNameAsync(Guid projectId, Id64 tableId)
    {
        var key = (projectId, tableId.Value);

        // 检查缓存
        if (_tableNameCache.TryGetValue(key, out var entry) && !entry.IsExpired)
            return entry.Value;

        string name = await LoadTableNameFromDbAsync(projectId, tableId);
        _tableNameCache[key] = new CacheEntry<string> { Value = name, ExpiresAt = DateTime.Now + DefaultCacheDuration };
        return name;
    }

    /// <summary>
    /// 同步获取项目名称（用于非 UI 线程，带缓存）
    /// </summary>
    public static string GetProjectNameById(Guid projectId)
    {
        // 先检查缓存
        if (Instance._projectNameCache.TryGetValue(projectId, out var entry) && !entry.IsExpired)
            return entry.Value;

        try
        {
            var projects = Task.Run(async () => await StorageRouter.GetProjects()).GetAwaiter().GetResult();
            var project = projects.FirstOrDefault(p => p.Id == projectId);
            string name = project != null
                ? (project.Name ?? projectId.ToString())
                : LoadProjectNameFromDb(projectId);

            Instance._projectNameCache[projectId] = new CacheEntry<string>
            {
                Value = name,
                ExpiresAt = DateTime.Now + DefaultCacheDuration
            };
            return name;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取项目名称失败: {ex.Message}");
            return projectId.ToString();
        }
    }

    /// <summary>
    /// 同步获取表名称（用于非 UI 线程，带缓存）
    /// </summary>
    public static string GetTableNameById(Guid projectId, Id64 tableId)
    {
        var key = (projectId, tableId.Value);

        // 先检查缓存
        if (Instance._tableNameCache.TryGetValue(key, out var entry) && !entry.IsExpired)
            return entry.Value;

        try
        {
            string name = LoadTableNameFromDbSync(projectId, tableId);
            Instance._tableNameCache[key] = new CacheEntry<string>
            {
                Value = name,
                ExpiresAt = DateTime.Now + DefaultCacheDuration
            };
            return name;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取表名称失败: {ex.Message}");
            return tableId.Value.ToString();
        }
    }

    /// <summary>
    /// 从数据库加载项目名称
    /// </summary>
    private static string LoadProjectNameFromDb(Guid projectId)
    {
        string dbPath = MainForm.GetDbPathByGuid(projectId);
        if (!File.Exists(dbPath))
            return projectId.ToString();

        var dal = new ProjectDAL(dbPath);
        var projectDto = dal.GetProject();
        return projectDto?.Name ?? projectId.ToString();
    }

    /// <summary>
    /// 异步从数据库加载表名称
    /// </summary>
    private static async Task<string> LoadTableNameFromDbAsync(Guid projectId, Id64 tableId)
    {
        return await Task.Run(() => LoadTableNameFromDbSync(projectId, tableId));
    }

    /// <summary>
    /// 同步从数据库加载表名称
    /// </summary>
    private static string LoadTableNameFromDbSync(Guid projectId, Id64 tableId)
    {
        string dbPath = MainForm.GetDbPathByGuid(projectId);
        if (!File.Exists(dbPath))
            return tableId.Value.ToString();

        var dal = new ProjectDAL(dbPath);
        var dto = dal.GetProject();
        if (dto == null)
            return tableId.Value.ToString();

        var project = new Auditai.Model.Project
        {
            Id = projectId,
            Name = dto.Name,
            Dal = dal
        };
        project.PopulateFieldsFromDto(dto);
        project.Load();

        var tableNode = project.GetAllTableNodes().FirstOrDefault(n => n.Id == tableId);
        if (tableNode != null)
            return tableNode.Number + " " + tableNode.Name;

        var tableDto = dal.GetTable(tableId);
        return tableDto?.Title ?? tableId.Value.ToString();
    }

    /// <summary>
    /// 清除指定项目的缓存
    /// </summary>
    public void ClearProjectCache(Guid projectId)
    {
        _projectNameCache.Remove(projectId);
        // 清除该项目关联的所有表名缓存
        var keysToRemove = _tableNameCache.Keys.Where(k => k.Item1 == projectId).ToList();
        foreach (var key in keysToRemove)
            _tableNameCache.Remove(key);
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void ClearAllCache()
    {
        _projectNameCache.Clear();
        _tableNameCache.Clear();
    }
}
