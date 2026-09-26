using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.C1Excel;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

// 多维核算主视图。
// 左侧科目树复用 AccountTreeEditor.Tree（LedgerViewer.SwitchToView 已处理 ShowTree，本类无 Tree 属性）。
// 条件工具栏（账期/凭证号/金额/视图切换/操作按钮）+ 动态维度筛选行 + 结果网格（明细/汇总两种视图）。
internal class MultiDimEditor : ISetTheme
{
	private const string TAG_TOTAL_ROW = "tag_total_row";

	// 期初无法确定（跨轴且无组合期初/期初行）时方向与余额列的诚实展示
	private const string UnknownText = "—";

	private enum ViewMode
	{
		Detail = 0,
		Summary = 1
	}

	private sealed class ConditionRow
	{
		public Panel Root;

		public ComboBox Cbo;

		public Button BtnPick;

		/// <summary>勾选的维度值；空列表 = 该类别按值不过滤</summary>
		public List<AuxiliaryItem> Selected = new List<AuxiliaryItem>();

		/// <summary>当前选中的类别名（用于判定切换并清空勾选）</summary>
		public string LastClassName;
	}

	// 维度值多选对话框（CheckedListBox + 确定/取消；按钮排布按项目惯例：取消贴右、右缘距窗体 50、宽 110）
	private sealed class AuxPickForm : Form
	{
		private readonly CheckedListBox _list;

		private readonly List<AuxiliaryItem> _selected = new List<AuxiliaryItem>();

		public AuxPickForm(string className, List<AuxiliaryItem> items, List<AuxiliaryItem> initial)
		{
			Font = new Font("微软雅黑", 9f);
			Text = "选择" + className + "值";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			ClientSize = new Size(380, 460);
			_list = new CheckedListBox
			{
				Dock = DockStyle.Fill,
				CheckOnClick = true,
				Font = new Font("微软雅黑", 9f),
				DisplayMember = "Name",
				BorderStyle = BorderStyle.FixedSingle
			};
			foreach (AuxiliaryItem item in items)
			{
				_list.Items.Add(item, initial != null && initial.Contains(item));
			}
			Button btnOk = new Button
			{
				Text = "确定",
				// 取消贴右（右缘距窗体 50），确定在左侧间距 12
				Location = new Point(ClientSize.Width - 50 - 110 - 12 - 110, ClientSize.Height - 40 - 12),
				Size = new Size(110, 30)
			};
			btnOk.Click += delegate
			{
				_selected.Clear();
				foreach (AuxiliaryItem checkedItem in _list.CheckedItems)
				{
					_selected.Add(checkedItem);
				}
				base.DialogResult = DialogResult.OK;
				Close();
			};
			Button btnCancel = new Button
			{
				Text = "取消",
				Location = new Point(ClientSize.Width - 50 - 110, ClientSize.Height - 40 - 12),
				Size = new Size(110, 30)
			};
			btnCancel.Click += delegate
			{
				base.DialogResult = DialogResult.Cancel;
				Close();
			};
			base.Controls.Add(_list);
			base.Controls.Add(btnOk);
			base.Controls.Add(btnCancel);
			base.AcceptButton = btnOk;
			base.CancelButton = btnCancel;
		}

		public List<AuxiliaryItem> GetSelected()
		{
			return new List<AuxiliaryItem>(_selected);
		}
	}

	// 明细视图导出：列序与网格一致（GroupOpening→期初余额、RunningBalance→期末余额）
	private sealed class DetailExporter : LedgerExporter
	{
		private readonly List<MultiDimGroup> _groups;

		private readonly List<string> _dims;

		public DetailExporter(List<MultiDimGroup> groups, List<string> dims)
		{
			_groups = groups;
			_dims = dims;
		}

		public override void Build()
		{
			XLSheet xLSheet = xlBook.Sheets[0];
			List<string> headers = new List<string>
			{
				"序号", "记录类型", "日期", "凭证号", "科目编码", "科目名称", "摘要", "借方金额", "贷方金额", "期初方向", "期初余额", "期末方向", "期末余额"
			};
			headers.AddRange(_dims);
			int[] widths = new int[13] { 40, 70, 80, 80, 90, 120, 200, 90, 90, 50, 100, 50, 100 };
			for (int i = 0; i < headers.Count; i++)
			{
				xLSheet[0, i].SetValue(headers[i], styleHCenter);
				int width = ((i < widths.Length) ? widths[i] : 110);
				xLSheet.Columns[i].Width = C1XLBook.PixelsToTwips((double)width);
			}
			int num = 0;
			int no = 0;
			foreach (MultiDimGroup group in _groups)
			{
				foreach (MultiDimRow row in group.Rows)
				{
					no++;
					num++;
					WriteRow(xLSheet, num, no, row);
				}
			}
			bool anyUnknown = _groups.Any((MultiDimGroup g) => !g.OpeningKnown);
			decimal opening = 0m;
			decimal debit = 0m;
			decimal credit = 0m;
			decimal ending = 0m;
			foreach (MultiDimGroup g in _groups)
			{
				opening += g.OpeningBalance;
				debit += g.Debit;
				credit += g.Credit;
				ending += g.EndingBalance;
			}
			num++;
			xLSheet[num, 0].SetValue("", styleBorder);
			xLSheet[num, 1].SetValue("", styleBorder);
			xLSheet[num, 2].SetValue("", styleBorder);
			xLSheet[num, 3].SetValue("", styleBorder);
			xLSheet[num, 4].SetValue("", styleBorder);
			xLSheet[num, 5].SetValue("", styleBorder);
			xLSheet[num, 6].SetValue("合 计", styleBorder);
			xLSheet[num, 7].SetValue(EmptyIf0(debit), styleMoney);
			xLSheet[num, 8].SetValue(EmptyIf0(credit), styleMoney);
			if (anyUnknown)
			{
				xLSheet[num, 9].SetValue(UnknownText, styleHCenter);
				xLSheet[num, 10].SetValue(UnknownText, styleBorder);
				xLSheet[num, 11].SetValue(UnknownText, styleHCenter);
				xLSheet[num, 12].SetValue(UnknownText, styleBorder);
			}
			else
			{
				xLSheet[num, 9].SetValue(DirChar(opening), styleHCenter);
				xLSheet[num, 10].SetValue(EmptyIf0(Math.Abs(opening)), styleMoney);
				xLSheet[num, 11].SetValue(DirChar(ending), styleHCenter);
				xLSheet[num, 12].SetValue(EmptyIf0(Math.Abs(ending)), styleMoney);
			}
			for (int j = 0; j < _dims.Count; j++)
			{
				xLSheet[num, 13 + j].SetValue("", styleBorder);
			}
			foreach (XLRow item in (IEnumerable)xLSheet.Rows)
			{
				item.Height = C1XLBook.PixelsToTwips(30.0);
			}
		}

