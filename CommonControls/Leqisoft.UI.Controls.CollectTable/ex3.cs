using System;
using System.Linq;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls.CollectTable;

internal static class ex3
{
	internal static object GetSubsidiaryValue(this SubsidiaryLedgerEntry subsidiaryEntry, string caption)
	{
		Voucher voucher = subsidiaryEntry.Voucher;
		object result = null;
		switch (caption)
		{
		case "日期":
		case "Date":
			result = voucher.Day;
			break;
		case "Type":
		case "字":
			result = voucher.Type.Name;
			break;
		case "号":
		case "Number":
			result = voucher.Number;
			break;
		case "字号":
		case "TypeNum":
			result = voucher.Type.Name + "-" + voucher.Number;
			break;
		case "摘要":
		case "Digest":
			result = voucher.Digest;
			break;
		case "科目名称":
		case "Name":
			result = voucher.GetDisplayAccountNameWithDetail();
			break;
		case "对方科目":
		case "Opposite":
			result = string.Join(",", voucher.OppositeAccounts.Select((Account t) => t.Name).Distinct());
			break;
		case "借方金额":
		case "CurrentDebit":
			result = (voucher.IsDebit ? voucher.Amount : 0m);
			break;
		case "贷方金额":
		case "CurrentCredit":
			result = (voucher.IsDebit ? 0m : voucher.Amount);
			break;
		case "方向":
		case "DC":
			result = GetDCChar(voucher.Account.IsDebit, subsidiaryEntry.Balance);
			break;
		case "余额":
		case "Balance":
			result = Math.Abs(subsidiaryEntry.Balance);
			break;
		}
		return result;
	}

	private static object GetDCChar(bool isDebit, decimal balance)
	{
		if (balance == 0m)
		{
			return "平";
		}
		if (balance > 0m)
		{
			if (!isDebit)
			{
				return "贷";
			}
			return "借";
		}
		if (!isDebit)
		{
			return "借";
		}
		return "贷";
	}
}
