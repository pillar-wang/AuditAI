using System;
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
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class TicketDesignEditor2
{
	private class GridResizeManager : GridResizingManager
	{
		public GridResizeManager(C1FlexGridEx grid)
			: base(grid)
		{
		}

		protected override int GetResizingColumn(MouseEventArgs e)
		{
			int resizingColumn = base.GetResizingColumn(e);
			if (resizingColumn == 0)
			{
				return -1;
			}
			return resizingColumn;
		}

		protected override int GetResizingRow(MouseEventArgs e)
		{
			return base.GetResizingRow(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_grid.HitTest().Type != HitTestTypeEnum.RowHeader)
			{
				_grid.Cursor = Cursors.Default;
			}
			else
			{
				_grid.Cursor = TableEditor.CursorRowHeader;
			}
		}
	}

	private C1SplitContainer _windowContainer;

	private C1SplitterPanel _editorPanel;

	private C1SplitterPanel _leftSidePanel;

	private C1FlexGridEx _gridColumns;

	private C1ToolBar _rightToolBar;

	private C1Command _cmdCopy;

	private C1CommandLink _lnkCopy;

	private C1Command _cmdCut;

	private C1CommandLink _lnkCut;

	private C1Command _cmdPaste;

	private C1CommandLink _lnkPaste;

	private C1CommandMenu _cmdAlign;

	private C1CommandLink _lnkAlign;

	private C1CommandLink _lnkAlignColumn;

	private C1CommandLink _lnkAlignRow;

	private C1CommandMenu _cmdField;

	private C1CommandLink _lnkField;

	private C1Command _cmdDeleteFormula;

	private C1CommandLink _lnkDeleteFormula;

	private C1Command _cmdMergeCells;

	private C1CommandLink _lnkMergeCells;

	private C1Command _cmdMergeEveryHorizontalCells;

	private C1CommandLink _lnkMergeEveryHorizontalCells;

	private C1Command _cmdSplitCells;

	private C1CommandLink _lnkSplitCells;

	private C1Command _cmdInsertColumns;

	private C1CommandLink _lnkInsertColumns;

	private C1Command _cmdAppendColumns;

	private C1CommandLink _lnkAppendColumns;

	private C1CommandLink _lnkAppendColumnsEmpty;

	private C1Command _cmdRemoveColumns;

	private C1CommandLink _lnkRemoveColumns;

	private C1Command _cmdSetColumnWidth;

	private C1CommandLink _lnkSetColumnWidth;

	private C1Command _cmdInsertRows;

	private C1CommandLink _lnkInsertRows;

	private C1Command _cmdAppendRows;

	private C1CommandLink _lnkAppendRows;

	private C1CommandLink _lnkAppendRowsEmpty;

	private C1Command _cmdRemoveRows;

	private C1CommandLink _lnkRemoveRows;

	private C1Command _cmdSetRowHeight;

	private C1CommandLink _lnkSetRowHeight;

	private C1Command _cmdHideColumn;

	private C1CommandLink _lnkHideColumn;

	private C1CommandMenu _cmdFormat;

	private C1CommandLink _lnkFormat;

	private C1Command _cmdFormatText;

	private C1CommandLink _lnkFormatText;

	private C1CommandMenu _cmdFormatNumeric;

	private C1CommandLink _lnkFormatNumeric;

	private C1CommandMenu _cmdFormatDate;

	private C1CommandLink _lnkFormatDate;

	private C1CommandMenu _cmdFormatTime;

	private C1CommandLink _lnkFormatTime;

	private C1CommandMenu _cmdFormatBool;

	private C1CommandLink _lnkFormatBool;

	private C1Command _cmdFrozenRow;

	private C1Command _cmdUnFrozenRow;

	private C1Command _cmdFrozenCol;

	private C1Command _cmdUnFrozenCol;

	private C1Command _cmdFlagToColumnHeaderRow;

	private C1Command _cmdCancelColumnHeaderRow;

	private C1ContextMenu _ctxCell;

	private C1ContextMenu _ctxColumn;

	private C1ContextMenu _ctxRow;

	private C1ContextMenu _ctxEmpty;

	private C1FlexGridEx _grid;

	private C1FlexGridEx _titleGrid;

	private C1FlexGridEx _footerGrid;

	private TicketDesignTableVM _vm;

	private TicketDesignTitleFooterEditor2 _titleEditor;

	private TicketDesignTitleFooterEditor2 _footerEditor;

	private GridResizingManager _gridResizingManager;

	private int _dragRow = -1;

	private bool _isDragging;

	private bool _skipGridSelChange;

	private Color _colorBorder;

	private int _columnNameStringHeight;

	private SolidBrush _panelBackgroundBrush;

	private SolidBrush _columnHeaderRowBackgroundBrush;

	private static readonly int _dragRectLeftEdgeOffset = 1;

	private static readonly int _dragRectTopEdgeOffsetExistTitle = 2;

	private static readonly int _dragRectTopEdgeOffsetUnExistTitle = 1;

	private static readonly int _dragRectRightEdgeOffset = 2;

	private static readonly int _dragRectBottomEdgeOffsetExistFooter = 3;

	private static readonly int _dragRectBottomEdgeOffsetUnExistFooter = 2;

	private static readonly Pen _pen = new Pen(Color.Red, 3f);

	private static readonly HatchBrush _brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Red, Color.Transparent);

	private static readonly SolidBrush _brushText = new SolidBrush(Color.Black);

	private static readonly Font _columnNameFont = new Font("微软雅黑", 7f);

	private static readonly SolidBrush _columnNameBrush = new SolidBrush(Color.Gray);

	private static readonly Font _rowNumberFont = new Font("微软雅黑", 9f);

	private static readonly SolidBrush _rowNumberBrush = new SolidBrush(Color.Gray);

	private static readonly StringFormat _sf = new StringFormat
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	private static readonly StringFormat _columnNameStringFormat = new StringFormat
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	private static readonly StringFormat _rowNumberStringFormat = new StringFormat
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	private RibbonImageProcess _imageProcess;

	private static readonly Regex _rxCol = new Regex("\\[.*]");

	public const int TABLE_MIN_WIDTH = 50;

	public Color NormalCellBorderColor = Color.FromArgb(255, 240, 240, 240);

	private bool _isMouseDownFromCell;

	public Leqisoft.Model.Table Table { get; set; }

	private TicketTable Ticket => Table.Ticket;

	public C1SplitContainer View => _windowContainer;

	public C1FlexGridEx GridTarget => _grid;

	public C1SplitterPanel EditorPanel => _editorPanel;

	public TicketDesignTableVM TicketVM => _vm;

	public int DragingRow => _dragRow;

	public TicketDesignTitleFooterEditor2 TitleEditor => _titleEditor;

	public TicketDesignTitleFooterEditor2 FooterEditor => _footerEditor;

	public TicketDesignEditor2()
	{
		_columnNameStringHeight = MeasureTextHeight("A", _columnNameFont, int.MaxValue);
		InitLayoutPanel();
		InitTitleEditor();
		InitFooterEditor();
		InitLeftSidePanel();
		InitRightToolBar();
		InitGridTable();
		InitContextMenu();
	}

	private void InitLayoutPanel()
	{
		_windowContainer = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_leftSidePanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			KeepRelativeSize = false,
			SizeRatio = 20.0,
			Collapsible = false,
			Resizable = true,
			MinWidth = 0
		};
		_windowContainer.Panels.Add(_leftSidePanel);
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Right,
			KeepRelativeSize = false,
			Width = 80,
			Collapsible = false,
			Resizable = false
		};
		_rightToolBar = new C1ToolBar
		{
			Name = "TicketDesignEditor2+_rightToolBar",
			Dock = DockStyle.Fill,
			Horizontal = false,
			MinButtonSize = 40,
			ButtonLookVert = ButtonLookFlags.TextAndImage
		};
		c1SplitterPanel.Controls.Add(_rightToolBar);
		_windowContainer.Panels.Add(c1SplitterPanel);
		_editorPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			KeepRelativeSize = false
		};
		_windowContainer.Panels.Add(_editorPanel);
		_editorPanel.Resize += Panel_Resize_EditorPanel;
		_editorPanel.MouseDown += Panel_MouseDown_EditorPanel;
	}

	private void InitGridTable()
	{
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.None,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowMergingFixed = AllowMergingEnum.Custom,
			AllowResizing = AllowResizingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			DragMode = DragModeEnum.Manual,
			DropMode = DropModeEnum.Manual,
			Cursor = TableEditor.CursorTable,
			FocusRect = FocusRectEnum.None,
			VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom
		};
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 1;
		_grid.Rows.DefaultSize = 30;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.BodyBeforeEdit += _grid_BodyBeforeEdit;
		_grid.BodyAfterEdit += _grid_BodyAfterEdit;
		_grid.BodyAfterRowColChange += _grid_BodyAfterRowColChange;
		_grid.MouseDown += _grid_MouseDown;
		_grid.MouseUp += _grid_MouseUp;
		_grid.MouseMove += _grid_MouseMove;
		_grid.MouseClick += _grid_MouseClick;
		_grid.KeyDown += _grid_KeyDown;
		_grid.Paint += _grid_Paint;
		_grid.DragOver += _grid_DragOver;
		_grid.DragDrop += _grid_DragDrop;
		_grid.DragEnter += _grid_DragEnter;
		_grid.DragLeave += _grid_DragLeave;
		_grid.Enter += _grid_Enter;
		_editorPanel.Controls.Add(_grid);
		_grid.BringToFront();
		_gridResizingManager = new GridResizeManager(_grid);
		_gridResizingManager.ResizeColumn += _gridResizingManager_ResizeColumn;
		_gridResizingManager.ResizeRow += _gridResizingManager_ResizeRow;
	}

	private void _grid_Enter(object sender, EventArgs e)
	{
		EnterEdit();
	}

	private void _gridResizingManager_ResizeRow(object sender, ResizeEventArgs e)
	{
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			int height = Math.Max(1, e.HeightWidth);
			if (!selection.ContainsRow(e.RowCol))
			{
				_vm.Rows[e.RowCol - _grid.Rows.Fixed].Height = height;
				_grid.Rows[e.RowCol].Height = height;
			}
			else
			{
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					_vm.Rows[i - _grid.Rows.Fixed].Height = height;
					_grid.Rows[i].Height = height;
				}
			}
			AdjustGridPositionImpl();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	private void _gridResizingManager_ResizeColumn(object sender, ResizeEventArgs e)
	{
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			int width = Math.Max(1, e.HeightWidth);
			if (!selection.ContainsCol(e.RowCol))
			{
				_vm.Columns[e.RowCol - _grid.Cols.Fixed].Width = width;
				_grid.Cols[e.RowCol].Width = width;
			}
			else
			{
				for (int i = selection.LeftCol; i <= selection.RightCol; i++)
				{
					_vm.Columns[i - _grid.Cols.Fixed].Width = width;
					_grid.Cols[i].Width = width;
				}
			}
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	private void InitTitleEditor()
	{
		_titleEditor = new TicketDesignTitleFooterEditor2(this, TicketDesignTitleFooterEditor2.EditorType.Title);
		_editorPanel.Controls.Add(_titleEditor.View);
		_titleGrid = _titleEditor.View;
		_titleEditor.CancelSelect();
	}

	private void InitFooterEditor()
	{
		_footerEditor = new TicketDesignTitleFooterEditor2(this, TicketDesignTitleFooterEditor2.EditorType.Footer);
		_editorPanel.Controls.Add(_footerEditor.View);
		_footerGrid = _footerEditor.View;
		_footerEditor.CancelSelect();
	}

	private void InitContextMenu()
	{
		_ctxCell = new C1ContextMenu();
		_ctxColumn = new C1ContextMenu();
		_ctxRow = new C1ContextMenu();
		_ctxEmpty = new C1ContextMenu();
		_cmdCopy = new C1Command
		{
			Text = "复制",
			Image = ContextResources.ctxCopy
		};
		_cmdCopy.Click += _cmdCopy_Click;
		_cmdCopy.CommandStateQuery += _cmdCopy_CommandStateQuery;
		_cmdCut = new C1Command
		{
			Text = "剪切",
			Image = ContextResources.ctxCut
		};
		_cmdCut.Click += _cmdCut_Click;
		_cmdCut.CommandStateQuery += _cmdCut_CommandStateQuery;
		_cmdPaste = new C1Command
		{
			Text = "粘贴",
			Image = ContextResources.ctxPaste
		};
		_cmdPaste.Click += _cmdPaste_Click;
		_cmdPaste.CommandStateQuery += _cmdPaste_CommandStateQuery;
		_cmdField = new C1CommandMenu
		{
			Text = "对应表格列",
			Image = Resources.TicketSetField
		};
		_cmdField.CommandStateQuery += _cmdField_CommandStateQuery;
		_cmdField.CommandLinks.Add(new C1CommandLink());
		_cmdField.Popup += _cmdField_Popup;
		_cmdDeleteFormula = new C1Command
		{
			Text = "删除单元格公式",
			Image = ContextResources.ctxDeleteFormula
		};
		_cmdDeleteFormula.CommandStateQuery += _cmdDeleteFormula_CommandStateQuery;
		_cmdDeleteFormula.Click += _cmdDeleteFormula_Click;
		_cmdInsertColumns = new C1Command
		{
			Text = "插入列...",
			Image = ContextResources.ctxInsertColumn
		};
		_cmdInsertColumns.CommandStateQuery += _cmdInsertColumns_CommandStateQuery;
		_cmdInsertColumns.Click += _cmdInsertColumns_Click;
		_cmdAppendColumns = new C1Command
		{
			Text = "追加列...",
			Image = ContextResources.ctxAppendColumn
		};
		_cmdAppendColumns.Click += _cmdAppendColumns_Click;
		_cmdRemoveColumns = new C1Command
		{
			Text = "删除列",
			Image = ContextResources.ctxDeleteColumn
		};
		_cmdRemoveColumns.CommandStateQuery += _cmdRemoveColumns_CommandStateQuery;
		_cmdRemoveColumns.Click += _cmdRemoveColumns_Click;
		_cmdInsertRows = new C1Command
		{
			Text = "插入行...",
			Image = ContextResources.ctxInsertRow
		};
		_cmdInsertRows.Click += _cmdInsertRows_Click;
		_cmdInsertRows.CommandStateQuery += _cmdInsertRows_CommandStateQuery;
		_cmdAppendRows = new C1Command
		{
			Text = "追加行...",
			Image = ContextResources.ctxAppendRow
		};
		_cmdAppendRows.Click += _cmdAppendRows_Click;
		_cmdRemoveRows = new C1Command
		{
			Text = "删除行",
			Image = ContextResources.ctxDeleteRow
		};
		_cmdRemoveRows.Click += _cmdRemoveRows_Click;
		_cmdRemoveRows.CommandStateQuery += _cmdRemoveRows_CommandStateQuery;
		_cmdMergeCells = new C1Command
		{
			Text = "合并单元格"
		};
		_cmdMergeCells.CommandStateQuery += _cmdMergeCells_CommandStateQuery;
		_cmdMergeCells.Click += _cmdMergeCells_Click;
		_cmdMergeEveryHorizontalCells = new C1Command
		{
			Text = "仅横向合并单元格"
		};
		_cmdMergeEveryHorizontalCells.CommandStateQuery += _cmdMergeEveryHorizontalCells_CommandStateQuery;
		_cmdMergeEveryHorizontalCells.Click += _cmdMergeEveryHorizontalCells_Click;
		_cmdSplitCells = new C1Command
		{
			Text = "拆分单元格"
		};
		_cmdSplitCells.CommandStateQuery += _cmdSplitCells_CommandStateQuery;
		_cmdSplitCells.Click += _cmdSplitCells_Click;
		_cmdAlign = new C1CommandMenu
		{
			Text = "对齐"
		};
		_cmdAlign.CommandStateQuery += _cmdAlign_CommandStateQuery;
		AddAlign("左上对齐", ContextResources.ctxAlignTopLeft, CellTextAlign.TopLeft);
		AddAlign("左中对齐", ContextResources.ctxAlignMiddleLeft, CellTextAlign.MiddleLeft);
		AddAlign("左下对齐", ContextResources.ctxAlignBottomLeft, CellTextAlign.BottomLeft);
		AddAlign("中上对齐", ContextResources.ctxAlignTopCenter, CellTextAlign.TopCenter);
		AddAlign("中中对齐", ContextResources.ctxAlignMiddleCenter, CellTextAlign.MiddleCenter);
		AddAlign("中下对齐", ContextResources.ctxAlignBottomCenter, CellTextAlign.BottomCenter);
		AddAlign("右上对齐", ContextResources.ctxAlignTopRight, CellTextAlign.TopRight);
		AddAlign("右中对齐", ContextResources.ctxAlignMiddleRight, CellTextAlign.MiddleRight);
		AddAlign("右下对齐", ContextResources.ctxAlignBottomRight, CellTextAlign.BottomRight);
		_cmdSetColumnWidth = new C1Command
		{
			Text = "设置列宽..."
		};
		_cmdSetColumnWidth.CommandStateQuery += _cmdSetColumnWidth_CommandStateQuery;
		_cmdSetColumnWidth.Click += _cmdSetColumnWidth_Click;
		_cmdSetRowHeight = new C1Command
		{
			Text = "设置行高..."
		};
		_cmdSetRowHeight.Click += _cmdSetRowHeight_Click;
		_cmdSetRowHeight.CommandStateQuery += _cmdSetRowHeight_CommandStateQuery;
		_cmdHideColumn = new C1Command
		{
			Text = "隐藏列"
		};
		_cmdHideColumn.Click += _cmdHideColumn_Click;
		_cmdHideColumn.CommandStateQuery += _cmdHideColumn_CommandStateQuery;
		_cmdFormat = new C1CommandMenu
		{
			Text = "数据格式"
		};
		_cmdFormat.CommandStateQuery += _cmdFormat_CommandStateQuery;
		_cmdFormatText = new C1Command
		{
			Text = "文本格式"
		};
		_cmdFormatText.Click += _cmdFormatText_Click;
		_cmdFormatNumeric = new C1CommandMenu
		{
			Text = "数值格式"
		};
		AddFormatNumeric("1234.56", DataFormatType.Number);
		AddFormatNumeric("1,234.56", DataFormatType.Comma);
		AddFormatNumeric("$1,234.56", DataFormatType.NumDollar);
		AddFormatNumeric("￥1,234.56", DataFormatType.NumRmb);
		AddFormatNumeric("123,456.78%", DataFormatType.Percentage);
		_cmdFormatDate = new C1CommandMenu
		{
			Text = "日期格式"
		};
		AddFormatDefault(_cmdFormatDate, "2017年12月31日", DataFormatType.DateChinese);
		AddFormatDefault(_cmdFormatDate, "2017-12-31", DataFormatType.DateDash);
		AddFormatDefault(_cmdFormatDate, "2017/12/31", DataFormatType.DateSlash);
		AddFormatDefault(_cmdFormatDate, "2017.12.31", DataFormatType.DateDot);
		AddFormatDefault(_cmdFormatDate, "2017年12月", DataFormatType.DateYearMonthChinese, isAddDelimiter: true);
		AddFormatDefault(_cmdFormatDate, "2017-12", DataFormatType.DateYearMonthDash);
		AddFormatDefault(_cmdFormatDate, "2017/12", DataFormatType.DateYearMonthSlash);
		AddFormatDefault(_cmdFormatDate, "2017.12", DataFormatType.DateYearMonthDot);
		_cmdFormatTime = new C1CommandMenu
		{
			Text = "时间格式"
		};
		AddFormatDefault(_cmdFormatTime, "10时20分30秒", DataFormatType.TimeLongChinese);
		AddFormatDefault(_cmdFormatTime, "10时20分", DataFormatType.TimeShortChinese);
		AddFormatDefault(_cmdFormatTime, "10:20:30", DataFormatType.TimeLong);
		AddFormatDefault(_cmdFormatTime, "10:20", DataFormatType.TimeShort);
		_cmdFormatBool = new C1CommandMenu
		{
			Text = "判断格式"
		};
		AddFormatDefault(_cmdFormatBool, "复选框", DataFormatType.BoolCheckBox);
		AddFormatDefault(_cmdFormatBool, "开关钮", DataFormatType.BoolOnOff);
		_cmdFrozenRow = new C1Command
		{
			Text = "冻结行"
		};
		_cmdFrozenRow.Click += delegate
		{
			_vm.TableRowsFrozenCount = _grid.BodyRowSel + 1;
			_grid.Invalidate();
		};
		_cmdFrozenRow.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_cmdUnFrozenRow = new C1Command
		{
			Text = "解冻行"
		};
		_cmdUnFrozenRow.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _vm.TableRowsFrozenCount > 0 && IsExistSelectedCell();
		};
		_cmdUnFrozenRow.Click += delegate
		{
			_vm.TableRowsFrozenCount = 0;
			_grid.Invalidate();
		};
		_cmdFrozenCol = new C1Command
		{
			Text = "冻结列"
		};
		_cmdFrozenCol.Click += delegate
		{
			_vm.TableColsFrozenCount = _grid.BodyColSel + 1;
			_grid.Invalidate();
		};
		_cmdFrozenCol.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = IsExistSelectedCell();
		};
		_cmdUnFrozenCol = new C1Command
		{
			Text = "解冻列"
		};
		_cmdUnFrozenCol.Click += delegate
		{
			_vm.TableColsFrozenCount = 0;
			_grid.Invalidate();
		};
		_cmdUnFrozenCol.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _vm.TableColsFrozenCount > 0 && IsExistSelectedCell();
		};
		_cmdFlagToColumnHeaderRow = new C1Command
		{
			Text = "标记为列头"
		};
		_cmdFlagToColumnHeaderRow.Click += delegate
		{
			_vm.ColumnHeaderRowsCount = _grid.BodyRowSel + 1;
			if (_vm.ColumnHeaderRowsCount < 0)
			{
				_vm.ColumnHeaderRowsCount = 0;
			}
			_grid.Invalidate();
		};
		_cmdCancelColumnHeaderRow = new C1Command
		{
			Text = "取消列头"
		};
		_cmdCancelColumnHeaderRow.Click += delegate
		{
			_vm.ColumnHeaderRowsCount = 0;
			_grid.Invalidate();
		};
		_cmdCancelColumnHeaderRow.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _vm.ColumnHeaderRowsCount > 0;
		};
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdInsertRows));
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdAppendRows));
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdRemoveRows));
		_lnkInsertColumns = new C1CommandLink(_cmdInsertColumns)
		{
			Delimiter = true
		};
		_ctxCell.CommandLinks.Add(_lnkInsertColumns);
		_lnkAppendColumns = new C1CommandLink(_cmdAppendColumns);
		_ctxCell.CommandLinks.Add(_lnkAppendColumns);
		_lnkRemoveColumns = new C1CommandLink(_cmdRemoveColumns);
		_ctxCell.CommandLinks.Add(_lnkRemoveColumns);
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdSetRowHeight)
		{
			Delimiter = true
		});
		_lnkSetColumnWidth = new C1CommandLink(_cmdSetColumnWidth);
		_ctxCell.CommandLinks.Add(_lnkSetColumnWidth);
		_lnkCopy = new C1CommandLink(_cmdCopy)
		{
			Delimiter = true
		};
		_ctxCell.CommandLinks.Add(_lnkCopy);
		_lnkCut = new C1CommandLink(_cmdCut);
		_ctxCell.CommandLinks.Add(_lnkCut);
		_lnkPaste = new C1CommandLink(_cmdPaste);
		_ctxCell.CommandLinks.Add(_lnkPaste);
		_lnkMergeCells = new C1CommandLink(_cmdMergeCells)
		{
			Delimiter = true
		};
		_ctxCell.CommandLinks.Add(_lnkMergeCells);
		_lnkMergeEveryHorizontalCells = new C1CommandLink(_cmdMergeEveryHorizontalCells);
		_ctxCell.CommandLinks.Add(_lnkMergeEveryHorizontalCells);
		_lnkSplitCells = new C1CommandLink(_cmdSplitCells);
		_ctxCell.CommandLinks.Add(_lnkSplitCells);
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdAlign)
		{
			Delimiter = true
		});
		_lnkFormat = new C1CommandLink(_cmdFormat);
		_lnkFormatText = new C1CommandLink(_cmdFormatText);
		_lnkFormatNumeric = new C1CommandLink(_cmdFormatNumeric);
		_lnkFormatDate = new C1CommandLink(_cmdFormatDate);
		_lnkFormatTime = new C1CommandLink(_cmdFormatTime);
		_lnkFormatBool = new C1CommandLink(_cmdFormatBool);
		_cmdFormat.CommandLinks.Add(_lnkFormatText);
		_cmdFormat.CommandLinks.Add(_lnkFormatNumeric);
		_cmdFormat.CommandLinks.Add(_lnkFormatDate);
		_cmdFormat.CommandLinks.Add(_lnkFormatTime);
		_cmdFormat.CommandLinks.Add(_lnkFormatBool);
		_ctxCell.CommandLinks.Add(_lnkFormat);
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdFlagToColumnHeaderRow)
		{
			Delimiter = true
		});
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdCancelColumnHeaderRow));
		_lnkField = new C1CommandLink(_cmdField)
		{
			Delimiter = true
		};
		_ctxCell.CommandLinks.Add(_lnkField);
		_lnkDeleteFormula = new C1CommandLink(_cmdDeleteFormula)
		{
			Delimiter = true
		};
		_ctxCell.CommandLinks.Add(_lnkDeleteFormula);
		_lnkInsertRows = new C1CommandLink(_cmdInsertRows);
		_ctxRow.CommandLinks.Add(_lnkInsertRows);
		_lnkAppendRows = new C1CommandLink(_cmdAppendRows);
		_ctxRow.CommandLinks.Add(_lnkAppendRows);
		_lnkRemoveRows = new C1CommandLink(_cmdRemoveRows);
		_ctxRow.CommandLinks.Add(_lnkRemoveRows);
		_lnkSetRowHeight = new C1CommandLink(_cmdSetRowHeight)
		{
			Delimiter = true
		};
		_ctxRow.CommandLinks.Add(_lnkSetRowHeight);
		_lnkAlignRow = new C1CommandLink(_cmdAlign)
		{
			Delimiter = true
		};
		_ctxRow.CommandLinks.Add(_lnkAlignRow);
		_ctxRow.CommandLinks.Add(new C1CommandLink(_cmdFlagToColumnHeaderRow)
		{
			Delimiter = true
		});
		_ctxRow.CommandLinks.Add(new C1CommandLink(_cmdCancelColumnHeaderRow));
		_lnkAppendColumnsEmpty = new C1CommandLink(_cmdAppendColumns);
		_ctxEmpty.CommandLinks.Add(_lnkAppendColumnsEmpty);
		_lnkAppendRowsEmpty = new C1CommandLink(_cmdAppendRows);
		_ctxEmpty.CommandLinks.Add(_lnkAppendRowsEmpty);
		C1Command c1Command = new C1Command();
		c1Command.Text = "添加标题区";
		c1Command.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _titleGrid.Rows.Count == 0;
		};
		c1Command.Click += delegate
		{
			_titleEditor.ResetGrid(2, 3);
			_titleEditor.MergeCells(0, 0, 0, 2);
		};
		_ctxEmpty.CommandLinks.Add(new C1CommandLink(c1Command)
		{
			Delimiter = true
		});
		C1Command c1Command2 = new C1Command();
		C1CommandLink lnk = new C1CommandLink(c1Command2);
		c1Command2.Text = "添加表底区";
		c1Command2.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _footerGrid.Rows.Count == 0;
			lnk.Delimiter = _titleGrid.Rows.Count != 0;
		};
		c1Command2.Click += delegate
		{
			_footerEditor.ResetGrid(1, 3);
		};
		_ctxEmpty.CommandLinks.Add(lnk);
		void AddFormatDefault(C1CommandMenu parent, string text, DataFormatType dft, bool isAddDelimiter = false)
		{
			C1Command c1Command3 = new C1Command
			{
				Text = text
			};
			c1Command3.Click += delegate
			{
				SetFormatDefault(dft);
			};
			C1CommandLink value = new C1CommandLink(c1Command3)
			{
				Delimiter = isAddDelimiter
			};
			parent.CommandLinks.Add(value);
		}
		void AddFormatNumeric(string text, DataFormatType dft)
		{
			C1Command c1Command3 = new C1Command
			{
				Text = text
			};
			c1Command3.Click += delegate
			{
				SetFormatNumeric(dft);
			};
			C1CommandLink value = new C1CommandLink(c1Command3);
			_cmdFormatNumeric.CommandLinks.Add(value);
		}
	}

	private void _cmdHideColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (IsExistSelectedCell())
		{
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			if (bodySelection.LeftCol < 0 || bodySelection.LeftCol >= _vm.Columns.Count)
			{
				e.Checked = false;
			}
			else
			{
				e.Checked = _vm.GetColumn(bodySelection.LeftCol).IsHiddenColumn;
			}
		}
	}

	private void _cmdHideColumn_Click(object sender, ClickEventArgs e)
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
		bool isHiddenColumn = true;
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			if (i >= 0 && i < _vm.Columns.Count)
			{
				isHiddenColumn = !_vm.Columns[i].IsHiddenColumn;
				break;
			}
		}
		for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
		{
			_vm.Columns[j].IsHiddenColumn = isHiddenColumn;
		}
	}

	private void _cmdCopy_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdCut_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdPaste_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdFormat_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdSetRowHeight_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdRemoveRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdInsertRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void InitLeftSidePanel()
	{
		_gridColumns = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowEditing = false,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.None,
			AllowMergingFixed = AllowMergingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			ExtendLastCol = true,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			SelectionMode = SelectionModeEnum.Row
		};
		_gridColumns.Cols.Count = 1;
		_gridColumns.Cols.Fixed = 0;
		_gridColumns.Rows.Count = 1;
		_gridColumns.Rows.Fixed = 1;
		_gridColumns.Rows.DefaultSize = 30;
		_gridColumns.Cols[0].Caption = "将列名拖拽至右侧表格处\r\n（按住Ctrl键可去掉方括号）";
		_gridColumns.Rows[0].Height = 50;
		_gridColumns.BodyOwnerDrawCell += _gridColumns_BodyOwnerDrawCell;
		_gridColumns.MouseDown += _gridColumns_MouseDown;
		_gridColumns.GiveFeedback += _gridColumns_GiveFeedback;
		_gridColumns.KeyDown += _gridColumns_KeyDown;
		_gridColumns.KeyUp += _gridColumns_KeyUp;
		_leftSidePanel.Controls.Add(_gridColumns);
	}

	private void InitRightToolBar()
	{
		C1Command c1Command = new C1Command
		{
			Text = "保存设计",
			Image = Resources.SaveRecord
		};
		c1Command.Click += _cmdSave_Click;
		C1Command c1Command2 = new C1Command
		{
			Text = "取消设计",
			Image = Resources.FormulaCancel
		};
		c1Command2.Click += _cmdCancelDesign_Click;
		C1Command c1Command3 = new C1Command
		{
			Text = "帮助中心",
			Image = Resources.HelpCenter,
			Visible = SoftwareLicenseManager.IsShowHelpDocumentButton()
		};
		c1Command3.Click += _cmdHelp_Click;
		_rightToolBar.CommandLinks.Add(new C1CommandLink(c1Command));
		_rightToolBar.CommandLinks.Add(new C1CommandLink(c1Command2));
		_rightToolBar.CommandLinks.Add(new C1CommandLink(c1Command3)
		{
			Delimiter = true
		});
		_imageProcess = new RibbonImageProcess();
		foreach (C1CommandLink commandLink in _rightToolBar.CommandLinks)
		{
			_imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
	}

	private int MeasureTextHeight(string text, Font font, int textAreaWidth, int topPadding = 0, int bottomPadding = 0)
	{
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		Size proposedSize = new Size(textAreaWidth, int.MaxValue);
		return TextRenderer.MeasureText(text, font, proposedSize, TextFormatFlags.HorizontalCenter | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height + topPadding + bottomPadding;
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

	private void VMCellsDoAction(int rowIndex, int colIndex, Action<int, int, TicketDesignCellVM> action)
	{
		TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(rowIndex, colIndex));
		if (ticketMerge != null)
		{
			for (int num = ticketMerge.TopRow; num <= ticketMerge.BottomRow; num++)
			{
				for (int num2 = ticketMerge.LeftColumn; num2 <= ticketMerge.RightColumn; num2++)
				{
					TicketDesignCellVM cell = _vm.GetCell(num, num2);
					action(num, num2, cell);
				}
			}
		}
		else
		{
			TicketDesignCellVM cell2 = _vm.GetCell(rowIndex, colIndex);
			action(rowIndex, colIndex, cell2);
		}
	}

	public void MoveLeftColumn()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		int leftCol = selection.LeftCol;
		int rightCol = selection.RightCol;
		if (leftCol < _grid.Cols.Fixed || leftCol >= _grid.Cols.Count || rightCol < _grid.Cols.Fixed || rightCol >= _grid.Cols.Count)
		{
			return;
		}
		int topRow = selection.TopRow;
		int bottomRow = selection.BottomRow;
		int moveCount = rightCol - leftCol + 1;
		FinishEditing();
		int afterMoveVMColStartIndex;
		int num = _vm.MoveColumnLeft(leftCol - _grid.Cols.Fixed, moveCount, out afterMoveVMColStartIndex);
		if (num <= 0)
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		try
		{
			PopulateImpl();
			_grid.Invalidate();
			_grid.SafeSelect(topRow, afterMoveVMColStartIndex + _grid.Cols.Fixed, bottomRow, afterMoveVMColStartIndex + num - 1 + _grid.Cols.Fixed);
		}
		finally
		{
			_editorPanel.ResumeDrawing();
		}
	}

	public void MoveRightColumn()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		int leftCol = selection.LeftCol;
		int rightCol = selection.RightCol;
		if (leftCol < _grid.Cols.Fixed || leftCol >= _grid.Cols.Count || rightCol < _grid.Cols.Fixed || rightCol >= _grid.Cols.Count)
		{
			return;
		}
		int topRow = selection.TopRow;
		int bottomRow = selection.BottomRow;
		int moveCount = rightCol - leftCol + 1;
		FinishEditing();
		int afterMoveVMColStartIndex;
		int num = _vm.MoveColumnRight(leftCol - _grid.Cols.Fixed, moveCount, out afterMoveVMColStartIndex);
		if (num <= 0)
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		try
		{
			PopulateImpl();
			_grid.Invalidate();
			_grid.SafeSelect(topRow, afterMoveVMColStartIndex + _grid.Cols.Fixed, bottomRow, afterMoveVMColStartIndex + num - 1 + _grid.Cols.Fixed);
		}
		finally
		{
			_editorPanel.ResumeDrawing();
		}
	}

	public void SetFontFamily(string ff)
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetFontFamily(ff);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetFontFamily(ff);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetFontSize(fs);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetFontSize(fs);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetForeColor(c);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetForeColor(c);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetBackColor(c);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetBackColor(c);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetBold(b);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetBold(b);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetItalic(b);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetItalic(b);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetAlign(a);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetAlign(a);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.GrowFont();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.GrowFont();
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.ShrinkFont();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.ShrinkFont();
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderTop();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderTop();
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			VMCellsDoAction(bodySelection.TopRow, i, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Top.Width = 1;
			});
		}
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderBottom()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderBottom();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderBottom();
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			VMCellsDoAction(bodySelection.BottomRow, i, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Bottom.Width = 1;
			});
		}
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderLeft()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderLeft();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderLeft();
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			VMCellsDoAction(i, bodySelection.LeftCol, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Left.Width = 1;
			});
		}
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderRight()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderRight();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderRight();
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			VMCellsDoAction(i, bodySelection.RightCol, delegate(int row, int col, TicketDesignCellVM u)
			{
				u.Right.Width = 1;
			});
		}
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderNone()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderNone();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderNone();
			return;
		}
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
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderAll()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderAll();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderAll();
			return;
		}
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
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderStyle1()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderStyle1();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderStyle1();
			return;
		}
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
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void BorderStyle2()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.BorderStyle2();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.BorderStyle2();
			return;
		}
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
		AutoAdjustDataGridPosition();
		_grid.Invalidate();
	}

	public void SetFormatText()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetFormatText();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetFormatText();
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetFormatNumeric(dft);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetFormatNumeric(dft);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetFormatDefault(dft);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetFormatDefault(dft);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.SetZeroFormat(zf);
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.SetZeroFormat(zf);
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.MorePrecision();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.MorePrecision();
			return;
		}
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
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.LessPrecision();
			return;
		}
		if (_footerEditor.IsInEditing)
		{
			_footerEditor.LessPrecision();
			return;
		}
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

	public void MergeEveryHorizontalCells()
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
		_gridColumns.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_grid.Styles[CellStyleEnum.SelectedColumnHeader].DefinedElements &= ~StyleElementFlags.ForeColor;
		if (Theme.SelectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			_imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			_imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		_imageProcess.ProcessImage();
		_colorBorder = Theme.SelectedLeqiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
		_grid.Styles.Normal.WordWrap = true;
		_panelBackgroundBrush = new SolidBrush(_editorPanel.BackColor);
		_columnHeaderRowBackgroundBrush = null;
		_grid.Styles.Normal.Border.Color = NormalCellBorderColor;
		_grid.Styles.EmptyArea.BackColor = _editorPanel.BackColor;
		_grid.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_titleEditor.SetTheme();
		_footerEditor.SetTheme();
	}

	protected void FinishEditing()
	{
		try
		{
			TitleEditor.FinishEditing();
			FooterEditor.FinishEditing();
			if (_grid.Editor != null)
			{
				try
				{
					if (!_grid.FinishEditing(cancel: false))
					{
						_grid.FinishEditing(cancel: true);
					}
					_grid.Editor = null;
				}
				catch
				{
				}
			}
			_grid.Select();
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	public void Save()
	{
		if (!SoftwareLicenseManager.IsTableRowsAndColsOutOfLicenseLimit(_vm.GetRowsCount(), _vm.GetColumnsCount()) && Program.MainForm.IsAllowToDesignTicket(Table))
		{
			FinishEditing();
			TicketDesignValidation ticketDesignValidation = _vm.Save();
			if (ticketDesignValidation.Success)
			{
				TreeNodeStateCache.Get(Table.Id).Kind = TreeNodeCacheKind.TicketInput;
				Program.MainForm.OpenTable();
			}
		}
	}

	public void CancelSave()
	{
		Program.MainForm.OpenTable();
	}

	public void Populate()
	{
		Program.MainForm.FormulaEditor.View.Enabled = true;
		if (Table != null)
		{
			_vm = new TicketDesignTableVM(Ticket);
			_titleEditor._vm = _vm.Title;
			_footerEditor._vm = _vm.Footer;
			PopulateImpl();
			EnterEdit();
			_grid.Select(0, 1);
		}
		SetTheme();
		SetLevel();
	}

	public void PopulateImpl()
	{
		_grid.BeginUpdate();
		try
		{
			int rowsCount = _vm.GetRowsCount();
			int columnsCount = _vm.GetColumnsCount();
			_skipGridSelChange = true;
			_grid.BodyRowsCount = rowsCount;
			_grid.BodyColsCount = columnsCount;
			_skipGridSelChange = false;
			for (int i = 0; i < rowsCount; i++)
			{
				_grid.BodyGetRow(i).Height = _vm.GetRow(i).Height;
			}
			for (int j = 0; j < columnsCount; j++)
			{
				_grid.BodyGetCol(j).Width = _vm.GetColumn(j).Width;
			}
			PopulateMerge();
			SetCommandState();
			_titleEditor.Populate();
			_footerEditor.Populate();
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
		}
		_gridColumns.BeginUpdate();
		_gridColumns.BodyRowsCount = Table.Columns.Count;
		_gridColumns.EndUpdate();
	}

	public void AppendColumns()
	{
		decimal? num = InputForm.Numeric("追加列", "请输入列数：", 1);
		if (!num.HasValue || SoftwareLicenseManager.IsTableColsOutOfLicenseLimit((int)num.Value + _vm.Columns.Count))
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			_vm.AppendColumns((int)num.Value);
			_grid.Cols.Add((int)num.Value);
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public void InsertColumns()
	{
		decimal? num = InputForm.Numeric("插入列", "请输入列数：", 1);
		if (!num.HasValue || SoftwareLicenseManager.IsTableColsOutOfLicenseLimit((int)num.Value + _vm.Columns.Count))
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			_vm.InsertColumns(_grid.BodyCol, (int)num.Value);
			_grid.Cols.InsertRange(_grid.BodyCol + _grid.Cols.Fixed, (int)num.Value);
			PopulateMerge();
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public void AppendRows()
	{
		decimal? num = InputForm.Numeric("追加行", "请输入行数：", 1);
		if (!num.HasValue || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit((int)num.Value + _vm.Rows.Count))
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			_vm.AppendRows((int)num.Value);
			_grid.Rows.Add((int)num.Value);
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public void InsertRows()
	{
		decimal? num = InputForm.Numeric("插入行", "请输入行数：", 1);
		if (!num.HasValue || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit((int)num.Value + _vm.Rows.Count))
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			_vm.InsertRows(_grid.BodyRow, (int)num.Value);
			_grid.Rows.InsertRange(_grid.BodyRow + _grid.Rows.Fixed, (int)num.Value);
			PopulateMerge();
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public void RemoveColumns()
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			_vm.RemoveColumns(bodySelection.LeftCol, bodySelection.RightCol - bodySelection.LeftCol + 1);
			_grid.Cols.RemoveRange(bodySelection.LeftCol + _grid.Cols.Fixed, bodySelection.RightCol - bodySelection.LeftCol + 1);
			PopulateMerge();
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public void RemoveRows()
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			if (_vm.ColumnHeaderRowsCount > bodySelection.BottomRow)
			{
				_vm.ColumnHeaderRowsCount -= bodySelection.BottomRow - bodySelection.TopRow + 1;
				if (_vm.ColumnHeaderRowsCount < 0)
				{
					_vm.ColumnHeaderRowsCount = 0;
				}
			}
			_vm.RemoveRows(bodySelection.TopRow, bodySelection.BottomRow - bodySelection.TopRow + 1);
			_grid.Rows.RemoveRange(bodySelection.TopRow + _grid.Rows.Fixed, bodySelection.BottomRow - bodySelection.TopRow + 1);
			PopulateMerge();
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
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
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
			{
				_vm.GetColumn(i).Width = num2;
				_grid.Cols[i + _grid.Cols.Fixed].Width = num2;
			}
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public void SetRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		decimal? num = InputForm.Numeric("设置行高", "请输入行高（像素）：", _vm.GetRow(bodySelection.TopRow).Height);
		if (!num.HasValue)
		{
			return;
		}
		int num2 = (int)num.Value;
		if (num2 < 0)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能小于0");
			return;
		}
		if (num2 > 9999)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能大于9999");
			return;
		}
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				_vm.GetRow(i).Height = num2;
				_grid.Rows[i + _grid.Rows.Fixed].Height = num2;
			}
			AdjustGridPositionImpl();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public string GetFormulaContextCellLabel()
	{
		if (_titleEditor.IsInEditing)
		{
			return _titleEditor.GetFormulaContextCellLabel();
		}
		if (_footerEditor.IsInEditing)
		{
			return _footerEditor.GetFormulaContextCellLabel();
		}
		return $"[{Leqisoft.Model.Column.GetExcelColumnName(_grid.BodyCol)},{_grid.BodyRow + 1}]";
	}

	public void OnFormulaEditorBeganEditing()
	{
		int editingFlagOnTicketDesign = 1;
		if (_titleEditor.IsInEditing)
		{
			editingFlagOnTicketDesign = 2;
		}
		else if (_footerEditor.IsInEditing)
		{
			editingFlagOnTicketDesign = 3;
		}
		Program.MainForm.FormulaEditor.Context.EditingFlagOnTicketDesign = editingFlagOnTicketDesign;
		Program.MainForm.SwitchStateTo(MainFormView.TicketFormula);
		_gridColumns.Enabled = false;
		_rightToolBar.Enabled = false;
	}

	public void OnFormulaEditorFinishedEditing()
	{
		switch (Program.MainForm.FormulaEditor.Context.EditingFlagOnTicketDesign)
		{
		case 2:
			_titleEditor.View.Select();
			break;
		case 3:
			_footerEditor.View.Select();
			break;
		default:
			_grid.Select();
			break;
		}
		_grid.Invalidate();
		_titleEditor.View.Invalidate();
		_footerEditor.View.Invalidate();
		Program.MainForm.SwitchStateTo(MainFormView.TicketDesign);
		_gridColumns.Enabled = true;
		_rightToolBar.Enabled = true;
	}

	public void Indent()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.Indent();
		}
		else if (_footerEditor.IsInEditing)
		{
			_footerEditor.Indent();
		}
		else
		{
			if (!IsExistSelectedCell())
			{
				return;
			}
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
	}

	public void Unindent()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.Unindent();
		}
		else if (_footerEditor.IsInEditing)
		{
			_footerEditor.Unindent();
		}
		else
		{
			if (!IsExistSelectedCell())
			{
				return;
			}
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
	}

	public void Cut()
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
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
		if (!IsExistSelectedCell())
		{
			return;
		}
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
		if (!IsExistSelectedCell())
		{
			return;
		}
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
						exception.Log("表单设计界面粘贴单元格时发生了未预期的异常");
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

	public void ImportTable()
	{
		FormSelectNode formSelectNode = new FormSelectNode();
		formSelectNode.Project = Leqisoft.Model.Project.Current;
		if (formSelectNode.ShowImportTicket() != DialogResult.OK)
		{
			return;
		}
		Leqisoft.Model.Table table = formSelectNode.SelectedTableNode.Table.LoadAndReturn();
		if (table.Ticket.IsEmpty())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格没有" + Ticket.GetLevelString() + "定义。");
			return;
		}
		TicketDesignTableVM ticketDesignTableVM = new TicketDesignTableVM(table.Ticket);
		if (!SoftwareLicenseManager.IsTableRowsAndColsOutOfLicenseLimit(ticketDesignTableVM.Rows.Count, ticketDesignTableVM.Columns.Count))
		{
			_vm.Title.Clear();
			_vm.Footer.Clear();
			_vm.CopyFrom(ticketDesignTableVM);
			_vm.SanitizeImportTable();
			PopulateImpl();
		}
	}

	public void ImportXlsx()
	{
		FormTicketImportXlsx formTicketImportXlsx = new FormTicketImportXlsx();
		if (formTicketImportXlsx.Import() && !SoftwareLicenseManager.IsTableRowsAndColsOutOfLicenseLimit(formTicketImportXlsx.Ticket.Rows.Count, formTicketImportXlsx.Ticket.Columns.Count))
		{
			_vm.Title.Clear();
			_vm.Footer.Clear();
			_vm.CopyFrom(formTicketImportXlsx.Ticket);
			PopulateImpl();
		}
	}

	private void Panel_MouseDown_EditorPanel(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			_ctxEmpty.ShowContextMenu(_editorPanel, e.Location);
		}
	}

	private void Panel_Resize_EditorPanel(object sender, EventArgs e)
	{
		if (_vm == null)
		{
			return;
		}
		_editorPanel.SuspendDrawing();
		try
		{
			AdjustDataGridPosition();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_editorPanel.ResumeDrawing();
		}
	}

	private void _gridColumns_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			_gridColumns.Invalidate();
		}
	}

	private void _gridColumns_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			_gridColumns.Invalidate();
		}
	}

	private void _gridColumns_GiveFeedback(object sender, GiveFeedbackEventArgs e)
	{
		C1FlexGridEx c1FlexGridEx = _grid;
		if (_titleEditor.IsInDragging)
		{
			c1FlexGridEx = _titleGrid;
		}
		else if (_footerEditor.IsInDragging)
		{
			c1FlexGridEx = _footerGrid;
		}
		if (c1FlexGridEx.HitTest().Type == HitTestTypeEnum.Cell)
		{
			c1FlexGridEx.Invalidate();
		}
	}

	private void _gridColumns_MouseDown(object sender, MouseEventArgs e)
	{
		if (_gridColumns.HitTest().Type == HitTestTypeEnum.Cell)
		{
			_dragRow = _gridColumns.MouseRow - _gridColumns.Rows.Fixed;
			_gridColumns.DoDragDrop(_dragRow, DragDropEffects.Copy);
		}
	}

	private void _gridColumns_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		string text = Table.Columns[e.Row].GetUniqueFormulaName();
		if (Control.ModifierKeys != Keys.Control)
		{
			text = "[" + text + "]";
		}
		e.Text = text;
		e.Image = Resources.ConfrimationCol;
	}

	private void _grid_DragLeave(object sender, EventArgs e)
	{
		_isDragging = false;
		_grid.Invalidate();
	}

	private void _grid_DragEnter(object sender, DragEventArgs e)
	{
		_isDragging = true;
		EnterDrag();
	}

	private void _grid_DragOver(object sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.Copy;
	}

	private void SetCellDefaultAlignType(TicketDesignCellVM cell, Type dataType)
	{
		if (dataType == typeof(string) || dataType == typeof(DateTime) || dataType == typeof(DateYearMonth) || dataType == typeof(TimeSpan))
		{
			cell.Align = CellTextAlign.MiddleLeft;
		}
		else if (dataType == typeof(double))
		{
			cell.Align = CellTextAlign.MiddleRight;
		}
		else if (dataType == typeof(bool))
		{
			cell.Align = CellTextAlign.MiddleCenter;
		}
	}

	private void _grid_DragDrop(object sender, DragEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Leqisoft.Model.Column column = Table.Columns[_dragRow];
			string text = column.GetUniqueFormulaName();
			Type dataType = column.GetDataType();
			bool flag = true;
			if (Control.ModifierKeys != Keys.Control)
			{
				text = "[" + text + "]";
				flag = false;
			}
			if (_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column) && !_grid.Selection.IsSingleCell)
			{
				for (int i = _grid.Selection.TopRow; i <= _grid.Selection.BottomRow; i++)
				{
					C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(i, hitTestInfo.Column);
					TicketDesignCellVM cell = _vm.GetCell(mergedRange.TopRow - _grid.Rows.Fixed, mergedRange.LeftCol - _grid.Cols.Fixed);
					bool flag2 = !string.IsNullOrEmpty(cell.Text);
					ReplaceCol(cell, text);
					if (!flag2 && !flag)
					{
						SetCellDefaultAlignType(cell, dataType);
					}
				}
			}
			else
			{
				C1.Win.C1FlexGrid.CellRange mergedRange2 = _grid.GetMergedRange(hitTestInfo.Row, hitTestInfo.Column);
				TicketDesignCellVM cell2 = _vm.GetCell(mergedRange2.TopRow - _grid.Rows.Fixed, mergedRange2.LeftCol - _grid.Cols.Fixed);
				bool flag3 = !string.IsNullOrEmpty(cell2.Text);
				ReplaceCol(cell2, text);
				if (!flag3 && !flag)
				{
					SetCellDefaultAlignType(cell2, dataType);
				}
			}
		}
		_isDragging = false;
		_grid.Invalidate();
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
		if (_vm.ColumnHeaderRowsCount > 0 && _columnHeaderRowBackgroundBrush == null)
		{
			Color color = _editorPanel.BackColor;
			if (color == Color.White)
			{
				color = Color.LightGray;
			}
			_columnHeaderRowBackgroundBrush = new SolidBrush(Leqisoft.UI.Controls.Util.DarkenColor(color, 0.01));
		}
		if (e.Col == 0)
		{
			if (_panelBackgroundBrush == null)
			{
				_panelBackgroundBrush = new SolidBrush(_editorPanel.BackColor);
			}
			Rectangle cellRect = _grid.GetCellRect(e.Row, e.Col);
			e.Graphics.FillRectangle(_panelBackgroundBrush, cellRect);
			e.Graphics.DrawString((e.Row + 1).ToString(), _rowNumberFont, _rowNumberBrush, cellRect, _rowNumberStringFormat);
			e.Handled = true;
		}
		else if (e.Row == 0 && e.Col > 0)
		{
			string excelColumnName = Leqisoft.Model.Column.GetExcelColumnName(e.Col - 1);
			Rectangle cellRect2 = _grid.GetCellRect(e.Row, e.Col);
			Rectangle rectangle = new Rectangle(cellRect2.X, -3, cellRect2.Width, _columnNameStringHeight);
			e.DrawCell(DrawCellFlags.Background | DrawCellFlags.Border);
			if (e.Row < _vm.ColumnHeaderRowsCount && !_grid.Selection.Contains(e.Row, e.Col) && e.Style?.BackColor == Color.White)
			{
				e.Graphics.FillRectangle(_columnHeaderRowBackgroundBrush, cellRect2);
			}
			e.Graphics.DrawString(excelColumnName, _columnNameFont, _columnNameBrush, rectangle, _columnNameStringFormat);
			e.DrawCell(DrawCellFlags.Content);
			e.Handled = true;
		}
		else if (e.Row < _vm.ColumnHeaderRowsCount && !_grid.Selection.Contains(e.Row, e.Col) && e.Style?.BackColor == Color.White)
		{
			Rectangle cellRect3 = _grid.GetCellRect(e.Row, e.Col);
			e.Graphics.FillRectangle(_columnHeaderRowBackgroundBrush, cellRect3);
			e.DrawCell(DrawCellFlags.Border | DrawCellFlags.Content);
			e.Handled = true;
		}
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

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			if (_grid.HitTest().Type == HitTestTypeEnum.None)
			{
				_ctxEmpty.ShowContextMenu(_grid, e.Location);
			}
			else
			{
				_ctxCell.ShowContextMenu(_grid, e.Location);
			}
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		_isMouseDownFromCell = false;
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (e.Button == MouseButtons.Right)
		{
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.Cell:
				_ctxCell.ShowContextMenu(_grid, e.Location);
				break;
			case HitTestTypeEnum.ColumnHeader:
				if (hitTestInfo.Column >= _grid.Cols.Fixed)
				{
					_ctxColumn.ShowContextMenu(_grid, e.Location);
				}
				break;
			case HitTestTypeEnum.RowHeader:
				_ctxRow.ShowContextMenu(_grid, e.Location);
				break;
			}
		}
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			_isMouseDownFromCell = true;
		}
	}

	private void _grid_BodyBeforeEdit(object sender, RowColEventArgs e)
	{
		TicketDesignCellVM cell = _vm.GetCell(e.Row, e.Col);
		_grid.BodySetData(e.Row, e.Col, cell.Text);
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
			using Pen pen = new Pen(NormalCellBorderColor, 1f);
			int widthDisplay = _grid.Cols[0].WidthDisplay;
			int x = Math.Min(_grid.GetGridWidth(), _grid.ClientSize.Width);
			e.Graphics.DrawLine(pen, widthDisplay, 0, x, 0);
			e.Graphics.DrawLine(pen, widthDisplay, 0, widthDisplay, GetGridHeight());
		}
		void Paint_Border()
		{
			if (_vm != null)
			{
				int count = _vm.Rows.Count;
				int count2 = _vm.Columns.Count;
				for (int i = 0; i < count; i++)
				{
					int j;
					for (j = 0; j < count2; j++)
					{
						TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(i, j));
						TicketDesignCellVM cell = _vm.GetCell(i, j);
						Rectangle cellRect = _grid.GetCellRect(i + _grid.Rows.Fixed, j + _grid.Cols.Fixed);
						cellRect.Offset(-1, -1);
						if (cell.Top.Width > 0 && (ticketMerge == null || i == ticketMerge.TopRow))
						{
							Rectangle rectangle = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (i == 0)
							{
								rectangle.Y += cell.Top.Width;
								rectangle.Height -= cell.Top.Width;
							}
							if (j == 0)
							{
								rectangle.X++;
								rectangle.Width--;
							}
							using Pen pen = new Pen(_colorBorder, cell.Top.Width);
							e.Graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
						}
						if (cell.Right.Width > 0 && (ticketMerge == null || j == ticketMerge.RightColumn))
						{
							Rectangle rectangle2 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (i == count - 1)
							{
								int num = 1;
								rectangle2.Height += num;
							}
							using Pen pen2 = new Pen(_colorBorder, cell.Right.Width);
							e.Graphics.DrawLine(pen2, rectangle2.Right, rectangle2.Top, rectangle2.Right, rectangle2.Bottom);
						}
						if (cell.Bottom.Width > 0 && (ticketMerge == null || i == ticketMerge.BottomRow))
						{
							Rectangle rectangle3 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (j == 0)
							{
								rectangle3.X++;
								rectangle3.Width--;
							}
							using Pen pen3 = new Pen(_colorBorder, cell.Bottom.Width);
							e.Graphics.DrawLine(pen3, rectangle3.Left, rectangle3.Bottom, rectangle3.Right, rectangle3.Bottom);
						}
						if (cell.Left.Width > 0 && (ticketMerge == null || j == ticketMerge.LeftColumn))
						{
							Rectangle rectangle4 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (j == 0)
							{
								int num2 = 1 + cell.Left.Width / 2;
								rectangle4.X += num2;
								rectangle4.Width -= num2;
							}
							using Pen pen4 = new Pen(_colorBorder, cell.Left.Width);
							e.Graphics.DrawLine(pen4, rectangle4.Left, rectangle4.Top, rectangle4.Left, rectangle4.Bottom);
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
					Rectangle rectangle;
					if (selection.Contains(hitTestInfo.Row, hitTestInfo.Column) && !selection.IsSingleCell)
					{
						rectangle = _grid.GetCellRangeRectUnclipped(selection.TopRow, selection.LeftCol, selection.BottomRow, selection.RightCol);
						Point point = _grid.PointToScreen(new Point(rectangle.X, rectangle.Y));
						Point point2 = _editorPanel.PointToScreen(new Point(_grid.Location.X + GetGridFixedColumnWidth(), _grid.Location.Y));
						if (point.X < point2.X)
						{
							int num = point2.X - point.X;
							rectangle.X += num;
							rectangle.Width -= num;
						}
					}
					else
					{
						rectangle = _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
					}
					Point point3 = _grid.PointToScreen(new Point(rectangle.X, rectangle.Y));
					Point point4 = _editorPanel.PointToScreen(new Point(_grid.Location.X + GetGridFixedColumnWidth(), _grid.Location.Y));
					if (point3.X == point4.X)
					{
						rectangle.X += _dragRectLeftEdgeOffset;
						rectangle.Width -= _dragRectLeftEdgeOffset;
					}
					if (point3.Y == point4.Y)
					{
						int num2 = ((_titleGrid.Rows.Count > 0) ? _dragRectTopEdgeOffsetExistTitle : _dragRectTopEdgeOffsetUnExistTitle);
						rectangle.Y += num2;
						rectangle.Height -= num2;
					}
					Point point5 = _grid.PointToScreen(new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
					Point point6 = _grid.ScrollBarsVisible switch
					{
						ScrollBars.Horizontal => _editorPanel.PointToScreen(new Point(_grid.Location.X + _grid.Width, _grid.Location.Y + _grid.Height - SystemInformation.HorizontalScrollBarHeight)), 
						ScrollBars.Vertical => _editorPanel.PointToScreen(new Point(_grid.Location.X + GetGridWidth(), _grid.Location.Y + _grid.Height)), 
						ScrollBars.Both => _editorPanel.PointToScreen(new Point(_grid.Location.X + _grid.Width - SystemInformation.VerticalScrollBarWidth, _grid.Location.Y + _grid.Height - SystemInformation.HorizontalScrollBarHeight)), 
						_ => _editorPanel.PointToScreen(new Point(_grid.Location.X + GetGridWidth(), _grid.Location.Y + _grid.Height)), 
					};
					if (point5.X == point6.X)
					{
						rectangle.Width -= _dragRectRightEdgeOffset;
					}
					if (point5.Y == point6.Y)
					{
						rectangle.Height -= ((_footerGrid.Rows.Count > 0) ? _dragRectBottomEdgeOffsetExistFooter : _dragRectBottomEdgeOffsetUnExistFooter);
					}
					e.Graphics.DrawRectangle(_pen, rectangle);
					e.Graphics.FillRectangle(_brush, rectangle);
					string text = Table.Columns[_dragRow].GetUniqueFormulaName();
					if (Control.ModifierKeys != Keys.Control)
					{
						text = "[" + text + "]";
					}
					e.Graphics.DrawString(text, _grid.Font, _brushText, rectangle, _sf);
				}
			}
		}
	}

	private void _grid_MouseUp(object sender, MouseEventArgs e)
	{
		FormulaEditor formulaEditor = Program.MainForm.FormulaEditor;
		if (formulaEditor.IsEditing)
		{
			if (_isMouseDownFromCell && _grid.Selection.IsValid)
			{
				C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
				string text = ((!bodySelection.IsSingleCell) ? $"[{Leqisoft.Model.Column.GetExcelColumnName(bodySelection.LeftCol)},{bodySelection.TopRow + 1}:{Leqisoft.Model.Column.GetExcelColumnName(bodySelection.RightCol)},{bodySelection.BottomRow + 1}]" : $"[{Leqisoft.Model.Column.GetExcelColumnName(bodySelection.LeftCol)},{bodySelection.TopRow + 1}]");
				formulaEditor.RemoveRefAtPos();
				formulaEditor.InsertRefText(text);
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

	private void _cmdAppendColumns_Click(object sender, ClickEventArgs e)
	{
		AppendColumns();
	}

	private void _cmdSplitCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
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

	private void _cmdSplitCells_Click(object sender, ClickEventArgs e)
	{
		SplitCells();
	}

	private void _cmdMergeCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!IsExistSelectedCell())
		{
			e.Visible = false;
		}
		else
		{
			_cmdMergeCells.Visible = !_grid.Selection.IsSingleCell;
		}
	}

	private void _cmdMergeCells_Click(object sender, ClickEventArgs e)
	{
		MergeCells();
	}

	private void _cmdMergeEveryHorizontalCells_Click(object sender, ClickEventArgs e)
	{
		MergeEveryHorizontalCells();
	}

	private void _cmdMergeEveryHorizontalCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!IsExistSelectedCell())
		{
			e.Visible = false;
		}
		else
		{
			_cmdMergeEveryHorizontalCells.Visible = !_grid.Selection.IsSingleCell;
		}
	}

	private void _cmdMoveDownRow_Click(object sender, ClickEventArgs e)
	{
	}

	private void _cmdMoveUpRow_Click(object sender, ClickEventArgs e)
	{
	}

	private void _cmdRemoveRows_Click(object sender, ClickEventArgs e)
	{
		RemoveRows();
	}

	private void _cmdInsertRows_Click(object sender, ClickEventArgs e)
	{
		InsertRows();
	}

	private void _cmdAlign_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		_cmdAlign.Visible = _grid.BodyCol >= 0;
	}

	private void _cmdRemoveColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		_cmdRemoveColumns.Visible = _grid.BodyCol >= 0;
	}

	private void _cmdRemoveColumns_Click(object sender, ClickEventArgs e)
	{
		RemoveColumns();
	}

	private void _cmdInsertColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		_cmdInsertColumns.Visible = _grid.BodyCol >= 0;
	}

	private void _cmdInsertColumns_Click(object sender, ClickEventArgs e)
	{
		InsertColumns();
	}

	private void _cmdField_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private bool IsExistSelectedCell()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return false;
		}
		return true;
	}

	private void _cmdField_Popup(object sender, EventArgs e)
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
		_cmdField.CommandLinks.Clear();
		foreach (Leqisoft.Model.Column column in Table.Columns)
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

	private void _cmdDeleteFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = IsExistSelectedCell();
	}

	private void _cmdDeleteFormula_Click(object sender, EventArgs e)
	{
		if (!IsExistSelectedCell())
		{
			return;
		}
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
		Save();
	}

	private void _cmdHelp_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ShowHelpCenter();
	}

	private void _cmdSetColumnWidth_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		_cmdSetColumnWidth.Visible = _grid.BodyCol >= 0;
	}

	private void _cmdSetColumnWidth_Click(object sender, ClickEventArgs e)
	{
		SetColumnWidth();
	}

	private void _cmdSetRowHeight_Click(object sender, ClickEventArgs e)
	{
		SetRowHeight();
	}

	private void _cmdCancelDesign_Click(object sender, ClickEventArgs e)
	{
		CancelSave();
	}

	private void _cmdFormatText_Click(object sender, ClickEventArgs e)
	{
		SetFormatText();
	}

	private void AddAlign(string text, System.Drawing.Image image, CellTextAlign align)
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
		_cmdAlign.CommandLinks.Add(value);
	}

	public void AutoAdjustDataGridPosition()
	{
		AdjustDataGridPosition();
	}

	protected void AdjustDataGridPosition()
	{
		_grid.BeginUpdate();
		try
		{
			AdjustGridPositionImpl();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	public int GetGridWidth()
	{
		int num = 0;
		int count = _grid.Cols.Count;
		for (int i = 0; i < count; i++)
		{
			num += _grid.Cols[i].WidthDisplay;
		}
		return num;
	}

	public int GetGridWidthWithoutFixedColumn()
	{
		int num = 0;
		int count = _grid.Cols.Count;
		for (int i = _grid.Cols.Fixed; i < count; i++)
		{
			num += _grid.Cols[i].WidthDisplay;
		}
		return num;
	}

	public int GetGridFixedColumnWidth()
	{
		int num = 0;
		for (int num2 = _grid.Cols.Fixed - 1; num2 >= 0; num2--)
		{
			num += _grid.Cols[num2].WidthDisplay;
		}
		return num;
	}

	public int GetGridHeight()
	{
		int num = 0;
		int count = _grid.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			num += _grid.Rows[i].HeightDisplay;
		}
		return num;
	}

	public Size GetEditorPanelSize()
	{
		return _editorPanel.Size;
	}

	private List<float> GetGridUnFixedColumnWidthPercent()
	{
		int count = _grid.Cols.Count;
		int totalWidth = 0;
		List<int> list = new List<int>(count);
		for (int i = _grid.Cols.Fixed; i < count; i++)
		{
			int widthDisplay = _grid.Cols[i].WidthDisplay;
			totalWidth += widthDisplay;
			list.Add(widthDisplay);
		}
		return list.Select((int u) => (float)u * 1f / (float)totalWidth).ToList();
	}

	public void EnterFootEdit()
	{
		_titleEditor.LeaveEdit();
		LeaveEdit();
	}

	public void EnterTitleEdit()
	{
		_footerEditor.LeaveEdit();
		LeaveEdit();
	}

	protected void EnterEdit()
	{
		_titleEditor.LeaveEdit();
		_footerEditor.LeaveEdit();
	}

	protected void EnterDrag()
	{
		_titleEditor.LeaveDrag();
		_footerEditor.LeaveDrag();
	}

	protected void LeaveEdit()
	{
		CancelSelect();
	}

	public void EnterTitleDrag()
	{
		_isDragging = false;
		_footerEditor.LeaveDrag();
		_grid.Invalidate();
	}

	public void EnterFooterDrag()
	{
		_isDragging = false;
		_titleEditor.LeaveDrag();
		_grid.Invalidate();
	}

	private void CancelSelect()
	{
		_grid.Select(-1, -1);
	}

	public void IncreaseGridWidth(int value, Action titleFooterWidthAdjustCallback)
	{
		int count = _grid.Cols.Count;
		List<float> gridUnFixedColumnWidthPercent = GetGridUnFixedColumnWidthPercent();
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			int num = value;
			for (int i = _grid.Cols.Fixed; i < count; i++)
			{
				int num2 = 0;
				if (i == count - 1)
				{
					num2 = _grid.Cols[i].WidthDisplay + num;
				}
				else
				{
					int num3 = (int)(gridUnFixedColumnWidthPercent[i - _grid.Cols.Fixed] * (float)value);
					num2 = _grid.Cols[i].WidthDisplay + num3;
					num -= num3;
				}
				num2 = Math.Max(1, num2);
				_grid.Cols[i].WidthDisplay = num2;
				_vm.Columns[i - _grid.Cols.Fixed].Width = num2;
			}
			AdjustGridPositionImpl();
			titleFooterWidthAdjustCallback();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	public void IncreaseGridWidth(int value)
	{
		int count = _grid.Cols.Count;
		List<float> gridUnFixedColumnWidthPercent = GetGridUnFixedColumnWidthPercent();
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			int num = value;
			for (int i = _grid.Cols.Fixed; i < count; i++)
			{
				int num2 = 0;
				if (i == count - 1)
				{
					num2 = _grid.Cols[i].WidthDisplay + num;
				}
				else
				{
					int num3 = (int)(gridUnFixedColumnWidthPercent[i - _grid.Cols.Fixed] * (float)value);
					num2 = _grid.Cols[i].WidthDisplay + num3;
					num -= num3;
				}
				num2 = Math.Max(1, num2);
				_grid.Cols[i].WidthDisplay = num2;
				_vm.Columns[i - _grid.Cols.Fixed].Width = num2;
			}
			AdjustGridPositionImpl();
			_titleEditor.AutoAdjustGridWidth();
			_footerEditor.AutoAdjustGridWidth();
		}
		finally
		{
			_grid.EndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	private void AdjustGridPositionImpl()
	{
		int allRowsTotalHeight = _titleEditor.GetAllRowsTotalHeight();
		int allRowsTotalHeight2 = _footerEditor.GetAllRowsTotalHeight();
		int height = _editorPanel.Height - allRowsTotalHeight - allRowsTotalHeight2;
		_grid.AdjustPosition(new Size(_editorPanel.Width, height), 0, top0: true);
		int num = (_editorPanel.Height - allRowsTotalHeight - _grid.Height - allRowsTotalHeight2) / 2;
		_titleGrid.Top = num;
		_titleGrid.Left = _grid.Left + GetGridFixedColumnWidth();
		int num2 = ((!_titleEditor.IsLastRowExistBottomBorder() && !IsFirstRowExistTopBorder()) ? 1 : 0);
		_grid.Top = num + allRowsTotalHeight - num2;
		num2 = ((!_footerEditor.IsFirstRowExistTopBorder()) ? 1 : 0);
		_footerGrid.Top = _grid.Top + _grid.Height - num2;
		_footerGrid.Left = _titleGrid.Left;
		if (allRowsTotalHeight2 == 0 && (_grid.ScrollBarsVisible == ScrollBars.Horizontal || GetGridHeight() < _editorPanel.Height - _grid.Top))
		{
			_grid.Height = _editorPanel.Height - _grid.Top;
		}
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

	private void PopulateMerge()
	{
		_grid.MergedRanges.Clear();
		foreach (TicketMerge merge in _vm.Merges)
		{
			_grid.BodyAddMergedRange(merge.TopRow, merge.LeftColumn, merge.BottomRow, merge.RightColumn);
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
				formulaEditor.Context.Table = Table;
				formulaEditor.Context.TicketCell = cell2;
				formulaEditor.Context.DataRowStart = Ticket.DataRowStart;
				formulaEditor.Context.DataRowCount = Ticket.DataRowCount;
				formulaEditor.Context.Ticket = Ticket;
				formulaEditor.Populate();
			}
		}
	}

	private void SetLevel()
	{
		AppCommandTabs.TicketDesign.RibbonTab.Text = Ticket.GetLevelString() + "设计";
		(AppCommands.TicketImportExcel.RibbonItem as RibbonButton).Text = "导入Excel" + Ticket.GetLevelString() + "样式";
		(AppCommands.TicketImportTable.RibbonItem as RibbonButton).Text = "导入他表" + Ticket.GetLevelString() + "样式";
		(AppCommands.TicketImportTable.RibbonItem as RibbonButton).LargeImage = Resources.TicketMode;
	}
}
