﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Linq;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 项目变量服务
    /// 封装项目模板变量（DataReference）的读取和编辑操作。
    /// 变量存储在 Project.DataReferenceManager 中，支持 Text、CellRef、BuiltIn 等类型。
    /// </summary>
    public static class ProjectVariableService
    {
        /// <summary>
        /// 获取项目中的所有模板变量
        /// </summary>
        /// <param name="projectId">项目 ID</param>
        /// <returns>变量列表的 JSON</returns>
        public static string GetProjectVariables(long projectId)
        {
            try
            {
                SessionState.Current.EnsureProject();
                var project = SessionState.Current.CurrentProject;

                var variables = project.DataReferenceManager.Enumerate()
                    .Select(v => new JObject
                    {
                        ["name"] = v.Key,
                        ["value"] = v.Value ?? "",
                        ["kind"] = v.Kind.ToString(),
                        ["status"] = v.Status.ToString()
                    })
                    .ToList();

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = project.Id.ToString(),
                    ["project_name"] = project.Name,
                    ["variable_count"] = variables.Count,
                    ["variables"] = new JArray(variables)
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取项目变量失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 设置指定变量的值
        /// </summary>
        /// <param name="projectId">项目 ID</param>
        /// <param name="variableName">变量名称</param>
        /// <param name="value">变量值</param>
        /// <returns>操作结果 JSON</returns>
        public static string SetProjectVariable(long projectId, string variableName, string value)
        {
            try
            {
                SessionState.Current.EnsureProject();
                var project = SessionState.Current.CurrentProject;

                var variable = project.DataReferenceManager.Get(variableName);
                if (variable == null)
                    return ErrorJson($"未找到变量: {variableName}");

                variable.UpdateValue(value ?? "");
                project.NeedSave = true;

                var result = new JObject
                {
                    ["success"] = true,
                    ["variable_name"] = variableName,
                    ["value"] = value,
                    ["message"] = "变量值已更新，请调用 save_project 保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置项目变量失败: " + ex.Message);
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