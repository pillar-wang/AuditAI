using System;
using System.ComponentModel;
using System.Drawing;
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

	private C1PictureBox imgBox;

	public UpdateForm()
	{
		InitializeComponent();
		base.Shown += UpdateForm_Shown;
		Text = "版本更新";
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
	}

	private void UpdateForm_Shown(object sender, EventArgs e)
	{
		base.Icon = Resources.UpdateIcon;
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
		StandardView();
		return base.ShowDialog();
	}

	private void AnchorPosition1(C1SplitterPanel panel, Control control)
	{
		control.Top = 0;
		control.Left = panel.Width - 300;
	}

	private void AnchorPosition2(C1SplitterPanel panel, Control control)
	{
		control.Top = 0;
		control.Left = panel.Width - 200;
	}

	private void AnchorPosition3(C1SplitterPanel panel, Control control)
	{
		control.Top = 0;
		control.Left = panel.Width - 100;
	}

	private void StandardView()
	{
		lblNotice.Font = new Font("微软雅黑", 9f);
		btnDetail.Font = new Font("微软雅黑", 9f);
		btnConfirm.Font = new Font("微软雅黑", 9f);
		btnCancel.Font = new Font("微软雅黑", 9f);
		ctnAll.SplitterWidth = 0;
		pnlImage.BorderWidth = 0;
		pnlContent.BorderWidth = 0;
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
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlButtons);
		this.ctnAll.Panels.Add(this.pnlImage);
		this.ctnAll.Panels.Add(this.pnlContent);
		this.ctnAll.Size = new System.Drawing.Size(504, 181);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnConfirm);
		this.pnlButtons.Controls.Add(this.btnDetail);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 40;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 141);
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size(504, 40);
		this.pnlButtons.TabIndex = 1;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(410, 3);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Location = new System.Drawing.Point(306, 3);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 1;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnDetail.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnDetail.Location = new System.Drawing.Point(201, 3);
		this.btnDetail.Name = "btnDetail";
		this.btnDetail.Size = new System.Drawing.Size(70, 26);
		this.btnDetail.TabIndex = 0;
		this.btnDetail.Text = "更新详情";
		this.btnDetail.UseVisualStyleBackColor = true;
		this.btnDetail.Click += new System.EventHandler(btnDetail_Click);
		this.pnlImage.Controls.Add(this.imgBox);
		this.pnlImage.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlImage.Height = 140;
		this.pnlImage.KeepRelativeSize = false;
		this.pnlImage.Location = new System.Drawing.Point(0, 0);
		this.pnlImage.Name = "pnlImage";
		this.pnlImage.Size = new System.Drawing.Size(92, 140);
		this.pnlImage.SizeRatio = 18.29;
		this.pnlImage.TabIndex = 0;
		this.pnlImage.Width = 92;
		this.imgBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.imgBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.imgBox.Location = new System.Drawing.Point(0, 0);
		this.imgBox.Name = "imgBox";
		this.imgBox.Size = new System.Drawing.Size(92, 140);
		this.imgBox.TabIndex = 0;
		this.imgBox.TabStop = false;
		this.pnlContent.Controls.Add(this.lblNotice);
		this.pnlContent.Height = 140;
		this.pnlContent.Location = new System.Drawing.Point(92, 0);
		this.pnlContent.Name = "pnlContent";
		this.pnlContent.Size = new System.Drawing.Size(412, 140);
		this.pnlContent.TabIndex = 2;
		this.lblNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblNotice.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblNotice.Location = new System.Drawing.Point(0, 0);
		this.lblNotice.Name = "lblNotice";
		this.lblNotice.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
		this.lblNotice.Size = new System.Drawing.Size(412, 140);
		this.lblNotice.TabIndex = 0;
		this.lblNotice.Tag = null;
		this.lblNotice.Text = "c1Label1";
		this.lblNotice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lblNotice.TextDetached = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(504, 181);
		base.Controls.Add(this.ctnAll);
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
