using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

internal class frmParagraphComment : C1RibbonForm
{
	internal bool _settingText;

	internal bool _changed;

	private IContainer components;

	private C1SplitContainer ctnDock;

	private C1SplitterPanel pnlInputBox;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancel;

	private C1Button btnConfirm;

	private C1DockingTab DockingTab;

	private C1DockingTabPage tabEdit;

	private C1SplitContainer ctnCommentInput;

	private C1SplitterPanel c1SplitterPanel4;

	private C1ContextMenu txbDropInputContextMenu;

	private C1CommandLink c1CommandLink1;

	private C1CommandHolder c1CommandHolder1;

	private C1ContextMenu c1ContextMenu1;

	internal C1TextBoxEx txbCommentInput;

	public frmParagraphComment()
	{
		InitializeComponent();
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
	}

	private void txbCommentInput_TextChanged(object sender, EventArgs e)
	{
		if (!_settingText)
		{
			_changed = true;
		}
	}

	private void frmParagraphComment_Load(object sender, EventArgs e)
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
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
		this.ctnDock = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.pnlInputBox = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.DockingTab = new C1.Win.C1Command.C1DockingTab();
		this.tabEdit = new C1.Win.C1Command.C1DockingTabPage();
		this.ctnCommentInput = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.c1SplitterPanel4 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txbCommentInput = new Auditai.UI.Controls.C1TextBoxEx();
		this.c1ContextMenu1 = new C1.Win.C1Command.C1ContextMenu();
		this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.txbDropInputContextMenu = new C1.Win.C1Command.C1ContextMenu();
		((System.ComponentModel.ISupportInitialize)this.ctnDock).BeginInit();
		this.ctnDock.SuspendLayout();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		this.pnlInputBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.DockingTab).BeginInit();
		this.DockingTab.SuspendLayout();
		this.tabEdit.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ctnCommentInput).BeginInit();
		this.ctnCommentInput.SuspendLayout();
		this.c1SplitterPanel4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txbCommentInput).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		base.SuspendLayout();
		this.ctnDock.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnDock.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnDock.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnDock.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnDock.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnDock.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnDock.HeaderHeight = 27;
		this.ctnDock.Location = new System.Drawing.Point(0, 0);
		this.ctnDock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnDock.Name = "ctnDock";
		this.ctnDock.Panels.Add(this.pnlButtons);
		this.ctnDock.Panels.Add(this.pnlInputBox);
		this.ctnDock.Size = new System.Drawing.Size(592, 419);
		this.ctnDock.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnDock.SplitterWidth = 0;
		this.ctnDock.TabIndex = 1;
		this.ctnDock.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnConfirm);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 40;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 379);
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size(592, 40);
		this.pnlButtons.TabIndex = 2;
		this.pnlButtons.Width = 592;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(499, 7);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Location = new System.Drawing.Point(396, 7);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 0;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.pnlInputBox.Controls.Add(this.DockingTab);
		this.pnlInputBox.Height = 378;
		this.pnlInputBox.Location = new System.Drawing.Point(0, 0);
		this.pnlInputBox.MinHeight = 0;
		this.pnlInputBox.MinWidth = 52;
		this.pnlInputBox.Name = "pnlInputBox";
		this.pnlInputBox.Size = new System.Drawing.Size(592, 378);
		this.pnlInputBox.SizeRatio = 100.0;
		this.pnlInputBox.TabIndex = 1;
		this.pnlInputBox.Width = 592;
		this.DockingTab.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.DockingTab.Controls.Add(this.tabEdit);
		this.DockingTab.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DockingTab.Location = new System.Drawing.Point(0, 0);
		this.DockingTab.Name = "DockingTab";
		this.DockingTab.ShowTabs = false;
		this.DockingTab.Size = new System.Drawing.Size(592, 378);
		this.DockingTab.TabIndex = 1;
		this.DockingTab.TabsShowFocusCues = false;
		this.DockingTab.TabsSpacing = 5;
		this.DockingTab.TabStyle = C1.Win.C1Command.TabStyleEnum.Office2007;
		this.DockingTab.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.DockingTab.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2007Blue;
		this.tabEdit.Controls.Add(this.ctnCommentInput);
		this.tabEdit.Location = new System.Drawing.Point(0, 1);
		this.tabEdit.Name = "tabEdit";
		this.tabEdit.Size = new System.Drawing.Size(592, 377);
		this.tabEdit.TabIndex = 2;
		this.tabEdit.Text = "编辑注释";
		this.ctnCommentInput.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnCommentInput.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnCommentInput.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnCommentInput.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnCommentInput.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnCommentInput.Location = new System.Drawing.Point(0, 0);
		this.ctnCommentInput.Name = "ctnCommentInput";
		this.ctnCommentInput.Panels.Add(this.c1SplitterPanel4);
		this.ctnCommentInput.Size = new System.Drawing.Size(592, 377);
		this.ctnCommentInput.SplitterWidth = 0;
		this.ctnCommentInput.TabIndex = 1;
		this.c1SplitterPanel4.Controls.Add(this.txbCommentInput);
		this.c1SplitterPanel4.Height = 377;
		this.c1SplitterPanel4.Location = new System.Drawing.Point(0, 0);
		this.c1SplitterPanel4.Name = "c1SplitterPanel4";
		this.c1SplitterPanel4.Size = new System.Drawing.Size(592, 377);
		this.c1SplitterPanel4.TabIndex = 1;
		this.txbCommentInput.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txbCommentInput.Location = new System.Drawing.Point(0, 0);
		this.txbCommentInput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txbCommentInput.Multiline = true;
		this.txbCommentInput.Name = "txbCommentInput";
		this.txbCommentInput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txbCommentInput.Size = new System.Drawing.Size(592, 377);
		this.txbCommentInput.TabIndex = 0;
		this.txbCommentInput.Tag = null;
		this.txbCommentInput.TextDetached = true;
		this.txbCommentInput.TextChanged += new System.EventHandler(txbCommentInput_TextChanged);
		this.c1ContextMenu1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[1] { this.c1CommandLink1 });
		this.c1ContextMenu1.Name = "c1ContextMenu1";
		this.c1ContextMenu1.ShortcutText = "";
		this.c1CommandLink1.Text = "新命令";
		this.c1CommandHolder1.Commands.Add(this.c1ContextMenu1);
		this.c1CommandHolder1.Commands.Add(this.txbDropInputContextMenu);
		this.c1CommandHolder1.Owner = this;
		this.txbDropInputContextMenu.Name = "txbDropInputContextMenu";
		this.txbDropInputContextMenu.ShortcutText = "";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(592, 419);
		base.Controls.Add(this.ctnDock);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmParagraphComment";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "段落注释";
		base.Load += new System.EventHandler(frmParagraphComment_Load);
		((System.ComponentModel.ISupportInitialize)this.ctnDock).EndInit();
		this.ctnDock.ResumeLayout(false);
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		this.pnlInputBox.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.DockingTab).EndInit();
		this.DockingTab.ResumeLayout(false);
		this.tabEdit.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ctnCommentInput).EndInit();
		this.ctnCommentInput.ResumeLayout(false);
		this.c1SplitterPanel4.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txbCommentInput).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		base.ResumeLayout(false);
	}
}
