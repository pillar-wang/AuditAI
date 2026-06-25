using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

public class AccountAndAuxiliarySelectEditor : C1FlexGrid, IC1EmbeddedEditor
{
	private Ledger ledger;

	private Rectangle rect;

	private object oldValue;

	private C1FlexGrid owner;

	private LedgerViewer _viewer;

	private Tuple<Account, IEnumerable<AuxiliaryItem>> selectedValue;

	private bool _eventAttached;

	public AccountAndAuxiliarySelectEditor(LedgerViewer viewer, C1FlexGrid owner, Ledger ledger)
	{
		AccountAndAuxiliarySelectEditor accountAndAuxiliarySelectEditor = this;
		_viewer = viewer;
		this.owner = owner;
		this.ledger = ledger;
		owner.SetupEditor += delegate
		{
			Rectangle r = accountAndAuxiliarySelectEditor.rect;
			accountAndAuxiliarySelectEditor.Location = new Point(r.Left, r.Bottom);
			accountAndAuxiliarySelectEditor.Width = 300;
			accountAndAuxiliarySelectEditor.Height = owner.Height - r.Bottom;
		};
		base.MouseDown += AuxiliarySelector_MouseDown;
		InitializeAccountTree(ledger);
		AttachEvent();
		base.Paint += delegate(object s1, PaintEventArgs e1)
		{
			accountAndAuxiliarySelectEditor.DrawFormBorder(e1.Graphics);
		};
		Theme.SetCurrentObject(this);
		base.Styles.Normal.Border.Color = Color.White;
	}

	private void AttachEvent()
	{
		if (!_eventAttached)
		{
			base.CellChecked += AuxiliarySelectEditor_CellChecked;
			_eventAttached = true;
		}
	}

	private void DettachEvent()
	{
		if (_eventAttached)
		{
			base.CellChecked -= AuxiliarySelectEditor_CellChecked;
			_eventAttached = false;
		}
	}

