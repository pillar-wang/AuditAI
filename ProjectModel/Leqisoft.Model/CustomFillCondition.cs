﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Leqisoft.Model;

/// <summary>条件运算符</summary>
public enum ConditionOperator
{
    [JsonProperty("=")]
    Equals,         // 等于
    [JsonProperty("!=")]
    NotEquals,      // 不等于
    [JsonProperty("*")]
    Contains,       // 包含
    [JsonProperty("!*")]
    NotContains,    // 不包含
    [JsonProperty(">")]
    GreaterThan,    // 大于
    [JsonProperty("<")]
    LessThan,       // 小于
    [JsonProperty("e")]
    IsEmpty,        // 为空
    [JsonProperty("ne")]
    IsNotEmpty      // 非空
}

/// <summary>填充条件：满足条件时才执行填充</summary>
[Serializable]
public class CustomFillCondition
{
    /// <summary>条件源位置（单元格引用，如 A1）</summary>
    [JsonProperty("S")]
    public string SourcePosition { get; set; }

    /// <summary>条件运算符</summary>
    [JsonProperty("O")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ConditionOperator Operator { get; set; }

    /// <summary>比较值（为空/非空时忽略）</summary>
    [JsonProperty("V")]
    public string CompareValue { get; set; }

    /// <summary>判断条件是否满足</summary>
    public bool IsMet(string cellValue)
    {
        return Operator switch
        {
            ConditionOperator.Equals => string.Equals(cellValue, CompareValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotEquals => !string.Equals(cellValue, CompareValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.Contains => cellValue?.IndexOf(CompareValue ?? "", StringComparison.OrdinalIgnoreCase) >= 0,
            ConditionOperator.NotContains => cellValue?.IndexOf(CompareValue ?? "", StringComparison.OrdinalIgnoreCase) < 0,
            ConditionOperator.GreaterThan => CompareNumeric(cellValue, CompareValue) > 0,
            ConditionOperator.LessThan => CompareNumeric(cellValue, CompareValue) < 0,
            ConditionOperator.IsEmpty => string.IsNullOrWhiteSpace(cellValue),
            ConditionOperator.IsNotEmpty => !string.IsNullOrWhiteSpace(cellValue),
            _ => true
        };
    }

    private static int CompareNumeric(string a, string b)
    {
        if (decimal.TryParse(a, out var da) && decimal.TryParse(b, out var db))
            return da.CompareTo(db);
        return string.Compare(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);
    }
}
