using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class PolicyForm : C1RibbonForm
{
	private string Title = "AuditAI";

	private readonly string SubTitle = $"版本号：{Assembly.GetEntryAssembly().GetName().Version}";

	private readonly string BottomInfo = "© 2026 AuditAI";

	private readonly Image ImageIcon = Resource1.logo_128_128;

	private readonly string policyurl = "about:blank";

	private readonly string agreementurl = "about:blank";

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1Label lblBottomInfo;

	private C1Label lblSubTitle;

	private C1Label lblUserAgreement;

	private C1Label lblTitle;

	private C1PictureBox picLogo;

	private C1Label lblPolicy;

	public PolicyForm()
	{
		InitializeComponent();
		base.Shown += PolicyForm_Shown;
		lblTitle.Text = Title;
		lblSubTitle.Text = SubTitle;
		lblBottomInfo.Text = BottomInfo;
		picLogo.BackgroundImage = ImageIcon;
		base.StartPosition = FormStartPosition.CenterScreen;
		picLogo.BackgroundImageLayout = ImageLayout.Stretch;
		lblPolicy.ForeColor = Color.Blue;
		lblUserAgreement.ForeColor = Color.Blue;
		Theme.SetCurrentTree(this);
	}

	public void SetTitle(string title)
	{
		Title = title;
		lblTitle.Text = Title;
	}

	private void PolicyForm_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.About);
	}

	private void lblUserAgreement_Click(object sender, EventArgs e)
	{
		Util.ShellExecuteUrl(agreementurl);
	}

	private void lblPolicy_Click(object sender, EventArgs e)
	{
		Util.ShellExecuteUrl(policyurl);
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
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
		this.lblBottomInfo = new C1.Win.C1Input.C1Label();
		this.picLogo = new C1.Win.C1Input.C1PictureBox();
		this.lblSubTitle = new C1.Win.C1Input.C1Label();
		this.lblTitle = new C1.Win.C1Input.C1Label();
		this.lblUserAgreement = new C1.Win.C1Input.C1Label();
		this.lblPolicy = new C1.Win.C1Input.C1Label();
		((System.ComponentModel.ISupportInitialize)this.lblBottomInfo).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picLogo).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblSubTitle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblTitle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserAgreement).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPolicy).BeginInit();
		base.SuspendLayout();
		this.lblBottomInfo.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.lblBottomInfo.BackColor = System.Drawing.Color.Transparent;
		this.lblBottomInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblBottomInfo.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblBottomInfo.ForeColor = System.Drawing.Color.Black;
		this.lblBottomInfo.Location = new System.Drawing.Point(3, 253);
		this.lblBottomInfo.Name = "lblBottomInfo";
		this.lblBottomInfo.Size = new System.Drawing.Size(397, 24);
		this.lblBottomInfo.TabIndex = 5;
		this.lblBottomInfo.Tag = null;
		this.lblBottomInfo.Text = "lblBottomInfo";
		this.lblBottomInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblBottomInfo.TextDetached = true;
		this.picLogo.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.picLogo.Location = new System.Drawing.Point(152, 21);
		this.picLogo.Name = "picLogo";
		this.picLogo.Size = new System.Drawing.Size(100, 100);
		this.picLogo.TabIndex = 0;
		this.picLogo.TabStop = false;
		this.lblSubTitle.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.lblSubTitle.BackColor = System.Drawing.Color.Transparent;
		this.lblSubTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblSubTitle.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSubTitle.ForeColor = System.Drawing.Color.Black;
		this.lblSubTitle.Location = new System.Drawing.Point(3, 177);
		this.lblSubTitle.Name = "lblSubTitle";
		this.lblSubTitle.Size = new System.Drawing.Size(397, 23);
		this.lblSubTitle.TabIndex = 4;
		this.lblSubTitle.Tag = null;
		this.lblSubTitle.Text = "lblSubTitle";
		this.lblSubTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblSubTitle.TextDetached = true;
		this.lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.lblTitle.BackColor = System.Drawing.Color.Transparent;
		this.lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblTitle.ForeColor = System.Drawing.Color.Black;
		this.lblTitle.Location = new System.Drawing.Point(3, 136);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(397, 25);
		this.lblTitle.TabIndex = 1;
		this.lblTitle.Tag = null;
		this.lblTitle.Text = "AuditAI";
		this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblTitle.TextDetached = true;
		this.lblUserAgreement.AutoSize = true;
		this.lblUserAgreement.BackColor = System.Drawing.Color.Transparent;
		this.lblUserAgreement.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblUserAgreement.Cursor = System.Windows.Forms.Cursors.Hand;
		this.lblUserAgreement.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblUserAgreement.ForeColor = System.Drawing.Color.Black;
		this.lblUserAgreement.Location = new System.Drawing.Point(108, 214);
		this.lblUserAgreement.Name = "lblUserAgreement";
		this.lblUserAgreement.Size = new System.Drawing.Size(80, 17);
		this.lblUserAgreement.TabIndex = 2;
		this.lblUserAgreement.Tag = null;
		this.lblUserAgreement.Text = "《用户协议》";
		this.lblUserAgreement.TextDetached = true;
		this.lblUserAgreement.Click += new System.EventHandler(lblUserAgreement_Click);
		this.lblPolicy.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblPolicy.AutoSize = true;
		this.lblPolicy.BackColor = System.Drawing.Color.Transparent;
		this.lblPolicy.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPolicy.Cursor = System.Windows.Forms.Cursors.Hand;
		this.lblPolicy.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPolicy.ForeColor = System.Drawing.Color.Black;
		this.lblPolicy.Location = new System.Drawing.Point(212, 214);
		this.lblPolicy.Name = "lblPolicy";
		this.lblPolicy.Size = new System.Drawing.Size(80, 17);
		this.lblPolicy.TabIndex = 8;
		this.lblPolicy.Tag = null;
		this.lblPolicy.Text = "《隐私政策》";
		this.lblPolicy.TextDetached = true;
		this.lblPolicy.Click += new System.EventHandler(lblPolicy_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BackgroundColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(403, 286);
		base.Controls.Add(this.lblPolicy);
		base.Controls.Add(this.lblBottomInfo);
		base.Controls.Add(this.picLogo);
		base.Controls.Add(this.lblUserAgreement);
		base.Controls.Add(this.lblSubTitle);
		base.Controls.Add(this.lblTitle);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "PolicyForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "关于";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.lblBottomInfo).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picLogo).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblSubTitle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblTitle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblUserAgreement).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPolicy).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
