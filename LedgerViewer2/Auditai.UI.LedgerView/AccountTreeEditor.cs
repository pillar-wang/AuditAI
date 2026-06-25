using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Controls.CollectTable;
using Auditai.UI.LedgerView.Properties;

namespace Auditai.UI.LedgerView;

public class AccountTreeEditor : ISetTheme
{
	private LedgerViewer _owner;

	private C1CommandLink lnkExpandAll = new C1CommandLink();

	private C1Command cmdExpandAll = new C1Command();

	private C1CommandLink lnkCollaspeAll = new C1CommandLink();

	private C1Command cmdCollaspeAll = new C1Command();

	private C1CommandLink lnkCancelSelect = new C1CommandLink();

	private C1Command cmdCancelSelect = new C1Command();

	private C1CommandLink lnkShowEmptyAccount = new C1CommandLink();

	private C1Command cmdShowEmptyAccount = new C1Command();

	private C1CommandLink lnkHideEmptyAccount = new C1CommandLink();

	private C1Command cmdHideEmptyAccount = new C1Command();

	private C1CommandLink lnkModifyAccount = new C1CommandLink();

	private C1Command cmdModifyAccount = new C1Command();

	private C1CommandLink lnkAddAccount = new C1CommandLink();

	private C1Command cmdAddAccount = new C1Command();

	private C1CommandLink lnkDeleteAccount = new C1CommandLink();

	private C1Command cmdDeleteAccount = new C1Command();

	private C1Command cmdShowAllAux = new C1Command();

	private C1CommandLink lnkShowAllAux = new C1CommandLink();

	private C1Command cmdHideAllAux = new C1Command();

	private C1CommandLink lnkHideAllAux = new C1CommandLink();

	private C1ContextMenu mnuTree = new C1ContextMenu();

	private int _mouseRow = -1;

	private readonly SolidBrush _brushHoverBackground = new SolidBrush(Color.Transparent);

	private C1.Win.C1FlexGrid.Row _currentOpenedRow;

	private System.Drawing.Image zb1Image = Resources.zb1;

	private System.Drawing.Image zb2Image = Resources.zb2;

	private System.Drawing.Image zb3Image = Resources.zb3;

	private System.Drawing.Image zb4Image = Resources.zb4;

	public Ledger Ledger => _owner.Ledger;

	public C1FlexGridEx Tree { get; private set; }

	public C1.Win.C1FlexGrid.Row CurrentOpendedRow
	{
		get
		{
			return _currentOpenedRow;
		}
		set
		{
			CloseNode(_currentOpenedRow);
			_currentOpenedRow = value;
			OpenNode(value);
			if (_owner.lazySubsidiaryEditor.IsValueCreated)
			{
				if (_currentOpenedRow != null)
				{
					_owner.SubsidiaryEditor.AddVisitHistory(_currentOpenedRow.UserData);
				}
				_owner.SubsidiaryEditor.currentUserData = _currentOpenedRow?.UserData;
			}
		}
	}

	public bool PendingAllEvent { get; set; }

	public bool DisplayEmptyAccount { get; set; }

	public List<Node> SelectedStructureNodes { get; private set; } = new List<Node>();


	public List<Node> SelectedTrendencyNodes { get; private set; } = new List<Node>();


	public HashSet<Account> Invalidate { get; private set; } = new HashSet<Account>();


	public FlickerManager FlickerManager { get; private set; } = new FlickerManager();


	public event EventHandler<List<Node>> CancelAll;

	public event EventHandler<bool> ShowEmptyAccount;

	public event EventHandler<bool> HideEmptyAccount;

	public event EventHandler<Tuple<Account, AuxiliaryClass>> ShowAuxiliaryNode;

	public event EventHandler<Tuple<Account, AuxiliaryClass>> HideAuxiliaryNode;

	public void OnBalanceTreeCancelAll(object sender, EventArgs e)
	{
		switch (_owner.CurrentView)
		{
		case ActiveView.TrendChart:
			Common.SetTreeCheck(Tree, CheckEnum.Unchecked);
			SelectedTrendencyNodes.Clear();
			this.CancelAll?.Invoke(Tree, SelectedTrendencyNodes);
			break;
		case ActiveView.PieChart:
			Common.SetTreeCheck(Tree, CheckEnum.Unchecked);
			SelectedStructureNodes.Clear();
			this.CancelAll?.Invoke(Tree, SelectedStructureNodes);
			break;
		}
	}

