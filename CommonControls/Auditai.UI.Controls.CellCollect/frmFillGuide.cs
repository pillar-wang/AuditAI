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
using Auditai.Model;
using Auditai.UI.Controls.CollectCell;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls.CellCollect;

public class frmFillGuide : C1RibbonForm
{
	internal const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	internal const string FORMATSTRING_DATE = "yyyy-MM-dd";

	internal const string CN_CODE = "Code";

	internal const string CN_NAME = "Name";

	internal const string CN_ITEMCLASS = "ItemClass";

	internal const string CN_ITEMNUMBER = "ItemNumber";

	internal const string CN_ITEMNAME = "ItemName";

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

	private const string BEGINBALANCE = "BeginBalance";

	private Dictionary<int, CollectItem> collectDic;

	private Account selectedAccount;

	private object selectedAuxiliary;

	private int auditYear;

	private Form owner;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1DockingTab DockingTab;

	private C1DockingTabPage tabBalance;

	private C1Button btnCancel;

	private C1Button btnConfirm;

	private C1FlexGridEx grdBalance;

	private C1DockingTabPage tabSubsidiary;

	private C1Label c1Label2;

	private C1Label c1Label1;

	private ComboTree comboAuxiliaryTree;

	private ComboTree comboAccountTree;

	private C1SplitContainer ctnSubTab;

	private C1SplitterPanel pnlSubHeader;

	private C1SplitterPanel pnlSubTable;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlDockingTab;

	private C1SplitterPanel pnlBottomBtn;

	private C1FlexGridEx grdSubsidiary;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel c1SplitterPanel2;

	private C1Label c1Label3;

	private C1SplitterPanel c1SplitterPanel1;

	private C1SplitterPanel c1SplitterPanel3;

	private C1Label c1Label4;

	public Ledger Ledger { get; set; }

	public DateTime StartTime { get; set; }

	public DateTime EndTime { get; set; }

	public IEnumerable<CollectItem> CollectItems => collectDic.Values;

	public frmFillGuide(Ledger ledger, int auditYear, Form owner)
	{
		InitializeComponent();
		base.Shown += FrmFillGuide_Shown;
		this.owner = owner;
		this.auditYear = auditYear;
		Ledger = ledger;
		Initialize();
	}

