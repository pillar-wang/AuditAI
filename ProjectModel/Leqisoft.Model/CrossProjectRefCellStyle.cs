﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

/// <summary>
/// 跨项目数据引用单元格样式定义
/// 用于在目标表中标记引用数据的来源和状态
/// </summary>
public static class CrossProjectRefCellStyle
{
    /// <summary>引用状态枚举</summary>
    public enum RefStatus
    {
        Normal = 0,         // 正常数据 - 白色背景
        CacheFallback = 1,  // 缓存降级 - 黄色背景
        DefaultValue = 2,   // 默认值 - 浅红色背景
        Error = 3,          // 错误 - 浅红色背景
        Refreshing = 4      // 正在刷新 - 浅蓝色背景
    }

    /// <summary>引用数据标记信息</summary>
    public class RefCellMark
    {
        public Id64 RefId { get; set; }
        public string RefName { get; set; }
        public string SourceProjectName { get; set; }
        public string SourceTableName { get; set; }
        public RefStatus Status { get; set; }
        public DateTime LastRefreshAt { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int StartCol { get; set; }
        public int EndCol { get; set; }
        public string UserDataKey => $"CrossProjectRef_{RefId.Value}";

        /// <summary>获取 ToolTip 显示文本</summary>
        public string GetToolTipText()
        {
            var statusText = Status switch
            {
                RefStatus.Normal => "数据正常",
                RefStatus.CacheFallback => "缓存数据（来源不可用）",
                RefStatus.DefaultValue => "默认值（来源不可用）",
                RefStatus.Error => "引用错误",
                RefStatus.Refreshing => "正在刷新...",
                _ => "未知状态"
            };
            return $"引用: {RefName}\n来源: {SourceProjectName}::{SourceTableName}\n状态: {statusText}\n最后刷新: {LastRefreshAt:yyyy-MM-dd HH:mm:ss}\n双击跳转到引用配置";
        }
    }

    /// <summary>存储引用标记信息的字典（Key = "CrossProjectRef_{RefId}"）</summary>
    private static readonly Dictionary<string, RefCellMark> _marks = new Dictionary<string, RefCellMark>();

    /// <summary>添加/更新标记</summary>
    public static void SetMark(RefCellMark mark)
    {
        _marks[mark.UserDataKey] = mark;
    }

    /// <summary>获取标记</summary>
    public static RefCellMark GetMark(Id64 refId)
    {
        _marks.TryGetValue($"CrossProjectRef_{refId.Value}", out var mark);
        return mark;
    }

    /// <summary>获取所有标记</summary>
    public static List<RefCellMark> GetAllMarks()
    {
        return _marks.Values.ToList();
    }

    /// <summary>清除指定引用的标记</summary>
    public static void ClearMark(Id64 refId)
    {
        _marks.Remove($"CrossProjectRef_{refId.Value}");
    }

    /// <summary>清除所有标记</summary>
    public static void ClearAll()
    {
        _marks.Clear();
    }
}