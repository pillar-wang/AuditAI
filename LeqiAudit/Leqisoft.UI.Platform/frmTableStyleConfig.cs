﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 自定义表格边框样式配置对话框。
/// 允许用户可视化地配置表格 10 条边框的线型与磅数，
/// 并提供预览、预设、模板保存/加载功能。
/// </summary>
public class frmTableStyleConfig : Form
{
	// ===== 控件字段 =====
	private ComboBox _cboHeaderTop, _cboHeaderBottom, _cboHeaderLeft, _cboHeaderRight;
	private ComboBox _cboTableTop, _cboTableBottom, _cboTableLeft, _cboTableRight;
	private ComboBox _cboInnerH, _cboInnerV;
	private NumericUpDown _numHeaderTop, _numHeaderBottom, _numHeaderLeft, _numHeaderRight;
	private NumericUpDown _numTableTop, _numTableBottom, _numTableLeft, _numTableRight;
	private NumericUpDown _numInnerH, _numInnerV;
	private CheckBox _chkKeywordBoldUnderline;
	private TextBox _txtKeywordList;
	private Label _lblKeywordList;
	private Panel _previewPanel;

	// ===== 线型显示名称（与 LineStyle 枚举顺序一致） =====
	private static readonly string[] LineStyleNames = { "无线", "细线", "粗线", "短划线", "点线", "点划线", "双点划线" };

	/// <summary>磅数转像素系数（1pt ≈ 1.33px）</summary>
	private const float PtToPx = 1.33f;

	public frmTableStyleConfig()
	{
		InitializeComponents();
		LoadFromStyle(TableBorderStyles.CreateCustom());
	}

