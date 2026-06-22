﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

/// <summary>
/// 跨项目数据引用授权验证提供程序
/// </summary>
public class CrossProjectRefAuthProvider
{
    private readonly Project _project;
    private readonly CrossProjectDataRefStore _store;

    public CrossProjectRefAuthProvider(Project project)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _store = new CrossProjectDataRefStore(project);
    }

    /// <summary>
    /// 检查目标项目是否有权限访问指定数据
    /// </summary>
    public bool CheckAccess(Guid sourceProjectId, Guid targetProjectId, Id64 tableId, List<Id64> columnIds)
    {
        // 1. 获取授权记录
        var auth = _store.GetAuth(sourceProjectId, targetProjectId);
        if (auth == null) return false;
        
        // 2. 检查是否有效
        if (!auth.IsActive) return false;
        if (auth.ExpiresAt.HasValue && auth.ExpiresAt.Value < DateTime.Now) return false;
        
        // 3. 检查表权限
        var allowedTableIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(auth.AllowedTableIds ?? "[]");
        if (allowedTableIds.Count > 0 && !allowedTableIds.Contains(tableId.Value))
            return false;
        
        // 4. 检查列权限
        if (columnIds != null && columnIds.Count > 0)
        {
            var allowedColumnIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(auth.AllowedColumnIds ?? "[]");
            if (allowedColumnIds.Count > 0 && columnIds.Any(c => !allowedColumnIds.Contains(c.Value)))
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// 检查目标项目是否有权限访问指定表
    /// </summary>
    public bool CheckTableAccess(Guid sourceProjectId, Guid targetProjectId, Id64 tableId)
    {
        return CheckAccess(sourceProjectId, targetProjectId, tableId, null);
    }

    /// <summary>
    /// 获取对当前项目已授权的项目列表
    /// </summary>
    public List<Guid> GetAuthorizedProjects(Guid targetProjectId)
    {
        var auths = _store.GetAllAuths(targetProjectId);
        return auths
            .Where(a => a.IsActive && (!a.ExpiresAt.HasValue || a.ExpiresAt.Value >= DateTime.Now))
            .Select(a => a.SourceProjectId)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// 生成授权请求信息
    /// </summary>
    public string GenerateAuthRequest(Guid targetProjectId, Id64 tableId, List<Id64> columnIds)
    {
        var request = new
        {
            RequestedBy = targetProjectId.ToString(),
            TableId = tableId.Value,
            ColumnIds = columnIds?.Select(c => c.Value).ToList(),
            RequestedAt = DateTime.Now
        };
        return Newtonsoft.Json.JsonConvert.SerializeObject(request);
    }
}