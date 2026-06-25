using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using Auditai.Model;

namespace Auditai.UI.Platform;

/// <summary>
/// 批量应用表格样式对话框
/// 提供"选择范围→选择样式→确认应用"三步流程
/// 显示文档结构树，通过"设为起始"/"设为结束"两个控件点击树节点选择范围
/// </summary>
public class frmBatchApplyRange : Form
{
	private readonly DocumentStructure _docStructure;
	private readonly DocumentEditor _docEditor;

	// UI 控件
	private Label _lblStep;
	private Button _btnConfigStyle;
	private Button _btnApply;
	private Button _btnCancel;
	private TextBox _txtRangeInfo;
	private TextBox _txtStyleInfo;
	private ProgressBar _progressBar;
	private Label _lblProgress;

	// 范围选择控件（两个）
	private RadioButton _rbSetStart;
	private RadioButton _rbSetEnd;

	// 树形结构图
	private C1FlexGrid _treeGrid;
	private Button _btnExpandAll;
	private Button _btnCollapseAll;

	// 状态
	private int _startPos = -1;
	private int _endPos = -1;
	private object _rangeStartKey;
	private object _rangeEndKey;
	private TableBorderStyle _selectedStyle;
	private int _tableCountInRange;

	public frmBatchApplyRange(DocumentStructure docStructure, DocumentEditor docEditor)
	{
		_docStructure = docStructure;
		_docEditor = docEditor;
		_selectedStyle = TableBorderStyles.CreateCustom();
		InitializeComponents();
		PopulateTree();
	}

	private void InitializeComponents()
	{
		this.Text = "批量应用表格样式";
		this.Size = new Size(620, 680);
		this.StartPosition = FormStartPosition.CenterParent;
		this.FormBorderStyle = FormBorderStyle.FixedDialog;
		this.MaximizeBox = false;
		this.MinimizeBox = false;

		int y = 15;

		// 步骤标题
		_lblStep = new Label
		{
			Text = "批量应用表格样式向导",
			Location = new Point(15, y),
			Size = new Size(580, 25),
			Font = new Font("微软雅黑", 12, FontStyle.Bold)
		};
		y += 35;

		// 步骤1: 选择范围——树形结构 + 两个控件设置起止
		var grpRange = new GroupBox
		{
			Text = "步骤1: 选择应用范围（在树中点击节点设置起始和结束位置）",
			Location = new Point(15, y),
			Size = new Size(580, 420)
		};

		// 两个 RangeButton 控件：设为起始 / 设为结束（互斥）
		_rbSetStart = new RadioButton
		{
			Text = "设为起始",
			Location = new Point(10, 20),
			Size = new Size(110, 22),
			Checked = true,
			BackColor = Color.FromArgb(230, 240, 255),
			FlatStyle = FlatStyle.Standard,
			Font = new Font("微软雅黑", 9, FontStyle.Bold)
		};
		grpRange.Controls.Add(_rbSetStart);

		_rbSetEnd = new RadioButton
		{
			Text = "设为结束",
			Location = new Point(130, 20),
			Size = new Size(110, 22),
			BackColor = Color.FromArgb(255, 235, 235),
			FlatStyle = FlatStyle.Standard,
			Font = new Font("微软雅黑", 9, FontStyle.Bold)
		};
		grpRange.Controls.Add(_rbSetEnd);

		// 展开/收缩按钮
		_btnExpandAll = new Button
		{
			Text = "全部展开",
			Location = new Point(260, 18),
			Size = new Size(80, 25)
		};
		_btnExpandAll.Click += (s, e) => _treeGrid.Tree.Show(_treeGrid.Tree.MaximumLevel);
		grpRange.Controls.Add(_btnExpandAll);

		_btnCollapseAll = new Button
		{
			Text = "全部收缩",
			Location = new Point(345, 18),
			Size = new Size(80, 25)
		};
		_btnCollapseAll.Click += (s, e) => _treeGrid.Tree.Show(0);
		grpRange.Controls.Add(_btnCollapseAll);

		// 树形结构图
		_treeGrid = new C1FlexGrid
		{
			Location = new Point(10, 50),
			Size = new Size(555, 310),
			Rows = { Fixed = 0, DefaultSize = 26 },
			Cols = { Fixed = 0, Count = 1 },
			ExtendLastCol = true,
			AllowEditing = false,
			SelectionMode = SelectionModeEnum.Row,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle,
			FocusRect = FocusRectEnum.None,
			Tree = { Column = 0, Style = TreeStyleFlags.Symbols }
		};
		_treeGrid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		_treeGrid.MouseClick += TreeGrid_MouseClick;
		_treeGrid.OwnerDrawCell += TreeGrid_OwnerDrawCell;
		grpRange.Controls.Add(_treeGrid);

		// 范围信息
		_txtRangeInfo = new TextBox
		{
			Location = new Point(10, 370),
			Size = new Size(555, 25),
			ReadOnly = true
		};
		_txtRangeInfo.Text = "提示：先点击「设为起始」或「设为结束」，再点击树中节点";
		_txtRangeInfo.ForeColor = Color.Gray;
		grpRange.Controls.Add(_txtRangeInfo);

		y += 430;

		// 步骤2: 选择样式
		var grpStyle = new GroupBox
		{
			Text = "步骤2: 配置表格样式",
			Location = new Point(15, y),
			Size = new Size(580, 70)
		};

		_btnConfigStyle = new Button
		{
			Text = "配置样式",
			Location = new Point(10, 25),
			Size = new Size(100, 30)
		};
		_btnConfigStyle.Click += BtnConfigStyle_Click;
		grpStyle.Controls.Add(_btnConfigStyle);

		_txtStyleInfo = new TextBox
		{
			Location = new Point(120, 28),
			Size = new Size(440, 25),
			ReadOnly = true
		};
		_txtStyleInfo.Text = "使用默认自定义样式";
		_txtStyleInfo.ForeColor = Color.Gray;
		grpStyle.Controls.Add(_txtStyleInfo);

		y += 80;

		// 步骤3: 应用
		var grpApply = new GroupBox
		{
			Text = "步骤3: 应用",
			Location = new Point(15, y),
			Size = new Size(580, 80)
		};

		_btnApply = new Button
		{
			Text = "批量应用",
			Location = new Point(10, 30),
			Size = new Size(100, 30),
			Enabled = false
		};
		_btnApply.Click += BtnApply_Click;
		grpApply.Controls.Add(_btnApply);

		_lblProgress = new Label
		{
			Text = "",
			Location = new Point(120, 20),
			Size = new Size(440, 20),
			ForeColor = Color.Gray
		};
		grpApply.Controls.Add(_lblProgress);

		_progressBar = new ProgressBar
		{
			Location = new Point(120, 45),
			Size = new Size(440, 20),
			Visible = false
		};
		grpApply.Controls.Add(_progressBar);

		y += 90;

		// 取消按钮
		_btnCancel = new Button
		{
			Text = "关闭",
			Location = new Point(495, y),
			Size = new Size(100, 30),
			DialogResult = DialogResult.Cancel
		};

		this.Controls.AddRange(new Control[] { _lblStep, grpRange, grpStyle, grpApply, _btnCancel });
	}

