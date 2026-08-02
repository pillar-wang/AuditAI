﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Win.C1Input;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

/// <summary>
/// 简洁化消息提示框（小清新浅蓝风格，与 frmLogin/frmFindPwd 视觉统一）。
/// 改动要点：
///   1. 基类改为 Form + 自定义无边框标题栏（移除 C1RibbonForm 依赖）
///   2. 去除左侧独立图标面板（96px），改为 32×32 小图标融入内容区左上角
///   3. 背景层次从 3 层缩为 2 层（浅蓝渐变窗体 + 白色内容卡），去除按钮顶部分隔边框
///   4. 最小尺寸 560×240 → 440×180，更小巧精致
///   5. 按钮高度 36→40（接近 44px 触控目标），按钮栏 72→60
///   6. 字体统一 Noto Sans SC 10.5f，主按钮 Primary 浅蓝填充 + 白字
///   7. 对外 MessageBox.Show(...) API 完全兼容，零侵入
/// </summary>
public class MessageShowBox : Form
{
    private readonly MessageBoxButtons _buttonType;
    private readonly MessageBoxIcon _boxIcon;
    private string _message = string.Empty;

#pragma warning disable CS0649
    private IContainer components;
#pragma warning restore CS0649

    // --- 自定义标题栏控件 ---
    private Panel pnlTitleBar;
    private Label lblTitle;
    private C1Button btnClose;

    // --- 内容与按钮容器 ---
    private Panel pnlCard;         // 白色圆角卡片（承载图标+文字+按钮栏）
    private Panel pnlContent;      // 图标 + 提示文字
    private PictureBox picIcon;    // 32×32 小图标（不再独立面板）
    private C1Label lblNotice;     // 短消息文字
    private C1TextBoxEx txtNotice; // 长消息滚动框

    #region === 设计令牌（小清新浅蓝风格，与 frmFindPwd 对齐） ===

    /// <summary>主色（74,144,217）用于主按钮/聚焦/下划线</summary>
    private static readonly Color Primary = Color.FromArgb(74, 144, 217);
    /// <summary>主色按下态（暗一档）</summary>
    private static readonly Color PrimaryDark = Color.FromArgb(53, 123, 189);
    /// <summary>主色悬停态（亮一档）</summary>
    private static readonly Color PrimaryLight = Color.FromArgb(90, 160, 230);
    /// <summary>边框/分隔线色（默认灰蓝）</summary>
    private static readonly Color LineColorDefault = Color.FromArgb(208, 215, 222);

