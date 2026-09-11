﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Auditai.DTO;
using Auditai.Model;
using User = Auditai.DTO.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditai.Util;

public static class WebApiClient
{
	private static string _version;

	private const string WaitingServerTaskEndKeyword = "WaitingTaskEnd:";

	private const string BadRequestMessageKeyWord = "BadRequestMessage:";

	private const string baseAddress = "https://Auditai.com:8957/api/";

	private static readonly HttpClient httpClient;

	private static readonly HttpClient _ossClient;

	private static HttpClientHandler _handler;

	public const string MACHINECODE = "MachineCode";

	public const string VALIDATECODE = "ValidateCode";

	public const string USERNAME = "UserName";

	public const string USERID = "userId";

	public const string TOKEN = "Token";

	public const string UPDATETOKEN = "UpdateToken";

	public const string PHONE = "Phone";

	public const string PROCESSID = "ProcessId";

	private const string ApplicationProcessName = "AuditAI";

	/// <summary>本地模式标志，由宿主程序（如 StorageRouter.Initialize）设置</summary>
	public static bool IsLocalMode { get; set; }

	/// <summary>本地模式 API 处理器，由宿主程序设置。参数为 API URL，返回响应 Stream</summary>
	public static Func<string, Task<Stream>> LocalApiHandler { get; set; }

	/// <summary>本地模式项目操作处理器，由宿主程序设置</summary>
	public static Func<Task<IEnumerable<Project>>> LocalGetProjectsHandler { get; set; }

	/// <summary>本地模式模板操作处理器</summary>
	public static Func<Task<IEnumerable<Project>>> LocalGetTemplatesHandler { get; set; }

	/// <summary>本地模式创建项目处理器</summary>
	public static Func<Project, Task> LocalCreateProjectHandler { get; set; }

	/// <summary>本地模式打开项目处理器</summary>
	public static Func<Guid, Task<Tuple<int, int>>> LocalOpenProjectHandler { get; set; }

	/// <summary>本地模式删除项目处理器</summary>
	public static Func<Guid, Task> LocalDeleteProjectHandler { get; set; }

	/// <summary>本地模式从服务器删除项目处理器</summary>
	public static Func<JObject, Task> LocalDeleteProjectFromServerHandler { get; set; }

	/// <summary>本地模式 PushTable 处理器</summary>
	public static Func<PushTable, Task<JObject>> LocalPushTableHandler { get; set; }

	/// <summary>本地模式 PushDocument 处理器</summary>
	public static Func<PushDocument, Task<JObject>> LocalPushDocumentHandler { get; set; }

	/// <summary>本地模式 TableCollectDic 处理器</summary>
	public static Func<int, Task<JObject>> LocalTableCollectDicHandler { get; set; }

	/// <summary>本地模式 CellCollectDic 处理器</summary>
	public static Func<int, Task<JObject>> LocalCellCollectDicHandler { get; set; }

	/// <summary>本地模式 LedgerValidateDic 处理器</summary>
	public static Func<int, Task<JObject>> LocalLedgerValidateDicHandler { get; set; }

	/// <summary>本地模式 StandardAccountDic 处理器</summary>
	public static Func<int, Task<JObject>> LocalStandardAccountDicHandler { get; set; }

	/// <summary>本地模式获取团队成员（含头像）处理器</summary>
	public static Func<Task<IEnumerable<User>>> LocalGetTeamUsersWithPicHandler { get; set; }

	/// <summary>本地模式获取用户团队处理器</summary>
	public static Func<Task<JObject>> LocalGetUserTeamsHandler { get; set; }

	/// <summary>本地模式上传文件处理器</summary>
	public static Func<Guid, Stream, Task> LocalUploadFileHandler { get; set; }

	/// <summary>本地模式下载文件处理器</summary>
	public static Func<Guid, Task<Stream>> LocalDownloadFileHandler { get; set; }

	public static string AppVersion => _version ?? (_version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "1.0.0.0");

	static WebApiClient()
	{
		_handler = new HttpClientHandler
		{
			UseDefaultCredentials = true
		};
		ServicePointManager.DefaultConnectionLimit = 10;
#if DEBUG
		// 开发环境临时跳过证书验证（生产环境需配置有效证书）
#pragma warning disable SCS0004
		ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
#pragma warning restore SCS0004
#endif
		httpClient = HttpClientFactory.Create(_handler, new CompressionHandler(), new TimeoutHandler());
		string appServer = ConfigurationManager.AppSettings["AppServer"];
		if (!string.IsNullOrWhiteSpace(appServer))
		{
			httpClient.BaseAddress = new Uri(appServer);
		}
		else
		{
			httpClient.BaseAddress = new Uri("https://Auditai.com:8957/api/");
		}
		httpClient.Timeout = Timeout.InfiniteTimeSpan;
		httpClient.DefaultRequestHeaders.TransferEncodingChunked = true;
		_ossClient = new HttpClient();
	}

	public static void SetBaseAddress(string address)
	{
		httpClient.BaseAddress = new Uri(address);
	}

	public static Uri BaseAddress => httpClient.BaseAddress;