	private void InitializeComponents()
	{
		// 窗体属性
		this.Text = "自定义表格样式配置";
		this.Size = new Size(720, 570);
		this.StartPosition = FormStartPosition.CenterParent;
		this.FormBorderStyle = FormBorderStyle.FixedDialog;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.BackColor = Color.White;

		// ===== 表头边框 GroupBox =====
		var grpHeader = new GroupBox
		{
			Text = "表头边框",
			Location = new Point(10, 10),
			Size = new Size(350, 140)
		};
		_cboHeaderTop = CreateBorderRow(grpHeader, 0, "表头上部:", out _numHeaderTop);
		_cboHeaderBottom = CreateBorderRow(grpHeader, 1, "表头下部:", out _numHeaderBottom);
		_cboHeaderLeft = CreateBorderRow(grpHeader, 2, "表头左侧:", out _numHeaderLeft);
		_cboHeaderRight = CreateBorderRow(grpHeader, 3, "表头右侧:", out _numHeaderRight);
		this.Controls.Add(grpHeader);

		// ===== 表格边框 GroupBox =====
		var grpTable = new GroupBox
		{
			Text = "表格边框",
			Location = new Point(10, 155),
			Size = new Size(350, 140)
		};
		_cboTableTop = CreateBorderRow(grpTable, 0, "表格顶部:", out _numTableTop);
		_cboTableBottom = CreateBorderRow(grpTable, 1, "表格底部:", out _numTableBottom);
		_cboTableLeft = CreateBorderRow(grpTable, 2, "表格左侧:", out _numTableLeft);
		_cboTableRight = CreateBorderRow(grpTable, 3, "表格右侧:", out _numTableRight);
		this.Controls.Add(grpTable);

		// ===== 内部边框 GroupBox =====
		var grpInner = new GroupBox
		{
			Text = "内部边框",
			Location = new Point(10, 300),
			Size = new Size(350, 90)
		};
		_cboInnerH = CreateBorderRow(grpInner, 0, "内部横线:", out _numInnerH);
		_cboInnerV = CreateBorderRow(grpInner, 1, "内部竖线:", out _numInnerV);
		this.Controls.Add(grpInner);

		// ===== 关键词行 CheckBox + 自定义关键词输入 =====
		_chkKeywordBoldUnderline = new CheckBox
		{
			Text = "关键词行加粗下划线",
			Location = new Point(15, 398),
			Size = new Size(160, 24)
		};
		_chkKeywordBoldUnderline.CheckedChanged += (s, e) => RefreshPreview();
		this.Controls.Add(_chkKeywordBoldUnderline);

		_lblKeywordList = new Label
		{
			Text = "关键词:",
			Location = new Point(15, 428),
			Size = new Size(55, 20),
			TextAlign = ContentAlignment.MiddleRight
		};
		this.Controls.Add(_lblKeywordList);

		_txtKeywordList = new TextBox
		{
			Location = new Point(75, 426),
			Size = new Size(275, 24),
			Text = "合计,小计,总计,关键词"
		};
		_txtKeywordList.TextChanged += (s, e) => RefreshPreview();
		this.Controls.Add(_txtKeywordList);

		// ===== 预览面板 =====
		_previewPanel = new Panel
		{
			Location = new Point(370, 10),
			Size = new Size(330, 400),
			BackColor = Color.White,
			BorderStyle = BorderStyle.FixedSingle
		};
		_previewPanel.Paint += PreviewPanel_Paint;
		this.Controls.Add(_previewPanel);

		// ===== 预设按钮 =====
		var btnGrid = new Button { Text = "普通", Location = new Point(10, 460), Size = new Size(60, 28) };
		btnGrid.Click += (s, e) => LoadFromStyle(PresetToCustom(TableBorderStyles.Grid));

		var btnStyle1 = new Button { Text = "样式1", Location = new Point(75, 460), Size = new Size(60, 28) };
		btnStyle1.Click += (s, e) => LoadFromStyle(PresetToCustom(TableBorderStyles.ThickUpDownDashBody));

		var btnStyle2 = new Button { Text = "样式2", Location = new Point(140, 460), Size = new Size(60, 28) };
		btnStyle2.Click += (s, e) => LoadFromStyle(PresetToCustom(TableBorderStyles.ThickUpDownThinBody));

		var btnStyle3 = new Button { Text = "样式3", Location = new Point(205, 460), Size = new Size(60, 28) };
		btnStyle3.Click += (s, e) => LoadFromStyle(PresetToCustom(TableBorderStyles.ThickBorderThinBody));

		var btnNoLine = new Button { Text = "无线", Location = new Point(270, 460), Size = new Size(60, 28) };
		btnNoLine.Click += (s, e) => LoadFromStyle(PresetToCustom(TableBorderStyles.NoLine));

		this.Controls.Add(btnGrid);
		this.Controls.Add(btnStyle1);
		this.Controls.Add(btnStyle2);
		this.Controls.Add(btnStyle3);
		this.Controls.Add(btnNoLine);

		// ===== 模板按钮 =====
		var btnSaveTemplate = new Button { Text = "保存模板", Location = new Point(10, 495), Size = new Size(80, 28) };
		btnSaveTemplate.Click += (s, e) => SaveTemplate();

		var btnLoadTemplate = new Button { Text = "加载模板", Location = new Point(95, 495), Size = new Size(80, 28) };
		btnLoadTemplate.Click += (s, e) => LoadTemplate();

		this.Controls.Add(btnSaveTemplate);
		this.Controls.Add(btnLoadTemplate);

		// ===== 确定/取消按钮 =====
		var btnOK = new Button
		{
			Text = "确定",
			Location = new Point(525, 495),
			Size = new Size(80, 28),
			DialogResult = DialogResult.OK
		};
		var btnCancel = new Button
		{
			Text = "取消",
			Location = new Point(615, 495),
			Size = new Size(80, 28),
			DialogResult = DialogResult.Cancel
		};
		this.Controls.Add(btnOK);
		this.Controls.Add(btnCancel);
		this.AcceptButton = btnOK;
		this.CancelButton = btnCancel;
	}

