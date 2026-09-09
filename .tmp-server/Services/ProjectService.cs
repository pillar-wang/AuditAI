﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using AuditApiServer.Data;
using AuditApiServer.Middleware;
using AuditApiServer.Models;

namespace AuditApiServer.Services;

/// <summary>
/// 项目业务逻辑服务。
/// 阶段 4 实现：项目 CRUD、模板、回收站、复制、分享、演示项目等业务逻辑。
/// 异步任务（CreateProjectAsyncTask/DuplicateProjectAsync/ShareProjectAsync）MVP 阶段同步执行 + 返回 fake taskId；
/// 阶段 5 接入真实异步任务引擎后改造为 BackgroundTask.Run。
/// SignalR PeerProjectMembersChanged 推送将在阶段 8 实现，此处仅日志记录。
/// </summary>
public class ProjectService
{
    private readonly ProjectRepository _projectRepo;
    private readonly TeamRepository _teamRepo;
    private readonly UserRepository _userRepo;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        ProjectRepository projectRepo,
        TeamRepository teamRepo,
        UserRepository userRepo,
        ILogger<ProjectService> logger)
    {
        _projectRepo = projectRepo;
        _teamRepo = teamRepo;
        _userRepo = userRepo;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有未删除项目。
    /// </summary>
    public Task<List<ProjectDto>> GetAllAsync() => _projectRepo.GetAllAsync();

    /// <summary>
    /// 获取模板项目列表。
    /// </summary>
    public Task<List<ProjectDto>> GetTemplatesAsync() => _projectRepo.GetTemplatesAsync();

    /// <summary>
    /// 获取回收站项目列表。
    /// </summary>
    public Task<List<ProjectDto>> GetRecycleProjectsAsync() => _projectRepo.GetRecycleProjectsAsync();

    /// <summary>
    /// 获取团队已付费项目列表。
    /// </summary>
    public Task<List<ProjectDto>> GetTeamPayedProjectsAsync(Guid teamId) => _projectRepo.GetTeamPayedProjectsAsync(teamId);

    /// <summary>
    /// 获取项目详情（含 Creator + Users）。
    /// </summary>
    public Task<ProjectDto?> GetProjectDtoAsync(Guid projectId) => _projectRepo.GetProjectDtoAsync(projectId);

    /// <summary>
    /// 获取项目详情（含 Creator + Users，无租户过滤）。
    /// 用于系统模板（TeamId=NULL）等需要绕过租户过滤的场景。
    /// </summary>
    public Task<ProjectDto?> GetProjectDtoNoTenantAsync(Guid projectId) => _projectRepo.GetProjectDtoNoTenantAsync(projectId);

    /// <summary>
    /// 判断指定用户是否为系统管理员。
    /// 用于系统模板编辑权限校验：系统下发的模板仅系统管理员可编辑。
    /// </summary>
    public Task<bool> IsSystemAdminAsync(long userId) => _userRepo.IsSystemAdminAsync(userId);

    /// <summary>
    /// 创建项目：获取 creator 的 TeamId 并设置到 project.TeamId，再调用 Repository.CreateAsync。
    /// </summary>
    public async Task<ProjectDto> CreateProjectAsync(ProjectDto project, long creatorId)
    {
        if (project.TeamId == null || project.TeamId == Guid.Empty)
        {
            // 修复: 使用 TenantContextAccessor 的 TeamId（由 TenantIsolationFilter 从 UserTeams 表解析），
            // 而非 LookupUserTeamIdAsync 从 Users.TeamId 列读取（可能因 CreateTeamAsync 未更新而过期），
            // 导致 CreateProject 写入的 TeamId 与请求上下文 TeamId 不匹配，后续 UpdateProject/DeleteProject 返回 403
            var teamIdStr = TenantContextAccessor.Current?.TeamId;
            if (!string.IsNullOrEmpty(teamIdStr) && Guid.TryParse(teamIdStr, out var tid) && tid != Guid.Empty)
            {
                project.TeamId = tid;
            }
        }
        return await _projectRepo.CreateAsync(project, creatorId);
    }

    /// <summary>
    /// 更新项目元数据。
    /// 多租户：从 TenantContextAccessor 取当前 teamId 传入 Repository，Repository 端附加 WHERE TeamId 校验。
    /// </summary>
    public Task UpdateProjectAsync(ProjectDto project)
        => _projectRepo.UpdateAsync(project, GetCurrentTeamId());

    /// <summary>
    /// 更新项目成员（全量替换）。
    /// 多租户：传入当前 teamId，Repository 端先校验项目归属再执行。
    /// </summary>
    public Task UpdateProjectMembersAsync(Guid projectId, IEnumerable<(long UserId, int Role)> members)
        => _projectRepo.UpdateProjectMembersAsync(projectId, members, GetCurrentTeamId());

    /// <summary>
    /// 软删除项目到回收站。
    /// 多租户：传入当前 teamId，Repository 端附加 WHERE TeamId 校验。
    /// </summary>
    public Task DeleteProjectAsync(Guid projectId)
        => _projectRepo.DeleteAsync(projectId, GetCurrentTeamId());

    /// <summary>
    /// 从回收站恢复项目。
    /// 多租户：传入当前 teamId，Repository 端附加 WHERE TeamId 校验。
    /// </summary>
    public Task RestoreProjectsAsync(IEnumerable<Guid> projectIds)
        => _projectRepo.RestoreAsync(projectIds, GetCurrentTeamId());

    /// <summary>
    /// 永久删除项目（物理删除）。
    /// 多租户：传入当前 teamId，Repository 端附加 WHERE TeamId 校验。
    /// </summary>
    public Task DeleteFromServerAsync(Guid projectId)
        => _projectRepo.DeleteFromServerAsync(projectId, GetCurrentTeamId());

    /// <summary>
    /// 复制项目。返回新项目 Id。
    /// 多租户：传入当前 teamId，Repository 端校验源项目归属。
    /// </summary>
    public Task<Guid> DuplicateProjectAsync(Guid sourceProjectId, string newName, long creatorId)
        => _projectRepo.DuplicateAsync(sourceProjectId, newName, creatorId, GetCurrentTeamId());

    /// <summary>
    /// 将指定模板推送到所有团队（仅 IsSystemAdmin 可调用）。
    /// </summary>
    public Task<List<TeamPushResultDto>> PushTemplateToAllTeamsAsync(Guid sourceProjectId, long operatorUserId)
        => _projectRepo.PushTemplateToAllTeamsAsync(sourceProjectId, operatorUserId);

    /// <summary>
    /// 分享项目给指定用户列表（等同于 UpdateProjectMembers）。
    /// 多租户：传入当前 teamId，Repository 端先校验项目归属再执行。
    /// </summary>
    public Task ShareProjectAsync(Guid projectId, IEnumerable<long> userIds)
        => _projectRepo.UpdateProjectMembersAsync(projectId, userIds.Select(uid => (UserId: uid, Role: 0)), GetCurrentTeamId());

    /// <summary>
    /// 创建演示项目。
    /// </summary>
    public Task<ProjectDto> CreateDemoAsync(long creatorId, string name)
        => _projectRepo.CreateDemoAsync(creatorId, name);

    /// <summary>
    /// 获取项目后代树（MVP：仅查一层子项目）。
    /// </summary>
    public Task<List<ProjectDto>> GetProjectDescendantsAsync(Guid projectId)
        => _projectRepo.GetDescendantsAsync(projectId);

    /// <summary>
    /// 更新项目版本号（Version + 1），返回新版本号。
    /// </summary>
    public Task<int> UpdateProjectVersionAsync(Guid projectId)
        => _projectRepo.UpdateProjectVersionAsync(projectId);

    /// <summary>
    /// 增加 OperationId（用于 OpenProject 记录打开次数），返回新值。
    /// </summary>
    public Task<int> IncrementOperationIdAsync(Guid projectId)
        => _projectRepo.IncrementOperationIdAsync(projectId);

    /// <summary>
    /// 获取项目当前 Version（用于 OpenProject 返回 ServerVersion）。
    /// </summary>
    public async Task<int> GetProjectVersionAsync(Guid projectId)
    {
        var project = await _projectRepo.GetByIdAsync(projectId);
        return project?.Version ?? 0;
    }

    /// <summary>
    /// 创建项目异步任务（MVP：同步执行 + 返回 fake taskId）。
    /// 阶段 5 接入 TaskService.BackgroundTask.Run 后改造为真正的异步流程。
    /// 客户端 WebApiClient.CreateProject 期望返回 { taskId: long }，此处返回 GUID 字符串作为任务标识。
    /// </summary>
    public async Task<string> CreateProjectAsyncTask(ProjectDto project, long creatorId)
    {
        await CreateProjectAsync(project, creatorId);
        var taskId = Guid.NewGuid().ToString("N");
        _logger.LogInformation("CreateProjectAsyncTask 完成（同步执行）: taskId={TaskId} projectId={ProjectId}", taskId, project.Id);
        return taskId;
    }

    /// <summary>
    /// 复制项目异步任务（MVP：同步执行 + 返回 fake taskId）。
    /// </summary>
    public async Task<string> DuplicateProjectAsyncTask(Guid sourceProjectId, string newName, long creatorId)
    {
        var newId = await DuplicateProjectAsync(sourceProjectId, newName, creatorId);
        var taskId = Guid.NewGuid().ToString("N");
        _logger.LogInformation("DuplicateProjectAsyncTask 完成（同步执行）: taskId={TaskId} newProjectId={NewProjectId}", taskId, newId);
        return taskId;
    }

    /// <summary>
    /// 分享项目异步任务（MVP：同步执行 + 返回 fake taskId）。
    /// </summary>
    public async Task<string> ShareProjectAsyncTask(Guid projectId, IEnumerable<long> userIds)
    {
        await ShareProjectAsync(projectId, userIds);
        var taskId = Guid.NewGuid().ToString("N");
        _logger.LogInformation("ShareProjectAsyncTask 完成（同步执行）: taskId={TaskId} projectId={ProjectId}", taskId, projectId);
        return taskId;
    }

    /// <summary>
    /// 更新项目版本异步任务（MVP：同步执行 + 返回 fake taskId）。
    /// </summary>
    public async Task<string> UpdateProjectVersionAsyncTask(Guid projectId)
    {
        await UpdateProjectVersionAsync(projectId);
        var taskId = Guid.NewGuid().ToString("N");
        _logger.LogInformation("UpdateProjectVersionAsyncTask 完成（同步执行）: taskId={TaskId} projectId={ProjectId}", taskId, projectId);
        return taskId;
    }

    /// <summary>
    /// 获取项目成员列表。
    /// </summary>
    public Task<List<UserDto>> GetProjectUsersAsync(Guid projectId)
        => _projectRepo.GetProjectUsersAsync(projectId);

    /// <summary>
    /// 查询用户的当前 TeamId（从 Users.TeamId 读取）。
    /// </summary>
    private async Task<Guid?> LookupUserTeamIdAsync(long userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user?.TeamId != null && user.TeamId != Guid.Empty) return user.TeamId;
        return null;
    }

    /// <summary>
    /// 从 TenantContextAccessor 获取当前请求的 TeamId（Guid）。
    /// 写操作必须携带 TeamId 以满足 Repository 端的跨租户隔离校验；
    /// 无租户上下文或 TeamId 无效时直接抛 UnauthorizedAccessException（fail-closed）。
    /// </summary>
    private static Guid GetCurrentTeamId()
    {
        var teamIdStr = TenantContextAccessor.Current?.TeamId;
        if (string.IsNullOrEmpty(teamIdStr) || !Guid.TryParse(teamIdStr, out var tid) || tid == Guid.Empty)
            throw new UnauthorizedAccessException("无法解析当前租户上下文 TeamId，写操作被拒绝");
        return tid;
    }
}
