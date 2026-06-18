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

internal class SummaryEditor : ISetTheme
{
	private const string CN_INDEX = "index";

	private const string CN_PROJECT = "ProjectName";

	private const string CN_MONTHSUM = "MonthSum";

	private const string CN_MONTHLIST = "MonthList";

	internal const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	public LedgerViewer _owner;

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1ContextMenu ctxFixed = new C1ContextMenu();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkCopy = new C1CommandLink();

	private readonly C1CommandLink _lnkFilter;

	private readonly C1CommandLink _lnkSample;

	private readonly C1CommandLink _lnkSelect;

	private readonly C1CommandLink _lnkCancel;

	private C1SplitterPanel pnlMonthTitle;

	private C1SplitterPanel pnlMonthHead;

	private C1SplitterPanel pnlMonthGrid;

	private C1Label lblSummaryTitle;

	private C1Label lblMonthAccount;

	internal C1FlexGridEx grdMonthSummary;

	private C1SplitterPanel pnlSidebar;

	private C1ContextMenu ctxSidebarAnalyzyProject = new C1ContextMenu();

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	private Ledger Ledger => _owner.Ledger;

	private DateTime StartDate => _owner.StartDate;

	private DateTime EndDate => _owner.EndDate;

	private Account Account => _owner.CurrentAccount;

	public bool PendingAllEvent { get; set; }

	public bool Direction { get; set; } = true;


	public AnalysisProject AnalysisProject { get; set; }

	public bool HasShow { get; private set; }

	public C1SplitContainer View { get; private set; }

