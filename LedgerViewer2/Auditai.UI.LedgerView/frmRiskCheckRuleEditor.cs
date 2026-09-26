using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

/// <summary>
/// 账务数据风险检查的检查项编辑对话框：条件模式（科目范围 + 期初/期末/借方/贷方/负数多维五个维度）与公式模式（左表达式 比较符 右表达式）。
/// rule 就地编辑，确定后写回同一对象；ShowDialog 返回 OK 表示已写入。
/// </summary>
public class frmRiskCheckRuleEditor : Form
{
	// LedgerViewer2 引用不到 Auditai.UI.Platform 的 VirtualTableBuilder，此处固化其表 Id。
	// 来源：BalanceVirtualTableBuilder.BalanceVirtualTableId = new Id64(int.MaxValue, int.MaxValue)
	//       VoucherVirtualTableBuilder.VoucherVirtualTableId  = new Id64(int.MaxValue, 2147483646)
	// Id64 拼接规则：(upper << 32) | (uint)lower，故两值分别为 0x7FFFFFFF7FFFFFFF / 0x7FFFFFFF7FFFFFFE
	// （十进制 9223372034707292159 / 9223372034707292158）。若上游调整表 Id，需同步以下常量。
	private const long BalanceVirtualTableIdValue = 9223372034707292159L;

	private const long VoucherVirtualTableIdValue = 9223372034707292158L;

	// 与 BalanceVirtualTableBuilder.BalanceVirtualTableColumnIndex 枚举顺序同步（列 Id = 枚举整数值 = 下标）
	private static readonly string[] BalanceColumnNames = new string[27]
	{
		"年月", "科目代码", "科目名称", "本级科目名称", "科目级次", "是否末级",
		"辅助核算类别", "辅助核算代码", "辅助核算名称", "期初借方余额", "期初贷方余额",
		"本期借方发生额", "本期贷方发生额", "本年累计借方发生额", "本年累计贷方发生额",
		"期末借方余额", "期末贷方余额", "期初借方净额", "期初贷方净额", "期末借方净额",
		"期末贷方净额", "是否挂接辅助核算", "一级科目名称", "年初借方余额", "年初贷方余额",
		"年初借方净额", "年初贷方净额"
	};

	// 与 VoucherVirtualTableBuilder.VoucherVirtualTableColumnIndex 枚举顺序同步（列 Id = 枚举整数值 = 下标）
	private static readonly string[] VoucherColumnNames = new string[11]
	{
		"凭证日期", "凭证年月", "凭证字号", "摘要", "科目代码", "科目名称",
		"辅助核算类别", "辅助核算代码", "辅助核算名称", "借方发生额", "贷方发生额"
	};

	// 显示文本下标即 Op 编码：0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"
	private static readonly string[] OperatorTexts = new string[6] { "=", "＞", "≥", "＜", "≤", "≠" };

	private static readonly string[] DirectionTexts = new string[3] { "不限", "借方", "贷方" };

	private static readonly string[] ScopeTexts = new string[2] { "本期合计", "单张凭证" };

	private readonly RiskCheckRule _rule;

	private readonly List<Control> _conditionControls = new List<Control>();

	private RadioButton _rbCondition;

	private RadioButton _rbFormula;

	private TextBox _txtAccountCodes;

	private TextBox _txtAccountNames;

	private CheckBox _chkRequireLeaf;

	private CheckBox _chkOpeningEnabled;

	private ComboBox _cboOpeningDirection;

	private ComboBox _cboOpeningOp;

	private TextBox _txtOpeningValue;

	private CheckBox _chkClosingEnabled;

	private ComboBox _cboClosingDirection;

	private ComboBox _cboClosingOp;

	private TextBox _txtClosingValue;

	private CheckBox _chkDebitEnabled;

	private ComboBox _cboDebitScope;

	private ComboBox _cboDebitOp;

	private TextBox _txtDebitValue;

	private CheckBox _chkCreditEnabled;

	private ComboBox _cboCreditScope;

	private ComboBox _cboCreditOp;

	private TextBox _txtCreditValue;

	private CheckBox _chkAuxEnabled;

	private ComboBox _cboAuxOp;

	private TextBox _txtAuxValue;

	private Panel _pnlFormula;

	private TextBox _txtLeft;

	private ComboBox _cboFormulaOp;

	private TextBox _txtRight;

