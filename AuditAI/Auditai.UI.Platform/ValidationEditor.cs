using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class ValidationEditor
{
	private const string CN_ID = "Id";

	private const string CN_LEFT = "Left";

	private const string CN_REL = "Rel";

	private const string CN_RIGHT = "Right";

	private const string CN_NOTE = "Note";

	private TooltipManager tooltipManager = new TooltipManager();

	private readonly C1FlexGridEx _grid;

	private readonly FormulaGridEditor _editor;

	internal int _editingPosition;

	private readonly TableEditor _owner;

	private readonly C1ContextMenu ctxEmpty = new C1ContextMenu();

	private readonly C1ContextMenu ctxCell = new C1ContextMenu();

	private readonly C1ContextMenu ctxRow = new C1ContextMenu();

	private readonly C1CommandLink lnkAppend = new C1CommandLink();

	private readonly C1Command cmdAppend = new C1Command();

	private readonly C1CommandLink lnkAppend2 = new C1CommandLink();

	private readonly C1CommandLink lnkRemove = new C1CommandLink();

	private readonly C1Command cmdRemove = new C1Command();

	private readonly C1Command cmdCopy = new C1Command
	{
		Text = "复制行"
	};

	private readonly C1CommandLink lnkCopy = new C1CommandLink();

	private readonly C1Command cmdPasteNew = new C1Command
	{
		Text = "粘贴行"
	};

	private readonly C1Command cmdPasteOverwrite = new C1Command
	{
		Text = "粘贴行"
	};

	private readonly C1CommandLink lnkPaste = new C1CommandLink();

	private readonly C1CommandLink lnkAppend3 = new C1CommandLink();

	private readonly C1CommandLink lnkRemove2 = new C1CommandLink();

	private readonly C1CommandLink lnkPaste2 = new C1CommandLink();

	private readonly C1CommandLink lnkPaste3 = new C1CommandLink();

	internal bool _showFunctionHint = true;

	private bool _enabled;

	private AppCommandTab _selectedTab;

	internal FormulaContext Context => Program.MainForm.FormulaEditor.Context;

	public Table ContextTable { get; set; }

	public C1SplitContainer View { get; }

	public bool HasStar => _owner.FormulaEditor.HasStarImpl(_editor._rtf.Text, _editor._rtf.SelectionStart);

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			_grid.AllowEditing = _enabled;
		}
	}

	public bool IsEditing { get; set; }

	public C1FlexGridEx Grid => _grid;

	public event EventHandler BeganEditing;

	public event EventHandler FinishedEditing;

	public ValidationEditor(TableEditor owner)
	{
		_owner = owner;
		_editor = new FormulaGridEditor(this);
		View = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			SizeRatio = 100.0,
			Height = 100,
			KeepRelativeSize = false
		};
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowSorting = AllowSortingEnum.None,
			AllowDragging = AllowDraggingEnum.None,
			DrawMode = DrawModeEnum.OwnerDraw,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
		};
		_grid.Rows.DefaultSize = 30;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		_grid.BeforeEdit += _grid_BeforeEdit;
		_grid.StartEdit += _grid_StartEdit;
		_grid.AfterEdit += _grid_AfterEdit;
		_grid.KeyPress += _grid_KeyPress;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.MouseClick += _grid_MouseClick;
		_grid.Resize += _grid_Resize;
		_grid.KeyDown += _grid_KeyDown;
		_grid.Enter += _grid_Enter;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Name = "Id";
		column.Caption = "序号";
		column.Width = 40;
		_grid.Cols.Fixed = 1;
		column = _grid.Cols.Add();
		column.Name = "Note";
		column.Caption = "公式说明";
		column = _grid.Cols.Add();
		column.Name = "Left";
		column.Caption = "左校验对象";
		column.Editor = _editor;
		column = _grid.Cols.Add();
		column.Name = "Rel";
		column.Caption = "关系";
		column.Width = 40;
		column.DataType = typeof(ValidationOperator);
		column.DataMap = ValidationOperator.Operators.ToDictionary((ValidationOperator o) => o, (ValidationOperator o) => o.Display);
		column = _grid.Cols.Add();
		column.Name = "Right";
		column.Caption = "右校验对象";
		column.Editor = _editor;
		c1SplitterPanel.Controls.Add(_grid);
		View.Panels.Add(c1SplitterPanel);
		cmdPasteNew.CommandStateQuery += CmdPasteNew_CommandStateQuery;
		cmdPasteNew.Click += CmdPasteNew_Click;
		cmdAppend.CommandStateQuery += CmdAppend_CommandStateQuery;
		cmdAppend.Click += CmdAppend_Click;
		cmdRemove.CommandStateQuery += CmdRemove_CommandStateQuery;
		cmdRemove.Click += CmdRemove_Click;
		cmdPasteOverwrite.CommandStateQuery += CmdPasteOverwrite_CommandStateQuery;
		cmdPasteOverwrite.Click += CmdPasteOverwrite_Click;
		cmdCopy.CommandStateQuery += CmdCopy_CommandStateQuery;
		cmdCopy.Click += CmdCopy_Click;
		lnkAppend.Command = cmdAppend;
		ctxEmpty.CommandLinks.Add(lnkAppend);
		lnkPaste3.Command = cmdPasteNew;
		ctxEmpty.CommandLinks.Add(lnkPaste3);
		lnkAppend2.Command = cmdAppend;
		lnkRemove.Command = cmdRemove;
		lnkPaste.Command = cmdPasteOverwrite;
		lnkAppend3.Command = cmdAppend;
		ctxRow.CommandLinks.Add(lnkAppend3);
		lnkRemove2.Command = cmdRemove;
		ctxRow.CommandLinks.Add(lnkRemove2);
		lnkPaste2.Command = cmdPasteOverwrite;
		ctxRow.CommandLinks.Add(lnkPaste2);
		lnkCopy.Command = cmdCopy;
		ctxRow.CommandLinks.Add(lnkCopy);
		AttachTooltip();
	}

	private void _grid_BeforeEdit(object sender, RowColEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			e.Cancel = true;
		}
	}

	public void PopulateGrid()
	{
		_grid.Rows.Count = _grid.Rows.Fixed;
		FormulaReferenceModelResolver formulaReferenceModelResolver = new FormulaReferenceModelResolver(_owner.Table.Project);
		foreach (ValidationFormula item in _owner.Table.Project.ValidationManager.Formulas.Where((ValidationFormula f) => f.TableId == _owner.Table.Id))
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
			row.UserData = item;
			row["Left"] = GetDisplayString(item.LeftExpr);
			row["Note"] = item.Note;
			row["Rel"] = item.Operator;
			row["Right"] = GetDisplayString(item.RightExpr);
		}
		AutoSizeCols();
	}

	public void RemoveRefAtPos()
	{
		try
		{
			_grid.StartEditing();
			FormulaDisplay formulaDisplay = new FormulaDisplay(_editor._rtf.Text);
			Tuple<int, int> refAtPos = formulaDisplay.GetRefAtPos(_editor._rtf.SelectionStart);
			if (refAtPos != null)
			{
				_editor._rtf.Select(refAtPos.Item1, refAtPos.Item2 - refAtPos.Item1 + 1);
			}
		}
		catch (FormulaException)
		{
		}
	}

	public void InsertRefTextAndFocus(string text)
	{
		string name = _grid.Cols[_grid.Col].Name;
		if (name == "Left" || name == "Right")
		{
			_editor._rtf.SelectedText = text;
			_editor._rtf.Focus();
		}
	}

	public void SetTheme()
	{
		_grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		_grid.Cols["Note"].TextAlign = TextAlignEnum.LeftCenter;
		_grid.Cols["Left"].TextAlign = TextAlignEnum.LeftCenter;
		_grid.Cols["Rel"].TextAlign = TextAlignEnum.CenterCenter;
		_grid.Cols["Right"].TextAlign = TextAlignEnum.LeftCenter;
		_editor?.SetTheme();
	}

	private void _grid_StartEdit(object sender, RowColEventArgs e)
	{
		string name = _grid.Cols[e.Col].Name;
		if (!IsEditing && (name == "Left" || name == "Right"))
		{
			_selectedTab = Program.MainForm.State.SelectedTab;
			AppCommandTabs.Formula.Select();
			IsEditing = true;
			_grid.BeforeMouseDown += _grid_BeforeMouseDown;
			_showFunctionHint = true;
			this.BeganEditing?.Invoke(this, EventArgs.Empty);
		}
	}

	private void _grid_Enter(object sender, EventArgs e)
	{
		if (_owner.TitleEditor.IsEditing)
		{
			_owner.TitleEditor.LeaveEdit();
		}
		else if (_owner.FootEditor.IsEditing)
		{
			_owner.FootEditor.LeaveEdit();
		}
		else if (_owner._isEditingHeaders)
		{
			_owner.EndEditColHeaders();
		}
	}

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			e.Cancel = true;
		}
		else if (Commit())
		{
			_grid.BeforeMouseDown -= _grid_BeforeMouseDown;
		}
		else
		{
			e.Cancel = true;
		}
	}

	private void _grid_AfterEdit(object sender, RowColEventArgs e)
	{
		string name = _grid.Cols[e.Col].Name;
		ValidationFormula validationFormula = (ValidationFormula)_grid.Rows[e.Row].UserData;
		if (!(name == "Rel"))
		{
			if (name == "Note")
			{
				validationFormula.Note = _grid[e.Row, e.Col].ToString();
				validationFormula.IsDirty = true;
			}
		}
		else
		{
			validationFormula.Operator = (ValidationOperator)_grid[e.Row, e.Col];
			validationFormula.IsDirty = true;
		}
	}

	private void _grid_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (_enabled && e.KeyChar == '\r')
		{
			e.Handled = true;
			if (_grid.Row != _grid.Rows.Count - 1 && _grid.Row >= 0)
			{
				_grid.Row++;
			}
		}
	}

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		if (!_enabled || !SoftwareLicenseManager.IsAllowEditFormula())
		{
			return;
		}
		try
		{
			if (e.Control && e.KeyCode == Keys.V)
			{
				if (ClipboardManager.Instance.ValidationFormulas != null && _grid.BodyGetCol(_grid.BodyCol).Name == "Note")
				{
					PasteOverwrite();
					return;
				}
				string clipboardString = GetClipboardString();
				_grid.StartEditing();
				if (_grid.Col == _grid.Cols["Note"].Index)
				{
					_grid.Editor.Text = clipboardString;
					_grid.FinishEditing();
				}
				else if (_grid.Col == _grid.Cols["Left"].Index || _grid.Col == _grid.Cols["Right"].Index)
				{
					_editor._rtf.Text = clipboardString;
					Commit();
				}
			}
			else if (e.Control && e.KeyCode == Keys.C)
			{
				ClipboardManager.Instance.Clear();
				_grid.Copy();
				if (_grid.IsEntireRowSelected)
				{
					Copy();
				}
			}
			else
			{
				if (e.KeyCode != Keys.Delete || !_grid.Selection.IsValid)
				{
					return;
				}
				for (int i = _grid.Selection.TopRow; i <= _grid.Selection.BottomRow; i++)
				{
					for (int j = _grid.Selection.LeftCol; j <= _grid.Selection.RightCol; j++)
					{
						ValidationFormula validationFormula = (ValidationFormula)_grid.Rows[i].UserData;
						if (_grid.Cols[j].Name == "Note")
						{
							_grid[i, j] = string.Empty;
							validationFormula.Note = string.Empty;
						}
						else if (_grid.Cols[j].Name == "Left")
						{
							_grid[i, j] = string.Empty;
							validationFormula.LeftExpr = string.Empty;
						}
						else if (_grid.Cols[j].Name == "Right")
						{
							_grid[i, j] = string.Empty;
							validationFormula.RightExpr = string.Empty;
						}
					}
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row >= _grid.Rows.Fixed && e.Col == 0)
		{
			e.Text = (e.Row - _grid.Rows.Fixed + 1).ToString();
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (!_enabled || !SoftwareLicenseManager.IsAllowEditFormula() || e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		switch (hitTestInfo.Type)
		{
		case HitTestTypeEnum.None:
			ctxEmpty.ShowContextMenu(_grid, e.Location);
			break;
		case HitTestTypeEnum.RowHeader:
			ctxRow.ShowContextMenu(_grid, e.Location);
			break;
		case HitTestTypeEnum.Cell:
			ctxCell.CommandLinks.Clear();
			ctxCell.CommandLinks.Add(lnkAppend2);
			ctxCell.CommandLinks.Add(lnkRemove);
			if (_grid.Cols[hitTestInfo.Column].Name == "Note")
			{
				ctxCell.CommandLinks.Add(lnkPaste);
			}
			ctxCell.ShowContextMenu(_grid, e.Location);
			break;
		}
	}

	private void _grid_Resize(object sender, EventArgs e)
	{
		AutoSizeCols();
	}

	private void CmdAppend_Click(object sender, ClickEventArgs e)
	{
		ValidationFormula validationFormula = new ValidationFormula
		{
			Id = Project.Current.GetNextId(),
			Status = SyncStatus.New,
			IsDirty = false,
			LeftExpr = string.Empty,
			Note = string.Empty,
			Operator = ValidationOperator.FromCode(0),
			RightExpr = string.Empty,
			TableId = _owner.Table.Id
		};
		_owner.Table.Project.ValidationManager.Formulas.Add(validationFormula);
		C1.Win.C1FlexGrid.Row row = _grid.Rows.Add();
		row.UserData = validationFormula;
		row["Left"] = validationFormula.LeftExpr;
		row["Rel"] = validationFormula.Operator;
		row["Right"] = validationFormula.RightExpr;
		row["Note"] = validationFormula.Note;
	}

	private void CmdAppend_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAppend.Text = "新增行";
		cmdAppend.Image = ContextResources.ctxAppendRow;
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			cmdAppend.Visible = false;
		}
		else
		{
			cmdAppend.Visible = true;
		}
	}

	private void CmdRemove_Click(object sender, ClickEventArgs e)
	{
		if (_grid.Row == _grid.RowSel)
		{
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定删除本条校验公式吗?", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				ValidationFormula vf = (ValidationFormula)_grid.Rows[_grid.Row].UserData;
				_owner.Table.Project.ValidationManager.RemoveOne(vf);
				_grid.Rows.Remove(_grid.Row);
			}
			return;
		}
		int topRow = _grid.Selection.TopRow;
		int bottomRow = _grid.Selection.BottomRow;
		if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, $"确定删除选中的 {bottomRow - topRow + 1} 条校验公式吗?", MessageBoxButtons.OKCancel) == DialogResult.OK)
		{
			for (int i = topRow; i <= bottomRow; i++)
			{
				ValidationFormula vf2 = (ValidationFormula)_grid.Rows[i].UserData;
				_owner.Table.Project.ValidationManager.RemoveOne(vf2);
			}
			_grid.Rows.RemoveRange(topRow, bottomRow - topRow + 1);
		}
	}

	private void CmdRemove_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRemove.Text = "删除行";
		cmdRemove.Image = ContextResources.ctxDeleteRow;
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			cmdRemove.Visible = false;
		}
		else
		{
			cmdRemove.Visible = true;
		}
	}

	private void CmdPasteNew_Click(object sender, ClickEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			return;
		}
		CellRange selection = _grid.Selection;
		foreach (ValidationFormula validationFormula2 in ClipboardManager.Instance.ValidationFormulas)
		{
			ValidationFormula validationFormula = validationFormula2.Duplicate();
			validationFormula.Status = SyncStatus.New;
			validationFormula.TableId = ContextTable.Id;
			Project.Current.ValidationManager.Formulas.Add(validationFormula);
		}
		PopulateGrid();
		_grid.Select(selection.BottomRow + 1, 1);
	}

	private void CmdPasteNew_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = ClipboardManager.Instance.ValidationFormulas != null;
	}

	private void CmdPasteOverwrite_Click(object sender, ClickEventArgs e)
	{
		if (SoftwareLicenseManager.IsAllowEditFormula())
		{
			PasteOverwrite();
		}
	}

	private void CmdPasteOverwrite_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = ClipboardManager.Instance.ValidationFormulas != null;
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Copy();
	}

	private void CmdCopy_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = _grid.Row >= _grid.Rows.Fixed;
	}

	private void PasteOverwrite()
	{
		CellRange selection = _grid.Selection;
		List<ValidationFormula> validationFormulas = ClipboardManager.Instance.ValidationFormulas;
		for (int i = 0; i < validationFormulas.Count; i++)
		{
			ValidationFormula validationFormula = validationFormulas[i];
			int num = _grid.BodyRow + i;
			if (num >= 0 && num < _grid.BodyRowsCount)
			{
				ValidationFormula validationFormula2 = _grid.BodyGetRow(num).UserData as ValidationFormula;
				validationFormula2.LeftExpr = validationFormula.LeftExpr;
				validationFormula2.IsDirty = true;
				validationFormula2.Note = validationFormula.Note;
				validationFormula2.Operator = validationFormula.Operator;
				validationFormula2.RightExpr = validationFormula.RightExpr;
			}
			else
			{
				ValidationFormula validationFormula3 = validationFormula.Duplicate();
				validationFormula3.TableId = ContextTable.Id;
				validationFormula3.Status = SyncStatus.New;
				Project.Current.ValidationManager.Formulas.Add(validationFormula3);
			}
		}
		PopulateGrid();
		_grid.Select(selection);
	}

	private void Copy()
	{
		List<ValidationFormula> list = new List<ValidationFormula>();
		for (int i = _grid.BodySelection.TopRow; i <= _grid.BodySelection.BottomRow; i++)
		{
			ValidationFormula item = _grid.BodyGetRow(i).UserData as ValidationFormula;
			list.Add(item);
		}
		ClipboardManager.Instance.ValidationFormulas = list;
	}

	public bool Commit()
	{
		if (IsEditing)
		{
			try
			{
				_grid.FinishEditing();
				string name = _grid.Cols[_grid.Col].Name;
				ValidationFormula validationFormula = (ValidationFormula)_grid.Rows[_grid.Row].UserData;
				if (!(name == "Left"))
				{
					if (name == "Right")
					{
						string text = _grid[_grid.Row, _grid.Col].ToString();
						if (!string.IsNullOrEmpty(text))
						{
							FormulaDisplay formulaDisplay = new FormulaDisplay(text);
							validationFormula.RightExpr = formulaDisplay.ToFormula(Program.MainForm.FormulaEditor.Context);
						}
					}
				}
				else
				{
					string text = _grid[_grid.Row, _grid.Col].ToString();
					if (!string.IsNullOrEmpty(text))
					{
						FormulaDisplay formulaDisplay2 = new FormulaDisplay(text);
						validationFormula.LeftExpr = formulaDisplay2.ToFormula(Program.MainForm.FormulaEditor.Context);
					}
				}
				validationFormula.IsDirty = true;
				Program.MainForm.CurrentProject.FormulaManager.ReplaceHostValidation(validationFormula);
				AutoSizeCols();
				Program.MainForm.ValidateOne(validationFormula);
				_editingPosition = 0;
				RaiseFinishedEditing();
				IsEditing = false;
				return true;
			}
			catch (FormulaParameterCountException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数参数数量错误");
				_grid.StartEditing();
				return false;
			}
			catch (FormulaFunctionNotExistException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "函数不存在");
				_grid.StartEditing();
				return false;
			}
			catch (FormulaSyntaxException ex3)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "语法错误\n" + ex3.Message);
				_grid.StartEditing();
				_editor._rtf.Select(ex3.CharPosition, 0);
				return false;
			}
			catch (FormulaBadReferenceException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
				_grid.StartEditing();
				return false;
			}
			catch (FormulaNotApplicableException ex5)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
				_grid.StartEditing();
				return false;
			}
			catch (FormulaBadValueException ex6)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
				_grid.StartEditing();
				return false;
			}
			catch (FormulaTypeMismatchException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
				_grid.StartEditing();
				return false;
			}
			catch (FormulaColumnWildcardNoRowException)
			{
				RaiseFinishedEditing();
				IsEditing = false;
				return true;
			}
			catch (FormulaIgnorableException)
			{
				RaiseFinishedEditing();
				IsEditing = false;
				return true;
			}
		}
		return true;
	}

	private void RaiseFinishedEditing()
	{
		this.FinishedEditing?.Invoke(this, EventArgs.Empty);
		_selectedTab?.Select();
	}

	public void Cancel()
	{
		if (!IsEditing)
		{
			return;
		}
		_grid.FinishEditing(cancel: true);
		string name = _grid.Cols[_grid.Col].Name;
		ValidationFormula validationFormula = (ValidationFormula)_grid.Rows[_grid.Row].UserData;
		if (!(name == "Left"))
		{
			if (name == "Right")
			{
				_grid[_grid.Row, _grid.Col] = GetDisplayString(validationFormula.RightExpr);
			}
		}
		else
		{
			_grid[_grid.Row, _grid.Col] = GetDisplayString(validationFormula.LeftExpr);
		}
		validationFormula.IsDirty = true;
		AutoSizeCols();
		_editingPosition = 0;
		RaiseFinishedEditing();
		IsEditing = false;
	}

	private string GetDisplayString(string formula)
	{
		if (string.IsNullOrWhiteSpace(formula))
		{
			return string.Empty;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Program.MainForm.CurrentProject);
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(formula);
			return formulaEvaluator.GetDisplayString(resolver);
		}
		catch (FormulaException)
		{
			return "[公式出错]";
		}
	}

	private void AutoSizeCols()
	{
		int minWidth = (_grid.Width - _grid.Cols["Id"].Width - _grid.Cols["Rel"].Width) / 3;
		AdjustWidth("Left");
		AdjustWidth("Note");
		AdjustWidth("Right");
		void AdjustWidth(string col)
		{
			_grid.AutoSizeCol(_grid.Cols[col].Index);
			if (_grid.Cols[col].Width < minWidth)
			{
				_grid.Cols[col].Width = minWidth;
			}
		}
	}

	public void AttachTooltip()
	{
		TipInfo tip = TipInfo.Parse(TipResource.校验公式文本框);
		_grid.MouseMove += delegate(object s1, MouseEventArgs e1)
		{
			if (tooltipManager.ShouldDisplay)
			{
				tooltipManager.Show(tip, _grid, e1.X, e1.Y);
			}
		};
		_grid.MouseLeave += delegate
		{
			tooltipManager.Hide();
		};
	}

	private string GetClipboardString()
	{
		try
		{
			string text = Clipboard.GetText();
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			if (text.EndsWith("\r\n"))
			{
				text = text.Substring(0, text.Length - 2);
			}
			if (text.Contains("\r\n") || text.Contains("\t"))
			{
				string[] array = text.Split(new string[1] { "\r\n" }, StringSplitOptions.None);
				return ClipboardUtil.GetValueFromPaste(array[0].Split('\t')[0]);
			}
			return text;
		}
		catch (ExternalException)
		{
			return string.Empty;
		}
	}
}
