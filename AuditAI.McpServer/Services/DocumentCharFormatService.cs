using System;
using System.Linq;
using System.Xml.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using Paragraph = Auditai.Model.Paragraph;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 文档字符格式管理服务
    /// 覆盖 AppCommands.DocumentFont / DocumentFontSize / DocForeColor / DocBackColor /
    /// Bold / Italic / Underline / DoubleUnderline / Subscript / Superscript 等 UI 操作
    /// 通过修改段落 OOXML 的 w:pPr/w:rPr 实现段落默认字符格式
    /// </summary>
    public static class DocumentCharFormatService
    {
        private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        // =============================================
        // 获取段落字符格式
        // =============================================

        public static string GetCharFormat(long documentNodeId, int paragraphIndex)
        {
            try
            {
                var para = GetParagraph(documentNodeId, paragraphIndex);
                var fmt = ParseCharFormat(para.Stream);

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["paragraph_index"] = paragraphIndex,
                    ["font_family"] = fmt.FontFamily ?? "",
                    ["font_size"] = fmt.FontSize,
                    ["bold"] = fmt.Bold,
                    ["italic"] = fmt.Italic,
                    ["underline"] = fmt.Underline ?? "none",
                    ["fore_color"] = fmt.ForeColor ?? "",
                    ["back_color"] = fmt.BackColor ?? "",
                    ["vertical_align"] = fmt.VerticalAlign ?? "baseline"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取字符格式失败: " + ex.Message); }
        }

        // =============================================
        // 设置段落字符格式（一次可设置多个属性）
        // =============================================

        public static string SetCharFormat(long documentNodeId, int paragraphIndex,
            string fontFamily = null, double? fontSize = null,
            bool? bold = null, bool? italic = null,
            string underline = null,
            string foreColor = null, string backColor = null,
            string verticalAlign = null)
        {
            try
            {
                var para = GetParagraph(documentNodeId, paragraphIndex);
                string stream = para.Stream ?? "<w:p xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"/>";
                var xDoc = XElement.Parse(stream);

                // 获取或创建 w:pPr
                var pPr = xDoc.Descendants(W + "pPr").FirstOrDefault();
                if (pPr == null)
                {
                    pPr = new XElement(W + "pPr");
                    xDoc.AddFirst(pPr);
                }

                // 获取或创建 w:pPr/w:rPr（段落标记的字符格式）
                var rPr = pPr.Element(W + "rPr");
                if (rPr == null)
                {
                    rPr = new XElement(W + "rPr");
                    pPr.Add(rPr);
                }

                // 字体族
                if (!string.IsNullOrEmpty(fontFamily))
                {
                    var rFonts = rPr.Element(W + "rFonts");
                    if (rFonts == null)
                    {
                        rFonts = new XElement(W + "rFonts");
                        rPr.AddFirst(rFonts);
                    }
                    rFonts.SetAttributeValue(W + "ascii", fontFamily);
                    rFonts.SetAttributeValue(W + "hAnsi", fontFamily);
                    rFonts.SetAttributeValue(W + "eastAsia", fontFamily);
                    rFonts.SetAttributeValue(W + "cs", fontFamily);
                }

                // 字号（OOXML 使用半磅为单位）
                if (fontSize.HasValue)
                {
                    SetOrRemoveElement(rPr, W + "sz", ((int)(fontSize.Value * 2)).ToString());
                    SetOrRemoveElement(rPr, W + "szCs", ((int)(fontSize.Value * 2)).ToString());
                }

                // 粗体
                if (bold.HasValue)
                {
                    if (bold.Value) SetOrRemoveElement(rPr, W + "b", null);
                    else rPr.Element(W + "b")?.Remove();
                }

                // 斜体
                if (italic.HasValue)
                {
                    if (italic.Value) SetOrRemoveElement(rPr, W + "i", null);
                    else rPr.Element(W + "i")?.Remove();
                }

                // 下划线（none/single/double）
                if (!string.IsNullOrEmpty(underline))
                {
                    string val = underline.ToLowerInvariant();
                    if (val == "none")
                    {
                        rPr.Element(W + "u")?.Remove();
                    }
                    else
                    {
                        string uVal = val == "double" ? "double" : "single";
                        var u = rPr.Element(W + "u");
                        if (u == null)
                        {
                            u = new XElement(W + "u");
                            rPr.Add(u);
                        }
                        u.SetAttributeValue(W + "val", uVal);
                    }
                }

                // 前景色（如 "FF0000" 表示红色，不带 #）
                if (!string.IsNullOrEmpty(foreColor))
                {
                    string colorVal = foreColor.TrimStart('#').ToUpperInvariant();
                    var color = rPr.Element(W + "color");
                    if (color == null)
                    {
                        color = new XElement(W + "color");
                        rPr.Add(color);
                    }
                    color.SetAttributeValue(W + "val", colorVal);
                }

                // 背景色（底纹）
                if (!string.IsNullOrEmpty(backColor))
                {
                    string fillVal = backColor.TrimStart('#').ToUpperInvariant();
                    var shd = rPr.Element(W + "shd");
                    if (shd == null)
                    {
                        shd = new XElement(W + "shd");
                        rPr.Add(shd);
                    }
                    shd.SetAttributeValue(W + "val", "clear");
                    shd.SetAttributeValue(W + "color", "auto");
                    shd.SetAttributeValue(W + "fill", fillVal);
                }

                // 上标/下标（baseline/subscript/superscript）
                if (!string.IsNullOrEmpty(verticalAlign))
                {
                    string vaVal = verticalAlign.ToLowerInvariant();
                    if (vaVal == "baseline")
                    {
                        rPr.Element(W + "vertAlign")?.Remove();
                    }
                    else
                    {
                        string xmlVal = vaVal == "superscript" ? "superscript" : "subscript";
                        var va = rPr.Element(W + "vertAlign");
                        if (va == null)
                        {
                            va = new XElement(W + "vertAlign");
                            rPr.Add(va);
                        }
                        va.SetAttributeValue(W + "val", xmlVal);
                    }
                }

                para.UpdateStream(xDoc.ToString(), para.Section);
                SaveDocument(documentNodeId);

                return Ok($"段落 {paragraphIndex} 字符格式已更新", documentNodeId,
                    new JObject
                    {
                        ["font_family"] = fontFamily ?? "",
                        ["font_size"] = fontSize ?? 0,
                        ["bold"] = bold ?? false,
                        ["italic"] = italic ?? false,
                        ["underline"] = underline ?? "",
                        ["fore_color"] = foreColor ?? "",
                        ["back_color"] = backColor ?? "",
                        ["vertical_align"] = verticalAlign ?? ""
                    });
            }
            catch (Exception ex) { return ErrorJson("设置字符格式失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static void SetOrRemoveElement(XElement parent, XName name, string val)
        {
            var el = parent.Element(name);
            if (el == null)
            {
                el = new XElement(name);
                parent.Add(el);
            }
            if (!string.IsNullOrEmpty(val))
                el.SetAttributeValue(W + "val", val);
        }

        private static CharFormat ParseCharFormat(string stream)
        {
            var fmt = new CharFormat();
            if (string.IsNullOrEmpty(stream)) return fmt;
            try
            {
                var xDoc = XElement.Parse(stream);
                var rPr = xDoc.Descendants(W + "pPr").FirstOrDefault()?.Element(W + "rPr")
                          ?? xDoc.Descendants(W + "rPr").FirstOrDefault();
                if (rPr == null) return fmt;

                var rFonts = rPr.Element(W + "rFonts");
                if (rFonts != null)
                {
                    fmt.FontFamily = (string)rFonts.Attribute(W + "ascii")
                        ?? (string)rFonts.Attribute(W + "eastAsia")
                        ?? "";
                }

                var sz = rPr.Element(W + "sz");
                if (sz != null)
                {
                    int halfPts = 0;
                    if (int.TryParse((string)sz.Attribute(W + "val"), out halfPts))
                        fmt.FontSize = halfPts / 2.0;
                }

                fmt.Bold = rPr.Element(W + "b") != null;
                fmt.Italic = rPr.Element(W + "i") != null;

                var u = rPr.Element(W + "u");
                if (u != null)
                {
                    fmt.Underline = (string)u.Attribute(W + "val") ?? "single";
                }
                else
                {
                    fmt.Underline = "none";
                }

                var color = rPr.Element(W + "color");
                if (color != null)
                {
                    fmt.ForeColor = (string)color.Attribute(W + "val");
                }

                var shd = rPr.Element(W + "shd");
                if (shd != null)
                {
                    fmt.BackColor = (string)shd.Attribute(W + "fill");
                }

                var va = rPr.Element(W + "vertAlign");
                if (va != null)
                {
                    fmt.VerticalAlign = (string)va.Attribute(W + "val");
                }
                else
                {
                    fmt.VerticalAlign = "baseline";
                }
            }
            catch { /* 解析失败返回默认值 */ }
            return fmt;
        }

        private static Paragraph GetParagraph(long documentNodeId, int paragraphIndex)
        {
            var project = EnsureProject();
            var id = new Id64(documentNodeId);
            var docNode = project.GetAllDocumentNodes().FirstOrDefault(d => d.Id == id);
            if (docNode == null)
                throw new InvalidOperationException($"未找到文档节点: {documentNodeId}");
            docNode.Document.LoadAndReturn();

            if (paragraphIndex < 0 || paragraphIndex >= docNode.Document.Paragraphs.Count)
                throw new InvalidOperationException($"段落索引 {paragraphIndex} 超出范围（共 {docNode.Document.Paragraphs.Count} 段）");

            return docNode.Document.Paragraphs[paragraphIndex];
        }

        private static Project EnsureProject()
        {
            var project = SessionState.Current.CurrentProject;
            if (project == null)
                throw new InvalidOperationException("未打开项目，请先调用 open_project 工具");
            Project.Current = project;
            return project;
        }

        private static void SaveDocument(long documentNodeId)
        {
            var project = SessionState.Current.CurrentProject;
            if (project == null) return;
            var id = new Id64(documentNodeId);
            var docNode = project.GetAllDocumentNodes().FirstOrDefault(d => d.Id == id);
            docNode?.Document?.Save();
        }

        private static string Ok(string msg, long documentNodeId, JObject extra = null)
        {
            var r = new JObject { ["success"] = true, ["document_node_id"] = documentNodeId.ToString(), ["message"] = msg };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }

        private class CharFormat
        {
            public string FontFamily;
            public double FontSize;
            public bool Bold;
            public bool Italic;
            public string Underline;
            public string ForeColor;
            public string BackColor;
            public string VerticalAlign;
        }
    }
}
