﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Auditai.DTO;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 凭证+序时账 MCP 工具注册
    /// </summary>
    public static class VoucherDetailTools
    {
        /// <summary>
        /// 注册所有凭证和序时账查询工具
        /// </summary>
        public static void Register()
        {
            // get_voucher_by_number
            ToolRegistry.Register("get_voucher_by_number",
                "按凭证号+凭证类型+期间精准查询单张凭证。返回凭证头信息（凭证号、日期、类型、合计金额、制单人、审核人、记账人）和所有分录行（科目编码、科目名称、借贷方向、金额、摘要、对方科目）。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["number"] = new JObject { ["type"] = "string", ["description"] = "凭证号，如 记-1" },
                        ["voucher_type"] = new JObject { ["type"] = "string", ["description"] = "可选凭证类型名称，如 记账凭证、收款凭证、付款凭证、转账凭证" },
                        ["period_year"] = new JObject { ["type"] = "integer", ["description"] = "可选期间年份，如 2025" },
                        ["period_month"] = new JObject { ["type"] = "integer", ["description"] = "可选期间月份（1-12）" }
                    },
                    ["required"] = new JArray { "project_id", "number" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string number = args["number"]?.ToString();
                    string voucherType = args["voucher_type"]?.ToString();
                    int? periodYear = args["period_year"]?.Value<int>();
                    int? periodMonth = args["period_month"]?.Value<int>();
                    return VoucherService.GetVoucherByNumber(projectId, number, voucherType, periodYear, periodMonth);
                });

            // get_voucher_detail
            ToolRegistry.Register("get_voucher_detail",
                "获取指定凭证的完整分录明细。返回凭证头信息（凭证号、日期、类型、合计金额、制单人、审核人、记账人）和逐行分录明细（分录ID、科目编码、科目名称、借方金额、贷方金额、摘要、对方科目）。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["voucher_id"] = new JObject { ["type"] = "integer", ["description"] = "凭证分录ID（Voucher表主键）" }
                    },
                    ["required"] = new JArray { "project_id", "voucher_id" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    long voucherId = args["voucher_id"]?.Value<long>() ?? 0;
                    return VoucherService.GetVoucherDetail(projectId, voucherId);
                });

            // get_vouchers_by_date
            ToolRegistry.Register("get_vouchers_by_date",
                "按日期范围查询凭证列表。返回符合条件的凭证列表（凭证号、日期、类型、借方合计、贷方合计、制单人），支持分页和按科目编码筛选。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["start_date"] = new JObject { ["type"] = "string", ["description"] = "开始日期，格式 yyyy-MM-dd，如 2025-01-01" },
                        ["end_date"] = new JObject { ["type"] = "string", ["description"] = "结束日期，格式 yyyy-MM-dd，如 2025-12-31" },
                        ["account_code"] = new JObject { ["type"] = "string", ["description"] = "可选科目编码筛选，如 1001" },
                        ["page"] = new JObject { ["type"] = "integer", ["description"] = "页码，从1开始，默认1" },
                        ["page_size"] = new JObject { ["type"] = "integer", ["description"] = "每页条数，默认50" }
                    },
                    ["required"] = new JArray { "project_id", "start_date", "end_date" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string startDate = args["start_date"]?.ToString();
                    string endDate = args["end_date"]?.ToString();
                    string accountCode = args["account_code"]?.ToString();
                    int page = args["page"]?.Value<int>() ?? 1;
                    int pageSize = args["page_size"]?.Value<int>() ?? 50;
                    return VoucherService.GetVouchersByDate(projectId, startDate, endDate, accountCode, page, pageSize);
                });

            // get_chronological_ledger
            ToolRegistry.Register("get_chronological_ledger",
                "获取序时账（按日期+凭证号排序的全量交易记录）。返回逐笔明细：日期、凭证号、摘要、科目编码、科目名称、借方金额、贷方金额。支持科目筛选和累计余额计算。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["start_date"] = new JObject { ["type"] = "string", ["description"] = "开始日期，格式 yyyy-MM-dd" },
                        ["end_date"] = new JObject { ["type"] = "string", ["description"] = "结束日期，格式 yyyy-MM-dd" },
                        ["account_code"] = new JObject { ["type"] = "string", ["description"] = "可选科目编码筛选，如 1001。指定后返回该科目的序时账并计算累计余额" }
                    },
                    ["required"] = new JArray { "project_id", "start_date", "end_date" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string startDate = args["start_date"]?.ToString();
                    string endDate = args["end_date"]?.ToString();
                    string accountCode = args["account_code"]?.ToString();
                    return LedgerComparisonService.GetChronologicalLedger(projectId, startDate, endDate, accountCode);
                });

            // get_balance_comparison
            ToolRegistry.Register("get_balance_comparison",
                "获取指定科目在多个会计年度的余额对比数据。返回各年度的期初余额（借/贷）、本期借方发生额、本期贷方发生额、期末余额（借/贷），便于进行多年度趋势分析。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["account_code"] = new JObject { ["type"] = "string", ["description"] = "科目编码，如 1001（库存现金）" },
                        ["start_year"] = new JObject { ["type"] = "integer", ["description"] = "起始年份，如 2024" },
                        ["end_year"] = new JObject { ["type"] = "integer", ["description"] = "结束年份，如 2026" }
                    },
                    ["required"] = new JArray { "project_id", "account_code", "start_year", "end_year" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string accountCode = args["account_code"]?.ToString();
                    int startYear = args["start_year"]?.Value<int>() ?? 0;
                    int endYear = args["end_year"]?.Value<int>() ?? 0;
                    return LedgerComparisonService.GetBalanceComparison(projectId, accountCode, startYear, endYear);
                });

            // query_vouchers
            ToolRegistry.Register("query_vouchers",
                "多条件组合查询凭证。支持按以下条件任意组合筛选：科目编码、日期范围、金额范围（最小值/最大值）、摘要关键词（LIKE模糊匹配）、凭证类型。所有条件均为可选，不传条件则查询全部凭证。返回分页列表，每张凭证包含凭证号、日期、类型、借方合计、贷方合计、分录条数、制单人。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["account_code"] = new JObject { ["type"] = "string", ["description"] = "可选，按科目编码筛选，如 1001" },
                        ["start_date"] = new JObject { ["type"] = "string", ["description"] = "可选，开始日期 yyyy-MM-dd" },
                        ["end_date"] = new JObject { ["type"] = "string", ["description"] = "可选，结束日期 yyyy-MM-dd" },
                        ["min_amount"] = new JObject { ["type"] = "number", ["description"] = "可选，最小金额筛选" },
                        ["max_amount"] = new JObject { ["type"] = "number", ["description"] = "可选，最大金额筛选" },
                        ["digest_keyword"] = new JObject { ["type"] = "string", ["description"] = "可选，摘要包含关键词（模糊匹配），如 借款、报销" },
                        ["voucher_type"] = new JObject { ["type"] = "string", ["description"] = "可选，凭证类型名称，如 记账凭证、收款凭证、付款凭证" },
                        ["page"] = new JObject { ["type"] = "integer", ["description"] = "页码，从1开始，默认1" },
                        ["page_size"] = new JObject { ["type"] = "integer", ["description"] = "每页条数，默认50" }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string accountCode = args["account_code"]?.ToString();
                    string startDate = args["start_date"]?.ToString();
                    string endDate = args["end_date"]?.ToString();
                    decimal? minAmount = args["min_amount"]?.Value<decimal>();
                    decimal? maxAmount = args["max_amount"]?.Value<decimal>();
                    string digestKeyword = args["digest_keyword"]?.ToString();
                    string voucherType = args["voucher_type"]?.ToString();
                    int page = args["page"]?.Value<int>() ?? 1;
                    int pageSize = args["page_size"]?.Value<int>() ?? 50;
                    return VoucherService.QueryVouchers(projectId, accountCode, startDate, endDate, minAmount, maxAmount, digestKeyword, voucherType, page, pageSize);
                });
        }
    }
}