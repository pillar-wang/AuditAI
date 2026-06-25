using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Resources = global::CrawlerForm.Properties.Resources;

namespace CrawlerForm;

public class MessageBox : Form
{
	private IContainer components = null;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlButton;

	private C1SplitterPanel pnlMessage;

	private C1Button btnMessage;

	private C1Button btnCertain;

	private C1PictureBox picWarn;

	private C1TextBox txtMessage;

	private C1TextBox txtNotice;

	public Bitmap Picture { get; set; }

	public MessageBox()
	{
		InitializeComponent();
		txtNotice.BackColor = pnlButton.BackColor;
		txtNotice.BorderStyle = BorderStyle.None;
		txtNotice.VerticalAlign = VerticalAlignEnum.Middle;
		txtMessage.ReadOnly = true;
		txtMessage.ScrollBars = ScrollBars.Vertical;
		pnlButton.KeepRelativeSize = false;
		pnlMessage.MinHeight = 0;
		base.Height -= pnlMessage.Height;
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	private void btnMessage_Click(object sender, EventArgs e)
	{
		base.Height = 400;
	}

	public DialogResult ShowDialog(string notice, string message)
	{
		if (Picture != null)
		{
			picWarn.Image = Picture;
		}
		txtNotice.Text = notice;
		txtMessage.Text = message;
		return ShowDialog();
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnCertain_Click(object sender, EventArgs e)
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
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButton = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtNotice = new C1.Win.C1Input.C1TextBox();
		this.btnMessage = new C1.Win.C1Input.C1Button();
		this.btnCertain = new C1.Win.C1Input.C1Button();
		this.picWarn = new C1.Win.C1Input.C1PictureBox();
		this.pnlMessage = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtMessage = new C1.Win.C1Input.C1TextBox();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlButton.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtNotice).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnMessage).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picWarn).BeginInit();
		this.pnlMessage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtMessage).BeginInit();
		base.SuspendLayout();
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
		this.ctnAll.Panels.Add(this.pnlMessage);
		this.ctnAll.Size = new System.Drawing.Size(431, 413);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.pnlButton.Controls.Add(this.txtNotice);
		this.pnlButton.Controls.Add(this.btnMessage);
		this.pnlButton.Controls.Add(this.btnCertain);
		this.pnlButton.Controls.Add(this.picWarn);
		this.pnlButton.Height = 200;
		this.pnlButton.KeepRelativeSize = false;
		this.pnlButton.Location = new System.Drawing.Point(0, 0);
		this.pnlButton.MinHeight = 200;
		this.pnlButton.MinWidth = 52;
		this.pnlButton.Name = "pnlButton";
		this.pnlButton.Size = new System.Drawing.Size(431, 200);
		this.pnlButton.SizeRatio = 48.426;
		this.pnlButton.TabIndex = 0;
		this.pnlButton.Width = 431;
		this.txtNotice.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.txtNotice.Location = new System.Drawing.Point(122, 21);
		this.txtNotice.Multiline = true;
		this.txtNotice.Name = "txtNotice";
		this.txtNotice.ReadOnly = true;
		this.txtNotice.Size = new System.Drawing.Size(287, 105);
		this.txtNotice.TabIndex = 4;
		this.txtNotice.Tag = null;
		this.txtNotice.TextDetached = true;
		this.txtNotice.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.btnMessage.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnMessage.Location = new System.Drawing.Point(271, 148);
		this.btnMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnMessage.Name = "btnMessage";
		this.btnMessage.Size = new System.Drawing.Size(70, 26);
		this.btnMessage.TabIndex = 3;
		this.btnMessage.Text = "详细信息";
		this.btnMessage.UseVisualStyleBackColor = true;
		this.btnMessage.Click += new System.EventHandler(btnMessage_Click);
		this.btnCertain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnCertain.Location = new System.Drawing.Point(82, 148);
		this.btnCertain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCertain.Name = "btnCertain";
		this.btnCertain.Size = new System.Drawing.Size(70, 26);
		this.btnCertain.TabIndex = 2;
		this.btnCertain.Text = "确定";
		this.btnCertain.UseVisualStyleBackColor = true;
		this.btnCertain.Click += new System.EventHandler(btnCertain_Click);
		this.picWarn.BackColor = System.Drawing.Color.Transparent;
		this.picWarn.Image = Resources.警告72;
		this.picWarn.Location = new System.Drawing.Point(27, 36);
		this.picWarn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.picWarn.Name = "picWarn";
		this.picWarn.Size = new System.Drawing.Size(74, 71);
		this.picWarn.TabIndex = 0;
		this.picWarn.TabStop = false;
		this.pnlMessage.Controls.Add(this.txtMessage);
		this.pnlMessage.Height = 213;
		this.pnlMessage.Location = new System.Drawing.Point(0, 200);
		this.pnlMessage.MinHeight = 0;
		this.pnlMessage.MinWidth = 52;
		this.pnlMessage.Name = "pnlMessage";
		this.pnlMessage.Size = new System.Drawing.Size(431, 213);
		this.pnlMessage.TabIndex = 1;
		this.pnlMessage.Width = 431;
		this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtMessage.Location = new System.Drawing.Point(0, 0);
		this.txtMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtMessage.Multiline = true;
		this.txtMessage.Name = "txtMessage";
		this.txtMessage.Size = new System.Drawing.Size(431, 213);
		this.txtMessage.TabIndex = 0;
		this.txtMessage.Tag = null;
		this.txtMessage.TextDetached = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(431, 413);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "MessageBox";
		this.Text = "MessageBox";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlButton.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txtNotice).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnMessage).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picWarn).EndInit();
		this.pnlMessage.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txtMessage).EndInit();
		base.ResumeLayout(false);
	}
}
