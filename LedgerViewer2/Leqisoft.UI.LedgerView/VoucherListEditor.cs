using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

public class VoucherListEditor : ISetTheme
{
	private const string DATETIMEFORMAT = "yyyy-MM-dd";

	private LedgerViewer _owner;

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkCopy = new C1CommandLink();

	private C1Command cmdDirectionChange = new C1Command();

	private C1CommandLink lnkDirectionChange = new C1CommandLink();

	private C1Command cmdDirectionReduce = new C1Command();

	private C1CommandLink lnkDirectionReduce = new C1CommandLink();

	private C1Command cmdMakeMark = new C1Command();

	private C1CommandLink lnkMakeMark = new C1CommandLink();

	private C1Command cmdCancelMark = new C1Command();

	private C1CommandLink lnkCancelMark = new C1CommandLink();

	private C1Command cmdColHide = new C1Command();

	private C1CommandLink lnkColHide = new C1CommandLink();

	private C1Command cmdCancelHide = new C1Command();

	private C1CommandLink lnkCancelHide = new C1CommandLink();

	private C1ContextMenu mnuTree = new C1ContextMenu();

	private readonly Tuple<Color, Color> _alternateColor = Tuple.Create(Color.AliceBlue, Color.LightYellow);

	private List<int> _sameNumberEnds = new List<int>();

	private int _mouseRow = -1;

	private readonly SolidBrush _brushHoverBackground = new SolidBrush(Color.Transparent);

	private C1SplitterPanel pnlVoucherListTitle;

	private C1SplitterPanel pnlVoucherListGrid;

	public C1Label lblVoucherListTitle;

	public C1FlexGridEx _grid;

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1ToolBar toolBar = new C1ToolBar();

	private C1SplitterPanel pnlSidebar;

	private C1CommandLink lnkSidebarDirectionChange = new C1CommandLink();

	private C1Command cmdSidebarDirectionChange = new C1Command();

	private C1CommandLink lnkSidebarMarkVoucher = new C1CommandLink();

	private C1Command cmdSidebarMarkVoucher = new C1Command();

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1ContextMenu ctxFixed = new C1ContextMenu();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	public C1FlexGridEx Tree { get; private set; }

	private Ledger Ledger => _owner.Ledger;

	public bool PendingAllEvent { get; set; }

	public List<Voucher> Vouchers { get; } = new List<Voucher>();


	public C1SplitContainer View { get; private set; }

