using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Auditai.UI.Controls;

public class frmTileSearch : Form
{
	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1TextBox txtKeyword;

	public Func<string, int> Filter { get; private set; }

	public event EventHandler<string> KeywordChanged;

	public frmTileSearch()
	{
		InitializeComponent();
		Filter = (string str) => FuzzySearch.Filter(str, txtKeyword.Text);
		txtKeyword.TextChanged += InputTextBox_TextChanged;
	}

	private void InputTextBox_TextChanged(object sender, EventArgs e)
	{
		this.KeywordChanged?.Invoke(sender, txtKeyword.Text);
	}

	public void SetText(string text)
	{
		txtKeyword.Text = text;
		txtKeyword.Select(txtKeyword.TextLength, 0);
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
		this.txtKeyword = new C1.Win.C1Input.C1TextBox();
		((System.ComponentModel.ISupportInitialize)this.txtKeyword).BeginInit();
		base.SuspendLayout();
		this.txtKeyword.AutoSize = false;
		this.txtKeyword.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtKeyword.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtKeyword.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtKeyword.Location = new System.Drawing.Point(0, 0);
		this.txtKeyword.Name = "txtKeyword";
		this.txtKeyword.Size = new System.Drawing.Size(318, 32);
		this.txtKeyword.TabIndex = 0;
		this.txtKeyword.Tag = null;
		this.txtKeyword.TextDetached = true;
		this.txtKeyword.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(318, 32);
		base.Controls.Add(this.txtKeyword);
		base.Name = "frmTileSearch";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "搜索";
		((System.ComponentModel.ISupportInitialize)this.txtKeyword).EndInit();
		base.ResumeLayout(false);
	}
}
