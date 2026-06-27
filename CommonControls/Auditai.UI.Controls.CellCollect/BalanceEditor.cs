using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.UI.Controls.CollectCell;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls.CellCollect;

internal class BalanceEditor
{
	private const string CN_CODE = "Code";

	private const string CN_NAME = "Name";

	private const string CN_INDEX = "Index";

	private const string CN_VALUE = "Value";

	private const string CN_OPERATION = "Operation";

	private const string CN_INCURRED = "AmountIncurred";

	private const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	private Dictionary<AmountEnum, string> AmountDic = new Dictionary<AmountEnum, string>
	{
		[AmountEnum.DebitBegin] = "期初借方余额",
		[AmountEnum.CreditBegin] = "期初贷方余额",
		[AmountEnum.DebitAmount] = "本期借方发生额",
		[AmountEnum.CreditAmount] = "本期贷方发生额",
		[AmountEnum.PreDebitAmount] = "上期借方发生额",
		[AmountEnum.PreCreditAmount] = "上期贷方发生额",
		[AmountEnum.DebitBalance] = "期末借方余额",
		[AmountEnum.CreditBalance] = "期末贷方余额"
	};

	private Dictionary<OperateEnum, string> OperateDic = new Dictionary<OperateEnum, string>
	{
		[OperateEnum.Add] = "加",
		[OperateEnum.Subtract] = "减"
	};

	private frmCellCollect _owner;

	private ComboTree accountTree;

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1CommandLink lnkAppendRow = new C1CommandLink();

	private C1Command cmdAppendRow = new C1Command();

	private C1CommandLink lnkDeleteRow = new C1CommandLink();

	private C1Command cmdDeleteRow = new C1Command();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private C1CommandLink lnkAppendRow2 = new C1CommandLink();

	private C1Command cmdAppendRow2 = new C1Command();

	private Dictionary<C1.Win.C1FlexGrid.Row, BalanceItem> collectsDic;

	private int auditYear;

	private bool eventAttached;

	private C1FlexGrid _grid => _owner.grdBalance;

	private Ledger _ledger => _owner.Ledger;

	private DateTime _start => _owner.StartTime;

	private DateTime _end => _owner.EndTime;

	public List<BalanceItem> Collects => collectsDic.Values.ToList();

	public BalanceEditor(frmCellCollect owner, int auditYear)
	{
		_owner = owner;
		this.auditYear = auditYear;
		Initialize(owner);
	}

	private void grdBalance_AutoAppendRow(object sender, RowColEventArgs e)
	{
		if (e.Row != _grid.Rows.Count - 1)
		{
			return;
		}
		bool flag = eventAttached;
		if (flag)
		{
			DetachEvents();
		}
		try
		{
			_grid.Rows.Add();
			PopulateRowIndex();
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
		}
	}

