using System.Collections.Generic;
using System.Linq;
using C1.C1Preview;
using Leqisoft.Model;

namespace Leqisoft.UI.LedgerView;

public class VoucherPrinter : LedgerPrinter
{
	private List<Voucher> _vouchers;

	public VoucherPrinter(Ledger ledger, IEnumerable<Voucher> vouchers)
		: base(ledger)
	{
		_vouchers = vouchers.ToList();
		_pd.PageLayout.PageSettings.Landscape = true;
	}

	protected override void CreateData()
	{
		VoucherPages pages = GetPages();
		_pd.AllowNonReflowableDocs = true;
		_pd.StartDoc();
		try
		{
			foreach (KeyValuePair<int, VoucherPage> item in pages)
			{
				if (base.Landscape)
				{
					RenderTable head;
					RenderTable foot;
					RenderTable ro = LandscapeTable(item.Value, out head, out foot);
					_pd.RenderBlock(head);
					_pd.RenderBlock(ro);
					_pd.RenderBlock(foot);
					_pd.NewPage();
					continue;
				}
				RenderTable head2;
				RenderTable foot2;
				RenderTable ro2 = PortraitTable(item.Value, out head2, out foot2);
				_pd.RenderBlock(head2);
				_pd.RenderBlock(ro2);
				_pd.RenderBlock(foot2);
				if (item.Key % 2 == 0 && item.Key < pages.Count)
				{
					_pd.NewPage();
				}
				if (item.Key % 2 == 1)
				{
					_pd.RenderBlock(EmptyText());
				}
			}
		}
		finally
		{
			_pd.EndDoc();
		}
	}

	private RenderText EmptyText()
	{
		RenderText renderText = new RenderText();
		renderText.Text = " ";
		renderText.Height = new Unit(5.0, UnitTypeEnum.Mm);
		return renderText;
	}

	private VoucherPages GetPages()
	{
		VoucherPages voucherPages = new VoucherPages();
		int num = 0;
		IEnumerable<IGrouping<string, Voucher>> enumerable = from v in _vouchers
			group v by v.Day.ToShortDateString() + v.Number + v.Type.Name;
		foreach (IGrouping<string, Voucher> item in enumerable)
		{
			num++;
			int num2 = 0;
			int num3 = 0;
			foreach (Voucher item2 in item)
			{
				if (num2 >= 5)
				{
					num2 = 0;
					num++;
				}
				if (!voucherPages.ContainsKey(num))
				{
					voucherPages.Add(num, new VoucherPage
					{
						PageNo = num,
						IndexOfVoucher = num3 / 5 + 1,
						PageCountOfVoucher = item.Count() / 5 + 1
					});
				}
				voucherPages[num].Vouchers.Add(item2);
				num2++;
				num3++;
			}
		}
		return voucherPages;
	}

