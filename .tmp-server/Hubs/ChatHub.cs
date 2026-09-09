﻿﻿﻿﻿﻿﻿﻿﻿using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using AuditApiServer.Data;
using AuditApiServer.Models;
using AuditApiServer.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Hubs;

// 阶段 8：SignalR 实时协作 Hub
// 兼容客户端 Microsoft.AspNet.SignalR.Client v2 协议（Newtonsoft.Json 序列化）。
// 客户端通过 HubConnection.CreateHubProxy("ChatHub") 创建代理，
// 调用 Invoke("MethodName", args) 触发服务端方法，通过 On<T1,T2,...>("CallbackName", handler) 接收推送。
//
// 静态状态管理：使用 ConcurrentDictionary 在内存中维护在线用户（不持久化）。
// 客户端断线重连：自动 5 秒后重新调用 Login，服务端需正确处理 OnDisconnectedAsync 清理状态。
//
// 安全加固（Critical 鉴权缺失修复）：
// 1. OnConnectedAsync 从 QueryString 读取 userId+token，调用 AuthService.ValidateTokenAsync 校验；
//    失败 Context.Abort() 不抛异常，避免污染日志。
// 2. 所有 Hub 方法从 Context.Items["UserId"] 读取已鉴权 userId，不信任客户端自报。
// 3. 加入 team_/project_ 组前校验用户归属（TeamRepository.GetUserTeamsAsync / ProjectMembers 表）。
// 4. Group 名格式校验（team_{GUID} / project_{GUID}），防止注入任意 group 名。
// 5. ChangeTeamMember/ChangeProjectMember/ChangeMemberInfo 仅供服务端推送回调，拒绝客户端调用。
// 6. SendToUser/PushTreeNodeToUser/PushFileSectionToUser 校验目标与调用者在同一团队/项目。
// 7. UserStateDto 本身不含 Token/Password 等敏感字段，无需额外脱敏。
public class ChatHub : Hub
{
    // ConnectionId -> UserState
    private static readonly ConcurrentDictionary<string, UserStateDto> _onlineUsers = new();
    // UserId -> 多个 ConnectionId（同一用户可能多端登录）
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    // 保护 _userConnections 的线程安全锁（HashSet 非线程安全）
    private static readonly object _userConnectionsLock = new();

    // Group 名格式：team_{GUID} 或 project_{GUID}，GUID 支持 32 位无连字符或 36 位带连字符
    private static readonly Regex GroupNameRegex = new(
        @"^(team|project)_[a-fA-F0-9]{8}-?[a-fA-F0-9]{4}-?[a-fA-F0-9]{4}-?[a-fA-F0-9]{4}-?[a-fA-F0-9]{12}$",
        RegexOptions.Compiled);

    private const string UserIdItemKey = "UserId";

    // Task 4（审批流程通知）：供服务端（Program.cs / IHubContext<ChatHub>）按 userId 获取
    // 该用户当前全部在线连接 ID 快照（多端登录），随后可 hubContext.Clients.Client(connId)
    // 逐连接推送事件。与 SendToUser 的既有按用户推送逻辑一致（_userConnectionsLock 保护）。
    internal static IReadOnlyList<string> GetConnectionIdsForUser(string userId)
    {
        lock (_userConnectionsLock)
        {
            return _userConnections.TryGetValue(userId, out var conns)
                ? conns.ToList()
                : new List<string>();
        }
    }

    private readonly AuthService _authService;
    private readonly TeamRepository _teamRepository;
    private readonly SqliteStorage _db;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(AuthService authService, TeamRepository teamRepository, SqliteStorage db, ILogger<ChatHub> logger)
    {
        _authService = authService;
        _teamRepository = teamRepository;
        _db = db;
        _logger = logger;
    }

    // ============= 鉴权与校验辅助 =============

    /// <summary>
    /// 从 Context.Items 读取已鉴权 userId；未鉴权返回 0。
    /// </summary>
    private long GetAuthUserId()
    {
        if (Context.Items.TryGetValue(UserIdItemKey, out var v) && v is long uid) return uid;
        return 0;
    }

