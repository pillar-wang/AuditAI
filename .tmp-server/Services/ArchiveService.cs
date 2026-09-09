﻿using AuditApiServer.Data;

namespace AuditApiServer.Services;

/// <summary>
/// 项目归档业务层：归档（生成 AR-yyyy-NNNN 编号）、取消归档、候选与列表查询。
/// 状态约定：Projects.ReviewStatus=2（已通过）才可归档；IsArchived=1 后推送端点拒绝修改。
/// </summary>
public class ArchiveService
{
    private readonly ArchiveRepository _repo;
    private readonly ReviewRepository _reviewRepo;
    private readonly TeamService _teamSvc;
    private readonly UserRepository _userRepo;
    private readonly ILogger<ArchiveService> _logger;

    public ArchiveService(ArchiveRepository repo, ReviewRepository reviewRepo, TeamService teamSvc, UserRepository userRepo, ILogger<ArchiveService> logger)
    {
        _repo = repo;
        _reviewRepo = reviewRepo;
        _teamSvc = teamSvc;
        _userRepo = userRepo;
        _logger = logger;
    }

    /// <summary>
    /// 工作流事件钩子（Task 4 SignalR 接入点）。
    /// - ProjectArchived：归档成功 → payload 含 teamId/projectId/archiveNo，供 Program.cs
    ///   转发为向团队全体成员广播 ChatHub 事件 ProjectArchived。
    /// </summary>
    public event Action<string, object>? WorkflowEvent;

    private void RaiseWorkflowEvent(string eventName, object payload)
    {
        try
        {
            WorkflowEvent?.Invoke(eventName, payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "工作流事件订阅者处理异常: {EventName}", eventName);
        }
    }

    /// <summary>
    /// 归档项目：校验 ReviewStatus=2、IsArchived=0、操作人为 TeamAdmin 或项目创建人；
    /// 生成 ArchiveNo（AR-yyyy-NNNN，NNNN = 该团队当年归档数 + 1，补零 4 位）并锁定项目。
    /// </summary>
    public async Task<ArchiveRecordDto> ArchiveAsync(long currentUserId, Guid teamId, Guid projectId, string? note)
    {
        if (teamId == Guid.Empty)
            throw new InvalidOperationException("无法解析当前团队");

        var project = await _reviewRepo.GetProjectStateAsync(projectId)
            ?? throw new InvalidOperationException("项目不存在");
        if (project.TeamId == null || project.TeamId.Value != teamId)
            throw new UnauthorizedAccessException("无权访问该项目");
        if (project.IsDeleted == 1)
            throw new InvalidOperationException("项目已删除，无法归档");
        if (project.ReviewStatus != 2)
            throw new InvalidOperationException("项目未通过审核，不能归档");
        if (project.IsArchived == 1)
            throw new InvalidOperationException("项目已归档，请勿重复操作");

        // 权限：TeamAdmin 或项目创建人
        var isTeamAdmin = await _teamSvc.IsTeamAdminAsync(currentUserId, teamId);
        if (!isTeamAdmin && project.CreatorId != currentUserId)
            throw new UnauthorizedAccessException("仅团队管理员或项目创建人可归档项目");

        // 归档编号：AR-yyyy-NNNN（团队内当年递增）
        var year = DateTime.Now.Year;
        var count = await _repo.CountArchiveNoByYearAsync(teamId, year);
        var archive = new ArchiveRecordDto
        {
            Id = Guid.NewGuid().ToString(),
            ProjectId = projectId,
            TeamId = teamId,
            ArchiveNo = $"AR-{year}-{(count + 1):D4}",
            OperatorId = currentUserId,
            ArchiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            RetentionYears = 10,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        };

        await _repo.CreateArchiveAsync(archive);
        _logger.LogInformation("归档项目: operator={UserId} project={ProjectId} archiveNo={ArchiveNo}",
            currentUserId, projectId, archive.ArchiveNo);

        // Task 4：ProjectArchived 团队广播（targetTeamId=teamId，含 projectId/ArchiveNo）
        // payload: { projectId, teamId, archiveNo, operatorId, operatorName }
        var archivedBy = await _userRepo.GetByIdAsync(currentUserId);
        RaiseWorkflowEvent("ProjectArchived", new
        {
            projectId,
            teamId,
            archiveNo = archive.ArchiveNo,
            operatorId = currentUserId,
            operatorName = !string.IsNullOrWhiteSpace(archivedBy?.Name) ? archivedBy.Name : archivedBy?.UserName
        });
        return archive;
    }

    /// <summary>
    /// 取消归档：仅 TeamAdmin；Projects.IsArchived=0（ReviewStatus 保留 2，可重新归档），归档记录保留。
    /// </summary>
    public async Task CancelAsync(long currentUserId, Guid teamId, Guid projectId)
    {
        if (teamId == Guid.Empty)
            throw new InvalidOperationException("无法解析当前团队");
        if (!await _teamSvc.IsTeamAdminAsync(currentUserId, teamId))
            throw new UnauthorizedAccessException("仅团队管理员可取消归档");

        var project = await _reviewRepo.GetProjectStateAsync(projectId)
            ?? throw new InvalidOperationException("项目不存在");
        if (project.TeamId == null || project.TeamId.Value != teamId)
            throw new UnauthorizedAccessException("无权访问该项目");
        if (project.IsArchived != 1)
            throw new InvalidOperationException("该项目未归档");

        await _repo.CancelArchiveAsync(projectId, teamId);
    }

    /// <summary>
    /// 可归档候选项目列表（ReviewStatus=2 且未归档未删除）。
    /// </summary>
    public Task<List<ArchiveCandidateDto>> GetCandidatesAsync(Guid teamId)
        => _repo.GetCandidatesAsync(teamId);

    /// <summary>
    /// 团队归档记录列表。
    /// </summary>
    public Task<List<ArchiveRecordDto>> GetArchivesAsync(Guid teamId)
        => _repo.GetArchivesAsync(teamId);
}
