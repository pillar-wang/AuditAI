﻿using System;
using AuditAI.McpServer.Protocol;
using AuditAI.McpServer.Services;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Tools
{
    /// <summary>
    /// 服务端运维 MCP 工具注册（Task 8）
    /// 提供本地构建、发布、SCP 部署、SSH 日志/DB 查询、HTTP 健康检查能力。
    /// </summary>
    public static class ServerOpsTools
    {
        /// <summary>
        /// 注册所有服务端运维工具（6 个）。
        /// </summary>
        public static void Register()
        {
            // build_server
            ToolRegistry.Register("build_server",
                "在本地构建服务端项目（e:\\lq\\Server\\AuditApiServer，目标 net8.0）。返回 {success, output, error, exitCode, durationMs}。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["configuration"] = new JObject { ["type"] = "string", ["description"] = "构建配置（Debug/Release，默认 Debug）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => ServerOpsService.BuildServer(args["configuration"] != null ? args["configuration"].ToString() : "Debug").ToJson());

            // publish_server
            ToolRegistry.Register("publish_server",
                "发布服务端到本地目录（dotnet publish -c Release -r linux-x64 --self-contained false -o %TEMP%/auditapi-publish），供后续 deploy_to_production 上传。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["configuration"] = new JObject { ["type"] = "string", ["description"] = "发布配置（默认 Release）" },
                        ["runtime"] = new JObject { ["type"] = "string", ["description"] = "目标 RID（默认 linux-x64）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => ServerOpsService.PublishServer(
                    args["configuration"] != null ? args["configuration"].ToString() : "Release",
                    args["runtime"] != null ? args["runtime"].ToString() : "linux-x64").ToJson());

            // deploy_to_production
            ToolRegistry.Register("deploy_to_production",
                "通过 SCP 上传已发布的服务端到生产服务器（82.156.108.218:/opt/auditapi/），重启 auditapi systemd 服务，并执行 HTTP 健康检查。需要 Windows 已安装 OpenSSH(scp/ssh) 或 PuTTY(pscp/plink)。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["localPublishPath"] = new JObject { ["type"] = "string", ["description"] = "本地发布产物目录（可选，默认 %TEMP%/auditapi-publish）" }
                    },
                    ["required"] = new JArray()
                },
                (args) => ServerOpsService.DeployToServer(args["localPublishPath"] != null ? args["localPublishPath"].ToString() : null).ToJson());

            // get_server_logs
            ToolRegistry.Register("get_server_logs",
                "通过 SSH 获取服务端 systemd 日志（journalctl -u auditapi）。可使用 filter 关键词过滤（grep -i，不区分大小写）。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["lines"] = new JObject { ["type"] = "integer", ["description"] = "获取日志行数（默认 100，上限 10000）" },
                        ["filter"] = new JObject { ["type"] = "string", ["description"] = "过滤关键词（如 error、exception、auditapi），不区分大小写" }
                    },
                    ["required"] = new JArray()
                },
                (args) => ServerOpsService.GetServerLogs(
                    args["lines"] != null ? args["lines"].Value<int>() : 100,
                    args["filter"] != null ? args["filter"].ToString() : null).ToJson());

            // query_server_db
            ToolRegistry.Register("query_server_db",
                "通过 SSH 在服务端执行 SQLite 查询（sqlite3 /opt/auditapi/Data/auditai_server.db）。仅用于只读 SELECT，避免执行写入/DDL。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["sql"] = new JObject { ["type"] = "string", ["description"] = "SQL 查询语句（建议只读 SELECT，如 SELECT name FROM sqlite_master WHERE type='table';）" }
                    },
                    ["required"] = new JArray { "sql" }
                },
                (args) => ServerOpsService.QueryServerDb(args["sql"] != null ? args["sql"].ToString() : "").ToJson());

            // get_server_health
            ToolRegistry.Register("get_server_health",
                "执行服务端健康检查（HTTP GET /api/User/UserNameExists?userName=healthcheck，白名单端点无需鉴权）。任意 HTTP 响应（含 4xx/5xx）即视为在线；网络异常则视为离线。",
                new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject(),
                    ["required"] = new JArray()
                },
                (args) => ServerOpsService.GetServerHealth().ToJson());
        }
    }
}
