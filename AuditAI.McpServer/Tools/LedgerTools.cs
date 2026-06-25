﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 账簿管理 MCP 工具注册
    /// </summary>
    public static class LedgerTools
    {
        /// <summary>
        /// 注册所有账簿管理工具
        /// </summary>
        public static void Register()
        {
            // get_ledger_accounts
            ToolRegistry.Register("get_ledger_accounts",
                "获取账簿的科目列表。返回科目编码、名称、层级、余额方向（借/贷）等信息。可选按科目类别筛选（传入科目编码前缀，如 1=资产类、2=负债类、4=所有者权益类、6=损益类）。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["account_class"] = new JObject { ["type"] = "string", ["description"] = "可选科目类别筛选（科目编码前缀：1资产/2负债/3共同/4权益/5成本/6损益）" }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string accountClass = args["account_class"]?.ToString();
                    return LedgerService.GetLedgerAccounts(projectId, accountClass);
                });

            // get_account_balance
            ToolRegistry.Register("get_account_balance",
                "获取指定科目在指定期间的余额信息。返回期初余额、本期借方发生额、本期贷方发生额、期末余额。科目编码可通过 get_ledger_accounts 查询。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["account_code"] = new JObject { ["type"] = "string", ["description"] = "科目编码，如 1001（库存现金）" },
                        ["start_year"] = new JObject { ["type"] = "integer", ["description"] = "开始年份，如 2025" },
                        ["start_month"] = new JObject { ["type"] = "integer", ["description"] = "开始月份（1-12）" },
                        ["end_year"] = new JObject { ["type"] = "integer", ["description"] = "结束年份，如 2025" },
                        ["end_month"] = new JObject { ["type"] = "integer", ["description"] = "结束月份（1-12）" }
                    },
                    ["required"] = new JArray { "project_id", "account_code", "start_year", "start_month", "end_year", "end_month" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string accountCode = args["account_code"]?.ToString();
                    int startYear = args["start_year"]?.Value<int>() ?? 0;
                    int startMonth = args["start_month"]?.Value<int>() ?? 0;
                    int endYear = args["end_year"]?.Value<int>() ?? 0;
                    int endMonth = args["end_month"]?.Value<int>() ?? 0;
                    return LedgerService.GetAccountBalance(projectId, accountCode, startYear, startMonth, endYear, endMonth);
                });

            // get_trial_balance
            ToolRegistry.Register("get_trial_balance",
                "获取试算平衡表。返回指定年月全部科目的期初余额（借/贷）、本期发生额（借/贷）、期末余额（借/贷），以及合计和平衡校验结果。这是审计中验证账簿数据准确性的核心报表。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["year"] = new JObject { ["type"] = "integer", ["description"] = "年份，如 2025" },
                        ["month"] = new JObject { ["type"] = "integer", ["description"] = "月份（1-12）" }
                    },
                    ["required"] = new JArray { "project_id", "year", "month" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    int year = args["year"]?.Value<int>() ?? 0;
                    int month = args["month"]?.Value<int>() ?? 0;
                    return LedgerService.GetTrialBalance(projectId, year, month);
                });

            // get_vouchers
            ToolRegistry.Register("get_vouchers",
                "获取凭证列表。返回凭证号、日期、摘要、科目、借贷方向、金额、制单人、审核人等信息。支持按日期范围和科目编码筛选。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["start_year"] = new JObject { ["type"] = "integer", ["description"] = "可选开始年份" },
                        ["start_month"] = new JObject { ["type"] = "integer", ["description"] = "可选开始月份（1-12）" },
                        ["end_year"] = new JObject { ["type"] = "integer", ["description"] = "可选结束年份" },
                        ["end_month"] = new JObject { ["type"] = "integer", ["description"] = "可选结束月份（1-12）" },
                        ["account_code"] = new JObject { ["type"] = "string", ["description"] = "可选科目编码筛选" }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    int? startYear = args["start_year"]?.Value<int>();
                    int? startMonth = args["start_month"]?.Value<int>();
                    int? endYear = args["end_year"]?.Value<int>();
                    int? endMonth = args["end_month"]?.Value<int>();
                    string accountCode = args["account_code"]?.ToString();
                    return LedgerService.GetVouchers(projectId, startYear, startMonth, endYear, endMonth, accountCode);
                });

            // get_subsidiary_ledger
            ToolRegistry.Register("get_subsidiary_ledger",
                "获取明细账。返回指定科目在指定期间的逐笔明细记录，包含凭证号、日期、摘要、借贷方向、金额、余额及累计余额。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["account_code"] = new JObject { ["type"] = "string", ["description"] = "科目编码，如 1001（库存现金）" },
                        ["start_year"] = new JObject { ["type"] = "integer", ["description"] = "开始年份，如 2025" },
                        ["start_month"] = new JObject { ["type"] = "integer", ["description"] = "开始月份（1-12）" },
                        ["end_year"] = new JObject { ["type"] = "integer", ["description"] = "结束年份，如 2025" },
                        ["end_month"] = new JObject { ["type"] = "integer", ["description"] = "结束月份（1-12）" }
                    },
                    ["required"] = new JArray { "project_id", "account_code", "start_year", "start_month", "end_year", "end_month" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string accountCode = args["account_code"]?.ToString();
                    int startYear = args["start_year"]?.Value<int>() ?? 0;
                    int startMonth = args["start_month"]?.Value<int>() ?? 0;
                    int endYear = args["end_year"]?.Value<int>() ?? 0;
                    int endMonth = args["end_month"]?.Value<int>() ?? 0;
                    return LedgerService.GetSubsidiaryLedger(projectId, accountCode, startYear, startMonth, endYear, endMonth);
                });

            // get_general_ledger
            ToolRegistry.Register("get_general_ledger",
                "获取总账。返回指定年月全部科目的期初余额、本期借方发生额、本期贷方发生额、期末余额汇总表。使用前需先调用 import_ledger 导入账簿。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["year"] = new JObject { ["type"] = "integer", ["description"] = "年份，如 2025" },
                        ["month"] = new JObject { ["type"] = "integer", ["description"] = "月份（1-12）" }
                    },
                    ["required"] = new JArray { "project_id", "year", "month" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    int year = args["year"]?.Value<int>() ?? 0;
                    int month = args["month"]?.Value<int>() ?? 0;
                    return LedgerService.GetGeneralLedger(projectId, year, month);
                });

            // import_ledger
            ToolRegistry.Register("import_ledger",
                "导入账簿文件（.db 格式的 SQLite 账簿数据库）。导入后即可调用其他账簿查询工具（如获取科目、试算平衡表、凭证等）。返回账簿基本信息（公司名称、会计期间、科目数、凭证数）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject { ["type"] = "integer", ["description"] = "项目ID" },
                        ["ledger_file_path"] = new JObject { ["type"] = "string", ["description"] = "账簿文件路径（.db 文件）" }
                    },
                    ["required"] = new JArray { "project_id", "ledger_file_path" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string ledgerFilePath = args["ledger_file_path"]?.ToString();
                    return LedgerService.ImportLedger(projectId, ledgerFilePath);
                });
        }
    }
}
