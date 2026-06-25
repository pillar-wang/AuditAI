using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditai.Model;

/// <summary>
/// 筛选预览结果
/// </summary>
public class FilterPreviewResult
{
    public int TotalCount { get; set; }
    public List<List<object>> SampleRows { get; set; }
}

/// <summary>
/// 筛选运算符
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEquals,
    LessThan,
    LessThanOrEquals,
    Between,
    Contains,
    StartsWith,
    EndsWith,
    Empty,
    NotEmpty,
    EmptyOrZero,
    NotEmptyOrZero
}

/// <summary>
/// 筛选条件
/// </summary>
public class FilterCondition
{
    /// <summary>列索引</summary>
    public int ColumnIndex { get; set; }

    /// <summary>运算符</summary>
    public FilterOperator Operator { get; set; }

    /// <summary>条件值</summary>
    public object Value { get; set; }

    /// <summary>第二个条件值（用于 Between）</summary>
    public object Value2 { get; set; }

    /// <summary>条件间关系 And / Or</summary>
    public string Relation { get; set; } = "And";
}

/// <summary>
/// 筛选配置
/// </summary>
public class FilterConfig
{
    /// <summary>条件列表</summary>
    public List<FilterCondition> Conditions { get; set; } = new();

    /// <summary>条件组列表（组间 Or，组内 And）</summary>
    public List<List<FilterCondition>> ConditionGroups { get; set; } = new();
}

/// <summary>
/// 轻量级高级筛选引擎，不依赖 FilterModel 类型
/// </summary>
public static class CrossProjectDataRefFilter
{
    /// <summary>
    /// 对数据应用筛选，返回满足条件的行索引列表
    /// </summary>
    /// <param name="filterConfigJson">筛选配置 JSON 字符串</param>
    /// <param name="sourceData">源数据（行列表，每行为单元格值列表）</param>
    /// <returns>满足条件的行索引列表</returns>
    public static List<int> ApplyFilter(string filterConfigJson, List<List<object>> sourceData)
    {
        if (sourceData == null || sourceData.Count == 0)
            return new List<int>();

        var config = ParseFilterConfig(filterConfigJson);
        if (config == null)
            return Enumerable.Range(0, sourceData.Count).ToList();

        var result = new List<int>();

        for (int rowIndex = 0; rowIndex < sourceData.Count; rowIndex++)
        {
            var row = sourceData[rowIndex];
            if (EvaluateRow(row, config))
                result.Add(rowIndex);
        }

        return result;
    }

    /// <summary>
    /// 预览筛选结果
    /// </summary>
    /// <param name="filterConfigJson">筛选配置 JSON 字符串</param>
    /// <param name="sourceData">源数据（行列表，每行为单元格值列表）</param>
    /// <param name="previewCount">预览行数</param>
    /// <returns>总行数和预览数据行</returns>
    public static FilterPreviewResult PreviewFilter(string filterConfigJson, List<List<object>> sourceData, int previewCount = 5)
    {
        if (sourceData == null || sourceData.Count == 0)
            return new FilterPreviewResult { TotalCount = 0, SampleRows = new List<List<object>>() };

        var matchedIndices = ApplyFilter(filterConfigJson, sourceData);
        var sample = new List<List<object>>();

        for (int i = 0; i < Math.Min(previewCount, matchedIndices.Count); i++)
        {
            sample.Add(sourceData[matchedIndices[i]]);
        }

        return new FilterPreviewResult { TotalCount = matchedIndices.Count, SampleRows = sample };
    }

