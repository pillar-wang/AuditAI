using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Auditai.DTO;

namespace Auditai.Model;

/// <summary>
/// 跨项目数据引用的公式运算引擎
/// </summary>
public static class CrossProjectDataRefCompute
{
    /// <summary>
    /// 公式验证结果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// 数据来源定义
    /// </summary>
    public class DataSource
    {
        /// <summary>A, B, C 标识名</summary>
        public string Name { get; set; }

        /// <summary>来源项目 ID</summary>
        public Guid ProjectId { get; set; }

        /// <summary>来源表 ID</summary>
        public Id64 TableId { get; set; }

        /// <summary>来源列 ID</summary>
        public Id64 ColumnId { get; set; }

        /// <summary>该列所有行的数值</summary>
        public List<double> RowValues { get; set; }
    }

    /// <summary>
    /// 公式运算结果
    /// </summary>
    public class ComputeResult
    {
        /// <summary>是否成功</summary>
        public bool Success { get; set; }

        /// <summary>错误信息</summary>
        public string Error { get; set; }

        /// <summary>每行一个计算结果</summary>
        public List<double> Results { get; set; }
    }

    private static readonly Regex DataRefRegex = new Regex(@"\[([A-Za-z0-9]+)\]", RegexOptions.Compiled);

    private static readonly Regex AggregateFuncRegex = new Regex(@"(SUM|AVG|MAX|MIN)\s*\(\s*\[([A-Za-z0-9]+)\]\s*\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex IfFuncRegex = new Regex(@"IF\s*\((.+?),\s*(.+?),\s*(.+)\)\s*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// 执行公式运算，逐行求值
    /// </summary>
    /// <param name="expression">公式表达式</param>
    /// <param name="dataSources">数据源列表</param>
    /// <returns>每行一个计算结果</returns>
    public static ComputeResult Compute(string expression, List<DataSource> dataSources)
    {
        var result = new ComputeResult
        {
            Success = true,
            Results = new List<double>()
        };

        if (dataSources == null || dataSources.Count == 0)
        {
            return result;
        }

        try
        {
            // 确定行数：取所有数据源的最小行数
            int rowCount = dataSources.Min(ds => ds.RowValues?.Count ?? 0);
            if (rowCount == 0)
            {
                return result;
            }

            // 构建数据源名称到数据源的映射
            var sourceMap = dataSources.ToDictionary(ds => ds.Name, StringComparer.OrdinalIgnoreCase);

            // 预处理：计算聚合函数值
            string processedExpr = PreprocessAggregates(expression, sourceMap);

            // 预处理：计算 IF 函数
            processedExpr = PreprocessIfFunctions(processedExpr, sourceMap);

            // 逐行计算
            for (int i = 0; i < rowCount; i++)
            {
                double rowResult = ComputeRowInternal(processedExpr, sourceMap, i);
                result.Results.Add(rowResult);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// 对单行数据进行公式求值
    /// </summary>
    /// <param name="expression">公式表达式（已预处理过的，不含聚合函数和 IF）</param>
    /// <param name="rowValues">行数据，键为数据源名称(A, B, C)，值为数值</param>
    /// <returns>计算结果</returns>
    public static double ComputeRow(string expression, Dictionary<string, double> rowValues)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return 0;
        }

        // 替换数据源引用为实际数值
        string evalExpr = DataRefRegex.Replace(expression, match =>
        {
            string name = match.Groups[1].Value;
            if (rowValues.TryGetValue(name, out double val))
            {
                return val.ToString(CultureInfo.InvariantCulture);
            }
            throw new FormulaSyntaxException($"未找到数据源 [{name}] 的值", 0);
        });

        return EvaluateArithmetic(evalExpr);
    }

    /// <summary>
    /// 验证公式语法
    /// </summary>
    /// <param name="expression">公式表达式</param>
    /// <param name="dataSources">数据源列表</param>
    /// <returns>验证结果</returns>
    public static ValidationResult ValidateExpression(string expression, List<DataSource> dataSources)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return new ValidationResult { IsValid = false, Error = "表达式不能为空" };
        }

        if (dataSources == null || dataSources.Count == 0)
        {
            return new ValidationResult { IsValid = false, Error = "数据源列表不能为空" };
        }

        try
        {
            var sourceMap = dataSources.ToDictionary(ds => ds.Name, StringComparer.OrdinalIgnoreCase);

            // 验证所有 [X] 引用都有对应的数据源
            var refMatches = DataRefRegex.Matches(expression);
            foreach (Match match in refMatches)
            {
                string name = match.Groups[1].Value;
                if (!sourceMap.ContainsKey(name))
                {
                    return new ValidationResult { IsValid = false, Error = $"未找到标识名为 [{name}] 的数据源" };
                }
            }

            // 尝试验证聚合函数内的引用
            var aggMatches = AggregateFuncRegex.Matches(expression);
            foreach (Match match in aggMatches)
            {
                string name = match.Groups[2].Value;
                if (!sourceMap.ContainsKey(name))
                {
                    return new ValidationResult { IsValid = false, Error = $"聚合函数中未找到标识名为 [{name}] 的数据源" };
                }
            }

            // 尝试进行一次完整的计算来验证语法
            ComputeResult computeResult = Compute(expression, dataSources);
            if (!computeResult.Success)
            {
                return new ValidationResult { IsValid = false, Error = computeResult.Error };
            }

            return new ValidationResult { IsValid = true, Error = null };
        }
        catch (Exception ex)
        {
            return new ValidationResult { IsValid = false, Error = $"公式语法错误: {ex.Message}" };
        }
    }

