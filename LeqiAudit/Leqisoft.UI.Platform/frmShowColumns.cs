using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class frmShowColumns : C1RibbonForm
{
	private IContainer components;

	private C1SplitContainer ctn;

	private C1SplitterPanel c1SplitterPanel1;

	private C1SplitterPanel c1SplitterPanel2;

	internal C1Button btnCancel;

	internal C1Button btnOk;

	internal CheckedListBox _clb;

	public frmShowColumns()
	{
		InitializeComponent();
	}

	private void frmShowColumns_Shown(object sender, EventArgs e)
	{
		Leqisoft.UI.Controls.Theme.SetCurrentTree(this);
		ctn.SplitterWidth = 0;
		c1SplitterPanel1.BorderWidth = 0;
		c1SplitterPanel2.BorderWidth = 0;
	}

	private void frmShowColumns_Load(object sender, EventArgs e)
	{
		_clb.DisplayMember = "CaptionDisplay";
	}

	private void btnOk_Click(object sender, EventArgs e)
	{
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
		this.ctn = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.c1SplitterPanel1 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnOk = new C1.Win.C1Input.C1Button();
		this.c1SplitterPanel2 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._clb = new System.Windows.Forms.CheckedListBox();
		((System.ComponentModel.ISupportInitialize)this.ctn).BeginInit();
		this.ctn.SuspendLayout();
		this.c1SplitterPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).BeginInit();
		this.c1SplitterPanel2.SuspendLayout();
		base.SuspendLayout();
		this.ctn.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctn.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctn.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctn.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctn.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctn.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctn.HeaderHeight = 27;
		this.ctn.Location = new System.Drawing.Point(0, 0);
		this.ctn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctn.Name = "ctn";
		this.ctn.Panels.Add(this.c1SplitterPanel1);
		this.ctn.Panels.Add(this.c1SplitterPanel2);
		this.ctn.Size = new System.Drawing.Size(288, 293);
		this.ctn.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctn.SplitterWidth = 0;
		this.ctn.TabIndex = 0;
		this.ctn.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.c1SplitterPanel1.Controls.Add(this.btnCancel);
		this.c1SplitterPanel1.Controls.Add(this.btnOk);
		this.c1SplitterPanel1.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.c1SplitterPanel1.Height = 52;
		this.c1SplitterPanel1.KeepRelativeSize = false;
		this.c1SplitterPanel1.Location = new System.Drawing.Point(0, 241);
		this.c1SplitterPanel1.MinHeight = 52;
		this.c1SplitterPanel1.MinWidth = 52;
		this.c1SplitterPanel1.Name = "c1SplitterPanel1";
		this.c1SplitterPanel1.Resizable = false;
		this.c1SplitterPanel1.Size = new System.Drawing.Size(288, 52);
		this.c1SplitterPanel1.SizeRatio = 8.176;
		this.c1SplitterPanel1.TabIndex = 0;
		this.c1SplitterPanel1.Width = 288;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(200, 14);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOk.Location = new System.Drawing.Point(109, 14);
		this.btnOk.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(70, 26);
		this.btnOk.TabIndex = 0;
		this.btnOk.Text = "确定";
		this.btnOk.UseVisualStyleBackColor = true;
		this.btnOk.Click += new System.EventHandler(btnOk_Click);
		this.c1SplitterPanel2.Controls.Add(this._clb);
		this.c1SplitterPanel2.Height = 240;
		this.c1SplitterPanel2.Location = new System.Drawing.Point(0, 0);
		this.c1SplitterPanel2.MinHeight = 52;
		this.c1SplitterPanel2.MinWidth = 52;
		this.c1SplitterPanel2.Name = "c1SplitterPanel2";
		this.c1SplitterPanel2.Resizable = false;
		this.c1SplitterPanel2.Size = new System.Drawing.Size(288, 240);
		this.c1SplitterPanel2.SizeRatio = 100.0;
		this.c1SplitterPanel2.TabIndex = 1;
		this.c1SplitterPanel2.Width = 288;
		this._clb.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this._clb.CheckOnClick = true;
		this._clb.Dock = System.Windows.Forms.DockStyle.Fill;
		this._clb.FormattingEnabled = true;
		this._clb.IntegralHeight = false;
		this._clb.Location = new System.Drawing.Point(0, 0);
		this._clb.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
		this._clb.Name = "_clb";
		this._clb.Size = new System.Drawing.Size(288, 240);
		this._clb.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(288, 293);
		base.Controls.Add(this.ctn);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmShowColumns";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "寻回列";
		base.Load += new System.EventHandler(frmShowColumns_Load);
		base.Shown += new System.EventHandler(frmShowColumns_Shown);
		((System.ComponentModel.ISupportInitialize)this.ctn).EndInit();
		this.ctn.ResumeLayout(false);
		this.c1SplitterPanel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnOk).EndInit();
		this.c1SplitterPanel2.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
