﻿﻿﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Auditai.DTO;

namespace Auditai.UI.Controls;

internal class InputBoxImpl : C1RibbonForm
{
	private InputFormEnum _inputFormEnum;

	public Func<string, bool> ValidCallback;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1TextBoxEx txtInputLeft;

	private C1Button btnConfirm;

	private C1Button btnCancel;

	private C1Label lblPrompt;

	private C1Label lblwarnNum;

	private C1DateEdit dateInputLeft;

	private C1TextBoxEx txtInputRight;

	private C1DateEdit dateInputRight;

	public object Value { get; set; }

	public bool Valid { get; set; }

	/// <summary>控件顶部距离 lblPrompt 底部的垂直间距</summary>
	private const int ControlSpacing = 10;

	/// <summary>左右边距</summary>
	private const int SideMargin = 26;

	/// <summary>按钮宽度（增大以提升点击舒适度）</summary>
	private const int ButtonWidth = 110;

	/// <summary>按钮高度（36→40 对齐统一按钮规格）</summary>
	private const int ButtonHeight = 40;

	/// <summary>按钮圆角半径（统一规范 8px）</summary>
	private const int ButtonRadius = 8;

	/// <summary>按钮右侧距窗体右边缘的距离（避免按钮贴边）</summary>
	private const int ButtonRightMargin = 50;

	/// <summary>按钮之间的水平间距</summary>
	private const int ButtonGap = 12;

	/// <summary>窗体 ClientSize 宽度</summary>
	private const int FormClientWidth = 600;

	/// <summary>确定按钮 Left 坐标（取消按钮在右，确定按钮在其左）</summary>
	private static readonly int BtnConfirmLeft = FormClientWidth - ButtonRightMargin - ButtonWidth - ButtonGap - ButtonWidth;

	/// <summary>取消按钮 Left 坐标（最右侧）</summary>
	private static readonly int BtnCancelLeft = FormClientWidth - ButtonRightMargin - ButtonWidth;

	#region === 设计令牌（同 MessageShowBox，支持 Google Blue 主题） ===

	private Color Primary;
	private Color PrimaryDark;
	private Color PrimaryLight;
	private Color LineColorDefault;
	private Color TextPrimary;

	/// <summary>根据当前主题加载颜色令牌：Google 蓝主题走 Google Blue 色板，其余走默认小清新浅蓝</summary>
	private void LoadThemeColors()
	{
		var theme = Theme.SelectedAuditaiTheme;
		if (theme != null && theme.Name == "auditai_GoogleBlue")
		{
			// Google Blue 色板
			Primary = Color.FromArgb(26, 115, 232);
			PrimaryDark = Color.FromArgb(21, 87, 176);
			PrimaryLight = Color.FromArgb(23, 101, 204);
			LineColorDefault = Color.FromArgb(226, 232, 240);
			TextPrimary = Color.FromArgb(15, 23, 42);
		}
		else
		{
			// 默认：小清新浅蓝
			Primary = Color.FromArgb(74, 144, 217);
			PrimaryDark = Color.FromArgb(53, 123, 189);
			PrimaryLight = Color.FromArgb(90, 160, 230);
			LineColorDefault = Color.FromArgb(208, 215, 222);
			TextPrimary = Color.FromArgb(30, 41, 59);
		}
	}

	#endregion

