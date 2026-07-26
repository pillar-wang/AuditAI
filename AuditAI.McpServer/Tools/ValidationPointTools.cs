using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 验证点（ValidationFormula）管理 MCP 工具注册
    /// 覆盖 AppCommands.AddValidationPoint / RemoveValidationPoint / DocValidationMgmt 等 UI 操作
    /// </summary>
    public static class ValidationPointTools
    {
        public static void Register()
        {
            // list_validation_points
            ToolRegistry.Register("list_validation_points",
                "列举项目中的所有验证点（校验公式）。可选按 table_node_id 过滤。当需要查看验证规则时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "按表格节点 ID 过滤（可选）" }
                    }
                },
                (args) =>
                {
                    long? tid = args["table_node_id"]?.Value<long>();
                    return ValidationPointService.ListValidationPoints(tid);
                });

            // add_validation_point
            ToolRegistry.Register("add_validation_point",
                "添加验证点（校验公式）。例如 left='A1+A2', op='==', right='B1' 表示 A1+A2 应等于 B1。当需要添加数据校验规则时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "目标表格节点 ID" },
                        ["left_expr"] = new JObject { ["type"] = "string", ["description"] = "左表达式（公式）" },
                        ["operator"] = new JObject { ["type"] = "string", ["description"] = "比较操作符（Equal/NotEqual/GreaterThan/GreaterThanOrEqual/LessThan/LessThanOrEqual 或 ==/!=/>/>=/</<=）" },
                        ["right_expr"] = new JObject { ["type"] = "string", ["description"] = "右表达式（公式）" },
                        ["note"] = new JObject { ["type"] = "string", ["description"] = "备注说明（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id", "left_expr", "operator", "right_expr" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    string left = args["left_expr"]?.ToString();
                    string op = args["operator"]?.ToString();
                    string right = args["right_expr"]?.ToString();
                    string note = args["note"]?.ToString();
                    return ValidationPointService.AddValidationPoint(tid, left, op, right, note);
                });

            // update_validation_point
            ToolRegistry.Register("update_validation_point",
                "更新验证点（校验公式）的字段。传入需要更新的字段（null 字段将被忽略）。当需要修改验证规则时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["validation_id"] = new JObject { ["type"] = "integer", ["description"] = "验证点 ID" },
                        ["left_expr"] = new JObject { ["type"] = "string", ["description"] = "新的左表达式（可选）" },
                        ["operator"] = new JObject { ["type"] = "string", ["description"] = "新的操作符（可选）" },
                        ["right_expr"] = new JObject { ["type"] = "string", ["description"] = "新的右表达式（可选）" },
                        ["note"] = new JObject { ["type"] = "string", ["description"] = "新的备注说明（可选）" }
                    },
                    ["required"] = new JArray { "validation_id" }
                },
                (args) =>
                {
                    long vid = args["validation_id"]?.Value<long>() ?? 0;
                    string left = args["left_expr"]?.ToString();
                    string op = args["operator"]?.ToString();
                    string right = args["right_expr"]?.ToString();
                    string note = args["note"]?.ToString();
                    return ValidationPointService.UpdateValidationPoint(vid, left, op, right, note);
                });

            // remove_validation_point
            ToolRegistry.Register("remove_validation_point",
                "删除指定的验证点（校验公式）。当需要移除验证规则时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["validation_id"] = new JObject { ["type"] = "integer", ["description"] = "验证点 ID" }
                    },
                    ["required"] = new JArray { "validation_id" }
                },
                (args) =>
                {
                    long vid = args["validation_id"]?.Value<long>() ?? 0;
                    return ValidationPointService.RemoveValidationPoint(vid);
                });

            // run_validation
            ToolRegistry.Register("run_validation",
                "执行指定验证点并返回验证结果（通过/失败）。当需要检查数据是否符合验证规则时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["validation_id"] = new JObject { ["type"] = "integer", ["description"] = "验证点 ID" }
                    },
                    ["required"] = new JArray { "validation_id" }
                },
                (args) =>
                {
                    long vid = args["validation_id"]?.Value<long>() ?? 0;
                    return ValidationPointService.RunValidation(vid);
                });

            // get_validation_errors - 覆盖 PreviousError/NextError/DocPreviousError/DocNextError 命令
            ToolRegistry.Register("get_validation_errors",
                "获取所有未通过的验证错误列表。可选按 table_node_id 过滤。覆盖 AppCommands.PreviousError/NextError/DocPreviousError/DocNextError。当需要遍历验证错误时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "按表格节点 ID 过滤（可选，不传则查全部）" }
                    }
                },
                (args) =>
                {
                    long? tid = args["table_node_id"]?.Value<long>();
                    return ValidationPointService.GetValidationErrors(tid);
                });

            // get_next_validation_error - 覆盖 NextError / DocNextError
            ToolRegistry.Register("get_next_validation_error",
                "获取下一个验证错误（基于当前 error_index）。返回 found=true 时表示找到下一个错误。覆盖 AppCommands.NextError/DocNextError。当需要逐个向下查看错误时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["current_index"] = new JObject { ["type"] = "integer", ["description"] = "当前错误索引（首次调用传 -1）" },
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "按表格节点 ID 过滤（可选）" }
                    },
                    ["required"] = new JArray { "current_index" }
                },
                (args) =>
                {
                    int idx = args["current_index"]?.Value<int>() ?? -1;
                    long? tid = args["table_node_id"]?.Value<long>();
                    return ValidationPointService.GetNextValidationError(tid, idx);
                });

            // get_previous_validation_error - 覆盖 PreviousError / DocPreviousError
            ToolRegistry.Register("get_previous_validation_error",
                "获取上一个验证错误（基于当前 error_index）。返回 found=true 时表示找到上一个错误。覆盖 AppCommands.PreviousError/DocPreviousError。当需要逐个向上查看错误时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["current_index"] = new JObject { ["type"] = "integer", ["description"] = "当前错误索引（首次调用传错误总数）" },
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "按表格节点 ID 过滤（可选）" }
                    },
                    ["required"] = new JArray { "current_index" }
                },
                (args) =>
                {
                    int idx = args["current_index"]?.Value<int>() ?? 0;
                    long? tid = args["table_node_id"]?.Value<long>();
                    return ValidationPointService.GetPreviousValidationError(tid, idx);
                });
        }
    }
}
