﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Auditai.Model;

// 账务数据风险检查——条件检查引擎。
// 公式检查经 FormulaRuleExecutor 涉及 UI 侧非线程安全缓存，须在 UI 线程调用。
//
// 符号约定（signed 归一化）：
//   模型内存储的余额（InitialBalance / 试算平衡表 Total）以“科目自然方向为正”：
//     借方向科目：正=借方余额、负=贷方余额；贷方向科目相反。
//   引擎统一换算为 signed（借方余额=正、贷方余额=负，与科目自身方向无关）后判定。
//   辅助项余额（ItemBalances）同样以科目自然方向为正，负数即“余额方向与科目自然方向相反”。
//   凭证对科目的带符号影响 = IsDebit ? Amount : -Amount（正=借方影响、负=贷方影响，
//   DirectionToggled 的负金额由此自然归一，判定金额取绝对值）。
public static class RiskCheckEngine
{
	// 公式检查引擎注入点（UI 层注册）；未注册时公式检查项输出一条 IsError 结果
	public static Func<Ledger, RiskCheckRule, List<RiskCheckResult>> FormulaRuleExecutor;

	private class VoucherHit
	{
		public Voucher Voucher;

		public decimal Impact;

		public string Description;
	}

	private class AuxHit
	{
		public AuxiliaryClass Class;

		public AuxiliaryItem Item;

		public decimal Signed;

		public decimal Abs;

		public string Description;
	}

	// 仅执行方案内全部条件检查项（供后台线程调用；试算平衡表整方案只算一次）
	// 公式检查项不经此入口，须由调用方在 UI 线程逐条调 RunFormulaRule
	public static List<RiskCheckResult> RunConditionRules(Ledger ledger, RiskCheckScheme scheme)
	{
		List<RiskCheckResult> results = new List<RiskCheckResult>();
		if (scheme == null || scheme.Rules == null)
		{
			return results;
		}
		TrialBalanceSheet trial = null;
		foreach (RiskCheckRule rule in scheme.Rules)
		{
			if (rule == null || rule.RuleType != RiskCheckRule.RULE_TYPE_CONDITION)
			{
				continue;
			}
			try
			{
				if (trial == null)
				{
					// 试算平衡表整方案只算一次，下传每条规则（原每规则重算）
					trial = ledger.GetTrialBalanceSheet(ledger.StartDate, ledger.EndDate);
				}
				results.AddRange(RunConditionRule(ledger, rule, trial));
			}
			catch (Exception ex)
			{
				results.Add(CreateErrorResult(rule, ex.Message));
			}
		}
		return results;
	}

	// 执行单条公式检查项（须在 UI 线程调用：经 FormulaRuleExecutor 委托访问 UI 侧虚拟表与缓存，非线程安全）
	public static List<RiskCheckResult> RunFormulaRule(Ledger ledger, RiskCheckRule rule)
	{
		List<RiskCheckResult> results = new List<RiskCheckResult>();
		if (rule == null)
		{
			return results;
		}
		try
		{
			if (FormulaRuleExecutor == null)
			{
				results.Add(CreateErrorResult(rule, "公式检查引擎未注册"));
			}
			else
			{
				List<RiskCheckResult> formulaResults = FormulaRuleExecutor(ledger, rule);
				if (formulaResults != null)
				{
					results.AddRange(formulaResults);
				}
			}
		}
		catch (Exception ex)
		{
			results.Add(CreateErrorResult(rule, ex.Message));
		}
		return results;
	}

	// 执行单个条件检查项，期间统一使用 Ledger.StartDate ~ Ledger.EndDate；
	// trial 为 null 时内部兜底计算一次，保证独立调用安全
	public static List<RiskCheckResult> RunConditionRule(Ledger ledger, RiskCheckRule rule, TrialBalanceSheet trial)
	{
		List<RiskCheckResult> results = new List<RiskCheckResult>();
		if (ledger == null || rule == null)
		{
			return results;
		}
		try
		{
			List<Account> accounts = FilterAccounts(ledger, rule);
			if (accounts.Count == 0)
			{
				return results;
			}
			if (trial == null)
			{
				trial = ledger.GetTrialBalanceSheet(ledger.StartDate, ledger.EndDate);
			}
			foreach (Account account in accounts)
			{
				CheckAccount(ledger, rule, trial, account, results);
			}
		}
		catch (Exception ex)
		{
			results.Add(CreateErrorResult(rule, ex.Message));
		}
		return results;
	}

