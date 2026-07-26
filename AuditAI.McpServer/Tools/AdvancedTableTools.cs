using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 高级表格操作 MCP 工具注册
    /// 提供拆分单元格、行列移动、行高列宽、隐藏、行角色、列重命名/公式、
    /// 锁定、冻结列、排序、控制公式、表格信息等扩展能力
    /// </summary>
    public static class AdvancedTableTools
    {
        public static void Register()
        {
            // split_cells
            ToolRegistry.Register("split_cells",
                "拆分合并单元格（取消指定单元格所在的合并区域）。当需要取消单元格合并时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引（从 0 开始）" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引（从 0 开始）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "col" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return AdvancedTableService.SplitCells(id, row, col);
                });

            // move_table_row
            ToolRegistry.Register("move_table_row",
                "移动表格行：将位于 from_index 的 count 行移动到 to_index 位置。当需要调整表格行顺序时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["from_index"] = new JObject { ["type"] = "integer", ["description"] = "要移动的起始行索引" },
                        ["to_index"] = new JObject { ["type"] = "integer", ["description"] = "目标行索引位置" },
                        ["count"] = new JObject { ["type"] = "integer", ["description"] = "要移动的行数（可选，默认 1）" }
                    },
                    ["required"] = new JArray { "table_node_id", "from_index", "to_index" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int from = args["from_index"]?.Value<int>() ?? 0;
                    int to = args["to_index"]?.Value<int>() ?? 0;
                    int count = args["count"]?.Value<int>() ?? 1;
                    return AdvancedTableService.MoveRow(id, from, to, count);
                });

            // move_table_column
            ToolRegistry.Register("move_table_column",
                "移动表格列：将位于 from_index 的 count 列移动到 to_index 位置。当需要调整表格列顺序时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["from_index"] = new JObject { ["type"] = "integer", ["description"] = "要移动的起始列索引" },
                        ["to_index"] = new JObject { ["type"] = "integer", ["description"] = "目标列索引位置" },
                        ["count"] = new JObject { ["type"] = "integer", ["description"] = "要移动的列数（可选，默认 1）" }
                    },
                    ["required"] = new JArray { "table_node_id", "from_index", "to_index" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int from = args["from_index"]?.Value<int>() ?? 0;
                    int to = args["to_index"]?.Value<int>() ?? 0;
                    int count = args["count"]?.Value<int>() ?? 1;
                    return AdvancedTableService.MoveColumn(id, from, to, count);
                });

            // set_row_height
            ToolRegistry.Register("set_row_height",
                "设置表格行高（像素）。当需要调整某行高度时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["height"] = new JObject { ["type"] = "integer", ["description"] = "行高（像素）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "height" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    int height = args["height"]?.Value<int>() ?? 0;
                    return AdvancedTableService.SetRowHeight(id, row, height);
                });

            // set_column_width
            ToolRegistry.Register("set_column_width",
                "设置表格列宽（像素）。当需要调整某列宽度时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["width"] = new JObject { ["type"] = "integer", ["description"] = "列宽（像素）" }
                    },
                    ["required"] = new JArray { "table_node_id", "col", "width" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    int width = args["width"]?.Value<int>() ?? 0;
                    return AdvancedTableService.SetColumnWidth(id, col, width);
                });

            // set_row_visible
            ToolRegistry.Register("set_row_visible",
                "设置表格行的可见性（隐藏/显示）。当需要隐藏或显示某行时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["visible"] = new JObject { ["type"] = "boolean", ["description"] = "true=显示，false=隐藏" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "visible" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    bool visible = args["visible"]?.Value<bool>() ?? true;
                    return AdvancedTableService.SetRowVisible(id, row, visible);
                });

            // set_column_visible
            ToolRegistry.Register("set_column_visible",
                "设置表格列的可见性（隐藏/显示）。当需要隐藏或显示某列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["visible"] = new JObject { ["type"] = "boolean", ["description"] = "true=显示，false=隐藏" }
                    },
                    ["required"] = new JArray { "table_node_id", "col", "visible" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    bool visible = args["visible"]?.Value<bool>() ?? true;
                    return AdvancedTableService.SetColumnVisible(id, col, visible);
                });

            // set_row_role
            ToolRegistry.Register("set_row_role",
                "设置表格行的角色类型。可选: Normal(普通)/Header(表头)/Fixed(固定)/Subtotal(小计)/Total(合计)/Among(中间)/Minus(减项)。当需要标记行用途时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["row"] = new JObject { ["type"] = "integer", ["description"] = "行索引" },
                        ["role"] = new JObject { ["type"] = "string", ["description"] = "行角色（Normal/Header/Fixed/Subtotal/Total/Among/Minus）" }
                    },
                    ["required"] = new JArray { "table_node_id", "row", "role" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int row = args["row"]?.Value<int>() ?? 0;
                    string role = args["role"]?.ToString();
                    return AdvancedTableService.SetRowRole(id, row, role);
                });

            // rename_column
            ToolRegistry.Register("rename_column",
                "重命名表格列的标题。当需要修改列标题文字时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["caption"] = new JObject { ["type"] = "string", ["description"] = "新的列标题" }
                    },
                    ["required"] = new JArray { "table_node_id", "col", "caption" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string caption = args["caption"]?.ToString();
                    return AdvancedTableService.RenameColumn(id, col, caption);
                });

            // set_column_formula
            ToolRegistry.Register("set_column_formula",
                "设置表格列的默认公式（用于新增行时自动填充）。当需要为某列设置默认公式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" },
                        ["formula"] = new JObject { ["type"] = "string", ["description"] = "列默认公式" }
                    },
                    ["required"] = new JArray { "table_node_id", "col", "formula" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    string formula = args["formula"]?.ToString();
                    return AdvancedTableService.SetColumnFormula(id, col, formula);
                });

            // get_column_formula
            ToolRegistry.Register("get_column_formula",
                "获取表格列的公式信息（列公式、标题公式）。当需要查看列公式配置时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "列索引" }
                    },
                    ["required"] = new JArray { "table_node_id", "col" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    return AdvancedTableService.GetColumnFormula(id, col);
                });

            // lock_table
            ToolRegistry.Register("lock_table",
                "锁定或解锁表格。locker=0 表示解锁，其他值表示锁定到指定用户/会话。当需要防止他人编辑时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["locker"] = new JObject { ["type"] = "integer", ["description"] = "锁定者 ID（0=解锁，其他=锁定）" }
                    },
                    ["required"] = new JArray { "table_node_id", "locker" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    long locker = args["locker"]?.Value<long>() ?? 0;
                    return AdvancedTableService.LockTable(id, locker);
                });

            // set_frozen_columns
            ToolRegistry.Register("set_frozen_columns",
                "设置表格冻结列数量（左侧固定的列数，常用于冻结首列或首两列）。当需要锁定左侧列时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["frozen_count"] = new JObject { ["type"] = "integer", ["description"] = "冻结的列数" }
                    },
                    ["required"] = new JArray { "table_node_id", "frozen_count" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int frozen = args["frozen_count"]?.Value<int>() ?? 0;
                    return AdvancedTableService.SetFrozenColumns(id, frozen);
                });

            // sort_table_by_column
            ToolRegistry.Register("sort_table_by_column",
                "按指定列对表格数据行进行升序或降序排序。仅对数据行排序，不影响标题行/合计行。当需要对表格数据排序时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["col"] = new JObject { ["type"] = "integer", ["description"] = "排序依据列索引" },
                        ["descending"] = new JObject { ["type"] = "boolean", ["description"] = "是否降序排序（可选，默认 false 升序）" }
                    },
                    ["required"] = new JArray { "table_node_id", "col" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    int col = args["col"]?.Value<int>() ?? 0;
                    bool desc = args["descending"]?.Value<bool>() ?? false;
                    return AdvancedTableService.SortTableByColumn(id, col, desc);
                });

            // set_control_formula
            ToolRegistry.Register("set_control_formula",
                "设置表格的控制公式（用于整表的勾稽关系校验）。当需要配置表格级勾稽公式时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["table_node_id"] = new JObject { ["type"] = "integer", ["description"] = "表格节点 ID" },
                        ["formula"] = new JObject { ["type"] = "string", ["description"] = "控制公式表达式" }
                    },
                    ["required"] = new JArray { "table_node_id", "formula" }
                },
                (args) =>
                {
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    string formula = args["formula"]?.ToString();
                    return AdvancedTableService.SetControlFormula(id, formula);
                });

            // get_control_formula
            ToolRegistry.Register("get_control_formula",
                "获取表格的控制公式。当需要查看表格级勾稽公式时调用此工具。",
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
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    return AdvancedTableService.GetControlFormula(id);
                });

            // get_table_info
            ToolRegistry.Register("get_table_info",
                "获取表格的完整信息：行数、列数、冻结列、锁定状态、所有列摘要（标题/宽度/可见/公式）、所有行摘要（高度/可见/角色/锁定）。当需要全面了解表格结构时调用此工具。",
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
                    long id = args["table_node_id"]?.Value<long>() ?? 0;
                    return AdvancedTableService.GetTableInfo(id);
                });
        }
    }
}