    /// <summary>
    /// 预处理聚合函数（SUM, AVG, MAX, MIN），用计算结果替换函数调用
    /// </summary>
    private static string PreprocessAggregates(string expression, Dictionary<string, DataSource> sourceMap)
    {
        return AggregateFuncRegex.Replace(expression, match =>
        {
            string funcName = match.Groups[1].Value.ToUpperInvariant();
            string sourceName = match.Groups[2].Value;

            if (!sourceMap.TryGetValue(sourceName, out DataSource ds))
            {
                throw new FormulaSyntaxException($"聚合函数中未找到标识名为 [{sourceName}] 的数据源", 0);
            }

            var values = ds.RowValues ?? new List<double>();
            if (values.Count == 0)
            {
                return "0";
            }

            double result = funcName switch
            {
                "SUM" => values.Sum(),
                "AVG" => values.Average(),
                "MAX" => values.Max(),
                "MIN" => values.Min(),
                _ => throw new FormulaSyntaxException($"不支持的聚合函数: {funcName}", 0)
            };

            return result.ToString(CultureInfo.InvariantCulture);
        });
    }

    /// <summary>
    /// 预处理 IF 函数，根据条件计算结果替换函数调用
    /// </summary>
    private static string PreprocessIfFunctions(string expression, Dictionary<string, DataSource> sourceMap)
    {
        // 需要反复替换，因为 IF 可能嵌套
        int maxIterations = 100;
        while (IfFuncRegex.IsMatch(expression) && maxIterations-- > 0)
        {
            expression = IfFuncRegex.Replace(expression, match =>
            {
                string condition = match.Groups[1].Value.Trim();
                string trueVal = match.Groups[2].Value.Trim();
                string falseVal = match.Groups[3].Value.Trim();

                // 解析条件中的 [X] 引用
                string resolvedCondition = DataRefRegex.Replace(condition, m =>
                {
                    string name = m.Groups[1].Value;
                    // IF 预处理阶段无法获得行号，所以将 [X] 替换为数据源的第一个值仅用于语法验证
                    // 在实际 ComputeRowInternal 中重新处理
                    return m.Value;
                });

                // 计算条件结果（需要行上下文，此处暂返回 placeholder 后由逐行引擎处理）
                // 返回特殊标记让逐行引擎处理
                return $"__IF({condition},{trueVal},{falseVal})";
            });
        }
        return expression;
    }

    /// <summary>
    /// 内部逐行计算，处理 IF 函数和数据源引用
    /// </summary>
    private static double ComputeRowInternal(string expression, Dictionary<string, DataSource> sourceMap, int rowIndex)
    {
        string rowExpr = expression;

        // 检查数据源行索引有效性
        foreach (var kvp in sourceMap)
        {
            var values = kvp.Value.RowValues;
            if (values == null || rowIndex >= values.Count)
            {
                throw new FormulaSyntaxException($"数据源 [{kvp.Key}] 在第 {rowIndex + 1} 行没有数据", 0);
            }
        }

        // 处理 IF 函数（自底向上处理最简单的 IF）
        rowExpr = ProcessRowIfFunctions(rowExpr, sourceMap, rowIndex);

        // 替换 [X] 引用为当前行的值
        rowExpr = DataRefRegex.Replace(rowExpr, match =>
        {
            string name = match.Groups[1].Value;
            if (sourceMap.TryGetValue(name, out DataSource ds))
            {
                return ds.RowValues[rowIndex].ToString(CultureInfo.InvariantCulture);
            }
            throw new FormulaSyntaxException($"未找到标识名为 [{name}] 的数据源", 0);
        });

        // 算术求值
        return EvaluateArithmetic(rowExpr);
    }

