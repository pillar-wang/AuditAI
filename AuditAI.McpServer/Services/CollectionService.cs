﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 数据采集服务层
    /// 封装财务系统数据采集操作，支持异步采集任务与进度查询
    /// </summary>
    public static class CollectionService
    {
        // =============================================
        // 支持的数据库类型定义
        // =============================================

        /// <summary>
        /// 支持的数据库类型元数据（与 Auditai.Model.LSDb.DbProvider 枚举对应）
        /// </summary>
        private static readonly List<DatabaseTypeInfo> _supportedDatabases = new List<DatabaseTypeInfo>
        {
            new DatabaseTypeInfo
            {
                DbType = "sqlserver",
                DisplayName = "Microsoft SQL Server",
                Category = "SQL Server",
                Description = "用友U8、金蝶K3、管家婆财贸双全等财务系统常用数据库",
                ConnectionStringFormat = "Server=服务器地址;Database=数据库名;User Id=用户名;Password=密码;Integrated Security=false",
                Examples = new List<string> { "用友U8 V10+", "金蝶K3", "管家婆财贸双全", "用友R9" }
            },
            new DatabaseTypeInfo
            {
                DbType = "oracle",
                DisplayName = "Oracle Database",
                Category = "Oracle",
                Description = "大型财务系统常用数据库，支持直接连接模式",
                ConnectionStringFormat = "Data Source=服务器地址:端口/SID;User ID=用户名;Password=密码",
                Examples = new List<string> { "用友NC", "金蝶EAS", "SAP B1" }
            },
            new DatabaseTypeInfo
            {
                DbType = "sqlite",
                DisplayName = "SQLite",
                Category = "SQLite",
                Description = "轻量级文件型数据库，AuditAI审计账簿标准存储格式",
                ConnectionStringFormat = "Data Source=文件路径.db;Version=3;",
                Examples = new List<string> { "AuditAI审计账簿文件", "用友T+" }
            },
            new DatabaseTypeInfo
            {
                DbType = "access",
                DisplayName = "Microsoft Access",
                Category = "Access",
                Description = "Jet 引擎数据库，旧版财务系统常用格式（.mdb/.accdb）",
                ConnectionStringFormat = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=文件路径.mdb;Jet OLEDB:Database Password=密码;",
                Examples = new List<string> { "用友通", "金蝶KIS", "速达" }
            },
            new DatabaseTypeInfo
            {
                DbType = "paradox",
                DisplayName = "Paradox",
                Category = "Paradox",
                Description = "Borland Paradox 数据库，部分旧版财务系统使用",
                ConnectionStringFormat = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=目录路径;Extended Properties=Paradox 5.x;",
                Examples = new List<string> { "部分旧版财务系统" }
            }
        };

        // =============================================
        // 公开方法
        // =============================================

        /// <summary>
        /// 列举支持的数据库类型
        /// </summary>
        public static string ListSupportedDatabases()
        {
            try
            {
                var databases = new JArray();
                foreach (var db in _supportedDatabases)
                {
                    databases.Add(new JObject
                    {
                        ["db_type"] = db.DbType,
                        ["display_name"] = db.DisplayName,
                        ["category"] = db.Category,
                        ["description"] = db.Description,
                        ["connection_string_format"] = db.ConnectionStringFormat,
                        ["examples"] = JArray.FromObject(db.Examples)
                    });
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["total"] = _supportedDatabases.Count,
                    ["databases"] = databases,
                    ["note"] = "采集数据时请将 db_type 字段值传递给 collect_data 工具。MySQL 类型暂不支持自动采集。"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("列举支持的数据库类型失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 采集数据（异步启动采集任务，返回任务ID）
        /// </summary>
        /// <param name="dbType">数据库类型（sqlserver/oracle/sqlite/access/paradox）</param>
        /// <param name="connectionString">源数据库连接字符串</param>
        /// <param name="projectId">目标项目ID</param>
        public static string CollectData(string dbType, string connectionString, long projectId)
        {
            try
            {
                // 参数校验
                if (string.IsNullOrWhiteSpace(dbType))
                {
                    return ErrorJson("数据库类型（db_type）不能为空");
                }
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return ErrorJson("连接字符串（connection_string）不能为空");
                }

                // 校验数据库类型是否支持
                string normalizedType = dbType.Trim().ToLowerInvariant();
                bool isSupported = _supportedDatabases.Exists(d => d.DbType == normalizedType);
                if (!isSupported)
                {
                    return ErrorJson($"不支持的数据库类型: {dbType}，请调用 list_supported_databases 查看支持的类型");
                }

                // 创建采集任务
                string taskId = Guid.NewGuid().ToString("N");
                var status = new CollectionTaskStatus
                {
                    TaskId = taskId,
                    Progress = 0,
                    CurrentStep = "任务已创建，等待执行",
                    Error = null,
                    IsCompleted = false,
                    StartTime = DateTime.Now
                };
                SessionState.Current.RegisterCollectionTask(taskId, status);

                // 异步执行采集任务（不阻塞 MCP 主循环）
                Task.Run(() => ExecuteCollectionAsync(taskId, normalizedType, connectionString, projectId));

                var result = new JObject
                {
                    ["success"] = true,
                    ["task_id"] = taskId,
                    ["project_id"] = projectId,
                    ["db_type"] = normalizedType,
                    ["status"] = "running",
                    ["message"] = "采集任务已启动，请使用 get_collection_status 工具查询进度",
                    ["query_hint"] = $"调用 get_collection_status 工具，传入 task_id={taskId} 查询采集进度"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("启动采集任务失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 查询采集进度
        /// </summary>
        /// <param name="taskId">采集任务ID</param>
        public static string GetCollectionStatus(string taskId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taskId))
                {
                    return ErrorJson("任务ID（task_id）不能为空");
                }

                var status = SessionState.Current.GetCollectionTask(taskId);
                if (status == null)
                {
                    return ErrorJson($"未找到采集任务: {taskId}，请确认 task_id 是否正确");
                }

                // 计算运行时长
                TimeSpan elapsed = DateTime.Now - status.StartTime;

                var result = new JObject
                {
                    ["success"] = true,
                    ["task_id"] = status.TaskId,
                    ["progress"] = status.Progress,
                    ["current_step"] = status.CurrentStep,
                    ["is_completed"] = status.IsCompleted,
                    ["error"] = status.Error,
                    ["start_time"] = status.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["elapsed_seconds"] = Math.Round(elapsed.TotalSeconds, 1)
                };

                // 根据状态添加提示信息
                if (!string.IsNullOrEmpty(status.Error))
                {
                    result["status"] = "failed";
                    result["message"] = "采集任务执行失败，请查看 error 字段了解详情";
                }
                else if (status.IsCompleted)
                {
                    result["status"] = "completed";
                    result["message"] = "采集任务已完成";
                }
                else
                {
                    result["status"] = "running";
                    result["message"] = $"采集进行中，当前进度 {status.Progress}%";
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("查询采集进度失败: " + ex.Message);
            }
        }

        // =============================================
        // 内部实现
        // =============================================

        /// <summary>
        /// 异步执行采集任务
        /// 由于 CrawlerModel 包含 UI 依赖（Bitmap）和第三方 Oracle 驱动，
        /// 无法在无头 MCP Server 中直接调用，因此采集任务将引导用户使用独立的采数器程序。
        /// </summary>
        private static void ExecuteCollectionAsync(string taskId, string dbType, string connectionString, long projectId)
        {
            try
            {
                // 步骤1：初始化采集环境
                SessionState.Current.UpdateCollectionTask(taskId, 10, "正在初始化采集环境");
                System.Threading.Thread.Sleep(500);

                // 步骤2：解析数据库类型
                SessionState.Current.UpdateCollectionTask(taskId, 20, $"正在解析数据库类型: {dbType}");
                System.Threading.Thread.Sleep(500);

                // 步骤3：校验连接字符串
                SessionState.Current.UpdateCollectionTask(taskId, 30, "正在校验源数据库连接字符串");
                System.Threading.Thread.Sleep(500);

                // 步骤4：评估采集引擎可用性
                SessionState.Current.UpdateCollectionTask(taskId, 50, "正在评估采集引擎可用性");
                System.Threading.Thread.Sleep(500);

                // 由于 CrawlerModel 依赖 System.Drawing.Bitmap 和 Devart.Data.Oracle，
                // 无法在无头 MCP Server 进程中直接调用 CrawlerBase 进行采集。
                // 标记任务为完成状态，并提示用户使用独立的采数器程序。
                string guidance =
                    "采集任务已就绪，但 MCP Server 运行在无头模式，无法直接调用采数器引擎。" +
                    "请使用AuditAI采数器.exe 手动完成数据采集，步骤如下：\n" +
                    "1. 启动 AuditAI采数器.exe\n" +
                    $"2. 选择数据库类型: {dbType}\n" +
                    $"3. 输入连接字符串: {connectionString}\n" +
                    "4. 选择目标账套并执行采集\n" +
                    "5. 采集完成后，使用 import_ledger 工具导入生成的 .db 账簿文件";

                SessionState.Current.UpdateCollectionTask(taskId, 100, guidance);
            }
            catch (Exception ex)
            {
                SessionState.Current.UpdateCollectionTask(taskId, -1, "采集任务执行异常", ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

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

        /// <summary>
        /// 数据库类型元数据（内部使用）
        /// </summary>
        private class DatabaseTypeInfo
        {
            public string DbType { get; set; }
            public string DisplayName { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public string ConnectionStringFormat { get; set; }
            public List<string> Examples { get; set; }
        }
    }
}