	/// <summary>
	/// 填充树形结构图
	/// </summary>
	private void PopulateTree()
	{
		if (_docStructure == null) return;

		var nodeData = _docStructure.GetTreeNodeData();
		if (nodeData.Count == 0)
		{
			_txtRangeInfo.Text = "文档结构图为空，请先生成文档结构图";
			_txtRangeInfo.ForeColor = Color.Red;
			return;
		}

		_treeGrid.BeginUpdate();
		try
		{
			_treeGrid.Rows.Count = 0;

			var nodeStack = new Stack<(int Depth, Node Node)>();

			foreach (var (depth, text, key) in nodeData)
			{
				while (nodeStack.Count > 0 && nodeStack.Peek().Depth >= depth)
					nodeStack.Pop();

				Node newNode;
				if (nodeStack.Count == 0)
				{
					newNode = _treeGrid.Rows.AddNode(0);
					newNode.Data = text;
					newNode.Key = key;
				}
				else
				{
					newNode = nodeStack.Peek().Node.AddNode(NodeTypeEnum.LastChild, text, key, null);
				}
				_treeGrid.Rows[newNode.Row.Index].UserData = key;
				nodeStack.Push((depth, newNode));
			}

			_treeGrid.Tree.Show(1);
		}
		finally
		{
			_treeGrid.EndUpdate();
		}
	}