    /// <summary>
    /// 逐行处理 IF 函数
    /// </summary>
    private static string ProcessRowIfFunctions(string expression, Dictionary<string, DataSource> sourceMap, int rowIndex)
    {
        // 处理 __IF(...) 标记
        var ifRegex = new Regex(@"__IF\((.+?),(.+?),(.+)\)", RegexOptions.Compiled);
        int maxIterations = 100;
        while (ifRegex.IsMatch(expression) && maxIterations-- > 0)
        {
            expression = ifRegex.Replace(expression, match =>
            {
                string condition = match.Groups[1].Value.Trim();
                string trueVal = match.Groups[2].Value.Trim();
                string falseVal = match.Groups[3].Value.Trim();

                // 解析条件是否成立
                bool conditionResult = EvaluateCondition(condition, sourceMap, rowIndex);

                return conditionResult ? trueVal : falseVal;
            });
        }
        return expression;
    }

    /// <summary>
    /// 评估条件表达式（如 [A] > 100, [B] == "Y"）
    /// </summary>
    private static bool EvaluateCondition(string condition, Dictionary<string, DataSource> sourceMap, int rowIndex)
    {
        condition = condition.Trim();

        // 解析比较操作符
        string[] operators = { ">=", "<=", "!=", "==", ">", "<" };
        string selectedOp = null;
        int opIndex = -1;

        foreach (string op in operators)
        {
            int idx = condition.IndexOf(op, StringComparison.Ordinal);
            if (idx >= 0)
            {
                // 取最左边的操作符
                if (opIndex < 0 || idx < opIndex)
                {
                    opIndex = idx;
                    selectedOp = op;
                }
            }
        }

        if (selectedOp == null)
        {
            // 没有比较操作符，尝试将条件本身作为数值判断（非零为 true）
            string resolved = ResolveConditionValue(condition, sourceMap, rowIndex);
            if (double.TryParse(resolved, NumberStyles.Any, CultureInfo.InvariantCulture, out double numVal))
            {
                return Math.Abs(numVal) > 1e-15;
            }
            // 字符串非空为 true
            return !string.IsNullOrEmpty(resolved);
        }

        string leftPart = condition.Substring(0, opIndex).Trim();
        string rightPart = condition.Substring(opIndex + selectedOp.Length).Trim();

        string leftValue = ResolveConditionValue(leftPart, sourceMap, rowIndex);
        string rightValue = ResolveConditionValue(rightPart, sourceMap, rowIndex);

        // 尝试数值比较
        if (double.TryParse(leftValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double leftNum) &&
            double.TryParse(rightValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double rightNum))
        {
            return selectedOp switch
            {
                ">" => leftNum > rightNum,
                "<" => leftNum < rightNum,
                ">=" => leftNum >= rightNum,
                "<=" => leftNum <= rightNum,
                "==" => Math.Abs(leftNum - rightNum) < 1e-15,
                "!=" => Math.Abs(leftNum - rightNum) >= 1e-15,
                _ => false
            };
        }

        // 字符串比较
        // 去掉引号
        leftValue = StripQuotes(leftValue);
        rightValue = StripQuotes(rightValue);

        return selectedOp switch
        {
            "==" => string.Equals(leftValue, rightValue, StringComparison.Ordinal),
            "!=" => !string.Equals(leftValue, rightValue, StringComparison.Ordinal),
            ">" => string.Compare(leftValue, rightValue, StringComparison.Ordinal) > 0,
            "<" => string.Compare(leftValue, rightValue, StringComparison.Ordinal) < 0,
            ">=" => string.Compare(leftValue, rightValue, StringComparison.Ordinal) >= 0,
            "<=" => string.Compare(leftValue, rightValue, StringComparison.Ordinal) <= 0,
            _ => false
        };
    }

    /// <summary>
    /// 解析条件中的值（替换 [X] 引用）
    /// </summary>
    private static string ResolveConditionValue(string part, Dictionary<string, DataSource> sourceMap, int rowIndex)
    {
        return DataRefRegex.Replace(part, match =>
        {
            string name = match.Groups[1].Value;
            if (sourceMap.TryGetValue(name, out DataSource ds))
            {
                if (ds.RowValues != null && rowIndex < ds.RowValues.Count)
                {
                    return ds.RowValues[rowIndex].ToString(CultureInfo.InvariantCulture);
                }
            }
            return match.Value;
        });
    }

    /// <summary>
    /// 去掉字符串两端的引号
    /// </summary>
    private static string StripQuotes(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
            (value.StartsWith("'") && value.EndsWith("'")))
        {
            return value.Substring(1, value.Length - 2);
        }
        return value;
    }

    /// <summary>
    /// 使用 DataTable.Compute 进行算术求值
    /// </summary>
    private static double EvaluateArithmetic(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return 0;
        }

        try
        {
            var dt = new DataTable();
            object result = dt.Compute(expression, null);
            return Convert.ToDouble(result, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new FormulaSyntaxException($"算术求值失败: {expression} - {ex.Message}", 0);
        }
    }
}