	public void OnBalanceTreeShowEmptyAccount(object sender, EventArgs e)
	{
		DisplayEmptyAccount = true;
		PopulateAccountTree(Ledger, DisplayEmptyAccount);
		this.ShowEmptyAccount?.Invoke(this, DisplayEmptyAccount);
		_owner.TriggerLedgerDataChangeEvent();
	}

	protected void OnBalanceTreeHideEmptyAccount(object sender, EventArgs e)
	{
		DisplayEmptyAccount = false;
		PopulateAccountTree(Ledger, DisplayEmptyAccount);
		this.HideEmptyAccount?.Invoke(sender, DisplayEmptyAccount);
		_owner.TriggerLedgerDataChangeEvent();
	}

	public void OnBalanceTreeShowAuxiliaryNode(Node node, AuxiliaryClass auxClass)
	{
		Tree.BeginUpdate();
		try
		{
			if (auxClass != null)
			{
				if (node.Key is Account item)
				{
					RemoveAuxiliaryItems(node);
					AppendAuxiliaryItems(node, auxClass);
					this.ShowAuxiliaryNode?.Invoke(this, Tuple.Create(item, auxClass));
				}
				else if (node.Key is Tuple<Account, AuxiliaryItem> tuple)
				{
					Node parent = node.Parent;
					RemoveAuxiliaryItems(parent);
					AppendAuxiliaryItems(parent, auxClass);
					this.ShowAuxiliaryNode?.Invoke(this, Tuple.Create(tuple.Item1, auxClass));
				}
			}
		}
		catch
		{
		}
		finally
		{
			Tree.EndUpdate();
		}
	}

	public void OnBalanceTreeHideAuxiliaryNode(object sender, EventArgs e)
	{
		object userData = ((C1Command)sender).UserData;
		if (userData is AuxiliaryClass auxiliaryClass)
		{
			Node node = Tree.Rows[Tree.Row].Node;
			if (node.Key is Tuple<Account, AuxiliaryItem>)
			{
				node = node.Parent;
			}
			if (RemoveAuxiliaryItems(node, auxiliaryClass))
			{
				this.HideAuxiliaryNode?.Invoke(sender, Tuple.Create(node.Key as Account, auxiliaryClass));
			}
		}
	}

	public Dictionary<Account, AuxiliaryClass> GetAccountExpandingAuxClass()
	{
		Dictionary<Account, AuxiliaryClass> dictionary = new Dictionary<Account, AuxiliaryClass>();
		for (int i = 0; i < Tree.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = Tree.Rows[i];
			if (row.UserData is Tuple<Account, AuxiliaryItem> tuple && !dictionary.ContainsKey(tuple.Item1))
			{
				dictionary.Add(tuple.Item1, tuple.Item2.Class);
			}
		}
		return dictionary;
	}

