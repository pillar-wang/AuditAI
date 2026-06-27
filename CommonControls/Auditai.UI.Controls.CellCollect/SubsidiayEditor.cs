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

internal class SubsidiayEditor
{
	private const string CN_CODE = "Code";

	private const string CN_NAME = "Name";

	private const string CN_INDEX = "Index";

	private const string CN_VALUE = "Value";

	private const string CN_DIGEST = "Digest";

	private const string CN_DIRECTION = "Direction";

	private const string CN_OPERATION = "Operation";

	private const string CN_TYPENUMBER = "TypeNumber";

	private const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	private const string CN_ITEMCLASS = "ItemClass";

	private const string CN_ITEMNUMBER = "ItemNumber";

	private const string CN_ITEMNAME = "ItemName";

	private const string CN_DEBITBEGINBALANCE = "DebitBeginBalance";

	private const string CN_CREDITBEGINBALANCE = "CreditBeginBalance";

	private const string CN_DEBIT = "Debit";

	private const string CN_CREDIT = "Credit";

	private const string CN_DEBITENDBALANCE = "DebitEndBalance";

	private const string CN_CREDITENDBALANCE = "CreditEndBalance";

	private const string CN_DATE = "Date";

	private const string CN_TYPE = "Type";

	private const string CN_TYPENUM = "TypeNum";

	private const string CN_NUMBER = "Number";

	private const string CN_DC = "DC";

	private const string CN_BALANCE = "Balance";

	public static Dictionary<OperateEnum, string> OperationDic = new Dictionary<OperateEnum, string>
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

	private Dictionary<C1.Win.C1FlexGrid.Row, SubsidiaryItem> collectDic;

	private int auditYear;

	private bool eventAttached;

	private C1FlexGrid _grid => _owner.grdSubsidiary;

	private Ledger _ledger => _owner.Ledger;

	private DateTime _start => _owner.StartTime;

	private DateTime _end => _owner.EndTime;

	public List<SubsidiaryItem> Collects => collectDic.Values.ToList();

	public SubsidiayEditor(frmCellCollect owner, int auditYear)
	{
		_owner = owner;
		this.auditYear = auditYear;
		Initialize(owner);
	}

