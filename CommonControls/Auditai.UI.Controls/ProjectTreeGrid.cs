using System;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class ProjectTreeGrid
{
	private C1FlexGridEx _grid;
	private C1ContextMenu _ctxMenu;
	private C1Command _cmdExpandAll;
	private C1Command _cmdCollapseAll;

	public Auditai.Model.Project Project { get; set; }

	public C1FlexGridEx View => _grid;

	public TreeNodeBase SelectedNode { get; private set; }

	public int Width
	{
		get
		{
			return _grid.Cols[0].Width;
		}
		set
		{
			_grid.Cols[0].Width = value;
		}
	}

	public event EventHandler<TreeNodeEventArgs> TreeNodeClicked;

	public event EventHandler<TreeNodeEventArgs> TreeNodeDoubleClicked;

	public event EventHandler<TreeNodeEventArgs> TreeNodeRightClicked;

	public ProjectTreeGrid()
	{
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			AllowEditing = false,
			ExtendLastCol = true,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			SelectionMode = SelectionModeEnum.Cell
		};
		_grid.Styles.Normal.Border.Width = 0;
		_grid.Tree.Style = TreeStyleFlags.CompleteLeaf;
		_grid.Rows.DefaultSize = 30;
		_grid.Rows.Count = 0;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 1;
		_grid.Cols.Fixed = 0;
		_grid.Tree.Column = 0;
		_grid.MouseClick += _grid_MouseClick;
		_grid.MouseDoubleClick += _grid_MouseDoubleClick;
		_grid.Paint += _grid_Paint;

		_cmdExpandAll = new C1Command();
		_cmdExpandAll.Text = "全部展开";
		_cmdExpandAll.Click += (s, args) => _grid.ExpandAll();

		_cmdCollapseAll = new C1Command();
		_cmdCollapseAll.Text = "全部收缩";
		_cmdCollapseAll.Click += (s, args) => _grid.CollapseAll();

		_ctxMenu = new C1ContextMenu();
		_ctxMenu.CommandLinks.Add(new C1CommandLink(_cmdExpandAll));
		_ctxMenu.CommandLinks.Add(new C1CommandLink(_cmdCollapseAll));
	}

	public void Populate()
	{
		SelectedNode = null;
		_grid.BeginUpdate();
		_grid.Rows.Count = _grid.Rows.Fixed;
		foreach (Auditai.Model.TreeGroup treeGroup in Project.TreeGroups)
		{
			Node node = _grid.Rows.AddNode(0);
			node.Key = treeGroup;
			node.Data = treeGroup.Name;
			node.Image = Resources.TreeGroup;
			node.Row.UserData = treeGroup;
			foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
			{
				Node node2 = null;
				if (!(rootNode is TreeDirectoryNode treeDirectoryNode))
				{
					if (!(rootNode is TreeTableNode treeTableNode))
					{
						if (rootNode is TreeDocumentNode treeDocumentNode)
						{
							node2 = node.AddNode(NodeTypeEnum.LastChild, treeDocumentNode.Name, treeDocumentNode, Resources.TreeDoc);
							if (node2 != null)
							{
								node2.Row.UserData = treeDocumentNode;
							}
						}
					}
					else
					{
						node2 = node.AddNode(NodeTypeEnum.LastChild, treeTableNode.Name, treeTableNode, Resources.TreeTable);
						if (node2 != null)
						{
							node2.Row.UserData = treeTableNode;
						}
					}
				}
				else
				{
					node2 = node.AddNode(NodeTypeEnum.LastChild, treeDirectoryNode.Name, treeDirectoryNode, Resources.TreeDir);
					if (node2 != null)
					{
						node2.Row.UserData = treeDirectoryNode;
					}
					AddDirectoryNode(treeDirectoryNode, node2);
				}
				if (!rootNode.Visible && node2 != null)
				{
					node2.Row.Visible = false;
				}
			}
			node.Collapsed = true;
		}
		_grid.EndUpdate();
		static void AddDirectoryNode(TreeDirectoryNode subRoot, Node subRootView)
		{
			foreach (TreeNodeBase child in subRoot.Children)
			{
				Node node3 = null;
				if (!(child is TreeDirectoryNode treeDirectoryNode2))
				{
					if (!(child is TreeTableNode treeTableNode2))
					{
						if (child is TreeDocumentNode treeDocumentNode2)
						{
							node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeDocumentNode2.Name, treeDocumentNode2, Resources.TreeDoc);
							if (node3 != null)
							{
								node3.Row.UserData = treeDocumentNode2;
							}
						}
					}
					else
					{
						node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeTableNode2.Name, treeTableNode2, Resources.TreeTable);
						if (node3 != null)
						{
							node3.Row.UserData = treeTableNode2;
						}
					}
				}
				else
				{
					node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeDirectoryNode2.Name, treeDirectoryNode2, Resources.TreeDir);
					if (node3 != null)
					{
						node3.Row.UserData = treeDirectoryNode2;
					}
					AddDirectoryNode(treeDirectoryNode2, node3);
				}
				if (!child.Visible && node3 != null)
				{
					node3.Row.Visible = false;
				}
			}
			subRootView.Expanded = false;
		}
	}

	public void FindAndSelectNodeById(Id64 nodeId)
	{
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
			if (row.UserData is TreeNodeBase treeNodeBase && treeNodeBase.Id == nodeId)
			{
				SelectedNode = treeNodeBase;
				_grid.Row = i;
				break;
			}
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Node node = _grid.Rows[hitTestInfo.Row].Node;
			if (node != null)
			{
				TreeNodeBase tnb = (SelectedNode = node.Key as TreeNodeBase);
				if (e.Button == MouseButtons.Right)
				{
					this.TreeNodeRightClicked?.Invoke(this, new TreeNodeEventArgs(tnb));
					_ctxMenu.ShowContextMenu(_grid, e.Location);
				}
				else
				{
					this.TreeNodeClicked?.Invoke(this, new TreeNodeEventArgs(tnb));
				}
			}
		}
		if (e.Button == MouseButtons.Left && hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column == _grid.Tree.Column)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
			if (row.IsNode)
			{
				Node node2 = row.Node;
				node2.Collapsed = !node2.Collapsed;
			}
		}
	}

	private void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			Node node = _grid.Rows[hitTestInfo.Row].Node;
			if (node != null)
			{
				TreeNodeBase tnb = (SelectedNode = node.Key as TreeNodeBase);
				this.TreeNodeDoubleClicked?.Invoke(this, new TreeNodeEventArgs(tnb));
			}
		}
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		_grid.DrawFormBorder(e.Graphics);
	}
}
