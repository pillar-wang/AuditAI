using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

public class ValidateEditor : ISetTheme
{
	public enum ValidateEnum
	{
		Account,
		Balance,
		Voucher
	}

	private const string CN_INDEX = "index";

	private const string CN_CODE = "code";

	private const string CN_NAME = "name";

	private const string CN_TIP = "tip";

	private const string CN_BALANCE = "balance";

	private const string CN_DATE = "date";

	private const string CN_TYPE = "type";

	private const string CN_NUMBER = "number";

	private const string CN_DIGEST = "digest";

	private const string CN_DEBIT = "debit";

	private const string CN_CREDIT = "credit";

	private const string DATETIMEFORMATE = "yyyy-MM-dd";

	private const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	private C1Command cmdExpandAll = new C1Command();

	private C1CommandLink lnkExpandAll = new C1CommandLink();

	private C1Command cmdCollaspeAll = new C1Command();

	private C1CommandLink lnkCollaspeAll = new C1CommandLink();

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkCopy = new C1CommandLink();

	private LedgerViewer _owner;

	private IEnumerable<ValidateResult> retAccount;

	private IEnumerable<ValidateResult> retBalance;

	private IEnumerable<ValidateResult> retVoucher;

	private C1SplitterPanel pnlValidateTitle;

	private C1SplitterPanel pnlValidateGrid;

	internal C1FlexGridEx grdValidate;

	internal C1Label lblValidateTitle;

	private System.Drawing.Image zb1Image = Resources.zb1;

	private System.Drawing.Image dirImage = Resources.TreeDir;

	private System.Drawing.Image vouImage = Resources.vouchers16;

	private C1ContextMenu ctxTreeCell = new C1ContextMenu();

	private C1ContextMenu ctxViewCell = new C1ContextMenu();

	private C1ContextMenu ctxViewFixed = new C1ContextMenu();

	private C1ContextMenu ctxViewEmpty = new C1ContextMenu();

	public C1FlexGridEx Tree { get; private set; }

	private Ledger Ledger => _owner.Ledger;

	private DateTime StartDate => _owner.StartDate;

	private DateTime EndDate => _owner.EndDate;

	public C1SplitContainer View { get; private set; }

	public event EventHandler<ValidateEnum> ValidateChanged;

