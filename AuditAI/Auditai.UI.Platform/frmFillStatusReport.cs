using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class frmFillStatusReport : Form
{
	private const int RCOL_STATUS = 0;

	private const int RCOL_RATE = 1;

	private const int RCOL_NAME = 2;

	private const int RCOL_GROUP = 3;

	private const int RCOL_SHOULD = 4;

	private const int RCOL_FILLED = 5;

	private const int NCOL_CHECK = 0;

	private const int NCOL_NAME = 1;

	private const int NCOL_GROUP = 2;

	private class FillStatusRow
	{
		public TreeTableNode Node;

		public string TableName;

		public string GroupPath;

		public bool Required;

		public bool StatFailed;

		public int ShouldFill;

		public int Filled;

		public double Rate;

		public bool CanRestore;

		public string StatusText = "无法统计";

		public string RateText = "—";
	}

	private List<FillStatusRow> _rows = new List<FillStatusRow>();

	private Panel _pnlSummary;

	private Panel _pnlBottom;

	private SplitContainer _split;

	private Panel _pnlRequired;

	private Panel _pnlNotRequired;

	private C1FlexGridEx _gridRequired;

	private C1FlexGridEx _gridNotRequired;

	private C1Button _btnRestore;

	private C1Button _btnClose;

	private readonly Font _fontTitle = new Font("微软雅黑", 12f, FontStyle.Bold);

	private readonly Font _fontCount = new Font("微软雅黑", 9.5f);

	private readonly Font _fontSection = new Font("微软雅黑", 10.5f, FontStyle.Bold);

	private readonly Font _fontGrid = new Font("微软雅黑", 10.5f);

	private int _requiredCount;

	private int _filledCount;

	private int _fillingCount;

	private int _unfilledCount;

	private int _notRequiredCount;

	private int _totalShould;

	private int _totalFilled;

	public Auditai.Model.Project Project { get; set; }

	public frmFillStatusReport()
	{
		InitializeComponent();
		InitializeGrids();
		base.Shown += frmFillStatusReport_Shown;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_fontTitle.Dispose();
			_fontCount.Dispose();
			_fontSection.Dispose();
			_fontGrid.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this._pnlSummary = new Panel();
		this._pnlBottom = new Panel();
		this._split = new SplitContainer();
		this._pnlRequired = new Panel();
		this._pnlNotRequired = new Panel();
		this._gridRequired = new C1FlexGridEx();
		this._gridNotRequired = new C1FlexGridEx();
		this._btnRestore = new C1Button();
		this._btnClose = new C1Button();
		this._split.SuspendLayout();
		base.SuspendLayout();
		this._pnlSummary.Dock = DockStyle.Top;
		this._pnlSummary.BackColor = Color.FromArgb(221, 231, 238);
		this._pnlSummary.Height = 76;
		this._pnlSummary.Paint += _pnlSummary_Paint;
		this._pnlBottom.Dock = DockStyle.Bottom;
		this._pnlBottom.BackColor = Color.FromArgb(221, 231, 238);
		this._pnlBottom.Height = 50;
		this._btnRestore.Location = new Point(618, 10);
		this._btnRestore.Size = new Size(110, 30);
		this._btnRestore.Text = "恢复填报";
		this._btnRestore.Click += _btnRestore_Click;
		this._btnClose.Location = new Point(740, 10);
		this._btnClose.Size = new Size(110, 30);
		this._btnClose.Text = "关闭";
		this._btnClose.DialogResult = DialogResult.Cancel;
		this._btnClose.Click += _btnClose_Click;
		this._pnlBottom.Controls.Add(this._btnRestore);
		this._pnlBottom.Controls.Add(this._btnClose);
		this._pnlRequired.Dock = DockStyle.Fill;
		this._pnlRequired.BackColor = Color.White;
		this._pnlNotRequired.Dock = DockStyle.Fill;
		this._pnlNotRequired.BackColor = Color.White;
		Label label = new Label
		{
			Text = "必填表格",
			Dock = DockStyle.Top,
			Height = 30,
			TextAlign = ContentAlignment.MiddleLeft,
			BackColor = Color.FromArgb(221, 231, 238),
			ForeColor = Color.FromArgb(21, 66, 139),
			Font = _fontSection,
			Padding = new Padding(10, 0, 0, 0)
		};
		Label label2 = new Label
		{
			Text = "不必填报",
			Dock = DockStyle.Top,
			Height = 30,
			TextAlign = ContentAlignment.MiddleLeft,
			BackColor = Color.FromArgb(221, 231, 238),
			ForeColor = Color.FromArgb(21, 66, 139),
			Font = _fontSection,
			Padding = new Padding(10, 0, 0, 0)
		};
		this._pnlRequired.Controls.Add(this._gridRequired);
		this._pnlRequired.Controls.Add(label);
		this._pnlNotRequired.Controls.Add(this._gridNotRequired);
		this._pnlNotRequired.Controls.Add(label2);
		this._split.Dock = DockStyle.Fill;
		this._split.Orientation = Orientation.Horizontal;
		this._split.IsSplitterFixed = true;
		this._split.SplitterWidth = 6;
		this._split.BackColor = Color.FromArgb(119, 147, 185);
		this._split.Panel1.Controls.Add(this._pnlRequired);
		this._split.Panel2.Controls.Add(this._pnlNotRequired);
		base.AutoScaleDimensions = new SizeF(7f, 17f);
		base.AutoScaleMode = AutoScaleMode.Font;
		base.ClientSize = new Size(900, 640);
		base.Controls.Add(this._split);
		base.Controls.Add(this._pnlSummary);
		base.Controls.Add(this._pnlBottom);
		base.Font = new Font("微软雅黑", 10.5f, FontStyle.Regular, GraphicsUnit.Point, 134);
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmFillStatusReport";
		base.ShowInTaskbar = false;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.CancelButton = this._btnClose;
		this.Text = "填报情况统计";
		this._split.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	private void InitializeGrids()
	{
		InitGridCommon(_gridRequired, 30);
		AddCol(_gridRequired, "状态", "status", TextAlignEnum.CenterCenter);
		AddCol(_gridRequired, "填报率", "rate", TextAlignEnum.CenterCenter);
		AddCol(_gridRequired, "表格名称", "name", TextAlignEnum.LeftCenter);
		AddCol(_gridRequired, "所在分组/目录", "group", TextAlignEnum.LeftCenter);
		AddCol(_gridRequired, "应填单元格数", "should", TextAlignEnum.CenterCenter);
		AddCol(_gridRequired, "已填单元格数", "filled", TextAlignEnum.CenterCenter);
		_gridRequired.BindAutoSizeColsFill(this);
		InitGridCommon(_gridNotRequired, 33);
		C1.Win.C1FlexGrid.Column column = _gridNotRequired.Cols.Add();
		column.Caption = "恢复填报";
		column.Name = "check";
		column.AllowEditing = true;
		column.TextAlign = TextAlignEnum.CenterCenter;
		AddCol(_gridNotRequired, "表格名称", "name", TextAlignEnum.LeftCenter);
		AddCol(_gridNotRequired, "所在分组/目录", "group", TextAlignEnum.LeftCenter);
		_gridNotRequired.Glyphs[GlyphEnum.Grayed] = IconRes.NoPermission;
		_gridNotRequired.BeforeMouseDown += _gridNotRequired_BeforeMouseDown;
		_gridNotRequired.BeforeEdit += _gridNotRequired_BeforeEdit;
		_gridNotRequired.BindAutoSizeColsFill(this);
	}

	private void InitGridCommon(C1FlexGridEx grid, int rowHeight)
	{
		grid.Dock = DockStyle.Fill;
		grid.ExtendLastCol = true;
		grid.AllowEditing = true;
		grid.ScrollBars = ScrollBars.Vertical;
		grid.AllowSorting = AllowSortingEnum.None;
		grid.DrawMode = DrawModeEnum.OwnerDraw;
		grid.FocusRect = FocusRectEnum.None;
		grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grid.Styles.Normal.Border.Width = 0;
		grid.Rows.DefaultSize = rowHeight;
		grid.Font = _fontGrid;
		grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		grid.Rows.Count = 1;
		grid.Rows.Fixed = 1;
		grid.Cols.Count = 0;
		grid.Cols.Fixed = 0;
		grid.Paint += _grid_Paint;
	}

	private static void AddCol(C1FlexGridEx grid, string caption, string name, TextAlignEnum align)
	{
		C1.Win.C1FlexGrid.Column column = grid.Cols.Add();
		column.Caption = caption;
		column.Name = name;
		column.AllowEditing = false;
		column.TextAlign = align;
	}

	private void frmFillStatusReport_Shown(object sender, EventArgs e)
	{
		if (_split.Height > 120)
		{
			_split.SplitterDistance = _split.Height * 58 / 100;
		}
		StartCount();
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
		if (c1FlexGridEx != null)
		{
			c1FlexGridEx.DrawFormBorder(e.Graphics);
		}
	}

	private void _btnClose_Click(object sender, EventArgs e)
	{
		base.Close();
	}

	private void _btnRestore_Click(object sender, EventArgs e)
	{
		int num = 0;
		List<string> list = new List<string>();
		for (int i = _gridNotRequired.Rows.Fixed; i < _gridNotRequired.Rows.Count; i++)
		{
			if (_gridNotRequired.GetCellCheck(i, NCOL_CHECK) != CheckEnum.Checked)
			{
				continue;
			}
			FillStatusRow fillStatusRow = _gridNotRequired.Rows[i].UserData as FillStatusRow;
			if (fillStatusRow != null && fillStatusRow.CanRestore && fillStatusRow.Node != null)
			{
				fillStatusRow.Node.UpdateVisible(true);
				if (!fillStatusRow.Node.AllAncestorsVisible())
				{
					list.Add(fillStatusRow.TableName);
				}
				num++;
			}
		}
		if (num == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Information, "请先勾选需要恢复填报的表格。", MessageBoxButtons.OK, "恢复填报", scroll: false);
			return;
		}
		if (list.Count > 0)
		{
			// 父目录隐藏时子表虽已恢复，但树上仍不可达——提示用户先取消隐藏所在目录
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Information, "以下表格已恢复填报，但其所在目录处于隐藏状态，需先将目录【取消隐藏】才能在项目树中看到：\r\n" + string.Join("、", list), MessageBoxButtons.OK, "恢复填报", scroll: true);
		}
		Program.MainForm.ProjectHierarchy.Populate();
		StartCount();
	}

	private void _gridNotRequired_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = _gridNotRequired.HitTest(e.X, e.Y);
		if (hitTestInfo.Type != HitTestTypeEnum.Checkbox || hitTestInfo.Column != NCOL_CHECK)
		{
			return;
		}
		e.Cancel = true;
		if (hitTestInfo.Row < _gridNotRequired.Rows.Fixed || hitTestInfo.Row >= _gridNotRequired.Rows.Count)
		{
			return;
		}
		FillStatusRow fillStatusRow = _gridNotRequired.Rows[hitTestInfo.Row].UserData as FillStatusRow;
		if (fillStatusRow == null || !fillStatusRow.CanRestore)
		{
			return;
		}
		CheckEnum checkEnum = _gridNotRequired.GetCellCheck(hitTestInfo.Row, NCOL_CHECK);
		_gridNotRequired.SetCellCheck(hitTestInfo.Row, NCOL_CHECK, (checkEnum == CheckEnum.Checked) ? CheckEnum.Unchecked : CheckEnum.Checked);
	}

	private void _gridNotRequired_BeforeEdit(object sender, RowColEventArgs e)
	{
		e.Cancel = true;
	}

	private void StartCount()
	{
		List<FillStatusRow> list = new List<FillStatusRow>();
		List<TreeTableNode> list2 = Project.GetAllTableNodes().ToList();
		List<TreeTableNode> list3 = list2.Where((TreeTableNode n) => n.Visible).ToList();
		List<TreeTableNode> list4 = list2.Where((TreeTableNode n) => !n.Visible).ToList();
		int num = list3.Count((TreeTableNode n) => n.Table.LocalExists);
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		ProgressForm2 progressForm2 = new ProgressForm2();
		progressForm2.ShowDialogOnUiThread(progressRuntimeData, async delegate
		{
			IProgress<ProgressInfo> progress = new ProgressRuntimeDataReporter(progressRuntimeData);
			int num2 = 0;
			foreach (TreeTableNode node in list3)
			{
				progress.Report(new ProgressInfo
				{
					MainCaption = "正在统计 " + node.Name,
					MainProgress = (num == 0) ? 100 : ((int)((double)(num2 + 1) / (double)num * 100.0))
				});
				if (!node.Table.LocalExists)
				{
					FillStatusRow fillStatusRow = BuildRow(node, required: true);
					fillStatusRow.StatFailed = true;
					list.Add(fillStatusRow);
					continue;
				}
				FillStatusRow item = await Task.Run(() => CountRequiredTable(node));
				num2++;
				list.Add(item);
			}
			foreach (TreeTableNode item2 in list4)
			{
				list.Add(BuildRow(item2, required: false));
			}
		});
		_rows = list;
		UpdateSummary();
		PopulateRequired();
		PopulateNotRequired();
	}

	private static FillStatusRow CountRequiredTable(TreeTableNode node)
	{
		FillStatusRow fillStatusRow = BuildRow(node, required: true);
		try
		{
			node.Table.LoadAndReturn();
			Auditai.Model.Table table = node.Table;
			if (table.IsCorrupted)
			{
				fillStatusRow.StatFailed = true;
				return fillStatusRow;
			}
			int num = 0;
			int num2 = 0;
			// 单遍遍历全表 Cells（Row.GetCells() 每行全表扫描为 O(rows²×cols)，大表统计明显变慢）
			// 按 (行Index, 列Index) 去重：Merge 冲突后同位置可能共存多个 Cell（加载修复清孤儿但不去重），
			// 与 table[row,col]/UI 展示取单值的口径保持一致，避免应填/已填双计。
			bool[] seen = new bool[table.Rows.Count * Math.Max(1, table.Columns.Count)];
			foreach (Auditai.Model.Cell cell in table.Cells)
			{
				Auditai.Model.Row row = cell.Row;
				Auditai.Model.Column column = cell.Column;
				if (row == null || column == null || row.Role == RowRole.Header || !row.Visible || !column.Visible)
				{
					continue;
				}
				int cellIndex = row.Index * table.Columns.Count + column.Index;
				if (cellIndex < 0 || cellIndex >= seen.Length || seen[cellIndex])
				{
					continue;
				}
				seen[cellIndex] = true;
				if (cell.HasCellFormulaOrColumnFormula || !cell.IsEditable)
				{
					continue;
				}
				num++;
				if (!cell.IsValueEmpty)
				{
					num2++;
				}
			}
			fillStatusRow.ShouldFill = num;
			fillStatusRow.Filled = num2;
			fillStatusRow.Rate = ((num == 0) ? 1.0 : ((double)num2 / (double)num));
			fillStatusRow.RateText = ((num == 0) ? "—" : ((int)Math.Round(fillStatusRow.Rate * 100.0) + "%"));
			if (num == 0 || num2 >= num)
			{
				fillStatusRow.StatusText = "已填写";
			}
			else if (num2 == 0)
			{
				fillStatusRow.StatusText = "未填写";
			}
			else
			{
				fillStatusRow.StatusText = "填写中";
			}
		}
		catch (Exception)
		{
			fillStatusRow.StatFailed = true;
		}
		return fillStatusRow;
	}

	private static FillStatusRow BuildRow(TreeTableNode node, bool required)
	{
		return new FillStatusRow
		{
			Node = node,
			TableName = node.Name,
			GroupPath = GetGroupPath(node),
			Required = required
		};
	}

	private static string GetGroupPath(TreeNodeBase node)
	{
		if (node.Parent == null)
		{
			if (node.Group != null)
			{
				return node.Group.Name;
			}
			return string.Empty;
		}
		Stack<string> stack = new Stack<string>();
		for (TreeDirectoryNode treeDirectoryNode = node.Parent; treeDirectoryNode != null; treeDirectoryNode = treeDirectoryNode.Parent)
		{
			stack.Push(treeDirectoryNode.Name);
		}
		return string.Join("/", stack);
	}

	private static int GetStatusOrder(FillStatusRow row)
	{
		switch (row.StatusText)
		{
		case "未填写":
			return 0;
		case "填写中":
			return 1;
		case "已填写":
			return 2;
		default:
			return 3;
		}
	}

	private static Color GetStatusColor(FillStatusRow row)
	{
		switch (row.StatusText)
		{
		case "未填写":
			return Color.FromArgb(192, 0, 0);
		case "填写中":
			return Color.FromArgb(176, 96, 0);
		case "已填写":
			return Color.FromArgb(0, 128, 96);
		default:
			return Color.FromArgb(100, 116, 139);
		}
	}

	private void UpdateSummary()
	{
		List<FillStatusRow> list = _rows.Where((FillStatusRow r) => r.Required).ToList();
		_requiredCount = list.Count;
		_totalShould = list.Where((FillStatusRow r) => !r.StatFailed).Sum((FillStatusRow r) => r.ShouldFill);
		_totalFilled = list.Where((FillStatusRow r) => !r.StatFailed).Sum((FillStatusRow r) => r.Filled);
		_filledCount = list.Count((FillStatusRow r) => r.StatusText == "已填写");
		_fillingCount = list.Count((FillStatusRow r) => r.StatusText == "填写中");
		_unfilledCount = list.Count((FillStatusRow r) => r.StatusText == "未填写");
		_notRequiredCount = _rows.Count((FillStatusRow r) => !r.Required);
		_pnlSummary.Invalidate();
	}

	private void PopulateRequired()
	{
		C1FlexGridEx grid = _gridRequired;
		grid.BeginUpdate();
		try
		{
			grid.Rows.Count = grid.Rows.Fixed;
			List<FillStatusRow> list = _rows.Where((FillStatusRow r) => r.Required)
				.OrderBy((FillStatusRow r) => GetStatusOrder(r))
				.ThenBy((FillStatusRow r) => r.Rate)
				.ThenBy((FillStatusRow r) => r.TableName, StringComparer.OrdinalIgnoreCase)
				.ToList();
			foreach (FillStatusRow item in list)
			{
				int num = grid.Rows.Count;
				grid.Rows.Add();
				grid[num, RCOL_STATUS] = item.StatusText;
				grid[num, RCOL_RATE] = item.RateText;
				grid[num, RCOL_NAME] = item.TableName;
				grid[num, RCOL_GROUP] = item.GroupPath;
				grid[num, RCOL_SHOULD] = (item.StatFailed ? "—" : item.ShouldFill.ToString());
				grid[num, RCOL_FILLED] = (item.StatFailed ? "—" : item.Filled.ToString());
				grid.Rows[num].StyleNew.ForeColor = GetStatusColor(item);
			}
		}
		finally
		{
			grid.EndUpdate();
		}
		grid.AutoSizeColsFill(90);
	}

	private void PopulateNotRequired()
	{
		C1FlexGridEx grid = _gridNotRequired;
		grid.BeginUpdate();
		try
		{
			grid.Rows.Count = grid.Rows.Fixed;
			foreach (FillStatusRow item in _rows.Where((FillStatusRow r) => !r.Required)
				.OrderBy((FillStatusRow r) => r.GroupPath, StringComparer.OrdinalIgnoreCase)
				.ThenBy((FillStatusRow r) => r.TableName, StringComparer.OrdinalIgnoreCase)
				.ToList())
			{
				// 恢复填报是写操作：用模型层真实权限判定（CanRemoveNode 仅检查树选中态，不反映该行节点权限）
				item.CanRestore = item.Node != null && item.Node.HasWritePermission();
				int num = grid.Rows.Count;
				grid.Rows.Add();
				grid[num, NCOL_NAME] = item.TableName;
				grid[num, NCOL_GROUP] = item.GroupPath;
				grid.Rows[num].UserData = item;
				grid.SetCellCheck(num, NCOL_CHECK, item.CanRestore ? CheckEnum.Unchecked : CheckEnum.Grayed);
			}
		}
		finally
		{
			grid.EndUpdate();
		}
		grid.AutoSizeColsFill(80);
	}

	private void _pnlSummary_Paint(object sender, PaintEventArgs e)
	{
		Rectangle clientRectangle = _pnlSummary.ClientRectangle;
		Color color = Color.FromArgb(21, 66, 139);
		string text = ((_totalShould > 0) ? ("总体进度：" + (int)Math.Round((double)_totalFilled * 100.0 / (double)_totalShould) + "%") : "总体进度：—");
		TextRenderer.DrawText(e.Graphics, text, _fontTitle, new Rectangle(clientRectangle.Left + 16, clientRectangle.Top + 6, 320, 32), color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
		int x = clientRectangle.Left + 16;
		int y = clientRectangle.Top + 42;
		x = DrawCount(e.Graphics, x, y, "必填 " + _requiredCount, color);
		x = DrawCount(e.Graphics, x, y, "已填写 " + _filledCount, Color.FromArgb(0, 128, 96));
		x = DrawCount(e.Graphics, x, y, "填写中 " + _fillingCount, Color.FromArgb(176, 96, 0));
		x = DrawCount(e.Graphics, x, y, "未填写 " + _unfilledCount, Color.FromArgb(192, 0, 0));
		DrawCount(e.Graphics, x, y, "不必填报 " + _notRequiredCount, Color.FromArgb(100, 116, 139));
	}

	private int DrawCount(Graphics g, int x, int y, string text, Color color)
	{
		Size size = TextRenderer.MeasureText(g, text, _fontCount);
		TextRenderer.DrawText(g, text, _fontCount, new Rectangle(x, y, size.Width + 12, 24), color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
		return x + size.Width + 36;
	}
}