	private static void CheckAccount(Ledger ledger, RiskCheckRule rule, TrialBalanceSheet trial, Account account, List<RiskCheckResult> results)
	{
		// —— 科目级维度：期初/期末余额、本期借/贷发生额合计（启用即须全部命中，AND） ——
		List<string> descriptions = new List<string>();
		decimal? closingAmount = null;
		decimal? openingAmount = null;
		decimal? debitTotalAmount = null;
		decimal? creditTotalAmount = null;
		bool accountDimEnabled = false;
		bool accountDimsAllHit = true;

		if (rule.OpeningEnabled)
		{
			accountDimEnabled = true;
			decimal signed = ToSigned(account, GetTotal(ledger.InitialBalance, account));
			string desc;
			decimal amount;
			if (CheckBalanceDim("期初余额", rule.OpeningDirection, rule.OpeningOp, rule.OpeningValue, signed, out desc, out amount))
			{
				descriptions.Add(desc);
				openingAmount = amount;
			}
			else
			{
				accountDimsAllHit = false;
			}
		}
		if (rule.ClosingEnabled)
		{
			accountDimEnabled = true;
			decimal signed = ToSigned(account, GetTotal(trial.End, account));
			string desc;
			decimal amount;
			if (CheckBalanceDim("期末余额", rule.ClosingDirection, rule.ClosingOp, rule.ClosingValue, signed, out desc, out amount))
			{
				descriptions.Add(desc);
				closingAmount = amount;
			}
			else
			{
				accountDimsAllHit = false;
			}
		}
		if (rule.DebitEnabled && rule.DebitScope == RiskCheckRule.SCOPE_PERIOD_TOTAL)
		{
			accountDimEnabled = true;
			decimal total = GetTotal(trial.Debit, account);
			if (Compare(rule.DebitOp, total, rule.DebitValue))
			{
				descriptions.Add($"本期借方发生额合计 {F(total)}（{OpSymbol(rule.DebitOp)} {F(rule.DebitValue)}）");
				debitTotalAmount = total;
			}
			else
			{
				accountDimsAllHit = false;
			}
		}
		if (rule.CreditEnabled && rule.CreditScope == RiskCheckRule.SCOPE_PERIOD_TOTAL)
		{
			accountDimEnabled = true;
			decimal total = GetTotal(trial.Credit, account);
			if (Compare(rule.CreditOp, total, rule.CreditValue))
			{
				descriptions.Add($"本期贷方发生额合计 {F(total)}（{OpSymbol(rule.CreditOp)} {F(rule.CreditValue)}）");
				creditTotalAmount = total;
			}
			else
			{
				accountDimsAllHit = false;
			}
		}

		// —— 单凭证维度（Scope=1：该科目及下级科目期间内逐张凭证判定） ——
		List<VoucherHit> debitVoucherHits = null;
		List<VoucherHit> creditVoucherHits = null;
		if (rule.DebitEnabled && rule.DebitScope == RiskCheckRule.SCOPE_VOUCHER_SINGLE)
		{
			debitVoucherHits = GetVoucherHits(ledger, account, isDebitSide: true, rule.DebitOp, rule.DebitValue);
		}
		if (rule.CreditEnabled && rule.CreditScope == RiskCheckRule.SCOPE_VOUCHER_SINGLE)
		{
			creditVoucherHits = GetVoucherHits(ledger, account, isDebitSide: false, rule.CreditOp, rule.CreditValue);
		}

		// —— 负数辅助核算维度（期末辅助项余额方向与科目自然方向相反） ——
		List<AuxHit> auxHits = null;
		if (rule.AuxNegativeEnabled)
		{
			auxHits = GetNegativeAuxHits(trial, account, rule.AuxOp, rule.AuxValue);
		}

		// —— 启用维度 AND：全部命中才输出结果 ——
		if (accountDimEnabled && !accountDimsAllHit)
		{
			return;
		}
		if (debitVoucherHits != null && debitVoucherHits.Count == 0)
		{
			return;
		}
		if (creditVoucherHits != null && creditVoucherHits.Count == 0)
		{
			return;
		}
		if (auxHits != null && auxHits.Count == 0)
		{
			return;
		}

		if (accountDimEnabled)
		{
			decimal amount = closingAmount ?? openingAmount ?? debitTotalAmount ?? creditTotalAmount ?? 0m;
			results.Add(new RiskCheckResult
			{
				Rule = rule,
				RuleNote = rule.Note,
				RuleType = rule.RuleType,
				AccountCode = account.Code,
				AccountName = account.Name,
				HitDescription = string.Join("；", descriptions),
				Amount = amount
			});
		}
		if (debitVoucherHits != null)
		{
			foreach (VoucherHit hit in debitVoucherHits)
			{
				results.Add(new RiskCheckResult
				{
					Rule = rule,
					RuleNote = rule.Note,
					RuleType = rule.RuleType,
					AccountCode = account.Code,
					AccountName = account.Name,
					HitDescription = hit.Description,
					Amount = hit.Impact,
					VoucherInfo = GetVoucherInfo(hit.Voucher),
					Voucher = hit.Voucher
				});
			}
		}
		if (creditVoucherHits != null)
		{
			foreach (VoucherHit hit in creditVoucherHits)
			{
				results.Add(new RiskCheckResult
				{
					Rule = rule,
					RuleNote = rule.Note,
					RuleType = rule.RuleType,
					AccountCode = account.Code,
					AccountName = account.Name,
					HitDescription = hit.Description,
					Amount = hit.Impact,
					VoucherInfo = GetVoucherInfo(hit.Voucher),
					Voucher = hit.Voucher
				});
			}
		}
		if (auxHits != null)
		{
			foreach (AuxHit hit in auxHits)
			{
				results.Add(new RiskCheckResult
				{
					Rule = rule,
					RuleNote = rule.Note,
					RuleType = rule.RuleType,
					AccountCode = account.Code,
					AccountName = account.Name,
					HitDescription = hit.Description,
					Amount = hit.Abs,
					AuxDetail = $"{hit.Class?.Name}:{hit.Item?.Name} {F(hit.Abs)}"
				});
			}
		}
	}

