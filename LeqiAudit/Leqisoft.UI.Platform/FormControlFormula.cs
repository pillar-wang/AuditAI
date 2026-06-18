using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class FormControlFormula
{
	private C1RibbonForm _form;

	private RichTextBoxEx _rtb;

	private C1SplitContainer _ctn;

	private C1SplitterPanel _pnlBottom;

	private C1SplitterPanel _pnlRtb;

	private C1SplitterPanel _pnlLabel;

	private C1SplitterPanel _pnlTbr;

	private C1Button _btnOk;

	private C1Button _btnCancel;

	private C1Label _lbl;

	private C1ToolBar _tbr;

	private C1Command _cmdLock;

	private C1CommandLink _lnkLock;

	private C1Command _cmdWarning;

	private C1CommandLink _lnkWarning;

	private C1Command _cmdRemind;

	private C1CommandLink _lnkRemind;

	private C1Command _cmdForeColor;

	private C1CommandLink _lnkForeColor;

	private C1Command _cmdBackColor;

	private C1CommandLink _lnkBackColor;

	private C1Command _cmdAllowRowEdit;

	private C1CommandLink _lnkAllowRowEdit;

	private bool _isTextChanging;

	public DialogResult DialogResult => _form.DialogResult;

	public string Result { get; set; }

	public bool IsEditing { get; private set; }

	public Table Table => Program.MainForm.TableEditor.Table;

	public event EventHandler Closed;

	public void New()
	{
		_form = new C1RibbonForm
		{
			Size = new Size(800, 600),
			Text = "控制公式",
			DialogResult = DialogResult.Cancel,
			Font = new Font("微软雅黑", 9f),
			StartPosition = FormStartPosition.CenterScreen,
			ShowInTaskbar = false
		};
		_form.FormClosed += _form_FormClosed;
		_ctn = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_pnlTbr = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Height = 24,
			MinHeight = 24,
			Resizable = false,
			KeepRelativeSize = false
		};
		_tbr = new C1ToolBar
		{
			Dock = DockStyle.Fill
		};
		_cmdLock = new C1Command
		{
			Text = "Lock函数"
		};
		_cmdLock.Click += _cmdLock_Click;
		_lnkLock = new C1CommandLink(_cmdLock)
		{
			ButtonLook = ButtonLookFlags.Text
		};
		_cmdWarning = new C1Command
		{
			Text = "Warning函数"
		};
		_cmdWarning.Click += _cmdWarning_Click;
		_lnkWarning = new C1CommandLink(_cmdWarning)
		{
			ButtonLook = ButtonLookFlags.Text
		};
		_cmdRemind = new C1Command
		{
			Text = "Remind函数"
		};
		_cmdRemind.Click += _cmdRemind_Click;
		_lnkRemind = new C1CommandLink(_cmdRemind)
		{
			ButtonLook = ButtonLookFlags.Text
		};
		_cmdForeColor = new C1Command
		{
			Text = "ForeColor函数"
		};
		_cmdForeColor.Click += _cmdForeColor_Click;
		_lnkForeColor = new C1CommandLink(_cmdForeColor)
		{
			ButtonLook = ButtonLookFlags.Text
		};
		_cmdBackColor = new C1Command
		{
			Text = "BackColor函数"
		};
		_cmdBackColor.Click += _cmdBackColor_Click;
		_lnkBackColor = new C1CommandLink(_cmdBackColor)
		{
			ButtonLook = ButtonLookFlags.Text
		};
		_cmdAllowRowEdit = new C1Command
		{
			Text = "AllowRowEdit函数"
		};
		_cmdAllowRowEdit.Click += _cmdAllowRowEdit_Click;
		_lnkAllowRowEdit = new C1CommandLink(_cmdAllowRowEdit)
		{
			ButtonLook = ButtonLookFlags.Text
		};
		_tbr.CommandLinks.Add(_lnkLock);
		_tbr.CommandLinks.Add(_lnkWarning);
		_tbr.CommandLinks.Add(_lnkRemind);
		_tbr.CommandLinks.Add(_lnkForeColor);
		_tbr.CommandLinks.Add(_lnkBackColor);
		_tbr.CommandLinks.Add(_lnkAllowRowEdit);
		_pnlTbr.Controls.Add(_tbr);
		_ctn.Panels.Add(_pnlTbr);
		_pnlBottom = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Bottom,
			Height = 50,
			MinHeight = 50,
			Resizable = false,
			KeepRelativeSize = false
		};
		_btnOk = new C1Button
		{
			Anchor = AnchorStyles.Right,
			Size = new Size(70, 30),
			Text = "确定"
		};
		_btnOk.Click += _btnOk_Click;
		_pnlBottom.Controls.Add(_btnOk);
		_btnCancel = new C1Button
		{
			Anchor = AnchorStyles.Right,
			Size = new Size(70, 30),
			Text = "取消"
		};
		_btnCancel.Click += _btnCancel_Click;
		_pnlBottom.Controls.Add(_btnCancel);
		_ctn.Panels.Add(_pnlBottom);
		_pnlLabel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Bottom,
			Height = 100,
			MinHeight = 100,
			Resizable = false,
			KeepRelativeSize = false
		};
		_lbl = new C1Label
		{
			Dock = DockStyle.Fill
		};
		_pnlLabel.Controls.Add(_lbl);
		_ctn.Panels.Add(_pnlLabel);
		_rtb = new RichTextBoxEx
		{
			Dock = DockStyle.Fill
		};
		_rtb.TextChanged += _rtb_TextChanged;
		_rtb.SelectionChanged += _rtb_SelectionChanged;
		_pnlRtb = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			Resizable = false,
			KeepRelativeSize = false
		};
		_pnlRtb.Controls.Add(_rtb);
		_ctn.Panels.Add(_pnlRtb);
		_form.Controls.Add(_ctn);
		IsEditing = true;
	}

	public void Show()
	{
		_form.Show(Program.MainForm.View);
		Theme.SetCurrentTree(_form);
		_btnOk.Location = new Point(600, 10);
		_btnCancel.Location = new Point(700, 10);
		_form.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.ControlFormula);
		if (Table.HasControlFormula)
		{
			ControlFormulaEvaluator controlFormulaEvaluator = new ControlFormulaEvaluator(Table.ControlFormula);
			_rtb.Text = controlFormulaEvaluator.GetDisplayString(new FormulaReferenceModelResolver(Project.Current), Table);
		}
	}

	public void InsertRefTextAndFocus(string t)
	{
		_form.Activate();
		_rtb.SelectedText = t;
		_rtb.SelectionStart += _rtb.SelectionLength;
	}

	public void RemoveRefAtPos()
	{
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(_rtb.Text);
			Tuple<int, int> refAtPos = formulaDisplay.GetRefAtPos(_rtb.SelectionStart);
			if (refAtPos != null)
			{
				_rtb.Select(refAtPos.Item1, refAtPos.Item2 - refAtPos.Item1 + 1);
			}
		}
		catch (FormulaException)
		{
		}
	}

	public bool UseWildcard()
	{
		return UseWildcardImpl(_rtb.Text, _rtb.SelectionStart);
	}

	private void _cmdLock_Click(object sender, ClickEventArgs e)
	{
		InsertRefTextAndFocus("Lock()");
		_rtb.SelectionStart--;
	}

	private void _cmdWarning_Click(object sender, ClickEventArgs e)
	{
		InsertRefTextAndFocus("Warning()");
		_rtb.SelectionStart--;
	}

	private void _cmdRemind_Click(object sender, ClickEventArgs e)
	{
		InsertRefTextAndFocus("Remind()");
		_rtb.SelectionStart--;
	}

	private void _cmdForeColor_Click(object sender, ClickEventArgs e)
	{
		InsertRefTextAndFocus("ForeColor()");
		_rtb.SelectionStart--;
	}

	private void _cmdBackColor_Click(object sender, ClickEventArgs e)
	{
		InsertRefTextAndFocus("BackColor()");
		_rtb.SelectionStart--;
	}

	private void _cmdAllowRowEdit_Click(object sender, ClickEventArgs e)
	{
		InsertRefTextAndFocus("AllowRowEdit()");
		_rtb.SelectionStart--;
	}

	private void _form_FormClosed(object sender, FormClosedEventArgs e)
	{
		IsEditing = false;
		OnClosed();
	}

	private void _rtb_TextChanged(object sender, EventArgs e)
	{
		SyntaxHighlight();
		ShowFunctionHint(_rtb.Text, _rtb.SelectionStart);
	}

	private void _rtb_SelectionChanged(object sender, EventArgs e)
	{
		if (!_isTextChanging)
		{
			ShowFunctionHint(_rtb.Text, _rtb.SelectionStart);
		}
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_form.Close();
		OnClosed();
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		if (ValidateFormula(out var formula))
		{
			Result = formula;
			_form.DialogResult = DialogResult.OK;
			_form.Close();
			OnClosed();
		}
	}

	private void OnClosed()
	{
		this.Closed?.Invoke(this, EventArgs.Empty);
	}

	private bool ValidateFormula(out string formula)
	{
		formula = string.Empty;
		if (string.IsNullOrWhiteSpace(_rtb.Text))
		{
			return true;
		}
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(_rtb.Text);
			formula = formulaDisplay.ToControlFormula(Program.MainForm.FormulaEditor.Context);
			return true;
		}
		catch (FormulaParameterCountException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
			return false;
		}
		catch (FormulaFunctionNotExistException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
			return false;
		}
		catch (FormulaSyntaxException ex3)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			_rtb.Select(ex3.CharPosition, 0);
			_rtb.Focus();
			return false;
		}
		catch (FormulaBadReferenceException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
			return false;
		}
		catch (FormulaNotApplicableException ex5)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
			return false;
		}
		catch (FormulaBadValueException ex6)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
			return false;
		}
		catch (FormulaTypeMismatchException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
			return false;
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "公式引用的列没有本表对应的行");
			return false;
		}
	}

	public void ShowFunctionHint(string formulaText, int pos)
	{
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(formulaText);
			Tuple<string, int> tup = formulaDisplay.GetFuncNameAtPos(pos);
			if (tup.Item1 == null)
			{
				_lbl.Value = "";
				return;
			}
			FunctionInfo functionInfo = FunctionInfo.AllFunctionInfos.FirstOrDefault((FunctionInfo f) => f.Name.Equals(tup.Item1, StringComparison.InvariantCultureIgnoreCase));
			if (functionInfo == null || !Program.MainForm.IsAllowShowFunctionInfoAtCurrentView(functionInfo, isControlFormulaForm: true))
			{
				_lbl.Value = "";
				return;
			}
			string text = "函数语法：" + functionInfo.Name + "(" + string.Join(", ", functionInfo.Parameters.Select((ParameterInfo p) => p.Name)) + ")\n";
			text = text + "函数功能：" + functionInfo.Description + "\n";
			text = text + "参数说明：" + string.Join("；", functionInfo.Parameters.Select((ParameterInfo p) => p.Name + "：" + p.Description));
			_lbl.Value = text;
		}
		catch (FormulaException)
		{
			_lbl.Value = "";
		}
	}

	private void SyntaxHighlight()
	{
		_isTextChanging = true;
		_rtb.SuspendDrawing();
		int selectionStart = _rtb.SelectionStart;
		int selectionLength = _rtb.SelectionLength;
		int vScrollPos = _rtb.VScrollPos;
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(_rtb.Text);
			_rtb.SelectAll();
			_rtb.SelectionBackColor = Color.Transparent;
			_rtb.SelectionColor = Color.Black;
			foreach (Tuple<int, int, Color> tokenColorInterval in formulaDisplay.GetTokenColorIntervals())
			{
				_rtb.Select(tokenColorInterval.Item1, tokenColorInterval.Item2);
				_rtb.SelectionColor = tokenColorInterval.Item3;
			}
			Tuple<List<FormulaDisplayRef>, Color> references = formulaDisplay.GetReferences(Program.MainForm.FormulaEditor.Context);
			Program.MainForm.FormulaEditor.RefIntervals = references.Item1;
			Program.MainForm.FormulaEditor.NextColor = references.Item2;
			foreach (FormulaDisplayRef refInterval in Program.MainForm.FormulaEditor.RefIntervals)
			{
				foreach (Tuple<int, int> interval in refInterval.Intervals)
				{
					_rtb.Select(interval.Item1, interval.Item2);
					_rtb.SelectionColor = refInterval.Color;
				}
			}
		}
		catch (FormulaException)
		{
		}
		finally
		{
			_rtb.Select(selectionStart, selectionLength);
			_rtb.VScrollPos = vScrollPos;
			_rtb.ResumeDrawing();
			_isTextChanging = false;
		}
	}

	private static bool UseWildcardImpl(string formulaText, int pos)
	{
		if (pos == 0)
		{
			return false;
		}
		char c = formulaText[pos - 1];
		if (c == '=' || c == '>' || c == '<')
		{
			return true;
		}
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(formulaText);
			Tuple<string, int> funcNameAtPos = formulaDisplay.GetFuncNameAtPos(pos);
			if ("Select".Equals(funcNameAtPos.Item1, StringComparison.OrdinalIgnoreCase) && funcNameAtPos.Item2 % 2 == 0)
			{
				return true;
			}
			if ("If".Equals(funcNameAtPos.Item1, StringComparison.OrdinalIgnoreCase) && funcNameAtPos.Item2 == 0)
			{
				return true;
			}
		}
		catch (FormulaException)
		{
			return false;
		}
		return false;
	}
}
