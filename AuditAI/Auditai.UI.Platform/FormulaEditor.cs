﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using TXTextControl;

namespace Auditai.UI.Platform;

public class FormulaEditor : ISetTheme
{
	public class LedgerVirtualTableSettingProvider : ILegderVirtualTableSetting
	{
		public long GetBalanceVirtualTableColumnId(string columnName)
		{
			if (BalanceVirtualTableBuilder.TryGetColumnId(columnName, out var columnId))
			{
				return columnId;
			}
			return -1L;
		}

		public string GetBalanceVirtualTableColumnName(long columnId)
		{
			if (BalanceVirtualTableBuilder.TryGetColumnName(columnId, out var columnName))
			{
				return columnName;
			}
			return null;
		}

		public long GetBalanceVirtualTableId()
		{
			return BalanceVirtualTableBuilder.BalanceVirtualTableId.Value;
		}

		public string GetBalanceVirtualTableName()
		{
			return "科目余额表";
		}

		public LedgerVirtualTable GetBalanceEmptyVirtualTable()
		{
			return BalanceVirtualTableBuilder.GetEmtpyTable();
		}

		public Auditai.Model.Column GetBalanceEmptyVirtualTableColumn(string columnName)
		{
			int columnIndex = BalanceVirtualTableBuilder.GetColumnIndex(columnName);
			if (columnIndex < 0)
			{
				return null;
			}
			LedgerVirtualTable emtpyTable = BalanceVirtualTableBuilder.GetEmtpyTable();
			return emtpyTable.Columns[columnIndex];
		}

		public long GetVoucherVirtualTableColumnId(string columnName)
		{
			if (VoucherVirtualTableBuilder.TryGetColumnId(columnName, out var columnId))
			{
				return columnId;
			}
			return -1L;
		}

		public string GetVoucherVirtualTableColumnName(long columnId)
		{
			if (VoucherVirtualTableBuilder.TryGetColumnName(columnId, out var columnName))
			{
				return columnName;
			}
			return null;
		}

		public long GetVoucherVirtualTableId()
		{
			return VoucherVirtualTableBuilder.VoucherVirtualTableId.Value;
		}

		public string GetVoucherVirtualTableName()
		{
			return "会计凭证表";
		}

		public LedgerVirtualTable GetVoucherEmptyVirtualTable()
		{
			return VoucherVirtualTableBuilder.GetEmtpyTable();
		}

		public Auditai.Model.Column GetVoucherEmptyVirtualTableColumn(string columnName)
		{
			int columnIndex = VoucherVirtualTableBuilder.GetColumnIndex(columnName);
			if (columnIndex < 0)
			{
				return null;
			}
			LedgerVirtualTable emtpyTable = VoucherVirtualTableBuilder.GetEmtpyTable();
			return emtpyTable.Columns[columnIndex];
		}
	}

	private const int MAX_FORMULA_LENGTH = 10000;

	private static HashSet<string> _setUseStar = new HashSet<string>(new string[30]
	{
		"LqAsc", "LqCountIf", "LqCrossTable", "LqDesc", "LqDistinct", "LqFilter", "LqMax", "LqMin", "LqSumFind", "LqSumIf",
		"LqVLookUp", "LqCollect", "Sum", "ColName", "Except", "Union", "Intersect", "DistinctUp", "CountIf", "CrossTable",
		"DistinctDown", "Distinct", "DistinctF", "MaxF", "MinF", "SumFind", "SumIf", "VLookUp", "CollectF", "Collect"
	}, StringComparer.OrdinalIgnoreCase);

	public const int DEFAULTHEIGHT = 31;

	public readonly C1TextBoxEx_SupportSelfBorder txtSourceCell;

	public readonly C1ButtonEx_SupportSelfBorder btnSelector;

	protected readonly C1DropDownControl drpSelector;

	protected readonly RichTextBoxEx rtbFormula;

	protected readonly C1SplitterPanel pnlFormula;

	protected readonly TooltipBox tooltipBox = new TooltipBox
	{
		Opacity = 0.8,
		IsBalloon = true
	};

	private readonly TooltipManager tooltipManager = new TooltipManager();

	private FunctionSelector _functionSelectDropDownForm;

	protected Pen _formulaBorderPen = new Pen(Color.Black, 1f);

	protected C1SplitterPanel _pnlSourceCell;

	protected C1SplitContainer _containerSourceCell;

	protected C1SplitContainer _containerSelectorButton;

	protected C1SplitContainer _containerFormulaTextBox;

	protected C1SplitterPanel _pnlAll;

	private const string _pardSpaceExp = "sb40\\";

	private bool _isCommitEventProcessed;

	private bool _suspendTextChangeEvent;

	private bool _isPreviousContextRestoreSelect = true;

	private TableEditor _owner => Program.MainForm.TableEditor;

	public object LastClickedComponent { get; set; }

	public bool IsEditing { get; private set; }

	public bool IsFinishingEditing { get; private set; }

	public int DesiredHeight { get; private set; }

	public FormulaContext Context { get; } = new FormulaContext
	{
		Kind = FormulaContextKind.None,
		LegderVirtualTableSetting = new LedgerVirtualTableSettingProvider()
	};


	public C1SplitContainer View { get; }