    /// <summary>
    /// 校验 group 名格式：team_{GUID} 或 project_{GUID}，防止注入任意 group 名。
    /// </summary>
    private static bool IsValidGroupName(string? group)
        => !string.IsNullOrEmpty(group) && GroupNameRegex.IsMatch(group);

    /// <summary>
    /// 校验用户是否属于指定团队（通过 TeamRepository.GetUserTeamsAsync）。
    /// </summary>
    private async Task<bool> IsTeamMemberAsync(long userId, string? teamId)
    {
        if (userId <= 0 || !Guid.TryParse(teamId, out var tid)) return false;
        var teams = await _teamRepository.GetUserTeamsAsync(userId);
        return teams.Any(t => t.Id == tid);
    }

    /// <summary>
    /// 校验用户是否为指定项目成员（直接查 ProjectMembers 表，不依赖 TenantContext）。
    /// </summary>
    private async Task<bool> IsProjectMemberAsync(long userId, string? projectId)
    {
        if (userId <= 0 || !Guid.TryParse(projectId, out var pid)) return false;
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM ProjectMembers WHERE ProjectId = @pid AND UserId = @uid";
        cmd.Parameters.AddWithValue("@pid", pid.ToString());
        cmd.Parameters.AddWithValue("@uid", userId);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    /// <summary>
    /// 校验两个用户是否在同一个团队或同一个项目（用于点对点推送授权）。
    /// 同一用户视为合法 peer。
    /// </summary>
    private async Task<bool> IsPeerAsync(long userId, long targetUserId)
    {
        if (userId <= 0 || targetUserId <= 0) return false;
        if (userId == targetUserId) return true;

        // 团队交集
        var myTeams = (await _teamRepository.GetUserTeamsAsync(userId)).Select(t => t.Id).ToHashSet();
        var peerTeams = await _teamRepository.GetUserTeamsAsync(targetUserId);
        if (peerTeams.Any(t => myTeams.Contains(t.Id))) return true;

        // 项目交集：直接查 ProjectMembers 自连接，避免 TenantContext 依赖
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(1) FROM ProjectMembers a
            INNER JOIN ProjectMembers b ON a.ProjectId = b.ProjectId
            WHERE a.UserId = @u1 AND b.UserId = @u2";
        cmd.Parameters.AddWithValue("@u1", userId);
        cmd.Parameters.AddWithValue("@u2", targetUserId);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    // ============= Client -> Server 方法（19 个）=============

    // 1. Login(string userId, UserState state) -> IEnumerable<UserState>
    // 客户端：_hp.Invoke<IEnumerable<UserState>>("Login", new object[] { userId, state })
    // 安全：不信任客户端自报 userId；若客户端传入 userId/state.UserId 与鉴权 userId 不一致则拒绝。
    [HubMethodName("Login")]
    public async Task<IEnumerable<UserStateDto>> LoginAsync(string userId, UserStateDto state)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return Array.Empty<UserStateDto>();
        var authUserIdStr = authUserId.ToString(CultureInfo.InvariantCulture);

        // 不信任客户端自报的 userId：若传入了且与鉴权 userId 不一致，拒绝
        if (!string.IsNullOrEmpty(userId) && userId != authUserIdStr)
        {
            _logger.LogWarning("Login 拒绝：客户端 userId={Client} 与鉴权 userId={Auth} 不一致", userId, authUserId);
            return Array.Empty<UserStateDto>();
        }
        if (!string.IsNullOrEmpty(state.UserId) && state.UserId != authUserIdStr)
        {
            _logger.LogWarning("Login 拒绝：state.UserId={Client} 与鉴权 userId={Auth} 不一致", state.UserId, authUserId);
            return Array.Empty<UserStateDto>();
        }

        var connId = Context.ConnectionId;
        state.UserId = authUserIdStr;
        state.ConnectionId = connId;

        // 校验 TeamId/ProjectId 归属，防止冒充加入任意组
        if (!string.IsNullOrEmpty(state.TeamId) && !await IsTeamMemberAsync(authUserId, state.TeamId))
        {
            _logger.LogWarning("Login 拒绝：userId={Uid} 不属于 TeamId={Tid}", authUserId, state.TeamId);
            state.TeamId = null;
        }
        if (!string.IsNullOrEmpty(state.ProjectId) && !await IsProjectMemberAsync(authUserId, state.ProjectId))
        {
            _logger.LogWarning("Login 拒绝：userId={Uid} 不属于 ProjectId={Pid}", authUserId, state.ProjectId);
            state.ProjectId = null;
        }

        _onlineUsers[connId] = state;

        // 加入用户连接映射（多端登录支持）
        lock (_userConnectionsLock)
        {
            var connections = _userConnections.GetOrAdd(authUserIdStr, _ => new HashSet<string>());
            connections.Add(connId);
        }

        // 加入团队和项目组（group 名已校验为 team_{GUID}/project_{GUID}）
        if (!string.IsNullOrEmpty(state.TeamId))
        {
            var g = $"team_{state.TeamId}";
            if (IsValidGroupName(g)) await Groups.AddToGroupAsync(connId, g);
        }
        if (!string.IsNullOrEmpty(state.ProjectId))
        {
            var g = $"project_{state.ProjectId}";
            if (IsValidGroupName(g)) await Groups.AddToGroupAsync(connId, g);
        }

        // 通知同团队/项目其他用户：PeerLogin(string peerId)
        if (!string.IsNullOrEmpty(state.TeamId))
        {
            var g = $"team_{state.TeamId}";
            if (IsValidGroupName(g))
                await Clients.GroupExcept(g, connId).SendAsync("PeerLogin", authUserIdStr);
        }
        if (!string.IsNullOrEmpty(state.ProjectId))
        {
            var g = $"project_{state.ProjectId}";
            if (IsValidGroupName(g))
                await Clients.GroupExcept(g, connId).SendAsync("PeerLogin", authUserIdStr);
        }

        // 返回当前在线用户列表（排除自己）
        return _onlineUsers.Values.Where(u => u.UserId != authUserIdStr).ToList();
    }

    // 2. OpenProject(string projectId) -> void
    [HubMethodName("OpenProject")]
    public async Task OpenProjectAsync(string projectId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        var userState = _onlineUsers.GetValueOrDefault(Context.ConnectionId);
        if (userState == null) return;

        // 校验用户是该项目的成员才能加入项目组
        if (!await IsProjectMemberAsync(authUserId, projectId))
        {
            _logger.LogWarning("OpenProject 拒绝：userId={Uid} 不属于 ProjectId={Pid}", authUserId, projectId);
            return;
        }

        var newGroup = $"project_{projectId}";
        if (!IsValidGroupName(newGroup)) return;

        // 离开旧项目组
        if (!string.IsNullOrEmpty(userState.ProjectId))
        {
            var oldGroup = $"project_{userState.ProjectId}";
            if (IsValidGroupName(oldGroup))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, oldGroup);
        }

        // 加入新项目组
        userState.ProjectId = projectId;
        await Groups.AddToGroupAsync(Context.ConnectionId, newGroup);

        // 通知同项目其他用户：PeerOpensProject(string peerId, string projectId)
        await Clients.GroupExcept(newGroup, Context.ConnectionId)
            .SendAsync("PeerOpensProject", userState.UserId, projectId);
    }