	/// <summary>
	/// 在 GroupBox 中创建一行边框配置控件（Label + ComboBox + NumericUpDown + 单位标签）。
	/// </summary>
	/// <param name="parent">所属 GroupBox</param>
	/// <param name="rowIndex">行索引（0 开始）</param>
	/// <param name="labelText">标签文本</param>
	/// <param name="numeric">输出的 NumericUpDown 控件</param>
	/// <returns>创建的 ComboBox 控件</returns>
	private ComboBox CreateBorderRow(GroupBox parent, int rowIndex, string labelText, out NumericUpDown numeric)
	{
		int y = 22 + rowIndex * 26;

		var lbl = new Label
		{
			Text = labelText,
			Location = new Point(10, y + 3),
			Size = new Size(75, 20),
			TextAlign = ContentAlignment.MiddleLeft
		};
		parent.Controls.Add(lbl);

		var cbo = new ComboBox
		{
			Location = new Point(90, y),
			Size = new Size(95, 23),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		cbo.Items.AddRange(LineStyleNames);
		cbo.SelectedIndex = 0;
		cbo.SelectedIndexChanged += (s, e) => RefreshPreview();
		parent.Controls.Add(cbo);

		numeric = new NumericUpDown
		{
			Location = new Point(190, y),
			Size = new Size(55, 23),
			Minimum = 0.25m,
			Maximum = 6m,
			Increment = 0.25m,
			DecimalPlaces = 2,
			Value = 0.5m
		};
		numeric.ValueChanged += (s, e) => RefreshPreview();
		parent.Controls.Add(numeric);

		var lblUnit = new Label
		{
			Text = "磅",
			Location = new Point(250, y + 3),
			Size = new Size(25, 20),
			TextAlign = ContentAlignment.MiddleLeft
		};
		parent.Controls.Add(lblUnit);

		return cbo;
	}

	/// <summary>
	/// 从 TableBorderStyle 加载配置到各控件。
	/// </summary>
	public void LoadFromStyle(TableBorderStyle style)
	{
		if (style == null)
		{
			style = TableBorderStyles.CreateCustom();
		}

		SetEdgeControls(_cboHeaderTop, _numHeaderTop, style.HeaderTop);
		SetEdgeControls(_cboHeaderBottom, _numHeaderBottom, style.HeaderBottom);
		SetEdgeControls(_cboHeaderLeft, _numHeaderLeft, style.HeaderLeft);
		SetEdgeControls(_cboHeaderRight, _numHeaderRight, style.HeaderRight);
		SetEdgeControls(_cboTableTop, _numTableTop, style.TableTop);
		SetEdgeControls(_cboTableBottom, _numTableBottom, style.TableBottom);
		SetEdgeControls(_cboTableLeft, _numTableLeft, style.TableLeft);
		SetEdgeControls(_cboTableRight, _numTableRight, style.TableRight);
		SetEdgeControls(_cboInnerH, _numInnerH, style.InnerHorizontal);
		SetEdgeControls(_cboInnerV, _numInnerV, style.InnerVertical);

		_chkKeywordBoldUnderline.Checked = style.KeywordRowBoldUnderline;
		_txtKeywordList.Text = string.IsNullOrEmpty(style.KeywordList) ? "合计,小计,总计,关键词" : style.KeywordList;

		RefreshPreview();
	}

	/// <summary>获取用户配置的表格边框样式</summary>
	public TableBorderStyle GetConfiguredStyle()
	{
		var style = TableBorderStyles.CreateCustom();
		style.HeaderTop = GetEdgeFromControls(_cboHeaderTop, _numHeaderTop);
		style.HeaderBottom = GetEdgeFromControls(_cboHeaderBottom, _numHeaderBottom);
		style.HeaderLeft = GetEdgeFromControls(_cboHeaderLeft, _numHeaderLeft);
		style.HeaderRight = GetEdgeFromControls(_cboHeaderRight, _numHeaderRight);
		style.TableTop = GetEdgeFromControls(_cboTableTop, _numTableTop);
		style.TableBottom = GetEdgeFromControls(_cboTableBottom, _numTableBottom);
		style.TableLeft = GetEdgeFromControls(_cboTableLeft, _numTableLeft);
		style.TableRight = GetEdgeFromControls(_cboTableRight, _numTableRight);
		style.InnerHorizontal = GetEdgeFromControls(_cboInnerH, _numInnerH);
		style.InnerVertical = GetEdgeFromControls(_cboInnerV, _numInnerV);
		style.KeywordRowBoldUnderline = _chkKeywordBoldUnderline.Checked;
		style.KeywordList = string.IsNullOrEmpty(_txtKeywordList.Text) ? "合计,小计,总计,关键词" : _txtKeywordList.Text;
		return style;
	}

	private void SetEdgeControls(ComboBox cbo, NumericUpDown num, BorderEdgeStyle edge)
	{
		if (edge == null)
		{
			cbo.SelectedIndex = 0;
			num.Value = 0.5m;
			return;
		}
		int idx = (int)edge.LineType;
		if (idx < 0 || idx >= LineStyleNames.Length) idx = 0;
		cbo.SelectedIndex = idx;
		decimal v = (decimal)edge.Weight;
		if (v < num.Minimum) v = num.Minimum;
		if (v > num.Maximum) v = num.Maximum;
		num.Value = v;
	}

	private BorderEdgeStyle GetEdgeFromControls(ComboBox cbo, NumericUpDown num)
	{
		int idx = cbo.SelectedIndex;
		if (idx < 0 || idx >= LineStyleNames.Length) idx = 0;
		return new BorderEdgeStyle((LineStyle)idx, (float)num.Value);
	}

	/// <summary>
	/// 将旧版预设样式（使用 UpDownLine/LeftRightLine/SecondLine/BodyLine）
	/// 转换为自定义样式（使用 10 个 BorderEdgeStyle 属性）。
	/// </summary>
	private TableBorderStyle PresetToCustom(TableBorderStyle preset)
	{
		var custom = TableBorderStyles.CreateCustom();

		LineStyle upDown = preset.UpDownLine;
		LineStyle leftRight = preset.LeftRightLine;
		LineStyle second = preset.SecondLine;
		LineStyle body = preset.BodyLine;

		// Thick 使用较粗磅数，其余细线使用 0.75pt，None 使用 0.5pt（不绘制）
		Func<LineStyle, BorderEdgeStyle> toEdge = ls =>
		{
			float w = ls == LineStyle.Thick ? 2.0f : (ls == LineStyle.None ? 0.5f : 0.75f);
			return new BorderEdgeStyle(ls, w);
		};

		custom.HeaderTop = toEdge(upDown);
		custom.HeaderBottom = toEdge(second);
		custom.HeaderLeft = toEdge(leftRight);
		custom.HeaderRight = toEdge(leftRight);
		custom.TableTop = toEdge(upDown);
		custom.TableBottom = toEdge(upDown);
		custom.TableLeft = toEdge(leftRight);
		custom.TableRight = toEdge(leftRight);
		custom.InnerHorizontal = toEdge(body);
		custom.InnerVertical = new BorderEdgeStyle(LineStyle.None, 0.5f);

		return custom;
	}

	private void RefreshPreview()
	{
		_previewPanel?.Invalidate();
	}

	// ===== 预览绘制 =====

	private void PreviewPanel_Paint(object sender, PaintEventArgs e)
	{
		var g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;

		// 表格尺寸
		const int cols = 3;
		const int rows = 4;
		float tableX = 25f;
		float tableY = 40f;
		float colWidth = 95f;
		float rowHeight = 50f;
		float tableWidth = colWidth * cols;
		float tableHeight = rowHeight * rows;
		float tableRight = tableX + tableWidth;
		float tableBottom = tableY + tableHeight;

		// 绘制标题
		using (var titleFont = new Font("宋体", 10, FontStyle.Bold))
		{
			g.DrawString("表格样式预览", titleFont, Brushes.DarkGray, tableX, 12f);
		}

		// 1. 绘制表头行背景（灰色）
		g.FillRectangle(new SolidBrush(Color.FromArgb(225, 225, 225)), tableX, tableY, tableWidth, rowHeight);

		// 2. 绘制单元格文字
		var headerFont = new Font("宋体", 9, FontStyle.Bold);
		var bodyFont = new Font("宋体", 9);
		var keywordFont = new Font("宋体", 9,
			_chkKeywordBoldUnderline.Checked ? FontStyle.Bold | FontStyle.Underline : FontStyle.Regular);

		string[] headers = { "列1", "列2", "列3" };
		string[,] bodyCells = {
			{ "关键词行", "数据", "数据" },
			{ "数据", "数据", "数据" },
			{ "数据", "数据", "数据" }
		};

		using (var sf = new StringFormat
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		})
		{
			// 表头文字
			for (int c = 0; c < cols; c++)
			{
				g.DrawString(headers[c], headerFont, Brushes.Black,
					new RectangleF(tableX + c * colWidth, tableY, colWidth, rowHeight), sf);
			}

			// 正文文字
			for (int r = 0; r < rows - 1; r++)
			{
				for (int c = 0; c < cols; c++)
				{
					var font = (r == 0) ? keywordFont : bodyFont;
					g.DrawString(bodyCells[r, c], font, Brushes.Black,
						new RectangleF(tableX + c * colWidth, tableY + (r + 1) * rowHeight, colWidth, rowHeight), sf);
				}
			}
		}

		// 3. 绘制内部竖线（全高度）
		var penInnerV = CreatePen(GetEdgeFromControls(_cboInnerV, _numInnerV), Color.Black);
		for (int c = 1; c < cols; c++)
		{
			float x = tableX + c * colWidth;
			DrawLineSafe(g, penInnerV, x, tableY, x, tableBottom);
		}

		// 4. 绘制内部横线（仅正文区域，不含表头分隔线）
		var penInnerH = CreatePen(GetEdgeFromControls(_cboInnerH, _numInnerH), Color.Black);
		for (int r = 2; r < rows; r++) // r=2 开始：表头(0) + 关键词行(1) 之后
		{
			float y = tableY + r * rowHeight;
			DrawLineSafe(g, penInnerH, tableX, y, tableRight, y);
		}

		// 5. 绘制表格外边框
		var penTableTop = CreatePen(GetEdgeFromControls(_cboTableTop, _numTableTop), Color.Black);
		var penTableBottom = CreatePen(GetEdgeFromControls(_cboTableBottom, _numTableBottom), Color.Black);
		var penTableLeft = CreatePen(GetEdgeFromControls(_cboTableLeft, _numTableLeft), Color.Black);
		var penTableRight = CreatePen(GetEdgeFromControls(_cboTableRight, _numTableRight), Color.Black);

		DrawLineSafe(g, penTableTop, tableX, tableY, tableRight, tableY);
		DrawLineSafe(g, penTableBottom, tableX, tableBottom, tableRight, tableBottom);
		DrawLineSafe(g, penTableLeft, tableX, tableY, tableX, tableBottom);
		DrawLineSafe(g, penTableRight, tableRight, tableY, tableRight, tableBottom);

		// 6. 绘制表头边框（覆盖在表格边框之上）
		var penHeaderTop = CreatePen(GetEdgeFromControls(_cboHeaderTop, _numHeaderTop), Color.Black);
		var penHeaderBottom = CreatePen(GetEdgeFromControls(_cboHeaderBottom, _numHeaderBottom), Color.Black);
		var penHeaderLeft = CreatePen(GetEdgeFromControls(_cboHeaderLeft, _numHeaderLeft), Color.Black);
		var penHeaderRight = CreatePen(GetEdgeFromControls(_cboHeaderRight, _numHeaderRight), Color.Black);

		DrawLineSafe(g, penHeaderTop, tableX, tableY, tableRight, tableY);
		DrawLineSafe(g, penHeaderBottom, tableX, tableY + rowHeight, tableRight, tableY + rowHeight);
		DrawLineSafe(g, penHeaderLeft, tableX, tableY, tableX, tableY + rowHeight);
		DrawLineSafe(g, penHeaderRight, tableRight, tableY, tableRight, tableY + rowHeight);

		// 7. 释放资源
		penInnerV.Dispose();
		penInnerH.Dispose();
		penTableTop.Dispose();
		penTableBottom.Dispose();
		penTableLeft.Dispose();
		penTableRight.Dispose();
		penHeaderTop.Dispose();
		penHeaderBottom.Dispose();
		penHeaderLeft.Dispose();
		penHeaderRight.Dispose();
		headerFont.Dispose();
		bodyFont.Dispose();
		keywordFont.Dispose();
	}

