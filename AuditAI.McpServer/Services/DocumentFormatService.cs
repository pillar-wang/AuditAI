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
    /// 文档段落格式管理服务
    /// 对应 AppCommands.ParagraphAlign*, LineSpacing*, AboveSpacing*, BelowSpacing*,
    /// IndentFirstLine, UnindentFirstLine, IndentParagraph, UnindentParagraph 等 UI 操作
    /// 通过解析和修改段落 OOXML 流来实现格式控制
    /// </summary>
    public static class DocumentFormatService
    {
        // =============================================
        // 获取段落格式
        // =============================================

        public static string GetParagraphFormat(long documentNodeId, int paragraphIndex)
        {
            try
            {
                var para = GetParagraph(documentNodeId, paragraphIndex);
                var fmt = ParseParagraphFormat(para.Stream);

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["paragraph_index"] = paragraphIndex,
                    ["alignment"] = fmt.Alignment,
                    ["line_spacing"] = fmt.LineSpacing,
                    ["line_spacing_rule"] = fmt.LineSpacingRule,
                    ["space_before"] = fmt.SpaceBefore,
                    ["space_after"] = fmt.SpaceAfter,
                    ["indent_left"] = fmt.IndentLeft,
                    ["indent_right"] = fmt.IndentRight,
                    ["indent_first_line"] = fmt.IndentFirstLine,
                    ["has_comment"] = !string.IsNullOrEmpty(para.Comment)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取段落格式失败: " + ex.Message); }
        }

        // =============================================
        // 设置段落对齐
        // =============================================

        public static string SetParagraphAlignment(long documentNodeId, int paragraphIndex, string alignment)
        {
            try
            {
                var para = GetParagraph(documentNodeId, paragraphIndex);
                string stream = para.Stream ?? "";
                var ns = GetWordNamespace(stream);
                var xDoc = XElement.Parse(stream);

                var pPr = xDoc.Descendants(ns + "pPr").FirstOrDefault();
                if (pPr == null)
                {
                    pPr = new XElement(ns + "pPr");
                    xDoc.AddFirst(pPr);
                }

                var jc = pPr.Element(ns + "jc");
                if (jc == null)
                {
                    jc = new XElement(ns + "jc");
                    pPr.Add(jc);
                }

                string val = alignment.ToLowerInvariant();
                switch (val)
                {
                    case "left": jc.SetAttributeValue(ns + "val", "left"); break;
                    case "center": jc.SetAttributeValue(ns + "val", "center"); break;
                    case "right": jc.SetAttributeValue(ns + "val", "right"); break;
                    case "justify": case "both": jc.SetAttributeValue(ns + "val", "both"); break;
                    case "distribute": jc.SetAttributeValue(ns + "val", "distribute"); break;
                    default: return ErrorJson($"不支持的对齐方式: {alignment}（可用: left/center/right/justify/distribute）");
                }

                para.UpdateStream(xDoc.ToString(), para.Section);
                SaveDocument(documentNodeId);

                return Ok($"段落 {paragraphIndex} 对齐已设置为 {val}", documentNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置段落对齐失败: " + ex.Message); }
        }

        // =============================================
        // 设置段落间距
        // =============================================

        public static string SetParagraphSpacing(long documentNodeId, int paragraphIndex, double? lineSpacing, string lineSpacingRule, double? spaceBefore, double? spaceAfter)
        {
            try
            {
                var para = GetParagraph(documentNodeId, paragraphIndex);
                string stream = para.Stream ?? "";
                var ns = GetWordNamespace(stream);
                var xDoc = XElement.Parse(stream);

                var pPr = xDoc.Descendants(ns + "pPr").FirstOrDefault();
                if (pPr == null)
                {
                    pPr = new XElement(ns + "pPr");
                    xDoc.AddFirst(pPr);
                }

                var spacing = pPr.Element(ns + "spacing");
                if (spacing == null)
                {
                    spacing = new XElement(ns + "spacing");
                    pPr.Add(spacing);
                }

                if (lineSpacing.HasValue)
                {
                    // lineSpacing 单位：行（1.0/1.5/2.0）
                    if (lineSpacingRule == "exact" || lineSpacingRule == "atLeast")
                    {
                        // twentieths of a point
                        spacing.SetAttributeValue(ns + "line", ((int)(lineSpacing.Value * 20)).ToString());
                        spacing.SetAttributeValue(ns + "lineRule", lineSpacingRule);
                    }
                    else
                    {
                        // auto (default): 240 = single, 360 = 1.5, 480 = double
                        spacing.SetAttributeValue(ns + "line", ((int)(lineSpacing.Value * 240)).ToString());
                        spacing.SetAttributeValue(ns + "lineRule", "auto");
                    }
                }
                if (spaceBefore.HasValue)
                {
                    spacing.SetAttributeValue(ns + "before", ((int)(spaceBefore.Value * 20)).ToString());
                }
                if (spaceAfter.HasValue)
                {
                    spacing.SetAttributeValue(ns + "after", ((int)(spaceAfter.Value * 20)).ToString());
                }

                para.UpdateStream(xDoc.ToString(), para.Section);
                SaveDocument(documentNodeId);

                return Ok($"段落 {paragraphIndex} 间距已设置", documentNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置段落间距失败: " + ex.Message); }
        }

        // =============================================
        // 设置段落缩进
        // =============================================

        public static string SetParagraphIndent(long documentNodeId, int paragraphIndex, double? indentLeft, double? indentRight, double? indentFirstLine)
        {
            try
            {
                var para = GetParagraph(documentNodeId, paragraphIndex);
                string stream = para.Stream ?? "";
                var ns = GetWordNamespace(stream);
                var xDoc = XElement.Parse(stream);

                var pPr = xDoc.Descendants(ns + "pPr").FirstOrDefault();
                if (pPr == null)
                {
                    pPr = new XElement(ns + "pPr");
                    xDoc.AddFirst(pPr);
                }

                var ind = pPr.Element(ns + "ind");
                if (ind == null)
                {
                    ind = new XElement(ns + "ind");
                    pPr.Add(ind);
                }

                // OOXML 缩进单位：twips（1 cm = 567 twips, 1 inch = 1440 twips）
                // 这里使用字符宽度作为单位（1 字符 ≈ 200 twips），与 Word 行为一致
                if (indentLeft.HasValue)
                {
                    ind.SetAttributeValue(ns + "left", ((int)(indentLeft.Value * 200)).ToString());
                }
                if (indentRight.HasValue)
                {
                    ind.SetAttributeValue(ns + "right", ((int)(indentRight.Value * 200)).ToString());
                }
                if (indentFirstLine.HasValue)
                {
                    if (indentFirstLine.Value >= 0)
                    {
                        ind.SetAttributeValue(ns + "firstLine", ((int)(indentFirstLine.Value * 200)).ToString());
                        ind.Attribute(ns + "hanging")?.Remove();
                    }
                    else
                    {
                        ind.SetAttributeValue(ns + "hanging", ((int)(-indentFirstLine.Value * 200)).ToString());
                        ind.Attribute(ns + "firstLine")?.Remove();
                    }
                }

                para.UpdateStream(xDoc.ToString(), para.Section);
                SaveDocument(documentNodeId);

                return Ok($"段落 {paragraphIndex} 缩进已设置", documentNodeId);
            }
            catch (Exception ex) { return ErrorJson("设置段落缩进失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

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

        private static XNamespace GetWordNamespace(string stream)
        {
            try
            {
                var root = XElement.Parse(stream);
                // 查找 w 命名空间
                var nsAttr = root.Attributes().FirstOrDefault(a => a.Name.LocalName == "w");
                if (nsAttr != null) return nsAttr.Value;
                // 默认命名空间
                return "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            }
            catch
            {
                return "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
            }
        }

        private static ParagraphFormat ParseParagraphFormat(string stream)
        {
            var fmt = new ParagraphFormat();
            if (string.IsNullOrEmpty(stream)) return fmt;
            try
            {
                var ns = GetWordNamespace(stream);
                var xDoc = XElement.Parse(stream);
                var pPr = xDoc.Descendants(ns + "pPr").FirstOrDefault();
                if (pPr == null) return fmt;

                var jc = pPr.Element(ns + "jc");
                if (jc != null)
                {
                    var v = jc.Attribute(ns + "val")?.Value;
                    if (!string.IsNullOrEmpty(v)) fmt.Alignment = v;
                }

                var spacing = pPr.Element(ns + "spacing");
                if (spacing != null)
                {
                    var line = spacing.Attribute(ns + "line")?.Value;
                    var rule = spacing.Attribute(ns + "lineRule")?.Value;
                    if (!string.IsNullOrEmpty(line))
                    {
                        if (rule == "auto") fmt.LineSpacing = double.Parse(line) / 240.0;
                        else fmt.LineSpacing = double.Parse(line) / 20.0;
                        fmt.LineSpacingRule = rule ?? "auto";
                    }
                    var before = spacing.Attribute(ns + "before")?.Value;
                    if (!string.IsNullOrEmpty(before)) fmt.SpaceBefore = double.Parse(before) / 20.0;
                    var after = spacing.Attribute(ns + "after")?.Value;
                    if (!string.IsNullOrEmpty(after)) fmt.SpaceAfter = double.Parse(after) / 20.0;
                }

                var ind = pPr.Element(ns + "ind");
                if (ind != null)
                {
                    var left = ind.Attribute(ns + "left")?.Value;
                    if (!string.IsNullOrEmpty(left)) fmt.IndentLeft = double.Parse(left) / 200.0;
                    var right = ind.Attribute(ns + "right")?.Value;
                    if (!string.IsNullOrEmpty(right)) fmt.IndentRight = double.Parse(right) / 200.0;
                    var fl = ind.Attribute(ns + "firstLine")?.Value;
                    if (!string.IsNullOrEmpty(fl)) fmt.IndentFirstLine = double.Parse(fl) / 200.0;
                    var hg = ind.Attribute(ns + "hanging")?.Value;
                    if (!string.IsNullOrEmpty(hg)) fmt.IndentFirstLine = -double.Parse(hg) / 200.0;
                }
            }
            catch { /* 解析失败返回默认值 */ }
            return fmt;
        }

        private class ParagraphFormat
        {
            public string Alignment = "left";
            public double LineSpacing = 1.0;
            public string LineSpacingRule = "auto";
            public double SpaceBefore = 0;
            public double SpaceAfter = 0;
            public double IndentLeft = 0;
            public double IndentRight = 0;
            public double IndentFirstLine = 0;
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
            // 文档段落存储在 Document 中，必须调用 Document.Save() 才能持久化
            // project.Save() 只保存项目元数据和树节点，不保存文档段落
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
    }
}
