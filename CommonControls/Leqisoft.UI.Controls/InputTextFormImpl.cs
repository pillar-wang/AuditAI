﻿using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Controls;

internal class InputTextFormImpl : C1RibbonForm
{
	internal string text;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	internal C1TextBoxEx txtInput;

	public InputTextFormImpl()
	{
		InitializeComponent();
	}

	private void txtInput_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			text = txtInput.Text;
			Close();
		}
		else if (e.KeyCode == Keys.Escape)
		{
			text = null;
			Close();
		}
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
		this.txtInput = new Leqisoft.UI.Controls.C1TextBoxEx();
		((System.ComponentModel.ISupportInitialize)this.txtInput).BeginInit();
		base.SuspendLayout();
		this.txtInput.AcceptsEscape = false;
		this.txtInput.AutoSize = false;
		this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtInput.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtInput.Location = new System.Drawing.Point(0, 0);
		this.txtInput.Name = "txtInput";
		this.txtInput.Size = new System.Drawing.Size(284, 261);
		this.txtInput.TabIndex = 0;
		this.txtInput.Tag = null;
		this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(txtInput_KeyDown);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(284, 261);
		base.Controls.Add(this.txtInput);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "InputTextFormImpl";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "InputTextBox";
		((System.ComponentModel.ISupportInitialize)this.txtInput).EndInit();
		base.ResumeLayout(false);
	}
}