	/// <summary>根据 BorderEdgeStyle 创建 Pen（None 线型使用透明色，不绘制）</summary>
	private Pen CreatePen(BorderEdgeStyle edge, Color color)
	{
		if (edge == null || edge.LineType == LineStyle.None)
		{
			return new Pen(Color.Transparent, 0.5f);
		}
		return CreatePen(edge.LineType, edge.Weight, color);
	}

	/// <summary>根据线型创建 Pen</summary>
	private Pen CreatePen(LineStyle lineType, float weight, Color color)
	{
		float widthInPx = weight * PtToPx;
		if (widthInPx < 0.5f) widthInPx = 0.5f;
		var pen = new Pen(color, widthInPx);
		switch (lineType)
		{
			case LineStyle.None:
				pen.Color = Color.Transparent;
				break;
			case LineStyle.Dash:
				pen.DashStyle = DashStyle.Dash;
				break;
			case LineStyle.Dotted:
				pen.DashStyle = DashStyle.Dot;
				break;
			case LineStyle.DotDash:
				pen.DashStyle = DashStyle.DashDot;
				break;
			case LineStyle.DoubleDotDash:
				pen.DashStyle = DashStyle.DashDotDot;
				break;
		}
		return pen;
	}

	/// <summary>安全绘制线段（None 线型透明色不绘制）</summary>
	private void DrawLineSafe(Graphics g, Pen pen, float x1, float y1, float x2, float y2)
	{
		if (pen.Color == Color.Transparent) return;
		g.DrawLine(pen, x1, y1, x2, y2);
	}

