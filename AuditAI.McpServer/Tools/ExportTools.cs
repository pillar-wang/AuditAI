﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 导出 MCP 工具注册
    /// 提供文档与表格的多格式导出能力（Excel/Word/PDF/图片/批量导出）。
    /// 工具会根据节点类型自动选择合适的导出方式。
    /// </summary>
    public static class ExportTools
    {
        /// <summary>
        /// 注册所有导出工具
        /// </summary>
        public static void Register()
        {
            // export_to_excel
            ToolRegistry.Register("export_to_excel",
                "将指定节点导出为 Excel 文件（.xlsx）。仅支持表格节点，包含列标题和所有单元格数据。当需要将审计底稿表格导出为 Excel 时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "树节点 ID（应为表格节点，可通过 get_project_tree 获取）"
                        },
                        ["output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "输出文件完整路径（.xlsx 后缀），如 C:\\\\export\\\\资产负债表.xlsx"
                        }
                    },
                    ["required"] = new JArray { "node_id", "output_path" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    string outputPath = args["output_path"]?.ToString();
                    return ExportService.ExportToExcel(nodeId, outputPath);
                });

            // export_to_word
            ToolRegistry.Register("export_to_word",
                "将指定节点导出为 Word 文件（.docx）。仅支持文档节点，通过文档模型直接生成 docx。当需要将审计报告、管理建议书等文档导出为 Word 时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "树节点 ID（应为文档节点，可通过 get_project_tree 获取）"
                        },
                        ["output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "输出文件完整路径（.docx 后缀），如 C:\\\\export\\\\审计报告.docx"
                        }
                    },
                    ["required"] = new JArray { "node_id", "output_path" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    string outputPath = args["output_path"]?.ToString();
                    return ExportService.ExportToWord(nodeId, outputPath);
                });

            // export_to_pdf
            ToolRegistry.Register("export_to_pdf",
                "将指定节点导出为 PDF 文件。文档节点通过 TX TextControl 在 STA 线程上由 docx 转换为 PDF；表格节点导出 PDF 暂未实现，建议使用 export_to_excel。当需要将文档导出为 PDF 时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "树节点 ID（文档节点可导出 PDF，表格节点暂不支持）"
                        },
                        ["output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "输出文件完整路径（.pdf 后缀），如 C:\\\\export\\\\审计报告.pdf"
                        }
                    },
                    ["required"] = new JArray { "node_id", "output_path" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    string outputPath = args["output_path"]?.ToString();
                    return ExportService.ExportToPdf(nodeId, outputPath);
                });

            // export_to_image
            ToolRegistry.Register("export_to_image",
                "将指定节点导出为图片文件。当前无头环境下暂未实现（需要 UI 渲染控件支持）。如需导出表格数据请使用 export_to_excel，导出文档请使用 export_to_word 或 export_to_pdf。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "树节点 ID"
                        },
                        ["output_path"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "输出文件完整路径（.png 后缀）"
                        }
                    },
                    ["required"] = new JArray { "node_id", "output_path" }
                },
                (args) =>
                {
                    long nodeId = args["node_id"]?.Value<long>() ?? 0;
                    string outputPath = args["output_path"]?.ToString();
                    return ExportService.ExportToImage(nodeId, outputPath);
                });

            // batch_export
            ToolRegistry.Register("batch_export",
                "批量导出多个节点到指定目录。根据节点类型（表格/文档）和目标格式自动选择导出方式：excel 仅导出表格，word/pdf 仅导出文档，image 暂未实现。输出文件名自动按节点名称生成，重名时追加序号。返回每个节点的导出结果汇总。当需要一次性导出多个底稿或文档时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["node_ids"] = new JObject
                        {
                            ["type"] = "array",
                            ["description"] = "要导出的节点 ID 数组",
                            ["items"] = new JObject { ["type"] = "integer" }
                        },
                        ["output_dir"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "输出目录路径，如 C:\\\\export\\\\batch"
                        },
                        ["format"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "目标导出格式：excel、word、pdf、image",
                            ["enum"] = new JArray { "excel", "word", "pdf", "image" }
                        }
                    },
                    ["required"] = new JArray { "node_ids", "output_dir", "format" }
                },
                (args) =>
                {
                    JArray nodeIds = args["node_ids"] as JArray;
                    string outputDir = args["output_dir"]?.ToString();
                    string format = args["format"]?.ToString();
                    return ExportService.BatchExport(nodeIds, outputDir, format);
                });
        }
    }
}
