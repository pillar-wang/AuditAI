using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Auditai.DTO;
using Auditai.Model;
using Document = Auditai.Model.Document;
using Paragraph = Auditai.Model.Paragraph;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 文档扩展服务
    /// 在 DocumentService 基础上补充段落级编辑能力：
    /// 设置段落批注、按位置插入段落、删除段落、获取段落信息、获取文档概要
    /// </summary>
    public static class DocumentExtensionService
    {
        private static readonly XNamespace W =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        // =============================================
        // 段落批注
        // =============================================

        /// <summary>
        /// 为指定段落设置批注
        /// </summary>
        public static string SetParagraphComment(long documentNodeId, int paragraphIndex, string comment)
        {
            try
            {
                var node = FindDocumentNode(documentNodeId);
                var document = node.Document;
                document.LoadAndReturn();

                var paragraph = FindParagraph(document, paragraphIndex);
                paragraph.UpdateComment(comment ?? "");

                node.IsEntityDirty = true;
                document.Project.NeedSave = true;

                return Ok("段落批注已更新", documentNodeId, new JObject
                {
                    ["paragraph_index"] = paragraphIndex,
                    ["paragraph_id"] = paragraph.Id.Value.ToString(),
                    ["comment"] = comment
                });
            }
            catch (Exception ex) { return ErrorJson("设置段落批注失败: " + ex.Message); }
        }

        // =============================================
        // 按位置插入段落
        // =============================================

        /// <summary>
        /// 在指定位置插入新段落，原位置及之后的段落索引自动后移
        /// </summary>
        public static string InsertParagraphAt(long documentNodeId, int position, string text)
        {
            try
            {
                var node = FindDocumentNode(documentNodeId);
                var document = node.Document;
                document.LoadAndReturn();

                if (position < 0 || position > document.Paragraphs.Count)
                    return ErrorJson($"插入位置无效: {position}（当前段落数: {document.Paragraphs.Count}）");

                var project = document.Project;
                var paragraph = new Paragraph
                {
                    Id = project.GetNextId(),
                    Index = position,
                    Status = SyncStatus.New,
                    Stream = CreateParagraphStream(text ?? ""),
                    Document = document
                };

                // 将插入位置及之后的段落索引 +1
                foreach (var p in document.Paragraphs)
                {
                    if (p.Index >= position) p.Index += 1;
                }

                document.Paragraphs.Add(paragraph);

                node.IsEntityDirty = true;
                project.NeedSave = true;

                // 立即保存 Document DTO 以持久化段落
                document.Save();

                return Ok("段落已插入", documentNodeId, new JObject
                {
                    ["position"] = position,
                    ["paragraph_id"] = paragraph.Id.Value.ToString(),
                    ["new_paragraph_count"] = document.Paragraphs.Count
                });
            }
            catch (Exception ex) { return ErrorJson("插入段落失败: " + ex.Message); }
        }

        // =============================================
        // 删除段落
        // =============================================

        /// <summary>
        /// 删除指定索引的段落（移入 RemovedParagraphs，保存时软删除）
        /// </summary>
        public static string DeleteParagraph(long documentNodeId, int paragraphIndex)
        {
            try
            {
                var node = FindDocumentNode(documentNodeId);
                var document = node.Document;
                document.LoadAndReturn();

                var paragraph = FindParagraph(document, paragraphIndex);
                document.RemovedParagraphs.Add(paragraph.Id);
                document.Paragraphs.Remove(paragraph);

                // 重新编号段落索引保持连续
                int idx = 0;
                foreach (var p in document.Paragraphs.OrderBy(x => x.Index))
                {
                    if (p.Index != idx)
                    {
                        p.Index = idx;
                        if (p.Status == SyncStatus.Synced) p.Dirty.IsIndexDirty = true;
                    }
                    idx++;
                }

                node.IsEntityDirty = true;
                document.Project.NeedSave = true;

                // 立即保存 Document DTO 以持久化删除操作
                document.Save();

                return Ok("段落已删除", documentNodeId, new JObject
                {
                    ["deleted_paragraph_index"] = paragraphIndex,
                    ["deleted_paragraph_id"] = paragraph.Id.Value.ToString(),
                    ["remaining_paragraph_count"] = document.Paragraphs.Count
                });
            }
            catch (Exception ex) { return ErrorJson("删除段落失败: " + ex.Message); }
        }

        // =============================================
        // 段落信息
        // =============================================

        public static string GetParagraphInfo(long documentNodeId, int paragraphIndex)
        {
            try
            {
                var node = FindDocumentNode(documentNodeId);
                var document = node.Document;
                document.LoadAndReturn();

                var paragraph = FindParagraph(document, paragraphIndex);
                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["document_name"] = node.Name,
                    ["paragraph_index"] = paragraphIndex,
                    ["paragraph_id"] = paragraph.Id.Value.ToString(),
                    ["text"] = ExtractText(paragraph.Stream),
                    ["comment"] = paragraph.Comment ?? "",
                    ["section"] = paragraph.Section ?? "",
                    ["status"] = paragraph.Status.ToString(),
                    ["is_index_dirty"] = paragraph.IsIndexDirty,
                    ["text_length"] = ExtractText(paragraph.Stream).Length
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取段落信息失败: " + ex.Message); }
        }

        // =============================================
        // 文档概要
        // =============================================

        public static string GetDocumentInfo(long documentNodeId)
        {
            try
            {
                var node = FindDocumentNode(documentNodeId);
                var document = node.Document;
                document.LoadAndReturn();

                var paragraphs = document.Paragraphs.OrderBy(p => p.Index).ToList();
                int totalLength = paragraphs.Sum(p => ExtractText(p.Stream ?? "").Length);
                int commentCount = paragraphs.Count(p => !string.IsNullOrEmpty(p.Comment));

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["document_id"] = document.Id.Value.ToString(),
                    ["document_name"] = node.Name,
                    ["paragraph_count"] = paragraphs.Count,
                    ["total_text_length"] = totalLength,
                    ["comment_count"] = commentCount,
                    ["removed_paragraph_count"] = document.RemovedParagraphs.Count,
                    ["can_reload"] = document.CanReload,
                    ["node_path"] = BuildNodePath(node)
                };

                // 段落摘要（前 20 个 + 索引）
                var summary = new JArray();
                foreach (var p in paragraphs.Take(20))
                {
                    string text = ExtractText(p.Stream ?? "");
                    summary.Add(new JObject
                    {
                        ["index"] = p.Index,
                        ["id"] = p.Id.Value.ToString(),
                        ["preview"] = text.Length > 60 ? text.Substring(0, 60) + "..." : text,
                        ["text_length"] = text.Length,
                        ["has_comment"] = !string.IsNullOrEmpty(p.Comment)
                    });
                }
                result["paragraphs_preview"] = summary;
                if (paragraphs.Count > 20)
                {
                    result["truncated"] = true;
                    result["truncated_count"] = paragraphs.Count - 20;
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex) { return ErrorJson("获取文档概要失败: " + ex.Message); }
        }

        // =============================================
        // 辅助方法
        // =============================================

        private static TreeDocumentNode FindDocumentNode(long documentNodeId)
        {
            SessionState.Current.EnsureProject();
            var project = SessionState.Current.CurrentProject;
            var id = new Id64(documentNodeId);
            var node = project.GetAllDocumentNodes().FirstOrDefault(n => n.Id == id);
            if (node == null)
                throw new InvalidOperationException($"未找到文档节点: {documentNodeId}");
            return node;
        }

        private static Paragraph FindParagraph(Document document, int paragraphIndex)
        {
            var paragraph = document.Paragraphs.FirstOrDefault(p => p.Index == paragraphIndex);
            if (paragraph == null)
                throw new InvalidOperationException($"未找到段落索引: {paragraphIndex}（当前段落数: {document.Paragraphs.Count}）");
            return paragraph;
        }

        private static string ExtractText(string stream)
        {
            if (string.IsNullOrEmpty(stream)) return "";
            try
            {
                var root = XElement.Parse(stream);
                var ns = root.GetNamespaceOfPrefix("w");
                if (ns == null || ns.NamespaceName.Length == 0) ns = W;
                return string.Concat(root.Descendants(ns + "t").Select(t => t.Value));
            }
            catch { return ""; }
        }

        private static string CreateParagraphStream(string text)
        {
            var p = new XElement(W + "p",
                new XAttribute(XNamespace.Xmlns + "w", W.NamespaceName),
                new XElement(W + "r",
                    new XElement(W + "t", text ?? "")));
            return p.ToString(SaveOptions.DisableFormatting);
        }

        private static string BuildNodePath(TreeNodeBase node)
        {
            var segs = new System.Collections.Generic.List<string>();
            segs.Add(node.Group?.Name ?? "");
            var cur = node;
            while (cur != null)
            {
                segs.Add(cur.Name);
                cur = cur.Parent;
            }
            segs.Reverse();
            return string.Join(" / ", segs);
        }

        private static string Ok(string msg, long documentNodeId, JObject extra = null)
        {
            var r = new JObject
            {
                ["success"] = true,
                ["document_node_id"] = documentNodeId.ToString(),
                ["message"] = msg
            };
            if (extra != null) r["data"] = extra;
            return JsonConvert.SerializeObject(r, Formatting.Indented);
        }

        private static string ErrorJson(string message)
        {
            return JsonConvert.SerializeObject(new JObject { ["success"] = false, ["error"] = message }, Formatting.Indented);
        }
    }
}
