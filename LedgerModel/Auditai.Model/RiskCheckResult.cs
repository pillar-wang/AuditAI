﻿namespace Auditai.Model;

// 账务数据风险检查结果行。
// 一行对应：一个科目的一次命中（科目级/单凭证级/辅助项级），或一条规则的错误信息（IsError=true）。
public class RiskCheckResult
{
	public RiskCheckRule Rule { get; set; }      // 来源检查项

	public string RuleNote { get; set; }          // 检查项说明（Rule.Note）

	public int RuleType { get; set; }             // 0 条件 / 1 公式

	public string AccountCode { get; set; }       // 科目代码（聚合/错误结果可空）

	public string AccountName { get; set; }

	public string HitDescription { get; set; }    // 命中说明（金额已格式化为 2 位小数）

	public decimal Amount { get; set; }           // 关键金额

	public string AuxDetail { get; set; }         // 辅助核算明细（类别:名称 金额）

	public string VoucherInfo { get; set; }       // 凭证号/日期/摘要（单凭证口径时）

	public Voucher Voucher;                       // 命中的凭证（凭证级命中非空，科目级/辅助项级为 null）；仅内存使用，不落库不序列化

	public bool IsError { get; set; }

	public string ErrorMessage { get; set; }
}