    // 3. SendToUser(string toId, string message) -> void
    [HubMethodName("SendToUser")]
    public async Task SendToUserAsync(string toId, string message)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        if (!long.TryParse(toId, out var targetUserId) || targetUserId <= 0) return;

        // 校验目标用户与调用者在同一团队/项目
        if (!await IsPeerAsync(authUserId, targetUserId))
        {
            _logger.LogWarning("SendToUser 拒绝：from={From} to={To} 无团队/项目关联", authUserId, targetUserId);
            return;
        }

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        List<string> connIds;
        lock (_userConnectionsLock)
        {
            connIds = _userConnections.GetValueOrDefault(toId)?.ToList() ?? new List<string>();
        }
        foreach (var connId in connIds)
        {
            await Clients.Client(connId).SendAsync("ReceiveFromUser", fromId, message);
        }
    }

    // 4. BroadcastToProjectUsers(string message) -> void
    [HubMethodName("BroadcastToProjectUsers")]
    public async Task BroadcastToProjectUsersAsync(string message)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        var userState = _onlineUsers.GetValueOrDefault(Context.ConnectionId);
        if (userState?.ProjectId == null) return;

        var g = $"project_{userState.ProjectId}";
        if (!IsValidGroupName(g)) return;
        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        // 客户端 On<string, string>("ProjectBroadcast", ...)
        await Clients.GroupExcept(g, Context.ConnectionId)
            .SendAsync("ProjectBroadcast", fromId, message);
    }

    // 5. BroadcastToTeamUsers(string message) -> void
    [HubMethodName("BroadcastToTeamUsers")]
    public async Task BroadcastToTeamUsersAsync(string message)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        var userState = _onlineUsers.GetValueOrDefault(Context.ConnectionId);
        if (userState?.TeamId == null) return;

        var g = $"team_{userState.TeamId}";
        if (!IsValidGroupName(g)) return;
        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        // 客户端 On<string, string>("TeamBroadcast", ...)
        await Clients.GroupExcept(g, Context.ConnectionId)
            .SendAsync("TeamBroadcast", fromId, message);
    }

    // 6. OpenTreeNode(string projectId, string nodeId) -> void
    [HubMethodName("OpenTreeNode")]
    public async Task OpenTreeNodeAsync(string projectId, string nodeId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;

        // 校验用户是该项目的成员才能向项目组广播
        if (!await IsProjectMemberAsync(authUserId, projectId))
        {
            _logger.LogWarning("OpenTreeNode 拒绝：userId={Uid} 不属于 ProjectId={Pid}", authUserId, projectId);
            return;
        }

        var g = $"project_{projectId}";
        if (!IsValidGroupName(g)) return;

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        // 客户端 On<string, string, string>("PeerOpensTreeNode", ...)
        await Clients.GroupExcept(g, Context.ConnectionId)
            .SendAsync("PeerOpensTreeNode", fromId, projectId, nodeId);
    }

    // 7. UpLoadTableCellId(string userId, string cellId) -> void
    // 安全：忽略客户端传入的 userId，统一使用鉴权 userId。
    [HubMethodName("UpLoadTableCellId")]
    public async Task UpLoadTableCellIdAsync(string userId, string cellId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        var userState = _onlineUsers.GetValueOrDefault(Context.ConnectionId);
        var projectId = userState?.ProjectId ?? "";
        if (string.IsNullOrEmpty(projectId)) return;

        var g = $"project_{projectId}";
        if (!IsValidGroupName(g)) return;

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        // 客户端 On<string, string, string>("PeerTableCellChange", ...)
        await Clients.GroupExcept(g, Context.ConnectionId)
            .SendAsync("PeerTableCellChange", fromId, projectId, cellId);
    }

    // 8. UploadParagraphId(string userId, string paragraphId) -> void
    // 安全：忽略客户端传入的 userId，统一使用鉴权 userId。
    [HubMethodName("UploadParagraphId")]
    public async Task UploadParagraphIdAsync(string userId, string paragraphId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        var userState = _onlineUsers.GetValueOrDefault(Context.ConnectionId);
        var projectId = userState?.ProjectId ?? "";
        if (string.IsNullOrEmpty(projectId)) return;

        var g = $"project_{projectId}";
        if (!IsValidGroupName(g)) return;

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        // 客户端 On<string, string, string>("PeerParagraphChange", ...)
        await Clients.GroupExcept(g, Context.ConnectionId)
            .SendAsync("PeerParagraphChange", fromId, projectId, paragraphId);
    }

    // 9. SyncProject(string projectId, string version?) -> void
    // 阶段 3：扩展 version 参数（可选，向后兼容老客户端 2 参数调用）。
    // 服务端 PushProject 成功后通过 IHubContext 主动调用时也会传入 version。
    // 客户端需升级到 On<string, string, string>("ProjectSynced", ...) 3 参数订阅。
    [HubMethodName("SyncProject")]
    public async Task SyncProjectAsync(string projectId, string? version = null)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;

        // 校验用户是该项目的成员才能向项目组广播
        if (!await IsProjectMemberAsync(authUserId, projectId))
        {
            _logger.LogWarning("SyncProject 拒绝：userId={Uid} 不属于 ProjectId={Pid}", authUserId, projectId);
            return;
        }

        var g = $"project_{projectId}";
        if (!IsValidGroupName(g)) return;

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        // 阶段 3：广播 3 参数（fromId, projectId, version）。version 可为 null，
        // 客户端按 3 参数订阅；老客户端 2 参数订阅也能解包（SignalR 容忍尾部多余参数）。
        await Clients.GroupExcept(g, Context.ConnectionId)
            .SendAsync("ProjectSynced", fromId, projectId, version);
    }

    // 10. PushTreeNodeToUser(string toId, string nodeId) -> void
    [HubMethodName("PushTreeNodeToUser")]
    public async Task PushTreeNodeToUserAsync(string toId, string nodeId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        if (!long.TryParse(toId, out var targetUserId) || targetUserId <= 0) return;

        // 校验目标用户与调用者在同一团队/项目
        if (!await IsPeerAsync(authUserId, targetUserId))
        {
            _logger.LogWarning("PushTreeNodeToUser 拒绝：from={From} to={To} 无团队/项目关联", authUserId, targetUserId);
            return;
        }

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        List<string> connIds;
        lock (_userConnectionsLock)
        {
            connIds = _userConnections.GetValueOrDefault(toId)?.ToList() ?? new List<string>();
        }
        foreach (var connId in connIds)
        {
            await Clients.Client(connId).SendAsync("PeerPushesTreeNode", fromId, nodeId);
        }
    }

    // 11. PushFileSectionToUser(string toId, FileSection section) -> void
    [HubMethodName("PushFileSectionToUser")]
    public async Task PushFileSectionToUserAsync(string toId, FileSectionDto section)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        if (!long.TryParse(toId, out var targetUserId) || targetUserId <= 0) return;

        // 校验目标用户与调用者在同一团队/项目
        if (!await IsPeerAsync(authUserId, targetUserId))
        {
            _logger.LogWarning("PushFileSectionToUser 拒绝：from={From} to={To} 无团队/项目关联", authUserId, targetUserId);
            return;
        }

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        List<string> connIds;
        lock (_userConnectionsLock)
        {
            connIds = _userConnections.GetValueOrDefault(toId)?.ToList() ?? new List<string>();
        }
        foreach (var connId in connIds)
        {
            await Clients.Client(connId).SendAsync("PeerFileSectionArrived", fromId, section);
        }
    }

    // 12. ChangeTeamMember(string userId, string oldTeamId, string newTeamId) -> void
    // 此方法仅供服务端通过 IHubContext.Clients.Group(...).SendAsync(...) 主动推送回调使用，
    // 不应由客户端直接 Invoke 调用，否则任意客户端可触发全局广播。拒绝客户端调用。
    [HubMethodName("ChangeTeamMember")]
    public Task ChangeTeamMemberAsync(string userId, string oldTeamId, string newTeamId)
    {
        _logger.LogWarning("ChangeTeamMember 被客户端非法调用：connId={Conn} userId={Uid}", Context.ConnectionId, userId);
        throw new HubException("此方法仅供服务端调用，禁止客户端直接调用");
    }

    // 13. ChangeProjectMember(string projectId) -> void
    // 同上：仅供服务端推送回调，拒绝客户端调用。
    [HubMethodName("ChangeProjectMember")]
    public Task ChangeProjectMemberAsync(string projectId)
    {
        _logger.LogWarning("ChangeProjectMember 被客户端非法调用：connId={Conn} projectId={Pid}", Context.ConnectionId, projectId);
        throw new HubException("此方法仅供服务端调用，禁止客户端直接调用");
    }

    // 14. ChangeMemberInfo(string userId) -> void
    // 同上：仅供服务端推送回调，拒绝客户端调用。
    [HubMethodName("ChangeMemberInfo")]
    public Task ChangeMemberInfoAsync(string userId)
    {
        _logger.LogWarning("ChangeMemberInfo 被客户端非法调用：connId={Conn} userId={Uid}", Context.ConnectionId, userId);
        throw new HubException("此方法仅供服务端调用，禁止客户端直接调用");
    }

    // 15. QueryOnlineTeam(string teamId) -> IEnumerable<UserState>
    [HubMethodName("QueryOnlineTeam")]
    public async Task<IEnumerable<UserStateDto>> QueryOnlineTeamAsync(string teamId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return Array.Empty<UserStateDto>();

        // 调用者必须是该团队成员才能查询
        if (!await IsTeamMemberAsync(authUserId, teamId))
        {
            _logger.LogWarning("QueryOnlineTeam 拒绝：userId={Uid} 不属于 TeamId={Tid}", authUserId, teamId);
            return Array.Empty<UserStateDto>();
        }
        var users = _onlineUsers.Values.Where(u => u.TeamId == teamId).ToList();
        return users;
    }

    // 16. QueryOnlineProject(string projectId) -> IEnumerable<UserState>
    [HubMethodName("QueryOnlineProject")]
    public async Task<IEnumerable<UserStateDto>> QueryOnlineProjectAsync(string projectId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return Array.Empty<UserStateDto>();

        // 调用者必须是该项目成员才能查询
        if (!await IsProjectMemberAsync(authUserId, projectId))
        {
            _logger.LogWarning("QueryOnlineProject 拒绝：userId={Uid} 不属于 ProjectId={Pid}", authUserId, projectId);
            return Array.Empty<UserStateDto>();
        }
        var users = _onlineUsers.Values.Where(u => u.ProjectId == projectId).ToList();
        return users;
    }

    // 17. UploadState(string userId, UserState state) -> bool
    // 安全：忽略客户端传入的 userId，统一使用鉴权 userId；TeamId/ProjectId 校验归属。
    [HubMethodName("UploadState")]
    public async Task<bool> UploadStateAsync(string userId, UserStateDto state)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return false;
        var authUserIdStr = authUserId.ToString(CultureInfo.InvariantCulture);

        var connId = Context.ConnectionId;
        state.UserId = authUserIdStr;
        state.ConnectionId = connId;

        // 校验 TeamId/ProjectId 归属，防止冒充
        if (!string.IsNullOrEmpty(state.TeamId) && !await IsTeamMemberAsync(authUserId, state.TeamId))
        {
            _logger.LogWarning("UploadState 拒绝：userId={Uid} 不属于 TeamId={Tid}", authUserId, state.TeamId);
            state.TeamId = null;
        }
        if (!string.IsNullOrEmpty(state.ProjectId) && !await IsProjectMemberAsync(authUserId, state.ProjectId))
        {
            _logger.LogWarning("UploadState 拒绝：userId={Uid} 不属于 ProjectId={Pid}", authUserId, state.ProjectId);
            state.ProjectId = null;
        }

        _onlineUsers[connId] = state;

        // 通知同项目其他用户状态更新：PeerStateUpload(string userId, UserState state)
        if (!string.IsNullOrEmpty(state.ProjectId))
        {
            var g = $"project_{state.ProjectId}";
            if (IsValidGroupName(g))
            {
                _ = Clients.GroupExcept(g, connId)
                    .SendAsync("PeerStateUpload", authUserIdStr, state);
            }
        }

        return true;
    }

    // 18. UpdownState(string userId) -> UserState
    // 注：客户端 v2 方法名是 "UpdownState"（疑似客户端笔误，但需保持一致）
    [HubMethodName("UpdownState")]
    public Task<UserStateDto?> UpdownStateAsync(string userId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return Task.FromResult<UserStateDto?>(null);

        string? connId;
        lock (_userConnectionsLock)
        {
            connId = _userConnections.GetValueOrDefault(userId)?.FirstOrDefault();
        }
        if (connId == null) return Task.FromResult<UserStateDto?>(null);
        return Task.FromResult(_onlineUsers.GetValueOrDefault(connId));
    }

    // 19. OpenTicketNavTreeNode(string tableId, string navTreeNodePath) -> void
    [HubMethodName("OpenTicketNavTreeNode")]
    public async Task OpenTicketNavTreeNodeAsync(string tableId, string navTreeNodePath)
    {
        var authUserId = GetAuthUserId();
        if (authUserId <= 0) return;
        var userState = _onlineUsers.GetValueOrDefault(Context.ConnectionId);
        var projectId = userState?.ProjectId ?? "";
        if (string.IsNullOrEmpty(projectId)) return;

        var g = $"project_{projectId}";
        if (!IsValidGroupName(g)) return;

        var fromId = authUserId.ToString(CultureInfo.InvariantCulture);
        // 客户端 On<string, string, string, string>("PeerOpenTicketNavTreeNode", ...)
        // 回调签名：(string peerId, string projectId, string tableId, string nodePath)
        await Clients.GroupExcept(g, Context.ConnectionId)
            .SendAsync("PeerOpenTicketNavTreeNode", fromId, projectId, tableId, navTreeNodePath);
    }

    // ============= 连接生命周期 =============

    public override async Task OnConnectedAsync()
    {
        // SignalR 连接通常通过 QueryString 传递 token（WebSocket 头受浏览器限制）
        var httpContext = Context.GetHttpContext();
        var query = httpContext?.Request.Query;

        var userIdStr = query?["userId"].ToString() ?? "";
        if (string.IsNullOrEmpty(userIdStr)) userIdStr = query?["UserId"].ToString() ?? "";
        var token = query?["token"].ToString() ?? "";
        if (string.IsNullOrEmpty(token)) token = query?["Token"].ToString() ?? "";

        // userId=0 或 token 为空，拒绝连接
        if (!long.TryParse(userIdStr, out var userId) || userId <= 0 || string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("ChatHub 连接拒绝：缺少 userId/token，connId={Conn}", Context.ConnectionId);
            Context.Abort();
            return;
        }

        // 调用 AuthService.ValidateTokenAsync 校验
        if (!await _authService.ValidateTokenAsync(userId, token))
        {
            _logger.LogWarning("ChatHub 连接拒绝：Token 校验失败 userId={Uid} connId={Conn}", userId, Context.ConnectionId);
            Context.Abort();
            return;
        }

        // 校验成功：将 userId 存入 Context.Items
        Context.Items[UserIdItemKey] = userId;
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connId = Context.ConnectionId;
        if (_onlineUsers.TryRemove(connId, out var state))
        {
            // 从用户连接列表移除（线程安全）
            var userId = state.UserId ?? "";
            if (!string.IsNullOrEmpty(userId))
            {
                lock (_userConnectionsLock)
                {
                    if (_userConnections.TryGetValue(userId, out var conns))
                    {
                        conns.Remove(connId);
                        if (conns.Count == 0)
                            _userConnections.TryRemove(userId, out _);
                    }
                }
            }

            // 离开组（group 名校验，防止清理任意 group）
            if (!string.IsNullOrEmpty(state.TeamId))
            {
                var g = $"team_{state.TeamId}";
                if (IsValidGroupName(g))
                    await Groups.RemoveFromGroupAsync(connId, g);
            }
            if (!string.IsNullOrEmpty(state.ProjectId))
            {
                var g = $"project_{state.ProjectId}";
                if (IsValidGroupName(g))
                    await Groups.RemoveFromGroupAsync(connId, g);
            }

            // 通知同团队/项目其他用户下线：PeerLogout(string peerId)
            if (!string.IsNullOrEmpty(state.TeamId))
            {
                var g = $"team_{state.TeamId}";
                if (IsValidGroupName(g))
                    await Clients.Group(g).SendAsync("PeerLogout", userId);
            }
            if (!string.IsNullOrEmpty(state.ProjectId))
            {
                var g = $"project_{state.ProjectId}";
                if (IsValidGroupName(g))
                    await Clients.Group(g).SendAsync("PeerLogout", userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