	// ===== 模板保存/加载 =====

	/// <summary>获取模板目录路径（%APPDATA%\LeqiAudit\TableStyleTemplates\）</summary>
	private string GetTemplateDirectory()
	{
		string dir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"LeqiAudit",
			"TableStyleTemplates");
		Directory.CreateDirectory(dir);
		return dir;
	}

	private void SaveTemplate()
	{
		try
		{
			string dir = GetTemplateDirectory();
			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = "保存表格样式模板";
				dlg.Filter = "表格样式模板 (*.json)|*.json";
				dlg.InitialDirectory = dir;
				dlg.DefaultExt = ".json";
				dlg.FileName = "表格样式_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					var style = GetConfiguredStyle();
					File.WriteAllText(dlg.FileName, style.ToJson());
					MessageBox.Show("模板保存成功！\n" + dlg.FileName, "提示",
						MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("保存模板失败：" + ex.Message, "错误",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void LoadTemplate()
	{
		try
		{
			string dir = GetTemplateDirectory();
			using (var dlg = new OpenFileDialog())
			{
				dlg.Title = "加载表格样式模板";
				dlg.Filter = "表格样式模板 (*.json)|*.json";
				dlg.InitialDirectory = dir;
				if (dlg.ShowDialog() == DialogResult.OK)
				{
					string json = File.ReadAllText(dlg.FileName);
					var style = TableBorderStyle.FromJson(json);
					if (style != null)
					{
						LoadFromStyle(style);
						MessageBox.Show("模板加载成功！", "提示",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						MessageBox.Show("模板文件格式无效。", "错误",
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("加载模板失败：" + ex.Message, "错误",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
