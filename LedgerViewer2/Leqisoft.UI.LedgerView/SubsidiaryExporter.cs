using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C1.C1Excel;
using Leqisoft.DTO;
using Leqisoft.Model;

namespace Leqisoft.UI.LedgerView;

internal class SubsidiaryExporter : LedgerExporter
{
	private const int CN_DATE = 0;

	private const int CN_TYPE = 1;

	private const int CN_NUMBER = 2;

	private const int CN_DIGEST = 3;

	private const int CN_OPPOSITE = 4;

	private const int CN_DEBIT = 5;

	private const int CN_CREDIT = 6;

	private const int CN_DC = 7;

	private const int CN_BALANCE = 8;

	private Ledger _ledger;

	private SubsidiaryLedger _subsidiary;

	public TotalFlag TotalFlag { get; set; } = TotalFlag.Data;


	internal SubsidiaryExporter(Ledger ledger, SubsidiaryLedger subsidiary)
	{
		_ledger = ledger;
		_subsidiary = subsidiary;
	}

	public override void Build()
	{
		XLSheet xLSheet = xlBook.Sheets[0];
		Account account = _subsidiary.Account;
		int num = 0;
		xLSheet[num, 0].SetValue("日期", styleHCenter);
		xLSheet.Columns[0].Width = C1XLBook.PixelsToTwips(90.0);
		xLSheet[num, 1].SetValue("字", styleHCenter);
		xLSheet[num, 2].SetValue("号", styleHCenter);
		xLSheet[num, 3].SetValue("摘要", styleHCenter);
		xLSheet.Columns[3].Width = C1XLBook.PixelsToTwips(250.0);
		xLSheet[num, 4].SetValue("对方科目", styleHCenter);
		xLSheet.Columns[4].Width = C1XLBook.PixelsToTwips(110.0);
		xLSheet[num, 5].SetValue("借方金额", styleHCenter);
		xLSheet.Columns[5].Width = C1XLBook.PixelsToTwips(110.0);
		xLSheet[num, 6].SetValue("贷方金额", styleHCenter);
		xLSheet.Columns[6].Width = C1XLBook.PixelsToTwips(110.0);
		xLSheet[num, 7].SetValue("方向", styleHCenter);
		xLSheet[num, 8].SetValue("余额", styleHCenter);
		xLSheet.Columns[8].Width = C1XLBook.PixelsToTwips(110.0);
		if (_subsidiary.BeginBalance != 0m)
		{
			num++;
			xLSheet[num, 0].SetValue("", styleBorder);
			xLSheet[num, 1].SetValue("", styleBorder);
			xLSheet[num, 2].SetValue("", styleBorder);
			xLSheet[num, 3].SetValue("期初余额", styleBorder);
			xLSheet[num, 4].SetValue("", styleBorder);
			xLSheet[num, 5].SetValue("", styleBorder);
			xLSheet[num, 6].SetValue("", styleBorder);
			xLSheet[num, 7].SetValue(LedgerExporter.GetDCChar(account.IsDebit, _subsidiary.BeginBalance), styleHCenter);
			xLSheet[num, 8].SetValue(LedgerExporter.EmptyIf0(Math.Abs(_subsidiary.BeginBalance)), styleMoney);
		}
		foreach (MonthSubsidiaryLedger item in from t in _subsidiary.Months
			orderby t.Year, t.Month
			select t)
		{
			List<SubsidiaryLedgerEntry> list = item.Entries.OrderBy((SubsidiaryLedgerEntry t) => t.Voucher.Day).ThenBy((SubsidiaryLedgerEntry s) => s.Voucher.Type.Name).ThenBy((SubsidiaryLedgerEntry m) => m.Voucher.Number, StringNumberComparer.Instance)
				.ToList();
			if (TotalFlag.HasFlag(TotalFlag.Data))
			{
				foreach (SubsidiaryLedgerEntry item2 in list)
				{
					num++;
					xLSheet[num, 0].SetValue(item2.Voucher.Day, styleDateTime);
					xLSheet[num, 1].SetValue(item2.Voucher.Type.Name, styleBorder);
					xLSheet[num, 2].SetValue(item2.Voucher.Number, styleBorder);
					xLSheet[num, 3].SetValue(item2.Voucher.Digest, styleBorder);
					xLSheet[num, 4].SetValue(string.Join(",", item2.Voucher.OppositeAccounts.Select((Account t) => t.Name).Distinct()), styleBorder);
					xLSheet[num, 5].SetValue(item2.Voucher.IsDebit ? ((object)item2.Voucher.Amount) : "", styleMoney);
					xLSheet[num, 6].SetValue(item2.Voucher.IsDebit ? "" : ((object)item2.Voucher.Amount), styleMoney);
					xLSheet[num, 7].SetValue(LedgerExporter.GetDCChar(account.IsDebit, item2.Balance), styleHCenter);
					xLSheet[num, 8].SetValue(LedgerExporter.EmptyIf0(Math.Abs(item2.Balance)), styleMoney);
				}
			}
			if (TotalFlag.HasFlag(TotalFlag.MonthSum))
			{
				num++;
				xLSheet[num, 0].SetValue("", styleBorder);
				xLSheet[num, 1].SetValue("", styleBorder);
				xLSheet[num, 2].SetValue("", styleBorder);
				xLSheet[num, 3].SetValue($"本月合计（{item.Month}月）", styleBorder);
				xLSheet[num, 4].SetValue("", styleBorder);
				xLSheet[num, 5].SetValue(item.Total.Debit, styleMoney);
				xLSheet[num, 6].SetValue(item.Total.Credit, styleMoney);
				xLSheet[num, 7].SetValue(LedgerExporter.GetDCChar(account.IsDebit, item.Total.Balance), styleHCenter);
				xLSheet[num, 8].SetValue(LedgerExporter.EmptyIf0(Math.Abs(item.Total.Balance)), styleMoney);
			}
			if (TotalFlag.HasFlag(TotalFlag.YearSum))
			{
				num++;
				xLSheet[num, 0].SetValue("", styleBorder);
				xLSheet[num, 1].SetValue("", styleBorder);
				xLSheet[num, 2].SetValue("", styleBorder);
				xLSheet[num, 3].SetValue($"本年累计（{item.Month}月）", styleBorder);
				xLSheet[num, 4].SetValue("", styleBorder);
				xLSheet[num, 5].SetValue(item.GrandTotal.Debit, styleMoney);
				xLSheet[num, 6].SetValue(item.GrandTotal.Credit, styleMoney);
				xLSheet[num, 7].SetValue(LedgerExporter.GetDCChar(account.IsDebit, item.GrandTotal.Balance), styleHCenter);
				xLSheet[num, 8].SetValue(LedgerExporter.EmptyIf0(Math.Abs(item.GrandTotal.Balance)), styleMoney);
			}
		}
		foreach (XLRow item3 in (IEnumerable)xLSheet.Rows)
		{
			item3.Height = C1XLBook.PixelsToTwips(30.0);
		}
	}
}
