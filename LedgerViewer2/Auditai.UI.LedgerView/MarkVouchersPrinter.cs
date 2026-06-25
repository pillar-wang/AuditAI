using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using C1.C1Preview;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public class MarkVouchersPrinter : LedgerPrinter
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

	private List<Voucher> _vouchers;

	public MarkVouchersPrinter(Ledger ledger, IEnumerable<Voucher> vouchers)
		: base(ledger)
	{
		_vouchers = vouchers.ToList();
	}

	protected override void CreateData()
	{
		_pd.StartDoc();
		RenderTable renderTable = new RenderTable();
		renderTable.Width = Unit.Auto;
		renderTable.SplitHorzBehavior = SplitBehaviorEnum.Never;
		renderTable.ColumnSizingMode = TableSizingModeEnum.Fixed;
		renderTable.RowSizingMode = TableSizingModeEnum.Auto;
		renderTable.StretchColumns = StretchTableEnum.AllVectors;
		renderTable.Style.GridLines.All = LineDef.Default;
		renderTable.Style.Font = new Font("微软雅黑", 9f);
		renderTable.RowGroups[0, 1].PageHeader = true;
		TableRow tableRow = renderTable.Rows[0];
		tableRow[0].Text = "日期";
		tableRow[1].Text = "字";
		tableRow[2].Text = "号";
		tableRow[3].Text = "摘要";
		tableRow[4].Text = "科目代码";
		tableRow[5].Text = "科目名称";
		tableRow[6].Text = "对方科目";
		tableRow[7].Text = "借方金额";
		tableRow[8].Text = "贷方金额";
		tableRow[9].Text = "制单人";
		tableRow[10].Text = "审核人";
		tableRow[11].Text = "记账人";
		tableRow.Style.TextAlignHorz = AlignHorzEnum.Center;
		int num = 0;
		foreach (Voucher voucher in _vouchers)
		{
			tableRow = renderTable.Rows[++num];
			tableRow[0].Text = voucher.Day.ToString("yyyy-MM-dd");
			tableRow[1].Text = voucher.Type.Name;
			tableRow[2].Text = voucher.Number;
			tableRow[3].Text = voucher.Digest;
			tableRow[4].Text = voucher.GetDisplayAccountCodeWithDetail();
			tableRow[5].Text = voucher.GetDisplayAccountNameWithDetail();
			tableRow[6].Text = string.Join(",", voucher.OppositeAccounts.Select((Account t) => t.Name).Distinct());
			tableRow[7].Text = (voucher.IsDebit ? voucher.Amount : 0m).ToString("#,0.00;-#,0.00;#");
			tableRow[8].Text = (voucher.IsDebit ? 0m : voucher.Amount).ToString("#,0.00;-#,0.00;#");
			tableRow[9].Text = voucher.Maker;
			tableRow[10].Text = voucher.Checker;
			tableRow[11].Text = voucher.Booker;
		}
		renderTable.Cols[1].Style.TextAlignHorz = AlignHorzEnum.Center;
		renderTable.Cols[2].Style.TextAlignHorz = AlignHorzEnum.Center;
		renderTable.Cols[3].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[4].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[5].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[6].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[7].Style.TextAlignHorz = AlignHorzEnum.Right;
		renderTable.Cols[8].Style.TextAlignHorz = AlignHorzEnum.Right;
		renderTable.Cols[9].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[10].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[11].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Rows[0].Style.TextAlignHorz = AlignHorzEnum.Center;
		for (int i = 0; i < renderTable.Cols.Count; i++)
		{
			renderTable.Cols[i].Style.TextAlignVert = AlignVertEnum.Center;
			renderTable.Rows[0][i].Style.TextAlignHorz = AlignHorzEnum.Center;
			renderTable.Rows[0][i].Style.TextAlignVert = AlignVertEnum.Center;
		}
		renderTable.Cols[0].Width = new Unit(120.0, UnitTypeEnum.Pixel);
		renderTable.Cols[1].Width = new Unit(30.0, UnitTypeEnum.Pixel);
		renderTable.Cols[2].Width = new Unit(50.0, UnitTypeEnum.Pixel);
		renderTable.Cols[3].Width = new Unit(100.0, UnitTypeEnum.Pixel);
		renderTable.Cols[4].Width = new Unit(100.0, UnitTypeEnum.Pixel);
		renderTable.Cols[5].Width = new Unit(100.0, UnitTypeEnum.Pixel);
		renderTable.Cols[6].Width = new Unit(100.0, UnitTypeEnum.Pixel);
		renderTable.Cols[7].Width = new Unit(120.0, UnitTypeEnum.Pixel);
		renderTable.Cols[8].Width = new Unit(120.0, UnitTypeEnum.Pixel);
		renderTable.Cols[9].Width = new Unit(60.0, UnitTypeEnum.Pixel);
		renderTable.Cols[10].Width = new Unit(60.0, UnitTypeEnum.Pixel);
		renderTable.Cols[11].Width = new Unit(60.0, UnitTypeEnum.Pixel);
		_pd.RenderBlock(renderTable);
		_pd.EndDoc();
	}

	protected override void CreateHeader()
	{
		RenderTable renderTable = new RenderTable();
		TableRow tableRow = renderTable.Rows[0];
		tableRow[0].SpanCols = 3;
		tableRow[0].Text = "我的关注";
		tableRow[0].Style.Font = new Font("微软雅黑", 12f);
		tableRow.Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow.Style.TextAlignVert = AlignVertEnum.Center;
		tableRow.Height = new Unit(20.0, UnitTypeEnum.Mm);
		_pd.PageLayout.PageHeader = renderTable;
	}
}
