using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using Table = Auditai.Model.Table;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing.Printing;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 页面设置管理服务
    /// 对应 AppCommands.PaperA4/A3/B4/B5/Custom, Portrait/Landscape, Margin*, Header*, Footer*, StartPage, ScalePage* 等 UI 操作
    /// </summary>
    public static class PageSetupService
    {
        // =============================================
        // 获取页面设置
        // =============================================

        public static string GetPageSetup(long tableNodeId)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ps = table.PageSetup;

                var result = new JObject
                {
                    ["success"] = true,
                    ["table_node_id"] = tableNodeId.ToString(),
                    ["paper_kind"] = ps.PaperKind.ToString(),
                    ["paper_width"] = ps.PaperWidth,
                    ["paper_height"] = ps.PaperHeight,
                    ["direction"] = ps.Direction.ToString(),
                    ["left_margin"] = ps.LeftMargin,
                    ["right_margin"] = ps.RightMargin,
                    ["top_margin"] = ps.TopMargin,
                    ["bottom_margin"] = ps.BottomMargin,
                    ["header_margin"] = ps.HeaderMargin,
                    ["footer_margin"] = ps.FooterMargin,
                    ["fit_page_width"] = ps.FitPageWidth,
                    ["fit_page_height"] = ps.FitPageHeight,
                    ["horizontal_zoom"] = ps.HorizontalZoom,
                    ["vertical_zoom"] = ps.VerticalZoom,
                    ["start_page_no"] = ps.StartPageNo,
                    ["fixed_print_cols_num"] = ps.FixedPrintColsNum,
                    ["print_page_range"] = ps.PrintPageRange ?? "",
                    ["print_copies"] = ps.PrintCopies,
                    ["one_color"] = ps.OneColor,
                    ["has_note_border"] = ps.HasNoteBorder,
                    ["is_print_index"] = ps.IsPrintIndex,
                    ["header"] = new JObject
                    {
                        ["height"] = ps.PageHeader.Height,
                        ["left"] = ps.PageHeader.LeftValue ?? "",
                        ["center"] = ps.PageHeader.CenterValue ?? "",
                        ["right"] = ps.PageHeader.RightValue ?? ""
                    },
                    ["footer"] = new JObject
                    {
                        ["height"] = ps.PageFooter.Height,
                        ["left"] = ps.PageFooter.LeftValue ?? "",
                        ["center"] = ps.PageFooter.CenterValue ?? "",
                        ["right"] = ps.PageFooter.RightValue ?? ""
                    }
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取页面设置失败: " + ex.Message); }
        }

        // =============================================
        // 设置纸张大小
        // =============================================

        public static string SetPaperSize(long tableNodeId, string paperKind, double? width, double? height)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ps = table.PageSetup;

                if (!string.IsNullOrEmpty(paperKind))
                {
                    if (Enum.TryParse(paperKind, true, out PaperKind pk))
                    {
                        ps.PaperKind = pk;
                    }
                    else
                    {
                        return ErrorJson($"不支持的纸张类型: {paperKind}（可用: A4/A3/B4/B5/Custom 等）");
                    }
                }
                if (width.HasValue) ps.PaperWidth = width.Value;
                if (height.HasValue) ps.PaperHeight = height.Value;

                table.TagPageSetupDirty();
                table.Save();

                return Ok($"纸张大小已设置: {ps.PaperKind}", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置纸张大小失败: " + ex.Message); }
        }

        // =============================================
        // 设置页面方向
        // =============================================

        public static string SetPageOrientation(long tableNodeId, string direction)
        {
            try
            {
                var table = GetTable(tableNodeId);
                Direction dir;
                if (string.Equals(direction, "portrait", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(direction, "vertical", StringComparison.OrdinalIgnoreCase))
                {
                    dir = Direction.Vertical;
                }
                else if (string.Equals(direction, "landscape", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(direction, "horizontal", StringComparison.OrdinalIgnoreCase))
                {
                    dir = Direction.Horizontal;
                }
                else if (Enum.TryParse(direction, true, out dir))
                {
                    // 已解析
                }
                else
                {
                    return ErrorJson($"不支持的方向: {direction}（可用: Portrait/Landscape 或 Vertical/Horizontal）");
                }

                table.PageSetup.Direction = dir;
                table.TagPageSetupDirty();
                table.Save();

                return Ok($"页面方向已设置: {dir}", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置页面方向失败: " + ex.Message); }
        }

        // =============================================
        // 设置页边距
        // =============================================

        public static string SetMargins(long tableNodeId, double? left, double? right, double? top, double? bottom, double? header, double? footer)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ps = table.PageSetup;

                if (left.HasValue) ps.LeftMargin = left.Value;
                if (right.HasValue) ps.RightMargin = right.Value;
                if (top.HasValue) ps.TopMargin = top.Value;
                if (bottom.HasValue) ps.BottomMargin = bottom.Value;
                if (header.HasValue) ps.HeaderMargin = header.Value;
                if (footer.HasValue) ps.FooterMargin = footer.Value;

                table.TagPageSetupDirty();
                table.Save();

                var updated = new JObject();
                if (left.HasValue) updated["left"] = left.Value;
                if (right.HasValue) updated["right"] = right.Value;
                if (top.HasValue) updated["top"] = top.Value;
                if (bottom.HasValue) updated["bottom"] = bottom.Value;
                if (header.HasValue) updated["header"] = header.Value;
                if (footer.HasValue) updated["footer"] = footer.Value;

                return Ok("页边距已设置", tableNodeId, updated);
            }
            catch (Exception ex) { return ErrorJson("设置页边距失败: " + ex.Message); }
        }

        // =============================================
        // 设置页眉页脚
        // =============================================

        public static string SetHeaderFooter(long tableNodeId, string position, string content, bool isHeader, double? height)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var hf = isHeader ? table.PageSetup.PageHeader : table.PageSetup.PageFooter;

                if (!string.IsNullOrEmpty(position))
                {
                    switch (position.ToLowerInvariant())
                    {
                        case "left": hf.LeftValue = content ?? ""; break;
                        case "center": hf.CenterValue = content ?? ""; break;
                        case "right": hf.RightValue = content ?? ""; break;
                        default: return ErrorJson($"不支持的位置: {position}（可用: left/center/right）");
                    }
                }
                if (height.HasValue) hf.Height = height.Value;

                table.TagPageSetupDirty();
                table.Save();

                return Ok($"{(isHeader ? "页眉" : "页脚")}已设置", tableNodeId,
                    new JObject { ["position"] = position ?? "", ["content"] = content ?? "", ["is_header"] = isHeader });
            }
            catch (Exception ex) { return ErrorJson("设置页眉页脚失败: " + ex.Message); }
        }

        // =============================================
        // 设置缩放
        // =============================================

        public static string SetPrintScale(long tableNodeId, double? horizontalZoom, double? verticalZoom, bool? fitPageWidth, bool? fitPageHeight)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ps = table.PageSetup;

                if (horizontalZoom.HasValue) ps.HorizontalZoom = horizontalZoom.Value;
                if (verticalZoom.HasValue) ps.VerticalZoom = verticalZoom.Value;
                if (fitPageWidth.HasValue) ps.FitPageWidth = fitPageWidth.Value;
                if (fitPageHeight.HasValue) ps.FitPageHeight = fitPageHeight.Value;

                table.TagPageSetupDirty();
                table.Save();

                return Ok("打印缩放已设置", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置打印缩放失败: " + ex.Message); }
        }

        // =============================================
        // 设置打印选项
        // =============================================

        public static string SetPrintOptions(long tableNodeId, int? startPageNo, int? fixedPrintColsNum, string printPageRange, short? printCopies, bool? oneColor, bool? hasNoteBorder, bool? isPrintIndex)
        {
            try
            {
                var table = GetTable(tableNodeId);
                var ps = table.PageSetup;

                if (startPageNo.HasValue) ps.StartPageNo = startPageNo.Value;
                if (fixedPrintColsNum.HasValue) ps.FixedPrintColsNum = fixedPrintColsNum.Value;
                if (!string.IsNullOrEmpty(printPageRange)) ps.PrintPageRange = printPageRange;
                if (printCopies.HasValue) ps.PrintCopies = printCopies.Value;
                if (oneColor.HasValue) ps.OneColor = oneColor.Value;
                if (hasNoteBorder.HasValue) ps.HasNoteBorder = hasNoteBorder.Value;
                if (isPrintIndex.HasValue) ps.IsPrintIndex = isPrintIndex.Value;

                table.TagPageSetupDirty();
                table.Save();

                return Ok("打印选项已设置", tableNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置打印选项失败: " + ex.Message); }
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
