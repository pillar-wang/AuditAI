﻿using AuditApiServer.Data;
using AuditApiServer.Models;

namespace AuditApiServer.Services;

/// <summary>
/// 上报审批人输入项（POST /api/Review/Submit 请求体 reviewers 数组元素）。
/// </summary>
public class ReviewerInput
{
    /// <summary>审批级别 1..3，须从 1 连续。</summary>
    public int Level { get; set; }
    /// <summary>审批人 UserId（须为当前团队成员）。</summary>
    public long UserId { get; set; }
}

/// <summary>
/// 上报审核业务层：上报、逐级审批、退回、撤回的状态流转与权限校验。
/// 状态约定：
/// - ReviewSubmissions.Status：0=审批中 1=已通过 2=已退回 3=已撤回
/// - ReviewNodes.Status：0=待审 1=通过 2=退回
/// - Projects.ReviewStatus：0=未上报 1=审批中 2=已通过 3=已退回
/// 当前待审节点 = Level 最小的 Status=0 节点（无显式 CurrentLevel 字段，按节点状态推导）。
/// </summary>
public class ReviewService
{
    private readonly ReviewRepository _repo;
    private readonly UserRepository _userRepo;
    private readonly TeamService _teamSvc;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ReviewRepository repo, UserRepository userRepo, TeamService teamSvc, ILogger<ReviewService> logger)
    {
        _repo = repo;
        _userRepo = userRepo;
        _teamSvc = teamSvc;
        _logger = logger;
    }

    /// <summary>
    /// 工作流事件钩子（Task 4 SignalR 接入点）。
    /// 事件名约定：
    /// - ReviewSubmitted：上报成功 → 目标用户 = 一级审批人（payload.targetUserIds）
    /// - ReviewAdvanced：节点通过流转 → 目标用户 = 下一级审批人 + 提交人
    /// - ReviewApproved：最终通过 → 目标用户 = 提交人
    /// - ReviewRejected：退回 → 目标用户 = 提交人
    /// payload 为匿名对象，含 submissionId/projectId/round/targetUserIds 等字段。
    /// </summary>
    public event Action<string, object>? WorkflowEvent;

    private void RaiseWorkflowEvent(string eventName, object payload)
    {
        // Task 4：接线已在 Program.cs（app.MapHub 之后）完成——订阅 WorkflowEvent 并
        // 按 payload.targetUserIds 通过 IHubContext<ChatHub> 逐用户推送：
        //   ReviewSubmitted → 一级审批人；ReviewAdvanced → 下一级审批人 + 提交人；
        //   ReviewApproved → 提交人；ReviewRejected → 提交人
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
    /// 上报审核：创建 Round 递增的审核单与审批节点，Projects.ReviewStatus=1。
    /// 校验：项目存在且属于该团队且未删除；提交人为项目成员；无进行中审核单；
    /// reviewers 1~3 个、Level 从 1 连续、审批人在团队内（UserTeams）。
    /// templateId 可选（review-flow-template）：按流程模板展开时留痕，仅记录不参与流转。
    /// </summary>
    public async Task<ReviewSubmissionDto> SubmitAsync(
        long currentUserId, Guid teamId, Guid projectId, int reportType, string? reportNo,
        int opinionType, string? note, List<ReviewerInput> reviewers, string? templateId = null)
    {
        if (teamId == Guid.Empty)
            throw new InvalidOperationException("无法解析当前团队");
        if (reviewers == null || reviewers.Count == 0)
            throw new InvalidOperationException("请至少选择 1 名审批人");
        if (reviewers.Count > 3)
            throw new InvalidOperationException("审批级数最多 3 级");

        // 项目归属与状态校验
        var project = await _repo.GetProjectStateAsync(projectId)
            ?? throw new InvalidOperationException("项目不存在");
        if (project.TeamId == null || project.TeamId.Value != teamId)
            throw new UnauthorizedAccessException("无权访问该项目");
        if (project.IsDeleted == 1)
            throw new InvalidOperationException("项目已删除，无法上报审核");

        // 提交人须为项目成员（ProjectMembers 或创建者）
        if (!await _repo.IsProjectMemberAsync(projectId, currentUserId))
            throw new UnauthorizedAccessException("仅项目成员可上报审核");

        // 无进行中审核单
        if (await _repo.GetActiveSubmissionByProjectAsync(projectId) != null)
            throw new InvalidOperationException("该项目已有进行中的审核单，请先完成审批或撤回");

        // 审批人校验：Level 从 1 连续、在团队内
        var ordered = reviewers.OrderBy(r => r.Level).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Level != i + 1)
                throw new InvalidOperationException("审批级别必须从 1 开始连续编号");
            if (ordered[i].UserId <= 0)
                throw new InvalidOperationException($"第 {ordered[i].Level} 级审批人无效");
            if (!await _repo.IsUserInTeamAsync(ordered[i].UserId, teamId))
                throw new InvalidOperationException($"第 {ordered[i].Level} 级审批人不属于当前团队");
        }

        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var round = await _repo.GetNextRoundAsync(projectId);
        var submission = new ReviewSubmissionDto
        {
            Id = Guid.NewGuid().ToString(),
            ProjectId = projectId,
            TeamId = teamId,
            Round = round,
            SubmitterId = currentUserId,
            ReportType = reportType,
            ReportNo = string.IsNullOrWhiteSpace(reportNo) ? null : reportNo.Trim(),
            OpinionType = opinionType,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            Status = 0,
            TotalLevel = ordered.Count,
            SubmitTime = now,
            TemplateId = string.IsNullOrWhiteSpace(templateId) ? null : templateId
        };

        var nodes = new List<ReviewNodeDto>();
        foreach (var reviewer in ordered)
        {
            var user = await _userRepo.GetByIdAsync(reviewer.UserId);
            nodes.Add(new ReviewNodeDto
            {
                SubmissionId = submission.Id,
                Level = reviewer.Level,
                ReviewerId = reviewer.UserId,
                // 姓名快照：优先真实姓名，回退登录名
                ReviewerName = !string.IsNullOrWhiteSpace(user?.Name) ? user.Name : user?.UserName,
                Status = 0
            });
        }

        await _repo.CreateSubmissionAsync(submission, nodes);
        submission.Nodes = nodes;
        submission.CurrentLevel = 1;

        _logger.LogInformation("上报审核: submitter={UserId} project={ProjectId} submission={SubmissionId} round={Round}",
            currentUserId, projectId, submission.Id, round);

        // TODO(Task4-SignalR): ReviewSubmitted → 一级审批人（nodes[0].ReviewerId）
        // payload: { submissionId, projectId, round, operatorId, operatorName, targetUserIds }
        var submitter = await _userRepo.GetByIdAsync(currentUserId);
        RaiseWorkflowEvent("ReviewSubmitted", new
        {
            submissionId = submission.Id,
            projectId,
            round,
            operatorId = currentUserId,
            operatorName = !string.IsNullOrWhiteSpace(submitter?.Name) ? submitter.Name : submitter?.UserName,
            targetUserIds = new long[] { nodes[0].ReviewerId }
        });

        return submission;
    }

    /// <summary>
    /// 通过：校验当前节点审批人，置节点通过；有下一节点则流转（ReviewAdvanced），
    /// 为最后节点则审核单 Status=1、FinishTime、Projects.ReviewStatus=2（ReviewApproved）。
    /// 返回 true 表示最终通过。
    /// </summary>
    public async Task<bool> ApproveAsync(long currentUserId, string submissionId, string? comment)
    {
        var submission = await RequireActiveSubmissionAsync(submissionId);
        var current = GetCurrentNodeOrThrow(submission, currentUserId);

        // Task 4：操作人姓名（供通知 payload）
        var operatorUser = await _userRepo.GetByIdAsync(currentUserId);
        var operatorName = !string.IsNullOrWhiteSpace(operatorUser?.Name) ? operatorUser.Name : operatorUser?.UserName;

        await _repo.UpdateNodeAsync(current.Id, 1, string.IsNullOrWhiteSpace(comment) ? null : comment.Trim());

        var next = submission.Nodes
            .Where(n => n.Level > current.Level)
            .OrderBy(n => n.Level)
            .FirstOrDefault();
        if (next != null)
        {
            // 流转到下一级：审核单保持审批中，Projects.ReviewStatus 保持 1
            _logger.LogInformation("审核流转: submission={SubmissionId} level={Level} → next={NextLevel}",
                submissionId, current.Level, next.Level);

            // TODO(Task4-SignalR): ReviewAdvanced → 下一级审批人（next.ReviewerId）+ 提交人（submission.SubmitterId）
            // payload: { submissionId, projectId, round, fromLevel, toLevel, operatorId, operatorName, targetUserIds }
            RaiseWorkflowEvent("ReviewAdvanced", new
            {
                submissionId = submission.Id,
                projectId = submission.ProjectId,
                round = submission.Round,
                fromLevel = current.Level,
                toLevel = next.Level,
                operatorId = currentUserId,
                operatorName,
                targetUserIds = new[] { next.ReviewerId, submission.SubmitterId }.Distinct().ToArray()
            });
            return false;
        }

        // 最后节点通过：审核单终态 + Projects.ReviewStatus=2
        await _repo.UpdateSubmissionStatusAsync(submission.Id, 1, 2);
        _logger.LogInformation("审核最终通过: submission={SubmissionId} approver={UserId}", submissionId, currentUserId);

        // TODO(Task4-SignalR): ReviewApproved → 提交人（submission.SubmitterId）
        // payload: { submissionId, projectId, round, operatorId, operatorName, targetUserIds }
        RaiseWorkflowEvent("ReviewApproved", new
        {
            submissionId = submission.Id,
            projectId = submission.ProjectId,
            round = submission.Round,
            operatorId = currentUserId,
            operatorName,
            targetUserIds = new long[] { submission.SubmitterId }
        });
        return true;
    }

    /// <summary>
    /// 退回：校验当前节点审批人，置节点退回；审核单 Status=2、FinishTime、Projects.ReviewStatus=3。
    /// </summary>
    public async Task RejectAsync(long currentUserId, string submissionId, string? comment)
    {
        var submission = await RequireActiveSubmissionAsync(submissionId);
        var current = GetCurrentNodeOrThrow(submission, currentUserId);

        await _repo.UpdateNodeAsync(current.Id, 2, string.IsNullOrWhiteSpace(comment) ? null : comment.Trim());
        await _repo.UpdateSubmissionStatusAsync(submission.Id, 2, 3);
        _logger.LogInformation("审核退回: submission={SubmissionId} reviewer={UserId} level={Level}",
            submissionId, currentUserId, current.Level);

        // TODO(Task4-SignalR): ReviewRejected → 提交人（submission.SubmitterId）
        // payload: { submissionId, projectId, round, level, operatorId, operatorName, comment, targetUserIds }
        var rejectedBy = await _userRepo.GetByIdAsync(currentUserId);
        RaiseWorkflowEvent("ReviewRejected", new
        {
            submissionId = submission.Id,
            projectId = submission.ProjectId,
            round = submission.Round,
            level = current.Level,
            operatorId = currentUserId,
            operatorName = !string.IsNullOrWhiteSpace(rejectedBy?.Name) ? rejectedBy.Name : rejectedBy?.UserName,
            comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            targetUserIds = new long[] { submission.SubmitterId }
        });
    }

    /// <summary>
    /// 撤回：仅提交人，且所有节点均未审批；审核单 Status=3、FinishTime、Projects.ReviewStatus=0。
    /// </summary>
    public async Task WithdrawAsync(long currentUserId, string submissionId)
    {
        var submission = await _repo.GetSubmissionByIdAsync(submissionId)
            ?? throw new InvalidOperationException("审核单不存在");
        if (submission.Status != 0)
            throw new InvalidOperationException("该审核单已结束，无法撤回");
        if (submission.SubmitterId != currentUserId)
            throw new UnauthorizedAccessException("仅提交人可撤回审核单");
        if (submission.Nodes.Any(n => n.Status != 0))
            throw new InvalidOperationException("审批已开始，无法撤回");

        await _repo.UpdateSubmissionStatusAsync(submission.Id, 3, 0);
        _logger.LogInformation("撤回审核单: submission={SubmissionId} submitter={UserId}", submissionId, currentUserId);
    }

    /// <summary>
    /// 查询审核单列表（scope=pending|mine|all，团队隔离）。
    /// </summary>
    public Task<List<ReviewSubmissionDto>> GetSubmissionsAsync(
        Guid teamId, string scope, long userId, bool isTeamAdmin, Guid? projectId)
        => _repo.GetSubmissionsAsync(teamId, scope, userId, isTeamAdmin, projectId);

    /// <summary>
    /// 获取项目全部轮次审核单（含各轮节点明细）。
    /// </summary>
    public Task<List<ReviewSubmissionDto>> GetHistoryAsync(Guid projectId, Guid teamId)
        => _repo.GetHistoryByProjectAsync(projectId, teamId);

    /// <summary>
    /// 当前用户是否为某项目的"当前节点审批人"（供只读放行判定，Task 5.5 使用）。
    /// </summary>
    public async Task<bool> IsPendingReviewerAsync(Guid projectId, long userId)
        => await _repo.GetPendingSubmissionForReviewerAsync(projectId, userId) != null;

    /// <summary>
    /// 团队管理员判定（复用 TeamService：Team Owner / IsTeamAdmin / IsSystemAdmin）。
    /// </summary>
    public Task<bool> IsTeamAdminAsync(long userId, Guid teamId)
        => _teamSvc.IsTeamAdminAsync(userId, teamId);

    // ============= 私有辅助 =============

    /// <summary>
    /// 获取进行中的审核单（Status=0），并推导 CurrentLevel。
    /// </summary>
    private async Task<ReviewSubmissionDto> RequireActiveSubmissionAsync(string submissionId)
    {
        var submission = await _repo.GetSubmissionByIdAsync(submissionId)
            ?? throw new InvalidOperationException("审核单不存在");
        if (submission.Status != 0)
            throw new InvalidOperationException("该审核单已结束审批");
        return submission;
    }

    /// <summary>
    /// 获取当前待审节点（Level 最小的 Status=0 节点），校验操作人为该节点审批人。
    /// </summary>
    private static ReviewNodeDto GetCurrentNodeOrThrow(ReviewSubmissionDto submission, long currentUserId)
    {
        var current = submission.Nodes
            .Where(n => n.Status == 0)
            .OrderBy(n => n.Level)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("审核单状态异常：无待审节点");
        if (current.ReviewerId != currentUserId)
            throw new UnauthorizedAccessException("仅当前节点审批人可执行该操作");
        return current;
    }
}