	public SummaryEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitializeComponent();
		Initialize();
		_lnkFilter = grdMonthSummary.FilterManager.GenLnkFilter();
		_lnkSample = grdMonthSummary.FilterManager.GenLnkSample();
		_lnkSelect = grdMonthSummary.FilterManager.GenLnkSelect();
		_lnkCancel = grdMonthSummary.FilterManager.GenLnkCancelCurrentColumn();
	}

	public void ShowSideToolbar()
	{
		pnlSidebar?.Show();
	}

	public void HideSideToolbar()
	{
		pnlSidebar?.Hide();
	}

	private void InitializeComponent()
	{
		View = new C1SplitContainer();
		pnlMonthTitle = new C1SplitterPanel();
		pnlMonthHead = new C1SplitterPanel();
		pnlMonthGrid = new C1SplitterPanel();
		lblSummaryTitle = new C1Label();
		lblMonthAccount = new C1Label();
		grdMonthSummary = new C1FlexGridEx();
		grdMonthSummary.Name = "grdMonthSummary";
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblSummaryTitle.TextDetached = true;
		lblSummaryTitle.BorderStyle = BorderStyle.None;
		lblSummaryTitle.Dock = DockStyle.Fill;
		lblSummaryTitle.Font = font;
		lblSummaryTitle.Text = "月度汇总表";
		lblSummaryTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlMonthTitle.Height = 30;
		pnlMonthTitle.KeepRelativeSize = false;
		pnlMonthTitle.Location = new Point(0, 0);
		pnlMonthTitle.MinHeight = 30;
		pnlMonthTitle.Resizable = false;
		pnlMonthTitle.Size = new Size(927, 30);
		pnlMonthTitle.SizeRatio = 4.769;
		pnlMonthTitle.Controls.Add(lblSummaryTitle);
		Font font2 = new Font("Microsoft YaHei", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblMonthAccount.TextDetached = true;
		lblMonthAccount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		lblMonthAccount.BorderStyle = BorderStyle.None;
		lblMonthAccount.Font = font2;
		lblMonthAccount.Location = new Point(3, 4);
		lblMonthAccount.Size = new Size(390, 17);
		lblMonthAccount.Text = "科目名称：";
		lblMonthAccount.TextAlign = ContentAlignment.MiddleLeft;
		pnlMonthHead.Height = 25;
		pnlMonthHead.KeepRelativeSize = false;
		pnlMonthHead.Location = new Point(0, 31);
		pnlMonthHead.MinHeight = 25;
		pnlMonthHead.Resizable = false;
		pnlMonthHead.Size = new Size(927, 25);
		pnlMonthHead.SizeRatio = 4.181;
		pnlMonthHead.Controls.Add(lblMonthAccount);
		pnlMonthHead.Paint += delegate(object s1, PaintEventArgs e1)
		{
			e1.Graphics.DrawLine(panelBorderPen, 0, pnlMonthHead.Height - 1, pnlMonthHead.Width, pnlMonthHead.Height - 1);
		};
		grdMonthSummary.AllowEditing = false;
		grdMonthSummary.AllowResizing = AllowResizingEnum.Both;
		grdMonthSummary.AllowSorting = AllowSortingEnum.None;
		grdMonthSummary.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdMonthSummary.Dock = DockStyle.Fill;
		grdMonthSummary.DrawMode = DrawModeEnum.OwnerDraw;
		grdMonthSummary.Font = font2;
		grdMonthSummary.Rows.DefaultSize = 20;
		grdMonthSummary.Tree.LineColor = Color.DimGray;
		grdMonthSummary.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		C1ToolBar c1ToolBar = new C1ToolBar();
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "借方发生额";
		c1Command.Image = null;
		c1Command.UserData = AnalysisProject.Debits;
		c1Command.Click += CmdSidebarAnalyzyProject_Click;
		c1CommandLink.Command = c1Command;
		ctxSidebarAnalyzyProject.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "贷方发生额";
		c1Command2.Image = null;
		c1Command2.UserData = AnalysisProject.Credits;
		c1Command2.Click += CmdSidebarAnalyzyProject_Click;
		c1CommandLink2.Command = c1Command2;
		ctxSidebarAnalyzyProject.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "分析选项";
		c1Command3.Image = Resources.sidebarAnalyzyProject;
		c1Command3.Click += CmdSidebarAnalyzyProject_Click1;
		c1CommandLink3.Command = c1Command3;
		c1ToolBar.CommandLinks.Add(c1CommandLink3);
		C1CommandLink c1CommandLink4 = new C1CommandLink();
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "切换样式";
		c1Command4.Image = Resources.sideSwitchStyle;
		c1Command4.Click += CmdDirection_Click;
		c1CommandLink4.Command = c1Command4;
		c1ToolBar.CommandLinks.Add(c1CommandLink4);
		C1CommandLink c1CommandLink5 = new C1CommandLink();
		c1CommandLink5.Delimiter = true;
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "隐藏侧边栏";
		c1Command5.Image = Resources.sideHideSidebar;
		c1Command5.Click += delegate
		{
			_owner.OnHideSidebarClick();
		};
		c1CommandLink5.Command = c1Command5;
		foreach (C1CommandLink commandLink in c1ToolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		C1SplitContainer value = ComponentFactory.BuildSidebar(grdMonthSummary, c1ToolBar, out pnlSidebar);
		pnlMonthGrid.Height = 573;
		pnlMonthGrid.Location = new Point(0, 57);
		pnlMonthGrid.Size = new Size(927, 573);
		pnlMonthGrid.SizeRatio = 100.0;
		pnlMonthGrid.Controls.Add(value);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.BackColor = Color.FromArgb(240, 240, 240);
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.ForeColor = Color.FromArgb(0, 0, 0);
		View.Panels.Add(pnlMonthTitle);
		View.Panels.Add(pnlMonthHead);
		View.Panels.Add(pnlMonthGrid);
	}

	private void CmdDirection_Click(object sender, ClickEventArgs e)
	{
		Direction = !Direction;
		Create(GetSelectNodeChildren());
	}

	private void CmdSidebarAnalyzyProject_Click1(object sender, ClickEventArgs e)
	{
		ctxSidebarAnalyzyProject.ShowContextMenu(e.CallerLink.Owner as C1ToolBar, new Point(e.CallerLink.Bounds.Left, e.CallerLink.Bounds.Bottom));
	}

	private void CmdSidebarAnalyzyProject_Click(object sender, ClickEventArgs e)
	{
		C1Command command = e.CallerLink.Command;
		if (command.UserData is AnalysisProject analysisProject)
		{
			AnalysisProject = analysisProject;
			Create(GetSelectNodeChildren());
		}
	}

	private void CtxCell_Popup(object sender, EventArgs e)
	{
		ctxCell.CommandLinks.Clear();
		ctxCell.CommandLinks.Add(lnkCopy);
		ctxCell.CommandLinks.Add(_lnkFilter);
		ctxCell.CommandLinks.Add(_lnkSample);
		ctxCell.CommandLinks.Add(_lnkSelect);
		ctxCell.CommandLinks.Add(_lnkCancel);
	}

	private void CmdLayer_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
	}

	private void CmdLayer_Click(object sender, ClickEventArgs e)
	{
	}

	public void SetAnalysisStatus()
	{
		Create(GetSelectNodeChildren());
	}

	public void Create(List<object> accountOrAuxItems)
	{
		if (Direction)
		{
			Horz(accountOrAuxItems);
		}
		else
		{
			Vert(accountOrAuxItems);
		}
		HasShow = true;
	}

	public void UpdateTitle(Account account)
	{
		lblMonthAccount.Text = "科目名称：" + Common.GetFullNameWithCode(account);
	}

	public void UpdateTitle(Account account, AuxiliaryClass auxiliaryClass)
	{
		lblMonthAccount.Text = "科目名称：" + Common.GetFullNameWithCode(account) + "    辅助核算类别：（" + auxiliaryClass.Code + "）" + auxiliaryClass.Name;
	}

	public void UpdateTitle(Account account, AuxiliaryItem auxiliaryItem)
	{
		lblMonthAccount.Text = "科目名称：" + Common.GetFullNameWithCode(account, auxiliaryItem);
	}

	public void SetTheme()
	{
		grdMonthSummary.Styles.Fixed.Border.Color = Color.DarkGray;
		if (Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
	}

	private void CmdCopy_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCopy.Visible = grdMonthSummary.Selection.RightCol - grdMonthSummary.Selection.LeftCol >= 0 && grdMonthSummary.BodySelection.BottomRow - grdMonthSummary.BodySelection.TopRow >= 0;
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Common.SetSelectionToClipboard(grdMonthSummary);
	}

	public List<object> GetSelectNodeChildren()
	{
		List<object> ret = new List<object>();
		C1.Win.C1FlexGrid.Row currentOpendedRow = _owner.AccountTreeEditor.CurrentOpendedRow;
		if (currentOpendedRow == null)
		{
			return ret;
		}
		addChildren(currentOpendedRow.Node);
		return ret;
		void addChildren(Node node)
		{
			if (node.Children == 0)
			{
				ret.Add(node.Key);
			}
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				addChildren(node2);
			}
		}
	}

	private void Initialize()
	{
		grdMonthSummary.AllowEditing = false;
		grdMonthSummary.AllowSorting = AllowSortingEnum.None;
		grdMonthSummary.AllowResizing = AllowResizingEnum.Both;
		cmdCopy.Text = "复制";
		cmdCopy.Image = ContextResources.ctxCopy;
		cmdCopy.Click += CmdCopy_Click;
		cmdCopy.CommandStateQuery += CmdCopy_CommandStateQuery;
		lnkCopy.Command = cmdCopy;
		ctxCell.Popup += CtxCell_Popup;
		ctxEmpty.CommandLinks.Add(grdMonthSummary.FilterManager.GenLnkCancelAll());
		ctxFixed.HideFirstDelimiter = true;
		C1Command c1Command = new C1Command();
		c1Command.Text = "隐藏本列";
		c1Command.UserData = grdMonthSummary;
		c1Command.Click += _owner.ColHide_Click;
		C1CommandLink c1CommandLink = new C1CommandLink();
		c1CommandLink.Command = c1Command;
		ctxFixed.CommandLinks.Add(c1CommandLink);
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "取消隐藏";
		c1Command2.UserData = grdMonthSummary;
		c1Command2.Click += _owner.CancelHide_Click;
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		c1CommandLink2.Command = c1Command2;
		ctxFixed.CommandLinks.Add(c1CommandLink2);
		grdMonthSummary.MouseClick += GrdMonthSummary_MouseClick;
	}

	private void GrdMonthSummary_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdMonthSummary.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxFixed.ShowContextMenu(grdMonthSummary, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(grdMonthSummary, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxCell.ShowContextMenu(grdMonthSummary, e.Location);
				break;
			}
		}
	}

	private void Horz(List<object> accountOrAuxItems)
	{
		if (AnalysisProject == AnalysisProject.Balance)
		{
			return;
		}
		Direction = true;
		grdMonthSummary.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			grdMonthSummary.Cols.Count = 0;
			grdMonthSummary.Rows.Count = 1;
			grdMonthSummary.Rows.Fixed = 1;
			grdMonthSummary.Rows.DefaultSize = 30;
			C1.Win.C1FlexGrid.Column column = grdMonthSummary.Cols.Add();
			column.Name = "index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grdMonthSummary.Cols.Add();
			column.Name = "ProjectName";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			for (int i = StartDate.Year; i <= EndDate.Year; i++)
			{
				for (int j = 1; j <= 12; j++)
				{
					column = grdMonthSummary.Cols.Add();
					column.Name = $"y{i}m{j}";
					column.Caption = $"{i}年{j}月";
					column.DataType = typeof(decimal);
					column.Format = "#,0.00;-#,0.00;#";
				}
			}
			column = grdMonthSummary.Cols.Add();
			column.Name = "MonthSum";
			column.Caption = ((AnalysisProject == AnalysisProject.Debits) ? "借方发生额合计" : ((AnalysisProject == AnalysisProject.Credits) ? "贷方发生额合计" : "合计"));
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			(column.Style ?? column.StyleNew).BackColor = Color.LightYellow;
			Dictionary<string, decimal> dictionary = new Dictionary<string, decimal>();
			for (int k = StartDate.Year; k <= EndDate.Year; k++)
			{
				for (int l = 1; l <= 12; l++)
				{
					dictionary.Add($"y{k}m{l}", 0m);
				}
			}
			int num = 1;
			foreach (object accountOrAuxItem in accountOrAuxItems)
			{
				SubsidiaryLedger subsidiaryLedger = null;
				string empty = string.Empty;
				if (!(accountOrAuxItem is Account account))
				{
					if (!(accountOrAuxItem is Tuple<Account, AuxiliaryItem> tuple))
					{
						continue;
					}
					empty = tuple.Item2.Name;
					subsidiaryLedger = Ledger.GetSubsidiaryLedger(tuple.Item1, StartDate, EndDate, tuple.Item2);
				}
				else
				{
					empty = account.Name;
					subsidiaryLedger = Ledger.GetSubsidiaryLedger(account, StartDate, EndDate);
				}
				C1.Win.C1FlexGrid.Row row = grdMonthSummary.Rows.Add();
				row.UserData = accountOrAuxItem;
				row["index"] = num++;
				row["ProjectName"] = empty;
				decimal num2 = default(decimal);
				for (int y = StartDate.Year; y <= EndDate.Year; y++)
				{
					int month;
					for (month = 1; month <= 12; month++)
					{
						MonthSubsidiaryLedger monthSubsidiaryLedger = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Year == y && m.Month == month);
						if (monthSubsidiaryLedger != null)
						{
							decimal totalValue = GetTotalValue(monthSubsidiaryLedger.Total, AnalysisProject);
							string text = $"y{y}m{month}";
							row[text] = totalValue;
							num2 += totalValue;
							dictionary[text] += totalValue;
						}
					}
				}
				row["MonthSum"] = num2;
			}
			C1.Win.C1FlexGrid.Row row2 = grdMonthSummary.Rows.Add();
			row2["index"] = num++;
			row2["ProjectName"] = ((AnalysisProject == AnalysisProject.Debits) ? "借方发生额合计" : ((AnalysisProject == AnalysisProject.Credits) ? "贷方发生额合计" : "合计"));
			C1.Win.C1FlexGrid.CellStyle cellStyle = row2.Style ?? row2.StyleNew;
			cellStyle.BackColor = Color.LightYellow;
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = grdMonthSummary.Styles.Add("textCenterStyle");
			cellStyle2.TextAlign = TextAlignEnum.CenterCenter;
			grdMonthSummary.SetCellStyle(row2.Index, grdMonthSummary.Cols["ProjectName"].Index, cellStyle2);
			for (int n = StartDate.Year; n <= EndDate.Year; n++)
			{
				for (int num3 = 1; num3 <= 12; num3++)
				{
					string text2 = $"y{n}m{num3}";
					row2[text2] = dictionary[text2];
				}
			}
			row2["MonthSum"] = dictionary.Sum((KeyValuePair<string, decimal> m) => m.Value);
			grdMonthSummary.Rows.Fixed = 1;
			grdMonthSummary.Cols.Fixed = 1;
			grdMonthSummary.AutoSizeCols(0, grdMonthSummary.Cols.Count, 5);
			grdMonthSummary.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		}
		finally
		{
			PendingAllEvent = false;
			grdMonthSummary.EndUpdate();
		}
	}

	private void Vert(List<object> accountOrAuxItems)
	{
		if (AnalysisProject == AnalysisProject.Balance)
		{
			return;
		}
		Direction = false;
		grdMonthSummary.BeginUpdate();
		try
		{
			grdMonthSummary.Cols.Count = 0;
			grdMonthSummary.Rows.Count = 1;
			grdMonthSummary.Rows.Fixed = 1;
			grdMonthSummary.Rows.DefaultSize = 30;
			C1.Win.C1FlexGrid.Column column = grdMonthSummary.Cols.Add();
			column.Name = "MonthList";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			Dictionary<string, C1.Win.C1FlexGrid.Row> dictionary = new Dictionary<string, C1.Win.C1FlexGrid.Row>();
			for (int i = StartDate.Year; i <= EndDate.Year; i++)
			{
				for (int j = 1; j <= 12; j++)
				{
					C1.Win.C1FlexGrid.Row row = grdMonthSummary.Rows.Add();
					row.UserData = Tuple.Create(i, j);
					row["MonthList"] = $"{i}年{j}月";
					dictionary.Add($"y{i}m{j}", row);
				}
			}
			C1.Win.C1FlexGrid.Row row2 = grdMonthSummary.Rows.Add();
			row2["MonthList"] = ((AnalysisProject == AnalysisProject.Debits) ? "借方发生额合计" : ((AnalysisProject == AnalysisProject.Credits) ? "贷方发生额合计" : "合计"));
			C1.Win.C1FlexGrid.CellStyle cellStyle = row2.Style ?? row2.StyleNew;
			cellStyle.BackColor = Color.LightYellow;
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = grdMonthSummary.Styles.Add("textCenterStyle");
			cellStyle2.TextAlign = TextAlignEnum.CenterCenter;
			grdMonthSummary.SetCellStyle(row2.Index, grdMonthSummary.Cols["MonthList"].Index, cellStyle2);
			Dictionary<string, decimal> dictionary2 = new Dictionary<string, decimal>();
			for (int k = StartDate.Year; k <= EndDate.Year; k++)
			{
				for (int l = 1; l <= 12; l++)
				{
					dictionary2.Add($"y{k}m{l}", 0m);
				}
			}
			foreach (object accountOrAuxItem in accountOrAuxItems)
			{
				SubsidiaryLedger subsidiaryLedger = null;
				string empty = string.Empty;
				if (!(accountOrAuxItem is Account account))
				{
					if (!(accountOrAuxItem is Tuple<Account, AuxiliaryItem> tuple))
					{
						continue;
					}
					empty = tuple.Item2.Name;
					subsidiaryLedger = Ledger.GetSubsidiaryLedger(tuple.Item1, StartDate, EndDate, tuple.Item2);
				}
				else
				{
					empty = account.Name;
					subsidiaryLedger = Ledger.GetSubsidiaryLedger(account, StartDate, EndDate);
				}
				column = grdMonthSummary.Cols.Add();
				column.UserData = accountOrAuxItem;
				column.Caption = empty;
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
				decimal num = default(decimal);
				for (int y = StartDate.Year; y <= EndDate.Year; y++)
				{
					int mon;
					for (mon = 1; mon <= 12; mon++)
					{
						string key = $"y{y}m{mon}";
						C1.Win.C1FlexGrid.Row row3 = dictionary[key];
						MonthSubsidiaryLedger monthSubsidiaryLedger = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Year == y && m.Month == mon);
						if (monthSubsidiaryLedger != null)
						{
							decimal totalValue = GetTotalValue(monthSubsidiaryLedger.Total, AnalysisProject);
							row3[column.Index] = totalValue;
							num += totalValue;
							dictionary2[key] += totalValue;
						}
					}
				}
				grdMonthSummary.Rows[grdMonthSummary.Rows.Count - 1][column.Index] = num;
			}
			column = grdMonthSummary.Cols.Add();
			column.Caption = ((AnalysisProject == AnalysisProject.Debits) ? "借方发生额合计" : ((AnalysisProject == AnalysisProject.Credits) ? "贷方发生额合计" : "合计"));
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			(column.Style ?? column.StyleNew).BackColor = Color.LightYellow;
			for (int n = StartDate.Year; n <= EndDate.Year; n++)
			{
				for (int num2 = 1; num2 <= 12; num2++)
				{
					string key2 = $"y{n}m{num2}";
					C1.Win.C1FlexGrid.Row row4 = dictionary[key2];
					row4[column.Index] = dictionary2[key2];
				}
			}
			grdMonthSummary.Rows[grdMonthSummary.Rows.Count - 1][column.Index] = dictionary2.Sum((KeyValuePair<string, decimal> m) => m.Value);
			grdMonthSummary.AutoSizeCols(0, grdMonthSummary.Cols.Count, 5);
			grdMonthSummary.Rows.Fixed = 1;
			grdMonthSummary.Cols.Fixed = 1;
			grdMonthSummary.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		}
		finally
		{
			grdMonthSummary.EndUpdate();
		}
	}

	private decimal GetTotalValue(SubsidiaryLedgerTotal slt, AnalysisProject ap)
	{
		return ap switch
		{
			AnalysisProject.Balance => slt.Balance, 
			AnalysisProject.Debits => slt.Debit, 
			AnalysisProject.Credits => slt.Credit, 
			_ => 0m, 
		};
	}
}