		private void WriteRow(XLSheet sheet, int row, int no, MultiDimRow r)
		{
			sheet[row, 0].SetValue(no, styleHCenter);
			sheet[row, 1].SetValue((r.RecordType == MultiDimRecordType.Opening) ? "期初余额" : "发生记录", styleBorder);
			if (r.Day.HasValue)
			{
				sheet[row, 2].SetValue(r.Day.Value, styleDateTime);
			}
			else
			{
				sheet[row, 2].SetValue("", styleBorder);
			}
			sheet[row, 3].SetValue(r.VoucherText ?? "", styleBorder);
			sheet[row, 4].SetValue(r.Account?.Code ?? "", styleBorder);
			sheet[row, 5].SetValue(r.Account?.Name ?? "", styleBorder);
			sheet[row, 6].SetValue(r.Digest ?? "", styleBorder);
			sheet[row, 7].SetValue(EmptyIf0(r.Debit), styleMoney);
			sheet[row, 8].SetValue(EmptyIf0(r.Credit), styleMoney);
			if (r.OpeningKnown)
			{
				sheet[row, 9].SetValue(DirChar(r.GroupOpening), styleHCenter);
				sheet[row, 10].SetValue(EmptyIf0(Math.Abs(r.GroupOpening)), styleMoney);
				sheet[row, 11].SetValue(DirChar(r.RunningBalance), styleHCenter);
				sheet[row, 12].SetValue(EmptyIf0(Math.Abs(r.RunningBalance)), styleMoney);
			}
			else
			{
				sheet[row, 9].SetValue(UnknownText, styleHCenter);
				sheet[row, 10].SetValue(UnknownText, styleBorder);
				sheet[row, 11].SetValue(UnknownText, styleHCenter);
				sheet[row, 12].SetValue(UnknownText, styleBorder);
			}
			for (int i = 0; i < _dims.Count; i++)
			{
				sheet[row, 13 + i].SetValue(r.GetDimension(_dims[i]), styleBorder);
			}
		}
	}

	// 汇总视图导出：每组一行 + 合计行
	private sealed class SummaryExporter : LedgerExporter
	{
		private readonly List<MultiDimGroup> _groups;

		private readonly List<string> _dims;

		public SummaryExporter(List<MultiDimGroup> groups, List<string> dims)
		{
			_groups = groups;
			_dims = dims;
		}

		public override void Build()
		{
			XLSheet xLSheet = xlBook.Sheets[0];
			List<string> headers = new List<string> { "科目编码", "科目名称" };
			headers.AddRange(_dims);
			headers.Add("期初方向");
			headers.Add("期初余额");
			headers.Add("借方发生额");
			headers.Add("贷方发生额");
			headers.Add("期末方向");
			headers.Add("期末余额");
			int[] widths = new int[8] { 90, 120, 50, 100, 110, 110, 50, 100 };
			for (int i = 0; i < headers.Count; i++)
			{
				xLSheet[0, i].SetValue(headers[i], styleHCenter);
				int width = ((i >= 2 && i < 2 + _dims.Count) ? 110 : ((i < 2) ? widths[i] : widths[i - _dims.Count]));
				xLSheet.Columns[i].Width = C1XLBook.PixelsToTwips((double)width);
			}
			int num = 0;
			foreach (MultiDimGroup group in _groups)
			{
				num++;
				WriteRow(xLSheet, num, group);
			}
			bool anyUnknown = _groups.Any((MultiDimGroup g) => !g.OpeningKnown);
			decimal opening = 0m;
			decimal debit = 0m;
			decimal credit = 0m;
			decimal ending = 0m;
			foreach (MultiDimGroup g in _groups)
			{
				opening += g.OpeningBalance;
				debit += g.Debit;
				credit += g.Credit;
				ending += g.EndingBalance;
			}
			num++;
			xLSheet[num, 0].SetValue("", styleBorder);
			xLSheet[num, 1].SetValue("合 计", styleBorder);
			for (int j = 0; j < _dims.Count; j++)
			{
				xLSheet[num, 2 + j].SetValue("", styleBorder);
			}
			int col = 2 + _dims.Count;
			xLSheet[num, col + 2].SetValue(EmptyIf0(debit), styleMoney);
			xLSheet[num, col + 3].SetValue(EmptyIf0(credit), styleMoney);
			if (anyUnknown)
			{
				xLSheet[num, col].SetValue(UnknownText, styleHCenter);
				xLSheet[num, col + 1].SetValue(UnknownText, styleBorder);
				xLSheet[num, col + 4].SetValue(UnknownText, styleHCenter);
				xLSheet[num, col + 5].SetValue(UnknownText, styleBorder);
			}
			else
			{
				xLSheet[num, col].SetValue(DirChar(opening), styleHCenter);
				xLSheet[num, col + 1].SetValue(EmptyIf0(Math.Abs(opening)), styleMoney);
				xLSheet[num, col + 4].SetValue(DirChar(ending), styleHCenter);
				xLSheet[num, col + 5].SetValue(EmptyIf0(Math.Abs(ending)), styleMoney);
			}
			foreach (XLRow item in (IEnumerable)xLSheet.Rows)
			{
				item.Height = C1XLBook.PixelsToTwips(30.0);
			}
		}

