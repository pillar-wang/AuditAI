using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 批量应用表格样式对话框
/// 提供"选择范围→选择样式→确认应用"三步流程
/// </summary>
public class frmBatchApplyRange : Form
{
	private readonly DocumentStructure _docStructure;
	private readonly DocumentEditor _docEditor;

	// UI 控件
	private Label _lblStep;
	private Label _lblInstruction;
	private Button _btnSelectRange;
	private Button _btnConfigStyle;
	private Button _btnApply;
	private Button _btnCancel;
	private TextBox _txtRangeInfo;
	private TextBox _txtStyleInfo;
	private ProgressBar _progressBar;
	private Label _lblProgress;

	// 状态
	private int _startPos = -1;
	private int _endPos = -1;
	private TableBorderStyle _selectedStyle;
	private int _tableCountInRange;

	public frmBatchApplyRange(DocumentStructure docStructure, DocumentEditor docEditor)
	{
		_docStructure = docStructure;
		_docEditor = docEditor;
		_selectedStyle = TableBorderStyles.CreateCustom();
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		this.Text = "批量应用表格样式";
		this.Size = new Size(500, 450);
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
			Size = new Size(460, 25),
			Font = new Font("微软雅黑", 12, FontStyle.Bold)
		};
		y += 35;

		// 步骤1: 选择范围
		var grpRange = new GroupBox
		{
			Text = "步骤1: 选择应用范围",
			Location = new Point(15, y),
			Size = new Size(460, 100)
		};

		_lblInstruction = new Label
		{
			Text = "点击下方按钮，然后在文档结构树中点击起始和结束位置",
			Location = new Point(10, 20),
			Size = new Size(440, 30),
			ForeColor = Color.Gray
		};
		grpRange.Controls.Add(_lblInstruction);

		_btnSelectRange = new Button
		{
			Text = "选择范围",
			Location = new Point(10, 55),
			Size = new Size(100, 30)
		};
		_btnSelectRange.Click += BtnSelectRange_Click;
		grpRange.Controls.Add(_btnSelectRange);

		_txtRangeInfo = new TextBox
		{
			Location = new Point(120, 58),
			Size = new Size(330, 25),
			ReadOnly = true
		};
		SetPlaceholder(_txtRangeInfo, "未选择范围");
		grpRange.Controls.Add(_txtRangeInfo);

		y += 110;

		// 步骤2: 选择样式
		var grpStyle = new GroupBox
		{
			Text = "步骤2: 配置表格样式",
			Location = new Point(15, y),
			Size = new Size(460, 80)
		};

		_btnConfigStyle = new Button
		{
			Text = "配置样式",
			Location = new Point(10, 30),
			Size = new Size(100, 30)
		};
		_btnConfigStyle.Click += BtnConfigStyle_Click;
		grpStyle.Controls.Add(_btnConfigStyle);

		_txtStyleInfo = new TextBox
		{
			Location = new Point(120, 33),
			Size = new Size(330, 25),
			ReadOnly = true
		};
		SetPlaceholder(_txtStyleInfo, "使用默认自定义样式");
		grpStyle.Controls.Add(_txtStyleInfo);

		y += 90;

		// 步骤3: 应用
		var grpApply = new GroupBox
		{
			Text = "步骤3: 应用",
			Location = new Point(15, y),
			Size = new Size(460, 80)
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
			Location = new Point(120, 35),
			Size = new Size(330, 25),
			ForeColor = Color.Gray
		};
		grpApply.Controls.Add(_lblProgress);

		_progressBar = new ProgressBar
		{
			Location = new Point(120, 55),
			Size = new Size(330, 20),
			Visible = false
		};
		grpApply.Controls.Add(_progressBar);

		y += 90;

		// 取消按钮
		_btnCancel = new Button
		{
			Text = "关闭",
			Location = new Point(375, y),
			Size = new Size(100, 30),
			DialogResult = DialogResult.Cancel
		};

		this.Controls.AddRange(new Control[] { _lblStep, grpRange, grpStyle, grpApply, _btnCancel });

		// 订阅范围选择事件
		if (_docStructure != null)
		{
			_docStructure.RangeSelected += OnRangeSelected;
		}

		this.FormClosing += (s, e) =>
		{
			if (_docStructure != null)
			{
				_docStructure.RangeSelected -= OnRangeSelected;
				_docStructure.ExitRangeSelectionMode();
			}
		};
	}

	/// <summary>
	/// 为只读 TextBox 设置占位提示文本（兼容 .NET Framework 4.6.2，无 PlaceHolderText 支持）
	/// </summary>
	private static void SetPlaceholder(TextBox txt, string placeholder)
	{
		txt.Text = placeholder;
		txt.ForeColor = Color.Gray;
	}

	/// <summary>
	/// 步骤1: 选择范围按钮点击
	/// </summary>
	private void BtnSelectRange_Click(object sender, EventArgs e)
	{
		if (_docStructure == null) return;
		_startPos = -1;
		_endPos = -1;
		_txtRangeInfo.Text = "请在文档结构树中点击起始位置...";
		_txtRangeInfo.ForeColor = Color.Gray;
		_btnApply.Enabled = false;
		_docStructure.EnterRangeSelectionMode();
	}

	/// <summary>
	/// 范围选择完成回调
	/// </summary>
	private void OnRangeSelected(int startPos, int endPos)
	{
		_startPos = startPos;
		_endPos = endPos;

		// 统计范围内表格数量
		var tables = _docStructure.GetTablesInRange(startPos, endPos);
		_tableCountInRange = tables.Count;

		_txtRangeInfo.Text = $"已选择范围: 位置 {startPos} - {endPos}，范围内有 {_tableCountInRange} 个表格";
		_txtRangeInfo.ForeColor = Color.Black;

		if (_tableCountInRange == 0)
		{
			MessageBox.Show("所选范围内未找到表格", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		UpdateApplyButton();
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

		// 调用 DocumentEditor 的批量应用方法（带进度回调）
		int successCount = _docEditor.BatchApplyTableStyle(tables, _selectedStyle, (current, total) =>
		{
			_progressBar.Value = current;
			_lblProgress.Text = $"正在应用样式: {current}/{total}";
			Application.DoEvents(); // 刷新 UI
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
