﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Auditai.Model;
using AuditAI.McpServer.State;
using AuditAI.McpServer.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 文档服务层
    /// 封装文档内容读取、编辑与导出操作。
    /// 文档数据存储在项目 SQLite 数据库中，通过 Project.Dal 访问；
    /// Document.MakePackage() 可生成 docx；PDF 导出通过 STA 线程上的 TX TextControl 完成。
    /// </summary>
    public static class DocumentService
    {
        private static readonly XNamespace W =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>
        /// 获取文档内容（段落列表）
        /// </summary>
        /// <param name="documentNodeId">文档树节点 ID</param>
        /// <returns>段落列表的 JSON</returns>
        public static string GetDocumentContent(long documentNodeId)
        {
            try
            {
                SessionState.Current.EnsureProject();

                var node = FindDocumentNode(documentNodeId);
                if (node == null)
                    return ErrorJson($"未找到文档节点: {documentNodeId}");

                var document = node.Document;
                document.LoadAndReturn();

                var paragraphs = document.Paragraphs.Select(p => new JObject
                {
                    ["index"] = p.Index,
                    ["id"] = p.Id.Value.ToString(),
                    ["text"] = ExtractText(p.Stream),
                    ["comment"] = p.Comment ?? ""
                }).ToList();

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["document_name"] = node.Name,
                    ["paragraph_count"] = paragraphs.Count,
                    ["paragraphs"] = new JArray(paragraphs)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取文档内容失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置文档内容（按段落拆分文本，替换文档内容）
        /// </summary>
        /// <param name="documentNodeId">文档树节点 ID</param>
        /// <param name="content">文档文本内容（按换行拆分为段落）</param>
        /// <returns>操作结果 JSON</returns>
        public static string SetDocumentContent(long documentNodeId, string content)
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

                // 将文本按换行拆分为段落
                var lines = string.IsNullOrEmpty(content)
                    ? new string[0]
                    : content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

                // 将旧段落标记为已移除（保存时软删除）
                foreach (var p in document.Paragraphs)
                {
                    document.RemovedParagraphs.Add(p.Id);
                }

                document.Paragraphs.Clear();

                int index = 0;
                foreach (var line in lines)
                {
                    var paragraph = new Paragraph
                    {
                        Id = project.GetNextId(),
                        Index = index,
                        Status = SyncStatus.New,
                        Stream = CreateParagraphStream(line),
                        Document = document
                    };
                    document.Paragraphs.Add(paragraph);
                    index++;
                }

                // 标记文档与项目为脏，需保存
                node.IsEntityDirty = true;
                project.NeedSave = true;

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["paragraph_count"] = index,
                    ["message"] = "文档内容已更新，请调用 save_project 保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置文档内容失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 向文档追加段落
        /// </summary>
        /// <param name="documentNodeId">文档树节点 ID</param>
        /// <param name="text">段落文本</param>
        /// <returns>操作结果 JSON</returns>
        public static string AddParagraph(long documentNodeId, string text)
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
                int nextIndex = document.Paragraphs.Count > 0
                    ? document.Paragraphs.Max(p => p.Index) + 1
                    : 0;

                var paragraph = new Paragraph
                {
                    Id = project.GetNextId(),
                    Index = nextIndex,
                    Status = SyncStatus.New,
                    Stream = CreateParagraphStream(text ?? ""),
                    Document = document
                };
                document.Paragraphs.Add(paragraph);

                node.IsEntityDirty = true;
                project.NeedSave = true;

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["paragraph_id"] = paragraph.Id.Value.ToString(),
                    ["index"] = nextIndex,
                    ["message"] = "段落已添加，请调用 save_project 保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("添加段落失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 导出文档
        /// </summary>
        /// <param name="documentNodeId">文档树节点 ID</param>
        /// <param name="format">导出格式：docx 或 pdf</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <returns>操作结果 JSON</returns>
        public static string ExportDocument(long documentNodeId, string format, string outputPath)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (string.IsNullOrWhiteSpace(outputPath))
                    return ErrorJson("输出路径不能为空");

                var node = FindDocumentNode(documentNodeId);
                if (node == null)
                    return ErrorJson($"未找到文档节点: {documentNodeId}");

                var document = node.Document;
                document.LoadAndReturn();

                string fmt = (format ?? "").ToLowerInvariant().TrimStart('.', ' ');

                if (fmt == "docx")
                {
                    return ExportToDocx(document, documentNodeId, outputPath);
                }
                if (fmt == "pdf")
                {
                    return ExportToPdf(document, documentNodeId, outputPath);
                }
                return ErrorJson($"不支持的导出格式: {format}（仅支持 docx 和 pdf）");
            }
            catch (Exception ex)
            {
                return ErrorJson("导出文档失败: " + ex.Message);
            }
        }

        // =============================================
        // 导出实现
        // =============================================

        /// <summary>
        /// 通过 Document.MakePackage() 导出 docx
        /// </summary>
        private static string ExportToDocx(Document document, long documentNodeId, string outputPath)
        {
            var package = document.MakePackage(isExport: true);
            string tempFile = package.Item1;
            try
            {
                EnsureOutputDirectory(outputPath);
                File.Copy(tempFile, outputPath, true);

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["format"] = "docx",
                    ["output_path"] = outputPath,
                    ["message"] = "文档已导出为 docx"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            finally
            {
                TryDeleteFile(tempFile);
            }
        }

        /// <summary>
        /// 通过 STA 线程上的 TX TextControl ServerTextControl 将 docx 转为 pdf
        /// </summary>
        private static string ExportToPdf(Document document, long documentNodeId, string outputPath)
        {
            // 先生成 docx 临时文件
            var package = document.MakePackage(isExport: true);
            string docxTempFile = package.Item1;
            string pdfTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");

            try
            {
                EnsureOutputDirectory(outputPath);

                // TX TextControl 操作必须在 STA 线程上执行
                StaRunner.Run(() =>
                {
                    var tx = new TXTextControl.ServerTextControl();
                    tx.Create();
                    tx.Load(docxTempFile, TXTextControl.StreamType.WordprocessingML);
                    tx.Save(pdfTempFile, TXTextControl.StreamType.AdobePDF);
                });

                File.Copy(pdfTempFile, outputPath, true);

                var result = new JObject
                {
                    ["success"] = true,
                    ["document_node_id"] = documentNodeId.ToString(),
                    ["format"] = "pdf",
                    ["output_path"] = outputPath,
                    ["message"] = "文档已导出为 pdf"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            finally
            {
                TryDeleteFile(docxTempFile);
                TryDeleteFile(pdfTempFile);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

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
        /// 从段落 OOXML Stream 中提取纯文本（拼接所有 w:t 元素）
        /// </summary>
        private static string ExtractText(string stream)
        {
            if (string.IsNullOrEmpty(stream))
                return "";

            try
            {
                var root = XElement.Parse(stream);
                var ns = root.GetNamespaceOfPrefix("w");
                if (ns == null || ns.NamespaceName.Length == 0)
                    ns = W;
                return string.Concat(root.Descendants(ns + "t").Select(t => t.Value));
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 创建包含指定文本的简单 OOXML 段落 Stream
        /// </summary>
        private static string CreateParagraphStream(string text)
        {
            var p = new XElement(W + "p",
                new XAttribute(XNamespace.Xmlns + "w", W.NamespaceName),
                new XElement(W + "r",
                    new XElement(W + "t", text ?? "")));
            return p.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// 确保输出目录存在
        /// </summary>
        private static void EnsureOutputDirectory(string outputPath)
        {
            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// 尝试删除文件（忽略失败）
        /// </summary>
        private static void TryDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // 忽略临时文件清理失败
            }
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
