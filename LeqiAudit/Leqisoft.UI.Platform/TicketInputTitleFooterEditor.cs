﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class TicketInputTitleFooterEditor : ISetTheme
{
	public enum EditorType
	{
		Title,
		Footer
	}

	private class GridResizeManager : GridResizingManager
	{
		protected TicketInputTitleFooterEditor _parent;

		public GridResizeManager(TicketInputTitleFooterEditor parent, C1FlexGridEx grid)
			: base(grid)
		{
			_parent = parent;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			_parent._grid_MouseMove(_grid, e);
		}
	}

	private static readonly C1TextBoxEx _dateEdit;

	private static readonly C1TextBoxEx _timeEdit;

	private C1FlexGridEx _grid;

	internal TicketInputTitleFooterVM _vm;

	private C1ContextMenu _ctxCell;

	private bool _isInEditing;

	private bool _isMouseOverCancelManualInputIcon;

	private static SolidBrush _cancelManualInputBackgroundBrush;

	private static readonly StringFormat _sf;

	private TooltipBox _ttpComment = new TooltipBox
	{
		Opacity = 0.8,
		IsBalloon = true
	};

	private Color _colorBorder;

	private TicketInputEditor2 _owner;

	private EditorType _editor_type;

	private GridResizingManager _gridResizingManager;

	private bool _isSuspendBodySelectionChangeEvent;

	public Leqisoft.Model.Table Table => _owner.Table;

	public TicketTable Ticket => Table.Ticket;

	public C1FlexGridEx View => _grid;

	public bool HasFillingFormula { get; set; }

	public ListDropDown ListDropDown { get; private set; }

	public InputListDropDown InputListDropDown { get; private set; }

	public TicketInputTitleFooterVM VMData
	{
		get
		{
			return _vm;
		}
		set
		{
			_vm = value;
		}
	}

	public bool IsInEditing => _isInEditing;

	protected TicketTitleFooter EditorSetting
	{
		get
		{
			if (_editor_type == EditorType.Title)
			{
				return Ticket.Title;
			}
			return Ticket.Footer;
		}
	}

	static TicketInputTitleFooterEditor()
	{
		_dateEdit = new C1TextBoxEx();
		_timeEdit = new C1TextBoxEx();
		_cancelManualInputBackgroundBrush = new SolidBrush(Color.Gray);
		_sf = new StringFormat
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		};
		_dateEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.ErrorInfo.ShowErrorMessage = false;
	}

	public TicketInputTitleFooterEditor(TicketInputEditor2 parent, EditorType editorType)
	{
		_owner = parent;
		_editor_type = editorType;
		InitTableGrid();
		InitContextMenu();
	}

	private void InitTableGrid()
	{
		_grid = new C1FlexGridEx
		{
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowMergingFixed = AllowMergingEnum.Custom,
			AllowResizing = AllowResizingEnum.Both,
			AllowSorting = AllowSortingEnum.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			ScrollBars = ScrollBars.None,
			Cursor = TableEditor.CursorTable,
			Dock = DockStyle.None,
			FocusRect = FocusRectEnum.None,
			AutoClipboard = false
		};
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 0;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.BodyBeforeEdit += _grid_BodyBeforeEdit;
		_grid.BodyAfterEdit += _grid_BodyAfterEdit;
		_grid.BodySetupEditor += _grid_BodySetupEditor;
		_grid.BodyStartEdit += _grid_BodyStartEdit;
		_grid.BodyValidateEdit += _grid_BodyValidateEdit;
		_grid.KeyDown += _grid_KeyDown;
		_grid.Paint += _grid_Paint;
		_grid.BodyAfterRowColChange += _grid_BodyAfterRowColChange;
		_grid.MouseClick += _grid_MouseClick;
		_grid.BeforeMouseDown += _grid_BeforeMouseDown;
		_grid.MouseMove += _grid_MouseMove;
		_grid.BodySelectionChanged += _grid_BodySelectionChanged;
		_grid.Enter += _grid_Enter;
		_gridResizingManager = new GridResizeManager(this, _grid);
		_gridResizingManager.ResizeColumn += _gridResizingManager_ResizeColumn;
		_gridResizingManager.ResizeRow += _gridResizingManager_ResizeRow;
	}

	private void InitContextMenu()
	{
		_ctxCell = new C1ContextMenu();
		C1Command c1Command = new C1Command();
		c1Command.Text = "复制";
		c1Command.Image = ContextResources.ctxCopy;
		c1Command.Click += _cmdCopy_Click;
		c1Command.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command));
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "剪切";
		c1Command2.Image = ContextResources.ctxCut;
		c1Command2.Click += _cmdCut_Click;
		c1Command2.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell() && !_owner.IsTicketLocked;
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command2));
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "粘贴";
		c1Command3.Image = ContextResources.ctxPaste;
		c1Command3.Click += _cmdPaste_Click;
		c1Command3.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell() && !_owner.IsTicketLocked;
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command3));
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "设置行高...";
		c1Command4.Click += _cmdRowHeight_Click;
		c1Command4.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command4)
		{
			Delimiter = true
		});
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "设置列宽...";
		c1Command5.Click += _cmdColumnWidth_Click;
		C1Command c1Command6 = new C1Command();
		c1Command6.Text = "插入单元格附件";
		c1Command6.Click += _cmdAddAttachment_Click;
		c1Command6.CommandStateQuery += _cmdAddAttachment_CommandStateQuery;
		ListDropDown = new ListDropDown(_grid);
		InputListDropDown = new InputListDropDown(_grid);
		ListDropDown.DropDown.ButtonCursor = Cursors.Arrow;
		InputListDropDown.DropDown.ButtonCursor = Cursors.Arrow;
		ThemeManager.GetInstance().Register(this);
		SetTheme();
	}

	public void Invalidate()
	{
		_grid.Invalidate();
	}

	public void GridBeginUpdate()
	{
		_grid.BeginUpdate();
	}

	public void GridEndUpdate()
	{
		_grid.EndUpdate();
	}

	public int GetAllColumnsTotalWidth()
	{
		int num = 0;
		int count = _grid.Cols.Count;
		for (int i = 0; i < count; i++)
		{
			num += _grid.Cols[i].WidthDisplay;
		}
		return num;
	}

	public int GetAllRowsTotalHeight()
	{
		int num = 0;
		int count = _grid.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			num += _grid.Rows[i].HeightDisplay;
		}
		return num;
	}

	public void LeaveEdit()
	{
		_isInEditing = false;
		CancelSelect();
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
			exception.Log("结束表单的编辑输入状态时发生了未预期的异常");
		}
	}

	public void CancelSelect()
	{
		_grid.Select(-1, -1);
	}

	public void Populate()
	{
		_grid.AllowEditing = Table?.IsLocked != true;
		HasFillingFormula = _owner.HasFillingFormula;
		GridBeginUpdate();
		try
		{
			PopulateVmWithoutReDrawing();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	protected void PopulateVM()
	{
		GridBeginUpdate();
		try
		{
			PopulateVmWithoutReDrawing();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	public void PopulateVmWithoutReDrawing()
	{
		int count = _vm.Rows.Count;
		int count2 = _vm.Columns.Count;
		_isSuspendBodySelectionChangeEvent = true;
		_grid.Rows.Count = 0;
		_grid.Cols.Count = 0;
		_grid.BodyRowsCount = count;
		_grid.BodyColsCount = count2;
		_isSuspendBodySelectionChangeEvent = false;
		for (int i = 0; i < count; i++)
		{
			_grid.BodyGetRow(i).Height = _vm.GetRowHeight(i);
		}
		for (int j = 0; j < count2; j++)
		{
			_grid.BodyGetCol(j).Width = _vm.GetColumnWidth(j);
		}
		_grid.MergedRanges.Clear();
		foreach (TicketMerge merge in _vm.Merges)
		{
			_grid.BodyAddMergedRange(merge.TopRow, merge.LeftColumn, merge.BottomRow, merge.RightColumn);
			for (int k = merge.TopRow; k <= merge.BottomRow; k++)
			{
				for (int l = merge.LeftColumn; l <= merge.RightColumn; l++)
				{
					_grid.BodyGetCell(k, l).StyleNew.DataType = null;
				}
			}
		}
		AutoAdjustGridHeight();
		_grid.Invalidate();
	}

	public void SetTheme()
	{
		Theme.SetCurrentObject(View);
		_grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_grid.Styles.Normal.WordWrap = true;
		_colorBorder = Theme.SelectedLeqiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
		Theme.SetCurrentObject(ListDropDown.DropDown);
	}

	public void Cut()
	{
		if (_owner.IsTicketLocked || IsPreventCutOrDeleteCellValue())
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		_owner._vm.BeginBatchUpdateValue();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
				stringBuilder.Append(cellVM.IsShowVirtualValue ? cellVM.VirtualValue : cellVM.GetDisplayValue());
				if (Table?.IsLocked != true && CanEditCell(cellVM))
			{
				_owner.ChangeVirtualValueToRealValue();
				_owner._vm.UpdateTicketCellValue(cellVM, "", isFormulaExistManualInputValue: true);
			}
				if (j < bodySelection.RightCol)
				{
					stringBuilder.Append("\t");
				}
			}
			stringBuilder.Append("\r\n");
		}
		_owner._vm.EndBatchUpdateValue();
		_owner._vm.CalculateTicket();
		Clipboard.SetText(stringBuilder.ToString());
		_grid.Invalidate();
	}

	public void Copy()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
				stringBuilder.Append(cellVM.IsShowVirtualValue ? cellVM.VirtualValue : cellVM.GetDisplayValue());
				if (j < bodySelection.RightCol)
				{
					stringBuilder.Append("\t");
				}
			}
			stringBuilder.Append("\r\n");
		}
		try
		{
			Clipboard.SetText(stringBuilder.ToString());
		}
		catch
		{
		}
	}

	private bool IsExistSelectedCell()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return false;
		}
		return true;
	}

	public void Paste()
	{
		if (Table?.IsLocked == true || _owner.IsTicketLocked || !IsExistSelectedCell())
		{
			return;
		}
		try
		{
			List<List<object>> clipboardAsTable = ClipboardUtil.GetClipboardAsTable();
			if (clipboardAsTable != null && clipboardAsTable.Count > 0)
			{
				int count = clipboardAsTable.Count;
				int count2 = clipboardAsTable[0].Count;
				int bodyRow = _grid.BodyRow;
				int num = Math.Min(Math.Max(count, _grid.BodyRowSel - bodyRow + 1), _vm.GetRowsCount() - bodyRow);
				int bodyCol = _grid.BodyCol;
				int num2 = Math.Min(Math.Max(count2, _grid.BodyColSel - bodyCol + 1), _vm.GetColumnsCount() - bodyCol);
				_owner._vm.BeginBatchUpdateValue();
				for (int i = 0; i < num; i++)
				{
					List<object> list = clipboardAsTable[i % count];
					for (int j = 0; j < num2; j++)
					{
						object obj = list[j % count2];
						TicketInputCellVM cellVM = _vm.GetCellVM(bodyRow + i, bodyCol + j);
						if (!CanEditCell(cellVM))
						{
							continue;
						}
						_owner.ChangeVirtualValueToRealValue();
						if (cellVM.Column != null)
						{
							try
							{
								Type displayDataType = cellVM.TempCell.DisplayDataType;
								object newValue = TicketInputEditor2.ConvertCopyValueToCellValue(obj, displayDataType);
								_owner._vm.UpdateTicketCellValue(cellVM, newValue, isFormulaExistManualInputValue: true);
							}
							catch
							{
							}
						}
						else if (cellVM.DataFormat.HasValue)
						{
							try
							{
								Type dataType = cellVM.DataFormat.Value.GetDataType();
								object newValue2 = TicketInputEditor2.ConvertCopyValueToCellValue(obj, dataType);
								_owner._vm.UpdateTicketCellValue(cellVM, newValue2, isFormulaExistManualInputValue: true);
							}
							catch
							{
							}
						}
						else
						{
							_owner._vm.UpdateTicketCellValue(cellVM, obj, isFormulaExistManualInputValue: true);
						}
					}
				}
				_owner._vm.EndBatchUpdateValue();
				try
				{
					_grid.BodySelect(bodyRow, bodyCol, bodyRow + num - 1, bodyCol + num2 - 1);
				}
				catch (Exception exception)
				{
					exception.Log("表单的标题/表底区输入界面粘贴单元格时发生了未预期的异常");
				}
			}
			_owner.VMData.CalculateTicket();
			_grid.Invalidate();
			_owner.SetInputDataDirty();
		}
		catch (TableModelException ex)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void IncreaseColumnWidth()
	{
		for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
		{
			EditorSetting.Columns[i].Width += 5;
		}
		Table.TagTicketDirty(isCanOverrideByServerData: true);
		GridBeginUpdate();
		try
		{
			_owner.CacheSelectRange();
			AdjustColWidthByPercent();
			_owner.RestoreSelectRange();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	public void DecreaseColumnWidth()
	{
		for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
		{
			EditorSetting.Columns[i].Width -= 5;
		}
		Table.TagTicketDirty(isCanOverrideByServerData: true);
		GridBeginUpdate();
		try
		{
			_owner.CacheSelectRange();
			AdjustColWidthByPercent();
			_owner.RestoreSelectRange();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	public void IncreaseRowHeight()
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			EditorSetting.Rows[i].Height += 5;
		}
		Table.TagTicketDirty(isCanOverrideByServerData: true);
		_owner.SuspendEditorPanelDrawing();
		GridBeginUpdate();
		try
		{
			_owner.CacheSelectRange();
			PopulateVmWithoutReDrawing();
			AutoAdjustGridHeight();
			_owner.AutoAdjustInputGridPosition();
			_owner.RestoreSelectRange();
		}
		finally
		{
			GridEndUpdate();
			_owner.ResumeEditorPanelDrawing();
		}
	}

	public void DecreaseRowHeight()
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			EditorSetting.Rows[i].Height -= 5;
		}
		Table.TagTicketDirty(isCanOverrideByServerData: true);
		_owner.SuspendEditorPanelDrawing();
		GridBeginUpdate();
		try
		{
			_owner.CacheSelectRange();
			PopulateVmWithoutReDrawing();
			AutoAdjustGridHeight();
			_owner.AutoAdjustInputGridPosition();
			_owner.RestoreSelectRange();
		}
		finally
		{
			GridEndUpdate();
			_owner.ResumeEditorPanelDrawing();
		}
	}

	public void SetRowsHeight()
	{
		int height = EditorSetting.Rows[_grid.BodyRow].Height;
		decimal? num = InputForm.Numeric("设置行高", "请输入行高，以像素为单位：", height);
		if (!num.HasValue)
		{
			return;
		}
		int num2 = (int)num.Value;
		if (num2 < 1)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能小于1");
			return;
		}
		if (num2 > 9999)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能大于9999");
			return;
		}
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			EditorSetting.Rows[i].Height = num2;
			_grid.Rows[i].Height = num2;
		}
		Table.TagTicketDirty(isCanOverrideByServerData: true);
		_owner.SuspendEditorPanelDrawing();
		GridBeginUpdate();
		try
		{
			_owner.CacheSelectRange();
			AutoAdjustGridHeight();
			_owner.AutoAdjustInputGridPosition();
			_owner.RestoreSelectRange();
		}
		finally
		{
			GridEndUpdate();
			_owner.ResumeEditorPanelDrawing();
		}
	}

	public void SetColumnsWidth()
	{
		int columnWidth = _vm.GetColumnWidth(_grid.BodyCol);
		decimal? num = InputForm.Numeric("设置列宽", "请输入列宽，以像素为单位：", columnWidth);
		if (!num.HasValue)
		{
			return;
		}
		int num2 = (int)num.Value;
		if (num2 < 1)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列宽不能小于1");
		}
		else if (num2 > 9999)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列宽不能大于9999");
		}
		else
		{
			if (_vm.Columns.Count <= 1)
			{
				return;
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			_owner.SuspendEditorPanelDrawing();
			GridBeginUpdate();
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			try
			{
				_owner.CacheSelectRange();
				for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
				{
					EditorSetting.Columns[i].Width = num2;
					_grid.Cols[i].Width = num2;
				}
				SetColToWidthWithoutForceReDrawing(bodySelection.LeftCol, bodySelection.RightCol);
				_owner.AutoAdjustInputGridPosition();
				_owner.RestoreSelectRange();
			}
			finally
			{
				GridEndUpdate();
				_owner.ResumeEditorPanelDrawing();
			}
		}
	}

	protected void SetColToWidthWithoutForceReDrawing(int fixedColStartIndex, int fixedColEndIndex)
	{
		int ticketBodyGridControlWidthWithoutFixedColumnWidth = GetTicketBodyGridControlWidthWithoutFixedColumnWidth();
		AdjustGridWidthImpl(ticketBodyGridControlWidthWithoutFixedColumnWidth, fixedColStartIndex, fixedColEndIndex);
	}

	private void AdjustColWidthByPercent()
	{
		TicketTitleFooter editorSetting = EditorSetting;
		int count = editorSetting.Columns.Count;
		int[] array = new int[count];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			int width = editorSetting.Columns[i].Width;
			num += width;
			array[i] = width;
		}
		SplitValueWithByPercent(_grid.Width, array, 0, count - 1);
		for (int j = 0; j < count; j++)
		{
			_grid.Cols[j].Width = array[j];
			editorSetting.Columns[j].Width = array[j];
		}
	}

	private void AdjustGridWidthImpl(int gridWillToWidth, int fixedColStartIndex, int fixedColEndIndex)
	{
		TicketTitleFooter editorSetting = EditorSetting;
		int count = editorSetting.Columns.Count;
		int[] array = new int[count];
		int num = 0;
		for (int i = 0; i < fixedColStartIndex; i++)
		{
			int width = editorSetting.Columns[i].Width;
			num += width;
			array[i] = width;
		}
		int num2 = 0;
		for (int j = fixedColStartIndex; j <= fixedColEndIndex; j++)
		{
			int width2 = editorSetting.Columns[j].Width;
			num2 += width2;
			array[j] = width2;
		}
		int num3 = 0;
		for (int k = fixedColEndIndex + 1; k < count; k++)
		{
			num3 += (array[k] = editorSetting.Columns[k].Width);
		}
		if (fixedColStartIndex == 0 && fixedColEndIndex == count - 1)
		{
			SplitValueWithByPercent(gridWillToWidth, array, 0, count - 1);
		}
		else if (num + num2 > gridWillToWidth)
		{
			if (num2 < gridWillToWidth)
			{
				int num4 = gridWillToWidth - num2;
				int num5 = (int)((float)num3 * 1f / (float)(num + num2 + num3) * (float)num4);
				int newTotalValue = num4 - num5;
				if (fixedColStartIndex > 0)
				{
					SplitValueWithByPercent(newTotalValue, array, 0, fixedColStartIndex);
				}
				if (fixedColEndIndex < count - 1)
				{
					SplitValueWithByPercent(num5, array, fixedColEndIndex + 1, count - 1);
				}
			}
			else
			{
				SplitValueWithByPercent(gridWillToWidth, array, 0, count - 1);
			}
		}
		else
		{
			int num6 = gridWillToWidth - num - num2;
			if (fixedColEndIndex < count - 1)
			{
				SplitValueWithByPercent(num6, array, fixedColEndIndex + 1, count - 1);
			}
			else
			{
				array[0] += num6;
			}
		}
		for (int l = 0; l < count; l++)
		{
			_grid.Cols[l].Width = array[l];
			editorSetting.Columns[l].Width = array[l];
		}
		_grid.Width = gridWillToWidth;
	}

	public void HideTooltip()
	{
		_ttpComment.Hide();
	}

	public void SaveGridCurrentWidthToTicket()
	{
		TicketTitleFooter editorSetting = EditorSetting;
		for (int i = 0; i < _grid.Cols.Count; i++)
		{
			editorSetting.Columns[i].Width = _grid.Cols[i].Width;
		}
		Table.TagTicketDirty(isCanOverrideByServerData: true);
	}

	public bool IsLastRowExistBottomBorder()
	{
		TicketTitleFooter ticketTitleFooter = ((_editor_type == EditorType.Title) ? Ticket.Title : Ticket.Footer);
		int rowIndex = ticketTitleFooter.Rows.Count - 1;
		if (rowIndex < 0)
		{
			return false;
		}
		int num = ticketTitleFooter.Columns.Count - 1;
		int i;
		for (i = 0; i < num; i++)
		{
			TicketMerge ticketMerge = ticketTitleFooter.Merges.FirstOrDefault((TicketMerge m) => m.Contains(rowIndex, i));
			int row = rowIndex;
			int col = i;
			if (ticketMerge != null)
			{
				row = ticketMerge.TopRow;
				col = ticketMerge.LeftColumn;
			}
			TicketCell cell = ticketTitleFooter.GetCell(row, col);
			if (cell.Bottom.Width > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFirstRowExistTopBorder()
	{
		TicketTitleFooter ticketTitleFooter = ((_editor_type == EditorType.Title) ? Ticket.Title : Ticket.Footer);
		if (ticketTitleFooter.Rows.Count == 0)
		{
			return false;
		}
		int count = ticketTitleFooter.Columns.Count;
		int i;
		for (i = 0; i < count; i++)
		{
			TicketMerge ticketMerge = ticketTitleFooter.Merges.FirstOrDefault((TicketMerge m) => m.Contains(0, i));
			int row = 0;
			int col = i;
			if (ticketMerge != null)
			{
				row = ticketMerge.TopRow;
				col = ticketMerge.LeftColumn;
			}
			TicketCell cell = ticketTitleFooter.GetCell(row, col);
			if (cell.Top.Width > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void AutoAdjustGridWidth()
	{
		GridBeginUpdate();
		try
		{
			AutoAdjustGridWidthImpl();
		}
		catch (Exception exception)
		{
			if (_editor_type == EditorType.Title)
			{
				exception.Log("调整表单表头区的列宽时发生了未预期的异常");
			}
			else
			{
				exception.Log("调整表单表底区的列宽时发生了未预期的异常");
			}
		}
		finally
		{
			GridEndUpdate();
		}
	}

	protected void AutoAdjustGridWidthImpl()
	{
		int num = GetTicketBodyGridControlWidthWithoutFixedColumnWidth();
		if (num <= 0)
		{
			num = 1;
		}
		AdjustGridWidthImpl(num);
	}

	private int GetTicketBodyGridControlWidthWithoutFixedColumnWidth()
	{
		int num = _owner.GetGridControlWidth() - _owner.GetGridFixedColumnWidth();
		if (_owner.IsGridExistVScrollbar())
		{
			num -= SystemInformation.VerticalScrollBarWidth;
		}
		return num;
	}

	public void AutoAdjustGridHeight()
	{
		_grid.Height = Math.Min(GetAllRowsTotalHeight(), _owner.GetEditorPanelSize().Height);
	}

	private void AdjustGridWidthImpl(int willToWidth)
	{
		if (_vm != null && _vm.Columns.Count == _grid.Cols.Count)
		{
			int count = _vm.Columns.Count;
			int[] array = new int[count];
			for (int i = 0; i < count; i++)
			{
				int columnWidth = _vm.GetColumnWidth(i);
				array[i] = columnWidth;
			}
			SplitValueWithByPercent(willToWidth, array, 0, count - 1);
			for (int j = 0; j < count; j++)
			{
				_grid.Cols[j].Width = array[j];
			}
			_grid.Width = willToWidth;
		}
	}

	private void SplitValueWithByPercent(int newTotalValue, int[] valueArr, int startIndex, int endIndex)
	{
		if (startIndex > endIndex)
		{
			return;
		}
		int num = 0;
		int num2 = endIndex - startIndex + 1;
		int[] array = new int[num2];
		for (int i = startIndex; i <= endIndex; i++)
		{
			num += (array[i - startIndex] = valueArr[i]);
		}
		int num3 = 0;
		float[] array2 = new float[num2];
		for (int j = 0; j < num2; j++)
		{
			if (j == num2 - 1)
			{
				array2[j] = 1f;
				continue;
			}
			num3 += array[j];
			array2[j] = (float)num3 * 1f / (float)num;
		}
		num3 = 0;
		for (int k = 0; k < num2; k++)
		{
			int num4;
			if (k == num2 - 1)
			{
				num4 = newTotalValue - num3;
			}
			else
			{
				num4 = (int)(array2[k] * (float)newTotalValue) - num3;
				num3 += num4;
			}
			valueArr[startIndex + k] = num4;
		}
	}

	private async Task AddAttachment()
	{
		TicketInputCellVM cell = _vm.GetCellVM(_grid.BodyRow, _grid.BodyCol);
		if (!CanEditCell(cell, editAttachments: true))
		{
			return;
		}
		OpenFileDialog ofd = new OpenFileDialog();
		if (ofd.ShowDialog() != DialogResult.OK || cell.IsFormula || cell.IsDynamicRowKeyCell)
		{
			return;
		}
		FileInfo fileInfo = new FileInfo(ofd.FileName);
		if (!SoftwareLicenseManager.IsTableAttachmentOutOfLicenseLimit(fileInfo.Length))
		{
			Guid fileId = Guid.NewGuid();
			try
			{
				Table.Project.FileCacheManager.CopyFrom(ofd.FileName, fileId);
				await Table.Project.FileCacheManager.Upload(fileId);
			}
			catch (Exception ex)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "操作失败: " + ex.Message);
				return;
			}
			if (cell.Attachments == null)
			{
				cell.Attachments = new Leqisoft.Model.CellAttachments();
			}
			cell.Attachments.Attachments.Add(new CellAttachment
			{
				FileId = fileId,
				Name = ofd.SafeFileName
			});
			cell.IsAttachmentsDirty = true;
			cell.Value = "";
			cell.TempCell.Value = "";
			_grid.Invalidate();
			_owner.SetInputDataDirty();
		}
	}

	private void ShowTooltip()
	{
		HideTooltip();
		if (!IsExistSelectedCell() || _owner.Owner.CurrentView != MainFormView.TicketInput || (!_grid.Selection.IsSingleCell && !_grid.MergedRanges.Contains(_grid.Selection)) || _grid.BodyRow < 0 || _grid.BodyCol < 0 || _grid.BodyRow >= _vm.GetRowsCount() || _grid.BodyCol >= _vm.GetColumnsCount())
		{
			return;
		}
		Rectangle cellRect = _grid.GetCellRect(_grid.Row, _grid.Col);
		if (cellRect.Width == 0 || cellRect.Height == 0)
		{
			return;
		}
		_ttpComment.LinkClicked -= _ttpComment_LinkClicked;
		Dictionary<string, object> linkDic = new Dictionary<string, object>();
		XElement xBody = new XElement("div");
		TicketInputCellVM cellVM = _vm.GetCellVM(_grid.BodyRow, _grid.BodyCol);
		bool flag = false;
		if (cellVM.IsExistWarning)
		{
			flag = true;
			xBody.Add(new XElement("b", "告警提示"));
			xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "选中的单元格因控制公式设置了告警条件，该单元格触发了告警。"));
			xBody.Add(new XElement("hr"));
		}
		bool flag2;
		if (Program.MainForm.TableValidationResults.TryGetValue(Table.TreeNode, out var value) && cellVM.IsTableExistCell)
		{
			Leqisoft.Model.Cell tableCell = cellVM.TempCell;
			int cellRowIndex = tableCell.Row.Index;
			int cellColIndex = tableCell.Column.Index;
			IEnumerable<Tuple<Leqisoft.Model.Cell, ValidationResult>> enumerable = value.Cells.Cast<Tuple<Leqisoft.Model.Cell, ValidationResult>>().Where((Tuple<Leqisoft.Model.Cell, ValidationResult> t) => t.Item1.Id == tableCell.Id);
			IEnumerable<Tuple<RangeOperand, ValidationResult>> enumerable2 = value.Ranges.Cast<Tuple<RangeOperand, ValidationResult>>().Where((Tuple<RangeOperand, ValidationResult> t) => t.Item1.TopLeft.Row.Index <= cellRowIndex && t.Item1.TopLeft.Column.Index <= cellColIndex && cellRowIndex <= t.Item1.BottomRight.Row.Index && cellColIndex <= t.Item1.BottomRight.Column.Index);
			IEnumerable<Tuple<Leqisoft.Model.Column, ValidationResult>> enumerable3 = value.Columns.Cast<Tuple<Leqisoft.Model.Column, ValidationResult>>().Where((Tuple<Leqisoft.Model.Column, ValidationResult> t) => t.Item1.Index == cellColIndex);
			IEnumerable<Tuple<Leqisoft.Model.Cell, ValidationResult>> enumerable4 = value.HeaderCells.Cast<Tuple<Leqisoft.Model.Cell, ValidationResult>>().Where((Tuple<Leqisoft.Model.Cell, ValidationResult> t) => t.Item1.Column.Index == cellColIndex && t.Item1.Row.Index < cellRowIndex && t.Item1.GetHeaderLastRow() >= cellRowIndex);
			flag2 = enumerable.Any() || enumerable2.Any() || enumerable3.Any() || enumerable4.Any();
			if (flag2)
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(_owner.Owner.FormulaEditor.Context.Project);
				xBody.Add(new XElement("b", "校验公式"));
				try
				{
					int i = 0;
					foreach (Tuple<Leqisoft.Model.Cell, ValidationResult> item in enumerable)
					{
						AddValidationResult(item.Item2, "c");
						i++;
					}
					i = 0;
					foreach (Tuple<RangeOperand, ValidationResult> item2 in enumerable2)
					{
						AddValidationResult(item2.Item2, "r");
						i++;
					}
					i = 0;
					foreach (Tuple<Leqisoft.Model.Column, ValidationResult> item3 in enumerable3)
					{
						AddValidationResult(item3.Item2, "l");
						i++;
					}
					i = 0;
					foreach (Tuple<Leqisoft.Model.Cell, ValidationResult> item4 in enumerable4)
					{
						AddValidationResult(item4.Item2, "h");
						i++;
					}
					void AddValidationResult(ValidationResult vf, string anchorPrefix)
					{
						xBody.Add(new XElement("p", "公式说明：", vf.Source.Note));
						XElement xElement3 = new XElement("p", "校验等式：");
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(vf.Source.LeftExpr);
						Tuple<List<TooltipListener.FormulaTooltipSegment>, string> formulaTooltipSegments = formulaEvaluator.GetFormulaTooltipSegments(resolver, null, vf);
						foreach (TooltipListener.FormulaTooltipSegment item5 in formulaTooltipSegments.Item1)
						{
							xElement3.Add(item5.PreText);
							string text = $"l{anchorPrefix}{i}{item5.AnchorNumber}";
							xElement3.Add(new XElement("a", new XAttribute("href", text), item5.Display));
							linkDic.Add(text, item5.Ref);
						}
						xElement3.Add(formulaTooltipSegments.Item2);
						xElement3.Add(vf.Source.Operator.Display);
						formulaEvaluator = new FormulaEvaluator(vf.Source.RightExpr);
						formulaTooltipSegments = formulaEvaluator.GetFormulaTooltipSegments(resolver, null, vf);
						foreach (TooltipListener.FormulaTooltipSegment item6 in formulaTooltipSegments.Item1)
						{
							xElement3.Add(item6.PreText);
							string text2 = $"r{anchorPrefix}{i}{item6.AnchorNumber}";
							xElement3.Add(new XElement("a", new XAttribute("href", text2), item6.Display));
							linkDic.Add(text2, item6.Ref);
						}
						xElement3.Add(formulaTooltipSegments.Item2);
						xBody.Add(xElement3);
						string text3 = null;
						string text4 = null;
						if ((vf.LeftValue.Equals(0.0) && vf.RightValue.Equals(string.Empty)) || (vf.LeftValue.Equals(string.Empty) && vf.RightValue.Equals(0.0)))
						{
							text3 = "0";
							text4 = "0";
						}
						else
						{
							text3 = ValidationResult.ValueToString(vf.LeftValue);
							text4 = ValidationResult.ValueToString(vf.RightValue);
						}
						xBody.Add(new XElement("p", new XAttribute("style", "color:" + (vf.Passed ? "green" : "red")), "校验" + (vf.Passed ? "正确" : "错误") + "：", text3, vf.Source.Operator.Display, text4));
						xBody.Add(new XElement("hr"));
					}
				}
				catch
				{
					xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "生成校验公式提示时发生错误，请尝试重新校验。"));
					xBody.Add(new XElement("hr"));
				}
			}
		}
		else
		{
			flag2 = false;
		}
		bool flag3 = cellVM.Attachments != null;
		if (flag3)
		{
			xBody.Add(new XElement("b", "附件管理"));
			for (int j = 0; j < cellVM.Attachments.Attachments.Count; j++)
			{
				CellAttachment cellAttachment = cellVM.Attachments.Attachments[j];
				xBody.Add(new XElement("p", cellAttachment.Name));
				XElement xElement = new XElement("p", new XElement("a", "打开附件", new XAttribute("href", $"openAttachment{j}")), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", "导出附件", new XAttribute("href", $"exportAttachment{j}")), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"));
				xBody.Add(xElement);
				linkDic.Add($"openAttachment{j}", j);
				linkDic.Add($"exportAttachment{j}", j);
				if (Table?.IsLocked != true && CanEditCell(cellVM, editAttachments: true))
				{
					xElement.Add(new XElement("a", "删除附件", new XAttribute("href", $"removeAttachment{j}")), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", "重命名附件", new XAttribute("href", $"renameAttachment{j}")));
					linkDic.Add($"removeAttachment{j}", j);
					linkDic.Add($"renameAttachment{j}", j);
				}
			}
			xBody.Add(new XElement("hr"));
		}
		if (flag || flag2 || flag3)
		{
			xBody.LastNode.Remove();
			XElement xElement2 = xBody.Element("b");
			xElement2.Remove();
			_ttpComment.SetText(xElement2.Value, xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(linkDic);
			_ttpComment.LinkClicked += _ttpComment_LinkClicked;
			_ttpComment.Show(_grid, new Point(cellRect.Right, cellRect.Top + cellRect.Height / 2));
		}
	}

	public bool CanEditCellValue(TicketInputCellVM cvm)
	{
		return CanEditCell(cvm);
	}

	private bool CanEditCell(TicketInputCellVM cvm, bool editAttachments = false)
	{
		try
		{
			if (cvm.Column == null)
			{
				return false;
			}
			if (cvm.IsFormula && !cvm.IsAllowManualInputOnFormula)
			{
				return false;
			}
			if (cvm.IsFormula && HasFillingFormula)
			{
				return false;
			}
			if (cvm.IsFixedMultiRowKey)
			{
				return false;
			}
			if (cvm.IsControlFormulaLocked)
			{
				return false;
			}
			if (_owner.IsTicketLocked)
			{
				return false;
			}
			Leqisoft.Model.Cell tempCell = cvm.TempCell;
			if (!tempCell.IsEditable)
			{
				return false;
			}
			if (!tempCell.Column.Permissions.Write.GrantAll && tempCell.Column.Permissions.CanWrite())
			{
				return true;
			}
			if (!Program.MainForm.TableEditor.CanEditColumn(tempCell.Column))
			{
				return false;
			}
			if (!Program.MainForm.TableEditor.CanEditRow(tempCell.Row))
			{
				return false;
			}
			if (tempCell.Row.Role == RowRole.Header)
			{
				return false;
			}
			if (tempCell.Column.CrossAttributes.Role != 0)
			{
				return false;
			}
			if (!editAttachments && cvm.Attachments != null)
			{
				return false;
			}
			if (cvm.TableCell == null && !SoftwareLicenseManager.IsAllowAddTableRows(showDialog: false))
			{
				return false;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void SetCommandState()
	{
		try
		{
			bool hasSelection = IsExistSelectedCell();
			bool isLocked = Table?.IsLocked == true || _owner?.IsTicketLocked == true;
			foreach (C1CommandLink link in _ctxCell?.CommandLinks)
			{
				if (link?.Command != null)
					link.Command.Enabled = hasSelection && !isLocked;
			}
		}
		catch { }
	}

	private bool IsPreventCutOrDeleteCellValue()
	{
		if (_owner.IsInShowingVirtualNode && !SoftwareLicenseManager.IsAllowAddTableRows())
		{
			return true;
		}
		return false;
	}

	private void ClearSelection()
	{
		if (Table?.IsLocked == true || _owner.IsTicketLocked || IsPreventCutOrDeleteCellValue())
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		_owner._vm.BeginBatchUpdateValue();
		_owner.ChangeVirtualValueToRealValue();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
				if (CanEditCell(cellVM))
				{
					_owner._vm.UpdateTicketCellValue(cellVM, "", isFormulaExistManualInputValue: true);
				}
			}
		}
		_owner._vm.EndBatchUpdateValue();
		_owner.VMData.CalculateTicket();
		_grid.Invalidate();
		_owner.SetInputDataDirty();
	}

	private async void _ttpComment_LinkClicked(object sender, object e)
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
		if (!(e is Leqisoft.Model.Column column))
		{
			if (!(e is Leqisoft.Model.Cell cell))
			{
				if (!(e is TreeNodeBase treeNodeBase))
				{
					if (e is CellsOperand cellsOperand)
					{
						if (cellsOperand.Table == Table)
						{
							_owner.SwitchToTableMode();
						}
						Program.MainForm.SetOpenModeToTableMode(cellsOperand.Table.TreeNode);
						Program.MainForm.ProjectHierarchy.FindAndSelectNode(cellsOperand.Table.TreeNode);
						if (Program.MainForm.TableEditor.Table == cellsOperand.Table)
						{
							Program.MainForm.TableEditor.ShowConditionCells(cellsOperand);
						}
						return;
					}
					string text = (string)sender;
					if (text.StartsWith("openAttachment"))
					{
						int index = (int)e;
						TicketInputCellVM cellVM = _vm.GetCellVM(_grid.BodyRow, _grid.BodyCol);
						CellAttachment ca = cellVM.Attachments.Attachments[index];
						await TableEditor.OpenAttachment(ca);
					}
					else if (text.StartsWith("exportAttachment"))
					{
						int index2 = (int)e;
						TicketInputCellVM cellVM2 = _vm.GetCellVM(_grid.BodyRow, _grid.BodyCol);
						CellAttachment attachment = cellVM2.Attachments.Attachments[index2];
						Guid fileId = attachment.FileId;
						await Table.Project.FileCacheManager.DownloadIfNotExist(fileId);
						SaveFileDialog saveFileDialog = new SaveFileDialog
						{
							Title = "导出附件",
							FileName = attachment.Name
						};
						if (saveFileDialog.ShowDialog() == DialogResult.OK)
						{
							try
							{
								Table.Project.FileCacheManager.DuplicateTo(fileId, saveFileDialog.FileName);
							}
							catch
							{
								Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出附件失败。");
							}
						}
					}
					else if (text.StartsWith("removeAttachment"))
					{
						int fileIndex = (int)e;
						TicketInputCellVM cellVM3 = _vm.GetCellVM(_grid.BodyRow, _grid.BodyCol);
						if (CanEditCell(cellVM3, editAttachments: true))
						{
							int cellRowIndex = _grid.BodyRow + _grid.Rows.Fixed;
							int cellColIndex = _grid.BodyCol + _grid.Cols.Fixed;
							_vm.RemoveAttachment(cellRowIndex, cellColIndex, fileIndex);
							cellVM3.IsAttachmentsDirty = true;
							_grid.Invalidate();
							ShowTooltip();
							_owner.SetInputDataDirty();
						}
					}
					else if (text.StartsWith("renameAttachment"))
					{
						int num = (int)e;
						TicketInputCellVM cellVM4 = _vm.GetCellVM(_grid.BodyRow, _grid.BodyCol);
						if (CanEditCell(cellVM4, editAttachments: true))
						{
							CellAttachment cellAttachment = cellVM4.Attachments.Attachments[num];
							string text2 = InputForm.Text("重命名附件", "将附件‘" + cellAttachment.Name + "’重命名为：", cellAttachment.Name);
							if (!string.IsNullOrWhiteSpace(text2))
							{
								int cellRowIndex2 = _grid.BodyRow + _grid.Rows.Fixed;
								int cellColIndex2 = _grid.BodyCol + _grid.Cols.Fixed;
								_vm.RenameAttachment(cellRowIndex2, cellColIndex2, num, text2);
							}
							cellVM4.IsAttachmentsDirty = true;
							ShowTooltip();
							_owner.SetInputDataDirty();
						}
					}
					else if (text.StartsWith("addAttachment"))
					{
						await AddAttachment();
					}
				}
				else if (treeNodeBase != Table.TreeNode)
				{
					_owner.Owner.ProjectHierarchy.FindAndSelectNode(treeNodeBase);
				}
			}
			else
			{
				if (cell.Column.Table == Table)
				{
					_owner.SwitchToTableMode();
				}
				Program.MainForm.SetOpenModeToTableMode(cell.Column.Table.TreeNode);
				_owner.Owner.ProjectHierarchy.FindAndSelectNode(cell.Column.Table.TreeNode);
				if (Program.MainForm.TableEditor.Table == cell.Column.Table)
				{
					Program.MainForm.TableEditor.Select(cell.Row.Index, cell.Column.Index);
				}
			}
		}
		else
		{
			if (column.Table == Table)
			{
				_owner.SwitchToTableMode();
			}
			Program.MainForm.SetOpenModeToTableMode(column.Table.TreeNode);
			_owner.Owner.ProjectHierarchy.FindAndSelectNode(column.Table.TreeNode);
			if (Program.MainForm.TableEditor.Table == column.Table)
			{
				Program.MainForm.TableEditor.SelectColumn(column.Index);
			}
		}
	}

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyData)
		{
		case Keys.Back:
		case Keys.Delete:
			ClearSelection();
			break;
		case Keys.X | Keys.Control:
			Cut();
			break;
		case Keys.C | Keys.Control:
			Copy();
			break;
		case Keys.V | Keys.Control:
			Paste();
			break;
		case Keys.Return:
		{
			e.SuppressKeyPress = true;
			if (!IsGridLastRow(_grid.Row, _grid.Col, out var suggetMoveToColIndex5))
			{
				_grid.Row++;
			}
			else if (_editor_type == EditorType.Title)
			{
				_owner.MoveFocusToBodyCell(0, suggetMoveToColIndex5);
			}
			break;
		}
		case Keys.Space:
			checkCellBox(_grid.Row, _grid.Col, null);
			break;
		case Keys.A | Keys.Control:
			_grid.BodySelect(0, 0, _grid.BodyRowsCount - 1, _grid.BodyColsCount - 1);
			break;
		case Keys.Up:
		{
			int suggetMoveToColIndex4;
			if (_editor_type == EditorType.Footer)
			{
				if (IsGridFirstRow(_grid.Row, _grid.Col, out var suggetMoveToColIndex3))
				{
					e.SuppressKeyPress = true;
					_owner.MoveFocusToBodyCell(_owner.Grid.Rows.Count - 1, _grid.Col);
				}
				else
				{
					e.SuppressKeyPress = true;
					_grid.SafeSelect(_grid.Row - 1, suggetMoveToColIndex3);
				}
			}
			else if (!IsGridFirstRow(_grid.Row, _grid.Col, out suggetMoveToColIndex4))
			{
				e.SuppressKeyPress = true;
				_grid.SafeSelect(_grid.Row - 1, suggetMoveToColIndex4);
			}
			break;
		}
		case Keys.Down:
		{
			int suggetMoveToColIndex2;
			if (_editor_type == EditorType.Title)
			{
				if (IsGridLastRow(_grid.Row, _grid.Col, out var suggetMoveToColIndex))
				{
					e.SuppressKeyPress = true;
					_owner.MoveFocusToBodyCell(0, suggetMoveToColIndex);
				}
				else
				{
					e.SuppressKeyPress = true;
					_grid.SafeSelect(_grid.Row + 1, suggetMoveToColIndex);
				}
			}
			else if (!IsGridLastRow(_grid.Row, _grid.Col, out suggetMoveToColIndex2))
			{
				e.SuppressKeyPress = true;
				_grid.SafeSelect(_grid.Row + 1, suggetMoveToColIndex2);
			}
			break;
		}
		}
	}

	public void MoveFocusToBodyCell(int rowIndex, int colIndex)
	{
		try
		{
			if (rowIndex < 0)
			{
				rowIndex = 0;
			}
			if (colIndex < 0)
			{
				colIndex = 0;
			}
			int num = _grid.Rows.Fixed + rowIndex;
			int num2 = _grid.Cols.Fixed + colIndex;
			if (num >= _grid.Rows.Count)
			{
				num = _grid.Rows.Count - 1;
			}
			if (num2 >= _grid.Cols.Count)
			{
				num2 = _grid.Cols.Count - 1;
			}
			_grid.SafeSelect(num, num2, num, num2);
			_grid.Focus();
		}
		catch
		{
		}
	}

	protected Operand GetCellValueList(int row, int col, string comboList)
	{
		if (_editor_type == EditorType.Title)
		{
			return _owner.VMData.GetTitleCellValueList(row, col, comboList);
		}
		return _owner.VMData.GetFooterCellValueList(row, col, comboList);
	}

	private void _grid_BodyStartEdit(object sender, RowColEventArgs e)
	{
		_owner.ChangeVirtualValueToRealValue();
		TicketInputCellVM cellVM = _vm.GetCellVM(e.Row, e.Col);
		_grid.BodySetData(e.Row, e.Col, cellVM.Value);
		DataFormat format = cellVM.Column.GetFormat();
		if (format.HasComboList)
		{
			if (cellVM.TableRow == null)
			{
				_owner._vm.BuildTableCellForTicketTitleFooterCell(cellVM);
			}
			Operand cellValueList = GetCellValueList(e.Row, e.Col, format.ComboList);
			if (_grid.Editor == ListDropDown.DropDown)
			{
				ListDropDown.DropDown.EditorDataType = typeof(string);
				ListDropDown.DropDown.EditorInitValue = null;
				Type dataType = format.GetDataType();
				if (dataType == typeof(DateTime) || dataType == typeof(DateYearMonth) || dataType == typeof(TimeSpan))
				{
					ListDropDown.DropDown.EditorInitValue = cellVM.GetDisplayValue();
				}
				if (cellValueList is TreeListOperand op)
				{
					if (format.MultiComboList)
					{
						ListDropDown.ViewKind = DropDownViewKind.TreeCheckList;
						ListDropDown.TreeCheckedList.Op = op;
						ListDropDown.TreeCheckedList.Populate();
					}
					else
					{
						ListDropDown.ViewKind = DropDownViewKind.TreeList;
						ListDropDown.TreeList.Op = op;
						ListDropDown.TreeList.Populate();
					}
				}
				else if (cellValueList is ValueSetOperand op2)
				{
					if (format.MultiComboList)
					{
						ListDropDown.ViewKind = DropDownViewKind.SimpleCheckList;
						ListDropDown.SimpleCheckedList.Op = op2;
						ListDropDown.SimpleCheckedList.Populate();
						ListDropDown.SimpleCheckedList.SetInitValue(cellVM.GetDisplayValue());
					}
					else
					{
						ListDropDown.ViewKind = DropDownViewKind.SimpleList;
						ListDropDown.SimpleList.Op = op2;
						ListDropDown.SimpleList.Populate();
					}
				}
				else if (cellValueList is TableListOperand op3)
				{
					if (format.MultiComboList)
					{
						ListDropDown.ViewKind = DropDownViewKind.TableCheckList;
						ListDropDown.TableCheckedList.Op = op3;
						ListDropDown.TableCheckedList.Populate();
						ListDropDown.TableCheckedList.SetInitValue(cellVM.GetDisplayValue());
					}
					else
					{
						ListDropDown.ViewKind = DropDownViewKind.TableList;
						ListDropDown.TableList.Op = op3;
						ListDropDown.TableList.Populate();
					}
				}
				else if (cellValueList is MultiListOperand op4)
				{
					if (format.MultiComboList)
					{
						ListDropDown.ViewKind = DropDownViewKind.MultiCheckList;
						ListDropDown.MultiCheckedList.Op = op4;
						ListDropDown.MultiCheckedList.Populate();
					}
					else
					{
						ListDropDown.ViewKind = DropDownViewKind.MultiList;
						ListDropDown.MultiList.Op = op4;
						ListDropDown.MultiList.Populate();
					}
				}
			}
			else if (_grid.Editor == InputListDropDown.DropDown)
			{
				InputListDropDown.DropDown.EditorDataType = typeof(string);
				InputListDropDown.DropDown.EditorInitValue = null;
				Type dataType2 = format.GetDataType();
				if (dataType2 == typeof(DateTime) || dataType2 == typeof(DateYearMonth) || dataType2 == typeof(TimeSpan))
				{
					InputListDropDown.DropDown.EditorInitValue = cellVM.GetDisplayValue();
				}
				InputListDropDown.Clear();
				InputListDropDown.CanInputTextbox = format.IgnoreComboList;
				if (cellValueList is InputListOperand op5)
				{
					InputListDropDown.Op = op5;
					InputListDropDown.Populate();
					InputListDropDown.SetInitValue(cellVM.GetDisplayValue());
				}
				else if (cellValueList == ValueSetOperand.Empty)
				{
					InputListDropDown.PopulateError();
				}
			}
		}
		_owner.TitleEditor.View.Invalidate();
		_owner.FooterEditor.View.Invalidate();
		_owner.Grid.Invalidate();
	}

	private void _grid_BodySetupEditor(object sender, RowColEventArgs e)
	{
		if (_grid.Editor != null)
		{
			_grid.Editor.Top++;
			_grid.Editor.Left++;
			_grid.Editor.Width -= 2;
			_grid.Editor.Height -= 2;
		}
		TicketInputCellVM cellVM = _vm.GetCellVM(e.Row, e.Col);
		Type dataType = cellVM.Column.GetDataType();
		if (_grid.Editor == ListDropDown.DropDown)
		{
			ListDropDown.DropDown.DataType = typeof(string);
		}
		else if (dataType == typeof(DateTime) && string.IsNullOrEmpty(cellVM.GetDisplayValue()))
		{
			_dateEdit.Value = DateTime.Now.Date;
		}
		else if (dataType == typeof(TimeSpan))
		{
			_timeEdit.DataType = typeof(object);
			_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
			_timeEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
			DateTime result;
			if (string.IsNullOrEmpty(cellVM.GetDisplayValue()))
			{
				_timeEdit.Value = DateTime.Now;
			}
			else if (cellVM.Value is TimeSpan timeSpan)
			{
				_timeEdit.Value = new DateTime(2000, 1, 1, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			}
			else if (DateTime.TryParse(cellVM.GetDisplayValue(), out result))
			{
				_timeEdit.Value = result;
			}
			else
			{
				_timeEdit.Value = DateTime.Now;
			}
			_timeEdit.DataType = typeof(DateTime);
			TableEditor.ConvertCellTimeDisplayFormatToTimeEditorFormat(cellVM.Column.GetFormat().GetFormatString(), out var customFormat, out var customEditFormat);
			_timeEdit.CustomFormat = customFormat;
			_timeEdit.EditFormat.CustomFormat = customEditFormat;
		}
		else if (dataType == typeof(string))
		{
			DataFormat format = cellVM.Column.GetFormat();
			if (_grid.Editor is TextBox textBox && !format.HasComboList)
			{
				try
				{
					if (!textBox.Visible)
					{
						textBox.Visible = true;
					}
					if (!textBox.Multiline)
					{
						textBox.Multiline = true;
					}
					textBox.Text = Regex.Replace(cellVM.GetDisplayValue(), "(?<!\\r)\\n", "\r\n");
				}
				catch (Exception exception)
				{
					exception.Log("表单编辑页面输入框赋值失败");
				}
			}
		}
		else if (dataType == typeof(DateYearMonth))
		{
			_dateEdit.DataType = typeof(DateTime);
			string displayValue = cellVM.GetDisplayValue();
			DateTime result2;
			DateTime dateTime = (string.IsNullOrWhiteSpace(displayValue) ? DateTime.Now.Date : ((!(cellVM.Value is DateYearMonth dateYearMonth)) ? ((!DateTime.TryParse(displayValue, out result2)) ? DateTime.Now.Date : result2.Date) : dateYearMonth.Date));
			_dateEdit.Value = dateTime;
			_dateEdit.CustomFormat = cellVM.Column.GetFormat().GetFormatString();
		}
		ListDropDown.SkipTextChanged = false;
		_owner._vm.BuildTableCellForTicketTitleFooterCell(cellVM);
	}

	private void _grid_BodyValidateEdit(object sender, ValidateEditEventArgs e)
	{
		if (_vm.IsIndexOutOfRange(e.Row, e.Col))
		{
			e.Cancel = true;
			return;
		}
		TicketInputCellVM cellVM = _vm.GetCellVM(e.Row, e.Col);
		Type dataType = cellVM.Column.GetDataType();
		if (dataType == typeof(DateYearMonth) && _dateEdit.Value is DateTime)
		{
			DateTime date = (DateTime)_dateEdit.Value;
			_dateEdit.DataType = typeof(DateYearMonth);
			_dateEdit.Value = new DateYearMonth(date)
			{
				ToStringFormat = cellVM.Column.GetFormat().GetFormatString()
			};
		}
		else if (dataType == typeof(TimeSpan) && _timeEdit.Value is DateTime dateTime)
		{
			_timeEdit.DataType = typeof(object);
			_timeEdit.Value = new TimeSpan(dateTime.Hour, dateTime.Minute, dateTime.Second);
			_timeEdit.DataType = typeof(TimeSpan);
		}
		DataFormat format = cellVM.Column.GetFormat();
		if (format.HasComboList && _grid.Editor != InputListDropDown.DropDown && _grid.Editor == ListDropDown.DropDown && !format.IgnoreComboList && !ListDropDown.Validate())
		{
			_grid.FinishEditing(cancel: true);
		}
		ListDropDown.SkipTextChanged = true;
	}

	private void _grid_BodyAfterEdit(object sender, RowColEventArgs e)
	{
		if (_vm.IsIndexOutOfRange(e.Row, e.Col))
		{
			return;
		}
		TicketInputCellVM cellVM = _vm.GetCellVM(e.Row, e.Col);
		object obj = _grid.BodyGetData(e.Row, e.Col);
		bool isFormulaExistManualInputValue = true;
		if (cellVM.IsFormula)
		{
			isFormulaExistManualInputValue = true;
			if (!cellVM.IsExistManualInputValue)
			{
				string displayValue = cellVM.GetDisplayValue();
				string text = cellVM.ConvertInputValueToDisplayValue(obj);
				if (text == displayValue)
				{
					isFormulaExistManualInputValue = false;
				}
			}
		}
		_owner._vm.UpdateTicketCellValue(cellVM, obj, isFormulaExistManualInputValue);
		_owner.VMData.CalculateTicket();
		_owner.SetInputDataDirty();
		_owner.TitleEditor.View.Invalidate();
		_owner.FooterEditor.View.Invalidate();
		_owner.Grid.Invalidate();
		if (_editor_type == EditorType.Title && IsGridLastRow(e.Row, e.Col, out var _) && IsGridLastAbleEditCell(e.Row, e.Col))
		{
			_owner.MoveFocusToBodyCell(0, 0);
		}
	}

	private bool IsGridFirstRow(int gridRowIndex, int gridColIndex, out int suggetMoveToColIndex)
	{
		suggetMoveToColIndex = gridColIndex;
		if (!_grid.IsIndexOutOfRange(gridRowIndex, gridColIndex))
		{
			int num = _grid.MergedRanges.IndexOf(gridRowIndex, gridColIndex);
			if (num != -1)
			{
				C1.Win.C1FlexGrid.CellRange cellRange = _grid.MergedRanges[num];
				gridRowIndex = cellRange.TopRow;
				suggetMoveToColIndex = cellRange.LeftCol;
			}
		}
		if (gridRowIndex <= 0)
		{
			return true;
		}
		if (_grid.Rows.Count == 0)
		{
			return true;
		}
		return false;
	}

	private bool IsGridLastRow(int gridRowIndex, int gridColIndex, out int suggetMoveToColIndex)
	{
		suggetMoveToColIndex = gridColIndex;
		if (!_grid.IsIndexOutOfRange(gridRowIndex, gridColIndex))
		{
			int num = _grid.MergedRanges.IndexOf(gridRowIndex, gridColIndex);
			if (num != -1)
			{
				C1.Win.C1FlexGrid.CellRange cellRange = _grid.MergedRanges[num];
				gridRowIndex = cellRange.BottomRow;
				suggetMoveToColIndex = cellRange.LeftCol;
			}
		}
		for (int i = gridRowIndex + 1; i < _grid.Rows.Count; i++)
		{
			if (_grid.Rows[i].HeightDisplay != 0)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsGridLastAbleEditCell(int gridRowIndex, int gridColIndex)
	{
		if (_grid.IsIndexOutOfRange(gridRowIndex, gridColIndex))
		{
			return true;
		}
		for (int i = gridColIndex + 1; i < _grid.Cols.Count; i++)
		{
			if (!_vm.IsIndexOutOfRange(gridRowIndex, i))
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(gridRowIndex, i);
				if (CanEditCell(cellVM))
				{
					return false;
				}
			}
		}
		for (int j = gridRowIndex + 1; j < _grid.Rows.Count; j++)
		{
			for (int k = 0; k < _grid.Cols.Count; k++)
			{
				if (!_vm.IsIndexOutOfRange(j, k))
				{
					TicketInputCellVM cellVM2 = _vm.GetCellVM(j, k);
					if (CanEditCell(cellVM2))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void _grid_BodyBeforeEdit(object sender, RowColEventArgs e)
	{
		if (Program.MainForm.IsInSyncingProject)
		{
			e.Cancel = true;
			return;
		}
		if (_owner.IsTicketLocked)
		{
			e.Cancel = true;
			return;
		}
		try
		{
			TicketInputCellVM cellVM = _vm.GetCellVM(e.Row, e.Col);
			if (!CanEditCell(cellVM))
			{
				e.Cancel = true;
			}
			else
			{
				if (_vm.Merges.Any((TicketMerge m) => m.Contains(e.Row, e.Col) && (m.TopRow != e.Row || m.LeftColumn != e.Col)))
				{
					return;
				}
				if (cellVM.IsField)
				{
					if (cellVM.Column == null)
					{
						e.Cancel = true;
						return;
					}
					DataFormat format = cellVM.Column.GetFormat();
					if (format.FormatType == DataFormatType.BoolCheckBox || format.FormatType == DataFormatType.BoolOnOff)
					{
						e.Cancel = true;
						return;
					}
					_grid.BodySetData(e.Row, e.Col, cellVM.Value);
					C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
					Type dataType = cellVM.Column.GetDataType();
					if (format.HasComboList)
					{
						InputListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = null;
						ListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = null;
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(format.ComboList);
						if (formulaEvaluator.HasInputList())
						{
							styleNew.Editor = InputListDropDown.DropDown;
						}
						else
						{
							styleNew.Editor = ListDropDown.DropDown;
						}
						if (dataType == typeof(DateYearMonth))
						{
							InputListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = ConvertDropDownListValueToDateYearMonthValue;
							ListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = ConvertDropDownListValueToDateYearMonthValue;
						}
					}
					else if (dataType == typeof(DateTime) || dataType == typeof(DateYearMonth))
					{
						styleNew.Editor = _dateEdit;
						_dateEdit.EditFormat.CustomFormat = format.GetFormatString();
					}
					else if (dataType == typeof(TimeSpan))
					{
						styleNew.Editor = _timeEdit;
					}
					else
					{
						styleNew.Editor = null;
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private object ConvertDropDownListValueToDateYearMonthValue(object value)
	{
		if (value == null)
		{
			return new DateYearMonth();
		}
		if (DateTime.TryParse(value.ToString(), out var result))
		{
			return new DateYearMonth(result);
		}
		return new DateYearMonth();
	}

	private Rectangle GetCancelManualInputIconArea(Rectangle cellRect, out bool isIconOutOfRange)
	{
		Rectangle result = new Rectangle(cellRect.X + 2, cellRect.Y + 2, Resources.CancelManualInput.Width, Resources.CancelManualInput.Height);
		isIconOutOfRange = false;
		if (result.X - 2 + Resources.CancelManualInput.Width + 4 >= cellRect.Right || result.Y - 2 + Resources.CancelManualInput.Height + 4 >= cellRect.Bottom)
		{
			isIconOutOfRange = true;
		}
		return result;
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		Color color = UserSet.Config.TableStyle.LockAreaColor;
		try
		{
			if (_owner != null)
			{
				color = _owner.EditorPanel.BackColor;
			}
			C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
			TicketInputCellVM cellVM = _vm.GetCellVM(e.Row, e.Col);
			e.Text = cellVM.GetDisplayValue();
			styleNew.Font = cellVM.Font;
			styleNew.ForeColor = cellVM.ForeColor;
			styleNew.BackColor = ((cellVM.BackColor == Color.White) ? color : cellVM.BackColor);
			styleNew.TextAlign = C1FlexGridEx.ToTextAlign(cellVM.Align);
			styleNew.Margins = new System.Drawing.Printing.Margins(cellVM.Indent, 0, 0, 0);
			if (cellVM.IsField && !cellVM.IsFixedMultiRowKey)
			{
				styleNew.BackColor = (Color.Empty.Equals(cellVM.BackColor) ? Color.White : cellVM.BackColor);
				if (cellVM.Column == null)
				{
					e.Text = "(无效列引用)";
				}
				else
				{
					Type dataType = cellVM.Column.GetDataType();
					styleNew.DataType = ((dataType == typeof(bool)) ? null : dataType);
					DataFormat format = cellVM.Column.GetFormat();
					if (format.FormatType == DataFormatType.BoolCheckBox)
					{
						e.Text = "";
						e.Image = (cellVM.Value.Equals(true) ? _grid.Glyphs[GlyphEnum.Checked] : _grid.Glyphs[GlyphEnum.Unchecked]);
						styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cellVM.Align);
					}
					else if (format.FormatType == DataFormatType.BoolOnOff)
					{
						e.Text = "";
						e.Image = (cellVM.Value.Equals(true) ? Resources.On : Resources.Off);
						styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cellVM.Align);
					}
					else if (cellVM.Attachments != null)
					{
						e.Image = Resources.CellAttachment;
						e.Text = ((cellVM.TempCell.DisplayAlign == CellTextAlign.MiddleCenter) ? "\n\n" : "") + $"({cellVM.Attachments.Attachments.Count}个附件)";
						styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cellVM.TempCell.DisplayAlign);
					}
					if (cellVM.TempCell.DisplayLocked != 0L || !_owner.VMData.CanEditColumn(cellVM.Column) || cellVM.IsControlFormulaLocked || (cellVM.TempCell.Row != null && Program.MainForm.TableEditor.Table != null && !Program.MainForm.TableEditor.CanEditRow(cellVM.TempCell.Row)))
					{
						styleNew.BackColor = UserSet.Config.TableStyle.LockAreaColor;
					}
					else if (cellVM.IsTableExistCell && cellVM.TempCell != null && cellVM.TempCell.HasFormula)
					{
						styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
					}
				}
			}
			styleNew.WordWrap = true;
			bool flag = false;
			if (cellVM.IsFormula && !cellVM.IsFixedMultiRowKey)
			{
				if (cellVM.IsAllowManualInputOnFormula)
				{
					if (cellVM.IsExistManualInputValue)
					{
						styleNew.BackColor = Color.White;
						flag = true;
					}
					else
					{
						styleNew.BackColor = UserSet.Config.TableStyle.AllowManualInputFormulaColor;
					}
				}
				else
				{
					styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
				}
			}
			if (cellVM.IsTableExistCell && Program.MainForm.ShowHelperTooltip && Program.MainForm.TableValidationResults.TryGetValue(Table.TreeNode, out var value))
			{
				Leqisoft.Model.Cell tableCell = cellVM.TempCell;
				int cellRowIndex = tableCell.Row.Index;
				int cellColIndex = tableCell.Column.Index;
				IEnumerable<Tuple<Leqisoft.Model.Cell, ValidationResult>> source = value.Cells.Cast<Tuple<Leqisoft.Model.Cell, ValidationResult>>().Where((Tuple<Leqisoft.Model.Cell, ValidationResult> t) => t.Item1.Id == tableCell.Id);
				IEnumerable<Tuple<RangeOperand, ValidationResult>> source2 = value.Ranges.Cast<Tuple<RangeOperand, ValidationResult>>().Where((Tuple<RangeOperand, ValidationResult> t) => t.Item1.TopLeft.Row.Index <= cellRowIndex && t.Item1.TopLeft.Column.Index <= cellColIndex && cellRowIndex <= t.Item1.BottomRight.Row.Index && cellColIndex <= t.Item1.BottomRight.Column.Index);
				IEnumerable<Tuple<Leqisoft.Model.Column, ValidationResult>> source3 = value.Columns.Cast<Tuple<Leqisoft.Model.Column, ValidationResult>>().Where((Tuple<Leqisoft.Model.Column, ValidationResult> t) => t.Item1.Index == cellColIndex);
				IEnumerable<Tuple<Leqisoft.Model.Cell, ValidationResult>> source4 = value.HeaderCells.Cast<Tuple<Leqisoft.Model.Cell, ValidationResult>>().Where((Tuple<Leqisoft.Model.Cell, ValidationResult> t) => t.Item1.Column.Index == cellColIndex && t.Item1.Row.Index < cellRowIndex && t.Item1.GetHeaderLastRow() >= cellRowIndex);
				if (source.Any() || source2.Any() || source3.Any() || source4.Any())
				{
					if (source.Any((Tuple<Leqisoft.Model.Cell, ValidationResult> t) => !t.Item2.Passed) || source2.Any((Tuple<RangeOperand, ValidationResult> t) => !t.Item2.Passed) || source3.Any((Tuple<Leqisoft.Model.Column, ValidationResult> t) => !t.Item2.Passed) || source4.Any((Tuple<Leqisoft.Model.Cell, ValidationResult> t) => !t.Item2.Passed))
					{
						styleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
					}
					else
					{
						styleNew.BackColor = UserSet.Config.TableStyle.CheckPassColor;
					}
				}
			}
			if (e.Style.Name == _grid.Styles.Highlight.Name)
			{
				styleNew.ForeColor = _grid.Styles.Highlight.ForeColor;
				styleNew.BackColor = _grid.Styles.Highlight.BackColor;
				if (cellVM.IsShowVirtualValue)
				{
					e.Text = ((cellVM.VirtualValue == null) ? string.Empty : cellVM.VirtualValue);
				}
			}
			else
			{
				if (cellVM.IsExistWarning)
				{
					styleNew.ForeColor = _owner.WarningTextColor;
					if (!_owner.IsWarningTextNeedToShow)
					{
						e.Text = string.Empty;
					}
				}
				else if (cellVM.IsExistRemind)
				{
					styleNew.ForeColor = _owner.RemindTextColor;
					if (!_owner.IsWarningTextNeedToShow)
					{
						e.Text = string.Empty;
					}
				}
				if (cellVM.IsField && _owner.IsTicketLocked)
				{
					styleNew.BackColor = UserSet.Config.TableStyle.LockAreaColor;
				}
				if (cellVM.TableCell != null && cellVM.TableCell.Row.Table.ControlForeColorCells.TryGetValue(cellVM.TableCell, out var value2))
				{
					styleNew.ForeColor = value2;
				}
				if (cellVM.TableCell != null && cellVM.TableCell.Row.Table.ControlBackColorCells.TryGetValue(cellVM.TableCell, out var value3))
				{
					styleNew.BackColor = value3;
				}
				if (cellVM.IsShowVirtualValue)
				{
					e.Text = ((cellVM.VirtualValue == null) ? string.Empty : cellVM.VirtualValue);
					styleNew.ForeColor = TicketNavGrid.VirtualNodeTextColor;
				}
			}
			styleNew.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			e.Style = styleNew;
			int num = e.Row + _grid.Rows.Fixed;
			int num2 = e.Col + _grid.Cols.Fixed;
			if (flag && _grid.Selection.Contains(num, num2) && CanEditCell(cellVM))
			{
				e.DrawCell(DrawCellFlags.All);
				PaintCancelManualInputIcon(num, num2);
				e.Handled = true;
			}
			else if (DrawUserHeaderIcon(cellVM, num, num2, styleNew))
			{
				e.Handled = true;
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		bool DrawUserHeaderIcon(TicketInputCellVM vmCell, int gridRowIndex, int gridColIndex, C1.Win.C1FlexGrid.CellStyle cs)
		{
			System.Drawing.Image ticketCellUserHeaderIcon = _owner.GridDecorator.GetTicketCellUserHeaderIcon(vmCell);
			if (ticketCellUserHeaderIcon == null)
			{
				return false;
			}
			if (cs.BackColor != Color.White && !vmCell.IsAllowManualInputOnFormula)
			{
				return false;
			}
			if (_grid.Selection.Contains(gridRowIndex, gridColIndex))
			{
				return false;
			}
			e.DrawCell(DrawCellFlags.Background | DrawCellFlags.Border);
			int height = _grid.Rows[gridRowIndex].Height;
			int y = e.Bounds.Y;
			y += ((height > ticketCellUserHeaderIcon.Height) ? ((height - ticketCellUserHeaderIcon.Height) / 2) : 0);
			e.Graphics.DrawImage(ticketCellUserHeaderIcon, e.Bounds.X, y);
			e.DrawCell(DrawCellFlags.Content);
			return true;
		}
		void PaintCancelManualInputIcon(int gridRowIndex, int gridColIndex)
		{
			Rectangle cellRect = _grid.GetCellRect(gridRowIndex, gridColIndex);
			bool isIconOutOfRange;
			Rectangle cancelManualInputIconArea = GetCancelManualInputIconArea(cellRect, out isIconOutOfRange);
			if (!isIconOutOfRange)
			{
				e.DrawCell(DrawCellFlags.All);
				if (_isMouseOverCancelManualInputIcon)
				{
					Rectangle rect = new Rectangle(cancelManualInputIconArea.X - 2, cancelManualInputIconArea.Y - 2, cancelManualInputIconArea.Width + 4, cancelManualInputIconArea.Height + 4);
					_cancelManualInputBackgroundBrush.Color = Leqisoft.UI.Controls.Util.DarkenColor(_grid.Styles.Highlight.BackColor, 0.1);
					e.Graphics.FillRectangle(_cancelManualInputBackgroundBrush, rect);
				}
				e.Graphics.DrawImage(Resources.CancelManualInput, cancelManualInputIconArea.X, cancelManualInputIconArea.Y);
			}
		}
	}

	private void AdjustVMWidthByOwnerGridColumnWidth()
	{
		AdjustVMWidthByOwnerGridColumnWidthImpl();
		if (_editor_type == EditorType.Title)
		{
			_owner.FooterEditor.AutoAdjustGridWidth();
		}
		else
		{
			_owner.TitleEditor.AutoAdjustGridWidth();
		}
	}

	private void AdjustVMWidthByOwnerGridColumnWidthImpl()
	{
		int count = _grid.Cols.Count;
		int[] array = new int[count];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			num += (array[i] = EditorSetting.Columns[i].Width);
		}
		for (int j = 0; j < count; j++)
		{
			_grid.Cols[j].Width = array[j];
		}
		AutoAdjustGridWidthImpl();
		for (int k = 0; k < count; k++)
		{
			EditorSetting.Columns[k].Width = array[k];
		}
	}

	private void _gridResizingManager_ResizeRow(object sender, ResizeEventArgs e)
	{
		_owner.EditorPanel.SuspendDrawing();
		GridBeginUpdate();
		try
		{
			_owner.CacheSelectRange();
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			int num = Math.Max(1, e.HeightWidth);
			EditorSetting.Rows[e.RowCol].Height = e.HeightWidth;
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			_owner.PopulateVm();
			_owner.RestoreSelectRange();
		}
		finally
		{
			GridEndUpdate();
			_owner.EditorPanel.ResumeDrawing();
		}
	}

	private void _gridResizingManager_ResizeColumn(object sender, ResizeEventArgs e)
	{
		_owner.EditorPanel.SuspendDrawing();
		GridBeginUpdate();
		try
		{
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			_owner.CacheSelectRange();
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			try
			{
				for (int num = _grid.Cols.Count - 1; num >= 0; num--)
				{
					EditorSetting.Columns[num].Width = _grid.Cols[num].WidthDisplay;
				}
			}
			catch
			{
			}
			if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				int value = e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay;
				EditorSetting.Columns[e.RowCol].Width = Math.Max(1, e.HeightWidth);
				_owner.IncreaseGridWidth(value, AdjustVMWidthByOwnerGridColumnWidth);
				PopulateVmWithoutReDrawing();
				_owner.PopulateVm();
			}
			else if (e.RowCol == _grid.Cols.Count - 1)
			{
				int value2 = e.HeightWidth - _grid.Cols[_grid.Cols.Count - 1].WidthDisplay;
				_owner.IncreaseGridWidth(value2);
				PopulateVmWithoutReDrawing();
				_owner.PopulateVm();
			}
			else
			{
				int widthDisplay = _grid.Cols[e.RowCol].WidthDisplay;
				int widthDisplay2 = _grid.Cols[e.RowCol + 1].WidthDisplay;
				int num2 = e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay;
				int val = _grid.Cols[e.RowCol].WidthDisplay + num2;
				val = Math.Max(1, val);
				val = Math.Min(widthDisplay + widthDisplay2 - 1, val);
				int width = widthDisplay + widthDisplay2 - val;
				EditorSetting.Columns[e.RowCol].Width = val;
				EditorSetting.Columns[e.RowCol + 1].Width = width;
				PopulateVmWithoutReDrawing();
				AutoAdjustGridWidth();
			}
			_owner.RestoreSelectRange();
		}
		finally
		{
			GridEndUpdate();
			_owner.EditorPanel.ResumeDrawing();
		}
	}

	private void _grid_Enter(object sender, EventArgs e)
	{
		_isInEditing = true;
		if (_editor_type == EditorType.Title)
		{
			_owner.EnterTitleEdit();
		}
		else
		{
			_owner.EnterFootEdit();
		}
	}

	private void SendBodySelectionChanged_SignalR(int rowIndex, int colIndex)
	{
		_ = BodySelectionChanged_SignalR(rowIndex, colIndex);
		async Task BodySelectionChanged_SignalR(int bodyRowIndex, int bodyColIndex)
		{
			try
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(bodyRowIndex, bodyColIndex);
				if (cellVM != null && cellVM.IsTableExistCell)
				{
					Leqisoft.Model.Cell tableCell = cellVM.TableCell;
					if (tableCell != null)
					{
						long value = tableCell.Id.Value;
						await SignalRClient.UpLoadTableCellId(Leqisoft.Model.User.Current.Id.ToString(), value.ToString());
					}
				}
			}
			catch
			{
			}
		}
	}

	private void _grid_BodySelectionChanged(object sender, EventArgs e)
	{
		if (_isSuspendBodySelectionChangeEvent)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange sel = _grid.BodySelection;
		if (sel.TopRow < 0 || sel.LeftCol < 0)
		{
			return;
		}
		try
		{
			DoStats();
			DoTooltip();
			SendBodySelectionChanged_SignalR(sel.TopRow, sel.LeftCol);
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		void DoStats()
		{
			if (Table != null && Ticket != null && _vm != null)
			{
				List<double> list = new List<double>();
				for (int i = sel.TopRow; i <= sel.BottomRow; i++)
				{
					for (int j = sel.LeftCol; j <= sel.RightCol; j++)
					{
						if (_vm.GetCellVM(i, j).Value is double item)
						{
							list.Add(item);
						}
					}
				}
				if (list.Count == 0)
				{
					Program.MainForm.SelectionStats.Text = "求和：0  计数：0  平均值：0";
				}
				else
				{
					Program.MainForm.SelectionStats.Text = $"求和：{list.Sum():#,0.##############################}  计数：{list.Count}  平均值：{list.Average():#,0.##############################}";
				}
			}
		}
		void DoTooltip()
		{
			ShowTooltip();
		}
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		bool isMouseOverCancelManualInputIcon = _isMouseOverCancelManualInputIcon;
		_isMouseOverCancelManualInputIcon = false;
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Cursor cursor = TableEditor.CursorTable;
			TicketInputCellVM mergeTopLeftCellVM = _vm.GetMergeTopLeftCellVM(hitTestInfo.Row - _grid.Rows.Fixed, hitTestInfo.Column - _grid.Cols.Fixed);
			if (mergeTopLeftCellVM.Column != null)
			{
				DataFormat format = mergeTopLeftCellVM.Column.GetFormat();
				if (format.HasComboList || format.FormatType == DataFormatType.BoolOnOff || format.FormatType == DataFormatType.BoolCheckBox)
				{
					cursor = Cursors.Arrow;
				}
				if (mergeTopLeftCellVM.IsExistManualInputValue && mergeTopLeftCellVM.IsAllowManualInputOnFormula && CanEditCell(mergeTopLeftCellVM))
				{
					Rectangle cellRect = _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
					bool isIconOutOfRange;
					Rectangle cancelManualInputIconArea = GetCancelManualInputIconArea(cellRect, out isIconOutOfRange);
					if (!isIconOutOfRange && cancelManualInputIconArea.Contains(hitTestInfo.X, hitTestInfo.Y))
					{
						_isMouseOverCancelManualInputIcon = true;
						cursor = Cursors.Arrow;
					}
				}
			}
			_grid.Cursor = cursor;
		}
		else if (hitTestInfo.Type == HitTestTypeEnum.None)
		{
			_grid.Cursor = Cursors.Arrow;
		}
		if (isMouseOverCancelManualInputIcon != _isMouseOverCancelManualInputIcon)
		{
			_grid.Invalidate();
		}
	}

	private void BeforeMouseDown_OnCellIconResponseClickEvent(BeforeMouseDownEventArgs e)
	{
		if (_isMouseOverCancelManualInputIcon && e.Button == MouseButtons.Left)
		{
			HitTestTypeEnum type = _grid.HitTest(e.X, e.Y).Type;
			if (type == HitTestTypeEnum.Cell && _isMouseOverCancelManualInputIcon)
			{
				CancelSelectRangeManualInputValue();
				e.Cancel = true;
			}
		}
	}

	private void CancelSelectRangeManualInputValue()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		HashSet<TicketInputCellVM> hashSet = new HashSet<TicketInputCellVM>();
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
				if (cellVM.IsExistManualInputValue && cellVM.IsAllowManualInputOnFormula && CanEditCell(cellVM))
				{
					hashSet.Add(cellVM);
				}
			}
		}
		if (hashSet.Count == 0)
		{
			return;
		}
		_owner._vm.BeginBatchUpdateValue();
		foreach (TicketInputCellVM item in hashSet)
		{
			_owner._vm.UpdateTicketCellValue(item, string.Empty, isFormulaExistManualInputValue: false);
		}
		_owner._vm.EndBatchUpdateValue();
		Table.CalculateRecursive();
		_owner._vm.CalculateTicket();
		_grid.Invalidate();
		_owner.Grid.Invalidate();
		_owner.SetInputDataDirty();
	}

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		BeforeMouseDown_OnCellIconResponseClickEvent(e);
		if (e.Cancel || Table?.IsLocked == true || e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.Cell)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyleDisplay = _grid.GetCellStyleDisplay(hitTestInfo.Row, hitTestInfo.Column);
			Rectangle cellRect = _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
			if (cellStyleDisplay.GetImageRectangle(cellRect, _grid.Glyphs[GlyphEnum.Checked]).Contains(e.X, e.Y))
			{
				checkCellBox(hitTestInfo.Row, hitTestInfo.Column, e);
				_grid.Select();
			}
		}
	}

	private void checkCellBox(int row, int col, BeforeMouseDownEventArgs e)
	{
		if (Table?.IsLocked == true || _owner.IsTicketLocked)
		{
			return;
		}
		int @fixed = _grid.Rows.Fixed;
		int fixed2 = _grid.Cols.Fixed;
		TicketInputCellVM cellVM = _vm.GetCellVM(row - @fixed, col - fixed2);
		if (cellVM.Column == null)
		{
			return;
		}
		bool flag = false;
		DataFormat format = cellVM.Column.GetFormat();
		if (format.FormatType != DataFormatType.BoolCheckBox && format.FormatType != DataFormatType.BoolOnOff)
		{
			return;
		}
		if (e != null)
		{
			e.Cancel = true;
		}
		_owner._vm.BeginBatchUpdateValue();
		_owner._vm.StartRecordNewAddTableRows();
		_owner.ChangeVirtualValueToRealValue();
		_owner._vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
		bool flag2 = !cellVM.Value.Equals(true);
		if (_grid.Selection.Contains(row, col))
		{
			for (int i = _grid.Selection.TopRow; i <= _grid.Selection.BottomRow; i++)
			{
				for (int j = _grid.Selection.LeftCol; j <= _grid.Selection.RightCol; j++)
				{
					TicketInputCellVM cellVM2 = _vm.GetCellVM(i - @fixed, j - fixed2);
					if (cellVM2.Column == null)
					{
						continue;
					}
					DataFormat format2 = cellVM2.Column.GetFormat();
					if (format2.FormatType == DataFormatType.BoolCheckBox || format2.FormatType == DataFormatType.BoolOnOff)
					{
						_owner._vm.BuildTableCellForTicketTitleFooterCell(cellVM2);
						if (CanEditCell(cellVM2))
						{
							_owner._vm.UpdateTicketCellValue(cellVM2, flag2, isFormulaExistManualInputValue: true);
							flag = true;
						}
					}
				}
			}
		}
		else
		{
			_owner._vm.BuildTableCellForTicketTitleFooterCell(cellVM);
			if (CanEditCell(cellVM))
			{
				_owner._vm.UpdateTicketCellValue(cellVM, flag2, isFormulaExistManualInputValue: true);
				flag = true;
			}
		}
		_owner._vm.EndBatchUpdateValue();
		if (flag || Table.NeedSave)
		{
			_owner.VMData.CalculateTicket();
			_owner.SetInputDataDirty();
			_owner.Grid.Invalidate();
			_owner.TitleEditor.View.Invalidate();
			_owner.FooterEditor.View.Invalidate();
		}
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		try
		{
			Paint_Border();
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		void Paint_Border()
		{
			int count = _vm.Rows.Count;
			int count2 = _vm.Columns.Count;
			for (int i = 0; i < count; i++)
			{
				int j;
				for (j = 0; j < count2; j++)
				{
					TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(i, j));
					TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
					Rectangle cellRect = _grid.GetCellRect(i, j);
					cellRect.Offset(-1, -1);
					TicketBorder top = cellVM.GetTop(isFirstDataRow: true);
					TicketBorder bottom = cellVM.GetBottom(isLastDataRow: true);
					if (top.Width > 0 && (ticketMerge == null || i == ticketMerge.TopRow))
					{
						Rectangle rectangle = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
						if (i == 0)
						{
							int num = top.Width / 2;
							while (rectangle.Y - num < 0)
							{
								rectangle.Y++;
								rectangle.Height--;
							}
						}
						using Pen pen = new Pen(_colorBorder, top.Width);
						e.Graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
					}
					if (cellVM.Right.Width > 0 && (ticketMerge == null || j == ticketMerge.RightColumn))
					{
						Rectangle rectangle2 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
						if (i == count - 1)
						{
							rectangle2.Height++;
						}
						using Pen pen2 = new Pen(_colorBorder, cellVM.Right.Width);
						e.Graphics.DrawLine(pen2, rectangle2.Right, rectangle2.Top, rectangle2.Right, rectangle2.Bottom);
					}
					if (bottom.Width > 0 && (ticketMerge == null || i == ticketMerge.BottomRow))
					{
						Rectangle rectangle3 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
						if (i == count - 1)
						{
							int height = _grid.ClientSize.Height;
							cellRect.Height += bottom.Width + 1;
							int num2 = bottom.Width / 2;
							while (rectangle3.Y + rectangle3.Height + num2 > height)
							{
								rectangle3.Height--;
							}
						}
						using Pen pen3 = new Pen(_colorBorder, bottom.Width);
						e.Graphics.DrawLine(pen3, rectangle3.Left, rectangle3.Bottom, rectangle3.Right, rectangle3.Bottom);
					}
					if (cellVM.Left.Width > 0 && (ticketMerge == null || j == ticketMerge.LeftColumn))
					{
						Rectangle rectangle4 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
						if (j == 0)
						{
							int num3 = cellVM.Left.Width / 2;
							while (rectangle4.X - num3 < 0)
							{
								rectangle4.X++;
								rectangle4.Width--;
							}
						}
						using Pen pen4 = new Pen(_colorBorder, cellVM.Left.Width);
						e.Graphics.DrawLine(pen4, rectangle4.Left, rectangle4.Top, rectangle4.Left, rectangle4.Bottom);
					}
				}
			}
		}
	}

	private void _grid_BodyAfterRowColChange(object sender, RangeEventArgs e)
	{
		SetCommandState();
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (Table?.IsLocked != true && e.Button == MouseButtons.Right)
		{
			_ctxCell.ShowContextMenu(_grid, e.Location);
		}
	}

	private void _cmdAddAttachment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!IsExistSelectedCell() || !_owner.IsTicketLocked)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = _vm.GetCellVM(_grid.BodyRow, _grid.BodyCol).Column != null;
		}
	}

	private async void _cmdAddAttachment_Click(object sender, ClickEventArgs e)
	{
		await AddAttachment();
	}

	private void _cmdColumnWidth_Click(object sender, ClickEventArgs e)
	{
		SetColumnsWidth();
	}

	private void _cmdRowHeight_Click(object sender, ClickEventArgs e)
	{
		SetRowsHeight();
	}

	private void _cmdPaste_Click(object sender, ClickEventArgs e)
	{
		Paste();
	}

	private void _cmdCut_Click(object sender, ClickEventArgs e)
	{
		Cut();
	}

	private void _cmdCopy_Click(object sender, ClickEventArgs e)
	{
		Copy();
	}

	private static void _timeEdit_Parsing(object sender, ParseEventArgs e)
	{
		if (TimeSpan.TryParseExact(e.Text, _timeEdit.EditFormat.CustomFormat, null, out var result))
		{
			e.Succeeded = true;
			e.Value = result;
		}
		else
		{
			e.Succeeded = false;
		}
	}
}