    /// <summary>
    /// 解析筛选配置 JSON（兼容内部格式和 UI 格式）
    /// </summary>
    private static FilterConfig ParseFilterConfig(string filterConfigJson)
    {
        if (string.IsNullOrWhiteSpace(filterConfigJson))
            return null;

        try
        {
            // 先尝试直接反序列化为标准格式
            var config = JsonConvert.DeserializeObject<FilterConfig>(filterConfigJson);
            var jobj = JObject.Parse(filterConfigJson);
            var conditionsArr = jobj["Conditions"] as JArray;

            if (config != null && config.Conditions != null && config.Conditions.Count > 0)
            {
                // 检查 JSON 中是否存在 UI 格式特有字段（"Column" 或 "Value1" 或 "Logic"）
                if (conditionsArr != null && conditionsArr.Count > 0)
                {
                    var first = conditionsArr[0];
                    bool hasUiField = first["Column"] != null || first["Value1"] != null || first["Logic"] != null;
                    if (!hasUiField)
                        return config; // 纯标准格式，直接返回
                }
                // 否则继续走 JObject 兼容解析
            }

            // 兼容 UI 格式：{"Conditions": [{"Column": "列名", "Operator": "等于", "Value1": "...", "Value2": "...", "Logic": "And"}]}
            var result = new FilterConfig();
            if (conditionsArr == null)
                return config; // 回退到直接反序列化结果

            foreach (var item in conditionsArr)
            {
                var condition = new FilterCondition();

                // ColumnIndex：UI 用 "Column"(列名) 或 "ColumnIndex"(数字)
                var colToken = item["ColumnIndex"];
                if (colToken != null)
                {
                    condition.ColumnIndex = colToken.Value<int>();
                }
                else
                {
                    // UI 格式没有 ColumnIndex，暂时用 -1 标记，需要外部传入列名→索引映射
                    // 这里用 0 作为默认值，实际执行时由 Manager 负责转换
                    condition.ColumnIndex = 0;
                }

                // Operator：UI 用中文字符串，内部用枚举
                var opToken = item["Operator"];
                if (opToken != null)
                {
                    if (opToken.Type == JTokenType.Integer)
                    {
                        condition.Operator = (FilterOperator)opToken.Value<int>();
                    }
                    else
                    {
                        condition.Operator = ParseOperatorString(opToken.Value<string>());
                    }
                }

                // Value：UI 用 "Value1"，内部用 "Value"
                var valToken = item["Value"] ?? item["Value1"];
                condition.Value = valToken?.Value<string>();

                // Value2
                condition.Value2 = item["Value2"]?.Value<string>();

                // Relation：UI 用 "Logic"，内部用 "Relation"
                var relToken = item["Relation"] ?? item["Logic"];
                condition.Relation = relToken?.Value<string>() ?? "And";

                result.Conditions.Add(condition);
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 将中文运算符字符串解析为枚举
    /// </summary>
    private static FilterOperator ParseOperatorString(string op)
    {
        return op switch
        {
            "等于" => FilterOperator.Equals,
            "不等于" => FilterOperator.NotEquals,
            "大于" => FilterOperator.GreaterThan,
            "大于等于" => FilterOperator.GreaterThanOrEquals,
            "小于" => FilterOperator.LessThan,
            "小于等于" => FilterOperator.LessThanOrEquals,
            "区间" => FilterOperator.Between,
            "包含" => FilterOperator.Contains,
            "开头是" => FilterOperator.StartsWith,
            "结尾是" => FilterOperator.EndsWith,
            "为空" => FilterOperator.Empty,
            "非空" => FilterOperator.NotEmpty,
            "为空或零" => FilterOperator.EmptyOrZero,
            "非空且非零" => FilterOperator.NotEmptyOrZero,
            _ => FilterOperator.Equals
        };
    }

    /// <summary>
    /// 判断某行数据是否满足筛选条件
    /// </summary>
    private static bool EvaluateRow(List<object> row, FilterConfig config)
    {
        // 当有 ConditionGroups 时：每组内 And，组间 Or
        if (config.ConditionGroups != null && config.ConditionGroups.Count > 0)
        {
            foreach (var group in config.ConditionGroups)
            {
                if (group == null || group.Count == 0)
                    continue;

                bool groupResult = true;
                foreach (var condition in group)
                {
                    if (!EvaluateCondition(row, condition))
                    {
                        groupResult = false;
                        break;
                    }
                }

                if (groupResult)
                    return true;
            }

            return false;
        }

        // 没有 ConditionGroups 时：所有 Conditions 按 Relation (And/Or) 组合判断
        if (config.Conditions == null || config.Conditions.Count == 0)
            return true;

        bool result = false;
        string currentRelation = "And";

        for (int i = 0; i < config.Conditions.Count; i++)
        {
            var condition = config.Conditions[i];
            bool conditionResult = EvaluateCondition(row, condition);

            if (i == 0)
            {
                result = conditionResult;
            }
            else
            {
                if (string.Equals(currentRelation, "Or", StringComparison.OrdinalIgnoreCase))
                    result = result || conditionResult;
                else
                    result = result && conditionResult;
            }

            // 记录下一个条件的 Relation（当前条件的 Relation 表示与下一个条件的关系）
            currentRelation = condition.Relation ?? "And";
        }

        return result;
    }

    /// <summary>
    /// 判断某行数据是否满足单个筛选条件
    /// </summary>
    private static bool EvaluateCondition(List<object> row, FilterCondition condition)
    {
        if (condition == null)
            return true;

        // 列索引越界视为条件不满足
        if (condition.ColumnIndex < 0 || condition.ColumnIndex >= row.Count)
            return false;

        var cellValue = row[condition.ColumnIndex];

        switch (condition.Operator)
        {
            case FilterOperator.Equals:
                return CompareEquals(cellValue, condition.Value);

            case FilterOperator.NotEquals:
                return !CompareEquals(cellValue, condition.Value);

            case FilterOperator.GreaterThan:
                return CompareNumeric(cellValue, condition.Value) > 0;

            case FilterOperator.GreaterThanOrEquals:
                return CompareNumeric(cellValue, condition.Value) >= 0;

            case FilterOperator.LessThan:
                return CompareNumeric(cellValue, condition.Value) < 0;

            case FilterOperator.LessThanOrEquals:
                return CompareNumeric(cellValue, condition.Value) <= 0;

            case FilterOperator.Between:
                return CompareNumeric(cellValue, condition.Value) >= 0
                    && CompareNumeric(cellValue, condition.Value2) <= 0;

            case FilterOperator.Contains:
                return ContainsString(cellValue, condition.Value);

            case FilterOperator.StartsWith:
                return StartsWithString(cellValue, condition.Value);

            case FilterOperator.EndsWith:
                return EndsWithString(cellValue, condition.Value);

            case FilterOperator.Empty:
                return IsEmpty(cellValue);

            case FilterOperator.NotEmpty:
                return !IsEmpty(cellValue);

            case FilterOperator.EmptyOrZero:
                return IsEmptyOrZero(cellValue);

            case FilterOperator.NotEmptyOrZero:
                return !IsEmptyOrZero(cellValue);

            default:
                return true;
        }
    }

    /// <summary>
    /// 比较两个值是否相等（忽略大小写）
    /// </summary>
    private static bool CompareEquals(object a, object b)
    {
        if (a == null && b == null)
            return true;
        if (a == null || b == null)
            return false;

        return string.Equals(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 数值比较，返回 -1/0/1
    /// </summary>
    private static int CompareNumeric(object a, object b)
    {
        try
        {
            double da = Convert.ToDouble(a);
            double db = Convert.ToDouble(b);
            return da.CompareTo(db);
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 字符串包含判断
    /// </summary>
    private static bool ContainsString(object a, object b)
    {
        if (a == null || b == null)
            return false;

        return a.ToString().IndexOf(b.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// 字符串开头判断
    /// </summary>
    private static bool StartsWithString(object a, object b)
    {
        if (a == null || b == null)
            return false;

        return a.ToString().StartsWith(b.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 字符串结尾判断
    /// </summary>
    private static bool EndsWithString(object a, object b)
    {
        if (a == null || b == null)
            return false;

        return a.ToString().EndsWith(b.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 判断是否为空（null 或空字符串）
    /// </summary>
    private static bool IsEmpty(object value)
    {
        return value == null || (value is string s && string.IsNullOrEmpty(s));
    }

    /// <summary>
    /// 判断是否为空或零（null 或空字符串或数值 0）
    /// </summary>
    private static bool IsEmptyOrZero(object value)
    {
        if (value == null)
            return true;

        if (value is string str && string.IsNullOrEmpty(str))
            return true;

        try
        {
            double d = Convert.ToDouble(value);
            return Math.Abs(d) < double.Epsilon;
        }
        catch
        {
            return false;
        }
    }
}