    /// <summary>主文字色（深靛蓝灰，WCAG 15.8:1 on white）</summary>
    private static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);
    /// <summary>次文字色（标签/占位，WCAG 6.2:1）</summary>
    private static readonly Color TextSecondary = Color.FromArgb(71, 85, 105);

    /// <summary>标题栏高度（含顶部 3px 光带，拖拽区）</summary>
    private const int TitleBarHeight = 40;
    /// <summary>圆角半径（与 frmFindPwd 一致：12px）</summary>
    private const int Radius = 12;
    /// <summary>卡片外边距（窗体顶边到卡片的距离）</summary>
    private const int CardMargin = 16;
    /// <summary>卡片距窗体左/右边缘的边距（比上下更大，呼吸感更好）</summary>
    private const int CardSideMargin = 30;
    /// <summary>卡片距窗体下边缘的边距（比上/左/右更大，呼吸感更好）</summary>
    private const int CardBottomMargin = 36;

    /// <summary>内容区内边距（图标/文字到卡片边缘）</summary>
    private const int ContentPaddingX = 20;
    private const int ContentPaddingTop = 20;
    private const int ContentPaddingBottom = 16;

    /// <summary>图标大小（不再独占 96px 面板，直接嵌入内容区）</summary>
    private const int IconSize = 32;
    /// <summary>图标与文字的水平间距</summary>
    private const int IconTextGap = 14;

    /// <summary>按钮栏高度（原 72→60，更紧凑）</summary>
    private const int ButtonBarHeight = 60;
    /// <summary>按钮宽度（保留原设计的 110px）</summary>
    private const int ButtonWidth = 110;
    /// <summary>按钮高度（原 36→40，接近 44px 触控目标）</summary>
    private const int ButtonHeight = 40;
    /// <summary>按钮距卡片右边缘距离（保留 50px 用户偏好）</summary>
    private const int ButtonRightMargin = 50;
    /// <summary>按钮水平间距（12px 用户偏好）</summary>
    private const int ButtonGap = 12;
    /// <summary>按钮圆角半径（高度 40 → 半径 8，约 20% 圆润度）</summary>
    private const int ButtonRadius = 8;

    /// <summary>弹窗最小尺寸：760×320（加宽 140px，更舒展）</summary>
    private static readonly Size MinDialogSize = new Size(760, 320);
    /// <summary>弹窗中等尺寸：760×480（再加宽 140px）</summary>
    private static readonly Size MidDialogSize = new Size(760, 480);
    /// <summary>弹窗最大尺寸（宽 700 → 840，高保持不变）</summary>
    private static readonly Size MaxDialogSize = new Size(
        Screen.PrimaryScreen != null
            ? Math.Min(Screen.PrimaryScreen.WorkingArea.Width - 80, 840)
            : 840,
        Screen.PrimaryScreen != null
            ? Math.Min(Screen.PrimaryScreen.WorkingArea.Height - 80, 640)
            : 640);

    #endregion

    public MessageShowBox(MessageBoxButtons buttonType, MessageBoxIcon boxIcon)
    {
        InitializeComponent();
        base.StartPosition = FormStartPosition.CenterScreen;
        base.TopMost = true;
        _buttonType = buttonType;
        _boxIcon = boxIcon;
        base.FormClosing += MessageShowBox_FormClosing;

        // 图标 + 标题映射
        switch (_boxIcon)
        {
            case MessageBoxIcon.Asterisk:
                lblTitle.Text = "提示";
                picIcon.BackgroundImage = Resource1.提示;
                break;
            case MessageBoxIcon.Question:
                lblTitle.Text = "询问";
                picIcon.BackgroundImage = Resource1.问号;
                break;
            case MessageBoxIcon.Exclamation:
                lblTitle.Text = "警告";
                picIcon.BackgroundImage = Resource1.警告;
                break;
            case MessageBoxIcon.Hand:
                lblTitle.Text = "错误";
                picIcon.BackgroundImage = Resource1.错误;
                break;
            default:
                lblTitle.Text = "提示";
                picIcon.BackgroundImage = Resource1.提示;
                break;
        }
        this.Text = lblTitle.Text;

        // 按按钮类型生成按钮
        switch (_buttonType)
        {
            case MessageBoxButtons.OK:
                InitOkView();
                break;
            case MessageBoxButtons.OKCancel:
                InitOkCancelView();
                break;
            case MessageBoxButtons.YesNo:
                InitYesNoView();
                break;
            case MessageBoxButtons.YesNoCancel:
                InitYesNoCancelView();
                break;
        }
    }

    internal void SetMessage(string message)
    {
        _message = message ?? string.Empty;
        txtNotice.TextDetached = true;
        txtNotice.Text = _message;
        txtNotice.ReadOnly = true;
        lblNotice.TextDetached = true;
        lblNotice.Text = _message;
    }

    public new DialogResult ShowDialog()
    {
        // 先判定是否需要滚动条
        bool needsScroll = _message.Length > 200 || ContentNeedsScroll();
        ShowScrollView(needsScroll);
        // 根据内容动态计算尺寸
        AutoSizeDialog();
        // 重新定位按钮
        RepositionButtons();
        return base.ShowDialog();
    }

    // --------------------------------------------------------------------
    // 圆角裁剪（与 frmFindPwd 完全一致）
    // --------------------------------------------------------------------
    private void ApplyRoundedRegion(int radius)
    {
        using (var path = new GraphicsPath())
        {
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(Width - radius * 2, Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            Region = new Region(path);
        }
    }

    // --------------------------------------------------------------------
    // 内容滚动判定 & 尺寸自适应
    // --------------------------------------------------------------------
    /// <summary>
    /// 是否需要切换到垂直滚动视图（内容过多时，固定中档高度 + 滚动条）。
    /// 阈值：对齐「中档内容高度」——超过中档就改用滚动条，不再让弹窗继续增高。
    /// </summary>
    private bool ContentNeedsScroll()
    {
        Font font = new Font("Noto Sans SC", 10.5f);
        int contentTextWidth = MidDialogSize.Width
                               - CardSideMargin * 2
                               - ContentPaddingX * 2
                               - IconSize - IconTextGap;
        Size textSize = TextRenderer.MeasureText(_message ?? "", font,
            new Size(contentTextWidth, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
        int textH = (int)(textSize.Height * 1.15f);
        int contentH = Math.Max(IconSize, textH) + ContentPaddingTop + ContentPaddingBottom;
        int midContentH = MidDialogSize.Height - TitleBarHeight - CardMargin - CardBottomMargin - ButtonBarHeight;
        return contentH > midContentH;  // 超过中档高度 → 用滚动条
    }

    private void AutoSizeDialog()
    {
        Font font = new Font("Noto Sans SC", 10.5f);
        const int dialogWidth = 760;

        // 1. 计算文字高度
        int contentTextWidth = dialogWidth
                               - CardSideMargin * 2
                               - ContentPaddingX * 2
                               - IconSize - IconTextGap;
        int textH;
        using (var g = CreateGraphics())
        {
            Size textSize = TextRenderer.MeasureText(g, _message ?? "", font,
                new Size(contentTextWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
            textH = (int)(textSize.Height * 1.15f);
        }

        // 2. 内容区高度 = 顶/底 Padding + max(图标高, 文字高)
        int contentPanelH = ContentPaddingTop + ContentPaddingBottom
                            + Math.Max(IconSize, textH);

        // 3. 高度档位：
        //    · 短消息 → 最小档，避免大片空白
        //    · 中消息 → 中等档，按 contentPanelH 精确匹配（介于 min~mid 之间）
        //    · 长消息（超过中档）→ 固定为中档高度，不再继续增高（用户要求：弹窗大小不变，用垂直滚动条承载）
        int minContentH = MinDialogSize.Height - TitleBarHeight - CardMargin - CardBottomMargin - ButtonBarHeight;
        int midContentH = MidDialogSize.Height - TitleBarHeight - CardMargin - CardBottomMargin - ButtonBarHeight;

        int desiredContentH;
        if (contentPanelH <= minContentH) desiredContentH = minContentH;
        else if (contentPanelH <= midContentH) desiredContentH = contentPanelH; // 介于 Min~Mid 之间时，用精确高度避免空白
        else desiredContentH = midContentH; // 超过中档：固定中档高度，不再增高，用垂直滚动条

        int totalHeight = TitleBarHeight + CardMargin + CardBottomMargin + desiredContentH + ButtonBarHeight;

        // 4. 应用尺寸 + 圆角
        ClientSize = new Size(dialogWidth, totalHeight);
        MinimumSize = new Size(dialogWidth, MinDialogSize.Height);
        ApplyRoundedRegion(Radius);

        // 5. 布局卡片 / 内容区 / 按钮栏 尺寸
        pnlCard.Location = new Point(CardSideMargin, TitleBarHeight + CardMargin);
        pnlCard.Size = new Size(dialogWidth - CardSideMargin * 2, desiredContentH + ButtonBarHeight);

        pnlContent.Location = new Point(0, 0);
        pnlContent.Size = new Size(pnlCard.Width, desiredContentH);

        // 6. 图标位置：内容区左上角
        picIcon.Location = new Point(ContentPaddingX, ContentPaddingTop);
        picIcon.Size = new Size(IconSize, IconSize);

        // 7. 文字标签/文本框：在图标右侧
        int textLeft = ContentPaddingX + IconSize + IconTextGap;
        int textWidth = pnlContent.Width - ContentPaddingX - textLeft;
        int textHeight = desiredContentH - ContentPaddingTop - ContentPaddingBottom;
        lblNotice.Location = new Point(textLeft, ContentPaddingTop);
        lblNotice.Size = new Size(textWidth, textHeight);
        txtNotice.Location = new Point(textLeft, ContentPaddingTop);
        txtNotice.Size = new Size(textWidth, textHeight);

        PerformLayout();
    }

    // --------------------------------------------------------------------
    // 按钮系统（位置重算 + 四种按钮组合）
    // --------------------------------------------------------------------
    private readonly System.Collections.Generic.List<C1Button> _buttons = new System.Collections.Generic.List<C1Button>();

    private C1Button NewButton(string text, bool isPrimary)
    {
        var btn = new C1Button
        {
            Text = text,
            Width = ButtonWidth,
            Height = ButtonHeight,
            Font = new Font("Noto Sans SC", 10.5f, FontStyle.Regular),
            Anchor = AnchorStyles.None,
            FlatStyle = FlatStyle.Flat
        };
        btn.FlatAppearance.BorderSize = 0;
        if (isPrimary)
        {
            // 主按钮：Primary 浅蓝填充 + 白字（与 frmFindPwd.btnFindPwd 风格统一）
            btn.BackColor = Primary;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.MouseDownBackColor = PrimaryDark;
            btn.FlatAppearance.MouseOverBackColor = PrimaryLight;
        }
        else
        {
            // 次按钮：白底灰字 + 浅灰蓝边框，视觉低调
            btn.BackColor = Color.White;
            btn.ForeColor = TextPrimary;
            btn.FlatAppearance.BorderColor = LineColorDefault;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(240, 247, 252);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 249, 252);
        }
        // 圆角化：用 GraphicsPath 把按钮区域裁成圆角（次按钮的 1px 边框会沿圆角边缘绘制）
        using (var path = new GraphicsPath())
        {
            int w = btn.Width, h = btn.Height, r = ButtonRadius * 2;
            path.AddArc(0, 0, r, r, 180, 90);
            path.AddArc(w - r, 0, r, r, 270, 90);
            path.AddArc(w - r, h - r, r, r, 0, 90);
            path.AddArc(0, h - r, r, r, 90, 90);
            path.CloseFigure();
            btn.Region = new Region(path);
        }
        return btn;
    }

    /// <summary>
    /// 按按钮在 _buttons 中的顺序，重新计算 Left，确保右对齐 + 用户偏好 50/110/12 规格。
    /// </summary>
    private void RepositionButtons()
    {
        if (pnlCard == null || _buttons.Count == 0) return;

        int panelW = pnlCard.Width;
        int count = _buttons.Count;
        int panelTop = pnlContent.Height; // 按钮栏在内容区下方

        for (int i = 0; i < count; i++)
        {
            // i=0 最左；i=count-1 最右（距右边缘 ButtonRightMargin）
            int rightIdx = count - 1 - i;
            int leftPos = panelW
                          - (ButtonRightMargin + ButtonWidth)
                          - rightIdx * (ButtonWidth + ButtonGap);
            _buttons[i].Location = new Point(
                leftPos,
                panelTop + (ButtonBarHeight - ButtonHeight) / 2);
        }
    }

    private void InitOkView()
    {
        var ok = NewButton("确定", isPrimary: true);
        AddButtonToCard(ok);
        ok.Click += delegate { DialogResult = DialogResult.OK; Close(); };
        ok.Focus();
    }

    private void InitOkCancelView()
    {
        var ok = NewButton("确定", isPrimary: true);
        var cancel = NewButton("取消", isPrimary: false);
        AddButtonToCard(ok);
        AddButtonToCard(cancel);
        ok.Click += delegate { DialogResult = DialogResult.OK; Close(); };
        cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
        ok.Focus();
    }

    private void InitYesNoView()
    {
        var yes = NewButton("是", isPrimary: true);
        var no = NewButton("否", isPrimary: false);
        AddButtonToCard(yes);
        AddButtonToCard(no);
        yes.Click += delegate { DialogResult = DialogResult.Yes; Close(); };
        no.Click += delegate { DialogResult = DialogResult.No; Close(); };
        yes.Focus();
    }

    private void InitYesNoCancelView()
    {
        var yes = NewButton("是", isPrimary: true);
        var no = NewButton("否", isPrimary: false);
        var cancel = NewButton("取消", isPrimary: false);
        AddButtonToCard(yes);
        AddButtonToCard(no);
        AddButtonToCard(cancel);
        yes.Click += delegate { DialogResult = DialogResult.Yes; Close(); };
        no.Click += delegate { DialogResult = DialogResult.No; Close(); };
        cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
        yes.Focus();
    }

    private void AddButtonToCard(C1Button btn)
    {
        pnlCard.Controls.Add(btn);
        btn.BringToFront();
        _buttons.Add(btn);
    }

    // --------------------------------------------------------------------
    // 滚动视图切换 / 关闭按钮 / 标题栏拖拽
    // --------------------------------------------------------------------
    private void ShowScrollView(bool scroll)
    {
        if (scroll)
        {
            lblNotice.Hide();
            txtNotice.Show();
            txtNotice.BringToFront();
        }
        else
        {
            txtNotice.Hide();
            lblNotice.Show();
            lblNotice.BringToFront();
        }
    }

    private void MessageShowBox_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.None) DialogResult = DialogResult.Cancel;
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    // 标题栏拖拽（与 frmFindPwd 一致，通过 Win32 消息）
    public const int WM_SYSCOMMAND = 0x0112;
    public const int SC_MOVE = 0xF010;
    public const int HTCAPTION = 0x0002;

    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    private void pnlTitleBar_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && e.Y <= TitleBarHeight)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_SYSCOMMAND, SC_MOVE | HTCAPTION, 0);
        }
    }

    // --------------------------------------------------------------------
    // 外观绘制：浅蓝渐变窗体背景 + 顶部细光带 + 白色卡片（与 frmFindPwd 对齐）
    // --------------------------------------------------------------------
    private void MessageShowBox_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 1) 浅蓝渐变窗体背景
        Rectangle bgRect = new Rectangle(0, 0, Width, Height);
        using (var bgBrush = new LinearGradientBrush(
            bgRect, Color.FromArgb(227, 240, 255), Color.FromArgb(245, 248, 250),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(bgBrush, bgRect);
        }
        using (var bgBrush2 = new LinearGradientBrush(
            new Point(Width, 0), new Point(0, Height),
            Color.FromArgb(150, 220, 235, 255), Color.FromArgb(150, 240, 248, 255)))
        {
            g.FillRectangle(bgBrush2, bgRect);
        }

        // 2) 标题栏顶部 3px 细光带（主色渐变）
        Rectangle topBar = new Rectangle(0, 0, Width, 3);
        using (var topBrush = new LinearGradientBrush(
            topBar, PrimaryLight, Primary, LinearGradientMode.Horizontal))
        {
            g.FillRectangle(topBrush, topBar);
        }

        // 3) 白色圆角卡片（阴影 + 白色填充 + 浅蓝细边框）
        int cardX = pnlCard.Left;
        int cardY = pnlCard.Top;
        int cardW = pnlCard.Width;
        int cardH = pnlCard.Height;
        using (var path = new GraphicsPath())
        {
            path.AddArc(cardX, cardY, Radius * 2, Radius * 2, 180, 90);
            path.AddArc(cardX + cardW - Radius * 2, cardY, Radius * 2, Radius * 2, 270, 90);
            path.AddArc(cardX + cardW - Radius * 2, cardY + cardH - Radius * 2, Radius * 2, Radius * 2, 0, 90);
            path.AddArc(cardX, cardY + cardH - Radius * 2, Radius * 2, Radius * 2, 90, 90);
            path.CloseFigure();

            using (var shadowBrush = new SolidBrush(Color.FromArgb(25, 74, 144, 226)))
            {
                g.TranslateTransform(3, 5);
                g.FillPath(shadowBrush, path);
                g.TranslateTransform(-3, -5);
            }
            using (var cardBrush = new SolidBrush(Color.White))
            {
                g.FillPath(cardBrush, path);
            }
            using (var borderPen = new Pen(Color.FromArgb(180, 215, 245), 1f))
            {
                g.DrawPath(borderPen, path);
            }
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (!string.IsNullOrEmpty(Name)) // 已初始化完成
        {
            ApplyRoundedRegion(Radius);
            Invalidate(); // 触发重绘阴影/边框
        }
    }

    // --------------------------------------------------------------------
    // Dispose + InitializeComponent
    // --------------------------------------------------------------------
    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.pnlTitleBar = new System.Windows.Forms.Panel();
        this.lblTitle = new System.Windows.Forms.Label();
        this.btnClose = new C1.Win.C1Input.C1Button();
        this.pnlCard = new System.Windows.Forms.Panel();
        this.pnlContent = new System.Windows.Forms.Panel();
        this.picIcon = new System.Windows.Forms.PictureBox();
        this.lblNotice = new C1.Win.C1Input.C1Label();
        this.txtNotice = new Auditai.UI.Controls.C1TextBoxEx();
        this.pnlTitleBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
        this.pnlCard.SuspendLayout();
        this.pnlContent.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.picIcon)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.lblNotice)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.txtNotice)).BeginInit();
        base.SuspendLayout();

        // ================== pnlTitleBar 自定义标题栏 ==================
        this.pnlTitleBar.BackColor = System.Drawing.Color.Transparent;
        this.pnlTitleBar.Controls.Add(this.lblTitle);
        this.pnlTitleBar.Controls.Add(this.btnClose);
        this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
        this.pnlTitleBar.Name = "pnlTitleBar";
        this.pnlTitleBar.Size = new System.Drawing.Size(620, TitleBarHeight);
        this.pnlTitleBar.TabIndex = 0;
        this.pnlTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(pnlTitleBar_MouseDown);

        // ---- lblTitle：居中显示标题（标题栏有内边距，不贴边缘） ----
        this.lblTitle.AutoSize = true;
        this.lblTitle.BackColor = System.Drawing.Color.Transparent;
        this.lblTitle.Font = new System.Drawing.Font("Noto Sans SC", 11f, System.Drawing.FontStyle.Bold,
            System.Drawing.GraphicsUnit.Point, 134);
        this.lblTitle.ForeColor = TextPrimary;
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Text = "提示";
        // 垂直居中 + 水平居中（给左侧/右侧留白，标题栏有内边距不贴边）
        this.lblTitle.Location = new System.Drawing.Point(16, 10);
        this.lblTitle.Size = new System.Drawing.Size(60, 25);
        this.lblTitle.MouseDown += new System.Windows.Forms.MouseEventHandler(pnlTitleBar_MouseDown);

        // ---- btnClose：右上角 36×36 关闭按钮（悬停变红） ----
        this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
        this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnClose.FlatAppearance.BorderSize = 0;
        this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 53, 69);
        this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnClose.Image = Auditai.UI.Controls.Properties.Resources.关闭;
        this.btnClose.Location = new System.Drawing.Point(620 - 36 - 6, 4);
        this.btnClose.Name = "btnClose";
        this.btnClose.Size = new System.Drawing.Size(32, 32);
        this.btnClose.TabIndex = 1;
        this.btnClose.UseVisualStyleBackColor = true;
        this.btnClose.Click += new System.EventHandler(btnClose_Click);

        // ================== pnlCard 白色圆角卡片容器 ==================
        this.pnlCard.BackColor = System.Drawing.Color.White;
        this.pnlCard.Controls.Add(this.pnlContent);
        this.pnlCard.Name = "pnlCard";
        this.pnlCard.TabIndex = 1;

        // ================== pnlContent 图标 + 文字区 ==================
        this.pnlContent.BackColor = System.Drawing.Color.White;
        this.pnlContent.Controls.Add(this.picIcon);
        this.pnlContent.Controls.Add(this.lblNotice);
        this.pnlContent.Controls.Add(this.txtNotice);
        this.pnlContent.Name = "pnlContent";
        this.pnlContent.TabIndex = 0;

        // ---- picIcon：32×32 小图标（居中缩放） ----
        this.picIcon.BackColor = System.Drawing.Color.Transparent;
        this.picIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
        this.picIcon.Name = "picIcon";
        this.picIcon.TabIndex = 0;
        this.picIcon.TabStop = false;

        // ---- lblNotice：短消息文字（高对比度） ----
        this.lblNotice.BackColor = System.Drawing.Color.White;
        this.lblNotice.ForeColor = TextPrimary;
        this.lblNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.lblNotice.Font = new System.Drawing.Font("Noto Sans SC", 10.5f,
            System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
        this.lblNotice.Name = "lblNotice";
        this.lblNotice.TabIndex = 1;
        this.lblNotice.Tag = null;
        this.lblNotice.TextDetached = true;
        this.lblNotice.TextAlign = System.Drawing.ContentAlignment.TopLeft;

        // ---- txtNotice：长消息滚动框 ----
        this.txtNotice.BackColor = System.Drawing.Color.White;
        this.txtNotice.ForeColor = TextPrimary;
        this.txtNotice.BorderColor = LineColorDefault;
        this.txtNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.txtNotice.Font = new System.Drawing.Font("Noto Sans SC", 10.5f,
            System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
        this.txtNotice.Multiline = true;
        this.txtNotice.Name = "txtNotice";
        this.txtNotice.ReadOnly = true;
        this.txtNotice.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtNotice.TabIndex = 0;
        this.txtNotice.Tag = null;
        this.txtNotice.TextDetached = true;
        this.txtNotice.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Top;

        // ================== 窗体整体 ==================
        base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.Color.FromArgb(245, 249, 252);
        base.ClientSize = new System.Drawing.Size(620, 200);
        base.Controls.Add(this.pnlCard);
        base.Controls.Add(this.pnlTitleBar);
        base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        base.MaximizeBox = false;
        base.MinimizeBox = false;
        this.MinimumSize = MinDialogSize;
        base.Name = "MessageShowBox";
        base.ShowInTaskbar = false;
        base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "MessageShowBox";
        base.TopMost = true;
        base.Paint += new System.Windows.Forms.PaintEventHandler(MessageShowBox_Paint);
        base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(MessageShowBox_FormClosing);

        this.pnlTitleBar.ResumeLayout(false);
        this.pnlTitleBar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
        this.pnlCard.ResumeLayout(false);
        this.pnlContent.ResumeLayout(false);
        this.pnlContent.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.picIcon)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.lblNotice)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.txtNotice)).EndInit();
        base.ResumeLayout(false);
    }
}