	private void _grid_AutoAppendRow(object sender, RowColEventArgs e)
	{
		if (e.Row != _grid.Rows.Count - 1)
		{
			return;
		}
		bool flag = eventAttached;
		if (flag)
		{
			DettachEvents();
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

	private void _grid_CalculateValue(object sender, RowColEventArgs e)
	{
		bool flag = eventAttached;
		if (flag)
		{
			DettachEvents();
		}
		try
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[e.Row];
			if (string.IsNullOrWhiteSpace(row["Operation"]?.ToString()))
			{
				row["Operation"] = OperationDic.First().Value;
			}
			if (collectDic.ContainsKey(row))
			{
				collectDic.Remove(row);
			}
			SubsidiaryItem subsidiaryItem = GetSubsidiaryItem(row);
			if (subsidiaryItem != null)
			{
				collectDic.Add(row, subsidiaryItem);
				decimal num = default(decimal);
				try
				{
					num = subsidiaryItem.GetValue(_ledger, auditYear);
				}
				catch (InvalidAuditYearException)
				{
				}
				catch (UnExpectAuditYearException)
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

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		bool flag = eventAttached;
		if (flag)
		{
			DettachEvents();
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
				if (collectDic.ContainsKey(row))
				{
					collectDic.Remove(row);
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

	private void _grid_AfterEdit(object sender, RowColEventArgs e)
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
			row["TypeNumber"] = string.Empty;
			row["Digest"] = string.Empty;
			row["Direction"] = string.Empty;
			row["Value"] = string.Empty;
			DropFlexGrid dropFlexGrid = new DropFlexGrid(500, 300);
			PopulateSubsidiary(dropFlexGrid.flex, account, _start, _end);
			C1.Win.C1FlexGrid.CellStyle cellStyle = _grid.Styles.Add($"Digest{_grid.Row}");
			cellStyle.Editor = dropFlexGrid;
			_grid.SetCellStyle(_grid.Row, "TypeNumber", cellStyle);
			_grid.Cols["TypeNumber"].AllowEditing = true;
			dropFlexGrid.SelectChanged += delegate(object s1, Voucher e1)
			{
				C1.Win.C1FlexGrid.Row row2 = _grid.Rows[_grid.Row];
				row2.UserData = e1;
				row2["TypeNumber"] = e1.Type.Name + e1.Number;
				row2["Direction"] = (e1.IsDebit ? "借方" : "贷方");
				row2["Digest"] = e1.Digest;
				row2["Value"] = e1.Amount;
			};
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

	private void AttachEvents()
	{
		if (!eventAttached)
		{
			_grid.AfterEdit += _grid_AfterEdit;
			_grid.CellChanged += _grid_CalculateValue;
			_grid.CellChanged += _grid_AutoAppendRow;
			_grid.KeyDown += _grid_KeyDown;
			_grid.AfterAddRow += _grid_AfterAddRow;
			_grid.AfterDeleteRow += _grid_AfterDeleteRow;
			eventAttached = true;
		}
	}

	private void DettachEvents()
	{
		if (eventAttached)
		{
			_grid.AfterEdit -= _grid_AfterEdit;
			_grid.CellChanged -= _grid_CalculateValue;
			_grid.CellChanged -= _grid_AutoAppendRow;
			_grid.KeyDown -= _grid_KeyDown;
			_grid.AfterAddRow -= _grid_AfterAddRow;
			_grid.AfterDeleteRow -= _grid_AfterDeleteRow;
			eventAttached = false;
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

	private void Initialize(frmCellCollect owner)
	{
		collectDic = new Dictionary<C1.Win.C1FlexGrid.Row, SubsidiaryItem>();
		_grid.Rows.DefaultSize = 30;
		accountTree = new ComboTree
		{
			DropWidth = 180,
			DropHeight = 250
		};
		PopulateAccountTree(accountTree, _ledger);
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
		_owner.grdSubsidiary.MouseClick += GrdSubsidiary_MouseClick;
		_owner.grdSubsidiary.MouseDown += GrdSubsidiary_MouseDown;
		cmdAppendRow2.Text = "新增行";
		lnkAppendRow2.Command = cmdAppendRow2;
		cmdAppendRow2.Click += CmdAppendRow_Click;
		cmdAppendRow2.Image = Resources.ctxAppendRow;
		ctxEmpty.CommandLinks.Add(lnkAppendRow2);
		InitiliazeSubsidiaryGrid(_grid);
		AttachEvents();
	}

	private void GrdSubsidiary_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		C1FlexGrid grdSubsidiary = _owner.grdSubsidiary;
		HitTestInfo hitTestInfo = grdSubsidiary.HitTest(e.Location);
		HitTestTypeEnum type = hitTestInfo.Type;
		if (type == HitTestTypeEnum.Cell || type == HitTestTypeEnum.RowHeader)
		{
			if (hitTestInfo.Column == 0)
			{
				grdSubsidiary.Select(new CellRange
				{
					r1 = hitTestInfo.Row,
					r2 = hitTestInfo.Row,
					c1 = 0,
					c2 = grdSubsidiary.Cols.Count - 1
				});
			}
			if (!grdSubsidiary.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				grdSubsidiary.Select(hitTestInfo.Row, hitTestInfo.Column);
			}
		}
	}

	private void GrdSubsidiary_MouseClick(object sender, MouseEventArgs e)
	{
		C1FlexGrid grdSubsidiary = _owner.grdSubsidiary;
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		switch (grdSubsidiary.HitTest(e.Location).Type)
		{
		case HitTestTypeEnum.Cell:
		case HitTestTypeEnum.RowHeader:
			if (grdSubsidiary.MouseRow >= grdSubsidiary.Rows.Fixed)
			{
				ctxCell.ShowContextMenu(grdSubsidiary, e.Location);
			}
			break;
		case HitTestTypeEnum.None:
			ctxEmpty.ShowContextMenu(grdSubsidiary, e.Location);
			break;
		}
	}

	private void CmdAppendRow_Click(object sender, ClickEventArgs e)
	{
		C1FlexGrid grdSubsidiary = _owner.grdSubsidiary;
		grdSubsidiary.BeginUpdate();
		bool flag = eventAttached;
		try
		{
			if (flag)
			{
				DettachEvents();
			}
			grdSubsidiary.Rows.Add(1);
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
			grdSubsidiary.EndUpdate();
		}
	}

	private void CmdInsertRow_Click(object sender, ClickEventArgs e)
	{
		C1FlexGrid grdSubsidiary = _owner.grdSubsidiary;
		if (grdSubsidiary.MouseRow < grdSubsidiary.Rows.Fixed || grdSubsidiary.Row >= grdSubsidiary.Rows.Count)
		{
			return;
		}
		decimal? num = InputForm.Numeric("插入行", "请输入插入行数：");
		if (!num.HasValue)
		{
			return;
		}
		grdSubsidiary.BeginUpdate();
		bool flag = eventAttached;
		try
		{
			if (flag)
			{
				DettachEvents();
			}
			for (int i = 0; (decimal)i < num.Value; i++)
			{
				grdSubsidiary.Rows.Insert(grdSubsidiary.Row);
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
			grdSubsidiary.EndUpdate();
		}
	}

	private void CmdDeleteRow_Click(object sender, ClickEventArgs e)
	{
		C1FlexGrid grdSubsidiary = _owner.grdSubsidiary;
		grdSubsidiary.BeginUpdate();
		bool flag = eventAttached;
		try
		{
			if (flag)
			{
				DettachEvents();
			}
			int topRow = grdSubsidiary.Selection.TopRow;
			int count = grdSubsidiary.Selection.BottomRow - topRow + 1;
			for (int i = topRow; i < topRow + count && i < grdSubsidiary.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdSubsidiary.Rows[i];
				if (collectDic.ContainsKey(row))
				{
					collectDic.Remove(row);
				}
			}
			grdSubsidiary.Rows.RemoveRange(topRow, count);
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
			grdSubsidiary.EndUpdate();
		}
	}

	private void PopulateRowIndex()
	{
		bool flag = eventAttached;
		if (flag)
		{
			DettachEvents();
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

	public void PopulateCollects(IEnumerable<SubsidiaryItem> collects)
	{
		bool flag = eventAttached;
		if (flag)
		{
			DettachEvents();
		}
		try
		{
			collectDic.Clear();
			InitiliazeSubsidiaryGrid(_grid);
			if (collects == null || collects.Count() == 0)
			{
				return;
			}
			int @fixed = _grid.Rows.Fixed;
			foreach (SubsidiaryItem collect in collects)
			{
				if (@fixed >= _grid.Rows.Count)
				{
					_grid.Rows.Add();
				}
				Voucher voucher = collect.GetVoucher(_ledger, auditYear);
				if (voucher != null)
				{
					C1.Win.C1FlexGrid.Row row = _grid.Rows[@fixed++];
					row.UserData = voucher;
					row["Operation"] = OperationDic[collect.Operation];
					row["Code"] = voucher.Account.Code;
					row["Name"] = voucher.Account.Name;
					row["TypeNumber"] = collect.TypeNumber;
					row["Direction"] = (voucher.IsDebit ? "借方" : "贷方");
					row["Digest"] = voucher.Digest;
					decimal num = default(decimal);
					try
					{
						num = collect.GetValue(_ledger, auditYear);
						row["Value"] = num;
						collectDic.Add(row, collect);
					}
					catch (InvalidAuditYearException)
					{
						row["Value"] = num;
						collectDic.Add(row, collect);
					}
					catch (UnExpectAuditYearException)
					{
						row["Value"] = num;
						collectDic.Add(row, collect);
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

	private SubsidiaryItem GetSubsidiaryItem(C1.Win.C1FlexGrid.Row row)
	{
		SubsidiaryItem subsidiaryItem = null;
		if (row.UserData is Voucher voucher)
		{
			if (_start == default(DateTime))
			{
				return null;
			}
			if (_end == default(DateTime))
			{
				return null;
			}
			subsidiaryItem = new SubsidiaryItem();
			subsidiaryItem.AccountCode = voucher.Account.Code;
			subsidiaryItem.StartTime = _start;
			subsidiaryItem.EndTime = _end;
			subsidiaryItem.TypeNumber = voucher.Type.Name + voucher.Number;
			object operateText = row["Operation"];
			if (!OperationDic.ContainsValue(operateText?.ToString()))
			{
				return null;
			}
			subsidiaryItem.Operation = OperationDic.First((KeyValuePair<OperateEnum, string> t) => t.Value.Equals(operateText)).Key;
			int num = GetVoucherIndex(_ledger, voucher);
			if (num == -1)
			{
				return null;
			}
			subsidiaryItem.Index = num;
		}
		return subsidiaryItem;
		int GetVoucherIndex(Ledger ledger, Voucher vou)
		{
			SubsidiaryLedger subsidiaryLedger = ledger.GetSubsidiaryLedger(vou.Account, _start, _end);
			IEnumerable<Voucher> source = from t in subsidiaryLedger.Months.SelectMany((MonthSubsidiaryLedger t) => t.Entries)
				select t.Voucher into t
				where t.Type.Name.Equals(vou.Type.Name) && t.Number.Equals(vou.Number)
				select t;
			if (!source.Contains(vou))
			{
				return -1;
			}
			return source.ToList().IndexOf(vou);
		}
	}

	private void InitiliazeSubsidiaryGrid(C1FlexGrid grid)
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
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "Operation";
			column.Caption = "加减";
			column.ComboList = string.Join("|", OperationDic.Values.ToArray());
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
			column.Name = "TypeNumber";
			column.Caption = "凭证字号";
			column.DataType = typeof(string);
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column.AllowEditing = false;
			column = grid.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.AllowEditing = false;
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "Direction";
			column.Caption = "发生额方向";
			column.DataType = typeof(string);
			column.TextAlignFixed = TextAlignEnum.CenterCenter;
			column.AllowEditing = false;
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

	private void PopulateAccountTree(ComboTree comboTree, Ledger ledger)
	{
		comboTree.TreeView.BeginUpdate();
		comboTree.TreeView.Nodes.Clear();
		comboTree.Text = string.Empty;
		TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(_start, _end);
		foreach (Account rootAccount in ledger.RootAccounts)
		{
			string text = rootAccount.Code + rootAccount.Name;
			TreeNode treeNode = comboTree.TreeView.Nodes.Add(text);
			treeNode.Tag = rootAccount;
			AddChildren(rootAccount, treeNode);
		}
		comboTree.TreeView.EndUpdate();
		static void AddChildren(Account account, TreeNode node)
		{
			foreach (Account child in account.Children)
			{
				string text2 = child.Code + child.Name;
				TreeNode treeNode2 = node.Nodes.Add(text2);
				treeNode2.Tag = child;
				AddChildren(child, treeNode2);
			}
		}
	}

	private void PopulateSubsidiary(C1FlexGrid grid, Account account, DateTime start, DateTime end)
	{
		grid.BeginUpdate();
		try
		{
			grid.Cols.Count = 0;
			grid.Rows.Count = 1;
			grid.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grid.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "Date";
			column.Caption = "日期";
			column.DataType = typeof(DateTime);
			column = grid.Cols.Add();
			column.Name = "Type";
			column.Caption = "字";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "Number";
			column.Caption = "号";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "TypeNum";
			column.Caption = "字号";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "ItemClass";
			column.Caption = "辅助核算类别";
			column.DataType = typeof(string);
			column.Visible = false;
			column = grid.Cols.Add();
			column.Name = "ItemNumber";
			column.Caption = "辅助核算编号";
			column.DataType = typeof(string);
			column.Visible = false;
			column = grid.Cols.Add();
			column.Name = "ItemName";
			column.Caption = "辅助核算名称";
			column.DataType = typeof(string);
			column.Visible = false;
			column = grid.Cols.Add();
			column.Name = "Debit";
			column.Format = "#,0.00;-#,0.00;#";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column = grid.Cols.Add();
			column.Name = "Credit";
			column.Format = "#,0.00;-#,0.00;#";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column = grid.Cols.Add();
			column.Name = "DC";
			column.Caption = "方向";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "Balance";
			column.Format = "#,0.00;-#,0.00;#";
			column.Caption = "余额";
			column.DataType = typeof(decimal);
			grid.Rows.Fixed = 1;
			grid.Cols.Fixed = 1;
			int num = 1;
			SubsidiaryLedger subsidiaryLedger = _ledger.GetSubsidiaryLedger(account, start, end);
			foreach (MonthSubsidiaryLedger item in subsidiaryLedger.Months.OrderBy((MonthSubsidiaryLedger t) => t.Month))
			{
				List<SubsidiaryLedgerEntry> list = item.Entries.OrderBy((SubsidiaryLedgerEntry t) => t.Voucher.Day).ThenBy((SubsidiaryLedgerEntry s) => s.Voucher.Type.Name).ThenBy((SubsidiaryLedgerEntry m) => m.Voucher.Number, StringNumberComparer.Instance)
					.ToList();
				foreach (SubsidiaryLedgerEntry item2 in list)
				{
					C1.Win.C1FlexGrid.Row row = grid.Rows.Add();
					row.UserData = item2.Voucher;
					row["Index"] = num++.ToString();
					row["Date"] = item2.Voucher.Day;
					row["Type"] = item2.Voucher.Type.Name;
					row["Number"] = item2.Voucher.Number;
					row["TypeNum"] = item2.Voucher.Type.Name + "-" + item2.Voucher.Number;
					row["Digest"] = item2.Voucher.Digest;
					List<AuxiliaryItem> details = item2.Voucher.Details;
					if (details.Count > 0)
					{
						grid.Cols["ItemClass"].Visible = true;
						grid.Cols["ItemName"].Visible = true;
						grid.Cols["ItemNumber"].Visible = true;
						row["ItemClass"] = string.Join(",", details.Select((AuxiliaryItem aux) => aux.Class.Code));
						row["ItemNumber"] = string.Join(",", details.Select((AuxiliaryItem aux) => aux.Code));
						row["ItemName"] = string.Join(",", details.Select((AuxiliaryItem aux) => aux.Name));
					}
					row["Debit"] = (item2.Voucher.IsDebit ? item2.Voucher.Amount : 0m);
					row["Credit"] = (item2.Voucher.IsDebit ? 0m : item2.Voucher.Amount);
					row["DC"] = GetDCChar(account.IsDebit, item2.Balance);
					row["Balance"] = Math.Abs(item2.Balance);
				}
			}
			grid.AllowEditing = false;
			grid.AutoSizeCols();
		}
		finally
		{
			grid.EndUpdate();
		}
	}
}
