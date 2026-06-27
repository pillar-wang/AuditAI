﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using C1.Win.C1Themes;
using Auditai.DTO;
using Auditai.Model;
using Auditai.PlatformResource;
using Auditai.SignalR;
using Auditai.UI.Controls;
using Auditai.UI.Controls.CellCollect;
using Auditai.UI.Controls.CollectCell;
using Auditai.UI.Controls.CollectTable;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Chat;
using Auditai.UI.Platform.Properties;
using Auditai.LocalDataStore;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class TableEditor : ISetTheme
{
	private class ValidateResultEqualityComparer : IEqualityComparer<Tuple<Auditai.Model.Cell, ValidationResult>>
	{
		public bool Equals(Tuple<Auditai.Model.Cell, ValidationResult> x, Tuple<Auditai.Model.Cell, ValidationResult> y)
		{
			if (x != null && y != null && x.Item1 == y.Item1 && x.Item2 != null && y.Item2 != null)
			{
				return x.Item2.Source == y.Item2.Source;
			}
			return false;
		}

		public int GetHashCode(Tuple<Auditai.Model.Cell, ValidationResult> obj)
		{
			return 0;
		}
	}

	private class NodeNumberInfo
	{
		public int Id { get; set; }

		public string Number { get; set; }

		public string Name { get; set; }

		public TreeNodeBase Node { get; set; }
	}

	private const int MAX_ROW_CONVERT_NUM = 10;

	private readonly C1ContextMenu ctxEmpty = new C1ContextMenu();

	private readonly C1ContextMenu ctxRow = new C1ContextMenu();

	private readonly C1ContextMenu ctxColumn = new C1ContextMenu();

	private readonly C1ContextMenu ctxRange = new C1ContextMenu();

	private readonly C1ContextMenu ctxLock = new C1ContextMenu();

	private readonly C1ContextMenu ctxCell = new C1ContextMenu();

	private readonly C1ContextMenu ctxColHeader = new C1ContextMenu();

	private readonly C1ContextMenu ctxHeaderCell = new C1ContextMenu();

	private readonly C1ContextMenu ctxTableHeader = new C1ContextMenu();

	private readonly C1CommandLink lnkAppendRows = new C1CommandLink();

	private readonly C1Command cmdAppendRows = new C1Command();

	private readonly C1CommandLink lnkAppendColumns = new C1CommandLink();

	private readonly C1Command cmdAppendColumns = new C1Command();

	private readonly C1CommandLink lnkInsertRows = new C1CommandLink();

	private readonly C1Command cmdInsertRows = new C1Command();

	private readonly C1CommandLink lnkAppendRows2 = new C1CommandLink();

	private readonly C1CommandLink lnkRemoveRows = new C1CommandLink();

	private readonly C1Command cmdRemoveRows = new C1Command();

	private readonly C1CommandLink lnkMoveUpRows = new C1CommandLink();

	private readonly C1Command cmdMoveUpRows = new C1Command();

	private readonly C1CommandLink lnkMoveDownRows = new C1CommandLink();

	private readonly C1Command cmdMoveDownRows = new C1Command();

	private readonly C1CommandLink lnkSetHeight = new C1CommandLink();

	private readonly C1Command cmdSetHeight = new C1Command();

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

	private readonly C1CommandLink lnkInsertColumns = new C1CommandLink();

	private readonly C1Command cmdInsertColumns = new C1Command();

	private readonly C1CommandLink lnkAppendColumns2 = new C1CommandLink();

	private readonly C1CommandLink lnkRemoveColumns = new C1CommandLink();

	private readonly C1Command cmdRemoveColumns = new C1Command();

	private readonly C1CommandLink lnkSetColumnWidth = new C1CommandLink();

	private readonly C1Command cmdSetColumnWidth = new C1Command();

	private readonly C1CommandLink lnkDataType = new C1CommandLink();

	private readonly C1CommandLink lnkDataType2 = new C1CommandLink();

	private readonly C1CommandLink lnkDataType3 = new C1CommandLink();

	private readonly C1CommandMenu cmdDataType = new C1CommandMenu();

	private readonly C1CommandLink lnkDataTypeString = new C1CommandLink();

	private readonly C1Command cmdDataTypeString = new C1Command();

	private readonly C1CommandLink lnkAlign2 = new C1CommandLink();

	private readonly C1CommandLink lnkSortAscending = new C1CommandLink();

	private readonly C1Command cmdSortAscending = new C1Command();

	private readonly C1CommandLink lnkSortDescending = new C1CommandLink();

	private readonly C1Command cmdSortDescending = new C1Command();

	private readonly C1Command cmdSubtotal = new C1Command();

	private readonly C1CommandLink lnkSubtotal = new C1CommandLink();

	private readonly C1CommandLink lnkCancelSubtotal = new C1CommandLink();

	private readonly C1Command cmdCancelSubtotal = new C1Command();

	private readonly C1CommandLink lnkCopyFill = new C1CommandLink();

	private readonly C1Command cmdCopyFill = new C1Command();

	private readonly C1CommandLink lnkSequenceFill = new C1CommandLink();

	private readonly C1Command cmdSequenceFill = new C1Command();

	private readonly C1Command cmdBlankFill = new C1Command();

	private readonly C1CommandLink lnkBlankFill = new C1CommandLink();

	private readonly C1CommandLink lnkAlign3 = new C1CommandLink();

	private readonly C1CommandLink lnkCut = new C1CommandLink();

	private readonly C1Command cmdCut = new C1Command();

	private readonly C1CommandLink lnkCopy = new C1CommandLink();

	private readonly C1Command cmdCopy = new C1Command();

	private readonly C1CommandLink lnkPaste = new C1CommandLink();

	private readonly C1CommandMenu cmdPaste = new C1CommandMenu();

	private readonly C1CommandLink lnkPasteValue = new C1CommandLink();

	private readonly C1Command cmdPasteValue = new C1Command();

	private readonly C1CommandLink lnkPasteFormat = new C1CommandLink();

	private readonly C1Command cmdPasteFormat = new C1Command();

	private readonly C1CommandLink lnkPasteFormula = new C1CommandLink();

	private readonly C1Command cmdPasteFormula = new C1Command();

	private readonly C1CommandLink lnkEditFormula = new C1CommandLink();

	private readonly C1Command cmdEditFormula = new C1Command();

	private readonly C1CommandLink lnkRemoveFormula = new C1CommandLink();

	private readonly C1Command cmdRemoveFormula = new C1Command();

	private readonly C1CommandLink lnkRemoveFormula2 = new C1CommandLink();

	private readonly C1CommandLink lnkRemoveColumnFormula = new C1CommandLink();

	private readonly C1Command cmdRemoveColumnFormula = new C1Command();

	private readonly C1CommandLink lnkAlign4 = new C1CommandLink();

	private readonly C1CommandLink lnkCut2 = new C1CommandLink();

	private readonly C1CommandLink lnkCopy2 = new C1CommandLink();

	private readonly C1CommandLink lnkPaste2 = new C1CommandLink();

	private readonly C1CommandLink lnkMergeEveryHorizontalCells = new C1CommandLink();

	private readonly C1Command cmdMergeEveryHorizontalCells = new C1Command();

	private readonly C1CommandLink lnkMergeCells = new C1CommandLink();

	private readonly C1Command cmdMergeCells = new C1Command();

	private readonly C1CommandLink lnkUnmergeCells = new C1CommandLink();

	private readonly C1Command cmdUnmergeCells = new C1Command();

	private readonly C1CommandLink lnkLockCells = new C1CommandLink();

	private readonly C1Command cmdLockCells = new C1Command();

	private readonly C1CommandLink lnkLockCells2 = new C1CommandLink();

	private readonly C1CommandLink lnkUnlockCells = new C1CommandLink();

	private readonly C1Command cmdUnlockCells = new C1Command();

	private readonly C1CommandLink lnkUnlockCells2 = new C1CommandLink();

	private readonly C1Command cmdSumColumns = new C1Command();

	private readonly C1CommandLink lnkSumColumns = new C1CommandLink();

	private readonly C1CommandLink lnkSumColumns2 = new C1CommandLink();

	private readonly C1CommandLink lnkSumColumns3 = new C1CommandLink();

	private readonly C1CommandLink lnkCancelSumColumns = new C1CommandLink();

	private readonly C1Command cmdCancelSumColumns = new C1Command();

	private readonly C1CommandLink lnkLockColumns = new C1CommandLink();

	private readonly C1Command cmdLockColumns = new C1Command();

	private readonly C1CommandLink lnkUnlockColumns = new C1CommandLink();

	private readonly C1Command cmdUnlockColumns = new C1Command();

	private readonly C1CommandLink lnkLockRows = new C1CommandLink();

	private readonly C1Command cmdLockRows = new C1Command();

	private readonly C1CommandLink lnkUnlockRows = new C1CommandLink();

	private readonly C1Command cmdUnlockRows = new C1Command();

	private readonly C1CommandLink lnkHideColumns = new C1CommandLink();

	private readonly C1Command cmdHideColumns = new C1Command();

	private readonly C1CommandLink lnkShowColumns = new C1CommandLink();

	private readonly C1Command cmdShowColumns = new C1Command();

	private readonly C1CommandLink lnkFreezeColumn = new C1CommandLink();

	private readonly C1Command cmdFreezeColumn = new C1Command();

	private readonly C1CommandLink lnkUnfreezeColumn = new C1CommandLink();

	private readonly C1Command cmdUnfreezeColumn = new C1Command();

	private readonly C1CommandLink lnkCustomColumnCaption = new C1CommandLink();

	private readonly C1Command cmdCustomColumnCaption = new C1Command();

	private readonly C1CommandLink lnkFixedColumnCaption = new C1CommandLink();

	private readonly C1Command cmdFixedColumnCaption = new C1Command();

	private readonly C1Command cmdAllowManualInputValue = new C1Command();

	private readonly C1CommandLink lnkAllowManualInputValue = new C1CommandLink();

	private readonly C1Command cmdForbiddenManualInputValue = new C1Command();

	private readonly C1CommandLink lnkForbiddenManualInputValue = new C1CommandLink();

	private readonly C1Command cmdCalculateTable = new C1Command();

	private readonly C1Command cmdAutoCollect = new C1Command();

	private readonly C1CommandLink lnkAutoCollect = new C1CommandLink();

	private readonly C1Command cmdCalculateTable2 = new C1Command();

	private readonly C1CommandLink lnkCalculateTable3 = new C1CommandLink();

	private readonly C1Command cmdValidateTable = new C1Command();

	private readonly C1Command cmdValidateTable2 = new C1Command();

	private readonly C1CommandLink lnkValidateTable3 = new C1CommandLink();

	private readonly C1Command cmdCollectFill = new C1Command();

	private readonly C1CommandLink lnkCollectFill = new C1CommandLink();

	private readonly C1Command cmdCollectFill2 = new C1Command();

	private readonly C1CommandLink lnkCollectFill2 = new C1CommandLink();

	private readonly C1CommandLink lnkColumnAccess = new C1CommandLink();

	private readonly C1CommandMenu mnuColumnAccess = new C1CommandMenu();

	private readonly C1CommandLink lnkColumnAccessAll = new C1CommandLink();

	private readonly C1Command cmdColumnAccessAll = new C1Command();

	private readonly C1CommandLink lnkRowAccess = new C1CommandLink();

	private readonly C1CommandMenu mnuRowAccess = new C1CommandMenu();

	private readonly C1CommandLink lnkRowAccessAll = new C1CommandLink();

	private readonly C1Command cmdRowAccessAll = new C1Command();

	private readonly C1Command cmdAddFootRow = new C1Command();

	private readonly C1CommandLink lnkAddFootRow = new C1CommandLink();

	private readonly C1CommandLink lnkAddFootRow2 = new C1CommandLink();

	private readonly C1CommandLink lnkSubtotalTable = new C1CommandLink();

	private readonly C1Command cmdSubtotalTable = new C1Command();

	private readonly C1CommandLink lnkRowRole = new C1CommandLink();

	private readonly C1CommandMenu cmdRowRole = new C1CommandMenu();

	private readonly C1Command cmdAuxEdit = new C1Command
	{
		Text = "下拉列表...",
		Image = Auditai.UI.Platform.Properties.Resources.ComboList16
	};

	private readonly C1CommandLink lnkAuxEdit = new C1CommandLink();

	private readonly C1CommandLink lnkAuxEdit2 = new C1CommandLink();

	private readonly C1CommandLink lnkAuxEdit3 = new C1CommandLink();

	private readonly C1Command cmdEditComment = new C1Command
	{
		Text = "编辑注释...",
		Image = ContextResources.ctxParagraphComment
	};

	private readonly C1CommandLink lnkEditComment = new C1CommandLink();

	private readonly C1CommandLink lnkEditComment2 = new C1CommandLink();

	private readonly C1CommandLink lnkEditComment3 = new C1CommandLink();

	private readonly C1Command cmdLedgerCollectFormulaEdit = new C1Command
	{
		Text = "采数公式",
		Image = Auditai.UI.Controls.Properties.Resources.TableCollect16
	};

	private readonly C1CommandLink lnkLedgerCollectFormulaEdit = new C1CommandLink();

	private readonly C1Command cmdFoot = new C1Command();

	private readonly C1CommandLink lnkFoot = new C1CommandLink();

	private readonly C1Command cmdMakerSign = new C1Command();

	private readonly C1CommandLink lnkMakerSign = new C1CommandLink();

	private readonly C1Command cmdCheckerSign = new C1Command();

	private readonly C1CommandLink lnkCheckerSign = new C1CommandLink();

	private readonly C1Command cmdHelpCenter = new C1Command
	{
		Image = Auditai.UI.Platform.Properties.Resources.HelpCenter,
		Text = "帮助中心",
		Visible = SoftwareLicenseManager.IsShowHelpDocumentButton()
	};

	private readonly C1CommandLink lnkHelpCenter = new C1CommandLink();

	private readonly C1Command cmdHideToolbar = new C1Command();

	private readonly C1CommandLink lnkHideToolbar = new C1CommandLink();

	private readonly C1Command cmdToolbarTables = new C1Command();

	private readonly C1CommandLink lnkToolbarTables = new C1CommandLink();

	private readonly C1ContextMenu ctxToolbarTables = new C1ContextMenu();

	private readonly C1Command cmdBack = new C1Command
	{
		Image = Auditai.UI.Platform.Properties.Resources.back32,
		Text = "后退"
	};

	private readonly C1CommandLink lnkBack = new C1CommandLink();

	private readonly C1Command cmdForward = new C1Command
	{
		Image = Auditai.UI.Platform.Properties.Resources.forward32,
		Text = "前进"
	};

	private readonly C1CommandLink lnkForward = new C1CommandLink();

	private readonly C1CommandLink lnkAlign5 = new C1CommandLink();

	private readonly C1Command cmdRemoveColHeaderFormula = new C1Command
	{
		Text = "删除公式",
		Image = ContextResources.ctxDeleteFormula
	};

	private readonly C1CommandLink lnkRemoveColHeaderFormula = new C1CommandLink();

	private readonly C1CommandMenu mnuRowConvertTo = new C1CommandMenu
	{
		Text = "转换为"
	};

	private readonly C1CommandLink lnkRowConvertTo = new C1CommandLink();

	private readonly C1Command cmdRowConvertToTitle = new C1Command
	{
		Text = "主标题"
	};

	private readonly C1CommandLink lnkRowConvertToTitle = new C1CommandLink();

	private readonly C1Command cmdRowConvertToSubtitle = new C1Command
	{
		Text = "副标题"
	};

	private readonly C1CommandLink lnkRowConvertToSubtitle = new C1CommandLink();

	private readonly C1Command cmdRowConvertToColHeader = new C1Command
	{
		Text = "表格列头"
	};

	private readonly C1CommandLink lnkRowConvertToColHeader = new C1CommandLink();

	private readonly C1Command cmdRowConvertToFoot = new C1Command
	{
		Text = "表底签名"
	};

	private readonly C1CommandLink lnkRowConvertToFoot = new C1CommandLink();

	private readonly C1Command cmdCopyHeaderCellFormula = new C1Command
	{
		Text = "复制列公式"
	};

	private readonly C1CommandLink lnkCopyHeaderCellFormula = new C1CommandLink();

	private readonly C1Command cmdPasteHeaderCellFormula = new C1Command
	{
		Text = "粘贴列公式"
	};

	private readonly C1CommandLink lnkPasteHeaderCellFormula = new C1CommandLink();

	private readonly C1Command cmdRemoveHeaderCellFormula = new C1Command
	{
		Text = "删除列公式",
		Image = ContextResources.ctxDeleteFormula
	};

	private readonly C1CommandLink lnkRemoveHeaderCellFormula = new C1CommandLink();

	private readonly C1CommandLink lnkAuxEdit4 = new C1CommandLink();

	private readonly C1CommandLink lnkDataType4 = new C1CommandLink();

	private readonly C1CommandLink lnkAlign6 = new C1CommandLink();

	private readonly C1Command cmdSortAscending2 = new C1Command
	{
		Text = "升序排序",
		Image = ContextResources.ctxAscending
	};

	private readonly C1CommandLink lnkSortAscending2 = new C1CommandLink();

	private readonly C1Command cmdSortDescending2 = new C1Command
	{
		Text = "降序排序",
		Image = ContextResources.ctxDescending
	};

	private readonly C1CommandLink lnkSortDescending2 = new C1CommandLink();

	private readonly C1Command cmdSumHeaderCells = new C1Command
	{
		Text = "生成合计行",
		Image = ContextResources.ctxTotal
	};

	private readonly C1CommandLink lnkSumHeaderCells = new C1CommandLink();

	private readonly C1Command cmdCopyColumnFormula = new C1Command
	{
		Text = "复制列公式"
	};

	private readonly C1CommandLink lnkCopyColumnFormula = new C1CommandLink();

	private readonly C1Command cmdPasteColumnFormula = new C1Command
	{
		Text = "粘贴列公式"
	};

	private readonly C1CommandLink lnkPasteColumnFormula = new C1CommandLink();

	private readonly C1CommandMenu mnuRowOwnerShare = new C1CommandMenu
	{
		Text = "行独占权限分享",
		CloseOnItemClick = false,
		Image = Auditai.UI.Platform.Properties.Resources.RowOwnerShare16
	};

	private readonly C1CommandLink lnkRowOwnerShare = new C1CommandLink();

	private readonly C1Command cmdCellCollect = new C1Command
	{
		Text = "单元格采账设置"
	};

	private readonly C1CommandLink lnkCellCollect = new C1CommandLink();

	private readonly C1Command cmdAddAttachment = new C1Command
	{
		Text = "插入单元格附件",
		Image = Auditai.UI.Platform.Properties.Resources.ctxAttachment
	};

	private readonly C1CommandLink lnkAddAttachment = new C1CommandLink();

	private readonly C1Command cmdRemoveAttachment = new C1Command
	{
		Text = "删除单元格附件",
		Image = Auditai.UI.Platform.Properties.Resources.ctxRemoveAttachment
	};

	private readonly C1CommandLink lnkRemoveAttachment = new C1CommandLink();

	private readonly C1Command cmdExportAttachment = new C1Command
	{
		Text = "导出单元格附件"
	};

	private readonly C1CommandLink lnkExportAttachment = new C1CommandLink();

	private readonly C1Command cmdCopyColumns = new C1Command
	{
		Text = "复制列"
	};

	private readonly C1CommandLink lnkCopyColumns = new C1CommandLink();

	private readonly C1CommandMenu cmdAdvancedPaste = new C1CommandMenu
	{
		Text = "高级粘贴"
	};

	private readonly C1CommandLink lnkAdvancedPaste = new C1CommandLink();

	private readonly C1Command cmdPasteDistinct = new C1Command
	{
		Text = "粘贴为去重填充列公式：Distinct"
	};

	private readonly C1CommandLink lnkPasteDistinct = new C1CommandLink();

	private readonly C1Command cmdPasteFilter = new C1Command
	{
		Text = "粘贴为筛选填充列公式：DistinctF"
	};

	private readonly C1CommandLink lnkPasteFilter = new C1CommandLink();

	private readonly C1Command cmdPasteVLookUp = new C1Command
	{
		Text = "粘贴为条件找值列公式：VLookUp"
	};

	private readonly C1CommandLink lnkPasteVLookUp = new C1CommandLink();

	private readonly C1Command cmdPasteSumIf = new C1Command
	{
		Text = "粘贴为条件汇总列公式：SumIf"
	};

	private readonly C1CommandLink lnkPasteSumIf = new C1CommandLink();

	private readonly C1Command cmdPasteSimpleList = new C1Command
	{
		Text = "粘贴为常规下拉列表"
	};

	private readonly C1CommandLink lnkPasteSimpleList = new C1CommandLink();

	private readonly C1Command cmdPasteTreeList = new C1Command
	{
		Text = "粘贴为树形下拉列表"
	};

	private readonly C1CommandLink lnkPasteTreeList = new C1CommandLink();

	private readonly C1Command cmdPasteTableList = new C1Command
	{
		Text = "粘贴为表式下拉列表"
	};

	private readonly C1CommandLink lnkPasteTableList = new C1CommandLink();

	private readonly C1Command cmdMoveRowToTop = new C1Command
	{
		Text = "移至首行"
	};

	private readonly C1CommandLink lnkMoveRowToTop = new C1CommandLink();

	private readonly C1Command cmdMoveRowToBottom = new C1Command
	{
		Text = "移至末行"
	};

	private readonly C1CommandLink lnkMoveRowToBottom = new C1CommandLink();

	private readonly C1Command cmdTicketInputMode = new C1Command
	{
		Text = "表单模式"
	};

	private readonly C1CommandLink lnkTicketInputMode = new C1CommandLink();

	private readonly C1Command cmdLockTable = new C1Command
	{
		Text = "锁定表格",
		CheckAutoToggle = true
	};

	private readonly C1CommandLink lnkLockTable = new C1CommandLink();

	private readonly C1Command cmdExportTable = new C1Command
	{
		Text = "导出表格"
	};

	private readonly C1CommandLink lnkExportTable = new C1CommandLink();

	private readonly C1Command cmdDesignTicket = new C1Command
	{
		Text = "设计表单",
		Image = Auditai.UI.Platform.Properties.Resources.TicketMode
	};

	private readonly C1CommandLink lnkDesignTicket = new C1CommandLink();

	private const int _ctxCommandCheckCellMaxCount = 10000;

	private const int MAX_ADD_ROWS_ONCE = 50000;

	private const int MAX_ADD_COLUMNS_ONCE = 50;

	public const int MIN_ROW_NUMBER_COL_WIDTH = 56;

	public static readonly Cursor CursorCross;

	public static readonly Cursor CursorTable;

	public static readonly Cursor CursorRowHeader;

	public static readonly Cursor CursorColumnHeader;

	private static readonly Cursor _curFormatBrush;

	internal static readonly Pen _penFormulaCell;

	private static readonly Pen _penThick;

	private static readonly Pen _penThin;

	public static readonly Pen PenResizeDragging;

	internal static readonly Pen _penFormulaRefRect;

	internal static readonly Pen _penAnimateDash;

	internal static readonly SolidBrush _brushFormulaRefRect;

	internal static SolidBrush _cancelManualInputBackgroundBrush;

	private static readonly Timer _timerFormulaHighlight;

	private static readonly Timer _timerWarningHighlight;

	private FlexGridDecorator gridDecorator;

	private static readonly C1TextBoxEx _dateEdit;

	private static readonly C1TextBoxEx _timeEdit;

	internal C1FlexGridEx _grid;

	private readonly MainForm _owner;

	private Auditai.Model.Table _table;

	/// <summary>
	/// 跨项目引用单元格样式缓存：(row, col) → RefStatus，由 ApplyCrossProjectRefCellStyles 构建，供 BodyOwnerDrawCell_Style O(1) 查找
	/// </summary>
	private Dictionary<(int row, int col), CrossProjectRefCellStyle.RefStatus> _refCellStyleCache;

	private Auditai.Model.Table _tableBeforeEnteringValidation;

	private bool _isUpdatingView;

	private bool _isStartingMouseDownFill;

	private bool _isSingleCellFill;

	private bool _isFilling;

	private bool _isSelectingHeaderCell;

	private bool _skipBeforeSelChange;

	private readonly Color _filterColor = Color.FromArgb(231, 69, 5);

	internal bool _isEditingHeaders;

	internal bool _isFormatBrushing;

	private bool _isRowResizingMouseDown;

	private int _resizingRow;

	private Point _mouseDownLocation;

	private bool _isRowResizingStartedDragging;

	private bool _isColumnResizingMouseDown;

	private int _resizingColumn;

	private bool _isColumnResizingStartedDragging;

	private bool _isMouseOverCancelManualInputIcon;

	private C1SplitterPanel pnlValidation;

	private C1SplitterPanel pnlToolbar;

	internal C1SplitterPanel pnlGrid;

	private C1SplitContainer _navTreeContainer;

	private C1SplitterPanel _navTreeTitlePanel;

	private C1SplitterPanel _navTreeGridPanel;

	private C1ContextMenu _navTitleCtx;

	private readonly TableFindFactory tableReplaceFactory = new TableFindFactory();

	internal TooltipBox _ttpComment = new TooltipBox
	{
		Opacity = 0.8,
		IsBalloon = true
	};

	private readonly TooltipManager tooltipManager = new TooltipManager();

	private readonly SolidBrush _brushStartEditingColHeaderBackground = new SolidBrush(Color.Black);

	private bool _isMouseInStartEditingColHeaderImageRect;

	private bool _isMouseInColHeaderShowMoreMenuImageRect;

	private int _mouseHeaderCol = -1;

	private TableFilterContext _tableFilterContext;

	private C1.Win.C1FlexGrid.CellRange AuxEditSelectionPreserve;

	private bool _isInPasting;

	private bool _hasDoubleClicked_EditingFormula;

	private bool _hasDoubleClicked_EditingValidation;

	public static Color RemindColor;

	private int _warningUpdateTimes;

	private bool _warningTextIsShown = true;

	private Color _warningTextColor = Color.Red;

	private Color _remindTextColor = Color.Orange;

	private string _formulaTextBeforeEdit;

	private bool _isMouseOverNavTreeTitlePanel;

	private bool _isMouseOverNavTreePanelMoreMenuIcon;

	private Bitmap _menuMoreOperationWhiteImage;

	private SolidBrush _brushMoreMenuIconBackground = new SolidBrush(Color.Black);

	private Color _navTreeTitleBackgroundColor = Color.Black;

	private SolidBrush _navTreeTitleBrush = new SolidBrush(Color.Black);

	private StringFormat _navTreeTitleTextFormat = new StringFormat
	{
		Alignment = StringAlignment.Near,
		LineAlignment = StringAlignment.Center
	};

	private Brush _navTreeTitlePanelBackgroundBrush;

	public Tuple<Auditai.Model.Cell, Auditai.Model.Cell> Clipboard { get; set; }

	private Auditai.Model.Column CopiedColumn { get; set; }

	private List<Auditai.Model.Column> CopiedColumns { get; } = new List<Auditai.Model.Column>();


	private Auditai.Model.Cell CopiedHeaderCell { get; set; }

	private UserConfig UserConfig => UserSet.Config;

	private int MouseHeaderCol
	{
		get
		{
			return _mouseHeaderCol;
		}
		set
		{
			if (value != _mouseHeaderCol)
			{
				_mouseHeaderCol = value;
				_grid.Invalidate();
			}
		}
	}

	public C1SplitContainer View { get; private set; }

	public bool IsNotShowComment { get; set; }

	public Auditai.Model.Table Table
	{
		get
		{
			return _table;
		}
		set
		{
			// 取消订阅旧 Table 的 StackChanged 事件
			if (_table != null && _table.CommandsManager != null)
			{
				_table.CommandsManager.StackChanged -= Table_CommandsManager_StackChanged;
			}
			_table = value;
			_tableFilterContext.Table = value;
			gridDecorator?.SetTable(_table);
			// 订阅新 Table 的 StackChanged 事件，编辑后立即更新撤销/恢复按钮状态
			if (_table != null && _table.CommandsManager != null)
			{
				_table.CommandsManager.StackChanged += Table_CommandsManager_StackChanged;
			}
		}
	}

	private void Table_CommandsManager_StackChanged(object sender, EventArgs e)
	{
		Program.MainForm.UpdateUndoRedoButtonState();
	}

	public bool ShowTableNote { get; set; }

	private FormulaContext FormulaContext => FormulaEditor.Context;

	public FormulaEditor FormulaEditor => _owner.FormulaEditor;

	public TableNavGrid TableNavGrid { get; private set; }

	public ValidationEditor ValidationEditor { get; private set; }

	public TableTitleEditor TitleEditor { get; private set; }

	public TableFootEditor FootEditor { get; private set; }

	public AuxEditor AuxEditor { get; private set; }

	public LedgerCollectFormulaEditor LedgerCollectFormulaEditor { get; set; }

	public FormControlFormula FormControlFormula { get; }

	public ListDropDown ListDropDown { get; private set; }

	public InputListDropDown InputListDropDown { get; private set; }

	public bool IsTableLocked => Table?.IsLocked == true;

	public C1ToolBar ToolBar { get; } = new C1ToolBar
	{
		HideFirstDelimiter = true,
		ShowToolTips = false
	};

	private readonly C1Command cmdCrossProjectDataRef = new C1Command
	{
		Text = "跨项目引用",
		Image = Auditai.UI.Platform.Properties.Resources.ReferenceManager
	};

	private readonly C1CommandLink lnkCrossProjectDataRef = new C1CommandLink();

	private readonly C1Command cmdRefreshCrossProjectRefs = new C1Command
	{
		Text = "刷新引用",
		Image = Auditai.UI.Platform.Properties.Resources.RefreshProject
	};

	private readonly C1CommandLink lnkRefreshCrossProjectRefs = new C1CommandLink();

	private readonly C1CommandLink lnkTB_CrossProjectDataRef = new C1CommandLink();
	private readonly C1CommandLink lnkTB_RefreshCrossProjectRefs = new C1CommandLink();

	// 单元格右键菜单中的跨项目引用操作
	private readonly C1Command cmdRefreshSingleRef = new C1Command
	{
		Text = "刷新此引用",
		Image = Auditai.UI.Platform.Properties.Resources.TicketMode
	};
	private readonly C1CommandLink lnkRefreshSingleRef = new C1CommandLink();


	private void InitializeContextMenu()
	{
		cmdAppendRows.CommandStateQuery += CmdAppendRows_CommandStateQuery;
		cmdAppendRows.Click += CmdAppendRows_Click;
		lnkAppendRows.Command = cmdAppendRows;
		ctxEmpty.CommandLinks.Add(lnkAppendRows);
		cmdAppendColumns.CommandStateQuery += CmdAppendColumns_CommandStateQuery;
		cmdAppendColumns.Click += CmdAppendColumns_Click;
		lnkAppendColumns.Command = cmdAppendColumns;
		ctxEmpty.CommandLinks.Add(lnkAppendColumns);
		lnkSumColumns2.Delimiter = true;
		lnkSumColumns2.Command = cmdSumColumns;
		ctxEmpty.CommandLinks.Add(lnkSumColumns2);
		lnkAddFootRow2.Command = cmdAddFootRow;
		lnkAddFootRow2.Delimiter = true;
		ctxEmpty.CommandLinks.Add(lnkAddFootRow2);
		C1CommandLink c1CommandLink = _grid.FilterManager.GenLnkCancelAll();
		c1CommandLink.Delimiter = true;
		ctxEmpty.CommandLinks.Add(c1CommandLink);
		cmdAutoCollect.CommandStateQuery += CmdAutoCollect_CommandStateQuery;
		cmdAutoCollect.Click += CmdAutoCollect_Click;
		lnkAutoCollect.Command = cmdAutoCollect;
		cmdInsertRows.CommandStateQuery += CmdInsertRows_CommandStateQuery;
		cmdInsertRows.Click += CmdInsertRows_Click;
		lnkInsertRows.Command = cmdInsertRows;
		ctxRow.CommandLinks.Add(lnkInsertRows);
		lnkAppendRows2.Command = cmdAppendRows;
		ctxRow.CommandLinks.Add(lnkAppendRows2);
		cmdRemoveRows.CommandStateQuery += CmdRemoveRows_CommandStateQuery;
		cmdRemoveRows.Click += CmdRemoveRows_Click;
		lnkRemoveRows.Command = cmdRemoveRows;
		ctxRow.CommandLinks.Add(lnkRemoveRows);
		mnuRowAccess.CommandStateQuery += MnuRowAccess_CommandStateQuery;
		mnuRowAccess.Popup += MnuRowAccess_Popup;
		lnkRowAccess.Command = mnuRowAccess;
		ctxRow.CommandLinks.Add(lnkRowAccess);
		cmdRowAccessAll.CommandStateQuery += CmdRowAccessAll_CommandStateQuery;
		cmdRowAccessAll.Click += CmdRowAccessAll_Click;
		lnkRowAccessAll.Command = cmdRowAccessAll;
		mnuRowAccess.CommandLinks.Add(lnkRowAccessAll);
		lnkRowOwnerShare.Command = mnuRowOwnerShare;
		mnuRowOwnerShare.CommandStateQuery += mnuRowOwnerShare_CommandStateQuery;
		mnuRowOwnerShare.Popup += MnuRowOwnerShare_Popup;
		ctxRow.CommandLinks.Add(lnkRowOwnerShare);
		cmdLockRows.CommandStateQuery += CmdLockRows_CommandStateQuery;
		cmdLockRows.Click += CmdLockRows_Click;
		lnkLockRows.Command = cmdLockRows;
		ctxRow.CommandLinks.Add(lnkLockRows);
		lnkLockRows.Delimiter = true;
		cmdUnlockRows.CommandStateQuery += CmdUnlockRows_CommandStateQuery;
		cmdUnlockRows.Click += CmdUnlockRows_Click;
		lnkUnlockRows.Command = cmdUnlockRows;
		ctxRow.CommandLinks.Add(lnkUnlockRows);
		cmdMoveUpRows.CommandStateQuery += CmdMoveUpRows_CommandStateQuery;
		cmdMoveUpRows.Click += CmdMoveUpRows_Click;
		lnkMoveUpRows.Command = cmdMoveUpRows;
		ctxRow.CommandLinks.Add(lnkMoveUpRows);
		lnkMoveUpRows.Delimiter = true;
		cmdMoveDownRows.CommandStateQuery += CmdMoveDownRows_CommandStateQuery;
		cmdMoveDownRows.Click += CmdMoveDownRows_Click;
		lnkMoveDownRows.Command = cmdMoveDownRows;
		ctxRow.CommandLinks.Add(lnkMoveDownRows);
		cmdMoveRowToTop.Click += CmdMoveRowToTop_Click;
		lnkMoveRowToTop.Command = cmdMoveRowToTop;
		ctxRow.CommandLinks.Add(lnkMoveRowToTop);
		cmdMoveRowToBottom.Click += CmdMoveRowToBottom_Click;
		lnkMoveRowToBottom.Command = cmdMoveRowToBottom;
		ctxRow.CommandLinks.Add(lnkMoveRowToBottom);
		lnkSumColumns3.Command = cmdSumColumns;
		lnkSumColumns3.Delimiter = true;
		ctxRow.CommandLinks.Add(lnkSumColumns3);
		cmdAlign.CommandStateQuery += CmdAlign_CommandStateQuery;
		lnkAlign.Command = cmdAlign;
		ctxRow.CommandLinks.Add(lnkAlign);
		lnkAlign.Delimiter = true;
		cmdSetHeight.CommandStateQuery += CmdSetHeight_CommandStateQuery;
		cmdSetHeight.Click += CmdSetHeight_Click;
		lnkSetHeight.Command = cmdSetHeight;
		ctxRow.CommandLinks.Add(lnkSetHeight);
		AddRowRoleCommand(RowRole.Normal, "常规类行");
		AddRowRoleCommand(RowRole.Total, "合计类行");
		AddRowRoleCommand(RowRole.Among, "其中类行");
		AddRowRoleCommand(RowRole.Minus, "减项类行");
		AddRowRoleCommand(RowRole.Header, "列头类行");
		AddRowRoleCommand(RowRole.Fixed, "固定类行");
		cmdRowRole.CommandStateQuery += CmdRowRole_CommandStateQuery;
		lnkRowRole.Command = cmdRowRole;
		ctxRow.CommandLinks.Add(lnkRowRole);
		lnkRowConvertTo.Command = mnuRowConvertTo;
		ctxRow.CommandLinks.Add(lnkRowConvertTo);
		mnuRowConvertTo.CommandStateQuery += MnuRowConvertTo_CommandStateQuery;
		lnkRowConvertToTitle.Command = cmdRowConvertToTitle;
		cmdRowConvertToTitle.Click += CmdRowConvertToTitle_Click;
		mnuRowConvertTo.CommandLinks.Add(lnkRowConvertToTitle);
		lnkRowConvertToSubtitle.Command = cmdRowConvertToSubtitle;
		cmdRowConvertToSubtitle.Click += CmdRowConvertToSubtitle_Click;
		mnuRowConvertTo.CommandLinks.Add(lnkRowConvertToSubtitle);
		lnkRowConvertToColHeader.Command = cmdRowConvertToColHeader;
		cmdRowConvertToColHeader.CommandStateQuery += CmdRowConvertToColHeader_CommandStateQuery;
		cmdRowConvertToColHeader.Click += CmdRowConvertToColHeader_Click;
		mnuRowConvertTo.CommandLinks.Add(lnkRowConvertToColHeader);
		lnkRowConvertToFoot.Command = cmdRowConvertToFoot;
		cmdRowConvertToFoot.Click += CmdRowConvertToFoot_Click;
		mnuRowConvertTo.CommandLinks.Add(lnkRowConvertToFoot);
		cmdAddFootRow.CommandStateQuery += CmdAddFootRow_CommandStateQuery;
		cmdAddFootRow.Click += CmdAddFootRow_Click;
		cmdAddFootRow.Text = "增加表底行";
		lnkAddFootRow.Command = cmdAddFootRow;
		lnkAddFootRow.Delimiter = true;
		ctxRow.CommandLinks.Add(lnkAddFootRow);
		cmdAlignTopLeft.CommandStateQuery += CmdAlignTopLeft_CommandStateQuery;
		cmdAlignTopLeft.Click += CmdAlignTopLeft_Click;
		lnkAlignTopLeft.Command = cmdAlignTopLeft;
		cmdAlign.CommandLinks.Add(lnkAlignTopLeft);
		cmdAlignMiddleLeft.CommandStateQuery += CmdAlignMiddleLeftCommandStateQuery;
		cmdAlignMiddleLeft.Click += CmdAlignMiddleLeftClick;
		lnkAlignMiddleLeft.Command = cmdAlignMiddleLeft;
		cmdAlign.CommandLinks.Add(lnkAlignMiddleLeft);
		cmdAlignBottomLeft.CommandStateQuery += CmdAlignBottomLeft_CommandStateQuery;
		cmdAlignBottomLeft.Click += CmdAlignBottomLeft_Click;
		lnkAlignBottomLeft.Command = cmdAlignBottomLeft;
		cmdAlign.CommandLinks.Add(lnkAlignBottomLeft);
		cmdAlignTopCenter.CommandStateQuery += CmdAlignTopCenter_CommandStateQuery;
		cmdAlignTopCenter.Click += CmdAlignTopCenter_Click;
		lnkAlignTopCenter.Command = cmdAlignTopCenter;
		cmdAlign.CommandLinks.Add(lnkAlignTopCenter);
		cmdAlignMiddleCenter.CommandStateQuery += CmdAlignMiddleCenterCommandStateQuery;
		cmdAlignMiddleCenter.Click += CmdAlignMiddleCenterClick;
		lnkAlignMiddleCenter.Command = cmdAlignMiddleCenter;
		cmdAlign.CommandLinks.Add(lnkAlignMiddleCenter);
		cmdAlignBottomCenter.CommandStateQuery += CmdAlignBottomCenter_CommandStateQuery;
		cmdAlignBottomCenter.Click += CmdAlignBottomCenter_Click;
		lnkAlignBottomCenter.Command = cmdAlignBottomCenter;
		cmdAlign.CommandLinks.Add(lnkAlignBottomCenter);
		cmdAlignTopRight.CommandStateQuery += CmdAlignTopRight_CommandStateQuery;
		cmdAlignTopRight.Click += CmdAlignTopRight_Click;
		lnkAlignTopRight.Command = cmdAlignTopRight;
		cmdAlign.CommandLinks.Add(lnkAlignTopRight);
		cmdAlignMiddleRight.CommandStateQuery += CmdAlignMiddleRightCommandStateQuery;
		cmdAlignMiddleRight.Click += CmdAlignMiddleRightClick;
		lnkAlignMiddleRight.Command = cmdAlignMiddleRight;
		cmdAlign.CommandLinks.Add(lnkAlignMiddleRight);
		cmdAlignBottomRight.CommandStateQuery += CmdAlignBottomRight_CommandStateQuery;
		cmdAlignBottomRight.Click += CmdAlignBottomRight_Click;
		lnkAlignBottomRight.Command = cmdAlignBottomRight;
		cmdAlign.CommandLinks.Add(lnkAlignBottomRight);
		cmdInsertColumns.CommandStateQuery += CmdInsertColumns_CommandStateQuery;
		cmdInsertColumns.Click += CmdInsertColumns_Click;
		lnkInsertColumns.Command = cmdInsertColumns;
		ctxColumn.CommandLinks.Add(lnkInsertColumns);
		lnkAppendColumns2.Command = cmdAppendColumns;
		ctxColumn.CommandLinks.Add(lnkAppendColumns2);
		cmdRemoveColumns.CommandStateQuery += CmdRemoveColumns_CommandStateQuery;
		cmdRemoveColumns.Click += CmdRemoveColumns_Click;
		lnkRemoveColumns.Command = cmdRemoveColumns;
		ctxColumn.CommandLinks.Add(lnkRemoveColumns);
		cmdCopyColumns.CommandStateQuery += CmdCopyColumns_CommandStateQuery;
		cmdCopyColumns.Click += CmdCopyColumns_Click;
		lnkCopyColumns.Command = cmdCopyColumns;
		lnkCopyColumns.Delimiter = true;
		ctxColumn.CommandLinks.Add(lnkCopyColumns);
		cmdAdvancedPaste.CommandStateQuery += CmdAdvancedPaste_CommandStateQuery;
		lnkAdvancedPaste.Command = cmdAdvancedPaste;
		ctxColumn.CommandLinks.Add(lnkAdvancedPaste);
		cmdPasteDistinct.CommandStateQuery += CmdPasteDistinct_CommandStateQuery;
		cmdPasteDistinct.Click += CmdPasteDistinct_Click;
		lnkPasteDistinct.Command = cmdPasteDistinct;
		cmdAdvancedPaste.CommandLinks.Add(lnkPasteDistinct);
		cmdPasteFilter.CommandStateQuery += CmdPasteFilter_CommandStateQuery;
		cmdPasteFilter.Click += CmdPasteFilter_Click;
		lnkPasteFilter.Command = cmdPasteFilter;
		cmdAdvancedPaste.CommandLinks.Add(lnkPasteFilter);
		cmdPasteVLookUp.CommandStateQuery += CmdPasteVLookUp_CommandStateQuery;
		cmdPasteVLookUp.Click += CmdPasteVLookUp_Click;
		lnkPasteVLookUp.Command = cmdPasteVLookUp;
		cmdAdvancedPaste.CommandLinks.Add(lnkPasteVLookUp);
		cmdPasteSumIf.CommandStateQuery += CmdPasteSumIf_CommandStateQuery;
		cmdPasteSumIf.Click += CmdPasteSumIf_Click;
		lnkPasteSumIf.Command = cmdPasteSumIf;
		cmdAdvancedPaste.CommandLinks.Add(lnkPasteSumIf);
		cmdPasteSimpleList.CommandStateQuery += CmdPasteSimpleList_CommandStateQuery;
		cmdPasteSimpleList.Click += CmdPasteSimpleList_Click;
		lnkPasteSimpleList.Command = cmdPasteSimpleList;
		lnkPasteSimpleList.Delimiter = true;
		cmdAdvancedPaste.CommandLinks.Add(lnkPasteSimpleList);
		cmdPasteTreeList.CommandStateQuery += CmdPasteTreeList_CommandStateQuery;
		cmdPasteTreeList.Click += CmdPasteTreeList_Click;
		lnkPasteTreeList.Command = cmdPasteTreeList;
		cmdAdvancedPaste.CommandLinks.Add(lnkPasteTreeList);
		cmdPasteTableList.CommandStateQuery += CmdPasteTableList_CommandStateQuery;
		cmdPasteTableList.Click += CmdPasteTableList_Click;
		lnkPasteTableList.Command = cmdPasteTableList;
		cmdAdvancedPaste.CommandLinks.Add(lnkPasteTableList);
		cmdCopyColumnFormula.Image = ContextResources.ctxCopy;
		cmdCopyColumnFormula.CommandStateQuery += CmdCopyColumnFormula_CommandStateQuery;
		cmdCopyColumnFormula.Click += CmdCopyColumnFormula_Click;
		lnkCopyColumnFormula.Delimiter = true;
		lnkCopyColumnFormula.Command = cmdCopyColumnFormula;
		ctxColumn.CommandLinks.Add(lnkCopyColumnFormula);
		cmdPasteColumnFormula.Image = ContextResources.ctxPaste;
		cmdPasteColumnFormula.CommandStateQuery += CmdPasteColumnFormula_CommandStateQuery;
		cmdPasteColumnFormula.Click += CmdPasteColumnFormula_Click;
		lnkPasteColumnFormula.Command = cmdPasteColumnFormula;
		ctxColumn.CommandLinks.Add(lnkPasteColumnFormula);
		cmdRemoveColumnFormula.CommandStateQuery += CmdRemoveColumnFormula_CommandStateQuery;
		cmdRemoveColumnFormula.Click += CmdRemoveColumnFormula_Click;
		lnkRemoveColumnFormula.Command = cmdRemoveColumnFormula;
		ctxColumn.CommandLinks.Add(lnkRemoveColumnFormula);
		cmdAllowManualInputValue.CommandStateQuery += CmdAllowManualInputValue_CommandStateQuery;
		cmdAllowManualInputValue.Click += CmdAllowManualInputValue_Click;
		lnkAllowManualInputValue.Command = cmdAllowManualInputValue;
		cmdForbiddenManualInputValue.CommandStateQuery += CmdForbiddenManualInputValue_CommandStateQuery;
		cmdForbiddenManualInputValue.Click += CmdForbiddenManualInputValue_Click;
		lnkForbiddenManualInputValue.Command = cmdForbiddenManualInputValue;
		mnuColumnAccess.CommandStateQuery += MnuColumnAccess_CommandStateQuery;
		mnuColumnAccess.Popup += MnuColumnAccess_Popup;
		lnkColumnAccess.Command = mnuColumnAccess;
		ctxColumn.CommandLinks.Add(lnkColumnAccess);
		lnkColumnAccess.Delimiter = true;
		cmdColumnAccessAll.CommandStateQuery += CmdColumnAccessAll_CommandStateQuery;
		cmdColumnAccessAll.Click += CmdColumnAccessAll_Click;
		lnkColumnAccessAll.Command = cmdColumnAccessAll;
		mnuColumnAccess.CommandLinks.Add(lnkColumnAccessAll);
		cmdLockColumns.CommandStateQuery += CmdLockColumns_CommandStateQuery;
		cmdLockColumns.Click += CmdLockColumns_Click;
		lnkLockColumns.Command = cmdLockColumns;
		ctxColumn.CommandLinks.Add(lnkLockColumns);
		cmdUnlockColumns.CommandStateQuery += CmdUnlockColumns_CommandStateQuery;
		cmdUnlockColumns.Click += CmdUnlockColumns_Click;
		lnkUnlockColumns.Command = cmdUnlockColumns;
		ctxColumn.CommandLinks.Add(lnkUnlockColumns);
		cmdHideColumns.CommandStateQuery += CmdHideColumns_CommandStateQuery;
		cmdHideColumns.Click += CmdHideColumns_Click;
		lnkHideColumns.Command = cmdHideColumns;
		ctxColumn.CommandLinks.Add(lnkHideColumns);
		lnkHideColumns.Delimiter = true;
		cmdShowColumns.CommandStateQuery += CmdShowColumns_CommandStateQuery;
		cmdShowColumns.Click += CmdShowColumns_Click;
		lnkShowColumns.Command = cmdShowColumns;
		ctxColumn.CommandLinks.Add(lnkShowColumns);
		cmdFreezeColumn.CommandStateQuery += CmdFreezeColumn_CommandStateQuery;
		cmdFreezeColumn.Click += CmdFreezeColumn_Click;
		lnkFreezeColumn.Command = cmdFreezeColumn;
		ctxColumn.CommandLinks.Add(lnkFreezeColumn);
		cmdUnfreezeColumn.CommandStateQuery += CmdUnfreezeColumn_CommandStateQuery;
		cmdUnfreezeColumn.Click += CmdUnfreezeColumn_Click;
		lnkUnfreezeColumn.Command = cmdUnfreezeColumn;
		ctxColumn.CommandLinks.Add(lnkUnfreezeColumn);
		cmdSetColumnWidth.CommandStateQuery += CmdSetColumnWidth_CommandStateQuery;
		cmdSetColumnWidth.Click += CmdSetColumnWidth_Click;
		lnkSetColumnWidth.Command = cmdSetColumnWidth;
		ctxColumn.CommandLinks.Add(lnkSetColumnWidth);
		lnkSetColumnWidth.Delimiter = true;
		cmdAuxEdit.CommandStateQuery += CmdAuxEdit_CommandStateQuery;
		cmdAuxEdit.Click += CmdAuxEdit_Click;
		lnkAuxEdit.Command = cmdAuxEdit;
		ctxColumn.CommandLinks.Add(lnkAuxEdit);
		cmdEditComment.CommandStateQuery += CmdEditComment_CommandStateQuery;
		cmdEditComment.Click += CmdEditComment_Click;
		lnkEditComment.Command = cmdEditComment;
		ctxColumn.CommandLinks.Add(lnkEditComment);
		cmdLedgerCollectFormulaEdit.CommandStateQuery += CmdLedgerCollectFormulaEdit_CommandStateQuery;
		cmdLedgerCollectFormulaEdit.Click += CmdLedgerCollectFormulaEdit_Click;
		lnkLedgerCollectFormulaEdit.Command = cmdLedgerCollectFormulaEdit;
		ctxColumn.CommandLinks.Add(lnkLedgerCollectFormulaEdit);
		cmdDataType.CommandStateQuery += CmdDataType_CommandStateQuery;
		lnkDataType.Command = cmdDataType;
		ctxColumn.CommandLinks.Add(lnkDataType);
		cmdDataTypeString.CommandStateQuery += CmdDataTypeString_CommandStateQuery;
		cmdDataTypeString.Click += CmdDataTypeString_Click;
		lnkDataTypeString.Command = cmdDataTypeString;
		cmdDataType.CommandLinks.Add(lnkDataTypeString);
		AddCommandMenu("数值格式", Auditai.UI.Platform.Properties.Resources.Numeric, SetDataFormatNumeric, Tuple.Create("1234.56", DataFormatType.Number), Tuple.Create("1,234.56", DataFormatType.Comma), Tuple.Create("$1,234.56", DataFormatType.NumDollar), Tuple.Create("￥1,234.56", DataFormatType.NumRmb), Tuple.Create("123,456.78%", DataFormatType.Percentage));
		C1CommandMenu menu = AddCommandMenu("日期格式", Auditai.UI.Platform.Properties.Resources.Date, SetDataFormatDate, Tuple.Create("2017年12月31日", DataFormatType.DateChinese), Tuple.Create("2017-12-31", DataFormatType.DateDash), Tuple.Create("2017/12/31", DataFormatType.DateSlash), Tuple.Create("2017.12.31", DataFormatType.DateDot));
		AppendCommandMenu(menu, SetDataFormatDateYearMonth, Tuple.Create("2017年12月", DataFormatType.DateYearMonthChinese), Tuple.Create("2017-12", DataFormatType.DateYearMonthDash), Tuple.Create("2017/12", DataFormatType.DateYearMonthSlash), Tuple.Create("2017.12", DataFormatType.DateYearMonthDot));
		AddCommandMenu("时间格式", null, SetDataFormatTime, Tuple.Create("10时20分30秒", DataFormatType.TimeLongChinese), Tuple.Create("10时20分", DataFormatType.TimeShortChinese), Tuple.Create("10:20:30", DataFormatType.TimeLong), Tuple.Create("10:20", DataFormatType.TimeShort));
		AddCommandMenu("判断格式", Auditai.UI.Platform.Properties.Resources.Boolean, SetDataFormatBoolean, Tuple.Create("复选框", DataFormatType.BoolCheckBox), Tuple.Create("开关钮", DataFormatType.BoolOnOff));
		lnkAlign2.Command = cmdAlign;
		C1CommandLink c1CommandLink2 = _grid.FilterManager.GenLnkFilter();
		c1CommandLink2.Delimiter = true;
		ctxColumn.CommandLinks.Add(c1CommandLink2);
		ctxColumn.CommandLinks.Add(_grid.FilterManager.GenLnkSelect());
		ctxColumn.CommandLinks.Add(_grid.FilterManager.GenLnkCancelCurrentColumn());
		cmdSortAscending.CommandStateQuery += CmdSortAscending_CommandStateQuery;
		cmdSortAscending.Click += CmdSortAscending_Click;
		lnkSortAscending.Command = cmdSortAscending;
		lnkSortAscending.Delimiter = true;
		ctxColumn.CommandLinks.Add(lnkSortAscending);
		cmdSortDescending.CommandStateQuery += CmdSortDescending_CommandStateQuery;
		cmdSortDescending.Click += CmdSortDescending_Click;
		lnkSortDescending.Command = cmdSortDescending;
		ctxColumn.CommandLinks.Add(lnkSortDescending);
		cmdSumColumns.CommandStateQuery += CmdSumColumns_CommandStateQuery;
		cmdSumColumns.Click += CmdSumColumns_Click;
		lnkSumColumns.Command = cmdSumColumns;
		ctxColumn.CommandLinks.Add(lnkSumColumns);
		cmdCancelSumColumns.CommandStateQuery += CmdCancelSumColumns_CommandStateQuery;
		cmdCancelSumColumns.Click += CmdCancelSumColumns_Click;
		lnkCancelSumColumns.Command = cmdCancelSumColumns;
		ctxColumn.CommandLinks.Add(lnkCancelSumColumns);
		cmdSubtotal.CommandStateQuery += CmdSubtotal_CommandStateQuery;
		cmdSubtotal.Click += CmdSubtotal_Click;
		lnkSubtotal.Command = cmdSubtotal;
		ctxColumn.CommandLinks.Add(lnkSubtotal);
		cmdCancelSubtotal.CommandStateQuery += CmdCancelSubtotal_CommandStateQuery;
		cmdCancelSubtotal.Click += CmdCancelSubtotal_Click;
		lnkCancelSubtotal.Command = cmdCancelSubtotal;
		ctxColumn.CommandLinks.Add(lnkCancelSubtotal);
		cmdSubtotalTable.CommandStateQuery += CmdSubtotalTable_CommandStateQuery;
		cmdSubtotalTable.Click += CmdSubtotalTable_Click;
		lnkSubtotalTable.Command = cmdSubtotalTable;
		ctxColumn.CommandLinks.Add(lnkSubtotalTable);
		lnkSortAscending.Delimiter = true;
		lnkSumColumns.Delimiter = true;
		lnkSubtotal.Delimiter = true;
		lnkSubtotalTable.Delimiter = true;
		cmdCopy.CommandStateQuery += CmdCopy_CommandStateQuery;
		cmdCopy.Click += CmdCopy_Click;
		lnkCopy.Command = cmdCopy;
		ctxRange.CommandLinks.Add(lnkCopy);
		cmdCut.CommandStateQuery += CmdCut_CommandStateQuery;
		cmdCut.Click += CmdCut_Click;
		lnkCut.Command = cmdCut;
		ctxRange.CommandLinks.Add(lnkCut);
		cmdPaste.CommandStateQuery += CmdPaste_CommandStateQuery;
		lnkPaste.Command = cmdPaste;
		ctxRange.CommandLinks.Add(lnkPaste);
		cmdMergeCells.CommandStateQuery += CmdMergeCells_CommandStateQuery;
		cmdMergeCells.Click += CmdMergeCells_Click;
		lnkMergeCells.Command = cmdMergeCells;
		ctxRange.CommandLinks.Add(lnkMergeCells);
		cmdMergeEveryHorizontalCells.CommandStateQuery += CmdMergeEveryHorizontalCells_CommandStateQuery;
		cmdMergeEveryHorizontalCells.Click += CmdMergeEveryHorizontalCells_Click;
		lnkMergeEveryHorizontalCells.Command = cmdMergeEveryHorizontalCells;
		ctxRange.CommandLinks.Add(lnkMergeEveryHorizontalCells);
		cmdUnmergeCells.CommandStateQuery += CmdUnmergeCells_CommandStateQuery;
		cmdUnmergeCells.Text = "拆分单元格";
		cmdUnmergeCells.Click += CmdUnmergeCells_Click;
		lnkUnmergeCells.Command = cmdUnmergeCells;
		ctxRange.CommandLinks.Add(lnkUnmergeCells);
		cmdLockCells.CommandStateQuery += CmdLockCells_CommandStateQuery;
		cmdLockCells.Click += CmdLockCells_Click;
		lnkLockCells.Command = cmdLockCells;
		ctxRange.CommandLinks.Add(lnkLockCells);
		cmdUnlockCells.CommandStateQuery += CmdUnlockCells_CommandStateQuery;
		cmdUnlockCells.Click += CmdUnlockCells_Click;
		lnkUnlockCells.Command = cmdUnlockCells;
		ctxRange.CommandLinks.Add(lnkUnlockCells);
		lnkAuxEdit3.Command = cmdAuxEdit;
		ctxRange.CommandLinks.Add(lnkAuxEdit3);
		lnkEditComment3.Command = cmdEditComment;
		ctxRange.CommandLinks.Add(lnkEditComment3);
		lnkDataType3.Command = cmdDataType;
		ctxRange.CommandLinks.Add(lnkDataType3);
		lnkAlign3.Command = cmdAlign;
		ctxRange.CommandLinks.Add(lnkAlign3);
		cmdCopyFill.CommandStateQuery += CmdCopyFill_CommandStateQuery;
		cmdCopyFill.Click += CmdCopyFill_Click;
		lnkCopyFill.Command = cmdCopyFill;
		ctxRange.CommandLinks.Add(lnkCopyFill);
		cmdSequenceFill.CommandStateQuery += CmdSequenceFill_CommandStateQuery;
		cmdSequenceFill.Click += CmdSequenceFill_Click;
		lnkSequenceFill.Command = cmdSequenceFill;
		ctxRange.CommandLinks.Add(lnkSequenceFill);
		cmdBlankFill.CommandStateQuery += CmdBlankFill_CommandStateQuery;
		cmdBlankFill.Click += CmdBlankFill_Click;
		lnkBlankFill.Command = cmdBlankFill;
		ctxRange.CommandLinks.Add(lnkBlankFill);
		lnkRemoveFormula2.Command = cmdRemoveFormula;
		ctxRange.CommandLinks.Add(lnkRemoveFormula2);
		cmdRemoveAttachment.Click += CmdRemoveAttachment_Click;
		cmdRemoveAttachment.CommandStateQuery += CmdRemoveAttachment_CommandStateQuery;
		lnkRemoveAttachment.Command = cmdRemoveAttachment;
		ctxRange.CommandLinks.Add(lnkRemoveAttachment);
		cmdExportAttachment.Click += CmdExportAttachment_Click;
		cmdExportAttachment.CommandStateQuery += CmdExportAttachment_CommandStateQuery;
		lnkExportAttachment.Command = cmdExportAttachment;
		ctxRange.CommandLinks.Add(lnkExportAttachment);
		ctxLock.CommandLinks.Add(new C1CommandLink(cmdExportAttachment));
		lnkMergeCells.Delimiter = true;
		lnkLockCells.Delimiter = true;
		lnkAuxEdit3.Delimiter = true;
		lnkCollectFill.Delimiter = true;
		lnkRemoveFormula2.Delimiter = true;
		cmdPasteValue.CommandStateQuery += CmdPasteValue_CommandStateQuery;
		cmdPasteValue.Click += CmdPasteValue_Click;
		lnkPasteValue.Command = cmdPasteValue;
		cmdPaste.CommandLinks.Add(lnkPasteValue);
		cmdPasteFormat.CommandStateQuery += CmdPasteFormat_CommandStateQuery;
		cmdPasteFormat.Click += CmdPasteFormat_Click;
		lnkPasteFormat.Command = cmdPasteFormat;
		cmdPaste.CommandLinks.Add(lnkPasteFormat);
		cmdPasteFormula.CommandStateQuery += CmdPasteFormula_CommandStateQuery;
		cmdPasteFormula.Click += CmdPasteFormula_Click;
		lnkPasteFormula.Command = cmdPasteFormula;
		cmdPaste.CommandLinks.Add(lnkPasteFormula);
		lnkCopy2.Command = cmdCopy;
		ctxCell.CommandLinks.Add(lnkCopy2);
		lnkCut2.Command = cmdCut;
		ctxCell.CommandLinks.Add(lnkCut2);
		lnkPaste2.Command = cmdPaste;
		ctxCell.CommandLinks.Add(lnkPaste2);
		lnkLockCells2.Command = cmdLockCells;
		ctxCell.CommandLinks.Add(lnkLockCells2);
		lnkUnlockCells2.Command = cmdUnlockCells;
		ctxCell.CommandLinks.Add(lnkUnlockCells2);
		lnkAuxEdit2.Delimiter = true;
		lnkAuxEdit2.Command = cmdAuxEdit;
		ctxCell.CommandLinks.Add(lnkAuxEdit2);
		lnkEditComment2.Command = cmdEditComment;
		ctxCell.CommandLinks.Add(lnkEditComment2);
		lnkDataType2.Command = cmdDataType;
		ctxCell.CommandLinks.Add(lnkDataType2);
		lnkAlign4.Command = cmdAlign;
		ctxCell.CommandLinks.Add(lnkAlign4);
		cmdEditFormula.CommandStateQuery += CmdEditFormula_CommandStateQuery;
		cmdEditFormula.Click += CmdEditFormula_Click;
		lnkEditFormula.Command = cmdEditFormula;
		cmdRemoveFormula.CommandStateQuery += CmdRemoveFormula_CommandStateQuery;
		cmdRemoveFormula.Click += CmdRemoveFormula_Click;
		lnkRemoveFormula.Command = cmdRemoveFormula;
		ctxCell.CommandLinks.Add(lnkRemoveFormula);
		C1CommandLink c1CommandLink3 = _grid.FilterManager.GenLnkFilter();
		c1CommandLink3.Delimiter = true;
		ctxCell.CommandLinks.Add(c1CommandLink3);
		ctxCell.CommandLinks.Add(_grid.FilterManager.GenLnkSelect());
		ctxCell.CommandLinks.Add(_grid.FilterManager.GenLnkCancelCurrentColumn());
		cmdCollectFill.CommandStateQuery += CmdCollectFill_CommandStateQuery;
		cmdCollectFill.Click += CmdCollectFill_Click;
		lnkCollectFill.Command = cmdCollectFill;
		cmdCellCollect.CommandStateQuery += CmdCellCollect_CommandStateQuery;
		cmdCellCollect.Click += CmdCellCollect_Click;
		lnkCellCollect.Command = cmdCellCollect;
		lnkCellCollect.Delimiter = true;
		ctxCell.CommandLinks.Add(lnkCellCollect);
		cmdAddAttachment.Click += CmdAddAttachment_Click;
		cmdAddAttachment.CommandStateQuery += CmdAddAttachment_CommandStateQuery;
		lnkAddAttachment.Command = cmdAddAttachment;
		lnkAddAttachment.Delimiter = true;
		ctxCell.CommandLinks.Add(lnkAddAttachment);
		lnkLockCells2.Delimiter = true;
		lnkEditFormula.Delimiter = true;
		lnkRemoveFormula.Delimiter = true;
		c1CommandLink3.Delimiter = true;
		lnkCollectFill.Delimiter = true;
		cmdCustomColumnCaption.CommandStateQuery += CmdCustomColumnCaption_CommandStateQuery;
		cmdCustomColumnCaption.Click += CmdCustomColumnCaption_Click;
		lnkCustomColumnCaption.Command = cmdCustomColumnCaption;
		ctxTableHeader.CommandLinks.Add(lnkCustomColumnCaption);
		cmdFixedColumnCaption.CommandStateQuery += CmdFixedColumnCaption_CommandStateQuery;
		cmdFixedColumnCaption.Click += CmdFixedColumnCaption_Click;
		lnkFixedColumnCaption.Command = cmdFixedColumnCaption;
		ctxTableHeader.CommandLinks.Add(lnkFixedColumnCaption);
		lnkAlign5.Command = cmdAlign;
		ctxColHeader.CommandLinks.Add(lnkAlign5);
		cmdRemoveColHeaderFormula.Click += CmdRemoveColHeaderFormula_Click;
		lnkRemoveColHeaderFormula.Command = cmdRemoveColHeaderFormula;
		ctxColHeader.CommandLinks.Add(lnkRemoveColHeaderFormula);
		cmdCopyHeaderCellFormula.Image = ContextResources.ctxCopy;
		cmdCopyHeaderCellFormula.CommandStateQuery += CmdCopyHeaderCellFormula_CommandStateQuery;
		cmdCopyHeaderCellFormula.Click += CmdCopyHeaderCellFormula_Click;
		lnkCopyHeaderCellFormula.Command = cmdCopyHeaderCellFormula;
		ctxHeaderCell.CommandLinks.Add(lnkCopyHeaderCellFormula);
		cmdPasteHeaderCellFormula.Image = ContextResources.ctxPaste;
		cmdPasteHeaderCellFormula.CommandStateQuery += CmdPasteHeaderCellFormula_CommandStateQuery;
		cmdPasteHeaderCellFormula.Click += CmdPasteHeaderCellFormula_Click;
		lnkPasteHeaderCellFormula.Command = cmdPasteHeaderCellFormula;
		ctxHeaderCell.CommandLinks.Add(lnkPasteHeaderCellFormula);
		cmdRemoveHeaderCellFormula.Click += CmdRemoveHeaderCellFormula_Click;
		cmdRemoveHeaderCellFormula.CommandStateQuery += CmdRemoveHeaderCellFormula_CommandStateQuery;
		lnkRemoveHeaderCellFormula.Command = cmdRemoveHeaderCellFormula;
		ctxHeaderCell.CommandLinks.Add(lnkRemoveHeaderCellFormula);
		lnkAuxEdit4.Delimiter = true;
		lnkAuxEdit4.Command = cmdAuxEdit;
		ctxHeaderCell.CommandLinks.Add(lnkAuxEdit4);
		lnkDataType4.Command = cmdDataType;
		ctxHeaderCell.CommandLinks.Add(lnkDataType4);
		lnkAlign6.Command = cmdAlign;
		ctxHeaderCell.CommandLinks.Add(lnkAlign6);
		cmdSortAscending2.Click += CmdSortAscending2_Click;
		lnkSortAscending2.Delimiter = true;
		lnkSortAscending2.Command = cmdSortAscending2;
		ctxHeaderCell.CommandLinks.Add(lnkSortAscending2);
		cmdSortDescending2.Click += CmdSortDescending2_Click;
		lnkSortDescending2.Command = cmdSortDescending2;
		ctxHeaderCell.CommandLinks.Add(lnkSortDescending2);
		cmdSumHeaderCells.Click += CmdSumHeaderCells_Click;
		lnkSumHeaderCells.Delimiter = true;
		lnkSumHeaderCells.Command = cmdSumHeaderCells;
		ctxHeaderCell.CommandLinks.Add(lnkSumHeaderCells);

		// 跨项目数据引用命令
		cmdCrossProjectDataRef.Click += CmdCrossProjectDataRef_Click;
		cmdCrossProjectDataRef.CommandStateQuery += CmdCrossProjectDataRef_CommandStateQuery;
		lnkCrossProjectDataRef.Command = cmdCrossProjectDataRef;
		lnkCrossProjectDataRef.Delimiter = true;
		ctxCell.CommandLinks.Add(lnkCrossProjectDataRef);
		// 刷新跨项目引用命令
		cmdRefreshCrossProjectRefs.Click += CmdRefreshCrossProjectRefs_Click;
		cmdRefreshCrossProjectRefs.CommandStateQuery += CmdRefreshCrossProjectRefs_CommandStateQuery;
		lnkRefreshCrossProjectRefs.Command = cmdRefreshCrossProjectRefs;
		ctxCell.CommandLinks.Add(lnkRefreshCrossProjectRefs);
		// 刷新此引用（针对当前单元格）
		cmdRefreshSingleRef.Click += CmdRefreshSingleRef_Click;
		cmdRefreshSingleRef.CommandStateQuery += CmdRefreshSingleRef_CommandStateQuery;
		lnkRefreshSingleRef.Command = cmdRefreshSingleRef;
		ctxCell.CommandLinks.Add(lnkRefreshSingleRef);
	}

	private void CmdForbiddenManualInputValue_Click(object sender, ClickEventArgs e)
	{
		SetFormulaColumnAllowManualInput(isAllowManualInput: false);
	}

	private void CmdForbiddenManualInputValue_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_grid.BodyCol < 0)
		{
			e.Visible = false;
			return;
		}
		if (!_grid.IsEntireColumnSelected)
		{
			e.Visible = false;
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			Auditai.Model.Column column = Table.Columns[i];
			Auditai.Model.CellStyle style = column.Style;
			if (style != null && (style.Format?.IsAllowEditOnExistFormula).GetValueOrDefault())
			{
				cmdForbiddenManualInputValue.Text = "禁止手动录值";
				e.Visible = true;
				return;
			}
		}
		e.Visible = false;
	}

	private void CmdAllowManualInputValue_Click(object sender, ClickEventArgs e)
	{
		SetFormulaColumnAllowManualInput(isAllowManualInput: true);
	}

	private void CmdAllowManualInputValue_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_grid.BodyCol < 0)
		{
			e.Visible = false;
			return;
		}
		if (!_grid.IsEntireColumnSelected)
		{
			e.Visible = false;
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			Auditai.Model.Column column = Table.Columns[i];
			if (column.HasFormula && CanEditColumn(column))
			{
				cmdAllowManualInputValue.Text = "允许手动录值";
				e.Visible = true;
				return;
			}
		}
		e.Visible = false;
	}

	private void AddRowRoleCommand(RowRole rr, string caption)
	{
		C1Command cmd = new C1Command
		{
			CheckAutoToggle = true
		};
		cmd.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			cmd.Text = caption;
			e.Checked = Table.Rows[_grid.BodyRow].Role == rr;
			if (rr == RowRole.Fixed || rr == RowRole.Header || rr == RowRole.Total)
			{
				if (!Table.RowOwnerLoad || CanSetRowOwnerExclusive())
				{
					e.Visible = true;
				}
				else
				{
					e.Visible = false;
				}
			}
		};
		cmd.Click += delegate
		{
			if (rr == RowRole.Header && Table.Columns.Any(delegate(Auditai.Model.Column c)
			{
				if (!c.HasFormula)
				{
					return false;
				}
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(c.Formula);
				return formulaEvaluator.HasLqCrossTable();
			}))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格应用了CrossTable函数，不能设置列头类行。");
			}
			else
			{
				for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
				{
					Table.Rows[i].UpdateRole(rr);
				}
				CalcCurrentTable();
				Table.Ticket.IsCacheExpired = true;
			}
		};
		C1CommandLink value = new C1CommandLink(cmd);
		cmdRowRole.CommandLinks.Add(value);
	}

	private C1CommandMenu AddCommandMenu(string text, System.Drawing.Image image, Action<DataFormatType> action, params Tuple<string, DataFormatType>[] commands)
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

	private void CmdAlignBottomRight_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.BottomRight);
	}

	private void CmdAlignBottomRight_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignBottomRight.Text = "右下对齐";
		cmdAlignBottomRight.Image = ContextResources.ctxAlignBottomRight;
		if (_isEditingHeaders)
		{
			cmdAlignBottomRight.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.BottomRight;
		}
		else
		{
			cmdAlignBottomRight.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.BottomRight;
		}
	}

	private void CmdAlignBottomCenter_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.BottomCenter);
	}

	private void CmdAlignBottomCenter_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignBottomCenter.Text = "中下对齐";
		cmdAlignBottomCenter.Image = ContextResources.ctxAlignBottomCenter;
		if (_isEditingHeaders)
		{
			cmdAlignBottomCenter.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.BottomCenter;
		}
		else
		{
			cmdAlignBottomCenter.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.BottomCenter;
		}
	}

	private void CmdAlignBottomLeft_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.BottomLeft);
	}

	private void CmdAlignBottomLeft_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignBottomLeft.Text = "左下对齐";
		cmdAlignBottomLeft.Image = ContextResources.ctxAlignBottomLeft;
		if (_isEditingHeaders)
		{
			cmdAlignBottomLeft.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.BottomLeft;
		}
		else
		{
			cmdAlignBottomLeft.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.BottomLeft;
		}
	}

	private void CmdAlignMiddleRightClick(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.MiddleRight);
	}

	private void CmdAlignMiddleRightCommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignMiddleRight.Text = "右中对齐";
		cmdAlignMiddleRight.Image = ContextResources.ctxAlignMiddleRight;
		if (_isEditingHeaders)
		{
			cmdAlignMiddleRight.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.MiddleRight;
		}
		else
		{
			cmdAlignMiddleRight.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.MiddleRight;
		}
	}

	private void CmdAlignMiddleCenterClick(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.MiddleCenter);
	}

	private void CmdAlignMiddleCenterCommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignMiddleCenter.Text = "中中对齐";
		cmdAlignMiddleCenter.Image = ContextResources.ctxAlignMiddleCenter;
		if (_isEditingHeaders)
		{
			cmdAlignMiddleCenter.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.MiddleCenter;
		}
		else
		{
			cmdAlignMiddleCenter.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.MiddleCenter;
		}
	}

	private void CmdAlignMiddleLeftClick(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.MiddleLeft);
	}

	private void CmdAlignMiddleLeftCommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignMiddleLeft.Text = "左中对齐";
		cmdAlignMiddleLeft.Image = ContextResources.ctxAlignMiddleLeft;
		if (_isEditingHeaders)
		{
			cmdAlignMiddleLeft.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.MiddleLeft;
		}
		else
		{
			cmdAlignMiddleLeft.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.MiddleLeft;
		}
	}

	private void CmdAlignTopCenter_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.TopCenter);
	}

	private void CmdAlignTopCenter_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignTopCenter.Text = "中上对齐";
		cmdAlignTopCenter.Image = ContextResources.ctxAlignTopCenter;
		if (_isEditingHeaders)
		{
			cmdAlignTopCenter.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.TopCenter;
		}
		else
		{
			cmdAlignTopCenter.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.TopCenter;
		}
	}

	private void CmdAlignTopRight_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.TopRight);
	}

	private void CmdAlignTopRight_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignTopRight.Text = "右上对齐";
		cmdAlignTopRight.Image = ContextResources.ctxAlignTopRight;
		if (_isEditingHeaders)
		{
			cmdAlignTopRight.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align.GetValueOrDefault() == CellTextAlign.TopRight;
		}
		else
		{
			cmdAlignTopRight.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.TopRight;
		}
	}

	private void CmdAlign_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlign.Text = "对齐";
		if (_grid.BodyCol < 0 || _grid.BodyRow < 0)
		{
			cmdAlign.Visible = false;
		}
		else
		{
			cmdAlign.Visible = !Table.Columns[_grid.BodyCol].IsLocked;
		}
	}

	private void CmdAlignTopLeft_Click(object sender, ClickEventArgs e)
	{
		SetAlign(CellTextAlign.TopLeft);
	}

	private void CmdAlignTopLeft_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAlignTopLeft.Text = "左上对齐";
		cmdAlignTopLeft.Image = ContextResources.ctxAlignTopLeft;
		if (_isEditingHeaders)
		{
			cmdAlignTopLeft.Checked = Table.Columns[_grid.Col - _grid.Cols.Fixed].CaptionStyle.Align == CellTextAlign.TopLeft;
		}
		else
		{
			cmdAlignTopLeft.Checked = Table[_grid.BodyRow, _grid.BodyCol].DisplayAlign == CellTextAlign.TopLeft;
		}
	}

	private void CmdSetHeight_Click(object sender, ClickEventArgs e)
	{
		int heightDisplay = _grid.Rows[_grid.Row].HeightDisplay;
		decimal? num = InputForm.Numeric("设置行高", "请输入行高", heightDisplay);
		if (num.HasValue)
		{
			SetRowHeight((int)num.Value);
		}
	}

	private void CmdSetHeight_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSetHeight.Text = "设置行高...";
		if (_grid.BodyRow < 0)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void CmdMoveRowToBottom_Click(object sender, ClickEventArgs e)
	{
		MoveRowsToBottom();
	}

	private void CmdMoveRowToTop_Click(object sender, ClickEventArgs e)
	{
		MoveRowsToTop();
	}

	private void CmdMoveDownRows_Click(object sender, ClickEventArgs e)
	{
		MoveDownRows();
	}

	private void CmdMoveDownRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdMoveDownRows.Text = "下移行";
		e.Enabled = _grid.BodyRowSel < _grid.BodyRowsCount - 1;
	}

	private void CmdMoveUpRows_Click(object sender, ClickEventArgs e)
	{
		MoveUpRows();
	}

	private void CmdMoveUpRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdMoveUpRows.Text = "上移行";
		e.Enabled = _grid.BodyRow > 0;
	}

	private void CmdRemoveRows_Click(object sender, ClickEventArgs e)
	{
		RemoveRows();
	}

	private void CmdRemoveRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRemoveRows.Text = "删除行";
		cmdRemoveRows.Image = ContextResources.ctxDeleteRow;
		if (_grid.BodyRow < 0)
		{
			e.Enabled = false;
			return;
		}
		if (IsTableLocked)
		{
			e.Enabled = false;
			return;
		}
		int start = _grid.BodyRow;
		int end = _grid.BodyRowSel;
		if (!Table.Rows.Where((Auditai.Model.Row r) => r.Index >= start && r.Index <= end).All((Auditai.Model.Row r) => CanEditRow(r)))
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private void CmdInsertRows_Click(object sender, ClickEventArgs e)
	{
		InsertRows();
	}

	private void CmdInsertRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdInsertRows.Text = "插入行...";
		cmdInsertRows.Image = ContextResources.ctxInsertRow;
	}

	private void CmdAddFootRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			cmdAddFootRow.Visible = false;
		}
		else
		{
			cmdAddFootRow.Visible = Table.Foot.Rows.Count == 0;
		}
	}

	private void CmdAddFootRow_Click(object sender, ClickEventArgs e)
	{
		FootEditor.AppendRow();
	}

	private void CmdAppendColumns_Click(object sender, ClickEventArgs e)
	{
		AppendColumns();
	}

	private void CmdAppendColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool flag = SoftwareLicenseManager.IsAllowModifyTableStruct();
		cmdAppendColumns.Enabled = flag && HasSchemaPermission();
		cmdAppendColumns.Text = "追加列...";
		cmdAppendColumns.Image = ContextResources.ctxAppendColumn;
		cmdAppendColumns.Visible = flag;
	}

	private void CmdAppendRows_Click(object sender, ClickEventArgs e)
	{
		AppendRows();
	}

	private void CmdAppendRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAppendRows.Text = "追加行...";
		cmdAppendRows.Image = ContextResources.ctxAppendRow;
	}

	private void CmdDataTypeString_Click(object sender, ClickEventArgs e)
	{
		SetDataFormatText();
	}

	private void CmdDataTypeString_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdDataTypeString.Text = "文本格式";
	}

	private void CmdDataType_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdDataType.Enabled = HasSchemaPermission();
		cmdDataType.Text = "数据格式";
		if (_grid.BodyCol < 0 || _grid.BodyRow < 0)
		{
			cmdDataType.Visible = false;
		}
		else
		{
			cmdDataType.Visible = !Table.Columns[_grid.BodyCol].IsLocked && CanEditColumn(Table.Columns[_grid.BodyCol]);
		}
	}

	private void CmdSetColumnWidth_Click(object sender, ClickEventArgs e)
	{
		if (_grid.Col >= 0 && _grid.Col < _grid.Cols.Count)
		{
			int widthDisplay = _grid.Cols[_grid.Col].WidthDisplay;
			decimal? num = InputForm.Numeric("设置列宽", "请输入列宽", widthDisplay);
			if (num.HasValue)
			{
				SetColumnWidth((int)num.Value);
			}
		}
	}

	private void CmdSetColumnWidth_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSetColumnWidth.Enabled = HasSchemaPermission();
		cmdSetColumnWidth.Text = "设置列宽...";
	}

	private void CmdRemoveColumns_Click(object sender, ClickEventArgs e)
	{
		RemoveColumns();
	}

	private void CmdRemoveColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool flag = SoftwareLicenseManager.IsAllowModifyTableStruct();
		cmdRemoveColumns.Enabled = flag && HasSchemaPermission();
		cmdRemoveColumns.Text = "删除列";
		cmdRemoveColumns.Image = ContextResources.ctxDeleteColumn;
		cmdRemoveColumns.Visible = flag;
	}

	private void CmdInsertColumns_Click(object sender, ClickEventArgs e)
	{
		InsertColumns();
	}

	private void CmdInsertColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool flag = SoftwareLicenseManager.IsAllowModifyTableStruct();
		cmdInsertColumns.Enabled = flag && HasSchemaPermission();
		cmdInsertColumns.Text = "插入列...";
		cmdInsertColumns.Image = ContextResources.ctxInsertColumn;
		cmdInsertColumns.Visible = flag;
	}

	private void CmdCancelSubtotal_Click(object sender, ClickEventArgs e)
	{
		CancelSubtotal();
	}

	private void CmdCancelSubtotal_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCancelSubtotal.Text = "取消汇总";
		e.Visible = Table.Rows.Any((Auditai.Model.Row r) => r.Role == RowRole.Subtotal || r.Role == RowRole.Total);
		if ((Table.RowOwnerLoad || Table.RowOwnerExclusive) && !Table.IsManager())
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private void CmdSubtotal_Click(object sender, ClickEventArgs e)
	{
		Subtotal();
	}

	private void CmdSubtotal_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSubtotal.Text = "分类汇总";
		cmdSubtotal.Image = ContextResources.ctxSubtotal;
		if ((Table.RowOwnerLoad || Table.RowOwnerExclusive) && !Table.IsManager())
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private void CmdSubtotalTable_Click(object sender, ClickEventArgs e)
	{
		if (SoftwareLicenseManager.IsProjectHierarchyTreeNodesCountOutOfLimit())
		{
			return;
		}
		TreeTableNode treeTableNode = ((!Table.TreeNode.IsRoot) ? Table.TreeNode.Parent.InsertChildTable(Table.TreeNode.Index + 1, InitTableMode.Empty) : Table.TreeNode.Group.InsertRootTable(Table.TreeNode.Index + 1, InitTableMode.Empty));
		Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
		treeTableNode.Name = Table.TreeNode.Name + "分类汇总表（按" + column.CaptionDisplay + "）";
		List<Auditai.Model.Column> list = Table.Columns.Where((Auditai.Model.Column c) => c.Style?.DataType == typeof(double)).ToList();
		Auditai.Model.Table table = treeTableNode.Table;
		table.Title.TitleCell.Value = treeTableNode.Name;
		TableStyleBrushImpl(Table, table);
		table.HeaderHeights = new int[1] { table.Rows.DefaultHeight };
		table.Columns.Append(1 + list.Count);
		Auditai.Model.Column column2 = table.Columns[0];
		column2.UpdateFormula($"Distinct([2:{Table.Id}:{column.Id}])");
		column2.UpdateCaption(column.Caption);
		Auditai.Model.CellStyle oldStyle = column.Style;
		if (oldStyle != null)
		{
			table.Columns[0].UpdateStyle(table.CellStyles.MutateAndGet(table.DefaultStyle, delegate(Auditai.Model.CellStyle cs)
			{
				cs.DataType = oldStyle.DataType;
				cs.Format = oldStyle.Format;
			}));
		}
		for (int i = 0; i < list.Count; i++)
		{
			column2 = table.Columns[1 + i];
			column2.UpdateFormula($"SumIf([2:{Table.Id}:{column.Id}]=[4:{table.Id}:{table.Columns[0].Id}],[2:{Table.Id}:{list[i].Id}])");
			column2.UpdateCaption(list[i].Caption);
			oldStyle = list[i].Style;
			if (oldStyle != null)
			{
				column2.UpdateStyle(table.CellStyles.MutateAndGet(table.DefaultStyle, delegate(Auditai.Model.CellStyle cs)
				{
					cs.DataType = oldStyle.DataType;
					cs.Format = oldStyle.Format;
					cs.Align = CellTextAlign.MiddleRight;
				}));
			}
		}
		ProjectHierarchy.TreeGroupView currentGroup = _owner.ProjectHierarchy._currentGroup;
		currentGroup.Grid.Row = currentGroup.Grid.Rows[currentGroup.Grid.Row].Node.AddNode(NodeTypeEnum.NextSibling, treeTableNode.Name, treeTableNode, Auditai.UI.Platform.Properties.Resources.TreeTable).Row.Index;
	}

	private void CmdSubtotalTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSubtotalTable.Text = "生成分类汇总表";
		bool flag = !SoftwareLicenseManager.IsAddFileOutOfLicenseLimit();
		if (flag && _grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			flag = false;
		}
		cmdSubtotalTable.Visible = flag;
	}

	private void CmdSortDescending_Click(object sender, ClickEventArgs e)
	{
		SortDescending();
	}

	private void CmdSortDescending_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSortDescending.Text = "降序排序";
		cmdSortDescending.Image = ContextResources.ctxDescending;
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		cmdSortDescending.Visible = visible;
	}

	private void CmdSortAscending_Click(object sender, ClickEventArgs e)
	{
		SortAscending();
	}

	private void CmdSortAscending_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSortAscending.Text = "升序排序";
		cmdSortAscending.Image = ContextResources.ctxAscending;
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		cmdSortAscending.Visible = visible;
	}

	private void CmdSortAscending2_Click(object sender, ClickEventArgs e)
	{
		_grid.BeginUpdate();
		Table[_grid.BodyRow, _grid.BodyCol].SortAscending();
		PopulateRows();
		_grid.EndUpdate();
	}

	private void CmdSortDescending2_Click(object sender, ClickEventArgs e)
	{
		_grid.BeginUpdate();
		Table[_grid.BodyRow, _grid.BodyCol].SortDescending();
		PopulateRows();
		_grid.EndUpdate();
	}

	private void CmdPasteFormula_Click(object sender, ClickEventArgs e)
	{
		PasteFormula();
	}

	private void CmdPasteFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			cmdPasteFormula.Visible = false;
			return;
		}
		cmdPasteFormula.Visible = true;
		cmdPasteFormula.Text = "粘贴公式";
		cmdPasteFormula.Enabled = HasSchemaPermission();
	}

	private void CmdPasteFormat_Click(object sender, ClickEventArgs e)
	{
		PasteFormat();
	}

	private void CmdPasteFormat_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdPasteFormat.Text = "粘贴格式";
	}

	private async void CmdPasteValue_Click(object sender, ClickEventArgs e)
	{
		await PasteValue();
	}

	private void CmdPasteValue_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdPasteValue.Text = "粘贴值";
	}

	private void CmdPaste_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdPaste.Text = "粘贴";
		cmdPaste.Image = ContextResources.ctxPaste;
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Copy();
	}

	private void CmdCopy_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCopy.Text = "复制";
		cmdCopy.Image = ContextResources.ctxCopy;
	}

	private void CmdCut_Click(object sender, ClickEventArgs e)
	{
		Cut();
	}

	private void CmdCut_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCut.Text = "剪切";
		cmdCut.Image = ContextResources.ctxCut;
	}

	private void CmdBlankFill_Click(object sender, ClickEventArgs e)
	{
		BlankFill();
	}

	private void CmdBlankFill_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdBlankFill.Text = "补空填充";
	}

	private void CmdSequenceFill_Click(object sender, ClickEventArgs e)
	{
		SequenceFill();
	}

	private void CmdSequenceFill_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSequenceFill.Text = "序列填充";
	}

	private void CmdCopyFill_Click(object sender, ClickEventArgs e)
	{
		CopyFill();
	}

	private void CmdCopyFill_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCopyFill.Text = "复制填充";
	}

	private void CmdRemoveColHeaderFormula_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			Table.Columns[i].UpdateCaptionFormula(string.Empty);
		}
		AfterSelChange_ColHeaderFormula();
	}

	private void CmdRemoveFormula_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				Auditai.Model.Cell cell = _table[i, j];
				if (CanEditRow(cell.Row) && CanEditColumn(cell.Column))
				{
					cell.UpdateFormula(string.Empty);
				}
			}
		}
		SetFormulaContext();
	}

	private void CmdPasteColumnFormula_Click(object sender, ClickEventArgs e)
	{
		PasteColumnFormula();
	}

	private void CmdPasteColumnFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = CopiedColumn != null && CopiedColumn.Status != SyncStatus.LocalDeleted;
		}
		e.Enabled = HasSchemaPermission();
	}

	private void CmdCopyColumnFormula_Click(object sender, ClickEventArgs e)
	{
		CopyColumnFormula();
	}

	private void CmdCopyColumnFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool flag = SoftwareLicenseManager.IsAllowModifyTableStruct();
		Auditai.Model.Column column = Table.Columns[_grid.BodySelection.LeftCol];
		e.Visible = flag && column.HasFormula;
	}

	private void CmdRemoveFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			cmdRemoveFormula.Visible = false;
		}
		else
		{
			cmdRemoveFormula.Visible = true;
		}
		cmdRemoveFormula.Enabled = HasSchemaPermission();
		cmdRemoveFormula.Text = "删除单元格公式";
		cmdRemoveFormula.Image = ContextResources.ctxDeleteFormula;
	}

	private void CmdRemoveColumnFormula_Click(object sender, ClickEventArgs e)
	{
		for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
		{
			_table.Columns[i].UpdateFormula(string.Empty);
		}
		ClearColumnAllowManualInputFlag(_grid.BodySelection, isReCalcTable: false);
		SetFormulaContext();
	}

	private void CmdRemoveColumnFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRemoveColumnFormula.Enabled = HasSchemaPermission();
		cmdRemoveColumnFormula.Text = "删除列公式";
		cmdRemoveColumnFormula.Image = ContextResources.ctxDeleteFormula;
		if (_grid.BodyCol < 0)
		{
			cmdRemoveColumnFormula.Visible = false;
		}
		else
		{
			Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
			cmdRemoveColumnFormula.Visible = !column.IsLocked && CanEditColumn(column) && column.HasFormula;
		}
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			cmdRemoveColumnFormula.Visible = false;
		}
	}

	private void CmdEditFormula_Click(object sender, ClickEventArgs e)
	{
		EnterFormula();
	}

	private void CmdEditFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdEditFormula.Text = "编辑公式";
	}

	private void CmdMergeEveryHorizontalCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			cmdMergeEveryHorizontalCells.Visible = false;
			return;
		}
		cmdMergeEveryHorizontalCells.Enabled = HasSchemaPermission();
		cmdMergeEveryHorizontalCells.Text = "仅横向合并单元格";
	}

	private void CmdMergeEveryHorizontalCells_Click(object sender, ClickEventArgs e)
	{
		MergeEveryHorizontalCells();
	}

	private void CmdMergeCells_Click(object sender, ClickEventArgs e)
	{
		MergeCells();
	}

	private void CmdMergeCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			cmdMergeCells.Visible = false;
			return;
		}
		cmdMergeCells.Visible = true;
		cmdMergeCells.Enabled = HasSchemaPermission();
		cmdMergeCells.Text = "合并单元格";
	}

	private void CmdUnmergeCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			cmdUnmergeCells.Visible = false;
			return;
		}
		cmdUnmergeCells.Visible = true;
		cmdUnmergeCells.Enabled = HasSchemaPermission();
	}

	private void CmdUnmergeCells_Click(object sender, ClickEventArgs e)
	{
		UnmergeCells();
	}

	private void CmdLockCells_Click(object sender, ClickEventArgs e)
	{
		LockCells();
	}

	private void CmdLockCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdLockCells.Enabled = HasSchemaPermission();
		cmdLockCells.Text = "锁定单元格";
		cmdLockCells.Image = ContextResources.ctxLockCell;
	}

	private void CmdCancelSumColumns_Click(object sender, ClickEventArgs e)
	{
		CancelSumColumns();
	}

	private void CmdCancelSumColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCancelSumColumns.Text = "取消合计行";
		e.Visible = Table.Rows.Any((Auditai.Model.Row r) => r.Role == RowRole.Total);
		if ((Table.RowOwnerLoad || Table.RowOwnerExclusive) && !Table.IsManager())
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private void CmdSumColumns_Click(object sender, ClickEventArgs e)
	{
		SumColumns();
	}

	private void CmdSumColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdSumColumns.Text = "生成合计行";
		cmdSumColumns.Image = ContextResources.ctxTotal;
		if ((Table.RowOwnerLoad || Table.RowOwnerExclusive) && !Table.IsManager())
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private void CmdSumHeaderCells_Click(object sender, ClickEventArgs e)
	{
		SumColumnsHeaderCell();
	}

	private void CmdLockRows_Click(object sender, ClickEventArgs e)
	{
		LockRows();
	}

	private void CmdLockRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdLockRows.Text = "锁定行";
		cmdLockRows.Image = ContextResources.ctxLockCell;
	}

	private void CmdLockColumns_Click(object sender, ClickEventArgs e)
	{
		LockColumns();
	}

	private void CmdLockColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdLockColumns.Text = "锁定列";
		cmdLockColumns.Image = ContextResources.ctxLockCell;
		if (_grid.BodyCol < 0)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = !Table.Columns[_grid.BodyCol].IsLocked;
		}
	}

	private void CmdShowColumns_Click(object sender, ClickEventArgs e)
	{
		ShowColumnsDialog();
	}

	private void CmdShowColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdShowColumns.Text = "寻回列";
		if (Table.Columns.Any((Auditai.Model.Column c) => !c.Visible))
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdHideColumns_Click(object sender, ClickEventArgs e)
	{
		HideColumns();
	}

	private void CmdHideColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdHideColumns.Enabled = HasSchemaPermission();
		cmdHideColumns.Text = "隐藏列";
	}

	private void CmdUnfreezeColumn_Click(object sender, ClickEventArgs e)
	{
		_grid.Cols.Frozen = 0;
		Table.UpdateFrozenCols(0);
	}

	private void CmdUnfreezeColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdUnfreezeColumn.Text = "解冻列";
		if (_grid.BodyCol < _grid.Cols.Frozen)
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdFreezeColumn_Click(object sender, ClickEventArgs e)
	{
		int bodyCol = _grid.BodyCol;
		_grid.Cols.Frozen = bodyCol + 1;
		Table.UpdateFrozenCols(bodyCol + 1);
	}

	private void CmdFreezeColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdFreezeColumn.Text = "冻结列";
		cmdFreezeColumn.Image = Auditai.UI.Platform.Properties.Resources.ctxFreezeCol;
		if (_grid.BodyCol == _grid.BodyColSel)
		{
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdFixedColumnCaption_Click(object sender, ClickEventArgs e)
	{
		SetHeaderMode(TableHeaderMode.Fixed);
	}

	private void CmdFixedColumnCaption_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdFixedColumnCaption.Text = "字母序列头模式";
	}

	private void CmdCustomColumnCaption_Click(object sender, ClickEventArgs e)
	{
		SetHeaderMode(TableHeaderMode.Custom);
	}

	private void CmdCustomColumnCaption_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCustomColumnCaption.Text = "自定制列头模式";
	}

	private void CmdUnlockColumns_Click(object sender, ClickEventArgs e)
	{
		UnlockColumns();
	}

	private void CmdUnlockColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdUnlockColumns.Image = ContextResources.ctxUnlockCell;
		cmdUnlockColumns.Text = "解锁列";
		if (_grid.BodyCol < 0)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = Table.Columns[_grid.BodyCol].IsLocked;
		}
	}

	private void CmdUnlockRows_Click(object sender, ClickEventArgs e)
	{
		UnlockRows();
	}

	private void CmdUnlockRows_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdUnlockRows.Image = ContextResources.ctxUnlockCell;
		cmdUnlockRows.Text = "解锁行";
	}

	private void CmdUnlockCells_Click(object sender, ClickEventArgs e)
	{
		UnlockCells();
	}

	private void CmdUnlockCells_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdUnlockCells.Image = ContextResources.ctxUnlockCell;
		cmdUnlockCells.Text = "解锁单元格";
		cmdUnlockCells.Enabled = HasSchemaPermission();
	}

	private async void CmdCalculateTable_Click(object sender, ClickEventArgs e)
	{
		FillDefaultValues();
		if (Table.ConsolidateSettings.Sources.Count > 0)
		{
			await ExecuteConsolidate(showDataCols: true);
		}
		CalcCurrentTable();
	}

	private void CmdCalculateTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCalculateTable.Text = "运算表格";
		cmdCalculateTable.Image = ContextResources.ctxCalculateTable;
	}

	private void CmdAuxEdit_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAuxEdit.Enabled = HasSchemaPermission();
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			visible = false;
		}
		cmdAuxEdit.Visible = visible;
	}

	private void CmdAuxEdit_Click(object sender, ClickEventArgs e)
	{
		SetComboListDialog();
	}

	private void CmdEditComment_Click(object sender, ClickEventArgs e)
	{
		SetEditCommentDialog();
	}

	private void CmdEditComment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdEditComment.Enabled = HasSchemaPermission();
		bool visible = true;
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			visible = false;
		}
		cmdEditComment.Visible = visible;
	}

	private void CmdLedgerCollectFormulaEdit_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdLedgerCollectFormulaEdit.Enabled = HasSchemaPermission();
		bool flag = Program.ClientPlatformType != PlatformType.AuditPlatform;
		if (Program.ClientPlatformType == PlatformType.AuditPlatform && Auditai.Model.User.Current.IsSystemSupporter)
		{
			flag = true;
		}
		if (flag && _grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			flag = false;
		}
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			flag = false;
		}
		cmdLedgerCollectFormulaEdit.Visible = flag;
	}

	private void CmdLedgerCollectFormulaEdit_Click(object sender, ClickEventArgs e)
	{
		SetEditLedgerCollectFormulaDialog();
	}

	private async void CmdCalculateTable2_Click(object sender, ClickEventArgs e)
	{
		FillDefaultValues();
		if (Table.ConsolidateSettings.Sources.Count > 0)
		{
			await ExecuteConsolidate(showDataCols: true);
		}
		CalcCurrentTable();
	}

	private void CmdCalculateTable2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCalculateTable2.Text = "运算表格";
	}

	private async void CmdAutoCollect_Click(object sender, ClickEventArgs e)
	{
		await _owner.AutoImport();
	}

	private void CmdAutoCollect_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAutoCollect.Text = "采账填充";
		cmdAutoCollect.Image = ContextResources.CollectFill;
	}

	private void CmdValidateTable_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ValidateCurrentTable();
	}

	private void CmdValidateTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdValidateTable.Text = "校验表格";
		cmdValidateTable.Image = ContextResources.ctxValidateTable;
	}

	private void CmdValidateTable2_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ValidateCurrentTable();
	}

	private void CmdValidateTable2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdValidateTable2.Text = "校验表格";
	}

	private void CmdLockTable_Click(object sender, ClickEventArgs e)
	{
		if (cmdLockTable.Checked)
		{
			LockTable();
		}
		else
		{
			UnlockTable();
		}
	}

	private void CmdLockTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = true;
		cmdLockTable.Checked = IsTableLocked;
	}

	private void CmdExportTable_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = true;
	}

	private void CmdExportTable_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ExportExcelDialog();
	}

	private void CmdForward_Click(object sender, ClickEventArgs e)
	{
		_owner.Forward();
	}

	private void CmdBack_Click(object sender, ClickEventArgs e)
	{
		_owner.Back();
	}

	private void CmdToolbarTables_Click(object sender, ClickEventArgs e)
	{
		ctxToolbarTables.ShowContextMenu(ToolBar, ToolBar.PointToClient(Cursor.Position));
	}

	private void CmdMakerSign_Click(object sender, ClickEventArgs e)
	{
		MakerSign();
	}

	private void CmdMakerSign_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdMakerSign.Text = "编制签名";
		cmdMakerSign.Image = Auditai.UI.Platform.Properties.Resources.MakerSign;
	}

	private void CmdCheckerSign_Click(object sender, ClickEventArgs e)
	{
		CheckerSign();
	}

	private void CmdCheckerSign_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCheckerSign.Text = "复核签名";
		cmdCheckerSign.Image = Auditai.UI.Platform.Properties.Resources.CheckerSign;
	}

	private void CmdFoot_Click(object sender, ClickEventArgs e)
	{
		if (cmdFoot.Pressed)
		{
			HideFootPane();
		}
		else
		{
			ShowFootPane();
		}
	}

	private void CmdFoot_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdFoot.Text = "表底签名";
		if (Program.MainForm != null)
		{
			cmdFoot.Visible = Program.MainForm.CurrentEdition.EnableLedger;
		}
	}

	public void SwitchToTicketInputMode()
	{
		CmdTicketInputMode_Click(null, ClickEventArgs.Empty);
	}

	private void CmdTicketInputMode_Click(object sender, ClickEventArgs e)
	{
		try
		{
			Program.MainForm.MainPanel.SuspendDrawing();
			Program.MainForm.SetPreviousTableTab();
			_ttpComment.Hide();
			EndEditColHeaders();
			Program.MainForm.SwitchToTicketInputView();
		}
		finally
		{
			Program.MainForm.MainPanel.ResumeDrawing();
		}
	}

	private void CmdDesignTicket_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.TableEditor.Table.Ticket.Level = TicketLevel.Report;
		Program.MainForm.SwitchToTicketDesignView();
	}

	private void CmdDesignTicket_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowDesignTicket())
		{
			e.Visible = false;
		}
		else if (Program.ClientPlatformType == PlatformType.EnterpriseReportPlatform || Program.ClientPlatformType == PlatformType.EnterpriseManagerPlatform || Program.ClientPlatformType == PlatformType.TableDevelopPlatform || Program.ClientPlatformType == PlatformType.ProductionCostAccountingSystem || Program.ClientPlatformType == PlatformType.ContractLedgerManagementSystem || Program.ClientPlatformType == PlatformType.RDExpenseLedgerSystem || Program.ClientPlatformType == PlatformType.SalesOrderManagementSystem || Program.ClientPlatformType == PlatformType.PSIManagementSystem || Program.ClientPlatformType == PlatformType.ProjectLedgerManagementSystem)
		{
			e.Visible = true;
		}
		else if (Program.ClientPlatformType == PlatformType.Custom)
		{
			e.Visible = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_Bool("table_right_menu_bar_show_design_ticket_button", defaultValue: true);
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdHelpCenter_Click(object sender, ClickEventArgs e)
	{
		_owner.ShowHelpSidebar();
	}

	private void CmdHideToolbar_Click(object sender, ClickEventArgs e)
	{
		_owner.ToggleSideToolbar();
	}

	private void CmdHideToolbar_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdHideToolbar.Text = "隐藏侧边栏";
	}

	private void CmdRowRole_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRowRole.Text = "标记为";
		if (_grid.BodyRow < 0)
		{
			e.Enabled = false;
			return;
		}
		if (IsTableLocked)
		{
			e.Enabled = false;
			return;
		}
		int start = _grid.BodyRow;
		int end = _grid.BodyRowSel;
		if (!Table.Rows.Where((Auditai.Model.Row r) => r.Index >= start && r.Index <= end).All((Auditai.Model.Row r) => CanEditRow(r)))
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private void CmdRowAccessAll_Click(object sender, ClickEventArgs e)
	{
		if (cmdRowAccessAll.Checked)
		{
			for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
			{
				Auditai.Model.Row row = Table.Rows[i];
				row.Permissions.Write = new Permission
				{
					GrantAll = true
				};
				row.TagPermissionsDirty();
			}
			return;
		}
		Permission permission = new Permission
		{
			GrantAll = false,
			Users = new List<long>()
		};
		foreach (C1CommandLink commandLink in mnuRowAccess.CommandLinks)
		{
			if (commandLink.Command.Checked && commandLink.Command.UserData is Auditai.DTO.User user)
			{
				permission.Users.Add(user.Id);
			}
		}
		for (int j = _grid.BodyRow; j <= _grid.BodyRowSel; j++)
		{
			Auditai.Model.Row row2 = Table.Rows[j];
			row2.Permissions.Write = permission;
			row2.TagPermissionsDirty();
		}
	}

	private void CmdRowAccessAll_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRowAccessAll.Text = "全体成员";
		cmdRowAccessAll.CheckAutoToggle = true;
		mnuRowAccess.Image = Auditai.UI.Platform.Properties.Resources.AccessControl16;
	}

	private void mnuRowOwnerShare_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = (Table.RowOwnerLoad || Table.RowOwnerExclusive) && _grid.BodyRow == _grid.BodyRowSel && HasSchemaPermission();
		mnuRowOwnerShare.CommandLinks.Add(new C1CommandLink());
	}

	private void CmdCopyHeaderCellFormula_Click(object sender, ClickEventArgs e)
	{
		CopyHeaderCellFormula();
	}

	private void CmdCopyHeaderCellFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool flag = SoftwareLicenseManager.IsAllowEditFormula();
		e.Visible = flag && Table[_grid.BodyRow, _grid.BodyCol].HasHeaderFormula;
	}

	private void CmdRemoveHeaderCellFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
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

	private void CmdRemoveHeaderCellFormula_Click(object sender, ClickEventArgs e)
	{
		if (SoftwareLicenseManager.IsAllowEditFormula())
		{
			int bodyRow = _grid.BodyRow;
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Table[bodyRow, i].UpdateHeaderFormula(string.Empty);
			}
		}
	}

	private void CmdPasteHeaderCellFormula_Click(object sender, ClickEventArgs e)
	{
		PasteHeaderCellFormula();
	}

	private void CmdPasteHeaderCellFormula_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowEditFormula())
		{
			e.Visible = false;
			return;
		}
		if (CopiedHeaderCell == null || CopiedHeaderCell.Status == SyncStatus.LocalDeleted)
		{
			e.Visible = false;
			return;
		}
		Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
		if (CopiedHeaderCell.Row != cell.Row)
		{
			e.Enabled = false;
			return;
		}
		e.Visible = true;
		e.Enabled = true;
	}

	private void MnuRowOwnerShare_Popup(object sender, EventArgs e)
	{
		mnuRowOwnerShare.CommandLinks.Clear();
		List<Member> list = new List<Member>();
		foreach (Auditai.DTO.User item in from kv in Table.Project.Users
			where kv.Value != UserRole.Manager
			select kv.Key)
		{
			Member member = MemberManager.GetInstance().GetMember(item.Id.ToString());
			if (member != null)
			{
				list.Add(member);
			}
		}
		Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
		Member creator = MemberManager.GetInstance().GetMember(row.Creator.ToString());
		if (creator != null)
		{
			foreach (Member user2 in list)
			{
				if (user2.Id != creator.Id)
				{
					C1Command cmd2 = new C1Command
					{
						CheckAutoToggle = true,
						Text = creator.Name + "→" + user2.Name,
						Checked = Table.RowOwnerLoadShare.Exists(long.Parse(creator.Id), long.Parse(user2.Id))
					};
					cmd2.CheckedChanged += delegate
					{
						if (cmd2.Checked)
						{
							Table.RowOwnerLoadShare.Add(long.Parse(creator.Id), long.Parse(user2.Id));
						}
						else
						{
							Table.RowOwnerLoadShare.Remove(long.Parse(creator.Id), long.Parse(user2.Id));
						}
						Table.TagRowOwnerLoadShareDirty();
					};
					C1CommandLink value = new C1CommandLink(cmd2);
					mnuRowOwnerShare.CommandLinks.Add(value);
				}
			}
			return;
		}
		if (!Table.IsManager())
		{
			return;
		}
		Auditai.Model.User current2 = Auditai.Model.User.Current;
		string text = current2.ToString();
		string strRowCreatorId = row.Creator.ToString();
		foreach (Member user in list)
		{
			if (!(user.Id != text))
			{
				continue;
			}
			C1Command cmd = new C1Command
			{
				CheckAutoToggle = true,
				Text = current2.Name + "→" + user.Name,
				Checked = Table.RowOwnerLoadShare.Exists(long.Parse(strRowCreatorId), long.Parse(user.Id))
			};
			cmd.CheckedChanged += delegate
			{
				if (cmd.Checked)
				{
					Table.RowOwnerLoadShare.Add(long.Parse(strRowCreatorId), long.Parse(user.Id));
				}
				else
				{
					Table.RowOwnerLoadShare.Remove(long.Parse(strRowCreatorId), long.Parse(user.Id));
				}
				Table.TagRowOwnerLoadShareDirty();
			};
			C1CommandLink value2 = new C1CommandLink(cmd);
			mnuRowOwnerShare.CommandLinks.Add(value2);
		}
	}

	private void MnuRowAccess_Popup(object sender, EventArgs e)
	{
		mnuRowAccess.CommandLinks.Clear();
		mnuRowAccess.CommandLinks.Add(lnkRowAccessAll);
		Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
		if (row.Permissions.Write == null || row.Permissions.Write.GrantAll)
		{
			cmdRowAccessAll.Checked = true;
		}
		else
		{
			cmdRowAccessAll.Checked = false;
		}
		foreach (KeyValuePair<Auditai.DTO.User, UserRole> item in _owner.CurrentProject.Users.Where((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Value == UserRole.Assistant || u.Value == UserRole.Checker))
		{
			C1Command c1Command = new C1Command
			{
				Text = item.Key.Name,
				CheckAutoToggle = true,
				UserData = item.Key
			};
			if (row.Permissions.Write == null || row.Permissions.Write.GrantAll)
			{
				c1Command.Checked = false;
			}
			else
			{
				c1Command.Checked = row.Permissions.Write.Users.Contains(item.Key.Id);
			}
			c1Command.Click += delegate
			{
				cmdRowAccessAll.Checked = false;
				Permission permission = new Permission
				{
					GrantAll = false,
					Users = new List<long>()
				};
				foreach (C1CommandLink commandLink in mnuRowAccess.CommandLinks)
				{
					if (commandLink.Command.Checked && commandLink.Command.UserData is Auditai.DTO.User user)
					{
						permission.Users.Add(user.Id);
					}
				}
				for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
				{
					Auditai.Model.Row row2 = Table.Rows[i];
					row2.Permissions.Write = permission;
					row2.TagPermissionsDirty();
				}
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			mnuRowAccess.CommandLinks.Add(value);
		}
	}

	private void MnuRowAccess_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		mnuRowAccess.Text = "编辑权限";
		mnuRowAccess.CloseOnItemClick = false;
		mnuRowAccess.Image = Auditai.UI.Platform.Properties.Resources.AccessControl16;
		e.Visible = _owner.CanAccessControl() && !Table.RowOwnerExclusive && !Table.RowOwnerLoad;
	}

	private void CmdColumnAccessAll_Click(object sender, ClickEventArgs e)
	{
		if (cmdColumnAccessAll.Checked)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				column.Permissions.Write = new Permission
				{
					GrantAll = true
				};
				column.TagPermissionsDirty();
			}
			return;
		}
		Permission permission = new Permission
		{
			GrantAll = false,
			Users = new List<long>()
		};
		foreach (C1CommandLink commandLink in mnuColumnAccess.CommandLinks)
		{
			if (commandLink.Command.Checked && commandLink.Command.UserData is Auditai.DTO.User user)
			{
				permission.Users.Add(user.Id);
			}
		}
		for (int j = _grid.BodyCol; j <= _grid.BodyColSel; j++)
		{
			Auditai.Model.Column column2 = Table.Columns[j];
			column2.Permissions.Write = permission;
			column2.TagPermissionsDirty();
		}
	}

	private void CmdColumnAccessAll_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdColumnAccessAll.Text = "全体成员";
		cmdColumnAccessAll.CheckAutoToggle = true;
	}

	private void MnuColumnAccess_Popup(object sender, EventArgs e)
	{
		mnuColumnAccess.CommandLinks.Clear();
		mnuColumnAccess.CommandLinks.Add(lnkColumnAccessAll);
		Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
		if (column.Permissions.Write == null || column.Permissions.Write.GrantAll)
		{
			cmdColumnAccessAll.Checked = true;
		}
		else
		{
			cmdColumnAccessAll.Checked = false;
		}
		foreach (KeyValuePair<Auditai.DTO.User, UserRole> item in _owner.CurrentProject.Users.Where((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Value == UserRole.Assistant || u.Value == UserRole.Checker))
		{
			C1Command c1Command = new C1Command
			{
				Text = item.Key.Name,
				CheckAutoToggle = true,
				UserData = item.Key
			};
			if (column.Permissions.Write == null || column.Permissions.Write.GrantAll)
			{
				c1Command.Checked = false;
			}
			else
			{
				c1Command.Checked = column.Permissions.Write.Users.Contains(item.Key.Id);
			}
			c1Command.Click += delegate
			{
				cmdColumnAccessAll.Checked = false;
				Permission permission = new Permission
				{
					GrantAll = false,
					Users = new List<long>()
				};
				foreach (C1CommandLink commandLink in mnuColumnAccess.CommandLinks)
				{
					if (commandLink.Command.Checked && commandLink.Command.UserData is Auditai.DTO.User user)
					{
						permission.Users.Add(user.Id);
					}
				}
				for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
				{
					Auditai.Model.Column column2 = Table.Columns[i];
					column2.Permissions.Write = permission;
					column2.TagPermissionsDirty();
				}
			};
			C1CommandLink value = new C1CommandLink(c1Command);
			mnuColumnAccess.CommandLinks.Add(value);
		}
	}

	private void MnuColumnAccess_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		mnuColumnAccess.Text = "编辑权限";
		mnuColumnAccess.CloseOnItemClick = false;
		mnuColumnAccess.Image = Auditai.UI.Platform.Properties.Resources.AccessControl16;
		e.Visible = _owner.CanAccessControl();
	}

	private async void CmdCollectFill_Click(object sender, ClickEventArgs e)
	{
		await _owner.AutoImport();
	}

	private void CmdCollectFill_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCollectFill.Text = "采账填充";
		cmdCollectFill.Image = ContextResources.CollectFill;
	}

	private async void CmdCollectFill2_Click(object sender, ClickEventArgs e)
	{
		await _owner.AutoImport();
	}

	private void CmdCollectFill2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Table != null)
		{
			cmdCollectFill2.Visible = SoftwareLicenseManager.IsLedgerModuleEnable() && Table.Ticket != null && Table.Ticket.IsEmpty() && Program.ClientPlatformType == PlatformType.AuditPlatform;
			cmdCollectFill2.Text = "采账填充";
		}
	}

	private void CmdCellCollect_Click(object sender, ClickEventArgs e)
	{
		_owner.CellCollectSet();
	}

	private void CmdCellCollect_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = Table.Cells.Any((Auditai.Model.Cell c) => !string.IsNullOrEmpty(c.CollectSource));
	}

	private void CmdRowConvertToColHeader_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
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

	private void CmdRowConvertToColHeader_Click(object sender, ClickEventArgs e)
	{
		if (!HasRowConvertPermission())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因您没有当前表格的【" + Table.TreeNode.GetDontHavePermissionString() + "】权限，因此，无法将选定行次内容转换为表格列头。");
			return;
		}
		int topRow = _grid.BodySelection.TopRow;
		int bottomRow = _grid.BodySelection.BottomRow;
		if (bottomRow - topRow + 1 > 10)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"此操作最多选定{10}行。");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < Table.Columns.Count; i++)
		{
			stringBuilder.Clear();
			for (int j = topRow; j <= bottomRow; j++)
			{
				Auditai.Model.Cell cell = Table[j, i];
				Auditai.Model.Cell mergeTopLeftCell = cell.GetMergeTopLeftCell();
				if (mergeTopLeftCell != null && mergeTopLeftCell.Row == cell.Row)
				{
					cell = mergeTopLeftCell;
				}
				string value = cell.GetDisplayValue().Replace("_", "");
				if (!string.IsNullOrWhiteSpace(value))
				{
					stringBuilder.Append(value);
					stringBuilder.Append("_");
				}
			}
			if (stringBuilder.Length >= 1)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			Auditai.Model.Column column = Table.Columns[i];
			column.UpdateCaption(stringBuilder.ToString());
			Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
			Auditai.Model.Cell cell2 = Table[topRow, i];
			captionStyle.FontFamily = cell2.DisplayFontFamily;
			captionStyle.FontSize = cell2.DisplayFontSize;
			captionStyle.Bold = cell2.DisplayBold;
			captionStyle.Italic = cell2.DisplayItalic;
			captionStyle.ForeColor = cell2.DisplayForeColor;
			column.UpdateCaptionStyle(captionStyle);
		}
		Table.UpdateHeaderMode(TableHeaderMode.Custom);
		RemoveRows();
		PopulateTable();
	}

	private void CmdRowConvertToSubtitle_Click(object sender, ClickEventArgs e)
	{
		if (!HasRowConvertPermission())
		{
			string permissionText = Table?.TreeNode != null ? Table.TreeNode.GetDontHavePermissionString() : "未知权限";
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因您没有当前表格的【" + permissionText + "】权限，因此，无法将选定行次内容转换为表格副标题。");
			return;
		}
		if (Table == null || Table.Title == null)
		{
			return;
		}
		int topRow = _grid.BodySelection.TopRow;
		int bottomRow = _grid.BodySelection.BottomRow;
		int num = bottomRow - topRow + 1;
		if (num > 10)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"此操作最多选定{10}行。");
			return;
		}
		if (bottomRow < 0 || bottomRow >= Table.Rows.Count)
		{
			return;
		}
		int lastNonEmptyRow = Table.Title.GetLastNonEmptyRow();
		int num2 = lastNonEmptyRow + 1;
		for (int i = topRow; i <= bottomRow; i++)
		{
			if (Table.Title.Rows.Count <= num2)
			{
				Table.Title.AppendRow(useNextRowStyle: false);
			}
			TableTitleRow tableTitleRow = Table.Title.Rows[num2];
			List<Auditai.Model.Cell> list = (from c in Table.Rows[i].GetCells()
				where !string.IsNullOrWhiteSpace(c.GetDisplayValue())
				select c).ToList();
			int num3 = Math.Min(list.Count, Table.Title.Columns.Count);
			for (int j = 0; j < num3; j++)
			{
				CopyToTitleCell(list[j], tableTitleRow.Cells[j]);
			}
			num2++;
		}
		Table.TagTitleDirty();
		TitleEditor.Populate();
		RemoveRows();
		DoLayout();
	}

	private void CmdExportAttachment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int bodyRow = _grid.BodyRow;
		int bodyCol = _grid.BodyCol;
		if (bodyRow < 0 || bodyCol < 0 || Table == null)
		{
			cmdRemoveAttachment.Visible = false;
			cmdExportAttachment.Visible = false;
			return;
		}
		if (Table.CellPropManager.DicCellAttachments.Count == 0)
		{
			cmdRemoveAttachment.Visible = false;
			cmdExportAttachment.Visible = false;
			return;
		}
		if (_grid.Selection.IsSingleCell)
		{
			cmdRemoveAttachment.Visible = false;
			cmdExportAttachment.Visible = false;
			return;
		}
		int num = 0;
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		bool flag = false;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			int row = i - _grid.Rows.Fixed;
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				int col = j - _grid.Cols.Fixed;
				if (!_grid.IsIndexOutOfRange(i, j))
				{
					if (num++ > 10000)
					{
						flag = true;
						break;
					}
					Auditai.Model.Cell cell = Table[row, col];
					if (Table.CellPropManager.TryGetAttachments(cell, out var _))
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag && Table?.IsLocked == true)
		{
			cmdRemoveAttachment.Visible = false;
			cmdExportAttachment.Visible = true;
		}
		else
		{
			cmdRemoveAttachment.Visible = flag;
			cmdExportAttachment.Visible = flag;
		}
	}

	private async void CmdExportAttachment_Click(object sender, ClickEventArgs e)
	{
		if (Table == null || Table.CellPropManager.DicCellAttachments.Count == 0)
		{
			return;
		}
		List<CellAttachment> list = new List<CellAttachment>();
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			int row = i - _grid.Rows.Fixed;
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				int col = j - _grid.Cols.Fixed;
				if (!_grid.IsIndexOutOfRange(i, j))
				{
					Auditai.Model.Cell cell = Table[row, col];
					if (Table.CellPropManager.TryGetAttachments(cell, out var attachments))
					{
						list.AddRange(attachments.Attachments);
					}
				}
			}
		}
		if (list.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "选中区域内没有任何附件！");
			return;
		}
		try
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "请选择保存附件的文件夹位置：";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				await ExportAttachmentsToFolder(list, folderBrowserDialog.SelectedPath);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "附件导出失败:\r\n" + ex.ToString(), MessageBoxButtons.OK, "错误!");
		}
	}

	private void CmdRemoveAttachment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = cmdRemoveAttachment.Visible;
		e.Enabled = !IsTableLocked;
	}

	private void CmdRemoveAttachment_Click(object sender, ClickEventArgs e)
	{
		if (Table == null || Table.CellPropManager.DicCellAttachments.Count == 0 || Table.IsLocked)
		{
			return;
		}
		bool flag = false;
		bool isTableExistFillFormula = IsTableExistFillFormula();
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			int row = i - _grid.Rows.Fixed;
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				int col = j - _grid.Cols.Fixed;
				if (!_grid.IsIndexOutOfRange(i, j))
				{
					Auditai.Model.Cell cell = Table[row, col];
					if (CanEditCell(cell, isTableExistFillFormula, ignoreProp: true) && Table.CellPropManager.TryGetAttachments(cell, out var _))
					{
						Table.CellPropManager.RemoveAllAttachment(cell);
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			_grid.Invalidate();
		}
	}

	private void CmdAddAttachment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAddAttachment.Visible = !Table.Columns[_grid.BodyCol].IsLocked && CanEditColumn(Table.Columns[_grid.BodyCol]);
	}

	private async void CmdAddAttachment_Click(object sender, ClickEventArgs e)
	{
		await AddAttachment();
	}

	private bool HasRowConvertPermission()
	{
		if (Program.MainForm.CurrentProject.Creator.Id == Auditai.Model.User.Current.Id)
		{
			return true;
		}
		if (!Program.MainForm.CurrentProject.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id))
		{
			return false;
		}
		if (Program.MainForm.CurrentProject.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		if (Table.TreeNode.RowRead || Table.TreeNode.RowWrite)
		{
			return false;
		}
		if (Table.TreeNode.Permissions.CanWrite())
		{
			return Table.TreeNode.Permissions.CanEditSchema();
		}
		return false;
	}

	private void MnuRowConvertTo_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!HasRowConvertPermission())
		{
			e.Enabled = false;
			return;
		}
		if (_grid.BodyRow < 0)
		{
			e.Enabled = false;
			return;
		}
		if (IsTableLocked)
		{
			e.Enabled = false;
			return;
		}
		int start = _grid.BodyRow;
		int end = _grid.BodyRowSel;
		if (!Table.Rows.Where((Auditai.Model.Row r) => r.Index >= start && r.Index <= end).All((Auditai.Model.Row r) => CanEditRow(r)))
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = true;
		}
	}

	private void CmdRowConvertToTitle_Click(object sender, ClickEventArgs e)
	{
		if (!HasRowConvertPermission())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因您没有当前表格的【" + Table.TreeNode.GetDontHavePermissionString() + "】权限，因此，无法将选定行次内容转换为表格主标题。");
			return;
		}
		int topRow = _grid.BodySelection.TopRow;
		int bottomRow = _grid.BodySelection.BottomRow;
		if (bottomRow - topRow + 1 > 10)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"此操作最多选定{10}行。");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = topRow; i <= bottomRow; i++)
		{
			for (int j = 0; j < Table.Columns.Count; j++)
			{
				stringBuilder.Append(Table[i, j].GetDisplayValue());
			}
		}
		CopyToTitleCell(Table[topRow, 0], Table.Title.TitleCell);
		Table.Title.TitleCell.Value = stringBuilder.ToString();
		Table.TagTitleDirty();
		RemoveRows();
		TitleEditor.Populate();
	}

	private void CmdRowConvertToFoot_Click(object sender, ClickEventArgs e)
	{
		if (!HasRowConvertPermission())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "因您没有当前表格的【" + Table.TreeNode.GetDontHavePermissionString() + "】权限，因此，无法将选定行次内容转换为表底签名。");
			return;
		}
		int topRow = _grid.BodySelection.TopRow;
		int bottomRow = _grid.BodySelection.BottomRow;
		int num = bottomRow - topRow + 1;
		if (num > 10)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"此操作最多选定{10}行。");
			return;
		}
		int lastNonEmptyRow = Table.Foot.GetLastNonEmptyRow();
		int num2 = lastNonEmptyRow + 1;
		for (int i = topRow; i <= bottomRow; i++)
		{
			if (Table.Foot.Rows.Count <= num2)
			{
				Table.Foot.AppendRow(useNextRowStyle: false);
			}
			TableTitleRow tableTitleRow = Table.Foot.Rows[num2];
			List<Auditai.Model.Cell> list = (from c in Table.Rows[i].GetCells()
				where !string.IsNullOrWhiteSpace(c.GetDisplayValue())
				select c).ToList();
			int num3 = Math.Min(list.Count, Table.Foot.Columns.Count);
			for (int j = 0; j < num3; j++)
			{
				CopyToTitleCell(list[j], tableTitleRow.Cells[j]);
			}
			num2++;
		}
		Table.TagFootDirty();
		RemoveRows();
		FootEditor.Populate();
		DoLayout();
	}

	private void CmdCopyColumns_Click(object sender, ClickEventArgs e)
	{
		CopyColumns();
		Copy();
	}

	private void CmdCopyColumns_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			e.Visible = false;
		}
		else if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void CmdPasteTableList_Click(object sender, ClickEventArgs e)
	{
		Auditai.Model.Column dstCol = Table.Columns[_grid.BodyCol];
		PasteTableList(CopiedColumns, dstCol);
	}

	private void CmdPasteTableList_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = CopiedColumns.Count > 0 && CopiedColumns[0].Table.Project.Id == Auditai.Model.Project.Current.Id;
	}

	private void CmdPasteTreeList_Click(object sender, ClickEventArgs e)
	{
		Auditai.Model.Column dstCol = Table.Columns[_grid.BodyCol];
		PasteTreeList(CopiedColumns, dstCol);
	}

	private void CmdPasteTreeList_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = CopiedColumns.Count > 0 && CopiedColumns[0].Table.Project.Id == Auditai.Model.Project.Current.Id;
	}

	private void CmdPasteSimpleList_Click(object sender, ClickEventArgs e)
	{
		Auditai.Model.Column srcCol = CopiedColumns[0];
		Auditai.Model.Column dstCol = Table.Columns[_grid.BodyCol];
		PasteSimpleList(srcCol, dstCol);
	}

	private void CmdPasteSimpleList_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = CopiedColumns.Count > 0 && CopiedColumns[0].Table.Project.Id == Auditai.Model.Project.Current.Id;
	}

	private void CmdPasteSumIf_Click(object sender, ClickEventArgs e)
	{
		Auditai.Model.Column srcCol = CopiedColumns[0];
		Auditai.Model.Column dstCol = Table.Columns[_grid.BodyCol];
		PasteAdvanced("粘贴为SumIf（条件汇总）列公式", "SumIf", srcCol, dstCol);
	}

	private void CmdPasteSumIf_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = CopiedColumns.Count > 0 && CopiedColumns[0].Table.Project.Id == Auditai.Model.Project.Current.Id;
	}

	private void CmdPasteVLookUp_Click(object sender, ClickEventArgs e)
	{
		Auditai.Model.Column srcCol = CopiedColumns[0];
		Auditai.Model.Column dstCol = Table.Columns[_grid.BodyCol];
		PasteAdvanced("粘贴为VLookUp（条件找值）列公式", "VLookUp", srcCol, dstCol);
	}

	private void CmdPasteVLookUp_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = CopiedColumns.Count > 0 && CopiedColumns[0].Table.Project.Id == Auditai.Model.Project.Current.Id;
	}

	private void CmdPasteFilter_Click(object sender, ClickEventArgs e)
	{
		Auditai.Model.Column srcCol = CopiedColumns[0];
		Auditai.Model.Column dstCol = Table.Columns[_grid.BodyCol];
		PasteFilter(srcCol, dstCol);
	}

	private void CmdPasteFilter_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		Auditai.Model.Column column = CopiedColumns[0];
		if (string.IsNullOrEmpty(column.Table.FilterInfo))
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void CmdPasteDistinct_Click(object sender, ClickEventArgs e)
	{
		Auditai.Model.Column srcCol = CopiedColumns[0];
		Auditai.Model.Column dstCol = Table.Columns[_grid.BodyCol];
		PasteDistinct(srcCol, dstCol);
	}

	private void CmdPasteDistinct_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = CopiedColumns.Count > 0 && CopiedColumns[0].Table.Project.Id == Auditai.Model.Project.Current.Id;
	}

	private void CmdAdvancedPaste_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_grid.Selection.RightCol - _grid.Selection.LeftCol >= 1)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = CopiedColumns.Count > 0 && CopiedColumns[0].Table.Project.Id == Auditai.Model.Project.Current.Id;
		}
	}

	private void CopyToTitleCell(Auditai.Model.Cell c, TableTitleCell tc)
	{
		tc.Value = c.GetDisplayValue();
		tc.Bold = c.DisplayBold;
		tc.Italic = c.DisplayItalic;
		tc.Underline = c.DisplayUnderline;
		tc.FontFamily = c.DisplayFontFamily;
		tc.FontSize = c.DisplayFontSize;
		tc.ForeColor = c.DisplayForeColor;
		tc.BackColor = c.DisplayBackColor;
	}

	public void AppendRow()
	{
		if (!IsTableLocked && SoftwareLicenseManager.IsAllowAddTableRows() && !SoftwareLicenseManager.IsTableRowsCountOutOfLicenseLimit(Table, 1))
		{
			pnlGrid.SuspendDrawing();
			Table.Rows.Append(1);
			PopulateRows();
			FormulaEvaluator.ClearCache();
			DoLayout();
			pnlGrid.ResumeDrawing();
		}
	}

	public void AppendRows()
	{
		if (IsTableLocked || !SoftwareLicenseManager.IsAllowAddTableRows())
		{
			return;
		}
		decimal? num = InputForm.Numeric("追加多行", "请输入行数：", 1, CheckIsInputValidInt_BigThanZero);
		if (!num.HasValue)
		{
			return;
		}
		int num2 = (int)num.Value;
		if (num2 > 50000)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"一次新增行数不得超过 {50000}");
		}
		else
		{
			if (SoftwareLicenseManager.IsTableRowsCountOutOfLicenseLimit(Table, num2))
			{
				return;
			}
			_grid.BeginUpdate();
			try
			{
				int count = Table.Rows.Count;
				Table.Rows.Append(num2);
				PopulateRows();
				DoLayout();
			}
			catch (TableModelException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			finally
			{
				FormulaEvaluator.ClearCache();
				_grid.EndUpdate();
			}
		}
	}

	public void CopyFill()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		int i;
		for (i = _grid.BodyRow; i < _grid.BodyRowsCount && !_grid.BodyGetRow(i).Visible; i++)
		{
		}
		bool isTableExistFillFormula = IsTableExistFillFormula();
		object value = Table[i, _grid.BodyCol].Value;
		List<Tuple<Auditai.Model.Cell, object>> list = new List<Tuple<Auditai.Model.Cell, object>>();
		for (int j = i; j <= _grid.BodyRowSel; j++)
		{
			if (!_grid.BodyGetRow(j).Visible)
			{
				continue;
			}
			for (int k = _grid.BodyCol; k <= _grid.BodyColSel; k++)
			{
				Auditai.Model.Cell cell = Table[j, k];
				if (CanEditCell(cell, isTableExistFillFormula))
				{
					try
					{
						object item = Convert.ChangeType(value, cell.DisplayDataType);
						list.Add(Tuple.Create(cell, item));
					}
					catch
					{
					}
				}
			}
		}
		BatchCellUpdateValueCommand command = new BatchCellUpdateValueCommand(Table, list)
		{
			IsExistManualInputValue = true
		};
		Table.CommandsManager.ExecuteCommand(command);
		_grid.Invalidate();
	}

	public void SequenceFill()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (bodySelection.BottomRow <= bodySelection.TopRow)
		{
			return;
		}
		List<Tuple<Auditai.Model.Cell, object>> list = new List<Tuple<Auditai.Model.Cell, object>>();
		int leftCol = bodySelection.LeftCol;
		int i;
		for (i = bodySelection.TopRow; i < _grid.BodyRowsCount && !_grid.BodyGetRow(i).Visible; i++)
		{
		}
		Auditai.Model.Cell cell = Table[i, leftCol];
		if (cell == null) return;
		object value = cell.Value;
		int j;
		for (j = i + 1; j < _grid.BodyRowsCount && !_grid.BodyGetRow(j).Visible; j++)
		{
		}
		bool isTableExistFillFormula = IsTableExistFillFormula();
		Auditai.Model.Cell cell2 = Table[j, leftCol];
		object value2 = cell2.Value;
		bool flag;
		DateTime dateTime2;
		int num5;
		int num6;
		int num7;
		int num8;
		if (value is double num && value2 is double num2)
		{
			double num3 = num2;
			double num4 = num2 - num;
			for (int k = j + 1; k <= bodySelection.BottomRow; k++)
			{
				if (!_grid.BodyGetRow(k).Visible)
				{
					continue;
				}
				num3 += num4;
				Auditai.Model.Cell cell3 = Table[k, leftCol];
				if (CanEditCell(cell3, isTableExistFillFormula))
				{
					try
					{
						object item = Convert.ChangeType(num3, cell3.DisplayDataType);
						list.Add(Tuple.Create(cell3, item));
					}
					catch
					{
					}
				}
			}
		}
		else if (value is DateTime || value is DateYearMonth)
		{
			flag = false;
			DateTime dateTime3;
			if (value is DateTime dateTime)
			{
				dateTime2 = dateTime;
				dateTime3 = dateTime;
			}
			else
			{
				if (!(value is DateYearMonth dateYearMonth))
				{
					goto IL_0530;
				}
				dateTime2 = dateYearMonth.Date;
				dateTime3 = dateYearMonth.Date;
				flag = true;
			}
			num5 = 0;
			num6 = 0;
			num7 = 0;
			num8 = j + 1;
			if (value2 is DateTime || value2 is DateYearMonth)
			{
				if (!(value.GetType() != value2.GetType()))
				{
					DateTime dateTime5;
					if (value2 is DateTime dateTime4)
					{
						dateTime5 = dateTime4;
					}
					else
					{
						if (!(value2 is DateYearMonth dateYearMonth2))
						{
							goto IL_0530;
						}
						dateTime5 = dateYearMonth2.Date;
					}
					dateTime2 = dateTime5;
					switch (cell2.DisplayFormat.FormatType)
					{
					case DataFormatType.DateSlash:
					case DataFormatType.DateDash:
					case DataFormatType.DateChinese:
					case DataFormatType.DateDot:
						if (dateTime3.Day != dateTime5.Day)
						{
							num5 = dateTime5.Day - dateTime3.Day;
						}
						else if (dateTime3.Month != dateTime5.Month)
						{
							num6 = dateTime5.Month - dateTime3.Month;
						}
						else if (dateTime3.Year != dateTime5.Year)
						{
							num7 = dateTime5.Year - dateTime3.Year;
						}
						break;
					case DataFormatType.DateYearMonthChinese:
					case DataFormatType.DateYearMonthDash:
					case DataFormatType.DateYearMonthSlash:
					case DataFormatType.DateYearMonthDot:
						if (dateTime3.Month != dateTime5.Month)
						{
							num6 = dateTime5.Month - dateTime3.Month;
						}
						else if (dateTime3.Year != dateTime5.Year)
						{
							num7 = dateTime5.Year - dateTime3.Year;
						}
						break;
					}
					goto IL_03a7;
				}
			}
			else if (value2 is string value3 && string.IsNullOrWhiteSpace(value3))
			{
				num8 = j;
				switch (cell2.DisplayFormat.FormatType)
				{
				case DataFormatType.DateSlash:
				case DataFormatType.DateDash:
				case DataFormatType.DateChinese:
				case DataFormatType.DateDot:
					num5 = 1;
					break;
				case DataFormatType.DateYearMonthChinese:
				case DataFormatType.DateYearMonthDash:
				case DataFormatType.DateYearMonthSlash:
				case DataFormatType.DateYearMonthDot:
					num6 = 1;
					break;
				}
				goto IL_03a7;
			}
		}
		else if (!(value2 is DateTime) && !(value2 is DateYearMonth))
		{
			SequenceInfo sequenceInfo = SequenceInfo.GetSequenceInfo(value.ToString(), value2.ToString());
			if (sequenceInfo != null)
			{
				for (int l = j; l <= bodySelection.BottomRow; l++)
				{
					if (!_grid.BodyGetRow(l).Visible)
					{
						continue;
					}
					Auditai.Model.Cell cell4 = Table[l, leftCol];
					if (CanEditCell(cell4, isTableExistFillFormula))
					{
						try
						{
							object item2 = Convert.ChangeType(sequenceInfo.GetNth(l - i), cell4.DisplayDataType);
							list.Add(Tuple.Create(cell4, item2));
						}
						catch
						{
						}
					}
				}
			}
		}
		goto IL_0530;
		IL_0530:
		BatchCellUpdateValueCommand command = new BatchCellUpdateValueCommand(Table, list)
		{
			IsExistManualInputValue = true
		};
		Table.CommandsManager.ExecuteCommand(command);
		_grid.Invalidate();
		return;
		IL_03a7:
		if (num5 == 0 && num6 == 0 && num7 == 0)
		{
			num8 = j;
			if (value is DateTime)
			{
				num5 = 1;
			}
			else
			{
				if (!(value is DateYearMonth))
				{
					goto IL_0530;
				}
				num6 = 1;
			}
		}
		DateTime dateTime6 = dateTime2;
		for (int m = num8; m <= bodySelection.BottomRow; m++)
		{
			if (!_grid.BodyGetRow(m).Visible)
			{
				continue;
			}
			Auditai.Model.Cell cell5 = Table[m, leftCol];
			if (!CanEditCell(cell5, isTableExistFillFormula))
			{
				continue;
			}
			try
			{
				if (num5 != 0)
				{
					dateTime6 = dateTime6.AddDays(num5);
				}
				if (num6 != 0)
				{
					dateTime6 = dateTime6.AddMonths(num6);
				}
				if (num7 != 0)
				{
					dateTime6 = dateTime6.AddYears(num7);
				}
				object item3 = ((!flag) ? ((object)dateTime6) : ((object)new DateYearMonth(dateTime6)));
				list.Add(Tuple.Create(cell5, item3));
			}
			catch
			{
			}
		}
		goto IL_0530;
	}

	public void BlankFill()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		int i;
		for (i = _grid.BodyRow; i < _grid.BodyRowsCount && !_grid.BodyGetRow(i).Visible; i++)
		{
		}
		bool isTableExistFillFormula = IsTableExistFillFormula();
		object[] array = Enumerable.Repeat((object)"", _grid.BodyColSel - _grid.BodyCol + 1).ToArray();
		List<Tuple<Auditai.Model.Cell, object>> list = new List<Tuple<Auditai.Model.Cell, object>>();
		for (int j = i; j <= _grid.BodyRowSel; j++)
		{
			if (!_grid.BodyGetRow(j).Visible)
			{
				continue;
			}
			for (int k = _grid.BodyCol; k <= _grid.BodyColSel; k++)
			{
				object value = array[k - _grid.BodyCol];
				Auditai.Model.Cell cell = Table[j, k];
				if ("".Equals(cell.Value))
				{
					if (CanEditCell(cell, isTableExistFillFormula))
					{
						try
						{
							object item = Convert.ChangeType(value, cell.DisplayDataType);
							list.Add(Tuple.Create(cell, item));
						}
						catch
						{
						}
					}
				}
				else
				{
					array[k - _grid.BodyCol] = cell.Value;
				}
			}
		}
		BatchCellUpdateValueCommand command = new BatchCellUpdateValueCommand(Table, list)
		{
			IsExistManualInputValue = true
		};
		Table.CommandsManager.ExecuteCommand(command);
		_grid.Invalidate();
	}

	public static bool CheckIsInputValidInt_BigThanZero(string strExp)
	{
		if (int.TryParse(strExp, out var result) && result > 0)
		{
			return true;
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "不是有效的数值！", MessageBoxButtons.OK, "错误");
		return false;
	}

	public void InsertRows()
	{
		if (_grid.BodyRow < 0 || IsTableLocked || !SoftwareLicenseManager.IsAllowAddTableRows())
		{
			return;
		}
		decimal? num = InputForm.Numeric("插入多行", "请输入行数：", 1, CheckIsInputValidInt_BigThanZero);
		if (!num.HasValue)
		{
			return;
		}
		if (num.Value > 50000m)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"一次新增行数不得超过 {50000}");
		}
		else
		{
			if (SoftwareLicenseManager.IsTableRowsCountOutOfLicenseLimit(Table, (int)num.Value))
			{
				return;
			}
			pnlGrid.SuspendDrawing();
			try
			{
				int bodyRow = _grid.BodyRow;
				int count = (int)num.Value;
				Table.Rows.Insert(bodyRow, count);
				_grid.QuickInsertRows(bodyRow, count);
				int num2 = bodyRow;
				while (true)
				{
					decimal num3 = num2;
					decimal value = bodyRow;
					decimal? num4 = num;
					decimal? num5 = (decimal?)value + num4;
					if (!((num3 < num5.GetValueOrDefault()) & num5.HasValue))
					{
						break;
					}
					PopulateRow(Table.Rows[num2], _grid.BodyGetRow(num2));
					num2++;
				}
				PopulateMerges();
				DoLayout();
			}
			catch (TableModelException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			finally
			{
				FormulaEvaluator.ClearCache();
				pnlGrid.ResumeDrawing();
			}
		}
	}

	public void RemoveRows()
	{
		if (_grid.BodyRow < 0 || IsTableLocked)
		{
			return;
		}
		int start = _grid.BodyRow;
		int end = _grid.BodyRowSel;
		if (!Table.Rows.Where((Auditai.Model.Row r) => r.Index >= start && r.Index <= end).All((Auditai.Model.Row r) => CanEditRow(r) && !r.IsLocked && !Table.ControlLockRows.Contains(r) && Table.IsControlFormulaAllowEditRow(r)))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "要删除的行包含您无权删除的行");
			return;
		}
		IEnumerable<Auditai.Model.Cell> source = Table.Cells.Where((Auditai.Model.Cell c) => c.Row.Index >= start && c.Row.Index <= end);
		List<FormulaDependency> list = Table.Project.FormulaManager.GetCellsReferrer(Table.Id, source.Select((Auditai.Model.Cell c) => c.Id)).ToList();
		if (list.Count > 0)
		{
			FormulaDependency formulaDependency = list[0];
			StringBuilder stringBuilder = new StringBuilder("要删除的行被");
			Auditai.Model.Table table = null;
			if (formulaDependency.HostTable == Table.Id)
			{
				table = Table;
				stringBuilder.Append(" 当前表 ");
			}
			else
			{
				table = Auditai.Model.Project.Current.GetTableById(formulaDependency.HostTable);
				if (table == null)
				{
					stringBuilder.Append("相关表");
				}
				else
				{
					stringBuilder.Append(" {" + table.GetCanonicalName() + "}");
				}
			}
			if (formulaDependency.HostKind == FormulaDependencyObjectKind.Cell)
			{
				Auditai.Model.Cell cellById = table.GetCellById(formulaDependency.HostObject);
				if (cellById == null)
				{
					stringBuilder.Append("相关单元格的单元格运算");
				}
				else
				{
					stringBuilder.Append($"[{cellById.Column.GetUniqueFormulaName()},{cellById.Row.Index + 1}] 的单元格运算");
				}
			}
			else if (formulaDependency.HostKind == FormulaDependencyObjectKind.Column)
			{
				Auditai.Model.Column byId = table.Columns.GetById(formulaDependency.HostObject);
				if (byId == null)
				{
					stringBuilder.Append("相关列的列运算");
				}
				else
				{
					stringBuilder.Append("[" + byId.GetUniqueFormulaName() + "] 的列运算");
				}
			}
			else if (formulaDependency.HostKind == FormulaDependencyObjectKind.ValidationFormula)
			{
				stringBuilder.Append("的校验");
			}
			stringBuilder.Append("公式引用，删除后将导致该公式失效，确定要删除吗？");
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, stringBuilder.ToString(), MessageBoxButtons.OKCancel) != DialogResult.OK)
			{
				return;
			}
		}
		List<CellMerge> list2 = Table.MergedCells.ToList();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			CellMerge cellMerge = list2[num];
			int num2 = cellMerge.TopLeft.Row.Index;
			int num3 = cellMerge.BottomRight.Row.Index;
			bool flag = false;
			if (start <= num2 && end >= num2)
			{
				num2 = end + 1;
				flag = true;
			}
			if (start <= num3 && end >= num3)
			{
				num3 = start - 1;
				flag = true;
			}
			if (flag)
			{
				if (num3 > num2)
				{
					Table.UnmergeCells(num2, cellMerge.TopLeft.Column.Index);
					Table.MergeCells(num2, cellMerge.TopLeft.Column.Index, num3, cellMerge.BottomRight.Column.Index);
				}
				else if (num3 == num2)
				{
					if (cellMerge.TopLeft.Column.Index == cellMerge.BottomRight.Column.Index)
					{
						Table.UnmergeCells(num2, cellMerge.TopLeft.Column.Index);
					}
					else
					{
						Table.UnmergeCells(num2, cellMerge.TopLeft.Column.Index);
						Table.MergeCells(num2, cellMerge.TopLeft.Column.Index, num3, cellMerge.BottomRight.Column.Index);
					}
				}
				else
				{
					Table.UnmergeCells(num2, cellMerge.TopLeft.Column.Index);
				}
			}
		}
		if (AnyHiddenRow(start, end))
		{
			for (int num4 = end; num4 >= start; num4--)
			{
				if (_grid.BodyGetRow(num4).Visible)
				{
					Table.Rows.Remove(num4, 1);
				}
			}
		}
		else
		{
			Table.Rows.Remove(start, end - start + 1);
		}
		FormulaEvaluator.ClearCache();
		PopulateRows();
		PopulateMerges();
		if (_grid.FilterManager.IsFiltering)
		{
			_grid.FilterManager.Execute();
		}
		DoLayout();
	}

	public void MoveUpRows()
	{
		if (_grid.BodyRow >= 0 && !IsTableLocked && _grid.BodyRow > 0)
		{
			Point scrollPosition = _grid.ScrollPosition;
			MoveRows(_grid.BodyRow, _grid.BodyRowSel - _grid.BodyRow + 1, _grid.BodyRow - 1);
			PopulateMerges();
			_grid.BodySelect(_grid.BodyRow - 1, _grid.BodyCol, _grid.BodyRowSel - 1, _grid.BodyColSel);
			_grid.ScrollPosition = scrollPosition;
			FormulaEvaluator.ClearCache();
		}
	}

	public void MoveDownRows()
	{
		if (_grid.BodyRow >= 0 && !IsTableLocked && _grid.BodyRowSel < _grid.BodyRowsCount - 1)
		{
			Point scrollPosition = _grid.ScrollPosition;
			MoveRows(_grid.BodyRow, _grid.BodyRowSel - _grid.BodyRow + 1, _grid.BodyRowSel + 2);
			PopulateMerges();
			_grid.BodySelect(_grid.BodyRow + 1, _grid.BodyCol, _grid.BodyRowSel + 1, _grid.BodyColSel);
			_grid.ScrollPosition = scrollPosition;
			FormulaEvaluator.ClearCache();
		}
	}

	public void MoveRowsToTop()
	{
		if (_grid.BodyRow >= 0 && !IsTableLocked && _grid.BodyRow > 0)
		{
			Point scrollPosition = _grid.ScrollPosition;
			int num = _grid.BodyRowSel - _grid.BodyRow + 1;
			MoveRows(_grid.BodyRow, num, 0);
			PopulateMerges();
			_grid.BodySelect(0, _grid.BodyCol, num - 1, _grid.BodyColSel);
			_grid.ScrollPosition = scrollPosition;
			FormulaEvaluator.ClearCache();
		}
	}

	public void MoveRowsToBottom()
	{
		if (_grid.BodyRow >= 0 && !IsTableLocked && _grid.BodyRowSel < _grid.BodyRowsCount - 1)
		{
			Point scrollPosition = _grid.ScrollPosition;
			int num = _grid.BodyRowSel - _grid.BodyRow + 1;
			MoveRows(_grid.BodyRow, num, Table.Rows.Count);
			PopulateMerges();
			_grid.BodySelect(Table.Rows.Count - num, _grid.BodyCol, Table.Rows.Count - 1, _grid.BodyColSel);
			_grid.ScrollPosition = scrollPosition;
			FormulaEvaluator.ClearCache();
		}
	}

	public bool IsNeedShowTableNavTree()
	{
		if (Table == null || Table.Ticket == null || Table.Title == null)
		{
			return false;
		}
		if (!Table.Ticket.IsEmpty())
		{
			return false;
		}
		if (Table.Title.NavTreeCellIdList == null || Table.Title.NavTreeCellIdList.Count == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsExistValidNavTreeCell()
	{
		if (Table == null || Table.Title == null || Table.Title.NavTreeCellIdList == null || Table.Title.NavTreeCellIdList.Count == 0)
		{
			return false;
		}
		foreach (string navTreeCellId in Table.Title.NavTreeCellIdList)
		{
			if (Table.Title.GetUIRenderCellByCellId(navTreeCellId) != null)
			{
				return true;
			}
		}
		return false;
	}

	public void SelectNavNodeByTableInputValue()
	{
		TableNavGrid.SelectNavNodeByTableInputValue();
	}

	public void ReBuildNavTree(bool isRestoreNavTreeStatusData = true)
	{
		if (Table == null || Table.Title == null)
		{
			Program.MainForm.HideNavigationPanel();
			return;
		}
		TableNavTreeStatusDataCacher.SaveNavTreeStatusData(Table.Id, TableNavGrid);
		if (IsNeedShowTableNavTree())
		{
			Program.MainForm.ShowNavigationPanel();
			TableNavGrid.Table = Table;
			TableNavGrid.Nav = Table.Title.NavTreeCellIdList;
			if (isRestoreNavTreeStatusData)
			{
				Program.MainForm.SuspendNavPanelDrawing();
				TableNavGrid.View.BeginUpdate();
				try
				{
					TableNavGrid.Populate();
					RestoreTableNavTreeStatusData();
					SelectNavNodeByTableInputValue();
					return;
				}
				finally
				{
					TableNavGrid.View.EndUpdate();
					Program.MainForm.ResumeNavPanelDrawing();
				}
			}
			TableNavGrid.Populate();
		}
		else
		{
			Program.MainForm.HideNavigationPanel();
		}
	}

	public void RestoreTableNavTreeStatusData()
	{
		if (TableNavGrid.Table == Table)
		{
			TableNavTreeStatusDataCacher.RestoreNavTreeStatusData(Table.Id, TableNavGrid);
		}
	}

	public void SaveTableNavTreeStatusData()
	{
		if (Table != null && Table.Title.NavTreeCellIdList != null && Table.Title.NavTreeCellIdList.Count > 0 && TableNavGrid.Table == Table)
		{
			TableNavTreeStatusDataCacher.SaveNavTreeStatusData(Table.Id, TableNavGrid);
		}
	}

	public void RecoverHistoryTable(Auditai.Model.Table historyTable)
	{
		Program.MainForm.SuspendMainPanelDrawing();
		Program.MainForm.SuspendNavPanelDrawing();
		Program.MainForm.SuspendNavPanelVisible();
		try
		{
			Table = historyTable;
			if (FormulaEditor != null)
			{
				FormulaEditor.Context.Table = historyTable;
				FormulaEditor.Context.Kind = FormulaContextKind.None;
			}
			Program.MainForm.HideNavigationPanel();
			Program.MainForm.SetOpenModeToTicketMode(Table.TreeNode);
			Program.MainForm.OpenTable();
		}
		finally
		{
			Program.MainForm.ResumeNavPanelVisible();
			Program.MainForm.ResumeNavPanelDrawing();
			Program.MainForm.ResumeMainPanelDrawing();
		}
	}

	public async void PopulateTable()
	{
		if (Table == null)
		{
			return;
		}
		pnlGrid.SuspendDrawing();
		_isUpdatingView = true;
		FormulaEditor.View.SuspendDrawing();
		View.Enabled = true;
		_table.LoadAndReturn();
		if (!_table.LocalExists)
		{
			SetCorruptedView();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格 " + _table.TreeNode.Name + " 未同步到本地，请重新同步。\n若同步后仍无数据，则表格还未同步过，请联系该表格的创建人执行同步。");
			return;
		}
		if (_table.IsCorrupted)
		{
			SetCorruptedView();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格 " + _table.TreeNode.Name + " 数据已损坏，请尝试删除此表格重新编辑。");
			return;
		}
		PopulateToolbar();
		_table.LoadFormulaDependencies();
		if (_table.Rows.Count <= UserConfig.RowsApplyFormulaAuto)
		{
			_table.TryApplyFormula(evalLqDistinct: true);
		}
		_grid.Cols[0].StyleNew.TextAlign = TextAlignEnum.CenterCenter;
		_grid.MergedRanges.Clear();
		_grid.BodyRowsCount = Table.Rows.Count;
		_grid.BodyColsCount = Table.Columns.Count;
		_grid.Cols.Frozen = Table.FrozenCols;
		PopulateRows();
		_grid.Select(-1, -1);
		TitleEditor.Title = _table.Title;
		TitleEditor.Populate();
		FootEditor.Foot = _table.Foot;
		FootEditor.Populate();
		bool showFoot = !UserSet.Config.HideFootRow;
		FootEditor.View.Visible = showFoot;
		cmdFoot.Pressed = showFoot;
		PopulateColumns();
		PopulateMerges();
		ValidationEditor.Enabled = !FormulaEditor.IsEditing;
		if (!Program.MainForm.IsInEditingFormula() && Table != null && Table.Title != null && Table.Title.NavTreeCellIdList != null && Table.Title.NavTreeCellIdList.Count > 0 && Table.Ticket != null && Table.Ticket.IsEmpty())
		{
			TableNavGrid.Table = Table;
			TableNavGrid.Nav = Table.Title.NavTreeCellIdList;
			TableNavGrid.Populate();
			RestoreTableNavTreeStatusData();
			SelectNavNodeByTableInputValue();
		}
		UpdateViewStateForLocker();
		ctxColumn.Enabled = true;
		PopulateRibbon();
		if (!HasSchemaPermission())
		{
			ValidationEditor.Enabled = false;
		}
		if (!ValidationEditor.IsEditing)
		{
			ValidationEditor.ContextTable = _table;
			ValidationEditor.PopulateGrid();
		}
		_grid.BeginUpdate();
		for (int i = 0; i < _grid.BodyRowsCount; i++)
		{
			_grid.BodyGetRow(i).Visible = true;
		}
		_grid.EndUpdate();
		_grid.FilterManager.Filters.Deserialize(Table.FilterInfo);
		_grid.FilterManager.Execute();
		DoLayout();
		PopulateTopLeftCell();
		if (!FormulaEditor.IsEditing)
		{
			if (TreeNodeStateCache.Contains(Table.Id))
			{
				TreeNodeCacheState treeNodeCacheState = TreeNodeStateCache.Get(Table.Id);
				treeNodeCacheState.Kind = TreeNodeCacheKind.Table;
				_grid.ScrollPosition = treeNodeCacheState.ScrollPosition;
				Select(treeNodeCacheState.Selection.Left, treeNodeCacheState.Selection.Top, treeNodeCacheState.Selection.Width, treeNodeCacheState.Selection.Height);
			}
			else
			{
				TreeNodeStateCache.Set(Table.Id, new TreeNodeCacheState
				{
					Kind = TreeNodeCacheKind.Table
				});
				Select(0, 0);
				_grid.ScrollPosition = Point.Empty;
			}
		}
		else if (FormulaEditor.IsEditing && !FormulaEditor.IsFinishingEditing)
		{
			_grid.Select(-1, -1);
		}
		if (FormulaEditor.IsEditing)
		{
			FormulaEditor.View.Enabled = true;
		}
		else if (!ValidationEditor.IsEditing && !_isFormatBrushing)
		{
			_ = _isEditingHeaders;
		}
		_isUpdatingView = false;
		SetFormulaContext();
		BodySelectionChanged_CommentToolTip();
		BodySelectionChanged_Stats();
		FormulaEditor.View.ResumeDrawing();
		pnlGrid.ResumeDrawing();

		// 检查跨项目数据引用更新通知
		try
		{
			var project = Auditai.Model.Project.Current;
			if (project != null && StorageRouter.IsLocalMode)
			{
				int notifyCount = CrossProjectRefSyncNotifier.GetPendingNotificationCount(project.Id);
				if (notifyCount > 0)
				{
					var result = Auditai.UI.Controls.MessageBox.Show(
						MessageBoxIcon.Question,
						$"检测到 {notifyCount} 个跨项目数据引用的来源数据已更新，是否立即刷新？",
						MessageBoxButtons.YesNo,
						"数据更新提示");
					if (result == DialogResult.Yes)
					{
						// 触发刷新
						var manager = new CrossProjectDataRefManager(project);
						var results = await manager.ExecuteAll(Table.Id);
						int success = results.Results.Count(r => r.Success);
						int failed = results.Results.Count(r => !r.Success);
						if (failed > 0)
						{
							Auditai.UI.Controls.MessageBox.Show(
								MessageBoxIcon.Information,
								$"刷新完成：成功 {success} 个，失败 {failed} 个");
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			ex.Log();
		}

		// 应用跨项目数据引用单元格样式
		ApplyCrossProjectRefCellStyles();
	}

	public void PopulateRibbon()
	{
		AppCommands.RowOwnerExclusive.Enabled = CanSetRowOwnerExclusive();
		AppCommands.RowOwnerExclusive.IsPressed = Table.RowOwnerExclusive;
		AppCommands.RowOwnerLoad.Enabled = CanSetRowOwnerExclusive();
		AppCommands.RowOwnerLoad.IsPressed = Table.RowOwnerLoad;
		AppCommands.GenerateBatchFormula.Enabled = HasSchemaPermission();
		AppCommandGroups.Column.Enabled = true;
		AppCommandGroups.DataFormat.Enabled = true;
		AppCommandGroups.AuxEdit.Enabled = true;
		if (!HasSchemaPermission())
		{
			AppCommandGroups.Column.Enabled = false;
			AppCommandGroups.DataFormat.Enabled = false;
			AppCommandGroups.AuxEdit.Enabled = false;
		}
	}

	public void RefreshTableLockShowStatus()
	{
		AppCommands.RowOwnerExclusive.Enabled = CanSetRowOwnerExclusive();
		AppCommands.RowOwnerLoad.Enabled = CanSetRowOwnerExclusive();
		if (Table != null)
		{
			AppCommands.RowOwnerExclusive.IsPressed = Table.RowOwnerExclusive;
			AppCommands.RowOwnerLoad.IsPressed = Table.RowOwnerLoad;
		}
	}

	internal bool HasSchemaPermission()
	{
		return Table?.HasSchemaPermission() == true;
	}

	private void PopulateRerredTables()
	{
		HashSet<Id64> hashSet = new HashSet<Id64>();
		foreach (Auditai.Model.Column column in Table.Columns)
		{
			if (column.HasFormula)
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(column.Formula);
					hashSet.UnionWith(formulaEvaluator.GetReferredTableIds());
				}
				catch (FormulaException)
				{
				}
			}
		}
		foreach (Auditai.Model.Cell cell in Table.Cells)
		{
			if (cell.HasFormula)
			{
				try
				{
					FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(cell.Formula);
					hashSet.UnionWith(formulaEvaluator2.GetReferredTableIds());
				}
				catch (FormulaException)
				{
				}
			}
		}
		List<TreeTableNode> source = Table.Project.GetAllTableNodes().ToList();
		foreach (Auditai.Model.Cell cell2 in Table.Cells)
		{
			string displayValue = cell2.GetDisplayValue();
			List<NodeNumberInfo> list = new List<NodeNumberInfo>();
			string[] array = displayValue.Split(new string[2]
			{
				"|",
				Environment.NewLine
			}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string t in array)
			{
				TreeTableNode treeTableNode = source.FirstOrDefault((TreeTableNode n) => n.Number == t);
				if (treeTableNode != null)
				{
					hashSet.Add(treeTableNode.Id);
				}
			}
		}
		hashSet.Remove(Table.Id);
		cmdToolbarTables.Visible = hashSet.Count > 0;
		ctxToolbarTables.CommandLinks.Clear();
		List<C1CommandLink> list2 = new List<C1CommandLink>();
		foreach (Id64 item2 in hashSet)
		{
			Auditai.Model.Table referredTable = Table.Project.GetTableById(item2);
			if (referredTable != null)
			{
				C1Command c1Command = new C1Command
				{
					Image = Auditai.UI.Platform.Properties.Resources.TreeTable,
					Text = referredTable.TreeNode.Number + " " + referredTable.TreeNode.Name
				};
				c1Command.Click += delegate
				{
					_owner.ProjectHierarchy.FindAndSelectNode(referredTable.TreeNode);
				};
				C1CommandLink item = new C1CommandLink(c1Command);
				list2.Add(item);
			}
		}
		ctxToolbarTables.CommandLinks.AddRange(list2);
	}

	public void PopulateToolbar()
	{
		if (Table != null)
		{
			AppCommands.TicketReport.Visible = UserTeam.Current.Level >= TeamLevel.Standard;
			CmdCollectFill2_CommandStateQuery(cmdCollectFill2, null);
		}
	}

	public void AppendColumns()
	{
		decimal? num = InputForm.Numeric("追加多列", "请输入列数：", 1, CheckIsInputValidInt_BigThanZero);
		if (!num.HasValue)
		{
			return;
		}
		if (num.Value > 50m)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"一次新增列数不得超过 {50}");
		}
		else
		{
			if (SoftwareLicenseManager.IsTableColsCountOutOfLicenseLimit(Table, (int)num.Value))
			{
				return;
			}
			pnlGrid.SuspendDrawing();
			try
			{
				Table.Columns.Append((int)num.Value);
				PopulateColumns();
			}
			catch (TableModelException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			finally
			{
				pnlGrid.ResumeDrawing();
				FormulaEvaluator.ClearCache();
			}
		}
	}

	public void InsertColumns()
	{
		if (_grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		decimal? num = InputForm.Numeric("插入多列", "请输入列数：", 1, CheckIsInputValidInt_BigThanZero);
		if (!num.HasValue)
		{
			return;
		}
		if (num.Value > 50m)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"一次新增列数不得超过 {50}");
		}
		else
		{
			if (SoftwareLicenseManager.IsTableColsCountOutOfLicenseLimit(Table, (int)num.Value))
			{
				return;
			}
			pnlGrid.SuspendDrawing();
			try
			{
				Table.Columns.Insert(_grid.BodyCol, (int)num.Value);
				PopulateColumns();
				PopulateMerges();
				SetFormulaContext();
			}
			catch (TableModelException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			finally
			{
				pnlGrid.ResumeDrawing();
				FormulaEvaluator.ClearCache();
			}
		}
	}

	public void RemoveColumns()
	{
		if (_grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		int start = _grid.BodyCol;
		int end = _grid.BodyColSel;
		if (!Table.Columns.Where((Auditai.Model.Column c) => c.Index >= start && c.Index <= end).All((Auditai.Model.Column c) => CanEditColumn(c) && !c.IsLocked))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "要删除的列包含您无权删除的列");
			return;
		}
		IEnumerable<Auditai.Model.Cell> source = Table.Cells.Where((Auditai.Model.Cell c) => c.Column.Index >= start && c.Column.Index <= end);
		List<FormulaDependency> list = Table.Project.FormulaManager.GetCellsReferrer(Table.Id, source.Select((Auditai.Model.Cell c) => c.Id)).Concat(Table.Project.FormulaManager.GetColumnsReferrer(Table.Id, from c in Table.Columns
			where c.Index >= start && c.Index <= end
			select c.Id)).ToList();
		if (list.Count > 0)
		{
			FormulaDependency formulaDependency = list[0];
			StringBuilder stringBuilder = new StringBuilder("要删除的列被");
			Auditai.Model.Table table = null;
			if (formulaDependency.HostTable == Table.Id)
			{
				table = Table;
				stringBuilder.Append(" 当前表 ");
			}
			else
			{
				table = Auditai.Model.Project.Current.GetTableById(formulaDependency.HostTable);
				if (table == null)
				{
					stringBuilder.Append("相关表");
				}
				else
				{
					stringBuilder.Append(" {" + table.GetCanonicalName() + "}");
				}
			}
			if (formulaDependency.HostKind == FormulaDependencyObjectKind.Cell)
			{
				Auditai.Model.Cell cellById = table.GetCellById(formulaDependency.HostObject);
				if (cellById == null)
				{
					stringBuilder.Append("相关单元格的单元格运算");
				}
				else
				{
					stringBuilder.Append($"[{cellById.Column.GetUniqueFormulaName()},{cellById.Row.Index + 1}] 的单元格运算");
				}
			}
			else if (formulaDependency.HostKind == FormulaDependencyObjectKind.Column)
			{
				Auditai.Model.Column byId = table.Columns.GetById(formulaDependency.HostObject);
				if (byId == null)
				{
					stringBuilder.Append("相关列的列运算");
				}
				else
				{
					stringBuilder.Append("[" + byId.GetUniqueFormulaName() + "] 的列运算");
				}
			}
			else if (formulaDependency.HostKind == FormulaDependencyObjectKind.ValidationFormula)
			{
				stringBuilder.Append("的校验");
			}
			stringBuilder.Append("公式引用，删除后将导致该公式失效，确定要删除吗？");
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, stringBuilder.ToString(), MessageBoxButtons.OKCancel) != DialogResult.OK)
			{
				return;
			}
		}
		List<CellMerge> list2 = Table.MergedCells.ToList();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			CellMerge cellMerge = list2[num];
			int num2 = cellMerge.TopLeft.Column.Index;
			int num3 = cellMerge.BottomRight.Column.Index;
			bool flag = false;
			if (start <= num2 && end >= num2)
			{
				num2 = end + 1;
				flag = true;
			}
			if (start <= num3 && end >= num3)
			{
				num3 = start - 1;
				flag = true;
			}
			if (flag)
			{
				if (num3 > num2)
				{
					Table.UnmergeCells(cellMerge.TopLeft.Row.Index, num2);
					Table.MergeCells(cellMerge.TopLeft.Row.Index, num2, cellMerge.BottomRight.Row.Index, num3);
				}
				else if (num3 == num2)
				{
					if (cellMerge.TopLeft.Row.Index == cellMerge.BottomRight.Row.Index)
					{
						Table.UnmergeCells(cellMerge.TopLeft.Row.Index, num2);
					}
					else
					{
						Table.UnmergeCells(cellMerge.TopLeft.Row.Index, num2);
						Table.MergeCells(cellMerge.TopLeft.Row.Index, num2, cellMerge.BottomRight.Row.Index, num3);
					}
				}
				else
				{
					Table.UnmergeCells(cellMerge.TopLeft.Row.Index, num2);
				}
			}
		}
		if (start < Table.FrozenCols)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "由于删除的列中包含有冻结列，冻结列已重置。");
			Table.UpdateFrozenCols(0);
			_grid.Cols.Frozen = 0;
		}
		pnlGrid.SuspendDrawing();
		Table.Columns.Remove(start, end - start + 1);
		PopulateColumns();
		PopulateMerges();
		pnlGrid.ResumeDrawing();
		SetFormulaContext();
		FormulaEvaluator.ClearCache();
	}

	public void HideColumns()
	{
		if (_grid.BodyCol >= 0 && !IsTableLocked)
		{
			pnlGrid.SuspendDrawing();
			int bodyCol = _grid.BodyCol;
			int bodyColSel = _grid.BodyColSel;
			for (int i = bodyCol; i <= bodyColSel; i++)
			{
				Table.Columns[i].UpdateVisible(visible: false);
			}
			PopulateColumns();
			pnlGrid.ResumeDrawing();
		}
	}

	public void ShowColumnsDialog()
	{
		if (_grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		ShowColumnsSelector showColumnsSelector = new ShowColumnsSelector();
		if (showColumnsSelector.ShowDialog(Table) != DialogResult.OK)
		{
			return;
		}
		pnlGrid.SuspendDrawing();
		foreach (Auditai.Model.Column item in showColumnsSelector.Selected)
		{
			item.UpdateVisible(visible: true);
		}
		PopulateColumns();
		pnlGrid.ResumeDrawing();
	}

	public void MoveLeftColumns()
	{
		if (_grid.BodyCol >= 0 && !IsTableLocked && _grid.BodyCol != 0)
		{
			Point scrollPosition = _grid.ScrollPosition;
			MoveColumns(_grid.BodyCol, _grid.BodyColSel - _grid.BodyCol + 1, _grid.BodyCol - 1);
			Select(_grid.BodyRow, _grid.BodyCol - 1, _grid.BodyRowSel, _grid.BodyColSel - 1);
			_grid.ScrollPosition = scrollPosition;
			FormulaEvaluator.ClearCache();
		}
	}

	public void MoveRightColumns()
	{
		if (_grid.BodyCol >= 0 && !IsTableLocked && _grid.BodyColSel != Table.Columns.Count - 1)
		{
			Point scrollPosition = _grid.ScrollPosition;
			MoveColumns(_grid.BodyCol, _grid.BodyColSel - _grid.BodyCol + 1, _grid.BodyColSel + 2);
			Select(_grid.BodyRow, _grid.BodyCol + 1, _grid.BodyRowSel, _grid.BodyColSel + 1);
			_grid.ScrollPosition = scrollPosition;
			FormulaEvaluator.ClearCache();
		}
	}

	public void SetDataType<T>()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		bool flag = false;
		if (_grid.FilterManager.Filters.Count > 0)
		{
			flag = true;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (Table.Rows.Skip(bodySelection.TopRow).Take(bodySelection.BottomRow - bodySelection.TopRow + 1).All((Auditai.Model.Row r) => !Table[r.Index, 0].ShouldApplyColumnFormula()))
		{
			foreach (Auditai.Model.Cell item in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
			{
				if ((!flag || IsGridRowVisible(item)) && CanEditRow(item.Row) && CanEditColumn(item.Column))
				{
					item.ChangeDataType(typeof(T));
				}
			}
		}
		else
		{
			foreach (Auditai.Model.Cell item2 in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
			{
				if ((!flag || IsGridRowVisible(item2)) && CanEditColumn(item2.Column) && CanEditRow(item2.Row) && item2.ShouldApplyColumnFormula())
				{
					item2.ChangeDataType(typeof(T));
				}
			}
		}
		MutateCellStyle((Auditai.Model.CellStyle cs) => cs.DataType, typeof(T), updateTableDefaultIfSelectAll: false);
	}

	public void SetDataFormatText()
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && Table.Rows.Count != 0 && !IsTableLocked)
		{
			DataFormat value = new DataFormat(DataFormatType.General);
			DataFormat displayFormat = Table[_grid.BodyRow, _grid.BodyCol].DisplayFormat;
			if (displayFormat.HasComboList)
			{
				value.ComboList = displayFormat.ComboList;
				value.IgnoreComboList = displayFormat.IgnoreComboList;
				value.MultiComboList = displayFormat.MultiComboList;
			}
			if (displayFormat.HasLedgerCollectFormula)
			{
				value.LedgerCollectFormula = displayFormat.LedgerCollectFormula;
			}
			SetDataType<string>();
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value);
			ForceRefresh();
		}
	}

	public void FlagSomeUserStayedCellChanged()
	{
		gridDecorator.SetDirty();
	}

	public void ClearColumnAllowManualInputFlag(int colIndex, bool isReCalcTable)
	{
		if (colIndex < 0 || colIndex >= Table.Columns.Count)
		{
			return;
		}
		bool flag = false;
		Auditai.Model.Column column = Table.Columns[colIndex];
		if (CanEditColumn(column))
		{
			Auditai.Model.CellStyle style = column.Style;
			if (style != null && (style.Format?.IsAllowEditOnExistFormula).GetValueOrDefault())
			{
				column.UpdateStyle(Table.CellStyles.MutateAndGet(column.Style, delegate(Auditai.Model.CellStyle cs)
				{
					DataFormat value = cs.Format?.Clone() ?? default(DataFormat);
					value.IsAllowEditOnExistFormula = false;
					cs.Format = value;
				}));
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		bool flag2 = false;
		int count = Table.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			Auditai.Model.Cell cell = Table[i, column.Index];
			if (cell.IsExistManualInputValue)
			{
				cell.IsExistManualInputValue = false;
				flag2 = true;
			}
		}
		if (flag2 && isReCalcTable)
		{
			CalcCurrentTable();
		}
	}

	public void ClearColumnAllowManualInputFlag(C1.Win.C1FlexGrid.CellRange range, bool isReCalcTable)
	{
		Lazy<List<Auditai.Model.Column>> lazy = new Lazy<List<Auditai.Model.Column>>();
		for (int i = range.LeftCol; i <= range.RightCol; i++)
		{
			Auditai.Model.Column column = Table.Columns[i];
			if (!CanEditColumn(column))
			{
				continue;
			}
			Auditai.Model.CellStyle style = column.Style;
			if (style != null && (style.Format?.IsAllowEditOnExistFormula).GetValueOrDefault())
			{
				column.UpdateStyle(Table.CellStyles.MutateAndGet(column.Style, delegate(Auditai.Model.CellStyle cs)
				{
					DataFormat value = cs.Format?.Clone() ?? default(DataFormat);
					value.IsAllowEditOnExistFormula = false;
					cs.Format = value;
				}));
				lazy.Value.Add(column);
			}
		}
		if (!lazy.IsValueCreated)
		{
			return;
		}
		bool flag = false;
		foreach (Auditai.Model.Column item in lazy.Value)
		{
			for (int j = range.TopRow; j <= range.BottomRow; j++)
			{
				Auditai.Model.Cell cell = Table[j, item.Index];
				if (cell.IsExistManualInputValue)
				{
					cell.IsExistManualInputValue = false;
					flag = true;
				}
			}
		}
		if (flag && isReCalcTable)
		{
			CalcCurrentTable();
		}
	}

	public void SetFormulaColumnAllowManualInput(bool isAllowManualInput)
	{
		Lazy<List<Auditai.Model.Column>> lazy = new Lazy<List<Auditai.Model.Column>>();
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
		{
			Auditai.Model.Column column = Table.Columns[i];
			if (column.HasFormula && CanEditColumn(column))
			{
				column.UpdateStyle(Table.CellStyles.MutateAndGet(column.Style, delegate(Auditai.Model.CellStyle cs)
				{
					DataFormat value = cs.Format?.Clone() ?? default(DataFormat);
					value.IsAllowEditOnExistFormula = isAllowManualInput;
					cs.Format = value;
				}));
				if (!isAllowManualInput)
				{
					lazy.Value.Add(column);
				}
			}
		}
		if (isAllowManualInput || !lazy.IsValueCreated)
		{
			return;
		}
		bool flag = false;
		foreach (Auditai.Model.Column item in lazy.Value)
		{
			for (int j = bodySelection.TopRow; j <= bodySelection.BottomRow; j++)
			{
				Auditai.Model.Cell cell = Table[j, item.Index];
				if (cell.IsExistManualInputValue)
				{
					cell.IsExistManualInputValue = false;
					flag = true;
				}
			}
		}
		if (flag)
		{
			CalcCurrentTable();
		}
	}

	public void SetDataFormatNumeric(DataFormatType dft)
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && Table.Rows.Count != 0 && !IsTableLocked)
		{
			SetDataType<double>();
			DataFormat displayFormat = Table[_grid.BodyRow, _grid.BodyCol].DisplayFormat;
			DataFormat dataFormat = new DataFormat(dft);
			dataFormat.DecimalLength = 2;
			DataFormat value = dataFormat;
			if (displayFormat.HasComboList)
			{
				value.ComboList = displayFormat.ComboList;
				value.IgnoreComboList = displayFormat.IgnoreComboList;
				value.MultiComboList = displayFormat.MultiComboList;
			}
			if (displayFormat.HasLedgerCollectFormula)
			{
				value.LedgerCollectFormula = displayFormat.LedgerCollectFormula;
			}
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value);
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Align, CellTextAlign.MiddleRight);
			ForceRefresh();
			if (dft == DataFormatType.Number || dft == DataFormatType.NumDollar || dft == DataFormatType.NumRmb || dft == DataFormatType.Percentage || dft == DataFormatType.Comma)
			{
				SetZeroFormat(ZeroFormat.Empty);
			}
		}
	}

	public void SetDataFormatBoolean(DataFormatType dft)
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && Table.Rows.Count != 0 && !IsTableLocked)
		{
			SetDataType<bool>();
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, new DataFormat(dft));
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Align, CellTextAlign.MiddleCenter);
			ForceRefresh();
		}
	}

	public void SetDataFormatDate(DataFormatType dft)
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && Table.Rows.Count != 0 && !IsTableLocked)
		{
			SetDataType<DateTime>();
			DataFormat displayFormat = Table[_grid.BodyRow, _grid.BodyCol].DisplayFormat;
			DataFormat value = new DataFormat(dft);
			if (displayFormat.HasComboList)
			{
				value.ComboList = displayFormat.ComboList;
				value.IgnoreComboList = displayFormat.IgnoreComboList;
				value.MultiComboList = displayFormat.MultiComboList;
			}
			if (displayFormat.HasLedgerCollectFormula)
			{
				value.LedgerCollectFormula = displayFormat.LedgerCollectFormula;
			}
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value);
			ForceRefresh();
		}
	}

	public void SetDataFormatDateYearMonth(DataFormatType dft)
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && Table.Rows.Count != 0 && !IsTableLocked)
		{
			SetDataType<DateYearMonth>();
			DataFormat displayFormat = Table[_grid.BodyRow, _grid.BodyCol].DisplayFormat;
			DataFormat value = new DataFormat(dft);
			if (displayFormat.HasComboList)
			{
				value.ComboList = displayFormat.ComboList;
				value.IgnoreComboList = displayFormat.IgnoreComboList;
				value.MultiComboList = displayFormat.MultiComboList;
			}
			if (displayFormat.HasLedgerCollectFormula)
			{
				value.LedgerCollectFormula = displayFormat.LedgerCollectFormula;
			}
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value);
			ForceRefresh();
		}
	}

	public void SetDataFormatTime(DataFormatType dft)
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && Table.Rows.Count != 0 && !IsTableLocked)
		{
			SetDataType<TimeSpan>();
			DataFormat displayFormat = Table[_grid.BodyRow, _grid.BodyCol].DisplayFormat;
			DataFormat value = new DataFormat(dft);
			if (displayFormat.HasComboList)
			{
				value.ComboList = displayFormat.ComboList;
				value.IgnoreComboList = displayFormat.IgnoreComboList;
				value.MultiComboList = displayFormat.MultiComboList;
			}
			if (displayFormat.HasLedgerCollectFormula)
			{
				value.LedgerCollectFormula = displayFormat.LedgerCollectFormula;
			}
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value);
			ForceRefresh();
		}
	}

	public void SetRowHeight(int height)
	{
		if (_grid.BodyRow < 0 || IsTableLocked)
		{
			return;
		}
		try
		{
			for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
			{
				Table.Rows[i].UpdateHeight(height);
			}
			PopulateRowsHeight();
		}
		catch (ArgumentOutOfRangeException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void SetColumnWidth(int width)
	{
		if (_grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		try
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Table.Columns[i].UpdateWidth(width);
			}
			pnlGrid.SuspendDrawing();
			PopulateColumns();
			pnlGrid.ResumeDrawing();
		}
		catch (ArgumentOutOfRangeException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void SetForeColor(Color color)
	{
		if (IsTableLocked)
		{
			return;
		}
		if (_isEditingHeaders)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.ForeColor = color;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			PopulateEditingColumnHeaders();
		}
		else if (TitleEditor.IsEditing)
		{
			TitleEditor.SetForeColor(color);
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetForeColor(color);
		}
		else if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.ForeColor, color);
		}
	}

	public void SetBackColor(Color color)
	{
		if (IsTableLocked)
		{
			return;
		}
		if (_isEditingHeaders)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.BackColor = color;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			PopulateEditingColumnHeaders();
		}
		else if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.BackColor, color);
		}
	}

	public void SetAlign(CellTextAlign align)
	{
		if (IsTableLocked)
		{
			return;
		}
		if (_isEditingHeaders)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.Align = align;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			PopulateEditingColumnHeaders();
		}
		else if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Align, align);
		}
	}

	public void SetFontSize(float fontSize)
	{
		if (IsTableLocked)
		{
			return;
		}
		fontSize = FontSizeComboHost.ToNearestHalf(fontSize);
		if (TitleEditor.IsEditing)
		{
			TitleEditor.SetFontSize(fontSize);
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetFontSize(fontSize);
		}
		else if (_isEditingHeaders)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.FontSize = fontSize;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			PopulateEditingColumnHeaders();
			AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(fontSize);
		}
		else if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.FontSize, fontSize);
			AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(fontSize);
		}
	}

	public void GrowFont()
	{
		if (IsTableLocked)
		{
			return;
		}
		if (TitleEditor.IsEditing)
		{
			TitleEditor.GrowFont();
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.GrowFont();
		}
		else if (_isEditingHeaders)
		{
			float value = Table.Columns[_grid.BodyCol].CaptionStyle.FontSize.Value;
			value = FontSize.Grow(value);
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.FontSize = value;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(value);
			PopulateEditingColumnHeaders();
		}
		else
		{
			if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
			{
				return;
			}
			int j = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[j].Role == RowRole.Header)
			{
				j++;
			}
			bool flag = false;
			for (; j <= _grid.BodyBottomRow; j++)
			{
				int num = _grid.Rows.Fixed + j;
				if (num >= _grid.Rows.Count)
				{
					return;
				}
				if (_grid.Rows[num].Visible)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				float displayFontSize = Table[j, _grid.BodyCol].DisplayFontSize;
				displayFontSize = FontSize.Grow(displayFontSize);
				MutateCellStyle((Auditai.Model.CellStyle cs) => cs.FontSize, displayFontSize);
				AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(displayFontSize);
			}
		}
	}

	public void ShrinkFont()
	{
		if (IsTableLocked)
		{
			return;
		}
		if (_isEditingHeaders)
		{
			float value = Table.Columns[_grid.BodyCol].CaptionStyle.FontSize.Value;
			value = FontSize.Shrink(value);
			if (value < 1f)
			{
				value = 1f;
			}
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.FontSize = value;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(value);
			PopulateEditingColumnHeaders();
		}
		else if (TitleEditor.IsEditing)
		{
			TitleEditor.ShrinkFont();
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.ShrinkFont();
		}
		else
		{
			if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
			{
				return;
			}
			int j = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[j].Role == RowRole.Header)
			{
				j++;
			}
			bool flag = false;
			for (; j <= _grid.BodyBottomRow; j++)
			{
				int num = _grid.Rows.Fixed + j;
				if (num >= _grid.Rows.Count)
				{
					return;
				}
				if (_grid.Rows[num].Visible)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				float displayFontSize = Table[j, _grid.BodyCol].DisplayFontSize;
				displayFontSize = FontSize.Shrink(displayFontSize);
				MutateCellStyle((Auditai.Model.CellStyle cs) => cs.FontSize, displayFontSize);
				AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(displayFontSize);
			}
		}
	}

	public void SetFontFamily(string ff)
	{
		if (IsTableLocked)
		{
			return;
		}
		if (_isEditingHeaders)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.FontFamily = ff;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			PopulateEditingColumnHeaders();
		}
		else if (TitleEditor.IsEditing)
		{
			TitleEditor.SetFontFamily(ff);
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetFontFamily(ff);
		}
		else if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.FontFamily, ff);
		}
	}

	public void MorePrecision()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		int i = _grid.BodyRow;
		if (!_isSelectingHeaderCell && Table.Rows[i].Role == RowRole.Header)
		{
			i++;
		}
		bool flag = false;
		for (; i <= _grid.BodyBottomRow; i++)
		{
			int num = _grid.Rows.Fixed + i;
			if (num >= _grid.Rows.Count)
			{
				return;
			}
			if (_grid.Rows[num].Visible)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			DataFormat value = Table[i, _grid.BodyCol].DisplayFormat.Clone();
			if (value.FormatType == DataFormatType.General)
			{
				DataFormat dataFormat = new DataFormat(DataFormatType.Number);
				dataFormat.DecimalLength = 1;
				value = dataFormat;
			}
			else
			{
				value.DecimalLength++;
			}
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value, updateTableDefaultIfSelectAll: false);
			ForceRefresh();
		}
	}

	public void LessPrecision()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		int i = _grid.BodyRow;
		if (!_isSelectingHeaderCell && Table.Rows[i].Role == RowRole.Header)
		{
			i++;
		}
		bool flag = false;
		for (; i <= _grid.BodyBottomRow; i++)
		{
			int num = _grid.Rows.Fixed + i;
			if (num >= _grid.Rows.Count)
			{
				return;
			}
			if (_grid.Rows[num].Visible)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			DataFormat value = Table[i, _grid.BodyCol].DisplayFormat.Clone();
			if (value.FormatType == DataFormatType.General)
			{
				DataFormat dataFormat = new DataFormat(DataFormatType.Number);
				dataFormat.DecimalLength = 1;
				value = dataFormat;
			}
			else
			{
				value.DecimalLength--;
			}
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value, updateTableDefaultIfSelectAll: false);
			ForceRefresh();
		}
	}

	public void SetZeroFormat(ZeroFormat zf)
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			int num = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[num].Role == RowRole.Header)
			{
				num++;
			}
			DataFormat value = Table[num, _grid.BodyCol].DisplayFormat.Clone();
			value.ZeroFormat = zf;
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value, updateTableDefaultIfSelectAll: false);
			ForceRefresh();
		}
	}

	public void SetEditCommentDialog()
	{
		if (IsTableLocked)
		{
			return;
		}
		if (TitleEditor.IsEditing)
		{
			TitleEditor.SetEditCommentDialog();
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetEditCommentDialog();
		}
		else
		{
			if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
			{
				return;
			}
			int num = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[num].Role == RowRole.Header)
			{
				num++;
			}
			Auditai.Model.Cell cell = Table[num, _grid.BodyCol];
			if (!CanEditColumn(cell.Column) || !CanEditRow(cell.Row))
			{
				return;
			}
			AuxEditor.New();
			if (_grid.IsEntireColumnSelected)
			{
				Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
				AuxEditor.View.CommentValue = column.Style?.Comment ?? string.Empty;
			}
			else
			{
				AuxEditor.View.CommentValue = cell.DisplayComment;
			}
			if (AuxEditor.ShowComment() == DialogResult.OK && AuxEditor.View.QueryCommentValueChanged)
			{
				MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Comment, AuxEditor.View.CommentValue, updateTableDefaultIfSelectAll: false, clearCellStyleIfEntireColumn: false);
			}
		}
	}

	public void SetComboListDialog()
	{
		if (IsTableLocked)
		{
			return;
		}
		if (TitleEditor.IsEditing)
		{
			TitleEditor.SetComboListDialog();
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetComboListDialog();
		}
		else
		{
			if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
			{
				return;
			}
			int num = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[num].Role == RowRole.Header)
			{
				num++;
			}
			Auditai.Model.Cell cell = Table[num, _grid.BodyCol];
			if (!CanEditColumn(cell.Column) || !CanEditRow(cell.Row))
			{
				return;
			}
			ToolBar.Enabled = false;
			AuxEditor.New();
			AuxEditor.View.AllowFreeInput = cell.DisplayFormat.IgnoreComboList;
			AuxEditor.View.AllowMultiSelect = cell.DisplayFormat.MultiComboList;
			DataFormat displayFormat = cell.DisplayFormat;
			AuxEditor.View.Value = "";
			if (displayFormat.HasComboList)
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(displayFormat.ComboList);
					FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
					AuxEditor.View.Value = formulaEvaluator.GetDisplayString(resolver, Table);
				}
				catch (FormulaSyntaxException)
				{
					AuxEditor.View.Value = displayFormat.ComboList;
				}
			}
			BeginFormula();
			FormulaEditor.Context.Kind = FormulaContextKind.TableAuxEdit;
			FormulaEditor.Context.Table = Table;
			FormulaEditor.Context.Project = Table.Project;
			FormulaEditor.View.Enabled = false;
			AuxEditSelectionPreserve = _grid.BodySelection;
			AuxEditor.Closed += AuxEditor_Closed;
			AuxEditor.ShowList(_owner.View);
			Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
			_grid.Select(-1, -1);
		}
	}

	private void AuxEditor_Closed(object sender, EventArgs e)
	{
		Program.MainForm.SuspendNavPanelDrawing();
		Program.MainForm.SuspendNavPanelVisible();
		pnlGrid.SuspendDrawing();
		try
		{
			AuxEditor.Closed -= AuxEditor_Closed;
			AuxFormulaCloseRestoreState();
			if (AuxEditor.View.DialogResult != DialogResult.OK)
			{
				return;
			}
			if (AuxEditor.View.QueryValueChanged)
			{
				string value = AuxEditor.View.Value;
				int num = _grid.BodyRow;
				if (!_isSelectingHeaderCell && Table.Rows[num].Role == RowRole.Header)
				{
					num++;
				}
				Auditai.Model.Cell cell = Table[num, _grid.BodyCol];
				DataFormat value2 = cell.DisplayFormat.Clone();
				value2.ComboList = value;
				value2.MultiComboList = AuxEditor.View.AllowMultiSelect;
				value2.IgnoreComboList = AuxEditor.View.AllowFreeInput;
				MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value2, updateTableDefaultIfSelectAll: false);
			}
			ForceRefresh();
			Invalidate();
		}
		finally
		{
			Program.MainForm.ResumeNavPanelVisible();
			pnlGrid.ResumeDrawing();
			Program.MainForm.ResumeNavPanelDrawing();
		}
	}

	public void AuxFormulaCloseRestoreState()
	{
		Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
		EndFormula();
		FormulaContext formulaContext = FormulaEditor.Context.Clone();
		FormulaEvaluator.ClearCache();
		_owner.ProjectHierarchy.FindAndSelectNode(formulaContext.Table.TreeNode);
		Table = formulaContext.Table;
		PopulateTable();
		_owner.SwitchStateTo(MainFormView.Table);
		_grid.Focus();
		Select(AuxEditSelectionPreserve.r1, AuxEditSelectionPreserve.c1, AuxEditSelectionPreserve.r2, AuxEditSelectionPreserve.c2);
		FormulaEditor.RefIntervals = null;
	}

	public void SetEditLedgerCollectFormulaDialog()
	{
		if (IsTableLocked)
		{
			return;
		}
		if (TitleEditor.IsEditing)
		{
			TitleEditor.SetComboListDialog();
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetComboListDialog();
		}
		else
		{
			if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
			{
				return;
			}
			int num = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[num].Role == RowRole.Header)
			{
				num++;
			}
			Auditai.Model.Cell cell = Table[num, _grid.BodyCol];
			if (!CanEditColumn(cell.Column) || !CanEditRow(cell.Row))
			{
				return;
			}
			ToolBar.Enabled = false;
			LedgerCollectFormulaEditor.New();
			FormulaEditor.Context.Kind = FormulaContextKind.LedgerCollectFormulaEdit;
			FormulaEditor.Context.Table = Table;
			FormulaEditor.Context.Project = Table.Project;
			FormulaEditor.View.Enabled = false;
			DataFormat displayFormat = cell.DisplayFormat;
			LedgerCollectFormulaEditor.View.Value = "";
			if (displayFormat.HasLedgerCollectFormula)
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(displayFormat.LedgerCollectFormula);
					FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
					LedgerCollectFormulaEditor.View.Value = formulaEvaluator.GetDisplayStringLedgerCollectFormula(resolver, Table, FormulaEditor.Context.LegderVirtualTableSetting);
				}
				catch (FormulaSyntaxException)
				{
					LedgerCollectFormulaEditor.View.Value = displayFormat.LedgerCollectFormula;
				}
				catch (Exception exception)
				{
					exception.Log("显示账套采集公式时发生了未预期的异常");
					LedgerCollectFormulaEditor.View.Value = displayFormat.LedgerCollectFormula;
				}
			}
			LedgerCollectFormulaEditor.View.TryRunFormulaHandle = LedgerCollectFormulaTryRunHandle;
			BeginFormula();
			Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
			_grid.Select(-1, -1);
			AuxEditSelectionPreserve = _grid.BodySelection;
			LedgerCollectFormulaEditor.Closed += LedgerCollectFormulaEditor_Closed;
			LedgerCollectFormulaEditor.ShowEditor(_owner.View, Table, cell.Column);
		}
	}

	private void LedgerCollectFormulaTryRunHandle(string formula)
	{
		LedgerVirtualTable balanceVirtualTable = null;
		LedgerVirtualTable voucherVirtualTable = null;
		if (Table.Rows.Count < 0)
		{
			throw new Exception("表格中请至少保留一行数据，否则无法校验公式是否正确!");
		}
		LedgerVirtualTableEvalContext tec = GetEvalContext();
		FormulaEvaluator formulaEvaluator = GenerateFormualEvaluator(formula, 0);
		Auditai.Model.Operand operand = formulaEvaluator.EvaluateOnLedgerVirtualTable(tec, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
		object obj = operand.Evaluate();
		FormulaEvaluator GenerateFormualEvaluator(string formula, int tableRowIndex)
		{
			FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
			FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
			{
				Resolver = resolver,
				RowIndex = tableRowIndex,
				HostTable = Table,
				RefManager = Table.Project.DataReferenceManager,
				RefEvalContext = new DataReferenceEvaluationContext
				{
					Project = Table.Project,
					CurrentTreeNode = Table.TreeNode
				}
			};
			return new FormulaEvaluator(formula)
			{
				Env = env
			};
		}
		LedgerVirtualTableEvalContext GetEvalContext()
		{
			return new LedgerVirtualTableEvalContext
			{
				BalanceTable_ResolveColumn = delegate(Id64 colId)
				{
					if (balanceVirtualTable == null)
					{
						balanceVirtualTable = BalanceVirtualTableBuilder.GetEmtpyTable();
					}
					int num2 = (int)colId.Value;
					return (num2 >= 0 && num2 < balanceVirtualTable.Columns.Count) ? new LedgerVirtualTableColumnOperand(balanceVirtualTable.Columns[num2]) : null;
				},
				VoucherTable_ResolveColumn = delegate(Id64 colId)
				{
					if (voucherVirtualTable == null)
					{
						voucherVirtualTable = VoucherVirtualTableBuilder.GetEmtpyTable();
					}
					int num = (int)colId.Value;
					return (num >= 0 && num < voucherVirtualTable.Columns.Count) ? new LedgerVirtualTableColumnOperand(voucherVirtualTable.Columns[num]) : null;
				}
			};
		}
	}

	private void LedgerCollectFormulaEditor_Closed(object sender, EventArgs e)
	{
		Program.MainForm.MainPanel.SuspendDrawing();
		try
		{
			Point scrollPosition = _grid.ScrollPosition;
			LedgerCollectFormulaEditor.Closed -= LedgerCollectFormulaEditor_Closed;
			LedgerCollectFormulaCloseRestoreState();
			if (LedgerCollectFormulaEditor.View.DialogResult == DialogResult.OK && LedgerCollectFormulaEditor.View.QueryValueChanged)
			{
				Dictionary<Auditai.Model.Column, string> changedFormula = LedgerCollectFormulaEditor.View.ChangedFormula;
				try
				{
					foreach (Auditai.Model.Column key in changedFormula.Keys)
					{
						string text = changedFormula[key];
						DataFormat value = key.GetFormat().Clone();
						value.LedgerCollectFormula = (string.IsNullOrWhiteSpace(text) ? null : text);
						MutateTableColumnCellStyle((Auditai.Model.CellStyle cs) => cs.Format, value, key);
					}
				}
				catch (Exception exception)
				{
					exception.Log("修改表格列的采集公式时发生了未预期的异常");
				}
			}
			_grid.ScrollPosition = scrollPosition;
		}
		finally
		{
			Program.MainForm.MainPanel.ResumeDrawing();
		}
	}

	private void LedgerCollectFormulaCloseRestoreState()
	{
		Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
		EndFormula();
		FormulaContext formulaContext = FormulaEditor.Context.Clone();
		_owner.ProjectHierarchy.FindAndSelectNode(formulaContext.Table.TreeNode);
		Table = formulaContext.Table;
		_owner.SwitchStateTo(MainFormView.Table);
		_grid.Focus();
		Select(AuxEditSelectionPreserve.r1, AuxEditSelectionPreserve.c1, AuxEditSelectionPreserve.r2, AuxEditSelectionPreserve.c2);
		FormulaEditor.RefIntervals = null;
	}

	public void BeginTitleFootAuxEdit()
	{
		Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
		BeginFormula();
	}

	public void EndTitleFootAuxEdit()
	{
		Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
		FormulaEditor.RefIntervals = null;
		EndFormula();
		FormulaContext formulaContext = FormulaEditor.Context.Clone();
		_owner.ProjectHierarchy.FindAndSelectNode(formulaContext.Table.TreeNode);
		Table = formulaContext.Table;
		PopulateTable();
		_owner.SwitchStateTo(MainFormView.Table);
		_grid.Focus();
	}

	public void SetBold(bool v)
	{
		if (IsTableLocked)
		{
			return;
		}
		if (_isEditingHeaders)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.Bold = v;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			PopulateEditingColumnHeaders();
		}
		else if (TitleEditor.IsEditing)
		{
			TitleEditor.SetBold(v);
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetBold(v);
		}
		else if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Bold, v);
		}
	}

	public void SetItalic(bool v)
	{
		if (IsTableLocked)
		{
			return;
		}
		if (_isEditingHeaders)
		{
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (CanEditColumn(column))
				{
					Auditai.Model.CellStyle captionStyle = column.CaptionStyle;
					captionStyle.Italic = v;
					column.UpdateCaptionStyle(captionStyle);
				}
			}
			PopulateEditingColumnHeaders();
		}
		else if (TitleEditor.IsEditing)
		{
			TitleEditor.SetItalic(v);
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.SetItalic(v);
		}
		else if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Italic, v);
		}
	}

	public void SortAscending()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			_grid.BeginUpdate();
			Table.Columns[_grid.BodyCol].SortAscending();
			PopulateRows();
			_grid.EndUpdate();
		}
	}

	public void SortDescending()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			_grid.BeginUpdate();
			Table.Columns[_grid.BodyCol].SortDescending();
			PopulateRows();
			_grid.EndUpdate();
		}
	}

	public void EnterFormula()
	{
		FormulaEditor.SetFocus();
	}

	public void MergeEveryHorizontalCells()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (!_table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol).All((Auditai.Model.Cell c) => CanEditRow(c.Row) && CanEditColumn(c.Column)))
		{
			return;
		}
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			MergeRowCellsImpl(i, bodySelection.LeftCol, bodySelection.RightCol);
		}
		if (bodySelection.LeftCol == 0 && bodySelection.RightCol == _grid.BodyColsCount - 1)
		{
			for (int j = _grid.BodyRow; j <= _grid.BodyRowSel; j++)
			{
				RowRole role = Table.Rows[j].Role;
				if (role == RowRole.Normal || role == RowRole.Among || role == RowRole.Minus)
				{
					Table.Rows[j].UpdateRole(RowRole.Fixed);
				}
			}
			CalcCurrentTable();
			Table.Ticket.IsCacheExpired = true;
		}
		else
		{
			_grid.BeginUpdate();
			PopulateMerges();
			_grid.EndUpdate();
		}
	}

	public void MergeCells()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (_table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol).All((Auditai.Model.Cell c) => CanEditRow(c.Row) && CanEditColumn(c.Column)))
		{
			if (_table.WillMergeEraseValue(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
			{
				MergeCellsImpl();
			}
			else
			{
				MergeCellsImpl();
			}
		}
	}

	public void UnmergeCells()
	{
		if (IsTableLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		foreach (Auditai.Model.Cell item in _table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
		{
			try
			{
				if (CanEditRow(item.Row) && CanEditColumn(item.Column))
				{
					_table.UnmergeCells(item.Row.Index, item.Column.Index);
				}
			}
			catch (Exception exception)
			{
				exception.Log();
			}
		}
		_grid.BeginUpdate();
		PopulateMerges();
		_grid.EndUpdate();
	}

	public void Undo()
	{
		_grid.BeginUpdate();
		Table.CommandsManager.Undo();
		_grid.EndUpdate();
	}

	public void Redo()
	{
		_grid.BeginUpdate();
		Table.CommandsManager.Redo();
		_grid.EndUpdate();
	}

	public void SetBorderStyle(TableBorderStyle bs)
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			Table.UpdateBorderStyle(bs);
			_grid.Invalidate();
		}
	}

	public bool CanTableCollect()
	{
		if (Table == null)
		{
			return false;
		}
		return TableCollectorAbstract.CanCollect(Table);
	}

	public bool CanCellCollect()
	{
		if (Table == null)
		{
			return false;
		}
		return CollectManager.CanCollect(Table);
	}

	public void HideToolbar()
	{
		pnlToolbar.Hide();
	}

	public void ShowToolbar()
	{
		pnlToolbar.Show();
	}

	public async Task TableStyleBrush()
	{
		frmNodeSelector frmNodeSelector2 = new frmNodeSelector();
		frmNodeSelector2.Project = Program.MainForm.CurrentProject;
		if (frmNodeSelector2.ShowBatchBrush() != DialogResult.OK)
		{
			return;
		}
		IEnumerable<Auditai.Model.Table> tables = from n in frmNodeSelector2.Selected.OfType<TreeTableNode>()
			select n.Table;
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		progressRuntimeData.NextStep("正在应用样式刷...");
		progressForm.ShowDialog(progressRuntimeData, delegate
		{
			int num = 0;
			int totalCount = tables.Count();
			foreach (Auditai.Model.Table item in tables)
			{
				num++;
				progressRuntimeData.UpdateProgress(num, totalCount);
				item.LoadAndReturn();
				if (item.TreeNode.HasWritePermission())
				{
					TableStyleBrushImpl(Table, item);
				}
			}
		});
		await Task.Delay(1);
	}

	public void BatchColumnDuplicate()
	{
		if (!_grid.IsEntireColumnSelected)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要复制的列");
			return;
		}
		frmNodeSelector frmNodeSelector2 = new frmNodeSelector();
		frmNodeSelector2.Project = Auditai.Model.Project.Current;
		if (frmNodeSelector2.ShowBatchColumnDuplicate() != DialogResult.OK)
		{
			return;
		}
		int leftCol = _grid.BodySelection.LeftCol;
		int rightCol = _grid.BodySelection.RightCol;
		if (SoftwareLicenseManager.IsTableColsCountOutOfLicenseLimit(Table, rightCol - leftCol + 1))
		{
			return;
		}
		bool flag = false;
		string caption = null;
		if (rightCol == Table.Columns.Count - 1)
		{
			flag = true;
		}
		else
		{
			caption = Table.Columns[rightCol + 1].GetUniqueFormulaName();
		}
		foreach (Auditai.Model.Table item in from n in frmNodeSelector2.Selected.OfType<TreeTableNode>().Except(new TreeTableNode[1] { Table.TreeNode })
			select n.Table.LoadAndReturn() into t
			where t.HasSchemaPermission()
			select t)
		{
			if (!item.TreeNode.HasReadPermission() || !item.TreeNode.HasWritePermission() || !item.TreeNode.HasSchemaPermission())
			{
				continue;
			}
			if (!flag)
			{
				Auditai.Model.Column byCaption = item.Columns.GetByCaption(caption);
				if (byCaption == null)
				{
					flag = true;
				}
				else
				{
					int index = byCaption.Index;
					item.Columns.Insert(index, rightCol - leftCol + 1);
					for (int i = leftCol; i <= rightCol; i++)
					{
						CopyColumn(Table.Columns[i], item.Columns[i - leftCol + index]);
					}
				}
			}
			if (flag)
			{
				item.Columns.Append(rightCol - leftCol + 1);
				for (int j = leftCol; j <= rightCol; j++)
				{
					CopyColumn(Table.Columns[j], item.Columns[item.Columns.Count - 1 - rightCol + j]);
				}
			}
		}
		static void CopyColumn(Auditai.Model.Column src, Auditai.Model.Column dst)
		{
			dst.Caption = src.Caption;
			dst.Width = src.Width;
			if (src.Style != null)
			{
				Auditai.Model.CellStyle style = dst.Table.CellStyles.MutateAndGet(dst.Style, delegate(Auditai.Model.CellStyle cs)
				{
					cs.DataType = src.Style.DataType;
					cs.Format = src.Style.Format;
					cs.Align = src.Style.Align;
					cs.Margin = src.Style.Margin;
				});
				dst.UpdateStyle(style);
			}
		}
	}

	public void BatchColumnRemove()
	{
		if (!_grid.IsEntireColumnSelected)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要删除的列");
			return;
		}
		frmNodeSelector frmNodeSelector2 = new frmNodeSelector();
		frmNodeSelector2.Project = Auditai.Model.Project.Current;
		if (frmNodeSelector2.ShowBatchColumnRemove() != DialogResult.OK)
		{
			return;
		}
		int leftCol = _grid.BodySelection.LeftCol;
		int rightCol = _grid.BodySelection.RightCol;
		HashSet<string> hashSet = new HashSet<string>(from c in Table.Columns.Skip(leftCol).Take(rightCol - leftCol + 1)
			select c.GetUniqueFormulaName());
		foreach (Auditai.Model.Table item in from n in frmNodeSelector2.Selected.OfType<TreeTableNode>()
			select n.Table.LoadAndReturn() into t
			where t.HasSchemaPermission()
			select t)
		{
			if (!item.TreeNode.HasReadPermission() || !item.TreeNode.HasWritePermission() || !item.TreeNode.HasSchemaPermission())
			{
				continue;
			}
			for (int num = item.Columns.Count - 1; num >= 0; num--)
			{
				if (hashSet.Contains(item.Columns[num].GetUniqueFormulaName()))
				{
					item.Columns.Remove(num, 1);
				}
			}
		}
		PopulateColumns();
	}

	public void BatchColumnRename()
	{
		if (!_grid.IsEntireColumnSelected)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择要重命名的列");
			return;
		}
		string text = InputForm.Text("", "将列重命名为：");
		if (text == null)
		{
			return;
		}
		frmNodeSelector frmNodeSelector2 = new frmNodeSelector();
		frmNodeSelector2.Project = Auditai.Model.Project.Current;
		if (frmNodeSelector2.ShowBatchColumnRename() != DialogResult.OK)
		{
			return;
		}
		int leftCol = _grid.BodySelection.LeftCol;
		int rightCol = _grid.BodySelection.RightCol;
		HashSet<string> hashSet = new HashSet<string>(from c in Table.Columns.Skip(leftCol).Take(rightCol - leftCol + 1)
			select c.GetUniqueFormulaName());
		foreach (Auditai.Model.Table item in from n in frmNodeSelector2.Selected.OfType<TreeTableNode>()
			select n.Table.LoadAndReturn() into t
			where t.HasSchemaPermission()
			select t)
		{
			if (!item.TreeNode.HasReadPermission() || !item.TreeNode.HasWritePermission() || !item.TreeNode.HasSchemaPermission())
			{
				continue;
			}
			for (int num = item.Columns.Count - 1; num >= 0; num--)
			{
				if (hashSet.Contains(item.Columns[num].GetUniqueFormulaName()))
				{
					item.Columns[num].UpdateCaption(text);
				}
			}
		}
		PopulateColumns();
	}

	public void GenerateBatchFormula()
	{
		if (!_grid.IsEntireColumnSelected)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择列");
			return;
		}
		if (Table == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择列");
			return;
		}
		if (Table.TreeNode.HasReadPermission())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有查看权限，无法进行编辑");
			return;
		}
		if (Table.TreeNode.HasWritePermission())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有编辑权限，无法进行编辑");
			return;
		}
		int leftCol = _grid.BodySelection.LeftCol;
		Auditai.Model.Column column = Table.Columns[leftCol];
		if (!column.HasFormula)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列公式为空");
			return;
		}
		FormulaEvaluator formulaEvaluator = new FormulaEvaluator(column.Formula);
		if (!formulaEvaluator.CanPatternMatchBatch())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "智能扩充跨表公式功能仅支持含有Distinct、Distinct、DistinctUp、DistinctDown、 Vlookup、Sumif函数的列公式的智能扩充，当前选中列的列公式中未包含这些函数，因此不支持使用该功能。");
			return;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		HashSet<Id64> referredTableIds = formulaEvaluator.GetReferredTableIds();
		referredTableIds.Remove(Table.Id);
		frmNodeSelector frmNodeSelector2 = new frmNodeSelector();
		frmNodeSelector2.Project = Auditai.Model.Project.Current;
		frmNodeSelector2.PreSelectNodes = referredTableIds;
		if (frmNodeSelector2.ShowGenerateBatchFormula() != DialogResult.OK || frmNodeSelector2.Selected.Count == 0)
		{
			return;
		}
		List<Auditai.Model.Table> list = (from n in frmNodeSelector2.Selected.OfType<TreeTableNode>()
			select n.Table.LoadAndReturn() into n
			where n.TreeNode.HasReadPermission()
			select n).ToList();
		if (list.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "选中的表格没有查看权限");
			return;
		}
		string text = formulaEvaluator.PatternMatchBatch(resolver, Table, from n in frmNodeSelector2.Selected.OfType<TreeTableNode>().Except(new TreeTableNode[1] { Table.TreeNode })
			select n.Table.LoadAndReturn());
		if (text == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "智能填充跨表公式操作失败。");
			return;
		}
		column.UpdateFormula(text);
		PopulateTable();
	}

	public void Cut()
	{
		Table.BeginBatchUpdateValue();
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		int num = bodySelection.BottomRow - bodySelection.TopRow + 1;
		bool flag = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
		bool isTableExistFillFormula = IsTableExistFillFormula();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			if (!_grid.BodyGetRow(i).Visible)
			{
				continue;
			}
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				if (!_grid.BodyGetCol(j).Visible)
				{
					continue;
				}
				stringBuilder.Append(GetValueForCopy(Table[i, j].GetDisplayValue()));
				if (j < bodySelection.RightCol)
				{
					stringBuilder.Append("\t");
				}
				Auditai.Model.Cell cell = Table[i, j];
				if (!CanEditCell(cell, isTableExistFillFormula))
				{
					continue;
				}
				if (cell.HasCellFormulaOrColumnFormula)
				{
					if (flag)
					{
						cell.IsExistManualInputValue = true;
						cell.UpdateValue("");
					}
				}
				else
				{
					cell.UpdateValue("");
				}
			}
			if (num > 1)
			{
				stringBuilder.Append("\r\n");
			}
		}
		Table.EndBatchUpdateValue();
		string text = stringBuilder.ToString();
		try
		{
			if (string.IsNullOrEmpty(text))
			{
				System.Windows.Forms.Clipboard.Clear();
			}
			else
			{
				System.Windows.Forms.Clipboard.SetText(text);
			}
		}
		catch (ExternalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		SetClipboard();
		_grid.Invalidate();
	}

	public static string GetValueForCopy(string s)
	{
		s = s.Replace("\t", " ");
		if (s.Contains("\n"))
		{
			return "\"" + s.Replace("\"", "\"\"") + "\"";
		}
		return s;
	}

	public void Copy()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			int num = bodySelection.BottomRow - bodySelection.TopRow + 1;
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				if (!_grid.BodyGetRow(i).Visible)
				{
					continue;
				}
				for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
				{
					if (_grid.BodyGetCol(j).Visible)
					{
						stringBuilder.Append(GetValueForCopy(Table[i, j].GetDisplayValue()));
						if (j < bodySelection.RightCol)
						{
							stringBuilder.Append("\t");
						}
					}
				}
				if (num > 1)
				{
					stringBuilder.Append("\r\n");
				}
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		string text = stringBuilder.ToString();
		DataObject dataObject = new DataObject();
		if (!string.IsNullOrEmpty(text))
		{
			dataObject.SetText(text);
		}
		try
		{
			dataObject.SetData("AuditaiClipboardCell", new ClipboardCell
			{
				TableId = Table.Id.Value,
				CellId = Table[_grid.BodyRow, _grid.BodyCol].Id.Value
			});
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		try
		{
			System.Windows.Forms.Clipboard.SetDataObject(dataObject);
		}
		catch (ExternalException ex3)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
		}
		SetClipboard();
	}

	private int GetVisibleRowsCountAfterRow(int gridBodyStartRowIndex)
	{
		int num = 0;
		int count = _grid.Rows.Count;
		for (int i = gridBodyStartRowIndex + _grid.Rows.Fixed; i < count; i++)
		{
			if (_grid.Rows[i].Visible)
			{
				num++;
			}
		}
		return num;
	}

	private int GetVisibleColsCountAfterCol(int gridBodyStartColIndex)
	{
		int num = 0;
		int count = _grid.Cols.Count;
		for (int i = gridBodyStartColIndex + _grid.Cols.Fixed; i < count; i++)
		{
			if (_grid.Cols[i].Visible)
			{
				num++;
			}
		}
		return num;
	}

	private bool IsSelectedRangeOnlyContainsFormulaArea(C1.Win.C1FlexGrid.CellRange gridRange)
	{
		if (gridRange.LeftCol < 0 || gridRange.RightCol >= _grid.Cols.Count)
		{
			return false;
		}
		int @fixed = _grid.Cols.Fixed;
		for (int i = gridRange.LeftCol; i <= gridRange.RightCol; i++)
		{
			if (i >= @fixed)
			{
				int index = i - @fixed;
				Auditai.Model.Column column = Table.Columns[index];
				if (_grid.Cols[i].Visible && !column.IsAllowManualInputValueFormulaColumn)
				{
					return false;
				}
			}
		}
		return true;
	}

	public async Task PasteValue()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		List<Tuple<Auditai.Model.Cell, object>> newValues = new List<Tuple<Auditai.Model.Cell, object>>();
		try
		{
			if (_isInPasting)
			{
				return;
			}
			_isInPasting = true;
			bool isOnlySelectFormulaArea = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
			pnlGrid.SuspendDrawing();
			_grid.BeginUpdate();
			int row = _grid.BodyRow;
			int col = _grid.BodyCol;
			ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
			ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
			progressRuntimeData.NextStep("正在进行粘贴准备，可能时间较长，请耐心等待…");
			progressRuntimeData.UpdateProgress(0.5f);
			List<List<object>> ret = null;
			await progressForm.ShowDialog(progressRuntimeData, async delegate
			{
				await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
				try
				{
					ret = await Task.Run((Func<List<List<object>>>)ClipboardUtil.GetClipboardAsTable);
				}
				catch (Exception ex3)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "复制粘贴的数据存在异常，请尝试重新复制粘贴。\r\n异常信息描述: " + ex3.Message);
					ex3.Log("解析剪切板上的数据时发生了未预期的异常");
					return;
				}
				if (ret != null && ret.Count > 0)
				{
					int currentLicenseAllowPasteTableRowsCount = SoftwareLicenseManager.GetCurrentLicenseAllowPasteTableRowsCount(ret.Count);
					if (currentLicenseAllowPasteTableRowsCount < ret.Count)
					{
						List<List<object>> list = new List<List<object>>();
						for (int i = 0; i < currentLicenseAllowPasteTableRowsCount; i++)
						{
							list.Add(ret[i]);
						}
						ret = list;
					}
					int num = ret.Count - GetVisibleRowsCountAfterRow(row);
					if (num < 0)
					{
						num = 0;
					}
					int num2 = ret[0].Count - GetVisibleColsCountAfterCol(col);
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num > 0 || num2 > 0)
					{
						if (num > 0 && !SoftwareLicenseManager.IsAllowAddTableRows())
						{
							return;
						}
						if (num2 > 0 && !SoftwareLicenseManager.IsAllowAddColumn(showDialog: true))
						{
							num2 = 0;
						}
						if (SoftwareLicenseManager.IsTableRowsAndColsOutOfLicenseLimit(Table.Rows.Count + num, Table.Columns.Count + num2))
						{
							return;
						}
						if (!HasSchemaPermission())
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您没有表格的结构调整权限，无法通过粘贴生成新的列");
							return;
						}
					}
					if (num2 > 0)
					{
						Table.Columns.Append(num2);
						Program.MainForm.View.Invoke(new Action(PopulateColumns));
					}
					if (num > 0)
					{
						Table.Rows.Append(num);
						Program.MainForm.View.Invoke(new Action(PopulateRows));
					}
				}
				progressRuntimeData.UpdateProgress(0.9f);
				List<List<object>> list2 = ret;
				if (list2 != null && list2.Count > 0)
				{
					int count = list2.Count;
					int count2 = list2[0].Count;
					int startRow = _grid.BodyRow;
					int startCol = _grid.BodyCol;
					int num3 = 0;
					int num4 = 0;
					int count3 = Table.Rows.Count;
					int count4 = Table.Columns.Count;
					int pasteEndRowIndex = count3 - 1;
					int pasteEndColIndex = count4 - 1;
					int bodyRowSel = _grid.BodyRowSel;
					int bodyColSel = _grid.BodyColSel;
					bool isTableExistFillFormula = IsTableExistFillFormula();
					for (row = startRow; row < count3; row++)
					{
						if (num3 >= count && row > bodyRowSel)
						{
							pasteEndRowIndex = row - 1;
							break;
						}
						if (!_grid.BodyGetRow(row).Visible)
						{
							num4++;
						}
						else
						{
							num3++;
							int num5 = 0;
							int num6 = 0;
							for (col = startCol; col < count4; col++)
							{
								if (num6 >= count2 && col > bodyColSel)
								{
									pasteEndColIndex = col - 1;
									break;
								}
								if (!Table.Columns[col].Visible)
								{
									num5++;
								}
								else
								{
									num6++;
									int index = (row - startRow - num4) % count;
									int index2 = (col - startCol - num5) % count2;
									Auditai.Model.Cell cell = Table[row, col];
									if (CanEditCell(cell, isTableExistFillFormula) && !Table.MergedCells.Any((CellMerge m) => m.ContainsAndNotTopLeft(cell)) && (!cell.HasCellFormulaOrColumnFormula || isOnlySelectFormulaArea))
									{
										try
										{
											object obj = list2[index][index2];
											double result;
											object item = ((obj is string text && text.EndsWith("%") && double.TryParse(text.TrimEnd('%'), out result) && cell.DisplayDataType == typeof(double)) ? ((object)(result / 100.0)) : ((!(cell.DisplayDataType == typeof(bool))) ? Auditai.Model.Cell.ChangeDataTypeImpl(obj, cell.DisplayDataType) : ((object)"√".Equals(obj))));
											newValues.Add(Tuple.Create(cell, item));
										}
										catch
										{
										}
									}
								}
							}
						}
					}
					Program.MainForm.View.Invoke((Action)delegate
					{
						Select(startRow, startCol, pasteEndRowIndex, pasteEndColIndex);
					});
					BatchCellUpdateValueCommand command = new BatchCellUpdateValueCommand(Table, newValues)
					{
						IsExistManualInputValue = true
					};
					Table.CommandsManager.ExecuteCommand(command);
					Program.MainForm.View.Invoke((Action)delegate
					{
						ForceRefresh();
						PopulateMerges();
					});
				}
			}, 1000);
		}
		catch (Win32Exception ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (TableModelException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		finally
		{
			_isInPasting = false;
			_grid.EndUpdate();
			DoLayout();
			pnlGrid.ResumeDrawing();
		}
	}

	public void PasteFormat()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || Clipboard == null || !Clipboard.Item1.IsExisting || !Clipboard.Item2.IsExisting || IsTableLocked)
		{
			return;
		}
		Auditai.Model.Table table = Clipboard.Item1.Row.Table;
		int index = Clipboard.Item1.Row.Index;
		int index2 = Clipboard.Item1.Column.Index;
		int num = Clipboard.Item2.Row.Index - index + 1;
		int num2 = Clipboard.Item2.Column.Index - index2 + 1;
		int bodyRow = _grid.BodyRow;
		int num3 = Math.Min(Math.Max(num, _grid.BodyRowSel - bodyRow + 1), Table.Rows.Count - bodyRow);
		int bodyCol = _grid.BodyCol;
		int num4 = Math.Min(Math.Max(num2, _grid.BodyColSel - bodyCol + 1), Table.Columns.Count - bodyCol);
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				Auditai.Model.Cell srcCell = table[index + i % num, index2 + j % num2];
				Auditai.Model.Cell cell = Table[bodyRow + i, bodyCol + j];
				if (!CanEditColumn(cell.Column) || !CanEditRow(cell.Row))
				{
					continue;
				}
				if (Table == table)
				{
					cell.UpdateStyle(srcCell.Style);
					continue;
				}
				Auditai.Model.CellStyle style = ((srcCell.Style == null) ? null : Table.CellStyles.MutateAndGet(null, delegate(Auditai.Model.CellStyle cs)
				{
					cs.CopyFrom(srcCell.Style);
				}));
				cell.UpdateStyle(style);
			}
		}
		_grid.Invalidate();
	}

	public void PasteFormula()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		try
		{
			if (Clipboard == null)
			{
				return;
			}
			Auditai.Model.Cell item = Clipboard.Item1;
			string formula = item.Formula;
			if (string.IsNullOrWhiteSpace(formula))
			{
				return;
			}
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				if (!_grid.BodyGetRow(i).IsVisible)
				{
					continue;
				}
				for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
				{
					try
					{
						bool flag = item.Row.Index == i;
						bool flag2 = item.Column.Index == j;
						Auditai.Model.Cell cell = Table[i, j];
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(formula);
						FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Auditai.Model.Project.Current);
						if (flag || flag2)
						{
							string text = formulaEvaluator.PasteFormula(resolver, item, cell);
							cell.UpdateFormula(text);
						}
						else
						{
							cell.UpdateFormula(formula);
						}
					}
					catch
					{
					}
				}
			}
			SetFormulaContext();
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	public void CopyHeaderCellFormula()
	{
		CopiedHeaderCell = Table[_grid.BodyRow, _grid.BodyCol];
	}

	public void PasteHeaderCellFormula()
	{
		if (!CopiedHeaderCell.HasHeaderFormula)
		{
			return;
		}
		for (int i = _grid.BodySelection.LeftCol; i <= _grid.BodySelection.RightCol; i++)
		{
			Auditai.Model.Cell cell = Table[_grid.BodyRow, i];
			FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Auditai.Model.Project.Current);
			try
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(CopiedHeaderCell.HeaderFormula);
				string text = formulaEvaluator.PasteHeaderCellFormula(resolver, CopiedHeaderCell, cell);
				cell.UpdateHeaderFormula(text);
				PopulateTable();
			}
			catch (FormulaBadReferenceException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
			}
			catch (FormulaColumnWildcardNoRowException)
			{
			}
			catch (FormulaTypeMismatchException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
			}
			catch (FormulaException ex4)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
			}
		}
	}

	public void CopyColumns()
	{
		CopiedColumns.Clear();
		CopiedColumns.AddRange(Table.Columns.Where((Auditai.Model.Column c) => c.Index >= _grid.BodySelection.LeftCol && c.Index <= _grid.BodySelection.RightCol));
	}

	public void CopyColumnFormula()
	{
		CopiedColumn = Table.Columns[_grid.BodySelection.LeftCol];
	}

	public void PasteColumnFormula()
	{
		if (!CopiedColumn.HasFormula)
		{
			return;
		}
		for (int i = _grid.BodySelection.LeftCol; i <= _grid.BodySelection.RightCol; i++)
		{
			Auditai.Model.Column column = Table.Columns[i];
			FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Auditai.Model.Project.Current);
			try
			{
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(CopiedColumn.Formula);
				string text = formulaEvaluator.PasteColumnFormula(resolver, CopiedColumn, column);
				column.UpdateFormula(text);
				PopulateTable();
			}
			catch (FormulaBadReferenceException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "引用不存在");
			}
			catch (FormulaColumnWildcardNoRowException)
			{
			}
			catch (FormulaTypeMismatchException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "参数类型错误");
			}
			catch (FormulaException ex4)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex4.Message);
			}
			catch (Exception ex5)
			{
				ex5.Log();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex5.Message);
			}
		}
	}

	public async Task EditConsolidateSettings()
	{
		if (!SoftwareLicenseManager.IsConsolidateOutOfLicenseLimit() && !IsTableLocked)
		{
			ConsolidateSettingsEditor consolidateSettingsEditor = new ConsolidateSettingsEditor
			{
				Table = Table
			};
			if (await consolidateSettingsEditor.ShowDialog() == DialogResult.OK)
			{
				await ExecuteConsolidate(showDataCols: true);
			}
		}
	}

	public async Task ExecuteConsolidate(bool showDataCols)
	{
		if (SoftwareLicenseManager.IsConsolidateOutOfLicenseLimit())
		{
			return;
		}
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		progressRuntimeData.NextStep("正在执行合并报表，请稍候...");
		progressRuntimeData.UpdateProgress(0.8f);
		await progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			if (await ValidateConsolidateSettings())
			{
				Program.MainForm.View.Invoke((Action)delegate
				{
					progressRuntimeData.UpdateProgress(0.9f);
					ExecuteConsolidateImpl(showDataCols);
					if (_grid.BodyColsCount != Table.Columns.Count)
					{
						PopulateTable();
					}
				});
			}
		}, 1000);
	}

	/// <summary>
	/// 刷新合并报表数据
	/// 重新从来源项目读取最新数据并更新当前表
	/// </summary>
	public async Task RefreshConsolidate()
	{
		if (SoftwareLicenseManager.IsConsolidateOutOfLicenseLimit())
		{
			return;
		}
		if (Table == null || Table.ConsolidateSettings == null || Table.ConsolidateSettings.Sources.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表没有配置合并报表数据");
			return;
		}
		await ExecuteConsolidate(showDataCols: true);
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并报表数据已刷新");
	}

	public void Subtotal()
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && !IsTableLocked)
		{
			CancelSubtotal();
			for (int i = _grid.BodyCol; i <= _grid.BodyColSel; i++)
			{
				Table.Columns[i].UpdateSubtotalAttribs(ColumnSubtotal.GroupBy);
			}
			SubtotalImpl();
		}
	}

	public void SubtotalImpl()
	{
		if (Table.Rows.Count == 0)
		{
			return;
		}
		_grid.BeginUpdate();
		BeginBatchUpdateValue();
		SortAscending();
		Auditai.Model.CellStyle style = Table.CellStyles.MutateAndGet(Table.DefaultStyle, delegate(Auditai.Model.CellStyle cs)
		{
			cs.Align = CellTextAlign.MiddleCenter;
		});
		List<Auditai.Model.Column> list = Table.Columns.Where((Auditai.Model.Column c) => c.SubtotalAttributes == ColumnSubtotal.GroupBy).ToList();
		List<int> groupByIndexes = list.Select((Auditai.Model.Column gb) => gb.Index).ToList();
		List<IGrouping<List<Auditai.Model.Cell>, Auditai.Model.Row>> list2 = Table.Rows.GroupBy((Auditai.Model.Row row) => groupByIndexes.Select((int i) => Table[row.Index, i]).ToList(), CellListByValueEqualsComparer.Instance).ToList();
		IEnumerable<int> enumerable = from c in Table.Columns
			where c.SubtotalAttributes != ColumnSubtotal.GroupBy && (c.Style?.Format?.IsNumericFormat()).GetValueOrDefault()
			select c.Index;
		Auditai.Model.Cell cell;
		foreach (IGrouping<List<Auditai.Model.Cell>, Auditai.Model.Row> item in list2)
		{
			Auditai.Model.Row row2 = item.Last();
			int num = row2.Index + 1;
			Table.Rows.Insert(num, 1);
			Table.Rows[num].UpdateRole(RowRole.Subtotal);
			Table.Rows[num].UpdateHeight(Table.Rows[num - 1].Height);
			cell = Table[num, list.First().Index];
			if (list.Count > 1)
			{
				Table.MergeCells(num, list[0].Index, num, list.Last().Index);
			}
			cell.UpdateFormula("\"" + string.Join(" ", item.Key.Select((Auditai.Model.Cell c) => c.Value?.ToString() ?? string.Empty)) + " 小计\"");
			cell.UpdateStyle(style);
		}
		Auditai.Model.Row row3 = Table.Rows.LastOrDefault((Auditai.Model.Row r) => r.Role == RowRole.Total);
		if (row3 == null)
		{
			Table.Rows.Append(1);
			row3 = Table.Rows[Table.Rows.Count - 1];
		}
		int index = row3.Index;
		Table.Rows[index].UpdateRole(RowRole.Total);
		cell = Table[index, list.First().Index];
		if (list.Count > 1)
		{
			Table.MergeCells(index, list[0].Index, index, list.Last().Index);
		}
		cell.UpdateFormula("\"总计\"");
		cell.UpdateStyle(style);
		foreach (int item2 in enumerable)
		{
			string text = null;
			text = ((!Table.RowOwnerLoad) ? $"Sum([2:{Table.Id}:{Table.Columns[item2].Id}])" : $"Sum([3:{Table.Id}:{Table[0, item2].Id}:{Table[Table.Rows.Count - 1, item2].Id}])");
			Table[index, item2].UpdateFormula(text);
		}
		foreach (IGrouping<List<Auditai.Model.Cell>, Auditai.Model.Row> item3 in list2)
		{
			Auditai.Model.Row firstRow = item3.First();
			Auditai.Model.Row row4 = item3.Last();
			int row5 = row4.Index + 1;
			foreach (int item4 in enumerable)
			{
				string arg = string.Join("AND", list.Select(delegate(Auditai.Model.Column gb)
				{
					string text3 = null;
					text3 = ((!Table.RowOwnerLoad) ? $"[2:{Table.Id}:{gb.Id}]" : $"[3:{Table.Id}:{Table[0, gb.Index].Id}:{Table[Table.Rows.Count - 1, gb.Index].Id}]");
					return $"{text3}=[1:{Table.Id}:{Table[firstRow.Index, gb.Index].Id}]";
				}));
				string text2 = $"SumIf({arg},[2:{Table.Id}:{Table.Columns[item4].Id}])";
				Table[row5, item4].UpdateFormula(text2);
			}
		}
		EndBatchUpdateValue();
		PopulateRows();
		PopulateMerges();
		_grid.EndUpdate();
	}

	public void SumColumns()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		try
		{
			pnlGrid.SuspendDrawing();
			_grid.BeginUpdate();
			Auditai.Model.Column column = Table.Columns[0];
			if (column.Style?.DataType == typeof(double))
			{
				column = Table.Columns.FirstOrDefault((Auditai.Model.Column c) => c.Visible && (c.Style == null || c.Style.DataType == null || c.Style.DataType == typeof(string))) ?? column;
			}
			Table.SumColumns(column);
			PopulateRows();
			PopulateMerges();
		}
		catch (TableModelException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			_grid.EndUpdate();
			DoLayout();
			pnlGrid.ResumeDrawing();
		}
	}

	public void SumColumnsHeaderCell()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		BeginBatchUpdateValue();
		int bodyRow = _grid.BodyRow;
		int headerLastRow = Table[bodyRow, 0].GetHeaderLastRow();
		Auditai.Model.Row row = Table.Rows.Skip(bodyRow + 1).Take(headerLastRow - bodyRow).LastOrDefault((Auditai.Model.Row r) => r.Role == RowRole.Total);
		if (row == null)
		{
			Table.Rows.Insert(headerLastRow + 1, 1);
			PopulateRows();
			row = Table.Rows[headerLastRow + 1];
		}
		int bodyCol = _grid.BodyCol;
		int bodyColSel = _grid.BodyColSel;
		Auditai.Model.Cell cell = Table[row.Index, 0];
		cell.UpdateFormula("\"合计\"");
		cell.UpdateStyle(Table.CellStyles.MutateAndGet(Table.DefaultStyle, delegate(Auditai.Model.CellStyle cs)
		{
			cs.Align = CellTextAlign.MiddleCenter;
		}));
		row.UpdateRole(RowRole.Total);
		Auditai.Model.Row row2 = Table.Rows.Skip(bodyRow + 1).Take(headerLastRow - bodyRow).LastOrDefault((Auditai.Model.Row r) => r.Role == RowRole.Normal || r.Role == RowRole.Among || r.Role == RowRole.Minus);
		if (row2 != null)
		{
			for (int i = 0; i < Table.Columns.Count; i++)
			{
				Auditai.Model.Column column = Table.Columns[i];
				if (Table[row2.Index, column.Index].DisplayFormat.IsNumericFormat())
				{
					string text = $"SUM([6:{Table.Id}:{Table[bodyRow, i].Id}])";
					Table[row.Index, i].UpdateFormula(text);
				}
			}
		}
		EndBatchUpdateValue();
		PopulateRowsHeight();
	}

	public void CancelSubtotal()
	{
		if (IsTableLocked)
		{
			return;
		}
		_grid.BeginUpdate();
		foreach (Auditai.Model.Row item in Table.Rows.ToList())
		{
			if (item.Role == RowRole.Subtotal || item.Role == RowRole.Total)
			{
				item.Remove();
			}
		}
		PopulateRows();
		_grid.EndUpdate();
		foreach (Auditai.Model.Column column in Table.Columns)
		{
			if (column.SubtotalAttributes == ColumnSubtotal.GroupBy)
			{
				column.UpdateSubtotalAttribs(ColumnSubtotal.None);
			}
		}
		PopulateMerges();
	}

	public void CancelSumColumns()
	{
		if (!IsTableLocked)
		{
			_grid.BeginUpdate();
			Table.CancelSumColumns();
			PopulateRows();
			_grid.EndUpdate();
		}
	}

	public void FinishEditorInputStatus(bool isCancel = false)
	{
		if (Table == null)
		{
			return;
		}
		TitleEditor.FinishEditorInputStatus(isCancel);
		FootEditor.FinishEditorInputStatus(isCancel);
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
			exception.Log("结束表格的编辑输入状态时发生了未预期的异常");
		}
	}

	public void Find()
	{
		ShowReplaceForm(replace: false);
	}

	public void Replace()
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && !IsTableLocked)
		{
			ShowReplaceForm(replace: true);
		}
	}

	public void Indent()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			int num = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[num].Role == RowRole.Header)
			{
				num++;
			}
			int displayMargin = Table[num, _grid.BodyCol].DisplayMargin;
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Margin, displayMargin + 10);
		}
	}

	public void Unindent()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			int num = _grid.BodyRow;
			if (!_isSelectingHeaderCell && Table.Rows[num].Role == RowRole.Header)
			{
				num++;
			}
			int displayMargin = Table[num, _grid.BodyCol].DisplayMargin;
			MutateCellStyle((Auditai.Model.CellStyle cs) => cs.Margin, displayMargin - 10);
		}
	}

	public void RibbonTopLeftClicked()
	{
		SetAlign(CellTextAlign.TopLeft);
	}

	public void RibbonTopCenterClicked()
	{
		SetAlign(CellTextAlign.TopCenter);
	}

	public void RibbonTopRightClicked()
	{
		SetAlign(CellTextAlign.TopRight);
	}

	public void RibbonMiddleLeftClicked()
	{
		SetAlign(CellTextAlign.MiddleLeft);
	}

	public void RibbonMiddleCenterClicked()
	{
		SetAlign(CellTextAlign.MiddleCenter);
	}

	public void RibbonMiddleRightClicked()
	{
		SetAlign(CellTextAlign.MiddleRight);
	}

	public void RibbonBottomLeftClicked()
	{
		SetAlign(CellTextAlign.BottomLeft);
	}

	public void RibbonBottomCenterClicked()
	{
		SetAlign(CellTextAlign.BottomCenter);
	}

	public void RibbonBottomRightClicked()
	{
		SetAlign(CellTextAlign.BottomRight);
	}

	public void RibbonIncreaseRowHeightClicked()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			int height = Table.Rows[_grid.BodyRow].Height;
			SetRowHeight(height + 5);
		}
	}

	public void RibbonDecreaseRowHeightClicked()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			int height = Table.Rows[_grid.BodyRow].Height;
			SetRowHeight(height - 5);
		}
	}

	public void RibbonIncreaseColumnWidthClicked()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			int width = Table.Columns[_grid.BodyCol].Width;
			SetColumnWidth(width + 5);
		}
	}

	public void RibbonDecreaseColumnWidthClicked()
	{
		if (!IsTableLocked && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
		{
			int width = Table.Columns[_grid.BodyCol].Width;
			SetColumnWidth(width - 5);
		}
	}

	public void BeginEditColHeaders(int col)
	{
		if (!HasSchemaPermission() || _isEditingHeaders)
		{
			return;
		}
		Program.MainForm.SuspendNavPanelVisible();
		try
		{
			gridDecorator.SetDirty();
			_isEditingHeaders = true;
			_owner.SwitchStateTo(MainFormView.EditingColHeader);
			_grid.BeginUpdate();
			_grid.Cursor = Cursors.Arrow;
			for (int num = _grid.MergedRanges.Count - 1; num >= 0; num--)
			{
				if (_grid.MergedRanges[num].TopRow < _grid.Rows.Fixed)
				{
					_grid.MergedRanges.RemoveAt(num);
				}
			}
			_grid.Rows.RemoveRange(0, _grid.Rows.Fixed);
			_grid.Rows.Fixed = 0;
			for (int i = 0; i < _grid.Rows.Count; i++)
			{
				_grid.Rows[i].AllowEditing = false;
			}
			_grid.Rows.Insert(0);
			PopulateEditingColumnHeaders();
			if (Table.HeaderMode == TableHeaderMode.Fixed)
			{
				_grid.Rows[0].AllowEditing = false;
			}
			_grid.Select(0, col + _grid.Cols.Fixed);
			_grid.EndUpdate();
			_grid.FilterManager.IsEditingColHeader = true;
		}
		finally
		{
			Program.MainForm.ResumeNavPanelVisible();
		}
	}

	public void EndEditColHeaders()
	{
		if (!_isEditingHeaders)
		{
			return;
		}
		SuspendNavPanelVisibleDrawing();
		try
		{
			gridDecorator.SetDirty();
			int bodyCol = _grid.BodyCol;
			_isEditingHeaders = false;
			_owner.SwitchStateTo(MainFormView.Table);
			pnlGrid.SuspendDrawing();
			_grid.Rows.Remove(0);
			for (int i = 0; i < Table.Rows.Count; i++)
			{
				_grid.Rows[i].AllowEditing = !Table.Rows[i].IsLocked && CanEditRow(Table.Rows[i]);
			}
			PopulateColumns();
			Select(0, bodyCol);
			_grid.FilterManager.IsEditingColHeader = false;
			pnlGrid.ResumeDrawing();
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	public void ShowValidationPane()
	{
		pnlValidation.Show();
		pnlValidation.SizeRatio = 33.0;
		AppCommands.ShowValidation.IsPressed = true;
		_owner.StatusBar.ValidationFormula.Checked = true;
	}

	public void HideValidationPane()
	{
		pnlValidation.Hide();
		AppCommands.ShowValidation.IsPressed = false;
		_owner.StatusBar.ValidationFormula.Checked = false;
	}

	public void ShowFootPane()
	{
		AppCommands.ShowFoot.IsPressed = true;
		_owner.StatusBar.TableFoot.Checked = true;
		cmdFoot.Pressed = true;
		FootEditor.View.Visible = true;
		DoLayout();
	}

	public void HideFootPane()
	{
		AppCommands.ShowFoot.IsPressed = false;
		_owner.StatusBar.TableFoot.Checked = false;
		cmdFoot.Pressed = false;
		FootEditor.View.Visible = false;
		DoLayout();
	}

	public void SetHeaderMode(TableHeaderMode hm)
	{
		Table.UpdateHeaderMode(hm);
		PopulateColumns();
	}

	public void CalcSumRows()
	{
		_grid.BeginUpdate();
		foreach (Auditai.Model.Row item in Table.Rows.Where((Auditai.Model.Row r) => r.Role == RowRole.Subtotal || r.Role == RowRole.Total))
		{
			foreach (Auditai.Model.Cell cell in item.GetCells())
			{
				cell.TryApplyFormula();
			}
		}
		_grid.EndUpdate();
	}

	public void CalcCurrentTable()
	{
		pnlGrid.SuspendDrawing();
		Table.CalculateRecursive();
		_isUpdatingView = true;
		List<Auditai.Model.Row> list = Table.TryApplyFormula(evalLqDistinct: true);
		PopulateRows();
		if (list.Count <= UserConfig.RowsApplyFormulaAuto)
		{
			Application.DoEvents();
			foreach (Auditai.Model.Row item in list)
			{
				if (item.IsExisting)
				{
					_grid.AutoSizeRow(item.Index + _grid.Rows.Fixed);
					if (_grid.BodyGetRow(item.Index).Height < Table.Rows.DefaultHeight)
					{
						item.UpdateHeight(Table.Rows.DefaultHeight);
						_grid.BodyGetRow(item.Index).Height = item.Height;
					}
					else
					{
						item.UpdateHeight(_grid.BodyGetRow(item.Index).Height);
					}
				}
			}
		}
		PopulateColumns();
		PopulateMerges();
		PopulateTopLeftCell();
		TitleEditor.Populate();
		FootEditor.Populate();
		_isUpdatingView = false;
		pnlGrid.ResumeDrawing();
	}

	public async Task CalcAllTables()
	{
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		progressRuntimeData.NextStep("准备开始进行运算...");
		TaskProgressValueUpdater updater = new TaskProgressValueUpdater(0f, 0.9f, progressRuntimeData.UpdateProgress, progressRuntimeData.UpdateMessage);
		await progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			FormulaManagerTransitional formulaManagerTransitional = new FormulaManagerTransitional(Auditai.Model.Project.Current);
			await formulaManagerTransitional.CalculateAllTables(updater);
			Program.MainForm.View.Invoke(new Action(PopulateTable));
		}, 1000);
	}

	public void LockRows()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			if (Program.MainForm.CurrentProject.Kind == ProjectType.Project)
			{
				Table.Rows[i].UpdateLocker(Auditai.Model.User.Current.Id);
			}
			else
			{
				Table.Rows[i].UpdateLocker(-1L);
			}
			PopulateRow(Table.Rows[i], _grid.BodyGetRow(i));
			for (int j = _grid.BodyCol; j <= _grid.BodyColSel; j++)
			{
				Table[i, j].ForceTagValueAndFormulaDirty();
			}
		}
	}

	public void UnlockRows()
	{
		if (_grid.BodyRow >= 0 && _grid.BodyCol >= 0 && !IsTableLocked)
		{
			for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
			{
				Auditai.Model.Row row = Table.Rows[i];
				row.UpdateLocker(0L);
				PopulateRow(row, _grid.BodyGetRow(i));
			}
		}
	}

	public void LockColumns()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		int bodyCol = _grid.BodyCol;
		int bodyColSel = _grid.BodyColSel;
		for (int i = bodyCol; i <= bodyColSel; i++)
		{
			Table.Columns[i].UpdateStyle(Table.CellStyles.MutateAndGet(Table.Columns[i].Style, delegate(Auditai.Model.CellStyle s)
			{
				s.Locker = ((Program.MainForm.CurrentProject.Kind == ProjectType.Project) ? Auditai.Model.User.Current.Id : (-1));
			}));
		}
		foreach (Auditai.Model.Cell item in Table.EnumerateCellRange(0, bodyCol, Table.Rows.Count - 1, bodyColSel))
		{
			item.UpdateStyle(Table.CellStyles.MutateAndGet(item.Style, delegate(Auditai.Model.CellStyle s)
			{
				s.Locker = null;
			}));
		}
		for (int j = _grid.BodyRow; j <= _grid.BodyRowSel; j++)
		{
			for (int k = _grid.BodyCol; k <= _grid.BodyColSel; k++)
			{
				Table[j, k].ForceTagValueAndFormulaDirty();
			}
		}
	}

	public void LockCells()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		foreach (Auditai.Model.Cell item in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
		{
			item.UpdateStyle(Table.CellStyles.MutateAndGet(item.Style, delegate(Auditai.Model.CellStyle s)
			{
				s.Locker = ((Program.MainForm.CurrentProject.Kind == ProjectType.Project) ? Auditai.Model.User.Current.Id : (-1));
			}));
		}
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			for (int j = _grid.BodyCol; j <= _grid.BodyColSel; j++)
			{
				Table[i, j].ForceTagValueAndFormulaDirty();
			}
		}
	}

	public void UnlockColumns()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		int bodyCol = _grid.BodyCol;
		int bodyColSel = _grid.BodyColSel;
		for (int i = bodyCol; i <= bodyColSel; i++)
		{
			Auditai.Model.Column column = Table.Columns[i];
			column.UpdateStyle(Table.CellStyles.MutateAndGet(column.Style, delegate(Auditai.Model.CellStyle cs)
			{
				cs.Locker = null;
			}));
		}
	}

	public void UnlockCells()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0 || IsTableLocked)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		foreach (Auditai.Model.Cell item in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
		{
			item.UpdateStyle(Table.CellStyles.MutateAndGet(item.Style, delegate(Auditai.Model.CellStyle cs)
			{
				cs.Locker = null;
			}));
		}
	}

	public void MakerSign()
	{
		if (IsTableLocked)
		{
			return;
		}
		try
		{
			TitleEditor.MakerSign();
		}
		catch (ArgumentOutOfRangeException)
		{
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表未留有默认的签名位置，是否要在当前选定单元格中签名？", MessageBoxButtons.OKCancel) == DialogResult.OK && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
			{
				Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
				if (CanEditCell(cell, IsTableExistFillFormula()))
				{
					string value = Program.MainForm.CurrentProject.DataReferenceManager.ReplaceString(UserSet.Config.SignatureStyle.SignatureFormat, new DataReferenceEvaluationContext
					{
						CurrentTreeNode = Table.TreeNode,
						Project = Table.Project
					});
					cell.UpdateValue(value);
				}
			}
		}
	}

	public void CheckerSign()
	{
		if (IsTableLocked)
		{
			return;
		}
		try
		{
			TitleEditor.CheckerSign();
		}
		catch (ArgumentOutOfRangeException)
		{
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表未留有默认的签名位置，是否要在当前选定单元格中签名？", MessageBoxButtons.OKCancel) == DialogResult.OK && _grid.BodyRow >= 0 && _grid.BodyCol >= 0)
			{
				Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
				if (CanEditCell(cell, IsTableExistFillFormula()))
				{
					string value = Program.MainForm.CurrentProject.DataReferenceManager.ReplaceString(UserSet.Config.SignatureStyle.ReviewSignFormat, new DataReferenceEvaluationContext
					{
						Project = Table.Project,
						CurrentTreeNode = Table.TreeNode
					});
					cell.UpdateValue(value);
				}
			}
		}
	}

	public void BeginFormatBrush()
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		if (IsTableLocked)
		{
			AppCommands.FormatBrush.IsPressed = false;
			return;
		}
		SuspendNavPanelVisibleDrawing();
		try
		{
			_isFormatBrushing = true;
			_grid.AllowEditing = false;
			SetClipboard();
			_owner.SwitchStateTo(MainFormView.FormatBrush);
			FormulaEditor.View.Enabled = false;
			_grid.Cursor = _curFormatBrush;
			AppCommands.Information.ShowInformation("状态提示", "当前处于格式刷状态，按Esc键可退出格式刷状态。");
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	public void EndFormatBrush()
	{
		SuspendNavPanelVisibleDrawing();
		try
		{
			_grid.AllowEditing = true;
			AppCommands.FormatBrush.IsPressed = false;
			_owner.SwitchStateTo(MainFormView.Table);
			_grid.Cursor = Cursors.Default;
			AppCommands.Information.HideInformation();
			_isFormatBrushing = false;
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	public void InsertSymbolsDialog()
	{
		Process.Start("charmap");
	}

	public void LockTable()
	{
		if (Program.MainForm.CurrentProject.Kind == ProjectType.Project)
		{
			Table.UpdateLocker(Auditai.Model.User.Current.Id);
		}
		else
		{
			Table.UpdateLocker(-1L);
		}
		UpdateViewStateForLocker();
		PopulateTopLeftCell();
	}

	public void UnlockTable()
	{
		Table.UpdateLocker(0L);
		UpdateViewStateForLocker();
		PopulateTopLeftCell();
	}

	public void SetRowOwnerExclusive(bool set)
	{
		if (CanSetRowOwnerExclusive())
		{
			if (set)
			{
				AppCommands.RowOwnerExclusive.IsPressed = true;
				Table.TreeNode.UpdateRowWrite(value: true);
			}
			else
			{
				AppCommands.RowOwnerExclusive.IsPressed = false;
				Table.TreeNode.UpdateRowWrite(value: false);
			}
			PopulateTopLeftCell();
		}
	}

	private bool CanSetRowOwnerExclusive()
	{
		if (!Auditai.Model.Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id))
		{
			return false;
		}
		if (Auditai.Model.Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return false;
	}

	public void SetRowOwnerLoad(bool set)
	{
		if (CanSetRowOwnerExclusive())
		{
			if (set)
			{
				AppCommands.RowOwnerLoad.IsPressed = true;
				Table.TreeNode.UpdateRowRead(value: true);
				Table.TreeNode.TagPermissionsDirty();
				Table.TreeNode.Permissions.Schema = new Permission
				{
					GrantAll = false
				};
			}
			else
			{
				AppCommands.RowOwnerLoad.IsPressed = false;
				Table.TreeNode.UpdateRowRead(value: false);
			}
		}
	}

	public void ReloadFromDb()
	{
		Program.MainForm.TableEditor.FinishEditorInputStatus(isCancel: true);
		Program.MainForm.TicketInputEditor.FinishEditorInputStatus(isCancel: true);
		if (Table != null)
		{
			Table._loaded = false;
			Table.LoadAndReturn();
		}
		if (Program.MainForm.State.ViewKind == MainFormView.Table)
		{
			PopulateTable();
		}
		else if (Program.MainForm.State.ViewKind == MainFormView.TicketInput)
		{
			Program.MainForm.TicketInputEditor.Populate();
		}
	}

	public void BeginBatchUpdateValue()
	{
		_grid.BeginUpdate();
		Table.BeginBatchUpdateValue();
	}

	public void EndBatchUpdateValue()
	{
		Table.EndBatchUpdateValue();
		_grid.EndUpdate();
	}

	public void Invalidate()
	{
		if (_table != null)
		{
			gridDecorator.Prepare();
			_grid.Invalidate();
			TitleEditor.View.Invalidate();
			FootEditor.View.Invalidate();
		}
	}

	public void Select(int r1, int c1, int r2, int c2)
	{
		try
		{
			_grid.BodySelect(r1, c1, r2, c2);
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	public void Select(int r, int c)
	{
		try
		{
			_grid.BodySelect(r, c);
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	public void ShowConditionCells(CellsOperand condCells)
	{
		_grid.BeginUpdate();
		int bodyRowsCount = _grid.BodyRowsCount;
		for (int i = 0; i < bodyRowsCount; i++)
		{
			_grid.BodyGetRow(i).Visible = false;
		}
		foreach (int row in condCells.Rows)
		{
			try
			{
				_grid.BodyGetRow(row).Visible = true;
			}
			catch (ArgumentOutOfRangeException exception)
			{
				exception.Log(string.Format("{0}, case {1}, _grid.BodyGetRow(row), row={2},Rows.Count={3},Rows.Fixed={4}", "_ttpComment_LinkClicked", "condCells", row, _grid.Rows.Count, _grid.Rows.Fixed));
			}
		}
		_grid.EndUpdate();
		_grid.FilterManager.IsFilteredExternally = true;
		Invalidate();
		BodySelectionChanged_Stats();
	}

	public void SelectColumn(int c)
	{
		try
		{
			if (_grid.BodyRowsCount > 0)
			{
				Point scrollPosition = _grid.ScrollPosition;
				_grid.BodySelect(0, c, _grid.BodyRowsCount - 1, c);
				_grid.ScrollPosition = scrollPosition;
			}
		}
		catch (IndexOutOfRangeException)
		{
		}
	}

	public async Task ExportAllAttachment()
	{
		try
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "请选择保存附件的文件夹位置：";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				await ExportTableAttachments(Table, folderBrowserDialog.SelectedPath);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "附件导出失败:\r\n" + ex.ToString(), MessageBoxButtons.OK, "错误!");
		}
	}

	private void TraceTableCell(Auditai.Model.Table table, int row, int col)
	{
		try
		{
			if (table == null) return;
			var cell = table[row, col];
			if (cell == null) return;

			// 确定使用的公式
			string formula = cell.Formula;
			if (string.IsNullOrWhiteSpace(formula))
			{
				if (cell.Column?.Formula == null) return;
				formula = cell.Column.Formula;
			}
			if (string.IsNullOrWhiteSpace(formula)) return;

			var project = table.Project;
			if (project == null) return;

			var resolver = new FormulaReferenceModelResolver(project);
			var evaluator = new FormulaEvaluator(formula);
			evaluator.Env = new FormulaEvaluationEnvironment
			{
				Resolver = resolver,
				RowIndex = row,
				HostTable = table,
				RefManager = project.DataReferenceManager,
				RefEvalContext = new DataReferenceEvaluationContext
				{
					Project = project,
					CurrentTreeNode = table.TreeNode
				}
			};

			var tableIds = evaluator.GetReferredTableIds();
			var refs = evaluator.GetReferences(resolver);

			var xBody = new System.Xml.Linq.XElement("div");
			xBody.Add(new System.Xml.Linq.XElement("b", "数据来源追踪（点击跳转）"));

			var tagDic = new Dictionary<string, object>();
			int linkIndex = 0;

			// 表格引用
			if (tableIds.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用表格 ==="));
				foreach (var tid in tableIds)
				{
					var t2 = project.GetTableById(tid);
					string name = t2?.GetCanonicalName() ?? $"表格ID={tid}";
					string href = $"tracetable_{linkIndex++}";
					var tableNode = project.GetNodeById(tid) as TreeTableNode;
					tagDic[href] = tableNode;
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📊 " + name)));
				}
			}

			// 列引用
			if (refs.ColumnReferences.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用列 ==="));
				foreach (var col2 in refs.ColumnReferences)
				{
					string href = $"tracecol_{linkIndex++}";
					tagDic[href] = col2;
					string caption = col2.Caption ?? $"列[{col2.Index}]";
					string tableName2 = col2.Table?.GetCanonicalName() ?? "";
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📋 " + caption + " (" + tableName2 + ")")));
				}
			}

			// 单元格引用
			if (refs.CellReferences.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用单元格 ==="));
				foreach (var cell2 in refs.CellReferences)
				{
					string href = $"tracecell_{linkIndex++}";
					tagDic[href] = cell2;
					string val = cell2.GetDisplayValue(applyZeroFormat: false);
					string tableName3 = cell2.Column?.Table?.GetCanonicalName() ?? "";
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📌 第" + (cell2.Row.Index + 1) + "行: " + val + " (" + tableName3 + ")")));
				}
			}

			// 范围引用
			if (refs.RangeReferences.Count > 0)
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "=== 引用区域 ==="));
				foreach (var range2 in refs.RangeReferences)
				{
					string href = $"tracerange_{linkIndex++}";
					tagDic[href] = range2;
					string tableName4 = range2.Table?.GetCanonicalName() ?? "";
					xBody.Add(new System.Xml.Linq.XElement("p", new System.Xml.Linq.XElement("a", new System.Xml.Linq.XAttribute("href", href), "📐 " + (range2.TopLeft.Row.Index + 1) + ":" + (range2.TopLeft.Column.Index + 1) + " ~ " + (range2.BottomRight.Row.Index + 1) + ":" + (range2.BottomRight.Column.Index + 1) + " (" + tableName4 + ")")));
				}
			}

			// 解析公式
			try
			{
				string display = evaluator.GetDisplayString(resolver);
				xBody.Add(new System.Xml.Linq.XElement("hr"));
				xBody.Add(new System.Xml.Linq.XElement("p", "解析公式: " + display));
			}
			catch
			{
				xBody.Add(new System.Xml.Linq.XElement("p", "解析公式: " + formula));
			}

			_ttpComment.SetText("追踪数据", xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(tagDic);

			var cellRect = _grid.GetCellRect(row, col);
			if (cellRect.Width > 0 && cellRect.Height > 0)
			{
				_ttpComment.Show(_grid, new Point(cellRect.Right, cellRect.Top + cellRect.Height / 2));
			}
		}
		catch (Exception ex)
		{
			ex.Log("TableEditor.TraceTableCell");
		}
	}

	protected async Task ExportTableAttachments(Auditai.Model.Table table, string finalDirName)
	{
		if (table.CellPropManager.DicCellAttachments.Count == 0)
		{
			return;
		}
		List<CellAttachment> list = new List<CellAttachment>();
		foreach (Auditai.Model.CellAttachments value in table.CellPropManager.DicCellAttachments.Values)
		{
			if (value.Status != SyncStatus.LocalDeleted && value.Status != SyncStatus.ServerDeleted)
			{
				list.AddRange(value.Attachments);
			}
		}
		if (list.Count != 0)
		{
			await ExportAttachmentsToFolder(list, finalDirName);
		}
	}

	protected async Task ExportAttachmentsToFolder(List<CellAttachment> exportAttachmentsList, string finalDirName)
	{
		if (exportAttachmentsList.Count == 0)
		{
			return;
		}
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		progressRuntimeData.NextStep("准备开始导出附件...");
		List<string> exportFailedAttachmentList = new List<string>();
		progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			string defaultFileName = string.Empty;
			for (int i = 0; i < exportAttachmentsList.Count; i++)
			{
				try
				{
					CellAttachment cellAttachment = exportAttachmentsList[i];
					Guid fileId = cellAttachment.FileId;
					defaultFileName = ((cellAttachment.Name == null) ? string.Empty : cellAttachment.Name);
					progressRuntimeData.UpdateMessage("正在导出附件: " + defaultFileName);
					progressRuntimeData.UpdateProgress(i + 1, exportAttachmentsList.Count);
					await Table.Project.FileCacheManager.DownloadIfNotExist(fileId);
					char[] invalidPathChars = Path.GetInvalidPathChars();
					foreach (char oldChar in invalidPathChars)
					{
						defaultFileName = defaultFileName.Replace(oldChar, '-');
					}
					string extension = Path.GetExtension(defaultFileName);
					defaultFileName = Path.GetFileNameWithoutExtension(defaultFileName);
					string text2 = Path.Combine(finalDirName, defaultFileName) + extension;
					int num = 0;
					while (File.Exists(text2))
					{
						num++;
						text2 = $"{Path.Combine(finalDirName, defaultFileName)}({num}){extension}";
						if (num == int.MaxValue)
						{
							break;
						}
					}
					Table.Project.FileCacheManager.DuplicateTo(fileId, text2);
				}
				catch (Exception exception)
				{
					exception.Log();
					if (!string.IsNullOrEmpty(defaultFileName))
					{
						exportFailedAttachmentList.Add(defaultFileName);
					}
				}
			}
		});
		if (exportFailedAttachmentList.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出成功！");
		}
		else
		{
			string text = string.Join("，", exportFailedAttachmentList);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "以下附件导出失败: \r\n" + text);
		}
		await Task.Delay(1);
	}

	public async Task AddAttachment()
	{
		Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
		if (!cell.IsEditable || cell.HasFormula || cell.Column.HasFormula)
		{
			return;
		}
		OpenFileDialog ofd = new OpenFileDialog();
		if (ofd.ShowDialog() != DialogResult.OK)
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
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "操作失败: " + ex.Message);
				return;
			}
			Table.CellPropManager.AddAttachment(cell, fileId, ofd.SafeFileName);
			cell.UpdateValue(string.Empty);
			Invalidate();
			ShowCommentTooltip();
		}
	}

	public void PasteDistinct(Auditai.Model.Column srcCol, Auditai.Model.Column dstCol)
	{
		try
		{
			if (!dstCol.HasFormula)
			{
				dstCol.UpdateFormula($"Distinct([2:{srcCol.Table.Id}:{srcCol.Id}])");
			}
			else
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(dstCol.Formula);
					dstCol.UpdateFormula(formulaEvaluator.PasteDistinct(srcCol));
				}
				catch (FormulaException)
				{
					dstCol.UpdateFormula($"Distinct([2:{srcCol.Table.Id}:{srcCol.Id}])");
				}
			}
			PopulateTable();
		}
		catch (FormulaNotApplicableException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (Exception ex3)
		{
			ex3.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
		}
	}

	public void PasteFilter(Auditai.Model.Column srcCol, Auditai.Model.Column dstCol)
	{
		if (string.IsNullOrEmpty(srcCol.Table.FilterInfo))
		{
			return;
		}
		FilterCollection filterCollection = new FilterCollection();
		filterCollection.Deserialize(srcCol.Table.FilterInfo);
		StringBuilder stringBuilder = new StringBuilder();
		if (filterCollection.Count > 0)
		{
			string value = FilterToFormula(srcCol.Table, filterCollection.First());
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			stringBuilder.Append(value);
			if (filterCollection.Count > 1)
			{
				foreach (FilterBase item in filterCollection.Skip(1))
				{
					value = FilterToFormula(srcCol.Table, item);
					if (string.IsNullOrEmpty(value))
					{
						return;
					}
					stringBuilder.Append($" {item.Relation} {value}");
				}
			}
			stringBuilder.Append($",[2:{srcCol.Table.Id}:{srcCol.Id}]");
		}
		try
		{
			if (!dstCol.HasFormula)
			{
				dstCol.UpdateFormula($"DistinctF({stringBuilder})");
			}
			else
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(dstCol.Formula);
					dstCol.UpdateFormula(formulaEvaluator.PasteFilter(stringBuilder.ToString()));
				}
				catch (FormulaException)
				{
					dstCol.UpdateFormula($"DistinctF({stringBuilder})");
				}
			}
			PopulateTable();
		}
		catch (FormulaNotApplicableException ex2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (FormulaException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "粘贴为DistinctF公式过程中出现错误。");
		}
	}

	private string FilterToFormula(Auditai.Model.Table srcTable, FilterBase f)
	{
		Auditai.Model.Column byId = srcTable.Columns.GetById(Id64.Parse(f.ColumnId));
		string colText = $"[2:{srcTable.Id}:{byId.Id}]";
		try
		{
			return f.ToFormula(colText);
		}
		catch
		{
			return "";
		}
	}

	public void PasteAdvanced(string formTitle, string functionName, Auditai.Model.Column srcCol, Auditai.Model.Column dstCol)
	{
		try
		{
			dstCol.TryApplyFormula(rethrow: true);
		}
		catch (FormulaException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "目标列的当前列公式有错误，请修正目标列的列公式后再执行粘贴操作。");
			return;
		}
		FormAdvancedPaste formAdvancedPaste = new FormAdvancedPaste();
		formAdvancedPaste.FunctionName = functionName;
		formAdvancedPaste.Form.Text = formTitle;
		formAdvancedPaste.SrcTable = srcCol.Table;
		formAdvancedPaste.DstTable = dstCol.Table;
		formAdvancedPaste.SrcCol = srcCol;
		formAdvancedPaste.DstCol = dstCol;
		formAdvancedPaste.GuessMatchCols();
		if (formAdvancedPaste.ShowDialog() == DialogResult.OK)
		{
			try
			{
				dstCol.UpdateFormula(formAdvancedPaste.GetResultFormula());
			}
			catch (FormulaException)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列公式有错误，请修正列公式后再执行粘贴操作。");
				return;
			}
			PopulateTable();
		}
	}

	public void PasteSimpleList(Auditai.Model.Column srcCol, Auditai.Model.Column dstCol)
	{
		Auditai.Model.CellStyle style = dstCol.Table.CellStyles.MutateAndGet(dstCol.Style, delegate(Auditai.Model.CellStyle cs)
		{
			DataFormat value = cs.Format?.Clone() ?? default(DataFormat);
			value.ComboList = $"[2:{srcCol.Table.Id}:{srcCol.Id}]";
			cs.Format = value;
		});
		dstCol.UpdateStyle(style);
	}

	public void PasteTreeList(List<Auditai.Model.Column> srcCols, Auditai.Model.Column dstCol)
	{
		Auditai.Model.CellStyle style = dstCol.Table.CellStyles.MutateAndGet(dstCol.Style, delegate(Auditai.Model.CellStyle cs)
		{
			string arg = string.Join(",", srcCols.Select((Auditai.Model.Column c) => $"[2:{c.Table.Id}:{c.Id}]"));
			DataFormat value = cs.Format?.Clone() ?? default(DataFormat);
			value.ComboList = $"TreeList([2:{srcCols[0].Table.Id}:{srcCols[0].Id}],{arg})";
			cs.Format = value;
		});
		dstCol.UpdateStyle(style);
	}

	public void PasteTableList(List<Auditai.Model.Column> srcCols, Auditai.Model.Column dstCol)
	{
		Auditai.Model.CellStyle style = dstCol.Table.CellStyles.MutateAndGet(dstCol.Style, delegate(Auditai.Model.CellStyle cs)
		{
			string arg = string.Join(",", srcCols.Select((Auditai.Model.Column c) => $"[2:{c.Table.Id}:{c.Id}]"));
			DataFormat value = cs.Format?.Clone() ?? default(DataFormat);
			value.ComboList = $"TableList([2:{srcCols[0].Table.Id}:{srcCols[0].Id}],{arg})";
			cs.Format = value;
		});
		dstCol.UpdateStyle(style);
	}

	public void SetControlFormula()
	{
		if (!IsTableLocked)
		{
			ToolBar.Enabled = false;
			string controlFormula = Table.ControlFormula;
			_grid.Styles.Highlight.ForeColor = Color.Black;
			_grid.Styles.Highlight.BackColor = Color.Transparent;
			_timerFormulaHighlight.Start();
			FormulaEditor.Context.Kind = FormulaContextKind.Control;
			FormulaEditor.Context.Table = Table;
			FormulaEditor.View.Enabled = false;
			AuxEditSelectionPreserve = _grid.BodySelection;
			Program.MainForm.CurrentEdition.Ribbon.Enabled = false;
			FormControlFormula.New();
			FormControlFormula.Closed += FormControlFormula_Closed;
			_grid.Select(-1, -1);
			FormControlFormula.Result = Table.ControlFormula;
			FormControlFormula.Show();
		}
	}

	private void FormControlFormula_Closed(object sender, EventArgs e)
	{
		Program.MainForm.SuspendNavPanelDrawing();
		Program.MainForm.SuspendNavPanelVisible();
		try
		{
			ControlFormulaCloseRestoreState();
			FormControlFormula.Closed -= FormControlFormula_Closed;
			if (FormControlFormula.DialogResult == DialogResult.OK)
			{
				Table.UpdateControlFormula(FormControlFormula.Result);
				try
				{
					Table.EvalControlFormula();
				}
				catch (FormulaException)
				{
				}
			}
			Invalidate();
		}
		finally
		{
			Program.MainForm.ResumeNavPanelVisible();
			Program.MainForm.ResumeNavPanelDrawing();
		}
	}

	public void ControlFormulaCloseRestoreState()
	{
		Program.MainForm.CurrentEdition.Ribbon.Enabled = true;
		_timerFormulaHighlight.Stop();
		_penAnimateDash.DashOffset = 0f;
		_grid.Styles.Highlight.ForeColor = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Highlight\\ForeColor");
		_grid.Styles.Highlight.BackColor = Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background");
		FormulaContext formulaContext = FormulaEditor.Context.Clone();
		FormulaEvaluator.ClearCache();
		_owner.ProjectHierarchy.FindAndSelectNode(formulaContext.Table.TreeNode);
		Table = formulaContext.Table;
		PopulateTable();
		_owner.SwitchStateTo(MainFormView.Table);
		_grid.Focus();
		Select(AuxEditSelectionPreserve.r1, AuxEditSelectionPreserve.c1, AuxEditSelectionPreserve.r2, AuxEditSelectionPreserve.c2);
		FormulaEditor.RefIntervals = null;
	}

	static TableEditor()
	{
		CursorCross = new Cursor(new MemoryStream(Auditai.UI.Platform.Properties.Resources.Fill));
		CursorTable = new Cursor(new MemoryStream(Auditai.UI.Platform.Properties.Resources.table));
		CursorRowHeader = new Cursor(new MemoryStream(Auditai.UI.Platform.Properties.Resources.RowHeader));
		CursorColumnHeader = new Cursor(new MemoryStream(Auditai.UI.Platform.Properties.Resources.ColumnHeader));
		_curFormatBrush = new Cursor(new MemoryStream(Auditai.UI.Platform.Properties.Resources.cursortable));
		_penFormulaCell = new Pen(Color.Red, 1f)
		{
			Alignment = PenAlignment.Center
		};
		_penThick = new Pen(Color.Black, 2f);
		_penThin = new Pen(Color.Black, 1f);
		PenResizeDragging = new Pen(Color.Gray, 1f)
		{
			DashStyle = DashStyle.Dash
		};
		_penFormulaRefRect = new Pen(Color.Red, 1f)
		{
			Alignment = PenAlignment.Center
		};
		_penAnimateDash = new Pen(Color.Red, 1f)
		{
			DashStyle = DashStyle.Dash,
			Alignment = PenAlignment.Center
		};
		_brushFormulaRefRect = new SolidBrush(Color.Red);
		_cancelManualInputBackgroundBrush = new SolidBrush(Color.Gray);
		_timerFormulaHighlight = new Timer
		{
			Interval = 100
		};
		_timerWarningHighlight = new Timer
		{
			Interval = 500
		};
		_dateEdit = new C1TextBoxEx();
		_timeEdit = new C1TextBoxEx();
		RemindColor = Color.FromArgb(255, 106, 0);
		_dateEdit.FormatType = FormatTypeEnum.CustomFormat;
		_dateEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.ErrorInfo.ShowErrorMessage = false;
	}

	public TableEditor(MainForm owner)
	{
		_owner = owner;
		FormControlFormula = new FormControlFormula();
		Initialize(owner);
		AttachTooltip();
		AttachCrossProjectRefToolTip();
		SetTheme();
		ThemeManager.GetInstance().Register(this);
		MemberManager.GetInstance().TableCellChanged += TableEditor_TableCellChanged;
	}

	private void TableEditor_TableCellChanged(object sender, long e)
	{
		Program.MainForm.TableEditor.FlagSomeUserStayedCellChanged();
		Program.MainForm.TicketInputEditor.FlagSomeUserStayedCellChanged();
		Invalidate();
		// 单元格变化后更新撤销/恢复按钮状态
		Program.MainForm.UpdateUndoRedoButtonState();
	}

	public void SetTheme()
	{
		AuditaiTheme selectedAuditaiTheme = Theme.SelectedAuditaiTheme;
		if (selectedAuditaiTheme != null)
		{
			pnlGrid.BackColor = Color.White;
			_grid.Styles[CellStyleEnum.SelectedColumnHeader].DefinedElements &= ~StyleElementFlags.ForeColor;
			_grid.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			_grid.Styles.EmptyArea.BackColor = Color.Transparent;
			Color color = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
			_penThick.Color = color;
			_penThin.Color = color;
			_navTreeTitleBackgroundColor = Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1Command\\C1OutBar\\Page\\Title\\Hot\\Background");
			if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
			{
				MainForm.ImageProcess.SetImageStrategy(new WhiteImageStrategy());
			}
			else
			{
				MainForm.ImageProcess.SetImageStrategy(new DefaultImageStrategy());
			}
			MainForm.ImageProcess.ProcessImage();
		}
		TitleEditor.SetTheme();
		ValidationEditor.SetTheme();
		FootEditor.SetTheme();
		Theme.SetCurrentObject(ListDropDown.DropDown);
		Theme.SetCurrentObject(InputListDropDown.DropDown);
		SetNavTreeTitlePanelBackgroundBrush();
	}

	public void SetFormulaContext()
	{
		if (Table == null || _isUpdatingView || _isEditingHeaders || (FormulaEditor.IsEditing && !FormulaEditor.IsFinishingEditing) || ValidationEditor.IsEditing || AuxEditor.IsEditing || TitleEditor.AuxEditor.IsEditing || FootEditor.AuxEditor.IsEditing || LedgerCollectFormulaEditor.IsEditing || _isFormatBrushing || FormControlFormula.IsEditing)
		{
			return;
		}
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			FormulaContext.Kind = FormulaContextKind.None;
			FormulaEditor.View.Enabled = false;
		}
		else
		{
			switch (_grid.SelectionType)
			{
			case SelectionType.Column:
			case SelectionType.Table:
			{
				if (IsCellRangeHeaderCellEntireColumn(_grid.BodySelection))
				{
					FormulaContext.Kind = FormulaContextKind.HeaderCell;
					Auditai.Model.Cell cell2 = Table[_grid.BodyRow, _grid.BodyCol];
					FormulaContext.Cell = cell2;
					FormulaEditor.View.Enabled = HasSchemaPermission() && !IsTableLocked && cell2.DisplayLocked == 0L && CanEditColumn(cell2.Column) && CanEditRow(cell2.Row);
					break;
				}
				Auditai.Model.Column column = null;
				for (int i = _grid.BodyCol; i < _grid.BodyColsCount; i++)
				{
					Auditai.Model.Column column2 = Table.Columns[i];
					if (column2.Visible)
					{
						column = column2;
						break;
					}
				}
				if (column == null)
				{
					FormulaContext.Kind = FormulaContextKind.None;
					FormulaEditor.View.Enabled = false;
				}
				else
				{
					FormulaContext.Kind = FormulaContextKind.Column;
					FormulaContext.Column = column;
					FormulaEditor.View.Enabled = !IsTableLocked && HasSchemaPermission() && CanEditColumn(column);
				}
				break;
			}
			case SelectionType.Range:
			case SelectionType.Row:
				try
				{
					Auditai.Model.Cell cell;
					if (IsCellRangeHeaderCellEntireColumn(_grid.BodySelection))
					{
						FormulaContext.Kind = FormulaContextKind.HeaderCell;
						cell = Table[_grid.BodyRow, _grid.BodyCol];
					}
					else
					{
						FormulaContext.Kind = FormulaContextKind.Cell;
						cell = Table[_grid.BodyRow, _grid.BodyCol];
					}
					FormulaContext.Cell = cell;
					FormulaEditor.View.Enabled = HasSchemaPermission() && !IsTableLocked && cell.DisplayLocked == 0L && CanEditColumn(cell.Column) && CanEditRow(cell.Row);
				}
				catch (ArgumentOutOfRangeException)
				{
				}
				break;
			}
		}
		FormulaEditor.Populate();
	}

	private void NoteEditor_Entered(object sender, EventArgs e)
	{
		FormulaEditor.SetFocus();
	}

	private async void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		if (_isEditingHeaders)
		{
			KeyDown_ColHeader(e);
		}
		else
		{
			await KeyDown_Normal(e);
		}
	}

	private bool IsCellRangeOutOfTableRange(C1.Win.C1FlexGrid.CellRange range)
	{
		if (range.r1 < 0 || range.r1 >= Table.Rows.Count)
		{
			return true;
		}
		if (range.r2 < 0 || range.r2 >= Table.Rows.Count)
		{
			return true;
		}
		if (range.c1 < 0 || range.c1 >= Table.Columns.Count)
		{
			return true;
		}
		if (range.c2 < 0 || range.c2 >= Table.Columns.Count)
		{
			return true;
		}
		return false;
	}

	private bool IsCellIndexOutOfTableRange(int row, int col)
	{
		if (row < 0 || row >= Table.Rows.Count)
		{
			return true;
		}
		if (col < 0 || col >= Table.Columns.Count)
		{
			return true;
		}
		return false;
	}

	private async Task KeyDown_Normal(KeyEventArgs e)
	{
		switch (e.KeyData)
		{
		case Keys.V | Keys.Control:
			await PasteValue();
			e.SuppressKeyPress = true;
			break;
		case Keys.Oemplus:
			e.SuppressKeyPress = true;
			EnterFormula();
			break;
		case Keys.Z | Keys.Control:
			Undo();
			break;
		case Keys.Y | Keys.Control:
			Redo();
			break;
		case Keys.Return:
			e.SuppressKeyPress = true;
			if (_grid.Row == _grid.Rows.Count - 1 && UserSet.Config.AutoRowAdd)
			{
				AppendRow();
			}
			if (_grid.Row < _grid.Rows.Count - 1)
			{
				_grid.Row++;
			}
			break;
		case Keys.Back:
		case Keys.Delete:
		{
			if (IsTableLocked)
			{
				break;
			}
			C1.Win.C1FlexGrid.CellRange sel = _grid.BodySelection;
			if (IsCellRangeOutOfTableRange(sel))
			{
				break;
			}
			if (sel.IsSingleCell)
			{
				Auditai.Model.Cell cell = Table[sel.TopRow, sel.LeftCol];
				if (CanEditCell(cell, IsTableExistFillFormula()))
				{
					CellUpdateValueCommand command = new CellUpdateValueCommand(cell, string.Empty)
					{
						IsExistManualInputValue = cell.IsAllowManualInputOnFormula
					};
					Table.CommandsManager.ExecuteCommand(command);
					_grid.Invalidate();
				}
				break;
			}
			bool flag = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
			bool flag2 = IsExcludeFixedRowAndSumRowCell();
			_grid.BeginUpdate();
			List<Tuple<Auditai.Model.Cell, object>> list5 = new List<Tuple<Auditai.Model.Cell, object>>();
			bool isTableExistFillFormula = IsTableExistFillFormula();
			foreach (Auditai.Model.Cell item in Table.EnumerateCellRange(sel.TopRow, sel.LeftCol, sel.BottomRow, sel.RightCol))
			{
				if (flag2)
				{
					Auditai.Model.Row row = item.Row;
					if (row.Role == RowRole.Total || row.Role == RowRole.Header || row.Role == RowRole.Fixed)
					{
						continue;
					}
				}
				if (CanEditCell(item, isTableExistFillFormula) && _grid.BodyGetRow(item.Row.Index).Visible && _grid.BodyGetCol(item.Column.Index).Visible && (!item.HasCellFormulaOrColumnFormula || flag))
				{
					list5.Add(Tuple.Create(item, (object)string.Empty));
				}
			}
			BatchCellUpdateValueCommand command2 = new BatchCellUpdateValueCommand(Table, list5)
			{
				IsExistManualInputValue = true
			};
			Table.CommandsManager.ExecuteCommand(command2);
			_grid.EndUpdate();
			break;
		}
		case Keys.X | Keys.Control:
			Cut();
			break;
		case Keys.C | Keys.Control:
			Copy();
			break;
		case Keys.A | Keys.Control:
			_grid.BodySelect(0, 0, _grid.BodyRowsCount - 1, _grid.BodyColsCount - 1);
			break;
		case Keys.F | Keys.Control:
			Find();
			break;
		case Keys.H | Keys.Control:
			Replace();
			break;
		case Keys.Space:
			KeyDown_Space();
			break;
		case Keys.Down | Keys.Alt:
		{
			if (IsCellIndexOutOfTableRange(_grid.BodyRow, _grid.BodyCol))
			{
				break;
			}
			Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
			if (cell == null) return;
			if (!cell.DisplayFormat.HasComboList)
			{
				break;
			}
			Auditai.Model.Operand list3 = GetList(cell.DisplayFormat.ComboList);
			if (!(list3 is ValueSetOperand valueSetOperand2))
			{
				break;
			}
			e.Handled = true;
			List<string> list4 = valueSetOperand2.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> tup) => tup.Item2.ToString()).ToList();
			int num2 = list4.IndexOf(cell.Value?.ToString() ?? string.Empty);
			if (num2 < list4.Count - 1)
			{
				if (num2 > -1)
				{
					cell.UpdateValue(list4[num2 + 1]);
					Invalidate();
				}
				else if (list4.Count > 0)
				{
					cell.UpdateValue(list4[0]);
					Invalidate();
				}
			}
			break;
		}
		case Keys.Up | Keys.Alt:
		{
			if (IsCellIndexOutOfTableRange(_grid.BodyRow, _grid.BodyCol))
			{
				break;
			}
			Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
			if (cell == null) return;
			if (!cell.DisplayFormat.HasComboList)
			{
				break;
			}
			Auditai.Model.Operand list = GetList(cell.DisplayFormat.ComboList);
			if (list is ValueSetOperand valueSetOperand)
			{
				e.Handled = true;
				List<string> list2 = valueSetOperand.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> tup) => tup.Item2.ToString()).ToList();
				int num = list2.IndexOf(cell.Value?.ToString() ?? string.Empty);
				if (num > 0)
				{
					cell.UpdateValue(list2[num - 1]);
					Invalidate();
				}
			}
			break;
		}
		}
	}

	private void KeyDown_Space()
	{
		if (IsCellIndexOutOfTableRange(_grid.BodyRow, _grid.BodyCol))
		{
			return;
		}
		Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
		if (cell == null) return;
		if ((cell.DisplayFormat.FormatType != DataFormatType.BoolCheckBox && cell.DisplayFormat.FormatType != DataFormatType.BoolOnOff) || Table?.IsLocked == true)
		{
			return;
		}
		bool flag = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
		bool isExistManualInputValue = false;
		object item = !true.Equals(cell.Value);
		List<Tuple<Auditai.Model.Cell, object>> list = new List<Tuple<Auditai.Model.Cell, object>>();
		bool isTableExistFillFormula = IsTableExistFillFormula();
		for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
		{
			if (!_grid.BodyGetRow(i).Visible)
			{
				continue;
			}
			for (int j = _grid.BodyCol; j <= _grid.BodyColSel; j++)
			{
				Auditai.Model.Cell cell2 = Table[i, j];
				if (!CanEditCell(cell2, isTableExistFillFormula) || (cell2.DisplayFormat.FormatType != DataFormatType.BoolCheckBox && cell2.DisplayFormat.FormatType != DataFormatType.BoolOnOff))
				{
					continue;
				}
				if (cell2.HasCellFormulaOrColumnFormula)
				{
					if (!flag)
					{
						continue;
					}
					isExistManualInputValue = true;
				}
				list.Add(Tuple.Create(cell2, item));
			}
		}
		BatchCellUpdateValueCommand command = new BatchCellUpdateValueCommand(Table, list)
		{
			IsExistManualInputValue = isExistManualInputValue
		};
		Table.CommandsManager.ExecuteCommand(command);
		_grid.Invalidate();
	}

	private void KeyDown_ColHeader(KeyEventArgs e)
	{
		switch (e.KeyData)
		{
		case Keys.X | Keys.Control:
			_grid.Cut();
			break;
		case Keys.C | Keys.Control:
			_grid.Copy();
			break;
		case Keys.V | Keys.Control:
			try
			{
				_grid.Paste();
				break;
			}
			catch
			{
				break;
			}
		}
	}

	private void _grid_BodyAfterEdit(object sender, RowColEventArgs e)
	{
		if (_isEditingHeaders || IsCellIndexOutOfTableRange(e.Row, e.Col))
		{
			return;
		}
		_grid.BeginUpdate();
		object obj = _grid.BodyGetData(e.Row, e.Col);
		Auditai.Model.Cell cell = Table[e.Row, e.Col];
		if (cell == null) return;
		CellUpdateValueCommand cellUpdateValueCommand = new CellUpdateValueCommand(cell, obj);
		if (cell.HasCellFormulaOrColumnFormula)
		{
			bool isExistManualInputValue = true;
			if (!cell.IsExistManualInputValue && cell.GetDisplayValue() == Auditai.Model.Cell.GetDisplayValueImpl(obj, cell.DisplayFormat))
			{
				isExistManualInputValue = false;
			}
			cellUpdateValueCommand.IsExistManualInputValue = isExistManualInputValue;
		}
		Table.CommandsManager.ExecuteCommand(cellUpdateValueCommand);
		FormulaEvaluator.ClearCache();
		_grid.EndUpdate();
	}

	private async void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		await MouseDoubleClick_Normal(e);
		MouseDoubleClick_EditingFormula(e);
		MouseDoubleClick_EditingValidation(e);
	}

	private async Task MouseDoubleClick_Normal(MouseEventArgs e)
	{
		if (FormulaEditor.IsEditing || ValidationEditor.IsEditing || AuxEditor.IsEditing || TitleEditor.AuxEditor.IsEditing || FootEditor.AuxEditor.IsEditing || LedgerCollectFormulaEditor.IsEditing || _isFormatBrushing || IsTableLocked)
		{
			return;
		}
		try
		{
			HitTestInfo hitTestInfo = _grid.HitTest();
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				if (SoftwareLicenseManager.IsAllowModifyTableStruct() && hitTestInfo.Column >= _grid.Cols.Fixed)
				{
					BeginEditColHeaders(hitTestInfo.Column - _grid.Cols.Fixed);
					_grid.Cursor = CursorTable;
				}
				break;
			case HitTestTypeEnum.RowResize:
				if (_isEditingHeaders || hitTestInfo.Row < _grid.Rows.Fixed || _grid.BodyRowSel - _grid.BodyRow > 1000)
				{
					break;
				}
				if (_grid.Row <= _resizingRow && _resizingRow <= _grid.RowSel)
				{
					_grid.BeginUpdate();
					_grid.AutoSizeRows(_grid.Row, 0, _grid.RowSel, _grid.Cols.Count - 1, 0, AutoSizeFlags.None);
					PopulateMerges();
					_grid.EndUpdate();
					for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
					{
						int heightDisplay = _grid.BodyGetRow(i).HeightDisplay;
						if (heightDisplay > 0)
						{
							Table.Rows[i].UpdateHeight(heightDisplay);
						}
					}
				}
				else
				{
					_grid.AutoSizeRow(_resizingRow);
					Table.Rows[_resizingRow - _grid.Rows.Fixed].UpdateHeight(_grid.Rows[_resizingRow].HeightDisplay);
					PopulateMerges();
				}
				DoLayout();
				break;
			case HitTestTypeEnum.ColumnResize:
				if (_isEditingHeaders || hitTestInfo.Column < _grid.Cols.Fixed || _grid.Rows.Count - _grid.Rows.Fixed > 1000)
				{
					break;
				}
				if (_grid.Col <= _resizingColumn && _resizingColumn <= _grid.ColSel)
				{
					_grid.BeginUpdate();
					_grid.AutoSizeCols(0, _grid.Col, _grid.Rows.Count - 1, _grid.ColSel, 0, AutoSizeFlags.None);
					PopulateMerges();
					_grid.EndUpdate();
					for (int j = _grid.BodyCol; j <= _grid.BodyColSel; j++)
					{
						Table.Columns[j].UpdateWidth(_grid.BodyGetCol(j).WidthDisplay);
					}
				}
				else
				{
					_grid.AutoSizeCol(_resizingColumn);
					Table.Columns[_resizingColumn - _grid.Cols.Fixed].UpdateWidth(_grid.Cols[_resizingColumn].WidthDisplay);
					PopulateMerges();
				}
				DoLayout();
				break;
			case HitTestTypeEnum.Cell:
			{
				Auditai.Model.CellAttachments attachments;
				if (Table.Rows[hitTestInfo.Row - _grid.Rows.Fixed].Role == RowRole.Header)
				{
					if (!_isSelectingHeaderCell)
					{
						_skipBeforeSelChange = true;
						Select(hitTestInfo.Row - _grid.Rows.Fixed, hitTestInfo.Column - _grid.Cols.Fixed);
						_grid.Cursor = CursorTable;
						_isSelectingHeaderCell = true;
						_skipBeforeSelChange = false;
					}
				}
				else if (Table.CellPropManager.TryGetAttachments(Table[hitTestInfo.Row - _grid.Rows.Fixed, hitTestInfo.Column - _grid.Cols.Fixed], out attachments))
				{
					await OpenAttachment(attachments.Attachments[0]);
				}
				break;
			}
			case HitTestTypeEnum.ColumnFreeze:
			case HitTestTypeEnum.RowHeader:
				break;
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void MouseDoubleClick_EditingFormula(MouseEventArgs e)
	{
		if (!FormulaEditor.IsEditing)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest();
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.Cell && Table.Rows[hitTestInfo.Row - _grid.Rows.Fixed].Role == RowRole.Header)
		{
			if (!_isSelectingHeaderCell)
			{
				_skipBeforeSelChange = true;
				Select(hitTestInfo.Row - _grid.Rows.Fixed, hitTestInfo.Column - _grid.Cols.Fixed);
				_isSelectingHeaderCell = true;
				_skipBeforeSelChange = false;
			}
			FormulaEditor.RemoveRefAtPos();
			StringBuilder stringBuilder = new StringBuilder();
			if (Table != FormulaContext.Table)
			{
				stringBuilder.Append("{" + Table.GetCanonicalName() + "}");
			}
			Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
			Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
			stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}]");
			FormulaEditor.InsertRefText(stringBuilder.ToString());
			_hasDoubleClicked_EditingFormula = true;
		}
	}

	private void MouseDoubleClick_EditingValidation(MouseEventArgs e)
	{
		if (!ValidationEditor.IsEditing)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest();
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.Cell && Table.Rows[hitTestInfo.Row - _grid.Rows.Fixed].Role == RowRole.Header)
		{
			if (!_isSelectingHeaderCell)
			{
				_skipBeforeSelChange = true;
				Select(hitTestInfo.Row - _grid.Rows.Fixed, hitTestInfo.Column - _grid.Cols.Fixed);
				_isSelectingHeaderCell = true;
				_skipBeforeSelChange = false;
			}
			ValidationEditor.RemoveRefAtPos();
			StringBuilder stringBuilder = new StringBuilder();
			if (Table != FormulaContext.Table)
			{
				stringBuilder.Append("{" + Table.GetCanonicalName() + "}");
			}
			Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
			Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
			stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}]");
			ValidationEditor.InsertRefTextAndFocus(stringBuilder.ToString());
			_hasDoubleClicked_EditingValidation = true;
		}
	}

	private void _grid_Enter(object sender, EventArgs e)
	{
		if (TitleEditor.IsEditing)
		{
			TitleEditor.LeaveEdit();
		}
		else if (FootEditor.IsEditing)
		{
			FootEditor.LeaveEdit();
		}
		if (Table != null)
		{
			switch ((Table.BorderStyle ?? TableBorderStyles.Grid).InternalNumber)
			{
			case 0:
				AppCommands.TableStyle0.Select();
				break;
			case 1:
				AppCommands.TableStyle1.Select();
				break;
			case 2:
				AppCommands.TableStyle2.Select();
				break;
			case 3:
				AppCommands.TableStyle3.Select();
				break;
			case 4:
				AppCommands.TableStyleNoLine.Select();
				break;
			}
		}
		SetFormulaContext();
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
			OwnerDrawCell_Number(e);
			OwnerDrawCell_Shield(e);
			OwnerDrawCell_ColHeaderFormula(e);
			OwnerDrawCell_ColHeader(e);
			OwnerDrawCell_Border(e);
			OwnerDrawCell_Warning(e);
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void _grid_MouseUp(object sender, MouseEventArgs e)
	{
		if (_isRowResizingStartedDragging)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[_resizingRow];
			int num = e.Location.Y - (row.Top + _grid.ScrollPosition.Y);
			if (num < 0)
			{
				num = 0;
			}
			if (_grid.Selection.TopRow <= _resizingRow && _resizingRow <= _grid.Selection.BottomRow)
			{
				try
				{
					for (int i = _grid.BodySelection.TopRow; i <= _grid.BodySelection.BottomRow; i++)
					{
						Table.Rows[i].UpdateHeight(num);
					}
				}
				catch (ArgumentOutOfRangeException ex)
				{
					ex.Log();
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, ex.ToString());
				}
			}
			else
			{
				Table.Rows[_resizingRow - _grid.Rows.Fixed].UpdateHeight(num);
			}
			PopulateRowsHeight();
			DoLayout();
			_isRowResizingStartedDragging = false;
			_grid.Invalidate();
		}
		_isRowResizingMouseDown = false;
		if (_isColumnResizingStartedDragging)
		{
			C1.Win.C1FlexGrid.Column column = _grid.Cols[_resizingColumn];
			int num2 = e.Location.X - (column.Left + _grid.ScrollPosition.X);
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (_grid.Selection.LeftCol <= _resizingColumn && _resizingColumn <= _grid.Selection.RightCol)
			{
				try
				{
					for (int j = _grid.BodySelection.LeftCol; j <= _grid.BodySelection.RightCol; j++)
					{
						Table.Columns[j].UpdateWidth(num2);
					}
				}
				catch (ArgumentOutOfRangeException ex2)
				{
					ex2.Log();
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, ex2.ToString());
				}
			}
			else
			{
				Table.Columns[_resizingColumn - _grid.Cols.Fixed].UpdateWidth(num2);
			}
			Point scrollPosition = _grid.ScrollPosition;
			pnlGrid.SuspendDrawing();
			PopulateColumns();
			DoLayout();
			_grid.ScrollPosition = scrollPosition;
			_isColumnResizingStartedDragging = false;
			_grid.Invalidate();
			pnlGrid.ResumeDrawing();
		}
		_isColumnResizingMouseDown = false;
		if (_isFormatBrushing)
		{
			MouseUp_FormatBrush(e);
		}
		else if (_isFilling)
		{
			MouseUp_Fill(e);
		}
		else if (FormulaEditor.IsEditing)
		{
			MouseUp_Formula(e);
		}
		else if (ValidationEditor.IsEditing)
		{
			MouseUp_Validation(e);
		}
		else if (AuxEditor.IsEditing || TitleEditor.AuxEditor.IsEditing || FootEditor.AuxEditor.IsEditing)
		{
			MouseUp_AuxEdit(e);
		}
		else if (FormControlFormula.IsEditing)
		{
			MouseUp_ControlFormula(e);
		}
		else if (LedgerCollectFormulaEditor.IsEditing)
		{
			MouseUp_LedgerCollectFormulaEdit(e);
		}
	}

	private void MouseUp_FormatBrush(MouseEventArgs e)
	{
		PasteFormat();
	}

	private void MouseUp_Fill(MouseEventArgs e)
	{
		try
		{
			Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
			if (cell.HasFormula)
			{
				SetClipboard();
				PasteFormula();
			}
			else if (_isSingleCellFill)
			{
				CopyFill();
			}
			else
			{
				SequenceFill();
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		finally
		{
			_isFilling = false;
		}
	}

	private void MouseUp_Formula(MouseEventArgs e)
	{
		if (!_hasDoubleClicked_EditingFormula)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Table != FormulaContext.Table)
			{
				stringBuilder.Append("{" + Table.GetCanonicalName() + "}");
			}
			if (FormulaEditor.IsInsideINDEX())
			{
				FormulaEditor.InsertRefText(stringBuilder.ToString());
				return;
			}
			FormulaEditor.RemoveRefAtPos();
			HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
			try
			{
				switch (hitTestInfo.Type)
				{
				case HitTestTypeEnum.ColumnHeader:
					if (hitTestInfo.Column == 0)
					{
						return;
					}
					switch (FormulaEditor.Context.Kind)
					{
					case FormulaContextKind.Cell:
					case FormulaContextKind.Column:
					case FormulaContextKind.Title:
					case FormulaContextKind.Foot:
					case FormulaContextKind.Document:
					case FormulaContextKind.Validation:
					case FormulaContextKind.HeaderCell:
						if (FormulaEditor.HasStar)
						{
							stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + ",*]");
						}
						else
						{
							stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + "]");
						}
						break;
					case FormulaContextKind.ColHeader:
						_grid.Select(0, hitTestInfo.Column, _grid.Rows.Fixed - 1, hitTestInfo.Column);
						stringBuilder.Insert(0, "ColName(");
						stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + "])");
						break;
					}
					break;
				case HitTestTypeEnum.Cell:
				{
					Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
					Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
					if (row.Role == RowRole.Header)
					{
						if (FormulaEditor.HasStar)
						{
							stringBuilder.Append("[" + Table[row.Index, column.Index].GetUniqueFormulaName() + ",*]");
						}
						else
						{
							stringBuilder.Append("[" + Table[row.Index, column.Index].GetUniqueFormulaName() + "]");
						}
						break;
					}
					if (_grid.Selection.IsSingleCell || _grid.MergedRanges.Contains(_grid.Selection))
					{
						stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}]");
						break;
					}
					Auditai.Model.Column column2 = Table.Columns[_grid.BodyColSel];
					Auditai.Model.Row row2 = Table.Rows[_grid.BodyRowSel];
					stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}:{column2.GetUniqueFormulaName()},{row2.Index + 1}]");
					break;
				}
				}
				FormulaEditor.InsertRefText(stringBuilder.ToString());
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
		_hasDoubleClicked_EditingFormula = false;
	}

	private void MouseUp_Validation(MouseEventArgs e)
	{
		if (!_hasDoubleClicked_EditingValidation)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{" + Table.GetCanonicalName() + "}");
			ValidationEditor.RemoveRefAtPos();
			HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.Cell:
			{
				Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
				Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
				if (row.Role == RowRole.Header)
				{
					if (ValidationEditor.HasStar)
					{
						stringBuilder.Append("[" + Table[row.Index, column.Index].GetUniqueFormulaName() + ",*]");
					}
					else
					{
						stringBuilder.Append("[" + Table[row.Index, column.Index].GetUniqueFormulaName() + "]");
					}
					break;
				}
				if (_grid.Selection.IsSingleCell)
				{
					stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}]");
					break;
				}
				Auditai.Model.Column column2 = Table.Columns[_grid.BodyColSel];
				Auditai.Model.Row row2 = Table.Rows[_grid.BodyRowSel];
				stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}:{column2.GetUniqueFormulaName()},{row2.Index + 1}]");
				break;
			}
			case HitTestTypeEnum.ColumnHeader:
				if (hitTestInfo.Column == 0)
				{
					return;
				}
				if (ValidationEditor.HasStar)
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + ",*]");
				}
				else
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + "]");
				}
				break;
			}
			ValidationEditor.InsertRefTextAndFocus(stringBuilder.ToString());
		}
		_hasDoubleClicked_EditingValidation = false;
	}

	private void MouseUp_AuxEdit(MouseEventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Table != FormulaContext.Table)
		{
			stringBuilder.Append("{" + Table.GetCanonicalName() + "}");
		}
		AuxEditor auxEditor = null;
		if (AuxEditor.IsEditing)
		{
			auxEditor = AuxEditor;
		}
		else if (TitleEditor.AuxEditor.IsEditing)
		{
			auxEditor = TitleEditor.AuxEditor;
		}
		else if (FootEditor.AuxEditor.IsEditing)
		{
			auxEditor = FootEditor.AuxEditor;
		}
		auxEditor.RemoveRefAtPos();
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		try
		{
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.Cell:
			{
				Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
				Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
				if (row.Role == RowRole.Header)
				{
					stringBuilder.Append("[" + Table[row.Index, column.Index].GetUniqueFormulaName() + "]");
					break;
				}
				if (_grid.Selection.IsSingleCell)
				{
					stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}]");
					break;
				}
				Auditai.Model.Column column2 = Table.Columns[_grid.BodyColSel];
				Auditai.Model.Row row2 = Table.Rows[_grid.BodyRowSel];
				stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}:{column2.GetUniqueFormulaName()},{row2.Index + 1}]");
				break;
			}
			case HitTestTypeEnum.ColumnHeader:
				if (hitTestInfo.Column == 0)
				{
					return;
				}
				if (auxEditor.UseWildcard())
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + ",*]");
				}
				else
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + "]");
				}
				break;
			}
			auxEditor.InsertRefTextAndFocus(stringBuilder.ToString());
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void MouseUp_ControlFormula(MouseEventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Table != FormulaContext.Table)
		{
			stringBuilder.Append("{" + Table.GetCanonicalName() + "}");
		}
		FormControlFormula.RemoveRefAtPos();
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		try
		{
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.Cell:
			{
				Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
				Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
				if (row.Role == RowRole.Header)
				{
					stringBuilder.Append("[" + Table[row.Index, column.Index].GetUniqueFormulaName() + "]");
					break;
				}
				if (_grid.Selection.IsSingleCell)
				{
					stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}]");
					break;
				}
				Auditai.Model.Column column2 = Table.Columns[_grid.BodyColSel];
				Auditai.Model.Row row2 = Table.Rows[_grid.BodyRowSel];
				stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}:{column2.GetUniqueFormulaName()},{row2.Index + 1}]");
				break;
			}
			case HitTestTypeEnum.ColumnHeader:
				if (hitTestInfo.Column == 0)
				{
					return;
				}
				if (FormControlFormula.UseWildcard())
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + ",*]");
				}
				else
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + "]");
				}
				break;
			}
			FormControlFormula.InsertRefTextAndFocus(stringBuilder.ToString());
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void MouseUp_LedgerCollectFormulaEdit(MouseEventArgs e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Table != FormulaContext.Table)
		{
			stringBuilder.Append("{" + Table.GetCanonicalName() + "}");
		}
		LedgerCollectFormulaEditor.RemoveRefAtPos();
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		try
		{
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.Cell:
			{
				Auditai.Model.Column column = Table.Columns[_grid.BodyCol];
				Auditai.Model.Row row = Table.Rows[_grid.BodyRow];
				if (row.Role == RowRole.Header)
				{
					stringBuilder.Append("[" + Table[row.Index, column.Index].GetUniqueFormulaName() + "]");
					break;
				}
				if (_grid.Selection.IsSingleCell)
				{
					stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}]");
					break;
				}
				Auditai.Model.Column column2 = Table.Columns[_grid.BodyColSel];
				Auditai.Model.Row row2 = Table.Rows[_grid.BodyRowSel];
				stringBuilder.Append($"[{column.GetUniqueFormulaName()},{row.Index + 1}:{column2.GetUniqueFormulaName()},{row2.Index + 1}]");
				break;
			}
			case HitTestTypeEnum.ColumnHeader:
				if (hitTestInfo.Column == 0)
				{
					return;
				}
				if (LedgerCollectFormulaEditor.UseWildcard())
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + ",*]");
				}
				else
				{
					stringBuilder.Append("[" + Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed].GetUniqueFormulaName() + "]");
				}
				break;
			}
			LedgerCollectFormulaEditor.InsertRefTextAndFocus(stringBuilder.ToString());
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log("编辑账套采集公式时发生了未预期的异常");
		}
	}

	private void _grid_BeforeEdit(object sender, RowColEventArgs e)
	{
		if (_isEditingHeaders && e.Col >= _grid.Cols.Fixed)
		{
			Auditai.Model.Column column = Table.Columns[e.Col - _grid.Cols.Fixed];
			if (!string.IsNullOrEmpty(column.CaptionFormula))
			{
				e.Cancel = true;
			}
		}
	}

	private void _grid_AfterEdit(object sender, RowColEventArgs e)
	{
		if (_isEditingHeaders)
		{
			string caption = _grid[e.Row, e.Col]?.ToString() ?? "";
			Table.Columns[e.Col - _grid.Cols.Fixed].UpdateCaption(caption);
			ValidationEditor.PopulateGrid();
		}
	}

	private void _grid_AfterSelChange(object sender, RangeEventArgs e)
	{
		AfterSelChange_ColHeaderFormula();
	}

	private void AfterSelChange_ColHeaderFormula()
	{
		if (_isEditingHeaders)
		{
			FormulaEditor.Context.Kind = FormulaContextKind.ColHeader;
			try
			{
				Auditai.Model.Column column = Table.Columns[_grid.Col - _grid.Cols.Fixed];
				FormulaEditor.Context.Column = column;
				FormulaEditor.View.Enabled = CanEditColumn(Table.Columns[_grid.Col - _grid.Cols.Fixed]);
				FormulaEditor.Populate();
				AppCommands.TableFont.FontSelector.SelectFontFamily(column.CaptionStyle.FontFamily);
				AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(column.CaptionStyle.FontSize.Value);
				AppCommands.Bold.IsPressed = column.CaptionStyle.Bold.Value;
				AppCommands.Italic.IsPressed = column.CaptionStyle.Italic.Value;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
	}

	private void _grid_BeforeSelChange(object sender, RangeEventArgs e)
	{
		if (_isStartingMouseDownFill)
		{
			BeforeSelChange_StartingMouseDownFill(e);
		}
		else if (_isEditingHeaders)
		{
			BeforeSelChange_ColHeader(e);
		}
		else
		{
			BeforeSelChange_HeaderCell(e);
		}
	}

	private void BeforeSelChange_ColHeader(RangeEventArgs e)
	{
		if (e.NewRange.BottomRow >= 1)
		{
			e.Cancel = true;
		}
	}

	private void BeforeSelChange_StartingMouseDownFill(RangeEventArgs e)
	{
		e.Cancel = true;
	}

	private void BeforeSelChange_HeaderCell(RangeEventArgs e)
	{
		if (_skipBeforeSelChange || Table == null || Table.Rows.Count == 0 || !e.NewRange.IsValid || _isUpdatingView)
		{
			return;
		}
		if (_isSelectingHeaderCell && e.OldRange.r1 == e.NewRange.r1)
		{
			if (e.NewRange.r2 != e.NewRange.r1)
			{
				e.Cancel = true;
			}
			return;
		}
		_isSelectingHeaderCell = false;
		C1.Win.C1FlexGrid.CellRange cellRange = _grid.ToBodyRange(e.NewRange);
		if (cellRange.TopRow == 0 && cellRange.BottomRow == Table.Rows.Count - 1)
		{
			e.Cancel = false;
			return;
		}
		if (cellRange.LeftCol == 0 && cellRange.RightCol == Table.Columns.Count - 1)
		{
			e.Cancel = false;
			try
			{
				if (cellRange.TopRow == cellRange.BottomRow && Table.Rows[cellRange.TopRow].Role == RowRole.Header)
				{
					_isSelectingHeaderCell = true;
				}
				return;
			}
			catch (ArgumentOutOfRangeException)
			{
				return;
			}
		}
		if (Table.Rows[cellRange.r1].Role == RowRole.Header)
		{
			e.Cancel = true;
			int headerLastRow = Table[cellRange.r1, cellRange.c1].GetHeaderLastRow();
			if (headerLastRow > cellRange.r1)
			{
				_grid.BeginUpdate();
				Point scrollPosition = _grid.ScrollPosition;
				_skipBeforeSelChange = true;
				Select(cellRange.r1, cellRange.c1, headerLastRow, cellRange.c2);
				_skipBeforeSelChange = false;
				_grid.ScrollPosition = scrollPosition;
				_grid.EndUpdate();
			}
		}
	}

	private bool IsCellRangeHeaderCellEntireColumn(C1.Win.C1FlexGrid.CellRange sel)
	{
		try
		{
			if (Table.Rows[sel.TopRow].Role != RowRole.Header)
			{
				return false;
			}
			Auditai.Model.Cell cell = Table[sel.TopRow, sel.LeftCol];
			return sel.BottomRow == cell.GetHeaderLastRow();
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
	}

	private void _grid_BeforeResizeRow(object sender, RowColEventArgs e)
	{
		if (e.Row >= _grid.Rows.Fixed)
		{
			e.Cancel = true;
		}
	}

	private void _grid_BeforeResizeColumn(object sender, RowColEventArgs e)
	{
		if (e.Col >= _grid.Cols.Fixed)
		{
			e.Cancel = true;
		}
	}

	private void _grid_AfterResizeRow(object sender, RowColEventArgs e)
	{
		if (e.Row < _grid.Rows.Fixed)
		{
			Table.UpdateHeaderRowHeight(e.Row, _grid.Rows[e.Row].HeightDisplay);
			DoLayout();
		}
	}

	private void _grid_BeforeAutosizeRow(object sender, RowColEventArgs e)
	{
		e.Cancel = true;
	}

	private void _grid_BeforeAutosizeColumn(object sender, RowColEventArgs e)
	{
		e.Cancel = true;
	}

	private void BodySelectionChanged_Stats()
	{
		if (_isUpdatingView || _isEditingHeaders)
		{
			return;
		}
		try
		{
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			List<double> list = ((_grid.BodyRow >= 0 && _grid.BodyCol >= 0) ? (from c in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol)
				where _grid.Rows[_grid.Rows.Fixed + c.Row.Index].IsVisible && _grid.Cols[_grid.Cols.Fixed + c.Column.Index].IsVisible
				select c.Value).OfType<double>().ToList() : new List<double>());
			Program.MainForm.SelectionStats.Visible = UserSet.Config.SelectionStatsEnabled;
			if (list.Count > 0)
			{
				Program.MainForm.SelectionStats.Text = $"求和：{list.Sum():#,0.##############################}  计数：{list.Count}  平均值：{list.Average():#,0.##############################}";
			}
			else
			{
				Program.MainForm.SelectionStats.Text = "";
			}
			if (_grid.FilterManager.IsFiltering)
			{
				Program.MainForm.SelectionStats.Text += $"  在 {Table.Rows.Count} 条记录中筛选出 {_grid.FilterManager.ResultCount} 条";
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void BodySelectionChanged_TreeNodeCache()
	{
		if (!_isUpdatingView && !_isEditingHeaders)
		{
			if (TreeNodeStateCache.Contains(Table.Id))
			{
				TreeNodeCacheState treeNodeCacheState = TreeNodeStateCache.Get(Table.Id);
				treeNodeCacheState.Kind = TreeNodeCacheKind.Table;
				treeNodeCacheState.Selection = new Rectangle(_grid.BodyRow, _grid.BodyCol, _grid.BodyRowSel, _grid.BodyColSel);
			}
			else
			{
				TreeNodeStateCache.Set(Table.Id, new TreeNodeCacheState
				{
					Kind = TreeNodeCacheKind.Table,
					ScrollPosition = Point.Empty,
					Selection = new Rectangle(_grid.BodyRow, _grid.BodyCol, _grid.BodyRowSel, _grid.BodyColSel)
				});
			}
		}
	}

	private async Task BodySelectionChanged_SignalR()
	{
		if (_isUpdatingView || _isEditingHeaders)
		{
			return;
		}
		int bodyRow = _grid.BodyRow;
		int bodyCol = _grid.BodyCol;
		if (bodyRow < 0 || bodyCol < 0)
		{
			return;
		}
		try
		{
			Auditai.Model.Cell cell = Table[bodyRow, bodyCol];
			await SignalRClient.UpLoadTableCellId(Auditai.Model.User.Current.Id.ToString(), cell?.Id.ToString() ?? string.Empty);
		}
		catch
		{
		}
	}

	private async void _ttpComment_LinkClicked(object sender, object e)
	{
		if (!(e is NodeNumberInfo nodeNumberInfo))
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
							if (e is CellsOperand cellsOperand)
							{
								if (e is RangeOperand rangeOp)
								{
									// 追踪区域引用：跳转到目标表格并选中该区域
									Program.MainForm.SetOpenModeToTableMode(rangeOp.Table.TreeNode);
									_owner.ProjectHierarchy.FindAndSelectNode(rangeOp.Table.TreeNode);
									if (Table == rangeOp.Table)
									{
										Select(rangeOp.TopLeft.Row.Index, rangeOp.TopLeft.Column.Index);
									}
									return;
								}
								Program.MainForm.SetOpenModeToTableMode(cellsOperand.Table.TreeNode);
								Program.MainForm.ProjectHierarchy.FindAndSelectNode(cellsOperand.Table.TreeNode);
								if (Table != cellsOperand.Table)
								{
									return;
								}
								_grid.BeginUpdate();
								int bodyRowsCount = _grid.BodyRowsCount;
								for (int i = 0; i < bodyRowsCount; i++)
								{
									_grid.BodyGetRow(i).Visible = false;
								}
								foreach (int row in cellsOperand.Rows)
								{
									try
									{
										_grid.BodyGetRow(row).Visible = true;
									}
									catch (ArgumentOutOfRangeException exception)
									{
										exception.Log(string.Format("{0}, case {1}, _grid.BodyGetRow(row), row={2},Rows.Count={3},Rows.Fixed={4}", "_ttpComment_LinkClicked", "condCells", row, _grid.Rows.Count, _grid.Rows.Fixed));
									}
								}
								_grid.EndUpdate();
								_grid.FilterManager.IsFilteredExternally = true;
								Invalidate();
								BodySelectionChanged_Stats();
								return;
							}
							if (e is Tuple<Auditai.Model.Table, int, int> traceTarget)
							{
								TraceTableCell(traceTarget.Item1, traceTarget.Item2, traceTarget.Item3);
								return;
							}
							string text = (string)sender;
							if (text == "refresh")
							{
								_ttpComment.Hide();
								// 从数据源重新加载表格数据
								Program.MainForm.TableEditor.ReloadFromDb();
								return;
							}
							if (text == "tableCollect")
							{
								await _owner.TableCollectSet();
							}
							else if (text == "cellCollect")
							{
								_owner.CellCollectSet();
							}
							else if (text.StartsWith("openAttachment"))
							{
								int index = (int)e;
								Auditai.Model.Cell cell2 = Table[_grid.BodyRow, _grid.BodyCol];
								Table.CellPropManager.TryGetAttachments(cell2, out var attachments);
								CellAttachment ca = attachments.Attachments[index];
								await OpenAttachment(ca);
							}
							else if (text.StartsWith("exportAttachment"))
							{
								int index2 = (int)e;
								Auditai.Model.Cell cell3 = Table[_grid.BodyRow, _grid.BodyCol];
								Table.CellPropManager.TryGetAttachments(cell3, out var attachments2);
								CellAttachment attachment = attachments2.Attachments[index2];
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
										Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出附件失败。");
									}
								}
							}
							else if (text.StartsWith("removeAttachment"))
							{
								int i2 = (int)e;
								Auditai.Model.Cell cell4 = Table[_grid.BodyRow, _grid.BodyCol];
								Table.CellPropManager.RemoveAttachmentAt(cell4, i2);
								Invalidate();
								ShowCommentTooltip();
							}
							else if (text.StartsWith("renameAttachment"))
							{
								int num = (int)e;
								Auditai.Model.Cell cell5 = Table[_grid.BodyRow, _grid.BodyCol];
								Table.CellPropManager.TryGetAttachments(cell5, out var attachments3);
								CellAttachment cellAttachment = attachments3.Attachments[num];
								string text2 = InputForm.Text("重命名附件", "将附件‘" + cellAttachment.Name + "’重命名为：", cellAttachment.Name);
								if (!string.IsNullOrWhiteSpace(text2))
								{
									Table.CellPropManager.RenameAttachmentAt(cell5, num, text2);
								}
							}
							else if (text.StartsWith("addAttachment"))
							{
								_ = Table[_grid.BodyRow, _grid.BodyCol];
								await AddAttachment();
							}
							else if (text.StartsWith("exportAllAttachment"))
							{
								await ExportAllAttachment();
							}
							return;
						}
						Auditai.Model.Project project = await _owner.OpenOrSwitchToProject(consolidateAttributes.ProjectId);
						if (project != null)
						{
							TreeTableNode treeTableNode = project.GetAllTableNodes().FirstOrDefault((TreeTableNode t) => t.Id == consolidateAttributes.TableId);
							if (treeTableNode == null)
							{
								Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "数据源表格不存在");
							}
							else
							{
								_owner.ProjectHierarchy.FindAndSelectNode(treeTableNode);
							}
						}
					}
					else
					{
						_owner.ProjectHierarchy.FindAndSelectNode(node);
					}
				}
				else
				{
					Program.MainForm.SetOpenModeToTableMode(cell.Column.Table.TreeNode);
					_owner.ProjectHierarchy.FindAndSelectNode(cell.Column.Table.TreeNode);
					if (Table == cell.Column.Table)
					{
						Select(cell.Row.Index, cell.Column.Index);
					}
				}
			}
			else
			{
				Program.MainForm.SetOpenModeToTableMode(column.Table.TreeNode);
				_owner.ProjectHierarchy.FindAndSelectNode(column.Table.TreeNode);
				if (Table == column.Table)
				{
					SelectColumn(column.Index);
				}
			}
		}
		else
		{
			_owner.ProjectHierarchy.FindAndSelectNode(nodeNumberInfo.Node);
		}
	}

	public static async Task OpenAttachment(CellAttachment ca)
	{
		Guid fileId = ca.FileId;
		await Auditai.Model.Project.Current.FileCacheManager.DownloadIfNotExist(fileId);
		try
		{
			string fileName = Auditai.Model.Project.Current.FileCacheManager.DuplicateToTemp(fileId, ca.Name);
			ProcessStartInfo startInfo = new ProcessStartInfo(fileName)
			{
				UseShellExecute = true
			};
			Process process = new Process
			{
				StartInfo = startInfo
			};
			process.Start();
		}
		catch
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开附件失败。");
		}
	}

	private void _grid_BodySelectionChanged(object sender, EventArgs e)
	{
		if (Table != null)
		{
			BodySelectionChanged_Stats();
			BodySelectionChanged_TreeNodeCache();
			SetFormulaContext();
			BodySelectionChanged_CommentToolTip();
			SendBodySelectionChanged_SignalR();
		}
	}

	private void SendBodySelectionChanged_SignalR()
	{
		_ = BodySelectionChanged_SignalR();
	}

	private void _timerFormulaHighlight_Tick(object sender, EventArgs e)
	{
		if (!Program.MainForm.IsInSyncingProject)
		{
			_penAnimateDash.DashOffset--;
			_grid.Invalidate();
			TitleEditor.View.Invalidate();
			FootEditor.View.Invalidate();
		}
	}

	private void UpdateWarningTextColor()
	{
		if (_warningUpdateTimes % 2 == 1)
		{
			_remindTextColor = RemindColor;
			_warningTextColor = Color.Red;
			_warningTextIsShown = true;
		}
		else
		{
			_warningTextIsShown = false;
		}
	}

	private void _timerWarningHighlight_Tick(object sender, EventArgs e)
	{
		if (Table == null || (Table.ControlWarningCells.Count == 0 && Table.ControlRemindCells.Count == 0))
		{
			_warningUpdateTimes = 0;
		}
		else if (!_grid.Visible)
		{
			_warningUpdateTimes = 0;
		}
		else if (!Program.MainForm.IsInSyncingProject)
		{
			_warningUpdateTimes++;
			UpdateWarningTextColor();
			_grid.Invalidate();
			TitleEditor.View.Invalidate();
			FootEditor.View.Invalidate();
		}
	}

	private void BodySelectionChanged_CommentToolTip()
	{
		ShowCommentTooltip();
	}

	public void CellCollect(Ledger ledger)
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择采集单元格");
			return;
		}
		if (_grid.BodyRow >= _grid.BodyRowsCount || _grid.BodyCol >= _grid.BodyColsCount)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择采集单元格");
			return;
		}
		if (Table == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择采集表格");
			return;
		}
		if (!Table.TreeNode.HasReadPermission())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有查看权限，不能执行采账处理");
			return;
		}
		if (!Table.TreeNode.HasWritePermission())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前表格没有编辑权限，不能执行采账处理");
			return;
		}
		Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
		Tuple<DateTime, DateTime> auditYear = DictionarySync.GetAuditYear(Table);
		if (auditYear == null)
		{
			throw new InvalidAuditYearException("未在当前表格的标题区发现截止日或期间信息，请在标题区完善这些信息。");
		}
		frmCellCollect frmCellCollect = new frmCellCollect(ledger, Table, auditYear, View.FindForm());
		try
		{
			frmCellCollect.LoadFormula(cell.CollectSource);
			frmCellCollect.Column = _grid.BodyCol;
			frmCellCollect.Row = _grid.BodyRow;
			if (frmCellCollect.ShowDialog() == DialogResult.OK)
			{
				if (frmCellCollect.Value.HasValue)
				{
					cell.UpdateValue(frmCellCollect.Value);
				}
				cell.UpdateCollectSource(frmCellCollect.Formula);
			}
		}
		catch (InvalidAuditYearException)
		{
			cell.UpdateCollectSource(frmCellCollect.Formula);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "审计年度设置不合理，未能采集数据，请重新设置审计年度");
		}
		catch (UnExpectAuditYearException)
		{
			cell.UpdateCollectSource(frmCellCollect.Formula);
		}
		catch (Exception ex)
		{
			ex.Log("单元格采账设置确定时出现异常");
			if (!string.IsNullOrEmpty(frmCellCollect.Formula))
			{
				cell.UpdateCollectSource(frmCellCollect.Formula);
			}
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		MouseClick_ContextMenu(e);
		MouseClick_ColHeader(e);
	}

	private void MouseClick_ContextMenu(MouseEventArgs e)
	{
		if (Table == null || FormulaEditor.IsEditing || ValidationEditor.IsEditing || AuxEditor.IsEditing || LedgerCollectFormulaEditor.IsEditing || _isFormatBrushing)
		{
			return;
		}
		_grid.FilterManager.IsFilterOnGridColumnHeader = false;
		if (IsTableLocked)
		{
			if (e.Button == MouseButtons.Right && _grid.HitTest(e.Location).Type == HitTestTypeEnum.Cell)
			{
				ctxLock.ShowContextMenu(_grid, e.Location);
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.RowHeader:
				if (!_isEditingHeaders && _grid.BodyRow >= 0)
				{
					ctxRow.ShowContextMenu(_grid, e.Location);
				}
				break;
			case HitTestTypeEnum.ColumnHeader:
				if (hitTestInfo.Column >= _grid.Cols.Fixed && _grid.BodyCol >= 0)
				{
					ctxColumn.ShowContextMenu(_grid, e.Location);
					AfterCtxColumnPopUp();
				}
				else if (hitTestInfo.Column < _grid.Cols.Fixed && HasSchemaPermission())
				{
					ctxTableHeader.ShowContextMenu(_grid, e.Location);
				}
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(_grid, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				if (_isEditingHeaders)
				{
					if (hitTestInfo.Row == 0 && CanEditColumn(Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed]))
					{
						ctxColHeader.ShowContextMenu(_grid, e.Location);
					}
				}
				else if (IsCellRangeHeaderCellEntireColumn(_grid.BodySelection) && Table.Rows[hitTestInfo.Row - _grid.Rows.Fixed].Role == RowRole.Header)
				{
					ctxHeaderCell.ShowContextMenu(_grid, e.Location);
				}
				else if (_grid.Selection.IsSingleCell)
				{
					ctxCell.ShowContextMenu(_grid, e.Location);
				}
				else
				{
					ctxRange.ShowContextMenu(_grid, e.Location);
				}
				break;
			case HitTestTypeEnum.ColumnResize:
			case HitTestTypeEnum.ColumnFreeze:
				break;
			}
		}
		else
		{
		}
	}

	private bool IsGridEntireColumnSelected()
	{
		if (_grid.Selection.TopRow <= _grid.Rows.Fixed && _grid.Selection.BottomRow == _grid.Rows.Count - 1)
		{
			return true;
		}
		return false;
	}

	private void BeforeMouseDown_OnClickShowMoreMenuIcon(BeforeMouseDownEventArgs e)
	{
		if (Table == null || IsTableLocked || FormulaEditor.IsEditing || ValidationEditor.IsEditing || AuxEditor.IsEditing || LedgerCollectFormulaEditor.IsEditing || _isFormatBrushing || e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type != HitTestTypeEnum.ColumnHeader || _grid.FilterManager.IsColumnInFilting(hitTestInfo.Column) || !IsPointInColHeaderShowMoreMenuImageRect(new Point(e.X, e.Y), hitTestInfo.Column) || _isColumnResizingStartedDragging)
		{
			return;
		}
		int num = hitTestInfo.Column - _grid.Cols.Fixed;
		if (Table == null || num < 0 || num >= Table.Columns.Count)
		{
			return;
		}
		Auditai.Model.Column column = Table.Columns[num];
		if (column.Width >= GetColumnMinWidthForShowColumnHeaderIcon())
		{
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			if (hitTestInfo.Column >= selection.LeftCol && hitTestInfo.Column <= selection.RightCol && IsGridEntireColumnSelected())
			{
				e.Cancel = true;
				_grid.FilterManager.IsFilterOnGridColumnHeader = true;
				ctxColumn.ShowContextMenu(_grid, new Point(e.X, e.Y));
				AfterCtxColumnPopUp();
			}
			else
			{
				_grid.FilterManager.IsFilterOnGridColumnHeader = true;
				ctxColumn.ShowContextMenu(_grid, new Point(e.X, e.Y));
				AfterCtxColumnPopUp();
			}
		}
	}

	private void AfterCtxColumnPopUp()
	{
		C1CommandLink c1CommandLink = null;
		foreach (object commandLink in ctxColumn.CommandLinks)
		{
			if (commandLink is C1CommandLink { Visible: not false } c1CommandLink2)
			{
				c1CommandLink = c1CommandLink2;
				break;
			}
		}
		if (c1CommandLink == lnkColumnAccess)
		{
			lnkColumnAccess.Delimiter = false;
		}
		else
		{
			lnkColumnAccess.Delimiter = true;
		}
	}

	private void MouseClick_ColHeader(MouseEventArgs e)
	{
		if (_isEditingHeaders)
		{
			HitTestInfo hitTestInfo = _grid.HitTest();
			if (hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Row > 0)
			{
				EndEditColHeaders();
				Select(hitTestInfo.Row - 1, hitTestInfo.Column - _grid.Cols.Fixed);
			}
			else if (hitTestInfo.Type == HitTestTypeEnum.None && hitTestInfo.Row != 0)
			{
				EndEditColHeaders();
			}
		}
	}

	private void BeforeMouseDown_OnCellIconResponseClickEvent(BeforeMouseDownEventArgs e)
	{
		if (_isMouseOverCancelManualInputIcon && Table != null && !IsTableLocked && !FormulaEditor.IsEditing && !ValidationEditor.IsEditing && !AuxEditor.IsEditing && !LedgerCollectFormulaEditor.IsEditing && !_isFormatBrushing && e.Button == MouseButtons.Left)
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
		bool flag = false;
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		Table.BeginBatchUpdateValue();
		bool isTableExistFillFormula = IsTableExistFillFormula();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				Auditai.Model.Cell cell = Table[i, j];
				if (cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula && CanEditCell(cell, isTableExistFillFormula))
				{
					cell.IsExistManualInputValue = false;
					cell.UpdateValue(string.Empty);
					flag = true;
				}
			}
		}
		Table.EndBatchUpdateValue();
		if (flag)
		{
			CalcCurrentTable();
		}
		_grid.Invalidate();
	}

	private Rectangle GetCancelManualInputIconArea(Rectangle cellRect, out bool isIconOutOfRange)
	{
		Rectangle result = new Rectangle(cellRect.X + 2, cellRect.Y + 2, Auditai.UI.Platform.Properties.Resources.CancelManualInput.Width, Auditai.UI.Platform.Properties.Resources.CancelManualInput.Height);
		isIconOutOfRange = false;
		if (result.X - 2 + Auditai.UI.Platform.Properties.Resources.CancelManualInput.Width + 4 >= cellRect.Right || result.Y - 2 + Auditai.UI.Platform.Properties.Resources.CancelManualInput.Height + 4 >= cellRect.Bottom)
		{
			isIconOutOfRange = true;
		}
		return result;
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		try
		{
			bool isMouseOverCancelManualInputIcon = _isMouseOverCancelManualInputIcon;
			_isMouseOverCancelManualInputIcon = false;
			if (Table == null || _isFormatBrushing || _isFilling)
			{
				return;
			}
			HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
			if (_isEditingHeaders)
			{
				if (hitTestInfo.Row == 0)
				{
					_grid.Cursor = CursorTable;
				}
				return;
			}
			if (_isRowResizingMouseDown && !_isRowResizingStartedDragging && Math.Abs(e.Location.Y - _mouseDownLocation.Y) > SystemInformation.DragSize.Height)
			{
				_isRowResizingStartedDragging = true;
			}
			if (_isColumnResizingMouseDown && !_isColumnResizingStartedDragging && Math.Abs(e.Location.X - _mouseDownLocation.X) > SystemInformation.DragSize.Width)
			{
				_isColumnResizingStartedDragging = true;
			}
			if (_isRowResizingStartedDragging || _isColumnResizingStartedDragging)
			{
				_grid.Invalidate();
			}
			if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader)
			{
				MouseHeaderCol = hitTestInfo.Column;
			}
			else
			{
				MouseHeaderCol = -1;
			}
			switch (hitTestInfo.Type)
			{
			case HitTestTypeEnum.Cell:
			{
				Auditai.Model.Cell cell = Table[hitTestInfo.Row - _grid.Rows.Fixed, hitTestInfo.Column - _grid.Cols.Fixed];
				if (cell == null) return;
				bool hasComboList = cell.DisplayFormat.HasComboList;
				if (ShouldStartMouseDownFill(e) && !hasComboList)
				{
					_grid.Cursor = CursorCross;
					_isStartingMouseDownFill = true;
					break;
				}
				if (!_isStartingMouseDownFill && cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula && CanEditCell(cell, IsTableExistFillFormula()))
				{
					Rectangle cellRect = _grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
					bool isIconOutOfRange;
					Rectangle cancelManualInputIconArea = GetCancelManualInputIconArea(cellRect, out isIconOutOfRange);
					if (!isIconOutOfRange && cancelManualInputIconArea.Contains(hitTestInfo.X, hitTestInfo.Y))
					{
						_isMouseOverCancelManualInputIcon = true;
						_grid.Cursor = Cursors.Arrow;
						break;
					}
				}
				if (cell.DisplayDataType == typeof(bool) || hasComboList)
				{
					_grid.Cursor = Cursors.Arrow;
				}
				else if (cell.Row.Role == RowRole.Header)
				{
					if (_isSelectingHeaderCell)
					{
						_grid.Cursor = CursorTable;
					}
					else
					{
						_grid.Cursor = CursorColumnHeader;
					}
				}
				else
				{
					_grid.Cursor = CursorTable;
				}
				_isStartingMouseDownFill = false;
				break;
			}
			case HitTestTypeEnum.ColumnHeader:
			{
				if (hitTestInfo.Column == 0)
				{
					_grid.Cursor = CursorTable;
					break;
				}
				Auditai.Model.Column column = Table.Columns[hitTestInfo.Column - _grid.Cols.Fixed];
				if (Program.MainForm.State.ViewKind == MainFormView.Table && hitTestInfo.Column > 0 && !AuxEditor.IsEditing && !TitleEditor.AuxEditor.IsEditing && !FootEditor.AuxEditor.IsEditing && !FormControlFormula.IsEditing && !IsTableLocked && !LedgerCollectFormulaEditor.IsEditing && HasSchemaPermission())
				{
					bool flag = false;
					Cursor arrow = Cursors.Arrow;
					if (CanEditColumn(column) && !column.IsLocked)
					{
						if (IsPointInStartEditingColHeaderImageRect(e.Location, hitTestInfo.Column))
						{
							if (!_isMouseInStartEditingColHeaderImageRect)
							{
								_isMouseInStartEditingColHeaderImageRect = true;
								flag = true;
							}
						}
						else if (_isMouseInStartEditingColHeaderImageRect)
						{
							_isMouseInStartEditingColHeaderImageRect = false;
							flag = true;
						}
					}
					if (IsPointInColHeaderShowMoreMenuImageRect(e.Location, hitTestInfo.Column))
					{
						if (!_isMouseInColHeaderShowMoreMenuImageRect)
						{
							_isMouseInColHeaderShowMoreMenuImageRect = true;
							flag = true;
						}
					}
					else if (_isMouseInColHeaderShowMoreMenuImageRect)
					{
						_isMouseInColHeaderShowMoreMenuImageRect = false;
						flag = true;
					}
					arrow = ((_isMouseInStartEditingColHeaderImageRect || _isMouseInColHeaderShowMoreMenuImageRect) ? Cursors.Arrow : CursorColumnHeader);
					_grid.Cursor = arrow;
					if (flag)
					{
						_grid.Invalidate();
					}
				}
				else
				{
					_grid.Cursor = CursorColumnHeader;
				}
				break;
			}
			case HitTestTypeEnum.RowHeader:
				_grid.Cursor = CursorRowHeader;
				break;
			case HitTestTypeEnum.None:
				_grid.Cursor = Cursors.Arrow;
				break;
			}
			if (isMouseOverCancelManualInputIcon != _isMouseOverCancelManualInputIcon)
			{
				_grid.Invalidate();
			}
		}
		catch
		{
		}
	}

	private bool IsPointInStartEditingColHeaderImageRect(Point p, int col)
	{
		if (_mouseHeaderCol == col)
		{
			return GetEditColHeaderImageRectangle(col).Contains(p);
		}
		return false;
	}

	private bool IsPointInColHeaderShowMoreMenuImageRect(Point p, int col)
	{
		if (_mouseHeaderCol == col)
		{
			return GetColHeaderShowMoreMenuImageShadowRectangle(col).Contains(p);
		}
		return false;
	}

	private bool ShouldStartMouseDownFill(MouseEventArgs e)
	{
		if (FormulaEditor.IsEditing)
		{
			return false;
		}
		if (ValidationEditor.IsEditing)
		{
			return false;
		}
		if (AuxEditor.IsEditing)
		{
			return false;
		}
		if (LedgerCollectFormulaEditor.IsEditing)
		{
			return false;
		}
		Rectangle cellRect = _grid.GetCellRect(_grid.Selection.BottomRow, _grid.Selection.RightCol);
		return new Rectangle(cellRect.Right - 10, cellRect.Bottom - 10, 10, 10).Contains(e.Location);
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		FormulaEditor.LastClickedComponent = this;
		MouseDown_StartFill(e);
		MouseDown_StartResizing(e);
	}

	private void MouseDown_StartFill(MouseEventArgs e)
	{
		if (_isStartingMouseDownFill)
		{
			_isStartingMouseDownFill = false;
			if (e.Button == MouseButtons.Left)
			{
				_isSingleCellFill = _grid.Selection.IsSingleCell;
				_isFilling = true;
			}
		}
	}

	private void MouseDown_StartResizing(MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.RowResize && e.Clicks == 1 && hitTestInfo.Row >= _grid.Rows.Fixed)
		{
			_isRowResizingMouseDown = true;
			_resizingRow = hitTestInfo.Row;
			_mouseDownLocation = e.Location;
		}
		else if (hitTestInfo.Type == HitTestTypeEnum.ColumnResize && e.Clicks == 1 && hitTestInfo.Column >= _grid.Cols.Fixed)
		{
			_isColumnResizingMouseDown = true;
			_resizingColumn = hitTestInfo.Column;
			_mouseDownLocation = e.Location;
		}
	}

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		BeforeMouseDown_OnCellIconResponseClickEvent(e);
		if (!e.Cancel)
		{
			BeforeMouseDown_OnClickShowMoreMenuIcon(e);
			if (!e.Cancel)
			{
				BeforeMouseDown_BoolCheck(e);
				BeforeMouseDown_StartEditColHeader(e);
			}
		}
		void BeforeMouseDown_StartEditColHeader(BeforeMouseDownEventArgs e)
		{
			if (Program.MainForm.State.ViewKind == MainFormView.Table && !AuxEditor.IsEditing && !TitleEditor.AuxEditor.IsEditing && !FootEditor.AuxEditor.IsEditing && !FormControlFormula.IsEditing && !LedgerCollectFormulaEditor.IsEditing && e.Button == MouseButtons.Left)
			{
				Point pt = new Point(e.X, e.Y);
				HitTestInfo hitTestInfo = _grid.HitTest(pt);
				if (hitTestInfo.Row < _grid.Rows.Fixed && hitTestInfo.Column > 0)
				{
					int num = hitTestInfo.Column - _grid.Cols.Fixed;
					Auditai.Model.Column column = Table.Columns[num];
					if (hitTestInfo.Column == _mouseHeaderCol && CanEditColumn(column) && !column.IsLocked && !IsTableLocked && column.Width >= 100 && GetEditColHeaderImageRectangle(hitTestInfo.Column).Contains(pt))
					{
						e.Cancel = true;
						BeginEditColHeaders(num);
					}
				}
			}
		}
	}

	private void BeforeMouseDown_BoolCheck(BeforeMouseDownEventArgs e)
	{
		if (Table == null || IsTableLocked || FormulaEditor.IsEditing || ValidationEditor.IsEditing || AuxEditor.IsEditing || LedgerCollectFormulaEditor.IsEditing || _isFormatBrushing || _isEditingHeaders || e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
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
			Auditai.Model.Cell cell = Table[hitTestInfo.Row - _grid.Rows.Fixed, hitTestInfo.Column - _grid.Cols.Fixed];
			if (cell == null) return;
			if (cell.DisplayFormat.FormatType != DataFormatType.BoolCheckBox && cell.DisplayFormat.FormatType != DataFormatType.BoolOnOff)
			{
				return;
			}
			e.Cancel = true;
			object obj = !true.Equals(cell.Value);
			if (_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				bool isTableExistFillFormula = IsTableExistFillFormula();
				List<Tuple<Auditai.Model.Cell, object>> list = new List<Tuple<Auditai.Model.Cell, object>>();
				for (int i = _grid.BodyRow; i <= _grid.BodyRowSel; i++)
				{
					if (!_grid.BodyGetRow(i).Visible)
					{
						continue;
					}
					for (int j = _grid.BodyCol; j <= _grid.BodyColSel; j++)
					{
						Auditai.Model.Cell cell2 = Table[i, j];
						if (CanEditCell(cell2, isTableExistFillFormula) && (cell2.DisplayFormat.FormatType == DataFormatType.BoolCheckBox || cell2.DisplayFormat.FormatType == DataFormatType.BoolOnOff))
						{
							list.Add(Tuple.Create(cell2, obj));
						}
					}
				}
				BatchCellUpdateValueCommand command = new BatchCellUpdateValueCommand(Table, list)
				{
					IsExistManualInputValue = true
				};
				Table.CommandsManager.ExecuteCommand(command);
			}
			else if (CanEditCell(cell, IsTableExistFillFormula()))
			{
				CellUpdateValueCommand command2 = new CellUpdateValueCommand(cell, obj)
				{
					IsExistManualInputValue = true
				};
				Table.CommandsManager.ExecuteCommand(command2);
			}
			_grid.Invalidate();
		}
		catch
		{
		}
	}

	private void _grid_BodyAfterScroll(object sender, RangeEventArgs e)
	{
		_grid.BeginUpdate();
		if (e.OldRange.r1 < e.NewRange.r1)
		{
			for (int k = e.OldRange.r1; k < e.NewRange.r1; k++)
			{
				for (int l = e.OldRange.c1; l <= e.OldRange.c2; l++)
				{
					ClearStyle(k, l);
				}
			}
		}
		else if (e.OldRange.r2 > e.NewRange.r2)
		{
			for (int num = e.OldRange.r2; num > e.NewRange.r2; num--)
			{
				for (int m = e.OldRange.c1; m <= e.OldRange.c2; m++)
				{
					ClearStyle(num, m);
				}
			}
		}
		else if (e.OldRange.c1 < e.NewRange.c1)
		{
			for (int n = e.OldRange.r1; n <= e.OldRange.r2; n++)
			{
				for (int num2 = e.OldRange.c1; num2 < e.NewRange.c1; num2++)
				{
					ClearStyle(n, num2);
				}
			}
		}
		else if (e.OldRange.c2 > e.NewRange.c2)
		{
			for (int num3 = e.OldRange.r1; num3 <= e.OldRange.r2; num3++)
			{
				for (int num4 = e.OldRange.c2; num4 > e.NewRange.c2; num4--)
				{
					ClearStyle(num3, num4);
				}
			}
		}
		_grid.EndUpdate();
		if (Table != null)
		{
			ShowCommentTooltip();
			if (TreeNodeStateCache.Contains(Table.Id))
			{
				TreeNodeCacheState treeNodeCacheState = TreeNodeStateCache.Get(Table.Id);
				treeNodeCacheState.ScrollPosition = _grid.ScrollPosition;
				return;
			}
			TreeNodeStateCache.Set(Table.Id, new TreeNodeCacheState
			{
				Kind = TreeNodeCacheKind.Table,
				ScrollPosition = _grid.ScrollPosition,
				Selection = Rectangle.Empty
			});
		}
		void ClearStyle(int i, int j)
		{
			C1.Win.C1FlexGrid.CellRange cellRange = _grid.BodyGetCell(i, j);
			try
			{
				cellRange.Style = null;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
	}

	private void _grid_Leave(object sender, EventArgs e)
	{
		_ttpComment.Hide();
	}

	private void _grid_LostFocus(object sender, EventArgs e)
	{
		_ttpComment.Hide();
	}

	private void OwnerDrawCell_Number(OwnerDrawCellEventArgs e)
	{
		if (_isEditingHeaders)
		{
			if (e.Col == 0 && e.Row > 0)
			{
				e.Text = e.Row.ToString();
			}
		}
		else if (e.Col == 0 && e.Row >= _grid.Rows.Fixed)
		{
			e.Text = (e.Row - _grid.Rows.Fixed + 1).ToString();
			e.Style.ForeColor = (_grid.FilterManager.IsFiltering ? _filterColor : Color.Black);
		}
	}

	private Pen GetPen(Auditai.Model.LineStyle ls)
	{
		return ls switch
		{
			Auditai.Model.LineStyle.Thick => _penThick, 
			Auditai.Model.LineStyle.Thin => _penThin, 
			Auditai.Model.LineStyle.None => Pens.Transparent, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void OwnerDrawCell_Shield(OwnerDrawCellEventArgs e)
	{
		if (e.Measuring || Table == null)
		{
			return;
		}
		if (e.Col == 0)
		{
			int num = (_isEditingHeaders ? (e.Row - 1) : (e.Row - _grid.Rows.Fixed));
			if (0 <= num && num < Table.Rows.Count)
			{
				Auditai.Model.Row row = Table.Rows[num];
				if (!CanEditRow(row) || row.IsLocked)
				{
					e.Image = Auditai.UI.Platform.Properties.Resources.TableLock;
					e.Style.ImageAlign = ImageAlignEnum.LeftCenter;
					e.Style.Display = DisplayEnum.Overlay;
				}
			}
		}
		int num2 = e.Col - _grid.Cols.Fixed;
		C1.Win.C1FlexGrid.CellRange mergedRange = _grid.GetMergedRange(e.Row, e.Col);
		if (mergedRange.ContainsRow(_grid.Rows.Fixed - 1) && mergedRange.TopRow == e.Row && 0 <= num2 && num2 < Table.Columns.Count)
		{
			Auditai.Model.Column column = Table.Columns[num2];
			if (!CanEditColumn(column) || column.IsLocked)
			{
				e.Image = Auditai.UI.Platform.Properties.Resources.TableLock;
				e.Style.Display = DisplayEnum.Overlay;
			}
		}
	}

	private void OwnerDrawCell_ColHeaderFormula(OwnerDrawCellEventArgs e)
	{
		if (FormulaEditor.IsEditing && FormulaEditor.Context.Kind == FormulaContextKind.ColHeader && FormulaEditor.Context.Table == Table && e.Row < _grid.Rows.Fixed && e.Col - _grid.Cols.Fixed == FormulaEditor.Context.Column.Index)
		{
			e.DrawCell(DrawCellFlags.Border | DrawCellFlags.Content);
			_penFormulaCell.Color = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
			Rectangle bounds = e.Bounds;
			bounds = new Rectangle(bounds.Left - 1, 0, bounds.Width, bounds.Height - 1);
			e.Graphics.DrawRectangle(_penFormulaCell, bounds);
		}
	}

	private void OwnerDrawCell_ColHeader(OwnerDrawCellEventArgs e)
	{
		if (!_isEditingHeaders || e.Col < _grid.Cols.Fixed || e.Row != 0)
		{
			return;
		}
		Auditai.Model.Column column = Table.Columns[e.Col - _grid.Cols.Fixed];
		if (e.Style.Name != _grid.Styles.Highlight.Name)
		{
			if (string.IsNullOrEmpty(column.CaptionFormula))
			{
				e.Style.BackColor = column.CaptionStyle.BackColor ?? Color.Transparent;
			}
			else
			{
				e.Style.BackColor = UserSet.Config.TableStyle.FormalaColor;
			}
		}
	}

	private void OwnerDrawCell_Border(OwnerDrawCellEventArgs e)
	{
		if (Table != null && e.Col == _grid.Cols.Count - 1 && e.Row >= 0 && e.Row < _grid.Rows.Fixed)
		{
			e.Style.Border.Color = _grid.Styles.Fixed.Border.Color;
			if ((Table.BorderStyle ?? TableBorderStyles.Grid).LeftRightLine == Auditai.Model.LineStyle.None)
			{
				e.Style.Border.Direction = BorderDirEnum.Horizontal;
			}
			else
			{
				e.Style.Border.Direction = BorderDirEnum.Both;
			}
		}
	}

	private void OwnerDrawCell_Warning(OwnerDrawCellEventArgs e)
	{
		if (e.Measuring || Table == null || (Table.ControlWarningCells.Count == 0 && Table.ControlRemindCells.Count == 0) || e.Col != 0)
		{
			return;
		}
		int num = (_isEditingHeaders ? (e.Row - 1) : (e.Row - _grid.Rows.Fixed));
		if (0 > num || num >= Table.Rows.Count)
		{
			return;
		}
		Auditai.Model.Row row = Table.Rows[num];
		int count = Table.Columns.Count;
		bool flag = false;
		bool flag2 = false;
		if (Table.ControlWarningCells.Count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				Auditai.Model.Cell item = Table[row.Index, i];
				if (Table.ControlWarningCells.Contains(item))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				goto IL_0153;
			}
		}
		if (Table.ControlRemindCells.Count > 0)
		{
			for (int j = 0; j < count; j++)
			{
				Auditai.Model.Cell item2 = Table[row.Index, j];
				if (Table.ControlRemindCells.Contains(item2))
				{
					flag2 = true;
					break;
				}
			}
		}
		goto IL_0153;
		IL_0153:
		if (!flag && !flag2)
		{
			return;
		}
		System.Drawing.Image image = null;
		if (_warningTextIsShown)
		{
			if (flag)
			{
				image = Auditai.UI.Platform.Properties.Resources.Warning16;
			}
			else if (flag2)
			{
				image = Auditai.UI.Platform.Properties.Resources.Remind16;
			}
		}
		e.Image = image;
		e.Style.ImageAlign = ImageAlignEnum.CenterCenter;
		e.Style.Display = DisplayEnum.Overlay;
		if (image != null)
		{
			e.Text = string.Empty;
		}
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
			BodyOwnerDrawCell_Style(e);
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void BodyOwnerDrawCell_Style(OwnerDrawCellEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		try
		{
			Auditai.Model.Cell cell;
			if (_isEditingHeaders)
			{
				if (e.Row == 0)
				{
					e.Text = Table.Columns[e.Col].CaptionDisplay;
					return;
				}
				cell = Table[e.Row - 1, e.Col];
			}
			else
			{
				cell = Table[e.Row, e.Col];
			}
			C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
			Type displayDataType = cell.DisplayDataType;
			styleNew.DataType = ((displayDataType == typeof(bool)) ? null : displayDataType);
			DataFormat displayFormat = cell.DisplayFormat;
			bool isExistManualInputValue = cell.IsExistManualInputValue;
			Auditai.Model.CellAttachments attachments;
			if (displayFormat.FormatType == DataFormatType.BoolCheckBox)
			{
				e.Text = string.Empty;
				e.Image = (true.Equals(cell.Value) ? _grid.Glyphs[GlyphEnum.Checked] : _grid.Glyphs[GlyphEnum.Unchecked]);
				styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.DisplayAlign);
			}
			else if (displayFormat.FormatType == DataFormatType.BoolOnOff)
			{
				e.Text = string.Empty;
				e.Image = (true.Equals(cell.Value) ? Auditai.UI.Platform.Properties.Resources.On : Auditai.UI.Platform.Properties.Resources.Off);
				styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.DisplayAlign);
			}
			else if (Table.CellPropManager.TryGetAttachments(cell, out attachments))
			{
				e.Image = Auditai.UI.Platform.Properties.Resources.CellAttachment;
				e.Text = ((cell.DisplayAlign == CellTextAlign.MiddleCenter) ? "\n\n" : "") + $"({attachments.Attachments.Count}个附件)";
				styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(cell.DisplayAlign);
			}
			else
			{
				e.Text = cell.GetDisplayValue();
			}
			switch (displayFormat.FormatType)
			{
			case DataFormatType.General:
			case DataFormatType.Number:
			case DataFormatType.Percentage:
			case DataFormatType.NumDollar:
			case DataFormatType.NumRmb:
			case DataFormatType.DateSlash:
			case DataFormatType.DateDash:
			case DataFormatType.DateChinese:
			case DataFormatType.Comma:
			case DataFormatType.BoolCheckBox:
			case DataFormatType.BoolOnOff:
			case DataFormatType.DateYearMonthChinese:
			case DataFormatType.DateYearMonthDash:
			case DataFormatType.DateYearMonthSlash:
			case DataFormatType.DateYearMonthDot:
			case DataFormatType.DateDot:
				styleNew.DataMap = null;
				break;
			case DataFormatType.BoolYesNo:
			case DataFormatType.BoolRightWrong:
			case DataFormatType.BoolTickCross:
				styleNew.DataMap = cell.DisplayFormat.GetFormatDictForBool();
				break;
			case DataFormatType.ComboList:
				styleNew.DataMap = null;
				break;
			}
			if (e.Style.Name == _grid.Styles.Highlight.Name)
			{
				styleNew.BackColor = _grid.Styles.Highlight.BackColor;
			}
			else
			{
				styleNew.BackColor = GetBackColor();
				if (Auditai.UI.Controls.Util.RgbEquals(styleNew.BackColor, Color.White))
				{
					Auditai.Model.Cell headerCell;
					if (cell.HasFormula)
					{
						styleNew.BackColor = Auditai.UI.Controls.Util.DarkenColor(UserSet.Config.TableStyle.FormalaColor, 0.08);
					}
					else if (cell.HasColumnFormula())
					{
						if (cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula)
						{
							styleNew.BackColor = Color.White;
						}
						else if (cell.IsAllowManualInputOnFormula)
						{
							styleNew.BackColor = UserSet.Config.TableStyle.AllowManualInputFormulaColor;
						}
						else
						{
							styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
						}
					}
					else if (cell.Row.Role == RowRole.Subtotal || cell.Row.Role == RowRole.Total || (cell.Column.ConsolidateAttributes != null && cell.Column.ConsolidateAttributes.Role != 0) || cell.TryGetHeaderCellFormulaCell(out headerCell) || cell.Column.CrossAttributes.Role != 0)
					{
						styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
					}
					bool flag = false;
					if (cell.Row.Role == RowRole.Among)
					{
						flag = true;
						styleNew.BackColor = UserSet.Config.TableStyle.RowAmongColor;
					}
					else if (cell.Row.Role == RowRole.Minus)
					{
						flag = true;
						styleNew.BackColor = UserSet.Config.TableStyle.RowMinusColor;
					}
					else if (cell.Row.Role == RowRole.Subtotal || cell.Row.Role == RowRole.Total)
					{
						flag = true;
						styleNew.BackColor = UserSet.Config.TableStyle.RowTotalColor;
					}
					else if (cell.Row.Role == RowRole.Fixed)
					{
						styleNew.BackColor = UserSet.Config.TableStyle.RowFixedColor;
					}
					if (flag && cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula)
					{
						styleNew.BackColor = Color.White;
					}
					if (cell.DisplayLocked != 0L || !CanEditRow(cell.Row) || cell.Row.IsLocked || !CanEditColumn(cell.Column) || Table.ControlLockCells.Contains(cell) || !Table.IsControlFormulaAllowEditRow(cell.Row))
					{
						styleNew.BackColor = UserSet.Config.TableStyle.LockAreaColor;
					}
					if (cell.Row.Role == RowRole.Header)
					{
						styleNew.BackColor = _grid.Styles[CellStyleEnum.Fixed].BackColor;
					}
					if (Program.MainForm.ShowHelperTooltip && Program.MainForm.TableValidationResults.TryGetValue(Table.TreeNode, out var value))
					{
						IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> source = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.Cells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1 == Table[e.Row, e.Col]);
						IEnumerable<Tuple<RangeOperand, ValidationResult>> source2 = ((IEnumerable<Tuple<RangeOperand, ValidationResult>>)value.Ranges).Where((Tuple<RangeOperand, ValidationResult> t) => t.Item1.TopLeft.Row.Index <= e.Row && t.Item1.TopLeft.Column.Index <= e.Col && e.Row <= t.Item1.BottomRight.Row.Index && e.Col <= t.Item1.BottomRight.Column.Index);
						IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>> source3 = ((IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>>)value.Columns).Where((Tuple<Auditai.Model.Column, ValidationResult> t) => t.Item1.Index == e.Col);
						IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> source4 = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.HeaderCells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1.Column.Index == e.Col && t.Item1.Row.Index < e.Row && t.Item1.GetHeaderLastRow() >= e.Row);
						if (source.Any() || source2.Any() || source3.Any() || source4.Any())
						{
							if (source.Any((Tuple<Auditai.Model.Cell, ValidationResult> t) => !t.Item2.Passed) || source2.Any((Tuple<RangeOperand, ValidationResult> t) => !t.Item2.Passed) || source3.Any((Tuple<Auditai.Model.Column, ValidationResult> t) => !t.Item2.Passed) || source4.Any((Tuple<Auditai.Model.Cell, ValidationResult> t) => !t.Item2.Passed))
							{
								styleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
							}
							else
							{
								styleNew.BackColor = UserSet.Config.TableStyle.CheckPassColor;
							}
						}
					}
					if (cell.Row.Table.ControlBackColorCells.TryGetValue(cell, out var value2))
				{
					styleNew.BackColor = value2;
				}
				// 跨项目数据引用单元格背景色（O(1) 查找缓存，覆盖在所有其他样式之上）
				if (_refCellStyleCache != null && _refCellStyleCache.TryGetValue((e.Row, e.Col), out var refStatus))
				{
					switch (refStatus)
					{
						case CrossProjectRefCellStyle.RefStatus.Normal:
							styleNew.BackColor = Color.FromArgb(232, 245, 233); // 浅绿色
							break;
						case CrossProjectRefCellStyle.RefStatus.CacheFallback:
							styleNew.BackColor = Color.FromArgb(255, 255, 200); // 浅黄色
							break;
						case CrossProjectRefCellStyle.RefStatus.DefaultValue:
						case CrossProjectRefCellStyle.RefStatus.Error:
							styleNew.BackColor = Color.FromArgb(255, 220, 220); // 浅红色
							break;
						case CrossProjectRefCellStyle.RefStatus.Refreshing:
							styleNew.BackColor = Color.FromArgb(200, 220, 255); // 浅蓝色
							break;
					}
				}
				}
			}
			if (e.Style.Name == _grid.Styles.Highlight.Name)
			{
				styleNew.ForeColor = _grid.Styles.Highlight.ForeColor;
			}
			else
			{
				if (Table.ControlWarningCells.Contains(cell))
				{
					styleNew.ForeColor = _warningTextColor;
					if (!_warningTextIsShown)
					{
						e.Text = string.Empty;
					}
				}
				else if (Table.ControlRemindCells.Contains(cell))
				{
					styleNew.ForeColor = _remindTextColor;
					if (!_warningTextIsShown)
					{
						e.Text = string.Empty;
					}
				}
				else
				{
					styleNew.ForeColor = cell.DisplayForeColor;
				}
				if (Table.ControlForeColorCells.TryGetValue(cell, out var value3))
				{
					styleNew.ForeColor = value3;
				}
			}
			styleNew.Font = cell.GetFont();
			styleNew.TextAlign = C1FlexGridEx.ToTextAlign(cell.DisplayAlign);
			System.Drawing.Printing.Margins margins = (System.Drawing.Printing.Margins)styleNew.Margins.Clone();
			margins.Left = cell.DisplayMargin;
			styleNew.Margins = margins;
			styleNew.WordWrap = cell.Value is string;
			styleNew.Border.Color = _grid.Styles.Normal.Border.Color;
			styleNew.Border.Width = (((Table.BorderStyle ?? TableBorderStyles.Grid).BodyLine != Auditai.Model.LineStyle.None) ? 1 : 0);
			if (e.Col == Table.Columns.Count - 1)
			{
				styleNew.Border.Direction = BorderDirEnum.Horizontal;
			}
			else
			{
				styleNew.Border.Direction = BorderDirEnum.Both;
			}
			e.Style = styleNew;
			if (!isExistManualInputValue || (!cell.HasFormula && !cell.Column.HasFormula) || !_grid.Selection.Contains(e.Row + _grid.Rows.Fixed, e.Col + _grid.Cols.Fixed) || !CanEditCell(cell, IsTableExistFillFormula()) || !cell.ShouldApplyColumnFormula())
			{
				return;
			}
			Rectangle cellRect = _grid.GetCellRect(e.Row + _grid.Rows.Fixed, e.Col + _grid.Cols.Fixed);
			bool isIconOutOfRange;
			Rectangle cancelManualInputIconArea = GetCancelManualInputIconArea(cellRect, out isIconOutOfRange);
			if (!isIconOutOfRange)
			{
				e.DrawCell(DrawCellFlags.All);
				if (_isMouseOverCancelManualInputIcon)
				{
					Rectangle rect = new Rectangle(cancelManualInputIconArea.X - 2, cancelManualInputIconArea.Y - 2, cancelManualInputIconArea.Width + 4, cancelManualInputIconArea.Height + 4);
					_cancelManualInputBackgroundBrush.Color = Auditai.UI.Controls.Util.DarkenColor(styleNew.BackColor, 0.1);
					e.Graphics.FillRectangle(_cancelManualInputBackgroundBrush, rect);
				}
				e.Graphics.DrawImage(Auditai.UI.Platform.Properties.Resources.CancelManualInput, cancelManualInputIconArea.X, cancelManualInputIconArea.Y);
				e.Handled = true;
			}
			Color GetBackColor()
			{
				Color displayBackColor = cell.DisplayBackColor;
				if (displayBackColor.ToArgb() == -1)
				{
					return Color.Transparent;
				}
				return displayBackColor;
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private bool IsTableExistFillFormula()
	{
		return Table.Columns.Any((Auditai.Model.Column u) => u.IsExistFillFormula);
	}

	private void _grid_BodyBeforeEdit(object sender, RowColEventArgs e)
	{
		if (_isEditingHeaders || Table == null)
		{
			return;
		}
		if (Program.MainForm.IsInSyncingProject)
		{
			e.Cancel = true;
			return;
		}
		try
		{
			if (!CanEditCell(Table[e.Row, e.Col], IsTableExistFillFormula()))
			{
				e.Cancel = true;
				return;
			}
			Auditai.Model.Cell cell = Table[_isEditingHeaders ? (e.Row - 1) : e.Row, e.Col];
			if (cell == null)
			{
				return;
			}
			if (cell.DisplayFormat.FormatType == DataFormatType.BoolCheckBox || cell.DisplayFormat.FormatType == DataFormatType.BoolOnOff)
			{
				e.Cancel = true;
				return;
			}
			_grid.BodySetData(e.Row, e.Col, cell.Value);
			DataFormat displayFormat = cell.DisplayFormat;
			C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
			if (displayFormat.HasComboList)
			{
				InputListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = null;
				ListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = null;
				FormulaEvaluator formulaEvaluator = new FormulaEvaluator(displayFormat.ComboList);
				if (formulaEvaluator.HasInputList())
				{
					styleNew.Editor = InputListDropDown.DropDown;
				}
				else
				{
					styleNew.Editor = ListDropDown.DropDown;
				}
				if (cell.DisplayDataType == typeof(DateYearMonth))
				{
					InputListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = ConvertDropDownListValueToDateYearMonthValue;
					ListDropDown.DropDown.ConvertEditorInputValueToOwnerCotrolNeedValue = ConvertDropDownListValueToDateYearMonthValue;
				}
			}
			else if (cell.DisplayDataType == typeof(DateTime) || cell.DisplayDataType == typeof(DateYearMonth))
			{
				styleNew.Editor = _dateEdit;
				_dateEdit.EditFormat.CustomFormat = cell.DisplayFormat.GetFormatString();
			}
			else if (cell.DisplayDataType == typeof(TimeSpan))
			{
				styleNew.Editor = _timeEdit;
			}
			else
			{
				styleNew.Editor = null;
			}
		}
		catch
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

	private void _grid_BodyAfterRowColChange(object sender, RangeEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		try
		{
			if (e.NewRange.r1 >= 0 && e.NewRange.c1 >= 0 && e.NewRange.r1 < Table.Rows.Count && e.NewRange.c1 < Table.Columns.Count)
			{
				Auditai.Model.Cell cell = Table[e.NewRange.r1, e.NewRange.c1];
				AppCommands.TableFont.FontSelector.SelectFontFamily(cell.DisplayFontFamily);
				AppCommands.TableFontSize.FontSizeSelector.SelectFontSize(cell.DisplayFontSize);
				AppCommands.Bold.IsPressed = cell.DisplayBold;
				AppCommands.Italic.IsPressed = cell.DisplayItalic;
			}
		}
		catch
		{
		}
	}

	public static void ConvertCellTimeDisplayFormatToTimeEditorFormat(string formatString, out string customFormat, out string customEditFormat)
	{
		formatString = formatString.Replace("h", "H");
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in formatString)
		{
			if (c == 'h' || c == 'm' || c == 's' || c == 'H')
			{
				stringBuilder.Append(c);
			}
			else
			{
				stringBuilder.Append("\\").Append(c);
			}
		}
		customEditFormat = stringBuilder.ToString();
		customFormat = formatString;
	}

	private void _grid_BodySetupEditor(object sender, RowColEventArgs e)
	{
		if (_isEditingHeaders)
		{
			return;
		}
		Auditai.Model.Cell cell = Table[e.Row, e.Col];
		if (cell == null) return;
		if (_grid.Editor == ListDropDown.DropDown)
		{
			ListDropDown.DropDown.DataType = typeof(string);
		}
		else if (cell.DisplayDataType == typeof(DateTime))
		{
			_dateEdit.FormatType = FormatTypeEnum.CustomFormat;
			_dateEdit.DataType = typeof(DateTime);
			if (string.IsNullOrEmpty(cell.Value?.ToString()))
			{
				_dateEdit.Value = DateTime.Now.Date;
			}
			_dateEdit.CustomFormat = cell.DisplayFormat.GetFormatString();
		}
		else if (cell.DisplayDataType == typeof(TimeSpan))
		{
			_timeEdit.DataType = typeof(object);
			_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
			_timeEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
			DateTime result;
			if (string.IsNullOrEmpty(cell.Value?.ToString()))
			{
				_timeEdit.Value = DateTime.Now;
			}
			else if (cell.Value is TimeSpan timeSpan)
			{
				_timeEdit.Value = new DateTime(2000, 1, 1, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			}
			else if (DateTime.TryParse(cell.Value?.ToString(), out result))
			{
				_timeEdit.Value = result;
			}
			else
			{
				_timeEdit.Value = DateTime.Now;
			}
			_timeEdit.DataType = typeof(DateTime);
			ConvertCellTimeDisplayFormatToTimeEditorFormat(cell.DisplayFormat.GetFormatString(), out var customFormat, out var customEditFormat);
			_timeEdit.CustomFormat = customFormat;
			_timeEdit.EditFormat.CustomFormat = customEditFormat;
		}
		else if (cell.DisplayDataType == typeof(DateYearMonth))
		{
			_dateEdit.DataType = typeof(DateTime);
			string text = cell.Value?.ToString();
			DateTime result2;
			DateTime dateTime = (string.IsNullOrEmpty(text) ? DateTime.Now.Date : ((!(cell.Value is DateYearMonth dateYearMonth)) ? ((!DateTime.TryParse(text, out result2)) ? DateTime.Now.Date : result2.Date) : dateYearMonth.Date));
			_dateEdit.Value = dateTime;
			_dateEdit.CustomFormat = cell.DisplayFormat.GetFormatString();
		}
		ListDropDown.SkipTextChanged = false;
	}

	private void _grid_BodyStartEdit(object sender, RowColEventArgs e)
	{
		if (_isEditingHeaders || e.Row < 0 || e.Row >= _grid.Rows.Count || e.Col < 0 || e.Col >= _grid.Cols.Count)
		{
			return;
		}
		_ttpComment.Hide();
		Auditai.Model.Cell cell = Table[e.Row, e.Col];
		if (cell == null) return;
		DataFormat dataFormat = ((!Table.HeaderRowCache.Contains(cell.Row)) ? cell.DisplayFormat : (cell.Style?.Format ?? Table.DefaultStyle.Format).Value);
		if (!dataFormat.HasComboList)
		{
			return;
		}
		Auditai.Model.Operand list = GetList(dataFormat.ComboList);
		if (_grid.Editor == ListDropDown.DropDown)
		{
			ListDropDown.DropDown.EditorDataType = typeof(string);
			ListDropDown.DropDown.EditorInitValue = null;
			Type displayDataType = cell.DisplayDataType;
			if (displayDataType == typeof(DateTime) || displayDataType == typeof(DateYearMonth) || displayDataType == typeof(TimeSpan))
			{
				ListDropDown.DropDown.EditorInitValue = cell.GetDisplayValue();
			}
			ListDropDown.DropDown.Font = cell.GetFont();
			if (list is TreeListOperand op)
			{
				if (dataFormat.MultiComboList)
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
				if (dataFormat.MultiComboList)
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
				if (dataFormat.MultiComboList)
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
				if (dataFormat.MultiComboList)
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
			Type displayDataType2 = cell.DisplayDataType;
			if (displayDataType2 == typeof(DateTime) || displayDataType2 == typeof(DateYearMonth) || displayDataType2 == typeof(TimeSpan))
			{
				InputListDropDown.DropDown.EditorInitValue = cell.GetDisplayValue();
			}
			InputListDropDown.Clear();
			InputListDropDown.CanInputTextbox = dataFormat.IgnoreComboList;
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

	private void _grid_BodyValidateEdit(object sender, ValidateEditEventArgs e)
	{
		if (!_isEditingHeaders)
		{
			Auditai.Model.Cell cell = Table[e.Row, e.Col];
			if (cell == null) return;
			DataFormat displayFormat = cell.DisplayFormat;
			if (cell.DisplayDataType == typeof(DateYearMonth) && _dateEdit.Value is DateTime)
			{
				DateTime date = (DateTime)_dateEdit.Value;
				_dateEdit.DataType = typeof(DateYearMonth);
				_dateEdit.Value = new DateYearMonth(date)
				{
					ToStringFormat = displayFormat.GetFormatString()
				};
			}
			else if (cell.DisplayDataType == typeof(TimeSpan) && _timeEdit.Value is DateTime dateTime)
			{
				_timeEdit.DataType = typeof(object);
				_timeEdit.Value = new TimeSpan(dateTime.Hour, dateTime.Minute, dateTime.Second);
				_timeEdit.DataType = typeof(TimeSpan);
			}
			if (displayFormat.HasComboList && _grid.Editor != InputListDropDown.DropDown && _grid.Editor == ListDropDown.DropDown && !displayFormat.IgnoreComboList && !ListDropDown.Validate())
			{
				_grid.FinishEditing(cancel: true);
			}
			ListDropDown.SkipTextChanged = true;
		}
	}

	private void _grid_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		if (e.Col == 0)
		{
			DoLayout();
		}
	}

	private void _grid_PaintBackground(object sender, PaintEventArgs e)
	{
		if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.Picture))
		{
			Point ptScreen = _grid.Parent.PointToScreen(_grid.Location);
			Point point = _owner.PnlMainRelativePosition(ptScreen);
			Bitmap currentBackgroudImage = Theme.CurrentBackgroudImage;
			e.Graphics.DrawImage(currentBackgroudImage, 0, 0, new Rectangle(point.X, point.Y, currentBackgroudImage.Width - point.X, currentBackgroudImage.Height - point.Y), GraphicsUnit.Pixel);
		}
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		try
		{
			if (Table != null)
			{
				DrawRowResizing(e);
				DrawColumnResizing(e);
				DrawBorderStyle(e);
				DrawRefIntervals(e);
				DrawEditingFormula(e);
				DrawBeginEditingColHeader(e);
			}
		}
		catch
		{
		}
		void DrawBeginEditingColHeader(PaintEventArgs e)
		{
			if (Program.MainForm.State.ViewKind == MainFormView.Table && !_grid.IsResizingColumn && !_grid.IsResizingRow && !IsTableLocked && !AuxEditor.IsEditing && !TitleEditor.AuxEditor.IsEditing && !FootEditor.AuxEditor.IsEditing && !FormControlFormula.IsEditing && !LedgerCollectFormulaEditor.IsEditing && HasSchemaPermission() && _mouseHeaderCol > 0)
			{
				Auditai.Model.Column column = Table.Columns[_mouseHeaderCol - _grid.Cols.Fixed];
				if (CanEditColumn(column) && !column.IsLocked && column.Width >= GetColumnMinWidthForShowColumnHeaderIcon())
				{
					Rectangle editColHeaderImageRectangle = GetEditColHeaderImageRectangle(_mouseHeaderCol);
					if (_isMouseInStartEditingColHeaderImageRect)
					{
						_brushStartEditingColHeaderBackground.Color = Auditai.UI.Controls.Util.DarkenColor(_grid.Styles.SelectedColumnHeader.BackColor, 0.1);
						e.Graphics.FillRectangle(_brushStartEditingColHeaderBackground, editColHeaderImageRectangle);
					}
					e.Graphics.DrawImage(Auditai.UI.Platform.Properties.Resources.EditColHeader, editColHeaderImageRectangle.Location);
				}
				if (column.Width >= GetColumnMinWidthForShowColumnHeaderIcon() && !_grid.FilterManager.IsColumnInFilting(_mouseHeaderCol))
				{
					Rectangle colHeaderShowMoreMenuImageRectangle = GetColHeaderShowMoreMenuImageRectangle(_mouseHeaderCol);
					if (_isMouseInColHeaderShowMoreMenuImageRect)
					{
						Rectangle colHeaderShowMoreMenuImageShadowRectangle = GetColHeaderShowMoreMenuImageShadowRectangle(_mouseHeaderCol);
						_brushStartEditingColHeaderBackground.Color = Auditai.UI.Controls.Util.DarkenColor(_grid.Styles.SelectedColumnHeader.BackColor, 0.1);
						e.Graphics.FillRectangle(_brushStartEditingColHeaderBackground, colHeaderShowMoreMenuImageShadowRectangle);
					}
					e.Graphics.DrawImage(Auditai.UI.Platform.Properties.Resources.menuMoreOperation, colHeaderShowMoreMenuImageRectangle.Location);
				}
			}
		}
		void DrawBorderStyle(PaintEventArgs e)
		{
			TableBorderStyle borderStyle = Table.BorderStyle;
			if (borderStyle != null)
			{
				int num15 = _grid.Cols[_grid.Cols.Count - 1].Right + _grid.ScrollPosition.X;
				int right = _grid.Cols[_grid.Cols.Fixed - 1].Right;
				int num16 = ((_grid.Rows.Fixed > 0) ? _grid.Rows[_grid.Rows.Fixed - 1].Bottom : 0);
				int num17 = _grid.Rows[_grid.Rows.Count - 1].Bottom + _grid.ScrollPosition.Y;
				Color color = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
				if (borderStyle.SecondLine != 0)
				{
					e.Graphics.DrawLine(GetPen(borderStyle.SecondLine), 0, num16 - 1, num15 - 1, num16 - 1);
				}
				if (borderStyle.UpDownLine != 0)
				{
					if (borderStyle.UpDownLine == Auditai.Model.LineStyle.Thick)
					{
						e.Graphics.DrawLine(GetPen(borderStyle.UpDownLine), 0, 0, num15 - 1, 0);
					}
					if ((Table.BorderStyle ?? TableBorderStyles.Grid).UpDownLine != 0)
					{
						e.Graphics.DrawLine(GetPen(borderStyle.UpDownLine), 0, num17 - 1, num15 - 1, num17 - 1);
					}
				}
				if (borderStyle.LeftRightLine != 0)
				{
					Pen pen = GetPen(borderStyle.LeftRightLine);
					e.Graphics.DrawLine(pen, pen.Width - 1f, 0f, pen.Width - 1f, num17 - 1);
					e.Graphics.DrawLine(GetPen(borderStyle.LeftRightLine), num15 - 1, 0, num15 - 1, num17 - 1);
				}
			}
		}
		void DrawColumnResizing(PaintEventArgs e)
		{
			if (_isColumnResizingStartedDragging)
			{
				Point point = _grid.PointToScreen(Point.Empty);
				int num18 = Control.MousePosition.X - point.X;
				e.Graphics.DrawLine(PenResizeDragging, num18, 0, num18, _grid.ClientSize.Height);
			}
		}
		void DrawEditingFormula(PaintEventArgs e)
		{
			if (FormulaEditor.IsEditing || ValidationEditor.IsEditing || AuxEditor.IsEditing || TitleEditor.AuxEditor.IsEditing || FootEditor.AuxEditor.IsEditing || LedgerCollectFormulaEditor.IsEditing)
			{
				if (FormulaContext.Table == Table)
				{
					_penFormulaCell.Color = Theme.SelectedAuditaiTheme.ThemeContext.DarkColor;
					switch (FormulaEditor.Context.Kind)
					{
					case FormulaContextKind.Cell:
					{
						int num3 = FormulaContext.Cell.Row.Index + _grid.Rows.Fixed;
						int num4 = FormulaContext.Cell.Column.Index + _grid.Cols.Fixed;
						Rectangle cellRangeRectUnclipped = _grid.GetCellRangeRectUnclipped(num3, num4, num3, num4);
						cellRangeRectUnclipped.Offset(-1, -1);
						e.Graphics.DrawRectangle(_penFormulaCell, cellRangeRectUnclipped);
						break;
					}
					case FormulaContextKind.Column:
					{
						int num2 = FormulaContext.Column.Index + _grid.Cols.Fixed;
						Rectangle cellRangeRectUnclippedJustContainsViewport2 = GetCellRangeRectUnclippedJustContainsViewport(_grid, _grid.Rows.Fixed, num2, _grid.Rows.Count - 1, num2);
						cellRangeRectUnclippedJustContainsViewport2.Offset(-1, -1);
						e.Graphics.DrawRectangle(_penFormulaCell, cellRangeRectUnclippedJustContainsViewport2);
						break;
					}
					case FormulaContextKind.HeaderCell:
					{
						int row = FormulaContext.Cell.Row.Index + 1 + _grid.Rows.Fixed;
						int num = FormulaContext.Cell.Column.Index + _grid.Cols.Fixed;
						int row2 = FormulaContext.Cell.GetHeaderLastRow() + _grid.Rows.Fixed;
						Rectangle cellRangeRectUnclippedJustContainsViewport = GetCellRangeRectUnclippedJustContainsViewport(_grid, row, num, row2, num);
						cellRangeRectUnclippedJustContainsViewport.Offset(-1, -1);
						e.Graphics.DrawRectangle(_penFormulaCell, cellRangeRectUnclippedJustContainsViewport);
						break;
					}
					}
				}
				if (FormulaEditor.LastClickedComponent is TableEditor && _grid.Selection.TopRow >= 0 && _grid.Selection.LeftCol >= 0)
				{
					try
					{
						int num5 = _grid.Selection.TopRow;
						if (Table.Rows[num5 - _grid.Rows.Fixed].Role == RowRole.Header)
						{
							num5++;
						}
						Rectangle cellRangeRectUnclippedJustContainsViewport3 = GetCellRangeRectUnclippedJustContainsViewport(_grid, num5, _grid.Selection.LeftCol, _grid.Selection.BottomRow, _grid.Selection.RightCol);
						cellRangeRectUnclippedJustContainsViewport3.Offset(-1, -1);
						cellRangeRectUnclippedJustContainsViewport3.Inflate(-1, -1);
						_penAnimateDash.Color = ((Control.MouseButtons == MouseButtons.Left) ? Theme.SelectedAuditaiTheme.ThemeContext.DarkColor : FormulaEditor.NextColor);
						e.Graphics.DrawRectangle(_penAnimateDash, cellRangeRectUnclippedJustContainsViewport3);
					}
					catch (ArgumentOutOfRangeException)
					{
					}
				}
			}
		}
		void DrawRefIntervals(PaintEventArgs e)
		{
			if (FormulaEditor.RefIntervals != null)
			{
				foreach (FormulaDisplayRef refInterval in FormulaEditor.RefIntervals)
				{
					FormulaDisplayRef ri = refInterval;
					switch (ri.Kind)
					{
					case FormulaDisplayRefKind.Cell:
						if (ri.Table == Table)
						{
							int row11 = ri.Cell.Row.Index + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
							int col5 = ri.Cell.Column.Index + _grid.Cols.Fixed;
							DrawBorder(_grid.GetCellRectUnclipped(row11, col5), clip: true);
						}
						break;
					case FormulaDisplayRefKind.Column:
						if (ri.Table == Table)
						{
							int col6 = ri.Column.Index + _grid.Cols.Fixed;
							DrawBorder(_grid.GetColumnRectUnclipped(col6), clip: true);
						}
						break;
					case FormulaDisplayRefKind.ColumnWildcard:
						if (ri.Table == Table)
						{
							if (FormulaEditor.Context.Kind == FormulaContextKind.Cell)
							{
								int num11 = FormulaEditor.Context.Cell.Row.Index + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								int col7 = ri.Column.Index + _grid.Cols.Fixed;
								if (num11 < _grid.Rows.Count)
								{
									DrawBorder(_grid.GetCellRectUnclipped(num11, col7), clip: true);
								}
							}
							else if (FormulaEditor.Context.Kind == FormulaContextKind.Column)
							{
								int col8 = ri.Column.Index + _grid.Cols.Fixed;
								DrawBorder(_grid.GetColumnRectUnclipped(col8), clip: true);
							}
							else if (FormulaEditor.Context.Kind == FormulaContextKind.Validation)
							{
								int col9 = ri.Column.Index + _grid.Cols.Fixed;
								DrawBorder(_grid.GetColumnRectUnclipped(col9), clip: true);
							}
							else if (FormulaEditor.Context.Kind == FormulaContextKind.HeaderCell)
							{
								Auditai.Model.Cell cell2 = FormulaEditor.Context.Cell;
								int row12 = cell2.Row.Index + 1 + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								int row13 = cell2.GetHeaderLastRow() + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								int num12 = ri.Column.Index + _grid.Cols.Fixed;
								DrawBorder(_grid.GetCellRangeRectUnclipped(row12, num12, row13, num12), clip: true);
							}
						}
						break;
					case FormulaDisplayRefKind.HeaderCellWildcard:
						if (ri.Table == Table)
						{
							if (FormulaEditor.Context.Kind == FormulaContextKind.Cell)
							{
								int num7 = FormulaEditor.Context.Cell.Row.Index + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								int col = ri.Cell.Column.Index + _grid.Cols.Fixed;
								if (num7 < _grid.Rows.Count)
								{
									DrawBorder(_grid.GetCellRectUnclipped(num7, col), clip: true);
								}
							}
							else if (FormulaEditor.Context.Kind == FormulaContextKind.Column)
							{
								int col2 = ri.Cell.Column.Index + _grid.Cols.Fixed;
								DrawBorder(_grid.GetColumnRectUnclipped(col2), clip: true);
							}
							else if (FormulaEditor.Context.Kind == FormulaContextKind.Validation)
							{
								int num8 = ri.Cell.Column.Index + _grid.Cols.Fixed;
								int row5 = ri.Cell.Row.Index + 1 + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								int row6 = ri.Cell.GetHeaderLastRow() + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								DrawBorder(_grid.GetCellRangeRectUnclipped(row5, num8, row6, num8), clip: true);
							}
							else if (FormulaEditor.Context.Kind == FormulaContextKind.HeaderCell)
							{
								Auditai.Model.Cell cell = FormulaEditor.Context.Cell;
								int row7 = cell.Row.Index + 1 + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								int row8 = cell.GetHeaderLastRow() + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
								int num9 = ri.Cell.Column.Index + _grid.Cols.Fixed;
								DrawBorder(_grid.GetCellRangeRectUnclipped(row7, num9, row8, num9), clip: true);
							}
						}
						break;
					case FormulaDisplayRefKind.Range:
						if (ri.Table == Table)
						{
							int row9 = ri.Cell.Row.Index + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
							int col3 = ri.Cell.Column.Index + _grid.Cols.Fixed;
							int row10 = ri.Cell2.Row.Index + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
							int col4 = ri.Cell2.Column.Index + _grid.Cols.Fixed;
							DrawBorder(_grid.GetCellRangeRectUnclipped(row9, col3, row10, col4), clip: true);
						}
						break;
					case FormulaDisplayRefKind.ColumnHeader:
						if (ri.Table == Table)
						{
							int num10 = ri.Column.Index + _grid.Cols.Fixed;
							if (FormulaEditor.Context.Kind == FormulaContextKind.ColHeader && !FormulaEditor.IsFinishingEditing && FormulaEditor.Context.Table == Table)
							{
								DrawBorder(_grid.GetCellRectUnclipped(0, num10), clip: false);
							}
							else
							{
								DrawBorder(_grid.GetCellRangeRectUnclipped(0, num10, _grid.Rows.Fixed - 1, num10), clip: false);
							}
						}
						break;
					case FormulaDisplayRefKind.HeaderCell:
						if (ri.Table == Table)
						{
							int row3 = ri.Cell.Row.Index + 1 + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
							int row4 = ri.Cell.GetHeaderLastRow() + (_isEditingHeaders ? 1 : _grid.Rows.Fixed);
							int num6 = ri.Cell.Column.Index + _grid.Cols.Fixed;
							DrawBorder(_grid.GetCellRangeRectUnclipped(row3, num6, row4, num6), clip: true);
						}
						break;
					}
					void DrawBorder(Rectangle rect, bool clip)
					{
						_penFormulaRefRect.Color = ri.Color;
						rect.Offset(-1, -1);
						if (rect.Top < 0)
						{
							rect = new Rectangle(rect.Left, 0, rect.Width, rect.Height + rect.Top);
						}
						if (clip)
						{
							int num13 = ((_grid.Cols.Fixed > 0) ? (_grid.Cols[_grid.Cols.Fixed - 1].Right - 1) : 0);
							int num14 = ((_grid.Rows.Fixed > 0) ? (_grid.Rows[_grid.Rows.Fixed - 1].Bottom - 1) : 0);
							e.Graphics.SetClip(new Rectangle(num13, num14, _grid.ClientRectangle.Width - num13, _grid.ClientRectangle.Height - num14));
						}
						e.Graphics.DrawRectangle(_penFormulaRefRect, rect);
						_brushFormulaRefRect.Color = Color.FromArgb(20, _penFormulaRefRect.Color);
						e.Graphics.FillRectangle(_brushFormulaRefRect, rect);
						if (clip)
						{
							e.Graphics.ResetClip();
						}
					}
				}
			}
		}
		void DrawRowResizing(PaintEventArgs e)
		{
			if (_isRowResizingStartedDragging)
			{
				Point point2 = _grid.PointToScreen(Point.Empty);
				int num19 = Control.MousePosition.Y - point2.Y;
				e.Graphics.DrawLine(PenResizeDragging, 0, num19, _grid.ClientSize.Width, num19);
			}
		}
	}

	private int GetColumnMinWidthForShowColumnHeaderIcon()
	{
		return 100;
	}

	protected Rectangle GetCellRangeRectUnclippedJustContainsViewport(C1FlexGridEx grid, int row1, int col1, int row2, int col2)
	{
		if (row2 - row1 <= 100)
		{
			return grid.GetCellRangeRectUnclipped(row1, col1, row2, col2);
		}
		int num = 0;
		for (int i = 0; i < grid.Rows.Fixed; i++)
		{
			C1.Win.C1FlexGrid.Row row3 = grid.Rows[i];
			if (row3.Visible)
			{
				num += row3.HeightDisplay;
			}
		}
		int height = grid.Height;
		int num2 = 0;
		int num3 = 0;
		int num4 = -1;
		int num5 = -1;
		int num6 = Math.Abs(grid.ScrollPosition.Y);
		bool flag = false;
		for (int j = grid.Rows.Fixed; j < grid.Rows.Count; j++)
		{
			C1.Win.C1FlexGrid.Row row4 = grid.Rows[j];
			if (!row4.Visible)
			{
				continue;
			}
			if (!flag)
			{
				if (num2 + row4.HeightDisplay + num >= num6)
				{
					flag = true;
					num4 = j;
				}
				else
				{
					num2 += row4.HeightDisplay;
				}
				continue;
			}
			if (num3 - num >= height)
			{
				num5 = j;
				break;
			}
			num3 += row4.HeightDisplay;
		}
		if (num4 == -1)
		{
			num4 = grid.Rows.Fixed;
		}
		if (num5 == -1)
		{
			num5 = grid.Rows.Count - 1;
		}
		if (num4 < row1)
		{
			num4 = row1;
		}
		if (num5 > row2)
		{
			num5 = row2;
		}
		Rectangle cellRectUnclipped = grid.GetCellRectUnclipped(num4, col1);
		Rectangle cellRectUnclipped2 = grid.GetCellRectUnclipped(num5, col2);
		return new Rectangle(cellRectUnclipped.Left, cellRectUnclipped.Top, cellRectUnclipped2.Right - cellRectUnclipped.Left, cellRectUnclipped2.Bottom - cellRectUnclipped.Top);
	}

	private void PnlGrid_Paint(object sender, PaintEventArgs e)
	{
		if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.Picture))
		{
			Point ptScreen = pnlGrid.Parent.PointToScreen(pnlGrid.Location);
			Point point = _owner.PnlMainRelativePosition(ptScreen);
			Bitmap currentBackgroudImage = Theme.CurrentBackgroudImage;
			e.Graphics.DrawImage(currentBackgroudImage, 0, 0, new Rectangle(point.X, point.Y, currentBackgroudImage.Width - point.X, currentBackgroudImage.Height - point.Y), GraphicsUnit.Pixel);
		}
	}

	private void PnlGrid_Resize(object sender, EventArgs e)
	{
		DoLayout();
	}

	private void PnlGrid_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && !_isEditingHeaders)
		{
			ctxEmpty.ShowContextMenu(pnlGrid, e.Location);
		}
	}

	private Rectangle GetEditColHeaderImageRectangle(int col)
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			return new Rectangle(new Point(-10000, -10000), new Size(0, 0));
		}
		Rectangle cellRect = _grid.GetCellRect(_grid.Rows.Fixed - 1, col);
		int x = cellRect.Left + 4;
		int y = cellRect.Top + (cellRect.Height - Auditai.UI.Platform.Properties.Resources.EditColHeader.Height) / 2;
		return new Rectangle(new Point(x, y), Auditai.UI.Platform.Properties.Resources.EditColHeader.Size);
	}

	private Rectangle GetColHeaderShowMoreMenuImageRectangle(int col)
	{
		Rectangle cellRect = _grid.GetCellRect(_grid.Rows.Fixed - 1, col);
		int x = cellRect.Right - Auditai.UI.Platform.Properties.Resources.menuMoreOperation.Width - 8;
		int y = cellRect.Top + (cellRect.Height - Auditai.UI.Platform.Properties.Resources.menuMoreOperation.Height) / 2;
		return new Rectangle(new Point(x, y), Auditai.UI.Platform.Properties.Resources.menuMoreOperation.Size);
	}

	private Rectangle GetColHeaderShowMoreMenuImageShadowRectangle(int col)
	{
		int num = 2;
		int num2 = 2;
		Rectangle colHeaderShowMoreMenuImageRectangle = GetColHeaderShowMoreMenuImageRectangle(col);
		return new Rectangle(colHeaderShowMoreMenuImageRectangle.X - num, colHeaderShowMoreMenuImageRectangle.Y - num2, colHeaderShowMoreMenuImageRectangle.Width + num * 2, colHeaderShowMoreMenuImageRectangle.Height + num2 * 2);
	}

	private void FilterManager_Changed(object sender, EventArgs e)
	{
		Table.UpdateFilterInfo(_grid.FilterManager.Filters.Serialize());
		BodySelectionChanged_Stats();
		DoLayout();
	}

	public void BeginFormula()
	{
		_grid.Styles.Highlight.ForeColor = Color.Black;
		_grid.Styles.Highlight.BackColor = Color.Transparent;
		TitleEditor.View.HighLight = HighLightEnum.Never;
		FootEditor.View.HighLight = HighLightEnum.Never;
		_timerFormulaHighlight.Start();
		_grid.FilterManager.IsEditingFormula = true;
	}

	public void EndFormula()
	{
		_timerFormulaHighlight.Stop();
		_penAnimateDash.DashOffset = 0f;
		_grid.Styles.Highlight.ForeColor = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Highlight\\ForeColor");
		_grid.Styles.Highlight.BackColor = Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background");
		TitleEditor.View.HighLight = HighLightEnum.WithFocus;
		FootEditor.View.HighLight = HighLightEnum.WithFocus;
		_grid.FilterManager.IsEditingFormula = false;
		FormulaEditor.View.Enabled = true;
	}

	public void SuspendNavPanelVisibleDrawing()
	{
		Program.MainForm.SuspendNavPanelVisible();
	}

	public void ResumeNavPanelVisibleDrawing()
	{
		Program.MainForm.ResumeNavPanelVisible();
	}

	public void OnFormulaEditorBeganEditing()
	{
		SuspendNavPanelVisibleDrawing();
		try
		{
			_owner.SwitchStateTo(MainFormView.EditingFormula);
			ValidationEditor.Enabled = false;
			_grid.Select(-1, -1);
			_formulaTextBeforeEdit = FormulaEditor.FormulaText;
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	public void OnFormulaEditorBeganEditing_ColHeader()
	{
		SuspendNavPanelVisibleDrawing();
		try
		{
			EndEditColHeaders();
			_owner.SwitchStateTo(MainFormView.EditingFormula);
			ValidationEditor.Enabled = false;
			AppCommands.Information.HideInformation();
			_grid.Select(-1, -1);
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	public void OnFormulaEditorFinishedEditing()
	{
		SuspendNavPanelVisibleDrawing();
		try
		{
			FormulaContext formulaContext = FormulaEditor.Context.Clone();
			FormulaEvaluator.ClearCache();
			_owner.ProjectHierarchy.FindAndSelectNode(formulaContext.Table.TreeNode);
			Table = formulaContext.Table;
			PopulateTable();
			_owner.SwitchStateTo(MainFormView.Table);
			switch (formulaContext.Kind)
			{
			case FormulaContextKind.Cell:
			case FormulaContextKind.HeaderCell:
				_grid.Select();
				Select(formulaContext.Cell.Row.Index, formulaContext.Cell.Column.Index);
				break;
			case FormulaContextKind.Column:
				_grid.Select();
				SelectColumn(formulaContext.Column.Index);
				if (!formulaContext.Column.HasFormula || string.IsNullOrWhiteSpace(_formulaTextBeforeEdit))
				{
					ClearColumnAllowManualInputFlag(formulaContext.Column.Index, isReCalcTable: true);
				}
				break;
			case FormulaContextKind.Title:
				TitleEditor.View.Invalidate();
				TitleEditor.View.Select();
				TitleEditor.EnterEdit();
				break;
			case FormulaContextKind.Foot:
				FootEditor.View.Invalidate();
				FootEditor.View.Select();
				FootEditor.EnterEdit();
				break;
			case FormulaContextKind.ColHeader:
				_grid.Select();
				Select(0, formulaContext.Column.Index);
				break;
			case FormulaContextKind.Document:
			case FormulaContextKind.Validation:
			case FormulaContextKind.TableAuxEdit:
			case FormulaContextKind.TitleAuxEdit:
			case FormulaContextKind.FootAuxEdit:
				break;
			}
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	public void OnFormulaEditorFinishedEditingTitle()
	{
		SuspendNavPanelVisibleDrawing();
		try
		{
			FormulaEvaluator.ClearCache();
			_owner.ProjectHierarchy.FindAndSelectNode(FormulaContext.Table.TreeNode);
			Table = FormulaContext.Table;
			PopulateTable();
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	public void ValidationEditor_BeganEditing(object sender, EventArgs e)
	{
		SuspendNavPanelVisibleDrawing();
		try
		{
			FormulaContext.Kind = FormulaContextKind.Validation;
			_timerFormulaHighlight.Start();
			_tableBeforeEnteringValidation = Table;
			Program.MainForm.SwitchStateTo(MainFormView.EditingValidation);
			FormulaEditor.View.Enabled = false;
			_grid.Styles.Highlight.ForeColor = Color.Black;
			_grid.Styles.Highlight.BackColor = Color.Transparent;
			_grid.Select(-1, -1);
			_grid.FilterManager.IsEditingFormula = true;
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	private void ValidationEditor_FinishedEditing(object sender, EventArgs e)
	{
		SuspendNavPanelVisibleDrawing();
		try
		{
			FormulaEditor.RefIntervals = null;
			_timerFormulaHighlight.Stop();
			Program.MainForm.ProjectHierarchy.FindAndSelectNode(_tableBeforeEnteringValidation.TreeNode);
			Table = _tableBeforeEnteringValidation;
			PopulateTable();
			Program.MainForm.SwitchStateTo(MainFormView.Table);
			_grid.Styles.Highlight.ForeColor = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Highlight\\ForeColor");
			_grid.Styles.Highlight.BackColor = Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background");
			_grid.FilterManager.IsEditingFormula = false;
		}
		finally
		{
			ResumeNavPanelVisibleDrawing();
		}
	}

	private void _ttpComment_CloseClick(object sender, EventArgs e)
	{
		_owner.TtpCommentClosedShowInformation();
	}

	private void Frf_FindNextHandler(object sender, FindNextEventArgs e)
	{
		if (!(Program.MainForm.ProjectHierarchy.SelectedNode is TreeTableNode) || _grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (e.ScopeMode == ScopeMode.Current)
		{
			if (bodySelection.IsSingleCell)
			{
				foreach (Auditai.Model.Cell item in from c in Table.Cells.Skip(bodySelection.TopRow * Table.Columns.Count + bodySelection.LeftCol + 1)
					where _grid.BodyGetRow(c.Row.Index).Visible && _grid.BodyGetCol(c.Column.Index).Visible
					select c)
				{
					if (FindImpl(item.Value?.ToString() ?? string.Empty, e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						_grid.BodySelect(item.Row.Index, item.Column.Index);
						return;
					}
				}
				foreach (Auditai.Model.Cell item2 in from c in Table.Cells.Take(bodySelection.TopRow * Table.Columns.Count + bodySelection.LeftCol + 1)
					where _grid.BodyGetRow(c.Row.Index).Visible && _grid.BodyGetCol(c.Column.Index).Visible
					select c)
				{
					if (FindImpl(item2.Value?.ToString() ?? string.Empty, e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						_grid.BodySelect(item2.Row.Index, item2.Column.Index);
						return;
					}
				}
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已查找当前表格，未找到字符串。");
				return;
			}
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				if (!_grid.BodyGetRow(i).Visible)
				{
					continue;
				}
				for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
				{
					if (_grid.BodyGetCol(j).Visible)
					{
						string src = (Table[i, j]?.Value?.ToString()) ?? string.Empty;
						if (FindImpl(src, e.FindValue, e.MatchMode, e.IsMatchCase))
						{
							_grid.BodySelect(i, j);
							return;
						}
					}
				}
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选区内无查找结果");
		}
		else
		{
			if (e.ScopeMode != ScopeMode.Global)
			{
				return;
			}
			foreach (Auditai.Model.Cell item3 in from c in Table.Cells.Skip(bodySelection.TopRow * Table.Columns.Count + bodySelection.LeftCol + 1)
				where _grid.BodyGetRow(c.Row.Index).Visible && _grid.BodyGetCol(c.Column.Index).Visible
				select c)
			{
				if (FindImpl(item3.Value?.ToString() ?? string.Empty, e.FindValue, e.MatchMode, e.IsMatchCase))
				{
					_grid.BodySelect(item3.Row.Index, item3.Column.Index);
					return;
				}
			}
			foreach (Auditai.Model.Table item4 in from n in Auditai.Model.Project.Current.GetAllTableNodes().SkipWhile((TreeTableNode n) => n.Table != Table).Skip(1)
				select n.Table)
			{
				foreach (Auditai.Model.Cell cell in item4.LoadAndReturn().Cells)
				{
					if (FindImpl(cell.Value?.ToString() ?? string.Empty, e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						Program.MainForm.ProjectHierarchy.FindAndSelectNode(item4.TreeNode);
						_grid.BodySelect(cell.Row.Index, cell.Column.Index);
						return;
					}
				}
			}
			foreach (Auditai.Model.Table item5 in from n in Auditai.Model.Project.Current.GetAllTableNodes().TakeWhile((TreeTableNode n) => n.Table != Table)
				select n.Table)
			{
				foreach (Auditai.Model.Cell cell2 in item5.LoadAndReturn().Cells)
				{
					if (FindImpl(cell2.Value.ToString(), e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						Program.MainForm.ProjectHierarchy.FindAndSelectNode(item5.TreeNode);
						_grid.BodySelect(cell2.Row.Index, cell2.Column.Index);
						return;
					}
				}
			}
			foreach (Auditai.Model.Cell item6 in from c in Table.Cells.Take(bodySelection.TopRow * Table.Columns.Count + bodySelection.LeftCol + 1)
				where _grid.BodyGetRow(c.Row.Index).Visible && _grid.BodyGetCol(c.Column.Index).Visible
				select c)
			{
				if (FindImpl(item6.Value?.ToString() ?? string.Empty, e.FindValue, e.MatchMode, e.IsMatchCase))
				{
					_grid.BodySelect(item6.Row.Index, item6.Column.Index);
					return;
				}
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已查找全部表格，未找到字符串。");
		}
	}

	private void Frf_ReplaceHandler(object sender, ReplaceEventArgs e)
	{
		if (!(Program.MainForm.ProjectHierarchy.SelectedNode is TreeTableNode))
		{
			return;
		}
		bool isExistFillFormula = IsTableExistFillFormula();
		if (e.IsReplaceAll)
		{
			if (e.ScopeMode == ScopeMode.Current)
			{
				C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
				if (bodySelection.IsSingleCell)
				{
					ReplaceTable(Table);
				}
				else
				{
					foreach (Auditai.Model.Cell item in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
					{
						ReplaceCell(item);
					}
				}
			}
			else if (e.ScopeMode == ScopeMode.Global)
			{
				foreach (TreeTableNode allTableNode in Auditai.Model.Project.Current.GetAllTableNodes())
				{
					ReplaceTable(allTableNode.Table.LoadAndReturn());
				}
			}
		}
		else
		{
			if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
			{
				return;
			}
			C1.Win.C1FlexGrid.CellRange bodySelection2 = _grid.BodySelection;
			Auditai.Model.Cell cell2 = Table[bodySelection2.TopRow, bodySelection2.LeftCol];
			ReplaceCell(cell2);
			Frf_FindNextHandler(sender, new FindNextEventArgs
			{
				FindValue = e.FindValue,
				IsMatchCase = e.IsMatchCase,
				MatchMode = e.MatchMode,
				ScopeMode = e.ScopeMode
			});
		}
		Invalidate();
		void ReplaceCell(Auditai.Model.Cell cell)
		{
			if (CanEditCell(cell, isExistFillFormula) && (!cell.HasCellFormulaOrColumnFormula || cell.IsExistManualInputValue))
			{
				string displayValue = cell.GetDisplayValue();
				if (FindImpl(displayValue, e.FindValue, e.MatchMode, e.IsMatchCase))
				{
					if (e.ReplaceMode == ReplaceMode.AllText)
					{
						cell.UpdateValue(e.ReplaceValue);
					}
					else if (e.ReplaceMode == ReplaceMode.MatchText)
					{
						cell.UpdateValue(displayValue.Replace(e.FindValue, e.ReplaceValue));
					}
				}
			}
		}
		void ReplaceTable(Auditai.Model.Table table)
		{
			table.BeginBatchUpdateValue();
			foreach (Auditai.Model.Cell cell3 in table.Cells)
			{
				ReplaceCell(cell3);
			}
			table.EndBatchUpdateValue();
		}
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

	private void MoveRows(int index, int count, int newIndex)
	{
		_grid.BeginUpdate();
		Table.Rows.Move(index, count, newIndex);
		PopulateRows();
		PopulateMerges();
		_grid.EndUpdate();
	}

	private void MoveColumns(int index, int count, int newIndex)
	{
		_grid.BeginUpdate();
		Table.Columns.Move(index, count, newIndex);
		PopulateColumns();
		PopulateMerges();
		_grid.EndUpdate();
	}

	private void SetClipboard()
	{
		try
		{
			Clipboard = Tuple.Create(Table[_grid.BodyRow, _grid.BodyCol], Table[_grid.BodyRowSel, _grid.BodyColSel]);
		}
		catch (ArgumentOutOfRangeException)
		{
		}
	}

	private void Initialize(MainForm owner)
	{
		AuxEditor = new AuxEditor();
		LedgerCollectFormulaEditor = new LedgerCollectFormulaEditor();
		ValidationEditor = new ValidationEditor(this);
		ValidationEditor.BeganEditing += ValidationEditor_BeganEditing;
		ValidationEditor.FinishedEditing += ValidationEditor_FinishedEditing;
		TableNavGrid = new TableNavGrid();
		TableNavGrid.TreeLeafNodeSelected += TableNavGrid_TreeLeafNodeSelected;
		TitleEditor = new TableTitleEditor(this);
		FootEditor = new TableFootEditor(this);
		C1SplitContainer c1SplitContainer2 = (View = new C1SplitContainer
		{
			BorderWidth = 0,
			Dock = DockStyle.Fill
		});
		c1SplitContainer2.SuspendLayout();
		pnlToolbar = new C1SplitterPanel
		{
			Collapsible = false,
			KeepRelativeSize = false,
			Width = 80,
			Resizable = false,
			Dock = PanelDockStyle.Right
		};
		pnlToolbar.Controls.Add(ToolBar);
		c1SplitContainer2.Panels.Add(pnlToolbar);
		pnlValidation = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Bottom,
			Resizable = true,
			KeepRelativeSize = false,
			Collapsible = false,
			MinHeight = 50,
			TabIndex = 3
		};
		pnlValidation.Controls.Add(ValidationEditor.View);
		c1SplitContainer2.Panels.Add(pnlValidation);
		C1Command c1Command = new C1Command
		{
			Text = "删除快捷查询列表",
			Image = ContextResources.ctxDelete
		};
		c1Command.Click += CmdDeleteNavTree_Click;
		_navTitleCtx = new C1ContextMenu();
		_navTitleCtx.HideFirstDelimiter = true;
		_navTitleCtx.CommandLinks.Add(new C1CommandLink(c1Command));
		_navTreeTitlePanel = new C1SplitterPanelEx
		{
			Dock = PanelDockStyle.Top,
			KeepRelativeSize = false,
			Height = 31,
			MinHeight = 31,
			Collapsible = false,
			Resizable = false,
			BorderWidth = 1,
			DoubleBuffered = true,
			BackgroundRenderCallback = _navTreeTitlePanelBackground_Paint,
			PaintCallback = _navTreeTitle_Paint
		};
		_navTreeTitlePanel.SizeChanged += _navTreeTitlePanel_SizeChanged;
		_navTreeTitlePanel.MouseClick += _navTreeTitlePanel_MouseClick;
		_navTreeTitlePanel.MouseEnter += _navTreeTitle_MouseEnter;
		_navTreeTitlePanel.MouseLeave += _navTreeTitle_MouseLeave;
		_navTreeTitlePanel.MouseMove += _navTreeTitle_MouseMove;
		_navTreeGridPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			SizeRatio = 100.0
		};
		_navTreeContainer = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_navTreeGridPanel.Controls.Add(TableNavGrid.View);
		_navTreeContainer.Panels.Add(_navTreeTitlePanel);
		_navTreeContainer.Panels.Add(_navTreeGridPanel);
		_grid = new C1FlexGridEx
		{
			AllowDragging = AllowDraggingEnum.None,
			AllowMerging = AllowMergingEnum.Custom,
			AllowResizing = AllowResizingEnum.Both,
			AllowSorting = AllowSortingEnum.None,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom,
			BackColor = Color.Transparent,
			ScrollOptions = ScrollFlags.None,
			AutoClipboard = false
		};
		_grid.BodyAfterScroll += _grid_BodyAfterScroll;
		_grid.LostFocus += _grid_LostFocus;
		_grid.Enter += _grid_Enter;
		_grid.Leave += _grid_Leave;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.Paint += _grid_Paint;
		_grid.BeforeResizeRow += _grid_BeforeResizeRow;
		_grid.BeforeResizeColumn += _grid_BeforeResizeColumn;
		_grid.AfterResizeRow += _grid_AfterResizeRow;
		_grid.BeforeAutosizeRow += _grid_BeforeAutosizeRow;
		_grid.BeforeAutosizeColumn += _grid_BeforeAutosizeColumn;
		_grid.MouseClick += _grid_MouseClick;
		_grid.MouseDown += _grid_MouseDown;
		_grid.BeforeMouseDown += _grid_BeforeMouseDown;
		_grid.MouseUp += _grid_MouseUp;
		_grid.MouseMove += _grid_MouseMove;
		_grid.MouseDoubleClick += _grid_MouseDoubleClick;
		_grid.KeyDown += _grid_KeyDown;
		_grid.BeforeSelChange += _grid_BeforeSelChange;
		_grid.AfterSelChange += _grid_AfterSelChange;
		_grid.BodyAfterRowColChange += _grid_BodyAfterRowColChange;
		_grid.BodySelectionChanged += _grid_BodySelectionChanged;
		_grid.BeforeEdit += _grid_BeforeEdit;
		_grid.BodyBeforeEdit += _grid_BodyBeforeEdit;
		_grid.BodySetupEditor += _grid_BodySetupEditor;
		_grid.BodyStartEdit += _grid_BodyStartEdit;
		_grid.AfterEdit += _grid_AfterEdit;
		_grid.BodyAfterEdit += _grid_BodyAfterEdit;
		_grid.BodyValidateEdit += _grid_BodyValidateEdit;
		_grid.AfterResizeColumn += _grid_AfterResizeColumn;
		_grid.PaintBackground += _grid_PaintBackground;
		_tableFilterContext = new TableFilterContext();
		_grid.FilterManager.Context = _tableFilterContext;
		_timerFormulaHighlight.Tick += _timerFormulaHighlight_Tick;
		_timerWarningHighlight.Tick += _timerWarningHighlight_Tick;
		ListDropDown = new ListDropDown(_grid);
		InputListDropDown = new InputListDropDown(_grid);
		pnlGrid = new C1SplitterPanel
		{
			Name = "pnlGrid",
			Collapsible = false,
			KeepRelativeSize = false,
			SizeRatio = 0.0,
			Resizable = false,
			Dock = PanelDockStyle.Top,
			DoubleBuffered = true
		};
		pnlGrid.Controls.Add(TitleEditor.View);
		pnlGrid.Controls.Add(_grid);
		pnlGrid.Controls.Add(FootEditor.View);
		FootEditor.View.BringToFront();
		pnlGrid.MouseClick += PnlGrid_MouseClick;
		c1SplitContainer2.Panels.Add(pnlGrid);
		c1SplitContainer2.ResumeLayout();
		ToolBar.Horizontal = false;
		ToolBar.Dock = DockStyle.Fill;
		ToolBar.ButtonLookVert = ButtonLookFlags.TextAndImage;
		ToolBar.MinButtonSize = 40;
		RibbonImageProcess imageProcess = MainForm.ImageProcess;
		lnkCollectFill2.Command = cmdCollectFill2;
		cmdCollectFill2.Image = Auditai.UI.Platform.Properties.Resources.GenerateWorkingPaper;
		cmdCollectFill2.CommandStateQuery += CmdCollectFill2_CommandStateQuery;
		cmdCollectFill2.Click += CmdCollectFill2_Click;
		ToolBar.CommandLinks.Add(lnkCollectFill2);
		lnkCalculateTable3.Command = cmdCalculateTable2;
		cmdCalculateTable2.Image = Auditai.UI.Platform.Properties.Resources.CalculateTable;
		cmdCalculateTable2.CommandStateQuery += CmdCalculateTable2_CommandStateQuery;
		cmdCalculateTable2.Click += CmdCalculateTable2_Click;
		lnkCalculateTable3.Delimiter = true;
		ToolBar.CommandLinks.Add(lnkCalculateTable3);
		lnkValidateTable3.Command = cmdValidateTable2;
		cmdValidateTable2.Image = Auditai.UI.Platform.Properties.Resources.ValidateTable;
		cmdValidateTable2.CommandStateQuery += CmdValidateTable2_CommandStateQuery;
		cmdValidateTable2.Click += CmdValidateTable2_Click;
		ToolBar.CommandLinks.Add(lnkValidateTable3);
		lnkLockTable.Command = cmdLockTable;
		cmdLockTable.Image = Auditai.UI.Platform.Properties.Resources.ToggleLockTable;
		cmdLockTable.CommandStateQuery += CmdLockTable_CommandStateQuery;
		cmdLockTable.Click += CmdLockTable_Click;
		ToolBar.CommandLinks.Add(lnkLockTable);
		lnkExportTable.Command = cmdExportTable;
		cmdExportTable.Image = Auditai.UI.Platform.Properties.Resources.ExportExcel;
		cmdExportTable.CommandStateQuery += CmdExportTable_CommandStateQuery;
		cmdExportTable.Click += CmdExportTable_Click;
		ToolBar.CommandLinks.Add(lnkExportTable);
		lnkFoot.Command = cmdFoot;
		cmdFoot.Image = Auditai.UI.Platform.Properties.Resources.TableFoot;
		cmdFoot.CommandStateQuery += CmdFoot_CommandStateQuery;
		cmdFoot.Click += CmdFoot_Click;
		lnkToolbarTables.Delimiter = true;
		lnkToolbarTables.Command = cmdToolbarTables;
		cmdToolbarTables.Image = Auditai.UI.Platform.Properties.Resources.ToolbarTable;
		cmdToolbarTables.Click += CmdToolbarTables_Click;
		cmdToolbarTables.Text = "关联表格";
		cmdToolbarTables.Image = Auditai.UI.Platform.Properties.Resources.ToolbarTable;
		lnkMakerSign.Command = cmdMakerSign;
		cmdMakerSign.CommandStateQuery += CmdMakerSign_CommandStateQuery;
		cmdMakerSign.Click += CmdMakerSign_Click;
		lnkCheckerSign.Command = cmdCheckerSign;
		cmdCheckerSign.CommandStateQuery += CmdCheckerSign_CommandStateQuery;
		cmdCheckerSign.Click += CmdCheckerSign_Click;
		lnkBack.Delimiter = true;
		lnkBack.Command = cmdBack;
		cmdBack.Image = Auditai.UI.Platform.Properties.Resources.back32;
		cmdBack.Click += CmdBack_Click;
		lnkForward.Command = cmdForward;
		cmdForward.Image = Auditai.UI.Platform.Properties.Resources.forward32;
		cmdForward.Click += CmdForward_Click;
		lnkTicketInputMode.Command = cmdTicketInputMode;
		lnkTicketInputMode.Delimiter = true;
		cmdTicketInputMode.Image = Auditai.UI.Platform.Properties.Resources.TicketMode;
		cmdTicketInputMode.Click += CmdTicketInputMode_Click;
		cmdDesignTicket.Click += CmdDesignTicket_Click;
		cmdDesignTicket.CommandStateQuery += CmdDesignTicket_CommandStateQuery;
		lnkDesignTicket.Delimiter = true;
		lnkDesignTicket.Command = cmdDesignTicket;
		ToolBar.CommandLinks.Add(lnkDesignTicket);
		lnkHideToolbar.Delimiter = true;
		lnkHideToolbar.Command = cmdHideToolbar;
		cmdHideToolbar.Image = Auditai.UI.Platform.Properties.Resources.HideSideToolbar;
		cmdHideToolbar.CommandStateQuery += CmdHideToolbar_CommandStateQuery;
		cmdHideToolbar.Click += CmdHideToolbar_Click;
		// 跨项目数据引用工具栏按钮
		lnkTB_CrossProjectDataRef.Delimiter = true;
		lnkTB_CrossProjectDataRef.Command = cmdCrossProjectDataRef;
		ToolBar.CommandLinks.Add(lnkTB_CrossProjectDataRef);
		lnkTB_RefreshCrossProjectRefs.Command = cmdRefreshCrossProjectRefs;
		ToolBar.CommandLinks.Add(lnkTB_RefreshCrossProjectRefs);
		foreach (C1CommandLink commandLink in ToolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		HideValidationPane();
		_grid.FilterManager.Changed += FilterManager_Changed;
		InitializeContextMenu();
		_ttpComment.CloseClick += _ttpComment_CloseClick;
		gridDecorator = new FlexGridDecorator(_grid);
		pnlGrid.Paint += PnlGrid_Paint;
		pnlGrid.Resize += PnlGrid_Resize;
		_timerWarningHighlight.Start();
	}

	private void _navTreeTitle_MouseLeave(object sender, EventArgs e)
	{
		_isMouseOverNavTreePanelMoreMenuIcon = false;
		_isMouseOverNavTreeTitlePanel = false;
		_navTreeTitlePanel.Invalidate();
	}

	private void _navTreeTitle_MouseEnter(object sender, EventArgs e)
	{
		_isMouseOverNavTreePanelMoreMenuIcon = false;
		_isMouseOverNavTreeTitlePanel = true;
		_navTreeTitlePanel.Invalidate();
	}

	private void _navTreeTitle_MouseMove(object sender, MouseEventArgs e)
	{
		bool flag = GetNavTreeTitleMoreMenuIconBackgroundRectangle().Contains(e.X, e.Y);
		if (flag != _isMouseOverNavTreePanelMoreMenuIcon)
		{
			_navTreeTitlePanel.Invalidate();
		}
		_isMouseOverNavTreePanelMoreMenuIcon = flag;
	}

	private bool _navTreeTitle_Paint(object sender, PaintEventArgs e)
	{
		try
		{
			Draw_TitleImage();
			Draw_Title();
			Draw_MoreMenuIcon();
		}
		catch
		{
			return true;
		}
		return true;
		void Draw_MoreMenuIcon()
		{
			if (_isMouseOverNavTreeTitlePanel)
			{
				bool flag = false;
				Bitmap bitmap = null;
				if (Theme.SelectedAuditaiTheme.ThemeContext.OutBarPageMoreMenuImageIndex == OutBarPageMoreMenuImageIndex.White)
				{
					if (_menuMoreOperationWhiteImage == null)
					{
						_menuMoreOperationWhiteImage = (Bitmap)new WhiteImageStrategy().ProcessImage(Auditai.UI.Platform.Properties.Resources.menuMoreOperation);
					}
					bitmap = _menuMoreOperationWhiteImage;
					flag = true;
				}
				else
				{
					bitmap = Auditai.UI.Platform.Properties.Resources.menuMoreOperation;
				}
				if (_isMouseOverNavTreePanelMoreMenuIcon)
				{
					if (flag)
					{
						_brushMoreMenuIconBackground.Color = Auditai.UI.Controls.Util.LightColor(_navTreeTitleBackgroundColor, 0.2);
					}
					else
					{
						_brushMoreMenuIconBackground.Color = Auditai.UI.Controls.Util.DarkenColor(_navTreeTitleBackgroundColor, 0.1);
					}
					e.Graphics.FillRectangle(_brushMoreMenuIconBackground, GetNavTreeTitleMoreMenuIconBackgroundRectangle());
				}
				e.Graphics.DrawImage(bitmap, GetNavTreeTitleMoreMenuIconLeftTopPosition());
			}
		}
		void Draw_Title()
		{
			e.Graphics.DrawString("快捷查询列表", _navTreeTitlePanel.Font, _navTreeTitleBrush, new RectangleF(32f, 0f, 100f, 30f), _navTreeTitleTextFormat);
		}
		void Draw_TitleImage()
		{
			e.Graphics.DrawImage(Auditai.UI.Platform.Properties.Resources.tableNavTreeIcon16, new Point(10, 7));
		}
	}

	private Point GetNavTreeTitleMoreMenuIconLeftTopPosition()
	{
		Rectangle clientRectangle = _navTreeTitlePanel.ClientRectangle;
		int num = 25;
		int x = clientRectangle.X + clientRectangle.Width - num;
		int y = clientRectangle.Y + (clientRectangle.Height - Auditai.UI.Platform.Properties.Resources.menuMoreOperation.Height) / 2;
		return new Point(x, y);
	}

	private Rectangle GetNavTreeTitleMoreMenuIconBackgroundRectangle()
	{
		Point navTreeTitleMoreMenuIconLeftTopPosition = GetNavTreeTitleMoreMenuIconLeftTopPosition();
		int num = 3;
		int num2 = 3;
		return new Rectangle(navTreeTitleMoreMenuIconLeftTopPosition.X - num, navTreeTitleMoreMenuIconLeftTopPosition.Y - num2, Auditai.UI.Platform.Properties.Resources.menuMoreOperation.Width + num * 2, Auditai.UI.Platform.Properties.Resources.menuMoreOperation.Height + num2 * 2);
	}

	private void _navTreeTitlePanel_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			_navTitleCtx.ShowContextMenu(_navTreeTitlePanel, e.Location);
		}
		else if (e.Button == MouseButtons.Left && _isMouseOverNavTreePanelMoreMenuIcon)
		{
			_navTitleCtx.ShowContextMenu(_navTreeTitlePanel, e.Location);
		}
	}

	private void CmdDeleteNavTree_Click(object sender, ClickEventArgs e)
	{
		if (Table == null || TableNavGrid.Table != Table)
		{
			return;
		}
		try
		{
			Table.Title.NavTreeCellIdList = null;
			Table.TagTitleDirty();
			ReBuildNavTree();
		}
		catch (Exception)
		{
		}
	}

	private bool _navTreeTitlePanelBackground_Paint(object sender, PaintEventArgs e)
	{
		if (_navTreeTitlePanelBackgroundBrush != null)
		{
			e.Graphics.FillRectangle(_navTreeTitlePanelBackgroundBrush, _navTreeTitlePanel.ClientRectangle);
			return true;
		}
		return false;
	}

	private void _navTreeTitlePanel_SizeChanged(object sender, EventArgs e)
	{
		SetNavTreeTitlePanelBackgroundBrush();
		_navTreeTitlePanel.Invalidate();
	}

	private void SetNavTreeTitlePanelBackgroundBrush()
	{
		if (_navTreeTitlePanelBackgroundBrush != null)
		{
			_navTreeTitlePanelBackgroundBrush.Dispose();
			_navTreeTitlePanelBackgroundBrush = null;
		}
		C1Theme c1Theme = Theme.SelectedAuditaiTheme.GetC1Theme();
		ThemeBackground background = Theme.SelectedAuditaiTheme.GetBackground("C1Command\\C1OutBar\\Page\\Title\\Default\\Background");
		_navTreeTitlePanelBackgroundBrush = background.GetBrush().GetBrush(_navTreeTitlePanel.ClientRectangle);
		_navTreeTitlePanel.Height = c1Theme.GetInt("C1Command\\C1OutBar\\Page\\Title\\Height");
		_navTreeTitlePanel.ForeColor = c1Theme.GetColor("C1Command\\C1OutBar\\Page\\Title\\Default\\ForeColor");
		_navTreeTitlePanel.BorderColor = c1Theme.GetColor("C1Command\\C1OutBar\\BorderColor");
		_navTreeTitleBrush.Color = _navTreeTitlePanel.ForeColor;
		if (Program.MainForm.ProjectHierarchy != null && Program.MainForm.ProjectHierarchy.View != null)
		{
			_navTreeTitlePanel.Font = Program.MainForm.ProjectHierarchy.View.Font;
		}
	}

	private void TableNavGrid_TreeLeafNodeSelected(object sender, EventArgs e)
	{
		if (Program.MainForm.IsInEditingFormula() || !(sender is TableNavGrid { SelectedTitleCells: { Count: not 0 } selectedTitleCells }))
		{
			return;
		}
		foreach (Tuple<TableTitleCell, object> item in selectedTitleCells)
		{
			if (item != null && item.Item1 != null)
			{
				TableTitleCell uIRenderCellByCellId = Table.Title.GetUIRenderCellByCellId(item.Item1.CellId);
				if (uIRenderCellByCellId == null)
				{
					return;
				}
				TitleEditor.UpdateCellValue(uIRenderCellByCellId, item.Item2);
			}
		}
		CalcCurrentTable();
		if (Program.MainForm.CurrentView == MainFormView.TablePreview)
		{
			Program.MainForm.Preview.Table = Table;
			Program.MainForm.Preview.CreatePaper();
		}
	}

	private void PopulateEditingColumnHeaders()
	{
		_grid.Rows[0].Height = Table.SumHeaderHeight(Table.GetNumCaptionRows());
		_grid.Rows[0].StyleNew.WordWrap = true;
		for (int i = 0; i < Table.Columns.Count; i++)
		{
			Auditai.Model.Column column = Table.Columns[i];
			C1.Win.C1FlexGrid.CellRange cellRange = _grid.BodyGetCell(0, i);
			cellRange.StyleNew.DataType = typeof(string);
			cellRange.Data = column.CaptionDisplay;
			cellRange.StyleNew.BackColor = column.CaptionStyle.BackColor.Value;
			cellRange.StyleNew.ForeColor = column.CaptionStyle.ForeColor.Value;
			cellRange.StyleNew.Font = column.GetCaptionFont();
			cellRange.StyleNew.TextAlign = C1FlexGridEx.ToTextAlign(column.CaptionStyle.Align.Value);
		}
	}

	private void PopulateMerges()
	{
		List<C1.Win.C1FlexGrid.CellRange> list = (from C1.Win.C1FlexGrid.CellRange merge in _grid.MergedRanges
			where merge.TopRow >= _grid.Rows.Fixed && merge.LeftCol >= _grid.Cols.Fixed
			select merge).ToList();
		foreach (C1.Win.C1FlexGrid.CellRange item in list)
		{
			_grid.MergedRanges.Remove(item);
		}
		foreach (CellMerge mergedCell in Table.MergedCells)
		{
			_grid.BodyAddMergedRange(mergedCell.TopLeft.Row.Index, mergedCell.TopLeft.Column.Index, mergedCell.BottomRight.Row.Index, mergedCell.BottomRight.Column.Index);
			for (int i = mergedCell.TopLeft.Row.Index; i <= mergedCell.BottomRight.Row.Index; i++)
			{
				for (int j = mergedCell.TopLeft.Column.Index; j <= mergedCell.BottomRight.Column.Index; j++)
				{
					_grid.BodyGetCell(i, j).StyleNew.DataType = null;
				}
			}
		}
	}

	private void PopulateColumn(Auditai.Model.Column model, C1.Win.C1FlexGrid.Column view)
	{
		view.Width = model.Width;
		view.Visible = model.Visible;
		view.DataType = null;
		view.AllowEditing = CanEditColumn(model);
	}

	public void PopulateRows()
	{
		_grid.BeginUpdate();
		_grid.BodyRowsCount = Table.Rows.Count;
		for (int i = 0; i < Table.Rows.Count; i++)
		{
			PopulateRow(Table.Rows[i], _grid.BodyGetRow(i));
		}
		if (_grid.Cols[0].Width < 56)
		{
			_grid.Cols[0].Width = 56;
		}
		_grid.EndUpdate();
	}

	public void PopulateColumns()
	{
		_isUpdatingView = true;
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		Point scrollPosition = _grid.ScrollPosition;
		_grid.BodyColsCount = Table.Columns.Count;
		for (int i = 0; i < Table.Columns.Count; i++)
		{
			PopulateColumn(Table.Columns[i], _grid.BodyGetCol(i));
		}
		bool flag = FormulaEditor.Context.Kind == FormulaContextKind.ColHeader && !FormulaEditor.IsFinishingEditing && FormulaEditor.Context.Table == Table;
		for (int num = _grid.MergedRanges.Count - 1; num >= 0; num--)
		{
			if (_grid.MergedRanges[num].TopRow < _grid.Rows.Fixed)
			{
				_grid.MergedRanges.RemoveAt(num);
			}
		}
		int num2 = (flag ? 1 : _table.GetNumCaptionRows());
		_grid.Rows.RemoveRange(0, _grid.Rows.Fixed);
		_grid.Rows.InsertRange(0, num2);
		_grid.Rows.Fixed = num2;
		for (int j = 0; j < num2; j++)
		{
			_grid.Rows[j].StyleNew.WordWrap = true;
		}
		if (flag)
		{
			for (int k = 0; k < Table.Columns.Count; k++)
			{
				_grid.SetData(0, k + _grid.Cols.Fixed, Table.Columns[k].CaptionDisplay);
			}
			_grid.Rows[0].Height = Table.SumHeaderHeight(Table.GetNumCaptionRows());
		}
		else
		{
			foreach (Auditai.Model.Column column2 in _table.Columns)
			{
				string[] array = column2.CaptionDisplay.Split('_');
				for (int l = 0; l < array.Length; l++)
				{
					_grid.SetData(l, column2.Index + _grid.Cols.Fixed, array[l]);
				}
			}
			List<Auditai.DTO.CellRange> mergeInfo = _table.GetMergeInfo(visibleOnly: false);
			foreach (Auditai.DTO.CellRange item in mergeInfo)
			{
				_grid.MergedRanges.Add(item.r1, item.c1 + _grid.Cols.Fixed, item.r2, item.c2 + _grid.Cols.Fixed);
			}
			for (int m = 0; m < num2; m++)
			{
				try
				{
					_grid.Rows[m].Height = _table.GetHeaderHeight(m);
				}
				catch (ArgumentOutOfRangeException)
				{
				}
			}
		}
		for (int n = 0; n < Table.Columns.Count; n++)
		{
			Auditai.Model.Column column = Table.Columns[n];
			C1.Win.C1FlexGrid.CellRange cellRange = _grid.GetCellRange(0, n + _grid.Cols.Fixed, _grid.Rows.Fixed - 1, n + _grid.Cols.Fixed);
			cellRange.StyleNew.ForeColor = column.CaptionStyle.ForeColor.Value;
			cellRange.StyleNew.Font = column.GetCaptionFont();
			cellRange.StyleNew.TextAlign = C1FlexGridEx.ToTextAlign(column.CaptionStyle.Align.Value);
		}
		Select((bodySelection.TopRow < _grid.BodyRowsCount) ? bodySelection.TopRow : (_grid.BodyRowsCount - 1), (bodySelection.LeftCol < _grid.BodyColsCount) ? bodySelection.LeftCol : (_grid.BodyColsCount - 1), (bodySelection.BottomRow < _grid.BodyRowsCount) ? bodySelection.BottomRow : (_grid.BodyRowsCount - 1), (bodySelection.RightCol < _grid.BodyColsCount) ? bodySelection.RightCol : (_grid.BodyColsCount - 1));
		_grid.ScrollPosition = scrollPosition;
		_grid.FilterManager.Populate();
		_grid.FilterManager.ResetGridColumnMergeRange();
		_isUpdatingView = false;
		DoLayout();
	}

	private void PopulateRow(Auditai.Model.Row model, C1.Win.C1FlexGrid.Row view)
	{
		view.Height = model.Height;
		view.AllowEditing = !model.IsLocked && CanEditRow(model);
	}

	private bool FindImpl(string src, string findWhat, MatchMode mode, bool caseSensitive)
	{
		StringComparison comparisonType = (caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
		return mode switch
		{
			MatchMode.Exact => src.Equals(findWhat, comparisonType), 
			MatchMode.Start => src.StartsWith(findWhat, comparisonType), 
			MatchMode.End => src.EndsWith(findWhat, comparisonType), 
			MatchMode.Any => src.IndexOf(findWhat, comparisonType) >= 0, 
			_ => throw new ArgumentOutOfRangeException("mode"), 
		};
	}

	private void MutateTableColumnCellStyle<T>(Expression<Func<Auditai.Model.CellStyle, T>> propertySelector, T value, Auditai.Model.Column column)
	{
		PropertyInfo pi = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
		if (CanEditColumn(column))
		{
			bool? oldIsAllowManualInput = column.Style?.Format?.IsAllowEditOnExistFormula;
			column.UpdateStyle(Table.CellStyles.MutateAndGet(column.Style, delegate(Auditai.Model.CellStyle s)
			{
				pi.SetValue(s, value);
				SetStyleFormatIsAllowManualInput(s, oldIsAllowManualInput);
			}));
		}
		static void SetStyleFormatIsAllowManualInput(Auditai.Model.CellStyle cellStyle, bool? isAllowManualInput)
		{
			if (isAllowManualInput.HasValue)
			{
				if (!cellStyle.Format.HasValue)
				{
					cellStyle.Format = new DataFormat(DataFormatType.General)
					{
						IsAllowEditOnExistFormula = isAllowManualInput.Value
					};
				}
				else
				{
					DataFormat value2 = cellStyle.Format.Value.Clone();
					value2.IsAllowEditOnExistFormula = isAllowManualInput.Value;
					cellStyle.Format = value2;
				}
			}
		}
	}

	private void MutateCellStyle<T>(Expression<Func<Auditai.Model.CellStyle, T>> propertySelector, T value, bool updateTableDefaultIfSelectAll = true, bool clearCellStyleIfEntireColumn = true)
	{
		PropertyInfo pi = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
		bool flag = false;
		bool flag2 = true;
		if (_grid.FilterManager.Filters.Count > 0)
		{
			string name = pi.Name;
			flag = true;
			flag2 = false;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (Table.Rows.Skip(bodySelection.TopRow).Take(bodySelection.BottomRow - bodySelection.TopRow + 1).All((Auditai.Model.Row r) => !Table[r.Index, 0].ShouldApplyColumnFormula()))
		{
			foreach (Auditai.Model.Cell item in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
			{
				if ((!flag || IsGridRowVisible(item)) && CanEditRow(item.Row) && CanEditColumn(item.Column))
				{
					item.UpdateStyle(Table.CellStyles.MutateAndGet(item.Style, delegate(Auditai.Model.CellStyle s)
					{
						pi.SetValue(s, value);
					}));
				}
			}
		}
		else if (_grid.IsEntireColumnSelected && flag2)
		{
			if (updateTableDefaultIfSelectAll && _grid.IsEntireRowSelected)
			{
				Table.UpdateDefaultStyle(Table.CellStyles.MutateAndGet(Table.DefaultStyle, delegate(Auditai.Model.CellStyle s)
				{
					pi.SetValue(s, value);
				}));
				foreach (Auditai.Model.Column column2 in Table.Columns)
				{
					bool? oldIsAllowManualInput2 = column2.Style?.Format?.IsAllowEditOnExistFormula;
					column2.UpdateStyle(Table.CellStyles.MutateAndGet(column2.Style, delegate(Auditai.Model.CellStyle s)
					{
						pi.SetValue(s, null);
						SetStyleFormatIsAllowManualInput(s, oldIsAllowManualInput2);
					}));
				}
				foreach (Auditai.Model.Cell cell in Table.Cells)
				{
					if (cell.ShouldApplyColumnFormula())
					{
						cell.UpdateStyle(Table.CellStyles.MutateAndGet(cell.Style, delegate(Auditai.Model.CellStyle s)
						{
							pi.SetValue(s, null);
						}));
					}
				}
			}
			else
			{
				for (int i = bodySelection.LeftCol; i <= bodySelection.RightCol; i++)
				{
					Auditai.Model.Column column = Table.Columns[i];
					if (CanEditColumn(column))
					{
						bool? oldIsAllowManualInput = column.Style?.Format?.IsAllowEditOnExistFormula;
						column.UpdateStyle(Table.CellStyles.MutateAndGet(column.Style, delegate(Auditai.Model.CellStyle s)
						{
							pi.SetValue(s, value);
							SetStyleFormatIsAllowManualInput(s, oldIsAllowManualInput);
						}));
					}
				}
				if (clearCellStyleIfEntireColumn)
				{
					foreach (Auditai.Model.Cell item2 in Table.EnumerateCellRange(0, bodySelection.LeftCol, Table.Rows.Count - 1, bodySelection.RightCol))
					{
						if (item2.ShouldApplyColumnFormula())
						{
							item2.UpdateStyle(Table.CellStyles.MutateAndGet(item2.Style, delegate(Auditai.Model.CellStyle s)
							{
								pi.SetValue(s, null);
							}));
						}
					}
				}
			}
		}
		else
		{
			foreach (Auditai.Model.Cell item3 in Table.EnumerateCellRange(bodySelection.TopRow, bodySelection.LeftCol, bodySelection.BottomRow, bodySelection.RightCol))
			{
				if ((!flag || IsGridRowVisible(item3)) && CanEditRow(item3.Row) && CanEditColumn(item3.Column) && item3.ShouldApplyColumnFormula())
				{
					item3.UpdateStyle(Table.CellStyles.MutateAndGet(item3.Style, delegate(Auditai.Model.CellStyle s)
					{
						pi.SetValue(s, value);
					}));
				}
			}
		}
		_grid.Invalidate();
		static void SetStyleFormatIsAllowManualInput(Auditai.Model.CellStyle cellStyle, bool? isAllowManualInput)
		{
			if (isAllowManualInput.HasValue)
			{
				if (!cellStyle.Format.HasValue)
				{
					cellStyle.Format = new DataFormat(DataFormatType.General)
					{
						IsAllowEditOnExistFormula = isAllowManualInput.Value
					};
				}
				else
				{
					DataFormat value2 = cellStyle.Format.Value.Clone();
					value2.IsAllowEditOnExistFormula = isAllowManualInput.Value;
					cellStyle.Format = value2;
				}
			}
		}
	}

	private bool IsGridRowVisible(Auditai.Model.Cell cell)
	{
		int num = _grid.Rows.Fixed + cell.Row.Index;
		if (num < 0 || num >= _grid.Rows.Count)
		{
			return true;
		}
		return _grid.Rows[num].Visible;
	}

	private void TableStyleBrushImpl(Auditai.Model.Table source, Auditai.Model.Table dst)
	{
		List<Auditai.Model.Cell> list = null;
		Auditai.Model.CellStyle cellStyle = null;
		foreach (Auditai.Model.Row item in dst.Rows.Where((Auditai.Model.Row r) => r.Role == RowRole.Total))
		{
			if (cellStyle == null)
			{
				cellStyle = dst.CellStyles.Get(dst.DefaultStyle, delegate(Auditai.Model.CellStyle cs)
				{
					cs.Align = CellTextAlign.MiddleCenter;
				});
				if (cellStyle == null)
				{
					break;
				}
			}
			int count = dst.Columns.Count;
			for (int i = 0; i < count; i++)
			{
				Auditai.Model.Cell cell = dst[item.Index, i];
				if (!(cell.Formula != "\"合计\"") && cell.Style != null && cell.Style.Equals(cellStyle))
				{
					if (list == null)
					{
						list = new List<Auditai.Model.Cell>();
					}
					list.Add(cell);
				}
			}
		}
		TableTitleCell tableTitleCell = dst.Title.TitleCell.Clone();
		dst.Title.Deserialize(source.Title.Serialize());
		dst.Title.TitleCell.Value = tableTitleCell.Value;
		dst.Title.TitleCell.ComboList = tableTitleCell.ComboList;
		dst.Title.TitleCell.Comment = tableTitleCell.Comment;
		dst.Title.TitleCell.DefaultValue = tableTitleCell.DefaultValue;
		dst.Title.TitleCell.IgnoreComboList = tableTitleCell.IgnoreComboList;
		dst.Title.TitleCell.MultiComboList = tableTitleCell.MultiComboList;
		for (int j = 0; j < source.Title.Rows.Count; j++)
		{
			for (int k = 0; k < source.Title.Rows[j].Cells.Count; k++)
			{
				dst.Title.Rows[j].Cells[k].Value = source.Title.Rows[j].Cells[k].Value;
			}
		}
		dst.TagTitleDirty();
		foreach (Auditai.Model.Column column in dst.Columns)
		{
			column.UpdateCaptionStyle(source.Columns[0].CaptionStyle);
		}
		bool flag = false;
		if (source.DefaultStyle.FontFamily != dst.DefaultStyle.FontFamily)
		{
			flag = true;
			dst.UpdateDefaultStyle(dst.CellStyles.MutateAndGet(dst.DefaultStyle, delegate(Auditai.Model.CellStyle cs)
			{
				cs.FontFamily = source.DefaultStyle.FontFamily;
			}));
		}
		if (source.DefaultStyle.FontSize != dst.DefaultStyle.FontSize)
		{
			flag = true;
			dst.UpdateDefaultStyle(dst.CellStyles.MutateAndGet(dst.DefaultStyle, delegate(Auditai.Model.CellStyle cs)
			{
				cs.FontSize = source.DefaultStyle.FontSize;
			}));
		}
		dst.PageSetup.Deserialize(source.PageSetup.Serialize());
		dst.TagPageSetupDirty();
		dst.UpdateBorderStyle(source.BorderStyle);
		dst.Foot.Deserialize(source.Foot.Serialize());
		for (int l = 0; l < source.Foot.Rows.Count; l++)
		{
			for (int m = 0; m < source.Foot.Rows[l].Cells.Count; m++)
			{
				dst.Foot.Rows[l].Cells[m].Value = source.Foot.Rows[l].Cells[m].Value;
			}
		}
		if (list == null || !flag)
		{
			return;
		}
		foreach (Auditai.Model.Cell item2 in list)
		{
			Auditai.Model.CellStyle style = dst.CellStyles.MutateAndGet(item2.Style, delegate(Auditai.Model.CellStyle cs)
			{
				cs.FontFamily = source.DefaultStyle.FontFamily;
				cs.FontSize = source.DefaultStyle.FontSize;
			});
			item2.UpdateStyle(style);
		}
	}

	private async Task<bool> ValidateConsolidateSettings()
	{
		if (Table.ConsolidateSettings.Sources.Count == 0)
		{
			return false;
		}
		foreach (ConsolidateEntry s in Table.ConsolidateSettings.Sources)
		{
			try
			{
				Auditai.DTO.Project project = (await StorageRouter.GetProjects()).FirstOrDefault(p => p.Id == s.ProjectId);
				s.Project = new Auditai.Model.Project
				{
					Id = s.ProjectId,
					Name = project.Name
				};
				await Syncer.Pull(s.Project);
			}
			catch (HttpRequestException ex)
			{
				if (ex.InnerException is WebException)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "被合并表格所在的" + StringConstBase.Current.Project + "已被删除，请重新设置合并参数。");
				}
				else
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
				}
				return false;
			}
			s.Table = s.Project.GetTableById(s.TableId);
			if (s.Table == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "表格不存在，可能是表格已在云端删除，请重新设置");
				return false;
			}
			try
			{
				await Syncer.Pull(s.Table);
			}
			catch (HttpRequestException ex3)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.InnerException.Message);
				return false;
			}
			s.GroupSrc = new List<Auditai.Model.Column>();
			foreach (Id64 item in s.GroupSrcId)
			{
				Auditai.Model.Column byId = s.Table.Columns.GetById(item);
				if (byId == null)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并维度不存在，可能是列已在云端删除，请重新设置");
					return false;
				}
				s.GroupSrc.Add(byId);
			}
			s.DataSrc = new List<Auditai.Model.Column>();
			foreach (Id64 item2 in s.DataSrcId)
			{
				Auditai.Model.Column byId2 = s.Table.Columns.GetById(item2);
				if (byId2 == null)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "数据列不存在，可能是列已在云端删除，请重新设置");
					return false;
				}
				s.DataSrc.Add(byId2);
			}
		}
		Table.ConsolidateSettings.GroupDest = new List<Auditai.Model.Column>();
		foreach (Id64 item3 in Table.ConsolidateSettings.GroupDestId)
		{
			Auditai.Model.Column byId3 = Table.Columns.GetById(item3);
			if (byId3 == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "目标合并维度不存在");
				return false;
			}
			Table.ConsolidateSettings.GroupDest.Add(byId3);
		}
		Table.ConsolidateSettings.AggregateDest = new List<Auditai.Model.Column>();
		foreach (Id64 item4 in Table.ConsolidateSettings.AggregateDestId)
		{
			Auditai.Model.Column byId4 = Table.Columns.GetById(item4);
			if (byId4 == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "目标合并金额不存在");
				return false;
			}
			Table.ConsolidateSettings.AggregateDest.Add(byId4);
		}
		return true;
	}

	private void MergeRowCellsImpl(int rowIndex, int leftCol, int rightCol)
	{
		foreach (CellMerge item in _table.MergedCells.Where((CellMerge m) => _table.AreMergesConflict(rowIndex, leftCol, rowIndex, rightCol, m.TopLeft.Row.Index, m.TopLeft.Column.Index, m.BottomRight.Row.Index, m.BottomRight.Column.Index)).ToList())
		{
			_table.UnmergeCells(item.TopLeft.Row.Index, item.TopLeft.Column.Index);
		}
		Auditai.Model.Cell cell = Table[rowIndex, leftCol];
		cell.ChangeDataType(typeof(string));
		cell.UpdateStyle(Table.CellStyles.MutateAndGet(cell.Style, delegate(Auditai.Model.CellStyle s)
		{
			s.DataType = typeof(string);
		}));
		_table.MergeCells(rowIndex, leftCol, rowIndex, rightCol);
	}

	private void MergeCellsImpl()
	{
		C1.Win.C1FlexGrid.CellRange sel = _grid.BodySelection;
		foreach (CellMerge item in _table.MergedCells.Where((CellMerge m) => _table.AreMergesConflict(sel.TopRow, sel.LeftCol, sel.BottomRow, sel.RightCol, m.TopLeft.Row.Index, m.TopLeft.Column.Index, m.BottomRight.Row.Index, m.BottomRight.Column.Index)).ToList())
		{
			_table.UnmergeCells(item.TopLeft.Row.Index, item.TopLeft.Column.Index);
		}
		if (_grid.IsEntireRowSelected && sel.TopRow == sel.BottomRow)
		{
			RowRole role = Table.Rows[sel.TopRow].Role;
			if (role == RowRole.Normal || role == RowRole.Among || role == RowRole.Minus)
			{
				Table.Rows[sel.TopRow].UpdateRole(RowRole.Fixed);
			}
		}
		Auditai.Model.Cell cell = Table[sel.TopRow, sel.LeftCol];
		cell.ChangeDataType(typeof(string));
		cell.UpdateStyle(Table.CellStyles.MutateAndGet(cell.Style, delegate(Auditai.Model.CellStyle s)
		{
			s.DataType = typeof(string);
		}));
		_table.MergeCells(sel.TopRow, sel.LeftCol, sel.BottomRow, sel.RightCol);
		_grid.BeginUpdate();
		PopulateMerges();
		_grid.EndUpdate();
	}

	private void ShowReplaceForm(bool replace)
	{
		TableFindInstance tableFindInstance = tableReplaceFactory.Get();
		tableFindInstance.FindNextHandler += Frf_FindNextHandler;
		tableFindInstance.ReplaceHandler += Frf_ReplaceHandler;
		if (replace)
		{
			tableFindInstance.ShowReplace();
		}
		else
		{
			tableFindInstance.ShowFind();
		}
	}

	public bool CanEditRow(Auditai.Model.Row row)
	{
		if (_owner.CurrentProject.Creator.Id == Auditai.Model.User.Current.Id)
		{
			return true;
		}
		if (!_owner.CurrentProject.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id))
		{
			return false;
		}
		if (_owner.CurrentProject.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		if (Table.RowOwnerExclusive)
		{
			if (row.Creator != Auditai.Model.User.Current.Id)
			{
				return Table.RowOwnerLoadShare.Exists(row.Creator, Auditai.Model.User.Current.Id);
			}
			return true;
		}
		if (Table.RowOwnerLoad)
		{
			return true;
		}
		return row.Permissions.CanWrite();
	}

	private bool CanEditCell(Auditai.Model.Cell cell, bool isTableExistFillFormula, bool ignoreProp = false)
	{
		if (!cell.IsEditable)
		{
			return false;
		}
		if (cell.HasColumnFormula() && isTableExistFillFormula)
		{
			return false;
		}
		if (!cell.Column.Permissions.Write.GrantAll && cell.Column.Permissions.CanWrite())
		{
			return true;
		}
		if (!CanEditRow(cell.Row))
		{
			return false;
		}
		if (!CanEditColumn(cell.Column))
		{
			return false;
		}
		if (!cell.Row.Table.IsControlFormulaAllowEditRow(cell.Row))
		{
			return false;
		}
		if (cell.Row.Role == RowRole.Header && !_isSelectingHeaderCell)
		{
			return false;
		}
		if (cell.Column.CrossAttributes.Role != 0)
		{
			return false;
		}
		if (!ignoreProp && Table.CellPropManager.TryGetAttachments(cell, out var _))
		{
			return false;
		}
		return true;
	}

	public static bool IsCurrentUserCanEditColumn(Auditai.Model.Column column)
	{
		if (Auditai.Model.Project.Current.Creator.Id == Auditai.Model.User.Current.Id)
		{
			return true;
		}
		if (!Auditai.Model.Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id))
		{
			return false;
		}
		if (Auditai.Model.Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return column.Permissions.CanWrite();
	}

	public bool CanEditColumn(Auditai.Model.Column column)
	{
		if (Auditai.Model.Project.Current.Creator.Id == Auditai.Model.User.Current.Id)
		{
			return true;
		}
		if (!Auditai.Model.Project.Current.Users.Any((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id))
		{
			return false;
		}
		if (Auditai.Model.Project.Current.Users.First((KeyValuePair<Auditai.DTO.User, UserRole> kv) => kv.Key.Id == Auditai.Model.User.Current.Id).Value == UserRole.Manager)
		{
			return true;
		}
		return column.Permissions.CanWrite();
	}

	private void ExecuteConsolidateImpl(bool showDataCols)
	{
		_grid.BeginUpdate();

		if (Table.ConsolidateSettings.Mode == MergeMode.Append)
		{
			ExecuteConsolidateAppend(showDataCols);
			_grid.EndUpdate();
			return;
		}

		List<ConsolidateEntry> list3 = Table.ConsolidateSettings.Sources.Where((ConsolidateEntry s) => s.Selected).OrderBy((ConsolidateEntry s) => s.Level).ToList();
		for (int num = Table.Columns.Count - 1; num >= 0; num--)
		{
			Auditai.Model.Column column = Table.Columns[num];
			ConsolidateAttributes ca = column.ConsolidateAttributes;
			if (ca != null)
			{
				switch (ca.Role)
				{
				case ConsolidateRole.GroupBy:
					if (!Table.ConsolidateSettings.GroupDest.Contains(column))
					{
						column.UpdateConsolidateAttribs(null);
					}
					break;
				case ConsolidateRole.Data:
					if (!list3.Any((ConsolidateEntry src) => src.ProjectId == ca.ProjectId && src.TableId == ca.TableId && src.DataSrcId.Any((Id64 ds) => ds == ca.ColumnId)))
					{
						Table.Columns.Remove(num, 1);
					}
					break;
				case ConsolidateRole.Aggregate:
				if (!Table.ConsolidateSettings.AggregateDest.Contains(column))
				{
					column.UpdateConsolidateAttribs(null);
				}
				break;
			case ConsolidateRole.Intercompany:
				if (!list3.Any((ConsolidateEntry src) => src.ProjectId == ca.ProjectId && src.TableId == ca.TableId && src.IntercompanyCols.Any((Id64 ds) => ds == ca.ColumnId)))
				{
					column.UpdateConsolidateAttribs(null);
				}
				break;
			}
			}
		}
		List<string> list4 = Table.ConsolidateSettings.AggregateDest.Select((Auditai.Model.Column c) => c.CaptionDisplay).ToList();
		int count = Table.ConsolidateSettings.AggregateDest.Count;
		List<Auditai.Model.Column> list5 = new List<Auditai.Model.Column>();
		for (int l = 0; l < list3.Count; l++)
		{
			ConsolidateEntry s2 = list3[l];
			int j;
			for (j = 0; j < count; j++)
			{
				Auditai.Model.Column column2 = Table.Columns.FirstOrDefault((Auditai.Model.Column c) => c.ConsolidateAttributes != null && c.ConsolidateAttributes.Role == ConsolidateRole.Data && c.ConsolidateAttributes.ProjectId == s2.ProjectId && c.ConsolidateAttributes.ColumnId == s2.DataSrcId[j]);
				Auditai.Model.Column column3 = Table.ConsolidateSettings.AggregateDest[j];
				int index = column3.Index;
				if (column2 == null)
				{
					Table.Columns.Insert(index, 1);
					column2 = Table.Columns[index];
					string text = list4[j] + "_" + s2.Project.Name;
					if (list3.Count((ConsolidateEntry ce) => ce.ProjectId == s2.ProjectId) > 1)
					{
						text = text + " " + s2.Table.TreeNode.Name;
					}
					column2.UpdateCaption(text);
					column2.UpdateStyle(column3.Style);
					column2.UpdateConsolidateAttribs(new ConsolidateAttributes
					{
						Role = ConsolidateRole.Data,
						ProjectId = s2.ProjectId,
						TableId = s2.TableId,
						ColumnId = s2.DataSrcId[j]
					});
					list5.Add(column2);
				}
				else
				{
					Table.Columns.Move(column2.Index, 1, index - 1);
					column2.UpdateStyle(column3.Style);
					list5.Add(column2);
				}
			}
		}
		for (int m = 0; m < Table.ConsolidateSettings.AggregateDest.Count; m++)
		{
			Auditai.Model.Column column4 = Table.ConsolidateSettings.AggregateDest[m];
			if (column4.ConsolidateAttributes == null || column4.ConsolidateAttributes.Role != ConsolidateRole.Aggregate)
			{
				column4.UpdateConsolidateAttribs(new ConsolidateAttributes
				{
					Role = ConsolidateRole.Aggregate
				});
			}
		}
		int count2 = Table.ConsolidateSettings.GroupDest.Count;
		for (int n = 0; n < count2; n++)
		{
			Auditai.Model.Column column5 = Table.ConsolidateSettings.GroupDest[n];
			if (column5.ConsolidateAttributes == null || column5.ConsolidateAttributes.Role != ConsolidateRole.GroupBy)
			{
				column5.UpdateConsolidateAttribs(new ConsolidateAttributes
				{
					Role = ConsolidateRole.GroupBy
				});
			}
		}
		int count3 = list3.Count;
		List<Dictionary<List<object>, List<object>>> list6 = list3.Select((ConsolidateEntry src) => {
			var icSet = new HashSet<int>(src.DataSrc.Select((Auditai.Model.Column col, int idx) => new { col, idx }).Where(x => src.IntercompanyCols.Contains(x.col.Id)).Select(x => x.idx));
			return src.Table.Rows.Where((Auditai.Model.Row r) => r.Role == RowRole.Normal).GroupBy((Auditai.Model.Row row1) => (from c in row1.GetCells()
				where src.GroupSrc.Contains(c.Column)
				select c.Value).ToList(), (Auditai.Model.Row row1) => (from c in row1.GetCells()
				where src.DataSrc.Contains(c.Column)
				select c.Value).ToList(), (List<object> key, IEnumerable<List<object>> rows) => new
			{
				key = key,
				data = rows.Aggregate((List<object> list1, List<object> list2) => Enumerable.Range(0, Math.Min(list1.Count, list2.Count)).Select((int i) => icSet.Contains(i) ? (object)0.0 : (object)(Auditai.Model.Cell.ToDoubleOr0(list1[i]) + Auditai.Model.Cell.ToDoubleOr0(list2[i]))).ToList())
			}, SequenceEqualsComparer<List<object>, object>.Instance).ToDictionary(tup => tup.key, tup => tup.data, SequenceEqualsComparer<List<object>, object>.Instance);
		}).ToList();
		List<IGrouping<List<object>, List<object>>> keys = list6.SelectMany((Dictionary<List<object>, List<object>> d) => d.Keys).GroupBy((List<object> k) => k, SequenceEqualsComparer<List<object>, object>.Instance).ToList();
		List<Tuple<Auditai.Model.Row, List<object>>> list7 = (from r in Table.Rows
			where r.Role == RowRole.Normal || r.Role == RowRole.Among || r.Role == RowRole.Minus
			select Tuple.Create(r, Table.ConsolidateSettings.GroupDest.Select((Auditai.Model.Column c) => Table[r.Index, c.Index].Value).ToList())).ToList();
		list7.Reverse();
		foreach (Tuple<Auditai.Model.Row, List<object>> row2 in list7)
		{
			if (!keys.Any((IGrouping<List<object>, List<object>> g) => g.Key.SequenceEqual(row2.Item2)))
			{
				row2.Item1.Remove();
			}
		}
		Auditai.Model.Row row3 = Table.Rows.FirstOrDefault((Auditai.Model.Row r) => r.Role == RowRole.Subtotal || r.Role == RowRole.Total);
		if (row3 != null && row3.Index == 0 && row3.IsLocked)
		{
			row3 = null;
		}
		int num2 = row3?.Index ?? Table.Rows.Count;
		Auditai.Model.Row[] array = new Auditai.Model.Row[keys.Count];
		int i;
		for (i = 0; i < keys.Count; i++)
		{
			Tuple<Auditai.Model.Row, List<object>> tuple = list7.FirstOrDefault((Tuple<Auditai.Model.Row, List<object>> tup) => tup.Item2.SequenceEqual(keys[i].Key));
			if (tuple == null)
			{
				Table.Rows.Insert(num2, 1);
				array[i] = Table.Rows[num2];
				num2++;
			}
			else
			{
				array[i] = tuple.Item1;
			}
		}
		int count4 = keys.Count;
		if (count4 > Table.Rows.Count)
		{
			Table.Rows.Insert(num2, count4 - Table.Rows.Count);
		}
		int minorityStartRow = num2;
		int minorityCount = 0;
		for (int miSrc = 0; miSrc < count3; miSrc++)
		{
			if (list3[miSrc].OwnershipRatio < 100m)
			{
				for (int miKey = 0; miKey < keys.Count; miKey++)
				{
					Table.Rows.Insert(num2, 1);
					num2++;
					minorityCount++;
				}
			}
		}
		BeginBatchUpdateValue();
		for (int num3 = 0; num3 < keys.Count; num3++)
		{
			int index2 = array[num3].Index;
			for (int num4 = 0; num4 < keys[num3].Key.Count; num4++)
			{
				Table[index2, Table.ConsolidateSettings.GroupDest[num4].Index].UpdateValue(keys[num3].Key[num4]);
			}
			for (int num5 = 0; num5 < count3; num5++)
			{
				List<object> value;
				bool flag = list6[num5].TryGetValue(keys[num3].Key, out value);
				for (int num6 = 0; num6 < count; num6++)
				{
					Table[index2, list5[num5 * count + num6].Index].UpdateValue(flag ? value[num6] : string.Empty);
				}
			}
			for (int num7 = 0; num7 < count; num7++)
			{
				double num8 = 0.0;
				for (int num9 = 0; num9 < count3; num9++)
				{
					if (list6[num9].TryGetValue(keys[num3].Key, out var value2))
					{
						num8 += Auditai.Model.Cell.ToDoubleOr0(value2[num7]) * (double)(list3[num9].OwnershipRatio / 100m);
					}
				}
				Table[index2, Table.ConsolidateSettings.AggregateDest[num7].Index].UpdateValue(num8);
			}
			int miOffset = 0;
			for (int miSrc = 0; miSrc < count3; miSrc++)
			{
				if (list3[miSrc].OwnershipRatio >= 100m)
					continue;
				double minorityRatio = 1.0 - (double)(list3[miSrc].OwnershipRatio / 100m);
				List<object> srcData;
				bool hasData = list6[miSrc].TryGetValue(keys[num3].Key, out srcData);
				int miRowIdx = minorityStartRow + miOffset + num3;
				if (miRowIdx < Table.Rows.Count)
				{
					for (int g = 0; g < count2; g++)
					{
						Table[miRowIdx, Table.ConsolidateSettings.GroupDest[g].Index].UpdateValue(g == 0 ? "少数股东权益" : string.Empty);
					}
					for (int col = 0; col < count; col++)
					{
						double val = hasData ? Auditai.Model.Cell.ToDoubleOr0(srcData[col]) * minorityRatio : 0.0;
						Table[miRowIdx, Table.ConsolidateSettings.AggregateDest[col].Index].UpdateValue(val);
					}
				}
				miOffset += keys.Count;
			}
		}
		EndBatchUpdateValue();
		foreach (Auditai.Model.Column item in list5)
		{
			item.UpdateVisible(showDataCols && Table.ConsolidateSettings.ShowDetail);
		}
		PopulateColumns();
		PopulateRows();
		_grid.EndUpdate();
	}

	/// <summary>
	/// 追加模式：将来源项目的行直接追加到当前表，不合并
	/// 例如项目A有2行、项目B有1行，结果为3行
	/// </summary>
	private void ExecuteConsolidateAppend(bool showDataCols)
	{
		var selectedSources = Table.ConsolidateSettings.Sources.Where(s => s.Selected).ToList();
		if (selectedSources.Count == 0)
		{
			return;
		}

		var groupDest = Table.ConsolidateSettings.GroupDest;
		var aggregateDest = Table.ConsolidateSettings.AggregateDest;

		foreach (var col in groupDest)
		{
			if (col.ConsolidateAttributes == null || col.ConsolidateAttributes.Role != ConsolidateRole.GroupBy)
			{
				col.UpdateConsolidateAttribs(new ConsolidateAttributes { Role = ConsolidateRole.GroupBy });
			}
		}

		foreach (var col in aggregateDest)
		{
			if (col.ConsolidateAttributes == null || col.ConsolidateAttributes.Role != ConsolidateRole.Aggregate)
			{
				col.UpdateConsolidateAttribs(new ConsolidateAttributes { Role = ConsolidateRole.Aggregate });
			}
		}

		var allRows = new List<Tuple<ConsolidateEntry, List<Auditai.Model.Row>>>();
		foreach (var source in selectedSources)
		{
			var sourceRows = source.Table.Rows
				.Where(r => r.Role == RowRole.Normal)
				.ToList();
			allRows.Add(Tuple.Create(source, sourceRows));
		}

		var insertPosition = Table.Rows.Count;
		var totalRow = Table.Rows.FirstOrDefault(r => r.Role == RowRole.Subtotal || r.Role == RowRole.Total);
		if (totalRow != null && totalRow.Index > 0)
		{
			insertPosition = totalRow.Index;
		}

		foreach (var tuple in allRows)
		{
			var source = tuple.Item1;
			var rows = tuple.Item2;
			foreach (var sourceRow in rows)
			{
				Table.Rows.Insert(insertPosition, 1);
				var newRow = Table.Rows[insertPosition];
				insertPosition++;

				for (int j = 0; j < groupDest.Count && j < source.GroupSrc.Count; j++)
				{
					var srcCol = source.Table.Columns.FirstOrDefault(c => c.Id == source.GroupSrcId[j]);
					if (srcCol != null)
					{
						var cellValue = sourceRow.GetCells().FirstOrDefault(c => c.Column == srcCol)?.Value;
						Table[newRow.Index, groupDest[j].Index].UpdateValue(cellValue ?? string.Empty);
					}
				}

				for (int j = 0; j < aggregateDest.Count && j < source.DataSrc.Count; j++)
				{
					if (source.IntercompanyCols.Contains(source.DataSrcId[j])) continue;
					var srcCol = source.Table.Columns.FirstOrDefault(c => c.Id == source.DataSrcId[j]);
					if (srcCol != null)
					{
						var cellValue = sourceRow.GetCells().FirstOrDefault(c => c.Column == srcCol)?.Value;
						if (source.OwnershipRatio < 100m)
						{
							cellValue = Auditai.Model.Cell.ToDoubleOr0(cellValue) * (double)(source.OwnershipRatio / 100m);
						}
						Table[newRow.Index, aggregateDest[j].Index].UpdateValue(cellValue ?? string.Empty);
					}
				}
			}
		}

		PopulateColumns();
		PopulateRows();
		if (Table.ConsolidateSettings != null)
		{
			foreach (Auditai.Model.Column col in Table.Columns)
			{
				if (col.ConsolidateAttributes != null && col.ConsolidateAttributes.Role == ConsolidateRole.Data)
				{
					col.UpdateVisible(showDataCols && Table.ConsolidateSettings.ShowDetail);
				}
			}
		}
	}

	private void ShowCommentTooltip()
	{
		try
		{
			ShowCommentTooltipImpl();
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void ShowCommentTooltipImpl()
	{
		if (IsNotShowComment)
		{
			return;
		}
		_ttpComment.Hide();
		_ttpComment.LinkClicked -= _ttpComment_LinkClicked;
		if (_owner.CurrentView != MainFormView.Table || _isUpdatingView || !_owner.ShowHelperTooltip || _isEditingHeaders || FormulaEditor.IsEditing || ValidationEditor.IsEditing || AuxEditor.IsEditing || LedgerCollectFormulaEditor.IsEditing || (!_grid.Selection.IsSingleCell && !_grid.MergedRanges.Contains(_grid.Selection)) || _grid.BodyRow < 0 || _grid.BodyCol < 0 || _grid.BodyRow >= Table.Rows.Count || _grid.BodyCol >= Table.Columns.Count)
		{
			return;
		}
		Rectangle cellRect = _grid.GetCellRect(_grid.Row, _grid.Col);
		if (cellRect.Width == 0 || cellRect.Height == 0)
		{
			return;
		}
		Auditai.Model.Cell cell = Table[_grid.BodyRow, _grid.BodyCol];
		if (cell == null) return;
		Auditai.Model.Column column = cell.Column;
		XElement xBody = new XElement("div");
		DataReferenceEvaluationContext drec = new DataReferenceEvaluationContext
		{
			CurrentTreeNode = Table.TreeNode,
			Project = Table.Project
		};
		Dictionary<string, object> linkDic = new Dictionary<string, object>();
		bool flag = false;
		if (!CanEditCell(cell, IsTableExistFillFormula()))
		{
			xBody.Add(new XElement("b", "权限提示"));
			Auditai.Model.Row row = cell.Row;
			if (!CanEditRow(row))
			{
				flag = true;
				if (Table.RowOwnerExclusive)
				{
					string text = MemberManager.GetInstance().GetMember(row.Creator.ToString())?.Name ?? "其他成员";
					xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "当前表格设置了增行独占编辑保护，选中的单元格所在行由" + text + "增加并独占，您无权进行编辑修改。"));
				}
				else
				{
					xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "选中的单元格所在行被" + StringConstBase.Current.Manager + "限定了编辑权限，您无权进行编辑修改。"));
				}
			}
			if (!CanEditColumn(cell.Column))
			{
				flag = true;
				xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "选中的单元格所在列被" + StringConstBase.Current.Manager + "限定了编辑权限，您无权进行编辑修改。"));
			}
			if (flag)
			{
				xBody.Add(new XElement("hr"));
			}
			else
			{
				xBody.LastNode.Remove();
			}
		}
		bool flag2 = false;
		string text2 = cell.Style?.Comment;
		bool flag3 = !string.IsNullOrWhiteSpace(text2);
		if (flag3)
		{
			xBody.Add(new XElement("b", "编辑注释"));
			xBody.Add(from s in text2.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None)
				select new XElement("p", new XAttribute("style", "color:red"), Table.Project.DataReferenceManager.ReplaceString(s, drec)));
			xBody.Add(new XElement("hr"));
		}
		else
		{
			text2 = column.Style?.Comment;
			flag3 = !string.IsNullOrWhiteSpace(text2);
			if (flag3)
			{
				xBody.Add(new XElement("b", "编辑注释"));
				xBody.Add(from s in text2.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None)
					select new XElement("p", new XAttribute("style", "color:red"), Table.Project.DataReferenceManager.ReplaceString(s, drec)));
				xBody.Add(new XElement("hr"));
			}
		}
		string displayValue = cell.GetDisplayValue();
		List<NodeNumberInfo> list = new List<NodeNumberInfo>();
		int i = 0;
		string[] array = displayValue.Split(new string[3] { "|", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string t2 in array)
		{
			TreeNodeBase treeNodeBase = Table.Project.GetAllTreeNodes().FirstOrDefault((TreeNodeBase n) => n.Number == t2);
			if (treeNodeBase != null)
			{
				NodeNumberInfo nodeNumberInfo = new NodeNumberInfo
				{
					Id = i,
					Number = treeNodeBase.Number,
					Name = treeNodeBase.Name,
					Node = treeNodeBase
				};
				list.Add(nodeNumberInfo);
				linkDic.Add($"n{i}", nodeNumberInfo);
				i++;
			}
		}
		string trimmedTableName = CollectorManager.formatParse(displayValue);
		List<TreeTableNode> list2 = null;
		list2 = ((!(cell.DisplayDataType != typeof(string)) && !string.IsNullOrWhiteSpace(trimmedTableName) && trimmedTableName.Length > 1) ? (from t in Table.Project.GetAllTableNodes()
			where t.Name.Contains(trimmedTableName)
			select t).ToList() : new List<TreeTableNode>());
		bool flag4 = list.Count > 0 || list2.Count > 0;
		if (flag4)
		{
			xBody.Add(new XElement("b", "追踪文件"));
			xBody.Add(list.Select((NodeNumberInfo ni) => new XElement("p", new XElement("a", new XAttribute("href", $"n{ni.Id}"), ni.Number + " " + ni.Name))));
			foreach (TreeTableNode item in list2)
			{
				xBody.Add(new XElement("p", new XElement("a", new XAttribute("href", $"n{i}")), item.Number + " " + item.Name));
				linkDic.Add($"n{i}", item);
				i++;
			}
			xBody.Add(new XElement("hr"));
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(FormulaContext.Project);
		i = 0;
		bool flag5 = cell.HasFormula;
		if (flag5)
		{
			try
			{
				if (cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula)
				{
					flag5 = false;
				}
				else
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
					formulaEvaluator.Env = new FormulaEvaluationEnvironment
					{
						Resolver = resolver,
						RowIndex = _grid.BodyRow,
						HostTable = Table,
						RefManager = Table.Project.DataReferenceManager,
						RefEvalContext = new DataReferenceEvaluationContext
						{
							Project = Table.Project,
							CurrentTreeNode = Table.TreeNode
						}
					};
					Tuple<List<TooltipListener.FormulaTooltipSegment>, string> formulaTooltipSegments = formulaEvaluator.GetFormulaTooltipSegments(resolver, Table);
					xBody.Add(new XElement("b", "单元格运算公式"));
					XElement xElement = new XElement("p");
					foreach (TooltipListener.FormulaTooltipSegment item2 in formulaTooltipSegments.Item1)
					{
						xElement.Add(item2.PreText);
						xElement.Add(new XElement("a", new XAttribute("href", item2.AnchorNumber), item2.Display));
						linkDic.Add(item2.AnchorNumber.ToString(), item2.Ref);
					}
					xElement.Add(formulaTooltipSegments.Item2);
					xBody.Add(xElement);
					xBody.Add(new XElement("hr"));
					CellsOperand cellsOperand = formulaEvaluator.CondExprToFilter();
					if (cellsOperand != null)
					{
						xBody.Add(new XElement("b", "追踪数据来源"));
						xBody.Add(new XElement("p", new XElement("a", new XAttribute("href", "CondFilter"), "点击此处查看数据来源")));
						linkDic.Add("CondFilter", cellsOperand);
						xBody.Add(new XElement("hr"));
					}
					// 添加操作链接表格（刷新 + 追踪）
					XElement opTable = new XElement("table");
					XElement opRow1 = new XElement("tr");
					opRow1.Add(new XElement("td", new XElement("a", new XAttribute("href", "refresh"), "刷新运算结果")));
					linkDic.Add("refresh", new Tuple<Auditai.Model.Table, int, int>(Table, _grid.BodyRow, _grid.BodyCol));
					opRow1.Add(new XElement("td", new XElement("a", new XAttribute("href", $"tableTrace_{_grid.BodyRow}_{_grid.BodyCol}"), "追踪数据")));
					linkDic.Add($"tableTrace_{_grid.BodyRow}_{_grid.BodyCol}", new Tuple<Auditai.Model.Table, int, int>(Table, _grid.BodyRow, _grid.BodyCol));
					opTable.Add(opRow1);
					xBody.Add(opTable);
					xBody.Add(new XElement("hr"));
				}
			}
			catch (FormulaException)
			{
				flag5 = false;
			}
		}
		else
		{
			flag5 = cell.HasColumnFormula();
			if (flag5)
			{
				try
				{
					if (cell.IsExistManualInputValue && cell.IsAllowManualInputOnFormula)
					{
						flag5 = false;
					}
					else
					{
						FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(column.Formula);
						formulaEvaluator2.Env = new FormulaEvaluationEnvironment
						{
							Resolver = resolver,
							RowIndex = _grid.BodyRow,
							HostTable = Table,
							RefManager = Table.Project.DataReferenceManager,
							RefEvalContext = new DataReferenceEvaluationContext
							{
								Project = Table.Project,
								CurrentTreeNode = Table.TreeNode
							}
						};
						Tuple<List<TooltipListener.FormulaTooltipSegment>, string> formulaTooltipSegments2 = formulaEvaluator2.GetFormulaTooltipSegments(resolver, Table);
						xBody.Add(new XElement("b", "列运算公式"));
						XElement xElement2 = new XElement("p");
						foreach (TooltipListener.FormulaTooltipSegment item3 in formulaTooltipSegments2.Item1)
						{
							xElement2.Add(item3.PreText);
							xElement2.Add(new XElement("a", new XAttribute("href", item3.AnchorNumber), item3.Display));
							linkDic.Add(item3.AnchorNumber.ToString(), item3.Ref);
						}
						xElement2.Add(formulaTooltipSegments2.Item2);
						xBody.Add(xElement2);
						xBody.Add(new XElement("hr"));
						CellsOperand cellsOperand2 = formulaEvaluator2.CondExprToFilter();
						if (cellsOperand2 != null)
						{
							xBody.Add(new XElement("b", "追踪数据来源"));
							xBody.Add(new XElement("p", new XElement("a", new XAttribute("href", "CondFilter"), "点击此处查看数据来源")));
							linkDic.Add("CondFilter", cellsOperand2);
							xBody.Add(new XElement("hr"));
						}
					}
				}
				catch (FormulaException)
				{
					flag5 = false;
				}
			}
			else
			{
				flag5 = cell.TryGetHeaderCellFormulaCell(out var headerCell);
				if (flag5)
				{
					try
					{
						FormulaEvaluator formulaEvaluator3 = new FormulaEvaluator(headerCell.HeaderFormula);
						formulaEvaluator3.Env = new FormulaEvaluationEnvironment
						{
							Resolver = resolver,
							RowIndex = _grid.BodyRow,
							HostTable = Table,
							RefManager = Table.Project.DataReferenceManager,
							RefEvalContext = new DataReferenceEvaluationContext
							{
								Project = Table.Project,
								CurrentTreeNode = Table.TreeNode
							}
						};
						Tuple<List<TooltipListener.FormulaTooltipSegment>, string> formulaTooltipSegments3 = formulaEvaluator3.GetFormulaTooltipSegments(resolver, Table);
						xBody.Add(new XElement("b", "列运算公式"));
						XElement xElement3 = new XElement("p");
						foreach (TooltipListener.FormulaTooltipSegment item4 in formulaTooltipSegments3.Item1)
						{
							xElement3.Add(item4.PreText);
							xElement3.Add(new XElement("a", new XAttribute("href", item4.AnchorNumber), item4.Display));
							linkDic.Add(item4.AnchorNumber.ToString(), item4.Ref);
						}
						xElement3.Add(formulaTooltipSegments3.Item2);
						xBody.Add(xElement3);
						xBody.Add(new XElement("hr"));
						CellsOperand cellsOperand3 = formulaEvaluator3.CondExprToFilter();
						if (cellsOperand3 != null)
						{
							xBody.Add(new XElement("b", "追踪数据来源"));
							xBody.Add(new XElement("p", new XElement("a", new XAttribute("href", "CondFilter"), "点击此处查看数据来源")));
							linkDic.Add("CondFilter", cellsOperand3);
							xBody.Add(new XElement("hr"));
						}
					}
					catch (FormulaException)
					{
						flag5 = false;
					}
				}
			}
		}
		TableValidationInfo value;
		bool flag6 = Program.MainForm.TableValidationResults.TryGetValue(Table.TreeNode, out value);
		if (flag6)
		{
			IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> enumerable = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.Cells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1 == Table[_grid.BodyRow, _grid.BodyCol]).Distinct(new ValidateResultEqualityComparer());
			IEnumerable<Tuple<RangeOperand, ValidationResult>> enumerable2 = ((IEnumerable<Tuple<RangeOperand, ValidationResult>>)value.Ranges).Where((Tuple<RangeOperand, ValidationResult> t) => t.Item1.TopLeft.Row.Index <= _grid.BodyRow && t.Item1.TopLeft.Column.Index <= _grid.BodyCol && _grid.BodyRow <= t.Item1.BottomRight.Row.Index && _grid.BodyCol <= t.Item1.BottomRight.Column.Index);
			IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>> enumerable3 = ((IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>>)value.Columns).Where((Tuple<Auditai.Model.Column, ValidationResult> t) => t.Item1.Index == _grid.BodyCol);
			IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> enumerable4 = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.HeaderCells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1.Column.Index == _grid.BodyCol && t.Item1.Row.Index < _grid.BodyRow && t.Item1.GetHeaderLastRow() >= _grid.BodyRow);
			flag6 = enumerable.Any() || enumerable2.Any() || enumerable3.Any() || enumerable4.Any();
			if (flag6)
			{
				xBody.Add(new XElement("b", "校验公式"));
				try
				{
					i = 0;
					foreach (Tuple<Auditai.Model.Cell, ValidationResult> item5 in enumerable)
					{
						AddValidationResult(item5.Item2, "c");
						i++;
					}
					i = 0;
					foreach (Tuple<RangeOperand, ValidationResult> item6 in enumerable2)
					{
						AddValidationResult(item6.Item2, "r");
						i++;
					}
					i = 0;
					foreach (Tuple<Auditai.Model.Column, ValidationResult> item7 in enumerable3)
					{
						AddValidationResult(item7.Item2, "l");
						i++;
					}
					i = 0;
					foreach (Tuple<Auditai.Model.Cell, ValidationResult> item8 in enumerable4)
					{
						AddValidationResult(item8.Item2, "h");
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
		bool flag7 = false;
		ConsolidateAttributes consolidateAttributes = Table.Columns[_grid.BodyCol].ConsolidateAttributes;
		if (consolidateAttributes != null && consolidateAttributes.Role == ConsolidateRole.Data)
		{
			flag7 = true;
			xBody.Add(new XElement("b", "追踪表格"));
			xBody.Add(new XElement("p", new XElement("a", new XAttribute("href", "ca"), "点击追踪合并报表数据源")));
			linkDic.Add("ca", consolidateAttributes);
			xBody.Add(new XElement("hr"));
		}
		bool flag8 = false;
		if (!string.IsNullOrWhiteSpace(cell.CollectSource))
		{
			if (_owner.IsLedgerEmpty())
			{
				flag8 = true;
				xBody.Add(new XElement("b", "单元格采账设置"));
				xBody.Add(new XElement("p", "本单元格含有采账设置，但您未打开账套，无法显示采账信息。请打开账套后重试。"));
				xBody.Add(new XElement("hr"));
			}
			else
			{
				Ledger ledger = _owner.CurrentLedgerViewer.Ledger;
				Tuple<DateTime, DateTime> auditYear = DictionarySync.GetAuditYear(Table);
				if (auditYear != null)
				{
					int year = auditYear.Item1.Year;
					flag8 = true;
					xBody.Add(new XElement("b", "单元格采账设置"));
					CollectManager collectManager = CollectManager.Parse(cell.CollectSource);
					switch (collectManager.CollectObject)
					{
					case CollectObjectEnum.Balance:
						xBody.Add(new XElement("p", "采集对象：科目余额表 ", new XElement("a", new XAttribute("href", "cellCollect"), "重新设置采账关系")));
						xBody.Add(new XElement("p", $"会计期间：{collectManager.CollectItems[0].StartTime:%M}月-{collectManager.CollectItems[0].EndTime:%M}月"));
						try
						{
							StringBuilder results = new StringBuilder();
							xBody.Add(new XElement("p", "采账关系：", new XElement("table", new XAttribute("cellspacing", "5"), collectManager.CollectItems.Select(delegate(CollectItem ci)
							{
								BalanceItem balanceItem = (BalanceItem)ci;
								decimal value3 = balanceItem.GetValue(ledger, year);
								if ((balanceItem.Operation == OperateEnum.Add && value3 >= 0m) || (balanceItem.Operation == OperateEnum.Subtract && value3 < 0m))
								{
									results.Append("+");
								}
								else
								{
									results.Append("-");
								}
								results.Append(Math.Abs(value3).ToString("N"));
								return new XElement("tr", new XElement("td", OperateEnumToString(balanceItem.Operation)), new XElement("td", balanceItem.AccountName), new XElement("td", AmountEnumToString(balanceItem.AmountEnum)), new XElement("td", new XAttribute("style", "text-align:right"), value3.ToString("N")));
							}))));
							if (results[0] == '+')
							{
								results.Remove(0, 1);
							}
							xBody.Add(new XElement("p", $"采账结果：{collectManager.GetValue(ledger, year).GetValueOrDefault():N}"));
						}
						catch (InvalidAuditYearException ex7)
						{
							xBody.Add(new XElement("p", new XAttribute("style", "color:red"), ex7.Message));
						}
						catch (InvalidCollectSettingException)
						{
							xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "采账设置有误"));
						}
						catch (UnExpectAuditYearException)
						{
							xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "审计年度与采账设置不符"));
						}
						break;
					case CollectObjectEnum.Subsidiary:
						xBody.Add(new XElement("p", "采集对象：明细账 ", new XElement("a", new XAttribute("href", "cellCollect"), "重新设置采账关系")));
						try
						{
							StringBuilder results2 = new StringBuilder();
							xBody.Add(new XElement("p", "采账关系：", new XElement("table", new XAttribute("cellspacing", "5"), collectManager.CollectItems.Select(delegate(CollectItem ci)
							{
								SubsidiaryItem subsidiaryItem = (SubsidiaryItem)ci;
								decimal value4 = subsidiaryItem.GetValue(ledger, year);
								if ((subsidiaryItem.Operation == OperateEnum.Add && value4 >= 0m) || (subsidiaryItem.Operation == OperateEnum.Subtract && value4 < 0m))
								{
									results2.Append("+");
								}
								else
								{
									results2.Append("-");
								}
								results2.Append(Math.Abs(value4).ToString("N"));
								Voucher voucher = subsidiaryItem.GetVoucher(ledger, year);
								return new XElement("tr", new XElement("td", OperateEnumToString(subsidiaryItem.Operation)), new XElement("td", subsidiaryItem.StartTime.ToString("yyyy-MM-dd")), new XElement("td", subsidiaryItem.TypeNumber), new XElement("td", voucher.Digest), new XElement("td", subsidiaryItem.AccountName), new XElement("td", voucher.IsDebit ? "借方发生额" : "贷方发生额"), new XElement("td", new XAttribute("style", "text-align:right"), value4.ToString("N")));
							}))));
							if (results2[0] == '+')
							{
								results2.Remove(0, 1);
							}
							xBody.Add(new XElement("p", $"采账结果：{collectManager.GetValue(ledger, year).GetValueOrDefault():N}"));
						}
						catch (InvalidAuditYearException ex4)
						{
							xBody.Add(new XElement("p", new XAttribute("style", "color:red"), ex4.Message));
						}
						catch (InvalidCollectSettingException)
						{
							xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "采账设置有误"));
						}
						catch (UnExpectAuditYearException)
						{
							xBody.Add(new XElement("p", new XAttribute("style", "color:red"), "审计年度与采账设置不符"));
						}
						break;
					}
					xBody.Add(new XElement("hr"));
				}
			}
		}
		else if (!string.IsNullOrWhiteSpace(Table.CollectSource) && (cell.Row.Role == RowRole.Normal || cell.Row.Role == RowRole.Among || cell.Row.Role == RowRole.Minus))
		{
			ExportArgs exportArgs = ExportArgs.Parse(Table.CollectSource);
			if (exportArgs.Mapping.TryGetValue(column.Id.Value, out var value2))
			{
				flag8 = true;
				xBody.Add(new XElement("b", "列对应采账设置"));
				xBody.Add(new XElement("p", "采集对象：" + CollectObjectEnumToString(exportArgs.CollectObject) + " ", new XElement("a", new XAttribute("href", "tableCollect"), "重新设置采账关系")));
				xBody.Add(new XElement("p", $"会计期间：{exportArgs.MonthStart}月-{exportArgs.MonthEnd}月"));
				xBody.Add(new XElement("p", "会计科目：" + exportArgs.AccountName));
				xBody.Add(new XElement("p", "列间对应：" + value2));
				xBody.Add(new XElement("hr"));
			}
		}
		Auditai.Model.CellAttachments attachments;
		bool flag9 = Table.CellPropManager.TryGetAttachments(cell, out attachments);
		if (flag9)
		{
			xBody.Add(new XElement("b", "附件管理"));
			for (i = 0; i < attachments.Attachments.Count; i++)
			{
				CellAttachment cellAttachment = attachments.Attachments[i];
				xBody.Add(new XElement("p", cellAttachment.Name));
				XElement xElement4 = new XElement("p", new XElement("a", "打开附件", new XAttribute("href", $"openAttachment{i}")), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", "导出附件", new XAttribute("href", $"exportAttachment{i}")), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"));
				xBody.Add(xElement4);
				linkDic.Add($"openAttachment{i}", i);
				linkDic.Add($"exportAttachment{i}", i);
				if (CanEditCell(cell, IsTableExistFillFormula(), ignoreProp: true))
				{
					xElement4.Add(new XElement("a", "删除附件", new XAttribute("href", $"removeAttachment{i}")), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", "重命名附件", new XAttribute("href", $"renameAttachment{i}")));
					linkDic.Add($"removeAttachment{i}", i);
					linkDic.Add($"renameAttachment{i}", i);
				}
			}
			XElement xElement5 = new XElement("p");
			xBody.Add(xElement5);
			if (CanEditCell(cell, IsTableExistFillFormula(), ignoreProp: true))
			{
				xElement5.Add(new XElement("a", new XAttribute("href", "addAttachment"), "插入附件"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"));
				linkDic.Add("addAttachment", null);
			}
			xElement5.Add(new XElement("a", "导出本表格所有附件", new XAttribute("href", "exportAllAttachment")));
			linkDic.Add("exportAllAttachment", null);
			xBody.Add(new XElement("hr"));
		}
		if (flag3 || flag4 || flag5 || flag6 || flag7 || flag8 || flag || flag9 || flag2)
		{
			xBody.LastNode.Remove();
			XElement xElement6 = xBody.Element("b");
			xElement6.Remove();
			_ttpComment.SetText(xElement6.Value, xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(linkDic);
			_ttpComment.LinkClicked += _ttpComment_LinkClicked;
			_ttpComment.Show(_grid, new Point(cellRect.Right, cellRect.Top + cellRect.Height / 2));
		}
		void AddValidationResult(ValidationResult vf, string anchorPrefix)
		{
			xBody.Add(new XElement("p", "公式说明：", vf.Source.Note));
			XElement xElement7 = new XElement("p", "校验等式：");
			FormulaEvaluator formulaEvaluator4 = new FormulaEvaluator(vf.Source.LeftExpr);
			Tuple<List<TooltipListener.FormulaTooltipSegment>, string> formulaTooltipSegments4 = formulaEvaluator4.GetFormulaTooltipSegments(resolver, null, vf);
			foreach (TooltipListener.FormulaTooltipSegment item9 in formulaTooltipSegments4.Item1)
			{
				xElement7.Add(item9.PreText);
				string text3 = $"l{anchorPrefix}{i}{item9.AnchorNumber}";
				xElement7.Add(new XElement("a", new XAttribute("href", text3), item9.Display));
				linkDic.Add(text3, item9.Ref);
			}
			xElement7.Add(formulaTooltipSegments4.Item2);
			xElement7.Add(vf.Source.Operator.Display);
			formulaEvaluator4 = new FormulaEvaluator(vf.Source.RightExpr);
			formulaTooltipSegments4 = formulaEvaluator4.GetFormulaTooltipSegments(resolver, null, vf);
			foreach (TooltipListener.FormulaTooltipSegment item10 in formulaTooltipSegments4.Item1)
			{
				xElement7.Add(item10.PreText);
				string text4 = $"r{anchorPrefix}{i}{item10.AnchorNumber}";
				xElement7.Add(new XElement("a", new XAttribute("href", text4), item10.Display));
				linkDic.Add(text4, item10.Ref);
			}
			xElement7.Add(formulaTooltipSegments4.Item2);
			xBody.Add(xElement7);
			string text5 = null;
			string text6 = null;
			if ((vf.LeftValue.Equals(0.0) && vf.RightValue.Equals(string.Empty)) || (vf.LeftValue.Equals(string.Empty) && vf.RightValue.Equals(0.0)))
			{
				text5 = "0";
				text6 = "0";
			}
			else
			{
				text5 = ValidationResult.ValueToString(vf.LeftValue);
				text6 = ValidationResult.ValueToString(vf.RightValue);
			}
			xBody.Add(new XElement("p", new XAttribute("style", "color:" + (vf.Passed ? "green" : "red")), "校验" + (vf.Passed ? "正确" : "错误") + "：", text5, vf.Source.Operator.Display, text6));
			xBody.Add(new XElement("hr"));
		}
	}

	private static string CollectObjectEnumToString(CollectObjectEnum coe)
	{
		return coe switch
		{
			CollectObjectEnum.Balance => "科目余额表", 
			CollectObjectEnum.Subsidiary => "明细账", 
			CollectObjectEnum.Summary => "月度汇总表", 
			_ => "", 
		};
	}

	private static string OperateEnumToString(OperateEnum oe)
	{
		return oe switch
		{
			OperateEnum.Add => "＋", 
			OperateEnum.Subtract => "－", 
			_ => "", 
		};
	}

	private static string AmountEnumToString(AmountEnum ae)
	{
		return ae switch
		{
			AmountEnum.CreditAmount => "本期贷方发生额", 
			AmountEnum.CreditBalance => "期末贷方余额", 
			AmountEnum.CreditBegin => "期初贷方余额", 
			AmountEnum.DebitAmount => "本期借方发生额", 
			AmountEnum.DebitBalance => "期末借方余额", 
			AmountEnum.DebitBegin => "期初借方余额", 
			AmountEnum.PreCreditAmount => "上期贷方发生额", 
			AmountEnum.PreDebitAmount => "上期借方发生额", 
			_ => "", 
		};
	}

	private void ForceRefresh()
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				_grid[i, j] = _grid[i, j];
			}
		}
	}

	private bool AnyHiddenRow(int start, int end)
	{
		for (int i = start; i <= end; i++)
		{
			if (!_grid.BodyGetRow(i).Visible)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateViewStateForLocker()
	{
		if (IsTableLocked)
		{
			cmdLockTable.Checked = true;
			AppCommands.TicketMode.Enabled = false;
			AppCommands.TicketReport.Enabled = false;
			ValidationEditor.Enabled = false;
			TitleEditor.View.Enabled = false;
			FootEditor.View.Enabled = false;
			_grid.AllowEditing = false;
			_grid.AllowResizing = AllowResizingEnum.None;
			FormulaEditor.View.Enabled = false;
			_grid.FilterManager.IsLocked = true;
		}
		else
		{
			cmdLockTable.Checked = false;
			AppCommands.TicketMode.Enabled = true;
			AppCommands.TicketReport.Enabled = true;
			ValidationEditor.Enabled = true;
			TitleEditor.View.Enabled = true;
			FootEditor.View.Enabled = true;
			_grid.AllowEditing = true;
			_grid.AllowResizing = AllowResizingEnum.Both;
			FormulaEditor.View.Enabled = true;
			_grid.FilterManager.IsLocked = false;
		}
	}

	private void PopulateTopLeftCell()
	{
		if (IsTableLocked)
		{
			_grid.SetCellImage(0, 0, Auditai.UI.Platform.Properties.Resources.TableLock);
		}
		else
		{
			_grid.SetCellImage(0, 0, null);
		}
		_grid.GetCellRange(0, 0).StyleNew.ImageAlign = ImageAlignEnum.CenterCenter;
	}

	private void PopulateRowsHeight()
	{
		_grid.BeginUpdate();
		for (int i = 0; i < Table.Rows.Count; i++)
		{
			_grid.BodyGetRow(i).Height = Table.Rows[i].Height;
		}
		_grid.EndUpdate();
		DoLayout();
	}

	private void SetCorruptedView()
	{
		_grid.Rows.Count = 0;
		_grid.Cols.Count = 1;
		TitleEditor.View.Rows.Count = 0;
		FootEditor.View.Rows.Count = 0;
		View.Enabled = false;
		FormulaEditor.View.Enabled = false;
	}

	private void FillDefaultValues()
	{
		try
		{
			if (Table == null) return;
			foreach (var cell in Table.Cells)
			{
				if (cell.Value == null || (cell.Value is string s && string.IsNullOrEmpty(s)))
				{
					string defaultValue = cell.DisplayDefaultValue;
					if (!string.IsNullOrEmpty(defaultValue))
					{
						cell.UpdateValue(defaultValue);
					}
				}
			}
		}
		catch { }
	}

	private bool IsExcludeFixedRowAndSumRowCell() { return false; }

	public void DoLayout()
	{
		pnlGrid.SuspendDrawing();
		int height = TitleEditor.GetHeight();
		TitleEditor.View.Height = height;
		int height2 = FootEditor.View.Visible ? FootEditor.GetHeight() : 0;
		FootEditor.View.Height = height2;
		int height3 = pnlGrid.Height - height - height2;
		_grid.AdjustPosition(new Size(pnlGrid.Width, height3), 0, top0: true, height2 == 0);
		TitleEditor.AdjustSize();
		FootEditor.AdjustSize();
		_grid.Top = height;
		FootEditor.View.Top = height + _grid.Height - 1;
		pnlGrid.ResumeDrawing();
	}

	public int GetPanelWidth()
	{
		return pnlGrid.Width;
	}

	public int GetGridWidth()
	{
		return _grid.GetGridWidth();
	}

	public int Get1stColumnWidth()
	{
		return _grid.Cols[0].WidthDisplay;
	}

	public void AttachTooltip()
	{
		_grid.MouseMove += delegate(object s1, MouseEventArgs e1)
		{
			if (tooltipManager.ShouldDisplay)
			{
				HitTestTypeEnum type = _grid.HitTest(e1.Location).Type;
				if (type == HitTestTypeEnum.ColumnHeader)
				{
					TipInfo tipInfo2 = TipInfo.Parse(TipResource.表格列头);
					if (tipInfo2 != null)
					{
						tooltipManager.Show(tipInfo2, _grid, e1.X, e1.Y);
					}
				}
				else
				{
					tooltipManager.Hide();
				}
			}
		};
		_grid.MouseLeave += delegate
		{
			tooltipManager.Hide();
		};
		ToolBar.CurrentLinkChanged += delegate(object s1, CommandLinkEventArgs e1)
		{
			if (!tooltipManager.ShouldDisplay)
			{
				tooltipManager.Hide();
			}
			else if (e1.CommandLink == null)
			{
				tooltipManager.Hide();
			}
			else
			{
				string str = null;
				switch (e1.CommandLink.Text.Replace("\n", ""))
				{
				case "填表提示":
					str = TipResource.云表格_填表提示;
					break;
				case "列对应采账设置":
					str = TipResource.列对应采数设置按钮;
					break;
				case "单元格采账设置":
					str = TipResource.单元格采数设置按钮;
					break;
				case "采账填充":
					str = TipResource.采数填充按钮;
					break;
				case "表底签名":
					str = TipResource.云表格_表底签名;
					break;
				case "关联表格":
					str = TipResource.云表格_侧边栏_关联表格;
					break;
				case "运算表格":
					str = TipResource.当前表运算;
					break;
				case "校验表格":
					str = TipResource.当前表校验;
					break;
				case "后退":
					str = TipResource.Ribbon菜单_主窗体右上角配置栏_回退;
					break;
				case "前进":
					str = TipResource.Ribbon菜单_主窗体右上角配置栏_前进;
					break;
				}
				if (e1.CommandLink.Text.Replace("\n", "") == StringConstBase.Current.TableNote)
				{
					str = TipResource.底稿说明;
				}
				TipInfo tipInfo = TipInfo.Parse(str);
				if (tipInfo != null)
				{
					Rectangle bounds = e1.CommandLink.Bounds;
					tooltipManager.Show(tipInfo, ToolBar, bounds.Left, bounds.Top);
				}
			}
		};
	}

	public Auditai.Model.Operand GetList(string comboList)
	{
		if (string.IsNullOrWhiteSpace(comboList))
		{
			return "";
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RowIndex = _grid.BodyRow,
			HostTable = Table,
			RefManager = Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Table.Project,
				CurrentTreeNode = Table.TreeNode
			}
		};
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(comboList)
			{
				Env = env
			};
			Auditai.Model.Operand operand = formulaEvaluator.EvaluateToOperand();
			if (operand is TreeListOperand || operand is TableListOperand || operand is MultiListOperand || operand is InputListOperand)
			{
				return operand;
			}
			return operand.ToValueSetOrderByRowIndex();
		}
		catch (FormulaException)
		{
			return ValueSetOperand.Empty;
		}
	}

	public void KeepNavigationPanelVisibleIfNecessary()
	{
		if (Table.Ticket.IsCurrentLevelSupported() && !Table.Ticket.IsEmpty())
		{
			Program.MainForm.ShowNavigationPanel();
		}
		else if (Table.Title?.NavTreeCellIdList != null && Table.Title.NavTreeCellIdList.Count > 0)
		{
			Program.MainForm.ShowNavigationPanel();
		}
	}

	public void OnEnterView()
	{
		Program.MainForm.HideNavigationPanel();
		if (Table != null)
		{
			if (Table.Ticket.IsCurrentLevelSupported() && !Table.Ticket.IsEmpty())
			{
				Program.MainForm.ShowNavigationPanel();
				Program.MainForm.TicketInputEditor.ShowNavTreePanelForTableEditor(Table);
			}
			else if (Table.Title?.NavTreeCellIdList != null && Table.Title.NavTreeCellIdList.Count > 0)
			{
				Program.MainForm.ShowNavigationPanel();
				Program.MainForm.BindControlToNavigationPanel(_navTreeContainer);
			}
		}
	}

	public void OnLeaveView()
	{
		Program.MainForm.HideNavigationPanel();
	}

	public void BindNavTreeViewToNavigationPanel()
	{
		Program.MainForm.BindControlToNavigationPanel(_navTreeContainer);
	}

	public void ScrollToAvailableRow(List<Auditai.Model.Row> rowsList)
	{
		if (rowsList == null || rowsList.Count == 0)
		{
			return;
		}
		foreach (Auditai.Model.Row rows in rowsList)
		{
			if (Table.ContainsRow(rows))
			{
				int num = _grid.Cols.Fixed;
				int num2 = _grid.Cols.Fixed;
				C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
				if (selection.LeftCol >= 0)
				{
					num = selection.LeftCol;
				}
				if (num >= _grid.Cols.Count)
				{
					num = _grid.Cols.Count - 1;
				}
				if (selection.RightCol >= 0)
				{
					num2 = selection.RightCol;
				}
				if (num2 >= _grid.Cols.Count)
				{
					num2 = _grid.Cols.Count - 1;
				}
				if (num < 0)
				{
					num = 0;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
				try
				{
					int num3 = rows.Index + _grid.Rows.Fixed;
					_grid.Select(num3, num, num3, num2, show: true);
					break;
				}
				catch (Exception exception)
				{
					exception.Log();
					break;
				}
			}
		}
	}

	#region 跨项目数据引用

	private void CmdCrossProjectDataRef_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (Auditai.Model.Project.Current == null || Table == null) return;
			using var frm = new frmCrossProjectDataRef(Auditai.Model.Project.Current, Table.Id);
		frm.ShowDialog();
		// 如果有引用被刷新过，重新从数据库加载表格数据
		if (frm.DataRefreshed)
		{
			ReloadFromDb();
		}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"打开跨项目数据引用失败:\n{ex.Message}\n\n堆栈:\n{ex.StackTrace}");
		}
	}

	private void CmdCrossProjectDataRef_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Checked = false;
		e.Enabled = Auditai.Model.Project.Current != null && Table != null;
	}

	private async void CmdRefreshCrossProjectRefs_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (Auditai.Model.Project.Current == null || Table == null) return;
			var manager = new CrossProjectDataRefManager(Auditai.Model.Project.Current);
			var results = await manager.ExecuteAll(Table.Id);
			int success = results.Results.Count(r => r.Success);
			int failed = results.Results.Count(r => !r.Success);
			Auditai.UI.Controls.MessageBox.Show(
				MessageBoxIcon.None,
				$"刷新完成：成功 {success} 个，失败 {failed} 个");
		}
		catch (Exception ex)
		{
			ex.Log();
		}
	}

	private void CmdRefreshCrossProjectRefs_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Checked = false;
		e.Enabled = Auditai.Model.Project.Current != null && Table != null;
	}

	private async void CmdRefreshSingleRef_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (Auditai.Model.Project.Current == null || Table == null || _grid.BodyRow < 0 || _grid.BodyCol < 0) return;

			// 检查当前单元格是否属于某个引用区域
			var marks = CrossProjectRefCellStyle.GetAllMarks();
			CrossProjectRefCellStyle.RefCellMark targetMark = null;
			foreach (var mark in marks)
			{
				if (_grid.BodyRow >= mark.StartRow && _grid.BodyRow <= mark.EndRow &&
					_grid.BodyCol >= mark.StartCol && _grid.BodyCol <= mark.EndCol)
				{
					targetMark = mark;
					break;
				}
			}

			if (targetMark == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前单元格不属于任何跨项目引用区域");
				return;
			}

			// 获取对应的引用配置并执行刷新
			var manager = new CrossProjectDataRefManager(Auditai.Model.Project.Current);
			var refs = manager.Store.Load(Table.Id).GetAwaiter().GetResult();
			var dataRef = refs.FirstOrDefault(r => r.Id.Value == targetMark.RefId.Value);
			if (dataRef == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未找到对应的引用配置");
				return;
			}

			var result = await manager.ExecuteRef(dataRef);
			if (result.Success)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"引用 \"{dataRef.Name}\" 刷新成功");
				// 重新应用样式
				ApplyCrossProjectRefCellStyles();
			}
			else
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, $"刷新失败: {result.ErrorMessage}");
			}
		}
		catch (Exception ex)
		{
			ex.Log();
		}
	}

	private void CmdRefreshSingleRef_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Checked = false;
		e.Enabled = Auditai.Model.Project.Current != null && Table != null && _grid != null && _grid.BodyRow >= 0 && _grid.BodyCol >= 0;
	}

	#region — 跨项目引用可视化标记 —

	/// <summary>
	/// 在表格加载完成后，根据 CrossProjectRefCellStyle 中的标记信息
	/// 设置引用数据单元格的背景色和 ToolTip
	/// </summary>
	private void ApplyCrossProjectRefCellStyles()
	{
		if (_table == null || _grid == null) return;
		try
		{
			// 获取当前表格的引用配置，用于解析行列范围
			var project = Auditai.Model.Project.Current;
			if (project == null) return;
			var manager = new CrossProjectDataRefManager(project);
			var refs = manager.Store.Load(_table.Id).GetAwaiter().GetResult();
			if (refs == null || refs.Count == 0)
			{
				_refCellStyleCache = null;
				return;
			}

			// 获取已有的标记（可能为空——未刷新过或程序重启后内存标记丢失）
			var marks = CrossProjectRefCellStyle.GetAllMarks();

			// 构建缓存字典：(row, col) → RefStatus，供 BodyOwnerDrawCell_Style O(1) 查找
			_refCellStyleCache = new Dictionary<(int row, int col), CrossProjectRefCellStyle.RefStatus>();

			_grid.BeginUpdate();
			foreach (var dataRef in refs)
			{
				// 查找对应的标记；未刷新过时默认 Normal 状态
				var mark = marks?.FirstOrDefault(m => m.RefId.Value == dataRef.Id.Value);
				var status = mark?.Status ?? CrossProjectRefCellStyle.RefStatus.Normal;

				// 解析行列范围
				int startRow = 0, endRow = -1, startCol = 0, endCol = -1;
				ResolveRefGridRange(dataRef, ref startRow, ref endRow, ref startCol, ref endCol);
				if (endRow < 0 || endCol < 0) continue;

				// CellRef 模式：通过 TargetCellId 查找实际行列位置
				if (dataRef.RefMode == RefMode.CellRef && _table != null)
				{
					try
					{
						var cfg = Newtonsoft.Json.Linq.JObject.Parse(dataRef.RefConfig);
						var targetCellId = cfg["TargetCellId"]?.ToObject<long>() ?? 0;
						if (targetCellId > 0)
						{
							bool found = false;
							for (int r = 0; r < _table.Rows.Count && !found; r++)
							{
								for (int c = 0; c < _table.Columns.Count && !found; c++)
								{
									var cell = _table[r, c];
									if (cell != null && cell.Id.Value == targetCellId)
									{
										startRow = r;
										endRow = r;
										startCol = c;
										endCol = c;
										found = true;
									}
								}
							}
						}
					}
					catch { /* 忽略解析失败 */ }
				}

				// 更新标记中的行列范围（如有标记）
				if (mark != null)
				{
					mark.StartRow = startRow;
					mark.EndRow = endRow;
					mark.StartCol = startCol;
					mark.EndCol = endCol;
				}

				// 填充缓存字典
				for (int r = startRow; r <= endRow && r < _grid.BodyRowsCount; r++)
				{
					for (int c = startCol; c <= endCol && c < _grid.BodyColsCount; c++)
					{
						_refCellStyleCache[(r, c)] = status;
					}
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[CrossProjectRefStyle] 应用引用样式失败: {ex.Message}");
		}
		finally
		{
			_grid?.EndUpdate();
		}
	}

	/// <summary>
	/// 根据引用配置解析目标表格中的行列范围
	/// </summary>
	private static void ResolveRefGridRange(CrossProjectDataRef dataRef, ref int startRow, ref int endRow, ref int startCol, ref int endCol)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(dataRef.RefConfig)) return;

			var config = Newtonsoft.Json.Linq.JObject.Parse(dataRef.RefConfig);

			switch (dataRef.RefMode)
			{
				case RefMode.CellRef:
					// 单元格引用：设置为单格范围（0,0），由调用方在表格上下文中精确匹配
					startRow = 0;
					endRow = 0;
					startCol = 0;
					endCol = 0;
					break;

				case RefMode.ColumnRef:
					{
						var targetStartRow = config["TargetStartRow"]?.ToObject<int>() ?? 0;
						startRow = targetStartRow;
						startCol = 0;
						// 通过 ColumnMapping 确定列数
						if (!string.IsNullOrWhiteSpace(dataRef.ColumnMapping))
						{
							try
							{
								var mapping = Newtonsoft.Json.Linq.JArray.Parse(dataRef.ColumnMapping);
								endCol = Math.Max(0, mapping.Count - 1);
							}
							catch
							{
								endCol = 0;
							}
						}
						else
						{
							endCol = 0;
						}
						endRow = startRow;
					}
					break;

				case RefMode.AreaRef:
					{
						// RefConfig 字段：TargetStartCol/TargetEndCol/TargetStartRow/TargetEndRow
						//                  SourceStartCol/SourceEndCol/SourceStartRow/SourceEndRow
						var targetStartRow = config["TargetStartRow"]?.ToObject<int>() ?? 0;
						var targetStartCol = config["TargetStartCol"]?.ToObject<int>() ?? 0;
						var targetEndRow = config["TargetEndRow"]?.ToObject<int>() ?? targetStartRow;
						var targetEndCol = config["TargetEndCol"]?.ToObject<int>() ?? targetStartCol;

						startRow = targetStartRow;
						startCol = targetStartCol;
						endRow = targetEndRow;
						endCol = targetEndCol;
					}
					break;

				case RefMode.FormulaCompute:
					startRow = 0;
					startCol = 0;
					endRow = 0;
					endCol = 0;
					break;
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[CrossProjectRefStyle] 解析引用范围失败: {ex.Message}");
		}
	}

	/// <summary>
	/// 在网格鼠标移动时显示跨项目引用数据的 ToolTip
	/// </summary>
	private void AttachCrossProjectRefToolTip()
	{
		if (_grid == null) return;
		_grid.MouseMove += (s, e) =>
		{
			try
			{
				var ht = _grid.HitTest(e.Location);
				if (ht.Type == C1.Win.C1FlexGrid.HitTestTypeEnum.None || ht.Row < 0 || ht.Column < 0) return;

				// 转换到 body 区域的行列
				int bodyRow = ht.Row - _grid.Rows.Fixed;
				int bodyCol = ht.Column - _grid.Cols.Fixed;
				if (bodyRow < 0 || bodyCol < 0) return;

				var marks = CrossProjectRefCellStyle.GetAllMarks();
				foreach (var mark in marks)
				{
					if (bodyRow >= mark.StartRow && bodyRow <= mark.EndRow &&
						bodyCol >= mark.StartCol && bodyCol <= mark.EndCol)
					{
						tooltipManager.Show(
							new TipInfo { Title = "跨项目引用", Body = mark.GetToolTipText() },
							_grid, e.X, e.Y);
						return;
					}
				}

				// 不在引用区域内，恢复默认 ToolTip
				tooltipManager.Hide();
			}
			catch
			{
				// 忽略 ToolTip 异常
			}
		};
	}

	#endregion

	#endregion
}
