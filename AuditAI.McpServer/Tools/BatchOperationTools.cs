using System;
using System.Linq;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 批量列操作与搜索 MCP 工具注册
    /// 覆盖 AppCommands.BatchColumnDuplicate / BatchColumnRemove / BatchColumnRename / Find 等 UI 操作
    /// </summary>
    public static class BatchOperationTools
    {
        public static void Register()
        {
            // batch_duplicate_column
            ToolRegistry.Register("batch_duplicate_column",
                "批量复制表格列。传入要复制的列索引数组，可指定新列名后缀。当需要批量复制列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col_indices"] = new JObject { ["type"] = "array", ["items"] = new JObject { ["type"] = "integer" }, ["description"] = "要复制的列索引数组" },
                        ["name_suffix"] = new JObject { ["type"] = "string", ["description"] = "新列名后缀（默认 _copy）" }
                    },
                    ["required"] = new JArray { "table_node_id", "col_indices" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    var idxArr = args["col_indices"] as JArray;
                    int[] indices = idxArr?.Select(x => x.Value<int>()).ToArray() ?? new int[0];
                    string suffix = args["name_suffix"]?.ToString() ?? "_copy";
                    return BatchOperationService.BatchDuplicateColumn(id, indices, suffix);
                });

            // batch_remove_column
            ToolRegistry.Register("batch_remove_column",
                "批量删除表格列。传入要删除的列索引数组。当需要批量删除列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col_indices"] = new JObject { ["type"] = "array", ["items"] = new JObject { ["type"] = "integer" }, ["description"] = "要删除的列索引数组" }
                    },
                    ["required"] = new JArray { "table_node_id", "col_indices" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    var idxArr = args["col_indices"] as JArray;
                    int[] indices = idxArr?.Select(x => x.Value<int>()).ToArray() ?? new int[0];
                    return BatchOperationService.BatchRemoveColumn(id, indices);
                });

            // batch_rename_column
            ToolRegistry.Register("batch_rename_column",
                "批量重命名表格列。传入包含 col_index 和 new_name 的对象数组。当需要批量重命名列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["renames"] = new JObject
                        {
                            ["type"] = "array",
                            ["description"] = "重命名对象数组",
                            ["items"] = new JObject
                            {
                                ["type"] = "object",
                                ["properties"] = new JObject
                                {
                                    ["col_index"] = new JObject { ["type"] = "integer" },
                                    ["new_name"] = new JObject { ["type"] = "string" }
                                }
                            }
                        }
                    },
                    ["required"] = new JArray { "table_node_id", "renames" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    var renamesArr = args["renames"] as JArray;
                    JObject[] renames = renamesArr?.Select(x => (JObject)x).ToArray() ?? new JObject[0];
                    return BatchOperationService.BatchRenameColumn(id, renames);
                });

            // find_in_table
            ToolRegistry.Register("find_in_table",
                "在指定表格中搜索关键字。返回匹配的单元格位置和内容。当需要在表格中查找数据时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["keyword"] = new JObject { ["type"] = "string", ["description"] = "搜索关键字" },
                        ["match_case"] = new JObject { ["type"] = "boolean", ["description"] = "是否区分大小写（默认 false）" },
                        ["match_whole_cell"] = new JObject { ["type"] = "boolean", ["description"] = "是否整单元格匹配（默认 false）" }
                    },
                    ["required"] = new JArray { "table_node_id", "keyword" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    string kw = args["keyword"]?.ToString();
                    bool mc = args["match_case"]?.Value<bool>() ?? false;
                    bool mwc = args["match_whole_cell"]?.Value<bool>() ?? false;
                    return BatchOperationService.FindInTable(id, kw, mc, mwc);
                });

            // find_in_all_tables
            ToolRegistry.Register("find_in_all_tables",
                "在项目所有表格中搜索关键字。返回每个表格的匹配结果。当需要全局搜索表格数据时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["keyword"] = new JObject { ["type"] = "string", ["description"] = "搜索关键字" },
                        ["match_case"] = new JObject { ["type"] = "boolean", ["description"] = "是否区分大小写（默认 false）" },
                        ["match_whole_cell"] = new JObject { ["type"] = "boolean", ["description"] = "是否整单元格匹配（默认 false）" }
                    },
                    ["required"] = new JArray { "keyword" }
                },
                (args) =>
                {
                    string kw = args["keyword"]?.ToString();
                    bool mc = args["match_case"]?.Value<bool>() ?? false;
                    bool mwc = args["match_whole_cell"]?.Value<bool>() ?? false;
                    return BatchOperationService.FindInAllTables(kw, mc, mwc);
                });

            // find_in_document
            ToolRegistry.Register("find_in_document",
                "在指定文档中搜索关键字。返回匹配的段落索引和片段。当需要在文档中查找文字时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["document_node_id"] = new JObject { ["type"] = "integer", ["description"] = "文档节点 ID" },
                        ["keyword"] = new JObject { ["type"] = "string", ["description"] = "搜索关键字" },
                        ["match_case"] = new JObject { ["type"] = "boolean", ["description"] = "是否区分大小写（默认 false）" }
                    },
                    ["required"] = new JArray { "document_node_id", "keyword" }
                },
                (args) =>
                {
                    long id = args["document_node_id"]?.Value<long>() ?? 0;
                    string kw = args["keyword"]?.ToString();
                    bool mc = args["match_case"]?.Value<bool>() ?? false;
                    return BatchOperationService.FindInDocument(id, kw, mc);
                });
        }
    }
}
