﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Auditai.DTO;

/// <summary>上报审核单（每轮一条）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ReviewSubmissionDto
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("projectId")]
	public string ProjectId { get; set; }

	[JsonProperty("teamId")]
	public string TeamId { get; set; }

	/// <summary>轮次（退回重报递增，1 起）</summary>
	[JsonProperty("round")]
	public int Round { get; set; }

	/// <summary>上报人 UserId</summary>
	[JsonProperty("submitterId")]
	public long SubmitterId { get; set; }

	/// <summary>上报人姓名（服务端可能返回，容错）</summary>
	[JsonProperty("submitterName")]
	public string SubmitterName { get; set; }

	/// <summary>报告类型：0=年报审计 1=中期审计 2=专项审计 3=其他</summary>
	[JsonProperty("reportType")]
	public int ReportType { get; set; }

	/// <summary>报告文号</summary>
	[JsonProperty("reportNo")]
	public string ReportNo { get; set; }

	/// <summary>意见类型：0=无保留 1=保留 2=否定 3=无法表示意见 4=其他</summary>
	[JsonProperty("opinionType")]
	public int OpinionType { get; set; }

	/// <summary>上报说明</summary>
	[JsonProperty("note")]
	public string Note { get; set; }

	/// <summary>状态：0=审批中 1=已通过 2=已退回 3=已撤回</summary>
	[JsonProperty("status")]
	public int Status { get; set; }

	/// <summary>审批级数（1~3）</summary>
	[JsonProperty("totalLevel")]
	public int TotalLevel { get; set; }

	/// <summary>当前待审级别（服务端推导，终态后无意义）</summary>
	[JsonProperty("currentLevel")]
	public int CurrentLevel { get; set; }

	/// <summary>提交时间</summary>
	[JsonProperty("submitTime")]
	public string SubmitTime { get; set; }

	/// <summary>终态时间</summary>
	[JsonProperty("finishTime")]
	public string FinishTime { get; set; }

	/// <summary>项目名称（列表展示用，服务端可能返回，容错）</summary>
	[JsonProperty("projectName")]
	public string ProjectName { get; set; }

	[JsonProperty("nodes")]
	public List<ReviewNodeDto> Nodes { get; set; } = new List<ReviewNodeDto>();
}

/// <summary>顺序审批节点</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ReviewNodeDto
{
	[JsonProperty("id")]
	public long Id { get; set; }

	/// <summary>审批级别 1..TotalLevel</summary>
	[JsonProperty("level")]
	public int Level { get; set; }

	/// <summary>审批人 UserId</summary>
	[JsonProperty("reviewerId")]
	public long ReviewerId { get; set; }

	/// <summary>审批人姓名快照</summary>
	[JsonProperty("reviewerName")]
	public string ReviewerName { get; set; }

	/// <summary>状态：0=待审 1=通过 2=退回</summary>
	[JsonProperty("status")]
	public int Status { get; set; }

	/// <summary>审批意见</summary>
	[JsonProperty("comment")]
	public string Comment { get; set; }

	/// <summary>审批时间</summary>
	[JsonProperty("reviewTime")]
	public string ReviewTime { get; set; }
}

/// <summary>上报审核请求</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ReviewSubmitRequest
{
	[JsonProperty("projectId")]
	public string ProjectId { get; set; }

	/// <summary>报告类型：0=年报审计 1=中期审计 2=专项审计 3=其他</summary>
	[JsonProperty("reportType")]
	public int ReportType { get; set; }

	/// <summary>报告文号</summary>
	[JsonProperty("reportNo")]
	public string ReportNo { get; set; }

	/// <summary>意见类型：0=无保留 1=保留 2=否定 3=无法表示意见 4=其他</summary>
	[JsonProperty("opinionType")]
	public int OpinionType { get; set; }

	/// <summary>上报说明</summary>
	[JsonProperty("note")]
	public string Note { get; set; }

	/// <summary>审批流程模板（review-flow-template，可空=手工配置）</summary>
	[JsonProperty("templateId", NullValueHandling = NullValueHandling.Ignore)]
	public string TemplateId { get; set; }

	/// <summary>审批节点配置（1~3 级，按 Level 顺序审批；套用模板时仅"发起人自选"级别需提供）</summary>
	[JsonProperty("reviewers")]
	public List<ReviewReviewerItem> Reviewers { get; set; } = new List<ReviewReviewerItem>();
}

