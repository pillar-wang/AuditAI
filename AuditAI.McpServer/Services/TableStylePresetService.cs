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
    /// 表格样式预设服务
    /// 覆盖 AppCommands.TableStyle0/1/2/3/4/NoLine/Custom/Style/BatchApplyTableStyle 等 UI 操作
    /// 通过 Table.UpdateBorderStyle 应用预设边框样式
    /// </summary>
    public static class TableStylePresetService
    {
        // =============================================
        // 获取表格样式
        // =============================================

        public static string GetTableStyle(long tableNodeId)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var bs = table.BorderStyle;

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["internal_number"] = bs?.InternalNumber ?? 0,
                    ["is_custom_style"] = bs?.IsCustomStyle ?? false,
                    ["up_down_line"] = bs?.UpDownLine.ToString() ?? "None",
                    ["left_right_line"] = bs?.LeftRightLine.ToString() ?? "None",
                    ["body_line"] = bs?.BodyLine.ToString() ?? "None",
                    ["second_line"] = bs?.SecondLine.ToString() ?? "None",
                    ["has_custom_json"] = !string.IsNullOrEmpty(table.CustomBorderStyle)
                };

                if (!string.IsNullOrEmpty(table.CustomBorderStyle))
                {
                    result["custom_border_style"] = table.CustomBorderStyle;
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取表格样式失败: " + ex.Message); }
        }

        // =============================================
        // 应用预设样式
        // preset: grid / thick_up_down_dash_body / thick_up_down_thin_body /
        //         thick_border_thin_body / no_line / custom
        // =============================================

        public static string ApplyPreset(long tableNodeId, string preset)
        {
            try
            {
                var table = GetTable(tableNodeId);
                TableBorderStyle bs;

                switch ((preset ?? "").ToLowerInvariant())
                {
                    case "grid":
                    case "0":
                        bs = TableBorderStyles.Grid;
                        break;
                    case "thick_up_down_dash_body":
                    case "1":
                        bs = TableBorderStyles.ThickUpDownDashBody;
                        break;
                    case "thick_up_down_thin_body":
                    case "2":
                        bs = TableBorderStyles.ThickUpDownThinBody;
                        break;
                    case "thick_border_thin_body":
                    case "3":
                        bs = TableBorderStyles.ThickBorderThinBody;
                        break;
                    case "no_line":
                    case "4":
                        bs = TableBorderStyles.NoLine;
                        break;
                    case "custom":
                        bs = TableBorderStyles.CreateCustom();
                        break;
                    default:
                        return ErrorJson($"不支持的预设样式: {preset}（可用: grid/thick_up_down_dash_body/thick_up_down_thin_body/thick_border_thin_body/no_line/custom）");
                }

                table.UpdateBorderStyle(bs);
                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok($"已应用预设样式: {preset} (internal_number={bs.InternalNumber})", tableNodeId,
                    new JObject { ["preset"] = preset, ["internal_number"] = bs.InternalNumber });
            }
            catch (Exception ex) { return ErrorJson("应用预设样式失败: " + ex.Message); }
        }

        // =============================================
        // 设置自定义边框样式（细粒度）
        // =============================================

        public static string SetCustomBorderStyle(long tableNodeId,
            string upDownLine = null, string leftRightLine = null,
            string bodyLine = null, string secondLine = null,
            bool keywordRowBoldUnderline = false, string keywordList = null)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var bs = TableBorderStyles.CreateCustom();

                if (!string.IsNullOrEmpty(upDownLine) && TryParseLineStyle(upDownLine, out var ud))
                    bs.UpDownLine = ud;
                if (!string.IsNullOrEmpty(leftRightLine) && TryParseLineStyle(leftRightLine, out var lr))
                    bs.LeftRightLine = lr;
                if (!string.IsNullOrEmpty(bodyLine) && TryParseLineStyle(bodyLine, out var bl))
                    bs.BodyLine = bl;
                if (!string.IsNullOrEmpty(secondLine) && TryParseLineStyle(secondLine, out var sl))
                    bs.SecondLine = sl;
                if (keywordRowBoldUnderline)
                    bs.KeywordRowBoldUnderline = true;
                if (!string.IsNullOrEmpty(keywordList))
                    bs.KeywordList = keywordList;

                table.UpdateBorderStyle(bs);
                table.TreeNode.Dirty = 1;
                table.Save();

                return Ok("已应用自定义边框样式", tableNodeId,
                    new JObject
                    {
                        ["up_down_line"] = bs.UpDownLine.ToString(),
                        ["left_right_line"] = bs.LeftRightLine.ToString(),
                        ["body_line"] = bs.BodyLine.ToString(),
                        ["second_line"] = bs.SecondLine.ToString(),
                        ["keyword_row_bold_underline"] = bs.KeywordRowBoldUnderline,
                        ["keyword_list"] = bs.KeywordList
                    });
            }
            catch (Exception ex) { return ErrorJson("设置自定义边框样式失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static bool TryParseLineStyle(string s, out LineStyle ls)
        {
            ls = LineStyle.None;
            if (string.IsNullOrEmpty(s)) return false;
            switch (s.ToLowerInvariant())
            {
                case "none": ls = LineStyle.None; return true;
                case "thin": ls = LineStyle.Thin; return true;
                case "thick": ls = LineStyle.Thick; return true;
                case "dash": ls = LineStyle.Dash; return true;
                case "dotted":
                case "dot": ls = LineStyle.Dotted; return true;
                case "dotdash":
                case "dashdot": ls = LineStyle.DotDash; return true;
                case "doubledotdash":
                case "dashdotdot": ls = LineStyle.DoubleDotDash; return true;
                default: return false;
            }
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
