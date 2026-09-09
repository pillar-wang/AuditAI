﻿﻿using AuditApiServer.Data;

namespace AuditApiServer.Services;

/// <summary>
/// 审批流程模板业务层：管理员预置审批链的 CRUD 与"按模板展开审批节点"。
/// 模板只是预填的 reviewers（1~3 级），上报时展开为既有 ReviewNodes 模型，
/// 审批流转/退回/撤回逻辑完全复用 ReviewService，不做任何改动。
/// </summary>
public class ReviewFlowService
{
    private readonly ReviewFlowRepository _repo;
    private readonly TeamService _teamSvc;
    private readonly ILogger<ReviewFlowService> _logger;

    public ReviewFlowService(ReviewFlowRepository repo, TeamService teamSvc, ILogger<ReviewFlowService> logger)
    {
        _repo = repo;
        _teamSvc = teamSvc;
        _logger = logger;
    }

    /// <summary>
    /// 模板列表（团队隔离）。includeDisabled=true 供管理界面（TeamAdmin）查看停用项。
    /// </summary>
    public async Task<List<ReviewFlowTemplateDto>> GetTemplatesAsync(Guid teamId, bool includeDisabled)
    {
        var list = await _repo.GetTemplatesAsync(teamId);
        return includeDisabled ? list : list.Where(t => t.Enabled == 1).ToList();
    }

    /// <summary>
    /// 保存模板（新建或更新，按 template.Id 是否为空区分）。仅 TeamAdmin。
    /// 校验：名称非空且团队内唯一、级数 1~3、节点 Level 从 1 连续且数量与级数一致、
    /// AssigneeType ∈ {0,1,2}、指定人级别 ReviewerId 属于本团队。
    /// </summary>
    public async Task<ReviewFlowTemplateDto> SaveAsync(long currentUserId, Guid teamId, ReviewFlowTemplateDto template)
    {
        await RequireTeamAdminAsync(currentUserId, teamId);

        var isUpdate = !string.IsNullOrWhiteSpace(template.Id);
        var name = (template.Name ?? "").Trim();
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("模板名称不能为空");
        if (name.Length > 50)
            throw new InvalidOperationException("模板名称不能超过 50 个字符");
        if (await _repo.IsNameExistsAsync(teamId, name, isUpdate ? template.Id : null))
            throw new InvalidOperationException($"模板名称\"{name}\"已存在");
        if (template.TotalLevel is < 1 or > 3)
            throw new InvalidOperationException("审批级数须为 1~3 级");

        var nodes = (template.Nodes ?? new List<ReviewFlowNodeDto>()).OrderBy(n => n.Level).ToList();
        if (nodes.Count != template.TotalLevel)
            throw new InvalidOperationException("节点数量与审批级数不一致");
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Level != i + 1)
                throw new InvalidOperationException("节点级别必须从 1 开始连续编号");
            if (nodes[i].AssigneeType is < 0 or > 2)
                throw new InvalidOperationException($"第 {nodes[i].Level} 级的审批人指定方式无效");
            if (nodes[i].AssigneeType == 0)
            {
                if (nodes[i].ReviewerId <= 0)
                    throw new InvalidOperationException($"第 {nodes[i].Level} 级未指定审批人");
                if (!await _repo.IsUserInTeamAsync(nodes[i].ReviewerId, teamId))
                    throw new InvalidOperationException($"第 {nodes[i].Level} 级指定审批人不属于当前团队");
            }
        }

        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (isUpdate)
        {
            // 编辑保留原 CreatedBy/CreatedAt
            var existing = await _repo.GetTemplateByIdAsync(teamId, template.Id)
                ?? throw new InvalidOperationException("模板不存在或无权修改");
            existing.Name = name;
            existing.TotalLevel = template.TotalLevel;
            existing.Enabled = template.Enabled == 1 ? 1 : 0;
            existing.Nodes = nodes;
            await _repo.UpdateAsync(existing);
            return existing;
        }

        var created = new ReviewFlowTemplateDto
        {
            Id = Guid.NewGuid().ToString(),
            TeamId = teamId,
            Name = name,
            TotalLevel = template.TotalLevel,
            Enabled = template.Enabled == 1 ? 1 : 0,
            CreatedBy = currentUserId,
            CreatedAt = now,
            Nodes = nodes
        };
        await _repo.CreateAsync(created);
        return created;
    }

    /// <summary>删除模板。仅 TeamAdmin。</summary>
    public async Task DeleteAsync(long currentUserId, Guid teamId, string templateId)
    {
        await RequireTeamAdminAsync(currentUserId, teamId);
        await _repo.DeleteAsync(teamId, templateId);
    }

    /// <summary>
    /// 启用/停用模板。仅 TeamAdmin。仅翻转 Enabled 不做节点校验——
    /// 指定审批人离职导致模板不合法时，管理员仍可将其停用（全量保存校验会拒绝这种场景）。
    /// </summary>
    public async Task SetEnabledAsync(long currentUserId, Guid teamId, string templateId, int enabled)
    {
        await RequireTeamAdminAsync(currentUserId, teamId);
        if (enabled is not (0 or 1))
            throw new InvalidOperationException("enabled 须为 0（停用）或 1（启用）");
        await _repo.UpdateEnabledAsync(teamId, templateId, enabled);
    }

    /// <summary>
    /// 按模板展开审批节点（供 Review/Submit 使用）。
    /// - 指定人（0）：取模板 ReviewerId
    /// - 团队管理员（1）：解析团队 Owner，回退首位 Users.IsTeamAdmin=1 的成员
    /// - 发起人自选（2）：取请求 reviewers 中对应 Level 的 userId
    /// 返回按 Level 排序的 reviewers 输入（可直接传入 ReviewService.SubmitAsync）。
    /// </summary>
    public async Task<(List<ReviewerInput> Reviewers, ReviewFlowTemplateDto Template)> ExpandTemplateAsync(
        Guid teamId, string templateId, List<ReviewerInput> overrides)
    {
        var template = await _repo.GetTemplateByIdAsync(teamId, templateId)
            ?? throw new InvalidOperationException("审批流程模板不存在");
        if (template.Enabled != 1)
            throw new InvalidOperationException("该审批流程模板已停用，请联系团队管理员");

        var overrideMap = (overrides ?? new List<ReviewerInput>())
            .Where(r => r.UserId > 0)
            .GroupBy(r => r.Level)
            .ToDictionary(g => g.Key, g => g.First().UserId);

        var reviewers = new List<ReviewerInput>();
        foreach (var node in template.Nodes.OrderBy(n => n.Level))
        {
            long userId = node.AssigneeType switch
            {
                0 => node.ReviewerId,
                1 => await _repo.ResolveTeamAdminAsync(teamId),
                _ => overrideMap.TryGetValue(node.Level, out var uid) ? uid : 0
            };
            if (userId <= 0)
                throw new InvalidOperationException(
                    node.AssigneeType == 1
                        ? $"第 {node.Level} 级审批人解析失败：团队无管理员"
                        : $"模板第 {node.Level} 级为\"发起人自选\"，请选择该级审批人");
            reviewers.Add(new ReviewerInput { Level = node.Level, UserId = userId });
        }
        return (reviewers, template);
    }

    /// <summary>校验当前用户为团队管理员（复用 TeamService：Owner / IsTeamAdmin / IsSystemAdmin）。</summary>
    private async Task RequireTeamAdminAsync(long userId, Guid teamId)
    {
        if (!await _teamSvc.IsTeamAdminAsync(userId, teamId))
            throw new UnauthorizedAccessException("仅团队管理员可管理审批流程模板");
    }
}