	/// <summary>
	/// 树节点点击——根据选中状态设为起始或结束
	/// </summary>
	private void TreeGrid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left) return;

		var hitTest = _treeGrid.HitTest(e.Location);
		if (hitTest.Row < 0 || hitTest.Row >= _treeGrid.Rows.Count)
			return;

		var row = _treeGrid.Rows[hitTest.Row];
		var node = row.Node;
		if (node == null || node.Key == null) return;

		if (_rbSetStart.Checked)
		{
			// 设为起始
			_rangeStartKey = node.Key;
			_startPos = _docStructure.GetPositionFromKey(_rangeStartKey);

			// 更新按钮文本显示当前选择
			_rbSetStart.Text = $"起始：{node.Data}";
		}
		else
		{
			// 设为结束
			_rangeEndKey = node.Key;
			_endPos = _docStructure.GetPositionFromKey(_rangeEndKey);

			_rbSetEnd.Text = $"结束：{node.Data}";
		}

		// 重新计算范围信息
		UpdateRangeInfo();

		// 让两个按钮可以再次点击重新选择
		_treeGrid.Invalidate();
	}

	/// <summary>
	/// 计算并显示当前范围信息
	/// </summary>
	private void UpdateRangeInfo()
	{
		bool startSelected = _startPos >= 0 && _rangeStartKey != null;
		bool endSelected = _endPos >= 0 && _rangeEndKey != null;

		if (!startSelected && !endSelected)
		{
			_txtRangeInfo.Text = "提示：先点击「设为起始」或「设为结束」，再点击树中节点";
			_txtRangeInfo.ForeColor = Color.Gray;
			_tableCountInRange = 0;
			UpdateApplyButton();
			return;
		}

		if (!startSelected)
		{
			_txtRangeInfo.Text = "已选择结束位置，请选择起始位置";
			_txtRangeInfo.ForeColor = Color.Gray;
			_tableCountInRange = 0;
			UpdateApplyButton();
			return;
		}

		if (!endSelected)
		{
			_txtRangeInfo.Text = "已选择起始位置，请选择结束位置";
			_txtRangeInfo.ForeColor = Color.Gray;
			_tableCountInRange = 0;
			UpdateApplyButton();
			return;
		}

		// 两者都已选择——自动交换顺序
		int rawStartPos = _docStructure.GetPositionFromKey(_rangeStartKey);
		int rawEndPos = _docStructure.GetPositionFromKey(_rangeEndKey);
		if (rawStartPos > rawEndPos)
		{
			_startPos = rawEndPos;
			_endPos = rawStartPos;
		}
		else
		{
			_startPos = rawStartPos;
			_endPos = rawEndPos;
		}

		var tables = _docStructure.GetTablesInRange(_startPos, _endPos);
		_tableCountInRange = tables.Count;

		_txtRangeInfo.Text = $"已选择范围: 位置 {_startPos} - {_endPos}，范围内有 {_tableCountInRange} 个表格";
		_txtRangeInfo.ForeColor = _tableCountInRange > 0 ? Color.Black : Color.Red;

		if (_tableCountInRange == 0)
		{
			MessageBox.Show("所选范围内未找到表格", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		UpdateApplyButton();
	}

	/// <summary>
	/// 在树节点上绘制"起"/"止"标记
	/// </summary>
	private void TreeGrid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row < 0 || e.Row >= _treeGrid.Rows.Count) return;

		var row = _treeGrid.Rows[e.Row];
		var node = row.Node;
		if (node == null || node.Key == null) return;

		bool isStart = node.Key.Equals(_rangeStartKey);
		bool isEnd = node.Key.Equals(_rangeEndKey);

		if (isStart || isEnd)
		{
			e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(120, 51, 153, 255)), e.Bounds);
			string label = isStart ? "起" : "止";
			using (var brush = new SolidBrush(Color.White))
			using (var font = new Font("微软雅黑", 9, FontStyle.Bold))
			{
				var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
				var labelBounds = new Rectangle(e.Bounds.Right - 30, e.Bounds.Top, 30, e.Bounds.Height);
				e.Graphics.DrawString(label, font, brush, labelBounds, sf);
			}
		}
	}

	/// <summary>
	/// 步骤2: 配置样式按钮点击
	/// </summary>
	private void BtnConfigStyle_Click(object sender, EventArgs e)
	{
		using (var dlg = new frmTableStyleConfig())
		{
			dlg.LoadFromStyle(_selectedStyle);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				_selectedStyle = dlg.GetConfiguredStyle();
				_txtStyleInfo.Text = "已配置自定义样式";
				_txtStyleInfo.ForeColor = Color.Black;
				UpdateApplyButton();
			}
		}
	}

	/// <summary>
	/// 步骤3: 批量应用按钮点击
	/// </summary>
	private void BtnApply_Click(object sender, EventArgs e)
	{
		if (_startPos < 0 || _endPos < 0 || _selectedStyle == null)
			return;

		var tables = _docStructure.GetTablesInRange(_startPos, _endPos);
		if (tables.Count == 0)
		{
			MessageBox.Show("所选范围内未找到表格", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		_progressBar.Visible = true;
		_progressBar.Maximum = tables.Count;
		_progressBar.Value = 0;
		_btnApply.Enabled = false;
		_lblProgress.Text = "正在应用样式: 0/" + tables.Count;

		int successCount = _docEditor.BatchApplyTableStyle(tables, _selectedStyle, (current, total) =>
		{
			_progressBar.Value = current;
			_lblProgress.Text = $"正在应用样式: {current}/{total}";
			Application.DoEvents();
		});

		_progressBar.Value = tables.Count;
		_lblProgress.Text = $"完成: 成功应用 {successCount}/{tables.Count} 个表格";
		_btnApply.Enabled = true;

		MessageBox.Show($"批量应用完成！\n成功: {successCount}/{tables.Count}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private void UpdateApplyButton()
	{
		_btnApply.Enabled = _startPos >= 0 && _endPos >= 0 && _tableCountInRange > 0 && _selectedStyle != null;
	}
}
