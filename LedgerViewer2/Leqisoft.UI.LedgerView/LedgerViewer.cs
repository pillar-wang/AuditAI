extern alias CrawlerModelAlias;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.C1Preview;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.Model.Crawlers;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

public class LedgerViewer
{
	protected class AuxiliaryClassReCodeSetting
	{
		public int oldId;

		public string oldCode;

		public int newId;

		public string newCode;
	}

	private TooltipManager tooltipManager = new TooltipManager();

	private C1SplitContainer ctnView;

	private C1SplitterPanel pnlTree;

	private C1SplitterPanel pnlData;

	internal readonly MultiLedgerViewer _owner;

	public LedgerCacheManager CacheManager = new LedgerCacheManager();

	private Lazy<AccountTreeEditor> lazyAccountTreeEditor;

	internal Lazy<BalanceEditor> lazyBalanceEditor;

	internal Lazy<SummaryEditor> lazySummaryEditor;

	internal Lazy<SubsidiaryEditor> lazySubsidiaryEditor;

	internal Lazy<VoucherListEditor> lazyVoucherListEditor;

	internal Lazy<LedgerAgingEditor> lazyLedgerAgingEditor;

	internal Lazy<TrendencyEditor> lazyTrendencyEditor;

	internal Lazy<StructureEditor> lazyStructureEditor;

	internal Lazy<ValidateEditor> lazyValidateEditor;

	internal Lazy<VoucherMarkedEditor> lazyVoucherMarkedEditor;

	internal Lazy<PreviewEditor> lazyPreviewEditor;

	public static Func<int, bool> LicenseCheckHandleOnCopyLedgerData;

	public static Func<int, bool> LicenseCheckHandleIsOpenedLedgerCountInLimit;

	public static bool IsAuditPlatform = true;

	private Account _currentAccount;

	private bool isShowFillToTable = true;

	private frmImport importForm;

	private frmVoucherEditor frmVoucherEditor;

	internal AccountTreeEditor AccountTreeEditor => lazyAccountTreeEditor.Value;

	internal BalanceEditor BalanceEditor => lazyBalanceEditor.Value;

	internal SummaryEditor SummaryEditor => lazySummaryEditor.Value;

	internal SubsidiaryEditor SubsidiaryEditor => lazySubsidiaryEditor.Value;

	internal VoucherListEditor VoucherListEditor => lazyVoucherListEditor.Value;

	internal LedgerAgingEditor LedgerAgingEditor => lazyLedgerAgingEditor.Value;

	internal TrendencyEditor TrendencyEditor => lazyTrendencyEditor.Value;

	internal StructureEditor StructureEditor => lazyStructureEditor.Value;

	internal ValidateEditor ValidateEditor => lazyValidateEditor.Value;

	internal VoucherMarkedEditor VoucherMarkedEditor => lazyVoucherMarkedEditor.Value;

	internal PreviewEditor PreviewEditor => lazyPreviewEditor.Value;

	internal StyleRecord StyleRecord { get; set; } = new StyleRecord();


	public Ledger Ledger { get; set; }

	public string CurrentFilePath { get; private set; }

	public DateTime StartDate => BalanceEditor.StartDate;

	public DateTime EndDate => BalanceEditor.EndDate;

	public Account CurrentAccount
	{
		get
		{
			return _currentAccount;
		}
		set
		{
			_currentAccount = value;
		}
	}

	public object CurrentAuxiliary { get; set; }

	public ActiveView CurrentView { get; private set; }

	public bool IsHideSidebar { get; set; }

	private C1ContextMenu grdTreeContexMenu { get; set; }

	#pragma warning disable CS0067
	public event EventHandler<List<double>> SelectionNumberChanged;
	#pragma warning restore CS0067

	public event EventHandler<bool> PrepareGridChanged;

	public event EventHandler<Tuple<C1CommandLink, string>> AfterShareLedgerClick;

	public event EventHandler<Account> AfterAddAccount;

	public event EventHandler<Account> AfterModifyAccount;

	public event EventHandler<Account> AfterDeleteAccount;

	public event EventHandler<IGrouping<string, Voucher>> AfterAddVoucher;

	public event EventHandler<IGrouping<string, Voucher>> AfterModifyVoucher;

	public event EventHandler<IGrouping<string, Voucher>> AfterDeleteVoucher;

	public event EventHandler AfterModifyBalance;