	private void grdBalance_CalculateValue(object sender, RowColEventArgs e)
	{
		bool flag = eventAttached;
		if (flag)
		{
			DetachEvents();
		}
		try
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[e.Row];
			if (string.IsNullOrWhiteSpace(row["Operation"]?.ToString()))
			{
				row["Operation"] = OperateDic.First().Value;
			}
			if (collectsDic.ContainsKey(row))
			{
				collectsDic.Remove(row);
			}
			BalanceItem balanceItem = GetBalanceItem(row);
			if (balanceItem != null)
			{
				collectsDic.Add(row, balanceItem);
				decimal num = default(decimal);
				try
				{
					num = balanceItem.GetValue(_ledger, auditYear);
				}
				catch (UnExpectAuditYearException)
				{
				}
				catch (InvalidAuditYearException)
				{
				}
				row["Value"] = num;
			}
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
		}
	}

	private void grdBalance_KeyDown(object sender, KeyEventArgs e)
	{
		bool flag = eventAttached;
		if (flag)
		{
			DetachEvents();
		}
		try
		{
			if (e.KeyCode != Keys.Delete)
			{
				return;
			}
			CellRange selection = _grid.Selection;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
				row.Clear(ClearFlags.Content | ClearFlags.UserData);
				if (collectsDic.ContainsKey(row))
				{
					collectsDic.Remove(row);
				}
			}
			PopulateRowIndex();
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
		}
	}

	private void grdBalance_AfterEdit(object sender, RowColEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = _grid.Rows[e.Row];
		int index = _grid.Cols["Code"].Index;
		if (index == e.Col && row.UserData is Account account)
		{
			row["Code"] = account.Code;
		}
	}

	private void _grid_AfterAddRow(object sender, RowColEventArgs e)
	{
		PopulateRowIndex();
	}

	private void _grid_AfterDeleteRow(object sender, RowColEventArgs e)
	{
		PopulateRowIndex();
	}

	private void accountTree_SelectNodeChanged(object sender, TreeViewEventArgs e)
	{
		if (e.Node?.Tag is Account account)
		{
			accountTree.Text = account.Code;
			C1.Win.C1FlexGrid.Row row = _grid.Rows[_grid.Row];
			row.UserData = account;
			row["Name"] = account.Name;
			row["Code"] = account.Code;
		}
	}

	private void accountTree_TreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
	{
		if (e.Node.Equals(accountTree.TreeView.SelectedNode))
		{
			accountTree.dropDown.Close();
			accountTree_SelectNodeChanged(null, new TreeViewEventArgs(e.Node));
		}
	}

	private void StartTimeChanged(object sender, DateTime t)
	{
		RefreshCollectsValue();
	}

	private void EndTimeChanged(object sender, DateTime t)
	{
		RefreshCollectsValue();
	}

	private void AttachEvents()
	{
		if (!eventAttached)
		{
			_grid.KeyDown += grdBalance_KeyDown;
			_grid.AfterEdit += grdBalance_AfterEdit;
			_grid.CellChanged += grdBalance_CalculateValue;
			_grid.CellChanged += grdBalance_AutoAppendRow;
			_grid.AfterAddRow += _grid_AfterAddRow;
			_grid.AfterDeleteRow += _grid_AfterDeleteRow;
			eventAttached = true;
		}
	}

	private void DetachEvents()
	{
		if (eventAttached)
		{
			_grid.KeyDown -= grdBalance_KeyDown;
			_grid.AfterEdit -= grdBalance_AfterEdit;
			_grid.CellChanged -= grdBalance_CalculateValue;
			_grid.CellChanged -= grdBalance_AutoAppendRow;
			_grid.AfterAddRow -= _grid_AfterAddRow;
			_grid.AfterDeleteRow -= _grid_AfterDeleteRow;
			eventAttached = false;
		}
	}

	private void Initialize(frmCellCollect owner)
	{
		collectsDic = new Dictionary<C1.Win.C1FlexGrid.Row, BalanceItem>();
		_grid.Rows.DefaultSize = 30;
		accountTree = new ComboTree
		{
			DropWidth = 180,
			DropHeight = 250
		};
		PopulateAccountTree(accountTree);
		accountTree.SelectNodeChanged += accountTree_SelectNodeChanged;
		accountTree.TreeView.NodeMouseClick += accountTree_TreeView_NodeMouseClick;
		cmdAppendRow.Text = "新增行";
		lnkAppendRow.Command = cmdAppendRow;
		cmdAppendRow.Click += CmdAppendRow_Click;
		cmdAppendRow.Image = Resources.ctxAppendRow;
		ctxCell.CommandLinks.Add(lnkAppendRow);
		cmdDeleteRow.Text = "删除行";
		lnkDeleteRow.Command = cmdDeleteRow;
		cmdDeleteRow.Click += CmdDeleteRow_Click;
		cmdDeleteRow.Image = Resources.ctxDeleteRow;
		ctxCell.CommandLinks.Add(lnkDeleteRow);
		_owner.grdBalance.MouseClick += GrdBalance_MouseClick;
		_owner.grdBalance.MouseDown += GrdBalance_MouseDown;
		cmdAppendRow2.Text = "新增行";
		lnkAppendRow2.Command = cmdAppendRow2;
		cmdAppendRow2.Click += CmdAppendRow_Click;
		cmdAppendRow2.Image = Resources.ctxAppendRow;
		ctxEmpty.CommandLinks.Add(lnkAppendRow2);
		_owner.comboStartMonth.Text = "1";
		_owner.comboEndMonth.Text = "12";
		frmCellCollect owner2 = _owner;
		owner2.StartTimeChanged = (EventHandler<DateTime>)Delegate.Combine(owner2.StartTimeChanged, new EventHandler<DateTime>(StartTimeChanged));
		frmCellCollect owner3 = _owner;
		owner3.EndTimeChanged = (EventHandler<DateTime>)Delegate.Combine(owner3.EndTimeChanged, new EventHandler<DateTime>(EndTimeChanged));
		InitalizationBalanceGrid(_grid);
		AttachEvents();
	}

	private void GrdBalance_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		C1FlexGrid grdBalance = _owner.grdBalance;
		HitTestInfo hitTestInfo = grdBalance.HitTest(e.Location);
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.Cell || type == HitTestTypeEnum.RowHeader)
		{
			if (hitTestInfo.Column == 0)
			{
				grdBalance.Select(new CellRange
				{
					r1 = hitTestInfo.Row,
					r2 = hitTestInfo.Row,
					c1 = 0,
					c2 = grdBalance.Cols.Count - 1
				});
			}
			else if (!grdBalance.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				grdBalance.Select(hitTestInfo.Row, hitTestInfo.Column);
			}
		}
	}

	private void GrdBalance_MouseClick(object sender, MouseEventArgs e)
	{
		C1FlexGrid grdBalance = _owner.grdBalance;
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		switch (grdBalance.HitTest(e.Location).Type)
		{
		case HitTestTypeEnum.Cell:
		case HitTestTypeEnum.RowHeader:
			if (grdBalance.MouseRow >= grdBalance.Rows.Fixed)
			{
				ctxCell.ShowContextMenu(grdBalance, e.Location);
			}
			break;
		case HitTestTypeEnum.None:
			ctxEmpty.ShowContextMenu(grdBalance, e.Location);
			break;
		}
	}

	private void CmdAppendRow_Click(object sender, ClickEventArgs e)
	{
		C1FlexGrid grdBalance = _owner.grdBalance;
		grdBalance.BeginUpdate();
		bool flag = eventAttached;
		try
		{
			if (flag)
			{
				DetachEvents();
			}
			grdBalance.Rows.Add(1);
			PopulateRowIndex();
		}
		catch (Exception ex)
		{
			MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
			grdBalance.EndUpdate();
		}
	}

	private void CmdInsertRow_Click(object sender, ClickEventArgs e)
	{
		C1FlexGrid grdBalance = _owner.grdBalance;
		if (grdBalance.MouseRow < grdBalance.Rows.Fixed || grdBalance.Row >= grdBalance.Rows.Count)
		{
			return;
		}
		decimal? num = InputForm.Numeric("插入行", "请输入插入行数：");
		if (!num.HasValue)
		{
			return;
		}
		grdBalance.BeginUpdate();
		bool flag = eventAttached;
		try
		{
			if (flag)
			{
				DetachEvents();
			}
			for (int i = 0; (decimal)i < num.Value; i++)
			{
				grdBalance.Rows.Insert(grdBalance.Row);
			}
			PopulateRowIndex();
		}
		catch (Exception ex)
		{
			MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
			grdBalance.EndUpdate();
		}
	}

	private void CmdDeleteRow_Click(object sender, ClickEventArgs e)
	{
		C1FlexGrid grdBalance = _owner.grdBalance;
		grdBalance.BeginUpdate();
		bool flag = eventAttached;
		try
		{
			if (flag)
			{
				DetachEvents();
			}
			int topRow = grdBalance.Selection.TopRow;
			int count = grdBalance.Selection.BottomRow - topRow + 1;
			for (int i = topRow; i < topRow + count && i < grdBalance.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
				if (collectsDic.ContainsKey(row))
				{
					collectsDic.Remove(row);
				}
			}
			grdBalance.Rows.RemoveRange(topRow, count);
			PopulateRowIndex();
		}
		catch (Exception ex)
		{
			MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
			grdBalance.EndUpdate();
		}
	}

	private void PopulateRowIndex()
	{
		bool flag = eventAttached;
		if (flag)
		{
			DetachEvents();
		}
		try
		{
			int num = 1;
			for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
			{
				_grid.Rows[i]["Index"] = num++.ToString();
			}
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
		}
	}

	public void PopulateCollects(IEnumerable<BalanceItem> collects)
	{
		bool flag = eventAttached;
		if (flag)
		{
			DetachEvents();
		}
		try
		{
			collectsDic.Clear();
			InitalizationBalanceGrid(_grid);
			if (collects == null || collects.Count() == 0)
			{
				return;
			}
			int @fixed = _grid.Rows.Fixed;
			foreach (BalanceItem collect in collects)
			{
				if (@fixed >= _grid.Rows.Count)
				{
					_grid.Rows.Add();
				}
				Account account = _ledger.Accounts.Find((Account a) => a.Code.Equals(collect.AccountCode));
				if (account != null)
				{
					C1.Win.C1FlexGrid.Row row = _grid.Rows[@fixed++];
					row.UserData = account;
					row["Operation"] = OperateDic[collect.Operation];
					row["AmountIncurred"] = AmountDic[collect.AmountEnum];
					row["Code"] = collect.AccountCode;
					row["Name"] = account.Name;
					decimal num = default(decimal);
					try
					{
						num = collect.GetValue(_ledger, auditYear);
						row["Value"] = num;
						collectsDic.Add(row, collect);
					}
					catch (InvalidAuditYearException)
					{
						row["Value"] = num;
						collectsDic.Add(row, collect);
					}
					catch (UnExpectAuditYearException)
					{
						row["Value"] = num;
						collectsDic.Add(row, collect);
					}
					catch (InvalidCollectSettingException)
					{
					}
				}
			}
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
		}
	}

	private void RefreshCollectsValue()
	{
		bool flag = eventAttached;
		if (flag)
		{
			DetachEvents();
		}
		try
		{
			List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
			list.AddRange(collectsDic.Keys);
			foreach (C1.Win.C1FlexGrid.Row item in list)
			{
				decimal num = default(decimal);
				try
				{
					collectsDic[item].StartTime = _start;
					collectsDic[item].EndTime = _end;
					BalanceItem balanceItem = collectsDic[item];
					num = balanceItem.GetValue(_ledger, auditYear);
				}
				catch
				{
				}
				item["Value"] = num;
			}
		}
		finally
		{
			if (flag)
			{
				AttachEvents();
			}
		}
	}

	private BalanceItem GetBalanceItem(C1.Win.C1FlexGrid.Row row)
	{
		BalanceItem balanceItem = null;
		if (row.UserData is Account account)
		{
			if (_start == default(DateTime))
			{
				return null;
			}
			if (_end == default(DateTime))
			{
				return null;
			}
			balanceItem = new BalanceItem();
			balanceItem.AccountCode = account.Code;
			object operateText = row["Operation"];
			if (!OperateDic.ContainsValue(operateText?.ToString()))
			{
				return null;
			}
			balanceItem.Operation = OperateDic.First((KeyValuePair<OperateEnum, string> t) => t.Value.Equals(operateText)).Key;
			object amountText = row["AmountIncurred"];
			if (!AmountDic.ContainsValue(amountText?.ToString()))
			{
				return null;
			}
			balanceItem.AmountEnum = AmountDic.First((KeyValuePair<AmountEnum, string> t) => t.Value.Equals(amountText)).Key;
			balanceItem.StartTime = _start;
			balanceItem.EndTime = _end;
		}
		return balanceItem;
	}

	private void InitalizationBalanceGrid(C1FlexGrid grid)
	{
		grid.BeginUpdate();
		try
		{
			grid.Cols.Count = 0;
			grid.Rows.Count = 0;
			grid.Rows.Count = 6;
			grid.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grid.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "Operation";
			column.Caption = "加减";
			column.ComboList = string.Join("|", OperateDic.Values.ToArray());
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.AllowEditing = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "AmountIncurred";
			column.Caption = "发生额/余额";
			column.ComboList = string.Join("|", AmountDic.Values.ToArray());
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "Value";
			column.Caption = "数据";
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column.AllowEditing = false;
			column.Format = "#,0.00;-#,0.00;#";
			grid.Rows.Fixed = 1;
			grid.Cols.Fixed = 1;
			grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grid.Cols["Code"].Editor = accountTree;
			PopulateRowIndex();
		}
		finally
		{
			grid.EndUpdate();
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
			TrialBalanceSheet trialBalanceSheet = _ledger.GetTrialBalanceSheet(_start, _end);
			foreach (Account rootAccount in _ledger.RootAccounts)
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
}
