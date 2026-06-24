﻿﻿﻿﻿﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Leqisoft.Model;

public enum CustomFillMode
{
    Overwrite,  // 覆盖
    Append      // 追加
}

[Serializable]
public class CustomFillRule
{
    /// <summary>目标表格的 ParaId（空字符串表示当前表格）</summary>
    [JsonProperty("T")]
    public string TargetTableId { get; set; }

    [JsonProperty("V")]
    public string Value { get; set; }

    [JsonProperty("P")]
    public string TargetPosition { get; set; }

    [JsonProperty("M")]
    [JsonConverter(typeof(StringEnumConverter))]
    public CustomFillMode FillMode { get; set; } = CustomFillMode.Overwrite;

    /// <summary>填充条件（null 表示无条件执行）</summary>
    [JsonProperty("C")]
    public CustomFillCondition Condition { get; set; }

    /// <summary>是否有条件</summary>
    [JsonIgnore]
    public bool HasCondition => Condition != null
        && !string.IsNullOrWhiteSpace(Condition.SourcePosition);
}