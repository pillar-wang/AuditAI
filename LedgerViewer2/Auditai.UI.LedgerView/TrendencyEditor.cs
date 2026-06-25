using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Chart;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using C1.Win.Chart;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.LedgerView.Properties;

namespace Auditai.UI.LedgerView;

public class TrendencyEditor : ISetTheme
{
	private const string CN_DATE = "Time";

	private const string CN_DATEDISPLAY = "TimeDisplay";

	private const string CN_MONTH = "Month";

	private const string CN_MONTHDISPLAY = "MonthDisplay";

	private const string AMOUNT = "Amount";

	private const string RATIO = "Ratio";

	private const string DATE = "Date";

	private const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	private LedgerViewer _owner;

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkCopy = new C1CommandLink();

	private C1SplitterPanel pnlTrendTitle;

	private C1SplitterPanel pnlTrendContent;

	private C1SplitContainer ctnTrendContent;

	private C1SplitterPanel pnlTrendGrid;

	private C1SplitterPanel pnlTrendChart;

	private C1Label lblTrendTitle;

	public C1FlexGridEx grdTrendTable;

	public FlexChart TrendChart;

	private C1SplitterPanel pnlSidebar;

	private C1ContextMenu ctxSidebarAnalyzyProject = new C1ContextMenu();

	private C1ContextMenu ctxSidebarAnalyzeMethod = new C1ContextMenu();

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1ContextMenu ctxFixed = new C1ContextMenu();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private DateTime StartDate => _owner.StartDate;

	private DateTime EndDate => _owner.EndDate;

	private Ledger Ledger => _owner.Ledger;

	public bool PendingAllEvent { get; set; }

	public ChartType ChartDisplay { get; set; }

	public AnalysisMethod AnalysisMethod { get; set; }

	public AnalysisProject AnalysisProject { get; set; }

	public ChartStatus TrendChartStatus { get; set; }

	public C1SplitContainer View { get; private set; }

