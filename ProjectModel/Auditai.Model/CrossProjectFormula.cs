﻿﻿﻿using System;
using System.Collections.Generic;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

/// <summary>
/// 跨项目公式引用类型
/// </summary>
public enum CrossProjectFormulaType
{
    /// <summary>批量引用：引用多列数据</summary>
    Batch,
    /// <summary>区域引用：引用矩形区域（多行多列）</summary>
    Range,
    /// <summary>公式运算：跨项目取数后进行运算</summary>
    Compute
}

/// <summary>
/// 跨项目公式引用模型
/// </summary>
[Serializable]
public class CrossProjectFormula
{
    /// <summary>唯一标识</summary>
    public Id64 Id { get; set; }
    
    /// <summary>来源项目ID</summary>
    public Guid SourceProjectId { get; set; }
    
    /// <summary>来源表ID</summary>
    public Id64 SourceTableId { get; set; }
    
    /// <summary>目标表ID（当前项目的表）</summary>
    public Id64 TargetTableId { get; set; }
    
    /// <summary>目标起始单元格ID（区域引用时使用）</summary>
    public Id64 TargetCellId { get; set; }
    
    /// <summary>公式类型</summary>
    public CrossProjectFormulaType FormulaType { get; set; }
    
    /// <summary>公式表达式</summary>
    /// <para>Batch: JSON数组 ["Col1", "Col2", ...] 表示来源列名列表</para>
    /// <para>Range: JSON对象 {"StartRow":0, "EndRow":5, "StartCol":0, "EndCol":3}</para>
    /// <para>Compute: 公式字符串如 "[Table1.Col1] + [Table2.Col2]"</para>
    public string FormulaExpression { get; set; }
    
    /// <summary>来源列ID列表（Batch/Compute 类型使用）</summary>
    public List<Id64> SourceColumnIds { get; set; } = new List<Id64>();
    
    /// <summary>目标列ID列表（Batch 类型使用）</summary>
    public List<Id64> TargetColumnIds { get; set; } = new List<Id64>();
    
    /// <summary>是否启用</summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static CrossProjectFormula Deserialize(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return null;
        return JsonConvert.DeserializeObject<CrossProjectFormula>(s);
    }
}