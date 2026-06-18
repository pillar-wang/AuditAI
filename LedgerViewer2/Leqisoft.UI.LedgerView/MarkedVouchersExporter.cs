using System.Collections;
using System.Collections.Generic;
using System.Linq;
using C1.C1Excel;
using Leqisoft.Model;

namespace Leqisoft.UI.LedgerView;

internal class MarkedVouchersExporter : LedgerExporter
{
	private const int CN_DATE = 0;

	private const int CN_TYPE = 1;

	private const int CN_NUMBER = 2;

	private const int CN_DIGEST = 3;

	private const int CN_CODE = 4;

	private const int CN_NAME = 5;

	private const int CN_OPPOSITE = 6;

	private const int CN_DEBIT = 7;

	private const int CN_CREDIT = 8;

	private const int CN_MAKER = 9;

	private const int CN_CHECKER = 10;

	private const int CN_BOOKER = 11;

	private Ledger _ledger;

	private List<Voucher> _vouchers;

	public MarkedVouchersExporter(Ledger ledger, IEnumerable<Voucher> vouchers)
	{
		_ledger = ledger;
		_vouchers = vouchers.ToList();
	}

	public override void Build()
	{
		XLSheet xLSheet = xlBook.Sheets[0];
		int num = 0;
		xLSheet[num, 0].SetValue("日期", styleHCenter);
		xLSheet.Columns[0].Width = C1XLBook.PixelsToTwips(90.0);
		xLSheet[num, 1].SetValue("字", styleHCenter);
		xLSheet[num, 2].SetValue("号", styleHCenter);
		xLSheet[num, 3].SetValue("摘要", styleHCenter);
		xLSheet.Columns[3].Width = C1XLBook.PixelsToTwips(250.0);
		xLSheet[num, 4].SetValue("科目代码", styleHCenter);
		xLSheet[num, 5].SetValue("科目名称", styleHCenter);
		xLSheet.Columns[5].Width = C1XLBook.PixelsToTwips(110.0);
		xLSheet[num, 6].SetValue("对方科目", styleHCenter);
		xLSheet.Columns[6].Width = C1XLBook.PixelsToTwips(110.0);
		xLSheet[num, 7].SetValue("借方金额", styleHCenter);
		xLSheet.Columns[7].Width = C1XLBook.PixelsToTwips(110.0);
		xLSheet[num, 8].SetValue("贷方金额", styleHCenter);
		xLSheet.Columns[8].Width = C1XLBook.PixelsToTwips(110.0);
		xLSheet[num, 9].SetValue("制单人", styleHCenter);
		xLSheet[num, 10].SetValue("审核人", styleHCenter);
		xLSheet[num, 11].SetValue("记账人", styleHCenter);
		foreach (Voucher voucher in _vouchers)
		{
			num++;
			xLSheet[num, 0].SetValue(voucher.Day, styleDateTime);
			xLSheet[num, 1].SetValue(voucher.Type.Name, styleBorder);
			xLSheet[num, 2].SetValue(voucher.Number, styleBorder);
			xLSheet[num, 3].SetValue(voucher.Digest, styleBorder);
			xLSheet[num, 4].SetValue(voucher.GetDisplayAccountCodeWithDetail(), styleBorder);
			xLSheet[num, 5].SetValue(voucher.GetDisplayAccountNameWithDetail(), styleBorder);
			xLSheet[num, 6].SetValue(string.Join(",", voucher.OppositeAccounts.Select((Account o) => o.Name).Distinct()), styleBorder);
			xLSheet[num, 7].SetValue(voucher.IsDebit ? LedgerExporter.EmptyIf0(voucher.Amount) : string.Empty, styleMoney);
			xLSheet[num, 8].SetValue(voucher.IsDebit ? string.Empty : LedgerExporter.EmptyIf0(voucher.Amount), styleMoney);
			xLSheet[num, 9].SetValue(voucher.Maker, styleBorder);
			xLSheet[num, 10].SetValue(voucher.Checker, styleBorder);
			xLSheet[num, 11].SetValue(voucher.Booker, styleBorder);
		}
		foreach (XLRow item in (IEnumerable)xLSheet.Rows)
		{
			item.Height = C1XLBook.PixelsToTwips(30.0);
		}
	}
}
