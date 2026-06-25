using System;
using System.Collections;
using System.Collections.Generic;
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

public class StructureEditor : ISetTheme
{
	private const string MONEY_FORMAT = "#,0.00;-#,0.00;#";

	private LedgerViewer _owner;

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkCopy = new C1CommandLink();

	public C1SplitContainer View;

	private C1SplitterPanel pnlStructureContent;

	private C1SplitContainer ctnStructureContent;

	private C1SplitterPanel pnlStructureTitle;

	public C1SplitterPanel pnlStructureGird;

	public C1SplitterPanel pnlStructureChart;

	public Sunburst ChartStructure;

	public C1FlexGridEx grdStructureTable;

	private C1Label lblStructTitle;

	private C1SplitterPanel pnlSidebar;

	private C1ContextMenu ctxAnalyzyProject = new C1ContextMenu();

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1ContextMenu ctxFixed = new C1ContextMenu();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private DateTime _startTime => _owner.StartDate;

	private DateTime _endTime => _owner.EndDate;

	private Ledger _ledger => _owner.Ledger;

	public bool PendingAllEvent { get; set; }

	public ChartStatus PieChartStatus { get; set; }

	public AnalysisProject PieProject { get; set; } = AnalysisProject.Balance;


	public StructureEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitComponent();
		Initialize();
		BindContexMenu();
		grdStructureTable.AfterResizeColumn += _grid_AfterResizeColumn;
		grdStructureTable.AfterResizeRow += _grid_AfterResizeRow;
		grdStructureTable.AfterDragColumn += _grid_AfterDragColumn;
		ChartStructure.MouseDown += _chart_MouseDown;
	}

	public void ShowSideToolbar()
	{
		pnlSidebar?.Show();
	}

	public void HideSideToolbar()
	{
		pnlSidebar?.Hide();
	}

	private void InitComponent()
	{
		View = new C1SplitContainer();
		pnlStructureTitle = new C1SplitterPanel();
		pnlStructureContent = new C1SplitterPanel();
		ctnStructureContent = new C1SplitContainer();
		pnlStructureGird = new C1SplitterPanel();
		pnlStructureChart = new C1SplitterPanel();
		lblStructTitle = new C1Label();
		grdStructureTable = new C1FlexGridEx();
		grdStructureTable.Name = "grdStructureTable";
		ChartStructure = new Sunburst();
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblStructTitle.TextDetached = true;
		lblStructTitle.BorderStyle = BorderStyle.None;
		lblStructTitle.Dock = DockStyle.Fill;
		lblStructTitle.Font = font;
		lblStructTitle.Text = "结构分析图表";
		lblStructTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlStructureTitle.BackColor = Color.WhiteSmoke;
		pnlStructureTitle.Height = 30;
		pnlStructureTitle.KeepRelativeSize = false;
		pnlStructureTitle.Location = new Point(0, 0);
		pnlStructureTitle.Resizable = false;
		pnlStructureTitle.Size = new Size(927, 30);
		pnlStructureTitle.SizeRatio = 3.0;
		pnlStructureTitle.Controls.Add(lblStructTitle);
		pnlStructureTitle.Paint += delegate(object s1, PaintEventArgs e1)
		{
			e1.Graphics.DrawLine(panelBorderPen, 0, pnlStructureTitle.Height - 1, pnlStructureTitle.Width, pnlStructureTitle.Height - 1);
		};
		grdStructureTable.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdStructureTable.Dock = DockStyle.Fill;
		grdStructureTable.DrawMode = DrawModeEnum.OwnerDraw;
		grdStructureTable.Rows.DefaultSize = 20;
		grdStructureTable.AllowSorting = AllowSortingEnum.None;
		grdStructureTable.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		C1ToolBar c1ToolBar = new C1ToolBar();
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "科目余额";
		c1Command.Image = null;
		c1Command.UserData = AnalysisProject.Balance;
		c1Command.Click += CmdAnalyzyProject_Click;
		c1CommandLink.Command = c1Command;
		ctxAnalyzyProject.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "借方发生额";
		c1Command2.Image = null;
		c1Command2.UserData = AnalysisProject.Debits;
		c1Command2.Click += CmdAnalyzyProject_Click;
		c1CommandLink2.Command = c1Command2;
		ctxAnalyzyProject.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "贷方发生额";
		c1Command3.Image = null;
		c1Command3.UserData = AnalysisProject.Credits;
		c1Command3.Click += CmdAnalyzyProject_Click;
		c1CommandLink3.Command = c1Command3;
		ctxAnalyzyProject.CommandLinks.Add(c1CommandLink3);
		C1CommandLink c1CommandLink4 = new C1CommandLink();
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "分析选项";
		c1Command4.Image = Resources.sidebarAnalyzyProject;
		c1Command4.Click += CmdAnalyzyProject_Click1;
		c1CommandLink4.Command = c1Command4;
		c1ToolBar.CommandLinks.Add(c1CommandLink4);
		C1CommandLink c1CommandLink5 = new C1CommandLink();
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "窗体布局";
		c1Command5.Image = Resources.sidebarViewLayout;
		c1Command5.Click += CmdViewStyle_Click1;
		c1CommandLink5.Command = c1Command5;
		C1CommandLink c1CommandLink6 = new C1CommandLink();
		c1CommandLink6.Delimiter = true;
		C1Command c1Command6 = new C1Command();
		c1Command6.Text = "隐藏侧边栏";
		c1Command6.Image = Resources.sideHideSidebar;
		c1Command6.Click += delegate
		{
			_owner.OnHideSidebarClick();
		};
		c1CommandLink6.Command = c1Command6;
		foreach (C1CommandLink commandLink in c1ToolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		pnlStructureGird.HeaderTextAlign = PanelTextAlign.Center;
		pnlStructureGird.Height = 223;
		pnlStructureGird.Location = new Point(0, 31);
		pnlStructureGird.MinHeight = 0;
		pnlStructureGird.Size = new Size(927, 223);
		pnlStructureGird.SizeRatio = 40.0;
		pnlStructureGird.Hide();
		pnlStructureGird.Controls.Add(grdStructureTable);
		ChartStructure.Dock = DockStyle.Fill;
		ChartStructure.Legend.ItemMaxWidth = 0;
		ChartStructure.Legend.Orientation = C1.Chart.Orientation.Auto;
		ChartStructure.Legend.Position = Position.Right;
		ChartStructure.Legend.Reversed = false;
		ChartStructure.Legend.TextWrapping = TextWrapping.None;
		ChartStructure.SelectionStyle.StrokeColor = Color.Red;
		ChartStructure.ToolTip.Content = "{value}";
		pnlStructureChart.Height = 334;
		pnlStructureChart.Location = new Point(0, 256);
		pnlStructureChart.MinHeight = 0;
		pnlStructureChart.Size = new Size(927, 334);
		pnlStructureChart.SizeRatio = 100.0;
		pnlStructureChart.Controls.Add(ChartStructure);
		C1SplitContainer c1SplitContainer = new C1SplitContainer();
		c1SplitContainer.Dock = DockStyle.Fill;
		c1SplitContainer.Panels.Add(pnlStructureGird);
		c1SplitContainer.Panels.Add(pnlStructureChart);
		C1SplitContainer value = ComponentFactory.BuildSidebar(c1SplitContainer, c1ToolBar, out pnlSidebar);
		ctnStructureContent.AutoSizeElement = AutoSizeElement.Both;
		ctnStructureContent.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnStructureContent.Dock = DockStyle.Fill;
		ctnStructureContent.SplitterWidth = 2;
		ctnStructureContent.Panels.Add(pnlStructureTitle);
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel();
		c1SplitterPanel.Controls.Add(value);
		ctnStructureContent.Panels.Add(c1SplitterPanel);
		pnlStructureContent.Height = 590;
		pnlStructureContent.Location = new Point(0, 0);
		pnlStructureContent.Size = new Size(927, 590);
		pnlStructureContent.SizeRatio = 95.0;
		pnlStructureContent.Controls.Add(ctnStructureContent);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.SplitterWidth = 0;
		View.Panels.Add(pnlStructureContent);
	}

	private void CmdAnalyzyProject_Click1(object sender, ClickEventArgs e)
	{
		ctxAnalyzyProject.ShowContextMenu(e.CallerLink.Owner as C1ToolBar, new Point(e.CallerLink.Bounds.Left, e.CallerLink.Bounds.Bottom));
	}

	private void CmdViewStyle_Click1(object sender, ClickEventArgs e)
	{
		if (PieChartStatus == ChartStatus.Both)
		{
			PieChartStatus = ChartStatus.Diagram;
			SwitchToView(PieChartStatus);
		}
		else if (PieChartStatus == ChartStatus.Diagram)
		{
			PieChartStatus = ChartStatus.Table;
			SwitchToView(PieChartStatus);
		}
		else if (PieChartStatus == ChartStatus.Table)
		{
			PieChartStatus = ChartStatus.Both;
			SwitchToView(PieChartStatus);
		}
		else
		{
			PieChartStatus = ChartStatus.Both;
			SwitchToView(PieChartStatus);
		}
	}

	private void CmdAnalyzyProject_Click(object sender, ClickEventArgs e)
	{
		C1Command command = e.CallerLink.Command;
		if (command.UserData is AnalysisProject pieProject)
		{
			PieProject = pieProject;
			List<Node> selectedStructureNodes = _owner.AccountTreeEditor.SelectedStructureNodes;
			PopulateStructure(GetDataSource(selectedStructureNodes));
		}
	}

	public void PopulateStructure(List<PieItem> datasource)
	{
		try
		{
			CreateChart(datasource);
			CreateTable(datasource);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void PopulateStructure(List<Node> selectNodes)
	{
		try
		{
			List<PieItem> dataSource = GetDataSource(selectNodes);
			PopulateStructure(dataSource);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void SelectColumn(object userdata)
	{
		if (userdata != null)
		{
			C1.Win.C1FlexGrid.Column column = Common.FindColumn(grdStructureTable, userdata);
			if (column != null)
			{
				grdStructureTable.Col = column.Index;
			}
		}
	}

	public void SetTheme()
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(View);
		grdStructureTable.Styles.Fixed.Border.Color = Color.DarkGray;
		switch (Auditai.UI.Controls.Theme.SelectedAuditaiTheme.Name)
		{
		case "auditai_Office2013LightGray":
			pnlStructureChart.BackColor = Color.FromArgb(255, 255, 255);
			break;
		case "auditai_MacBlue":
			pnlStructureChart.BackColor = Color.FromArgb(250, 250, 250);
			break;
		case "auditai_MacSilver":
			pnlStructureChart.BackColor = Color.FromArgb(250, 250, 250);
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

	private void _chart_MouseDown(object sender, MouseEventArgs e)
	{
		try
		{
			C1.Chart.HitTestInfo hitTestInfo = ChartStructure.HitTest(e.Location);
			if (!(hitTestInfo.Item is PieItem { Account: not null } pieItem))
			{
				return;
			}
			foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)grdStructureTable.Rows)
			{
				if (item.UserData == pieItem.Account)
				{
					grdStructureTable.Row = item.Index;
					break;
				}
			}
		}
		catch
		{
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

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Common.SetSelectionToClipboard(grdStructureTable);
	}

	private void Initialize()
	{
		ChartStructure.ToolTip.Content = "{y:#,0.00;-#,0.00;#}";
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
			ctxCell.CommandLinks.Add(grdStructureTable.FilterManager.GenLnkFilter());
			ctxCell.CommandLinks.Add(grdStructureTable.FilterManager.GenLnkSample());
			ctxCell.CommandLinks.Add(grdStructureTable.FilterManager.GenLnkSelect());
			ctxCell.CommandLinks.Add(grdStructureTable.FilterManager.GenLnkCancelCurrentColumn());
			ctxEmpty.CommandLinks.Add(grdStructureTable.FilterManager.GenLnkCancelAll());
			ctxFixed.HideFirstDelimiter = true;
			C1Command c1Command = new C1Command();
			c1Command.Text = "隐藏本列";
			c1Command.UserData = grdStructureTable;
			c1Command.Click += _owner.ColHide_Click;
			C1CommandLink c1CommandLink = new C1CommandLink();
			c1CommandLink.Command = c1Command;
			ctxFixed.CommandLinks.Add(c1CommandLink);
			C1Command c1Command2 = new C1Command();
			c1Command2.Text = "取消隐藏";
			c1Command2.UserData = grdStructureTable;
			c1Command2.Click += _owner.CancelHide_Click;
			C1CommandLink c1CommandLink2 = new C1CommandLink();
			c1CommandLink2.Command = c1Command2;
			ctxFixed.CommandLinks.Add(c1CommandLink2);
			grdStructureTable.MouseClick += GrdStructureTable_MouseClick;
		}
		catch
		{
		}
	}

	private void GrdStructureTable_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdStructureTable.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxFixed.ShowContextMenu(grdStructureTable, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(grdStructureTable, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxCell.ShowContextMenu(grdStructureTable, e.Location);
				break;
			}
		}
	}

	private void SwitchToView(ChartStatus chart)
	{
		PieChartStatus = chart;
		switch (chart)
		{
		case ChartStatus.Diagram:
			pnlStructureGird.SizeRatio = 0.0;
			pnlStructureChart.SizeRatio = 100.0;
			ctnStructureContent.SplitterWidth = 0;
			break;
		case ChartStatus.Table:
			pnlStructureGird.SizeRatio = 100.0;
			pnlStructureChart.SizeRatio = 0.0;
			ctnStructureContent.SplitterWidth = 0;
			break;
		case ChartStatus.Both:
			pnlStructureGird.SizeRatio = 50.0;
			pnlStructureChart.SizeRatio = 100.0;
			ctnStructureContent.SplitterWidth = 4;
			break;
		}
	}

	private void CreateTable(List<PieItem> dataSource)
	{
		grdStructureTable.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			grdStructureTable.Cols.Count = 0;
			grdStructureTable.Rows.Count = 1;
			grdStructureTable.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grdStructureTable.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.Style.TextAlign = TextAlignEnum.CenterCenter;
			column = grdStructureTable.Cols.Add();
			column.Name = "Name";
			column.Caption = "项目名称";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column = grdStructureTable.Cols.Add();
			column.Name = "Amount";
			column.Caption = ((PieProject == AnalysisProject.Balance) ? "科目余额" : ((PieProject == AnalysisProject.Debits) ? "借方发生额" : "贷方发生额"));
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdStructureTable.Cols.Add();
			column.Name = "Ratio";
			column.Caption = "结构比";
			column.DataType = typeof(decimal);
			column.Format = "P";
			grdStructureTable.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grdStructureTable.Cols.Fixed = 1;
			grdStructureTable.Rows.Fixed = 1;
			grdStructureTable.Tree.Column = 1;
			grdStructureTable.AutoSizeCol(grdStructureTable.Tree.Column + 1);
			grdStructureTable.Tree.Show(1);
			decimal sum = default(decimal);
			foreach (PieItem item in dataSource)
			{
				sum += item.Value;
			}
			foreach (PieItem item2 in dataSource)
			{
				C1.Win.C1FlexGrid.Row row = grdStructureTable.Rows.Add();
				row.IsNode = true;
				Node node2 = row.Node;
				node2.Level = 0;
				node2.Data = item2.Name ?? "";
				node2.Key = item2.Account;
				row["Amount"] = item2.Value;
				if (sum != 0m)
				{
					row["Ratio"] = item2.Value / sum;
				}
				AddChildren(item2, node2);
			}
			for (int i = grdStructureTable.Rows.Fixed; i < grdStructureTable.Rows.Count; i++)
			{
				grdStructureTable.Rows[i]["Index"] = i - grdStructureTable.Rows.Fixed + 1;
			}
			_owner.StyleRecord.ResumeStyle(grdStructureTable);
			_owner.StyleRecord.ResumeStyle(_owner.AccountTreeEditor.Tree);
			grdStructureTable.AutoSizeCol(1);
			void AddChildren(PieItem item, Node node)
			{
				foreach (PieItem childItem in item.ChildItems)
				{
					C1.Win.C1FlexGrid.Row row2 = grdStructureTable.Rows.Add();
					row2.IsNode = true;
					Node node3 = row2.Node;
					node3.Level = node.Level + 1;
					node3.Data = childItem.Name;
					node3.Key = childItem.Account;
					row2["Amount"] = childItem.Value;
					if (sum != 0m)
					{
						row2["Ratio"] = childItem.Value / sum;
					}
					AddChildren(childItem, node3);
				}
			}
		}
		finally
		{
			PendingAllEvent = false;
			grdStructureTable.EndUpdate();
		}
	}

	private void CreateChart(List<PieItem> datasource)
	{
		ChartStructure.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			ChartStructure.Legend.Position = Position.Right;
			ChartStructure.DataSource = datasource;
			ChartStructure.Binding = "Value";
			ChartStructure.BindingName = ",,,,,,,,,,,,,,,,,,,,,,";
			ChartStructure.ChildItemsPath = "ChildItems";
			ChartStructure.DataLabel.Content = "{Name}\n{Display}";
			ChartStructure.DataLabel.Position = PieLabelPosition.Inside;
		}
		finally
		{
			PendingAllEvent = false;
			ChartStructure.EndUpdate();
		}
	}

	private List<PieItem> GetDataSource(List<Node> selected)
	{
		List<PieItem> list = new List<PieItem>();
		foreach (Node item in selected)
		{
			if (item != null)
			{
				Node node2 = item;
				list.Add(GetNodeItems(node2));
			}
		}
		if (list.Count == 1 && list[0].ChildItems.Count > 0)
		{
			list = list[0].ChildItems;
		}
		return list;
		PieItem GetNodeItems(Node node, Account account = null, object Aux = null)
		{
			SubsidiaryLedger subsidiaryLedger = null;
			PieItem pieItem = new PieItem();
			try
			{
				if (node.Key is Account account2)
				{
					account = account2;
				}
				if (!(Aux is AuxiliaryClass auxiliaryClass))
				{
					if (Aux is AuxiliaryItem auxiliaryItem)
					{
						subsidiaryLedger = _ledger.GetSubsidiaryLedger(account, _startTime, _endTime, auxiliaryItem);
						pieItem.Name = auxiliaryItem.Name;
					}
					else
					{
						subsidiaryLedger = _ledger.GetSubsidiaryLedger(account, _startTime, _endTime);
						pieItem.Name = account.Name;
					}
				}
				else
				{
					subsidiaryLedger = _ledger.GetSubsidiaryLedger(account, _startTime, _endTime, auxiliaryClass);
					pieItem.Name = auxiliaryClass.Name;
				}
				pieItem.Account = account;
				switch (_owner.StructureEditor.PieProject)
				{
				case AnalysisProject.Balance:
				{
					decimal num = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal.Balance ?? subsidiaryLedger.BeginBalance;
					object dCChar = Common.GetDCChar(account.IsDebit, num);
					pieItem.Value = Math.Abs(num);
					pieItem.Display = dCChar?.ToString() + "方余额：" + pieItem.Value.ToString("#,0.00;-#,0.00;#");
					break;
				}
				case AnalysisProject.Credits:
					pieItem.Value = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal.Credit ?? 0m;
					pieItem.Display = "贷方发生额：" + pieItem.Value.ToString("#,0.00;-#,0.00;#");
					break;
				case AnalysisProject.Debits:
					pieItem.Value = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal.Debit ?? 0m;
					pieItem.Display = "借方发生额：" + pieItem.Value.ToString("#,0.00;-#,0.00;#");
					break;
				}
				if (node.Nodes.Count() == 0)
				{
					return pieItem;
				}
				Node[] nodes = node.Nodes;
				foreach (Node node3 in nodes)
				{
					if (node3.Key is Account)
					{
						pieItem.ChildItems.Add(GetNodeItems(node3));
					}
					else if (node3.Key is Tuple<AuxiliaryClass, List<int>> tuple)
					{
						pieItem.ChildItems.Add(GetNodeItems(node3, account, tuple.Item1));
					}
					else if (node3.Key is Tuple<Account, AuxiliaryItem> tuple2)
					{
						pieItem.ChildItems.Add(GetNodeItems(node3, account, tuple2.Item2));
					}
				}
			}
			catch
			{
			}
			return pieItem;
		}
	}
}