	private void AuxiliarySelectEditor_CellChecked(object sender, RowColEventArgs e)
	{
		BeginUpdate();
		try
		{
			DettachEvent();
			CheckEnum cellCheck = GetCellCheck(e.Row, e.Col);
			if (cellCheck != CheckEnum.Checked)
			{
				return;
			}
			Node node = base.Rows[e.Row].Node.Parent;
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				if (node2.Row.Index != e.Row)
				{
					node2.Checked = CheckEnum.Unchecked;
				}
			}
			Account account = node.Parent.Key as Account;
			for (int j = 0; j < base.Rows.Count; j++)
			{
				Node node3 = base.Rows[j].Node;
				if (node3.Checked == CheckEnum.Checked && node3.Key is Tuple<Account, AuxiliaryItem> tuple && tuple.Item1 != account)
				{
					node3.Checked = CheckEnum.Unchecked;
				}
			}
		}
		finally
		{
			AttachEvent();
			EndUpdate();
		}
	}

	public string C1EditorFormat(object value, string mask)
	{
		return string.Empty;
	}

	public UITypeEditorEditStyle C1EditorGetStyle()
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public object C1EditorGetValue()
	{
		return selectedValue;
	}

	public void C1EditorInitialize(object value, IDictionary editorAttributes)
	{
		oldValue = value;
		for (int i = 0; i < base.Rows.Count; i++)
		{
			Node node = base.Rows[i].Node;
			if (node.Checked == CheckEnum.Checked)
			{
				node.Checked = CheckEnum.Unchecked;
			}
			node.Collapsed = true;
		}
		if (oldValue is Tuple<Account, IEnumerable<AuxiliaryItem>> { Item2: not null } tuple)
		{
			for (int j = 0; j < base.Rows.Count; j++)
			{
				Node node2 = base.Rows[j].Node;
				if (node2.Checked == CheckEnum.Unchecked && node2.Key is Tuple<Account, AuxiliaryItem> tuple2 && tuple.Item1 == tuple2.Item1 && tuple.Item2.Contains(tuple2.Item2))
				{
					node2.Checked = CheckEnum.Checked;
				}
			}
		}
		selectedValue = null;
	}

	public bool C1EditorKeyDownFinishEdit(KeyEventArgs e)
	{
		return true;
	}

	public void C1EditorUpdateBounds(Rectangle rc)
	{
		rect = rc;
	}

	public bool C1EditorValueIsValid()
	{
		if (selectedValue != null)
		{
			return true;
		}
		Account item = null;
		List<AuxiliaryItem> list = new List<AuxiliaryItem>();
		for (int i = 0; i < base.Rows.Count; i++)
		{
			Node node = base.Rows[i].Node;
			if (node.Checked == CheckEnum.Checked && node.Key is Tuple<Account, AuxiliaryItem> tuple)
			{
				item = tuple.Item1;
				list.Add(tuple.Item2);
			}
		}
		if (list.Count > 0)
		{
			selectedValue = Tuple.Create(item, (IEnumerable<AuxiliaryItem>)list);
			return true;
		}
		if (oldValue is Tuple<Account, IEnumerable<AuxiliaryItem>> { Item2: not null })
		{
			return true;
		}
		selectedValue = oldValue as Tuple<Account, IEnumerable<AuxiliaryItem>>;
		return true;
	}

	private void InitializeAccountTree(Ledger ledger)
	{
		BeginUpdate();
		try
		{
			DettachEvent();
			base.Rows.Count = 0;
			base.Rows.Fixed = 0;
			base.Cols.Count = 1;
			base.Cols.Fixed = 0;
			base.Tree.Column = 0;
			base.Rows.DefaultSize = 30;
			foreach (Account item in ledger.RootAccounts.OrderBy((Account a) => a.Code))
			{
				addChildren(null, item);
			}
			base.ExtendLastCol = true;
			base.SelectionMode = SelectionModeEnum.Row;
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
			base.Tree.Show(0);
		}
		finally
		{
			AttachEvent();
			EndUpdate();
		}
		void addChildren(Node parentNode, Account account)
		{
			Node node = ((parentNode == null) ? base.Rows.AddNode(0) : parentNode.AddNode(NodeTypeEnum.LastChild, null));
			node.Data = account.Code + " " + account.Name;
			node.Key = account;
			node.Collapsed = true;
			node.Row.AllowEditing = false;
			AccountBalance accountBalanceWithCache = _viewer.CacheManager.GetAccountBalanceWithCache(ledger, account);
			bool flag = accountBalanceWithCache.ClassBalances.Count > 1;
			foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in accountBalanceWithCache.ClassBalances)
			{
				AuxiliaryClass key = classBalance.Key;
				Node node2 = node.AddNode(NodeTypeEnum.LastChild, string.Empty);
				node2.Data = key.Code + " " + key.Name;
				node2.Key = Tuple.Create(account, key);
				node2.Collapsed = true;
				node2.Row.AllowEditing = false;
				foreach (AuxiliaryItem key2 in classBalance.Value.ItemBalances.Keys)
				{
					Node node3 = node2.AddNode(NodeTypeEnum.LastChild, string.Empty);
					node3.Data = key2.Code + " " + key2.Name;
					node3.Key = Tuple.Create(account, key2);
					node3.Collapsed = true;
					node3.Row.AllowEditing = false;
					if (flag)
					{
						SetCellCheck(node3.Row.Index, 0, CheckEnum.Unchecked);
						node3.Row.AllowEditing = true;
					}
				}
			}
			foreach (Account child in account.Children)
			{
				addChildren(node, child);
			}
		}
	}

	private void AuxiliarySelector_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell && base.Row >= base.Rows.Fixed && base.Row < base.Rows.Count)
		{
			Node node = base.Rows[hitTestInfo.Row].Node;
			if (node.Children > 0)
			{
				node.Collapsed = !node.Collapsed;
			}
			else if (node.Key is Account item)
			{
				selectedValue = Tuple.Create<Account, IEnumerable<AuxiliaryItem>>(item, null);
				owner.FinishEditing();
			}
			else if (node.Key is Tuple<Account, AuxiliaryItem> tuple && node.Checked == CheckEnum.None)
			{
				selectedValue = Tuple.Create(tuple.Item1, (IEnumerable<AuxiliaryItem>)new AuxiliaryItem[1] { tuple.Item2 });
				owner.FinishEditing();
			}
		}
	}
}