	internal AccountTreeEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitializeComponent();
		BindBalanceTreeContexMenu();
	}

	public void PopulateAccountTree(Ledger ledger, bool displayEmpty, int showLayer = 0)
	{
		FlickerManager.Clear();
		Invalidate.Clear();
		Tree.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			Tree.Rows.Count = 0;
			Tree.Tree.Column = 0;
			Tree.Rows.DefaultSize = 30;
			Tree.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			TrialBalanceSheet sheet = _owner.CacheManager.GetTrialBalanceSheetWithCache(ledger);
			foreach (Account item in ledger.RootAccounts.OrderBy((Account a) => a.Code))
			{
				AddChildren(item, null);
			}
			Tree.Tree.Show(showLayer);
			if (Tree.Cols.Count > 0)
			{
				Tree.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
			}
			ValidateAccountTree(ledger);
			void AddChildren(Account account, Node parentNode)
			{
				if (!displayEmpty && _owner.CacheManager.IsEmptyAccountWithCache(account))
				{
					return;
				}
				Node node = ((parentNode != null) ? parentNode.AddNode(NodeTypeEnum.LastChild, account.Code + " " + account.Name) : Tree.Rows.AddNode(0));
				node.Data = account.Code + " " + account.Name;
				node.Key = account;
				node.Image = GetNodeImage(isAccountNode: true, isOpenedState: false);
				Dictionary<AuxiliaryClass, ClassBalance> classBalances = sheet.End[account].ClassBalances;
				if (classBalances.Count > 0)
				{
					AuxiliaryClass firstOrDefaultAuxiliary = TableCollectorAbstract.GetFirstOrDefaultAuxiliary(ledger, account, sheet);
					AppendAuxiliaryItems(node, firstOrDefaultAuxiliary);
				}
				foreach (Account child in account.Children)
				{
					AddChildren(child, node);
				}
			}
		}
		catch
		{
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			Tree.EndUpdate();
		}
	}

	private void ValidateAccountTree(Ledger ledger)
	{
		ValidateAccount validateAccount = new ValidateAccount(DictionarySync.CellCollector);
		List<Account> accounts = validateAccount.Validate(ledger);
		DealInvalidateAccounts(accounts);
	}

	public void SwitchStructure(Account currentAcc)
	{
		Tree.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			Common.SetTreeCheck(Tree, CheckEnum.Unchecked);
			if (SelectedStructureNodes.Count == 0 && currentAcc != null)
			{
				foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)Tree.Rows)
				{
					if (currentAcc.Equals(item.UserData))
					{
						SelectedStructureNodes.Add(item.Node);
						break;
					}
				}
			}
			foreach (Node selectedStructureNode in SelectedStructureNodes)
			{
				selectedStructureNode.Checked = CheckEnum.Checked;
				CheckChildrens(selectedStructureNode, CheckEnum.Checked);
			}
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			Tree.EndUpdate();
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

	public void SwitchTrendency(Account currentAcc)
	{
		Tree.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			Common.SetTreeCheck(Tree, CheckEnum.Unchecked);
			if (SelectedTrendencyNodes.Count == 0 && currentAcc != null)
			{
				foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)Tree.Rows)
				{
					if (currentAcc.Equals(item.UserData))
					{
						SelectedTrendencyNodes.Add(item.Node);
						break;
					}
				}
			}
			foreach (Node selectedTrendencyNode in SelectedTrendencyNodes)
			{
				Tree.SetCellCheck(selectedTrendencyNode.Row.Index, 0, CheckEnum.Checked);
			}
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			Tree.EndUpdate();
		}
	}

	public void UpdateNodeStatus(object userdata)
	{
		if (userdata == null)
		{
			CurrentOpendedRow = null;
			return;
		}
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)Tree.Rows)
		{
			if (userdata.Equals(item.UserData))
			{
				CurrentOpendedRow = item;
				return;
			}
		}
		CurrentOpendedRow = null;
	}

	public void DealInvalidateAccounts(IEnumerable<Account> accounts)
	{
		if (accounts == null || accounts.Count() == 0)
		{
			return;
		}
		Dictionary<Account, C1.Win.C1FlexGrid.Row> dictionary = new Dictionary<Account, C1.Win.C1FlexGrid.Row>();
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)Tree.Rows)
		{
			if (item.UserData is Account key)
			{
				dictionary.Add(key, item);
			}
		}
		foreach (Account account in accounts)
		{
			if (dictionary.ContainsKey(account))
			{
				C1.Win.C1FlexGrid.Row row2 = dictionary[account];
				(row2.Style ?? row2.StyleNew).ForeColor = Color.Red;
				RowFlickerProxy rowFlickerProxy = new RowFlickerProxy(row2, contentFlick: true);
				rowFlickerProxy.SetTimer(SecondTrigger.Trigger);
				rowFlickerProxy.SetFlickTime(10);
				FlickerManager.Add(row2, rowFlickerProxy);
				FlickerManager.Start(row2);
				Invalidate.Add(account);
			}
		}
	}

	public void Dispose()
	{
		FlickerManager.Dispose();
		Tree.Clear(ClearFlags.UserData);
		Tree.Rows.Count = 0;
		Tree.Cols.Count = 0;
		Invalidate = null;
	}

	private void CmdShowEmptyAccount_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = !DisplayEmptyAccount;
	}

	private void CmdHideEmptyAccount_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = DisplayEmptyAccount;
	}

	private void CmdAddAccount_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int row = Tree.Row;
		e.Visible = row >= 0 && row < Tree.Rows.Count && Tree.Rows[row].UserData is Account;
	}

	private void CmdModifyAccount_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int row = Tree.Row;
		e.Visible = row >= 0 && row < Tree.Rows.Count && Tree.Rows[row].UserData is Account;
	}

	private void mnuTree_Popup(object sernder, EventArgs e)
	{
		try
		{
			C1ContextMenu c1ContextMenu = sernder as C1ContextMenu;
			c1ContextMenu.CommandLinks.Clear();
			c1ContextMenu.CommandLinks.Add(lnkExpandAll);
			c1ContextMenu.CommandLinks.Add(lnkCollaspeAll);
			c1ContextMenu.CommandLinks.Add(lnkShowEmptyAccount);
			c1ContextMenu.CommandLinks.Add(lnkHideEmptyAccount);
			c1ContextMenu.CommandLinks.Add(lnkAddAccount);
			c1ContextMenu.CommandLinks.Add(lnkModifyAccount);
			c1ContextMenu.CommandLinks.Add(lnkDeleteAccount);
			lnkCancelSelect.Delimiter = true;
			lnkAddAccount.Delimiter = true;
			c1ContextMenu.CommandLinks.Add(lnkCancelSelect);
			c1ContextMenu.ShowAll();
			if (_owner.CurrentView != 0 && _owner.CurrentView != ActiveView.Subsidiary && _owner.CurrentView != ActiveView.AgeAnalazy)
			{
				return;
			}
			c1ContextMenu.HideLinks(lnkCancelSelect);
			int mouseRow = Tree.MouseRow;
			if (Tree.Row != mouseRow)
			{
				return;
			}
			C1.Win.C1FlexGrid.Row row = Tree.Rows[mouseRow];
			if (row.UserData is Account account)
			{
				Dictionary<AuxiliaryClass, ClassBalance> classBalances = _owner.CacheManager.GetAccountBalanceWithCache(Ledger, account).ClassBalances;
				if (classBalances.Count <= 0)
				{
					return;
				}
				bool flag = true;
				foreach (AuxiliaryClass auxClass2 in classBalances.Keys)
				{
					C1CommandLink c1CommandLink = Common.MakeCommandLink("展开" + auxClass2.Name + "核算", delegate
					{
						OnBalanceTreeShowAuxiliaryNode(Tree.Rows[Tree.Row].Node, auxClass2);
					}, auxClass2);
					if (flag)
					{
						flag = false;
						c1CommandLink.Delimiter = true;
					}
					c1ContextMenu.CommandLinks.Add(c1CommandLink);
				}
				AuxiliaryClass userData = (row.Node.Nodes.FirstOrDefault(delegate(Node n)
				{
					Tuple<Account, AuxiliaryItem> tuple2 = n.Key as Tuple<Account, AuxiliaryItem>;
					return tuple2 != null;
				})?.Key as Tuple<Account, AuxiliaryItem>)?.Item2.Class;
				C1CommandLink c1CommandLink2 = Common.MakeCommandLink("隐藏辅助核算", delegate(object s1, ClickEventArgs e1)
				{
					OnBalanceTreeHideAuxiliaryNode(s1, e1);
				}, userData);
				if (flag)
				{
					c1CommandLink2.Delimiter = true;
				}
				c1ContextMenu.CommandLinks.Add(c1CommandLink2);
			}
			else
			{
				if (!(row.UserData is Tuple<Account, AuxiliaryItem> tuple))
				{
					return;
				}
				bool flag2 = true;
				Dictionary<AuxiliaryClass, ClassBalance> classBalances2 = _owner.CacheManager.GetAccountBalanceWithCache(Ledger, tuple.Item1).ClassBalances;
				foreach (AuxiliaryClass auxClass in classBalances2.Keys)
				{
					if (auxClass != tuple.Item2.Class)
					{
						C1CommandLink c1CommandLink3 = Common.MakeCommandLink("切换" + auxClass.Name + "核算", delegate
						{
							OnBalanceTreeShowAuxiliaryNode(Tree.Rows[Tree.Row].Node, auxClass);
						}, auxClass);
						c1ContextMenu.CommandLinks.Add(c1CommandLink3);
						if (flag2)
						{
							c1CommandLink3.Delimiter = true;
							flag2 = false;
						}
					}
				}
				C1CommandLink c1CommandLink4 = Common.MakeCommandLink("隐藏辅助核算", delegate(object s1, ClickEventArgs e1)
				{
					OnBalanceTreeHideAuxiliaryNode(s1, e1);
				}, tuple.Item2.Class);
				c1ContextMenu.CommandLinks.Add(c1CommandLink4);
				if (flag2)
				{
					c1CommandLink4.Delimiter = true;
				}
			}
		}
		catch
		{
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Tree.HitTest(e.Location);
		if (e.Button == MouseButtons.Left)
		{
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				Rectangle cellRect = Tree.GetCellRect(hitTestInfo.Row, 0);
				Node node = Tree.Rows[hitTestInfo.Row].Node;
				node.Collapsed = !node.Collapsed;
				if (node.Key is Account item && Invalidate.Contains(item))
				{
					Rectangle cellRect2 = Tree.GetCellRect(hitTestInfo.Row, 0);
					Point point = new Point((cellRect2.Left + cellRect2.Right) / 2, cellRect2.Top);
				}
				else
				{
					Common.HideTooltipInfo();
				}
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			mnuTree.ShowContextMenu(Tree, e.Location);
		}
	}

	private void Tree_MouseLeave(object sender, EventArgs e)
	{
		_mouseRow = -1;
		Tree.Invalidate();
	}

	private void Tree_MouseMove(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Tree.HitTest();
		if (_mouseRow != hitTestInfo.Row)
		{
			_mouseRow = hitTestInfo.Row;
			Tree.Invalidate();
		}
	}

	private void Tree_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Row == _mouseRow)
		{
			e.Graphics.FillRectangle(_brushHoverBackground, e.Bounds);
		}
	}

	private void CmdCollaspeAll_Click(object sender, ClickEventArgs e)
	{
		Tree.Tree.Show(0);
		_owner.BalanceEditor.grdBalance.Tree.Show(0);
	}

	private void CmdExpandAll_Click(object sender, ClickEventArgs e)
	{
		Tree.Tree.Show(Tree.Tree.MaximumLevel);
		_owner.BalanceEditor.grdBalance.Tree.Show(_owner.BalanceEditor.grdBalance.Tree.MaximumLevel);
	}

	private void Tree_LostFocus(object sender, EventArgs e)
	{
		Common.HideTooltipInfo();
	}

	private void CloseNode(C1.Win.C1FlexGrid.Row e)
	{
		try
		{
			if (e == null)
			{
				return;
			}
			object userData = e.UserData;
			if (!(userData is Account key))
			{
				if (!(userData is Tuple<Account, AuxiliaryClass> tuple))
				{
					if (userData is Tuple<Account, AuxiliaryItem> tuple2 && _owner.CacheManager.GetTrialBalanceSheetWithCache(Ledger).End.ContainsKey(tuple2.Item1))
					{
						e.Node.Image = GetNodeImage(isAccountNode: false, isOpenedState: false);
					}
				}
				else if (_owner.CacheManager.GetTrialBalanceSheetWithCache(Ledger).End.ContainsKey(tuple.Item1))
				{
					e.Node.Image = GetNodeImage(isAccountNode: false, isOpenedState: false);
				}
			}
			else if (_owner.CacheManager.GetTrialBalanceSheetWithCache(Ledger).End.ContainsKey(key))
			{
				e.Node.Image = GetNodeImage(isAccountNode: true, isOpenedState: false);
			}
		}
		catch
		{
		}
	}

	private void OpenNode(C1.Win.C1FlexGrid.Row e)
	{
		try
		{
			if (e == null)
			{
				return;
			}
			object userData = e.UserData;
			if (!(userData is Account))
			{
				Tuple<Account, AuxiliaryClass> tuple = userData as Tuple<Account, AuxiliaryClass>;
				if (tuple == null)
				{
					Tuple<Account, AuxiliaryItem> tuple2 = userData as Tuple<Account, AuxiliaryItem>;
					if (tuple2 == null)
					{
						return;
					}
				}
				e.Node.Image = GetNodeImage(isAccountNode: false, isOpenedState: true);
			}
			else
			{
				e.Node.Image = GetNodeImage(isAccountNode: true, isOpenedState: true);
			}
		}
		catch
		{
		}
	}

	private void InitializeComponent()
	{
		Tree = GridFactory.Create("tree");
		Tree.Name = "Tree";
		Tree.Visible = true;
		Tree.AllowEditing = false;
		Tree.LostFocus += Tree_LostFocus;
		Tree.MouseClick += _grid_MouseClick;
		Tree.MouseMove += Tree_MouseMove;
		Tree.MouseLeave += Tree_MouseLeave;
		Tree.OwnerDrawCell += Tree_OwnerDrawCell;
	}

	private void BindBalanceTreeContexMenu()
	{
		mnuTree.Popup += mnuTree_Popup;
		cmdExpandAll.Text = "全部展开";
		cmdExpandAll.Click += CmdExpandAll_Click;
		lnkExpandAll.Command = cmdExpandAll;
		cmdCollaspeAll.Text = "全部收缩";
		cmdCollaspeAll.Click += CmdCollaspeAll_Click;
		lnkCollaspeAll.Command = cmdCollaspeAll;
		cmdCancelSelect.Text = "取消选择";
		cmdCancelSelect.Click += delegate
		{
			OnBalanceTreeCancelAll(Tree, EventArgs.Empty);
		};
		lnkCancelSelect.Command = cmdCancelSelect;
		cmdHideEmptyAccount.Text = "不显示空科目";
		cmdHideEmptyAccount.CommandStateQuery += CmdHideEmptyAccount_CommandStateQuery;
		cmdHideEmptyAccount.Click += delegate
		{
			OnBalanceTreeHideEmptyAccount(this, EventArgs.Empty);
		};
		lnkHideEmptyAccount.Command = cmdHideEmptyAccount;
		cmdShowEmptyAccount.Text = "显示空科目";
		cmdShowEmptyAccount.CommandStateQuery += CmdShowEmptyAccount_CommandStateQuery;
		cmdShowEmptyAccount.Click += delegate
		{
			OnBalanceTreeShowEmptyAccount(this, EventArgs.Empty);
		};
		lnkShowEmptyAccount.Command = cmdShowEmptyAccount;
		cmdAddAccount.Text = "新增科目";
		cmdAddAccount.Image = ContextResources.addLedger;
		cmdAddAccount.CommandStateQuery += CmdAddAccount_CommandStateQuery;
		cmdAddAccount.Click += delegate
		{
			if (Tree.Rows[Tree.Row].UserData is Account)
			{
				AddAccount();
			}
		};
		lnkAddAccount.Command = cmdAddAccount;
		cmdModifyAccount.Text = "修改科目";
		cmdModifyAccount.Image = ContextResources.modifyLedger;
		cmdModifyAccount.CommandStateQuery += CmdModifyAccount_CommandStateQuery;
		cmdModifyAccount.Click += delegate
		{
			ModifyAccount(Tree.Rows[Tree.Row]);
		};
		lnkModifyAccount.Command = cmdModifyAccount;
		cmdDeleteAccount.Text = "删除科目";
		cmdDeleteAccount.Image = ContextResources.deleteLedger;
		cmdDeleteAccount.CommandStateQuery += CmdDeleteAccount_CommandStateQuery;
		cmdDeleteAccount.Click += delegate
		{
			DeleteAccount(Tree.Rows[Tree.Row]);
		};
		lnkDeleteAccount.Command = cmdDeleteAccount;
		lnkShowAllAux.Command = cmdShowAllAux;
		cmdShowAllAux.Text = "显示全部科目辅助核算";
		cmdShowAllAux.Click += CmdShowAllAux_Click;
		lnkHideAllAux.Command = cmdHideAllAux;
		cmdHideAllAux.Text = "隐藏全部科目辅助核算";
		cmdHideAllAux.Click += CmdHideAllAux_Click;
	}

	private void CmdHideAllAux_Click(object sender, ClickEventArgs e)
	{
	}

	private void CmdShowAllAux_Click(object sender, ClickEventArgs e)
	{
	}

	private void CmdDeleteAccount_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int row = Tree.Row;
		e.Visible = row >= Tree.Rows.Fixed && row < Tree.Rows.Count && Tree.Rows[row].UserData is Account;
	}

	private void AddAccount()
	{
		frmAccountEditor frmAccountEditor2 = new frmAccountEditor(_owner, this);
		if (DialogResult.OK != frmAccountEditor2.ShowAddAccountDialog())
		{
			return;
		}
		Account account = new Account();
		account.Code = frmAccountEditor2.AccountCode;
		account.Name = frmAccountEditor2.AccountName;
		account.Dirty = 1;
		account.Id = ((Ledger.Accounts.Count != 0) ? (Ledger.Accounts.Max((Account a) => a.Id) + 1) : 0);
		account.Ledger = Ledger;
		if (frmAccountEditor2.ParentAccount != null)
		{
			account.Parent = frmAccountEditor2.ParentAccount;
			frmAccountEditor2.ParentAccount.Children.Add(account);
			frmAccountEditor2.ParentAccount.Dirty = 2;
		}
		Ledger.InitialBalance.Add(account, new AccountBalance());
		Ledger.Accounts.Add(account);
		Ledger.Save();
		_owner.OnAfterAddAccount(account);
		if (account.Parent == null)
		{
			ValidateAccount validateAccount = new ValidateAccount(DictionarySync.CellCollector);
			if (!validateAccount.Validate(account))
			{
				DealInvalidateAccounts(new Account[1] { account });
			}
		}
	}

	private void ModifyAccount(C1.Win.C1FlexGrid.Row row)
	{
		if (!(row.UserData is Account account))
		{
			return;
		}
		frmAccountEditor frmAccountEditor2 = new frmAccountEditor(_owner, this);
		frmAccountEditor2.SetAccountCode(account.Code);
		frmAccountEditor2.SetAccountName(account.Name);
		if (DialogResult.OK != frmAccountEditor2.ShowModifyAccountDialog(account))
		{
			return;
		}
		string code = account.Code;
		if (!(frmAccountEditor2.AccountCode != account.Code) && !(frmAccountEditor2.AccountName != account.Name))
		{
			return;
		}
		account.Code = frmAccountEditor2.AccountCode;
		account.Name = frmAccountEditor2.AccountName;
		account.Dirty = 2;
		Ledger.Save();
		row.Node.Data = account.Code + " " + account.Name;
		_owner.OnAfterModifyAccount(account);
		if (account.Parent == null)
		{
			ValidateAccount validateAccount = new ValidateAccount(DictionarySync.CellCollector);
			if (validateAccount.Validate(account))
			{
				(row.Style ?? row.StyleNew).ForeColor = Color.Black;
				FlickerManager.Remove(row);
				Invalidate.Remove(account);
			}
			else
			{
				DealInvalidateAccounts(new Account[1] { account });
			}
		}
	}

	private void DeleteAccount(C1.Win.C1FlexGrid.Row row)
	{
		if (!(row.UserData is Account account))
		{
			return;
		}
		foreach (Account item in account.DescendantsAndSelf)
		{
			if (!_owner.CacheManager.IsEmptyAccountWithCache(item))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该科目含有数据，不能删除");
				return;
			}
		}
		if (DialogResult.OK != Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "确定要删除选定科目吗？", MessageBoxButtons.OKCancel))
		{
			return;
		}
		List<Account> descendantsAndSelf = account.DescendantsAndSelf;
		List<Voucher> list = new List<Voucher>();
		foreach (Account acc in descendantsAndSelf)
		{
			foreach (Voucher item2 in Ledger.Vouchers.Where((Voucher v) => v.Account == acc))
			{
				item2.Dirty = -1;
				list.Add(item2);
			}
			acc.Dirty = -1;
		}
		if (account.Parent != null)
		{
			account.Parent.Children.Remove(account);
			account.Parent.Dirty = 2;
		}
		Ledger.Save();
		foreach (Voucher item3 in list)
		{
			Ledger.Vouchers.Remove(item3);
		}
		foreach (Account item4 in descendantsAndSelf)
		{
			Ledger.Accounts.Remove(item4);
		}
		row.Node.RemoveNode();
		_owner.OnAfterDeleteAccount(account);
	}

	private async Task AddAuxiliaryItem(AuxiliaryClass auxiliaryClass)
	{
		frmAccountEditor frmAccountEditor2 = new frmAccountEditor(_owner, this);
		if (DialogResult.OK != frmAccountEditor2.ShowAddAuxiliaryDialog(auxiliaryClass))
		{
			return;
		}
		AuxiliaryItem auxiliaryItem = new AuxiliaryItem();
		auxiliaryItem.Code = frmAccountEditor2.AccountCode;
		auxiliaryItem.Name = frmAccountEditor2.AccountName;
		auxiliaryItem.Class = auxiliaryClass;
		auxiliaryClass.Items.Add(auxiliaryItem);
		Ledger.AuxiliaryItems.Add(auxiliaryItem);
		DateBalance initialBalance = Ledger.InitialBalance;
		foreach (Account account in Ledger.Accounts)
		{
			if (!initialBalance.ContainsKey(account))
			{
				continue;
			}
			AccountBalance accountBalance = initialBalance[account];
			if (accountBalance.ClassBalances.ContainsKey(auxiliaryClass))
			{
				ClassBalance classBalance = accountBalance.ClassBalances[auxiliaryClass];
				if (!classBalance.ItemBalances.ContainsKey(auxiliaryItem))
				{
					classBalance.ItemBalances.Add(auxiliaryItem, 0m);
				}
			}
		}
		Ledger.Save();
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "新增" + auxiliaryClass.Name + "成功！即将重新加载账套");
		await _owner.OpenLedger(_owner.CurrentFilePath);
	}

	private async Task ModifyAuxiliaryClass(AuxiliaryClass auxiliaryClass)
	{
		frmAccountEditor frmAccountEditor2 = new frmAccountEditor(_owner, this);
		frmAccountEditor2.SetAccountCode(auxiliaryClass.Code);
		frmAccountEditor2.SetAccountName(auxiliaryClass.Name);
		if (DialogResult.OK == frmAccountEditor2.ShowModifyAuxiliaryDialog(auxiliaryClass) && (frmAccountEditor2.AccountCode != auxiliaryClass.Code || frmAccountEditor2.AccountName != auxiliaryClass.Name))
		{
			auxiliaryClass.Code = frmAccountEditor2.AccountCode;
			auxiliaryClass.Name = frmAccountEditor2.AccountName;
			Ledger.Save();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "修改" + auxiliaryClass.Name + "成功！即将重新加载账套");
			await _owner.OpenLedger(_owner.CurrentFilePath);
		}
	}

	private async Task DeleteAuxiliaryClass(Account account, AuxiliaryClass auxiliaryClass)
	{
		List<Voucher> list = Ledger.Vouchers.Where((Voucher v) => v.Account == account).ToList();
		list.ForEach(delegate(Voucher v)
		{
			v.Details.RemoveAll((AuxiliaryItem d) => d.Class == auxiliaryClass);
		});
		if (Ledger.InitialBalance.ContainsKey(account))
		{
			Ledger.InitialBalance[account].ClassBalances?.Remove(auxiliaryClass);
		}
		Ledger.Save();
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "删除科目成功！即将重新加载账套");
		await _owner.OpenLedger(_owner.CurrentFilePath);
	}

	private bool AppendAuxiliaryItems(Node accNode, AuxiliaryClass auxClass)
	{
		if (!(accNode.Key is Account account))
		{
			return false;
		}
		Tree.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = _owner.CacheManager.GetAccountBalanceWithCache(Ledger, account).ClassBalances;
			foreach (KeyValuePair<AuxiliaryItem, decimal> item in classBalances[auxClass].ItemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
			{
				if (_owner.IsDisplayEmptyAccount() || !_owner.IsEmptyAuxiliaryItem(account, item.Key))
				{
					AuxiliaryItem key = item.Key;
					Node node = accNode.AddNode(NodeTypeEnum.LastChild, account.Code + "-" + key.Code + " " + key.Name);
					node.Key = Tuple.Create(account, item.Key);
					node.Image = zb3Image;
					node.Expanded = true;
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return false;
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			Tree.EndUpdate();
		}
	}

	private bool RemoveAuxiliaryItems(Node accNode, AuxiliaryClass auxClass = null)
	{
		if (!(accNode.Key is Account))
		{
			return false;
		}
		List<Node> list = new List<Node>();
		Node[] nodes = accNode.Nodes;
		foreach (Node node in nodes)
		{
			if (node.Key is Tuple<Account, AuxiliaryItem> tuple && (auxClass == null || tuple.Item2.Class == auxClass))
			{
				list.Add(node);
			}
		}
		Tree.BeginUpdate();
		try
		{
			foreach (Node item in list)
			{
				item.RemoveNode();
			}
			return true;
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			return false;
		}
		finally
		{
			Tree.EndUpdate();
		}
	}

	private System.Drawing.Image GetNodeImage(bool isAccountNode, bool isOpenedState)
	{
		if (isAccountNode)
		{
			if (!isOpenedState)
			{
				return zb1Image;
			}
			return zb2Image;
		}
		if (!isOpenedState)
		{
			return zb3Image;
		}
		return zb4Image;
	}

	public void SetTheme()
	{
		Tree.Styles.Alternate.BackColor = Color.Transparent;
		Tree.Styles.Fixed.Border.Width = 0;
		Tree.Styles.Normal.Border.Width = 0;
		Tree.Styles.EmptyArea.BackColor = Color.Transparent;
		Tree.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_brushHoverBackground.Color = Color.FromArgb(100, Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background"));
	}
}
