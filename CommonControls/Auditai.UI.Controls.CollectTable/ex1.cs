using System.Linq;
using Auditai.Model;

namespace Auditai.UI.Controls.CollectTable;

public static class ex1
{
	internal static object GetBalanceValue(this TrialBalanceSheet trialBalance, Account account, string caption, AccNameStyleEnum accNameStyle, bool isShowFirstLevelName)
	{
		object result = null;
		switch (caption)
		{
		case "科目代码":
		case "Code":
			result = account.Code;
			break;
		case "科目名称":
		case "Name":
			result = ((accNameStyle == AccNameStyleEnum.SecondFullName) ? GetSecondFullName(account, isShowFirstLevelName) : account.Name);
			break;
		case "期初借方余额":
		case "DebitBeginBalance":
			result = (account.IsDebit ? trialBalance.Start[account].Total : (-trialBalance.Start[account].Total));
			break;
		case "期初贷方余额":
		case "CreditBeginBalance":
			result = (account.IsDebit ? (-trialBalance.Start[account].Total) : trialBalance.Start[account].Total);
			break;
		case "本期借方发生额":
		case "CurrentDebit":
			result = (trialBalance.Debit.ContainsKey(account) ? ((object)trialBalance.Debit[account].Total) : null);
			break;
		case "本月借方发生额":
			result = (trialBalance.Debit.ContainsKey(account) ? ((object)trialBalance.Debit[account].Total) : null);
			break;
		case "本期贷方发生额":
		case "CurrentCredit":
			result = (trialBalance.Credit.ContainsKey(account) ? ((object)trialBalance.Credit[account].Total) : null);
			break;
		case "本月贷方发生额":
			result = (trialBalance.Credit.ContainsKey(account) ? ((object)trialBalance.Credit[account].Total) : null);
			break;
		case "上期借方发生额":
		case "PriorDebit":
			result = (trialBalance.Debit.ContainsKey(account) ? ((object)trialBalance.Debit[account].Total) : null);
			break;
		case "上期贷方发生额":
		case "PriorCredit":
			result = (trialBalance.Credit.ContainsKey(account) ? ((object)trialBalance.Credit[account].Total) : null);
			break;
		case "期末借方余额":
		case "DebitEndBalance":
			result = (account.IsDebit ? trialBalance.End[account].Total : (-trialBalance.End[account].Total));
			break;
		case "期末贷方余额":
		case "CreditEndBalance":
			result = (account.IsDebit ? (-trialBalance.End[account].Total) : trialBalance.End[account].Total);
			break;
		}
		return result;
	}

	internal static object GetBalanceValue(this SubsidiaryLedger subsidiaryLedger, Account account, AuxiliaryItem auxiliaryItem, string caption, AccNameStyleEnum accNameStyle, bool isShowFirstLevelName)
	{
		SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
		decimal num = subsidiaryLedgerTotal?.Balance ?? subsidiaryLedger.BeginBalance;
		object result = null;
		switch (caption)
		{
		case "科目代码":
		case "Code":
			result = account.Code + "-" + auxiliaryItem.Code;
			break;
		case "科目名称":
		case "Name":
			result = ((accNameStyle == AccNameStyleEnum.SecondFullName) ? GetSecondFullName(account, auxiliaryItem, isShowFirstLevelName) : auxiliaryItem.Name);
			break;
		case "辅助核算类别":
		case "ItemClass":
			result = auxiliaryItem.Class.Code;
			break;
		case "辅助核算代码":
		case "ItemNumber":
			result = auxiliaryItem.Code;
			break;
		case "辅助核算名称":
		case "ItemName":
			result = auxiliaryItem.Name;
			break;
		case "期初借方余额":
		case "DebitBeginBalance":
			result = (account.IsDebit ? subsidiaryLedger.BeginBalance : (-subsidiaryLedger.BeginBalance));
			break;
		case "期初贷方余额":
		case "CreditBeginBalance":
			result = (account.IsDebit ? (-subsidiaryLedger.BeginBalance) : subsidiaryLedger.BeginBalance);
			break;
		case "本期借方发生额":
		case "CurrentDebit":
			result = subsidiaryLedgerTotal?.Debit ?? 0m;
			break;
		case "本月借方发生额":
			result = subsidiaryLedgerTotal?.Debit ?? 0m;
			break;
		case "本期贷方发生额":
		case "CurrentCredit":
			result = subsidiaryLedgerTotal?.Credit ?? 0m;
			break;
		case "本月贷方发生额":
			result = subsidiaryLedgerTotal?.Credit ?? 0m;
			break;
		case "PriorDebit":
		case "上期借方发生额":
			result = subsidiaryLedgerTotal?.Debit ?? 0m;
			break;
		case "上期贷方发生额":
		case "PriorCredit":
			result = subsidiaryLedgerTotal?.Credit ?? 0m;
			break;
		case "期末借方余额":
		case "DebitEndBalance":
			result = (account.IsDebit ? num : (-num));
			break;
		case "期末贷方余额":
		case "CreditEndBalance":
			result = (account.IsDebit ? (-num) : num);
			break;
		}
		return result;
	}

	public static string GetSecondFullName(Account account, AuxiliaryItem auxiliaryItem, bool isShowFirstLevelName = false)
	{
		if (!isShowFirstLevelName)
		{
			if (account.Parent != null)
			{
				return GetSecondFullName(account) + "-" + auxiliaryItem.Name;
			}
			return auxiliaryItem.Name;
		}
		return GetSecondFullName(account, isShowFirstLevelName: true) + "-" + auxiliaryItem.Name;
	}

	public static string GetSecondFullName(Account account, bool isShowFirstLevelName = false)
	{
		string text = account.Name;
		Account parent = account.Parent;
		if (!isShowFirstLevelName)
		{
			while (parent != null && parent.Parent != null)
			{
				text = parent.Name + "-" + text;
				parent = parent.Parent;
			}
		}
		else
		{
			while (parent != null)
			{
				text = parent.Name + "-" + text;
				parent = parent.Parent;
			}
		}
		return text;
	}
}
