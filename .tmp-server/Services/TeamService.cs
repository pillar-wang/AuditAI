﻿﻿﻿﻿using AuditApiServer.Data;
using AuditApiServer.Models;
using Microsoft.Data.Sqlite;

namespace AuditApiServer.Services;

/// <summary>
/// 团队业务逻辑服务。
/// 阶段 3 实现：团队 CRUD、成员管理、用户分组、权限、合并请求等业务逻辑。
/// SignalR PeerTeamMembersChanged 推送将在阶段 8 实现，此处仅日志记录。
/// </summary>
public class TeamService
{
    private readonly TeamRepository _teamRepo;
    private readonly UserRepository _userRepo;
    private readonly SqliteStorage _db;
    private readonly ILogger<TeamService> _logger;

    public TeamService(TeamRepository teamRepo, UserRepository userRepo, SqliteStorage db, ILogger<TeamService> logger)
    {
        _teamRepo = teamRepo;
        _userRepo = userRepo;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 创建团队。
    /// </summary>
    public async Task<TeamDto> CreateTeamAsync(long ownerUserId, string name, int type = 0)
        => await _teamRepo.CreateTeamAsync(ownerUserId, name, type);

    /// <summary>
    /// 解散团队（校验权限：仅 Owner 可操作）。
    /// </summary>
    public async Task<bool> DismissTeamAsync(Guid teamId, long requesterId)
    {
        var success = await _teamRepo.DismissTeamAsync(teamId, requesterId);
        if (!success) throw new UnauthorizedAccessException("仅团队所有者可解散团队");
        return true;
    }

    /// <summary>
    /// 更新团队名称。
    /// </summary>
    public async Task UpdateTeamNameAsync(Guid teamId, string newName)
        => await _teamRepo.UpdateTeamNameAsync(teamId, newName);

    /// <summary>
    /// 切换用户当前团队（更新 Users.TeamId）。
    /// 安全审计修复 V2-C-09：执行 UPDATE 前校验用户属于目标团队且目标团队未软删除，
    /// 防止任意已登录用户跨团队切换。
    /// </summary>
    public async Task UpdateCurrentTeamAsync(long userId, Guid teamId)
    {
        using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        // 1. 校验用户是目标团队成员（UserTeams 存在关联记录）
        using (var memberCmd = conn.CreateCommand())
        {
            memberCmd.CommandText = "SELECT COUNT(1) FROM UserTeams WHERE UserId = @uid AND TeamId = @tid";
            memberCmd.Parameters.AddWithValue("@uid", userId);
            memberCmd.Parameters.AddWithValue("@tid", teamId.ToString());
            if (Convert.ToInt32(await memberCmd.ExecuteScalarAsync()) == 0)
            {
                throw new UnauthorizedAccessException("用户不属于该团队");
            }
        }

        // 2. 校验目标团队未软删除
        using (var teamCmd = conn.CreateCommand())
        {
            teamCmd.CommandText = "SELECT COUNT(1) FROM Teams WHERE Id = @tid AND (IsDeleted = 0 OR IsDeleted IS NULL)";
            teamCmd.Parameters.AddWithValue("@tid", teamId.ToString());
            if (Convert.ToInt32(await teamCmd.ExecuteScalarAsync()) == 0)
            {
                throw new UnauthorizedAccessException("目标团队不存在或已删除");
            }
        }

        // 3. 通过校验后执行 UPDATE
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET TeamId = @tid WHERE Id = @uid";
        cmd.Parameters.AddWithValue("@tid", teamId.ToString());
        cmd.Parameters.AddWithValue("@uid", userId);
        await cmd.ExecuteNonQueryAsync();
        _logger.LogInformation("用户 {UserId} 切换当前团队到 {TeamId}", userId, teamId);
    }

    /// <summary>
    /// 获取用户的所有团队。
    /// </summary>
    public async Task<List<TeamDto>> GetUserTeamsAsync(long userId)
        => await _teamRepo.GetUserTeamsAsync(userId);

    /// <summary>
    /// 获取团队成员列表。
    /// </summary>
    public async Task<List<UserDto>> GetTeamUsersAsync(Guid teamId)
        => await _teamRepo.GetTeamUsersAsync(teamId);

    /// <summary>
    /// 校验调用者是否指定团队的管理员（Team Owner / IsTeamAdmin / IsSystemAdmin）。
    /// 安全审计修复（High）：成员管理操作必须在服务层校验调用者权限，防止越权添加/移除成员。
    /// </summary>
    public async Task<bool> IsTeamAdminAsync(long operatorUserId, Guid teamId)
    {
        if (operatorUserId <= 0 || teamId == Guid.Empty) return false;

        using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        // 1. 校验调用者是该团队成员（UserTeams 存在记录）
        using (var memberCmd = conn.CreateCommand())
        {
            memberCmd.CommandText = "SELECT COUNT(1) FROM UserTeams WHERE UserId = @uid AND TeamId = @tid";
            memberCmd.Parameters.AddWithValue("@uid", operatorUserId);
            memberCmd.Parameters.AddWithValue("@tid", teamId.ToString());
            if (Convert.ToInt32(await memberCmd.ExecuteScalarAsync()) == 0) return false;
        }

        // 2. 校验调用者是团队 Owner 或全局 TeamAdmin/SystemAdmin
        using var adminCmd = conn.CreateCommand();
        adminCmd.CommandText = @"
            SELECT t.OwnerUserId, u.IsTeamAdmin, u.IsSystemAdmin
            FROM Teams t
            INNER JOIN Users u ON u.Id = @uid
            WHERE t.Id = @tid AND (t.IsDeleted = 0 OR t.IsDeleted IS NULL)";
        adminCmd.Parameters.AddWithValue("@uid", operatorUserId);
        adminCmd.Parameters.AddWithValue("@tid", teamId.ToString());
        using var reader = await adminCmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return false;

        var ownerUserId = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
        var isTeamAdmin = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
        var isSystemAdmin = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));

