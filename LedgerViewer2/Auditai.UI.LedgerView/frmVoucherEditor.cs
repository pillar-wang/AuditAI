using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
using Auditai.UI.Controls;
using Auditai.UI.LedgerView.Properties;

namespace Auditai.UI.LedgerView;

public class frmVoucherEditor : C1RibbonForm
{
	internal const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	internal const string FORMATSTRING_DATE = "yyyy-MM-dd";

	internal const string CN_CODE = "Code";

	internal const string CN_NAME = "Name";

	internal const string CN_BEGINDC = "BeginDC";

	internal const string CN_BEGINBALANCE = "BeginBalance";

	internal const string CN_DEBIT = "Debit";

	internal const string CN_CREDIT = "Credit";

	internal const string CN_ENDDC = "EndDC";

	internal const string CN_ENDBALANCE = "EndBalance";

	internal const string CN_DATE = "Date";

	internal const string CN_TYPE = "Type";

	internal const string CN_NUMBER = "Number";

	internal const string CN_DIGEST = "Digest";

	internal const string CN_DC = "DC";

	internal const string CN_BALANCE = "Balance";

	internal const string CN_MAKER = "Maker";

	internal const string CN_CHECKER = "Checker";

	internal const string CN_BOOKER = "Booker";

	internal const string CN_INDEX = "Index";

	internal const string CN_AMOUNT = "Amount";

	internal const string CN_RATIO = "Ratio";

	internal const string CN_OPPOSITE = "Opposite";

	private const string TAG_TOTAL = "tag_total";

	private C1ContextMenu ctxFixedCol = new C1ContextMenu();

	private C1CommandLink lnkInsertRow = new C1CommandLink();

	private C1Command cmdInsertRow = new C1Command();

	private C1CommandLink lnkAppendRow = new C1CommandLink();

	private C1Command cmdAppendRow = new C1Command();

	private C1CommandLink lnkDeleteRow = new C1CommandLink();

	private C1Command cmdDeleteRow = new C1Command();

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1CommandLink lnkCopy1 = new C1CommandLink();

	private C1Command cmdCopy1 = new C1Command();

	private C1CommandLink lnkPaste1 = new C1CommandLink();

	private C1Command cmdPaste1 = new C1Command();

	private C1CommandLink lnkInsertRow3 = new C1CommandLink();

	private C1Command cmdInsertRow3 = new C1Command();

	private C1CommandLink lnkAppendRow3 = new C1CommandLink();

	private C1Command cmdAppendRow3 = new C1Command();

	private C1CommandLink lnkDeleteRow3 = new C1CommandLink();

	private C1Command cmdDeleteRow3 = new C1Command();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private C1CommandLink lnkAppendRow2 = new C1CommandLink();

	private C1Command cmdAppendRow2 = new C1Command();

	private AccountAndAuxiliarySelectEditor auxiliarySelector;

	private ShowContext showContext;

	private LedgerViewer _owner;

	private ViewEnum currentView;

	private bool _eventAttached;

	private RibbonImageProcess ImageProcess = new RibbonImageProcess();

	private bool firstLoad = true;

	private IGrouping<string, Voucher> currentVoucherGroup;

	private IGrouping<string, Voucher> tempRemoveGroup;

#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnVoucher;

	private C1SplitterPanel pnlVoucherFoot;

	private C1SplitterPanel pnlVoucherTitle;

	private C1SplitterPanel pnlVoucherHead;

	private C1SplitterPanel pnlVoucherGrid;

	private C1Label lblVoucherTitle;

	private C1TextBox txtNumAttachments;

	private C1TextBox txtVoucherNumber;

	private C1FlexGridEx grdVoucher;

	private C1TextBox txtBooker;

	private C1TextBox txtMaker;

	private C1TextBox txtChecker;

	private C1Label lblChecker;

	private C1Label lblMaker;

	private C1Label lblBooker;

	private C1Label lblVoucherDate;

	private C1Label lblNumAttachments;

	private C1Label lblVoucherNumber;

	private C1Label lblVoucherType;

	private C1DateEdit txtVoucherDate;

	private C1SplitterPanel pnlToolbar;

	private C1CommandDock c1CommandDock1;

	private C1ToolBar c1ToolBar1;

	private C1CommandLink lnkAddVoucher;

	private C1CommandLink lnkModifyVoucher;

	private C1CommandLink lnkDeleteVoucher;

	private C1Command cmdAddVoucher;

	private C1CommandHolder c1CommandHolder1;

	private C1Command cmdModifyVoucher;

	private C1Command cmdDeleteVoucher;

	private C1CommandLink lnkSaveVoucher;

	private C1Command cmdSaveVoucher;

	private C1CommandLink lnkPreviousVoucher;

	private C1CommandLink lnkNextVoucher;

	private C1Command cmdPreviousVoucher;

	private C1Command cmdNextVoucher;

	private C1ComboBox comboVoucherType;

	private C1CommandLink lnkCancelSave;

	private C1Command cmdCancelSave;

	public Ledger Ledger { get; private set; }

	public event EventHandler<IGrouping<string, Voucher>> AfterAddVoucher;

	public event EventHandler<IGrouping<string, Voucher>> AfterModifyVoucher;

	public event EventHandler<IGrouping<string, Voucher>> AfterDeleteVoucher;

	public frmVoucherEditor(LedgerViewer owner, Ledger ledger)
	{
		_owner = owner;
		InitializeComponent();
		base.Shown += FrmVoucherEditor_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		SpecificalControlFactory.CreateDateInputTextBox(txtVoucherDate);
		Ledger = ledger;
		Initialize(ledger);
		AttachEvent();
	}

