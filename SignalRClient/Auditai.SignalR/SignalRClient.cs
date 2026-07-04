﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FileTransferModel;
using Microsoft.AspNet.SignalR.Client;

namespace Auditai.SignalR;

public static class SignalRClient
{
	private const string HubAddress = "https://Auditai.com:8957/";

	private static HubConnection _hc;

	private static IHubProxy _hp;

	private static bool _callingStop;

	private static HashSet<IDisposable> _hubProxies;

	public static Dictionary<string, UserState> ColleagueState;

	public static UserState UserState { get; }

	public static event EventHandler<MessageReceivedEventArgs> MessageReceived;

	public static event EventHandler<LoginEventArgs> Logined;

	public static event EventHandler Disconnected;

	static SignalRClient()
	{
		_callingStop = false;
		_hubProxies = new HashSet<IDisposable>();
		UserState = new UserState();
		ColleagueState = new Dictionary<string, UserState>();
	}

	private static async void _hc_Closed()
	{
		if (!_callingStop)
		{
			foreach (IDisposable hp in _hubProxies)
			{
				hp.Dispose();
			}
			_hubProxies.Clear();
			SignalRClient.Disconnected?.Invoke(null, EventArgs.Empty);
			await Task.Delay(5000);
			try
			{
				await Start();
				await Login(UserState.UserId, UserState);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
				// 必须捕获所有异常：async void 中未捕获的非 Timeout/HttpRequest 异常
				// 会触发 UnobservedTaskException 导致进程崩溃。
			}
		}
		_callingStop = false;
	}

	public static void Create()
	{
		string hubAddress = "https://Auditai.com:8957/";
		string appServer = ConfigurationManager.AppSettings["AppServer"];
		if (!string.IsNullOrWhiteSpace(appServer))
		{
			hubAddress = appServer;
		}
		_hc = new HubConnection(hubAddress);
		_hp = _hc.CreateHubProxy("ChatHub");
		_hc.Closed += _hc_Closed;
	}

	public static void Create(string address)
	{
		_hc = new HubConnection(address);
		_hp = _hc.CreateHubProxy("ChatHub");
		_hc.Closed += _hc_Closed;
	}

	public static async Task Start()
	{
		if (_hc == null)
		{
			return;
		}
		if (_hc.State == ConnectionState.Disconnected)
		{
			_hubProxies.Add(_hp.On<string, string>("ReceiveFromUser", ReceiveFromUser));
			_hubProxies.Add(_hp.On<string>("PeerLogin", PeerLogin));
			_hubProxies.Add(_hp.On<string>("PeerLogout", PeerLogout));
			_hubProxies.Add(_hp.On<string, UserState>("PeerStateUpload", PeerStateUpload));
			_hubProxies.Add(_hp.On<string, string>("PeerOpensProject", PeerOpensProject));
			_hubProxies.Add(_hp.On<string, string, string>("PeerOpensTreeNode", PeerOpensTreeNode));
			_hubProxies.Add(_hp.On<string, string, string>("PeerTableCellChange", PeerTableCellChange));
			_hubProxies.Add(_hp.On<string, string, string>("PeerParagraphChange", PeerParagraphChange));
			_hubProxies.Add(_hp.On<string, string>("ProjectBroadcast", ProjectBroadcast));
			_hubProxies.Add(_hp.On<string, string>("TeamBroadcast", TeamBroadcast));
			_hubProxies.Add(_hp.On<string, string>("ProjectSynced", ProjectSynced));
			_hubProxies.Add(_hp.On<string, string>("PeerPushesTreeNode", PeerPushesTreeNode));
			_hubProxies.Add(_hp.On("PeerMemberInfoChanged", PeerMemberInfoChanged));
			_hubProxies.Add(_hp.On("PeerTeamMembersChanged", PeerTeamMembersChanged));
			_hubProxies.Add(_hp.On("PeerProjectMembersChanged", PeerProjectMembersChanged));
			_hubProxies.Add(_hp.On<string, FileSection>("PeerFileSectionArrived", PeerFileSectionArrived));
			_hubProxies.Add(_hp.On<string, string, string, string>("PeerOpenTicketNavTreeNode", PeerOpenTicketNavTreeNode));
			try
		{
			// 加 15 秒超时保护：HubConnection.Start() 在 URL 错误或服务端不响应时
			// 可能阻塞 15-90 秒（HttpClient 默认 100s + TransportConnectTimeout × 3 传输），
			// 期间 UI 会卡死。超时后让 Start 静默失败，State 保持 Disconnected，
			// 后续 Login/QueryOnlineTeam 等方法的 State 检查会立即短路返回。
			Task startTask = _hc.Start();
			if (await Task.WhenAny(startTask, Task.Delay(TimeSpan.FromSeconds(15.0))) != startTask)
			{
				// 超时：尝试停止连接以释放底层资源
				try { _hc.Stop(TimeSpan.FromSeconds(2.0)); } catch { }
			}
		}
		catch (TimeoutException)
		{
		}
		catch (HttpRequestException)
		{
		}
		catch (Exception)
		{
		}
		}
	}