	public TrendencyEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitializeComponent();
		BindContexMenu();
		Initialize();
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
		pnlTrendTitle = new C1SplitterPanel();
		pnlTrendContent = new C1SplitterPanel();
		ctnTrendContent = new C1SplitContainer();
		pnlTrendGrid = new C1SplitterPanel();
		pnlTrendChart = new C1SplitterPanel();
		lblTrendTitle = new C1Label();
		grdTrendTable = new C1FlexGridEx();
		grdTrendTable.Name = "grdTrendTable";
		TrendChart = new FlexChart();
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblTrendTitle.TextDetached = true;
		lblTrendTitle.BorderStyle = BorderStyle.None;
		lblTrendTitle.Dock = DockStyle.Fill;
		lblTrendTitle.Font = font;
		lblTrendTitle.Text = "趋势分析图表";
		lblTrendTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlTrendTitle.Height = 30;
		pnlTrendTitle.KeepRelativeSize = false;
		pnlTrendTitle.Location = new Point(0, 0);
		pnlTrendTitle.Resizable = false;
		pnlTrendTitle.Size = new Size(927, 30);
		pnlTrendTitle.SizeRatio = 6.0;
		pnlTrendTitle.Controls.Add(lblTrendTitle);
		pnlTrendTitle.Paint += delegate(object s1, PaintEventArgs e1)
		{
			e1.Graphics.DrawLine(panelBorderPen, 0, pnlTrendTitle.Height - 1, pnlTrendTitle.Width, pnlTrendTitle.Height - 1);
		};
		grdTrendTable.AllowEditing = false;
		grdTrendTable.AllowMerging = AllowMergingEnum.Custom;
		grdTrendTable.AllowResizing = AllowResizingEnum.Both;
		grdTrendTable.AllowSorting = AllowSortingEnum.None;
		grdTrendTable.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdTrendTable.Dock = DockStyle.Fill;
		grdTrendTable.DrawMode = DrawModeEnum.OwnerDraw;
		grdTrendTable.Rows.DefaultSize = 20;
		grdTrendTable.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		C1ToolBar c1ToolBar = new C1ToolBar();
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "按月分析";
		c1Command.Click += CmdSidebarByMonth_Click;
		c1CommandLink2.Command = c1Command;
		ctxSidebarAnalyzeMethod.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "按日分析";
		c1Command2.Click += CmdSidebarByDay_Click;
		c1CommandLink3.Command = c1Command2;
		ctxSidebarAnalyzeMethod.CommandLinks.Add(c1CommandLink3);
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "分析间隔";
		c1Command3.Image = Resources.sidebarAnalyzyInterval;
		c1Command3.Click += CmdSidebarAnalyzyMethod_Click1;
		c1CommandLink.Command = c1Command3;
		c1ToolBar.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink4 = new C1CommandLink();
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "科目余额";
		c1Command4.Image = null;
		c1Command4.UserData = AnalysisProject.Balance;
		c1Command4.Click += CmdSidebarAnalyzyProject_Click;
		c1CommandLink4.Command = c1Command4;
		ctxSidebarAnalyzyProject.CommandLinks.Add(c1CommandLink4);
		C1CommandLink c1CommandLink5 = new C1CommandLink();
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "借方发生额";
		c1Command5.Image = null;
		c1Command5.UserData = AnalysisProject.Debits;
		c1Command5.Click += CmdSidebarAnalyzyProject_Click;
		c1CommandLink5.Command = c1Command5;
		ctxSidebarAnalyzyProject.CommandLinks.Add(c1CommandLink5);
		C1CommandLink c1CommandLink6 = new C1CommandLink();
		C1Command c1Command6 = new C1Command();
		c1Command6.Text = "贷方发生额";
		c1Command6.Image = null;
		c1Command6.UserData = AnalysisProject.Credits;
		c1Command6.Click += CmdSidebarAnalyzyProject_Click;
		c1CommandLink6.Command = c1Command6;
		ctxSidebarAnalyzyProject.CommandLinks.Add(c1CommandLink6);
		C1CommandLink c1CommandLink7 = new C1CommandLink();
		C1Command c1Command7 = new C1Command();
		c1Command7.Text = "分析选项";
		c1Command7.Image = Resources.sidebarAnalyzyProject;
		c1Command7.Click += CmdSidebarAnalyzyProject_Click1;
		c1CommandLink7.Command = c1Command7;
		c1ToolBar.CommandLinks.Add(c1CommandLink7);
		C1CommandLink c1CommandLink8 = new C1CommandLink();
		C1Command c1Command8 = new C1Command();
		c1Command8.Text = "切换样式";
		c1Command8.Image = Resources.sideSwitchStyle;
		c1Command8.Click += CmdChartType_Click;
		c1CommandLink8.Command = c1Command8;
		c1ToolBar.CommandLinks.Add(c1CommandLink8);
		C1CommandLink c1CommandLink9 = new C1CommandLink();
		C1Command c1Command9 = new C1Command();
		c1Command9.Text = "窗体布局";
		c1Command9.Image = Resources.sidebarViewLayout;
		c1Command9.Click += CmdViewStyle_Click1;
		c1CommandLink9.Command = c1Command9;
		C1CommandLink c1CommandLink10 = new C1CommandLink();
		c1CommandLink10.Delimiter = true;
		C1Command c1Command10 = new C1Command();
		c1Command10.Text = "隐藏侧边栏";
		c1Command10.Image = Resources.sideHideSidebar;
		c1Command10.Click += delegate
		{
			_owner.OnHideSidebarClick();
		};
		c1CommandLink10.Command = c1Command10;
		foreach (C1CommandLink commandLink in c1ToolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		pnlTrendGrid.Height = 278;
		pnlTrendGrid.Location = new Point(0, 0);
		pnlTrendGrid.MinHeight = 0;
		pnlTrendGrid.Padding = new Padding(1, 1, 2, 2);
		pnlTrendGrid.Size = new Size(927, 278);
		pnlTrendGrid.Hide();
		pnlTrendGrid.Controls.Add(grdTrendTable);
		TrendChart.AxisX.Chart = TrendChart;
		TrendChart.AxisX.PlotAreaName = null;
		TrendChart.AxisX.Position = Position.Bottom;
		TrendChart.AxisY.AxisLine = false;
		TrendChart.AxisY.Chart = TrendChart;
		TrendChart.AxisY.MajorGrid = true;
		TrendChart.AxisY.MajorTickMarks = TickMark.None;
		TrendChart.AxisY.Position = Position.Left;
		TrendChart.Dock = DockStyle.Fill;
		TrendChart.Legend.ItemMaxWidth = 0;
		TrendChart.Legend.Orientation = C1.Chart.Orientation.Auto;
		TrendChart.Legend.Position = Position.Right;
		TrendChart.Legend.Reversed = false;
		TrendChart.Legend.TextWrapping = TextWrapping.None;
		TrendChart.Margin = new Padding(10);
		TrendChart.PlotMargin = new Padding(0);
		TrendChart.SelectionStyle.StrokeColor = Color.Red;
		TrendChart.Text = "flexChart1";
		TrendChart.ToolTip.Content = "{value}";
		TrendChart.DataLabel.Content = "{value}";
		TrendChart.DataLabel.Position = LabelPosition.Top;
		pnlTrendChart.Height = 278;
		pnlTrendChart.Location = new Point(0, 280);
		pnlTrendChart.MinHeight = 0;
		pnlTrendChart.Size = new Size(927, 278);
		pnlTrendChart.Controls.Add(TrendChart);
		ctnTrendContent.AutoSizeElement = AutoSizeElement.Both;
		ctnTrendContent.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnTrendContent.Dock = DockStyle.Fill;
		ctnTrendContent.LineBelowHeader = false;
		ctnTrendContent.SplitterWidth = 2;
		ctnTrendContent.Panels.Add(pnlTrendGrid);
		ctnTrendContent.Panels.Add(pnlTrendChart);
		C1SplitContainer value = ComponentFactory.BuildSidebar(ctnTrendContent, c1ToolBar, out pnlSidebar);
		pnlTrendContent.Height = 558;
		pnlTrendContent.Location = new Point(0, 31);
		pnlTrendContent.Resizable = false;
		pnlTrendContent.Size = new Size(927, 558);
		pnlTrendContent.SizeRatio = 95.0;
		pnlTrendContent.Controls.Add(value);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.BackColor = Color.FromArgb(240, 240, 240);
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.ForeColor = Color.FromArgb(0, 0, 0);
		View.LineBelowHeader = false;
		View.SplitterWidth = 2;
		View.Panels.Add(pnlTrendTitle);
		View.Panels.Add(pnlTrendContent);
	}

