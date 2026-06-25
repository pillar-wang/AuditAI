﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 项目变量 MCP 工具注册
    /// </summary>
    public static class ProjectVariableTools
    {
        /// <summary>
        /// 注册所有项目变量工具
        /// </summary>
        public static void Register()
        {
            // get_project_variables
            ToolRegistry.Register("get_project_variables",
                "获取当前项目中所有模板变量的列表。返回每个变量的名称、当前值、类型和状态。需要先调用 open_project 打开项目。变量用于存储项目级别的动态数据（如项目编号、单位名称等）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "项目 ID（当前项目 ID，可通过 get_project_info 获取）"
                        }
                    },
                    ["required"] = new JArray { "project_id" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    return ProjectVariableService.GetProjectVariables(projectId);
                });

            // set_project_variable
            ToolRegistry.Register("set_project_variable",
                "设置项目中指定模板变量的值。变量名可通过 get_project_variables 获取。修改后需调用 save_project 保存。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["project_id"] = new JObject
                        {
                            ["type"] = "integer",
                            ["description"] = "项目 ID（当前项目 ID，可通过 get_project_info 获取）"
                        },
                        ["variable_name"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "变量名称"
                        },
                        ["value"] = new JObject
                        {
                            ["type"] = "string",
                            ["description"] = "要设置的变量值"
                        }
                    },
                    ["required"] = new JArray { "project_id", "variable_name", "value" }
                },
                (args) =>
                {
                    long projectId = args["project_id"]?.Value<long>() ?? 0;
                    string variableName = args["variable_name"]?.Value<string>() ?? "";
                    string value = args["value"]?.Value<string>() ?? "";
                    return ProjectVariableService.SetProjectVariable(projectId, variableName, value);
                });
        }
    }
}