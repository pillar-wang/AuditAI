using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls.CollectTable;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class frmTableCollect2 : C1RibbonForm
{
	protected class DataGridFilterContext : FilterContext
	{
		private readonly C1FlexGridEx _grid;

		private readonly frmTableCollect2 _parent_form;

		public DataGridFilterContext(frmTableCollect2 parent)
		{
			_grid = parent._dataGrid;
			_parent_form = parent;
		}

		public override List<FilterValue> GetColumnData(string columnId)
		{
			int col = GetColumnIndex(columnId);
			return (from i in Enumerable.Range(0, _grid.BodyRowsCount)
				select GetData(i, col)).ToList();
		}

		public override string GetColumnId(C1.Win.C1FlexGrid.Column col)
		{
			int index = col.Index;
			if (_parent_form.selectCollectObject() == CollectObjectEnum.Balance && index < 3)
			{
				return null;
			}
			return (index - _grid.Cols.Fixed).ToString();
		}

		public override int GetColumnIndex(string columnId)
		{
			if (columnId == null)
			{
				return -1;
			}
			return int.Parse(columnId);
		}

		public override FilterValue GetData(int row, int col)
		{
			return FilterValue.FromObject(_grid.BodyGetData(row, col), _grid.GetDataDisplay(row + _grid.Rows.Fixed, col + _grid.Cols.Fixed));
		}

		public override Type GetColumnDataType(string columnId)
		{
			TableHeaderCellSetting tableHeaderSetting = _parent_form.GetTableHeaderSetting(_parent_form._tableHeaderRowsCount - 1, GetColumnIndex(columnId) + _grid.Cols.Fixed);
			if (tableHeaderSetting.ColumnMappingData == null || !tableHeaderSetting.ColumnMappingData.IsMappingItemSet)
			{
				return typeof(string);
			}
			if (tableHeaderSetting.ColumnMappingData.MappingItemName == "科目代码" || tableHeaderSetting.ColumnMappingData.MappingItemName == "科目名称")
			{
				return typeof(string);
			}
			return typeof(decimal);
		}

		public override string GetColumnDataTypeFormatString(string columnId)
		{
			return string.Empty;
		}

		public override Tuple<bool, string, string> IsCheckBox(int row, int col)
		{
			if (!(_grid.BodyGetCell(row, col).Style?.DataType == typeof(bool)))
			{
				return Tuple.Create(item1: false, "", "");
			}
			return Tuple.Create(item1: true, "选中", "未选中");
		}

		public override string GetColumnCaption(string columnId)
		{
			TableHeaderCellSetting tableHeaderSetting = _parent_form.GetTableHeaderSetting(_parent_form._tableHeaderRowsCount - 1, GetColumnIndex(columnId) + _grid.Cols.Fixed);
			if (tableHeaderSetting.CaptionDisplaySetting != null)
			{
				return tableHeaderSetting.CaptionDisplaySetting.CaptionText;
			}
			return "";
		}
	}

	protected class TableColumnMappingData
	{
		public int RowIndex { get; set; }

		public int ColumnIndex { get; set; }

		public string MappingItemName { get; set; }

		public Id64 TableColumnId { get; set; }

		public Auditai.Model.Column TableColumn { get; set; }

		public bool IsMappingItemSet { get; set; }

		public bool IsUsePositiveValue { get; set; }

		internal int DataTableColIndex { get; set; } = -1;


		public TableColumnMappingData()
		{
		}

		public TableColumnMappingData(int rowIndex, int colIndex, Auditai.Model.Column column)
		{
			RowIndex = rowIndex;
			ColumnIndex = colIndex;
			TableColumnId = column.Id;
			TableColumn = column;
		}
	}

	protected class TableHeaderCellColumnMappingDisplaySetting
	{
		public int CellLeftPadding { get; set; }

		public int CellRightPadding { get; set; }

		public int CellTopPadding { get; set; } = 3;


		public int CellBottomPadding { get; set; } = 3;


		public Rectangle MappingItemTextDrawRectInCell { get; set; }

		public Rectangle MappingItemTextDrawRectInGrid { get; set; }

		public string MappingItemDisplayString { get; set; }

		public int DisplayAreaMinHeight { get; set; }

		public C1.Win.C1FlexGrid.CellRange CellAreaRange { get; set; }
	}

	protected class TableHeaderCellCaptionDisplaySetting
	{
		public List<TableHeaderCellSetting> ColumnMappingItemList;

		public int CellLeftPadding { get; set; }

		public int CellRightPadding { get; set; }

		public int CellTopPadding { get; set; } = 3;


		public int CellBottomPadding { get; set; } = 3;


		public string CaptionText { get; set; }

		public Rectangle CaptionTextDrawRectInCell { get; set; }

		public string CaptionDisplayString { get; set; }

		public int DisplayAreaMinHeight { get; set; }

		public int DisplayAreaTextHeight { get; set; }
	}

	protected class TableHeaderCellSetting
	{
		public bool IsMergeRange { get; set; }

		public bool IsMergeRangeLeftTopCell { get; set; }

		public C1.Win.C1FlexGrid.CellRange CellAreaRange { get; set; }

		public int RowIndex { get; set; }

		public int ColumnIndex { get; set; }

		public bool IsMappingItemCell { get; set; }

		public TableColumnMappingData ColumnMappingData { get; set; }

		public TableHeaderCellColumnMappingDisplaySetting ColumnMappingDisplaySetting { get; set; }

		public TableHeaderCellCaptionDisplaySetting CaptionDisplaySetting { get; set; }
	}

	protected class MosueOverCellInfo
	{
		public int RowIndex { get; set; }

		public int ColIndex { get; set; }

		public Point MouseLocation { get; set; }

		public MosueOverCellInfo(int row, int col)
		{
			RowIndex = row;
			ColIndex = col;
		}
	}

	protected class RowData
	{
		public int RowIndex { get; set; }

		public object UserData { get; set; }
	}

	protected class ExpandToLevelData
	{
		public int ToLevel { get; set; }

		public bool isShowAuxItem { get; set; }
	}

	protected enum DataGridRowDataType
	{
		Account,
		AuxiliaryItem,
		Voucher
	}

	protected class DataGridRowUserData
	{
		public DataRow DataSource { get; set; }

		public Account AccountData { get; set; }

		public Tuple<Account, AuxiliaryItem> AuxiliaryData { get; set; }

		public Voucher VoucherData { get; set; }

		public DataGridRowDataType DataType { get; set; }

		public object RawData { get; set; }

		public DataGridRowUserData(object rowData, DataRow dataRow, CollectObjectEnum collectObject)
		{
			RawData = rowData;
			switch (collectObject)
			{
			case CollectObjectEnum.Balance:
			case CollectObjectEnum.Summary:
				if (rowData is Account accountData)
				{
					AccountData = accountData;
					DataSource = dataRow;
					DataType = DataGridRowDataType.Account;
					return;
				}
				if (rowData is Tuple<Account, AuxiliaryItem> auxiliaryData)
				{
					AuxiliaryData = auxiliaryData;
					DataSource = dataRow;
					DataType = DataGridRowDataType.AuxiliaryItem;
					return;
				}
				break;
			case CollectObjectEnum.Subsidiary:
				if (rowData is Voucher voucherData)
				{
					VoucherData = voucherData;
					DataSource = dataRow;
					DataType = DataGridRowDataType.Voucher;
					return;
				}
				break;
			}
			throw new InvalidCastException("表格行的数据格式不正确，无法进行处理!");
		}
	}

	private class VoucherAccountTreeNode
	{
		public VoucherAccountTreeNode parentNode;

		public object nodeData;

		public List<Tuple<AuxiliaryClass, List<AuxiliaryItem>>> auxClassList;

		public List<VoucherAccountTreeNode> Children;
	}

	private const string CO_BALANCE = "科目余额表";

	private const string CO_SUBSIDIARY = "记账凭证表";

	private const string CO_SUMMARY = "月度汇总表";

	private const string FORMAT_STRING_MONEY = "#,0.00;-#,0.00;#";

	private const string FORMAT_STRING_DATE = "yyyy-MM-dd";

	private const string CN_USERDATA = "UserData";

	private const string CN_PROJECT_NAME = "项目";

	private const string CN_ACCOUNT_CODE = "科目代码";

	private const string CN_ACCOUNT_NAME = "科目名称";

	private const string CN_DATE = "日期";

	private const string CN_TYPE = "字";

	private const string CN_NUMBER = "号";

	private const string CN_TYPE_NAME = "字号";

	private const string CN_DIGEST = "摘要";

	private const string CN_OPPOSITE = "对方科目";

	private const string CN_DC = "方向";

	private const string CN_DEBIT = "借方金额";

	private const string CN_CREDIT = "贷方金额";

	private const string CN_BALANCE = "余额";

	private static string[] SubsidiaryExcludeItemOnAllAccountSelected = new string[2] { "余额", "方向" };

	private C1ContextMenu _ctxMappingMenu = new C1ContextMenu();

	private C1ContextMenu _ctxIconMenu = new C1ContextMenu();

	private C1ContextMenu _ctxCellRightClick = new C1ContextMenu();

	private C1ContextMenu _ctxColSort = new C1ContextMenu();

	private C1FlexGridEx _dataGrid;

	private Ledger _ledger;

	private Auditai.Model.Table _table;

	private int _ledger_max_level;

	private int[] _titleGridRawWidthArr;

	private int[] _titleGridRawHeightArr;

	private int _titlePanelHeight;

	private List<string> _detailAccountNameCheckList = new List<string>();

	private bool _isDisableCollectObjectCombox;

	public static Func<int, bool> LicenseCheckHandleOnCopyLedgerData;

	private Form owner;

	private System.Windows.Forms.TreeNode _currentSelectedAccountNode;

	#pragma warning disable CS0649
		private bool _isSuspendNotFilterAccountCheckEvent;
#pragma warning restore CS0649

	private bool _isInResizingColumn;

	private VoucherAccountTreeNode _myMarkAccountTreeRootNode;

	private Dictionary<Account, VoucherAccountTreeNode> _myMarkAccountDataSet;

	protected List<Tuple<DateTime, DateTime, TrialBalanceSheet>> _trialBalanceSheetCache = new List<Tuple<DateTime, DateTime, TrialBalanceSheet>>();

	private bool _attachEvent;

	protected const int ORDER_NUMBER_COLUMN_INDEX = 0;

	protected const int CHECK_BOX_COLUMN_INDEX = 1;

	protected const int ADJUST_LEVEL_COLUMN_INDEX = 2;

	protected const int ACCOUNT_CODE_COLUMN_INDEX = 3;

	protected const int COLUMN_MIN_WIDTH = 100;

	protected const int additionalColumnCount = 4;

	private const int captionTextAndMappingItemTextSpaceHeight = 5;

	private const string CollectColumnUnSetDisplayString = "未设置采数列";

	private const string ContextMenuCollectColumnUnSetDisplayString = "空";

	private readonly Color HighLightColor = Color.White;

	private readonly Color DefaultBackgroundColor = Color.LightYellow;

	private Font _columHeaderCaptionFont = new Font("微软雅黑", 9f, FontStyle.Bold);

	private Font _dataGridCellFont = new Font("微软雅黑", 9f);

	private Font _MainTitleFont = new Font("微软雅黑", 12f, FontStyle.Bold);

	private Font _SubTitleFont = new Font("微软雅黑", 9f);

	private Brush _columnHeaderCaptionBrush = new SolidBrush(Color.Black);

	private Brush _columnHeaderUnSettingBrush = new SolidBrush(Color.Gray);

	private Brush _columnHeaderHasSettingBrush = new SolidBrush(Color.FromArgb(228, 109, 10));

	private SolidBrush _adjustLevelIconOnFocusBackgroundBrush = new SolidBrush(Color.Gray);

	private MosueOverCellInfo _mouseCurrentOverWhichAdjustLevelCell;

	protected int _tableHeaderRowsCount;

	protected int _tableHeaderColumnsCount;

	private List<int> _columnsMinWidthList = new List<int>();

	private List<TableHeaderCellSetting> _tableHeaderSettingList = new List<TableHeaderCellSetting>();

	private List<TableColumnMappingData> _balanceMapSettingData = new List<TableColumnMappingData>();

	private List<TableColumnMappingData> _monthSummaryMapSettingData = new List<TableColumnMappingData>();

	private List<TableColumnMappingData> _subsidiaryMapSettingData = new List<TableColumnMappingData>();

	private List<TableColumnMappingData> _ledgerAgeMappSettingData = new List<TableColumnMappingData>();

	private CollectObjectEnum _ctxMappingItemType = CollectObjectEnum.None;

	private TableColumnMappingData _currentSelectedColumnMappingData;

	private TableCollectorBalance _change_banlance_level_collector;

	private TableCollectorSummary _change_summary_level_collector;

	private DataTable _tableDataSourceColumnNameTable;

	private List<DataRow> _recollect_data_list = new List<DataRow>();

	private HashSet<int> _has_no_data_in_ledger_columns_index_set = new HashSet<int>();

	private List<int> _tableColumRawWithList = new List<int>();

	private int _tableRawTotalWidth;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1ContextMenu c1ContextMenu1;

	private C1CommandLink c1CommandLink1;

	private C1CommandHolder c1CommandHolder1;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlBottomButtons;

	private C1Button btnIntelligenceFill;

	private C1Button btnCancel;

	private C1Button btnConfirm;

	private C1SplitterPanel pnlDockingTab;

	private C1DockingTab DockingTab;

	private C1DockingTabPage TabPageBalance;

	private C1SplitContainer ctnBlance;

	private C1SplitterPanel pnlBalanceConditions;

	internal ComboTree comboAccountTree;

	internal ComboTree comboAuxiliaryTree;

	private C1Label lblCollectObject;

	private C1Label c1Label2;

	private C1ComboBox comboEndMonth;

	private C1Label lblKjqj;

	private C1ComboBox comboStartMonth;

	private C1Label lblAuxiliary;

	private C1ComboBox comboCollectObject;

	private C1Label lblStartMonth;

	private C1Label lblEndMonth;

	private C1SplitterPanel pnlTitlePanel;

	private C1FlexGrid _gridTitle;

	private C1SplitButtonEx spbAnalysis;

	private DropDownItem dropDownItem1;

	private DropDownItem dropDownItem2;

	private DropDownItem dropDownItem3;

	private C1SplitterPanel panelDataTable;

	private C1CheckBox checkBoxAllAccount;

	private C1CheckBox checkBoxOnlyMyMark;

	private C1CheckBox checkBoxNotFilterAccount;

	public TableCollectorAbstract Collector { get; private set; }

	public C1FlexGridEx DataView
	{
		get
		{
			return _dataGrid;
		}
		private set
		{
			_dataGrid = value;
		}
	}

	public int MaxLevel { get; set; } = -1;


	public AccNameStyleEnum CurrentAccountNameStyle { get; set; } = AccNameStyleEnum.SecondFullName;


	public TableCollectResult Result { get; private set; }

	public frmTableCollect2(Ledger ledger, Auditai.Model.Table table, Form owner)
	{
		if (ledger == null)
		{
			throw new Exception("请先打开账套");
		}
		if (table == null)
		{
			throw new Exception("请先打开表格");
		}
		InitializeComponent();
		base.Shown += FrmTableCollect_Shown;
		base.SizeChanged += FrmTableCollect2_SizeChanged;
		this.owner = owner;
		_ledger = ledger;
		_table = table;
		Initialize(ledger, table);
		AttachEvent();
	}

	private void FrmTableCollect2_SizeChanged(object sender, EventArgs e)
	{
		AutoAdjustTableHeaderDisplaySetting();
	}

	private void FrmTableCollect_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.TableCollect16);
	}

	private void AdjustButtonToCenter()
	{
		int num = 150;
		int num2 = pnlBottomButtons.Width / 2;
		int num3 = pnlBottomButtons.Height / 2;
		Point location = btnIntelligenceFill.Location;
		location.X = num2 - btnConfirm.Width / 2 - num - btnIntelligenceFill.Width;
		location.Y = num3 - btnIntelligenceFill.Height / 2;
		btnIntelligenceFill.Location = location;
		location = btnConfirm.Location;
		location.X = num2 - btnConfirm.Width / 2;
		location.Y = num3 - btnConfirm.Height / 2;
		btnConfirm.Location = location;
		location = btnCancel.Location;
		location.X = num2 + btnConfirm.Width / 2 + num;
		location.Y = num3 - btnCancel.Height / 2;
		btnCancel.Location = location;
	}

	private void ExtendDataGridToWindowWidth()
	{
		if (base.WindowState != FormWindowState.Maximized)
		{
			return;
		}
		int num = base.Size.Width - 18;
		int count = _dataGrid.Cols.Count;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[i];
			if (column.Visible)
			{
				num2 += column.WidthDisplay;
			}
		}
		int num3 = 0;
		for (int j = 0; j < 4; j++)
		{
			C1.Win.C1FlexGrid.Column column2 = _dataGrid.Cols[j];
			if (column2.Visible)
			{
				num3 += column2.WidthDisplay;
			}
		}
		int num4 = 0;
		int num5 = 0;
		List<Tuple<C1.Win.C1FlexGrid.Column, int>> list = new List<Tuple<C1.Win.C1FlexGrid.Column, int>>();
		for (int k = 4; k < count; k++)
		{
			C1.Win.C1FlexGrid.Column item = _dataGrid.Cols[k];
			int num6 = _tableColumRawWithList[k - 4];
			num5 += num6;
			list.Add(Tuple.Create(item, num6));
		}
		int val = num - num3;
		val = Math.Max(val, _tableRawTotalWidth);
		int num7 = val - num4;
		int num8 = num7;
		int count2 = list.Count;
		for (int l = 0; l < count2; l++)
		{
			Tuple<C1.Win.C1FlexGrid.Column, int> tuple = list[l];
			if (l == count2 - 1)
			{
				tuple.Item1.Width = Math.Max(num8, 100);
				continue;
			}
			int val2 = (int)((float)tuple.Item2 * 1f / (float)num5 * (float)num7);
			val2 = Math.Max(val2, 100);
			tuple.Item1.Width = val2;
			num8 -= val2;
		}
	}

	protected void CheckIsNeededDataInput(Auditai.Model.Table table, Ledger ledger)
	{
		Tuple<DateTime, DateTime> auditYear = DictionarySync.GetAuditYear(table);
		if (auditYear == null)
		{
			throw new InvalidAuditYearException("未在标题区中发现会计截止日或会计期间信息，请在标题区中输入会计截止日或会计期间信息后，再进行采账填充操作！");
		}
	}

	public void LoadCollectSetting(string settingExp)
	{
		Ledger ledger = Collector.Setting.Ledger;
		Auditai.Model.Table table = Collector.Setting.Table;
		if (string.IsNullOrWhiteSpace(settingExp))
		{
			Collector = TableCollectorAbstract.Intelligence(ledger, table);
		}
		else
		{
			Collector = TableCollectorAbstract.Deserialize(settingExp, ledger, table);
			Collector.IntelligenceShouldSelectedAccounts(ledger, table);
			Collector.IntelligenceShouldSelectedDetailAccount(ledger, table);
		}
		Collector = Collector ?? new TableCollectorBalance(ledger, table);
		InitTableColumnMappingData();
		ReInitComponentValue();
		ResetMappingItemContextMenu();
		ReCollectData();
		RefreshTableView(isAdjustColAutoWidth: true);
		UpdateLedgerMaxLevel();
		ReInitShouldSelectedItemByFilter();
		UnSelectEmptyAccount();
	}

	public void InitBalanceCollectByAccount(string settingExp, Account account, HashSet<Account> shouldSelectedAccount)
	{
		Ledger ledger = Collector.Setting.Ledger;
		Auditai.Model.Table table = Collector.Setting.Table;
		Collector = new TableCollectorBalance(ledger, table);
		if (string.IsNullOrWhiteSpace(settingExp))
		{
			TableCollectorAbstract tableCollectorAbstract = TableCollectorAbstract.Intelligence(ledger, table);
			if (tableCollectorAbstract.CollectObject == CollectObjectEnum.Balance)
			{
				Collector.Maps = tableCollectorAbstract.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract.Setting.End;
		}
		else
		{
			TableCollectorAbstract tableCollectorAbstract2 = TableCollectorAbstract.Deserialize(settingExp, ledger, table);
			if (tableCollectorAbstract2.CollectObject == CollectObjectEnum.Balance)
			{
				Collector.Maps = tableCollectorAbstract2.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract2.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract2.Setting.End;
		}
		Collector.Setting.Account = account;
		Collector.Setting.CollectAllAccount = DictionarySync.TableCollector.IntellegenceIsNeedSelectAllAccount(table.TreeNode.Name);
		Collector.Setting.IsOnlyMyMark = false;
		_isDisableCollectObjectCombox = true;
		if (!Collector.Setting.CollectAllAccount && shouldSelectedAccount != null)
		{
			Collector.Setting.CheckCollectItemShouldBeSelectedFilter = new CollectItemShouldSelectFilter(shouldSelectedAccount);
		}
		InitTableColumnMappingData();
		ReInitComponentValue();
		ResetMappingItemContextMenu();
		ReCollectData();
		RefreshTableView(isAdjustColAutoWidth: true);
		UpdateLedgerMaxLevel();
		ReInitShouldSelectedItemByFilter();
		UnSelectEmptyAccount();
	}

	public void InitSubsidiaryCollectByAccount(string settingExp, Account account, List<Voucher> selectedVouchersList)
	{
		Ledger ledger = Collector.Setting.Ledger;
		Auditai.Model.Table table = Collector.Setting.Table;
		Collector = new TableCollectorSubsidiary(ledger, table);
		if (string.IsNullOrWhiteSpace(settingExp))
		{
			TableCollectorAbstract tableCollectorAbstract = TableCollectorAbstract.Intelligence(ledger, table);
			if (tableCollectorAbstract.CollectObject == CollectObjectEnum.Subsidiary)
			{
				Collector.Maps = tableCollectorAbstract.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract.Setting.End;
		}
		else
		{
			TableCollectorAbstract tableCollectorAbstract2 = TableCollectorAbstract.Deserialize(settingExp, ledger, table);
			if (tableCollectorAbstract2.CollectObject == CollectObjectEnum.Subsidiary)
			{
				Collector.Maps = tableCollectorAbstract2.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract2.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract2.Setting.End;
		}
		Collector.Setting.Account = account;
		Collector.Setting.CollectAllAccount = account == null;
		Collector.Setting.IsOnlyMyMark = false;
		_isDisableCollectObjectCombox = true;
		if (selectedVouchersList != null)
		{
			Collector.Setting.CheckCollectItemShouldBeSelectedFilter = new CollectItemShouldSelectFilter(selectedVouchersList);
		}
		InitTableColumnMappingData();
		ReInitComponentValue();
		ResetMappingItemContextMenu();
		ReCollectData();
		RefreshTableView(isAdjustColAutoWidth: true);
		UpdateLedgerMaxLevel();
		ReInitShouldSelectedItemByFilter();
		UnSelectEmptyAccount();
	}

	public void InitSubsidiaryCollectByMyMark(string settingExp, Account account, object auxiliary, List<Voucher> selectedVouchersList)
	{
		PopulateAccountTreeByMyMark();
		Ledger ledger = Collector.Setting.Ledger;
		Auditai.Model.Table table = Collector.Setting.Table;
		Collector = new TableCollectorSubsidiary(ledger, table);
		if (string.IsNullOrWhiteSpace(settingExp))
		{
			TableCollectorAbstract tableCollectorAbstract = TableCollectorAbstract.Intelligence(ledger, table);
			if (tableCollectorAbstract.CollectObject == CollectObjectEnum.Subsidiary)
			{
				Collector.Maps = tableCollectorAbstract.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract.Setting.End;
		}
		else
		{
			TableCollectorAbstract tableCollectorAbstract2 = TableCollectorAbstract.Deserialize(settingExp, ledger, table);
			if (tableCollectorAbstract2.CollectObject == CollectObjectEnum.Subsidiary)
			{
				Collector.Maps = tableCollectorAbstract2.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract2.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract2.Setting.End;
		}
		Collector.Setting.Account = account;
		Collector.Setting.Auxiliary = auxiliary;
		Collector.Setting.CollectAllAccount = account == null && auxiliary == null;
		Collector.Setting.IsOnlyMyMark = true;
		_isDisableCollectObjectCombox = true;
		if (selectedVouchersList != null)
		{
			Collector.Setting.CheckCollectItemShouldBeSelectedFilter = new CollectItemShouldSelectFilter(selectedVouchersList);
		}
		InitTableColumnMappingData();
		ReInitComponentValue();
		ResetMappingItemContextMenu();
		ReCollectData();
		RefreshTableView(isAdjustColAutoWidth: true);
		UpdateLedgerMaxLevel();
		ReInitShouldSelectedItemByFilter();
		UnSelectEmptyAccount();
	}

	public void InitSummaryCollectByAccount(string settingExp, Account account, HashSet<Account> shouldSelectedAccount)
	{
		Ledger ledger = Collector.Setting.Ledger;
		Auditai.Model.Table table = Collector.Setting.Table;
		Collector = new TableCollectorSummary(ledger, table);
		if (string.IsNullOrWhiteSpace(settingExp))
		{
			TableCollectorAbstract tableCollectorAbstract = TableCollectorAbstract.Intelligence(ledger, table);
			if (tableCollectorAbstract.CollectObject == CollectObjectEnum.Summary)
			{
				Collector.Maps = tableCollectorAbstract.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract.Setting.End;
		}
		else
		{
			TableCollectorAbstract tableCollectorAbstract2 = TableCollectorAbstract.Deserialize(settingExp, ledger, table);
			if (tableCollectorAbstract2.CollectObject == CollectObjectEnum.Summary)
			{
				Collector.Maps = tableCollectorAbstract2.Maps;
			}
			Collector.Setting.Start = tableCollectorAbstract2.Setting.Start;
			Collector.Setting.End = tableCollectorAbstract2.Setting.End;
		}
		Collector.Setting.Account = account;
		Collector.Setting.CollectAllAccount = account == null;
		Collector.Setting.IsOnlyMyMark = false;
		_isDisableCollectObjectCombox = true;
		if (!Collector.Setting.CollectAllAccount && shouldSelectedAccount != null)
		{
			Collector.Setting.CheckCollectItemShouldBeSelectedFilter = new CollectItemShouldSelectFilter(shouldSelectedAccount);
		}
		InitTableColumnMappingData();
		ReInitComponentValue();
		ResetMappingItemContextMenu();
		ReCollectData();
		RefreshTableView(isAdjustColAutoWidth: true);
		UpdateLedgerMaxLevel();
		ReInitShouldSelectedItemByFilter();
		UnSelectEmptyAccount();
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentTree(this);
		return ShowDialog(owner);
	}

	private void Combox_OnCollectObjectChanged(object sender, EventArgs e)
	{
		pnlBalanceConditions.SuspendDrawing();
		try
		{
			PopulateAuxiliaryTree(selectedAccount());
			CollectObjectEnum collectObjectEnum = selectCollectObject();
			switch (collectObjectEnum)
			{
			case CollectObjectEnum.Subsidiary:
				lblKjqj.Visible = false;
				lblStartMonth.Visible = false;
				lblEndMonth.Visible = false;
				comboStartMonth.Visible = false;
				comboEndMonth.Visible = false;
				spbAnalysis.Visible = false;
				checkBoxAllAccount.Visible = false;
				checkBoxNotFilterAccount.Visible = true;
				checkBoxOnlyMyMark.Visible = true;
				comboAccountTree.Enabled = !checkBoxNotFilterAccount.Checked && !checkBoxOnlyMyMark.Checked;
				comboAuxiliaryTree.Enabled = !checkBoxNotFilterAccount.Checked && !checkBoxOnlyMyMark.Checked && comboAuxiliaryTree.TreeView.Nodes.Count > 0;
				break;
			case CollectObjectEnum.Balance:
				lblKjqj.Visible = true;
				lblStartMonth.Visible = true;
				lblEndMonth.Visible = true;
				comboStartMonth.Visible = true;
				comboEndMonth.Visible = true;
				spbAnalysis.Visible = false;
				checkBoxAllAccount.Visible = true;
				checkBoxNotFilterAccount.Visible = false;
				checkBoxOnlyMyMark.Visible = false;
				comboAccountTree.Enabled = !checkBoxAllAccount.Checked;
				comboAuxiliaryTree.Visible = false;
				break;
			case CollectObjectEnum.Summary:
				lblKjqj.Visible = true;
				lblStartMonth.Visible = true;
				lblEndMonth.Visible = true;
				comboStartMonth.Visible = true;
				comboEndMonth.Visible = true;
				spbAnalysis.Visible = true;
				checkBoxAllAccount.Visible = false;
				checkBoxNotFilterAccount.Visible = false;
				checkBoxOnlyMyMark.Visible = false;
				comboAccountTree.Enabled = true;
				comboAuxiliaryTree.Visible = false;
				break;
			}
			Setting setting = Collector.Setting;
			switch (collectObjectEnum)
			{
			case CollectObjectEnum.Balance:
				Collector = new TableCollectorBalance(setting.Ledger, setting.Table);
				SetVisibleAuxiliary(visble: false);
				break;
			case CollectObjectEnum.Subsidiary:
				Collector = new TableCollectorSubsidiary(setting.Ledger, setting.Table);
				SetVisibleAuxiliary(visble: true);
				break;
			case CollectObjectEnum.Summary:
				Collector = new TableCollectorSummary(setting.Ledger, setting.Table);
				SetVisibleAuxiliary(visble: false);
				break;
			}
			Collector.Setting.Start = setting.Start;
			Collector.Setting.End = setting.End;
			Collector.Setting.Account = setting.Account;
			Collector.Setting.Auxiliary = setting.Auxiliary;
			SelectAuxiliaryNode(Collector.Setting.Auxiliary);
			ReCollectData();
			ResetMappingItemContextMenu();
			RefreshTableView();
			UnSelectEmptyAccount();
		}
		finally
		{
			pnlBalanceConditions.ResumeDrawing();
		}
	}

	private void Combox_OnStartMonthChanged(object sender, EventArgs e)
	{
		int year = Collector.TitlePeriod.Item1.Year;
		int month = int.Parse(comboStartMonth.Text.Trim());
		Collector.Setting.Start = new DateTime(year, month, 1);
		ReCollectData();
		RefreshTableView();
		UpdateLedgerMaxLevel();
		UnSelectEmptyAccount();
	}

	private void Combox_OnEndMonthChanged(object sender, EventArgs e)
	{
		int year = Collector.TitlePeriod.Item1.Year;
		int month = int.Parse(comboEndMonth.Text.Trim());
		Collector.Setting.End = new DateTime(year, month, DateTime.DaysInMonth(year, month));
		ReCollectData();
		RefreshTableView();
		UpdateLedgerMaxLevel();
		UnSelectEmptyAccount();
	}

	private void ResetCurrentSelectedAccountNode()
	{
		if (comboAccountTree.Text == string.Empty)
		{
			_currentSelectedAccountNode = null;
		}
		else
		{
			_currentSelectedAccountNode = comboAccountTree.SelectedNode;
		}
	}

	private void Combox_OnAccountTreeChanged(object sender, TreeViewEventArgs e)
	{
		if (_currentSelectedAccountNode != e.Node)
		{
			_currentSelectedAccountNode = e.Node;
			PopulateAuxiliaryTree(selectedAccount());
			ReCollectData();
			RefreshTableView();
			UnSelectEmptyAccount();
			UnSelectEmptyAccount();
		}
	}

	private void Combox_OnAuxiliaryTree_SelectNodeChanged(object sender, TreeViewEventArgs e)
	{
		ReCollectData();
		RefreshTableView();
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		Collector.Maps = new Dictionary<long, string>();
		TableCollectResult tableCollectResult = new TableCollectResult(Collector);
		bool flag = !Collector.Setting.CollectAllAccount;
		for (int i = 4; i < _tableHeaderColumnsCount; i++)
		{
			TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, i);
			TableColumnMappingData columnMappingData = tableHeaderSetting.ColumnMappingData;
			if (columnMappingData == null || !columnMappingData.IsMappingItemSet)
			{
				continue;
			}
			if (!string.IsNullOrWhiteSpace(columnMappingData.MappingItemName))
			{
				Collector.Maps.Add(columnMappingData.TableColumnId.Value, columnMappingData.MappingItemName);
			}
			if (_tableDataSourceColumnNameTable == null || _has_no_data_in_ledger_columns_index_set.Contains(columnMappingData.DataTableColIndex))
			{
				continue;
			}
			List<object> list = new List<object>();
			int num = _dataGrid.Rows.Count - _tableHeaderRowsCount;
			for (int j = 0; j < num; j++)
			{
				int num2 = j + _tableHeaderRowsCount;
				C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[num2];
				if (row.IsVisible && (!flag || _dataGrid.GetCellCheck(num2, 1) == CheckEnum.Checked))
				{
					DataGridRowUserData dataGridRowUserData = (DataGridRowUserData)row.UserData;
					object obj = dataGridRowUserData.DataSource[columnMappingData.DataTableColIndex];
					if (columnMappingData.IsUsePositiveValue && obj != null && obj is decimal num3 && num3 < 0m)
					{
						obj = null;
					}
					list.Add(Auditai.Model.Cell.ConvertInputValueToCellValue(obj));
				}
			}
			tableCollectResult.Values.Add(columnMappingData.TableColumn, list);
		}
		Result = tableCollectResult;
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void btnCancle_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private async void btnIntelligenceFill_Click(object sender, EventArgs e)
	{
		if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
		{
			try
			{
				await DictionarySync.CheckTableCollectVersionAndUpdate();
			}
			catch (WebException)
			{
				if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
				{
					System.Windows.Forms.MessageBox.Show("因网络问题，字典更新失败！");
				}
			}
			catch (TimeoutException)
			{
				if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
				{
					MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
				}
			}
			catch (Exception ex3)
			{
				if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && DictionarySync.TableCollector.Version == 0)
				{
					MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex3.Message + ",请重试");
				}
			}
		}
		Ledger ledger = Collector.Setting.Ledger;
		Auditai.Model.Table table = Collector.Setting.Table;
		Collector = TableCollectorAbstract.Intelligence(ledger, table);
		Collector = Collector ?? new TableCollectorBalance(ledger, table);
		InitTableColumnMappingData();
		ReInitComponentValue();
		ResetMappingItemContextMenu();
		ReCollectData();
		RefreshTableView();
		ReInitShouldSelectedItemByFilter();
		UnSelectEmptyAccount();
		base.ActiveControl = null;
	}

	private void SpbAnalysis_ItemClick(object sender, object e)
	{
		AnalysisProject analysisProject = (AnalysisProject)e;
		(Collector as TableCollectorSummary).AnalysisProject = analysisProject;
		List<object> list = null;
		int count = _dataGrid.Rows.Count;
		if (count > _tableHeaderRowsCount)
		{
			list = new List<object>();
			for (int i = _tableHeaderRowsCount; i < count; i++)
			{
				list.Add(((DataGridRowUserData)_dataGrid.Rows[i].UserData).RawData);
			}
		}
		ReCollectData(list);
		RefreshTableView();
		UnSelectEmptyAccount();
	}

	private void Initialize(Ledger ledger, Auditai.Model.Table table)
	{
		InitializeTitle(table);
		InitDataTable(table);
		InitContextMenu();
		CheckIsNeededDataInput(table, ledger);
		_change_banlance_level_collector = new TableCollectorBalance(ledger, table);
		_change_summary_level_collector = new TableCollectorSummary(ledger, table);
		Collector = new TableCollectorBalance(ledger, table);
		spbAnalysis.Initialize();
		spbAnalysis.AddItem("借方发生额", AnalysisProject.Debits);
		spbAnalysis.AddItem("贷方发生额", AnalysisProject.Credits);
		spbAnalysis.FinishAdd();
		spbAnalysis.Visible = false;
		comboCollectObject.SelectedIndex = 0;
		comboStartMonth.Text = "1";
		comboEndMonth.Text = "12";
		PopulateAccountTree(ledger);
		comboAuxiliaryTree.Enabled = false;
		comboAuxiliaryTree.TreeView.KeyDown += ComboAuxiliaryTree_KeyDown;
		comboAuxiliaryTree.dropDown.KeyDown += ComboAuxiliaryTree_KeyDown;
		checkBoxOnlyMyMark.Visible = false;
		checkBoxNotFilterAccount.Visible = false;
	}

	private void ComboAuxiliaryTree_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyData = e.KeyData;
		if (keyData == Keys.Back || keyData == Keys.Delete)
		{
			SelectAuxiliaryNode(null);
		}
	}

	private void CheckBox_AllAccountCheckStatusChanged(object sender, EventArgs e)
	{
		comboAccountTree.Enabled = !checkBoxAllAccount.Checked;
		ReCollectData();
		RefreshTableView();
		UnSelectEmptyAccount();
	}

	private void CheckBox_NotFilterAccountCheckStatusChanged(object sender, EventArgs e)
	{
		if (_isSuspendNotFilterAccountCheckEvent)
		{
			return;
		}
		pnlBalanceConditions.SuspendDrawing();
		try
		{
			bool enabled = true;
			if (comboAuxiliaryTree.TreeView.Nodes.Count == 0 || checkBoxNotFilterAccount.Checked)
			{
				enabled = false;
			}
			comboAuxiliaryTree.Enabled = enabled;
			comboAccountTree.Enabled = !checkBoxNotFilterAccount.Checked;
			if (checkBoxNotFilterAccount.Checked)
			{
				CancelColumnMapSettingByMappingName(SubsidiaryExcludeItemOnAllAccountSelected);
			}
		}
		finally
		{
			pnlBalanceConditions.ResumeDrawing();
		}
		ReCollectData();
		RefreshTableView();
		UnSelectEmptyAccount();
	}

	private void CheckBox_OnlyMyMarkCheckStatusChanged(object sender, EventArgs e)
	{
		pnlBalanceConditions.SuspendDrawing();
		try
		{
			if (checkBoxOnlyMyMark.Checked)
			{
				CancelColumnMapSettingByMappingName(SubsidiaryExcludeItemOnAllAccountSelected);
				Account account = selectedAccount();
				bool flag = false;
				PopulateAccountTreeByMyMark();
				if (account != null)
				{
					flag = _myMarkAccountDataSet.ContainsKey(account);
				}
				SelectAccountNode(flag ? account : null);
				SelectAuxiliaryNode(null);
				comboAccountTree.Enabled = !checkBoxNotFilterAccount.Checked;
			}
			else
			{
				Account account2 = selectedAccount();
				PopulateAccountTree(_ledger);
				SelectAccountNode(account2);
				comboAccountTree.Enabled = !checkBoxNotFilterAccount.Checked;
			}
		}
		finally
		{
			pnlBalanceConditions.ResumeDrawing();
		}
		ReCollectData();
		RefreshTableView();
		UnSelectEmptyAccount();
	}

	private void CancelColumnMapSettingByMappingName(string[] namesArr)
	{
		_dataGrid.BeginUpdate();
		try
		{
			int count = _dataGrid.Cols.Count;
			for (int i = 4; i < count; i++)
			{
				TableColumnMappingData columnMappingData = GetTableHeaderSetting(_tableHeaderRowsCount - 1, i).ColumnMappingData;
				if (columnMappingData != null && columnMappingData.IsMappingItemSet && namesArr.Contains(columnMappingData.MappingItemName))
				{
					int columnIndex = columnMappingData.ColumnIndex;
					columnMappingData.IsMappingItemSet = false;
					columnMappingData.MappingItemName = null;
					ChangeDataTableColumnData(columnIndex);
					CancelSort(columnIndex);
				}
			}
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private C1FlexGrid GetTableHeaderGrid(Auditai.Model.Table table)
	{
		if (table is TicketCollectFillTable ticketCollectFillTable)
		{
			C1FlexGrid c1FlexGrid = new C1FlexGrid();
			if (!(ticketCollectFillTable.CollectPageTableHeaderData is C1FlexGrid c1FlexGrid2))
			{
				c1FlexGrid.Rows.Count = 1;
				c1FlexGrid.Cols.Count = 0;
				return c1FlexGrid;
			}
			int count = c1FlexGrid2.Rows.Count;
			int count2 = c1FlexGrid2.Cols.Count;
			c1FlexGrid.Rows.Count = count;
			c1FlexGrid.Cols.Count = count2;
			for (int i = 0; i < count; i++)
			{
				c1FlexGrid.Rows[i].Height = c1FlexGrid2.Rows[i].Height;
				for (int j = 0; j < count2; j++)
				{
					c1FlexGrid[i, j] = c1FlexGrid2[i, j];
				}
			}
			for (int k = 0; k < count2; k++)
			{
				c1FlexGrid.Cols[k].Width = c1FlexGrid2.Cols[k].Width;
			}
			{
				foreach (C1.Win.C1FlexGrid.CellRange item in (IEnumerable)c1FlexGrid2.MergedRanges)
				{
					c1FlexGrid.MergedRanges.Add(item);
				}
				return c1FlexGrid;
			}
		}
		C1FlexGrid c1FlexGrid3 = new C1FlexGrid();
		int numCaptionRows = table.GetNumCaptionRows();
		int count3 = table.Columns.Count;
		c1FlexGrid3.Rows.Count = numCaptionRows;
		c1FlexGrid3.Cols.Count = count3;
		for (int l = 0; l < count3; l++)
		{
			Auditai.Model.Column column = table.Columns[l];
			C1.Win.C1FlexGrid.Column column2 = c1FlexGrid3.Cols[l];
			column2.Visible = column.Visible;
			column2.Visible = column.Visible;
			string[] array = column.CaptionDisplay.Split('_');
			for (int m = 0; m < array.Length; m++)
			{
				c1FlexGrid3.SetData(m, l, array[m]);
			}
		}
		List<Auditai.DTO.CellRange> mergeInfo = table.GetMergeInfo(visibleOnly: false);
		foreach (Auditai.DTO.CellRange item2 in mergeInfo)
		{
			c1FlexGrid3.MergedRanges.Add(item2.r1, item2.c1, item2.r2, item2.c2);
		}
		return c1FlexGrid3;
	}

	private void InitDataTable(Auditai.Model.Table table)
	{
		C1FlexGridEx c1FlexGridEx2 = (DataView = new C1FlexGridEx());
		panelDataTable.Controls.Add(c1FlexGridEx2);
		panelDataTable.BringToFront();
		c1FlexGridEx2.BringToFront();
		c1FlexGridEx2.FilterManager.Context = new DataGridFilterContext(this);
		c1FlexGridEx2.Cols.Count = 0;
		c1FlexGridEx2.Cols.Fixed = 0;
		c1FlexGridEx2.Dock = DockStyle.None;
		c1FlexGridEx2.Rows.DefaultSize = 30;
		c1FlexGridEx2.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		c1FlexGridEx2.AllowSorting = AllowSortingEnum.None;
		c1FlexGridEx2.AllowMerging = AllowMergingEnum.Custom;
		c1FlexGridEx2.AllowResizing = AllowResizingEnum.Columns;
		c1FlexGridEx2.AllowDragging = AllowDraggingEnum.None;
		c1FlexGridEx2.AllowEditing = true;
		c1FlexGridEx2.DrawMode = DrawModeEnum.OwnerDraw;
		c1FlexGridEx2.OwnerDrawCell += Grid_OwnerDrawCell;
		c1FlexGridEx2.MouseMove += Grid_MouseMove;
		c1FlexGridEx2.MouseClick += Grid_MouseClick;
		c1FlexGridEx2.Font = _dataGridCellFont;
		c1FlexGridEx2.BeforeResizeColumn += Grid_BeforeResizeColumn;
		c1FlexGridEx2.AfterResizeColumn += Grid_AfterResizeColumn;
		c1FlexGridEx2.DoubleClick += Grid_DoubleClick;
		c1FlexGridEx2.KeyDown += Grid_KeyDown;
		C1.Win.C1FlexGrid.CellStyle cellStyle = c1FlexGridEx2.Styles.Add("checkColStyle");
		cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
		C1FlexGrid tableHeaderGrid = GetTableHeaderGrid(table);
		c1FlexGridEx2.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Column column = c1FlexGridEx2.Cols.Add();
			column.Caption = "序号";
			column.DataType = typeof(int);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 50;
			c1FlexGridEx2.Cols.Fixed = 1;
			_columnsMinWidthList.Add(column.Width);
			column = c1FlexGridEx2.Cols.Add();
			column.DataType = typeof(bool);
			column.Style = cellStyle;
			column.Width = 50;
			column.Visible = false;
			_columnsMinWidthList.Add(column.Width);
			column = c1FlexGridEx2.Cols.Add();
			column.Caption = "调整级次";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 100;
			_columnsMinWidthList.Add(column.Width);
			column = c1FlexGridEx2.Cols.Add();
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.LeftCenter;
			column.Width = 100;
			_columnsMinWidthList.Add(column.Width);
			int count = tableHeaderGrid.Rows.Count;
			int count2 = tableHeaderGrid.Cols.Count;
			c1FlexGridEx2.Rows.Count = count;
			c1FlexGridEx2.Rows.Fixed = count;
			c1FlexGridEx2.Cols.Add(count2);
			for (int i = 0; i < count; i++)
			{
				C1.Win.C1FlexGrid.CellStyle styleNew = c1FlexGridEx2.Rows[i].StyleNew;
				styleNew.WordWrap = true;
				styleNew.Font = _columHeaderCaptionFont;
				int[] array = new int[count2];
				int num = 0;
				for (int j = 0; j < count2; j++)
				{
					C1.Win.C1FlexGrid.Column column2 = tableHeaderGrid.Cols[j];
					column = c1FlexGridEx2.Cols[j + 4];
					column.Visible = column2.Visible;
					array[i] = (column2.Visible ? Math.Max(column2.Width, 100) : 0);
					column.Width = array[i];
					num += array[i];
					_tableColumRawWithList.Add(array[i]);
					c1FlexGridEx2.SetData(i, j + 4, tableHeaderGrid[i, j]);
					_columnsMinWidthList.Add(100);
				}
				_tableRawTotalWidth = num;
			}
			foreach (C1.Win.C1FlexGrid.CellRange item in (IEnumerable)tableHeaderGrid.MergedRanges)
			{
				c1FlexGridEx2.MergedRanges.Add(item.r1, item.c1 + 4, item.r2, item.c2 + 4);
			}
			if (count > 1)
			{
				for (int k = 0; k < 4; k++)
				{
					c1FlexGridEx2.MergedRanges.Add(0, k, count - 1, k);
				}
			}
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = _dataGrid.GetCellStyle(0, 0);
			if (cellStyle2 == null)
			{
				cellStyle2 = _dataGrid.Styles.Fixed.Clone();
			}
			cellStyle2.Font = _columHeaderCaptionFont;
			_dataGrid.SetCellStyle(0, 0, cellStyle2);
			_tableHeaderRowsCount = count;
			_tableHeaderColumnsCount = count2 + 4;
			for (int l = 0; l < _tableHeaderRowsCount; l++)
			{
				for (int m = 0; m < _tableHeaderColumnsCount; m++)
				{
					TableHeaderCellSetting tableHeaderCellSetting = new TableHeaderCellSetting();
					tableHeaderCellSetting.ColumnIndex = m;
					tableHeaderCellSetting.RowIndex = l;
					int num2 = c1FlexGridEx2.MergedRanges.IndexOf(l, m);
					tableHeaderCellSetting.CellAreaRange = ((num2 == -1) ? c1FlexGridEx2.GetCellRange(l, m) : c1FlexGridEx2.MergedRanges[num2]);
					tableHeaderCellSetting.IsMergeRange = !tableHeaderCellSetting.CellAreaRange.IsSingleCell;
					if (tableHeaderCellSetting.IsMergeRange)
					{
						tableHeaderCellSetting.IsMergeRangeLeftTopCell = l == tableHeaderCellSetting.CellAreaRange.TopRow && m == tableHeaderCellSetting.CellAreaRange.LeftCol;
						if (tableHeaderCellSetting.IsMergeRangeLeftTopCell)
						{
							tableHeaderCellSetting.CaptionDisplaySetting = new TableHeaderCellCaptionDisplaySetting();
							tableHeaderCellSetting.CaptionDisplaySetting.CaptionText = c1FlexGridEx2.GetDataDisplay(l, m);
						}
						else
						{
							tableHeaderCellSetting.CaptionDisplaySetting = GetTableHeaderSetting(tableHeaderCellSetting.CellAreaRange.TopRow, tableHeaderCellSetting.CellAreaRange.LeftCol).CaptionDisplaySetting;
						}
					}
					else
					{
						tableHeaderCellSetting.CaptionDisplaySetting = new TableHeaderCellCaptionDisplaySetting();
						tableHeaderCellSetting.CaptionDisplaySetting.CaptionText = c1FlexGridEx2.GetDataDisplay(l, m);
					}
					_tableHeaderSettingList.Add(tableHeaderCellSetting);
				}
			}
			for (int n = 0; n < count2; n++)
			{
				int num3 = count - 1;
				int num4 = 4 + n;
				int num5 = c1FlexGridEx2.MergedRanges.IndexOf(num3, num4);
				TableColumnMappingData tableColumnMappingData = new TableColumnMappingData();
				tableColumnMappingData.ColumnIndex = num4;
				tableColumnMappingData.RowIndex = num3;
				TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(num3, num4);
				tableHeaderSetting.IsMappingItemCell = true;
				tableHeaderSetting.ColumnMappingData = tableColumnMappingData;
				tableHeaderSetting.ColumnMappingDisplaySetting = new TableHeaderCellColumnMappingDisplaySetting();
				tableHeaderSetting.ColumnMappingDisplaySetting.CellAreaRange = c1FlexGridEx2.GetCellRange(num3, num4);
				if (tableHeaderSetting.CaptionDisplaySetting.ColumnMappingItemList == null)
				{
					tableHeaderSetting.CaptionDisplaySetting.ColumnMappingItemList = new List<TableHeaderCellSetting>();
				}
				tableHeaderSetting.CaptionDisplaySetting.ColumnMappingItemList.Add(tableHeaderSetting);
				Auditai.Model.Column tableColumn = GetTableColumn(table, n);
				_balanceMapSettingData.Add(new TableColumnMappingData(num3, num4, tableColumn));
				_monthSummaryMapSettingData.Add(new TableColumnMappingData(num3, num4, tableColumn));
				_ledgerAgeMappSettingData.Add(new TableColumnMappingData(num3, num4, tableColumn));
				_subsidiaryMapSettingData.Add(new TableColumnMappingData(num3, num4, tableColumn));
			}
			for (int num6 = 0; num6 < _tableHeaderRowsCount; num6++)
			{
				c1FlexGridEx2.SetCellStyle(num6, 1, cellStyle);
				c1FlexGridEx2.SetCellCheck(num6, 1, CheckEnum.Checked);
			}
			for (int num7 = 0; num7 < _tableHeaderColumnsCount; num7++)
			{
				c1FlexGridEx2.Cols[num7].AllowEditing = num7 == 1;
			}
		}
		finally
		{
			c1FlexGridEx2.EndUpdate();
		}
		c1FlexGridEx2.BeforeMouseDown += Grid_BeforeMouseDown;
		AttachDataGridCellCheckEvent();
	}

	private void Grid_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyData = e.KeyData;
		if (keyData == (Keys.C | Keys.Control))
		{
			Copy();
		}
	}

	public void Copy()
	{
		C1.Win.C1FlexGrid.CellRange selection = _dataGrid.Selection;
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			int num = 0;
			int num2 = _dataGrid.BodySelection.BottomRow - _dataGrid.BodySelection.TopRow + 1;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				if (i < _dataGrid.Rows.Fixed)
				{
					continue;
				}
				C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[i];
				if (!row.Visible)
				{
					continue;
				}
				for (int j = selection.LeftCol; j <= selection.RightCol; j++)
				{
					C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[j];
					if (column.Visible)
					{
						string text = null;
						text = ((j == 1) ? ((_dataGrid.GetCellCheck(i, j) != CheckEnum.Checked) ? "" : "√") : ((j >= 4) ? _dataGrid.GetDataDisplay(i, j) : ""));
						text = ((text != null) ? text.Trim().Replace("\r", "").Replace("\n", "")
							.Replace("\t", "") : "");
						stringBuilder.Append(text);
						if (j < selection.RightCol)
						{
							stringBuilder.Append("\t");
						}
					}
				}
				if (num2 > 1)
				{
					stringBuilder.Append("\r\n");
				}
				num++;
			}
			if (LicenseCheckHandleOnCopyLedgerData != null && !LicenseCheckHandleOnCopyLedgerData(num))
			{
				return;
			}
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		string value = stringBuilder.ToString();
		if (!string.IsNullOrWhiteSpace(value))
		{
			Clipboard.SetText(value);
		}
	}

	private Auditai.Model.Column GetTableColumn(Auditai.Model.Table table, int colIndex)
	{
		if (table is TicketCollectFillTable ticketCollectFillTable)
		{
			return ticketCollectFillTable.GetColumnByIndex(colIndex);
		}
		return table.Columns[colIndex];
	}

	private void Grid_CellChecked(object sender, RowColEventArgs e)
	{
		if (e.Col == 1 && e.Row >= _tableHeaderRowsCount)
		{
			_dataGrid.Rows[e.Row].StyleNew.BackColor = ((_dataGrid.GetCellCheck(e.Row, e.Col) == CheckEnum.Checked) ? DefaultBackgroundColor : HighLightColor);
		}
	}

	private void Grid_DoubleClick(object sender, EventArgs e)
	{
		if (Control.MouseButtons == MouseButtons.Right || Collector.CollectObject != CollectObjectEnum.Subsidiary)
		{
			return;
		}
		HitTestInfo hitTestInfo = _dataGrid.HitTest();
		if (hitTestInfo.Type != HitTestTypeEnum.ColumnHeader || hitTestInfo.Row >= _tableHeaderRowsCount || hitTestInfo.Column <= 3)
		{
			return;
		}
		TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, hitTestInfo.Column);
		if (tableHeaderSetting.ColumnMappingData != null && tableHeaderSetting.ColumnMappingData.IsMappingItemSet && (tableHeaderSetting.ColumnMappingData.MappingItemName == "借方金额" || tableHeaderSetting.ColumnMappingData.MappingItemName == "贷方金额" || tableHeaderSetting.ColumnMappingData.MappingItemName == "余额"))
		{
			C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[hitTestInfo.Column];
			if (column.Sort == SortFlags.None)
			{
				column.Sort = SortFlags.Descending;
			}
			else if (column.Sort == SortFlags.Descending)
			{
				column.Sort = SortFlags.Ascending;
			}
			else if (column.Sort == SortFlags.Ascending)
			{
				column.Sort = SortFlags.None;
				_dataGrid.Sort(SortFlags.Ascending, 0);
				_dataGrid.Sort(SortFlags.None, 0);
				return;
			}
			_dataGrid.Sort(SortFlags.UseColSort, column.Index);
		}
	}

	private void Grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		HitTestInfo hitInfo = _dataGrid.HitTest(e.X, e.Y);
		if (hitInfo.Type == HitTestTypeEnum.Cell && hitInfo.Column == 2)
		{
			int row = hitInfo.Row;
			int column = hitInfo.Column;
			Rectangle cellRect = _dataGrid.GetCellRect(row, column);
			GetAdjustLevelCellIconPosition(row, column, cellRect, out var menuIcon, out var upIcon, out var dwonIcon);
			if (menuIcon.Contains(hitInfo.Point))
			{
				Icon_OnMenuIconClicked(hitInfo.Row, hitInfo.Column, hitInfo.Point);
				e.Cancel = true;
			}
			else if (upIcon.Contains(hitInfo.Point))
			{
				Icon_OnUpLevelIconClicked(hitInfo.Row, hitInfo.Column);
				e.Cancel = true;
			}
			else if (dwonIcon.Contains(hitInfo.Point))
			{
				Icon_OnDownLevelIconClicked(hitInfo.Row, hitInfo.Column);
				e.Cancel = true;
			}
		}
		else if (hitInfo.Type == HitTestTypeEnum.ColumnHeader && _ctxMappingItemType != CollectObjectEnum.None && IsMouseOverMappingItemText(hitInfo))
		{
			_currentSelectedColumnMappingData = GetTableHeaderSetting(hitInfo.Row, hitInfo.Column).ColumnMappingData;
			foreach (object commandLink in _ctxMappingMenu.CommandLinks)
			{
				C1CommandLink c1CommandLink = (C1CommandLink)commandLink;
				if (!_currentSelectedColumnMappingData.IsMappingItemSet)
				{
					c1CommandLink.Command.Checked = c1CommandLink.Command.UserData as string == "空";
					continue;
				}
				string text = c1CommandLink.Command.UserData as string;
				c1CommandLink.Command.Checked = text == _currentSelectedColumnMappingData.MappingItemName;
				c1CommandLink.Command.Text = ConvertMappingItemNameToDisplayName(text, 0, 0);
			}
			_ctxMappingMenu.ShowContextMenu(_dataGrid, hitInfo.Point);
			e.Cancel = true;
		}
		else
		{
			if (hitInfo.Column != 1)
			{
				return;
			}
			C1.Win.C1FlexGrid.CellStyle cellStyleDisplay = _dataGrid.GetCellStyleDisplay(hitInfo.Row, hitInfo.Column);
			Rectangle cellRect2 = _dataGrid.GetCellRect(hitInfo.Row, hitInfo.Column);
			if (cellStyleDisplay.GetImageRectangle(cellRect2, _dataGrid.Glyphs[GlyphEnum.Checked]).Contains(hitInfo.Point))
			{
				e.Cancel = true;
				DettachDataGridCellCheckEvent();
				bool isSelect = _dataGrid.GetCellCheck(hitInfo.Row, hitInfo.Column) != CheckEnum.Checked;
				if (hitInfo.Row < _tableHeaderRowsCount)
				{
					SelectAllRows(hitInfo.Row, hitInfo.Column, isSelect);
				}
				else if (_dataGrid.Selection.Contains(hitInfo.Row, hitInfo.Column))
				{
					SelectRows(_dataGrid.Selection, hitInfo.Column, isSelect);
				}
				else
				{
					SelectRow(hitInfo.Row, hitInfo.Column, isSelect);
				}
				AttachDataGridCellCheckEvent();
			}
		}
	}

	private void DettachDataGridCellCheckEvent()
	{
		_dataGrid.CellChecked -= Grid_CellChecked;
	}

	private void AttachDataGridCellCheckEvent()
	{
		_dataGrid.CellChecked += Grid_CellChecked;
	}

	private void Grid_BeforeResizeColumn(object sender, RowColEventArgs e)
	{
		_isInResizingColumn = true;
	}

	private void Grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (_isInResizingColumn || e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTestInfo = _dataGrid.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			if (hitTestInfo.Row >= _dataGrid.Rows.Fixed && hitTestInfo.Column >= 3)
			{
				Mouse_OnCellRightClicked(hitTestInfo.Row, hitTestInfo.Column, hitTestInfo.Point);
			}
		}
		else if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader && Collector.CollectObject == CollectObjectEnum.Subsidiary && hitTestInfo.Row < _tableHeaderRowsCount && hitTestInfo.Column > 3)
		{
			TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, hitTestInfo.Column);
			if (tableHeaderSetting.ColumnMappingData != null && tableHeaderSetting.ColumnMappingData.IsMappingItemSet && (tableHeaderSetting.ColumnMappingData.MappingItemName == "借方金额" || tableHeaderSetting.ColumnMappingData.MappingItemName == "贷方金额" || tableHeaderSetting.ColumnMappingData.MappingItemName == "余额"))
			{
				_ctxColSort.ShowContextMenu(_dataGrid, e.Location);
			}
		}
	}

	private void Grid_MouseMove(object sender, MouseEventArgs e)
	{
		MosueOverCellInfo mouseCurrentOverWhichAdjustLevelCell = _mouseCurrentOverWhichAdjustLevelCell;
		_mouseCurrentOverWhichAdjustLevelCell = null;
		if ((Control.MouseButtons & (MouseButtons.Left | MouseButtons.Right)) == 0)
		{
			HitTestInfo hitInfo = _dataGrid.HitTest();
			switch (hitInfo.Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				_dataGrid.Cursor = (IsMouseOverMappingItemText(hitInfo) ? Cursors.Hand : Cursors.Default);
				break;
			case HitTestTypeEnum.Cell:
				OnMouseOverCell(hitInfo.Row, hitInfo.Column, e);
				_dataGrid.Cursor = Cursors.Default;
				break;
			default:
				_dataGrid.Cursor = Cursors.Default;
				break;
			case HitTestTypeEnum.ColumnResize:
			case HitTestTypeEnum.RowResize:
				break;
			}
			RepaintOldMouseOverCell(mouseCurrentOverWhichAdjustLevelCell, _mouseCurrentOverWhichAdjustLevelCell);
		}
		else
		{
			RepaintOldMouseOverCell(mouseCurrentOverWhichAdjustLevelCell, _mouseCurrentOverWhichAdjustLevelCell);
		}
		void OnMouseOverCell(int row, int col, MouseEventArgs e)
		{
			if (row >= _dataGrid.Rows.Fixed && col == 2)
			{
				_mouseCurrentOverWhichAdjustLevelCell = new MosueOverCellInfo(row, col);
				_mouseCurrentOverWhichAdjustLevelCell.MouseLocation = e.Location;
				_dataGrid.Invalidate(_dataGrid.GetCellRect(row, col));
			}
		}
		void RepaintOldMouseOverCell(MosueOverCellInfo oldCell, MosueOverCellInfo newCell)
		{
			if (oldCell != null && (newCell == null || newCell.RowIndex != oldCell.RowIndex || newCell.ColIndex != oldCell.ColIndex))
			{
				_dataGrid.Invalidate(_dataGrid.GetCellRect(oldCell.RowIndex, oldCell.ColIndex));
			}
		}
	}

	private bool IsMouseOverMappingItemText(HitTestInfo hitInfo)
	{
		TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(hitInfo.Row, hitInfo.Column);
		if (tableHeaderSetting == null || tableHeaderSetting.ColumnMappingData == null || tableHeaderSetting.ColumnMappingDisplaySetting == null)
		{
			return false;
		}
		return tableHeaderSetting.ColumnMappingDisplaySetting.MappingItemTextDrawRectInGrid.Contains(hitInfo.Point);
	}

	private void Grid_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		_isInResizingColumn = false;
		_dataGrid.BeginUpdate();
		try
		{
			AutoAdjustTableHeaderDisplaySetting();
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void SelectAllRows(int row, int col, bool isSelect)
	{
		int count = _dataGrid.Rows.Count;
		if (count == 0)
		{
			return;
		}
		_dataGrid.BeginUpdate();
		try
		{
			Color backColor = (isSelect ? DefaultBackgroundColor : HighLightColor);
			CheckEnum check = (isSelect ? CheckEnum.Checked : CheckEnum.Unchecked);
			for (int i = 0; i < _tableHeaderRowsCount; i++)
			{
				_dataGrid.SetCellCheck(i, col, check);
			}
			for (int j = _tableHeaderRowsCount; j < count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = _dataGrid.Rows[j];
				_dataGrid.SetCellCheck(j, col, check);
				row2.StyleNew.BackColor = backColor;
			}
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void SelectRow(int row, int col, bool isSelect)
	{
		C1.Win.C1FlexGrid.Row row2 = _dataGrid.Rows[row];
		_dataGrid.SetCellCheck(row, col, isSelect ? CheckEnum.Checked : CheckEnum.Unchecked);
		row2.StyleNew.BackColor = (isSelect ? DefaultBackgroundColor : HighLightColor);
		_dataGrid.Invalidate();
	}

	private void SelectRows(C1.Win.C1FlexGrid.CellRange range, int col, bool isSelect)
	{
		if (range.BottomRow < range.TopRow)
		{
			return;
		}
		_dataGrid.BeginUpdate();
		try
		{
			Color backColor = (isSelect ? DefaultBackgroundColor : HighLightColor);
			CheckEnum check = (isSelect ? CheckEnum.Checked : CheckEnum.Unchecked);
			for (int i = range.TopRow; i <= range.BottomRow; i++)
			{
				C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[i];
				_dataGrid.SetCellCheck(i, col, check);
				row.StyleNew.BackColor = backColor;
			}
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void AdjustTableColumnWidthToDisplayMappingItemNameInSingleRow(int colIndex)
	{
		C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[colIndex];
		int mappingItemNameDisplayWidthInOneLine = GetMappingItemNameDisplayWidthInOneLine(colIndex);
		if (column.Width < mappingItemNameDisplayWidthInOneLine)
		{
			column.Width = mappingItemNameDisplayWidthInOneLine;
		}
	}

	private void AdjustAllTableColumnWidthToDisplayMappingItemNameInSingleRow()
	{
		for (int i = 4; i < _tableHeaderColumnsCount; i++)
		{
			C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[i];
			int mappingItemNameDisplayWidthInOneLine = GetMappingItemNameDisplayWidthInOneLine(i);
			if (column.Width < mappingItemNameDisplayWidthInOneLine)
			{
				column.Width = mappingItemNameDisplayWidthInOneLine;
			}
		}
	}

	private void AutoResizeTableColumnWidth(int colIndex = -1)
	{
		if (_tableDataSourceColumnNameTable == null)
		{
			return;
		}
		if (colIndex >= 0)
		{
			TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, colIndex);
			if (tableHeaderSetting.ColumnMappingData != null && tableHeaderSetting.ColumnMappingData.IsMappingItemSet)
			{
				string mappingItemName = tableHeaderSetting.ColumnMappingData.MappingItemName;
				C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[colIndex];
				column.Width = GetColumStringMaxWidth(mappingItemName);
				int mappingItemNameDisplayWidthInOneLine = GetMappingItemNameDisplayWidthInOneLine(colIndex);
				if (column.Width < mappingItemNameDisplayWidthInOneLine)
				{
					column.Width = mappingItemNameDisplayWidthInOneLine;
				}
			}
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>
		{
			["科目代码"] = -1,
			["科目名称"] = -1,
			["字"] = -1,
			["号"] = -1,
			["字号"] = -1,
			["摘要"] = -1,
			["对方科目"] = -1,
			["项目"] = -1
		};
		List<Tuple<int, int>> list = new List<Tuple<int, int>>();
		for (int i = 4; i < _tableHeaderColumnsCount; i++)
		{
			TableHeaderCellSetting tableHeaderSetting2 = GetTableHeaderSetting(_tableHeaderRowsCount - 1, i);
			if (tableHeaderSetting2.ColumnMappingData == null || !tableHeaderSetting2.ColumnMappingData.IsMappingItemSet)
			{
				if (_dataGrid.Cols[i].Width < 100)
				{
					list.Add(Tuple.Create(i, 100));
				}
				continue;
			}
			string mappingItemName2 = tableHeaderSetting2.ColumnMappingData.MappingItemName;
			if (dictionary.TryGetValue(mappingItemName2, out var value))
			{
				if (value == -1)
				{
					value = (dictionary[mappingItemName2] = Math.Max(GetColumStringMaxWidth(mappingItemName2), 100));
				}
				list.Add(Tuple.Create(i, value));
			}
		}
		CollectObjectEnum collectObjectEnum = selectCollectObject();
		if (collectObjectEnum == CollectObjectEnum.Balance || collectObjectEnum == CollectObjectEnum.Summary)
		{
			if (!dictionary.TryGetValue("科目代码", out var value2) || value2 == -1)
			{
				value2 = GetColumStringMaxWidth("科目代码");
				value2 = Math.Max(100, value2);
			}
			list.Add(Tuple.Create(3, value2));
		}
		foreach (Tuple<int, int> item in list)
		{
			_dataGrid.Cols[item.Item1].Width = item.Item2;
		}
		for (int j = 4; j < _tableHeaderColumnsCount; j++)
		{
			C1.Win.C1FlexGrid.Column column2 = _dataGrid.Cols[j];
			int mappingItemNameDisplayWidthInOneLine2 = GetMappingItemNameDisplayWidthInOneLine(j);
			if (column2.Width < mappingItemNameDisplayWidthInOneLine2)
			{
				column2.Width = mappingItemNameDisplayWidthInOneLine2;
			}
		}
		int GetColumStringMaxWidth(string colName, int padding = 4)
		{
			int num2 = _tableDataSourceColumnNameTable.Columns.IndexOf(colName);
			if (num2 == -1)
			{
				return 100;
			}
			int count = _dataGrid.Rows.Count;
			string text = string.Empty;
			for (int k = _tableHeaderRowsCount; k < count; k++)
			{
				DataGridRowUserData dataGridRowUserData = (DataGridRowUserData)_dataGrid.Rows[k].UserData;
				object obj = dataGridRowUserData.DataSource[num2];
				if (obj != null)
				{
					string text2 = obj.ToString();
					if (text2.Length > text.Length)
					{
						text = text2;
					}
				}
			}
			return MeasureTextWidth(text, _dataGridCellFont, int.MaxValue, padding, padding);
		}
	}

	private int GetMappingItemNameDisplayWidthInOneLine(int colIndex)
	{
		string empty = string.Empty;
		TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, colIndex);
		empty = ((tableHeaderSetting.ColumnMappingData != null && tableHeaderSetting.ColumnMappingData.IsMappingItemSet) ? ("[" + ConvertMappingItemNameToDisplayName(tableHeaderSetting.ColumnMappingData.MappingItemName, 0, 0) + "]") : "[未设置采数列]");
		return MeasureTextWidth(empty, _columHeaderCaptionFont, int.MaxValue, 4, 4);
	}

	private string FormatCaptionDisplayString(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		return text.Trim();
	}

	private string ConvertMappingItemNameToDisplayName(string strExpression, int startExpLength, int endExpLength)
	{
		if (strExpression == null || Collector == null)
		{
			return strExpression;
		}
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = strExpression;
		if (startExpLength > 0)
		{
			text = text3.Substring(0, startExpLength);
			text3 = text3.Substring(startExpLength);
		}
		if (endExpLength > 0)
		{
			text2 = text3.Substring(text3.Length - endExpLength);
			text3 = text3.Substring(0, text3.Length - endExpLength);
		}
		if (Collector.CollectObject == CollectObjectEnum.Balance)
		{
			if (text3.StartsWith("期初"))
			{
				return $"{text}{Collector.Setting.Start.Year}年{Collector.Setting.Start.Month}月 {text3}{text2}";
			}
			if (text3.StartsWith("期末"))
			{
				return $"{text}{Collector.Setting.End.Year}年{Collector.Setting.End.Month}月 {text3}{text2}";
			}
			if (text3.StartsWith("本月"))
			{
				return $"{text}{Collector.Setting.End.Year}年{Collector.Setting.End.Month}月 {text3.Substring(2)}{text2}";
			}
			if (text3.StartsWith("本期"))
			{
				return $"{text}{Collector.Setting.Start.Year}年{Collector.Setting.Start.Month}月-{Collector.Setting.End.Month}月 {text3.Substring(2)}{text2}";
			}
			if (text3.StartsWith("上期"))
			{
				return $"{text}{Collector.Setting.Start.Year - 1}年{Collector.Setting.Start.Month}月-{Collector.Setting.End.Month}月 {text3.Substring(2)}{text2}";
			}
			return strExpression;
		}
		if (Collector.CollectObject == CollectObjectEnum.Summary)
		{
			if (text3.EndsWith("月"))
			{
				string text4 = text3.Substring(0, text3.Length - 1);
				if (int.TryParse(text4, out var _))
				{
					return $"{text}{Collector.Setting.Start.Year}年{text3}{text2}";
				}
			}
			return strExpression;
		}
		return strExpression;
	}

	private void AutoAdjustTableHeaderDisplaySetting()
	{
		foreach (TableHeaderCellSetting tableHeaderSetting8 in _tableHeaderSettingList)
		{
			if (tableHeaderSetting8.CaptionDisplaySetting != null)
			{
				tableHeaderSetting8.CaptionDisplaySetting.DisplayAreaMinHeight = 0;
				tableHeaderSetting8.CaptionDisplaySetting.DisplayAreaTextHeight = 0;
				tableHeaderSetting8.CaptionDisplaySetting.CaptionDisplayString = null;
				tableHeaderSetting8.CaptionDisplaySetting.CaptionTextDrawRectInCell = new Rectangle(-1, -1, 0, 0);
			}
			if (tableHeaderSetting8.ColumnMappingDisplaySetting != null)
			{
				tableHeaderSetting8.ColumnMappingDisplaySetting.DisplayAreaMinHeight = 0;
				tableHeaderSetting8.ColumnMappingDisplaySetting.MappingItemDisplayString = null;
				tableHeaderSetting8.ColumnMappingDisplaySetting.MappingItemTextDrawRectInGrid = new Rectangle(-1, -1, 0, 0);
				tableHeaderSetting8.ColumnMappingDisplaySetting.MappingItemTextDrawRectInCell = new Rectangle(-1, -1, 0, 0);
			}
		}
		switch (Collector.CollectObject)
		{
		case CollectObjectEnum.Balance:
			_dataGrid.Cols[3].Visible = true;
			_dataGrid.Cols[2].Visible = true;
			_dataGrid.Cols[1].Visible = true;
			break;
		case CollectObjectEnum.Summary:
			_dataGrid.Cols[3].Visible = true;
			_dataGrid.Cols[2].Visible = true;
			_dataGrid.Cols[1].Visible = true;
			break;
		case CollectObjectEnum.Subsidiary:
			_dataGrid.Cols[3].Visible = false;
			_dataGrid.Cols[2].Visible = false;
			_dataGrid.Cols[1].Visible = true;
			break;
		}
		for (int num = _tableHeaderRowsCount - 1; num >= 0; num--)
		{
			for (int i = 0; i < _tableHeaderColumnsCount; i++)
			{
				TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(num, i);
				if (!tableHeaderSetting.IsMergeRange && tableHeaderSetting.ColumnMappingData == null)
				{
					TableHeaderCellCaptionDisplaySetting captionDisplaySetting = tableHeaderSetting.CaptionDisplaySetting;
					int cellRangeWidth = GetCellRangeWidth(tableHeaderSetting.CellAreaRange);
					captionDisplaySetting.CaptionDisplayString = FormatCaptionDisplayString(captionDisplaySetting.CaptionText);
					captionDisplaySetting.DisplayAreaMinHeight = MeasureTextHeight(captionDisplaySetting.CaptionDisplayString, _columHeaderCaptionFont, cellRangeWidth, captionDisplaySetting.CellTopPadding, captionDisplaySetting.CellBottomPadding);
				}
			}
		}
		for (int num2 = _tableHeaderRowsCount - 1; num2 >= 0; num2--)
		{
			for (int j = 0; j < _tableHeaderColumnsCount; j++)
			{
				TableHeaderCellSetting tableHeaderSetting2 = GetTableHeaderSetting(num2, j);
				TableHeaderCellCaptionDisplaySetting captionDisplaySetting2 = tableHeaderSetting2.CaptionDisplaySetting;
				if (captionDisplaySetting2.ColumnMappingItemList == null || captionDisplaySetting2.ColumnMappingItemList.Count == 0)
				{
					continue;
				}
				int cellRangeWidth2 = GetCellRangeWidth(tableHeaderSetting2.CellAreaRange);
				captionDisplaySetting2.CaptionDisplayString = FormatCaptionDisplayString(captionDisplaySetting2.CaptionText);
				int num3 = MeasureTextHeight(captionDisplaySetting2.CaptionDisplayString, _columHeaderCaptionFont, cellRangeWidth2);
				int num4 = 0;
				foreach (TableHeaderCellSetting columnMappingItem in captionDisplaySetting2.ColumnMappingItemList)
				{
					TableHeaderCellColumnMappingDisplaySetting columnMappingDisplaySetting = columnMappingItem.ColumnMappingDisplaySetting;
					int cellRangeWidth3 = GetCellRangeWidth(columnMappingDisplaySetting.CellAreaRange);
					columnMappingDisplaySetting.MappingItemDisplayString = ((!columnMappingItem.ColumnMappingData.IsMappingItemSet) ? "[未设置采数列]" : ("[" + columnMappingItem.ColumnMappingData.MappingItemName + "]"));
					columnMappingDisplaySetting.MappingItemDisplayString = ConvertMappingItemNameToDisplayName(columnMappingDisplaySetting.MappingItemDisplayString, 1, 1);
					int val = MeasureTextHeight(columnMappingDisplaySetting.MappingItemDisplayString, _columHeaderCaptionFont, cellRangeWidth3);
					num4 = Math.Max(num4, val);
				}
				int num5 = ((captionDisplaySetting2.CaptionDisplayString != null) ? 5 : 0);
				tableHeaderSetting2.CaptionDisplaySetting.DisplayAreaMinHeight = num3 + num5 + num4 + tableHeaderSetting2.CaptionDisplaySetting.CellTopPadding + tableHeaderSetting2.CaptionDisplaySetting.CellBottomPadding;
				tableHeaderSetting2.CaptionDisplaySetting.DisplayAreaTextHeight = num3 + num5 + num4;
			}
		}
		for (int num6 = _tableHeaderRowsCount - 2; num6 >= 0; num6--)
		{
			for (int k = 0; k < _tableHeaderColumnsCount; k++)
			{
				TableHeaderCellSetting tableHeaderSetting3 = GetTableHeaderSetting(num6, k);
				if (tableHeaderSetting3.IsMergeRange && tableHeaderSetting3.IsMergeRangeLeftTopCell)
				{
					TableHeaderCellCaptionDisplaySetting captionDisplaySetting3 = tableHeaderSetting3.CaptionDisplaySetting;
					if (captionDisplaySetting3.ColumnMappingItemList == null || captionDisplaySetting3.ColumnMappingItemList.Count <= 0)
					{
						int cellRangeWidth4 = GetCellRangeWidth(tableHeaderSetting3.CellAreaRange);
						captionDisplaySetting3.CaptionDisplayString = FormatCaptionDisplayString(captionDisplaySetting3.CaptionText);
						captionDisplaySetting3.DisplayAreaMinHeight = MeasureTextHeight(captionDisplaySetting3.CaptionDisplayString, _columHeaderCaptionFont, cellRangeWidth4, captionDisplaySetting3.CellTopPadding, captionDisplaySetting3.CellBottomPadding);
					}
				}
			}
		}
		int defaultSize = _dataGrid.Rows.DefaultSize;
		for (int num7 = _tableHeaderRowsCount - 1; num7 >= 0; num7--)
		{
			int val2 = defaultSize;
			for (int l = 0; l < _tableHeaderColumnsCount; l++)
			{
				TableHeaderCellSetting tableHeaderSetting4 = GetTableHeaderSetting(num7, l);
				if (!tableHeaderSetting4.IsMergeRange)
				{
					val2 = Math.Max(tableHeaderSetting4.CaptionDisplaySetting.DisplayAreaMinHeight, val2);
				}
			}
			_dataGrid.Rows[num7].Height = val2;
		}
		for (int num8 = _tableHeaderRowsCount - 1; num8 >= 0; num8--)
		{
			int num9 = _dataGrid.Rows[num8].Height;
			for (int m = 0; m < _tableHeaderColumnsCount; m++)
			{
				TableHeaderCellSetting tableHeaderSetting5 = GetTableHeaderSetting(num8, m);
				if (tableHeaderSetting5.IsMergeRange && tableHeaderSetting5.IsMergeRangeLeftTopCell)
				{
					int cellRangeHeight = GetCellRangeHeight(tableHeaderSetting5.CellAreaRange);
					if (cellRangeHeight < tableHeaderSetting5.CaptionDisplaySetting.DisplayAreaMinHeight)
					{
						_dataGrid.Rows[num8].Height = _dataGrid.Rows[num8].Height + (tableHeaderSetting5.CaptionDisplaySetting.DisplayAreaMinHeight - cellRangeHeight);
					}
				}
			}
		}
		for (int num10 = _tableHeaderRowsCount - 1; num10 >= 0; num10--)
		{
			int num11 = _dataGrid.Rows[num10].Height;
			for (int n = 0; n < _tableHeaderColumnsCount; n++)
			{
				TableHeaderCellSetting tableHeaderSetting6 = GetTableHeaderSetting(num10, n);
				if (tableHeaderSetting6.ColumnMappingData == null && (!tableHeaderSetting6.IsMergeRange || tableHeaderSetting6.IsMergeRangeLeftTopCell))
				{
					int cellRangeWidth5 = GetCellRangeWidth(tableHeaderSetting6.CellAreaRange);
					tableHeaderSetting6.CaptionDisplaySetting.CaptionTextDrawRectInCell = new Rectangle(0, 0, cellRangeWidth5, num11);
				}
			}
		}
		for (int num12 = _tableHeaderRowsCount - 1; num12 >= 0; num12--)
		{
			for (int num13 = 0; num13 < _tableHeaderColumnsCount; num13++)
			{
				TableHeaderCellSetting tableHeaderSetting7 = GetTableHeaderSetting(num12, num13);
				if (tableHeaderSetting7.CaptionDisplaySetting.ColumnMappingItemList == null || tableHeaderSetting7.CaptionDisplaySetting.ColumnMappingItemList.Count == 0)
				{
					continue;
				}
				int num14 = ((tableHeaderSetting7.CaptionDisplaySetting.CaptionDisplayString != null) ? 5 : 0);
				int cellRangeWidth6 = GetCellRangeWidth(tableHeaderSetting7.CellAreaRange);
				int cellRangeHeight2 = GetCellRangeHeight(tableHeaderSetting7.CellAreaRange);
				int num15 = MeasureTextHeight(tableHeaderSetting7.CaptionDisplaySetting.CaptionDisplayString, _columHeaderCaptionFont, cellRangeWidth6);
				int num16 = (cellRangeHeight2 - tableHeaderSetting7.CaptionDisplaySetting.DisplayAreaTextHeight) / 2;
				tableHeaderSetting7.CaptionDisplaySetting.CaptionTextDrawRectInCell = new Rectangle(0, num16, cellRangeWidth6, num15);
				foreach (TableHeaderCellSetting columnMappingItem2 in tableHeaderSetting7.CaptionDisplaySetting.ColumnMappingItemList)
				{
					TableHeaderCellColumnMappingDisplaySetting columnMappingDisplaySetting2 = columnMappingItem2.ColumnMappingDisplaySetting;
					int cellRangeWidth7 = GetCellRangeWidth(columnMappingDisplaySetting2.CellAreaRange);
					int num17 = cellRangeWidth7 - columnMappingDisplaySetting2.CellLeftPadding - columnMappingDisplaySetting2.CellRightPadding;
					int num18 = MeasureTextHeight(columnMappingDisplaySetting2.MappingItemDisplayString, _columHeaderCaptionFont, num17);
					int num19 = num16 + num15 + num14;
					int num20 = MeasureTextWidth(columnMappingDisplaySetting2.MappingItemDisplayString, _columHeaderCaptionFont, num17, 2, 2);
					columnMappingDisplaySetting2.MappingItemTextDrawRectInCell = new Rectangle((cellRangeWidth7 - num20) / 2, num19, num20, num18);
				}
			}
		}
	}

	private void Grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row < _dataGrid.Rows.Fixed && e.Col >= 4)
		{
			DrawTableHeader(e);
		}
		else if (e.Row >= _dataGrid.Rows.Fixed && e.Col == 2)
		{
			DrawAdjustLevelIcon(e);
		}
		else if (e.Row < _dataGrid.Rows.Fixed && e.Col == 0)
		{
			DrawLeftTopCellFilterIcon(e);
		}
	}

	private void DrawTableHeader(OwnerDrawCellEventArgs e)
	{
		TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(e.Row, e.Col);
		if (tableHeaderSetting == null || tableHeaderSetting.CaptionDisplaySetting == null)
		{
			return;
		}
		e.DrawCell(DrawCellFlags.Background);
		StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Center;
		stringFormat.LineAlignment = StringAlignment.Center;
		if ((!tableHeaderSetting.IsMergeRange || (tableHeaderSetting.IsMergeRange && tableHeaderSetting.IsMergeRangeLeftTopCell)) && tableHeaderSetting.CaptionDisplaySetting.CaptionDisplayString != null)
		{
			Rectangle captionTextDrawRectInCell = tableHeaderSetting.CaptionDisplaySetting.CaptionTextDrawRectInCell;
			captionTextDrawRectInCell.X += e.Bounds.X;
			captionTextDrawRectInCell.Y += e.Bounds.Y;
			e.Graphics.DrawString(tableHeaderSetting.CaptionDisplaySetting.CaptionDisplayString, _columHeaderCaptionFont, _columnHeaderCaptionBrush, captionTextDrawRectInCell, stringFormat);
		}
		if (tableHeaderSetting.CaptionDisplaySetting.ColumnMappingItemList != null)
		{
			foreach (TableHeaderCellSetting columnMappingItem in tableHeaderSetting.CaptionDisplaySetting.ColumnMappingItemList)
			{
				TableHeaderCellColumnMappingDisplaySetting columnMappingDisplaySetting = columnMappingItem.ColumnMappingDisplaySetting;
				Rectangle mappingItemTextDrawRectInCell = columnMappingDisplaySetting.MappingItemTextDrawRectInCell;
				Rectangle cellRect = _dataGrid.GetCellRect(columnMappingDisplaySetting.CellAreaRange.TopRow, columnMappingDisplaySetting.CellAreaRange.LeftCol);
				mappingItemTextDrawRectInCell.X += cellRect.X;
				mappingItemTextDrawRectInCell.Y += cellRect.Y;
				int num = columnMappingDisplaySetting.CellAreaRange.LeftCol - e.Col;
				for (int i = 0; i < num; i++)
				{
					mappingItemTextDrawRectInCell.X += _dataGrid.Cols[e.Col + i].WidthDisplay;
				}
				columnMappingDisplaySetting.MappingItemTextDrawRectInGrid = mappingItemTextDrawRectInCell;
				e.Graphics.DrawString(columnMappingDisplaySetting.MappingItemDisplayString, _columHeaderCaptionFont, columnMappingItem.ColumnMappingData.IsMappingItemSet ? _columnHeaderHasSettingBrush : _columnHeaderUnSettingBrush, mappingItemTextDrawRectInCell, stringFormat);
			}
		}
		e.DrawCell(DrawCellFlags.Border);
		e.Handled = true;
	}

	private void GetAdjustLevelCellIconPosition(int row, int col, Rectangle cellRect, out Rectangle menuIcon, out Rectangle upIcon, out Rectangle dwonIcon)
	{
		int num = 16;
		int num2 = 5;
		int num3 = num / 2;
		int num4 = num;
		int num5 = num4 / 2;
		int num6 = _dataGrid.Cols[col].WidthDisplay / 2;
		int num7 = _dataGrid.Rows[row].HeightDisplay / 2;
		Point point = new Point(num6 - num3 - num2 - num, num7 - num5);
		Point point2 = new Point(num6 - num3, num7 - num5);
		Point point3 = new Point(num6 + num3 + num2, num7 - num5);
		int num8 = cellRect.X;
		int num9 = cellRect.Y;
		menuIcon = new Rectangle(num8 + point.X, num9 + point.Y, num, num4);
		upIcon = new Rectangle(num8 + point2.X, num9 + point2.Y, num, num4);
		dwonIcon = new Rectangle(num8 + point3.X, num9 + point3.Y, num, num4);
	}

	private void DrawAdjustLevelIcon(OwnerDrawCellEventArgs e)
	{
		e.DrawCell(DrawCellFlags.Background);
		GetAdjustLevelCellIconPosition(e.Row, e.Col, e.Bounds, out var menuIcon, out var upIcon, out var dwonIcon);
		if (_mouseCurrentOverWhichAdjustLevelCell != null)
		{
			if (menuIcon.Contains(_mouseCurrentOverWhichAdjustLevelCell.MouseLocation))
			{
				DrawIconBackground(menuIcon);
			}
			else if (upIcon.Contains(_mouseCurrentOverWhichAdjustLevelCell.MouseLocation))
			{
				DrawIconBackground(upIcon);
			}
			else if (dwonIcon.Contains(_mouseCurrentOverWhichAdjustLevelCell.MouseLocation))
			{
				DrawIconBackground(dwonIcon);
			}
		}
		e.Graphics.DrawImage(Resources.btnMenu16, menuIcon);
		e.Graphics.DrawImage(Resources.btnUp16, upIcon);
		e.Graphics.DrawImage(Resources.btnDown16, dwonIcon);
		e.DrawCell(DrawCellFlags.Border);
		e.Handled = true;
		void DrawIconBackground(Rectangle rt)
		{
			Rectangle rect = new Rectangle(rt.X - 2, rt.Y - 2, rt.Width + 4, rt.Height + 4);
			_adjustLevelIconOnFocusBackgroundBrush.Color = Util.DarkenColor(_dataGrid.Styles.SelectedColumnHeader.BackColor, 0.1);
			e.Graphics.FillRectangle(_adjustLevelIconOnFocusBackgroundBrush, rect);
		}
	}

	private void DrawLeftTopCellFilterIcon(OwnerDrawCellEventArgs e)
	{
		if (_dataGrid.FilterManager.IsFiltering)
		{
			Rectangle cellRect = _dataGrid.GetCellRect(0, 0);
			int num = Resources.Filter12.Width;
			int num2 = Resources.Filter12.Height;
			int num3 = (cellRect.Width - num) / 2;
			int num4 = (cellRect.Height - num2) / 2;
			Rectangle rect = new Rectangle(cellRect.X + num3, cellRect.Y + num4, num, num2);
			e.DrawCell(DrawCellFlags.Background);
			e.Graphics.DrawImage(Resources.Filter12, rect);
			e.DrawCell(DrawCellFlags.Border);
			e.Handled = true;
		}
	}

	private void Icon_OnMenuIconClicked(int row, int col, Point point)
	{
		ShowContextMenu_OnIconMenuClicked(row, point);
	}

	private void Icon_OnUpLevelIconClicked(int row, int col)
	{
		MakeLevelUp(row);
	}

	private void Icon_OnDownLevelIconClicked(int row, int col)
	{
		MakeLevelDown(row);
	}

	private void Mouse_OnCellRightClicked(int row, int col, Point point)
	{
		ShowContextMenu_OnCellRightClicked(row, col, point);
	}

	private int GetCellRangeWidth(C1.Win.C1FlexGrid.CellRange cellRange)
	{
		int num = 0;
		for (int i = cellRange.LeftCol; i <= cellRange.RightCol; i++)
		{
			num += _dataGrid.Cols[i].Width;
		}
		return num;
	}

	private int GetCellRangeHeight(C1.Win.C1FlexGrid.CellRange cellRange)
	{
		int num = 0;
		for (int i = cellRange.TopRow; i <= cellRange.BottomRow; i++)
		{
			num += _dataGrid.Rows[i].Height;
		}
		return num;
	}

	private int MeasureTextWidth(string text, Font font, int textAreaMaxWidth, int leftPadding = 0, int rightPadding = 0)
	{
		if (text == null)
		{
			return 0;
		}
		Size proposedSize = new Size(textAreaMaxWidth, int.MaxValue);
		return TextRenderer.MeasureText(text, font, proposedSize, TextFormatFlags.HorizontalCenter | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Width + leftPadding + rightPadding;
	}

	private int MeasureTextHeight(string text, Font font, int textAreaWidth, int topPadding = 0, int bottomPadding = 0)
	{
		if (text == null)
		{
			return 0;
		}
		Size proposedSize = new Size(textAreaWidth, int.MaxValue);
		return TextRenderer.MeasureText(text, font, proposedSize, TextFormatFlags.HorizontalCenter | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height + topPadding + bottomPadding;
	}

	private C1FlexGrid GetTitleGrid(Auditai.Model.Table table)
	{
		if (table is TicketCollectFillTable ticketCollectFillTable)
		{
			C1FlexGrid c1FlexGrid = new C1FlexGrid();
			if (!(ticketCollectFillTable.CollectPageTitleData is C1FlexGrid c1FlexGrid2))
			{
				c1FlexGrid.Rows.Count = 0;
				c1FlexGrid.Cols.Count = 0;
				return c1FlexGrid;
			}
			int count = c1FlexGrid2.Rows.Count;
			int count2 = c1FlexGrid2.Cols.Count;
			c1FlexGrid.Rows.Count = count;
			c1FlexGrid.Cols.Count = count2;
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < count2; j++)
				{
					c1FlexGrid[i, j] = c1FlexGrid2[i, j];
					c1FlexGrid.SetCellStyle(i, j, CreateCellStyleFromSrcStyle(c1FlexGrid, i, j, c1FlexGrid2.GetCellStyle(i, j), i == 0));
					TicketCollectFillTable.TableCellUserData tableCellUserData = c1FlexGrid2.GetUserData(i, j) as TicketCollectFillTable.TableCellUserData;
					GuessDetailAccountNameByDataFormat(c1FlexGrid2[i, j] as string, tableCellUserData?.tableColumnDataFormat);
				}
			}
			for (int k = 0; k < count; k++)
			{
				c1FlexGrid.Rows[k].Height = ((k == 0) ? 45 : 30);
			}
			for (int l = 0; l < count2; l++)
			{
				c1FlexGrid.Cols[l].Width = c1FlexGrid2.Cols[l].Width;
			}
			{
				foreach (C1.Win.C1FlexGrid.CellRange item in (IEnumerable)c1FlexGrid2.MergedRanges)
				{
					c1FlexGrid.MergedRanges.Add(item);
				}
				return c1FlexGrid;
			}
		}
		C1FlexGrid c1FlexGrid3 = new C1FlexGrid();
		TableTitle title = table.Title;
		if (title.Columns.Count == 0 || title.Rows.Count == 0)
		{
			string displayValue = title.TitleCell.GetDisplayValue();
			if (string.IsNullOrEmpty(displayValue))
			{
				c1FlexGrid3.Rows.Count = 0;
				c1FlexGrid3.Cols.Count = 0;
				return c1FlexGrid3;
			}
			c1FlexGrid3.Rows.Count = 1;
			c1FlexGrid3.Cols.Count = 1;
			c1FlexGrid3[0, 0] = displayValue;
			return c1FlexGrid3;
		}
		c1FlexGrid3.Rows.Count = title.Rows.Count + 1;
		c1FlexGrid3.Cols.Count = title.Columns.Count;
		for (int m = 0; m < title.Rows.Count; m++)
		{
			for (int n = 0; n < title.Columns.Count; n++)
			{
				TableTitleCell cell = title.GetCell(m + 1, n);
				string exp = (string)(c1FlexGrid3[m + 1, n] = cell.GetDisplayValue());
				c1FlexGrid3.SetCellStyle(m + 1, n, CreateCellStyle(c1FlexGrid3, m + 1, n, cell, isMainTitle: false));
				GuessDetailAccountNameByDataFormat(exp, cell.DataFormat);
			}
		}
		c1FlexGrid3.MergedRanges.Add(0, 0, 0, title.Columns.Count - 1);
		c1FlexGrid3[0, 0] = title.TitleCell.GetDisplayValue();
		c1FlexGrid3.Rows[0].Height = 45;
		c1FlexGrid3.SetCellStyle(0, 0, CreateCellStyle(c1FlexGrid3, 0, 0, title.TitleCell, isMainTitle: true));
		foreach (TicketMerge merge in title.Merges)
		{
			c1FlexGrid3.MergedRanges.Add(merge.TopRow + 1, merge.LeftColumn, merge.BottomRow + 1, merge.RightColumn);
		}
		for (int num = 0; num < title.Rows.Count; num++)
		{
			c1FlexGrid3.Rows[num + 1].Height = 30;
		}
		for (int num2 = 0; num2 < title.Columns.Count; num2++)
		{
			c1FlexGrid3.Cols[num2].Width = (int)title.Columns[num2].Width;
		}
		return c1FlexGrid3;
		C1.Win.C1FlexGrid.CellStyle CreateCellStyle(C1FlexGrid grid, int rowIndex, int colIndex, TableTitleCell titleCell, bool isMainTitle)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = grid.Styles.Add(rowIndex + "_" + colIndex, grid.Styles.Normal);
			cellStyle.Font = (isMainTitle ? _MainTitleFont : _SubTitleFont);
			cellStyle.TextAlign = C1FlexGridEx.ToTextAlign(titleCell.Align);
			cellStyle.ForeColor = Color.Black;
			cellStyle.BackColor = Color.Transparent;
			return cellStyle;
		}
		C1.Win.C1FlexGrid.CellStyle CreateCellStyleFromSrcStyle(C1FlexGrid grid, int rowIndex, int colIndex, C1.Win.C1FlexGrid.CellStyle srcStyle, bool isMainTitle)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = grid.Styles.Add(rowIndex + "_" + colIndex, srcStyle);
			cellStyle2.ForeColor = Color.Black;
			cellStyle2.BackColor = Color.Transparent;
			cellStyle2.Font = (isMainTitle ? _MainTitleFont : _SubTitleFont);
			return cellStyle2;
		}
	}

	private void GuessDetailAccountNameByDataFormat(string exp, DataFormat? dataFormat)
	{
		if (string.IsNullOrWhiteSpace(exp) || !dataFormat.HasValue)
		{
			return;
		}
		if (dataFormat.Value.HasComboList)
		{
			_detailAccountNameCheckList.Add(exp.Trim());
			return;
		}
		DataFormatType formatType = dataFormat.Value.FormatType;
		if (formatType == DataFormatType.General || formatType == DataFormatType.ComboList)
		{
			_detailAccountNameCheckList.Add(exp.Trim());
		}
	}

	private void InitializeTitle(Auditai.Model.Table table)
	{
		C1FlexGrid titleGrid = GetTitleGrid(table);
		if (titleGrid.Rows.Count == 0)
		{
			pnlTitlePanel.Visible = false;
			return;
		}
		int count = titleGrid.Rows.Count;
		int count2 = titleGrid.Cols.Count;
		int num = 0;
		int num2 = 0;
		_titleGridRawWidthArr = new int[count2];
		_titleGridRawHeightArr = new int[count];
		for (int i = 0; i < count; i++)
		{
			int num3 = titleGrid.Rows[i].Height;
			num += num3;
			_titleGridRawHeightArr[i] = num3;
		}
		for (int j = 0; j < count2; j++)
		{
			int num4 = titleGrid.Cols[j].Width;
			num2 += num4;
			_titleGridRawWidthArr[j] = num4;
		}
		_titlePanelHeight = num;
		pnlTitlePanel.Visible = false;
		pnlTitlePanel.Controls.Remove(_gridTitle);
		panelDataTable.Controls.Add(_gridTitle);
		panelDataTable.SizeChanged += PanelDataTable_SizeChanged;
		_gridTitle.Cols.Count = count2;
		_gridTitle.Cols.Fixed = 0;
		_gridTitle.Rows.Count = count;
		_gridTitle.Rows.Fixed = 0;
		_gridTitle.ExtendLastCol = false;
		_gridTitle.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		_gridTitle.ScrollBars = ScrollBars.None;
		_gridTitle.AutoResize = false;
		_gridTitle.AllowSorting = AllowSortingEnum.None;
		_gridTitle.AllowEditing = false;
		_gridTitle.AllowMerging = AllowMergingEnum.Custom;
		_gridTitle.BeforeScroll += _gridTitle_BeforeScroll;
		_gridTitle.Select(-1, -1);
		_gridTitle.BeforeSelChange += _gridTitle_BeforeSelChange;
		foreach (C1.Win.C1FlexGrid.CellRange item in (IEnumerable)titleGrid.MergedRanges)
		{
			_gridTitle.MergedRanges.Add(item);
		}
		for (int k = 0; k < count; k++)
		{
			for (int l = 0; l < count2; l++)
			{
				_gridTitle[k, l] = titleGrid[k, l];
				_gridTitle.SetCellStyle(k, l, titleGrid.GetCellStyle(k, l));
			}
		}
	}

	private int GetAdditionalColumnsWidth()
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			num += _dataGrid.Cols[i].WidthDisplay;
		}
		return num;
	}

	private void AdjustTitleGridWidth(int width)
	{
		int[] array = SplitValueWithByPercent(width, _titleGridRawWidthArr);
		for (int i = 0; i < array.Length; i++)
		{
			_gridTitle.Cols[i].Width = array[i];
		}
		_gridTitle.Width = width;
	}

	private void AdjustTitleGridHeight(int height)
	{
		int[] array = SplitValueWithByPercent(height, _titleGridRawHeightArr);
		for (int i = 0; i < array.Length; i++)
		{
			_gridTitle.Rows[i].Height = array[i];
		}
		_gridTitle.Height = height;
	}

	private void PanelDataTable_SizeChanged(object sender, EventArgs e)
	{
		_gridTitle.BeginUpdate();
		_dataGrid.BeginUpdate();
		try
		{
			_gridTitle.Left = 0;
			_gridTitle.Top = 0;
			AdjustTitleGridWidth(panelDataTable.Width);
			AdjustTitleGridHeight(_titlePanelHeight);
			_dataGrid.Left = 0;
			_dataGrid.Top = _gridTitle.Height;
			_dataGrid.Width = panelDataTable.Width;
			_dataGrid.Height = panelDataTable.Height - _gridTitle.Height;
		}
		finally
		{
			_gridTitle.EndUpdate();
			_dataGrid.EndUpdate();
		}
	}

	private void _gridTitle_BeforeSelChange(object sender, RangeEventArgs e)
	{
		e.Cancel = true;
	}

	private void _gridTitle_BeforeScroll(object sender, RangeEventArgs e)
	{
		e.Cancel = true;
	}

	private void PnlDockingTab_SizeChanged(object sender, EventArgs e)
	{
		if (_titleGridRawHeightArr != null)
		{
			double num;
			for (num = (float)_titlePanelHeight * 100f / (float)pnlDockingTab.Height; (int)((double)pnlDockingTab.Height * num) < _titlePanelHeight; num += 0.0010000000474974513)
			{
			}
			pnlTitlePanel.SizeRatio = num;
			pnlTitlePanel.AutoScroll = false;
			int titlePanelHeight = _titlePanelHeight;
			int[] array = SplitValueWithByPercent(titlePanelHeight, _titleGridRawHeightArr);
			for (int i = 0; i < array.Length; i++)
			{
				_gridTitle.Rows[i].Height = array[i];
			}
			_gridTitle.Height = _titlePanelHeight;
		}
	}

	private void PnlTitlePanel_SizeChanged(object sender, EventArgs e)
	{
		if (_titleGridRawWidthArr != null)
		{
			_gridTitle.Width = pnlTitlePanel.Width;
			_gridTitle.Height = _titlePanelHeight;
			int[] array = SplitValueWithByPercent(pnlTitlePanel.Width, _titleGridRawWidthArr);
			for (int i = 0; i < array.Length; i++)
			{
				_gridTitle.Cols[i].Width = array[i];
			}
		}
	}

	private int[] SplitValueWithByPercent(int newTotalValue, int[] valueArr)
	{
		int num = valueArr.Sum();
		int num2 = valueArr.Length;
		int[] array = new int[num2];
		int num3 = 0;
		float[] array2 = new float[num2];
		for (int i = 0; i < num2; i++)
		{
			if (i == num2 - 1)
			{
				array2[i] = 1f;
				continue;
			}
			num3 += valueArr[i];
			array2[i] = (float)num3 * 1f / (float)num;
		}
		num3 = 0;
		for (int j = 0; j < num2; j++)
		{
			int num4;
			if (j == num2 - 1)
			{
				num4 = newTotalValue - num3;
			}
			else
			{
				num4 = (int)(array2[j] * (float)newTotalValue) - num3;
				num3 += num4;
			}
			array[j] = num4;
		}
		return array;
	}

	private void InitContextMenu()
	{
		_ctxCellRightClick.CommandLinks.Add(_dataGrid.FilterManager.GenLnkFilter());
		_ctxCellRightClick.CommandLinks.Add(_dataGrid.FilterManager.GenLnkSample());
		_ctxCellRightClick.CommandLinks.Add(_dataGrid.FilterManager.GenLnkSelect());
		_ctxCellRightClick.CommandLinks.Add(_dataGrid.FilterManager.GenLnkCancelCurrentColumn());
		C1Command c1Command = new C1Command
		{
			Text = "复制",
			Image = Resources.ctxCopy
		};
		c1Command.Click += Cmd_Click;
		_ctxCellRightClick.CommandLinks.Add(new C1CommandLink(c1Command)
		{
			Delimiter = true
		});
		C1Command c1Command2 = new C1Command
		{
			Text = "降序排列",
			Image = Resources.ctxDescending
		};
		c1Command2.Click += Cmd_SortDesc_Click;
		_ctxColSort.CommandLinks.Add(new C1CommandLink(c1Command2)
		{
			Delimiter = true
		});
		C1Command c1Command3 = new C1Command
		{
			Text = "升序排列",
			Image = Resources.ctxAscending
		};
		c1Command3.Click += Cmd_SortAsc_Click;
		_ctxColSort.CommandLinks.Add(new C1CommandLink(c1Command3));
		C1Command c1Command4 = new C1Command
		{
			Text = "取消排序"
		};
		c1Command4.Click += Cmd_CancelSort_Click;
		_ctxColSort.CommandLinks.Add(new C1CommandLink(c1Command4));
	}

	private void Cmd_Click(object sender, ClickEventArgs e)
	{
		Copy();
	}

	private CollectObjectEnum selectCollectObject()
	{
		return comboCollectObject.Text.Trim() switch
		{
			"科目余额表" => CollectObjectEnum.Balance, 
			"记账凭证表" => CollectObjectEnum.Subsidiary, 
			"月度汇总表" => CollectObjectEnum.Summary, 
			_ => CollectObjectEnum.None, 
		};
	}

	private Account selectedAccount()
	{
		if (comboAccountTree.Text == string.Empty)
		{
			return null;
		}
		return comboAccountTree.SelectedNode?.Tag as Account;
	}

	private object selectedAuxiliary()
	{
		if (comboAuxiliaryTree.Text == string.Empty)
		{
			return null;
		}
		object obj = comboAuxiliaryTree.SelectedNode?.Tag;
		if (obj is Tuple<Account, AuxiliaryClass> tuple)
		{
			return tuple.Item2;
		}
		if (obj is Tuple<Account, AuxiliaryItem> tuple2)
		{
			return tuple2.Item2;
		}
		return null;
	}

	private AnalysisProject selectedSpbAnalysis()
	{
		return (AnalysisProject)spbAnalysis.Tag;
	}

	private DateTime selectedStartDate()
	{
		int month = int.Parse(comboStartMonth.Text.Trim());
		return new DateTime(Collector.TitlePeriod.Item1.Year, month, 1);
	}

	private DateTime selectedEndDate()
	{
		int month = int.Parse(comboEndMonth.Text.Trim());
		return new DateTime(Collector.TitlePeriod.Item1.Year, month, DateTime.DaysInMonth(Collector.TitlePeriod.Item1.Year, month));
	}

	private void SelectCollectObject(CollectObjectEnum collectObject)
	{
		switch (collectObject)
		{
		case CollectObjectEnum.Balance:
			comboCollectObject.SelectedIndex = 0;
			break;
		case CollectObjectEnum.Subsidiary:
			comboCollectObject.SelectedIndex = 1;
			break;
		case CollectObjectEnum.Summary:
			comboCollectObject.SelectedIndex = 2;
			break;
		case CollectObjectEnum.None:
			break;
		}
	}

	private void SelectAccountNode(Account account)
	{
		if (account == null || string.IsNullOrWhiteSpace(account.Name))
		{
			comboAccountTree.SelectedIndex = -1;
			comboAccountTree.Text = string.Empty;
			comboAccountTree.SelectedNode = null;
			comboAuxiliaryTree.SelectedIndex = -1;
			comboAuxiliaryTree.Text = string.Empty;
			comboAuxiliaryTree.SelectedNode = null;
			return;
		}
		List<System.Windows.Forms.TreeNode> list = new List<System.Windows.Forms.TreeNode>();
		AllChildren(comboAccountTree.TreeView.Nodes, list);
		foreach (System.Windows.Forms.TreeNode item in list)
		{
			if (item.Tag is Account account2 && account2 == account)
			{
				comboAccountTree.SelectedNode = item;
				PopulateAuxiliaryTree(account2);
				return;
			}
		}
		PopulateAuxiliaryTree(null);
	}

	private void SetAuxiliarySelectNodeToNone()
	{
		comboAuxiliaryTree.SelectedIndex = -1;
		comboAuxiliaryTree.Text = string.Empty;
		comboAuxiliaryTree.SelectedNode = null;
	}

	private void SelectAuxiliaryNode(object auxiliary)
	{
		if (auxiliary == null)
		{
			SetAuxiliarySelectNodeToNone();
			return;
		}
		Account account = selectedAccount();
		if (account == null)
		{
			SetAuxiliarySelectNodeToNone();
			return;
		}
		string name = account.Name;
		string empty = string.Empty;
		if (!(auxiliary is AuxiliaryClass auxiliaryClass))
		{
			if (!(auxiliary is AuxiliaryItem auxiliaryItem))
			{
				comboAuxiliaryTree.SelectedIndex = -1;
				comboAuxiliaryTree.Text = string.Empty;
				return;
			}
			empty = auxiliaryItem.Name;
		}
		else
		{
			empty = auxiliaryClass.Name;
		}
		List<System.Windows.Forms.TreeNode> list = new List<System.Windows.Forms.TreeNode>();
		AllChildren(comboAuxiliaryTree.TreeView.Nodes, list);
		foreach (System.Windows.Forms.TreeNode item in list)
		{
			object tag = item.Tag;
			if (!(tag is Tuple<Account, AuxiliaryClass> tuple))
			{
				if (tag is Tuple<Account, AuxiliaryItem> tuple2 && tuple2.Item1.Name == name && tuple2.Item2.Name == empty)
				{
					comboAuxiliaryTree.SelectedNode = item;
				}
			}
			else if (tuple.Item1.Name == name && tuple.Item2.Name == empty)
			{
				comboAuxiliaryTree.SelectedNode = item;
			}
		}
	}

	private void ResetMappingItemContextMenu()
	{
		CollectObjectEnum collectObjectEnum = selectCollectObject();
		if (collectObjectEnum == _ctxMappingItemType)
		{
			return;
		}
		_ctxMappingMenu = new C1ContextMenu();
		List<C1CommandLink> list = new List<C1CommandLink>();
		C1Command c1Command = new C1Command();
		c1Command.Text = "空";
		c1Command.Click += Cmd_OnCancelMappingItemClicked;
		c1Command.UserData = "空";
		list.Add(new C1CommandLink(c1Command));
		switch (collectObjectEnum)
		{
		case CollectObjectEnum.Balance:
			foreach (string key in CaptionDic.Balance.Keys)
			{
				C1Command c1Command3 = new C1Command();
				c1Command3.Text = ConvertMappingItemNameToDisplayName(key, 0, 0);
				c1Command3.Click += Cmd_OnMappingItemClicked;
				c1Command3.UserData = key;
				list.Add(new C1CommandLink(c1Command3));
			}
			_ctxMappingItemType = CollectObjectEnum.Balance;
			SwitchCurrentMappingDataSource(_balanceMapSettingData);
			break;
		case CollectObjectEnum.Summary:
			foreach (string key2 in CaptionDic.Summary.Keys)
			{
				C1Command c1Command4 = new C1Command();
				c1Command4.Text = ConvertMappingItemNameToDisplayName(key2, 0, 0);
				c1Command4.Click += Cmd_OnMappingItemClicked;
				c1Command4.UserData = key2;
				list.Add(new C1CommandLink(c1Command4));
			}
			_ctxMappingItemType = CollectObjectEnum.Summary;
			SwitchCurrentMappingDataSource(_monthSummaryMapSettingData);
			break;
		case CollectObjectEnum.Subsidiary:
			foreach (string key3 in CaptionDic.Subsidiary.Keys)
			{
				C1Command c1Command2 = new C1Command();
				c1Command2.Text = key3;
				c1Command2.Click += Cmd_OnMappingItemClicked;
				c1Command2.UserData = key3;
				if (SubsidiaryExcludeItemOnAllAccountSelected.Contains(key3))
				{
					c1Command2.CommandStateQuery += Cmd_SubsidiaryBalanceItemVisibleCommandStateQuery;
				}
				list.Add(new C1CommandLink(c1Command2));
			}
			_ctxMappingItemType = CollectObjectEnum.Subsidiary;
			SwitchCurrentMappingDataSource(_subsidiaryMapSettingData);
			break;
		default:
			_ctxMappingItemType = CollectObjectEnum.None;
			SwitchCurrentMappingDataSource(null);
			break;
		}
		_ctxMappingMenu.CommandLinks.AddRange(list);
	}

	private void Cmd_SubsidiaryBalanceItemVisibleCommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (checkBoxNotFilterAccount.Checked || checkBoxOnlyMyMark.Checked)
		{
			e.Visible = false;
		}
		else
		{
			e.Visible = true;
		}
	}

	private void SwitchCurrentMappingDataSource(List<TableColumnMappingData> target)
	{
		for (int j = 0; j < _tableHeaderRowsCount; j++)
		{
			int i;
			for (i = 0; i < _tableHeaderColumnsCount; i++)
			{
				TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(j, i);
				if (!tableHeaderSetting.IsMappingItemCell)
				{
					continue;
				}
				if (target == null)
				{
					tableHeaderSetting.ColumnMappingData = new TableColumnMappingData
					{
						IsMappingItemSet = false
					};
					continue;
				}
				tableHeaderSetting.ColumnMappingData = target.Where((TableColumnMappingData u) => u.ColumnIndex == i).FirstOrDefault();
				if (tableHeaderSetting.ColumnMappingData == null)
				{
					tableHeaderSetting.ColumnMappingData = new TableColumnMappingData
					{
						IsMappingItemSet = false
					};
				}
			}
		}
		_currentSelectedColumnMappingData = null;
	}

	private void Cmd_OnCancelMappingItemClicked(object sender, ClickEventArgs e)
	{
		if (_currentSelectedColumnMappingData != null)
		{
			_currentSelectedColumnMappingData.MappingItemName = null;
			_currentSelectedColumnMappingData.IsMappingItemSet = false;
			int columnIndex = _currentSelectedColumnMappingData.ColumnIndex;
			ChangeDataTableColumnData(columnIndex);
			CancelSort(columnIndex);
		}
	}

	private void Cmd_OnMappingItemClicked(object sender, ClickEventArgs e)
	{
		if (_currentSelectedColumnMappingData != null)
		{
			C1Command c1Command = (C1Command)sender;
			_currentSelectedColumnMappingData.MappingItemName = c1Command.UserData as string;
			_currentSelectedColumnMappingData.IsMappingItemSet = true;
			ChangeDataTableColumnData(_currentSelectedColumnMappingData.ColumnIndex);
		}
	}

	private void CancelSort()
	{
		_dataGrid.Sort(SortFlags.Ascending, 0);
		_dataGrid.Sort(SortFlags.None, 0);
	}

	private void CancelSort(int colIndex)
	{
		C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[colIndex];
		if (column.Sort != 0)
		{
			column.Sort = SortFlags.None;
			_dataGrid.Sort(SortFlags.Ascending, 0);
			_dataGrid.Sort(SortFlags.None, 0);
		}
	}

	private bool UpdateColSortFlag(int srcColIndex, int dstColIndex)
	{
		C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[srcColIndex];
		if (column.Sort != 0)
		{
			_dataGrid.Cols[dstColIndex].Sort = column.Sort;
			column.Sort = SortFlags.None;
			return true;
		}
		return false;
	}

	private bool UpdateColSortToNone(int colIndex)
	{
		C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[colIndex];
		if (column.Sort != 0)
		{
			column.Sort = SortFlags.None;
			return true;
		}
		return false;
	}

	private bool IsNeedCancelColumnSort(string colName, int colIndex)
	{
		if (Collector.CollectObject != CollectObjectEnum.Subsidiary)
		{
			return false;
		}
		C1.Win.C1FlexGrid.Column column = _dataGrid.Cols[colIndex];
		if (column.Sort == SortFlags.None)
		{
			return false;
		}
		switch (colName)
		{
		case "贷方金额":
		case "余额":
		case "借方金额":
			return false;
		default:
			if (column.Sort != 0)
			{
				column.Sort = SortFlags.None;
				return true;
			}
			return false;
		}
	}

	private void SortByColumnFlag(int colIndex)
	{
		_dataGrid.Sort(SortFlags.UseColSort, colIndex);
	}

	private void ReInitShouldSelectedItemByFilter()
	{
		if (Collector.Setting.CheckCollectItemShouldBeSelectedFilter == null)
		{
			return;
		}
		DettachDataGridCellCheckEvent();
		_dataGrid.BeginUpdate();
		try
		{
			int count = _dataGrid.Rows.Count;
			CollectItemShouldSelectFilter checkCollectItemShouldBeSelectedFilter = Collector.Setting.CheckCollectItemShouldBeSelectedFilter;
			for (int i = _tableHeaderRowsCount; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[i];
				DataGridRowUserData dataGridRowUserData = (DataGridRowUserData)row.UserData;
				Account account = null;
				Voucher voucher = null;
				if (dataGridRowUserData.DataType == DataGridRowDataType.Account)
				{
					account = dataGridRowUserData.AccountData;
				}
				else if (dataGridRowUserData.DataType == DataGridRowDataType.AuxiliaryItem)
				{
					account = dataGridRowUserData.AuxiliaryData.Item1;
				}
				else if (dataGridRowUserData.DataType == DataGridRowDataType.Voucher)
				{
					voucher = dataGridRowUserData.VoucherData;
				}
				bool flag = false;
				if (account != null && checkCollectItemShouldBeSelectedFilter.IsAccountShouldBeSelected(account))
				{
					flag = true;
				}
				if (voucher != null && checkCollectItemShouldBeSelectedFilter.IsVoucherShouldBeSelected(voucher))
				{
					flag = true;
				}
				_dataGrid.SetCellCheck(i, 1, flag ? CheckEnum.Checked : CheckEnum.Unchecked);
				row.StyleNew.BackColor = (flag ? DefaultBackgroundColor : HighLightColor);
			}
		}
		finally
		{
			_dataGrid.EndUpdate();
			AttachDataGridCellCheckEvent();
		}
		Collector.Setting.CheckCollectItemShouldBeSelectedFilter = null;
	}

	private void UnSelectEmptyAccount()
	{
		if (!Collector.IsNeedRunEmptyAccountCheckToSetSelectStatus || !Collector.Setting.IsCancelEmptyAccountSelectStatus)
		{
			return;
		}
		DettachDataGridCellCheckEvent();
		_dataGrid.BeginUpdate();
		try
		{
			int count = _dataGrid.Rows.Count;
			for (int i = _tableHeaderRowsCount; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[i];
				DataGridRowUserData dataGridRowUserData = (DataGridRowUserData)row.UserData;
				if (dataGridRowUserData.DataType == DataGridRowDataType.Account)
				{
					Account accountData = dataGridRowUserData.AccountData;
					if (_dataGrid.GetCellCheck(i, 1) == CheckEnum.Checked && Collector.IsEmptyAccountWithCache(accountData))
					{
						_dataGrid.SetCellCheck(i, 1, CheckEnum.Unchecked);
						row.StyleNew.BackColor = HighLightColor;
					}
				}
			}
		}
		finally
		{
			_dataGrid.EndUpdate();
			AttachDataGridCellCheckEvent();
		}
	}

	private void RefreshTableView(bool isAdjustColAutoWidth = false, bool isDisplayMappingItemInSingleRow = false)
	{
		_dataGrid.FilterManager.Clear();
		_dataGrid.BeginUpdate();
		try
		{
			PopulateDataTable();
			if (isAdjustColAutoWidth)
			{
				AutoResizeTableColumnWidth();
			}
			if (isDisplayMappingItemInSingleRow)
			{
				AdjustAllTableColumnWidthToDisplayMappingItemNameInSingleRow();
			}
			AutoAdjustTableHeaderDisplaySetting();
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void InitTableColumnMappingData()
	{
		ClearMappingSet(_balanceMapSettingData);
		ClearMappingSet(_subsidiaryMapSettingData);
		ClearMappingSet(_monthSummaryMapSettingData);
		ClearMappingSet(_ledgerAgeMappSettingData);
		List<TableColumnMappingData> list2 = null;
		switch (Collector.CollectObject)
		{
		default:
			return;
		case CollectObjectEnum.Balance:
			list2 = _balanceMapSettingData;
			break;
		case CollectObjectEnum.Subsidiary:
			list2 = _subsidiaryMapSettingData;
			break;
		case CollectObjectEnum.Summary:
			list2 = _monthSummaryMapSettingData;
			break;
		case CollectObjectEnum.None:
			return;
		}
		foreach (TableColumnMappingData item in list2)
		{
			if (Collector.Maps.TryGetValue(item.TableColumnId.Value, out var value))
			{
				item.IsMappingItemSet = true;
				item.MappingItemName = value;
			}
		}
		FindOutWhichColunmNeedUsePositiveValue(list2);
		static void ClearMappingSet(List<TableColumnMappingData> list)
		{
			foreach (TableColumnMappingData item2 in list)
			{
				item2.IsMappingItemSet = false;
				item2.MappingItemName = null;
				item2.IsUsePositiveValue = false;
			}
		}
	}

	private void FindOutCurrentColumnMapWhichNeedUsePositiveValue(List<int> changedColList = null)
	{
		List<TableColumnMappingData> list = new List<TableColumnMappingData>();
		for (int i = 4; i < _tableHeaderColumnsCount; i++)
		{
			TableColumnMappingData columnMappingData = GetTableHeaderSetting(_tableHeaderRowsCount - 1, i).ColumnMappingData;
			if (columnMappingData != null)
			{
				list.Add(columnMappingData);
			}
		}
		FindOutWhichColunmNeedUsePositiveValue(list, changedColList);
	}

	private void FindOutWhichColunmNeedUsePositiveValue(List<TableColumnMappingData> list, List<int> changedColList = null)
	{
		if (list == null)
		{
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (TableColumnMappingData item in list)
		{
			if (item.IsMappingItemSet && !dictionary.ContainsKey(item.MappingItemName))
			{
				dictionary.Add(item.MappingItemName, null);
			}
		}
		OppositeCaptionNameDic oppositeCaptionNameDic = null;
		if (Collector.CollectObject == CollectObjectEnum.Balance)
		{
			oppositeCaptionNameDic = OppositeCaptionDic.Balance;
		}
		if (oppositeCaptionNameDic == null)
		{
			return;
		}
		foreach (TableColumnMappingData item2 in list)
		{
			if (item2.IsMappingItemSet)
			{
				bool isUsePositiveValue = item2.IsUsePositiveValue;
				bool flag = false;
				if (oppositeCaptionNameDic.TryGetValue(item2.MappingItemName, out var value) && dictionary.ContainsKey(item2.MappingItemName) && dictionary.ContainsKey(value))
				{
					flag = true;
				}
				item2.IsUsePositiveValue = flag;
				if (changedColList != null && isUsePositiveValue != flag)
				{
					changedColList.Add(item2.ColumnIndex);
				}
			}
		}
	}

	private void PrepareMyMarkAccountTreeNodeData()
	{
		if (_myMarkAccountTreeRootNode == null)
		{
			_myMarkAccountTreeRootNode = BuildAccountTreeFromMyMark();
			ParseTreeNodeAuxiliaryData(_myMarkAccountTreeRootNode);
			SortAccountTreeNode(_myMarkAccountTreeRootNode);
		}
		VoucherAccountTreeNode BuildAccountTreeFromMyMark()
		{
			VoucherAccountTreeNode voucherAccountTreeNode = new VoucherAccountTreeNode
			{
				Children = new List<VoucherAccountTreeNode>()
			};
			Dictionary<Account, VoucherAccountTreeNode> dictionary2 = new Dictionary<Account, VoucherAccountTreeNode>();
			foreach (Voucher voucher in _ledger.Vouchers)
			{
				if (voucher.VoucherMark)
				{
					VoucherAccountTreeNode voucherAccountTreeNode2 = new VoucherAccountTreeNode
					{
						nodeData = voucher
					};
					for (Account account4 = voucher.Account; account4 != null; account4 = account4.Parent)
					{
						if (dictionary2.TryGetValue(account4, out var value2))
						{
							if (value2.Children == null)
							{
								value2.Children = new List<VoucherAccountTreeNode>();
							}
							value2.Children.Add(voucherAccountTreeNode2);
							voucherAccountTreeNode2.parentNode = value2;
							voucherAccountTreeNode2 = null;
							break;
						}
						value2 = new VoucherAccountTreeNode
						{
							nodeData = account4
						};
						value2.Children = new List<VoucherAccountTreeNode> { voucherAccountTreeNode2 };
						voucherAccountTreeNode2.parentNode = value2;
						dictionary2.Add(account4, value2);
						voucherAccountTreeNode2 = value2;
					}
					if (voucherAccountTreeNode2 != null)
					{
						voucherAccountTreeNode.Children.Add(voucherAccountTreeNode2);
						voucherAccountTreeNode2.parentNode = voucherAccountTreeNode;
						if (voucherAccountTreeNode2.nodeData != null && voucherAccountTreeNode2.nodeData is Account key && !dictionary2.ContainsKey(key))
						{
							dictionary2.Add(key, voucherAccountTreeNode2);
						}
					}
				}
			}
			_myMarkAccountDataSet = dictionary2;
			return voucherAccountTreeNode;
		}
		static void ParseTreeNodeAuxiliaryData(VoucherAccountTreeNode node)
		{
			if (node.nodeData == null)
			{
				foreach (VoucherAccountTreeNode child in node.Children)
				{
					ParseTreeNodeAuxiliaryData(child);
				}
				return;
			}
			if (node.Children != null)
			{
				foreach (VoucherAccountTreeNode child2 in node.Children)
				{
					ParseTreeNodeAuxiliaryData(child2);
				}
				List<Voucher> list = null;
				foreach (VoucherAccountTreeNode child3 in node.Children)
				{
					Account account3 = child3.nodeData as Account;
					if (account3 == null && child3.nodeData is Voucher item)
					{
						if (list == null)
						{
							list = new List<Voucher>();
						}
						list.Add(item);
					}
				}
				if (list != null)
				{
					Dictionary<AuxiliaryClass, HashSet<AuxiliaryItem>> dictionary = new Dictionary<AuxiliaryClass, HashSet<AuxiliaryItem>>();
					foreach (Voucher item3 in list)
					{
						if (item3.Details != null)
						{
							foreach (AuxiliaryItem detail in item3.Details)
							{
								if (!dictionary.TryGetValue(detail.Class, out var value))
								{
									dictionary.Add(detail.Class, new HashSet<AuxiliaryItem> { detail });
								}
								else
								{
									value.Add(detail);
								}
							}
						}
					}
					node.auxClassList = new List<Tuple<AuxiliaryClass, List<AuxiliaryItem>>>();
					IOrderedEnumerable<AuxiliaryClass> orderedEnumerable = from u in dictionary.Keys.ToList()
						orderby u.Code
						select u;
					foreach (AuxiliaryClass item4 in orderedEnumerable)
					{
						List<AuxiliaryItem> item2 = new List<AuxiliaryItem>(dictionary[item4]).OrderBy((AuxiliaryItem u) => u.Code).ToList();
						node.auxClassList.Add(Tuple.Create(item4, item2));
					}
				}
			}
		}
		static void SortAccountTreeNode(VoucherAccountTreeNode node)
		{
			if (node.Children == null || node.Children.Count <= 1)
			{
				return;
			}
			node.Children.Sort(delegate(VoucherAccountTreeNode left, VoucherAccountTreeNode right)
			{
				Account account = left.nodeData as Account;
				Account account2 = right.nodeData as Account;
				if (account == null && account2 == null)
				{
					return 0;
				}
				if (account == null && account2 != null)
				{
					return -1;
				}
				return (account != null && account2 == null) ? 1 : account.Code.CompareTo(account2.Code);
			});
			foreach (VoucherAccountTreeNode child4 in node.Children)
			{
				SortAccountTreeNode(child4);
			}
		}
	}

	private void PopulateAccountTreeByMyMark()
	{
		PrepareMyMarkAccountTreeNodeData();
		comboAccountTree.SelectedIndex = -1;
		comboAccountTree.Text = string.Empty;
		comboAccountTree.TreeView.Nodes.Clear();
		comboAccountTree.TreeView.BeginUpdate();
		try
		{
			foreach (VoucherAccountTreeNode child in _myMarkAccountTreeRootNode.Children)
			{
				if (child.nodeData is Account account)
				{
					string text = account.Code + " " + account.Name;
					System.Windows.Forms.TreeNode treeNode = comboAccountTree.TreeView.Nodes.Add(text);
					treeNode.Tag = account;
					AddComboxAccountTreeChild(child, treeNode);
				}
			}
		}
		finally
		{
			comboAccountTree.TreeView.EndUpdate();
		}
		static void AddComboxAccountTreeChild(VoucherAccountTreeNode parentNode, System.Windows.Forms.TreeNode node)
		{
			if (parentNode.Children == null || parentNode.Children.Count == 0)
			{
				return;
			}
			foreach (VoucherAccountTreeNode child2 in parentNode.Children)
			{
				if (child2.nodeData is Account account2)
				{
					string text2 = account2.Code + " " + account2.Name;
					System.Windows.Forms.TreeNode treeNode2 = node.Nodes.Add(text2);
					treeNode2.Tag = account2;
					AddComboxAccountTreeChild(child2, treeNode2);
				}
			}
		}
	}

	private void PopulateAccountTree(Ledger ledger)
	{
		comboAccountTree.SelectedIndex = -1;
		comboAccountTree.Text = string.Empty;
		comboAccountTree.TreeView.Nodes.Clear();
		if (ledger == null)
		{
			return;
		}
		comboAccountTree.TreeView.BeginUpdate();
		try
		{
			foreach (Account rootAccount in ledger.RootAccounts)
			{
				string text = rootAccount.Code + " " + rootAccount.Name;
				System.Windows.Forms.TreeNode treeNode = comboAccountTree.TreeView.Nodes.Add(text);
				treeNode.Tag = rootAccount;
				addChildren(rootAccount, treeNode);
			}
		}
		finally
		{
			comboAccountTree.TreeView.EndUpdate();
		}
		static void addChildren(Account parent, System.Windows.Forms.TreeNode node)
		{
			foreach (Account child in parent.Children)
			{
				string text2 = child.Code + " " + child.Name;
				System.Windows.Forms.TreeNode treeNode2 = node.Nodes.Add(text2);
				treeNode2.Tag = child;
				addChildren(child, treeNode2);
			}
		}
	}

	private void PopulateAuxiliaryTree(Account account)
	{
		comboAuxiliaryTree.SelectedIndex = -1;
		comboAuxiliaryTree.Text = string.Empty;
		comboAuxiliaryTree.TreeView.Nodes.Clear();
		comboAuxiliaryTree.Enabled = false;
		if (account == null)
		{
			return;
		}
		comboAuxiliaryTree.TreeView.BeginUpdate();
		try
		{
			if (checkBoxOnlyMyMark.Checked)
			{
				PrepareMyMarkAccountTreeNodeData();
				if (_myMarkAccountDataSet.TryGetValue(account, out var value) && value.auxClassList != null && value.auxClassList.Count > 0)
				{
					foreach (Tuple<AuxiliaryClass, List<AuxiliaryItem>> auxClass in value.auxClassList)
					{
						AuxiliaryClass item = auxClass.Item1;
						string text = item.Name ?? "";
						System.Windows.Forms.TreeNode treeNode = comboAuxiliaryTree.TreeView.Nodes.Add(text);
						treeNode.Tag = Tuple.Create(account, item);
						if (comboCollectObject.SelectedIndex != 1)
						{
							continue;
						}
						foreach (AuxiliaryItem item2 in auxClass.Item2)
						{
							string text2 = item2.ToString();
							System.Windows.Forms.TreeNode treeNode2 = treeNode.Nodes.Add(text2);
							treeNode2.Tag = Tuple.Create(account, item2);
						}
					}
				}
			}
			else
			{
				TrialBalanceSheet trialBalanceSheet = Collector.Setting.Ledger.GetTrialBalanceSheet(selectedStartDate(), selectedEndDate());
				AccountBalance value2 = trialBalanceSheet.End.FirstOrDefault((KeyValuePair<Account, AccountBalance> t) => t.Key == account).Value;
				if (value2 == null)
				{
					return;
				}
				foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in value2.ClassBalances)
				{
					AuxiliaryClass key = classBalance.Key;
					Dictionary<AuxiliaryItem, decimal> itemBalances = classBalance.Value.ItemBalances;
					string text3 = key.ToString();
					System.Windows.Forms.TreeNode treeNode3 = comboAuxiliaryTree.TreeView.Nodes.Add(text3);
					treeNode3.Tag = Tuple.Create(account, key);
					if (comboCollectObject.SelectedIndex != 1)
					{
						continue;
					}
					foreach (KeyValuePair<AuxiliaryItem, decimal> item3 in itemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
					{
						AuxiliaryItem key2 = item3.Key;
						string text4 = key2.ToString();
						System.Windows.Forms.TreeNode treeNode4 = treeNode3.Nodes.Add(text4);
						treeNode4.Tag = Tuple.Create(account, key2);
					}
				}
			}
			comboAuxiliaryTree.Enabled = comboAuxiliaryTree.TreeView.Nodes.Count > 0 && !checkBoxNotFilterAccount.Checked;
		}
		finally
		{
			comboAuxiliaryTree.TreeView.EndUpdate();
		}
	}

	private void ReInitComponentValue()
	{
		DettachEvent();
		try
		{
			_tableDataSourceColumnNameTable = null;
			bool flag = false;
			bool enabled = true;
			bool @checked = false;
			bool enabled2 = true;
			bool flag2 = false;
			bool enabled3 = true;
			switch (Collector.CollectObject)
			{
			case CollectObjectEnum.Balance:
				SelectCollectObject(CollectObjectEnum.Balance);
				spbAnalysis.Visible = false;
				checkBoxAllAccount.Visible = true;
				checkBoxOnlyMyMark.Visible = false;
				checkBoxNotFilterAccount.Visible = false;
				flag = Collector.Setting.CollectAllAccount;
				enabled = !flag;
				SetVisibleAuxiliary(visble: false);
				break;
			case CollectObjectEnum.Subsidiary:
				SelectCollectObject(CollectObjectEnum.Subsidiary);
				spbAnalysis.Visible = false;
				checkBoxAllAccount.Visible = false;
				checkBoxOnlyMyMark.Visible = true;
				checkBoxNotFilterAccount.Visible = true;
				flag2 = Collector.Setting.CollectAllAccount;
				enabled3 = true;
				enabled = !flag2;
				enabled2 = true;
				@checked = Collector.Setting.IsOnlyMyMark;
				SetVisibleAuxiliary(visble: true);
				break;
			case CollectObjectEnum.Summary:
				SelectCollectObject(CollectObjectEnum.Summary);
				spbAnalysis.SelectItem((Collector as TableCollectorSummary).AnalysisProject);
				spbAnalysis.Visible = true;
				checkBoxAllAccount.Visible = false;
				checkBoxOnlyMyMark.Visible = false;
				checkBoxNotFilterAccount.Visible = false;
				SetVisibleAuxiliary(visble: false);
				break;
			}
			if (Collector.CollectObject == CollectObjectEnum.Subsidiary)
			{
				lblKjqj.Visible = false;
				lblStartMonth.Visible = false;
				lblEndMonth.Visible = false;
				comboStartMonth.Visible = false;
				comboEndMonth.Visible = false;
			}
			else
			{
				lblKjqj.Visible = true;
				lblStartMonth.Visible = true;
				lblEndMonth.Visible = true;
				comboStartMonth.Visible = true;
				comboEndMonth.Visible = true;
			}
			if (_isDisableCollectObjectCombox)
			{
				comboCollectObject.Enabled = false;
			}
			DateTime start = Collector.Setting.Start;
			DateTime end = Collector.Setting.End;
			comboStartMonth.Text = ((start == default(DateTime)) ? "1" : start.Month.ToString());
			comboEndMonth.Text = ((end == default(DateTime)) ? "12" : end.Month.ToString());
			checkBoxAllAccount.Checked = flag;
			comboAccountTree.Enabled = enabled;
			checkBoxNotFilterAccount.Checked = flag2;
			checkBoxNotFilterAccount.Enabled = enabled3;
			checkBoxOnlyMyMark.Checked = @checked;
			checkBoxOnlyMyMark.Enabled = enabled2;
			ReConfirmSelectWhichDetailAccount();
			SelectAccountNode(Collector.Setting.Account);
			PopulateAuxiliaryTree(Collector.Setting.Account);
			SelectAuxiliaryNode(Collector.Setting.Auxiliary);
		}
		finally
		{
			AttachEvent();
		}
		ResetCurrentSelectedAccountNode();
	}

	private void ReConfirmSelectWhichDetailAccount()
	{
		if (!Collector.Setting.IsNeedSelectDetailAccount)
		{
			return;
		}
		Collector.Setting.IsNeedSelectDetailAccount = false;
		List<Account> subAccountsList;
		TrialBalanceSheet balanceSheet;
		if (Collector.Setting.Account != null)
		{
			subAccountsList = GetAllSubAccount(Collector.Setting.Account);
			if (subAccountsList == null)
			{
				subAccountsList = new List<Account>();
			}
			subAccountsList.Add(Collector.Setting.Account);
			balanceSheet = null;
			IsFindOutNeedSelectedDetailItem(_detailAccountNameCheckList);
		}
		bool IsFindOutNeedSelectedDetailItem(List<string> accoutNameCheckList)
		{
			if (accoutNameCheckList == null || accoutNameCheckList.Count == 0)
			{
				return false;
			}
			Account account2 = subAccountsList.FirstOrDefault((Account a) => accoutNameCheckList.Contains(a.Name));
			if (account2 != null)
			{
				Collector.Setting.Account = account2;
				Collector.Setting.Auxiliary = null;
				return true;
			}
			if (balanceSheet == null)
			{
				balanceSheet = Collector.Setting.Ledger.GetTrialBalanceSheet(selectedStartDate(), selectedEndDate());
			}
			foreach (Account account in subAccountsList)
			{
				try
				{
					AccountBalance value = balanceSheet.End.FirstOrDefault((KeyValuePair<Account, AccountBalance> t) => t.Key == account).Value;
					if (value != null)
					{
						foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in value.ClassBalances)
						{
							AuxiliaryClass key = classBalance.Key;
							Dictionary<AuxiliaryItem, decimal> itemBalances = classBalance.Value.ItemBalances;
							string text = key.ToString();
							foreach (KeyValuePair<AuxiliaryItem, decimal> item in itemBalances)
							{
								AuxiliaryItem key2 = item.Key;
								string name = key2.Name;
								if (accoutNameCheckList.Contains(name))
								{
									Collector.Setting.Account = account;
									Collector.Setting.Auxiliary = key2;
									return true;
								}
							}
						}
					}
				}
				catch (Exception exception)
				{
					exception.Log("在辅助核算项目中查找明细科目时发生了未预期的异常");
				}
			}
			return false;
		}
	}

	private List<Account> GetAllSubAccount(Account account)
	{
		System.Windows.Forms.TreeNode treeNodeByAccount = GetTreeNodeByAccount(comboAccountTree.TreeView.Nodes, account);
		if (treeNodeByAccount == null)
		{
			return null;
		}
		List<Account> list = new List<Account>();
		AllChildrenAccount(treeNodeByAccount.Nodes, list);
		return list;
	}

	private void AllChildrenAccount(TreeNodeCollection Nodes, List<Account> list)
	{
		foreach (System.Windows.Forms.TreeNode Node in Nodes)
		{
			if (Node.Tag is Account item)
			{
				list.Add(item);
			}
			AllChildrenAccount(Node.Nodes, list);
		}
	}

	private System.Windows.Forms.TreeNode GetTreeNodeByAccount(TreeNodeCollection nodes, Account account)
	{
		foreach (System.Windows.Forms.TreeNode node in nodes)
		{
			if (node.Tag is Account account2 && account2 == account)
			{
				return node;
			}
			System.Windows.Forms.TreeNode treeNodeByAccount = GetTreeNodeByAccount(node.Nodes, account);
			if (treeNodeByAccount != null)
			{
				return node;
			}
		}
		return null;
	}

	private void InsertAccountDataAfterRow(Account account, List<object> collectTargetList, int gridTableRowIndex)
	{
		if (collectTargetList.Count == 0)
		{
			return;
		}
		TableCollectorAbstract tableCollectorAbstract = null;
		if (Collector.CollectObject == CollectObjectEnum.Balance)
		{
			_change_banlance_level_collector.AccNameStyle = ((TableCollectorBalance)Collector).AccNameStyle;
			tableCollectorAbstract = _change_banlance_level_collector;
		}
		else
		{
			if (Collector.CollectObject != CollectObjectEnum.Summary)
			{
				return;
			}
			_change_summary_level_collector.AnalysisProject = selectedSpbAnalysis();
			tableCollectorAbstract = _change_summary_level_collector;
		}
		tableCollectorAbstract.Source = collectTargetList;
		tableCollectorAbstract.Setting.Account = account;
		tableCollectorAbstract.Setting.Start = Collector.Setting.Start;
		tableCollectorAbstract.Setting.End = Collector.Setting.End;
		tableCollectorAbstract.Setting.CollectAllAccount = checkBoxAllAccount.Checked;
		tableCollectorAbstract.Setting.Table = _table;
		tableCollectorAbstract.Setting.Ledger = _ledger;
		tableCollectorAbstract.IsSaveCollectResultByColumnId = true;
		tableCollectorAbstract.Maps.Clear();
		DataTable dataTable = new DataTable();
		CreateSearchedColumnIDNameMap(Collector.CollectObject, dataTable, tableCollectorAbstract.Maps);
		List<DataRow> valueList = ExecuteCollect(tableCollectorAbstract, dataTable);
		InsertDataToTableAfterRow(gridTableRowIndex, collectTargetList, valueList);
	}

	private void ResetDataGridOrderNumber()
	{
		int count = _dataGrid.Rows.Count;
		for (int i = _tableHeaderRowsCount; i < count; i++)
		{
			_dataGrid.SetData(i, 0, i - _tableHeaderRowsCount + 1);
		}
	}

	private void RemoveDataTableRow(List<C1.Win.C1FlexGrid.Row> rowList)
	{
		List<int> list = (from u in rowList
			select u.Index into u
			orderby u
			select u).ToList();
		_dataGrid.FilterManager.Clear();
		_dataGrid.BeginUpdate();
		try
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				_dataGrid.Rows.Remove(list[num]);
			}
			ResetDataGridOrderNumber();
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void RemoveDataTableRow(int gridTableRowIndex, int rowCount = 1)
	{
		if (gridTableRowIndex < _tableHeaderRowsCount || gridTableRowIndex >= _dataGrid.Rows.Count)
		{
			return;
		}
		int num = Math.Min(gridTableRowIndex + rowCount, _dataGrid.Rows.Count);
		int num2 = num - gridTableRowIndex;
		if (num2 <= 0)
		{
			return;
		}
		_dataGrid.FilterManager.Clear();
		_dataGrid.BeginUpdate();
		try
		{
			_dataGrid.Rows.RemoveRange(gridTableRowIndex, num2);
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void InsertDataToTableAfterRow(int gridTableRowIndex, List<object> collectTargetList, List<DataRow> valueList)
	{
		int count = collectTargetList.Count;
		if (count == 0)
		{
			return;
		}
		_dataGrid.FilterManager.Clear();
		_dataGrid.BeginUpdate();
		try
		{
			_dataGrid.Rows.InsertRange(gridTableRowIndex, count);
			CollectObjectEnum collectObjectEnum = selectCollectObject();
			for (int i = 0; i < count; i++)
			{
				int num = gridTableRowIndex + i;
				DataRow dataRow = valueList[i];
				C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[num];
				object rowData = collectTargetList[i];
				row.UserData = new DataGridRowUserData(rowData, dataRow, collectObjectEnum);
				UpdateRowDataBySelectedObject(collectObjectEnum, dataRow, rowData, num);
				UpdateRowDataByMappingData(dataRow, num);
			}
			ResetDataGridOrderNumber();
			AutoAdjustTableHeaderDisplaySetting();
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private List<DataRow> ExecuteCollect(TableCollectorAbstract collector, DataTable colNameDataTable, HashSet<int> outUnExistColIdSet = null)
	{
		TableCollectResult tableCollectResult = collector.Collect(collector.Setting.Start.Year);
		outUnExistColIdSet?.Clear();
		int count = colNameDataTable.Columns.Count;
		if (tableCollectResult.ValuesOnColumn.Count == 0)
		{
			if (outUnExistColIdSet != null)
			{
				for (int i = 0; i < count; i++)
				{
					outUnExistColIdSet.Add(i);
				}
			}
			return new List<DataRow>();
		}
		int columnIndex = colNameDataTable.Columns.IndexOf("UserData");
		int capacity = collector.Source.Count();
		List<DataRow> list = new List<DataRow>(capacity);
		foreach (object item in collector.Source)
		{
			DataRow dataRow = colNameDataTable.NewRow();
			dataRow[columnIndex] = item;
			list.Add(dataRow);
		}
		for (int j = 0; j < count; j++)
		{
			if (!tableCollectResult.ValuesOnColumn.TryGetValue(j, out var value))
			{
				outUnExistColIdSet?.Add(j);
				continue;
			}
			for (int k = 0; k < value.Count; k++)
			{
				object obj = value[k];
				list[k][j] = ((obj == null) ? DBNull.Value : obj);
			}
		}
		return list;
	}

	private void CreateSearchedColumnIDNameMap(CollectObjectEnum selectObject, DataTable dataTable, Dictionary<long, string> outIdNameMapper)
	{
		bool flag = false;
		CaptionTypeDic captionTypeDic = null;
		switch (selectObject)
		{
		default:
			return;
		case CollectObjectEnum.Balance:
			captionTypeDic = CaptionDic.Balance;
			break;
		case CollectObjectEnum.Subsidiary:
			captionTypeDic = CaptionDic.Subsidiary;
			break;
		case CollectObjectEnum.Summary:
			captionTypeDic = CaptionDic.Summary;
			flag = true;
			break;
		case CollectObjectEnum.None:
			return;
		}
		foreach (string key in captionTypeDic.Keys)
		{
			DataColumn dataColumn = dataTable.Columns.Add(key);
			dataColumn.DataType = captionTypeDic[key];
			outIdNameMapper.Add(dataTable.Columns.Count - 1, key);
		}
		if (flag && dataTable.Columns.IndexOf("科目代码") == -1)
		{
			DataColumn dataColumn2 = dataTable.Columns.Add("科目代码");
			dataColumn2.DataType = typeof(string);
			outIdNameMapper.Add(dataTable.Columns.Count - 1, "科目代码");
		}
		DataColumn dataColumn3 = dataTable.Columns.Add("UserData");
		dataColumn3.DataType = typeof(object);
	}

	private bool UpdateCollectorSearchConditionByUIInput(TableCollectorAbstract collector)
	{
		switch (collector.CollectObject)
		{
		case CollectObjectEnum.Balance:
			Collector.Setting.Account = selectedAccount();
			Collector.Setting.CollectAllAccount = checkBoxAllAccount.Checked;
			if (!checkBoxAllAccount.Checked && Collector.Setting.Account == null)
			{
				return false;
			}
			break;
		case CollectObjectEnum.Subsidiary:
			Collector.Setting.Account = selectedAccount();
			Collector.Setting.CollectAllAccount = checkBoxNotFilterAccount.Checked;
			Collector.Setting.IsOnlyMyMark = checkBoxOnlyMyMark.Checked;
			if (Collector.Setting.Account == null && !Collector.Setting.CollectAllAccount && !Collector.Setting.IsOnlyMyMark)
			{
				return false;
			}
			break;
		case CollectObjectEnum.Summary:
			(Collector as TableCollectorSummary).AnalysisProject = selectedSpbAnalysis();
			Collector.Setting.CollectAllAccount = false;
			Collector.Setting.Account = selectedAccount();
			if (Collector.Setting.Account == null)
			{
				return false;
			}
			break;
		default:
			return false;
		}
		return true;
	}

	private void ReCollectData(List<object> collectTargetList = null, SubAccountFilterMode subAccountFilterMode = SubAccountFilterMode.OnlyChildAccountAndAuxiliaryItem)
	{
		Collector.Maps.Clear();
		_tableDataSourceColumnNameTable = new DataTable();
		_recollect_data_list.Clear();
		CollectObjectEnum selectObject = selectCollectObject();
		if (UpdateCollectorSearchConditionByUIInput(Collector))
		{
			CreateSearchedColumnIDNameMap(selectObject, _tableDataSourceColumnNameTable, Collector.Maps);
			Collector.Setting.Auxiliary = selectedAuxiliary();
			Collector.IsSaveCollectResultByColumnId = true;
			Collector.Source = collectTargetList;
			Collector.Setting.SubAccountFitlerMode = subAccountFilterMode;
			_recollect_data_list = ExecuteCollect(Collector, _tableDataSourceColumnNameTable, _has_no_data_in_ledger_columns_index_set);
		}
	}

	private void ChangeDataTableColumnData(int gridCol)
	{
		_dataGrid.BeginUpdate();
		try
		{
			TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, gridCol);
			TableColumnMappingData columnMappingData = tableHeaderSetting.ColumnMappingData;
			if (columnMappingData == null)
			{
				return;
			}
			bool flag = false;
			bool flag2 = IsNeedCancelColumnSort(columnMappingData.MappingItemName, gridCol);
			List<int> list = new List<int> { gridCol };
			if (!columnMappingData.IsMappingItemSet)
			{
				columnMappingData.DataTableColIndex = -1;
			}
			else
			{
				columnMappingData.DataTableColIndex = _tableDataSourceColumnNameTable.Columns.IndexOf(columnMappingData.MappingItemName);
				int count = _dataGrid.Cols.Count;
				for (int i = 4; i < count; i++)
				{
					if (i != gridCol)
					{
						TableHeaderCellSetting tableHeaderSetting2 = GetTableHeaderSetting(_tableHeaderRowsCount - 1, i);
						if (tableHeaderSetting2.ColumnMappingData != null && tableHeaderSetting2.ColumnMappingData.MappingItemName == columnMappingData.MappingItemName)
						{
							tableHeaderSetting2.ColumnMappingData.IsMappingItemSet = false;
							tableHeaderSetting2.ColumnMappingData.DataTableColIndex = -1;
							tableHeaderSetting2.ColumnMappingData.MappingItemName = null;
							list.Add(i);
							bool flag3 = UpdateColSortFlag(i, gridCol);
							flag = flag || flag3;
						}
					}
				}
			}
			if (Collector.CollectObject == CollectObjectEnum.Balance)
			{
				FindOutCurrentColumnMapWhichNeedUsePositiveValue(list);
			}
			list = list.Distinct().ToList();
			UpdateColumnDataType(_dataGrid.Cols[gridCol], columnMappingData.MappingItemName);
			int count2 = _dataGrid.Rows.Count;
			foreach (int item in list)
			{
				TableHeaderCellSetting tableHeaderSetting3 = GetTableHeaderSetting(_tableHeaderRowsCount - 1, item);
				if (tableHeaderSetting3.ColumnMappingData == null || !tableHeaderSetting3.ColumnMappingData.IsMappingItemSet)
				{
					for (int j = _tableHeaderRowsCount; j < count2; j++)
					{
						_dataGrid.SetData(j, item, null);
					}
					continue;
				}
				TableColumnMappingData columnMappingData2 = tableHeaderSetting3.ColumnMappingData;
				bool flag4 = false;
				if (columnMappingData2.IsUsePositiveValue)
				{
					flag4 = true;
				}
				for (int k = _tableHeaderRowsCount; k < count2; k++)
				{
					if (columnMappingData2.DataTableColIndex == -1)
					{
						_dataGrid.SetData(k, item, null);
						continue;
					}
					object obj = ((DataGridRowUserData)_dataGrid.Rows[k].UserData).DataSource[columnMappingData2.DataTableColIndex];
					if (flag4 && obj != null && obj is decimal num && num < 0m)
					{
						obj = null;
					}
					_dataGrid.SetData(k, item, obj);
				}
			}
			if (flag)
			{
				SortByColumnFlag(gridCol);
			}
			if (flag2)
			{
				CancelSort();
			}
			AdjustTableColumnWidthToDisplayMappingItemNameInSingleRow(gridCol);
			AutoAdjustTableHeaderDisplaySetting();
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void PopulateDataTable()
	{
		_dataGrid.Rows.Count = _tableHeaderRowsCount;
		_dataGrid.Rows.Fixed = _tableHeaderRowsCount;
		if (_tableDataSourceColumnNameTable == null || _recollect_data_list.Count == 0)
		{
			return;
		}
		List<TableColumnMappingData> list = new List<TableColumnMappingData>();
		for (int i = 4; i < _tableHeaderColumnsCount; i++)
		{
			TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, i);
			TableColumnMappingData columnMappingData = tableHeaderSetting.ColumnMappingData;
			list.Add(columnMappingData);
			if (columnMappingData != null)
			{
				columnMappingData.DataTableColIndex = -1;
				if (columnMappingData.IsMappingItemSet)
				{
					columnMappingData.DataTableColIndex = _tableDataSourceColumnNameTable.Columns.IndexOf(columnMappingData.MappingItemName);
					UpdateColumnDataType(_dataGrid.Cols[i], columnMappingData.MappingItemName);
				}
			}
		}
		CollectObjectEnum collectObjectEnum = selectCollectObject();
		int columnIndex = _tableDataSourceColumnNameTable.Columns.IndexOf("UserData");
		int count = _recollect_data_list.Count;
		_dataGrid.Rows.Add(count);
		for (int j = 0; j < count; j++)
		{
			int num = _tableHeaderRowsCount + j;
			DataRow dataRow = _recollect_data_list[j];
			object rowData = dataRow[columnIndex];
			C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[num];
			row.UserData = new DataGridRowUserData(rowData, dataRow, collectObjectEnum);
			_dataGrid.SetData(num, 0, j + 1);
			UpdateRowDataBySelectedObject(collectObjectEnum, dataRow, rowData, num);
			UpdateRowDataByMappingData(dataRow, num);
		}
	}

	private void UpdateColumnDataType(C1.Win.C1FlexGrid.Column column, string colName)
	{
		if (colName != null)
		{
			switch (colName.Length)
			{
			case 4:
			{
				char c = colName[2];
				if (c != '代')
				{
					if (c != '名')
					{
						if (c != '科' || !(colName == "对方科目"))
						{
							break;
						}
					}
					else if (!(colName == "科目名称"))
					{
						break;
					}
				}
				else if (!(colName == "科目代码"))
				{
					break;
				}
				goto IL_0153;
			}
			case 2:
			{
				char c = colName[0];
				if ((uint)c <= 25688u)
				{
					if (c == '字')
					{
						if (!(colName == "字号"))
						{
							break;
						}
						goto IL_0183;
					}
					if (c != '摘' || !(colName == "摘要"))
					{
						break;
					}
				}
				else
				{
					if (c == '方')
					{
						if (!(colName == "方向"))
						{
							break;
						}
						column.DataType = typeof(string);
						column.TextAlign = TextAlignEnum.CenterCenter;
						return;
					}
					if (c == '日')
					{
						if (!(colName == "日期"))
						{
							break;
						}
						column.DataType = typeof(DateTime);
						column.TextAlign = TextAlignEnum.LeftCenter;
						column.Format = "yyyy-MM-dd";
						return;
					}
					if (c != '项' || !(colName == "项目"))
					{
						break;
					}
				}
				goto IL_0153;
			}
			case 1:
				{
					char c = colName[0];
					if (c != '号' && c != '字')
					{
						break;
					}
					goto IL_0183;
				}
				IL_0153:
				column.DataType = typeof(string);
				column.TextAlign = TextAlignEnum.LeftCenter;
				return;
				IL_0183:
				column.DataType = typeof(string);
				column.TextAlign = TextAlignEnum.CenterCenter;
				return;
			}
		}
		column.DataType = typeof(decimal);
		column.TextAlign = TextAlignEnum.RightCenter;
		column.Format = "#,0.00;-#,0.00;#";
	}

	private void UpdateRowDataBySelectedObject(CollectObjectEnum selectObject, DataRow dataRow, object rowData, int rowIndex)
	{
		switch (selectObject)
		{
		case CollectObjectEnum.Balance:
			Update_Balance();
			break;
		case CollectObjectEnum.Subsidiary:
			Update_Subsidiary();
			break;
		case CollectObjectEnum.Summary:
			Update_Balance();
			break;
		case CollectObjectEnum.None:
			break;
		}
		void Update_Balance()
		{
			C1.Win.C1FlexGrid.Row row2 = _dataGrid.Rows[rowIndex];
			row2.StyleNew.BackColor = DefaultBackgroundColor;
			Account account = null;
			if (rowData is Tuple<Account, AuxiliaryItem>)
			{
				row2.StyleNew.ForeColor = Color.Purple;
				C1.Win.C1FlexGrid.CellStyle cellStyle = _dataGrid.GetCellStyle(rowIndex, 0);
				if (cellStyle == null)
				{
					cellStyle = _dataGrid.Styles.Fixed.Clone();
				}
				cellStyle.ForeColor = Color.Purple;
				_dataGrid.SetCellStyle(rowIndex, 0, cellStyle);
			}
			else if (rowData is Account account2)
			{
				account = account2;
			}
			_dataGrid.SetData(rowIndex, 3, dataRow["科目代码"]);
			CheckEnum checkEnum = CheckEnum.Checked;
			if (account != null && Collector.IsNeedRunEmptyAccountCheckToSetSelectStatus && Collector.Setting.IsCancelEmptyAccountSelectStatus && Collector.IsEmptyAccountWithCache(account))
			{
				checkEnum = CheckEnum.Unchecked;
			}
			_dataGrid.SetCellCheck(rowIndex, 1, checkEnum);
			_dataGrid.Rows[rowIndex].StyleNew.BackColor = ((checkEnum == CheckEnum.Checked) ? DefaultBackgroundColor : HighLightColor);
		}
		void Update_Subsidiary()
		{
			C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[rowIndex];
			row.StyleNew.BackColor = DefaultBackgroundColor;
			_dataGrid.SetCellCheck(rowIndex, 1, CheckEnum.Checked);
		}
	}

	private void UpdateRowDataByMappingData(DataRow dataRow, int gridRowIndex)
	{
		for (int i = 4; i < _tableHeaderColumnsCount; i++)
		{
			TableHeaderCellSetting tableHeaderSetting = GetTableHeaderSetting(_tableHeaderRowsCount - 1, i);
			TableColumnMappingData columnMappingData = tableHeaderSetting.ColumnMappingData;
			if (columnMappingData == null || columnMappingData.DataTableColIndex < 0)
			{
				_dataGrid.SetData(gridRowIndex, i, null);
				continue;
			}
			object obj = dataRow[columnMappingData.DataTableColIndex];
			if (columnMappingData.IsUsePositiveValue && obj != null && obj is decimal num && num < 0m)
			{
				obj = null;
			}
			_dataGrid.SetData(gridRowIndex, i, obj);
		}
	}

	private void MakeLevelDown(int rowIndex)
	{
		CollectObjectEnum collectObject = Collector.CollectObject;
		if (collectObject == CollectObjectEnum.Balance || collectObject == CollectObjectEnum.Summary)
		{
			MakeLevelDown_Impl(rowIndex);
		}
	}

	private void MakeLevelUp(int rowIndex)
	{
		CollectObjectEnum collectObject = Collector.CollectObject;
		if (collectObject == CollectObjectEnum.Balance || collectObject == CollectObjectEnum.Summary)
		{
			MakeLevelUp_Impl(rowIndex);
		}
	}

	private void ShowContextMenu_OnIconMenuClicked(int rowIndex, Point point)
	{
		switch (Collector.CollectObject)
		{
		case CollectObjectEnum.Balance:
			ShowContextMenu_OnIconMenuClicked_Balance(rowIndex, point);
			break;
		case CollectObjectEnum.Summary:
			ShowContextMenu_OnIconMenuClicked_Summary(rowIndex, point);
			break;
		}
	}

	private void ShowContextMenu_OnCellRightClicked(int rowIndex, int colIndex, Point point)
	{
		switch (Collector.CollectObject)
		{
		case CollectObjectEnum.Balance:
			ShowContextMenu_OnCellRightClicked_Balance(rowIndex, colIndex, point);
			break;
		case CollectObjectEnum.Subsidiary:
			ShowContextMenu_OnCellRightClicked_Subsidiary(rowIndex, colIndex, point);
			break;
		case CollectObjectEnum.Summary:
			ShowContextMenu_OnCellRightClicked_Summary(rowIndex, colIndex, point);
			break;
		case CollectObjectEnum.None:
			break;
		}
	}

	private DataGridRowUserData GetRowData(int girdRowIndex)
	{
		return (DataGridRowUserData)_dataGrid.Rows[girdRowIndex].UserData;
	}

	private bool IsAncestor(Account ancestor, Account account)
	{
		foreach (Account ancestor2 in account.Ancestors)
		{
			if (ancestor == ancestor2)
			{
				return true;
			}
		}
		return false;
	}

	private void MakeLevelUp_Impl(int gridTableRowIndex)
	{
		DataGridRowUserData rowData = GetRowData(gridTableRowIndex);
		if (rowData.DataType == DataGridRowDataType.Account)
		{
			Account accountData = rowData.AccountData;
			if (accountData.Parent == null)
			{
				return;
			}
			List<object> list = new List<object>();
			list.Add(accountData.Parent);
			InsertAccountDataAfterRow(accountData.Parent, list, gridTableRowIndex);
			int count = _dataGrid.Rows.Count;
			List<C1.Win.C1FlexGrid.Row> list2 = new List<C1.Win.C1FlexGrid.Row>();
			for (int i = _tableHeaderRowsCount; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[i];
				DataGridRowUserData dataGridRowUserData = (DataGridRowUserData)row.UserData;
				if ((dataGridRowUserData.DataType == DataGridRowDataType.Account && IsAncestor(accountData.Parent, dataGridRowUserData.AccountData)) || (dataGridRowUserData.DataType == DataGridRowDataType.AuxiliaryItem && (dataGridRowUserData.AuxiliaryData.Item1 == accountData.Parent || IsAncestor(accountData.Parent, dataGridRowUserData.AuxiliaryData.Item1))))
				{
					list2.Add(row);
				}
			}
			RemoveDataTableRow(list2);
		}
		else if (rowData.DataType == DataGridRowDataType.AuxiliaryItem)
		{
			Tuple<Account, AuxiliaryItem> auxiliaryData = rowData.AuxiliaryData;
			List<object> list3 = new List<object>();
			list3.Add(auxiliaryData.Item1);
			InsertAccountDataAfterRow(auxiliaryData.Item1, list3, gridTableRowIndex);
			int count2 = _dataGrid.Rows.Count;
			List<C1.Win.C1FlexGrid.Row> list4 = new List<C1.Win.C1FlexGrid.Row>();
			for (int j = _tableHeaderRowsCount; j < count2; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = _dataGrid.Rows[j];
				DataGridRowUserData dataGridRowUserData2 = (DataGridRowUserData)row2.UserData;
				if ((dataGridRowUserData2.DataType == DataGridRowDataType.Account && IsAncestor(auxiliaryData.Item1, dataGridRowUserData2.AccountData)) || (dataGridRowUserData2.DataType == DataGridRowDataType.AuxiliaryItem && (dataGridRowUserData2.AuxiliaryData.Item1 == auxiliaryData.Item1 || IsAncestor(auxiliaryData.Item1, dataGridRowUserData2.AuxiliaryData.Item1))))
				{
					list4.Add(row2);
				}
			}
			RemoveDataTableRow(list4);
		}
		_dataGrid.BeginUpdate();
		try
		{
			AutoAdjustTableHeaderDisplaySetting();
		}
		finally
		{
			_dataGrid.EndUpdate();
		}
	}

	private void MakeLevelDown_Impl(int gridTableRowIndex)
	{
		DataGridRowUserData rowData = GetRowData(gridTableRowIndex);
		if (rowData.DataType != 0)
		{
			return;
		}
		List<object> list = new List<object>();
		GetChildAuxiliaryItem(rowData.AccountData, list);
		GetChildAccount(rowData.AccountData, list);
		if (list.Count != 0)
		{
			C1.Win.C1FlexGrid.CellRange selection = _dataGrid.Selection;
			RemoveDataTableRow(gridTableRowIndex);
			InsertAccountDataAfterRow(rowData.AccountData, list, gridTableRowIndex);
			try
			{
				_dataGrid.Select(selection);
			}
			catch (Exception)
			{
			}
		}
	}

	private void SwitchAuxClass_Impl(int gridTableRowIndex, AuxiliaryClass newAuxClass)
	{
		DataGridRowUserData rowData = GetRowData(gridTableRowIndex);
		if (rowData.DataType != DataGridRowDataType.AuxiliaryItem)
		{
			return;
		}
		int count = _dataGrid.Rows.Count;
		List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
		int num = int.MaxValue;
		for (int i = _tableHeaderRowsCount; i < count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _dataGrid.Rows[i];
			DataGridRowUserData dataGridRowUserData = (DataGridRowUserData)row.UserData;
			if (dataGridRowUserData.DataType == DataGridRowDataType.AuxiliaryItem && dataGridRowUserData.AuxiliaryData.Item1 == rowData.AuxiliaryData.Item1 && dataGridRowUserData.AuxiliaryData.Item2.Class == rowData.AuxiliaryData.Item2.Class)
			{
				num = Math.Min(row.Index, num);
				list.Add(row);
			}
		}
		if (list.Count != 0)
		{
			RemoveDataTableRow(list);
			List<object> list2 = new List<object>();
			GetChildAuxiliaryItem(rowData.AuxiliaryData.Item1, list2, newAuxClass);
			InsertAccountDataAfterRow(rowData.AuxiliaryData.Item1, list2, num);
		}
	}

	private void ExpandAuxiliaryClass_Impl(int gridTableRowIndex, Account account, AuxiliaryClass auxClass)
	{
		List<object> list = new List<object>();
		GetChildAuxiliaryItem(account, list, auxClass);
		if (list.Count != 0)
		{
			RemoveDataTableRow(gridTableRowIndex);
			InsertAccountDataAfterRow(account, list, gridTableRowIndex);
		}
	}

	private void ChangeAccountNameDisplayStyle_Balance()
	{
		TableCollectorBalance tableCollectorBalance = (TableCollectorBalance)Collector;
		tableCollectorBalance.AccNameStyle = ((tableCollectorBalance.AccNameStyle == AccNameStyleEnum.Normal) ? AccNameStyleEnum.SecondFullName : AccNameStyleEnum.Normal);
		List<object> list = new List<object>();
		int count = _dataGrid.Rows.Count;
		for (int i = _tableHeaderRowsCount; i < count; i++)
		{
			list.Add(((DataGridRowUserData)_dataGrid.Rows[i].UserData).RawData);
		}
		ReCollectData(list);
		RefreshTableView();
		UnSelectEmptyAccount();
	}

	private bool IsHasAuxiliaryItem(Account account, DateTime startTime, DateTime endTime)
	{
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_ledger, startTime, endTime);
		return TableCollectorAbstract.GetFirstOrDefaultAuxiliary(_ledger, account, trialBalanceSheetWithCache) != null;
	}

	private bool GetChildAuxiliaryItem(Account parent, List<object> list, AuxiliaryClass auxiliaryClass = null)
	{
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_ledger, selectedStartDate(), selectedEndDate());
		if (auxiliaryClass == null)
		{
			auxiliaryClass = TableCollectorAbstract.GetFirstOrDefaultAuxiliary(_ledger, parent, trialBalanceSheetWithCache);
		}
		if (auxiliaryClass == null)
		{
			return false;
		}
		ClassBalance classBalance = trialBalanceSheetWithCache.End[parent].ClassBalances[auxiliaryClass];
		List<AuxiliaryItem> list2 = (from t in classBalance.ItemBalances
			orderby t.Key.Code
			select t.Key).ToList();
		if (list2.Count == 0)
		{
			return false;
		}
		foreach (AuxiliaryItem item in list2)
		{
			list.Add(Tuple.Create(parent, item));
		}
		return true;
	}

	private bool GetChildAccount(Account parent, List<object> list)
	{
		if (parent.Children.Count == 0)
		{
			return false;
		}
		foreach (Account child in parent.Children)
		{
			list.Add(child);
		}
		return true;
	}

	private void ShowContextMenu_OnIconMenuClicked_Impl(int rowIndex, Point point, bool isShowSwitchName)
	{
		_ctxIconMenu = new C1ContextMenu();
		DataGridRowUserData rowData = GetRowData(rowIndex);
		if (rowData.DataType == DataGridRowDataType.Account)
		{
			Account accountData = rowData.AccountData;
			if (isShowSwitchName)
			{
				C1CommandLink c1CommandLink = new C1CommandLink();
				C1Command c1Command = new C1Command();
				c1Command.Text = "切换科目名称样式";
				c1Command.UserData = new RowData
				{
					RowIndex = rowIndex,
					UserData = accountData
				};
				c1Command.Click += Cmd_ChangeAccountNameStyle;
				c1CommandLink.Command = c1Command;
				_ctxIconMenu.CommandLinks.Add(c1CommandLink);
			}
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = GetTrialBalanceSheetWithCache(_ledger, selectedStartDate(), selectedEndDate()).End[accountData].ClassBalances;
			foreach (AuxiliaryClass key in classBalances.Keys)
			{
				C1Command c1Command2 = new C1Command();
				c1Command2.Text = "展开" + key.Name + "核算";
				c1Command2.UserData = new RowData
				{
					RowIndex = rowIndex,
					UserData = Tuple.Create(accountData, key)
				};
				c1Command2.Click += Cmd_ExpandAuxClassClick;
				_ctxIconMenu.CommandLinks.Add(new C1CommandLink(c1Command2));
			}
		}
		else
		{
			if (rowData.DataType != DataGridRowDataType.AuxiliaryItem)
			{
				return;
			}
			Tuple<Account, AuxiliaryItem> auxiliaryData = rowData.AuxiliaryData;
			if (isShowSwitchName)
			{
				C1CommandLink c1CommandLink2 = new C1CommandLink();
				C1Command c1Command3 = new C1Command();
				c1Command3.Text = "切换科目名称样式";
				c1Command3.UserData = new RowData
				{
					RowIndex = rowIndex,
					UserData = rowData
				};
				c1Command3.Click += Cmd_ChangeAccountNameStyle;
				c1CommandLink2.Command = c1Command3;
				_ctxIconMenu.CommandLinks.Add(c1CommandLink2);
			}
			Dictionary<AuxiliaryClass, ClassBalance> classBalances2 = GetTrialBalanceSheetWithCache(_ledger, selectedStartDate(), selectedEndDate()).End[auxiliaryData.Item1].ClassBalances;
			foreach (AuxiliaryClass key2 in classBalances2.Keys)
			{
				if (key2 != auxiliaryData.Item2.Class)
				{
					C1Command c1Command4 = new C1Command();
					c1Command4.Text = "切换" + key2.Name + "核算";
					c1Command4.UserData = new RowData
					{
						RowIndex = rowIndex,
						UserData = Tuple.Create(auxiliaryData.Item1, key2)
					};
					c1Command4.Click += Cmd_SwitchAuxClassClick;
					_ctxIconMenu.CommandLinks.Add(new C1CommandLink(c1Command4));
				}
			}
		}
		int num;
		int num2;
		if (checkBoxAllAccount.Checked && Collector.CollectObject == CollectObjectEnum.Balance)
		{
			num = 0;
			num2 = _ledger_max_level;
		}
		else
		{
			Account account = selectedAccount();
			int level = account.GetLevel();
			num = level;
			num2 = GetChildAccountMaxLevel(account, level);
		}
		for (int i = num; i < num2; i++)
		{
			C1Command c1Command5 = new C1Command();
			c1Command5.Text = $"全部科目显示至{i + 1}级";
			c1Command5.UserData = new ExpandToLevelData
			{
				ToLevel = i,
				isShowAuxItem = (i == num2 - 1)
			};
			c1Command5.Click += Cmd_ExandToLevelClick;
			_ctxIconMenu.CommandLinks.Add(new C1CommandLink(c1Command5)
			{
				Delimiter = (i == num)
			});
		}
		_ctxIconMenu.ShowContextMenu(_dataGrid, point);
	}

	private void ShowContextMenu_OnIconMenuClicked_Balance(int rowIndex, Point point)
	{
		ShowContextMenu_OnIconMenuClicked_Impl(rowIndex, point, isShowSwitchName: true);
	}

	private void ShowContextMenu_OnIconMenuClicked_Summary(int rowIndex, Point point)
	{
		ShowContextMenu_OnIconMenuClicked_Impl(rowIndex, point, isShowSwitchName: false);
	}

	private void ShowContextMenu_OnCellRightClicked_Balance(int rowIndex, int colIndex, Point point)
	{
		_ctxCellRightClick.ShowContextMenu(_dataGrid, point);
	}

	private void ShowContextMenu_OnCellRightClicked_Subsidiary(int rowIndex, int colIndex, Point point)
	{
		_ctxCellRightClick.ShowContextMenu(_dataGrid, point);
	}

	private void ShowContextMenu_OnCellRightClicked_Summary(int rowIndex, int colIndex, Point point)
	{
		_ctxCellRightClick.ShowContextMenu(_dataGrid, point);
	}

	private int GetChildAccountMaxLevel(Account parent, int parentLevel)
	{
		int num = parentLevel + 1;
		if (parent.Children.Count == 0)
		{
			return num;
		}
		int num2 = num;
		foreach (Account child in parent.Children)
		{
			num2 = Math.Max(GetChildAccountMaxLevel(child, num), num2);
		}
		return num2;
	}

	private void UpdateLedgerMaxLevel()
	{
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_ledger, selectedStartDate(), selectedEndDate());
		List<Account> list = trialBalanceSheetWithCache.End.Keys.ToList();
		int num = 0;
		foreach (Account item in list)
		{
			if (item.Children.Count != 0)
			{
				num = Math.Max(GetChildAccountMaxLevel(item, 1), num);
			}
		}
		_ledger_max_level = num;
	}

	private void Cmd_AccountLevelUpClick(object sender, ClickEventArgs e)
	{
		C1Command c1Command = (C1Command)sender;
		RowData rowData = (RowData)c1Command.UserData;
		MakeLevelUp_Impl(rowData.RowIndex);
	}

	private void Cmd_AccountLevelDownClick(object sender, ClickEventArgs e)
	{
		C1Command c1Command = (C1Command)sender;
		RowData rowData = (RowData)c1Command.UserData;
		MakeLevelDown_Impl(rowData.RowIndex);
	}

	private void Cmd_ExpandAuxClassClick(object sender, ClickEventArgs e)
	{
		C1Command c1Command = (C1Command)sender;
		RowData rowData = (RowData)c1Command.UserData;
		Tuple<Account, AuxiliaryClass> tuple = (Tuple<Account, AuxiliaryClass>)rowData.UserData;
		ExpandAuxiliaryClass_Impl(rowData.RowIndex, tuple.Item1, tuple.Item2);
	}

	private void Cmd_SwitchAuxClassClick(object sender, ClickEventArgs e)
	{
		C1Command c1Command = (C1Command)sender;
		RowData rowData = (RowData)c1Command.UserData;
		Tuple<Account, AuxiliaryClass> tuple = (Tuple<Account, AuxiliaryClass>)rowData.UserData;
		SwitchAuxClass_Impl(rowData.RowIndex, tuple.Item2);
	}

	private void Cmd_ChangeAccountNameStyle(object sender, ClickEventArgs e)
	{
		if (Collector.CollectObject == CollectObjectEnum.Balance)
		{
			ChangeAccountNameDisplayStyle_Balance();
		}
	}

	private void Cmd_ExandToLevelClick(object sender, ClickEventArgs e)
	{
		C1Command c1Command = (C1Command)sender;
		ExpandToLevelData expandToLevelData = (ExpandToLevelData)c1Command.UserData;
		SubAccountFilterMode subAccountFilterMode;
		if (expandToLevelData.isShowAuxItem)
		{
			subAccountFilterMode = SubAccountFilterMode.AllChildAndAuxiliaryItem;
		}
		else
		{
			Collector.Setting.CollectMaxLevel = expandToLevelData.ToLevel;
			subAccountFilterMode = SubAccountFilterMode.AllAboveLevelChild;
		}
		ReCollectData(null, subAccountFilterMode);
		RefreshTableView();
		UnSelectEmptyAccount();
	}

	private void Cmd_CancelSort_Click(object sender, ClickEventArgs e)
	{
		_dataGrid.Sort(SortFlags.Ascending, 0);
		_dataGrid.Sort(SortFlags.None, 0);
	}

	private void Cmd_SortAsc_Click(object sender, ClickEventArgs e)
	{
		_dataGrid.Sort(SortFlags.Ascending, _dataGrid.Col);
	}

	private void Cmd_SortDesc_Click(object sender, ClickEventArgs e)
	{
		_dataGrid.Sort(SortFlags.Descending, _dataGrid.Col);
	}

	protected TrialBalanceSheet GetTrialBalanceSheetWithCache(Ledger ledger, DateTime start, DateTime end)
	{
		Tuple<DateTime, DateTime, TrialBalanceSheet> tuple = _trialBalanceSheetCache.Find((Tuple<DateTime, DateTime, TrialBalanceSheet> tp) => tp.Item1.Equals(start) && tp.Item2.Equals(end));
		if (tuple != null)
		{
			return tuple.Item3;
		}
		TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(start, end);
		_trialBalanceSheetCache.Add(Tuple.Create(start, end, trialBalanceSheet));
		return trialBalanceSheet;
	}

	private void AttachEvent()
	{
		if (!_attachEvent)
		{
			spbAnalysis.ItemClick += SpbAnalysis_ItemClick;
			comboCollectObject.SelectedIndexChanged += Combox_OnCollectObjectChanged;
			comboAccountTree.SelectNodeChanged += Combox_OnAccountTreeChanged;
			comboAuxiliaryTree.SelectNodeChanged += Combox_OnAuxiliaryTree_SelectNodeChanged;
			comboStartMonth.TextChanged += Combox_OnStartMonthChanged;
			comboEndMonth.TextChanged += Combox_OnEndMonthChanged;
			checkBoxAllAccount.CheckedChanged += CheckBox_AllAccountCheckStatusChanged;
			checkBoxOnlyMyMark.CheckedChanged += CheckBox_OnlyMyMarkCheckStatusChanged;
			checkBoxNotFilterAccount.CheckedChanged += CheckBox_NotFilterAccountCheckStatusChanged;
			_attachEvent = true;
		}
	}

	private void DettachEvent()
	{
		if (_attachEvent)
		{
			spbAnalysis.ItemClick -= SpbAnalysis_ItemClick;
			comboCollectObject.SelectedIndexChanged -= Combox_OnCollectObjectChanged;
			comboAccountTree.SelectNodeChanged -= Combox_OnAccountTreeChanged;
			comboAuxiliaryTree.SelectNodeChanged -= Combox_OnAuxiliaryTree_SelectNodeChanged;
			comboStartMonth.TextChanged -= Combox_OnStartMonthChanged;
			comboEndMonth.TextChanged -= Combox_OnEndMonthChanged;
			checkBoxAllAccount.CheckedChanged -= CheckBox_AllAccountCheckStatusChanged;
			checkBoxOnlyMyMark.CheckedChanged -= CheckBox_OnlyMyMarkCheckStatusChanged;
			checkBoxNotFilterAccount.CheckedChanged -= CheckBox_NotFilterAccountCheckStatusChanged;
			_attachEvent = false;
		}
	}

	private void AllChildren(TreeNodeCollection Nodes, List<System.Windows.Forms.TreeNode> list)
	{
		foreach (System.Windows.Forms.TreeNode Node in Nodes)
		{
			list.Add(Node);
			AllChildren(Node.Nodes, list);
		}
	}

	private void SetVisibleAuxiliary(bool visble)
	{
		lblAuxiliary.Visible = visble;
		comboAuxiliaryTree.Visible = visble;
	}

	private TableHeaderCellSetting GetTableHeaderSetting(int rowIndex, int colIndex)
	{
		int num = rowIndex * _tableHeaderColumnsCount + colIndex;
		if (num < 0 || num >= _tableHeaderSettingList.Count)
		{
			return null;
		}
		return _tableHeaderSettingList[num];
	}

	private int GetColumnMinWidth(int colIndex)
	{
		if (colIndex < 0 || colIndex >= _columnsMinWidthList.Count)
		{
			return -1;
		}
		return _columnsMinWidthList[colIndex];
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Controls.frmTableCollect2));
		this.c1ContextMenu1 = new C1.Win.C1Command.C1ContextMenu();
		this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlBottomButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnIntelligenceFill = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.pnlDockingTab = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.DockingTab = new C1.Win.C1Command.C1DockingTab();
		this.TabPageBalance = new C1.Win.C1Command.C1DockingTabPage();
		this.ctnBlance = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlBalanceConditions = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.comboAuxiliaryTree = new Auditai.UI.Controls.ComboTree();
		this.lblAuxiliary = new C1.Win.C1Input.C1Label();
		this.checkBoxNotFilterAccount = new C1.Win.C1Input.C1CheckBox();
		this.checkBoxOnlyMyMark = new C1.Win.C1Input.C1CheckBox();
		this.checkBoxAllAccount = new C1.Win.C1Input.C1CheckBox();
		this.comboAccountTree = new Auditai.UI.Controls.ComboTree();
		this.lblCollectObject = new C1.Win.C1Input.C1Label();
		this.c1Label2 = new C1.Win.C1Input.C1Label();
		this.comboEndMonth = new C1.Win.C1Input.C1ComboBox();
		this.lblKjqj = new C1.Win.C1Input.C1Label();
		this.comboStartMonth = new C1.Win.C1Input.C1ComboBox();
		this.comboCollectObject = new C1.Win.C1Input.C1ComboBox();
		this.lblStartMonth = new C1.Win.C1Input.C1Label();
		this.lblEndMonth = new C1.Win.C1Input.C1Label();
		this.spbAnalysis = new Auditai.UI.Controls.C1SplitButtonEx();
		this.dropDownItem1 = new C1.Win.C1Input.DropDownItem();
		this.dropDownItem2 = new C1.Win.C1Input.DropDownItem();
		this.dropDownItem3 = new C1.Win.C1Input.DropDownItem();
		this.pnlTitlePanel = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._gridTitle = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.panelDataTable = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlBottomButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnIntelligenceFill).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		this.pnlDockingTab.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.DockingTab).BeginInit();
		this.DockingTab.SuspendLayout();
		this.TabPageBalance.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ctnBlance).BeginInit();
		this.ctnBlance.SuspendLayout();
		this.pnlBalanceConditions.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.comboAuxiliaryTree).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblAuxiliary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkBoxNotFilterAccount).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkBoxOnlyMyMark).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.checkBoxAllAccount).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboAccountTree).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectObject).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboEndMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblKjqj).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboStartMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboCollectObject).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblStartMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblEndMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.spbAnalysis).BeginInit();
		this.pnlTitlePanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._gridTitle).BeginInit();
		base.SuspendLayout();
		this.c1ContextMenu1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[1] { this.c1CommandLink1 });
		this.c1ContextMenu1.Name = "c1ContextMenu1";
		this.c1ContextMenu1.ShortcutText = "";
		this.c1CommandLink1.Text = "新命令";
		this.c1CommandHolder1.Commands.Add(this.c1ContextMenu1);
		this.c1CommandHolder1.Owner = this;
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(207, 221, 238);
		this.ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(240, 245, 250);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.FixedLineColor = System.Drawing.Color.FromArgb(145, 166, 194);
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(30, 57, 91);
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlBottomButtons);
		this.ctnAll.Panels.Add(this.pnlDockingTab);
		this.ctnAll.Size = new System.Drawing.Size(1140, 734);
		this.ctnAll.SplitterColor = System.Drawing.Color.FromArgb(145, 166, 194);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 5;
		this.ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlBottomButtons.Controls.Add(this.btnIntelligenceFill);
		this.pnlBottomButtons.Controls.Add(this.btnCancel);
		this.pnlBottomButtons.Controls.Add(this.btnConfirm);
		this.pnlBottomButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlBottomButtons.Height = 60;
		this.pnlBottomButtons.KeepRelativeSize = false;
		this.pnlBottomButtons.Location = new System.Drawing.Point(0, 674);
		this.pnlBottomButtons.Name = "pnlBottomButtons";
		this.pnlBottomButtons.Resizable = false;
		this.pnlBottomButtons.Size = new System.Drawing.Size(1140, 60);
		this.pnlBottomButtons.SizeRatio = 7.255;
		this.pnlBottomButtons.TabIndex = 1;
		this.btnIntelligenceFill.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.btnIntelligenceFill.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnIntelligenceFill.Location = new System.Drawing.Point(740, 17);
		this.btnIntelligenceFill.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnIntelligenceFill.Name = "btnIntelligenceFill";
		this.btnIntelligenceFill.Size = new System.Drawing.Size(70, 26);
		this.btnIntelligenceFill.TabIndex = 1;
		this.btnIntelligenceFill.Text = "智能设置";
		this.btnIntelligenceFill.UseVisualStyleBackColor = true;
		this.btnIntelligenceFill.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.btnIntelligenceFill.Click += new System.EventHandler(btnIntelligenceFill_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(980, 17);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancle_Click);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnConfirm.Location = new System.Drawing.Point(860, 17);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 2;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.pnlDockingTab.Controls.Add(this.DockingTab);
		this.pnlDockingTab.Height = 673;
		this.pnlDockingTab.Location = new System.Drawing.Point(0, 0);
		this.pnlDockingTab.Name = "pnlDockingTab";
		this.pnlDockingTab.Size = new System.Drawing.Size(1140, 673);
		this.pnlDockingTab.TabIndex = 0;
		this.DockingTab.Controls.Add(this.TabPageBalance);
		this.DockingTab.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DockingTab.Location = new System.Drawing.Point(0, 0);
		this.DockingTab.Name = "DockingTab";
		this.DockingTab.ShowTabs = false;
		this.DockingTab.Size = new System.Drawing.Size(1140, 673);
		this.DockingTab.TabIndex = 3;
		this.DockingTab.TabsSpacing = 5;
		this.DockingTab.TabStyle = C1.Win.C1Command.TabStyleEnum.Office2007;
		this.DockingTab.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.DockingTab.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2007Blue;
		this.TabPageBalance.CaptionText = "采账设置";
		this.TabPageBalance.Controls.Add(this.ctnBlance);
		this.TabPageBalance.Location = new System.Drawing.Point(1, 1);
		this.TabPageBalance.Name = "TabPageBalance";
		this.TabPageBalance.Size = new System.Drawing.Size(1138, 671);
		this.TabPageBalance.TabIndex = 0;
		this.TabPageBalance.Text = "采账设置";
		this.ctnBlance.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnBlance.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnBlance.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnBlance.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnBlance.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnBlance.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnBlance.Location = new System.Drawing.Point(0, 0);
		this.ctnBlance.Name = "ctnBlance";
		this.ctnBlance.Panels.Add(this.pnlBalanceConditions);
		this.ctnBlance.Panels.Add(this.pnlTitlePanel);
		this.ctnBlance.Panels.Add(this.panelDataTable);
		this.ctnBlance.Size = new System.Drawing.Size(1138, 671);
		this.ctnBlance.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnBlance.SplitterWidth = 0;
		this.ctnBlance.TabIndex = 0;
		this.ctnBlance.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlBalanceConditions.Controls.Add(this.comboAuxiliaryTree);
		this.pnlBalanceConditions.Controls.Add(this.lblAuxiliary);
		this.pnlBalanceConditions.Controls.Add(this.checkBoxNotFilterAccount);
		this.pnlBalanceConditions.Controls.Add(this.checkBoxOnlyMyMark);
		this.pnlBalanceConditions.Controls.Add(this.checkBoxAllAccount);
		this.pnlBalanceConditions.Controls.Add(this.comboAccountTree);
		this.pnlBalanceConditions.Controls.Add(this.lblCollectObject);
		this.pnlBalanceConditions.Controls.Add(this.c1Label2);
		this.pnlBalanceConditions.Controls.Add(this.comboEndMonth);
		this.pnlBalanceConditions.Controls.Add(this.lblKjqj);
		this.pnlBalanceConditions.Controls.Add(this.comboStartMonth);
		this.pnlBalanceConditions.Controls.Add(this.comboCollectObject);
		this.pnlBalanceConditions.Controls.Add(this.lblStartMonth);
		this.pnlBalanceConditions.Controls.Add(this.lblEndMonth);
		this.pnlBalanceConditions.Controls.Add(this.spbAnalysis);
		this.pnlBalanceConditions.Height = 80;
		this.pnlBalanceConditions.KeepRelativeSize = false;
		this.pnlBalanceConditions.Location = new System.Drawing.Point(0, 0);
		this.pnlBalanceConditions.Name = "pnlBalanceConditions";
		this.pnlBalanceConditions.Resizable = false;
		this.pnlBalanceConditions.Size = new System.Drawing.Size(1138, 80);
		this.pnlBalanceConditions.SizeRatio = 19.608;
		this.pnlBalanceConditions.TabIndex = 0;
		this.comboAuxiliaryTree.AllowSpinLoop = false;
		this.comboAuxiliaryTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboAuxiliaryTree.DropHeight = -1;
		this.comboAuxiliaryTree.DropWidth = -1;
		this.comboAuxiliaryTree.Font = new System.Drawing.Font("微软雅黑", 9f);
		this.comboAuxiliaryTree.GapHeight = 0;
		this.comboAuxiliaryTree.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboAuxiliaryTree.ItemsDisplayMember = "";
		this.comboAuxiliaryTree.ItemsValueMember = "";
		this.comboAuxiliaryTree.Location = new System.Drawing.Point(433, 46);
		this.comboAuxiliaryTree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboAuxiliaryTree.Name = "comboAuxiliaryTree";
		this.comboAuxiliaryTree.SelectedNode = null;
		this.comboAuxiliaryTree.Size = new System.Drawing.Size(203, 21);
		this.comboAuxiliaryTree.TabIndex = 29;
		this.comboAuxiliaryTree.Tag = null;
		this.comboAuxiliaryTree.TextDetached = true;
		this.lblAuxiliary.BackColor = System.Drawing.Color.Transparent;
		this.lblAuxiliary.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblAuxiliary.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblAuxiliary.ForeColor = System.Drawing.Color.Black;
		this.lblAuxiliary.Location = new System.Drawing.Point(361, 48);
		this.lblAuxiliary.Name = "lblAuxiliary";
		this.lblAuxiliary.Size = new System.Drawing.Size(68, 17);
		this.lblAuxiliary.TabIndex = 22;
		this.lblAuxiliary.Tag = null;
		this.lblAuxiliary.Text = "辅助核算：";
		this.lblAuxiliary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.lblAuxiliary.TextDetached = true;
		this.lblAuxiliary.Value = "辅助核算：";
		this.checkBoxNotFilterAccount.BackColor = System.Drawing.Color.Transparent;
		this.checkBoxNotFilterAccount.BorderColor = System.Drawing.Color.Transparent;
		this.checkBoxNotFilterAccount.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.checkBoxNotFilterAccount.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkBoxNotFilterAccount.Location = new System.Drawing.Point(669, 46);
		this.checkBoxNotFilterAccount.Name = "checkBoxNotFilterAccount";
		this.checkBoxNotFilterAccount.Padding = new System.Windows.Forms.Padding(1);
		this.checkBoxNotFilterAccount.Size = new System.Drawing.Size(96, 21);
		this.checkBoxNotFilterAccount.TabIndex = 33;
		this.checkBoxNotFilterAccount.Text = "全部科目";
		this.checkBoxNotFilterAccount.UseVisualStyleBackColor = false;
		this.checkBoxNotFilterAccount.Value = null;
		this.checkBoxNotFilterAccount.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.checkBoxOnlyMyMark.BackColor = System.Drawing.Color.Transparent;
		this.checkBoxOnlyMyMark.BorderColor = System.Drawing.Color.Transparent;
		this.checkBoxOnlyMyMark.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.checkBoxOnlyMyMark.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkBoxOnlyMyMark.Location = new System.Drawing.Point(773, 46);
		this.checkBoxOnlyMyMark.Name = "checkBoxOnlyMyRemark";
		this.checkBoxOnlyMyMark.Padding = new System.Windows.Forms.Padding(1);
		this.checkBoxOnlyMyMark.Size = new System.Drawing.Size(96, 21);
		this.checkBoxOnlyMyMark.TabIndex = 32;
		this.checkBoxOnlyMyMark.Text = "仅我的关注";
		this.checkBoxOnlyMyMark.UseVisualStyleBackColor = false;
		this.checkBoxOnlyMyMark.Value = null;
		this.checkBoxOnlyMyMark.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.checkBoxAllAccount.BackColor = System.Drawing.Color.Transparent;
		this.checkBoxAllAccount.BorderColor = System.Drawing.Color.Transparent;
		this.checkBoxAllAccount.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.checkBoxAllAccount.ForeColor = System.Drawing.SystemColors.ControlText;
		this.checkBoxAllAccount.Location = new System.Drawing.Point(360, 46);
		this.checkBoxAllAccount.Name = "checkBoxAllAccount";
		this.checkBoxAllAccount.Padding = new System.Windows.Forms.Padding(1);
		this.checkBoxAllAccount.Size = new System.Drawing.Size(96, 21);
		this.checkBoxAllAccount.TabIndex = 30;
		this.checkBoxAllAccount.Text = "全部科目";
		this.checkBoxAllAccount.UseVisualStyleBackColor = false;
		this.checkBoxAllAccount.Value = null;
		this.checkBoxAllAccount.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.comboAccountTree.AllowSpinLoop = false;
		this.comboAccountTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboAccountTree.DropHeight = -1;
		this.comboAccountTree.DropWidth = -1;
		this.comboAccountTree.Font = new System.Drawing.Font("微软雅黑", 9f);
		this.comboAccountTree.GapHeight = 0;
		this.comboAccountTree.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboAccountTree.ItemsDisplayMember = "";
		this.comboAccountTree.ItemsValueMember = "";
		this.comboAccountTree.Location = new System.Drawing.Point(96, 46);
		this.comboAccountTree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboAccountTree.Name = "comboAccountTree";
		this.comboAccountTree.SelectedNode = null;
		this.comboAccountTree.Size = new System.Drawing.Size(233, 21);
		this.comboAccountTree.TabIndex = 28;
		this.comboAccountTree.Tag = null;
		this.comboAccountTree.TextDetached = true;
		this.lblCollectObject.AutoSize = true;
		this.lblCollectObject.BackColor = System.Drawing.Color.Transparent;
		this.lblCollectObject.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCollectObject.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblCollectObject.ForeColor = System.Drawing.Color.Black;
		this.lblCollectObject.Location = new System.Drawing.Point(22, 17);
		this.lblCollectObject.Name = "lblCollectObject";
		this.lblCollectObject.Size = new System.Drawing.Size(68, 17);
		this.lblCollectObject.TabIndex = 19;
		this.lblCollectObject.Tag = null;
		this.lblCollectObject.Text = "采集对象：";
		this.lblCollectObject.TextDetached = true;
		this.lblCollectObject.Value = "采集对象：";
		this.c1Label2.BackColor = System.Drawing.Color.Transparent;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label2.ForeColor = System.Drawing.Color.Black;
		this.c1Label2.Location = new System.Drawing.Point(22, 48);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(68, 17);
		this.c1Label2.TabIndex = 20;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "科目名称：";
		this.c1Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.c1Label2.TextDetached = true;
		this.c1Label2.Value = "科目名称：";
		this.comboEndMonth.AllowSpinLoop = false;
		this.comboEndMonth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboEndMonth.GapHeight = 0;
		this.comboEndMonth.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboEndMonth.Items.Add("1");
		this.comboEndMonth.Items.Add("2");
		this.comboEndMonth.Items.Add("3");
		this.comboEndMonth.Items.Add("4");
		this.comboEndMonth.Items.Add("5");
		this.comboEndMonth.Items.Add("6");
		this.comboEndMonth.Items.Add("7");
		this.comboEndMonth.Items.Add("8");
		this.comboEndMonth.Items.Add("9");
		this.comboEndMonth.Items.Add("10");
		this.comboEndMonth.Items.Add("11");
		this.comboEndMonth.Items.Add("12");
		this.comboEndMonth.ItemsDisplayMember = "";
		this.comboEndMonth.ItemsValueMember = "";
		this.comboEndMonth.Location = new System.Drawing.Point(544, 15);
		this.comboEndMonth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboEndMonth.Name = "comboEndMonth";
		this.comboEndMonth.Size = new System.Drawing.Size(59, 21);
		this.comboEndMonth.TabIndex = 27;
		this.comboEndMonth.Tag = null;
		this.comboEndMonth.TextDetached = true;
		this.lblKjqj.AutoSize = true;
		this.lblKjqj.BackColor = System.Drawing.Color.Transparent;
		this.lblKjqj.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblKjqj.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblKjqj.ForeColor = System.Drawing.Color.Black;
		this.lblKjqj.Location = new System.Drawing.Point(361, 17);
		this.lblKjqj.Name = "lblKjqj";
		this.lblKjqj.Size = new System.Drawing.Size(68, 17);
		this.lblKjqj.TabIndex = 21;
		this.lblKjqj.Tag = null;
		this.lblKjqj.Text = "会计期间：";
		this.lblKjqj.TextDetached = true;
		this.lblKjqj.Value = "会计期间：";
		this.comboStartMonth.AllowSpinLoop = false;
		this.comboStartMonth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboStartMonth.GapHeight = 0;
		this.comboStartMonth.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboStartMonth.Items.Add("1");
		this.comboStartMonth.Items.Add("2");
		this.comboStartMonth.Items.Add("3");
		this.comboStartMonth.Items.Add("4");
		this.comboStartMonth.Items.Add("5");
		this.comboStartMonth.Items.Add("6");
		this.comboStartMonth.Items.Add("7");
		this.comboStartMonth.Items.Add("8");
		this.comboStartMonth.Items.Add("9");
		this.comboStartMonth.Items.Add("10");
		this.comboStartMonth.Items.Add("11");
		this.comboStartMonth.Items.Add("12");
		this.comboStartMonth.ItemsDisplayMember = "";
		this.comboStartMonth.ItemsValueMember = "";
		this.comboStartMonth.Location = new System.Drawing.Point(435, 15);
		this.comboStartMonth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboStartMonth.Name = "comboStartMonth";
		this.comboStartMonth.Size = new System.Drawing.Size(72, 21);
		this.comboStartMonth.TabIndex = 26;
		this.comboStartMonth.Tag = null;
		this.comboStartMonth.TextDetached = true;
		this.comboCollectObject.AllowSpinLoop = false;
		this.comboCollectObject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboCollectObject.GapHeight = 0;
		this.comboCollectObject.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboCollectObject.Items.Add("科目余额表");
		this.comboCollectObject.Items.Add("记账凭证表");
		this.comboCollectObject.Items.Add("月度汇总表");
		this.comboCollectObject.ItemsDisplayMember = "";
		this.comboCollectObject.ItemsValueMember = "";
		this.comboCollectObject.Location = new System.Drawing.Point(96, 15);
		this.comboCollectObject.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboCollectObject.Name = "comboCollectObject";
		this.comboCollectObject.Size = new System.Drawing.Size(233, 21);
		this.comboCollectObject.TabIndex = 25;
		this.comboCollectObject.Tag = null;
		this.lblStartMonth.AutoSize = true;
		this.lblStartMonth.BackColor = System.Drawing.Color.Transparent;
		this.lblStartMonth.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblStartMonth.ForeColor = System.Drawing.Color.Black;
		this.lblStartMonth.Location = new System.Drawing.Point(513, 17);
		this.lblStartMonth.Name = "lblStartMonth";
		this.lblStartMonth.Size = new System.Drawing.Size(25, 17);
		this.lblStartMonth.TabIndex = 23;
		this.lblStartMonth.Tag = null;
		this.lblStartMonth.Text = "月-";
		this.lblStartMonth.TextDetached = true;
		this.lblStartMonth.Value = "月-";
		this.lblEndMonth.AutoSize = true;
		this.lblEndMonth.BackColor = System.Drawing.Color.Transparent;
		this.lblEndMonth.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEndMonth.ForeColor = System.Drawing.Color.Black;
		this.lblEndMonth.Location = new System.Drawing.Point(615, 17);
		this.lblEndMonth.Name = "lblEndMonth";
		this.lblEndMonth.Size = new System.Drawing.Size(20, 17);
		this.lblEndMonth.TabIndex = 24;
		this.lblEndMonth.Tag = null;
		this.lblEndMonth.Text = "月";
		this.lblEndMonth.TextDetached = true;
		this.lblEndMonth.Value = "月";
		this.spbAnalysis.Items.Add(this.dropDownItem1);
		this.spbAnalysis.Items.Add(this.dropDownItem2);
		this.spbAnalysis.Items.Add(this.dropDownItem3);
		this.spbAnalysis.Location = new System.Drawing.Point(363, 43);
		this.spbAnalysis.Name = "spbAnalysis";
		this.spbAnalysis.Size = new System.Drawing.Size(105, 26);
		this.spbAnalysis.TabIndex = 31;
		this.spbAnalysis.Text = "借方发生额";
		this.spbAnalysis.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.spbAnalysis.UseVisualStyleBackColor = true;
		this.dropDownItem1.Text = "科目余额";
		this.dropDownItem2.Text = "借方发生额";
		this.dropDownItem3.Text = "贷方发生额";
		this.pnlTitlePanel.Controls.Add(this._gridTitle);
		this.pnlTitlePanel.Height = 40;
		this.pnlTitlePanel.Location = new System.Drawing.Point(0, 81);
		this.pnlTitlePanel.MinHeight = 0;
		this.pnlTitlePanel.Name = "pnlTitlePanel";
		this.pnlTitlePanel.Resizable = false;
		this.pnlTitlePanel.Size = new System.Drawing.Size(1138, 40);
		this.pnlTitlePanel.SizeRatio = 6.791;
		this.pnlTitlePanel.TabIndex = 1;
		this._gridTitle.AllowEditing = false;
		this._gridTitle.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this._gridTitle.AutoResize = true;
		this._gridTitle.BackColor = System.Drawing.Color.Gainsboro;
		this._gridTitle.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle;
		this._gridTitle.ColumnInfo = "0,0,0,0,0,100,Columns:";
		this._gridTitle.FocusRect = C1.Win.C1FlexGrid.FocusRectEnum.None;
		this._gridTitle.HighLight = C1.Win.C1FlexGrid.HighLightEnum.Never;
		this._gridTitle.Location = new System.Drawing.Point(0, 0);
		this._gridTitle.Margin = new System.Windows.Forms.Padding(0);
		this._gridTitle.Name = "_gridTitle";
		this._gridTitle.Rows.Count = 0;
		this._gridTitle.Rows.DefaultSize = 20;
		this._gridTitle.Rows.Fixed = 0;
		this._gridTitle.Size = new System.Drawing.Size(1138, 40);
		this._gridTitle.StyleInfo = resources.GetString("_gridTitle.StyleInfo");
		this._gridTitle.TabIndex = 1;
		this.panelDataTable.Height = 549;
		this.panelDataTable.Location = new System.Drawing.Point(0, 122);
		this.panelDataTable.Name = "panelDataTable";
		this.panelDataTable.Size = new System.Drawing.Size(1138, 549);
		this.panelDataTable.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1140, 734);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.MinimumSize = new System.Drawing.Size(650, 300);
		base.Name = "frmTableCollect2";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "列对应采账设置";
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Office2010Blue;
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlBottomButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnIntelligenceFill).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		this.pnlDockingTab.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.DockingTab).EndInit();
		this.DockingTab.ResumeLayout(false);
		this.TabPageBalance.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ctnBlance).EndInit();
		this.ctnBlance.ResumeLayout(false);
		this.pnlBalanceConditions.ResumeLayout(false);
		this.pnlBalanceConditions.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.comboAuxiliaryTree).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblAuxiliary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkBoxNotFilterAccount).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkBoxOnlyMyMark).EndInit();
		((System.ComponentModel.ISupportInitialize)this.checkBoxAllAccount).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboAccountTree).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectObject).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboEndMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblKjqj).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboStartMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboCollectObject).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblStartMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblEndMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.spbAnalysis).EndInit();
		this.pnlTitlePanel.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this._gridTitle).EndInit();
		base.ResumeLayout(false);
	}
}
