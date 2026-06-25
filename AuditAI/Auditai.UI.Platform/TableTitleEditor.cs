using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class TableTitleEditor
{
	private static readonly C1TextBoxEx _dateEdit;

	private static readonly C1TextBoxEx _timeEdit;

	private const int MAX_TITLE_LENGTH = 10000;

	private C1.Win.C1FlexGrid.CellRange? _lastMouseUpCell;

	private readonly C1FlexGridEx _grid;

	private readonly TooltipManager tooltipManager = new TooltipManager();

	private readonly TableEditor _owner;

	private readonly C1ContextMenu ctxTitle = new C1ContextMenu();

	private readonly C1Command cmdInsertRow = new C1Command
	{
		Text = "插入副标题行",
		Image = ContextResources.ctxInsertRow
	};

	private readonly C1CommandLink lnkInsertRow = new C1CommandLink();

	private readonly C1CommandLink lnkAppendRow = new C1CommandLink();

	private readonly C1Command cmdAppendRow = new C1Command();

	private readonly C1CommandLink lnkRemoveRow = new C1CommandLink();

	private readonly C1Command cmdRemoveRow = new C1Command();

	private readonly C1CommandLink lnkHeight = new C1CommandLink();

	private readonly C1Command cmdHeight = new C1Command();

	private readonly C1CommandLink lnkMakerSign = new C1CommandLink();

	private readonly C1Command cmdMakerSign = new C1Command();

	private readonly C1CommandLink lnkCheckerSign = new C1CommandLink();

	private readonly C1Command cmdCheckerSign = new C1Command();

	private readonly C1Command cmdAuxEdit = new C1Command
	{
		Text = "下拉列表...",
		Image = Auditai.UI.Platform.Properties.Resources.ComboList16
	};

	private readonly C1CommandLink lnkAuxEdit = new C1CommandLink();

	private readonly C1Command cmdEditComment = new C1Command
	{
		Text = "编辑注释...",
		Image = ContextResources.ctxParagraphComment
	};

	private readonly C1CommandLink lnkEditComment = new C1CommandLink();

	private readonly C1Command cmdRemoveFormula = new C1Command
	{
		Text = "删除单元格公式",
		Image = ContextResources.ctxDeleteFormula
	};

	private readonly C1CommandLink lnkRemoveFormula = new C1CommandLink();

	private readonly C1Command cmdInsertColumn = new C1Command
	{
		Text = "插入副标题列",
		Image = ContextResources.ctxInsertColumn
	};

	private readonly C1CommandLink lnkInsertColumn = new C1CommandLink();

	private readonly C1Command cmdAppendColumn = new C1Command
	{
		Text = "追加副标题列",
		Image = ContextResources.ctxAppendColumn
	};

	private readonly C1CommandLink lnkAppendColumn = new C1CommandLink();

	private readonly C1Command cmdRemoveColumn = new C1Command
	{
		Text = "删除副标题列",
		Image = ContextResources.ctxDeleteColumn
	};

	private readonly C1CommandLink lnkRemoveColumn = new C1CommandLink();

	private readonly C1Command cmdColumnSameWidth = new C1Command
	{
		Text = "平均分布列宽"
	};

	private readonly C1CommandLink lnkMergeEveryHorizontalCells = new C1CommandLink();

	private readonly C1Command cmdMergeEveryHorizontalCells = new C1Command
	{
		Text = "仅横向合并单元格"
	};

	private readonly C1CommandLink lnkColumnSameWidth = new C1CommandLink();

	private readonly C1Command cmdMergeCells = new C1Command
	{
		Text = "合并单元格"
	};

	private readonly C1CommandLink lnkMergeCells = new C1CommandLink();

	private readonly C1Command cmdSplitCells = new C1Command
	{
		Text = "拆分单元格"
	};

	private readonly C1CommandLink lnkSplitCells = new C1CommandLink();

	private readonly C1CommandLink lnkAlign = new C1CommandLink();

	private readonly C1CommandMenu cmdAlign = new C1CommandMenu();

	private readonly C1CommandLink lnkAlignTopLeft = new C1CommandLink();

	private readonly C1Command cmdAlignTopLeft = new C1Command();

	private readonly C1CommandLink lnkAlignTopCenter = new C1CommandLink();

	private readonly C1Command cmdAlignTopCenter = new C1Command();

	private readonly C1CommandLink lnkAlignTopRight = new C1CommandLink();

	private readonly C1Command cmdAlignTopRight = new C1Command();

	private readonly C1CommandLink lnkAlignMiddleLeft = new C1CommandLink();

	private readonly C1Command cmdAlignMiddleLeft = new C1Command();

	private readonly C1CommandLink lnkAlignMiddleCenter = new C1CommandLink();

	private readonly C1Command cmdAlignMiddleCenter = new C1Command();

	private readonly C1CommandLink lnkAlignMiddleRight = new C1CommandLink();

	private readonly C1Command cmdAlignMiddleRight = new C1Command();

	private readonly C1CommandLink lnkAlignBottomLeft = new C1CommandLink();

	private readonly C1Command cmdAlignBottomLeft = new C1Command();

	private readonly C1CommandLink lnkAlignBottomCenter = new C1CommandLink();

	private readonly C1Command cmdAlignBottomCenter = new C1Command();

	private readonly C1CommandLink lnkAlignBottomRight = new C1CommandLink();

	private readonly C1Command cmdAlignBottomRight = new C1Command();

	private readonly C1CommandMenu cmdDataType = new C1CommandMenu();

	private readonly C1CommandLink lnkDataType = new C1CommandLink();

	private readonly C1Command cmdDataTypeString = new C1Command();

	private readonly C1CommandLink lnkDataTypeString = new C1CommandLink();

	private readonly C1Command cmdAddToNavTree = new C1Command();

	private readonly C1CommandLink lnkAddToNavTree = new C1CommandLink();

	private readonly C1Command cmdRemoveFromNavTree = new C1Command();

	private readonly C1CommandLink lnkRemoveFromNavTree = new C1CommandLink();

	private readonly TooltipBox _ttpComment = new TooltipBox
	{
		IsBalloon = true
	};

	private readonly GridResizingManager _gridResizingManager;

	private readonly Pen _penBorder = new Pen(Color.White);

	private List<TableTitleCell> _preserveAuxEditSelection = new List<TableTitleCell>();

	public C1FlexGridEx View => _grid;

	public TableTitle Title { get; set; }

	public bool IsEditing { get; set; }

	public AuxEditor AuxEditor { get; private set; }

	public ListDropDown ListDropDown { get; private set; }

	public InputListDropDown InputListDropDown { get; private set; }

	static TableTitleEditor()
	{
		_dateEdit = new C1TextBoxEx();
		_timeEdit = new C1TextBoxEx();
		_dateEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.EditFormat.FormatType = FormatTypeEnum.UseEvent;
		_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.ErrorInfo.ShowErrorMessage = false;
	}

	public TableTitleEditor(TableEditor owner)
	{
		_owner = owner;
		_grid = new C1FlexGridEx
		{
			AllowSorting = AllowSortingEnum.None,
			AllowResizing = AllowResizingEnum.None,
			ScrollBars = ScrollBars.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowEditing = true,
			VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom,
			DrawMode = DrawModeEnum.OwnerDraw,
			FocusRect = FocusRectEnum.None
		};
		cmdInsertRow.CommandStateQuery += CmdInsertRow_CommandStateQuery;
		cmdInsertRow.Click += CmdInsertRow_Click;
		lnkInsertRow.Command = cmdInsertRow;
		ctxTitle.CommandLinks.Add(lnkInsertRow);
		cmdAppendRow.CommandStateQuery += CmdAddRow_CommandStateQuery;
		cmdAppendRow.Click += CmdAddRow_Click;
		lnkAppendRow.Command = cmdAppendRow;
		ctxTitle.CommandLinks.Add(lnkAppendRow);
		cmdRemoveRow.CommandStateQuery += CmdRemoveRow_CommandStateQuery;
		cmdRemoveRow.Click += CmdRemoveRow_Click;
		lnkRemoveRow.Command = cmdRemoveRow;
		ctxTitle.CommandLinks.Add(lnkRemoveRow);
		cmdHeight.CommandStateQuery += CmdHeight_CommandStateQuery;
		cmdHeight.Click += CmdHeight_Click;
		lnkHeight.Command = cmdHeight;
		ctxTitle.CommandLinks.Add(lnkHeight);
		cmdInsertColumn.CommandStateQuery += CmdInsertColumn_CommandStateQuery;
		cmdInsertColumn.Click += CmdInsertColumn_Click;
		lnkInsertColumn.Command = cmdInsertColumn;
		lnkInsertColumn.Delimiter = true;
		ctxTitle.CommandLinks.Add(lnkInsertColumn);
		cmdAppendColumn.CommandStateQuery += CmdAppendColumn_CommandStateQuery;
		cmdAppendColumn.Click += CmdAppendColumn_Click;
		lnkAppendColumn.Command = cmdAppendColumn;
		ctxTitle.CommandLinks.Add(lnkAppendColumn);
		cmdRemoveColumn.CommandStateQuery += CmdRemoveColumn_CommandStateQuery;
		cmdRemoveColumn.Click += CmdRemoveColumn_Click;
		lnkRemoveColumn.Command = cmdRemoveColumn;
		ctxTitle.CommandLinks.Add(lnkRemoveColumn);
		cmdColumnSameWidth.Click += CmdColumnSameWidth_Click;
		lnkColumnSameWidth.Command = cmdColumnSameWidth;
		cmdMergeCells.Click += CmdMergeCells_Click;
		cmdMergeCells.CommandStateQuery += CmdMergeCells_CommandStateQuery;
		lnkMergeCells.Delimiter = true;
		lnkMergeCells.Command = cmdMergeCells;
		ctxTitle.CommandLinks.Add(lnkMergeCells);
		cmdMergeEveryHorizontalCells.Click += CmdMergeEveryHorizontalCells_Click;
		cmdMergeEveryHorizontalCells.CommandStateQuery += CmdMergeEveryHorizontalCells_CommandStateQuery;
		lnkMergeEveryHorizontalCells.Command = cmdMergeEveryHorizontalCells;
		ctxTitle.CommandLinks.Add(lnkMergeEveryHorizontalCells);
		cmdSplitCells.Click += CmdSplitCells_Click;
		cmdSplitCells.CommandStateQuery += CmdSplitCells_CommandStateQuery;
		lnkSplitCells.Command = cmdSplitCells;
		ctxTitle.CommandLinks.Add(lnkSplitCells);
		cmdMakerSign.CommandStateQuery += CmdMakerSign_CommandStateQuery;
		cmdMakerSign.Click += CmdMakerSign_Click;
		lnkMakerSign.Command = cmdMakerSign;
		cmdCheckerSign.CommandStateQuery += CmdCheckerSign_CommandStateQuery;
		cmdCheckerSign.Click += CmdCheckerSign_Click;
		lnkCheckerSign.Command = cmdCheckerSign;
		cmdAuxEdit.Click += CmdAuxEdit_Click;
		cmdAuxEdit.CommandStateQuery += CmdAuxEdit_CommandStateQuery;
		lnkAuxEdit.Command = cmdAuxEdit;
		lnkAuxEdit.Delimiter = true;
		ctxTitle.CommandLinks.Add(lnkAuxEdit);
		cmdEditComment.Click += CmdEditComment_Click;
		lnkEditComment.Command = cmdEditComment;
		ctxTitle.CommandLinks.Add(lnkEditComment);
		cmdDataType.Text = "数据格式";
		lnkDataType.Command = cmdDataType;
		ctxTitle.CommandLinks.Add(lnkDataType);
		cmdDataTypeString.Text = "文本格式";
		cmdDataTypeString.Click += CmdDataTypeString_Click;
		lnkDataTypeString.Command = cmdDataTypeString;
		cmdDataType.CommandLinks.Add(lnkDataTypeString);
		AddCommandMenu("数值格式", SetDataFormatNumeric, Tuple.Create("1234.56", DataFormatType.Number), Tuple.Create("1,234.56", DataFormatType.Comma), Tuple.Create("$1,234.56", DataFormatType.NumDollar), Tuple.Create("￥1,234.56", DataFormatType.NumRmb), Tuple.Create("123,456.78%", DataFormatType.Percentage));
		C1CommandMenu menu = AddCommandMenu("日期格式", SetDataFormatDate, Tuple.Create("2017年12月31日", DataFormatType.DateChinese), Tuple.Create("2017-12-31", DataFormatType.DateDash), Tuple.Create("2017/12/31", DataFormatType.DateSlash), Tuple.Create("2017.12.31", DataFormatType.DateDot));
		AppendCommandMenu(menu, SetDataFormatDateYearMonth, Tuple.Create("2017年12月", DataFormatType.DateYearMonthChinese), Tuple.Create("2017-12", DataFormatType.DateYearMonthDash), Tuple.Create("2017/12", DataFormatType.DateYearMonthSlash), Tuple.Create("2017.12", DataFormatType.DateYearMonthDot));
		AddCommandMenu("时间格式", SetDataFormatTime, Tuple.Create("10时20分30秒", DataFormatType.TimeLongChinese), Tuple.Create("10时20分", DataFormatType.TimeShortChinese), Tuple.Create("10:20:30", DataFormatType.TimeLong), Tuple.Create("10:20", DataFormatType.TimeShort));
		AddCommandMenu("判断格式", SetDataFormatBoolean, Tuple.Create("复选框", DataFormatType.BoolCheckBox), Tuple.Create("开关钮", DataFormatType.BoolOnOff));
		cmdAlign.Text = "对齐";
		cmdAlign.Popup += CmdAlign_Popup;
		lnkAlign.Command = cmdAlign;
		ctxTitle.CommandLinks.Add(lnkAlign);
		cmdAlignTopLeft.Text = "左上对齐";
		cmdAlignTopLeft.Image = ContextResources.ctxAlignTopLeft;
		cmdAlignTopLeft.Click += CmdAlignTopLeft_Click;
		lnkAlignTopLeft.Command = cmdAlignTopLeft;
		cmdAlign.CommandLinks.Add(lnkAlignTopLeft);
		cmdAlignTopCenter.Text = "中上对齐";
		cmdAlignTopCenter.Image = ContextResources.ctxAlignTopCenter;
		cmdAlignTopCenter.Click += CmdAlignTopCenter_Click;
		lnkAlignTopCenter.Command = cmdAlignTopCenter;
		cmdAlign.CommandLinks.Add(lnkAlignTopCenter);
		cmdAlignTopRight.Text = "右上对齐";
		cmdAlignTopRight.Image = ContextResources.ctxAlignTopRight;
		cmdAlignTopRight.Click += CmdAlignTopRight_Click;
		lnkAlignTopRight.Command = cmdAlignTopRight;
		cmdAlign.CommandLinks.Add(lnkAlignTopRight);
		cmdAlignMiddleLeft.Text = "左中对齐";
		cmdAlignMiddleLeft.Image = ContextResources.ctxAlignMiddleLeft;
		cmdAlignMiddleLeft.Click += CmdAlignMiddleLeft_Click;
		lnkAlignMiddleLeft.Command = cmdAlignMiddleLeft;
		cmdAlign.CommandLinks.Add(lnkAlignMiddleLeft);
		cmdAlignMiddleCenter.Text = "中中对齐";
		cmdAlignMiddleCenter.Image = ContextResources.ctxAlignMiddleCenter;
		cmdAlignMiddleCenter.Click += CmdAlignMiddleCenter_Click;
		lnkAlignMiddleCenter.Command = cmdAlignMiddleCenter;
		cmdAlign.CommandLinks.Add(lnkAlignMiddleCenter);
		cmdAlignMiddleRight.Text = "右中对齐";
		cmdAlignMiddleRight.Image = ContextResources.ctxAlignMiddleRight;
		cmdAlignMiddleRight.Click += CmdAlignMiddleRight_Click;
		lnkAlignMiddleRight.Command = cmdAlignMiddleRight;
		cmdAlign.CommandLinks.Add(lnkAlignMiddleRight);
		cmdAlignBottomLeft.Text = "左下对齐";
		cmdAlignBottomLeft.Image = ContextResources.ctxAlignBottomLeft;
		cmdAlignBottomLeft.Click += CmdAlignBottomLeft_Click;
		lnkAlignBottomLeft.Command = cmdAlignBottomLeft;
		cmdAlign.CommandLinks.Add(lnkAlignBottomLeft);
		cmdAlignBottomCenter.Text = "中下对齐";
		cmdAlignBottomCenter.Image = ContextResources.ctxAlignBottomCenter;
		cmdAlignBottomCenter.Click += CmdAlignBottomCenter_Click;
		lnkAlignBottomCenter.Command = cmdAlignBottomCenter;
		cmdAlign.CommandLinks.Add(lnkAlignBottomCenter);
		cmdAlignBottomRight.Text = "右下对齐";
		cmdAlignBottomRight.Image = ContextResources.ctxAlignBottomRight;
		cmdAlignBottomRight.Click += CmdAlignBottomRight_Click;
		lnkAlignBottomRight.Command = cmdAlignBottomRight;
		cmdAlign.CommandLinks.Add(lnkAlignBottomRight);
		cmdAddToNavTree.Text = "生成快捷查询列表";
		cmdAddToNavTree.Click += CmdAddToNavTree_Click;
		cmdAddToNavTree.CommandStateQuery += CmdAddToNavTree_CommandStateQuery;
		lnkAddToNavTree.Command = cmdAddToNavTree;
		lnkAddToNavTree.Delimiter = true;
		ctxTitle.CommandLinks.Add(lnkAddToNavTree);
		cmdRemoveFormula.CommandStateQuery += CmdRemoveFormula_CommandStateQuery;
		cmdRemoveFormula.Click += CmdRemoveFormula_Click;
		lnkRemoveFormula.Command = cmdRemoveFormula;
		lnkRemoveFormula.Delimiter = true;
		ctxTitle.CommandLinks.Add(lnkRemoveFormula);
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 0;
		_grid.Select(-1, -1);
		_grid.AfterEdit += _grid_AfterEdit;
		_grid.MouseClick += _grid_MouseClick;
		_grid.KeyDown += _grid_KeyDown;
		_grid.BeforeEdit += _grid_BeforeEdit;
		_grid.MouseEnterCell += _grid_MouseEnterCell;
		_grid.MouseLeaveCell += _grid_MouseLeaveCell;
		_grid.MouseUp += _grid_MouseUp;
		_grid.MouseDown += _grid_MouseDown;
		_grid.SelChange += _grid_SelChange;
		_grid.Paint += _grid_Paint;
		_grid.SetupEditor += _grid_SetupEditor;
		_grid.StartEdit += _grid_StartEdit;
		_grid.ValidateEdit += _grid_ValidateEdit;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.BodyBeforeEdit += _grid_BodyBeforeEdit;
		_grid.BeforeMouseDown += _grid_BeforeMouseDown;
		_grid.PaintBackground += _grid_PaintBackground;
		_grid.Enter += _grid_Enter;
		AttachTooltip();
		_owner.ValidationEditor.BeganEditing += ValidationEditor_BeganEditing;
		_owner.ValidationEditor.FinishedEditing += ValidationEditor_FinishedEditing;
		AuxEditor = new AuxEditor();
		ListDropDown = new ListDropDown(_grid);
		InputListDropDown = new InputListDropDown(_grid);
		_gridResizingManager = new GridResizingManager(_grid);
		_gridResizingManager.ResizeColumn += _gridResizingManager_ResizeColumn;
		_gridResizingManager.ResizeRow += _gridResizingManager_ResizeRow;
	}

	private void CmdAddToNavTree_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool visible = false;
		bool enabled = true;
		try
		{
			TableTitleCell uIRenderCell = Title.GetUIRenderCell(_grid.Row, _grid.Col);
			if (!string.IsNullOrWhiteSpace(uIRenderCell.ComboList))
			{
				visible = true;
				if (uIRenderCell.CellId != null && Title.NavTreeCellIdList != null && Title.NavTreeCellIdList.Contains(uIRenderCell.CellId))
				{
					enabled = false;
				}
			}
		}
		catch (Exception)
		{
		}
		e.Visible = visible;
		e.Enabled = enabled;
	}

	private void CmdAddToNavTree_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (Title.NavTreeCellIdList == null)
			{
				Title.NavTreeCellIdList = new List<string>();
			}
			TableTitleCell uIRenderCell = Title.GetUIRenderCell(_grid.Row, _grid.Col);
			if (string.IsNullOrWhiteSpace(uIRenderCell.CellId))
			{
				uIRenderCell.CellId = Guid.NewGuid().ToString("D");
			}
			if (Title.NavTreeCellIdList.Contains(uIRenderCell.CellId))
			{
				return;
			}
			if (_owner.TableNavGrid.Table != Title.Table)
			{
				Title.NavTreeCellIdList.Add(uIRenderCell.CellId);
			}
			else
			{
				TableTitleCell navTreeTopestLevelWhichRelateTargetCell = _owner.TableNavGrid.GetNavTreeTopestLevelWhichRelateTargetCell(uIRenderCell);
				if (navTreeTopestLevelWhichRelateTargetCell == null)
				{
					Title.NavTreeCellIdList.Add(uIRenderCell.CellId);
				}
				else
				{
					int num = -1;
					for (int i = 0; i < Title.NavTreeCellIdList.Count; i++)
					{
						if (Title.NavTreeCellIdList[i] == navTreeTopestLevelWhichRelateTargetCell.CellId)
						{
							num = i;
							break;
						}
					}
					if (num == -1)
					{
						Title.NavTreeCellIdList.Add(uIRenderCell.CellId);
					}
					else
					{
						Title.NavTreeCellIdList.Insert(num, uIRenderCell.CellId);
					}
				}
			}
			Title.Table.TagTitleDirty();
			Program.MainForm.SuspendNavPanelDrawing();
			Program.MainForm.SuspendNavPanelVisible();
			try
			{
				if (_owner.IsNeedShowTableNavTree())
				{
					_owner.ReBuildNavTree();
					_owner.BindNavTreeViewToNavigationPanel();
				}
			}
			finally
			{
				Program.MainForm.ResumeNavPanelVisible();
				Program.MainForm.ResumeNavPanelDrawing();
			}
		}
		catch (Exception)
		{
		}
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
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
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
				Title.Table.TagTitleDirty();
			}
		}
	}

	public void SetComboListDialog()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		_preserveAuxEditSelection.Clear();
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				_preserveAuxEditSelection.Add(GetCell(i, j));
			}
		}
		TableTitleCell tableTitleCell = _preserveAuxEditSelection.First();
		AuxEditor.New();
		AuxEditor.View.AllowFreeInput = tableTitleCell.IgnoreComboList;
		AuxEditor.View.AllowMultiSelect = tableTitleCell.MultiComboList;
		AuxEditor.View.Value = "";
		if (!string.IsNullOrWhiteSpace(tableTitleCell.ComboList))
		{
			try
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(tableTitleCell.ComboList);
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Title.Table.Project);
				AuxEditor.View.Value = formulaEvaluator.GetDisplayString(resolver, Title.Table);
			}
			catch (FormulaSyntaxException)
			{
				AuxEditor.View.Value = tableTitleCell.ComboList;
			}
		}
		_owner.FormulaEditor.Context.Kind = FormulaContextKind.TitleAuxEdit;
		_owner.FormulaEditor.Context.Table = Title.Table;
		_owner.FormulaEditor.Context.TitleOrFoot = tableTitleCell;
		_owner.FormulaEditor.View.Enabled = false;
		LeaveEdit();
		_grid.Select(selection);
		_lastMouseUpCell = null;
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
				List<string> list = null;
				foreach (TableTitleCell item in _preserveAuxEditSelection)
				{
					item.IgnoreComboList = AuxEditor.View.AllowFreeInput;
					item.MultiComboList = AuxEditor.View.AllowMultiSelect;
					item.ComboList = AuxEditor.View.Value;
					if (string.IsNullOrWhiteSpace(item.ComboList) && !string.IsNullOrEmpty(item.CellId))
					{
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(item.CellId);
					}
				}
				Populate();
				Title.Table.TagTitleDirty();
				if (list != null && Title.NavTreeCellIdList != null && Title.NavTreeCellIdList.Count > 0)
				{
					for (int i = 0; i < list.Count; i++)
					{
						Title.NavTreeCellIdList.Remove(list[i]);
					}
				}
				_owner.ReBuildNavTree();
			}
			_grid.Select();
			SetFormulaContext();
		}
		finally
		{
			_grid.ResumeDrawing();
			_owner.pnlGrid.ResumeDrawing();
			Program.MainForm.ResumeNavPanelVisible();
			Program.MainForm.ResumeNavPanelDrawing();
		}
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
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
	}

	public void SetAlign(CellTextAlign align)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				GetCell(i, j).Align = align;
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
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
		Title.Table.TagTitleDirty();
	}

	public void IncreaseRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			if (selection.TopRow == 0)
			{
				Title.TitleHeight += 5;
			}
			for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
			{
				Title.Rows[i - 1].Height += 5;
			}
			Populate();
			_owner.DoLayout();
			Title.Table.TagTitleDirty();
		}
	}

	public void DecreaseRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			if (selection.TopRow == 0)
			{
				Title.TitleHeight -= 5;
			}
			for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
			{
				Title.Rows[i - 1].Height -= 5;
			}
			Populate();
			_owner.DoLayout();
			Title.Table.TagTitleDirty();
		}
	}

	public void UnifyRowHeight()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			int num = selection.BottomRow - selection.TopRow + 1;
			int num2 = Title.Rows.Skip(selection.TopRow - 1).Take(num).Sum((TableTitleRow r) => r.Height);
			int height = num2 / num;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				Title.Rows[i - 1].Height = height;
			}
			Populate();
			Title.Table.TagTitleDirty();
		}
	}

	public void UnifyColumnWidth()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			int count = selection.RightCol - selection.LeftCol + 1;
			Title.UnifyColumnWidth(selection.LeftCol, count);
			AdjustSize();
			Title.Table.TagTitleDirty();
		}
	}

	public void IncreaseColumnWidth()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			Title.IncreaseColumnWidth(selection.LeftCol, selection.RightCol - selection.LeftCol + 1, 5, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
			_owner.pnlGrid.SuspendDrawing();
			Populate();
			_owner.PopulateColumns();
			_owner.pnlGrid.ResumeDrawing();
			Title.Table.TagTitleDirty();
		}
	}

	public void DecreaseColumnWidth()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsValid)
		{
			Title.DecreaseColumnWidth(selection.LeftCol, selection.RightCol - selection.LeftCol + 1, 5, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
			_owner.pnlGrid.SuspendDrawing();
			Populate();
			_owner.PopulateColumns();
			_owner.pnlGrid.ResumeDrawing();
			Title.Table.TagTitleDirty();
		}
	}

	public void Populate()
	{
		_ttpComment.Hide();
		_grid.BeginUpdate();
		_grid.AllowResizing = (_owner.HasSchemaPermission() ? AllowResizingEnum.Rows : AllowResizingEnum.None);
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		_grid.Rows.Count = 1 + Title.Rows.Count;
		_grid.Rows[0].Height = Title.TitleHeight;
		PopulateCell(0, 0, Title.TitleCell);
		_grid.GetCellRange(0, 0).StyleNew.Border.Direction = BorderDirEnum.Horizontal;
		_grid.Cols.Count = Title.Columns.Count;
		PopulateMerge();
		for (int i = 0; i < Title.Rows.Count; i++)
		{
			TableTitleRow tableTitleRow = Title.Rows[i];
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i + 1];
			row.Height = tableTitleRow.Height;
			for (int j = 0; j < tableTitleRow.Cells.Count; j++)
			{
				PopulateCell(row.Index, j, tableTitleRow.Cells[j]);
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

	public void PopulateMerge()
	{
		_grid.MergedRanges.Clear();
		_grid.BodyAddMergedRange(0, 0, 0, Title.Columns.Count - 1);
		foreach (TicketMerge merge in Title.Merges)
		{
			_grid.BodyAddMergedRange(merge.TopRow + 1, merge.LeftColumn, merge.BottomRow + 1, merge.RightColumn);
			for (int i = merge.TopRow + 1; i <= merge.BottomRow + 1; i++)
			{
				for (int j = merge.LeftColumn; j <= merge.RightColumn; j++)
				{
					_grid.BodyGetCell(i, j).StyleNew.DataType = null;
				}
			}
		}
	}

	public void MergeRowCells(int rowIndex, int leftCol, int rightCol)
	{
		Title.Merges.RemoveAll((TicketMerge m) => m.IntersectsWith(rowIndex, leftCol, rowIndex, rightCol));
		Title.Merges.Add(new TicketMerge
		{
			TopRow = rowIndex,
			LeftColumn = leftCol,
			BottomRow = rowIndex,
			RightColumn = rightCol
		});
		Title.Rows[rowIndex].Cells[leftCol].Value = GetValue();
		for (int i = leftCol + 1; i <= rightCol; i++)
		{
			Title.Rows[rowIndex].Cells[i].Value = "";
		}
		object GetValue()
		{
			for (int j = leftCol; j <= rightCol; j++)
			{
				if (!"".Equals(Title.Rows[rowIndex].Cells[j].Value))
				{
					return Title.Rows[rowIndex].Cells[j].Value;
				}
			}
			return "";
		}
	}

	public void MergeCells()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		int topRow = Math.Max(bodySelection.TopRow, 1) - 1;
		int bottomRow = Math.Max(bodySelection.BottomRow, 1) - 1;
		int leftCol = bodySelection.LeftCol;
		int rightCol = bodySelection.RightCol;
		Title.Merges.RemoveAll((TicketMerge m) => m.IntersectsWith(topRow, leftCol, bottomRow, rightCol));
		Title.Merges.Add(new TicketMerge
		{
			TopRow = topRow,
			LeftColumn = leftCol,
			BottomRow = bottomRow,
			RightColumn = rightCol
		});
		Title.Rows[topRow].Cells[leftCol].Value = GetValue();
		for (int i = topRow; i <= bottomRow; i++)
		{
			for (int j = leftCol; j <= rightCol; j++)
			{
				if (i != topRow || j != leftCol)
				{
					Title.Rows[i].Cells[j].Value = "";
				}
			}
		}
		Populate();
		object GetValue()
		{
			for (int k = topRow; k <= bottomRow; k++)
			{
				for (int l = leftCol; l <= rightCol; l++)
				{
					if (!"".Equals(Title.Rows[k].Cells[l].Value))
					{
						return Title.Rows[k].Cells[l].Value;
					}
				}
			}
			return "";
		}
	}

	public void SplitCells()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int j = bodySelection.TopRow; j <= bodySelection.BottomRow; j++)
		{
			for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
			{
				try
				{
					int rowIndex = Math.Max(j, 1) - 1;
					Title.Merges.RemoveAll((TicketMerge m) => m.TopRow == rowIndex && m.LeftColumn == i);
				}
				catch (Exception exception)
				{
					exception.Log();
				}
			}
		}
		PopulateMerge();
	}

	public void EnterEdit()
	{
		_owner._grid.HighLight = HighLightEnum.WithFocus;
		IsEditing = true;
		Program.MainForm.SwitchStateTo(MainFormView.EditingTitle);
		_grid.PreviewKeyDown += _grid_PreviewKeyDown;
	}

	public void LeaveEdit()
	{
		if (IsEditing)
		{
			Program.MainForm.SuspendNavPanelVisible();
			try
			{
				IsEditing = false;
				_grid.PreviewKeyDown -= _grid_PreviewKeyDown;
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

	public void OnFormulaEditorBeganEditing()
	{
		if (IsEditing)
		{
			Program.MainForm.SuspendNavPanelVisible();
			try
			{
				IsEditing = false;
				_grid.PreviewKeyDown -= _grid_PreviewKeyDown;
				Populate();
				_lastMouseUpCell = null;
				Program.MainForm.SwitchStateTo(MainFormView.EditingFormula);
				AppCommands.Information.HideInformation();
			}
			finally
			{
				Program.MainForm.ResumeNavPanelVisible();
			}
		}
	}

	public void MakerSign()
	{
		string value = Program.MainForm.CurrentProject.DataReferenceManager.ReplaceString(UserSet.Config.SignatureStyle.SignatureFormat, new DataReferenceEvaluationContext
		{
			CurrentTreeNode = _owner.Table.TreeNode,
			Project = _owner.Table.Project
		});
		string signatureRow = UserSet.Config.SignatureStyle.SignatureRow;
		SignAlign signatureAlign = UserSet.Config.SignatureStyle.SignatureAlign;
		TableTitleRow tableTitleRow = Title.Rows[int.Parse(signatureRow) - 1];
		switch (signatureAlign)
		{
		case SignAlign.Left:
			tableTitleRow.Cells[0].Value = value;
			break;
		case SignAlign.Center:
			tableTitleRow.Cells[Math.Min(1, Title.Columns.Count - 1)].Value = value;
			break;
		case SignAlign.Right:
			tableTitleRow.Cells[Math.Min(2, Title.Columns.Count - 1)].Value = value;
			break;
		}
		_owner.Table.TagTitleDirty();
		Populate();
	}

	public void ShowCommentTooltip(int row, int col, bool forceShow)
	{
		_ttpComment.LinkClicked -= _ttpComment_LinkClicked;
		_ttpComment.Hide();
		if (!forceShow && !Program.MainForm.ShowHelperTooltip)
		{
			return;
		}
		TableTitleCell cell = GetCell(row, col);
		string comment = cell.Comment;
		XElement xBody = new XElement("div");
		bool flag = !string.IsNullOrWhiteSpace(comment);
		if (flag)
		{
			DataReferenceEvaluationContext drec = new DataReferenceEvaluationContext
			{
				CurrentTreeNode = Title.Table.TreeNode,
				Project = Title.Table.Project
			};
			xBody.Add(new XElement("b", "填表提示"));
			xBody.Add(from s in comment.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None)
				select new XElement("p", new XAttribute("style", "color:red"), Title.Table.Project.DataReferenceManager.ReplaceString(s, drec)));
			xBody.Add(new XElement("hr"));
		}
		Dictionary<string, object> linkDic = new Dictionary<string, object>();
		TableValidationInfo value;
		bool flag2 = Program.MainForm.TableValidationResults.TryGetValue(Title.Table.TreeNode, out value);
		FormulaReferenceModelResolver resolver;
		int i;
		if (flag2)
		{
			IEnumerable<Tuple<Auditai.Model.Table, int, int, ValidationResult>> enumerable = value.Titles.Cast<Tuple<Auditai.Model.Table, int, int, ValidationResult>>().Where((Tuple<Auditai.Model.Table, int, int, ValidationResult> t) => t.Item2 == row + 1 && t.Item3 == col + 1);
			flag2 = enumerable.Any();
			if (flag2)
			{
				xBody.Add(new XElement("b", "校验公式"));
				try
				{
					resolver = new FormulaReferenceModelResolver(Title.Table.Project);
					i = 0;
					foreach (Tuple<Auditai.Model.Table, int, int, ValidationResult> item in enumerable)
					{
						AddValidationResult(item.Item4, "c");
						i++;
					}
				}
				catch
				{
					xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "生成校验公式提示时发生错误，请尝试重新校验。"));
					xBody.Add(new XElement("hr"));
				}
			}
		}
		if (flag || flag2)
		{
			xBody.LastNode.Remove();
			XElement xElement = xBody.Element("b");
			xElement.Remove();
			_ttpComment.SetText(xElement.Value, xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(linkDic);
			_ttpComment.LinkClicked += _ttpComment_LinkClicked;
			Rectangle cellRect = _grid.GetCellRect(row, col);
			_ttpComment.Show(_grid, new Point(cellRect.Left + cellRect.Width / 2, cellRect.Top + cellRect.Height / 2));
		}
		void AddValidationResult(ValidationResult vf, string anchorPrefix)
		{
			xBody.Add(new XElement("p", "公式说明：", vf.Source.Note));
			XElement xElement2 = new XElement("p", "校验等式：");
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(vf.Source.LeftExpr);
			Tuple<List<TooltipListener.FormulaTooltipSegment>, string> formulaTooltipSegments = formulaEvaluator.GetFormulaTooltipSegments(resolver, null);
			foreach (TooltipListener.FormulaTooltipSegment item2 in formulaTooltipSegments.Item1)
			{
				xElement2.Add(item2.PreText);
				string text = $"l{anchorPrefix}{i}{item2.AnchorNumber}";
				xElement2.Add(new XElement("a", new XAttribute("href", text), item2.Display));
				linkDic.Add(text, item2.Ref);
			}
			xElement2.Add(formulaTooltipSegments.Item2);
			xElement2.Add(vf.Source.Operator.Display);
			formulaEvaluator = new FormulaEvaluator(vf.Source.RightExpr);
			formulaTooltipSegments = formulaEvaluator.GetFormulaTooltipSegments(resolver, null);
			foreach (TooltipListener.FormulaTooltipSegment item3 in formulaTooltipSegments.Item1)
			{
				xElement2.Add(item3.PreText);
				string text2 = $"r{anchorPrefix}{i}{item3.AnchorNumber}";
				xElement2.Add(new XElement("a", new XAttribute("href", text2), item3.Display));
				linkDic.Add(text2, item3.Ref);
			}
			xElement2.Add(formulaTooltipSegments.Item2);
			xBody.Add(xElement2);
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

	public void CheckerSign()
	{
		string value = Program.MainForm.CurrentProject.DataReferenceManager.ReplaceString(UserSet.Config.SignatureStyle.ReviewSignFormat, new DataReferenceEvaluationContext
		{
			CurrentTreeNode = _owner.Table.TreeNode,
			Project = _owner.Table.Project
		});
		string reviewSignRow = UserSet.Config.SignatureStyle.ReviewSignRow;
		SignAlign reviewSignAlign = UserSet.Config.SignatureStyle.ReviewSignAlign;
		TableTitleRow tableTitleRow = Title.Rows[int.Parse(reviewSignRow) - 1];
		switch (reviewSignAlign)
		{
		case SignAlign.Left:
			tableTitleRow.Cells[0].Value = value;
			break;
		case SignAlign.Center:
			tableTitleRow.Cells[Math.Min(1, Title.Columns.Count - 1)].Value = value;
			break;
		case SignAlign.Right:
			tableTitleRow.Cells[Math.Min(2, Title.Columns.Count - 1)].Value = value;
			break;
		}
		_owner.Table.TagTitleDirty();
		Populate();
	}

	public void AdjustSize()
	{
		if (Program.MainForm != null)
		{
			TableEditor tableEditor = Program.MainForm.TableEditor;
			int gridWidth = tableEditor.GetGridWidth();
			gridWidth = Math.Min(gridWidth, tableEditor.GetPanelWidth());
			Title.Resize(gridWidth);
			_grid.Width = gridWidth;
			for (int i = 0; i < Title.Columns.Count; i++)
			{
				_grid.Cols[i].Width = Title.Columns[i].WidthDisplay;
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
		Title.Table.TagTitleDirty();
	}

	public void SetDataFormatNumeric(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft)
				{
					DecimalLength = 2
				};
				cell.Value = Auditai.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(double));
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
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
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft);
				cell.Value = Auditai.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(DateTime));
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
	}

	public void SetDataFormatDateYearMonth(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft);
				cell.Value = Auditai.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(DateYearMonth));
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
	}

	public void SetDataFormatTime(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft);
				cell.Value = Auditai.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(TimeSpan));
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
	}

	public void SetDataFormatBoolean(DataFormatType dft)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				cell.DataFormat = new DataFormat(dft)
				{
					DecimalLength = 2
				};
				cell.Value = Auditai.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(bool));
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
	}

	public void SetZeroFormat(ZeroFormat zf)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				DataFormat dataFormat = cell.DataFormat;
				dataFormat.ZeroFormat = zf;
				cell.DataFormat = dataFormat;
				cell.Value = Auditai.Model.Cell.ChangeDataTypeImpl(cell.Value, typeof(double));
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
	}

	public void MorePrecision()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
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
		Title.Table.TagTitleDirty();
	}

	public void LessPrecision()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		for (int i = Math.Max(1, selection.TopRow); i <= selection.BottomRow; i++)
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
		Title.Table.TagTitleDirty();
	}

	public void SetTheme()
	{
		Theme.SetCurrentObject(ListDropDown.DropDown);
		_grid.GetCellRange(0, 0).StyleNew.Border.Color = _grid.Styles.Normal.Border.Color;
		_grid.Styles.Normal.BackColor = Color.Transparent;
		_penBorder.Color = _grid.Styles.Normal.Border.Color;
	}

	public void AttachTooltip()
	{
		TipInfo tip = TipInfo.Parse(TipResource.主窗体内容区域_主窗体云表格区域_表格标题);
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

	public void SetRowHeight()
	{
		int num = ((_grid.Selection.TopRow != 0) ? Title.Rows[_grid.Selection.TopRow - 1].Height : Title.TitleHeight);
		decimal? num2 = InputForm.Numeric("设置行高", "请输入行高：", num);
		if (!num2.HasValue)
		{
			return;
		}
		int num3 = (int)num2.Value;
		int topRow = _grid.Selection.TopRow;
		int bottomRow = _grid.Selection.BottomRow;
		for (int i = topRow; i <= bottomRow; i++)
		{
			if (i == 0)
			{
				Title.TitleHeight = num3;
			}
			else
			{
				Title.Rows[i - 1].Height = num3;
			}
		}
		Populate();
		_owner.DoLayout();
	}

	public void Select(int row, int col)
	{
		_grid.Select(row, col);
		SetFormulaContext();
	}

	private void CmdHeight_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdHeight.Text = "设置行高...";
	}

	private void CmdHeight_Click(object sender, ClickEventArgs e)
	{
		SetRowHeight();
	}

	private void CmdInsertRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = _grid.BodySelection.TopRow >= 1;
		}
	}

	private void CmdInsertRow_Click(object sender, ClickEventArgs e)
	{
		try
		{
			Title.InsertRow(_grid.BodySelection.TopRow - 1, useNextRowStyle: true);
			Title.Table.TagTitleDirty();
			Populate();
			_owner.DoLayout();
		}
		catch (InvalidOperationException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private void CmdRemoveRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRemoveRow.Text = "删除副标题行";
		cmdRemoveRow.Image = ContextResources.ctxDeleteRow;
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (hitTestInfo.Row == 0)
		{
			e.Visible = false;
		}
		else if (hitTestInfo.Row > 0)
		{
			e.Visible = true;
			(sender as C1Command).UserData = hitTestInfo.Row;
		}
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
	}

	private void CmdRemoveRow_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (bodySelection.BottomRow > 0)
		{
			int num = Math.Max(0, bodySelection.TopRow - 1);
			int num2 = bodySelection.BottomRow - num;
			List<TableTitleCell> rowsCell = GetRowsCell(num, num2);
			Title.RemoveRow(num, num2);
			Title.Table.TagTitleDirty();
			Populate();
			RebuildNavTreeAfterCellsRemoved(rowsCell);
			_owner.DoLayout();
		}
	}

	private void CmdRemoveColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = Title.Columns.Count > 1;
		}
	}

	private void CmdRemoveColumn_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		int leftCol = selection.LeftCol;
		if (leftCol >= 0 && leftCol < Title.Columns.Count)
		{
			List<TableTitleCell> columnsCell = GetColumnsCell(leftCol, selection.RightCol - leftCol + 1);
			Title.RemoveColumn(leftCol, selection.RightCol - leftCol + 1, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
			_owner.pnlGrid.SuspendDrawing();
			Populate();
			_owner.PopulateColumns();
			_owner.pnlGrid.ResumeDrawing();
			RebuildNavTreeAfterCellsRemoved(columnsCell);
		}
	}

	private void CmdInsertColumn_Click(object sender, ClickEventArgs e)
	{
		Title.InsertColumn(_grid.Selection.LeftCol, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
		_owner.pnlGrid.SuspendDrawing();
		Populate();
		_owner.PopulateColumns();
		_owner.pnlGrid.ResumeDrawing();
		Title.Table.TagTitleDirty();
	}

	private void CmdAppendColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void CmdAppendColumn_Click(object sender, ClickEventArgs e)
	{
		Title.InsertColumn(Title.Columns.Count, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
		_owner.pnlGrid.SuspendDrawing();
		Populate();
		_owner.PopulateColumns();
		_owner.pnlGrid.ResumeDrawing();
		Title.Table.TagTitleDirty();
	}

	private void CmdColumnSameWidth_Click(object sender, ClickEventArgs e)
	{
		for (int i = 0; i < Title.Columns.Count; i++)
		{
			Title.Columns[i].Width = 1f;
		}
		Title.Table.TagTitleDirty();
		Populate();
	}

	private void CmdSplitCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			cmdSplitCells.Visible = false;
		}
		else
		{
			cmdSplitCells.Visible = !_grid.Selection.IsSingleCell;
		}
	}

	private void CmdSplitCells_Click(object sender, ClickEventArgs e)
	{
		SplitCells();
	}

	private void CmdMergeCells_Click(object sender, ClickEventArgs e)
	{
		MergeCells();
	}

	private void CmdMergeCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = !_grid.Selection.IsSingleCell;
		}
	}

	private void CmdMergeEveryHorizontalCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = !_grid.Selection.IsSingleCell;
		}
	}

	private void CmdMergeEveryHorizontalCells_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		int num = Math.Max(selection.TopRow, 1) - 1;
		int num2 = Math.Max(selection.BottomRow, 1) - 1;
		for (int i = num; i <= num2; i++)
		{
			MergeRowCells(i, selection.LeftCol, selection.RightCol);
		}
		Populate();
	}

	private void CmdAlign_Popup(object sender, EventArgs e)
	{
		foreach (C1CommandLink commandLink in cmdAlign.CommandLinks)
		{
			commandLink.Command.Checked = false;
		}
		(GetCell(_grid.BodyRow, _grid.BodyCol).Align switch
		{
			CellTextAlign.TopLeft => cmdAlignTopLeft, 
			CellTextAlign.TopCenter => cmdAlignTopCenter, 
			CellTextAlign.TopRight => cmdAlignTopRight, 
			CellTextAlign.MiddleLeft => cmdAlignMiddleLeft, 
			CellTextAlign.MiddleCenter => cmdAlignMiddleCenter, 
			CellTextAlign.MiddleRight => cmdAlignMiddleRight, 
			CellTextAlign.BottomLeft => cmdAlignBottomLeft, 
			CellTextAlign.BottomCenter => cmdAlignBottomCenter, 
			CellTextAlign.BottomRight => cmdAlignBottomRight, 
			_ => cmdAlignTopLeft, 
		}).Checked = true;
	}

	private void CmdAlignBottomRight_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.BottomRight);
	}

	private void CmdAlignBottomCenter_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.BottomCenter);
	}

	private void CmdAlignBottomLeft_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.BottomLeft);
	}

	private void CmdAlignMiddleRight_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.MiddleRight);
	}

	private void CmdAlignMiddleCenter_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.MiddleCenter);
	}

	private void CmdAlignMiddleLeft_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.MiddleLeft);
	}

	private void CmdAlignTopRight_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.TopRight);
	}

	private void CmdAlignTopCenter_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.TopCenter);
	}

	private void CmdAlignTopLeft_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.TopLeft);
	}

	private void CmdRemoveFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void CmdInsertColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void CmdRemoveFormula_Click(object sender, ClickEventArgs e)
	{
		RemoveFormula();
	}

	private void CmdAuxEdit_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void CmdAuxEdit_Click(object sender, ClickEventArgs e)
	{
		SetComboListDialog();
	}

	private void CmdEditComment_Click(object sender, ClickEventArgs e)
	{
		SetEditCommentDialog();
	}

	private void CmdAddRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
			return;
		}
		cmdAppendRow.Text = "新增副标题行";
		cmdAppendRow.Image = ContextResources.ctxAppendRow;
		e.Enabled = Title.CanAddRow;
		e.Visible = true;
	}

	private void CmdAddRow_Click(object sender, ClickEventArgs e)
	{
		try
		{
			Title.AppendRow(useNextRowStyle: true);
			Title.Table.TagTitleDirty();
			Populate();
			_owner.DoLayout();
		}
		catch (InvalidOperationException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private void CmdCheckerSign_Click(object sender, ClickEventArgs e)
	{
		CheckerSign();
	}

	private void CmdCheckerSign_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCheckerSign.Text = "复核签名";
		if (_grid.Row - 1 == int.Parse(UserSet.Config.SignatureStyle.ReviewSignRow) && UserSet.Config.SignatureStyle.ReviewSignAlign == (SignAlign)_grid.Col)
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdMakerSign_Click(object sender, ClickEventArgs e)
	{
		MakerSign();
	}

	private void CmdMakerSign_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdMakerSign.Text = "编制签名";
		if (_grid.Row - 1 == int.Parse(UserSet.Config.SignatureStyle.SignatureRow) && UserSet.Config.SignatureStyle.SignatureAlign == (SignAlign)_grid.Col)
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdDataTypeString_Click(object sender, ClickEventArgs e)
	{
		SetDataFormatText();
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (!IsEditingFormula() && !_owner._isFormatBrushing && !_owner.AuxEditor.IsEditing && !_owner.LedgerCollectFormulaEditor.IsEditing && !AuxEditor.IsEditing && !_owner.FootEditor.AuxEditor.IsEditing && _owner.Table != null && !_gridResizingManager.IsResizing && e.Button == MouseButtons.Right && _grid.HitTest(e.Location).Type != 0 && _owner.HasSchemaPermission())
		{
			ctxTitle.ShowContextMenu(_grid, e.Location);
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		_owner.FormulaEditor.LastClickedComponent = this;
		MouseDown_Right(e);
	}

	private void MouseDown_Right(MouseEventArgs e)
	{
		if (IsEditing && e.Button == MouseButtons.Right)
		{
			HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
			if (hitTestInfo.Type == HitTestTypeEnum.Cell && !_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				_grid.Select(hitTestInfo.Row, hitTestInfo.Column);
			}
		}
	}

	private void _grid_Enter(object sender, EventArgs e)
	{
		if (IsEditingFormula() || _owner._isFormatBrushing || _owner.AuxEditor.IsEditing || _owner.LedgerCollectFormulaEditor.IsEditing || AuxEditor.IsEditing || _owner.FootEditor.AuxEditor.IsEditing || _owner.Table == null || _gridResizingManager.IsResizing || IsEditing)
		{
			return;
		}
		Program.MainForm.SuspendNavPanelVisible();
		try
		{
			if (_owner.FootEditor.IsEditing)
			{
				_owner.FootEditor.LeaveEdit();
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

	private void _grid_MouseUp(object sender, MouseEventArgs e)
	{
		_grid_MouseUp_Formula(e);
	}

	private void _grid_SelChange(object sender, EventArgs e)
	{
		SetFormulaContext();
	}

	public void UpdateCellValue(TableTitleCell cell, object value)
	{
		if (value == null)
		{
			value = string.Empty;
		}
		object obj = Auditai.Model.Cell.ChangeDataTypeImpl(value, cell.DataFormat.GetDataType());
		if (cell.Value != obj)
		{
			cell.Value = obj;
			Title.Table.TagTitleDirty();
		}
	}

	private bool IsNeedRebuildNavTreeOnCellValueChanged(TableTitleCell titleCell)
	{
		if (!_owner.Table.Ticket.IsEmpty())
		{
			return false;
		}
		if (_owner.Table.Title.NavTreeCellIdList == null || _owner.Table.Title.NavTreeCellIdList.Count == 0)
		{
			return false;
		}
		if (!_owner.TableNavGrid.IsNavTreeRelatedTitleCell(titleCell))
		{
			return false;
		}
		return true;
	}

	private void _grid_AfterEdit(object sender, RowColEventArgs e)
	{
		object value = _grid[e.Row, e.Col] ?? "";
		TableTitleCell cell = GetCell(e.Row, e.Col);
		cell.Value = value;
		Title.Table.TagTitleDirty();
		Program.MainForm.SuspendNavPanelVisible();
		bool flag = IsNeedRebuildNavTreeOnCellValueChanged(cell);
		_owner.CalcCurrentTable();
		if (flag)
		{
			_owner.ReBuildNavTree();
		}
		if (!_owner.Table.Ticket.IsEmpty() || (_owner.IsNeedShowTableNavTree() && _owner.IsExistValidNavTreeCell()))
		{
			Program.MainForm.ShowNavigationPanel();
		}
		else
		{
			Program.MainForm.HideNavigationPanel();
		}
		Program.MainForm.ResumeNavPanelVisible();
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
			if (IsEditingFormula() || AuxEditor.IsEditing)
			{
				FormulaContext context = Program.MainForm.FormulaEditor.Context;
				if (context != null && (context.Kind == FormulaContextKind.Title || context.Kind == FormulaContextKind.TitleAuxEdit) && context.Table == Title.Table && e.Row == context.TitleOrFootRow && e.Col == context.TitleOrFootCol)
				{
					e.DrawCell(DrawCellFlags.Border | DrawCellFlags.Content);
				}
			}
			TableTitleCell cell = Title.GetCell(e.Row, e.Col);
			C1.Win.C1FlexGrid.CellStyle styleNew = _grid.GetCellRange(e.Row, e.Col).StyleNew;
			styleNew.WordWrap = true;
			DataFormat dataFormat = cell.DataFormat;
			Type type = dataFormat.GetDataType();
			if (type == typeof(bool))
			{
				type = typeof(string);
			}
			styleNew.DataType = type;
			if (dataFormat.FormatType == DataFormatType.BoolCheckBox)
			{
				e.Text = string.Empty;
				e.Image = (cell.Value.Equals(true) ? _grid.Glyphs[GlyphEnum.Checked] : _grid.Glyphs[GlyphEnum.Unchecked]);
				styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.Align);
			}
			else if (dataFormat.FormatType == DataFormatType.BoolOnOff)
			{
				e.Text = string.Empty;
				e.Image = (cell.Value.Equals(true) ? Auditai.UI.Platform.Properties.Resources.On : Auditai.UI.Platform.Properties.Resources.Off);
				styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.Align);
			}
			else
			{
				e.Text = cell.GetDisplayValue();
			}
			styleNew.BackColor = cell.BackColor;
			if (!string.IsNullOrEmpty(cell.Formula))
			{
				styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
			}
			if (Program.MainForm.ShowHelperTooltip && Program.MainForm.TableValidationResults.TryGetValue(Title.Table.TreeNode, out var value))
			{
				Tuple<Auditai.Model.Table, int, int, ValidationResult> tuple = value.Titles.Cast<Tuple<Auditai.Model.Table, int, int, ValidationResult>>().FirstOrDefault((Tuple<Auditai.Model.Table, int, int, ValidationResult> t) => t.Item2 == e.Row + 1 && t.Item3 == e.Col + 1);
				if (tuple != null)
				{
					styleNew.BackColor = (tuple.Item4.Passed ? UserSet.Config.TableStyle.CheckPassColor : UserSet.Config.TableStyle.CheckFailColor);
				}
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void _grid_BodyBeforeEdit(object sender, RowColEventArgs e)
	{
		if (Program.MainForm.IsInSyncingProject)
		{
			e.Cancel = true;
			return;
		}
		TableTitleCell cell = Title.GetCell(e.Row, e.Col);
		DataFormat dataFormat = cell.DataFormat;
		if (dataFormat.FormatType == DataFormatType.BoolCheckBox || dataFormat.FormatType == DataFormatType.BoolOnOff)
		{
			e.Cancel = true;
			return;
		}
		_grid.BodySetData(e.Row, e.Col, cell.Value);
		C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
		Type dataType = dataFormat.GetDataType();
		if (!string.IsNullOrEmpty(cell.ComboList))
		{
			InputListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = null;
			ListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = null;
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.ComboList);
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
			_dateEdit.EditFormat.CustomFormat = dataFormat.GetFormatString();
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

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		BeforeMouseDown_Checkbox(e);
	}

	private void BeforeMouseDown_Checkbox(BeforeMouseDownEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
		if (hitTestInfo.Row <= 0)
		{
			return;
		}
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type != HitTestTypeEnum.Cell)
		{
			return;
		}
		try
		{
			C1.Win.C1FlexGrid.CellStyle cellStyleDisplay = _grid.GetCellStyleDisplay(hitTestInfo.Row, hitTestInfo.Column);
			Rectangle cellRect = _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
			if (!cellStyleDisplay.GetImageRectangle(cellRect, _grid.Glyphs[GlyphEnum.Checked]).Contains(e.X, e.Y))
			{
				return;
			}
			TableTitleCell cell = GetCell(hitTestInfo.Row, hitTestInfo.Column);
			if (cell.DataFormat.FormatType == DataFormatType.BoolCheckBox || cell.DataFormat.FormatType == DataFormatType.BoolOnOff)
			{
				e.Cancel = true;
				object obj = !cell.Value.Equals(true);
				if (_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
				{
					SetSelectionCheckState(obj);
					return;
				}
				cell.Value = obj;
				Title.Table.TagTitleDirty();
				_grid.Invalidate();
			}
		}
		catch
		{
		}
	}

	private void _grid_PaintBackground(object sender, PaintEventArgs e)
	{
		if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.Picture))
		{
			Point ptScreen = _grid.Parent.PointToScreen(_grid.Location);
			Point point = Program.MainForm.PnlMainRelativePosition(ptScreen);
			Bitmap currentBackgroudImage = Theme.CurrentBackgroudImage;
			e.Graphics.DrawImage(currentBackgroudImage, 0, 0, new Rectangle(point.X, point.Y, currentBackgroudImage.Width - point.X, currentBackgroudImage.Height - point.Y), GraphicsUnit.Pixel);
		}
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		try
		{
			Paint_Border(e);
			Paint_Formula(e);
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void Paint_Border(PaintEventArgs e)
	{
		if (Title.Rows.Count > 0)
		{
			try
			{
				e.Graphics.DrawLine(_penBorder, 0, _grid.Rows[1].Top, 0, _grid.Rows[Title.Rows.Count].Bottom);
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
	}

	private void Paint_Formula(PaintEventArgs e)
	{
		e.Graphics.ResetClip();
		if (!IsEditingFormula() && !AuxEditor.IsEditing && !_owner.AuxEditor.IsEditing && !_owner.FootEditor.AuxEditor.IsEditing && !_owner.LedgerCollectFormulaEditor.IsEditing)
		{
			return;
		}
		FormulaContext context = Program.MainForm.FormulaEditor.Context;
		if ((context.Kind == FormulaContextKind.Title || context.Kind == FormulaContextKind.TitleAuxEdit) && context.Table == Title.Table)
		{
			TableEditor._penFormulaCell.Color = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
			Rectangle rect = _grid.GetCellRect(context.TitleOrFootRow, context.TitleOrFootCol);
			rect.Offset(-1, -1);
			if (rect.Left < 0)
			{
				rect = new Rectangle(0, rect.Y, rect.Width + rect.Left, rect.Height);
			}
			if (rect.Top < 0)
			{
				rect = new Rectangle(rect.Left, 0, rect.Width, rect.Height + rect.Top);
			}
			e.Graphics.DrawRectangle(TableEditor._penFormulaCell, rect);
		}
		if (_owner.FormulaEditor.LastClickedComponent is TableTitleEditor && _lastMouseUpCell.HasValue)
		{
			Rectangle cellRect = _grid.GetCellRect(_lastMouseUpCell.Value.TopRow, _lastMouseUpCell.Value.LeftCol);
			cellRect.Offset(-1, -1);
			cellRect.Inflate(-1, -1);
			TableEditor._penAnimateDash.Color = ((Control.MouseButtons == MouseButtons.Left) ? Theme.SelectedAuditaiTheme.ThemeContext.DarkColor : _owner.FormulaEditor.NextColor);
			e.Graphics.DrawRectangle(TableEditor._penAnimateDash, cellRect);
		}
		if (_owner.FormulaEditor.RefIntervals == null)
		{
			return;
		}
		foreach (FormulaDisplayRef refInterval in _owner.FormulaEditor.RefIntervals)
		{
			if (refInterval.Kind == FormulaDisplayRefKind.Title && refInterval.Table == Title.Table)
			{
				TableEditor._penFormulaRefRect.Color = refInterval.Color;
				Rectangle rect2 = _grid.GetCellRect(refInterval.TitleOrFootRow - 1, refInterval.TitleOrFootCol - 1);
				rect2.Offset(-1, -1);
				if (rect2.Top < 0)
				{
					rect2 = new Rectangle(rect2.Left, 0, rect2.Width, rect2.Height + rect2.Top);
				}
				e.Graphics.DrawRectangle(TableEditor._penFormulaRefRect, rect2);
				TableEditor._brushFormulaRefRect.Color = Color.FromArgb(20, TableEditor._penFormulaRefRect.Color);
				e.Graphics.FillRectangle(TableEditor._brushFormulaRefRect, rect2);
			}
		}
	}

	private void _grid_MouseEnterCell(object sender, RowColEventArgs e)
	{
		if (!IsEditingFormula() && !IsEditing)
		{
			C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(e.Row, e.Col);
			if (mergedRange.IsValid)
			{
				ShowCommentTooltip(mergedRange.TopRow, mergedRange.LeftCol, forceShow: false);
			}
		}
	}

	private void _grid_MouseLeaveCell(object sender, RowColEventArgs e)
	{
		_ttpComment.Hide();
	}

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Delete)
		{
			bool flag = false;
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
					if (string.IsNullOrEmpty(cell.Formula))
					{
						cell.Value = string.Empty;
						if (!flag)
						{
							flag = IsNeedRebuildNavTreeOnCellValueChanged(cell);
						}
					}
				}
			}
			Title.Table.TagTitleDirty();
			Populate();
			if (flag)
			{
				_owner.CalcCurrentTable();
				_owner.ReBuildNavTree();
			}
		}
		else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Down)
		{
			TableTitleCell cell2 = GetCell(_grid.Row, _grid.Col);
			if (string.IsNullOrWhiteSpace(cell2.ComboList))
			{
				return;
			}
			Operand list = _owner.GetList(cell2.ComboList);
			if (!(list is ValueSetOperand valueSetOperand))
			{
				return;
			}
			e.Handled = true;
			List<string> list2 = valueSetOperand.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> tup) => tup.Item2.ToString()).ToList();
			int num = list2.IndexOf(cell2.GetDisplayValue());
			if (num < list2.Count - 1)
			{
				if (num > -1)
				{
					_grid[_grid.Row, _grid.Col] = list2[num + 1];
					_grid_AfterEdit(_grid, new RowColEventArgs(_grid.Row, _grid.Col));
				}
				else if (list2.Count > 0)
				{
					_grid[_grid.Row, _grid.Col] = list2[0];
					_grid_AfterEdit(_grid, new RowColEventArgs(_grid.Row, _grid.Col));
				}
			}
		}
		else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Up)
		{
			TableTitleCell cell3 = GetCell(_grid.Row, _grid.Col);
			if (string.IsNullOrWhiteSpace(cell3.ComboList))
			{
				return;
			}
			Operand list3 = _owner.GetList(cell3.ComboList);
			if (list3 is ValueSetOperand valueSetOperand2)
			{
				e.Handled = true;
				List<string> list4 = valueSetOperand2.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> tup) => tup.Item2.ToString()).ToList();
				int num2 = list4.IndexOf(cell3.GetDisplayValue());
				if (num2 > 0)
				{
					_grid[_grid.Row, _grid.Col] = list4[num2 - 1];
					_grid_AfterEdit(_grid, new RowColEventArgs(_grid.Row, _grid.Col));
				}
			}
		}
		else if (e.KeyCode == Keys.Space)
		{
			TableTitleCell cell4 = GetCell(_grid.Row, _grid.Col);
			bool flag2 = !true.Equals(cell4.Value);
			SetSelectionCheckState(flag2);
		}
	}

	private void _grid_BeforeEdit(object sender, RowColEventArgs e)
	{
		if (_owner._isEditingHeaders)
		{
			return;
		}
		TableTitleCell cell = GetCell(e.Row, e.Col);
		if (!string.IsNullOrEmpty(cell.Formula))
		{
			e.Cancel = true;
		}
		if (!IsEditingFormula())
		{
			try
			{
				AppCommands.TableFont.FontSelector.SelectFontFamily(cell.FontFamily);
			}
			catch (ArgumentException)
			{
			}
			AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(cell.FontSize);
			AppCommands.Bold.IsPressed = cell.Bold;
			AppCommands.Italic.IsPressed = cell.Italic;
		}
	}

	private void _grid_SetupEditor(object sender, RowColEventArgs e)
	{
		TableTitleCell cell = GetCell(e.Row, e.Col);
		Type dataType = cell.DataFormat.GetDataType();
		if (_grid.Editor == ListDropDown.DropDown)
		{
			ListDropDown.DropDown.DataType = typeof(string);
		}
		else if (dataType == typeof(DateTime) && string.IsNullOrEmpty(cell.GetDisplayValue()))
		{
			_dateEdit.Value = DateTime.Now.Date;
		}
		else if (dataType == typeof(TimeSpan))
		{
			_timeEdit.DataType = typeof(object);
			_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
			_timeEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
			DateTime result;
			if (string.IsNullOrEmpty(cell.GetDisplayValue()))
			{
				_timeEdit.Value = DateTime.Now;
			}
			else if (cell.Value is TimeSpan timeSpan)
			{
				_timeEdit.Value = new DateTime(2000, 1, 1, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			}
			else if (DateTime.TryParse(cell.GetDisplayValue(), out result))
			{
				_timeEdit.Value = result;
			}
			else
			{
				_timeEdit.Value = DateTime.Now;
			}
			_timeEdit.DataType = typeof(DateTime);
			TableEditor.ConvertCellTimeDisplayFormatToTimeEditorFormat(cell.DataFormat.GetFormatString(), out var customFormat, out var customEditFormat);
			_timeEdit.CustomFormat = customFormat;
			_timeEdit.EditFormat.CustomFormat = customEditFormat;
		}
		else if (dataType == typeof(string))
		{
			if (_grid.Editor is TextBox textBox && string.IsNullOrEmpty(cell.ComboList))
			{
				textBox.Text = Regex.Replace(cell.GetDisplayValue(), "(?<!\\r)\\n", "\r\n");
			}
		}
		else if (dataType == typeof(DateYearMonth))
		{
			_dateEdit.DataType = typeof(DateTime);
			DateTime dateTime = ((!string.IsNullOrEmpty(cell.Value?.ToString())) ? ((DateYearMonth)cell.Value).Date : DateTime.Now.Date);
			_dateEdit.Value = dateTime;
			_dateEdit.CustomFormat = cell.DataFormat.GetFormatString();
		}
		ListDropDown.SkipTextChanged = false;
		if (_grid.Editor != null)
		{
			_grid.Editor.Top++;
			_grid.Editor.Left++;
			_grid.Editor.Width--;
			_grid.Editor.Height--;
		}
	}

	private void _grid_StartEdit(object sender, RowColEventArgs e)
	{
		TableTitleCell cell = GetCell(e.Row, e.Col);
		if (string.IsNullOrWhiteSpace(cell.ComboList))
		{
			return;
		}
		Operand list = _owner.GetList(cell.ComboList);
		if (_grid.Editor == ListDropDown.DropDown)
		{
			ListDropDown.DropDown.EditorDataType = typeof(string);
			ListDropDown.DropDown.EditorInitValue = null;
			Type dataType = cell.DataFormat.GetDataType();
			if (dataType == typeof(DateTime) || dataType == typeof(DateYearMonth) || dataType == typeof(TimeSpan))
			{
				ListDropDown.DropDown.EditorInitValue = cell.GetDisplayValue();
			}
			ListDropDown.DropDown.Font = new Font(cell.FontFamily, cell.FontSize);
			if (list is TreeListOperand op)
			{
				if (cell.MultiComboList)
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
			else if (list is ValueSetOperand op2)
			{
				if (cell.MultiComboList)
				{
					ListDropDown.ViewKind = DropDownViewKind.SimpleCheckList;
					ListDropDown.SimpleCheckedList.Op = op2;
					ListDropDown.SimpleCheckedList.Populate();
					ListDropDown.SimpleCheckedList.SetInitValue(cell.GetDisplayValue());
				}
				else
				{
					ListDropDown.ViewKind = DropDownViewKind.SimpleList;
					ListDropDown.SimpleList.Op = op2;
					ListDropDown.SimpleList.Populate();
				}
			}
			else if (list is TableListOperand op3)
			{
				if (cell.MultiComboList)
				{
					ListDropDown.ViewKind = DropDownViewKind.TableCheckList;
					ListDropDown.TableCheckedList.Op = op3;
					ListDropDown.TableCheckedList.Populate();
					ListDropDown.TableCheckedList.SetInitValue(cell.GetDisplayValue());
				}
				else
				{
					ListDropDown.ViewKind = DropDownViewKind.TableList;
					ListDropDown.TableList.Op = op3;
					ListDropDown.TableList.Populate();
				}
			}
			else if (list is MultiListOperand op4)
			{
				if (cell.MultiComboList)
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
			Type dataType2 = cell.DataFormat.GetDataType();
			if (dataType2 == typeof(DateTime) || dataType2 == typeof(DateYearMonth) || dataType2 == typeof(TimeSpan))
			{
				InputListDropDown.DropDown.EditorInitValue = cell.GetDisplayValue();
			}
			InputListDropDown.Clear();
			InputListDropDown.CanInputTextbox = cell.DataFormat.IgnoreComboList;
			if (list is InputListOperand op5)
			{
				InputListDropDown.Op = op5;
				InputListDropDown.Populate();
				InputListDropDown.SetInitValue(cell.GetDisplayValue());
			}
			else if (list == ValueSetOperand.Empty)
			{
				InputListDropDown.PopulateError();
			}
		}
	}

	private void _grid_ValidateEdit(object sender, ValidateEditEventArgs e)
	{
		TableTitleCell cell = GetCell(e.Row, e.Col);
		if (cell.DataFormat.GetDataType() == typeof(DateYearMonth) && _dateEdit.Value is DateTime)
		{
			DateTime date = (DateTime)_dateEdit.Value;
			_dateEdit.DataType = typeof(DateYearMonth);
			_dateEdit.Value = new DateYearMonth(date)
			{
				ToStringFormat = cell.DataFormat.GetFormatString()
			};
		}
		else if (cell.DataFormat.GetDataType() == typeof(TimeSpan) && _timeEdit.Value is DateTime dateTime)
		{
			_timeEdit.DataType = typeof(object);
			_timeEdit.Value = new TimeSpan(dateTime.Hour, dateTime.Minute, dateTime.Second);
			_timeEdit.DataType = typeof(TimeSpan);
		}
		if (!string.IsNullOrWhiteSpace(cell.ComboList) && _grid.Editor != InputListDropDown.DropDown && _grid.Editor == ListDropDown.DropDown && !cell.IgnoreComboList && !ListDropDown.Validate())
		{
			_grid.FinishEditing(cancel: true);
		}
		ListDropDown.SkipTextChanged = true;
	}

	private void ValidationEditor_FinishedEditing(object sender, EventArgs e)
	{
		_grid.MouseUp -= _grid_MouseUp_Validation;
		_grid.MouseEnterCell += _grid_MouseEnterCell;
		_grid.MouseClick += _grid_MouseClick;
	}

	private void ValidationEditor_BeganEditing(object sender, EventArgs e)
	{
		_grid.MouseEnterCell -= _grid_MouseEnterCell;
		_grid.MouseClick -= _grid_MouseClick;
		_grid.MouseUp += _grid_MouseUp_Validation;
	}

	private async void _ttpComment_LinkClicked(object sender, object e)
	{
		if (!(e is Auditai.Model.Column column))
		{
			if (!(e is Auditai.Model.Cell cell))
			{
				if (!(e is TreeNodeBase node))
				{
					ConsolidateAttributes consolidateAttributes = e as ConsolidateAttributes;
					if (consolidateAttributes == null)
					{
						return;
					}
					Auditai.Model.Project project = await Program.MainForm.OpenOrSwitchToProject(consolidateAttributes.ProjectId);
					if (project != null)
					{
						TreeTableNode treeTableNode = project.GetAllTableNodes().FirstOrDefault((TreeTableNode t) => t.Id == consolidateAttributes.TableId);
						if (treeTableNode == null)
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "数据源表格不存在");
						}
						else
						{
							Program.MainForm.ProjectHierarchy.FindAndSelectNode(treeTableNode);
						}
					}
				}
				else
				{
					Program.MainForm.ProjectHierarchy.FindAndSelectNode(node);
				}
			}
			else
			{
				Program.MainForm.ProjectHierarchy.FindAndSelectNode(cell.Column.Table.TreeNode);
				_owner.Select(cell.Row.Index, cell.Column.Index);
			}
		}
		else
		{
			Program.MainForm.ProjectHierarchy.FindAndSelectNode(column.Table.TreeNode);
			_owner.SelectColumn(column.Index);
		}
	}

	private void _grid_MouseUp_Validation(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(hitTestInfo.Row, hitTestInfo.Column);
		if (mergedRange.IsValid)
		{
			string text = $"Title({{{Title.Table.GetCanonicalName()}}},{mergedRange.TopRow + 1},{mergedRange.LeftCol + 1})";
			_owner.ValidationEditor.RemoveRefAtPos();
			_owner.ValidationEditor.InsertRefTextAndFocus(text);
		}
	}

	private void _grid_MouseUp_Formula(MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (hitTestInfo.Row < 0 || hitTestInfo.Column < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(hitTestInfo.Row, hitTestInfo.Column);
		if (mergedRange.IsValid)
		{
			_lastMouseUpCell = mergedRange;
			string text = $"Title({{{Title.Table.GetCanonicalName()}}},{mergedRange.TopRow + 1},{mergedRange.LeftCol + 1})";
			if (IsEditingFormula())
			{
				_owner.FormulaEditor.RemoveRefAtPos();
				_owner.FormulaEditor.InsertRefText(text);
			}
			else if (_owner.AuxEditor.IsEditing)
			{
				_owner.AuxEditor.RemoveRefAtPos();
				_owner.AuxEditor.InsertRefTextAndFocus(text);
			}
			else if (AuxEditor.IsEditing)
			{
				AuxEditor.RemoveRefAtPos();
				AuxEditor.InsertRefTextAndFocus(text);
			}
			else if (_owner.FootEditor.AuxEditor.IsEditing)
			{
				_owner.FootEditor.AuxEditor.RemoveRefAtPos();
				_owner.FootEditor.AuxEditor.InsertRefTextAndFocus(text);
			}
			else if (_owner.LedgerCollectFormulaEditor.IsEditing)
			{
				_owner.LedgerCollectFormulaEditor.RemoveRefAtPos();
				_owner.LedgerCollectFormulaEditor.InsertRefTextAndFocus(text);
			}
		}
	}

	private void _grid_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		switch (e.KeyData)
		{
		case Keys.C | Keys.Control:
			_grid.Copy();
			break;
		case Keys.V | Keys.Control:
		{
			bool flag = IsAfterPaseValueNeedRebuldNavTree();
			_grid.Paste();
			Populate();
			if (flag)
			{
				_owner.ReBuildNavTree();
			}
			break;
		}
		case Keys.X | Keys.Control:
			Cut();
			break;
		}
	}

	private bool IsAfterPaseValueNeedRebuldNavTree()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				if (IsNeedRebuildNavTreeOnCellValueChanged(cell))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Cut()
	{
		try
		{
			_grid.Cut();
			bool flag = false;
			bool flag2 = false;
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
				{
					TableTitleCell cell = GetCell(i, j);
					cell.Value = string.Empty;
					flag = true;
					if (!flag2)
					{
						flag2 = IsNeedRebuildNavTreeOnCellValueChanged(cell);
					}
				}
			}
			if (flag)
			{
				Title.Table.TagTitleDirty();
				Populate();
				if (flag2)
				{
					_owner.ReBuildNavTree();
				}
				_owner.CalcCurrentTable();
			}
		}
		catch (Exception ex)
		{
			ex.Log("表格的标题区进行剪切时发生了未预期的异常");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	protected void IncreaseWidth(int value)
	{
		TableEditor tableEditor = Program.MainForm.TableEditor;
		int gridWidth = tableEditor.GetGridWidth();
		int num = Math.Max(gridWidth + value, 1);
		int width = Math.Min(num, tableEditor.GetPanelWidth());
		Title.Resize(width);
		_grid.Width = width;
		for (int i = 0; i < Title.Columns.Count; i++)
		{
			_grid.Cols[i].Width = Title.Columns[i].WidthDisplay;
		}
		_grid.Left = tableEditor._grid.Left;
		_owner.Table.Columns.Resize(num);
		_owner.PopulateColumns();
	}

	private void _gridResizingManager_ResizeColumn(object sender, ResizeEventArgs e)
	{
		Title.Table.TagTitleDirty();
		if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
		{
			_owner.pnlGrid.SuspendDrawing();
			try
			{
				TableEditor tableEditor = Program.MainForm.TableEditor;
				if (tableEditor.GetGridWidth() >= tableEditor.GetPanelWidth())
				{
					int value = e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay;
					Title.ResizeColumn(e.RowCol, 1, e.HeightWidth, _owner.GetPanelWidth(), _owner.Get1stColumnWidth());
					IncreaseWidth(value);
					return;
				}
				if (Title.ResizeColumn(e.RowCol, 1, e.HeightWidth, _owner.GetPanelWidth(), _owner.Get1stColumnWidth()))
				{
					_owner.PopulateColumns();
				}
				AdjustSize();
				return;
			}
			finally
			{
				_owner.pnlGrid.ResumeDrawing();
			}
		}
		if (e.RowCol == _grid.Cols.Count - 1)
		{
			int value2 = e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay;
			_owner.pnlGrid.SuspendDrawing();
			try
			{
				IncreaseWidth(value2);
				return;
			}
			finally
			{
				_owner.pnlGrid.ResumeDrawing();
			}
		}
		_owner.pnlGrid.SuspendDrawing();
		try
		{
			int widthDisplay = _grid.Cols[e.RowCol].WidthDisplay;
			int widthDisplay2 = _grid.Cols[e.RowCol + 1].WidthDisplay;
			int num = e.HeightWidth - _grid.Cols[e.RowCol].WidthDisplay;
			int val = _grid.Cols[e.RowCol].WidthDisplay + num;
			int val2 = ((e.RowCol != 0) ? 1 : 2);
			val = Math.Max(val2, val);
			val = Math.Min(widthDisplay + widthDisplay2 - 1, val);
			int num2 = widthDisplay + widthDisplay2 - val;
			_grid.Cols[e.RowCol].Width = val;
			_grid.Cols[e.RowCol + 1].Width = num2;
			Title.Columns[e.RowCol].Width = val;
			Title.Columns[e.RowCol].WidthDisplay = val;
			Title.Columns[e.RowCol + 1].Width = num2;
			Title.Columns[e.RowCol + 1].WidthDisplay = num2;
		}
		finally
		{
			_owner.pnlGrid.ResumeDrawing();
		}
	}

	private void _gridResizingManager_ResizeRow(object sender, ResizeEventArgs e)
	{
		if (e.RowCol == 0)
		{
			Title.TitleHeight = e.HeightWidth;
		}
		else
		{
			Title.Rows[e.RowCol - 1].Height = e.HeightWidth;
		}
		Title.Table.TagTitleDirty();
		Populate();
		_owner.DoLayout();
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
		styleNew.Font = new Font(cell.FontFamily, cell.FontSize, fontStyle);
		styleNew.ForeColor = cell.ForeColor;
		if (row == 0)
		{
			styleNew.TextAlign = TextAlignEnum.CenterCenter;
		}
		else
		{
			styleNew.TextAlign = C1FlexGridEx.ToTextAlign(cell.Align);
		}
		styleNew.Margins = new System.Drawing.Printing.Margins(cell.Margin, 0, 0, 0);
		styleNew.WordWrap = true;
	}

	private void RemoveFormula()
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
				GetCell(i, j).Formula = string.Empty;
			}
		}
		Populate();
		Title.Table.TagTitleDirty();
		SetFormulaContext();
	}

	private TableTitleCell GetCell(int row, int col)
	{
		return Title.GetCell(row, col);
	}

	private C1CommandMenu AddCommandMenu(string text, Action<DataFormatType> action, params Tuple<string, DataFormatType>[] commands)
	{
		C1CommandMenu c1CommandMenu = new C1CommandMenu
		{
			Text = text
		};
		foreach (Tuple<string, DataFormatType> b in commands)
		{
			C1Command c1Command = new C1Command
			{
				Text = b.Item1
			};
			c1Command.Click += delegate
			{
				action(b.Item2);
			};
			c1CommandMenu.CommandLinks.Add(new C1CommandLink(c1Command));
		}
		cmdDataType.CommandLinks.Add(new C1CommandLink(c1CommandMenu));
		return c1CommandMenu;
	}

	private void AppendCommandMenu(C1CommandMenu menu, Action<DataFormatType> action, params Tuple<string, DataFormatType>[] commands)
	{
		bool flag = true;
		foreach (Tuple<string, DataFormatType> b in commands)
		{
			C1Command c1Command = new C1Command
			{
				Text = b.Item1
			};
			c1Command.Click += delegate
			{
				action(b.Item2);
			};
			menu.CommandLinks.Add(new C1CommandLink(c1Command)
			{
				Delimiter = flag
			});
			flag = false;
		}
	}

	private bool IsEditingFormula()
	{
		return Program.MainForm.FormulaEditor.IsEditing;
	}

	private void SetFormulaContext()
	{
		if (!IsEditing)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(_grid.Selection.TopRow, _grid.Selection.LeftCol);
		if (mergedRange.IsValid)
		{
			TableTitleCell cell = GetCell(mergedRange.TopRow, mergedRange.LeftCol);
			if (!IsEditingFormula())
			{
				FormulaContext context = Program.MainForm.FormulaEditor.Context;
				context.TitleOrFoot = cell;
				context.Kind = FormulaContextKind.Title;
				context.TitleOrFootRow = mergedRange.TopRow;
				context.TitleOrFootCol = mergedRange.LeftCol;
				_owner.FormulaEditor.Populate();
				_owner.FormulaEditor.View.Enabled = _owner.HasSchemaPermission();
			}
		}
	}

	private void SetSelectionCheckState(object newValue)
	{
		bool flag = false;
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			for (int j = _grid.BodyCol; j <= _grid.BodyColSel; j++)
			{
				TableTitleCell cell = GetCell(i, j);
				if (cell.DataFormat.FormatType == DataFormatType.BoolCheckBox || cell.DataFormat.FormatType == DataFormatType.BoolOnOff)
				{
					cell.Value = newValue;
					if (!flag)
					{
						flag = IsNeedRebuildNavTreeOnCellValueChanged(cell);
					}
				}
			}
		}
		Title.Table.TagTitleDirty();
		_grid.Invalidate();
		if (flag)
		{
			_owner.ReBuildNavTree();
		}
	}

	protected List<TableTitleCell> GetRowsCell(int index, int rowCount)
	{
		List<TableTitleCell> list = new List<TableTitleCell>();
		for (int i = 0; i < rowCount; i++)
		{
			list.AddRange(Title.GetRowCells(index + 1 + i));
		}
		return list;
	}

	protected List<TableTitleCell> GetColumnsCell(int colIndex, int colCount)
	{
		List<TableTitleCell> list = new List<TableTitleCell>();
		for (int i = 0; i < colCount; i++)
		{
			list.AddRange(Title.GetColumnCells(colIndex + i));
		}
		return list;
	}

	protected void RebuildNavTreeAfterCellsRemoved(List<TableTitleCell> removedCellList)
	{
		if (removedCellList != null && removedCellList.Count != 0 && Title.NavTreeCellIdList != null && Title.NavTreeCellIdList.Count != 0 && removedCellList.Any((TableTitleCell u) => _owner.TableNavGrid.IsNavTreeRelatedTitleCell(u)))
		{
			HashSet<string> existCellIdHasSet = new HashSet<string>(from u in removedCellList
				where !string.IsNullOrEmpty(u.CellId)
				select u.CellId);
			List<string> list = Title.NavTreeCellIdList.Where((string u) => existCellIdHasSet.Contains(u)).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				Title.NavTreeCellIdList.Remove(list[i]);
			}
			Title.Table.TagTitleDirty();
			_owner.ReBuildNavTree();
		}
	}
}
