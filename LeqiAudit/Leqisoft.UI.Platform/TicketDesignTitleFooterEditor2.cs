﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class TicketDesignTitleFooterEditor2
{
	public enum EditorType
	{
		Title,
		Footer
	}

	protected class ValueChangeData
	{
		public int OldValue;

		public int NewValue;

		public ValueChangeData()
		{
		}

		public ValueChangeData(int oldValue, int newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	private C1FlexGridEx _grid;

	private C1CommandMenu _cmdField;

	private C1Command _cmdSetColumnWidth;

	private C1ContextMenu _ctxCell;

	private GridResizingManager _gridResizingManager;

	private bool _isDragging;

	private bool _skipGridSelChange;

	private bool _isInEditing;

	private EditorType _editor_type;

	private Color _colorBorder;

	private static readonly int _dragRectTopLeftEdgeOffset = 1;

	private static readonly int _dragRectBottomRightEdgeOffset = 2;

	private static readonly Pen _pen = new Pen(Color.Red, 3f);

	private static readonly HatchBrush _brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Red, Color.Transparent);

	private static readonly SolidBrush _brushText = new SolidBrush(Color.Black);

	private static readonly StringFormat _sf = new StringFormat
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	private static readonly Regex _rxCol = new Regex("\\[.*]");

	private TicketDesignEditor2 _owner;

	protected Leqisoft.Model.Table _table => _owner.Table;

	public TicketDesignTitleFooterVM _vm { get; set; }

	public C1FlexGridEx View => _grid;

	public bool IsInEditing => _isInEditing;

	public bool IsInDragging => _isDragging;

	public TicketDesignTitleFooterEditor2(TicketDesignEditor2 owner, EditorType editorType)
	{
		_owner = owner;
		_editor_type = editorType;
		InitTableGrid();
		InitContextMenu();
	}

	private void InitTableGrid()
	{
		_grid = new C1FlexGridEx
		{
			AllowDragging = AllowDraggingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			ScrollBars = ScrollBars.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowEditing = true,
			VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom,
			DrawMode = DrawModeEnum.OwnerDraw,
			DragMode = DragModeEnum.Manual,
			DropMode = DropModeEnum.Manual,
			FocusRect = FocusRectEnum.None
		};
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 3;
		_grid.Cols.Fixed = 0;
		_grid.Rows.DefaultSize = 30;
		_grid.Cols.DefaultSize = 100;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.BodyBeforeEdit += _grid_BodyBeforeEdit;
		_grid.BodyAfterEdit += _grid_BodyAfterEdit;
		_grid.BodyAfterRowColChange += _grid_BodyAfterRowColChange;
		_grid.MouseDown += _grid_MouseDown;
		_grid.MouseUp += _grid_MouseUp;
		_grid.MouseClick += _grid_MouseClick;
		_grid.KeyDown += _grid_KeyDown;
		_grid.SetupEditor += _grid_SetupEditor;
		_grid.Paint += _grid_Paint;
		_grid.DragOver += _grid_DragOver;
		_grid.DragDrop += _grid_DragDrop;
		_grid.DragEnter += _grid_DragEnter;
		_grid.DragLeave += _grid_DragLeave;
		_grid.Enter += _grid_Enter;
		_gridResizingManager = new GridResizingManager(_grid);
		_gridResizingManager.ResizeColumn += _gridResizingManager_ResizeColumn;
		_gridResizingManager.ResizeRow += _gridResizingManager_ResizeRow;
	}

	private void InitContextMenu()
	{
		_ctxCell = new C1ContextMenu();
		C1Command c1Command = new C1Command();
		c1Command.Text = ((_editor_type == EditorType.Title) ? "插入标题行" : "插入表底行");
		c1Command.Click += Cmd_Click_InsertRow;
		c1Command.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		c1Command.Image = ContextResources.ctxInsertRow;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command));
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = ((_editor_type == EditorType.Title) ? "新增标题行" : "新增表底行");
		c1Command2.Image = ContextResources.ctxAppendRow;
		c1Command2.Click += Cmd_Click_AppendRow;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command2));
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = ((_editor_type == EditorType.Title) ? "删除标题行" : "删除表底行");
		c1Command3.Image = ContextResources.ctxDeleteRow;
		c1Command3.CommandStateQuery += Cmd_CommandStateQuery_DeleteRow;
		c1Command3.Click += Cmd_Click_RemoveRows;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command3));
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = ((_editor_type == EditorType.Title) ? "插入标题列" : "插入表底列");
		c1Command4.Click += Cmd_Click_InsertColumn;
		c1Command4.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		c1Command4.Image = ContextResources.ctxInsertColumn;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command4)
		{
			Delimiter = true
		});
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = ((_editor_type == EditorType.Title) ? "新增标题列" : "新增表底列");
		c1Command5.Click += Cmd_Click_AppendColumn;
		c1Command5.Image = ContextResources.ctxAppendColumn;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command5));
		C1Command c1Command6 = new C1Command();
		c1Command6.Text = ((_editor_type == EditorType.Title) ? "删除标题列" : "删除表底列");
		c1Command6.Click += Cmd_Click_RemoveColumns;
		c1Command6.CommandStateQuery += Cmd_CommandStateQuery_RemoveColumns;
		c1Command6.Image = ContextResources.ctxDeleteColumn;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command6));
		C1Command c1Command7 = new C1Command();
		c1Command7.Text = "设置行高...";
		c1Command7.Click += Cmd_Click_SetRowHeight;
		c1Command7.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command7)
		{
			Delimiter = true
		});
		C1Command c1Command8 = new C1Command();
		c1Command8.Text = "设置列宽...";
		c1Command8.Click += Cmd_Click_SetColumnWidth;
		c1Command8.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		C1Command c1Command9 = new C1Command();
		c1Command9.Text = "复制";
		c1Command9.Image = ContextResources.ctxCopy;
		c1Command9.Click += _cmdCopy_Click;
		c1Command9.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command9)
		{
			Delimiter = true
		});
		C1Command c1Command10 = new C1Command();
		c1Command10.Text = "剪切";
		c1Command10.Image = ContextResources.ctxCut;
		c1Command10.Click += _cmdCut_Click;
		c1Command10.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command10));
		C1Command c1Command11 = new C1Command();
		c1Command11.Text = "粘贴";
		c1Command11.Image = ContextResources.ctxPaste;
		c1Command11.Click += _cmdPaste_Click;
		c1Command11.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command11));
		C1Command c1Command12 = new C1Command();
		c1Command12.Text = "合并单元格";
		c1Command12.Click += Cmd_Click_MergeCells;
		c1Command12.CommandStateQuery += Cmd_CommandStateQuery_MergeCells;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command12)
		{
			Delimiter = true
		});
		C1Command c1Command13 = new C1Command();
		c1Command13.Text = "仅横向合并单元格";
		c1Command13.Click += Cmd_Click_OnlyMergeHorizontalCells;
		c1Command13.CommandStateQuery += Cmd_CommandStateQuery_OnlyMergeHorizontalCells;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command13));
		C1Command c1Command14 = new C1Command();
		c1Command14.Text = "拆分单元格";
		c1Command14.Click += Cmd_Click_SplitCells;
		c1Command14.CommandStateQuery += Cmd_CommandStateQuery_SplitCells;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command14));
		C1CommandMenu c1CommandMenu = new C1CommandMenu();
		c1CommandMenu.Text = "对齐";
		c1CommandMenu.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		AddAlignSubMenu(c1CommandMenu, "左上对齐", ContextResources.ctxAlignTopLeft, CellTextAlign.TopLeft);
		AddAlignSubMenu(c1CommandMenu, "左中对齐", ContextResources.ctxAlignMiddleLeft, CellTextAlign.MiddleLeft);
		AddAlignSubMenu(c1CommandMenu, "左下对齐", ContextResources.ctxAlignBottomLeft, CellTextAlign.BottomLeft);
		AddAlignSubMenu(c1CommandMenu, "中上对齐", ContextResources.ctxAlignTopCenter, CellTextAlign.TopCenter);
		AddAlignSubMenu(c1CommandMenu, "中中对齐", ContextResources.ctxAlignMiddleCenter, CellTextAlign.MiddleCenter);
		AddAlignSubMenu(c1CommandMenu, "中下对齐", ContextResources.ctxAlignBottomCenter, CellTextAlign.BottomCenter);
		AddAlignSubMenu(c1CommandMenu, "右上对齐", ContextResources.ctxAlignTopRight, CellTextAlign.TopRight);
		AddAlignSubMenu(c1CommandMenu, "右中对齐", ContextResources.ctxAlignMiddleRight, CellTextAlign.MiddleRight);
		AddAlignSubMenu(c1CommandMenu, "右下对齐", ContextResources.ctxAlignBottomRight, CellTextAlign.BottomRight);
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1CommandMenu)
		{
			Delimiter = true
		});
		C1CommandMenu c1CommandMenu2 = new C1CommandMenu();
		c1CommandMenu2.Text = "数据格式";
		c1CommandMenu2.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1CommandMenu2));
		C1CommandMenu c1CommandMenu3 = new C1CommandMenu();
		c1CommandMenu3.Text = "数值格式";
		AddFormatSubMenu_Numeric(c1CommandMenu3, "1234.56", DataFormatType.Number);
		AddFormatSubMenu_Numeric(c1CommandMenu3, "1,234.56", DataFormatType.Comma);
		AddFormatSubMenu_Numeric(c1CommandMenu3, "$1,234.56", DataFormatType.NumDollar);
		AddFormatSubMenu_Numeric(c1CommandMenu3, "￥1,234.56", DataFormatType.NumRmb);
		AddFormatSubMenu_Numeric(c1CommandMenu3, "123,456.78%", DataFormatType.Percentage);
		c1CommandMenu2.CommandLinks.Add(new C1CommandLink(c1CommandMenu3));
		C1CommandMenu c1CommandMenu4 = new C1CommandMenu();
		c1CommandMenu4.Text = "日期格式";
		AddFormatSubMenu_Default(c1CommandMenu4, "2017年12月31日", DataFormatType.DateChinese);
		AddFormatSubMenu_Default(c1CommandMenu4, "2017-12-31", DataFormatType.DateDash);
		AddFormatSubMenu_Default(c1CommandMenu4, "2017/12/31", DataFormatType.DateSlash);
		AddFormatSubMenu_Default(c1CommandMenu4, "2017.12.31", DataFormatType.DateDot);
		AddFormatSubMenu_Default(c1CommandMenu4, "2017年12月", DataFormatType.DateYearMonthChinese, isAddDelimiter: true);
		AddFormatSubMenu_Default(c1CommandMenu4, "2017-12", DataFormatType.DateYearMonthDash);
		AddFormatSubMenu_Default(c1CommandMenu4, "2017/12", DataFormatType.DateYearMonthSlash);
		AddFormatSubMenu_Default(c1CommandMenu4, "2017.12", DataFormatType.DateYearMonthDot);
		c1CommandMenu2.CommandLinks.Add(new C1CommandLink(c1CommandMenu4));
		C1CommandMenu c1CommandMenu5 = new C1CommandMenu();
		c1CommandMenu5.Text = "时间格式";
		AddFormatSubMenu_Default(c1CommandMenu5, "10时20分30秒", DataFormatType.TimeLongChinese);
		AddFormatSubMenu_Default(c1CommandMenu5, "10时20分", DataFormatType.TimeShortChinese);
		AddFormatSubMenu_Default(c1CommandMenu5, "10:20:30", DataFormatType.TimeLong);
		AddFormatSubMenu_Default(c1CommandMenu5, "10:20", DataFormatType.TimeShort);
		c1CommandMenu2.CommandLinks.Add(new C1CommandLink(c1CommandMenu5));
		C1CommandMenu c1CommandMenu6 = new C1CommandMenu();
		c1CommandMenu6.Text = "判断格式";
		AddFormatSubMenu_Default(c1CommandMenu6, "复选框", DataFormatType.BoolCheckBox);
		AddFormatSubMenu_Default(c1CommandMenu6, "开关钮", DataFormatType.BoolOnOff);
		c1CommandMenu2.CommandLinks.Add(new C1CommandLink(c1CommandMenu6));
		_cmdField = new C1CommandMenu();
		_cmdField.Text = "对应表格列";
		_cmdField.Image = Resources.TicketSetField;
		_cmdField.CommandStateQuery += Cmd_CommandStateQuery_Field;
		_cmdField.CommandLinks.Add(new C1CommandLink());
		_cmdField.Popup += Cmd_Popup_Field;
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdField)
		{
			Delimiter = true
		});
		C1Command c1Command15 = new C1Command();
		c1Command15.Text = "删除单元格公式";
		c1Command15.Image = ContextResources.ctxDeleteFormula;
		c1Command15.CommandStateQuery += Cmd_CommandStateQuery_DeleteFormula;
		c1Command15.Click += Cmd_Click_DeleteFormula;
		_ctxCell.CommandLinks.Add(new C1CommandLink(c1Command15)
		{
			Delimiter = true
		});
	}

	private void AddAlignSubMenu(C1CommandMenu menu, string text, System.Drawing.Image image, CellTextAlign align)
	{
		C1Command c1Command = new C1Command
		{
			Text = text,
			Image = image
		};
		c1Command.Click += delegate
		{
			SetAlign(align);
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		menu.CommandLinks.Add(value);
	}

	private void AddFormatSubMenu_Numeric(C1CommandMenu menu, string text, DataFormatType dft)
	{
		C1Command c1Command = new C1Command
		{
			Text = text
		};
		c1Command.Click += delegate
		{
			SetFormatNumeric(dft);
		};
		C1CommandLink value = new C1CommandLink(c1Command);
		menu.CommandLinks.Add(value);
	}

	private void AddFormatSubMenu_Default(C1CommandMenu parent, string text, DataFormatType dft, bool isAddDelimiter = false)
	{
		C1Command c1Command = new C1Command
		{
			Text = text
		};
		c1Command.Click += delegate
		{
			SetFormatDefault(dft);
		};
		C1CommandLink value = new C1CommandLink(c1Command)
		{
			Delimiter = isAddDelimiter
		};
		parent.CommandLinks.Add(value);
	}

	private void Cmd_Click_InsertRow(object sender, ClickEventArgs e)
	{
		int topRow = _grid.Selection.TopRow;
		_vm.InsertRows(topRow, 1);
		_grid.Rows.Insert(topRow);
		if (topRow == 0 && _editor_type == EditorType.Title)
		{
			for (int i = 0; i < _vm.Columns.Count; i++)
			{
				TicketDesignCellVM cell = _vm.GetCell(0, i);
				cell.FontSize = 14f;
				cell.Bold = true;
			}
		}
		PopulateMerge();
		AutoAdjustGridHeight();
		_owner.AutoAdjustDataGridPosition();
	}

	private void Cmd_Click_AppendRow(object sender, ClickEventArgs e)
	{
		_vm.AppendRows(1);
		_grid.Rows.Insert(_grid.Rows.Count);
		AutoAdjustGridHeight();
		_owner.AutoAdjustDataGridPosition();
	}

	private void Cmd_Click_RemoveRows(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.BottomRow >= 0)
		{
			int num = Math.Max(0, selection.TopRow);
			int count = selection.BottomRow - num + 1;
			_vm.RemoveRows(num, count);
			_grid.Rows.RemoveRange(num, count);
			PopulateMerge();
			AutoAdjustGridHeight();
			_owner.AutoAdjustDataGridPosition();
		}
	}

	private void Cmd_CommandStateQuery_DeleteRow(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void Cmd_Click_SetRowHeight(object sender, ClickEventArgs e)
	{
		SetRowHeight();
	}

	private void _grid_SetupEditor(object sender, RowColEventArgs e)
	{
		if (_grid.Editor != null)
		{
			_grid.Editor.Top++;
			_grid.Editor.Left++;
			_grid.Editor.Width--;
			_grid.Editor.Height--;
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

	private void AdjustVMWidthByOwnerGridColumnWidth()
	{
		_grid.BeginUpdate();
		try
		{
			AdjustVMWidthByOwnerGridColumnWidthImpl();
			if (_editor_type == EditorType.Title)
			{
				((dynamic)_owner.FooterEditor).AutoAdjustGridWidth();
			}
			else
			{
				((dynamic)_owner.TitleEditor).AutoAdjustGridWidth();
			}
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	private void AdjustVMWidthByOwnerGridColumnWidthImpl()
	{
		int count = _grid.Cols.Count;
		int[] array = new int[count];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			num += (array[i] = _vm.Columns[i].Width);
		}
		for (int j = 0; j < count; j++)
		{
			_grid.Cols[j].Width = array[j];
		}
		AutoAdjustGridWidthWithOutForceReDrawing();
	}

	private void _gridResizingManager_ResizeColumn(object sender, ResizeEventArgs e)
	{
		if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
		{
			int value = e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay;
			int width = Math.Max(1, e.HeightWidth);
			_vm.Columns[e.RowCol].Width = width;
			_owner.IncreaseGridWidth(value, (Action)AdjustVMWidthByOwnerGridColumnWidth);
			return;
		}
		if (e.RowCol == _grid.Cols.Count - 1)
		{
			_owner.IncreaseGridWidth(e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay);
			return;
		}
		_grid.BeginUpdate();
		try
		{
			int widthDisplay = _grid.Cols[e.RowCol].WidthDisplay;
			int widthDisplay2 = _grid.Cols[e.RowCol + 1].WidthDisplay;
			int num = e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay;
			int val = _grid.Cols[e.RowCol].WidthDisplay + num;
			val = Math.Max(1, val);
			val = Math.Min(widthDisplay + widthDisplay2 - 1, val);
			int width2 = widthDisplay + widthDisplay2 - val;
			_grid.Cols[e.RowCol].Width = val;
			_grid.Cols[e.RowCol + 1].Width = width2;
			_vm.Columns[e.RowCol].Width = val;
			_vm.Columns[e.RowCol + 1].Width = width2;
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	private void _gridResizingManager_ResizeRow(object sender, ResizeEventArgs e)
	{
		_grid.Rows[e.RowCol].Height = e.HeightWidth;
		_vm.Rows[e.RowCol].Height = e.HeightWidth;
		AutoAdjustGridHeight();
		_owner.AutoAdjustDataGridPosition();
	}

	public void LeaveEdit()
	{
		_isInEditing = false;
		CancelSelect();
	}

	public void LeaveDrag()
	{
		_isDragging = false;
		_grid.Invalidate();
	}

	public void FinishEditing()
	{
		if (!_isInEditing)
		{
			return;
		}
		try
		{
			if (_grid.Editor != null)
			{
				_grid.FinishEditing(cancel: false);
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	public void CancelSelect()
	{
		_grid.Select(-1, -1);
	}

	private void VMCellsDoAction(int rowIndex, int colIndex, Action<int, int, TicketDesignCellVM> action)
	{
		TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(rowIndex, colIndex));
		if (ticketMerge != null)
		{
			for (int i = ticketMerge.TopRow; i <= ticketMerge.BottomRow; i++)
			{
				for (int j = ticketMerge.LeftColumn; j <= ticketMerge.RightColumn; j++)
				{
					TicketDesignCellVM cell = _vm.GetCell(i, j);
					action(i, j, cell);
				}
			}
		}
		else
		{
			TicketDesignCellVM cell2 = _vm.GetCell(rowIndex, colIndex);
			action(rowIndex, colIndex, cell2);
		}
	}

	private TicketMerge GetVMCellRangeOuterEdge(C1.Win.C1FlexGrid.CellRange range)
	{
		TicketMerge ticketMerge = new TicketMerge();
		ticketMerge.TopRow = range.TopRow;
		ticketMerge.BottomRow = range.BottomRow;
		ticketMerge.LeftColumn = range.LeftCol;
		ticketMerge.RightColumn = range.RightCol;
		for (int i = range.r1; i <= range.r2; i++)
		{
			int j;
			for (j = range.c1; j <= range.c2; j++)
			{
				TicketMerge ticketMerge2 = _vm.Merges.FirstOrDefault((TicketMerge u) => u.Contains(i, j));
				if (ticketMerge2 != null)
				{
					ticketMerge.TopRow = Math.Min(ticketMerge2.TopRow, ticketMerge.TopRow);
					ticketMerge.BottomRow = Math.Max(ticketMerge2.BottomRow, ticketMerge.BottomRow);
					ticketMerge.LeftColumn = Math.Min(ticketMerge2.LeftColumn, ticketMerge.LeftColumn);
					ticketMerge.RightColumn = Math.Max(ticketMerge2.RightColumn, ticketMerge.RightColumn);
				}
				else
				{
					ticketMerge.TopRow = Math.Min(i, ticketMerge.TopRow);
					ticketMerge.BottomRow = Math.Max(i, ticketMerge.BottomRow);
					ticketMerge.LeftColumn = Math.Min(j, ticketMerge.LeftColumn);
					ticketMerge.RightColumn = Math.Max(j, ticketMerge.RightColumn);
				}
			}
		}
		return ticketMerge;
	}

	public void SetFontFamily(string ff)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.FontFamily = ff;
				});
			}
		}
		_grid.Invalidate();
	}

	public void SetFontSize(float fs)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.FontSize = fs;
				});
			}
		}
		_grid.Invalidate();
	}

	public void SetForeColor(Color c)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM u)
				{
					u.ForeColor = c;
				});
			}
		}
		_grid.Invalidate();
	}

	public void SetBackColor(Color c)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM u)
				{
					u.BackColor = c;
				});
			}
		}
		_grid.Invalidate();
	}

	public void SetBold(bool b)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM u)
				{
					u.Bold = b;
				});
			}
		}
		_grid.Invalidate();
	}

	public void SetItalic(bool b)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM u)
				{
					u.Italic = b;
				});
			}
		}
		_grid.Invalidate();
	}

	public void SetAlign(CellTextAlign a)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM u)
				{
					u.Align = a;
				});
			}
		}
		_grid.Invalidate();
	}

	public void GrowFont()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		float fs = FontSize.Grow(_vm.GetCell(bodySelection.TopRow, bodySelection.LeftCol).FontSize);
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM u)
				{
					u.FontSize = fs;
				});
			}
		}
		AppCommands.TicketFontSize.FontSizeSelector.SelectFontSize(fs);
		_grid.Invalidate();
	}

	public void ShrinkFont()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		float fs = FontSize.Shrink(_vm.GetCell(bodySelection.TopRow, bodySelection.LeftCol).FontSize);
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM u)
				{
					u.FontSize = fs;
				});
			}
		}
		AppCommands.TicketFontSize.FontSizeSelector.SelectFontSize(fs);
		_grid.Invalidate();
	}

	public void BorderTop()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			VMCellsDoAction(bodySelection.TopRow, i, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Top.Width = 1;
			});
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderBottom()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			VMCellsDoAction(bodySelection.BottomRow, i, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Bottom.Width = 1;
			});
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderLeft()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			VMCellsDoAction(i, bodySelection.LeftCol, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Left.Width = 1;
			});
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderRight()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			VMCellsDoAction(i, bodySelection.RightCol, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Right.Width = 1;
			});
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderNone()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.Top.Width = 0;
					c.Right.Width = 0;
					c.Bottom.Width = 0;
					c.Left.Width = 0;
				});
			}
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderAll()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.Top.Width = 1;
					c.Right.Width = 1;
					c.Bottom.Width = 1;
					c.Left.Width = 1;
				});
			}
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderStyle1()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		TicketMerge edge = GetVMCellRangeOuterEdge(bodySelection);
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.Top.Width = ((row != edge.TopRow) ? 1 : 2);
					c.Right.Width = ((col != edge.RightColumn) ? 1 : 0);
					c.Bottom.Width = ((row != edge.BottomRow) ? 1 : 2);
					c.Left.Width = ((col != edge.LeftColumn) ? 1 : 0);
				});
			}
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderStyle2()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		TicketMerge edge = GetVMCellRangeOuterEdge(bodySelection);
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.Top.Width = ((row != edge.TopRow) ? 1 : 2);
					c.Right.Width = ((col != edge.RightColumn) ? 1 : 2);
					c.Bottom.Width = ((row != edge.BottomRow) ? 1 : 2);
					c.Left.Width = ((col != edge.LeftColumn) ? 1 : 2);
				});
			}
		}
		_owner.AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void SetFormatText()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.DataFormat = null;
				});
			}
		}
	}

	public void SetFormatNumeric(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.DataFormat = new DataFormat(dft)
					{
						DecimalLength = 2,
						ZeroFormat = ZeroFormat.Empty
					};
				});
			}
		}
	}

	public void SetFormatDefault(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					c.DataFormat = new DataFormat(dft);
				});
			}
		}
	}

	public void SetZeroFormat(ZeroFormat zf)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					if (!c.DataFormat.HasValue)
					{
						c.DataFormat = new DataFormat(DataFormatType.Number)
						{
							ZeroFormat = zf
						};
					}
					else
					{
						DataFormat value = c.DataFormat.Value.Clone();
						value.ZeroFormat = zf;
						c.DataFormat = value;
					}
				});
			}
		}
	}

	public void MorePrecision()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					if (c.DataFormat.HasValue)
					{
						DataFormat value = c.DataFormat.Value.Clone();
						value.DecimalLength++;
						c.DataFormat = value;
					}
				});
			}
		}
	}

	public void LessPrecision()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				VMCellsDoAction(i, j, delegate(int row, int col, TicketDesignCellVM c)
				{
					if (c.DataFormat.HasValue)
					{
						DataFormat value = c.DataFormat.Value.Clone();
						value.DecimalLength--;
						c.DataFormat = value;
					}
				});
			}
		}
	}

	public void SplitCells()
	{
		C1.Win.C1FlexGrid.CellRange sel = _grid.BodySelection;
		_vm.Merges.RemoveAll((TicketMerge m) => sel.Contains(m.TopRow, m.LeftColumn));
		PopulateMerge();
	}

	public void MergeCells()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		_vm.MergeCells(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol);
		PopulateMerge();
	}

	public void MergeCells(int topRow, int leftCol, int bottomRow, int rightCol)
	{
		_vm.MergeCells(topRow, leftCol, bottomRow, rightCol);
		PopulateMerge();
	}

	public void OnlyMergeHorizontalCells()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			_vm.MergeCells(i, bodySelection.LeftCol, i, bodySelection.RightCol);
		}
		PopulateMerge();
	}

	public void SetTheme()
	{
		Theme.SetCurrentTree(View);
		_colorBorder = Theme.SelectedLeqiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
		_grid.Styles.Normal.Border.Color = _owner.NormalCellBorderColor;
		_grid.Styles.Normal.WordWrap = true;
	}

	public void Populate()
	{
		_grid.BeginUpdate();
		try
		{
			PopulateImpl();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	protected void PopulateImpl()
	{
		int rowsCount = _vm.GetRowsCount();
		int columnsCount = _vm.GetColumnsCount();
		_skipGridSelChange = true;
		_grid.BodyRowsCount = rowsCount;
		_grid.BodyColsCount = columnsCount;
		_skipGridSelChange = false;
		int num = 0;
		for (int i = 0; i < rowsCount; i++)
		{
			int height = _vm.GetRow(i).Height;
			_grid.BodyGetRow(i).Height = height;
			num += height;
		}
		for (int j = 0; j < columnsCount; j++)
		{
			_grid.BodyGetCol(j).Width = _vm.GetColumn(j).Width;
		}
		AutoAdjustGridHeight();
		PopulateMerge();
	}

	private void OnRowsCountChanged()
	{
		AutoAdjustGridHeight();
		_owner.AutoAdjustDataGridPosition();
	}

	private void OnAddNewColumns(int girdNewWidth)
	{
		bool flag = _grid.Width != girdNewWidth;
		AdjustGridWidthImpl(GetGridAbleWidth(girdNewWidth));
		if (flag)
		{
			_owner.AutoAdjustDataGridPosition();
		}
	}

	private void OnRemoveColumns(int girdNewWidth)
	{
		bool flag = _grid.Width != girdNewWidth;
		AdjustGridWidthImpl(GetGridAbleWidth(girdNewWidth));
		if (flag)
		{
			_owner.AutoAdjustDataGridPosition();
		}
	}

	public void AppendColumn()
	{
		if (SoftwareLicenseManager.IsTableColsOutOfLicenseLimit(1 + _vm.Columns.Count))
		{
			return;
		}
		_grid.BeginUpdate();
		try
		{
			int width = _grid.Width;
			_vm.AppendColumns(1);
			_grid.Cols.Add(1);
			OnAddNewColumns(width);
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void AppendColumns()
	{
		decimal? num = InputForm.Numeric("追加列", "请输入列数：", 1);
		if (num.HasValue && !SoftwareLicenseManager.IsTableColsOutOfLicenseLimit((int)num.Value + _vm.Columns.Count))
		{
			_grid.BeginUpdate();
			try
			{
				int width = _grid.Width;
				_vm.AppendColumns((int)num.Value);
				_grid.Cols.Add((int)num.Value);
				OnAddNewColumns(width);
			}
			finally
			{
				_grid.EndUpdate();
			}
		}
	}

	public void InsertColumn()
	{
		if (SoftwareLicenseManager.IsTableColsOutOfLicenseLimit(1 + _vm.Columns.Count))
		{
			return;
		}
		_grid.BeginUpdate();
		try
		{
			int width = _grid.Width;
			_vm.InsertColumns(_grid.BodyCol, 1);
			_grid.Cols.InsertRange(_grid.BodyCol, 1);
			PopulateMerge();
			OnAddNewColumns(width);
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void InsertColumns()
	{
		decimal? num = InputForm.Numeric("插入列", "请输入列数：", 1);
		if (num.HasValue && !SoftwareLicenseManager.IsTableColsOutOfLicenseLimit((int)num.Value + _vm.Columns.Count))
		{
			_grid.BeginUpdate();
			try
			{
				int width = _grid.Width;
				_vm.InsertColumns(_grid.BodyCol, (int)num.Value);
				_grid.Cols.InsertRange(_grid.BodyCol, (int)num.Value);
				PopulateMerge();
				OnAddNewColumns(width);
			}
			finally
			{
				_grid.EndUpdate();
			}
		}
	}

	public void ResetGrid(int rowCount, int colCount)
	{
		_grid.BeginUpdate();
		try
		{
			_vm.Clear();
			_vm.AppendColumns(colCount);
			_vm.AppendRows(rowCount);
			_grid.Rows.Count = 0;
			_grid.Cols.Count = 0;
			_grid.Rows.InsertRange(0, rowCount);
			_grid.Cols.InsertRange(0, colCount);
			AutoAdjustGridWidthWithOutForceReDrawing();
			OnRowsCountChanged();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void AppendRows()
	{
		decimal? num = InputForm.Numeric("追加行", "请输入行数：");
		if (num.HasValue && !SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit((int)num.Value + _vm.Rows.Count))
		{
			_grid.BeginUpdate();
			try
			{
				_vm.AppendRows((int)num.Value);
				_grid.Rows.Add((int)num.Value);
				OnRowsCountChanged();
			}
			finally
			{
				_grid.EndUpdate();
			}
		}
	}

	public void InsertRows()
	{
		decimal? num = InputForm.Numeric("插入行", "请输入行数：");
		if (num.HasValue && !SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit((int)num.Value + _vm.Rows.Count))
		{
			_grid.BeginUpdate();
			try
			{
				_vm.InsertRows(_grid.BodyRow, (int)num.Value);
				_grid.Rows.InsertRange(_grid.BodyRow, (int)num.Value);
				PopulateMerge();
				OnRowsCountChanged();
			}
			finally
			{
				_grid.EndUpdate();
			}
		}
	}

	public void RemoveColumns()
	{
		_grid.BeginUpdate();
		try
		{
			int width = _grid.Width;
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			_vm.RemoveColumns(bodySelection.LeftCol, bodySelection.RightCol - bodySelection.LeftCol + 1);
			_grid.Cols.RemoveRange(bodySelection.LeftCol, bodySelection.RightCol - bodySelection.LeftCol + 1);
			if (_vm.GetColumnsCount() == 0)
			{
				_vm.AppendColumns(1);
				_grid.Cols.Add(1);
			}
			PopulateMerge();
			OnRemoveColumns(width);
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void RemoveRows()
	{
		_grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			_vm.RemoveRows(bodySelection.TopRow, bodySelection.BottomRow - bodySelection.TopRow + 1);
			_grid.Rows.RemoveRange(bodySelection.TopRow, bodySelection.BottomRow - bodySelection.TopRow + 1);
			PopulateMerge();
			OnRowsCountChanged();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public void SetColumnWidth()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		decimal? num = InputForm.Numeric("设置列宽", "请输入列宽（像素）：", _vm.GetColumn(bodySelection.LeftCol).Width);
		if (!num.HasValue)
		{
			return;
		}
		int num2 = (int)num.Value;
		if (num2 < 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列宽不能小于0");
			return;
		}
		if (num2 > 9999)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列宽不能大于9999");
			return;
		}
		_owner.EditorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
			{
				_vm.GetColumn(i).Width = num2;
				_grid.Cols[i].Width = num2;
			}
			SetColToWidthWithoutForceReDrawing(bodySelection.LeftCol, bodySelection.RightCol);
			_owner.AutoAdjustDataGridPosition();
		}
		finally
		{
			_grid.EndUpdate();
			_owner.EditorPanel.ResumeDrawing();
		}
	}

	public void SetRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		int num = ((selection.TopRow >= 0 && selection.TopRow < _grid.Rows.Count) ? _vm.GetRow(selection.TopRow).Height : _vm.GetRow(0).Height);
		decimal? num2 = InputForm.Numeric("设置行高", "请输入行高（像素）：", num);
		if (!num2.HasValue)
		{
			return;
		}
		int num3 = (int)num2.Value;
		if (num3 < 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能小于0");
			return;
		}
		if (num3 > 9999)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能大于9999");
			return;
		}
		_grid.BeginUpdate();
		try
		{
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				_grid.Rows[i].Height = num3;
				_vm.GetRow(i).Height = num3;
			}
			AutoAdjustGridHeight();
			_owner.AutoAdjustDataGridPosition();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	protected string GetFormulaContextCellLabel(int row, int col)
	{
		if (_editor_type == EditorType.Title)
		{
			return string.Format("{0}({1},{2})", "TicketTitle", row, col);
		}
		return string.Format("{0}({1},{2})", "TicketFoot", row, col);
	}

	public string GetFormulaContextCellLabel()
	{
		return GetFormulaContextCellLabel(_grid.BodyRow + 1, _grid.BodyCol + 1);
	}

	public void Indent()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				_vm.GetCell(i, j).Indent += 10;
			}
		}
		_grid.Invalidate();
	}

	public void Unindent()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				_vm.GetCell(i, j).Indent -= 10;
			}
		}
		_grid.Invalidate();
	}

	public void Cut()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketDesignCellVM cell = _vm.GetCell(i, j);
				stringBuilder.Append(cell.Text);
				if (j < bodySelection.RightCol)
				{
					stringBuilder.Append("\t");
				}
				cell.Text = "";
			}
			stringBuilder.Append("\r\n");
		}
		Clipboard.SetText(stringBuilder.ToString());
		_grid.Invalidate();
	}

	public void Copy()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		TicketDesignCellVM cell = _vm.GetCell(bodySelection.TopRow, bodySelection.LeftCol);
		DataObject dataObject = new DataObject();
		if (cell.HasFormula())
		{
			ClipboardTicketDesignCell data = new ClipboardTicketDesignCell
			{
				Row = bodySelection.TopRow,
				Col = bodySelection.LeftCol,
				Text = cell.Text,
				Formula = cell.Formula
			};
			dataObject.SetData("LeqiClipboardTicketDesignCell", data);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketDesignCellVM cell2 = _vm.GetCell(i, j);
				stringBuilder.Append(cell2.Text);
				if (j < bodySelection.RightCol)
				{
					stringBuilder.Append("\t");
				}
			}
			stringBuilder.Append("\r\n");
		}
		if (stringBuilder.Length > 0)
		{
			dataObject.SetText(stringBuilder.ToString());
		}
		Clipboard.SetDataObject(dataObject);
	}

	public void Paste()
	{
		if (Clipboard.ContainsData("LeqiClipboardTicketDesignCell"))
		{
			ClipboardTicketDesignCell clipboardTicketDesignCell = (ClipboardTicketDesignCell)Clipboard.GetData("LeqiClipboardTicketDesignCell");
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
				{
					try
					{
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(clipboardTicketDesignCell.Formula);
						TicketDesignCellVM cell = _vm.GetCell(i, j);
						cell.Text = clipboardTicketDesignCell.Text;
						cell.UpdateFormula(formulaEvaluator.PasteTicketFormula(i - clipboardTicketDesignCell.Row, j - clipboardTicketDesignCell.Col, _vm.GetRowsCount(), _vm.GetColumnsCount()));
						SetCommandState();
					}
					catch (FormulaException)
					{
					}
				}
			}
		}
		else
		{
			try
			{
				List<List<object>> clipboardAsTable = ClipboardUtil.GetClipboardAsTable();
				if (clipboardAsTable != null && clipboardAsTable.Count > 0)
				{
					int bodyRow = _grid.BodyRow;
					int num = Math.Min(Math.Max(clipboardAsTable.Count, _grid.BodyRowSel - bodyRow + 1), _vm.GetRowsCount() - bodyRow);
					int bodyCol = _grid.BodyCol;
					int num2 = Math.Min(Math.Max(clipboardAsTable[0].Count, _grid.BodyColSel - bodyCol + 1), _vm.GetColumnsCount() - bodyCol);
					int count = clipboardAsTable.Count;
					int count2 = clipboardAsTable[0].Count;
					for (int k = 0; k < num; k++)
					{
						List<object> list = clipboardAsTable[k % count];
						for (int l = 0; l < num2; l++)
						{
							object obj = list[l % count2] ?? "";
							TicketDesignCellVM cell2 = _vm.GetCell(bodyRow + k, bodyCol + l);
							cell2.Text = obj.ToString();
						}
					}
					try
					{
						_grid.BodySelect(bodyRow, bodyCol, bodyRow + num - 1, bodyCol + num2 - 1);
					}
					catch (Exception exception)
					{
						exception.Log("表单标题/表底区设计界面粘贴单元格时发生了未预期的异常");
					}
				}
			}
			catch (TableModelException ex2)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
			}
		}
		_grid.Invalidate();
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

	public void AutoAdjustGridWidth()
	{
		_grid.BeginUpdate();
		try
		{
			AutoAdjustGridWidthWithOutForceReDrawing();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	protected void AutoAdjustGridWidthWithOutForceReDrawing()
	{
		int gridAbleWidth = GetGridAbleWidth(_owner.GetGridWidthWithoutFixedColumn());
		AdjustGridWidthImpl(gridAbleWidth);
	}

	private int GetGridMaxWidth()
	{
		return _owner.EditorPanel.Width - _owner.GetGridFixedColumnWidth();
	}

	private int GetGridAbleWidth(int gridWantToWidth)
	{
		return Math.Min(_owner.EditorPanel.Width - _owner.GetGridFixedColumnWidth(), gridWantToWidth);
	}

	public void AutoAdjustGridHeight()
	{
		_grid.Height = Math.Min(GetAllRowsTotalHeight(), _owner.GetEditorPanelSize().Height);
	}

	private void SplitValueWithByPercent(int newTotalValue, int[] valueArr, int startIndex, int endIndex)
	{
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

	protected void SetColToWidthWithoutForceReDrawing(int fixedColStartIndex, int fixedColEndIndex)
	{
		int gridAbleWidth = GetGridAbleWidth(_owner.GetGridWidthWithoutFixedColumn());
		AdjustGridWidthImpl(gridAbleWidth, fixedColStartIndex, fixedColEndIndex);
	}

	private void AdjustGridWidthImpl(int gridWillToWidth, int fixedColStartIndex, int fixedColEndIndex)
	{
		int count = _vm.Columns.Count;
		int[] array = new int[count];
		int num = 0;
		for (int i = 0; i < fixedColStartIndex; i++)
		{
			int width = _vm.Columns[i].Width;
			num += width;
			array[i] = width;
		}
		int num2 = 0;
		for (int j = fixedColStartIndex; j <= fixedColEndIndex; j++)
		{
			int width2 = _vm.Columns[j].Width;
			num2 += width2;
			array[j] = width2;
		}
		int num3 = 0;
		for (int k = fixedColEndIndex + 1; k < count; k++)
		{
			num3 += (array[k] = _vm.Columns[k].Width);
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
			if (_vm != null)
			{
				_vm.Columns[l].Width = array[l];
			}
		}
		_grid.Width = gridWillToWidth;
	}

	private void AdjustGridWidthImpl(int willToWidth)
	{
		int count = _grid.Cols.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			int widthDisplay = _grid.Cols[i].WidthDisplay;
			array[i] = widthDisplay;
		}
		SplitValueWithByPercent(willToWidth, array, 0, count - 1);
		for (int j = 0; j < count; j++)
		{
			_grid.Cols[j].Width = array[j];
			if (_vm != null)
			{
				_vm.Columns[j].Width = array[j];
			}
		}
		_grid.Width = willToWidth;
	}

	private void _grid_DragLeave(object sender, EventArgs e)
	{
		_isDragging = false;
	}

	private void _grid_DragEnter(object sender, DragEventArgs e)
	{
		_isDragging = true;
		if (_editor_type == EditorType.Title)
		{
			_owner.EnterTitleDrag();
		}
		else
		{
			_owner.EnterFooterDrag();
		}
	}

	private void _grid_DragOver(object sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.Copy;
	}

	private void _grid_DragDrop(object sender, DragEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			string text = _table.Columns[_owner.DragingRow].GetUniqueFormulaName();
			if (Control.ModifierKeys != Keys.Control)
			{
				text = "[" + text + "]";
			}
			if (_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column) && !_grid.Selection.IsSingleCell)
			{
				for (int i = _grid.Selection.TopRow; i <= _grid.Selection.BottomRow; i++)
				{
					C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(i, hitTestInfo.Column);
					ReplaceCol(_vm.GetCell(mergedRange.TopRow - _grid.Rows.Fixed, mergedRange.LeftCol - _grid.Cols.Fixed), text);
				}
			}
			else
			{
				C1.Win.C1FlexGrid.CellRange mergedRange2 = _grid.GetMergedRange(hitTestInfo.Row, hitTestInfo.Column);
				ReplaceCol(_vm.GetCell(mergedRange2.TopRow - _grid.Rows.Fixed, mergedRange2.LeftCol - _grid.Cols.Fixed), text);
			}
			_grid.Invalidate();
		}
		_isDragging = false;
		static void ReplaceCol(TicketDesignCellVM c, string replace)
		{
			if (_rxCol.IsMatch(c.Text))
			{
				c.Text = _rxCol.Replace(c.Text, replace);
			}
			else
			{
				c.Text += replace;
			}
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
		TicketDesignCellVM cell = _vm.GetCell(e.Row, e.Col);
		FontStyle fontStyle = FontStyle.Regular;
		if (cell.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (cell.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		styleNew.Font = new Font(cell.FontFamily, cell.FontSize, fontStyle);
		styleNew.ForeColor = cell.ForeColor;
		styleNew.TextAlign = C1FlexGridEx.ToTextAlign(cell.Align);
		styleNew.Margins = new Margins(cell.Indent, 0, 0, 0);
		if (cell.HasFormula())
		{
			styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
			try
			{
				FormulaReferenceModelResolver formulaReferenceModelResolver = new FormulaReferenceModelResolver(Leqisoft.Model.Project.Current);
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
				e.Text = cell.Text;
				return;
			}
			catch (FormulaException)
			{
				e.Text = "[公式出错]";
				return;
			}
		}
		e.Text = cell.Text;
		styleNew.BackColor = cell.BackColor;
	}

	private void _grid_BodyAfterEdit(object sender, RowColEventArgs e)
	{
		TicketDesignCellVM cell = _vm.GetCell(e.Row, e.Col);
		cell.Text = ((string)_grid.BodyGetData(e.Row, e.Col)) ?? "";
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			_ctxCell.ShowContextMenu(_grid, e.Location);
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		try
		{
			if (e.Button == MouseButtons.Left && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
			{
				TicketDesignCellVM cell = _vm.GetCell(_grid.BodyRow, _grid.BodyCol);
				if (cell?.HasFormula() == true)
				{
					_grid.DoDragDrop(cell.Text, DragDropEffects.Copy);
				}
			}
		}
		catch { }
	}

	private void _grid_BodyBeforeEdit(object sender, RowColEventArgs e)
	{
		TicketDesignCellVM cell = _vm.GetCell(e.Row, e.Col);
		_grid.BodySetData(e.Row, e.Col, cell.Text);
		e.Cancel = cell.HasFormula();
	}

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyData)
		{
		case Keys.Back:
		case Keys.Delete:
		{
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
				{
					TicketDesignCellVM cell = _vm.GetCell(i, j);
					cell.Text = "";
				}
			}
			_grid.Invalidate();
			break;
		}
		case Keys.X | Keys.Control:
			Cut();
			break;
		case Keys.C | Keys.Control:
			Copy();
			break;
		case Keys.V | Keys.Control:
			Paste();
			break;
		case Keys.A | Keys.Control:
			_grid.BodySelect(0, 0, _grid.BodyRowsCount - 1, _grid.BodyColsCount - 1);
			break;
		}
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		PaintGridOuterBorder();
		Paint_Border();
		Paint_Drag();
		void PaintGridOuterBorder()
		{
			using Pen pen5 = new Pen(_owner.NormalCellBorderColor, 1f);
			e.Graphics.DrawLine(pen5, 0, 0, 0, _grid.ClientSize.Height);
			e.Graphics.DrawLine(pen5, 0, 0, _grid.ClientSize.Width, 0);
		}
		void Paint_Border()
		{
			if (_owner.TicketVM != null)
			{
				int rowsCount = _vm.GetRowsCount();
				int columnsCount = _vm.GetColumnsCount();
				for (int i = 0; i < rowsCount; i++)
				{
					int j;
					for (j = 0; j < columnsCount; j++)
					{
						TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(i, j));
						TicketDesignCellVM cell = _vm.GetCell(i, j);
						Rectangle cellRect = _grid.GetCellRect(i + _grid.Rows.Fixed, j + _grid.Cols.Fixed);
						cellRect.Offset(-1, -1);
						if (cell.Top.Width > 0 && (ticketMerge == null || i == ticketMerge.TopRow))
						{
							Rectangle rectangle2 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (i == 0)
							{
								int num = cell.Top.Width / 2;
								while (rectangle2.Y - num < 0)
								{
									rectangle2.Y++;
									rectangle2.Height--;
								}
							}
							using Pen pen = new Pen(_colorBorder, cell.Top.Width);
							e.Graphics.DrawLine(pen, rectangle2.Left, rectangle2.Top, rectangle2.Right, rectangle2.Top);
						}
						if (cell.Right.Width > 0 && (ticketMerge == null || j == ticketMerge.RightColumn))
						{
							Rectangle rectangle3 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (i == rowsCount - 1)
							{
								rectangle3.Height++;
							}
							using Pen pen2 = new Pen(_colorBorder, cell.Right.Width);
							e.Graphics.DrawLine(pen2, rectangle3.Right, rectangle3.Top, rectangle3.Right, rectangle3.Bottom);
						}
						if (cell.Bottom.Width > 0 && (ticketMerge == null || i == ticketMerge.BottomRow))
						{
							Rectangle rectangle4 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (i == rowsCount - 1)
							{
								int height = _grid.ClientSize.Height;
								cellRect.Height += cell.Bottom.Width + 1;
								int num2 = cell.Bottom.Width / 2;
								while (rectangle4.Y + rectangle4.Height + num2 > height)
								{
									rectangle4.Height--;
								}
							}
							using Pen pen3 = new Pen(_colorBorder, cell.Bottom.Width);
							e.Graphics.DrawLine(pen3, rectangle4.Left, rectangle4.Bottom, rectangle4.Right, rectangle4.Bottom);
						}
						if (cell.Left.Width > 0 && (ticketMerge == null || j == ticketMerge.LeftColumn))
						{
							Rectangle rectangle5 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (j == 0)
							{
								int num3 = cell.Left.Width / 2;
								while (rectangle5.X - num3 < 0)
								{
									rectangle5.X++;
									rectangle5.Width--;
								}
							}
							using Pen pen4 = new Pen(_colorBorder, cell.Left.Width);
							e.Graphics.DrawLine(pen4, rectangle5.Left, rectangle5.Top, rectangle5.Left, rectangle5.Bottom);
						}
					}
				}
			}
		}
		void Paint_Drag()
		{
			if (_isDragging)
			{
				HitTestInfo hitTestInfo = _grid.HitTest();
				if (hitTestInfo.Type == HitTestTypeEnum.Cell)
				{
					C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
					Rectangle rectangle = ((!selection.Contains(hitTestInfo.Row, hitTestInfo.Column) || selection.IsSingleCell) ? _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column) : _grid.GetCellRangeRectUnclipped(selection.TopRow, selection.LeftCol, selection.BottomRow, selection.RightCol));
					if (rectangle.X == 0)
					{
						rectangle.X += _dragRectTopLeftEdgeOffset;
						rectangle.Width -= _dragRectTopLeftEdgeOffset;
					}
					if (rectangle.Y == 0)
					{
						rectangle.Y += _dragRectTopLeftEdgeOffset;
						rectangle.Height -= _dragRectTopLeftEdgeOffset;
					}
					if (rectangle.X + rectangle.Width == _grid.Width)
					{
						rectangle.Width -= _dragRectBottomRightEdgeOffset;
					}
					if (rectangle.Y + rectangle.Height == _grid.Height)
					{
						rectangle.Height -= _dragRectBottomRightEdgeOffset;
					}
					e.Graphics.DrawRectangle(_pen, rectangle);
					e.Graphics.FillRectangle(_brush, rectangle);
					string text = _table.Columns[_owner.DragingRow].GetUniqueFormulaName();
					if (Control.ModifierKeys != Keys.Control)
					{
						text = "[" + text + "]";
					}
					e.Graphics.DrawString(text, _grid.Font, _brushText, rectangle, _sf);
				}
			}
		}
	}

	public bool IsLastRowExistBottomBorder()
	{
		int rowIndex = _vm.GetRowsCount() - 1;
		if (rowIndex < 0)
		{
			return false;
		}
		int columnsCount = _vm.GetColumnsCount();
		int i;
		for (i = 0; i < columnsCount; i++)
		{
			TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(rowIndex, i));
			int row = rowIndex;
			int col = i;
			if (ticketMerge != null)
			{
				row = ticketMerge.TopRow;
				col = ticketMerge.LeftColumn;
			}
			TicketDesignCellVM cell = _vm.GetCell(row, col);
			if (cell.Bottom.Width > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFirstRowExistTopBorder()
	{
		if (_vm.GetRowsCount() == 0)
		{
			return false;
		}
		int columnsCount = _vm.GetColumnsCount();
		int i;
		for (i = 0; i < columnsCount; i++)
		{
			TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(0, i));
			int row = 0;
			int col = i;
			if (ticketMerge != null)
			{
				row = ticketMerge.TopRow;
				col = ticketMerge.LeftColumn;
			}
			TicketDesignCellVM cell = _vm.GetCell(row, col);
			if (cell.Top.Width > 0)
			{
				return true;
			}
		}
		return false;
	}

	private void _grid_MouseUp(object sender, MouseEventArgs e)
	{
		FormulaEditor formulaEditor = Program.MainForm.FormulaEditor;
		if (formulaEditor.IsEditing)
		{
			if (_grid.HitTest().Type == HitTestTypeEnum.Cell)
			{
				C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
				string formulaContextCellLabel = GetFormulaContextCellLabel(bodySelection.TopRow + 1, bodySelection.LeftCol + 1);
				formulaEditor.RemoveRefAtPos();
				formulaEditor.InsertRefText(formulaContextCellLabel);
			}
			formulaEditor.SetFocus();
		}
	}

	private void _grid_BodyAfterRowColChange(object sender, RangeEventArgs e)
	{
		if (!_skipGridSelChange)
		{
			SetCommandState();
		}
	}

	private void _cmdAppendRows_Click(object sender, ClickEventArgs e)
	{
		AppendRows();
	}

	private void Cmd_Click_AppendColumn(object sender, ClickEventArgs e)
	{
		AppendColumn();
	}

	private void Cmd_CommandStateQuery_SplitCells(object sender, CommandStateQueryEventArgs e)
	{
		if (!IsExistSelectedCell())
		{
			e.Visible = false;
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				if (_grid.MergedRanges.IndexOf(i, j) != -1)
				{
					e.Visible = true;
					return;
				}
			}
		}
		e.Visible = false;
	}

	private void Cmd_Click_SplitCells(object sender, ClickEventArgs e)
	{
		SplitCells();
	}

	private void Cmd_CommandStateQuery_MergeCells(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void Cmd_Click_MergeCells(object sender, ClickEventArgs e)
	{
		MergeCells();
	}

	private void Cmd_Click_OnlyMergeHorizontalCells(object sender, ClickEventArgs e)
	{
		OnlyMergeHorizontalCells();
	}

	private void Cmd_CommandStateQuery_OnlyMergeHorizontalCells(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdRemoveRows_Click(object sender, ClickEventArgs e)
	{
		RemoveRows();
	}

	private void _cmdInsertRows_Click(object sender, ClickEventArgs e)
	{
		InsertRows();
	}

	private void Cmd_CommandStateQuery_RemoveColumns(object sender, CommandStateQueryEventArgs e)
	{
		if (!IsExistSelectedCell())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = _grid.Cols.Count > 1;
		}
	}

	private void Cmd_Click_RemoveColumns(object sender, ClickEventArgs e)
	{
		RemoveColumns();
	}

	private void Cmd_Click_InsertColumn(object sender, ClickEventArgs e)
	{
		InsertColumn();
	}

	private void Cmd_CommandStateQuery_Field(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void Cmd_Popup_Field(object sender, EventArgs e)
	{
		_cmdField.CommandLinks.Clear();
		foreach (Leqisoft.Model.Column column in _table.Columns)
		{
			C1Command c1Command = new C1Command
			{
				Text = column.GetUniqueFormulaName()
			};
			c1Command.Click += delegate
			{
				C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
				if (_grid.MergedRanges.Contains(_grid.Selection))
				{
					TicketDesignCellVM cell = _vm.GetCell(bodySelection.TopRow, _grid.BodyCol);
					cell.Text = "[" + column.GetUniqueFormulaName() + "]";
					for (int i = bodySelection.TopRow + 1; i <= bodySelection.BottomRow; i++)
					{
						cell = _vm.GetCell(i, _grid.BodyCol);
						cell.Text = "";
					}
				}
				else
				{
					for (int j = bodySelection.TopRow; j <= bodySelection.BottomRow; j++)
					{
						TicketDesignCellVM cell2 = _vm.GetCell(j, _grid.BodyCol);
						cell2.Text = "[" + column.GetUniqueFormulaName() + "]";
					}
				}
				_grid.Invalidate();
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			_cmdField.CommandLinks.Add(value);
		}
		if (_cmdField.CommandLinks.Count == 0)
		{
			_cmdField.CommandLinks.Add(new C1CommandLink());
		}
	}

	private void Cmd_CommandStateQuery_DeleteFormula(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void Cmd_Click_DeleteFormula(object sender, EventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketDesignCellVM cell = _vm.GetCell(i, j);
				cell.UpdateFormula(string.Empty);
			}
		}
		_grid.Invalidate();
		Program.MainForm.FormulaEditor.SetFormulaText(string.Empty);
	}

	private bool IsExistSelectedCell()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return false;
		}
		return true;
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

	private void _cmdSave_Click(object sender, ClickEventArgs e)
	{
		try
		{
			_owner?.AutoAdjustDataGridPosition();
		}
		catch { }
	}

	private void _cmdHelp_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ShowHelpCenter();
	}

	private void _cmdSetColumnWidth_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		_cmdSetColumnWidth.Visible = _grid.BodyCol >= 0;
	}

	private void Cmd_Click_SetColumnWidth(object sender, ClickEventArgs e)
	{
		SetColumnWidth();
	}

	private void _cmdSetRowHeight_Click(object sender, ClickEventArgs e)
	{
		SetRowHeight();
	}

	private void _cmdCancelDesign_Click(object sender, ClickEventArgs e)
	{
		try
		{
			_skipGridSelChange = true;
			_grid.Select(-1, -1);
			_skipGridSelChange = false;
		}
		catch { }
	}

	private void _cmdFormatText_Click(object sender, ClickEventArgs e)
	{
		SetFormatText();
	}

	private void PopulateMerge()
	{
		_grid.MergedRanges.Clear();
		foreach (TicketMerge merge in _vm.Merges)
		{
			_grid.BodyAddMergedRange(merge.TopRow, merge.LeftColumn, merge.BottomRow, merge.RightColumn);
		}
	}

	private void SetCommandState()
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			TicketDesignCellVM cell = _vm.GetCell(_grid.BodyRow, _grid.BodyCol);
			AppCommands.TicketFont.FontSelector.SelectFontFamily(cell.FontFamily);
			AppCommands.TicketFontSize.FontSizeSelector.SelectFontSize(cell.FontSize);
			AppCommands.TicketBold.IsPressed = cell.Bold;
			AppCommands.TicketItalic.IsPressed = cell.Italic;
			SetFormulaContext();
		}
		void SetFormulaContext()
		{
			FormulaEditor formulaEditor = Program.MainForm.FormulaEditor;
			if (!formulaEditor.IsEditing)
			{
				TicketDesignCellVM cell2 = _vm.GetCell(_grid.BodyRow, _grid.BodyCol);
				formulaEditor.Context.Kind = FormulaContextKind.TicketDesign;
				formulaEditor.Context.Table = _owner.Table;
				formulaEditor.Context.TicketCell = cell2;
				formulaEditor.Context.DataRowStart = 0;
				formulaEditor.Context.DataRowCount = _grid.Rows.Count;
				formulaEditor.Populate();
			}
		}
	}
}
