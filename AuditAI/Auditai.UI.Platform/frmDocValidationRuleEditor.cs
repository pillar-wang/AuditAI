﻿﻿﻿﻿﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Auditai.Model;

namespace Auditai.UI.Platform;

/// <summary>
/// 文档校验规则编辑对话框（选区转域时使用）。
/// 用户指定运算符、右表达式和说明，确定后通过 GetRuleJson() 返回规则 JSON。
/// </summary>
public class frmDocValidationRuleEditor : Form
{
	private TextBox txtSelectedText;
	private TextBox txtLeftValue;
	private ComboBox cboOperator;
	private TextBox txtRightExpr;
	private TextBox txtNote;
	private Button btnSelectCell;
	private Button btnOK;
	private Button btnCancel;

	private static readonly string[] Operators = { "=", ">", ">=", "<", "<=", "<>" };

	public string LeftValue
	{
		get { return txtLeftValue.Text; }
		set { txtLeftValue.Text = value ?? string.Empty; }
	}

	public string SelectedText
	{
		get { return txtSelectedText.Text; }
		set { txtSelectedText.Text = value ?? string.Empty; }
	}

	/// <summary>
	/// 获取右表达式输入。
	/// </summary>
	public string RightExpression
	{
		get { return txtRightExpr.Text; }
	}

	/// <summary>
	/// 获取规则说明。
	/// </summary>
	public string Note
	{
		get { return txtNote.Text; }
	}

	/// <summary>
	/// 获取运算符（返回 ValidationOperator 枚举值，Code 与 ValidationOperator 对齐：=→0, >→1, >=→2, <→3, <=→4, <>→5）。
	/// </summary>
	public ValidationOperator GetOperator()
	{
		int code = GetOperatorCode(cboOperator.SelectedItem as string);
		return ValidationOperator.FromCode(code);
	}

	public frmDocValidationRuleEditor()
	{
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		this.Text = "校验规则编辑";
		this.Size = new Size(420, 360);
		this.StartPosition = FormStartPosition.CenterParent;
		this.FormBorderStyle = FormBorderStyle.FixedDialog;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.HelpButton = false;

		int labelX = 12;
		int controlX = 100;
		int labelWidth = 80;
		int controlWidth = 290;
		int y = 12;
		int labelH = 20;
		int controlH = 24;
		int gap = 6;

		// 选区原文
		var lblSelectedText = new Label
		{
			Text = "选区原文:",
			Location = new Point(labelX, y + 3),
			Size = new Size(labelWidth, labelH),
			AutoSize = false
		};
		this.Controls.Add(lblSelectedText);

		txtSelectedText = new TextBox
		{
			Location = new Point(controlX, y),
			Size = new Size(controlWidth, controlH),
			ReadOnly = true,
			BackColor = SystemColors.Control
		};
		this.Controls.Add(txtSelectedText);
		y += controlH + gap;

		// 左值
		var lblLeftValue = new Label
		{
			Text = "左值:",
			Location = new Point(labelX, y + 3),
			Size = new Size(labelWidth, labelH),
			AutoSize = false
		};
		this.Controls.Add(lblLeftValue);

		txtLeftValue = new TextBox
		{
			Location = new Point(controlX, y),
			Size = new Size(controlWidth, controlH),
			ReadOnly = true,
			BackColor = SystemColors.Control
		};
		this.Controls.Add(txtLeftValue);
		y += controlH + gap;

		// 运算符
		var lblOperator = new Label
		{
			Text = "运算符:",
			Location = new Point(labelX, y + 3),
			Size = new Size(labelWidth, labelH),
			AutoSize = false
		};
		this.Controls.Add(lblOperator);

		cboOperator = new ComboBox
		{
			Location = new Point(controlX, y),
			Size = new Size(80, controlH),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		cboOperator.Items.AddRange(Operators);
		cboOperator.SelectedIndex = 0;
		this.Controls.Add(cboOperator);
		y += controlH + gap;

		// 右表达式
		var lblRightExpr = new Label
		{
			Text = "右表达式:",
			Location = new Point(labelX, y + 3),
			Size = new Size(labelWidth, labelH),
			AutoSize = false
		};
		this.Controls.Add(lblRightExpr);

		txtRightExpr = new TextBox
		{
			Location = new Point(controlX, y),
			Size = new Size(controlWidth - 90, controlH)
		};
		this.Controls.Add(txtRightExpr);

		btnSelectCell = new Button
		{
			Text = "选择单元格…",
			Location = new Point(controlX + controlWidth - 80, y - 1),
			Size = new Size(82, controlH + 2),
			Enabled = false  // 暂留空实现，后续完善
		};
		this.Controls.Add(btnSelectCell);
		y += controlH + gap;

		// 说明
		var lblNote = new Label
		{
			Text = "说明:",
			Location = new Point(labelX, y + 3),
			Size = new Size(labelWidth, labelH),
			AutoSize = false
		};
		this.Controls.Add(lblNote);

		txtNote = new TextBox
		{
			Location = new Point(controlX, y),
			Size = new Size(controlWidth, controlH)
		};
		this.Controls.Add(txtNote);
		y += controlH + gap * 2;

		// 按钮区
		btnOK = new Button
		{
			Text = "确定",
			DialogResult = DialogResult.OK,
			Location = new Point(controlX + controlWidth - 170, y),
			Size = new Size(80, 26)
		};
		this.Controls.Add(btnOK);

		btnCancel = new Button
		{
			Text = "取消",
			DialogResult = DialogResult.Cancel,
			Location = new Point(controlX + controlWidth - 80, y),
			Size = new Size(80, 26)
		};
		this.Controls.Add(btnCancel);

		this.AcceptButton = btnOK;
		this.CancelButton = btnCancel;
	}

	/// <summary>
	/// 返回 JSON 格式的规则字符串。
	/// 格式：{"operator":0,"rightExpr":"表1.A1","note":"营业收入核对"}
	/// 运算符编码与 ValidationOperator.Code 对齐：=→0, >→1, >=→2, <→3, <=→4, <>→5
	/// </summary>
	public string GetRuleJson()
	{
		int opCode = GetOperatorCode(cboOperator.SelectedItem as string);
		string rightExpr = EscapeJson(txtRightExpr.Text);
		string note = EscapeJson(txtNote.Text);
		return "{\"operator\":" + opCode + ",\"rightExpr\":\"" + rightExpr + "\",\"note\":\"" + note + "\"}";
	}

	private static int GetOperatorCode(string op)
	{
		switch (op)
		{
			case "=": return 0;
			case ">": return 1;
			case ">=": return 2;
			case "<": return 3;
			case "<=": return 4;
			case "<>": return 5;
			default: return 0;
		}
	}

	private static string EscapeJson(string s)
	{
		if (string.IsNullOrEmpty(s)) return string.Empty;
		return s
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("\r", "\\r")
			.Replace("\n", "\\n")
			.Replace("\t", "\\t");
	}
}
