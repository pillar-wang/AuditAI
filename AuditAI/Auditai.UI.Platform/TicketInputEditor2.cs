using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1Themes;
using Auditai.DTO;
using Auditai.Model;
using Auditai.PlatformResource;
using Auditai.SignalR;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Controls.CollectTable;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class TicketInputEditor2 : ISetTheme
{
	protected enum ShadowAreaIndex
	{
		Center,
		CenterTop,
		CeneterBottom,
		LeftTop,
		LeftCeneter,
		LeftBottom,
		RightTop,
		RightCenter,
		RightBottom
	}

	protected class OutPageData
	{
		public C1OutPage page;

		public TicketNavGrid navGrid;
	}

	protected class CollectFormulaDependSettingData
	{
		public object userData;

		public string Formula;

		public Auditai.Model.Cell FormulaComeFromWhichTableCell;

		public Auditai.Model.Column FormulaComeFromWhichTableColumn;

		public HashSet<Auditai.Model.Column> DependTableColumnSet;

		public HashSet<Auditai.Model.Cell> DependTableCellSet;
	}

	private struct CellCoordinate
	{
		public int x;

		public int y;

		public long longValue;

		public CellCoordinate(int xIndex, int yIndex)
		{
			longValue = ((long)xIndex << 32) + yIndex;
			x = xIndex;
			y = yIndex;
		}
	}

	private class GridResizeManager : GridResizingManager
	{
		protected TicketInputEditor2 _parent;

		public GridResizeManager(TicketInputEditor2 parent, C1FlexGridEx grid)
			: base(grid)
		{
			_parent = parent;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			_parent._grid_MouseMove(_grid, e);
		}

		protected override int GetResizingColumn(MouseEventArgs e)
		{
			int mouseRow = _grid.MouseRow;
			if (mouseRow >= _grid.Rows.Fixed)
			{
				return -1;
			}
			int resizingColumn = base.GetResizingColumn(e);
			if (resizingColumn == -1)
			{
				return -1;
			}
			if (resizingColumn < _grid.Cols.Fixed)
			{
				return -1;
			}
			if (_parent._isMouseOverCancelManualInputIcon)
			{
				return -1;
			}
			if (_grid.HitTest(e.X, e.Y).Type == HitTestTypeEnum.None)
			{
				return -1;
			}
			for (int num = resizingColumn; num >= _grid.Cols.Fixed; num--)
			{
				if (_grid.Cols[num].IsVisible)
				{
					return num;
				}
			}
			return -1;
		}

		protected override int GetResizingRow(MouseEventArgs e)
		{
			int mouseCol = _grid.MouseCol;
			if (mouseCol >= _grid.Cols.Fixed)
			{
				return -1;
			}
			int resizingRow = base.GetResizingRow(e);
			if (resizingRow == -1)
			{
				return -1;
			}
			if (_parent._isMouseOverCancelManualInputIcon)
			{
				return -1;
			}
			if (_grid.HitTest(e.X, e.Y).Type == HitTestTypeEnum.None)
			{
				return -1;
			}
			for (int num = resizingRow; num >= 0; num--)
			{
				if (_grid.Rows[num].IsVisible)
				{
					return num;
				}
			}
			return -1;
		}
	}

	protected class RowFillSetting
	{
		public bool IsNewRow = true;

		public int RowIndex = -1;
	}

	protected class RowIncreaseSetting
	{
		public int IncreaseStartIndex;

		public int IncreaseCount;
	}

	protected class TicketInputGridFilterContext : FilterContext
	{
		private readonly C1FlexGridEx _grid;

		private readonly TicketInputEditor2 _parentForm;

		public TicketInputGridFilterContext(TicketInputEditor2 parent)
		{
			_grid = parent._grid;
			_parentForm = parent;
		}

		public override List<FilterValue> GetColumnData(string columnId)
		{
			int col = GetColumnIndex(columnId);
			if (col == -1)
			{
				return new List<FilterValue>();
			}
			return (from i in Enumerable.Range(0, _grid.BodyRowsCount)
				select GetData(i, col)).ToList();
		}

		public override string GetColumnId(C1.Win.C1FlexGrid.Column col)
		{
			return (col.Index - _grid.Cols.Fixed).ToString();
		}

		public override int GetColumnIndex(string columnId)
		{
			if (columnId == null)
			{
				return -1;
			}
			int num = int.Parse(columnId);
			if (num < 0 || num + _grid.Cols.Fixed >= _grid.Cols.Count)
			{
				return -1;
			}
			return num;
		}

		public override FilterValue GetData(int row, int col)
		{
			TicketInputCellVM gridBodyCellVM = _parentForm.GetGridBodyCellVM(row, col);
			Type t = typeof(string);
			object value = gridBodyCellVM.Value;
			if (gridBodyCellVM.TableCell != null)
			{
				t = gridBodyCellVM.TableCell.DisplayDataType;
			}
			else if (gridBodyCellVM.Column != null)
			{
				t = gridBodyCellVM.Column.GetDataType();
			}
			string displayValue = gridBodyCellVM.GetDisplayValue();
			object value2 = Auditai.Model.Cell.ChangeDataTypeImpl(value, t);
			return FilterValue.FromObject(value2, displayValue);
		}

		public override FilterValue GetDataFilterOnColumnHeader(int col)
		{
			int num = Math.Min(_grid.BodyRowsCount, 100);
			Type type = typeof(string);
			object obj = string.Empty;
			DataFormat df = new DataFormat(DataFormatType.General);
			for (int i = 0; i < num; i++)
			{
				TicketInputCellVM gridBodyCellVM = _parentForm.GetGridBodyCellVM(i, col);
				if (gridBodyCellVM.TableCell != null)
				{
					type = gridBodyCellVM.TableCell.DisplayDataType;
					obj = gridBodyCellVM.TableCell.Value;
					df = (gridBodyCellVM.DataFormat.HasValue ? gridBodyCellVM.DataFormat.Value : gridBodyCellVM.TableCell.Column.GetFormat());
					break;
				}
				if (gridBodyCellVM.Column != null)
				{
					type = gridBodyCellVM.Column.GetDataType();
					obj = gridBodyCellVM.Value;
					df = gridBodyCellVM.Column.GetFormat();
					break;
				}
			}
			if (obj == null || "" == obj.ToString())
			{
				if (type == typeof(DateTime))
				{
					obj = DateTime.Now;
				}
				else if (type == typeof(TimeSpan))
				{
					obj = TimeSpan.FromDays(1.0);
				}
				else if (type == typeof(DateYearMonth))
				{
					obj = new DateYearMonth(DateTime.Now);
				}
			}
			object value = Auditai.Model.Cell.ChangeDataTypeImpl(obj, type);
			return FilterValue.FromObject(value, Auditai.Model.Cell.GetDisplayValueImpl(value, df));
		}

		public override Type GetColumnDataType(string columnId)
		{
			if (_parentForm.Ticket.Kind == TicketKind.FixedOneRow)
			{
				return typeof(string);
			}
			int columnIndex = GetColumnIndex(columnId);
			if (columnIndex == -1 || columnIndex >= _parentForm._vm.GetColumnsCount())
			{
				return typeof(string);
			}
			int rowsCount = _parentForm._vm.GetRowsCount();
			for (int i = 0; i < rowsCount; i++)
			{
				TicketInputCellVM cellVM = _parentForm._vm.GetCellVM(i, columnIndex);
				if (cellVM.Column != null)
				{
					return cellVM.Column.GetDataType();
				}
			}
			return typeof(string);
		}

		public override string GetColumnDataTypeFormatString(string columnId)
		{
			if (_parentForm.Ticket.Kind == TicketKind.FixedOneRow)
			{
				return string.Empty;
			}
			int columnIndex = GetColumnIndex(columnId);
			if (columnIndex == -1 || columnIndex >= _parentForm._vm.GetColumnsCount())
			{
				return string.Empty;
			}
			int rowsCount = _parentForm._vm.GetRowsCount();
			for (int i = 0; i < rowsCount; i++)
			{
				TicketInputCellVM cellVM = _parentForm._vm.GetCellVM(i, columnIndex);
				if (cellVM.Column != null)
				{
					return cellVM.Column.GetFormat().GetFormatString();
				}
			}
			return string.Empty;
		}

		public override Tuple<bool, string, string> IsCheckBox(int row, int col)
		{
			TicketInputCellVM gridBodyCellVM = _parentForm.GetGridBodyCellVM(row, col);
			if (gridBodyCellVM.IsField && gridBodyCellVM.Column != null)
			{
				switch (gridBodyCellVM.Column.GetFormat().FormatType)
				{
				case DataFormatType.BoolCheckBox:
					return Tuple.Create(item1: true, "选中", "未选中");
				case DataFormatType.BoolOnOff:
					return Tuple.Create(item1: true, "打开", "关闭");
				}
			}
			return Tuple.Create(item1: false, "", "");
		}

		public override Tuple<bool, string, string> IsCheckBoxFilterOnColumnHeader(int col)
		{
			int num = Math.Min(_grid.BodyRowsCount, 100);
			DataFormatType dataFormatType = DataFormatType.General;
			for (int i = 0; i < num; i++)
			{
				TicketInputCellVM gridBodyCellVM = _parentForm.GetGridBodyCellVM(i, col);
				if (gridBodyCellVM.TableCell != null)
				{
					dataFormatType = (gridBodyCellVM.DataFormat.HasValue ? gridBodyCellVM.DataFormat.Value.FormatType : gridBodyCellVM.TableCell.Column.GetFormat().FormatType);
					break;
				}
				if (gridBodyCellVM.Column != null)
				{
					dataFormatType = gridBodyCellVM.Column.GetFormat().FormatType;
					break;
				}
			}
			return dataFormatType switch
			{
				DataFormatType.BoolCheckBox => Tuple.Create(item1: true, "选中", "未选中"), 
				DataFormatType.BoolOnOff => Tuple.Create(item1: true, "打开", "关闭"), 
				_ => Tuple.Create(item1: false, "", ""), 
			};
		}

		public override string GetColumnCaption(string columnId)
		{
			if (_parentForm.Ticket.ColumnHeaderRowsCount < 1)
			{
				return "";
			}
			int columnIndex = GetColumnIndex(columnId);
			if (columnIndex == -1)
			{
				return "";
			}
			int num = columnIndex + _grid.Cols.Fixed;
			if (num >= _grid.Cols.Count)
			{
				return "";
			}
			int col = _parentForm.ConvertGridColIndexToVMColIndex(num);
			int rowsCount = _parentForm._vm.GetRowsCount();
			for (int num2 = _parentForm.Ticket.ColumnHeaderRowsCount - 1; num2 >= 0; num2--)
			{
				if (num2 < rowsCount)
				{
					TicketInputCellVM mergeTopLeftCellVM = _parentForm._vm.GetMergeTopLeftCellVM(num2, col);
					return mergeTopLeftCellVM.GetDisplayValue();
				}
			}
			return "";
		}
	}

	protected class ShowColumnsSelector
	{
		private frmShowColumns _form;

		private CheckedListBox _clb => _form._clb;

		public IEnumerable<ColumnsSetting> Selected { get; private set; }

		private void BtnOk_Click(object sender, EventArgs e)
		{
			Selected = _clb.CheckedItems.Cast<ColumnsSetting>();
		}

		public DialogResult ShowDialog(TicketTable ticket)
		{
			_form = new frmShowColumns();
			_form.btnOk.Click += BtnOk_Click;
			for (int i = 0; i < ticket.Columns.Count; i++)
			{
				TicketColumn ticketColumn = ticket.Columns[i];
				if (!ticketColumn.IsHiddenColumn)
				{
					continue;
				}
				string text = string.Empty;
				if (ticket.ColumnHeaderRowsCount > 0)
				{
					for (int j = 0; j < ticket.ColumnHeaderRowsCount; j++)
					{
						TicketCell cell = ticket.GetCell(j, i);
						if (!string.IsNullOrWhiteSpace(cell.Text))
						{
							text = cell.Text;
							break;
						}
					}
				}
				if (string.IsNullOrWhiteSpace(text))
				{
					text = Auditai.Model.Column.GetExcelColumnName(i);
				}
				ColumnsSetting columnsSetting = new ColumnsSetting();
				columnsSetting.ColumIndex = i;
				columnsSetting.CaptionDisplay = text;
				_clb.Items.Add(columnsSetting);
			}
			return _form.ShowDialog();
		}
	}

	protected class ColumnsSetting
	{
		public string CaptionDisplay { get; set; }

		public int ColumIndex { get; set; }
	}

	protected class TicketColumnLedgerCollectFormulaSetting
	{
		public int TicketColumnIndex;

		public TicketColumn TicketColumn;

		public Auditai.Model.Column TableColumn;

		public string LedgerCollectFormula;

		public IsLedgerCollectFillFormula IsFill;
	}

	protected class MixTicketDynamicDataRowRangeSetting
	{
		public int DataRowStartIndex;

		public int DataRowsCount;

		public int DataRowTemplateId = -1;

		public int DesignModeDataRowsCount = 1;
	}

	protected class CancelTicketInputDataException : Exception
	{
	}

	public static readonly Cursor CursorInDragging;

	public static readonly Cursor CursorDisableDrag;

	private static readonly C1TextBoxEx _dateEdit;

	private static readonly C1TextBoxEx _timeEdit;

	private readonly C1FlexGridEx _grid;

	internal TicketInputTableVM _vm;

	private int _currentRecord;

	private bool _isAdd;

	private bool _isDirty;

	private bool _isMouseOverCancelManualInputIcon;

	private TicketInputTitleFooterEditor _titleEditor;

	private TicketInputTitleFooterEditor _footerEditor;

	private GridResizingManager _gridResizingManager;

	private LazyExcute _lazySearchExcute = new LazyExcute();

	private readonly frmSearchTicketRecord frmSearchTicketRecord = new frmSearchTicketRecord();

	private readonly C1SplitterPanel _editorPanel;

	private readonly C1SplitContainer _splc;

	private readonly C1SplitterPanel _pnlToolbar;

	private readonly C1SplitContainer _navTreeContainer;

	private readonly C1SplitterPanel _switchViewPanel;

	private readonly C1SplitterPanel _navTreePanel;

	private readonly C1ToolBar _tbr;

	private readonly C1Command _cmdQuitTicket;

	private readonly C1CommandLink _lnkQuitTicket;

	private readonly C1Command _cmdAdd;

	private readonly C1CommandLink _lnkAdd;

	private readonly C1Command _cmdDelete;

	private readonly C1CommandLink _lnkDelete;

	private readonly C1Command _cmdSave;

	private readonly C1CommandLink _lnkSave;

	private readonly C1Command _cmdCancelSave;

	private readonly C1CommandLink _lnkCancelSave;

	private readonly C1Command _cmdPrevious;

	private readonly C1CommandLink _lnkPrevious;

	private readonly C1Command _cmdNext;

	private readonly C1CommandLink _lnkNext;

	private readonly C1Command _cmdHelp;

	private readonly C1CommandLink _lnkHelp;

	private readonly C1Button _btnInsertRow;

	private readonly C1Button _btnRemoveRow;

	private readonly C1ContextMenu _ctxNav;

	private readonly C1Command _cmdAddNav;

	private readonly C1CommandLink _lnkAddNav;

	private readonly C1Command _cmdModifyNav;

	private readonly C1CommandLink _lnkModifyNav;

	private readonly C1Command _cmdDeleteNav;

	private readonly C1CommandLink _lnkDeleteNav;

	private readonly C1Command _cmdCopy;

	private readonly C1CommandLink _lnkCopy;

	private readonly C1Command _cmdCut;

	private readonly C1CommandLink _lnkCut;

	private readonly C1Command _cmdPaste;

	private readonly C1CommandLink _lnkPaste;

	private readonly C1Command _cmdInsertDataRow;

	private readonly C1CommandLink _lnkInsertDataRow;

	private readonly C1Command _cmdAppendDataRow;

	private readonly C1CommandLink _lnkAppendDataRow;

	private readonly C1Command _cmdRemoveDataRow;

	private readonly C1CommandLink _lnkRemoveDataRow;

	private readonly C1Command _cmdMoveUpDataRow;

	private readonly C1Command _cmdMoveDownDataRow;

	private readonly C1Command _cmdMoveTopestDataRow;

	private readonly C1Command _cmdMoveBottomestDataRow;

	private readonly C1Command _cmdRowHeight;

	private readonly C1CommandLink _lnkRowHeight;

	private readonly C1Command _cmdColumnWidth;

	private readonly C1CommandLink _lnkColumnWidth;

	private readonly C1Command _cmdAddAttachment;

	private readonly C1CommandLink _lnkAddAttachment;

	private readonly C1Command _cmdRemoveAttachment;

	private readonly C1CommandLink _lnkRemoveAttachment;

	private readonly C1Command _cmdExportAttachment;

	private readonly C1CommandLink _lnkExportAttachment;

	private readonly C1Command _cmdCalculateTable;

	private readonly C1Command _cmdCheckTable;

	private readonly C1Command _cmdDesignTicket;

	private readonly C1Command _cmdExportTicket;

	private readonly C1Command _cmdCollectFill;

	private readonly C1Command _cmdCollectFill2;

	private readonly C1Command _cmdFrozenRow;

	private readonly C1Command _cmdUnFrozenRow;

	private readonly C1Command _cmdFrozenCol;

	private readonly C1Command _cmdUnFrozenCol;

	private readonly C1Command _cmdSetColHeader;

	private readonly C1Command _cmdCancelColHeader;

	private readonly C1Command _cmdHideColumn;

	private readonly C1Command _cmdShowColumn;

	private readonly C1ContextMenu _ctxCell;

	private readonly C1ContextMenu _ctxEmpty;

	private readonly C1ContextMenu _ctxLock;

	private readonly C1ContextMenu _ctxRowHeader;

	private readonly C1CommandLink _lnkRowHeaderSetRowHeight;

	private readonly C1RadioButtonExSupportCheckImage _radioButtonOpenTableView;

	private readonly C1RadioButtonExSupportCheckImage _radioButtonOpenTicketView;

	private readonly List<TicketNavGrid> _navGrids = new List<TicketNavGrid>();

	private readonly RibbonImageProcess _imageProcess;

	private readonly C1OutBarEx _otbNavs;

	private readonly MainForm _owner;

	private static SolidBrush _cancelManualInputBackgroundBrush;

	private static readonly Timer _timerWarningHighlight;

	private static readonly StringFormat _sf;

	private bool _skipOtbIndexChange;

	private TooltipBox _ttpComment = new TooltipBox
	{
		Opacity = 0.8,
		IsBalloon = true
	};

	private Color _colorBorder;

	private TicketNavGrid _preSelectedNavGrid;

	protected List<NavTreeScrollPosition> _navTreeScollPositionList = new List<NavTreeScrollPosition>();

	protected TicketGridDecorator _gridDecorator;

	private long _wantPopulatedRecordContainsRowId;

	private Auditai.Model.Row _recordFirstTableRow;

	private int _recordFirstTableRowIndex = -1;

	private bool _isSuggestKeepNavTreeNodeInTablePosition;

	private bool _isTicketLocked;

	protected const int NUMBER_COLUMN_WIDTH = 30;

	private bool _isMouseOverRowNumberColumn;

	private int _mouseOverHeaderColumn = -1;

	private bool _isMouseOverShowMoreMenuImage;

	private readonly SolidBrush _brushStartEditingColHeaderBackground = new SolidBrush(Color.Black);

	private static readonly Font _rowNumberFont;

	protected bool _isCurrentTicketComeFromVirtualNode;

	protected bool _isInShowingVirtualNode;

	protected Rectangle[] _shadowAreaRangeArr = new Rectangle[9];

	protected System.Drawing.Image[] _shadowAreaImageArr = new System.Drawing.Image[9];

	protected const int NineSquareGridImageCenterSize = 32;

	protected bool _isNeedPaintShadow;

	protected Queue<OutPageData> _freePageList = new Queue<OutPageData>();

	protected List<OutPageData> _inUsingPageList = new List<OutPageData>();

	protected Color _existTableDataRowNumberColor = Color.DimGray;

	protected bool _isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;

	private bool _isSuspendViewChangeModeCheckEvent;

	private Brush _switchViewPanelBackgroundBrush;

	private int _warningUpdateTimes;

	private Color _warningTextColor = Color.Red;

	private Color _remindTextColor = Color.Orange;

	private bool _warningTextIsShown = true;

	private bool _isSuspendFilterManager_AfterFilterExecuteEvent;

	private C1FlexGridEx _currentSelectGrid;

	private C1.Win.C1FlexGrid.CellRange _currentselectRange;

	private List<int> _filterVisibleRowIndexList = new List<int>();

	private bool _suspendPanelResizeEvent;

	private bool _isBuildTableCellInModifyModeOnOpenCurrentRecord;

	private List<C1CommandLink> _filterCommandLinkList = new List<C1CommandLink>();

	private bool _isTableSaveActionOccured;

	private bool _isForceReCalculateTableBeforeSaveRecord;

	private bool _isTableDataReloadFromDB;

	private bool _isSuspendTicketRecordChangeEvent;

	private Dictionary<TicketRecord, int> _recordValidationErrorCountDic = new Dictionary<TicketRecord, int>();

	private Dictionary<C1OutPage, TicketNavGrid> _pageNavGridDic = new Dictionary<C1OutPage, TicketNavGrid>();

	private Dictionary<TicketNavGrid, Tuple<C1OutPage, string>> _navGridPagesDic = new Dictionary<TicketNavGrid, Tuple<C1OutPage, string>>();

	private const int ShadowTopSpace = 0;

	private const int ShadowLeftSpace = 0;

	private const int ShadowRightSpace = 0;

	private const int ShadowBottomSpace = 0;

	private bool _isGridNeedShowVScrollBar;

	private bool _isSuspendBodySelectionChangeEvent;

	private C1.Win.C1FlexGrid.CellRange[] _ticketGridColumnMerges;

	private const int _ctxCommandCheckCellMaxCount = 10000;

	private C1OutPage _currentMouseOverPage;

	private bool _isMouseOverMoreMenuIcon;

	private Bitmap _menuMoreOperationWhiteImage;

	private SolidBrush _brushMoreMenuIconBackground = new SolidBrush(Color.Black);

	private Color _pageTitleBackgroundColor = Color.Black;

	private bool _isSuspendVirtualNodeSelectChangeEvent;

	private bool _isInTrySelectTicketNavTreeFirstNodeMode;

	public Auditai.Model.Table Table { get; set; }

	public TicketTable Ticket => Table.Ticket;

	public C1SplitContainer View => _splc;

	public TicketRecord Record { get; set; }

	public PageSetup PageSetup => Ticket.PageSetup;

	private TicketNavGrid _currentNavGrid => _navGrids[_otbNavs.SelectedIndex];

	public bool HasFillingFormula { get; set; }

	public bool IsAllowModifyTableRowOrder { get; private set; }

	internal TicketInputTableVM VMData
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

	public C1SplitterPanel EditorPanel => _editorPanel;

	public TicketInputTitleFooterEditor TitleEditor => _titleEditor;

	public TicketInputTitleFooterEditor FooterEditor => _footerEditor;

	public MainForm Owner => _owner;

	public C1FlexGridEx Grid => _grid;

	public TicketGridDecorator GridDecorator => _gridDecorator;

	public C1OutBarEx NavTreeOutBar => _otbNavs;

	public bool IsInShowingVirtualNode => _isInShowingVirtualNode;

	public bool IsTicketLocked => _isTicketLocked;

	public ListDropDown ListDropDown { get; private set; }

	public InputListDropDown InputListDropDown { get; private set; }

	public Color WarningTextColor => _warningTextColor;

	public Color RemindTextColor => _remindTextColor;

	public bool IsWarningTextNeedToShow => _warningTextIsShown;

	private bool IsHasSaveDataPermission => !Table.IsLocked;

	private void SetCurrentRecord()
	{
		if (Ticket == null)
		{
			return;
		}
		TicketRecord ticketRecord = null;
		if (_wantPopulatedRecordContainsRowId != 0L && Ticket.Navs.Count > 0)
		{
			TicketRecord ticketRecord2 = Ticket.Records.Find((TicketRecord r) => r.Rows.Any((Auditai.Model.Row u) => u.Id.Value == _wantPopulatedRecordContainsRowId));
			if (ticketRecord2 != null)
			{
				_currentNavGrid.FindAndSelectRecord(ticketRecord2);
				_currentRecord = _currentNavGrid.GetCurrentIndex();
				ticketRecord = ticketRecord2;
			}
			_wantPopulatedRecordContainsRowId = 0L;
		}
		if (_currentRecord < 0)
		{
			_currentRecord = int.MaxValue;
		}
		if (Ticket.Navs.Count > 0)
		{
			if (_currentRecord >= _currentNavGrid.RecordList.Count)
			{
				_currentRecord = _currentNavGrid.RecordList.Count - 1;
			}
			if (_currentRecord < 0)
			{
				Record = null;
			}
			else
			{
				Record = (TicketRecord)_currentNavGrid.RecordList[_currentRecord];
				if (Record != ticketRecord)
				{
					_currentNavGrid.FindAndSelectRecord(Record);
				}
			}
		}
		else
		{
			if (_currentRecord >= Ticket.Records.Count)
			{
				_currentRecord = Ticket.Records.Count - 1;
			}
			if (_currentRecord < 0)
			{
				Record = null;
			}
			else
			{
				Record = Ticket.Records[_currentRecord];
			}
		}
		_isSuggestKeepNavTreeNodeInTablePosition = false;
		if (Record != null && Record.Rows != null && Record.Rows.Count > 0)
		{
			_recordFirstTableRow = Record.Rows[0];
			_recordFirstTableRowIndex = _recordFirstTableRow.Index;
			_isTicketLocked = Record.Rows.Any((Auditai.Model.Row u) => !Table.IsControlFormulaAllowEditRow(u));
		}
		else
		{
			_recordFirstTableRow = null;
			_recordFirstTableRowIndex = -1;
		}
		TicketNavTreeStatusDataCacher.SaveNavTreeSelectedRecordData(Table.Id, Record);
	}

	static TicketInputEditor2()
	{
		CursorInDragging = new Cursor(new MemoryStream(Resources.draggingCursor));
		CursorDisableDrag = new Cursor(new MemoryStream(Resources.disableDragCursor));
		_dateEdit = new C1TextBoxEx();
		_timeEdit = new C1TextBoxEx();
		_cancelManualInputBackgroundBrush = new SolidBrush(Color.Gray);
		_timerWarningHighlight = new Timer
		{
			Interval = 500
		};
		_sf = new StringFormat
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		};
		_rowNumberFont = new Font("微软雅黑", 9f);
		_dateEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
		_timeEdit.ErrorInfo.ShowErrorMessage = false;
	}

	public TicketInputEditor2(MainForm owner)
	{
		_owner = owner;
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
			Cursor = TableEditor.CursorTable,
			Dock = DockStyle.None,
			FocusRect = FocusRectEnum.None,
			AutoClipboard = false,
			ScrollOptions = ScrollFlags.None
		};
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 0;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
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
		_grid.BeforeScroll += _grid_BeforeScroll;
		_grid.AfterScroll += _grid_AfterScroll;
		_grid.Enter += _grid_Enter;
		_grid.MouseLeave += _grid_MouseLeave;
		_grid.AllowResizing = AllowResizingEnum.None;
		_grid.FilterManager.Context = new TicketInputGridFilterContext(this);
		_grid.FilterManager.AfterFilterExecute += FilterManager_AfterFilterExecute;
		_gridResizingManager = new GridResizeManager(this, _grid);
		_gridResizingManager.ResizeColumn += _gridResizingManager_ResizeColumn;
		_gridResizingManager.ResizeRow += _gridResizingManager_ResizeRow;
		_splc = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_pnlToolbar = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Right,
			Width = 80,
			KeepRelativeSize = false,
			Resizable = false,
			Collapsible = false
		};
		_tbr = new C1ToolBar
		{
			Dock = DockStyle.Fill,
			Horizontal = false,
			MinButtonSize = 40,
			ButtonLookVert = ButtonLookFlags.TextAndImage
		};
		_cmdQuitTicket = new C1Command
		{
			Text = "表格模式",
			Image = Resources.TableMode
		};
		_cmdQuitTicket.Click += _cmdQuitTicket_Click;
		_cmdAdd = new C1Command
		{
			Text = "新增表单",
			Image = Resources.CreateTemplate
		};
		_cmdAdd.Click += _cmdAdd_Click;
		_cmdDelete = new C1Command
		{
			Text = "删除表单",
			Image = Resources.RemoveProject
		};
		_cmdDelete.Click += _cmdDelete_Click;
		_cmdSave = new C1Command
		{
			Text = "保存表单",
			Image = Resources.SaveRecord
		};
		_cmdSave.Click += _cmdSave_Click;
		_cmdCancelSave = new C1Command
		{
			Text = "取消保存",
			Image = Resources.FormulaCancel
		};
		_cmdCancelSave.Click += _cmdCancelSave_Click;
		_cmdPrevious = new C1Command
		{
			Text = "上一个表单",
			Image = Resources.PreviousError
		};
		_cmdPrevious.Click += _cmdPrevious_Click;
		_cmdNext = new C1Command
		{
			Text = "下一个表单",
			Image = Resources.NextError
		};
		_cmdNext.Click += _cmdNext_Click;
		_cmdHelp = new C1Command
		{
			Text = "帮助中心",
			Image = Resources.HelpCenter,
			Visible = SoftwareLicenseManager.IsShowHelpDocumentButton()
		};
		_cmdHelp.Click += _cmdHelp_Click;
		_cmdDesignTicket = new C1Command
		{
			Text = "设计表单",
			Image = Resources.TicketMode
		};
		_cmdDesignTicket.Click += _cmdDesignTicket_Click;
		_cmdDesignTicket.CommandStateQuery += _cmdDesignTicket_CommandStateQuery;
		_cmdCalculateTable = new C1Command
		{
			Text = "运算表单",
			Image = Resources.CalculateTable
		};
		_cmdCheckTable = new C1Command
		{
			Text = "校验表单",
			Image = Resources.ValidateTable
		};
		_cmdExportTicket = new C1Command
		{
			Text = "导出表单",
			Image = Resources.ExportExcel
		};
		_cmdExportTicket.Click += _cmdExportTicket_Click;
		_cmdCollectFill = new C1Command
		{
			Text = "采账填充",
			Image = Resources.GenerateWorkingPaper
		};
		_cmdCollectFill.Click += Cmd_Click_CollectFill;
		_cmdCollectFill.CommandStateQuery += _cmdCollectFill_CommandStateQuery;
		_tbr.CommandLinks.Add(new C1CommandLink(_cmdCollectFill));
		_cmdCollectFill2 = new C1Command
		{
			Text = "采数填充",
			Image = Resources.GenerateWorkingPaper
		};
		_cmdCollectFill2.Click += _cmdCollectFill2_Click;
		_cmdCollectFill2.CommandStateQuery += _cmdCollectFill2_CommandStateQuery;
		_tbr.CommandLinks.Add(new C1CommandLink(_cmdCollectFill2));
		_cmdCalculateTable.Click += _cmdCalculateTable_Click;
		_cmdCheckTable.Click += _cmdCheckTable_Click;
		_lnkAdd = new C1CommandLink(_cmdAdd)
		{
			Delimiter = true
		};
		_tbr.CommandLinks.Add(_lnkAdd);
		_lnkDelete = new C1CommandLink(_cmdDelete);
		_tbr.CommandLinks.Add(_lnkDelete);
		_lnkSave = new C1CommandLink(_cmdSave);
		_tbr.CommandLinks.Add(_lnkSave);
		_lnkCancelSave = new C1CommandLink(_cmdCancelSave);
		_tbr.CommandLinks.Add(_lnkCancelSave);
		_lnkPrevious = new C1CommandLink(_cmdPrevious)
		{
			Delimiter = true
		};
		_lnkNext = new C1CommandLink(_cmdNext);
		_tbr.CommandLinks.Add(new C1CommandLink(_cmdCalculateTable)
		{
			Delimiter = true
		});
		_tbr.CommandLinks.Add(new C1CommandLink(_cmdCheckTable));
		_tbr.CommandLinks.Add(new C1CommandLink(_cmdExportTicket));
		_lnkQuitTicket = new C1CommandLink(_cmdQuitTicket)
		{
			Delimiter = true
		};
		_tbr.CommandLinks.Add(new C1CommandLink(_cmdDesignTicket)
		{
			Delimiter = true
		});
		_lnkHelp = new C1CommandLink(_cmdHelp)
		{
			Delimiter = true
		};
		_tbr.CommandLinks.Add(_lnkHelp);
		_pnlToolbar.Controls.Add(_tbr);
		_splc.Panels.Add(_pnlToolbar);
		_navTreeContainer = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_switchViewPanel = new C1SplitterPanelEx
		{
			Dock = PanelDockStyle.Top,
			KeepRelativeSize = false,
			Height = 31,
			MinHeight = 31,
			Collapsible = false,
			Resizable = false,
			BorderWidth = 1,
			DoubleBuffered = true
		};
		_navTreePanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			SizeRatio = 100.0
		};
		_otbNavs = new C1OutBarEx
		{
			Dock = DockStyle.Fill,
			ShowScrollButtons = false
		};
		_otbNavs.SelectedIndexChanged += _otbNavs_SelectedIndexChanged;
		_otbNavs.Paint += _otbNavs_Paint;
		_otbNavs.MouseClick += _otbNavs_MouseClick;
		_otbNavs.MouseMove += _otbNavs_MouseMove;
		_otbNavs.MouseLeave += _otbNavs_MouseLeave;
		_otbNavs.PageBeforeSelectEventHandle = C1OutPageSelectEvent;
		_otbNavs.PageTitlePostPaintHandle = _otbNavs_PageTitlePostPaint;
		_otbNavs.Animate = false;
		C1SplitContainer c1SplitContainer = new C1SplitContainer
		{
			Dock = DockStyle.Fill,
			BackColor = Color.Transparent
		};
		C1SplitterPanelEx c1SplitterPanelEx = new C1SplitterPanelEx
		{
			Dock = PanelDockStyle.Left,
			SizeRatio = 100.0,
			BackColor = Color.Transparent,
			Resizable = false,
			DoubleBuffered = true,
			BackgroundRenderCallback = _switchViewPanelBackground_Paint
		};
		_radioButtonOpenTableView = new C1RadioButtonExSupportCheckImage
		{
			CheckedImage = Resources.TreeTable,
			UnCheckedImage = Resources.TreeTable,
			Text = "表格模式",
			CheckImageSize = new Size(16, 16),
			TextImageDistance = 20,
			Size = new Size(78, 25),
			TextOnImageLeft = false,
			Margin = new Padding(3, 0, 0, 0),
			CornerRadius = 4,
			BackgroundPaintHandle = _openViewRadioButtonBackground_Paint
		};
		_radioButtonOpenTicketView = new C1RadioButtonExSupportCheckImage
		{
			CheckedImage = Resources.Ticket16,
			UnCheckedImage = Resources.Ticket16,
			Text = "表单模式",
			CheckImageSize = new Size(16, 16),
			TextImageDistance = 20,
			Size = new Size(78, 25),
			TextOnImageLeft = false,
			Margin = new Padding(3, 0, 0, 0),
			CornerRadius = 4,
			BackgroundPaintHandle = _openViewRadioButtonBackground_Paint
		};
		c1SplitterPanelEx.Controls.Add(_radioButtonOpenTableView);
		c1SplitterPanelEx.Controls.Add(_radioButtonOpenTicketView);
		c1SplitContainer.Panels.Add(c1SplitterPanelEx);
		_switchViewPanel.Controls.Add(c1SplitContainer);
		_switchViewPanel.SizeChanged += _switchViewPanel_SizeChanged;
		_radioButtonOpenTableView.CheckedChanged += _radioButtonOpenTableView_CheckedChanged;
		_radioButtonOpenTicketView.CheckedChanged += _radioButtonOpenTicketView_CheckedChanged;
		_navTreePanel.Controls.Add(_otbNavs);
		_navTreeContainer.Panels.Add(_switchViewPanel);
		_navTreeContainer.Panels.Add(_navTreePanel);
		SecondTrigger.Trigger.Tick += Trigger_Tick;
		_cmdAddNav = new C1Command
		{
			Text = "新建导航树...",
			Image = Resources.AddTicketNav
		};
		_cmdAddNav.CommandStateQuery += _cmdAddNav_CommandStateQuery;
		_cmdAddNav.Click += _cmdAddNav_Click;
		_cmdModifyNav = new C1Command
		{
			Text = "修改导航树...",
			Image = Resources.ModifyTicketNav
		};
		_cmdModifyNav.CommandStateQuery += _cmdModifyNav_CommandStateQuery;
		_cmdModifyNav.Click += _cmdModifyNav_Click;
		_cmdDeleteNav = new C1Command
		{
			Text = "删除导航树",
			Image = Resources.RemoveTicketNav
		};
		_cmdDeleteNav.CommandStateQuery += _cmdDeleteNav_CommandStateQuery;
		_cmdDeleteNav.Click += _cmdDeleteNav_Click;
		_ctxNav = new C1ContextMenu();
		_lnkAddNav = new C1CommandLink(_cmdAddNav);
		_ctxNav.CommandLinks.Add(_lnkAddNav);
		_lnkModifyNav = new C1CommandLink(_cmdModifyNav);
		_ctxNav.CommandLinks.Add(_lnkModifyNav);
		_lnkDeleteNav = new C1CommandLink(_cmdDeleteNav);
		_ctxNav.CommandLinks.Add(_lnkDeleteNav);
		_cmdCopy = new C1Command
		{
			Text = "复制",
			Image = ContextResources.ctxCopy
		};
		_cmdCopy.Click += _cmdCopy_Click;
		_cmdCopy.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _grid.BodyRow >= 0 && _grid.BodyCol >= 0;
		};
		_cmdCut = new C1Command
		{
			Text = "剪切",
			Image = ContextResources.ctxCut
		};
		_cmdCut.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			if (_isTicketLocked)
			{
				e.Visible = false;
			}
			else
			{
				bool visible2 = _grid.BodyRow >= 0 && _grid.BodyCol >= 0;
				if (Table != null && Table.IsLocked)
				{
					visible2 = false;
				}
				e.Visible = visible2;
			}
		};
		_cmdCut.Click += _cmdCut_Click;
		_cmdPaste = new C1Command
		{
			Text = "粘贴",
			Image = ContextResources.ctxPaste
		};
		_cmdPaste.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			if (_isTicketLocked)
			{
				e.Visible = false;
			}
			else
			{
				bool visible = _grid.BodyRow >= 0 && _grid.BodyCol >= 0;
				if (Table != null && Table.IsLocked)
				{
					visible = false;
				}
				e.Visible = visible;
			}
		};
		_cmdPaste.Click += _cmdPaste_Click;
		_cmdInsertDataRow = new C1Command
		{
			Text = "插入行...",
			Image = ContextResources.ctxInsertRow
		};
		_cmdInsertDataRow.CommandStateQuery += _cmdInsertDataRow_CommandStateQuery;
		_cmdInsertDataRow.Click += _cmdInsertDataRow_Click;
		_cmdAppendDataRow = new C1Command
		{
			Text = "追加行...",
			Image = ContextResources.ctxAppendRow
		};
		_cmdAppendDataRow.CommandStateQuery += _cmdAppendDataRow_CommandStateQuery;
		_cmdAppendDataRow.Click += _cmdAppendDataRow_Click;
		_cmdRemoveDataRow = new C1Command
		{
			Text = "删除行",
			Image = ContextResources.ctxDeleteRow
		};
		_cmdRemoveDataRow.CommandStateQuery += _cmdRemoveDataRow_CommandStateQuery;
		_cmdRemoveDataRow.Click += _cmdRemoveDataRow_Click;
		_cmdRowHeight = new C1Command
		{
			Text = "设置行高..."
		};
		_cmdRowHeight.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _grid.BodyRow >= 0 && _grid.BodyCol >= 0;
		};
		_cmdRowHeight.Click += _cmdRowHeight_Click;
		_cmdColumnWidth = new C1Command
		{
			Text = "设置列宽..."
		};
		_cmdColumnWidth.Click += _cmdColumnWidth_Click;
		_cmdColumnWidth.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _grid.BodyRow >= 0 && _grid.BodyCol >= 0;
		};
		_cmdAddAttachment = new C1Command
		{
			Text = "插入单元格附件",
			Image = Resources.ctxAttachment
		};
		_cmdAddAttachment.CommandStateQuery += _cmdAddAttachment_CommandStateQuery;
		_cmdAddAttachment.Click += _cmdAddAttachment_Click;
		_cmdRemoveAttachment = new C1Command
		{
			Text = "删除单元格附件",
			Image = Resources.ctxRemoveAttachment
		};
		_cmdRemoveAttachment.CommandStateQuery += _cmdRemoveAttachment_CommandStateQuery;
		_cmdRemoveAttachment.Click += _cmdRemoveAttachment_Click;
		_cmdExportAttachment = new C1Command
		{
			Text = "导出单元格附件"
		};
		_cmdExportAttachment.CommandStateQuery += _cmdExportAttachment_CommandStateQuery;
		_cmdExportAttachment.Click += _cmdExportAttachment_Click;
		_cmdFrozenRow = new C1Command
		{
			Text = "冻结行",
			Image = Resources.ctxFreezeRow
		};
		_cmdFrozenRow.Click += delegate
		{
			_vm.Table.Ticket.TableRowsFrozenCount = _grid.BodyRowSel + 1;
			_grid.Rows.Frozen = _vm.Table.Ticket.TableRowsFrozenCount;
			_vm.Table.TagTicketDirty(isCanOverrideByServerData: true);
			_grid.Invalidate();
		};
		_cmdFrozenRow.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _grid.BodyRowSel >= 0;
		};
		_cmdUnFrozenRow = new C1Command
		{
			Text = "解冻行"
		};
		_cmdUnFrozenRow.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _vm.Table.Ticket.TableRowsFrozenCount > 0 && _grid.BodyRowSel >= 0;
		};
		_cmdUnFrozenRow.Click += delegate
		{
			_vm.Table.Ticket.TableRowsFrozenCount = 0;
			_grid.Rows.Frozen = 0;
			_vm.Table.TagTicketDirty(isCanOverrideByServerData: true);
			_grid.Invalidate();
		};
		_cmdFrozenCol = new C1Command
		{
			Text = "冻结列",
			Image = Resources.ctxFreezeCol
		};
		_cmdFrozenCol.Click += delegate
		{
			_vm.Table.Ticket.TableColsFrozenCount = _grid.BodyColSel + 1;
			_grid.Cols.Frozen = _vm.Table.Ticket.TableColsFrozenCount;
			_vm.Table.TagTicketDirty(isCanOverrideByServerData: true);
			_grid.Invalidate();
		};
		_cmdFrozenCol.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _grid.BodyColSel >= 0;
		};
		_cmdUnFrozenCol = new C1Command
		{
			Text = "解冻列"
		};
		_cmdUnFrozenCol.CommandStateQuery += delegate(object s, CommandStateQueryEventArgs e)
		{
			e.Visible = _vm.Table.Ticket.TableColsFrozenCount > 0 && _grid.BodyColSel >= 0;
		};
		_cmdUnFrozenCol.Click += delegate
		{
			_vm.Table.Ticket.TableColsFrozenCount = 0;
			_grid.Cols.Frozen = 0;
			_vm.Table.TagTicketDirty(isCanOverrideByServerData: true);
			_grid.Invalidate();
		};
		_cmdSetColHeader = new C1Command
		{
			Text = "设置为列头"
		};
		_cmdSetColHeader.CommandStateQuery += _cmdSetColHeader_CommandStateQuery;
		_cmdSetColHeader.Click += _cmdSetColHeader_Click;
		_cmdCancelColHeader = new C1Command
		{
			Text = "取消列头"
		};
		_cmdCancelColHeader.CommandStateQuery += _cmdCancelColHeader_CommandStateQuery;
		_cmdCancelColHeader.Click += _cmdCancelColHeader_Click;
		_cmdHideColumn = new C1Command
		{
			Text = "隐藏列"
		};
		_cmdHideColumn.CommandStateQuery += _cmdHideColumn_CommandStateQuery;
		_cmdHideColumn.Click += _cmdHideColumn_Click;
		_cmdShowColumn = new C1Command
		{
			Text = "寻回列"
		};
		_cmdShowColumn.CommandStateQuery += _cmdShowColumn_CommandStateQuery;
		_cmdShowColumn.Click += _cmdShowColumn_Click;
		_cmdMoveUpDataRow = new C1Command
		{
			Text = "上移行"
		};
		_cmdMoveUpDataRow.CommandStateQuery += _cmdMoveUpDataRow_CommandStateQuery;
		_cmdMoveUpDataRow.Click += _cmdMoveUpDataRow_Click;
		_cmdMoveDownDataRow = new C1Command
		{
			Text = "下移行"
		};
		_cmdMoveDownDataRow.CommandStateQuery += _cmdMoveDownDataRow_CommandStateQuery;
		_cmdMoveDownDataRow.Click += _cmdMoveDownDataRow_Click;
		_cmdMoveTopestDataRow = new C1Command
		{
			Text = "移至首行"
		};
		_cmdMoveTopestDataRow.CommandStateQuery += _cmdMoveTopestDataRow_CommandStateQuery;
		_cmdMoveTopestDataRow.Click += _cmdMoveTopestDataRow_Click;
		_cmdMoveBottomestDataRow = new C1Command
		{
			Text = "移至末行"
		};
		_cmdMoveBottomestDataRow.CommandStateQuery += _cmdMoveBottomestDataRow_CommandStateQuery;
		_cmdMoveBottomestDataRow.Click += _cmdMoveBottomestDataRow_Click;
		_lnkCopy = new C1CommandLink(_cmdCopy);
		_lnkCut = new C1CommandLink(_cmdCut);
		_lnkPaste = new C1CommandLink(_cmdPaste);
		_lnkInsertDataRow = new C1CommandLink(_cmdInsertDataRow)
		{
			Delimiter = true
		};
		_lnkAppendDataRow = new C1CommandLink(_cmdAppendDataRow);
		_lnkRemoveDataRow = new C1CommandLink(_cmdRemoveDataRow);
		_lnkRowHeight = new C1CommandLink(_cmdRowHeight)
		{
			Delimiter = true
		};
		_lnkColumnWidth = new C1CommandLink(_cmdColumnWidth);
		_lnkAddAttachment = new C1CommandLink(_cmdAddAttachment)
		{
			Delimiter = true
		};
		_lnkRemoveAttachment = new C1CommandLink(_cmdRemoveAttachment)
		{
			Delimiter = true
		};
		_lnkExportAttachment = new C1CommandLink(_cmdExportAttachment);
		_ctxCell = new C1ContextMenu();
		_ctxCell.CommandLinks.Add(_lnkCopy);
		_ctxCell.CommandLinks.Add(_lnkCut);
		_ctxCell.CommandLinks.Add(_lnkPaste);
		_ctxCell.CommandLinks.Add(_lnkInsertDataRow);
		_ctxCell.CommandLinks.Add(_lnkAppendDataRow);
		_ctxCell.CommandLinks.Add(_lnkRemoveDataRow);
		_ctxCell.CommandLinks.Add(_lnkRowHeight);
		_ctxCell.CommandLinks.Add(_lnkColumnWidth);
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdHideColumn)
		{
			Delimiter = true
		});
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdShowColumn));
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdFrozenCol));
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdUnFrozenCol));
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdSetColHeader)
		{
			Delimiter = true
		});
		_ctxCell.CommandLinks.Add(new C1CommandLink(_cmdCancelColHeader));
		_ctxCell.CommandLinks.Add(_lnkAddAttachment);
		_ctxCell.CommandLinks.Add(_lnkRemoveAttachment);
		_ctxCell.CommandLinks.Add(_lnkExportAttachment);
		_ctxEmpty = new C1ContextMenu();
		C1Command c1Command = new C1Command();
		c1Command.Text = "追加行...";
		c1Command.Image = ContextResources.ctxAppendRow;
		c1Command.CommandStateQuery += _cmdAppendDataRow_CommandStateQuery;
		c1Command.Click += _cmdAppendDataRow_Click;
		_ctxEmpty.CommandLinks.Add(new C1CommandLink(c1Command));
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "寻回列";
		c1Command2.CommandStateQuery += _emptyContextShowColumn_CommandStateQuery;
		c1Command2.Click += _cmdShowColumn_Click;
		_ctxEmpty.CommandLinks.Add(new C1CommandLink(c1Command2));
		_ctxLock = new C1ContextMenu();
		_ctxLock.CommandLinks.Add(new C1CommandLink(_cmdExportAttachment));
		_ctxRowHeader = new C1ContextMenu();
		_ctxRowHeader.CommandLinks.Add(new C1CommandLink(_cmdInsertDataRow));
		_ctxRowHeader.CommandLinks.Add(new C1CommandLink(_cmdAppendDataRow));
		_ctxRowHeader.CommandLinks.Add(new C1CommandLink(_cmdRemoveDataRow));
		_lnkRowHeaderSetRowHeight = new C1CommandLink(_cmdRowHeight)
		{
			Delimiter = true
		};
		_ctxRowHeader.CommandLinks.Add(_lnkRowHeaderSetRowHeight);
		_ctxRowHeader.CommandLinks.Add(new C1CommandLink(_cmdMoveUpDataRow)
		{
			Delimiter = true
		});
		_ctxRowHeader.CommandLinks.Add(new C1CommandLink(_cmdMoveDownDataRow));
		_ctxRowHeader.CommandLinks.Add(new C1CommandLink(_cmdMoveTopestDataRow));
		_ctxRowHeader.CommandLinks.Add(new C1CommandLink(_cmdMoveBottomestDataRow));
		_editorPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Top,
			KeepRelativeSize = false,
			DoubleBuffered = true
		};
		_btnInsertRow = new C1Button
		{
			Dock = DockStyle.None,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Image = Resources.TicketInputInsertRow,
			Visible = false,
			FlatStyle = FlatStyle.Flat
		};
		_btnInsertRow.FlatAppearance.BorderSize = 0;
		_btnInsertRow.Click += _btnInsertRow_Click;
		_btnRemoveRow = new C1Button
		{
			Dock = DockStyle.None,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			Image = Resources.TicketInputRemoveRow,
			Visible = false,
			FlatStyle = FlatStyle.Flat
		};
		_btnRemoveRow.FlatAppearance.BorderSize = 0;
		_btnRemoveRow.Click += _btnRemoveRow_Click;
		_splc.Panels.Add(_editorPanel);
		_editorPanel.Controls.Add(_grid);
		_editorPanel.Controls.Add(_btnInsertRow);
		_editorPanel.Controls.Add(_btnRemoveRow);
		_editorPanel.Paint += _editorPanel_Paint;
		_editorPanel.Resize += _pnlMain_Resize;
		_editorPanel.MouseClick += _pnlMain_MouseClick;
		ListDropDown = new ListDropDown(_grid);
		InputListDropDown = new InputListDropDown(_grid);
		ListDropDown.DropDown.ButtonCursor = Cursors.Arrow;
		InputListDropDown.DropDown.ButtonCursor = Cursors.Arrow;
		_imageProcess = new RibbonImageProcess();
		foreach (C1CommandLink commandLink in _tbr.CommandLinks)
		{
			_imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		ThemeManager.GetInstance().Register(this);
		frmSearchTicketRecord.SelectNode += FrmSearchTicketRecord_SelectNode;
		_lazySearchExcute.SetAction(delegate
		{
			frmSearchTicketRecord.SetKeyword("");
			frmSearchTicketRecord.TicketNavGrids = _navGrids.Select((TicketNavGrid n) => Tuple.Create(n, _navGridPagesDic[n].Item2)).ToList();
			frmSearchTicketRecord.UpdateDisplay();
			frmSearchTicketRecord.ShowDialog();
		});
		_titleEditor = new TicketInputTitleFooterEditor(this, TicketInputTitleFooterEditor.EditorType.Title);
		_footerEditor = new TicketInputTitleFooterEditor(this, TicketInputTitleFooterEditor.EditorType.Footer);
		_editorPanel.Controls.Add(_titleEditor.View);
		_editorPanel.Controls.Add(_footerEditor.View);
		_grid.BringToFront();
		_gridDecorator = new TicketGridDecorator(this);
		_timerWarningHighlight.Tick += _timerWarningHighlight_Tick;
		_timerWarningHighlight.Start();
		MemberManager.GetInstance().OpenTicketNavTreeNodeChanged += TicketInputEditor_OpenTicketNavTreeNodeChanged;
		InitShadowData();
	}

	private void _cmdMoveBottomestDataRow_Click(object sender, ClickEventArgs e)
	{
		if (Ticket.Kind != TicketKind.DynamicRow && Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return;
		}
		int topRow = _grid.Selection.TopRow;
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		int num = ConvertGridRowIndexToVMRowIndex(topRow);
		int num2 = ConvertGridRowIndexToVMRowIndex(selection.BottomRow);
		int rowsCount = num2 - num + 1;
		int num3 = 0;
		int afterMoveFistRowIndex = num;
		Point scrollPosition = _grid.ScrollPosition;
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			num3 = _vm.MoveDataRowToBottom_DynamicRowTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		else
		{
			if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
			{
				return;
			}
			num3 = _vm.MoveDataRowToBottom_MixTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		_grid.SuspendDrawing();
		int vmRowIndex = afterMoveFistRowIndex;
		int vmRowIndex2 = afterMoveFistRowIndex + num3 - 1;
		int topRow2 = ConvertVMRowIndexToGridRowIndex(vmRowIndex);
		int buttonRow = ConvertVMRowIndexToGridRowIndex(vmRowIndex2);
		_grid.SafeSelect(topRow2, selection.LeftCol, buttonRow, selection.RightCol);
		if (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Horizontal))
		{
			Point scrollPosition2 = _grid.ScrollPosition;
			scrollPosition2.X = scrollPosition.X;
			try
			{
				_grid.ScrollPosition = scrollPosition2;
			}
			catch
			{
			}
		}
		_grid.Invalidate();
		_grid.ResumeDrawing();
		ShowRecordButtons();
		SetCommandState();
	}

	private void _cmdMoveBottomestDataRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
			return;
		}
		int topRow = _grid.Selection.TopRow;
		int index = ConvertGridRowIndexToVMRowIndex(topRow);
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			e.Visible = false;
		}
		else if (Ticket.Kind == TicketKind.DynamicRow)
		{
			e.Visible = _vm.IsRecordDataRow_DynamicRowTicket(index) && !HasFillingFormula;
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			e.Visible = _vm.GetRow(index).TicketRow.IsMixRangeDynamicDataRow && !HasFillingFormula;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void _cmdMoveTopestDataRow_Click(object sender, ClickEventArgs e)
	{
		if (Ticket.Kind != TicketKind.DynamicRow && Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return;
		}
		int topRow = _grid.Selection.TopRow;
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		int num = ConvertGridRowIndexToVMRowIndex(topRow);
		int num2 = ConvertGridRowIndexToVMRowIndex(selection.BottomRow);
		int rowsCount = num2 - num + 1;
		int num3 = 0;
		int afterMoveFistRowIndex = num;
		Point scrollPosition = _grid.ScrollPosition;
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			num3 = _vm.MoveDataRowToTop_DynamicRowTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		else
		{
			if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
			{
				return;
			}
			num3 = _vm.MoveDataRowToTop_MixTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		_grid.SuspendDrawing();
		int vmRowIndex = afterMoveFistRowIndex;
		int vmRowIndex2 = afterMoveFistRowIndex + num3 - 1;
		int topRow2 = ConvertVMRowIndexToGridRowIndex(vmRowIndex);
		int buttonRow = ConvertVMRowIndexToGridRowIndex(vmRowIndex2);
		_grid.SafeSelect(topRow2, selection.LeftCol, buttonRow, selection.RightCol);
		if (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Horizontal))
		{
			Point scrollPosition2 = _grid.ScrollPosition;
			scrollPosition2.X = scrollPosition.X;
			try
			{
				_grid.ScrollPosition = scrollPosition2;
			}
			catch
			{
			}
		}
		_grid.Invalidate();
		_grid.ResumeDrawing();
		ShowRecordButtons();
		SetCommandState();
	}

	private void _cmdMoveTopestDataRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
			return;
		}
		int topRow = _grid.Selection.TopRow;
		int index = ConvertGridRowIndexToVMRowIndex(topRow);
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			e.Visible = false;
		}
		else if (Ticket.Kind == TicketKind.DynamicRow)
		{
			e.Visible = _vm.IsRecordDataRow_DynamicRowTicket(index) && !HasFillingFormula;
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			e.Visible = _vm.GetRow(index).TicketRow.IsMixRangeDynamicDataRow && !HasFillingFormula;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void _cmdMoveDownDataRow_Click(object sender, ClickEventArgs e)
	{
		if (Ticket.Kind != TicketKind.DynamicRow && Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return;
		}
		int topRow = _grid.Selection.TopRow;
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		int num = ConvertGridRowIndexToVMRowIndex(topRow);
		int num2 = ConvertGridRowIndexToVMRowIndex(selection.BottomRow);
		int rowsCount = num2 - num + 1;
		int num3 = 0;
		int afterMoveFistRowIndex = num;
		Point scrollPosition = _grid.ScrollPosition;
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			num3 = _vm.MoveDownDataRow_DynamicRowTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		else
		{
			if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
			{
				return;
			}
			num3 = _vm.MoveDownDataRow_MixTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		_grid.SuspendDrawing();
		int topRow2 = ConvertVMRowIndexToGridRowIndex(afterMoveFistRowIndex);
		int buttonRow = ConvertVMRowIndexToGridRowIndex(afterMoveFistRowIndex + num3 - 1);
		_grid.SafeSelect(topRow2, selection.LeftCol, buttonRow, selection.RightCol);
		if (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Horizontal))
		{
			Point scrollPosition2 = _grid.ScrollPosition;
			scrollPosition2.X = scrollPosition.X;
			try
			{
				_grid.ScrollPosition = scrollPosition2;
			}
			catch
			{
			}
		}
		_grid.Invalidate();
		_grid.ResumeDrawing();
		ShowRecordButtons();
		SetCommandState();
	}

	private void _cmdMoveDownDataRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
			return;
		}
		int topRow = _grid.Selection.TopRow;
		int index = ConvertGridRowIndexToVMRowIndex(topRow);
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			e.Visible = false;
		}
		else if (Ticket.Kind == TicketKind.DynamicRow)
		{
			e.Visible = _vm.IsRecordDataRow_DynamicRowTicket(index) && !HasFillingFormula;
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			e.Visible = _vm.GetRow(index).TicketRow.IsMixRangeDynamicDataRow && !HasFillingFormula;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void _cmdMoveUpDataRow_Click(object sender, ClickEventArgs e)
	{
		if (Ticket.Kind != TicketKind.DynamicRow && Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return;
		}
		int topRow = _grid.Selection.TopRow;
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (!selection.IsValid)
		{
			return;
		}
		int num = ConvertGridRowIndexToVMRowIndex(topRow);
		int num2 = ConvertGridRowIndexToVMRowIndex(selection.BottomRow);
		int rowsCount = num2 - num + 1;
		int num3 = 0;
		int afterMoveFistRowIndex = num;
		Point scrollPosition = _grid.ScrollPosition;
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			num3 = _vm.MoveUpDataRow_DynamicRowTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		else
		{
			if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
			{
				return;
			}
			num3 = _vm.MoveUpDataRow_MixTicket(num, rowsCount, out afterMoveFistRowIndex);
			if (num3 <= 0)
			{
				return;
			}
		}
		_grid.SuspendDrawing();
		int topRow2 = ConvertVMRowIndexToGridRowIndex(afterMoveFistRowIndex);
		int buttonRow = ConvertVMRowIndexToGridRowIndex(afterMoveFistRowIndex + num3 - 1);
		_grid.SafeSelect(topRow2, selection.LeftCol, buttonRow, selection.RightCol);
		if (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Horizontal))
		{
			Point scrollPosition2 = _grid.ScrollPosition;
			scrollPosition2.X = scrollPosition.X;
			try
			{
				_grid.ScrollPosition = scrollPosition2;
			}
			catch
			{
			}
		}
		_grid.Invalidate();
		_grid.ResumeDrawing();
		ShowRecordButtons();
		SetCommandState();
	}

	private void _cmdMoveUpDataRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
			return;
		}
		int topRow = _grid.Selection.TopRow;
		int index = ConvertGridRowIndexToVMRowIndex(topRow);
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			e.Visible = false;
		}
		else if (Ticket.Kind == TicketKind.DynamicRow)
		{
			e.Visible = _vm.IsRecordDataRow_DynamicRowTicket(index) && !HasFillingFormula;
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			e.Visible = _vm.GetRow(index).TicketRow.IsMixRangeDynamicDataRow && !HasFillingFormula;
		}
		else
		{
			e.Visible = false;
		}
	}

	public bool IsMoveTicketRowButtonVisible()
	{
		if (Table == null)
		{
			return false;
		}
		if (Ticket.Kind != TicketKind.DynamicRow && Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return false;
		}
		return true;
	}

	public void MoveUpTicketRow()
	{
		_cmdMoveUpDataRow_Click(null, ClickEventArgs.Empty);
	}

	public void MoveDownTicketRow()
	{
		_cmdMoveDownDataRow_Click(null, ClickEventArgs.Empty);
	}

	private void InitShadowData()
	{
		for (int i = 0; i < _shadowAreaRangeArr.Length; i++)
		{
			_shadowAreaRangeArr[i] = new Rectangle(0, 0, -1, -1);
		}
		int num = Resources.ticketShadow.Width / 2;
		int num2 = Resources.ticketShadow.Height / 2;
		int num3 = 16;
		int num4 = num - num3;
		int num5 = Resources.ticketShadow.Width - num4 - 32;
		int num6 = num2 - num3;
		int num7 = Resources.ticketShadow.Height - num6 - 32;
		if (num4 > 0 && num5 > 0 && num6 > 0 && num7 > 0)
		{
			_shadowAreaImageArr[3] = Resources.ticketShadow.Clone(new Rectangle(0, 0, num4, num6), Resources.ticketShadow.PixelFormat);
			_shadowAreaImageArr[1] = Resources.ticketShadow.Clone(new Rectangle(num4, 0, 32, num6), Resources.ticketShadow.PixelFormat);
			_shadowAreaImageArr[6] = Resources.ticketShadow.Clone(new Rectangle(num4 + 32, 0, num5, num6), Resources.ticketShadow.PixelFormat);
			_shadowAreaImageArr[4] = Resources.ticketShadow.Clone(new Rectangle(0, num6, num5, 32), Resources.ticketShadow.PixelFormat);
			_shadowAreaImageArr[7] = Resources.ticketShadow.Clone(new Rectangle(num4 + 32, num6, num5, 32), Resources.ticketShadow.PixelFormat);
			_shadowAreaImageArr[5] = Resources.ticketShadow.Clone(new Rectangle(0, num6 + 32, num4, num7), Resources.ticketShadow.PixelFormat);
			_shadowAreaImageArr[2] = Resources.ticketShadow.Clone(new Rectangle(num4, num6 + 32, 32, num7), Resources.ticketShadow.PixelFormat);
			_shadowAreaImageArr[8] = Resources.ticketShadow.Clone(new Rectangle(num4 + 32, num6 + 32, num5, num7), Resources.ticketShadow.PixelFormat);
		}
	}

	private void _editorPanel_Paint(object sender, PaintEventArgs e)
	{
		if (!_isNeedPaintShadow)
		{
			return;
		}
		InterpolationMode interpolationMode = e.Graphics.InterpolationMode;
		PixelOffsetMode pixelOffsetMode = e.Graphics.PixelOffsetMode;
		e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
		e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		for (int i = 0; i < _shadowAreaImageArr.Length; i++)
		{
			System.Drawing.Image image = _shadowAreaImageArr[i];
			if (image != null)
			{
				Rectangle rect = _shadowAreaRangeArr[i];
				if (rect.Width >= 0 && rect.Height >= 0)
				{
					e.Graphics.DrawImage(image, rect);
				}
			}
		}
		e.Graphics.InterpolationMode = interpolationMode;
		e.Graphics.PixelOffsetMode = pixelOffsetMode;
	}

	private void _cmdShowColumn_Click(object sender, ClickEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		ShowColumnsSelector showColumnsSelector = new ShowColumnsSelector();
		if (showColumnsSelector.ShowDialog(Ticket) != DialogResult.OK || showColumnsSelector.Selected.Count() == 0)
		{
			return;
		}
		SuspendDrawing();
		try
		{
			foreach (ColumnsSetting item in showColumnsSelector.Selected)
			{
				Ticket.Columns[item.ColumIndex].IsHiddenColumn = false;
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			PopulateVm();
		}
		finally
		{
			ResumeDrawing();
		}
	}

	private void _emptyContextShowColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = Table != null && Ticket.Columns.Any((TicketColumn u) => u.IsHiddenColumn) && Ticket.Columns.Count((TicketColumn u) => !u.IsHiddenColumn) == 0;
	}

	private void _cmdShowColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = Table != null && Ticket.Columns.Any((TicketColumn u) => u.IsHiddenColumn);
	}

	private void _cmdHideColumn_Click(object sender, ClickEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		int leftCol = _grid.Selection.LeftCol;
		int rowIndex = 0;
		int colIndex = ConvertGridColIndexToVMColIndex(leftCol);
		if (_vm.IsIndexOutOfRange(rowIndex, colIndex))
		{
			return;
		}
		SuspendDrawing();
		try
		{
			for (int i = _grid.Selection.LeftCol; i <= _grid.Selection.RightCol; i++)
			{
				if (!_grid.IsIndexOutOfRange(0, i) && _grid.Visible)
				{
					colIndex = ConvertGridColIndexToVMColIndex(i);
					if (!_vm.IsIndexOutOfRange(rowIndex, colIndex))
					{
						_vm.GetColumn(colIndex).IsHiddenColumn = true;
					}
				}
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			PopulateVm();
		}
		finally
		{
			ResumeDrawing();
		}
	}

	private void _cmdHideColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = Table != null && _grid.Selection.LeftCol >= _grid.Cols.Fixed;
	}

	private void _cmdCancelColHeader_Click(object sender, ClickEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		_ticketGridColumnMerges = null;
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		_vm.Table.Ticket.ColumnHeaderRowsCount = 0;
		_vm.Table.TagTicketDirty(isCanOverrideByServerData: true);
		_grid.BeginUpdate();
		try
		{
			_grid.Rows.Fixed = 0;
			ShowFilterContextMenu(isShow: false);
			if (_grid.FilterManager.IsFiltering)
			{
				_grid.FilterManager.Clear();
			}
			_grid.FilterManager.ResetGridColumnMergeRange();
			_grid.Select(selection);
			_grid.Invalidate();
		}
		catch (Exception)
		{
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	private void _cmdCancelColHeader_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Table == null)
		{
			e.Enabled = false;
		}
		else
		{
			e.Enabled = _vm.Table.Ticket.ColumnHeaderRowsCount > 0 && IsAllowAdjustColumnHeader();
		}
	}

	private void _cmdSetColHeader_Click(object sender, ClickEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.BottomRow < 0)
		{
			return;
		}
		_ticketGridColumnMerges = null;
		_vm.Table.Ticket.ColumnHeaderRowsCount = _grid.Selection.BottomRow + 1;
		_vm.Table.TagTicketDirty(isCanOverrideByServerData: true);
		try
		{
			_grid.Rows.Fixed = _vm.Table.Ticket.ColumnHeaderRowsCount;
			ShowFilterContextMenu(_vm.Table.Ticket.ColumnHeaderRowsCount > 0);
			_grid.FilterManager.ResetGridColumnMergeRange();
			_grid.Select(selection);
			_grid.Invalidate();
		}
		catch (Exception)
		{
		}
	}

	private void _cmdSetColHeader_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		bool enabled = false;
		try
		{
			if (Table == null || !IsAllowAdjustColumnHeader())
			{
				return;
			}
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			if (selection.BottomRow < 0 || (Ticket.Kind == TicketKind.DynamicRow && selection.BottomRow >= Ticket.DataRowStart))
			{
				return;
			}
			int columnsCount = _vm.GetColumnsCount();
			for (int i = 0; i <= selection.BottomRow; i++)
			{
				for (int j = 0; j < columnsCount; j++)
				{
					TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
					if (cellVM.TicketCell.HasField() || cellVM.TicketCell.HasFormula())
					{
						return;
					}
				}
			}
			int num = selection.BottomRow + 1;
			foreach (TicketMerge merge in _vm.Merges)
			{
				if (merge.TopRow < num && merge.BottomRow >= num)
				{
					return;
				}
			}
			enabled = true;
		}
		catch (Exception)
		{
		}
		finally
		{
			e.Enabled = enabled;
		}
	}

	private bool IsAllowAdjustColumnHeader()
	{
		if (Table == null)
		{
			return false;
		}
		if (!Table.TreeNode.HasWritePermission())
		{
			return false;
		}
		if (!Table.TreeNode.HasSchemaPermission())
		{
			return false;
		}
		return true;
	}

	public void SaveCurrentSelectdRecordByRowId()
	{
		if (_navGrids.Count <= 0 || Table == null)
		{
			return;
		}
		TicketRecord selectedRecord = (TicketRecord)_currentNavGrid.SelectedRecord;
		if (selectedRecord == null || selectedRecord.Rows.Count <= 0)
		{
			return;
		}
		foreach (Auditai.Model.Row row in selectedRecord.Rows)
		{
			if (Table.ContainsRow(row))
			{
				TicketNavTreeStatusDataCacher.SaveNavTreeSelectedRecordIndex(Table.Id, _currentRecord, isSelectByRowId: true, row.Id.Value);
				break;
			}
		}
	}

	private void _radioButtonOpenTicketView_CheckedChanged(object sender, EventArgs e)
	{
		if (!_isSuspendViewChangeModeCheckEvent && _radioButtonOpenTicketView.Checked)
		{
			if (Program.MainForm.CurrentView == MainFormView.TablePreview)
			{
				_isSuspendViewChangeModeCheckEvent = true;
				_radioButtonOpenTableView.Checked = true;
				_radioButtonOpenTicketView.Checked = false;
				_isSuspendViewChangeModeCheckEvent = false;
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请退出预览模式后再进行切换！");
			}
			else
			{
				SaveCurrentSelectdRecordByRowId();
				Program.MainForm.TableEditor.SwitchToTicketInputMode();
			}
		}
	}

	private void _radioButtonOpenTableView_CheckedChanged(object sender, EventArgs e)
	{
		if (!_isSuspendViewChangeModeCheckEvent && _radioButtonOpenTableView.Checked)
		{
			if (Program.MainForm.CurrentView == MainFormView.TicketPrint)
			{
				_isSuspendViewChangeModeCheckEvent = true;
				_radioButtonOpenTableView.Checked = false;
				_radioButtonOpenTicketView.Checked = true;
				_isSuspendViewChangeModeCheckEvent = false;
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请退出预览模式后再进行切换！");
			}
			else if (Program.MainForm.CurrentView == MainFormView.TicketInput)
			{
				SwitchToTableMode();
				JumpToCurrentRecordAvailableTableRow();
			}
		}
	}

	private void _switchViewPanel_SizeChanged(object sender, EventArgs e)
	{
		ReLayoutSwitchViewPanel();
		SetSwitchViewPanelBackgroundBrush();
		_switchViewPanel.Invalidate();
	}

	private void ReLayoutSwitchViewPanel()
	{
		int num = 0;
		Control[] array = new Control[2] { _radioButtonOpenTableView, _radioButtonOpenTicketView };
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Top = (_switchViewPanel.Size.Height - array[i].Height) / 2;
		}
		int num2 = _switchViewPanel.Size.Width - num;
		int num3 = array[0].Width + array[1].Width;
		if (num2 <= num3)
		{
			array[0].Left = num;
			array[1].Left = num + array[0].Width;
		}
		else
		{
			int num4 = (num2 - num3) / 3;
			array[0].Left = num + num4;
			array[1].Left = num + num4 + array[0].Width + num4;
		}
	}

	private bool _switchViewPanelBackground_Paint(object sender, PaintEventArgs e)
	{
		if (_switchViewPanelBackgroundBrush != null)
		{
			e.Graphics.FillRectangle(_switchViewPanelBackgroundBrush, _switchViewPanel.ClientRectangle);
			return true;
		}
		return false;
	}

	private void _openViewRadioButtonBackground_Paint(object sender, PaintEventArgs e)
	{
		if (sender is Control { Location: var location } && _switchViewPanelBackgroundBrush != null)
		{
			Rectangle clipRectangle = e.ClipRectangle;
			clipRectangle.X--;
			clipRectangle.Y -= location.Y + 1;
			clipRectangle.Width++;
			clipRectangle.Height = _switchViewPanel.Height;
			e.Graphics.FillRectangle(_switchViewPanelBackgroundBrush, clipRectangle);
		}
	}

	private void _grid_MouseLeave(object sender, EventArgs e)
	{
		_isMouseOverCancelManualInputIcon = false;
		_isMouseOverRowNumberColumn = false;
		_isMouseOverShowMoreMenuImage = false;
		_mouseOverHeaderColumn = -1;
		_grid.Invalidate();
	}

	private void UpdateWarningTextColor()
	{
		if (_warningUpdateTimes % 2 == 1)
		{
			_remindTextColor = TableEditor.RemindColor;
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
			FooterEditor.View.Invalidate();
		}
	}

	private void TicketInputEditor_OpenTicketNavTreeNodeChanged(object sender, Tuple<string, string, string, string> e)
	{
		_gridDecorator.HandleOpenNavTreeNodeDelay(e.Item1);
	}

	public TicketNavGrid GetTicketNavByOpenPath(string openPath)
	{
		if (_navGrids != null)
		{
			foreach (TicketNavGrid navGrid in _navGrids)
			{
				if (navGrid.IsNavTreeNodeOpenPathMatchedCurrentNavTree(openPath))
				{
					return navGrid;
				}
			}
		}
		return null;
	}

	public C1FlexGridEx GetCurrentInShowingNavTreeGrid()
	{
		int selectedIndex = _otbNavs.SelectedIndex;
		if (selectedIndex >= 0 && selectedIndex < _navGrids.Count)
		{
			return _currentNavGrid.View;
		}
		return null;
	}

	public TicketNavGrid GetCurrentInShowingNavTree()
	{
		int selectedIndex = _otbNavs.SelectedIndex;
		if (selectedIndex >= 0 && selectedIndex < _navGrids.Count)
		{
			return _currentNavGrid;
		}
		return null;
	}

	public C1OutPage GetNavTreeGridPage(TicketNavGrid navGrid)
	{
		if (_navGrids == null)
		{
			return null;
		}
		for (int i = 0; i < _navGrids.Count; i++)
		{
			if (_navGrids[i] == navGrid)
			{
				return _otbNavs.Pages[i];
			}
		}
		return null;
	}

	private void FilterManager_AfterFilterExecute(object sender, EventArgs e)
	{
		if (_isSuspendFilterManager_AfterFilterExecuteEvent)
		{
			return;
		}
		SuspendDrawing();
		try
		{
			AutoAdjustInputGridPosition();
		}
		finally
		{
			ResumeDrawing();
		}
	}

	public void RefreshNavTreeNodeFlickImage()
	{
		_gridDecorator.RefreshNavTreeNodeFlickImageWithoutRebuild();
	}

	public void FlagSomeUserStayedCellChanged()
	{
		_gridDecorator.SetCellFlickerDirty();
	}

	public void HandleOpenNavTreeNodeDelay(string userId)
	{
		_gridDecorator.HandleOpenNavTreeNodeDelay(userId);
	}

	private void FrmSearchTicketRecord_SelectNode(object sender, Tuple<Node, TicketNavGrid> e)
	{
		if (!_navGridPagesDic.TryGetValue(e.Item2, out var value))
		{
			return;
		}
		int num = _otbNavs.Pages.IndexOf(value.Item1);
		if (num < 0)
		{
			return;
		}
		SuspendDrawing();
		try
		{
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			_otbNavs.SelectedIndex = num;
			e.Item2.ClickToShowRow(e.Item1.Row.Index);
		}
		catch
		{
		}
		finally
		{
			ResumeDrawing();
		}
	}

	private void _cmdExportTicket_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.TicketInputEditor.ExportXlsx();
	}

	private void _cmdDesignTicket_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsAllowDesignTicket())
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void _cmdDesignTicket_Click(object sender, ClickEventArgs e)
	{
		SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
		Program.MainForm.TableEditor.Table.Ticket.Level = TicketLevel.Report;
		Program.MainForm.SwitchToTicketDesignView();
	}

	private void _cmdCollectFill_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (!SoftwareLicenseManager.IsLedgerModuleEnable() || Program.ClientPlatformType != PlatformType.AuditPlatform)
		{
			e.Visible = false;
		}
		else if (Table == null || Ticket == null || Ticket.Kind != TicketKind.DynamicRow)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private async void Cmd_Click_CollectFill(object sender, ClickEventArgs e)
	{
		if (Ticket.Kind != TicketKind.DynamicRow)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前类型的表单不支持采账填充!");
			return;
		}
		SaveGridSelectRangeAndScrollPosition();
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		ChangeVirtualValueToRealValue();
		await Program.MainForm.TicketCollectSet(CreateCollectDataTable());
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
		RestoreGridSelectRangeAndScrollPosition();
	}

	private TicketCollectFillTable CreateCollectDataTable()
	{
		TicketCollectFillTable vmTable = new TicketCollectFillTable(_vm.Table);
		vmTable.CollectPageTitleData = CreateTitleGrid();
		vmTable.CollectPageTableHeaderData = CreateTableColumnHeaderGrid();
		InitTitle();
		InitColumn((C1FlexGrid)vmTable.CollectPageTableHeaderData);
		return vmTable;
		C1FlexGrid CreateTableColumnHeaderGrid()
		{
			C1FlexGrid c1FlexGrid = new C1FlexGrid();
			int columnsCount = _vm.GetColumnsCount();
			c1FlexGrid.Cols.Count = columnsCount;
			for (int num = 0; num < columnsCount; num++)
			{
				int index = ConvertVMColIndexToGridColIndex(num);
				c1FlexGrid.Cols[num].Width = _grid.Cols[index].Width;
				c1FlexGrid.Cols[num].Visible = _grid.Cols[index].Visible;
			}
			int num2 = 0;
			if (Ticket.TableRowsFrozenCount > 0 && Ticket.DataRowStart > 0)
			{
				num2 = Math.Min(Ticket.TableRowsFrozenCount, Ticket.DataRowStart);
			}
			else if (Ticket.DataRowStart > 0)
			{
				num2 = Ticket.DataRowStart;
			}
			else if (Ticket.TableRowsFrozenCount > 0)
			{
				num2 = Ticket.TableRowsFrozenCount;
			}
			List<Tuple<TicketMerge, TicketMerge>> list = new List<Tuple<TicketMerge, TicketMerge>>();
			foreach (C1.Win.C1FlexGrid.CellRange item3 in (IEnumerable)_grid.MergedRanges)
			{
				if (item3.TopRow < num2)
				{
					TicketMerge item = new TicketMerge
					{
						TopRow = item3.TopRow,
						LeftColumn = ConvertGridColIndexToVMColIndex(item3.LeftCol),
						BottomRow = item3.BottomRow,
						RightColumn = ConvertGridColIndexToVMColIndex(item3.RightCol)
					};
					TicketMerge ticketMerge = new TicketMerge
					{
						TopRow = item3.TopRow,
						LeftColumn = ConvertGridColIndexToVMColIndex(item3.LeftCol),
						BottomRow = item3.BottomRow,
						RightColumn = ConvertGridColIndexToVMColIndex(item3.RightCol)
					};
					if (item3.BottomRow >= num2)
					{
						ticketMerge.BottomRow = num2 - 1;
					}
					list.Add(Tuple.Create(item, ticketMerge));
				}
			}
			int num3 = 0;
			c1FlexGrid.Rows.Count = 0;
			int num4 = 0;
			List<bool> list2 = new List<bool>(num2);
			for (int num5 = 0; num5 < num2; num5++)
			{
				for (int num6 = 0; num6 < columnsCount; num6++)
				{
					TicketCell cell = Ticket.GetCell(num5, num6);
					list2.Add(item: false);
					if (!string.IsNullOrEmpty(cell.Formula) && cell.Formula.StartsWith("sum", StringComparison.OrdinalIgnoreCase))
					{
						list2[num5] = true;
						num4++;
					}
				}
			}
			for (int num7 = 0; num7 < num2; num7++)
			{
				if (list2[num7] && (num4 != num2 || num7 != 0))
				{
					foreach (Tuple<TicketMerge, TicketMerge> item4 in list)
					{
						if (num7 >= item4.Item1.TopRow && num7 <= item4.Item1.BottomRow)
						{
							item4.Item2.BottomRow--;
						}
					}
				}
				else
				{
					C1.Win.C1FlexGrid.Row row = c1FlexGrid.Rows.Add();
					row.Height = _grid.Rows[num7].Height;
					for (int num8 = 0; num8 < columnsCount; num8++)
					{
						c1FlexGrid.SetData(num3, num8, _vm.GetCellVM(num7, num8).GetDisplayValue());
					}
					num3++;
				}
			}
			try
			{
				foreach (Tuple<TicketMerge, TicketMerge> item5 in list)
				{
					TicketMerge item2 = item5.Item2;
					c1FlexGrid.MergedRanges.Add(item2.TopRow, item2.LeftColumn, item2.BottomRow, item2.RightColumn);
				}
			}
			catch (Exception exception)
			{
				exception.Log("表单采账填充时处理合并单元格失败");
			}
			if (c1FlexGrid.Rows.Count == 0)
			{
				C1.Win.C1FlexGrid.Row row2 = c1FlexGrid.Rows.Add();
				row2.Height = 30;
				for (int num9 = 0; num9 < columnsCount; num9++)
				{
					c1FlexGrid.SetData(0, num9, string.Empty);
				}
			}
			return c1FlexGrid;
		}
		C1FlexGrid CreateTitleGrid()
		{
			C1FlexGrid view = TitleEditor.View;
			TicketInputTitleFooterVM vMData = TitleEditor.VMData;
			C1FlexGrid c1FlexGrid2 = new C1FlexGrid();
			int count3 = view.Rows.Count;
			int count4 = view.Cols.Count;
			c1FlexGrid2.Rows.Count = count3;
			c1FlexGrid2.Cols.Count = count4;
			for (int num10 = 0; num10 < count3; num10++)
			{
				for (int num11 = 0; num11 < count4; num11++)
				{
					TicketInputCellVM cellVM2 = vMData.GetCellVM(num10, num11);
					c1FlexGrid2[num10, num11] = cellVM2.GetDisplayValue();
					c1FlexGrid2.SetCellStyle(num10, num11, view.GetCellStyle(num10, num11));
					c1FlexGrid2.SetUserData(num10, num11, new TicketCollectFillTable.TableCellUserData
					{
						tableColumnDataFormat = ((cellVM2.Column != null) ? new DataFormat?(cellVM2.Column.GetFormat()) : null)
					});
				}
			}
			for (int num12 = 0; num12 < count3; num12++)
			{
				c1FlexGrid2.Rows[num12].Height = view.Rows[num12].Height;
			}
			for (int num13 = 0; num13 < count4; num13++)
			{
				c1FlexGrid2.Cols[num13].Width = view.Cols[num13].Width;
			}
			foreach (C1.Win.C1FlexGrid.CellRange item6 in (IEnumerable)view.MergedRanges)
			{
				c1FlexGrid2.MergedRanges.Add(item6);
			}
			return c1FlexGrid2;
		}
		void InitColumn(C1FlexGrid tableHeaderGrid)
		{
			int count = tableHeaderGrid.Rows.Count;
			int count2 = tableHeaderGrid.Cols.Count;
			vmTable.InitColumns(count2);
			for (int i = 0; i < count2; i++)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int j = 0; j < count; j++)
				{
					object data = tableHeaderGrid.GetData(j, i);
					if (data != null)
					{
						stringBuilder.Append(data.ToString() + "_");
					}
				}
				string text = stringBuilder.ToString();
				if (text.EndsWith("_"))
				{
					text = text.Substring(0, text.Length - 1);
				}
				vmTable.SetColumnCaption(i, text);
			}
		}
		void InitTitle()
		{
			int rowsCount = _vm.Title.Rows.Count;
			int colsCount = _vm.Title.Columns.Count;
			vmTable.SetMainTitleValue(GetMainTitle());
			vmTable.InitSubTitle(rowsCount, colsCount);
			for (int k = 0; k < rowsCount; k++)
			{
				for (int l = 0; l < colsCount; l++)
				{
					vmTable.SetSubTitleValue(k, l, _vm.Title.GetCellVM(k, l).GetDisplayValue());
				}
			}
			string GetMainTitle()
			{
				for (int m = 0; m < rowsCount; m++)
				{
					for (int n = 0; n < colsCount; n++)
					{
						TicketInputCellVM cellVM = _vm.Title.GetCellVM(m, n);
						string displayValue = cellVM.GetDisplayValue();
						if (!string.IsNullOrEmpty(displayValue))
						{
							return displayValue;
						}
					}
				}
				return string.Empty;
			}
		}
	}

	private void _grid_Enter(object sender, EventArgs e)
	{
		EnterEdit();
	}

	private bool GetCollectFill2Visible()
	{
		if (Program.ClientPlatformType == PlatformType.AuditPlatform && Auditai.Model.User.Current != null && Auditai.Model.User.Current.IsSystemSupporter)
		{
			return true;
		}
		if (Program.ClientPlatformType == PlatformType.AuditPlatform)
		{
			return false;
		}
		if (_vm != null)
		{
			return _vm.IsExistLedgerCollectFormula;
		}
		return true;
	}

	private void _cmdCollectFill2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = GetCollectFill2Visible();
	}

	private void _cmdCollectFill2_Click(object sender, ClickEventArgs e)
	{
		if (_vm == null || Table == null || Table.IsLocked)
		{
			return;
		}
		bool isFailedDueToEmtpyLedger = false;
		bool isBalanceVirtualTableBuildFailed = false;
		bool isVoucherVirtualTableBuildFailed = false;
		LedgerVirtualTable balanceVirtualTable = null;
		LedgerVirtualTable voucherVirtualTable = null;
		SaveGridSelectRangeAndScrollPosition();
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SuspendDrawing();
		try
		{
			CacheSelectRange();
			ChangeVirtualValueToRealValue();
			_vm.BuildTableCellForAllTicketCell();
			if (IsAllowExecuteCollectFormulaOnCurrentTicket())
			{
				try
				{
					_vm.BeginBatchUpdateValue();
					_vm.StartRecordNewAddTableRows();
					_grid.FilterManager.Clear();
					ClearRecordFilterSetting();
					ExcuteFixedCellFormula();
					ExcuteDynamicDataRowFormula();
				}
				catch (FormulaBadReferenceException exception)
				{
					if (!isFailedDueToEmtpyLedger)
					{
						exception.Log("账套采集公式运算失败");
					}
				}
				catch (Exception ex)
				{
					ex.Log();
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账套采集公式运算失败:\r\n" + ex.ToString());
				}
				finally
				{
					_vm.EndBatchUpdateValue();
				}
			}
			_vm.CalculateTicket();
			PopulateVm();
			_grid.Invalidate();
			TitleEditor.Invalidate();
			FooterEditor.Invalidate();
			RestoreSelectRange();
			SetCommandState();
			if (isFailedDueToEmtpyLedger)
			{
				Program.MainForm.ShowOpenLedgerTip();
			}
			else if (isBalanceVirtualTableBuildFailed)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "从账套的科目余额表中读取数据失败!");
			}
			else if (isVoucherVirtualTableBuildFailed)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "从账套的会计凭证表中读取数据失败!");
			}
		}
		catch (Exception ex2)
		{
			ex2.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "采数公式运算失败:\r\n" + ex2.ToString());
		}
		finally
		{
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
		}
		void ClearTicketColumnCellValue(int vmStartRowIndex, int rowsCount, int colIndex)
		{
			int rowsCount2 = _vm.GetRowsCount();
			for (int n = 0; n < rowsCount; n++)
			{
				int num6 = vmStartRowIndex + n;
				if (num6 >= rowsCount2)
				{
					break;
				}
				TicketInputCellVM cellVM7 = _vm.GetCellVM(num6, colIndex);
				if (cellVM7.TableCell != null && CanEditCell(cellVM7))
				{
					object newValue4 = Auditai.Model.Cell.ChangeDataTypeImpl(string.Empty, cellVM7.TableCell.DisplayDataType);
					_vm.UpdateTicketCellValue(cellVM7, newValue4, isFormulaExistManualInputValue: false);
				}
			}
		}
		void ExcuteDataRowFormula_DynamicRowTicket()
		{
			List<TicketColumnLedgerCollectFormulaSetting> list6 = new List<TicketColumnLedgerCollectFormulaSetting>();
			for (int num14 = 0; num14 < Ticket.Columns.Count; num14++)
			{
				TicketColumn ticketColumn = Ticket.Columns[num14];
				if (ticketColumn.HasField())
				{
					Auditai.Model.Column byId = Table.Columns.GetById(ticketColumn.Field);
					if (byId != null && !byId.IsLocked)
					{
						DataFormat format3 = byId.GetFormat();
						if (format3.HasLedgerCollectFormula && TableEditor.IsCurrentUserCanEditColumn(byId))
						{
							list6.Add(new TicketColumnLedgerCollectFormulaSetting
							{
								TicketColumnIndex = num14,
								TicketColumn = ticketColumn,
								TableColumn = byId,
								LedgerCollectFormula = format3.LedgerCollectFormula
							});
						}
					}
				}
			}
			if (list6.Count != 0)
			{
				TicketColumnLedgerCollectFormulaSetting ticketColumnLedgerCollectFormulaSetting2 = null;
				foreach (TicketColumnLedgerCollectFormulaSetting item2 in list6)
				{
					FormulaEvaluator formulaEvaluator4 = new FormulaEvaluator(item2.LedgerCollectFormula);
					item2.IsFill = formulaEvaluator4.IsLedgerCollectFillFormula();
					if (ticketColumnLedgerCollectFormulaSetting2 == null && item2.IsFill.IsFill)
					{
						ticketColumnLedgerCollectFormulaSetting2 = item2;
					}
				}
				TicketInputCellVM existTableCellTicketCell = GetExistTableCellTicketCell();
				if (existTableCellTicketCell != null)
				{
					LedgerVirtualTableEvalContext evalContext2 = GetEvalContext();
					if (ticketColumnLedgerCollectFormulaSetting2 != null && !_vm.IsHasFillingFormula)
					{
						try
						{
							_isSuggestKeepNavTreeNodeInTablePosition = true;
							_vm.RemoveDataRows(Ticket.DataRowStart, _vm.DataRowsCount);
							_vm.InsertDataRows(Ticket.DataRowStart, 1);
							_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
							_vm.StartRecordNewAddTableRows();
							FormulaEvaluator formulaEvaluator5 = GenerateFormualEvaluator(ticketColumnLedgerCollectFormulaSetting2.LedgerCollectFormula, existTableCellTicketCell.TableCell.Row.Index);
							Operand operand3 = formulaEvaluator5.EvaluateOnLedgerVirtualTable(evalContext2, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
							List<object> list7 = new List<object>();
							if (operand3 is ValueSetOperand valueSetOperand2)
							{
								List<object> list8 = valueSetOperand2.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> u) => u.Item2.Object).ToList();
								list7 = (ticketColumnLedgerCollectFormulaSetting2.IsFill.IsLqAsc ? list8.OrderByCellValue((object u) => u).ToList() : ((!ticketColumnLedgerCollectFormulaSetting2.IsFill.IsLqDesc) ? list8 : list8.OrderByCellValueDescending((object u) => u).ToList()));
							}
							if (_vm.DataRowsCount < list7.Count)
							{
								_vm.InsertDataRows(Ticket.DataRowStart + _vm.DataRowsCount, list7.Count - _vm.DataRowsCount);
							}
							else if (_vm.DataRowsCount > list7.Count)
							{
								int num15 = _vm.DataRowsCount - list7.Count;
								if (num15 == _vm.DataRowsCount)
								{
									num15 = _vm.DataRowsCount - 1;
								}
								int ticketRowIndex4 = Ticket.DataRowStart + (_vm.DataRowsCount - num15);
								_vm.RemoveDataRows(ticketRowIndex4, num15);
							}
							int num16 = Math.Min(_vm.DataRowsCount, list7.Count);
							_vm.BuildTableRowsForTicketDataRows_DynamicRow(Ticket.DataRowStart, num16);
							for (int num17 = 0; num17 < num16; num17++)
							{
								TicketInputCellVM cellVM11 = _vm.GetCellVM(Ticket.DataRowStart + num17, ticketColumnLedgerCollectFormulaSetting2.TicketColumnIndex);
								if (cellVM11.TableCell != null)
								{
									object newValue5 = Auditai.Model.Cell.ChangeDataTypeImpl(list7[num17], cellVM11.TableCell.DisplayDataType);
									_vm.UpdateTicketCellValue(cellVM11, newValue5, isFormulaExistManualInputValue: false);
								}
							}
						}
						catch (FormulaBadReferenceException exception6)
						{
							if (isFailedDueToEmtpyLedger)
							{
								throw;
							}
							exception6.Log("账套采集公式运算异常:" + ticketColumnLedgerCollectFormulaSetting2.LedgerCollectFormula);
						}
						catch (FormulaBreakExecuteException)
						{
						}
						catch (Exception exception7)
						{
							exception7.Log("账套采集公式运算异常:" + ticketColumnLedgerCollectFormulaSetting2.LedgerCollectFormula);
						}
					}
					else if (!_vm.IsHasFillingFormula)
					{
						_vm.BuildTableRowsForTicketDataRows_DynamicRow(Ticket.DataRowStart, _vm.DataRowsCount);
					}
					_vm.BuildTableRowsForTicketDataRows_DynamicRow(Ticket.DataRowStart, 1);
					List<CollectFormulaDependSettingData> list9 = new List<CollectFormulaDependSettingData>();
					foreach (TicketColumnLedgerCollectFormulaSetting item3 in list6)
					{
						if (!item3.IsFill.IsFill)
						{
							CollectFormulaDependSettingData collectFormulaDependSettingData2 = new CollectFormulaDependSettingData
							{
								Formula = item3.LedgerCollectFormula,
								FormulaComeFromWhichTableColumn = item3.TableColumn,
								FormulaComeFromWhichTableCell = null,
								DependTableCellSet = new HashSet<Auditai.Model.Cell>(),
								DependTableColumnSet = new HashSet<Auditai.Model.Column>(),
								userData = item3
							};
							GetDependSameTableOtherColumnsAndCells(item3.LedgerCollectFormula, collectFormulaDependSettingData2.DependTableColumnSet, collectFormulaDependSettingData2.DependTableCellSet);
							list9.Add(collectFormulaDependSettingData2);
						}
					}
					SortByDpend(list9);
					list6 = list9.Select((CollectFormulaDependSettingData u) => (TicketColumnLedgerCollectFormulaSetting)u.userData).ToList();
					_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
					_vm.StartRecordNewAddTableRows();
					foreach (TicketColumnLedgerCollectFormulaSetting item4 in list6)
					{
						if (!item4.IsFill.IsFill)
						{
							for (int num18 = 0; num18 < _vm.DataRowsCount; num18++)
							{
								TicketInputCellVM cellVM12 = _vm.GetCellVM(Ticket.DataRowStart + num18, item4.TicketColumnIndex);
								if (cellVM12.TableCell != null)
								{
									try
									{
										FormulaEvaluator formulaEvaluator6 = GenerateFormualEvaluator(item4.LedgerCollectFormula, cellVM12.TableCell.Row.Index);
										Operand operand4 = formulaEvaluator6.EvaluateOnLedgerVirtualTable(evalContext2, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
										if (operand4 is CellsOperand { IsCollectFill: not false } cellsOperand2)
										{
											if (!_vm.IsHasFillingFormula)
											{
												_isSuggestKeepNavTreeNodeInTablePosition = true;
												ClearTicketColumnCellValue(Ticket.DataRowStart, _vm.DataRowsCount, item4.TicketColumnIndex);
												List<object> list10 = cellsOperand2.Cells.Select((Auditai.Model.Cell u) => u.Value).ToList();
												if (_vm.DataRowsCount < list10.Count)
												{
													_vm.InsertDataRows(Ticket.DataRowStart + _vm.DataRowsCount, list10.Count - _vm.DataRowsCount);
												}
												else if (_vm.DataRowsCount > list10.Count)
												{
													int num19 = _vm.DataRowsCount - list10.Count;
													if (num19 == _vm.DataRowsCount)
													{
														num19 = _vm.DataRowsCount - 1;
													}
													int ticketRowIndex5 = Ticket.DataRowStart + (_vm.DataRowsCount - num19);
													_vm.RemoveDataRows(ticketRowIndex5, num19);
												}
												int num20 = Math.Min(_vm.DataRowsCount, list10.Count);
												_vm.BuildTableRowsForTicketDataRows_DynamicRow(Ticket.DataRowStart, num20);
												_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
												_vm.StartRecordNewAddTableRows();
												for (int num21 = 0; num21 < num20; num21++)
												{
													TicketInputCellVM cellVM13 = _vm.GetCellVM(Ticket.DataRowStart + num21, item4.TicketColumnIndex);
													if (cellVM13.TableCell != null)
													{
														object newValue6 = Auditai.Model.Cell.ChangeDataTypeImpl(list10[num21], cellVM13.TableCell.DisplayDataType);
														_vm.UpdateTicketCellValue(cellVM13, newValue6, isFormulaExistManualInputValue: false);
													}
												}
											}
											break;
										}
										object value3 = operand4.Evaluate();
										object newValue7 = Auditai.Model.Cell.ChangeDataTypeImpl(value3, cellVM12.TableCell.DisplayDataType);
										_vm.UpdateTicketCellValue(cellVM12, newValue7, isFormulaExistManualInputValue: false);
									}
									catch (FormulaBadReferenceException exception8)
									{
										if (isFailedDueToEmtpyLedger)
										{
											throw;
										}
										exception8.Log("账套采集公式运算异常:" + ticketColumnLedgerCollectFormulaSetting2.LedgerCollectFormula);
									}
									catch (FormulaBreakExecuteException)
									{
										break;
									}
									catch (Exception exception9)
									{
										exception9.Log("账套采集公式运算异常:" + item4.LedgerCollectFormula);
									}
								}
							}
						}
					}
					if (_vm.DataRowsCount < Ticket.DataRowCount)
					{
						_isSuggestKeepNavTreeNodeInTablePosition = true;
						int ticketRowIndex6 = Ticket.DataRowStart + _vm.DataRowsCount;
						int count4 = Ticket.DataRowCount - _vm.DataRowsCount;
						_vm.InsertDataRows(ticketRowIndex6, count4);
						_vm.BuildTableRowsForTicketDataRows_DynamicRow(Ticket.DataRowStart, _vm.DataRowsCount);
					}
				}
			}
		}
		void ExcuteDataRowFormula_MixTicket()
		{
			List<MixTicketDynamicDataRowRangeSetting> mixTicketDynamicDataRowRangesDesc = GetMixTicketDynamicDataRowRangesDesc();
			foreach (MixTicketDynamicDataRowRangeSetting item5 in mixTicketDynamicDataRowRangesDesc)
			{
				_vm.StartRecordNewAddTableRows();
				ExcuteDataRowRangeFormula_MixTicket(item5);
			}
		}
		void ExcuteDataRowRangeFormula_MixTicket(MixTicketDynamicDataRowRangeSetting dynamicRange)
		{
			if (dynamicRange.DataRowsCount != 0)
			{
				List<TicketColumnLedgerCollectFormulaSetting> list = new List<TicketColumnLedgerCollectFormulaSetting>();
				int columnsCount = _vm.GetColumnsCount();
				for (int i = 0; i < columnsCount; i++)
				{
					TicketInputCellVM cellVM = _vm.GetCellVM(dynamicRange.DataRowStartIndex, i);
					if (cellVM.Column != null)
					{
						Auditai.Model.Column column = cellVM.Column;
						if (column != null && !column.IsLocked)
						{
							DataFormat format = column.GetFormat();
							if (format.HasLedgerCollectFormula && TableEditor.IsCurrentUserCanEditColumn(column))
							{
								list.Add(new TicketColumnLedgerCollectFormulaSetting
								{
									TicketColumnIndex = i,
									TicketColumn = Ticket.Columns[i],
									TableColumn = column,
									LedgerCollectFormula = format.LedgerCollectFormula
								});
							}
						}
					}
				}
				if (list.Count != 0)
				{
					TicketColumnLedgerCollectFormulaSetting ticketColumnLedgerCollectFormulaSetting = null;
					foreach (TicketColumnLedgerCollectFormulaSetting item6 in list)
					{
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(item6.LedgerCollectFormula);
						item6.IsFill = formulaEvaluator.IsLedgerCollectFillFormula();
						if (ticketColumnLedgerCollectFormulaSetting == null && item6.IsFill.IsFill)
						{
							ticketColumnLedgerCollectFormulaSetting = item6;
						}
					}
					_vm.BuildTableRowsForTicketDataRows_MixTicket(dynamicRange.DataRowStartIndex, 1);
					TicketInputCellVM ticketInputCellVM = null;
					for (int j = 0; j < columnsCount; j++)
					{
						TicketInputCellVM cellVM2 = _vm.GetCellVM(dynamicRange.DataRowStartIndex, j);
						if (cellVM2.TableCell != null)
						{
							ticketInputCellVM = cellVM2;
							break;
						}
					}
					if (ticketInputCellVM != null)
					{
						LedgerVirtualTableEvalContext evalContext = GetEvalContext();
						if (ticketColumnLedgerCollectFormulaSetting != null && !_vm.IsHasFillingFormula)
						{
							try
							{
								_isSuggestKeepNavTreeNodeInTablePosition = true;
								_vm.RemoveDataRows(dynamicRange.DataRowStartIndex, dynamicRange.DataRowsCount);
								_vm.InsertDataRows_MixTicket(dynamicRange.DataRowStartIndex, 1, dynamicRange.DataRowTemplateId);
								dynamicRange.DataRowsCount = 1;
								_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
								_vm.StartRecordNewAddTableRows();
								FormulaEvaluator formulaEvaluator2 = GenerateFormualEvaluator(ticketColumnLedgerCollectFormulaSetting.LedgerCollectFormula, ticketInputCellVM.TableCell.Row.Index);
								Operand operand = formulaEvaluator2.EvaluateOnLedgerVirtualTable(evalContext, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
								List<object> list2 = new List<object>();
								if (operand is ValueSetOperand valueSetOperand)
								{
									List<object> list3 = valueSetOperand.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> u) => u.Item2.Object).ToList();
									list2 = (ticketColumnLedgerCollectFormulaSetting.IsFill.IsLqAsc ? list3.OrderByCellValue((object u) => u).ToList() : ((!ticketColumnLedgerCollectFormulaSetting.IsFill.IsLqDesc) ? list3 : list3.OrderByCellValueDescending((object u) => u).ToList()));
									List<int> mixTicketMixRangeFixedRowsIndex = _vm.GetMixTicketMixRangeFixedRowsIndex(dynamicRange.DataRowStartIndex);
									if (mixTicketMixRangeFixedRowsIndex.Count > 0)
									{
										HashSet<string> hashSet = new HashSet<string>();
										foreach (int item7 in mixTicketMixRangeFixedRowsIndex)
										{
											int mixTicketColumnWhichRefTableColumn = _vm.GetMixTicketColumnWhichRefTableColumn(item7, ticketColumnLedgerCollectFormulaSetting.TableColumn);
											if (mixTicketColumnWhichRefTableColumn != -1)
											{
												TicketInputCellVM cellVM3 = _vm.GetCellVM(item7, mixTicketColumnWhichRefTableColumn);
												if (cellVM3 != null)
												{
													if (cellVM3.TicketCell != null && !string.IsNullOrEmpty(cellVM3.TicketCell.GetInputValue()))
													{
														hashSet.Add(cellVM3.TicketCell.GetInputValue());
													}
													else
													{
														string displayValue = cellVM3.GetDisplayValue();
														if (!string.IsNullOrEmpty(displayValue))
														{
															hashSet.Add(displayValue);
														}
													}
												}
											}
										}
										Type dataType = ticketColumnLedgerCollectFormulaSetting.TableColumn.GetDataType();
										DataFormat format2 = ticketColumnLedgerCollectFormulaSetting.TableColumn.GetFormat();
										for (int num = list2.Count - 1; num >= 0; num--)
										{
											try
											{
												object value = Auditai.Model.Cell.ChangeDataTypeImpl(list2[num], dataType);
												string displayValueImpl = Auditai.Model.Cell.GetDisplayValueImpl(value, format2);
												if (hashSet.Contains(displayValueImpl))
												{
													list2.RemoveAt(num);
												}
											}
											catch
											{
											}
										}
									}
								}
								if (dynamicRange.DataRowsCount < list2.Count)
								{
									_vm.InsertDataRows_MixTicket(dynamicRange.DataRowStartIndex + dynamicRange.DataRowsCount, list2.Count - dynamicRange.DataRowsCount, dynamicRange.DataRowTemplateId);
									dynamicRange.DataRowsCount = list2.Count;
								}
								else if (dynamicRange.DataRowsCount > list2.Count)
								{
									int num2 = dynamicRange.DataRowsCount - list2.Count;
									if (num2 == dynamicRange.DataRowsCount)
									{
										num2 = dynamicRange.DataRowsCount - 1;
									}
									int ticketRowIndex = dynamicRange.DataRowStartIndex + (dynamicRange.DataRowsCount - num2);
									_vm.RemoveDataRows(ticketRowIndex, num2);
									dynamicRange.DataRowsCount -= num2;
								}
								int num3 = Math.Min(dynamicRange.DataRowsCount, list2.Count);
								_vm.BuildTableRowsForTicketDataRows_MixTicket(dynamicRange.DataRowStartIndex, num3);
								for (int k = 0; k < num3; k++)
								{
									TicketInputCellVM cellVM4 = _vm.GetCellVM(dynamicRange.DataRowStartIndex + k, ticketColumnLedgerCollectFormulaSetting.TicketColumnIndex);
									if (cellVM4.TableCell != null)
									{
										object newValue = Auditai.Model.Cell.ChangeDataTypeImpl(list2[k], cellVM4.TableCell.DisplayDataType);
										_vm.UpdateTicketCellValue(cellVM4, newValue, isFormulaExistManualInputValue: false);
									}
								}
							}
							catch (FormulaBadReferenceException exception2)
							{
								if (isFailedDueToEmtpyLedger)
								{
									throw;
								}
								exception2.Log("账套采集公式运算异常:" + ticketColumnLedgerCollectFormulaSetting.LedgerCollectFormula);
							}
							catch (FormulaBreakExecuteException)
							{
							}
							catch (Exception exception3)
							{
								exception3.Log("账套采集公式运算异常:" + ticketColumnLedgerCollectFormulaSetting.LedgerCollectFormula);
							}
						}
						else if (!_vm.IsHasFillingFormula)
						{
							_vm.BuildTableRowsForTicketDataRows_MixTicket(dynamicRange.DataRowStartIndex, dynamicRange.DataRowsCount);
						}
						_vm.BuildTableRowsForTicketDataRows_MixTicket(dynamicRange.DataRowStartIndex, 1);
						_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
						_vm.StartRecordNewAddTableRows();
						List<CollectFormulaDependSettingData> list4 = new List<CollectFormulaDependSettingData>();
						foreach (TicketColumnLedgerCollectFormulaSetting item8 in list)
						{
							if (!item8.IsFill.IsFill)
							{
								CollectFormulaDependSettingData collectFormulaDependSettingData = new CollectFormulaDependSettingData
								{
									Formula = item8.LedgerCollectFormula,
									FormulaComeFromWhichTableColumn = item8.TableColumn,
									FormulaComeFromWhichTableCell = null,
									DependTableCellSet = new HashSet<Auditai.Model.Cell>(),
									DependTableColumnSet = new HashSet<Auditai.Model.Column>(),
									userData = item8
								};
								GetDependSameTableOtherColumnsAndCells(item8.LedgerCollectFormula, collectFormulaDependSettingData.DependTableColumnSet, collectFormulaDependSettingData.DependTableCellSet);
								list4.Add(collectFormulaDependSettingData);
							}
						}
						SortByDpend(list4);
						list = list4.Select((CollectFormulaDependSettingData u) => (TicketColumnLedgerCollectFormulaSetting)u.userData).ToList();
						foreach (TicketColumnLedgerCollectFormulaSetting item9 in list)
						{
							if (!item9.IsFill.IsFill)
							{
								for (int l = 0; l < dynamicRange.DataRowsCount; l++)
								{
									TicketInputCellVM cellVM5 = _vm.GetCellVM(dynamicRange.DataRowStartIndex + l, item9.TicketColumnIndex);
									if (cellVM5.TableCell != null)
									{
										try
										{
											FormulaEvaluator formulaEvaluator3 = GenerateFormualEvaluator(item9.LedgerCollectFormula, cellVM5.TableCell.Row.Index);
											Operand operand2 = formulaEvaluator3.EvaluateOnLedgerVirtualTable(evalContext, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
											if (operand2 is CellsOperand { IsCollectFill: not false } cellsOperand)
											{
												if (!_vm.IsHasFillingFormula)
												{
													_isSuggestKeepNavTreeNodeInTablePosition = true;
													ClearTicketColumnCellValue(dynamicRange.DataRowStartIndex, dynamicRange.DataRowsCount, item9.TicketColumnIndex);
													List<object> list5 = cellsOperand.Cells.Select((Auditai.Model.Cell u) => u.Value).ToList();
													if (dynamicRange.DataRowsCount < list5.Count)
													{
														_vm.InsertDataRows_MixTicket(dynamicRange.DataRowStartIndex + dynamicRange.DataRowsCount, list5.Count - dynamicRange.DataRowsCount, dynamicRange.DataRowTemplateId);
														dynamicRange.DataRowsCount = list5.Count;
													}
													else if (dynamicRange.DataRowsCount > list5.Count)
													{
														int num4 = dynamicRange.DataRowsCount - list5.Count;
														if (num4 == dynamicRange.DataRowsCount)
														{
															num4 = dynamicRange.DataRowsCount - 1;
														}
														int ticketRowIndex2 = dynamicRange.DataRowStartIndex + (dynamicRange.DataRowsCount - num4);
														_vm.RemoveDataRows(ticketRowIndex2, num4);
														dynamicRange.DataRowsCount -= num4;
													}
													int num5 = Math.Min(dynamicRange.DataRowsCount, list5.Count);
													_vm.BuildTableRowsForTicketDataRows_MixTicket(dynamicRange.DataRowStartIndex, num5);
													_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
													_vm.StartRecordNewAddTableRows();
													for (int m = 0; m < num5; m++)
													{
														TicketInputCellVM cellVM6 = _vm.GetCellVM(dynamicRange.DataRowStartIndex + m, item9.TicketColumnIndex);
														if (cellVM6.TableCell != null)
														{
															object newValue2 = Auditai.Model.Cell.ChangeDataTypeImpl(list5[m], cellVM6.TableCell.DisplayDataType);
															_vm.UpdateTicketCellValue(cellVM6, newValue2, isFormulaExistManualInputValue: false);
														}
													}
												}
												break;
											}
											object value2 = operand2.Evaluate();
											object newValue3 = Auditai.Model.Cell.ChangeDataTypeImpl(value2, cellVM5.TableCell.DisplayDataType);
											_vm.UpdateTicketCellValue(cellVM5, newValue3, isFormulaExistManualInputValue: false);
										}
										catch (FormulaBadReferenceException exception4)
										{
											if (isFailedDueToEmtpyLedger)
											{
												throw;
											}
											exception4.Log("账套采集公式运算异常:" + ticketColumnLedgerCollectFormulaSetting.LedgerCollectFormula);
										}
										catch (FormulaBreakExecuteException)
										{
											break;
										}
										catch (Exception exception5)
										{
											exception5.Log("账套采集公式运算异常:" + item9.LedgerCollectFormula);
										}
									}
								}
							}
						}
						if (dynamicRange.DataRowsCount < dynamicRange.DesignModeDataRowsCount)
						{
							_isSuggestKeepNavTreeNodeInTablePosition = true;
							int ticketRowIndex3 = dynamicRange.DataRowStartIndex + dynamicRange.DataRowsCount;
							int count = dynamicRange.DesignModeDataRowsCount - dynamicRange.DataRowsCount;
							_vm.InsertDataRows_MixTicket(ticketRowIndex3, count, dynamicRange.DataRowTemplateId);
							dynamicRange.DataRowsCount = dynamicRange.DesignModeDataRowsCount;
							_vm.BuildTableRowsForTicketDataRows_MixTicket(dynamicRange.DataRowStartIndex, dynamicRange.DataRowsCount);
						}
					}
				}
			}
		}
		void ExcuteDynamicDataRowFormula()
		{
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				ExcuteDataRowFormula_DynamicRowTicket();
			}
			else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				ExcuteDataRowFormula_MixTicket();
			}
		}
		void ExcuteFixedCellFormula()
		{
			List<Tuple<TicketInputCellVM, string>> list13 = new List<Tuple<TicketInputCellVM, string>>();
			Dictionary<TicketInputCellVM, Auditai.Model.Cell> dictionary3 = new Dictionary<TicketInputCellVM, Auditai.Model.Cell>();
			_vm.StartRecordNewAddTableRows();
			int rowsCount5 = _vm.GetRowsCount();
			int columnsCount4 = _vm.GetColumnsCount();
			for (int num23 = 0; num23 < rowsCount5; num23++)
			{
				TicketInputRowVM row2 = _vm.GetRow(num23);
				if (Ticket.Kind == TicketKind.DynamicRow)
				{
					if (row2.IsDynamicRowTicketDataRow)
					{
						continue;
					}
				}
				else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow && row2.IsMixTicketDynamicDataRow)
				{
					continue;
				}
				for (int num24 = 0; num24 < columnsCount4; num24++)
				{
					TicketInputCellVM cellVM14 = _vm.GetCellVM(num23, num24);
					if (cellVM14.TicketCell != null && cellVM14.Column != null)
					{
						DataFormat format4 = cellVM14.Column.GetFormat();
						if (format4.HasLedgerCollectFormula && CanEditCell(cellVM14))
						{
							_vm.BuildTableCellForTicketCell(num23, num24);
							list13.Add(Tuple.Create(cellVM14, format4.LedgerCollectFormula));
						}
					}
				}
			}
			TicketInputTitleFooterVM[] array3 = new TicketInputTitleFooterVM[2] { _vm.Title, _vm.Footer };
			Func<TicketInputCellVM, bool>[] array4 = new Func<TicketInputCellVM, bool>[2] { TitleEditor.CanEditCellValue, FooterEditor.CanEditCellValue };
			for (int num25 = 0; num25 < array3.Length; num25++)
			{
				TicketInputTitleFooterVM ticketInputTitleFooterVM2 = array3[num25];
				if (ticketInputTitleFooterVM2 != null)
				{
					Func<TicketInputCellVM, bool> func2 = array4[num25];
					int count5 = ticketInputTitleFooterVM2.Rows.Count;
					int count6 = ticketInputTitleFooterVM2.Columns.Count;
					for (int num26 = 0; num26 < count5; num26++)
					{
						for (int num27 = 0; num27 < count6; num27++)
						{
							TicketInputCellVM cellVM15 = ticketInputTitleFooterVM2.GetCellVM(num26, num27);
							if (cellVM15.TicketCell != null && cellVM15.Column != null)
							{
								DataFormat format5 = cellVM15.Column.GetFormat();
								if (format5.HasLedgerCollectFormula && func2(cellVM15))
								{
									_vm.BuildTableCellForTicketTitleFooterCell(cellVM15);
									list13.Add(Tuple.Create(cellVM15, format5.LedgerCollectFormula));
								}
							}
						}
					}
				}
			}
			if (list13.Count == 0)
			{
				return;
			}
			_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
			List<CollectFormulaDependSettingData> list14 = new List<CollectFormulaDependSettingData>();
			foreach (Tuple<TicketInputCellVM, string> item10 in list13)
			{
				if (item10.Item1.TableCell != null)
				{
					Auditai.Model.Cell cell = null;
					Auditai.Model.Column column2 = null;
					if (dictionary3.TryGetValue(item10.Item1, out var value10))
					{
						cell = value10;
						column2 = null;
					}
					else
					{
						cell = null;
						column2 = item10.Item1.TableCell.Column;
					}
					CollectFormulaDependSettingData collectFormulaDependSettingData4 = new CollectFormulaDependSettingData
					{
						Formula = item10.Item2,
						FormulaComeFromWhichTableCell = cell,
						FormulaComeFromWhichTableColumn = column2,
						DependTableCellSet = new HashSet<Auditai.Model.Cell>(),
						DependTableColumnSet = new HashSet<Auditai.Model.Column>(),
						userData = item10
					};
					GetDependSameTableOtherColumnsAndCells(item10.Item2, collectFormulaDependSettingData4.DependTableColumnSet, collectFormulaDependSettingData4.DependTableCellSet);
					list14.Add(collectFormulaDependSettingData4);
				}
			}
			SortByDpend(list14);
			list13 = list14.Select((CollectFormulaDependSettingData u) => (Tuple<TicketInputCellVM, string>)u.userData).ToList();
			LedgerVirtualTableEvalContext evalContext3 = GetEvalContext();
			foreach (Tuple<TicketInputCellVM, string> item11 in list13)
			{
				try
				{
					if (item11.Item1.TableCell != null)
					{
						FormulaEvaluator formulaEvaluator8 = GenerateFormualEvaluator(item11.Item2, item11.Item1.TableCell.Row.Index);
						Operand operand5 = formulaEvaluator8.EvaluateOnLedgerVirtualTable(evalContext3, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
						object value11 = operand5.Evaluate();
						object newValue8 = Auditai.Model.Cell.ChangeDataTypeImpl(value11, item11.Item1.TableCell.DisplayDataType);
						_vm.UpdateTicketCellValue(item11.Item1, newValue8, isFormulaExistManualInputValue: false);
					}
				}
				catch (FormulaBadReferenceException exception10)
				{
					if (isFailedDueToEmtpyLedger)
					{
						throw;
					}
					exception10.Log("账套采集公式运算异常:" + item11.Item2);
				}
				catch (Exception exception11)
				{
					exception11.Log("账套采集公式运算异常:" + item11.Item2);
				}
			}
		}
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
		void GetDependSameTableOtherColumnsAndCells(string formula, HashSet<Auditai.Model.Column> colSet, HashSet<Auditai.Model.Cell> cellSet)
		{
			FormulaEvaluator formulaEvaluator7 = new FormulaEvaluator(formula);
			formulaEvaluator7.GetReferredTableColumnsAndCells(Table, out var referredColumns, out var referredCells);
			if (referredColumns != null && referredColumns.Count > 0)
			{
				foreach (Auditai.Model.Column item12 in referredColumns)
				{
					colSet.Add(item12);
				}
			}
			if (referredCells != null && referredCells.Count > 0)
			{
				foreach (Auditai.Model.Cell item13 in referredCells)
				{
					cellSet.Add(item13);
				}
			}
		}
		LedgerVirtualTableEvalContext GetEvalContext()
		{
			return new LedgerVirtualTableEvalContext
			{
				BalanceTable_ResolveColumn = delegate(Id64 colId)
				{
					if (isBalanceVirtualTableBuildFailed)
					{
						return (CellsOperand)null;
					}
					if (balanceVirtualTable == null)
					{
						if (Program.MainForm.IsLedgerEmpty())
						{
							isFailedDueToEmtpyLedger = true;
							return (CellsOperand)null;
						}
						balanceVirtualTable = LedgerVirtualTableUtils.GetBalanceVirtualTable(Program.MainForm.CurrentLedgerViewer);
						if (balanceVirtualTable == null)
						{
							isBalanceVirtualTableBuildFailed = true;
							return (CellsOperand)null;
						}
						if (balanceVirtualTable.DefaultStyle == null)
						{
							balanceVirtualTable.SetDefaultStyle(Ticket.Table.DefaultStyle);
						}
					}
					int num29 = (int)colId.Value;
					return (num29 >= 0 && num29 < balanceVirtualTable.Columns.Count) ? new LedgerVirtualTableColumnOperand(balanceVirtualTable.Columns[num29]) : null;
				},
				VoucherTable_ResolveColumn = delegate(Id64 colId)
				{
					if (isVoucherVirtualTableBuildFailed)
					{
						return (CellsOperand)null;
					}
					if (voucherVirtualTable == null)
					{
						if (Program.MainForm.IsLedgerEmpty())
						{
							isFailedDueToEmtpyLedger = true;
							return (CellsOperand)null;
						}
						voucherVirtualTable = LedgerVirtualTableUtils.GetVoucherVirtualTable(Program.MainForm.CurrentLedgerViewer.Ledger);
						if (voucherVirtualTable == null)
						{
							isVoucherVirtualTableBuildFailed = true;
							return (CellsOperand)null;
						}
						if (voucherVirtualTable.DefaultStyle == null)
						{
							voucherVirtualTable.SetDefaultStyle(Ticket.Table.DefaultStyle);
						}
					}
					int num28 = (int)colId.Value;
					return (num28 >= 0 && num28 < voucherVirtualTable.Columns.Count) ? new LedgerVirtualTableColumnOperand(voucherVirtualTable.Columns[num28]) : null;
				}
			};
		}
		TicketInputCellVM GetExistTableCellTicketCell()
		{
			TicketInputCellVM ticketInputCellVM2 = null;
			int cellRowIndex = 0;
			int cellColIndex = 0;
			bool flag = false;
			TicketInputTitleFooterVM[] array = new TicketInputTitleFooterVM[2] { _vm.Title, _vm.Footer };
			Func<TicketInputCellVM, bool>[] array2 = new Func<TicketInputCellVM, bool>[2] { TitleEditor.CanEditCellValue, FooterEditor.CanEditCellValue };
			for (int num7 = 0; num7 < array.Length; num7++)
			{
				TicketInputTitleFooterVM ticketInputTitleFooterVM = array[num7];
				if (ticketInputTitleFooterVM != null)
				{
					Func<TicketInputCellVM, bool> func = array2[num7];
					int count2 = ticketInputTitleFooterVM.Rows.Count;
					int count3 = ticketInputTitleFooterVM.Columns.Count;
					for (int num8 = 0; num8 < count2; num8++)
					{
						for (int num9 = 0; num9 < count3; num9++)
						{
							TicketInputCellVM cellVM8 = ticketInputTitleFooterVM.GetCellVM(num8, num9);
							if (cellVM8.TableCell != null)
							{
								return cellVM8;
							}
							if (ticketInputCellVM2 == null && cellVM8.IsField && func(cellVM8))
							{
								ticketInputCellVM2 = cellVM8;
								flag = true;
								cellRowIndex = num8;
								cellColIndex = num9;
							}
						}
					}
				}
			}
			int rowsCount3 = _vm.GetRowsCount();
			int columnsCount2 = _vm.GetColumnsCount();
			for (int num10 = 0; num10 < rowsCount3; num10++)
			{
				TicketInputRowVM row = _vm.GetRow(num10);
				if (Ticket.Kind == TicketKind.DynamicRow)
				{
					if (row.IsDynamicRowTicketDataRow)
					{
						continue;
					}
				}
				else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow && row.IsMixTicketDynamicDataRow)
				{
					continue;
				}
				for (int num11 = 0; num11 < columnsCount2; num11++)
				{
					TicketInputCellVM cellVM9 = _vm.GetCellVM(num10, num11);
					if (cellVM9.TableCell != null)
					{
						return cellVM9;
					}
					if (ticketInputCellVM2 == null && cellVM9.IsField && CanEditCell(cellVM9))
					{
						ticketInputCellVM2 = cellVM9;
						cellRowIndex = num10;
						cellColIndex = num11;
					}
				}
			}
			if (ticketInputCellVM2 != null)
			{
				if (flag)
				{
					_vm.BuildTableCellForTicketTitleFooterCell(ticketInputCellVM2);
				}
				else
				{
					_vm.BuildTableCellForTicketCell(cellRowIndex, cellColIndex);
				}
				if (ticketInputCellVM2.TableCell != null)
				{
					return ticketInputCellVM2;
				}
			}
			int rowsCount4 = _vm.GetRowsCount();
			int columnsCount3 = _vm.GetColumnsCount();
			for (int num12 = 0; num12 < rowsCount4; num12++)
			{
				for (int num13 = 0; num13 < columnsCount3; num13++)
				{
					TicketInputCellVM cellVM10 = _vm.GetCellVM(num12, num13);
					if (CanEditCell(cellVM10))
					{
						_vm.BuildTableCellForTicketCell(num12, num13);
						if (cellVM10.TableCell != null)
						{
							return cellVM10;
						}
					}
				}
			}
			return null;
		}
		Auditai.Model.Row GetFirstAnyTableRow()
		{
			TicketInputTitleFooterVM[] array5 = new TicketInputTitleFooterVM[2] { _vm.Title, _vm.Footer };
			TicketInputTitleFooterVM[] array6 = array5;
			foreach (TicketInputTitleFooterVM ticketInputTitleFooterVM3 in array6)
			{
				int count7 = ticketInputTitleFooterVM3.Rows.Count;
				int count8 = ticketInputTitleFooterVM3.Columns.Count;
				for (int num31 = 0; num31 < count7; num31++)
				{
					for (int num32 = 0; num32 < count8; num32++)
					{
						TicketInputCellVM cellVM16 = ticketInputTitleFooterVM3.GetCellVM(num31, num32);
						if (cellVM16.TableCell != null)
						{
							return cellVM16.TableCell.Row;
						}
					}
				}
			}
			int rowsCount6 = _vm.GetRowsCount();
			int columnsCount5 = _vm.GetColumnsCount();
			for (int num33 = 0; num33 < rowsCount6; num33++)
			{
				for (int num34 = 0; num34 < columnsCount5; num34++)
				{
					TicketInputCellVM cellVM17 = _vm.GetCellVM(num33, num34);
					if (cellVM17.TableCell != null)
					{
						return cellVM17.TableCell.Row;
					}
				}
			}
			return null;
		}
		bool IsAllowExecuteCollectFormulaOnCurrentTicket()
		{
			Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
			bool flag4 = false;
			HashSet<int> hashSet4 = null;
			int rowsCount7 = _vm.GetRowsCount();
			int columnsCount6 = _vm.GetColumnsCount();
			for (int num35 = 0; num35 < rowsCount7; num35++)
			{
				TicketInputRowVM row3 = _vm.GetRow(num35);
				if (Ticket.Kind == TicketKind.DynamicRow)
				{
					if (row3.IsDynamicRowTicketDataRow)
					{
						if (!flag4)
						{
							flag4 = true;
							for (int num36 = 0; num36 < Ticket.Columns.Count; num36++)
							{
								TicketColumn ticketColumn2 = Ticket.Columns[num36];
								if (ticketColumn2.HasField())
								{
									Auditai.Model.Column byId2 = Table.Columns.GetById(ticketColumn2.Field);
									if (byId2 != null)
									{
										DataFormat format6 = byId2.GetFormat();
										if (format6.HasLedgerCollectFormula)
										{
											dictionary4[format6.LedgerCollectFormula] = null;
										}
									}
								}
							}
						}
						continue;
					}
				}
				else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow && row3.IsMixTicketDynamicDataRow)
				{
					if (hashSet4 == null || !hashSet4.Contains(row3.TicketRow.MixRangeDynamicDataRowTemplateId))
					{
						if (hashSet4 == null)
						{
							hashSet4 = new HashSet<int>();
						}
						hashSet4.Add(row3.TicketRow.MixRangeDynamicDataRowTemplateId);
						for (int num37 = 0; num37 < Ticket.Columns.Count; num37++)
						{
							TicketInputCellVM cellVM18 = _vm.GetCellVM(num35, num37);
							if (cellVM18.Column != null)
							{
								Auditai.Model.Column column3 = cellVM18.Column;
								if (column3 != null)
								{
									DataFormat format7 = column3.GetFormat();
									if (format7.HasLedgerCollectFormula)
									{
										dictionary4[format7.LedgerCollectFormula] = null;
									}
								}
							}
						}
					}
					continue;
				}
				for (int num38 = 0; num38 < columnsCount6; num38++)
				{
					TicketInputCellVM cellVM19 = _vm.GetCellVM(num35, num38);
					if (cellVM19.TableCell != null)
					{
						DataFormat displayFormat = cellVM19.TableCell.DisplayFormat;
						if (displayFormat.HasLedgerCollectFormula)
						{
							dictionary4[displayFormat.LedgerCollectFormula] = null;
							continue;
						}
					}
					if (cellVM19.TicketCell != null && cellVM19.Column != null)
					{
						DataFormat format8 = cellVM19.Column.GetFormat();
						if (format8.HasLedgerCollectFormula)
						{
							dictionary4[format8.LedgerCollectFormula] = null;
						}
					}
				}
			}
			TicketInputTitleFooterVM[] array7 = new TicketInputTitleFooterVM[2] { _vm.Title, _vm.Footer };
			foreach (TicketInputTitleFooterVM ticketInputTitleFooterVM4 in array7)
			{
				if (ticketInputTitleFooterVM4 != null)
				{
					int count9 = ticketInputTitleFooterVM4.Rows.Count;
					int count10 = ticketInputTitleFooterVM4.Columns.Count;
					for (int num40 = 0; num40 < count9; num40++)
					{
						for (int num41 = 0; num41 < count10; num41++)
						{
							TicketInputCellVM cellVM20 = ticketInputTitleFooterVM4.GetCellVM(num40, num41);
							if (cellVM20.TableCell != null)
							{
								DataFormat displayFormat2 = cellVM20.TableCell.DisplayFormat;
								if (displayFormat2.HasLedgerCollectFormula)
								{
									dictionary4[displayFormat2.LedgerCollectFormula] = null;
									continue;
								}
							}
							if (cellVM20.TicketCell != null && cellVM20.Column != null)
							{
								DataFormat format9 = cellVM20.Column.GetFormat();
								if (format9.HasLedgerCollectFormula)
								{
									dictionary4[format9.LedgerCollectFormula] = null;
								}
							}
						}
					}
				}
			}
			Dictionary<string, string>.KeyCollection keys = dictionary4.Keys;
			List<string> list15 = null;
			foreach (string item14 in keys)
			{
				FormulaEvaluator formulaEvaluator9 = new FormulaEvaluator(item14);
				if (formulaEvaluator9.HasCancelFunction())
				{
					if (list15 == null)
					{
						list15 = new List<string>();
					}
					list15.Add(item14);
				}
			}
			if (list15 == null || list15.Count == 0)
			{
				return true;
			}
			_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
			Auditai.Model.Row firstAnyTableRow = GetFirstAnyTableRow();
			if (firstAnyTableRow == null)
			{
				return false;
			}
			LedgerVirtualTableEvalContext evalContext4 = GetEvalContext();
			foreach (string item15 in keys)
			{
				try
				{
					FormulaEvaluator formulaEvaluator10 = GenerateFormualEvaluator(item15, firstAnyTableRow.Index);
					formulaEvaluator10.Env.IsAllowExecuteCancelFunction = true;
					Operand operand6 = formulaEvaluator10.EvaluateOnLedgerVirtualTable(evalContext4, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
					object obj2 = operand6.Evaluate();
				}
				catch (FormulaBadReferenceException exception12)
				{
					if (isFailedDueToEmtpyLedger)
					{
						return false;
					}
					exception12.Log("检查当前表单上是否需要运行采集公式时发生了未预期的异常");
					return false;
				}
				catch (FormulaBreakExecuteException)
				{
					return false;
				}
				catch (Exception exception13)
				{
					exception13.Log("检查当前表单上是否需要运行采集公式时发生了未预期的异常");
					return false;
				}
			}
			return true;
		}
		static void SortByDpend(List<CollectFormulaDependSettingData> sortList)
		{
			Dictionary<Auditai.Model.Cell, HashSet<CollectFormulaDependSettingData>> dictionary = new Dictionary<Auditai.Model.Cell, HashSet<CollectFormulaDependSettingData>>();
			Dictionary<Auditai.Model.Column, HashSet<CollectFormulaDependSettingData>> dictionary2 = new Dictionary<Auditai.Model.Column, HashSet<CollectFormulaDependSettingData>>();
			foreach (CollectFormulaDependSettingData sort in sortList)
			{
				if (sort.FormulaComeFromWhichTableCell != null)
				{
					if (!dictionary.TryGetValue(sort.FormulaComeFromWhichTableCell, out var value4))
					{
						value4 = new HashSet<CollectFormulaDependSettingData> { sort };
						dictionary.Add(sort.FormulaComeFromWhichTableCell, value4);
					}
					else
					{
						value4.Add(sort);
					}
				}
				if (sort.FormulaComeFromWhichTableColumn != null)
				{
					if (!dictionary2.TryGetValue(sort.FormulaComeFromWhichTableColumn, out var value5))
					{
						value5 = new HashSet<CollectFormulaDependSettingData> { sort };
						dictionary2.Add(sort.FormulaComeFromWhichTableColumn, value5);
					}
					else
					{
						value5.Add(sort);
					}
				}
			}
			List<CollectFormulaDependSettingData> list11 = new List<CollectFormulaDependSettingData>();
			List<CollectFormulaDependSettingData> list12 = new List<CollectFormulaDependSettingData>();
			HashSet<CollectFormulaDependSettingData> hashSet2 = new HashSet<CollectFormulaDependSettingData>();
			list11.AddRange(sortList);
			for (int num22 = 0; num22 < list11.Count; num22++)
			{
				CollectFormulaDependSettingData collectFormulaDependSettingData3 = list11[num22];
				if (!hashSet2.Contains(collectFormulaDependSettingData3))
				{
					bool flag2 = false;
					foreach (Auditai.Model.Cell item16 in collectFormulaDependSettingData3.DependTableCellSet)
					{
						if (dictionary.TryGetValue(item16, out var value6))
						{
							foreach (CollectFormulaDependSettingData item17 in value6)
							{
								if (item17 != collectFormulaDependSettingData3)
								{
									flag2 = true;
									break;
								}
							}
							if (flag2)
							{
								break;
							}
						}
					}
					if (!flag2)
					{
						foreach (Auditai.Model.Column item18 in collectFormulaDependSettingData3.DependTableColumnSet)
						{
							if (dictionary2.TryGetValue(item18, out var value7))
							{
								foreach (CollectFormulaDependSettingData item19 in value7)
								{
									if (item19 != collectFormulaDependSettingData3)
									{
										flag2 = true;
										break;
									}
								}
								if (flag2)
								{
									break;
								}
							}
						}
					}
					if (!flag2)
					{
						list12.Add(collectFormulaDependSettingData3);
						hashSet2.Add(collectFormulaDependSettingData3);
					}
				}
			}
			if (hashSet2.Count > 0)
			{
				foreach (CollectFormulaDependSettingData item20 in hashSet2)
				{
					list11.Remove(item20);
				}
			}
			while (list11.Count > 0)
			{
				HashSet<CollectFormulaDependSettingData> hashSet3 = new HashSet<CollectFormulaDependSettingData>();
				foreach (CollectFormulaDependSettingData item21 in list11)
				{
					bool flag3 = false;
					foreach (Auditai.Model.Cell item22 in item21.DependTableCellSet)
					{
						if (dictionary.TryGetValue(item22, out var value8))
						{
							foreach (CollectFormulaDependSettingData item23 in value8)
							{
								if (!hashSet2.Contains(item23))
								{
									flag3 = true;
									break;
								}
							}
							if (flag3)
							{
								break;
							}
						}
					}
					if (flag3)
					{
						break;
					}
					foreach (Auditai.Model.Column item24 in item21.DependTableColumnSet)
					{
						if (dictionary2.TryGetValue(item24, out var value9))
						{
							foreach (CollectFormulaDependSettingData item25 in value9)
							{
								if (!hashSet2.Contains(item25))
								{
									flag3 = true;
									break;
								}
							}
							if (flag3)
							{
								break;
							}
						}
					}
					if (!flag3)
					{
						hashSet3.Add(item21);
					}
				}
				if (hashSet3.Count > 0)
				{
					foreach (CollectFormulaDependSettingData item26 in hashSet3)
					{
						list12.Add(item26);
						hashSet2.Add(item26);
						list11.Remove(item26);
					}
				}
				else
				{
					if (list11.Count == 0)
					{
						break;
					}
					CollectFormulaDependSettingData item = list11[0];
					list12.Add(item);
					hashSet2.Add(item);
					list11.Remove(item);
				}
			}
			sortList.Clear();
			sortList.AddRange(list12);
		}
	}

	private List<MixTicketDynamicDataRowRangeSetting> GetMixTicketDynamicDataRowRangesDesc()
	{
		List<MixTicketDynamicDataRowRangeSetting> list = new List<MixTicketDynamicDataRowRangeSetting>();
		for (int num = _vm.GetRowsCount() - 1; num >= 0; num--)
		{
			TicketInputRowVM row = _vm.GetRow(num);
			if (row.IsMixTicketDynamicDataRow)
			{
				MixTicketDynamicDataRowRangeSetting mixTicketDynamicDataRowRangeSetting = new MixTicketDynamicDataRowRangeSetting();
				int mixTicketDynamicDataRowRangeRowsCount = GetMixTicketDynamicDataRowRangeRowsCount(num);
				mixTicketDynamicDataRowRangeSetting.DataRowStartIndex = num - mixTicketDynamicDataRowRangeRowsCount + 1;
				mixTicketDynamicDataRowRangeSetting.DataRowsCount = mixTicketDynamicDataRowRangeRowsCount;
				mixTicketDynamicDataRowRangeSetting.DataRowTemplateId = row.MixTicketDynamicDataRowTemplate.TemplateId;
				mixTicketDynamicDataRowRangeSetting.DesignModeDataRowsCount = row.MixTicketDynamicDataRowTemplate.GetTicketTableRowsCount();
				num = mixTicketDynamicDataRowRangeSetting.DataRowStartIndex;
				list.Add(mixTicketDynamicDataRowRangeSetting);
			}
		}
		return list;
	}

	protected void DebugOutputTicketTableRowsData(string outFilePath = null)
	{
		if (Table == null || Ticket == null)
		{
			return;
		}
		HashSet<Auditai.Model.Row> hashSet = new HashSet<Auditai.Model.Row>();
		foreach (TicketInputCellVM cell in _vm.Title.Cells)
		{
			if (cell.TableCell != null)
			{
				hashSet.Add(cell.TableCell.Row);
			}
		}
		foreach (TicketInputCellVM cell2 in _vm.Footer.Cells)
		{
			if (cell2.TableCell != null)
			{
				hashSet.Add(cell2.TableCell.Row);
			}
		}
		int rowsCount = _vm.GetRowsCount();
		int columnsCount = _vm.GetColumnsCount();
		for (int i = 0; i < rowsCount; i++)
		{
			for (int j = 0; j < columnsCount; j++)
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
				if (cellVM.TableCell != null)
				{
					hashSet.Add(cellVM.TableCell.Row);
				}
			}
		}
		List<Auditai.Model.Row> list = hashSet.ToList();
		list.Sort((Auditai.Model.Row left, Auditai.Model.Row right) => left.Index.CompareTo(right.Index));
		rowsCount = list.Count;
		columnsCount = Table.Columns.Count;
		List<string> list2 = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		for (int k = 0; k < columnsCount; k++)
		{
			stringBuilder.Append(Table.Columns[k].GetUniqueFormulaName());
			stringBuilder.Append(",");
		}
		list2.Add(stringBuilder.ToString());
		for (int l = 0; l < rowsCount; l++)
		{
			int index = list[l].Index;
			StringBuilder stringBuilder2 = new StringBuilder();
			for (int m = 0; m < columnsCount; m++)
			{
				stringBuilder2.Append(Table[index, m].GetDisplayValue());
				stringBuilder2.Append(",");
			}
			list2.Add(stringBuilder2.ToString());
		}
		if (!string.IsNullOrWhiteSpace(outFilePath))
		{
			File.WriteAllLines(outFilePath, list2);
			return;
		}
		foreach (string item in list2)
		{
		}
	}

	public void CacheSelectRange()
	{
		_currentSelectGrid = null;
		_currentselectRange = _grid.Selection;
		if (_currentselectRange.IsValid)
		{
			_currentSelectGrid = _grid;
		}
		else if (_titleEditor.IsInEditing)
		{
			_currentSelectGrid = _titleEditor.View;
			_currentselectRange = _currentSelectGrid.Selection;
		}
		else if (_footerEditor.IsInEditing)
		{
			_currentSelectGrid = _footerEditor.View;
			_currentselectRange = _currentSelectGrid.Selection;
		}
	}

	public void RestoreSelectRange()
	{
		try
		{
			if (_currentSelectGrid != null)
			{
				_currentSelectGrid.SafeSelect(_currentselectRange.TopRow, _currentselectRange.LeftCol, _currentselectRange.BottomRow, _currentselectRange.RightCol);
			}
			_currentSelectGrid = null;
		}
		catch
		{
		}
	}

	private void _gridResizingManager_ResizeRow(object sender, ResizeEventArgs e)
	{
		_editorPanel.SuspendDrawing();
		GridBeginUpdate();
		try
		{
			SaveRecordFilterSetting();
			CacheSelectRange();
			int index = ConvertGridRowIndexToVMRowIndex(e.RowCol);
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			int height = Math.Max(1, e.HeightWidth);
			Point scrollPosition = _grid.ScrollPosition;
			if (selection.BottomRow - selection.TopRow == 0 || !selection.ContainsRow(e.RowCol))
			{
				_vm.SetRowHeight(index, height);
			}
			else
			{
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					int index2 = ConvertGridRowIndexToVMRowIndex(i);
					_vm.SetRowHeight(index2, height);
				}
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			PopulateVm();
			RestoreRecordFilterSetting();
			RestoreSelectRange();
			_grid.ScrollPosition = scrollPosition;
		}
		finally
		{
			GridEndUpdate();
			_editorPanel.ResumeDrawing();
		}
	}

	private void _gridResizingManager_ResizeColumn(object sender, ResizeEventArgs e)
	{
		_editorPanel.SuspendDrawing();
		GridBeginUpdate();
		try
		{
			SaveRecordFilterSetting();
			CacheSelectRange();
			int index = ConvertGridColIndexToVMColIndex(e.RowCol);
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			int width = Math.Max(1, e.HeightWidth);
			Point scrollPosition = _grid.ScrollPosition;
			if (selection.RightCol - selection.LeftCol == 0 || !selection.ContainsCol(e.RowCol))
			{
				Ticket.Columns[index].Width = width;
			}
			else
			{
				for (int i = selection.LeftCol; i <= selection.RightCol; i++)
				{
					if (_grid.Cols[i].IsVisible)
					{
						int index2 = ConvertGridColIndexToVMColIndex(i);
						Ticket.Columns[index2].Width = width;
					}
				}
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			PopulateVm();
			RestoreRecordFilterSetting();
			RestoreSelectRange();
			_grid.ScrollPosition = scrollPosition;
		}
		finally
		{
			GridEndUpdate();
			_editorPanel.ResumeDrawing();
		}
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

	public void IncreaseGridWidth(int value, Action titleFooterWidthAdjustCallback)
	{
		int count = _grid.Cols.Count;
		List<float> gridUnFixedColumnWidthPercent = GetGridUnFixedColumnWidthPercent();
		_editorPanel.SuspendDrawing();
		_grid.BeginUpdate();
		try
		{
			Table.TagTicketDirty(isCanOverrideByServerData: true);
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
				Ticket.Columns[i - _grid.Cols.Fixed].Width = num2;
			}
			AdjustInputGridPositionImpl();
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
		GridBeginUpdate();
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
				Ticket.Columns[i - _grid.Cols.Fixed].Width = num2;
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			PopulateVm();
			_titleEditor.SaveGridCurrentWidthToTicket();
			_footerEditor.SaveGridCurrentWidthToTicket();
		}
		finally
		{
			GridEndUpdate();
		}
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

	public void OnEnterView()
	{
		EnterEdit();
		ResetNavTreePanel();
		if (_grid.Rows.Count > 0 && _grid.Cols.Count > 0)
		{
			TableTicketSelectionRangeAndScrollStatusCacher.TicketGridStatusCacheData ticketGridCacheData = GetTicketGridCacheData();
			if (ticketGridCacheData == null)
			{
				int row = 0;
				if (_grid.Rows.Fixed > 0 && _grid.Rows.Fixed < _grid.Rows.Count)
				{
					row = _grid.Rows.Fixed;
				}
				_grid.Select(row, 0);
			}
		}
		_isSuspendViewChangeModeCheckEvent = true;
		_radioButtonOpenTicketView.Checked = true;
		_isSuspendViewChangeModeCheckEvent = false;
	}

	public void OnLeaveView()
	{
		SaveGridSelectRangeAndScrollPosition();
	}

	private void RestorePreviousSelectedRecordInTableMode()
	{
		if (_navGrids.Count <= 0 || _currentNavGrid.RecordList.Count == 0)
		{
			return;
		}
		bool isSelectByRowId;
		long rowId;
		int num = TicketNavTreeStatusDataCacher.GetLastSelectedNavTreeRecordIndex(Table.Id, out isSelectByRowId, out rowId);
		if (isSelectByRowId)
		{
			TicketRecord ticketRecord = Ticket.Records.Find((TicketRecord r) => r.Rows.Any((Auditai.Model.Row u) => u.Id.Value == rowId));
			if (ticketRecord != null)
			{
				_currentNavGrid.FindAndSelectRecord(ticketRecord);
				RestoreCurrentNavTreeScrollPosition();
				ExpandNavTreeToCurrentRecord();
				return;
			}
		}
		if (num < 0)
		{
			num = _currentNavGrid.RecordList.Count - 1;
		}
		else if (num >= _currentNavGrid.RecordList.Count)
		{
			num = _currentNavGrid.RecordList.Count - 1;
		}
		TicketRecord record = (TicketRecord)_currentNavGrid.RecordList[num];
		_currentNavGrid.FindAndSelectRecord(record);
		RestoreCurrentNavTreeScrollPosition();
		ExpandNavTreeToCurrentRecord();
	}

	public void ShowNavTreePanelForTableEditor(Auditai.Model.Table willToShowTable)
	{
		Program.MainForm.BindControlToNavigationPanel(_navTreeContainer);
		if (Table != willToShowTable)
		{
			Table = willToShowTable;
			PopulateNavs();
			RestorePreviousSelectedRecordInTableMode();
		}
		else
		{
			RestoreCurrentNavTreeScrollPosition();
		}
		_isSuspendViewChangeModeCheckEvent = true;
		_radioButtonOpenTableView.Checked = true;
		_isSuspendViewChangeModeCheckEvent = false;
		Program.MainForm.ShowNavigationPanel();
	}

	public void ResetNavTreePanel()
	{
		Program.MainForm.BindControlToNavigationPanel(_navTreeContainer);
		Program.MainForm.ShowNavigationPanel();
	}

	protected void LeaveEdit()
	{
		CancelSelect();
	}

	private void CancelSelect()
	{
		_grid.Select(-1, -1);
	}

	public void InsertRecordRow()
	{
		if (Table == null || !SoftwareLicenseManager.IsAllowAddTableRows() || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(Table.Rows.Count + 1) || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(_vm.GetRowsCount() + 1))
		{
			return;
		}
		SuspendDrawing();
		try
		{
			int topRow = _grid.Selection.TopRow;
			if (_grid.IsRowIndexOutOfRange(topRow))
			{
				return;
			}
			int num = -1;
			int num2 = -1;
			Point currentSelectionOffsetPosition = GetCurrentSelectionOffsetPosition(0, 0);
			int templateRowId = -1;
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				num2 = ConvertGridRowIndexToVMRowIndex(topRow);
				num = ConvertVMRowIndexToGridRowIndex(num2);
				if (!_vm.IsRecordDataRow_DynamicRowTicket(num2) && !_vm.IsRecordDataRow_DynamicRowTicket(num2 - 1))
				{
					return;
				}
			}
			else
			{
				if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
				{
					return;
				}
				num2 = ConvertGridRowIndexToVMRowIndex(topRow);
				num = ConvertVMRowIndexToGridRowIndex(num2);
				TicketRow ticketRow = _vm.GetRow(num2).TicketRow;
				if (ticketRow.IsMixRangeDynamicDataRow)
				{
					templateRowId = ticketRow.MixRangeDynamicDataRowTemplateId;
				}
				else
				{
					int num3 = num2 - 1;
					if (num3 < 0 || num3 >= _vm.GetRowsCount())
					{
						return;
					}
					ticketRow = _vm.GetRow(num3).TicketRow;
					if (!ticketRow.IsMixRangeDynamicDataRow)
					{
						return;
					}
					templateRowId = ticketRow.MixRangeDynamicDataRowTemplateId;
				}
			}
			_vm.StartRecordNewAddTableRows();
			SaveRecordFilterSetting();
			CacheFilterVisibleRowsBeforeInsert(num, 1);
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				_vm.InsertDataRows(num2, 1);
			}
			else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				_vm.InsertDataRows_MixTicket(num2, 1, templateRowId);
			}
			_vm.CalculateTicket();
			_isDirty = true;
			PopulateVm();
			RestoreRecordFilterSetting();
			MakeCachedFilterVisibleRowsToVisible();
			TrySelectCell(currentSelectionOffsetPosition.X, currentSelectionOffsetPosition.Y);
		}
		finally
		{
			ResumeDrawing();
		}
	}

	public void InsertRecordRows()
	{
		if (Table == null || !SoftwareLicenseManager.IsAllowAddTableRows())
		{
			return;
		}
		decimal? num = InputForm.Numeric("插入行", "插入行数量：", 1, TableEditor.CheckIsInputValidInt_BigThanZero);
		if (!num.HasValue || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(Table.Rows.Count + (int)num.Value) || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(_vm.GetRowsCount() + (int)num.Value))
		{
			return;
		}
		SuspendDrawing();
		try
		{
			int topRow = _grid.Selection.TopRow;
			if (_grid.IsRowIndexOutOfRange(topRow))
			{
				return;
			}
			int num2 = -1;
			int num3 = -1;
			int num4 = (int)num.Value;
			Point currentSelectionOffsetPosition = GetCurrentSelectionOffsetPosition(0, 0);
			int templateRowId = -1;
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				num3 = ConvertGridRowIndexToVMRowIndex(topRow);
				num2 = ConvertVMRowIndexToGridRowIndex(num3);
			}
			else
			{
				if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
				{
					return;
				}
				num3 = ConvertGridRowIndexToVMRowIndex(topRow);
				num2 = ConvertVMRowIndexToGridRowIndex(num3);
				TicketRow ticketRow = _vm.GetRow(num3).TicketRow;
				if (!ticketRow.IsMixRangeDynamicDataRow)
				{
					return;
				}
				templateRowId = ticketRow.MixRangeDynamicDataRowTemplateId;
			}
			_vm.StartRecordNewAddTableRows();
			SaveRecordFilterSetting();
			CacheFilterVisibleRowsBeforeInsert(num2, num4);
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				_vm.InsertDataRows(num3, num4);
			}
			else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				_vm.InsertDataRows_MixTicket(num3, num4, templateRowId);
			}
			_vm.CalculateTicket();
			_isDirty = true;
			PopulateVm();
			RestoreRecordFilterSetting();
			MakeCachedFilterVisibleRowsToVisible();
			TrySelectCell(currentSelectionOffsetPosition.X, currentSelectionOffsetPosition.Y);
		}
		finally
		{
			ResumeDrawing();
		}
	}

	public void AppendRecordRows()
	{
		if (Table == null || !SoftwareLicenseManager.IsAllowAddTableRows())
		{
			return;
		}
		decimal? num = InputForm.Numeric("追加行", "追加行数量：", 1, TableEditor.CheckIsInputValidInt_BigThanZero);
		if (!num.HasValue || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(Table.Rows.Count + (int)num.Value) || SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(_vm.GetRowsCount() + (int)num.Value))
		{
			return;
		}
		SuspendDrawing();
		try
		{
			int count = _grid.Rows.Count;
			int num2 = (int)num.Value;
			int num3 = -1;
			int num4 = -1;
			Point currentSelectionOffsetPosition = GetCurrentSelectionOffsetPosition(0, 0);
			int templateRowId = -1;
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				num3 = Ticket.DataRowStart + _vm.DataRowsCount;
				num4 = ConvertVMRowIndexToGridRowIndex(num3);
			}
			else
			{
				if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow || _grid.IsRowIndexOutOfRange(currentSelectionOffsetPosition.X))
				{
					return;
				}
				int num5 = ConvertVMRowIndexToGridRowIndex(currentSelectionOffsetPosition.X);
				num3 = GetMixTicketDynamicDataRowRangeLastDataRowNextRowIndex(num5);
				if (num3 == -1)
				{
					return;
				}
				num4 = ConvertVMRowIndexToGridRowIndex(num3);
				TicketRow ticketRow = _vm.GetRow(num5).TicketRow;
				if (!ticketRow.IsMixRangeDynamicDataRow)
				{
					return;
				}
				templateRowId = ticketRow.MixRangeDynamicDataRowTemplateId;
			}
			_vm.StartRecordNewAddTableRows();
			SaveRecordFilterSetting();
			CacheFilterVisibleRowsBeforeInsert(num4, num2);
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				_vm.InsertDataRows(num3, num2);
			}
			else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				_vm.InsertDataRows_MixTicket(num3, num2, templateRowId);
			}
			_vm.CalculateTicket();
			_isDirty = true;
			PopulateVm();
			RestoreRecordFilterSetting();
			MakeCachedFilterVisibleRowsToVisible();
			TrySelectCell(currentSelectionOffsetPosition.X, currentSelectionOffsetPosition.Y);
		}
		finally
		{
			ResumeDrawing();
		}
	}

	private void CacheFilterVisibleRowsBeforeInsert(int insertGridRowIndex, int insertRowsCount)
	{
		if (_grid.FilterManager.Filters.Count == 0)
		{
			return;
		}
		_filterVisibleRowIndexList.Clear();
		int count = _grid.Rows.Count;
		int num = insertGridRowIndex + insertRowsCount - 1;
		for (int i = 0; i < count; i++)
		{
			if (_grid.Rows[i].Visible)
			{
				if (i < insertGridRowIndex)
				{
					_filterVisibleRowIndexList.Add(i);
				}
				else if (i >= insertGridRowIndex)
				{
					_filterVisibleRowIndexList.Add(i + insertRowsCount);
				}
			}
		}
		for (int j = insertGridRowIndex; j <= num; j++)
		{
			_filterVisibleRowIndexList.Add(j);
		}
	}

	private HashSet<TicketInputRowVM> GetFilterVisibleRowsBeforeDelete(List<Tuple<int, int>> vmRemoveRangeRowIndexList)
	{
		_filterVisibleRowIndexList.Clear();
		if (_grid.FilterManager.Filters.Count == 0)
		{
			return null;
		}
		HashSet<TicketInputRowVM> hashSet = new HashSet<TicketInputRowVM>();
		int num = 0;
		int num2 = 0;
		foreach (Tuple<int, int> vmRemoveRangeRowIndex in vmRemoveRangeRowIndexList)
		{
			for (int i = num; i < vmRemoveRangeRowIndex.Item1; i++)
			{
				int num3 = ConvertVMRowIndexToGridRowIndex(i);
				if (_grid.Rows[num3].Visible)
				{
					hashSet.Add(_vm.GetRow(ConvertGridRowIndexToVMRowIndex(num3)));
				}
			}
			num = vmRemoveRangeRowIndex.Item1 + vmRemoveRangeRowIndex.Item2;
			num2 += vmRemoveRangeRowIndex.Item2;
		}
		int rowsCount = _vm.GetRowsCount();
		for (int j = num; j < rowsCount; j++)
		{
			int num4 = ConvertVMRowIndexToGridRowIndex(j);
			if (_grid.Rows[num4].Visible)
			{
				hashSet.Add(_vm.GetRow(ConvertGridRowIndexToVMRowIndex(num4)));
			}
		}
		return hashSet;
	}

	private void CacheFilterVisibleRowsBeforeDelete(int removedGridRowIndex, int removeRowsCount)
	{
		if (_grid.FilterManager.Filters.Count == 0)
		{
			return;
		}
		_filterVisibleRowIndexList.Clear();
		int count = _grid.Rows.Count;
		int num = removedGridRowIndex + removeRowsCount - 1;
		for (int i = 0; i < count; i++)
		{
			if (_grid.Rows[i].Visible)
			{
				if (i < removedGridRowIndex)
				{
					_filterVisibleRowIndexList.Add(i);
				}
				else if (i > num)
				{
					_filterVisibleRowIndexList.Add(i - removeRowsCount);
				}
			}
		}
	}

	private void MakeCachedFilterVisibleRowsToVisible()
	{
		if (_grid.FilterManager.Filters.Count == 0)
		{
			_filterVisibleRowIndexList.Clear();
		}
		else
		{
			if (_filterVisibleRowIndexList.Count == 0)
			{
				return;
			}
			int count = _grid.Rows.Count;
			foreach (int filterVisibleRowIndex in _filterVisibleRowIndexList)
			{
				if (filterVisibleRowIndex >= 0 && filterVisibleRowIndex < count)
				{
					_grid.Rows[filterVisibleRowIndex].Visible = true;
				}
			}
			_filterVisibleRowIndexList.Clear();
			AdjustInputGridPositionImpl();
		}
	}

	private void MakeGridRowsToVisible(int gridRowIndex, int count)
	{
		int count2 = _grid.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			int num = gridRowIndex + i;
			if (num >= 0 && num < count2)
			{
				_grid.Rows[i].Visible = true;
			}
		}
	}

	private Point GetCurrentSelectionOffsetPosition(int xOffset, int yOffset)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		int x = selection.TopRow + xOffset;
		int y = selection.LeftCol + yOffset;
		return new Point(x, y);
	}

	private void TrySelectCell(int girdRowIndex, int gridColIndex)
	{
		if (girdRowIndex >= 0 && girdRowIndex < _grid.Rows.Count && gridColIndex >= 0 && gridColIndex < _grid.Cols.Count)
		{
			_grid.Select(girdRowIndex, gridColIndex);
			ShowRecordButtons();
		}
	}

	public void RemoveRecordRow()
	{
		int num = 0;
		int num2 = -1;
		int templateRowId = -1;
		int topRow = _grid.Selection.TopRow;
		int bottomRow = _grid.Selection.BottomRow;
		int num3 = ConvertGridRowIndexToVMRowIndex(topRow);
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			if (num3 < Ticket.DataRowStart || num3 > Ticket.DataRowStart + _vm.DataRowsCount)
			{
				return;
			}
		}
		else
		{
			if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow || num3 < 0 || num3 >= _vm.GetRowsCount())
			{
				return;
			}
			TicketRow ticketRow = _vm.GetRow(num3).TicketRow;
			if (ticketRow == null || !ticketRow.IsMixRangeDynamicDataRow)
			{
				return;
			}
			templateRowId = ticketRow.MixRangeDynamicDataRowTemplateId;
			num2 = num3;
			num = GetMixTicketDynamicDataRowRangeRowsCount(num3);
		}
		TicketInputRowVM row = _vm.GetRow(num3);
		Auditai.Model.Row tempRow = row.TempRow;
		if (tempRow != null && (!Program.MainForm.TableEditor.CanEditRow(tempRow) || tempRow.IsLocked || Table.ControlLockRows.Contains(tempRow)))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您无权删除该行。");
			return;
		}
		int num4 = -1;
		SuspendDrawing();
		try
		{
			Point currentSelectionOffsetPosition = GetCurrentSelectionOffsetPosition(0, 0);
			SaveRecordFilterSetting();
			CacheFilterVisibleRowsBeforeDelete(topRow, 1);
			_vm.RemoveDataRow(num3);
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				if (_vm.DataRowsCount == 0)
				{
					_vm.AppendDataRow();
					num4 = ConvertVMRowIndexToGridRowIndex(Ticket.DataRowStart);
				}
			}
			else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow && num - 1 == 0 && num2 >= 0)
			{
				_vm.InsertDataRow_MixTicket(num2, templateRowId);
				num4 = ConvertVMRowIndexToGridRowIndex(num2);
			}
			_vm.CalculateTicket();
			_isDirty = true;
			PopulateVm();
			RestoreRecordFilterSetting();
			MakeCachedFilterVisibleRowsToVisible();
			if (num4 >= 0)
			{
				MakeGridRowsToVisible(num4, 1);
			}
			TrySelectCell(currentSelectionOffsetPosition.X, currentSelectionOffsetPosition.Y);
		}
		finally
		{
			ResumeDrawing();
		}
	}

	public void RemoveRecordRows()
	{
		if (Table == null || Table.IsLocked || Ticket == null || (Ticket.Kind != TicketKind.DynamicRow && Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow))
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		int num = selection.BottomRow - selection.TopRow + 1;
		if (num <= 0)
		{
			return;
		}
		List<int> list = new List<int>(num);
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			if (i >= 0 && i < _grid.Rows.Count && _grid.Rows[i].Visible)
			{
				list.Add(i);
				int index = ConvertGridRowIndexToVMRowIndex(i);
				Auditai.Model.Row tempRow = _vm.GetRow(index).TempRow;
				if (tempRow != null && (!Program.MainForm.TableEditor.CanEditRow(tempRow) || tempRow.IsLocked || Table.ControlLockRows.Contains(tempRow)))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "要删除的行包含您无权删除的行。");
					return;
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		List<Tuple<int, int>> list2 = new List<Tuple<int, int>>();
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			int gridRowIndex = list[0];
			int num2 = 0;
			int num3 = -1;
			for (int j = 0; j < list.Count; j++)
			{
				int num4 = list[j];
				int num5 = ConvertGridRowIndexToVMRowIndex(num4);
				if (num5 >= Ticket.DataRowStart + _vm.DataRowsCount)
				{
					if (num2 > 0)
					{
						list2.Add(Tuple.Create(ConvertGridRowIndexToVMRowIndex(gridRowIndex), num2));
						num2 = 0;
					}
					break;
				}
				if (num5 >= Ticket.DataRowStart)
				{
					if (num2 == 0)
					{
						gridRowIndex = num4;
						num3 = num4;
						num2 = 1;
					}
					else if (num4 != num3 + 1)
					{
						list2.Add(Tuple.Create(ConvertGridRowIndexToVMRowIndex(gridRowIndex), num2));
						gridRowIndex = num4;
						num3 = num4;
						num2 = 1;
					}
					else
					{
						num3 = num4;
						num2++;
					}
				}
			}
			if (num2 > 0)
			{
				list2.Add(Tuple.Create(ConvertGridRowIndexToVMRowIndex(gridRowIndex), num2));
			}
		}
		else
		{
			if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
			{
				return;
			}
			int gridRowIndex2 = list[0];
			int num6 = 0;
			int num7 = -1;
			int num8 = -1;
			for (int k = 0; k < list.Count; k++)
			{
				int num9 = list[k];
				int index2 = ConvertGridRowIndexToVMRowIndex(num9);
				TicketInputRowVM row = _vm.GetRow(index2);
				if (!row.IsMixTicketDynamicDataRow)
				{
					if (num6 > 0)
					{
						list2.Add(Tuple.Create(ConvertGridRowIndexToVMRowIndex(gridRowIndex2), num6));
						num6 = 0;
					}
				}
				else if (num6 == 0)
				{
					gridRowIndex2 = num9;
					num7 = num9;
					num6 = 1;
					num8 = row.TicketRow.MixRangeDynamicDataRowTemplateId;
				}
				else if (row.TicketRow.MixRangeDynamicDataRowTemplateId != num8)
				{
					list2.Add(Tuple.Create(ConvertGridRowIndexToVMRowIndex(gridRowIndex2), num6));
					gridRowIndex2 = num9;
					num7 = num9;
					num6 = 1;
					num8 = row.TicketRow.MixRangeDynamicDataRowTemplateId;
				}
				else if (num9 != num7 + 1)
				{
					list2.Add(Tuple.Create(ConvertGridRowIndexToVMRowIndex(gridRowIndex2), num6));
					gridRowIndex2 = num9;
					num7 = num9;
					num6 = 1;
				}
				else
				{
					num7 = num9;
					num6++;
				}
			}
			if (num6 > 0)
			{
				list2.Add(Tuple.Create(ConvertGridRowIndexToVMRowIndex(gridRowIndex2), num6));
			}
		}
		if (list2.Count == 0)
		{
			return;
		}
		HashSet<TicketInputRowVM> filterVisibleRowsBeforeDelete = GetFilterVisibleRowsBeforeDelete(list2);
		list2.Reverse();
		SuspendDrawing();
		try
		{
			Point currentSelectionOffsetPosition = GetCurrentSelectionOffsetPosition(0, 0);
			SaveRecordFilterSetting();
			foreach (Tuple<int, int> item in list2)
			{
				DeleteRecordDataRows(item.Item1, item.Item2);
			}
			_vm.CalculateTicket();
			PopulateVm();
			RestoreRecordFilterSetting();
			if (filterVisibleRowsBeforeDelete != null)
			{
				_filterVisibleRowIndexList.Clear();
				int rowsCount = _vm.GetRowsCount();
				for (int l = 0; l < rowsCount; l++)
				{
					TicketInputRowVM row2 = _vm.GetRow(l);
					if (filterVisibleRowsBeforeDelete.Contains(row2))
					{
						_filterVisibleRowIndexList.Add(ConvertVMRowIndexToGridRowIndex(l));
					}
				}
				MakeCachedFilterVisibleRowsToVisible();
			}
			TrySelectCell(currentSelectionOffsetPosition.X, currentSelectionOffsetPosition.Y);
		}
		finally
		{
			ResumeDrawing();
		}
	}

	private void DeleteRecordDataRows(int vmStartRowIndex, int count)
	{
		if (vmStartRowIndex < 0 || count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = -1;
		int num3 = 0;
		int num4 = -1;
		int num5 = -1;
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			int num6 = Math.Max(Ticket.DataRowStart, vmStartRowIndex);
			int num7 = Math.Min(Ticket.DataRowStart + _vm.DataRowsCount - 1, vmStartRowIndex + count - 1);
			num = num7 - num6 + 1;
			num2 = num6;
		}
		else
		{
			if (Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow || vmStartRowIndex < 0 || vmStartRowIndex >= _vm.GetRowsCount())
			{
				return;
			}
			TicketRow ticketRow = _vm.GetRow(vmStartRowIndex).TicketRow;
			if (ticketRow == null || !ticketRow.IsMixRangeDynamicDataRow)
			{
				return;
			}
			num5 = ticketRow.MixRangeDynamicDataRowTemplateId;
			num4 = vmStartRowIndex;
			num3 = GetMixTicketDynamicDataRowRangeRowsCount(vmStartRowIndex);
			num2 = vmStartRowIndex;
			num = 0;
			for (int i = 0; i < count; i++)
			{
				int num8 = ConvertGridRowIndexToVMRowIndex(vmStartRowIndex + i);
				if (num8 < 0 || num8 >= _vm.GetRowsCount())
				{
					break;
				}
				TicketRow ticketRow2 = _vm.GetRow(num8).TicketRow;
				if (ticketRow2 == null || !ticketRow2.IsMixRangeDynamicDataRow || ticketRow2.MixRangeDynamicDataRowTemplateId != num5)
				{
					break;
				}
				num++;
			}
		}
		if (num <= 0)
		{
			return;
		}
		_vm.RemoveDataRows(num2, num);
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			if (_vm.DataRowsCount == 0)
			{
				_vm.AppendDataRow();
			}
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow && num3 - num == 0 && num4 >= 0)
		{
			_vm.InsertDataRow_MixTicket(num4, num5);
		}
		_isDirty = true;
	}

	public void Populate(bool isReCalculateTable = true)
	{
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		PopuldateNavGridValidationFailedCount();
		_isMouseOverRowNumberColumn = false;
		_isMouseOverCancelManualInputIcon = false;
		if (isReCalculateTable)
		{
			Table.LoadFormulaDependencies();
			ReCalculateTable();
		}
		_isInShowingVirtualNode = Table.Rows.Count == 0;
		Populate(isOpenSelectedRecordOnLeave: true, isExandToCurrentSelecteRecord: true);
		if (Table.NeedSave)
		{
			SaveTable(isReCalculateTable: false);
			SetCommandState();
		}
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
	}

	public void SuspendEditorPanelDrawing()
	{
		_editorPanel.SuspendDrawing();
	}

	public void ResumeEditorPanelDrawing()
	{
		_editorPanel.ResumeDrawing();
	}

	public void SuspendDrawing()
	{
		_navTreePanel.SuspendDrawing();
		foreach (C1SplitterPanel panel in _splc.Panels)
		{
			panel.SuspendDrawing();
		}
	}

	public void ResumeDrawing()
	{
		_navTreePanel.ResumeDrawing();
		foreach (C1SplitterPanel panel in _splc.Panels)
		{
			panel.ResumeDrawing();
		}
	}

	public void SuspendPanelResizeEvent()
	{
		_suspendPanelResizeEvent = true;
	}

	public void ResumePanelResizeEvent()
	{
		_suspendPanelResizeEvent = false;
	}

	protected void SuspendSaveNavTreeScrollPosition()
	{
		try
		{
			TicketNavTreeStatusDataCacher.SuspendProcessNavTreeScrollEvent(Table.Id);
		}
		catch
		{
		}
	}

	protected void ResumeSaveNavTreeScrollPosition()
	{
		try
		{
			TicketNavTreeStatusDataCacher.ResumeProcessNavTreeScrollEvent(Table.Id);
		}
		catch
		{
		}
	}

	private void ScrollCurrentNavTreeSelectionToVisible()
	{
		try
		{
			if (_navGrids.Count != 0)
			{
				C1.Win.C1FlexGrid.CellRange selection = _currentNavGrid.View.Selection;
				_currentNavGrid.View.ShowCell(selection.TopRow, selection.LeftCol);
			}
		}
		catch
		{
		}
	}

	protected void RestoreCurrentNavTreeScrollPosition(bool isResumeSaveScollPosition = true)
	{
		try
		{
			if (_otbNavs.SelectedIndex >= 0 && _otbNavs.SelectedIndex < _navTreeScollPositionList.Count)
			{
				NavTreeScrollPosition navTreeScrollPosition = _navTreeScollPositionList[_otbNavs.SelectedIndex];
				if (navTreeScrollPosition != null)
				{
					_navGrids[_otbNavs.SelectedIndex].View.ScrollPosition = navTreeScrollPosition.Position;
				}
				if (isResumeSaveScollPosition)
				{
					ResumeSaveNavTreeScrollPosition();
				}
			}
		}
		catch
		{
		}
	}

	protected void Populate(bool isOpenSelectedRecordOnLeave, bool isExandToCurrentSelecteRecord, bool isRestoreFilterSetting = true)
	{
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SuspendDrawing();
		SuspendSaveNavTreeScrollPosition();
		try
		{
			GridBeginUpdate();
			_grid.AllowEditing = !Table.IsLocked;
			if (TreeNodeStateCache.Contains(Table.Id))
			{
				TreeNodeStateCache.Get(Table.Id).Kind = TreeNodeCacheKind.TicketInput;
			}
			else
			{
				TreeNodeStateCache.Set(Table.Id, new TreeNodeCacheState
				{
					Kind = TreeNodeCacheKind.TicketInput
				});
			}
			IsAllowModifyTableRowOrder = true;
			HasFillingFormula = Table.Columns.Where((Auditai.Model.Column c) => c.HasFormula).Any(delegate(Auditai.Model.Column c)
			{
				FormulaEvaluator formulaEval = new FormulaEvaluator(c.Formula);
				bool isAllowModifyTableRowOrder;
				bool result = TicketInputTableVM.IsExistFillFormula(formulaEval, out isAllowModifyTableRowOrder);
				if (!isAllowModifyTableRowOrder)
				{
					IsAllowModifyTableRowOrder = false;
				}
				return result;
			});
			PopulateNavs();
			RestorePreviousSelectedRecord(isOpenSelectedRecordOnLeave);
			PopulateRecordImpl();
			ExpandNavTreeToCurrentRecord(isExandToCurrentSelecteRecord);
			PopulateToolBar();
			if (isRestoreFilterSetting)
			{
				RestoreRecordFilterSetting();
			}
			_gridDecorator.SetCellFlickerDirty();
			_gridDecorator.ReBuildNavTreeFlicker();
		}
		finally
		{
			RestoreCurrentNavTreeScrollPosition();
			GridEndUpdate();
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
		}
	}

	private string GetTableName()
	{
		if (Table == null)
		{
			return string.Empty;
		}
		if (Table.TreeNode == null)
		{
			return string.Empty;
		}
		return Table.TreeNode.Name;
	}

	private void RestorePreviousSelectedRecord(bool isRestore)
	{
		if (isRestore)
		{
			_currentRecord = TicketNavTreeStatusDataCacher.GetLastSelectedNavTreeRecordIndex(Table.Id, out var isSelectByRowId, out var rowId);
			if (isSelectByRowId)
			{
				_wantPopulatedRecordContainsRowId = rowId;
			}
		}
	}

	private void ExpandNavTreeToCurrentRecord(bool isExapnd = true)
	{
		if (isExapnd && _otbNavs.SelectedIndex >= 0 && _otbNavs.SelectedIndex < _navGrids.Count)
		{
			TicketNavTreeStatusDataCacher.ExpandTicketRecordParentNode(_currentNavGrid, Record);
			C1.Win.C1FlexGrid.CellRange selection = _currentNavGrid.View.Selection;
			if (selection.TopRow >= 0 && selection.LeftCol >= 0)
			{
				_currentNavGrid.View.ShowCell(selection.TopRow, selection.LeftCol);
			}
		}
	}

	public void PopulateRecord()
	{
		GridBeginUpdate();
		try
		{
			PopulateRecordImpl();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	protected void PopulateRecordImpl()
	{
		_ticketGridColumnMerges = null;
		_isTicketLocked = false;
		bool flag = false;
		if (Ticket == null || Ticket.Records == null)
		{
			_vm = new TicketInputTableVM(null, null, HasFillingFormula, isCalculateTicket: false);
			_vm.IsInShowingVirtualNode = true;
			_isAdd = true;
			_isDirty = false;
			Record = new TicketRecord();
			_vm.BuildTableCellForAllTicketCell();
			_vm.InitCombolistForNewRecord();
			flag = true;
		}
		else if (Ticket.Records.Count > 0)
		{
			SetCurrentRecord();
			_vm = new TicketInputTableVM(Ticket, Record, HasFillingFormula, isCalculateTicket: false);
			_isAdd = false;
			_isDirty = false;
			if (_isBuildTableCellInModifyModeOnOpenCurrentRecord)
			{
				_isBuildTableCellInModifyModeOnOpenCurrentRecord = false;
				_vm.BuildTableCellForAllTicketCell();
			}
		}
		else
		{
			_vm = new TicketInputTableVM(Ticket, null, HasFillingFormula, isCalculateTicket: false);
			_vm.IsInShowingVirtualNode = true;
			_isAdd = true;
			_isDirty = false;
			Record = new TicketRecord();
			_vm.BuildTableCellForAllTicketCell();
			_vm.InitCombolistForNewRecord();
			flag = true;
		}
		Ticket.Level = TicketLevel.Report;
		_titleEditor.VMData = _vm.Title;
		_footerEditor.VMData = _vm.Footer;
		_vm.CalculateTicket();
		PopulateVmImpl();
		SendTicketNavTreeNodeChangeEventToOtherClient();
		if (!flag)
		{
			return;
		}
		try
		{
			_isInTrySelectTicketNavTreeFirstNodeMode = true;
			if (_currentNavGrid != null)
			{
				_currentNavGrid.TryToSelectFirstAvailableNode();
			}
		}
		catch
		{
		}
		finally
		{
			_isInTrySelectTicketNavTreeFirstNodeMode = false;
		}
	}

	protected void GridBeginUpdate()
	{
		_grid.BeginUpdate();
		_titleEditor.GridBeginUpdate();
		_footerEditor.GridBeginUpdate();
	}

	protected void GridEndUpdate()
	{
		_grid.EndUpdate();
		_titleEditor.GridEndUpdate();
		_footerEditor.GridEndUpdate();
	}

	public void PopulateVm()
	{
		GridBeginUpdate();
		try
		{
			PopulateVmImpl();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	private void ShowFilterContextMenu(bool isShow)
	{
		if (!isShow)
		{
			foreach (C1CommandLink filterCommandLink in _filterCommandLinkList)
			{
				_ctxCell.CommandLinks.Remove(filterCommandLink);
			}
			_filterCommandLinkList.Clear();
		}
		else if (_filterCommandLinkList.Count == 0)
		{
			int num = _ctxCell.CommandLinks.IndexOf(_lnkExportAttachment);
			num = ((num != -1) ? (num + 1) : _ctxCell.CommandLinks.Count);
			C1CommandLink c1CommandLink = _grid.FilterManager.GenLnkFilter();
			c1CommandLink.Delimiter = true;
			_filterCommandLinkList.Add(c1CommandLink);
			_ctxCell.CommandLinks.Insert(num++, c1CommandLink);
			c1CommandLink = _grid.FilterManager.GenLnkSelect();
			_filterCommandLinkList.Add(c1CommandLink);
			_ctxCell.CommandLinks.Insert(num++, c1CommandLink);
			c1CommandLink = _grid.FilterManager.GenLnkCancelCurrentColumn();
			_filterCommandLinkList.Add(c1CommandLink);
			_ctxCell.CommandLinks.Insert(num++, c1CommandLink);
		}
	}

	protected void ResetGridStruct()
	{
		_grid.FilterManager.IsDrawCancelAllFilterIcon = true;
		_grid.FilterManager.IsDrawCancelAllFilterOnLeft = true;
		if (_grid.FilterManager.IsFiltering)
		{
			try
			{
				_isSuspendFilterManager_AfterFilterExecuteEvent = true;
				_grid.FilterManager.Clear();
			}
			catch (Exception exception)
			{
				exception.Log();
			}
			finally
			{
				_isSuspendFilterManager_AfterFilterExecuteEvent = false;
			}
		}
		_isSuspendBodySelectionChangeEvent = true;
		_grid.Rows.Fixed = 0;
		_grid.Rows.Frozen = 0;
		_grid.Rows.Count = 0;
		_grid.Cols.Fixed = 0;
		_grid.Cols.Count = 0;
		_grid.Rows.Count = _vm.GetRowsCount();
		_grid.Cols.Count = Ticket.Columns.Count + 1;
		_grid.Cols.Fixed = 1;
		_isSuspendBodySelectionChangeEvent = false;
		_grid.Cols[0].Width = 30;
		int count = _grid.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			int index = ConvertGridRowIndexToVMRowIndex(i);
			_grid.Rows[i].Height = _vm.GetRowHeight(index);
		}
		for (int j = 0; j < Ticket.Columns.Count; j++)
		{
			int index2 = ConvertVMColIndexToGridColIndex(j);
			_grid.Cols[index2].Width = Ticket.Columns[j].Width;
			_grid.Cols[index2].Visible = !Ticket.Columns[j].IsHiddenColumn;
		}
		_grid.MergedRanges.Clear();
		foreach (TicketMerge merge in _vm.Merges)
		{
			int topRow = ConvertVMRowIndexToGridRowIndex(merge.TopRow);
			int leftCol = ConvertVMColIndexToGridColIndex(merge.LeftColumn);
			int bottomRow = ConvertVMRowIndexToGridRowIndex(merge.BottomRow);
			int rightCol = ConvertVMColIndexToGridColIndex(merge.RightColumn);
			_grid.MergedRanges.Add(topRow, leftCol, bottomRow, rightCol);
			for (int k = merge.TopRow; k <= merge.BottomRow; k++)
			{
				for (int l = merge.LeftColumn; l <= merge.RightColumn; l++)
				{
					int num = ConvertVMRowIndexToGridRowIndex(k);
					int num2 = ConvertVMColIndexToGridColIndex(l);
					if (!_grid.IsIndexOutOfRange(num, num2))
					{
						_grid.GetCellRange(num, num2).StyleNew.DataType = null;
					}
				}
			}
		}
		_grid.Rows.Fixed = Ticket.ColumnHeaderRowsCount;
		ShowFilterContextMenu(Ticket.ColumnHeaderRowsCount > 0);
		_grid.Cols.Frozen = Math.Max(0, Ticket.TableColsFrozenCount);
		_grid.FilterManager.ResetGridColumnMergeRange();
		_grid.Invalidate();
	}

	protected void PopulateVmImpl()
	{
		ResetGridStruct();
		_titleEditor.Populate();
		_footerEditor.Populate();
		AutoAdjustInputGridPositionImpl();
		SetCommandState();
		ShowRecordButtons();
		HideTooltip();
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

	public void PreviousRecord()
	{
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SaveGridSelectRangeAndScrollPosition();
		SuspendDrawing();
		try
		{
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			if (_currentRecord > 0)
			{
				_currentRecord--;
				PopulateRecord();
			}
		}
		finally
		{
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
		}
	}

	public void NextRecord()
	{
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SaveGridSelectRangeAndScrollPosition();
		SuspendDrawing();
		try
		{
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			if (_currentRecord < Ticket.Records.Count - 1)
			{
				_currentRecord++;
				PopulateRecord();
			}
		}
		finally
		{
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
		}
	}

	public void DeleteRecord()
	{
		if (_isTicketLocked || Record == null || Record.Rows == null)
		{
			return;
		}
		if (Record.Rows.Any((Auditai.Model.Row r) => !Program.MainForm.TableEditor.CanEditRow(r) || r.IsLocked || Table.ControlLockRows.Contains(r)))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "您无权删除当前" + Ticket.GetLevelString() + "。");
		}
		else
		{
			if (Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "确定要删除当前" + Ticket.GetLevelString() + "吗？", MessageBoxButtons.OKCancel) != DialogResult.OK)
			{
				return;
			}
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
			SaveGridSelectRangeAndScrollPosition();
			ClearRecordFilterSetting();
			SuspendDrawing();
			try
			{
				Table.BeginBatchUpdateValue();
				foreach (Auditai.Model.Row row in Record.Rows)
				{
					row.Remove();
				}
				Table.EndBatchUpdateValue();
				SaveTable(isReCalculateTable: true);
				Populate(isOpenSelectedRecordOnLeave: false, isExandToCurrentSelecteRecord: true);
			}
			finally
			{
				_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
				RestoreGridSelectRangeAndScrollPosition();
				ResumeDrawing();
			}
		}
	}

	protected void OpenVirtualNavNode(Dictionary<Id64, string> comboListCellInitValue = null)
	{
		if (Table == null)
		{
			return;
		}
		FinishEditorInputStatus();
		if (SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(Table.Rows.Count))
		{
			return;
		}
		_isTicketLocked = false;
		SuspendDrawing();
		GridBeginUpdate();
		try
		{
			if (_isInTrySelectTicketNavTreeFirstNodeMode)
			{
				_isInShowingVirtualNode = true;
				_isCurrentTicketComeFromVirtualNode = true;
				_vm.IsInShowingVirtualNode = true;
				_vm.BuildTableCellForAllTicketCell();
				_vm.InitCombolistForNewRecord(comboListCellInitValue);
				_vm.CalculateTicket(forceCalculate: true);
			}
			else
			{
				SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
				_isInShowingVirtualNode = true;
				_isCurrentTicketComeFromVirtualNode = true;
				if (_isTableSaveActionOccured)
				{
					PopulateNavs();
				}
				_isAdd = true;
				_isDirty = false;
				_vm = new TicketInputTableVM(Ticket, null, HasFillingFormula);
				_titleEditor.VMData = _vm.Title;
				_footerEditor.VMData = _vm.Footer;
				_vm.IsInShowingVirtualNode = true;
				_vm.BuildTableCellForAllTicketCell();
				_vm.InitCombolistForNewRecord(comboListCellInitValue);
				_vm.CalculateTicket();
				PopulateVm();
			}
			SetCommandState();
			_grid.Invalidate();
			_titleEditor.View.Invalidate();
			_footerEditor.View.Invalidate();
		}
		catch (TableModelException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			CancelSave();
		}
		finally
		{
			GridEndUpdate();
			ResumeDrawing();
		}
	}

	public void AddRecord()
	{
		if (Table == null || !SoftwareLicenseManager.IsAllowAddTableRows())
		{
			return;
		}
		FinishEditorInputStatus();
		if (SoftwareLicenseManager.IsTableRowsOutOfLicenseLimit(Table.Rows.Count))
		{
			return;
		}
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SaveGridSelectRangeAndScrollPosition();
		SuspendDrawing();
		GridBeginUpdate();
		try
		{
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			_isInShowingVirtualNode = false;
			_isCurrentTicketComeFromVirtualNode = false;
			if (_isTableSaveActionOccured)
			{
				PopulateNavs();
				RestorePreviousSelectedRecord(isRestore: true);
				if (Ticket.Records.Count > 0)
				{
					SetCurrentRecord();
					ExpandNavTreeToCurrentRecord();
				}
			}
			_isAdd = true;
			_isDirty = false;
			_vm = new TicketInputTableVM(Ticket, null, HasFillingFormula);
			_titleEditor.VMData = _vm.Title;
			_footerEditor.VMData = _vm.Footer;
			_vm.BuildTableCellForAllTicketCell();
			_vm.InitCombolistForNewRecord();
			_vm.CalculateTicket();
			PopulateVm();
			SetCommandState();
			_grid.Invalidate();
			_titleEditor.View.Invalidate();
			_footerEditor.View.Invalidate();
		}
		catch (TableModelException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			CancelSave();
		}
		finally
		{
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			GridEndUpdate();
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
		}
	}

	public void SetInputDataDirty()
	{
		_isDirty = true;
		SetCommandState();
	}

	public void PrintTableRowsData(string msg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (msg != null)
		{
			stringBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff "));
			stringBuilder.Append(msg);
			stringBuilder.Append("\r\n");
		}
		foreach (TicketFixedMultiRowVM fixedMultiRowVM in _vm.FixedMultiRowVMs)
		{
			if (fixedMultiRowVM.Row != null)
			{
				stringBuilder.Append(fixedMultiRowVM.Row.Index + 1).Append(":");
				for (int i = 0; i < Table.Columns.Count; i++)
				{
					Auditai.Model.Cell cell = Table[fixedMultiRowVM.Row.Index, i];
					stringBuilder.Append(cell.Value.ToString());
					stringBuilder.Append(",");
				}
				stringBuilder.Append("\r\n");
			}
		}
		stringBuilder.Append("\r\n");
	}

	private void ReCalculateTable()
	{
		try
		{
			Table.CalculateRecursive();
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		try
		{
			Table.EvalControlFormula();
		}
		catch (Exception exception2)
		{
			exception2.Log();
		}
	}

	private void SaveTable(bool isReCalculateTable)
	{
		if (isReCalculateTable)
		{
			ReCalculateTable();
		}
		Table.Save();
		Table.TreeNode.IsEntityDirty = true;
		Table.Project.Dal.SaveTreeNodes(new Auditai.DTO.TreeNode[1] { Table.TreeNode.ToDto() });
	}

	public void FinishEditorInputStatus(bool isCancel = false)
	{
		TitleEditor.FinishEditorInputStatus(isCancel);
		FooterEditor.FinishEditorInputStatus(isCancel);
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

	public void SaveRecord(bool isSaveReccordFilterSetting = true, bool isRePopulate = true)
	{
		_isTableSaveActionOccured = false;
		_isTableDataReloadFromDB = false;
		if (_isInShowingVirtualNode || Table == null)
		{
			return;
		}
		FinishEditorInputStatus();
		if (isSaveReccordFilterSetting)
		{
			SaveRecordFilterSetting();
		}
		bool isReCalculateTable = true;
		if (_isForceReCalculateTableBeforeSaveRecord)
		{
			ReCalculateTable();
			isReCalculateTable = false;
		}
		if (!IsHasSaveDataPermission)
		{
			return;
		}
		Auditai.Model.Row row = null;
		switch (Ticket.Kind)
		{
		case TicketKind.FixedOneRow:
			row = RemoveTableEmptyRow_FixedOneRow();
			break;
		case TicketKind.FixedMultiRow:
			row = RemoveTableEmptyRow_FixedMultiRow();
			break;
		case TicketKind.DynamicRow:
			row = RemoveTableEmptyRow_DynamicRow();
			break;
		case TicketKind.FixedDataRowMixDynamicDataRow:
			row = RemoveTableEmptyRow_MixTicket();
			break;
		}
		if (IsPreventSaveDueToNewTableRow())
		{
			_isTableDataReloadFromDB = true;
			CancelSave();
			return;
		}
		if (SoftwareLicenseManager.IsTableRowsCountOutOfLicenseLimit(Table, 0))
		{
			_isTableDataReloadFromDB = true;
			CancelSave();
			return;
		}
		if (row != null && _isSuggestKeepNavTreeNodeInTablePosition)
		{
			_isSuggestKeepNavTreeNodeInTablePosition = false;
			int num = -1;
			if (_recordFirstTableRow != null && (_recordFirstTableRow.Status == SyncStatus.LocalDeleted || _recordFirstTableRow.Status == SyncStatus.ServerDeleted))
			{
				num = _recordFirstTableRowIndex;
				if (num < 0 || num >= Table.Rows.Count)
				{
					num = -1;
				}
			}
			if (num != -1)
			{
				Table.Rows.Move(row.Index, 1, num);
			}
		}
		if (Table.NeedSave)
		{
			SaveTable(isReCalculateTable);
			_isTableSaveActionOccured = true;
		}
		if (!isRePopulate)
		{
			if (_isAdd && Ticket.Navs.Count > 0 && row != null)
			{
				TicketNavTreeStatusDataCacher.SaveNavTreeSelectedRecordIndex(Table.Id, _currentRecord, isSelectByRowId: true, row.Id.Value);
			}
			return;
		}
		SuspendDrawing();
		SuspendSaveNavTreeScrollPosition();
		try
		{
			bool isOpenSelectedRecordOnLeave = false;
			bool isExandToCurrentSelecteRecord = false;
			if (_isAdd && Ticket.Navs.Count > 0 && row != null)
			{
				_wantPopulatedRecordContainsRowId = row.Id.Value;
				TicketNavTreeStatusDataCacher.SaveNavTreeSelectedRecordIndex(Table.Id, _currentRecord, isSelectByRowId: true, row.Id.Value);
				isExandToCurrentSelecteRecord = true;
			}
			else if (Ticket.Navs.Count > 0 && row != null)
			{
				_wantPopulatedRecordContainsRowId = row.Id.Value;
				TicketNavTreeStatusDataCacher.SaveNavTreeSelectedRecordIndex(Table.Id, _currentRecord, isSelectByRowId: true, row.Id.Value);
				isExandToCurrentSelecteRecord = true;
				isOpenSelectedRecordOnLeave = true;
			}
			Populate(isOpenSelectedRecordOnLeave, isExandToCurrentSelecteRecord);
		}
		finally
		{
			ResumeDrawing();
			RestoreCurrentNavTreeScrollPosition();
		}
		ScrollCurrentNavTreeSelectionToVisible();
		void DeleteTableRows(HashSet<Auditai.Model.Row> rowSet)
		{
			if (rowSet.Count != 0)
			{
				if (rowSet.Count == 1)
				{
					foreach (Auditai.Model.Row item in rowSet)
					{
						item.Remove();
					}
					return;
				}
				List<Auditai.Model.Row> list = rowSet.ToList();
				list.Sort((Auditai.Model.Row left, Auditai.Model.Row right) => left.Index.CompareTo(right.Index));
				List<List<Auditai.Model.Row>> list2 = new List<List<Auditai.Model.Row>>
				{
					new List<Auditai.Model.Row> { list[0] }
				};
				for (int j = 1; j < list.Count; j++)
				{
					if (list[j].Index != list[j - 1].Index + 1)
					{
						list2.Add(new List<Auditai.Model.Row> { list[j] });
					}
					else
					{
						list2[list2.Count - 1].Add(list[j]);
					}
				}
				Table.BeginBatchUpdateValue();
				for (int num2 = list2.Count - 1; num2 >= 0; num2--)
				{
					int index = list2[num2][0].Index;
					int count = list2[num2].Count;
					Table.Rows.Remove(index, count);
				}
				Table.EndBatchUpdateValue();
			}
		}
		Auditai.Model.Row RemoveTableEmptyRow_DynamicRow()
		{
			bool flag2 = false;
			Auditai.Model.Row keyCellsRefTableRow_DynamicRow = _vm.GetKeyCellsRefTableRow_DynamicRow();
			Auditai.Model.Row row3 = null;
			HashSet<Auditai.Model.Row> hashSet2 = new HashSet<Auditai.Model.Row>();
			int rowsCount = _vm.GetRowsCount();
			int columnsCount = _vm.GetColumnsCount();
			for (int i = 0; i < rowsCount; i++)
			{
				TicketInputRowVM row4 = _vm.GetRow(i);
				if (row4.IsDynamicRowTicketDataRow && row4.TableRow != null)
				{
					if (keyCellsRefTableRow_DynamicRow != null && keyCellsRefTableRow_DynamicRow == row4.TableRow)
					{
						flag2 = true;
					}
					if (_vm.IsDataRowEmpty(i))
					{
						hashSet2.Add(row4.TableRow);
					}
					else if (row3 == null)
					{
						row3 = row4.TableRow;
					}
				}
			}
			hashSet2.UnionWith(_vm.RemovedRows);
			if (keyCellsRefTableRow_DynamicRow != null && _vm.IsKeyCellsRefTableRowBeNewAddedRow_DynamicRow())
			{
				if (flag2 && row3 == null)
				{
					hashSet2.Add(keyCellsRefTableRow_DynamicRow);
				}
				else if (!flag2)
				{
					if (_vm.IsTableRowDataColumnEmpty_DynamicRowTicket(keyCellsRefTableRow_DynamicRow) && _vm.IsTableRowKeyColumnEmpty_DynamicRowTicket(keyCellsRefTableRow_DynamicRow))
					{
						hashSet2.Add(keyCellsRefTableRow_DynamicRow);
					}
					else if (row3 != null)
					{
						hashSet2.Add(keyCellsRefTableRow_DynamicRow);
					}
				}
			}
			if (row3 == null && keyCellsRefTableRow_DynamicRow != null && !hashSet2.Contains(keyCellsRefTableRow_DynamicRow))
			{
				row3 = keyCellsRefTableRow_DynamicRow;
			}
			DeleteTableRows(hashSet2);
			return row3;
		}
		Auditai.Model.Row RemoveTableEmptyRow_FixedMultiRow()
		{
			Auditai.Model.Row row2 = null;
			HashSet<Auditai.Model.Row> hashSet = new HashSet<Auditai.Model.Row>();
			foreach (TicketFixedMultiRowVM fixedMultiRowVM in _vm.FixedMultiRowVMs)
			{
				if (fixedMultiRowVM.Row != null)
				{
					bool flag = true;
					foreach (TicketInputCellVM valueCell in fixedMultiRowVM.ValueCells)
					{
						if (valueCell.TableCell != null)
						{
							if (!valueCell.TableCell.IsEmpty)
							{
								flag = false;
								break;
							}
							if (HasFillingFormula)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						hashSet.Add(fixedMultiRowVM.Row);
						fixedMultiRowVM.Row = null;
					}
					else if (row2 == null)
					{
						row2 = fixedMultiRowVM.Row;
					}
				}
			}
			DeleteTableRows(hashSet);
			return row2;
		}
		Auditai.Model.Row RemoveTableEmptyRow_FixedOneRow()
		{
			if (!_vm.IsFixedOneRowTicketRefTableRowEmpty())
			{
				return _vm.GetAnyOrDefaultTableRow();
			}
			_vm.GetAnyOrDefaultTableRow()?.Remove();
			return null;
		}
		Auditai.Model.Row RemoveTableEmptyRow_MixTicket()
		{
			Auditai.Model.Row row5 = null;
			HashSet<Auditai.Model.Row> hashSet3 = new HashSet<Auditai.Model.Row>();
			int rowsCount2 = _vm.GetRowsCount();
			int columnsCount2 = _vm.GetColumnsCount();
			for (int k = 0; k < rowsCount2; k++)
			{
				TicketInputRowVM row6 = _vm.GetRow(k);
				if (row6.TableRow != null && (row6.IsMixTicketFixedDataRow || row6.IsMixTicketDynamicDataRow))
				{
					if (_vm.IsMixTicketDataRowEmpty(k))
					{
						hashSet3.Add(row6.TableRow);
					}
					else if (row5 == null)
					{
						row5 = row6.TableRow;
					}
				}
			}
			hashSet3.UnionWith(_vm.RemovedRows);
			Auditai.Model.Row keyCellsRefTableRow_MixTicket = _vm.GetKeyCellsRefTableRow_MixTicket();
			if (keyCellsRefTableRow_MixTicket != null && _vm.IsKeyCellsRefTableRowBeNewAddedRow_MixTicket())
			{
				if (row5 != null)
				{
					hashSet3.Add(keyCellsRefTableRow_MixTicket);
				}
				else if (_vm.IsTableRowDataColumnEmpty_MixTicket(keyCellsRefTableRow_MixTicket) && _vm.IsTableRowKeyColumnEmpty_MixTicket(keyCellsRefTableRow_MixTicket))
				{
					hashSet3.Add(keyCellsRefTableRow_MixTicket);
				}
				else if (_vm.IsTableRowDataColumnEmpty_OnlyCheckAutoFillColumn_MixTicket(keyCellsRefTableRow_MixTicket) && !_vm.ChangeEmptyTableRowToAvailableDynamicDataRow_MixTicket(keyCellsRefTableRow_MixTicket))
				{
					hashSet3.Add(keyCellsRefTableRow_MixTicket);
				}
			}
			if (row5 == null && keyCellsRefTableRow_MixTicket != null && !hashSet3.Contains(keyCellsRefTableRow_MixTicket))
			{
				row5 = keyCellsRefTableRow_MixTicket;
			}
			DeleteTableRows(hashSet3);
			return row5;
		}
	}

	public void CancelSave()
	{
		if (Table != null)
		{
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
			SaveGridSelectRangeAndScrollPosition();
			ClearRecordFilterSetting();
			Table.NeedSave = false;
			Program.MainForm.TableEditor.ReloadFromDb();
			ScrollCurrentNavTreeSelectionToVisible();
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			RestoreGridSelectRangeAndScrollPosition();
		}
	}

	public void SetTheme()
	{
		Theme.SetCurrentObject(View);
		_grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_grid.Styles.EmptyArea.BackColor = _editorPanel.BackColor;
		_grid.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_grid.Styles.Normal.WordWrap = true;
		_colorBorder = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
		_btnInsertRow.FlatStyle = FlatStyle.Flat;
		_btnInsertRow.FlatAppearance.BorderSize = 0;
		_btnRemoveRow.FlatStyle = FlatStyle.Flat;
		_btnRemoveRow.FlatAppearance.BorderSize = 0;
		if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			_imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			_imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		_imageProcess.ProcessImage();
		Theme.SetCurrentObject(ListDropDown.DropDown);
		SetThemeNavs();
	}

	public void AddNav()
	{
		FormTicketNav formTicketNav = new FormTicketNav();
		formTicketNav.Title = "新建导航树";
		formTicketNav.InputVM = _vm;
		formTicketNav.Ticket = Ticket;
		if (formTicketNav.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		GridBeginUpdate();
		try
		{
			SuspendSaveNavTreeScrollPosition();
			Ticket.Navs.Add(new TicketNav
			{
				Columns = ((System.Collections.IEnumerable)formTicketNav.Result).Cast<Auditai.Model.Column>().Select((Auditai.Model.Column c) => c.Id).ToList()
			});
			Table.TagTicketDirty();
			PopulateNavs();
			_otbNavs.SelectedIndex = _navGrids.Count - 1;
			_currentNavGrid.FindAndSelectRecord(Record);
			_currentRecord = _currentNavGrid.GetCurrentIndex();
			ResumeSaveNavTreeScrollPosition();
			_gridDecorator.SetCellFlickerDirty();
			_gridDecorator.ReBuildNavTreeFlicker();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	private int GetOtbNavsHotPageIndex()
	{
		if (_navGrids == null)
		{
			return -1;
		}
		C1OutPage hotPage = _otbNavs.HotPage;
		if (hotPage == null)
		{
			return -1;
		}
		if (!_pageNavGridDic.TryGetValue(hotPage, out var value))
		{
			return -1;
		}
		for (int i = 0; i < _navGrids.Count; i++)
		{
			if (_navGrids[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public void ModifyNav()
	{
		if (Ticket == null || Ticket.Navs == null)
		{
			return;
		}
		int otbNavsHotPageIndex = GetOtbNavsHotPageIndex();
		if (otbNavsHotPageIndex < 0 || otbNavsHotPageIndex >= Ticket.Navs.Count)
		{
			return;
		}
		FormTicketNav formTicketNav = new FormTicketNav();
		formTicketNav.Title = "修改导航树";
		formTicketNav.InputVM = _vm;
		formTicketNav.Ticket = Ticket;
		formTicketNav.Result = Ticket.Navs[otbNavsHotPageIndex].Columns.Select((Id64 id) => Table.Columns.GetById(id)).ToList();
		if (formTicketNav.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		GridBeginUpdate();
		try
		{
			SuspendSaveNavTreeScrollPosition();
			TicketNavTreeStatusDataCacher.RemoveNavTreeByPageIndex(Table.Id, otbNavsHotPageIndex);
			Ticket.Navs[otbNavsHotPageIndex] = new TicketNav
			{
				Columns = ((System.Collections.IEnumerable)formTicketNav.Result).Cast<Auditai.Model.Column>().Select((Auditai.Model.Column c) => c.Id).ToList()
			};
			Table.TagTicketDirty();
			PopulateNavs();
			PopulateRecord();
			ExpandNavTreeToCurrentRecord();
			ResumeSaveNavTreeScrollPosition();
			_gridDecorator.SetCellFlickerDirty();
			_gridDecorator.ReBuildNavTreeFlicker();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	public void DeleteNav()
	{
		if (Ticket == null || Ticket.Navs == null)
		{
			return;
		}
		int otbNavsHotPageIndex = GetOtbNavsHotPageIndex();
		if (otbNavsHotPageIndex < 0 || otbNavsHotPageIndex >= Ticket.Navs.Count)
		{
			return;
		}
		GridBeginUpdate();
		try
		{
			TicketNavTreeStatusDataCacher.ResetNavTreeIDWithRemovePageIndex(Table.Id, otbNavsHotPageIndex);
			if (otbNavsHotPageIndex >= 0 && otbNavsHotPageIndex < _navTreeScollPositionList.Count)
			{
				_navTreeScollPositionList.RemoveAt(otbNavsHotPageIndex);
			}
			SuspendSaveNavTreeScrollPosition();
			Ticket.Navs.RemoveAt(otbNavsHotPageIndex);
			Table.TagTicketDirty();
			PopulateNavs();
			if (Ticket.Navs.Count > 0)
			{
				_currentNavGrid.FindAndSelectRecord(Record);
				if (Ticket.Navs.Count == 0)
				{
					_currentRecord = Ticket.Records.IndexOf(Record);
				}
				else
				{
					_currentRecord = _currentNavGrid.GetCurrentIndex();
				}
				ExpandNavTreeToCurrentRecord();
			}
			ResumeSaveNavTreeScrollPosition();
			_gridDecorator.SetCellFlickerDirty();
			_gridDecorator.ReBuildNavTreeFlicker();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	public void PopulateNavs()
	{
		if (Ticket == null || Table == null)
		{
			return;
		}
		_otbNavs.SuspendDrawing();
		_otbNavs.BeginUpdate();
		_skipOtbIndexChange = true;
		int selectedIndex = _otbNavs.SelectedIndex;
		_navGrids.Clear();
		foreach (OutPageData inUsingPage in _inUsingPageList)
		{
			_freePageList.Enqueue(inUsingPage);
		}
		_inUsingPageList.Clear();
		_otbNavs.Pages.Clear();
		for (int num = Ticket.Navs.Count - 1; num >= 0; num--)
		{
			TicketNav ticketNav = Ticket.Navs[num];
			if (ticketNav.Columns.Any((Id64 id) => Table.Columns.GetById(id) == null))
			{
				Ticket.Navs.RemoveAt(num);
				Table.TagTicketDirty();
			}
		}
		_pageNavGridDic.Clear();
		_navGridPagesDic.Clear();
		_navTreeScollPositionList = new List<NavTreeScrollPosition>(Ticket.Navs.Count);
		for (int i = 0; i < Ticket.Navs.Count; i++)
		{
			TicketNav ticketNav2 = Ticket.Navs[i];
			C1OutPage c1OutPage = null;
			TicketNavGrid ticketNavGrid = null;
			if (_freePageList.Count > 0)
			{
				OutPageData outPageData = _freePageList.Dequeue();
				c1OutPage = outPageData.page;
				ticketNavGrid = outPageData.navGrid;
				ticketNavGrid.Reset();
				c1OutPage.Image = Resources.TicketNav;
				_inUsingPageList.Add(outPageData);
			}
			else
			{
				c1OutPage = new C1OutPage();
				ticketNavGrid = new TicketNavGrid();
				c1OutPage.Image = Resources.TicketNav;
				ticketNavGrid.RecordSelected += _ticketNavGrid_RecordSelected;
				ticketNavGrid.VirtualNodeSelected += _ticketNavGrid_VirtualNodeSelected;
				_inUsingPageList.Add(new OutPageData
				{
					page = c1OutPage,
					navGrid = ticketNavGrid
				});
				C1Command c1Command = new C1Command
				{
					Text = "搜索" + Ticket.GetLevelString(),
					Image = ContextResources.ctxSearch
				};
				c1Command.Click += CmdSearchTicket_Click;
				C1CommandLink value = new C1CommandLink
				{
					Command = c1Command,
					Delimiter = true
				};
				ticketNavGrid.Ctx.CommandLinks.Insert(0, value);
				C1Command c1Command2 = new C1Command
				{
					Text = "删除" + Ticket.GetLevelString(),
					Image = Resources.RemoveTicketNav
				};
				c1Command2.CommandStateQuery += ticketNavGrid.CmdDeleteTicket_CommandStateQuery;
				c1Command2.Click += CmdRemoveTicket_Click;
				C1CommandLink value2 = new C1CommandLink
				{
					Command = c1Command2
				};
				ticketNavGrid.Ctx.CommandLinks.Insert(0, value2);
				C1Command c1Command3 = new C1Command
				{
					Text = "新增" + Ticket.GetLevelString(),
					Image = Resources.AddTicketNav
				};
				c1Command3.CommandStateQuery += ticketNavGrid.CmdAddTicket_CommandStateQuery;
				c1Command3.Click += CmdAddTicket_Click;
				C1CommandLink value3 = new C1CommandLink
				{
					Command = c1Command3
				};
				ticketNavGrid.Ctx.CommandLinks.Insert(0, value3);
			}
			_navGrids.Add(ticketNavGrid);
			ticketNavGrid.Nav = ticketNav2.Columns.Select((Id64 id) => Table.Columns.GetById(id)).ToList();
			c1OutPage.Text = TicketNavToString(ticketNavGrid.Nav);
			ticketNavGrid.Ticket = Ticket;
			ticketNavGrid.NavSetting = ticketNav2;
			ticketNavGrid.IsHasFillingFormula = HasFillingFormula;
			ticketNavGrid.IsAllowModifyTableRowOrder = IsAllowModifyTableRowOrder;
			ticketNavGrid.Populate();
			c1OutPage.Controls.Add(ticketNavGrid.View);
			_otbNavs.Pages.Add(c1OutPage);
			_navGridPagesDic.Add(ticketNavGrid, Tuple.Create(c1OutPage, c1OutPage.Text));
			_pageNavGridDic.Add(c1OutPage, ticketNavGrid);
			TicketNavTreeID ticketNavTreeID = TicketNavTreeStatusDataCacher.GenerateTicketNavTreeID(i, ticketNavGrid.Nav);
			_navTreeScollPositionList.Add(TicketNavTreeStatusDataCacher.GetNavTreeLastScrollPosition(Table.Id, ticketNavTreeID));
			TicketNavTreeStatusDataCacher.RestoreNavTreeCollapseStatus(Table.Id, ticketNavTreeID, ticketNavGrid);
			ticketNavGrid.NavTreeID = ticketNavTreeID;
			ticketNavGrid.NavTreeName = c1OutPage.Text;
		}
		_otbNavs.SelectedIndex = TicketNavTreeStatusDataCacher.GetLastSelectedNavTreePageIndex(Table.Id, _otbNavs.Pages.Count - 1);
		if (_otbNavs.SelectedIndex < 0)
		{
			_otbNavs.SelectedIndex = _otbNavs.Pages.Count - 1;
		}
		if (_otbNavs.SelectedIndex >= 0 && _otbNavs.SelectedIndex < _navGrids.Count)
		{
			_preSelectedNavGrid = _navGrids[_otbNavs.SelectedIndex];
		}
		_skipOtbIndexChange = false;
		UpdateNavGirdNodeValidationFailedCount();
		_otbNavs.EndUpdate();
		_otbNavs.ResumeDrawing();
	}

	public void RefreshValidationResult()
	{
		if (Table == null || _owner.CurrentView != MainFormView.TicketInput)
		{
			return;
		}
		SuspendDrawing();
		try
		{
			CalculateTicketRecordValidationFailedCount();
			UpdateNavGirdNodeValidationFailedCount();
			_grid.Invalidate();
			TitleEditor.View.Invalidate();
			FooterEditor.View.Invalidate();
			foreach (TicketNavGrid navGrid in _navGrids)
			{
				navGrid.View.Invalidate();
			}
		}
		catch
		{
		}
		finally
		{
			ResumeDrawing();
		}
	}

	private void PopuldateNavGridValidationFailedCount()
	{
		Ticket.TicketReocrdRefreshCallback = OnTicketRecordChanged;
		CalculateTicketRecordValidationFailedCount();
	}

	private void OnTicketRecordChanged(TicketTable ticket)
	{
		if (!_isSuspendTicketRecordChangeEvent && (ticket.Table == Table || _owner.CurrentView == MainFormView.TicketInput))
		{
			CalculateTicketRecordValidationFailedCount();
		}
	}

	private void CalculateTicketRecordValidationFailedCount()
	{
		_recordValidationErrorCountDic.Clear();
		if (!Program.MainForm.ShowHelperTooltip || !Program.MainForm.TableValidationResults.TryGetValue(Table.TreeNode, out var value) || value.ErrorRefs.Count == 0)
		{
			return;
		}
		_isSuspendTicketRecordChangeEvent = true;
		try
		{
			HashSet<Id64> refColsIdSet = Ticket.GetReferencedTableColumnId();
			foreach (TicketRecord record in Ticket.Records)
			{
				int cellCount = 0;
				int rpCount = 0;
				HashSet<Id64> refRowsId = new HashSet<Id64>(record.Rows.Select((Auditai.Model.Row r) => r.Id));
				int value2 = value.ErrorRefs.Sum(delegate(object u)
				{
					if (u is Auditai.Model.Cell cell)
					{
						cellCount++;
						if (!refRowsId.Contains(cell.Row.Id) || !refColsIdSet.Contains(cell.Column.Id))
						{
							return 0;
						}
						return 1;
					}
					if (u is Auditai.Model.Column column)
					{
						return refColsIdSet.Contains(column.Id) ? 1 : 0;
					}
					if (u is RangeOperand rangeOperand)
					{
						rpCount++;
						return rangeOperand.Cells.Any((Auditai.Model.Cell c) => refRowsId.Contains(c.Row.Id) && refColsIdSet.Contains(c.Column.Id)) ? 1 : 0;
					}
					return 0;
				});
				_recordValidationErrorCountDic.Add(record, value2);
			}
		}
		catch (Exception exception)
		{
			exception.Log("计算表单上的校验错误数量时发生了未预期的异常");
		}
		finally
		{
			_isSuspendTicketRecordChangeEvent = false;
		}
	}

	private void UpdateNavGirdNodeValidationFailedCount()
	{
		if (_recordValidationErrorCountDic.Count == 0)
		{
			foreach (Tuple<C1OutPage, string> value3 in _navGridPagesDic.Values)
			{
				if (!(value3.Item1.Text == value3.Item2))
				{
					value3.Item1.Text = value3.Item2;
					value3.Item1.Invalidate();
				}
			}
			return;
		}
		foreach (TicketNavGrid navGrid in _navGrids)
		{
			C1FlexGridEx view = navGrid.View;
			int count = view.Rows.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = view.Rows[i];
				if (row.IsNode && row.Node.Level == 0)
				{
					num += UpdateValidationFailedCount(row.Node);
				}
			}
			if (_navGridPagesDic.TryGetValue(navGrid, out var value))
			{
				string text = ((num != 0 && Program.MainForm.ShowHelperTooltip) ? $"{value.Item2} ({num})" : value.Item2);
				if (value.Item1.Text != text)
				{
					value.Item1.Text = text;
					value.Item1.Invalidate();
				}
			}
		}
		int UpdateValidationFailedCount(Node node)
		{
			int num2 = 0;
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				num2 += UpdateValidationFailedCount(node2);
			}
			if (node.Key is TicketNavGrid.NavNode navNode)
			{
				if (navNode.Record != null && _recordValidationErrorCountDic.TryGetValue(navNode.Record, out var value2))
				{
					num2 += value2;
				}
				navNode.ValidationCheckFailedCount = num2;
			}
			return num2;
		}
	}

	public void ExportXlsx()
	{
		TicketExportXlsx ticketExportXlsx = new TicketExportXlsx();
		ticketExportXlsx.Ticket = Ticket;
		ticketExportXlsx.VM = _vm;
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = "Excel 工作簿|*.xlsx";
		saveFileDialog.DefaultExt = "xlsx";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			try
			{
				ticketExportXlsx.WaterMarkPageSetup = Program.MainForm?.GenerateExcelExportWaterMarkSetting();
				ticketExportXlsx.Generate();
				ticketExportXlsx.Save(saveFileDialog.FileName);
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件导出成功");
			}
			catch (Exception ex)
			{
				ex.Log("表单导出到Excel时发生了未预期的异常");
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
		}
	}

	private bool IsSelectedRangeOnlyContainsFormulaArea(C1.Win.C1FlexGrid.CellRange gridRange)
	{
		int @fixed = _grid.Rows.Fixed;
		int fixed2 = _grid.Cols.Fixed;
		for (int i = gridRange.LeftCol; i <= gridRange.RightCol; i++)
		{
			if (i < fixed2 || i >= _grid.Cols.Count || !_grid.Cols[i].IsVisible)
			{
				continue;
			}
			int col = ConvertGridColIndexToVMColIndex(i);
			for (int j = gridRange.TopRow; j <= gridRange.BottomRow; j++)
			{
				if (j >= @fixed && j < _grid.Rows.Count)
				{
					int row = ConvertGridRowIndexToVMRowIndex(j);
					TicketInputCellVM cellVM = _vm.GetCellVM(row, col);
					if (!cellVM.IsFormula && cellVM.Column != null && !cellVM.IsFixedMultiRowKey)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void BuildTableCellForSelectRange()
	{
		ChangeVirtualValueToRealValue();
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			int num = ConvertGridRowIndexToVMRowIndex(selection.TopRow);
			int num2 = ConvertGridRowIndexToVMRowIndex(selection.BottomRow) - num + 1;
			if (num < 0)
			{
				num = 0;
			}
			if (num + num2 >= _vm.GetRowsCount())
			{
				num2 = _vm.GetRowsCount() - num;
			}
			_vm.BuildTableRowsForTicketDataRows_DynamicRow(num, num2);
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			C1.Win.C1FlexGrid.CellRange selection2 = _grid.Selection;
			int num3 = ConvertGridRowIndexToVMRowIndex(selection2.TopRow);
			int num4 = ConvertGridRowIndexToVMRowIndex(selection2.BottomRow) - num3 + 1;
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (num3 + num4 >= _vm.GetRowsCount())
			{
				num4 = _vm.GetRowsCount() - num3;
			}
			_vm.BuildTableRowsForTicketDataRows_MixTicket(num3, num4);
		}
		else
		{
			_vm.BuildTableCellForAllTicketCell();
		}
	}

	private void BuildTableCellForTicketCell(int gridRowIndex, int girdColIndex)
	{
		ChangeVirtualValueToRealValue();
		int cellRowIndex = ConvertGridRowIndexToVMRowIndex(gridRowIndex);
		int cellColIndex = ConvertGridColIndexToVMColIndex(girdColIndex);
		_vm.BuildTableCellForTicketCell(cellRowIndex, cellColIndex);
	}

	public void Cut()
	{
		if (_isTicketLocked || IsPreventCutOrDeleteCellValue())
		{
			return;
		}
		bool flag = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		_vm.BeginBatchUpdateValue();
		if (!Table.IsLocked)
		{
			BuildTableCellForSelectRange();
		}
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			int num = i + _grid.Rows.Fixed;
			if (_grid.IsRowIndexOutOfRange(num) || !_grid.Rows[num].IsVisible)
			{
				continue;
			}
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(i, j);
				if (!_grid.Cols[ConvertVMColIndexToGridColIndex(j)].IsVisible)
				{
					continue;
				}
				stringBuilder.Append(gridBodyCellVM.GetDisplayValue());
				if (!Table.IsLocked && CanEditCell(gridBodyCellVM))
				{
					if (gridBodyCellVM.IsFormula)
					{
						if (gridBodyCellVM.IsAllowManualInputOnFormula && flag)
						{
							_vm.UpdateTicketCellValue(gridBodyCellVM, "", isFormulaExistManualInputValue: true);
						}
					}
					else
					{
						_vm.UpdateTicketCellValue(gridBodyCellVM, "", isFormulaExistManualInputValue: false);
					}
				}
				if (j < bodySelection.RightCol)
				{
					stringBuilder.Append("\t");
				}
			}
			stringBuilder.Append("\r\n");
		}
		_vm.EndBatchUpdateValue();
		_vm.CalculateTicket();
		try
		{
			Clipboard.SetText(stringBuilder.ToString());
		}
		catch (Exception exception)
		{
			exception.Log("向剪贴板粘贴数据失败");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "无法向剪贴板上写入数据！", MessageBoxButtons.OK, "剪贴板错误");
		}
		_grid.Invalidate();
		TitleEditor.Invalidate();
		FooterEditor.Invalidate();
		SetCommandState();
	}

	public void Copy()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			int num = i + _grid.Rows.Fixed;
			if (_grid.IsRowIndexOutOfRange(num) || !_grid.Rows[num].IsVisible)
			{
				continue;
			}
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(i, j);
				if (_grid.Cols[ConvertVMColIndexToGridColIndex(j)].IsVisible)
				{
					stringBuilder.Append(gridBodyCellVM.GetDisplayValue());
					if (j < bodySelection.RightCol)
					{
						stringBuilder.Append("\t");
					}
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

	public static object ConvertCopyValueToCellValue(object value, Type dataType)
	{
		if (dataType == typeof(double) && value is string text && text.EndsWith("%") && double.TryParse(text.TrimEnd('%'), out var result))
		{
			return result / 100.0;
		}
		if (dataType == typeof(bool))
		{
			string text2 = value as string;
			switch (text2)
			{
			case "√":
			case "":
				return text2 == "√";
			}
		}
		return Auditai.Model.Cell.ChangeDataTypeImpl(value, dataType);
	}

	public void Paste()
	{
		if (Table.IsLocked || _isTicketLocked || _grid.BodyRow < 0 || _grid.BodyCol < 0 || (_isInShowingVirtualNode && !SoftwareLicenseManager.IsAllowAddTableRows()))
		{
			return;
		}
		SuspendDrawing();
		try
		{
			Point scrollPosition = _grid.ScrollPosition;
			List<List<object>> list = ClipboardUtil.GetClipboardAsTable();
			if (list == null || list.Count == 0)
			{
				return;
			}
			if (list != null && list.Count > 0)
			{
				int currentLicenseAllowPasteTableRowsCount = SoftwareLicenseManager.GetCurrentLicenseAllowPasteTableRowsCount(list.Count);
				if (currentLicenseAllowPasteTableRowsCount < list.Count)
				{
					List<List<object>> list2 = new List<List<object>>();
					for (int i = 0; i < currentLicenseAllowPasteTableRowsCount; i++)
					{
						list2.Add(list[i]);
					}
					list = list2;
				}
				if (SoftwareLicenseManager.IsTableRowsAndColsOutOfLicenseLimit(list.Count, 0))
				{
					return;
				}
				if (!SoftwareLicenseManager.IsAllowAddTableRows(showDialog: false))
				{
					int topRow = _grid.Selection.TopRow;
					int columnsCount = _vm.GetColumnsCount();
					for (int j = 0; j < list.Count; j++)
					{
						int num = ConvertGridRowIndexToVMRowIndex(topRow + j);
						for (int k = 0; k < columnsCount; k++)
						{
							if (_vm.IsIndexOutOfRange(num, k))
							{
								SoftwareLicenseManager.IsAllowAddTableRows();
								return;
							}
							TicketInputCellVM cellVM = _vm.GetCellVM(num, k);
							if (cellVM.Column != null && cellVM.TableCell == null)
							{
								SoftwareLicenseManager.IsAllowAddTableRows();
								return;
							}
						}
					}
				}
				_vm.BeginBatchUpdateValue();
				_vm.StartRecordNewAddTableRows();
				ChangeVirtualValueToRealValue();
				int topRow2 = _grid.Selection.TopRow;
				int leftCol = _grid.Selection.LeftCol;
				int rightCol = _grid.Selection.RightCol;
				bool isFiltering = _grid.FilterManager.IsFiltering;
				if (isFiltering)
				{
					SaveRecordFilterSetting();
				}
				int num2 = -1;
				int count = list.Count;
				int count2 = list[0].Count;
				int num3 = ConvertGridRowIndexToVMRowIndex(_grid.Selection.TopRow);
				int num4 = Math.Max(count, ConvertGridRowIndexToVMRowIndex(_grid.Selection.BottomRow) - num3 + 1);
				if (!isFiltering)
				{
					if (Ticket.Kind == TicketKind.DynamicRow)
					{
						num4 = Math.Min(num4, Ticket.DataRowStart + _vm.DataRowsCount - num3);
					}
					else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
					{
						int num5 = 0;
						for (int l = num3; l < _vm.GetRowsCount(); l++)
						{
							TicketInputRowVM row = _vm.GetRow(l);
							if (row.IsMixTicketDynamicDataRow)
							{
								int mixRangeDynamicDataRowTemplateId = row.TicketRow.MixRangeDynamicDataRowTemplateId;
								num2 = l;
								for (int m = l; m < _vm.GetRowsCount(); m++)
								{
									row = _vm.GetRow(m);
									if (!row.IsMixTicketDynamicDataRow || row.TicketRow.MixRangeDynamicDataRowTemplateId != mixRangeDynamicDataRowTemplateId)
									{
										break;
									}
									num5++;
									if (num5 >= count)
									{
										break;
									}
								}
								break;
							}
							num5++;
							if (num5 >= count)
							{
								break;
							}
						}
						num4 = num5;
					}
					else
					{
						num4 = Math.Min(num4, _vm.GetRowsCount() - num3);
					}
					if (!HasFillingFormula)
					{
						if (Ticket.Kind == TicketKind.DynamicRow && num4 < count)
						{
							_vm.AppendDataRows(count - num4);
							num4 = count;
						}
						else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow && num4 < count && num2 != -1)
						{
							int mixTicketDynamicDataRowRangeLastDataRowNextRowIndex = GetMixTicketDynamicDataRowRangeLastDataRowNextRowIndex(num2);
							if (mixTicketDynamicDataRowRangeLastDataRowNextRowIndex != -1)
							{
								_vm.InsertDataRows_MixTicket(mixTicketDynamicDataRowRangeLastDataRowNextRowIndex, count - num4, _vm.GetRow(num2).TicketRow.MixRangeDynamicDataRowTemplateId);
								num4 = count;
							}
						}
					}
				}
				else
				{
					int num6 = 0;
					int bodyRowsCount = _grid.BodyRowsCount;
					int num7 = 0;
					for (int n = 0; n < bodyRowsCount; n++)
					{
						num7++;
						int num8 = ConvertVMRowIndexToGridRowIndex(num3 + n);
						if (_grid.IsRowIndexOutOfRange(num8))
						{
							break;
						}
						if (_grid.Rows[num8].IsVisible)
						{
							num6++;
							if (num6 >= num4)
							{
								break;
							}
						}
					}
					num4 = num7;
				}
				if (Ticket.Kind == TicketKind.DynamicRow)
				{
					_vm.BuildTableRowsForTicketDataRows_DynamicRow(num3, num4);
				}
				else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
				{
					_vm.BuildTableRowsForTicketDataRows_MixTicket(num3, num4);
				}
				else
				{
					_vm.BuildTableCellForAllTicketCell();
				}
				bool flag = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
				int num9 = ConvertGridColIndexToVMColIndex(_grid.Selection.LeftCol);
				int num10 = Math.Min(Math.Max(count2, ConvertGridColIndexToVMColIndex(_grid.Selection.RightCol) - num9 + 1), Ticket.Columns.Count - num9);
				int num11 = 0;
				int num12 = 0;
				int num13 = 0;
				while (num11 < num10)
				{
					int num14 = num9 + num13;
					if (num14 >= _vm.GetColumnsCount())
					{
						break;
					}
					num12++;
					if (_grid.Cols[ConvertVMColIndexToGridColIndex(num14)].IsVisible)
					{
						num11++;
					}
					num13++;
				}
				int num15 = 0;
				for (int num16 = 0; num16 < num4; num16++)
				{
					int num17 = ConvertVMRowIndexToGridRowIndex(num3 + num16);
					if ((!_grid.IsRowIndexOutOfRange(num17) && !_grid.Rows[num17].IsVisible) || _vm.IsRowIndexOutOfRange(num3 + num16))
					{
						continue;
					}
					List<object> list3 = list[num15++ % count];
					num11 = 0;
					for (int num18 = 0; num18 < num12; num18++)
					{
						if (!_grid.Cols[ConvertVMColIndexToGridColIndex(num9 + num18)].IsVisible)
						{
							continue;
						}
						object obj = list3[num11++ % count2];
						TicketInputCellVM cellVM2 = _vm.GetCellVM(num3 + num16, num9 + num18);
						if (!CanEditCell(cellVM2))
						{
							continue;
						}
						bool isFormulaExistManualInputValue = false;
						if (cellVM2.IsFormula)
						{
							if (!cellVM2.IsAllowManualInputOnFormula || !flag)
							{
								continue;
							}
							isFormulaExistManualInputValue = true;
						}
						if (cellVM2.TableCell != null)
						{
							try
							{
								Type displayDataType = cellVM2.TableCell.DisplayDataType;
								object newValue = ConvertCopyValueToCellValue(obj, displayDataType);
								_vm.UpdateTicketCellValue(cellVM2, newValue, isFormulaExistManualInputValue);
							}
							catch
							{
							}
						}
						else if (cellVM2.DataFormat.HasValue)
						{
							try
							{
								Type dataType = cellVM2.DataFormat.Value.GetDataType();
								object newValue2 = ConvertCopyValueToCellValue(obj, dataType);
								_vm.UpdateTicketCellValue(cellVM2, newValue2, isFormulaExistManualInputValue);
							}
							catch
							{
							}
						}
						else
						{
							_vm.UpdateTicketCellValue(cellVM2, obj, isFormulaExistManualInputValue);
						}
					}
				}
				_vm.EndBatchUpdateValue();
				try
				{
					_vm.CalculateTicket();
					_isDirty = true;
					PopulateVm();
					if (isFiltering)
					{
						RestoreRecordFilterSetting();
					}
					_grid.SafeSelect(topRow2, leftCol, topRow2 + num4 - 1, leftCol + num12 - 1);
					_grid.ScrollPosition = scrollPosition;
				}
				catch (Exception exception)
				{
					exception.Log("表单输入界面粘贴单元格时发生了未预期的异常");
				}
			}
			SetCommandState();
		}
		catch (TableModelException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			ResumeDrawing();
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

	public void IncreaseColumnWidth()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.IncreaseColumnWidth();
		}
		else if (_footerEditor.IsInEditing)
		{
			_footerEditor.IncreaseColumnWidth();
		}
		else
		{
			if (!IsExistSelectedCell())
			{
				return;
			}
			_editorPanel.SuspendDrawing();
			try
			{
				CacheSelectRange();
				C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
				for (int i = selection.LeftCol; i <= selection.RightCol; i++)
				{
					int index = ConvertGridColIndexToVMColIndex(i);
					Ticket.Columns[index].Width += 5;
				}
				Table.TagTicketDirty(isCanOverrideByServerData: true);
				PopulateVm();
				RestoreSelectRange();
			}
			finally
			{
				_editorPanel.ResumeDrawing();
			}
		}
	}

	public void DecreaseColumnWidth()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.DecreaseColumnWidth();
		}
		else if (_footerEditor.IsInEditing)
		{
			_footerEditor.DecreaseColumnWidth();
		}
		else
		{
			if (!IsExistSelectedCell())
			{
				return;
			}
			_editorPanel.SuspendDrawing();
			try
			{
				CacheSelectRange();
				C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
				for (int i = selection.LeftCol; i <= selection.RightCol; i++)
				{
					int index = ConvertGridColIndexToVMColIndex(i);
					Ticket.Columns[index].Width -= 5;
				}
				Table.TagTicketDirty(isCanOverrideByServerData: true);
				PopulateVm();
				RestoreSelectRange();
			}
			finally
			{
				_editorPanel.ResumeDrawing();
			}
		}
	}

	public void IncreaseRowHeight()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.IncreaseRowHeight();
		}
		else if (_footerEditor.IsInEditing)
		{
			_footerEditor.IncreaseRowHeight();
		}
		else
		{
			if (!IsExistSelectedCell())
			{
				return;
			}
			_editorPanel.SuspendDrawing();
			try
			{
				CacheSelectRange();
				HashSet<int> hashSet = null;
				bool flag = false;
				C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					int index = ConvertGridRowIndexToVMRowIndex(i);
					int height = _vm.GetRowHeight(index) + 5;
					if (_vm.IsRecordDataRow_DynamicRowTicket(index))
					{
						if (!flag)
						{
							_vm.SetRowHeight(index, height);
							flag = true;
						}
					}
					else if (_vm.GetRow(index).IsMixTicketDynamicDataRow)
					{
						if (hashSet == null)
						{
							hashSet = new HashSet<int>();
						}
						TicketRow ticketRow = _vm.GetRow(index).TicketRow;
						if (!hashSet.Contains(ticketRow.MixRangeDynamicDataRowTemplateId))
						{
							hashSet.Add(ticketRow.MixRangeDynamicDataRowTemplateId);
							_vm.SetRowHeight(index, height);
						}
					}
					else
					{
						_vm.SetRowHeight(index, height);
					}
				}
				Table.TagTicketDirty(isCanOverrideByServerData: true);
				PopulateVm();
				RestoreSelectRange();
			}
			finally
			{
				_editorPanel.ResumeDrawing();
			}
		}
	}

	public void DecreaseRowHeight()
	{
		if (_titleEditor.IsInEditing)
		{
			_titleEditor.DecreaseRowHeight();
		}
		else if (_footerEditor.IsInEditing)
		{
			_footerEditor.DecreaseRowHeight();
		}
		else
		{
			if (!IsExistSelectedCell())
			{
				return;
			}
			_editorPanel.SuspendDrawing();
			try
			{
				CacheSelectRange();
				HashSet<int> hashSet = null;
				bool flag = false;
				C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					int index = ConvertGridRowIndexToVMRowIndex(i);
					int num = _vm.GetRowHeight(index) - 5;
					if (num <= 0)
					{
						num = 1;
					}
					if (_vm.IsRecordDataRow_DynamicRowTicket(index))
					{
						if (!flag)
						{
							_vm.SetRowHeight(index, num);
							flag = true;
						}
					}
					else if (_vm.GetRow(index).IsMixTicketDynamicDataRow)
					{
						if (hashSet == null)
						{
							hashSet = new HashSet<int>();
						}
						TicketRow ticketRow = _vm.GetRow(index).TicketRow;
						if (!hashSet.Contains(ticketRow.MixRangeDynamicDataRowTemplateId))
						{
							_vm.SetRowHeight(index, num);
							hashSet.Add(ticketRow.MixRangeDynamicDataRowTemplateId);
						}
					}
					else
					{
						_vm.SetRowHeight(index, num);
					}
				}
				Table.TagTicketDirty(isCanOverrideByServerData: true);
				PopulateVm();
				RestoreSelectRange();
			}
			finally
			{
				_editorPanel.ResumeDrawing();
			}
		}
	}

	public void SetRowsHeight()
	{
		if (_grid.BodyRow < 0)
		{
			return;
		}
		int index = ConvertGridRowIndexToVMRowIndex(_grid.Selection.TopRow);
		int rowHeight = _vm.GetRowHeight(index);
		decimal? num = InputForm.Numeric("设置行高", "请输入行高，以像素为单位：", rowHeight);
		if (!num.HasValue)
		{
			return;
		}
		int num2 = (int)num.Value;
		if (num2 < 1)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能小于1");
			return;
		}
		if (num2 > 9999)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "行高不能大于9999");
			return;
		}
		SuspendDrawing();
		try
		{
			CacheSelectRange();
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				index = ConvertGridRowIndexToVMRowIndex(i);
				_vm.SetRowHeight(index, num2);
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			PopulateVm();
			RestoreSelectRange();
		}
		finally
		{
			ResumeDrawing();
		}
	}

	public void SetColumnsWidth()
	{
		if (_grid.BodyCol < 0)
		{
			return;
		}
		int index = ConvertGridColIndexToVMColIndex(_grid.Selection.LeftCol);
		int columnWidth = _vm.GetColumnWidth(index);
		decimal? num = InputForm.Numeric("设置列宽", "请输入列宽，以像素为单位：", columnWidth);
		if (!num.HasValue)
		{
			return;
		}
		int num2 = (int)num.Value;
		if (num2 < 1)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列宽不能小于1");
			return;
		}
		if (num2 > 9999)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "列宽不能大于9999");
			return;
		}
		SuspendDrawing();
		try
		{
			CacheSelectRange();
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			for (int i = selection.LeftCol; i <= selection.RightCol; i++)
			{
				index = ConvertGridColIndexToVMColIndex(i);
				Ticket.Columns[index].Width = num2;
			}
			Table.TagTicketDirty(isCanOverrideByServerData: true);
			PopulateVm();
			RestoreSelectRange();
		}
		finally
		{
			ResumeDrawing();
		}
	}

	public void SetPaperKind(PaperKind kind)
	{
		PageSetup.PaperKind = kind;
		Table.TagTicketDirty();
	}

	public void SetPaperCustom(double width, double height)
	{
		PageSetup.PaperKind = PaperKind.Custom;
		PageSetup.PaperWidth = Math.Max(width, PageSetup.LeftMargin + PageSetup.RightMargin + 50.0);
		PageSetup.PaperHeight = Math.Max(height, PageSetup.TopMargin + PageSetup.BottomMargin + 50.0);
		Table.TagTicketDirty();
	}

	public void Portrait()
	{
		PageSetup.Direction = Direction.Vertical;
		Table.TagTicketDirty();
	}

	public void Landscape()
	{
		PageSetup.Direction = Direction.Horizontal;
		Table.TagTicketDirty();
	}

	public void SetMarginLeft(double mm)
	{
		PageSetup.LeftMargin = mm;
		Table.TagTicketDirty();
	}

	public void SetMarginTop(double mm)
	{
		PageSetup.TopMargin = mm;
		Table.TagTicketDirty();
	}

	public void SetMarginRight(double mm)
	{
		PageSetup.RightMargin = mm;
		Table.TagTicketDirty();
	}

	public void SetMarginBottom(double mm)
	{
		PageSetup.BottomMargin = mm;
		Table.TagTicketDirty();
	}

	public void SetStartPage(int p)
	{
		PageSetup.StartPageNo = p;
		Table.TagTicketDirty();
	}

	public void SetOneColor(bool b)
	{
		PageSetup.OneColor = b;
		Table.TagTicketDirty();
	}

	public void SetFitWidth(bool value)
	{
		PageSetup.FitPageWidth = value;
	}

	public void SetFitHeight(bool value)
	{
		PageSetup.FitPageHeight = value;
		Table.TagTicketDirty();
	}

	public void SetWidthScale(double value)
	{
		PageSetup.HorizontalZoom = value;
		Table.TagTicketDirty();
	}

	public void SetHeightScale(double value)
	{
		PageSetup.VerticalZoom = value;
		Table.TagTicketDirty();
	}

	public void SetHeaderMargin(double value)
	{
		PageSetup.HeaderMargin = value;
		Table.TagTicketDirty();
	}

	public void SetFooterMargin(double value)
	{
		PageSetup.FooterMargin = value;
		Table.TagTicketDirty();
	}

	public void ExportPdf()
	{
		TicketPrinter ticketPrinter = Program.MainForm.TicketPrinter;
		ticketPrinter.Ticket = Ticket;
		ticketPrinter.SetVm();
		ticketPrinter.Populate();
		try
		{
			ticketPrinter.ExportPdf();
		}
		catch (IOException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "导出失败: " + ex.Message, MessageBoxButtons.OK, "错误!");
			ex.Log();
		}
	}

	public void HideTooltip()
	{
		_ttpComment.Hide();
		TitleEditor.HideTooltip();
		FooterEditor.HideTooltip();
	}

	public void AutoAdjustInputGridPosition()
	{
		GridBeginUpdate();
		try
		{
			AutoAdjustInputGridPositionImpl();
			_editorPanel.Invalidate();
		}
		finally
		{
			GridEndUpdate();
		}
	}

	protected void AutoAdjustInputGridPositionImpl()
	{
		AdjustInputGridPositionImpl();
		_titleEditor.AutoAdjustGridWidth();
		_footerEditor.AutoAdjustGridWidth();
		ShowRecordButtons();
	}

	private void CalculateGridSize(Size bound, out bool isNeedShowVScrollBar)
	{
		isNeedShowVScrollBar = false;
		int num = 0;
		for (int i = 0; i < _grid.Rows.Count; i++)
		{
			num += _grid.Rows[i].HeightDisplay;
		}
		int num2 = 0;
		for (int j = 0; j < _grid.Cols.Count; j++)
		{
			num2 += _grid.Cols[j].WidthDisplay;
		}
		bool flag = false;
		if (num2 > bound.Width)
		{
			flag = true;
			num += SystemInformation.HorizontalScrollBarHeight;
		}
		if (num > bound.Height)
		{
			num2 += SystemInformation.VerticalScrollBarWidth;
			if (num2 > bound.Width && !flag)
			{
				flag = true;
				num += SystemInformation.HorizontalScrollBarHeight;
			}
		}
		bool flag2 = num2 > bound.Width;
		bool flag3 = num > bound.Height;
		if (flag2)
		{
			_grid.Width = bound.Width;
			_grid.Left = 0;
		}
		else
		{
			_grid.Left = (bound.Width - num2) / 2;
			_grid.Width = num2;
		}
		if (flag3)
		{
			_grid.Height = bound.Height;
			_grid.Top = 0;
			isNeedShowVScrollBar = true;
		}
		else
		{
			_grid.Top = 0;
			_grid.Height = num;
		}
	}

	public bool IsGridExistVScrollbar()
	{
		return _isGridNeedShowVScrollBar;
	}

	private void AdjustInputGridPositionImpl()
	{
		_isGridNeedShowVScrollBar = false;
		int allRowsTotalHeight = _titleEditor.GetAllRowsTotalHeight();
		int allRowsTotalHeight2 = _footerEditor.GetAllRowsTotalHeight();
		int num = _editorPanel.Width - (Resources.ticketShadow.Width - 32);
		int num2 = _editorPanel.Height - (Resources.ticketShadow.Height - 32) - allRowsTotalHeight - allRowsTotalHeight2;
		if (num <= 0)
		{
			num = 1;
		}
		if (num2 <= 0)
		{
			num2 = 1;
		}
		CalculateGridSize(new Size(num, num2), out var isNeedShowVScrollBar);
		_isGridNeedShowVScrollBar = isNeedShowVScrollBar;
		int gridFixedColumnWidth = GetGridFixedColumnWidth();
		int num3 = (Resources.ticketShadow.Width - 32) / 2;
		int num4 = num3 + _grid.Left;
		int num5 = num4 + _grid.Width;
		num3 -= gridFixedColumnWidth / 2;
		if (isNeedShowVScrollBar)
		{
			num3 += SystemInformation.VerticalScrollBarWidth / 2;
		}
		int num6 = num3 + _grid.Left;
		if (num6 < 0)
		{
			num6 = 0;
		}
		_grid.Left = num6;
		int num7 = gridFixedColumnWidth;
		int num8 = (_editorPanel.Height - allRowsTotalHeight - _grid.Height - allRowsTotalHeight2) / 2;
		_titleEditor.View.Top = num8;
		_titleEditor.View.Left = _grid.Left + num7;
		int num9 = ((!_titleEditor.IsLastRowExistBottomBorder() && !IsFirstRowExistTopBorder()) ? 1 : 0);
		_grid.Top = num8 + allRowsTotalHeight - num9;
		num9 = ((!_footerEditor.IsFirstRowExistTopBorder()) ? 1 : 0);
		_footerEditor.View.Top = _grid.Top + _grid.Height - num9;
		_footerEditor.View.Left = _titleEditor.View.Left;
		int num10 = Resources.ticketShadow.Width / 2;
		int num11 = Resources.ticketShadow.Height / 2;
		int num12 = 16;
		int num13 = num10 - num12;
		int width = Resources.ticketShadow.Width - num13 - 32;
		int num14 = num11 - num12;
		int height = Resources.ticketShadow.Height - num14 - 32;
		_isNeedPaintShadow = true;
		if (_isNeedPaintShadow)
		{
			int num15 = num5 - num4;
			int num16 = _footerEditor.View.Top + allRowsTotalHeight2 - _titleEditor.View.Top;
			int num17 = num4;
			int top = _titleEditor.View.Top;
			_shadowAreaRangeArr[3] = new Rectangle(num17 - num13, top - num14, num13, num14);
			_shadowAreaRangeArr[1] = new Rectangle(num17, top - num14, num15, num14);
			_shadowAreaRangeArr[6] = new Rectangle(num17 + num15, top - num14, width, num14);
			_shadowAreaRangeArr[4] = new Rectangle(num17 - num13, top, num13, num16);
			_shadowAreaRangeArr[7] = new Rectangle(num17 + num15, top, width, num16);
			_shadowAreaRangeArr[5] = new Rectangle(num17 - num13, top + num16, num13, height);
			_shadowAreaRangeArr[2] = new Rectangle(num17, top + num16, num15, height);
			_shadowAreaRangeArr[8] = new Rectangle(num17 + num15, top + num16, width, height);
		}
	}

	public int GetGridControlWidth()
	{
		return _grid.Width;
	}

	public bool IsLastRowExistBottomBorder()
	{
		int rowIndex = Ticket.Rows.Count - 1;
		if (rowIndex < 0)
		{
			return false;
		}
		int count = Ticket.Columns.Count;
		int i;
		for (i = 0; i < count; i++)
		{
			TicketMerge ticketMerge = Ticket.Merges.FirstOrDefault((TicketMerge m) => m.Contains(rowIndex, i));
			int row = rowIndex;
			int col = i;
			if (ticketMerge != null)
			{
				row = ticketMerge.TopRow;
				col = ticketMerge.LeftColumn;
			}
			TicketCell cell = Ticket.GetCell(row, col);
			if (cell.Bottom.Width > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFirstRowExistTopBorder()
	{
		if (Ticket.Rows.Count == 0)
		{
			return false;
		}
		int count = Ticket.Columns.Count;
		int i;
		for (i = 0; i < count; i++)
		{
			TicketMerge ticketMerge = Ticket.Merges.FirstOrDefault((TicketMerge m) => m.Contains(0, i));
			int row = 0;
			int col = i;
			if (ticketMerge != null)
			{
				row = ticketMerge.TopRow;
				col = ticketMerge.LeftColumn;
			}
			TicketCell cell = Ticket.GetCell(row, col);
			if (cell.Top.Width > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetGridFixedRowsHeight()
	{
		int num = 0;
		for (int num2 = _grid.Rows.Fixed - 1; num2 >= 0; num2--)
		{
			num += _grid.Rows[num2].HeightDisplay;
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

	public Size GetEditorPanelSize()
	{
		return _editorPanel.Size;
	}

	public static string TicketNavToString(List<Auditai.Model.Column> nav)
	{
		return string.Join("→", nav.Select((Auditai.Model.Column c) => c.GetUniqueFormulaName()));
	}

	private int GetMixTicketDynamicDataRowRangeLastDataRowNextRowIndex(int vmDynamicDataRowIndex)
	{
		TicketInputRowVM row = _vm.GetRow(vmDynamicDataRowIndex);
		if (!row.IsMixTicketDynamicDataRow)
		{
			return -1;
		}
		int mixRangeDynamicDataRowTemplateId = row.TicketRow.MixRangeDynamicDataRowTemplateId;
		int result = vmDynamicDataRowIndex + 1;
		int rowsCount = _vm.GetRowsCount();
		for (int i = vmDynamicDataRowIndex + 1; i < rowsCount; i++)
		{
			TicketInputRowVM row2 = _vm.GetRow(i);
			if (!row2.IsMixTicketDynamicDataRow)
			{
				result = i;
				break;
			}
			if (row2.TicketRow.MixRangeDynamicDataRowTemplateId != mixRangeDynamicDataRowTemplateId)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private int GetMixTicketDynamicDataRowRangeRowsCount(int vmDynamicDataRowIndex)
	{
		TicketInputRowVM row = _vm.GetRow(vmDynamicDataRowIndex);
		if (!row.IsMixTicketDynamicDataRow)
		{
			return 0;
		}
		int num = 1;
		int mixRangeDynamicDataRowTemplateId = row.TicketRow.MixRangeDynamicDataRowTemplateId;
		for (int num2 = vmDynamicDataRowIndex - 1; num2 >= 0; num2--)
		{
			TicketInputRowVM row2 = _vm.GetRow(num2);
			if (!row2.IsMixTicketDynamicDataRow || row2.TicketRow.MixRangeDynamicDataRowTemplateId != mixRangeDynamicDataRowTemplateId)
			{
				break;
			}
			num++;
		}
		int rowsCount = _vm.GetRowsCount();
		for (int i = vmDynamicDataRowIndex + 1; i < rowsCount; i++)
		{
			TicketInputRowVM row3 = _vm.GetRow(i);
			if (!row3.IsMixTicketDynamicDataRow || row3.TicketRow.MixRangeDynamicDataRowTemplateId != mixRangeDynamicDataRowTemplateId)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private bool IsVMRowBeDynamicDataRow_MixTicket(int vmRowIndex)
	{
		if (vmRowIndex < 0 || vmRowIndex >= _vm.GetRowsCount())
		{
			return false;
		}
		return _vm.GetRow(vmRowIndex).IsMixTicketDynamicDataRow;
	}

	private void ShowRecordButtons()
	{
		if (_isTicketLocked)
		{
			_btnInsertRow.Visible = false;
			_btnRemoveRow.Visible = false;
			return;
		}
		if (Ticket.Kind != TicketKind.DynamicRow && Ticket.Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			_btnInsertRow.Visible = false;
			_btnRemoveRow.Visible = false;
			return;
		}
		bool visible = false;
		bool visible2 = false;
		if (_vm != null && !HasFillingFormula)
		{
			if (Ticket.Kind == TicketKind.DynamicRow)
			{
				int topRow = _grid.Selection.TopRow;
				int num = ConvertGridRowIndexToVMRowIndex(topRow);
				if (_grid.IsRowIndexOutOfRange(topRow))
				{
					_btnInsertRow.Visible = false;
					_btnRemoveRow.Visible = false;
					return;
				}
				if (!Table.IsLocked && ((_vm.IsRecordDataRow_DynamicRowTicket(num) && _grid.Rows[topRow].Visible) || _vm.IsRecordDataRow_DynamicRowTicket(num - 1)))
				{
					Rectangle cellRect = _grid.GetCellRect(topRow, 0);
					bool flag = true;
					if (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Vertical))
					{
						Rectangle cellRectUnclipped = _grid.GetCellRectUnclipped(topRow, 0);
						int gridFixedRowsHeight = GetGridFixedRowsHeight();
						int num2 = _grid.Height - (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Horizontal) ? SystemInformation.HorizontalScrollBarHeight : 0);
						if (cellRectUnclipped.Y + cellRectUnclipped.Height <= gridFixedRowsHeight || cellRectUnclipped.Y >= num2)
						{
							flag = false;
						}
					}
					if (flag)
					{
						int num3 = cellRect.Top + _grid.Top;
						num3 += (cellRect.Height - _btnInsertRow.Height) / 2;
						_btnInsertRow.Location = new Point(_grid.Left - _btnInsertRow.Width - 2, num3);
						visible = true;
						_btnRemoveRow.Location = new Point(_grid.Right + GetGridFixedColumnWidth() / 2, num3);
						visible2 = _vm.IsRecordDataRow_DynamicRowTicket(num);
					}
				}
			}
			else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
			{
				int topRow2 = _grid.Selection.TopRow;
				int num4 = ConvertGridRowIndexToVMRowIndex(topRow2);
				if (_grid.IsRowIndexOutOfRange(topRow2))
				{
					_btnInsertRow.Visible = false;
					_btnRemoveRow.Visible = false;
					return;
				}
				if (!Table.IsLocked && ((_vm.GetRow(num4).TicketRow.IsMixRangeDynamicDataRow && _grid.Rows[topRow2].Visible) || IsVMRowBeDynamicDataRow_MixTicket(num4 - 1)))
				{
					Rectangle cellRect2 = _grid.GetCellRect(topRow2, 0);
					bool flag2 = true;
					if (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Vertical))
					{
						Rectangle cellRectUnclipped2 = _grid.GetCellRectUnclipped(topRow2, 0);
						int gridFixedRowsHeight2 = GetGridFixedRowsHeight();
						int num5 = _grid.Height - (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Horizontal) ? SystemInformation.HorizontalScrollBarHeight : 0);
						if (cellRectUnclipped2.Y + cellRectUnclipped2.Height <= gridFixedRowsHeight2 || cellRectUnclipped2.Y >= num5)
						{
							flag2 = false;
						}
					}
					if (flag2)
					{
						int num6 = cellRect2.Top + _grid.Top;
						num6 += (cellRect2.Height - _btnInsertRow.Height) / 2;
						_btnInsertRow.Location = new Point(_grid.Left - _btnInsertRow.Width - 2, num6);
						visible = true;
						_btnRemoveRow.Location = new Point(_grid.Right + GetGridFixedColumnWidth() / 2, num6);
						visible2 = _vm.GetRow(num4).TicketRow.IsMixRangeDynamicDataRow;
					}
				}
			}
		}
		_btnInsertRow.Visible = visible;
		_btnRemoveRow.Visible = visible2;
	}

	private async Task ExportTableAttachmentsToFolder(TicketInputTableVM vm, string outputDirName)
	{
		if (vm != null && vm.Table != null && vm.Table.CellPropManager.DicCellAttachments.Count != 0)
		{
			List<CellAttachment> allAttachments = vm.GetAllAttachments();
			if (allAttachments.Count != 0)
			{
				await ExportAttachmentsToFolder(allAttachments, outputDirName);
			}
		}
	}

	private async Task ExportAttachmentsToFolder(List<CellAttachment> attachmentList, string outputDirName)
	{
		ProgressForm2 progressForm = new ProgressForm2(new ProgressDisplayValueConverter_SmoothByTime(0.1f));
		ProgressRuntimeData progressRuntimeData = new ProgressRuntimeData();
		progressRuntimeData.NextStep("准备开始导出附件...");
		List<string> exportFailedAttachmentList = new List<string>();
		progressForm.ShowDialog(progressRuntimeData, async delegate
		{
			await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
			string defaultFileName = string.Empty;
			for (int i = 0; i < attachmentList.Count; i++)
			{
				try
				{
					CellAttachment cellAttachment = attachmentList[i];
					Guid fileId = cellAttachment.FileId;
					defaultFileName = ((cellAttachment.Name == null) ? string.Empty : cellAttachment.Name);
					progressRuntimeData.UpdateMessage("正在导出附件: " + defaultFileName);
					progressRuntimeData.UpdateProgress(i + 1, attachmentList.Count);
					await _vm.Table.Project.FileCacheManager.DownloadIfNotExist(fileId);
					char[] invalidPathChars = Path.GetInvalidPathChars();
					foreach (char oldChar in invalidPathChars)
					{
						defaultFileName = defaultFileName.Replace(oldChar, '-');
					}
					string extension = Path.GetExtension(defaultFileName);
					defaultFileName = Path.GetFileNameWithoutExtension(defaultFileName);
					string text2 = Path.Combine(outputDirName, defaultFileName) + extension;
					int num = 0;
					while (File.Exists(text2))
					{
						num++;
						text2 = $"{Path.Combine(outputDirName, defaultFileName)}({num}){extension}";
						if (num == int.MaxValue)
						{
							break;
						}
					}
					_vm.Table.Project.FileCacheManager.DuplicateTo(fileId, text2);
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

	private async Task ExportAllAttachment()
	{
		try
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "请选择保存附件的文件夹位置：";
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				await ExportTableAttachmentsToFolder(_vm, folderBrowserDialog.SelectedPath);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Hand, "附件导出失败:\r\n" + ex.ToString(), MessageBoxButtons.OK, "错误!");
		}
	}

	private async Task AddAttachment()
	{
		int vmRowIndex = ConvertGridRowIndexToVMRowIndex(_grid.Selection.TopRow);
		int vmColIndex = ConvertGridColIndexToVMColIndex(_grid.Selection.LeftCol);
		TicketInputCellVM cell = _vm.GetCellVM(vmRowIndex, vmColIndex);
		if (!CanEditCell(cell, editAttachments: true) || cell.IsFormula || cell.IsDynamicRowKeyCell)
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
			_vm.AddAttachment(vmRowIndex, vmColIndex, fileId, ofd.SafeFileName);
			_vm.UpdateTicketCellValue(cell, "", isFormulaExistManualInputValue: true);
			cell.IsAttachmentsDirty = true;
			_isDirty = true;
			_grid.Invalidate();
			SetCommandState();
			ShowTooltip();
		}
	}

	private void ShowTooltip()
	{
		HideTooltip();
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (_owner.CurrentView != MainFormView.TicketInput || (!selection.IsSingleCell && !_grid.MergedRanges.Contains(selection)))
		{
			return;
		}
		int num = ConvertGridRowIndexToVMRowIndex(selection.TopRow);
		int num2 = ConvertGridColIndexToVMColIndex(selection.LeftCol);
		if (num < 0 || num2 < 0 || num >= _vm.GetRowsCount() || num2 >= _vm.GetColumnsCount())
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
		TicketInputCellVM cellVM = _vm.GetCellVM(num, num2);
		bool flag = false;
		bool flag2;
		if (Program.MainForm.TableValidationResults.TryGetValue(Table.TreeNode, out var value) && cellVM.IsTableExistCell)
		{
			Auditai.Model.Cell tableCell = cellVM.TempCell;
			int cellRowIndex = tableCell.Row.Index;
			int cellColIndex = tableCell.Column.Index;
			IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> enumerable = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.Cells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1.Id == tableCell.Id);
			IEnumerable<Tuple<RangeOperand, ValidationResult>> enumerable2 = ((IEnumerable<Tuple<RangeOperand, ValidationResult>>)value.Ranges).Where((Tuple<RangeOperand, ValidationResult> t) => t.Item1.TopLeft.Row.Index <= cellRowIndex && t.Item1.TopLeft.Column.Index <= cellColIndex && cellRowIndex <= t.Item1.BottomRight.Row.Index && cellColIndex <= t.Item1.BottomRight.Column.Index);
			IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>> enumerable3 = ((IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>>)value.Columns).Where((Tuple<Auditai.Model.Column, ValidationResult> t) => t.Item1.Index == cellColIndex);
			IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> enumerable4 = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.HeaderCells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1.Column.Index == cellColIndex && t.Item1.Row.Index < cellRowIndex && t.Item1.GetHeaderLastRow() >= cellRowIndex);
			flag2 = enumerable.Any() || enumerable2.Any() || enumerable3.Any() || enumerable4.Any();
			if (flag2)
			{
				FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(_owner.FormulaEditor.Context.Project);
				xBody.Add(new XElement("b", "校验公式"));
				try
				{
					int i = 0;
					foreach (Tuple<Auditai.Model.Cell, ValidationResult> item in enumerable)
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
					foreach (Tuple<Auditai.Model.Column, ValidationResult> item3 in enumerable3)
					{
						AddValidationResult(item3.Item2, "l");
						i++;
					}
					i = 0;
					foreach (Tuple<Auditai.Model.Cell, ValidationResult> item4 in enumerable4)
					{
						AddValidationResult(item4.Item2, "h");
						i++;
					}
					void AddValidationResult(ValidationResult vf, string anchorPrefix)
					{
						xBody.Add(new XElement("p", "公式说明：", vf.Source.Note));
						XElement xElement4 = new XElement("p", "校验等式：");
						FormulaEvaluator formulaEvaluator = new FormulaEvaluator(vf.Source.LeftExpr);
						Tuple<List<TooltipListener.FormulaTooltipSegment>, string> formulaTooltipSegments = formulaEvaluator.GetFormulaTooltipSegments(resolver, null, vf);
						foreach (TooltipListener.FormulaTooltipSegment item5 in formulaTooltipSegments.Item1)
						{
							xElement4.Add(item5.PreText);
							string text = $"l{anchorPrefix}{i}{item5.AnchorNumber}";
							xElement4.Add(new XElement("a", new XAttribute("href", text), item5.Display));
							linkDic.Add(text, item5.Ref);
						}
						xElement4.Add(formulaTooltipSegments.Item2);
						xElement4.Add(vf.Source.Operator.Display);
						formulaEvaluator = new FormulaEvaluator(vf.Source.RightExpr);
						formulaTooltipSegments = formulaEvaluator.GetFormulaTooltipSegments(resolver, null, vf);
						foreach (TooltipListener.FormulaTooltipSegment item6 in formulaTooltipSegments.Item1)
						{
							xElement4.Add(item6.PreText);
							string text2 = $"r{anchorPrefix}{i}{item6.AnchorNumber}";
							xElement4.Add(new XElement("a", new XAttribute("href", text2), item6.Display));
							linkDic.Add(text2, item6.Ref);
						}
						xElement4.Add(formulaTooltipSegments.Item2);
						xBody.Add(xElement4);
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
				if (!Table.IsLocked && CanEditCell(cellVM, editAttachments: true))
				{
					xElement.Add(new XElement("a", "删除附件", new XAttribute("href", $"removeAttachment{j}")), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XElement("a", "重命名附件", new XAttribute("href", $"renameAttachment{j}")));
					linkDic.Add($"removeAttachment{j}", j);
					linkDic.Add($"renameAttachment{j}", j);
				}
			}
			XElement xElement2 = new XElement("p");
			xBody.Add(xElement2);
			if (!Table.IsLocked && CanEditCell(cellVM, editAttachments: true))
			{
				xElement2.Add(new XElement("a", new XAttribute("href", "addAttachment"), "插入附件"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"));
				linkDic.Add("addAttachment", null);
			}
			xElement2.Add(new XElement("a", "导出本表单所有附件", new XAttribute("href", "exportAllAttachment")));
			linkDic.Add("exportAllAttachment", null);
			xBody.Add(new XElement("hr"));
		}
		if (flag || flag2 || flag3)
		{
			xBody.LastNode.Remove();
			XElement xElement3 = xBody.Element("b");
			xElement3.Remove();
			_ttpComment.SetText(xElement3.Value, xBody.ToString(), canClose: true);
			_ttpComment.SetTagDic(linkDic);
			_ttpComment.LinkClicked += _ttpComment_LinkClicked;
			_ttpComment.Show(_grid, new Point(cellRect.Right, cellRect.Top + cellRect.Height / 2));
		}
	}

	private bool CanEditCell(TicketInputCellVM cvm, bool editAttachments = false)
	{
		try
		{
			if (cvm.Column == null)
			{
				return false;
			}
			if (_isTicketLocked)
			{
				return false;
			}
			if (IsMixTicketDataRowCell(cvm) && !IsMixTicketDataRowCellCanEdit(cvm))
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
			Auditai.Model.Cell tempCell = cvm.TempCell;
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

	private bool IsGridColumnCanEdit(Auditai.Model.Column column)
	{
		if (column == null)
		{
			return false;
		}
		if (!column.Permissions.Write.GrantAll && column.Permissions.CanWrite())
		{
			return true;
		}
		if (!Program.MainForm.TableEditor.CanEditColumn(column))
		{
			return false;
		}
		if (column.CrossAttributes.Role != 0)
		{
			return false;
		}
		return true;
	}

	private bool IsGridCellCanEditWithoutColumnCheck(TicketInputCellVM cvm)
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
		Auditai.Model.Cell tempCell = cvm.TempCell;
		if (!tempCell.IsEditable)
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
		return true;
	}

	private void SetThemeNavTreePanel()
	{
		Theme.SetCurrentObject(_switchViewPanel);
		Theme.SetCurrentObject(_otbNavs);
		SetSwitchViewPanelBackgroundBrush();
		C1Theme c1Theme = Theme.SelectedAuditaiTheme.GetC1Theme();
		_switchViewPanel.Height = c1Theme.GetInt("C1Command\\C1OutBar\\Page\\Title\\Height");
		_switchViewPanel.ForeColor = c1Theme.GetColor("C1Command\\C1OutBar\\Page\\Title\\Default\\ForeColor");
		_switchViewPanel.BorderColor = c1Theme.GetColor("C1Command\\C1OutBar\\BorderColor");
		_radioButtonOpenTableView.ForeColor = _switchViewPanel.ForeColor;
		_radioButtonOpenTicketView.ForeColor = _switchViewPanel.ForeColor;
		TableTicketViewModeThemeContext tableTicketViewModePanelContext = Theme.SelectedAuditaiTheme.ThemeContext.TableTicketViewModePanelContext;
		_radioButtonOpenTableView.HotBackgroundColor = tableTicketViewModePanelContext.HotBackColor;
		_radioButtonOpenTableView.CheckedBackgroundColor = tableTicketViewModePanelContext.CheckedBackColor;
		_radioButtonOpenTableView.CheckedForeColor = tableTicketViewModePanelContext.CheckedForeColor;
		_radioButtonOpenTableView.HotForeColor = tableTicketViewModePanelContext.HotForeColor;
		_radioButtonOpenTableView.PressedForeColor = tableTicketViewModePanelContext.HotForeColor;
		_radioButtonOpenTicketView.HotBackgroundColor = _radioButtonOpenTableView.HotBackgroundColor;
		_radioButtonOpenTicketView.CheckedBackgroundColor = _radioButtonOpenTableView.CheckedBackgroundColor;
		_radioButtonOpenTicketView.CheckedForeColor = _radioButtonOpenTableView.CheckedForeColor;
		_radioButtonOpenTicketView.HotForeColor = _radioButtonOpenTableView.HotForeColor;
		_radioButtonOpenTicketView.PressedForeColor = _radioButtonOpenTableView.PressedForeColor;
	}

	private void SetSwitchViewPanelBackgroundBrush()
	{
		if (_switchViewPanelBackgroundBrush != null)
		{
			_switchViewPanelBackgroundBrush.Dispose();
			_switchViewPanelBackgroundBrush = null;
		}
		ThemeBackground background = Theme.SelectedAuditaiTheme.GetBackground("C1Command\\C1OutBar\\Page\\Title\\Default\\Background");
		_switchViewPanelBackgroundBrush = background.GetBrush().GetBrush(_switchViewPanel.ClientRectangle);
	}

	private void SetThemeNavs()
	{
		SetThemeNavTreePanel();
		foreach (TicketNavGrid navGrid in _navGrids)
		{
			navGrid.SetTheme();
		}
		if (_freePageList.Count > 0)
		{
			List<OutPageData> list = new List<OutPageData>();
			while (_freePageList.Count > 0)
			{
				OutPageData outPageData = _freePageList.Dequeue();
				list.Add(outPageData);
				outPageData.navGrid.SetTheme();
			}
			foreach (OutPageData item in list)
			{
				_freePageList.Enqueue(item);
			}
		}
		_pageTitleBackgroundColor = Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1Command\\C1OutBar\\Page\\Title\\Hot\\Background");
	}

	private void SetCommandState()
	{
		_isDirty = Table.NeedSave || _vm.RemovedRows.Count > 0;
		_cmdAdd.Enabled = !Table.IsLocked && !HasFillingFormula;
		_cmdDelete.Enabled = !Table.IsLocked && !HasFillingFormula && !_isAdd;
		_cmdSave.Enabled = (_isAdd || _isDirty) && IsHasSaveDataPermission && !_isInShowingVirtualNode;
		_cmdCancelSave.Enabled = (_isAdd || _isDirty) && IsHasSaveDataPermission && !_isInShowingVirtualNode;
		_cmdCollectFill2.Enabled = !Table.IsLocked && _vm.IsExistLedgerCollectFormula;
		AppCommands.TicketDesign.Enabled = !Table.IsLocked;
		AppCommands.TicketColumnWidthIncrease.Enabled = !Table.IsLocked;
		AppCommands.TicketColumnWidthDecrease.Enabled = !Table.IsLocked;
		AppCommands.TicketRowHeightDecrease.Enabled = !Table.IsLocked;
		AppCommands.TicketRowHeightIncrease.Enabled = !Table.IsLocked;
		_cmdDesignTicket.Enabled = AppCommands.TicketDesign.Enabled;
		_cmdCalculateTable.Enabled = true;
		_cmdCheckTable.Enabled = true;
		if (!SoftwareLicenseManager.IsAllowAddTableRows(showDialog: false))
		{
			_cmdAdd.Enabled = false;
			_cmdCollectFill2.Enabled = false;
			if (_isInShowingVirtualNode)
			{
				_cmdDelete.Enabled = false;
				_cmdSave.Enabled = false;
				_cmdCancelSave.Enabled = false;
				_cmdCalculateTable.Enabled = false;
				_cmdCheckTable.Enabled = false;
			}
		}
		if (_isTicketLocked)
		{
			_cmdAdd.Enabled = false;
			_cmdDelete.Enabled = false;
			_cmdSave.Enabled = false;
			_cmdCancelSave.Enabled = false;
			_cmdCollectFill2.Enabled = false;
			_cmdCalculateTable.Enabled = false;
			_cmdCheckTable.Enabled = false;
		}
	}

	private bool IsPreventCutOrDeleteCellValue()
	{
		if (_isInShowingVirtualNode && !SoftwareLicenseManager.IsAllowAddTableRows())
		{
			return true;
		}
		return false;
	}

	private void ClearSelection()
	{
		if (Table.IsLocked || _isTicketLocked || IsPreventCutOrDeleteCellValue())
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		_vm.BeginBatchUpdateValue();
		BuildTableCellForSelectRange();
		bool flag = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			int num = i + _grid.Rows.Fixed;
			if (_grid.IsRowIndexOutOfRange(num) || !_grid.Rows[num].IsVisible)
			{
				continue;
			}
			for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
			{
				if (!_grid.Cols[ConvertVMColIndexToGridColIndex(j)].IsVisible)
				{
					continue;
				}
				TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(i, j);
				if (!CanEditCell(gridBodyCellVM))
				{
					continue;
				}
				bool isFormulaExistManualInputValue = false;
				if (gridBodyCellVM.IsFormula)
				{
					if (!gridBodyCellVM.IsAllowManualInputOnFormula || !flag)
					{
						continue;
					}
					isFormulaExistManualInputValue = true;
				}
				_isDirty = true;
				_vm.UpdateTicketCellValue(gridBodyCellVM, "", isFormulaExistManualInputValue);
			}
		}
		_vm.EndBatchUpdateValue();
		_vm.CalculateTicket();
		_grid.Invalidate();
		TitleEditor.Invalidate();
		FooterEditor.Invalidate();
		SetCommandState();
	}

	private void PopulateToolBar()
	{
		AppCommandTabs.TicketInput.RibbonTab.Text = Ticket.GetLevelString() + "编辑";
		AppCommandGroups.TicketManage.RibbonGroup.Text = "设计" + Ticket.GetLevelString();
		AppCommandGroups.TicketLock.RibbonGroup.Text = Ticket.GetLevelString() + "权限保护";
		AppCommands.RowOwnerExclusive.Enabled = CanSetRowOwnerExclusive();
		AppCommands.RowOwnerExclusive.IsPressed = Table.RowOwnerExclusive;
		AppCommands.RowOwnerLoad.Enabled = CanSetRowOwnerExclusive();
		AppCommands.RowOwnerLoad.IsPressed = Table.RowOwnerLoad;
		(AppCommands.TicketDesign.RibbonItem as RibbonButton).Text = "设计" + Ticket.GetLevelString();
		(AppCommands.TicketDesign.RibbonItem as RibbonButton).LargeImage = Resources.TicketMode;
		_cmdAdd.Text = "新增" + Ticket.GetLevelString();
		_cmdDelete.Text = "删除" + Ticket.GetLevelString();
		_cmdSave.Text = "保存" + Ticket.GetLevelString();
		_cmdPrevious.Text = "上一个" + Ticket.GetLevelString();
		_cmdNext.Text = "下一个" + Ticket.GetLevelString();
		_cmdCalculateTable.Text = "运算" + Ticket.GetLevelString();
		_cmdCollectFill.Visible = Ticket.Kind == TicketKind.DynamicRow;
		_lnkAdd.Delimiter = _cmdCollectFill.Visible || _cmdCollectFill2.Visible;
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

	public void RefreshTicketLockShowStatus()
	{
		AppCommands.RowOwnerExclusive.Enabled = CanSetRowOwnerExclusive();
		AppCommands.RowOwnerLoad.Enabled = CanSetRowOwnerExclusive();
		if (Table != null)
		{
			AppCommands.RowOwnerExclusive.IsPressed = Table.RowOwnerExclusive;
			AppCommands.RowOwnerLoad.IsPressed = Table.RowOwnerLoad;
		}
	}

	private async void _ttpComment_LinkClicked(object sender, object e)
	{
		if (_grid.BodyRow < 0 || _grid.BodyCol < 0)
		{
			return;
		}
		if (!(e is Auditai.Model.Column column))
		{
			if (!(e is Auditai.Model.Cell cell))
			{
				if (!(e is TreeNodeBase treeNodeBase))
				{
					if (e is CellsOperand cellsOperand)
					{
						if (cellsOperand.Table == Table)
						{
							SwitchToTableMode();
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
						TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(_grid.BodyRow, _grid.BodyCol);
						CellAttachment ca = gridBodyCellVM.Attachments.Attachments[index];
						await TableEditor.OpenAttachment(ca);
					}
					else if (text.StartsWith("exportAttachment"))
					{
						int index2 = (int)e;
						TicketInputCellVM gridBodyCellVM2 = GetGridBodyCellVM(_grid.BodyRow, _grid.BodyCol);
						CellAttachment attachment = gridBodyCellVM2.Attachments.Attachments[index2];
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
						int fileIndex = (int)e;
						TicketInputCellVM gridBodyCellVM3 = GetGridBodyCellVM(_grid.BodyRow, _grid.BodyCol);
						if (CanEditCell(gridBodyCellVM3, editAttachments: true))
						{
							int cellRowIndex = ConvertGridBodyRowIndexToVMRowIndex(_grid.BodyRow);
							int cellColIndex = ConvertGridBodyColIndexToVMColIndex(_grid.BodyCol);
							_vm.RemoveAttachment(cellRowIndex, cellColIndex, fileIndex);
							gridBodyCellVM3.IsAttachmentsDirty = true;
							_grid.Invalidate();
							_isDirty = true;
							ShowTooltip();
							SetCommandState();
						}
					}
					else if (text.StartsWith("renameAttachment"))
					{
						int num = (int)e;
						TicketInputCellVM gridBodyCellVM4 = GetGridBodyCellVM(_grid.BodyRow, _grid.BodyCol);
						if (CanEditCell(gridBodyCellVM4, editAttachments: true))
						{
							CellAttachment cellAttachment = gridBodyCellVM4.Attachments.Attachments[num];
							string text2 = InputForm.Text("重命名附件", "将附件‘" + cellAttachment.Name + "’重命名为：", cellAttachment.Name);
							if (!string.IsNullOrWhiteSpace(text2))
							{
								int cellRowIndex2 = ConvertGridBodyRowIndexToVMRowIndex(_grid.BodyRow);
								int cellColIndex2 = ConvertGridBodyColIndexToVMColIndex(_grid.BodyCol);
								_vm.RenameAttachment(cellRowIndex2, cellColIndex2, num, text2);
							}
							gridBodyCellVM4.IsAttachmentsDirty = true;
							_isDirty = true;
							ShowTooltip();
							SetCommandState();
						}
					}
					else if (text.StartsWith("addAttachment"))
					{
						await AddAttachment();
					}
					else if (text.StartsWith("exportAllAttachment"))
					{
						await ExportAllAttachment();
					}
				}
				else if (treeNodeBase != Table.TreeNode)
				{
					_owner.ProjectHierarchy.FindAndSelectNode(treeNodeBase);
				}
			}
			else
			{
				if (cell.Column.Table == Table)
				{
					SwitchToTableMode();
				}
				Program.MainForm.SetOpenModeToTableMode(cell.Column.Table.TreeNode);
				_owner.ProjectHierarchy.FindAndSelectNode(cell.Column.Table.TreeNode);
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
				SwitchToTableMode();
			}
			Program.MainForm.SetOpenModeToTableMode(column.Table.TreeNode);
			_owner.ProjectHierarchy.FindAndSelectNode(column.Table.TreeNode);
			if (Program.MainForm.TableEditor.Table == column.Table)
			{
				Program.MainForm.TableEditor.SelectColumn(column.Index);
			}
		}
	}

	private void _pnlMain_MouseClick(object sender, MouseEventArgs e)
	{
		_titleEditor.LeaveEdit();
		_footerEditor.LeaveEdit();
		if (e.Button == MouseButtons.Right && Table != null && !Table.IsLocked)
		{
			_grid.FilterManager.IsFilterOnGridColumnHeader = false;
			_ctxEmpty.ShowContextMenu(_editorPanel, e.Location);
		}
	}

	private void _pnlMain_Resize(object sender, EventArgs e)
	{
		if (_suspendPanelResizeEvent || Table == null || _vm == null || _vm.Table != Table || _editorPanel.Width == 0 || _editorPanel.Height == 0)
		{
			return;
		}
		SuspendDrawing();
		try
		{
			AutoAdjustInputGridPosition();
		}
		finally
		{
			ResumeDrawing();
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
		case Keys.Space:
			checkCellBox(_grid.Row, _grid.Col, null);
			break;
		case Keys.A | Keys.Control:
			_grid.BodySelect(0, 0, _grid.BodyRowsCount - 1, _grid.BodyColsCount - 1);
			break;
		case Keys.Up:
		{
			int suggestMoveToColIndex2;
			int gridUpVisibleRowIndex = GetGridUpVisibleRowIndex(_grid.Row, _grid.Col, out suggestMoveToColIndex2);
			if (gridUpVisibleRowIndex == -1)
			{
				if (_titleEditor.View.Rows.Count > 0)
				{
					e.SuppressKeyPress = true;
					_titleEditor.MoveFocusToBodyCell(_titleEditor.View.Rows.Count - 1, suggestMoveToColIndex2 - _grid.Cols.Fixed);
				}
			}
			else
			{
				e.SuppressKeyPress = true;
				int nearestVisibleColIndex2 = GetNearestVisibleColIndex(suggestMoveToColIndex2);
				_grid.SafeSelect(gridUpVisibleRowIndex, nearestVisibleColIndex2, gridUpVisibleRowIndex, nearestVisibleColIndex2);
			}
			break;
		}
		case Keys.Return:
		case Keys.Down:
		{
			int suggestMoveToColIndex;
			int gridDownVisibleRowIndex = GetGridDownVisibleRowIndex(_grid.Row, _grid.Col, out suggestMoveToColIndex);
			if (gridDownVisibleRowIndex == -1)
			{
				if (_footerEditor.View.Rows.Count > 0)
				{
					e.SuppressKeyPress = true;
					_footerEditor.MoveFocusToBodyCell(0, suggestMoveToColIndex - _grid.Cols.Fixed);
				}
			}
			else
			{
				e.SuppressKeyPress = true;
				int nearestVisibleColIndex = GetNearestVisibleColIndex(suggestMoveToColIndex);
				_grid.SafeSelect(gridDownVisibleRowIndex, nearestVisibleColIndex, gridDownVisibleRowIndex, nearestVisibleColIndex);
			}
			break;
		}
		}
	}

	private int GetGridUpVisibleRowIndex(int gridRowIndex, int girdColIndex, out int suggestMoveToColIndex)
	{
		suggestMoveToColIndex = girdColIndex;
		if (!_grid.IsIndexOutOfRange(gridRowIndex, girdColIndex))
		{
			int num = _grid.MergedRanges.IndexOf(gridRowIndex, girdColIndex);
			if (num != -1)
			{
				C1.Win.C1FlexGrid.CellRange cellRange = _grid.MergedRanges[num];
				gridRowIndex = cellRange.TopRow;
				suggestMoveToColIndex = cellRange.LeftCol;
			}
		}
		for (int num2 = gridRowIndex - 1; num2 >= _grid.Rows.Fixed; num2--)
		{
			if (_grid.Rows[num2].HeightDisplay != 0)
			{
				return num2;
			}
		}
		return -1;
	}

	private int GetGridDownVisibleRowIndex(int gridRowIndex, int gridColIndex, out int suggestMoveToColIndex)
	{
		suggestMoveToColIndex = gridColIndex;
		if (!_grid.IsIndexOutOfRange(gridRowIndex, gridColIndex))
		{
			int num = _grid.MergedRanges.IndexOf(gridRowIndex, gridColIndex);
			if (num != -1)
			{
				C1.Win.C1FlexGrid.CellRange cellRange = _grid.MergedRanges[num];
				gridRowIndex = cellRange.BottomRow;
				suggestMoveToColIndex = cellRange.LeftCol;
			}
		}
		for (int i = gridRowIndex + 1; i < _grid.Rows.Count; i++)
		{
			if (_grid.Rows[i].HeightDisplay != 0)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetNearestVisibleColIndex(int gridColIndex)
	{
		int count = _grid.Cols.Count;
		if (count == 0)
		{
			return -1;
		}
		if (gridColIndex < 0)
		{
			gridColIndex = _grid.Cols.Fixed;
		}
		else if (gridColIndex >= count)
		{
			gridColIndex = count - 1;
		}
		if (_grid.Cols[gridColIndex].WidthDisplay > 0)
		{
			return gridColIndex;
		}
		for (int i = gridColIndex + 1; i < _grid.Cols.Count; i++)
		{
			if (_grid.Cols[i].WidthDisplay != 0)
			{
				return i;
			}
		}
		for (int num = gridColIndex; num >= 0; num--)
		{
			if (_grid.Cols[num].WidthDisplay != 0)
			{
				return num;
			}
		}
		return -1;
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		try
		{
			if (e.Row < _grid.Rows.Fixed && e.Col >= _grid.Cols.Fixed)
			{
				PopuldateColumHeaderCellStyle(e.Row, e.Col);
			}
			else if (e.Col < _grid.Cols.Fixed)
			{
				PopuldateRowHeaderCellStyle(e.Row, e.Col);
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		void PopuldateColumHeaderCellStyle(int gridRowIndex, int gridColIndex)
		{
			try
			{
				int row4 = ConvertGridRowIndexToVMRowIndex(gridRowIndex);
				int col3 = ConvertGridColIndexToVMColIndex(gridColIndex);
				C1.Win.C1FlexGrid.CellStyle styleNew2 = _grid.GetCellRange(gridRowIndex, gridColIndex).StyleNew;
				TicketInputCellVM cellVM3 = _vm.GetCellVM(row4, col3);
				e.Text = cellVM3.GetDisplayValue();
				styleNew2.Font = cellVM3.Font;
				styleNew2.ForeColor = cellVM3.ForeColor;
				styleNew2.BackColor = ((cellVM3.BackColor == Color.White) ? UserSet.Config.TableStyle.LockAreaColor : cellVM3.BackColor);
				styleNew2.TextAlign = C1FlexGridEx.ToTextAlign(cellVM3.Align);
				styleNew2.Margins = new System.Drawing.Printing.Margins(cellVM3.Indent, 0, 0, 0);
				styleNew2.DataType = typeof(string);
				if (e.Style.Name == _grid.Styles.SelectedColumnHeader.Name)
				{
					styleNew2.BackColor = _grid.Styles.SelectedColumnHeader.BackColor;
				}
				e.Style = styleNew2;
				e.DrawCell(DrawCellFlags.Background | DrawCellFlags.Content);
				PaintCellBorder(e.Graphics, e.Row, e.Col);
				if (!_gridResizingManager.IsResizing && IsMouseOverHeaderColumn(gridColIndex) && !_grid.FilterManager.IsColumnInFilting(gridColIndex) && Table != null && !Table.IsLocked)
				{
					Rectangle colHeaderShowMoreMenuImageRectangle = GetColHeaderShowMoreMenuImageRectangle(gridColIndex);
					if (_isMouseOverShowMoreMenuImage)
					{
						Rectangle colHeaderShowMoreMenuImageShadowRectangle = GetColHeaderShowMoreMenuImageShadowRectangle(_mouseOverHeaderColumn);
						_brushStartEditingColHeaderBackground.Color = Auditai.UI.Controls.Util.DarkenColor(_grid.Styles.SelectedColumnHeader.BackColor, 0.1);
						e.Graphics.FillRectangle(_brushStartEditingColHeaderBackground, colHeaderShowMoreMenuImageShadowRectangle);
					}
					e.Graphics.DrawImage(Resources.menuMoreOperation, colHeaderShowMoreMenuImageRectangle.Location);
				}
				e.Handled = true;
			}
			catch (ArgumentOutOfRangeException exception3)
			{
				exception3.Log("渲染表单表格的列头时出现了未预期的异常");
			}
		}
		void PopuldateRowHeaderCellStyle(int gridRowIndex, int gridColIndex)
		{
			try
			{
				C1.Win.C1FlexGrid.CellStyle styleNew = _grid.GetCellRange(gridRowIndex, gridColIndex).StyleNew;
				if (gridRowIndex < _grid.Rows.Fixed && gridColIndex < _grid.Cols.Fixed)
				{
					styleNew.BackColor = _editorPanel.BackColor;
					e.Style = styleNew;
					e.DrawCell(DrawCellFlags.Background);
					PaintRowNumberCellBorder(e.Graphics, gridRowIndex, gridColIndex);
					e.Handled = true;
				}
				else
				{
					Color foreColor = Color.DarkGray;
					if (Table != null && _vm != null)
					{
						if (Ticket.Kind == TicketKind.DynamicRow)
						{
							int index = ConvertGridRowIndexToVMRowIndex(e.Row);
							TicketInputRowVM row2 = _vm.GetRow(index);
							if (_vm.IsRecordDataRow_DynamicRowTicket(index) && !row2.IsNew && row2.TableRow != null)
							{
								foreColor = _existTableDataRowNumberColor;
							}
						}
						else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
						{
							int index2 = ConvertGridRowIndexToVMRowIndex(e.Row);
							TicketInputRowVM row3 = _vm.GetRow(index2);
							if (row3.IsMixTicketDynamicDataRow && !row3.IsNew && row3.TableRow != null)
							{
								foreColor = _existTableDataRowNumberColor;
							}
						}
					}
					styleNew.TextAlign = TextAlignEnum.CenterCenter;
					styleNew.BackColor = _editorPanel.BackColor;
					styleNew.ForeColor = foreColor;
					e.Text = (gridRowIndex - _grid.Rows.Fixed + 1).ToString();
					styleNew.Font = _rowNumberFont;
					e.Style = styleNew;
					e.Image = SetRowWarningImage(gridRowIndex);
					e.Style.ImageAlign = ImageAlignEnum.CenterCenter;
					e.Style.Display = DisplayEnum.Overlay;
					e.DrawCell(DrawCellFlags.Background | DrawCellFlags.Content);
					PaintRowNumberCellBorder(e.Graphics, gridRowIndex, gridColIndex);
					e.Handled = true;
				}
			}
			catch (ArgumentOutOfRangeException exception2)
			{
				exception2.Log("渲染表单表格的行头时出现了未预期的异常");
			}
		}
		System.Drawing.Image SetRowWarningImage(int gridRowIndex)
		{
			if (Table.ControlWarningCells.Count == 0 && Table.ControlRemindCells.Count == 0)
			{
				return null;
			}
			int row = ConvertGridRowIndexToVMRowIndex(gridRowIndex);
			if (Table.ControlWarningCells.Count > 0)
			{
				for (int i = _grid.Cols.Fixed; i < _grid.Cols.Count; i++)
				{
					int col = ConvertGridColIndexToVMColIndex(i);
					TicketInputCellVM cellVM = _vm.GetCellVM(row, col);
					if (cellVM.TableCell != null && Table.ControlWarningCells.Contains(cellVM.TableCell))
					{
						if (_warningTextIsShown)
						{
							e.Text = string.Empty;
							return Resources.Warning16;
						}
						return null;
					}
				}
			}
			if (Table.ControlRemindCells.Count > 0)
			{
				for (int j = _grid.Cols.Fixed; j < _grid.Cols.Count; j++)
				{
					int col2 = ConvertGridColIndexToVMColIndex(j);
					TicketInputCellVM cellVM2 = _vm.GetCellVM(row, col2);
					if (cellVM2.TableCell != null && Table.ControlRemindCells.Contains(cellVM2.TableCell))
					{
						if (_warningTextIsShown)
						{
							e.Text = string.Empty;
							return Resources.Remind16;
						}
						return null;
					}
				}
			}
			return null;
		}
	}

	private TicketInputCellVM GetGridBodyCellVM(int bodyRowIndex, int bodyColIndex)
	{
		int row = ConvertGridBodyRowIndexToVMRowIndex(bodyRowIndex);
		int col = ConvertGridBodyColIndexToVMColIndex(bodyColIndex);
		return _vm.GetCellVM(row, col);
	}

	private Operand GetGridBodyComboList(int bodyRowIndex, int bodyColIndex, string comboList)
	{
		int vmRowIndex = ConvertGridBodyRowIndexToVMRowIndex(bodyRowIndex);
		int vmColIndex = ConvertGridBodyColIndexToVMColIndex(bodyColIndex);
		return _vm.GetComboList(vmRowIndex, vmColIndex, comboList);
	}

	private void _grid_BodyStartEdit(object sender, RowColEventArgs e)
	{
		bool flag = ChangeVirtualValueToRealValue();
		bool flag2 = flag;
		TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(e.Row, e.Col);
		_grid.BodySetData(e.Row, e.Col, gridBodyCellVM.Value);
		DataFormat format = gridBodyCellVM.Column.GetFormat();
		if (format.HasComboList)
		{
			if (gridBodyCellVM.TableRow == null)
			{
				_vm.StartRecordNewAddTableRows();
				int cellRowIndex = ConvertGridBodyRowIndexToVMRowIndex(e.Row);
				int cellColIndex = ConvertGridBodyColIndexToVMColIndex(e.Col);
				_vm.BuildTableCellForTicketCell(cellRowIndex, cellColIndex);
				_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
				flag2 = true;
			}
			Operand gridBodyComboList = GetGridBodyComboList(e.Row, e.Col, format.ComboList);
			if (_grid.Editor == ListDropDown.DropDown)
			{
				ListDropDown.DropDown.EditorDataType = typeof(string);
				ListDropDown.DropDown.EditorInitValue = null;
				Type dataType = format.GetDataType();
				if (dataType == typeof(DateTime) || dataType == typeof(DateYearMonth) || dataType == typeof(TimeSpan))
				{
					ListDropDown.DropDown.EditorInitValue = gridBodyCellVM.GetDisplayValue();
				}
				if (gridBodyComboList is TreeListOperand op)
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
				else if (gridBodyComboList is ValueSetOperand op2)
				{
					if (format.MultiComboList)
					{
						ListDropDown.ViewKind = DropDownViewKind.SimpleCheckList;
						ListDropDown.SimpleCheckedList.Op = op2;
						ListDropDown.SimpleCheckedList.Populate();
						ListDropDown.SimpleCheckedList.SetInitValue(gridBodyCellVM.GetDisplayValue());
					}
					else
					{
						ListDropDown.ViewKind = DropDownViewKind.SimpleList;
						ListDropDown.SimpleList.Op = op2;
						ListDropDown.SimpleList.Populate();
					}
				}
				else if (gridBodyComboList is TableListOperand op3)
				{
					if (format.MultiComboList)
					{
						ListDropDown.ViewKind = DropDownViewKind.TableCheckList;
						ListDropDown.TableCheckedList.Op = op3;
						ListDropDown.TableCheckedList.Populate();
						ListDropDown.TableCheckedList.SetInitValue(gridBodyCellVM.GetDisplayValue());
					}
					else
					{
						ListDropDown.ViewKind = DropDownViewKind.TableList;
						ListDropDown.TableList.Op = op3;
						ListDropDown.TableList.Populate();
					}
				}
				else if (gridBodyComboList is MultiListOperand op4)
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
					InputListDropDown.DropDown.EditorInitValue = gridBodyCellVM.GetDisplayValue();
				}
				InputListDropDown.Clear();
				InputListDropDown.CanInputTextbox = format.IgnoreComboList;
				if (gridBodyComboList is InputListOperand op5)
				{
					InputListDropDown.Op = op5;
					InputListDropDown.Populate();
					InputListDropDown.SetInitValue(gridBodyCellVM.GetDisplayValue());
				}
				else if (gridBodyComboList == ValueSetOperand.Empty)
				{
					InputListDropDown.PopulateError();
				}
			}
			if (flag2)
			{
				_editorPanel.BeginInvoke((Action)delegate
				{
					_grid.Invalidate();
					_titleEditor.View.Invalidate();
					_footerEditor.View.Invalidate();
					try
					{
						SetCommandState();
					}
					catch
					{
					}
				});
				flag2 = false;
			}
			else
			{
				_editorPanel.BeginInvoke((Action)delegate
				{
					_grid.Invalidate();
					_titleEditor.View.Invalidate();
					_footerEditor.View.Invalidate();
				});
			}
		}
		if (!flag2)
		{
			return;
		}
		_editorPanel.BeginInvoke((Action)delegate
		{
			try
			{
				SetCommandState();
			}
			catch
			{
			}
		});
	}

	public bool ChangeVirtualValueToRealValue()
	{
		if (!_isInShowingVirtualNode)
		{
			return false;
		}
		_isInShowingVirtualNode = false;
		if (_vm != null)
		{
			_vm.ChangeVirtualValueToRealValue();
		}
		return true;
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
		TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(e.Row, e.Col);
		Type dataType = gridBodyCellVM.Column.GetDataType();
		if (_grid.Editor == ListDropDown.DropDown)
		{
			ListDropDown.DropDown.DataType = typeof(string);
		}
		else if (dataType == typeof(DateTime) && string.IsNullOrEmpty(gridBodyCellVM.GetDisplayValue()))
		{
			_dateEdit.Value = DateTime.Now.Date;
		}
		else if (dataType == typeof(TimeSpan))
		{
			_timeEdit.DataType = typeof(object);
			_timeEdit.FormatType = FormatTypeEnum.CustomFormat;
			_timeEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
			DateTime result;
			if (string.IsNullOrEmpty(gridBodyCellVM.GetDisplayValue()))
			{
				_timeEdit.Value = DateTime.Now;
			}
			else if (gridBodyCellVM.Value is TimeSpan timeSpan)
			{
				_timeEdit.Value = new DateTime(2000, 1, 1, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			}
			else if (DateTime.TryParse(gridBodyCellVM.GetDisplayValue(), out result))
			{
				_timeEdit.Value = result;
			}
			else
			{
				_timeEdit.Value = DateTime.Now;
			}
			_timeEdit.DataType = typeof(DateTime);
			TableEditor.ConvertCellTimeDisplayFormatToTimeEditorFormat(gridBodyCellVM.Column.GetFormat().GetFormatString(), out var customFormat, out var customEditFormat);
			_timeEdit.CustomFormat = customFormat;
			_timeEdit.EditFormat.CustomFormat = customEditFormat;
		}
		else if (dataType == typeof(string))
		{
			DataFormat format = gridBodyCellVM.Column.GetFormat();
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
					textBox.Text = Regex.Replace(gridBodyCellVM.GetDisplayValue(), "(?<!\\r)\\n", "\r\n");
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
			string displayValue = gridBodyCellVM.GetDisplayValue();
			DateTime result2;
			DateTime dateTime = (string.IsNullOrWhiteSpace(displayValue) ? DateTime.Now.Date : ((!(gridBodyCellVM.Value is DateYearMonth dateYearMonth)) ? ((!DateTime.TryParse(displayValue, out result2)) ? DateTime.Now.Date : result2.Date) : dateYearMonth.Date));
			_dateEdit.Value = dateTime;
			_dateEdit.CustomFormat = gridBodyCellVM.Column.GetFormat().GetFormatString();
		}
		ListDropDown.SkipTextChanged = false;
		_vm.StartRecordNewAddTableRows();
		int cellRowIndex = ConvertGridBodyRowIndexToVMRowIndex(e.Row);
		int cellColIndex = ConvertGridBodyColIndexToVMColIndex(e.Col);
		_vm.BuildTableCellForTicketCell(cellRowIndex, cellColIndex);
		_vm.ExecuteNewAddedTableRowsWhichNotAbleAutoTriggerFormula();
	}

	private void _grid_BodyValidateEdit(object sender, ValidateEditEventArgs e)
	{
		if (_grid.IsIndexOutOfRange(e.Row, e.Col))
		{
			e.Cancel = true;
			return;
		}
		TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(e.Row, e.Col);
		Type dataType = gridBodyCellVM.Column.GetDataType();
		if (dataType == typeof(DateYearMonth) && _dateEdit.Value is DateTime)
		{
			DateTime date = (DateTime)_dateEdit.Value;
			_dateEdit.DataType = typeof(DateYearMonth);
			_dateEdit.Value = new DateYearMonth(date)
			{
				ToStringFormat = gridBodyCellVM.Column.GetFormat().GetFormatString()
			};
		}
		else if (dataType == typeof(TimeSpan) && _timeEdit.Value is DateTime dateTime)
		{
			_timeEdit.DataType = typeof(object);
			_timeEdit.Value = new TimeSpan(dateTime.Hour, dateTime.Minute, dateTime.Second);
			_timeEdit.DataType = typeof(TimeSpan);
		}
		DataFormat format = gridBodyCellVM.Column.GetFormat();
		if (format.HasComboList && _grid.Editor != InputListDropDown.DropDown && _grid.Editor == ListDropDown.DropDown && !format.IgnoreComboList && !ListDropDown.Validate())
		{
			_grid.FinishEditing(cancel: true);
		}
		ListDropDown.SkipTextChanged = true;
	}

	private void _grid_BodyAfterEdit(object sender, RowColEventArgs e)
	{
		if (_grid.IsIndexOutOfRange(e.Row, e.Col))
		{
			e.Cancel = true;
			return;
		}
		TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(e.Row, e.Col);
		object obj = _grid.BodyGetData(e.Row, e.Col);
		bool isFormulaExistManualInputValue = false;
		if (gridBodyCellVM.IsFormula)
		{
			isFormulaExistManualInputValue = true;
			if (!gridBodyCellVM.IsExistManualInputValue)
			{
				string displayValue = gridBodyCellVM.GetDisplayValue();
				string text = gridBodyCellVM.ConvertInputValueToDisplayValue(obj);
				if (text == displayValue)
				{
					isFormulaExistManualInputValue = false;
				}
			}
		}
		_vm.BeginBatchUpdateValue();
		_vm.UpdateTicketCellValue(gridBodyCellVM, obj, isFormulaExistManualInputValue);
		_vm.EndBatchUpdateValue();
		_vm.CalculateTicket();
		_grid.Invalidate();
		_titleEditor.View.Invalidate();
		_footerEditor.View.Invalidate();
		_isDirty = true;
		SetCommandState();
	}

	private void _grid_BodyBeforeEdit(object sender, RowColEventArgs e)
	{
		if (Program.MainForm.IsInSyncingProject)
		{
			e.Cancel = true;
			return;
		}
		if (IsTicketLocked)
		{
			e.Cancel = true;
			return;
		}
		try
		{
			if (Table.IsLocked)
			{
				e.Cancel = true;
				return;
			}
			TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(e.Row, e.Col);
			if (!CanEditCell(gridBodyCellVM))
			{
				e.Cancel = true;
				return;
			}
			int bodyMergeRowIndex = ConvertGridBodyRowIndexToVMRowIndex(e.Row);
			int bodyMergeColIndex = ConvertGridBodyColIndexToVMColIndex(e.Col);
			if (_vm.Merges.Any((TicketMerge m) => m.Contains(bodyMergeRowIndex, bodyMergeColIndex) && (m.TopRow != bodyMergeRowIndex || m.LeftColumn != bodyMergeColIndex)))
			{
				return;
			}
			if (gridBodyCellVM.IsField)
			{
				if (gridBodyCellVM.Column == null)
				{
					e.Cancel = true;
					return;
				}
				DataFormat format = gridBodyCellVM.Column.GetFormat();
				if (format.FormatType == DataFormatType.BoolCheckBox || format.FormatType == DataFormatType.BoolOnOff)
				{
					e.Cancel = true;
					return;
				}
				_grid.BodySetData(e.Row, e.Col, gridBodyCellVM.Value);
				C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
				Type dataType = gridBodyCellVM.Column.GetDataType();
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

	public void Invalidate()
	{
		SuspendDrawing();
		GridBeginUpdate();
		try
		{
			_grid.Invalidate();
			_titleEditor.Invalidate();
			_titleEditor.Invalidate();
		}
		finally
		{
			GridEndUpdate();
			ResumeDrawing();
		}
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

	private bool IsMixTicketDataRowCell(TicketInputCellVM cellVM)
	{
		if (!cellVM.IsMixTicketDynamicDataRow)
		{
			return cellVM.IsMixTicketFixedDataRow;
		}
		return true;
	}

	private bool IsMixTicketDataRowCellCanEdit(TicketInputCellVM cellVM)
	{
		if (cellVM.IsMixTicketExistDesignInputValue)
		{
			return false;
		}
		return true;
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
			C1.Win.C1FlexGrid.CellStyle styleNew = _grid.BodyGetCell(e.Row, e.Col).StyleNew;
			TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(e.Row, e.Col);
			e.Text = gridBodyCellVM.GetDisplayValue();
			styleNew.Font = gridBodyCellVM.Font;
			styleNew.ForeColor = gridBodyCellVM.ForeColor;
			styleNew.BackColor = ((gridBodyCellVM.BackColor == Color.White) ? UserSet.Config.TableStyle.LockAreaColor : gridBodyCellVM.BackColor);
			styleNew.TextAlign = C1FlexGridEx.ToTextAlign(gridBodyCellVM.Align);
			styleNew.Margins = new System.Drawing.Printing.Margins(gridBodyCellVM.Indent, 0, 0, 0);
			bool flag = gridBodyCellVM.IsField && !gridBodyCellVM.IsFixedMultiRowKey;
			if (IsMixTicketDataRowCell(gridBodyCellVM) && !IsMixTicketDataRowCellCanEdit(gridBodyCellVM))
			{
				flag = false;
			}
			if (flag)
			{
				styleNew.BackColor = (Color.Empty.Equals(gridBodyCellVM.BackColor) ? Color.White : gridBodyCellVM.BackColor);
				if (gridBodyCellVM.Column == null)
				{
					e.Text = "(无效列引用)";
				}
				else
				{
					Type dataType = gridBodyCellVM.Column.GetDataType();
					styleNew.DataType = ((dataType == typeof(bool)) ? null : dataType);
					DataFormat format = gridBodyCellVM.Column.GetFormat();
					if (format.FormatType == DataFormatType.BoolCheckBox)
					{
						e.Text = "";
						e.Image = (gridBodyCellVM.Value.Equals(true) ? _grid.Glyphs[GlyphEnum.Checked] : _grid.Glyphs[GlyphEnum.Unchecked]);
						styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(gridBodyCellVM.Align);
					}
					else if (format.FormatType == DataFormatType.BoolOnOff)
					{
						e.Text = "";
						e.Image = (gridBodyCellVM.Value.Equals(true) ? Resources.On : Resources.Off);
						styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(gridBodyCellVM.Align);
					}
					else if (gridBodyCellVM.Attachments != null)
					{
						e.Image = Resources.CellAttachment;
						e.Text = ((gridBodyCellVM.TempCell.DisplayAlign == CellTextAlign.MiddleCenter) ? "\n\n" : "") + $"({gridBodyCellVM.Attachments.Attachments.Count}个附件)";
						styleNew.ImageAlign = C1FlexGridEx.ToImageAlign(gridBodyCellVM.TempCell.DisplayAlign);
					}
					if (gridBodyCellVM.TempCell.DisplayLocked != 0L || !_vm.CanEditColumn(gridBodyCellVM.Column) || gridBodyCellVM.IsControlFormulaLocked || (gridBodyCellVM.TempCell.Row != null && Program.MainForm.TableEditor.Table != null && !Program.MainForm.TableEditor.CanEditRow(gridBodyCellVM.TempCell.Row)))
					{
						styleNew.BackColor = UserSet.Config.TableStyle.LockAreaColor;
					}
					else if (gridBodyCellVM.IsTableExistCell && gridBodyCellVM.TempCell != null && gridBodyCellVM.TempCell.HasFormula)
					{
						styleNew.BackColor = Auditai.UI.Controls.Util.DarkenColor(UserSet.Config.TableStyle.FormalaColor, 0.08);
					}
				}
			}
			bool flag2 = false;
			if (gridBodyCellVM.IsMixTicketFixedDataRow && gridBodyCellVM.IsMixTicketExistDesignInputValue)
			{
				styleNew.BackColor = UserSet.Config.TableStyle.LockAreaColor;
			}
			else if (gridBodyCellVM.IsFormula && !gridBodyCellVM.IsFixedMultiRowKey)
			{
				if (gridBodyCellVM.IsAllowManualInputOnFormula)
				{
					if (gridBodyCellVM.IsExistManualInputValue)
					{
						styleNew.BackColor = Color.White;
						flag2 = true;
					}
					else
					{
						styleNew.BackColor = UserSet.Config.TableStyle.AllowManualInputFormulaColor;
					}
				}
				else if (gridBodyCellVM.IsFormulaFromTicket)
				{
					styleNew.BackColor = Auditai.UI.Controls.Util.DarkenColor(UserSet.Config.TableStyle.FormalaColor, 0.08);
				}
				else if (gridBodyCellVM.IsTableExistCell && gridBodyCellVM.TempCell != null && gridBodyCellVM.TempCell.HasFormula)
				{
					styleNew.BackColor = Auditai.UI.Controls.Util.DarkenColor(UserSet.Config.TableStyle.FormalaColor, 0.08);
				}
				else
				{
					styleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
				}
			}
			if (gridBodyCellVM.IsTableExistCell && Program.MainForm.ShowHelperTooltip && Program.MainForm.TableValidationResults.TryGetValue(Table.TreeNode, out var value))
			{
				Auditai.Model.Cell tableCell = gridBodyCellVM.TempCell;
				int cellRowIndex = tableCell.Row.Index;
				int cellColIndex = tableCell.Column.Index;
				IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> source = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.Cells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1.Id == tableCell.Id);
				IEnumerable<Tuple<RangeOperand, ValidationResult>> source2 = ((IEnumerable<Tuple<RangeOperand, ValidationResult>>)value.Ranges).Where((Tuple<RangeOperand, ValidationResult> t) => t.Item1.TopLeft.Row.Index <= cellRowIndex && t.Item1.TopLeft.Column.Index <= cellColIndex && cellRowIndex <= t.Item1.BottomRight.Row.Index && cellColIndex <= t.Item1.BottomRight.Column.Index);
				IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>> source3 = ((IEnumerable<Tuple<Auditai.Model.Column, ValidationResult>>)value.Columns).Where((Tuple<Auditai.Model.Column, ValidationResult> t) => t.Item1.Index == cellColIndex);
				IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>> source4 = ((IEnumerable<Tuple<Auditai.Model.Cell, ValidationResult>>)value.HeaderCells).Where((Tuple<Auditai.Model.Cell, ValidationResult> t) => t.Item1.Column.Index == cellColIndex && t.Item1.Row.Index < cellRowIndex && t.Item1.GetHeaderLastRow() >= cellRowIndex);
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
			if (e.Style.Name == _grid.Styles.Highlight.Name)
			{
				styleNew.ForeColor = _grid.Styles.Highlight.ForeColor;
				styleNew.BackColor = _grid.Styles.Highlight.BackColor;
				if (gridBodyCellVM.IsShowVirtualValue)
				{
					e.Text = ((gridBodyCellVM.VirtualValue == null) ? string.Empty : gridBodyCellVM.VirtualValue);
				}
			}
			else
			{
				if (gridBodyCellVM.IsExistWarning)
				{
					styleNew.ForeColor = _warningTextColor;
					if (!_warningTextIsShown)
					{
						e.Text = string.Empty;
					}
				}
				else if (gridBodyCellVM.IsExistRemind)
				{
					styleNew.ForeColor = _remindTextColor;
					if (!_warningTextIsShown)
					{
						e.Text = string.Empty;
					}
				}
				if (_isTicketLocked || gridBodyCellVM.IsControlFormulaLocked)
				{
					styleNew.BackColor = UserSet.Config.TableStyle.LockAreaColor;
				}
				if (gridBodyCellVM.TableCell != null && gridBodyCellVM.TableCell.Row.Table.ControlForeColorCells.TryGetValue(gridBodyCellVM.TableCell, out var value2))
				{
					styleNew.ForeColor = value2;
				}
				if (gridBodyCellVM.TableCell != null && gridBodyCellVM.TableCell.Row.Table.ControlBackColorCells.TryGetValue(gridBodyCellVM.TableCell, out var value3))
				{
					styleNew.BackColor = value3;
				}
				if (gridBodyCellVM.IsShowVirtualValue)
				{
					e.Text = ((gridBodyCellVM.VirtualValue == null) ? string.Empty : gridBodyCellVM.VirtualValue);
					styleNew.ForeColor = TicketNavGrid.VirtualNodeTextColor;
				}
			}
			styleNew.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			e.Style = styleNew;
			int num = e.Row + _grid.Rows.Fixed;
			int num2 = e.Col + _grid.Cols.Fixed;
			if (flag2 && _grid.Selection.Contains(num, num2) && CanEditCell(gridBodyCellVM))
			{
				e.DrawCell(DrawCellFlags.All);
				PaintCancelManualInputIcon(num, num2);
				e.Handled = true;
			}
			else if (DrawUserHeaderIcon(gridBodyCellVM, num, num2, styleNew))
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
			System.Drawing.Image ticketCellUserHeaderIcon = _gridDecorator.GetTicketCellUserHeaderIcon(vmCell);
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
					_cancelManualInputBackgroundBrush.Color = Auditai.UI.Controls.Util.DarkenColor(_grid.Styles.Highlight.BackColor, 0.1);
					e.Graphics.FillRectangle(_cancelManualInputBackgroundBrush, rect);
				}
				e.Graphics.DrawImage(Resources.CancelManualInput, cancelManualInputIconArea.X, cancelManualInputIconArea.Y);
			}
		}
	}

	private void PaintRowNumberCellBorder(Graphics graphics, int gridRowIndex, int gridColIndex)
	{
		if (!_isMouseOverRowNumberColumn)
		{
			return;
		}
		int vmRowIndex = ConvertGridRowIndexToVMRowIndex(gridRowIndex);
		int vmColIndex = 0;
		TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(vmRowIndex, vmColIndex));
		TicketInputCellVM cellVM = _vm.GetCellVM(vmRowIndex, vmColIndex);
		Rectangle cellRect = _grid.GetCellRect(gridRowIndex, gridColIndex);
		cellRect.Offset(0, -1);
		TicketBorder cellTopBorder = _vm.GetCellTopBorder(cellVM, vmRowIndex, vmColIndex);
		TicketBorder ticketBorder = null;
		if (ticketMerge != null)
		{
			TicketInputCellVM cellVM2 = _vm.GetCellVM(ticketMerge.BottomRow, vmColIndex);
			ticketBorder = _vm.GetCellBottomBorder(cellVM2, vmRowIndex, vmColIndex);
			cellVM2 = _vm.GetCellVM(ticketMerge.TopRow, ticketMerge.RightColumn);
		}
		else
		{
			ticketBorder = _vm.GetCellBottomBorder(cellVM, vmRowIndex, vmColIndex);
		}
		if (cellTopBorder.Width > 0)
		{
			Rectangle rectangle = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
			if (gridRowIndex == 0)
			{
				rectangle.Y += cellTopBorder.Width;
				rectangle.Height -= cellTopBorder.Width;
			}
			using Pen pen = new Pen(_colorBorder, cellTopBorder.Width);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
		}
		if (ticketBorder.Width <= 0)
		{
			return;
		}
		Rectangle rectangle2 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
		using Pen pen2 = new Pen(_colorBorder, ticketBorder.Width);
		graphics.DrawLine(pen2, rectangle2.Left, rectangle2.Bottom, rectangle2.Right, rectangle2.Bottom);
	}

	private TicketMerge PaintCellBorder(Graphics graphics, int gridRowIndex, int gridColIndex)
	{
		int vmRowIndex = ConvertGridRowIndexToVMRowIndex(gridRowIndex);
		int vmColIndex = ConvertGridColIndexToVMColIndex(gridColIndex);
		TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(vmRowIndex, vmColIndex));
		TicketInputCellVM cellVM = _vm.GetCellVM(vmRowIndex, vmColIndex);
		Rectangle cellRect = _grid.GetCellRect(gridRowIndex, gridColIndex);
		cellRect.Offset(-1, -1);
		TicketBorder cellTopBorder = _vm.GetCellTopBorder(cellVM, vmRowIndex, vmColIndex);
		TicketBorder ticketBorder = null;
		TicketBorder right = cellVM.Right;
		if (ticketMerge != null)
		{
			TicketInputCellVM cellVM2 = _vm.GetCellVM(ticketMerge.BottomRow, vmColIndex);
			ticketBorder = _vm.GetCellBottomBorder(cellVM2, vmRowIndex, vmColIndex);
			cellVM2 = _vm.GetCellVM(ticketMerge.TopRow, ticketMerge.RightColumn);
			right = cellVM2.Right;
		}
		else
		{
			ticketBorder = _vm.GetCellBottomBorder(cellVM, vmRowIndex, vmColIndex);
		}
		if (cellTopBorder.Width > 0)
		{
			Rectangle rectangle = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
			if (gridRowIndex == 0)
			{
				rectangle.Y += cellTopBorder.Width;
				rectangle.Height -= cellTopBorder.Width;
			}
			if (vmColIndex == 0)
			{
				rectangle.X++;
				rectangle.Width--;
			}
			using Pen pen = new Pen(_colorBorder, cellTopBorder.Width);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
		}
		if (right.Width > 0)
		{
			Rectangle rectangle2 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
			int num = 1;
			rectangle2.Height += num;
			int width = right.Width;
			using Pen pen2 = new Pen(_colorBorder, width);
			graphics.DrawLine(pen2, rectangle2.Right, rectangle2.Top, rectangle2.Right, rectangle2.Bottom);
		}
		if (ticketBorder.Width > 0)
		{
			Rectangle rectangle3 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
			if (vmColIndex == 0)
			{
				rectangle3.X++;
				rectangle3.Width--;
			}
			using Pen pen3 = new Pen(_colorBorder, ticketBorder.Width);
			graphics.DrawLine(pen3, rectangle3.Left, rectangle3.Bottom, rectangle3.Right, rectangle3.Bottom);
		}
		if (cellVM.Left.Width > 0)
		{
			Rectangle rectangle4 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
			if (vmColIndex == 0)
			{
				int num2 = 1 + cellVM.Left.Width / 2;
				rectangle4.X += num2;
				rectangle4.Width -= num2;
			}
			using Pen pen4 = new Pen(_colorBorder, cellVM.Left.Width);
			graphics.DrawLine(pen4, rectangle4.Left, rectangle4.Top, rectangle4.Left, rectangle4.Bottom);
		}
		return ticketMerge;
	}

	private void _grid_BeforeScroll(object sender, RangeEventArgs e)
	{
		try
		{
			if (_grid.Editor != null)
				e.Cancel = true;
		}
		catch { }
	}

	private void _grid_AfterScroll(object sender, RangeEventArgs e)
	{
		if (!_titleEditor.IsInEditing && !_footerEditor.IsInEditing)
		{
			ShowTooltip();
			ShowRecordButtons();
		}
	}

	private void SendBodySelectionChanged_SignalR(int rowIndex, int colIndex)
	{
		_ = BodySelectionChanged_SignalR(rowIndex, colIndex);
		async Task BodySelectionChanged_SignalR(int bodyRowIndex, int bodyColIndex)
		{
			try
			{
				long num = 0L;
				TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(bodyRowIndex, bodyColIndex);
				if (gridBodyCellVM != null && gridBodyCellVM.IsTableExistCell)
				{
					Auditai.Model.Cell tableCell = gridBodyCellVM.TableCell;
					if (tableCell != null)
					{
						num = tableCell.Id.Value;
					}
				}
				await SignalRClient.UpLoadTableCellId(Auditai.Model.User.Current.Id.ToString(), num.ToString());
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
			List<double> list = new List<double>();
			for (int i = sel.TopRow; i <= sel.BottomRow; i++)
			{
				int num = ConvertGridBodyRowIndexToGridIndex(i);
				if (!_grid.IsRowIndexOutOfRange(num) && _grid.Rows[num].IsVisible)
				{
					for (int j = sel.LeftCol; j <= sel.RightCol; j++)
					{
						if (GetGridBodyCellVM(i, j).Value is double item)
						{
							list.Add(item);
						}
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
		void DoTooltip()
		{
			ShowTooltip();
		}
	}

	private int ConvertGridRowIndexToVMRowIndex(int gridRowIndex)
	{
		return gridRowIndex;
	}

	private int ConvertGridColIndexToVMColIndex(int gridColIndex)
	{
		return gridColIndex - _grid.Cols.Fixed;
	}

	private int ConvertVMRowIndexToGridRowIndex(int vmRowIndex)
	{
		return vmRowIndex;
	}

	private int ConvertVMColIndexToGridColIndex(int vmColIndex)
	{
		return vmColIndex + _grid.Cols.Fixed;
	}

	private int ConvertGridBodyRowIndexToVMRowIndex(int gridBodyRowIndex)
	{
		return gridBodyRowIndex + _grid.Rows.Fixed;
	}

	private int ConvertGridBodyColIndexToVMColIndex(int gridBodyColIndex)
	{
		return gridBodyColIndex;
	}

	private int ConvertGridBodyRowIndexToGridIndex(int gridBodyRowIndex)
	{
		return gridBodyRowIndex + _grid.Rows.Fixed;
	}

	private Rectangle GetColHeaderShowMoreMenuImageRectangle(int col)
	{
		if (_grid.Rows.Fixed <= 0)
		{
			return new Rectangle(-1000, -1000, Resources.menuMoreOperation.Width, Resources.menuMoreOperation.Height);
		}
		if (col < 0 || col >= _grid.Cols.Count)
		{
			return new Rectangle(-1000, -1000, Resources.menuMoreOperation.Width, Resources.menuMoreOperation.Height);
		}
		Rectangle cellRect = _grid.GetCellRect(_grid.Rows.Fixed - 1, col);
		int x = cellRect.Right - Resources.menuMoreOperation.Width - 8;
		int y = cellRect.Top + (cellRect.Height - Resources.menuMoreOperation.Height) / 2;
		return new Rectangle(new Point(x, y), Resources.menuMoreOperation.Size);
	}

	private Rectangle GetColHeaderShowMoreMenuImageShadowRectangle(int col)
	{
		int num = 2;
		int num2 = 2;
		Rectangle colHeaderShowMoreMenuImageRectangle = GetColHeaderShowMoreMenuImageRectangle(col);
		return new Rectangle(colHeaderShowMoreMenuImageRectangle.X - num, colHeaderShowMoreMenuImageRectangle.Y - num2, colHeaderShowMoreMenuImageRectangle.Width + num * 2, colHeaderShowMoreMenuImageRectangle.Height + num2 * 2);
	}

	private bool IsPointInColHeaderShowMoreMenuImageRect(Point p, int col)
	{
		if (IsMouseOverHeaderColumn(col))
		{
			return GetColHeaderShowMoreMenuImageShadowRectangle(col).Contains(p);
		}
		return false;
	}

	private bool IsMouseOverHeaderColumn(int gridColIndex)
	{
		if (_mouseOverHeaderColumn == gridColIndex)
		{
			return true;
		}
		if (_ticketGridColumnMerges != null && gridColIndex >= 0 && gridColIndex < _ticketGridColumnMerges.Length)
		{
			C1.Win.C1FlexGrid.CellRange cellRange = _ticketGridColumnMerges[gridColIndex];
			if (_mouseOverHeaderColumn >= cellRange.LeftCol && _mouseOverHeaderColumn <= cellRange.RightCol)
			{
				return true;
			}
		}
		return false;
	}

	private void BuildTicketGridColumnMergesData()
	{
		if ((_ticketGridColumnMerges != null && _ticketGridColumnMerges.Length == _grid.Cols.Count) || _grid.Rows.Fixed == 0)
		{
			return;
		}
		int count = _grid.Cols.Count;
		int row = _grid.Rows.Fixed - 1;
		_ticketGridColumnMerges = new C1.Win.C1FlexGrid.CellRange[count];
		for (int i = 0; i < count; i++)
		{
			int num = _grid.MergedRanges.IndexOf(row, i);
			if (num >= 0 && num <= _grid.MergedRanges.Count)
			{
				_ticketGridColumnMerges[i] = _grid.MergedRanges[num];
			}
			else
			{
				_ticketGridColumnMerges[i] = _grid.GetCellRange(row, i);
			}
		}
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		bool isMouseOverRowNumberColumn = _isMouseOverRowNumberColumn;
		bool isMouseOverCancelManualInputIcon = _isMouseOverCancelManualInputIcon;
		bool isMouseOverShowMoreMenuImage = _isMouseOverShowMoreMenuImage;
		int mouseOverHeaderColumn = _mouseOverHeaderColumn;
		_isMouseOverRowNumberColumn = false;
		_isMouseOverCancelManualInputIcon = false;
		_isMouseOverShowMoreMenuImage = false;
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader)
		{
			_mouseOverHeaderColumn = hitTestInfo.Column;
			BuildTicketGridColumnMergesData();
		}
		else
		{
			_mouseOverHeaderColumn = -1;
		}
		bool flag = false;
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Cursor cursor = TableEditor.CursorTable;
			int row = ConvertGridRowIndexToVMRowIndex(hitTestInfo.Row);
			int col = ConvertGridColIndexToVMColIndex(hitTestInfo.Column);
			TicketInputCellVM mergeTopLeftCellVM = _vm.GetMergeTopLeftCellVM(row, col);
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
			Cursor cursor2 = _grid.Cursor;
			_grid.Cursor = cursor;
			if (cursor2 != cursor)
			{
				flag = true;
			}
		}
		else if (hitTestInfo.Type == HitTestTypeEnum.None)
		{
			if (_grid.Cursor != Cursors.Arrow)
			{
				_grid.Cursor = Cursors.Arrow;
				flag = true;
			}
		}
		else if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader)
		{
			if (hitTestInfo.Column < _grid.Cols.Fixed)
			{
				_isMouseOverRowNumberColumn = true;
			}
			if (hitTestInfo.Row < _grid.Rows.Fixed && hitTestInfo.Column < _grid.Cols.Fixed)
			{
				if (_grid.Cursor != TableEditor.CursorTable)
				{
					_grid.Cursor = TableEditor.CursorTable;
					flag = true;
				}
			}
			else if (Table != null && !Table.IsLocked && IsPointInColHeaderShowMoreMenuImageRect(e.Location, _mouseOverHeaderColumn) && !_gridResizingManager.IsResizing)
			{
				_isMouseOverShowMoreMenuImage = true;
				_grid.Cursor = Cursors.Arrow;
			}
			else if (_grid.Cursor != TableEditor.CursorColumnHeader)
			{
				_grid.Cursor = TableEditor.CursorColumnHeader;
				flag = true;
			}
		}
		else if (hitTestInfo.Type == HitTestTypeEnum.RowHeader)
		{
			_isMouseOverRowNumberColumn = true;
			if (_grid.Cursor != TableEditor.CursorRowHeader)
			{
				_grid.Cursor = TableEditor.CursorRowHeader;
				flag = true;
			}
		}
		if (!flag)
		{
			if (mouseOverHeaderColumn != _mouseOverHeaderColumn)
			{
				flag = true;
			}
			else if (isMouseOverRowNumberColumn != _isMouseOverRowNumberColumn)
			{
				flag = true;
			}
			else if (isMouseOverCancelManualInputIcon != _isMouseOverCancelManualInputIcon)
			{
				flag = true;
			}
			else if (isMouseOverShowMoreMenuImage != _isMouseOverShowMoreMenuImage)
			{
				flag = true;
			}
		}
		if (flag)
		{
			_grid.Invalidate();
		}
	}

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		BeforeMouseDown_OnCellIconResponseClickEvent(e);
		if (e.Cancel)
		{
			return;
		}
		BeforeMouseDown_OnClickShowMoreMenuIcon(e);
		if (e.Cancel || Table.IsLocked || e.Button != MouseButtons.Left)
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
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.ColumnHeader && hitTestInfo.Column >= _grid.Cols.Fixed && hitTestInfo.Type == HitTestTypeEnum.ColumnHeader && !_grid.FilterManager.IsColumnInFilting(hitTestInfo.Column) && IsPointInColHeaderShowMoreMenuImageRect(new Point(e.X, e.Y), hitTestInfo.Column) && !_gridResizingManager.IsResizing)
		{
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			if (hitTestInfo.Column >= selection.LeftCol && hitTestInfo.Column <= selection.RightCol && IsGridEntireColumnSelected())
			{
				e.Cancel = true;
				_grid.FilterManager.IsFilterOnGridColumnHeader = true;
				_ctxCell.ShowContextMenu(_grid, new Point(e.X, e.Y));
			}
			else
			{
				_grid.FilterManager.IsFilterOnGridColumnHeader = true;
				_ctxCell.ShowContextMenu(_grid, new Point(e.X, e.Y));
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
				int row = ConvertGridRowIndexToVMRowIndex(i);
				int col = ConvertGridColIndexToVMColIndex(j);
				TicketInputCellVM cellVM = _vm.GetCellVM(row, col);
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
		_vm.BeginBatchUpdateValue();
		foreach (TicketInputCellVM item in hashSet)
		{
			_vm.UpdateTicketCellValue(item, string.Empty, isFormulaExistManualInputValue: false);
		}
		_vm.EndBatchUpdateValue();
		ReCalculateTable();
		_vm.CalculateTicket();
		_grid.Invalidate();
		_titleEditor.View.Invalidate();
		_footerEditor.View.Invalidate();
		_isDirty = true;
		SetCommandState();
	}

	private void checkCellBox(int gridRowIndex, int gridColIndex, BeforeMouseDownEventArgs e)
	{
		if (Table.IsLocked || _isTicketLocked)
		{
			return;
		}
		int row = ConvertGridRowIndexToVMRowIndex(gridRowIndex);
		int col = ConvertGridColIndexToVMColIndex(gridColIndex);
		TicketInputCellVM ticketInputCellVM = _vm.GetCellVM(row, col);
		if (ticketInputCellVM.Column == null)
		{
			TicketInputCellVM mergeTopLeftCellVM = _vm.GetMergeTopLeftCellVM(row, col);
			if (mergeTopLeftCellVM.Column != null)
			{
				ticketInputCellVM = mergeTopLeftCellVM;
			}
		}
		if (ticketInputCellVM.Column == null)
		{
			return;
		}
		bool flag = false;
		DataFormat format = ticketInputCellVM.Column.GetFormat();
		if (format.FormatType != DataFormatType.BoolCheckBox && format.FormatType != DataFormatType.BoolOnOff)
		{
			return;
		}
		if (e != null)
		{
			e.Cancel = true;
		}
		bool flag2 = !ticketInputCellVM.Value.Equals(true);
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		_vm.BeginBatchUpdateValue();
		if (selection.Contains(gridRowIndex, gridColIndex))
		{
			BuildTableCellForSelectRange();
			bool flag3 = IsSelectedRangeOnlyContainsFormulaArea(_grid.Selection);
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				for (int j = selection.LeftCol; j <= selection.RightCol; j++)
				{
					if (!_grid.Cols[j].IsVisible)
					{
						continue;
					}
					int row2 = ConvertGridRowIndexToVMRowIndex(i);
					int col2 = ConvertGridColIndexToVMColIndex(j);
					TicketInputCellVM cellVM = _vm.GetCellVM(row2, col2);
					if (cellVM.Column == null)
					{
						continue;
					}
					DataFormat format2 = cellVM.Column.GetFormat();
					if ((format2.FormatType != DataFormatType.BoolCheckBox && format2.FormatType != DataFormatType.BoolOnOff) || !CanEditCell(cellVM))
					{
						continue;
					}
					bool isFormulaExistManualInputValue = false;
					if (cellVM.IsFormula)
					{
						if (!cellVM.IsAllowManualInputOnFormula || !flag3)
						{
							continue;
						}
						isFormulaExistManualInputValue = true;
					}
					_vm.UpdateTicketCellValue(cellVM, flag2, isFormulaExistManualInputValue);
					flag = true;
				}
			}
		}
		else
		{
			BuildTableCellForTicketCell(gridRowIndex, gridColIndex);
			if (CanEditCell(ticketInputCellVM))
			{
				_vm.UpdateTicketCellValue(ticketInputCellVM, flag2, isFormulaExistManualInputValue: true);
				flag = true;
			}
		}
		_vm.EndBatchUpdateValue();
		if (flag || Table.NeedSave)
		{
			_vm.CalculateTicket();
			_grid.Invalidate();
			_titleEditor.View.Invalidate();
			_footerEditor.View.Invalidate();
			_isDirty = true;
			SetCommandState();
		}
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		try
		{
			Paint_Border();
		}
		catch (ArgumentOutOfRangeException exception)
		{
			exception.Log();
		}
		catch (Exception exception2)
		{
			exception2.Log();
		}
		void Paint_Border()
		{
			int rowsCount = _vm.GetRowsCount();
			int count = Ticket.Columns.Count;
			Rectangle clientRectangle = _grid.ClientRectangle;
			int num = Math.Abs(_grid.ScrollPosition.Y);
			int num2 = num + _grid.Size.Height;
			int num3 = 0;
			if (_grid.Rows.Frozen > 0)
			{
				for (int k = 0; k < _grid.Rows.Frozen; k++)
				{
					num += _grid.Rows[_grid.Rows.Fixed + k].HeightDisplay;
				}
			}
			for (int i = 0; i < rowsCount; i++)
			{
				if (num3 > num2)
				{
					break;
				}
				num3 += _grid.Rows[i].HeightDisplay;
				if (num3 >= num)
				{
					int j;
					for (j = 0; j < count; j++)
					{
						int row = ConvertVMRowIndexToGridRowIndex(i);
						int col = ConvertVMColIndexToGridColIndex(j);
						TicketMerge ticketMerge = _vm.Merges.FirstOrDefault((TicketMerge m) => m.Contains(i, j));
						TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
						Rectangle cellRect = _grid.GetCellRect(row, col);
						cellRect.Offset(-1, -1);
						TicketBorder cellTopBorder = _vm.GetCellTopBorder(cellVM, i, j);
						TicketBorder cellBottomBorder = _vm.GetCellBottomBorder(cellVM, i, j);
						if (cellTopBorder.Width > 0 && (ticketMerge == null || i == ticketMerge.TopRow))
						{
							Rectangle rectangle = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (i == 0)
							{
								rectangle.Y += cellTopBorder.Width;
								rectangle.Height -= cellTopBorder.Width;
							}
							if (j == 0)
							{
								rectangle.X++;
								rectangle.Width--;
							}
							using Pen pen = new Pen(_colorBorder, cellTopBorder.Width);
							e.Graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
						}
						if (cellVM.Right.Width > 0 && (ticketMerge == null || j == ticketMerge.RightColumn))
						{
							Rectangle rectangle2 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (i == rowsCount - 1)
							{
								int num4 = 1;
								rectangle2.Height += num4;
							}
							using Pen pen2 = new Pen(_colorBorder, cellVM.Right.Width);
							e.Graphics.DrawLine(pen2, rectangle2.Right, rectangle2.Top, rectangle2.Right, rectangle2.Bottom);
						}
						if (cellBottomBorder.Width > 0 && (ticketMerge == null || i == ticketMerge.BottomRow))
						{
							Rectangle rectangle3 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (j == 0)
							{
								rectangle3.X++;
								rectangle3.Width--;
							}
							using Pen pen3 = new Pen(_colorBorder, cellBottomBorder.Width);
							e.Graphics.DrawLine(pen3, rectangle3.Left, rectangle3.Bottom, rectangle3.Right, rectangle3.Bottom);
						}
						if (cellVM.Left.Width > 0 && (ticketMerge == null || j == ticketMerge.LeftColumn))
						{
							Rectangle rectangle4 = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
							if (j == 0)
							{
								int num5 = 1 + cellVM.Left.Width / 2;
								rectangle4.X += num5;
								rectangle4.Width -= num5;
							}
							using Pen pen4 = new Pen(_colorBorder, cellVM.Left.Width);
							e.Graphics.DrawLine(pen4, rectangle4.Left, rectangle4.Top, rectangle4.Left, rectangle4.Bottom);
						}
					}
				}
			}
		}
	}

	private bool IsCellRangeIntersect(C1.Win.C1FlexGrid.CellRange cellRange, int topRow, int bottomRow)
	{
		if (cellRange.r1 <= topRow)
		{
			if (cellRange.r2 >= topRow)
			{
				return true;
			}
			return false;
		}
		if (cellRange.r2 >= bottomRow)
		{
			if (cellRange.r1 <= bottomRow)
			{
				return true;
			}
		}
		else if (cellRange.r1 >= topRow && cellRange.r2 <= bottomRow)
		{
			return true;
		}
		return false;
	}

	private void _grid_BodyAfterRowColChange(object sender, RangeEventArgs e)
	{
		ShowRecordButtons();
		if (_grid.Rows.Frozen > 0 && IsCellRangeIntersect(e.OldRange, 0, _grid.Rows.Frozen - 1))
		{
			_grid.Invalidate();
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (Table == null)
		{
			return;
		}
		_grid.FilterManager.IsFilterOnGridColumnHeader = false;
		if (Table.IsLocked)
		{
			if (e.Button == MouseButtons.Right && _grid.HitTest().Type == HitTestTypeEnum.Cell)
			{
				_ctxLock.ShowContextMenu(_grid, e.Location);
			}
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (e.Button == MouseButtons.Right)
		{
			if (hitTestInfo.Type == HitTestTypeEnum.None)
			{
				_ctxEmpty.ShowContextMenu(_grid, e.Location);
			}
			else if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				_ctxCell.ShowContextMenu(_grid, e.Location);
			}
			else if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader)
			{
				_grid.FilterManager.IsFilterOnGridColumnHeader = true;
				_ctxCell.ShowContextMenu(_grid, e.Location);
			}
			else if (hitTestInfo.Type == HitTestTypeEnum.RowHeader)
			{
				_ctxRowHeader.ShowContextMenu(_grid, e.Location);
				AfterCtxRowHeaderPopUp();
			}
		}
		else
		{
			_ = e.Button;
			_ = 1048576;
		}
	}

	private void AfterCtxRowHeaderPopUp()
	{
		C1CommandLink c1CommandLink = null;
		foreach (object commandLink in _ctxRowHeader.CommandLinks)
		{
			if (commandLink is C1CommandLink { Visible: not false } c1CommandLink2)
			{
				c1CommandLink = c1CommandLink2;
				break;
			}
		}
		if (c1CommandLink == _lnkRowHeaderSetRowHeight)
		{
			_lnkRowHeaderSetRowHeight.Delimiter = false;
		}
		else
		{
			_lnkRowHeaderSetRowHeight.Delimiter = true;
		}
	}

	private void _cmdHelp_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ShowHelpCenter();
	}

	private void _cmdNext_Click(object sender, ClickEventArgs e)
	{
		NextRecord();
	}

	private void _cmdPrevious_Click(object sender, ClickEventArgs e)
	{
		PreviousRecord();
	}

	private bool IsPreventSaveDueToNewTableRow()
	{
		if (SoftwareLicenseManager.IsAllowAddTableRows(showDialog: false))
		{
			return false;
		}
		if (_vm != null)
		{
			int rowsCount = _vm.GetRowsCount();
			int columnsCount = _vm.GetColumnsCount();
			for (int i = 0; i < rowsCount; i++)
			{
				for (int j = 0; j < columnsCount; j++)
				{
					TicketInputCellVM cellVM = _vm.GetCellVM(i, j);
					if (cellVM.TableCell != null && cellVM.TableCell.Row.Status == SyncStatus.New)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void _cmdSave_Click(object sender, ClickEventArgs e)
	{
		if (SoftwareLicenseManager.IsTableRowsCountOutOfLicenseLimit(Table, 0))
		{
			return;
		}
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SaveGridSelectRangeAndScrollPosition();
		SuspendDrawing();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = TitleEditor.View.Selection;
			C1.Win.C1FlexGrid.CellRange selection2 = FooterEditor.View.Selection;
			C1.Win.C1FlexGrid.CellRange selection3 = _grid.Selection;
			SaveRecord();
			try
			{
				if (FooterEditor.View.Visible && selection2.TopRow >= 0)
				{
					FooterEditor.View.SafeSelect(selection2.TopRow, selection2.LeftCol, selection2.BottomRow, selection2.RightCol);
				}
				if (TitleEditor.View.Visible && selection.TopRow >= 0)
				{
					TitleEditor.View.SafeSelect(selection.TopRow, selection.LeftCol, selection.BottomRow, selection.RightCol);
				}
				if (selection3.TopRow >= -1)
				{
					_grid.SafeSelect(selection3.TopRow, selection3.LeftCol, selection3.BottomRow, selection3.RightCol);
				}
			}
			catch (Exception)
			{
			}
		}
		finally
		{
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
		}
	}

	private void _cmdCancelSave_Click(object sender, ClickEventArgs e)
	{
		CancelSave();
	}

	private void _cmdDelete_Click(object sender, ClickEventArgs e)
	{
		DeleteRecord();
	}

	private void _cmdAdd_Click(object sender, ClickEventArgs e)
	{
		AddRecord();
	}

	private void SwitchToTableViewImpl()
	{
		try
		{
			Program.MainForm.MainPanel.SuspendDrawing();
			Program.MainForm.SetPreviousTicketTab();
			HideTooltip();
			SaveRecord();
			Program.MainForm.TableEditor.PopulateTable();
			Program.MainForm.SwitchToTableView();
		}
		finally
		{
			Program.MainForm.MainPanel.ResumeDrawing();
		}
	}

	private void _cmdQuitTicket_Click(object sender, ClickEventArgs e)
	{
		try
		{
			Program.MainForm.MainPanel.SuspendDrawing();
			Program.MainForm.SetPreviousTicketTab();
			HideTooltip();
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			Program.MainForm.TableEditor.PopulateTable();
			Program.MainForm.SwitchToTableView();
		}
		finally
		{
			Program.MainForm.MainPanel.ResumeDrawing();
		}
	}

	public void SwitchToTableMode()
	{
		SwitchToTableViewImpl();
	}

	private void _btnRemoveRow_Click(object sender, EventArgs e)
	{
		RemoveRecordRows();
	}

	private void _btnInsertRow_Click(object sender, EventArgs e)
	{
		InsertRecordRow();
	}

	private void _cmdDeleteNav_Click(object sender, ClickEventArgs e)
	{
		DeleteNav();
	}

	private void _cmdModifyNav_Click(object sender, ClickEventArgs e)
	{
		ModifyNav();
	}

	private void _cmdAddNav_Click(object sender, ClickEventArgs e)
	{
		AddNav();
	}

	private void _cmdAddNav_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Program.MainForm.CurrentView != MainFormView.TicketInput)
		{
			_cmdAddNav.Visible = false;
		}
		else
		{
			_cmdAddNav.Visible = true;
		}
	}

	private void _cmdDeleteNav_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Program.MainForm.CurrentView != MainFormView.TicketInput)
		{
			_cmdDeleteNav.Visible = false;
		}
		else
		{
			_cmdDeleteNav.Visible = Ticket.Navs.Count > 0;
		}
	}

	private void _cmdModifyNav_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (Program.MainForm.CurrentView != MainFormView.TicketInput)
		{
			_cmdModifyNav.Visible = false;
		}
		else
		{
			_cmdModifyNav.Visible = Ticket.Navs.Count > 0;
		}
	}

	private void _cmdExportAttachment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int bodyRow = _grid.BodyRow;
		int bodyCol = _grid.BodyCol;
		if (bodyRow < 0 || bodyCol < 0 || Table == null || _vm == null)
		{
			_cmdRemoveAttachment.Visible = false;
			_cmdExportAttachment.Visible = false;
			return;
		}
		if (Table.CellPropManager.DicCellAttachments.Count == 0)
		{
			_cmdRemoveAttachment.Visible = false;
			_cmdExportAttachment.Visible = false;
			return;
		}
		if (_grid.Selection.IsSingleCell)
		{
			_cmdRemoveAttachment.Visible = false;
			_cmdExportAttachment.Visible = false;
			return;
		}
		int num = 0;
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		bool flag = false;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			int num2 = ConvertGridRowIndexToVMRowIndex(i);
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				int num3 = ConvertGridColIndexToVMColIndex(j);
				if (!_vm.IsIndexOutOfRange(num2, num3))
				{
					if (num++ > 10000)
					{
						flag = true;
						break;
					}
					TicketInputCellVM cellVM = _vm.GetCellVM(num2, num3);
					if (cellVM.TableCell != null && Table.CellPropManager.TryGetAttachments(cellVM.TableCell, out var _))
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (flag && Table.IsLocked)
		{
			_cmdRemoveAttachment.Visible = false;
			_cmdExportAttachment.Visible = true;
		}
		else
		{
			_cmdRemoveAttachment.Visible = flag;
			_cmdExportAttachment.Visible = flag;
		}
	}

	private async void _cmdExportAttachment_Click(object sender, ClickEventArgs e)
	{
		if (Table == null || Table.CellPropManager.DicCellAttachments.Count == 0 || _vm == null)
		{
			return;
		}
		List<CellAttachment> list = new List<CellAttachment>();
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			int num = ConvertGridRowIndexToVMRowIndex(i);
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				int num2 = ConvertGridColIndexToVMColIndex(j);
				if (!_vm.IsIndexOutOfRange(num, num2))
				{
					TicketInputCellVM cellVM = _vm.GetCellVM(num, num2);
					if (cellVM.TableCell != null && Table.CellPropManager.TryGetAttachments(cellVM.TableCell, out var attachments))
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

	private void _cmdRemoveAttachment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void _cmdRemoveAttachment_Click(object sender, ClickEventArgs e)
	{
		if (Table == null || Table.CellPropManager.DicCellAttachments.Count == 0 || _vm == null || Table.IsLocked)
		{
			return;
		}
		bool flag = false;
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			int num = ConvertGridRowIndexToVMRowIndex(i);
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				int num2 = ConvertGridColIndexToVMColIndex(j);
				if (!_vm.IsIndexOutOfRange(num, num2))
				{
					TicketInputCellVM cellVM = _vm.GetCellVM(num, num2);
					if (cellVM.TableCell != null && CanEditCell(cellVM, editAttachments: true) && _vm.Table.CellPropManager.TryGetAttachments(cellVM.TableCell, out var _))
					{
						_vm.RemoveAllAttachment(num, num2);
						cellVM.IsAttachmentsDirty = true;
						flag = true;
						_isDirty = true;
					}
				}
			}
		}
		if (flag)
		{
			_grid.Invalidate();
			SetCommandState();
		}
	}

	private void _cmdAddAttachment_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			_cmdAddAttachment.Visible = false;
			return;
		}
		int bodyRow = _grid.BodyRow;
		int bodyCol = _grid.BodyCol;
		if (bodyRow < 0 || bodyCol < 0 || _vm == null || Table == null || Table.IsLocked)
		{
			_cmdAddAttachment.Visible = false;
			return;
		}
		TicketInputCellVM gridBodyCellVM = GetGridBodyCellVM(bodyRow, bodyCol);
		if (gridBodyCellVM.Column == null || !CanEditCell(gridBodyCellVM, editAttachments: true) || _grid.FilterManager.IsFilterOnGridColumnHeader)
		{
			_cmdAddAttachment.Visible = false;
		}
		else if (!_grid.Selection.IsSingleCell && !_grid.MergedRanges.Contains(_grid.Selection))
		{
			_cmdAddAttachment.Visible = false;
		}
		else
		{
			_cmdAddAttachment.Visible = true;
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

	private void _cmdRemoveDataRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
			return;
		}
		int topRow = _grid.Selection.TopRow;
		int index = ConvertGridRowIndexToVMRowIndex(topRow);
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			e.Visible = false;
		}
		else if (Ticket.Kind == TicketKind.DynamicRow)
		{
			e.Visible = _vm.IsRecordDataRow_DynamicRowTicket(index) && !HasFillingFormula;
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			e.Visible = _vm.GetRow(index).TicketRow.IsMixRangeDynamicDataRow && !HasFillingFormula;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void _cmdRemoveDataRow_Click(object sender, ClickEventArgs e)
	{
		RemoveRecordRows();
	}

	private void _cmdAppendDataRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
			return;
		}
		_lnkAppendDataRow.Delimiter = !_cmdInsertDataRow.Visible;
		if (Ticket.Kind == TicketKind.DynamicRow)
		{
			e.Visible = Ticket.HasDataRow() && !HasFillingFormula;
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			int topRow = _grid.Selection.TopRow;
			if (_grid.IsRowIndexOutOfRange(topRow))
			{
				e.Visible = false;
				return;
			}
			int index = ConvertGridRowIndexToVMRowIndex(topRow);
			e.Visible = _vm.GetRow(index).TicketRow.IsMixRangeDynamicDataRow && !HasFillingFormula;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void _cmdAppendDataRow_Click(object sender, ClickEventArgs e)
	{
		AppendRecordRows();
	}

	private void _cmdInsertDataRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (_isTicketLocked)
		{
			e.Visible = false;
			return;
		}
		int topRow = _grid.Selection.TopRow;
		if (_grid.IsRowIndexOutOfRange(topRow))
		{
			e.Visible = false;
		}
		else if (Ticket.Kind == TicketKind.DynamicRow)
		{
			int index = ConvertGridRowIndexToVMRowIndex(topRow);
			e.Visible = _vm.IsRecordDataRow_DynamicRowTicket(index) && !HasFillingFormula;
		}
		else if (Ticket.Kind == TicketKind.FixedDataRowMixDynamicDataRow)
		{
			int index2 = ConvertGridRowIndexToVMRowIndex(topRow);
			e.Visible = _vm.GetRow(index2).TicketRow.IsMixRangeDynamicDataRow && !HasFillingFormula;
		}
		else
		{
			e.Visible = false;
		}
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

	private void _cmdInsertDataRow_Click(object sender, ClickEventArgs e)
	{
		InsertRecordRows();
	}

	private void _cmdCheckTable_Click(object sender, ClickEventArgs e)
	{
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SaveGridSelectRangeAndScrollPosition();
		SuspendDrawing();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			Ticket.TicketReocrdRefreshCallback = OnTicketRecordChanged;
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			Program.MainForm.ValidateCurrentTable();
			CalculateTicketRecordValidationFailedCount();
			Populate(isOpenSelectedRecordOnLeave: false, isExandToCurrentSelecteRecord: false);
			_grid.Select(selection.TopRow, selection.LeftCol, selection.BottomRow, selection.RightCol, show: true);
		}
		finally
		{
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
		}
	}

	private void _cmdCalculateTable_Click(object sender, ClickEventArgs e)
	{
		_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = true;
		SaveGridSelectRangeAndScrollPosition();
		SuspendDrawing();
		try
		{
			if (_isInShowingVirtualNode)
			{
				_isAdd = true;
				_isDirty = true;
				ChangeVirtualValueToRealValue();
				_vm.BuildTableCellForAllTicketCell();
				_vm.InitCombolistForNewRecord();
				ReCalculateTable();
				_vm.CalculateTicket();
				SetCommandState();
				_grid.Invalidate();
				_titleEditor.View.Invalidate();
				_footerEditor.View.Invalidate();
				return;
			}
			if (_isAdd)
			{
				_isDirty = true;
				_vm.BuildTableCellForAllTicketCell();
				ReCalculateTable();
				_vm.CalculateTicket();
				SetCommandState();
				_grid.Invalidate();
				_titleEditor.View.Invalidate();
				_footerEditor.View.Invalidate();
				return;
			}
			C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
			_isBuildTableCellInModifyModeOnOpenCurrentRecord = true;
			_isForceReCalculateTableBeforeSaveRecord = true;
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			Populate(isOpenSelectedRecordOnLeave: false, isExandToCurrentSelecteRecord: false);
			if (Table.NeedSave)
			{
				SaveTable(isReCalculateTable: false);
			}
			try
			{
				int row = ((selection.TopRow >= 0) ? selection.TopRow : 0);
				int rowSel = ((selection.BottomRow >= _grid.Rows.Count) ? (_grid.Rows.Count - 1) : selection.BottomRow);
				int col = ((selection.LeftCol >= 0) ? selection.LeftCol : 0);
				int colSel = ((selection.RightCol >= _grid.Cols.Count) ? (_grid.Cols.Count - 1) : selection.RightCol);
				_grid.Select(row, col, rowSel, colSel, show: true);
			}
			catch (Exception)
			{
			}
		}
		finally
		{
			_isForceReCalculateTableBeforeSaveRecord = false;
			_isSuspendTicketSelectRangeAndScrollPositionCacheEvent = false;
			RestoreGridSelectRangeAndScrollPosition();
			ResumeDrawing();
		}
	}

	private void CmdAddTicket_Click(object sender, ClickEventArgs e)
	{
		AddRecord();
	}

	private void CmdRemoveTicket_Click(object sender, ClickEventArgs e)
	{
		DeleteRecord();
	}

	private void CmdSearchTicket_Click(object sender, ClickEventArgs e)
	{
		_lazySearchExcute.Excute();
	}

	private void _otbNavs_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (_skipOtbIndexChange)
		{
			return;
		}
		_otbNavs.BeginUpdate();
		GridBeginUpdate();
		try
		{
			SaveRecordFilterSetting(isSaveToPreSelectedNavGrid: true);
			_currentNavGrid.FindAndSelectRecord(Record);
			_currentRecord = _currentNavGrid.GetCurrentIndex();
			TicketNavTreeStatusDataCacher.SaveNavTreePageSelectedIndex(Table.Id, _otbNavs.SelectedIndex);
			ExpandNavTreeToCurrentRecord();
			RestoreCurrentNavTreeScrollPosition();
			bool flag = false;
			if (_grid.FilterManager.Filters.Count > 0)
			{
				_grid.FilterManager.Filters.Clear();
				flag = true;
			}
			if (RestoreRecordFilterSetting())
			{
				flag = false;
			}
			if (flag)
			{
				_grid.FilterManager.Execute();
			}
			_preSelectedNavGrid = _currentNavGrid;
			_gridDecorator.SetCellFlickerDirty();
			_gridDecorator.RefreshNavTreeNodeFlickImageWithoutRebuild();
			SendTicketNavTreeNodeChangeEventToOtherClient();
			_grid.Invalidate();
		}
		finally
		{
			GridEndUpdate();
			_otbNavs.EndUpdate();
		}
	}

	private void _otbNavs_Paint(object sender, PaintEventArgs e)
	{
		if (Program.MainForm.CurrentView == MainFormView.TicketInput && Table != null && Ticket.Navs.Count == 0 && SecondTrigger.Display)
		{
			e.Graphics.DrawString("点击鼠标右键新增导航树", _grid.Font, Brushes.Red, _otbNavs.ClientRectangle, _sf);
		}
	}

	private void _otbNavs_PageTitlePostPaint(C1OutPage page, PaintEventArgs e, Rectangle rect)
	{
		if (_currentMouseOverPage == null || _currentMouseOverPage != page)
		{
			return;
		}
		try
		{
			bool flag = false;
			Bitmap bitmap = null;
			if (Theme.SelectedAuditaiTheme.ThemeContext.OutBarPageMoreMenuImageIndex == OutBarPageMoreMenuImageIndex.White)
			{
				if (_menuMoreOperationWhiteImage == null)
				{
					_menuMoreOperationWhiteImage = (Bitmap)new WhiteImageStrategy().ProcessImage(Resources.menuMoreOperation);
				}
				bitmap = _menuMoreOperationWhiteImage;
				flag = true;
			}
			else
			{
				bitmap = Resources.menuMoreOperation;
			}
			if (_isMouseOverMoreMenuIcon)
			{
				if (flag)
				{
					_brushMoreMenuIconBackground.Color = Auditai.UI.Controls.Util.LightColor(_pageTitleBackgroundColor, 0.2);
				}
				else
				{
					_brushMoreMenuIconBackground.Color = Auditai.UI.Controls.Util.DarkenColor(_pageTitleBackgroundColor, 0.1);
				}
				e.Graphics.FillRectangle(_brushMoreMenuIconBackground, GetPageMoreMenuIconBackgroundRectangle(page));
			}
			e.Graphics.DrawImage(bitmap, GetPageMoreMenuIconLeftTopPosition(page));
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void C1OutPageSelectEvent(C1OutBarEx.PageSelectEvent e)
	{
		if (_currentMouseOverPage != null && GetPageMoreMenuIconBackgroundRectangle(e.Page).Contains(e.MouseEvent.X, e.MouseEvent.Y))
		{
			e.Cancel = true;
			_otbNavs.Invalidate(e.Page.CaptionBounds);
			_ctxNav.ShowContextMenu(_otbNavs, new Point(e.MouseEvent.X, e.MouseEvent.Y));
		}
	}

	private Point GetPageMoreMenuIconLeftTopPosition(C1OutPage page)
	{
		Rectangle captionBounds = page.CaptionBounds;
		int num = 25;
		int x = captionBounds.X + captionBounds.Width - num;
		int y = captionBounds.Y + (captionBounds.Height - Resources.menuMoreOperation.Height) / 2;
		return new Point(x, y);
	}

	private Rectangle GetPageMoreMenuIconBackgroundRectangle(C1OutPage page)
	{
		Point pageMoreMenuIconLeftTopPosition = GetPageMoreMenuIconLeftTopPosition(page);
		int num = 3;
		int num2 = 3;
		return new Rectangle(pageMoreMenuIconLeftTopPosition.X - num, pageMoreMenuIconLeftTopPosition.Y - num2, Resources.menuMoreOperation.Width + num * 2, Resources.menuMoreOperation.Height + num2 * 2);
	}

	private void _otbNavs_MouseMove(object sender, MouseEventArgs e)
	{
		C1OutPage hitPage = _otbNavs.GetHitPage(e.X, e.Y);
		if (hitPage != _currentMouseOverPage)
		{
			if (_currentMouseOverPage != null)
			{
				_otbNavs.Invalidate(_currentMouseOverPage.CaptionBounds);
				_currentMouseOverPage = null;
			}
			_currentMouseOverPage = hitPage;
			if (hitPage != null)
			{
				_isMouseOverMoreMenuIcon = GetPageMoreMenuIconBackgroundRectangle(hitPage).Contains(e.X, e.Y);
				_otbNavs.Invalidate(hitPage.CaptionBounds);
			}
		}
		else if (_currentMouseOverPage != null)
		{
			bool flag = GetPageMoreMenuIconBackgroundRectangle(_currentMouseOverPage).Contains(e.X, e.Y);
			if (_isMouseOverMoreMenuIcon != flag)
			{
				_isMouseOverMoreMenuIcon = flag;
				_otbNavs.Invalidate(_currentMouseOverPage.CaptionBounds);
			}
		}
	}

	private void _otbNavs_MouseLeave(object sender, EventArgs e)
	{
		if (_currentMouseOverPage != null)
		{
			_otbNavs.Invalidate(_currentMouseOverPage.CaptionBounds);
		}
		_isMouseOverMoreMenuIcon = false;
		_currentMouseOverPage = null;
	}

	private void _otbNavs_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			_ctxNav.ShowContextMenu(_otbNavs, e.Location);
		}
	}

	private void Trigger_Tick(object sender, EventArgs e)
	{
		if (Table != null && Ticket.Navs.Count == 0)
		{
			_otbNavs.Invalidate();
		}
	}

	private void JumpToCurrentRecordAvailableTableRow(TicketNavGrid ticketNavGrid = null)
	{
		if (ticketNavGrid == null)
		{
			if (_navGrids.Count == 0)
			{
				return;
			}
			ticketNavGrid = _currentNavGrid;
		}
		TicketRecord selectedRecord = ticketNavGrid.SelectedRecord as TicketRecord;
		if (selectedRecord != null && selectedRecord.Rows != null && selectedRecord.Rows.Count > 0)
		{
			Program.MainForm.TableEditor.ScrollToAvailableRow(selectedRecord.Rows);
		}
	}

	private void _ticketNavGrid_VirtualNodeSelected(object sender, EventArgs e)
	{
		if (_isSuspendVirtualNodeSelectChangeEvent || (!_isInTrySelectTicketNavTreeFirstNodeMode && Program.MainForm.CurrentView != MainFormView.TicketInput && Program.MainForm.CurrentView != MainFormView.TicketPrint) || !(sender is TicketNavGrid ticketNavGrid))
		{
			return;
		}
		Dictionary<Id64, string> comboListCellInitValue = null;
		int selectedVirtualNodeRowIndex = ticketNavGrid.SelectedVirtualNodeRowIndex;
		if (ticketNavGrid.SelectedVirtualNode != null)
		{
			comboListCellInitValue = (ticketNavGrid.SelectedVirtualNode as System.Collections.IEnumerable).Cast<Tuple<Id64, string>>().ToDictionary((Tuple<Id64, string> u) => u.Item1, (Tuple<Id64, string> v) => v.Item2);
		}
		if (!_isSuspendTicketSelectRangeAndScrollPositionCacheEvent)
		{
			SaveGridSelectRangeAndScrollPosition();
		}
		_isSuspendVirtualNodeSelectChangeEvent = true;
		SuspendDrawing();
		_otbNavs.SuspendDrawing();
		try
		{
			string treeNodePath = ticketNavGrid.GetTreeNodePath(selectedVirtualNodeRowIndex);
			OpenVirtualNavNode(comboListCellInitValue);
			try
			{
				if (_currentNavGrid != ticketNavGrid)
				{
					_currentNavGrid.FindAndSelectTreeNodePath(treeNodePath);
				}
			}
			catch
			{
			}
			if (!_isSuspendTicketSelectRangeAndScrollPositionCacheEvent)
			{
				RestoreGridSelectRangeAndScrollPosition();
			}
		}
		finally
		{
			_isSuspendVirtualNodeSelectChangeEvent = false;
			_otbNavs.ResumeDrawing();
			ResumeDrawing();
		}
	}

	private void _ticketNavGrid_RecordSelected(object sender, EventArgs e)
	{
		if (_radioButtonOpenTableView.Checked)
		{
			SaveCurrentSelectdRecordByRowId();
			if (Program.MainForm.CurrentView != MainFormView.TicketPrint && Program.MainForm.CurrentView != MainFormView.TablePreview)
			{
				JumpToCurrentRecordAvailableTableRow(sender as TicketNavGrid);
			}
			return;
		}
		if (Program.MainForm.CurrentView == MainFormView.TablePreview)
		{
			SaveCurrentSelectdRecordByRowId();
			return;
		}
		if (!_isSuspendTicketSelectRangeAndScrollPositionCacheEvent)
		{
			SaveGridSelectRangeAndScrollPosition();
		}
		SuspendDrawing();
		try
		{
			TicketNavGrid ticketNavGrid = sender as TicketNavGrid;
			int currentIndex = ticketNavGrid.GetCurrentIndex();
			SaveRecord(isSaveReccordFilterSetting: true, isRePopulate: false);
			bool isTableSaveActionOccured = _isTableSaveActionOccured;
			if (_isTableDataReloadFromDB)
			{
				_currentRecord = currentIndex;
			}
			else
			{
				_currentRecord = ticketNavGrid.GetCurrentIndex();
			}
			_isInShowingVirtualNode = false;
			_isCurrentTicketComeFromVirtualNode = false;
			if (isTableSaveActionOccured)
			{
				Populate(isOpenSelectedRecordOnLeave: false, isExandToCurrentSelecteRecord: false);
			}
			else
			{
				PopulateRecord();
			}
			RestoreRecordFilterSetting();
			TicketNavTreeStatusDataCacher.SaveNavTreeSelectedRecordIndex(Table.Id, _currentRecord, isSelectByRowId: false, 0L);
			_gridDecorator.SetCellFlickerDirty();
			_gridDecorator.RefreshCellFlickerWithOutRebuild();
			SendTicketNavTreeNodeChangeEventToOtherClient();
			if (!_isSuspendTicketSelectRangeAndScrollPositionCacheEvent)
			{
				RestoreGridSelectRangeAndScrollPosition();
			}
		}
		finally
		{
			ResumeDrawing();
		}
		if (Program.MainForm.CurrentView != MainFormView.TicketPrint)
		{
			return;
		}
		try
		{
			if (Record != null)
			{
				Program.MainForm.TicketPrinter.Ticket = Ticket;
				Program.MainForm.TicketPrinter.SetVm(Ticket, Record);
				Program.MainForm.TicketPrinter.Populate();
			}
		}
		catch (Exception exception)
		{
			exception.Log("表单打印预览模式下，切换表单节点时发生了未预期的异常");
		}
	}

	private async Task TicketNavTreeNodeChanged_SignalR()
	{
		try
		{
			int selectedIndex = _otbNavs.SelectedIndex;
			if (selectedIndex >= 0 && selectedIndex < _navGrids.Count)
			{
				string recordNavTreeNodeOpenPath = _currentNavGrid.GetRecordNavTreeNodeOpenPath(Record);
				string tableId = Table.Id.Value.ToString();
				await SignalRClient.OpenTicketNavTreeNode(tableId, recordNavTreeNodeOpenPath);
			}
		}
		catch (Exception exception)
		{
			exception.Log("向其它客户端发送表单导航树节点打开消息时发生了未预期的异常");
		}
	}

	public void FlagSomeUserOpenFileTreeNode(string userId)
	{
		_gridDecorator.HandleOpenNavTreeNodeDelay(userId);
	}

	public void OnNavTreeCurrentSelectedRecordIndexChanged(TicketNavGrid grid)
	{
		_currentRecord = grid.GetCurrentIndex();
		TicketNavTreeStatusDataCacher.SaveNavTreeSelectedRecordIndex(Table.Id, _currentRecord, isSelectByRowId: false, 0L);
		_gridDecorator.ReBuildNavTreeFlicker();
		SendTicketNavTreeNodeChangeEventToOtherClient();
	}

	private void SendTicketNavTreeNodeChangeEventToOtherClient()
	{
		_ = TicketNavTreeNodeChanged_SignalR();
	}

	private void SaveRecordFilterSetting(bool isSaveToPreSelectedNavGrid = false)
	{
		try
		{
			if (Table != null)
			{
				TicketNavGrid navTree = null;
				TicketRecord ticketRecord = null;
				if (Ticket.Navs.Count > 0)
				{
					navTree = (isSaveToPreSelectedNavGrid ? _preSelectedNavGrid : _currentNavGrid);
					ticketRecord = Record;
				}
				string filterSetting = null;
				if (_grid.FilterManager.Filters.Count > 0)
				{
					filterSetting = _grid.FilterManager.Filters.Serialize();
				}
				TicketNavTreeStatusDataCacher.SaveNavTreeRecordFilterSetting(Table.Id, navTree, ticketRecord, filterSetting);
			}
		}
		catch (Exception exception)
		{
			exception.Log("保存表单的筛选状态时发生了未预期的异常");
		}
	}

	private void ClearRecordFilterSetting()
	{
		TicketNavGrid navTree = null;
		TicketRecord ticketRecord = null;
		if (Ticket.Navs.Count > 0)
		{
			navTree = _currentNavGrid;
			ticketRecord = Record;
		}
		TicketNavTreeStatusDataCacher.SaveNavTreeRecordFilterSetting(Table.Id, navTree, ticketRecord, null);
	}

	private bool RestoreRecordFilterSetting()
	{
		try
		{
			TicketNavGrid navTree = null;
			TicketRecord ticketRecord = null;
			if (Ticket.Navs.Count > 0)
			{
				navTree = _currentNavGrid;
				ticketRecord = Record;
			}
			string navTreeRecordFilterSetting = TicketNavTreeStatusDataCacher.GetNavTreeRecordFilterSetting(Table.Id, navTree, ticketRecord);
			if (string.IsNullOrWhiteSpace(navTreeRecordFilterSetting))
			{
				return false;
			}
			_grid.FilterManager.Filters.Deserialize(navTreeRecordFilterSetting);
			_grid.FilterManager.Execute();
			return true;
		}
		catch (Exception exception)
		{
			exception.Log("还原表单的筛选状态时发生了未预期的异常");
			return true;
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

	protected bool IsCellValueEmpty(TicketInputCellVM cell)
	{
		object value = cell.Value;
		if (!(value is string value2))
		{
			if (value is double num)
			{
				return num == 0.0;
			}
			return false;
		}
		return string.IsNullOrWhiteSpace(value2);
	}

	protected Queue<int> GetAllEmptyRow(List<int> emptyRowCheckColumnList)
	{
		Queue<int> queue = new Queue<int>();
		int dataRowStart = Ticket.DataRowStart;
		int dataRowsCount = _vm.DataRowsCount;
		int count = Ticket.Columns.Count;
		bool[] array = new bool[count];
		if (emptyRowCheckColumnList == null || emptyRowCheckColumnList.Count == 0)
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = true;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				array[j] = emptyRowCheckColumnList.Contains(j);
			}
		}
		for (int k = 0; k < dataRowsCount; k++)
		{
			int num = dataRowStart + k;
			bool flag = true;
			for (int l = 0; l < count; l++)
			{
				if (!array[l])
				{
					continue;
				}
				TicketInputCellVM cellVM = _vm.GetCellVM(num, l);
				if (cellVM.Column != null)
				{
					string displayValue = cellVM.GetDisplayValue();
					if (!("" == displayValue) && !IsCellValueEmpty(cellVM))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				queue.Enqueue(num);
			}
		}
		return queue;
	}

	public void SaveGridSelectRangeAndScrollPosition()
	{
		if (Table != null && Ticket != null && _grid != null)
		{
			TableTicketSelectionRangeAndScrollStatusCacher.SaveTicketCacheData(Table.Id, new TableTicketSelectionRangeAndScrollStatusCacher.TicketGridStatusCacheData
			{
				Selection = _grid.Selection,
				ScrollPosition = _grid.ScrollPosition
			});
		}
	}

	private void RestoreGridSelectRangeAndScrollPosition()
	{
		try
		{
			TableTicketSelectionRangeAndScrollStatusCacher.TicketGridStatusCacheData ticketGridCacheData = GetTicketGridCacheData();
			if (ticketGridCacheData != null)
			{
				_grid.SafeSelect(ticketGridCacheData.Selection.TopRow, ticketGridCacheData.Selection.LeftCol, ticketGridCacheData.Selection.BottomRow, ticketGridCacheData.Selection.RightCol, isToShow: false);
				_grid.ScrollPosition = ticketGridCacheData.ScrollPosition;
			}
		}
		catch (Exception)
		{
		}
	}

	private TableTicketSelectionRangeAndScrollStatusCacher.TicketGridStatusCacheData GetTicketGridCacheData()
	{
		if (Table == null || Ticket == null)
		{
			return null;
		}
		if (_grid == null)
		{
			return null;
		}
		return TableTicketSelectionRangeAndScrollStatusCacher.GetTicketCacheData(Table.Id);
	}

	private List<RowFillSetting> PrepareVMDataForFillCollectResult_Balance(TableCollectorBalance balanceCollector, TableCollectResult collectResult, TicketCollectFillTable ticketTable, RowIncreaseSetting rowIncreaseSetting)
	{
		List<RowFillSetting> list = null;
		List<int> list2 = new List<int>();
		if (balanceCollector.Maps.Any((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")))
		{
			int num = (int)balanceCollector.Maps.First((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")).Key;
			list2.Add(num);
			Auditai.Model.Column columnByIndex = ticketTable.GetColumnByIndex(num);
			if (columnByIndex != null && collectResult.Values.ContainsKey(columnByIndex))
			{
				list = new List<RowFillSetting>();
				List<object> list3 = collectResult.Values[columnByIndex];
				Dictionary<string, int> dictionary = GetColumnValueList(num);
				int count = collectResult.Values.First().Value.Count;
				for (int i = 0; i < count; i++)
				{
					object obj = list3[i];
					int value;
					if (obj == null)
					{
						list.Add(new RowFillSetting
						{
							IsNewRow = true
						});
					}
					else if (!dictionary.TryGetValue(obj.ToString(), out value))
					{
						list.Add(new RowFillSetting
						{
							IsNewRow = true
						});
					}
					else
					{
						list.Add(new RowFillSetting
						{
							IsNewRow = false,
							RowIndex = value
						});
					}
				}
			}
		}
		if (list == null)
		{
			list = new List<RowFillSetting>();
			int count2 = collectResult.Values.First().Value.Count;
			for (int j = 0; j < count2; j++)
			{
				list.Add(new RowFillSetting
				{
					IsNewRow = true
				});
			}
		}
		Queue<int> allEmptyRow = GetAllEmptyRow(list2);
		int num2 = list.Sum((RowFillSetting u) => u.IsNewRow ? 1 : 0);
		if (num2 > allEmptyRow.Count)
		{
			int num3 = Ticket.DataRowStart + _vm.DataRowsCount;
			int num4 = num2 - allEmptyRow.Count;
			_vm.AppendDataRows(num4);
			rowIncreaseSetting.IncreaseStartIndex = num3;
			rowIncreaseSetting.IncreaseCount = num4;
			for (int k = 0; k < num4; k++)
			{
				allEmptyRow.Enqueue(num3 + k);
			}
		}
		foreach (RowFillSetting item in list)
		{
			if (item.IsNewRow)
			{
				item.IsNewRow = false;
				item.RowIndex = allEmptyRow.Dequeue();
			}
		}
		return list;
		Dictionary<string, int> GetColumnValueList(int colIndex)
		{
			int dataRowStart = Ticket.DataRowStart;
			int dataRowsCount = _vm.DataRowsCount;
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			for (int l = 0; l < dataRowsCount; l++)
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(dataRowStart + l, colIndex);
				string displayValue = cellVM.GetDisplayValue();
				if (!dictionary2.ContainsKey(displayValue))
				{
					dictionary2.Add(displayValue, dataRowStart + l);
				}
			}
			return dictionary2;
		}
	}

	private List<RowFillSetting> PrepareVMDataForFillCollectResult_Subsidiary(TableCollectorSubsidiary subsidiayCollector, TableCollectResult collectResult, TicketCollectFillTable ticketTable, RowIncreaseSetting rowIncreaseSetting)
	{
		List<RowFillSetting> list = null;
		List<Auditai.Model.Column> condtionCols = (from m in subsidiayCollector.Maps
			where new string[7] { "日期", "字号", "字", "号", "摘要", "借方金额", "贷方金额" }.Contains(m.Value)
			select m into kv
			select ticketTable.GetColumnByIndex((int)kv.Key)).ToList();
		List<Auditai.Model.Column> decimalValueCols = (from m in subsidiayCollector.Maps
			where new string[2] { "借方金额", "贷方金额" }.Contains(m.Value)
			select m into kv
			select ticketTable.GetColumnByIndex((int)kv.Key)).ToList();
		if (condtionCols.Count != 0)
		{
			list = new List<RowFillSetting>();
			List<List<Tuple<string, TicketInputCellVM, int>>> rowsCondtionDataList2 = GetAllRowsConditionData(condtionCols);
			int count = collectResult.Values.First().Value.Count;
			for (int i = 0; i < count; i++)
			{
				int num = GetGridMatchRowIndex(rowsCondtionDataList2, condtionCols, i);
				if (num < 0)
				{
					list.Add(new RowFillSetting
					{
						IsNewRow = true
					});
				}
				else
				{
					list.Add(new RowFillSetting
					{
						IsNewRow = false,
						RowIndex = num
					});
				}
			}
		}
		if (list == null)
		{
			list = new List<RowFillSetting>();
			int count2 = collectResult.Values.First().Value.Count;
			for (int j = 0; j < count2; j++)
			{
				list.Add(new RowFillSetting
				{
					IsNewRow = true
				});
			}
		}
		int num2 = GetDataAppendStartRowIndex();
		int num3 = Ticket.DataRowStart + _vm.DataRowsCount - num2;
		int num4 = list.Sum((RowFillSetting u) => u.IsNewRow ? 1 : 0);
		if (num4 > num3)
		{
			_vm.AppendDataRows(num4 - num3);
			rowIncreaseSetting.IncreaseStartIndex = num2;
			rowIncreaseSetting.IncreaseCount = num4 - num3;
		}
		foreach (RowFillSetting item2 in list)
		{
			if (item2.IsNewRow)
			{
				item2.IsNewRow = false;
				item2.RowIndex = num2++;
			}
		}
		return list;
		List<List<Tuple<string, TicketInputCellVM, int>>> GetAllRowsConditionData(List<Auditai.Model.Column> condtionColIList)
		{
			List<List<Tuple<string, TicketInputCellVM, int>>> list3 = new List<List<Tuple<string, TicketInputCellVM, int>>>();
			int dataRowStart2 = Ticket.DataRowStart;
			int dataRowsCount2 = _vm.DataRowsCount;
			for (int n = 0; n < dataRowsCount2; n++)
			{
				List<Tuple<string, TicketInputCellVM, int>> list4 = new List<Tuple<string, TicketInputCellVM, int>>();
				bool flag2 = true;
				foreach (Auditai.Model.Column condtionColI in condtionColIList)
				{
					TicketInputCellVM cellVM2 = _vm.GetCellVM(dataRowStart2 + n, (int)condtionColI.Id.Value);
					string displayValue = cellVM2.GetDisplayValue();
					list4.Add(Tuple.Create(displayValue, cellVM2, dataRowStart2 + n));
					if (!string.IsNullOrWhiteSpace(displayValue))
					{
						flag2 = false;
					}
				}
				if (!flag2)
				{
					list3.Add(list4);
				}
			}
			return list3;
		}
		int GetDataAppendStartRowIndex()
		{
			int dataRowStart = Ticket.DataRowStart;
			int dataRowsCount = _vm.DataRowsCount;
			for (int num5 = dataRowStart + dataRowsCount - 1; num5 >= dataRowStart; num5--)
			{
				foreach (Auditai.Model.Column item3 in condtionCols)
				{
					TicketInputCellVM cellVM = _vm.GetCellVM(num5, (int)item3.Id.Value);
					if (!object.Equals(cellVM.GetDisplayValue(), "") && (!decimalValueCols.Contains(item3) || !IsCellValueEmpty(cellVM)))
					{
						return num5 + 1;
					}
				}
			}
			return dataRowStart;
		}
		int GetGridMatchRowIndex(List<List<Tuple<string, TicketInputCellVM, int>>> rowsCondtionDataList, List<Auditai.Model.Column> condtionColIList, int checkRowIndex)
		{
			int count3 = rowsCondtionDataList.Count;
			for (int k = 0; k < count3; k++)
			{
				List<Tuple<string, TicketInputCellVM, int>> list2 = rowsCondtionDataList[k];
				if (list2.Count == 0)
				{
					return -1;
				}
				int count4 = condtionColIList.Count;
				bool flag = true;
				for (int l = 0; l < count4; l++)
				{
					Auditai.Model.Column column = condtionColIList[l];
					object objA = list2[(int)column.Id.Value].Item2.ConvertInputValueToDisplayValue(collectResult.Values[column][checkRowIndex]);
					string item = list2[(int)column.Id.Value].Item1;
					if (!object.Equals(objA, item))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return list2[0].Item3;
				}
			}
			return -1;
		}
	}

	private List<RowFillSetting> PrepareVMDataForFillCollectResult_Summary(TableCollectorSummary summaryCollector, TableCollectResult collectResult, TicketCollectFillTable ticketTable, RowIncreaseSetting rowIncreaseSetting)
	{
		List<RowFillSetting> list = null;
		List<int> list2 = new List<int>();
		if (summaryCollector.Maps.Any((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")))
		{
			int num = (int)summaryCollector.Maps.First((KeyValuePair<long, string> m) => m.Value.Equals("科目名称")).Key;
			list2.Add(num);
			Auditai.Model.Column columnByIndex = ticketTable.GetColumnByIndex(num);
			if (columnByIndex != null && collectResult.Values.ContainsKey(columnByIndex))
			{
				list = new List<RowFillSetting>();
				List<object> list3 = collectResult.Values[columnByIndex];
				Dictionary<string, int> dictionary = GetColumnValueList(num);
				int count = collectResult.Values.First().Value.Count;
				for (int i = 0; i < count; i++)
				{
					object obj = list3[i];
					int value;
					if (obj == null)
					{
						list.Add(new RowFillSetting
						{
							IsNewRow = true
						});
					}
					else if (!dictionary.TryGetValue(obj.ToString(), out value))
					{
						list.Add(new RowFillSetting
						{
							IsNewRow = true
						});
					}
					else
					{
						list.Add(new RowFillSetting
						{
							IsNewRow = false,
							RowIndex = value
						});
					}
				}
			}
		}
		if (list == null)
		{
			list = new List<RowFillSetting>();
			int count2 = collectResult.Values.First().Value.Count;
			for (int j = 0; j < count2; j++)
			{
				list.Add(new RowFillSetting
				{
					IsNewRow = true
				});
			}
		}
		Queue<int> allEmptyRow = GetAllEmptyRow(list2);
		int num2 = list.Sum((RowFillSetting u) => u.IsNewRow ? 1 : 0);
		if (num2 > allEmptyRow.Count)
		{
			int num3 = Ticket.DataRowStart + _vm.DataRowsCount;
			int num4 = num2 - allEmptyRow.Count;
			_vm.AppendDataRows(num4);
			rowIncreaseSetting.IncreaseStartIndex = num3;
			rowIncreaseSetting.IncreaseCount = num4;
			for (int k = 0; k < num4; k++)
			{
				allEmptyRow.Enqueue(num3 + k);
			}
		}
		foreach (RowFillSetting item in list)
		{
			if (item.IsNewRow)
			{
				item.IsNewRow = false;
				item.RowIndex = allEmptyRow.Dequeue();
			}
		}
		return list;
		Dictionary<string, int> GetColumnValueList(int colIndex)
		{
			int dataRowStart = Ticket.DataRowStart;
			int dataRowsCount = _vm.DataRowsCount;
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			for (int l = 0; l < dataRowsCount; l++)
			{
				TicketInputCellVM cellVM = _vm.GetCellVM(dataRowStart + l, colIndex);
				string displayValue = cellVM.GetDisplayValue();
				if (!dictionary2.ContainsKey(displayValue))
				{
					dictionary2.Add(displayValue, dataRowStart + l);
				}
			}
			return dictionary2;
		}
	}

	public void FillCollectResult(TableCollectResult collectResult, TicketCollectFillTable ticketTable)
	{
		if (collectResult == null || collectResult.Values.Count == 0 || collectResult.Values.First().Value.Count == 0)
		{
			return;
		}
		RowIncreaseSetting rowIncreaseSetting = new RowIncreaseSetting();
		List<RowFillSetting> list;
		try
		{
			TableCollectorAbstract tableCollector = collectResult.TableCollector;
			if (!(tableCollector is TableCollectorBalance balanceCollector))
			{
				if (!(tableCollector is TableCollectorSubsidiary subsidiayCollector))
				{
					if (!(tableCollector is TableCollectorSummary summaryCollector))
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前版本暂时不支持该类型的采账填充!");
						return;
					}
					list = PrepareVMDataForFillCollectResult_Summary(summaryCollector, collectResult, ticketTable, rowIncreaseSetting);
				}
				else
				{
					list = PrepareVMDataForFillCollectResult_Subsidiary(subsidiayCollector, collectResult, ticketTable, rowIncreaseSetting);
				}
			}
			else
			{
				list = PrepareVMDataForFillCollectResult_Balance(balanceCollector, collectResult, ticketTable, rowIncreaseSetting);
			}
			if (list.Count == 0)
			{
				return;
			}
		}
		catch (TableModelException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return;
		}
		SuspendDrawing();
		try
		{
			_vm.BeginBatchUpdateValue();
			_vm.BuildTableRowsForTicketDataRows_DynamicRow(list.Min((RowFillSetting u) => u.RowIndex), list.Count);
			foreach (RowFillSetting item in list)
			{
				int rowIndex = item.RowIndex;
				if (_vm.GetRow(rowIndex).TableRow == null)
				{
					_vm.BuildTableRowsForTicketDataRows_DynamicRow(rowIndex, 1);
				}
			}
			int count = Ticket.Columns.Count;
			bool[] array = new bool[count];
			for (int i = 0; i < count; i++)
			{
				TicketInputColumnVM column = _vm.GetColumn(i);
				array[i] = IsGridColumnCanEdit((Auditai.Model.Column)column.TableColumn);
			}
			foreach (Auditai.Model.Column key in collectResult.Values.Keys)
			{
				int num = (int)key.Id.Value;
				if (!array[num])
				{
					continue;
				}
				List<object> list2 = collectResult.Values[key];
				int count2 = list2.Count;
				for (int j = 0; j < count2; j++)
				{
					int rowIndex2 = list[j].RowIndex;
					TicketInputCellVM cellVM = _vm.GetCellVM(rowIndex2, num);
					if (IsGridCellCanEditWithoutColumnCheck(cellVM))
					{
						object obj = list2[j];
						if (obj is double num2 && num2 == 0.0)
						{
							obj = ((!cellVM.IsSetToNumberFormat()) ? "" : ((object)0.0));
						}
						_vm.UpdateTicketCellValue(cellVM, Auditai.Model.Cell.ConvertInputValueToCellValue(obj), isFormulaExistManualInputValue: true);
					}
				}
			}
			_vm.EndBatchUpdateValue();
			_isDirty = true;
			_vm.CalculateTicket();
			PopulateVm();
			SetCommandState();
		}
		catch (Exception ex2)
		{
			ex2.Log("表单采账填充时发生了未预期的异常");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		finally
		{
			ResumeDrawing();
		}
	}
}