	// 余额维度判定：direction 1=借 2=贷 0=不限。
	// 方向=借：要求余额确实在借方（signed>0），取其金额与阈值比较；
	// 方向=贷：要求余额确实在贷方（signed<0），取其相反数（即绝对值）与阈值比较；
	// 方向=不限：直接用 signed 比较（正=借方、负=贷方，描述中注明）。
	private static bool CheckBalanceDim(string label, int direction, int op, decimal threshold, decimal signed, out string description, out decimal amount)
	{
		if (direction == RiskCheckRule.DIRECTION_DEBIT)
		{
			if (signed <= 0m)
			{
				description = null;
				amount = 0m;
				return false;
			}
			amount = signed;
		}
		else if (direction == RiskCheckRule.DIRECTION_CREDIT)
		{
			if (signed >= 0m)
			{
				description = null;
				amount = 0m;
				return false;
			}
			amount = -signed;
		}
		else
		{
			amount = signed;
		}
		if (!Compare(op, amount, threshold))
		{
			description = null;
			return false;
		}
		string directionText = ((direction == RiskCheckRule.DIRECTION_DEBIT) ? "为借方 " : ((direction == RiskCheckRule.DIRECTION_CREDIT) ? "为贷方 " : "（带符号，正为借方、负为贷方） "));
		description = $"{label}{directionText}{F(amount)}（{OpSymbol(op)} {F(threshold)}）";
		return true;
	}

	private static List<VoucherHit> GetVoucherHits(Ledger ledger, Account account, bool isDebitSide, int op, decimal threshold)
	{
		List<VoucherHit> hits = new List<VoucherHit>();
		HashSet<Account> scope = new HashSet<Account>(account.DescendantsAndSelf);
		DateTime start = ledger.StartDate.Date;
		DateTime end = ledger.EndDate.Date;
		foreach (Voucher voucher in ledger.Vouchers)
		{
			if (voucher == null || voucher.Account == null || !scope.Contains(voucher.Account))
			{
				continue;
			}
			if (voucher.Day.Date < start || voucher.Day.Date > end)
			{
				continue;
			}
			// 带符号影响：正=借方影响、负=贷方影响（DirectionToggled 负金额据此归一，判定金额取绝对值）
			decimal signedImpact = (voucher.IsDebit ? voucher.Amount : (-voucher.Amount));
			decimal impact;
			if (isDebitSide)
			{
				if (signedImpact <= 0m)
				{
					continue;
				}
				impact = signedImpact;
			}
			else
			{
				if (signedImpact >= 0m)
				{
					continue;
				}
				impact = -signedImpact;
			}
			if (!Compare(op, impact, threshold))
			{
				continue;
			}
			string sideText = (isDebitSide ? "单张凭证借方金额 " : "单张凭证贷方金额 ");
			hits.Add(new VoucherHit
			{
				Voucher = voucher,
				Impact = impact,
				Description = $"{sideText}{F(impact)}（{OpSymbol(op)} {F(threshold)}）"
			});
		}
		return hits;
	}