/// <summary>审批节点配置项</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ReviewReviewerItem
{
	/// <summary>审批级别 1~3</summary>
	[JsonProperty("level")]
	public int Level { get; set; }

	/// <summary>审批人 UserId</summary>
	[JsonProperty("userId")]
	public long UserId { get; set; }
}

/// <summary>审批流程模板节点（assigneeType：0=指定人 1=团队管理员 2=发起人自选）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ReviewFlowNodeDto
{
	[JsonProperty("level")]
	public int Level { get; set; }

	[JsonProperty("assigneeType")]
	public int AssigneeType { get; set; }

	/// <summary>仅 assigneeType=0 时有效</summary>
	[JsonProperty("reviewerId")]
	public long ReviewerId { get; set; }

	/// <summary>指定人姓名（服务端可能返回，容错）</summary>
	[JsonProperty("reviewerName")]
	public string ReviewerName { get; set; }
}

/// <summary>稽核检查页：项目节点名称映射（服务端 ProjectTreeNodes 投影）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class TreeNodeNameDto
{
	[JsonProperty("Id")]
	public long Id { get; set; }

	[JsonProperty("Name")]
	public string Name { get; set; }
}

/// <summary>稽核检查页：项目级校验规则集合（服务端 GetProjectValidations 响应）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ValidationRuleSetDto
{
	[JsonProperty("formulas")]
	public List<ValidationFormula> Formulas { get; set; } = new List<ValidationFormula>();

	[JsonProperty("tables")]
	public List<TreeNodeNameDto> Tables { get; set; } = new List<TreeNodeNameDto>();
}

/// <summary>审批流程模板（团队管理员预置的审批链）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ReviewFlowTemplateDto
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("teamId")]
	public string TeamId { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>审批级数（1~3）</summary>
	[JsonProperty("totalLevel")]
	public int TotalLevel { get; set; }

	/// <summary>1=启用 0=停用</summary>
	[JsonProperty("enabled")]
	public int Enabled { get; set; }

	[JsonProperty("createdAt")]
	public string CreatedAt { get; set; }

	[JsonProperty("updatedAt")]
	public string UpdatedAt { get; set; }

	[JsonProperty("nodes")]
	public List<ReviewFlowNodeDto> Nodes { get; set; } = new List<ReviewFlowNodeDto>();
}

/// <summary>项目归档记录</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class ProjectArchiveDto
{
	[JsonProperty("id")]
	public string Id { get; set; }

	[JsonProperty("projectId")]
	public string ProjectId { get; set; }

	[JsonProperty("teamId")]
	public string TeamId { get; set; }

	/// <summary>归档编号 AR-yyyy-NNNN（团队内递增）</summary>
	[JsonProperty("archiveNo")]
	public string ArchiveNo { get; set; }

	/// <summary>归档操作人 UserId</summary>
	[JsonProperty("operatorId")]
	public long OperatorId { get; set; }

	/// <summary>归档操作人姓名（服务端可能返回，容错）</summary>
	[JsonProperty("operatorName")]
	public string OperatorName { get; set; }

	/// <summary>归档时间</summary>
	[JsonProperty("archiveTime")]
	public string ArchiveTime { get; set; }

	/// <summary>保管期限（年），默认 10，行业要求 ≥10 年</summary>
	[JsonProperty("retentionYears")]
	public int RetentionYears { get; set; }

	/// <summary>备注</summary>
	[JsonProperty("note")]
	public string Note { get; set; }

	/// <summary>项目名称（列表展示用，服务端可能返回，容错）</summary>
	[JsonProperty("projectName")]
	public string ProjectName { get; set; }
}
