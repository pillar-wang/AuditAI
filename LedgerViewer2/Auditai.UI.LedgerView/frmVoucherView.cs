using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using Auditai.UI.LedgerView.Properties;

namespace Auditai.UI.LedgerView;

/// <summary>
/// 只读凭证查看弹窗。以非模态方式展示一组凭证的明细（底部凭证区域同款布局），
/// 支持双击回调、关注标记、右键菜单与表格式样的持久化。
/// </summary>
public class frmVoucherView : Form
{
	private readonly LedgerViewer _owner;
	private readonly IEnumerable<Voucher> _vouchers;
	private readonly Action<Voucher> _onVoucherDoubleClick;
	private readonly Action _onMarkChanged;

	private Label lblVoucherType;
	private Label lblVoucherNumber;
	private Label lblVoucherDate;
	private Label lblNumAttachments;
	private Label lblCompanyName;

	private Label lblMaker;
	private Label lblBooker;
	private Label lblChecker;

	public C1FlexGridEx grdVoucher;

	private C1ContextMenu ctxVouCell = new C1ContextMenu();
	private C1ContextMenu ctxVouFixed = new C1ContextMenu();
	private C1ContextMenu ctxVouEmpty = new C1ContextMenu();

	private C1Command cmdCopy2 = new C1Command();
	private C1CommandLink lnkCopy2 = new C1CommandLink();
	private C1Command cmdDirectionChange2 = new C1Command();
	private C1CommandLink lnkDirectionChange2 = new C1CommandLink();
	private C1Command cmdDirectionReduce2 = new C1Command();
	private C1CommandLink lnkDirectionReduce2 = new C1CommandLink();
	private C1Command cmdMakeMark2 = new C1Command();
	private C1CommandLink lnkMakeMark2 = new C1CommandLink();
	private C1Command cmdCancelMark2 = new C1Command();
	private C1CommandLink lnkCancelMark2 = new C1CommandLink();
	private C1Command cmdColHide2 = new C1Command();
	private C1CommandLink lnkColHide2 = new C1CommandLink();
	private C1Command cmdCancelHide2 = new C1Command();
	private C1CommandLink lnkCancelHide2 = new C1CommandLink();

	private bool initializedVoucherCaption;

	private static int _instanceOffset;

	public frmVoucherView(LedgerViewer owner, IEnumerable<Voucher> vouchers, Action<Voucher> onVoucherDoubleClick = null, Action onMarkChanged = null)
	{
		_owner = owner;
		_vouchers = vouchers;
		_onVoucherDoubleClick = onVoucherDoubleClick;
		_onMarkChanged = onMarkChanged;
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
		base.Shown += FrmVoucherView_Shown;
		base.FormClosed += delegate
		{
			Dispose();
		};
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		PopulateVouchers(vouchers);
		BindVoucherContexMenu();
	}

