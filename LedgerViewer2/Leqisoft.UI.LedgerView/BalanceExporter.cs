using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C1.C1Excel;
using Leqisoft.Model;

namespace Leqisoft.UI.LedgerView;

internal class BalanceExporter : LedgerExporter
{
	private const int CN_CODE = 0;

	private const int CN_NAME = 1;

	private const int CN_BEGINDC = 2;

	private const int CN_BEGINBALANCE = 3;

	private const int CN_DEBIT = 4;

	private const int CN_CREDIT = 5;

	private const int CN_ENDDC = 6;

	private const int CN_ENDBALANCE = 7;

	private Ledger _ledger;

	private List<Account> _accounts;

	internal BalanceExporter(Ledger ledger, IEnumerable<Account> accounts)
	{
		_ledger = ledger;
		_accounts = accounts.ToList();
	}

	private string GetAccountName(Account account)
	{
		return account.GetFullName();
	}

	public override void Build()
	{
		XLSheet xlSheet = xlBook.Sheets[0];
		XLRow xLRow = xlSheet.Rows.Add();
		int i = 0;
		xlSheet[i, 0].SetValue("科目代码", styleHCenter);
		xlSheet[i, 1].SetValue("科目名称", styleHCenter);
		xlSheet.Columns[1].Width = C1XLBook.PixelsToTwips(110.0);
		xlSheet[i, 2].SetValue("期初余额方向", styleHCenter);
		xlSheet[i, 3].SetValue("期初余额", styleHCenter);
		xlSheet.Columns[3].Width = C1XLBook.PixelsToTwips(110.0);
		xlSheet[i, 4].SetValue("借方发生额", styleHCenter);
		xlSheet.Columns[4].Width = C1XLBook.PixelsToTwips(110.0);
		xlSheet[i, 5].SetValue("贷方发生额", styleHCenter);
		xlSheet.Columns[5].Width = C1XLBook.PixelsToTwips(110.0);
		xlSheet[i, 6].SetValue("期末余额方向", styleHCenter);
		xlSheet[i, 7].SetValue("期末余额", styleHCenter);
		xlSheet.Columns[7].Width = C1XLBook.PixelsToTwips(110.0);
		TrialBalanceSheet sheet = _ledger.GetTrialBalanceSheet(base.StartDate, base.EndDate);
		foreach (Account account in _accounts)
		{
			int num = i + 1;
			i = num;
			xlSheet[i, 0].SetValue(account.Code, styleBorder);
			xlSheet[i, 1].SetValue(GetAccountName(account), styleBorder);
			xlSheet[i, 2].SetValue(LedgerExporter.GetDCChar(account.IsDebit, sheet.Start[account].Total), styleHCenter);
			xlSheet[i, 3].SetValue(LedgerExporter.EmptyIf0(Math.Abs(sheet.Start[account].Total)), styleMoney);
			xlSheet[i, 4].SetValue(sheet.Debit.ContainsKey(account) ? LedgerExporter.EmptyIf0(sheet.Debit[account].Total) : string.Empty, styleMoney);
			xlSheet[i, 5].SetValue(sheet.Credit.ContainsKey(account) ? LedgerExporter.EmptyIf0(sheet.Credit[account].Total) : string.Empty, styleMoney);
			xlSheet[i, 6].SetValue(LedgerExporter.GetDCChar(account.IsDebit, sheet.End[account].Total), styleHCenter);
			xlSheet[i, 7].SetValue(LedgerExporter.EmptyIf0(Math.Abs(sheet.End[account].Total)), styleMoney);
			appendAuxItem(account);
			appendChildren(account);
		}
		foreach (XLRow item in (IEnumerable)xlSheet.Rows)
		{
			item.Height = C1XLBook.PixelsToTwips(30.0);
		}
		void appendAuxItem(Account account)
		{
			if (!sheet.End.TryGetValue(account, out var value))
			{
				return;
			}
			sheet.Start.TryGetValue(account, out var value2);
			sheet.Debit.TryGetValue(account, out var value3);
			sheet.Credit.TryGetValue(account, out var value4);
			foreach (AuxiliaryClass item2 in from u in value.ClassBalances.Keys.ToList()
				orderby u.Name
				select u)
			{
				ClassBalance value5 = null;
				ClassBalance value6 = null;
				ClassBalance value7 = null;
				ClassBalance classBalance = value.ClassBalances[item2];
				value2?.ClassBalances.TryGetValue(item2, out value5);
				value3?.ClassBalances.TryGetValue(item2, out value6);
				value4?.ClassBalances.TryGetValue(item2, out value7);
				IOrderedEnumerable<AuxiliaryItem> orderedEnumerable = from u in value.ClassBalances[item2].ItemBalances.Select((KeyValuePair<AuxiliaryItem, decimal> u) => u.Key).ToList()
					orderby u.Code
					select u;
				foreach (AuxiliaryItem item3 in orderedEnumerable)
				{
					string value8 = account.Code + "-" + item3.Code;
					string value9 = GetAccountName(account) + "-" + item2.Name + ":" + item3.Name;
					object value10 = "平";
					object value11 = string.Empty;
					object value12 = string.Empty;
					object value13 = string.Empty;
					object value14 = "平";
					object value15 = string.Empty;
					if (value5 != null && value5.ItemBalances.TryGetValue(item3, out var value16))
					{
						value10 = LedgerExporter.GetDCChar(account.IsDebit, value16);
						value11 = LedgerExporter.EmptyIf0(Math.Abs(value16));
					}
					if (value6 != null && value6.ItemBalances.TryGetValue(item3, out var value17))
					{
						value12 = LedgerExporter.EmptyIf0(value17);
					}
					if (value7 != null && value7.ItemBalances.TryGetValue(item3, out var value18))
					{
						value13 = LedgerExporter.EmptyIf0(value18);
					}
					if (classBalance != null && classBalance.ItemBalances.TryGetValue(item3, out var value19))
					{
						value14 = LedgerExporter.GetDCChar(account.IsDebit, value19);
						value15 = LedgerExporter.EmptyIf0(Math.Abs(value19));
					}
					int num3 = i + 1;
					i = num3;
					xlSheet[i, 0].SetValue(value8, styleBorder);
					xlSheet[i, 1].SetValue(value9, styleBorder);
					xlSheet[i, 2].SetValue(value10, styleHCenter);
					xlSheet[i, 3].SetValue(value11, styleMoney);
					xlSheet[i, 4].SetValue(value12, styleMoney);
					xlSheet[i, 5].SetValue(value13, styleMoney);
					xlSheet[i, 6].SetValue(value14, styleHCenter);
					xlSheet[i, 7].SetValue(value15, styleMoney);
				}
			}
		}
		void appendChildren(Account account)
		{
			foreach (Account child in account.Children)
			{
				int num2 = i + 1;
				i = num2;
				xlSheet[i, 0].SetValue(child.Code, styleBorder);
				xlSheet[i, 1].SetValue(GetAccountName(child), styleBorder);
				xlSheet[i, 2].SetValue(LedgerExporter.GetDCChar(child.IsDebit, sheet.Start[child].Total), styleHCenter);
				xlSheet[i, 3].SetValue(LedgerExporter.EmptyIf0(Math.Abs(sheet.Start[child].Total)), styleMoney);
				xlSheet[i, 4].SetValue(sheet.Debit.ContainsKey(child) ? LedgerExporter.EmptyIf0(sheet.Debit[child].Total) : string.Empty, styleMoney);
				xlSheet[i, 5].SetValue(sheet.Credit.ContainsKey(child) ? LedgerExporter.EmptyIf0(sheet.Credit[child].Total) : string.Empty, styleMoney);
				xlSheet[i, 6].SetValue(LedgerExporter.GetDCChar(child.IsDebit, sheet.End[child].Total), styleHCenter);
				xlSheet[i, 7].SetValue(LedgerExporter.EmptyIf0(Math.Abs(sheet.End[child].Total)), styleMoney);
				appendAuxItem(child);
				appendChildren(child);
			}
		}
	}
}