	public static void Stop()
	{
		if (_hc == null)
		{
			return;
		}
		if (_hc.State == ConnectionState.Connected)
		{
			_callingStop = true;
			try
			{
				_hc.Stop(TimeSpan.FromSeconds(2.0));
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task<IEnumerable<UserState>> Login(string userId, UserState state)
	{
		if (_hc == null || _hc.State != ConnectionState.Connected)
		{
			return Enumerable.Empty<UserState>();
		}
		try
		{
			IEnumerable<UserState> ret = await _hp.Invoke<IEnumerable<UserState>>("Login", new object[2] { userId, state });
			SignalRClient.Logined?.Invoke(null, new LoginEventArgs
			{
				OnlineUsers = ret
			});
			return ret;
		}
		catch (TimeoutException)
		{
			return Enumerable.Empty<UserState>();
		}
		catch (HttpRequestException)
		{
			return Enumerable.Empty<UserState>();
		}
		catch (Exception)
		{
			return Enumerable.Empty<UserState>();
		}
	}

	public static async Task<bool> UploadState(string userId, UserState state)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				return await _hp.Invoke<bool>("UploadState", new object[2] { userId, state });
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
		return false;
	}

	public static async Task<UserState> UpdownState(string userId)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				return await _hp.Invoke<UserState>("UpdownState", new object[1] { userId });
			}
			catch (TimeoutException)
			{
				return null;
			}
			catch (HttpRequestException)
			{
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}
		return null;
	}

	public static async Task OpenProject(string projectId)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				UserState.ProjectId = projectId;
				await _hp.Invoke("OpenProject", projectId);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task SendToUser(string toId, string message)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				await _hp.Invoke("SendToUser", toId, message);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task BroadcastToProjectUsers(string message)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				await _hp.Invoke("BroadcastToProjectUsers", message);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task BroadcastToTeamUsers(string message)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				await _hp.Invoke("BroadcastToTeamUsers", message);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task OpenTreeNode(string projectId, string nodeId)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				UserState.TreeNodeId = nodeId;
				await _hp.Invoke("OpenTreeNode", projectId, nodeId);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task UpLoadTableCellId(string userId, string cellId)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				UserState.TableCellId = cellId;
				await _hp.Invoke("UpLoadTableCellId", userId, cellId);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task UploadParagraphId(string userId, string paragraphId)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				UserState.DocParagraphId = paragraphId;
				await _hp.Invoke("UploadParagraphId", userId, paragraphId);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task SyncProject(string projectId)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				await _hp.Invoke("SyncProject", projectId);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task PushTreeNodeToUser(string toId, string nodeId)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				await _hp.Invoke("PushTreeNodeToUser", toId, nodeId);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task PushFileSectionToUser(string toId, FileSection section)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				await _hp.Invoke("PushFileSectionToUser", toId, section);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	public static async Task ChangeTeamMember(string userId, string oldTeamId, string newTeamId)
	{
		if (_hc == null || _hp == null || _hc.State != ConnectionState.Connected)
		{
			return;
		}
		try
		{
			if (userId == UserState.UserId)
			{
				UserState.TeamId = newTeamId;
			}
			await _hp.Invoke("ChangeTeamMember", userId, oldTeamId, newTeamId);
		}
		catch (TimeoutException)
		{
		}
		catch (HttpRequestException)
		{
		}
		catch (Exception)
		{
		}
	}

	public static async Task ChangeProjectMember(string projectId)
	{
		if (_hc == null || _hp == null || _hc.State != ConnectionState.Connected)
		{
			return;
		}
		try
		{
			await _hp.Invoke("ChangeProjectMember", projectId);
		}
		catch (TimeoutException)
		{
		}
		catch (HttpRequestException)
		{
		}
		catch (Exception)
		{
		}
	}

	public static async Task ChangeMemberInfo(string userId)
	{
		if (_hc == null || _hp == null || _hc.State != ConnectionState.Connected)
		{
			return;
		}
		try
		{
			await _hp.Invoke("ChangeMemberInfo", userId);
		}
		catch (TimeoutException)
		{
		}
		catch (HttpRequestException)
		{
		}
		catch (Exception)
		{
		}
	}

