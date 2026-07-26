using System;
using System.Linq;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 采集与汇总 MCP 工具注册
    /// 覆盖 AppCommands.CollectByColumn / CollectByCell / ExecuteCollect / ConsolidateSetting /
    /// ExecuteConsolidateFull / ExecuteConsolidateBrief / RefreshConsolidate / OneClickCollect 等 UI 操作
    /// </summary>
    public static class CollectConsolidateTools
    {
        public static void Register()
        {
            // get_collect_config
            ToolRegistry.Register("get_collect_config",
                "获取指定表格的采集配置与已采集单元格信息（含前 20 个样本）。当需要查看采集状态时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    return CollectConsolidateService.GetCollectConfig(tid);
                });

            // collect_cells
            ToolRegistry.Register("collect_cells",
                "采集指定单元格（按 row*10000+col 编码的索引数组）。当需要标记特定单元格为采集状态时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["cell_indices"] = new JObject
                        {
                            ["type"] = "array",
                            ["items"] = new JObject { ["type"] = "integer" },
                            ["description"] = "单元格索引数组，每个索引 = row * 10000 + col"
                        },
                        ["collect_key"] = new JObject { ["type"] = "string", ["description"] = "采集键/批次标识（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id", "cell_indices" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    var arr = args["cell_indices"] as JArray;
                    int[] indices = arr?.Select(t => t.Value<int>()).ToArray() ?? new int[0];
                    string key = args["collect_key"]?.ToString();
                    return CollectConsolidateService.CollectCells(tid, indices, key);
                });

            // collect_by_column
            ToolRegistry.Register("collect_by_column",
                "采集指定列的所有单元格。当需要按整列进行数据采集时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col_index"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" },
                        ["collect_key"] = new JObject { ["type"] = "string", ["description"] = "采集键/批次标识（可选）" }
                    },
                    ["required"] = new JArray { "table_node_id", "col_index" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col_index"]?.Value<int>() ?? 0;
                    string key = args["collect_key"]?.ToString();
                    return CollectConsolidateService.CollectByColumn(tid, col, key);
                });

            // get_consolidate_config
            ToolRegistry.Register("get_consolidate_config",
                "获取指定表格的汇总配置与跨项目引用信息。当需要查看汇总配置时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    return CollectConsolidateService.GetConsolidateConfig(tid);
                });

            // execute_consolidate
            ToolRegistry.Register("execute_consolidate",
                "执行汇总：刷新表格中所有含 REF 公式的单元格。当需要重新计算跨项目引用数据时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["full_refresh"] = new JObject { ["type"] = "boolean", ["description"] = "是否全量刷新（默认 false）" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    bool full = args["full_refresh"]?.Value<bool>() ?? false;
                    return CollectConsolidateService.ExecuteConsolidate(tid, full);
                });

            // one_click_collect
            ToolRegistry.Register("one_click_collect",
                "一键采集：采集表格中所有非表头/标题行的数据单元格。当需要快速采集全部数据时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" }
                    },
                    ["required"] = new JArray { "table_node_id" }
                },
                (args) =>
                {
                    long tid = args["table_node_id"]?.Value<long>() ?? 0;
                    return CollectConsolidateService.OneClickCollect(tid);
                });
        }
    }
}
