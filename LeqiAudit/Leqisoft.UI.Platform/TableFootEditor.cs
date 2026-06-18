﻿﻿﻿﻿﻿﻿﻿using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class TableFootEditor : UserControl
{
	private readonly C1FlexGridEx _grid;
	private readonly TableEditor _owner;

	public AuxEditor AuxEditor { get; } = new AuxEditor();
	public C1FlexGridEx View => _grid;
	public TableFoot Foot { get; set; }
	public bool IsEditing { get; set; }

	public TableFootEditor()
	{
		_grid = new C1FlexGridEx
		{
			AllowSorting = AllowSortingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			ScrollBars = ScrollBars.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowEditing = true,
			VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom,
			DrawMode = DrawModeEnum.OwnerDraw,
			FocusRect = FocusRectEnum.None
		};
	}

	public TableFootEditor(TableEditor owner) : this()
	{
		_owner = owner;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 0;
		_grid.Select(-1, -1);
		_grid.Enter += _grid_Enter;
	}

	public void SetBold(bool v)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).Bold = v;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetItalic(bool v)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).Italic = v;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetAlign(CellTextAlign align)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).Align = align;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetBackColor(Color color)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).BackColor = color;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetForeColor(Color color)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).ForeColor = color;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetFontSize(float fontSize)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).FontSize = fontSize;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetFontFamily(string ff)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).FontFamily = ff;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void GrowFont()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		float fontSize = GetCell(selection.TopRow, selection.LeftCol).FontSize;
		fontSize = FontSize.Grow(fontSize);
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).FontSize = fontSize;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void ShrinkFont()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		float fontSize = GetCell(selection.TopRow, selection.LeftCol).FontSize;
		fontSize = FontSize.Shrink(fontSize);
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).FontSize = fontSize;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void Indent()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).Margin += 10;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void Unindent()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).Margin -= 10;
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void IncreaseRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			int maxRow = Math.Min(selection.BottomRow, Foot.Rows.Count - 1);
			for (int i = selection.TopRow; i <= maxRow; i++)
			{
				Foot.Rows[i].Height += 5;
			}
			Populate();
			_owner.DoLayout();
			Foot.Table.TagFootDirty();
		}
	}

	public void DecreaseRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			int maxRow = Math.Min(selection.BottomRow, Foot.Rows.Count - 1);
			for (int i = selection.TopRow; i <= maxRow; i++)
			{
				Foot.Rows[i].Height -= 5;
			}
			Populate();
			_owner.DoLayout();
			Foot.Table.TagFootDirty();
		}
	}

	public void UnifyRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			int num = selection.BottomRow - selection.TopRow + 1;
			int num2 = Foot.Rows.Skip(selection.TopRow).Take(num).Sum((TableTitleRow r) => r.Height);
			int height = num2 / num;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				Foot.Rows[i].Height = height;
			}
			Populate();
			Foot.Table.TagFootDirty();
		}
	}

	public void UnifyColumnWidth()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			int count = selection.RightCol - selection.LeftCol + 1;
			Foot.UnifyColumnWidth(selection.LeftCol, count);
			AdjustSize();
			Foot.Table.TagFootDirty();
		}
	}

	public void IncreaseColumnWidth()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			Foot.IncreaseColumnWidth(selection.LeftCol, selection.RightCol - selection.LeftCol + 1, 5, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
			_owner.pnlGrid.SuspendDrawing();
			Populate();
			_owner.PopulateColumns();
			_owner.pnlGrid.ResumeDrawing();
			Foot.Table.TagFootDirty();
		}
	}

	public void DecreaseColumnWidth()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			Foot.DecreaseColumnWidth(selection.LeftCol, selection.RightCol - selection.LeftCol + 1, 5, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
			_owner.pnlGrid.SuspendDrawing();
			Populate();
			_owner.PopulateColumns();
			_owner.pnlGrid.ResumeDrawing();
			Foot.Table.TagFootDirty();
		}
	}

	public void SetDataFormatText()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).DataFormat = new DataFormat(DataFormatType.General);
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetDataFormatNumeric(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft)
				{
					DecimalLength = 2
				};
				cell.Value = Leqisoft.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(double));
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
		if (dft == DataFormatType.Number || dft == DataFormatType.NumDollar || dft == DataFormatType.NumRmb || dft == DataFormatType.Percentage || dft == DataFormatType.Comma)
		{
			SetZeroFormat(ZeroFormat.Empty);
		}
	}

	public void SetDataFormatDate(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft);
				cell.Value = Leqisoft.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(DateTime));
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetDataFormatDateYearMonth(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft);
				cell.Value = Leqisoft.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(DateYearMonth));
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetDataFormatTime(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft);
				cell.Value = Leqisoft.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(TimeSpan));
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetDataFormatBoolean(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft)
				{
					DecimalLength = 2
				};
				cell.Value = Leqisoft.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(bool));
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void SetZeroFormat(ZeroFormat zf)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				DataFormat dataFormat = cell.DataFormat;
				dataFormat.ZeroFormat = zf;
				cell.DataFormat = dataFormat;
				cell.Value = Leqisoft.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(double));
			}
		}
		Populate();
		Foot.Table.TagFootDirty();
	}

	public void MorePrecision()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				DataFormat dataFormat = cell.DataFormat;
				if (dataFormat.IsNumericFormat())
				{
					dataFormat.DecimalLength++;
				}
				cell.DataFormat = dataFormat;
			}
		}
		_grid.Invalidate();
		Foot.Table.TagFootDirty();
	}

	public void LessPrecision()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				DataFormat dataFormat = cell.DataFormat;
				if (dataFormat.IsNumericFormat())
				{
					dataFormat.DecimalLength--;
				}
				cell.DataFormat = dataFormat;
			}
		}
		_grid.Invalidate();
		Foot.Table.TagFootDirty();
	}

	public void SetComboListDialog()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		TableTitleCell cell = GetCell(selection.TopRow, selection.LeftCol);
		AuxEditor.New();
		AuxEditor.View.AllowFreeInput = cell.IgnoreComboList;
		AuxEditor.View.AllowMultiSelect = cell.MultiComboList;
		AuxEditor.View.Value = "";
		if (!string.IsNullOrWhiteSpace(cell.ComboList))
		{
			try
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.ComboList);
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Foot.Table.Project);
				AuxEditor.View.Value = formulaEvaluator.GetDisplayString(resolver, Foot.Table);
			}
			catch (FormulaSyntaxException)
			{
				AuxEditor.View.Value = cell.ComboList;
			}
		}
		_owner.FormulaEditor.Context.Kind = FormulaContextKind.FootAuxEdit;
		_owner.FormulaEditor.Context.Table = Foot.Table;
		_owner.FormulaEditor.Context.TitleOrFoot = cell;
		_owner.FormulaEditor.View.Enabled = false;
		LeaveEdit();
		_grid.Select(selection);
		_owner.ToolBar.Enabled = false;
		_owner.BeginTitleFootAuxEdit();
		AuxEditor.Closed += AuxEditor_Closed;
		AuxEditor.ShowList(Program.MainForm.View);
	}

	private void AuxEditor_Closed(object sender, EventArgs e)
	{
		Program.MainForm.SuspendNavPanelDrawing();
		Program.MainForm.SuspendNavPanelVisible();
		_grid.SuspendDrawing();
		_owner.pnlGrid.SuspendDrawing();
		try
		{
			AuxEditor.Closed -= AuxEditor_Closed;
			_owner.EndTitleFootAuxEdit();
			if (AuxEditor.View.DialogResult == DialogResult.OK)
			{
				C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
				if (!selection.IsValid) return;
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					for (int j = selection.LeftCol; j <= selection.RightCol; j++)
					{
						TableTitleCell cell = GetCell(i, j);
						cell.IgnoreComboList = AuxEditor.View.AllowFreeInput;
						cell.MultiComboList = AuxEditor.View.AllowMultiSelect;
						cell.ComboList = AuxEditor.View.Value;
					}
				}
				Populate();
				Foot.Table.TagFootDirty();
			}
			_grid.Select();
		}
		finally
		{
			_grid.ResumeDrawing();
			_owner.pnlGrid.ResumeDrawing();
			Program.MainForm.ResumeNavPanelVisible();
			Program.MainForm.ResumeNavPanelDrawing();
		}
	}

	public void SetEditCommentDialog()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			TableTitleCell cell = GetCell(selection.TopRow, selection.LeftCol);
			AuxEditor.New();
			AuxEditor.View.CommentValue = cell.Comment;
			if (AuxEditor.ShowComment() == DialogResult.OK)
			{
				cell.Comment = AuxEditor.View.CommentValue;
				Foot.Table.TagFootDirty();
			}
		}
	}

	public void Populate()
	{
		if (Foot == null || Foot.Rows.Count == 0)
		{
			_grid.Rows.Count = 0;
			_grid.Cols.Count = 1;
			return;
		}
		_grid.BeginUpdate();
		_grid.AllowResizing = (_owner.HasSchemaPermission() ? AllowResizingEnum.Rows : AllowResizingEnum.None);
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		_grid.Rows.Count = Foot.Rows.Count;
		_grid.Cols.Count = Foot.Columns.Count;
		PopulateMerge();
		for (int i = 0; i < Foot.Rows.Count; i++)
		{
			TableTitleRow tableTitleRow = Foot.Rows[i];
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
			row.Height = tableTitleRow.Height;
			for (int j = 0; j < tableTitleRow.Cells.Count; j++)
			{
				PopulateCell(i, j, tableTitleRow.Cells[j]);
			}
		}
		try
		{
			_grid.Select(selection);
		}
		catch (IndexOutOfRangeException)
		{
		}
		_grid.EndUpdate();
	}

	private void PopulateMerge()
	{
		_grid.MergedRanges.Clear();
		foreach (TicketMerge merge in Foot.Merges)
		{
			if (merge.TopRow < 0 || merge.BottomRow >= Foot.Rows.Count || merge.LeftColumn < 0 || merge.RightColumn >= Foot.Columns.Count)
			{
				continue;
			}
			_grid.BodyAddMergedRange(merge.TopRow, merge.LeftColumn, merge.BottomRow, merge.RightColumn);
			for (int i = merge.TopRow; i <= merge.BottomRow; i++)
			{
				for (int j = merge.LeftColumn; j <= merge.RightColumn; j++)
				{
					_grid.BodyGetCell(i, j).StyleNew.DataType = null;
				}
			}
		}
	}

	private void PopulateCell(int row, int col, TableTitleCell cell)
	{
		C1.Win.C1FlexGrid.CellStyle styleNew = _grid.GetCellRange(row, col).StyleNew;
		FontStyle fontStyle = FontStyle.Regular;
		if (cell.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (cell.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (cell.Strikeout)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (cell.Underline)
		{
			fontStyle |= FontStyle.Underline;
		}
		styleNew.Font = new Font(cell.FontFamily ?? "微软雅黑", cell.FontSize, fontStyle);
		styleNew.ForeColor = cell.ForeColor;
		styleNew.TextAlign = C1FlexGridEx.ToTextAlign(cell.Align);
		styleNew.Margins = new System.Drawing.Printing.Margins(cell.Margin, 0, 0, 0);
		styleNew.WordWrap = true;
	}

	public void SetTheme()
	{
		_grid.Styles.Normal.BackColor = Color.Transparent;
	}

	public void LeaveEdit()
	{
		if (IsEditing)
		{
			Program.MainForm.SuspendNavPanelVisible();
			try
			{
				IsEditing = false;
				_grid.Select(-1, -1);
				_owner._grid.HighLight = HighLightEnum.Always;
				Program.MainForm.SwitchStateTo(MainFormView.Table);
				_owner.PopulateToolbar();
			}
			finally
			{
				Program.MainForm.ResumeNavPanelVisible();
			}
		}
	}

	public void FinishEditorInputStatus(bool isCancel)
	{
		try
		{
			if (_grid.Editor != null)
			{
				if (isCancel)
				{
					_grid.FinishEditing(cancel: true);
				}
				else if (!_grid.FinishEditing(cancel: false))
				{
					_grid.FinishEditing(cancel: true);
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log("结束表格表底区的编辑输入状态时发生了未预期的异常");
		}
	}

	public void EnterEdit()
	{
		_owner._grid.HighLight = HighLightEnum.WithFocus;
		IsEditing = true;
		Program.MainForm.SwitchStateTo(MainFormView.EditingFoot);
	}

	public void AdjustSize()
	{
		if (Program.MainForm != null && Foot != null && Foot.Columns.Count > 0)
		{
			TableEditor tableEditor = Program.MainForm.TableEditor;
			int gridWidth = tableEditor.GetGridWidth();
			gridWidth = Math.Min(gridWidth, tableEditor.GetPanelWidth());
			Foot.Resize(gridWidth);
			_grid.Width = gridWidth;
			for (int i = 0; i < Math.Min(Foot.Columns.Count, _grid.Cols.Count); i++)
			{
				_grid.Cols[i].Width = Foot.Columns[i].WidthDisplay;
			}
			_grid.Left = tableEditor._grid.Left;
		}
	}

	public int GetHeight()
	{
		int num = 0;
		for (int i = 0; i < _grid.Rows.Count; i++)
		{
			num += _grid.Rows[i].HeightDisplay;
		}
		return num;
	}

	public void AppendRow()
	{
		try
		{
			Foot.AppendRow(useNextRowStyle: true);
			Foot.Table.TagFootDirty();
			Populate();
			_owner.DoLayout();
		}
		catch (InvalidOperationException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void OnFormulaEditorBeganEditing()
	{
		if (IsEditing)
		{
			Program.MainForm.SuspendNavPanelVisible();
			try
			{
				IsEditing = false;
				Populate();
				Program.MainForm.SwitchStateTo(MainFormView.EditingFormula);
				AppCommands.Information.HideInformation();
			}
			finally
			{
				Program.MainForm.ResumeNavPanelVisible();
			}
		}
	}

	private TableTitleCell GetCell(int row, int col)
	{
		return Foot.GetCell(row, col);
	}

	private void _grid_Enter(object sender, EventArgs e)
	{
		if (IsEditing || _owner == null || _owner.Table == null)
		{
			return;
		}
		Program.MainForm.SuspendNavPanelVisible();
		try
		{
			if (_owner.TitleEditor.IsEditing)
			{
				_owner.TitleEditor.LeaveEdit();
			}
			else if (_owner._isEditingHeaders)
			{
				_owner.EndEditColHeaders();
			}
			EnterEdit();
		}
		finally
		{
			Program.MainForm.ResumeNavPanelVisible();
		}
	}
}