	private void FrmVoucherEditor_Shown(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.largeModifyVoucher);
	}

	public DialogResult ShowAddDialog(ShowContext context)
	{
		showContext = context;
		DettachEvent();
		try
		{
			SwitchViewTo(ViewEnum.Add);
			base.ActiveControl = grdVoucher;
			Auditai.UI.Controls.Theme.SetCurrentTree(this);
			SetTheme();
		}
		finally
		{
			AttachEvent();
		}
		return ShowDialog();
	}

	public DialogResult ShowModifyDialog(IEnumerable<Voucher> voucherList, ShowContext context)
	{
		showContext = context;
		DettachEvent();
		try
		{
			currentVoucherGroup = GetIGroup(voucherList);
			SwitchViewTo(ViewEnum.Modify);
			base.ActiveControl = grdVoucher;
			Auditai.UI.Controls.Theme.SetCurrentTree(this);
			SetTheme();
		}
		finally
		{
			AttachEvent();
		}
		return ShowDialog();
	}

	private void SwitchViewTo(ViewEnum view)
	{
		currentView = view;
		switch (currentView)
		{
		case ViewEnum.Add:
		{
			PopulateVouchers(null);
			string type = ((currentVoucherGroup == null) ? showContext.Type : currentVoucherGroup.First().Type.Name);
			comboVoucherType.Text = type;
			DateTime date = ((currentVoucherGroup == null) ? showContext.Date : currentVoucherGroup.First().Day);
			txtVoucherNumber.Text = (from v in Ledger.Vouchers
				where v.Day.Year == date.Year && v.Day.Month == date.Month && v.Type.Name == type
				orderby v.Day
				select v).ThenBy((Voucher v) => v.Number, StringNumberComparer.Instance).LastOrDefault()?.Number;
			txtVoucherDate.Value = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
			txtVoucherNumber.Enabled = true;
			comboVoucherType.Enabled = true;
			txtVoucherDate.Enabled = true;
			txtNumAttachments.Enabled = true;
			txtMaker.Enabled = true;
			txtChecker.Enabled = true;
			txtBooker.Enabled = true;
			grdVoucher.AllowEditing = true;
			currentVoucherGroup = null;
			if (grdVoucher.Rows.Count > 1 && grdVoucher.Cols.Count > 1)
			{
				grdVoucher.Row = 1;
				grdVoucher.Col = 1;
			}
			break;
		}
		case ViewEnum.Modify:
			PopulateVouchersAndHead(currentVoucherGroup);
			tempRemoveGroup = currentVoucherGroup;
			txtVoucherNumber.Enabled = false;
			comboVoucherType.Enabled = false;
			txtVoucherDate.Enabled = false;
			txtNumAttachments.Enabled = true;
			txtMaker.Enabled = true;
			txtChecker.Enabled = true;
			txtBooker.Enabled = true;
			grdVoucher.AllowEditing = true;
			if (grdVoucher.Rows.Count > 1 && grdVoucher.Cols.Count > 1)
			{
				grdVoucher.Row = 1;
				grdVoucher.Col = 1;
			}
			break;
		case ViewEnum.ReadOnly:
			txtVoucherNumber.Enabled = false;
			comboVoucherType.Enabled = false;
			txtVoucherDate.Enabled = false;
			txtNumAttachments.Enabled = false;
			txtMaker.Enabled = false;
			txtChecker.Enabled = false;
			txtBooker.Enabled = false;
			grdVoucher.AllowEditing = false;
			break;
		}
		for (int i = grdVoucher.Rows.Fixed; i < grdVoucher.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[i];
			if (!(row.UserData?.ToString() == "tag_total"))
			{
				C1.Win.C1FlexGrid.CellStyle cellStyle = row.Style ?? row.StyleNew;
				cellStyle.BackColor = ((currentView == ViewEnum.ReadOnly) ? UserSet.Config.TableStyle.LockAreaColor : Color.White);
			}
		}
	}

	public IGrouping<string, Voucher> GetIGroup(IEnumerable<Voucher> vouchers)
	{
		IEnumerable<IGrouping<string, Voucher>> source = from v in vouchers
			group v by Common.GetVoucherKey(v);
		if (source.Count() > 1)
		{
			throw new Exception("凭证不是一组");
		}
		return source.FirstOrDefault();
	}

	private void AttachEvent()
	{
		if (!_eventAttached)
		{
			grdVoucher.CellChanged += GrdVoucher_CellChanged;
			_eventAttached = true;
		}
	}

	private void DettachEvent()
	{
		if (_eventAttached)
		{
			grdVoucher.CellChanged -= GrdVoucher_CellChanged;
			_eventAttached = false;
		}
	}

	private void Initialize(Ledger ledger)
	{
		comboVoucherType.TextDetached = true;
		grdVoucher.Paint += GrdVoucher_Paint;
		grdVoucher.MouseClick += GrdVoucher_MouseClick;
		grdVoucher.MouseDown += GrdVoucher_MouseDown;
		grdVoucher.KeyDown += GrdVoucher_KeyDown;
		grdVoucher.DrawMode = DrawModeEnum.OwnerDraw;
		grdVoucher.OwnerDrawCell += GrdVoucher_OwnerDrawCell;
		cmdInsertRow.Text = "插入行";
		lnkInsertRow.Command = cmdInsertRow;
		cmdInsertRow.Click += CmdInsertRow_Click;
		cmdInsertRow.CommandStateQuery += CmdInsertRow_CommandStateQuery;
		cmdInsertRow.Image = ContextResources.ctxInsertRow;
		ctxFixedCol.CommandLinks.Add(lnkInsertRow);
		cmdAppendRow.Text = "新增行";
		lnkAppendRow.Command = cmdAppendRow;
		cmdAppendRow.Click += CmdAppendRow_Click;
		cmdAppendRow.CommandStateQuery += CmdAppendRow_CommandStateQuery;
		cmdAppendRow.Image = ContextResources.ctxAppendRow;
		ctxFixedCol.CommandLinks.Add(lnkAppendRow);
		cmdDeleteRow.Text = "删除行";
		lnkDeleteRow.Command = cmdDeleteRow;
		cmdDeleteRow.Click += CmdDeleteRow_Click;
		cmdDeleteRow.CommandStateQuery += CmdDeleteRow_CommandStateQuery;
		cmdDeleteRow.Image = ContextResources.ctxDeleteRow;
		ctxFixedCol.CommandLinks.Add(lnkDeleteRow);
		cmdAppendRow2.Text = "新增行";
		lnkAppendRow2.Command = cmdAppendRow2;
		cmdAppendRow2.Click += CmdAppendRow_Click;
		cmdAppendRow2.CommandStateQuery += CmdAppendRow2_CommandStateQuery;
		cmdAppendRow2.Image = ContextResources.ctxAppendRow;
		ctxEmpty.CommandLinks.Add(lnkAppendRow2);
		cmdCopy1.Text = "复制";
		lnkCopy1.Command = cmdCopy1;
		cmdCopy1.Click += CmdCopy1_Click;
		cmdCopy1.Image = ContextResources.ctxCopy;
		ctxCell.CommandLinks.Add(lnkCopy1);
		cmdPaste1.Text = "粘贴";
		lnkPaste1.Command = cmdPaste1;
		cmdPaste1.Click += CmdPaste1_Click;
		cmdPaste1.CommandStateQuery += CmdPaste1_CommandStateQuery;
		cmdPaste1.Image = ContextResources.ctxPaste;
		ctxCell.CommandLinks.Add(lnkPaste1);
		cmdInsertRow3.Text = "插入行";
		lnkInsertRow3.Command = cmdInsertRow3;
		cmdInsertRow3.Click += CmdInsertRow_Click;
		cmdInsertRow3.CommandStateQuery += CmdInsertRow3_CommandStateQuery;
		cmdInsertRow3.Image = ContextResources.ctxInsertRow;
		ctxCell.CommandLinks.Add(lnkInsertRow3);
		lnkInsertRow3.Delimiter = true;
		cmdAppendRow3.Text = "新增行";
		lnkAppendRow3.Command = cmdAppendRow3;
		cmdAppendRow3.Click += CmdAppendRow_Click;
		cmdAppendRow3.CommandStateQuery += CmdAppendRow3_CommandStateQuery;
		cmdAppendRow3.Image = ContextResources.ctxAppendRow;
		ctxCell.CommandLinks.Add(lnkAppendRow3);
		cmdDeleteRow3.Text = "删除行";
		lnkDeleteRow3.Command = cmdDeleteRow3;
		cmdDeleteRow3.Click += CmdDeleteRow_Click;
		cmdDeleteRow3.CommandStateQuery += CmdDeleteRow3_CommandStateQuery;
		cmdDeleteRow3.Image = ContextResources.ctxDeleteRow;
		ctxCell.CommandLinks.Add(lnkDeleteRow3);
		cmdDeleteVoucher.CommandStateQuery += CmdDeleteVoucher_CommandStateQuery;
		cmdModifyVoucher.CommandStateQuery += CmdModifyVoucher_CommandStateQuery;
		cmdPreviousVoucher.CommandStateQuery += CmdPreviousVoucher_CommandStateQuery;
		cmdNextVoucher.CommandStateQuery += CmdNextVoucher_CommandStateQuery;
		cmdAddVoucher.CommandStateQuery += CmdAddVoucher_CommandStateQuery;
		cmdSaveVoucher.CommandStateQuery += CmdSaveVoucher_CommandStateQuery;
		cmdCancelSave.CommandStateQuery += CmdCancelSave_CommandStateQuery;
		auxiliarySelector = new AccountAndAuxiliarySelectEditor(_owner, grdVoucher, ledger);
		foreach (VoucherType voucherType in ledger.VoucherTypes)
		{
			comboVoucherType.Items.Add(voucherType.Name);
		}
		foreach (C1CommandLink commandLink in c1ToolBar1.CommandLinks)
		{
			ImageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
	}

	private void CmdDeleteRow3_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = grdVoucher.MouseRow >= grdVoucher.Rows.Fixed && currentView != ViewEnum.ReadOnly;
	}

	private void CmdAppendRow3_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = grdVoucher.MouseRow >= grdVoucher.Rows.Fixed && currentView != ViewEnum.ReadOnly;
	}

	private void CmdInsertRow3_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = currentView != ViewEnum.ReadOnly;
	}

	private void CmdPaste1_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = currentView != ViewEnum.ReadOnly;
	}

	private void CmdPaste1_Click(object sender, ClickEventArgs e)
	{
		grdVoucher.Paste();
	}

	private void CmdCopy1_Click(object sender, ClickEventArgs e)
	{
		grdVoucher.Copy();
	}

	private void CmdAppendRow2_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = currentView != ViewEnum.ReadOnly;
	}

	private void CmdDeleteRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = currentView != ViewEnum.ReadOnly;
	}

	private void CmdAppendRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = currentView != ViewEnum.ReadOnly;
	}

	private void CmdInsertRow_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = currentView != ViewEnum.ReadOnly;
	}

	private void GrdVoucher_Paint(object sender, PaintEventArgs e)
	{
		Auditai.UI.Controls.Theme.DrawFormBorder(grdVoucher, e.Graphics);
	}

	private void GrdVoucher_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTestInfo = grdVoucher.HitTest(e.Location);
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.Cell || type == HitTestTypeEnum.RowHeader)
		{
			if (hitTestInfo.Column == 0)
			{
				grdVoucher.Select(new C1.Win.C1FlexGrid.CellRange
				{
					r1 = hitTestInfo.Row,
					r2 = hitTestInfo.Row,
					c1 = 0,
					c2 = grdVoucher.Cols.Count - 1
				});
			}
			else if (!grdVoucher.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				grdVoucher.Select(hitTestInfo.Row, hitTestInfo.Column);
			}
		}
	}

	private void CmdCancelSave_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Enabled = currentView != ViewEnum.ReadOnly;
	}

	private void CmdSaveVoucher_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Enabled = currentView != ViewEnum.ReadOnly;
	}

	private void CmdAddVoucher_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Enabled = currentView != 0 && currentView != ViewEnum.Modify;
	}

	private void CmdNextVoucher_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Enabled = currentView != 0 && currentView != ViewEnum.Modify;
	}

	private void CmdPreviousVoucher_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Enabled = currentView != 0 && currentView != ViewEnum.Modify;
	}

	private void CmdModifyVoucher_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Enabled = currentVoucherGroup != null && currentView != 0 && currentView != ViewEnum.Modify;
	}

	private void CmdDeleteVoucher_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Enabled = currentVoucherGroup != null && currentView != 0 && currentView != ViewEnum.Modify;
	}

	private void PopulateVouchersAndHead(IEnumerable<Voucher> voucherList)
	{
		Voucher voucher = voucherList?.FirstOrDefault();
		txtMaker.Text = voucher?.Maker ?? string.Empty;
		txtBooker.Text = voucher?.Booker ?? string.Empty;
		txtChecker.Text = voucher?.Checker ?? string.Empty;
		PopulateVouchers(voucherList);
		txtVoucherDate.Value = voucher?.Day ?? default(DateTime);
		comboVoucherType.Text = voucher?.Type.Name ?? string.Empty;
		txtVoucherNumber.Text = voucher?.Number ?? string.Empty;
		txtNumAttachments.Text = voucher?.NumAttachments.ToString() ?? string.Empty;
	}

	private bool ValidateAndRead()
	{
		string type = string.Empty;
		string number = string.Empty;
		DateTime day = default(DateTime);
		int num = 0;
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		if (string.IsNullOrWhiteSpace(comboVoucherType.Text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "凭证字不能为空");
			comboVoucherType.Focus();
			return false;
		}
		type = comboVoucherType.Text.Trim();
		number = txtVoucherNumber.Text.Trim();
		if (DateTime.TryParse(txtVoucherDate.Text.Trim(), out var result))
		{
			day = result;
			if (currentView == ViewEnum.Add && Ledger.Vouchers.Any((Voucher v) => v.Number == number && v.Day.Month == day.Month && v.Day.Year == day.Year && v.Type.Name == type))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该凭证号当月已存在");
				return false;
			}
			num = (int.TryParse(txtNumAttachments.Text.Trim(), out var result2) ? result2 : 0);
			empty = txtMaker.Text.Trim();
			empty2 = txtBooker.Text.Trim();
			empty3 = txtChecker.Text.Trim();
			bool flag = false;
			VoucherType voucherType = Ledger.VoucherTypes.Find((VoucherType t) => t.Name == type);
			if (voucherType == null)
			{
				voucherType = new VoucherType
				{
					Name = type
				};
				flag = true;
			}
			bool flag2 = false;
			Currency currency = Ledger.BaseCurrency;
			if (currency == null)
			{
				currency = Ledger.Vouchers.FirstOrDefault()?.Currency;
				if (currency == null)
				{
					currency = new Currency
					{
						Name = "RMB"
					};
					flag2 = true;
				}
			}
			int num2 = ((Ledger.Vouchers.Count != 0) ? Ledger.Vouchers.Max((Voucher v) => v.Id) : 0);
			List<Voucher> list = new List<Voucher>();
			Dictionary<string, Account> dictionary = Ledger.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
			for (int j = grdVoucher.Rows.Fixed; j < grdVoucher.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[j];
				if (IsEmptyRow(row) || row.UserData?.ToString() == "tag_total")
				{
					continue;
				}
				string digest = row["Digest"]?.ToString()?.Trim();
				if (!(row["Code"] is Tuple<Account, IEnumerable<AuxiliaryItem>> { Item1: not null } tuple))
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码不能为空");
					grdVoucher.Row = j;
					grdVoucher.Col = grdVoucher.Cols["Code"].Index;
					return false;
				}
				if (tuple.Item1.Children.Count > 0)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码必须为末级科目");
					grdVoucher.Row = j;
					grdVoucher.Col = grdVoucher.Cols["Code"].Index;
					return false;
				}
				Voucher voucher = new Voucher();
				num2 = (voucher.Id = num2 + 1);
				voucher.Day = day;
				voucher.Type = voucherType;
				voucher.Number = number;
				voucher.Digest = digest;
				voucher.Account = tuple.Item1;
				voucher.Maker = empty;
				voucher.Booker = empty2;
				voucher.Checker = empty3;
				voucher.Currency = currency;
				voucher.NumAttachments = num;
				Voucher voucher2 = voucher;
				bool isDebit;
				decimal voucherValue = GetVoucherValue(row, out isDebit);
				voucher2.IsDebit = isDebit;
				voucher2.Amount = voucherValue;
				AccountBalance accountBalanceWithCache = _owner.CacheManager.GetAccountBalanceWithCache(Ledger, tuple.Item1);
				if (accountBalanceWithCache.ClassBalances.Count > 0)
				{
					IEnumerable<AuxiliaryItem> item = tuple.Item2;
					if (item == null)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目（" + tuple.Item1.Code + "）" + tuple.Item1.Name + "为带辅助核算科目，必须为凭证挂接辅助核算项");
						grdVoucher.Row = j;
						grdVoucher.Col = grdVoucher.Cols["Code"].Index;
						return false;
					}
					foreach (AuxiliaryClass auxClass in accountBalanceWithCache.ClassBalances.Keys)
					{
						if (!item.Any((AuxiliaryItem i) => i.Class == auxClass))
						{
							Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该凭证未为辅助核算类（" + auxClass.Code + "）" + auxClass.Name + "挂接项目");
							grdVoucher.Row = j;
							grdVoucher.Col = grdVoucher.Cols["Code"].Index;
							return false;
						}
					}
					voucher2.Details.AddRange(item);
				}
				list.Add(voucher2);
			}
			if (list.Count == 0)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "凭证为空");
				return false;
			}
			if (list.Sum((Voucher v) => (!v.IsDebit) ? (-v.Amount) : v.Amount) != 0m)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "会计凭证借贷发生额不平衡");
				return false;
			}
			if (flag && list.Count > 0)
			{
				Ledger.VoucherTypes.Add(voucherType);
			}
			if (flag2)
			{
				Ledger.Currencies.Add(currency);
			}
			currentVoucherGroup = GetIGroup(list);
			return true;
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "日期格式不正确");
		txtVoucherDate.Focus();
		return false;
	}

	private decimal GetVoucherValue(C1.Win.C1FlexGrid.Row row, out bool isDebit)
	{
		object obj = row["Debit"];
		object obj2 = row["Credit"];
		decimal num = default(decimal);
		decimal num2 = default(decimal);
		if (decimal.TryParse(obj?.ToString(), out var result))
		{
			num = result;
		}
		if (decimal.TryParse(obj2?.ToString(), out var result2))
		{
			num2 = result2;
		}
		if (Math.Abs(num) > Math.Abs(num2))
		{
			decimal result3 = num - num2;
			isDebit = true;
			return result3;
		}
		decimal result4 = num2 - num;
		isDebit = false;
		return result4;
	}

	private void DeleteSelection(C1FlexGrid grid)
	{
		grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = grid.Selection;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				for (int j = selection.LeftCol; j <= selection.RightCol; j++)
				{
					grid[i, j] = null;
				}
			}
			PopulateIndex(grdVoucher);
			ReGenerateVoucherTotal();
		}
		finally
		{
			grid.EndUpdate();
		}
	}

	private void PopulateVouchers(IEnumerable<Voucher> vouchers)
	{
		grdVoucher.BeginUpdate();
		try
		{
			DettachEvent();
			if (firstLoad)
			{
				grdVoucher.Cols.Count = 1;
				grdVoucher.Cols.Fixed = 1;
				grdVoucher.Rows.Count = 1;
				grdVoucher.Rows.Fixed = 1;
				grdVoucher.Rows.DefaultSize = 30;
				C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[0];
				column.Name = "Index";
				column.Caption = "序号";
				column.DataType = typeof(string);
				column.TextAlign = TextAlignEnum.CenterCenter;
				column.AllowMerging = true;
				column = grdVoucher.Cols.Add();
				column.Name = "Digest";
				column.Caption = "摘要";
				column.DataType = typeof(string);
				column.AllowMerging = true;
				column.Width = 200;
				column = grdVoucher.Cols.Add();
				column.Name = "Code";
				column.Caption = "科目代码";
				column.Editor = auxiliarySelector;
				column.AllowMerging = true;
				column.TextAlign = TextAlignEnum.LeftCenter;
				column.Width = 120;
				column = grdVoucher.Cols.Add();
				column.Name = "Name";
				column.Caption = "科目名称";
				column.DataType = typeof(string);
				column.AllowMerging = true;
				column.AllowEditing = false;
				column.Width = 200;
				firstLoad = false;
				column = grdVoucher.Cols.Add();
				column.Name = "Debit";
				column.Caption = "借方金额";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
				column.AllowMerging = true;
				column = grdVoucher.Cols.Add();
				column.Name = "Credit";
				column.Caption = "贷方金额";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
				column.AllowMerging = true;
				grdVoucher.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
				grdVoucher.AllowEditing = true;
				firstLoad = false;
			}
			else
			{
				grdVoucher.Rows.Count = 1;
			}
			if (vouchers == null || vouchers.Count() == 0)
			{
				grdVoucher.Rows.Count = 4;
				PopulateIndex(grdVoucher);
				ReGenerateVoucherTotal();
				return;
			}
			int num = 1;
			foreach (Voucher voucher in vouchers)
			{
				C1.Win.C1FlexGrid.Row row = grdVoucher.Rows.Add();
				row.UserData = voucher;
				row["Index"] = num++;
				row["Digest"] = voucher.Digest;
				row["Code"] = Tuple.Create(voucher.Account, (IEnumerable<AuxiliaryItem>)((voucher.Details.Count == 0) ? null : voucher.Details));
				row["Name"] = voucher.GetDisplayAccountNameWithDetail();
				row["Debit"] = (voucher.IsDebit ? voucher.Amount : 0m);
				row["Credit"] = (voucher.IsDebit ? 0m : voucher.Amount);
			}
			PopulateIndex(grdVoucher);
			ReGenerateVoucherTotal();
		}
		finally
		{
			AttachEvent();
			grdVoucher.EndUpdate();
		}
	}

	private bool IsEmptyRow(C1.Win.C1FlexGrid.Row row)
	{
		for (int i = grdVoucher.Cols.Fixed; i < grdVoucher.Cols.Count; i++)
		{
			string value = row[i]?.ToString();
			if (!string.IsNullOrWhiteSpace(value) && (!(grdVoucher.Cols[i].DataType == typeof(decimal)) || !decimal.TryParse(value, out var result) || !(result == 0m)))
			{
				return false;
			}
		}
		return true;
	}

	private void ReGenerateVoucherTotal()
	{
		DettachEvent();
		try
		{
			Point point = grdVoucher.ScrollPosition;
			GrdVoucher_RemoveTotalRow();
			GrdVoucher_AppendTotalRow();
			grdVoucher.ScrollPosition = point;
		}
		finally
		{
			AttachEvent();
		}
		void GrdVoucher_AppendTotalRow()
		{
			decimal num = default(decimal);
			decimal num2 = default(decimal);
			for (int i = grdVoucher.Rows.Fixed; i < grdVoucher.Rows.Count; i++)
			{
				num += (decimal.TryParse(grdVoucher.Rows[i]["Debit"]?.ToString(), out var result) ? result : 0m);
				num2 += (decimal.TryParse(grdVoucher.Rows[i]["Credit"]?.ToString(), out var result2) ? result2 : 0m);
			}
			C1.Win.C1FlexGrid.Row row = grdVoucher.Rows.Add();
			row.AllowEditing = false;
			row.UserData = "tag_total";
			row["Debit"] = num;
			row["Credit"] = num2;
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdVoucher.Styles.Add("total");
			cellStyle.DataType = typeof(string);
			cellStyle.TextAlign = TextAlignEnum.CenterCenter;
			grdVoucher.SetCellStyle(row.Index, "Digest", cellStyle);
			(row.Style ?? row.StyleNew).BackColor = Color.LightYellow;
			row["Digest"] = "合计";
		}
		void GrdVoucher_RemoveTotalRow()
		{
			for (int num3 = grdVoucher.Rows.Count - 1; num3 >= grdVoucher.Rows.Fixed; num3--)
			{
				C1.Win.C1FlexGrid.Row row2 = grdVoucher.Rows[num3];
				if (row2.UserData?.ToString() == "tag_total")
				{
					grdVoucher.Rows.Remove(row2);
					break;
				}
			}
		}
	}

	private void PopulateIndex(C1FlexGrid grid)
	{
		if (grid != null)
		{
			for (int i = grid.Rows.Fixed; i < grid.Rows.Count; i++)
			{
				grid.Cols[0][i] = i.ToString();
			}
			grid.AutoSizeCol(0);
		}
	}

	private void SetTheme()
	{
		grdVoucher.Styles.Fixed.Border.Color = Color.DarkGray;
		grdVoucher.Styles.SelectedColumnHeader.Clear();
		if (Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			ImageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			ImageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		ImageProcess.ProcessImage();
	}

	private void GrdVoucher_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyData)
		{
		case Keys.X | Keys.Control:
			grdVoucher.Cut();
			break;
		case Keys.C | Keys.Control:
			grdVoucher.Copy();
			break;
		case Keys.V | Keys.Control:
			grdVoucher.Paste();
			break;
		case Keys.Delete:
			DeleteSelection(grdVoucher);
			break;
		}
	}

	private void GrdVoucher_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		switch (grdVoucher.HitTest(e.Location).Type)
		{
		case HitTestTypeEnum.Cell:
			ctxCell.ShowContextMenu(grdVoucher, e.Location);
			break;
		case HitTestTypeEnum.None:
			ctxEmpty.ShowContextMenu(grdVoucher, e.Location);
			break;
		case HitTestTypeEnum.RowHeader:
			if (grdVoucher.MouseRow >= grdVoucher.Rows.Fixed)
			{
				ctxFixedCol.ShowContextMenu(grdVoucher, e.Location);
			}
			break;
		}
	}

	private void GrdVoucher_CellChanged(object sender, RowColEventArgs e)
	{
		DettachEvent();
		try
		{
			if (e.Col == grdVoucher.Cols["Code"].Index)
			{
				object obj = grdVoucher.Rows[e.Row]["Code"];
				Tuple<Account, IEnumerable<AuxiliaryItem>> tp = obj as Tuple<Account, IEnumerable<AuxiliaryItem>>;
				if (tp != null && tp.Item1 != null)
				{
					if (tp.Item2 == null)
					{
						grdVoucher.Rows[e.Row]["Name"] = tp.Item1.GetFullName();
						return;
					}
					grdVoucher.Rows[e.Row]["Name"] = string.Join("|", tp.Item2.Select((AuxiliaryItem i) => tp.Item1.GetFullName() + "-" + i.Name));
				}
				else
				{
					grdVoucher.Rows[e.Row]["Name"] = string.Empty;
				}
			}
			else if (e.Col == grdVoucher.Cols["Debit"].Index || e.Col == grdVoucher.Cols["Credit"].Index)
			{
				PopulateIndex(grdVoucher);
				ReGenerateVoucherTotal();
			}
		}
		finally
		{
			AttachEvent();
		}
	}

	private void GrdVoucher_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col != grdVoucher.Cols["Code"].Index)
		{
			return;
		}
		if (e.Row == 0)
		{
			return;
		}
		object obj = grdVoucher.Rows[e.Row]["Code"];
		Tuple<Account, IEnumerable<AuxiliaryItem>> tp = obj as Tuple<Account, IEnumerable<AuxiliaryItem>>;
		if (tp != null && tp.Item1 != null)
		{
			if (tp.Item2 == null)
			{
				e.Text = tp.Item1.Code;
				return;
			}
			e.Text = string.Join("|", tp.Item2.Select((AuxiliaryItem i) => tp.Item1.Code + "-" + i.Code));
		}
		else
		{
			e.Text = string.Empty;
		}
	}

	private void CmdAppendRow_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("追加行", "请输入追加行数：");
		if (!num.HasValue)
		{
			return;
		}
		grdVoucher.BeginUpdate();
		try
		{
			grdVoucher.Rows.Add((int)num.Value);
			PopulateIndex(grdVoucher);
			ReGenerateVoucherTotal();
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void CmdInsertRow_Click(object sender, ClickEventArgs e)
	{
		if (grdVoucher.MouseRow < grdVoucher.Rows.Fixed || grdVoucher.Row >= grdVoucher.Rows.Count)
		{
			return;
		}
		decimal? num = InputForm.Numeric("插入行", "请输入插入行数：");
		if (!num.HasValue)
		{
			return;
		}
		grdVoucher.BeginUpdate();
		try
		{
			for (int i = 0; (decimal)i < num.Value; i++)
			{
				grdVoucher.Rows.Insert(grdVoucher.Row);
			}
			PopulateIndex(grdVoucher);
			ReGenerateVoucherTotal();
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void CmdDeleteRow_Click(object sender, ClickEventArgs e)
	{
		grdVoucher.BeginUpdate();
		try
		{
			int count = grdVoucher.Selection.BottomRow - grdVoucher.Selection.TopRow + 1;
			grdVoucher.Rows.RemoveRange(grdVoucher.Selection.TopRow, count);
			PopulateIndex(grdVoucher);
			ReGenerateVoucherTotal();
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void cmdNextVoucher_Click(object sender, ClickEventArgs e)
	{
		NextVoucherGroup();
	}

	private void cmdPreviousVoucher_Click(object sender, ClickEventArgs e)
	{
		PreviousVoucherGroup();
	}

	private void PreviousVoucherGroup()
	{
		if (Ledger.Vouchers.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前账套没有凭证数据");
			return;
		}
		List<IGrouping<string, Voucher>> list = (from v in (from v in Ledger.Vouchers
				orderby v.Day, v.Type.Name
				select v).ThenBy((Voucher v) => v.Number, StringNumberComparer.Instance)
			group v by Common.GetVoucherKey(v)).ToList();
		int num = list.FindIndex((IGrouping<string, Voucher> g) => g.Key == currentVoucherGroup?.Key);
		if (num - 1 >= 0 && num - 1 < list.Count)
		{
			currentVoucherGroup = list[num - 1];
			PopulateVouchersAndHead(currentVoucherGroup);
		}
		else
		{
			currentVoucherGroup = list.First();
			PopulateVouchersAndHead(currentVoucherGroup);
		}
		SwitchViewTo(ViewEnum.ReadOnly);
	}

	private void NextVoucherGroup()
	{
		if (Ledger.Vouchers.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前账套没有凭证数据");
			return;
		}
		List<IGrouping<string, Voucher>> list = (from v in (from v in Ledger.Vouchers
				orderby v.Day, v.Type.Name
				select v).ThenBy((Voucher v) => v.Number, StringNumberComparer.Instance)
			group v by Common.GetVoucherKey(v)).ToList();
		int num = list.FindIndex((IGrouping<string, Voucher> g) => g.Key == currentVoucherGroup?.Key);
		if (num + 1 >= list.Count)
		{
			currentVoucherGroup = list[list.Count - 1];
			PopulateVouchersAndHead(currentVoucherGroup);
		}
		else
		{
			currentVoucherGroup = list[num + 1];
			PopulateVouchersAndHead(currentVoucherGroup);
		}
		SwitchViewTo(ViewEnum.ReadOnly);
	}

	private void Save()
	{
		if (!ValidateAndRead())
		{
			return;
		}
		switch (currentView)
		{
		case ViewEnum.Add:
			foreach (Voucher item in currentVoucherGroup)
			{
				List<Account> oppositeAccount2 = Ledger.GetOppositeAccount(item);
				item.OppositeAccounts.AddRange(oppositeAccount2);
				item.Dirty = 1;
			}
			Ledger.Vouchers.AddRange(currentVoucherGroup);
			Ledger.Save();
			SwitchViewTo(ViewEnum.ReadOnly);
			this.AfterAddVoucher?.Invoke(this, currentVoucherGroup);
			break;
		case ViewEnum.Modify:
			foreach (Voucher item2 in tempRemoveGroup)
			{
				if (item2.Currency == null)
				{
					item2.Currency = Ledger.BaseCurrency;
				}
				item2.Dirty = -1;
			}
			foreach (Voucher item3 in currentVoucherGroup)
			{
				List<Account> oppositeAccount = Ledger.GetOppositeAccount(item3);
				item3.OppositeAccounts.AddRange(oppositeAccount);
				item3.Dirty = 1;
			}
			Ledger.Vouchers.AddRange(currentVoucherGroup);
			Ledger.Save();
			foreach (Voucher item4 in tempRemoveGroup)
			{
				Ledger.Vouchers.Remove(item4);
			}
			SwitchViewTo(ViewEnum.ReadOnly);
			this.AfterModifyVoucher?.Invoke(this, currentVoucherGroup);
			break;
		}
	}

	private void cmdSaveVoucher_Click(object sender, ClickEventArgs e)
	{
		Save();
	}

	private void cmdAddVoucher_Click(object sender, ClickEventArgs e)
	{
		SwitchViewTo(ViewEnum.Add);
	}

	private void cmdModifyVoucher_Click(object sender, ClickEventArgs e)
	{
		SwitchViewTo(ViewEnum.Modify);
	}

	private void cmdDeleteVoucher_Click(object sender, ClickEventArgs e)
	{
		if (DialogResult.OK != Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "凭证删除后无法恢复，确定要删除该凭证吗？", MessageBoxButtons.OKCancel))
		{
			return;
		}
		foreach (Voucher item in currentVoucherGroup)
		{
			item.Dirty = -1;
		}
		Ledger.Save();
		foreach (Voucher item2 in currentVoucherGroup)
		{
			Ledger.Vouchers.Remove(item2);
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "删除成功");
		this.AfterDeleteVoucher?.Invoke(this, currentVoucherGroup);
		if (Ledger.Vouchers.Count > 0)
		{
			PreviousVoucherGroup();
			return;
		}
		SwitchViewTo(ViewEnum.Add);
		SwitchViewTo(ViewEnum.ReadOnly);
	}

	private void cmdCancelSave_Click(object sender, ClickEventArgs e)
	{
		switch (currentView)
		{
		case ViewEnum.Add:
			PreviousVoucherGroup();
			break;
		case ViewEnum.Modify:
			PopulateVouchersAndHead(currentVoucherGroup);
			SwitchViewTo(ViewEnum.ReadOnly);
			break;
		}
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
		this.ctnVoucher = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlVoucherFoot = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblChecker = new C1.Win.C1Input.C1Label();
		this.lblMaker = new C1.Win.C1Input.C1Label();
		this.lblBooker = new C1.Win.C1Input.C1Label();
		this.txtChecker = new C1.Win.C1Input.C1TextBox();
		this.txtMaker = new C1.Win.C1Input.C1TextBox();
		this.txtBooker = new C1.Win.C1Input.C1TextBox();
		this.pnlToolbar = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1CommandDock1 = new C1.Win.C1Command.C1CommandDock();
		this.c1ToolBar1 = new C1.Win.C1Command.C1ToolBar();
		this.lnkAddVoucher = new C1.Win.C1Command.C1CommandLink();
		this.cmdAddVoucher = new C1.Win.C1Command.C1Command();
		this.lnkModifyVoucher = new C1.Win.C1Command.C1CommandLink();
		this.cmdModifyVoucher = new C1.Win.C1Command.C1Command();
		this.lnkDeleteVoucher = new C1.Win.C1Command.C1CommandLink();
		this.cmdDeleteVoucher = new C1.Win.C1Command.C1Command();
		this.lnkCancelSave = new C1.Win.C1Command.C1CommandLink();
		this.cmdCancelSave = new C1.Win.C1Command.C1Command();
		this.lnkSaveVoucher = new C1.Win.C1Command.C1CommandLink();
		this.cmdSaveVoucher = new C1.Win.C1Command.C1Command();
		this.lnkPreviousVoucher = new C1.Win.C1Command.C1CommandLink();
		this.cmdPreviousVoucher = new C1.Win.C1Command.C1Command();
		this.lnkNextVoucher = new C1.Win.C1Command.C1CommandLink();
		this.cmdNextVoucher = new C1.Win.C1Command.C1Command();
		this.pnlVoucherTitle = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblVoucherTitle = new C1.Win.C1Input.C1Label();
		this.pnlVoucherHead = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.comboVoucherType = new C1.Win.C1Input.C1ComboBox();
		this.txtVoucherDate = new C1.Win.C1Input.C1DateEdit();
		this.lblVoucherDate = new C1.Win.C1Input.C1Label();
		this.lblNumAttachments = new C1.Win.C1Input.C1Label();
		this.lblVoucherNumber = new C1.Win.C1Input.C1Label();
		this.lblVoucherType = new C1.Win.C1Input.C1Label();
		this.txtVoucherNumber = new C1.Win.C1Input.C1TextBox();
		this.txtNumAttachments = new C1.Win.C1Input.C1TextBox();
		this.pnlVoucherGrid = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.grdVoucher = new Auditai.UI.Controls.C1FlexGridEx();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		((System.ComponentModel.ISupportInitialize)this.ctnVoucher).BeginInit();
		this.ctnVoucher.SuspendLayout();
		this.pnlVoucherFoot.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblChecker).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMaker).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblBooker).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtChecker).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtMaker).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtBooker).BeginInit();
		this.pnlToolbar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1CommandDock1).BeginInit();
		this.c1CommandDock1.SuspendLayout();
		this.pnlVoucherTitle.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherTitle).BeginInit();
		this.pnlVoucherHead.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.comboVoucherType).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtVoucherDate).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherDate).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblNumAttachments).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherNumber).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherType).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtVoucherNumber).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtNumAttachments).BeginInit();
		this.pnlVoucherGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdVoucher).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		base.SuspendLayout();
		this.ctnVoucher.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnVoucher.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnVoucher.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnVoucher.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnVoucher.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnVoucher.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnVoucher.HeaderHeight = 27;
		this.ctnVoucher.Location = new System.Drawing.Point(0, 0);
		this.ctnVoucher.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnVoucher.Name = "ctnVoucher";
		this.ctnVoucher.Panels.Add(this.pnlVoucherFoot);
		this.ctnVoucher.Panels.Add(this.pnlToolbar);
		this.ctnVoucher.Panels.Add(this.pnlVoucherTitle);
		this.ctnVoucher.Panels.Add(this.pnlVoucherHead);
		this.ctnVoucher.Panels.Add(this.pnlVoucherGrid);
		this.ctnVoucher.Size = new System.Drawing.Size(792, 503);
		this.ctnVoucher.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnVoucher.SplitterWidth = 5;
		this.ctnVoucher.TabIndex = 0;
		this.ctnVoucher.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlVoucherFoot.Controls.Add(this.lblChecker);
		this.pnlVoucherFoot.Controls.Add(this.lblMaker);
		this.pnlVoucherFoot.Controls.Add(this.lblBooker);
		this.pnlVoucherFoot.Controls.Add(this.txtChecker);
		this.pnlVoucherFoot.Controls.Add(this.txtMaker);
		this.pnlVoucherFoot.Controls.Add(this.txtBooker);
		this.pnlVoucherFoot.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlVoucherFoot.Height = 39;
		this.pnlVoucherFoot.KeepRelativeSize = false;
		this.pnlVoucherFoot.Location = new System.Drawing.Point(0, 464);
		this.pnlVoucherFoot.MinHeight = 39;
		this.pnlVoucherFoot.MinWidth = 39;
		this.pnlVoucherFoot.Name = "pnlVoucherFoot";
		this.pnlVoucherFoot.Resizable = false;
		this.pnlVoucherFoot.Size = new System.Drawing.Size(792, 39);
		this.pnlVoucherFoot.SizeRatio = 12.745;
		this.pnlVoucherFoot.TabIndex = 1;
		this.pnlVoucherFoot.Width = 792;
		this.lblChecker.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.lblChecker.AutoSize = true;
		this.lblChecker.BackColor = System.Drawing.Color.Transparent;
		this.lblChecker.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblChecker.ForeColor = System.Drawing.Color.Black;
		this.lblChecker.Location = new System.Drawing.Point(621, 8);
		this.lblChecker.Name = "lblChecker";
		this.lblChecker.Size = new System.Drawing.Size(56, 17);
		this.lblChecker.TabIndex = 5;
		this.lblChecker.Tag = null;
		this.lblChecker.Text = "审核人：";
		this.lblChecker.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblChecker.TextDetached = true;
		this.lblMaker.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.lblMaker.AutoSize = true;
		this.lblMaker.BackColor = System.Drawing.Color.Transparent;
		this.lblMaker.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMaker.ForeColor = System.Drawing.Color.Black;
		this.lblMaker.Location = new System.Drawing.Point(0, 8);
		this.lblMaker.Name = "lblMaker";
		this.lblMaker.Size = new System.Drawing.Size(56, 17);
		this.lblMaker.TabIndex = 4;
		this.lblMaker.Tag = null;
		this.lblMaker.Text = "制单人：";
		this.lblMaker.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblMaker.TextDetached = true;
		this.lblBooker.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this.lblBooker.AutoSize = true;
		this.lblBooker.BackColor = System.Drawing.Color.Transparent;
		this.lblBooker.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblBooker.ForeColor = System.Drawing.Color.Black;
		this.lblBooker.Location = new System.Drawing.Point(309, 8);
		this.lblBooker.Name = "lblBooker";
		this.lblBooker.Size = new System.Drawing.Size(56, 17);
		this.lblBooker.TabIndex = 3;
		this.lblBooker.Tag = null;
		this.lblBooker.Text = "记账人：";
		this.lblBooker.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblBooker.TextDetached = true;
		this.txtChecker.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.txtChecker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtChecker.Location = new System.Drawing.Point(679, 9);
		this.txtChecker.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtChecker.Name = "txtChecker";
		this.txtChecker.NumericInput = false;
		this.txtChecker.Size = new System.Drawing.Size(101, 21);
		this.txtChecker.TabIndex = 0;
		this.txtChecker.Tag = null;
		this.txtChecker.TextDetached = true;
		this.txtMaker.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.txtMaker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtMaker.Location = new System.Drawing.Point(59, 9);
		this.txtMaker.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtMaker.Name = "txtMaker";
		this.txtMaker.NumericInput = false;
		this.txtMaker.Size = new System.Drawing.Size(101, 21);
		this.txtMaker.TabIndex = 1;
		this.txtMaker.Tag = null;
		this.txtMaker.TextDetached = true;
		this.txtBooker.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this.txtBooker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtBooker.Location = new System.Drawing.Point(367, 9);
		this.txtBooker.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtBooker.Name = "txtBooker";
		this.txtBooker.NumericInput = false;
		this.txtBooker.Size = new System.Drawing.Size(101, 21);
		this.txtBooker.TabIndex = 2;
		this.txtBooker.Tag = null;
		this.txtBooker.TextDetached = true;
		this.pnlToolbar.Controls.Add(this.c1CommandDock1);
		this.pnlToolbar.Height = 63;
		this.pnlToolbar.KeepRelativeSize = false;
		this.pnlToolbar.Location = new System.Drawing.Point(0, 0);
		this.pnlToolbar.Name = "pnlToolbar";
		this.pnlToolbar.Resizable = false;
		this.pnlToolbar.Size = new System.Drawing.Size(792, 63);
		this.pnlToolbar.SizeRatio = 22.105;
		this.pnlToolbar.TabIndex = 5;
		this.c1CommandDock1.Controls.Add(this.c1ToolBar1);
		this.c1CommandDock1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1CommandDock1.Id = 1;
		this.c1CommandDock1.Location = new System.Drawing.Point(0, 0);
		this.c1CommandDock1.Name = "c1CommandDock1";
		this.c1CommandDock1.Size = new System.Drawing.Size(792, 63);
		this.c1ToolBar1.AccessibleName = "Tool Bar";
		this.c1ToolBar1.AutoSize = false;
		this.c1ToolBar1.Border.Width = 0;
		this.c1ToolBar1.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.c1ToolBar1.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.c1ToolBar1.CommandHolder = null;
		this.c1ToolBar1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[7] { this.lnkAddVoucher, this.lnkModifyVoucher, this.lnkDeleteVoucher, this.lnkCancelSave, this.lnkSaveVoucher, this.lnkPreviousVoucher, this.lnkNextVoucher });
		this.c1ToolBar1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1ToolBar1.Location = new System.Drawing.Point(0, 0);
		this.c1ToolBar1.MinButtonSize = 42;
		this.c1ToolBar1.Movable = false;
		this.c1ToolBar1.Name = "c1ToolBar1";
		this.c1ToolBar1.Size = new System.Drawing.Size(823, 61);
		this.c1ToolBar1.Text = "c1ToolBar2";
		this.c1ToolBar1.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.c1ToolBar1.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.lnkAddVoucher.Command = this.cmdAddVoucher;
		this.cmdAddVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.largeCreateVoucher;
		this.cmdAddVoucher.Name = "cmdAddVoucher";
		this.cmdAddVoucher.ShortcutText = "";
		this.cmdAddVoucher.Text = "新增凭证";
		this.cmdAddVoucher.Click += new C1.Win.C1Command.ClickEventHandler(cmdAddVoucher_Click);
		this.lnkModifyVoucher.Command = this.cmdModifyVoucher;
		this.lnkModifyVoucher.SortOrder = 1;
		this.cmdModifyVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.largeModifyVoucher;
		this.cmdModifyVoucher.Name = "cmdModifyVoucher";
		this.cmdModifyVoucher.ShortcutText = "";
		this.cmdModifyVoucher.Text = "修改凭证";
		this.cmdModifyVoucher.Click += new C1.Win.C1Command.ClickEventHandler(cmdModifyVoucher_Click);
		this.lnkDeleteVoucher.Command = this.cmdDeleteVoucher;
		this.lnkDeleteVoucher.SortOrder = 2;
		this.cmdDeleteVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.largeRemoveVoucher;
		this.cmdDeleteVoucher.Name = "cmdDeleteVoucher";
		this.cmdDeleteVoucher.ShortcutText = "";
		this.cmdDeleteVoucher.Text = "删除凭证";
		this.cmdDeleteVoucher.Click += new C1.Win.C1Command.ClickEventHandler(cmdDeleteVoucher_Click);
		this.lnkCancelSave.Command = this.cmdCancelSave;
		this.lnkCancelSave.SortOrder = 3;
		this.cmdCancelSave.Image = Auditai.UI.LedgerView.Properties.Resources.largeCancelSave;
		this.cmdCancelSave.Name = "cmdCancelSave";
		this.cmdCancelSave.ShortcutText = "";
		this.cmdCancelSave.Text = "取消保存";
		this.cmdCancelSave.Click += new C1.Win.C1Command.ClickEventHandler(cmdCancelSave_Click);
		this.lnkSaveVoucher.Command = this.cmdSaveVoucher;
		this.lnkSaveVoucher.SortOrder = 4;
		this.cmdSaveVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.saveData;
		this.cmdSaveVoucher.Name = "cmdSaveVoucher";
		this.cmdSaveVoucher.ShortcutText = "";
		this.cmdSaveVoucher.Text = "保存凭证";
		this.cmdSaveVoucher.Click += new C1.Win.C1Command.ClickEventHandler(cmdSaveVoucher_Click);
		this.lnkPreviousVoucher.Command = this.cmdPreviousVoucher;
		this.lnkPreviousVoucher.Delimiter = true;
		this.lnkPreviousVoucher.SortOrder = 5;
		this.cmdPreviousVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.largePreviousVoucher;
		this.cmdPreviousVoucher.Name = "cmdPreviousVoucher";
		this.cmdPreviousVoucher.ShortcutText = "";
		this.cmdPreviousVoucher.Text = "上一个凭证";
		this.cmdPreviousVoucher.Click += new C1.Win.C1Command.ClickEventHandler(cmdPreviousVoucher_Click);
		this.lnkNextVoucher.Command = this.cmdNextVoucher;
		this.lnkNextVoucher.SortOrder = 6;
		this.cmdNextVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.largeNextVoucher;
		this.cmdNextVoucher.Name = "cmdNextVoucher";
		this.cmdNextVoucher.ShortcutText = "";
		this.cmdNextVoucher.Text = "下一个凭证";
		this.cmdNextVoucher.Click += new C1.Win.C1Command.ClickEventHandler(cmdNextVoucher_Click);
		this.pnlVoucherTitle.Controls.Add(this.lblVoucherTitle);
		this.pnlVoucherTitle.HeaderTextAlign = C1.Win.C1SplitContainer.PanelTextAlign.Center;
		this.pnlVoucherTitle.Height = 26;
		this.pnlVoucherTitle.KeepRelativeSize = false;
		this.pnlVoucherTitle.Location = new System.Drawing.Point(0, 64);
		this.pnlVoucherTitle.MinHeight = 26;
		this.pnlVoucherTitle.MinWidth = 52;
		this.pnlVoucherTitle.Name = "pnlVoucherTitle";
		this.pnlVoucherTitle.Resizable = false;
		this.pnlVoucherTitle.Size = new System.Drawing.Size(792, 26);
		this.pnlVoucherTitle.SizeRatio = 9.774;
		this.pnlVoucherTitle.TabIndex = 2;
		this.pnlVoucherTitle.Width = 792;
		this.lblVoucherTitle.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.lblVoucherTitle.AutoSize = true;
		this.lblVoucherTitle.BackColor = System.Drawing.Color.Transparent;
		this.lblVoucherTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVoucherTitle.Font = new System.Drawing.Font("Microsoft YaHei", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblVoucherTitle.ForeColor = System.Drawing.Color.Black;
		this.lblVoucherTitle.Location = new System.Drawing.Point(363, 2);
		this.lblVoucherTitle.Name = "lblVoucherTitle";
		this.lblVoucherTitle.Size = new System.Drawing.Size(74, 21);
		this.lblVoucherTitle.TabIndex = 0;
		this.lblVoucherTitle.Tag = null;
		this.lblVoucherTitle.Text = "记账凭证";
		this.lblVoucherTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblVoucherTitle.TextDetached = true;
		this.pnlVoucherHead.Controls.Add(this.comboVoucherType);
		this.pnlVoucherHead.Controls.Add(this.txtVoucherDate);
		this.pnlVoucherHead.Controls.Add(this.lblVoucherDate);
		this.pnlVoucherHead.Controls.Add(this.lblNumAttachments);
		this.pnlVoucherHead.Controls.Add(this.lblVoucherNumber);
		this.pnlVoucherHead.Controls.Add(this.lblVoucherType);
		this.pnlVoucherHead.Controls.Add(this.txtVoucherNumber);
		this.pnlVoucherHead.Controls.Add(this.txtNumAttachments);
		this.pnlVoucherHead.HeaderTextAlign = C1.Win.C1SplitContainer.PanelTextAlign.Center;
		this.pnlVoucherHead.Height = 30;
		this.pnlVoucherHead.KeepRelativeSize = false;
		this.pnlVoucherHead.Location = new System.Drawing.Point(0, 91);
		this.pnlVoucherHead.MinHeight = 30;
		this.pnlVoucherHead.MinWidth = 52;
		this.pnlVoucherHead.Name = "pnlVoucherHead";
		this.pnlVoucherHead.Resizable = false;
		this.pnlVoucherHead.Size = new System.Drawing.Size(792, 30);
		this.pnlVoucherHead.SizeRatio = 12.146;
		this.pnlVoucherHead.TabIndex = 3;
		this.pnlVoucherHead.Width = 792;
		this.comboVoucherType.AllowSpinLoop = false;
		this.comboVoucherType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboVoucherType.GapHeight = 0;
		this.comboVoucherType.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboVoucherType.ItemsDisplayMember = "";
		this.comboVoucherType.ItemsValueMember = "";
		this.comboVoucherType.Location = new System.Drawing.Point(37, 5);
		this.comboVoucherType.Name = "comboVoucherType";
		this.comboVoucherType.Size = new System.Drawing.Size(61, 21);
		this.comboVoucherType.TabIndex = 10;
		this.comboVoucherType.Tag = null;
		this.txtVoucherDate.AllowSpinLoop = false;
		this.txtVoucherDate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this.txtVoucherDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtVoucherDate.Calendar.Font = new System.Drawing.Font("Tahoma", 8f);
		this.txtVoucherDate.Calendar.VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.txtVoucherDate.Calendar.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.txtVoucherDate.ImagePadding = new System.Windows.Forms.Padding(0);
		this.txtVoucherDate.Location = new System.Drawing.Point(367, 5);
		this.txtVoucherDate.Name = "txtVoucherDate";
		this.txtVoucherDate.Size = new System.Drawing.Size(80, 21);
		this.txtVoucherDate.TabIndex = 9;
		this.txtVoucherDate.Tag = null;
		this.lblVoucherDate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this.lblVoucherDate.AutoSize = true;
		this.lblVoucherDate.BackColor = System.Drawing.Color.Transparent;
		this.lblVoucherDate.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVoucherDate.ForeColor = System.Drawing.Color.Black;
		this.lblVoucherDate.Location = new System.Drawing.Point(298, 7);
		this.lblVoucherDate.Name = "lblVoucherDate";
		this.lblVoucherDate.Size = new System.Drawing.Size(68, 17);
		this.lblVoucherDate.TabIndex = 8;
		this.lblVoucherDate.Tag = null;
		this.lblVoucherDate.Text = "制单日期：";
		this.lblVoucherDate.TextDetached = true;
		this.lblNumAttachments.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.lblNumAttachments.AutoSize = true;
		this.lblNumAttachments.BackColor = System.Drawing.Color.Transparent;
		this.lblNumAttachments.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblNumAttachments.ForeColor = System.Drawing.Color.Black;
		this.lblNumAttachments.Location = new System.Drawing.Point(622, 5);
		this.lblNumAttachments.Name = "lblNumAttachments";
		this.lblNumAttachments.Size = new System.Drawing.Size(68, 17);
		this.lblNumAttachments.TabIndex = 7;
		this.lblNumAttachments.Tag = null;
		this.lblNumAttachments.Text = "附件张数：";
		this.lblNumAttachments.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblNumAttachments.TextDetached = true;
		this.lblVoucherNumber.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.lblVoucherNumber.AutoSize = true;
		this.lblVoucherNumber.BackColor = System.Drawing.Color.Transparent;
		this.lblVoucherNumber.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVoucherNumber.ForeColor = System.Drawing.Color.Black;
		this.lblVoucherNumber.Location = new System.Drawing.Point(104, 5);
		this.lblVoucherNumber.Name = "lblVoucherNumber";
		this.lblVoucherNumber.Size = new System.Drawing.Size(32, 17);
		this.lblVoucherNumber.TabIndex = 6;
		this.lblVoucherNumber.Tag = null;
		this.lblVoucherNumber.Text = "号：";
		this.lblVoucherNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblVoucherNumber.TextDetached = true;
		this.lblVoucherType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.lblVoucherType.AutoSize = true;
		this.lblVoucherType.BackColor = System.Drawing.Color.Transparent;
		this.lblVoucherType.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblVoucherType.ForeColor = System.Drawing.Color.Black;
		this.lblVoucherType.Location = new System.Drawing.Point(3, 5);
		this.lblVoucherType.Name = "lblVoucherType";
		this.lblVoucherType.Size = new System.Drawing.Size(32, 17);
		this.lblVoucherType.TabIndex = 5;
		this.lblVoucherType.Tag = null;
		this.lblVoucherType.Text = "字：";
		this.lblVoucherType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.lblVoucherType.TextDetached = true;
		this.txtVoucherNumber.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.txtVoucherNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtVoucherNumber.Location = new System.Drawing.Point(140, 5);
		this.txtVoucherNumber.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
		this.txtVoucherNumber.Name = "txtVoucherNumber";
		this.txtVoucherNumber.Size = new System.Drawing.Size(46, 21);
		this.txtVoucherNumber.TabIndex = 1;
		this.txtVoucherNumber.Tag = null;
		this.txtVoucherNumber.TextDetached = true;
		this.txtNumAttachments.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.txtNumAttachments.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtNumAttachments.Location = new System.Drawing.Point(693, 5);
		this.txtNumAttachments.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtNumAttachments.Name = "txtNumAttachments";
		this.txtNumAttachments.Size = new System.Drawing.Size(87, 21);
		this.txtNumAttachments.TabIndex = 3;
		this.txtNumAttachments.Tag = null;
		this.txtNumAttachments.TextDetached = true;
		this.pnlVoucherGrid.Controls.Add(this.grdVoucher);
		this.pnlVoucherGrid.Height = 341;
		this.pnlVoucherGrid.Location = new System.Drawing.Point(0, 122);
		this.pnlVoucherGrid.MinHeight = 52;
		this.pnlVoucherGrid.MinWidth = 52;
		this.pnlVoucherGrid.Name = "pnlVoucherGrid";
		this.pnlVoucherGrid.Size = new System.Drawing.Size(792, 341);
		this.pnlVoucherGrid.SizeRatio = 95.0;
		this.pnlVoucherGrid.TabIndex = 4;
		this.pnlVoucherGrid.Width = 792;
		this.grdVoucher.AllowEditing = false;
		this.grdVoucher.AllowResizing = C1.Win.C1FlexGrid.AllowResizingEnum.Both;
		this.grdVoucher.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.grdVoucher.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdVoucher.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdVoucher.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdVoucher.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.grdVoucher.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.grdVoucher.Location = new System.Drawing.Point(0, 0);
		this.grdVoucher.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdVoucher.Name = "grdVoucher";
		this.grdVoucher.Rows.DefaultSize = 20;
		this.grdVoucher.Size = new System.Drawing.Size(792, 341);
		this.grdVoucher.TabIndex = 0;
		this.c1CommandHolder1.Commands.Add(this.cmdAddVoucher);
		this.c1CommandHolder1.Commands.Add(this.cmdModifyVoucher);
		this.c1CommandHolder1.Commands.Add(this.cmdDeleteVoucher);
		this.c1CommandHolder1.Commands.Add(this.cmdSaveVoucher);
		this.c1CommandHolder1.Commands.Add(this.cmdPreviousVoucher);
		this.c1CommandHolder1.Commands.Add(this.cmdNextVoucher);
		this.c1CommandHolder1.Commands.Add(this.cmdCancelSave);
		this.c1CommandHolder1.Owner = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(792, 503);
		base.Controls.Add(this.ctnVoucher);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmVoucherEditor";
		this.Text = "编辑凭证";
		((System.ComponentModel.ISupportInitialize)this.ctnVoucher).EndInit();
		this.ctnVoucher.ResumeLayout(false);
		this.pnlVoucherFoot.ResumeLayout(false);
		this.pnlVoucherFoot.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.lblChecker).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMaker).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblBooker).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtChecker).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtMaker).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtBooker).EndInit();
		this.pnlToolbar.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1CommandDock1).EndInit();
		this.c1CommandDock1.ResumeLayout(false);
		this.pnlVoucherTitle.ResumeLayout(false);
		this.pnlVoucherTitle.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherTitle).EndInit();
		this.pnlVoucherHead.ResumeLayout(false);
		this.pnlVoucherHead.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.comboVoucherType).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtVoucherDate).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherDate).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblNumAttachments).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherNumber).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblVoucherType).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtVoucherNumber).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtNumAttachments).EndInit();
		this.pnlVoucherGrid.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdVoucher).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		base.ResumeLayout(false);
	}
}
