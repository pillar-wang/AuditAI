using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class TreeListDropDownForm : ListDropDownFormBase
{
	private TreeListOperand _op;

	private Dictionary<TreeListNode, int> _leaves = new Dictionary<TreeListNode, int>();

	public C1FlexGridEx Grid { get; }

	public override Operand Op
	{
		get
		{
			return _op;
		}
		set
		{
			_op = (TreeListOperand)value;
		}
	}

	public TreeListDropDownForm(ListDropDown owner)
		: base(owner)
	{
		Grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowResizing = AllowResizingEnum.None,
			AllowEditing = false,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.None,
			AllowMergingFixed = AllowMergingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			Font = new Font("微软雅黑", 9f),
			ExtendLastCol = true
		};
		Grid.Cols.Count = 1;
		Grid.Cols.Fixed = 0;
		Grid.Rows.Count = 0;
		Grid.Rows.Fixed = 0;
		Grid.Tree.Column = 0;
		Grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		base.Form.Controls.Add(Grid);
		Grid.MouseClick += Grid_MouseClick;
		Grid.MouseMove += Grid_MouseMove;
		Grid.MouseWheel += Grid_MouseWheel;
		Grid.BodyOwnerDrawCell += Grid_BodyOwnerDrawCell;
	}

	private void Grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = Grid.BodyGetRow(e.Row);
		if (row.IsNode && row.Node.Children > 0)
		{
			if (row.Node.Collapsed)
			{
				e.Image = Resources.TreeListCollapsed;
			}
			else
			{
				e.Image = Resources.TreeListExpanded;
			}
		}
	}

	private void Grid_MouseWheel(object sender, MouseEventArgs e)
	{
		Point scrollPosition = Grid.ScrollPosition;
		scrollPosition.Offset(0, e.Delta);
		Grid.ScrollPosition = scrollPosition;
	}

	private void Grid_MouseMove(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Grid.Row = hitTestInfo.Row;
		}
	}

	private void Grid_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Grid.HitTest(e.Location);
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Node node = Grid.Rows[hitTestInfo.Row].Node;
			if (node.Children == 0)
			{
				base.Text = (string)Grid[hitTestInfo.Row, hitTestInfo.Column];
				CloseDropDown();
				_owner.FinishEditing();
			}
			else
			{
				node.Collapsed = !node.Collapsed;
			}
		}
	}

	protected override int GetTotalWidth()
	{
		int num = 0;
		int count = Grid.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			Node node = Grid.Rows[i].Node;
			int num2 = ((node.Data != null) ? node.Data.ToString().Length : 0);
			int num3 = num2 * 14 + ((node.Image != null) ? node.Image.Width : 0) + (node.Level + 1) * 14;
			if (num3 > num)
			{
				num = num3;
			}
			if (i >= 10000)
			{
				break;
			}
		}
		if (num > 500)
		{
			num = 500;
		}
		return num;
	}

	public override void Populate()
	{
		_leaves.Clear();
		Grid.BeginUpdate();
		Grid.Rows.Count = 0;
		Grid.Rows.Count = GetRowsTotalCount();
		int i = 0;
		int level = 0;
		foreach (TreeListNode root in _op.Roots)
		{
			AddNode(root);
		}
		Grid.EndUpdate();
		void AddNode(TreeListNode node)
		{
			Grid.BodyGetRow(i).IsNode = true;
			Node node2 = Grid.BodyGetRow(i).Node;
			node2.Level = level;
			node2.Data = node.Text;
			i++;
			if (node.Children.Count > 0)
			{
				foreach (TreeListNode child in node.Children)
				{
					level++;
					AddNode(child);
					level--;
				}
				node2.Image = Resources.TreeListCollapsed;
				node2.Collapsed = true;
			}
			else
			{
				node2.Image = Resources.TreeListLeaf3;
				_leaves.Add(node, node2.Row.Index);
			}
		}
	}

	private int GetRowsTotalCount()
	{
		int num = 0;
		foreach (TreeListNode root in _op.Roots)
		{
			num += GetNodesCount(root);
		}
		return num;
		static int GetNodesCount(TreeListNode node)
		{
			if (node.Children.Count == 0)
			{
				return 1;
			}
			int num2 = 1;
			foreach (TreeListNode child in node.Children)
			{
				num2 += GetNodesCount(child);
			}
			return num2;
		}
	}

	private void AddNode(Node parent, TreeListNode n)
	{
		Node node;
		if (parent == null)
		{
			node = Grid.Rows.AddNode(0);
			node.Data = n.Text;
		}
		else
		{
			node = parent.AddNode(NodeTypeEnum.LastChild, n.Text);
		}
		if (n.Children.Count == 0)
		{
			node.Image = Resources.TreeListLeaf3;
			_leaves.Add(n, node.Row.Index);
			return;
		}
		node.Image = Resources.TreeListCollapsed;
		foreach (TreeListNode child in n.Children)
		{
			AddNode(node, child);
		}
	}

	public void Filter(string s)
	{
		Grid.BeginUpdate();
		foreach (KeyValuePair<TreeListNode, int> leaf in _leaves)
		{
			if (leaf.Key.Text.Contains(s))
			{
				Grid.Rows[leaf.Value].Node.EnsureVisible();
			}
			else
			{
				Grid.Rows[leaf.Value].Visible = false;
			}
		}
		Node[] nodes = Grid.Nodes;
		foreach (Node node in nodes)
		{
			node.Row.Visible = SetNodeVisible(node);
			if (node.Row.Visible)
			{
				Grid.Row = node.Row.Index;
			}
		}
		Grid.EndUpdate();
	}

	private bool SetNodeVisible(Node n)
	{
		if (n.Children == 0)
		{
			return n.Row.Visible;
		}
		bool flag = false;
		Node[] nodes = n.Nodes;
		foreach (Node nodeVisible in nodes)
		{
			flag |= SetNodeVisible(nodeVisible);
		}
		return flag;
	}

	public void Up(int count)
	{
		int num = count;
		int num2 = Grid.Row;
		while (num > 0)
		{
			num2--;
			if (num2 < 0)
			{
				Grid.Row = 0;
				return;
			}
			if (Grid.Rows[num2].IsVisible)
			{
				num--;
			}
		}
		Grid.Row = num2;
	}

	public void Down(int count)
	{
		int num = count;
		int num2 = Grid.Row;
		while (num > 0)
		{
			num2++;
			if (num2 >= Grid.Rows.Count)
			{
				Grid.Row = Grid.Rows.Count - 1;
				return;
			}
			if (Grid.Rows[num2].IsVisible)
			{
				num--;
			}
		}
		Grid.Row = num2;
	}

	public bool IsLeaf()
	{
		if (Grid.IsCellValid(Grid.Row, 0))
		{
			return Grid.Rows[Grid.Row].Node.Children == 0;
		}
		return false;
	}

	public string GetText()
	{
		if (Grid.IsCellValid(Grid.Row, 0))
		{
			return (string)Grid[Grid.Row, 0];
		}
		return string.Empty;
	}

	public override void OnTextChanged(string t)
	{
		Filter(t);
	}

	public override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Up:
			e.Handled = true;
			Up(1);
			if (IsLeaf())
			{
				_owner.SkipTextChanged = true;
				base.Text = GetText();
				_owner.DropDown.SelectAll();
				_owner.SkipTextChanged = false;
			}
			break;
		case Keys.Down:
			e.Handled = true;
			Down(1);
			if (IsLeaf())
			{
				_owner.SkipTextChanged = true;
				base.Text = GetText();
				_owner.DropDown.SelectAll();
				_owner.SkipTextChanged = false;
			}
			break;
		case Keys.Prior:
			e.Handled = true;
			Up(10);
			break;
		case Keys.Next:
			e.Handled = true;
			Down(10);
			break;
		}
	}

	public override bool Validate()
	{
		return _leaves.Keys.Any((TreeListNode n) => n.Text == base.Text);
	}

	public override void OnSetTheme()
	{
		Grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		Grid.Rows.DefaultSize = TextRenderer.MeasureText("啊", Grid.Font).Height + 10;
	}
}
