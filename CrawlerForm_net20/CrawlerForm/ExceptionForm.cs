using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CrawlerForm;

public class ExceptionForm : Form
{
	private string _message;

	private IContainer components = null;

	private TextBox txtMessage;

	private Button btnCopy;

	private Button btnClose;

	public ExceptionForm()
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	public DialogResult ShowDialog(string message)
	{
		txtMessage.Text = message;
		_message = message;
		return ShowDialog();
	}

	private void btnCopy_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(_message?.Trim()))
		{
			return;
		}
		try
		{
			Clipboard.SetText(_message);
		}
		catch (ExternalException)
		{
		}
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
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
		this.txtMessage = new System.Windows.Forms.TextBox();
		this.btnCopy = new System.Windows.Forms.Button();
		this.btnClose = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtMessage.Dock = System.Windows.Forms.DockStyle.Top;
		this.txtMessage.Location = new System.Drawing.Point(0, 0);
		this.txtMessage.Multiline = true;
		this.txtMessage.Name = "txtMessage";
		this.txtMessage.ReadOnly = true;
		this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtMessage.Size = new System.Drawing.Size(286, 342);
		this.txtMessage.TabIndex = 0;
		this.btnCopy.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCopy.Location = new System.Drawing.Point(108, 354);
		this.btnCopy.Name = "btnCopy";
		this.btnCopy.Size = new System.Drawing.Size(75, 23);
		this.btnCopy.TabIndex = 1;
		this.btnCopy.Text = "复制";
		this.btnCopy.UseVisualStyleBackColor = true;
		this.btnCopy.Click += new System.EventHandler(btnCopy_Click);
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.Location = new System.Drawing.Point(199, 354);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(75, 23);
		this.btnClose.TabIndex = 2;
		this.btnClose.Text = "关闭";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(286, 389);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.btnCopy);
		base.Controls.Add(this.txtMessage);
		base.MaximizeBox = false;
		base.Name = "ExceptionForm";
		base.ShowIcon = false;
		this.Text = "异常信息";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
