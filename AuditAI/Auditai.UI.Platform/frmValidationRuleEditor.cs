using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Auditai.DTO;

namespace Auditai.UI.Platform;

/// <summary>
/// 稽核检查页的校验规则编辑对话框：左表达式 + 运算符 + 右表达式 + 说明 + 归属表格。
/// 文档域规则（DocumentFieldId 非 0）不允许修改归属表格。
/// </summary>
public class frmValidationRuleEditor : Form
{
	private readonly ValidationFormula _rule;

	private readonly Dictionary<long, string> _tableNames;

	private readonly bool _allowChangeOwner;

	private ComboBox _cboTable;

	private TextBox _txtLeft;

	private ComboBox _cboOperator;

	private TextBox _txtRight;

	private TextBox _txtNote;

	private Button _btnOk;

	private Button _btnCancel;

	public frmValidationRuleEditor(ValidationFormula rule, Dictionary<long, string> tableNames, bool allowChangeOwner)
	{
		_rule = rule;
		_tableNames = tableNames ?? new Dictionary<long, string>();
		_allowChangeOwner = allowChangeOwner;
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		base.Font = new Font("微软雅黑", 9f);
		Text = "稽核规则编辑";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		ClientSize = new Size(430, 282);

		bool isDocRule = !_rule.DocumentFieldId.IsZero();

		Label lblTable = new Label
		{
			Text = "归属表格：",
			Location = new Point(18, 22),
			AutoSize = true
		};
		_cboTable = new ComboBox
		{
			Location = new Point(100, 18),
			Size = new Size(290, 26),
			DropDownStyle = ComboBoxStyle.DropDownList,
			Enabled = _allowChangeOwner && !isDocRule
		};
		foreach (KeyValuePair<long, string> kv in _tableNames.OrderBy((KeyValuePair<long, string> i) => i.Value))
		{
			_cboTable.Items.Add(kv.Value);
			if (!_rule.TableId.IsZero() && kv.Key == _rule.TableId.Value)
			{
				_cboTable.SelectedIndex = _cboTable.Items.Count - 1;
			}
		}
		if (isDocRule)
		{
			_cboTable.Items.Insert(0, "文档域");
			_cboTable.SelectedIndex = 0;
		}

		Label lblLeft = new Label
		{
			Text = "左表达式：",
			Location = new Point(18, 62),
			AutoSize = true
		};
		_txtLeft = new TextBox
		{
			Location = new Point(100, 58),
			Size = new Size(290, 26)
		};

		Label lblOp = new Label
		{
			Text = "运算符：",
			Location = new Point(18, 102),
			AutoSize = true
		};
		_cboOperator = new ComboBox
		{
			Location = new Point(100, 98),
			Size = new Size(100, 26),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		_cboOperator.Items.AddRange(new object[6] { "=", ">", ">=", "<", "<=", "<>" });
		_cboOperator.SelectedIndex = Math.Min(Math.Max(_rule.Operator, 0), 5);

		Label lblRight = new Label
		{
			Text = "右表达式：",
			Location = new Point(18, 142),
			AutoSize = true
		};
		_txtRight = new TextBox
		{
			Location = new Point(100, 138),
			Size = new Size(290, 26)
		};

		Label lblNote = new Label
		{
			Text = "说明：",
			Location = new Point(18, 182),
			AutoSize = true
		};
		_txtNote = new TextBox
		{
			Location = new Point(100, 178),
			Size = new Size(290, 26)
		};

		_btnOk = new Button
		{
			Text = "确定",
			// 取消贴右（右缘距窗体 50），确定在左侧间距 12；原实现两者 x 计算颠倒导致取消溢出窗体右侧 60px
			Location = new Point(ClientSize.Width - 50 - 110 - 12 - 110, ClientSize.Height - 40 - 12),
			Size = new Size(110, 30)
		};
		_btnOk.Click += delegate
		{
			OnOk();
		};
		_btnCancel = new Button
		{
			Text = "取消",
			Location = new Point(ClientSize.Width - 50 - 110, ClientSize.Height - 40 - 12),
			Size = new Size(110, 30)
		};
		_btnCancel.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		};

		base.Controls.Add(lblTable);
		base.Controls.Add(_cboTable);
		base.Controls.Add(lblLeft);
		base.Controls.Add(_txtLeft);
		base.Controls.Add(lblOp);
		base.Controls.Add(_cboOperator);
		base.Controls.Add(lblRight);
		base.Controls.Add(_txtRight);
		base.Controls.Add(lblNote);
		base.Controls.Add(_txtNote);
		base.Controls.Add(_btnOk);
		base.Controls.Add(_btnCancel);
		base.AcceptButton = _btnOk;
		base.CancelButton = _btnCancel;
	}

	private void OnOk()
	{
		if (string.IsNullOrWhiteSpace(_txtLeft.Text) && string.IsNullOrWhiteSpace(_txtRight.Text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "左表达式和右表达式不能同时为空");
			return;
		}
		_rule.LeftExpr = _txtLeft.Text.Trim();
		_rule.Operator = _cboOperator.SelectedIndex;
		_rule.RightExpr = _txtRight.Text.Trim();
		_rule.Note = _txtNote.Text.Trim();
		if (_cboTable.Enabled && _rule.DocumentFieldId.IsZero())
		{
			// 表格规则：按选中名字反查 TableId
			long tableId = _tableNames.FirstOrDefault((KeyValuePair<long, string> kv) => kv.Value == (_cboTable.SelectedItem as string)).Key;
			_rule.TableId = new Id64(tableId);
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
