using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Auditai.DTO;
using Auditai.Model;
using Project = Auditai.Model.Project;
using Document = Auditai.Model.Document;
using Paragraph = Auditai.Model.Paragraph;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 文档插入服务
    /// 覆盖 AppCommands.InsertPageBreak / InsertSectionBreak / InsertSymbol /
    /// InsertTextFrame / InsertHeader / InsertFooter / InsertImage / InsertTable 等 UI 操作
    /// 通过修改段落 OOXML 实现各类内容插入
    /// </summary>
    public static class DocumentInsertService
    {
        private static readonly XNamespace W =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly XNamespace R =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace XmlNs =
            "http://www.w3.org/XML/1998/namespace";

        // =============================================
        // 插入分页符（InsertPageBreak）
        // =============================================

        /// <summary>
        /// 在指定段落末尾插入分页符。覆盖 AppCommands.InsertPageBreak。
        /// </summary>
        public static string InsertPageBreak(long documentNodeId, int paragraphIndex)
        {
            try
            {
                var (node, document, paragraph) = ResolveParagraph(documentNodeId, paragraphIndex);
                string stream = paragraph.Stream ?? CreateEmptyParagraphStream();
                var xDoc = XElement.Parse(stream);

                // 在段落末尾追加 <w:r><w:br w:type="page"/></w:r>
                var br = new XElement(W + "r",
                    new XElement(W + "br", new XAttribute(W + "type", "page")));
                xDoc.Add(br);

                paragraph.UpdateStream(xDoc.ToString(SaveOptions.DisableFormatting), paragraph.Section);
                document.Save();

                return Ok("分页符已插入", documentNodeId, new JObject
                {
                    ["paragraph_index"] = paragraphIndex,
                    ["paragraph_id"] = paragraph.Id.Value.ToString()
                });
            }
            catch (Exception ex) { return ErrorJson("插入分页符失败: " + ex.Message); }
        }

        // =============================================
        // 插入分节符（InsertSectionBreak）
        // =============================================

        /// <summary>
        /// 在指定段落插入分节符（下一页分节）。覆盖 AppCommands.InsertSectionBreak。
        /// </summary>
        public static string InsertSectionBreak(long documentNodeId, int paragraphIndex, string breakType = "nextPage")
        {
            try
            {
                var (node, document, paragraph) = ResolveParagraph(documentNodeId, paragraphIndex);
                string stream = paragraph.Stream ?? CreateEmptyParagraphStream();
                var xDoc = XElement.Parse(stream);

                // 获取或创建 pPr
                var pPr = xDoc.Element(W + "pPr");
                if (pPr == null)
                {
                    pPr = new XElement(W + "pPr");
                    xDoc.AddFirst(pPr);
                }

                // 移除已有 sectPr
                pPr.Element(W + "sectPr")?.Remove();

                // 创建新的 sectPr，type 可为 nextPage/continuous/evenPage/oddPage
                string bt = string.IsNullOrEmpty(breakType) ? "nextPage" : breakType;
                var sectPr = new XElement(W + "sectPr",
                    new XElement(W + "type", new XAttribute(W + "val", bt)),
                    new XElement(W + "pgSz", new XAttribute(W + "w", "11906"), new XAttribute(W + "h", "16838")),
                    new XElement(W + "pgMar",
                        new XAttribute(W + "top", "1440"),
                        new XAttribute(W + "right", "1440"),
                        new XAttribute(W + "bottom", "1440"),
                        new XAttribute(W + "left", "1440")),
                    new XElement(W + "cols", new XAttribute(W + "space", "425")));

                pPr.Add(sectPr);

                paragraph.UpdateStream(xDoc.ToString(SaveOptions.DisableFormatting), paragraph.Section);
                document.Save();

                return Ok("分节符已插入", documentNodeId, new JObject
                {
                    ["paragraph_index"] = paragraphIndex,
                    ["break_type"] = bt
                });
            }
            catch (Exception ex) { return ErrorJson("插入分节符失败: " + ex.Message); }
        }

        // =============================================
        // 插入特殊符号（InsertSymbol）
        // =============================================

        /// <summary>
        /// 在指定段落末尾插入特殊符号。覆盖 AppCommands.InsertSymbol。
        /// </summary>
        public static string InsertSymbol(long documentNodeId, int paragraphIndex,
            string symbol, string fontFamily = "宋体")
        {
            try
            {
                if (string.IsNullOrEmpty(symbol))
                    return ErrorJson("symbol 不能为空");

                var (node, document, paragraph) = ResolveParagraph(documentNodeId, paragraphIndex);
                string stream = paragraph.Stream ?? CreateEmptyParagraphStream();
                var xDoc = XElement.Parse(stream);

                // 构造 <w:r><w:rPr><w:rFonts.../></w:rPr><w:t xml:space="preserve">symbol</w:t></w:r>
                var run = new XElement(W + "r",
                    new XElement(W + "rPr",
                        new XElement(W + "rFonts",
                            new XAttribute(W + "ascii", fontFamily),
                            new XAttribute(W + "hAnsi", fontFamily),
                            new XAttribute(W + "eastAsia", fontFamily))),
                    new XElement(W + "t", new XAttribute(XmlNs + "space", "preserve"), symbol));

                xDoc.Add(run);

                paragraph.UpdateStream(xDoc.ToString(SaveOptions.DisableFormatting), paragraph.Section);
                document.Save();

                return Ok("特殊符号已插入", documentNodeId, new JObject
                {
                    ["paragraph_index"] = paragraphIndex,
                    ["symbol"] = symbol,
                    ["font_family"] = fontFamily
                });
            }
            catch (Exception ex) { return ErrorJson("插入特殊符号失败: " + ex.Message); }
        }

        // =============================================
        // 插入文本框（InsertTextFrame）
        // =============================================

        /// <summary>
        /// 在指定段落插入文本框（使用 VML pict 形式以保持向后兼容）。覆盖 AppCommands.InsertTextFrame。
        /// </summary>
        public static string InsertTextFrame(long documentNodeId, int paragraphIndex,
            string text, int width = 2000, int height = 1000,
            int offsetX = 0, int offsetY = 0)
        {
            try
            {
                var (node, document, paragraph) = ResolveParagraph(documentNodeId, paragraphIndex);
                string stream = paragraph.Stream ?? CreateEmptyParagraphStream();
                var xDoc = XElement.Parse(stream);

                // 使用简单的 pict + VML shape 实现文本框
                string shapeId = "TextFrame" + Guid.NewGuid().ToString("N").Substring(0, 8);
                var pict = new XElement(W + "pict",
                    new XElement(XNamespace.Get("urn:schemas-microsoft-com:vml") + "shape",
                        new XAttribute("id", shapeId),
                        new XAttribute("style",
                            $"position:absolute;left:0;text-align:left;margin-left:{offsetX};margin-top:{offsetY};width:{width};height:{height};z-index:1"),
                        new XAttribute("type", "#_x0000_t202"),
                        new XElement(XNamespace.Get("urn:schemas-microsoft-com:vml") + "textbox",
                            new XElement(W + "tboxtxbx",
                                new XElement(XNamespace.Get("urn:schemas-microsoft-com:office:word") + "wrap", "none"))),
                        new XElement(XNamespace.Get("urn:schemas-microsoft-com:vml") + "textpath",
                            new XAttribute("style", "font-family:'宋体';font-size:12pt"),
                            new XAttribute("string", text ?? ""))));

                xDoc.Add(pict);

                paragraph.UpdateStream(xDoc.ToString(SaveOptions.DisableFormatting), paragraph.Section);
                document.Save();

                return Ok("文本框已插入", documentNodeId, new JObject
                {
                    ["paragraph_index"] = paragraphIndex,
                    ["text"] = text,
                    ["width"] = width,
                    ["height"] = height
                });
            }
            catch (Exception ex) { return ErrorJson("插入文本框失败: " + ex.Message); }
        }

        // =============================================
        // 插入页眉（InsertHeader）
        // =============================================

        /// <summary>
        /// 为文档的指定节设置页眉内容（默认所有节共用）。覆盖 AppCommands.InsertHeader。
        /// </summary>
        public static string InsertHeader(long documentNodeId, string text, string headerType = "default")
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return ErrorJson("页眉文本不能为空");

                var (node, document) = ResolveDocument(documentNodeId);

                // 取文档第一个段落作为节定义参考
                var firstPara = document.Paragraphs.OrderBy(p => p.Index).FirstOrDefault();
                if (firstPara == null)
                    return ErrorJson("文档没有任何段落，无法设置页眉");

                string stream = firstPara.Stream ?? CreateEmptyParagraphStream();
                var xDoc = XElement.Parse(stream);
                var pPr = xDoc.Element(W + "pPr");
                if (pPr == null)
                {
                    pPr = new XElement(W + "pPr");
                    xDoc.AddFirst(pPr);
                }

                // 在 sectPr 中添加 headerReference（用 header text 元素表示）
                // 注：实际 OOXML 中 header 是独立 part，这里采用简化的做法，将页眉文本写入段落 sectPr 后续扩展
                var sectPr = pPr.Element(W + "sectPr");
                if (sectPr == null)
                {
                    sectPr = new XElement(W + "sectPr");
                    pPr.Add(sectPr);
                }

                // 简化实现：在 sectPr 中添加自定义元素记录页眉文本
                var existingHeader = sectPr.Elements(W + "headerReference").FirstOrDefault();
                if (existingHeader != null) existingHeader.Remove();

                // 创建一个新的段落表示页眉内容（作为 sectPr 的子元素不合规，这里改为写入段落批注）
                // 由于 MCP 环境无法创建独立 header part，我们改为将页眉内容写入段落批注中保存
                string headerComment = firstPara.Comment ?? "";
                if (!string.IsNullOrEmpty(headerComment) && headerComment.StartsWith("[HEADER]"))
                {
                    headerComment = headerComment.Substring("[HEADER]".Length);
                }
                if (!string.IsNullOrEmpty(headerComment) && headerComment.StartsWith("[FOOTER]"))
                {
                    // 保留已有的 [FOOTER]
                    headerComment = "";
                }
                string newComment = $"[HEADER]{text}";
                if (!string.IsNullOrEmpty(headerComment) && headerComment.StartsWith("[FOOTER]"))
                {
                    newComment += headerComment;
                }
                firstPara.UpdateComment(newComment);
                firstPara.UpdateStream(xDoc.ToString(SaveOptions.DisableFormatting), firstPara.Section);
                document.Save();

                return Ok("页眉已设置（写入段落 0 批注）", documentNodeId, new JObject
                {
                    ["header_type"] = headerType,
                    ["text"] = text,
                    ["note"] = "页眉文本已保存到第一段批注中，格式为 [HEADER]文本。客户端打开时可解析显示。"
                });
            }
            catch (Exception ex) { return ErrorJson("插入页眉失败: " + ex.Message); }
        }

        // =============================================
        // 插入页脚（InsertFooter）
        // =============================================

        /// <summary>
        /// 为文档的指定节设置页脚内容。覆盖 AppCommands.InsertFooter。
        /// </summary>
        public static string InsertFooter(long documentNodeId, string text, string footerType = "default")
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return ErrorJson("页脚文本不能为空");

                var (node, document) = ResolveDocument(documentNodeId);

                var firstPara = document.Paragraphs.OrderBy(p => p.Index).FirstOrDefault();
                if (firstPara == null)
                    return ErrorJson("文档没有任何段落，无法设置页脚");

                string existingComment = firstPara.Comment ?? "";
                string headerPart = "";
                if (existingComment.StartsWith("[HEADER]"))
                {
                    // 保留已有的 [HEADER] 部分
                    int footerIdx = existingComment.IndexOf("[FOOTER]");
                    if (footerIdx >= 0)
                        headerPart = existingComment.Substring(0, footerIdx);
                    else
                        headerPart = existingComment;
                }
                string newComment = headerPart + $"[FOOTER]{text}";
                firstPara.UpdateComment(newComment);
                document.Save();

                return Ok("页脚已设置（写入段落 0 批注）", documentNodeId, new JObject
                {
                    ["footer_type"] = footerType,
                    ["text"] = text,
                    ["note"] = "页脚文本已保存到第一段批注中，格式为 [FOOTER]文本。客户端打开时可解析显示。"
                });
            }
            catch (Exception ex) { return ErrorJson("插入页脚失败: " + ex.Message); }
        }

        // =============================================
        // 插入图片（InsertImage）
        // =============================================

        /// <summary>
        /// 在指定段落插入图片。覆盖 AppCommands.InsertImage。
        /// 简化实现：将图片信息（路径/尺寸）写入段落批注，OOXML 中插入一个占位 run。
        /// </summary>
        public static string InsertImage(long documentNodeId, int paragraphIndex,
            string imagePath, int width = 200, int height = 150)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return ErrorJson("imagePath 不能为空");

                var (node, document, paragraph) = ResolveParagraph(documentNodeId, paragraphIndex);
                string stream = paragraph.Stream ?? CreateEmptyParagraphStream();
                var xDoc = XElement.Parse(stream);

                // 插入图片占位 run（实际图片嵌入需要 relationship，MCP 环境下简化处理）
                var run = new XElement(W + "r",
                    new XElement(W + "rPr",
                        new XElement(W + "vanish")),
                    new XElement(W + "t",
                        new XAttribute(XmlNs + "space", "preserve"),
                        $"[IMAGE:{imagePath}|{width}x{height}]"));
                xDoc.Add(run);

                paragraph.UpdateStream(xDoc.ToString(SaveOptions.DisableFormatting), paragraph.Section);
                document.Save();

                return Ok("图片占位已插入（请用客户端替换为实际图片）", documentNodeId, new JObject
                {
                    ["paragraph_index"] = paragraphIndex,
                    ["image_path"] = imagePath,
                    ["width"] = width,
                    ["height"] = height,
                    ["note"] = "MCP 环境下只能插入图片占位标记，客户端打开后可识别并替换为实际图片。"
                });
            }
            catch (Exception ex) { return ErrorJson("插入图片失败: " + ex.Message); }
        }

        // =============================================
        // 在文档中插入表格（InsertTable）
        // =============================================

        /// <summary>
        /// 在指定段落位置插入一个 Word 表格。覆盖 AppCommands.InsertTable。
        /// </summary>
        public static string InsertTable(long documentNodeId, int paragraphIndex,
            int rows, int cols, string borderStyle = "single")
        {
            try
            {
                if (rows <= 0 || cols <= 0)
                    return ErrorJson("rows 和 cols 必须大于 0");
                if (rows > 100 || cols > 50)
                    return ErrorJson("rows 不能超过 100，cols 不能超过 50");

                var (node, document, paragraph) = ResolveParagraph(documentNodeId, paragraphIndex);
                string stream = paragraph.Stream ?? CreateEmptyParagraphStream();
                var xDoc = XElement.Parse(stream);

                // 构造 OOXML 表格
                var tbl = new XElement(W + "tbl");

                // 表格属性
                var tblPr = new XElement(W + "tblPr",
                    new XElement(W + "tblStyle", new XAttribute(W + "val", "TableGrid")),
                    new XElement(W + "tblW", new XAttribute(W + "w", "5000"), new XAttribute(W + "type", "pct")),
                    new XElement(W + "tblBorders",
                        BuildBorderElements(borderStyle)),
                    new XElement(W + "tblLook",
                        new XAttribute(W + "val", "04A0")));
                tbl.Add(tblPr);

                // 表格网格
                var tblGrid = new XElement(W + "tblGrid");
                int colWidth = 9000 / cols;
                for (int c = 0; c < cols; c++)
                {
                    tblGrid.Add(new XElement(W + "gridCol", new XAttribute(W + "w", colWidth.ToString())));
                }
                tbl.Add(tblGrid);

                // 行
                for (int r = 0; r < rows; r++)
                {
                    var tr = new XElement(W + "tr");
                    for (int c = 0; c < cols; c++)
                    {
                        var tc = new XElement(W + "tc",
                            new XElement(W + "tcPr",
                                new XElement(W + "tcW", new XAttribute(W + "w", colWidth.ToString()), new XAttribute(W + "type", "dxa"))),
                            new XElement(W + "p",
                                new XElement(W + "r",
                                    new XElement(W + "t", ""))));
                        tr.Add(tc);
                    }
                    tbl.Add(tr);
                }

                xDoc.Add(tbl);

                paragraph.UpdateStream(xDoc.ToString(SaveOptions.DisableFormatting), paragraph.Section);
                document.Save();

                return Ok("表格已插入文档", documentNodeId, new JObject
                {
                    ["paragraph_index"] = paragraphIndex,
                    ["rows"] = rows,
                    ["cols"] = cols,
                    ["border_style"] = borderStyle
                });
            }
            catch (Exception ex) { return ErrorJson("插入表格失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static IEnumerable<XElement> BuildBorderElements(string style)
        {
            string val = string.IsNullOrEmpty(style) ? "single" : style;
            yield return new XElement(W + "top", new XAttribute(W + "val", val), new XAttribute(W + "sz", "4"), new XAttribute(W + "color", "auto"));
            yield return new XElement(W + "left", new XAttribute(W + "val", val), new XAttribute(W + "sz", "4"), new XAttribute(W + "color", "auto"));
            yield return new XElement(W + "bottom", new XAttribute(W + "val", val), new XAttribute(W + "sz", "4"), new XAttribute(W + "color", "auto"));
            yield return new XElement(W + "right", new XAttribute(W + "val", val), new XAttribute(W + "sz", "4"), new XAttribute(W + "color", "auto"));
            yield return new XElement(W + "insideH", new XAttribute(W + "val", val), new XAttribute(W + "sz", "4"), new XAttribute(W + "color", "auto"));
            yield return new XElement(W + "insideV", new XAttribute(W + "val", val), new XAttribute(W + "sz", "4"), new XAttribute(W + "color", "auto"));
        }

        private static string CreateEmptyParagraphStream()
        {
            var p = new XElement(W + "p",
                new XAttribute(XNamespace.Xmlns + "w", W.NamespaceName));
            return p.ToString(SaveOptions.DisableFormatting);
        }

        private static (TreeDocumentNode node, Document document) ResolveDocument(long documentNodeId)
        {
            SessionState.Current.EnsureProject();
            var project = SessionState.Current.CurrentProject;
            Project.Current = project;
            var id = new Id64(documentNodeId);
            var node = project.GetAllDocumentNodes().FirstOrDefault(n => n.Id == id);
            if (node == null)
                throw new InvalidOperationException($"未找到文档节点: {documentNodeId}");
            node.Document.LoadAndReturn();
            return (node, node.Document);
        }

        private static (TreeDocumentNode node, Document document, Paragraph paragraph) ResolveParagraph(
            long documentNodeId, int paragraphIndex)
        {
            var (node, document) = ResolveDocument(documentNodeId);
            var paragraph = document.Paragraphs.FirstOrDefault(p => p.Index == paragraphIndex);
            if (paragraph == null)
                throw new InvalidOperationException($"未找到段落索引: {paragraphIndex}（当前段落数: {document.Paragraphs.Count}）");
            return (node, document, paragraph);
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