	private RenderTable NewTable(VoucherPage page, out RenderTable head, out RenderTable foot)
	{
		Voucher voucher = page.Vouchers.First();
		head = new RenderTable();
		head.Style.Font = DefaultHeadFont;
		head.Style.GridLines.All = LineDef.Empty;
		TableRow tableRow = head.Rows[0];
		tableRow.Height = new Unit(15.0, UnitTypeEnum.Mm);
		tableRow[0].SpanCols = 3;
		tableRow[0].Text = "记账凭证";
		tableRow[0].Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow[0].Style.TextAlignVert = AlignVertEnum.Center;
		tableRow[3].Text = " ";
		head.Cols[3].Width = new Unit(3.0, UnitTypeEnum.Mm);
		tableRow = head.Rows[1];
		tableRow.Height = new Unit(6.0, UnitTypeEnum.Mm);
		tableRow.Style.Font = DefaultFont;
		tableRow[0].Text = string.Empty;
		tableRow[1].Text = $"{voucher.Day:yyyy年MM月dd日}";
		string text = "第" + voucher.Number + ((page.PageCountOfVoucher > 1) ? $"-{page.IndexOfVoucher}" : string.Empty) + "号";
		tableRow[2].Text = voucher.Type.Name + "字" + text;
		tableRow[0].Style.TextAlignHorz = AlignHorzEnum.Left;
		tableRow[1].Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow[2].Style.TextAlignHorz = AlignHorzEnum.Right;
		foot = new RenderTable();
		foot.Height = new Unit(6.0, UnitTypeEnum.Mm);
		foot.Style.Font = DefaultFont;
		foot.Style.GridLines.All = LineDef.Empty;
		tableRow = foot.Rows[0];
		tableRow[0].Text = "记账人：" + voucher.Booker;
		tableRow[1].Text = "审核人：" + voucher.Checker;
		tableRow[2].Text = "制单人：" + voucher.Maker;
		tableRow[0].Style.TextAlignHorz = AlignHorzEnum.Left;
		tableRow[1].Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow[2].Style.TextAlignHorz = AlignHorzEnum.Right;
		tableRow.Style.TextAlignVert = AlignVertEnum.Center;
		tableRow[3].Text = " ";
		foot.Cols[3].Width = new Unit(3.0, UnitTypeEnum.Mm);
		RenderTable renderTable = new RenderTable
		{
			Width = Unit.Auto,
			RowSizingMode = TableSizingModeEnum.Auto,
			ColumnSizingMode = TableSizingModeEnum.Fixed,
			SplitHorzBehavior = SplitBehaviorEnum.Never,
			StretchColumns = StretchTableEnum.AllVectors
		};
		renderTable.Style.Font = DefaultFont;
		renderTable.Style.GridLines.All = LineDef.Default;
		renderTable.Rows.Count = 6;
		renderTable.Cols.Count = 6;
		for (int i = 0; i < 6; i++)
		{
			renderTable.Rows[i][0].Text = " ";
		}
		int num = 0;
		tableRow = renderTable.Rows[num++];
		tableRow[0].Text = "摘要";
		tableRow[1].Text = "科目代码";
		tableRow[2].Text = "科目名称";
		tableRow[3].Text = "借方金额";
		tableRow[4].Text = "贷方金额";
		tableRow[5].SpanRows = 6;
		tableRow[5].Text = $"附件{voucher.NumAttachments}张";
		tableRow[5].Style.Borders.Top = LineDef.Empty;
		tableRow[5].Style.Borders.Right = LineDef.Empty;
		tableRow[5].Style.Borders.Bottom = LineDef.Empty;
		tableRow[5].Style.TextAlignHorz = AlignHorzEnum.Center;
		tableRow[5].Style.TextAlignVert = AlignVertEnum.Center;
		foreach (Voucher voucher2 in page.Vouchers)
		{
			tableRow = renderTable.Rows[num++];
			tableRow[0].Text = voucher2.Digest;
			tableRow[1].Text = voucher2.GetDisplayAccountCodeWithDetail();
			tableRow[2].Text = voucher2.GetDisplayAccountNameWithDetail();
			tableRow[3].Text = (voucher2.IsDebit ? voucher2.Amount : 0m).ToString("#,0.00;-#,0.00;#");
			tableRow[4].Text = (voucher2.IsDebit ? 0m : voucher2.Amount).ToString("#,0.00;-#,0.00;#");
		}
		renderTable.Cols[0].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[1].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[2].Style.TextAlignHorz = AlignHorzEnum.Left;
		renderTable.Cols[3].Style.TextAlignHorz = AlignHorzEnum.Right;
		renderTable.Cols[4].Style.TextAlignHorz = AlignHorzEnum.Right;
		for (int j = 0; j < renderTable.Rows.Count; j++)
		{
			renderTable.Rows[j].Style.TextAlignVert = AlignVertEnum.Center;
		}
		for (int k = 0; k < renderTable.Cols.Count; k++)
		{
			renderTable.Rows[0][k].Style.TextAlignHorz = AlignHorzEnum.Center;
		}
		renderTable.Cols[0].Width = new Unit(30.0, UnitTypeEnum.Mm);
		renderTable.Cols[1].Width = new Unit(20.0, UnitTypeEnum.Mm);
		renderTable.Cols[2].Width = new Unit(30.0, UnitTypeEnum.Mm);
		renderTable.Cols[3].Width = new Unit(20.0, UnitTypeEnum.Mm);
		renderTable.Cols[4].Width = new Unit(20.0, UnitTypeEnum.Mm);
		renderTable.Cols[5].Width = new Unit(3.0, UnitTypeEnum.Mm);
		return renderTable;
	}

	private RenderTable PortraitTable(VoucherPage page, out RenderTable head, out RenderTable foot)
	{
		RenderTable renderTable = NewTable(page, out head, out foot);
		for (int i = 0; i < 6; i++)
		{
			renderTable.Rows[i].Height = new Unit(14.0, UnitTypeEnum.Mm);
		}
		renderTable.Cols[5].Width = new Unit(3.0, UnitTypeEnum.Mm);
		return renderTable;
	}

	private RenderTable LandscapeTable(VoucherPage page, out RenderTable head, out RenderTable foot)
	{
		RenderTable renderTable = NewTable(page, out head, out foot);
		for (int i = 0; i < 6; i++)
		{
			renderTable.Rows[i].Height = new Unit(22.0, UnitTypeEnum.Mm);
		}
		renderTable.Cols[5].Width = new Unit(2.0, UnitTypeEnum.Mm);
		return renderTable;
	}

	private string GetTotalName(Voucher voucher)
	{
		Account account = voucher.Account;
		while (account.Parent != null)
		{
			account = account.Parent;
		}
		return account.Code + " " + account.Name;
	}

	private string GetDetailName(Voucher voucher)
	{
		string text = getFullName(voucher.Account);
		string[] array = text.Split('-');
		string text2 = string.Empty;
		if (array.Count() > 1)
		{
			text2 = voucher.Account.Code + " " + text.Remove(0, array[0].Length).Trim('-');
		}
		string text3 = string.Join("|", voucher.Details.Select((AuxiliaryItem t) => t.Code + " " + t.Name));
		return text2 + ((!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3)) ? "|" : "") + text3;
		string getFullName(Account account)
		{
			Account account2 = voucher.Account;
			string text4 = account2.Name;
			while ((account2 = account2.Parent) != null)
			{
				text4 = string.Join("-", account2.Name, text4);
			}
			return text4;
		}
	}
}
