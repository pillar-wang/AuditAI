﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Linq;
using System.Xml.Linq;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 文档书签服务
    /// 封装文档书签的读取和编辑操作。
    /// 文档书签存储在段落 OOXML Stream 中的 w:bookmarkStart / w:bookmarkEnd 元素中。
    /// </summary>
    public static class DocumentBookmarkService
    {
        private static readonly XNamespace W =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>
        /// 获取文档中所有书签的信息
        /// </summary>
        /// <param name="documentNodeId">文档树节点 ID</param>
        /// <returns>书签列表的 JSON</returns>
        public static string GetDocumentBookmarks(long documentNodeId)
        {
            try
            {
                SessionState.Current.EnsureProject();

                var node = FindDocumentNode(documentNodeId);
                if (node == null)
                    return ErrorJson($"未找到文档节点: {documentNodeId}");

                var document = node.Document;
                document.LoadAndReturn();

                var bookmarks = new JArray();
                foreach (var paragraph in document.Paragraphs)
                {
                    if (string.IsNullOrEmpty(paragraph.Stream))
                        continue;

                    try
                    {
                        var root = XElement.Parse(paragraph.Stream);
                        foreach (var bmStart in root.Descendants(W + "bookmarkStart"))
                        {
                            var name = (string)bmStart.Attribute(W + "name");
                            var id = (string)bmStart.Attribute(W + "id");
                            if (!string.IsNullOrEmpty(name))
                            {
                                bookmarks.Add(new JObject
                                {
                                    ["name"] = name,
                                    ["paragraph_index"] = paragraph.Index,
                                    ["bookmark_id"] = id ?? ""
                                });
                            }
                        }
                    }
                    catch
                    {
                        // 跳过 XML 解析失败的段落
                    }
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["bookmark_count"] = bookmarks.Count,
                    ["bookmarks"] = bookmarks
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取文档书签失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置指定书签位置的文本内容
        /// </summary>
        /// <param name="documentNodeId">文档树节点 ID</param>
        /// <param name="bookmarkName">书签名称</param>
        /// <param name="text">要设置的文本</param>
        /// <returns>操作结果 JSON</returns>
        public static string SetBookmarkText(long documentNodeId, string bookmarkName, string text)
        {
            try
            {
                SessionState.Current.EnsureProject();

                var node = FindDocumentNode(documentNodeId);
                if (node == null)
                    return ErrorJson($"未找到文档节点: {documentNodeId}");

                var document = node.Document;
                document.LoadAndReturn();
                var project = document.Project;

                bool found = false;
                foreach (var paragraph in document.Paragraphs)
                {
                    if (string.IsNullOrEmpty(paragraph.Stream))
                        continue;

                    try
                    {
                        var root = XElement.Parse(paragraph.Stream);
                        var bmStart = root.Descendants(W + "bookmarkStart")
                            .FirstOrDefault(b => (string)b.Attribute(W + "name") == bookmarkName);

                        if (bmStart == null)
                            continue;

                        found = true;

                        // 查找匹配的 bookmarkEnd
                        var bmId = (string)bmStart.Attribute(W + "id");
                        var bmEnd = root.Descendants(W + "bookmarkEnd")
                            .FirstOrDefault(b => (string)b.Attribute(W + "id") == bmId);

                        // 移除 bookmarkStart 和 bookmarkEnd 之间的所有元素（原有文本运行）
                        var elementsToRemove = new System.Collections.Generic.List<XElement>();
                        bool between = false;
                        foreach (var element in root.Elements())
                        {
                            if (element == bmStart)
                            {
                                between = true;
                                continue;
                            }
                            if (bmEnd != null && element == bmEnd)
                                break;
                            if (between)
                                elementsToRemove.Add(element);
                        }

                        foreach (var el in elementsToRemove)
                            el.Remove();

                        // 在 bookmarkStart 后添加新的文本运行
                        var run = new XElement(W + "r",
                            new XElement(W + "t", text ?? ""));
                        bmStart.AddAfterSelf(run);

                        paragraph.UpdateStream(root.ToString(), paragraph.Section);
                        node.IsEntityDirty = true;
                        project.NeedSave = true;
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (!found)
                    return ErrorJson($"未找到书签: {bookmarkName}");

                var result = new JObject
                {
                    ["success"] = true,
                    ["bookmark_name"] = bookmarkName,
                    ["text"] = text,
                    ["message"] = "书签文本已更新，请调用 save_project 保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置书签文本失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 根据节点 ID 查找文档树节点
        /// </summary>
        private static TreeDocumentNode FindDocumentNode(long documentNodeId)
        {
            var project = SessionState.Current.CurrentProject;
            var id = new Auditai.DTO.Id64(documentNodeId);
            return project.GetAllDocumentNodes().FirstOrDefault(n => n.Id == id);
        }

        /// <summary>
        /// 生成错误 JSON 响应
        /// </summary>
        private static string ErrorJson(string message)
        {
            var result = new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }
    }
}