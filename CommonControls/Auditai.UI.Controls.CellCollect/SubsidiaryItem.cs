using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Auditai.Model;
using Newtonsoft.Json;

namespace Auditai.UI.Controls.CellCollect;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
[JsonObject("SubsidiaryItem")]
public class SubsidiaryItem : CollectItem
{
	[JsonProperty(PropertyName = "TypeNumber")]
	public string TypeNumber { get; set; }

	[JsonProperty(PropertyName = "SortIndex")]
	public int Index { get; set; }

	public Voucher GetVoucher(Ledger ledger, int auditYear)
	{
		Account account = ledger.Accounts.Find((Account t) => t.Code.Equals(AccountCode));
		if (account == null)
		{
			throw new InvalidCollectSettingException(string.Empty);
		}
		base.AccountName = account.Name;
		if (auditYear < ledger.StartDate.Year || auditYear > ledger.GetEndDate().Year)
		{
			throw new InvalidAuditYearException("识别到当前表格的年度信息与账套年度不匹配，无法进行智能采账填充。");
		}
		DateTime start = StartTime.CopyToSpecificYear(auditYear);
		DateTime end = EndTime.CopyToSpecificYear(auditYear);
		SubsidiaryLedger subsidiaryLedger = ledger.GetSubsidiaryLedger(account, start, end);
		IEnumerable<Voucher> source = from s in subsidiaryLedger.Months.SelectMany((MonthSubsidiaryLedger t) => t.Entries)
			select s.Voucher;
		List<Voucher> list = source.Where((Voucher v) => (v.Type.Name + v.Number).Equals(TypeNumber)).ToList();
		if (Index >= list.Count)
		{
			return null;
		}
		return list[Index];
	}

	public override decimal GetValue(Ledger ledger, int auditYear)
	{
		return GetVoucher(ledger, auditYear)?.Amount ?? 0m;
	}
}