	protected void OnPreparePrintGridChanged()
	{
		ActiveView currentView = CurrentView;
		if (currentView == ActiveView.Balance || (uint)(currentView - 3) <= 1u)
		{
			this.PrepareGridChanged?.Invoke(this, e: true);
		}
		else
		{
			this.PrepareGridChanged?.Invoke(this, e: false);
		}
		if (!lazyPreviewEditor.IsValueCreated)
		{
			return;
		}
		try
		{
			PrintPreview(PreviewEditor.IsPreview);
		}
		catch (ArgumentOutOfRangeException ex)
		{
			ex.Log();
			PreviewEditor.Preview.Document = EmptyView();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ParamName);
		}
		catch (Exception ex2)
		{
			ex2.Log();
			PreviewEditor.Preview.Document = EmptyView();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	internal void OnAfterShareLedgerClick(C1CommandLink lnk, string file)
	{
		this.AfterShareLedgerClick?.Invoke(this, Tuple.Create(lnk, file));
	}

	public void OnAfterAddAccount(Account e)
	{
		CacheManager.CacheValid = false;
		this.AfterAddAccount?.Invoke(this, e);
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void OnAfterModifyAccount(Account e)
	{
		CacheManager.CacheValid = false;
		this.AfterModifyAccount?.Invoke(this, e);
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void OnAfterDeleteAccount(Account e)
	{
		CacheManager.CacheValid = false;
		this.AfterDeleteAccount?.Invoke(this, e);
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void OnAfterAddVoucher(IGrouping<string, Voucher> e)
	{
		CacheManager.CacheValid = false;
		this.AfterAddVoucher?.Invoke(this, e);
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void OnAfterModifyVoucher(IGrouping<string, Voucher> e)
	{
		CacheManager.CacheValid = false;
		this.AfterModifyVoucher?.Invoke(this, e);
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void OnAfterDeleteVoucher(IGrouping<string, Voucher> e)
	{
		CacheManager.CacheValid = false;
		this.AfterDeleteVoucher?.Invoke(this, e);
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void OnAfterModifyBalance(EventArgs e)
	{
		CacheManager.CacheValid = false;
		this.AfterModifyBalance?.Invoke(this, e);
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void TriggerLedgerDataChangeEvent()
	{
		_owner.OnAfterLedgerDataChanged(new LedgerEventArgs
		{
			Viewer = this
		});
	}

	public void OnHideSidebarClick()
	{
	}

	public LedgerViewer(MultiLedgerViewer owner)
	{
		ctnView = new C1SplitContainer();
		pnlTree = new C1SplitterPanel();
		pnlData = new C1SplitterPanel();
		pnlTree.Name = "LedgerViewer+pnlTree";
		pnlTree.Dock = PanelDockStyle.Left;
		pnlTree.Location = new Point(0, 0);
		pnlTree.Size = new Size(234, 660);
		pnlTree.SizeRatio = 20.0;
		pnlTree.Width = 234;
		pnlData.Height = 660;
		pnlData.Location = new Point(236, 0);
		pnlData.Size = new Size(935, 660);
		pnlData.SizeRatio = 100.0;
		pnlData.Width = 935;
		ctnView.Name = "LedgerViewer+ctnView";
		ctnView.AutoSizeElement = AutoSizeElement.Both;
		ctnView.BackColor = Color.FromArgb(240, 240, 240);
		ctnView.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnView.ForeColor = Color.FromArgb(0, 0, 0);
		ctnView.SplitterWidth = 2;
		ctnView.Panels.Add(pnlTree);
		ctnView.Panels.Add(pnlData);
		_owner = owner;
		lazyAccountTreeEditor = new Lazy<AccountTreeEditor>(delegate
		{
			AccountTreeEditor accountTreeEditor = new AccountTreeEditor(this);
			pnlTree.Controls.Add(accountTreeEditor.Tree);
			accountTreeEditor.Tree.Dock = DockStyle.Fill;
			accountTreeEditor.Tree.AfterCollapse += AccountTreeEditor_Tree_AfterCollapse;
			accountTreeEditor.Tree.DoubleClick += AccountTreeEditor_Tree_DoubleClick;
			accountTreeEditor.Tree.MouseDown += AccountTreeEditor_Tree_MouseDown;
			accountTreeEditor.Tree.CellChecked += AccountTreeEditor_CellChecked;
			accountTreeEditor.ShowAuxiliaryNode += AccountTreeEditor_ShowAuxiliaryNode;
			accountTreeEditor.HideAuxiliaryNode += AccountTreeEditor_HideAuxiliaryNode;
			accountTreeEditor.ShowEmptyAccount += AccountTreeEditor_ShowEmptyAccount;
			accountTreeEditor.HideEmptyAccount += AccountTreeEditor_HideEmptyAccount;
			accountTreeEditor.CancelAll += AccountTreeEditor_CancelAllChecked;
			AttachGenerateEvent2(accountTreeEditor.Tree);
			Leqisoft.UI.Controls.Theme.SetCurrentTree(pnlTree);
			accountTreeEditor.SetTheme();
			return accountTreeEditor;
		});
		lazyBalanceEditor = new Lazy<BalanceEditor>(delegate
		{
			BalanceEditor balanceEditor = new BalanceEditor(this);
			pnlData.Controls.Add(balanceEditor.View);
			balanceEditor.grdBalance.Click += BalanceEditor_GrdBalance_Click;
			balanceEditor.grdBalance.DoubleClick += BalanceEditor_GrdBalance_DoubleClick;
			balanceEditor.grdBalance.AfterCollapse += BalanceEditor_GrdBalance_AfterCollapse;
			balanceEditor.AccountPeriodChanged += BalanceEditor_AccountPeriod_Changed;
			AttachGenerateEvent1(balanceEditor.grdBalance);
			balanceEditor.AttachTooltip(tooltipManager);
			if (IsHideSidebar)
			{
				balanceEditor.HideSideToolbar();
			}
			balanceEditor.SetTheme();
			balanceEditor.ShowFillToTable(isShowFillToTable);
			return balanceEditor;
		});
		lazySubsidiaryEditor = new Lazy<SubsidiaryEditor>(delegate
		{
			SubsidiaryEditor subsidiaryEditor = new SubsidiaryEditor(this);
			pnlData.Controls.Add(subsidiaryEditor.View);
			subsidiaryEditor.SetTitle(StartDate.ToString("yyyy-MM-dd"), EndDate.ToString("yyyy-MM-dd"));
			subsidiaryEditor.ShowVoucher(visible: false);
			AttachGenerateEvent1(subsidiaryEditor.grdSubsidiary);
			AttachGenerateEvent1(subsidiaryEditor.grdVoucher);
			subsidiaryEditor.AttachTooltip(tooltipManager);
			if (IsHideSidebar)
			{
				subsidiaryEditor.HideSideToolbar();
			}
			Leqisoft.UI.Controls.Theme.SetCurrentTree(subsidiaryEditor.View);
			subsidiaryEditor.SetTheme();
			subsidiaryEditor.ShowFillToTable(isShowFillToTable);
			return subsidiaryEditor;
		});
		lazySummaryEditor = new Lazy<SummaryEditor>(delegate
		{
			SummaryEditor summaryEditor = new SummaryEditor(this);
			pnlData.Controls.Add(summaryEditor.View);
			summaryEditor.grdMonthSummary.MouseDoubleClick += GrdMonthSummary_MouseDoubleClick;
			AttachGenerateEvent1(summaryEditor.grdMonthSummary);
			if (IsHideSidebar)
			{
				summaryEditor.HideSideToolbar();
			}
			Leqisoft.UI.Controls.Theme.SetCurrentTree(summaryEditor.View);
			summaryEditor.SetTheme();
			return summaryEditor;
		});
		lazyLedgerAgingEditor = new Lazy<LedgerAgingEditor>(delegate
		{
			LedgerAgingEditor ledgerAgingEditor = new LedgerAgingEditor(this);
			pnlData.Controls.Add(ledgerAgingEditor.View);
			ledgerAgingEditor.dteAnalyzeDate.Value = EndDate;
			ledgerAgingEditor.DisplayLevel = AccountTreeEditor.Tree.Tree.MaximumLevel;
			ledgerAgingEditor.BaseDate = EndDate;
			if (Ledger != null)
			{
				ledgerAgingEditor.SheetCacheManager.Cache(Ledger, EndDate);
			}
			AttachGenerateEvent1(ledgerAgingEditor.grid);
			if (IsHideSidebar)
			{
				ledgerAgingEditor.HideSideToolbar();
			}
			Leqisoft.UI.Controls.Theme.SetCurrentTree(ledgerAgingEditor.View);
			ledgerAgingEditor.SetTheme();
			return ledgerAgingEditor;
		});
		lazyVoucherListEditor = new Lazy<VoucherListEditor>(delegate
		{
			VoucherListEditor voucherListEditor = new VoucherListEditor(this);
			pnlData.Controls.Add(voucherListEditor.View);
			pnlTree.Controls.Add(voucherListEditor.Tree);
			voucherListEditor.Tree.Dock = DockStyle.Fill;
			voucherListEditor.PopulateTree();
			voucherListEditor._grid.DoubleClick += VoucherListEditor_GrdVoucherList_DoubleClick;
			AttachGenerateEvent2(voucherListEditor._grid);
			if (IsHideSidebar)
			{
				voucherListEditor.HideSideToolbar();
			}
			Leqisoft.UI.Controls.Theme.SetCurrentObject(voucherListEditor.Tree);
			Leqisoft.UI.Controls.Theme.SetCurrentTree(voucherListEditor.View);
			voucherListEditor.SetTheme();
			return voucherListEditor;
		});
		lazyTrendencyEditor = new Lazy<TrendencyEditor>(delegate
		{
			TrendencyEditor trendencyEditor = new TrendencyEditor(this);
			pnlData.Controls.Add(trendencyEditor.View);
			trendencyEditor.grdTrendTable.DoubleClick += TrendencyEditor_GrdTrendTable_DoubleClick;
			AttachGenerateEvent1(trendencyEditor.grdTrendTable);
			if (IsHideSidebar)
			{
				trendencyEditor.HideSideToolbar();
			}
			Leqisoft.UI.Controls.Theme.SetCurrentTree(trendencyEditor.View);
			trendencyEditor.SetTheme();
			return trendencyEditor;
		});
		lazyStructureEditor = new Lazy<StructureEditor>(delegate
		{
			StructureEditor structureEditor = new StructureEditor(this);
			pnlData.Controls.Add(structureEditor.View);
			structureEditor.grdStructureTable.DoubleClick += StructureEditor_GrdStructureTable_DoubleClick;
			AttachGenerateEvent1(structureEditor.grdStructureTable);
			if (IsHideSidebar)
			{
				structureEditor.HideSideToolbar();
			}
			Leqisoft.UI.Controls.Theme.SetCurrentTree(structureEditor.View);
			structureEditor.SetTheme();
			return structureEditor;
		});
		lazyValidateEditor = new Lazy<ValidateEditor>(delegate
		{
			ValidateEditor validateEditor = new ValidateEditor(this);
			pnlData.Controls.Add(validateEditor.View);
			pnlTree.Controls.Add(validateEditor.Tree);
			validateEditor.Tree.Dock = DockStyle.Fill;
			Leqisoft.UI.Controls.Theme.SetCurrentTree(validateEditor.View);
			validateEditor.SetTheme();
			return validateEditor;
		});
		lazyVoucherMarkedEditor = new Lazy<VoucherMarkedEditor>(delegate
		{
			VoucherMarkedEditor voucherMarkedEditor = new VoucherMarkedEditor(this);
			pnlData.Controls.Add(voucherMarkedEditor.View);
			pnlTree.Controls.Add(voucherMarkedEditor.Tree);
			AttachGenerateEvent1(voucherMarkedEditor.grdVouchers);
			voucherMarkedEditor.Tree.Dock = DockStyle.Fill;
			if (IsHideSidebar)
			{
				voucherMarkedEditor.HideSideToolbar();
			}
			Leqisoft.UI.Controls.Theme.SetCurrentTree(voucherMarkedEditor.View);
			voucherMarkedEditor.SetTheme();
			return voucherMarkedEditor;
		});
		lazyPreviewEditor = new Lazy<PreviewEditor>(delegate
		{
			PreviewEditor previewEditor = new PreviewEditor(this);
			pnlData.Controls.Add(previewEditor.Preview);
			return previewEditor;
		});
		StyleRecord.Load(ViewStyle.Load(ConfigManager.USERCONFIGFILE));
		LoadSetting(UserSet.Config.BooksStyle);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(ctnView);
		SetTheme();
		AfterModifyBalance += LedgerViewer_AfterModifyBalance;
		AfterDeleteAccount += LedgerViewer_AfterDeleteAccount;
		AfterModifyAccount += LedgerViewer_AfterModifyAccount;
		AfterAddAccount += LedgerViewer_AfterAddAccount;
		AfterAddVoucher += LedgerViewer_AfterAddVoucher;
		AfterModifyVoucher += LedgerViewer_AfterModifyVoucher;
		AfterDeleteVoucher += LedgerViewer_AfterDeleteVoucher;
	}

	public void ShowFillToTable(bool isShow)
	{
		isShowFillToTable = isShow;
		if (lazyBalanceEditor.IsValueCreated)
		{
			lazyBalanceEditor.Value.ShowFillToTable(isShow);
		}
		if (lazySubsidiaryEditor.IsValueCreated)
		{
			lazySubsidiaryEditor.Value.ShowFillToTable(isShow);
		}
	}

	public bool IsDisplayEmptyAccount()
	{
		if (!lazyAccountTreeEditor.IsValueCreated)
		{
			return false;
		}
		return lazyAccountTreeEditor.Value.DisplayEmptyAccount;
	}

	public bool IsEmptyAccount(Account target)
	{
		return CacheManager.IsEmptyAccountWithCache(target);
	}

	public bool IsEmptyAuxiliaryItem(Account account, AuxiliaryItem auxItem)
	{
		return CacheManager.IsEmptyAuxiliaryItemWithCache(account, auxItem);
	}

	private void LedgerViewer_AfterDeleteVoucher(object sender, IGrouping<string, Voucher> e)
	{
		AccountTreeEditor.PopulateAccountTree(Ledger, AccountTreeEditor.DisplayEmptyAccount);
		foreach (Voucher item in e)
		{
			BalanceEditor.UpdateAccountData(item.Account);
		}
		switch (CurrentView)
		{
		case ActiveView.Subsidiary:
			VoucherListEditor.PopulateTree();
			SubsidiaryEditor.UpdateValueFromUserData(SubsidiaryEditor.currentUserData);
			break;
		case ActiveView.VoucherList:
			foreach (C1.Win.C1FlexGrid.Row item2 in (IEnumerable)VoucherListEditor.Tree.Rows)
			{
				if (item2.UserData is Tuple<NodeFlag, List<Voucher>> tuple)
				{
					Voucher voucher = tuple.Item2.FirstOrDefault();
					string voucherKey = Common.GetVoucherKey(voucher);
					if (voucherKey == e.Key)
					{
						item2.Node.RemoveNode();
						break;
					}
				}
			}
			VoucherListEditor.FillVoucherList();
			VoucherListEditor.PopulateVoucherList();
			break;
		}
	}

	private void LedgerViewer_AfterModifyVoucher(object sender, IGrouping<string, Voucher> e)
	{
		AccountTreeEditor.PopulateAccountTree(Ledger, AccountTreeEditor.DisplayEmptyAccount);
		foreach (Voucher item in e)
		{
			BalanceEditor.UpdateAccountData(item.Account);
		}
		switch (CurrentView)
		{
		case ActiveView.Subsidiary:
			VoucherListEditor.PopulateTree();
			SubsidiaryEditor.UpdateValueFromUserData(SubsidiaryEditor.currentUserData);
			break;
		case ActiveView.VoucherList:
			foreach (C1.Win.C1FlexGrid.Row item2 in (IEnumerable)VoucherListEditor.Tree.Rows)
			{
				if (item2.UserData is Tuple<NodeFlag, List<Voucher>> tuple)
				{
					Voucher voucher = tuple.Item2.FirstOrDefault();
					string voucherKey = Common.GetVoucherKey(voucher);
					if (voucherKey == e.Key)
					{
						item2.UserData = Tuple.Create(tuple.Item1, e.ToList());
						break;
					}
				}
			}
			VoucherListEditor.FillVoucherList();
			VoucherListEditor.PopulateVoucherList();
			break;
		}
	}

	private void LedgerViewer_AfterAddVoucher(object sender, IGrouping<string, Voucher> e)
	{
		AccountTreeEditor.PopulateAccountTree(Ledger, AccountTreeEditor.DisplayEmptyAccount);
		foreach (Voucher item in e)
		{
			BalanceEditor.UpdateAccountData(item.Account);
		}
		switch (CurrentView)
		{
		case ActiveView.Subsidiary:
			VoucherListEditor.PopulateTree();
			SubsidiaryEditor.UpdateValueFromUserData(SubsidiaryEditor.currentUserData);
			break;
		case ActiveView.VoucherList:
		{
			int year = e.First().Day.Year;
			int month = e.First().Day.Month;
			string type = e.First().Type.Name;
			Node node = VoucherListEditor.Tree.Nodes.FirstOrDefault((Node n) => n.Key is Tuple<NodeFlag, int> { Item1: NodeFlag.Year } tuple3 && tuple3.Item2 == year);
			if (node == null)
			{
				node = VoucherListEditor.Tree.Rows.AddNode(0);
				node.Data = $"{year}年";
				node.Key = Tuple.Create(NodeFlag.Year, year);
				node.Checked = CheckEnum.Unchecked;
			}
			Node node2 = node.Nodes.FirstOrDefault((Node n) => n.Key is Tuple<NodeFlag, int> { Item1: NodeFlag.Month } tuple2 && tuple2.Item2 == month);
			if (node2 == null)
			{
				node2 = node.AddNode(NodeTypeEnum.LastChild, $"{month}月");
				node2.Key = Tuple.Create(NodeFlag.Month, month);
				node2.Checked = CheckEnum.Unchecked;
			}
			Node node3 = node2.Nodes.FirstOrDefault((Node n) => n.Key is Tuple<NodeFlag, string> tuple && tuple.Item2 == type);
			if (node3 == null)
			{
				node3 = node2.AddNode(NodeTypeEnum.LastChild, type ?? "");
				node3.Key = Tuple.Create(NodeFlag.Type, type);
				node3.Checked = CheckEnum.Unchecked;
			}
			Node node4 = node3.AddNode(NodeTypeEnum.LastChild, e.First().Number);
			node4.Key = Tuple.Create(NodeFlag.Vouchers, e.ToList());
			node4.Checked = CheckEnum.Unchecked;
			break;
		}
		}
	}

	private void LedgerViewer_AfterAddAccount(object sender, Account e)
	{
		SwitchToView(ActiveView.Balance);
		AccountTreeEditor.DisplayEmptyAccount = true;
		AccountTreeEditor.PopulateAccountTree(Ledger, AccountTreeEditor.DisplayEmptyAccount);
		BalanceEditor.PopulateBalanceSheet(Ledger, AccountTreeEditor.DisplayEmptyAccount);
		for (int i = 0; i < AccountTreeEditor.Tree.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = AccountTreeEditor.Tree.Rows[i];
			if (row.UserData is Account account && account == e)
			{
				row.Node.Expanded = true;
				Node node = row.Node;
				while (node.Parent != null)
				{
					node.Parent.Expanded = true;
					node = node.Parent;
				}
				AccountTreeEditor.Tree.Row = i;
				AccountTreeEditor.Tree.ShowCell(i, 0);
				break;
			}
		}
	}

	private void LedgerViewer_AfterModifyBalance(object sender, EventArgs e)
	{
		SwitchToView(ActiveView.Balance);
		BalanceEditor.PopulateBalanceSheet(Ledger, AccountTreeEditor.DisplayEmptyAccount);
		AccountTreeEditor.PopulateAccountTree(Ledger, AccountTreeEditor.DisplayEmptyAccount);
	}

	private void LedgerViewer_AfterModifyAccount(object sender, Account e)
	{
		SwitchToView(ActiveView.Balance);
		BalanceEditor.PopulateBalanceSheet(Ledger, AccountTreeEditor.DisplayEmptyAccount);
	}

	private void LedgerViewer_AfterDeleteAccount(object sender, Account e)
	{
		SwitchToView(ActiveView.Balance);
		BalanceEditor.PopulateBalanceSheet(Ledger, AccountTreeEditor.DisplayEmptyAccount);
	}

	public C1SplitContainer GetMainView()
	{
		return ctnView;
	}

	public void ShowSideToolbar()
	{
		IsHideSidebar = false;
		if (lazyBalanceEditor.IsValueCreated)
		{
			BalanceEditor.ShowSideToolbar();
		}
		if (lazySubsidiaryEditor.IsValueCreated)
		{
			SubsidiaryEditor.ShowSideToolbar();
		}
		if (lazyVoucherListEditor.IsValueCreated)
		{
			VoucherListEditor.ShowSideToolbar();
		}
		if (lazySummaryEditor.IsValueCreated)
		{
			SummaryEditor.ShowSideToolbar();
		}
		if (lazyTrendencyEditor.IsValueCreated)
		{
			TrendencyEditor.ShowSideToolbar();
		}
		if (lazyStructureEditor.IsValueCreated)
		{
			StructureEditor.ShowSideToolbar();
		}
		if (lazyLedgerAgingEditor.IsValueCreated)
		{
			LedgerAgingEditor.ShowSideToolbar();
		}
		if (lazyVoucherMarkedEditor.IsValueCreated)
		{
			VoucherMarkedEditor.ShowSideToolbar();
		}
	}

	public void HideSideToolbar()
	{
		IsHideSidebar = true;
		if (lazyBalanceEditor.IsValueCreated)
		{
			BalanceEditor.HideSideToolbar();
		}
		if (lazySubsidiaryEditor.IsValueCreated)
		{
			SubsidiaryEditor.HideSideToolbar();
		}
		if (lazyVoucherListEditor.IsValueCreated)
		{
			VoucherListEditor.HideSideToolbar();
		}
		if (lazySummaryEditor.IsValueCreated)
		{
			SummaryEditor.HideSideToolbar();
		}
		if (lazyTrendencyEditor.IsValueCreated)
		{
			TrendencyEditor.HideSideToolbar();
		}
		if (lazyStructureEditor.IsValueCreated)
		{
			StructureEditor.HideSideToolbar();
		}
		if (lazyLedgerAgingEditor.IsValueCreated)
		{
			LedgerAgingEditor.HideSideToolbar();
		}
		if (lazyVoucherMarkedEditor.IsValueCreated)
		{
			VoucherMarkedEditor.HideSideToolbar();
		}
	}

	public async Task OpenLedgerDialog()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = "账套文件（*.db,*.001）|*.db;*.001"
		};
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			try
			{
				await OpenLedgerFileImpl(openFileDialog.FileName);
			}
			catch (FileNotFoundException exception)
			{
				exception.Log();
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件不存在！");
			}
			catch (SQLiteException exception2)
			{
				exception2.Log();
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件未能识别为账套格式文件，无法打开。");
			}
			catch (Exception ex)
			{
				ex.Log();
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
		}
	}

	public async Task<bool> OpenLedger(string fullPath)
	{
		try
		{
			setFileAttribute(fullPath);
			await OpenLedgerFileImpl(fullPath);
			return true;
		}
		catch (FileNotFoundException exception)
		{
			exception.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件不存在！");
			return false;
		}
		catch (SQLiteException exception2)
		{
			exception2.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件未能识别为账套格式文件，无法打开。");
			return false;
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return false;
		}
	}

	public void OpenExcelLedger()
	{
		if (importForm == null)
		{
			importForm = new frmImport();
			importForm.FormClosed += delegate
			{
				importForm = null;
				GetMainView().FindForm()?.BringToFront();
			};
			importForm.AfterGenerateSuccess += async delegate(object s1, string e1)
			{
				await OpenLedger(e1);
			};
		}
		try
		{
			importForm.Show();
			importForm.BringToFront();
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private void OpenExcelTemplate()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string text = directoryName + "/序时账模板.xlsx";
			using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write))
			{
				byte[] xsz = Resources.xsz;
				fileStream.Write(xsz, 0, xsz.Length);
			}
			Process.Start(text);
		}
		catch (IOException ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "生成序时账模板错误，请先关闭打开的序时账模板文件。错误信息：" + ex.Message);
		}
		catch (Exception ex2)
		{
			ex2.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "打开序时账模板错误！" + ex2.Message);
		}
	}

	public string MergeLedger()
	{
		try
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "账套文件（*.db）|*.db"
			};
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string text = MergeLedgerImpl(openFileDialog.FileName);
				if (text != null)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账套合并完成，合并后生成的账套文件为：" + text);
					return text;
				}
			}
			return null;
		}
		catch (FileNotFoundException exception)
		{
			exception.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "文件不存在！");
			return null;
		}
		catch (CannotMergeException ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return null;
		}
		catch (LedgerMergeException ex2)
		{
			ex2.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "合并错误！" + ex2.Message);
			return null;
		}
		catch (Exception ex3)
		{
			ex3.Log(ex3.Message);
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
			return null;
		}
	}

	public void CloseLedger(bool fromView = true)
	{
		try
		{
			SwitchToView(ActiveView.Empty);
			CurrentFilePath = null;
			CurrentAccount = null;
			CurrentAuxiliary = null;
			Ledger = null;
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ShowBalance()
	{
		try
		{
			if (Ledger == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
				return;
			}
			BringToFront();
			SwitchToView(ActiveView.Balance);
			Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void BringToFront()
	{
		ctnView.BringToFront();
		_owner.ToLedgerViewer();
	}

	public void ShowMonthSummary()
	{
		try
		{
			if (Ledger == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
				return;
			}
			BringToFront();
			if (CurrentAccount != null)
			{
				SummaryEditor.AnalysisProject = ((!CurrentAccount.IsDebit) ? AnalysisProject.Credits : AnalysisProject.Debits);
			}
			SwitchToView(ActiveView.MonthSummary);
			Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			if (!SummaryEditor.HasShow)
			{
				SummaryEditor.AnalysisProject = ((!CurrentAccount.IsDebit) ? AnalysisProject.Credits : AnalysisProject.Debits);
			}
			SummaryEditor.UpdateTitle(CurrentAccount);
			SummaryEditor.Create(SummaryEditor.GetSelectNodeChildren());
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public static string GetBalanceDCChar(Account account, decimal accountBalance)
	{
		return (string)Common.GetDCChar(account.IsDebit, accountBalance);
	}

	public void SelectTotal()
	{
		if (Ledger == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		try
		{
			BringToFront();
			SwitchToView(ActiveView.Subsidiary);
			Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			SubsidiaryEditor.SubStatus = SubOrTotal.Total;
			SubsidiaryEditor.PopulateSubsidiarySheet(CurrentAccount, StartDate, EndDate);
			SubsidiaryEditor.UpdateTitle(CurrentAccount);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void SelectSub()
	{
		if (Ledger == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		try
		{
			BringToFront();
			SwitchToView(ActiveView.Subsidiary);
			Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			SubsidiaryEditor.SubStatus = SubOrTotal.Subsidiary;
			SubsidiaryEditor.PopulateSubsidiarySheet(CurrentAccount, StartDate, EndDate);
			SubsidiaryEditor.UpdateTitle(CurrentAccount);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ShowVoucherList()
	{
		if (Ledger == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		try
		{
			BringToFront();
			SwitchToView(ActiveView.VoucherList);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ShowMarkedVouchers()
	{
		if (Ledger == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		try
		{
			BringToFront();
			SwitchToView(ActiveView.MarkVoucers);
			VoucherMarkedEditor.PopulateVouchers();
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ShowLedgerAgeAnalazy()
	{
		if (Ledger == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		try
		{
			BringToFront();
			if (!LedgerAgingEditor.CanAnalyze())
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账期小于1年，无法进行账龄分析！");
				return;
			}
			SwitchToView(ActiveView.AgeAnalazy);
			Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			object obj = AccountTreeEditor.CurrentOpendedRow?.UserData;
			if (!(obj is Tuple<Account, AuxiliaryClass> tuple))
			{
				if (!(obj is Tuple<Account, AuxiliaryItem> tuple2))
				{
					if (obj is Account account)
					{
						LedgerAgingEditor.Account = account;
						LedgerAgingEditor.Populate(null);
					}
					else
					{
						LedgerAgingEditor.Account = CurrentAccount;
						LedgerAgingEditor.Populate(null);
					}
				}
				else
				{
					LedgerAgingEditor.Account = tuple2.Item1;
					LedgerAgingEditor.Populate(tuple2.Item2);
				}
			}
			else
			{
				LedgerAgingEditor.Account = tuple.Item1;
				LedgerAgingEditor.Populate(tuple.Item2);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ShowTrend()
	{
		if (Ledger == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		try
		{
			BringToFront();
			SwitchToView(ActiveView.TrendChart);
			AccountTreeEditor.SwitchTrendency(CurrentAccount);
			TrendencyEditor.PopulateTrendency(AccountTreeEditor.SelectedTrendencyNodes);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ShowPie()
	{
		if (Ledger == null)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		try
		{
			BringToFront();
			SwitchToView(ActiveView.PieChart);
			AccountTreeEditor.SwitchStructure(CurrentAccount);
			StructureEditor.PopulateStructure(AccountTreeEditor.SelectedStructureNodes);
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void FillToTable()
	{
		try
		{
			if (Ledger == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
				return;
			}
			BringToFront();
			switch (CurrentView)
			{
			case ActiveView.Balance:
				BalanceEditor.FillToTable();
				break;
			case ActiveView.Subsidiary:
				SubsidiaryEditor.FillToTable();
				break;
			case ActiveView.MarkVoucers:
				VoucherMarkedEditor.FillToTable();
				break;
			case ActiveView.VoucherList:
				VoucherListEditor.FillToTable();
				break;
			default:
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前页面不支持采账填充");
				break;
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void PrintPreview(bool toPreview)
	{
		try
		{
			if (toPreview)
			{
				try
				{
					PreviewEditor.PrintPreview();
					SwitchToPreview(preview: true);
					return;
				}
				catch (PreviewNotSupport previewNotSupport)
				{
					Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, previewNotSupport.Message);
					throw;
				}
			}
			SwitchToPreview(preview: false);
		}
		catch (Exception ex) when (!(ex is PreviewNotSupport))
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void Print()
	{
		try
		{
			PreviewEditor.Print();
		}
		catch (Exception ex) when (!(ex is PreviewNotSupport))
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void SaveToExcel()
	{
		try
		{
			PreviewEditor.SaveToExcel();
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ShowValidate()
	{
		try
		{
			if (Ledger == null)
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			}
			else
			{
				SwitchToView(ActiveView.Validate);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void ChangePreviewDirection(bool landscape)
	{
		try
		{
			if (PreviewEditor._printer == null)
			{
				throw new ArgumentNullException("请先打印预览", "请先打印预览");
			}
			PreviewEditor._printer.Landscape = landscape;
			PreviewEditor._printer.Build();
			PreviewEditor.Preview.Document = PreviewEditor._printer.PrintDocument;
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void LoadSetting(BooksStyle booksStyle)
	{
		try
		{
			Color fontColor = booksStyle.FontStyle.FontColor;
			StyleRecord.ViewStyle.FamilyName = booksStyle.FontStyle.FontFamily;
			StyleRecord.ViewStyle.FontSize = booksStyle.FontStyle.FontSize;
			StyleRecord.ViewStyle.Height = booksStyle.BooksRowHeight;
			int height = Convert.ToInt32(StyleRecord.ViewStyle.Height);
			if (lazyBalanceEditor.IsValueCreated)
			{
				SetGridStyle(BalanceEditor.grdBalance);
				if (CurrentView == ActiveView.Balance)
				{
					SetGridHeight(BalanceEditor.grdBalance);
				}
			}
			if (lazySubsidiaryEditor.IsValueCreated)
			{
				SetGridStyle(SubsidiaryEditor.grdSubsidiary);
				SetGridStyle(SubsidiaryEditor.grdVoucher);
				SubsidiaryEditor.SubDisplay = booksStyle.SubDisplay;
				SubsidiaryEditor.TotalDisplay = booksStyle.TotalDisplay;
				SubsidiaryEditor.SubStatus = booksStyle.BalanceTo;
				if (CurrentView == ActiveView.Subsidiary)
				{
					SetGridHeight(SubsidiaryEditor.grdSubsidiary);
					SetGridHeight(SubsidiaryEditor.grdVoucher);
				}
			}
			if (lazyVoucherListEditor.IsValueCreated)
			{
				SetGridStyle(VoucherListEditor._grid);
				if (CurrentView == ActiveView.VoucherList)
				{
					SetGridHeight(VoucherListEditor._grid);
				}
			}
			if (lazyTrendencyEditor.IsValueCreated)
			{
				SetGridStyle(TrendencyEditor.grdTrendTable);
				if (CurrentView == ActiveView.TrendChart)
				{
					SetGridHeight(TrendencyEditor.grdTrendTable);
				}
			}
			if (lazyStructureEditor.IsValueCreated)
			{
				SetGridStyle(StructureEditor.grdStructureTable);
				if (CurrentView == ActiveView.PieChart)
				{
					SetGridHeight(StructureEditor.grdStructureTable);
				}
			}
			void SetGridHeight(C1FlexGrid flex)
			{
				if (height != 0)
				{
					foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)flex.Rows)
					{
						item.Height = height;
					}
				}
			}
			void SetGridStyle(C1FlexGrid flex)
			{
				flex.BeginUpdate();
				flex.ForeColor = fontColor;
				flex.Font = new Font(StyleRecord.ViewStyle.FamilyName, StyleRecord.ViewStyle.FontSize);
				if (height != 0)
				{
					flex.Rows.DefaultSize = height;
				}
				flex.EndUpdate();
			}
		}
		catch (Exception exception)
		{
			exception.Log();
			exception.Log("LoadSetting");
		}
	}

	private void AccountTreeEditor_CellChecked(object sender, RowColEventArgs e)
	{
		if (AccountTreeEditor.PendingAllEvent)
		{
			return;
		}
		switch (CurrentView)
		{
		case ActiveView.TrendChart:
		{
			Node currentNode = AccountTreeEditor.Tree.Rows[e.Row].Node;
			List<Node> selectedTrendencyNodes = AccountTreeEditor.SelectedTrendencyNodes;
			if (currentNode.Checked == CheckEnum.Checked)
			{
				if (!selectedTrendencyNodes.Contains(currentNode))
				{
					selectedTrendencyNodes.Add(currentNode);
				}
			}
			else
			{
				selectedTrendencyNodes.RemoveAll((Node n) => n.Key == currentNode.Key);
			}
			TrendencyEditor.PopulateTrendency(selectedTrendencyNodes);
			break;
		}
		case ActiveView.PieChart:
		{
			CheckEnum cellCheck = AccountTreeEditor.Tree.GetCellCheck(e.Row, e.Col);
			for (int i = AccountTreeEditor.Tree.Rows.Fixed; i < AccountTreeEditor.Tree.Rows.Count; i++)
			{
				AccountTreeEditor.Tree.Rows[i].Node.Checked = CheckEnum.Unchecked;
			}
			Node currentNode2 = AccountTreeEditor.Tree.Rows[e.Row].Node;
			List<Node> selectedStructureNodes = AccountTreeEditor.SelectedStructureNodes;
			while (currentNode2.Parent != null)
			{
				currentNode2 = currentNode2.Parent;
			}
			currentNode2.Checked = cellCheck;
			CheckChildrens(currentNode2, currentNode2.Checked);
			selectedStructureNodes.Clear();
			if (currentNode2.Checked == CheckEnum.Checked)
			{
				if (!selectedStructureNodes.Exists((Node n) => n.Key.Equals(currentNode2.Key)))
				{
					selectedStructureNodes.Add(currentNode2);
				}
			}
			else
			{
				selectedStructureNodes.RemoveAll((Node t) => t.Key.Equals(currentNode2.Key));
			}
			StructureEditor.PopulateStructure(selectedStructureNodes);
			break;
		}
		}
		static void CheckChildrens(Node node1, CheckEnum checkEnum)
		{
			Node[] nodes = node1.Nodes;
			foreach (Node node2 in nodes)
			{
				node2.Checked = checkEnum;
				CheckChildrens(node2, checkEnum);
			}
		}
	}

	private void VoucherListEditor_GrdVoucherList_DoubleClick(object sender, EventArgs e)
	{
		if (VoucherListEditor.PendingAllEvent)
		{
			return;
		}
		C1FlexGridEx grid = VoucherListEditor._grid;
		if (grid.MouseRow < grid.Rows.Fixed || grid.MouseCol < grid.Cols.Fixed)
		{
			return;
		}
		try
		{
			if (!grid.Cols.Contains("Code"))
			{
				return;
			}
			Voucher voucher = VoucherListEditor.Vouchers[grid.BodyRow];
			if (SwitchToView(ActiveView.Subsidiary))
			{
				Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
				if (voucher.Details.Count > 0)
				{
					AuxiliaryItem auxiliaryItem = voucher.Details[0];
					SubsidiaryEditor.PopulateSubsidiarySheet(voucher.Account, StartDate, EndDate, auxiliaryItem);
					SubsidiaryEditor.UpdateTitle(voucher.Account, auxiliaryItem);
					CurrentAccount = voucher.Account;
					CurrentAuxiliary = auxiliaryItem;
					AccountTreeEditor.UpdateNodeStatus(Tuple.Create(voucher.Account, auxiliaryItem));
				}
				else
				{
					SubsidiaryEditor.PopulateSubsidiarySheet(voucher.Account, StartDate, EndDate);
					SubsidiaryEditor.UpdateTitle(voucher.Account);
					CurrentAccount = voucher.Account;
					CurrentAuxiliary = null;
					AccountTreeEditor.UpdateNodeStatus(voucher.Account);
				}
			}
		}
		catch
		{
		}
	}

	private void StructureEditor_GrdStructureTable_DoubleClick(object sender, EventArgs e)
	{
		if (StructureEditor.PendingAllEvent)
		{
			return;
		}
		C1FlexGridEx grdStructureTable = StructureEditor.grdStructureTable;
		if (grdStructureTable.MouseRow < grdStructureTable.Rows.Fixed || grdStructureTable.MouseCol < grdStructureTable.Cols.Fixed)
		{
			return;
		}
		try
		{
			if (grdStructureTable.Rows[grdStructureTable.Row].UserData is Account account)
			{
				SubsidiaryEditor.PopulateSubsidiarySheet(account, StartDate, EndDate);
				SubsidiaryEditor.UpdateTitle(account);
				if (SwitchToView(ActiveView.Subsidiary))
				{
					Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
				}
				CurrentAccount = account;
				CurrentAuxiliary = null;
			}
		}
		catch
		{
		}
	}

	private void TrendencyEditor_GrdTrendTable_DoubleClick(object sender, EventArgs e)
	{
		if (TrendencyEditor.PendingAllEvent)
		{
			return;
		}
		C1FlexGridEx grdTrendTable = TrendencyEditor.grdTrendTable;
		if (grdTrendTable.MouseRow >= grdTrendTable.Rows.Fixed && grdTrendTable.MouseCol >= grdTrendTable.Cols.Fixed && grdTrendTable.Cols[grdTrendTable.Col].UserData is Account account)
		{
			SubsidiaryEditor.PopulateSubsidiarySheet(account, StartDate, EndDate);
			SubsidiaryEditor.UpdateTitle(account);
			if (SwitchToView(ActiveView.Subsidiary))
			{
				Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			}
			CurrentAccount = account;
			CurrentAuxiliary = null;
			AccountTreeEditor.UpdateNodeStatus(account);
		}
	}

	private void GrdMonthSummary_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (SummaryEditor.PendingAllEvent)
		{
			return;
		}
		C1FlexGridEx grdMonthSummary = SummaryEditor.grdMonthSummary;
		int mouseRow = grdMonthSummary.MouseRow;
		int mouseCol = grdMonthSummary.MouseCol;
		if (mouseRow < grdMonthSummary.Rows.Fixed || mouseCol < grdMonthSummary.Cols.Fixed)
		{
			return;
		}
		object obj = (SummaryEditor.Direction ? grdMonthSummary.Rows[mouseRow].UserData : grdMonthSummary.Cols[mouseCol].UserData);
		if (!(obj is Account account))
		{
			if (obj is Tuple<Account, AuxiliaryItem> tuple)
			{
				CurrentAccount = tuple.Item1;
				CurrentAuxiliary = tuple.Item2;
				AccountTreeEditor.UpdateNodeStatus(tuple);
				SubsidiaryEditor.PopulateSubsidiarySheet(tuple.Item1, StartDate, EndDate, tuple.Item2);
				SubsidiaryEditor.UpdateTitle(tuple.Item1, tuple.Item2);
				SubsidiaryEditor.SubStatus = SubOrTotal.Subsidiary;
				if (SwitchToView(ActiveView.Subsidiary))
				{
					Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
				}
			}
		}
		else
		{
			CurrentAccount = account;
			CurrentAuxiliary = null;
			AccountTreeEditor.UpdateNodeStatus(account);
			SubsidiaryEditor.PopulateSubsidiarySheet(account, StartDate, EndDate);
			SubsidiaryEditor.UpdateTitle(account);
			SubsidiaryEditor.SubStatus = SubOrTotal.Subsidiary;
			if (SwitchToView(ActiveView.Subsidiary))
			{
				Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			}
		}
	}

	private void BalanceEditor_GrdBalance_AfterCollapse(object sender, RowColEventArgs e)
	{
		if (BalanceEditor.PendingAllEvent)
		{
			return;
		}
		C1FlexGridEx grdBalance = BalanceEditor.grdBalance;
		if (e.Row < 0 || e.Row >= grdBalance.Rows.Count)
		{
			return;
		}
		grdBalance.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows[e.Row];
			if (row.UserData == null)
			{
				return;
			}
			foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)AccountTreeEditor.Tree.Rows)
			{
				if (row.UserData.Equals(item.UserData))
				{
					item.Node.Collapsed = row.Node.Collapsed;
					break;
				}
			}
		}
		catch
		{
		}
		finally
		{
			grdBalance.EndUpdate();
		}
	}

	private void BalanceEditor_GrdBalance_DoubleClick(object sender, EventArgs e)
	{
		if (BalanceEditor.PendingAllEvent)
		{
			return;
		}
		try
		{
			C1FlexGridEx grdBalance = BalanceEditor.grdBalance;
			int row = grdBalance.Row;
			if (grdBalance.MouseRow < grdBalance.Rows.Fixed || grdBalance.MouseCol < grdBalance.Cols.Fixed)
			{
				return;
			}
			SubsidiaryEditor.SubStatus = UserSet.Config.BooksStyle.BalanceTo;
			if (SwitchToView(ActiveView.Subsidiary))
			{
				Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			}
			object userData = grdBalance.Rows[row].UserData;
			if (!(userData is Account account))
			{
				if (userData is Tuple<Account, AuxiliaryItem> tuple)
				{
					CurrentAccount = tuple.Item1;
					CurrentAuxiliary = tuple.Item2;
					AccountTreeEditor.UpdateNodeStatus(tuple);
					SubsidiaryEditor.PopulateSubsidiarySheet(tuple.Item1, StartDate, EndDate, tuple.Item2);
					SubsidiaryEditor.UpdateTitle(tuple.Item1, tuple.Item2);
				}
			}
			else
			{
				CurrentAccount = account;
				CurrentAuxiliary = null;
				AccountTreeEditor.UpdateNodeStatus(account);
				SubsidiaryEditor.PopulateSubsidiarySheet(account, StartDate, EndDate);
				SubsidiaryEditor.UpdateTitle(account);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private void BalanceEditor_GrdBalance_Click(object sender, EventArgs e)
	{
		if (BalanceEditor.PendingAllEvent)
		{
			return;
		}
		try
		{
			C1FlexGridEx grdBalance = BalanceEditor.grdBalance;
			int row = grdBalance.Row;
			if (row < grdBalance.Rows.Fixed)
			{
				return;
			}
			object userData = grdBalance.Rows[row].UserData;
			if (userData == null)
			{
				return;
			}
			foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)AccountTreeEditor.Tree.Rows)
			{
				if (userData.Equals(item.UserData))
				{
					AccountTreeEditor.Tree.Row = item.Index;
					break;
				}
			}
		}
		catch
		{
		}
	}

	private void AccountTreeEditor_HideAuxiliaryNode(object sender, Tuple<Account, AuxiliaryClass> e)
	{
		try
		{
			BalanceEditor.RemoveAuxiliaryItems(e.Item1, e.Item2);
		}
		catch
		{
		}
	}

	private void AccountTreeEditor_ShowAuxiliaryNode(object sender, Tuple<Account, AuxiliaryClass> e)
	{
		if (!BalanceEditor.ShouldShowAuxiliaryNode)
		{
			return;
		}
		try
		{
			BalanceEditor.RemoveAuxiliaryItems(e.Item1);
			BalanceEditor.AppendAuxiliaryItems(e.Item1, e.Item2);
			try
			{
				BalanceEditor.grdBalance.BeginUpdate();
				BalanceEditor.PopulateIndex(BalanceEditor.grdBalance);
			}
			finally
			{
				BalanceEditor.grdBalance.EndUpdate();
			}
		}
		catch
		{
		}
	}

	private void AccountTreeEditor_HideEmptyAccount(object sender, bool displayEmpty)
	{
		BalanceEditor.PopulateBalanceSheet(Ledger, displayEmpty);
	}

	private void AccountTreeEditor_ShowEmptyAccount(object sender, bool displayEmpty)
	{
		BalanceEditor.PopulateBalanceSheet(Ledger, displayEmpty);
	}

	private void AccountTreeEditor_CancelAllChecked(object sender, List<Node> selectedNodes)
	{
		switch (CurrentView)
		{
		case ActiveView.TrendChart:
			TrendencyEditor.PopulateTrendency(selectedNodes);
			break;
		case ActiveView.PieChart:
			StructureEditor.PopulateStructure(selectedNodes);
			break;
		}
	}

	private void AccountTreeEditor_Tree_AfterCollapse(object sender, RowColEventArgs e)
	{
		if (AccountTreeEditor.PendingAllEvent)
		{
			return;
		}
		try
		{
			if (e.Row < AccountTreeEditor.Tree.Rows.Fixed)
			{
				return;
			}
			C1.Win.C1FlexGrid.Row row = AccountTreeEditor.Tree.Rows[e.Row];
			if (row.Node.Expanded)
			{
				Node[] nodes = row.Node.Nodes;
				foreach (Node node in nodes)
				{
					node.Collapsed = true;
				}
			}
			foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)BalanceEditor.grdBalance.Rows)
			{
				if (item.UserData == row.UserData)
				{
					item.Node.Collapsed = row.Node.Collapsed;
					break;
				}
			}
		}
		catch
		{
		}
	}

	private void AccountTreeEditor_Tree_MouseDown(object sender, MouseEventArgs e)
	{
		if (AccountTreeEditor.PendingAllEvent)
		{
			return;
		}
		int mouseRow = AccountTreeEditor.Tree.MouseRow;
		if (mouseRow >= AccountTreeEditor.Tree.Rows.Fixed)
		{
			switch (CurrentView)
			{
			case ActiveView.Balance:
				BalanceEditor.SelectRow(AccountTreeEditor.Tree.Rows[mouseRow].UserData);
				break;
			case ActiveView.TrendChart:
				TrendencyEditor.SelectColumn(AccountTreeEditor.Tree.Rows[mouseRow].UserData);
				break;
			case ActiveView.PieChart:
				StructureEditor.SelectColumn(AccountTreeEditor.Tree.Rows[mouseRow].UserData);
				break;
			}
		}
	}

	private void AccountTreeEditor_Tree_DoubleClick(object sender, EventArgs e)
	{
		if (!AccountTreeEditor.PendingAllEvent && AccountTreeEditor.Tree.MouseRow >= AccountTreeEditor.Tree.Rows.Fixed && AccountTreeEditor.Tree.MouseCol >= AccountTreeEditor.Tree.Cols.Fixed)
		{
			int row = AccountTreeEditor.Tree.Row;
			switch (CurrentView)
			{
			case ActiveView.Balance:
				PopulateBalance(AccountTreeEditor.Tree.Rows[row].UserData);
				BalanceEditor.RemoveTotal();
				break;
			case ActiveView.Subsidiary:
				PopulateSubsidiary(AccountTreeEditor.Tree.Rows[row].UserData);
				break;
			case ActiveView.MonthSummary:
				PopulateSummary(AccountTreeEditor.Tree.Rows[row].UserData);
				break;
			case ActiveView.AgeAnalazy:
				PopulateAgeAnalyze(AccountTreeEditor.Tree.Rows[row].UserData);
				break;
			}
			UpdatePreview();
		}
	}

	private void PopulateBalance(object userdata)
	{
		C1FlexGridEx grid1 = BalanceEditor.grdBalance;
		grid1.BeginUpdate();
		try
		{
			Account account = userdata as Account;
			if (account == null)
			{
				Tuple<Account, AuxiliaryClass> tuple = userdata as Tuple<Account, AuxiliaryClass>;
				if (tuple == null)
				{
					Tuple<Account, AuxiliaryItem> tuple2 = userdata as Tuple<Account, AuxiliaryItem>;
					if (tuple2 != null)
					{
						foreachVisible((Account ac) => false, (Tuple<Account, AuxiliaryClass> tp1) => false, (Tuple<Account, AuxiliaryItem> tp2) => tp2.Item1.Code.StartsWith(tuple2.Item1.Code) && tp2.Item2.Code == tuple2.Item2.Code);
						CurrentAccount = tuple2.Item1;
						CurrentAuxiliary = tuple2.Item2;
						AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
					}
				}
				else
				{
					foreachVisible((Account ac) => false, (Tuple<Account, AuxiliaryClass> tp1) => tp1.Item1.Code.StartsWith(tuple.Item1.Code), (Tuple<Account, AuxiliaryItem> tp2) => tp2.Item1.Code.StartsWith(tuple.Item1.Code));
					CurrentAccount = tuple.Item1;
					CurrentAuxiliary = tuple.Item2;
					AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
				}
			}
			else
			{
				foreachVisible((Account ac) => ac.Code.StartsWith(account.Code), (Tuple<Account, AuxiliaryClass> tp1) => tp1.Item1.Code.StartsWith(account.Code), (Tuple<Account, AuxiliaryItem> tp2) => tp2.Item1.Code.StartsWith(account.Code));
				CurrentAccount = account;
				CurrentAuxiliary = null;
				AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
			}
		}
		finally
		{
			grid1.EndUpdate();
		}
		void foreachVisible(Predicate<Account> p1, Predicate<Tuple<Account, AuxiliaryClass>> p2, Predicate<Tuple<Account, AuxiliaryItem>> p3)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)grid1.Rows)
			{
				object userData = item.UserData;
				if (!(userData is Account account2))
				{
					Tuple<Account, AuxiliaryClass> tuple3 = userData as Tuple<Account, AuxiliaryClass>;
					if (tuple3 == null && userData is Tuple<Account, AuxiliaryItem> tuple4 && p3(tuple4))
					{
						hashSet.Add(tuple4.Item1.Code + "-" + tuple4.Item2.Code);
					}
				}
				else if (p1(account2))
				{
					hashSet.Add(account2.Code);
				}
			}
			grid1.FilterManager.Clear();
			grid1.FilterManager.Filters.Add(new SelectFilter
			{
				Kind = FilterKind.Select,
				ColumnId = "Code",
				Relation = FilterRelation.And,
				Values = hashSet
			});
			grid1.FilterManager.Execute();
		}
	}

	private void PopulateSubsidiary(object userdata)
	{
		try
		{
			if (!(userdata is Account account))
			{
				if (userdata is Tuple<Account, AuxiliaryItem> tuple)
				{
					CurrentAccount = tuple.Item1;
					CurrentAuxiliary = tuple.Item2;
					AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
					SubsidiaryEditor.PopulateSubsidiarySheet(tuple.Item1, StartDate, EndDate, tuple.Item2);
					SubsidiaryEditor.UpdateTitle(tuple.Item1, tuple.Item2);
				}
			}
			else
			{
				CurrentAccount = account;
				CurrentAuxiliary = null;
				AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
				SubsidiaryEditor.PopulateSubsidiarySheet(account, StartDate, EndDate);
				SubsidiaryEditor.UpdateTitle(account);
			}
		}
		catch
		{
		}
	}

	private void PopulateSummary(object userdata)
	{
		if (userdata is Account account)
		{
			CurrentAccount = account;
			CurrentAuxiliary = null;
			AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
			SummaryEditor.AnalysisProject = ((!CurrentAccount.IsDebit) ? AnalysisProject.Credits : AnalysisProject.Debits);
			SummaryEditor.Create(SummaryEditor.GetSelectNodeChildren());
			SummaryEditor.UpdateTitle(account);
		}
		else if (userdata is Tuple<Account, AuxiliaryClass> tuple)
		{
			CurrentAccount = tuple.Item1;
			CurrentAuxiliary = tuple.Item2;
			AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
			SummaryEditor.AnalysisProject = ((!CurrentAccount.IsDebit) ? AnalysisProject.Credits : AnalysisProject.Debits);
			SummaryEditor.Create(SummaryEditor.GetSelectNodeChildren());
			SummaryEditor.UpdateTitle(tuple.Item1, tuple.Item2);
		}
		else if (userdata is Tuple<Account, AuxiliaryItem> tuple2)
		{
			CurrentAccount = tuple2.Item1;
			CurrentAuxiliary = tuple2.Item2;
			AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
			SummaryEditor.AnalysisProject = ((!CurrentAccount.IsDebit) ? AnalysisProject.Credits : AnalysisProject.Debits);
			SummaryEditor.Create(SummaryEditor.GetSelectNodeChildren());
			SummaryEditor.UpdateTitle(tuple2.Item1, tuple2.Item2);
		}
		SummaryEditor.SetAnalysisStatus();
	}

	private void PopulateAgeAnalyze(object userData)
	{
		if (!(userData is Account currentAccount))
		{
			if (!(userData is Tuple<Account, AuxiliaryClass> tuple))
			{
				if (userData is Tuple<Account, AuxiliaryItem> tuple2)
				{
					CurrentAccount = tuple2.Item1;
					CurrentAuxiliary = tuple2.Item2;
					AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
					LedgerAgingEditor.Account = tuple2.Item1;
					LedgerAgingEditor.Populate(tuple2.Item2);
				}
			}
			else
			{
				CurrentAccount = tuple.Item1;
				CurrentAuxiliary = tuple.Item2;
				AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
				LedgerAgingEditor.Account = tuple.Item1;
				LedgerAgingEditor.Populate(tuple.Item2);
			}
		}
		else
		{
			CurrentAccount = currentAccount;
			CurrentAuxiliary = null;
			AccountTreeEditor.CurrentOpendedRow = AccountTreeEditor.Tree.Rows[AccountTreeEditor.Tree.Row];
			LedgerAgingEditor.Account = CurrentAccount;
			LedgerAgingEditor.Populate(null);
		}
	}

	public void SaveConfig()
	{
		ViewStyle.Save(ConfigManager.USERCONFIGFILE, StyleRecord.ViewStyle);
		LedgerHistory2.OpenHistory.Save(ConfigManager.RECENTLEDGERPATH);
	}

	private void BalanceEditor_AccountPeriod_Changed(object sender, Tuple<DateTime, DateTime> e)
	{
		if (!BalanceEditor.PendingAllEvent && lazySubsidiaryEditor.IsValueCreated)
		{
			SubsidiaryEditor.SetTitle(e.Item1.ToString("yyyy-MM-dd"), e.Item2.ToString("yyyy-MM-dd"));
		}
	}

	private void grid_BodySelectionChanged(object sender, EventArgs e)
	{
		try
		{
			List<double> list = new List<double>();
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			C1.Win.C1FlexGrid.CellRange selection = c1FlexGridEx.Selection;
			for (int i = selection.r1; i <= selection.r2; i++)
			{
				if (i < 0 || !c1FlexGridEx.Rows[i].IsVisible)
				{
					continue;
				}
				for (int j = selection.c1; j <= selection.c2; j++)
				{
					if (j >= 0 && c1FlexGridEx.Cols[j].IsVisible && double.TryParse(c1FlexGridEx[i, j]?.ToString(), out var result))
					{
						list.Add(result);
					}
				}
			}
			_owner.OnLedgerSelectionChanged(new LedgerSelectionChangedEventArgs
			{
				Viewer = this,
				Numbers = list
			});
		}
		catch
		{
		}
	}

	private void grid_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		Keys keyData = e.KeyData;
		if (keyData != (Keys.C | Keys.Control))
		{
			return;
		}
		try
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			if (c1FlexGridEx.Row >= 0 && c1FlexGridEx.Col >= 0)
			{
				int rowsCount;
				string selectionContent = Common.GetSelectionContent(c1FlexGridEx, out rowsCount);
				if ((LicenseCheckHandleOnCopyLedgerData == null || LicenseCheckHandleOnCopyLedgerData(rowsCount)) && !string.IsNullOrWhiteSpace(selectionContent))
				{
					Clipboard.SetText(selectionContent);
				}
			}
		}
		catch (ExternalException)
		{
		}
	}

	private void grid_MouseWheel(object sender, MouseEventArgs e)
	{
		if ((Control.ModifierKeys & Keys.Control) == Keys.Control && sender is C1FlexGridEx c1FlexGridEx)
		{
			string name = c1FlexGridEx.Font.FontFamily.Name;
			float size = c1FlexGridEx.Font.Size;
			float num = ((e.Delta > 0) ? (size + 1f) : (size - 1f));
			StyleRecord.ViewStyle.FontSize = ((num > 1f) ? num : 1f);
			StyleRecord.ViewStyle.Height *= StyleRecord.ViewStyle.FontSize / size;
			switch (CurrentView)
			{
			case ActiveView.Balance:
				StyleRecord.ResumeFont(AccountTreeEditor.Tree);
				StyleRecord.ResumeHeight(AccountTreeEditor.Tree);
				StyleRecord.ResumeFont(BalanceEditor.grdBalance);
				StyleRecord.ResumeHeight(BalanceEditor.grdBalance);
				break;
			case ActiveView.Subsidiary:
				StyleRecord.ResumeFont(AccountTreeEditor.Tree);
				StyleRecord.ResumeHeight(AccountTreeEditor.Tree);
				StyleRecord.ResumeFont(SubsidiaryEditor.grdVoucher);
				StyleRecord.ResumeHeight(SubsidiaryEditor.grdVoucher);
				StyleRecord.ResumeFont(SubsidiaryEditor.grdSubsidiary);
				StyleRecord.ResumeHeight(SubsidiaryEditor.grdSubsidiary);
				break;
			case ActiveView.TrendChart:
				StyleRecord.ResumeFont(AccountTreeEditor.Tree);
				StyleRecord.ResumeHeight(AccountTreeEditor.Tree);
				StyleRecord.ResumeFont(TrendencyEditor.grdTrendTable);
				StyleRecord.ResumeHeight(TrendencyEditor.grdTrendTable);
				break;
			case ActiveView.PieChart:
				StyleRecord.ResumeFont(AccountTreeEditor.Tree);
				StyleRecord.ResumeHeight(AccountTreeEditor.Tree);
				StyleRecord.ResumeFont(StructureEditor.grdStructureTable);
				StyleRecord.ResumeHeight(StructureEditor.grdStructureTable);
				break;
			case ActiveView.MonthSummary:
				StyleRecord.ResumeFont(AccountTreeEditor.Tree);
				StyleRecord.ResumeHeight(AccountTreeEditor.Tree);
				StyleRecord.ResumeFont(SummaryEditor.grdMonthSummary);
				StyleRecord.ResumeHeight(SummaryEditor.grdMonthSummary);
				break;
			case ActiveView.AgeAnalazy:
				StyleRecord.ResumeFont(AccountTreeEditor.Tree);
				StyleRecord.ResumeHeight(AccountTreeEditor.Tree);
				StyleRecord.ResumeFont(LedgerAgingEditor.grid);
				StyleRecord.ResumeHeight(LedgerAgingEditor.grid);
				break;
			case ActiveView.VoucherList:
			case ActiveView.MarkVoucers:
				break;
			}
		}
	}

	private void LedgerViewer_PreviewStatusChanged(object sender, bool e)
	{
		if (!e)
		{
			PreviewEditor._printer = null;
		}
	}

	private void AttachGenerateEvent1(C1FlexGridEx grid)
	{
		grid.MouseWheel += grid_MouseWheel;
		grid.PreviewKeyDown += grid_PreviewKeyDown;
		grid.BodySelectionChanged += grid_BodySelectionChanged;
	}

	private void AttachGenerateEvent2(C1FlexGridEx grid)
	{
		grid.MouseWheel += grid_MouseWheel;
	}

	public void ModifyBeginBalance(object userData)
	{
		try
		{
			frmBalanceEditor frmBalanceEditor2 = new frmBalanceEditor(Ledger, userData, this);
			if (DialogResult.OK != frmBalanceEditor2.ShowDialog())
			{
				return;
			}
			foreach (Tuple<Account, decimal> item in frmBalanceEditor2.UpdateAccount)
			{
				Ledger.InitialBalance[item.Item1].Total = item.Item2;
				item.Item1.Dirty = 2;
			}
			foreach (Tuple<Account, AuxiliaryItem, decimal> tp in frmBalanceEditor2.UpdateAuxiliary)
			{
				Dictionary<AuxiliaryItem, decimal> itemBalances = Ledger.InitialBalance[tp.Item1].ClassBalances.First((KeyValuePair<AuxiliaryClass, ClassBalance> c) => c.Key == tp.Item2.Class).Value.ItemBalances;
				itemBalances[tp.Item2] = tp.Item3;
			}
			if (frmBalanceEditor2.UpdateAccount.Count > 0 || frmBalanceEditor2.UpdateAuxiliary.Count > 0)
			{
				Ledger.Save();
				OnAfterModifyBalance(null);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public void AddVoucher(string type, DateTime date)
	{
		try
		{
			if (frmVoucherEditor == null || frmVoucherEditor.Ledger != Ledger)
			{
				frmVoucherEditor = new frmVoucherEditor(this, Ledger);
				frmVoucherEditor.AfterAddVoucher += delegate(object s1, IGrouping<string, Voucher> e1)
				{
					OnAfterAddVoucher(e1);
				};
				frmVoucherEditor.AfterModifyVoucher += delegate(object s1, IGrouping<string, Voucher> e1)
				{
					OnAfterModifyVoucher(e1);
				};
				frmVoucherEditor.AfterDeleteVoucher += delegate(object s1, IGrouping<string, Voucher> e1)
				{
					OnAfterDeleteVoucher(e1);
				};
			}
			if (DialogResult.OK == frmVoucherEditor.ShowAddDialog(new ShowContext(type, date)))
			{
				Ledger.Save();
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public async Task ModifyVoucher(Voucher voucher)
	{
		try
		{
			if (frmVoucherEditor == null || frmVoucherEditor.Ledger != Ledger)
			{
				frmVoucherEditor = new frmVoucherEditor(this, Ledger);
				frmVoucherEditor.AfterAddVoucher += delegate(object s1, IGrouping<string, Voucher> e1)
				{
					OnAfterAddVoucher(e1);
				};
				frmVoucherEditor.AfterModifyVoucher += delegate(object s1, IGrouping<string, Voucher> e1)
				{
					OnAfterModifyVoucher(e1);
				};
				frmVoucherEditor.AfterDeleteVoucher += delegate(object s1, IGrouping<string, Voucher> e1)
				{
					OnAfterDeleteVoucher(e1);
				};
			}
			IEnumerable<Voucher> voucherList = Ledger.Vouchers.Where((Voucher v) => v.Day.Year == voucher.Day.Year && v.Day.Month == voucher.Day.Month && v.Day.Day == voucher.Day.Day && v.Type.Name == voucher.Type.Name && v.Number == voucher.Number);
			if (DialogResult.OK == frmVoucherEditor.ShowModifyDialog(voucherList, new ShowContext(voucher.Type.Name, voucher.Day)))
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "会计凭证修改成功，即将重新加载账套");
				await OpenLedger(CurrentFilePath);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	public static string Parse001File(string xjy001File)
	{
		setFileAttribute(xjy001File);
		string dbFilePath = null;
		ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> progress)
		{
			XinJiYuan xjy = new XinJiYuan();
			int totalProgress = 114;
			xjy.ProgressChanged += delegate(object s, CrawlerModelAlias::Leqisoft.Model.GetLedgerProgressEventArgs e)
			{
				progress.Report(new ProgressInfo
				{
					MainCaption = "正在转换账套文件，请稍候......",
					MainProgress = e.Progress * 100 / totalProgress
				});
			};
			await Task.Run(delegate
			{
				string text = xjy.Convert001File(xjy001File, Path.GetDirectoryName(xjy001File));
				dbFilePath = text;
			});
			return Task.FromResult(new object());
		});
		progressForm.ShowDialog();
		try
		{
			progressForm.Task.Wait();
		}
		catch (AggregateException ex)
		{
			throw ex.InnerException;
		}
		return dbFilePath;
	}

	public void Dispose()
	{
		AccountTreeEditor.Dispose();
		Ledger = null;
		CurrentAccount = null;
		CacheManager = null;
		BalanceEditor.Dispose();
	}

	private async Task OpenLedgerFileImpl(string fullPath)
	{
		if (!File.Exists(fullPath))
		{
			throw new FileNotFoundException(fullPath);
		}
		setFileAttribute(fullPath);
		ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> progress)
		{
			progress.Report(new ProgressInfo
			{
				MainCaption = "正在打开账套，请稍候......",
				MainProgress = 0
			});
			Application.DoEvents();
			Ledger = Ledger.LoadFromFile(fullPath);
			CurrentFilePath = fullPath;
			InitStatus();
			if (lazyBalanceEditor.IsValueCreated)
			{
				SwitchToView(ActiveView.Balance);
			}
			else
			{
				OnPreparePrintGridChanged();
			}
			BalanceEditor.StartDate = Ledger.StartDate;
			BalanceEditor.EndDate = Ledger.GetEndDate();
			CurrentAccount = Ledger.RootAccounts.FirstOrDefault();
			CurrentAuxiliary = null;
			AccountTreeEditor.PopulateAccountTree(Ledger, AccountTreeEditor.DisplayEmptyAccount);
			AccountTreeEditor.UpdateNodeStatus(CurrentAccount);
			BalanceEditor.PopulateBalanceSheet(Ledger, AccountTreeEditor.DisplayEmptyAccount);
			ctnView.BringToFront();
			InitOtherView();
			SetTheme();
			return await Task.FromResult(new object());
		});
		progressForm.ShowDialog();
		await progressForm.Task;
	}

	private void InitStatus()
	{
		if (lazyAccountTreeEditor.IsValueCreated)
		{
			AccountTreeEditor.SelectedStructureNodes.Clear();
			AccountTreeEditor.SelectedTrendencyNodes.Clear();
		}
	}

	private void InitOtherView()
	{
		if (lazySubsidiaryEditor.IsValueCreated)
		{
			SubsidiaryEditor.ShowVoucher(visible: false);
		}
		if (lazyVoucherListEditor.IsValueCreated)
		{
			VoucherListEditor.PopulateTree();
		}
		if (lazySummaryEditor.IsValueCreated)
		{
			SummaryEditor.Create(SummaryEditor.GetSelectNodeChildren());
		}
	}

	public void RenameCurrentFile(string newPath)
	{
		CurrentFilePath = newPath;
	}

	private string MergeLedgerImpl(string otherFile)
	{
		string text = Path.GetTempFileName() + new Random().Next(1000, 9999);
		string text2 = Path.GetTempFileName() + new Random().Next(1000, 9999);
		File.Copy(CurrentFilePath, text, overwrite: true);
		File.Copy(otherFile, text2, overwrite: true);
		PrepareForMergeLedger(text, text2);
		Ledger ledger = Ledger.LoadFromFile(text);
		Ledger ledger2 = Ledger.LoadFromFile(text2);
		Ledger ledger3 = ((ledger.StartDate.Year < ledger2.StartDate.Year) ? ledger : ledger2);
		Ledger ledger4 = ((ledger.StartDate.Year >= ledger2.StartDate.Year) ? ledger : ledger2);
		string sourceFileName = ((ledger.StartDate.Year < ledger2.StartDate.Year) ? text : text2);
		new LedgerMerge().Merge(ledger3, ledger4).TotalSave();
		string directoryName = Path.GetDirectoryName(CurrentFilePath);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(CurrentFilePath);
		string arg = Regex.Replace(fileNameWithoutExtension, "([-_]*20[0-9]{2}[-_]*)+$", "").TrimEnd('_');
		string text3 = Path.Combine(directoryName, $"{arg}_{ledger3.StartDate.Year}-{ledger4.GetEndDate().Year}.db");
		if (File.Exists(text3))
		{
			if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前路径包含同名文件是否覆盖？", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				File.Copy(sourceFileName, text3, overwrite: true);
				return text3;
			}
			return null;
		}
		File.Copy(sourceFileName, text3, overwrite: true);
		return text3;
	}

	private void PrepareForMergeLedger(string filePath1, string filePath2)
	{
		PrepareAuxiliaryClassForMergeLedger(filePath1, filePath2);
	}

	private void PrepareAuxiliaryClassForMergeLedger(string filePath1, string filePath2)
	{
		List<Tuple<int, string, string>> tableData_ItemClass = Ledger.GetTableData_ItemClass(filePath1);
		List<Tuple<int, string, string>> tableData_ItemClass2 = Ledger.GetTableData_ItemClass(filePath2);
		if (tableData_ItemClass.Count == 0 || tableData_ItemClass2.Count == 0)
		{
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		foreach (Tuple<int, string, string> item4 in tableData_ItemClass)
		{
			if (item4.Item2 != null && item4.Item3 != null)
			{
				dictionary[item4.Item3] = item4.Item2;
				dictionary2[item4.Item2] = item4.Item3;
			}
		}
		int num = 0;
		List<AuxiliaryClassReCodeSetting> list = new List<AuxiliaryClassReCodeSetting>();
		foreach (Tuple<int, string, string> item5 in tableData_ItemClass2)
		{
			int item = item5.Item1;
			string item2 = item5.Item2;
			string item3 = item5.Item3;
			if (item > num)
			{
				num = item;
			}
			AuxiliaryClassReCodeSetting auxiliaryClassReCodeSetting = null;
			if (dictionary.TryGetValue(item3, out var value))
			{
				auxiliaryClassReCodeSetting = ((!(item2 == value)) ? new AuxiliaryClassReCodeSetting
				{
					oldId = item,
					oldCode = item2,
					newCode = value
				} : new AuxiliaryClassReCodeSetting
				{
					oldId = item,
					oldCode = item2,
					newCode = item2
				});
			}
			else if (!dictionary2.ContainsKey(item2))
			{
				auxiliaryClassReCodeSetting = new AuxiliaryClassReCodeSetting
				{
					oldId = item,
					oldCode = item2,
					newCode = item2
				};
			}
			else
			{
				int num2 = 0;
				string text = num2.ToString().PadLeft(3, '0');
				while (dictionary2.ContainsKey(text))
				{
					text = num2++.ToString().PadLeft(3, '0');
				}
				auxiliaryClassReCodeSetting = new AuxiliaryClassReCodeSetting
				{
					oldId = item,
					oldCode = item2,
					newCode = text
				};
			}
			if (auxiliaryClassReCodeSetting == null)
			{
				auxiliaryClassReCodeSetting = new AuxiliaryClassReCodeSetting
				{
					oldId = item,
					oldCode = item2,
					newCode = item2
				};
			}
			list.Add(auxiliaryClassReCodeSetting);
		}
		List<AuxiliaryClassReCodeSetting> list2 = new List<AuxiliaryClassReCodeSetting>();
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			AuxiliaryClassReCodeSetting auxiliaryClassReCodeSetting2 = list[num3];
			if (auxiliaryClassReCodeSetting2.newCode != auxiliaryClassReCodeSetting2.oldCode)
			{
				list2.Add(auxiliaryClassReCodeSetting2);
			}
			else
			{
				auxiliaryClassReCodeSetting2.newId = auxiliaryClassReCodeSetting2.oldId;
			}
		}
		if (list2.Count == 0)
		{
			return;
		}
		num++;
		list2.Sort((AuxiliaryClassReCodeSetting left, AuxiliaryClassReCodeSetting right) => left.oldId.CompareTo(right.oldId));
		foreach (AuxiliaryClassReCodeSetting item6 in list2)
		{
			item6.newId = num++;
		}
		List<Tuple<int, int, string>> dataList = list2.Select((AuxiliaryClassReCodeSetting row) => Tuple.Create(row.oldId, row.newId, row.newCode)).ToList();
		Ledger.UpdateTableData_ItemClassIdAndCode(filePath2, dataList);
		list.Sort((AuxiliaryClassReCodeSetting left, AuxiliaryClassReCodeSetting right) => left.newId.CompareTo(right.newId));
		List<Tuple<int, int>> list3 = new List<Tuple<int, int>>();
		for (int num4 = list.Count - 1; num4 >= 0; num4--)
		{
			AuxiliaryClassReCodeSetting auxiliaryClassReCodeSetting3 = list[num4];
			int newId = auxiliaryClassReCodeSetting3.newId;
			int num5 = num4;
			if (newId != num5)
			{
				list3.Add(Tuple.Create(newId, num5));
			}
		}
		Ledger.UpdateTableData_ItemClassId(filePath2, list3);
	}

	private void SwitchToPreview(bool preview)
	{
		PreviewEditor.IsPreview = preview;
		if (preview)
		{
			PreviewEditor.Preview.BringToFront();
		}
		else
		{
			PreviewEditor.Preview.SendToBack();
		}
	}

	internal bool SwitchToView(ActiveView view)
	{
		if (Ledger == null && view != ActiveView.Empty)
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return false;
		}
		CurrentView = view;
		switch (view)
		{
		case ActiveView.Balance:
			BalanceEditor.View.BringToFront();
			ShowTree(AccountTreeEditor.Tree);
			Common.SetTreeCheck(AccountTreeEditor.Tree, CheckEnum.None);
			ResumeBalanceViewStyle();
			OnPreparePrintGridChanged();
			break;
		case ActiveView.Subsidiary:
			SubsidiaryEditor.View.BringToFront();
			ShowTree(AccountTreeEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.VoucherList:
			VoucherListEditor.View.BringToFront();
			ShowTree(VoucherListEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.MarkVoucers:
			VoucherMarkedEditor.View.BringToFront();
			ShowTree(VoucherMarkedEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.PieChart:
			StructureEditor.View.BringToFront();
			ShowTree(AccountTreeEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.TrendChart:
			TrendencyEditor.View.BringToFront();
			ShowTree(AccountTreeEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.MonthSummary:
			SummaryEditor.View.BringToFront();
			ShowTree(AccountTreeEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.AgeAnalazy:
			LedgerAgingEditor.View.BringToFront();
			ShowTree(AccountTreeEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.Validate:
			ValidateEditor.View.BringToFront();
			ShowTree(ValidateEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		case ActiveView.Empty:
			OnPreparePrintGridChanged();
			break;
		default:
			BalanceEditor.View.BringToFront();
			ShowTree(AccountTreeEditor.Tree);
			OnPreparePrintGridChanged();
			break;
		}
		return true;
	}

	internal void ResumeBalanceViewStyle()
	{
		StyleRecord.ResumeFont(BalanceEditor.grdBalance);
		StyleRecord.ResumeHeight(BalanceEditor.grdBalance);
		StyleRecord.ResumeFont(AccountTreeEditor.Tree);
		StyleRecord.ResumeHeight(AccountTreeEditor.Tree);
	}

	private void ShowTree(C1FlexGrid tree)
	{
		pnlTree.Visible = true;
		foreach (object control in pnlTree.Controls)
		{
			if (control is C1FlexGrid c1FlexGrid)
			{
				c1FlexGrid.Visible = c1FlexGrid == tree;
			}
		}
	}

	public void SetTheme()
	{
		if (lazyAccountTreeEditor.IsValueCreated)
		{
			AccountTreeEditor.SetTheme();
		}
		if (lazyBalanceEditor.IsValueCreated)
		{
			BalanceEditor.SetTheme();
		}
		if (lazySubsidiaryEditor.IsValueCreated)
		{
			SubsidiaryEditor.SetTheme();
		}
		if (lazyTrendencyEditor.IsValueCreated)
		{
			TrendencyEditor.SetTheme();
		}
		if (lazyStructureEditor.IsValueCreated)
		{
			StructureEditor.SetTheme();
		}
		if (lazyVoucherListEditor.IsValueCreated)
		{
			VoucherListEditor.SetTheme();
		}
		if (lazySummaryEditor.IsValueCreated)
		{
			SummaryEditor.SetTheme();
		}
		if (lazyVoucherMarkedEditor.IsValueCreated)
		{
			VoucherMarkedEditor.SetTheme();
		}
		if (lazyLedgerAgingEditor.IsValueCreated)
		{
			LedgerAgingEditor.SetTheme();
		}
	}

	public void UpdatePreview()
	{
		try
		{
			PrintPreview(PreviewEditor.IsPreview);
		}
		catch (ArgumentOutOfRangeException ex)
		{
			ex.Log();
			PreviewEditor.Preview.Document = EmptyView();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ParamName);
		}
		catch (Exception ex2)
		{
			ex2.Log();
			PreviewEditor.Preview.Document = EmptyView();
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
	}

	private C1PrintDocument EmptyView()
	{
		return new C1PrintDocument();
	}

	internal void DirectionChange_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (!(sender is C1Command { UserData: C1FlexGridEx { Selection: { r1: var i } selection } userData }))
			{
				return;
			}
			for (; i <= selection.r2; i++)
			{
				if (i >= userData.Rows.Fixed && i < userData.Rows.Count && userData.Rows[i].UserData is Voucher { DirectionToggled: false } voucher && userData.Rows[i].Visible)
				{
					voucher.ToggleDirection();
					CacheManager.CacheValid = false;
					BalanceEditor.PopulateBalanceSheet(Ledger, AccountTreeEditor.DisplayEmptyAccount);
					userData.Rows[i]["Debit"] = (voucher.IsDebit ? voucher.Amount : 0m);
					userData.Rows[i]["Credit"] = (voucher.IsDebit ? 0m : voucher.Amount);
				}
			}
			TriggerLedgerDataChangeEvent();
			Ledger.Save();
		}
		catch
		{
		}
	}

	internal void DirectionReduce_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (!(sender is C1Command { UserData: C1FlexGridEx { Selection: { r1: var i } selection } userData }))
			{
				return;
			}
			for (; i <= selection.r2; i++)
			{
				if (i >= userData.Rows.Fixed && i < userData.Rows.Count && userData.Rows[i].UserData is Voucher { DirectionToggled: not false } voucher && userData.Rows[i].Visible)
				{
					voucher.ToggleDirection();
					CacheManager.CacheValid = false;
					BalanceEditor.PopulateBalanceSheet(Ledger, AccountTreeEditor.DisplayEmptyAccount);
					userData.Rows[i]["Debit"] = (voucher.IsDebit ? voucher.Amount : 0m);
					userData.Rows[i]["Credit"] = (voucher.IsDebit ? 0m : voucher.Amount);
				}
			}
			TriggerLedgerDataChangeEvent();
			Ledger.Save();
		}
		catch
		{
		}
	}

	internal void MarkGridRowImpl(C1.Win.C1FlexGrid.Row row)
	{
		if (row.UserData is Voucher { VoucherMark: false } voucher && row.Visible)
		{
			voucher.ToggleMark();
			row.StyleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
			row.StyleNew.ForeColor = Common.MarkForeColor;
		}
	}

	internal void MakeMarkRow(int rowIndex, C1FlexGridEx grid)
	{
		try
		{
			if (rowIndex >= grid.Rows.Fixed && rowIndex < grid.Rows.Count)
			{
				MarkGridRowImpl(grid.Rows[rowIndex]);
				Ledger.Save();
			}
		}
		catch (Exception exception)
		{
			exception.Log("标记关注时发生了未预期的异常");
		}
	}

	internal void MakeMarkRows(C1.Win.C1FlexGrid.CellRange cellRange, C1FlexGridEx grid)
	{
		try
		{
			for (int i = cellRange.TopRow; i <= cellRange.BottomRow; i++)
			{
				MarkGridRowImpl(grid.Rows[i]);
			}
			Ledger.Save();
		}
		catch (Exception exception)
		{
			exception.Log("标记关注时发生了未预期的异常");
		}
	}

	internal void MakeMark_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (!(sender is C1Command { UserData: C1FlexGridEx userData }) || userData.Selection.r1 < 0)
			{
				return;
			}
			C1.Win.C1FlexGrid.CellRange selection = userData.Selection;
			for (int i = selection.r1; i <= selection.r2; i++)
			{
				if (i >= userData.Rows.Fixed && i < userData.Rows.Count)
				{
					MarkGridRowImpl(userData.Rows[i]);
				}
			}
			Ledger.Save();
		}
		catch
		{
		}
	}

	internal void MarkCancel_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (sender is C1Command { UserData: C1FlexGridEx { Selection: { r1: var i } selection } userData })
			{
				for (; i <= selection.r2; i++)
				{
					MarkCancelRowImpl(i, userData);
				}
				Ledger.Save();
			}
		}
		catch
		{
		}
	}

	internal void MarkCancelRow(int rowIndex, C1FlexGridEx grid)
	{
		try
		{
			if (rowIndex >= grid.Rows.Fixed && rowIndex < grid.Rows.Count)
			{
				MarkCancelRowImpl(rowIndex, grid);
				Ledger.Save();
			}
		}
		catch (Exception exception)
		{
			exception.Log("标记关注时发生了未预期的异常");
		}
	}

	internal void MarkCancelRows(C1.Win.C1FlexGrid.CellRange cellRange, C1FlexGridEx grid)
	{
		try
		{
			for (int i = cellRange.TopRow; i <= cellRange.BottomRow; i++)
			{
				MarkCancelRowImpl(i, grid);
			}
			Ledger.Save();
		}
		catch (Exception exception)
		{
			exception.Log("取消关注时发生了未预期的异常");
		}
	}

	internal void MarkCancelRowImpl(int rowIndex, C1FlexGridEx grid)
	{
		if (rowIndex < grid.Rows.Fixed || rowIndex >= grid.Rows.Count)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row = grid.Rows[rowIndex];
		if (!(row.UserData is Voucher { VoucherMark: not false } voucher) || !row.Visible)
		{
			return;
		}
		voucher.ToggleMark();
		Color color = Color.White;
		try
		{
			if (rowIndex > 0 && grid.Rows[rowIndex - 1].UserData is Voucher voucher2 && voucher2.Number == voucher.Number && voucher2.Type == voucher.Type)
			{
				color = grid.Rows[rowIndex - 1].Style?.BackColor ?? Color.White;
			}
			else if (grid.Rows[rowIndex + 1].UserData is Voucher voucher3 && voucher3.Number == voucher.Number && voucher3.Type == voucher.Type)
			{
				color = grid.Rows[rowIndex + 1].Style?.BackColor ?? Color.White;
			}
		}
		catch (Exception exception)
		{
			exception.Log("取消关注时发生了未预期的异常");
		}
		row.StyleNew.BackColor = ((color == UserSet.Config.TableStyle.CheckFailColor) ? Color.White : color);
		row.StyleNew.ForeColor = grid.ForeColor;
	}

	internal void ColHide_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (sender is C1Command { UserData: C1FlexGridEx userData })
			{
				userData.Cols[userData.Col].Visible = false;
				StyleRecord.RecordVisible(userData.Name, userData.Cols[userData.Col].Name, Visible: false);
			}
		}
		catch
		{
		}
	}

	internal void CancelHide_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (sender is C1Command { UserData: C1FlexGridEx userData })
			{
				StyleRecord.ResumeVisible(userData);
			}
		}
		catch
		{
		}
	}

	private static void setFileAttribute(string fileName)
	{
		try
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}
		catch (Exception)
		{
		}
	}
}