	/// <summary>
	/// 统一按钮样式：确定=主按钮（蓝底白字无边框），取消=次按钮（白底深灰字+浅灰边框），均 Flat + 8px 圆角 Region
	/// </summary>
	private void ApplyButtonStyle(C1Button btn, bool isPrimary)
	{
		btn.FlatStyle = FlatStyle.Flat;
		if (isPrimary)
		{
			// 主按钮：Primary 蓝填充 + 白字（悬停/按下深浅变体）
			btn.BackColor = Primary;
			btn.ForeColor = Color.White;
			btn.FlatAppearance.BorderSize = 0;
			btn.FlatAppearance.MouseDownBackColor = PrimaryDark;
			btn.FlatAppearance.MouseOverBackColor = PrimaryLight;
		}
		else
		{
			// 次按钮：白底深灰字 + 浅灰边框，视觉低调
			btn.BackColor = Color.White;
			btn.ForeColor = TextPrimary;
			btn.FlatAppearance.BorderColor = LineColorDefault;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(240, 247, 252);
			btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 249, 252);
		}
		// 8px 圆角 Region（四角 AddArc，同 MessageShowBox；只裁剪圆角，不影响按钮 Top 动态变化）
		using (var path = new GraphicsPath())
		{
			int r = ButtonRadius * 2;
			path.AddArc(0, 0, r, r, 180, 90);
			path.AddArc(btn.Width - r, 0, r, r, 270, 90);
			path.AddArc(btn.Width - r, btn.Height - r, r, r, 0, 90);
			path.AddArc(0, btn.Height - r, r, r, 90, 90);
			path.CloseFigure();
			btn.Region = new Region(path);
		}
	}

	/// <summary>统一应用两个按钮样式（构造末尾调用；主题应用后可在 ShowDialog 再次调用防覆盖）</summary>
	private void RefreshButtonStyles()
	{
		ApplyButtonStyle(btnConfirm, isPrimary: true);
		ApplyButtonStyle(btnCancel, isPrimary: false);
	}

	public InputBoxImpl()
	{
		// 先加载主题颜色令牌（须在 InitializeComponent 之前）
		LoadThemeColors();
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
		// 统一按钮样式 + 8px 圆角（构造最终位置应用）
		RefreshButtonStyles();
	}

	public InputBoxImpl(string title = "", string prompt = "", InputFormEnum inputEnum = InputFormEnum.Num)
	{
		// 先加载主题颜色令牌（须在 InitializeComponent 之前）
		LoadThemeColors();
		InitializeComponent();
		Text = title;
		lblPrompt.Text = prompt;
		_inputFormEnum = inputEnum;

		// 根据 prompt 内容自动调整 lblPrompt 高度和下方控件位置
		LayoutControlsByPrompt(prompt);

		switch (_inputFormEnum)
		{
		case InputFormEnum.Date:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = false;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月dd日";
			dateInputRight.CustomFormat = "yyyy年MM月dd日";
			break;
		case InputFormEnum.Num:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			break;
		case InputFormEnum.NumRange:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = true;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			txtInputLeft.Width = 273;
			txtInputRight.Width = 273;
			break;
		case InputFormEnum.DateRange:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = true;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月dd日";
			dateInputRight.CustomFormat = "yyyy年MM月dd日";
			dateInputLeft.Width = 273;
			dateInputRight.Width = 273;
			break;
		case InputFormEnum.Text:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			break;
		case InputFormEnum.MultiText:
			txtInputLeft.Visible = true;
			txtInputLeft.Multiline = true;
			txtInputLeft.AcceptsReturn = true;
			txtInputLeft.Width = 530;
			txtInputLeft.Height = 110;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			// 多重文本模式下，调整下方输入框和按钮位置
			int mtInputTop = lblPrompt.Bottom + ControlSpacing;
			txtInputLeft.Location = new Point(SideMargin, mtInputTop);
			int btnTop = txtInputLeft.Bottom + ControlSpacing + 10;
			base.ClientSize = new Size(FormClientWidth, btnTop + btnConfirm.Height + 20);
			btnConfirm.Location = new Point(BtnConfirmLeft, btnTop);
			btnCancel.Location = new Point(BtnCancelLeft, btnTop);
			lblwarnNum.Location = new Point(SideMargin, txtInputLeft.Bottom + 4);
			break;
		case InputFormEnum.Time:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			txtInputLeft.DataType = typeof(DateTime);
			txtInputLeft.FormatType = FormatTypeEnum.LongTime;
			txtInputLeft.Value = DateTime.Now;
			break;
		case InputFormEnum.TimeRange:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = true;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			txtInputLeft.DataType = typeof(DateTime);
			txtInputLeft.FormatType = FormatTypeEnum.LongTime;
			txtInputLeft.Value = DateTime.Now;
			txtInputRight.DataType = typeof(DateTime);
			txtInputRight.FormatType = FormatTypeEnum.LongTime;
			txtInputRight.Value = DateTime.Now;
			txtInputLeft.Width = 273;
			txtInputRight.Width = 273;
			break;
		case InputFormEnum.DateYearMonth:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = false;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月";
			dateInputRight.CustomFormat = "yyyy年MM月";
			break;
		case InputFormEnum.DateYearMonthRange:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = true;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月";
			dateInputRight.CustomFormat = "yyyy年MM月";
			dateInputLeft.Width = 273;
			dateInputRight.Width = 273;
			break;
		}
		// 统一按钮样式 + 8px 圆角（构造最终位置应用）
		RefreshButtonStyles();
	}

	/// <summary>
	/// 根据 prompt 长度自动计算 lblPrompt 的高度，
	/// 并调整下方输入控件、警告标签和按钮的垂直位置，
	/// 确保长提示文字不被截断。
	/// </summary>
	private void LayoutControlsByPrompt(string prompt)
	{
		if (string.IsNullOrEmpty(prompt)) return;
		if (_inputFormEnum == InputFormEnum.MultiText) return; // MultiText 单独处理

		Font font = lblPrompt.Font;
		int maxWidth = 550; // lblPrompt 设计宽度
		int minHeight = 28;

		SizeF size;
		using (var g = CreateGraphics())
		{
			StringFormat fmt = (StringFormat)StringFormat.GenericTypographic.Clone();
			fmt.FormatFlags |= StringFormatFlags.LineLimit;
			size = g.MeasureString(prompt, font, maxWidth, fmt);
		}

		// 高度上限 120 与 ApplyPromptAutoScrollOrTextBox 的滚动阈值一致：
		// 超长提示由滚动文本框承载，避免窗体高度无上限导致底部按钮超出屏幕
		int lblHeight = Math.Min(Math.Max(minHeight, (int)Math.Ceiling(size.Height) + 4), 120);
		lblPrompt.Height = lblHeight;

		// 下方控件的 Top = lblPrompt.Bottom + 间距
		int inputTop = lblPrompt.Bottom + ControlSpacing;

		// 调整输入控件 Location
		txtInputLeft.Location = new Point(txtInputLeft.Left, inputTop);
		txtInputRight.Location = new Point(txtInputRight.Left, inputTop);
		dateInputLeft.Location = new Point(dateInputLeft.Left, inputTop);
		dateInputRight.Location = new Point(dateInputRight.Left, inputTop);

		// 警告标签位置
		lblwarnNum.Location = new Point(lblwarnNum.Left, txtInputLeft.Bottom + 4);

		// 按钮位置（Bottom 对齐）
		int btnTop = inputTop + Math.Max(txtInputLeft.Height, dateInputLeft.Height) + ControlSpacing + 8;
		btnConfirm.Location = new Point(btnConfirm.Left, btnTop);
		btnCancel.Location = new Point(btnCancel.Left, btnTop);

		// 调整窗体 ClientSize
		int totalHeight = btnTop + btnConfirm.Height + 20;
		int minFormHeight = 175;
		if (totalHeight < minFormHeight) totalHeight = minFormHeight;
		base.ClientSize = new Size(FormClientWidth, totalHeight);
	}

	public void SetInputLeftWidth(int width = 390)
	{
		txtInputLeft.Width = width;
	}

	public void SetInputTextValue(string value)
	{
		txtInputLeft.TextDetached = true;
		txtInputLeft.Value = value;
		txtInputLeft.Text = value;
	}

	public void SetInputTextForPassword()
	{
		txtInputLeft.PasswordChar = '*';
	}

	public void SetInputDateValue(DateTime value)
	{
		dateInputLeft.Value = value;
	}

	public void SetInputDateYearMonthValue(DateYearMonth value)
	{
		dateInputLeft.Value = value.Date;
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		if (ValidCallback != null && !ValidCallback(txtInputLeft.Text.Trim()))
		{
			return;
		}
		switch (_inputFormEnum)
		{
		case InputFormEnum.Num:
		{
			if (decimal.TryParse(txtInputLeft.Text.Trim(), out var result6))
			{
				Value = result6;
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.Text:
		case InputFormEnum.MultiText:
			Value = txtInputLeft.Text;
			Valid = true;
			break;
		case InputFormEnum.Date:
		{
			if (DateTime.TryParse(dateInputLeft.Value?.ToString(), out var result7))
			{
				Value = result7;
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.DateRange:
		{
			object value3 = dateInputLeft.Value;
			object value4 = dateInputRight.Value;
			if (DateTime.TryParse(value3.ToString(), out var result3) && DateTime.TryParse(value4.ToString(), out var result4))
			{
				Value = Tuple.Create(result3, result4);
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.NumRange:
		{
			if (decimal.TryParse(txtInputLeft.Text.Trim(), out var result8) && decimal.TryParse(txtInputRight.Text.Trim(), out var result9))
			{
				Value = Tuple.Create(result8, result9);
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.Time:
			Value = txtInputLeft.Value;
			Valid = true;
			break;
		case InputFormEnum.TimeRange:
			Value = Tuple.Create(txtInputLeft.Value, txtInputRight.Value);
			Valid = true;
			break;
		case InputFormEnum.DateYearMonth:
		{
			if (DateTime.TryParse(dateInputLeft.Value?.ToString(), out var result5))
			{
				Value = result5;
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.DateYearMonthRange:
		{
			object value = dateInputLeft.Value;
			object value2 = dateInputRight.Value;
			if (DateTime.TryParse(value.ToString(), out var result) && DateTime.TryParse(value2.ToString(), out var result2))
			{
				Value = Tuple.Create(result, result2);
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		}
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void txtInputLeft_KeyPress(object sender, KeyPressEventArgs e)
	{
	}

	private void txtInputRight_KeyPress(object sender, KeyPressEventArgs e)
	{
	}

	private void dateInputLeft_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			dateInputLeft.Value = dateInputLeft.Text;
		}
	}

	private void dateInputRight_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			dateInputRight.Value = dateInputRight.Text;
		}
	}

	private void txtInputLeft_TextChanged(object sender, EventArgs e)
	{
		if (_inputFormEnum == InputFormEnum.Num || _inputFormEnum == InputFormEnum.NumRange)
		{
			if (decimal.TryParse(txtInputLeft.Text.Trim(), out var _))
			{
				lblwarnNum.Visible = false;
				return;
			}
			lblwarnNum.Visible = true;
			lblwarnNum.Text = "请输入有效数值类型";
		}
	}

	private void txtInputRight_TextChanged(object sender, EventArgs e)
	{
		if (_inputFormEnum == InputFormEnum.Num || _inputFormEnum == InputFormEnum.NumRange)
		{
			if (decimal.TryParse(txtInputRight.Text.Trim(), out var _))
			{
				lblwarnNum.Visible = false;
				return;
			}
			lblwarnNum.Visible = true;
			lblwarnNum.Text = "请输入有效数值类型";
		}
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentObject(this);
		Theme.SetCurrentObject(btnConfirm);
		Theme.SetCurrentObject(btnCancel);
		// 主题应用后重设按钮样式，防止被 C1Theme 覆盖
		RefreshButtonStyles();
		base.AcceptButton = btnConfirm;

		// 根据提示文字的长度自动布局控件（避免 lblPrompt 过高时，下方输入框/按钮看不见）
		LayoutControlsByPrompt(lblPrompt.Text ?? "");

		// 超长提示：如果 lblPrompt 计算出的实际高度比预设固定值高很多，提示内容改成可滚动的只读 TextBox，防止窗体过大
		ApplyPromptAutoScrollOrTextBox();

		// 多重文本模式：添加垂直滚动条，内容多时可滚动浏览
		if (_inputFormEnum == InputFormEnum.MultiText)
		{
			txtInputLeft.WordWrap = true;
			txtInputLeft.ScrollBars = ScrollBars.Vertical;
		}

		return base.ShowDialog();
	}

	/// <summary>当提示文字超长（> MaxPromptHeight）时，把 lblPrompt 隐藏，换成一个同样区域的只读可滚动 C1TextBox。
	/// 这样不会让整个窗体尺寸过大，用户可以滚动阅读。</summary>
	private void ApplyPromptAutoScrollOrTextBox()
	{
		const int maxPromptHeight = 120;
		if (_promptTextBox != null)
		{
			// 已有滚动文本框，重置可见性
			_promptTextBox.Visible = lblPrompt.Height > maxPromptHeight;
			lblPrompt.Visible = !_promptTextBox.Visible;
			if (_promptTextBox.Visible)
				_promptTextBox.Text = lblPrompt.Text ?? "";
			return;
		}

		if (lblPrompt.Height <= maxPromptHeight) return;

		var textBox = new C1TextBoxEx
		{
			Name = "txtPromptScroll",
			BackColor = BackColor,
			BorderStyle = BorderStyle.None,
			Multiline = true,
			ReadOnly = true,
			ScrollBars = ScrollBars.Vertical,
			WordWrap = true,
			Font = new Font("Noto Sans SC", 10.5f), // 字体统一 Noto Sans SC（字号不变）
			Location = lblPrompt.Location,
			Size = new Size(lblPrompt.Width, maxPromptHeight),
			Text = lblPrompt.Text ?? "",
			Anchor = lblPrompt.Anchor
		};
		textBox.VerticalAlign = VerticalAlignEnum.Top;
		_promptTextBox = textBox;
		Controls.Add(textBox);
		textBox.BringToFront();
		lblPrompt.Visible = false;
	}

	/// <summary>Prompt 超长时承载滚动显示的只读 TextBox</summary>
	private C1TextBoxEx _promptTextBox;

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.txtInputLeft = new Auditai.UI.Controls.C1TextBoxEx();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.lblPrompt = new C1.Win.C1Input.C1Label();
		this.lblwarnNum = new C1.Win.C1Input.C1Label();
		this.dateInputLeft = new C1.Win.C1Input.C1DateEdit();
		this.txtInputRight = new Auditai.UI.Controls.C1TextBoxEx();
		this.dateInputRight = new C1.Win.C1Input.C1DateEdit();
		((System.ComponentModel.ISupportInitialize)this.txtInputLeft).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPrompt).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblwarnNum).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputLeft).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtInputRight).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputRight).BeginInit();
		base.SuspendLayout();
		this.txtInputLeft.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtInputLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtInputLeft.Location = new System.Drawing.Point(26, 59);
		this.txtInputLeft.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtInputLeft.Name = "txtInputLeft";
		this.txtInputLeft.Size = new System.Drawing.Size(530, 28);
		this.txtInputLeft.TabIndex = 0;
		this.txtInputLeft.Tag = null;
		this.txtInputLeft.TextChanged += new System.EventHandler(txtInputLeft_TextChanged);
		this.txtInputLeft.KeyPress += new System.Windows.Forms.KeyPressEventHandler(txtInputLeft_KeyPress);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 Noto Sans SC
		this.btnConfirm.Location = new System.Drawing.Point(BtnConfirmLeft, 130);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnConfirm.TabIndex = 1;
		this.btnConfirm.Text = "确定";
		// 去除 UseVisualStyleBackColor，改用统一主按钮样式（RefreshButtonStyles 应用）
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 Noto Sans SC
		this.btnCancel.Location = new System.Drawing.Point(BtnCancelLeft, 130);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		// 去除 UseVisualStyleBackColor，改用统一次按钮样式（RefreshButtonStyles 应用）
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.lblPrompt.AutoSize = false;
		this.lblPrompt.BackColor = System.Drawing.Color.Transparent;
		this.lblPrompt.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPrompt.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 Noto Sans SC
		// 界面文字统一深灰（原 Color.Black）
		this.lblPrompt.ForeColor = TextPrimary;
		this.lblPrompt.Location = new System.Drawing.Point(26, 20);
		this.lblPrompt.Name = "lblPrompt";
		this.lblPrompt.Size = new System.Drawing.Size(550, 28);
		this.lblPrompt.TabIndex = 3;
		this.lblPrompt.Tag = null;
		this.lblPrompt.Text = "提示：";
		this.lblPrompt.TextDetached = true;
		this.lblwarnNum.AutoSize = true;
		this.lblwarnNum.BackColor = System.Drawing.Color.Transparent;
		this.lblwarnNum.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblwarnNum.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 Noto Sans SC（Color.Red 警告语义保留）
		this.lblwarnNum.ForeColor = System.Drawing.Color.Red;
		this.lblwarnNum.Location = new System.Drawing.Point(26, 94);
		this.lblwarnNum.Name = "lblwarnNum";
		this.lblwarnNum.Size = new System.Drawing.Size(57, 22);
		this.lblwarnNum.TabIndex = 4;
		this.lblwarnNum.Tag = null;
		this.lblwarnNum.Text = "警告：";
		this.lblwarnNum.TextDetached = true;
		this.lblwarnNum.Visible = false;
		this.dateInputLeft.AllowSpinLoop = false;
		this.dateInputLeft.Calendar.DayNameLength = 1;
		this.dateInputLeft.CustomFormat = "yyyy-MM-dd";
		this.dateInputLeft.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputLeft.ImagePadding = new System.Windows.Forms.Padding(0);
		this.dateInputLeft.Location = new System.Drawing.Point(26, 59);
		this.dateInputLeft.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.dateInputLeft.Name = "dateInputLeft";
		this.dateInputLeft.Size = new System.Drawing.Size(249, 28);
		this.dateInputLeft.VisibleButtons = C1.Win.C1Input.DropDownControlButtonFlags.None;
		this.dateInputLeft.DisplayFormat.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputLeft.KeyDown += new System.Windows.Forms.KeyEventHandler(dateInputLeft_KeyDown);
		this.txtInputRight.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtInputRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtInputRight.Location = new System.Drawing.Point(300, 59);
		this.txtInputRight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtInputRight.Name = "txtInputRight";
		this.txtInputRight.Size = new System.Drawing.Size(274, 28);
		this.txtInputRight.TabIndex = 6;
		this.txtInputRight.Tag = null;
		this.txtInputRight.TextChanged += new System.EventHandler(txtInputRight_TextChanged);
		this.txtInputRight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(txtInputRight_KeyPress);
		this.dateInputRight.AllowSpinLoop = false;
		this.dateInputRight.Calendar.DayNameLength = 1;
		this.dateInputRight.CustomFormat = "yyyy-MM-dd";
		this.dateInputRight.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputRight.ImagePadding = new System.Windows.Forms.Padding(0);
		this.dateInputRight.Location = new System.Drawing.Point(300, 59);
		this.dateInputRight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.dateInputRight.Name = "dateInputRight";
		this.dateInputRight.Size = new System.Drawing.Size(274, 28);
		this.dateInputRight.VisibleButtons = C1.Win.C1Input.DropDownControlButtonFlags.None;
		this.dateInputRight.DisplayFormat.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputRight.KeyDown += new System.Windows.Forms.KeyEventHandler(dateInputRight_KeyDown);
		base.AcceptButton = this.btnConfirm;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(FormClientWidth, 175);
		base.Controls.Add(this.dateInputRight);
		base.Controls.Add(this.txtInputRight);
		base.Controls.Add(this.dateInputLeft);
		base.Controls.Add(this.lblwarnNum);
		base.Controls.Add(this.lblPrompt);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnConfirm);
		base.Controls.Add(this.txtInputLeft);
		this.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 Noto Sans SC
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "InputBoxImpl";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.txtInputLeft).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPrompt).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblwarnNum).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputLeft).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtInputRight).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputRight).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