	/// <summary>说明（RiskCheckRule.Note）：结果表"检查项"列与导出的说明文本来源。</summary>
	private TextBox _txtNote;

	private ListBox _lstBalanceColumns;

	private ListBox _lstVoucherColumns;

	private TextBox _lastExprBox;

	public frmRiskCheckRuleEditor(RiskCheckRule rule)
	{
		_rule = rule;
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		base.Font = new Font("微软雅黑", 9f);
		Text = "检查项编辑";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		// 高度从 600 提到 640：底部腾出一行给"说明"输入（条件区到 y=498、公式面板到 y=522，
		// 按钮位置由 ClientSize.Height 计算，会自动下移，无需改按钮坐标）。
		ClientSize = new Size(520, 640);

		_rbCondition = new RadioButton
		{
			Text = "条件检查",
			Location = new Point(14, 16),
			AutoSize = true
		};
		_rbFormula = new RadioButton
		{
			Text = "公式检查",
			Location = new Point(122, 16),
			AutoSize = true
		};
		_rbCondition.CheckedChanged += delegate
		{
			ApplyMode();
		};

		BuildConditionArea();
		BuildFormulaArea();

		// 说明（Rule.Note）：字段本身已持久化（RiskCheckRule.note），
		// RiskCheckEngine 也会把它填进结果的 RuleNote，结果表与 Excel/HTML 导出都显示这一列，
		// 但此前整个编辑器没有录入入口 —— 说明列恒为空。此处在条件区/公式面板下方的空档补上，
		// 两种模式下都可见（故不加入 _conditionControls）。
		Label lblNote = CreateFieldLabel("说明：", 20, 533);
		_txtNote = new TextBox
		{
			Location = new Point(86, 530),
			Size = new Size(410, 23),
			MaxLength = 200
		};
		base.Controls.Add(lblNote);
		base.Controls.Add(_txtNote);

		Button btnOk = new Button
		{
			Text = "确定",
			// 取消贴右（右缘距窗体 50），确定在左侧间距 12
			Location = new Point(ClientSize.Width - 50 - 110 - 12 - 110, ClientSize.Height - 40 - 12),
			Size = new Size(110, 30)
		};
		btnOk.Click += delegate
		{
			OnOk();
		};
		Button btnCancel = new Button
		{
			Text = "取消",
			Location = new Point(ClientSize.Width - 50 - 110, ClientSize.Height - 40 - 12),
			Size = new Size(110, 30)
		};
		btnCancel.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		};

		base.Controls.Add(_rbCondition);
		base.Controls.Add(_rbFormula);
		base.Controls.Add(btnOk);
		base.Controls.Add(btnCancel);
		base.AcceptButton = btnOk;
		base.CancelButton = btnCancel;

