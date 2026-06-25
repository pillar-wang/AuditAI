using System;
using System.Collections;
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
using C1.Win.C1SuperTooltip;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class frmAccessManage : C1RibbonForm
{
	private const string CN_NODE = "node";

	private const string CN_READ = "view";

	private const string CN_WRITE = "editor";

	private const string CN_SCHEMA = "schema";

	private const string CN_ROWWRITE = "rowwrite";

	private const string CN_ROWREAD = "rowread";

	private readonly C1ContextMenu ctxMenu = new C1ContextMenu();

	private readonly C1CommandLink lnkCollapse = new C1CommandLink();

	private readonly C1Command cmdCollapse = new C1Command();

	private readonly C1CommandLink lnkExpand = new C1CommandLink();

	private readonly C1Command cmdExpand = new C1Command();

	private AccessCheckedListBoxEditor editor;

	private List<CellValue> _cellChanged = new List<CellValue>();

	private Dictionary<long, string> _nameCache = new Dictionary<long, string>();

	private IContainer components;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlEditor;

	private C1FlexGridEx _grid;

	private C1SplitterPanel pnlButtons;

	private C1Button btnCancel;

	private C1Button btnCertain;

	private C1SuperLabel c1SuperLabel1;

	private C1SplitterPanel pnlSearch;

	private C1SplitContainer splSearch;

	private C1SplitterPanel pnlSearchIcon;

	private C1PictureBox picSearchIcon;

	private C1SplitterPanel pnlSearchTxt;

	private C1TextBox txbSearch;

	public Auditai.Model.Project Project { get; set; }

	public frmAccessManage()
	{
		InitializeComponent();
		base.Shown += FrmAccessManage_Shown;
		Initialize();
	}

	private void FrmAccessManage_Shown(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.AccessControl);
	}

	public new void Show()
	{
		Populate();
		base.Size = new Size(511, 500);
		base.Show();
	}

	public new DialogResult ShowDialog()
	{
		Populate();
		base.Size = new Size(850, 665);
		return base.ShowDialog();
	}

	private void btnCertain_Click(object sender, EventArgs e)
	{
		if (Program.MainForm != null && !Program.MainForm.CanAccessControl())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "禁止修改，您没有权限执行该操作!");
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
		else
		{
			if (_cellChanged == null)
			{
				return;
			}
			IEnumerable<Node> enumerable = from c in _cellChanged
				group c by c.Node into i
				select i.Key;
			foreach (Node item in enumerable)
			{
				TreeNodeBase treeNodeBase = item.Key as TreeNodeBase;
				if (treeNodeBase is TreePdfNode || treeNodeBase is TreeTableNode || treeNodeBase is TreeImageNode || treeNodeBase is TreeDocumentNode)
				{
					treeNodeBase.Permissions.Read = (item.Row["view"] as CellValue).Permission;
					treeNodeBase.Permissions.Write = (item.Row["editor"] as CellValue).Permission;
					treeNodeBase.Permissions.Schema = (item.Row["schema"] as CellValue).Permission;
					treeNodeBase.TagPermissionsDirty();
				}
			}
			for (int j = _grid.Rows.Fixed; j < _grid.Rows.Count; j++)
			{
				if (_grid.Rows[j].UserData is TreeTableNode treeTableNode)
				{
					treeTableNode.UpdateRowWrite((bool)_grid[j, "rowwrite"]);
					treeTableNode.UpdateRowRead((bool)_grid[j, "rowread"]);
				}
			}
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void GrdEditor_SetupEditor(object sender, RowColEventArgs e)
	{
		editor.C1EditorInitialize(_grid[e.Row, e.Col], null);
	}

	private void GrdEditor_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode != Keys.Delete)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				switch (_grid.Cols[j].Name)
				{
				case "view":
				case "editor":
				case "schema":
				{
					Permission permission = new Permission
					{
						GrantAll = false,
						Users = new List<long>()
					};
					if (!(_grid[i, j] is CellValue cellValue))
					{
						_grid[i, j] = new CellValue(_grid.Rows[i].Node, _grid.Cols[j].Name, permission);
					}
					else
					{
						cellValue.Permission = permission;
					}
					_cellChanged.Add(_grid[i, j] as CellValue);
					break;
				}
				case "rowwrite":
				case "rowread":
					if (_grid.GetCellCheck(i, j) != 0)
					{
						_grid.SetCellCheck(i, j, CheckEnum.Unchecked);
					}
					break;
				}
			}
		}
		Invalidate(invalidateChildren: true);
	}

	private void Editor_AfterCellChange(object sender, CellChangeEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = _grid.Rows[e.Cell.Node.Row.Index];
		object userData = row.UserData;
		Auditai.Model.TreeGroup treeGroup = userData as Auditai.Model.TreeGroup;
		if (treeGroup == null)
		{
			TreeDirectoryNode treeDirectoryNode = userData as TreeDirectoryNode;
			if (treeDirectoryNode == null)
			{
				_cellChanged.Add(e.Cell);
				goto IL_0076;
			}
		}
		updateChildren(row.Node, e.Cell.Col, e.Cell.Permission);
		goto IL_0076;
		IL_0076:
		_grid.Invalidate();
		void updateChildren(Node node, string col, Permission permission)
		{
			CellValue cellValue = node.Row[col] as CellValue;
			cellValue.Permission = (Permission)permission.Clone();
			_cellChanged.Add(cellValue);
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				updateChildren(node2, col, permission);
			}
		}
	}

	private void Editor_CellEditFinished(object sender, CellValue e)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		if (selection.IsSingleCell)
		{
			return;
		}
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				if (i != _grid.Row || j != _grid.Col)
				{
					if (!(_grid[i, j] is CellValue cellValue))
					{
						_grid[i, j] = new CellValue(_grid.Rows[i].Node, _grid.Cols[j].Name, (Permission)e.Permission.Clone());
					}
					else
					{
						cellValue.Permission = (Permission)e.Permission.Clone();
					}
					_cellChanged.Add(_grid[i, j] as CellValue);
				}
			}
		}
	}

	private void GrdEditor_Resize(object sender, EventArgs e)
	{
		_grid.BeginUpdate();
		KeepSize();
		_grid.EndUpdate();
	}

	private void GrdEditor_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (e.Button == MouseButtons.Left && hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column == 0)
		{
			Node node = _grid.Rows[hitTestInfo.Row].Node;
			node.Collapsed = !node.Collapsed;
		}
	}

	private void GrdEditor_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (!(_grid[e.Row, e.Col] is CellValue cellValue))
		{
			return;
		}
		if (cellValue.Node.Key is Auditai.Model.TreeGroup || cellValue.Node.Key is TreeDirectoryNode)
		{
			e.Text = string.Empty;
			return;
		}
		if (cellValue.Permission.GrantAll)
		{
			e.Text = "全体成员";
			return;
		}
		List<string> list = new List<string>();
		foreach (long user in cellValue.Permission.Users)
		{
			if (_nameCache.ContainsKey(user))
			{
				list.Add(_nameCache[user]);
			}
		}
		e.Text = string.Join("|", list);
	}

	private void TxbSearch_TextChanged(object sender, EventArgs e)
	{
		List<TreeNodeBase> list = (from n in Project.GetAllTreeNodes()
			select Tuple.Create(n, FuzzySearch.Filter(n.Name, txbSearch.Text)) into tup
			where tup.Item2 > 0
			orderby tup.Item2 descending
			select tup.Item1).ToList();
		HashSet<TreeNodeBase> hashSet = new HashSet<TreeNodeBase>(list);
		foreach (TreeNodeBase item3 in list)
		{
			for (TreeDirectoryNode treeDirectoryNode = item3.Parent; treeDirectoryNode != null; treeDirectoryNode = treeDirectoryNode.Parent)
			{
				hashSet.Add(treeDirectoryNode);
			}
		}
		HashSet<Auditai.Model.TreeGroup> hashSet2 = new HashSet<Auditai.Model.TreeGroup>();
		foreach (TreeNodeBase item4 in hashSet)
		{
			hashSet2.Add(item4.Group);
		}
		_grid.BeginUpdate();
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
			if (row.UserData is Auditai.Model.TreeGroup item)
			{
				row.Visible = hashSet2.Contains(item);
			}
			else if (row.UserData is TreeNodeBase item2)
			{
				if (hashSet.Contains(item2))
				{
					row.Visible = true;
					row.Node.Expanded = true;
				}
				else
				{
					row.Visible = false;
				}
			}
		}
		_grid.EndUpdate();
	}

	private void Initialize()
	{
		c1SuperLabel1.Text = "权限选项为空时，代表仅" + StringConstBase.Current.Manager + "具备相应权限。";
		editor = new AccessCheckedListBoxEditor(_grid);
		editor.AfterItemCheckedChange += Editor_AfterCellChange;
		editor.CellEditFinished += Editor_CellEditFinished;
		_grid.Dock = DockStyle.Fill;
		_grid.ExtendLastCol = false;
		_grid.AllowEditing = true;
		_grid.AllowSorting = AllowSortingEnum.None;
		_grid.SelectionMode = SelectionModeEnum.CellRange;
		_grid.Tree.Style = TreeStyleFlags.Simple;
		_grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		_grid.Styles.Normal.Border.Width = 0;
		_grid.Rows.DefaultSize = 30;
		_grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		_grid.Cols.Fixed = 0;
		_grid.Tree.Column = 0;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Caption = StringConstBase.Current.Project + "文件";
		column.Name = "node";
		column.AllowEditing = false;
		column.Width = TextRenderer.MeasureText("汉".PadLeft(19, '汉'), _grid.Font).Width;
		column = _grid.Cols.Add();
		column.Caption = "查看权限";
		column.Name = "view";
		column.AllowEditing = true;
		column.Editor = editor;
		column.Width = 100;
		column = _grid.Cols.Add();
		column.Caption = "编辑权限";
		column.Name = "editor";
		column.AllowEditing = true;
		column.Editor = editor;
		column.Width = 100;
		column = _grid.Cols.Add();
		column.Caption = "结构调整权限";
		column.Name = "schema";
		column.AllowEditing = true;
		column.Editor = editor;
		column.Width = 100;
		column = _grid.Cols.Add();
		column.Caption = "增行独占编辑保护";
		column.Name = "rowwrite";
		column.AllowEditing = true;
		column.DataType = typeof(bool);
		column.Width = 100;
		column = _grid.Cols.Add();
		column.Caption = "增行独占可见保护";
		column.Name = "rowread";
		column.AllowEditing = true;
		column.DataType = typeof(bool);
		column.Width = 100;
		_grid.DrawMode = DrawModeEnum.OwnerDraw;
		_grid.MouseClick += GrdEditor_MouseClick;
		_grid.SetupEditor += GrdEditor_SetupEditor;
		_grid.OwnerDrawCell += GrdEditor_OwnerDrawCell;
		_grid.Resize += GrdEditor_Resize;
		_grid.AfterResizeColumn += GrdEditor_Resize;
		_grid.KeyDown += GrdEditor_KeyDown;
		_grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		_grid.Paint += delegate(object s1, PaintEventArgs e1)
		{
			_grid.DrawFormBorder(e1.Graphics);
		};
		c1SuperLabel1.Text = "<html><body><p style='color:red;'>权限选项为空时，代表仅" + StringConstBase.Current.Manager + "具备相应权限。</p></body></html>";
		cmdExpand.Text = "全部展开";
		lnkExpand.Command = cmdExpand;
		cmdExpand.Click += delegate
		{
			ExpandAll();
		};
		cmdCollapse.Text = "全部收缩";
		lnkCollapse.Command = cmdCollapse;
		cmdCollapse.Click += delegate
		{
			CollapsedAll();
		};
		ctxMenu.CommandLinks.Add(lnkExpand);
		ctxMenu.CommandLinks.Add(lnkCollapse);
		C1CommandHolder c1CommandHolder = new C1CommandHolder
		{
			Owner = this
		};
		c1CommandHolder.SetC1ContextMenu(_grid, ctxMenu);
		txbSearch.TextChanged += TxbSearch_TextChanged;
		picSearchIcon.Size = new Size(txbSearch.Height, txbSearch.Height);
		pnlSearchIcon.Width = picSearchIcon.Width;
		pnlSearch.Height = txbSearch.Height;
	}

	public void ExpandAll()
	{
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)_grid.Rows)
		{
			if (item.IsNode)
			{
				item.Node.Collapsed = false;
			}
		}
	}

	public void CollapsedAll()
	{
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)_grid.Rows)
		{
			if (item.IsNode)
			{
				item.Node.Collapsed = true;
			}
		}
	}

	private Permissions GetDirectoryPermissions(TreeDirectoryNode d)
	{
		return GetCommonPermissionsImpl(d.GetDescendants());
	}

	private Permissions GetGroupPermissions(Auditai.Model.TreeGroup g)
	{
		return GetCommonPermissionsImpl(g.GetAllNodes());
	}

	private Permissions GetCommonPermissionsImpl(IEnumerable<TreeNodeBase> nodes)
	{
		Permissions permissions = new Permissions();
		List<Permissions> source = (from n in nodes
			where !(n is TreeDirectoryNode)
			select n.Permissions).ToList();
		if (source.All((Permissions p) => p.Read.GrantAll))
		{
			permissions.Read = new Permission
			{
				GrantAll = true
			};
		}
		else if (source.All((Permissions p) => !p.Read.GrantAll))
		{
			List<long> users = source.Select((Permissions p) => p.Read.Users.AsEnumerable()).Aggregate((IEnumerable<long> accu, IEnumerable<long> next) => accu.Intersect(next)).ToList();
			permissions.Read = new Permission
			{
				GrantAll = false,
				Users = users
			};
		}
		else
		{
			permissions.Read = new Permission
			{
				GrantAll = false
			};
		}
		if (source.All((Permissions p) => p.Write.GrantAll))
		{
			permissions.Write = new Permission
			{
				GrantAll = true
			};
		}
		else if (source.All((Permissions p) => !p.Write.GrantAll))
		{
			List<long> users2 = source.Select((Permissions p) => p.Write.Users.AsEnumerable()).Aggregate((IEnumerable<long> accu, IEnumerable<long> next) => accu.Intersect(next)).ToList();
			permissions.Write = new Permission
			{
				GrantAll = false,
				Users = users2
			};
		}
		else
		{
			permissions.Write = new Permission
			{
				GrantAll = false
			};
		}
		if (source.All((Permissions p) => p.Schema.GrantAll))
		{
			permissions.Schema = new Permission
			{
				GrantAll = true
			};
		}
		else if (source.All((Permissions p) => !p.Schema.GrantAll))
		{
			List<long> users3 = source.Select((Permissions p) => p.Schema.Users.AsEnumerable()).Aggregate((IEnumerable<long> accu, IEnumerable<long> next) => accu.Intersect(next)).ToList();
			permissions.Schema = new Permission
			{
				GrantAll = false,
				Users = users3
			};
		}
		else
		{
			permissions.Schema = new Permission
			{
				GrantAll = false
			};
		}
		return permissions;
	}

	public void Populate()
	{
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			_grid.Cols["schema"].Visible = false;
		}
		else
		{
			_grid.Cols["schema"].Visible = true;
		}
		_cellChanged = new List<CellValue>();
		_nameCache.Clear();
		editor.Items.Clear();
		editor.Items.Add(new Auditai.DTO.User
		{
			Id = 0L,
			Name = "全体成员"
		});
		foreach (Auditai.DTO.User item in Project.Users.Keys.Where((Auditai.DTO.User u) => u.Role != UserRole.Manager))
		{
			Member member = MemberManager.GetInstance().GetMember(item.Id.ToString());
			if (member != null && item.Name != member.Name)
			{
				item.Name = member.Name;
			}
			editor.Items.Add(item);
			_nameCache.Add(item.Id, item.Name);
		}
		_grid.BeginUpdate();
		try
		{
			_grid.Rows.Count = _grid.Rows.Fixed;
			foreach (Auditai.Model.TreeGroup treeGroup in Project.TreeGroups)
			{
				Node node = _grid.Rows.AddNode(0);
				node.Key = treeGroup;
				node.Data = treeGroup.Name;
				node.Image = ContextResources.TreeGroup;
				Permissions groupPermissions = GetGroupPermissions(treeGroup);
				node.Row["view"] = new CellValue(node, "view", groupPermissions.Read);
				node.Row["editor"] = new CellValue(node, "editor", groupPermissions.Write);
				node.Row["schema"] = new CellValue(node, "schema", groupPermissions.Schema);
				foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
				{
					Node node2 = null;
					if (!(rootNode is TreeDirectoryNode treeDirectoryNode))
					{
						if (!(rootNode is TreeTableNode treeTableNode))
						{
							if (!(rootNode is TreeDocumentNode treeDocumentNode))
							{
								if (!(rootNode is TreeImageNode treeImageNode))
								{
									if (rootNode is TreePdfNode treePdfNode)
									{
										node2 = node.AddNode(NodeTypeEnum.LastChild, treePdfNode.Number + " " + treePdfNode.Name, treePdfNode, Resources.TreePdf);
									}
								}
								else
								{
									node2 = node.AddNode(NodeTypeEnum.LastChild, treeImageNode.Number + " " + treeImageNode.Name, treeImageNode, Resources.TreeImage);
								}
							}
							else
							{
								node2 = node.AddNode(NodeTypeEnum.LastChild, treeDocumentNode.Number + " " + treeDocumentNode.Name + " ", treeDocumentNode, Resources.TreeDoc);
							}
						}
						else
						{
							node2 = node.AddNode(NodeTypeEnum.LastChild, treeTableNode.Number + " " + treeTableNode.Name, treeTableNode, Resources.TreeTable);
							node2.Row["rowwrite"] = treeTableNode.RowWrite;
							node2.Row["rowread"] = treeTableNode.RowRead;
						}
						if (node2 != null)
						{
							node2.Row["view"] = new CellValue(node2, "view", (Permission)rootNode.Permissions.Read.Clone());
							node2.Row["editor"] = new CellValue(node2, "editor", (Permission)rootNode.Permissions.Write.Clone());
							node2.Row["schema"] = new CellValue(node2, "schema", (Permission)rootNode.Permissions.Schema.Clone());
						}
					}
					else
					{
						node2 = node.AddNode(NodeTypeEnum.LastChild, treeDirectoryNode.Number + " " + treeDirectoryNode.Name, treeDirectoryNode, Resources.TreeDir);
						AddDirectoryNode(treeDirectoryNode, node2);
						Permissions directoryPermissions = GetDirectoryPermissions(treeDirectoryNode);
						node2.Row["view"] = new CellValue(node2, "view", directoryPermissions.Read);
						node2.Row["editor"] = new CellValue(node2, "editor", directoryPermissions.Write);
						node2.Row["schema"] = new CellValue(node2, "schema", directoryPermissions.Schema);
					}
				}
			}
			for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
			{
				_grid.Rows[i].Node.Collapsed = true;
			}
			_grid.Cols["node"].TextAlign = TextAlignEnum.LeftCenter;
			_grid.Cols[0].Width = TextRenderer.MeasureText("汉".PadLeft(19, '汉'), _grid.Font).Width;
			Auditai.UI.Controls.Theme.SetCurrentTree(this);
			SetTheme();
		}
		finally
		{
			_grid.EndUpdate();
		}
		void AddDirectoryNode(TreeDirectoryNode subRoot, Node subRootView)
		{
			foreach (TreeNodeBase child in subRoot.Children)
			{
				Node node3 = null;
				if (!(child is TreeDirectoryNode treeDirectoryNode2))
				{
					if (!(child is TreeTableNode treeTableNode2))
					{
						if (!(child is TreeDocumentNode treeDocumentNode2))
						{
							if (!(child is TreeImageNode treeImageNode2))
							{
								if (child is TreePdfNode treePdfNode2)
								{
									node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treePdfNode2.Number + " " + treePdfNode2.Name, treePdfNode2, Resources.TreePdf);
								}
							}
							else
							{
								node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeImageNode2.Number + " " + treeImageNode2.Name, treeImageNode2, Resources.TreeImage);
							}
						}
						else
						{
							node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeDocumentNode2.Number + " " + treeDocumentNode2.Name, treeDocumentNode2, Resources.TreeDoc);
						}
					}
					else
					{
						node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeTableNode2.Number + " " + treeTableNode2.Name, treeTableNode2, Resources.TreeTable);
						node3.Row["rowwrite"] = treeTableNode2.RowWrite;
						node3.Row["rowread"] = treeTableNode2.RowRead;
					}
					if (node3 != null)
					{
						node3.Row["view"] = new CellValue(node3, "view", (Permission)child.Permissions.Read.Clone());
						node3.Row["editor"] = new CellValue(node3, "editor", (Permission)child.Permissions.Write.Clone());
						node3.Row["schema"] = new CellValue(node3, "schema", (Permission)child.Permissions.Schema.Clone());
					}
				}
				else
				{
					node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeDirectoryNode2.Number + " " + treeDirectoryNode2.Name, treeDirectoryNode2, Resources.TreeDir);
					AddDirectoryNode(treeDirectoryNode2, node3);
					Permissions directoryPermissions2 = GetDirectoryPermissions(treeDirectoryNode2);
					node3.Row["view"] = new CellValue(node3, "view", directoryPermissions2.Read);
					node3.Row["editor"] = new CellValue(node3, "editor", directoryPermissions2.Write);
					node3.Row["schema"] = new CellValue(node3, "schema", directoryPermissions2.Schema);
				}
			}
		}
	}

	private void KeepSize()
	{
		int num = (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Vertical) ? SystemInformation.VerticalScrollBarWidth : 0);
		int num2 = base.Width - _grid.Cols["node"].Width - num;
		int num3 = 5;
		if (!_grid.Cols["schema"].Visible)
		{
			num3 = 4;
		}
		int num4 = num2 / num3;
		_grid.Cols["view"].Width = num4;
		_grid.Cols["editor"].Width = num4;
		_grid.Cols["schema"].Width = num4;
		_grid.Cols["rowwrite"].Width = num4;
		_grid.Cols["rowread"].Width = num4;
	}

	private void SetTheme()
	{
		ctnAll.SplitterWidth = 0;
		c1SuperLabel1.ForeColor = Color.Red;
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
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlSearch = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.splSearch = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlSearchIcon = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.picSearchIcon = new C1.Win.C1Input.C1PictureBox();
		this.pnlSearchTxt = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txbSearch = new C1.Win.C1Input.C1TextBox();
		this.pnlButtons = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1SuperLabel1 = new C1.Win.C1SuperTooltip.C1SuperLabel();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.btnCertain = new C1.Win.C1Input.C1Button();
		this.pnlEditor = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._grid = new Auditai.UI.Controls.C1FlexGridEx();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlSearch.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splSearch).BeginInit();
		this.splSearch.SuspendLayout();
		this.pnlSearchIcon.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.picSearchIcon).BeginInit();
		this.pnlSearchTxt.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txbSearch).BeginInit();
		this.pnlButtons.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).BeginInit();
		this.pnlEditor.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._grid).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.HeaderHeight = 27;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlSearch);
		this.ctnAll.Panels.Add(this.pnlButtons);
		this.ctnAll.Panels.Add(this.pnlEditor);
		this.ctnAll.Size = new System.Drawing.Size(842, 634);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.pnlSearch.Controls.Add(this.splSearch);
		this.pnlSearch.Height = 21;
		this.pnlSearch.KeepRelativeSize = false;
		this.pnlSearch.Location = new System.Drawing.Point(0, 0);
		this.pnlSearch.MinHeight = 21;
		this.pnlSearch.Name = "pnlSearch";
		this.pnlSearch.Resizable = false;
		this.pnlSearch.Size = new System.Drawing.Size(842, 21);
		this.pnlSearch.SizeRatio = 3.318;
		this.pnlSearch.TabIndex = 2;
		this.splSearch.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.splSearch.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.splSearch.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splSearch.HeaderHeight = 27;
		this.splSearch.Location = new System.Drawing.Point(0, 0);
		this.splSearch.Name = "splSearch";
		this.splSearch.Panels.Add(this.pnlSearchIcon);
		this.splSearch.Panels.Add(this.pnlSearchTxt);
		this.splSearch.Size = new System.Drawing.Size(842, 21);
		this.splSearch.SplitterWidth = 0;
		this.splSearch.TabIndex = 0;
		this.pnlSearchIcon.Controls.Add(this.picSearchIcon);
		this.pnlSearchIcon.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlSearchIcon.KeepRelativeSize = false;
		this.pnlSearchIcon.Location = new System.Drawing.Point(0, 0);
		this.pnlSearchIcon.MinHeight = 0;
		this.pnlSearchIcon.MinWidth = 0;
		this.pnlSearchIcon.Name = "pnlSearchIcon";
		this.pnlSearchIcon.Resizable = false;
		this.pnlSearchIcon.Size = new System.Drawing.Size(40, 21);
		this.pnlSearchIcon.TabIndex = 0;
		this.pnlSearchIcon.Width = 40;
		this.picSearchIcon.Image = Auditai.UI.Platform.Properties.Resources.btnSearch;
		this.picSearchIcon.Location = new System.Drawing.Point(0, 0);
		this.picSearchIcon.Name = "picSearchIcon";
		this.picSearchIcon.Size = new System.Drawing.Size(40, 21);
		this.picSearchIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
		this.picSearchIcon.TabIndex = 0;
		this.picSearchIcon.TabStop = false;
		this.pnlSearchTxt.Controls.Add(this.txbSearch);
		this.pnlSearchTxt.Height = 21;
		this.pnlSearchTxt.Location = new System.Drawing.Point(41, 0);
		this.pnlSearchTxt.Name = "pnlSearchTxt";
		this.pnlSearchTxt.Size = new System.Drawing.Size(801, 21);
		this.pnlSearchTxt.TabIndex = 1;
		this.txbSearch.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txbSearch.Location = new System.Drawing.Point(0, 0);
		this.txbSearch.Name = "txbSearch";
		this.txbSearch.Size = new System.Drawing.Size(801, 21);
		this.txbSearch.TabIndex = 0;
		this.txbSearch.Tag = null;
		this.pnlButtons.Controls.Add(this.c1SuperLabel1);
		this.pnlButtons.Controls.Add(this.btnCancel);
		this.pnlButtons.Controls.Add(this.btnCertain);
		this.pnlButtons.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButtons.Height = 40;
		this.pnlButtons.KeepRelativeSize = false;
		this.pnlButtons.Location = new System.Drawing.Point(0, 594);
		this.pnlButtons.MinHeight = 39;
		this.pnlButtons.MinWidth = 52;
		this.pnlButtons.Name = "pnlButtons";
		this.pnlButtons.Size = new System.Drawing.Size(842, 40);
		this.pnlButtons.SizeRatio = 5.548;
		this.pnlButtons.TabIndex = 1;
		this.pnlButtons.Width = 842;
		this.c1SuperLabel1.ForeColor = System.Drawing.Color.Red;
		this.c1SuperLabel1.Location = new System.Drawing.Point(10, 10);
		this.c1SuperLabel1.Name = "c1SuperLabel1";
		this.c1SuperLabel1.Size = new System.Drawing.Size(304, 23);
		this.c1SuperLabel1.TabIndex = 2;
		this.c1SuperLabel1.UseMnemonic = true;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(754, 9);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnCertain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCertain.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCertain.Location = new System.Drawing.Point(667, 9);
		this.btnCertain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCertain.Name = "btnCertain";
		this.btnCertain.Size = new System.Drawing.Size(70, 26);
		this.btnCertain.TabIndex = 0;
		this.btnCertain.Text = "确定";
		this.btnCertain.UseVisualStyleBackColor = true;
		this.btnCertain.Click += new System.EventHandler(btnCertain_Click);
		this.pnlEditor.Controls.Add(this._grid);
		this.pnlEditor.Height = 572;
		this.pnlEditor.Location = new System.Drawing.Point(0, 22);
		this.pnlEditor.MinHeight = 52;
		this.pnlEditor.MinWidth = 52;
		this.pnlEditor.Name = "pnlEditor";
		this.pnlEditor.Size = new System.Drawing.Size(842, 572);
		this.pnlEditor.TabIndex = 0;
		this.pnlEditor.Width = 842;
		this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle;
		this._grid.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this._grid.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this._grid.Location = new System.Drawing.Point(0, 0);
		this._grid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._grid.Name = "_grid";
		this._grid.Rows.DefaultSize = 20;
		this._grid.Size = new System.Drawing.Size(842, 572);
		this._grid.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(842, 634);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmAccessManage";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "权限控制";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlSearch.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splSearch).EndInit();
		this.splSearch.ResumeLayout(false);
		this.pnlSearchIcon.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.picSearchIcon).EndInit();
		this.pnlSearchTxt.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txbSearch).EndInit();
		this.pnlButtons.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).EndInit();
		this.pnlEditor.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this._grid).EndInit();
		base.ResumeLayout(false);
	}
}
