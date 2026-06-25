﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 导入 MCP 工具注册
    /// 提供 Excel 数据导入到表格的能力
    /// </summary>
    public static class ImportTools
    {
        /// <summary>
        /// 注册所有导入工具
        /// </summary>
        public static void Register()
        {
            // import_excel_to_table
            ToolRegistry.Register("import_excel_to_table",
                "将 Excel 文件中的数据导入到指定的表格节点中。支持指定起始行列位置，可选择是否将 Excel 第一行作为表头（表头行不导入）。导入时会自动扩展表格的行列数以容纳数据。适用于将外部数据快速导入审计底稿表格。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["file_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "Excel 文件完整路径，如 C:\\\\data\\\\导入数据.xlsx"
                        },
                        ["table_node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "目标表格节点 ID（可通过 get_project_tree 或 search_nodes 获取）"
                        },
                        ["start_row"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "数据写入的起始行索引（从 0 开始，默认为 0）",
                            ["default"] = 0
                        },
                        ["start_col"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "数据写入的起始列索引（从 0 开始，默认为 0）",
                            ["default"] = 0
                        },
                        ["has_header"] = new JObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "Excel 第一行是否为列标题（为 true 时跳过第一行不导入），默认为 false",
                            ["default"] = false
                        }
                    },
                    ["required"] = new JArray { "file_path", "table_node_id" }
                },
                (args) =>
                {
                    string filePath = args["file_path"]?.ToString();
                    long tableNodeId = args["table_node_id"]?.Value<long>() ?? 0;
                    int startRow = args["start_row"]?.Value<int>() ?? 0;
                    int startCol = args["start_col"]?.Value<int>() ?? 0;
                    bool hasHeader = args["has_header"]?.Value<bool>() ?? false;
                    return MiscService.ImportExcelToTable(filePath, tableNodeId, startRow, startCol, hasHeader);
                });
        }
    }
}