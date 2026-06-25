using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using C1.C1Preview;
using Auditai.DTO;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public class SubsidiaryPrinter : LedgerPrinter
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

	private SubsidiaryLedger _subsidiary;

	private object _auxiliary;

	public TotalFlag TotalFlag { get; set; } = TotalFlag.Data;


	public SubsidiaryPrinter(Ledger ledger, SubsidiaryLedger subsidiary, object auxiliary)
		: base(ledger)
	{
		_subsidiary = subsidiary;
		_auxiliary = auxiliary;
	}

	protected override void CreateData()
	{
		_pd.StartDoc();
		Account account = _subsidiary.Account;
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
		tableRow[4].Text = "对方科目";
		tableRow[5].Text = "借方金额";
		tableRow[6].Text = "贷方金额";
		tableRow[7].Text = "方向";
		tableRow[8].Text = "余额";
		tableRow.Style.TextAlignHorz = AlignHorzEnum.Center;
		int num = 0;
		if (_subsidiary.BeginBalance != 0m)
		{
			tableRow = renderTable.Rows[++num];
			tableRow[3].Text = "期初余额";
			tableRow[7].Text = GetDCChar(account.IsDebit, _subsidiary.BeginBalance);
			tableRow[8].Text = Math.Abs(_subsidiary.BeginBalance).ToString("#,0.00;-#,0.00;#");
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
					tableRow = renderTable.Rows[++num];
					tableRow[0].Text = item2.Voucher.Day.ToString("yyyy-MM-dd");
					tableRow[1].Text = item2.Voucher.Type.Name;
					tableRow[2].Text = item2.Voucher.Number;
					tableRow[3].Text = item2.Voucher.Digest;
					tableRow[4].Text = string.Join(",", item2.Voucher.OppositeAccounts.Select((Account t) => t.Name).Distinct());
					tableRow[5].Text = (item2.Voucher.IsDebit ? item2.Voucher.Amount : 0m).ToString("#,0.00;-#,0.00;#");
					tableRow[6].Text = (item2.Voucher.IsDebit ? 0m : item2.Voucher.Amount).ToString("#,0.00;-#,0.00;#");
					tableRow[7].Text = GetDCChar(account.IsDebit, item2.Balance);
					tableRow[8].Text = Math.Abs(item2.Balance).ToString("#,0.00;-#,0.00;#");
				}
			}
			if (TotalFlag.HasFlag(TotalFlag.MonthSum))
			{
				tableRow = renderTable.Rows[++num];
				tableRow[3].Text = $"本月合计（{item.Month}月）";
				tableRow[5].Text = item.Total.Debit.ToString("#,0.00;-#,0.00;#");
				tableRow[6].Text = item.Total.Credit.ToString("#,0.00;-#,0.00;#");
				tableRow[7].Text = GetDCChar(account.IsDebit, item.Total.Balance);
				tableRow[8].Text = Math.Abs(item.Total.Balance).ToString("#,0.00;-#,0.00;#");
			}
			if (TotalFlag.HasFlag(TotalFlag.YearSum))
			{
				tableRow = renderTable.Rows[++num];
				tableRow[3].Text = $"本年累计（{item.Month}月）";
				tableRow[5].Text = item.GrandTotal.Debit.ToString("#,0.00;-#,0.00;#");
				tableRow[6].Text = item.GrandTotal.Credit.ToString("#,0.00;-#,0.00;#");
				tableRow[7].Text = GetDCChar(account.IsDebit, item.GrandTotal.Balance);
				tableRow[8].Text = Math.Abs(item.GrandTotal.Balance).ToString("#,0.00;-#,0.00;#");
			}
		}
		renderTable.Cols[1].Style.TextAlignHorz = AlignHorzEnum.Center;
		renderTable.Cols[2].Style.TextAlignHorz = AlignHorzEnum.Center;
		renderTable.Cols[5].Style.TextAlignHorz = AlignHorzEnum.Right;
		renderTable.Cols[6].Style.TextAlignHorz = AlignHorzEnum.Right;
		renderTable.Cols[7].Style.TextAlignHorz = AlignHorzEnum.Center;
		renderTable.Cols[8].Style.TextAlignHorz = AlignHorzEnum.Right;
		renderTable.Rows[0].Style.TextAlignHorz = AlignHorzEnum.Center;
		for (int i = 0; i < renderTable.Cols.Count; i++)
		{
			renderTable.Cols[i].Style.TextAlignVert = AlignVertEnum.Center;
			renderTable.Rows[0][i].Style.TextAlignHorz = AlignHorzEnum.Center;
			renderTable.Rows[0][i].Style.TextAlignVert = AlignVertEnum.Center;
		}
		renderTable.Cols[0].Width = new Unit(16.0, UnitTypeEnum.Mm);
		renderTable.Cols[1].Width = new Unit(10.0, UnitTypeEnum.Mm);
		renderTable.Cols[2].Width = new Unit(10.0, UnitTypeEnum.Mm);
		renderTable.Cols[3].Width = new Unit(20.0, UnitTypeEnum.Mm);
		renderTable.Cols[4].Width = new Unit(20.0, UnitTypeEnum.Mm);
		renderTable.Cols[5].Width = new Unit(20.0, UnitTypeEnum.Mm);
		renderTable.Cols[6].Width = new Unit(20.0, UnitTypeEnum.Mm);
		renderTable.Cols[7].Width = new Unit(10.0, UnitTypeEnum.Mm);
		renderTable.Cols[8].Width = new Unit(20.0, UnitTypeEnum.Mm);
		_pd.RenderBlock(renderTable);
		_pd.EndDoc();
	}

	protected override void CreateHeader()
	{
		RenderTable renderTable = new RenderTable();
		TableRow tableRow = renderTable.Rows[0];
		tableRow[0].SpanCols = 3;
		tableRow[0].Text = (TotalFlag.HasFlag(TotalFlag.Data) ? "明细账" : "总账");
		tableRow[0].Style.Font = new Font("微软雅黑", 12f);
		tableRow.Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow.Style.TextAlignVert = AlignVertEnum.Center;
		tableRow.Height = new Unit(20.0, UnitTypeEnum.Mm);
		string text = string.Empty;
		object auxiliary = _auxiliary;
		AuxiliaryClass auxiliaryClass = auxiliary as AuxiliaryClass;
		if (auxiliaryClass == null && auxiliary is AuxiliaryItem auxiliaryItem)
		{
			text = "|（" + auxiliaryItem.Code + "）" + auxiliaryItem.Name;
		}
		tableRow = renderTable.Rows[1];
		tableRow[0].Text = "科目名称：" + FullNameDisplay(_subsidiary.Account) + text;
		tableRow[1].Text = base.StartDate.ToString("yyyy-MM-dd") + "至" + base.EndDate.ToString("yyyy-MM-dd");
		tableRow[2].Text = "金额单位：" + (_subsidiary.Account.Ledger.BaseCurrency?.Name ?? "人民币元");
		tableRow[0].Style.TextAlignHorz = AlignHorzEnum.Left;
		tableRow[1].Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow[2].Style.TextAlignHorz = AlignHorzEnum.Right;
		_pd.PageLayout.PageHeader = renderTable;
	}

	private string FullNameDisplay(Account account)
	{
		Account account2 = account;
		string code = account2.Code;
		string text = account2.Name;
		while ((account2 = account2.Parent) != null)
		{
			text = string.Join("-", account2.Name, text);
		}
		return "（" + code + "）" + text;
	}
}
