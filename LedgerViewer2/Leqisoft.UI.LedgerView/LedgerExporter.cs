using System;
using System.Drawing;
using C1.C1Excel;

namespace Leqisoft.UI.LedgerView;

public abstract class LedgerExporter
{
	public C1XLBook xlBook;

	protected XLStyle styleDateTime;

	protected XLStyle styleMoney;

	protected XLStyle styleBorder;

	protected XLStyle styleHCenter;

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public abstract void Build();

	internal LedgerExporter()
	{
		xlBook = new C1XLBook();
		xlBook.DefaultFont = new Font("微软雅黑", 9f);
		styleDateTime = new XLStyle(xlBook)
		{
			Format = XLStyle.FormatDotNetToXL("yyyy-MM-dd", typeof(DateTime)),
			AlignHorz = XLAlignHorzEnum.Left
		};
		styleDateTime.SetBorderStyle(XLLineStyleEnum.Thin);
		styleMoney = new XLStyle(xlBook)
		{
			Format = XLStyle.FormatDotNetToXL("#,###.00", typeof(decimal))
		};
		styleMoney.SetBorderStyle(XLLineStyleEnum.Thin);
		styleBorder = new XLStyle(xlBook);
		styleBorder.SetBorderStyle(XLLineStyleEnum.Thin);
		styleHCenter = new XLStyle(xlBook)
		{
			AlignHorz = XLAlignHorzEnum.Center
		};
		styleHCenter.SetBorderStyle(XLLineStyleEnum.Thin);
	}

	protected static string GetDCChar(bool isDebit, decimal balance)
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

	protected static object EmptyIf0(decimal d)
	{
		if (!(d == 0m))
		{
			return d;
		}
		return string.Empty;
	}
}
