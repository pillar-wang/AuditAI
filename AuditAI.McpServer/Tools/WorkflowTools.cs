﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 审计工作流 MCP 工具注册
    /// 提供组合式审计工作流能力，将多步骤审计流程封装为单次调用。
    /// 包括：一键创建审计项目、一键生成审计报告、端到端全自动审计。
    /// </summary>
    public static class WorkflowTools
    {
        /// <summary>
        /// 注册所有审计工作流工具
        /// </summary>
        public static void Register()
        {
            // create_audit_project
            ToolRegistry.Register("create_audit_project",
                "一键创建审计项目，自动完成项目创建、数据采集、账簿导入、底稿生成。适合开始新审计项目时调用。" +
                "工作流步骤：创建项目 → 打开项目 → 采集数据 → 导入账簿 → 生成底稿模板。" +
                "每步独立执行，某步失败不影响后续步骤，返回每步执行状态与耗时。" +
                "注意：采集任务为异步执行，账簿导入步骤需采集完成后手动调用 import_ledger 工具完成。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["company_name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "被审计单位名称，如：ABC有限公司"
                        },
                        ["year"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "审计年度，如：2025"
                        },
                        ["db_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "数据库类型标识，可选值：sqlserver、oracle、sqlite、access、paradox。请先调用 list_supported_databases 查询完整列表。"
                        },
                        ["connection_string"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "源数据库连接字符串。格式因数据库类型而异，可通过 list_supported_databases 工具查询各类型的连接字符串格式。"
                        }
                    },
                    ["required"] = new JArray { "company_name", "year", "db_type", "connection_string" }
                },
                (args) =>
                {
                    string companyName = args["company_name"]?.ToString();
                    int year = args["year"]?.Value<int>() ?? DateTime.Now.Year;
                    string dbType = args["db_type"]?.ToString();
                    string connectionString = args["connection_string"]?.ToString();
                    return WorkflowService.CreateAuditProject(companyName, year, dbType, connectionString);
                });

            // generate_audit_report
            ToolRegistry.Register("generate_audit_report",
                "自动汇总校验结果、计算关键指标、填充报告模板并导出。适合审计完成后生成报告。" +
                "工作流步骤：校验所有表格 → 计算试算平衡 → 填充报告模板 → 导出 Word。" +
                "每步独立执行，某步失败不影响后续步骤，返回每步执行状态与耗时。" +
                "前提：需已打开项目且项目中存在指定名称的文档节点（可通过 create_audit_project 工具自动生成\"审计报告\"文档节点）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "项目ID，用于试算平衡计算的上下文标识"
                        },
                        ["template_name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "报告模板名称（文档节点名称），默认为\"审计报告\"。工具会在项目树中查找该名称的文档节点并填充内容。"
                        },
                        ["output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "Word 输出文件完整路径（.docx 后缀），如 C:\\\\export\\\\审计报告.docx"
                        }
                    },
                    ["required"] = new JArray { "project_id", "template_name", "output_path" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string templateName = args["template_name"]?.ToString();
                    string outputPath = args["output_path"]?.ToString();
                    return WorkflowService.GenerateAuditReport(projectId, templateName, outputPath);
                });

            // run_full_audit
            ToolRegistry.Register("run_full_audit",
                "从零开始执行完整审计流程：创建项目→采集数据→计算表格→校验数据→生成报告。适合全自动审计。" +
                "工作流步骤：创建审计项目（含采集、底稿生成）→ 计算所有表格 → 校验所有表格 → 生成审计报告。" +
                "每步独立执行，某步失败不影响后续步骤，返回每步执行状态与耗时。" +
                "注意：由于采集任务为异步执行，账簿数据可能未就绪，计算和校验步骤的结果可能不完整。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["company_name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "被审计单位名称，如：ABC有限公司"
                        },
                        ["year"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "审计年度，如：2025"
                        },
                        ["db_type"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "数据库类型标识，可选值：sqlserver、oracle、sqlite、access、paradox"
                        },
                        ["connection_string"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "源数据库连接字符串"
                        },
                        ["report_output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "审计报告输出文件完整路径（.docx 后缀），如 C:\\\\export\\\\审计报告.docx"
                        }
                    },
                    ["required"] = new JArray { "company_name", "year", "db_type", "connection_string", "report_output_path" }
                },
                (args) =>
                {
                    string companyName = args["company_name"]?.ToString();
                    int year = args["year"]?.Value<int>() ?? DateTime.Now.Year;
                    string dbType = args["db_type"]?.ToString();
                    string connectionString = args["connection_string"]?.ToString();
                    string reportOutputPath = args["report_output_path"]?.ToString();
                    return WorkflowService.RunFullAudit(companyName, year, dbType, connectionString, reportOutputPath);
                });
        }
    }
}
