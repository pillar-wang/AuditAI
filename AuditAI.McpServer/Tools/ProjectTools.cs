﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 项目管理 MCP 工具注册
    /// </summary>
    public static class ProjectTools
    {
        /// <summary>
        /// 注册所有项目管理工具
        /// </summary>
        public static void Register()
        {
            // list_projects
            ToolRegistry.Register("list_projects",
                "列举所有本地审计项目。返回项目名称、路径、修改时间、大小等信息。当用户想查看有哪些项目时调用此工具。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => ProjectService.ListProjects());

            // create_project
            ToolRegistry.Register("create_project",
                "创建新的审计项目。需要提供项目名称和年度。可选提供模板类型（如：审计、税务、评估）。返回项目ID和路径。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["name"] = new JObject { ["type"] = "string", ["description"] = "项目名称，如：ABC有限公司2025年度审计" },
                        ["year"] = new JObject { ["type"] = "integer", ["description"] = "审计年度，如：2025" },
                        ["template_type"] = new JObject { ["type"] = "string", ["description"] = "模板类型（可选）：audit/tax/appraiser" }
                    },
                    ["required"] = new JArray { "name", "year" }
                },
                (args) =>
                {
                    string name = args["name"]?.ToString();
                    int year = args["year"]?.Value<int>() ?? DateTime.Now.Year;
                    string templateType = args["template_type"]?.ToString();
                    return ProjectService.CreateProject(name, year, templateType);
                });

            // open_project
            ToolRegistry.Register("open_project",
                "打开指定路径的审计项目，加载到当前会话。后续操作（如获取项目树、编辑表格等）将作用于该项目。返回项目元信息。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["path"] = new JObject { ["type"] = "string", ["description"] = "项目文件路径（.db文件）" }
                    },
                    ["required"] = new JArray { "path" }
                },
                (args) =>
                {
                    string path = args["path"]?.ToString();
                    return ProjectService.OpenProject(path);
                });

            // save_project
            ToolRegistry.Register("save_project",
                "保存当前打开项目的所有变更到数据库。在对项目进行修改后调用此工具持久化数据。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => ProjectService.SaveProject());

            // close_project
            ToolRegistry.Register("close_project",
                "关闭当前打开的项目，释放资源。关闭后需要重新打开项目才能继续操作。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => ProjectService.CloseProject());

            // get_project_info
            ToolRegistry.Register("get_project_info",
                "获取当前打开项目的详细信息，包括项目名称、年度、创建时间、节点数量等。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => ProjectService.GetProjectInfo());
        }
    }
}
