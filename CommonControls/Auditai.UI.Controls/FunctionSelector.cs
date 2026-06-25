using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using Auditai.Model;

namespace Auditai.UI.Controls;

[Obfuscation(ApplyToMembers = false, Exclude = true, StripAfterObfuscation = false)]
public class FunctionSelector : DropDownForm
{
	private class GirdRowData
	{
		public FunctionInfo FunInfo;

		public string FunName;

		public string FunDesc;
	}

	private struct FunctionInfo
	{
		public MethodInfo Source { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public string Category { get; set; }

		public string DisplayName { get; set; }
	}

	private C1DockingTab dock;

	private C1FlexGrid lstAllGrid;

	private C1DockingTabPage tabAll;

	private List<FunctionInfo> functions;

	private List<C1FlexGrid> _gridsList = new List<C1FlexGrid>();

	private readonly SolidBrush _brushHoverBackground = new SolidBrush(Color.Transparent);

	private int _mouseOverRow = -1;

	public MethodInfo SelectedFunction { get; private set; }

	public Func<string, bool> CheckFunctionIsVisibleCallback { get; set; }

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Controls.FunctionSelector));
		this.dock = new C1.Win.C1Command.C1DockingTab();
		this.tabAll = new C1.Win.C1Command.C1DockingTabPage();
		((System.ComponentModel.ISupportInitialize)this.dock).BeginInit();
		this.dock.SuspendLayout();
		this.tabAll.SuspendLayout();
		base.SuspendLayout();
		this.dock.Controls.Add(this.tabAll);
		this.dock.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dock.Location = new System.Drawing.Point(0, 0);
		this.dock.Name = "dock";
		this.dock.Size = new System.Drawing.Size(341, 300);
		this.dock.TabIndex = 0;
		this.dock.TabsSpacing = 5;
		this.dock.TabsShowFocusCues = false;
		this.dock.TabStyle = C1.Win.C1Command.TabStyleEnum.WindowsXP;
		this.dock.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.dock.TabClick += new System.EventHandler(Dock_TabClick);
		this.tabAll.Location = new System.Drawing.Point(2, 28);
		this.tabAll.Name = "tabAll";
		this.tabAll.Size = new System.Drawing.Size(335, 268);
		this.tabAll.TabIndex = 0;
		this.tabAll.Text = "全部";
		base.ClientSize = new System.Drawing.Size(341, 300);
		base.Controls.Add(this.dock);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Name = "FunctionSelector";
		base.Open += new System.EventHandler(FunctionSelector_Open);
		base.Shown += new System.EventHandler(FunctionsDropDownForm_Shown);
		((System.ComponentModel.ISupportInitialize)this.dock).EndInit();
		this.dock.ResumeLayout(false);
		this.tabAll.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	private void Dock_TabClick(object sender, EventArgs e)
	{
		_mouseOverRow = -1;
	}

	public FunctionSelector()
	{
		InitializeComponent();
		InitGrid();
		dock.TabSizeMode = TabSizeModeEnum.FillToEnd;
	}

	private void InitGrid()
	{
		functions = (from f in typeof(FunctionEvaluator).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
			orderby (Attribute.GetCustomAttribute(f, typeof(OrderAttribute)) as OrderAttribute)?.Order
			select f).Select(delegate(MethodInfo f)
		{
			FunctionInfo result = default(FunctionInfo);
			result.Source = f;
			result.Name = f.Name;
			result.DisplayName = (Attribute.GetCustomAttribute(f, typeof(DisplayNameAttribute)) as DisplayNameAttribute).DisplayName;
			result.Description = (Attribute.GetCustomAttribute(f, typeof(DescriptionAttribute)) as DescriptionAttribute).Description;
			result.Category = (Attribute.GetCustomAttribute(f, typeof(CategoryAttribute)) as CategoryAttribute).Category;
			return result;
		}).ToList();
		List<GirdRowData> list = new List<GirdRowData>();
		foreach (FunctionInfo function in functions)
		{
			list.Add(new GirdRowData
			{
				FunInfo = function,
				FunName = function.Name,
				FunDesc = function.DisplayName
			});
		}
		lstAllGrid = new C1FlexGridEx();
		InitFunListGrid(lstAllGrid, list);
		tabAll.Controls.Add(lstAllGrid);
		_gridsList.Add(lstAllGrid);
		foreach (IGrouping<string, FunctionInfo> item in from p in functions
			group p by p.Category)
		{
			C1DockingTabPage c1DockingTabPage = new C1DockingTabPage();
			c1DockingTabPage.Location = new Point(2, 26);
			c1DockingTabPage.Name = item.Key;
			c1DockingTabPage.TabIndex = 0;
			c1DockingTabPage.Text = item.Key;
			dock.TabPages.Add(c1DockingTabPage);
			List<GirdRowData> list2 = new List<GirdRowData>();
			foreach (FunctionInfo item2 in item)
			{
				list2.Add(new GirdRowData
				{
					FunInfo = item2,
					FunName = item2.Name,
					FunDesc = item2.DisplayName
				});
			}
			C1FlexGridEx c1FlexGridEx = new C1FlexGridEx();
			InitFunListGrid(c1FlexGridEx, list2);
			_gridsList.Add(c1FlexGridEx);
			c1DockingTabPage.Controls.Add(c1FlexGridEx);
		}
		SetTheme();
	}

	private void InitFunListGrid(C1FlexGrid grid, List<GirdRowData> dataList)
	{
		grid.BeginUpdate();
		int num = 1;
		int num2 = 2;
		grid.Rows.Count = 0;
		grid.Rows.Fixed = 0;
		grid.Rows.Frozen = 0;
		grid.Cols.Count = num2;
		grid.Cols.Fixed = 0;
		grid.Cols.Frozen = 0;
		grid.Rows.DefaultSize = 30;
		grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grid.DoubleBuffer = true;
		grid.AllowEditing = false;
		grid.AllowFiltering = false;
		grid.AllowDelete = false;
		grid.AllowResizing = AllowResizingEnum.Columns;
		grid.AllowSorting = AllowSortingEnum.None;
		grid.AllowDragging = AllowDraggingEnum.None;
		grid.AllowFreezing = AllowFreezingEnum.None;
		grid.DrawMode = DrawModeEnum.OwnerDraw;
		grid.SelectionMode = SelectionModeEnum.Row;
		grid.FocusRect = FocusRectEnum.None;
		grid.ExtendLastCol = true;
		grid.OwnerDrawCell += FunGrid_OwnerDrawCell;
		grid.MouseMove += FunGrid_MouseMove;
		grid.MouseLeave += FunGrid_MouseLeave;
		grid.MouseClick += FunGrid_MouseClick;
		grid.MouseEnter += FunGrid_MouseEnter;
		for (int i = 0; i < num2; i++)
		{
			C1.Win.C1FlexGrid.Column column = grid.Cols[i];
			column.DataType = typeof(string);
		}
		grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		grid.Rows.Count = dataList.Count + num;
		grid.Rows.Fixed = num;
		grid[0, 0] = "名称";
		grid[0, 1] = "功能";
		grid.Cols[0].Width = 110;
		grid.Width = tabAll.Width + 4;
		grid.Height = tabAll.Height + 4;
		C1.Win.C1FlexGrid.CellStyle styleNew = grid.Rows[0].StyleNew;
		styleNew.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
		for (int j = 0; j < dataList.Count; j++)
		{
			GirdRowData girdRowData = dataList[j];
			C1.Win.C1FlexGrid.Row row = grid.Rows[num + j];
			grid[num + j, 0] = girdRowData.FunName;
			grid[num + j, 1] = girdRowData.FunDesc;
			row.UserData = girdRowData;
		}
		grid.EndUpdate();
		grid.Select(-1, -1);
	}

	private void FunGrid_MouseLeave(object sender, EventArgs e)
	{
		_mouseOverRow = -1;
		if (sender is C1FlexGrid c1FlexGrid)
		{
			c1FlexGrid.Invalidate();
		}
	}

	private void FunGrid_MouseMove(object sender, MouseEventArgs e)
	{
		if (sender is C1FlexGrid c1FlexGrid)
		{
			HitTestInfo hitTestInfo = c1FlexGrid.HitTest();
			if (_mouseOverRow != hitTestInfo.Row)
			{
				_mouseOverRow = hitTestInfo.Row;
				c1FlexGrid.Invalidate();
			}
		}
	}

	private void FunGrid_MouseEnter(object sender, EventArgs e)
	{
		if (!(sender is C1FlexGrid c1FlexGrid))
		{
			return;
		}
		try
		{
			c1FlexGrid.Focus();
		}
		catch
		{
		}
	}

	private void FunGrid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (sender is C1FlexGrid c1FlexGrid)
		{
			if (e.Row < c1FlexGrid.Rows.Fixed)
			{
				e.Style.ForeColor = c1FlexGrid.Styles.Fixed.ForeColor;
				e.Style.BackColor = c1FlexGrid.Styles.Fixed.BackColor;
			}
			else if (e.Row == _mouseOverRow)
			{
				e.Graphics.FillRectangle(_brushHoverBackground, e.Bounds);
			}
		}
	}

	private void FunGrid_MouseClick(object sender, MouseEventArgs e)
	{
		if (sender is C1FlexGrid c1FlexGrid)
		{
			HitTestInfo hitTestInfo = c1FlexGrid.HitTest();
			if (hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Row >= c1FlexGrid.Rows.Fixed && hitTestInfo.Row < c1FlexGrid.Rows.Count && c1FlexGrid.Rows[hitTestInfo.Row].UserData is GirdRowData girdRowData)
			{
				SelectedFunction = girdRowData.FunInfo.Source;
				CloseDropDown();
			}
		}
	}

	private void FunctionsDropDownForm_Shown(object sender, EventArgs e)
	{
	}

	private bool IsFunctionVisible(FunctionInfo funInfo)
	{
		if (CheckFunctionIsVisibleCallback == null)
		{
			return true;
		}
		return CheckFunctionIsVisibleCallback(funInfo.Name);
	}

	public void SetTheme()
	{
		Theme.SetCurrentTree(this);
		_brushHoverBackground.Color = Color.FromArgb(100, Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background"));
		foreach (C1FlexGrid grids in _gridsList)
		{
			grids.Styles.Alternate.Clear();
		}
	}

	public void RefreshFuncList()
	{
		foreach (C1FlexGrid grids in _gridsList)
		{
			grids.Select(-1, -1);
			int count = grids.Rows.Count;
			for (int i = grids.Rows.Fixed; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grids.Rows[i];
				if (!(row.UserData is GirdRowData girdRowData))
				{
					row.Visible = false;
				}
				else if (!IsFunctionVisible(girdRowData.FunInfo))
				{
					row.Visible = false;
				}
				else
				{
					row.Visible = true;
				}
			}
		}
	}

	private void FunctionSelector_Open(object sender, EventArgs e)
	{
		SelectedFunction = null;
		try
		{
			int num = dock.TabIndex;
			if (num >= 0 && num < _gridsList.Count)
			{
				C1FlexGrid c1FlexGrid = _gridsList[num];
			}
		}
		catch
		{
		}
	}
}