	private void FrmFillGuide_Shown(object sender, EventArgs e)
	{
		Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.CollectFill);
	}

	public DialogResult ShowBalance()
	{
		PopulateBalance();
		collectDic.Clear();
		base.DialogResult = DialogResult.None;
		DockingTab.SelectedTab = tabBalance;
		Theme.SetCurrentTree(this);
		return ShowDialog(owner);
	}

	public DialogResult ShowSubsidiary()
	{
		SubsidiaryLedger subsidiaryLedger = null;
		if (selectedAccount != null)
		{
			object obj = selectedAuxiliary;
			subsidiaryLedger = ((obj is AuxiliaryClass auxClass) ? Ledger.GetSubsidiaryLedger(selectedAccount, StartTime, EndTime, auxClass) : ((!(obj is AuxiliaryItem auxItem)) ? Ledger.GetSubsidiaryLedger(selectedAccount, StartTime, EndTime) : Ledger.GetSubsidiaryLedger(selectedAccount, StartTime, EndTime, auxItem)));
		}
		PopulateSubsidiary(selectedAccount, subsidiaryLedger);
		collectDic.Clear();
		base.DialogResult = DialogResult.None;
		DockingTab.SelectedTab = tabSubsidiary;
		Theme.SetCurrentTree(this);
		return ShowDialog(owner);
	}

	private void Initialize()
	{
		base.AcceptButton = btnConfirm;
		DockingTab.ShowTabs = false;
		grdBalance.Rows.DefaultSize = 30;
		grdSubsidiary.Rows.DefaultSize = 30;
		base.StartPosition = FormStartPosition.CenterScreen;
		collectDic = new Dictionary<int, CollectItem>();
		grdBalance.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdBalance.DrawFormBorder(e1.Graphics);
		};
		grdSubsidiary.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdSubsidiary.DrawFormBorder(e1.Graphics);
		};
		comboAccountTree.SelectNodeChanged += ComboAccountTree_SelectNodeChanged;
		comboAuxiliaryTree.SelectNodeChanged += ComboAuxiliaryTree_SelectNodeChanged;
		grdSubsidiary.Click += grdSubsidiary_Click;
		grdBalance.Click += grdBalance_Click;
		PopulateAccountTree(comboAccountTree);
	}

	private void PopulateBalance()
	{
		grdBalance.BeginUpdate();
		try
		{
			grdBalance.Cols.Count = 0;
			grdBalance.Rows.Count = 1;
			grdBalance.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grdBalance.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "ItemClass";
			column.Caption = "辅助核算类别";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			column.Visible = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "ItemNumber";
			column.Caption = "辅助核算代码";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			column.Visible = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "ItemName";
			column.Caption = "辅助核算名称";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			column.Visible = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "BeginDC";
			column.Caption = "期初余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "BeginBalance";
			column.Caption = "期初余额";
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowEditing = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方发生额";
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowEditing = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方发生额";
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowEditing = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "EndDC";
			column.Caption = "期末余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "EndBalance";
			column.Caption = "期末余额";
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowEditing = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			grdBalance.Rows.Fixed = 1;
			grdBalance.Cols.Fixed = 1;
			TrialBalanceSheet sheet = Ledger.GetTrialBalanceSheet(StartTime, EndTime);
			grdBalance.Tree.Column = 1;
			grdBalance.AutoSizeCol(grdBalance.Tree.Column);
			int rowIndex = 1;
			foreach (Account rootAccount in Ledger.RootAccounts)
			{
				C1.Win.C1FlexGrid.Row row = grdBalance.Rows.Add();
				row.IsNode = true;
				row.UserData = rootAccount;
				Node node2 = row.Node;
				node2.Level = 0;
				node2.Data = rootAccount.Code;
				row["Index"] = rowIndex++.ToString();
				row["Code"] = rootAccount.Code;
				row["Name"] = " ".PadLeft(node2.Level * 4) + rootAccount.Name;
				row["BeginDC"] = GetDCChar(rootAccount.IsDebit, sheet.Start[rootAccount].Total);
				row["BeginBalance"] = Math.Abs(sheet.Start[rootAccount].Total);
				row["Debit"] = (sheet.Debit.ContainsKey(rootAccount) ? ((object)sheet.Debit[rootAccount].Total) : null);
				row["Credit"] = (sheet.Credit.ContainsKey(rootAccount) ? ((object)sheet.Credit[rootAccount].Total) : null);
				row["EndDC"] = GetDCChar(rootAccount.IsDebit, sheet.End[rootAccount].Total);
				row["EndBalance"] = Math.Abs(sheet.End[rootAccount].Total);
				addChildren(rootAccount, node2);
			}
			grdBalance.AllowEditing = false;
			grdBalance.AutoSizeCols();
			grdBalance.Tree.Show(0);
			void addChildren(Account account, Node node)
			{
				foreach (Account child in account.Children)
				{
					C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows.Add();
					row2.IsNode = true;
					row2.UserData = child;
					Node node3 = row2.Node;
					node3.Level = node.Level + 1;
					node3.Data = child.Code;
					row2["Index"] = rowIndex++.ToString();
					row2["Code"] = child.Code;
					row2["Name"] = " ".PadLeft(node3.Level * 4) + child.Name;
					row2["BeginDC"] = GetDCChar(child.IsDebit, sheet.Start[child].Total);
					row2["BeginBalance"] = Math.Abs(sheet.Start[child].Total);
					row2["Debit"] = (sheet.Debit.ContainsKey(child) ? ((object)sheet.Debit[child].Total) : null);
					row2["Credit"] = (sheet.Credit.ContainsKey(child) ? ((object)sheet.Credit[child].Total) : null);
					row2["EndDC"] = GetDCChar(child.IsDebit, sheet.End[child].Total);
					row2["EndBalance"] = Math.Abs(sheet.End[child].Total);
					addChildren(child, node3);
				}
			}
		}
		finally
		{
			grdBalance.EndUpdate();
		}
	}

	private void PopulateSubsidiary(Account account, SubsidiaryLedger subsidiaryLedger)
	{
		grdSubsidiary.BeginUpdate();
		try
		{
			grdSubsidiary.Cols.Count = 0;
			grdSubsidiary.Rows.Count = 1;
			grdSubsidiary.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grdSubsidiary.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Date";
			column.Caption = "日期";
			column.AllowMerging = true;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Type";
			column.Caption = "字";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Number";
			column.Caption = "号";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.AllowMerging = true;
			column.DataType = typeof(string);
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "ItemClass";
			column.Caption = "辅助核算类别";
			column.DataType = typeof(string);
			column.Visible = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "ItemNumber";
			column.Caption = "辅助核算编号";
			column.DataType = typeof(string);
			column.Visible = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "ItemName";
			column.Caption = "辅助核算名称";
			column.DataType = typeof(string);
			column.Visible = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方金额";
			column.AllowMerging = true;
			column.Format = "#,0.00;-#,0.00;#";
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方金额";
			column.AllowMerging = true;
			column.Format = "#,0.00;-#,0.00;#";
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "DC";
			column.Caption = "方向";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Balance";
			column.Caption = "余额";
			column.AllowMerging = true;
			column.Format = "#,0.00;-#,0.00;#";
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			grdSubsidiary.Rows.Fixed = 1;
			grdSubsidiary.Cols.Fixed = 1;
			grdSubsidiary.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			if (account == null || subsidiaryLedger == null)
			{
				return;
			}
			int num = 1;
			foreach (MonthSubsidiaryLedger item in subsidiaryLedger.Months.OrderBy((MonthSubsidiaryLedger t) => t.Month))
			{
				List<SubsidiaryLedgerEntry> list = item.Entries.OrderBy((SubsidiaryLedgerEntry t) => t.Voucher.Day).ThenBy((SubsidiaryLedgerEntry s) => s.Voucher.Type.Name).ThenBy((SubsidiaryLedgerEntry m) => m.Voucher.Number, StringNumberComparer.Instance)
					.ToList();
				foreach (SubsidiaryLedgerEntry item2 in list)
				{
					C1.Win.C1FlexGrid.Row row = grdSubsidiary.Rows.Add();
					row["Index"] = num++.ToString();
					row["Date"] = item2.Voucher.Day;
					row["Type"] = item2.Voucher.Type.Name;
					row["Number"] = item2.Voucher.Number;
					row["Digest"] = item2.Voucher.Digest;
					List<AuxiliaryItem> details = item2.Voucher.Details;
					if (details.Count > 0)
					{
						grdSubsidiary.Cols["ItemClass"].Visible = true;
						grdSubsidiary.Cols["ItemName"].Visible = true;
						grdSubsidiary.Cols["ItemNumber"].Visible = true;
						row["ItemClass"] = string.Join(",", details.Select((AuxiliaryItem aux) => aux.Class.Code));
						row["ItemNumber"] = string.Join(",", details.Select((AuxiliaryItem aux) => aux.Code));
						row["ItemName"] = string.Join(",", details.Select((AuxiliaryItem aux) => aux.Name));
					}
					row["Debit"] = (item2.Voucher.IsDebit ? item2.Voucher.Amount : 0m);
					row["Credit"] = (item2.Voucher.IsDebit ? 0m : item2.Voucher.Amount);
					row["DC"] = GetDCChar(account.IsDebit, item2.Balance);
					row["Balance"] = Math.Abs(item2.Balance);
					row.UserData = item2.Voucher;
				}
			}
			grdSubsidiary.AllowEditing = false;
			grdSubsidiary.AutoSizeCols();
		}
		finally
		{
			grdSubsidiary.EndUpdate();
		}
	}

	private void PopulateAccountTree(ComboTree comboTree)
	{
		comboTree.TreeView.BeginUpdate();
		try
		{
			comboTree.TreeView.Nodes.Clear();
			comboTree.SelectedIndex = -1;
			comboTree.Text = string.Empty;
			foreach (Account rootAccount in Ledger.RootAccounts)
			{
				string text = rootAccount.Code + rootAccount.Name;
				TreeNode treeNode = comboTree.TreeView.Nodes.Add(text);
				treeNode.Tag = rootAccount;
				addChildren(rootAccount, treeNode);
			}
		}
		finally
		{
			comboTree.TreeView.EndUpdate();
		}
		static void addChildren(Account account, TreeNode node)
		{
			foreach (Account child in account.Children)
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
		comboAuxiliaryTree.TreeView.BeginUpdate();
		try
		{
			comboAuxiliaryTree.TreeView.Nodes.Clear();
			comboAuxiliaryTree.Text = string.Empty;
			comboAuxiliaryTree.SelectedIndex = -1;
			TrialBalanceSheet trialBalanceSheet = Ledger.GetTrialBalanceSheet(StartTime, EndTime);
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = trialBalanceSheet.End.First((KeyValuePair<Account, AccountBalance> t) => t.Key == account).Value.ClassBalances;
			foreach (KeyValuePair<AuxiliaryClass, ClassBalance> item in classBalances)
			{
				AuxiliaryClass key = item.Key;
				string text = key.ToString();
				TreeNode treeNode = comboAuxiliaryTree.TreeView.Nodes.Add(text);
				treeNode.Tag = Tuple.Create(account, key);
				foreach (KeyValuePair<AuxiliaryItem, decimal> item2 in item.Value.ItemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
				{
					AuxiliaryItem key2 = item2.Key;
					TreeNode treeNode2 = treeNode.Nodes.Add(key2.ToString());
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

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void grdBalance_Click(object sender, EventArgs e)
	{
		int mouseRow = grdBalance.MouseRow;
		int mouseCol = grdBalance.MouseCol;
		if (mouseRow < grdBalance.Rows.Fixed || mouseCol < grdBalance.Cols.Fixed)
		{
			return;
		}
		Account account = grdBalance.Rows[mouseRow].UserData as Account;
		List<int> validCols;
		if (account != null)
		{
			validCols = new List<int>();
			validCols.Add(grdBalance.Cols["Debit"].Index);
			validCols.Add(grdBalance.Cols["Credit"].Index);
			validCols.Add(grdBalance.Cols["EndBalance"].Index);
			validCols.Add(grdBalance.Cols["BeginBalance"].Index);
			if (validCols.Contains(mouseCol))
			{
				selectCell(mouseRow, mouseCol);
			}
		}
		void selectCell(int r, int c)
		{
			if (collectDic.ContainsKey(r))
			{
				collectDic.Remove(r);
			}
			foreach (int item in validCols)
			{
				if (item == c)
				{
					BalanceItem balanceItem = new BalanceItem
					{
						AccountCode = account.Code,
						StartTime = StartTime,
						EndTime = EndTime
					};
					if (c == grdBalance.Cols["BeginBalance"].Index)
					{
						balanceItem.AmountEnum = (account.IsDebit ? AmountEnum.DebitBegin : AmountEnum.CreditBegin);
					}
					else if (c == grdBalance.Cols["Debit"].Index)
					{
						balanceItem.AmountEnum = AmountEnum.DebitAmount;
					}
					else if (c == grdBalance.Cols["Credit"].Index)
					{
						balanceItem.AmountEnum = AmountEnum.CreditAmount;
					}
					else
					{
						if (c != grdBalance.Cols["EndBalance"].Index)
						{
							continue;
						}
						balanceItem.AmountEnum = (account.IsDebit ? AmountEnum.DebitBalance : AmountEnum.CreditBalance);
					}
					object userData = grdBalance.GetUserData(r, item);
					if (userData == null)
					{
						grdBalance.SetUserData(r, item, OperateEnum.Add);
						grdBalance.SetCellImage(r, item, Resource1.Add);
						balanceItem.Operation = OperateEnum.Add;
						collectDic.Add(r, balanceItem);
					}
					else if ((OperateEnum)userData == OperateEnum.Add)
					{
						grdBalance.SetUserData(r, item, OperateEnum.Subtract);
						grdBalance.SetCellImage(r, item, Resource1.Substract);
						balanceItem.Operation = OperateEnum.Subtract;
						collectDic.Add(r, balanceItem);
					}
					else if ((OperateEnum)userData == OperateEnum.Subtract)
					{
						grdBalance.SetUserData(r, item, null);
						grdBalance.SetCellImage(r, item, null);
					}
				}
				else
				{
					grdBalance.SetUserData(r, item, null);
					grdBalance.SetCellImage(r, item, null);
				}
			}
			grdBalance.Update();
		}
	}

	private void grdSubsidiary_Click(object sender, EventArgs e)
	{
		int mouseRow = grdSubsidiary.MouseRow;
		int mouseCol = grdSubsidiary.MouseCol;
		if (mouseRow < grdSubsidiary.Rows.Fixed || mouseCol < grdSubsidiary.Cols.Fixed)
		{
			return;
		}
		Voucher voucher = grdSubsidiary.Rows[mouseRow].UserData as Voucher;
		List<int> validCols;
		if (voucher != null)
		{
			validCols = new List<int>();
			int index = grdSubsidiary.Cols["Debit"].Index;
			int index2 = grdSubsidiary.Cols["Credit"].Index;
			validCols.Add(index);
			validCols.Add(index2);
			if ((mouseCol == index && voucher.IsDebit) || (mouseCol == index2 && !voucher.IsDebit))
			{
				selectCell(mouseRow, mouseCol);
			}
		}
		void selectCell(int r, int c)
		{
			if (collectDic.ContainsKey(r))
			{
				collectDic.Remove(r);
			}
			foreach (int item in validCols)
			{
				if (item == c)
				{
					SubsidiaryItem subsidiaryItem = new SubsidiaryItem
					{
						AccountCode = voucher.Account.Code,
						StartTime = StartTime,
						EndTime = EndTime,
						TypeNumber = voucher.Type.Name + voucher.Number,
						Index = 0
					};
					object userData = grdSubsidiary.GetUserData(r, item);
					if (userData == null)
					{
						grdSubsidiary.SetUserData(r, item, OperateEnum.Add);
						grdSubsidiary.SetCellImage(r, item, Resource1.Add);
						subsidiaryItem.Operation = OperateEnum.Add;
						collectDic.Add(r, subsidiaryItem);
					}
					else if ((OperateEnum)userData == OperateEnum.Add)
					{
						grdSubsidiary.SetUserData(r, item, OperateEnum.Subtract);
						grdSubsidiary.SetCellImage(r, item, Resource1.Substract);
						subsidiaryItem.Operation = OperateEnum.Subtract;
						collectDic.Add(r, subsidiaryItem);
					}
					else if ((OperateEnum)userData == OperateEnum.Subtract)
					{
						grdSubsidiary.SetUserData(r, item, null);
						grdSubsidiary.SetCellImage(r, item, null);
					}
				}
				else
				{
					grdSubsidiary.SetUserData(r, item, null);
					grdSubsidiary.SetCellImage(r, item, null);
				}
			}
			grdSubsidiary.Update();
		}
	}

	private void ComboAccountTree_SelectNodeChanged(object sender, TreeViewEventArgs e)
	{
		if (comboAccountTree.SelectedNode.Tag is Account account)
		{
			collectDic.Clear();
			selectedAccount = account;
			selectedAuxiliary = null;
			PopulateAuxiliaryTree(account);
			PopulateSubsidiary(account, Ledger.GetSubsidiaryLedger(account, StartTime, EndTime));
		}
	}

	private void ComboAuxiliaryTree_SelectNodeChanged(object sender, TreeViewEventArgs e)
	{
		if (comboAuxiliaryTree.SelectedNode?.Tag is Tuple<Account, AuxiliaryClass> tuple)
		{
			collectDic.Clear();
			Account account = (selectedAccount = tuple.Item1);
			AuxiliaryClass auxClass = (AuxiliaryClass)(selectedAuxiliary = tuple.Item2);
			PopulateSubsidiary(account, Ledger.GetSubsidiaryLedger(account, StartTime, EndTime, auxClass));
		}
		else if (comboAuxiliaryTree.SelectedNode?.Tag is Tuple<Account, AuxiliaryItem> tuple2)
		{
			collectDic.Clear();
			Account account2 = (selectedAccount = tuple2.Item1);
			AuxiliaryItem auxItem = (AuxiliaryItem)(selectedAuxiliary = tuple2.Item2);
			PopulateSubsidiary(tuple2.Item1, Ledger.GetSubsidiaryLedger(account2, StartTime, EndTime, auxItem));
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
		this.DockingTab = new C1.Win.C1Command.C1DockingTab();
		this.tabBalance = new C1.Win.C1Command.C1DockingTabPage();
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.c1SplitterPanel2 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1Label3 = new C1.Win.C1Input.C1Label();
		this.c1SplitterPanel1 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.grdBalance = new Auditai.UI.Controls.C1FlexGridEx();
		this.tabSubsidiary = new C1.Win.C1Command.C1DockingTabPage();
		this.ctnSubTab = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.c1SplitterPanel3 = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1Label4 = new C1.Win.C1Input.C1Label();
		this.pnlSubHeader = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1Label2 = new C1.Win.C1Input.C1Label();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.comboAuxiliaryTree = new Auditai.UI.Controls.ComboTree();
		this.comboAccountTree = new Auditai.UI.Controls.ComboTree();
		this.pnlSubTable = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.grdSubsidiary = new Auditai.UI.Controls.C1FlexGridEx();
		this.btnConfirm = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlBottomBtn = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlDockingTab = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.DockingTab).BeginInit();
		this.DockingTab.SuspendLayout();
		this.tabBalance.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.c1SplitterPanel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label3).BeginInit();
		this.c1SplitterPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdBalance).BeginInit();
		this.tabSubsidiary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ctnSubTab).BeginInit();
		this.ctnSubTab.SuspendLayout();
		this.c1SplitterPanel3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label4).BeginInit();
		this.pnlSubHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboAuxiliaryTree).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.comboAccountTree).BeginInit();
		this.pnlSubTable.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdSubsidiary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlBottomBtn.SuspendLayout();
		this.pnlDockingTab.SuspendLayout();
		base.SuspendLayout();
		this.DockingTab.Controls.Add(this.tabBalance);
		this.DockingTab.Controls.Add(this.tabSubsidiary);
		this.DockingTab.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DockingTab.Location = new System.Drawing.Point(0, 0);
		this.DockingTab.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		this.DockingTab.Name = "DockingTab";
		this.DockingTab.Size = new System.Drawing.Size(951, 523);
		this.DockingTab.TabIndex = 0;
		this.DockingTab.TabsSpacing = 0;
		this.DockingTab.TabStyle = C1.Win.C1Command.TabStyleEnum.WindowsXP;
		this.DockingTab.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.DockingTab.VisualStyleBase = C1.Win.C1Command.VisualStyle.WindowsXP;
		this.tabBalance.Controls.Add(this.c1SplitContainer1);
		this.tabBalance.Location = new System.Drawing.Point(2, 28);
		this.tabBalance.Name = "tabBalance";
		this.tabBalance.Size = new System.Drawing.Size(945, 491);
		this.tabBalance.TabIndex = 0;
		this.tabBalance.Text = "第1页";
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.c1SplitContainer1.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.c1SplitterPanel2);
		this.c1SplitContainer1.Panels.Add(this.c1SplitterPanel1);
		this.c1SplitContainer1.Size = new System.Drawing.Size(945, 491);
		this.c1SplitContainer1.TabIndex = 1;
		this.c1SplitterPanel2.Controls.Add(this.c1Label3);
		this.c1SplitterPanel2.Height = 25;
		this.c1SplitterPanel2.KeepRelativeSize = false;
		this.c1SplitterPanel2.Location = new System.Drawing.Point(0, 0);
		this.c1SplitterPanel2.MinHeight = 20;
		this.c1SplitterPanel2.Name = "c1SplitterPanel2";
		this.c1SplitterPanel2.Resizable = false;
		this.c1SplitterPanel2.Size = new System.Drawing.Size(945, 25);
		this.c1SplitterPanel2.TabIndex = 1;
		this.c1Label3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1Label3.Location = new System.Drawing.Point(0, 0);
		this.c1Label3.Name = "c1Label3";
		this.c1Label3.Size = new System.Drawing.Size(945, 25);
		this.c1Label3.TabIndex = 0;
		this.c1Label3.Tag = null;
		this.c1Label3.Text = "科目余额表";
		this.c1Label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.c1Label3.TextDetached = true;
		this.c1SplitterPanel1.Controls.Add(this.grdBalance);
		this.c1SplitterPanel1.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.c1SplitterPanel1.Height = 465;
		this.c1SplitterPanel1.Location = new System.Drawing.Point(0, 26);
		this.c1SplitterPanel1.Name = "c1SplitterPanel1";
		this.c1SplitterPanel1.Size = new System.Drawing.Size(945, 465);
		this.c1SplitterPanel1.TabIndex = 0;
		this.grdBalance.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdBalance.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdBalance.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdBalance.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.grdBalance.Location = new System.Drawing.Point(0, 0);
		this.grdBalance.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		this.grdBalance.Name = "grdBalance";
		this.grdBalance.Rows.Count = 0;
		this.grdBalance.Rows.DefaultSize = 20;
		this.grdBalance.Rows.Fixed = 0;
		this.grdBalance.Size = new System.Drawing.Size(945, 465);
		this.grdBalance.TabIndex = 0;
		this.tabSubsidiary.Controls.Add(this.ctnSubTab);
		this.tabSubsidiary.Location = new System.Drawing.Point(2, 28);
		this.tabSubsidiary.Name = "tabSubsidiary";
		this.tabSubsidiary.Size = new System.Drawing.Size(945, 491);
		this.tabSubsidiary.TabIndex = 1;
		this.tabSubsidiary.Text = "第2页";
		this.ctnSubTab.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnSubTab.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnSubTab.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnSubTab.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnSubTab.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnSubTab.HeaderHeight = 27;
		this.ctnSubTab.Location = new System.Drawing.Point(0, 0);
		this.ctnSubTab.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		this.ctnSubTab.Name = "ctnSubTab";
		this.ctnSubTab.Panels.Add(this.c1SplitterPanel3);
		this.ctnSubTab.Panels.Add(this.pnlSubHeader);
		this.ctnSubTab.Panels.Add(this.pnlSubTable);
		this.ctnSubTab.Size = new System.Drawing.Size(945, 491);
		this.ctnSubTab.SplitterWidth = 5;
		this.ctnSubTab.TabIndex = 5;
		this.c1SplitterPanel3.Controls.Add(this.c1Label4);
		this.c1SplitterPanel3.Height = 25;
		this.c1SplitterPanel3.KeepRelativeSize = false;
		this.c1SplitterPanel3.Location = new System.Drawing.Point(0, 0);
		this.c1SplitterPanel3.MinHeight = 20;
		this.c1SplitterPanel3.Name = "c1SplitterPanel3";
		this.c1SplitterPanel3.Resizable = false;
		this.c1SplitterPanel3.Size = new System.Drawing.Size(945, 25);
		this.c1SplitterPanel3.TabIndex = 2;
		this.c1Label4.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label4.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1Label4.Location = new System.Drawing.Point(0, 0);
		this.c1Label4.Name = "c1Label4";
		this.c1Label4.Size = new System.Drawing.Size(945, 25);
		this.c1Label4.TabIndex = 0;
		this.c1Label4.Tag = null;
		this.c1Label4.Text = "明细账";
		this.c1Label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.c1Label4.TextDetached = true;
		this.pnlSubHeader.Controls.Add(this.c1Label2);
		this.pnlSubHeader.Controls.Add(this.c1Label1);
		this.pnlSubHeader.Controls.Add(this.comboAuxiliaryTree);
		this.pnlSubHeader.Controls.Add(this.comboAccountTree);
		this.pnlSubHeader.Height = 52;
		this.pnlSubHeader.KeepRelativeSize = false;
		this.pnlSubHeader.Location = new System.Drawing.Point(0, 26);
		this.pnlSubHeader.MinHeight = 52;
		this.pnlSubHeader.MinWidth = 52;
		this.pnlSubHeader.Name = "pnlSubHeader";
		this.pnlSubHeader.Resizable = false;
		this.pnlSubHeader.Size = new System.Drawing.Size(945, 52);
		this.pnlSubHeader.SizeRatio = 10.421;
		this.pnlSubHeader.TabIndex = 0;
		this.pnlSubHeader.Width = 945;
		this.c1Label2.AutoSize = true;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label2.Location = new System.Drawing.Point(267, 14);
		this.c1Label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(68, 17);
		this.c1Label2.TabIndex = 4;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "辅助核算：";
		this.c1Label2.TextDetached = true;
		this.c1Label1.AutoSize = true;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.Location = new System.Drawing.Point(17, 13);
		this.c1Label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(68, 17);
		this.c1Label1.TabIndex = 3;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "科目名称：";
		this.c1Label1.TextDetached = true;
		this.comboAuxiliaryTree.AllowSpinLoop = false;
		this.comboAuxiliaryTree.DropHeight = -1;
		this.comboAuxiliaryTree.DropWidth = -1;
		this.comboAuxiliaryTree.Enabled = false;
		this.comboAuxiliaryTree.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.comboAuxiliaryTree.GapHeight = 0;
		this.comboAuxiliaryTree.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboAuxiliaryTree.ItemsDisplayMember = "";
		this.comboAuxiliaryTree.ItemsValueMember = "";
		this.comboAuxiliaryTree.Location = new System.Drawing.Point(343, 12);
		this.comboAuxiliaryTree.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
		this.comboAuxiliaryTree.Name = "comboAuxiliaryTree";
		this.comboAuxiliaryTree.SelectedNode = null;
		this.comboAuxiliaryTree.Size = new System.Drawing.Size(120, 21);
		this.comboAuxiliaryTree.TabIndex = 2;
		this.comboAuxiliaryTree.Tag = null;
		this.comboAuxiliaryTree.TextDetached = true;
		this.comboAccountTree.AllowSpinLoop = false;
		this.comboAccountTree.DropHeight = -1;
		this.comboAccountTree.DropWidth = -1;
		this.comboAccountTree.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.comboAccountTree.GapHeight = 0;
		this.comboAccountTree.ImagePadding = new System.Windows.Forms.Padding(0);
		this.comboAccountTree.ItemsDisplayMember = "";
		this.comboAccountTree.ItemsValueMember = "";
		this.comboAccountTree.Location = new System.Drawing.Point(93, 12);
		this.comboAccountTree.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
		this.comboAccountTree.Name = "comboAccountTree";
		this.comboAccountTree.SelectedNode = null;
		this.comboAccountTree.Size = new System.Drawing.Size(120, 21);
		this.comboAccountTree.TabIndex = 1;
		this.comboAccountTree.Tag = null;
		this.comboAccountTree.TextDetached = true;
		this.pnlSubTable.Controls.Add(this.grdSubsidiary);
		this.pnlSubTable.Height = 412;
		this.pnlSubTable.Location = new System.Drawing.Point(0, 79);
		this.pnlSubTable.MinHeight = 52;
		this.pnlSubTable.MinWidth = 52;
		this.pnlSubTable.Name = "pnlSubTable";
		this.pnlSubTable.Size = new System.Drawing.Size(945, 412);
		this.pnlSubTable.TabIndex = 1;
		this.pnlSubTable.Width = 945;
		this.grdSubsidiary.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdSubsidiary.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdSubsidiary.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdSubsidiary.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.grdSubsidiary.Location = new System.Drawing.Point(0, 0);
		this.grdSubsidiary.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		this.grdSubsidiary.Name = "grdSubsidiary";
		this.grdSubsidiary.Rows.Count = 0;
		this.grdSubsidiary.Rows.DefaultSize = 20;
		this.grdSubsidiary.Rows.Fixed = 0;
		this.grdSubsidiary.Size = new System.Drawing.Size(945, 412);
		this.grdSubsidiary.TabIndex = 1;
		this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnConfirm.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnConfirm.Location = new System.Drawing.Point(715, 21);
		this.btnConfirm.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		this.btnConfirm.Name = "btnConfirm";
		this.btnConfirm.Size = new System.Drawing.Size(70, 26);
		this.btnConfirm.TabIndex = 1;
		this.btnConfirm.Text = "确定";
		this.btnConfirm.UseVisualStyleBackColor = true;
		this.btnConfirm.Click += new System.EventHandler(btnConfirm_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(840, 21);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.HeaderHeight = 27;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlBottomBtn);
		this.ctnAll.Panels.Add(this.pnlDockingTab);
		this.ctnAll.Size = new System.Drawing.Size(951, 584);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 3;
		this.pnlBottomBtn.Controls.Add(this.btnConfirm);
		this.pnlBottomBtn.Controls.Add(this.btnCancel);
		this.pnlBottomBtn.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlBottomBtn.Height = 60;
		this.pnlBottomBtn.KeepRelativeSize = false;
		this.pnlBottomBtn.Location = new System.Drawing.Point(0, 524);
		this.pnlBottomBtn.MinHeight = 60;
		this.pnlBottomBtn.MinWidth = 52;
		this.pnlBottomBtn.Name = "pnlBottomBtn";
		this.pnlBottomBtn.Resizable = false;
		this.pnlBottomBtn.Size = new System.Drawing.Size(951, 60);
		this.pnlBottomBtn.TabIndex = 1;
		this.pnlBottomBtn.Width = 951;
		this.pnlDockingTab.Controls.Add(this.DockingTab);
		this.pnlDockingTab.Height = 523;
		this.pnlDockingTab.Location = new System.Drawing.Point(0, 0);
		this.pnlDockingTab.MinHeight = 52;
		this.pnlDockingTab.MinWidth = 52;
		this.pnlDockingTab.Name = "pnlDockingTab";
		this.pnlDockingTab.Size = new System.Drawing.Size(951, 523);
		this.pnlDockingTab.SizeRatio = 100.0;
		this.pnlDockingTab.TabIndex = 0;
		this.pnlDockingTab.Width = 951;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(951, 584);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
		base.Name = "frmFillGuide";
		base.ShowInTaskbar = false;
		this.Text = "填充向导";
		((System.ComponentModel.ISupportInitialize)this.DockingTab).EndInit();
		this.DockingTab.ResumeLayout(false);
		this.tabBalance.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.c1SplitterPanel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1Label3).EndInit();
		this.c1SplitterPanel1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdBalance).EndInit();
		this.tabSubsidiary.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ctnSubTab).EndInit();
		this.ctnSubTab.ResumeLayout(false);
		this.c1SplitterPanel3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1Label4).EndInit();
		this.pnlSubHeader.ResumeLayout(false);
		this.pnlSubHeader.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboAuxiliaryTree).EndInit();
		((System.ComponentModel.ISupportInitialize)this.comboAccountTree).EndInit();
		this.pnlSubTable.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdSubsidiary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnConfirm).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlBottomBtn.ResumeLayout(false);
		this.pnlDockingTab.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
