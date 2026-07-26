using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using Table = Auditai.Model.Table;
using Row = Auditai.Model.Row;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 票据管理服务
    /// 覆盖 AppCommands.TicketAdd / TicketDelete / TicketSave / TicketPrevious / TicketNext / TicketMode 等 UI 操作
    /// </summary>
    public static class TicketService
    {
        // =============================================
        // 获取票据信息
        // =============================================

        public static string GetTicketInfo(long tableNodeId)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置");

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["kind"] = ticket.Kind.ToString(),
                    ["level"] = ticket.Level.ToString(),
                    ["data_row_start"] = ticket.DataRowStart,
                    ["data_row_count"] = ticket.DataRowCount,
                    ["data_row_height"] = ticket.DataRowHeight,
                    ["table_rows_frozen_count"] = ticket.TableRowsFrozenCount,
                    ["table_cols_frozen_count"] = ticket.TableColsFrozenCount,
                    ["column_header_rows_count"] = ticket.ColumnHeaderRowsCount,
                    ["is_hidden_column"] = ticket.IsHiddenColumn,
                    ["is_allow_show_virtual_node"] = ticket.IsAllowShowVirtualNode,
                    ["columns_count"] = ticket.Columns.Count,
                    ["rows_count"] = ticket.Rows.Count,
                    ["cells_count"] = ticket.Cells.Count,
                    ["merges_count"] = ticket.Merges.Count,
                    ["records_count"] = ticket.Records.Count,
                    ["navs_count"] = ticket.Navs.Count
                };

                var cols = new JArray(ticket.Columns.Select(c => new JObject
                {
                    ["width"] = c.Width,
                    ["is_hidden_column"] = c.IsHiddenColumn,
                    ["has_field"] = c.HasField(),
                    ["font_family"] = c.FontFamily ?? "",
                    ["font_size"] = c.FontSize,
                    ["align"] = c.Align.ToString(),
                    ["bold"] = c.Bold,
                    ["italic"] = c.Italic
                }));
                result["columns"] = cols;

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取票据信息失败: " + ex.Message); }
        }

        // =============================================
        // 获取票据记录列表
        // =============================================

        public static string GetTicketRecords(long tableNodeId, int offset = 0, int limit = 50)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置");

                var records = ticket.Records;
                int total = records.Count;
                var page = records.Skip(offset).Take(limit).ToList();

                var arr = new JArray(page.Select((r, i) => new JObject
                {
                    ["record_index"] = offset + i,
                    ["row_count"] = r.Rows?.Count ?? 0,
                    ["row_indices"] = new JArray((r.Rows ?? new List<Row>()).Select(row => new JObject
                    {
                        ["index"] = row.Index,
                        ["height"] = row.Height,
                        ["visible"] = row.Visible,
                        ["role"] = row.Role.ToString()
                    }))
                }));

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["total"] = total,
                    ["offset"] = offset,
                    ["limit"] = limit,
                    ["returned"] = page.Count,
                    ["records"] = arr
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取票据记录列表失败: " + ex.Message); }
        }

        // =============================================
        // 设置票据类型
        // =============================================

        public static string SetTicketKind(long tableNodeId, string kind)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置");

                if (!Enum.TryParse(kind, true, out TicketKind tk))
                    return ErrorJson($"不支持的票据类型: {kind}");

                ticket.Kind = tk;
                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"票据类型已设置为 {tk}", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置票据类型失败: " + ex.Message); }
        }

        // =============================================
        // 设置票据级别
        // =============================================

        public static string SetTicketLevel(long tableNodeId, string level)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置");

                if (!Enum.TryParse(level, true, out TicketLevel tl))
                    return ErrorJson($"不支持的票据级别: {level}");

                ticket.Level = tl;
                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"票据级别已设置为 {tl}", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置票据级别失败: " + ex.Message); }
        }

        // =============================================
        // 设置票据数据行高
        // =============================================

        public static string SetTicketDataRowHeight(long tableNodeId, int height)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置");

                ticket.DataRowHeight = height;
                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"票据数据行高已设置为 {height}", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置票据数据行高失败: " + ex.Message); }
        }

        // =============================================
        // 设置票据冻结行列
        // =============================================

        public static string SetTicketFrozen(long tableNodeId, int? rowsFrozen, int? colsFrozen)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ticket = table.Ticket;
                if (ticket == null)
                    return ErrorJson($"表格 {tableNodeId} 没有关联的票据配置");

                if (rowsFrozen.HasValue) ticket.TableRowsFrozenCount = rowsFrozen.Value;
                if (colsFrozen.HasValue) ticket.TableColsFrozenCount = colsFrozen.Value;
                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"票据冻结设置已更新", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置票据冻结失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

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
