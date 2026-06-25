using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class frmNodeSelectorWithTicketRecord : C1RibbonForm
{
	protected class AddTicketNodeStepByStepRuner
	{
		[CompilerGenerated]
		private sealed class _003CAddTicketNodeStepByStep_003Ed__2 : IEnumerator<object>, IDisposable, IEnumerator
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public AddTicketNodeStepByStepRuner _003C_003E4__this;

			private C1FlexGridEx _003C_grid_003E5__2;

			private int _003CtotalCount_003E5__3;

			private int _003CfixedRowsCount_003E5__4;

			private int _003ChasProcessCount_003E5__5;

			private int _003Ci_003E5__6;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			[DebuggerHidden]
			public _003CAddTicketNodeStepByStep_003Ed__2(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				_003C_grid_003E5__2 = null;
				_003C_003E1__state = -2;
			}

			private bool MoveNext()
			{
				int num = _003C_003E1__state;
				AddTicketNodeStepByStepRuner addTicketNodeStepByStepRuner = _003C_003E4__this;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -1;
					goto IL_00db;
				}
				_003C_003E1__state = -1;
				_003C_grid_003E5__2 = addTicketNodeStepByStepRuner._owner._grid;
				_003CtotalCount_003E5__3 = _003C_grid_003E5__2.Rows.Count;
				_003CfixedRowsCount_003E5__4 = _003C_grid_003E5__2.Rows.Fixed;
				_003ChasProcessCount_003E5__5 = 0;
				_003Ci_003E5__6 = _003CtotalCount_003E5__3 - 1;
				goto IL_0116;
				IL_00db:
				addTicketNodeStepByStepRuner.UpdateProgressValue((float)_003ChasProcessCount_003E5__5++ / ((float)_003CtotalCount_003E5__3 * 1f));
				_003Ci_003E5__6--;
				goto IL_0116;
				IL_0116:
				if (_003Ci_003E5__6 >= _003CfixedRowsCount_003E5__4)
				{
					C1.Win.C1FlexGrid.Row row = _003C_grid_003E5__2.Rows[_003Ci_003E5__6];
					row.Visible = true;
					if (row.UserData is TreeTableNode treeTableNode && treeTableNode.HasReadPermission())
					{
						addTicketNodeStepByStepRuner._owner.AddTicketNode(row.Node, treeTableNode);
						Application.DoEvents();
						_003C_003E2__current = null;
						_003C_003E1__state = 1;
						return true;
					}
					goto IL_00db;
				}
				foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)_003C_grid_003E5__2.Rows)
				{
					if (item.IsNode)
					{
						item.Node.Collapsed = false;
					}
				}
				addTicketNodeStepByStepRuner._owner.progressBar.Value = 100;
				Application.DoEvents();
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		protected frmNodeSelectorWithTicketRecord _owner;

		public AddTicketNodeStepByStepRuner(frmNodeSelectorWithTicketRecord owner)
		{
			_owner = owner;
		}

		[IteratorStateMachine(typeof(_003CAddTicketNodeStepByStep_003Ed__2))]
		public IEnumerator AddTicketNodeStepByStep()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003CAddTicketNodeStepByStep_003Ed__2(0)
			{
				_003C_003E4__this = this
			};
		}

		protected void UpdateProgressValue(float percent)
		{
			int num = (int)(percent * 100f);
			if (num >= 100)
			{
				num = 99;
			}
			else if (num < 0)
			{
				num = 0;
			}
			_owner.progressBar.Value = num;
		}
	}

	protected class MosueOverCellInfo
	{
		public int RowIndex { get; set; }

		public int ColIndex { get; set; }

		public Point MouseLocation { get; set; }

		public MosueOverCellInfo(int row, int col)
		{
			RowIndex = row;
			ColIndex = col;
		}
	}

	public class TicketNavTreeNodeBase : TreeNodeBase
	{
		public TreeNodeBase ParentNode;

		public Node GridNode { get; set; }

		protected override int GetCode()
		{
			return -1;
		}
	}

	public class TicketNavTreeFolderNode : TicketNavTreeNodeBase
	{
	}

	public class TicketNavTreePageNode : TicketNavTreeFolderNode
	{
		public bool IsReocrdGenerated { get; set; }

		public Id64 NodeId { get; set; }

		public TicketNav TicketNavTree { get; set; }

		public TicketTable Ticket { get; set; }

		public List<Auditai.Model.Column> TicketColumnList { get; set; }
	}

	public class TicketNavTreeRecordNode : TicketNavTreeNodeBase
	{
		public TicketRecord Record { get; set; }

		public TreeTableNode TableNode { get; set; }
	}

	private C1ContextMenu ctxMenu = new C1ContextMenu();

	private C1CommandLink lnkCollaspe = new C1CommandLink();

	private C1Command cmdCollaspe = new C1Command();

	private C1CommandLink lnkExpand = new C1CommandLink();

	private C1Command cmdExpand = new C1Command();

	private TooltipBox _ttp = new TooltipBox
	{
		Duration = 3000,
		IsBalloon = true
	};

	private const string CN_NODE = "node";

	private const string CN_CHECK = "check";

	private const int COL_NODE = 0;

	private const int COL_CHECK = 1;

	private const int COL_CHECK_WIDTH = 100;

	private int _idHighValue = -1;

	private int _idLowValue = int.MinValue;

	private MosueOverCellInfo _mouseCurrentOverWhichTicketRecordIconCell;

	private SolidBrush _ticketRecordIconOnFocusBackgrndBrush = new SolidBrush(Color.Gray);

	private HashSet<Id64> _hashLoadTicketTable = new HashSet<Id64>();

	private bool _isAllTicketGenerated;

	private bool _isInSearching;

	private IContainer components;

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlEditor;

	private C1FlexGridEx _grid;

	private C1SplitterPanel pnlButton;

	private C1Button btnCertain;

	private C1Button btnCancel;

	private C1CheckBox ckbExportExcel;

	private C1ComboBox cboShowHideNodes;

	private C1SplitterPanel pnlSearch;

	private C1TextBox txtSearch;

	private C1SplitContainer splSearch;

	private C1SplitterPanel pnlSearchIcon;

	private C1SplitterPanel pnlSearchTxt;

	private C1Button btnSearch;

	private WinformProgressBarEx2 progressBar;

	public bool SameDirExcelSaveToOneFile => ckbExportExcel.Checked;

	public Auditai.Model.Project Project { get; set; }

	public List<TreeNodeBase> Selected { get; private set; }

	public List<TreeNodeBase> Unselected { get; private set; }

	public HashSet<Id64> PreSelectNodes { get; set; }

	public C1FlexGrid Grid => _grid;

	public frmNodeSelectorWithTicketRecord()
	{
		InitializeComponent();
		Initialize();
	}

	private Id64 GenerateSingleId()
	{
		return new Id64(_idHighValue, _idLowValue++);
	}

	private new DialogResult ShowDialog()
	{
		base.Size = new Size(471, 594);
		return base.ShowDialog();
	}

	public DialogResult ShowBatchPrinter()
	{
		Text = "批量打印文件";
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.BatchPrint16);
		ckbExportExcel.Visible = false;
		cboShowHideNodes.Visible = false;
		Populate();
		PopulateBatchPrintCheck();
		_grid.MouseClick += _grid_MouseClick_Tooltip_Print;
		return ShowDialog();
	}

	public DialogResult ShowBatchExporter()
	{
		Text = "批量导出文件";
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.BatchExport16);
		ckbExportExcel.Visible = true;
		cboShowHideNodes.Visible = false;
		Populate();
		PopulateBatchPrintCheck();
		_grid.MouseClick += _grid_MouseClick_Tooltip_Export;
		return ShowDialog();
	}

	private void Initialize()
	{
		_grid.Dock = DockStyle.Fill;
		_grid.ExtendLastCol = false;
		_grid.AllowEditing = true;
		_grid.ScrollBars = ScrollBars.Vertical;
		_grid.AllowSorting = AllowSortingEnum.None;
		_grid.DrawMode = DrawModeEnum.OwnerDraw;
		_grid.Tree.Style = TreeStyleFlags.Simple;
		_grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		_grid.Styles.Normal.Border.Width = 0;
		_grid.Rows.DefaultSize = 30;
		_grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		_grid.Glyphs[GlyphEnum.Grayed] = Resources.NoPermission;
		_grid.Rows.Count = 1;
		_grid.Rows.Fixed = 1;
		_grid.Cols.Count = 0;
		_grid.Cols.Fixed = 0;
		_grid.Tree.Column = 0;
		C1.Win.C1FlexGrid.Column column = _grid.Cols.Add();
		column.Caption = StringConstBase.Current.Project + "文件";
		column.Name = "node";
		column.AllowEditing = false;
		column = _grid.Cols.Add();
		column.Caption = "选择";
		column.Name = "check";
		column.AllowEditing = true;
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.ImageAlign = ImageAlignEnum.CenterCenter;
		column.Width = 100;
		_grid.CellChecked += GrdEditor_CellChecked;
		_grid.MouseClick += GrdEditor_MouseClick;
		_grid.Resize += GrdEditor_Resize;
		_grid.AfterResizeColumn += GrdEditor_Resize;
		_grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		_grid.Paint += _grid_Paint;
		_grid.BeforeEdit += GrdEditor_BeforeEdit;
		_grid.AfterCollapse += _grid_AfterCollapse;
		_grid.OwnerDrawCell += _grid_OwnerDrawCell;
		_grid.MouseMove += _grid_MouseMove;
		_grid.KeyDown += _grid_KeyDown;
		_grid.BeforeMouseDown += _grid_BeforeMouseDown;
		_grid.AfterSelChange += _grid_AfterSelChange;
		cmdExpand.Text = "全部展开";
		lnkExpand.Command = cmdExpand;
		cmdExpand.Click += delegate
		{
			_grid.ExpandAll();
			KeepSize();
		};
		cmdCollaspe.Text = "全部收缩";
		lnkCollaspe.Command = cmdCollaspe;
		cmdCollaspe.Click += delegate
		{
			_grid.CollapseAll();
			KeepSize();
		};
		ctxMenu.CommandLinks.Add(lnkExpand);
		ctxMenu.CommandLinks.Add(lnkCollaspe);
		C1CommandHolder c1CommandHolder = new C1CommandHolder
		{
			Owner = this
		};
		c1CommandHolder.SetC1ContextMenu(_grid, ctxMenu);
		txtSearch.TextChanged += TxtSearch_TextChanged;
		txtSearch.KeyDown += TxtSearch_KeyDown;
		pnlButton.SizeChanged += PnlButton_SizeChanged;
		progressBar.Visible = false;
		progressBar.Width = pnlButton.Width;
	}

	private void _grid_AfterSelChange(object sender, RangeEventArgs e)
	{
		_grid.Invalidate();
	}

	private void _grid_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyData = e.KeyData;
		if (keyData == Keys.Space)
		{
			e.Handled = true;
			SwitchSelectRangeCellChecBox();
		}
	}

	protected void SwitchSelectRangeCellChecBox(int mouseClickGridRowIndex = -1)
	{
		C1.Win.C1FlexGrid.CellRange selection = _grid.Selection;
		int num = -1;
		if (mouseClickGridRowIndex != -1)
		{
			if (_grid.Rows[mouseClickGridRowIndex].UserData is TreeTableNode && IsTableNodeExistVisibleTicketNodeRange(_grid.Rows[mouseClickGridRowIndex].Node, selection))
			{
				SwitchCellCheckStatus(mouseClickGridRowIndex);
				return;
			}
			num = mouseClickGridRowIndex;
		}
		else
		{
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				if (i >= _grid.Rows.Fixed && i < _grid.Rows.Count && _grid.Rows[i].Visible && (!(_grid.Rows[i].UserData is TreeTableNode) || !IsTableNodeExistVisibleTicketNodeRange(_grid.Rows[i].Node, selection)))
				{
					num = i;
					break;
				}
			}
		}
		if (num == -1)
		{
			return;
		}
		CheckEnum cellCheck = _grid.GetCellCheck(num, 1);
		cellCheck = ((cellCheck != CheckEnum.Checked) ? CheckEnum.Checked : CheckEnum.Unchecked);
		try
		{
			_grid.CellChecked -= GrdEditor_CellChecked;
			int num2 = -1;
			if (cellCheck == CheckEnum.Unchecked)
			{
				for (int j = selection.TopRow; j <= selection.BottomRow; j++)
				{
					if (j < _grid.Rows.Fixed || j >= _grid.Rows.Count)
					{
						continue;
					}
					if (_grid.GetCellCheck(j, 1) == CheckEnum.Grayed)
					{
						num2 = -1;
						continue;
					}
					C1.Win.C1FlexGrid.Row row = _grid.Rows[j];
					if (row.Visible)
					{
						_grid.SetCellCheck(j, 1, CheckEnum.Unchecked);
						num2 = j;
					}
				}
			}
			else
			{
				for (int k = selection.TopRow; k <= selection.BottomRow; k++)
				{
					if (k < _grid.Rows.Fixed || k >= _grid.Rows.Count)
					{
						continue;
					}
					C1.Win.C1FlexGrid.Row row2 = _grid.Rows[k];
					if (!row2.Visible)
					{
						continue;
					}
					if (_grid.GetCellCheck(k, 1) == CheckEnum.Grayed)
					{
						num2 = -1;
						continue;
					}
					TreeNodeBase treeNodeBase = row2.UserData as TreeNodeBase;
					if (treeNodeBase != null && !treeNodeBase.HasReadPermission())
					{
						num2 = -1;
						_grid.SetCellCheck(k, 1, CheckEnum.Unchecked);
					}
					else if (!(treeNodeBase is TreeTableNode) || !IsTableNodeExistVisibleTicketNodeRange(row2.Node, selection))
					{
						_grid.SetCellCheck(k, 1, CheckEnum.Checked);
						num2 = k;
					}
				}
			}
			if (num2 == -1)
			{
				return;
			}
			object userData = _grid.Rows[num2].UserData;
			Auditai.Model.TreeGroup treeGroup = userData as Auditai.Model.TreeGroup;
			if (treeGroup == null)
			{
				TreeDirectoryNode treeDirectoryNode = userData as TreeDirectoryNode;
				if (treeDirectoryNode == null)
				{
					TicketNavTreePageNode ticketNavTreePageNode = userData as TicketNavTreePageNode;
					if (ticketNavTreePageNode == null)
					{
						TicketNavTreeFolderNode ticketNavTreeFolderNode = userData as TicketNavTreeFolderNode;
						if (ticketNavTreeFolderNode == null)
						{
							return;
						}
					}
				}
			}
			checkChildren(_grid.Rows[num2].Node, cellCheck);
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		finally
		{
			_grid.CellChecked += GrdEditor_CellChecked;
		}
		static bool IsTableNodeExistVisibleTicketNodeRange(Node node, C1.Win.C1FlexGrid.CellRange range)
		{
			if (node.Row.UserData is TicketNavTreeRecordNode && node.Row.Visible && range.ContainsRow(node.Row.Index))
			{
				return true;
			}
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				if (IsTableNodeExistVisibleTicketNodeRange(node2, range))
				{
					return true;
				}
			}
			return false;
		}
	}

	private void SwitchCellCheckStatus(int gridRowIndex)
	{
		if (gridRowIndex < _grid.Rows.Fixed || gridRowIndex >= _grid.Rows.Count)
		{
			return;
		}
		_grid.BeginUpdate();
		try
		{
			_grid.CellChecked -= GrdEditor_CellChecked;
			CheckEnum check;
			switch (_grid.GetCellCheck(gridRowIndex, 1))
			{
			case CheckEnum.Grayed:
				return;
			case CheckEnum.Checked:
				check = CheckEnum.Unchecked;
				break;
			default:
				check = CheckEnum.Checked;
				break;
			}
			_grid.SetCellCheck(gridRowIndex, 1, check);
			object userData = _grid.Rows[gridRowIndex].UserData;
			Auditai.Model.TreeGroup treeGroup = userData as Auditai.Model.TreeGroup;
			if (treeGroup == null)
			{
				TreeDirectoryNode treeDirectoryNode = userData as TreeDirectoryNode;
				if (treeDirectoryNode == null)
				{
					TicketNavTreePageNode ticketNavTreePageNode = userData as TicketNavTreePageNode;
					if (ticketNavTreePageNode == null)
					{
						TicketNavTreeFolderNode ticketNavTreeFolderNode = userData as TicketNavTreeFolderNode;
						if (ticketNavTreeFolderNode == null)
						{
							return;
						}
					}
				}
			}
			checkChildren(_grid.Rows[gridRowIndex].Node, check);
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		finally
		{
			_grid.EndUpdate();
			_grid.CellChecked += GrdEditor_CellChecked;
		}
	}

	private void checkChildren(Node node, CheckEnum check)
	{
		if (node.Row.Visible)
		{
			CheckEnum cellCheck = _grid.GetCellCheck(node.Row.Index, 1);
			if (cellCheck != CheckEnum.Grayed && cellCheck != 0)
			{
				_grid.SetCellCheck(node.Row.Index, 1, check);
			}
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				checkChildren(node2, check);
			}
		}
	}

	private void PnlButton_SizeChanged(object sender, EventArgs e)
	{
		progressBar.Width = pnlButton.Width;
	}

	private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			SearchImpl();
		}
	}

	private void _grid_MouseMove(object sender, MouseEventArgs e)
	{
		MosueOverCellInfo mouseCurrentOverWhichTicketRecordIconCell = _mouseCurrentOverWhichTicketRecordIconCell;
		_mouseCurrentOverWhichTicketRecordIconCell = null;
		if ((Control.MouseButtons & (MouseButtons.Left | MouseButtons.Right)) == 0)
		{
			HitTestInfo hitTestInfo = _grid.HitTest();
			if (hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column == 0)
			{
				_mouseCurrentOverWhichTicketRecordIconCell = new MosueOverCellInfo(hitTestInfo.Row, hitTestInfo.Column);
				_mouseCurrentOverWhichTicketRecordIconCell.MouseLocation = e.Location;
				_grid.Invalidate(_grid.GetCellRect(hitTestInfo.Row, hitTestInfo.Column));
			}
		}
		RepaintOldMouseOverCell(mouseCurrentOverWhichTicketRecordIconCell, _mouseCurrentOverWhichTicketRecordIconCell);
		void RepaintOldMouseOverCell(MosueOverCellInfo oldCell, MosueOverCellInfo newCell)
		{
			if (oldCell != null && (newCell == null || newCell.RowIndex != oldCell.RowIndex || newCell.ColIndex != oldCell.ColIndex))
			{
				_grid.Invalidate(_grid.GetCellRect(oldCell.RowIndex, oldCell.ColIndex));
			}
		}
	}

	private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col == 0 && e.Row >= _grid.Selection.TopRow && e.Row <= _grid.Selection.BottomRow)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[e.Row];
			if (row.UserData is TreeTableNode treeTableNode && treeTableNode.HasReadPermission())
			{
				DrawShowTicketRecordIcon(e);
			}
		}
	}

	private bool IsClickOnTicketRecordIcon(int row, int col, int clickX, int clickY)
	{
		return GetTicketRecordIconPosition(row, col).Contains(clickX, clickY);
	}

	private Rectangle GetTicketRecordIconPosition(int row, int col)
	{
		Rectangle cellRect = _grid.GetCellRect(row, col);
		int num = Resources.TicketNav.Width;
		int num2 = Resources.TicketNav.Height;
		int num3 = cellRect.Right - num - 8;
		int num4 = cellRect.Top + (cellRect.Height - num2) / 2;
		return new Rectangle(num3, num4, num, num2);
	}

	private void DrawShowTicketRecordIcon(OwnerDrawCellEventArgs e)
	{
		e.DrawCell(DrawCellFlags.Background | DrawCellFlags.Content);
		Rectangle ticketRecordIconPosition = GetTicketRecordIconPosition(e.Row, e.Col);
		if (_mouseCurrentOverWhichTicketRecordIconCell != null && _mouseCurrentOverWhichTicketRecordIconCell.RowIndex == e.Row && _mouseCurrentOverWhichTicketRecordIconCell.ColIndex == e.Col && ticketRecordIconPosition.Contains(_mouseCurrentOverWhichTicketRecordIconCell.MouseLocation))
		{
			Rectangle rect = new Rectangle(ticketRecordIconPosition.X - 3, ticketRecordIconPosition.Y - 3, ticketRecordIconPosition.Width + 6, ticketRecordIconPosition.Height + 6);
			_ticketRecordIconOnFocusBackgrndBrush.Color = Auditai.UI.Controls.Util.DarkenColor(_grid.Styles.SelectedColumnHeader.BackColor, 0.1);
			e.Graphics.FillRectangle(_ticketRecordIconOnFocusBackgrndBrush, rect);
		}
		e.Graphics.DrawImage(Resources.TicketNav, ticketRecordIconPosition);
		e.DrawCell(DrawCellFlags.Border);
		e.Handled = true;
	}

	private void _grid_AfterCollapse(object sender, RowColEventArgs e)
	{
		KeepSize();
	}

	private void KeepSize()
	{
		int num = (_grid.ScrollBarsVisible.HasFlag(ScrollBars.Vertical) ? SystemInformation.VerticalScrollBarWidth : 0);
		_grid.Cols["check"].Width = 100;
		_grid.Cols["node"].Width = base.Width - 100 - num;
	}

	private void Populate()
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
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
									if (rootNode is TreePdfNode { Visible: not false } treePdfNode)
									{
										node2 = node.AddNode(NodeTypeEnum.LastChild, treePdfNode.Number + " " + treePdfNode.Name, treePdfNode, Resources.TreePdf);
									}
								}
								else if (treeImageNode.Visible)
								{
									node2 = node.AddNode(NodeTypeEnum.LastChild, treeImageNode.Number + " " + treeImageNode.Name, treeImageNode, Resources.TreeImage);
								}
							}
							else if (treeDocumentNode.Visible)
							{
								node2 = node.AddNode(NodeTypeEnum.LastChild, treeDocumentNode.Number + " " + treeDocumentNode.Name, treeDocumentNode, Resources.TreeDoc);
							}
						}
						else if (treeTableNode.Visible)
						{
							node2 = node.AddNode(NodeTypeEnum.LastChild, treeTableNode.Number + " " + treeTableNode.Name, treeTableNode, Resources.TreeTable);
						}
					}
					else if (treeDirectoryNode.Visible)
					{
						node2 = node.AddNode(NodeTypeEnum.LastChild, treeDirectoryNode.Number + " " + treeDirectoryNode.Name, treeDirectoryNode, Resources.TreeDir);
						AddDirectoryNode(treeDirectoryNode, node2);
					}
					if (node2 != null && PreSelectNodes != null && PreSelectNodes.Contains(rootNode.Id))
					{
						_grid.SetCellCheck(node2.Row.Index, _grid.Cols[1].Index, CheckEnum.Checked);
					}
				}
			}
			C1.Win.C1FlexGrid.CellStyle cellStyle = _grid.Styles.Add("check");
			cellStyle.TextAlign = TextAlignEnum.CenterCenter;
			cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
			for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
			{
				if (_grid.GetCellCheck(i, 1) == CheckEnum.None)
				{
					_grid.SetCellCheck(i, 1, CheckEnum.Unchecked);
				}
				_grid.SetCellStyle(i, "check", cellStyle);
				_grid.Rows[i].Node.Collapsed = true;
			}
			_grid.Cols["node"].TextAlign = TextAlignEnum.LeftCenter;
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
								if (child is TreePdfNode { Visible: not false } treePdfNode2)
								{
									node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treePdfNode2.Number + " " + treePdfNode2.Name, treePdfNode2, Resources.TreePdf);
								}
							}
							else if (treeImageNode2.Visible)
							{
								node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeImageNode2.Number + " " + treeImageNode2.Name, treeImageNode2, Resources.TreeImage);
							}
						}
						else if (treeDocumentNode2.Visible)
						{
							node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeDocumentNode2.Number + " " + treeDocumentNode2.Name, treeDocumentNode2, Resources.TreeDoc);
						}
					}
					else if (treeTableNode2.Visible)
					{
						node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeTableNode2.Number + " " + treeTableNode2.Name, treeTableNode2, Resources.TreeTable);
					}
				}
				else if (treeDirectoryNode2.Visible)
				{
					node3 = subRootView.AddNode(NodeTypeEnum.LastChild, treeDirectoryNode2.Number + " " + treeDirectoryNode2.Name, treeDirectoryNode2, Resources.TreeDir);
					AddDirectoryNode(treeDirectoryNode2, node3);
				}
				if (node3 != null && PreSelectNodes != null && PreSelectNodes.Contains(child.Id))
				{
					_grid.SetCellCheck(node3.Row.Index, 1, CheckEnum.Checked);
				}
			}
		}
	}

	private void AddTicketNode(Node parentNode, TreeTableNode table)
	{
		if (_hashLoadTicketTable.Contains(table.Id))
		{
			return;
		}
		_hashLoadTicketTable.Add(table.Id);
		table.Table.LoadAndReturn();
		if (table.Table.IsCorrupted)
		{
			_grid.SetCellCheck(parentNode.Row.Index, 1, CheckEnum.Checked);
		}
		else
		{
			if (table.Table.Ticket.IsEmpty() || !table.HasReadPermission())
			{
				return;
			}
			foreach (TicketNav nav in table.Table.Ticket.Navs)
			{
				TicketNavTreePageNode ticketNavTreePageNode = new TicketNavTreePageNode
				{
					NodeId = GenerateSingleId(),
					TicketNavTree = nav,
					IsReocrdGenerated = false,
					Ticket = table.Table.Ticket
				};
				ticketNavTreePageNode.TicketColumnList = nav.Columns.Select((Id64 id) => table.Table.Columns.GetById(id)).ToList();
				ticketNavTreePageNode.Name = TicketInputEditor2.TicketNavToString(ticketNavTreePageNode.TicketColumnList);
				ticketNavTreePageNode.ParentNode = table;
				ticketNavTreePageNode.GridNode = parentNode.AddNode(NodeTypeEnum.LastChild, ticketNavTreePageNode.Name ?? "", ticketNavTreePageNode, Resources.TicketNav);
				_grid.SetCellCheck(ticketNavTreePageNode.GridNode.Row.Index, 1, CheckEnum.Unchecked);
				MakeNavTree(table, ticketNavTreePageNode);
			}
		}
	}

	private void MakeNavTree(TreeTableNode tableNode, TicketNavTreePageNode rootNode)
	{
		if (rootNode.TicketColumnList == null || rootNode.TicketColumnList.Count == 0)
		{
			return;
		}
		TicketNavGrid.NavNode navNode = new TicketNavGrid.NavNode();
		Auditai.Model.Column column = rootNode.TicketColumnList.Last();
		foreach (TicketRecord record in rootNode.Ticket.Records)
		{
			Auditai.Model.Row row = record.Rows[0];
			TicketNavGrid.NavNode navNode2 = navNode;
			foreach (Auditai.Model.Column ticketColumn in rootNode.TicketColumnList)
			{
				string displayValue = row.Table[row.Index, ticketColumn.Index].GetDisplayValue();
				if (column == ticketColumn)
				{
					navNode2 = navNode2.AddLastCol(displayValue);
					navNode2.Record = record;
				}
				else
				{
					navNode2 = navNode2.AddOrGet(displayValue);
				}
			}
		}
		foreach (TicketNavGrid.NavNode child in navNode.Children())
		{
			BuildGridNodeTree(tableNode, rootNode, child, rootNode.GridNode);
		}
		void BuildGridNodeTree(TreeTableNode tableNode, TreeNodeBase parentTreeNode, TicketNavGrid.NavNode node, Node parentGridNode)
		{
			if (node.Children().Length == 0)
			{
				TicketNavTreeRecordNode ticketNavTreeRecordNode = new TicketNavTreeRecordNode();
				ticketNavTreeRecordNode.Record = node.Record;
				ticketNavTreeRecordNode.Name = (string.IsNullOrWhiteSpace(node.Text) ? "(空)" : node.Text);
				ticketNavTreeRecordNode.ParentNode = parentTreeNode;
				ticketNavTreeRecordNode.TableNode = tableNode;
				ticketNavTreeRecordNode.GridNode = parentGridNode.AddNode(NodeTypeEnum.LastChild, ticketNavTreeRecordNode.Name, ticketNavTreeRecordNode, Resources.Ticket16);
				_grid.SetCellCheck(ticketNavTreeRecordNode.GridNode.Row.Index, 1, CheckEnum.Unchecked);
				return;
			}
			TicketNavTreeFolderNode ticketNavTreeFolderNode = new TicketNavTreeFolderNode();
			ticketNavTreeFolderNode.Name = (string.IsNullOrWhiteSpace(node.Text) ? "(空)" : node.Text);
			ticketNavTreeFolderNode.ParentNode = parentTreeNode;
			ticketNavTreeFolderNode.GridNode = parentGridNode.AddNode(NodeTypeEnum.LastChild, ticketNavTreeFolderNode.Name, ticketNavTreeFolderNode, Resources.TicketNavTreeListExpanded);
			_grid.SetCellCheck(ticketNavTreeFolderNode.GridNode.Row.Index, 1, CheckEnum.Unchecked);
			foreach (TicketNavGrid.NavNode child2 in node.Children())
			{
				BuildGridNodeTree(tableNode, ticketNavTreeFolderNode, child2, ticketNavTreeFolderNode.GridNode);
			}
		}
	}

	private void PopulateBatchPrintCheck()
	{
		for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
		{
			if (_grid.Rows[i].UserData is TreeNodeBase treeNodeBase && !treeNodeBase.HasReadPermission())
			{
				_grid.SetCellCheck(i, 1, CheckEnum.Grayed);
			}
		}
	}

	private void SetTheme()
	{
		ctnAll.SplitterWidth = 0;
	}

	private void GrdEditor_Resize(object sender, EventArgs e)
	{
		_grid.BeginUpdate();
		KeepSize();
		_grid.EndUpdate();
	}

	private void _grid_Paint(object sender, PaintEventArgs e)
	{
		_grid.DrawFormBorder(e.Graphics);
	}

	private void GrdEditor_BeforeEdit(object sender, RowColEventArgs e)
	{
		if (e.Col == 1)
		{
			e.Cancel = true;
		}
	}

	private void _grid_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestInfo hitTestInfo = _grid.HitTest(e.X, e.Y);
		if (hitTestInfo.Type == HitTestTypeEnum.Checkbox && hitTestInfo.Column == 1)
		{
			if (_grid.Selection.Contains(hitTestInfo.Row, hitTestInfo.Column))
			{
				SwitchSelectRangeCellChecBox(hitTestInfo.Row);
			}
			else
			{
				SwitchCellCheckStatus(hitTestInfo.Row);
			}
			e.Cancel = true;
		}
	}

	private void GrdEditor_MouseClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = _grid.HitTest(e.Location);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (hitTestInfo.Type == HitTestTypeEnum.Cell && hitTestInfo.Column == 0)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[hitTestInfo.Row];
			Node node = row.Node;
			if (row.UserData is TreeTableNode table && IsClickOnTicketRecordIcon(hitTestInfo.Row, hitTestInfo.Column, e.X, e.Y))
			{
				AddTicketNode(node, table);
				node.Expanded = true;
			}
			else
			{
				node.Collapsed = !node.Collapsed;
			}
		}
		else if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader && _grid.Rows.Count - _grid.Rows.Fixed > 0)
		{
			_grid.SafeSelect(_grid.Rows.Fixed, hitTestInfo.Column, _grid.Rows.Count - 1, hitTestInfo.Column, isToShow: false);
		}
	}

	private void GrdEditor_CellChecked(object sender, RowColEventArgs e)
	{
		try
		{
			_grid.CellChecked -= GrdEditor_CellChecked;
			CheckEnum cellCheck = _grid.GetCellCheck(e.Row, e.Col);
			object userData = _grid.Rows[e.Row].UserData;
			Auditai.Model.TreeGroup treeGroup = userData as Auditai.Model.TreeGroup;
			if (treeGroup == null)
			{
				TreeDirectoryNode treeDirectoryNode = userData as TreeDirectoryNode;
				if (treeDirectoryNode == null)
				{
					TicketNavTreePageNode ticketNavTreePageNode = userData as TicketNavTreePageNode;
					if (ticketNavTreePageNode == null)
					{
						TicketNavTreeFolderNode ticketNavTreeFolderNode = userData as TicketNavTreeFolderNode;
						if (ticketNavTreeFolderNode == null)
						{
							return;
						}
					}
				}
			}
			_grid.BeginUpdate();
			try
			{
				checkChildren(_grid.Rows[e.Row].Node, cellCheck);
			}
			finally
			{
				_grid.EndUpdate();
			}
		}
		finally
		{
			_grid.CellChecked += GrdEditor_CellChecked;
		}
		void checkChildren(Node node, CheckEnum check)
		{
			if (node.Row.Visible)
			{
				CheckEnum cellCheck2 = _grid.GetCellCheck(node.Row.Index, 1);
				if (cellCheck2 != CheckEnum.Grayed && cellCheck2 != 0)
				{
					_grid.SetCellCheck(node.Row.Index, 1, check);
				}
				Node[] nodes = node.Nodes;
				foreach (Node node2 in nodes)
				{
					checkChildren(node2, check);
				}
			}
		}
	}

	private void _grid_MouseClick_Tooltip_Print(object sender, MouseEventArgs e)
	{
		_ttp.Hide();
		if (_grid.Col == 1 && _grid.GetCellCheck(_grid.Row, _grid.Col) == CheckEnum.Grayed && _grid.Rows[_grid.Row].UserData is TreeNodeBase treeNodeBase && !treeNodeBase.HasReadPermission())
		{
			string content = "<p style=\"color:red\">因您没有该文件的【查看】权限，因此，无法对该文件执行打印操作。</p>";
			_ttp.SetText("操作提示", content);
			Rectangle cellRect = _grid.GetCellRect(_grid.Row, _grid.Col);
			_ttp.Show(_grid, new Point(cellRect.Right, cellRect.Top + cellRect.Height / 2));
		}
	}

	private void _grid_MouseClick_Tooltip_Export(object sender, MouseEventArgs e)
	{
		_ttp.Hide();
		if (_grid.Col == 1 && _grid.GetCellCheck(_grid.Row, _grid.Col) == CheckEnum.Grayed && _grid.Rows[_grid.Row].UserData is TreeNodeBase treeNodeBase && !treeNodeBase.HasReadPermission())
		{
			string content = "<p style=\"color:red\">因您没有该文件的【查看】权限，因此，无法对该文件执行导出操作。</p>";
			_ttp.SetText("操作提示", content);
			Rectangle cellRect = _grid.GetCellRect(_grid.Row, _grid.Col);
			_ttp.Show(_grid, new Point(cellRect.Right, cellRect.Top + cellRect.Height / 2));
		}
	}

	private void btnCertain_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Selected = new List<TreeNodeBase>();
		Unselected = new List<TreeNodeBase>();
		foreach (C1.Win.C1FlexGrid.Row item2 in (IEnumerable)_grid.Rows)
		{
			if (item2.UserData is TreeNodeBase item)
			{
				switch (_grid.GetCellCheck(item2.Index, 1))
				{
				case CheckEnum.Checked:
					Selected.Add(item);
					break;
				case CheckEnum.Unchecked:
					Unselected.Add(item);
					break;
				}
			}
		}
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private List<TreeNodeBase> GetAllTreeNodesExclueDirNode()
	{
		List<TreeNodeBase> list = new List<TreeNodeBase>();
		int count = _grid.Rows.Count;
		for (int i = _grid.Rows.Fixed; i < count; i++)
		{
			C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
			if (row.UserData is TreeNodeBase treeNodeBase && !(treeNodeBase is TreeDirectoryNode) && !(treeNodeBase is TicketNavTreeFolderNode))
			{
				list.Add(treeNodeBase);
			}
		}
		return list;
	}

	private void TxtSearch_TextChanged(object sender, EventArgs e)
	{
		try
		{
			BeginInvoke(new Action(SearchImpl));
		}
		catch { }
	}

	private void GenerateAllTickeNode()
	{
		if (_isAllTicketGenerated)
		{
			return;
		}
		_grid.BeginUpdate();
		try
		{
			progressBar.ForeColor = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeContext.ProgressBarColor;
			progressBar.Visible = true;
			AddTicketNodeStepByStepRuner addTicketNodeStepByStepRuner = new AddTicketNodeStepByStepRuner(this);
			IEnumerator enumerator = addTicketNodeStepByStepRuner.AddTicketNodeStepByStep();
			while (enumerator.MoveNext())
			{
			}
		}
		finally
		{
			_grid.EndUpdate();
			progressBar.Visible = false;
		}
		_isAllTicketGenerated = true;
	}

	private void SearchImpl()
	{
		if (_isInSearching)
		{
			return;
		}
		_isInSearching = true;
		pnlEditor.SuspendDrawing();
		try
		{
			GenerateAllTickeNode();
			List<TreeNodeBase> collection = (from n in GetAllTreeNodesExclueDirNode()
				select Tuple.Create(n, FuzzySearch.Filter(n.Name, txtSearch.Text)) into tup
				where tup.Item2 > 0
				orderby tup.Item2 descending
				select tup.Item1).ToList();
			HashSet<TreeNodeBase> hashSet = new HashSet<TreeNodeBase>(collection);
			HashSet<Auditai.Model.TreeGroup> hashSet2 = new HashSet<Auditai.Model.TreeGroup>();
			foreach (TreeNodeBase item3 in hashSet)
			{
				hashSet2.Add(item3.Group);
			}
			_grid.BeginUpdate();
			for (int i = _grid.Rows.Fixed; i < _grid.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = _grid.Rows[i];
				if (row.UserData is Auditai.Model.TreeGroup item)
				{
					row.Visible = hashSet2.Contains(item);
					if (row.Visible)
					{
						row.Node.Expanded = true;
					}
				}
				else if (row.UserData is TreeNodeBase item2)
				{
					if (hashSet.Contains(item2))
					{
						row.Visible = true;
						row.Node.Expanded = true;
						MakeParentNodeVisibleAndExpand(row.Node);
					}
					else
					{
						row.Visible = false;
					}
				}
			}
			_grid.EndUpdate();
			KeepSize();
		}
		catch (Exception exception)
		{
			exception.Log();
		}
		finally
		{
			_isInSearching = false;
			pnlEditor.ResumeDrawing();
		}
	}

	private void MakeParentNodeVisibleAndExpand(Node node)
	{
		if (node.Parent != null)
		{
			Node node2 = node.Parent;
			if (node2.Row != null && !node2.Row.Visible)
			{
				node2.Row.Visible = true;
				node2.Expanded = true;
				MakeParentNodeVisibleAndExpand(node2);
			}
		}
	}

	private void btnSearch_Click(object sender, EventArgs e)
	{
		try
		{
			SearchImpl();
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void checkBoxSearchTicket_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			BeginInvoke(new Action(SearchImpl));
		}
		catch (Exception exception)
		{
			exception.Log();
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
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButton = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.progressBar = new Auditai.UI.Controls.WinformProgressBarEx2();
		this.cboShowHideNodes = new C1.Win.C1Input.C1ComboBox();
		this.ckbExportExcel = new C1.Win.C1Input.C1CheckBox();
		this.btnCertain = new C1.Win.C1Input.C1Button();
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.pnlSearch = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.splSearch = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlSearchIcon = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnSearch = new C1.Win.C1Input.C1Button();
		this.pnlSearchTxt = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.txtSearch = new C1.Win.C1Input.C1TextBox();
		this.pnlEditor = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this._grid = new Auditai.UI.Controls.C1FlexGridEx();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlButton.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cboShowHideNodes).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbExportExcel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		this.pnlSearch.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splSearch).BeginInit();
		this.splSearch.SuspendLayout();
		this.pnlSearchIcon.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnSearch).BeginInit();
		this.pnlSearchTxt.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtSearch).BeginInit();
		this.pnlEditor.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this._grid).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnAll.HeaderHeight = 27;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlButton);
		this.ctnAll.Panels.Add(this.pnlSearch);
		this.ctnAll.Panels.Add(this.pnlEditor);
		this.ctnAll.Size = new System.Drawing.Size(409, 563);
		this.ctnAll.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlButton.Controls.Add(this.progressBar);
		this.pnlButton.Controls.Add(this.cboShowHideNodes);
		this.pnlButton.Controls.Add(this.ckbExportExcel);
		this.pnlButton.Controls.Add(this.btnCertain);
		this.pnlButton.Controls.Add(this.btnCancel);
		this.pnlButton.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButton.Height = 40;
		this.pnlButton.KeepRelativeSize = false;
		this.pnlButton.Location = new System.Drawing.Point(0, 523);
		this.pnlButton.MinHeight = 20;
		this.pnlButton.Name = "pnlButton";
		this.pnlButton.Size = new System.Drawing.Size(409, 40);
		this.pnlButton.SizeRatio = 10.0;
		this.pnlButton.TabIndex = 1;
		this.progressBar.ForeColor = System.Drawing.Color.Green;
		this.progressBar.Location = new System.Drawing.Point(0, 35);
		this.progressBar.Name = "progressBar";
		this.progressBar.Size = new System.Drawing.Size(409, 5);
		this.progressBar.TabIndex = 6;
		this.cboShowHideNodes.AllowSpinLoop = false;
		this.cboShowHideNodes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.cboShowHideNodes.GapHeight = 0;
		this.cboShowHideNodes.ImagePadding = new System.Windows.Forms.Padding(0);
		this.cboShowHideNodes.ItemsDisplayMember = "";
		this.cboShowHideNodes.ItemsValueMember = "";
		this.cboShowHideNodes.Location = new System.Drawing.Point(5, 10);
		this.cboShowHideNodes.Name = "cboShowHideNodes";
		this.cboShowHideNodes.Size = new System.Drawing.Size(200, 21);
		this.cboShowHideNodes.TabIndex = 5;
		this.cboShowHideNodes.Tag = null;
		this.ckbExportExcel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ckbExportExcel.BackColor = System.Drawing.Color.Transparent;
		this.ckbExportExcel.BorderColor = System.Drawing.Color.Transparent;
		this.ckbExportExcel.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbExportExcel.ForeColor = System.Drawing.Color.Black;
		this.ckbExportExcel.Location = new System.Drawing.Point(5, 11);
		this.ckbExportExcel.Name = "ckbExportExcel";
		this.ckbExportExcel.Padding = new System.Windows.Forms.Padding(1);
		this.ckbExportExcel.Size = new System.Drawing.Size(233, 24);
		this.ckbExportExcel.TabIndex = 3;
		this.ckbExportExcel.Text = "同文件夹下表格导出至一个Excel文件";
		this.ckbExportExcel.UseVisualStyleBackColor = true;
		this.ckbExportExcel.Value = null;
		this.btnCertain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCertain.Location = new System.Drawing.Point(235, 8);
		this.btnCertain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCertain.Name = "btnCertain";
		this.btnCertain.Size = new System.Drawing.Size(70, 26);
		this.btnCertain.TabIndex = 1;
		this.btnCertain.Text = "确定";
		this.btnCertain.UseVisualStyleBackColor = true;
		this.btnCertain.Click += new System.EventHandler(btnCertain_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(322, 8);
		this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.pnlSearch.Controls.Add(this.splSearch);
		this.pnlSearch.Height = 24;
		this.pnlSearch.KeepRelativeSize = false;
		this.pnlSearch.Location = new System.Drawing.Point(0, 0);
		this.pnlSearch.MinHeight = 0;
		this.pnlSearch.Name = "pnlSearch";
		this.pnlSearch.Resizable = false;
		this.pnlSearch.Size = new System.Drawing.Size(409, 24);
		this.pnlSearch.SizeRatio = 4.598;
		this.pnlSearch.TabIndex = 2;
		this.splSearch.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.splSearch.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.splSearch.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splSearch.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.splSearch.HeaderHeight = 27;
		this.splSearch.Location = new System.Drawing.Point(0, 0);
		this.splSearch.Name = "splSearch";
		this.splSearch.Panels.Add(this.pnlSearchIcon);
		this.splSearch.Panels.Add(this.pnlSearchTxt);
		this.splSearch.Size = new System.Drawing.Size(409, 24);
		this.splSearch.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.splSearch.SplitterWidth = 0;
		this.splSearch.TabIndex = 1;
		this.splSearch.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlSearchIcon.Controls.Add(this.btnSearch);
		this.pnlSearchIcon.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Right;
		this.pnlSearchIcon.Height = 24;
		this.pnlSearchIcon.KeepRelativeSize = false;
		this.pnlSearchIcon.Location = new System.Drawing.Point(385, 0);
		this.pnlSearchIcon.MinHeight = 30;
		this.pnlSearchIcon.MinWidth = 0;
		this.pnlSearchIcon.Name = "pnlSearchIcon";
		this.pnlSearchIcon.Resizable = false;
		this.pnlSearchIcon.Size = new System.Drawing.Size(24, 24);
		this.pnlSearchIcon.SizeRatio = 5.882;
		this.pnlSearchIcon.TabIndex = 0;
		this.pnlSearchIcon.Width = 24;
		this.btnSearch.FlatAppearance.BorderSize = 0;
		this.btnSearch.Image = Auditai.UI.Platform.Properties.Resources.btnSearch;
		this.btnSearch.Location = new System.Drawing.Point(-1, -1);
		this.btnSearch.Name = "btnSearch";
		this.btnSearch.Size = new System.Drawing.Size(26, 26);
		this.btnSearch.TabIndex = 0;
		this.btnSearch.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
		this.btnSearch.UseVisualStyleBackColor = true;
		this.btnSearch.Click += new System.EventHandler(btnSearch_Click);
		this.pnlSearchTxt.Controls.Add(this.txtSearch);
		this.pnlSearchTxt.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Right;
		this.pnlSearchTxt.Height = 24;
		this.pnlSearchTxt.Location = new System.Drawing.Point(0, 0);
		this.pnlSearchTxt.MinHeight = 25;
		this.pnlSearchTxt.Name = "pnlSearchTxt";
		this.pnlSearchTxt.Resizable = false;
		this.pnlSearchTxt.Size = new System.Drawing.Size(384, 24);
		this.pnlSearchTxt.SizeRatio = 100.0;
		this.pnlSearchTxt.TabIndex = 1;
		this.pnlSearchTxt.Width = 384;
		this.txtSearch.AutoSize = false;
		this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtSearch.Location = new System.Drawing.Point(0, 0);
		this.txtSearch.Margin = new System.Windows.Forms.Padding(0);
		this.txtSearch.Name = "txtSearch";
		this.txtSearch.Size = new System.Drawing.Size(384, 24);
		this.txtSearch.TabIndex = 0;
		this.txtSearch.Tag = null;
		this.pnlEditor.Controls.Add(this._grid);
		this.pnlEditor.Height = 498;
		this.pnlEditor.Location = new System.Drawing.Point(0, 25);
		this.pnlEditor.MinHeight = 52;
		this.pnlEditor.MinWidth = 52;
		this.pnlEditor.Name = "pnlEditor";
		this.pnlEditor.Size = new System.Drawing.Size(409, 498);
		this.pnlEditor.TabIndex = 0;
		this.pnlEditor.Width = 409;
		this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle;
		this._grid.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this._grid.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this._grid.FocusRect = C1.Win.C1FlexGrid.FocusRectEnum.None;
		this._grid.Location = new System.Drawing.Point(0, 0);
		this._grid.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this._grid.Name = "_grid";
		this._grid.Rows.DefaultSize = 20;
		this._grid.Size = new System.Drawing.Size(409, 498);
		this._grid.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(409, 563);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmNodeSelectorWithTicketRecord";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "样式刷";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlButton.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.cboShowHideNodes).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbExportExcel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCertain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		this.pnlSearch.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splSearch).EndInit();
		this.splSearch.ResumeLayout(false);
		this.pnlSearchIcon.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.btnSearch).EndInit();
		this.pnlSearchTxt.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.txtSearch).EndInit();
		this.pnlEditor.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this._grid).EndInit();
		base.ResumeLayout(false);
	}
}