		private void WriteRow(XLSheet sheet, int row, MultiDimGroup g)
		{
			sheet[row, 0].SetValue(g.Account?.Code ?? "", styleBorder);
			sheet[row, 1].SetValue(g.Account?.Name ?? "", styleBorder);
			for (int i = 0; i < _dims.Count; i++)
			{
				sheet[row, 2 + i].SetValue(g.DimensionValues.TryGetValue(_dims[i], out string v) ? (v ?? "") : "", styleBorder);
			}
			int col = 2 + _dims.Count;
			sheet[row, col + 2].SetValue(EmptyIf0(g.Debit), styleMoney);
			sheet[row, col + 3].SetValue(EmptyIf0(g.Credit), styleMoney);
			if (g.OpeningKnown)
			{
				sheet[row, col].SetValue(DirChar(g.OpeningBalance), styleHCenter);
				sheet[row, col + 1].SetValue(EmptyIf0(Math.Abs(g.OpeningBalance)), styleMoney);
				sheet[row, col + 4].SetValue(DirChar(g.EndingBalance), styleHCenter);
				sheet[row, col + 5].SetValue(EmptyIf0(Math.Abs(g.EndingBalance)), styleMoney);
			}
			else
			{
				sheet[row, col].SetValue(UnknownText, styleHCenter);
				sheet[row, col + 1].SetValue(UnknownText, styleBorder);
				sheet[row, col + 4].SetValue(UnknownText, styleHCenter);
				sheet[row, col + 5].SetValue(UnknownText, styleBorder);
			}
		}
	}

	private readonly LedgerViewer _owner;

	private C1FlexGridEx grd;

	private Label lblEmpty;

	private FlowLayoutPanel flowToolbar;

	private FlowLayoutPanel flowConds;

	private DateTimePicker dtpStart;

	private DateTimePicker dtpEnd;

	private TextBox txtVoucher;

	private TextBox txtMin;

	private TextBox txtMax;

	private RadioButton rbDetail;

	private RadioButton rbSummary;

	private Button btnQuery;

	private C1.Win.C1FlexGrid.CellStyle _headerStyle;

	private C1.Win.C1FlexGrid.CellStyle _totalStyle;

	private readonly List<ConditionRow> _condRows = new List<ConditionRow>();

	private List<MultiDimGroup> _lastGroups;

	private ViewMode _viewMode = ViewMode.Detail;

	private bool _querying;

	private bool _suppressCondEvent;

	private bool _focusPending = true;

	// 当前视图数据绑定的账套；检测到账套重载（OpenLedger 换新对象）时需作废缓存
	private Ledger _boundLedger;

	public Panel View { get; private set; }

