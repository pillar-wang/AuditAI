using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class FormNodesIndexEdit
{
	private const string CN_NODE = "CN_NODE";

	private const string CN_NUMBER = "CN_NUMBER";

	private readonly C1RibbonForm _form;

	private readonly C1SplitContainer _splAll;

	private readonly C1FlexGridEx _grid;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	private readonly C1SplitterPanel _pnlGrid;

	private readonly C1SplitterPanel _pnlButtons;

	private readonly C1.Win.C1FlexGrid.CellStyle Style_Group;

	private readonly C1ContextMenu ctxCell;

	private readonly C1Command cmdSequenceFill;

	private readonly C1CommandLink lnkSequenceFill;

	public Leqisoft.Model.Project Project { get; set; }

	public FormNodesIndexEdit()
	{
		_form = FormFactory.Create();
		_form.Text = "批量编辑索引号";
		_form.Size = new Size(471, 594);
		_form.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.EditNodesNumber16);
		_form.ShowInTaskbar = false;
		_splAll = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			ExtendLastCol = true,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowSorting = AllowSortingEnum.None,
			AutoClipboard = false,
			ClipboardCopyMode = ClipboardCopyModeEnum.Disabled,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None
		};
		_grid.Rows.Count = 1;
		_grid.Cols.Count = 0;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Fixed = 0;
		_grid.Tree.Column = 0;
		_grid.Rows.DefaultSize = 30;
		_grid.BodyBeforeEdit += _grid_BodyBeforeEdit;
		_grid.KeyDown += _grid_KeyDown;
		_grid.MouseDown += _grid_MouseDown;
		_grid.MouseClick += _grid_MouseClick;
		_grid.Paint += _grid_Paint;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Caption = StringConstBase.Current.Project + "文件";
		column.Name = "CN_NODE";
		column.AllowEditing = false;
		column.Width = 350;
		column = _grid.Cols.Add();
		column.Caption = "索引号";
		column.Name = "CN_NUMBER";
		column.TextAlign = TextAlignEnum.LeftCenter;
		Style_Group = _grid.Styles.Add("Style_Group");
		Style_Group.BackColor = UserSet.Config.TableStyle.LockAreaColor;
		_btnOk = new C1Button
		{
			Text = "确定",
			Dock = DockStyle.None,
			Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
			Location = new Point(-170, -35),
			Size = new Size(70, 26)
		};
		_btnOk.Click += _btnOk_Click;
		_btnCancel = new C1Button
		{
			Text = "取消",
			Dock = DockStyle.None,
			Location = new Point(-85, -35),
			Size = new Size(70, 26),
			Anchor = (AnchorStyles.Bottom | AnchorStyles.Right)
		};
		_btnCancel.Click += _btnCancel_Click;
		_pnlButtons = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Bottom,
			MinHeight = 0,
			Height = 40,
			KeepRelativeSize = false,
			Resizable = false,
			Collapsible = false,
			Text = ""
		};
		_pnlGrid = new C1SplitterPanel
		{
			Text = "",
			Resizable = false,
			KeepRelativeSize = true,
			SizeRatio = 100.0
		};
		_form.Controls.Add(_splAll);
		_splAll.Panels.Add(_pnlButtons);
		_splAll.Panels.Add(_pnlGrid);
		_pnlButtons.Controls.Add(_btnOk);
		_pnlButtons.Controls.Add(_btnCancel);
		_pnlGrid.Controls.Add(_grid);
		ctxCell = new C1ContextMenu();
		cmdSequenceFill = new C1Command
		{
			Text = "自动排号"
		};
		cmdSequenceFill.Click += CmdSequenceFill_Click;
		lnkSequenceFill = new C1CommandLink(cmdSequenceFill);
		ctxCell.CommandLinks.Add(lnkSequenceFill);
		Theme.SetCurrentTree(_form);
	}

	public DialogResult ShowDialog()
	{
		Populate();
		return _form.ShowDialog();
	}

	private void _grid_BodyBeforeEdit(object sender, RowColEventArgs e)
	{
		if (e.Row >= 0 && _grid.BodyGetRow(e.Row).UserData is Leqisoft.Model.TreeGroup && e.Col == _grid.Cols.IndexOf("CN_NUMBER"))
		{
			e.Cancel = true;
		}
	}

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyData)
		{
		case Keys.Delete:
		{
			C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
			for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
			{
				for (int j = bodySelection.LeftCol; j <= bodySelection.RightCol; j++)
				{
					if (j == _grid.Cols.IndexOf("CN_NUMBER") && CanEditRow(i))
					{
						_grid.BodySetData(i, j, string.Empty);
					}
				}
			}
			break;
		}
		case Keys.C | Keys.Control:
			Copy();
			break;
		case Keys.V | Keys.Control:
			Paste();
			break;
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (e.Button == MouseButtons.Right && hitTestInfo.Type == HitTestTypeEnum.Cell)
		{
			ctxCell.ShowContextMenu(_grid, e.Location);
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (e.Button == MouseButtons.Left && hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column == _grid.Cols.IndexOf("CN_NODE") && hitTestInfo.Row >= _grid.Rows.Fixed && hitTestInfo.Row < _grid.Rows.Count)
		{
			Node node = _grid.Rows[hitTestInfo.Row].Node;
			node.Collapsed = !node.Collapsed;
		}
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		_grid.DrawFormBorder(e.Graphics);
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		for (int i = 0; i < _grid.BodyRowsCount; i++)
		{
			if (_grid.BodyGetRow(i).UserData is TreeNodeBase treeNodeBase)
			{
				treeNodeBase.UpdateNumber(((string)_grid.BodyGetData(i, 1)) ?? string.Empty);
			}
		}
		_form.DialogResult = DialogResult.OK;
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.Cancel;
	}

	private void CmdSequenceFill_Click(object sender, ClickEventArgs e)
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		C1.Win.C1FlexGrid.Row row = _grid.BodyGetRow(bodySelection.TopRow);
		if (!(row.UserData is TreeNodeBase) || bodySelection.TopRow == bodySelection.BottomRow)
		{
			return;
		}
		int nextVisibleRow = GetNextVisibleRow(bodySelection.TopRow);
		if (nextVisibleRow >= _grid.BodyRowsCount || row.Node.Level != _grid.BodyGetRow(nextVisibleRow).Node.Level)
		{
			return;
		}
		string s = ((string)_grid.BodyGetData(bodySelection.TopRow, _grid.Cols.IndexOf("CN_NUMBER"))) ?? string.Empty;
		string s2 = ((string)_grid.BodyGetData(nextVisibleRow, _grid.Cols.IndexOf("CN_NUMBER"))) ?? string.Empty;
		SequenceInfo sequenceInfo = SequenceInfo.GetSequenceInfo(s, s2);
		if (sequenceInfo == null)
		{
			return;
		}
		int num = 0;
		int num2 = nextVisibleRow;
		while (num2 <= bodySelection.BottomRow)
		{
			num++;
			if (_grid.BodyGetRow(num2).Node.Level == row.Node.Level)
			{
				_grid.BodySetData(num2, _grid.Cols.IndexOf("CN_NUMBER"), sequenceInfo.GetNth(num));
				num2 = GetNextVisibleRow(num2);
				continue;
			}
			break;
		}
	}

	private int GetNextVisibleRow(int row)
	{
		for (int i = row + 1; i < _grid.BodyRowsCount; i++)
		{
			if (_grid.BodyGetRow(i).IsVisible)
			{
				return i;
			}
		}
		return _grid.BodyRowsCount;
	}

	private bool CanEditRow(int row)
	{
		return _grid.BodyGetRow(row).UserData is TreeNodeBase;
	}

	private void Copy()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		if (bodySelection.LeftCol > _grid.Cols.IndexOf("CN_NUMBER") || _grid.Cols.IndexOf("CN_NUMBER") > bodySelection.RightCol)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = bodySelection.TopRow; i <= bodySelection.BottomRow; i++)
		{
			if (_grid.BodyGetRow(i).IsVisible)
			{
				stringBuilder.AppendLine(((string)_grid.BodyGetData(i, _grid.Cols.IndexOf("CN_NUMBER"))) ?? string.Empty);
			}
		}
		try
		{
			Clipboard.SetText(stringBuilder.ToString());
		}
		catch (ExternalException)
		{
		}
	}

	private void Paste()
	{
		C1.Win.C1FlexGrid.CellRange bodySelection = _grid.BodySelection;
		try
		{
			List<List<object>> clipboardAsTable = ClipboardUtil.GetClipboardAsTable();
			if (clipboardAsTable == null)
			{
				return;
			}
			List<List<object>>.Enumerator enumerator = clipboardAsTable.GetEnumerator();
			int num = bodySelection.TopRow;
			while (num < _grid.BodyRowsCount && enumerator.MoveNext())
			{
				List<object> current = enumerator.Current;
				if (current.Count > 0 && CanEditRow(num))
				{
					_grid.BodySetData(num, _grid.Cols.IndexOf("CN_NUMBER"), current[0].ToString());
				}
				num = GetNextVisibleRow(num);
			}
		}
		catch (ExternalException)
		{
		}
	}

	private void Populate()
	{
		foreach (Leqisoft.Model.TreeGroup treeGroup in Project.TreeGroups)
		{
			Node node = _grid.Rows.AddNode(0);
			node.Data = treeGroup.Name;
			node.Key = treeGroup;
			node.Image = ContextResources.TreeGroup;
			_grid.SetCellStyle(node.Row.Index, "CN_NUMBER", Style_Group);
			foreach (TreeNodeBase rootNode in treeGroup.RootNodes)
			{
				if (!rootNode.Visible)
				{
					continue;
				}
				Node node2 = null;
				if (!(rootNode is TreeDirectoryNode dir2))
				{
					if (!(rootNode is TreeTableNode))
					{
						if (!(rootNode is TreeDocumentNode))
						{
							if (!(rootNode is TreeImageNode))
							{
								if (rootNode is TreePdfNode)
								{
									node2 = node.AddNode(NodeTypeEnum.LastChild, rootNode.Name, rootNode, Resources.TreePdf);
								}
							}
							else
							{
								node2 = node.AddNode(NodeTypeEnum.LastChild, rootNode.Name, rootNode, Resources.TreeImage);
							}
						}
						else
						{
							node2 = node.AddNode(NodeTypeEnum.LastChild, rootNode.Name, rootNode, Resources.TreeDoc);
						}
					}
					else
					{
						node2 = node.AddNode(NodeTypeEnum.LastChild, rootNode.Name, rootNode, Resources.TreeTable);
					}
				}
				else
				{
					node2 = node.AddNode(NodeTypeEnum.LastChild, rootNode.Name, rootNode, Resources.TreeDir);
					AddDirectoryNode(dir2, node2);
					node2.Collapsed = true;
				}
				_grid[node2.Row.Index, "CN_NUMBER"] = rootNode.Number;
			}
			node.Collapsed = true;
		}
		void AddDirectoryNode(TreeDirectoryNode dir, Node dirView)
		{
			foreach (TreeNodeBase child in dir.Children)
			{
				if (child.Visible)
				{
					Node node3 = null;
					if (!(child is TreeDirectoryNode dir3))
					{
						if (!(child is TreeTableNode))
						{
							if (!(child is TreeDocumentNode))
							{
								if (!(child is TreeImageNode))
								{
									if (child is TreePdfNode)
									{
										node3 = dirView.AddNode(NodeTypeEnum.LastChild, child.Name, child, Resources.TreePdf);
									}
								}
								else
								{
									node3 = dirView.AddNode(NodeTypeEnum.LastChild, child.Name, child, Resources.TreeImage);
								}
							}
							else
							{
								node3 = dirView.AddNode(NodeTypeEnum.LastChild, child.Name, child, Resources.TreeDoc);
							}
						}
						else
						{
							node3 = dirView.AddNode(NodeTypeEnum.LastChild, child.Name, child, Resources.TreeTable);
						}
					}
					else
					{
						node3 = dirView.AddNode(NodeTypeEnum.LastChild, child.Name, child, Resources.TreeDir);
						AddDirectoryNode(dir3, node3);
						node3.Collapsed = true;
					}
					_grid[node3.Row.Index, "CN_NUMBER"] = child.Number;
				}
			}
		}
	}
}
