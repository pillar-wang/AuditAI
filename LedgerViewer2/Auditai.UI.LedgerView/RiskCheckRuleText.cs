using System.Collections.Generic;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

/// <summary>
/// 风险检查项摘要文本渲染工具：把单条规则拼成短文本（如"代码:1122*|期末:贷≥0|负数辅助"）。
/// 由 RiskCheckEditor 抽取而来，供检查项网格与云端方案库预览共用。
/// </summary>
internal static class RiskCheckRuleText
{
	// 条件检查项：把启用的维度拼成短文本，如"代码:1122*|期末:贷≥0|负数辅助"
	public static string BuildDigest(RiskCheckRule rule)
	{
		if (rule.RuleType == RiskCheckRule.RULE_TYPE_FORMULA)
		{
			return Truncate($"{rule.LeftExpr} {OpSymbol(rule.OperatorCode)} {rule.RightExpr}", 60);
		}
		List<string> parts = new List<string>();
		if (!string.IsNullOrWhiteSpace(rule.AccountCodes))
		{
			parts.Add("代码:" + rule.AccountCodes);
		}
		if (!string.IsNullOrWhiteSpace(rule.AccountNames))
		{
			parts.Add("名称:" + rule.AccountNames);
		}
		if (rule.RequireLeaf)
		{
			parts.Add("仅叶子科目");
		}
		if (rule.OpeningEnabled)
		{
			parts.Add($"期初:{DirectionChar(rule.OpeningDirection)}{OpSymbol(rule.OpeningOp)}{FormatValue(rule.OpeningValue)}");
		}
		if (rule.ClosingEnabled)
		{
			parts.Add($"期末:{DirectionChar(rule.ClosingDirection)}{OpSymbol(rule.ClosingOp)}{FormatValue(rule.ClosingValue)}");
		}
		if (rule.DebitEnabled)
		{
			parts.Add((rule.DebitScope == RiskCheckRule.SCOPE_PERIOD_TOTAL ? "本期借方" : "单凭证借方") + $":{OpSymbol(rule.DebitOp)}{FormatValue(rule.DebitValue)}");
		}
		if (rule.CreditEnabled)
		{
			parts.Add((rule.CreditScope == RiskCheckRule.SCOPE_PERIOD_TOTAL ? "本期贷方" : "单凭证贷方") + $":{OpSymbol(rule.CreditOp)}{FormatValue(rule.CreditValue)}");
		}
		if (rule.AuxNegativeEnabled)
		{
			parts.Add((rule.AuxValue != 0m) ? $"负数辅助:{OpSymbol(rule.AuxOp)}{FormatValue(rule.AuxValue)}" : "负数辅助");
		}
		return string.Join("|", parts);
	}

	// Op 编码对齐 ValidationOperator.Code：0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"
	public static string OpSymbol(int op)
	{
		switch (op)
		{
		case 0:
			return "=";
		case 1:
			return ">";
		case 2:
			return ">=";
		case 3:
			return "<";
		case 4:
			return "<=";
		case 5:
			return "<>";
		default:
			return op.ToString();
		}
	}

	public static string DirectionChar(int direction)
	{
		switch (direction)
		{
		case RiskCheckRule.DIRECTION_DEBIT:
			return "借";
		case RiskCheckRule.DIRECTION_CREDIT:
			return "贷";
		default:
			return "";
		}
	}

	public static string FormatValue(decimal value)
	{
		return value.ToString("0.##");
	}

	public static string Truncate(string text, int max)
	{
		if (string.IsNullOrEmpty(text) || text.Length <= max)
		{
			return text ?? "";
		}
		return text.Substring(0, max) + "…";
	}
}
