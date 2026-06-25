﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 凭证查询服务层
    /// 提供精准的凭证查询功能，包括按凭证号查询、凭证分录明细、按日期范围查询
    /// </summary>
    public static class VoucherService
    {
        /// <summary>
        /// 按凭证号+凭证类型+期间精准查询单张凭证
        /// </summary>
        public static string GetVoucherByNumber(long projectId, string number, string voucherType, int? periodYear, int? periodMonth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(number))
                    return ErrorJson("凭证号不能为空");

                string dbPath = ResolveLedgerPath();

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 查询凭证类型ID
                    int? typeId = null;
                    if (!string.IsNullOrWhiteSpace(voucherType))
                    {
                        cmd.CommandText = "SELECT id FROM VoucherType WHERE name = @typeName";
                        cmd.Parameters.AddWithValue("@typeName", voucherType);
                        var typeIdResult = cmd.ExecuteScalar();
                        if (typeIdResult != null && typeIdResult != DBNull.Value)
                            typeId = Convert.ToInt32(typeIdResult);
                        else
                            return ErrorJson($"未找到凭证类型: {voucherType}");
                    }

                    // 查询该凭证号的所有分录行
                    var sql = new System.Text.StringBuilder();
                    sql.Append("SELECT v.id, v.number, v.day, v.digest, v.dc, v.amount, ");
                    sql.Append("v.maker, v.checker, v.booker, v.OppositeAccounts, ");
                    sql.Append("a.code AS accountCode, a.name AS accountName, ");
                    sql.Append("vt.name AS typeName, v.type, v.accountId ");
                    sql.Append("FROM Voucher v ");
                    sql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    sql.Append("LEFT JOIN VoucherType vt ON v.type = vt.id ");
                    sql.Append("WHERE v.number = @number");
                    cmd.Parameters.AddWithValue("@number", number);

                    if (typeId.HasValue)
                    {
                        sql.Append(" AND v.type = @typeId");
                        cmd.Parameters.AddWithValue("@typeId", typeId.Value);
                    }

                    if (periodYear.HasValue)
                    {
                        sql.Append(" AND strftime('%Y', v.day) = @year");
                        cmd.Parameters.AddWithValue("@year", periodYear.Value.ToString("D4"));
                    }

                    if (periodMonth.HasValue)
                    {
                        sql.Append(" AND strftime('%m', v.day) = @month");
                        cmd.Parameters.AddWithValue("@month", periodMonth.Value.ToString("D2"));
                    }

                    sql.Append(" ORDER BY v.id");
                    cmd.CommandText = sql.ToString();

                    var entries = new List<JObject>();
                    string voucherNumber = null;
                    DateTime? voucherDate = null;
                    string voucherTypeName = null;
                    string maker = null;
                    string checker = null;
                    string poster = null;
                    decimal totalDebit = 0;
                    decimal totalCredit = 0;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bool isDebit = reader.GetBoolean(4);
                            decimal amount = reader.GetDecimal(5);

                            if (voucherNumber == null)
                            {
                                voucherNumber = reader.IsDBNull(1) ? null : reader.GetString(1);
                                voucherDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);
                                voucherTypeName = reader.IsDBNull(12) ? null : reader.GetString(12);
                                maker = reader.IsDBNull(6) ? null : reader.GetString(6);
                                checker = reader.IsDBNull(7) ? null : reader.GetString(7);
                                poster = reader.IsDBNull(8) ? null : reader.GetString(8);
                            }

                            if (isDebit)
                                totalDebit += amount;
                            else
                                totalCredit += amount;

                            entries.Add(new JObject
                            {
                                ["entry_id"] = reader.GetInt32(0),
                                ["account_code"] = reader.IsDBNull(10) ? null : reader.GetString(10),
                                ["account_name"] = reader.IsDBNull(11) ? null : reader.GetString(11),
                                ["direction"] = isDebit ? "借" : "贷",
                                ["amount"] = amount,
                                ["summary"] = reader.IsDBNull(3) ? null : reader.GetString(3),
                                ["opposite_accounts"] = reader.IsDBNull(9) ? null : reader.GetString(9)
                            });
                        }
                    }

                    if (voucherNumber == null)
                    {
                        return ErrorJson($"未找到凭证号: {number}" + (typeId.HasValue ? $" (类型: {voucherType})" : ""));
                    }

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["voucher"] = new JObject
                        {
                            ["voucher_id"] = entries.Count > 0 ? entries[0]["entry_id"] : 0,
                            ["number"] = voucherNumber,
                            ["date"] = voucherDate?.ToString("yyyy-MM-dd"),
                            ["type"] = voucherTypeName,
                            ["summary"] = entries.Count > 0 ? entries[0]["summary"] : null,
                            ["total_debit"] = totalDebit,
                            ["total_credit"] = totalCredit,
                            ["attachment_count"] = null,
                            ["maker"] = maker,
                            ["reviewer"] = checker,
                            ["poster"] = poster
                        },
                        ["entries"] = new JArray(entries),
                        ["entry_count"] = entries.Count
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("按凭证号查询失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取指定凭证的完整分录明细
        /// </summary>
        public static string GetVoucherDetail(long projectId, long voucherId)
        {
            try
            {
                string dbPath = ResolveLedgerPath();

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 先查询该分录行所属的凭证号
                    cmd.CommandText = "SELECT number FROM Voucher WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", voucherId);
                    var numberResult = cmd.ExecuteScalar();
                    if (numberResult == null || numberResult == DBNull.Value)
                        return ErrorJson($"未找到凭证分录: {voucherId}");

                    string number = Convert.ToString(numberResult);

                    // 查询该凭证号的所有分录行
                    cmd.CommandText = @"
                        SELECT v.id, v.number, v.day, v.digest, v.dc, v.amount,
                               v.maker, v.checker, v.booker, v.OppositeAccounts,
                               a.code AS accountCode, a.name AS accountName,
                               vt.name AS typeName
                        FROM Voucher v
                        LEFT JOIN Account a ON v.accountId = a.id
                        LEFT JOIN VoucherType vt ON v.type = vt.id
                        WHERE v.number = @number
                        ORDER BY v.id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@number", number);

                    var entries = new List<JObject>();
                    string voucherNumber = null;
                    DateTime? voucherDate = null;
                    string voucherTypeName = null;
                    string maker = null;
                    string reviewer = null;
                    string poster = null;
                    string summary = null;
                    decimal totalDebit = 0;
                    decimal totalCredit = 0;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long entryId = reader.GetInt32(0);
                            bool isDebit = reader.GetBoolean(4);
                            decimal amount = reader.GetDecimal(5);

                            if (voucherNumber == null)
                            {
                                voucherNumber = reader.IsDBNull(1) ? null : reader.GetString(1);
                                voucherDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);
                                voucherTypeName = reader.IsDBNull(12) ? null : reader.GetString(12);
                                maker = reader.IsDBNull(6) ? null : reader.GetString(6);
                                reviewer = reader.IsDBNull(7) ? null : reader.GetString(7);
                                poster = reader.IsDBNull(8) ? null : reader.GetString(8);
                            }

                            if (summary == null)
                                summary = reader.IsDBNull(3) ? null : reader.GetString(3);

                            if (isDebit)
                                totalDebit += amount;
                            else
                                totalCredit += amount;

                            var entry = new JObject
                            {
                                ["entry_id"] = entryId,
                                ["account_code"] = reader.IsDBNull(10) ? null : reader.GetString(10),
                                ["account_name"] = reader.IsDBNull(11) ? null : reader.GetString(11),
                                ["debit_amount"] = isDebit ? amount : 0m,
                                ["credit_amount"] = isDebit ? 0m : amount,
                                ["summary"] = reader.IsDBNull(3) ? null : reader.GetString(3),
                                ["opposite_account"] = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };

                            entries.Add(entry);
                        }
                    }

                    if (voucherNumber == null)
                        return ErrorJson($"未找到凭证分录: {voucherId}");

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["voucher"] = new JObject
                        {
                            ["voucher_id"] = voucherId,
                            ["number"] = voucherNumber,
                            ["date"] = voucherDate?.ToString("yyyy-MM-dd"),
                            ["type"] = voucherTypeName,
                            ["summary"] = summary,
                            ["total_debit"] = totalDebit,
                            ["total_credit"] = totalCredit,
                            ["maker"] = maker,
                            ["reviewer"] = reviewer,
                            ["poster"] = poster
                        },
                        ["entries"] = new JArray(entries),
                        ["entry_count"] = entries.Count
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("获取凭证分录明细失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 按日期范围查询凭证列表，支持分页和科目筛选
        /// </summary>
        public static string GetVouchersByDate(long projectId, string startDate, string endDate, string accountCode, int page = 1, int pageSize = 50)
        {
            try
            {
                string dbPath = ResolveLedgerPath();

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 先查询符合条件的凭证号列表（去重）
                    var countSql = new System.Text.StringBuilder();
                    countSql.Append("SELECT COUNT(DISTINCT v.number) FROM Voucher v ");
                    countSql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    countSql.Append("WHERE 1=1");

                    if (!string.IsNullOrWhiteSpace(startDate))
                    {
                        countSql.Append(" AND v.day >= @startDate");
                    }
                    if (!string.IsNullOrWhiteSpace(endDate))
                    {
                        countSql.Append(" AND v.day <= @endDate");
                    }
                    if (!string.IsNullOrWhiteSpace(accountCode))
                    {
                        countSql.Append(" AND a.code = @accountCode");
                    }

                    cmd.CommandText = countSql.ToString();
                    if (!string.IsNullOrWhiteSpace(startDate))
                        cmd.Parameters.AddWithValue("@startDate", DateTime.Parse(startDate));
                    if (!string.IsNullOrWhiteSpace(endDate))
                        cmd.Parameters.AddWithValue("@endDate", DateTime.Parse(endDate));
                    if (!string.IsNullOrWhiteSpace(accountCode))
                        cmd.Parameters.AddWithValue("@accountCode", accountCode);

                    int totalCount = Convert.ToInt32(cmd.ExecuteScalar());

                    // 查询分页后的凭证号列表
                    var pageSql = new System.Text.StringBuilder();
                    pageSql.Append("SELECT v.number FROM Voucher v ");
                    pageSql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    pageSql.Append("WHERE 1=1");

                    if (!string.IsNullOrWhiteSpace(startDate))
                        pageSql.Append(" AND v.day >= @startDate");
                    if (!string.IsNullOrWhiteSpace(endDate))
                        pageSql.Append(" AND v.day <= @endDate");
                    if (!string.IsNullOrWhiteSpace(accountCode))
                        pageSql.Append(" AND a.code = @accountCode");

                    pageSql.Append(" GROUP BY v.number ORDER BY MIN(v.day), v.number");
                    pageSql.Append(" LIMIT @limit OFFSET @offset");

                    cmd.Parameters.Clear();
                    if (!string.IsNullOrWhiteSpace(startDate))
                        cmd.Parameters.AddWithValue("@startDate", DateTime.Parse(startDate));
                    if (!string.IsNullOrWhiteSpace(endDate))
                        cmd.Parameters.AddWithValue("@endDate", DateTime.Parse(endDate));
                    if (!string.IsNullOrWhiteSpace(accountCode))
                        cmd.Parameters.AddWithValue("@accountCode", accountCode);
                    cmd.Parameters.AddWithValue("@limit", pageSize);
                    cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);

                    cmd.CommandText = pageSql.ToString();
                    var voucherNumbers = new List<string>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            voucherNumbers.Add(reader.GetString(0));
                        }
                    }

                    if (voucherNumbers.Count == 0)
                    {
                        var emptyResult = new JObject
                        {
                            ["success"] = true,
                            ["project_id"] = projectId,
                            ["total"] = 0,
                            ["page"] = page,
                            ["page_size"] = pageSize,
                            ["total_pages"] = 0,
                            ["vouchers"] = new JArray()
                        };
                        return JsonConvert.SerializeObject(emptyResult, Formatting.Indented);
                    }

                    // 查询这些凭证号的所有分录行
                    var placeholders = voucherNumbers.Select((_, i) => $"@n{i}").ToList();
                    var detailSql = new System.Text.StringBuilder();
                    detailSql.Append("SELECT v.id, v.number, v.day, v.digest, v.dc, v.amount, ");
                    detailSql.Append("v.maker, v.checker, v.booker, ");
                    detailSql.Append("a.code AS accountCode, a.name AS accountName, ");
                    detailSql.Append("vt.name AS typeName ");
                    detailSql.Append("FROM Voucher v ");
                    detailSql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    detailSql.Append("LEFT JOIN VoucherType vt ON v.type = vt.id ");
                    detailSql.Append("WHERE v.number IN (" + string.Join(",", placeholders) + ") ");
                    detailSql.Append("ORDER BY v.day, v.number, v.id");

                    cmd.Parameters.Clear();
                    for (int i = 0; i < voucherNumbers.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@n{i}", voucherNumbers[i]);
                    }
                    cmd.CommandText = detailSql.ToString();

                    // 按凭证号分组
                    var voucherDict = new Dictionary<string, JObject>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string num = reader.GetString(1);
                            bool isDebit = reader.GetBoolean(4);
                            decimal amount = reader.GetDecimal(5);

                            if (!voucherDict.TryGetValue(num, out JObject voucher))
                            {
                                voucher = new JObject
                                {
                                    ["id"] = reader.GetInt32(0),
                                    ["number"] = num,
                                    ["date"] = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                    ["type"] = reader.IsDBNull(11) ? null : reader.GetString(11),
                                    ["total_debit"] = 0m,
                                    ["total_credit"] = 0m,
                                    ["maker"] = reader.IsDBNull(6) ? null : reader.GetString(6)
                                };
                                voucherDict[num] = voucher;
                            }

                            if (isDebit)
                                voucher["total_debit"] = (decimal)voucher["total_debit"] + amount;
                            else
                                voucher["total_credit"] = (decimal)voucher["total_credit"] + amount;
                        }
                    }

                    var vouchers = voucherNumbers
                        .Where(n => voucherDict.ContainsKey(n))
                        .Select(n => voucherDict[n])
                        .ToList();

                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["total"] = totalCount,
                        ["page"] = page,
                        ["page_size"] = pageSize,
                        ["total_pages"] = totalPages,
                        ["vouchers"] = new JArray(vouchers)
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("按日期范围查询凭证失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 多条件组合查询凭证
        /// 支持按科目、期间、金额范围、摘要关键词、凭证类型等多种条件组合查询
        /// </summary>
        public static string QueryVouchers(long projectId,
            string accountCode = null,
            string startDate = null, string endDate = null,
            decimal? minAmount = null, decimal? maxAmount = null,
            string digestKeyword = null,
            string voucherType = null,
            int page = 1, int pageSize = 50)
        {
            try
            {
                string dbPath = ResolveLedgerPath();

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 50;

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 查询凭证类型ID（可选）
                    int? typeId = null;
                    if (!string.IsNullOrWhiteSpace(voucherType))
                    {
                        cmd.CommandText = "SELECT id FROM VoucherType WHERE name = @typeName";
                        cmd.Parameters.AddWithValue("@typeName", voucherType);
                        var typeIdResult = cmd.ExecuteScalar();
                        if (typeIdResult != null && typeIdResult != DBNull.Value)
                            typeId = Convert.ToInt32(typeIdResult);
                    }

                    // 构建 WHERE 条件
                    var whereClauses = new List<string>();
                    var countParams = new List<SQLiteParameter>();

                    if (!string.IsNullOrWhiteSpace(startDate))
                    {
                        whereClauses.Add("v.day >= @startDate");
                        countParams.Add(new SQLiteParameter("@startDate", DateTime.Parse(startDate)));
                    }
                    if (!string.IsNullOrWhiteSpace(endDate))
                    {
                        whereClauses.Add("v.day <= @endDate");
                        countParams.Add(new SQLiteParameter("@endDate", DateTime.Parse(endDate)));
                    }
                    if (!string.IsNullOrWhiteSpace(accountCode))
                    {
                        whereClauses.Add("a.code = @accountCode");
                        countParams.Add(new SQLiteParameter("@accountCode", accountCode));
                    }
                    if (minAmount.HasValue)
                    {
                        whereClauses.Add("v.amount >= @minAmount");
                        countParams.Add(new SQLiteParameter("@minAmount", minAmount.Value));
                    }
                    if (maxAmount.HasValue)
                    {
                        whereClauses.Add("v.amount <= @maxAmount");
                        countParams.Add(new SQLiteParameter("@maxAmount", maxAmount.Value));
                    }
                    if (!string.IsNullOrWhiteSpace(digestKeyword))
                    {
                        whereClauses.Add("v.digest LIKE @keyword");
                        countParams.Add(new SQLiteParameter("@keyword", "%" + digestKeyword + "%"));
                    }
                    if (typeId.HasValue)
                    {
                        whereClauses.Add("v.type = @typeId");
                        countParams.Add(new SQLiteParameter("@typeId", typeId.Value));
                    }

                    string whereSql = whereClauses.Count > 0
                        ? " AND " + string.Join(" AND ", whereClauses)
                        : "";

                    // 先查询符合条件的唯一凭证总数（去重）
                    cmd.Parameters.Clear();
                    var countSql = new System.Text.StringBuilder();
                    countSql.Append("SELECT COUNT(DISTINCT v.number) FROM Voucher v ");
                    countSql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    countSql.Append("WHERE 1=1");
                    countSql.Append(whereSql);
                    foreach (var p in countParams)
                        cmd.Parameters.Add(p);

                    cmd.CommandText = countSql.ToString();
                    int totalCount = Convert.ToInt32(cmd.ExecuteScalar());

                    if (totalCount == 0)
                    {
                        var emptyResult = new JObject
                        {
                            ["success"] = true,
                            ["project_id"] = projectId,
                            ["query_conditions"] = BuildConditionJObject(accountCode, startDate, endDate, minAmount, maxAmount, digestKeyword, voucherType),
                            ["total"] = 0,
                            ["page"] = page,
                            ["page_size"] = pageSize,
                            ["total_pages"] = 0,
                            ["vouchers"] = new JArray()
                        };
                        return JsonConvert.SerializeObject(emptyResult, Formatting.Indented);
                    }

                    // 查询分页后的凭证号列表
                    cmd.Parameters.Clear();
                    foreach (var p in countParams)
                        cmd.Parameters.Add(p);

                    var pageSql = new System.Text.StringBuilder();
                    pageSql.Append("SELECT v.number FROM Voucher v ");
                    pageSql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    pageSql.Append("WHERE 1=1");
                    pageSql.Append(whereSql);
                    pageSql.Append(" GROUP BY v.number ORDER BY MIN(v.day), v.number");
                    pageSql.Append(" LIMIT @limit OFFSET @offset");
                    cmd.Parameters.AddWithValue("@limit", pageSize);
                    cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
                    cmd.CommandText = pageSql.ToString();

                    var voucherNumbers = new List<string>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            voucherNumbers.Add(reader.GetString(0));
                    }

                    if (voucherNumbers.Count == 0)
                    {
                        var emptyResult = new JObject
                        {
                            ["success"] = true,
                            ["project_id"] = projectId,
                            ["query_conditions"] = BuildConditionJObject(accountCode, startDate, endDate, minAmount, maxAmount, digestKeyword, voucherType),
                            ["total"] = totalCount,
                            ["page"] = page,
                            ["page_size"] = pageSize,
                            ["total_pages"] = (int)Math.Ceiling((double)totalCount / pageSize),
                            ["vouchers"] = new JArray()
                        };
                        return JsonConvert.SerializeObject(emptyResult, Formatting.Indented);
                    }

                    // 查询这些凭证号的所有分录行，按凭证号分组聚合
                    var placeholders = voucherNumbers.Select((_, i) => $"@n{i}").ToList();
                    cmd.Parameters.Clear();
                    for (int i = 0; i < voucherNumbers.Count; i++)
                        cmd.Parameters.AddWithValue($"@n{i}", voucherNumbers[i]);

                    var detailSql = new System.Text.StringBuilder();
                    detailSql.Append("SELECT v.id, v.number, v.day, v.digest, v.dc, v.amount, ");
                    detailSql.Append("v.maker, v.checker, v.booker, ");
                    detailSql.Append("a.code AS accountCode, a.name AS accountName, ");
                    detailSql.Append("vt.name AS typeName ");
                    detailSql.Append("FROM Voucher v ");
                    detailSql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    detailSql.Append("LEFT JOIN VoucherType vt ON v.type = vt.id ");
                    detailSql.Append("WHERE v.number IN (" + string.Join(",", placeholders) + ") ");
                    detailSql.Append("ORDER BY v.day, v.number, v.id");
                    cmd.CommandText = detailSql.ToString();

                    var voucherDict = new Dictionary<string, JObject>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string num = reader.GetString(1);
                            bool isDebit = reader.GetBoolean(4);
                            decimal amount = reader.GetDecimal(5);

                            if (!voucherDict.TryGetValue(num, out JObject voucher))
                            {
                                voucher = new JObject
                                {
                                    ["id"] = reader.GetInt32(0),
                                    ["number"] = num,
                                    ["date"] = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                    ["type"] = reader.IsDBNull(11) ? null : reader.GetString(11),
                                    ["total_debit"] = 0m,
                                    ["total_credit"] = 0m,
                                    ["entry_count"] = 0,
                                    ["maker"] = reader.IsDBNull(6) ? null : reader.GetString(6)
                                };
                                voucherDict[num] = voucher;
                            }

                            if (isDebit)
                                voucher["total_debit"] = (decimal)voucher["total_debit"] + amount;
                            else
                                voucher["total_credit"] = (decimal)voucher["total_credit"] + amount;
                            voucher["entry_count"] = (int)voucher["entry_count"] + 1;
                        }
                    }

                    var vouchers = voucherNumbers
                        .Where(n => voucherDict.ContainsKey(n))
                        .Select(n => voucherDict[n])
                        .ToList();

                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["query_conditions"] = BuildConditionJObject(accountCode, startDate, endDate, minAmount, maxAmount, digestKeyword, voucherType),
                        ["total"] = totalCount,
                        ["page"] = page,
                        ["page_size"] = pageSize,
                        ["total_pages"] = totalPages,
                        ["vouchers"] = new JArray(vouchers)
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("多条件查询凭证失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 构建查询条件描述对象
        /// </summary>
        private static JObject BuildConditionJObject(
            string accountCode, string startDate, string endDate,
            decimal? minAmount, decimal? maxAmount,
            string digestKeyword, string voucherType)
        {
            var cond = new JObject();
            if (!string.IsNullOrWhiteSpace(accountCode))
                cond["account_code"] = accountCode;
            if (!string.IsNullOrWhiteSpace(startDate))
                cond["start_date"] = startDate;
            if (!string.IsNullOrWhiteSpace(endDate))
                cond["end_date"] = endDate;
            if (minAmount.HasValue)
                cond["min_amount"] = minAmount.Value;
            if (maxAmount.HasValue)
                cond["max_amount"] = maxAmount.Value;
            if (!string.IsNullOrWhiteSpace(digestKeyword))
                cond["digest_keyword"] = digestKeyword;
            if (!string.IsNullOrWhiteSpace(voucherType))
                cond["voucher_type"] = voucherType;
            return cond;
        }

        // =============================================
        // 辅助方法（与 LedgerService 保持一致）
        // =============================================

        private static string ResolveLedgerPath()
        {
            if (SessionState.Current.HasLedger)
                return SessionState.Current.CurrentLedgerFilePath;
            throw new InvalidOperationException("未导入账簿，请先调用 import_ledger 工具导入账簿文件");
        }

        private static SQLiteConnection OpenConnection(string dbPath)
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbPath,
                ReadOnly = true
            };
            var cnn = new SQLiteConnection(builder.ConnectionString);
            cnn.Open();
            return cnn;
        }

        private static string ErrorJson(string message)
        {
            var result = new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }
    }
}