        return ownerUserId == operatorUserId || isTeamAdmin == 1 || isSystemAdmin == 1;
    }

    /// <summary>
    /// 添加用户到团队（同时触发 SignalR 推送，阶段 8 实现）。
    /// 安全审计修复（High）：校验 operatorUserId 是 Team Admin，否则抛 UnauthorizedAccessException。
    /// </summary>
    public async Task AddUserToTeamAsync(long operatorUserId, long userId, Guid teamId)
    {
        if (!await IsTeamAdminAsync(operatorUserId, teamId))
            throw new UnauthorizedAccessException("仅团队管理员可添加成员");

        await _teamRepo.AddUserToTeamAsync(userId, teamId);
        _logger.LogInformation("操作者 {OperatorId} 将用户 {UserId} 加入团队 {TeamId}", operatorUserId, userId, teamId);
    }

    /// <summary>
    /// 从团队移除用户。
    /// 安全审计修复（High）：校验 operatorUserId 是 Team Admin，否则抛 UnauthorizedAccessException。
    /// </summary>
    public async Task RemoveUserFromTeamAsync(long operatorUserId, long userId, Guid teamId)
    {
        if (!await IsTeamAdminAsync(operatorUserId, teamId))
            throw new UnauthorizedAccessException("仅团队管理员可移除成员");

        await _teamRepo.RemoveUserFromTeamAsync(userId, teamId);
        _logger.LogInformation("操作者 {OperatorId} 将用户 {UserId} 移出团队 {TeamId}", operatorUserId, userId, teamId);
    }

    /// <summary>
    /// 获取团队下的用户分组。
    /// </summary>
    public async Task<List<UserGroupDto>> GetTeamUserGroupsAsync(Guid teamId)
        => await _teamRepo.GetTeamUserGroupsAsync(teamId);

    /// <summary>
    /// 添加用户分组。
    /// </summary>
    public async Task<UserGroupDto> AddUserGroupAsync(string name, Guid teamId, long? parentId)
        => await _teamRepo.AddUserGroupAsync(name, teamId, parentId);

    /// <summary>
    /// 移动用户到分组。
    /// </summary>
    public async Task MoveUserToGroupAsync(long userId, long groupId)
        => await _teamRepo.MoveUserToGroupAsync(userId, groupId);

    /// <summary>
    /// 删除用户分组。
    /// </summary>
    public async Task DeleteUserGroupAsync(long groupId)
        => await _teamRepo.DeleteUserGroupAsync(groupId);

    /// <summary>
    /// 重命名用户分组。
    /// </summary>
    public async Task RenameUserGroupAsync(long groupId, string newName)
        => await _teamRepo.RenameUserGroupAsync(groupId, newName);

    /// <summary>
    /// 更新用户职务。
    /// </summary>
    public async Task UpdateJobTitleAsync(long userId, string jobTitle)
        => await _teamRepo.UpdateJobTitleAsync(userId, jobTitle);

    /// <summary>
    /// 获取用户权限 JSON。
    /// </summary>
    public async Task<string?> GetUserPermissionsAsync(long userId, Guid teamId)
        => await _teamRepo.GetUserPermissionsAsync(userId, teamId);

    /// <summary>
    /// 设置用户权限 JSON。
    /// </summary>
    public async Task SetUserPermissionsAsync(long userId, Guid teamId, string permissionsJson)
        => await _teamRepo.SetUserPermissionsAsync(userId, teamId, permissionsJson);

    /// <summary>
    /// 允许/禁止团队合并（MVP：仅记录）。
    /// </summary>
    public async Task AllowTeamMergeAsync(Guid teamId, bool allow)
        => await _teamRepo.AllowTeamMergeAsync(teamId, allow);

    /// <summary>
    /// 团队合并请求（MVP：仅记录）。
    /// </summary>
    public async Task TeamMergeRequestAsync(Guid fromTeamId, Guid toTeamId, long requesterId)
        => await _teamRepo.TeamMergeRequestAsync(fromTeamId, toTeamId, requesterId);

    /// <summary>
    /// 获取项目成员列表。
    /// </summary>
    public async Task<List<UserDto>> GetProjectUsersAsync(Guid projectId)
        => await _teamRepo.GetProjectUsersAsync(projectId);

    /// <summary>
    /// 获取用户当前团队 Id（从 Users.TeamId 读取）。
    /// </summary>
    public async Task<Guid> GetCurrentUserTeamIdAsync(long userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        return user?.TeamId ?? Guid.Empty;
    }
}
