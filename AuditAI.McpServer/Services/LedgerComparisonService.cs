﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 序时账查询和多年度对比服务层
    /// </summary>
    public static class LedgerComparisonService
    {
        /// <summary>
        /// 按日期+凭证号排序返回全量交易，支持科目筛选
        /// </summary>
        public static string GetChronologicalLedger(long projectId, string startDate, string endDate, string accountCode)
        {
            try
            {
                string dbPath = ResolveLedgerPath();

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 如果指定了科目编码，先查询科目信息用于余额计算
                    int? accountId = null;
                    bool? accountIsDebit = null;
                    decimal accountInitialBalance = 0;
                    if (!string.IsNullOrWhiteSpace(accountCode))
                    {
                        cmd.CommandText = "SELECT id, dc, balance FROM Account WHERE code = @code";
                        cmd.Parameters.AddWithValue("@code", accountCode);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                accountId = reader.GetInt32(0);
                                accountIsDebit = reader.GetBoolean(1);
                                accountInitialBalance = reader.GetDecimal(2);
                            }
                            else
                            {
                                return ErrorJson($"未找到科目编码: {accountCode}");
                            }
                        }
                    }

                    // 查询序时账数据
                    var sql = new System.Text.StringBuilder();
                    sql.Append("SELECT v.id, v.number, v.day, v.digest, v.dc, v.amount, ");
                    sql.Append("a.code AS accountCode, a.name AS accountName, ");
                    sql.Append("vt.name AS typeName ");
                    sql.Append("FROM Voucher v ");
                    sql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    sql.Append("LEFT JOIN VoucherType vt ON v.type = vt.id ");
                    sql.Append("WHERE 1=1");

                    if (!string.IsNullOrWhiteSpace(startDate))
                    {
                        sql.Append(" AND v.day >= @startDate");
                    }
                    if (!string.IsNullOrWhiteSpace(endDate))
                    {
                        sql.Append(" AND v.day <= @endDate");
                    }
                    if (accountId.HasValue)
                    {
                        sql.Append(" AND v.accountId = @accountId");
                    }

                    sql.Append(" ORDER BY v.day, v.number, v.id");

                    cmd.Parameters.Clear();
                    if (!string.IsNullOrWhiteSpace(startDate))
                        cmd.Parameters.AddWithValue("@startDate", DateTime.Parse(startDate));
                    if (!string.IsNullOrWhiteSpace(endDate))
                        cmd.Parameters.AddWithValue("@endDate", DateTime.Parse(endDate));
                    if (accountId.HasValue)
                        cmd.Parameters.AddWithValue("@accountId", accountId.Value);

                    cmd.CommandText = sql.ToString();

                    var entries = new List<JObject>();
                    // 如果指定了科目且为单一科目，计算累计余额
                    bool computeBalance = accountId.HasValue;
                    DateTime startDt = string.IsNullOrWhiteSpace(startDate) ? DateTime.MinValue : DateTime.Parse(startDate);
                    decimal runningBalance = accountInitialBalance;

                    // 计算期初余额：建账余额 + 建账日至开始日期前的发生额
                    if (computeBalance && accountId.HasValue && !string.IsNullOrWhiteSpace(startDate))
                    {
                        cmd.CommandText = "SELECT COALESCE(SUM(CASE WHEN dc=1 THEN amount ELSE 0 END), 0), " +
                                          "COALESCE(SUM(CASE WHEN dc=0 THEN amount ELSE 0 END), 0) " +
                                          "FROM Voucher WHERE accountId = @aid AND day < @start";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", accountId.Value);
                        cmd.Parameters.AddWithValue("@start", startDt);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal priorDebit = reader.GetDecimal(0);
                                decimal priorCredit = reader.GetDecimal(1);
                                runningBalance = accountIsDebit.Value
                                    ? runningBalance + priorDebit - priorCredit
                                    : runningBalance + priorCredit - priorDebit;
                            }
                        }
                    }

                    // 重新执行主查询
                    cmd.Parameters.Clear();
                    if (!string.IsNullOrWhiteSpace(startDate))
                        cmd.Parameters.AddWithValue("@startDate", DateTime.Parse(startDate));
                    if (!string.IsNullOrWhiteSpace(endDate))
                        cmd.Parameters.AddWithValue("@endDate", DateTime.Parse(endDate));
                    if (accountId.HasValue)
                        cmd.Parameters.AddWithValue("@accountId", accountId.Value);

                    cmd.CommandText = sql.ToString();

                    decimal totalDebit = 0, totalCredit = 0;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bool isDebit = reader.GetBoolean(4);
                            decimal amount = reader.GetDecimal(5);

                            totalDebit += isDebit ? amount : 0;
                            totalCredit += isDebit ? 0 : amount;

                            if (computeBalance && accountIsDebit.HasValue)
                            {
                                runningBalance += (accountIsDebit.Value == isDebit) ? amount : -amount;
                            }

                            var entry = new JObject
                            {
                                ["date"] = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                ["voucher_number"] = reader.IsDBNull(1) ? null : reader.GetString(1),
                                ["summary"] = reader.IsDBNull(3) ? null : reader.GetString(3),
                                ["account_code"] = reader.IsDBNull(6) ? null : reader.GetString(6),
                                ["account_name"] = reader.IsDBNull(7) ? null : reader.GetString(7),
                                ["debit_amount"] = isDebit ? amount : 0m,
                                ["credit_amount"] = isDebit ? 0m : amount,
                                ["voucher_type"] = reader.IsDBNull(8) ? null : reader.GetString(8)
                            };

                            if (computeBalance)
                            {
                                entry["balance"] = runningBalance;
                            }

                            entries.Add(entry);
                        }
                    }

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["total"] = entries.Count,
                        ["total_debit"] = totalDebit,
                        ["total_credit"] = totalCredit,
                        ["account_code"] = accountCode ?? (JToken)JValue.CreateNull(),
                        ["entries"] = new JArray(entries)
                    };

                    if (computeBalance && accountId.HasValue)
                    {
                        result["begin_balance"] = runningBalance;
                    }

                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("获取序时账失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 查询指定科目在多个年度的余额和借贷发生额对比
        /// </summary>
        public static string GetBalanceComparison(long projectId, string accountCode, int startYear, int endYear)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountCode))
                    return ErrorJson("科目编码不能为空");

                string dbPath = ResolveLedgerPath();

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 查询科目信息
                    cmd.CommandText = "SELECT id, code, name, dc, balance FROM Account WHERE code = @code";
                    cmd.Parameters.AddWithValue("@code", accountCode);
                    int accountId = 0;
                    string accountName = null;
                    bool isDebit = true;
                    decimal initialBalance = 0;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            accountId = reader.GetInt32(0);
                            accountName = reader.GetString(2);
                            isDebit = reader.GetBoolean(3);
                            initialBalance = reader.GetDecimal(4);
                        }
                        else
                        {
                            return ErrorJson($"未找到科目编码: {accountCode}");
                        }
                    }

                    // 获取该科目及其所有下级科目的ID
                    var accountIds = new List<int> { accountId };
                    cmd.CommandText = "SELECT descendantId FROM AccountRel WHERE ancestorId = @aid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@aid", accountId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accountIds.Add(reader.GetInt32(0));
                        }
                    }

                    var idList = string.Join(",", accountIds);

                    // 获取账簿开始年份
                    int ledgerStartYear = startYear;
                    cmd.CommandText = "SELECT startDate FROM Ledger";
                    cmd.Parameters.Clear();
                    var ledgerResult = cmd.ExecuteScalar();
                    if (ledgerResult != null && ledgerResult != DBNull.Value)
                    {
                        DateTime ledgerStart = Convert.ToDateTime(ledgerResult);
                        ledgerStartYear = ledgerStart.Year;
                    }

                    // 计算各年度数据
                    var years = new List<JObject>();
                    decimal cumulativeBalance = initialBalance;

                    // 如果账簿开始年份早于查询起始年，计算累计发生额到查询起始年前一天
                    DateTime accStartDate = new DateTime(startYear, 1, 1);
                    cmd.CommandText = $"SELECT COALESCE(SUM(CASE WHEN dc=1 THEN amount ELSE 0 END), 0), " +
                                      $"COALESCE(SUM(CASE WHEN dc=0 THEN amount ELSE 0 END), 0) " +
                                      $"FROM Voucher WHERE accountId IN ({idList}) AND day < @startDate";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@startDate", accStartDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal priorDebit = reader.GetDecimal(0);
                            decimal priorCredit = reader.GetDecimal(1);
                            cumulativeBalance = isDebit
                                ? cumulativeBalance + priorDebit - priorCredit
                                : cumulativeBalance + priorCredit - priorDebit;
                        }
                    }

                    for (int year = startYear; year <= endYear; year++)
                    {
                        DateTime yearStart = new DateTime(year, 1, 1);
                        DateTime yearEnd = new DateTime(year, 12, 31);

                        // 期初余额
                        decimal openingBalance = cumulativeBalance;

                        // 本年借方发生额
                        cmd.CommandText = $"SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId IN ({idList}) AND dc = 1 AND day >= @start AND day <= @end";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@start", yearStart);
                        cmd.Parameters.AddWithValue("@end", yearEnd);
                        decimal debitAmount = Convert.ToDecimal(cmd.ExecuteScalar());

                        // 本年贷方发生额
                        cmd.CommandText = $"SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId IN ({idList}) AND dc = 0 AND day >= @start AND day <= @end";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@start", yearStart);
                        cmd.Parameters.AddWithValue("@end", yearEnd);
                        decimal creditAmount = Convert.ToDecimal(cmd.ExecuteScalar());

                        // 期末余额
                        decimal closingBalance = isDebit
                            ? openingBalance + debitAmount - creditAmount
                            : openingBalance + creditAmount - debitAmount;

                        // 按借贷方向列示
                        decimal openingDebit = isDebit ? Math.Max(openingBalance, 0) : 0;
                        decimal openingCredit = isDebit ? 0 : Math.Max(openingBalance, 0);
                        decimal closingDebit = isDebit ? Math.Max(closingBalance, 0) : 0;
                        decimal closingCredit = isDebit ? 0 : Math.Max(closingBalance, 0);

                        years.Add(new JObject
                        {
                            ["year"] = year,
                            ["opening_debit"] = openingDebit,
                            ["opening_credit"] = openingCredit,
                            ["debit_total"] = debitAmount,
                            ["credit_total"] = creditAmount,
                            ["closing_debit"] = closingDebit,
                            ["closing_credit"] = closingCredit
                        });

                        cumulativeBalance = closingBalance;
                    }

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["account_code"] = accountCode,
                        ["account_name"] = accountName,
                        ["direction"] = isDebit ? "借" : "贷",
                        ["start_year"] = startYear,
                        ["end_year"] = endYear,
                        ["years"] = new JArray(years)
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("获取多年度余额对比失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
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