	public static async Task<IEnumerable<UserState>> QueryOnlineTeam(string teamId)
	{
		// 必须检查 _hc.State == Connected：未连接时 _hp.Invoke 会阻塞或抛异常导致登录卡死。
		// 与 Login (line 156) 保持一致。
		if (_hc == null || _hp == null || _hc.State != ConnectionState.Connected)
		{
			return null;
		}
		try
		{
			return await _hp.Invoke<IEnumerable<UserState>>("QueryOnlineTeam", new object[1] { teamId });
		}
		catch (Exception)
		{
			// 原代码 catch when (!(ex is TimeoutException) || !(ex is HttpRequestException))
			// 是永真式（德摩根律：!(A) || !(B) == !(A && B)，而异常不可能同时是两种类型）
			// 等价于 catch (Exception)，此处简化为明确语义。
			return null;
		}
	}

	public static async Task<IEnumerable<UserState>> QueryOnlineProject(string projectId)
	{
		// 必须检查 _hc.State == Connected（同 QueryOnlineTeam）。
		if (_hc == null || _hp == null || _hc.State != ConnectionState.Connected)
		{
			return null;
		}
		try
		{
			return await _hp.Invoke<IEnumerable<UserState>>("QueryOnlineProject", new object[1] { projectId });
		}
		catch (Exception)
		{
			// 同 QueryOnlineTeam：原 catch when 是永真式，简化为 catch (Exception)。
			return null;
		}
	}

	public static async Task OpenTicketNavTreeNode(string tableId, string navTreeNodePath)
	{
		if (_hc != null && _hc.State == ConnectionState.Connected)
		{
			try
			{
				UserState.TicketNavTreeNodePath = navTreeNodePath;
				await _hp.Invoke("OpenTicketNavTreeNode", tableId, navTreeNodePath);
			}
			catch (TimeoutException)
			{
			}
			catch (HttpRequestException)
			{
			}
			catch (Exception)
			{
			}
		}
	}

	private static void ReceiveFromUser(string fromId, string message)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.MessageFromUser,
			FromId = fromId,
			Message = message
		});
	}

	private static void PeerLogin(string peerId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerLogin,
			FromId = peerId
		});
	}

	private static void PeerLogout(string peerId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerLogout,
			FromId = peerId
		});
	}

	private static void PeerStateUpload(string userId, UserState state)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerStateUpload,
			ProjectId = state.ProjectId,
			NodeId = state.TreeNodeId,
			FromId = userId
		});
	}

	private static void PeerOpensProject(string peerId, string projectId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerOpensProject,
			FromId = peerId,
			ProjectId = projectId
		});
	}

	private static void PeerOpensTreeNode(string peerId, string projectId, string nodeId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerOpensTreeNode,
			FromId = peerId,
			ProjectId = projectId,
			NodeId = nodeId
		});
	}

	private static void PeerTableCellChange(string peerId, string projectId, string cellId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerTableCellChange,
			FromId = peerId,
			ProjectId = projectId,
			TableCellId = cellId
		});
	}

	private static void PeerParagraphChange(string peerId, string projectId, string paragraphId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerParagraphChange,
			FromId = peerId,
			ProjectId = projectId,
			ParagraphId = paragraphId
		});
	}

	private static void ProjectBroadcast(string fromId, string message)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.ProjectBroadcast,
			FromId = fromId,
			Message = message
		});
	}

	private static void TeamBroadcast(string fromId, string message)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.TeamBroadcast,
			FromId = fromId,
			Message = message
		});
	}

	private static void ProjectSynced(string fromId, string projectId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.ProjectSynced,
			ProjectId = projectId,
			FromId = fromId
		});
	}

	private static void PeerPushesTreeNode(string peerId, string nodeId)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerPushesTreeNode,
			FromId = peerId,
			NodeId = nodeId
		});
	}

	private static void PeerMemberInfoChanged()
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerMemberInfoChanged
		});
	}

	private static void PeerTeamMembersChanged()
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerTeamMembersChanged
		});
	}

	private static void PeerProjectMembersChanged()
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerProjectMembersChanged
		});
	}

	private static void PeerFileSectionArrived(string fromId, FileSection section)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerFileSectionArrived,
			FromId = fromId,
			FileSection = section
		});
	}

	private static void PeerOpenTicketNavTreeNode(string peerId, string projectId, string tableId, string nodePath)
	{
		SignalRClient.MessageReceived?.Invoke(null, new MessageReceivedEventArgs
		{
			Kind = MessageKind.PeerOpenTicketNavTreeNode,
			FromId = peerId,
			ProjectId = projectId,
			NodeId = tableId,
			TicketNavTreeNodePath = nodePath
		});
	}
}
