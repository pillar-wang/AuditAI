﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class TableNavGrid : ISetTheme
{
	protected class FindRefTitleCellResolver : FormulaReferenceTableTitleCellResolver
	{
		protected Id64 _checkTableId;

		public HashSet<TableTitleCell> tableTitleCellsSet = new HashSet<TableTitleCell>();

		public FindRefTitleCellResolver(Id64 checkTableId)
		{
			_checkTableId = checkTableId;
		}

		public override TableTitleCell GetTableTitleCell(Auditai.Model.Table table, int row, int col)
		{
			TableTitle title = table.LoadAndReturn().Title;
			TableTitleCell cell = title.GetCell(row, col);
			if (table.Id == _checkTableId && cell != null)
			{
				tableTitleCellsSet.Add(cell);
			}
			return cell;
		}
	}

	protected class VirtualTableTitleCellResolver : FormulaReferenceTableTitleCellResolver
	{
		protected Id64 _checkTableId;

		protected VirtualTable _vritualTable;

		public VirtualTableTitleCellResolver(Id64 checkTableId, VirtualTable virtualTable)
		{
			_checkTableId = checkTableId;
			_vritualTable = virtualTable;
		}

		public override TableTitleCell GetTableTitleCell(Auditai.Model.Table table, int row, int col)
		{
			if (table.Id == _checkTableId && _vritualTable != null)
			{
				return _vritualTable.Title.GetCell(row, col);
			}
			TableTitle title = table.LoadAndReturn().Title;
			return title.GetCell(row, col);
		}
	}

	public class NavNode
	{
		public string Text { get; set; }

		public List<NavNode> Children { get; } = new List<NavNode>();


		public Dictionary<string, NavNode> Hash { get; } = new Dictionary<string, NavNode>();


		public TableTitleCell TitleCell { get; set; }

		public object DisplayStringRealValue { get; set; }

		public ValueOperand RealValueOnTitleCellSameType { get; set; }

		public NavNode AddOrGet(string text)
		{
			if (!Hash.TryGetValue(text, out var value))
			{
				value = new NavNode
				{
					Text = text
				};
				Children.Add(value);
				Hash.Add(text, value);
			}
			return value;
		}

		public NavNode AddLastLevel(string text)
		{
			NavNode navNode = new NavNode
			{
				Text = text
			};
			Children.Add(navNode);
			return navNode;
		}

		public NavNode AddIfNotExist(string text)
		{
			if (Hash.TryGetValue(text, out var value))
			{
				return null;
			}
			value = new NavNode
			{
				Text = text
			};
			Children.Add(value);
			Hash.Add(text, value);
			return value;
		}

		public int GetRowCount()
		{
			return Children.Sum((NavNode c) => c.GetRowCount()) + 1;
		}
	}

	private readonly C1FlexGridEx _grid;

	private readonly SolidBrush _brushHoverBackground = new SolidBrush(Color.Transparent);

	private readonly SolidBrush _gridMouseOverMoreMenuIconBrush = new SolidBrush(Color.Black);

	private readonly C1Command _cmdDeleteCurrentLevel;

	private readonly C1CommandLink _lnkDeleteCurrentLevel;

	private readonly C1Command _cmdExpandAll;

	private readonly C1CommandLink _lnkExpandAll;

	private readonly C1Command _cmdCollapseAll;

	private readonly C1CommandLink _lnkCollapseAll;

	private readonly C1Command _cmdUpLevel;

	private readonly C1CommandLink _lnkUpLevel;

	private readonly C1Command _cmdDownLevel;

	private readonly C1CommandLink _lnkDownLevel;

	protected C1ContextMenu _gridCtx;

	private int _mouseRow = -1;

	protected Color _colorPositionLine;

	protected Color _contentTextDefaultColor = Color.Black;

	protected bool _isNavTreeShowInComblistTreeMode;

	private bool _isMouseOverMoreMenuIcon;

	private static Bitmap _menuMoreOperationWhiteImage;

	protected HashSet<TableTitleCell> _relatedTitleCellSet = new HashSet<TableTitleCell>();

	protected Dictionary<TableTitleCell, List<TableTitleCell>> _cellRelatedOtherCellDic = new Dictionary<TableTitleCell, List<TableTitleCell>>();

	protected Color _NodeTextColor = Color.FromArgb(0, 0, 0);

	protected List<TableTitleCell> _finalNav;

	public const int VirtualNodeMaxCount = 20000;

	private NavNode _mouseRightClickNavNode;

	private bool _isSuspendSelectChangeEvent;

	private bool _isOnlyRunGridDefaultSelectEventProcess;

	private bool _shouldSkipBodyAfterRowColChange;

	private int _hasFilledVirtualNodeCount;

	public C1FlexGridEx View => _grid;

	public List<string> Nav { get; set; }

	public Auditai.Model.Table Table { get; set; }

	public List<Tuple<TableTitleCell, object>> SelectedTitleCells { get; protected set; }

	public event EventHandler TreeLeafNodeSelected;

	public TableNavGrid()
	{
		ThemeManager.GetInstance().Register(this);
		_grid = new C1FlexGridEx
		{
			Dock = DockStyle.Fill,
			ExtendLastCol = true,
			BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
			AllowEditing = false,
			SelectionMode = SelectionModeEnum.Row,
			AllowAddNew = false,
			AllowDelete = false,
			AllowDragging = AllowDraggingEnum.None,
			AllowFiltering = false,
			AllowFreezing = AllowFreezingEnum.None,
			AllowMerging = AllowMergingEnum.None,
			AllowMergingFixed = AllowMergingEnum.None,
			AllowResizing = AllowResizingEnum.Both,
			AllowSorting = AllowSortingEnum.None,
			FocusRect = FocusRectEnum.None
		};
		_grid.Rows.Count = 0;
		_grid.Rows.Fixed = 0;
		_grid.Cols.Count = 1;
		_grid.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		_grid.Cols.Fixed = 0;
		_grid.Tree.Column = 0;
		_grid.Rows.DefaultSize = 30;
		_grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
		_grid.MouseMove += _grid_MouseMove;
		_grid.MouseLeave += _grid_MouseLeave;
		_grid.MouseDown += _grid_MouseDown;
		_grid.MouseUp += _grid_MouseUp;
		_grid.Paint += _grid_Paint;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.MouseClick += _grid_MouseClick;
		_grid.BodyAfterRowColChange += _grid_BodyAfterRowColChange;
		_grid.BeforeSelChange += _grid_BeforeSelChange;
		_grid.PaintBackground += _grid_PaintBackground;
		_cmdExpandAll = new C1Command
		{
			Text = "全部展开"
		};
		_cmdExpandAll.Click += _cmdExpandAll_Click;
		_cmdCollapseAll = new C1Command
		{
			Text = "全部收缩"
		};
		_cmdCollapseAll.Click += _cmdCollapseAll_Click;
		_lnkExpandAll = new C1CommandLink(_cmdExpandAll)
		{
			Delimiter = true
		};
		_lnkCollapseAll = new C1CommandLink(_cmdCollapseAll);
		_cmdUpLevel = new C1Command
		{
			Text = "上提一级"
		};
		_cmdUpLevel.Click += _cmdUpLevel_Click;
		_cmdUpLevel.CommandStateQuery += _cmdUpLevel_CommandStateQuery;
		_lnkUpLevel = new C1CommandLink(_cmdUpLevel)
		{
			Delimiter = true
		};
		_cmdDownLevel = new C1Command
		{
			Text = "下沉一级"
		};
		_cmdDownLevel.Click += _cmdDownLevel_Click;
		_cmdDownLevel.CommandStateQuery += _cmdDownLevel_CommandStateQuery;
		_lnkDownLevel = new C1CommandLink(_cmdDownLevel);
		_cmdDeleteCurrentLevel = new C1Command
		{
			Text = "删除当前层级",
			Image = ContextResources.ctxDelete
		};
		_cmdDeleteCurrentLevel.Click += _cmdDeleteCurrentNode_Click;
		_lnkDeleteCurrentLevel = new C1CommandLink(_cmdDeleteCurrentLevel)
		{
			Delimiter = true
		};
		_gridCtx = new C1ContextMenu();
		_gridCtx.HideFirstDelimiter = true;
		_gridCtx.CommandLinks.Add(_lnkUpLevel);
		_gridCtx.CommandLinks.Add(_lnkDownLevel);
		_gridCtx.CommandLinks.Add(_lnkExpandAll);
		_gridCtx.CommandLinks.Add(_lnkCollapseAll);
		_gridCtx.CommandLinks.Add(_lnkDeleteCurrentLevel);
		SetTheme();
	}

	private void _cmdDeleteCurrentNode_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (_finalNav == null || _mouseRightClickNavNode == null || _mouseRightClickNavNode.TitleCell == null)
			{
				return;
			}
			int num = _finalNav.IndexOf(_mouseRightClickNavNode.TitleCell);
			if (num != -1)
			{
				List<TableTitleCell> list = new List<TableTitleCell>(_finalNav);
				list.RemoveAt(num);
				List<string> navTreeCellIdList = list.Select((TableTitleCell u) => u.CellId).ToList();
				Table.Title.NavTreeCellIdList = navTreeCellIdList;
				Table.TagTitleDirty();
				Program.MainForm.TableEditor.ReBuildNavTree();
			}
		}
		catch (Exception)
		{
		}
	}

	private void _cmdDownLevel_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		try
		{
			if (_finalNav == null || _finalNav.Count <= 1)
			{
				e.Enabled = false;
				return;
			}
			if (_mouseRightClickNavNode == null || _mouseRightClickNavNode.TitleCell == null)
			{
				e.Enabled = false;
				return;
			}
			int num = _finalNav.IndexOf(_mouseRightClickNavNode.TitleCell);
			if (num == _finalNav.Count - 1)
			{
				e.Enabled = false;
			}
			else if (IsTargetCellInNavCellRelatedList(_finalNav[num + 1], _mouseRightClickNavNode.TitleCell))
			{
				e.Enabled = false;
			}
			else
			{
				e.Enabled = true;
			}
		}
		catch (Exception)
		{
		}
	}

	private void _cmdDownLevel_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (_finalNav == null || _mouseRightClickNavNode == null || _mouseRightClickNavNode.TitleCell == null)
			{
				return;
			}
			int num = _finalNav.IndexOf(_mouseRightClickNavNode.TitleCell);
			if (num < _finalNav.Count - 1)
			{
				int index = num + 1;
				List<TableTitleCell> list = new List<TableTitleCell>(_finalNav);
				list[index] = _finalNav[num];
				list[num] = _finalNav[index];
				List<string> navTreeCellIdList = list.Select((TableTitleCell u) => u.CellId).ToList();
				Table.Title.NavTreeCellIdList = navTreeCellIdList;
				Table.TagTitleDirty();
				Program.MainForm.TableEditor.ReBuildNavTree();
			}
		}
		catch (Exception)
		{
		}
	}

	private void _cmdUpLevel_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		try
		{
			if (_finalNav == null || _finalNav.Count <= 1)
			{
				e.Enabled = false;
				return;
			}
			if (_mouseRightClickNavNode == null || _mouseRightClickNavNode.TitleCell == null)
			{
				e.Enabled = false;
				return;
			}
			int num = _finalNav.IndexOf(_mouseRightClickNavNode.TitleCell);
			if (num == 0)
			{
				e.Enabled = false;
			}
			else if (IsTargetCellInNavCellRelatedList(_mouseRightClickNavNode.TitleCell, _finalNav[num - 1]))
			{
				e.Enabled = false;
			}
			else
			{
				e.Enabled = true;
			}
		}
		catch (Exception)
		{
		}
	}

	private void _cmdUpLevel_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (_finalNav == null || _mouseRightClickNavNode == null || _mouseRightClickNavNode.TitleCell == null)
			{
				return;
			}
			int num = _finalNav.IndexOf(_mouseRightClickNavNode.TitleCell);
			if (num > 0)
			{
				int index = num - 1;
				List<TableTitleCell> list = new List<TableTitleCell>(_finalNav);
				list[index] = _finalNav[num];
				list[num] = _finalNav[index];
				List<string> navTreeCellIdList = list.Select((TableTitleCell u) => u.CellId).ToList();
				Table.Title.NavTreeCellIdList = navTreeCellIdList;
				Table.TagTitleDirty();
				Program.MainForm.TableEditor.ReBuildNavTree();
			}
		}
		catch (Exception)
		{
		}
	}

	private void _grid_PaintBackground(object sender, PaintEventArgs e)
	{
		if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.Picture))
		{
			Point ptScreen = _grid.Parent.PointToScreen(_grid.Location);
			Point point = Program.MainForm.PnlMainRelativePosition(ptScreen);
			Bitmap currentBackgroudImage = Theme.CurrentBackgroudImage;
			e.Graphics.DrawImage(currentBackgroudImage, 0, 0, new Rectangle(point.X, point.Y, currentBackgroudImage.Width - point.X, currentBackgroudImage.Height - point.Y), GraphicsUnit.Pixel);
		}
	}

	private void _grid_MouseDown(object sender, MouseEventArgs e)
	{
		try
		{
			if (e.Button == MouseButtons.Right)
			{
				HitTestInfo hitTestInfo = _grid.HitTest();
				if (hitTestInfo.Type == HitTestTypeEnum.Cell && !_grid.IsRowIndexOutOfRange(hitTestInfo.Row))
				{
					_mouseRightClickNavNode = _grid.Rows[hitTestInfo.Row].UserData as NavNode;
				}
			}
		}
		catch { }
	}

	private void _grid_MouseUp(object sender, MouseEventArgs e)
	{
	}

	private void _grid_BeforeSelChange(object sender, RangeEventArgs e)
	{
		if (!_isOnlyRunGridDefaultSelectEventProcess && _isSuspendSelectChangeEvent)
		{
			e.Cancel = true;
		}
	}

	public TableTitleCell GetNavTreeTopestLevelWhichRelateTargetCell(TableTitleCell targetCell)
	{
		if (_finalNav == null || _finalNav.Count == 0)
		{
			return null;
		}
		foreach (TableTitleCell item in _finalNav)
		{
			if (_cellRelatedOtherCellDic.TryGetValue(item, out var value) && value.Contains(targetCell))
			{
				return item;
			}
		}
		return null;
	}

	public bool IsNavTreeRelatedTitleCell(TableTitleCell titleCell)
	{
		return _relatedTitleCellSet.Contains(titleCell);
	}

	protected bool IsTargetCellInNavCellRelatedList(TableTitleCell navCell, TableTitleCell targetCell)
	{
		if (!_cellRelatedOtherCellDic.TryGetValue(navCell, out var value))
		{
			return false;
		}
		return value.Contains(targetCell);
	}

	public void SelectNavNodeByTableInputValue()
	{
		if (_finalNav == null || _finalNav.Count == 0)
		{
			return;
		}
		List<ValueOperand> list = new List<ValueOperand>();
		foreach (TableTitleCell item in _finalNav)
		{
			list.Add(ValueOperand.FromObject(item.Value));
		}
		ValueOperand[] array = list.ToArray();
		int count = _grid.Rows.Count;
		for (int i = _grid.Rows.Fixed; i < count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
			if (row.UserData is NavNode navNode)
			{
				navNode.RealValueOnTitleCellSameType = Auditai.Model.Cell.ChangeToValueOperand(navNode.DisplayStringRealValue, navNode.TitleCell.DataFormat.GetDataType());
			}
		}
		int num = -1;
		if (_isNavTreeShowInComblistTreeMode)
		{
			for (int j = _grid.Rows.Fixed; j < count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = _grid.Rows[j];
				if (row2.IsNode && row2.Node.Children <= 0 && row2.UserData is NavNode { RealValueOnTitleCellSameType: var realValueOnTitleCellSameType } && realValueOnTitleCellSameType.Equals(array[array.Length - 1]))
				{
					num = j;
					break;
				}
			}
		}
		else
		{
			int num2 = array.Length - 1;
			for (int num3 = num2; num3 >= 0; num3--)
			{
				for (int k = _grid.Rows.Fixed; k < count; k++)
				{
					C1.Win.C1FlexGrid.Row row3 = _grid.Rows[k];
					if (!(row3.UserData is NavNode navNode3))
					{
						continue;
					}
					int level = row3.Node.Level;
					if (level >= array.Length || level != num3)
					{
						continue;
					}
					ValueOperand realValueOnTitleCellSameType2 = navNode3.RealValueOnTitleCellSameType;
					ValueOperand obj = array[level];
					if (!realValueOnTitleCellSameType2.Equals(obj))
					{
						continue;
					}
					bool flag = true;
					for (Node parent = row3.Node.Parent; parent != null; parent = parent.Parent)
					{
						int level2 = parent.Level;
						if (!(parent.Row.UserData is NavNode { RealValueOnTitleCellSameType: var realValueOnTitleCellSameType3 }))
						{
							flag = false;
							break;
						}
						obj = array[level2];
						if (!realValueOnTitleCellSameType3.Equals(obj))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						num = k;
						break;
					}
				}
				if (num != -1)
				{
					break;
				}
			}
		}
		if (num != -1)
		{
			_shouldSkipBodyAfterRowColChange = true;
			_isOnlyRunGridDefaultSelectEventProcess = true;
			_grid.Select(num, 0);
			_grid.ShowCell(num, 0);
			_shouldSkipBodyAfterRowColChange = false;
			_isOnlyRunGridDefaultSelectEventProcess = false;
		}
	}

	public void Populate()
	{
		Debug.WriteLine($"[TableNavGrid] Populate: Nav={(Nav != null ? Nav.Count : 0)}, Table={(Table != null ? "not null" : "null")}");
		_relatedTitleCellSet.Clear();
		_cellRelatedOtherCellDic.Clear();
		_finalNav = null;
		if (Table == null)
		{
			return;
		}
		int @fixed = _grid.Rows.Fixed;
		_grid.Rows.Count = _grid.Rows.Fixed;
		_grid.BeginUpdate();
		int i;
		int level;
		if (Nav != null && Nav.Count > 0)
		{
			NavNode navNode = MakeTree();
			_grid.Rows.Count += navNode.GetRowCount() - 1;
			i = 0;
			level = 0;
			foreach (NavNode child in navNode.Children)
			{
				AddNode(child);
			}
		}
		_grid.EndUpdate();
		void AddNode(NavNode node)
		{
			_grid.BodyGetRow(i).IsNode = true;
			Node node2 = _grid.BodyGetRow(i).Node;
			node2.Level = level;
			node2.Key = node;
			i++;
			foreach (NavNode child2 in node.Children)
			{
				level++;
				AddNode(child2);
				level--;
			}
		}
	}

	public void SetTheme()
	{
		Theme.SetCurrentObject(_grid);
		_grid.Styles.Alternate.Clear();
		_grid.Styles.EmptyArea.BackColor = Color.Transparent;
		_grid.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_brushHoverBackground.Color = Color.FromArgb(100, Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background"));
		_colorPositionLine = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
		_contentTextDefaultColor = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("BaseThemeProperties\\Styles\\Content\\ForeColor", Color.Black);
		_gridMouseOverMoreMenuIconBrush.Color = Auditai.UI.Controls.Util.DarkenColor(_brushHoverBackground.Color, 0.1);
		if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.Picture))
		{
			_grid.Styles.Alternate.BackColor = Color.Transparent;
			_grid.Styles.EmptyArea.BackColor = Color.Transparent;
		}
		else
		{
			_grid.Styles.EmptyArea.BackColor = Color.White;
		}
	}

	private Rectangle GetGridRowMoreMenuIconBackgroundRectangle(int rowIndex)
	{
		Point gridRowMoreMenuIconLeftTopPosition = GetGridRowMoreMenuIconLeftTopPosition(rowIndex);
		int num = 3;
		int num2 = 3;
		return new Rectangle(gridRowMoreMenuIconLeftTopPosition.X - num, gridRowMoreMenuIconLeftTopPosition.Y - num2, Resources.menuMoreOperation.Width + num * 2, Resources.menuMoreOperation.Height + num2 * 2);
	}

	private Point GetGridRowMoreMenuIconLeftTopPosition(int rowIndex)
	{
		if (_grid.IsRowIndexOutOfRange(rowIndex))
		{
			return new Point(-100, -100);
		}
		Rectangle cellRect = _grid.GetCellRect(rowIndex, 0);
		int num = 25;
		int x = cellRect.X + cellRect.Width - num;
		int y = cellRect.Y + (cellRect.Height - Resources.menuMoreOperation.Height) / 2;
		return new Point(x, y);
	}

	private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
			C1.Win.C1FlexGrid.Row row = _grid.BodyGetRow(e.Row);
			if (row.UserData is NavNode navNode)
			{
				e.Text = (string.IsNullOrEmpty(navNode.Text) ? "(空)" : navNode.Text);
				C1.Win.C1FlexGrid.CellStyle cellStyle = ((e.Style == null) ? _grid.GetCellRange(e.Row, e.Col).StyleNew : e.Style);
				if (e.Style.Name == _grid.Styles.Highlight.Name)
				{
					cellStyle.ForeColor = _grid.Styles.Highlight.ForeColor;
				}
				else
				{
					cellStyle.ForeColor = _NodeTextColor;
				}
				e.Style = cellStyle;
				if (navNode.Children.Count == 0)
				{
					System.Drawing.Image cellImage = _grid.GetCellImage(e.Row, e.Col);
					if (cellImage != null)
					{
						e.Image = cellImage;
					}
					else
					{
						e.Image = Resources.TreeListLeaf3;
					}
				}
				else if (row.IsNode)
				{
					System.Drawing.Image cellImage2 = _grid.GetCellImage(e.Row, e.Col);
					if (cellImage2 != null)
					{
						e.Image = cellImage2;
					}
					else if (row.Node.Collapsed)
					{
						e.Image = Resources.TicketNavTreeListCollapsed;
					}
					else
					{
						e.Image = Resources.TicketNavTreeListExpanded;
					}
				}
			}
			if (e.Row == _mouseRow)
			{
				e.Graphics.FillRectangle(_brushHoverBackground, e.Bounds);
			}
			if (e.Row != _mouseRow)
			{
				return;
			}
			bool isUseWhiteImage2;
			Bitmap image = GetGridRowMoreMenuImage(out isUseWhiteImage2);
			e.DrawCell(DrawCellFlags.All);
			e.Handled = true;
			if (_isMouseOverMoreMenuIcon)
			{
				if (isUseWhiteImage2)
				{
					_gridMouseOverMoreMenuIconBrush.Color = Auditai.UI.Controls.Util.LightColor(e.Style.BackColor, 0.2);
				}
				else
				{
					_gridMouseOverMoreMenuIconBrush.Color = Auditai.UI.Controls.Util.DarkenColor(_grid.Styles.SelectedColumnHeader.BackColor, 0.1);
				}
				e.Graphics.FillRectangle(_gridMouseOverMoreMenuIconBrush, GetGridRowMoreMenuIconBackgroundRectangle(e.Row));
			}
			Point gridRowMoreMenuIconLeftTopPosition = GetGridRowMoreMenuIconLeftTopPosition(e.Row);
			e.Graphics.DrawImage(image, gridRowMoreMenuIconLeftTopPosition);
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		Bitmap GetGridRowMoreMenuImage(out bool isUseWhiteImage)
		{
			isUseWhiteImage = false;
			if (e.Style.Name != _grid.Styles.Highlight.Name)
			{
				return Resources.menuMoreOperation;
			}
			if (Theme.SelectedAuditaiTheme.ThemeContext.GridMoreMenuImageIndexOnHighLightRow == GridMoreMenuImageIndex.White)
			{
				if (_menuMoreOperationWhiteImage == null)
				{
					_menuMoreOperationWhiteImage = (Bitmap)new WhiteImageStrategy().ProcessImage(Resources.menuMoreOperation);
				}
				isUseWhiteImage = true;
				return _menuMoreOperationWhiteImage;
			}
			return Resources.menuMoreOperation;
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
	}

	private void _grid_MouseLeave(object sender, EventArgs e)
	{
		_isMouseOverMoreMenuIcon = false;
		_mouseRow = -1;
		_grid.Invalidate();
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		try
		{
			bool flag = false;
			HitTestInfo hitTestInfo = _grid.HitTest();
			if (_mouseRow != hitTestInfo.Row)
			{
				_mouseRow = hitTestInfo.Row;
				flag = true;
			}
			if (_mouseRow != -1)
			{
				bool flag2 = GetGridRowMoreMenuIconBackgroundRectangle(_mouseRow).Contains(e.X, e.Y);
				if (flag2 != _isMouseOverMoreMenuIcon)
				{
					flag = true;
				}
				_isMouseOverMoreMenuIcon = flag2;
			}
			else
			{
				_isMouseOverMoreMenuIcon = false;
			}
			if (flag)
			{
				_grid.Invalidate();
			}
		}
		catch (Exception)
		{
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		_mouseRightClickNavNode = null;
		if (e.Button == MouseButtons.Left)
		{
			if (_mouseRow != -1 && _isMouseOverMoreMenuIcon && !_grid.IsRowIndexOutOfRange(_mouseRow))
			{
				_grid.Row = _mouseRow;
				PrepareToShowCtx(e);
				_gridCtx.ShowContextMenu(_grid, e.Location);
				return;
			}
			HitTestInfo hitTestInfo = _grid.HitTest();
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
				if (row.IsNode)
				{
					row.Node.Collapsed = !row.Node.Collapsed;
				}
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			PrepareToShowCtx(e);
			_gridCtx.ShowContextMenu(_grid, e.Location);
		}
	}

	private void PrepareToShowCtx(MouseEventArgs e)
	{
		try
		{
			HitTestInfo hitTestInfo = _grid.HitTest();
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				_cmdUpLevel.Visible = true;
				_cmdDownLevel.Visible = true;
				_mouseRightClickNavNode = _grid.Rows[hitTestInfo.Row].UserData as NavNode;
				_cmdDeleteCurrentLevel.Visible = true;
			}
			else
			{
				_cmdUpLevel.Visible = false;
				_cmdDownLevel.Visible = false;
				_cmdDeleteCurrentLevel.Visible = false;
			}
		}
		catch (Exception)
		{
		}
	}

	private void _grid_BodyAfterRowColChange(object sender, RangeEventArgs e)
	{
		SelectedTitleCells = null;
		if (_shouldSkipBodyAfterRowColChange)
		{
			return;
		}
		int row = _grid.Row;
		if (row < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row2 = _grid.BodyGetRow(row);
		if (!row2.IsNode || row2.Node.Children != 0)
		{
			return;
		}
		HashSet<TableTitleCell> hashSet = new HashSet<TableTitleCell>();
		SelectedTitleCells = new List<Tuple<TableTitleCell, object>>();
		for (Node node = row2.Node; node != null; node = node.Parent)
		{
			if (node.Row.UserData is NavNode navNode)
			{
				SelectedTitleCells.Add(Tuple.Create(navNode.TitleCell, navNode.DisplayStringRealValue));
				hashSet.Add(navNode.TitleCell);
			}
		}
		SelectedTitleCells.Reverse();
		if (_finalNav != null)
		{
			foreach (TableTitleCell item in _finalNav)
			{
				if (!hashSet.Contains(item))
				{
					SelectedTitleCells.Add(Tuple.Create(item, (object)string.Empty));
					hashSet.Add(item);
				}
			}
		}
		this.TreeLeafNodeSelected?.Invoke(this, EventArgs.Empty);
	}

	public void SelectRowWithoutTriggerSelectEvent(int rowIndex)
	{
		_shouldSkipBodyAfterRowColChange = true;
		_isOnlyRunGridDefaultSelectEventProcess = true;
		_grid.Select(rowIndex, 0);
		_shouldSkipBodyAfterRowColChange = false;
		_isOnlyRunGridDefaultSelectEventProcess = false;
	}

	private void _cmdCollapseAll_Click(object sender, ClickEventArgs e)
	{
		_grid.CollapseAll();
	}

	private void _cmdExpandAll_Click(object sender, ClickEventArgs e)
	{
		_grid.ExpandAll();
	}

	private NavNode MakeTree()
	{
		NavNode navNode = new NavNode();
		if (Nav != null && Nav.Any())
		{
			List<TableTitleCell> navListContainsComboListCell = GetNavListContainsComboListCell();
			Debug.WriteLine($"[TableNavGrid] MakeTree: Nav.Count={Nav.Count}, comboListCellCount={(navListContainsComboListCell != null ? navListContainsComboListCell.Count : 0)}");
			if (navListContainsComboListCell != null && navListContainsComboListCell.Count > 0)
			{
				FillNavTreeNode(navNode, navListContainsComboListCell);
				_finalNav = navListContainsComboListCell;
			}
		}
		else
		{
			Debug.WriteLine($"[TableNavGrid] MakeTree: Nav is null or empty");
		}
		Debug.WriteLine($"[TableNavGrid] MakeTree: root.Children.Count={navNode.Children.Count}");
		return navNode;
	}

	private void FillNavTreeNode(NavNode rootNode, List<TableTitleCell> comboxListCell)
	{
		_isNavTreeShowInComblistTreeMode = false;
		if (Table.IsLocked)
		{
			Debug.WriteLine("[TableNavGrid] FillNavTreeNode: Table is locked, return");
			return;
		}
		if (comboxListCell.Count == 1)
		{
			Debug.WriteLine($"[TableNavGrid] FillNavTreeNode: single cell, ComboList='{comboxListCell[0].ComboList}'");
			List<TableTitleCell> referredSameTableOtherTitleCell = GetReferredSameTableOtherTitleCell(comboxListCell[0]);
			foreach (TableTitleCell item in referredSameTableOtherTitleCell)
			{
				_relatedTitleCellSet.Add(item);
			}
			_relatedTitleCellSet.Add(comboxListCell[0]);
			_cellRelatedOtherCellDic[comboxListCell[0]] = new List<TableTitleCell>(referredSameTableOtherTitleCell);
			TreeListOperand treeListData;
			List<Tuple<string, string>> comboListValue = GetComboListValue(comboxListCell[0], comboxListCell.Count, out treeListData);
			Debug.WriteLine($"[TableNavGrid] FillNavTreeNode: comboListValue={(comboListValue != null ? comboListValue.Count : "null")}, treeListData={(treeListData != null ? "not null" : "null")}");
			if (treeListData != null)
			{
				TreeListOperand treeListOperand = treeListData;
				_isNavTreeShowInComblistTreeMode = true;
				{
					foreach (TreeListNode root in treeListOperand.Roots)
					{
						AddNavNode(rootNode, root, comboxListCell[0]);
					}
					Debug.WriteLine($"[TableNavGrid] FillNavTreeNode: tree mode, root.Children.Count={rootNode.Children.Count}");
					return;
				}
			}
			if (comboListValue == null)
			{
				Debug.WriteLine("[TableNavGrid] FillNavTreeNode: comboListValue is null, return");
				return;
			}
			for (int i = 0; i < comboListValue.Count; i++)
			{
				Tuple<string, string> tuple = comboListValue[i];
				if (!string.IsNullOrWhiteSpace(tuple.Item1))
				{
					NavNode navNode = rootNode.AddLastLevel(tuple.Item1);
					navNode.TitleCell = comboxListCell[0];
					navNode.DisplayStringRealValue = tuple.Item2;
				}
			}
			return;
		}
		VirtualTable virtualTable = new VirtualTable(1, Table.Columns.Count);
		virtualTable.SetDefaultStyle(Table.DefaultStyle);
		int count = Table.Title.Rows.Count;
		int count2 = Table.Title.Columns.Count;
		virtualTable.BuildTitleCell(count, count2);
		virtualTable.SetTitleCellValue(0, 0, Table.Title.TitleCell.Value);
		if (count > 0)
		{
			for (int j = 0; j < count; j++)
			{
				for (int k = 0; k < count2; k++)
				{
					TableTitleCell rawTableTitleCell = Table.Title.GetCell(j, k);
					if (string.IsNullOrWhiteSpace(rawTableTitleCell.CellId))
					{
						virtualTable.SetTitleCellValue(j, k, rawTableTitleCell.Value);
					}
					else if (!comboxListCell.Any((TableTitleCell u) => u.CellId == rawTableTitleCell.CellId))
					{
						virtualTable.SetTitleCellValue(j, k, rawTableTitleCell.Value);
					}
				}
			}
		}
		for (int l = 0; l < comboxListCell.Count; l++)
		{
			TableTitleCell tableTitleCell = comboxListCell[l];
			List<TableTitleCell> referredSameTableOtherTitleCell2 = GetReferredSameTableOtherTitleCell(tableTitleCell);
			foreach (TableTitleCell item2 in referredSameTableOtherTitleCell2)
			{
				_relatedTitleCellSet.Add(item2);
			}
			_relatedTitleCellSet.Add(tableTitleCell);
			_cellRelatedOtherCellDic[tableTitleCell] = new List<TableTitleCell>(referredSameTableOtherTitleCell2);
			if (referredSameTableOtherTitleCell2.Count == 0)
			{
				TreeListOperand treeListData2;
				List<Tuple<string, string>> comboListValue2 = GetComboListValue(tableTitleCell, comboxListCell.Count, out treeListData2);
				if (comboListValue2 != null)
				{
					FillNavNodeValue(rootNode, l, comboListValue2, tableTitleCell);
				}
				continue;
			}
			if (!referredSameTableOtherTitleCell2.Any((TableTitleCell u) => comboxListCell.Contains(u)))
			{
				TreeListOperand treeListData3;
				List<Tuple<string, string>> comboListValue3 = GetComboListValue(tableTitleCell, comboxListCell.Count, out treeListData3);
				if (comboListValue3 != null)
				{
					FillNavNodeValue(rootNode, l, comboListValue3, tableTitleCell);
				}
				continue;
			}
			FormulaEvaluator formulaEvaluator = GenerateColComboListEvaluator(tableTitleCell);
			if (formulaEvaluator != null)
			{
				formulaEvaluator.Env.TableTitleCellResolver = new VirtualTableTitleCellResolver(Table.Id, virtualTable);
				FillNavNodeValue(rootNode, l, formulaEvaluator, virtualTable, tableTitleCell);
			}
		}
		static void AddNavNode(NavNode parent, TreeListNode wantAddNode, TableTitleCell titleCell)
		{
			if (!string.IsNullOrWhiteSpace(wantAddNode.Text))
			{
				NavNode navNode2 = ((wantAddNode.Children.Count > 0) ? parent.AddOrGet(wantAddNode.Text) : parent.AddLastLevel(wantAddNode.Text));
				navNode2.TitleCell = titleCell;
				navNode2.DisplayStringRealValue = wantAddNode.Text;
				if (wantAddNode.Children.Count > 0)
				{
					foreach (TreeListNode child in wantAddNode.Children)
					{
						AddNavNode(navNode2, child, titleCell);
					}
				}
			}
		}
	}

	private List<TableTitleCell> GetReferredSameTableOtherTitleCell(TableTitleCell cell)
	{
		List<TableTitleCell> result = new List<TableTitleCell>();
		if (string.IsNullOrWhiteSpace(cell.ComboList))
		{
			return result;
		}
		try
		{
			FindRefTitleCellResolver findRefTitleCellResolver = new FindRefTitleCellResolver(Table.Id);
			FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
			FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
			{
				Resolver = resolver,
				RowIndex = 0,
				HostTable = Table,
				RefManager = Table.Project.DataReferenceManager,
				RefEvalContext = new DataReferenceEvaluationContext
				{
					Project = Table.Project,
					CurrentTreeNode = Table.TreeNode
				},
				TableTitleCellResolver = findRefTitleCellResolver
			};
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.ComboList)
			{
				Env = env
			};
			Operand operand = formulaEvaluator.EvaluateToOperand();
			result = findRefTitleCellResolver.tableTitleCellsSet.ToList();
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}

	private void FillNavNodeValue(NavNode rootNode, int fillLevel, List<Tuple<string, string>> valueList, TableTitleCell cell)
	{
		VistNavTree(rootNode, 0);
		void VistNavTree(NavNode parent, int currentLevel)
		{
			if (currentLevel == fillLevel)
			{
				foreach (Tuple<string, string> value in valueList)
				{
					if (!string.IsNullOrWhiteSpace(value.Item1))
					{
						NavNode navNode = parent.AddIfNotExist(value.Item1);
						if (navNode != null)
						{
							navNode.TitleCell = cell;
							navNode.DisplayStringRealValue = value.Item2;
							_hasFilledVirtualNodeCount++;
							if (_hasFilledVirtualNodeCount >= 20000)
							{
								break;
							}
						}
					}
				}
				return;
			}
			if (currentLevel < fillLevel)
			{
				List<NavNode> children = parent.Children;
				if (children != null)
				{
					foreach (NavNode item in children)
					{
						VistNavTree(item, currentLevel + 1);
						if (_hasFilledVirtualNodeCount >= 20000)
						{
							break;
						}
					}
				}
			}
		}
	}

	private void FillNavNodeValue(NavNode rootNode, int fillLevel, FormulaEvaluator formulaEvaluator, VirtualTable virtualTable, TableTitleCell cell)
	{
		VistNavTree(rootNode, 0);
		void UpdateVirtualTableCellValue(NavNode navNode, object cellValue)
		{
			if (navNode != null && navNode.TitleCell != null && Table.Title.GetCellIndex(navNode.TitleCell, out var rowIndex, out var colIndex))
			{
				if (cellValue == null)
				{
					cellValue = string.Empty;
				}
				object value = Auditai.Model.Cell.ChangeDataTypeImpl(cellValue, navNode.TitleCell.DataFormat.GetDataType());
				virtualTable.SetTitleCellValue(rowIndex, colIndex, value);
			}
		}
		void VistNavTree(NavNode parent, int currentLevel)
		{
			if (currentLevel == fillLevel)
			{
				List<Tuple<string, string>> comboListValueByVirtualTable = GetComboListValueByVirtualTable(formulaEvaluator, cell);
				if (comboListValueByVirtualTable != null)
				{
					foreach (Tuple<string, string> item in comboListValueByVirtualTable)
					{
						if (!string.IsNullOrWhiteSpace(item.Item1))
						{
							NavNode navNode2 = parent.AddIfNotExist(item.Item1);
							if (navNode2 != null)
							{
								navNode2.TitleCell = cell;
								navNode2.Text = item.Item1;
								navNode2.DisplayStringRealValue = item.Item2;
								_hasFilledVirtualNodeCount++;
								if (_hasFilledVirtualNodeCount >= 20000)
								{
									break;
								}
							}
						}
					}
				}
			}
			else if (currentLevel < fillLevel)
			{
				List<NavNode> children = parent.Children;
				if (children != null)
				{
					foreach (NavNode item2 in children)
					{
						UpdateVirtualTableCellValue(item2, item2.DisplayStringRealValue);
						VistNavTree(item2, currentLevel + 1);
						UpdateVirtualTableCellValue(item2, null);
						if (_hasFilledVirtualNodeCount >= 20000)
						{
							break;
						}
					}
				}
			}
		}
	}

	private List<Tuple<string, string>> ConvertOperandToNodeDisplayValueList(Operand op, bool isNavCellCountMoreThanOne, out TreeListOperand treeListData)
	{
		treeListData = null;
		if (op is MultiListOperand)
		{
			return null;
		}
		if (op is TreeListOperand treeListOperand)
		{
			List<Tuple<string, string>> list = new List<Tuple<string, string>>();
			Queue<string> upLevelTextQueue2 = new Queue<string>();
			foreach (TreeListNode root in treeListOperand.Roots)
			{
				AddLeafNode(root, list, upLevelTextQueue2);
			}
			treeListData = treeListOperand;
			return list;
		}
		if (op is TableListOperand tableListOperand)
		{
			int count = tableListOperand.DataTable.Rows.Count;
			if (tableListOperand.DataTable.Columns.Count == 0)
			{
				return null;
			}
			int count2 = tableListOperand.DataTable.Columns.Count;
			List<Tuple<string, string>> list2 = new List<Tuple<string, string>>();
			for (int i = 0; i < count; i++)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int j = 0; j < count2; j++)
				{
					string value = tableListOperand.DataTable.Rows[i][j] as string;
					if (!string.IsNullOrWhiteSpace(value))
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.Append(" ");
						}
						stringBuilder.Append(value);
					}
				}
				string item = tableListOperand.DataTable.Rows[i][0] as string;
				string item2 = stringBuilder.ToString();
				list2.Add(Tuple.Create(item2, item));
			}
			return list2;
		}
		return ((!(op is InputListOperand inputListOperand)) ? op.ToValueSetOrderByRowIndex() : inputListOperand)?.Set.Select((Tuple<Auditai.Model.Row, ValueOperand> tup) => Tuple.Create(tup.Item2.ToString(), tup.Item2.ToString())).ToList();
		static void AddLeafNode(TreeListNode node, List<Tuple<string, string>> outList, Queue<string> upLevelTextQueue)
		{
			if (node.Children == null || node.Children.Count == 0)
			{
				string fullName = GetFullName(node.Text, upLevelTextQueue);
				outList.Add(Tuple.Create(fullName, node.Text));
			}
			else
			{
				upLevelTextQueue.Enqueue(node.Text);
				foreach (TreeListNode child in node.Children)
				{
					AddLeafNode(child, outList, upLevelTextQueue);
				}
				upLevelTextQueue.Dequeue();
			}
		}
		static string GetFullName(string currentLevelText, Queue<string> upLevelTextQueue)
		{
			if (upLevelTextQueue.Count == 0)
			{
				return currentLevelText;
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (string item3 in upLevelTextQueue)
			{
				if (!string.IsNullOrWhiteSpace(item3))
				{
					if (stringBuilder2.Length > 0)
					{
						stringBuilder2.Append(" ");
					}
					stringBuilder2.Append(item3);
				}
			}
			if (!string.IsNullOrWhiteSpace(currentLevelText))
			{
				if (stringBuilder2.Length > 0)
				{
					stringBuilder2.Append(" ");
				}
				stringBuilder2.Append(currentLevelText);
			}
			return stringBuilder2.ToString();
		}
	}

	protected List<Tuple<string, string>> GetComboListValue(TableTitleCell cell, int navCellCount, out TreeListOperand treeListData)
	{
		treeListData = null;
		if (string.IsNullOrWhiteSpace(cell.ComboList))
		{
			Debug.WriteLine("[TableNavGrid] GetComboListValue: ComboList is empty, return null");
			return null;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RowIndex = 0,
			HostTable = Table,
			RefManager = Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Table.Project,
				CurrentTreeNode = Table.TreeNode
			}
		};
		try
		{
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.ComboList)
			{
				Env = env
			};
			Operand op = formulaEvaluator.EvaluateToOperand();
			var result = ConvertOperandToNodeDisplayValueList(op, navCellCount > 1, out treeListData);
			Debug.WriteLine($"[TableNavGrid] GetComboListValue: result.Count={(result != null ? result.Count : "null")}, treeListData={(treeListData != null ? "not null" : "null")}, opType={op?.GetType().Name}");
			return result;
		}
		catch (FormulaException ex)
		{
			Debug.WriteLine($"[TableNavGrid] GetComboListValue: FormulaException: {ex.Message}");
			return null;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[TableNavGrid] GetComboListValue: Exception: {ex.GetType().Name}: {ex.Message}");
			return null;
		}
	}

	private List<TableTitleCell> GetNavListContainsComboListCell()
	{
		if (Nav == null || Nav.Count == 0)
		{
			return null;
		}
		List<TableTitleCell> list = null;
		HashSet<TableTitleCell> hashSet = null;
		foreach (string item in Nav)
		{
			TableTitleCell uIRenderCellByCellId = Table.Title.GetUIRenderCellByCellId(item);
			if (uIRenderCellByCellId != null && !string.IsNullOrWhiteSpace(uIRenderCellByCellId.ComboList))
			{
				if (list == null)
				{
					list = new List<TableTitleCell>();
					hashSet = new HashSet<TableTitleCell>();
				}
				if (!hashSet.Contains(uIRenderCellByCellId))
				{
					list.Add(uIRenderCellByCellId);
					hashSet.Add(uIRenderCellByCellId);
				}
			}
		}
		return list;
	}

	private FormulaEvaluator GenerateColComboListEvaluator(TableTitleCell cell)
	{
		if (string.IsNullOrWhiteSpace(cell.ComboList))
		{
			return null;
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(Table.Project);
		FormulaEvaluationEnvironment env = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RowIndex = 0,
			HostTable = Table,
			RefManager = Table.Project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = Table.Project,
				CurrentTreeNode = Table.TreeNode
			}
		};
		return new FormulaEvaluator(cell.ComboList)
		{
			Env = env
		};
	}

	private List<Tuple<string, string>> GetComboListValueByVirtualTable(FormulaEvaluator evaluator, TableTitleCell cell)
	{
		try
		{
			Operand op = evaluator.EvaluateToOperand();
			TreeListOperand treeListData;
			return ConvertOperandToNodeDisplayValueList(op, isNavCellCountMoreThanOne: true, out treeListData);
		}
		catch (FormulaException)
		{
			return null;
		}
		catch (Exception exception)
		{
			exception.Log("运算下拉框公式时发生了未预期的异常");
			return null;
		}
	}
}
