using Auditai.DTO;
using Auditai.Util;
using UserModel = Auditai.Model.User;
using RiskCheckScheme = Auditai.Model.RiskCheckScheme;
using RiskCheckRule = Auditai.Model.RiskCheckRule;

namespace Auditai.UI.LedgerView;

/// <summary>
/// 风险检查方案云端共享库的公共辅助：可用性/权限判定、范围文案、云端详情与本地方案的互转、上传请求构造。
/// 只做纯数据映射，不发网络请求、不落库。
/// </summary>
internal static class RiskCheckCloudHelper
{
	/// <summary>云端共享库是否可用（本地模式不可用，UI 需隐藏云端入口）</summary>
	public static bool IsCloudAvailable => !WebApiClient.IsLocalMode;

	/// <summary>是否可管理团队库（团队管理员或系统管理员）</summary>
	public static bool CanManageTeamLibrary
	{
		get
		{
			UserModel current = UserModel.Current;
			return current != null && (current.IsTeamAdmin || current.IsSystemAdmin);
		}
	}

	/// <summary>是否可管理系统库（仅系统管理员）</summary>
	public static bool CanManageSystemLibrary
	{
		get
		{
			UserModel current = UserModel.Current;
			return current != null && current.IsSystemAdmin;
		}
	}

	/// <summary>范围文案：0→"团队库"，1→"系统库"，其他→空串</summary>
	public static string ScopeLabel(int scope)
	{
		switch (scope)
		{
		case 0:
			return "团队库";
		case 1:
			return "系统库";
		default:
			return "";
		}
	}

	/// <summary>
	/// 云端方案详情→本地方案（未落库）：Id/规则 Id 置 0 由本地库重新分配；
	/// SourceScope = detail.Scope + 1（0 团队库→1，1 系统库→2），SourceSchemeId/SourceVersion 记录来源。
	/// </summary>
	public static RiskCheckScheme MapToLocal(RiskCheckSchemeDetailDto detail)
	{
		if (detail == null)
		{
			return null;
		}
		RiskCheckScheme scheme = new RiskCheckScheme
		{
			Id = 0,
			Name = detail.Name,
			Note = detail.Note,
			SourceScope = detail.Scope + 1,
			SourceSchemeId = detail.Id,
			SourceVersion = detail.Version
		};
		if (detail.Rules != null)
		{
			foreach (RiskCheckRuleDto dto in detail.Rules)
			{
				scheme.Rules.Add(ToLocalRule(dto));
			}
		}
		return scheme;
	}

	/// <summary>
	/// 本地方案→新建上传请求：目标库（团队库/系统库）由调用方决定，此处 Id 一律填 0（新建）。
	/// 是否覆盖同名云端方案由 overwrite 决定。
	/// </summary>
	public static RiskCheckSchemeSaveRequestDto BuildSaveRequest(RiskCheckScheme scheme, string name, string note, string sourceLedgerName, bool overwrite)
	{
		return BuildSaveRequest(0L, scheme, name, note, sourceLedgerName, overwrite);
	}

	/// <summary>本地方案→上传请求重载：用于更新已有云端方案（Id 设为 cloudSchemeId）</summary>
	public static RiskCheckSchemeSaveRequestDto BuildSaveRequest(long cloudSchemeId, RiskCheckScheme scheme, string name, string note, string sourceLedgerName, bool overwrite)
	{
		RiskCheckSchemeSaveRequestDto request = new RiskCheckSchemeSaveRequestDto
		{
			Id = cloudSchemeId,
			Name = name,
			Note = note,
			SourceLedgerName = sourceLedgerName,
			Overwrite = overwrite
		};
		if (scheme != null && scheme.Rules != null)
		{
			foreach (RiskCheckRule rule in scheme.Rules)
			{
				request.Rules.Add(ToDtoRule(rule));
			}
		}
		return request;
	}

	private static RiskCheckRule ToLocalRule(RiskCheckRuleDto dto)
	{
		return new RiskCheckRule
		{
			Id = 0,
			SchemeId = 0,
			RuleType = dto.RuleType,
			Note = dto.Note,
			AccountCodes = dto.AccountCodes,
			AccountNames = dto.AccountNames,
			RequireLeaf = dto.RequireLeaf,
			OpeningEnabled = dto.OpeningEnabled,
			OpeningDirection = dto.OpeningDirection,
			OpeningOp = dto.OpeningOp,
			OpeningValue = dto.OpeningValue,
			ClosingEnabled = dto.ClosingEnabled,
			ClosingDirection = dto.ClosingDirection,
			ClosingOp = dto.ClosingOp,
			ClosingValue = dto.ClosingValue,
			DebitEnabled = dto.DebitEnabled,
			DebitScope = dto.DebitScope,
			DebitOp = dto.DebitOp,
			DebitValue = dto.DebitValue,
			CreditEnabled = dto.CreditEnabled,
			CreditScope = dto.CreditScope,
			CreditOp = dto.CreditOp,
			CreditValue = dto.CreditValue,
			AuxNegativeEnabled = dto.AuxNegativeEnabled,
			AuxOp = dto.AuxOp,
			AuxValue = dto.AuxValue,
			LeftExpr = dto.LeftExpr,
			OperatorCode = dto.OperatorCode,
			RightExpr = dto.RightExpr
		};
	}

	private static RiskCheckRuleDto ToDtoRule(RiskCheckRule rule)
	{
		return new RiskCheckRuleDto
		{
			RuleType = rule.RuleType,
			Note = rule.Note,
			AccountCodes = rule.AccountCodes,
			AccountNames = rule.AccountNames,
			RequireLeaf = rule.RequireLeaf,
			OpeningEnabled = rule.OpeningEnabled,
			OpeningDirection = rule.OpeningDirection,
			OpeningOp = rule.OpeningOp,
			OpeningValue = rule.OpeningValue,
			ClosingEnabled = rule.ClosingEnabled,
			ClosingDirection = rule.ClosingDirection,
			ClosingOp = rule.ClosingOp,
			ClosingValue = rule.ClosingValue,
			DebitEnabled = rule.DebitEnabled,
			DebitScope = rule.DebitScope,
			DebitOp = rule.DebitOp,
			DebitValue = rule.DebitValue,
			CreditEnabled = rule.CreditEnabled,
			CreditScope = rule.CreditScope,
			CreditOp = rule.CreditOp,
			CreditValue = rule.CreditValue,
			AuxNegativeEnabled = rule.AuxNegativeEnabled,
			AuxOp = rule.AuxOp,
			AuxValue = rule.AuxValue,
			LeftExpr = rule.LeftExpr,
			OperatorCode = rule.OperatorCode,
			RightExpr = rule.RightExpr
		};
	}
}