	private void CmdSidebarByDay_Click(object sender, ClickEventArgs e)
	{
		AnalysisMethod = AnalysisMethod.ByDay;
		List<Node> selectedTrendencyNodes = _owner.AccountTreeEditor.SelectedTrendencyNodes;
		PopulateTrendency(selectedTrendencyNodes);
	}

	private void CmdSidebarByMonth_Click(object sender, ClickEventArgs e)
	{
		AnalysisMethod = AnalysisMethod.ByMonth;
		List<Node> selectedTrendencyNodes = _owner.AccountTreeEditor.SelectedTrendencyNodes;
		PopulateTrendency(selectedTrendencyNodes);
	}

	private void CmdViewStyle_Click1(object sender, ClickEventArgs e)
	{
		if (TrendChartStatus == ChartStatus.Both)
		{
			TrendChartStatus = ChartStatus.Diagram;
			SwitchToView(TrendChartStatus);
		}
		else if (TrendChartStatus == ChartStatus.Diagram)
		{
			TrendChartStatus = ChartStatus.Table;
			SwitchToView(TrendChartStatus);
		}
		else if (TrendChartStatus == ChartStatus.Table)
		{
			TrendChartStatus = ChartStatus.Both;
			SwitchToView(TrendChartStatus);
		}
		else
		{
			TrendChartStatus = ChartStatus.Both;
			SwitchToView(TrendChartStatus);
		}
	}

	private void CmdChartType_Click(object sender, ClickEventArgs e)
	{
		if (ChartDisplay == ChartType.Column)
		{
			ChartDisplay = ChartType.Line;
			List<Node> selectedTrendencyNodes = _owner.AccountTreeEditor.SelectedTrendencyNodes;
			PopulateTrendency(selectedTrendencyNodes);
		}
		else if (ChartDisplay == ChartType.Line)
		{
			ChartDisplay = ChartType.Column;
			List<Node> selectedTrendencyNodes2 = _owner.AccountTreeEditor.SelectedTrendencyNodes;
			PopulateTrendency(selectedTrendencyNodes2);
		}
		else
		{
			ChartDisplay = ChartType.Column;
			List<Node> selectedTrendencyNodes3 = _owner.AccountTreeEditor.SelectedTrendencyNodes;
			PopulateTrendency(selectedTrendencyNodes3);
		}
	}

	private void CmdSidebarAnalyzyProject_Click1(object sender, ClickEventArgs e)
	{
		ctxSidebarAnalyzyProject.ShowContextMenu(e.CallerLink.Owner as C1ToolBar, new Point(e.CallerLink.Bounds.Left, e.CallerLink.Bounds.Bottom));
	}

	private void CmdSidebarAnalyzyMethod_Click1(object sender, ClickEventArgs e)
	{
		ctxSidebarAnalyzeMethod.ShowContextMenu(e.CallerLink.Owner as C1ToolBar, new Point(e.CallerLink.Bounds.Left, e.CallerLink.Bounds.Bottom));
	}

	private void CmdSidebarAnalyzyProject_Click(object sender, ClickEventArgs e)
	{
		C1Command command = e.CallerLink.Command;
		if (command.UserData is AnalysisProject analysisProject)
		{
			AnalysisProject = analysisProject;
			List<Node> selectedTrendencyNodes = _owner.AccountTreeEditor.SelectedTrendencyNodes;
			PopulateTrendency(selectedTrendencyNodes);
		}
	}

	public void SelectColumn(object userdata)
	{
		if (userdata != null)
		{
			C1.Win.C1FlexGrid.Column column = Common.FindColumn(grdTrendTable, userdata);
			if (column != null)
			{
				grdTrendTable.Col = column.Index;
			}
		}
	}

