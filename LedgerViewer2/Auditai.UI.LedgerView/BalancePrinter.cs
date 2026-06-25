using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using C1.C1Preview;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public class BalancePrinter : LedgerPrinter
{
	private const int CN_CODE = 0;

	private const int CN_NAME = 1;

	private const int CN_BEGINDC = 2;

	private const int CN_BEGINBALANCE = 3;

	private const int CN_DEBIT = 4;

	private const int CN_CREDIT = 5;

	private const int CN_ENDDC = 6;

	private const int CN_ENDBALANCE = 7;

	private List<Account> _accounts;

	public BalancePrinter(Ledger ledger, IEnumerable<Account> accounts)
		: base(ledger)
	{
		_accounts = accounts.ToList();
	}

	protected override void CreateData()
	{
		_pd.StartDoc();
		TrialBalanceSheet sheet = _ledger.GetTrialBalanceSheet(base.StartDate, base.EndDate);
		RenderTable dt = new RenderTable();
		dt.Width = Unit.Auto;
		dt.SplitHorzBehavior = SplitBehaviorEnum.Never;
		dt.ColumnSizingMode = TableSizingModeEnum.Fixed;
		dt.RowSizingMode = TableSizingModeEnum.Auto;
		dt.StretchColumns = StretchTableEnum.AllVectors;
		dt.Style.GridLines.All = LineDef.Default;
		dt.Style.Font = new Font("微软雅黑", 9f);
		dt.RowGroups[0, 1].PageHeader = true;
		TableRow row = dt.Rows[0];
		row[0].Text = "科目代码";
		row[1].Text = "科目名称";
		row[2].Text = "期初余额方向";
		row[3].Text = "期初余额";
		row[4].Text = "借方发生额";
		row[5].Text = "贷方发生额";
		row[6].Text = "期末余额方向";
		row[7].Text = "期末余额";
		row.Style.TextAlignHorz = AlignHorzEnum.Center;
		int i = 0;
		foreach (Account account in _accounts)
		{
			row = dt.Rows[++i];
			row[0].Text = account.Code;
			row[1].Text = account.Name;
			row[2].Text = GetDCChar(account.IsDebit, sheet.Start[account].Total);
			row[3].Text = Math.Abs(sheet.Start[account].Total).ToString("#,0.00;-#,0.00;#");
			row[4].Text = (sheet.Debit.ContainsKey(account) ? sheet.Debit[account].Total : 0m).ToString("#,0.00;-#,0.00;#");
			row[5].Text = (sheet.Credit.ContainsKey(account) ? sheet.Credit[account].Total : 0m).ToString("#,0.00;-#,0.00;#");
			row[6].Text = GetDCChar(account.IsDebit, sheet.End[account].Total);
			row[7].Text = Math.Abs(sheet.End[account].Total).ToString("#,0.00;-#,0.00;#");
			appendChildren(account);
		}
		dt.Cols[2].Style.TextAlignHorz = AlignHorzEnum.Center;
		dt.Cols[3].Style.TextAlignHorz = AlignHorzEnum.Right;
		dt.Cols[5].Style.TextAlignHorz = AlignHorzEnum.Right;
		dt.Cols[4].Style.TextAlignHorz = AlignHorzEnum.Right;
		dt.Cols[6].Style.TextAlignHorz = AlignHorzEnum.Center;
		dt.Cols[7].Style.TextAlignHorz = AlignHorzEnum.Right;
		dt.Rows[0].Style.TextAlignHorz = AlignHorzEnum.Center;
		for (int j = 0; j < dt.Cols.Count; j++)
		{
			dt.Cols[j].Style.TextAlignVert = AlignVertEnum.Center;
			dt.Rows[0][j].Style.TextAlignHorz = AlignHorzEnum.Center;
			dt.Rows[0][j].Style.TextAlignVert = AlignVertEnum.Center;
		}
		dt.Cols[0].Width = new Unit(20.0, UnitTypeEnum.Mm);
		dt.Cols[1].Width = new Unit(20.0, UnitTypeEnum.Mm);
		dt.Cols[2].Width = new Unit(10.0, UnitTypeEnum.Mm);
		dt.Cols[3].Width = new Unit(20.0, UnitTypeEnum.Mm);
		dt.Cols[4].Width = new Unit(20.0, UnitTypeEnum.Mm);
		dt.Cols[5].Width = new Unit(20.0, UnitTypeEnum.Mm);
		dt.Cols[6].Width = new Unit(10.0, UnitTypeEnum.Mm);
		dt.Cols[7].Width = new Unit(20.0, UnitTypeEnum.Mm);
		_pd.RenderBlock(dt);
		_pd.EndDoc();
		void appendChildren(Account account)
		{
			foreach (Account child in account.Children)
			{
				row = dt.Rows[++i];
				row[0].Text = child.Code;
				row[1].Text = child.Name;
				row[2].Text = GetDCChar(child.IsDebit, sheet.Start[child].Total);
				row[3].Text = Math.Abs(sheet.Start[child].Total).ToString("#,0.00;-#,0.00;#");
				row[4].Text = (sheet.Debit.ContainsKey(child) ? sheet.Debit[child].Total : 0m).ToString("#,0.00;-#,0.00;#");
				row[5].Text = (sheet.Credit.ContainsKey(child) ? sheet.Credit[child].Total : 0m).ToString("#,0.00;-#,0.00;#");
				row[6].Text = GetDCChar(child.IsDebit, sheet.End[child].Total);
				row[7].Text = Math.Abs(sheet.End[child].Total).ToString("#,0.00;-#,0.00;#");
				appendChildren(child);
			}
		}
	}

	protected override void CreateHeader()
	{
		RenderTable renderTable = new RenderTable();
		TableRow tableRow = renderTable.Rows[0];
		tableRow[0].SpanCols = 3;
		tableRow[0].Text = "科目余额表";
		tableRow[0].Style.Font = new Font("微软雅黑", 12f);
		tableRow.Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow.Style.TextAlignVert = AlignVertEnum.Center;
		tableRow.Height = new Unit(20.0, UnitTypeEnum.Mm);
		tableRow = renderTable.Rows[1];
		tableRow[0].Text = "核算单位：" + _ledger.CompanyName;
		tableRow[1].Text = base.StartDate.ToString("yyyy-MM-dd") + "至" + base.EndDate.ToString("yyyy-MM-dd");
		tableRow[2].Text = "金额单位：" + (_ledger.BaseCurrency?.Name ?? "人民币元");
		tableRow[0].Style.TextAlignHorz = AlignHorzEnum.Left;
		tableRow[1].Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow[2].Style.TextAlignHorz = AlignHorzEnum.Right;
		_pd.PageLayout.PageHeader = renderTable;
	}
}
