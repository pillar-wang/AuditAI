using System;
using System.Reflection;
using Auditai.Model;
using Newtonsoft.Json;

namespace Auditai.UI.Controls.CellCollect;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
[JsonObject("BalanceItem")]
public class BalanceItem : CollectItem
{
	[JsonProperty(PropertyName = "AmountEnum")]
	public AmountEnum AmountEnum { get; set; }

	public override decimal GetValue(Ledger ledger, int auditYear)
	{
		int year = ledger.StartDate.Year;
		int year2 = ledger.GetEndDate().Year;
		if (auditYear < year || auditYear > year2 + 1)
		{
			throw new InvalidAuditYearException("识别到当前表格的年度信息与账套年度不匹配，无法进行智能采账填充");
		}
		int num = auditYear - 1;
		Account account = ledger.Accounts.Find((Account t) => t.Code.Equals(AccountCode));
		if (account == null)
		{
			throw new InvalidCollectSettingException(string.Empty);
		}
		base.AccountName = account.Name;
		DateTime start = StartTime.CopyToSpecificYear(auditYear);
		DateTime dateTime = EndTime.CopyToSpecificYear(auditYear);
		dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month)).AddDays(1.0).AddMilliseconds(-1.0);
		switch (AmountEnum)
		{
		case AmountEnum.DebitBegin:
		{
			if (auditYear > year2 || auditYear < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			TrialBalanceSheet trialBalanceSheet2 = ledger.GetTrialBalanceSheet(start, dateTime);
			if (!trialBalanceSheet2.Start.ContainsKey(account))
			{
				break;
			}
			if (!account.IsDebit)
			{
				return -trialBalanceSheet2.Start[account].Total;
			}
			return trialBalanceSheet2.Start[account].Total;
		}
		case AmountEnum.CreditBegin:
		{
			if (auditYear > year2 || auditYear < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			TrialBalanceSheet trialBalanceSheet8 = ledger.GetTrialBalanceSheet(start, dateTime);
			if (!trialBalanceSheet8.Start.ContainsKey(account))
			{
				break;
			}
			if (!account.IsDebit)
			{
				return trialBalanceSheet8.Start[account].Total;
			}
			return -trialBalanceSheet8.Start[account].Total;
		}
		case AmountEnum.DebitBalance:
		{
			if (auditYear > year2 || auditYear < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			TrialBalanceSheet trialBalanceSheet4 = ledger.GetTrialBalanceSheet(start, dateTime);
			if (!trialBalanceSheet4.End.ContainsKey(account))
			{
				break;
			}
			if (!account.IsDebit)
			{
				return -trialBalanceSheet4.End[account].Total;
			}
			return trialBalanceSheet4.End[account].Total;
		}
		case AmountEnum.CreditBalance:
		{
			if (auditYear > year2 || auditYear < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			TrialBalanceSheet trialBalanceSheet7 = ledger.GetTrialBalanceSheet(start, dateTime);
			if (!trialBalanceSheet7.End.ContainsKey(account))
			{
				break;
			}
			if (!account.IsDebit)
			{
				return trialBalanceSheet7.End[account].Total;
			}
			return -trialBalanceSheet7.End[account].Total;
		}
		case AmountEnum.DebitAmount:
		{
			if (auditYear > year2 || auditYear < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			TrialBalanceSheet trialBalanceSheet5 = ledger.GetTrialBalanceSheet(start, dateTime);
			if (trialBalanceSheet5.Debit.ContainsKey(account))
			{
				return trialBalanceSheet5.Debit[account].Total;
			}
			break;
		}
		case AmountEnum.CreditAmount:
		{
			if (auditYear > year2 || auditYear < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			TrialBalanceSheet trialBalanceSheet3 = ledger.GetTrialBalanceSheet(start, dateTime);
			if (trialBalanceSheet3.Credit.ContainsKey(account))
			{
				return trialBalanceSheet3.Credit[account].Total;
			}
			break;
		}
		case AmountEnum.PreDebitAmount:
		{
			if (num > year2 || num < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			DateTime start3 = start.AddYears(-1);
			DateTime end2 = dateTime.AddYears(-1);
			TrialBalanceSheet trialBalanceSheet6 = ledger.GetTrialBalanceSheet(start3, end2);
			if (trialBalanceSheet6.Debit.ContainsKey(account))
			{
				return trialBalanceSheet6.Debit[account].Total;
			}
			break;
		}
		case AmountEnum.PreCreditAmount:
		{
			if (num > year2 || num < year)
			{
				throw new UnExpectAuditYearException(string.Empty);
			}
			DateTime start2 = start.AddYears(-1);
			DateTime end = dateTime.AddYears(-1);
			TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(start2, end);
			if (trialBalanceSheet.Credit.ContainsKey(account))
			{
				return trialBalanceSheet.Credit[account].Total;
			}
			break;
		}
		}
		return 0m;
	}
}
