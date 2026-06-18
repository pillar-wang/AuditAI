﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Leqisoft.DTO;

namespace Leqisoft.UI.Controls;

internal class InputBoxImpl : C1RibbonForm
{
	private InputFormEnum _inputFormEnum;

	public Func<string, bool> ValidCallback;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1TextBoxEx txtInputLeft;

	private C1Button btnConfirm;

	private C1Button btnCancel;

	private C1Label lblPrompt;

	private C1Label lblwarnNum;

	private C1DateEdit dateInputLeft;

	private C1TextBoxEx txtInputRight;

	private C1DateEdit dateInputRight;

	public object Value { get; set; }

	public bool Valid { get; set; }

	public InputBoxImpl()
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
	}

	public InputBoxImpl(string title = "", string prompt = "", InputFormEnum inputEnum = InputFormEnum.Num)
	{
		InitializeComponent();
		txtInputLeft.Multiline = false;
		txtInputLeft.AcceptsReturn = false;
		txtInputLeft.Width = 128;
		txtInputLeft.Height = 19;
		Text = title;
		lblPrompt.Text = prompt;
		_inputFormEnum = inputEnum;
		switch (_inputFormEnum)
		{
		case InputFormEnum.Date:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = false;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月dd日";
			dateInputRight.CustomFormat = "yyyy年MM月dd日";
			break;
		case InputFormEnum.Num:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			break;
		case InputFormEnum.NumRange:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = true;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			break;
		case InputFormEnum.DateRange:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = true;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月dd日";
			dateInputRight.CustomFormat = "yyyy年MM月dd日";
			break;
		case InputFormEnum.Text:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			break;
		case InputFormEnum.MultiText:
			txtInputLeft.Visible = true;
			txtInputLeft.Multiline = true;
			txtInputLeft.AcceptsReturn = true;
			txtInputLeft.Width = 128;
			txtInputLeft.Height = 54;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			break;
		case InputFormEnum.Time:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			txtInputLeft.DataType = typeof(DateTime);
			txtInputLeft.FormatType = FormatTypeEnum.LongTime;
			txtInputLeft.Value = DateTime.Now;
			break;
		case InputFormEnum.TimeRange:
			txtInputLeft.Visible = true;
			txtInputRight.Visible = true;
			dateInputLeft.Visible = false;
			dateInputRight.Visible = false;
			txtInputLeft.DataType = typeof(DateTime);
			txtInputLeft.FormatType = FormatTypeEnum.LongTime;
			txtInputLeft.Value = DateTime.Now;
			txtInputRight.DataType = typeof(DateTime);
			txtInputRight.FormatType = FormatTypeEnum.LongTime;
			txtInputRight.Value = DateTime.Now;
			break;
		case InputFormEnum.DateYearMonth:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = false;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月";
			dateInputRight.CustomFormat = "yyyy年MM月";
			break;
		case InputFormEnum.DateYearMonthRange:
			txtInputLeft.Visible = false;
			txtInputRight.Visible = false;
			dateInputLeft.Visible = true;
			dateInputRight.Visible = true;
			dateInputLeft.FormatType = FormatTypeEnum.CustomFormat;
			dateInputRight.FormatType = FormatTypeEnum.CustomFormat;
			dateInputLeft.CustomFormat = "yyyy年MM月";
			dateInputRight.CustomFormat = "yyyy年MM月";
			break;
		}
	}

	public void SetInputLeftWidth(int width = 128)
	{
		txtInputLeft.Width = width;
	}

	public void SetInputTextValue(string value)
	{
		txtInputLeft.TextDetached = true;
		txtInputLeft.Value = value;
		txtInputLeft.Text = value;
	}

	public void SetInputTextForPassword()
	{
		txtInputLeft.PasswordChar = '*';
	}

	public void SetInputDateValue(DateTime value)
	{
		dateInputLeft.Value = value;
	}

	public void SetInputDateYearMonthValue(DateYearMonth value)
	{
		dateInputLeft.Value = value.Date;
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		if (ValidCallback != null && !ValidCallback(txtInputLeft.Text.Trim()))
		{
			return;
		}
		switch (_inputFormEnum)
		{
		case InputFormEnum.Num:
		{
			if (decimal.TryParse(txtInputLeft.Text.Trim(), out var result6))
			{
				Value = result6;
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.Text:
		case InputFormEnum.MultiText:
			Value = txtInputLeft.Text;
			Valid = true;
			break;
		case InputFormEnum.Date:
		{
			if (DateTime.TryParse(dateInputLeft.Value?.ToString(), out var result7))
			{
				Value = result7;
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.DateRange:
		{
			object value3 = dateInputLeft.Value;
			object value4 = dateInputRight.Value;
			if (DateTime.TryParse(value3.ToString(), out var result3) && DateTime.TryParse(value4.ToString(), out var result4))
			{
				Value = Tuple.Create(result3, result4);
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.NumRange:
		{
			if (decimal.TryParse(txtInputLeft.Text.Trim(), out var result8) && decimal.TryParse(txtInputRight.Text.Trim(), out var result9))
			{
				Value = Tuple.Create(result8, result9);
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.Time:
			Value = txtInputLeft.Value;
			Valid = true;
			break;
		case InputFormEnum.TimeRange:
			Value = Tuple.Create(txtInputLeft.Value, txtInputRight.Value);
			Valid = true;
			break;
		case InputFormEnum.DateYearMonth:
		{
			if (DateTime.TryParse(dateInputLeft.Value?.ToString(), out var result5))
			{
				Value = result5;
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		case InputFormEnum.DateYearMonthRange:
		{
			object value = dateInputLeft.Value;
			object value2 = dateInputRight.Value;
			if (DateTime.TryParse(value.ToString(), out var result) && DateTime.TryParse(value2.ToString(), out var result2))
			{
				Value = Tuple.Create(result, result2);
				Valid = true;
			}
			else
			{
				Value = null;
				Valid = false;
			}
			break;
		}
		}
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void txtInputLeft_KeyPress(object sender, KeyPressEventArgs e)
	{
	}

	private void txtInputRight_KeyPress(object sender, KeyPressEventArgs e)
	{
	}

	private void dateInputLeft_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			dateInputLeft.Value = dateInputLeft.Text;
		}
	}

	private void dateInputRight_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			dateInputRight.Value = dateInputRight.Text;
		}
	}

	private void txtInputLeft_TextChanged(object sender, EventArgs e)
	{
		if (_inputFormEnum == InputFormEnum.Num || _inputFormEnum == InputFormEnum.NumRange)
		{
			if (decimal.TryParse(txtInputLeft.Text.Trim(), out var _))
			{
				lblwarnNum.Visible = false;
				return;
			}
			lblwarnNum.Visible = true;
			lblwarnNum.Text = "请输入有效数值类型";
		}
	}

	private void txtInputRight_TextChanged(object sender, EventArgs e)
	{
		if (_inputFormEnum == InputFormEnum.Num || _inputFormEnum == InputFormEnum.NumRange)
		{
			if (decimal.TryParse(txtInputRight.Text.Trim(), out var _))
			{
				lblwarnNum.Visible = false;
				return;
			}
			lblwarnNum.Visible = true;
			lblwarnNum.Text = "请输入有效数值类型";
		}
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentObject(this);
		Theme.SetCurrentObject(btnConfirm);
		Theme.SetCurrentObject(btnCancel);
		base.AcceptButton = btnConfirm;
		return base.ShowDialog();
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
		this.txtInputLeft = new Leqisoft.UI.Controls.C1TextBoxEx();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.lblPrompt = new C1.Win.C1Input.C1Label();
		this.lblwarnNum = new C1.Win.C1Input.C1Label();
		this.dateInputLeft = new C1.Win.C1Input.C1DateEdit();
		this.txtInputRight = new Leqisoft.UI.Controls.C1TextBoxEx();
		this.dateInputRight = new C1.Win.C1Input.C1DateEdit();
		((System.ComponentModel.ISupportInitialize)this.txtInputLeft).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblPrompt).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblwarnNum).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputLeft).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtInputRight).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputRight).BeginInit();
		base.SuspendLayout();
		this.txtInputLeft.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtInputLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtInputLeft.Location = new System.Drawing.Point(56, 48);
		this.txtInputLeft.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtInputLeft.Name = "txtInputLeft";
		this.txtInputLeft.Size = new System.Drawing.Size(149, 21);
		this.txtInputLeft.TabIndex = 0;
		this.txtInputLeft.Tag = null;
		this.txtInputLeft.TextChanged += new System.EventHandler(txtInputLeft_TextChanged);
		this.txtInputLeft.KeyPress += new System.Windows.Forms.KeyPressEventHandler(txtInputLeft_KeyPress);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnConfirm.Location = new System.Drawing.Point(241, 84);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 1;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(349, 84);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.lblPrompt.AutoSize = true;
		this.lblPrompt.BackColor = System.Drawing.Color.Transparent;
		this.lblPrompt.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblPrompt.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblPrompt.ForeColor = System.Drawing.Color.Black;
		this.lblPrompt.Location = new System.Drawing.Point(14, 13);
		this.lblPrompt.Name = "lblPrompt";
		this.lblPrompt.Size = new System.Drawing.Size(44, 17);
		this.lblPrompt.TabIndex = 3;
		this.lblPrompt.Tag = null;
		this.lblPrompt.Text = "提示：";
		this.lblPrompt.TextDetached = true;
		this.lblwarnNum.AutoSize = true;
		this.lblwarnNum.BackColor = System.Drawing.Color.Transparent;
		this.lblwarnNum.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblwarnNum.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblwarnNum.ForeColor = System.Drawing.Color.Black;
		this.lblwarnNum.Location = new System.Drawing.Point(241, 13);
		this.lblwarnNum.Name = "lblwarnNum";
		this.lblwarnNum.Size = new System.Drawing.Size(44, 17);
		this.lblwarnNum.TabIndex = 4;
		this.lblwarnNum.Tag = null;
		this.lblwarnNum.Text = "警告：";
		this.lblwarnNum.TextDetached = true;
		this.lblwarnNum.Visible = false;
		this.dateInputLeft.AllowSpinLoop = false;
		this.dateInputLeft.Calendar.DayNameLength = 1;
		this.dateInputLeft.CustomFormat = "yyyy-MM-dd";
		this.dateInputLeft.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputLeft.ImagePadding = new System.Windows.Forms.Padding(0);
		this.dateInputLeft.Location = new System.Drawing.Point(14, 48);
		this.dateInputLeft.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.dateInputLeft.Name = "dateInputLeft";
		this.dateInputLeft.Size = new System.Drawing.Size(191, 21);
		this.dateInputLeft.VisibleButtons = C1.Win.C1Input.DropDownControlButtonFlags.None;
		this.dateInputLeft.DisplayFormat.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputLeft.KeyDown += new System.Windows.Forms.KeyEventHandler(dateInputLeft_KeyDown);
		this.txtInputRight.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.txtInputRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtInputRight.Location = new System.Drawing.Point(233, 48);
		this.txtInputRight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtInputRight.Name = "txtInputRight";
		this.txtInputRight.Size = new System.Drawing.Size(149, 21);
		this.txtInputRight.TabIndex = 6;
		this.txtInputRight.Tag = null;
		this.txtInputRight.TextChanged += new System.EventHandler(txtInputRight_TextChanged);
		this.txtInputRight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(txtInputRight_KeyPress);
		this.dateInputRight.AllowSpinLoop = false;
		this.dateInputRight.Calendar.DayNameLength = 1;
		this.dateInputRight.CustomFormat = "yyyy-MM-dd";
		this.dateInputRight.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputRight.ImagePadding = new System.Windows.Forms.Padding(0);
		this.dateInputRight.Location = new System.Drawing.Point(233, 48);
		this.dateInputRight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.dateInputRight.Name = "dateInputRight";
		this.dateInputRight.Size = new System.Drawing.Size(216, 21);
		this.dateInputRight.VisibleButtons = C1.Win.C1Input.DropDownControlButtonFlags.None;
		this.dateInputRight.DisplayFormat.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
		this.dateInputRight.KeyDown += new System.Windows.Forms.KeyEventHandler(dateInputRight_KeyDown);
		base.AcceptButton = this.btnConfirm;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(463, 123);
		base.Controls.Add(this.dateInputRight);
		base.Controls.Add(this.txtInputRight);
		base.Controls.Add(this.dateInputLeft);
		base.Controls.Add(this.lblwarnNum);
		base.Controls.Add(this.lblPrompt);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnConfirm);
		base.Controls.Add(this.txtInputLeft);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "InputBoxImpl";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Custom;
		((System.ComponentModel.ISupportInitialize)this.txtInputLeft).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblPrompt).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblwarnNum).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputLeft).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtInputRight).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dateInputRight).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