	// 遍历期末 ClassBalances→ItemBalances，标记余额为负（方向与科目自然方向相反）且
	// 绝对值满足 AuxOp/AuxValue 的辅助项
	private static List<AuxHit> GetNegativeAuxHits(TrialBalanceSheet trial, Account account, int op, decimal threshold)
	{
		List<AuxHit> hits = new List<AuxHit>();
		AccountBalance endBalance;
		if (trial == null || trial.End == null || !trial.End.TryGetValue(account, out endBalance) || endBalance == null)
		{
			return hits;
		}
		foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classPair in endBalance.ClassBalances)
		{
			if (classPair.Key == null || classPair.Value == null)
			{
				continue;
			}
			foreach (KeyValuePair<AuxiliaryItem, decimal> itemPair in classPair.Value.ItemBalances)
			{
				decimal signed = itemPair.Value;   // 正=科目自然方向、负=相反方向
				if (signed >= 0m)
				{
					continue;
				}
				decimal abs = -signed;
				if (!Compare(op, abs, threshold))
				{
					continue;
				}
				hits.Add(new AuxHit
				{
					Class = classPair.Key,
					Item = itemPair.Key,
					Signed = signed,
					Abs = abs,
					Description = $"辅助核算余额方向与科目方向相反（期末）：{classPair.Key.Name}:{itemPair.Key?.Name} 余额 {F(signed)}（绝对值 {OpSymbol(op)} {F(threshold)}）"
				});
			}
		}
		return hits;
	}

	// 科目范围：AccountCodes（, / ，分隔，支持 * 通配与纯前缀）× AccountNames 关键字 × RequireLeaf，取交集
	private static List<Account> FilterAccounts(Ledger ledger, RiskCheckRule rule)
	{
		List<string> codePatterns = SplitList(rule.AccountCodes);
		List<string> nameKeywords = SplitList(rule.AccountNames);
		List<Account> matched = new List<Account>();
		foreach (Account account in ledger.Accounts)
		{
			if (account == null)
			{
				continue;
			}
			if (codePatterns.Count > 0 && !codePatterns.Any((string p) => MatchCode(account.Code, p)))
			{
				continue;
			}
			if (nameKeywords.Count > 0 && !nameKeywords.Any((string k) => !string.IsNullOrEmpty(account.Name) && account.Name.Contains(k)))
			{
				continue;
			}
			if (rule.RequireLeaf && account.Children.Count > 0)
			{
				continue;
			}
			matched.Add(account);
		}
		return matched;
	}

	private static List<string> SplitList(string text)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrWhiteSpace(text))
		{
			return list;
		}
		string[] array = text.Split(new char[2] { ',', '，' });
		foreach (string token in array)
		{
			string item = token.Trim();
			if (item.Length > 0)
			{
				list.Add(item);
			}
		}
		return list;
	}

	// 代码通配模式 → 已编译正则缓存。
	// MatchCode 会在 FilterAccounts 里按"每科目 × 每模式"调用，原实现每次都 new Regex，
	// 上千科目的账套下开销可观；模式串来自规则（数量有限），可安全缓存。
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Regex> CodePatternCache =
		new System.Collections.Concurrent.ConcurrentDictionary<string, Regex>();

	// "1122" 等价 "1122*"（纯前缀）；含 * 时按通配符整体匹配
	private static bool MatchCode(string code, string pattern)
	{
		if (string.IsNullOrEmpty(code))
		{
			return false;
		}
		if (pattern.IndexOf('*') >= 0)
		{
			Regex regex = CodePatternCache.GetOrAdd(pattern,
				(string p) => new Regex("^" + Regex.Escape(p).Replace("\\*", ".*") + "$", RegexOptions.IgnoreCase));
			return regex.IsMatch(code);
		}
		return code.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
	}

	// Op 编码对齐 ValidationOperator.Code：0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"
	private static bool Compare(int op, decimal left, decimal right)
	{
		switch (op)
		{
			case 0:
				return left == right;
			case 1:
				return left > right;
			case 2:
				return left >= right;
			case 3:
				return left < right;
			case 4:
				return left <= right;
			case 5:
				return left != right;
			default:
				return false;
		}
	}

	private static string OpSymbol(int op)
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

	// 存储余额（科目自然方向为正）→ signed（借方余额为正、贷方余额为负）
	private static decimal ToSigned(Account account, decimal stored)
	{
		return (account.IsDebit ? stored : (-stored));
	}

	private static decimal GetTotal(DateBalance balances, Account account)
	{
		AccountBalance balance;
		if (balances == null || !balances.TryGetValue(account, out balance) || balance == null)
		{
			return 0m;
		}
		return balance.Total;
	}

	private static string GetVoucherInfo(Voucher voucher)
	{
		string type = voucher.Type?.Name ?? "";
		string digest = voucher.Digest ?? "";
		return $"{type}字第{voucher.Number}号 {voucher.Day:yyyy-MM-dd} {digest}";
	}

	private static RiskCheckResult CreateErrorResult(RiskCheckRule rule, string message)
	{
		return new RiskCheckResult
		{
			Rule = rule,
			RuleNote = rule?.Note,
			RuleType = rule?.RuleType ?? 0,
			IsError = true,
			ErrorMessage = message
		};
	}

	private static string F(decimal value)
	{
		return value.ToString("N2", CultureInfo.InvariantCulture);
	}
}
