﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class LinkForm : C1RibbonForm
{
	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private LinkLabel linkLable;

	private C1PictureBox picIcon;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlButton;

	private C1Button btnCertain;

	private C1SplitterPanel pnlImage;

	private C1SplitterPanel pnlLink;

	private C1SplitterPanel pnlMessage;

	private C1Label lblMessage;

	private string UrlLink { get; set; } = "about:blank";

	#region 按钮尺寸规范
	private const int ButtonWidth = 110;
	private const int ButtonHeight = 34;
	private const int ButtonRightMargin = 50;
	#endregion


	private string UrlText { get; set; } = "访问官方网站";


	private LinkForm()
	{
		InitializeComponent();
		picIcon.BackgroundImage = Resource1.提示;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	public LinkForm(string message, string text, string url, Image image = null)
		: this()
	{
		lblMessage.Text = message;
		UrlText = text;
		linkLable.Text = UrlText;
		UrlLink = url;
		if (image != null)
		{
			picIcon.BackgroundImage = image;
		}
	}

	private void linkLable_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		Util.ShellExecuteUrl(UrlLink);
	}

	private void btnCertain_Click(object sender, EventArgs e)
	{
		Util.ShellExecuteUrl(UrlLink);
		Close();
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentTree(this);
		linkLable.BackColor = lblMessage.BackColor;
		ctnAll.SplitterWidth = 0;
		pnlMessage.BorderWidth = 0;
		pnlButton.BorderWidth = 0;
		pnlImage.BorderWidth = 0;
		pnlLink.BorderWidth = 0;
		AutoSizeByContent();
		EnsureScrollable();
		return base.ShowDialog();
	}

	#region 按内容尺寸自动调整窗体（3 档高度，封顶时消息区域自动滚动）
	/// <summary>图标面板宽度</summary>
	private const int ImagePanelWidth = 156;
	/// <summary>按钮面板高度</summary>
	private const int ButtonPanelHeight = 62;
	/// <summary>链接面板高度</summary>
	private const int LinkPanelHeight = 90;
	/// <summary>最小/中间/最大窗体尺寸（宽度固定，高度 3 档）</summary>
	private static readonly Size MinDialogSize = new Size(666, 243);
	private static readonly Size MidDialogSize = new Size(666, 380);
	private static readonly Size MaxDialogSize = new Size(Screen.PrimaryScreen != null
		? Math.Min(Screen.PrimaryScreen.WorkingArea.Width - 80, 950) : 950,
		Screen.PrimaryScreen != null ? Math.Min(Screen.PrimaryScreen.WorkingArea.Height - 80, 560) : 560);

	/// <summary>按内容计算窗体高度，超限时改用滚动视图</summary>
	private void AutoSizeByContent()
	{
		string message = lblMessage.Text ?? "";
		Font font = new Font("微软雅黑", 10.5f);
		const int dialogWidth = 666;
		int textWidth = dialogWidth - ImagePanelWidth - 26; // 左 Padding 12 + 右 Padding 14

		int textH;
		using (var g = CreateGraphics())
		{
			Size textSize = TextRenderer.MeasureText(g, message, font,
				new Size(textWidth, int.MaxValue),
				TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
			textH = (int)(textSize.Height * 1.15f) + 8;
		}
		int messagePanelH = Math.Max(91, textH);

		int totalHeight = ButtonPanelHeight + LinkPanelHeight + messagePanelH;
		if (totalHeight < MinDialogSize.Height) totalHeight = MinDialogSize.Height;
		else if (totalHeight < MidDialogSize.Height) totalHeight = MidDialogSize.Height;
		else if (totalHeight > MaxDialogSize.Height) totalHeight = MaxDialogSize.Height;

		ctnAll.AutoSizeElement = AutoSizeElement.None;
		base.ClientSize = new Size(dialogWidth, totalHeight);
		ctnAll.Size = new Size(dialogWidth, totalHeight);

		// 重新设定面板高度（按钮+链接 固定，消息区 占剩余）
		pnlButton.Height = ButtonPanelHeight;
		pnlLink.Height = LinkPanelHeight;
		// pnlMessage 高度自动由 SplitContainer 填充（因为 pnlButton 和 pnlLink Docked Bottom）
		// 但如果总高度 < Min，pnlMessage 会被压扁，这里显式设置 MinimumSize
		int msgHeight = totalHeight - ButtonPanelHeight - LinkPanelHeight;
		if (msgHeight > 0)
		{
			pnlMessage.MinHeight = msgHeight;
			pnlMessage.Height = msgHeight;
		}

		// 按钮重定位
		btnCertain.Top = (pnlButton.Height - ButtonHeight) / 2;
		btnCertain.Left = pnlButton.Width - ButtonRightMargin - ButtonWidth;
	}

	/// <summary>如果内容高度超过 Max，则切换 lblMessage 到可滚动的只读 TextBox</summary>
	private void EnsureScrollable()
	{
		if (!_needsScroll)
		{
			_needsScroll = ContentNeedsScroll();
			if (!_needsScroll) return;

			var textBox = new C1TextBoxEx
			{
				Name = "txtMessageScroll",
				BackColor = lblMessage.BackColor,
				BorderStyle = BorderStyle.None,
				Multiline = true,
				ReadOnly = true,
				ScrollBars = ScrollBars.Vertical,
				WordWrap = true,
				Font = new Font("微软雅黑", 10.5f),
				Dock = DockStyle.Fill,
				Text = lblMessage.Text ?? "",
				Padding = new Padding(12, 8, 12, 8)
			};
			textBox.VerticalAlign = VerticalAlignEnum.Top;
			_txtMessageScroll = textBox;
			pnlMessage.Controls.Add(textBox);
			textBox.BringToFront();
			lblMessage.Visible = false;
		}
	}

	private bool _needsScroll;
	private C1TextBoxEx _txtMessageScroll;

	private bool ContentNeedsScroll()
	{
		string message = lblMessage.Text ?? "";
		Font font = new Font("微软雅黑", 10.5f);
		int textWidth = MaxDialogSize.Width - ImagePanelWidth - 26;
		int maxMsgH = MaxDialogSize.Height - ButtonPanelHeight - LinkPanelHeight - 16;
		using (var g = CreateGraphics())
		{
			Size textSize = TextRenderer.MeasureText(g, message, font,
				new Size(textWidth, int.MaxValue),
				TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix);
			return (int)(textSize.Height * 1.15f) > maxMsgH;
		}
	}
	#endregion


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
		this.linkLable = new System.Windows.Forms.LinkLabel();
		this.picIcon = new C1.Win.C1Input.C1PictureBox();
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButton = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCertain = new C1.Win.C1Input.C1Button();
		this.pnlImage = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlLink = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlMessage = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblMessage = new C1.Win.C1Input.C1Label();
		((System.ComponentModel.ISupportInitialize)this.picIcon).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlButton.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).BeginInit();
		this.pnlImage.SuspendLayout();
		this.pnlLink.SuspendLayout();
		this.pnlMessage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblMessage).BeginInit();
		base.SuspendLayout();
		this.linkLable.Cursor = System.Windows.Forms.Cursors.Hand;
		this.linkLable.Dock = System.Windows.Forms.DockStyle.Fill;
		this.linkLable.Location = new System.Drawing.Point(0, 0);
		this.linkLable.Name = "linkLable";
		this.linkLable.Padding = new System.Windows.Forms.Padding(16, 4, 16, 18);
		this.linkLable.Size = new System.Drawing.Size(510, 90);
		this.linkLable.TabIndex = 0;
		this.linkLable.TabStop = true;
		this.linkLable.Text = "到官网升级";
		this.linkLable.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLable_LinkClicked);
		this.picIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.picIcon.Dock = System.Windows.Forms.DockStyle.Fill;
		this.picIcon.Location = new System.Drawing.Point(0, 0);
		this.picIcon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.picIcon.Name = "picIcon";
		this.picIcon.Size = new System.Drawing.Size(156, 181);
		this.picIcon.TabIndex = 1;
		this.picIcon.TabStop = false;
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.HeaderHeight = 27;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlButton);
		this.ctnAll.Panels.Add(this.pnlImage);
		this.ctnAll.Panels.Add(this.pnlLink);
		this.ctnAll.Panels.Add(this.pnlMessage);
		this.ctnAll.Size = new System.Drawing.Size(666, 243);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 2;
		this.pnlButton.Controls.Add(this.btnCertain);
		this.pnlButton.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButton.Height = 62;
		this.pnlButton.KeepRelativeSize = false;
		this.pnlButton.Location = new System.Drawing.Point(0, 181);
		this.pnlButton.MinWidth = 52;
		this.pnlButton.Name = "pnlButton";
		this.pnlButton.Resizable = false;
		this.pnlButton.Size = new System.Drawing.Size(666, 62);
		this.pnlButton.SizeRatio = 25.806;
		this.pnlButton.TabIndex = 2;
		this.pnlButton.Width = 666;
		this.btnCertain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCertain.Location = new System.Drawing.Point(666 - ButtonRightMargin - ButtonWidth, 16);
		this.btnCertain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCertain.Name = "btnCertain";
		this.btnCertain.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnCertain.TabIndex = 0;
		this.btnCertain.Text = "确定";
		this.btnCertain.UseVisualStyleBackColor = true;
		this.btnCertain.Click += new System.EventHandler(btnCertain_Click);
		this.pnlImage.Controls.Add(this.picIcon);
		this.pnlImage.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlImage.Height = 181;
		this.pnlImage.KeepRelativeSize = false;
		this.pnlImage.Location = new System.Drawing.Point(0, 0);
		this.pnlImage.MinHeight = 52;
		this.pnlImage.MinWidth = 52;
		this.pnlImage.Name = "pnlImage";
		this.pnlImage.Size = new System.Drawing.Size(156, 181);
		this.pnlImage.SizeRatio = 23.438;
		this.pnlImage.TabIndex = 0;
		this.pnlImage.Width = 156;
		this.pnlLink.Controls.Add(this.linkLable);
		this.pnlLink.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlLink.Height = 90;
		this.pnlLink.Location = new System.Drawing.Point(156, 91);
		this.pnlLink.MinHeight = 52;
		this.pnlLink.MinWidth = 52;
		this.pnlLink.Name = "pnlLink";
		this.pnlLink.Size = new System.Drawing.Size(510, 90);
		this.pnlLink.TabIndex = 1;
		this.pnlLink.Width = 510;
		this.pnlMessage.Controls.Add(this.lblMessage);
		this.pnlMessage.Height = 91;
		this.pnlMessage.Location = new System.Drawing.Point(156, 0);
		this.pnlMessage.Name = "pnlMessage";
		this.pnlMessage.Size = new System.Drawing.Size(510, 91);
		this.pnlMessage.SizeRatio = 100.0;
		this.pnlMessage.TabIndex = 3;
		this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblMessage.Location = new System.Drawing.Point(0, 0);
		this.lblMessage.Name = "lblMessage";
		this.lblMessage.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
		this.lblMessage.Size = new System.Drawing.Size(510, 91);
		this.lblMessage.TabIndex = 0;
		this.lblMessage.Tag = null;
		this.lblMessage.Text = "您不是组织版管理员账号，无成员管理的权限！";
		this.lblMessage.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		this.lblMessage.TextDetached = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(666, 243);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "LinkForm";
		base.ShowInTaskbar = false;
		this.Text = "账户升级";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.picIcon).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlButton.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCertain).EndInit();
		this.pnlImage.ResumeLayout(false);
		this.pnlLink.ResumeLayout(false);
		this.pnlMessage.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.lblMessage).EndInit();
		base.ResumeLayout(false);
	}
}