		LoadFromRule();
		ApplyMode();
	}

	private void BuildConditionArea()
	{
		// 1 科目范围
		GroupBox grpAccount = CreateGroup("科目范围", 42, 100);
		_txtAccountCodes = new TextBox
		{
			Location = new Point(86, 25),
			Size = new Size(190, 23)
		};
		_txtAccountNames = new TextBox
		{
			Location = new Point(86, 53),
			Size = new Size(190, 23)
		};
		_chkRequireLeaf = new CheckBox
		{
			Text = "仅末级科目",
			Location = new Point(86, 78),
			AutoSize = true
		};
		grpAccount.Controls.Add(CreateFieldLabel("科目代码：", 14, 28));
		grpAccount.Controls.Add(_txtAccountCodes);
		grpAccount.Controls.Add(CreateHintLabel("多科目用逗号分隔，支持 * 通配，空=全部", 284, 28));
		grpAccount.Controls.Add(CreateFieldLabel("科目名称：", 14, 56));
		grpAccount.Controls.Add(_txtAccountNames);
		grpAccount.Controls.Add(CreateHintLabel("关键字逗号分隔，空=不限", 284, 56));
		grpAccount.Controls.Add(_chkRequireLeaf);
		_conditionControls.Add(grpAccount);
		base.Controls.Add(grpAccount);

		// 2 期初余额 / 3 期末余额
		_chkOpeningEnabled = new CheckBox();
		_cboOpeningDirection = new ComboBox();
		_cboOpeningOp = new ComboBox();
		_txtOpeningValue = new TextBox();
		BuildBalanceGroup(CreateGroup("期初余额", 150, 58), _chkOpeningEnabled, _cboOpeningDirection, _cboOpeningOp, _txtOpeningValue);

		_chkClosingEnabled = new CheckBox();
		_cboClosingDirection = new ComboBox();
		_cboClosingOp = new ComboBox();
		_txtClosingValue = new TextBox();
		BuildBalanceGroup(CreateGroup("期末余额", 216, 58), _chkClosingEnabled, _cboClosingDirection, _cboClosingOp, _txtClosingValue);

		// 4 借方金额 / 5 贷方金额
		_chkDebitEnabled = new CheckBox();
		_cboDebitScope = new ComboBox();
		_cboDebitOp = new ComboBox();
		_txtDebitValue = new TextBox();
		BuildAmountGroup(CreateGroup("借方金额", 282, 58), _chkDebitEnabled, _cboDebitScope, _cboDebitOp, _txtDebitValue);

		_chkCreditEnabled = new CheckBox();
		_cboCreditScope = new ComboBox();
		_cboCreditOp = new ComboBox();
		_txtCreditValue = new TextBox();
		BuildAmountGroup(CreateGroup("贷方金额", 348, 58), _chkCreditEnabled, _cboCreditScope, _cboCreditOp, _txtCreditValue);

		// 6 负数多维核算
		GroupBox grpAux = CreateGroup("负数多维核算", 414, 84);
		_chkAuxEnabled = new CheckBox
		{
			Text = "启用",
			Location = new Point(14, 26),
			AutoSize = true
		};
		_cboAuxOp = new ComboBox
		{
			Location = new Point(120, 51),
			Size = new Size(64, 23)
		};
		FillCombo(_cboAuxOp, OperatorTexts);
		_txtAuxValue = new TextBox
		{
			Location = new Point(240, 51),
			Size = new Size(216, 23)
		};
		grpAux.Controls.Add(_chkAuxEnabled);
		grpAux.Controls.Add(CreateHintLabel("辅助核算余额方向与科目方向相反即负数（按绝对值比较）", 66, 28));
		grpAux.Controls.Add(CreateFieldLabel("运算符：", 64, 55));
		grpAux.Controls.Add(_cboAuxOp);
		grpAux.Controls.Add(CreateFieldLabel("阈值：", 196, 55));
		grpAux.Controls.Add(_txtAuxValue);
		WireEnable(_chkAuxEnabled, _cboAuxOp, _txtAuxValue);
		_conditionControls.Add(grpAux);
		base.Controls.Add(grpAux);
	}

	private void BuildBalanceGroup(GroupBox grp, CheckBox chkEnabled, ComboBox cboDirection, ComboBox cboOp, TextBox txtValue)
	{
		chkEnabled.Text = "启用";
		chkEnabled.Location = new Point(14, 26);
		chkEnabled.AutoSize = true;
		cboDirection.Location = new Point(108, 23);
		cboDirection.Size = new Size(64, 23);
		FillCombo(cboDirection, DirectionTexts);
		cboOp.Location = new Point(178, 23);
		cboOp.Size = new Size(64, 23);
		FillCombo(cboOp, OperatorTexts);
		txtValue.Location = new Point(292, 23);
		txtValue.Size = new Size(164, 23);
		grp.Controls.Add(chkEnabled);
		grp.Controls.Add(CreateFieldLabel("方向：", 64, 27));
		grp.Controls.Add(cboDirection);
		grp.Controls.Add(CreateFieldLabel("运算符：", 176, 27));
		grp.Controls.Add(cboOp);
		grp.Controls.Add(CreateFieldLabel("阈值：", 248, 27));
		grp.Controls.Add(txtValue);
		WireEnable(chkEnabled, cboDirection, cboOp, txtValue);
		_conditionControls.Add(grp);
		base.Controls.Add(grp);
	}

	private void BuildAmountGroup(GroupBox grp, CheckBox chkEnabled, ComboBox cboScope, ComboBox cboOp, TextBox txtValue)
	{
		chkEnabled.Text = "启用";
		chkEnabled.Location = new Point(14, 26);
		chkEnabled.AutoSize = true;
		cboScope.Location = new Point(108, 23);
		cboScope.Size = new Size(88, 23);
		FillCombo(cboScope, ScopeTexts);
		cboOp.Location = new Point(202, 23);
		cboOp.Size = new Size(64, 23);
		FillCombo(cboOp, OperatorTexts);
		txtValue.Location = new Point(316, 23);
		txtValue.Size = new Size(140, 23);
		grp.Controls.Add(chkEnabled);
		grp.Controls.Add(CreateFieldLabel("口径：", 64, 27));
		grp.Controls.Add(cboScope);
		grp.Controls.Add(CreateFieldLabel("运算符：", 176, 27));
		grp.Controls.Add(cboOp);
		grp.Controls.Add(CreateFieldLabel("阈值：", 272, 27));
		grp.Controls.Add(txtValue);
		WireEnable(chkEnabled, cboScope, cboOp, txtValue);
		_conditionControls.Add(grp);
		base.Controls.Add(grp);
	}

	private void BuildFormulaArea()
	{
		_pnlFormula = new Panel
		{
			Location = new Point(12, 42),
			Size = new Size(496, 480)
		};
		Label lblHint = new Label
		{
			Text = "左侧表达式 [比较符] 右侧表达式；双击下方列名可在光标处插入 [2:表Id:列Id] 引用；支持 Sum/Abs/Round/If 等函数。",
			Location = new Point(10, 8),
			Size = new Size(476, 32),
			ForeColor = Color.Gray
		};
		_txtLeft = new TextBox
		{
			Location = new Point(86, 46),
			Size = new Size(394, 58),
			Multiline = true
		};
		_txtLeft.Enter += delegate
		{
			_lastExprBox = _txtLeft;
		};
		_cboFormulaOp = new ComboBox
		{
			Location = new Point(86, 112),
			Size = new Size(100, 23),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		FillCombo(_cboFormulaOp, OperatorTexts);
		_txtRight = new TextBox
		{
			Location = new Point(86, 142),
			Size = new Size(394, 58),
			Multiline = true
		};
		_txtRight.Enter += delegate
		{
			_lastExprBox = _txtRight;
		};
		_lstBalanceColumns = new ListBox
		{
			Location = new Point(10, 226),
			Size = new Size(236, 240)
		};
		string[] balanceColumns = BalanceColumnNames;
		foreach (string name in balanceColumns)
		{
			_lstBalanceColumns.Items.Add(name);
		}
		_lstBalanceColumns.DoubleClick += delegate
		{
			InsertColumnRef(BalanceVirtualTableIdValue, _lstBalanceColumns);
		};
		_lstVoucherColumns = new ListBox
		{
			Location = new Point(256, 226),
			Size = new Size(230, 240)
		};
		string[] voucherColumns = VoucherColumnNames;
		foreach (string name in voucherColumns)
		{
			_lstVoucherColumns.Items.Add(name);
		}
		_lstVoucherColumns.DoubleClick += delegate
		{
			InsertColumnRef(VoucherVirtualTableIdValue, _lstVoucherColumns);
		};
		_pnlFormula.Controls.Add(lblHint);
		_pnlFormula.Controls.Add(CreateFieldLabel("左表达式：", 10, 50));
		_pnlFormula.Controls.Add(_txtLeft);
		_pnlFormula.Controls.Add(CreateFieldLabel("运算符：", 10, 116));
		_pnlFormula.Controls.Add(_cboFormulaOp);
		_pnlFormula.Controls.Add(CreateFieldLabel("右表达式：", 10, 146));
		_pnlFormula.Controls.Add(_txtRight);
		_pnlFormula.Controls.Add(CreateHintLabel("科目余额表（双击插入）：", 10, 204));
		_pnlFormula.Controls.Add(_lstBalanceColumns);
		_pnlFormula.Controls.Add(CreateHintLabel("会计凭证表（双击插入）：", 256, 204));
		_pnlFormula.Controls.Add(_lstVoucherColumns);
		base.Controls.Add(_pnlFormula);
		_lastExprBox = _txtLeft;
	}

	private void LoadFromRule()
	{
		bool isFormula = _rule.RuleType == RiskCheckRule.RULE_TYPE_FORMULA;
		_rbFormula.Checked = isFormula;
		_rbCondition.Checked = !isFormula;

		_txtNote.Text = _rule.Note ?? string.Empty;

		_txtAccountCodes.Text = _rule.AccountCodes ?? string.Empty;
		_txtAccountNames.Text = _rule.AccountNames ?? string.Empty;
		_chkRequireLeaf.Checked = _rule.RequireLeaf;

		_chkOpeningEnabled.Checked = _rule.OpeningEnabled;
		_cboOpeningDirection.SelectedIndex = Clamp(_rule.OpeningDirection, DirectionTexts.Length);
		_cboOpeningOp.SelectedIndex = Clamp(_rule.OpeningOp, OperatorTexts.Length);
		_txtOpeningValue.Text = _rule.OpeningValue.ToString();

		_chkClosingEnabled.Checked = _rule.ClosingEnabled;
		_cboClosingDirection.SelectedIndex = Clamp(_rule.ClosingDirection, DirectionTexts.Length);
		_cboClosingOp.SelectedIndex = Clamp(_rule.ClosingOp, OperatorTexts.Length);
		_txtClosingValue.Text = _rule.ClosingValue.ToString();

		_chkDebitEnabled.Checked = _rule.DebitEnabled;
		_cboDebitScope.SelectedIndex = Clamp(_rule.DebitScope, ScopeTexts.Length);
		_cboDebitOp.SelectedIndex = Clamp(_rule.DebitOp, OperatorTexts.Length);
		_txtDebitValue.Text = _rule.DebitValue.ToString();

		_chkCreditEnabled.Checked = _rule.CreditEnabled;
		_cboCreditScope.SelectedIndex = Clamp(_rule.CreditScope, ScopeTexts.Length);
		_cboCreditOp.SelectedIndex = Clamp(_rule.CreditOp, OperatorTexts.Length);
		_txtCreditValue.Text = _rule.CreditValue.ToString();

		_chkAuxEnabled.Checked = _rule.AuxNegativeEnabled;
		_cboAuxOp.SelectedIndex = Clamp(_rule.AuxOp, OperatorTexts.Length);
		_txtAuxValue.Text = _rule.AuxValue.ToString();

		_txtLeft.Text = _rule.LeftExpr ?? string.Empty;		_cboFormulaOp.SelectedIndex = Clamp(_rule.OperatorCode, OperatorTexts.Length);
		_txtRight.Text = _rule.RightExpr ?? string.Empty;
	}

	private void ApplyMode()
	{
		bool condition = _rbCondition.Checked;
		foreach (Control control in _conditionControls)
		{
			control.Visible = condition;
		}
		_pnlFormula.Visible = !condition;
	}

	private void InsertColumnRef(long tableId, ListBox list)
	{
		TextBox box = _lastExprBox;
		if (box == null || list.SelectedIndex < 0)
		{
			return;
		}
		string reference = "[2:" + tableId + ":" + list.SelectedIndex + "]";
		string text = box.Text ?? string.Empty;
		int start = box.SelectionStart;
		if (start < 0 || start > text.Length)
		{
			start = text.Length;
		}
		box.Text = text.Insert(start, reference);
		box.SelectionStart = start + reference.Length;
		box.SelectionLength = 0;
		box.Focus();
	}

	private void OnOk()
	{
		// 说明对条件/公式两种检查项都适用，放在模式分支之前统一回写
		_rule.Note = _txtNote.Text.Trim();
		if (_rbCondition.Checked)
		{
			decimal openingValue = 0m;
			decimal closingValue = 0m;
			decimal debitValue = 0m;
			decimal creditValue = 0m;
			decimal auxValue = 0m;
			if (_chkOpeningEnabled.Checked && !decimal.TryParse(_txtOpeningValue.Text.Trim(), out openingValue))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "期初余额阈值不是有效的数字", MessageBoxButtons.OK, Text);
				return;
			}
			if (_chkClosingEnabled.Checked && !decimal.TryParse(_txtClosingValue.Text.Trim(), out closingValue))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "期末余额阈值不是有效的数字", MessageBoxButtons.OK, Text);
				return;
			}
			if (_chkDebitEnabled.Checked && !decimal.TryParse(_txtDebitValue.Text.Trim(), out debitValue))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "借方金额阈值不是有效的数字", MessageBoxButtons.OK, Text);
				return;
			}
			if (_chkCreditEnabled.Checked && !decimal.TryParse(_txtCreditValue.Text.Trim(), out creditValue))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "贷方金额阈值不是有效的数字", MessageBoxButtons.OK, Text);
				return;
			}
			if (_chkAuxEnabled.Checked && !decimal.TryParse(_txtAuxValue.Text.Trim(), out auxValue))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "负数多维核算阈值不是有效的数字", MessageBoxButtons.OK, Text);
				return;
			}
			if (!_chkOpeningEnabled.Checked && !_chkClosingEnabled.Checked && !_chkDebitEnabled.Checked && !_chkCreditEnabled.Checked && !_chkAuxEnabled.Checked)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "条件检查至少需要启用一个余额或金额维度", MessageBoxButtons.OK, Text);
				return;
			}
			_rule.RuleType = RiskCheckRule.RULE_TYPE_CONDITION;
			_rule.AccountCodes = _txtAccountCodes.Text.Trim();
			_rule.AccountNames = _txtAccountNames.Text.Trim();
			_rule.RequireLeaf = _chkRequireLeaf.Checked;
			if (_chkOpeningEnabled.Checked)
			{
				_rule.OpeningEnabled = true;
				_rule.OpeningDirection = _cboOpeningDirection.SelectedIndex;
				_rule.OpeningOp = _cboOpeningOp.SelectedIndex;
				_rule.OpeningValue = openingValue;
			}
			else
			{
				_rule.OpeningEnabled = false;
			}
			if (_chkClosingEnabled.Checked)
			{
				_rule.ClosingEnabled = true;
				_rule.ClosingDirection = _cboClosingDirection.SelectedIndex;
				_rule.ClosingOp = _cboClosingOp.SelectedIndex;
				_rule.ClosingValue = closingValue;
			}
			else
			{
				_rule.ClosingEnabled = false;
			}
			if (_chkDebitEnabled.Checked)
			{
				_rule.DebitEnabled = true;
				_rule.DebitScope = _cboDebitScope.SelectedIndex;
				_rule.DebitOp = _cboDebitOp.SelectedIndex;
				_rule.DebitValue = debitValue;
			}
			else
			{
				_rule.DebitEnabled = false;
			}
			if (_chkCreditEnabled.Checked)
			{
				_rule.CreditEnabled = true;
				_rule.CreditScope = _cboCreditScope.SelectedIndex;
				_rule.CreditOp = _cboCreditOp.SelectedIndex;
				_rule.CreditValue = creditValue;
			}
			else
			{
				_rule.CreditEnabled = false;
			}
			if (_chkAuxEnabled.Checked)
			{
				_rule.AuxNegativeEnabled = true;
				_rule.AuxOp = _cboAuxOp.SelectedIndex;
				_rule.AuxValue = auxValue;
			}
			else
			{
				_rule.AuxNegativeEnabled = false;
			}
		}
		else
		{
			if (string.IsNullOrWhiteSpace(_txtLeft.Text) || string.IsNullOrWhiteSpace(_txtRight.Text))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "公式模式下左表达式和右表达式均不能为空", MessageBoxButtons.OK, Text);
				return;
			}
			_rule.RuleType = RiskCheckRule.RULE_TYPE_FORMULA;
			_rule.LeftExpr = _txtLeft.Text.Trim();
			_rule.OperatorCode = _cboFormulaOp.SelectedIndex;
			_rule.RightExpr = _txtRight.Text.Trim();
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private static void WireEnable(CheckBox chkEnabled, params Control[] linked)
	{
		foreach (Control control in linked)
		{
			control.Enabled = chkEnabled.Checked;
		}
		chkEnabled.CheckedChanged += delegate
		{
			foreach (Control control in linked)
			{
				control.Enabled = chkEnabled.Checked;
			}
		};
	}

	private static GroupBox CreateGroup(string title, int y, int height)
	{
		return new GroupBox
		{
			Text = title,
			Location = new Point(12, y),
			Size = new Size(496, height)
		};
	}

	private static Label CreateFieldLabel(string text, int x, int y)
	{
		return new Label
		{
			Text = text,
			Location = new Point(x, y),
			AutoSize = true
		};
	}

	private static Label CreateHintLabel(string text, int x, int y)
	{
		return new Label
		{
			Text = text,
			Location = new Point(x, y),
			AutoSize = true,
			ForeColor = Color.Gray
		};
	}

	private static void FillCombo(ComboBox combo, string[] items)
	{
		combo.DropDownStyle = ComboBoxStyle.DropDownList;
		combo.Items.AddRange(items);
		combo.SelectedIndex = 0;
	}

	private static int Clamp(int value, int count)
	{
		return Math.Min(Math.Max(value, 0), count - 1);
	}
}
