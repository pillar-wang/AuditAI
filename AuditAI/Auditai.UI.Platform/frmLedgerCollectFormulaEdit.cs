﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class frmLedgerCollectFormulaEdit : Form
{
	protected class VirtaulTableColumnData
	{
		public int GridColumnIndex;

		public string ColumnName;

		public Type ColumnDataType;

		public int VirtualTableColumnIndex;
	}

	protected class BalanceVirtualTableFilterContext : FilterContext
	{
		protected frmLedgerCollectFormulaEdit _owner;

		protected LedgerVirtualTable _virtualTable;

		public BalanceVirtualTableFilterContext (frmLedgerCollectFormulaEdit owner, LedgerVirtualTable virtualTable)
		{
			_owner = owner;
			_virtualTable = virtualTable;
		}

		public override string GetColumnCaption (string columnId)
		{
			if (!int.TryParse (columnId, out var id)) {
				return string.Empty;
			}
			VirtaulTableColumnData virtaulTableColumnData = _owner._balanceGridColumnDataList.FirstOrDefault ((VirtaulTableColumnData virtaulTableColumnData2) => virtaulTableColumnData2.GridColumnIndex == id);
			if (virtaulTableColumnData != null) {
				return virtaulTableColumnData.ColumnName;
			}
			return string.Empty;
		}

		private int ConvertGridColumnIndexToVirtualTableColumnIndex (int gridColumnIndex)
		{
			return _owner._balanceGridColumnDataList [gridColumnIndex].VirtualTableColumnIndex;
		}

		public override List<FilterValue> GetColumnData (string columnId)
		{
			int columnIndex = GetColumnIndex (columnId);
			if (columnIndex < 0) {
				return new List<FilterValue> (0);
			}
			if (_virtualTable.Rows.Count == 0) {
				return new List<FilterValue> { GetData (1, columnIndex) };
			}
			int count = _virtualTable.Rows.Count;
			List<FilterValue> list = new List<FilterValue> (count);
			for (int num = 0; num < count; num++) {
				object value = _virtualTable [num, ConvertGridColumnIndexToVirtualTableColumnIndex (columnIndex)].Value;
				FilterValue filterValue = null;
				if (!(value is string text)) {
					if (value is double num2) {
						filterValue = ((columnIndex != _owner._balanceLevelColumnIndex) ? FilterValue.FromObject (num2, num2.ToString ("#,0.00;-#,0.00;#")) : FilterValue.FromObject (num2, num2.ToString ()));
					} else if (!(value is bool flag)) {
						if (value is DateYearMonth dateYearMonth) {
							filterValue = FilterValue.FromObject (dateYearMonth, dateYearMonth.Date.ToString ("yyyy-MM"));
						}
					} else {
						filterValue = FilterValue.FromObject (flag, flag ? "是" : "否");
					}
				} else {
					filterValue = FilterValue.FromObject (text, text);
				}
				if (filterValue != null) {
					list.Add (filterValue);
				}
			}
			return list;
		}

		public override Type GetColumnDataType (string columnId)
		{
			int columnIndex = GetColumnIndex (columnId);
			if (columnIndex < 0) {
				return typeof(string);
			}
			return _owner._balanceGridColumnDataList [columnIndex].ColumnDataType;
		}

		public override string GetColumnDataTypeFormatString (string columnId)
		{
			int columnIndex = GetColumnIndex (columnId);
			if (columnIndex < 0) {
				return string.Empty;
			}
			if (columnIndex == _owner._balanceLevelColumnIndex) {
				return string.Empty;
			}
			if (_owner._balanceGridColumnDataList [columnIndex].ColumnDataType == typeof(double)) {
				return "#,0.00;-#,0.00;#";
			}
			return string.Empty;
		}

		public override string GetColumnId (C1.Win.C1FlexGrid.Column col)
		{
			return (col.Index - _owner._balanceGrid.Cols.Fixed).ToString ();
		}

		public override int GetColumnIndex (string columnId)
		{
			if (!int.TryParse (columnId, out var result)) {
				return -1;
			}
			return result;
		}

		public override FilterValue GetData (int row, int col)
		{
			if (col < 0 || col >= _virtualTable.Columns.Count) {
				return FilterValue.FromObject (string.Empty, string.Empty);
			}
			if (row >= _virtualTable.Rows.Count) {
				VirtaulTableColumnData virtaulTableColumnData = _owner._balanceGridColumnDataList [col];
				if (virtaulTableColumnData.ColumnDataType == typeof(string)) {
					return FilterValue.FromObject (string.Empty, string.Empty);
				}
				if (virtaulTableColumnData.ColumnDataType == typeof(double)) {
					return FilterValue.FromObject (0.0, "0");
				}
				if (virtaulTableColumnData.ColumnDataType == typeof(bool)) {
					return FilterValue.FromObject (false, "否");
				}
				if (virtaulTableColumnData.ColumnDataType == typeof(DateYearMonth)) {
					return FilterValue.FromObject (new DateYearMonth (), string.Empty);
				}
				return FilterValue.FromObject (string.Empty, string.Empty);
			}
			object value = _virtualTable [row, ConvertGridColumnIndexToVirtualTableColumnIndex (col)].Value;
			if (!(value is string text)) {
				if (!(value is double num)) {
					if (!(value is bool flag)) {
						if (value is DateYearMonth dateYearMonth) {
							return FilterValue.FromObject (dateYearMonth, dateYearMonth.Date.ToString ("yyyy-MM"));
						}
						return FilterValue.FromObject (string.Empty, string.Empty);
					}
					return FilterValue.FromObject (flag, flag ? "是" : "否");
				}
				if (col == _owner._balanceLevelColumnIndex) {
					return FilterValue.FromObject (num, num.ToString ());
				}
				return FilterValue.FromObject (num, num.ToString ("#,0.00;-#,0.00;#"));
			}
			return FilterValue.FromObject (text, text);
		}

		public override Tuple<bool, string, string> IsCheckBox (int row, int col)
		{
			bool flag = false;
			if (_owner._balanceGridColumnDataList [col].ColumnDataType == typeof(bool)) {
				flag = true;
			}
			if (!flag) {
				return Tuple.Create (item1: false, "", "");
			}
			return Tuple.Create (item1: true, "是", "否");
		}
	}

	protected class VoucherVirtualTableFilterContext : FilterContext
	{
		protected frmLedgerCollectFormulaEdit _owner;

		public VoucherVirtualTableFilterContext (frmLedgerCollectFormulaEdit owner)
		{
			_owner = owner;
		}

		public override string GetColumnCaption (string columnId)
		{
			if (!long.TryParse (columnId, out var result)) {
				return string.Empty;
			}
			if (VoucherVirtualTableBuilder.TryGetColumnName (result, out var columnName)) {
				return columnName;
			}
			return string.Empty;
		}

		public override List<FilterValue> GetColumnData (string columnId)
		{
			int columnIndex = GetColumnIndex (columnId);
			if (columnIndex < 0) {
				return new List<FilterValue> (0);
			}
			if (_owner._voucherVirtualTable.Rows.Count == 0) {
				return new List<FilterValue> { GetData (1, columnIndex) };
			}
			int count = _owner._voucherVirtualTable.Rows.Count;
			List<FilterValue> list = new List<FilterValue> (count);
			for (int num = 0; num < count; num++) {
				object value = _owner._voucherVirtualTable [num, columnIndex].Value;
				FilterValue filterValue = null;
				if (!(value is string text)) {
					if (!(value is double num2)) {
						if (!(value is DateYearMonth dateYearMonth)) {
							if (!(value is DateTime dateTime)) {
								if (value is bool flag) {
									filterValue = FilterValue.FromObject (flag, flag ? "是" : "否");
								}
							} else {
								filterValue = FilterValue.FromObject (dateTime, dateTime.ToString ("yyyy-MM-dd"));
							}
						} else {
							filterValue = FilterValue.FromObject (dateYearMonth, dateYearMonth.Date.ToString ("yyyy-MM"));
						}
					} else {
						filterValue = FilterValue.FromObject (num2, num2.ToString ("#,0.00;-#,0.00;#"));
					}
				} else {
					filterValue = FilterValue.FromObject (text, text);
				}
				if (filterValue != null) {
					list.Add (filterValue);
				}
			}
			return list;
		}

		public override Type GetColumnDataType (string columnId)
		{
			int columnIndex = GetColumnIndex (columnId);
			if (columnIndex < 0) {
				return typeof(string);
			}
			return _owner._voucherVirtualTableColumnDataList [columnIndex].ColumnDataType;
		}

		public override string GetColumnDataTypeFormatString (string columnId)
		{
			int columnIndex = GetColumnIndex (columnId);
			if (columnIndex < 0) {
				return string.Empty;
			}
			if (_owner._voucherVirtualTableColumnDataList [columnIndex].ColumnDataType == typeof(double)) {
				return "#,0.00;-#,0.00;#";
			}
			return string.Empty;
		}

		public override string GetColumnId (C1.Win.C1FlexGrid.Column col)
		{
			if (VoucherVirtualTableBuilder.TryGetColumnIdByColumnIndex (col.Index - _owner._voucherGrid.Cols.Fixed, out var columnId)) {
				return columnId.ToString ();
			}
			return "-1";
		}

		public override int GetColumnIndex (string columnId)
		{
			if (!long.TryParse (columnId, out var result)) {
				return -1;
			}
			return VoucherVirtualTableBuilder.GetColumnIndex (result);
		}

		public override FilterValue GetData (int row, int col)
		{
			if (col < 0 || col >= _owner._voucherVirtualTable.Columns.Count) {
				return FilterValue.FromObject (string.Empty, string.Empty);
			}
			if (row >= _owner._voucherVirtualTable.Rows.Count) {
				VirtaulTableColumnData virtaulTableColumnData = _owner._voucherVirtualTableColumnDataList [col];
				if (virtaulTableColumnData.ColumnDataType == typeof(string)) {
					return FilterValue.FromObject (string.Empty, string.Empty);
				}
				if (virtaulTableColumnData.ColumnDataType == typeof(double)) {
					return FilterValue.FromObject (0.0, "0");
				}
				if (virtaulTableColumnData.ColumnDataType == typeof(DateYearMonth)) {
					return FilterValue.FromObject (new DateYearMonth (), string.Empty);
				}
				if (virtaulTableColumnData.ColumnDataType == typeof(DateTime)) {
					return FilterValue.FromObject (DateTime.MinValue, string.Empty);
				}
				if (virtaulTableColumnData.ColumnDataType == typeof(bool)) {
					return FilterValue.FromObject (false, "否");
				}
				return FilterValue.FromObject (string.Empty, string.Empty);
			}
			object value = _owner._voucherVirtualTable [row, col].Value;
			if (!(value is string text)) {
				if (!(value is double num)) {
					if (!(value is bool flag)) {
						if (!(value is DateYearMonth dateYearMonth)) {
							if (value is DateTime dateTime) {
								return FilterValue.FromObject (dateTime, dateTime.ToString ("yyyy-MM-dd"));
							}
							return FilterValue.FromObject (string.Empty, string.Empty);
						}
						return FilterValue.FromObject (dateYearMonth, dateYearMonth.Date.ToString ("yyyy-MM"));
					}
					return FilterValue.FromObject (flag, flag ? "是" : "否");
				}
				return FilterValue.FromObject (num, num.ToString ("#,0.00;-#,0.00;#"));
			}
			return FilterValue.FromObject (text, text);
		}

		public override Tuple<bool, string, string> IsCheckBox (int row, int col)
		{
			return Tuple.Create (item1: false, "", "");
		}
	}

	private string _dropValue;

	private string _inputFormula = string.Empty;

	private C1ContextMenu ctx1 = new C1ContextMenu ();

	private C1Command cmdCopy1 = new C1Command ();

	private C1CommandLink lnkCopy1 = new C1CommandLink ();

	private C1Command cmdCut1 = new C1Command ();

	private C1CommandLink lnkCut1 = new C1CommandLink ();

	private C1Command cmdPaste1 = new C1Command ();

	private C1CommandLink lnkPaste1 = new C1CommandLink ();

	private C1Command cmdInsertVariable1 = new C1Command ();

	private C1CommandLink lnkInsertVariable1 = new C1CommandLink ();

	private C1Command cmdInsertColumn1 = new C1Command ();

	private C1CommandLink lnkInsertColumn1 = new C1CommandLink ();

	private C1ContextMenu ctx2 = new C1ContextMenu ();

	private C1Command cmdCopy2 = new C1Command ();

	private C1CommandLink lnkCopy2 = new C1CommandLink ();

	private C1Command cmdCut2 = new C1Command ();

	private C1CommandLink lnkCut2 = new C1CommandLink ();

	private C1Command cmdPaste2 = new C1Command ();

	private C1CommandLink lnkPaste2 = new C1CommandLink ();

	private C1Command cmdInsertVariable2 = new C1Command ();

	private C1CommandLink lnkInsertVariable2 = new C1CommandLink ();

	private C1ContextMenu ctx3 = new C1ContextMenu ();

	private C1Command cmdCopy3 = new C1Command ();

	private C1CommandLink lnkCopy3 = new C1CommandLink ();

	private C1Command cmdCut3 = new C1Command ();

	private C1CommandLink lnkCut3 = new C1CommandLink ();

	private C1Command cmdPaste3 = new C1Command ();

	private C1CommandLink lnkPaste3 = new C1CommandLink ();

	private C1ContextMenu _ctxCellBalaceGrid = new C1ContextMenu ();

	private C1ContextMenu _ctxCellVoucherGrid = new C1ContextMenu ();

	private C1Command cmdCollectF = new C1Command {
		Text = "CollectF函数"
	};

	private C1CommandLink lnkCollectF;

	private C1Command cmdDistinctF = new C1Command {
		Text = "DistinctF函数"
	};

	private C1CommandLink lnkDistinctF;

	private C1Command cmdSumIf = new C1Command {
		Text = "SumIf函数"
	};

	private C1CommandLink lnkSumIf;

	private C1Command cmdVLookUp = new C1Command {
		Text = "VLookUp函数"
	};

	private C1CommandLink lnkVLookUp;

	private C1Command cmdOtherFunc = new C1Command {
		Text = "其他函数"
	};

	private C1CommandLink lnkOtherFunc;

	private C1ContextMenu ctxOtherFunc = new C1ContextMenu ();

	private C1Command cmdInsertVariable3 = new C1Command ();

	private C1CommandLink lnkInsertVariable3 = new C1CommandLink ();

	internal static readonly Pen _penFormulaRefRect = new Pen (Color.Red, 1f) {
		Alignment = PenAlignment.Center
	};

	internal static readonly Pen _penAnimateDash = new Pen (Color.Red, 1f) {
		DashStyle = DashStyle.Dash,
		Alignment = PenAlignment.Center
	};

	internal static readonly SolidBrush _brushFormulaRefRect = new SolidBrush (Color.Red);

	private readonly Timer _timerFormulaHighlight = new Timer {
		Interval = 100
	};

	private Color _gridHilightForeColor = Color.Black;

	private Color _gridHilightBackColor = Color.Transparent;

	private Color _gridHightBorderColor = Color.Black;

	private Color _gridNormalBorderColor = Color.Black;

	private Dictionary<Auditai.Model.Column, string> _changedFormula;

	private LedgerCollectFormulaEditor _owner;

	private Auditai.Model.Table _inEditingTable;

	private Auditai.Model.Column _inEditingColumn;

	private C1FlexGridEx _navGrid;

	private Dictionary<Auditai.Model.Column, string> _rawFormulaDic = new Dictionary<Auditai.Model.Column, string> ();

	private Dictionary<Auditai.Model.Column, string> _afterModifyFormulaTextDic = new Dictionary<Auditai.Model.Column, string> ();

	private bool _isSuspendSelectionChangeEvent;

	private int _balanceLevelColumnIndex = 4;

	private int _balanceAccountCodeColumnIndex = 1;

	private int _balanceAccountFullNameColumnIndex = 2;

	private int _balanceAccountCurrentNameColumnIndex = 3;

	private bool _balanceVirtualTableEmpty;

	private LedgerVirtualTable _balanceVirtualTable;

	private C1FlexGridEx _balanceGrid;

	private List<VirtaulTableColumnData> _balanceGridColumnDataList;

	private int[] _balanceVirtualTableColumnIndexToGridIndex;

	private int _voucherAccountCodeColumnIndex = 4;

	private int _voucherAccountFullNameColumnIndex = 5;

	private bool _voucherVirtualTableEmpty;

	private LedgerVirtualTable _voucherVirtualTable;

	private C1FlexGridEx _voucherGrid;

	private List<VirtaulTableColumnData> _voucherVirtualTableColumnDataList;

	private bool _isVoucherGridPopuldate;

	private bool _isTextChanging;

	private bool _attachEvent;

	private IContainer components;

	private C1SplitContainer ctnAll;

	private C1SplitContainer ctnDock;

	private C1SplitterPanel pnlLeftPart;

	private C1SplitterPanel pnlRightPart;

	private C1SplitterPanel navGridPanel;

	private C1SplitterPanel pnlInput;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancle;

	private C1Button btnConfirm;

	private C1SplitContainer ctnInput;

	private C1ContextMenu txbFunctionListContextMenu;

	private C1CommandLink c1CommandLink1;

	private C1CommandHolder c1CommandHolder1;

	private C1ContextMenu c1ContextMenu1;

	private C1SplitterPanel pnlFunctions;

	private C1ToolBar tbrFunctions;

	private C1SplitterPanel pnlFunctionHint;

	private C1Label lblFunctionHint;

	private C1SplitterPanel pnlFormulaInput;

	public RichTextBoxEx rtbFormulaInput;

	private C1SplitterPanel pnlGrid;

	private C1ContextMenu txbDropInputContextMenu;

	private C1DockingTab tabDockGrid;

	private C1DockingTabPage balanceGridPage;

	private C1DockingTabPage voucherGridPage;

	public string Value {
		get {
			return _dropValue;
		}
		set {
			_dropValue = value;
			rtbFormulaInput.Text = Value;
		}
	}

	public Dictionary<Auditai.Model.Column, string> ChangedFormula => _changedFormula;

	public Action<string> TryRunFormulaHandle { get; set; }

	public bool QueryValueChanged { get; private set; }

	public frmLedgerCollectFormulaEdit (LedgerCollectFormulaEditor owner)
	{
		DoubleBuffered = true;
		_owner = owner;
		InitializeComponent ();
		Initialize ();
	}

	public void ShowFunctionHint (string formulaText, int pos)
	{
		try {
			FormulaDisplay formulaDisplay = new FormulaDisplay (formulaText);
			Tuple<string, int> tup = formulaDisplay.GetFuncNameAtPos (pos);
			if (tup.Item1 == null) {
				lblFunctionHint.Value = "";
				return;
			}
			FunctionInfo functionInfo = FunctionInfo.AllFunctionInfos.FirstOrDefault ((FunctionInfo functionInfo2) => functionInfo2.Name.Equals (tup.Item1, StringComparison.InvariantCultureIgnoreCase));
			if (functionInfo == null || !Program.MainForm.IsAllowShowFunctionInfoAtCurrentView (functionInfo, isControlFormulaForm: false, isCollectFormulaForm: true)) {
				lblFunctionHint.Value = "";
				return;
			}
			string text = "函数语法：" + functionInfo.Name + "(" + string.Join (", ", functionInfo.Parameters.Select ((ParameterInfo parameterInfo) => parameterInfo.Name)) + ")\n";
			text = text + "函数功能：" + functionInfo.Description + "\n";
			text = text + "参数说明：" + string.Join ("；", functionInfo.Parameters.Select ((ParameterInfo parameterInfo) => parameterInfo.Name + "：" + parameterInfo.Description));
			lblFunctionHint.Value = text;
		} catch (FormulaException) {
			lblFunctionHint.Value = "";
		}
	}

	internal bool UseWildcard ()
	{
		return UseWildcardImpl (rtbFormulaInput.Text, rtbFormulaInput.SelectionStart);
	}

	private void btnConfirm_Click (object sender, EventArgs e)
	{
		if (_inEditingColumn != null) {
			_afterModifyFormulaTextDic [_inEditingColumn] = rtbFormulaInput.Text;
		}
		if (ValidteAllColumnFormula (out var changedFormulaDic)) {
			base.DialogResult = DialogResult.OK;
			_changedFormula = changedFormulaDic;
			Close ();
		} else {
			rtbFormulaInput.Focus ();
		}
	}

	private void btnCancle_Click (object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close ();
	}

	private void ResetQueryStatus ()
	{
		QueryValueChanged = false;
	}

	private void Initialize ()
	{
		rtbFormulaInput.SelectionChanged += TxbDropInput_SelectionChanged;
		cmdCopy1.Text = "复制";
		cmdCopy1.Click += CmdCopy_Click;
		lnkCopy1.Command = cmdCopy1;
		cmdCut1.Text = "剪切";
		cmdCut1.Click += CmdCut_Click;
		lnkCut1.Command = cmdCut1;
		cmdPaste1.Text = "粘贴";
		cmdPaste1.Click += CmdPaste_Click;
		lnkPaste1.Command = cmdPaste1;
		cmdInsertVariable1.Text = "插入变量";
		cmdInsertVariable1.Click += CmdInsertVariable_Click;
		lnkInsertVariable1.Command = cmdInsertVariable1;
		cmdInsertColumn1.Text = "插入列来源";
		cmdInsertColumn1.Click += CmdInsertColumn1_Click;
		lnkInsertColumn1.Command = cmdInsertColumn1;
		ctx1.CommandLinks.Add (lnkCopy1);
		ctx1.CommandLinks.Add (lnkCut1);
		ctx1.CommandLinks.Add (lnkPaste1);
		ctx1.CommandLinks.Add (lnkInsertVariable1);
		ctx1.CommandLinks.Add (lnkInsertColumn1);
		cmdCopy2.Text = "复制";
		cmdCopy2.Click += CmdCopy_Click;
		lnkCopy2.Command = cmdCopy2;
		cmdCut2.Text = "剪切";
		cmdCut2.Click += CmdCut_Click;
		lnkCut2.Command = cmdCut2;
		cmdPaste2.Text = "粘贴";
		cmdPaste2.Click += CmdPaste_Click;
		lnkPaste2.Command = cmdPaste2;
		cmdInsertVariable2.Text = "插入变量";
		cmdInsertVariable2.Click += CmdInsertVariable_Click;
		lnkInsertVariable2.Command = cmdInsertVariable2;
		ctx2.CommandLinks.Add (lnkCopy2);
		ctx2.CommandLinks.Add (lnkCut2);
		ctx2.CommandLinks.Add (lnkPaste2);
		ctx2.CommandLinks.Add (lnkInsertVariable2);
		cmdCopy3.Text = "复制";
		cmdCopy3.Click += CmdCopy_Click;
		lnkCopy3.Command = cmdCopy3;
		cmdCut3.Text = "剪切";
		cmdCut3.Click += CmdCut_Click;
		lnkCut3.Command = cmdCut3;
		cmdPaste3.Text = "粘贴";
		cmdPaste3.Click += CmdPaste_Click;
		lnkPaste3.Command = cmdPaste3;
		cmdInsertVariable3.Text = "插入变量";
		cmdInsertVariable3.Click += CmdInsertVariable_Click;
		lnkInsertVariable3.Command = cmdInsertVariable3;
		ctx3.CommandLinks.Add (lnkCopy3);
		ctx3.CommandLinks.Add (lnkCut3);
		ctx3.CommandLinks.Add (lnkPaste3);
		ctx3.CommandLinks.Add (lnkInsertVariable3);
		cmdCollectF.Click += CmdCollectF_Click;
		lnkCollectF = new C1CommandLink (cmdCollectF);
		lnkCollectF.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add (lnkCollectF);
		cmdDistinctF.Click += CmdDistinctF_Click;
		lnkDistinctF = new C1CommandLink (cmdDistinctF);
		lnkDistinctF.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add (lnkDistinctF);
		cmdSumIf.Click += CmdSumIf_Click;
		lnkSumIf = new C1CommandLink (cmdSumIf);
		lnkSumIf.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add (lnkSumIf);
		cmdVLookUp.Click += CmdVLookUp_Click;
		lnkVLookUp = new C1CommandLink (cmdVLookUp);
		lnkVLookUp.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add (lnkVLookUp);
		cmdOtherFunc.Click += CmdOtherFunc_Click;
		lnkOtherFunc = new C1CommandLink (cmdOtherFunc);
		lnkOtherFunc.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add (lnkOtherFunc);
		tabDockGrid.SelectedTabChanged += TabDockGrid_SelectedTabChanged;
		c1CommandHolder1.SetC1ContextMenu (rtbFormulaInput, ctx1);
		AttachEvent ();
		base.Shown += Form_Shown;
		base.FormClosing += Form_FormClosing;
		PopulateFunctions ();
		rtbFormulaInput.KeyDown += TxbDropInput_KeyDown;
		Auditai.UI.Controls.Theme.SetCurrentTree (this);
		_timerFormulaHighlight.Tick += _timerFormulaHighlight_Tick;
		base.FormClosed += FrmLedgerCollectFormulaEdit_FormClosed;
	}

	private void _navGrid_BodySelectionChanged (object sender, EventArgs e)
	{
		if (_isSuspendSelectionChangeEvent) {
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _navGrid.Selection;
		int topRow = selection.TopRow;
		int leftCol = selection.LeftCol;
		if (topRow >= _navGrid.Rows.Fixed && topRow < _navGrid.Rows.Count && leftCol >= _navGrid.Cols.Fixed && leftCol < _navGrid.Cols.Count && _navGrid.GetUserData (topRow, leftCol) is Auditai.Model.Column column && column != _inEditingColumn) {
			if (_inEditingColumn != null) {
				_afterModifyFormulaTextDic [_inEditingColumn] = rtbFormulaInput.Text;
			}
			string value = null;
			if (!_afterModifyFormulaTextDic.TryGetValue (column, out value) && _rawFormulaDic.TryGetValue (column, out var value2)) {
				value = GetLedgerFormulaDisplayText (value2);
				_afterModifyFormulaTextDic [column] = value;
			}
			if (value == null) {
				value = string.Empty;
				_afterModifyFormulaTextDic [column] = value;
			}
			Value = value;
			_inEditingColumn = column;
		}
	}

	private string GetLedgerFormulaDisplayText (string formula)
	{
		try {
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator (formula);
			FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver (_inEditingTable.Project);
			return formulaEvaluator.GetDisplayStringLedgerCollectFormula (resolver, _inEditingTable, Program.MainForm.FormulaEditor.Context.LegderVirtualTableSetting);
		} catch (FormulaSyntaxException) {
			return string.Empty;
		} catch (Exception exception) {
			exception.Log ("显示账套采集公式时发生了未预期的异常");
			return string.Empty;
		}
	}

	public void PopulateNavGrid (Auditai.Model.Table table, Auditai.Model.Column column)
	{
		_inEditingTable = table;
		_inEditingColumn = column;
		List<Tuple<string, Auditai.Model.Column>> list = new List<Tuple<string, Auditai.Model.Column>> ();
		foreach (Auditai.Model.Column column2 in table.Columns) {
			string value = string.Empty;
			if (column2.HasLedgerCollectFormula) {
				value = column2.LedgerCollectFormula;
			}
			_rawFormulaDic [column2] = value;
			list.Add (Tuple.Create (column2.GetUniqueFormulaName (), column2));
		}
		_navGrid = new C1FlexGridEx {
			Dock = DockStyle.Fill,
			ExtendLastCol = true,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowEditing = false,
			SelectionMode = SelectionModeEnum.Row
		};
		pnlLeftPart.Controls.Add (_navGrid);
		Auditai.UI.Controls.Theme.SetCurrentTree (_navGrid);
		_navGrid.AllowAddNew = false;
		_navGrid.AllowSorting = AllowSortingEnum.None;
		_navGrid.AllowFreezing = AllowFreezingEnum.None;
		int num = 1;
		_navGrid.Cols.Count = 1;
		_navGrid.Cols.Fixed = 0;
		_navGrid.Rows.Count = list.Count + num;
		_navGrid.Rows.Fixed = num;
		_navGrid.Rows.DefaultSize = 30;
		_navGrid.Cols [0].DataType = typeof(string);
		_navGrid.Cols [0].TextAlign = TextAlignEnum.LeftCenter;
		_navGrid.Styles.Alternate.Clear ();
		_navGrid.Styles.Normal.TextAlign = TextAlignEnum.LeftCenter;
		C1.Win.C1FlexGrid.CellStyle styleNew = _navGrid.GetCellRange (0, 0).StyleNew;
		styleNew.DataType = typeof(string);
		styleNew.TextAlign = TextAlignEnum.CenterCenter;
		styleNew.ImageAlign = ImageAlignEnum.LeftCenter;
		_navGrid.SetData (0, 0, "表格列");
		int num2 = -1;
		_navGrid.BeginUpdate ();
		for (int num3 = 0; num3 < list.Count; num3++) {
			int num4 = num3 + _navGrid.Rows.Fixed;
			Tuple<string, Auditai.Model.Column> tuple = list [num3];
			_navGrid.SetData (num4, 0, tuple.Item1);
			_navGrid.SetUserData (num4, 0, tuple.Item2);
			C1.Win.C1FlexGrid.CellStyle styleNew2 = _navGrid.Rows [num4].StyleNew;
			styleNew2.Border.Color = Color.Transparent;
			styleNew2.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			if (tuple.Item2 == column) {
				num2 = num4;
			}
		}
		_navGrid.EndUpdate ();
		_isSuspendSelectionChangeEvent = true;
		try {
			if (num2 != -1) {
				_navGrid.Select (num2, 0);
			}
		} finally {
			_isSuspendSelectionChangeEvent = false;
		}
		_navGrid.BodySelectionChanged += _navGrid_BodySelectionChanged;
		_navGrid.BodyOwnerDrawCell += _navGrid_BodyOwnerDrawCell;
	}

	private void TryToSelectNavGridRow (Auditai.Model.Column tableColum, out string tableColumnName)
	{
		tableColumnName = string.Empty;
		int num = -1;
		for (int num2 = 0; num2 < _navGrid.Rows.Count; num2++) {
			Auditai.Model.Column column = _navGrid.GetUserData (num2, 0) as Auditai.Model.Column;
			if (column == tableColum) {
				num = num2;
				tableColumnName = _navGrid.GetDataDisplay (num2, 0);
				break;
			}
		}
		if (num >= 0) {
			_navGrid.Select (num, 0);
		}
	}

	private void _navGrid_BodyOwnerDrawCell (object sender, OwnerDrawCellEventArgs e)
	{
		string text = e.Text;
		if (!string.IsNullOrWhiteSpace (text)) {
			text = "[" + text + "]";
		}
		e.Text = text;
		e.Image = Resources.ConfrimationCol;
	}

	public void PrepareToShow ()
	{
		tabDockGrid.SelectedIndex = 0;
		if (!SoftwareLicenseManager.IsLedgerModuleEnable ()) {
			pnlGrid.Visible = false;
		}
	}

	private void TabDockGrid_SelectedTabChanged (object sender, EventArgs e)
	{
		if (tabDockGrid.SelectedTab == voucherGridPage) {
			PopulateVoucherGrid ();
			if (IsGridEntireColumnSelected (_voucherGrid) && IsGridSelectColumnInFormula (_voucherGrid)) {
				ChangeGridHighlightColor (_voucherGrid, isBackTransparent: true);
			} else {
				ChangeGridHighlightColor (_voucherGrid, isBackTransparent: false);
			}
		} else if (tabDockGrid.SelectedTab == balanceGridPage) {
			if (IsGridEntireColumnSelected (_balanceGrid) && IsGridSelectColumnInFormula (_balanceGrid)) {
				ChangeGridHighlightColor (_balanceGrid, isBackTransparent: true);
			} else {
				ChangeGridHighlightColor (_balanceGrid, isBackTransparent: false);
			}
		}
	}

	private void FrmLedgerCollectFormulaEdit_FormClosed (object sender, FormClosedEventArgs e)
	{
		_timerFormulaHighlight.Stop ();
	}

	private bool IsGridEntireColumnSelected (C1FlexGridEx grid)
	{
		C1.Win.C1FlexGrid.CellRange selection = grid.Selection;
		if (!selection.IsValid) {
			return false;
		}
		if (selection.TopRow == grid.Rows.Fixed && selection.BottomRow == grid.Rows.Count - 1) {
			return true;
		}
		return false;
	}

	private void _timerFormulaHighlight_Tick (object sender, EventArgs e)
	{
		if (tabDockGrid.SelectedTab == balanceGridPage) {
			if (IsGridEntireColumnSelected (_balanceGrid)) {
				_penAnimateDash.DashOffset--;
				_balanceGrid.Invalidate ();
				return;
			}
		} else if (tabDockGrid.SelectedTab == voucherGridPage && IsGridEntireColumnSelected (_voucherGrid)) {
			_penAnimateDash.DashOffset--;
			_voucherGrid.Invalidate ();
			return;
		}
		_penAnimateDash.DashOffset = 0f;
	}

	private void CmdOtherFunc_Click (object sender, ClickEventArgs e)
	{
		ctxOtherFunc.ShowContextMenu (tbrFunctions, tbrFunctions.PointToClient (Cursor.Position));
	}

	private void CmdVLookUp_Click (object sender, ClickEventArgs e)
	{
		rtbFormulaInput.SelectedText = "VLookUp()";
		rtbFormulaInput.SelectionStart = rtbFormulaInput.SelectionStart + rtbFormulaInput.SelectionLength - 1;
	}

	private void CmdSumIf_Click (object sender, ClickEventArgs e)
	{
		rtbFormulaInput.SelectedText = "SumIf()";
		rtbFormulaInput.SelectionStart = rtbFormulaInput.SelectionStart + rtbFormulaInput.SelectionLength - 1;
	}

	private void CmdDistinctF_Click (object sender, ClickEventArgs e)
	{
		rtbFormulaInput.SelectedText = "DistinctF()";
		rtbFormulaInput.SelectionStart = rtbFormulaInput.SelectionStart + rtbFormulaInput.SelectionLength - 1;
	}

	private void CmdCollectF_Click (object sender, ClickEventArgs e)
	{
		rtbFormulaInput.SelectedText = "CollectF()";
		rtbFormulaInput.SelectionStart = rtbFormulaInput.SelectionStart + rtbFormulaInput.SelectionLength - 1;
	}

	private void Form_Shown (object sender, EventArgs e)
	{
		Auditai.UI.Controls.Theme.SetCurrentTree (this);
		ctnDock.SplitterWidth = 2;
		ctnInput.SplitterWidth = 2;
		ResetQueryStatus ();
		_owner.IsEditing = true;
		_gridHilightForeColor = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetC1Theme ().GetColor ("C1FlexGrid\\Styles\\Highlight\\ForeColor");
		_gridHilightBackColor = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetBackgroundSolidColor ("C1FlexGrid\\Styles\\Highlight\\Background");
		_gridHightBorderColor = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetC1Theme ().GetColor ("C1FlexGrid\\Styles\\Highlight\\Border\\Color");
		_gridNormalBorderColor = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetC1Theme ().GetColor ("C1FlexGrid\\Styles\\Normal\\Border\\Color");
		_timerFormulaHighlight.Start ();
	}

	public void FocusAtFormulaInputBox ()
	{
		rtbFormulaInput.Focus ();
	}

	public void PopulateBalanceGrid ()
	{
		try {
			if (!Program.MainForm.MultiLedgerViewer.IsLedgerEmpty ()) {
				_balanceVirtualTable = LedgerVirtualTableUtils.GetBalanceVirtualTable (Program.MainForm.MultiLedgerViewer.CurrentLedgerViewer);
			}
			if (_balanceVirtualTable == null) {
				_balanceVirtualTable = BalanceVirtualTableBuilder.GetEmtpyTable ();
			}
			_balanceGridColumnDataList = new List<VirtaulTableColumnData> ();
			List<string> allColumnNames = BalanceVirtualTableBuilder.GetAllColumnNames ();
			allColumnNames.Remove ("是否挂接辅助核算");
			int num = allColumnNames.IndexOf ("是否末级");
			allColumnNames.Insert (num + 1, "是否挂接辅助核算");
			allColumnNames.Remove ("一级科目名称");
			num = allColumnNames.IndexOf ("科目名称");
			allColumnNames.Insert (num, "一级科目名称");
			allColumnNames.Remove ("期初借方净额");
			num = allColumnNames.IndexOf ("本期借方发生额");
			allColumnNames.Insert (num, "期初借方净额");
			allColumnNames.Remove ("期初贷方净额");
			num = allColumnNames.IndexOf ("本期借方发生额");
			allColumnNames.Insert (num, "期初贷方净额");
			_balanceVirtualTableColumnIndexToGridIndex = new int[allColumnNames.Count];
			for (int num2 = 0; num2 < allColumnNames.Count; num2++) {
				VirtaulTableColumnData virtaulTableColumnData = new VirtaulTableColumnData ();
				virtaulTableColumnData.GridColumnIndex = num2;
				virtaulTableColumnData.ColumnName = allColumnNames [num2];
				virtaulTableColumnData.ColumnDataType = BalanceVirtualTableBuilder.GetColumnDataType (allColumnNames [num2]);
				virtaulTableColumnData.VirtualTableColumnIndex = BalanceVirtualTableBuilder.GetColumnIndex (allColumnNames [num2]);
				_balanceGridColumnDataList.Add (virtaulTableColumnData);
				_balanceVirtualTableColumnIndexToGridIndex [virtaulTableColumnData.VirtualTableColumnIndex] = num2;
				switch ((BalanceVirtualTableBuilder.BalanceVirtualTableColumnIndex)virtaulTableColumnData.VirtualTableColumnIndex) {
				case BalanceVirtualTableBuilder.BalanceVirtualTableColumnIndex.科目级次:
					_balanceLevelColumnIndex = num2;
					break;
				case BalanceVirtualTableBuilder.BalanceVirtualTableColumnIndex.科目代码:
					_balanceAccountCodeColumnIndex = num2;
					break;
				case BalanceVirtualTableBuilder.BalanceVirtualTableColumnIndex.科目名称:
					_balanceAccountFullNameColumnIndex = num2;
					break;
				case BalanceVirtualTableBuilder.BalanceVirtualTableColumnIndex.本级科目名称:
					_balanceAccountCurrentNameColumnIndex = num2;
					break;
				}
			}
			_balanceVirtualTableEmpty = _balanceVirtualTable.Rows.Count == 0;
			int num3 = (_balanceVirtualTableEmpty ? 10 : _balanceVirtualTable.Rows.Count);
			_balanceGrid = new C1FlexGridEx ();
			balanceGridPage.Controls.Add (_balanceGrid);
			_balanceGrid.Dock = DockStyle.Fill;
			_balanceGrid.AllowEditing = false;
			_balanceGrid.AllowDragging = AllowDraggingEnum.None;
			_balanceGrid.AllowSorting = AllowSortingEnum.None;
			_balanceGrid.OwnerDrawCell += _balanceGrid_OwnerDrawCell;
			_balanceGrid.BodyOwnerDrawCell += _balanceGrid_BodyOwnerDrawCell;
			_balanceGrid.MouseMove += _balanceGrid_MouseMove;
			_balanceGrid.MouseUp += _balanceGrid_MouseUp;
			_balanceGrid.MouseClick += _balanceGrid_MouseClick;
			_balanceGrid.Paint += _balanceGrid_Paint;
			_balanceGrid.BodySelectionChanged += _balanceGrid_BodySelectionChanged;
			_balanceGrid.FilterManager.Context = new BalanceVirtualTableFilterContext (this, _balanceVirtualTable);
			_balanceGrid.Rows.DefaultSize = 30;
			_balanceGrid.ExtendLastCol = false;
			_balanceGrid.FocusRect = FocusRectEnum.None;
			C1CommandLink value = _balanceGrid.FilterManager.GenLnkFilter ();
			_ctxCellBalaceGrid.CommandLinks.Add (value);
			_ctxCellBalaceGrid.CommandLinks.Add (_balanceGrid.FilterManager.GenLnkSelect ());
			_ctxCellBalaceGrid.CommandLinks.Add (_balanceGrid.FilterManager.GenLnkCancelCurrentColumn ());
			_balanceGrid.BeginUpdate ();
			int num4 = 1;
			int num5 = 1;
			_balanceGrid.Rows.Count = num3 + num4;
			_balanceGrid.Rows.Fixed = num4;
			_balanceGrid.Cols.Count = _balanceGridColumnDataList.Count + num5;
			_balanceGrid.Cols.Fixed = num5;
			_balanceGrid.Cols [0].Width = 45;
			_balanceGrid.Cols [0].StyleNew.TextAlign = TextAlignEnum.CenterCenter;
			_balanceGrid.Rows [0].Height = 40;
			float num6 = 3f;
			int[] array = new int[12] {
				30, 40, 40, 60, 40, 30, 25, 40, 40, 40,
				60, 45
			};
			TextAlignEnum[] array2 = new TextAlignEnum[12] {
				TextAlignEnum.LeftCenter,
				TextAlignEnum.LeftCenter,
				TextAlignEnum.LeftCenter,
				TextAlignEnum.LeftCenter,
				TextAlignEnum.LeftCenter,
				TextAlignEnum.CenterCenter,
				TextAlignEnum.CenterCenter,
				TextAlignEnum.CenterCenter,
				TextAlignEnum.LeftCenter,
				TextAlignEnum.LeftCenter,
				TextAlignEnum.LeftCenter,
				TextAlignEnum.RightCenter
			};
			for (int num7 = 0; num7 < _balanceGridColumnDataList.Count; num7++) {
				VirtaulTableColumnData virtaulTableColumnData2 = _balanceGridColumnDataList [num7];
				C1.Win.C1FlexGrid.Column column = _balanceGrid.Cols [_balanceGrid.Cols.Fixed + num7];
				if (num7 >= array.Length) {
					column.Width = (int)((float)array [array.Length - 1] * num6);
				} else {
					column.Width = (int)((float)array [num7] * num6);
				}
				column.StyleFixedNew.TextAlign = TextAlignEnum.CenterCenter;
				if (num7 >= array2.Length) {
					column.StyleNew.TextAlign = array2 [array2.Length - 1];
				} else {
					column.StyleNew.TextAlign = array2 [num7];
				}
			}
			_balanceGrid.EndUpdate ();
		} catch (Exception ex) {
			ex.Log ();
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, ex.Message);
		}
	}

	private void _balanceGrid_MouseClick (object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && _balanceGrid.HitTest (e.Location).Type == HitTestTypeEnum.Cell) {
			_ctxCellBalaceGrid.ShowContextMenu (_balanceGrid, e.Location);
		}
	}

	private void _balanceGrid_BodySelectionChanged (object sender, EventArgs e)
	{
		if (IsGridEntireColumnSelected (_balanceGrid)) {
			ChangeGridHighlightColor (_balanceGrid, isBackTransparent: true);
		} else {
			ChangeGridHighlightColor (_balanceGrid, isBackTransparent: false);
		}
	}

	protected Rectangle GetCellRangeRectUnclippedJustContainsViewport (C1FlexGridEx grid, int row1, int col1, int row2, int col2)
	{
		if (row2 - row1 <= 100) {
			return grid.GetCellRangeRectUnclipped (row1, col1, row2, col2);
		}
		int num = 0;
		for (int num2 = 0; num2 < grid.Rows.Fixed; num2++) {
			C1.Win.C1FlexGrid.Row row3 = grid.Rows [num2];
			if (row3.Visible) {
				num += row3.HeightDisplay;
			}
		}
		int num3 = grid.Height;
		int num4 = 0;
		int num5 = 0;
		int num6 = -1;
		int num7 = -1;
		int num8 = Math.Abs (grid.ScrollPosition.Y);
		bool flag = false;
		for (int num9 = grid.Rows.Fixed; num9 < grid.Rows.Count; num9++) {
			C1.Win.C1FlexGrid.Row row4 = grid.Rows [num9];
			if (!row4.Visible) {
				continue;
			}
			if (!flag) {
				if (num4 + row4.HeightDisplay + num >= num8) {
					flag = true;
					num6 = num9;
				} else {
					num4 += row4.HeightDisplay;
				}
				continue;
			}
			if (num5 - num >= num3) {
				num7 = num9;
				break;
			}
			num5 += row4.HeightDisplay;
		}
		if (num6 == -1) {
			num6 = grid.Rows.Fixed;
		}
		if (num7 == -1) {
			num7 = grid.Rows.Count - 1;
		}
		if (num6 < row1) {
			num6 = row1;
		}
		if (num7 > row2) {
			num7 = row2;
		}
		Rectangle cellRectUnclipped = grid.GetCellRectUnclipped (num6, col1);
		Rectangle cellRectUnclipped2 = grid.GetCellRectUnclipped (num7, col2);
		return new Rectangle (cellRectUnclipped.Left, cellRectUnclipped.Top, cellRectUnclipped2.Right - cellRectUnclipped.Left, cellRectUnclipped2.Bottom - cellRectUnclipped.Top);
	}

	private void _balanceGrid_Paint (object sender, PaintEventArgs e)
	{
		try {
			DrawRefIntervals (e);
			DrawSelectionRangeBorder (e);
		} catch (Exception ex) {
			ex.Log();
		}
		void DrawRefIntervals (PaintEventArgs e2)
		{
			if (Program.MainForm.FormulaEditor.RefIntervals != null) {
				LedgerVirtualTable emtpyTable = BalanceVirtualTableBuilder.GetEmtpyTable ();
				foreach (FormulaDisplayRef refInterval in Program.MainForm.FormulaEditor.RefIntervals) {
					FormulaDisplayRef ri = refInterval;
					FormulaDisplayRefKind kind = ri.Kind;
					if (kind == FormulaDisplayRefKind.Column && ri.Table == emtpyTable) {
						int num = _balanceVirtualTableColumnIndexToGridIndex [ri.Column.Index];
						int col = num + _balanceGrid.Cols.Fixed;
						DrawBorder (_balanceGrid.GetColumnRectUnclipped (col), clip: true);
					}
					void DrawBorder (Rectangle rect, bool clip)
					{
						_penFormulaRefRect.Color = ri.Color;
						rect.Offset (-1, -1);
						if (rect.Top < 0) {
							rect = new Rectangle (rect.Left, 0, rect.Width, rect.Height + rect.Top);
						}
						if (clip) {
							int num2 = ((_balanceGrid.Cols.Fixed > 0) ? (_balanceGrid.Cols [_balanceGrid.Cols.Fixed - 1].Right - 1) : 0);
							int num3 = ((_balanceGrid.Rows.Fixed > 0) ? (_balanceGrid.Rows [_balanceGrid.Rows.Fixed - 1].Bottom - 1) : 0);
							e2.Graphics.SetClip (new Rectangle (num2, num3, _balanceGrid.ClientRectangle.Width - num2, _balanceGrid.ClientRectangle.Height - num3));
						}
						e2.Graphics.DrawRectangle (_penFormulaRefRect, rect);
						_brushFormulaRefRect.Color = Color.FromArgb (20, _penFormulaRefRect.Color);
						e2.Graphics.FillRectangle (_brushFormulaRefRect, rect);
						if (clip) {
							e2.Graphics.ResetClip ();
						}
					}
				}
			}
		}
		void DrawSelectionRangeBorder (PaintEventArgs e2)
		{
			if (!IsGridEntireColumnSelected (_balanceGrid)) {
				return;
			}
			try {
				if (IsGridSelectColumnInFormula (_balanceGrid)) {
					int topRow = _balanceGrid.Selection.TopRow;
					Rectangle cellRangeRectUnclippedJustContainsViewport = GetCellRangeRectUnclippedJustContainsViewport (_balanceGrid, topRow, _balanceGrid.Selection.RightCol, _balanceGrid.Selection.BottomRow, _balanceGrid.Selection.RightCol);
					cellRangeRectUnclippedJustContainsViewport.Offset (-1, -1);
					cellRangeRectUnclippedJustContainsViewport.Inflate (-1, -1);
					_penAnimateDash.Color = ((Control.MouseButtons == MouseButtons.Left) ? Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeContext.DarkColor : Program.MainForm.FormulaEditor.NextColor);
					e2.Graphics.DrawRectangle (_penAnimateDash, cellRangeRectUnclippedJustContainsViewport);
				}
			} catch (Exception) {
			}
		}
	}

	private void _balanceGrid_MouseUp (object sender, MouseEventArgs e)
	{
		if (IsGridEntireColumnSelected (_balanceGrid)) {
			int colIndex = _balanceGrid.Selection.RightCol - _balanceGrid.Cols.Fixed;
			VirtaulTableColumnData virtaulTableColumnData = _balanceGridColumnDataList.FirstOrDefault ((VirtaulTableColumnData virtaulTableColumnData2) => virtaulTableColumnData2.GridColumnIndex == colIndex);
			if (virtaulTableColumnData != null) {
				string text = "{科目余额表}[" + virtaulTableColumnData.ColumnName + "]";
				_owner.RemoveRefAtPos ();
				_owner.InsertRefTextAndFocus (text);
				_balanceGrid.Invalidate ();
			}
		}
	}

	private void _balanceGrid_MouseMove (object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _balanceGrid.HitTest ();
		if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader) {
			_balanceGrid.Cursor = TableEditor.CursorColumnHeader;
		} else if (hitTestInfo.Type == HitTestTypeEnum.ColumnResize) {
			_voucherGrid.Cursor = GridResizingManager.CursorResizeCol;
		} else {
			_balanceGrid.Cursor = Cursors.Arrow;
		}
	}

	private void _balanceGrid_BodyOwnerDrawCell (object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col >= _balanceGridColumnDataList.Count || e.Row >= _balanceVirtualTable.Rows.Count) {
			return;
		}
		int virtualTableColumnIndex = _balanceGridColumnDataList [e.Col].VirtualTableColumnIndex;
		Auditai.Model.Cell cell = _balanceVirtualTable [e.Row, virtualTableColumnIndex];
		if (cell.Value is DateYearMonth dateYearMonth) {
			e.Text = dateYearMonth.Date.ToString ("yyyy-MM");
		} else if (cell.Value is bool flag) {
			e.Text = (flag ? "√" : string.Empty);
		} else if (cell.Value is double num) {
			if (num == 0.0) {
				e.Text = string.Empty;
			} else if (e.Col == _balanceLevelColumnIndex) {
				e.Text = num.ToString ();
			} else {
				e.Text = num.ToString ("#,0.00;-#,0.00;#");
			}
		} else if (cell.Value is string text) {
			e.Text = text;
		} else {
			e.Text = string.Empty;
		}
	}

	private void _balanceGrid_OwnerDrawCell (object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row < _balanceGrid.Rows.Fixed && e.Col >= _balanceGrid.Cols.Fixed) {
			int num = e.Col - _balanceGrid.Cols.Fixed;
			string text = _balanceGridColumnDataList [num].ColumnName;
			if (num == _balanceAccountCodeColumnIndex) {
				text += "\r\n（含辅助核算代码）";
			} else if (num == _balanceAccountFullNameColumnIndex) {
				text += "\r\n（含辅助核算名称）";
			} else if (num == _balanceAccountCurrentNameColumnIndex) {
				text += "\r\n（或辅助核算名称）";
			}
			e.Text = text;
		} else if (!_balanceVirtualTableEmpty && e.Col < _balanceGrid.Cols.Fixed && e.Row >= _balanceGrid.Rows.Fixed) {
			e.Text = (e.Row - _balanceGrid.Rows.Fixed + 1).ToString ();
		}
	}

	public void BuildVoucherGrid ()
	{
		try {
			_voucherVirtualTable = VoucherVirtualTableBuilder.GetEmtpyTable ();
			_voucherVirtualTableColumnDataList = new List<VirtaulTableColumnData> ();
			List<string> allColumnNames = VoucherVirtualTableBuilder.GetAllColumnNames ();
			for (int num = 0; num < allColumnNames.Count; num++) {
				VirtaulTableColumnData virtaulTableColumnData = new VirtaulTableColumnData ();
				virtaulTableColumnData.GridColumnIndex = num;
				virtaulTableColumnData.VirtualTableColumnIndex = num;
				virtaulTableColumnData.ColumnName = allColumnNames [num];
				virtaulTableColumnData.ColumnDataType = VoucherVirtualTableBuilder.GetColumnDataType (allColumnNames [num]);
				_voucherVirtualTableColumnDataList.Add (virtaulTableColumnData);
				switch ((VoucherVirtualTableBuilder.VoucherVirtualTableColumnIndex)virtaulTableColumnData.VirtualTableColumnIndex) {
				case VoucherVirtualTableBuilder.VoucherVirtualTableColumnIndex.科目代码:
					_voucherAccountCodeColumnIndex = num;
					break;
				case VoucherVirtualTableBuilder.VoucherVirtualTableColumnIndex.科目名称:
					_voucherAccountFullNameColumnIndex = num;
					break;
				}
			}
			_voucherVirtualTableEmpty = _voucherVirtualTable.Rows.Count == 0;
			int num2 = (_voucherVirtualTableEmpty ? 10 : _voucherVirtualTable.Rows.Count);
			_voucherGrid = new C1FlexGridEx ();
			voucherGridPage.Controls.Add (_voucherGrid);
			_voucherGrid.Dock = DockStyle.Fill;
			_voucherGrid.AllowEditing = false;
			_voucherGrid.AllowDragging = AllowDraggingEnum.None;
			_voucherGrid.AllowSorting = AllowSortingEnum.None;
			_voucherGrid.OwnerDrawCell += _voucherGrid_OwnerDrawCell;
			_voucherGrid.BodyOwnerDrawCell += _voucherGrid_BodyOwnerDrawCell;
			_voucherGrid.MouseMove += _voucherGrid_MouseMove;
			_voucherGrid.MouseUp += _voucherGrid_MouseUp;
			_voucherGrid.MouseClick += _voucherGrid_MouseClick;
			_voucherGrid.Paint += _voucherGrid_Paint;
			_voucherGrid.BodySelectionChanged += _voucherGrid_BodySelectionChanged;
			_voucherGrid.FilterManager.Context = new VoucherVirtualTableFilterContext (this);
			_voucherGrid.Rows.DefaultSize = 30;
			_voucherGrid.ExtendLastCol = false;
			_voucherGrid.FocusRect = FocusRectEnum.None;
			C1CommandLink value = _voucherGrid.FilterManager.GenLnkFilter ();
			_ctxCellVoucherGrid.CommandLinks.Add (value);
			_ctxCellVoucherGrid.CommandLinks.Add (_voucherGrid.FilterManager.GenLnkSelect ());
			_ctxCellVoucherGrid.CommandLinks.Add (_voucherGrid.FilterManager.GenLnkCancelCurrentColumn ());
			_voucherGrid.BeginUpdate ();
			int num3 = 1;
			int num4 = 1;
			_voucherGrid.Rows.Count = num2 + num3;
			_voucherGrid.Rows.Fixed = num3;
			_voucherGrid.Cols.Count = _voucherVirtualTableColumnDataList.Count + num4;
			_voucherGrid.Cols.Fixed = num4;
			_voucherGrid.Cols [0].Width = 45;
			_voucherGrid.Cols [0].StyleNew.TextAlign = TextAlignEnum.CenterCenter;
			_voucherGrid.Rows [0].Height = 40;
			float num5 = 3f;
			int[] array = new int[8] { 40, 30, 30, 60, 40, 60, 40, 40 };
			for (int num6 = 0; num6 < _voucherVirtualTableColumnDataList.Count; num6++) {
				VirtaulTableColumnData virtaulTableColumnData2 = _voucherVirtualTableColumnDataList [num6];
				C1.Win.C1FlexGrid.Column column = _voucherGrid.Cols [_voucherGrid.Cols.Fixed + num6];
				if (num6 >= array.Length) {
					column.Width = (int)((float)array [array.Length - 1] * num5);
				} else {
					column.Width = (int)((float)array [num6] * num5);
				}
				column.StyleFixedNew.TextAlign = TextAlignEnum.CenterCenter;
				if (num6 >= _voucherVirtualTableColumnDataList.Count - 2) {
					column.StyleNew.TextAlign = TextAlignEnum.RightCenter;
				} else {
					column.StyleNew.TextAlign = TextAlignEnum.LeftCenter;
				}
			}
			_voucherGrid.EndUpdate ();
		} catch (Exception ex) {
			ex.Log ();
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, ex.ToString ());
		}
	}

	private void PopulateVoucherGrid ()
	{
		if (_isVoucherGridPopuldate) {
			return;
		}
		_isVoucherGridPopuldate = true;
		try {
			LedgerVirtualTable ledgerVirtualTable = null;
			if (!Program.MainForm.MultiLedgerViewer.IsLedgerEmpty ()) {
				ledgerVirtualTable = LedgerVirtualTableUtils.GetVoucherVirtualTable (Program.MainForm.MultiLedgerViewer.CurrentLedgerViewer.Ledger);
			}
			if (ledgerVirtualTable != null) {
				_voucherGrid.BeginUpdate ();
				int num = 1;
				int num2 = 1;
				int count = ledgerVirtualTable.Rows.Count;
				_voucherGrid.Rows.Count = count + num;
				_voucherGrid.Rows.Fixed = num;
				_voucherGrid.Cols.Count = _voucherVirtualTableColumnDataList.Count + num2;
				_voucherGrid.Cols.Fixed = num2;
				_voucherVirtualTable = ledgerVirtualTable;
				_voucherVirtualTableEmpty = ledgerVirtualTable.Rows.Count == 0;
				_voucherGrid.EndUpdate ();
			}
		} catch (Exception ex) {
			ex.Log ();
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, ex.Message);
		}
	}

	private void _voucherGrid_MouseClick (object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && _voucherGrid.HitTest (e.Location).Type == HitTestTypeEnum.Cell) {
			_ctxCellVoucherGrid.ShowContextMenu (_voucherGrid, e.Location);
		}
	}

	private bool IsGridSelectColumnInFormula (C1FlexGridEx grid)
	{
		int num = grid.Selection.RightCol - grid.Cols.Fixed;
		int num2 = num;
		LedgerVirtualTable virtualTable = null;
		if (grid == _balanceGrid) {
			virtualTable = BalanceVirtualTableBuilder.GetEmtpyTable ();
			num2 = _balanceGridColumnDataList [num].VirtualTableColumnIndex;
		} else {
			if (grid != _voucherGrid) {
				return false;
			}
			virtualTable = VoucherVirtualTableBuilder.GetEmtpyTable ();
		}
		if (num2 < 0 || num2 >= virtualTable.Columns.Count) {
			return false;
		}
		Auditai.Model.Column col = virtualTable.Columns [num2];
		return Program.MainForm.FormulaEditor.RefIntervals != null && Program.MainForm.FormulaEditor.RefIntervals.Any ((FormulaDisplayRef formulaDisplayRef) => formulaDisplayRef.Table == virtualTable && formulaDisplayRef.Column == col);
	}

	private void ChangeGridHighlightColor (C1FlexGridEx grid, bool isBackTransparent)
	{
		if (isBackTransparent) {
			grid.Styles.Highlight.ForeColor = Color.Black;
			grid.Styles.Highlight.BackColor = Color.Transparent;
			grid.Styles.Highlight.Border.Color = _gridNormalBorderColor;
		} else {
			grid.Styles.Highlight.ForeColor = _gridHilightForeColor;
			grid.Styles.Highlight.BackColor = _gridHilightBackColor;
			grid.Styles.Highlight.Border.Color = _gridHightBorderColor;
		}
	}

	private void _voucherGrid_BodySelectionChanged (object sender, EventArgs e)
	{
		if (IsGridEntireColumnSelected (_voucherGrid)) {
			ChangeGridHighlightColor (_voucherGrid, isBackTransparent: true);
		} else {
			ChangeGridHighlightColor (_voucherGrid, isBackTransparent: false);
		}
	}

	private void _voucherGrid_Paint (object sender, PaintEventArgs e)
	{
		try {
			DrawRefIntervals (e);
			DrawSelectionRangeBorder (e);
		} catch (Exception ex) {
			ex.Log();
		}
		void DrawRefIntervals (PaintEventArgs e2)
		{
			if (Program.MainForm.FormulaEditor.RefIntervals != null) {
				LedgerVirtualTable emtpyTable = VoucherVirtualTableBuilder.GetEmtpyTable ();
				foreach (FormulaDisplayRef refInterval in Program.MainForm.FormulaEditor.RefIntervals) {
					FormulaDisplayRef ri = refInterval;
					FormulaDisplayRefKind kind = ri.Kind;
					if (kind == FormulaDisplayRefKind.Column && ri.Table == emtpyTable) {
						int col = ri.Column.Index + _voucherGrid.Cols.Fixed;
						DrawBorder (_voucherGrid.GetColumnRectUnclipped (col), clip: true);
					}
					void DrawBorder (Rectangle rect, bool clip)
					{
						_penFormulaRefRect.Color = ri.Color;
						rect.Offset (-1, -1);
						if (rect.Top < 0) {
							rect = new Rectangle (rect.Left, 0, rect.Width, rect.Height + rect.Top);
						}
						if (clip) {
							int num = ((_voucherGrid.Cols.Fixed > 0) ? (_voucherGrid.Cols [_voucherGrid.Cols.Fixed - 1].Right - 1) : 0);
							int num2 = ((_voucherGrid.Rows.Fixed > 0) ? (_voucherGrid.Rows [_voucherGrid.Rows.Fixed - 1].Bottom - 1) : 0);
							e2.Graphics.SetClip (new Rectangle (num, num2, _voucherGrid.ClientRectangle.Width - num, _voucherGrid.ClientRectangle.Height - num2));
						}
						e2.Graphics.DrawRectangle (_penFormulaRefRect, rect);
						_brushFormulaRefRect.Color = Color.FromArgb (20, _penFormulaRefRect.Color);
						e2.Graphics.FillRectangle (_brushFormulaRefRect, rect);
						if (clip) {
							e2.Graphics.ResetClip ();
						}
					}
				}
			}
		}
		void DrawSelectionRangeBorder (PaintEventArgs e2)
		{
			if (!IsGridEntireColumnSelected (_voucherGrid)) {
				return;
			}
			try {
				if (IsGridSelectColumnInFormula (_voucherGrid)) {
					int topRow = _voucherGrid.Selection.TopRow;
					Rectangle cellRangeRectUnclippedJustContainsViewport = GetCellRangeRectUnclippedJustContainsViewport (_voucherGrid, topRow, _voucherGrid.Selection.RightCol, _voucherGrid.Selection.BottomRow, _voucherGrid.Selection.RightCol);
					cellRangeRectUnclippedJustContainsViewport.Offset (-1, -1);
					cellRangeRectUnclippedJustContainsViewport.Inflate (-1, -1);
					_penAnimateDash.Color = ((Control.MouseButtons == MouseButtons.Left) ? Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeContext.DarkColor : Program.MainForm.FormulaEditor.NextColor);
					e2.Graphics.DrawRectangle (_penAnimateDash, cellRangeRectUnclippedJustContainsViewport);
				}
			} catch (Exception) {
			}
		}
	}

	private void _voucherGrid_MouseUp (object sender, MouseEventArgs e)
	{
		if (IsGridEntireColumnSelected (_voucherGrid)) {
			int colIndex = _voucherGrid.Selection.RightCol - _voucherGrid.Cols.Fixed;
			VirtaulTableColumnData virtaulTableColumnData = _voucherVirtualTableColumnDataList.FirstOrDefault ((VirtaulTableColumnData virtaulTableColumnData2) => virtaulTableColumnData2.GridColumnIndex == colIndex);
			if (virtaulTableColumnData != null) {
				string text = "{会计凭证表}[" + virtaulTableColumnData.ColumnName + "]";
				_owner.RemoveRefAtPos ();
				_owner.InsertRefTextAndFocus (text);
				_voucherGrid.Invalidate ();
			}
		}
	}

	private void _voucherGrid_MouseMove (object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _voucherGrid.HitTest ();
		if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader) {
			_voucherGrid.Cursor = TableEditor.CursorColumnHeader;
		} else if (hitTestInfo.Type == HitTestTypeEnum.ColumnResize) {
			_voucherGrid.Cursor = GridResizingManager.CursorResizeCol;
		} else {
			_voucherGrid.Cursor = Cursors.Arrow;
		}
	}

	private void _voucherGrid_BodyOwnerDrawCell (object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col >= _voucherVirtualTableColumnDataList.Count || e.Row >= _voucherVirtualTable.Rows.Count) {
			return;
		}
		Auditai.Model.Cell cell = _voucherVirtualTable [e.Row, e.Col];
		if (cell.Value is DateYearMonth dateYearMonth) {
			e.Text = dateYearMonth.Date.ToString ("yyyy-MM");
		} else if (cell.Value is DateTime dateTime) {
			e.Text = dateTime.ToString ("yyyy-MM-dd");
		} else if (cell.Value is double num) {
			if (num == 0.0) {
				e.Text = string.Empty;
			} else {
				e.Text = num.ToString ("#,0.00;-#,0.00;#");
			}
		} else if (cell.Value is string text) {
			e.Text = text;
		} else {
			e.Text = string.Empty;
		}
	}

	private void _voucherGrid_OwnerDrawCell (object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row < _voucherGrid.Rows.Fixed && e.Col >= _voucherGrid.Cols.Fixed) {
			int num = e.Col - _voucherGrid.Cols.Fixed;
			string text = _voucherVirtualTableColumnDataList [num].ColumnName;
			if (num == _voucherAccountCodeColumnIndex) {
				text += "\r\n（含辅助核算代码）";
			} else if (num == _voucherAccountFullNameColumnIndex) {
				text += "\r\n（含辅助核算名称）";
			}
			e.Text = text;
		} else if (!_voucherVirtualTableEmpty && e.Col < _voucherGrid.Cols.Fixed && e.Row >= _voucherGrid.Rows.Fixed) {
			e.Text = (e.Row - _voucherGrid.Rows.Fixed + 1).ToString ();
		}
	}

	private void TxbDropInput_KeyDown (object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.V) {
			e.Handled = true;
			rtbFormulaInput.SelectedText = Clipboard.GetText ();
		}
	}

	private void TxbDropInput_SelectionChanged (object sender, EventArgs e)
	{
		if (!_isTextChanging) {
			ShowFunctionHint (rtbFormulaInput.Text, rtbFormulaInput.SelectionStart);
		}
	}

	private void Form_FormClosing (object sender, FormClosingEventArgs e)
	{
		_owner.IsEditing = false;
		_owner.OnClosed ();
	}

	private void PopulateFunctions ()
	{
		List<FunctionInfo> publicFunctionInfos = FunctionInfo.PublicFunctionInfos;
		foreach (IGrouping<string, FunctionInfo> item in from functionInfo in publicFunctionInfos
			group functionInfo by functionInfo.Category) {
			C1CommandMenu c1CommandMenu = new C1CommandMenu {
				Text = item.Key
			};
			C1CommandLink value = new C1CommandLink (c1CommandMenu);
			ctxOtherFunc.CommandLinks.Add (value);
			foreach (FunctionInfo f2 in item) {
				if (Program.MainForm.IsAllowShowFunctionInfoAtCurrentView (f2, isControlFormulaForm: false, isCollectFormulaForm: true)) {
					C1Command c1Command = new C1Command {
						Text = f2.Name
					};
					C1CommandLink value2 = new C1CommandLink (c1Command);
					c1Command.Click += delegate {
						rtbFormulaInput.SelectedText = f2.Name + "()";
						rtbFormulaInput.SelectionStart = rtbFormulaInput.SelectionStart + rtbFormulaInput.SelectionLength - 1;
					};
					c1CommandMenu.CommandLinks.Add (value2);
				}
			}
		}
	}

	private void CmdInsertColumn1_Click (object sender, ClickEventArgs e)
	{
		MergeForm form = MergeForm.GetInstance ();
		form.Show (Program.MainForm.CurrentProject);
		form.AfterSelected += delegate(object obj, Auditai.Model.Column column) {
			ActiveTextBox ().SelectedText = "{" + column.Table.GetCanonicalName () + "}[" + column.GetUniqueFormulaName () + "]";
			form.Hide ();
		};
	}

	private void CmdInsertVariable_Click (object sender, ClickEventArgs e)
	{
		ReferenceEditor referenceEditor = new ReferenceEditor ();
		if (referenceEditor.ShowSelect () == DialogResult.OK) {
			Auditai.Model.DataReference selectedReference = referenceEditor.SelectedReference;
			TextBoxBase textBoxBase = ActiveTextBox ();
			textBoxBase.SelectedText = textBoxBase.SelectedText + "Var(\"" + selectedReference.Key + "\")";
		}
	}

	private void CmdPaste_Click (object sender, ClickEventArgs e)
	{
		try {
			TextBoxBase textBoxBase = ActiveTextBox ();
			string selectedText = Clipboard.GetText ();
			textBoxBase.SelectedText = selectedText;
		} catch (Exception ex) {
			ex.Log();
		}
	}

	private void CmdCut_Click (object sender, ClickEventArgs e)
	{
		TextBoxBase textBoxBase = ActiveTextBox ();
		try {
			string selectedText = textBoxBase.SelectedText;
			if (string.IsNullOrEmpty (selectedText)) {
				Clipboard.Clear ();
			} else {
				Clipboard.SetText (selectedText);
			}
		} catch (ExternalException) {
		}
		textBoxBase.SelectedText = string.Empty;
	}

	private void CmdCopy_Click (object sender, ClickEventArgs e)
	{
		try {
			TextBoxBase textBoxBase = ActiveTextBox ();
			string selectedText = textBoxBase.SelectedText;
			if (string.IsNullOrEmpty (selectedText)) {
				Clipboard.Clear ();
			} else {
				Clipboard.SetText (selectedText);
			}
		} catch (ExternalException) {
		}
	}

	private TextBoxBase ActiveTextBox ()
	{
		return rtbFormulaInput;
	}

	private void txbDropInput_TextChanged (object sender, EventArgs e)
	{
		_isTextChanging = true;
		rtbFormulaInput.SuspendDrawing ();
		QueryValueChanged = true;
		int selectionStart = rtbFormulaInput.SelectionStart;
		int selectionLength = rtbFormulaInput.SelectionLength;
		int vScrollPos = rtbFormulaInput.VScrollPos;
		try {
			FormulaDisplay formulaDisplay = new FormulaDisplay (rtbFormulaInput.Text);
			rtbFormulaInput.SelectAll ();
			rtbFormulaInput.SelectionBackColor = Color.Transparent;
			rtbFormulaInput.SelectionColor = Color.Black;
			foreach (Tuple<int, int, Color> tokenColorInterval in formulaDisplay.GetTokenColorIntervals ()) {
				rtbFormulaInput.Select (tokenColorInterval.Item1, tokenColorInterval.Item2);
				rtbFormulaInput.SelectionColor = tokenColorInterval.Item3;
			}
			Tuple<List<FormulaDisplayRef>, Color> references = formulaDisplay.GetReferences (Program.MainForm.FormulaEditor.Context);
			Program.MainForm.FormulaEditor.RefIntervals = references.Item1;
			Program.MainForm.FormulaEditor.NextColor = references.Item2;
			foreach (FormulaDisplayRef refInterval in Program.MainForm.FormulaEditor.RefIntervals) {
				foreach (Tuple<int, int> interval in refInterval.Intervals) {
					rtbFormulaInput.Select (interval.Item1, interval.Item2);
					rtbFormulaInput.SelectionColor = refInterval.Color;
				}
			}
		} catch (FormulaException) {
		} catch (Exception exception) {
			exception.Log ("账套采集公式解析时发生了异常");
		} finally {
			_isTextChanging = false;
			rtbFormulaInput.Select (selectionStart, selectionLength);
			rtbFormulaInput.VScrollPos = vScrollPos;
			rtbFormulaInput.ResumeDrawing ();
		}
	}

	private void AttachEvent ()
	{
		if (!_attachEvent) {
			rtbFormulaInput.TextChanged += txbDropInput_TextChanged;
		}
	}

	private bool ValidteAllColumnFormula (out Dictionary<Auditai.Model.Column, string> changedFormulaDic)
	{
		changedFormulaDic = null;
		Dictionary<Auditai.Model.Column, string> dictionary = new Dictionary<Auditai.Model.Column, string> ();
		foreach (Auditai.Model.Column key in _afterModifyFormulaTextDic.Keys) {
			string text = _afterModifyFormulaTextDic [key];
			text?.Trim ('\r', '\n', '\t', ' ');
			if (string.IsNullOrWhiteSpace (text)) {
				dictionary [key] = string.Empty;
				continue;
			}
			try {
				FormulaDisplay formulaDisplay = new FormulaDisplay (text);
				string text2 = formulaDisplay.ToFormula (Program.MainForm.FormulaEditor.Context);
				TryRunFormulaHandle (text2);
				dictionary [key] = text2;
			} catch (FormulaParameterCountException) {
				TryToSelectNavGridRow (key, out var tableColumnName);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName) + "函数参数数量错误");
				return false;
			} catch (FormulaFunctionNotExistException) {
				TryToSelectNavGridRow (key, out var tableColumnName2);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName2) + "函数不存在");
				return false;
			} catch (FormulaSyntaxException ex3) {
				TryToSelectNavGridRow (key, out var tableColumnName3);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName3) + "语法错误: " + ex3.Message);
				try {
					rtbFormulaInput.Select (ex3.CharPosition, 0);
				} catch {
				}
				return false;
			} catch (FormulaBadReferenceException) {
				TryToSelectNavGridRow (key, out var tableColumnName4);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName4) + "引用不存在");
				return false;
			} catch (FormulaNotApplicableException ex5) {
				TryToSelectNavGridRow (key, out var tableColumnName5);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName5) + ex5.Message);
				return false;
			} catch (FormulaBadValueException ex6) {
				TryToSelectNavGridRow (key, out var tableColumnName6);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName6) + "值范围错误: " + ex6.Message);
				return false;
			} catch (FormulaTypeMismatchException) {
				TryToSelectNavGridRow (key, out var tableColumnName7);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName7) + "参数类型错误");
				return false;
			} catch (FormulaColumnWildcardNoRowException) {
				TryToSelectNavGridRow (key, out var tableColumnName8);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName8) + "公式引用的列没有本表对应的行");
				return false;
			} catch (Exception ex9) {
				ex9.Log ();
				TryToSelectNavGridRow (key, out var tableColumnName9);
				Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, GetErrorMessagePrefix (tableColumnName9) + ex9.Message);
				return false;
			}
		}
		changedFormulaDic = new Dictionary<Auditai.Model.Column, string> ();
		foreach (Auditai.Model.Column key2 in dictionary.Keys) {
			string text3 = dictionary [key2];
			if (!_rawFormulaDic.TryGetValue (key2, out var value)) {
				changedFormulaDic.Add (key2, text3);
			} else if (value != text3) {
				changedFormulaDic.Add (key2, text3);
			}
		}
		return true;
		string GetErrorMessagePrefix (string colName)
		{
			if (_rawFormulaDic.Count <= 1) {
				return string.Empty;
			}
			return "表格列【" + colName + "】的" + Text + "不正确:\n";
		}
	}

	private bool ValidateFormula (out string formula)
	{
		formula = string.Empty;
		if (string.IsNullOrWhiteSpace (rtbFormulaInput.Text)) {
			return true;
		}
		try {
			FormulaDisplay formulaDisplay = new FormulaDisplay (rtbFormulaInput.Text);
			formula = formulaDisplay.ToFormula (Program.MainForm.FormulaEditor.Context);
			TryRunFormulaHandle (formula);
			return true;
		} catch (FormulaParameterCountException) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, "函数参数数量错误");
			return false;
		} catch (FormulaFunctionNotExistException) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, "函数不存在");
			return false;
		} catch (FormulaSyntaxException ex3) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, "语法错误。\n" + ex3.Message);
			rtbFormulaInput.Select (ex3.CharPosition, 0);
			rtbFormulaInput.Focus ();
			return false;
		} catch (FormulaBadReferenceException) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, "引用不存在");
			return false;
		} catch (FormulaNotApplicableException ex5) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, ex5.Message);
			return false;
		} catch (FormulaBadValueException ex6) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, "值范围错误\n" + ex6.Message);
			return false;
		} catch (FormulaTypeMismatchException) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, "参数类型错误");
			return false;
		} catch (FormulaColumnWildcardNoRowException) {
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, "公式引用的列没有本表对应的行");
			return false;
		} catch (Exception ex9) {
			ex9.Log ();
			Auditai.UI.Controls.MessageBox.Show (MessageBoxIcon.None, ex9.Message);
			return false;
		}
	}

	private static bool UseWildcardImpl (string formulaText, int pos)
	{
		if (pos == 0) {
			return false;
		}
		char c10 = formulaText [pos - 1];
		if (c10 == '=' || c10 == '>' || c10 == '<') {
			return true;
		}
		try {
			FormulaDisplay formulaDisplay = new FormulaDisplay (formulaText);
			Tuple<string, int> funcNameAtPos = formulaDisplay.GetFuncNameAtPos (pos);
			if ("Select".Equals (funcNameAtPos.Item1, StringComparison.OrdinalIgnoreCase) && funcNameAtPos.Item2 % 2 == 0) {
				return true;
			}
			if ("If".Equals (funcNameAtPos.Item1, StringComparison.OrdinalIgnoreCase) && funcNameAtPos.Item2 == 0) {
				return true;
			}
		} catch (FormulaException) {
			return false;
		}
		return false;
	}

	protected override void Dispose (bool disposing)
	{
		if (disposing && components != null) {
			components.Dispose ();
		}
		base.Dispose (disposing);
	}

	private void InitializeComponent ()
	{
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer ();
		this.pnlLeftPart = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.pnlRightPart = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.ctnDock = new C1.Win.C1SplitContainer.C1SplitContainer ();
		this.pnlGrid = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.tabDockGrid = new C1.Win.C1Command.C1DockingTab ();
		this.balanceGridPage = new C1.Win.C1Command.C1DockingTabPage ();
		this.voucherGridPage = new C1.Win.C1Command.C1DockingTabPage ();
		this.pnlInput = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.ctnInput = new C1.Win.C1SplitContainer.C1SplitContainer ();
		this.pnlFunctions = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.tbrFunctions = new C1.Win.C1Command.C1ToolBar ();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder ();
		this.c1ContextMenu1 = new C1.Win.C1Command.C1ContextMenu ();
		this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink ();
		this.txbDropInputContextMenu = new C1.Win.C1Command.C1ContextMenu ();
		this.pnlFormulaInput = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.rtbFormulaInput = new System.Windows.Forms.RichTextBoxEx ();
		this.pnlFunctionHint = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.lblFunctionHint = new C1.Win.C1Input.C1Label ();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel ();
		this.btnCancle = new C1.Win.C1Input.C1Button ();
		this.btnConfirm = new C1.Win.C1Input.C1Button ();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit ();
		this.ctnAll.SuspendLayout ();
		this.pnlRightPart.SuspendLayout ();
		((System.ComponentModel.ISupportInitialize)this.ctnDock).BeginInit ();
		this.ctnDock.SuspendLayout ();
		this.pnlGrid.SuspendLayout ();
		((System.ComponentModel.ISupportInitialize)this.tabDockGrid).BeginInit ();
		this.tabDockGrid.SuspendLayout ();
		this.pnlInput.SuspendLayout ();
		((System.ComponentModel.ISupportInitialize)this.ctnInput).BeginInit ();
		this.ctnInput.SuspendLayout ();
		this.pnlFunctions.SuspendLayout ();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit ();
		this.pnlFormulaInput.SuspendLayout ();
		this.pnlFunctionHint.SuspendLayout ();
		((System.ComponentModel.ISupportInitialize)this.lblFunctionHint).BeginInit ();
		this.pnlButtons.SuspendLayout ();
		((System.ComponentModel.ISupportInitialize)this.btnCancle).BeginInit ();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit ();
		base.SuspendLayout ();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.Location = new System.Drawing.Point (0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add (this.pnlLeftPart);
		this.ctnAll.Panels.Add (this.pnlRightPart);
		this.ctnAll.Size = new System.Drawing.Size (960, 620);
		this.ctnAll.TabIndex = 0;
		this.ctnAll.UseParentVisualStyle = false;
		this.pnlLeftPart.BackColor = System.Drawing.SystemColors.Control;
		this.pnlLeftPart.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlLeftPart.KeepRelativeSize = false;
		this.pnlLeftPart.Location = new System.Drawing.Point (0, 0);
		this.pnlLeftPart.Name = "pnlLeftPart";
		this.pnlLeftPart.Size = new System.Drawing.Size (150, 620);
		this.pnlLeftPart.SizeRatio = 19.036;
		this.pnlLeftPart.TabIndex = 0;
		this.pnlLeftPart.Width = 150;
		this.pnlRightPart.Controls.Add (this.ctnDock);
		this.pnlRightPart.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Right;
		this.pnlRightPart.Location = new System.Drawing.Point (154, 0);
		this.pnlRightPart.Name = "pnlRightPart";
		this.pnlRightPart.Size = new System.Drawing.Size (810, 620);
		this.pnlRightPart.TabIndex = 1;
		this.pnlRightPart.Width = 810;
		this.ctnDock.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnDock.CollapsingAreaColor = System.Drawing.Color.FromArgb (221, 231, 238);
		this.ctnDock.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnDock.FixedLineColor = System.Drawing.Color.FromArgb (119, 147, 185);
		this.ctnDock.FixedLineWidth = 0;
		this.ctnDock.ForeColor = System.Drawing.Color.FromArgb (21, 66, 139);
		this.ctnDock.HeaderHeight = 27;
		this.ctnDock.LineBelowHeader = false;
		this.ctnDock.Location = new System.Drawing.Point (0, 0);
		this.ctnDock.Margin = new System.Windows.Forms.Padding (3, 4, 3, 4);
		this.ctnDock.Name = "ctnDock";
		this.ctnDock.Panels.Add (this.pnlGrid);
		this.ctnDock.Panels.Add (this.pnlInput);
		this.ctnDock.Panels.Add (this.pnlButtons);
		this.ctnDock.Size = new System.Drawing.Size (810, 620);
		this.ctnDock.SplitterColor = System.Drawing.Color.LightGray;
		this.ctnDock.SplitterMovingColor = System.Drawing.Color.Silver;
		this.ctnDock.SplitterWidth = 2;
		this.ctnDock.TabIndex = 1;
		this.ctnDock.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.ctnDock.UseParentVisualStyle = false;
		this.pnlGrid.Controls.Add (this.tabDockGrid);
		this.pnlGrid.Height = 129;
		this.pnlGrid.Location = new System.Drawing.Point (0, 0);
		this.pnlGrid.MinHeight = 0;
		this.pnlGrid.Name = "pnlGrid";
		this.pnlGrid.Size = new System.Drawing.Size (770, 129);
		this.pnlGrid.SizeRatio = 26.362;
		this.pnlGrid.TabIndex = 3;
		this.tabDockGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tabDockGrid.Controls.Add (this.balanceGridPage);
		this.tabDockGrid.Controls.Add (this.voucherGridPage);
		this.tabDockGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabDockGrid.Location = new System.Drawing.Point (0, 0);
		this.tabDockGrid.Name = "tabDockGrid";
		this.tabDockGrid.SelectedIndex = 2;
		this.tabDockGrid.Size = new System.Drawing.Size (770, 129);
		this.tabDockGrid.TabIndex = 0;
		this.tabDockGrid.TabsShowFocusCues = false;
		this.tabDockGrid.TabsSpacing = 0;
		this.balanceGridPage.Location = new System.Drawing.Point (0, 27);
		this.balanceGridPage.Name = "balanceGridPage";
		this.balanceGridPage.Size = new System.Drawing.Size (770, 102);
		this.balanceGridPage.TabIndex = 0;
		this.balanceGridPage.Text = "科目余额表";
		this.voucherGridPage.Location = new System.Drawing.Point (0, 27);
		this.voucherGridPage.Name = "voucherGridPage";
		this.voucherGridPage.Size = new System.Drawing.Size (770, 102);
		this.voucherGridPage.TabIndex = 1;
		this.voucherGridPage.Text = "会计凭证表";
		this.pnlInput.Controls.Add (this.ctnInput);
		this.pnlInput.Height = 396;
		this.pnlInput.Location = new System.Drawing.Point (0, 131);
		this.pnlInput.MinHeight = 100;
		this.pnlInput.MinWidth = 52;
		this.pnlInput.Name = "pnlInput";
		this.pnlInput.Size = new System.Drawing.Size (770, 396);
		this.pnlInput.SizeRatio = 100.0;
		this.pnlInput.TabIndex = 1;
		this.pnlInput.Width = 770;
		this.ctnInput.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnInput.CollapsingCueColor = System.Drawing.Color.FromArgb (133, 133, 150);
		this.ctnInput.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnInput.FixedLineWidth = 0;
		this.ctnInput.ForeColor = System.Drawing.Color.FromArgb (0, 0, 0);
		this.ctnInput.LineBelowHeader = false;
		this.ctnInput.Location = new System.Drawing.Point (0, 0);
		this.ctnInput.Name = "ctnInput";
		this.ctnInput.Panels.Add (this.pnlFunctions);
		this.ctnInput.Panels.Add (this.pnlFormulaInput);
		this.ctnInput.Panels.Add (this.pnlFunctionHint);
		this.ctnInput.Size = new System.Drawing.Size (770, 396);
		this.ctnInput.SplitterColor = System.Drawing.Color.LightGray;
		this.ctnInput.SplitterMovingColor = System.Drawing.Color.Silver;
		this.ctnInput.SplitterWidth = 2;
		this.ctnInput.TabIndex = 0;
		this.ctnInput.UseParentVisualStyle = false;
		this.pnlFunctions.Controls.Add (this.tbrFunctions);
		this.pnlFunctions.Height = 30;
		this.pnlFunctions.KeepRelativeSize = false;
		this.pnlFunctions.Location = new System.Drawing.Point (0, 0);
		this.pnlFunctions.MinHeight = 30;
		this.pnlFunctions.Name = "pnlFunctions";
		this.pnlFunctions.Resizable = false;
		this.pnlFunctions.Size = new System.Drawing.Size (770, 30);
		this.pnlFunctions.SizeRatio = 11.321;
		this.pnlFunctions.TabIndex = 3;
		this.tbrFunctions.AutoSize = false;
		this.tbrFunctions.CommandHolder = this.c1CommandHolder1;
		this.tbrFunctions.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tbrFunctions.Location = new System.Drawing.Point (0, 0);
		this.tbrFunctions.MinButtonSize = 30;
		this.tbrFunctions.Movable = false;
		this.tbrFunctions.Name = "tbrFunctions";
		this.tbrFunctions.Size = new System.Drawing.Size (770, 30);
		this.tbrFunctions.Text = "c1ToolBar1";
		this.tbrFunctions.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.tbrFunctions.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.c1CommandHolder1.Commands.Add (this.c1ContextMenu1);
		this.c1CommandHolder1.Commands.Add (this.txbDropInputContextMenu);
		this.c1CommandHolder1.Owner = this;
		this.c1ContextMenu1.CommandLinks.AddRange (new C1.Win.C1Command.C1CommandLink[1] { this.c1CommandLink1 });
		this.c1ContextMenu1.Name = "c1ContextMenu1";
		this.c1ContextMenu1.ShortcutText = "";
		this.c1CommandLink1.Text = "新命令";
		this.txbDropInputContextMenu.Name = "txbDropInputContextMenu";
		this.txbDropInputContextMenu.ShortcutText = "";
		this.pnlFormulaInput.Controls.Add (this.rtbFormulaInput);
		this.pnlFormulaInput.Height = 286;
		this.pnlFormulaInput.Location = new System.Drawing.Point (0, 30);
		this.pnlFormulaInput.Name = "pnlFormulaInput";
		this.pnlFormulaInput.Resizable = false;
		this.pnlFormulaInput.Size = new System.Drawing.Size (770, 286);
		this.pnlFormulaInput.SizeRatio = 100.0;
		this.pnlFormulaInput.TabIndex = 2;
		this.rtbFormulaInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.rtbFormulaInput.Dock = System.Windows.Forms.DockStyle.Fill;
		this.rtbFormulaInput.Location = new System.Drawing.Point (0, 0);
		this.rtbFormulaInput.Margin = new System.Windows.Forms.Padding (3, 4, 3, 4);
		this.rtbFormulaInput.Name = "rtbFormulaInput";
		this.rtbFormulaInput.Size = new System.Drawing.Size (770, 286);
		this.rtbFormulaInput.TabIndex = 0;
		this.rtbFormulaInput.Text = "";
		this.rtbFormulaInput.VScrollPos = 0;
		this.pnlFunctionHint.Controls.Add (this.lblFunctionHint);
		this.pnlFunctionHint.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlFunctionHint.Height = 80;
		this.pnlFunctionHint.KeepRelativeSize = false;
		this.pnlFunctionHint.Location = new System.Drawing.Point (0, 316);
		this.pnlFunctionHint.MinHeight = 80;
		this.pnlFunctionHint.Name = "pnlFunctionHint";
		this.pnlFunctionHint.Resizable = false;
		this.pnlFunctionHint.Size = new System.Drawing.Size (770, 80);
		this.pnlFunctionHint.SizeRatio = 10.0;
		this.pnlFunctionHint.TabIndex = 4;
		this.lblFunctionHint.BackColor = System.Drawing.SystemColors.Control;
		this.lblFunctionHint.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblFunctionHint.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblFunctionHint.Location = new System.Drawing.Point (0, 0);
		this.lblFunctionHint.Name = "lblFunctionHint";
		this.lblFunctionHint.Size = new System.Drawing.Size (770, 80);
		this.lblFunctionHint.TabIndex = 0;
		this.lblFunctionHint.Tag = null;
		this.lblFunctionHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.pnlButtons.BackColor = System.Drawing.SystemColors.Control;
		this.pnlButtons.Controls.Add (this.btnCancle);
		this.pnlButtons.Controls.Add (this.btnConfirm);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 40;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point (0, 529);
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Resizable = false;
		this.pnlButtons.Size = new System.Drawing.Size (770, 40);
		this.pnlButtons.SizeRatio = 100.0;
		this.pnlButtons.TabIndex = 2;
		this.pnlButtons.Width = 770;
		this.btnCancle.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancle.Location = new System.Drawing.Point (655, 8);
		this.btnCancle.Margin = new System.Windows.Forms.Padding (3, 4, 3, 4);
		this.btnCancle.Name = "btnCancle";
		this.btnCancle.Size = new System.Drawing.Size (70, 26);
		this.btnCancle.TabIndex = 1;
		this.btnCancle.Text = "取消";
		this.btnCancle.UseVisualStyleBackColor = true;
		this.btnCancle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.btnCancle.Click += new System.EventHandler (btnCancle_Click);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Location = new System.Drawing.Point (555, 8);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding (3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size (70, 26);
		this.btnConfirm.TabIndex = 0;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler (btnConfirm_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF (7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size (960, 620);
		base.Controls.Add (this.ctnAll);
		this.Font = new System.Drawing.Font ("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.AcceptButton = this.btnConfirm;
		base.CancelButton = this.btnCancle;
		base.Margin = new System.Windows.Forms.Padding (3, 4, 3, 4);
		base.Name = "frmLedgerCollectFormulaEdit";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "账套采集公式";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit ();
		this.ctnAll.ResumeLayout (false);
		this.pnlRightPart.ResumeLayout (false);
		((System.ComponentModel.ISupportInitialize)this.ctnDock).EndInit ();
		this.ctnDock.ResumeLayout (false);
		this.pnlGrid.ResumeLayout (false);
		((System.ComponentModel.ISupportInitialize)this.tabDockGrid).EndInit ();
		this.tabDockGrid.ResumeLayout (false);
		this.pnlInput.ResumeLayout (false);
		((System.ComponentModel.ISupportInitialize)this.ctnInput).EndInit ();
		this.ctnInput.ResumeLayout (false);
		this.pnlFunctions.ResumeLayout (false);
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit ();
		this.pnlFormulaInput.ResumeLayout (false);
		this.pnlFunctionHint.ResumeLayout (false);
		((System.ComponentModel.ISupportInitialize)this.lblFunctionHint).EndInit ();
		this.pnlButtons.ResumeLayout (false);
		((System.ComponentModel.ISupportInitialize)this.btnCancle).EndInit ();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit ();
		base.ResumeLayout (false);
	}
}