	public static async Task<IEnumerable<Project>> GetProjects()
	{
		if (IsLocalMode)
		{
			return LocalGetProjectsHandler != null ? await LocalGetProjectsHandler() : Enumerable.Empty<Project>();
		}
		return await SendAsObject<IEnumerable<Project>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetProjects",
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		}) ?? Enumerable.Empty<Project>();
	}

	public static async Task<IEnumerable<Project>> GetTeamPayedProjects(Guid teamId)
	{
		if (IsLocalMode)
		{
			return LocalGetProjectsHandler != null ? await LocalGetProjectsHandler() : Enumerable.Empty<Project>();
		}
		return await SendAsObject<IEnumerable<Project>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetTeamPayedProjects?teamId=" + teamId.ToString("D"),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? Enumerable.Empty<Project>();
	}

	public static async Task<IEnumerable<Project>> GetRecycleProjects()
	{
		if (IsLocalMode)
		{
			return Enumerable.Empty<Project>();
		}
		return await SendAsObject<IEnumerable<Project>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetRecycleProjects",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? Enumerable.Empty<Project>();
	}

	public static async Task RestoreProjects(JObject jobj)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/RestoreProjects",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = jobj
		});
	}

	public static async Task DeleteProjectFromServer(JObject jobj)
	{
		if (IsLocalMode)
		{
			if (LocalDeleteProjectFromServerHandler != null) await LocalDeleteProjectFromServerHandler(jobj);
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/DeleteProjectFromServer",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = jobj
		});
	}

	#region 上报审核与归档

	/// <summary>上报审核：提交审核单（1~3 级顺序审批），成功后项目审核状态置为"审批中"</summary>
	public static async Task<ReviewSubmissionDto> SubmitReview(ReviewSubmitRequest req)
	{
		if (IsLocalMode)
		{
			return null;
		}
		return await SendAsObject<ReviewSubmissionDto>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Review/Submit",
			Body = req,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>获取审核单列表：scope=pending 待我审批 / mine 我提交的 / all 本团队全部；projectId 为 null 时不带该参数</summary>
	public static async Task<List<ReviewSubmissionDto>> GetReviewSubmissions(string scope, string projectId = null)
	{
		if (IsLocalMode)
		{
			return new List<ReviewSubmissionDto>();
		}
		string url = "Review/GetSubmissions?scope=" + scope;
		if (!string.IsNullOrEmpty(projectId))
		{
			url = url + "&projectId=" + projectId;
		}
		return await SendAsObject<List<ReviewSubmissionDto>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = url,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new List<ReviewSubmissionDto>();
	}

	/// <summary>稽核检查页：读取项目级校验规则清单与节点名映射（项目未打开时的数据源）</summary>
	public static async Task<ValidationRuleSetDto> GetProjectValidations(Guid projectId)
	{
		if (IsLocalMode)
		{
			return null;
		}
		return await SendAsObject<ValidationRuleSetDto>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetProjectValidations?projectId=" + projectId,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>稽核检查页：批量保存（UPSERT）项目级校验规则，服务端递增版本并记录变更历史</summary>
	public static async Task SaveProjectValidations(Guid projectId, IEnumerable<ValidationFormula> formulas)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/SaveProjectValidations",
			Body = new JObject
			{
				["ProjectId"] = projectId.ToString(),
				["Formulas"] = JArray.FromObject(formulas)
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>稽核检查页：批量删除项目级校验规则（按 Id）</summary>
	public static async Task DeleteProjectValidations(Guid projectId, IEnumerable<long> ids)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/DeleteProjectValidations",
			Body = new JObject
			{
				["ProjectId"] = projectId.ToString(),
				["Ids"] = JArray.FromObject(ids)
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>审批通过（仅当前节点审批人），最后一级通过后项目审核状态置为"已通过"</summary>
	public static async Task ApproveReview(string submissionId, string comment)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Review/Approve",
			Body = new JObject
			{
				["submissionId"] = submissionId,
				["comment"] = comment
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>审批退回（仅当前节点审批人），整体退回提交人，重新上报产生新一轮记录</summary>
	public static async Task RejectReview(string submissionId, string comment)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Review/Reject",
			Body = new JObject
			{
				["submissionId"] = submissionId,
				["comment"] = comment
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>撤回审核单（仅提交人且所有节点均未审批）</summary>
	public static async Task WithdrawReview(string submissionId)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Review/Withdraw",
			Body = new JObject
			{
				["submissionId"] = submissionId
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>获取项目全部轮次审核单（含节点明细，供审批记录查看）</summary>
	public static async Task<List<ReviewSubmissionDto>> GetReviewHistory(string projectId)
	{
		if (IsLocalMode)
		{
			return new List<ReviewSubmissionDto>();
		}
		return await SendAsObject<List<ReviewSubmissionDto>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Review/GetHistory?projectId=" + projectId,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new List<ReviewSubmissionDto>();
	}

	/// <summary>获取审批流程模板列表；includeDisabled=true 供管理界面查看停用项（仅 TeamAdmin 生效）</summary>
	public static async Task<List<ReviewFlowTemplateDto>> GetReviewFlowTemplates(bool includeDisabled = false)
	{
		if (IsLocalMode)
		{
			return new List<ReviewFlowTemplateDto>();
		}
		string url = "ReviewFlow/GetTemplates";
		if (includeDisabled)
		{
			url = url + "?includeDisabled=1";
		}
		return await SendAsObject<List<ReviewFlowTemplateDto>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = url,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new List<ReviewFlowTemplateDto>();
	}

	/// <summary>保存审批流程模板（新建 id 为空/更新 id 非空，仅 TeamAdmin），返回保存后的模板</summary>
	public static async Task<ReviewFlowTemplateDto> SaveReviewFlowTemplate(ReviewFlowTemplateDto template)
	{
		if (IsLocalMode)
		{
			return null;
		}
		return await SendAsObject<ReviewFlowTemplateDto>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "ReviewFlow/SaveTemplate",
			Body = template,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>删除审批流程模板（仅 TeamAdmin）</summary>
	public static async Task DeleteReviewFlowTemplate(string templateId)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "ReviewFlow/DeleteTemplate",
			Body = new JObject
			{
				["id"] = templateId
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>启用/停用审批流程模板（仅 TeamAdmin；仅翻转状态，不做节点校验）</summary>
	public static async Task SetReviewFlowTemplateEnabled(string templateId, int enabled)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "ReviewFlow/SetTemplateEnabled",
			Body = new JObject
			{
				["id"] = templateId,
				["enabled"] = enabled
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>归档项目（需审核已通过且未归档），返回归档记录；归档后项目锁定、禁止数据推送</summary>
	public static async Task<ProjectArchiveDto> ArchiveProject(string projectId, string note)
	{
		if (IsLocalMode)
		{
			return null;
		}
		return await SendAsObject<ProjectArchiveDto>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Archive/Submit",
			Body = new JObject
			{
				["projectId"] = projectId,
				["note"] = note
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>获取可归档项目列表（审核已通过且未归档）</summary>
	public static async Task<List<Project>> GetArchiveCandidates()
	{
		if (IsLocalMode)
		{
			return new List<Project>();
		}
		return await SendAsObject<List<Project>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Archive/GetCandidates",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new List<Project>();
	}

	/// <summary>获取本团队归档列表</summary>
	public static async Task<List<ProjectArchiveDto>> GetArchives()
	{
		if (IsLocalMode)
		{
			return new List<ProjectArchiveDto>();
		}
		return await SendAsObject<List<ProjectArchiveDto>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Archive/GetArchives",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new List<ProjectArchiveDto>();
	}

	/// <summary>取消归档（仅 TeamAdmin），项目恢复可编辑，保留"已通过"状态可再次归档</summary>
	public static async Task CancelArchive(string projectId)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Archive/Cancel",
			Body = new JObject
			{
				["projectId"] = projectId
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	#endregion

	public static async Task<IEnumerable<Project>> GetTemplates()
	{
		if (IsLocalMode)
		{
			return LocalGetTemplatesHandler != null ? await LocalGetTemplatesHandler() : Enumerable.Empty<Project>();
		}
		var result = await SendAsObject<IEnumerable<Project>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetTemplates",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
		return result ?? Enumerable.Empty<Project>();
	}

	public static async Task<Project> GetProjectDto(Guid projectId)
	{
		if (IsLocalMode)
		{
			var projects = LocalGetProjectsHandler != null ? await LocalGetProjectsHandler() : Enumerable.Empty<Project>();
			return projects.FirstOrDefault(p => p.Id == projectId);
		}
		return await SendAsObject<Project>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/GetProjectDto",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new
			{
				ProjectId = projectId
			}
		});
	}

	public static async Task<JArray> GetUserTeams()
	{
		if (IsLocalMode)
		{
			var result = LocalGetUserTeamsHandler != null ? await LocalGetUserTeamsHandler() : new JObject();
			return result["teams"] as JArray ?? new JArray();
		}
		JObject resp = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetUserTeams",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
		return resp?.GetValue("teams") as JArray ?? new JArray();
	}

	public static async Task<JObject> UpdateCurrentTeam(Guid teamId)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/UpdateCurrentTeam",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new
			{
				TeamId = teamId
			}
		});
	}

	public static async Task<long> AddUserGroup(string groupName, long parentId = 0L)
	{
		if (IsLocalMode)
		{
			return 0L;
		}
		return await SendAsObject<long>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/AddUserGroup",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { groupName, parentId }
		});
	}

	public static async Task<JObject> MoveUserToGroup(string userId, string groupId)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/MoveUserToGroup",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { userId, groupId }
		});
	}

	public static async Task<bool> DeleteUserGroup(long groupId)
	{
		if (IsLocalMode)
		{
			return true;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/DeleteUserGroup",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { groupId }
		});
	}

	public static async Task<bool> RenameUserGroup(long groupId, string groupName)
	{
		if (IsLocalMode)
		{
			return true;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/RenameUserGroup",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { groupId, groupName }
		});
	}

	public static async Task<bool> UpdateJobTitle(long userId, string jobTitle)
	{
		if (IsLocalMode)
		{
			return true;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/UpdateJobTitle",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { userId, jobTitle }
		});
	}

	public static async Task TeamMergeRequest(JObject jobj)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/TeamMergeRequest",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = jobj
		});
	}

	public static async Task<bool> AllowTeamMerge(string desManagerId)
	{
		if (IsLocalMode)
		{
			return true;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/AllowTeamMerge",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new
			{
				desUserId = desManagerId
			}
		});
	}

	public static async Task UpdateTeamName(string teamName)
	{
		if (IsLocalMode)
		{
			return;
		}
		await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/UpdateTeamName",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { teamName }
		});
	}

	public static async Task UpdateProject(Project project)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/UpdateProject",
			Body = project,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task UpdateProjectMembers(Project project)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/UpdateProjectMembers",
			Body = project,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task DuplicateProject(JObject dupInfo)
	{
		if (IsLocalMode)
		{
			return;
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/DuplicateProject",
			Body = dupInfo,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 1f, null);
		long taskId = (long?)jObject["taskId"] ?? 0;
		await WaitingServerTaskRunOver(taskId, serverProgressUpdater);
	}

	public static async Task SaveProjectAsTemplate(JObject info)
	{
		if (IsLocalMode)
		{
			return;
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/SaveProjectAsTemplate",
			Body = info,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
	}

	public static async Task ShareProject(JObject dupInfo)
	{
		if (IsLocalMode)
		{
			return;
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/ShareProject",
			Body = dupInfo,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 1f, null);
		// 修复：服务端返回 null 时 jObject["taskId"] 抛 NRE。
		long taskId = (long?)jObject?["taskId"] ?? 0;
		await WaitingServerTaskRunOver(taskId, serverProgressUpdater);
	}

	public static async Task CreateProject(Project project)
	{
		if (IsLocalMode)
		{
			if (LocalCreateProjectHandler != null) await LocalCreateProjectHandler(project);
			return;
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/CreateProject",
			Body = project,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 1f, null);
		// 修复：服务端返回 null 时 jObject["taskId"] 抛 NRE。
		long taskId = (long?)jObject?["taskId"] ?? 0;
		await WaitingServerTaskRunOver(taskId, serverProgressUpdater);
	}

	public static async Task DeleteProject(Guid projectId)
	{
		if (IsLocalMode)
		{
			if (LocalDeleteProjectHandler != null) await LocalDeleteProjectHandler(projectId);
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/DeleteProject",
			Body = projectId,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task<JObject> UpdateProjectVersion(JObject project)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/UpdateProjectVersion",
			Body = project,
			Timeout = TimeSpan.FromMinutes(3.0),
			WithAuthorization = true
		});
	}

	public static async Task<JObject> PushProject(Guid projectId, JObject project, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new JObject { ["Result"] = "ok" };
		}
		int nodesCount = project["Nodes"]?.Count() ?? 0;
		int vfsCount = project["VFs"]?.Count() ?? 0;
		if (nodesCount + vfsCount <= 500)
		{
			return await SendAsObject<JObject>(new RequestOptions
			{
				Method = HttpMethod.Post,
				Url = "Project/PushProjectQuick",
				Body = project,
				Timeout = TimeSpan.FromMinutes(3.0),
				WithAuthorization = true
			});
		}
		return await SendAsPushProject(projectId, project, new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PushProject",
			Timeout = TimeSpan.FromMinutes(3.0),
			WithAuthorization = true
		}, reportCallback);
	}

	public static async Task<JObject> PullProject(JObject request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PullProject",
			Body = request,
			Timeout = TimeSpan.FromMinutes(3.0),
			WithAuthorization = true
		});
		// 修复：服务端返回 null 时 jObject["Result"] 抛 NRE。
		if (jObject == null)
		{
			return null;
		}
		string text = (string)jObject["Result"];
		if (text == null || !text.StartsWith("WaitingTaskEnd:"))
		{
			return jObject;
		}
		long taskId = (long?)jObject["taskId"] ?? 0;
		string taskResultFileDownloadUrl = (string)jObject["url"];
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 0.8f, reportCallback);
		TaskProgressValueUpdater downloadProcessProgressUpdater = new TaskProgressValueUpdater(0.8f, 0.2f, reportCallback);
		string responseString = string.Empty;
		await WaitingServerTaskRunOver(taskId, taskResultFileDownloadUrl, serverProgressUpdater, delegate(ServerTaskResultFileStreamReader reader)
		{
			responseString = reader.ReadString();
			downloadProcessProgressUpdater.UpdateProgress(1f);
		});
		return JsonConvert.DeserializeObject<JObject>(responseString);
	}

	public static async Task<JObject> PushTable(PushTable request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return LocalPushTableHandler != null ? await LocalPushTableHandler(request) : new JObject();
		}
		if (request.Cells.Count <= 1000)
		{
			return await SendAsObject<JObject>(new RequestOptions
			{
				Method = HttpMethod.Post,
				Url = "Project/PushTableQuick",
				Body = request,
				Timeout = TimeSpan.FromMinutes(5.0),
				WithAuthorization = true
			});
		}
		return await SendAsPushTable(request, new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PushTable",
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		}, reportCallback);
	}

	/// <summary>
	/// 节点级强锁：获取对指定表格的编辑锁。
	/// 客户端打开表格节点时调用，服务端返回 Success 表示获取锁成功（可编辑），
	/// 返回 Locked 表示已被其他用户占用（仅可查看，UI 应进入只读模式）。
	/// 返回 JObject: { Result: "Success", Locker: <userId> } 或 { Result: "Locked", Locker: <userId>, LockerName: <name> }
	/// </summary>
	public static async Task<JObject> AcquireTableLock(Guid projectId, long tableId)
	{
		if (IsLocalMode)
		{
			return new JObject { ["Result"] = "Success", ["Locker"] = 0L };
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/AcquireTableLock",
			Body = new { ProjectId = projectId, TableId = tableId },
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>
	/// 节点级强锁：释放对指定表格的编辑锁。
	/// 客户端切换到其他节点/关闭项目/退出应用时调用。
	/// 服务端仅当 Locker == 当前用户才释放（避免误释放他人锁）。
	/// 失败静默处理（如网络异常），不影响后续业务；服务端 30 分钟超时会自动释放。
	/// </summary>
	public static async Task ReleaseTableLock(Guid projectId, long tableId)
	{
		if (IsLocalMode)
		{
			return;
		}
		try
		{
			await SendAsObject<JObject>(new RequestOptions
			{
				Method = HttpMethod.Post,
				Url = "Project/ReleaseTableLock",
				Body = new { ProjectId = projectId, TableId = tableId },
				Timeout = TimeSpan.FromSeconds(15.0),
				WithAuthorization = true
			});
		}
		catch
		{
			// 释放锁失败不阻断业务：服务端 30 分钟超时会自动释放
		}
	}

	public static async Task<PullTable> PullTable(JObject request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new PullTable();
		}
		return await SendAsPullTable(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PullTable",
			Body = request,
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		}, reportCallback);
	}

	public static async Task<JObject> RevertTable(JObject request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/RevertTable",
			Body = request,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 1f, reportCallback);
		long taskId = (long?)jObject["taskId"] ?? 0;
		return JsonConvert.DeserializeObject<JObject>(await WaitingServerTaskRunOver(taskId, serverProgressUpdater));
	}

	public static async Task<PullDocument> RevertDocument(JObject request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new PullDocument();
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/RevertDocument",
			Body = request,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
		long taskId = (long?)jObject["taskId"] ?? 0;
		string taskResultFileDownloadUrl = (string)jObject["url"];
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 0.8f, reportCallback);
		TaskProgressValueUpdater downloadProcessProgressUpdater = new TaskProgressValueUpdater(0.8f, 0.2f, reportCallback);
		PullDocument pullDocument = new PullDocument();
		await WaitingServerTaskRunOver(taskId, taskResultFileDownloadUrl, serverProgressUpdater, delegate(ServerTaskResultFileStreamReader reader)
		{
			reader.ReadDataBlock(pullDocument);
			downloadProcessProgressUpdater.UpdateProgress(1f);
		});
		return pullDocument;
	}

	public static async Task<PullTable> GetTableRevertDiff(JObject request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new PullTable();
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/GetTableRevertDiff",
			Body = request,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
		long taskId = (long?)jObject["taskId"] ?? 0;
		string taskResultFileDownloadUrl = (string)jObject["url"];
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 0.8f, reportCallback);
		TaskProgressValueUpdater downloadProcessProgressUpdater = new TaskProgressValueUpdater(0.8f, 0.2f, reportCallback);
		PullTable pullTable = new PullTable();
		await WaitingServerTaskRunOver(taskId, taskResultFileDownloadUrl, serverProgressUpdater, delegate(ServerTaskResultFileStreamReader reader)
		{
			reader.ReadDataBlock(pullTable);
			downloadProcessProgressUpdater.UpdateProgress(1f);
		});
		return pullTable;
	}

	public static async Task<PullDocument> GetDocumentRevertDiff(JObject request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new PullDocument();
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/GetDocumentRevertDiff",
			Body = request,
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
		long taskId = (long?)jObject["taskId"] ?? 0;
		string taskResultFileDownloadUrl = (string)jObject["url"];
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 0.8f, reportCallback);
		TaskProgressValueUpdater downloadProcessProgressUpdater = new TaskProgressValueUpdater(0.8f, 0.2f, reportCallback);
		PullDocument pullDocument = new PullDocument();
		await WaitingServerTaskRunOver(taskId, taskResultFileDownloadUrl, serverProgressUpdater, delegate(ServerTaskResultFileStreamReader reader)
		{
			reader.ReadDataBlock(pullDocument);
			downloadProcessProgressUpdater.UpdateProgress(1f);
		});
		return pullDocument;
	}

	public static async Task<JArray> GetTableTimeline(JObject request)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/GetTableTimeline",
			Body = request,
			Timeout = TimeSpan.FromMinutes(1.0),
			WithAuthorization = true
		}) ?? new JArray();
	}

	public static async Task<JArray> GetDocumentTimeline(JObject request)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/GetDocumentTimeline",
			Body = request,
			Timeout = TimeSpan.FromMinutes(1.0),
			WithAuthorization = true
		}) ?? new JArray();
	}

	public static async Task<JArray> QueryTableVersions(JObject request)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/QueryTableVersions",
			Body = request,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new JArray();
	}

	public static async Task<JArray> QueryDocumentVersions(JObject request)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/QueryDocumentVersions",
			Body = request,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new JArray();
	}

	public static async Task<JArray> QueryImageVersions(JObject request)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/QueryImageVersions",
			Body = request,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new JArray();
	}

	public static async Task<JArray> QueryPdfVersions(JObject request)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/QueryPdfVersions",
			Body = request,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new JArray();
	}

	public static async Task<JArray> GetTableColumns(JObject request)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/GetTableColumns",
			Body = request,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? new JArray();
	}

	public static async Task<JObject> PushDocument(PushDocument request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return LocalPushDocumentHandler != null ? await LocalPushDocumentHandler(request) : new JObject();
		}
		if (request.CalculateSize() <= 1048576)
		{
			return await SendAsObject<JObject>(new RequestOptions
			{
				Method = HttpMethod.Post,
				Url = "Project/PushDocumentQuick",
				Body = request,
				Timeout = TimeSpan.FromMinutes(5.0),
				WithAuthorization = true
			});
		}
		return await SendAsPushDocument(request, new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PushDocument",
			Body = request,
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		}, reportCallback);
	}

	public static async Task<PullDocument> PullDocument(JObject request, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return new PullDocument();
		}
		return await SendAsPullDocument(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PullDocument",
			Body = request,
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		}, reportCallback);
	}

	public static async Task<JObject> PushImage(JObject request)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PushImage",
			Body = request,
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		});
	}

	public static async Task<JObject> PullImage(JObject request)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PullImage",
			Body = request,
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		});
	}

	public static async Task<JObject> PushPdf(JObject request)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PushPdf",
			Body = request,
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		});
	}

	public static async Task<JObject> PullPdf(JObject request)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/PullPdf",
			Body = request,
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		});
	}

	public static async Task<Tuple<int, int>> OpenProject(Guid projectId)
	{
		if (IsLocalMode)
		{
			return LocalOpenProjectHandler != null ? await LocalOpenProjectHandler(projectId) : Tuple.Create(0, 0);
		}
		JObject jObject = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"Project/OpenProject?projectId={projectId}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
		return Tuple.Create((int?)jObject["OperationId"] ?? 0, (int?)jObject["ServerVersion"] ?? 0);
	}

	public static async Task<JObject> CreateTeam(string teamName, int type)
	{
		if (IsLocalMode)
		{
			return new JObject { ["Id"] = Guid.NewGuid().ToString() };
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/CreateTeam",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { teamName, type }
		});
	}

	public static async Task CreateDemo()
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/CreateDemo",
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		});
	}

	public static async Task<User> AddUserToTeam(string userName)
	{
		if (IsLocalMode)
		{
			return new User { Id = 0, Name = userName, UserName = userName };
		}
		return await SendAsObject<User>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/AddUserToTeam",
			Body = new
			{
				UserName = userName
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task RemoveUserFromTeam(string userName, Guid? teamId = null)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/RemoveUserFromTeam",
			Body = new
			{
				UserName = userName,
				TeamId = teamId
			},
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task<bool> DismissTeam()
	{
		if (IsLocalMode)
		{
			return true;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/DismissTeam",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task<IEnumerable<User>> GetTeamUsers()
	{
		if (IsLocalMode)
		{
			return LocalGetTeamUsersWithPicHandler != null ? await LocalGetTeamUsersWithPicHandler() : Enumerable.Empty<User>();
		}
		return await SendAsObject<IEnumerable<User>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetTeamUsers",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		}) ?? Enumerable.Empty<User>();
	}

	public static async Task<IEnumerable<User>> GetTeamUsersWithPic()
	{
		if (IsLocalMode)
		{
			return LocalGetTeamUsersWithPicHandler != null ? await LocalGetTeamUsersWithPicHandler() : Enumerable.Empty<User>();
		}
		List<User> users = new List<User>();
		var response = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetTeamUsersWithPic",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
		JToken usersToken = response?["users"];
		if (usersToken != null && usersToken.Type == JTokenType.Array)
		{
			foreach (var item in usersToken.Select((JToken c) => new
			{
				Id = c.Value<long>("Id"),
				UserName = c.Value<string>("UserName"),
				Name = c.Value<string>("Name"),
				Picture = c["Picture"],
				TeamId = c.Value<string>("TeamId"),
				Email = c.Value<string>("Email"),
				Company = c.Value<string>("Company"),
				Sex = c.Value<string>("Sex"),
				Phone = c.Value<string>("Phone"),
				City = c.Value<string>("City"),
				GroupId = c.Value<long?>("groupId"),
				JobTitle = c.Value<string>("jobTitle"),
				IsTeamAdmin = c.Value<bool>("IsTeamAdmin"),
				Permissions = c.Value<string>("Permissions")
			}))
			{
				string text = item.Picture?.Value<string>();
				try
				{
					// 修复：原 Convert.FromBase64String 无逐条 try/catch，一条头像非法
					// Base64 会让整个用户列表加载失败。坏行跳过，不影响其余用户。
					byte[] picture = (text == null) ? null : Convert.FromBase64String(text);
					users.Add(new User
					{
						Id = item.Id,
						UserName = item.UserName,
						Name = item.Name,
						Picture = picture,
						TeamId = Guid.TryParse(item.TeamId, out var tid) ? tid : Guid.Empty,
						Email = item.Email,
						Company = item.Company,
						Sex = item.Sex,
						Phone = item.Phone,
						City = item.City,
						GroupId = item.GroupId,
						JobTitle = item.JobTitle,
						IsTeamAdmin = item.IsTeamAdmin,
						Permissions = UserTeamPermissions.Deserialize(item.Permissions)
					});
				}
				catch (FormatException)
				{
					// 头像 Base64 非法，跳过该用户头像
				}
				catch (ArgumentException)
				{
					// 头像 Base64 非法（含非法字符），跳过该用户头像
				}
			}
		}
		return users;
	}

	public static async Task<IEnumerable<User>> GetTeamUserPermissions(JObject jobj)
	{
		if (IsLocalMode)
		{
			return Enumerable.Empty<User>();
		}
		var response = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/GetTeamUserPermissions",
			Body = jobj,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
		JToken usersToken = response?["Users"];
		if (usersToken != null && usersToken.Type == JTokenType.Array)
		{
			List<User> permissionsUsers = new List<User>();
			foreach (JToken j in usersToken)
			{
				// 防御：服务端 userId 缺失/类型不符时，(long) 直接强转会抛 InvalidCastException
				// 导致整个权限列表加载失败。改为 TryGetValue 安全提取，异常行跳过。
				long id = 0L;
				JToken idToken = j?["userId"];
				if (idToken == null || !long.TryParse(idToken.ToString(), out id))
				{
					continue;
				}
				permissionsUsers.Add(new User
				{
					Id = id,
					Name = (string)j["Name"],
					Permissions = UserTeamPermissions.Deserialize((string)j["Permissions"])
				});
			}
			return permissionsUsers;
		}
		return Enumerable.Empty<User>();
	}

	public static async Task<IEnumerable<User>> GetProjectUsersWithPic(Guid projectId)
	{
		if (IsLocalMode)
		{
			return LocalGetTeamUsersWithPicHandler != null ? await LocalGetTeamUsersWithPicHandler() : Enumerable.Empty<User>();
		}
		List<User> users = new List<User>();
		var response = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"Project/GetProjectUsersWithPic?projectId={projectId}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
		JToken usersToken = response?["users"];
		if (usersToken != null && usersToken.Type == JTokenType.Array)
		{
			foreach (var item in usersToken.Select((JToken c) => new
			{
				Id = c.Value<long>("Id"),
				UserName = c.Value<string>("UserName"),
				Name = c.Value<string>("Name"),
				Picture = c["Picture"],
				TeamId = c.Value<string>("TeamId"),
				Email = c.Value<string>("Email"),
				Company = c.Value<string>("Company"),
				Sex = c.Value<string>("Sex"),
				Phone = c.Value<string>("Phone"),
				City = c.Value<string>("City"),
				UserRole = (UserRole)c.Value<int>("Role")
			}))
			{
				string text = item.Picture?.Value<string>();
				try
				{
					// 修复：同 GetTeamUsersWithPic，单条头像非法 Base64 不拖垮整个项目成员列表。
					byte[] picture = (text == null) ? null : Convert.FromBase64String(text);
					users.Add(new User
					{
						Id = item.Id,
						UserName = item.UserName,
						Name = item.Name,
						Picture = picture,
						TeamId = Guid.TryParse(item.TeamId, out var tid) ? tid : Guid.Empty,
						Email = item.Email,
						Company = item.Company,
						Sex = item.Sex,
						Phone = item.Phone,
						City = item.City,
						Role = item.UserRole
					});
				}
				catch (FormatException)
				{
					// 头像 Base64 非法，跳过该用户头像
				}
				catch (ArgumentException)
				{
					// 头像 Base64 非法（含非法字符），跳过该用户头像
				}
			}
		}
		return users;
	}

	public static async Task<Tuple<List<User>, List<UserGroup>>> GetTeamUserGroups()
	{
		if (IsLocalMode)
		{
			return Tuple.Create(new List<User>(), new List<UserGroup>());
		}
		List<UserGroup> userGroups = new List<UserGroup>();
		JObject resp = await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetTeamUserGroups",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
		JToken userGroupsToken = resp?["userGroups"];
		if (userGroupsToken != null)
		{
			foreach (JToken item in userGroupsToken)
			{
				long id = item.Value<long>("id");
				string name = item.Value<string>("name");
				long? parentId = item.Value<long?>("parentid");
				userGroups.Add(new UserGroup
				{
					Id = id,
					Name = name,
					ParentId = parentId
				});
			}
		}
		// 修复：原 ToDictionary 在服务端返回重复组 Id 时抛 ArgumentException。
		// 改为 GroupBy 取每组第一条，保证不崩溃且结果可用。
		Dictionary<long, UserGroup> userGroupMap = userGroups.GroupBy((UserGroup g) => g.Id).Select((IGrouping<long, UserGroup> g) => g.First()).ToDictionary((UserGroup g) => g.Id, (UserGroup g) => g);
		List<UserGroup> topUserGroup = new List<UserGroup>();
		foreach (UserGroup item2 in userGroups)
		{
			if (!item2.ParentId.HasValue || item2.ParentId <= 0)
			{
				topUserGroup.Add(item2);
				continue;
			}
			// 父组 id 可能指向不存在的组（服务器数据不一致），用 TryGetValue 防御 KeyNotFoundException
			if (userGroupMap.TryGetValue(item2.ParentId.Value, out var parentGroup))
			{
				item2.ParentGroup = parentGroup;
				item2.ParentGroup.Children.Add(item2);
			}
			else
			{
				topUserGroup.Add(item2);
			}
		}
		IEnumerable<User> enumerable = await GetTeamUsersWithPic();
		List<User> list = new List<User>();
		foreach (User item3 in enumerable)
		{
			if (!item3.GroupId.HasValue || item3.GroupId <= 0)
			{
				list.Add(item3);
				continue;
			}
			// 同上：用户所属组 id 可能不存在，用 TryGetValue 防御
			if (userGroupMap.TryGetValue(item3.GroupId.Value, out var userGroup2))
			{
				item3.UserGroup = userGroup2;
				userGroup2.Users.Add(item3);
			}
			else
			{
				list.Add(item3);
			}
		}
		return Tuple.Create(list, topUserGroup);
	}

	public static async Task<IEnumerable<Project>> GetProjectDescendants(Guid projectId)
	{
		if (IsLocalMode)
		{
			return Enumerable.Empty<Project>();
		}
		return await SendAsObject<IEnumerable<Project>>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/GetProjectDescendants",
			Body = new
			{
				ProjectId = projectId
			},
			Timeout = TimeSpan.FromMinutes(5.0),
			WithAuthorization = true
		}) ?? Enumerable.Empty<Project>();
	}

	public static async Task<Tuple<Stream, int>> PullProjectDirect(Guid projectId, TaskProgressValueReportCallback reportCallback = null)
	{
		if (IsLocalMode)
		{
			return Tuple.Create(new MemoryStream() as Stream, 0);
		}
		RequestOptions options = new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"Project/PullProjectDirect?projectId={projectId}",
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true
		};
		JObject respObj = await SendAsObject<JObject>(options);
		long taskId = (long?)respObj?["taskId"] ?? 0;
		TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 1f, reportCallback);
		JObject jObject = JsonConvert.DeserializeObject<JObject>(await WaitingServerTaskRunOver(taskId, serverProgressUpdater));
		string requestUri = jObject?.Value<string>("Url");
		if (string.IsNullOrEmpty(requestUri))
		{
			throw new InvalidOperationException("服务器未返回下载 URL");
		}
		// _ossClient 没有认证头，服务端 DownloadPullProjectDirect 会返回 401。
		// 改用带认证头的 httpClient 发送请求（完整 URL 会覆盖 BaseAddress）。
		HttpRequestMessage downloadReq = new HttpRequestMessage(HttpMethod.Get, requestUri);
		if (TokenTimer.LoginInfo != null)
		{
			downloadReq.Headers.TryAddWithoutValidation("UserId", TokenTimer.LoginInfo.userId.ToString());
		}
		if (TokenTimer.Token != null)
		{
			downloadReq.Headers.TryAddWithoutValidation("Token", TokenTimer.Token.TokenValue);
		}
		HttpResponseMessage downloadResp = await httpClient.SendAsync(downloadReq, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(continueOnCapturedContext: false);
		if (!downloadResp.IsSuccessStatusCode)
		{
			string errBody = await downloadResp.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
			downloadResp.Dispose();
			throw new HttpRequestException($"下载项目数据库失败: HTTP {(int)downloadResp.StatusCode} {downloadResp.StatusCode}: {errBody}");
		}
		Stream downloadStream = await downloadResp.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
		return Tuple.Create(downloadStream, jObject.Value<int>("Length"));
	}

	public static async Task<long> Register(User userInfo, string validateCode)
	{
		if (IsLocalMode)
		{
			return 1L;
		}
		if (string.IsNullOrEmpty(userInfo.Password))
		{
			userInfo.Password = Guid.NewGuid().ToString("N");
		}
		userInfo.Password = Encrypts.SHA256Encrypt(userInfo.Password, isUrl: false);
		return await SendAsObject<long>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/Register",
			Body = userInfo,
			Timeout = TimeSpan.FromMinutes(1.0),
			WithMachineCode = true,
			ActivationCode = validateCode
		});
	}

	public static async Task<long> SingleRegister(User userInfo)
	{
		if (IsLocalMode)
		{
			return 1L;
		}
		if (string.IsNullOrEmpty(userInfo.Password))
		{
			userInfo.Password = Guid.NewGuid().ToString("N");
		}
		userInfo.Password = Encrypts.SHA256Encrypt(userInfo.Password, isUrl: false);
		return await SendAsObject<long>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/SingleRegister",
			Body = userInfo,
			Timeout = TimeSpan.FromMinutes(1.0),
			WithMachineCode = true
		});
	}

	public static async Task<bool> UpdatePicture(byte[] bytes)
	{
		if (IsLocalMode)
		{
			return true;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/UpdatePicture",
			Body = bytes,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task UpdateUserInfo(User userInfo)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/UpdateUserInfo",
			Body = userInfo,
			Timeout = TimeSpan.FromMinutes(1.0),
			WithAuthorization = true,
			WithMachineCode = true
		});
	}

	public static async Task UpdatePhoneInfo(User userInfo, string validateCode)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/UpdatePhoneInfo",
			Body = userInfo,
			Timeout = TimeSpan.FromMinutes(1.0),
			WithAuthorization = true,
			WithMachineCode = true,
			ValidationCode = validateCode
		});
	}

	public static async Task ClientQuit()
	{
		if (IsLocalMode)
		{
			TokenTimer.TokenUpdater.Stop();
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/ClientQuit",
			Timeout = TimeSpan.FromSeconds(10.0),
			WithAuthorization = true
		});
		TokenTimer.TokenUpdater.Stop();
	}

	private static bool IsProcessExist()
	{
		try
		{
			Process[] processesByName = Process.GetProcessesByName("AuditAI");
			return processesByName != null && processesByName.Length > 1;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static async Task<Tuple<UserToken, User>> AccountLogin(string userName, string hashPassword)
	{
		if (IsLocalMode)
		{
			var localTuple = Tuple.Create(new UserToken { TokenValue = "local-token" }, new User { Id = 1, Name = "管理员", UserName = "admin", Role = UserRole.Manager });
			TokenTimer.LoginInfo = new LoginInfo
			{
				userId = localTuple.Item2.Id,
				userName = localTuple.Item2.UserName,
				password = hashPassword,
				LoginMode = LoginMode.Password
			};
			TokenTimer.Token = localTuple.Item1;
			TokenTimer.TokenUpdater.Start();
			return localTuple;
		}
		RequestOptions requestOptions = new RequestOptions();
		requestOptions.Method = HttpMethod.Get;
		requestOptions.Url = $"User/AccountLogin?userName={Uri.EscapeDataString(userName)}&password={hashPassword}&version={AppVersion}&hasProcess={IsProcessExist()}&machineCode={Uri.EscapeDataString(MachineCode.Code)}";
		requestOptions.Timeout = TimeSpan.FromSeconds(30.0);
		requestOptions.WithMachineCode = true;
		requestOptions.WithMachineSign = true;
		Tuple<UserToken, User> tuple = await SendAsObject<Tuple<UserToken, User>>(requestOptions);
		if (tuple == null || tuple.Item1 == null || tuple.Item2 == null)
		{
			throw new ServerException
			{
				ExceptionMessage = "登录失败：服务器返回无效数据",
				ExceptionType = "NullResponse"
			};
		}
		LoginInfo loginInfo = new LoginInfo
		{
			userId = tuple.Item2.Id,
			userName = tuple.Item2.UserName,
			password = hashPassword,
			LoginMode = LoginMode.Password
		};
		TokenTimer.LoginInfo = loginInfo;
		TokenTimer.Token = tuple.Item1;
		TokenTimer.TokenUpdater.Start();
		return tuple;
	}

	public static async Task<Tuple<UserToken, User>> AccountLoginBySMS(string phoneNumber, string validateCode)
	{
		if (IsLocalMode)
		{
			var localTuple = Tuple.Create(new UserToken { TokenValue = "local-token" }, new User { Id = 1, Name = "管理员", UserName = "admin", Role = UserRole.Manager });
			TokenTimer.LoginInfo = new LoginInfo
			{
				userId = localTuple.Item2.Id,
				userName = localTuple.Item2.UserName,
				password = string.Empty,
				LoginMode = LoginMode.SMS
			};
			TokenTimer.Token = localTuple.Item1;
			TokenTimer.TokenUpdater.Start();
			return localTuple;
		}
		Tuple<UserToken, User> tuple = await SendAsObject<Tuple<UserToken, User>>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"User/AccountLoginBySMS?phone={Uri.EscapeDataString(phoneNumber)}&version={AppVersion}&hasProcess={IsProcessExist()}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true,
			WithMachineSign = true,
			ValidationCode = validateCode
		});
		if (tuple == null || tuple.Item1 == null || tuple.Item2 == null)
		{
			throw new ServerException
			{
				ExceptionMessage = "登录失败：服务器返回无效数据",
				ExceptionType = "NullResponse"
			};
		}
		LoginInfo loginInfo = new LoginInfo
		{
			userId = tuple.Item2.Id,
			userName = tuple.Item2.UserName,
			password = string.Empty,
			LoginMode = LoginMode.SMS,
			validateCode = validateCode
		};
		TokenTimer.LoginInfo = loginInfo;
		TokenTimer.Token = tuple.Item1;
		TokenTimer.TokenUpdater.Start();
		return tuple;
	}

	public static async Task<Tuple<UserToken, User>> SMSReLogin(string userName, string validateCode = null)
	{
		if (IsLocalMode)
		{
			var localTuple = Tuple.Create(new UserToken { TokenValue = "local-token" }, new User { Id = 1, Name = "管理员", UserName = "admin", Role = UserRole.Manager });
			TokenTimer.LoginInfo = new LoginInfo
			{
				userId = localTuple.Item2.Id,
				userName = localTuple.Item2.UserName,
				password = string.Empty,
				LoginMode = LoginMode.SMS
			};
			TokenTimer.Token = localTuple.Item1;
			TokenTimer.TokenUpdater.Start();
			return localTuple;
		}
		RequestOptions options = new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/SMSReLogin?userName=" + Uri.EscapeDataString(userName) + "&version=" + AppVersion,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true,
			WithMachineSign = true
		};
		if (!string.IsNullOrEmpty(validateCode))
		{
			options.ValidationCode = validateCode;
		}
		return await SendAsObject<Tuple<UserToken, User>>(options);
	}

	public static async Task<string> GetDeleteProjectValidateCode(string phone)
	{
		if (IsLocalMode)
		{
			return "000000";
		}
		return await SendAsObject<string>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetDeleteProjectValidateCode?phone=" + Uri.EscapeDataString(phone),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task<User> GetUserById(long userId)
	{
		if (IsLocalMode)
		{
			return new User { Id = userId, Name = "管理员", UserName = "admin" };
		}
		return await SendAsObject<User>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"User/GetUserById?userId={userId}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task<User> GetUserByName(string userName)
	{
		if (IsLocalMode)
		{
			return new User { Id = 1, Name = "管理员", UserName = userName };
		}
		return await SendAsObject<User>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetUserByName?userName=" + Uri.EscapeDataString(userName),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task<string> GetFuzzyPhone(string userName)
	{
		if (IsLocalMode)
		{
			return "138****0000";
		}
		return await SendAsObject<string>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetFuzzyPhone?userName=" + Uri.EscapeDataString(userName),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task<bool> UserNameExists(string userName)
	{
		if (IsLocalMode)
		{
			return false;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/UserNameExists?userName=" + Uri.EscapeDataString(userName),
			Timeout = TimeSpan.FromSeconds(30.0)
		});
	}

	public static async Task<bool> PhoneExists(string phone)
	{
		if (IsLocalMode)
		{
			return false;
		}
		return await SendAsObject<bool>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/PhoneExists?phone=" + Uri.EscapeDataString(phone),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	public static async Task FindPassword(string userName, string password, string validateCode)
	{
		if (IsLocalMode)
		{
			return;
		}
		// 修复 BUG: 使用 isUrl: true 使密码哈希经过 URL 编码，
		// 避免 Base64 中的 +, /, = 字符在 URL 传输时被破坏（+ 会被服务器解码为空格）
		string text = Encrypts.SHA256Encrypt(password, isUrl: true);
		await Send(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/FindPassword?userName=" + Uri.EscapeDataString(userName) + "&password=" + text,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true,
			ValidationCode = validateCode
		});
	}

	public static async Task ResetPassword(string oldPassword, string newPassword, string validateCode)
	{
		if (IsLocalMode)
		{
			return;
		}
		// 修复 BUG: 同 FindPassword，使用 isUrl: true 保证密码哈希 URL 安全
		string text = Encrypts.SHA256Encrypt(oldPassword, isUrl: true);
		string text2 = Encrypts.SHA256Encrypt(newPassword, isUrl: true);
		await Send(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/ResetPassword?oldPassword=" + text + "&newPassword=" + text2,
			Timeout = TimeSpan.FromSeconds(30.0),
			ValidationCode = validateCode,
			WithAuthorization = true,
			WithMachineCode = true
		});
	}

	public static async Task ResetPasswordWithoutSMS(string oldPassword, string newPassword)
	{
		if (IsLocalMode)
		{
			return;
		}
		// 修复 BUG: 同 FindPassword，使用 isUrl: true 保证密码哈希 URL 安全
		string text = Encrypts.SHA256Encrypt(oldPassword, isUrl: true);
		string text2 = Encrypts.SHA256Encrypt(newPassword, isUrl: true);
		await Send(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/ResetPasswordWithoutSMS?oldPassword=" + text + "&newPassword=" + text2,
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			WithMachineCode = true
		});
	}

	public static async Task GetValidateCode(string phone, string smsTemplate)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetValidateCode?phone=" + Uri.EscapeDataString(phone) + "&smsTemplate=" + Uri.EscapeDataString(smsTemplate),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true
		});
	}

	public static async Task GetCodeByName(string userName, string smsTemplate)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetCodeByName?userName=" + Uri.EscapeDataString(userName) + "&smsTemplate=" + Uri.EscapeDataString(smsTemplate),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true
		});
	}

	public static async Task<string> GetUsernameByPhone(string phone)
	{
		if (IsLocalMode)
		{
			return "admin";
		}
		return await SendAsObject<string>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetUsernameByPhone?phone=" + Uri.EscapeDataString(phone),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			WithMachineCode = true
		});
	}

	public static async Task<string> GetUsernameByEmail(string email)
	{
		if (IsLocalMode)
		{
			return "admin";
		}
		return await SendAsObject<string>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetUsernameByEmail?email=" + Uri.EscapeDataString(email),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			WithMachineCode = true
		});
	}

	public static async Task GetValidateCodeByEmail(string email)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/GetValidateCodeByEmail?email=" + Uri.EscapeDataString(email),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true
		});
	}

	public static async Task<UserToken> UpdateToken(long userId)
	{
		if (IsLocalMode)
		{
			return new UserToken { TokenValue = "local-token" };
		}
		return await SendAsObject<UserToken>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "User/UpdateToken",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			IsUpdateToken = true,
			WithMachineCode = true
		});
	}

	public static async Task<JObject> TableCollectDic(int version)
	{
		if (IsLocalMode)
		{
			return LocalTableCollectDicHandler != null ? await LocalTableCollectDicHandler(version) : new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"DataSource/TableCollectDic?version={version}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true,
			WithAuthorization = true
		});
	}

	public static async Task<JObject> CellCollectDic(int version)
	{
		if (IsLocalMode)
		{
			return LocalCellCollectDicHandler != null ? await LocalCellCollectDicHandler(version) : new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"DataSource/CellCollectDic?version={version}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true,
			WithAuthorization = true
		});
	}

	public static async Task<JObject> LedgerValidateDic(int version)
	{
		if (IsLocalMode)
		{
			return LocalLedgerValidateDicHandler != null ? await LocalLedgerValidateDicHandler(version) : new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"DataSource/LedgerValidateDic?version={version}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true,
			WithAuthorization = true
		});
	}

	public static async Task<JObject> StandardAccountDic(int version)
	{
		if (IsLocalMode)
		{
			return LocalStandardAccountDicHandler != null ? await LocalStandardAccountDicHandler(version) : new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = $"DataSource/StandardAccountDic?version={version}",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithMachineCode = true,
			WithAuthorization = true
		});
	}

	public static async Task UploadFile(Guid fileId, Stream stream, Guid projectId = default, string fileName = "file.bin")
	{
		if (IsLocalMode)
		{
			if (LocalUploadFileHandler != null) await LocalUploadFileHandler(fileId, stream);
			return;
		}
		string query = $"?projectId={(projectId == Guid.Empty ? "" : projectId.ToString())}&fileName={Uri.EscapeDataString(fileName ?? "file.bin")}";
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/UploadFile" + query,
			Timeout = TimeSpan.FromMinutes(10.0),
			WithAuthorization = true,
			Body = stream,
			FileId = fileId
		});
	}

	/// <summary>
	/// 服务端模式导入项目：将解压后的 project.db 二进制流 + 元信息（QueryString）上传到服务端，
	/// 服务端创建 Projects 主库记录并落盘 .db 文件。
	/// 返回服务端创建的 Project DTO。
	/// </summary>
	public static async Task<Project> ImportProject(
		Stream dbStream,
		string name,
		string number,
		string category,
		string note,
		string auditee,
		DateTime createTime,
		int schemaVersion)
	{
		// QueryString 传递元信息（与 UploadFile 风格一致：Body 为裸二进制流）
		var query = $"?name={Uri.EscapeDataString(name ?? "")}" +
			$"&number={Uri.EscapeDataString(number ?? "")}" +
			$"&category={Uri.EscapeDataString(category ?? "")}" +
			$"&note={Uri.EscapeDataString(note ?? "")}" +
			$"&auditee={Uri.EscapeDataString(auditee ?? "")}" +
			$"&createTime={Uri.EscapeDataString(createTime.ToString("o"))}" +
			$"&schemaVersion={schemaVersion}";

		return await SendAsObject<Project>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/ImportProject" + query,
			Timeout = TimeSpan.FromMinutes(10.0),
			WithAuthorization = true,
			Body = dbStream
		});
	}

	/// <summary>
	/// 服务端模式导入模板：将解压后的 template.db 二进制流 + 元信息（QueryString）上传到服务端，
	/// 服务端创建 Projects 主库记录（Type=1, IsTemplate=1）并落盘到 Templates/ 目录 + seed 主库 TreeGroup/TreeNode。
	/// 返回服务端创建的 Project DTO。
	/// 与 ImportProject 的差异：去掉 auditee 参数（模板不需要），增加 teamVisible 参数；路由改为 Project/ImportTemplate。
	/// </summary>
	public static async Task<Project> ImportTemplate(
		Stream dbStream,
		string name,
		string number,
		string category,
		string note,
		DateTime createTime,
		int schemaVersion,
		bool teamVisible)
	{
		// QueryString 传递元信息（与 ImportProject 风格一致：Body 为裸二进制流）
		var query = $"?name={Uri.EscapeDataString(name ?? "")}" +
			$"&number={Uri.EscapeDataString(number ?? "")}" +
			$"&category={Uri.EscapeDataString(category ?? "")}" +
			$"&note={Uri.EscapeDataString(note ?? "")}" +
			$"&createTime={Uri.EscapeDataString(createTime.ToString("o"))}" +
			$"&schemaVersion={schemaVersion}" +
			$"&teamVisible={teamVisible}";

		return await SendAsObject<Project>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/ImportTemplate" + query,
			Timeout = TimeSpan.FromMinutes(10.0),
			WithAuthorization = true,
			Body = dbStream
		});
	}

	public static async Task<Stream> DownloadFile(Guid fileId)
	{
		if (IsLocalMode)
		{
			return LocalDownloadFileHandler != null ? await LocalDownloadFileHandler(fileId) : new MemoryStream();
		}
		return await SendAsStream(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/DownloadFile",
			Timeout = TimeSpan.FromMinutes(10.0),
			WithAuthorization = true,
			FileId = fileId
		});
	}

	public static async Task SetUserTeamPermissions(User user)
	{
		if (IsLocalMode)
		{
			return;
		}
		await Send(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/SetUserTeamPermissions",
			Timeout = TimeSpan.FromSeconds(10.0),
			WithAuthorization = true,
			Body = user
		});
	}

	/// <summary>激活 License（绑定当前机器）</summary>
	public static async Task<JObject> ActivateLicense(string licenseKey, string machineCode)
	{
		if (IsLocalMode)
		{
			return new JObject { ["activated"] = true };
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "License/Activate",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { licenseKey, machineCode }
		});
	}

	/// <summary>查询当前用户 License 状态</summary>
	public static async Task<JObject> GetLicenseStatus()
	{
		if (IsLocalMode)
		{
			return new JObject { ["isExpired"] = false, ["inGracePeriod"] = false, ["daysRemaining"] = 365 };
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "License/Status",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	/// <summary>续费 License</summary>
	public static async Task<JObject> RenewLicense(int licenseId, int years)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "License/Renew",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { licenseId, years }
		});
	}

	/// <summary>批量导入用户到团队</summary>
	public static async Task<JObject> BatchImportUsers(Guid teamId, JArray users)
	{
		if (IsLocalMode)
		{
			return new JObject { ["successCount"] = 0, ["failedCount"] = 0 };
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "User/BatchImport",
			Timeout = TimeSpan.FromMinutes(2.0),
			WithAuthorization = true,
			Body = new { teamId = teamId.ToString("D"), users }
		});
	}

	/// <summary>邀请用户加入团队</summary>
	public static async Task<JObject> InviteUser(Guid teamId, string phone, string email, int role = 0)
	{
		if (IsLocalMode)
		{
			return new JObject();
		}
		return await SendAsObject<JObject>(new RequestOptions
		{
			Method = HttpMethod.Post,
			Url = "Project/InviteUser",
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true,
			Body = new { teamId = teamId.ToString("D"), phone, email, role }
		});
	}

	/// <summary>获取待接受邀请列表</summary>
	public static async Task<JArray> GetPendingInvitations(Guid teamId)
	{
		if (IsLocalMode)
		{
			return new JArray();
		}
		return await SendAsObject<JArray>(new RequestOptions
		{
			Method = HttpMethod.Get,
			Url = "Project/GetPendingInvitations?teamId=" + teamId.ToString("D"),
			Timeout = TimeSpan.FromSeconds(30.0),
			WithAuthorization = true
		});
	}

	#region SendAsStream
	/// <summary>401 已触发过一次自动重新登录的标志（进程内仅触发一次，防重入）</summary>
	private static int _reloginTriggered;

	private static async Task<Stream> SendAsStream(RequestOptions options)
	{
		string url = options.Url ?? "";
		DebugLog($"[SendAsStream] START url={url} method={options.Method} localMode={IsLocalMode} hasLocalHandler={LocalApiHandler != null}");

		// ★ 本地模式：优先使用 LocalApiHandler
		if (LocalApiHandler != null)
		{
			DebugLog($"[SendAsStream] using LocalApiHandler for url={url}");
			return await LocalApiHandler(url);
		}

		// ★ 服务器模式：发送实际 HTTP 请求
		try
		{
			HttpRequestMessage request = GetRequest(options);
			DebugLog($"[SendAsStream] sending HTTP request to: {request.RequestUri}");
			HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(continueOnCapturedContext: false);
			DebugLog($"[SendAsStream] response: {(int)response.StatusCode} {response.StatusCode}");

			// 检查响应状态码
			if (!response.IsSuccessStatusCode)
			{
				string errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
				DebugLog($"[SendAsStream] ERROR response body: {errorContent}");
				// 先构造错误消息再 Dispose，避免访问已释放对象的属性
				// 服务端错误体格式为 {"error":"forbidden","message":"无权访问该项目"}，优先提取 message 给用户看
				string msg;
				try
				{
					var errObj = JsonConvert.DeserializeObject<JObject>(errorContent);
					string errMessage = errObj?["message"]?.ToString();
					msg = string.IsNullOrEmpty(errMessage)
						? $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}"
						: $"HTTP {(int)response.StatusCode}: {errMessage}";
				}
				catch
				{
					msg = $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}";
				}
				if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					// Token 失效：防重入触发一次自动重新登录（不重放当前请求，客户端下次调用自然成功）。
					// Message 保留 "401/Unauthorized" 关键字，兼容 TokenUpdater 的失效检测逻辑。
					if (Interlocked.CompareExchange(ref _reloginTriggered, 1, 0) == 0)
					{
						DebugLog("[SendAsStream] 401 received, triggering one-shot auto relogin");
						_ = Task.Run(async () =>
						{
							try { await ReloginForTokenUpdate(); }
							catch { }
						});
					}
					msg = "登录状态已失效，请重新登录（HTTP 401 Unauthorized）";
				}
				response.Dispose();
				throw new HttpRequestException(msg);
			}

			return await response.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex)
		{
			DebugLog($"[SendAsStream] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
			throw;
		}
	}
	#endregion

	/// <summary>临时调试日志（写文件），联调完成后移除。仅 DEBUG 编译写入，避免生产环境数据外泄/磁盘膨胀。</summary>
	private static void DebugLog(string message)
	{
#if DEBUG
		try
		{
			string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
			if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
			string logFile = Path.Combine(logPath, "debug_apicalls.txt");
			File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
		}
		catch { }
#endif
	}

	/// <summary>Token 更新失败时的自动重新登录入口（供 TokenUpdater 调用）</summary>
	public static async Task ReloginForTokenUpdate()
	{
		await Relogin();
	}

	private static async Task Relogin()
	{
		LoginInfo loginInfo = TokenTimer.LoginInfo;
		Tuple<UserToken, User> tuple = null;
		switch (loginInfo.LoginMode)
		{
		case LoginMode.Password:
			tuple = await AccountLogin(loginInfo.userName, loginInfo.password);
			break;
		case LoginMode.SMS:
			tuple = await SMSReLogin(loginInfo.userName, loginInfo.validateCode);
			break;
		}
		// tuple/Item1 可能为 null（如 LoginMode 不在枚举中或服务器返回无效数据）
		if (tuple != null && tuple.Item1 != null)
		{
			TokenTimer.Token = tuple.Item1;
		}
	}

	private static HttpRequestMessage GetRequest(RequestOptions options)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(options.Method, options.Url);
		httpRequestMessage.SetTimeout(options.Timeout);
		if (options.WithAuthorization)
		{
			if (TokenTimer.LoginInfo != null && TokenTimer.LoginInfo.userId > 0)
			{
				httpRequestMessage.Headers.TryAddWithoutValidation("UserId", TokenTimer.LoginInfo.userId.ToString());
			}
			if (TokenTimer.Token != null && !string.IsNullOrEmpty(TokenTimer.Token.TokenValue))
			{
				httpRequestMessage.Headers.TryAddWithoutValidation("Token", TokenTimer.Token.TokenValue);
			}
			else
			{
				DebugLog($"[GetRequest] WARNING: TokenValue is null/empty, request will likely get 401. url={options.Url}");
			}
		}
		if (options.WithMachineCode)
		{
			httpRequestMessage.Headers.TryAddWithoutValidation("MachineCode", MachineCode.Code);
		}
		if (options.WithMachineSign)
		{
			string value = TokenTimer.Token.Cookie?.Get("cookie_machine_sign")?.ToString();
			httpRequestMessage.Headers.TryAddWithoutValidation("cookie_machine_sign", value);
		}
		if (options.ValidationCode != null)
		{
			httpRequestMessage.Headers.TryAddWithoutValidation("ValidateCode", options.ValidationCode);
		}
		if (options.ActivationCode != null)
		{
			httpRequestMessage.Headers.TryAddWithoutValidation("ActivationCode", options.ActivationCode);
		}
		if (options.FileId.HasValue)
		{
			httpRequestMessage.Headers.TryAddWithoutValidation("FileId", options.FileId.Value.ToString());
		}
		if (options.Method == HttpMethod.Post && options.Body != null)
		{
			httpRequestMessage.Content = new PushStreamContent(async delegate(Stream s, HttpContent httpContent, TransportContext context)
			{
				object body = options.Body;
				if (body is Stream stream)
				{
					await stream.CopyToAsync(s);
					stream.Close();
					s.Close();
					return;
				}
				if (options.Body is Google.Protobuf.IMessage message)
				{
					using CodedOutputStream output = new CodedOutputStream(s);
					message.WriteTo(output);
					return;
				}
				JsonSerializer jsonSerializer = new JsonSerializer();
				using StreamWriter textWriter = new StreamWriter(s);
				using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
				jsonSerializer.Serialize(jsonWriter, options.Body);
			});
		}
		return httpRequestMessage;
	}

	private static async Task Send(RequestOptions options)
	{
		await SendAsStream(options);
	}

	private static async Task<T> SendAsObject<T>(RequestOptions options)
	{
		try
		{
			Stream stream = await SendAsStream(options).ConfigureAwait(continueOnCapturedContext: false);
			JsonSerializer jsonSerializer = new JsonSerializer();
			using StreamReader reader = new StreamReader(stream);
			string responseText = await reader.ReadToEndAsync().ConfigureAwait(continueOnCapturedContext: false);

			T result;
			using (JsonTextReader reader2 = new JsonTextReader(new StringReader(responseText)))
			{
				result = jsonSerializer.Deserialize<T>(reader2);
			}

			if (result == null && typeof(T) != typeof(string))
			{
				if (typeof(T).IsInterface && typeof(T).IsGenericType)
				{
					Type genericTypeDef = typeof(T).GetGenericTypeDefinition();
					if (genericTypeDef == typeof(IEnumerable<>) || genericTypeDef == typeof(IList<>) || genericTypeDef == typeof(ICollection<>))
					{
						Type elementType = typeof(T).GetGenericArguments()[0];
						Type listType = typeof(List<>).MakeGenericType(elementType);
						using (JsonTextReader reader3 = new JsonTextReader(new StringReader(responseText)))
						{
							object listResult = jsonSerializer.Deserialize(reader3, listType);
							if (listResult != null)
							{
								return (T)listResult;
							}
						}
						return (T)(object)Activator.CreateInstance(listType);
					}
				}

				try
				{
					var errorObj = JsonConvert.DeserializeObject<JObject>(responseText);
					if (errorObj != null && errorObj.ContainsKey("error"))
					{
						string errorMessage = errorObj["error"]?.ToString() ?? "请求失败";
						string message = errorObj["message"]?.ToString();
						if (!string.IsNullOrEmpty(message))
						{
							errorMessage = message;
						}
						throw new ServerException
						{
							ExceptionMessage = errorMessage,
							ExceptionType = "ServerError"
						};
					}
				}
				catch (ServerException) { throw; }
				catch (JsonReaderException) { /* 响应非合法 JSON（如 HTML 错误页），交给 result 兜底 */ }
				catch (JsonSerializationException) { /* 同上 */ }
			}

			return result;
		}
		catch (IOException ex)
		{
			throw new HttpRequestException(ex.Message, ex);
		}
		catch (JsonSerializationException ex)
		{
			throw new ServerException
			{
				ExceptionMessage = "服务器返回数据格式错误: " + ex.Message,
				ExceptionType = "JsonSerializationError",
				ExceptionStackTrace = ex.StackTrace
			};
		}
		catch (JsonReaderException ex)
		{
			// 修复：主反序列化遇到非法 JSON（如服务器返回 HTML 错误页）时，
			// 原实现只捕获 IOException/JsonSerializationException，JsonReaderException
			// 会裸抛给上层。此处统一转为 ServerException 友好错误。
			throw new ServerException
			{
				ExceptionMessage = "服务器返回数据格式错误: " + ex.Message,
				ExceptionType = "JsonReaderError",
				ExceptionStackTrace = ex.StackTrace
			};
		}
	}

	private static async Task<JObject> SendAsPushProject(Guid projectId, JObject request, RequestOptions options, TaskProgressValueReportCallback progressReportCallback)
	{
		try
		{
			TaskProgressValueUpdater dataGenerateProgressUpdater = new TaskProgressValueUpdater(0f, 0.2f, progressReportCallback);
			TaskProgressValueUpdater fileUploadProgressUpdater = new TaskProgressValueUpdater(0.2f, 0.1f, progressReportCallback);
			TaskProgressValueUpdater serverProcessProgressUpdater = new TaskProgressValueUpdater(0.3f, 0.7f, progressReportCallback);
			long taskId = await SendTaskInputFileToServer(fileUploadProgressUpdater, delegate(ServerTaskInputFileStreamWriter streamWriter)
			{
				string value = JsonConvert.SerializeObject(request);
				streamWriter.WriteString(value);
				dataGenerateProgressUpdater.UpdateProgress(1f);
			}).ConfigureAwait(continueOnCapturedContext: false);
			options.Method = HttpMethod.Get;
			options.Body = null;
			options.Url = string.Format("{0}?taskId={1}&projectId={2}", options.Url, taskId, projectId.ToString("D"));
			JObject jObject = await SendAsObject<JObject>(options);
			if (jObject == null) return new JObject();
			string text = (string)jObject["Result"];
			if (string.IsNullOrEmpty(text) || !text.StartsWith("WaitingTaskEnd:"))
			{
				return jObject;
			}
			return JsonConvert.DeserializeObject<JObject>(await WaitingServerTaskRunOver(taskId, serverProcessProgressUpdater));
		}
		catch (IOException ex)
		{
			throw new HttpRequestException(ex.Message, ex);
		}
	}

	private static async Task<JObject> SendAsPushTable(PushTable request, RequestOptions options, TaskProgressValueReportCallback progressReportCallback)
	{
		try
		{
			TaskProgressValueUpdater dataGenerateProgressUpdater = new TaskProgressValueUpdater(0f, 0.2f, progressReportCallback);
			TaskProgressValueUpdater fileUploadProgressUpdater = new TaskProgressValueUpdater(0.2f, 0.1f, progressReportCallback);
			TaskProgressValueUpdater serverProcessProgressUpdater = new TaskProgressValueUpdater(0.3f, 0.7f, progressReportCallback);
			long taskId = await SendTaskInputFileToServer(fileUploadProgressUpdater, delegate(ServerTaskInputFileStreamWriter streamWriter)
			{
				int value = request.Columns.Count((PushColumn c) => c.Action == 1);
				int value2 = request.Columns.Count((PushColumn c) => c.Action == 3);
				int value3 = request.Rows.Count((PushRow c) => c.Action == 1);
				int value4 = request.Rows.Count((PushRow c) => c.Action == 3);
				int value5 = request.Cells.Count((PushCell c) => c.Action == 1);
				int value6 = request.Cells.Count((PushCell c) => c.Action == 3);
				streamWriter.WriteCount(value);
				streamWriter.WriteCount(value2);
				streamWriter.WriteCount(value3);
				streamWriter.WriteCount(value4);
				streamWriter.WriteCount(value5);
				streamWriter.WriteCount(value6);
				PushTable pushTable = new PushTable();
				pushTable.MergeFrom(request);
				pushTable.Columns.Clear();
				pushTable.Rows.Clear();
				pushTable.Cells.Clear();
				pushTable.CellAttachments.Clear();
				pushTable.CellStyles.Clear();
				pushTable.Merges.Clear();
				streamWriter.WriteData(pushTable);
				int num = request.Columns.Count + request.Rows.Count + request.Cells.Count + request.CellStyles.Count + request.Merges.Count + request.CellAttachments.Count;
				int num2 = 0;
				streamWriter.WriteCount(num);
				int count = request.Columns.Count;
				streamWriter.WriteCount(count);
				for (int i = 0; i < count; i++)
				{
					streamWriter.WriteData(request.Columns[i]);
					dataGenerateProgressUpdater.UpdateProgress(num2++, num);
				}
				int count2 = request.Rows.Count;
				streamWriter.WriteCount(count2);
				for (int j = 0; j < count2; j++)
				{
					streamWriter.WriteData(request.Rows[j]);
					dataGenerateProgressUpdater.UpdateProgress(num2++, num);
				}
				int count3 = request.Cells.Count;
				streamWriter.WriteCount(count3);
				for (int k = 0; k < count3; k++)
				{
					streamWriter.WriteData(request.Cells[k]);
					dataGenerateProgressUpdater.UpdateProgress(num2++, num);
				}
				int count4 = request.CellStyles.Count;
				streamWriter.WriteCount(count4);
				for (int l = 0; l < count4; l++)
				{
					streamWriter.WriteData(request.CellStyles[l]);
					dataGenerateProgressUpdater.UpdateProgress(num2++, num);
				}
				int count5 = request.Merges.Count;
				streamWriter.WriteCount(count5);
				for (int m = 0; m < count5; m++)
				{
					streamWriter.WriteData(request.Merges[m]);
					dataGenerateProgressUpdater.UpdateProgress(num2++, num);
				}
				int count6 = request.CellAttachments.Count;
				streamWriter.WriteCount(count6);
				for (int n = 0; n < count6; n++)
				{
					streamWriter.WriteData(request.CellAttachments[n]);
					dataGenerateProgressUpdater.UpdateProgress(num2++, num);
				}
				if (num2 != num)
				{
					throw new NormalException("任务数据的实际个数与查询到的总数不一致！");
				}
			}).ConfigureAwait(continueOnCapturedContext: false);
			string text = new Guid(request.ProjectId.ToByteArray()).ToString("D");
			var tableIdBytes = new byte[16];
			BitConverter.GetBytes(request.Id).CopyTo(tableIdBytes, 0);
			string guidTableId = new Guid(tableIdBytes).ToString("D");
			options.Method = HttpMethod.Get;
			options.Body = null;
			options.Url = $"{options.Url}?taskId={taskId}&projectId={text}&tableId={guidTableId}&version={request.Version}";
			JObject jObject = await SendAsObject<JObject>(options);
			if (jObject == null) return new JObject();
			string text2 = (string)jObject["Result"];
			if (string.IsNullOrEmpty(text2) || !text2.StartsWith("WaitingTaskEnd:"))
			{
				return jObject;
			}
			return JsonConvert.DeserializeObject<JObject>(await WaitingServerTaskRunOver(taskId, serverProcessProgressUpdater));
		}
		catch (IOException ex)
		{
			throw new HttpRequestException(ex.Message, ex);
		}
	}

	private static async Task<PullTable> SendAsPullTable(RequestOptions options, TaskProgressValueReportCallback progressReportCallback)
	{
		try
		{
			using Stream stream = await SendAsStream(options).ConfigureAwait(continueOnCapturedContext: false);
			PullTable pullTable = Auditai.DTO.PullTable.Parser.ParseFrom(stream);
			if (string.IsNullOrEmpty(pullTable.Result) || !pullTable.Result.StartsWith("WaitingTaskEnd:"))
			{
				return pullTable;
			}
			string value = pullTable.Result.Substring("WaitingTaskEnd:".Length);
			JObject jObject = JsonConvert.DeserializeObject<JObject>(value);
			long taskId = (long?)jObject["taskId"] ?? 0;
			string taskResultFileDownloadUrl = (string)jObject["url"];
			if (string.IsNullOrEmpty(taskResultFileDownloadUrl))
			{
				// url 字段缺失时 OSS 下载会抛 ArgumentNullException，提前返回空对象。
				return new PullTable();
			}
			TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 0.8f, progressReportCallback);
			TaskProgressValueUpdater clientProcessProgressUpdater = new TaskProgressValueUpdater(0.8f, 0.2f, progressReportCallback);
			pullTable = new PullTable();
			await WaitingServerTaskRunOver(taskId, taskResultFileDownloadUrl, serverProgressUpdater, delegate(ServerTaskResultFileStreamReader fileDataReader)
			{
				fileDataReader.ReadDataBlock(pullTable);
				long num = fileDataReader.ReadCount();
				long num2 = 0L;
				long num3 = fileDataReader.ReadCount();
				for (int i = 0; i < num3; i++)
				{
					PullColumn pullColumn = new PullColumn();
					fileDataReader.ReadDataBlockWithActionType(out var actionType, pullColumn);
					switch (actionType)
					{
					case 1:
						pullTable.NewColumns.Add(pullColumn);
						break;
					case 2:
						pullTable.ModColumns.Add(pullColumn);
						break;
					case 3:
						pullTable.DelColumns.Add(pullColumn);
						break;
					default:
						throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
					}
					clientProcessProgressUpdater.UpdateProgress(num2++, num);
				}
				long num4 = fileDataReader.ReadCount();
				for (int j = 0; j < num4; j++)
				{
					PullRow pullRow = new PullRow();
					fileDataReader.ReadDataBlockWithActionType(out var actionType2, pullRow);
					switch (actionType2)
					{
					case 1:
						pullTable.NewRows.Add(pullRow);
						break;
					case 2:
						pullTable.ModRows.Add(pullRow);
						break;
					case 3:
						pullTable.DelRows.Add(pullRow);
						break;
					default:
						throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
					}
					clientProcessProgressUpdater.UpdateProgress(num2++, num);
				}
				long num5 = fileDataReader.ReadCount();
				for (int k = 0; k < num5; k++)
				{
					PullCell pullCell = new PullCell();
					fileDataReader.ReadDataBlockWithActionType(out var actionType3, pullCell);
					switch (actionType3)
					{
					case 1:
						pullTable.NewCells.Add(pullCell);
						break;
					case 2:
						pullTable.ModCells.Add(pullCell);
						break;
					case 3:
						pullTable.DelCells.Add(pullCell);
						break;
					default:
						throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
					}
					clientProcessProgressUpdater.UpdateProgress(num2++, num);
				}
				long num6 = fileDataReader.ReadCount();
				for (int l = 0; l < num6; l++)
				{
					PullCellStyle pullCellStyle = new PullCellStyle();
					fileDataReader.ReadDataBlock(pullCellStyle);
					pullTable.CellStyles.Add(pullCellStyle);
					clientProcessProgressUpdater.UpdateProgress(num2++, num);
				}
				long num7 = fileDataReader.ReadCount();
				for (int m = 0; m < num7; m++)
				{
					PullMerge pullMerge = new PullMerge();
					fileDataReader.ReadDataBlockWithActionType(out var actionType4, pullMerge);
					switch (actionType4)
					{
					case 1:
						pullTable.NewMerges.Add(pullMerge);
						break;
					case 3:
						pullTable.DelMerges.Add(pullMerge);
						break;
					default:
						throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
					}
					clientProcessProgressUpdater.UpdateProgress(num2++, num);
				}
				long num8 = fileDataReader.ReadCount();
				for (int n = 0; n < num8; n++)
				{
					PullCellProp pullCellProp = new PullCellProp();
					fileDataReader.ReadDataBlockWithActionType(out var actionType5, pullCellProp);
					switch (actionType5)
					{
					case 1:
						pullTable.NewCellProps.Add(pullCellProp);
						break;
					case 2:
						pullTable.ModCellProps.Add(pullCellProp);
						break;
					case 3:
						pullTable.DelCellProps.Add(pullCellProp);
						break;
					default:
						throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
					}
					clientProcessProgressUpdater.UpdateProgress(num2++, num);
				}
				if (num2 != num)
				{
					throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
				}
				clientProcessProgressUpdater.UpdateProgress(1f);
			});
			return pullTable;
		}
		catch (IOException ex)
		{
			throw new HttpRequestException(ex.Message, ex);
		}
	}

	private static void ClearServerTaskCacheData(long taskId)
	{
		try
		{
			_ = Send(new RequestOptions
			{
				Method = HttpMethod.Get,
				Url = "ServerTask/ClearTaskCacheData?taskId=" + taskId,
				Timeout = TimeSpan.FromSeconds(30.0),
				WithAuthorization = true
			});
		}
		catch (Exception)
		{
		}
	}

	private static void DeleteLocalFile(string filePath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
		catch
		{
		}
	}

	public static void DecompressGZipStream(string zipFilePath, string unzipFilePath)
	{
		using FileStream destination = new FileStream(unzipFilePath, FileMode.Create, FileAccess.Write);
		using GZipStream gZipStream = new GZipStream(new FileStream(zipFilePath, FileMode.Open, FileAccess.Read), CompressionMode.Decompress);
		gZipStream.CopyTo(destination);
	}

	public static void CompressToGZipStream(string srcFilePath, string zipFilePath)
	{
		using FileStream fileStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read);
		using GZipStream destination = new GZipStream(new FileStream(zipFilePath, FileMode.Create, FileAccess.Write), CompressionMode.Compress);
		fileStream.CopyTo(destination);
	}

	private static async Task<string> WaitingServerTaskRunOver(long taskId, TaskProgressValueUpdater serverProgressUpdater, bool isClearTaskServerCacheData = true)
	{
		int tryCount = 6;
		int timeOutTryTimes = tryCount;
		// 修复：原实现在每次成功轮询后把 timeOutTryTimes 重置回 6，任务未结束时
		// Task.Delay(2000) 无限轮询，服务端任务永久挂起时客户端无限等待（UI 卡死）。
		// 增加总截止时间（10 分钟），超时抛错退出。
		var deadline = DateTime.UtcNow.AddMinutes(10);
		try
		{
			while (timeOutTryTimes > 0)
			{
				if (DateTime.UtcNow > deadline)
				{
					throw new HttpRequestException("服务器任务处理超时（超过 10 分钟仍未完成），请稍后重试");
				}
				try
				{
					JObject jObject = await SendAsObject<JObject>(new RequestOptions
				{
					Method = HttpMethod.Get,
					Url = "ServerTask/GetTaskRunningStatus?taskId=" + taskId,
					Timeout = TimeSpan.FromSeconds(30.0),
					WithAuthorization = true
				}).ConfigureAwait(continueOnCapturedContext: false);
				timeOutTryTimes = tryCount;
				float progressValue = (float)jObject["progressValue"];
				bool flag = (bool)jObject["isTaskEnd"];
				bool flag2 = (bool)jObject["isTaskSuccess"];
				bool flag3 = (bool)jObject["isTimeOut"];
				string text = (string)jObject["taskDesc"];
				string result = (string)jObject["taskResult"];
					if (!flag)
					{
						serverProgressUpdater.UpdateProgress(progressValue);
						await Task.Delay(2000).ConfigureAwait(continueOnCapturedContext: false);
						continue;
					}
					if (!flag2)
					{
						timeOutTryTimes = 0;
						if (flag3)
						{
							NormalException ex = new NormalException(string.IsNullOrWhiteSpace(text) ? "服务器请求超时！" : text);
							throw new HttpRequestException(ex.Message, ex);
						}
						if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("BadRequestMessage:"))
						{
							string message = text.Substring("BadRequestMessage:".Length);
							message = StringConstBase.DePlaceHolder(message);
							NormalException ex2 = new NormalException(message);
							throw new HttpRequestException(ex2.Message, ex2);
						}
						NormalException ex3 = new NormalException(string.IsNullOrWhiteSpace(text) ? "服务器处理失败！" : text);
						throw new HttpRequestException(ex3.Message, ex3);
					}
					return result;
				}
				catch (TimeoutException ex4)
				{
					if (timeOutTryTimes > 0)
					{
						timeOutTryTimes--;
						continue;
					}
					throw new HttpRequestException(ex4.Message, ex4);
				}
				catch (NormalException ex5)
				{
					throw new HttpRequestException(ex5.Message, ex5);
				}
				catch (HttpRequestException ex6)
				{
					if (ex6.InnerException is TimeoutException && timeOutTryTimes > 0)
					{
						timeOutTryTimes--;
						continue;
					}
					throw;
				}
				catch (IOException ex7)
				{
					throw new HttpRequestException(ex7.Message, ex7);
				}
				catch (UnauthorizedAccessException ex8)
				{
					throw new HttpRequestException(ex8.Message + "(如果存在杀毒软件，建议关闭杀毒软件)", ex8);
				}
			}
		}
		finally
		{
			if (isClearTaskServerCacheData)
			{
				ClearServerTaskCacheData(taskId);
			}
		}
		TimeoutException ex9 = new TimeoutException("服务器处理失败！");
		throw new HttpRequestException(ex9.Message, ex9);
	}

	private static async Task<string> WaitingServerTaskRunOver(long taskId, string taskResultFileDownloadUrl, TaskProgressValueUpdater serverProgressUpdater, Action<ServerTaskResultFileStreamReader> taskResultParseHandle)
	{
		int tryCount = 6;
		int timeOutTryTimes = tryCount;
		// 修复：同无下载 URL 重载，增加总截止时间防止服务端任务挂起时无限轮询。
		var deadline = DateTime.UtcNow.AddMinutes(10);
		while (timeOutTryTimes > 0)
		{
			if (DateTime.UtcNow > deadline)
			{
				throw new HttpRequestException("服务器任务处理超时（超过 10 分钟仍未完成），请稍后重试");
			}
			try
			{
				JObject jObject = await SendAsObject<JObject>(new RequestOptions
				{
					Method = HttpMethod.Get,
					Url = "ServerTask/GetTaskRunningStatus?taskId=" + taskId,
					Timeout = TimeSpan.FromSeconds(30.0),
					WithAuthorization = true
				}).ConfigureAwait(continueOnCapturedContext: false);
				timeOutTryTimes = tryCount;
				float progressValue = (float)jObject["progressValue"];
				bool flag = (bool)jObject["isTaskEnd"];
				bool flag2 = (bool)jObject["isTaskSuccess"];
				bool flag3 = (bool)jObject["isTimeOut"];
				string text = (string)jObject["taskDesc"];
				if (!flag)
				{
					serverProgressUpdater.UpdateProgress(progressValue);
					await Task.Delay(2000).ConfigureAwait(continueOnCapturedContext: false);
					continue;
				}
				if (!flag2)
				{
					timeOutTryTimes = 0;
					if (flag3)
					{
						NormalException ex = new NormalException(string.IsNullOrWhiteSpace(text) ? "服务器请求超时！" : text);
						throw new HttpRequestException(ex.Message, ex);
					}
					if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("BadRequestMessage:"))
					{
						string message = text.Substring("BadRequestMessage:".Length);
						message = StringConstBase.DePlaceHolder(message);
						NormalException ex2 = new NormalException(message);
						throw new HttpRequestException(ex2.Message, ex2);
					}
					NormalException ex3 = new NormalException(string.IsNullOrWhiteSpace(text) ? "服务器处理失败！" : text);
					throw new HttpRequestException(ex3.Message, ex3);
				}
			}
			catch (TimeoutException ex4)
			{
				if (timeOutTryTimes > 0)
				{
					timeOutTryTimes--;
					continue;
				}
				throw new HttpRequestException(ex4.Message, ex4);
			}
			catch (NormalException ex5)
			{
				throw new HttpRequestException(ex5.Message, ex5);
			}
			catch (HttpRequestException ex6)
			{
				if (ex6.InnerException is TimeoutException && timeOutTryTimes > 0)
				{
					timeOutTryTimes--;
					continue;
				}
				throw;
			}
			catch (IOException ex7)
			{
				throw new HttpRequestException(ex7.Message, ex7);
			}
			catch (UnauthorizedAccessException ex8)
			{
				throw new HttpRequestException(ex8.Message + "(如果存在杀毒软件，建议关闭杀毒软件)", ex8);
			}
			break;
		}
		string tempzipFilePath = Path.GetTempFileName();
		string rawDataFilePath = Path.GetTempFileName();
		try
		{
			Stream stream2;
		HttpRequestMessage downloadReq = new HttpRequestMessage(HttpMethod.Get, taskResultFileDownloadUrl);
		if (TokenTimer.LoginInfo != null)
		{
			downloadReq.Headers.TryAddWithoutValidation("UserId", TokenTimer.LoginInfo.userId.ToString());
		}
		if (TokenTimer.Token != null)
		{
			downloadReq.Headers.TryAddWithoutValidation("Token", TokenTimer.Token.TokenValue);
		}
		HttpResponseMessage downloadResp = await httpClient.SendAsync(downloadReq, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(continueOnCapturedContext: false);
		if (!downloadResp.IsSuccessStatusCode)
		{
			string errBody = await downloadResp.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
			downloadResp.Dispose();
			throw new HttpRequestException($"下载任务结果文件失败: HTTP {(int)downloadResp.StatusCode} {downloadResp.StatusCode}: {errBody}");
		}
		stream2 = await downloadResp.Content.ReadAsStreamAsync().ConfigureAwait(continueOnCapturedContext: false);
			using (stream2)
			{
				using FileStream zipFileStream = new FileStream(tempzipFilePath, FileMode.Create, FileAccess.Write);
				await stream2.CopyToAsync(zipFileStream);
			}
			DecompressGZipStream(tempzipFilePath, rawDataFilePath);
			using (FileStream input = new FileStream(rawDataFilePath, FileMode.Open, FileAccess.Read))
			{
				using CodedInputStream codedInputStream = new CodedInputStream(input);
				taskResultParseHandle(new ServerTaskResultFileStreamReader(codedInputStream));
			}
			return null;
		}
		catch (TimeoutException ex9)
		{
			throw new HttpRequestException(ex9.Message, ex9);
		}
		catch (NormalException ex10)
		{
			throw new HttpRequestException(ex10.Message, ex10);
		}
		catch (HttpRequestException)
		{
			throw;
		}
		catch (IOException ex12)
		{
			throw new HttpRequestException(ex12.Message, ex12);
		}
		catch (UnauthorizedAccessException ex13)
		{
			throw new HttpRequestException(ex13.Message + "(如果存在杀毒软件，建议关闭杀毒软件)", ex13);
		}
		finally
		{
			DeleteLocalFile(tempzipFilePath);
			DeleteLocalFile(rawDataFilePath);
			ClearServerTaskCacheData(taskId);
		}
	}

	private static async Task<long> UploadTaskInputFileFilePartDataToServer(long taskId, FileStream dataFileStream, int offset, int dataLength, long fileLength, int timeOut)
	{
		dataFileStream.Seek(offset, SeekOrigin.Begin);
		int num = (int)((offset + dataLength > fileLength) ? (fileLength - offset) : dataLength);
		byte[] buffer = new byte[num];
		int reallyReadLength = dataFileStream.Read(buffer, 0, num);
		if (reallyReadLength != num)
		{
			throw new NormalException("文件数据上传失败!");
		}
		using (MemoryStream ms = new MemoryStream(buffer))
		{
			await Send(new RequestOptions
			{
				Method = HttpMethod.Post,
				Url = $"ServerTask/UploadTaskInputFile?taskId={taskId}&offset={offset}",
				Timeout = TimeSpan.FromSeconds(timeOut),
				Body = ms,
				WithAuthorization = true
			});
		}
		return reallyReadLength;
	}

	private static async Task<long> UploadTaskInputFile(long taskId, string filePath, TaskProgressValueUpdater fileUploadProgressUpdater)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		long fileLength = fileInfo.Length;
		int maxSendDataLength = 2097152;
		int blockCount = (int)(fileLength / maxSendDataLength);
		if (maxSendDataLength * blockCount < fileLength)
		{
			blockCount++;
		}
		using (FileStream fileInputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		{
			long num = 0L;
			int offset = 0;
			for (int i = 0; i < blockCount; i++)
			{
				fileUploadProgressUpdater.UpdateProgress(i, blockCount);
				long num2 = num;
				num = num2 + await UploadTaskInputFileFilePartDataToServer(taskId, fileInputStream, offset, maxSendDataLength, fileLength, 120);
				offset += maxSendDataLength;
			}
			if (num != fileLength)
			{
				throw new NormalException("文件数据上传失败！");
			}
			fileUploadProgressUpdater.UpdateProgress(blockCount, blockCount);
		}
		return fileLength;
	}

	private static async Task<long> SendTaskInputFileToServer(TaskProgressValueUpdater fileUploadProgressUpdater, Action<ServerTaskInputFileStreamWriter> taskInputGenerateHandle)
	{
		string tempZipFilePath = string.Empty;
		string rawDataFilePath = string.Empty;
		try
		{
			long taskId = (long)(await SendAsObject<JObject>(new RequestOptions
			{
				Method = HttpMethod.Get,
				Url = "ServerTask/GenerateTaskId",
				Timeout = TimeSpan.FromSeconds(30.0),
				WithAuthorization = true
			}).ConfigureAwait(continueOnCapturedContext: false))["taskId"];
			rawDataFilePath = Path.GetTempFileName();
			using (FileStream output = new FileStream(rawDataFilePath, FileMode.Create, FileAccess.Write))
			{
				using CodedOutputStream codedOutputStream = new CodedOutputStream(output);
				ServerTaskInputFileStreamWriter obj = new ServerTaskInputFileStreamWriter(codedOutputStream);
				taskInputGenerateHandle(obj);
			}
			tempZipFilePath = Path.GetTempFileName();
			CompressToGZipStream(rawDataFilePath, tempZipFilePath);
			await UploadTaskInputFile(taskId, tempZipFilePath, fileUploadProgressUpdater);
			return taskId;
		}
		catch (TimeoutException ex)
		{
			throw new HttpRequestException(ex.Message, ex);
		}
		catch (NormalException ex2)
		{
			throw new HttpRequestException(ex2.Message, ex2);
		}
		catch (HttpRequestException)
		{
			throw;
		}
		catch (IOException ex4)
		{
			throw new HttpRequestException(ex4.Message, ex4);
		}
		catch (UnauthorizedAccessException ex5)
		{
			throw new HttpRequestException(ex5.Message + "(如果存在杀毒软件，建议关闭杀毒软件)", ex5);
		}
		finally
		{
			DeleteLocalFile(tempZipFilePath);
			DeleteLocalFile(rawDataFilePath);
		}
	}

	private static async Task<JObject> SendAsPushDocument(PushDocument request, RequestOptions options, TaskProgressValueReportCallback progressReportCallback)
	{
		try
		{
			TaskProgressValueUpdater dataGenerateProgressUpdater = new TaskProgressValueUpdater(0f, 0.2f, progressReportCallback);
			TaskProgressValueUpdater fileUploadProgressUpdater = new TaskProgressValueUpdater(0.2f, 0.1f, progressReportCallback);
			TaskProgressValueUpdater serverProcessProgressUpdater = new TaskProgressValueUpdater(0.3f, 0.7f, progressReportCallback);
			long taskId = await SendTaskInputFileToServer(fileUploadProgressUpdater, delegate(ServerTaskInputFileStreamWriter streamWriter)
			{
				PushDocument pushDocument = new PushDocument();
				pushDocument.MergeFrom(request);
				pushDocument.Paragraphs.Clear();
				streamWriter.WriteData(pushDocument);
				int count = request.Paragraphs.Count;
				int num = 0;
				streamWriter.WriteCount(count);
				for (int i = 0; i < count; i++)
				{
					streamWriter.WriteData(request.Paragraphs[i]);
					dataGenerateProgressUpdater.UpdateProgress(num++, count);
				}
				if (num != count)
				{
					throw new NormalException("任务数据的实际个数与查询到的总数不一致！");
				}
			}).ConfigureAwait(continueOnCapturedContext: false);
			string text = new Guid(request.ProjectId.ToByteArray()).ToString("D");
			options.Method = HttpMethod.Get;
			options.Body = null;
			options.Url = $"{options.Url}?taskId={taskId}&projectId={text}&docId={request.Id}&version={request.Version}";
			JObject jObject = await SendAsObject<JObject>(options);
			if (jObject == null) return new JObject();
			string text2 = (string)jObject["Result"];
			if (string.IsNullOrEmpty(text2) || !text2.StartsWith("WaitingTaskEnd:"))
			{
				return jObject;
			}
			return JsonConvert.DeserializeObject<JObject>(await WaitingServerTaskRunOver(taskId, serverProcessProgressUpdater));
		}
		catch (IOException ex)
		{
			throw new HttpRequestException(ex.Message, ex);
		}
	}

	private static async Task<PullDocument> SendAsPullDocument(RequestOptions options, TaskProgressValueReportCallback progressReportCallback)
	{
		try
		{
			using Stream stream = await SendAsStream(options).ConfigureAwait(continueOnCapturedContext: false);
			PullDocument pullDocument = Auditai.DTO.PullDocument.Parser.ParseFrom(stream);
			if (string.IsNullOrEmpty(pullDocument.Result) || !pullDocument.Result.StartsWith("WaitingTaskEnd:"))
			{
				return pullDocument;
			}
			string value = pullDocument.Result.Substring("WaitingTaskEnd:".Length);
			JObject jObject = JsonConvert.DeserializeObject<JObject>(value);
			long taskId = (long?)jObject["taskId"] ?? 0;
			string taskResultFileDownloadUrl = (string)jObject["url"];
			if (string.IsNullOrEmpty(taskResultFileDownloadUrl))
			{
				// url 字段缺失时 OSS 下载会抛 ArgumentNullException，提前返回空对象。
				return new PullDocument();
			}
			TaskProgressValueUpdater serverProgressUpdater = new TaskProgressValueUpdater(0f, 0.8f, progressReportCallback);
			TaskProgressValueUpdater clientProcessProgressUpdater = new TaskProgressValueUpdater(0.8f, 0.2f, progressReportCallback);
			pullDocument = new PullDocument();
			await WaitingServerTaskRunOver(taskId, taskResultFileDownloadUrl, serverProgressUpdater, delegate(ServerTaskResultFileStreamReader fileDataReader)
			{
				fileDataReader.ReadDataBlock(pullDocument);
				long num = fileDataReader.ReadCount();
				long num2 = 0L;
				for (int i = 0; i < num; i++)
				{
					PullParagraph pullParagraph = new PullParagraph();
					fileDataReader.ReadDataBlockWithActionType(out var actionType, pullParagraph);
					switch (actionType)
					{
					case 1:
						pullDocument.NewParagraphs.Add(pullParagraph);
						break;
					case 2:
						pullDocument.ModParagraphs.Add(pullParagraph);
						break;
					case 3:
						pullDocument.DelParagraphs.Add(pullParagraph);
						break;
					default:
						throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
					}
					clientProcessProgressUpdater.UpdateProgress(num2++, num);
				}
				if (num2 != num)
				{
					throw new NormalException("服务器的响应数据的格式不正确，无法解析！");
				}
				clientProcessProgressUpdater.UpdateProgress(1f);
			});
			return pullDocument;
		}
		catch (IOException ex)
		{
			throw new HttpRequestException(ex.Message, ex);
		}
	}
}
