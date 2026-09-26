using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Auditai.DTO;

/// <summary>风险检查方案：单条规则（字段与本地 RiskCheckRule 一一对应，云端不使用本地 Id/SchemeId）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class RiskCheckRuleDto
{
	/// <summary>规则类型：0=条件 1=公式</summary>
	[JsonProperty("ruleType")]
	public int RuleType { get; set; }

	/// <summary>规则说明</summary>
	[JsonProperty("note")]
	public string Note { get; set; }

	/// <summary>科目编码（多值以分隔符拼接）</summary>
	[JsonProperty("accountCodes")]
	public string AccountCodes { get; set; }

	/// <summary>科目名称（多值以分隔符拼接）</summary>
	[JsonProperty("accountNames")]
	public string AccountNames { get; set; }

	/// <summary>是否要求末级科目</summary>
	[JsonProperty("requireLeaf")]
	public bool RequireLeaf { get; set; }

	[JsonProperty("openingEnabled")]
	public bool OpeningEnabled { get; set; }

	/// <summary>期初方向：0=不限 1=借 2=贷</summary>
	[JsonProperty("openingDirection")]
	public int OpeningDirection { get; set; }

	/// <summary>期初比较符：0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"</summary>
	[JsonProperty("openingOp")]
	public int OpeningOp { get; set; }

	[JsonProperty("openingValue")]
	public decimal OpeningValue { get; set; }

	[JsonProperty("closingEnabled")]
	public bool ClosingEnabled { get; set; }

	/// <summary>期末方向：0=不限 1=借 2=贷</summary>
	[JsonProperty("closingDirection")]
	public int ClosingDirection { get; set; }

	/// <summary>期末比较符（编码同期初）</summary>
	[JsonProperty("closingOp")]
	public int ClosingOp { get; set; }

	[JsonProperty("closingValue")]
	public decimal ClosingValue { get; set; }

	[JsonProperty("debitEnabled")]
	public bool DebitEnabled { get; set; }

	/// <summary>借方口径：0=本期借方合计 1=单张凭证借方金额</summary>
	[JsonProperty("debitScope")]
	public int DebitScope { get; set; }

	/// <summary>借方比较符（编码同期初）</summary>
	[JsonProperty("debitOp")]
	public int DebitOp { get; set; }

	[JsonProperty("debitValue")]
	public decimal DebitValue { get; set; }

	[JsonProperty("creditEnabled")]
	public bool CreditEnabled { get; set; }

	/// <summary>贷方口径：0=本期贷方合计 1=单张凭证贷方金额</summary>
	[JsonProperty("creditScope")]
	public int CreditScope { get; set; }

	/// <summary>贷方比较符（编码同期初）</summary>
	[JsonProperty("creditOp")]
	public int CreditOp { get; set; }

	[JsonProperty("creditValue")]
	public decimal CreditValue { get; set; }

	[JsonProperty("auxNegativeEnabled")]
	public bool AuxNegativeEnabled { get; set; }

	/// <summary>辅助核算比较符（编码同期初）</summary>
	[JsonProperty("auxOp")]
	public int AuxOp { get; set; }

	[JsonProperty("auxValue")]
	public decimal AuxValue { get; set; }

	/// <summary>公式：左表达式</summary>
	[JsonProperty("leftExpr")]
	public string LeftExpr { get; set; }

	/// <summary>公式：比较符编码</summary>
	[JsonProperty("operatorCode")]
	public int OperatorCode { get; set; }

	/// <summary>公式：右表达式</summary>
	[JsonProperty("rightExpr")]
	public string RightExpr { get; set; }
}

/// <summary>风险检查方案云端共享库：列表项摘要</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class RiskCheckSchemeSummaryDto
{
	[JsonProperty("id")]
	public long Id { get; set; }

	/// <summary>方案库归属：0=团队库 1=系统库（全团队可见、只读）</summary>
	[JsonProperty("scope")]
	public int Scope { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>备注</summary>
	[JsonProperty("note")]
	public string Note { get; set; }

	/// <summary>来源账套名称</summary>
	[JsonProperty("sourceLedgerName")]
	public string SourceLedgerName { get; set; }

	/// <summary>归属人姓名</summary>
	[JsonProperty("ownerName")]
	public string OwnerName { get; set; }

	/// <summary>规则条数</summary>
	[JsonProperty("ruleCount")]
	public int RuleCount { get; set; }

	/// <summary>版本号</summary>
	[JsonProperty("version")]
	public int Version { get; set; }

	/// <summary>更新时间（服务端字符串时间）</summary>
	[JsonProperty("updatedAt")]
	public string UpdatedAt { get; set; }
}

/// <summary>风险检查方案云端共享库：方案详情（含规则清单）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class RiskCheckSchemeDetailDto
{
	[JsonProperty("id")]
	public long Id { get; set; }

	/// <summary>方案库归属：0=团队库 1=系统库</summary>
	[JsonProperty("scope")]
	public int Scope { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>备注</summary>
	[JsonProperty("note")]
	public string Note { get; set; }

	/// <summary>来源账套名称</summary>
	[JsonProperty("sourceLedgerName")]
	public string SourceLedgerName { get; set; }

	/// <summary>归属人姓名</summary>
	[JsonProperty("ownerName")]
	public string OwnerName { get; set; }

	/// <summary>版本号</summary>
	[JsonProperty("version")]
	public int Version { get; set; }

	/// <summary>更新时间（服务端字符串时间）</summary>
	[JsonProperty("updatedAt")]
	public string UpdatedAt { get; set; }

	[JsonProperty("rules")]
	public List<RiskCheckRuleDto> Rules { get; set; } = new List<RiskCheckRuleDto>();
}

/// <summary>风险检查方案云端共享库：保存请求（Id=0 新建，非 0 更新）</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class RiskCheckSchemeSaveRequestDto
{
	/// <summary>方案 Id，0=新建</summary>
	[JsonProperty("id")]
	public long Id { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>备注</summary>
	[JsonProperty("note")]
	public string Note { get; set; }

	/// <summary>来源账套名称</summary>
	[JsonProperty("sourceLedgerName")]
	public string SourceLedgerName { get; set; }

	/// <summary>同名方案是否覆盖</summary>
	[JsonProperty("overwrite")]
	public bool Overwrite { get; set; }

	[JsonProperty("rules")]
	public List<RiskCheckRuleDto> Rules { get; set; } = new List<RiskCheckRuleDto>();
}

/// <summary>风险检查方案云端共享库：保存结果</summary>
[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class RiskCheckSchemeSaveResultDto
{
	[JsonProperty("id")]
	public long Id { get; set; }

	/// <summary>保存后的版本号</summary>
	[JsonProperty("version")]
	public int Version { get; set; }
}
