using System;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using Table = Auditai.Model.Table;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 单元格边框服务
    /// 覆盖 AppCommands.TicketBorderTop / Bottom / Left / Right / None / All / Border1 / Border2 等 UI 操作
    /// 票据单元格的边框存储在 TicketCell.Top/Right/Bottom/Left (TicketBorder 类型)
    /// </summary>
    public static class CellBorderService
    {
        // =============================================
        // 获取单元格边框
        // =============================================

        public static string GetCellBorders(long tableNodeId, int row, int col)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置，无法获取边框");

                var cell = GetTicketCell(ticket, row, col);
                if (cell == null)
                    return ErrorJson($"单元格 ({row},{col}) 不存在");

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["row"] = row,
                    ["col"] = col,
                    ["top"] = BorderToJObject(cell.Top),
                    ["right"] = BorderToJObject(cell.Right),
                    ["bottom"] = BorderToJObject(cell.Bottom),
                    ["left"] = BorderToJObject(cell.Left)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取单元格边框失败: " + ex.Message); }
        }

        // =============================================
        // 设置单元格单条边框
        // edge: top / right / bottom / left
        // width: 0=无边框, 1=细线, 2=中线, 3=粗线
        // =============================================

        public static string SetCellBorder(long tableNodeId, int row, int col, string edge, int width)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置，无法设置边框");

                var cell = GetTicketCell(ticket, row, col);
                if (cell == null)
                    return ErrorJson($"单元格 ({row},{col}) 不存在");

                TicketBorder border;
                switch ((edge ?? "").ToLowerInvariant())
                {
                    case "top": border = cell.Top ?? (cell.Top = new TicketBorder()); break;
                    case "right": border = cell.Right ?? (cell.Right = new TicketBorder()); break;
                    case "bottom": border = cell.Bottom ?? (cell.Bottom = new TicketBorder()); break;
                    case "left": border = cell.Left ?? (cell.Left = new TicketBorder()); break;
                    default: return ErrorJson($"不支持的边框方向: {edge}（应为 top/right/bottom/left）");
                }
                border.Width = width;

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"单元格 ({row},{col}) 的 {edge} 边框已设置 (width={width})", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置单元格边框失败: " + ex.Message); }
        }

        // =============================================
        // 设置单元格所有边框
        // 覆盖 TicketBorderAll / TicketBorderNone
        // =============================================

        public static string SetCellAllBorders(long tableNodeId, int row, int col, int width)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置，无法设置边框");

                var cell = GetTicketCell(ticket, row, col);
                if (cell == null)
                    return ErrorJson($"单元格 ({row},{col}) 不存在");

                if (cell.Top == null) cell.Top = new TicketBorder();
                if (cell.Right == null) cell.Right = new TicketBorder();
                if (cell.Bottom == null) cell.Bottom = new TicketBorder();
                if (cell.Left == null) cell.Left = new TicketBorder();
                cell.Top.Width = width;
                cell.Right.Width = width;
                cell.Bottom.Width = width;
                cell.Left.Width = width;

                table.TreeNode.Dirty = 1;
                table.Save();

                string desc = width == 0 ? "已清除所有边框" : $"已设置所有边框 (width={width})";
                return Ok($"单元格 ({row},{col}) {desc}", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置单元格所有边框失败: " + ex.Message); }
        }

        // =============================================
        // 批量设置单元格边框（支持区域）
        // 覆盖 TicketBorder1 / TicketBorder2（外框线/内框线）
        // =============================================

        public static string SetRangeBorders(long tableNodeId, int startRow, int startCol, int endRow, int endCol,
            string mode, int width)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置，无法设置边框");

                int colCount = ticket.Columns.Count;
                int rowCount = ticket.Rows.Count;
                int affected = 0;

                for (int r = Math.Max(0, startRow); r <= Math.Min(endRow, rowCount - 1); r++)
                {
                    for (int c = Math.Max(0, startCol); c <= Math.Min(endCol, colCount - 1); c++)
                    {
                        var cell = GetTicketCell(ticket, r, c);
                        if (cell == null) continue;

                        if (cell.Top == null) cell.Top = new TicketBorder();
                        if (cell.Right == null) cell.Right = new TicketBorder();
                        if (cell.Bottom == null) cell.Bottom = new TicketBorder();
                        if (cell.Left == null) cell.Left = new TicketBorder();

                        switch ((mode ?? "").ToLowerInvariant())
                        {
                            case "all":
                                cell.Top.Width = width;
                                cell.Right.Width = width;
                                cell.Bottom.Width = width;
                                cell.Left.Width = width;
                                break;
                            case "none":
                                cell.Top.Width = 0;
                                cell.Right.Width = 0;
                                cell.Bottom.Width = 0;
                                cell.Left.Width = 0;
                                break;
                            case "outline":
                                if (r == startRow) cell.Top.Width = width;
                                if (r == endRow) cell.Bottom.Width = width;
                                if (c == startCol) cell.Left.Width = width;
                                if (c == endCol) cell.Right.Width = width;
                                break;
                            case "inner":
                                if (r > startRow) cell.Top.Width = width;
                                if (r < endRow) cell.Bottom.Width = width;
                                if (c > startCol) cell.Left.Width = width;
                                if (c < endCol) cell.Right.Width = width;
                                break;
                            default:
                                return ErrorJson($"不支持的边框模式: {mode}（应为 all/none/outline/inner）");
                        }
                        affected++;
                    }
                }

                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"已对 {affected} 个单元格应用 {mode} 边框 (width={width})", tableNodeId,
                    new JObject { ["affected"] = affected, ["mode"] = mode, ["width"] = width });
            }
            catch (Exception ex) { return ErrorJson("批量设置边框失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static TicketCell GetTicketCell(TicketTable ticket, int row, int col)
        {
            int colCount = ticket.Columns.Count;
            if (colCount == 0) return null;
            if (row < 0 || col < 0 || col >= colCount) return null;

            int idx = row * colCount + col;
            if (idx < 0 || idx >= ticket.Cells.Count) return null;
            return ticket.Cells[idx];
        }

        private static JObject BorderToJObject(TicketBorder b)
        {
            if (b == null) return new JObject { ["width"] = 0 };
            return new JObject { ["width"] = b.Width };
        }

        private static Table GetTable(long tableNodeId)
        {
            var project = EnsureProject();
            var id = new Id64(tableNodeId);
            var table = project.GetTableById(id);
            if (table == null)
                throw new InvalidOperationException($"未找到表格节点: {tableNodeId}");
            table.LoadAndReturn(true);
            return table;
        }

        private static Project EnsureProject()
        {
            var project = SessionState.Current.CurrentProject;
            if (project == null)
                throw new InvalidOperationException("未打开项目，请先调用 open_project 工具");
            Project.Current = project;
            return project;
        }

        private static string Ok(string msg, long tableNodeId, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["table_node_id"] = tableNodeId.ToString(), ["message"] = msg };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
