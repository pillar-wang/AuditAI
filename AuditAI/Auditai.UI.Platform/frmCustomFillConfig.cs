using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Ribbon;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class frmCustomFillConfig : C1RibbonForm
{
	private C1FlexGrid _grid;
	private StatusStrip _statusStrip;
	private ToolStripStatusLabel _lblRuleCount;
	private ToolStripStatusLabel _lblValidation;
	private ToolStripStatusLabel _lblCurrentTable;
	private ContextMenuStrip _contextMenu;

	private bool _hasUnsavedChanges;

	// ===== 选区模式（类似 Excel 公式编辑器）=====
	private bool _isSelectionMode;
	private int _selectingRow = -1;
	private int _selectingCol = -1;
	private Size _savedWindowSize;
	private Point _savedWindowLocation;
	private string _selectedRangeText = "";
	private Label _selModeLabel;
	private Button _selModeOk;
	private Button _selModeCancel;

	/// <summary>所有表格节点（用于表格选择下拉）</summary>
	private readonly List<(Id64 Id, string Title)> _allTables = new();

	/// <summary>"(当前表格)"占位项</summary>
	private const string CurrentTableOption = "(当前表格)";

	/// <summary>条件运算符显示名称映射</summary>
	private static readonly Dictionary<string, Auditai.Model.ConditionOperator> _opMap = new()
	{
		{ "等于", Auditai.Model.ConditionOperator.Equals },
		{ "不等于", Auditai.Model.ConditionOperator.NotEquals },
		{ "包含", Auditai.Model.ConditionOperator.Contains },
		{ "不包含", Auditai.Model.ConditionOperator.NotContains },
		{ "大于", Auditai.Model.ConditionOperator.GreaterThan },
		{ "小于", Auditai.Model.ConditionOperator.LessThan },
		{ "为空", Auditai.Model.ConditionOperator.IsEmpty },
		{ "非空", Auditai.Model.ConditionOperator.IsNotEmpty },
	};

	private static readonly Dictionary<Auditai.Model.ConditionOperator, string> _opReverseMap;

	private const string OpList = "等于|不等于|包含|不包含|大于|小于|为空|非空";

	// Column indices
	private const int ColTable = 0;
	private const int ColValue = 1;
	private const int ColTarget = 2;
	private const int ColMode = 3;
	private const int ColCondSource = 4;
	private const int ColCondOp = 5;
	private const int ColCondValue = 6;
	private const int ColCount = 7;

	static frmCustomFillConfig()
	{
		_opReverseMap = new();
		foreach (var kv in _opMap)
			_opReverseMap[kv.Value] = kv.Key;
	}

	public frmCustomFillConfig()
	{
		InitializeComponent();
		Load += OnLoad;
		FormClosing += OnFormClosing;
		KeyPreview = true;
		KeyDown += OnKeyDown;
	}

	private void InitializeComponent()
	{
		Text = "自定义填充配置";
		Size = new Size(1200, 600);
		MinimumSize = new Size(900, 400);
		StartPosition = FormStartPosition.CenterParent;
		FormBorderStyle = FormBorderStyle.Sizable;
		MaximizeBox = true;
		MinimizeBox = false;

		// ===== ToolStrip (顶部工具栏) =====
		var toolStrip = new ToolStrip
		{
			Dock = DockStyle.Top,
			GripStyle = ToolStripGripStyle.Hidden,
			ImageScalingSize = new Size(16, 16)
		};

		var btnAdd = new ToolStripButton("添加行") { ToolTipText = "添加一行填充规则 (Ctrl+N)" };
		btnAdd.Click += BtnAddRow_Click;
		var btnDelete = new ToolStripButton("删除行") { ToolTipText = "删除选中的行 (Delete)" };
		btnDelete.Click += BtnDeleteRow_Click;
		var btnDuplicate = new ToolStripButton("复制行") { ToolTipText = "复制选中行 (Ctrl+D)" };
		btnDuplicate.Click += BtnDuplicateRow_Click;
		var sep1 = new ToolStripSeparator();
		var btnUp = new ToolStripButton("上移") { ToolTipText = "上移选中行 (Ctrl+↑)" };
		btnUp.Click += BtnMoveUp_Click;
		var btnDown = new ToolStripButton("下移") { ToolTipText = "下移选中行 (Ctrl+↓)" };
		btnDown.Click += BtnMoveDown_Click;
		var sep2 = new ToolStripSeparator();
		var btnPickFromTable = new ToolStripButton("从表格选择") { ToolTipText = "从当前表格选区填充目标位置" };
		btnPickFromTable.Click += BtnTargetPicker_Click;
		var sep3 = new ToolStripSeparator();
		var btnClear = new ToolStripButton("清空") { ToolTipText = "清空所有规则" };
		btnClear.Click += BtnClearAll_Click;
		var btnImport = new ToolStripButton("导入") { ToolTipText = "从 JSON 文件导入规则" };
		btnImport.Click += BtnImport_Click;
		var btnExport = new ToolStripButton("导出") { ToolTipText = "导出规则到 JSON 文件" };
		btnExport.Click += BtnExport_Click;
		var sep4 = new ToolStripSeparator();
		var btnHelp = new ToolStripButton("帮助") { ToolTipText = "查看占位符和用法说明" };
		btnHelp.Click += BtnHelp_Click;

		toolStrip.Items.AddRange(new ToolStripItem[]
		{
			btnAdd, btnDelete, btnDuplicate, sep1,
			btnUp, btnDown, sep2,
			btnPickFromTable, sep3,
			btnClear, btnImport, btnExport, sep4,
			btnHelp
		});

		// ===== Grid =====
		_grid = new C1FlexGrid
		{
			Dock = DockStyle.Fill,
			AllowEditing = true,
			AllowSorting = AllowSortingEnum.None,
			AutoResize = false,
			SelectionMode = SelectionModeEnum.Default
		};
		_grid.Cols.Count = ColCount;
		_grid.Cols[ColTable].Caption = "目标表格";
		_grid.Cols[ColTable].Width = 150;
		_grid.Cols[ColValue].Caption = "填充值";
		_grid.Cols[ColValue].Width = 160;
		_grid.Cols[ColTarget].Caption = "目标位置";
		_grid.Cols[ColTarget].Width = 120;
		_grid.Cols[ColTarget].ComboList = "...";
		_grid.Cols[ColMode].Caption = "填充模式";
		_grid.Cols[ColMode].Width = 80;
		_grid.Cols[ColMode].ComboList = "覆盖|追加";
		_grid.Cols[ColCondSource].Caption = "条件源位置";
		_grid.Cols[ColCondSource].Width = 110;
		_grid.Cols[ColCondSource].ComboList = "...";
		_grid.Cols[ColCondOp].Caption = "条件运算符";
		_grid.Cols[ColCondOp].Width = 90;
		_grid.Cols[ColCondOp].ComboList = OpList;
		_grid.Cols[ColCondValue].Caption = "条件值";
		_grid.Cols[ColCondValue].Width = 120;
		_grid.Cols.Fixed = 0;
		_grid.Rows.Fixed = 1;
		_grid.AllowResizing = AllowResizingEnum.Columns;
		_grid.AllowDragging = AllowDraggingEnum.None;
		_grid.KeyDown += Grid_KeyDown;
		_grid.CellChanged += Grid_CellChanged;
		_grid.CellButtonClick += Grid_CellButtonClick;

		// 列头 tooltip
		_grid.Cols[ColTable].UserData = "填充到哪个表格，留空=当前表格";
		_grid.Cols[ColValue].UserData = "填充的文本/数字，支持占位符: {date} {time} {table} {seq} {row}";
		_grid.Cols[ColTarget].UserData = "目标单元格，如 A1 或 A1:C3";
		_grid.Cols[ColMode].UserData = "覆盖=替换原有内容，追加=在末尾添加";
		_grid.Cols[ColCondSource].UserData = "条件判断的源单元格，如 A1";
		_grid.Cols[ColCondOp].UserData = "条件运算符";
		_grid.Cols[ColCondValue].UserData = "条件比较值（为空/非空时忽略）";

		// ===== ContextMenu =====
		_contextMenu = new ContextMenuStrip();
		_contextMenu.Items.Add("添加行", null, (s, e) => BtnAddRow_Click(s, e));
		_contextMenu.Items.Add("删除行", null, (s, e) => BtnDeleteRow_Click(s, e));
		_contextMenu.Items.Add("复制行", null, (s, e) => BtnDuplicateRow_Click(s, e));
		_contextMenu.Items.Add(new ToolStripSeparator());
		_contextMenu.Items.Add("上移", null, (s, e) => BtnMoveUp_Click(s, e));
		_contextMenu.Items.Add("下移", null, (s, e) => BtnMoveDown_Click(s, e));
		_contextMenu.Items.Add(new ToolStripSeparator());
		_contextMenu.Items.Add("从表格选择目标位置", null, (s, e) => BtnTargetPicker_Click(s, e));
		_contextMenu.Items.Add(new ToolStripSeparator());
		_contextMenu.Items.Add("清空所有", null, (s, e) => BtnClearAll_Click(s, e));
		_grid.ContextMenuStrip = _contextMenu;

		// ===== StatusStrip (底部状态栏) =====
		_statusStrip = new StatusStrip { Dock = DockStyle.Bottom };
		_lblRuleCount = new ToolStripStatusLabel { Text = "规则数: 0", Spring = false, Margin = new Padding(4, 0, 16, 0) };
		_lblValidation = new ToolStripStatusLabel { Text = "✓", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
		_lblCurrentTable = new ToolStripStatusLabel { Text = "当前表格: -", Spring = false, Margin = new Padding(16, 0, 4, 0) };
		_statusStrip.Items.AddRange(new ToolStripItem[] { _lblRuleCount, _lblValidation, _lblCurrentTable });

		// ===== Bottom buttons panel =====
		var bottomPanel = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.RightToLeft,
			Dock = DockStyle.Bottom,
			Height = 44,
			Padding = new Padding(6),
			AutoSize = false
		};

		var btnExecute = new Button { Text = "执行填充", Width = 110, Height = 30 };
		btnExecute.Click += BtnExecute_Click;

		var btnSave = new Button { Text = "保存配置", Width = 110, Height = 30 };
		btnSave.Click += BtnSave_Click;

		var btnCancel = new Button { Text = "关闭", Width = 80, Height = 30 };
		btnCancel.Click += (s, e) => Close();

		bottomPanel.Controls.AddRange(new Control[] { btnExecute, btnSave, btnCancel });

		// ===== Layout assembly =====
		var gridPanel = new Panel { Dock = DockStyle.Fill };
		gridPanel.Controls.Add(_grid);

		Controls.Add(gridPanel);
		Controls.Add(toolStrip);
		Controls.Add(_statusStrip);
		Controls.Add(bottomPanel);
	}

	private void OnLoad(object sender, EventArgs e)
	{
		PopulateTableList();
		LoadConfig();
		UpdateCurrentTableLabel();
		UpdateStatus();
		_hasUnsavedChanges = false;
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		// 如果在选区模式中，先退出
		if (_isSelectionMode)
		{
			ExitSelectionMode();
			e.Cancel = true;
			return;
		}

		if (!_hasUnsavedChanges) return;

		var result = Auditai.UI.Controls.MessageBox.Show(
			MessageBoxIcon.None,
			"配置已修改但未保存，是否保存？",
			MessageBoxButtons.YesNoCancel);

		if (result == DialogResult.Yes)
		{
			SaveConfigToProject();
		}
		else if (result == DialogResult.Cancel)
		{
			e.Cancel = true;
		}
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		// 选区模式下的快捷键
		if (_isSelectionMode)
		{
			if (e.KeyCode == Keys.Escape)
			{
				CancelSelectionMode();
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.Enter && _selModeOk != null && _selModeOk.Enabled)
			{
				ConfirmSelectionMode();
				e.Handled = true;
				return;
			}
			return; // 选区模式下不处理其他快捷键
		}

		// 键盘快捷键（非编辑状态下生效）
		if (_grid.Editor != null) return;

		switch (e.KeyCode)
		{
			case Keys.Delete:
				BtnDeleteRow_Click(sender, e);
				e.Handled = true;
				break;
			case Keys.Up when e.Control:
				BtnMoveUp_Click(sender, e);
				e.Handled = true;
				break;
			case Keys.Down when e.Control:
				BtnMoveDown_Click(sender, e);
				e.Handled = true;
				break;
			case Keys.D when e.Control:
				BtnDuplicateRow_Click(sender, e);
				e.Handled = true;
				break;
			case Keys.N when e.Control:
				BtnAddRow_Click(sender, e);
				e.Handled = true;
				break;
			case Keys.S when e.Control:
				BtnSave_Click(sender, e);
				e.Handled = true;
				break;
		}
	}

	private void Grid_KeyDown(object sender, KeyEventArgs e)
	{
		// Grid 内按键也处理（非编辑状态）
		if (_grid.Editor != null) return;

		if (e.KeyCode == Keys.Delete)
		{
			BtnDeleteRow_Click(sender, e);
			e.Handled = true;
		}
		else if (e.Control && e.KeyCode == Keys.Up)
		{
			BtnMoveUp_Click(sender, e);
			e.Handled = true;
		}
		else if (e.Control && e.KeyCode == Keys.Down)
		{
			BtnMoveDown_Click(sender, e);
			e.Handled = true;
		}
		else if (e.Control && e.KeyCode == Keys.D)
		{
			BtnDuplicateRow_Click(sender, e);
			e.Handled = true;
		}
	}

	private void Grid_CellChanged(object sender, RowColEventArgs e)
	{
		// 忽略 header row
		if (e.Row < 1) return;
		_hasUnsavedChanges = true;
		UpdateStatus();
	}

	/// <summary>单元格按钮点击：进入选区模式（类似 Excel 公式编辑器）</summary>
	private void Grid_CellButtonClick(object sender, RowColEventArgs e)
	{
		if (e.Row < 1) return;
		EnterSelectionMode(e.Row, e.Col);
	}

	/// <summary>进入选区模式：缩小窗口，监听表格选区变化</summary>
	private void EnterSelectionMode(int row, int col)
	{
		if (_isSelectionMode) return;

		_selectingRow = row;
		_selectingCol = col;
		_isSelectionMode = true;
		_selectedRangeText = "";

		// 保存当前窗口状态
		_savedWindowSize = Size;
		_savedWindowLocation = Location;

		// 隐藏主界面控件，显示选区模式栏
		foreach (Control c in Controls)
		{
			if (c is StatusStrip) continue; // 保留状态栏
			c.Visible = false;
		}

		// 创建选区模式面板
		var selPanel = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = SystemColors.Info,
			Padding = new Padding(8)
		};

		var lblTitle = new Label
		{
			Left = 8, Top = 8, Width = 300, Height = 20,
			Font = new Font(Font, FontStyle.Bold),
			Text = col == ColTarget ? "请选择目标位置..." : "请选择条件源位置..."
		};

		_selModeLabel = new Label
		{
			Left = 8, Top = 32, Width = 300, Height = 24,
			Font = new Font(Font, FontStyle.Bold),
			ForeColor = Color.Blue,
			Text = "当前选区: （请在表格中选择）"
		};

		var lblHint = new Label
		{
			Left = 8, Top = 60, Width = 300, Height = 20,
			ForeColor = Color.Gray,
			Text = "在表格中拖选单元格区域，然后点击确认"
		};

		_selModeOk = new Button
		{
			Left = 160, Top = 84, Width = 70, Height = 26,
			Text = "确认",
			Enabled = false
		};
		_selModeOk.Click += (s, e) => ConfirmSelectionMode();

		_selModeCancel = new Button
		{
			Left = 238, Top = 84, Width = 70, Height = 26,
			Text = "取消"
		};
		_selModeCancel.Click += (s, e) => CancelSelectionMode();

		selPanel.Controls.AddRange(new Control[] { lblTitle, _selModeLabel, lblHint, _selModeOk, _selModeCancel });
		selPanel.Name = "SelectionPanel";
		Controls.Add(selPanel);

		// 缩小窗口
		Size = new Size(320, 150);
		TopMost = true;
		// 保持窗口在屏幕上方
		if (Location.Y > 100)
			Location = new Point(Location.X, 50);

		// 订阅表格编辑器的选区变化事件
		try
		{
			var tableEditor = Program.MainForm?.TableEditor;
			if (tableEditor?._grid != null)
			{
				tableEditor._grid.AfterSelChange += TableEditor_SelectionChanged;
				// 立即读取当前选区
				UpdateSelectionFromTableEditor();
			}
		}
		catch (Exception ex) { ex.Log(); }
	}

	/// <summary>表格编辑器选区变化时更新显示</summary>
	private void TableEditor_SelectionChanged(object sender, RangeEventArgs e)
	{
		UpdateSelectionFromTableEditor();
	}

	/// <summary>从表格编辑器读取当前选区并更新显示</summary>
	private void UpdateSelectionFromTableEditor()
	{
		try
		{
			string range = GetSelectedRangeFromTableEditor();
			_selectedRangeText = range ?? "";
			if (_selModeLabel != null && !_selModeLabel.IsDisposed)
			{
				_selModeLabel.Text = string.IsNullOrEmpty(_selectedRangeText)
					? "当前选区: （请在表格中选择）"
					: $"当前选区: {_selectedRangeText}";
			}
			if (_selModeOk != null && !_selModeOk.IsDisposed)
			{
				_selModeOk.Enabled = !string.IsNullOrEmpty(_selectedRangeText);
			}
		}
		catch (Exception ex) { ex.Log(); }
	}

	/// <summary>确认选区：写入单元格并退出选区模式</summary>
	private void ConfirmSelectionMode()
	{
		if (string.IsNullOrEmpty(_selectedRangeText))
			return;

		if (_selectingRow >= 0 && _selectingRow < _grid.Rows.Count)
		{
			_grid[_selectingRow, _selectingCol] = _selectedRangeText;
		}
		ExitSelectionMode();
	}

	/// <summary>取消选区：退出选区模式不写入</summary>
	private void CancelSelectionMode()
	{
		ExitSelectionMode();
	}

	/// <summary>退出选区模式：恢复窗口和控件</summary>
	private void ExitSelectionMode()
	{
		// 取消订阅选区事件
		try
		{
			var tableEditor = Program.MainForm?.TableEditor;
			if (tableEditor?._grid != null)
			{
				tableEditor._grid.AfterSelChange -= TableEditor_SelectionChanged;
			}
		}
		catch { }

		// 保存要恢复选中的行
		int restoreRow = _selectingRow;

		// 移除选区面板
		var selPanel = Controls.Find("SelectionPanel", false);
		if (selPanel.Length > 0)
		{
			Controls.Remove(selPanel[0]);
			selPanel[0].Dispose();
		}

		// 恢复控件可见性
		foreach (Control c in Controls)
		{
			c.Visible = true;
		}

		// 恢复窗口状态
		Size = _savedWindowSize;
		Location = _savedWindowLocation;
		TopMost = false;
		_isSelectionMode = false;
		_selectingRow = -1;
		_selectingCol = -1;

		// 重新选中刚才编辑的行
		if (restoreRow >= 1 && restoreRow < _grid.Rows.Count)
		{
			_grid.Rows[restoreRow].Selected = true;
		}
	}

	/// <summary>从 Project.Current 收集所有表格，填充表格列下拉</summary>
	private void PopulateTableList()
	{
		_allTables.Clear();
		var project = Auditai.Model.Project.Current;
		if (project == null) return;

		try
		{
			foreach (var node in project.GetAllTableNodes())
			{
				string title = node.Name ?? node.Id.ToString();
				_allTables.Add((node.Id, title));
			}
		}
		catch (Exception ex) { ex.Log(); }

		// 按标题排序
		_allTables.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.Ordinal));

		// 下拉列表：第一项为"(当前表格)"，后面是所有表格
		var titles = new List<string> { CurrentTableOption };
		titles.AddRange(_allTables.Select(t => t.Title));
		_grid.Cols[ColTable].ComboList = string.Join("|", titles);
	}

	/// <summary>根据表格标题查找 ParaId（"(当前表格)"返回 null）</summary>
	private Id64? GetTableIdByTitle(string title)
	{
		if (string.IsNullOrEmpty(title) || title == CurrentTableOption)
			return null;

		foreach (var (id, t) in _allTables)
		{
			if (string.Equals(t, title, StringComparison.OrdinalIgnoreCase))
				return id;
		}
		return null;
	}

	/// <summary>根据表格 ParaId 查找标题</summary>
	private string GetTableTitleById(string tableIdStr)
	{
		if (string.IsNullOrEmpty(tableIdStr))
			return CurrentTableOption;

		try
		{
			var id = Id64.Parse(tableIdStr);
			foreach (var (tid, title) in _allTables)
			{
				if (tid == id)
					return title;
			}
		}
		catch { }
		return tableIdStr;
	}

	private void UpdateCurrentTableLabel()
	{
		try
		{
			var table = Program.MainForm?.CurrentTable;
			_lblCurrentTable.Text = table != null
				? $"当前表格: {table.TreeNode?.Name ?? table.Id.ToString()}"
				: "当前表格: -";
		}
		catch
		{
			_lblCurrentTable.Text = "当前表格: -";
		}
	}

	private void UpdateStatus()
	{
		int dataRows = Math.Max(0, _grid.Rows.Count - 1);
		_lblRuleCount.Text = $"规则数: {dataRows}";

		// 验证
		int invalidCount = 0;
		for (int i = 1; i < _grid.Rows.Count; i++)
		{
			string position = _grid[i, ColTarget]?.ToString() ?? "";
			string value = _grid[i, ColValue]?.ToString() ?? "";
			if (string.IsNullOrWhiteSpace(position) && string.IsNullOrWhiteSpace(value))
				continue; // 空行跳过
			if (!IsValidPosition(position))
				invalidCount++;
		}

		if (invalidCount > 0)
		{
			_lblValidation.Text = $"⚠ {invalidCount} 行位置格式无效";
			_lblValidation.ForeColor = Color.Red;
		}
		else
		{
			_lblValidation.Text = "✓ 配置有效";
			_lblValidation.ForeColor = Color.Green;
		}
	}

	/// <summary>验证目标位置格式（A1 或 A1:C3）</summary>
	private static bool IsValidPosition(string position)
	{
		if (string.IsNullOrWhiteSpace(position))
			return false;
		try
		{
			var parts = position.Split(':');
			return IsValidCellRef(parts[0]) && (parts.Length == 1 || IsValidCellRef(parts[1]));
		}
		catch
		{
			return false;
		}
	}

	private static bool IsValidCellRef(string cellRef)
	{
		return Regex.IsMatch(cellRef.Trim(), @"^[A-Za-z]+[1-9]\d*$");
	}

	private void LoadConfig()
	{
		var project = Auditai.Model.Project.Current;
		if (project?.Dal == null) return;

		try
		{
			var dto = project.Dal.GetProject();
			if (dto == null) return;

			var config = CustomFillConfig.Deserialize(dto.CustomFillConfig);
			if (config?.Rules == null || config.Rules.Count == 0)
				return;

			_grid.Rows.Count = 1 + config.Rules.Count;
			for (int i = 0; i < config.Rules.Count; i++)
			{
				var rule = config.Rules[i];
				int row = i + 1;
				_grid[row, ColTable] = GetTableTitleById(rule.TargetTableId);
				_grid[row, ColValue] = rule.Value;
				_grid[row, ColTarget] = rule.TargetPosition;
				_grid[row, ColMode] = rule.FillMode == CustomFillMode.Overwrite ? "覆盖" : "追加";

				if (rule.Condition != null && !string.IsNullOrWhiteSpace(rule.Condition.SourcePosition))
				{
					_grid[row, ColCondSource] = rule.Condition.SourcePosition;
					_grid[row, ColCondOp] = _opReverseMap.TryGetValue(rule.Condition.Operator, out var display)
						? display : "";
					if (rule.Condition.Operator != Auditai.Model.ConditionOperator.IsEmpty
						&& rule.Condition.Operator != Auditai.Model.ConditionOperator.IsNotEmpty)
					{
						_grid[row, ColCondValue] = rule.Condition.CompareValue;
					}
				}
			}
		}
		catch (Exception ex) { ex.Log(); }
	}

	private CustomFillConfig BuildConfigFromGrid()
	{
		var config = new CustomFillConfig();
		int dataRowCount = _grid.Rows.Count - 1;
		for (int i = 0; i < dataRowCount; i++)
		{
			int row = i + 1;
			string tableTitle = _grid[row, ColTable]?.ToString() ?? "";
			string value = _grid[row, ColValue]?.ToString() ?? "";
			string position = _grid[row, ColTarget]?.ToString() ?? "";
			string modeStr = _grid[row, ColMode]?.ToString() ?? "覆盖";
			var mode = modeStr.Equals("追加", StringComparison.OrdinalIgnoreCase)
				? CustomFillMode.Append
				: CustomFillMode.Overwrite;

			if (string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(position))
				continue;

			string tableId = null;
			var tid = GetTableIdByTitle(tableTitle);
			if (tid.HasValue)
				tableId = tid.Value.ToString();

			var rule = new CustomFillRule
			{
				TargetTableId = tableId,
				Value = value,
				TargetPosition = position,
				FillMode = mode
			};

			string condSource = _grid[row, ColCondSource]?.ToString() ?? "";
			string condOpDisplay = _grid[row, ColCondOp]?.ToString() ?? "";
			if (!string.IsNullOrWhiteSpace(condSource) && !string.IsNullOrWhiteSpace(condOpDisplay)
				&& _opMap.TryGetValue(condOpDisplay, out var condOp))
			{
				string condValue = (condOp == Auditai.Model.ConditionOperator.IsEmpty || condOp == Auditai.Model.ConditionOperator.IsNotEmpty)
					? null
					: (_grid[row, ColCondValue]?.ToString() ?? "");

				rule.Condition = new CustomFillCondition
				{
					SourcePosition = condSource,
					Operator = condOp,
					CompareValue = condValue
				};
			}

			config.Rules.Add(rule);
		}
		return config;
	}

	private void SaveConfigToProject()
	{
		var project = Auditai.Model.Project.Current;
		if (project?.Dal == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "无法保存：项目未加载");
			return;
		}

		var config = BuildConfigFromGrid();
		var json = config.Serialize();

		var dto = project.Dal.GetProject();
		if (dto == null) return;
		dto.CustomFillConfig = json;
		project.Dal.SaveProject(dto);
		_hasUnsavedChanges = false;
	}

	// ===== 按钮事件 =====

	private void BtnAddRow_Click(object sender, EventArgs e)
	{
		int newRowIndex = _grid.Rows.Count;
		_grid.Rows.Add();
		_grid[newRowIndex, ColTable] = CurrentTableOption;
		_grid[newRowIndex, ColMode] = "覆盖";
		_grid.Rows[newRowIndex].Selected = true;
		_hasUnsavedChanges = true;
		UpdateStatus();
	}

	private void BtnDeleteRow_Click(object sender, EventArgs e)
	{
		if (_grid.Rows.Count <= 1)
			return;

		var sel = _grid.Selection;
		int rowFrom = Math.Max(1, sel.TopRow);
		int rowTo = Math.Max(rowFrom, sel.BottomRow);

		if (rowFrom > _grid.Rows.Count - 1)
			return;

		for (int r = rowTo; r >= rowFrom; r--)
		{
			if (r > 0 && r < _grid.Rows.Count)
				_grid.Rows.Remove(r);
		}
		_hasUnsavedChanges = true;
		UpdateStatus();
	}

	private void BtnDuplicateRow_Click(object sender, EventArgs e)
	{
		var sel = _grid.Selection;
		int row = sel.TopRow;
		if (row < 1 || row >= _grid.Rows.Count)
			return;

		_grid.Rows.Insert(row + 1);
		for (int c = 0; c < ColCount; c++)
		{
			_grid[row + 1, c] = _grid[row, c];
		}
		_grid.Rows[row + 1].Selected = true;
		_hasUnsavedChanges = true;
		UpdateStatus();
	}

	private void BtnMoveUp_Click(object sender, EventArgs e)
	{
		var sel = _grid.Selection;
		int row = sel.TopRow;
		if (row <= 1)
			return;

		SwapRows(row, row - 1);
		_grid.Rows[row - 1].Selected = true;
		_hasUnsavedChanges = true;
	}

	private void BtnMoveDown_Click(object sender, EventArgs e)
	{
		var sel = _grid.Selection;
		int row = sel.TopRow;
		if (row <= 0 || row >= _grid.Rows.Count - 1)
			return;

		SwapRows(row, row + 1);
		_grid.Rows[row + 1].Selected = true;
		_hasUnsavedChanges = true;
	}

	private void BtnClearAll_Click(object sender, EventArgs e)
	{
		if (_grid.Rows.Count <= 1)
			return;

		var result = Auditai.UI.Controls.MessageBox.Show(
			MessageBoxIcon.None,
			"确定要清空所有规则吗？",
			MessageBoxButtons.YesNo);

		if (result != DialogResult.Yes)
			return;

		_grid.Rows.Count = 1;
		_hasUnsavedChanges = true;
		UpdateStatus();
	}

	private void BtnImport_Click(object sender, EventArgs e)
	{
		using var ofd = new OpenFileDialog
		{
			Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
			Title = "导入填充规则"
		};

		if (ofd.ShowDialog() != DialogResult.OK)
			return;

		try
		{
			var json = File.ReadAllText(ofd.FileName);
			var config = CustomFillConfig.Deserialize(json);
			if (config?.Rules == null || config.Rules.Count == 0)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件中没有有效的填充规则");
				return;
			}

			_grid.Rows.Count = 1 + config.Rules.Count;
			for (int i = 0; i < config.Rules.Count; i++)
			{
				var rule = config.Rules[i];
				int row = i + 1;
				_grid[row, ColTable] = GetTableTitleById(rule.TargetTableId);
				_grid[row, ColValue] = rule.Value;
				_grid[row, ColTarget] = rule.TargetPosition;
				_grid[row, ColMode] = rule.FillMode == CustomFillMode.Overwrite ? "覆盖" : "追加";

				if (rule.Condition != null && !string.IsNullOrWhiteSpace(rule.Condition.SourcePosition))
				{
					_grid[row, ColCondSource] = rule.Condition.SourcePosition;
					_grid[row, ColCondOp] = _opReverseMap.TryGetValue(rule.Condition.Operator, out var display) ? display : "";
					if (rule.Condition.Operator != Auditai.Model.ConditionOperator.IsEmpty
						&& rule.Condition.Operator != Auditai.Model.ConditionOperator.IsNotEmpty)
					{
						_grid[row, ColCondValue] = rule.Condition.CompareValue;
					}
				}
			}
			_hasUnsavedChanges = true;
			UpdateStatus();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"已导入 {config.Rules.Count} 条规则");
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导入失败: " + ex.Message);
		}
	}

	private void BtnExport_Click(object sender, EventArgs e)
	{
		var config = BuildConfigFromGrid();
		if (config.Rules.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "没有规则可导出");
			return;
		}

		using var sfd = new SaveFileDialog
		{
			Filter = "JSON 文件 (*.json)|*.json",
			Title = "导出填充规则",
			FileName = "自定义填充规则.json"
		};

		if (sfd.ShowDialog() != DialogResult.OK)
			return;

		try
		{
			File.WriteAllText(sfd.FileName, config.Serialize());
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"已导出 {config.Rules.Count} 条规则");
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出失败: " + ex.Message);
		}
	}

	private void BtnHelp_Click(object sender, EventArgs e)
	{
		var help = @"【自定义填充 - 使用说明】

■ 位置选择（类似 Excel 公式编辑器）：
  点击目标位置或条件源位置列的 ... 按钮
  窗口自动缩小，在表格中拖选单元格区域
  选区实时显示在缩小窗口中，点击确认完成选择

■ 填充值支持以下占位符（不区分大小写）：
  {date}     → 当前日期 (yyyy-MM-dd)
  {time}     → 当前时间 (HH:mm:ss)
  {datetime} → 当前日期时间
  {table}    → 当前表格名称
  {seq}      → 区域内序号 (从1开始)
  {row}      → 行号
  {col}      → 列字母

■ 目标位置格式：
  A1       → 单个单元格
  A1:C3    → 矩形区域

■ 条件列（可选）：
  设置条件后，仅当条件满足时才执行该行填充
  条件源位置：如 A1（判断该单元格的值）
  为空/非空运算符无需填写条件值

■ 快捷键：
  Ctrl+N     添加行
  Delete     删除选中行
  Ctrl+D     复制选中行
  Ctrl+↑/↓   上移/下移
  Ctrl+S     保存配置

■ 示例：
  填充值: 审计日期: {date}   位置: A1  模式: 覆盖
  填充值: 序号{seq}          位置: A1:A10  模式: 覆盖
  填充值: 已复核             位置: B1:B10  条件: A1 不等于 空";
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, help);
	}

	private void SwapRows(int rowA, int rowB)
	{
		for (int c = 0; c < _grid.Cols.Count; c++)
		{
			object temp = _grid[rowA, c];
			_grid[rowA, c] = _grid[rowB, c];
			_grid[rowB, c] = temp;
		}
	}

	private void BtnTargetPicker_Click(object sender, EventArgs e)
	{
		var sel = _grid.Selection;
		int row = sel.TopRow;
		if (row <= 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中一个数据行");
			return;
		}

		int targetCol = (sel.LeftCol == ColCondSource) ? ColCondSource : ColTarget;
		EnterSelectionMode(row, targetCol);
	}

	private static string GetSelectedRangeFromTableEditor()
	{
		try
		{
			var tableEditor = Program.MainForm?.TableEditor;
			if (tableEditor?._grid == null)
				return null;

			var grid = tableEditor._grid;
			var gridSel = grid.Selection;
			if (!gridSel.IsValid)
				return null;

			int fixedRows = grid.Rows.Fixed;
			int fixedCols = grid.Cols.Fixed;

			int topRow = gridSel.TopRow - fixedRows;
			int leftCol = gridSel.LeftCol - fixedCols;
			int bottomRow = gridSel.BottomRow - fixedRows;
			int rightCol = gridSel.RightCol - fixedCols;

			if (topRow < 0 || leftCol < 0 || bottomRow < 0 || rightCol < 0)
				return null;

			string start = CellIndexToA1(topRow, leftCol);
			string end = CellIndexToA1(bottomRow, rightCol);

			return (start == end) ? start : $"{start}:{end}";
		}
		catch (Exception ex)
		{
			ex.Log();
			return null;
		}
	}

	private static string CellIndexToA1(int row, int col)
	{
		string colLetters = "";
		int c = col + 1;
		while (c > 0)
		{
			c--;
			colLetters = (char)('A' + (c % 26)) + colLetters;
			c /= 26;
		}
		return $"{colLetters}{row + 1}";
	}

	private void BtnSave_Click(object sender, EventArgs e)
	{
		SaveConfigToProject();
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "填充配置已保存");
	}

	private async void BtnExecute_Click(object sender, EventArgs e)
	{
		var config = BuildConfigFromGrid();
		if (config.Rules.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先配置填充规则");
			return;
		}

		// 执行前确认
		var result = Auditai.UI.Controls.MessageBox.Show(
			MessageBoxIcon.None,
			$"即将执行 {config.Rules.Count} 条填充规则。\n执行后可通过 Ctrl+Z 撤销。\n\n是否继续？",
			MessageBoxButtons.YesNo);

		if (result != DialogResult.Yes)
			return;

		SaveConfigToProject();
		await Program.MainForm.ExecuteCustomFill();
	}
}
