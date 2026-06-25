using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class FormulaGridEditor : Control, IC1EmbeddedEditor
{
	private readonly ValidationEditor _owner;

	private readonly C1DropDownControl _ddc;

	private readonly FunctionSelector _functionSelectDropDownForm;

	internal readonly RichTextBox _rtf;

	public FormulaGridEditor(ValidationEditor owner)
	{
		_owner = owner;
		C1SplitContainer c1SplitContainer = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		C1Button c1Button = new C1Button
		{
			Text = "函数",
			Dock = DockStyle.Fill,
			FlatStyle = FlatStyle.Flat
		};
		c1Button.FlatAppearance.BorderSize = 0;
		c1Button.Click += _btn_Click;
		_functionSelectDropDownForm = new FunctionSelector();
		_functionSelectDropDownForm.CheckFunctionIsVisibleCallback = Program.MainForm.IsAllowShowFunctionInfoInFunctionList;
		_ddc = new C1DropDownControl
		{
			DropDownForm = _functionSelectDropDownForm
		};
		_ddc.Enter += _ddc_Enter;
		_ddc.DropDownClosed += _ddc_DropDownClosed;
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			KeepRelativeSize = false,
			Width = 50,
			Resizable = false
		};
		c1SplitterPanel.Controls.Add(c1Button);
		c1SplitterPanel.Controls.Add(_ddc);
		c1SplitContainer.Panels.Add(c1SplitterPanel);
		_rtf = new RichTextBox
		{
			Dock = DockStyle.Fill,
			BorderStyle = BorderStyle.None,
			Multiline = true,
			ScrollBars = RichTextBoxScrollBars.None
		};
		_rtf.Enter += _rtf_Enter;
		_rtf.Leave += _rtf_Leave;
		_rtf.KeyDown += _rtf_KeyDown;
		_rtf.LostFocus += _rtf_LostFocus;
		_rtf.SelectionChanged += _rtf_SelectionChanged;
		_rtf.TextChanged += _rtf_TextChanged;
		_rtf.ContentsResized += _rtf_ContentsResized;
		c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			SizeRatio = 100.0,
			KeepRelativeSize = true
		};
		c1SplitterPanel.Controls.Add(_rtf);
		c1SplitContainer.Panels.Add(c1SplitterPanel);
		base.Controls.Add(c1SplitContainer);
	}

	public string C1EditorFormat(object value, string mask)
	{
		return value.ToString();
	}

	public UITypeEditorEditStyle C1EditorGetStyle()
	{
		return UITypeEditorEditStyle.None;
	}

	public void SetTheme()
	{
		_functionSelectDropDownForm?.SetTheme();
	}

	public object C1EditorGetValue()
	{
		return ReplaceFullWidth(_rtf.Text);
	}

	public void C1EditorInitialize(object value, IDictionary editorAttributes)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		_rtf.ResetText();
		_rtf.Text = value.ToString();
		if (_owner._editingPosition == 0)
		{
			_rtf.Select(_rtf.Text.Length, 0);
		}
		else
		{
			_rtf.Select(_owner._editingPosition, 0);
		}
	}

	public bool C1EditorKeyDownFinishEdit(KeyEventArgs e)
	{
		return true;
	}

	public void C1EditorUpdateBounds(Rectangle rc)
	{
		base.Location = rc.Location;
	}

	public bool C1EditorValueIsValid()
	{
		return true;
	}

	private void _btn_Click(object sender, EventArgs e)
	{
		if (SoftwareLicenseManager.IsAllowEditFormula())
		{
			_functionSelectDropDownForm.RefreshFuncList();
			_ddc.OpenDropDown();
		}
	}

	private void _ddc_Enter(object sender, EventArgs e)
	{
		_rtf.Focus();
	}

	private void _ddc_DropDownClosed(object sender, DropDownClosedEventArgs e)
	{
		MethodInfo selectedFunction = _functionSelectDropDownForm.SelectedFunction;
		if (!(selectedFunction == null))
		{
			_rtf.SelectedText = selectedFunction.Name + "()";
			_rtf.Select(_rtf.SelectionStart - 1, 0);
		}
	}

	private void _rtf_SelectionChanged(object sender, EventArgs e)
	{
		if (_rtf.Focused)
		{
			ShowFunctionHint();
		}
	}

	private void _rtf_TextChanged(object sender, EventArgs e)
	{
		if (!_owner.IsEditing)
		{
			return;
		}
		_rtf.SelectionChanged -= _rtf_SelectionChanged;
		_rtf.SuspendDrawing();
		int selectionStart = _rtf.SelectionStart;
		int selectionLength = _rtf.SelectionLength;
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(_rtf.Text);
			_rtf.SelectAll();
			_rtf.SelectionBackColor = Color.Transparent;
			_rtf.SelectionColor = Color.Black;
			foreach (Tuple<int, int, Color> tokenColorInterval in formulaDisplay.GetTokenColorIntervals())
			{
				_rtf.Select(tokenColorInterval.Item1, tokenColorInterval.Item2);
				_rtf.SelectionColor = tokenColorInterval.Item3;
			}
			Tuple<List<FormulaDisplayRef>, Color> references = formulaDisplay.GetReferences(_owner.Context);
			Program.MainForm.FormulaEditor.RefIntervals = references.Item1;
			Program.MainForm.FormulaEditor.NextColor = references.Item2;
			foreach (FormulaDisplayRef refInterval in Program.MainForm.FormulaEditor.RefIntervals)
			{
				foreach (Tuple<int, int> interval in refInterval.Intervals)
				{
					_rtf.Select(interval.Item1, interval.Item2);
					_rtf.SelectionColor = refInterval.Color;
				}
			}
		}
		catch (FormulaException)
		{
		}
		finally
		{
			_rtf.Select(selectionStart, selectionLength);
			_rtf.ResumeDrawing();
			_rtf.SelectionChanged += _rtf_SelectionChanged;
		}
	}

	private void _rtf_Leave(object sender, EventArgs e)
	{
		Program.MainForm.HideFunctionHint();
	}

	private void _rtf_Enter(object sender, EventArgs e)
	{
		ShowFunctionHint();
	}

	private void _rtf_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			_owner.Commit();
		}
		else if (e.KeyCode == Keys.Escape)
		{
			_owner.Cancel();
		}
		else
		{
			if (e.KeyCode != Keys.V || e.Modifiers != Keys.Control)
			{
				return;
			}
			try
			{
				if (Clipboard.ContainsText())
				{
					_rtf.SelectedText = Clipboard.GetText().Replace("\n", "").Replace("\r", "");
				}
			}
			catch (ExternalException)
			{
			}
			e.Handled = true;
		}
	}

	private void _rtf_LostFocus(object sender, EventArgs e)
	{
		_owner._editingPosition = _rtf.SelectionStart;
	}

	private void _rtf_ContentsResized(object sender, ContentsResizedEventArgs e)
	{
		if (_owner.Grid == null)
		{
			base.Height = e.NewRectangle.Height;
			return;
		}
		int defaultSize = e.NewRectangle.Height;
		if (defaultSize < _owner.Grid.Rows.DefaultSize)
		{
			defaultSize = _owner.Grid.Rows.DefaultSize;
		}
		base.Height = defaultSize;
	}

	private void TooltipBox_CloseClick(object sender, EventArgs e)
	{
		_owner._showFunctionHint = false;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		_rtf.Focus();
	}

	private static string ReplaceFullWidth(string fullWidth)
	{
		return fullWidth.Replace('（', '(').Replace('）', ')').Replace('，', ',')
			.Replace('＝', '=');
	}

	private void ShowFunctionHint()
	{
		Program.MainForm.ShowFunctionHint(_rtf.Text, _rtf.SelectionStart);
	}
}