	public void PopulateTrendency(List<Node> selected)
	{
		if (AnalysisMethod == AnalysisMethod.ByMonth)
		{
			DataTable dataTable = DataSourceByMonth(selected);
			GenerateChartByMonth(dataTable);
			GenerateTableByMonth(dataTable);
		}
		else
		{
			DataTable dataSource = DataSourceByDay(selected);
			GenerateChartByDay(dataSource);
			GenerateTableByDay(dataSource);
		}
	}

	public void SetTheme()
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(View);
		grdTrendTable.Styles.Fixed.Border.Color = Color.DarkGray;
		switch (Auditai.UI.Controls.Theme.SelectedAuditaiTheme.Name)
		{
		case "auditai_Office2013LightGray":
			pnlTrendChart.BackColor = Color.FromArgb(255, 255, 255);
			break;
		case "auditai_MacBlue":
			pnlTrendChart.BackColor = Color.FromArgb(250, 250, 250);
			break;
		case "auditai_MacSilver":
			pnlTrendChart.BackColor = Color.FromArgb(250, 250, 250);
			break;
		}
		if (Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
	}

	public void SetStatus()
	{
	}

	private void _chart_MouseDown(object sender, MouseEventArgs e)
	{
		try
		{
			C1.Chart.HitTestInfo hitTestInfo = TrendChart.HitTest(e.Location);
			if (hitTestInfo.Series == null)
			{
				return;
			}
			string binding = hitTestInfo.Series.Binding;
			foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)grdTrendTable.Cols)
			{
				if (item.Name.StartsWith(binding))
				{
					grdTrendTable.Col = item.Index;
					break;
				}
			}
			SelectRow(hitTestInfo.X);
		}
		catch
		{
		}
	}

	private void TableView_AfterResizeRow(object sender, RowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			_owner.StyleRecord.RecordHeight(sender as C1FlexGridEx, e.Row);
		}
	}

	private void TableView_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		if (PendingAllEvent)
		{
			return;
		}
		grdTrendTable.BeginUpdate();
		try
		{
			if (grdTrendTable.Cols[e.Col].Name.EndsWith("Amount"))
			{
				_owner.StyleRecord.ViewStyle.AmountWidth = grdTrendTable.Cols[e.Col].Width;
				SetAllColumn((C1.Win.C1FlexGrid.Column col) => col.Name.EndsWith("Amount"), _owner.StyleRecord.ViewStyle.AmountWidth);
			}
			else if (grdTrendTable.Cols[e.Col].Name.EndsWith("Ratio"))
			{
				_owner.StyleRecord.ViewStyle.RatioWidth = grdTrendTable.Cols[e.Col].Width;
				SetAllColumn((C1.Win.C1FlexGrid.Column col) => col.Name.EndsWith("Ratio"), _owner.StyleRecord.ViewStyle.RatioWidth);
			}
			if (grdTrendTable.Cols[e.Col].Name == "Date")
			{
				_owner.StyleRecord.ViewStyle.DateWidth = grdTrendTable.Cols[e.Col].Width;
			}
		}
		catch
		{
		}
		finally
		{
			grdTrendTable.EndUpdate();
		}
		C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
		_owner.StyleRecord.RecordWidth(c1FlexGridEx.Name, c1FlexGridEx.Cols[e.Col].Name, c1FlexGridEx.Cols[e.Col].Width);
	}

	private void TableView_AfterDragColumn(object sender, DragRowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordOrder(c1FlexGridEx.Name, from C1.Win.C1FlexGrid.Column t in c1FlexGridEx.Cols
				select t.Name);
		}
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Common.SetSelectionToClipboard(grdTrendTable);
	}

	private void Initialize()
	{
		TrendChart.MouseDown += _chart_MouseDown;
		grdTrendTable.AfterResizeRow += TableView_AfterResizeRow;
		grdTrendTable.AfterResizeColumn += TableView_AfterResizeColumn;
		grdTrendTable.AfterDragColumn += TableView_AfterDragColumn;
		ChartDisplay = ChartType.Column;
		AnalysisMethod = AnalysisMethod.ByMonth;
		AnalysisProject = AnalysisProject.Balance;
		TrendChart.ToolTip.Content = "{y:#,0.00;-#,0.00;#}";
		TrendChart.Legend.Position = Position.Right;
	}

	private void SwitchToView(ChartStatus chart)
	{
		TrendChartStatus = chart;
		switch (chart)
		{
		case ChartStatus.Diagram:
			pnlTrendGrid.SizeRatio = 0.0;
			pnlTrendChart.SizeRatio = 100.0;
			ctnTrendContent.SplitterWidth = 0;
			break;
		case ChartStatus.Table:
			pnlTrendGrid.SizeRatio = 100.0;
			pnlTrendChart.SizeRatio = 0.0;
			ctnTrendContent.SplitterWidth = 0;
			break;
		case ChartStatus.Both:
			pnlTrendGrid.SizeRatio = 50.0;
			pnlTrendChart.SizeRatio = 100.0;
			ctnTrendContent.SplitterWidth = 4;
			break;
		}
	}

	private void BindContexMenu()
	{
		cmdCopy.Text = "复制";
		cmdCopy.Image = ContextResources.ctxCopy;
		cmdCopy.Click += CmdCopy_Click;
		lnkCopy.Command = cmdCopy;
		ctxCell.CommandLinks.Add(lnkCopy);
		ctxCell.CommandLinks.Add(grdTrendTable.FilterManager.GenLnkFilter());
		ctxCell.CommandLinks.Add(grdTrendTable.FilterManager.GenLnkSample());
		ctxCell.CommandLinks.Add(grdTrendTable.FilterManager.GenLnkSelect());
		ctxCell.CommandLinks.Add(grdTrendTable.FilterManager.GenLnkCancelCurrentColumn());
		ctxEmpty.CommandLinks.Add(grdTrendTable.FilterManager.GenLnkCancelAll());
		ctxFixed.HideFirstDelimiter = true;
		C1Command c1Command = new C1Command();
		c1Command.Text = "隐藏本列";
		c1Command.UserData = grdTrendTable;
		c1Command.Click += _owner.ColHide_Click;
		C1CommandLink c1CommandLink = new C1CommandLink();
		c1CommandLink.Command = c1Command;
		ctxFixed.CommandLinks.Add(c1CommandLink);
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "取消隐藏";
		c1Command2.UserData = grdTrendTable;
		c1Command2.Click += _owner.CancelHide_Click;
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		c1CommandLink2.Command = c1Command2;
		ctxFixed.CommandLinks.Add(c1CommandLink2);
		grdTrendTable.MouseClick += GrdTrendTable_MouseClick;
	}

	private void GrdTrendTable_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdTrendTable.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxFixed.ShowContextMenu(grdTrendTable, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(grdTrendTable, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxCell.ShowContextMenu(grdTrendTable, e.Location);
				break;
			}
		}
	}

	private DataTable DataSourceByDay(List<Node> selected)
	{
		DataTable dataTable = new DataTable("source");
		dataTable.Columns.Add("Time", typeof(string));
		dataTable.Columns.Add("TimeDisplay", typeof(string));
		Dictionary<DateTime, int> dictionary = new Dictionary<DateTime, int>();
		foreach (Node item in selected)
		{
			Account account = null;
			object aux = null;
			string empty = string.Empty;
			object key = item.Key;
			if (!(key is Account account2))
			{
				if (!(key is Tuple<Account, AuxiliaryClass> tuple))
				{
					if (!(key is Tuple<Account, AuxiliaryItem> tuple2))
					{
						continue;
					}
					account = tuple2.Item1;
					aux = tuple2.Item2;
					empty = tuple2.Item1.Code + "－" + tuple2.Item2.Code + tuple2.Item2.Name;
				}
				else
				{
					account = tuple.Item1;
					aux = tuple.Item2;
					empty = tuple.Item2.Code + tuple.Item2.Name;
				}
			}
			else
			{
				account = account2;
				empty = account.Code + account.Name;
			}
			List<Tuple<DateTime, decimal>> list = null;
			switch (AnalysisProject)
			{
			case AnalysisProject.Balance:
				list = Ledger.GetBalanceByDay(account, StartDate, EndDate, aux);
				break;
			case AnalysisProject.Credits:
				list = Ledger.GetCreditsByDay(account, StartDate, EndDate, aux);
				break;
			case AnalysisProject.Debits:
				list = Ledger.GetDebitsByDay(account, StartDate, EndDate, aux);
				break;
			default:
				continue;
			}
			if (!dataTable.Columns.Contains(empty))
			{
				dataTable.Columns.Add(empty, typeof(decimal));
			}
			foreach (Tuple<DateTime, decimal> item2 in list)
			{
				if (!dictionary.ContainsKey(item2.Item1))
				{
					DataRow dataRow = dataTable.Rows.Add();
					dataRow["Time"] = item2.Item1.ToString("yyyy-MM-dd");
					dataRow["TimeDisplay"] = item2.Item1.ToString("yyyy-MM-dd");
					dictionary.Add(item2.Item1, dataTable.Rows.Count - 1);
				}
				DataRow dataRow2 = dataTable.Rows[dictionary[item2.Item1]];
				dataRow2[empty] = item2.Item2;
			}
		}
		dataTable.DefaultView.Sort = "Time ASC";
		return dataTable.DefaultView.ToTable();
	}

	private void GenerateChartByDay(DataTable dataSource)
	{
		TrendChart.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			TrendChart.Series.Clear();
			foreach (DataColumn column in dataSource.Columns)
			{
				if (!(column.ColumnName == "Time") && !(column.ColumnName == "TimeDisplay"))
				{
					Series series = new Series();
					series.Binding = column.ColumnName;
					series.Name = column.ColumnName;
					TrendChart.Series.Add(series);
				}
			}
			TrendChart.ChartType = ChartDisplay;
			TrendChart.DataSource = dataSource;
			TrendChart.BindingX = "Time";
			TrendChart.AxisX.LabelAngle = 45.0;
			TrendChart.ToolTip.Content = "{y:#,0.00;-#,0.00;#}";
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "生成趋势图异常！");
		}
		finally
		{
			PendingAllEvent = false;
			TrendChart.EndUpdate();
		}
	}

	private void GenerateTableByDay(DataTable dataSource)
	{
		grdTrendTable.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			grdTrendTable.Cols.Count = 1;
			grdTrendTable.Cols.Fixed = 1;
			grdTrendTable.Rows.Count = 2;
			grdTrendTable.Rows.Fixed = 2;
			grdTrendTable.MergedRanges.Clear();
			C1.Win.C1FlexGrid.Column column = grdTrendTable.Cols[0];
			column.Name = "Index";
			column.Caption = "序号";
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 40;
			column = grdTrendTable.Cols.Add();
			column.Name = "Time";
			column.Caption = "日期";
			column.TextAlign = TextAlignEnum.CenterCenter;
			grdTrendTable.MergedRanges.Add(0, 0, 1, 0);
			grdTrendTable.MergedRanges.Add(0, 1, 1, 1);
			grdTrendTable.AllowMerging = AllowMergingEnum.Custom;
			Dictionary<string, C1.Win.C1FlexGrid.Row> dictionary = new Dictionary<string, C1.Win.C1FlexGrid.Row>();
			foreach (DataColumn column2 in dataSource.Columns)
			{
				if (column2.ColumnName == "Time" || column2.ColumnName == "TimeDisplay")
				{
					continue;
				}
				AuxiliaryItem item;
				Account accountByColumnName = GetAccountByColumnName(column2.ColumnName, out item);
				if (accountByColumnName == null)
				{
					continue;
				}
				column = grdTrendTable.Cols.Add();
				column.UserData = ((item == null) ? ((object)accountByColumnName) : ((object)Tuple.Create(accountByColumnName, item)));
				column.Name = column2.ColumnName;
				column.Format = "#,0.00;-#,0.00;#";
				if (_owner.StyleRecord.ViewStyle.AmountWidth != 0)
				{
					column.Width = _owner.StyleRecord.ViewStyle.AmountWidth;
				}
				switch (AnalysisProject)
				{
				case AnalysisProject.Balance:
					grdTrendTable.Rows[1][column.Index] = "科目余额";
					break;
				case AnalysisProject.Debits:
					grdTrendTable.Rows[1][column.Index] = "借方发生额";
					break;
				case AnalysisProject.Credits:
					grdTrendTable.Rows[1][column.Index] = "贷方发生额";
					break;
				}
				grdTrendTable.Rows[0][column.Index] = ((item == null) ? (accountByColumnName.Code + accountByColumnName.Name) : (accountByColumnName.Code + "－" + item.Code + item.Name));
				foreach (DataRow row3 in dataSource.Rows)
				{
					if (row3[column2.ColumnName] is decimal num)
					{
						string text = row3["Time"].ToString();
						if (!dictionary.ContainsKey(text))
						{
							C1.Win.C1FlexGrid.Row row = grdTrendTable.Rows.Add();
							row.UserData = text;
							row["Time"] = row3["Time"];
							dictionary.Add(text, row);
						}
						C1.Win.C1FlexGrid.Row row2 = dictionary[text];
						row2[column2.ColumnName] = num;
					}
				}
			}
			grdTrendTable.AllowSorting = AllowSortingEnum.SingleColumn;
			grdTrendTable.Sort(SortFlags.Ascending, 1);
			grdTrendTable.AllowSorting = AllowSortingEnum.None;
			grdTrendTable.ShowSortPosition = ShowSortPositionEnum.None;
			for (int i = grdTrendTable.Rows.Fixed; i < grdTrendTable.Rows.Count; i++)
			{
				grdTrendTable.Rows[i]["Index"] = i - grdTrendTable.Rows.Fixed + 1;
			}
			_owner.StyleRecord.ResumeStyle(grdTrendTable);
			_owner.StyleRecord.ResumeStyle(_owner.AccountTreeEditor.Tree);
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "生成趋势表异常！");
		}
		finally
		{
			PendingAllEvent = false;
			grdTrendTable.EndUpdate();
		}
	}

	private DataTable DataSourceByMonth(List<Node> selected)
	{
		DataTable dataTable = new DataTable("source");
		Dictionary<string, DataRow> dictionary = new Dictionary<string, DataRow>();
		dataTable.Columns.Add("Month", typeof(string));
		dataTable.Columns.Add("MonthDisplay", typeof(string));
		for (int i = StartDate.Year; i <= EndDate.Year; i++)
		{
			for (int j = 1; j <= 12; j++)
			{
				DataRow dataRow = dataTable.Rows.Add();
				string key = (string)(dataRow["Month"] = $"y{i}m{j}");
				dataRow["MonthDisplay"] = $"{i}-{j.ToString().PadLeft(2, '0')}";
				dictionary.Add(key, dataRow);
			}
		}
		foreach (Node item in selected)
		{
			SubsidiaryLedger subsidiaryLedger = null;
			string empty = string.Empty;
			object key2 = item.Key;
			if (!(key2 is Account account))
			{
				if (!(key2 is Tuple<Account, AuxiliaryClass> tuple))
				{
					if (!(key2 is Tuple<Account, AuxiliaryItem> tuple2))
					{
						continue;
					}
					empty = tuple2.Item1.Code + "－" + tuple2.Item2.Code + tuple2.Item2.Name;
					subsidiaryLedger = Ledger.GetSubsidiaryLedger(tuple2.Item1, StartDate, EndDate, tuple2.Item2);
				}
				else
				{
					empty = tuple.Item2.Code + tuple.Item2.Name;
					subsidiaryLedger = Ledger.GetSubsidiaryLedger(tuple.Item1, StartDate, EndDate, tuple.Item2);
				}
			}
			else
			{
				empty = account.Code + account.Name;
				subsidiaryLedger = Ledger.GetSubsidiaryLedger(account, StartDate, EndDate);
			}
			empty = empty.Replace(".", "");
			if (!dataTable.Columns.Contains(empty))
			{
				dataTable.Columns.Add(empty, typeof(decimal));
			}
			if (AnalysisProject == AnalysisProject.Balance)
			{
				decimal num = subsidiaryLedger.BeginBalance;
				for (int y = StartDate.Year; y <= EndDate.Year; y++)
				{
					int mon;
					for (mon = 1; mon <= 12; mon++)
					{
						DataRow dataRow2 = dictionary[$"y{y}m{mon}"];
						if (subsidiaryLedger.Months.Any((MonthSubsidiaryLedger m) => m.Year == y && m.Month == mon))
						{
							num = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Year == y && m.Month == mon).Total.Balance;
							dataRow2[empty] = num;
						}
						else
						{
							dataRow2[empty] = num;
						}
					}
				}
				continue;
			}
			foreach (MonthSubsidiaryLedger item2 in from m in subsidiaryLedger.Months
				orderby m.Year, m.Month
				select m)
			{
				DataRow dataRow3 = dictionary[$"y{item2.Year}m{item2.Month}"];
				switch (AnalysisProject)
				{
				case AnalysisProject.Credits:
					dataRow3[empty] = item2.Total.Credit;
					break;
				case AnalysisProject.Debits:
					dataRow3[empty] = item2.Total.Debit;
					break;
				}
			}
		}
		dataTable.DefaultView.Sort = "MonthDisplay ASC";
		return dataTable.DefaultView.ToTable();
	}

	private DataTable GenerateChartByMonth(DataTable dataSource)
	{
		try
		{
			TrendChart.Series.Clear();
			foreach (DataColumn column in dataSource.Columns)
			{
				if (!(column.ColumnName == "Month") && !(column.ColumnName == "MonthDisplay"))
				{
					Series series = new Series();
					series.Binding = column.ColumnName;
					series.Name = column.ColumnName;
					TrendChart.Series.Add(series);
				}
			}
			TrendChart.ToolTip.Content = "{y:#,0.00;-#,0.00;#}";
			TrendChart.ChartType = ChartDisplay;
			TrendChart.DataSource = dataSource;
			TrendChart.BindingX = "MonthDisplay";
			TrendChart.AxisX.LabelAngle = 0.0;
			return dataSource;
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "生成趋势图异常！");
			return null;
		}
	}

	private void GenerateTableByMonth(DataTable dataTable)
	{
		grdTrendTable.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			grdTrendTable.Cols.Count = 1;
			grdTrendTable.Cols.Fixed = 1;
			grdTrendTable.Rows.Count = 2;
			grdTrendTable.Rows.Fixed = 2;
			grdTrendTable.MergedRanges.Clear();
			C1.Win.C1FlexGrid.Column column = grdTrendTable.Cols.Add();
			column.Name = "Time";
			column.Caption = "月份";
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = ((_owner.StyleRecord.ViewStyle.DateWidth == 0) ? 80 : _owner.StyleRecord.ViewStyle.DateWidth);
			grdTrendTable.MergedRanges.Add(0, 1, 1, 1);
			grdTrendTable.AllowMerging = AllowMergingEnum.Custom;
			Dictionary<string, C1.Win.C1FlexGrid.Row> dictionary = new Dictionary<string, C1.Win.C1FlexGrid.Row>();
			for (int i = StartDate.Year; i <= EndDate.Year; i++)
			{
				for (int j = 1; j <= 12; j++)
				{
					C1.Win.C1FlexGrid.Row row = grdTrendTable.Rows.Add();
					row[0] = row.Index - grdTrendTable.Rows.Fixed + 1;
					row.UserData = new DateTime(i, j, 1);
					row["Time"] = $"{i}年{j}月";
					dictionary.Add($"y{i}m{j}", row);
				}
			}
			foreach (DataColumn column2 in dataTable.Columns)
			{
				if (column2.ColumnName == "Month")
				{
					continue;
				}
				AuxiliaryItem item;
				Account accountByColumnName = GetAccountByColumnName(column2.ColumnName, out item);
				if (accountByColumnName == null)
				{
					continue;
				}
				string text = column2.ColumnName + "Amount";
				column = grdTrendTable.Cols.Add();
				column.UserData = ((item == null) ? ((object)accountByColumnName) : ((object)Tuple.Create(accountByColumnName, item)));
				column.Name = text;
				column.Caption = ((item == null) ? (accountByColumnName.Code + accountByColumnName.Name) : (accountByColumnName.Code + "－" + item.Code + item.Name));
				column.Format = "#,0.00;-#,0.00;#";
				switch (AnalysisProject)
				{
				case AnalysisProject.Balance:
					grdTrendTable.Rows[1][column.Index] = "科目余额";
					break;
				case AnalysisProject.Debits:
					grdTrendTable.Rows[1][column.Index] = "借方发生额";
					break;
				case AnalysisProject.Credits:
					grdTrendTable.Rows[1][column.Index] = "贷方发生额";
					break;
				}
				if (_owner.StyleRecord.ViewStyle.AmountWidth != 0)
				{
					column.Width = _owner.StyleRecord.ViewStyle.AmountWidth;
				}
				string text2 = column2.ColumnName + "Ratio";
				column = grdTrendTable.Cols.Add();
				column.UserData = ((item == null) ? ((object)accountByColumnName) : ((object)Tuple.Create(accountByColumnName, item)));
				column.Name = text2;
				grdTrendTable.Rows[1][column.Index] = "环比";
				if (_owner.StyleRecord.ViewStyle.RatioWidth != 0)
				{
					column.Width = _owner.StyleRecord.ViewStyle.RatioWidth;
				}
				grdTrendTable.MergedRanges.Add(0, column.Index - 1, 0, column.Index);
				foreach (DataRow row3 in dataTable.Rows)
				{
					if (!(row3[column2.ColumnName] is decimal num))
					{
						continue;
					}
					C1.Win.C1FlexGrid.Row row2 = dictionary[row3["Month"].ToString()];
					row2[text] = num;
					if (row2.Index - 1 >= grdTrendTable.Rows.Fixed)
					{
						object obj = grdTrendTable.Rows[row2.Index - 1][text];
						if (num != 0m && obj is decimal num2 && num2 != 0m)
						{
							row2[text2] = (num / num2).ToString("P");
						}
					}
				}
			}
			_owner.StyleRecord.ResumeStyle(grdTrendTable);
			_owner.StyleRecord.ResumeStyle(_owner.AccountTreeEditor.Tree);
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "生成趋势表异常！");
		}
		finally
		{
			PendingAllEvent = false;
			grdTrendTable.EndUpdate();
		}
	}

	private Account GetAccountByColumnName(string columnName, out AuxiliaryItem item)
	{
		item = null;
		TrialBalanceSheet trialBalanceSheetWithCache = _owner.CacheManager.GetTrialBalanceSheetWithCache(Ledger);
		foreach (Account account in Ledger.Accounts)
		{
			if (account.Code + account.Name == columnName)
			{
				return account;
			}
			foreach (KeyValuePair<AuxiliaryItem, decimal> item2 in trialBalanceSheetWithCache.End[account].ClassBalances.SelectMany((KeyValuePair<AuxiliaryClass, ClassBalance> c) => c.Value.ItemBalances))
			{
				if (account.Code + "－" + item2.Key.Code + item2.Key.Name == columnName)
				{
					item = item2.Key;
					return account;
				}
			}
		}
		return null;
	}

	private void SetAllColumn(Predicate<C1.Win.C1FlexGrid.Column> predicate, int width)
	{
		foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)grdTrendTable.Cols)
		{
			if (predicate(item))
			{
				item.Width = width;
			}
		}
	}

	private void SelectRow(object userData)
	{
		if (userData == null || !DateTime.TryParse(userData.ToString(), out var result))
		{
			return;
		}
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)grdTrendTable.Rows)
		{
			if (item.UserData != null && DateTime.TryParse(item.UserData.ToString(), out var result2) && result.Equals(result2))
			{
				grdTrendTable.Row = item.Index;
				break;
			}
		}
	}
}
