﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 账簿服务层
    /// 封装账簿数据查询操作，通过直接查询 SQLite 账簿数据库实现
    /// </summary>
    public static class LedgerService
    {
        // =============================================
        // 公开方法
        // =============================================

        /// <summary>
        /// 获取科目列表
        /// </summary>
        /// <param name="projectId">项目ID（用于上下文标识，实际使用当前会话账簿）</param>
        /// <param name="accountClass">可选科目类别筛选（按科目编码首字母：1资产/2负债/3共同/4权益/5成本/6损益）</param>
        public static string GetLedgerAccounts(long projectId, string accountClass = null)
        {
            try
            {
                string dbPath = ResolveLedgerPath();
                var accounts = new List<JObject>();

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 读取全部科目，按编码排序
                    cmd.CommandText = "SELECT id, parentId, code, name, dc, balance FROM Account ORDER BY code";
                    var rows = new List<AccountRow>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new AccountRow
                            {
                                Id = reader.GetInt32(0),
                                ParentId = reader.GetInt32(1),
                                Code = reader.GetString(2),
                                Name = reader.GetString(3),
                                IsDebit = reader.GetBoolean(4),
                                Balance = reader.GetDecimal(5)
                            });
                        }
                    }

                    // 构建层级关系
                    var idToRow = rows.ToDictionary(r => r.Id);
                    var codeToLevel = new Dictionary<string, int>();
                    foreach (var row in rows)
                    {
                        int level = ComputeLevel(row, idToRow);
                        codeToLevel[row.Code] = level;
                    }

                    // 按类别筛选
                    IEnumerable<AccountRow> filtered = rows;
                    if (!string.IsNullOrWhiteSpace(accountClass))
                    {
                        string prefix = accountClass.Trim();
                        filtered = rows.Where(r => r.Code.StartsWith(prefix, StringComparison.Ordinal));
                    }

                    foreach (var row in filtered)
                    {
                        accounts.Add(new JObject
                        {
                            ["id"] = row.Id,
                            ["code"] = row.Code,
                            ["name"] = row.Name,
                            ["direction"] = row.IsDebit ? "借" : "贷",
                            ["is_debit"] = row.IsDebit,
                            ["level"] = codeToLevel[row.Code],
                            ["initial_balance"] = row.Balance,
                            ["parent_id"] = row.ParentId == -1 ? null : (JToken)row.ParentId
                        });
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId,
                    ["total"] = accounts.Count,
                    ["accounts"] = new JArray(accounts)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取科目列表失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取科目余额
        /// </summary>
        public static string GetAccountBalance(long projectId, string accountCode, int startYear, int startMonth, int endYear, int endMonth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountCode))
                {
                    return ErrorJson("科目编码不能为空");
                }

                string dbPath = ResolveLedgerPath();
                DateTime startDate = new DateTime(startYear, startMonth, 1);
                DateTime endDate = new DateTime(endYear, endMonth, DateTime.DaysInMonth(endYear, endMonth));

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 查询指定科目（含其所有下级科目的汇总）
                    // 先查科目本身
                    cmd.CommandText = "SELECT id, code, name, dc, balance FROM Account WHERE code = @code";
                    cmd.Parameters.AddWithValue("@code", accountCode);
                    AccountRow account = null;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            account = new AccountRow
                            {
                                Id = reader.GetInt32(0),
                                Code = reader.GetString(1),
                                Name = reader.GetString(2),
                                IsDebit = reader.GetBoolean(3),
                                Balance = reader.GetDecimal(4)
                            };
                        }
                    }

                    if (account == null)
                    {
                        return ErrorJson($"未找到科目编码: {accountCode}");
                    }

                    // 获取该科目及其所有下级科目的ID（通过 AccountRel 表）
                    var accountIds = new List<int> { account.Id };
                    cmd.CommandText = "SELECT descendantId FROM AccountRel WHERE ancestorId = @aid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@aid", account.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accountIds.Add(reader.GetInt32(0));
                        }
                    }

                    // 查询期初余额（科目本身的 balance 字段为建账期初余额）
                    decimal beginBalance = account.Balance;

                    // 查询本期借方发生额
                    cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId IN (" +
                                      BuildInClause(accountIds) + ") AND dc = 1 AND day >= @start AND day <= @end";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@start", startDate);
                    cmd.Parameters.AddWithValue("@end", endDate);
                    decimal debit = Convert.ToDecimal(cmd.ExecuteScalar());

                    // 查询本期贷方发生额
                    cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId IN (" +
                                      BuildInClause(accountIds) + ") AND dc = 0 AND day >= @start AND day <= @end";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@start", startDate);
                    cmd.Parameters.AddWithValue("@end", endDate);
                    decimal credit = Convert.ToDecimal(cmd.ExecuteScalar());

                    // 计算期末余额
                    // 借方科目：期末 = 期初 + 借方 - 贷方
                    // 贷方科目：期末 = 期初 + 贷方 - 借方
                    decimal endBalance = account.IsDebit
                        ? beginBalance + debit - credit
                        : beginBalance + credit - debit;

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["account_code"] = account.Code,
                        ["account_name"] = account.Name,
                        ["direction"] = account.IsDebit ? "借" : "贷",
                        ["period"] = new JObject
                        {
                            ["start"] = $"{startYear}-{startMonth:D2}",
                            ["end"] = $"{endYear}-{endMonth:D2}"
                        },
                        ["begin_balance"] = beginBalance,
                        ["debit_amount"] = debit,
                        ["credit_amount"] = credit,
                        ["end_balance"] = endBalance
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("获取科目余额失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取试算平衡表
        /// </summary>
        public static string GetTrialBalance(long projectId, int year, int month)
        {
            try
            {
                string dbPath = ResolveLedgerPath();
                DateTime startDate = new DateTime(year, 1, 1);
                DateTime endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                DateTime periodStart = new DateTime(year, month, 1);

                var rows = new List<JObject>();
                decimal totalBeginDebit = 0, totalBeginCredit = 0;
                decimal totalDebit = 0, totalCredit = 0;
                decimal totalEndDebit = 0, totalEndCredit = 0;

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 读取全部科目
                    cmd.CommandText = "SELECT id, parentId, code, name, dc, balance FROM Account ORDER BY code";
                    var accounts = new List<AccountRow>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accounts.Add(new AccountRow
                            {
                                Id = reader.GetInt32(0),
                                ParentId = reader.GetInt32(1),
                                Code = reader.GetString(2),
                                Name = reader.GetString(3),
                                IsDebit = reader.GetBoolean(4),
                                Balance = reader.GetDecimal(5)
                            });
                        }
                    }

                    // 仅计算叶子科目（没有子科目的科目），避免重复汇总
                    var parentIds = new HashSet<int>(accounts.Where(a => a.ParentId != -1).Select(a => a.ParentId));
                    var leafAccounts = accounts.Where(a => !parentIds.Contains(a.Id)).ToList();

                    foreach (var acc in leafAccounts)
                    {
                        // 期初余额（balance 字段为建账期初，需要计算到指定月份之前的累计发生额）
                        // 简化处理：期初 = 建账余额 + 建账日至期初的累计发生额
                        // 这里采用简化版本：期初余额 = Account.balance + 年初至本期前的发生额
                        decimal beginBalance = acc.Balance;

                        // 加上建账开始日期到本期开始前的发生额
                        // 假设建账日期为年初，计算 1月1日 到 periodStart-1 的发生额
                        cmd.CommandText = "SELECT COALESCE(SUM(CASE WHEN dc=1 THEN amount ELSE 0 END), 0), " +
                                          "COALESCE(SUM(CASE WHEN dc=0 THEN amount ELSE 0 END), 0) " +
                                          "FROM Voucher WHERE accountId = @aid AND day < @periodStart";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", acc.Id);
                        cmd.Parameters.AddWithValue("@periodStart", periodStart);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal priorDebit = reader.GetDecimal(0);
                                decimal priorCredit = reader.GetDecimal(1);
                                beginBalance = acc.IsDebit
                                    ? beginBalance + priorDebit - priorCredit
                                    : beginBalance + priorCredit - priorDebit;
                            }
                        }

                        // 本期借方发生额
                        cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId = @aid AND dc = 1 AND day >= @start AND day <= @end";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", acc.Id);
                        cmd.Parameters.AddWithValue("@start", periodStart);
                        cmd.Parameters.AddWithValue("@end", endDate);
                        decimal debit = Convert.ToDecimal(cmd.ExecuteScalar());

                        // 本期贷方发生额
                        cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId = @aid AND dc = 0 AND day >= @start AND day <= @end";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", acc.Id);
                        cmd.Parameters.AddWithValue("@start", periodStart);
                        cmd.Parameters.AddWithValue("@end", endDate);
                        decimal credit = Convert.ToDecimal(cmd.ExecuteScalar());

                        // 期末余额
                        decimal endBalance = acc.IsDebit
                            ? beginBalance + debit - credit
                            : beginBalance + credit - debit;

                        // 试算平衡表展示：余额按借贷方向分别列示
                        decimal beginDebit = acc.IsDebit ? Math.Max(beginBalance, 0) : 0;
                        decimal beginCredit = acc.IsDebit ? 0 : Math.Max(beginBalance, 0);
                        decimal endDebit = acc.IsDebit ? Math.Max(endBalance, 0) : 0;
                        decimal endCredit = acc.IsDebit ? 0 : Math.Max(endBalance, 0);

                        totalBeginDebit += beginDebit;
                        totalBeginCredit += beginCredit;
                        totalDebit += debit;
                        totalCredit += credit;
                        totalEndDebit += endDebit;
                        totalEndCredit += endCredit;

                        rows.Add(new JObject
                        {
                            ["code"] = acc.Code,
                            ["name"] = acc.Name,
                            ["direction"] = acc.IsDebit ? "借" : "贷",
                            ["begin_debit"] = beginDebit,
                            ["begin_credit"] = beginCredit,
                            ["debit_amount"] = debit,
                            ["credit_amount"] = credit,
                            ["end_debit"] = endDebit,
                            ["end_credit"] = endCredit
                        });
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId,
                    ["period"] = $"{year}-{month:D2}",
                    ["total"] = rows.Count,
                    ["accounts"] = new JArray(rows),
                    ["summary"] = new JObject
                    {
                        ["begin_debit"] = totalBeginDebit,
                        ["begin_credit"] = totalBeginCredit,
                        ["debit_amount"] = totalDebit,
                        ["credit_amount"] = totalCredit,
                        ["end_debit"] = totalEndDebit,
                        ["end_credit"] = totalEndCredit,
                        ["begin_balanced"] = totalBeginDebit == totalBeginCredit,
                        ["period_balanced"] = totalDebit == totalCredit,
                        ["end_balanced"] = totalEndDebit == totalEndCredit
                    }
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取试算平衡表失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取凭证列表
        /// </summary>
        public static string GetVouchers(long projectId, int? startYear, int? startMonth, int? endYear, int? endMonth, string accountCode = null)
        {
            try
            {
                string dbPath = ResolveLedgerPath();

                // 构造日期范围（默认使用账簿全量）
                DateTime? startDate = null;
                DateTime? endDate = null;
                if (startYear.HasValue && startMonth.HasValue)
                {
                    startDate = new DateTime(startYear.Value, startMonth.Value, 1);
                }
                if (endYear.HasValue && endMonth.HasValue)
                {
                    endDate = new DateTime(endYear.Value, endMonth.Value, DateTime.DaysInMonth(endYear.Value, endMonth.Value));
                }

                var vouchers = new List<JObject>();

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 构造查询
                    var sql = new System.Text.StringBuilder();
                    sql.Append("SELECT v.id, v.number, v.day, v.digest, v.dc, v.amount, ");
                    sql.Append("v.maker, v.checker, v.booker, v.OppositeAccounts, ");
                    sql.Append("a.code AS accountCode, a.name AS accountName, ");
                    sql.Append("vt.name AS typeName ");
                    sql.Append("FROM Voucher v ");
                    sql.Append("LEFT JOIN Account a ON v.accountId = a.id ");
                    sql.Append("LEFT JOIN VoucherType vt ON v.type = vt.id ");
                    sql.Append("WHERE 1=1 ");

                    if (startDate.HasValue)
                    {
                        sql.Append("AND v.day >= @start ");
                        cmd.Parameters.AddWithValue("@start", startDate.Value);
                    }
                    if (endDate.HasValue)
                    {
                        sql.Append("AND v.day <= @end ");
                        cmd.Parameters.AddWithValue("@end", endDate.Value);
                    }
                    if (!string.IsNullOrWhiteSpace(accountCode))
                    {
                        sql.Append("AND a.code = @code ");
                        cmd.Parameters.AddWithValue("@code", accountCode);
                    }
                    sql.Append("ORDER BY v.day, v.id");
                    cmd.CommandText = sql.ToString();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vouchers.Add(new JObject
                            {
                                ["id"] = reader.GetInt32(0),
                                ["number"] = reader.IsDBNull(1) ? null : reader.GetString(1),
                                ["date"] = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                ["digest"] = reader.IsDBNull(3) ? null : reader.GetString(3),
                                ["direction"] = reader.GetBoolean(4) ? "借" : "贷",
                                ["amount"] = reader.GetDecimal(5),
                                ["maker"] = reader.IsDBNull(6) ? null : reader.GetString(6),
                                ["checker"] = reader.IsDBNull(7) ? null : reader.GetString(7),
                                ["booker"] = reader.IsDBNull(8) ? null : reader.GetString(8),
                                ["opposite_accounts"] = reader.IsDBNull(9) ? null : reader.GetString(9),
                                ["account_code"] = reader.IsDBNull(10) ? null : reader.GetString(10),
                                ["account_name"] = reader.IsDBNull(11) ? null : reader.GetString(11),
                                ["voucher_type"] = reader.IsDBNull(12) ? null : reader.GetString(12)
                            });
                        }
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId,
                    ["total"] = vouchers.Count,
                    ["vouchers"] = new JArray(vouchers)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取凭证列表失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取明细账
        /// </summary>
        public static string GetSubsidiaryLedger(long projectId, string accountCode, int startYear, int startMonth, int endYear, int endMonth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountCode))
                {
                    return ErrorJson("科目编码不能为空");
                }

                string dbPath = ResolveLedgerPath();
                DateTime startDate = new DateTime(startYear, startMonth, 1);
                DateTime endDate = new DateTime(endYear, endMonth, DateTime.DaysInMonth(endYear, endMonth));

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 查询科目信息
                    cmd.CommandText = "SELECT id, code, name, dc, balance FROM Account WHERE code = @code";
                    cmd.Parameters.AddWithValue("@code", accountCode);
                    AccountRow account = null;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            account = new AccountRow
                            {
                                Id = reader.GetInt32(0),
                                Code = reader.GetString(1),
                                Name = reader.GetString(2),
                                IsDebit = reader.GetBoolean(3),
                                Balance = reader.GetDecimal(4)
                            };
                        }
                    }

                    if (account == null)
                    {
                        return ErrorJson($"未找到科目编码: {accountCode}");
                    }

                    // 计算期初余额（建账余额 + 建账至开始日期前的发生额）
                    decimal beginBalance = account.Balance;
                    cmd.CommandText = "SELECT COALESCE(SUM(CASE WHEN dc=1 THEN amount ELSE 0 END), 0), " +
                                      "COALESCE(SUM(CASE WHEN dc=0 THEN amount ELSE 0 END), 0) " +
                                      "FROM Voucher WHERE accountId = @aid AND day < @start";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@aid", account.Id);
                    cmd.Parameters.AddWithValue("@start", startDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal priorDebit = reader.GetDecimal(0);
                            decimal priorCredit = reader.GetDecimal(1);
                            beginBalance = account.IsDebit
                                ? beginBalance + priorDebit - priorCredit
                                : beginBalance + priorCredit - priorDebit;
                        }
                    }

                    // 查询本期明细
                    var entries = new List<JObject>();
                    decimal runningBalance = beginBalance;
                    decimal totalDebit = 0, totalCredit = 0;

                    cmd.CommandText = "SELECT id, number, day, digest, dc, amount, maker, checker " +
                                      "FROM Voucher WHERE accountId = @aid AND day >= @start AND day <= @end " +
                                      "ORDER BY day, id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@aid", account.Id);
                    cmd.Parameters.AddWithValue("@start", startDate);
                    cmd.Parameters.AddWithValue("@end", endDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bool isDebit = reader.GetBoolean(4);
                            decimal amount = reader.GetDecimal(5);
                            totalDebit += isDebit ? amount : 0;
                            totalCredit += isDebit ? 0 : amount;
                            runningBalance += (account.IsDebit == isDebit) ? amount : -amount;

                            entries.Add(new JObject
                            {
                                ["id"] = reader.GetInt32(0),
                                ["number"] = reader.IsDBNull(1) ? null : reader.GetString(1),
                                ["date"] = reader.IsDBNull(2) ? null : reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                ["digest"] = reader.IsDBNull(3) ? null : reader.GetString(3),
                                ["direction"] = isDebit ? "借" : "贷",
                                ["amount"] = amount,
                                ["balance"] = runningBalance,
                                ["maker"] = reader.IsDBNull(6) ? null : reader.GetString(6),
                                ["checker"] = reader.IsDBNull(7) ? null : reader.GetString(7)
                            });
                        }
                    }

                    var result = new JObject
                    {
                        ["success"] = true,
                        ["project_id"] = projectId,
                        ["account_code"] = account.Code,
                        ["account_name"] = account.Name,
                        ["direction"] = account.IsDebit ? "借" : "贷",
                        ["period"] = new JObject
                        {
                            ["start"] = $"{startYear}-{startMonth:D2}",
                            ["end"] = $"{endYear}-{endMonth:D2}"
                        },
                        ["begin_balance"] = beginBalance,
                        ["total_debit"] = totalDebit,
                        ["total_credit"] = totalCredit,
                        ["end_balance"] = runningBalance,
                        ["entry_count"] = entries.Count,
                        ["entries"] = new JArray(entries)
                    };
                    return JsonConvert.SerializeObject(result, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return ErrorJson("获取明细账失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取总账
        /// </summary>
        public static string GetGeneralLedger(long projectId, int year, int month)
        {
            try
            {
                string dbPath = ResolveLedgerPath();
                DateTime periodStart = new DateTime(year, month, 1);
                DateTime periodEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                var rows = new List<JObject>();

                using (var cnn = OpenConnection(dbPath))
                using (var cmd = cnn.CreateCommand())
                {
                    // 读取全部科目
                    cmd.CommandText = "SELECT id, parentId, code, name, dc, balance FROM Account ORDER BY code";
                    var accounts = new List<AccountRow>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accounts.Add(new AccountRow
                            {
                                Id = reader.GetInt32(0),
                                ParentId = reader.GetInt32(1),
                                Code = reader.GetString(2),
                                Name = reader.GetString(3),
                                IsDebit = reader.GetBoolean(4),
                                Balance = reader.GetDecimal(5)
                            });
                        }
                    }

                    // 仅计算叶子科目
                    var parentIds = new HashSet<int>(accounts.Where(a => a.ParentId != -1).Select(a => a.ParentId));
                    var leafAccounts = accounts.Where(a => !parentIds.Contains(a.Id)).ToList();

                    foreach (var acc in leafAccounts)
                    {
                        // 期初余额
                        decimal beginBalance = acc.Balance;
                        cmd.CommandText = "SELECT COALESCE(SUM(CASE WHEN dc=1 THEN amount ELSE 0 END), 0), " +
                                          "COALESCE(SUM(CASE WHEN dc=0 THEN amount ELSE 0 END), 0) " +
                                          "FROM Voucher WHERE accountId = @aid AND day < @periodStart";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", acc.Id);
                        cmd.Parameters.AddWithValue("@periodStart", periodStart);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal priorDebit = reader.GetDecimal(0);
                                decimal priorCredit = reader.GetDecimal(1);
                                beginBalance = acc.IsDebit
                                    ? beginBalance + priorDebit - priorCredit
                                    : beginBalance + priorCredit - priorDebit;
                            }
                        }

                        // 本期借方
                        cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId = @aid AND dc = 1 AND day >= @start AND day <= @end";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", acc.Id);
                        cmd.Parameters.AddWithValue("@start", periodStart);
                        cmd.Parameters.AddWithValue("@end", periodEnd);
                        decimal debit = Convert.ToDecimal(cmd.ExecuteScalar());

                        // 本期贷方
                        cmd.CommandText = "SELECT COALESCE(SUM(amount), 0) FROM Voucher WHERE accountId = @aid AND dc = 0 AND day >= @start AND day <= @end";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", acc.Id);
                        cmd.Parameters.AddWithValue("@start", periodStart);
                        cmd.Parameters.AddWithValue("@end", periodEnd);
                        decimal credit = Convert.ToDecimal(cmd.ExecuteScalar());

                        // 期末余额
                        decimal endBalance = acc.IsDebit
                            ? beginBalance + debit - credit
                            : beginBalance + credit - debit;

                        rows.Add(new JObject
                        {
                            ["code"] = acc.Code,
                            ["name"] = acc.Name,
                            ["direction"] = acc.IsDebit ? "借" : "贷",
                            ["begin_balance"] = beginBalance,
                            ["debit_amount"] = debit,
                            ["credit_amount"] = credit,
                            ["end_balance"] = endBalance
                        });
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId,
                    ["period"] = $"{year}-{month:D2}",
                    ["total"] = rows.Count,
                    ["accounts"] = new JArray(rows)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取总账失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 导入账簿
        /// </summary>
        public static string ImportLedger(long projectId, string ledgerFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ledgerFilePath))
                {
                    return ErrorJson("账簿文件路径不能为空");
                }

                if (!File.Exists(ledgerFilePath))
                {
                    return ErrorJson($"账簿文件不存在: {ledgerFilePath}");
                }

                // 验证是否为有效的账簿数据库
                string companyName = null;
                DateTime startDate = default, endDate = default;
                int accountCount = 0, voucherCount = 0;

                try
                {
                    using (var cnn = OpenConnection(ledgerFilePath))
                    using (var cmd = cnn.CreateCommand())
                    {
                        // 读取账簿元信息
                        cmd.CommandText = "SELECT companyName, startDate, endDate FROM Ledger";
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                companyName = reader.IsDBNull(0) ? null : reader.GetString(0);
                                startDate = reader.IsDBNull(1) ? default : reader.GetDateTime(1);
                                endDate = reader.IsDBNull(2) ? default : reader.GetDateTime(2);
                            }
                        }

                        // 统计科目数
                        cmd.CommandText = "SELECT COUNT(*) FROM Account";
                        accountCount = Convert.ToInt32(cmd.ExecuteScalar());

                        // 统计凭证数
                        cmd.CommandText = "SELECT COUNT(*) FROM Voucher";
                        voucherCount = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    return ErrorJson($"账簿文件格式无效或无法读取: {ex.Message}");
                }

                // 保存到会话状态
                SessionState.Current.SetLedgerFilePath(ledgerFilePath);

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = projectId,
                    ["ledger_file_path"] = ledgerFilePath,
                    ["company_name"] = companyName,
                    ["start_date"] = startDate == default ? null : startDate.ToString("yyyy-MM-dd"),
                    ["end_date"] = endDate == default ? null : endDate.ToString("yyyy-MM-dd"),
                    ["account_count"] = accountCount,
                    ["voucher_count"] = voucherCount,
                    ["message"] = "账簿导入成功"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("导入账簿失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        /// <summary>
        /// 解析账簿文件路径：优先使用会话中已导入的账簿，其次尝试从当前项目推断
        /// </summary>
        private static string ResolveLedgerPath()
        {
            if (SessionState.Current.HasLedger)
            {
                return SessionState.Current.CurrentLedgerFilePath;
            }
            throw new InvalidOperationException("未导入账簿，请先调用 import_ledger 工具导入账簿文件");
        }

        /// <summary>
        /// 打开 SQLite 连接（只读模式）
        /// </summary>
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

        /// <summary>
        /// 计算科目层级深度
        /// </summary>
        private static int ComputeLevel(AccountRow row, Dictionary<int, AccountRow> idToRow)
        {
            int level = 0;
            int parentId = row.ParentId;
            while (parentId != -1 && idToRow.ContainsKey(parentId))
            {
                level++;
                parentId = idToRow[parentId].ParentId;
            }
            return level;
        }

        /// <summary>
        /// 构建 IN 子句的参数列表
        /// </summary>
        private static string BuildInClause(List<int> ids)
        {
            return string.Join(",", ids);
        }

        /// <summary>
        /// 生成错误 JSON 响应
        /// </summary>
        private static string ErrorJson(string message)
        {
            var result = new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        /// <summary>
        /// 科目行数据（内部使用）
        /// </summary>
        private class AccountRow
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public bool IsDebit { get; set; }
            public decimal Balance { get; set; }
        }
    }
}
