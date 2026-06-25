﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

/// <summary>
/// 跨项目数据引用验证器
/// </summary>
public static class CrossProjectRefValidator
{
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
    }

    public class ValidationError
    {
        public int RowIndex { get; set; }
        public string ColumnName { get; set; }
        public string Message { get; set; }
    }

    /// <summary>数据类型验证</summary>
    public static bool ValidateDataType(object value, string targetColumnType)
    {
        if (value == null || value == DBNull.Value) return true; // 空值不验证类型
        var typeStr = targetColumnType?.ToLower() ?? "";
        if (typeStr.Contains("decimal") || typeStr.Contains("numeric") || typeStr.Contains("money"))
        {
            return value is sbyte || value is byte || value is short || value is ushort ||
                   value is int || value is uint || value is long || value is ulong ||
                   value is float || value is double || value is decimal;
        }
        if (typeStr.Contains("int"))
        {
            return value is sbyte || value is byte || value is short || value is ushort ||
                   value is int || value is uint || value is long || value is ulong;
        }
        if (typeStr.Contains("date"))
        {
            return value is DateTime;
        }
        return true;
    }

    /// <summary>数值范围验证</summary>
    public static bool ValidateRange(object value, decimal? min, decimal? max)
    {
        if (value == null || value == DBNull.Value) return true;
        if (!min.HasValue && !max.HasValue) return true;
        try
        {
            var val = Convert.ToDecimal(value);
            if (min.HasValue && val < min.Value) return false;
            if (max.HasValue && val > max.Value) return false;
            return true;
        }
        catch { return false; }
    }

    /// <summary>字符串长度验证</summary>
    public static bool ValidateStringLength(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value)) return true;
        return value.Length <= maxLen;
    }

    /// <summary>日期格式验证</summary>
    public static bool ValidateDateTimeFormat(string value, string format)
    {
        if (string.IsNullOrEmpty(value)) return true;
        if (string.IsNullOrEmpty(format)) return true;
        return DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out _);
    }

    /// <summary>批量验证数据</summary>
    public static ValidationResult ValidateData(List<List<object>> rows, List<string> targetColumnNames, 
        Dictionary<string, string> columnTypes = null, Dictionary<string, decimal?> colMins = null, 
        Dictionary<string, decimal?> colMaxs = null)
    {
        var result = new ValidationResult();
        if (rows == null || rows.Count == 0) return result;

        for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
        {
            var row = rows[rowIdx];
            for (int colIdx = 0; colIdx < row.Count && colIdx < targetColumnNames.Count; colIdx++)
            {
                var colName = targetColumnNames[colIdx];
                var value = row[colIdx];
                var valStr = value?.ToString();

                // 数据类型验证
                if (columnTypes != null && columnTypes.ContainsKey(colName))
                {
                    if (!ValidateDataType(value, columnTypes[colName]))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            RowIndex = rowIdx,
                            ColumnName = colName,
                            Message = $"第{rowIdx + 1}行列\"{colName}\"数据类型不符合要求（期望{columnTypes[colName]}）"
                        });
                    }
                }

                // 数值范围验证
                if (colMins != null && colMins.ContainsKey(colName) || colMaxs != null && colMaxs.ContainsKey(colName))
                {
                    var min = colMins?.ContainsKey(colName) == true ? colMins[colName] : null;
                    var max = colMaxs?.ContainsKey(colName) == true ? colMaxs[colName] : null;
                    if (!ValidateRange(value, min, max))
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError
                        {
                            RowIndex = rowIdx,
                            ColumnName = colName,
                            Message = $"第{rowIdx + 1}行列\"{colName}\"数值超出范围"
                        });
                    }
                }

                // 字符串长度验证
                if (valStr != null && valStr.Length > 4000)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError
                    {
                        RowIndex = rowIdx,
                        ColumnName = colName,
                        Message = $"第{rowIdx + 1}行列\"{colName}\"字符串长度({valStr.Length})超过最大长度(4000)"
                    });
                }
            }
        }
        return result;
    }
}