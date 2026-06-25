﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.IO;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 导出服务层
    /// 统一封装文档与表格的多格式导出能力。
    /// 通过节点 ID 自动识别节点类型（文档/表格），并委派给 DocumentService 或 TableService 完成。
    /// 支持格式：Excel(xlsx)、Word(docx)、PDF、图片（暂未实现）、批量导出。
    /// </summary>
    public static class ExportService
    {
        // =============================================
        // 单项导出
        // =============================================

        /// <summary>
        /// 导出为 Excel（.xlsx）
        /// 仅支持表格节点，委派给 TableService.ExportTable。
        /// </summary>
        /// <param name="nodeId">树节点 ID（应为表格节点）</param>
        /// <param name="outputPath">输出文件路径（.xlsx）</param>
        /// <returns>操作结果 JSON</returns>
        public static string ExportToExcel(long nodeId, string outputPath)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (string.IsNullOrWhiteSpace(outputPath))
                    return ErrorJson("输出路径不能为空");

                var node = FindNode(nodeId);
                if (node == null)
                    return ErrorJson($"未找到节点: {nodeId}");

                if (node is TreeTableNode)
                {
                    return TableService.ExportTable(nodeId, outputPath);
                }

                return ErrorJson($"节点 {nodeId}（{node.Name}）不是表格节点，无法导出为 Excel");
            }
            catch (Exception ex)
            {
                return ErrorJson("导出 Excel 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 导出为 Word（.docx）
        /// 仅支持文档节点，委派给 DocumentService.ExportDocument（format=docx）。
        /// </summary>
        /// <param name="nodeId">树节点 ID（应为文档节点）</param>
        /// <param name="outputPath">输出文件路径（.docx）</param>
        /// <returns>操作结果 JSON</returns>
        public static string ExportToWord(long nodeId, string outputPath)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (string.IsNullOrWhiteSpace(outputPath))
                    return ErrorJson("输出路径不能为空");

                var node = FindNode(nodeId);
                if (node == null)
                    return ErrorJson($"未找到节点: {nodeId}");

                if (node is TreeDocumentNode)
                {
                    return DocumentService.ExportDocument(nodeId, "docx", outputPath);
                }

                return ErrorJson($"节点 {nodeId}（{node.Name}）不是文档节点，无法导出为 Word");
            }
            catch (Exception ex)
            {
                return ErrorJson("导出 Word 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 导出为 PDF
        /// 文档节点：委派给 DocumentService.ExportDocument（format=pdf），通过 TX TextControl 转换。
        /// 表格节点：暂未实现（需 C1FlexGrid/C1Report 渲染）。
        /// </summary>
        /// <param name="nodeId">树节点 ID</param>
        /// <param name="outputPath">输出文件路径（.pdf）</param>
        /// <returns>操作结果 JSON</returns>
        public static string ExportToPdf(long nodeId, string outputPath)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (string.IsNullOrWhiteSpace(outputPath))
                    return ErrorJson("输出路径不能为空");

                var node = FindNode(nodeId);
                if (node == null)
                    return ErrorJson($"未找到节点: {nodeId}");

                if (node is TreeDocumentNode)
                {
                    return DocumentService.ExportDocument(nodeId, "pdf", outputPath);
                }

                if (node is TreeTableNode)
                {
                    // 表格导出 PDF 需要 C1FlexGrid/C1Report 在 UI 线程渲染，当前无头环境暂未实现
                    return ErrorJson($"表格节点 {nodeId}（{node.Name}）导出为 PDF 暂未实现，请使用 export_to_excel 导出为 Excel");
                }

                return ErrorJson($"节点 {nodeId}（{node.Name}）不支持导出为 PDF");
            }
            catch (Exception ex)
            {
                return ErrorJson("导出 PDF 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 导出为图片
        /// 当前无头环境下暂未实现（需 UI 渲染控件支持）。
        /// </summary>
        /// <param name="nodeId">树节点 ID</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <returns>操作结果 JSON</returns>
        public static string ExportToImage(long nodeId, string outputPath)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (string.IsNullOrWhiteSpace(outputPath))
                    return ErrorJson("输出路径不能为空");

                var node = FindNode(nodeId);
                if (node == null)
                    return ErrorJson($"未找到节点: {nodeId}");

                // 图片导出需要 UI 控件（C1FlexGrid/TX TextControl）渲染到位图，无头环境暂未实现
                return ErrorJson($"节点 {nodeId}（{node.Name}）导出为图片暂未实现");
            }
            catch (Exception ex)
            {
                return ErrorJson("导出图片失败: " + ex.Message);
            }
        }

        // =============================================
        // 批量导出
        // =============================================

        /// <summary>
        /// 批量导出多个节点
        /// 根据节点类型和目标格式自动选择导出方式：
        /// - excel: 表格 → TableService.ExportTable
        /// - word:  文档 → DocumentService.ExportDocument(docx)
        /// - pdf:   文档 → DocumentService.ExportDocument(pdf)；表格暂未实现
        /// - image: 暂未实现
        /// 输出文件名自动生成：{节点名称}.{格式后缀}，若已存在则追加序号。
        /// </summary>
        /// <param name="nodeIds">节点 ID 数组</param>
        /// <param name="outputDir">输出目录</param>
        /// <param name="format">目标格式：excel/word/pdf/image</param>
        /// <returns>批量导出结果汇总 JSON</returns>
        public static string BatchExport(JArray nodeIds, string outputDir, string format)
        {
            try
            {
                SessionState.Current.EnsureProject();

                if (nodeIds == null || nodeIds.Count == 0)
                    return ErrorJson("节点 ID 列表不能为空");

                if (string.IsNullOrWhiteSpace(outputDir))
                    return ErrorJson("输出目录不能为空");

                if (string.IsNullOrWhiteSpace(format))
                    return ErrorJson("导出格式不能为空");

                string fmt = format.ToLowerInvariant().TrimStart('.', ' ');
                string ext = GetExtensionForFormat(fmt);
                if (ext == null)
                    return ErrorJson($"不支持的导出格式: {format}（支持 excel/word/pdf/image）");

                // 确保输出目录存在
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                var results = new JArray();
                int successCount = 0;
                int failCount = 0;

                foreach (var idToken in nodeIds)
                {
                    long nodeId = idToken.Value<long>();
                    var item = new JObject
                    {
                        ["node_id"] = nodeId.ToString()
                    };

                    try
                    {
                        var node = FindNode(nodeId);
                        if (node == null)
                        {
                            item["success"] = false;
                            item["error"] = $"未找到节点: {nodeId}";
                            item["node_name"] = "";
                            item["node_type"] = "unknown";
                            failCount++;
                            results.Add(item);
                            continue;
                        }

                        item["node_name"] = node.Name;
                        item["node_type"] = GetNodeTypeName(node);

                        string fileName = MakeSafeFileName(node.Name) + ext;
                        string outputPath = Path.Combine(outputDir, fileName);
                        outputPath = EnsureUniquePath(outputPath);

                        string exportResultJson;
                        switch (fmt)
                        {
                            case "excel":
                            case "xlsx":
                                exportResultJson = ExportToExcel(nodeId, outputPath);
                                break;
                            case "word":
                            case "docx":
                                exportResultJson = ExportToWord(nodeId, outputPath);
                                break;
                            case "pdf":
                                exportResultJson = ExportToPdf(nodeId, outputPath);
                                break;
                            case "image":
                                exportResultJson = ExportToImage(nodeId, outputPath);
                                break;
                            default:
                                item["success"] = false;
                                item["error"] = $"不支持的格式: {format}";
                                failCount++;
                                results.Add(item);
                                continue;
                        }

                        // 解析单项导出结果，提取关键信息
                        var exportResult = JObject.Parse(exportResultJson);
                        bool ok = exportResult["success"]?.Value<bool>() ?? false;
                        item["success"] = ok;
                        item["output_path"] = exportResult["output_path"]?.ToString() ?? outputPath;
                        if (!ok)
                        {
                            item["error"] = exportResult["error"]?.ToString() ?? "导出失败";
                            failCount++;
                        }
                        else
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        item["success"] = false;
                        item["error"] = ex.Message;
                        failCount++;
                    }

                    results.Add(item);
                }

                var summary = new JObject
                {
                    ["success"] = failCount == 0,
                    ["format"] = fmt,
                    ["output_dir"] = outputDir,
                    ["total"] = nodeIds.Count,
                    ["success_count"] = successCount,
                    ["fail_count"] = failCount,
                    ["results"] = results,
                    ["message"] = $"批量导出完成：成功 {successCount} 个，失败 {failCount} 个"
                };
                return JsonConvert.SerializeObject(summary, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("批量导出失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        /// <summary>
        /// 根据节点 ID 查找树节点
        /// </summary>
        private static TreeNodeBase FindNode(long nodeId)
        {
            var project = SessionState.Current.CurrentProject;
            return project.GetNodeById(new Id64(nodeId));
        }

        /// <summary>
        /// 获取节点类型名称
        /// </summary>
        private static string GetNodeTypeName(TreeNodeBase node)
        {
            if (node is TreeDirectoryNode) return "directory";
            if (node is TreeTableNode) return "table";
            if (node is TreeDocumentNode) return "document";
            if (node is TreeImageNode) return "image";
            if (node is TreePdfNode) return "pdf";
            return "unknown";
        }

        /// <summary>
        /// 根据格式名称获取文件扩展名（含点号）
        /// </summary>
        private static string GetExtensionForFormat(string fmt)
        {
            switch (fmt)
            {
                case "excel":
                case "xlsx":
                    return ".xlsx";
                case "word":
                case "docx":
                    return ".docx";
                case "pdf":
                    return ".pdf";
                case "image":
                case "png":
                    return ".png";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 将节点名称转换为安全的文件名（移除非法字符）
        /// </summary>
        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "未命名";

            char[] invalid = Path.GetInvalidFileNameChars();
            var sb = new System.Text.StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (Array.IndexOf(invalid, c) >= 0)
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            string safe = sb.ToString().Trim();
            return safe.Length == 0 ? "未命名" : safe;
        }

        /// <summary>
        /// 确保文件路径唯一：若已存在则追加序号 (1)、(2)...
        /// </summary>
        private static string EnsureUniquePath(string path)
        {
            if (!File.Exists(path))
                return path;

            string dir = Path.GetDirectoryName(path);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);

            int seq = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(dir, $"{nameWithoutExt} ({seq}){ext}");
                seq++;
            }
            while (File.Exists(candidate));

            return candidate;
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
