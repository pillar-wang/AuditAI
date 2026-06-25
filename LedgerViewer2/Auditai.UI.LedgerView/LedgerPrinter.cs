using System;
using System.Drawing;
using System.Drawing.Printing;
using C1.C1Preview;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public abstract class LedgerPrinter
{
	protected const string DEFAULTFONTNAME = "微软雅黑";

	protected const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	protected C1PrintDocument _pd;

	protected Ledger _ledger;

	protected Font DefaultFont = new Font("微软雅黑", 9f);

	protected Font DefaultHeadFont = new Font("微软雅黑", 12f);

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public bool Landscape { get; set; }

	public C1PrintDocument PrintDocument => _pd;

	public LedgerPrinter(Ledger ledger)
	{
		_ledger = ledger;
		StartDate = ledger.StartDate;
		EndDate = ledger.GetEndDate();
		_pd = new C1PrintDocument();
		_pd.PageLayout.PageSettings.PaperKind = PaperKind.A4;
		_pd.PageLayout.PageSettings.RightMargin = new Unit(10.0, UnitTypeEnum.Mm);
	}

	protected virtual void CreateHeader()
	{
		RenderTable pageHeader = new RenderTable();
		_pd.PageLayout.PageHeader = pageHeader;
	}

	protected virtual void CreateFooter()
	{
		RenderTable renderTable = new RenderTable();
		TableCell tableCell = renderTable.Rows[0][0];
		tableCell.Text = "第[PageNo]页";
		tableCell.Style.TextAlignHorz = AlignHorzEnum.Center;
		tableCell.Style.TextAlignVert = AlignVertEnum.Bottom;
		_pd.PageLayout.PageFooter = renderTable;
	}

	protected abstract void CreateData();

	public void Build()
	{
		_pd.PageLayout.PageSettings.Landscape = Landscape;
		CreateHeader();
		CreateFooter();
		CreateData();
	}

	protected string GetDCChar(bool isDebit, decimal balance)
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
