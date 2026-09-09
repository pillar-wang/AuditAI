﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class UpdateForm : C1RibbonForm
{
	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlImage;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancel;

	private C1Button btnConfirm;

	private C1Button btnDetail;

	private C1SplitterPanel pnlContent;

	private C1Label lblNotice;

	/// <summary>超长内容时承载滚动显示的文本框</summary>
	private C1TextBoxEx _txtNoticeScroll;

	private C1PictureBox imgBox;

	#region === 设计令牌（默认小清新浅蓝，Google 蓝主题下自动切换） ===

	/// <summary>内容区左右内边距</summary>
	private const int ContentPaddingX = 24;

	/// <summary>内容区顶部内边距</summary>
	private const int ContentPaddingTop = 24;

	/// <summary>内容区底部内边距</summary>
	private const int ContentPaddingBottom = 16;

	/// <summary>图标面板宽度</summary>
	private const int ImagePanelWidth = 120;

	/// <summary>按钮面板高度</summary>
	private const int ButtonPanelHeight = 72;

	/// <summary>按钮宽度（增大以提升点击舒适度）</summary>
	private const int ButtonWidth = 110;

	/// <summary>按钮高度（36→40 对齐统一按钮规格）</summary>
	private const int ButtonHeight = 40;

	/// <summary>按钮圆角半径（统一规范 8px）</summary>
	private const int ButtonRadius = 8;

	/// <summary>按钮右侧距面板右边缘的距离（避免按钮贴边）</summary>
	private const int ButtonRightMargin = 50;

	/// <summary>按钮之间的水平间距</summary>
	private const int ButtonGap = 12;

	/// <summary>弹窗最小尺寸（短消息）</summary>
	private static readonly Size MinDialogSize = new Size(680, 300);

	/// <summary>弹窗中等尺寸（中等长度消息）</summary>
	private static readonly Size MidDialogSize = new Size(680, 440);

	/// <summary>弹窗最大尺寸</summary>
	private static readonly Size MaxDialogSize = new Size(Screen.PrimaryScreen != null
		? Math.Min(Screen.PrimaryScreen.WorkingArea.Width - 80, 950)
		: 950, Screen.PrimaryScreen != null
		? Math.Min(Screen.PrimaryScreen.WorkingArea.Height - 80, 700)
		: 700);

	/// <summary>Surface-0：窗体最底层背景</summary>
	private Color Surface0;

	/// <summary>Surface-1：内容卡片背景（纯白，形成层叠深度）</summary>
	private Color Surface1;

	/// <summary>Surface-2：按钮面板背景</summary>
	private Color Surface2;

	/// <summary>边框/分隔线色（主题主色）</summary>
	private Color LineColor;

	/// <summary>主文字色（确保 WCAG 4.5:1 对比度）</summary>
	private Color TextPrimary;

	/// <summary>次要文字色</summary>
	private Color TextSecondary;

	/// <summary>主色按下态（暗一档）</summary>
	private Color PrimaryDark;

	/// <summary>主色悬停态（亮一档）</summary>
	private Color PrimaryLight;

	/// <summary>次按钮边框/分隔线色</summary>
	private Color LineColorDefault;

	/// <summary>
	/// 根据当前主题加载颜色令牌
	/// 
	/// 注意：必须在实例构造时调用，不能用静态构造——
	/// 静态构造只执行一次，如果那时主题还没切到 Google Blue，之后就不会变了。
	/// </summary>
	private void LoadThemeColors()
	{
		var theme = Theme.SelectedAuditaiTheme;
		if (theme != null && theme.Name == "auditai_GoogleBlue")
		{
			// 清新蓝色板
			Surface0 = Color.FromArgb(250, 251, 252);      // #fafbfc
			Surface1 = Color.FromArgb(255, 255, 255);       // #ffffff
			Surface2 = Color.FromArgb(243, 244, 246);       // #f3f4f6
			LineColor = Color.FromArgb(59, 130, 246);       // #3b82f6
			TextPrimary = Color.FromArgb(30, 41, 59);       // #1e293b
			TextSecondary = Color.FromArgb(71, 85, 105);    // #475569
			// 主按钮悬停/按下变体 + 次按钮边框色
			PrimaryDark = Color.FromArgb(29, 78, 216);
			PrimaryLight = Color.FromArgb(37, 99, 235);
			LineColorDefault = Color.FromArgb(229, 231, 235);
		}
		else
		{
			// 默认：小清新浅蓝
			Surface0 = Color.FromArgb(245, 249, 252);
			Surface1 = Color.FromArgb(255, 255, 255);
			Surface2 = Color.FromArgb(240, 247, 252);
			LineColor = Color.FromArgb(74, 144, 217);
			TextPrimary = Color.FromArgb(30, 41, 59);
			TextSecondary = Color.FromArgb(71, 85, 105);
			// 主按钮悬停/按下变体 + 次按钮边框色
			PrimaryDark = Color.FromArgb(53, 123, 189);
			PrimaryLight = Color.FromArgb(90, 160, 230);
			LineColorDefault = Color.FromArgb(229, 231, 235);
		}
	}

	#endregion

	/// <summary>当前窗体创建的按钮列表（用于重定位）</summary>
	private readonly System.Collections.Generic.List<C1Button> _buttons = new System.Collections.Generic.List<C1Button>();

	public UpdateForm()
	{
		// 先加载主题颜色（必须在 InitializeComponent 之前）
		LoadThemeColors();
		InitializeComponent();
		base.Shown += UpdateForm_Shown;
		Text = "版本更新";
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		// 初始化主题色（主题应用后会再次重设，防止被覆盖）
		ApplyThemeColors();
		// 统一按钮样式：确定=主按钮，取消/更新详情=次按钮，8px 圆角
		ApplyButtonStyle(btnConfirm, isPrimary: true);
		ApplyButtonStyle(btnCancel, isPrimary: false);
		ApplyButtonStyle(btnDetail, isPrimary: false);
		// 超长内容滚动用的 TextBox（仿 MessageShowBox）
		_txtNoticeScroll = new C1TextBoxEx
		{
			Dock = DockStyle.Fill,
			BackColor = Surface1,
			ForeColor = TextPrimary,
			BorderColor = LineColor,
			BorderStyle = BorderStyle.None,
			Multiline = true,
			ReadOnly = true,
			ScrollBars = ScrollBars.Vertical,
			Visible = false,
			Font = new Font("微软雅黑", 10.5f), // 字体统一 微软雅黑（字号不变）
			Name = "txtNoticeScroll"
		};
		pnlContent.Controls.Add(_txtNoticeScroll);
	}

	private void UpdateForm_Shown(object sender, EventArgs e)
	{
		base.Icon = IconLibrary.CreateIcon("arrows-clockwise", 32, IconLibrary.DefaultColor);
	}

	/// <summary>
	/// 应用小清新浅蓝配色到各个控件，主题设置前后都会调用，防止被 C1Theme 覆盖。
	/// </summary>
	private void ApplyThemeColors()
	{
		ctnAll.BackColor = Surface0;
		ctnAll.BorderColor = LineColor;
		ctnAll.ForeColor = TextPrimary;

		pnlImage.BackColor = Surface0;
		pnlContent.BackColor = Surface1;
		pnlButtons.BackColor = Surface2;
		// 按钮区顶部细线分隔
		pnlButtons.BorderWidth = 1;
		pnlButtons.BorderColor = LineColor;

		lblNotice.BackColor = Surface1;
		lblNotice.ForeColor = TextPrimary;
	}

	/// <summary>
	/// 统一按钮样式：确定=主按钮（蓝底白字无边框），取消/更新详情=次按钮（白底深灰字+浅灰边框），均 Flat + 8px 圆角 Region。
	/// </summary>
	private void ApplyButtonStyle(C1Button btn, bool isPrimary)
	{
		btn.FlatStyle = FlatStyle.Flat;
		btn.Font = new Font("微软雅黑", 10.5f, FontStyle.Regular); // 字体统一 微软雅黑
		if (isPrimary)
		{
			// 主按钮：LineColor 即主题主色 Primary，蓝填充 + 白字（悬停/按下深浅变体）
			btn.BackColor = LineColor;
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
		// 8px 圆角 Region（四角 AddArc，同 MessageShowBox）
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

	/// <summary>
	/// 主题应用后强制重设样式，确保高对比度不被主题覆盖（同 MessageShowBox 模式）。
	/// </summary>
	private void EnsureStylesCorrect()
	{
		ApplyThemeColors();
		// 统一重设主/次按钮样式，防止被 C1Theme 覆盖（主按钮保持白字）
		ApplyButtonStyle(btnConfirm, isPrimary: true);
		ApplyButtonStyle(btnCancel, isPrimary: false);
		ApplyButtonStyle(btnDetail, isPrimary: false);
		if (_txtNoticeScroll != null)
		{
			_txtNoticeScroll.BackColor = Surface1;
			_txtNoticeScroll.ForeColor = TextPrimary;
			_txtNoticeScroll.BorderColor = LineColor;
		}
	}

	public UpdateForm(MessageBoxIcon icon, MessageBoxButtons buttons, string text)
		: this()
	{
		switch (icon)
		{
		case MessageBoxIcon.Asterisk:
			imgBox.BackgroundImage = Resource1.提示;
			break;
		case MessageBoxIcon.Question:
			imgBox.BackgroundImage = Resource1.问号;
			break;
		case MessageBoxIcon.Exclamation:
			imgBox.BackgroundImage = Resource1.警告;
			break;
		case MessageBoxIcon.Hand:
			imgBox.BackgroundImage = Resource1.错误;
			break;
		default:
			imgBox.BackgroundImage = Resource1.提示;
			break;
		}
		switch (buttons)
		{
		case MessageBoxButtons.YesNo:
			btnConfirm.Click += btnConfirmYesNo_Click;
			btnCancel.Click += btnCancelYesNo_Click;
			AnchorPosition1(pnlButtons, btnDetail);
			AnchorPosition2(pnlButtons, btnConfirm);
			AnchorPosition3(pnlButtons, btnCancel);
			break;
		case MessageBoxButtons.OK:
			btnConfirm.Click += btnConfirmYesNo_Click;
			btnCancel.Visible = false;
			AnchorPosition2(pnlButtons, btnDetail);
			AnchorPosition3(pnlButtons, btnConfirm);
			break;
		}
		lblNotice.Text = text;
	}

	private void btnDetail_Click(object sender, EventArgs e)
	{
		// 已禁用远程更新详情页面
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前为本地模式，不支持在线更新");
	}

	private void btnConfirmYesNo_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Yes;
		Close();
	}

	private void btnCancelYesNo_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.No;
		Close();
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentObject(this);
		Theme.SetCurrentTree(this);
		// 主题设置后强制重设颜色，确保高对比度不被 C1Theme 覆盖
		EnsureStylesCorrect();
		StandardView();
		// 超长内容切换到滚动视图
		if (ContentNeedsScroll()) ShowScrollView(true);
		// 根据内容自动调整尺寸
		AutoSizeDialog();
		RepositionButtons();
		return base.ShowDialog();
	}

	/// <summary>根据给定的最大高度判断，内容是否超出最大尺寸，需要启用垂直滚动条</summary>
	private bool ContentNeedsScroll()
	{
		string message = lblNotice.Text ?? "";
		Font font = new Font("微软雅黑", 10.5f); // 字体统一 微软雅黑（字号不变）
		const int dialogWidth = 680;
		int contentTextWidth = dialogWidth - ImagePanelWidth - ContentPaddingX * 2;
		int maxContentH = MaxDialogSize.Height - ButtonPanelHeight;
		int maxTextH = maxContentH - ContentPaddingTop - ContentPaddingBottom;

		using (var g = CreateGraphics())
		{
			Size textSize = TextRenderer.MeasureText(g, message, font,
				new Size(contentTextWidth, int.MaxValue),
				TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
			// 15% 缓冲，避免文字实际行间距大于 MeasureText 时判断不准确
			return (int)(textSize.Height * 1.15f) > maxTextH;
		}
	}

	/// <summary>切换显示视图：滚动版(true)/标签版(false)</summary>
	private void ShowScrollView(bool show)
	{
		if (show)
		{
			lblNotice.Visible = false;
			_txtNoticeScroll.Text = lblNotice.Text ?? "";
			_txtNoticeScroll.Visible = true;
			_txtNoticeScroll.VerticalAlign = VerticalAlignEnum.Top;
		}
		else
		{
			_txtNoticeScroll.Visible = false;
			lblNotice.Visible = true;
		}
	}

	/// <summary>根据内容自动调整弹窗尺寸：3 档（短/中/长）+ 超长封顶滚动条</summary>
	private void AutoSizeDialog()
	{
		string message = lblNotice.Text ?? "";
		Font font = new Font("微软雅黑", 10.5f); // 字体统一 微软雅黑（字号不变）
		const int dialogWidth = 680;
		int contentTextWidth = dialogWidth - ImagePanelWidth - ContentPaddingX * 2;
		int textH;

		using (var g = CreateGraphics())
		{
			Size textSize = TextRenderer.MeasureText(g, message, font,
				new Size(contentTextWidth, int.MaxValue),
				TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
			textH = (int)(textSize.Height * 1.15f);
		}

		int contentPanelH = textH + ContentPaddingTop + ContentPaddingBottom;

		int minContentH = MinDialogSize.Height - ButtonPanelHeight;
		int midContentH = MidDialogSize.Height - ButtonPanelHeight;
		int maxContentH = MaxDialogSize.Height - ButtonPanelHeight;

		int desiredContentH;
		if (contentPanelH <= minContentH) desiredContentH = minContentH;
		else if (contentPanelH <= midContentH) desiredContentH = midContentH;
		else if (contentPanelH <= maxContentH) desiredContentH = contentPanelH;
		else desiredContentH = maxContentH;

		int totalHeight = desiredContentH + ButtonPanelHeight;

		base.ClientSize = new Size(dialogWidth, totalHeight);
		this.MinimumSize = new Size(MinDialogSize.Width, MinDialogSize.Height);

		pnlButtons.Height = ButtonPanelHeight;

		this.PerformLayout();
		Padding pad = new Padding(ContentPaddingX, ContentPaddingTop, ContentPaddingX, ContentPaddingBottom);
		if (lblNotice.Width > pad.Left + pad.Right + 10 && lblNotice.Height > pad.Top + pad.Bottom + 10)
		{
			lblNotice.Padding = pad;
		}
		this.PerformLayout();

		lblNotice.TextAlign = ContentAlignment.TopLeft;
	}

	private void AnchorPosition1(C1SplitterPanel panel, Control control)
	{
		// C1SplitterPanel 内按钮使用显式 Location，必须 Anchor=None 避免冲突（project_memory 约束）
		control.Anchor = AnchorStyles.None;
		control.Top = (panel.Height - control.Height) / 2;
		control.Left = panel.Width - (ButtonRightMargin + ButtonWidth * 3 + ButtonGap * 2);
		((C1Button)control).ForeColor = TextPrimary;
		_buttons.Add((C1Button)control);
	}

	private void AnchorPosition2(C1SplitterPanel panel, Control control)
	{
		control.Anchor = AnchorStyles.None;
		control.Top = (panel.Height - control.Height) / 2;
		control.Left = panel.Width - (ButtonRightMargin + ButtonWidth * 2 + ButtonGap);
		((C1Button)control).ForeColor = TextPrimary;
		_buttons.Add((C1Button)control);
	}

	private void AnchorPosition3(C1SplitterPanel panel, Control control)
	{
		control.Anchor = AnchorStyles.None;
		control.Top = (panel.Height - control.Height) / 2;
		control.Left = panel.Width - (ButtonRightMargin + ButtonWidth);
		((C1Button)control).ForeColor = TextPrimary;
		_buttons.Add((C1Button)control);
	}

	private void RepositionButtons()
	{
		// 统一规则：offsets[i] = ButtonRightMargin + ButtonWidth + (count-1-i) * (ButtonWidth + ButtonGap)
		// 即最右侧按钮距 panel 右边 ButtonRightMargin，按钮间间距 ButtonGap，按钮宽度 ButtonWidth
		if (pnlButtons == null || _buttons.Count == 0) return;

		int panelW = pnlButtons.Width;
		int count = _buttons.Count;
		int[] offsets = new int[count];
		for (int i = 0; i < count; i++)
		{
			int rightIdx = count - 1 - i; // 0 = 最右，1 = 次右，...
			offsets[i] = ButtonRightMargin + ButtonWidth + rightIdx * (ButtonWidth + ButtonGap);
		}

		for (int i = 0; i < count; i++)
		{
			_buttons[i].Top = (pnlButtons.Height - _buttons[i].Height) / 2;
			_buttons[i].Left = panelW - offsets[i];
		}
	}

	private void StandardView()
	{
		// 字体统一 微软雅黑（字号不变）
		lblNotice.Font = new Font("微软雅黑", 10.5f);
		btnDetail.Font = new Font("微软雅黑", 10.5f);
		btnConfirm.Font = new Font("微软雅黑", 10.5f);
		btnCancel.Font = new Font("微软雅黑", 10.5f);
		// C1SplitterPanel 内按钮使用显式 Location，必须 Anchor=None
		btnDetail.Anchor = AnchorStyles.None;
		btnConfirm.Anchor = AnchorStyles.None;
		btnCancel.Anchor = AnchorStyles.None;
		btnConfirm.Size = new Size(ButtonWidth, ButtonHeight);
		btnCancel.Size = new Size(ButtonWidth, ButtonHeight);
		btnDetail.Size = new Size(ButtonWidth, ButtonHeight);
		btnConfirm.ForeColor = TextPrimary;
		btnCancel.ForeColor = TextPrimary;
		btnDetail.ForeColor = TextPrimary;
		ctnAll.SplitterWidth = 0;
		pnlImage.BorderWidth = 0;
		pnlContent.BorderWidth = 0;
		// 按钮区顶部细线分隔（视觉层次）
		pnlButtons.BorderWidth = 1;
		pnlButtons.BorderColor = LineColor;
		imgBox.BackgroundImageLayout = ImageLayout.Center;
	}

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
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.btnDetail = new C1.Win.C1Input.C1Button();
		this.pnlImage = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.imgBox = new C1.Win.C1Input.C1PictureBox();
		this.pnlContent = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblNotice = new C1.Win.C1Input.C1Label();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnDetail).BeginInit();
		this.pnlImage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.imgBox).BeginInit();
		this.pnlContent.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblNotice).BeginInit();
		base.SuspendLayout();
		// === C1SplitContainer: AutoSizeElement=Both + 显式尺寸，避免布局错位（project_memory 约束） ===
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = Surface0;
		this.ctnAll.BorderColor = LineColor;
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		this.ctnAll.ForeColor = TextPrimary;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlButtons);
		this.ctnAll.Panels.Add(this.pnlImage);
		this.ctnAll.Panels.Add(this.pnlContent);
		this.ctnAll.Size = new System.Drawing.Size(680, 300);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		// === 按钮面板：Bottom 停靠，固定高度，显式 Location/Size/SizeRatio ===
		this.pnlButtons.BackColor = Surface2;
		this.pnlButtons.BorderColor = LineColor;
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnConfirm);
		this.pnlButtons.Controls.Add(this.btnDetail);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = ButtonPanelHeight;
		this.pnlButtons.Location = new System.Drawing.Point(0, 300 - ButtonPanelHeight);
		this.pnlButtons.Size = new System.Drawing.Size(680, ButtonPanelHeight);
		this.pnlButtons.SizeRatio = 1f;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.MinHeight = ButtonPanelHeight;
		this.pnlButtons.MinWidth = 41;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.TabIndex = 1;
		// === 按钮：Anchor=None（显式定位）+ 高对比度文字 + 统一尺寸 ===
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		this.btnCancel.ForeColor = TextPrimary;
		this.btnCancel.Location = new System.Drawing.Point(547, (ButtonPanelHeight - ButtonHeight) / 2); // 垂直居中改用 ButtonHeight 常量（高度 36→40 自适配）
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		// 去除 UseVisualStyleBackColor，改用统一次按钮样式（ApplyButtonStyle 应用）
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.btnConfirm.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		this.btnConfirm.ForeColor = TextPrimary;
		this.btnConfirm.Location = new System.Drawing.Point(418, (ButtonPanelHeight - ButtonHeight) / 2); // 垂直居中改用 ButtonHeight 常量（高度 36→40 自适配）
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnConfirm.TabIndex = 1;
		this.btnConfirm.Text = "确定";
		// 去除 UseVisualStyleBackColor，改用统一主按钮样式（ApplyButtonStyle 应用）
		this.btnDetail.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.btnDetail.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		this.btnDetail.ForeColor = TextPrimary;
		this.btnDetail.Location = new System.Drawing.Point(290, (ButtonPanelHeight - ButtonHeight) / 2); // 垂直居中改用 ButtonHeight 常量（高度 36→40 自适配）
		this.btnDetail.Name = "btnDetail";
		this.btnDetail.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnDetail.TabIndex = 0;
		this.btnDetail.Text = "更新详情";
		// 去除 UseVisualStyleBackColor，改用统一次按钮样式（ApplyButtonStyle 应用）
		this.btnDetail.Click += new System.EventHandler(btnDetail_Click);
		// === 图标面板：Left 停靠，固定宽度，显式 Location/Size/SizeRatio ===
		this.pnlImage.BackColor = Surface0;
		this.pnlImage.Controls.Add(this.imgBox);
		this.pnlImage.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlImage.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		this.pnlImage.Width = ImagePanelWidth;
		this.pnlImage.Location = new System.Drawing.Point(0, 0);
		this.pnlImage.Size = new System.Drawing.Size(ImagePanelWidth, 300 - ButtonPanelHeight);
		this.pnlImage.SizeRatio = 1f;
		this.pnlImage.KeepRelativeSize = false;
		this.pnlImage.MinHeight = 41;
		this.pnlImage.MinWidth = ImagePanelWidth;
		this.pnlImage.Name = "pnlImage";
		this.pnlImage.TabIndex = 0;
		this.imgBox.BackColor = System.Drawing.Color.Transparent;
		this.imgBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.imgBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.imgBox.Location = new System.Drawing.Point(0, 0);
		this.imgBox.Name = "imgBox";
		this.imgBox.TabIndex = 0;
		this.imgBox.TabStop = false;
		// === 内容面板：填充剩余区域，显式 Location/Size/SizeRatio ===
		this.pnlContent.BackColor = Surface1;
		this.pnlContent.Controls.Add(this.lblNotice);
		this.pnlContent.Location = new System.Drawing.Point(ImagePanelWidth, 0);
		this.pnlContent.Size = new System.Drawing.Size(680 - ImagePanelWidth, 300 - ButtonPanelHeight);
		this.pnlContent.SizeRatio = 1f;
		this.pnlContent.Name = "pnlContent";
		this.pnlContent.TabIndex = 2;
		// === 内容标签：高对比度前景色 ===
		this.lblNotice.BackColor = Surface1;
		this.lblNotice.ForeColor = TextPrimary;
		this.lblNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblNotice.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblNotice.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		this.lblNotice.Location = new System.Drawing.Point(0, 0);
		this.lblNotice.Name = "lblNotice";
		// Padding 不在此处设置，避免 Theme 设置期间控件尺寸 < Padding 总和触发校验异常
		this.lblNotice.TabIndex = 0;
		this.lblNotice.Tag = null;
		this.lblNotice.Text = "c1Label1";
		this.lblNotice.TextAlign = System.Drawing.ContentAlignment.TopLeft;
		this.lblNotice.TextDetached = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(680, 300);
		base.Controls.Add(this.ctnAll);
		this.MinimumSize = MinDialogSize;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "UpdateForm";
		this.Text = "UpdateForm";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnDetail).EndInit();
		this.pnlImage.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.imgBox).EndInit();
		this.pnlContent.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.lblNotice).EndInit();
		base.ResumeLayout(false);
	}
}