	private void FrmVoucherView_Shown(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Auditai.UI.LedgerView.Properties.Resources.largeModifyVoucher);
	}

	/// <summary>非模态显示，父窗体支持下，多个实例做简单级联偏移避免完全重叠。</summary>
	public void ShowView()
	{
		Control parent = _owner.GetMainView()?.FindForm();
		if (parent != null)
		{
			Show(parent);
		}
		else
		{
			Show();
		}
		_instanceOffset++;
		int offset = (_instanceOffset % 10) * 28;
		Left += offset;
		Top += offset;
	}

	private void InitializeComponent()
	{
		base.ClientSize = new Size(1120, 360);
		base.MinimumSize = new Size(880, 300);
		base.Font = new Font("Microsoft YaHei", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 134);

		TableLayoutPanel table = new TableLayoutPanel();
		table.Dock = DockStyle.Fill;
		table.ColumnCount = 1;
		table.RowCount = 3;
		table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		table.Margin = new Padding(0);

		TableLayoutPanel headerTable = BuildHeader();
		grdVoucher = BuildGrid();
		TableLayoutPanel footerTable = BuildFooter();

		table.Controls.Add(headerTable, 0, 0);
		table.Controls.Add(grdVoucher, 0, 1);
		table.Controls.Add(footerTable, 0, 2);

		base.Controls.Add(table);
	}

	private TableLayoutPanel BuildHeader()
	{
		lblCompanyName = CreateHeadLabel("核算单位：" + (_owner.Ledger?.CompanyName ?? ""));
		lblVoucherType = CreateHeadLabel("字：", new Padding(16, 0, 0, 0));
		lblVoucherNumber = CreateHeadLabel("号： ", new Padding(16, 0, 0, 0));
		lblVoucherDate = CreateHeadLabel("制单日期：0000-00-00", new Padding(16, 0, 0, 0));
		lblNumAttachments = CreateHeadLabel("附件张数：", new Padding(16, 0, 0, 0));

		TableLayoutPanel header = new TableLayoutPanel();
		header.AutoSize = true;
		header.Dock = DockStyle.Fill;
		header.ColumnCount = 6;
		header.RowCount = 1;
		header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		header.Padding = new Padding(9, 2, 0, 2);

		header.Controls.Add(lblCompanyName, 0, 0);
		header.Controls.Add(lblVoucherType, 1, 0);
		header.Controls.Add(lblVoucherNumber, 2, 0);
		header.Controls.Add(lblVoucherDate, 4, 0);
		header.Controls.Add(lblNumAttachments, 5, 0);
		return header;
	}

	private TableLayoutPanel BuildFooter()
	{
		lblMaker = CreateFootLabel("制单人：", ContentAlignment.MiddleLeft);
		lblBooker = CreateFootLabel("记账人：", ContentAlignment.MiddleCenter);
		lblChecker = CreateFootLabel("审核人：", ContentAlignment.MiddleRight);

		TableLayoutPanel footer = new TableLayoutPanel();
		footer.AutoSize = true;
		footer.Dock = DockStyle.Fill;
		footer.ColumnCount = 4;
		footer.RowCount = 1;
		footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		footer.Padding = new Padding(9, 4, 0, 4);

		footer.Controls.Add(lblMaker, 0, 0);
		footer.Controls.Add(lblBooker, 1, 0);
		footer.Controls.Add(lblChecker, 3, 0);
		return footer;
	}

	private static Label CreateHeadLabel(string text, Padding? margin = null)
	{
		Label label = new Label();
		label.AutoSize = true;
		label.Text = text;
		label.Margin = margin ?? Padding.Empty;
		return label;
	}

	private static Label CreateFootLabel(string text, ContentAlignment align)
	{
		Label label = new Label();
		label.AutoSize = true;
		label.Text = text;
		label.TextAlign = align;
		return label;
	}

	private C1FlexGridEx BuildGrid()
	{
		C1FlexGridEx grid = new C1FlexGridEx();
		grid.Name = "grdVoucher";
		grid.AllowEditing = false;
		grid.AllowResizing = AllowResizingEnum.Both;
		grid.AllowSorting = AllowSortingEnum.None;
		grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grid.Dock = DockStyle.Fill;
		grid.DrawMode = DrawModeEnum.OwnerDraw;
		grid.ExtendLastCol = true;
		grid.Font = new Font("Microsoft YaHei", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 134);
		grid.Rows.DefaultSize = 20;
		grid.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		return grid;
	}

	private void PopulateVouchers(IEnumerable<Voucher> vouchers)
	{
		grdVoucher.BeginUpdate();
		try
		{
			InitializeVoucherCaption();
			grdVoucher.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grdVoucher.Rows.Fixed = 1;
			grdVoucher.Cols.Fixed = 1;
			int num = 1;
			decimal debitTotal = default(decimal);
			decimal creditTotal = default(decimal);
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdVoucher.Styles.Add("center");
			cellStyle.TextAlign = TextAlignEnum.CenterCenter;
			cellStyle.DataType = typeof(string);
			foreach (Voucher voucher in vouchers)
			{
				C1.Win.C1FlexGrid.Row row = grdVoucher.Rows.Add();
				row.UserData = voucher;
				row["Index"] = num++;
				row["Digest"] = voucher.Digest;
				row["Code"] = voucher.GetDisplayAccountCodeWithDetail();
				row["Name"] = voucher.GetDisplayAccountNameWithDetail();
				row["Debit"] = (voucher.IsDebit ? voucher.Amount : 0m);
				row["Credit"] = (voucher.IsDebit ? 0m : voucher.Amount);
				debitTotal += (voucher.IsDebit ? voucher.Amount : 0m);
				creditTotal += (voucher.IsDebit ? 0m : voucher.Amount);
				if (voucher.VoucherMark)
				{
					row.StyleNew.BackColor = Common.MarkBackColor;
					row.StyleNew.ForeColor = Common.MarkForeColor;
				}
			}
			C1.Win.C1FlexGrid.Row totalRow = grdVoucher.Rows.Add();
			totalRow["Digest"] = "合计";
			totalRow["Debit"] = debitTotal;
			totalRow["Credit"] = creditTotal;
			totalRow.StyleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
			grdVoucher.SetCellStyle(totalRow.Index, "Digest", cellStyle);
			SetVoucherHeader(vouchers.FirstOrDefault());
			_owner.StyleRecord.ResumeStyle(grdVoucher);
			AutoSizeVoucherColumns();
		}
		catch
		{
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void InitializeVoucherCaption()
	{
		if (!initializedVoucherCaption)
		{
			grdVoucher.Cols.Count = 0;
			grdVoucher.Rows.Count = 1;
			grdVoucher.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grdVoucher.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(int);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowMerging = true;
			column.Width = 50;
			column = grdVoucher.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 220;
			column = grdVoucher.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 100;
			column = grdVoucher.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 200;
			column = grdVoucher.Cols.Add();
			column.Name = "Unit";
			column.Caption = "单位名称";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 120;
			column = grdVoucher.Cols.Add();
			column.Name = "Opposite";
			column.Caption = "对方科目";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 120;
			column = grdVoucher.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 100;
			column = grdVoucher.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 100;
			initializedVoucherCaption = true;
		}
	}

	private void SetVoucherHeader(Voucher voucher)
	{
		if (voucher != null)
		{
			lblVoucherType.Text = $"字：{voucher.Type}";
			lblVoucherNumber.Text = "号： " + voucher.Number;
			lblVoucherDate.Text = "制单日期：" + voucher.Day.ToString("yyyy-MM-dd");
			lblNumAttachments.Text = $"附件张数：{voucher.NumAttachments} ";
			lblMaker.Text = "制单人：" + voucher.Maker;
			lblBooker.Text = "记账人：" + voucher.Booker;
			lblChecker.Text = "审核人：" + voucher.Checker;
		}
	}

	private void AutoSizeVoucherColumns()
	{
		if (grdVoucher == null || grdVoucher.Cols.Count <= grdVoucher.Cols.Fixed)
		{
			return;
		}
		int clientWidth = grdVoucher.ClientSize.Width;
		if (clientWidth <= 0) return;
		int fixedWidth = 0;
		for (int i = 0; i < grdVoucher.Cols.Fixed; i++)
			fixedWidth += grdVoucher.Cols[i].WidthDisplay;
		int availableWidth = clientWidth - fixedWidth;
		if (availableWidth <= 0) return;
		string[] colNames = { "Index", "Digest", "Code", "Name", "Unit", "Opposite", "Debit", "Credit" };
		double[] ratios = { 0.05, 0.22, 0.09, 0.13, 0.14, 0.14, 0.11, 0.12 };
		grdVoucher.BeginUpdate();
		try
		{
			for (int i = 0; i < colNames.Length && i < ratios.Length; i++)
			{
				if (grdVoucher.Cols.Contains(colNames[i]) && grdVoucher.Cols[colNames[i]].Visible)
				{
					int width = (int)(availableWidth * ratios[i]);
					if (width < 30) width = 30;
					grdVoucher.Cols[colNames[i]].Width = width;
				}
			}
		}
		finally { grdVoucher.EndUpdate(); }
	}

	private void GrdVoucher_DoubleClick(object sender, EventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[grdVoucher.Row];
		if (row.UserData is Voucher voucher)
		{
			_onVoucherDoubleClick?.Invoke(voucher);
		}
	}

	private void GrdVoucher_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyData = e.KeyData;
		if (keyData == Keys.Space)
		{
			CheckCellBox();
		}
	}

	private void CheckCellBox()
	{
		C1.Win.C1FlexGrid.CellRange selection = grdVoucher.Selection;
		int num = -1;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			if (i >= grdVoucher.Rows.Fixed && grdVoucher.Rows[i].UserData is Voucher)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			C1Command cmd = new C1Command();
			cmd.UserData = grdVoucher;
			Voucher voucher = grdVoucher.Rows[num].UserData as Voucher;
			if (voucher.VoucherMark)
			{
				CancelMarkImpl(cmd, ClickEventArgs.Empty);
			}
			else
			{
				MakeMarkImpl(cmd, ClickEventArgs.Empty);
			}
		}
	}

	private void MakeMarkImpl(object sender, ClickEventArgs e)
	{
		if (!(sender is C1Command { UserData: C1FlexGridEx userData }))
		{
			return;
		}
		userData.BeginUpdate();
		try
		{
			_owner.MakeMark_Click(sender, e);
			RefreshVouchersGridBackground();
			_onMarkChanged?.Invoke();
		}
		finally
		{
			userData.EndUpdate();
		}
	}

	private void CancelMarkImpl(object sender, ClickEventArgs e)
	{
		if (!(sender is C1Command { UserData: C1FlexGridEx userData }))
		{
			return;
		}
		userData.BeginUpdate();
		try
		{
			_owner.MarkCancel_Click(sender, e);
			RefreshVouchersGridBackground();
			_onMarkChanged?.Invoke();
		}
		finally
		{
			userData.EndUpdate();
		}
	}

	private void RefreshVouchersGridBackground()
	{
		grdVoucher.BeginUpdate();
		try
		{
			int count = grdVoucher.Rows.Count;
			for (int i = grdVoucher.Rows.Fixed; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[i];
				if (row.UserData is Voucher voucher)
				{
					if (voucher.VoucherMark)
					{
						row.StyleNew.BackColor = Common.MarkBackColor;
						row.StyleNew.ForeColor = Common.MarkForeColor;
					}
					else
					{
						row.StyleNew.BackColor = Color.White;
						row.StyleNew.ForeColor = grdVoucher.ForeColor;
					}
				}
			}
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void BindVoucherContexMenu()
	{
		try
		{
			cmdCopy2.Text = "复制";
			cmdCopy2.Image = ContextResources.ctxCopy;
			cmdCopy2.Click += delegate
			{
				Common.SetSelectionToClipboard(grdVoucher);
			};
			lnkCopy2.Command = cmdCopy2;
			ctxVouCell.CommandLinks.Add(lnkCopy2);
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkFilter());
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkSample());
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkSelect());
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkCancelCurrentColumn());
			cmdDirectionChange2.Text = "方向调整";
			cmdDirectionChange2.UserData = grdVoucher;
			cmdDirectionChange2.Image = ContextResources.ctxDirectionChange;
			cmdDirectionChange2.Click += _owner.DirectionChange_Click;
			lnkDirectionChange2.Command = cmdDirectionChange2;
			lnkDirectionChange2.Delimiter = true;
			ctxVouCell.CommandLinks.Add(lnkDirectionChange2);
			cmdDirectionReduce2.Text = "方向还原";
			cmdDirectionReduce2.UserData = grdVoucher;
			cmdDirectionReduce2.Click += _owner.DirectionReduce_Click;
			lnkDirectionReduce2.Command = cmdDirectionReduce2;
			ctxVouCell.CommandLinks.Add(lnkDirectionReduce2);
			cmdMakeMark2.Text = "标记关注";
			cmdMakeMark2.UserData = grdVoucher;
			cmdMakeMark2.Image = ContextResources.ctxMakeMark;
			cmdMakeMark2.Click += MakeMarkImpl;
			lnkMakeMark2.Command = cmdMakeMark2;
			ctxVouCell.CommandLinks.Add(lnkMakeMark2);
			cmdCancelMark2.Text = "取消关注";
			cmdCancelMark2.UserData = grdVoucher;
			cmdCancelMark2.Click += CancelMarkImpl;
			lnkCancelMark2.Command = cmdCancelMark2;
			ctxVouCell.CommandLinks.Add(lnkCancelMark2);
			C1CommandLink c1CommandLink = new C1CommandLink();
			C1Command c1Command = new C1Command();
			c1Command.Text = "修改凭证";
			c1Command.Image = ContextResources.modifyLedger;
			c1Command.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				int row = grdVoucher.Row;
				e1.Visible = row >= grdVoucher.Rows.Fixed && row < grdVoucher.Rows.Count && grdVoucher.Rows[row].UserData is Voucher;
			};
			c1Command.Click += async delegate
			{
				await _owner.ModifyVoucher(grdVoucher.Rows[grdVoucher.Row].UserData as Voucher);
			};
			c1CommandLink.Command = c1Command;
			c1CommandLink.Delimiter = true;
			ctxVouCell.CommandLinks.Add(c1CommandLink);
			ctxVouCell.Popup += delegate
			{
				ctxVouCell.ShowAll();
				if (grdVoucher.MouseRow >= 0 && grdVoucher.MouseRow < grdVoucher.Rows.Fixed)
				{
					ctxVouCell.OnlyShow(lnkColHide2, lnkCancelHide2);
				}
				else
				{
					ctxVouCell.HideLinks(lnkColHide2, lnkCancelHide2);
					bool flag = grdVoucher.Selection.r2 - grdVoucher.Selection.r1 == 0;
					if (grdVoucher.Row >= grdVoucher.Rows.Fixed && flag && grdVoucher.Rows[grdVoucher.Row].UserData is Voucher voucher)
					{
						if (voucher.DirectionToggled)
						{
							ctxVouCell.HideLinks(lnkDirectionChange2);
						}
						else
						{
							ctxVouCell.HideLinks(lnkDirectionReduce2);
						}
						if (voucher.VoucherMark)
						{
							ctxVouCell.HideLinks(lnkMakeMark2);
						}
						else
						{
							ctxVouCell.HideLinks(lnkCancelMark2);
						}
					}
				}
			};
			ctxVouEmpty.CommandLinks.Add(grdVoucher.FilterManager.GenLnkCancelAll());
			ctxVouFixed.HideFirstDelimiter = true;
			cmdColHide2.Text = "隐藏本列";
			cmdColHide2.UserData = grdVoucher;
			cmdColHide2.Click += _owner.ColHide_Click;
			lnkColHide2.Command = cmdColHide2;
			ctxVouFixed.CommandLinks.Add(lnkColHide2);
			cmdCancelHide2.Text = "取消隐藏";
			cmdCancelHide2.UserData = grdVoucher;
			cmdCancelHide2.Click += _owner.CancelHide_Click;
			lnkCancelHide2.Command = cmdCancelHide2;
			ctxVouFixed.CommandLinks.Add(lnkCancelHide2);
			grdVoucher.MouseClick += GrdVoucher_MouseClick;
			grdVoucher.DoubleClick += GrdVoucher_DoubleClick;
			grdVoucher.KeyDown += GrdVoucher_KeyDown;
			grdVoucher.AfterResizeRow += GrdVoucher_AfterResizeRow;
			grdVoucher.AfterResizeColumn += GrdVoucher_AfterResizeColumn;
			grdVoucher.AfterDragColumn += GrdVoucher_AfterDragColumn;
		}
		catch
		{
		}
	}

	private void GrdVoucher_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdVoucher.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxVouFixed.ShowContextMenu(grdVoucher, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxVouEmpty.ShowContextMenu(grdVoucher, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxVouCell.ShowContextMenu(grdVoucher, e.Location);
				break;
			}
		}
	}

	private void GrdVoucher_AfterResizeRow(object sender, RowColEventArgs e)
	{
		_owner.StyleRecord.RecordHeight(grdVoucher, e.Row);
	}

	private void GrdVoucher_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		C1FlexGridEx grid = sender as C1FlexGridEx;
		_owner.StyleRecord.RecordWidth(grid.Name, grid.Cols[e.Col].Name, grid.Cols[e.Col].Width);
	}

	private void GrdVoucher_AfterDragColumn(object sender, DragRowColEventArgs e)
	{
		C1FlexGridEx grid = sender as C1FlexGridEx;
		_owner.StyleRecord.RecordOrder(grid.Name, from C1.Win.C1FlexGrid.Column t in grid.Cols
			select t.Name);
	}
}