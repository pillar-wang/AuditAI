﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.Util;
using Newtonsoft.Json.Linq;

namespace Leqisoft.LocalDataStore
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
        /// 初始化存储模式
        /// 应在 Program.Main() 的早期调用
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            string mode = ConfigurationManager.AppSettings["StorageMode"] ?? "Server";
            _isLocalMode = mode.Equals("Local", StringComparison.OrdinalIgnoreCase);

            if (_isLocalMode)
            {
                string dbPath = ConfigurationManager.AppSettings["LocalDbPath"]
                    ?? "Data/leqi_audit.db";
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
                WebApiClient.LocalGetTeamUsersWithPicHandler = LocalDataStore.GetTeamUsersWithPic;
                WebApiClient.LocalGetUserTeamsHandler = LocalDataStore.GetUserTeams;
                WebApiClient.LocalUploadFileHandler = LocalDataStore.UploadFile;
                WebApiClient.LocalDownloadFileHandler = LocalDataStore.DownloadFile;
            }
        }

        // =============================================
        // 路由方法 - 每个替换 WebApiClient 中原有的静态方法
        // =============================================

        public static async Task<IEnumerable<Leqisoft.DTO.Project>> GetProjects()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetProjects();
            return await WebApiClient.GetProjects();
        }

        public static async Task<IEnumerable<Leqisoft.DTO.Project>> GetTemplates()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetTemplates();
            return await WebApiClient.GetTemplates();
        }

        public static async Task CreateProject(Leqisoft.DTO.Project project)
        {
            if (_isLocalMode)
                await LocalDataStore.CreateProject(project);
            else
                await WebApiClient.CreateProject(project);
        }

        public static async Task UpdateProject(Leqisoft.DTO.Project project)
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

        public static async Task<IEnumerable<Leqisoft.DTO.User>> GetTeamUsersWithPic()
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
        public static async Task<Leqisoft.DTO.Project> GetTemplateById(Guid templateId)
        {
            if (_isLocalMode)
                return await LocalDataStore.GetTemplateById(templateId);
            else
                return await WebApiClient.GetProjectDto(templateId);
        }

        /// <summary>
        /// 更新模板信息（本地模式：更新 .db 文件；远程模式：调用 WebApi）
        /// </summary>
        public static async Task UpdateTemplate(Leqisoft.DTO.Project template)
        {
            if (_isLocalMode)
                await LocalDataStore.UpdateTemplate(template);
            else
                await WebApiClient.UpdateProject(template);
        }

        /// <summary>
        /// 复制模板（本地模式：复制 .db 文件；远程模式：调用 WebApi）
        /// </summary>
        public static async Task DuplicateTemplate(Guid sourceTemplateId, Leqisoft.DTO.Project newTemplate)
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
        public static async Task SaveProjectAsTemplate(Guid sourceProjectId, Leqisoft.DTO.Project newTemplate)
        {
            if (_isLocalMode)
                await LocalDataStore.SaveProjectAsTemplate(sourceProjectId, newTemplate);
            else
            {
                JObject jObj = new JObject();
                jObj["OldProject"] = sourceProjectId;
                jObj["NewProject"] = JToken.FromObject(newTemplate);
                jObj["ClearPermissions"] = true;
                await WebApiClient.DuplicateProject(jObj);
            }
        }

        public static async Task DeleteProjectFromServer(JObject jobj)
        {
            if (_isLocalMode)
                await LocalDataStore.DeleteProjectFromServer(jobj);
            else
                await WebApiClient.DeleteProjectFromServer(jobj);
        }

        public static async Task<IEnumerable<Leqisoft.DTO.Project>> GetRecycleProjects()
        {
            if (_isLocalMode)
                return await LocalDataStore.GetRecycleProjects();
            return await WebApiClient.GetRecycleProjects();
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
