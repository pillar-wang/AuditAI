﻿﻿﻿using System;
using System.Collections.Generic;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

/// <summary>
/// 跨项目数据引用模式
/// </summary>
public enum RefMode
{
    /// <summary>单元格引用：引用单个单元格的值</summary>
    CellRef,
    /// <summary>列引用：引用某列的所有数据行</summary>
    ColumnRef,
    /// <summary>区域引用：引用一个矩形区域（多行多列）</summary>
    AreaRef,
    /// <summary>公式运算：从多个来源取数后经公式运算得到结果</summary>
    FormulaCompute
}

/// <summary>
/// 跨项目数据引用模型
/// </summary>
[Serializable]
public class CrossProjectDataRef
{
    /// <summary>唯一标识</summary>
    public Id64 Id { get; set; }

    /// <summary>引用名称</summary>
    public string Name { get; set; }

    /// <summary>来源项目 ID</summary>
    public Guid SourceProjectId { get; set; }

    /// <summary>来源表 ID</summary>
    public Id64 SourceTableId { get; set; }

    /// <summary>目标表 ID</summary>
    public Id64 TargetTableId { get; set; }

    /// <summary>引用模式</summary>
    public RefMode RefMode { get; set; }

    /// <summary>JSON 字符串，按 RefMode 不同解析</summary>
    /// <para>CellRef: {"TargetCellId": 12345}</para>
    /// <para>ColumnRef: {"SourceColumnIds": [1,2,3], "TargetStartRow": 0}</para>
    /// <para>AreaRef: {"StartRow": 0, "EndRow": 10, "StartCol": 0, "EndCol": 5, "TargetStartRow": 0, "TargetStartCol": 0}</para>
    /// <para>FormulaCompute: {"DataSources": [{"Name": "A", "ProjectId": "guid", "TableId": 123, "ColumnId": 456}, ...]}</para>
    public string RefConfig { get; set; }

    /// <summary>JSON 筛选配置</summary>
    public string FilterConfig { get; set; }

    /// <summary>公式表达式</summary>
    public string FormulaExpression { get; set; }

    /// <summary>JSON，来源列ID→目标列ID 映射</summary>
    /// <para>[{"SourceColumnId": 111, "TargetColumnId": 222}, ...]</para>
    public string ColumnMapping { get; set; }

    /// <summary>是否自动刷新</summary>
    public bool AutoRefresh { get; set; } = true;

    /// <summary>是否启用</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static CrossProjectDataRef Deserialize(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return null;
        return JsonConvert.DeserializeObject<CrossProjectDataRef>(s);
    }
}