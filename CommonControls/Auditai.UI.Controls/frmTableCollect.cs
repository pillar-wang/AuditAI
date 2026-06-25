using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls.CollectTable;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class frmTableCollect : C1RibbonForm
{
	private const string CO_BALANCE = "科目余额表";

	private const string CO_SUBSIDIARY = "明细账";

	private const string CO_SUMMARY = "月度汇总表";

	private SelectorEditor selectorEditor;

	private Form owner;

	private bool _attachEvent;

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

	private C1SplitterPanel pnlBalanceGrid;

	private C1FlexGrid grdMapping;

	private C1SplitButtonEx spbAnalysis;

	private DropDownItem dropDownItem1;

	private DropDownItem dropDownItem2;

	private DropDownItem dropDownItem3;

	private C1SplitterPanel pnlBalanceSelector;

	private C1SplitterPanel pnlSelectorTitle;

	private C1Label lblSelctorTitle;

	public TableCollectorAbstract Collector { get; private set; }

	public TableCollectResult Result { get; private set; }

	public frmTableCollect(Ledger ledger, Table table, Form owner)
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
		this.owner = owner;
		Initialize(ledger, table);
		AttachEvent();
	}

	private void FrmTableCollect_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.TableCollect16);
	}

	public void LoadFormula(string formula)
	{
		Ledger ledger = Collector.Setting.Ledger;
		Table table = Collector.Setting.Table;
		if (string.IsNullOrWhiteSpace(formula))
		{
			Collector = TableCollectorAbstract.Intelligence(ledger, table);
		}
		else
		{
			Collector = TableCollectorAbstract.Deserialize(formula, ledger, table);
		}
		Collector = Collector ?? new TableCollectorBalance(ledger, table);
		UpdateView();
		selectorEditor.SetCollector(Collector);
		selectorEditor.Populate();
		UpdateTitle();
	}

	public new DialogResult ShowDialog()
	{
		Theme.SetCurrentTree(this);
		return ShowDialog(owner);
	}

	private object GetDCChar(bool isDebit, decimal balance)
	{
		if (balance == 0m)
		{
			return "平";
		}
		if (balance > 0m)
		{
			if (!isDebit)
			{
				return "贷";
			}
			return "借";
		}
		if (!isDebit)
		{
			return "借";
		}
		return "贷";
	}

	private void comboCollectObject_TextChanged(object sender, EventArgs e)
	{
		SetComboList();
		PopulateAuxiliaryTree(selectedAccount());
		CollectObjectEnum collectObjectEnum = selectCollectObject();
		if (collectObjectEnum == CollectObjectEnum.Subsidiary)
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
		spbAnalysis.Visible = collectObjectEnum == CollectObjectEnum.Summary;
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
		selectorEditor.SetCollector(Collector);
		selectorEditor.Populate();
		UpdateTitle();
	}

	private void ComboStartMonth_SelectedItemChanged(object sender, EventArgs e)
	{
		int year = Collector.TitlePeriod.Item1.Year;
		int month = int.Parse(comboStartMonth.Text.Trim());
		Collector.Setting.Start = new DateTime(year, month, 1);
		selectorEditor.Populate();
		UpdateTitle();
	}

	private void ComboEndMonth_SelectedItemChanged(object sender, EventArgs e)
	{
		int year = Collector.TitlePeriod.Item1.Year;
		int month = int.Parse(comboEndMonth.Text.Trim());
		Collector.Setting.End = new DateTime(year, month, DateTime.DaysInMonth(year, month));
		selectorEditor.Populate();
		UpdateTitle();
	}

	private void ComboAccountTree_SelectNodeChanged(object sender, TreeViewEventArgs e)
	{
		PopulateAuxiliaryTree(selectedAccount());
		Collector.Setting.Account = selectedAccount();
		object obj = selectedAuxiliary();
		object auxiliary = Collector.Setting.Auxiliary;
		if (selectCollectObject() == CollectObjectEnum.Balance && ((obj == null && auxiliary != null) || (obj != null && auxiliary == null)))
		{
			SetComboList();
		}
		Collector.Setting.Auxiliary = obj;
		selectorEditor.Populate();
		UpdateTitle();
		if (grdMapping.Cols.Count > 1)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdMapping.GetCellStyle(1, 1);
			if (string.IsNullOrWhiteSpace(cellStyle.ComboList))
			{
				SetComboList();
			}
		}
		grdMapping.AllowEditing = selectedAccount() != null;
	}

	private void ComboAuxiliaryTree_SelectNodeChanged(object sender, TreeViewEventArgs e)
	{
		object obj = selectedAuxiliary();
		object auxiliary = Collector.Setting.Auxiliary;
		if (selectCollectObject() == CollectObjectEnum.Balance && ((obj == null && auxiliary != null) || (obj != null && auxiliary == null)))
		{
			SetComboList();
		}
		Collector.Setting.Account = selectedAccount();
		Collector.Setting.Auxiliary = obj;
		selectorEditor.Populate();
		UpdateTitle();
		if (grdMapping.Cols.Count > 1)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdMapping.GetCellStyle(1, 1);
			if (string.IsNullOrWhiteSpace(cellStyle.ComboList))
			{
				SetComboList();
			}
		}
		grdMapping.AllowEditing = selectedAccount() != null;
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		Collector.Maps = new Dictionary<long, string>();
		foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)grdMapping.Cols)
		{
			if (item.UserData is Auditai.Model.Column column2)
			{
				string value = grdMapping[1, item.Index]?.ToString();
				if (!string.IsNullOrWhiteSpace(value))
				{
					Collector.Maps.Add(column2.Id.Value, value);
				}
			}
		}
		Result = selectorEditor.GetResult();
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
		Table table = Collector.Setting.Table;
		Collector = TableCollectorAbstract.Intelligence(ledger, table);
		Collector = Collector ?? new TableCollectorBalance(ledger, table);
		UpdateView();
		selectorEditor.SetCollector(Collector);
		selectorEditor.Populate();
		UpdateTitle();
	}

	private void UpdateTitle()
	{
		if (Collector.Setting.Account == null)
		{
			lblSelctorTitle.Text = string.Empty;
			return;
		}
		switch (Collector.CollectObject)
		{
		case CollectObjectEnum.Balance:
			lblSelctorTitle.Text = getAllPath(Collector.Setting.Account) + "科目余额表";
			break;
		case CollectObjectEnum.Subsidiary:
			lblSelctorTitle.Text = getAllPath(Collector.Setting.Account) + "明细账";
			break;
		case CollectObjectEnum.Summary:
			lblSelctorTitle.Text = getAllPath(Collector.Setting.Account) + "月度汇总表";
			break;
		case CollectObjectEnum.None:
			break;
		}
		static string getAllPath(Account a)
		{
			List<string> list = new List<string>();
			if (a != null)
			{
				list.Add(a.Name);
				while (a.Parent != null)
				{
					a = a.Parent;
					list.Add(a.Name);
				}
			}
			if (list.Count == 0)
			{
				return string.Empty;
			}
			list.Reverse();
			return "（" + string.Join("-", list) + "）";
		}
	}

	private void GrdMapping_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode != Keys.Delete)
		{
			return;
		}
		CellRange selection = grdMapping.Selection;
		if (selection.TopRow > 1 || selection.BottomRow < 1)
		{
			return;
		}
		for (int i = grdMapping.Selection.c1; i <= grdMapping.Selection.c2; i++)
		{
			if (i != 0)
			{
				grdMapping.Rows[1][i] = string.Empty;
			}
		}
	}

	private void SpbAnalysis_ItemClick(object sender, object e)
	{
		AnalysisProject analysisProject = (AnalysisProject)e;
		(Collector as TableCollectorSummary).AnalysisProject = analysisProject;
		selectorEditor.Populate();
	}

	private void Initialize(Ledger ledger, Table table)
	{
		lblSelctorTitle.Font = new Font("微软雅黑", 10f);
		Collector = new TableCollectorBalance(ledger, table);
		selectorEditor = new SelectorEditor(this, Collector);
		pnlBalanceSelector.Controls.Add(selectorEditor.View);
		selectorEditor.View.Dock = DockStyle.Fill;
		selectorEditor.View.Rows.DefaultSize = 30;
		selectorEditor.View.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		selectorEditor.View.Paint += delegate(object s1, PaintEventArgs e1)
		{
			selectorEditor.View.DrawFormBorder(e1.Graphics);
		};
		grdMapping.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdMapping.DrawFormBorder(e1.Graphics);
		};
		spbAnalysis.ItemClick += SpbAnalysis_ItemClick;
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
		grdMapping.Resize += GrdMapping_Resize;
		pnlBalanceGrid.Height = 60;
		InitializeMapping(CollectObjectEnum.Balance);
		base.Shown += delegate
		{
			ResizeCols();
		};
	}

	private void GrdMapping_Resize(object sender, EventArgs e)
	{
		ResizeCols();
	}

	private void InitializeMapping(CollectObjectEnum collectObject)
	{
		grdMapping.Cols.Count = 1;
		grdMapping.Cols.Fixed = 1;
		grdMapping.Rows.Count = 2;
		grdMapping.Rows.Fixed = 1;
		grdMapping.Rows.DefaultSize = 30;
		grdMapping.ExtendLastCol = true;
		grdMapping.Font = new Font("微软雅黑", 9f);
		grdMapping.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdMapping.Cols[0].TextAlign = TextAlignEnum.CenterCenter;
		grdMapping[0, 0] = "采账目标列名";
		grdMapping[1, 0] = "采账来源列名";
		grdMapping.ScrollBars = ScrollBars.None;
		grdMapping.AutoResize = true;
		grdMapping.AllowSorting = AllowSortingEnum.None;
		grdMapping.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		C1.Win.C1FlexGrid.CellStyle cellStyle = grdMapping.GetCellStyle(0, 0);
		if (cellStyle == null)
		{
			cellStyle = grdMapping.Styles.Add("0");
		}
		cellStyle.Font = new Font("微软雅黑", 9f);
		grdMapping.SetCellStyle(0, 0, cellStyle);
		foreach (Auditai.Model.Column column2 in Collector.Setting.Table.Columns)
		{
			C1.Win.C1FlexGrid.Column column = grdMapping.Cols.Add();
			column.UserData = column2;
			column.TextAlign = TextAlignEnum.CenterCenter;
			grdMapping[0, column.Index] = column2.CaptionDisplay;
		}
		if (selectedAccount() == null)
		{
			grdMapping.AllowEditing = false;
		}
		else
		{
			grdMapping.AllowEditing = true;
		}
		ResizeCols();
	}

	private CollectObjectEnum selectCollectObject()
	{
		return comboCollectObject.Text.Trim() switch
		{
			"科目余额表" => CollectObjectEnum.Balance, 
			"明细账" => CollectObjectEnum.Subsidiary, 
			"月度汇总表" => CollectObjectEnum.Summary, 
			_ => CollectObjectEnum.None, 
		};
	}

	private Account selectedAccount()
	{
		return comboAccountTree.SelectedNode?.Tag as Account;
	}

	private object selectedAuxiliary()
	{
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
			comboAuxiliaryTree.SelectedIndex = -1;
			comboAuxiliaryTree.Text = string.Empty;
			return;
		}
		List<TreeNode> list = new List<TreeNode>();
		AllChildren(comboAccountTree.TreeView.Nodes, list);
		foreach (TreeNode item in list)
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

	private void SelectAuxiliaryNode(object auxiliary)
	{
		Account account = selectedAccount();
		if (account == null)
		{
			comboAuxiliaryTree.SelectedIndex = -1;
			comboAuxiliaryTree.Text = string.Empty;
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
		List<TreeNode> list = new List<TreeNode>();
		AllChildren(comboAuxiliaryTree.TreeView.Nodes, list);
		foreach (TreeNode item in list)
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

	private void PopulateMapping(Dictionary<long, string> maps)
	{
		foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)grdMapping.Cols)
		{
			if (item.UserData is Auditai.Model.Column)
			{
				grdMapping[1, item.Index] = string.Empty;
			}
		}
		if (maps == null)
		{
			return;
		}
		foreach (KeyValuePair<long, string> map in maps)
		{
			foreach (C1.Win.C1FlexGrid.Column item2 in (IEnumerable)grdMapping.Cols)
			{
				if (item2.UserData is Auditai.Model.Column { Id: { Value: var value } } && value.Equals(map.Key))
				{
					grdMapping[1, item2.Index] = map.Value;
				}
			}
		}
		ResizeCols();
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
				string text = rootAccount.Code + rootAccount.Name;
				TreeNode treeNode = comboAccountTree.TreeView.Nodes.Add(text);
				treeNode.Tag = rootAccount;
				addChildren(rootAccount, treeNode);
			}
		}
		finally
		{
			comboAccountTree.TreeView.EndUpdate();
		}
		static void addChildren(Account parent, TreeNode node)
		{
			foreach (Account child in parent.Children)
			{
				string text2 = child.Code + child.Name;
				TreeNode treeNode2 = node.Nodes.Add(text2);
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
		if (account == null)
		{
			comboAuxiliaryTree.Enabled = false;
			return;
		}
		comboAuxiliaryTree.TreeView.BeginUpdate();
		try
		{
			TrialBalanceSheet trialBalanceSheet = Collector.Setting.Ledger.GetTrialBalanceSheet(selectedStartDate(), selectedEndDate());
			AccountBalance value = trialBalanceSheet.End.FirstOrDefault((KeyValuePair<Account, AccountBalance> t) => t.Key == account).Value;
			if (value == null)
			{
				return;
			}
			foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in value.ClassBalances)
			{
				AuxiliaryClass key = classBalance.Key;
				Dictionary<AuxiliaryItem, decimal> itemBalances = classBalance.Value.ItemBalances;
				string text = key.ToString();
				TreeNode treeNode = comboAuxiliaryTree.TreeView.Nodes.Add(text);
				treeNode.Tag = Tuple.Create(account, key);
				if (comboCollectObject.SelectedIndex != 1)
				{
					continue;
				}
				foreach (KeyValuePair<AuxiliaryItem, decimal> item in itemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
				{
					AuxiliaryItem key2 = item.Key;
					string text2 = key2.ToString();
					TreeNode treeNode2 = treeNode.Nodes.Add(text2);
					treeNode2.Tag = Tuple.Create(account, key2);
				}
			}
			comboAuxiliaryTree.Enabled = comboAuxiliaryTree.TreeView.Nodes.Count > 0;
		}
		finally
		{
			comboAuxiliaryTree.TreeView.EndUpdate();
		}
	}

	private void UpdateView()
	{
		DettachEvent();
		try
		{
			switch (Collector.CollectObject)
			{
			case CollectObjectEnum.Balance:
				SelectCollectObject(CollectObjectEnum.Balance);
				spbAnalysis.Visible = false;
				SetVisibleAuxiliary(visble: false);
				break;
			case CollectObjectEnum.Subsidiary:
				SelectCollectObject(CollectObjectEnum.Subsidiary);
				spbAnalysis.Visible = false;
				SetVisibleAuxiliary(visble: true);
				break;
			case CollectObjectEnum.Summary:
				SelectCollectObject(CollectObjectEnum.Summary);
				spbAnalysis.SelectItem((Collector as TableCollectorSummary).AnalysisProject);
				spbAnalysis.Visible = true;
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
			DateTime start = Collector.Setting.Start;
			DateTime end = Collector.Setting.End;
			comboStartMonth.Text = ((start == default(DateTime)) ? "1" : start.Month.ToString());
			comboEndMonth.Text = ((end == default(DateTime)) ? "12" : end.Month.ToString());
			SelectAccountNode(Collector.Setting.Account);
			PopulateAuxiliaryTree(Collector.Setting.Account);
			SelectAuxiliaryNode(Collector.Setting.Auxiliary);
			SetComboList();
			PopulateMapping(Collector.Maps);
			grdMapping.AllowEditing = Collector.Setting.Account != null;
		}
		finally
		{
			AttachEvent();
		}
	}

	private void AttachEvent()
	{
		if (!_attachEvent)
		{
			comboCollectObject.TextChanged += comboCollectObject_TextChanged;
			comboAccountTree.SelectNodeChanged += ComboAccountTree_SelectNodeChanged;
			comboAuxiliaryTree.SelectNodeChanged += ComboAuxiliaryTree_SelectNodeChanged;
			comboStartMonth.TextChanged += ComboStartMonth_SelectedItemChanged;
			comboEndMonth.TextChanged += ComboEndMonth_SelectedItemChanged;
			grdMapping.KeyDown += GrdMapping_KeyDown;
			_attachEvent = true;
		}
	}

	private void DettachEvent()
	{
		if (_attachEvent)
		{
			comboCollectObject.TextChanged -= comboCollectObject_TextChanged;
			comboAccountTree.SelectNodeChanged -= ComboAccountTree_SelectNodeChanged;
			comboAuxiliaryTree.SelectNodeChanged -= ComboAuxiliaryTree_SelectNodeChanged;
			comboStartMonth.TextChanged -= ComboStartMonth_SelectedItemChanged;
			comboEndMonth.TextChanged -= ComboEndMonth_SelectedItemChanged;
			grdMapping.KeyDown -= GrdMapping_KeyDown;
			_attachEvent = false;
		}
	}

	private void SetComboList()
	{
		CollectObjectEnum collectObjectEnum = selectCollectObject();
		string text = getComboList(collectObjectEnum);
		InitializeMapping(collectObjectEnum);
		for (int i = 1; i < grdMapping.Cols.Count; i++)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdMapping.GetCellStyle(1, i);
			if (cellStyle == null)
			{
				cellStyle = grdMapping.Styles.Add("combo");
			}
			if (cellStyle.ComboList != text)
			{
				cellStyle.ComboList = text;
			}
			grdMapping.SetCellStyle(1, i, cellStyle);
		}
		grdMapping.AllowEditing = selectedAccount() != null;
		ResizeCols();
		string getComboList(CollectObjectEnum _object)
		{
			string result = string.Empty;
			switch (_object)
			{
			case CollectObjectEnum.Balance:
				result = ((selectedAuxiliary() != null) ? string.Join("|", CaptionDic.AuxBalance.Captions) : string.Join("|", CaptionDic.Balance.Captions));
				break;
			case CollectObjectEnum.Subsidiary:
				result = string.Join("|", CaptionDic.Subsidiary.Captions);
				break;
			case CollectObjectEnum.Summary:
				result = string.Join("|", CaptionDic.Summary.Captions);
				break;
			}
			return result;
		}
	}

	private void AllChildren(TreeNodeCollection Nodes, List<TreeNode> list)
	{
		foreach (TreeNode Node in Nodes)
		{
			list.Add(Node);
			AllChildren(Node.Nodes, list);
		}
	}

	private void ResizeCols()
	{
		if (grdMapping.Cols.Count <= 9)
		{
			grdMapping.AutoSizeCol(0, 10);
			int num = grdMapping.Cols[0].Width;
			int num2 = grdMapping.Cols.Count - 1;
			int num3 = (base.Width - num - 10) / num2;
			for (int i = 1; i < grdMapping.Cols.Count - 1; i++)
			{
				grdMapping.Cols[i].Width = num3;
			}
		}
		else
		{
			grdMapping.AutoSizeCol(0, 10);
			for (int j = 0; j < grdMapping.Cols.Count; j++)
			{
				grdMapping.AutoSizeCol(j, 10);
			}
		}
	}

	private void SetVisibleAuxiliary(bool visble)
	{
		lblAuxiliary.Visible = visble;
		comboAuxiliaryTree.Visible = visble;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Controls.frmTableCollect));
		this.c1ContextMenu1 = new C1.Win.C1Command.C1ContextMenu();
		this.c1CommandLink1 = new C1.Win.C1Command.C1CommandLink();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlBottomButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.spbAnalysis = new Auditai.UI.Controls.C1SplitButtonEx();
		this.dropDownItem1 = new C1.Win.C1Input.DropDownItem();
		this.dropDownItem2 = new C1.Win.C1Input.DropDownItem();
		this.dropDownItem3 = new C1.Win.C1Input.DropDownItem();
		this.btnIntelligenceFill = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.pnlDockingTab = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.DockingTab = new C1.Win.C1Command.C1DockingTab();
		this.TabPageBalance = new C1.Win.C1Command.C1DockingTabPage();
		this.ctnBlance = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlBalanceConditions = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.comboAccountTree = new Auditai.UI.Controls.ComboTree();
		this.comboAuxiliaryTree = new Auditai.UI.Controls.ComboTree();
		this.lblCollectObject = new C1.Win.C1Input.C1Label();
		this.c1Label2 = new C1.Win.C1Input.C1Label();
		this.comboEndMonth = new C1.Win.C1Input.C1ComboBox();
		this.lblKjqj = new C1.Win.C1Input.C1Label();
		this.comboStartMonth = new C1.Win.C1Input.C1ComboBox();
		this.lblAuxiliary = new C1.Win.C1Input.C1Label();
		this.comboCollectObject = new C1.Win.C1Input.C1ComboBox();
		this.lblStartMonth = new C1.Win.C1Input.C1Label();
		this.lblEndMonth = new C1.Win.C1Input.C1Label();
		this.pnlBalanceGrid = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.grdMapping = new C1.Win.C1FlexGrid.C1FlexGrid();
		this.pnlSelectorTitle = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblSelctorTitle = new C1.Win.C1Input.C1Label();
		this.pnlBalanceSelector = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlBottomButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.spbAnalysis).BeginInit();
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
		((System.ComponentModel.ISupportInitialize)this.comboAccountTree).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboAuxiliaryTree).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectObject).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboEndMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblKjqj).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboStartMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblAuxiliary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboCollectObject).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblStartMonth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblEndMonth).BeginInit();
		this.pnlBalanceGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdMapping).BeginInit();
		this.pnlSelectorTitle.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblSelctorTitle).BeginInit();
		base.SuspendLayout();
		this.c1ContextMenu1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[1] { this.c1CommandLink1 });
		this.c1ContextMenu1.Name = "c1ContextMenu1";
		this.c1ContextMenu1.ShortcutText = "";
		this.c1CommandLink1.Text = "新命令";
		this.c1CommandHolder1.Commands.Add(this.c1ContextMenu1);
		this.c1CommandHolder1.Owner = this;
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlBottomButtons);
		this.ctnAll.Panels.Add(this.pnlDockingTab);
		this.ctnAll.Size = new System.Drawing.Size(751, 472);
		this.ctnAll.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 5;
		this.ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlBottomButtons.Controls.Add(this.spbAnalysis);
		this.pnlBottomButtons.Controls.Add(this.btnIntelligenceFill);
		this.pnlBottomButtons.Controls.Add(this.btnCancel);
		this.pnlBottomButtons.Controls.Add(this.btnConfirm);
		this.pnlBottomButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlBottomButtons.Height = 60;
		this.pnlBottomButtons.KeepRelativeSize = false;
		this.pnlBottomButtons.Location = new System.Drawing.Point(0, 412);
		this.pnlBottomButtons.Name = "pnlBottomButtons";
		this.pnlBottomButtons.Resizable = false;
		this.pnlBottomButtons.Size = new System.Drawing.Size(751, 60);
		this.pnlBottomButtons.SizeRatio = 7.255;
		this.pnlBottomButtons.TabIndex = 1;
		this.spbAnalysis.Items.Add(this.dropDownItem1);
		this.spbAnalysis.Items.Add(this.dropDownItem2);
		this.spbAnalysis.Items.Add(this.dropDownItem3);
		this.spbAnalysis.Location = new System.Drawing.Point(337, 21);
		this.spbAnalysis.Name = "spbAnalysis";
		this.spbAnalysis.Size = new System.Drawing.Size(105, 26);
		this.spbAnalysis.TabIndex = 3;
		this.spbAnalysis.Text = "借方发生额";
		this.spbAnalysis.UseVisualStyleBackColor = true;
		this.dropDownItem1.Text = "科目余额";
		this.dropDownItem2.Text = "借方发生额";
		this.dropDownItem3.Text = "贷方发生额";
		this.btnIntelligenceFill.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnIntelligenceFill.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnIntelligenceFill.Location = new System.Drawing.Point(467, 21);
		this.btnIntelligenceFill.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnIntelligenceFill.Name = "btnIntelligenceFill";
		this.btnIntelligenceFill.Size = new System.Drawing.Size(70, 26);
		this.btnIntelligenceFill.TabIndex = 2;
		this.btnIntelligenceFill.Text = "智能设置";
		this.btnIntelligenceFill.UseVisualStyleBackColor = true;
		this.btnIntelligenceFill.Click += new System.EventHandler(btnIntelligenceFill_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(658, 21);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancle_Click);
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnConfirm.Location = new System.Drawing.Point(565, 21);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 0;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.pnlDockingTab.Controls.Add(this.DockingTab);
		this.pnlDockingTab.Height = 411;
		this.pnlDockingTab.Location = new System.Drawing.Point(0, 0);
		this.pnlDockingTab.Name = "pnlDockingTab";
		this.pnlDockingTab.Size = new System.Drawing.Size(751, 411);
		this.pnlDockingTab.TabIndex = 0;
		this.DockingTab.Controls.Add(this.TabPageBalance);
		this.DockingTab.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DockingTab.Location = new System.Drawing.Point(0, 0);
		this.DockingTab.Name = "DockingTab";
		this.DockingTab.ShowTabs = false;
		this.DockingTab.Size = new System.Drawing.Size(751, 411);
		this.DockingTab.TabIndex = 3;
		this.DockingTab.TabsSpacing = 5;
		this.DockingTab.TabStyle = C1.Win.C1Command.TabStyleEnum.Office2007;
		this.DockingTab.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.DockingTab.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2007Blue;
		this.TabPageBalance.CaptionText = "采账设置";
		this.TabPageBalance.Controls.Add(this.ctnBlance);
		this.TabPageBalance.Location = new System.Drawing.Point(1, 1);
		this.TabPageBalance.Name = "TabPageBalance";
		this.TabPageBalance.Size = new System.Drawing.Size(749, 409);
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
		this.ctnBlance.Panels.Add(this.pnlBalanceGrid);
		this.ctnBlance.Panels.Add(this.pnlSelectorTitle);
		this.ctnBlance.Panels.Add(this.pnlBalanceSelector);
		this.ctnBlance.Size = new System.Drawing.Size(749, 409);
		this.ctnBlance.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnBlance.SplitterWidth = 0;
		this.ctnBlance.TabIndex = 0;
		this.ctnBlance.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlBalanceConditions.Controls.Add(this.comboAccountTree);
		this.pnlBalanceConditions.Controls.Add(this.comboAuxiliaryTree);
		this.pnlBalanceConditions.Controls.Add(this.lblCollectObject);
		this.pnlBalanceConditions.Controls.Add(this.c1Label2);
		this.pnlBalanceConditions.Controls.Add(this.comboEndMonth);
		this.pnlBalanceConditions.Controls.Add(this.lblKjqj);
		this.pnlBalanceConditions.Controls.Add(this.comboStartMonth);
		this.pnlBalanceConditions.Controls.Add(this.lblAuxiliary);
		this.pnlBalanceConditions.Controls.Add(this.comboCollectObject);
		this.pnlBalanceConditions.Controls.Add(this.lblStartMonth);
		this.pnlBalanceConditions.Controls.Add(this.lblEndMonth);
		this.pnlBalanceConditions.Height = 80;
		this.pnlBalanceConditions.KeepRelativeSize = false;
		this.pnlBalanceConditions.Location = new System.Drawing.Point(0, 0);
		this.pnlBalanceConditions.Name = "pnlBalanceConditions";
		this.pnlBalanceConditions.Resizable = false;
		this.pnlBalanceConditions.Size = new System.Drawing.Size(749, 80);
		this.pnlBalanceConditions.SizeRatio = 19.608;
		this.pnlBalanceConditions.TabIndex = 0;
		this.comboAccountTree.AllowSpinLoop = false;
		this.comboAccountTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboAccountTree.DropHeight = -1;
		this.comboAccountTree.DropWidth = -1;
		this.comboAccountTree.Font = new System.Drawing.Font("Microsoft YaHei", 9f);
		this.comboAccountTree.GapHeight = 0;
		this.comboAccountTree.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboAccountTree.ItemsDisplayMember = "";
		this.comboAccountTree.ItemsValueMember = "";
		this.comboAccountTree.Location = new System.Drawing.Point(96, 46);
		this.comboAccountTree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboAccountTree.Name = "comboAccountTree";
		this.comboAccountTree.SelectedNode = null;
		this.comboAccountTree.Size = new System.Drawing.Size(233, 21);
		this.comboAccountTree.TabIndex = 18;
		this.comboAccountTree.Tag = null;
		this.comboAccountTree.TextDetached = true;
		this.comboAuxiliaryTree.AllowSpinLoop = false;
		this.comboAuxiliaryTree.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.comboAuxiliaryTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboAuxiliaryTree.DropHeight = -1;
		this.comboAuxiliaryTree.DropWidth = -1;
		this.comboAuxiliaryTree.Font = new System.Drawing.Font("Microsoft YaHei", 9f);
		this.comboAuxiliaryTree.GapHeight = 0;
		this.comboAuxiliaryTree.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboAuxiliaryTree.ItemsDisplayMember = "";
		this.comboAuxiliaryTree.ItemsValueMember = "";
		this.comboAuxiliaryTree.Location = new System.Drawing.Point(518, 46);
		this.comboAuxiliaryTree.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboAuxiliaryTree.Name = "comboAuxiliaryTree";
		this.comboAuxiliaryTree.SelectedNode = null;
		this.comboAuxiliaryTree.Size = new System.Drawing.Size(203, 21);
		this.comboAuxiliaryTree.TabIndex = 28;
		this.comboAuxiliaryTree.Tag = null;
		this.comboAuxiliaryTree.TextDetached = true;
		this.lblCollectObject.AutoSize = true;
		this.lblCollectObject.BackColor = System.Drawing.Color.Transparent;
		this.lblCollectObject.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCollectObject.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblCollectObject.ForeColor = System.Drawing.Color.Black;
		this.lblCollectObject.Location = new System.Drawing.Point(22, 17);
		this.lblCollectObject.Name = "lblCollectObject";
		this.lblCollectObject.Size = new System.Drawing.Size(68, 17);
		this.lblCollectObject.TabIndex = 19;
		this.lblCollectObject.Tag = null;
		this.lblCollectObject.Text = "采集对象：";
		this.lblCollectObject.TextDetached = true;
		this.lblCollectObject.Value = "采集对象：";
		this.c1Label2.AutoSize = true;
		this.c1Label2.BackColor = System.Drawing.Color.Transparent;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label2.ForeColor = System.Drawing.Color.Black;
		this.c1Label2.Location = new System.Drawing.Point(22, 48);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(68, 17);
		this.c1Label2.TabIndex = 20;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "科目名称：";
		this.c1Label2.TextDetached = true;
		this.c1Label2.Value = "科目名称：";
		this.comboEndMonth.AllowSpinLoop = false;
		this.comboEndMonth.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
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
		this.comboEndMonth.Location = new System.Drawing.Point(636, 15);
		this.comboEndMonth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboEndMonth.Name = "comboEndMonth";
		this.comboEndMonth.Size = new System.Drawing.Size(59, 21);
		this.comboEndMonth.TabIndex = 27;
		this.comboEndMonth.Tag = null;
		this.comboEndMonth.TextDetached = true;
		this.lblKjqj.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblKjqj.AutoSize = true;
		this.lblKjqj.BackColor = System.Drawing.Color.Transparent;
		this.lblKjqj.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblKjqj.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblKjqj.ForeColor = System.Drawing.Color.Black;
		this.lblKjqj.Location = new System.Drawing.Point(453, 17);
		this.lblKjqj.Name = "lblKjqj";
		this.lblKjqj.Size = new System.Drawing.Size(68, 17);
		this.lblKjqj.TabIndex = 21;
		this.lblKjqj.Tag = null;
		this.lblKjqj.Text = "会计期间：";
		this.lblKjqj.TextDetached = true;
		this.lblKjqj.Value = "会计期间：";
		this.comboStartMonth.AllowSpinLoop = false;
		this.comboStartMonth.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
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
		this.comboStartMonth.Location = new System.Drawing.Point(527, 15);
		this.comboStartMonth.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboStartMonth.Name = "comboStartMonth";
		this.comboStartMonth.Size = new System.Drawing.Size(72, 21);
		this.comboStartMonth.TabIndex = 26;
		this.comboStartMonth.Tag = null;
		this.comboStartMonth.TextDetached = true;
		this.lblAuxiliary.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblAuxiliary.AutoSize = true;
		this.lblAuxiliary.BackColor = System.Drawing.Color.Transparent;
		this.lblAuxiliary.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblAuxiliary.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblAuxiliary.ForeColor = System.Drawing.Color.Black;
		this.lblAuxiliary.Location = new System.Drawing.Point(453, 48);
		this.lblAuxiliary.Name = "lblAuxiliary";
		this.lblAuxiliary.Size = new System.Drawing.Size(68, 17);
		this.lblAuxiliary.TabIndex = 22;
		this.lblAuxiliary.Tag = null;
		this.lblAuxiliary.Text = "辅助核算：";
		this.lblAuxiliary.TextDetached = true;
		this.lblAuxiliary.Value = "辅助核算：";
		this.comboCollectObject.AllowSpinLoop = false;
		this.comboCollectObject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.comboCollectObject.GapHeight = 0;
		this.comboCollectObject.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboCollectObject.Items.Add("科目余额表");
		this.comboCollectObject.Items.Add("明细账");
		this.comboCollectObject.Items.Add("月度汇总表");
		this.comboCollectObject.ItemsDisplayMember = "";
		this.comboCollectObject.ItemsValueMember = "";
		this.comboCollectObject.Location = new System.Drawing.Point(96, 15);
		this.comboCollectObject.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.comboCollectObject.Name = "comboCollectObject";
		this.comboCollectObject.Size = new System.Drawing.Size(233, 21);
		this.comboCollectObject.TabIndex = 25;
		this.comboCollectObject.Tag = null;
		this.lblStartMonth.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblStartMonth.AutoSize = true;
		this.lblStartMonth.BackColor = System.Drawing.Color.Transparent;
		this.lblStartMonth.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblStartMonth.ForeColor = System.Drawing.Color.Black;
		this.lblStartMonth.Location = new System.Drawing.Point(605, 17);
		this.lblStartMonth.Name = "lblStartMonth";
		this.lblStartMonth.Size = new System.Drawing.Size(25, 17);
		this.lblStartMonth.TabIndex = 23;
		this.lblStartMonth.Tag = null;
		this.lblStartMonth.Text = "月-";
		this.lblStartMonth.TextDetached = true;
		this.lblStartMonth.Value = "月-";
		this.lblEndMonth.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblEndMonth.AutoSize = true;
		this.lblEndMonth.BackColor = System.Drawing.Color.Transparent;
		this.lblEndMonth.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblEndMonth.ForeColor = System.Drawing.Color.Black;
		this.lblEndMonth.Location = new System.Drawing.Point(707, 17);
		this.lblEndMonth.Name = "lblEndMonth";
		this.lblEndMonth.Size = new System.Drawing.Size(20, 17);
		this.lblEndMonth.TabIndex = 24;
		this.lblEndMonth.Tag = null;
		this.lblEndMonth.Text = "月";
		this.lblEndMonth.TextDetached = true;
		this.lblEndMonth.Value = "月";
		this.pnlBalanceGrid.Controls.Add(this.grdMapping);
		this.pnlBalanceGrid.Height = 60;
		this.pnlBalanceGrid.KeepRelativeSize = false;
		this.pnlBalanceGrid.Location = new System.Drawing.Point(0, 81);
		this.pnlBalanceGrid.Name = "pnlBalanceGrid";
		this.pnlBalanceGrid.Resizable = false;
		this.pnlBalanceGrid.Size = new System.Drawing.Size(749, 60);
		this.pnlBalanceGrid.SizeRatio = 18.349;
		this.pnlBalanceGrid.TabIndex = 1;
		this.grdMapping.AllowEditing = false;
		this.grdMapping.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.grdMapping.AutoResize = true;
		this.grdMapping.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdMapping.ColumnInfo = resources.GetString("grdMapping.ColumnInfo");
		this.grdMapping.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdMapping.Location = new System.Drawing.Point(0, 0);
		this.grdMapping.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdMapping.Name = "grdMapping";
		this.grdMapping.Rows.Count = 1;
		this.grdMapping.Rows.DefaultSize = 20;
		this.grdMapping.Size = new System.Drawing.Size(749, 60);
		this.grdMapping.TabIndex = 1;
		this.pnlSelectorTitle.Controls.Add(this.lblSelctorTitle);
		this.pnlSelectorTitle.Height = 30;
		this.pnlSelectorTitle.KeepRelativeSize = false;
		this.pnlSelectorTitle.Location = new System.Drawing.Point(0, 142);
		this.pnlSelectorTitle.MinHeight = 30;
		this.pnlSelectorTitle.Name = "pnlSelectorTitle";
		this.pnlSelectorTitle.Resizable = false;
		this.pnlSelectorTitle.Size = new System.Drawing.Size(749, 30);
		this.pnlSelectorTitle.SizeRatio = 12.245;
		this.pnlSelectorTitle.TabIndex = 3;
		this.lblSelctorTitle.BackColor = System.Drawing.Color.Transparent;
		this.lblSelctorTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblSelctorTitle.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblSelctorTitle.Font = new System.Drawing.Font("Microsoft YaHei", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
		this.lblSelctorTitle.ForeColor = System.Drawing.Color.Black;
		this.lblSelctorTitle.Location = new System.Drawing.Point(0, 0);
		this.lblSelctorTitle.Name = "lblSelctorTitle";
		this.lblSelctorTitle.Size = new System.Drawing.Size(749, 30);
		this.lblSelctorTitle.TabIndex = 0;
		this.lblSelctorTitle.Tag = null;
		this.lblSelctorTitle.Text = "c1Label3";
		this.lblSelctorTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblSelctorTitle.TextDetached = true;
		this.pnlBalanceSelector.Height = 236;
		this.pnlBalanceSelector.Location = new System.Drawing.Point(0, 173);
		this.pnlBalanceSelector.Name = "pnlBalanceSelector";
		this.pnlBalanceSelector.Size = new System.Drawing.Size(749, 236);
		this.pnlBalanceSelector.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(751, 472);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.MinimumSize = new System.Drawing.Size(650, 300);
		base.Name = "frmTableCollect";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "列对应采账设置";
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlBottomButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.spbAnalysis).EndInit();
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
		((System.ComponentModel.ISupportInitialize)this.comboAccountTree).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboAuxiliaryTree).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCollectObject).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboEndMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblKjqj).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboStartMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblAuxiliary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboCollectObject).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblStartMonth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblEndMonth).EndInit();
		this.pnlBalanceGrid.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdMapping).EndInit();
		this.pnlSelectorTitle.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.lblSelctorTitle).EndInit();
		base.ResumeLayout(false);
	}
}