	public VoucherListEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitComponent();
		BindContexMenu();
		Tree.Click += _tree_Click;
		Tree.MouseClick += _tree_MouseClick;
		Tree.CellChecked += Tree_CellChecked;
		Tree.OwnerDrawCell += Tree_OwnerDrawCell;
		Tree.MouseMove += Tree_MouseMove;
		Tree.MouseLeave += Tree_MouseLeave;
		InitializeCaption();
		_grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		_grid.DrawMode = DrawModeEnum.OwnerDraw;
		_grid.Click += _grid_Click;
		_grid.AfterResizeRow += _grid_AfterResizeRow;
		_grid.AfterResizeColumn += _grid_AfterResizeColumn;
		_grid.AfterDragColumn += _grid_AfterDragColumn;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.BodySelectionChanged += _grid_BodySelectionChanged;
		_grid.BodyAfterScroll += _grid_BodyAfterScroll;
		_grid.BeforeMouseDown += _grid_BeforeMouseDown;
		_grid.KeyDown += _grid_KeyDown;
		_grid.FilterManager.Context = new VoucherFilterContext(this);
	}

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
		if (hitTestInfo.Column == _grid.Cols["MyMark"].Index && hitTestInfo.Row >= _grid.Rows.Fixed)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyleDisplay = _grid.GetCellStyleDisplay(hitTestInfo.Row, hitTestInfo.Column);
			Rectangle cellRect = _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
			if (cellStyleDisplay.GetImageRectangle(cellRect, _grid.Glyphs[GlyphEnum.Checked]).Contains(hitTestInfo.Point))
			{
				e.Cancel = true;
				MakeSelectRangeMarkImpl(hitTestInfo.Row);
			}
		}
	}

	private void MakeSelectedRangeToMark()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		int num = -1;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			if (i >= _grid.Rows.Fixed)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			MakeSelectRangeMarkImpl(num);
		}
	}

	private void MakeSelectRangeMarkImpl(int clickRowIndex)
	{
		int num = clickRowIndex - _grid.Rows.Fixed;
		if (num < 0 || num >= Vouchers.Count)
		{
			return;
		}
		Voucher voucher = Vouchers[num];
		bool flag = !voucher.VoucherMark;
		_grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			if (selection.ContainsRow(clickRowIndex))
			{
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					if (i >= _grid.Rows.Fixed && _grid.Rows[i].Visible)
					{
						Voucher voucher2 = Vouchers[i - _grid.Rows.Fixed];
						if (voucher2.VoucherMark != flag)
						{
							voucher2.ToggleMark();
						}
					}
				}
			}
			else if (_grid.Rows[clickRowIndex].Visible && voucher.VoucherMark != flag)
			{
				voucher.ToggleMark();
			}
			Ledger.Save();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void Copy()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			if (!_grid.BodyGetRow(i).Visible)
			{
				continue;
			}
			num++;
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				if (_grid.BodyGetCol(j).Visible)
				{
					stringBuilder.Append(GetTextDisplay(i, j));
					if (j < bodySelection.RightCol)
					{
						stringBuilder.Append("\t");
					}
				}
			}
			stringBuilder.Append("\r\n");
		}
		if (LedgerViewer.LicenseCheckHandleOnCopyLedgerData != null && !LedgerViewer.LicenseCheckHandleOnCopyLedgerData(num))
		{
			return;
		}
		string text = stringBuilder.ToString();
		try
		{
			Clipboard.SetText(text);
		}
		catch (ExternalException)
		{
		}
	}

	private void _grid_BodyAfterScroll(object sender, RangeEventArgs e)
	{
		_grid.BeginUpdate();
		if (e.OldRange.r1 < e.NewRange.r1)
		{
			for (int k = e.OldRange.r1; k < e.NewRange.r1; k++)
			{
				for (int l = e.OldRange.c1; l <= e.OldRange.c2; l++)
				{
					ClearStyle(k, l);
				}
			}
		}
		else if (e.OldRange.r2 > e.NewRange.r2)
		{
			for (int num = e.OldRange.r2; num > e.NewRange.r2; num--)
			{
				for (int m = e.OldRange.c1; m <= e.OldRange.c2; m++)
				{
					ClearStyle(num, m);
				}
			}
		}
		else if (e.OldRange.c1 < e.NewRange.c1)
		{
			for (int n = e.OldRange.r1; n <= e.OldRange.r2; n++)
			{
				for (int num2 = e.OldRange.c1; num2 < e.NewRange.c1; num2++)
				{
					ClearStyle(n, num2);
				}
			}
		}
		else if (e.OldRange.c2 > e.NewRange.c2)
		{
			for (int num3 = e.OldRange.r1; num3 <= e.OldRange.r2; num3++)
			{
				for (int num4 = e.OldRange.c2; num4 > e.NewRange.c2; num4--)
				{
					ClearStyle(num3, num4);
				}
			}
		}
		_grid.EndUpdate();
		void ClearStyle(int i, int j)
		{
			C1.Win.C1FlexGrid.CellRange cellRange = _grid.BodyGetCell(i, j);
			try
			{
				cellRange.Style = null;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		C1.Win.C1FlexGrid.Column column = _grid.Cols[e.Col];
		if (column.Name == "Index" && e.Row >= _grid.Rows.Fixed)
		{
			e.Text = (e.Row - _grid.Rows.Fixed + 1).ToString();
		}
	}

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.C)
		{
			Copy();
		}
		else if (e.KeyData == Keys.Space)
		{
			MakeSelectedRangeToMark();
		}
	}

	private Color GetBackColorByIndex(int index)
	{
		int num = _sameNumberEnds.FindIndex((int n) => index < n);
		if (num % 2 == 0)
		{
			return _alternateColor.Item1;
		}
		return _alternateColor.Item2;
	}

	public object GetValue(int row, int col)
	{
		Voucher voucher = Vouchers[row];
		return _grid.BodyGetCol(col).Name switch
		{
			"MyMark" => voucher.VoucherMark, 
			"Date" => voucher.Day, 
			"Type" => voucher.Type.Name, 
			"Number" => voucher.Number, 
			"Digest" => voucher.Digest, 
			"Code" => voucher.GetDisplayAccountCodeWithDetail(), 
			"Name" => voucher.GetDisplayAccountNameWithDetail(), 
			"Debit" => voucher.IsDebit ? voucher.Amount : 0m, 
			"Credit" => voucher.IsDebit ? 0m : voucher.Amount, 
			"Maker" => voucher.Maker, 
			"Checker" => voucher.Checker, 
			"Booker" => voucher.Booker, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public string GetTextDisplay(int row, int col)
	{
		Voucher voucher = Vouchers[row];
		switch (_grid.BodyGetCol(col).Name)
		{
		case "MyMark":
			if (!voucher.VoucherMark)
			{
				return string.Empty;
			}
			return "√";
		case "Date":
			return voucher.Day.ToString("yyyy-MM-dd");
		case "Type":
			return voucher.Type.Name;
		case "Number":
			return voucher.Number;
		case "Digest":
			return voucher.Digest;
		case "Code":
			return voucher.GetDisplayAccountCodeWithDetail();
		case "Name":
			return voucher.GetDisplayAccountNameWithDetail();
		case "Debit":
			if (!voucher.IsDebit)
			{
				return string.Empty;
			}
			return voucher.Amount.ToString("#,0.00;-#,0.00;#");
		case "Credit":
			if (!voucher.IsDebit)
			{
				return voucher.Amount.ToString("#,0.00;-#,0.00;#");
			}
			return string.Empty;
		case "Maker":
			return voucher.Maker;
		case "Checker":
			return voucher.Checker;
		case "Booker":
			return voucher.Booker;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		Voucher voucher = Vouchers[e.Row];
		if (e.Col + _grid.Cols.Fixed == _grid.Cols["MyMark"].Index)
		{
			if (voucher.VoucherMark)
			{
				e.Image = _grid.Glyphs[GlyphEnum.Checked];
			}
			else
			{
				e.Image = _grid.Glyphs[GlyphEnum.Unchecked];
			}
			e.Text = string.Empty;
		}
		else
		{
			e.Text = GetTextDisplay(e.Row, e.Col);
		}
		C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
		if (voucher.VoucherMark)
		{
			styleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
			styleNew.ForeColor = Common.MarkForeColor;
		}
		else
		{
			styleNew.BackColor = GetBackColorByIndex(e.Row);
			styleNew.DefinedElements &= ~StyleElementFlags.ForeColor;
		}
	}

	private void Tree_CellChecked(object sender, RowColEventArgs e)
	{
		if (PendingAllEvent)
		{
			return;
		}
		try
		{
			Node node = Tree.Rows[e.Row].Node;
			CheckEnum cellCheck = Tree.GetCellCheck(e.Row, 0);
			Common.CheckChildren(Tree, node, cellCheck);
			_owner.UpdatePreview();
			FillVoucherList();
			PopulateVoucherList();
		}
		catch
		{
		}
	}

	public void FillVoucherList()
	{
		Vouchers.Clear();
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)Tree.Rows)
		{
			if (item.Node.Checked == CheckEnum.Checked && item.UserData is Tuple<NodeFlag, List<Voucher>> tuple)
			{
				Vouchers.AddRange(tuple.Item2);
			}
		}
	}

	public void ShowSideToolbar()
	{
		pnlSidebar?.Show();
	}

	public void HideSideToolbar()
	{
		pnlSidebar?.Hide();
	}

	public void InitComponent()
	{
		View = new C1SplitContainer();
		pnlVoucherListTitle = new C1SplitterPanel();
		pnlVoucherListGrid = new C1SplitterPanel();
		lblVoucherListTitle = new C1Label();
		_grid = new C1FlexGridEx();
		_grid.Name = "_grid";
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblVoucherListTitle.TextDetached = true;
		lblVoucherListTitle.BorderStyle = BorderStyle.None;
		lblVoucherListTitle.Dock = DockStyle.Fill;
		lblVoucherListTitle.Font = font;
		lblVoucherListTitle.Text = "记账凭证列表";
		lblVoucherListTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlVoucherListTitle.Height = 30;
		pnlVoucherListTitle.KeepRelativeSize = false;
		pnlVoucherListTitle.Location = new Point(0, 0);
		pnlVoucherListTitle.Resizable = false;
		pnlVoucherListTitle.Size = new Size(927, 30);
		pnlVoucherListTitle.SizeRatio = 3.0;
		pnlVoucherListTitle.Controls.Add(lblVoucherListTitle);
		pnlVoucherListTitle.Paint += PnlVoucherListTitle_Paint;
		_grid.AllowEditing = false;
		_grid.AllowResizing = AllowResizingEnum.Both;
		_grid.AllowSorting = AllowSortingEnum.None;
		_grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		_grid.Dock = DockStyle.Fill;
		_grid.Location = new Point(0, 0);
		_grid.Rows.DefaultSize = 20;
		_grid.Size = new Size(927, 599);
		_grid.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		cmdSidebarDirectionChange.Text = "方向调整";
		cmdSidebarDirectionChange.Image = Resources.sideDirectionChange;
		cmdSidebarDirectionChange.UserData = _grid;
		cmdSidebarDirectionChange.Click += CmdSidebarDirectionChange_Click;
		lnkSidebarDirectionChange.Command = cmdSidebarDirectionChange;
		toolBar.CommandLinks.Add(lnkSidebarDirectionChange);
		cmdSidebarMarkVoucher.Text = "标记关注";
		cmdSidebarMarkVoucher.Image = Resources.sideMarkVoucher;
		cmdSidebarMarkVoucher.UserData = _grid;
		cmdSidebarMarkVoucher.Click += CmdSidebarFlagMarkVoucher_Click;
		lnkSidebarMarkVoucher.Command = cmdSidebarMarkVoucher;
		toolBar.CommandLinks.Add(lnkSidebarMarkVoucher);
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "修改凭证";
		c1Command.Image = Resources.sideModifyVoucher;
		c1Command.Click += CmdModifyVoucher_Click;
		c1CommandLink.Command = c1Command;
		toolBar.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "填充至底稿";
		c1Command2.Image = Resources.sideFillToTable;
		c1Command2.Click += delegate
		{
			FillToTable();
		};
		c1Command2.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
		{
			e1.Visible = LedgerViewer.IsAuditPlatform;
		};
		c1CommandLink2.Command = c1Command2;
		toolBar.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		c1CommandLink3.Delimiter = true;
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "隐藏侧边栏";
		c1Command3.Image = Resources.sideHideSidebar;
		c1Command3.Click += CmdHideSidebar_Click;
		c1CommandLink3.Command = c1Command3;
		foreach (C1CommandLink commandLink in toolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		C1SplitContainer value = ComponentFactory.BuildSidebar(_grid, toolBar, out pnlSidebar);
		pnlVoucherListGrid.Height = 599;
		pnlVoucherListGrid.Location = new Point(0, 31);
		pnlVoucherListGrid.Size = new Size(927, 599);
		pnlVoucherListGrid.SizeRatio = 100.0;
		pnlVoucherListGrid.Controls.Add(value);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.BackColor = Color.FromArgb(240, 240, 240);
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.ForeColor = Color.FromArgb(0, 0, 0);
		View.Panels.Add(pnlVoucherListTitle);
		View.Panels.Add(pnlVoucherListGrid);
		Tree = GridFactory.Create("tree");
		Tree.Paint += delegate(object s1, PaintEventArgs e1)
		{
			Tree.DrawFormBorder(e1.Graphics);
		};
		BindTreeContexMenu();
	}

	private async void CmdModifyVoucher_Click(object sender, ClickEventArgs e)
	{
		if (_grid.Row >= _grid.Rows.Fixed)
		{
			Voucher voucher = Vouchers[_grid.BodyRow];
			await _owner.ModifyVoucher(voucher);
		}
		else
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择修改凭证的凭证行");
		}
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Copy();
	}

	private void RefreshCmdSidebarMarkVoucherText()
	{
		try
		{
			if (_grid.BodyRowSel <= 0)
			{
				cmdSidebarMarkVoucher.Text = "标记关注";
				return;
			}
			Voucher voucher = Vouchers[_grid.BodyRow];
			cmdSidebarMarkVoucher.Text = (voucher.VoucherMark ? "取消关注" : "标记关注");
		}
		catch (Exception)
		{
		}
	}

	private void CmdSidebarFlagMarkVoucher_Click(object sender, ClickEventArgs e)
	{
		if (_grid.BodyRow <= 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择标记关注的凭证行");
			return;
		}
		Voucher voucher = null;
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			if (_grid.BodyGetRow(i).Visible)
			{
				voucher = Vouchers[i];
				break;
			}
		}
		if (voucher != null)
		{
			if (voucher.VoucherMark)
			{
				CmdSidebarCancelMarkVoucher_Click(sender, e);
			}
			else
			{
				CmdSidebarMarkVoucher_Click(sender, e);
			}
		}
	}

	private void CmdSidebarMarkVoucher_Click(object sender, ClickEventArgs e)
	{
		if (_grid.BodyRow >= 0)
		{
			for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
			{
				if (_grid.BodyGetRow(i).Visible)
				{
					Voucher voucher = Vouchers[i];
					if (!voucher.VoucherMark)
					{
						voucher.ToggleMark();
					}
				}
			}
			RefreshCmdSidebarMarkVoucherText();
			_grid.Invalidate();
			Ledger.Save();
		}
		else
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择标记关注的凭证行");
		}
	}

	private void CmdSidebarCancelMarkVoucher_Click(object sender, ClickEventArgs e)
	{
		if (_grid.BodyRow >= 0)
		{
			for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
			{
				if (_grid.BodyGetRow(i).Visible)
				{
					Voucher voucher = Vouchers[i];
					if (voucher.VoucherMark)
					{
						voucher.ToggleMark();
					}
				}
			}
			RefreshCmdSidebarMarkVoucherText();
			_grid.Invalidate();
			Ledger.Save();
		}
		else
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择取消关注的凭证行");
		}
	}

	private void CmdSidebarDirectionChange_Click(object sender, ClickEventArgs e)
	{
		if (_grid.BodyRow >= 0)
		{
			for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
			{
				if (_grid.BodyGetRow(i).Visible)
				{
					Voucher voucher = Vouchers[i];
					voucher.ToggleDirection();
					_owner.CacheManager.CacheValid = false;
					_owner.BalanceEditor.PopulateBalanceSheet(Ledger, _owner.AccountTreeEditor.DisplayEmptyAccount);
				}
			}
			_owner.TriggerLedgerDataChangeEvent();
			_grid.Invalidate();
			Ledger.Save();
		}
		else
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择方向调整的凭证行");
		}
	}

	private void PnlVoucherListTitle_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.DrawLine(panelBorderPen, 0, pnlVoucherListTitle.Height - 1, pnlVoucherListTitle.Width, pnlVoucherListTitle.Height - 1);
	}

	private void CmdHideSidebar_Click(object sender, ClickEventArgs e)
	{
		_owner.OnHideSidebarClick();
	}

	public void PopulateVoucherList()
	{
		_grid.BeginUpdate();
		try
		{
			_grid.Rows.Count = Vouchers.Count + _grid.Rows.Fixed;
			SetSameNumberEnds();
			_owner.StyleRecord.ResumeStyle(_grid);
		}
		catch
		{
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void FillToTable()
	{
		List<object> list = new List<object>();
		int count = _grid.Rows.Count;
		for (int i = _grid.Rows.Fixed; i < count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
			if (!row.Visible)
			{
				continue;
			}
			int num = i - _grid.Rows.Fixed;
			if (num >= 0 && num < Vouchers.Count)
			{
				Voucher voucher = Vouchers[num];
				if (voucher.VoucherMark)
				{
					list.Add(voucher);
				}
			}
		}
		_owner._owner.OnAfterCollect(new LedgerCollectEventArgs
		{
			Viewer = _owner,
			Account = null,
			Auxiliary = null,
			CollectObject = CollectObjectEnum.Subsidiary,
			StartTime = _owner.StartDate,
			EndTime = _owner.EndDate,
			Source = list,
			IsShowSubsidiaryAllAccountTypeTable = true
		});
	}

	private void InitializeCaption()
	{
		_grid.Cols.Count = 0;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Name = "Index";
		column.Caption = "序号";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.AllowMerging = true;
		column.Width = 50;
		_grid.Cols.Fixed = 1;
		column = _grid.Cols.Add();
		column.Name = "MyMark";
		column.Caption = "标记关注";
		column.DataType = typeof(bool);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.Width = 65;
		column = _grid.Cols.Add();
		column.Name = "Date";
		column.Caption = "日期";
		column.DataType = typeof(DateTime);
		column.Format = "yyyy-MM-dd";
		column.AllowMerging = true;
		column.Width = 80;
		column = _grid.Cols.Add();
		column.Name = "Type";
		column.Caption = "字";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.Width = 30;
		column = _grid.Cols.Add();
		column.Name = "Number";
		column.Caption = "号";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.Width = 50;
		column = _grid.Cols.Add();
		column.Name = "Digest";
		column.Caption = "摘要";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.Width = 200;
		column = _grid.Cols.Add();
		column.Name = "Code";
		column.Caption = "科目代码";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.Width = 100;
		column = _grid.Cols.Add();
		column.Name = "Name";
		column.Caption = "科目名称";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.Width = 200;
		column = _grid.Cols.Add();
		column.Name = "Debit";
		column.Caption = "借方金额";
		column.DataType = typeof(decimal);
		column.Format = "#,0.00;-#,0.00;#";
		column.AllowMerging = true;
		column.Width = 80;
		column = _grid.Cols.Add();
		column.Name = "Credit";
		column.Caption = "贷方金额";
		column.DataType = typeof(decimal);
		column.Format = "#,0.00;-#,0.00;#";
		column.AllowMerging = true;
		column.Width = 80;
		column = _grid.Cols.Add();
		column.Name = "Maker";
		column.Caption = "制单人";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.Width = 60;
		column = _grid.Cols.Add();
		column.Name = "Checker";
		column.Caption = "审核人";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.Width = 60;
		column = _grid.Cols.Add();
		column.Name = "Booker";
		column.Caption = "记账人";
		column.DataType = typeof(string);
		column.AllowMerging = true;
		column.Width = 60;
	}

	public void PopulateTree()
	{
		Tree.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			Tree.Rows.Count = 0;
			Tree.Tree.Column = 0;
			Tree.Rows.DefaultSize = 30;
			Tree.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
			int num = 0;
			IOrderedEnumerable<IGrouping<int, Voucher>> orderedEnumerable = from v in Ledger.Vouchers
				group v by v.Day.Year into y
				orderby y.Key
				select y;
			foreach (IGrouping<int, Voucher> item in orderedEnumerable)
			{
				num++;
				IOrderedEnumerable<IGrouping<int, Voucher>> orderedEnumerable2 = from t in item
					group t by t.Day.Month into t
					orderby t.Key
					select t;
				foreach (IGrouping<int, Voucher> item2 in orderedEnumerable2)
				{
					num++;
					IOrderedEnumerable<IGrouping<VoucherType, Voucher>> orderedEnumerable3 = from t in item2
						group t by t.Type into t
						orderby t.Key.Name
						select t;
					foreach (IGrouping<VoucherType, Voucher> item3 in orderedEnumerable3)
					{
						num++;
						IOrderedEnumerable<IGrouping<string, Voucher>> orderedEnumerable4 = (from t in item3
							group t by t.Number).OrderBy((IGrouping<string, Voucher> t) => t.Key, StringNumberComparer.Instance);
						foreach (IGrouping<string, Voucher> item4 in orderedEnumerable4)
						{
							num++;
						}
					}
				}
			}
			Tree.Rows.Count = num;
			int num2 = 0;
			foreach (IGrouping<int, Voucher> item5 in orderedEnumerable)
			{
				int key = item5.Key;
				C1.Win.C1FlexGrid.Row row = Tree.Rows[num2];
				row.IsNode = true;
				row.Node.Level = 0;
				row[0] = $"{key}年";
				row.UserData = Tuple.Create(NodeFlag.Year, key);
				num2++;
				IOrderedEnumerable<IGrouping<int, Voucher>> orderedEnumerable5 = from t in item5
					group t by t.Day.Month into t
					orderby t.Key
					select t;
				foreach (IGrouping<int, Voucher> item6 in orderedEnumerable5)
				{
					int key2 = item6.Key;
					C1.Win.C1FlexGrid.Row row2 = Tree.Rows[num2];
					row2.IsNode = true;
					row2.Node.Level = 1;
					row2[0] = $"{key2}月";
					row2.UserData = Tuple.Create(NodeFlag.Month, key2);
					num2++;
					IOrderedEnumerable<IGrouping<VoucherType, Voucher>> orderedEnumerable6 = from t in item6
						group t by t.Type into t
						orderby t.Key.Name
						select t;
					foreach (IGrouping<VoucherType, Voucher> item7 in orderedEnumerable6)
					{
						string name = item7.Key.Name;
						C1.Win.C1FlexGrid.Row row3 = Tree.Rows[num2];
						row3.IsNode = true;
						row3.Node.Level = 2;
						row3[0] = name;
						row3.UserData = Tuple.Create(NodeFlag.Type, name);
						num2++;
						IOrderedEnumerable<IGrouping<string, Voucher>> orderedEnumerable7 = (from t in item7
							group t by t.Number).OrderBy((IGrouping<string, Voucher> t) => t.Key, StringNumberComparer.Instance);
						foreach (IGrouping<string, Voucher> item8 in orderedEnumerable7)
						{
							C1.Win.C1FlexGrid.Row row4 = Tree.Rows[num2];
							row4.IsNode = true;
							row4.Node.Level = 3;
							row4[0] = item8.Key;
							row4.UserData = Tuple.Create(NodeFlag.Vouchers, item8.ToList());
							num2++;
						}
					}
				}
			}
			_owner.StyleRecord.ResumeStyle(Tree);
			foreach (C1.Win.C1FlexGrid.Row item9 in (IEnumerable)Tree.Rows)
			{
				object userData = item9.UserData;
				if (!(userData is Tuple<NodeFlag, int> tuple))
				{
					if (userData is Tuple<NodeFlag, string> { Item1: NodeFlag.Type })
					{
						item9.Node.Checked = CheckEnum.Checked;
						item9.Node.Collapsed = true;
						continue;
					}
				}
				else
				{
					if (tuple.Item1 == NodeFlag.Year)
					{
						item9.Node.Checked = CheckEnum.Checked;
						item9.Node.Collapsed = false;
						continue;
					}
					Tuple<NodeFlag, int> tuple3 = tuple;
					if (tuple3.Item1 == NodeFlag.Month)
					{
						item9.Node.Checked = CheckEnum.Checked;
						item9.Node.Collapsed = true;
						continue;
					}
				}
				item9.Node.Checked = CheckEnum.Checked;
				item9.Node.Collapsed = true;
			}
			Tree.AllowEditing = true;
		}
		catch
		{
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			Tree.EndUpdate();
		}
		FillVoucherList();
		PopulateVoucherList();
	}

	public void SetTheme()
	{
		_grid.Styles.Fixed.Border.Color = Color.DarkGray;
		Tree.Styles.Alternate.BackColor = Color.Transparent;
		Tree.Styles.Fixed.Border.Width = 0;
		Tree.Styles.Normal.Border.Width = 0;
		Tree.Styles.EmptyArea.BackColor = Color.Transparent;
		Tree.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		if (Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
		_brushHoverBackground.Color = Color.FromArgb(100, Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background"));
	}

	private void _tree_Click(object sender, EventArgs e)
	{
		int mouseRow = Tree.MouseRow;
		if (mouseRow < Tree.Rows.Fixed || mouseRow >= Tree.Rows.Count)
		{
			return;
		}
		object userData = Tree.Rows[mouseRow].UserData;
		if (userData is Tuple<NodeFlag, List<Voucher>> tuple)
		{
			Voucher voucher = tuple.Item2.FirstOrDefault();
			if (voucher != null)
			{
				int num = Vouchers.IndexOf(voucher);
				_grid.Row = num + _grid.Rows.Fixed;
			}
		}
	}

	private void _tree_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Tree.HitTest(e.Location);
		if (e.Button == MouseButtons.Left)
		{
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				Node node = Tree.Rows[hitTestInfo.Row].Node;
				node.Collapsed = !node.Collapsed;
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			mnuTree.ShowContextMenu(Tree, e.Location);
		}
	}

	private void Tree_MouseLeave(object sender, EventArgs e)
	{
		_mouseRow = -1;
		Tree.Invalidate();
	}

	private void Tree_MouseMove(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Tree.HitTest();
		if (_mouseRow != hitTestInfo.Row)
		{
			_mouseRow = hitTestInfo.Row;
			Tree.Invalidate();
		}
	}

	private void Tree_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row == _mouseRow)
		{
			e.Graphics.FillRectangle(_brushHoverBackground, e.Bounds);
		}
	}

	private void _grid_Click(object sender, EventArgs e)
	{
		if (_grid.Row < 0)
		{
			return;
		}
		object userData = _grid.Rows[_grid.Row].UserData;
		if (userData == null)
		{
			return;
		}
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)Tree.Rows)
		{
			if (item.UserData is Tuple<NodeFlag, List<Voucher>> tuple && tuple.Item2.Contains(userData))
			{
				Tree.Row = item.Index;
				break;
			}
		}
	}

	private void _grid_AfterResizeRow(object sender, RowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			_owner.StyleRecord.RecordHeight(sender as C1FlexGridEx, e.Row);
		}
	}

	private void _grid_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordWidth(c1FlexGridEx.Name, c1FlexGridEx.Cols[e.Col].Name, c1FlexGridEx.Cols[e.Col].Width);
		}
	}

	private void _grid_AfterDragColumn(object sender, DragRowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordOrder(c1FlexGridEx.Name, from C1.Win.C1FlexGrid.Column t in c1FlexGridEx.Cols
				select t.Name);
		}
	}

	private void BindContexMenu()
	{
		try
		{
			cmdCopy.Text = "复制";
			cmdCopy.Image = ContextResources.ctxCopy;
			cmdCopy.Click += CmdCopy_Click;
			lnkCopy.Command = cmdCopy;
			ctxCell.CommandLinks.Add(lnkCopy);
			ctxCell.CommandLinks.Add(_grid.FilterManager.GenLnkFilter());
			ctxCell.CommandLinks.Add(_grid.FilterManager.GenLnkSample());
			ctxCell.CommandLinks.Add(_grid.FilterManager.GenLnkSelect());
			ctxCell.CommandLinks.Add(_grid.FilterManager.GenLnkCancelCurrentColumn());
			cmdDirectionChange.Text = "方向调整";
			cmdDirectionChange.UserData = _grid;
			cmdDirectionChange.Image = ContextResources.ctxDirectionChange;
			cmdDirectionChange.Click += CmdSidebarDirectionChange_Click;
			lnkDirectionChange.Command = cmdDirectionChange;
			lnkDirectionChange.Delimiter = true;
			ctxCell.CommandLinks.Add(lnkDirectionChange);
			cmdDirectionReduce.Text = "方向还原";
			cmdDirectionReduce.UserData = _grid;
			cmdDirectionReduce.Click += CmdSidebarDirectionChange_Click;
			lnkDirectionReduce.Command = cmdDirectionReduce;
			ctxCell.CommandLinks.Add(lnkDirectionReduce);
			cmdMakeMark.Text = "标记关注";
			cmdMakeMark.UserData = _grid;
			cmdMakeMark.Image = ContextResources.ctxMakeMark;
			cmdMakeMark.Click += CmdSidebarMarkVoucher_Click;
			lnkMakeMark.Command = cmdMakeMark;
			ctxCell.CommandLinks.Add(lnkMakeMark);
			cmdCancelMark.Text = "取消关注";
			cmdCancelMark.UserData = _grid;
			cmdCancelMark.Click += CmdSidebarCancelMarkVoucher_Click;
			lnkCancelMark.Command = cmdCancelMark;
			ctxCell.CommandLinks.Add(lnkCancelMark);
			C1CommandLink c1CommandLink = new C1CommandLink();
			C1Command c1Command = new C1Command();
			c1Command.Text = "修改凭证";
			c1Command.Image = ContextResources.modifyLedger;
			c1Command.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				int row = _grid.Row;
				e1.Visible = row >= _grid.Rows.Fixed && row < _grid.Rows.Count;
			};
			c1Command.Click += async delegate
			{
				if (_grid.Row >= _grid.Rows.Fixed)
				{
					Voucher voucher2 = Vouchers[_grid.BodyRow];
					await _owner.ModifyVoucher(voucher2);
				}
			};
			c1CommandLink.Command = c1Command;
			c1CommandLink.Delimiter = true;
			ctxCell.CommandLinks.Add(c1CommandLink);
			ctxCell.Popup += delegate
			{
				ctxCell.ShowAll();
				if (_grid.MouseRow >= 0 && _grid.MouseRow < _grid.Rows.Fixed)
				{
					ctxCell.OnlyShow(lnkColHide, lnkCancelHide);
				}
				else
				{
					ctxCell.HideLinks(lnkColHide, lnkCancelHide);
					bool flag = _grid.Selection.r2 - _grid.Selection.r1 == 0;
					if (_grid.Row >= _grid.Rows.Fixed && flag)
					{
						Voucher voucher = Vouchers[_grid.BodyRow];
						if (voucher.DirectionToggled)
						{
							ctxCell.HideLinks(lnkDirectionChange);
						}
						else
						{
							ctxCell.HideLinks(lnkDirectionReduce);
						}
						if (voucher.VoucherMark)
						{
							ctxCell.HideLinks(lnkMakeMark);
						}
						else
						{
							ctxCell.HideLinks(lnkCancelMark);
						}
					}
				}
			};
			ctxEmpty.CommandLinks.Add(_grid.FilterManager.GenLnkCancelAll());
			ctxFixed.HideFirstDelimiter = true;
			cmdColHide.Text = "隐藏本列";
			cmdColHide.UserData = _grid;
			cmdColHide.Click += _owner.ColHide_Click;
			lnkColHide.Command = cmdColHide;
			ctxFixed.CommandLinks.Add(lnkColHide);
			cmdCancelHide.Text = "取消隐藏";
			cmdCancelHide.UserData = _grid;
			cmdCancelHide.Click += _owner.CancelHide_Click;
			lnkCancelHide.Command = cmdCancelHide;
			ctxFixed.CommandLinks.Add(lnkCancelHide);
			_grid.MouseClick += GrdVoucherList_MouseClick;
		}
		catch
		{
		}
	}

	private void _grid_BodySelectionChanged(object sender, EventArgs e)
	{
		int row = _grid.Row;
		if (row < _grid.Rows.Fixed)
		{
			cmdSidebarDirectionChange.Visible = false;
			cmdSidebarMarkVoucher.Visible = false;
			return;
		}
		Voucher voucher = Vouchers[_grid.BodyRow];
		cmdSidebarDirectionChange.Visible = true;
		cmdSidebarMarkVoucher.Visible = true;
		cmdSidebarDirectionChange.Text = (voucher.DirectionToggled ? "方向还原" : "方向调整");
		cmdSidebarMarkVoucher.Text = (voucher.VoucherMark ? "取消关注" : "标记关注");
		List<double> list = new List<double>();
		int index = _grid.Cols["Credit"].Index - _grid.Cols.Fixed;
		if (_grid.BodySelection.ContainsCol(index))
		{
			list.AddRange(from i in (from i in Enumerable.Range(_grid.BodySelection.TopRow, _grid.BodySelection.BottomRow - _grid.BodySelection.TopRow + 1)
					where _grid.BodyGetRow(i).Visible
					select GetValue(i, index)).OfType<decimal>()
				select (double)i);
		}
		index = _grid.Cols["Debit"].Index - _grid.Cols.Fixed;
		if (_grid.BodySelection.ContainsCol(index))
		{
			list.AddRange(from i in (from i in Enumerable.Range(_grid.BodySelection.TopRow, _grid.BodySelection.BottomRow - _grid.BodySelection.TopRow + 1)
					where _grid.BodyGetRow(i).Visible
					select GetValue(i, index)).OfType<decimal>()
				select (double)i);
		}
		_owner._owner.OnLedgerSelectionChanged(new LedgerSelectionChangedEventArgs
		{
			Viewer = _owner,
			Numbers = list
		});
	}

	private void GrdVoucherList_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (_grid.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxFixed.ShowContextMenu(_grid, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(_grid, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxCell.ShowContextMenu(_grid, e.Location);
				break;
			}
		}
	}

	private void BindTreeContexMenu()
	{
		try
		{
			C1Command c1Command = new C1Command();
			C1CommandLink c1CommandLink = new C1CommandLink();
			C1Command c1Command2 = new C1Command();
			C1CommandLink c1CommandLink2 = new C1CommandLink();
			C1Command c1Command3 = new C1Command();
			C1CommandLink c1CommandLink3 = new C1CommandLink();
			C1Command c1Command4 = new C1Command();
			C1CommandLink c1CommandLink4 = new C1CommandLink();
			c1Command.Text = "全部展开";
			c1Command.Click += delegate
			{
				Tree.Tree.Show(Tree.Tree.MaximumLevel);
			};
			c1CommandLink.Command = c1Command;
			c1Command2.Text = "全部收缩";
			c1Command2.Click += delegate
			{
				Tree.BeginUpdate();
				for (int i = Tree.Rows.Fixed; i < Tree.Rows.Count; i++)
				{
					Tree.Rows[i].Node.Collapsed = true;
				}
				Tree.EndUpdate();
			};
			c1CommandLink2.Command = c1Command2;
			c1Command3.Text = "取消选择";
			c1Command3.Click += delegate
			{
				CancelAll();
			};
			c1CommandLink3.Command = c1Command3;
			c1Command4.Text = "全部选择";
			c1Command4.Click += delegate
			{
				SelectAll();
			};
			c1CommandLink4.Command = c1Command4;
			C1CommandLink c1CommandLink5 = new C1CommandLink();
			C1Command c1Command5 = new C1Command();
			c1Command5.Text = "新增凭证";
			c1Command5.Image = ContextResources.addLedger;
			c1Command5.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				int row4 = Tree.Row;
				e1.Visible = row4 >= Tree.Rows.Fixed && row4 < Tree.Rows.Count && (Tree.Rows[row4].UserData is Tuple<NodeFlag, string> || Tree.Rows[row4].UserData is Tuple<NodeFlag, List<Voucher>>);
			};
			c1Command5.Click += delegate
			{
				int row3 = Tree.Row;
				if (row3 >= Tree.Rows.Fixed && row3 < Tree.Rows.Count)
				{
					Node node2 = Tree.Rows[row3].Node;
					AddVoucher(node2);
				}
			};
			c1CommandLink5.Command = c1Command5;
			C1CommandLink c1CommandLink6 = new C1CommandLink();
			C1Command c1Command6 = new C1Command();
			c1Command6.Text = "修改凭证";
			c1Command6.Image = ContextResources.modifyLedger;
			c1Command6.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				int row2 = Tree.Row;
				e1.Visible = row2 >= Tree.Rows.Fixed && row2 < Tree.Rows.Count && Tree.Rows[row2].UserData is Tuple<NodeFlag, List<Voucher>> { Item1: NodeFlag.Vouchers } tuple3 && tuple3.Item2.Count > 0;
			};
			c1Command6.Click += async delegate
			{
				if (Tree.Row > 0 && Tree.Rows[Tree.Row].UserData is Tuple<NodeFlag, List<Voucher>> tuple2)
				{
					await _owner.ModifyVoucher(tuple2.Item2?.FirstOrDefault());
				}
			};
			c1CommandLink6.Command = c1Command6;
			C1CommandLink c1CommandLink7 = new C1CommandLink();
			C1Command c1Command7 = new C1Command();
			c1Command7.Text = "删除凭证";
			c1Command7.Image = ContextResources.deleteLedger;
			c1Command7.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				int row = Tree.Row;
				e1.Visible = row >= Tree.Rows.Fixed && row < Tree.Rows.Count && Tree.Rows[row].UserData is Tuple<NodeFlag, List<Voucher>>;
			};
			c1Command7.Click += delegate
			{
				if (Tree.Row < Tree.Rows.Fixed || Tree.Row >= Tree.Rows.Count || DialogResult.OK != Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "凭证删除后无法恢复，确定要删除该凭证吗？", MessageBoxButtons.OKCancel))
				{
					return;
				}
				try
				{
					Node node = Tree.Rows[Tree.Row].Node;
					if (node.Key is Tuple<NodeFlag, List<Voucher>> tuple)
					{
						foreach (Voucher item in tuple.Item2)
						{
							item.Dirty = -1;
						}
						Ledger.Save();
						foreach (Voucher item2 in tuple.Item2)
						{
							Ledger.Vouchers.Remove(item2);
						}
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "删除成功");
						_owner.OnAfterDeleteVoucher((from v in tuple.Item2
							group v by Common.GetVoucherKey(v)).FirstOrDefault());
					}
				}
				catch (Exception ex)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
				}
			};
			c1CommandLink7.Command = c1Command7;
			mnuTree.CommandLinks.Add(c1CommandLink);
			mnuTree.CommandLinks.Add(c1CommandLink2);
			mnuTree.CommandLinks.Add(c1CommandLink5);
			mnuTree.CommandLinks.Add(c1CommandLink6);
			mnuTree.CommandLinks.Add(c1CommandLink7);
			c1CommandLink4.Delimiter = true;
			c1CommandLink5.Delimiter = true;
		}
		catch
		{
		}
	}

	private void AddVoucher(Node node)
	{
		object key = node.Key;
		if (!(key is Tuple<NodeFlag, string>))
		{
			if (key is Tuple<NodeFlag, List<Voucher>>)
			{
				Show2(node?.Parent?.Parent?.Parent, node?.Parent?.Parent, node?.Parent, node);
			}
		}
		else
		{
			Show2(node?.Parent?.Parent, node?.Parent, node, null);
		}
		static int? GetMonth(Node n)
		{
			if (n?.Key is Tuple<NodeFlag, int> { Item1: NodeFlag.Month } tuple5)
			{
				return tuple5.Item2;
			}
			return null;
		}
		static string GetType(Node n)
		{
			if (n?.Key is Tuple<NodeFlag, string> { Item1: NodeFlag.Type } tuple4)
			{
				return tuple4.Item2;
			}
			return null;
		}
		static List<Voucher> GetVouchers(Node n)
		{
			if (n?.Key is Tuple<NodeFlag, List<Voucher>> { Item1: NodeFlag.Vouchers } tuple3)
			{
				return tuple3.Item2;
			}
			return null;
		}
		static int? GetYear(Node n)
		{
			if (n?.Key is Tuple<NodeFlag, int> { Item1: NodeFlag.Year } tuple6)
			{
				return tuple6.Item2;
			}
			return null;
		}
		void Show2(Node yNode, Node mNode, Node tNode, Node vNode)
		{
			int? year = GetYear(yNode);
			if (year.HasValue && yNode.Children != 0)
			{
				Node node2 = mNode ?? yNode.LastChild;
				int? month = GetMonth(node2);
				if (month.HasValue && node2.Children != 0)
				{
					Node node3 = tNode ?? node2.LastChild;
					string type = GetType(node3);
					if (type != null)
					{
						if (node3.Children == 0)
						{
							_owner.AddVoucher(type, new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value)));
						}
						else
						{
							Node n2 = vNode ?? node3.LastChild;
							List<Voucher> vouchers = GetVouchers(n2);
							if (vouchers == null || vouchers.Count == 0)
							{
								_owner.AddVoucher(type, new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value)));
							}
							else
							{
								string text = ((vNode == null) ? (vouchers.Max((Voucher v) => v.Number) + 1) : vouchers[0].Number);
								DateTime dateTime = ((vNode == null) ? new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value)) : vouchers[0].Day);
								_owner.AddVoucher(type, new DateTime(year.Value, month.Value, DateTime.DaysInMonth(year.Value, month.Value)));
							}
						}
					}
				}
			}
		}
	}

	private void SelectAll()
	{
		if (!PendingAllEvent)
		{
			Common.SetTreeCheck(Tree, CheckEnum.Checked);
			PopulateVoucherList();
		}
	}

	private void CancelAll()
	{
		if (!PendingAllEvent)
		{
			Common.SetTreeCheck(Tree, CheckEnum.Unchecked);
			PopulateVoucherList();
			_owner.UpdatePreview();
		}
	}

	private void SetSameNumberEnds()
	{
		_sameNumberEnds.Clear();
		if (Vouchers.Count <= 0)
		{
			return;
		}
		Voucher voucher = Vouchers[0];
		for (int i = 1; i < Vouchers.Count; i++)
		{
			Voucher voucher2 = Vouchers[i];
			if (voucher2.Type != voucher.Type || !(voucher2.Number == voucher.Number))
			{
				_sameNumberEnds.Add(i);
				voucher = voucher2;
			}
		}
	}
}