	public List<FormulaDisplayRef> RefIntervals { get; internal set; }

	public Color NextColor { get; internal set; }

	public ApplicationField AF { get; set; }

	public string FormulaText => rtbFormula.Text;

	public bool HasStar
	{
		get
		{
			if (Context.Kind == FormulaContextKind.Cell || Context.Kind == FormulaContextKind.Column || Context.Kind == FormulaContextKind.HeaderCell)
			{
				return HasStarImpl(rtbFormula.Text, rtbFormula.SelectionStart);
			}
			return false;
		}
	}

	public event EventHandler ResizeRequest;

	public void SetFormulaText(string text)
	{
		try
		{
			pnlFormula.SuspendDrawing();
			rtbFormula.SuspendDrawing();
			rtbFormula.Text = text;
			RtbFormula_AddPardSBN();
			rtbFormula.Select(rtbFormula.Text.Length, 0);
		}
		finally
		{
			rtbFormula.ResumeDrawing();
			pnlFormula.ResumeDrawing();
		}
	}

	public void RemoveRefAtPos()
	{
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(rtbFormula.Text);
			Tuple<int, int> refAtPos = formulaDisplay.GetRefAtPos(rtbFormula.SelectionStart);
			if (refAtPos != null)
			{
				rtbFormula.Select(refAtPos.Item1, refAtPos.Item2 - refAtPos.Item1 + 1);
			}
		}
		catch (Auditai.Model.FormulaException)
		{
		}
	}

	public void InsertRefText(string text)
	{
		rtbFormula.SelectedText = text;
		rtbFormula.Focus();
	}

	public bool HasStarImpl(string formulaText, int pos)
	{
		if (pos == 0)
		{
			return true;
		}
		char c = formulaText[pos - 1];
		while (true)
		{
			switch (c)
			{
			case ' ':
				pos--;
				if (pos < 0)
				{
					return true;
				}
				break;
			default:
				if (c != '<')
				{
					try
					{
						FormulaDisplay formulaDisplay = new FormulaDisplay(formulaText);
						Tuple<string, int> funcNameAtPos = formulaDisplay.GetFuncNameAtPos(pos);
						if ("Union".Equals(funcNameAtPos.Item1, StringComparison.OrdinalIgnoreCase) || "Intersect".Equals(funcNameAtPos.Item1, StringComparison.OrdinalIgnoreCase) || "Except".Equals(funcNameAtPos.Item1, StringComparison.OrdinalIgnoreCase))
						{
							return Context.Table == Program.MainForm.TableEditor.Table;
						}
						if (_setUseStar.Contains(funcNameAtPos.Item1))
						{
							return false;
						}
					}
					catch (Auditai.Model.FormulaException)
					{
						return true;
					}
					return true;
				}
				goto case '=';
			case '=':
			case '>':
				return true;
			}
			c = formulaText[pos];
		}
	}

	public void SetFocus()
	{
		rtbFormula.Focus();
	}

	public bool IsInsideINDEX()
	{
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(rtbFormula.Text);
			Tuple<string, int> funcNameAtPos = formulaDisplay.GetFuncNameAtPos(rtbFormula.SelectionStart);
			return "Index".Equals(funcNameAtPos.Item1, StringComparison.InvariantCultureIgnoreCase) && funcNameAtPos.Item2 == 0;
		}
		catch (Auditai.Model.FormulaException)
		{
			return false;
		}
	}

	public FormulaEditor()
	{
		C1SplitContainer c1SplitContainer = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_pnlAll = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			KeepRelativeSize = false,
			Resizable = false,
			DoubleBuffered = true
		};
		c1SplitContainer.Panels.Add(_pnlAll);
		_containerSourceCell = new C1SplitContainer
		{
			Dock = DockStyle.None
		};
		_containerSelectorButton = new C1SplitContainer
		{
			Dock = DockStyle.None
		};
		_containerFormulaTextBox = new C1SplitContainer
		{
			Dock = DockStyle.None
		};
		_pnlAll.Controls.Add(_containerSourceCell);
		_pnlAll.Controls.Add(_containerSelectorButton);
		_pnlAll.Controls.Add(_containerFormulaTextBox);
		txtSourceCell = new C1TextBoxEx_SupportSelfBorder
		{
			Dock = DockStyle.Fill,
			AutoSize = false,
			ReadOnly = true,
			VerticalAlign = VerticalAlignEnum.Middle,
			WordWrap = false,
			ScrollBars = ScrollBars.None
		};
		_pnlSourceCell = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			KeepRelativeSize = false,
			Width = 200,
			Resizable = false,
			BackColor = Color.Transparent,
			BorderWidth = 0,
			DoubleBuffered = true
		};
		_pnlSourceCell.Controls.Add(txtSourceCell);
		_containerSourceCell.Panels.Add(_pnlSourceCell);
		btnSelector = new C1ButtonEx_SupportSelfBorder
		{
			Text = "函数",
			Dock = DockStyle.Fill
		};
		btnSelector.Click += Button_Click;
		_functionSelectDropDownForm = new FunctionSelector();
		drpSelector = new C1DropDownControl
		{
			DropDownForm = _functionSelectDropDownForm,
			BorderStyle = System.Windows.Forms.BorderStyle.None,
			Size = new Size(0, 0)
		};
		drpSelector.Enter += DropDown_Enter;
		drpSelector.DropDownClosed += DropDown_DropDownClosed;
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			KeepRelativeSize = false,
			Width = 50,
			Resizable = false,
			BackColor = Color.Transparent,
			BorderWidth = 0
		};
		c1SplitterPanel.Controls.Add(btnSelector);
		c1SplitterPanel.Controls.Add(drpSelector);
		_containerSelectorButton.Panels.Add(c1SplitterPanel);
		rtbFormula = new RichTextBoxEx
		{
			Dock = DockStyle.None,
			BorderStyle = System.Windows.Forms.BorderStyle.None,
			Multiline = true,
			ScrollBars = RichTextBoxScrollBars.None,
			ImeMode = ImeMode.NoControl,
			DetectUrls = false
		};
		rtbFormula.KeyDown += rtbFormula_KeyDown;
		rtbFormula.Enter += rtbFormula_Enter;
		rtbFormula.Leave += rtbFormula_Leave;
		rtbFormula.SelectionChanged += rtbFormula_SelectionChanged;
		rtbFormula.ContentsResized += RtbFormula_ContentsResized;
		rtbFormula.TextChanged += RtbFormula_TextChanged;
		pnlFormula = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			KeepRelativeSize = true,
			DoubleBuffered = true,
			BorderColor = GetFormulaEditorBorderColor(),
			BorderWidth = 1,
			AutoScroll = false
		};
		pnlFormula.Controls.Add(rtbFormula);
		_containerFormulaTextBox.Panels.Add(pnlFormula);
		tooltipBox.CloseClick += TooltipBox_CloseClick;
		View = c1SplitContainer;
		View.SizeChanged += Container_SizeChanged;
		AttachTooltip();
		ThemeManager.GetInstance().Register(this);
	}

	private Color GetFormulaEditorBorderColor()
	{
		if (Theme.SelectedAuditaiTheme == null)
		{
			return Color.Gray;
		}
		return Theme.SelectedAuditaiTheme.ThemeContext.FormulaEditorBorderColor;
	}

	private void RtbFormula_AddPardSBN()
	{
		_suspendTextChangeEvent = true;
		try
		{
			string text = "sb40\\";
			string text2 = "\\uc1\\pard\\";
			int num = rtbFormula.Rtf.IndexOf(text2);
			if (num > 0 && rtbFormula.Rtf.Substring(num + text2.Length, text.Length) != text)
			{
				string rtf = rtbFormula.Rtf.Insert(num + text2.Length, text);
				rtbFormula.Rtf = rtf;
			}
		}
		catch
		{
		}
		finally
		{
			_suspendTextChangeEvent = false;
		}
	}

	private void RtbFormula_RemovePardSBN()
	{
		_suspendTextChangeEvent = true;
		try
		{
			string text = "sb40\\";
			string text2 = "\\uc1\\pard\\";
			int num = rtbFormula.Rtf.IndexOf(text2);
			if (num > 0 && rtbFormula.Rtf.Substring(num + text2.Length, text.Length) == text)
			{
				string rtf = rtbFormula.Rtf.Remove(num + text2.Length, text.Length);
				rtbFormula.Rtf = rtf;
			}
		}
		catch
		{
		}
		finally
		{
			_suspendTextChangeEvent = false;
		}
	}

	private void RtbFormula_LostFocus(object sender, EventArgs e)
	{
		rtbFormula.Invalidate(new Rectangle(0, 0, rtbFormula.Width, 2));
	}

	private void Container_SizeChanged(object sender, EventArgs e)
	{
		if (Program.MainForm != null)
		{
			ReLayoutFormulaPanel(Program.MainForm.CurrentView);
		}
	}

	private void ReLayoutFormulaPanel(MainFormView view)
	{
		if (view != MainFormView.Empty)
		{
			int x = 1;
			if (view == MainFormView.TicketDesign)
			{
				x = 0;
			}
			_containerSourceCell.Width = 200;
			_containerSourceCell.Height = View.Height;
			_containerSourceCell.Location = new Point(x, 0);
			x = _containerSourceCell.Location.X + _containerSourceCell.Width + 2;
			_containerSelectorButton.Width = 50;
			_containerSelectorButton.Height = View.Height;
			_containerSelectorButton.Location = new Point(x, 0);
			x = _containerSelectorButton.Location.X + _containerSelectorButton.Width + 2;
			_containerFormulaTextBox.Width = View.Width - x;
			_containerFormulaTextBox.Height = View.Height;
			_containerFormulaTextBox.Location = new Point(x, 0);
			rtbFormula.Location = new Point(0, 0);
			rtbFormula.Width = _containerFormulaTextBox.Width;
			rtbFormula.Height = _containerFormulaTextBox.Height;
			int num = View.Height - drpSelector.Height;
			if (num < 0)
			{
				num = 0;
			}
			drpSelector.Location = new Point(0, num);
		}
	}

	private void TooltipBox_CloseClick(object sender, EventArgs e)
	{
		Program.MainForm.TtpCommentClosedShowInformation();
	}

	protected void DropDown_Enter(object sender, EventArgs e)
	{
		rtbFormula.Focus();
	}

	private void Button_Click(object sender, EventArgs e)
	{
		if (SoftwareLicenseManager.IsAllowEditFormula())
		{
			if (_functionSelectDropDownForm.CheckFunctionIsVisibleCallback == null)
			{
				_functionSelectDropDownForm.CheckFunctionIsVisibleCallback = Program.MainForm.IsAllowShowFunctionInfoInFunctionList;
			}
			_functionSelectDropDownForm.RefreshFuncList();
			drpSelector.OpenDropDown();
		}
	}

	private void rtbFormula_SelectionChanged(object sender, EventArgs e)
	{
		if (rtbFormula.Focused)
		{
			ShowFunctionHint();
		}
	}

	private void rtbFormula_Leave(object sender, EventArgs e)
	{
		Program.MainForm.HideFunctionHint();
	}

	private void rtbFormula_Enter(object sender, EventArgs e)
	{
		RaiseBeganEditing();
		ShowFunctionHint();
	}

	public void SetTheme()
	{
		Color formulaEditorBorderColor = Theme.SelectedAuditaiTheme.ThemeContext.FormulaEditorBorderColor;
		txtSourceCell.SelfBorderColor = formulaEditorBorderColor;
		btnSelector.BorderColor = formulaEditorBorderColor;
		pnlFormula.BorderColor = formulaEditorBorderColor;
		UpdatePanelColorToView(Program.MainForm.CurrentView);
		_functionSelectDropDownForm?.SetTheme();
	}

	public void SuspendSourceCellPanelDrawing()
	{
		txtSourceCell.SuspendDrawing();
		if (txtSourceCell.Parent != null)
		{
			txtSourceCell.Parent.SuspendDrawing();
		}
	}

	public void ResumeSourceCellPanelDrawing()
	{
		txtSourceCell.ResumeDrawing();
		if (txtSourceCell.Parent != null)
		{
			txtSourceCell.Parent.ResumeDrawing();
		}
	}

	public void UpdatePanelColorToView(MainFormView view)
	{
		ReLayoutFormulaPanel(view);
		Color backColor = Color.White;
		switch (view)
		{
		case MainFormView.Table:
			backColor = Color.White;
			break;
		case MainFormView.Document:
			if (Program.MainForm.CurrentDocumentEditor != null)
			{
				backColor = Color.White;
			}
			break;
		}
		foreach (C1SplitterPanel panel in View.Panels)
		{
			panel.BackColor = backColor;
		}
	}

	public void Commit()
	{
		try
		{
			switch (Context.Kind)
			{
			case FormulaContextKind.Cell:
				CommitInputForCell();
				break;
			case FormulaContextKind.Column:
				CommitInputForColumn();
				break;
			case FormulaContextKind.Document:
				CommitInputForDocument();
				break;
			case FormulaContextKind.Title:
				CommitInputForTitle();
				break;
			case FormulaContextKind.Foot:
				CommitInputForFoot();
				break;
			case FormulaContextKind.ColHeader:
				CommitInputForColHeader();
				break;
			case FormulaContextKind.HeaderCell:
				CommitInputForHeaderCell();
				break;
			case FormulaContextKind.TicketDesign:
				CommitInputForTicketDesign();
				break;
			default:
				_isCommitEventProcessed = false;
				RaiseFinishedEditing();
				break;
			}
		}
		catch (Exception ex)
		{
			ex.Log("提交编辑的公式时发生了未预期的异常");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
	}

	public void CancelEdit()
	{
		Program.MainForm.SuspendMainPanelDrawing();
		try
		{
			rtbFormula.Clear();
			RaiseFinishedEditing();
		}
		finally
		{
			Program.MainForm.ResumeMainPanelDrawing();
		}
	}

	private void rtbFormula_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.V)
		{
			try
			{
				if (Clipboard.ContainsText())
				{
					rtbFormula.SelectedText = Clipboard.GetText().Replace("\n", "").Replace("\r", "");
				}
			}
			catch (ExternalException)
			{
			}
			e.Handled = true;
		}
		_isCommitEventProcessed = true;
		_isPreviousContextRestoreSelect = true;
		switch (e.KeyCode)
		{
		case Keys.Return:
			e.Handled = true;
			Commit();
			if (!_isCommitEventProcessed)
			{
				rtbFormula.Clear();
				if (Program.MainForm.CurrentView == MainFormView.Table)
				{
					Program.MainForm.TableEditor._grid?.Select();
					Program.MainForm.TableEditor._grid?.SafeSelect(0, 0);
				}
				else if (Program.MainForm.CurrentView == MainFormView.Document)
				{
					Program.MainForm.CurrentDocumentEditor?._tx?.Focus();
				}
			}
			break;
		case Keys.Escape:
			e.Handled = true;
			CancelEdit();
			if (!_isPreviousContextRestoreSelect)
			{
				if (Program.MainForm.CurrentView == MainFormView.Table)
				{
					Program.MainForm.TableEditor._grid?.Select();
					Program.MainForm.TableEditor._grid?.SafeSelect(0, 0);
				}
				else if (Program.MainForm.CurrentView == MainFormView.Document)
				{
					Program.MainForm.CurrentDocumentEditor?._tx?.Focus();
				}
			}
			break;
		}
	}

	private void CommitInputForColHeader()
	{
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(text))
			{
				Context.Column.UpdateCaptionFormula(string.Empty);
			}
			else
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				Context.Column.UpdateCaptionFormula(formulaDisplay.ToFormula(Context));
				Context.Column.EvaluateCaptionFormula();
			}
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
	}

	private void CommitInputForTitle()
	{
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			string formula = "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				formula = formulaDisplay.ToFormula(Context);
			}
			Context.TitleOrFoot.Formula = formula;
			Context.TitleOrFoot.EvaluateFormula();
			Context.Table.TagTitleDirty();
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
	}

	private void CommitInputForFoot()
	{
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			string formula = "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				formula = formulaDisplay.ToFormula(Context);
			}
			Context.TitleOrFoot.Formula = formula;
			Context.TitleOrFoot.EvaluateFormula();
			Context.Table.TagFootDirty();
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
	}

	private void CommitInputForCell()
	{
		string formula = Context.Cell.Formula;
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			_owner.pnlGrid.SuspendDrawing();
			Context.Table.BeginBatchUpdateValue();
			string text2 = "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				text2 = formulaDisplay.ToFormula(Context);
			}
			Context.Cell.UpdateFormula(text2);
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
			Context.Cell.Formula = formula;
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
			Context.Cell.Formula = formula;
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
			Context.Cell.Formula = formula;
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
			Context.Cell.Formula = formula;
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
			Context.Cell.Formula = formula;
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
			Context.Cell.Formula = formula;
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
			Context.Cell.Formula = formula;
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
		finally
		{
			Context.Table.EndBatchUpdateValue();
			_owner.pnlGrid.ResumeDrawing();
		}
	}

	private void CommitInputForHeaderCell()
	{
		string headerFormula = Context.Cell.HeaderFormula;
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			_owner._grid.BeginUpdate();
			Context.Table.BeginBatchUpdateValue();
			string text2 = "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				text2 = formulaDisplay.ToFormula(Context);
			}
			Context.Cell.UpdateHeaderFormula(text2);
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
			Context.Cell.HeaderFormula = headerFormula;
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
			Context.Cell.HeaderFormula = headerFormula;
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
			Context.Cell.HeaderFormula = headerFormula;
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
			Context.Cell.HeaderFormula = headerFormula;
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
			Context.Cell.HeaderFormula = headerFormula;
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
			Context.Cell.HeaderFormula = headerFormula;
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
			Context.Cell.HeaderFormula = headerFormula;
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
		finally
		{
			Context.Table.EndBatchUpdateValue();
			_owner._grid.EndUpdate();
		}
	}

	private void CommitInputForColumn()
	{
		string formula = Context.Column.Formula;
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			_owner.pnlGrid.SuspendDrawing();
			Context.Table.BeginBatchUpdateValue();
			string text2 = "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				text2 = formulaDisplay.ToFormula(Context);
			}
			Context.Column.UpdateFormula(text2);
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
			Context.Column.Formula = formula;
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
			Context.Column.Formula = formula;
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
			Context.Column.Formula = formula;
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
			Context.Column.Formula = formula;
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
			Context.Column.Formula = formula;
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
			Context.Column.Formula = formula;
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
			Context.Column.Formula = formula;
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
		finally
		{
			Context.Table.EndBatchUpdateValue();
			_owner.pnlGrid.ResumeDrawing();
		}
	}

	private void CommitInputForDocument()
	{
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				if (AF != null)
				{
					int start = AF.Start - 1;
					Program.MainForm.CurrentDocumentEditor._tx.ApplicationFields.Remove(AF, keepText: true);
					Program.MainForm.CurrentDocumentEditor._tx.Select(start, 0);
					Program.MainForm.CurrentDocumentEditor.OnChanged();
				}
			}
			else
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				string text2 = formulaDisplay.ToFormula(Context);
				string text3 = EvaluateDocumentFormula(text2, calculateTable: false);
				if (AF == null)
				{
					ApplicationField applicationField = new ApplicationField(ApplicationFieldFormat.MSWord, "MERGEFIELD", text3, new string[3]
					{
						"Formula",
						text2,
						Auditai.Model.Project.Current.GetNextId().ToString()
					})
					{
						Deleteable = true,
						DoubledInputPosition = true,
						Editable = false,
						HighlightMode = Program.MainForm.CurrentDocumentEditor.GlobalHighlightMode ? HighlightMode.Activated : HighlightMode.Never,
						HighlightColor = Program.MainForm.CurrentDocumentEditor.GetFieldColor()
					};
					TextControlEx tx = Program.MainForm.CurrentDocumentEditor._tx;
					if (!((dynamic)tx.ApplicationFields).Add(applicationField))
					{
						tx.Selection.Text = " ";
						tx.Select(tx.Selection.Start, 0);
						((dynamic)tx.ApplicationFields).Add(applicationField);
						tx.Selection.Text = "";
						tx.Select(applicationField.Start - 1 + applicationField.Length, 0);
					}
				}
				else
				{
					ApplicationField aF = AF;
					aF.Text = text3;
					// 保留 Parameters[3]（稽核规则 JSON），避免编辑公式时丢失
					var refreshedParams = new List<string> { "Formula", text2, aF.Parameters[2] };
					if (aF.Parameters.Length >= 4) refreshedParams.Add(aF.Parameters[3]);
					aF.Parameters = refreshedParams.ToArray();
					Program.MainForm.CurrentDocumentEditor._tx.Select(aF.Start - 1 + aF.Length, 0);
					Program.MainForm.CurrentDocumentEditor.OnChanged();
				}
			}
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
	}

	public static string EvaluateDocumentFormula(string formula, bool calculateTable)
	{
		FormulaEvaluator formulaEvaluator = new FormulaEvaluator(formula);
		if (calculateTable)
		{
			HashSet<Id64> referredTableIds = formulaEvaluator.GetReferredTableIds();
			foreach (Id64 item in referredTableIds)
			{
				Auditai.Model.Project.Current?.GetTableById(item)?.CalculateRecursive();
			}
		}
		var currentProject = Auditai.Model.Project.Current;
		if (currentProject == null)
		{
			return "[无当前项目]";
		}
		formulaEvaluator.Env = new FormulaEvaluationEnvironment
		{
			Resolver = new FormulaReferenceModelResolver(currentProject),
			RefManager = currentProject.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = currentProject,
				CurrentTreeNode = Program.MainForm.ProjectHierarchy?.SelectedNode
			}
		};
		Operand operand = formulaEvaluator.EvaluateToOperand();
		string text = ((operand is NumberOperand { Value: var value }) ? value.ToString("#,##0.00") : ((!(operand is CellOperand cellOperand)) ? operand.ToString() : cellOperand.Cell.GetDisplayValue(applyZeroFormat: false)));
		if (string.IsNullOrEmpty(text))
		{
			text = "  ";
		}
		text = text.Replace("\r", "");
		return text.Replace("\n", "");
	}

	private void CommitInputForTicketDesign()
	{
		string text = rtbFormula.Text;
		if (text.Length > 10000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"公式长度不得超过{10000}字符");
			return;
		}
		try
		{
			string s = "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				FormulaDisplay formulaDisplay = new FormulaDisplay(text);
				s = formulaDisplay.ToTicketFormula(Context);
			}
			Context.TicketCell.UpdateFormula(s);
			RaiseFinishedEditing();
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
		}
		catch (FormulaSyntaxException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormula.Select(ex3.CharPosition, 0);
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
		}
		catch (FormulaNotApplicableException ex5)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
		}
		catch (FormulaBadValueException ex6)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			RaiseFinishedEditing();
		}
		catch (FormulaIgnorableException)
		{
			RaiseFinishedEditing();
		}
	}

	private void RtbFormula_ContentsResized(object sender, ContentsResizedEventArgs e)
	{
		try
		{
			_pnlAll.SuspendDrawing();
			DesiredHeight = e.NewRectangle.Height + 1;
			this.ResizeRequest?.Invoke(this, EventArgs.Empty);
		}
		finally
		{
			_pnlAll.ResumeDrawing();
		}
	}

	private void RtbFormula_TextChanged(object sender, EventArgs e)
	{
		if (_suspendTextChangeEvent)
		{
			return;
		}
		rtbFormula.SelectionChanged -= rtbFormula_SelectionChanged;
		rtbFormula.SuspendDrawing();
		int selectionStart = rtbFormula.SelectionStart;
		int selectionLength = rtbFormula.SelectionLength;
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(rtbFormula.Text);
			rtbFormula.SelectAll();
			rtbFormula.SelectionBackColor = Color.Transparent;
			rtbFormula.SelectionColor = Color.Black;
			foreach (Tuple<int, int, Color> tokenColorInterval in formulaDisplay.GetTokenColorIntervals())
			{
				rtbFormula.Select(tokenColorInterval.Item1, tokenColorInterval.Item2);
				rtbFormula.SelectionColor = tokenColorInterval.Item3;
			}
			Tuple<List<FormulaDisplayRef>, Color> references = formulaDisplay.GetReferences(Context);
			RefIntervals = references.Item1;
			NextColor = references.Item2;
			foreach (FormulaDisplayRef refInterval in RefIntervals)
			{
				foreach (Tuple<int, int> interval in refInterval.Intervals)
				{
					rtbFormula.Select(interval.Item1, interval.Item2);
					rtbFormula.SelectionColor = refInterval.Color;
				}
			}
		}
		catch (Auditai.Model.FormulaException)
		{
		}
		finally
		{
			rtbFormula.Select(selectionStart, selectionLength);
			rtbFormula.Refresh();
			rtbFormula.ResumeDrawing();
			_owner.Invalidate();
			_owner.TitleEditor.View.Invalidate();
			_owner.FootEditor.View.Invalidate();
			rtbFormula.SelectionChanged += rtbFormula_SelectionChanged;
		}
	}

	private void RtbFormula_Paint1(object sender, PaintEventArgs e)
	{
		e.Graphics.DrawLine(_formulaBorderPen, 0, 0, rtbFormula.Width, 0);
		e.Graphics.DrawLine(_formulaBorderPen, 0, 0, 0, rtbFormula.Height - 1);
		e.Graphics.DrawLine(_formulaBorderPen, rtbFormula.Width - 1, 0, rtbFormula.Width - 1, rtbFormula.Height - 1);
		e.Graphics.DrawLine(_formulaBorderPen, 0, rtbFormula.Height - 1, rtbFormula.Width, rtbFormula.Height - 1);
	}

	private void DropDown_DropDownClosed(object sender, DropDownClosedEventArgs e)
	{
		MethodInfo selectedFunction = _functionSelectDropDownForm.SelectedFunction;
		if (!(selectedFunction == null))
		{
			rtbFormula.SelectedText = selectedFunction.Name + "()";
			rtbFormula.Select(rtbFormula.SelectionStart - 1, 0);
		}
	}

	public void Populate()
	{
		switch (Context.Kind)
		{
		case FormulaContextKind.None:
			PopulateForNone();
			break;
		case FormulaContextKind.Cell:
			PopulateForCell();
			break;
		case FormulaContextKind.HeaderCell:
			PopulateForHeaderCell();
			break;
		case FormulaContextKind.Column:
			PopulateForColumn();
			break;
		case FormulaContextKind.Document:
			PopulateForDocument();
			break;
		case FormulaContextKind.Title:
			PopulateForTitle();
			break;
		case FormulaContextKind.Foot:
			PopulateForFoot();
			break;
		case FormulaContextKind.ColHeader:
			PopulateForColHeader();
			break;
		case FormulaContextKind.TicketDesign:
			PopulateForTicket();
			break;
		}
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			rtbFormula.Enabled = false;
		}
	}

	public void SetSourceCellValue(string value)
	{
		txtSourceCell.Value = value;
	}

	private void PopulateForNone()
	{
		SetSourceCellValue(string.Empty);
		SetFormulaText(string.Empty);
		rtbFormula.Enabled = false;
	}

	private void PopulateForCell()
	{
		SetSourceCellValue($"{{{Context.Table.GetCanonicalName()}}}[{Context.Cell.Column.GetUniqueFormulaName()},{Context.Cell.Row.Index + 1}]");
		if (!Context.Cell.HasFormula)
		{
			SetFormulaText(string.Empty);
		}
		else
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Context.Project);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Context.Cell.Formula);
				SetFormulaText(formulaEvaluator.GetDisplayString(resolver, Context.Table));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		rtbFormula.Enabled = true;
	}

	private void PopulateForHeaderCell()
	{
		SetSourceCellValue("{" + Context.Table.GetCanonicalName() + "}[" + Context.Cell.GetUniqueFormulaName() + "]");
		if (!Context.Cell.HasHeaderFormula)
		{
			SetFormulaText(string.Empty);
		}
		else
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Context.Project);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Context.Cell.HeaderFormula);
				SetFormulaText(formulaEvaluator.GetDisplayString(resolver, Context.Table));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		rtbFormula.Enabled = true;
	}

	private void PopulateForColumn()
	{
		SetSourceCellValue("{" + Context.Table.GetCanonicalName() + "}[" + Context.Column.GetUniqueFormulaName() + "]");
		if (!Context.Column.HasFormula)
		{
			SetFormulaText(string.Empty);
		}
		else
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Context.Project);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Context.Column.Formula);
				SetFormulaText(formulaEvaluator.GetDisplayString(resolver, Context.Table));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		rtbFormula.Enabled = true;
	}

	private void PopulateForTitle()
	{
		SetSourceCellValue($"Title({{{Context.Table.GetCanonicalName()}}},{Context.TitleOrFootRow + 1},{Context.TitleOrFootCol + 1})");
		if (string.IsNullOrEmpty(Context.TitleOrFoot.Formula))
		{
			SetFormulaText(string.Empty);
		}
		else
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Context.Project);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Context.TitleOrFoot.Formula);
				SetFormulaText(formulaEvaluator.GetDisplayString(resolver, Context.Table));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		rtbFormula.Enabled = true;
	}

	private void PopulateForFoot()
	{
		SetSourceCellValue($"Foot({{{Context.Table.GetCanonicalName()}}},{Context.TitleOrFootRow + 1},{Context.TitleOrFootCol + 1})");
		if (string.IsNullOrEmpty(Context.TitleOrFoot.Formula))
		{
			SetFormulaText(string.Empty);
		}
		else
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Context.Project);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Context.TitleOrFoot.Formula);
				SetFormulaText(formulaEvaluator.GetDisplayString(resolver, Context.Table));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		rtbFormula.Enabled = true;
	}

	private void PopulateForColHeader()
	{
		SetSourceCellValue("{" + Context.Table.GetCanonicalName() + "}[" + Context.Column.GetUniqueFormulaName() + ",0]");
		if (string.IsNullOrEmpty(Context.Column.CaptionFormula))
		{
			SetFormulaText(string.Empty);
		}
		else
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Context.Project);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Context.Column.CaptionFormula);
				SetFormulaText(formulaEvaluator.GetDisplayString(resolver, Context.Table));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		rtbFormula.Enabled = true;
	}

	private void PopulateForDocument()
	{
		if (Program.MainForm.CurrentDocumentEditor == null)
		{
			return;
		}
		SetSourceCellValue("{" + Program.MainForm.CurrentDocumentEditor.Document.TreeNode.FormulaUniqueName + "}");
		if (AF != null && Program.MainForm.CurrentDocumentEditor.HasValidFormula(AF))
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Auditai.Model.Project.Current);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Program.MainForm.CurrentDocumentEditor.GetFormulaFromAF(AF));
				SetFormulaText(formulaEvaluator.GetDisplayString(resolver));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		else
		{
			SetFormulaText(string.Empty);
		}
		rtbFormula.Enabled = true;
	}

	private void PopulateForTicket()
	{
		SetSourceCellValue(Program.MainForm.TicketDesignEditor.GetFormulaContextCellLabel());
		if (Context.TicketCell.HasFormula())
		{
			try
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Context.Project);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Context.TicketCell.Formula);
				SetFormulaText(formulaEvaluator.GetDisplayStringTicket(resolver, Context.DataRowStart, Context.DataRowCount, Context.Table, Context.Ticket));
			}
			catch (Auditai.Model.FormulaException)
			{
				SetFormulaText("[公式出错]");
			}
		}
		else
		{
			SetFormulaText("");
		}
		rtbFormula.Enabled = true;
	}

	private void ShowFunctionHint()
	{
		Program.MainForm.ShowFunctionHint(rtbFormula.Text, rtbFormula.SelectionStart);
		rtbFormula.Invalidate();
		rtbFormula.Update();
	}

	private void RaiseBeganEditing()
	{
		if (!IsEditing)
		{
			IsEditing = true;
			switch (Context.Kind)
			{
			case FormulaContextKind.Cell:
			case FormulaContextKind.Column:
			case FormulaContextKind.HeaderCell:
				Program.MainForm.TableEditor.OnFormulaEditorBeganEditing();
				break;
			case FormulaContextKind.Document:
				Program.MainForm.CurrentDocumentEditor.OnFormulaEditorBeganEditing();
				break;
			case FormulaContextKind.Title:
				Program.MainForm.TableEditor.TitleEditor.OnFormulaEditorBeganEditing();
				break;
			case FormulaContextKind.Foot:
				Program.MainForm.TableEditor.FootEditor.OnFormulaEditorBeganEditing();
				break;
			case FormulaContextKind.ColHeader:
				Program.MainForm.TableEditor.OnFormulaEditorBeganEditing_ColHeader();
				break;
			case FormulaContextKind.TicketDesign:
				Program.MainForm.TicketDesignEditor.OnFormulaEditorBeganEditing();
				break;
			}
			Program.MainForm.TableEditor.BeginFormula();
			this.ResizeRequest?.Invoke(this, EventArgs.Empty);
		}
	}

	private void RaiseFinishedEditing()
	{
		if (IsEditing)
		{
			IsFinishingEditing = true;
			FormulaContext formulaContext = Context.Clone();
			switch (formulaContext.Kind)
			{
			case FormulaContextKind.Cell:
			case FormulaContextKind.Column:
			case FormulaContextKind.HeaderCell:
				Program.MainForm.TableEditor.OnFormulaEditorFinishedEditing();
				break;
			case FormulaContextKind.Document:
				Program.MainForm.SuspendNavPanelVisible();
				try
				{
					Program.MainForm.CurrentDocumentEditor.OnFormulaEditorFinishedEditing();
					Program.MainForm.ProjectHierarchy.FindAndSelectNode(Program.MainForm.CurrentDocumentEditor.Document.TreeNode);
					Program.MainForm.SwitchToDocumentView();
					Program.MainForm.CurrentDocumentEditor._tx.Focus();
				}
				finally
				{
					Program.MainForm.ResumeNavPanelVisible();
				}
				break;
			case FormulaContextKind.Title:
			case FormulaContextKind.Foot:
				Program.MainForm.TableEditor.OnFormulaEditorFinishedEditing();
				break;
			case FormulaContextKind.ColHeader:
				Program.MainForm.TableEditor.OnFormulaEditorFinishedEditing();
				break;
			case FormulaContextKind.TicketDesign:
				Program.MainForm.TicketDesignEditor.OnFormulaEditorFinishedEditing();
				break;
			default:
				_isPreviousContextRestoreSelect = false;
				break;
			}
			Program.MainForm.TableEditor.EndFormula();
			IsEditing = false;
			IsFinishingEditing = false;
			switch (formulaContext.Kind)
			{
			case FormulaContextKind.Title:
				Program.MainForm.TableEditor.TitleEditor.Select(formulaContext.TitleOrFootRow, formulaContext.TitleOrFootCol);
				break;
			case FormulaContextKind.Foot:
				((dynamic)Program.MainForm.TableEditor.FootEditor).Select(formulaContext.TitleOrFootRow, formulaContext.TitleOrFootCol);
				break;
			}
		}
		else
		{
			_isPreviousContextRestoreSelect = false;
		}
	}

	public void AttachTooltip()
	{
		rtbFormula.MouseMove += delegate(object s1, MouseEventArgs e1)
		{
			if (tooltipManager.ShouldDisplay)
			{
				if (!IsEditing)
				{
					TipInfo tipInfo = tooltipManager.Get(rtbFormula);
					tooltipManager.Show(tipInfo, rtbFormula, e1.X, e1.Y);
				}
				else
				{
					tooltipManager.Hide();
				}
			}
		};
		rtbFormula.MouseLeave += delegate
		{
			tooltipManager.Hide();
		};
		tooltipManager.Attach(rtbFormula, TipInfo.Parse(TipResource.主窗体运算公式文本框));
	}
}