	public ValidateEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitComponent();
		Initialize();
		grdValidate.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdValidate.DrawFormBorder(e1.Graphics);
		};
		Tree.DoubleClick += ValidateTree_DoubleClick;
		ValidateChanged += ValidateEditor_ValidateChanged;
	}

	private void InitComponent()
	{
		View = new C1SplitContainer();
		pnlValidateTitle = new C1SplitterPanel();
		pnlValidateGrid = new C1SplitterPanel();
		lblValidateTitle = new C1Label();
		grdValidate = new C1FlexGridEx();
		lblValidateTitle.TextDetached = true;
		lblValidateTitle.BorderStyle = BorderStyle.None;
		lblValidateTitle.Dock = DockStyle.Fill;
		lblValidateTitle.Text = "科目设置检查结果";
		lblValidateTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlValidateTitle.Height = 30;
		pnlValidateTitle.KeepRelativeSize = false;
		pnlValidateTitle.Location = new Point(0, 0);
		pnlValidateTitle.MinHeight = 30;
		pnlValidateTitle.Resizable = false;
		pnlValidateTitle.Size = new Size(927, 30);
		pnlValidateTitle.SizeRatio = 4.769;
		pnlValidateTitle.Controls.Add(lblValidateTitle);
		grdValidate.AllowMerging = AllowMergingEnum.Custom;
		grdValidate.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdValidate.Dock = DockStyle.Fill;
		grdValidate.DrawMode = DrawModeEnum.OwnerDraw;
		grdValidate.Rows.DefaultSize = 20;
		grdValidate.Size = new Size(927, 599);
		grdValidate.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		pnlValidateGrid.Height = 599;
		pnlValidateGrid.Location = new Point(0, 31);
		pnlValidateGrid.Size = new Size(927, 599);
		pnlValidateGrid.Controls.Add(grdValidate);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.BackColor = Color.FromArgb(240, 240, 240);
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.ForeColor = Color.FromArgb(0, 0, 0);
		View.Panels.Add(pnlValidateTitle);
		View.Panels.Add(pnlValidateGrid);
		Tree = GridFactory.Create("tree");
		Tree.Paint += delegate(object s1, PaintEventArgs e1)
		{
			Tree.DrawFormBorder(e1.Graphics);
		};
	}

	public void Validate()
	{
		try
		{
			retAccount = new AccountValidate(Ledger.RootAccounts).Validate();
			retBalance = new BalanceValidate(_owner, Ledger.Accounts).Validate();
			retVoucher = new VoucherValidate(Ledger.Vouchers).Validate();
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
		try
		{
			Tree.BeginUpdate();
			Tree.Rows.Count = 0;
			Tree.Cols.Count = 0;
			Tree.Cols.Add();
			Tree.Rows.DefaultSize = 30;
			Tree.Tree.Column = 0;
			C1.Win.C1FlexGrid.Row row = Tree.Rows.Add();
			row.IsNode = true;
			row.Node.Data = $"科目设置检查（{retAccount.Count()}）";
			row.Node.Image = dirImage;
			row.Node.Key = retAccount;
			foreach (ValidateResult item in retAccount)
			{
				Account account = item.Key as Account;
				Node node = row.Node.AddNode(NodeTypeEnum.LastChild, account.Code + " " + account.Name);
				node.Key = item;
				node.Image = zb1Image;
			}
			C1.Win.C1FlexGrid.Row row2 = Tree.Rows.Add();
			row2.IsNode = true;
			row2.Node.Data = $"科目余额检查（{retBalance.Count()}）";
			row2.Node.Image = dirImage;
			row2.Node.Key = retBalance;
			foreach (ValidateResult item2 in retBalance)
			{
				Account account2 = item2.Key as Account;
				Node node2 = row2.Node.AddNode(NodeTypeEnum.LastChild, account2.Code + " " + account2.Name);
				node2.Key = item2;
				node2.Image = zb1Image;
			}
			C1.Win.C1FlexGrid.Row row3 = Tree.Rows.Add();
			row3.IsNode = true;
			row3.Node.Data = $"记账凭证检查（{retVoucher.Count()}）";
			row3.Node.Image = dirImage;
			row3.Node.Key = retVoucher;
			foreach (ValidateResult item3 in retVoucher)
			{
				Voucher voucher = item3.Key as Voucher;
				Node node3 = row3.Node.AddNode(NodeTypeEnum.LastChild, $"{voucher.Day.Month}月 {voucher.Type.Name}－{voucher.Number}");
				node3.Key = item3;
				node3.Image = vouImage;
			}
			Tree.AllowEditing = false;
			ValidateAccount(retAccount);
		}
		catch (Exception ex2)
		{
			ex2.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.ToString());
		}
		finally
		{
			Tree.EndUpdate();
		}
	}

	public void ValidRow(C1.Win.C1FlexGrid.Row row)
	{
		if (row.UserData is ValidateResult item)
		{
			if (row.Node.Parent.Key == retAccount)
			{
				ValidateAccount(new List<ValidateResult> { item });
			}
			else if (row.Node.Parent.Key == retBalance)
			{
				ValidateBalance(new List<ValidateResult> { item });
			}
			else if (row.Node.Parent.Key == retVoucher)
			{
				ValidateVoucher(new List<ValidateResult> { item });
			}
		}
		else if (row.Node.Key == retAccount)
		{
			ValidateAccount(retAccount);
		}
		else if (row.Node.Key == retBalance)
		{
			ValidateBalance(retBalance);
		}
		else if (row.Node.Key == retVoucher)
		{
			ValidateVoucher(retVoucher);
		}
	}

	public void SetTheme()
	{
		Tree.Styles.Alternate.BackColor = Color.Transparent;
		Tree.Styles.Fixed.Border.Width = 0;
		Tree.Styles.Normal.Border.Width = 0;
	}

	private void ValidateTree_DoubleClick(object sender, EventArgs e)
	{
		int mouseRow = Tree.MouseRow;
		if (mouseRow >= 0)
		{
			C1.Win.C1FlexGrid.Row row = Tree.Rows[mouseRow];
			ValidRow(row);
		}
	}

	private void CmdCopy_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		(sender as C1Command).Visible = grdValidate.Row >= grdValidate.Rows.Fixed && grdValidate.Col >= grdValidate.Cols.Fixed;
	}

	private void CmdCollaspeAll_Click(object sender, ClickEventArgs e)
	{
		Tree.Tree.Show(0);
	}

	private void CmdExpandAll_Click(object sender, ClickEventArgs e)
	{
		Tree.Tree.Show(Tree.Tree.MaximumLevel);
	}

	private void ValidateEditor_ValidateChanged(object sender, ValidateEnum e)
	{
		switch (e)
		{
		case ValidateEnum.Account:
			lblValidateTitle.Text = "科目设置检查结果";
			break;
		case ValidateEnum.Balance:
			lblValidateTitle.Text = "科目余额检查结果";
			break;
		case ValidateEnum.Voucher:
			lblValidateTitle.Text = "记账凭证检查结果";
			break;
		}
	}

	private void Initialize()
	{
		cmdExpandAll.Text = "全部展开";
		cmdExpandAll.Click += CmdExpandAll_Click;
		lnkExpandAll.Command = cmdExpandAll;
		ctxTreeCell.CommandLinks.Add(lnkExpandAll);
		cmdCollaspeAll.Text = "全部收缩";
		cmdCollaspeAll.Click += CmdCollaspeAll_Click;
		lnkCollaspeAll.Command = cmdCollaspeAll;
		ctxTreeCell.CommandLinks.Add(lnkCollaspeAll);
		Tree.MouseClick += Tree_MouseClick;
		cmdCopy.Text = "复制";
		cmdCopy.Image = ContextResources.ctxCopy;
		cmdCopy.Click += delegate
		{
			Common.SetSelectionToClipboard(grdValidate);
		};
		cmdCopy.CommandStateQuery += CmdCopy_CommandStateQuery;
		lnkCopy.Command = cmdCopy;
		ctxViewCell.CommandLinks.Add(lnkCopy);
		ctxViewCell.CommandLinks.Add(grdValidate.FilterManager.GenLnkFilter());
		ctxViewCell.CommandLinks.Add(grdValidate.FilterManager.GenLnkSample());
		ctxViewCell.CommandLinks.Add(grdValidate.FilterManager.GenLnkSelect());
		ctxViewCell.CommandLinks.Add(grdValidate.FilterManager.GenLnkCancelCurrentColumn());
		ctxViewEmpty.CommandLinks.Add(grdValidate.FilterManager.GenLnkCancelAll());
		ctxViewFixed.HideFirstDelimiter = true;
		C1Command c1Command = new C1Command();
		c1Command.Text = "隐藏本列";
		c1Command.UserData = grdValidate;
		c1Command.Click += _owner.ColHide_Click;
		C1CommandLink c1CommandLink = new C1CommandLink();
		c1CommandLink.Command = c1Command;
		ctxViewFixed.CommandLinks.Add(c1CommandLink);
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "取消隐藏";
		c1Command2.UserData = grdValidate;
		c1Command2.Click += _owner.CancelHide_Click;
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		c1CommandLink2.Command = c1Command2;
		ctxViewFixed.CommandLinks.Add(c1CommandLink2);
		grdValidate.MouseClick += GrdValidate_MouseClick;
	}

	private void Tree_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			HitTestTypeEnum type = Tree.HitTest(e.Location).Type;
			if (type == HitTestTypeEnum.Cell)
			{
				ctxTreeCell.ShowContextMenu(Tree, e.Location);
			}
		}
	}

	private void GrdValidate_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdValidate.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxViewFixed.ShowContextMenu(grdValidate, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxViewEmpty.ShowContextMenu(grdValidate, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxViewCell.ShowContextMenu(grdValidate, e.Location);
				break;
			}
		}
	}

	private void ValidateAccount(IEnumerable<ValidateResult> results)
	{
		try
		{
			grdValidate.BeginUpdate();
			grdValidate.Rows.Count = 0;
			grdValidate.Cols.Count = 0;
			grdValidate.Rows.DefaultSize = 30;
			C1.Win.C1FlexGrid.Column column = grdValidate.Cols.Add();
			column.Caption = "序号";
			column.Name = "index";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "科目代码";
			column.Name = "code";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "科目名称";
			column.Name = "name";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "风险提示";
			column.Name = "tip";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			C1.Win.C1FlexGrid.Row row = grdValidate.Rows.Add();
			C1.Win.C1FlexGrid.CellStyle newStyle = FixStyle();
			for (int i = 0; i < grdValidate.Cols.Count; i++)
			{
				grdValidate.SetCellStyle(0, i, newStyle);
			}
			row["index"] = "序号";
			row["code"] = "科目代码";
			row["name"] = "科目名称";
			row["tip"] = "风险提示";
			int num = 1;
			foreach (ValidateResult result in results)
			{
				Account account = result.Key as Account;
				row = grdValidate.Rows.Add();
				row.UserData = result;
				row["index"] = num++;
				row["code"] = account.Code;
				row["name"] = account.Name;
				row["tip"] = result.Tip;
			}
			for (int j = 0; j < grdValidate.Rows.Count; j++)
			{
				grdValidate.SetCellStyle(j, 0, newStyle);
			}
			grdValidate.Rows.Fixed = 1;
			grdValidate.Cols.Fixed = 1;
			grdValidate.AutoSizeCols();
			grdValidate.AllowEditing = false;
			grdValidate.ExtendLastCol = true;
			OnValidateChanged(ValidateEnum.Account);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
		finally
		{
			grdValidate.EndUpdate();
		}
	}

	private void ValidateBalance(IEnumerable<ValidateResult> results)
	{
		try
		{
			grdValidate.BeginUpdate();
			grdValidate.Rows.Count = 0;
			grdValidate.Cols.Count = 0;
			grdValidate.Rows.DefaultSize = 30;
			C1.Win.C1FlexGrid.Column column = grdValidate.Cols.Add();
			column.Caption = "序号";
			column.Name = "index";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "科目代码";
			column.Name = "code";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "科目名称";
			column.Name = "name";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "期末余额";
			column.Name = "balance";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdValidate.Cols.Add();
			column.Caption = "风险提示";
			column.Name = "tip";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			C1.Win.C1FlexGrid.Row row = grdValidate.Rows.Add();
			C1.Win.C1FlexGrid.CellStyle newStyle = FixStyle();
			for (int i = 0; i < grdValidate.Cols.Count; i++)
			{
				grdValidate.SetCellStyle(0, i, newStyle);
			}
			row["index"] = "序号";
			row["code"] = "科目代码";
			row["name"] = "科目名称";
			row["balance"] = "期末余额";
			row["tip"] = "风险提示";
			int num = 1;
			DateBalance end = Ledger.GetTrialBalanceSheet(StartDate, EndDate).End;
			foreach (ValidateResult result in results)
			{
				Account account = result.Key as Account;
				row = grdValidate.Rows.Add();
				row.UserData = result;
				row["index"] = num++;
				row["code"] = account.Code;
				row["name"] = account.Name;
				row["balance"] = end[account].Total;
				row["tip"] = result.Tip;
			}
			for (int j = 0; j < grdValidate.Rows.Count; j++)
			{
				grdValidate.SetCellStyle(j, 0, newStyle);
			}
			grdValidate.Rows.Fixed = 1;
			grdValidate.Cols.Fixed = 1;
			grdValidate.AutoSizeCols();
			grdValidate.AllowEditing = false;
			grdValidate.ExtendLastCol = true;
			OnValidateChanged(ValidateEnum.Balance);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
		finally
		{
			grdValidate.EndUpdate();
		}
	}

	private void ValidateVoucher(IEnumerable<ValidateResult> results)
	{
		try
		{
			grdValidate.BeginUpdate();
			grdValidate.Rows.Count = 0;
			grdValidate.Cols.Count = 0;
			grdValidate.Rows.DefaultSize = 30;
			C1.Win.C1FlexGrid.Column column = grdValidate.Cols.Add();
			column.Caption = "序号";
			column.Name = "index";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "日期";
			column.Name = "date";
			column.DataType = typeof(DateTime);
			column.Format = "yyyy-MM-dd";
			column = grdValidate.Cols.Add();
			column.Caption = "字";
			column.Name = "type";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grdValidate.Cols.Add();
			column.Caption = "号";
			column.Name = "number";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grdValidate.Cols.Add();
			column.Caption = "摘要";
			column.Name = "digest";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "科目代码";
			column.Name = "code";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "科目名称";
			column.Name = "name";
			column.DataType = typeof(string);
			column = grdValidate.Cols.Add();
			column.Caption = "借方金额";
			column.Name = "debit";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdValidate.Cols.Add();
			column.Caption = "贷方金额";
			column.Name = "credit";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdValidate.Cols.Add();
			column.Name = "tip";
			column.Caption = "风险提示";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			C1.Win.C1FlexGrid.Row row = grdValidate.Rows.Add();
			C1.Win.C1FlexGrid.CellStyle newStyle = FixStyle();
			for (int i = 0; i < grdValidate.Cols.Count; i++)
			{
				grdValidate.SetCellStyle(0, i, newStyle);
			}
			row["index"] = "序号";
			row["date"] = "日期";
			row["type"] = "字";
			row["number"] = "号";
			row["digest"] = "摘要";
			row["code"] = "科目代码";
			row["name"] = "科目名称";
			row["debit"] = "借方金额";
			row["credit"] = "贷方金额";
			row["tip"] = "风险提示";
			int num = 1;
			foreach (ValidateResult result in results)
			{
				Voucher voucher = result.Key as Voucher;
				row = grdValidate.Rows.Add();
				row.UserData = result;
				row["index"] = num++;
				row["date"] = voucher.Day;
				row["type"] = voucher.Type.Name;
				row["number"] = voucher.Number;
				row["digest"] = voucher.Digest;
				row["code"] = voucher.Account.Code;
				row["name"] = voucher.Account.Name;
				row["debit"] = (voucher.Account.IsDebit ? voucher.Amount : 0m);
				row["credit"] = (voucher.Account.IsDebit ? 0m : voucher.Amount);
				row["tip"] = result.Tip;
			}
			for (int j = 0; j < grdValidate.Rows.Count; j++)
			{
				grdValidate.SetCellStyle(j, 0, newStyle);
			}
			MergeTip(grdValidate, "tip");
			grdValidate.Rows.Fixed = 1;
			grdValidate.Cols.Fixed = 1;
			grdValidate.AutoSizeCols();
			grdValidate.AllowEditing = false;
			grdValidate.ExtendLastCol = true;
			OnValidateChanged(ValidateEnum.Voucher);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
		finally
		{
			grdValidate.EndUpdate();
		}
	}

	protected void OnValidateChanged(ValidateEnum validateEnum)
	{
		this.ValidateChanged?.Invoke(this, validateEnum);
	}

	private void MergeTip(C1FlexGrid grid, string colName)
	{
		grid.AllowMerging = AllowMergingEnum.Custom;
		int index = grid.Cols[colName].Index;
		for (int i = grid.Rows.Fixed; i < grid.Rows.Count; i++)
		{
			int num = i;
			for (; i + 1 < grid.Rows.Count && grid.Rows[i + 1][colName] == grid.Rows[i][colName]; i++)
			{
			}
			if (i > num)
			{
				grid.MergedRanges.Add(num, index, i, index);
			}
		}
	}

	private C1.Win.C1FlexGrid.CellStyle FixStyle()
	{
		C1.Win.C1FlexGrid.CellStyle cellStyle = grdValidate.Styles.Add("f");
		cellStyle.DataType = typeof(string);
		cellStyle.TextAlign = TextAlignEnum.CenterCenter;
		return cellStyle;
	}
}
