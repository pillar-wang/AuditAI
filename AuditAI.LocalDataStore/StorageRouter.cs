﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Auditai.DTO;
using Auditai.Model;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.LocalDataStore
{
    /// <summary>
    /// 存储路由 - 根据配置自动选择 Local 或 Server 模式
    /// 作为 WebApiClient 的替代入口
    /// </summary>
    public static class StorageRouter
    {
        private static bool _isLocalMode;
        private static bool _initialized;

        /// <summary>是否已初始化为本地模式</summary>
        public static bool IsLocalMode => _isLocalMode;

        /// <summary>
        /// Server 模式激活时的回调（由宿主程序注册，用于启用云端同步等依赖 ProjectModel 的逻辑；
        /// LocalDataStore 不直接引用 ProjectModel，避免项目循环引用）
        /// </summary>
        public static Action OnServerModeActivated { get; set; }

        /// <summary>
        /// 初始化存储模式
        /// 应在 Program.Main() 的早期调用
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            string mode = ConfigurationManager.AppSettings["StorageMode"] ?? "Server";
            _isLocalMode = mode.Equals("Local", StringComparison.OrdinalIgnoreCase);

            if (_isLocalMode)
            {
                string dbPath = ConfigurationManager.AppSettings["LocalDbPath"]
                    ?? "Data/auditai_audit.db";
                string projectDataPath = ConfigurationManager.AppSettings["LocalProjectDataPath"]
                    ?? "Data/Projects";

                LocalDataStore.Initialize(dbPath, projectDataPath);

                // 注册本地模式处理器到 WebApiClient
                WebApiClient.IsLocalMode = true;
                WebApiClient.LocalGetProjectsHandler = LocalDataStore.GetProjects;
                WebApiClient.LocalGetTemplatesHandler = LocalDataStore.GetTemplates;
                WebApiClient.LocalCreateProjectHandler = LocalDataStore.CreateProject;
                WebApiClient.LocalOpenProjectHandler = LocalDataStore.OpenProject;
                WebApiClient.LocalDeleteProjectHandler = LocalDataStore.DeleteProject;
                WebApiClient.LocalDeleteProjectFromServerHandler = LocalDataStore.DeleteProjectFromServer;
                WebApiClient.LocalPushTableHandler = LocalDataStore.PushTable;
                WebApiClient.LocalPushDocumentHandler = LocalDataStore.PushDocument;
                WebApiClient.LocalTableCollectDicHandler = LocalDataStore.GetTableCollectDic;
                WebApiClient.LocalCellCollectDicHandler = LocalDataStore.GetCellCollectDic;
                WebApiClient.LocalLedgerValidateDicHandler = LocalDataStore.GetLedgerValidateDic;
                WebApiClient.LocalStandardAccountDicHandler = LocalDataStore.GetStandardAccountDic;
                WebApiClient.LocalGetTeamUsersWithPicHandler = LocalDataStore.GetTeamUsersWithPic;
                WebApiClient.LocalGetUserTeamsHandler = LocalDataStore.GetUserTeams;
                WebApiClient.LocalUploadFileHandler = LocalDataStore.UploadFile;
                WebApiClient.LocalDownloadFileHandler = LocalDataStore.DownloadFile;
            }
            else
            {
                // 非本地模式（Server 模式）：通知宿主启用 Syncer，允许 Push/Pull 与服务器同步
                OnServerModeActivated?.Invoke();
            }

            // 修复：原实现把 _initialized = true 放在方法开头，工作尚未完成即置位。
            // 一旦 LocalDataStore.Initialize 或 WebApiClient 处理器注册抛异常并被宿主忽略，
            // 再次调用会因 _initialized 直接 return，留下"_isLocalMode 已为 true、
            // 但 WebApiClient 本地处理器一个都没注册"的半初始化状态，后续所有本地请求都会走空。
            // 改为全部完成后才置位：失败时不置位，允许重试。
            _initialized = true;
        }

        // =============================================
        // 路由方法 - 每个替换 WebApiClient 中原有的静态方法
        // =============================================

        public static async Task<IEnumerable<Auditai.DTO.Project>> GetProjects()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetProjects() ?? Enumerable.Empty<Auditai.DTO.Project>();
            return await WebApiClient.GetProjects() ?? Enumerable.Empty<Auditai.DTO.Project>();
        }

        public static async Task<IEnumerable<Auditai.DTO.Project>> GetTemplates()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetTemplates() ?? Enumerable.Empty<Auditai.DTO.Project>();
            return await WebApiClient.GetTemplates() ?? Enumerable.Empty<Auditai.DTO.Project>();
        }

        public static async Task CreateProject(Auditai.DTO.Project project)
        {
            if (_isLocalMode)
                await LocalDataStore.CreateProject(project);
            else
                await WebApiClient.CreateProject(project);
        }

        public static async Task UpdateProject(Auditai.DTO.Project project)
        {
            if (_isLocalMode)
                await LocalDataStore.UpdateProject(project);
            else
                await WebApiClient.UpdateProject(project);
        }

        public static async Task<Tuple<int, int>> OpenProject(Guid projectId)
        {
            if (_isLocalMode)
                return await LocalDataStore.OpenProject(projectId);
            return await WebApiClient.OpenProject(projectId);
        }

        public static async Task<JObject> PushTable(PushTable request,
            TaskProgressValueReportCallback reportCallback = null)
        {
            if (_isLocalMode)
                return await LocalDataStore.PushTable(request);
            return await WebApiClient.PushTable(request, reportCallback);
        }

        public static async Task<PullTable> PullTable(JObject request,
            TaskProgressValueReportCallback reportCallback = null)
        {
            if (_isLocalMode)
                return await LocalDataStore.PullTable(request);
            return await WebApiClient.PullTable(request, reportCallback);
        }

        public static async Task<JObject> PushDocument(PushDocument request,
            TaskProgressValueReportCallback reportCallback = null)
        {
            if (_isLocalMode)
                return await LocalDataStore.PushDocument(request);
            return await WebApiClient.PushDocument(request, reportCallback);
        }

        public static async Task<PullDocument> PullDocument(JObject request,
            TaskProgressValueReportCallback reportCallback = null)
        {
            if (_isLocalMode)
                return await LocalDataStore.PullDocument(request);
            return await WebApiClient.PullDocument(request, reportCallback);
        }

        public static async Task<JObject> GetTableCollectDic(int version = 0)
        {
            if (_isLocalMode)
                return await LocalDataStore.GetTableCollectDic(version);
            return await WebApiClient.TableCollectDic(version);
        }

        public static async Task<JObject> GetCellCollectDic(int version = 0)
        {
            if (_isLocalMode)
                return await LocalDataStore.GetCellCollectDic(version);
            return await WebApiClient.CellCollectDic(version);
        }

        public static async Task<JObject> GetLedgerValidateDic(int version = 0)
        {
            if (_isLocalMode)
                return await LocalDataStore.GetLedgerValidateDic(version);
            return await WebApiClient.LedgerValidateDic(version);
        }

        /// <summary>
        /// 获取标准科目字典（Server 分支失败/未部署时回退本地 config 文件）
        /// </summary>
        public static async Task<JObject> GetStandardAccountDic(int version = 0)
        {
            if (_isLocalMode)
                return await LocalDataStore.GetStandardAccountDic(version);
            try
            {
                var result = await WebApiClient.StandardAccountDic(version);
                if (result != null && result.TryGetValue("Accounts", out _))
                    return result;
            }
            catch { }
            // 服务器未部署/失败时回退本地 config 文件
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "StandardAccountDic.json");
            if (File.Exists(configPath))
                return JObject.Parse(File.ReadAllText(configPath));
            return new JObject { ["Version"] = 0, ["Accounts"] = new JArray() };
        }

        public static async Task<IEnumerable<Auditai.DTO.User>> GetTeamUsersWithPic()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetTeamUsersWithPic();
            return await WebApiClient.GetTeamUsersWithPic();
        }

        public static async Task<JObject> GetUserTeams()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetUserTeams();
            var jarray = await WebApiClient.GetUserTeams();
            return new JObject { ["teams"] = jarray };
        }

        public static async Task DeleteProject(Guid projectId)
        {
            if (_isLocalMode)
                await LocalDataStore.DeleteProject(projectId);
            else
                await WebApiClient.DeleteProject(projectId);
        }

        /// <summary>
        /// 删除模板（本地模式：删除 .db 文件；远程模式：调用 WebApi）
        /// </summary>
        public static async Task DeleteTemplate(Guid templateId)
        {
            if (_isLocalMode)
                await LocalDataStore.DeleteTemplate(templateId);
            else
                await WebApiClient.DeleteProject(templateId);
        }

        /// <summary>
        /// 获取模板 DTO（本地模式：从 Data\Templates 读取；远程模式：调用 WebApi）
        /// </summary>
        public static async Task<Auditai.DTO.Project> GetTemplateById(Guid templateId)
        {
            if (_isLocalMode)
                return await LocalDataStore.GetTemplateById(templateId);
            else
                return await WebApiClient.GetProjectDto(templateId);
        }

        /// <summary>
        /// 更新模板信息（本地模式：更新 .db 文件；远程模式：调用 WebApi）
        /// </summary>
        public static async Task UpdateTemplate(Auditai.DTO.Project template)
        {
            if (_isLocalMode)
                await LocalDataStore.UpdateTemplate(template);
            else
                await WebApiClient.UpdateProject(template);
        }

        /// <summary>
        /// 复制模板（本地模式：复制 .db 文件；远程模式：调用 WebApi）
        /// </summary>
        public static async Task DuplicateTemplate(Guid sourceTemplateId, Auditai.DTO.Project newTemplate)
        {
            if (_isLocalMode)
                await LocalDataStore.DuplicateTemplate(sourceTemplateId, newTemplate);
            else
            {
                JObject jObj = new JObject();
                jObj["OldProject"] = sourceTemplateId;
                jObj["NewProject"] = JToken.FromObject(newTemplate);
                jObj["ClearPermissions"] = false;
                await WebApiClient.DuplicateProject(jObj);
            }
        }

        /// <summary>
        /// 将项目另存为模板（本地模式：复制项目 .db 到 Data\Templates；远程模式：调用 WebApi）
        /// </summary>
        public static async Task SaveProjectAsTemplate(Guid sourceProjectId, Auditai.DTO.Project newTemplate)
        {
            if (_isLocalMode)
                await LocalDataStore.SaveProjectAsTemplate(sourceProjectId, newTemplate);
            else
            {
                JObject jObj = new JObject();
                jObj["OldProject"] = sourceProjectId;
                jObj["NewProject"] = JToken.FromObject(newTemplate);
                jObj["ClearPermissions"] = true;
                await WebApiClient.SaveProjectAsTemplate(jObj);
            }
        }

        public static async Task DeleteProjectFromServer(JObject jobj)
        {
            if (_isLocalMode)
                await LocalDataStore.DeleteProjectFromServer(jobj);
            else
                await WebApiClient.DeleteProjectFromServer(jobj);
        }

        public static async Task<IEnumerable<Auditai.DTO.Project>> GetRecycleProjects()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetRecycleProjects() ?? Enumerable.Empty<Auditai.DTO.Project>();
            return await WebApiClient.GetRecycleProjects() ?? Enumerable.Empty<Auditai.DTO.Project>();
        }

        public static async Task RestoreProjects(JObject jobj)
        {
            if (_isLocalMode)
                await LocalDataStore.RestoreProjects(jobj);
            else
                await WebApiClient.RestoreProjects(jobj);
        }

        public static async Task UploadFile(Guid fileId, Stream fileStream)
        {
            if (_isLocalMode)
                await LocalDataStore.UploadFile(fileId, fileStream);
            else
                await WebApiClient.UploadFile(fileId, fileStream);
        }

        public static async Task<Stream> DownloadFile(Guid fileId)
        {
            if (_isLocalMode)
                return await LocalDataStore.DownloadFile(fileId);
            return await WebApiClient.DownloadFile(fileId);
        }
    }
}