	public MultiDimEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitComponent();
		InitPeriod();
		RebuildGridOnly();
		_boundLedger = owner.Ledger;
	}

	private void InitComponent()
	{
		View = new Panel();
		View.Dock = DockStyle.Fill;
		View.BackColor = Color.FromArgb(243, 244, 246);
		grd = new C1FlexGridEx();
		SetupGrid(grd);
		_headerStyle = grd.Styles.Add("mdHeader");
		_headerStyle.TextAlign = TextAlignEnum.CenterCenter;
		_totalStyle = grd.Styles.Add("mdTotal");
		_totalStyle.BackColor = Color.LightYellow;
		_totalStyle.Font = new Font("微软雅黑", 9.5f, FontStyle.Bold);
		grd.DoubleClick += Grd_DoubleClick;
		grd.BindAutoSizeColsFill(View);
		lblEmpty = new Label
		{
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleCenter,
			Font = new Font("微软雅黑", 10f),
			ForeColor = Color.FromArgb(107, 114, 128),
			BackColor = Color.White,
			Visible = false
		};
		BuildToolbar();
		BuildCondPanel();
		// 布局顺序：工具栏最上 → 维度筛选行 → 空态提示/网格填充余下空间
		View.Controls.Add(grd);
		View.Controls.Add(lblEmpty);
		View.Controls.Add(flowConds);
		View.Controls.Add(flowToolbar);
	}

	private void BuildToolbar()
	{
		flowToolbar = new FlowLayoutPanel();
		flowToolbar.Dock = DockStyle.Top;
		flowToolbar.AutoSize = true;
		flowToolbar.WrapContents = true;
		flowToolbar.Padding = new Padding(8, 6, 8, 4);
		flowToolbar.BackColor = Color.White;
		flowToolbar.Paint += delegate(object s, PaintEventArgs e)
		{
			e.Graphics.DrawLine(Pens.Gainsboro, 0, flowToolbar.Height - 1, flowToolbar.Width, flowToolbar.Height - 1);
		};
		Label lblPeriod = MakeLabel("账期");
		dtpStart = MakeMonthPicker();
		Label lblTo = MakeLabel("至");
		dtpEnd = MakeMonthPicker();
		Label lblVoucher = MakeLabel("凭证号");
		txtVoucher = new TextBox
		{
			Font = new Font("微软雅黑", 9f),
			Width = 100,
			Margin = new Padding(0, 4, 8, 0)
		};
		Label lblAmount = MakeLabel("金额");
		txtMin = new TextBox
		{
			Font = new Font("微软雅黑", 9f),
			Width = 76,
			Margin = new Padding(0, 4, 2, 0)
		};
		Label lblAmountTo = MakeLabel("～");
		txtMax = new TextBox
		{
			Font = new Font("微软雅黑", 9f),
			Width = 76,
			Margin = new Padding(0, 4, 8, 0)
		};
		rbDetail = new RadioButton
		{
			Text = "明细",
			Checked = true,
			AutoSize = true,
			Font = new Font("微软雅黑", 9f),
			Margin = new Padding(4, 5, 2, 0),
			ForeColor = Color.FromArgb(30, 41, 59)
		};
		rbSummary = new RadioButton
		{
			Text = "汇总",
			AutoSize = true,
			Font = new Font("微软雅黑", 9f),
			Margin = new Padding(4, 5, 12, 0),
			ForeColor = Color.FromArgb(30, 41, 59)
		};
		rbDetail.CheckedChanged += delegate
		{
			_viewMode = (rbDetail.Checked ? ViewMode.Detail : ViewMode.Summary);
			RebuildView();
		};
		btnQuery = MakeButton("查 询", 88);
		Button btnExport = MakeButton("导出 Excel", 96);
		Button btnAddCond = MakeButton("添加条件", 88);
		Button btnClearCond = MakeButton("清空条件", 88);
		btnQuery.Click += delegate
		{
			RunQuery();
		};
		btnExport.Click += delegate
		{
			ExportExcel();
		};
		btnAddCond.Click += delegate
		{
			AddConditionRow();
		};
		btnClearCond.Click += delegate
		{
			ClearConditions();
		};
		KeyEventHandler enterToQuery = delegate(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				RunQuery();
			}
		};
		txtVoucher.KeyDown += enterToQuery;
		txtMin.KeyDown += enterToQuery;
		txtMax.KeyDown += enterToQuery;
		flowToolbar.Controls.Add(lblPeriod);
		flowToolbar.Controls.Add(dtpStart);
		flowToolbar.Controls.Add(lblTo);
		flowToolbar.Controls.Add(dtpEnd);
		flowToolbar.Controls.Add(lblVoucher);
		flowToolbar.Controls.Add(txtVoucher);
		flowToolbar.Controls.Add(lblAmount);
		flowToolbar.Controls.Add(txtMin);
		flowToolbar.Controls.Add(lblAmountTo);
		flowToolbar.Controls.Add(txtMax);
		flowToolbar.Controls.Add(rbDetail);
		flowToolbar.Controls.Add(rbSummary);
		flowToolbar.Controls.Add(btnQuery);
		flowToolbar.Controls.Add(btnExport);
		flowToolbar.Controls.Add(btnAddCond);
		flowToolbar.Controls.Add(btnClearCond);
	}

	private void BuildCondPanel()
	{
		flowConds = new FlowLayoutPanel();
		flowConds.Dock = DockStyle.Top;
		flowConds.FlowDirection = FlowDirection.TopDown;
		flowConds.WrapContents = false;
		flowConds.AutoSize = true;
		flowConds.Padding = new Padding(8, 4, 8, 6);
		flowConds.BackColor = Color.White;
		flowConds.Visible = false;
		flowConds.Paint += delegate(object s, PaintEventArgs e)
		{
			e.Graphics.DrawLine(Pens.Gainsboro, 0, flowConds.Height - 1, flowConds.Width, flowConds.Height - 1);
		};
	}

	private void InitPeriod()
	{
		// 账期默认：起始=账套 StartDate 当月，结束=账套内最大凭证日当月（无凭证取 StartDate 当月）
		Ledger ledger = _owner.Ledger;
		DateTime endBase = ((ledger != null) ? ledger.GetEndDate() : DateTime.Today);
		DateTime startBase = ((ledger != null) ? ledger.StartDate : DateTime.Today);
		dtpStart.Value = new DateTime(startBase.Year, startBase.Month, 1);
		dtpEnd.Value = new DateTime(endBase.Year, endBase.Month, 1);
	}

	private static void SetupGrid(C1FlexGridEx grid)
	{
		grid.AllowEditing = false;
		grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grid.Dock = DockStyle.Fill;
		grid.DrawMode = DrawModeEnum.OwnerDraw;
		grid.Font = new Font("微软雅黑", 9.5f);
		grid.Rows.DefaultSize = 20;
		grid.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		grid.ExtendLastCol = true;
		grid.SelectionMode = SelectionModeEnum.Row;
	}

	private static DateTimePicker MakeMonthPicker()
	{
		DateTimePicker picker = new DateTimePicker
		{
			Format = DateTimePickerFormat.Custom,
			CustomFormat = "yyyy-MM",
			ShowUpDown = false,
			Width = 92,
			Font = new Font("微软雅黑", 9f),
			Margin = new Padding(0, 3, 6, 0)
		};
		return picker;
	}

	private static Label MakeLabel(string text)
	{
		Label label = new Label
		{
			Text = text,
			Font = new Font("微软雅黑", 9f),
			AutoSize = true,
			Margin = new Padding(0, 5, 4, 0),
			ForeColor = Color.FromArgb(30, 41, 59)
		};
		return label;
	}

	private static Button MakeButton(string text, int width)
	{
		Button button = new Button();
		button.Text = text;
		button.Font = new Font("微软雅黑", 9f);
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
		button.FlatAppearance.BorderSize = 1;
		button.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 247, 252);
		button.BackColor = Color.White;
		button.ForeColor = Color.FromArgb(30, 41, 59);
		button.Size = new Size(width, 26);
		button.Margin = new Padding(0, 0, 8, 0);
		button.UseVisualStyleBackColor = false;
		button.Cursor = Cursors.Hand;
		return button;
	}

	public void SetTheme()
	{
		grd.Styles.Fixed.Border.Color = Color.DarkGray;
		grd.Styles.Fixed.Font = grd.Font;
	}

	// —— 查询 ——
	private void RunQuery()
	{
		if (_querying)
		{
			return;
		}
		Ledger ledger = _owner.Ledger;
		if (ledger == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		if (!MultiDimensionQuery.HasAuxiliaryData(ledger))
		{
			ShowNoAuxData();
			return;
		}
		DateTime start = new DateTime(dtpStart.Value.Year, dtpStart.Value.Month, 1);
		DateTime endMonth = new DateTime(dtpEnd.Value.Year, dtpEnd.Value.Month, 1);
		if (endMonth < start)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账期范围不正确：结束账期早于起始账期");
			return;
		}
		// 选择器只精确到月，查询区间取整月（末日=次月 1 日的前一天）
		DateTime end = endMonth.AddMonths(1).AddDays(-1);
		string keyword = txtVoucher.Text.Trim();
		if (keyword.Length == 0)
		{
			keyword = null;
		}
		decimal? minAmount = null;
		decimal? maxAmount = null;
		string minText = txtMin.Text.Trim();
		if (minText.Length > 0)
		{
			if (!decimal.TryParse(minText, out decimal parsedMin))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "金额下限格式不正确");
				return;
			}
			minAmount = parsedMin;
		}
		string maxText = txtMax.Text.Trim();
		if (maxText.Length > 0)
		{
			if (!decimal.TryParse(maxText, out decimal parsedMax))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "金额上限格式不正确");
				return;
			}
			maxAmount = parsedMax;
		}
		if (minAmount.HasValue && maxAmount.HasValue && minAmount.Value > maxAmount.Value)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "金额范围不正确：下限大于上限");
			return;
		}
		List<Account> accounts = CollectCheckedAccounts();
		List<DimensionCondition> conditions = BuildConditions();
		_querying = true;
		btnQuery.Enabled = false;
		RunQueryCore(ledger, accounts, start, end, conditions, keyword, minAmount, maxAmount);
	}

	private async void RunQueryCore(Ledger ledger, List<Account> accounts, DateTime start, DateTime end, List<DimensionCondition> conditions, string keyword, decimal? minAmount, decimal? maxAmount)
	{
		try
		{
			List<MultiDimGroup> groups = await Task.Run(() => MultiDimensionQuery.Query(ledger, accounts, start, end, conditions, keyword, minAmount, maxAmount));
			_lastGroups = groups ?? new List<MultiDimGroup>();
			FillCurrentView();
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "多维核算查询失败：" + ex.Message);
		}
		finally
		{
			_querying = false;
			btnQuery.Enabled = true;
		}
	}

	// 从左侧科目树收集勾选的科目（含勾选父科目，引擎会展开为其叶子后代）；无勾选返回 null = 全部叶子
	private List<Account> CollectCheckedAccounts()
	{
		List<Account> result = new List<Account>();
		C1FlexGridEx tree = _owner.AccountTreeEditor.Tree;
		for (int i = tree.Rows.Fixed; i < tree.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = tree.Rows[i];
			if (row.UserData is Account account && tree.GetCellCheck(i, 0) == CheckEnum.Checked)
			{
				result.Add(account);
			}
		}
		if (result.Count == 0)
		{
			return null;
		}
		return result;
	}

	private List<DimensionCondition> BuildConditions()
	{
		List<DimensionCondition> list = new List<DimensionCondition>();
		Ledger ledger = _owner.Ledger;
		if (ledger == null)
		{
			return list;
		}
		foreach (ConditionRow condRow in _condRows)
		{
			string className = condRow.Cbo.SelectedItem as string;
			if (className == null)
			{
				continue;
			}
			AuxiliaryClass cls = ledger.AuxiliaryClasses.FirstOrDefault((AuxiliaryClass c) => c.Name == className);
			if (cls == null)
			{
				continue;
			}
			list.Add(new DimensionCondition
			{
				Class = cls,
				Items = new List<AuxiliaryItem>(condRow.Selected)
			});
		}
		return list;
	}

	private List<string> DimensionClassNames()
	{
		Ledger ledger = _owner.Ledger;
		if (ledger == null)
		{
			return new List<string>();
		}
		return ledger.AuxiliaryClasses.Select((AuxiliaryClass c) => c.Name).ToList();
	}

	// —— 网格填充 ——
	private void FillCurrentView()
	{
		grd.BeginUpdate();
		try
		{
			BuildColumnsCore();
			FillDataCore();
		}
		finally
		{
			grd.EndUpdate();
		}
		lblEmpty.Visible = (_lastGroups != null && _lastGroups.Count == 0);
		if (lblEmpty.Visible)
		{
			lblEmpty.Text = "无符合条件的数据";
		}
		grd.AutoSizeColsFill();
	}

	private void RebuildView()
	{
		if (_lastGroups == null)
		{
			RebuildGridOnly();
		}
		else
		{
			FillCurrentView();
		}
	}

	private void RebuildGridOnly()
	{
		grd.BeginUpdate();
		try
		{
			BuildColumnsCore();
		}
		finally
		{
			grd.EndUpdate();
		}
		lblEmpty.Visible = false;
		grd.AutoSizeColsFill();
	}

	// 每次切换到本视图时由 LedgerViewer.SwitchToView 显式调用（不依赖视图可见事件时序）
	internal void OnViewShown()
	{
		Ledger ledger = _owner.Ledger;
		if (ledger == null)
		{
			return;
		}
		// 账套被重载（修改凭证后 OpenLedger 会换全新 Ledger 对象）：作废旧数据与旧条件，重置账期
		if (!ReferenceEquals(_boundLedger, ledger))
		{
			_boundLedger = ledger;
			_lastGroups = null;
			ClearConditions();
			InitPeriod();
			RebuildGridOnly();
			if (!MultiDimensionQuery.HasAuxiliaryData(ledger))
			{
				ShowNoAuxData();
			}
		}
		if (_focusPending)
		{
			_focusPending = false;
			btnQuery.Focus();
		}
	}

	private void ShowNoAuxData()
	{
		_lastGroups = null;
		RebuildGridOnly();
		lblEmpty.Text = "当前账套无辅助核算数据";
		lblEmpty.Visible = true;
	}

	// 列序固定 + 动态维度列在后；重建列后按惯例补一行固定表头
	private void BuildColumnsCore()
	{
		grd.Rows.Count = 0;
		grd.Cols.Count = 0;
		if (_viewMode == ViewMode.Detail)
		{
			AddTextCol("no", "序号", 46, TextAlignEnum.CenterCenter);
			AddTextCol("rectype", "记录类型", 80, TextAlignEnum.CenterCenter);
			AddTextCol("day", "日期", 86, TextAlignEnum.CenterCenter);
			AddTextCol("voucher", "凭证号", 84);
			AddTextCol("code", "科目编码", 92);
			AddTextCol("acname", "科目名称", 130);
			AddTextCol("digest", "摘要", 180);
			AddMoneyCol("debit", "借方金额", 100);
			AddMoneyCol("credit", "贷方金额", 100);
			AddTextCol("bdc", "期初方向", 56, TextAlignEnum.CenterCenter);
			AddTextCol("bbal", "期初余额", 110, TextAlignEnum.RightCenter);
			AddTextCol("edc", "期末方向", 56, TextAlignEnum.CenterCenter);
			AddTextCol("ebal", "期末余额", 110, TextAlignEnum.RightCenter);
		}
		else
		{
			AddTextCol("code", "科目编码", 92);
			AddTextCol("acname", "科目名称", 130);
			AddTextCol("bdc", "期初方向", 56, TextAlignEnum.CenterCenter);
			AddTextCol("bbal", "期初余额", 110, TextAlignEnum.RightCenter);
			AddMoneyCol("debit", "借方发生额", 110);
			AddMoneyCol("credit", "贷方发生额", 110);
			AddTextCol("edc", "期末方向", 56, TextAlignEnum.CenterCenter);
			AddTextCol("ebal", "期末余额", 110, TextAlignEnum.RightCenter);
		}
		List<string> dims = DimensionClassNames();
		for (int i = 0; i < dims.Count; i++)
		{
			C1.Win.C1FlexGrid.Column col = grd.Cols.Add();
			col.Caption = dims[i];
			col.Name = "d" + i;
			col.DataType = typeof(string);
			col.Width = 110;
		}
		C1.Win.C1FlexGrid.Row header = grd.Rows.Add();
		for (int j = 0; j < grd.Cols.Count; j++)
		{
			grd.SetCellStyle(0, j, _headerStyle);
			header[grd.Cols[j].Name] = grd.Cols[j].Caption;
		}
		grd.Rows.Fixed = 1;
		grd.Cols.Fixed = 0;
	}

	private void AddTextCol(string name, string caption, int width, TextAlignEnum align = TextAlignEnum.LeftCenter)
	{
		C1.Win.C1FlexGrid.Column col = grd.Cols.Add();
		col.Caption = caption;
		col.Name = name;
		col.DataType = typeof(string);
		col.Width = width;
		col.TextAlign = align;
	}

	private void AddMoneyCol(string name, string caption, int width)
	{
		C1.Win.C1FlexGrid.Column col = grd.Cols.Add();
		col.Caption = caption;
		col.Name = name;
		col.DataType = typeof(decimal);
		col.Format = "#,0.00;-#,0.00;#";
		col.TextAlign = TextAlignEnum.RightCenter;
		col.Width = width;
	}

	private void FillDataCore()
	{
		List<string> dims = DimensionClassNames();
		if (_viewMode == ViewMode.Detail)
		{
			FillDetailRows(dims);
		}
		else
		{
			FillSummaryRows(dims);
		}
		AppendTotalRow();
	}

	// 明细视图：展平所有组（组序=引擎返回序，组内行序=引擎排好序）；RunningBalance→期末余额列、GroupOpening→期初余额列
	private void FillDetailRows(List<string> dims)
	{
		int no = 0;
		foreach (MultiDimGroup group in _lastGroups)
		{
			foreach (MultiDimRow r in group.Rows)
			{
				no++;
				C1.Win.C1FlexGrid.Row row = grd.Rows.Add();
				row.UserData = r;
				row["no"] = no.ToString();
				row["rectype"] = ((r.RecordType == MultiDimRecordType.Opening) ? "期初余额" : "发生记录");
				row["day"] = (r.Day.HasValue ? r.Day.Value.ToString("yyyy-MM-dd") : "");
				row["voucher"] = r.VoucherText ?? "";
				row["code"] = r.Account?.Code ?? "";
				row["acname"] = r.Account?.Name ?? "";
				row["digest"] = r.Digest ?? "";
				row["debit"] = r.Debit;
				row["credit"] = r.Credit;
				if (r.OpeningKnown)
				{
					row["bdc"] = DirChar(r.GroupOpening);
					row["bbal"] = AbsMoney(r.GroupOpening);
					row["edc"] = DirChar(r.RunningBalance);
					row["ebal"] = AbsMoney(r.RunningBalance);
				}
				else
				{
					row["bdc"] = UnknownText;
					row["bbal"] = UnknownText;
					row["edc"] = UnknownText;
					row["ebal"] = UnknownText;
				}
				for (int i = 0; i < dims.Count; i++)
				{
					row["d" + i] = r.GetDimension(dims[i]);
				}
			}
		}
	}

	private void FillSummaryRows(List<string> dims)
	{
		foreach (MultiDimGroup group in _lastGroups)
		{
			C1.Win.C1FlexGrid.Row row = grd.Rows.Add();
			row.UserData = group;
			row["code"] = group.Account?.Code ?? "";
			row["acname"] = group.Account?.Name ?? "";
			for (int i = 0; i < dims.Count; i++)
			{
				row["d" + i] = (group.DimensionValues.TryGetValue(dims[i], out string v) ? (v ?? "") : "");
			}
			row["debit"] = group.Debit;
			row["credit"] = group.Credit;
			if (group.OpeningKnown)
			{
				row["bdc"] = DirChar(group.OpeningBalance);
				row["bbal"] = AbsMoney(group.OpeningBalance);
				row["edc"] = DirChar(group.EndingBalance);
				row["ebal"] = AbsMoney(group.EndingBalance);
			}
			else
			{
				row["bdc"] = UnknownText;
				row["bbal"] = UnknownText;
				row["edc"] = UnknownText;
				row["ebal"] = UnknownText;
			}
		}
	}

	private void AppendTotalRow()
	{
		if (_lastGroups == null || _lastGroups.Count == 0)
		{
			return;
		}
		bool anyUnknown = _lastGroups.Any((MultiDimGroup g) => !g.OpeningKnown);
		decimal opening = 0m;
		decimal debit = 0m;
		decimal credit = 0m;
		decimal ending = 0m;
		foreach (MultiDimGroup g in _lastGroups)
		{
			opening += g.OpeningBalance;
			debit += g.Debit;
			credit += g.Credit;
			ending += g.EndingBalance;
		}
		C1.Win.C1FlexGrid.Row row = grd.Rows.Add();
		row.UserData = TAG_TOTAL_ROW;
		if (_viewMode == ViewMode.Detail)
		{
			row["digest"] = "合 计";
		}
		else
		{
			row["acname"] = "合 计";
		}
		row["debit"] = debit;
		row["credit"] = credit;
		if (anyUnknown)
		{
			row["bdc"] = UnknownText;
			row["bbal"] = UnknownText;
			row["edc"] = UnknownText;
			row["ebal"] = UnknownText;
		}
		else
		{
			row["bdc"] = DirChar(opening);
			row["bbal"] = AbsMoney(opening);
			row["edc"] = DirChar(ending);
			row["ebal"] = AbsMoney(ending);
		}
		for (int i = 0; i < grd.Cols.Count; i++)
		{
			grd.SetCellStyle(row.Index, i, _totalStyle);
		}
	}

	// 余额带符号、以科目方向为正（引擎口径）：>0 借、<0 贷、=0 平
	private static string DirChar(decimal balance)
	{
		if (balance > 0m)
		{
			return "借";
		}
		if (balance < 0m)
		{
			return "贷";
		}
		return "平";
	}

	private static string AbsMoney(decimal value)
	{
		return Math.Abs(value).ToString("N2");
	}

	// —— 维度条件行 ——
	private void AddConditionRow()
	{
		Ledger ledger = _owner.Ledger;
		if (ledger == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		if (ledger.AuxiliaryClasses.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前账套无辅助核算类别");
			return;
		}
		if (_condRows.Count >= ledger.AuxiliaryClasses.Count)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "全部维度类别均已添加条件");
			return;
		}
		ConditionRow condRow = new ConditionRow();
		Panel root = new Panel
		{
			Height = 28,
			Width = 330,
			Margin = new Padding(0, 0, 0, 2)
		};
		ComboBox cbo = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			Location = new Point(0, 2),
			Size = new Size(160, 24),
			Font = new Font("微软雅黑", 9f)
		};
		Button btnPick = MakeButton("选择值…", 110);
		btnPick.Location = new Point(164, 1);
		btnPick.Size = new Size(110, 24);
		btnPick.Margin = Padding.Empty;
		Button btnDel = MakeButton("删 除", 52);
		btnDel.Location = new Point(278, 1);
		btnDel.Size = new Size(52, 24);
		btnDel.Margin = Padding.Empty;
		root.Controls.Add(cbo);
		root.Controls.Add(btnPick);
		root.Controls.Add(btnDel);
		condRow.Root = root;
		condRow.Cbo = cbo;
		condRow.BtnPick = btnPick;
		cbo.SelectedIndexChanged += delegate
		{
			OnCondClassChanged(condRow);
		};
		btnPick.Click += delegate
		{
			PickConditionItems(condRow);
		};
		btnDel.Click += delegate
		{
			RemoveConditionRow(condRow);
		};
		_condRows.Add(condRow);
		flowConds.Controls.Add(root);
		flowConds.Visible = true;
		RefreshConditionClassOptions();
	}

	private void RemoveConditionRow(ConditionRow condRow)
	{
		_condRows.Remove(condRow);
		flowConds.Controls.Remove(condRow.Root);
		condRow.Root.Dispose();
		if (_condRows.Count == 0)
		{
			flowConds.Visible = false;
		}
		RefreshConditionClassOptions();
	}

	private void ClearConditions()
	{
		foreach (ConditionRow condRow in _condRows)
		{
			flowConds.Controls.Remove(condRow.Root);
			condRow.Root.Dispose();
		}
		_condRows.Clear();
		flowConds.Visible = false;
	}

	// 每行类别下拉的候选 = 全部类别 - 其他行已选类别（类别不可重复选择）
	private void RefreshConditionClassOptions()
	{
		_suppressCondEvent = true;
		try
		{
			Ledger ledger = _owner.Ledger;
			List<string> allNames = ((ledger != null) ? ledger.AuxiliaryClasses.Select((AuxiliaryClass c) => c.Name).ToList() : new List<string>());
			foreach (ConditionRow condRow in _condRows)
			{
				string current = condRow.LastClassName;
				List<string> available = allNames.Where((string n) => n == current || !_condRows.Exists((ConditionRow r) => r != condRow && r.LastClassName == n)).ToList();
				condRow.Cbo.Items.Clear();
				foreach (string name in available)
				{
					condRow.Cbo.Items.Add(name);
				}
				if (current != null && available.Contains(current))
				{
					condRow.Cbo.SelectedItem = current;
				}
				else if (available.Count > 0)
				{
					current = available[0];
					condRow.Cbo.SelectedItem = current;
				}
				else
				{
					current = null;
					condRow.Cbo.SelectedItem = null;
				}
				condRow.LastClassName = current;
			}
		}
		finally
		{
			_suppressCondEvent = false;
		}
	}

	private void OnCondClassChanged(ConditionRow condRow)
	{
		if (_suppressCondEvent)
		{
			return;
		}
		string selected = condRow.Cbo.SelectedItem as string;
		if (!string.Equals(selected, condRow.LastClassName))
		{
			condRow.LastClassName = selected;
			condRow.Selected.Clear();
			UpdatePickText(condRow);
		}
	}

	private void PickConditionItems(ConditionRow condRow)
	{
		string className = condRow.Cbo.SelectedItem as string;
		if (string.IsNullOrEmpty(className))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择维度类别");
			return;
		}
		Ledger ledger = _owner.Ledger;
		AuxiliaryClass cls = ledger.AuxiliaryClasses.FirstOrDefault((AuxiliaryClass c) => c.Name == className);
		List<AuxiliaryItem> items = ((cls != null) ? ledger.AuxiliaryItems.Where((AuxiliaryItem i) => i.Class == cls).ToList() : new List<AuxiliaryItem>());
		if (items.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该维度类别下没有可选值");
			return;
		}
		using AuxPickForm form = new AuxPickForm(className, items, condRow.Selected);
		Form owner = View.FindForm();
		DialogResult result = ((owner != null) ? form.ShowDialog(owner) : form.ShowDialog());
		if (result == DialogResult.OK)
		{
			condRow.Selected = form.GetSelected();
			UpdatePickText(condRow);
		}
	}

	private void UpdatePickText(ConditionRow condRow)
	{
		if (condRow.Selected.Count == 0)
		{
			condRow.BtnPick.Text = "选择值…";
			return;
		}
		string joined = string.Join(",", condRow.Selected.Select((AuxiliaryItem i) => i.Name));
		condRow.BtnPick.Text = ((joined.Length <= 10) ? joined : (joined.Substring(0, 10) + "…"));
	}

	// —— 交互 ——
	// 明细视图双击发生行打开凭证（复用 LedgerViewer.ModifyVoucher 通道，与 VoucherListEditor/SubsidiaryEditor 一致）
	private async void Grd_DoubleClick(object sender, EventArgs e)
	{
		try
		{
			if (_viewMode != ViewMode.Detail)
			{
				return;
			}
			int row = grd.MouseRow;
			if (row < grd.Rows.Fixed || row >= grd.Rows.Count)
			{
				return;
			}
			if (grd.Rows[row].UserData is MultiDimRow data && data.RecordType == MultiDimRecordType.Transaction && data.Voucher != null)
			{
				await _owner.ModifyVoucher(data.Voucher);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	// —— 导出 ——
	private void ExportExcel()
	{
		if (_lastGroups == null || _lastGroups.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先查询后再导出");
			return;
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "Excel 文件|*.xlsx",
			DefaultExt = ".xlsx",
			FileName = $"多维核算_{DateTime.Now:yyyyMMdd_HHmmss}"
		};
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			List<string> dims = DimensionClassNames();
			LedgerExporter exporter = ((_viewMode == ViewMode.Detail) ? ((LedgerExporter)new DetailExporter(_lastGroups, dims)) : ((LedgerExporter)new SummaryExporter(_lastGroups, dims)));
			exporter.Build();
			exporter.xlBook.Save(saveFileDialog.FileName);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出成功");
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "导出失败：" + ex.Message);
		}
	}
}
