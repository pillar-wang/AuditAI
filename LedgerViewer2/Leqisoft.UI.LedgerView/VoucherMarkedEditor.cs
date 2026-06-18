using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

public class VoucherMarkedEditor : ISetTheme
{
	private enum NodeType
	{
		None,
		Voucher,
		Account,
		AuxiliaryItem
	}

	private class VoucherAccountTreeNodeData
	{
		public NodeType nodeType;

		public Account account;

		public Voucher voucher;

		public AuxiliaryItem auxiliaryItem;

		public List<VoucherAccountTreeNodeData> children;

		public AuxiliaryClass inShowingAuxClass;

		public VoucherAccountTreeNodeData()
		{
			nodeType = NodeType.None;
		}

		public VoucherAccountTreeNodeData(Voucher v)
		{
			voucher = v;
			nodeType = NodeType.Voucher;
		}

		public VoucherAccountTreeNodeData(Account acc)
		{
			account = acc;
			nodeType = NodeType.Account;
		}

		public VoucherAccountTreeNodeData(AuxiliaryItem item)
		{
			auxiliaryItem = item;
			nodeType = NodeType.AuxiliaryItem;
		}

		public string GetTreeDisplayCode()
		{
			if (nodeType == NodeType.Account)
			{
				return account.Code;
			}
			if (nodeType == NodeType.AuxiliaryItem)
			{
				if (account != null)
				{
					return account.Code + "-" + auxiliaryItem.Code;
				}
				return auxiliaryItem.Code;
			}
			return string.Empty;
		}

		public List<AuxiliaryClass> GetAllAuxiliaryClass()
		{
			if (children == null)
			{
				return null;
			}
			HashSet<AuxiliaryClass> hashSet = new HashSet<AuxiliaryClass>();
			foreach (VoucherAccountTreeNodeData child in children)
			{
				if (child.nodeType != NodeType.Voucher || child.voucher.Details == null || child.voucher.Details.Count == 0)
				{
					continue;
				}
				foreach (AuxiliaryItem detail in child.voucher.Details)
				{
					if (detail.Class != null && !hashSet.Contains(detail.Class))
					{
						hashSet.Add(detail.Class);
					}
				}
			}
			if (hashSet.Count == 0)
			{
				return null;
			}
			return hashSet.OrderBy((AuxiliaryClass u) => u.Code).ToList();
		}

		public List<AuxiliaryItem> GetAuxiliaryItemByAuxiliaryClass(AuxiliaryClass auxClass)
		{
			if (children == null)
			{
				return null;
			}
			HashSet<AuxiliaryItem> hashSet = new HashSet<AuxiliaryItem>();
			foreach (VoucherAccountTreeNodeData child in children)
			{
				if (child.nodeType != NodeType.Voucher || child.voucher.Details == null || child.voucher.Details.Count == 0)
				{
					continue;
				}
				foreach (AuxiliaryItem detail in child.voucher.Details)
				{
					if (detail.Class != null && detail.Class == auxClass)
					{
						hashSet.Add(detail);
					}
				}
			}
			if (hashSet.Count == 0)
			{
				return null;
			}
			return hashSet.OrderBy((AuxiliaryItem u) => u.Code).ToList();
		}
	}

	private LedgerViewer _owner;

	private C1.Win.C1FlexGrid.CellStyle _csCenter;

	private readonly C1Command cmdCopy;

	private readonly C1CommandLink lnkCopy;

	private readonly C1Command cmdCopyDetail;

	private readonly C1CommandLink lnkCopyDetail;

	private readonly C1ContextMenu mnuVoucher;

	private readonly C1ContextMenu mnuVoucherColumn;

	private readonly C1ContextMenu mnuDetail;

	private readonly C1Command cmdSort;

	private readonly C1CommandLink lnkSort;

	private readonly C1Command cmdSortDesc;

	private readonly C1CommandLink lnkSortDesc;

	private readonly C1Command cmdCancelSort;

	private readonly C1CommandLink lnkCancelSort;

	private readonly C1ContextMenu ctxTree;

	private readonly C1CommandLink lnkExpandAll;

	private readonly C1CommandLink lnkCollaspeAll;

	private readonly SolidBrush _brushHoverBackground = new SolidBrush(Color.Transparent);

	private SolidBrush _adjustLevelIconOnFocusBackgroundBrush = new SolidBrush(Color.Gray);

	private System.Drawing.Image zb1Image = Resources.zb1;

	private System.Drawing.Image zb2Image = Resources.zb2;

	private System.Drawing.Image zb3Image = Resources.zb3;

	private System.Drawing.Image zb4Image = Resources.zb4;

	private C1.Win.C1FlexGrid.Row _currentInFilterRow;

	private Account _currentInFilterAccount;

	private object _currentInFilterAuxiliary;

	private int _mouseRow = -1;

	private C1SplitterPanel pnlVouchers;

	internal C1SplitterPanel pnlDetails;

	private C1SplitContainer ctnVouchers;

	private C1SplitterPanel pnlVoucherTitle;

	private C1SplitterPanel pnlVoucherGrid;

	private C1SplitContainer ctnDetails;

	private C1SplitterPanel pnlDetailHead;

	private C1SplitterPanel pnlDetailGrid;

	private C1SplitterPanel pnlDetailFoot;

	private C1Label lblVoucherTitle;

	internal C1Label lblDetailMaker;

	internal C1Label lblDetailBooker;

	internal C1Label lblDetailChecker;

	internal C1Label lblDetailType;

	internal C1Label lblDetailNumber;

	internal C1Label lblDetailDate;

	internal C1Label lblDetailAttachNum;

	private C1Label lblDetailTitle;

	internal C1FlexGridEx grdVouchers;

	internal C1FlexGridEx grdDetail;

	private bool _isMouseOverCancelMyMarkIcon;

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1SplitterPanel pnlSidebar;

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	public Ledger Ledger => _owner.Ledger;

	public C1SplitContainer View { get; private set; }

	public C1FlexGridEx Tree { get; private set; }

	public bool PendingAllEvent { get; set; }

	public VoucherMarkedEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitComponent();
		Initialize();
		mnuVoucher = new C1ContextMenu();
		cmdCopy = new C1Command
		{
			Text = "复制",
			Image = ContextResources.ctxCopy
		};
		cmdCopy.Click += CmdCopy_Click;
		lnkCopy = new C1CommandLink(cmdCopy);
		mnuVoucher.CommandLinks.Add(lnkCopy);
		mnuVoucher.CommandLinks.Add(grdVouchers.FilterManager.GenLnkFilter());
		mnuVoucher.CommandLinks.Add(grdVouchers.FilterManager.GenLnkSample());
		mnuVoucher.CommandLinks.Add(grdVouchers.FilterManager.GenLnkSelect());
		mnuVoucher.CommandLinks.Add(grdVouchers.FilterManager.GenLnkCancelCurrentColumn());
		mnuVoucherColumn = new C1ContextMenu();
		cmdSortDesc = new C1Command
		{
			Text = "降序排序",
			Image = ContextResources.ctxDescending
		};
		cmdSortDesc.CommandStateQuery += CmdSortDesc_CommandStateQuery;
		cmdSortDesc.Click += CmdSortDesc_Click;
		lnkSortDesc = new C1CommandLink(cmdSortDesc);
		mnuVoucherColumn.CommandLinks.Add(lnkSortDesc);
		cmdSort = new C1Command
		{
			Text = "升序排序",
			Image = ContextResources.ctxAscending
		};
		cmdSort.CommandStateQuery += CmdSort_CommandStateQuery;
		cmdSort.Click += CmdSort_Click;
		lnkSort = new C1CommandLink(cmdSort);
		mnuVoucherColumn.CommandLinks.Add(lnkSort);
		cmdCancelSort = new C1Command
		{
			Text = "取消排序"
		};
		cmdCancelSort.CommandStateQuery += CmdCancelSort_CommandStateQuery;
		cmdCancelSort.Click += CmdCancelSort_Click;
		lnkCancelSort = new C1CommandLink(cmdCancelSort);
		mnuVoucherColumn.CommandLinks.Add(lnkCancelSort);
		mnuDetail = new C1ContextMenu();
		cmdCopyDetail = new C1Command
		{
			Text = "复制",
			Image = ContextResources.ctxCopy
		};
		cmdCopyDetail.Click += CmdCopyDetail_Click;
		lnkCopyDetail = new C1CommandLink(cmdCopyDetail);
		mnuDetail.CommandLinks.Add(lnkCopyDetail);
		mnuDetail.CommandLinks.Add(grdDetail.FilterManager.GenLnkFilter());
		mnuDetail.CommandLinks.Add(grdDetail.FilterManager.GenLnkSample());
		mnuDetail.CommandLinks.Add(grdDetail.FilterManager.GenLnkSelect());
		mnuDetail.CommandLinks.Add(grdDetail.FilterManager.GenLnkCancelCurrentColumn());
		ctxTree = new C1ContextMenu();
		ctxTree.Popup += CtxTree_Popup;
		lnkExpandAll = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "全部展开";
		c1Command.Click += CmdExpandAll_Click;
		lnkExpandAll.Command = c1Command;
		lnkCollaspeAll = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "全部收缩";
		c1Command2.Click += CmdCollaspeAll_Click;
		lnkCollaspeAll.Command = c1Command2;
	}

	public void PopulateVouchers()
	{
		IEnumerable<Voucher> source = Ledger.Vouchers.Where((Voucher v) => v.VoucherMark);
		source = source.OrderBy((Voucher v) => v.Number, StringNumberComparer.Instance).ThenBy((Voucher v) => v.Type.Name);
		PopulateVouchersImpl(grdVouchers, source);
		PopulateNavTreeImpl();
	}

	public IEnumerable<Voucher> GetVouchers()
	{
		IEnumerable<Voucher> source = Ledger.Vouchers.Where((Voucher v) => v.VoucherMark);
		return source.OrderBy((Voucher v) => v.Number, StringNumberComparer.Instance).ThenBy((Voucher v) => v.Type.Name);
	}

	public void SetTheme()
	{
		grdVouchers.Styles.Fixed.Border.Color = Color.DarkGray;
		grdDetail.Styles.Fixed.Border.Color = Color.DarkGray;
		Tree.Styles.Fixed.Border.Width = 0;
		Tree.Styles.Normal.Border.Width = 0;
		Tree.Styles.EmptyArea.BackColor = Color.Transparent;
		Tree.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		_brushHoverBackground.Color = Color.FromArgb(100, Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background"));
		if (Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
	}

	private void PnlMarkDetails_CloseButtonClick(object sender, EventArgs e)
	{
		pnlDetails.Visible = false;
	}

	private void GrdVouchers_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (grdVouchers.Cols[e.Col].Name == "MyMark" && e.Row >= grdVouchers.Rows.Fixed)
		{
			e.Text = string.Empty;
			if (!grdVouchers.Selection.ContainsRow(e.Row))
			{
				e.Image = null;
				e.DrawCell(DrawCellFlags.All);
				return;
			}
			Rectangle cancelMarkIconRect = GetCancelMarkIconRect(e.Row, e.Col);
			if (_isMouseOverCancelMyMarkIcon)
			{
				e.DrawCell(DrawCellFlags.Background);
				DrawIconBackground(cancelMarkIconRect);
				e.Graphics.DrawImage(Resources.cancelMark16, cancelMarkIconRect);
				e.DrawCell(DrawCellFlags.Border);
			}
			else
			{
				e.DrawCell(DrawCellFlags.Background);
				e.Graphics.DrawImage(Resources.cancelMark16, cancelMarkIconRect);
				e.DrawCell(DrawCellFlags.Border);
			}
		}
		else
		{
			e.DrawCell(DrawCellFlags.All);
		}
		void DrawIconBackground(Rectangle rt)
		{
			Rectangle rect = new Rectangle(rt.X - 2, rt.Y - 2, rt.Width + 4, rt.Height + 4);
			_adjustLevelIconOnFocusBackgroundBrush.Color = Leqisoft.UI.Controls.Util.DarkenColor(grdVouchers.Styles.SelectedColumnHeader.BackColor, 0.1);
			e.Graphics.FillRectangle(_adjustLevelIconOnFocusBackgroundBrush, rect);
		}
	}

	private void GrdVouchers_BodySelectionChanged(object sender, EventArgs e)
	{
		grdVouchers.Invalidate();
	}

	private void GrdVouchers_MouseMove(object sender, MouseEventArgs e)
	{
		bool flag = false;
		HitTestInfo hitTestInfo = grdVouchers.HitTest();
		if (hitTestInfo.Type == HitTestTypeEnum.Cell && grdVouchers.Selection.ContainsRow(hitTestInfo.Row))
		{
			int index = grdVouchers.Cols["MyMark"].Index;
			if (hitTestInfo.Column == index && GetCancelMarkIconRect(hitTestInfo.Row, hitTestInfo.Column).Contains(e.Location))
			{
				flag = true;
			}
		}
		if (flag != _isMouseOverCancelMyMarkIcon)
		{
			_isMouseOverCancelMyMarkIcon = flag;
			grdVouchers.Invalidate();
		}
	}

	private Rectangle GetCancelMarkIconRect(int row, int col)
	{
		Rectangle cellRect = grdVouchers.GetCellRect(row, col);
		int width = Resources.cancelMark16.Width;
		int height = Resources.cancelMark16.Height;
		int num = width / 2;
		int num2 = height / 2;
		int num3 = cellRect.X + cellRect.Width / 2;
		int num4 = cellRect.Y + cellRect.Height / 2;
		return new Rectangle(num3 - num, num4 - num2, width, height);
	}

	private void GrdVouchers_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			HitTestInfo hitTestInfo = grdVouchers.HitTest();
			if (hitTestInfo.Type == HitTestTypeEnum.Cell && grdVouchers.Cols[hitTestInfo.Column].Name == "MyMark" && grdVouchers.Selection.ContainsRow(hitTestInfo.Row) && GetCancelMarkIconRect(hitTestInfo.Row, hitTestInfo.Column).Contains(e.X, e.Y))
			{
				e.Cancel = true;
				MarkSelectedRangeCancelImpl();
			}
		}
	}

	private void _grdVouchers_DoubleClick(object sender, EventArgs e)
	{
		int row = grdVouchers.Row;
		if (row < grdVouchers.Rows.Fixed || row >= grdVouchers.Rows.Count)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row2 = grdVouchers.Rows[row];
		if (row2.UserData is Voucher voucher)
		{
			if (!pnlDetails.Visible)
			{
				pnlDetails.Visible = true;
			}
			PopulateDetails(voucher);
		}
	}

	private void _grdVouchers_Click(object sender, EventArgs e)
	{
		if (!pnlDetails.Visible)
		{
			return;
		}
		int row = grdVouchers.Row;
		if (row >= grdVouchers.Rows.Fixed && row < grdVouchers.Rows.Count)
		{
			C1.Win.C1FlexGrid.Row row2 = grdVouchers.Rows[row];
			if (row2.UserData is Voucher voucher)
			{
				PopulateDetails(voucher);
			}
		}
	}

	private void GrdVouchers_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			HitTestInfo hitTestInfo = grdVouchers.HitTest();
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				mnuVoucher.ShowContextMenu(grdVouchers, e.Location);
			}
			else if (hitTestInfo.Type == HitTestTypeEnum.ColumnHeader)
			{
				mnuVoucherColumn.ShowContextMenu(grdVouchers, e.Location);
			}
		}
	}

	private void GrdVouchers_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.C)
		{
			Common.SetSelectionToClipboard(grdVouchers);
		}
	}

	private void GrdDetail_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.C)
		{
			Common.SetSelectionToClipboard(grdDetail);
		}
	}

	private void GrdDetail_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && grdDetail.HitTest().Type == HitTestTypeEnum.Cell)
		{
			mnuDetail.ShowContextMenu(grdDetail, e.Location);
		}
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Common.SetSelectionToClipboard(grdVouchers);
	}

	private void CmdCopyDetail_Click(object sender, ClickEventArgs e)
	{
		Common.SetSelectionToClipboard(grdDetail);
	}

	private void CmdCancelSort_Click(object sender, ClickEventArgs e)
	{
		grdVouchers.Sort(SortFlags.None, grdVouchers.Cols.Fixed, grdVouchers.Cols.Count - 1);
		PopulateVouchers();
	}

	private void CmdCancelSort_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
	}

	private void CmdSortDesc_Click(object sender, ClickEventArgs e)
	{
		grdVouchers.Sort(SortFlags.Descending, grdVouchers.Col);
	}

	private void CmdSortDesc_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
	}

	private void CmdSort_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
	}

	private void CmdSort_Click(object sender, ClickEventArgs e)
	{
		grdVouchers.Sort(SortFlags.Ascending, grdVouchers.Col);
	}

	private void Initialize()
	{
		grdDetail.AllowEditing = false;
		grdVouchers.AllowEditing = false;
		grdVouchers.Click += _grdVouchers_Click;
		grdVouchers.DoubleClick += _grdVouchers_DoubleClick;
		grdVouchers.KeyDown += GrdVouchers_KeyDown;
		grdVouchers.MouseClick += GrdVouchers_MouseClick;
		grdVouchers.OwnerDrawCell += GrdVouchers_OwnerDrawCell;
		grdVouchers.BodySelectionChanged += GrdVouchers_BodySelectionChanged;
		grdVouchers.MouseMove += GrdVouchers_MouseMove;
		grdVouchers.BeforeMouseDown += GrdVouchers_BeforeMouseDown;
		pnlDetails.Visible = false;
		pnlDetails.CloseButtonClick += PnlMarkDetails_CloseButtonClick;
		grdDetail.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdDetail.DrawFormBorder(e1.Graphics);
		};
		grdDetail.KeyDown += GrdDetail_KeyDown;
		grdDetail.MouseClick += GrdDetail_MouseClick;
	}

	public void ShowSideToolbar()
	{
		pnlSidebar?.Show();
	}

	public void HideSideToolbar()
	{
		pnlSidebar?.Hide();
	}

	private void InitComponent()
	{
		View = new C1SplitContainer();
		pnlVouchers = new C1SplitterPanel();
		pnlDetails = new C1SplitterPanel();
		ctnVouchers = new C1SplitContainer();
		pnlVoucherTitle = new C1SplitterPanel();
		pnlVoucherGrid = new C1SplitterPanel();
		ctnDetails = new C1SplitContainer();
		pnlDetailHead = new C1SplitterPanel();
		pnlDetailGrid = new C1SplitterPanel();
		pnlDetailFoot = new C1SplitterPanel();
		lblVoucherTitle = new C1Label();
		grdVouchers = new C1FlexGridEx();
		grdVouchers.Name = "grdVouchers";
		lblDetailTitle = new C1Label();
		lblDetailType = new C1Label();
		lblDetailNumber = new C1Label();
		lblDetailDate = new C1Label();
		lblDetailAttachNum = new C1Label();
		grdDetail = new C1FlexGridEx();
		grdDetail.Name = "grdDetail";
		lblDetailMaker = new C1Label();
		lblDetailBooker = new C1Label();
		lblDetailChecker = new C1Label();
		Tree = GridFactory.Create("tree");
		Tree.Name = "Tree";
		Tree.Cols.Count = 1;
		Tree.Cols.Fixed = 0;
		Tree.Rows.Count = 0;
		Tree.Rows.Fixed = 0;
		Tree.Styles.EmptyArea.BackColor = Color.Transparent;
		Tree.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		Tree.DrawMode = DrawModeEnum.OwnerDraw;
		Tree.OwnerDrawCell += Tree_OwnerDrawCell;
		Tree.MouseMove += Tree_MouseMove;
		Tree.MouseLeave += Tree_MouseLeave;
		Tree.MouseClick += Tree_MouseClick;
		Tree.MouseDoubleClick += Tree_MouseDoubleClick;
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblVoucherTitle.TextDetached = true;
		lblVoucherTitle.BorderStyle = BorderStyle.None;
		lblVoucherTitle.Dock = DockStyle.Fill;
		lblVoucherTitle.Font = font;
		lblVoucherTitle.Text = "我的关注";
		lblVoucherTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlVoucherTitle.Height = 30;
		pnlVoucherTitle.KeepRelativeSize = false;
		pnlVoucherTitle.Location = new Point(0, 0);
		pnlVoucherTitle.MinHeight = 30;
		pnlVoucherTitle.Resizable = false;
		pnlVoucherTitle.Size = new Size(927, 30);
		pnlVoucherTitle.SizeRatio = 9.524;
		pnlVoucherTitle.TabIndex = 0;
		pnlVoucherTitle.Controls.Add(lblVoucherTitle);
		pnlVoucherTitle.Paint += delegate(object s1, PaintEventArgs e1)
		{
			e1.Graphics.DrawLine(panelBorderPen, 0, pnlVoucherTitle.Height - 1, pnlVoucherTitle.Width, pnlVoucherTitle.Height - 1);
		};
		grdVouchers.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdVouchers.Dock = DockStyle.Fill;
		grdVouchers.DrawMode = DrawModeEnum.OwnerDraw;
		grdVouchers.Rows.Count = 1;
		grdVouchers.Rows.DefaultSize = 20;
		grdVouchers.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		grdVouchers.AllowSorting = AllowSortingEnum.None;
		C1ToolBar c1ToolBar = new C1ToolBar();
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "取消关注";
		c1Command.Image = Resources.sidebarMarkCancel;
		c1Command.Click += CmdSideMarkCancel_Click;
		c1CommandLink.Command = c1Command;
		c1ToolBar.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "填充至底稿";
		c1Command2.Image = Resources.sideFillToTable;
		c1Command2.Click += delegate
		{
			FillToTable();
		};
		c1Command2.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
		{
			e1.Visible = LedgerViewer.IsAuditPlatform;
		};
		c1CommandLink2.Command = c1Command2;
		c1ToolBar.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		c1CommandLink3.Delimiter = true;
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "隐藏侧边栏";
		c1Command3.Image = Resources.sideHideSidebar;
		c1Command3.Click += delegate
		{
			_owner.OnHideSidebarClick();
		};
		c1CommandLink3.Command = c1Command3;
		C1SplitContainer value = ComponentFactory.BuildSidebar(grdVouchers, c1ToolBar, out pnlSidebar);
		pnlVoucherGrid.Height = 284;
		pnlVoucherGrid.Location = new Point(0, 31);
		pnlVoucherGrid.Size = new Size(927, 284);
		pnlVoucherGrid.Controls.Add(value);
		foreach (C1CommandLink commandLink in c1ToolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		ctnVouchers.AutoSizeElement = AutoSizeElement.Both;
		ctnVouchers.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnVouchers.Dock = DockStyle.Fill;
		ctnVouchers.Location = new Point(0, 0);
		ctnVouchers.Size = new Size(927, 315);
		ctnVouchers.SplitterWidth = 0;
		ctnVouchers.TabIndex = 0;
		ctnVouchers.Panels.Add(pnlVoucherTitle);
		ctnVouchers.Panels.Add(pnlVoucherGrid);
		pnlVouchers.Height = 315;
		pnlVouchers.Location = new Point(0, 0);
		pnlVouchers.Size = new Size(927, 315);
		pnlVouchers.TabIndex = 0;
		pnlVouchers.Controls.Add(ctnVouchers);
		Font font2 = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblDetailTitle.TextDetached = true;
		lblDetailTitle.Anchor = AnchorStyles.Top;
		lblDetailTitle.AutoSize = true;
		lblDetailTitle.BorderStyle = BorderStyle.None;
		lblDetailTitle.Font = font2;
		lblDetailTitle.Location = new Point(426, 4);
		lblDetailTitle.Size = new Size(74, 21);
		lblDetailTitle.Text = "记账凭证";
		lblDetailTitle.TextAlign = ContentAlignment.MiddleCenter;
		lblDetailType.TextDetached = true;
		lblDetailType.AutoSize = true;
		lblDetailType.BorderStyle = BorderStyle.None;
		lblDetailType.Location = new Point(12, 29);
		lblDetailType.Size = new Size(32, 17);
		lblDetailType.Text = "字：";
		lblDetailType.TextAlign = ContentAlignment.MiddleLeft;
		lblDetailNumber.TextDetached = true;
		lblDetailNumber.AutoSize = true;
		lblDetailNumber.BorderStyle = BorderStyle.None;
		lblDetailNumber.Location = new Point(91, 29);
		lblDetailNumber.Margin = new Padding(10, 0, 3, 0);
		lblDetailNumber.Size = new Size(32, 17);
		lblDetailNumber.Text = "号：";
		lblDetailNumber.TextAlign = ContentAlignment.MiddleLeft;
		lblDetailDate.TextDetached = true;
		lblDetailDate.Anchor = AnchorStyles.Top;
		lblDetailDate.BorderStyle = BorderStyle.None;
		lblDetailDate.Location = new Point(395, 29);
		lblDetailDate.Size = new Size(150, 17);
		lblDetailDate.Text = "制单日期：0000-00-00";
		lblDetailDate.TextAlign = ContentAlignment.MiddleCenter;
		lblDetailAttachNum.TextDetached = true;
		lblDetailAttachNum.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		lblDetailAttachNum.BorderStyle = BorderStyle.None;
		lblDetailAttachNum.ImageAlign = ContentAlignment.MiddleRight;
		lblDetailAttachNum.Location = new Point(739, 29);
		lblDetailAttachNum.Size = new Size(180, 17);
		lblDetailAttachNum.Text = "附件张数：";
		lblDetailAttachNum.TextAlign = ContentAlignment.MiddleRight;
		pnlDetailHead.Height = 50;
		pnlDetailHead.KeepRelativeSize = false;
		pnlDetailHead.Location = new Point(0, 0);
		pnlDetailHead.MinHeight = 30;
		pnlDetailHead.Resizable = false;
		pnlDetailHead.Size = new Size(927, 50);
		pnlDetailHead.SizeRatio = 19.011;
		pnlDetailHead.Controls.Add(lblDetailTitle);
		pnlDetailHead.Controls.Add(lblDetailType);
		pnlDetailHead.Controls.Add(lblDetailNumber);
		pnlDetailHead.Controls.Add(lblDetailDate);
		pnlDetailHead.Controls.Add(lblDetailAttachNum);
		grdDetail.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdDetail.Dock = DockStyle.Fill;
		grdDetail.DrawMode = DrawModeEnum.OwnerDraw;
		grdDetail.Rows.Count = 1;
		grdDetail.Rows.DefaultSize = 20;
		grdDetail.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		pnlDetailGrid.Height = 212;
		pnlDetailGrid.Location = new Point(0, 51);
		pnlDetailGrid.Size = new Size(927, 212);
		pnlDetailGrid.Controls.Add(grdDetail);
		lblDetailMaker.TextDetached = true;
		lblDetailMaker.Anchor = AnchorStyles.Left;
		lblDetailMaker.BorderStyle = BorderStyle.None;
		lblDetailMaker.Location = new Point(12, 2);
		lblDetailMaker.Size = new Size(150, 25);
		lblDetailMaker.Text = "制单人：";
		lblDetailMaker.TextAlign = ContentAlignment.MiddleLeft;
		lblDetailBooker.TextDetached = true;
		lblDetailBooker.Anchor = AnchorStyles.Top;
		lblDetailBooker.BorderStyle = BorderStyle.None;
		lblDetailBooker.Location = new Point(395, 2);
		lblDetailBooker.Size = new Size(150, 25);
		lblDetailBooker.Text = "记账人：";
		lblDetailBooker.TextAlign = ContentAlignment.MiddleCenter;
		lblDetailChecker.TextDetached = true;
		lblDetailChecker.Anchor = AnchorStyles.Right;
		lblDetailChecker.BorderStyle = BorderStyle.None;
		lblDetailChecker.ImageAlign = ContentAlignment.MiddleRight;
		lblDetailChecker.Location = new Point(739, 2);
		lblDetailChecker.Size = new Size(180, 25);
		lblDetailChecker.Text = "审核人：";
		lblDetailChecker.TextAlign = ContentAlignment.MiddleRight;
		pnlDetailFoot.Dock = PanelDockStyle.Bottom;
		pnlDetailFoot.Height = 30;
		pnlDetailFoot.KeepRelativeSize = false;
		pnlDetailFoot.Location = new Point(0, 264);
		pnlDetailFoot.MinHeight = 30;
		pnlDetailFoot.Resizable = false;
		pnlDetailFoot.Size = new Size(927, 30);
		pnlDetailFoot.SizeRatio = 10.239;
		pnlDetailFoot.Controls.Add(lblDetailMaker);
		pnlDetailFoot.Controls.Add(lblDetailBooker);
		pnlDetailFoot.Controls.Add(lblDetailChecker);
		ctnDetails.AutoSizeElement = AutoSizeElement.Both;
		ctnDetails.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnDetails.Dock = DockStyle.Fill;
		ctnDetails.SplitterWidth = 0;
		ctnDetails.Panels.Add(pnlDetailFoot);
		ctnDetails.Panels.Add(pnlDetailHead);
		ctnDetails.Panels.Add(pnlDetailGrid);
		pnlDetails.Height = 315;
		pnlDetails.Location = new Point(0, 336);
		pnlDetails.ShowCloseButton = true;
		pnlDetails.Size = new Size(927, 294);
		pnlDetails.Text = " ";
		pnlDetails.Controls.Add(ctnDetails);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.BackColor = Color.FromArgb(240, 240, 240);
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.ForeColor = Color.FromArgb(0, 0, 0);
		View.SplitterWidth = 0;
		View.Panels.Add(pnlVouchers);
		View.Panels.Add(pnlDetails);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(View);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(Tree);
		_csCenter = grdDetail.Styles.Add("center");
		_csCenter.TextAlign = TextAlignEnum.CenterCenter;
	}

	private void CmdCollaspeAll_Click(object sender, ClickEventArgs e)
	{
		Tree.Tree.Show(0);
	}

	private void CmdExpandAll_Click(object sender, ClickEventArgs e)
	{
		Tree.Tree.Show(Tree.Tree.MaximumLevel);
	}

	private void CtxTree_Popup(object sender, EventArgs e)
	{
		try
		{
			C1ContextMenu c1ContextMenu = sender as C1ContextMenu;
			c1ContextMenu.CommandLinks.Clear();
			c1ContextMenu.CommandLinks.Add(lnkExpandAll);
			c1ContextMenu.CommandLinks.Add(lnkCollaspeAll);
			int mouseRow = Tree.MouseRow;
			if (Tree.Row != mouseRow)
			{
				return;
			}
			C1.Win.C1FlexGrid.Row row = Tree.Rows[mouseRow];
			VoucherAccountTreeNodeData voucherAccountTreeNodeData = row.UserData as VoucherAccountTreeNodeData;
			if (voucherAccountTreeNodeData == null)
			{
				return;
			}
			C1.Win.C1FlexGrid.Row row2 = row;
			if (voucherAccountTreeNodeData.nodeType == NodeType.AuxiliaryItem)
			{
				Node parent = row.Node.Parent;
				if (parent == null)
				{
					return;
				}
				voucherAccountTreeNodeData = parent.Row.UserData as VoucherAccountTreeNodeData;
				if (voucherAccountTreeNodeData == null)
				{
					return;
				}
				row2 = parent.Row;
			}
			List<AuxiliaryClass> allAuxiliaryClass = voucherAccountTreeNodeData.GetAllAuxiliaryClass();
			if (allAuxiliaryClass == null || allAuxiliaryClass.Count == 0)
			{
				return;
			}
			Node[] nodes = row2.Node.Nodes;
			AuxiliaryClass auxiliaryClass = null;
			if (nodes.Length != 0 && nodes[0].Row.UserData is VoucherAccountTreeNodeData { nodeType: NodeType.AuxiliaryItem } voucherAccountTreeNodeData2)
			{
				auxiliaryClass = voucherAccountTreeNodeData2.auxiliaryItem.Class;
			}
			List<AuxiliaryClass> list = new List<AuxiliaryClass>();
			foreach (AuxiliaryClass item in allAuxiliaryClass)
			{
				if (item != auxiliaryClass)
				{
					list.Add(item);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				AuxiliaryClass auxiliaryClass2 = list[i];
				C1Command c1Command = new C1Command();
				c1Command.Text = "展开" + auxiliaryClass2.Name + "核算";
				c1Command.UserData = Tuple.Create(row2, auxiliaryClass2);
				c1Command.Click += SwitchTreeAuxClass;
				c1ContextMenu.CommandLinks.Add(new C1CommandLink
				{
					Delimiter = (i == 0),
					Command = c1Command
				});
			}
		}
		catch
		{
		}
	}

	private void SwitchTreeAuxClass(object sender, ClickEventArgs e)
	{
		if (!(sender is C1Command { UserData: Tuple<C1.Win.C1FlexGrid.Row, AuxiliaryClass> userData }))
		{
			return;
		}
		Tree.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Row item = userData.Item1;
			AuxiliaryClass item2 = userData.Item2;
			Node[] nodes = item.Node.Nodes;
			for (int num = nodes.Length - 1; num >= 0; num--)
			{
				nodes[num].RemoveNode();
			}
			if (item.UserData is VoucherAccountTreeNodeData voucherAccountTreeNodeData)
			{
				voucherAccountTreeNodeData.inShowingAuxClass = item2;
				List<AuxiliaryItem> auxiliaryItemByAuxiliaryClass = voucherAccountTreeNodeData.GetAuxiliaryItemByAuxiliaryClass(item2);
				if (auxiliaryItemByAuxiliaryClass != null)
				{
					foreach (AuxiliaryItem item3 in auxiliaryItemByAuxiliaryClass)
					{
						VoucherAccountTreeNodeData voucherAccountTreeNodeData2 = new VoucherAccountTreeNodeData(item3)
						{
							account = voucherAccountTreeNodeData.account,
							inShowingAuxClass = item2
						};
						string data = voucherAccountTreeNodeData2.GetTreeDisplayCode() + " " + item3.Name;
						Node node = item.Node.AddNode(NodeTypeEnum.LastChild, data);
						node.Row.UserData = voucherAccountTreeNodeData2;
						node.Image = GetNodeImage(isAccountNode: false, isOpenedState: false);
					}
				}
			}
			item.Node.Expanded = true;
		}
		finally
		{
			Tree.EndUpdate();
		}
	}

	private System.Drawing.Image GetTreeRowImage(C1.Win.C1FlexGrid.Row row)
	{
		bool isAccountNode = false;
		bool isOpenedState = _currentInFilterRow == row;
		if (row.UserData is VoucherAccountTreeNodeData { nodeType: NodeType.Account })
		{
			isAccountNode = true;
		}
		return GetNodeImage(isAccountNode, isOpenedState);
	}

	private void Tree_MouseClick(object sender, MouseEventArgs e)
	{
		Tree_MouseSingleClick(sender, e);
	}

	private void Tree_MouseSingleClick(object sender, MouseEventArgs e)
	{
		HitTestInfo hitTestInfo = Tree.HitTest(e.Location);
		if (e.Button == MouseButtons.Left)
		{
			if (hitTestInfo.Type == HitTestTypeEnum.Cell)
			{
				C1.Win.C1FlexGrid.Row row = Tree.Rows[hitTestInfo.Row];
				Node node = row.Node;
				node.Collapsed = !node.Collapsed;
				node.Image = GetTreeRowImage(node.Row);
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			ctxTree.ShowContextMenu(Tree, e.Location);
		}
	}

	private void Tree_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		int mouseRow = Tree.MouseRow;
		if (Tree.Row != mouseRow)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row = Tree.Rows[mouseRow];
		grdVouchers.BeginUpdate();
		try
		{
			if (!(row.UserData is VoucherAccountTreeNodeData voucherAccountTreeNodeData))
			{
				return;
			}
			string treeDisplayCode = voucherAccountTreeNodeData.GetTreeDisplayCode();
			if (string.IsNullOrWhiteSpace(treeDisplayCode))
			{
				return;
			}
			grdVouchers.FilterManager.Clear();
			grdVouchers.FilterManager.Filters.Add(new SelectFilter
			{
				Kind = FilterKind.Select,
				Values = FindOutShuldVisibleRows(treeDisplayCode, "Code"),
				ColumnId = "Code",
				Relation = FilterRelation.And
			});
			grdVouchers.FilterManager.Execute();
			_currentInFilterRow = row;
			_currentInFilterAccount = voucherAccountTreeNodeData.account;
			if (voucherAccountTreeNodeData.nodeType == NodeType.AuxiliaryItem)
			{
				_currentInFilterAuxiliary = voucherAccountTreeNodeData.auxiliaryItem;
			}
			else
			{
				_currentInFilterAuxiliary = null;
			}
			Tree.BeginUpdate();
			try
			{
				if (voucherAccountTreeNodeData.nodeType != NodeType.AuxiliaryItem)
				{
					row.Node.Expanded = true;
				}
				Tree.Invalidate();
			}
			finally
			{
				Tree.EndUpdate();
			}
		}
		finally
		{
			grdVouchers.EndUpdate();
		}
		HashSet<string> FindOutShuldVisibleRows(string startWithExp, string colName)
		{
			HashSet<string> hashSet = new HashSet<string>();
			int index = grdVouchers.Cols[colName].Index;
			int count = grdVouchers.Rows.Count;
			for (int i = grdVouchers.Rows.Fixed; i < count; i++)
			{
				string dataDisplay = grdVouchers.GetDataDisplay(i, index);
				if (!string.IsNullOrWhiteSpace(dataDisplay))
				{
					string[] array = dataDisplay.Split('|');
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j].StartsWith(startWithExp))
						{
							hashSet.Add(dataDisplay);
							break;
						}
					}
				}
			}
			return hashSet;
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
		if (e.Col == 0)
		{
			e.Image = GetTreeRowImage(Tree.Rows[e.Row]);
		}
	}

	private void CmdSideMarkCancel_Click(object sender, ClickEventArgs e)
	{
		int row = grdVouchers.Row;
		if (row >= grdVouchers.Rows.Fixed)
		{
			MarkSelectedRangeCancelImpl();
		}
	}

	private void MarkSelectedRangeCancelImpl()
	{
		if (grdVouchers.Selection.BottomRow < grdVouchers.Rows.Fixed)
		{
			return;
		}
		grdVouchers.BeginUpdate();
		try
		{
			for (int i = grdVouchers.Selection.TopRow; i <= grdVouchers.Selection.BottomRow; i++)
			{
				if (grdVouchers.Rows[i].UserData is Voucher { VoucherMark: not false } voucher)
				{
					voucher.ToggleMark();
				}
			}
			Ledger.Save();
			grdVouchers.Rows.RemoveRange(grdVouchers.Selection.TopRow, grdVouchers.Selection.BottomRow - grdVouchers.Selection.TopRow + 1);
			int count = grdVouchers.Rows.Count;
			int index = grdVouchers.Cols["Index"].Index;
			int num = grdVouchers.Rows.Fixed;
			int num2 = 1;
			while (num < count)
			{
				grdVouchers.SetData(num, index, num2.ToString());
				num++;
				num2++;
			}
			grdVouchers.Select(-1, -1);
			RefreshNavTreeImpl();
		}
		finally
		{
			grdVouchers.EndUpdate();
		}
	}

	public void FillToTable()
	{
		List<object> list = new List<object>();
		int count = grdVouchers.Rows.Count;
		for (int i = grdVouchers.Rows.Fixed; i < count; i++)
		{
			C1.Win.C1FlexGrid.Row row = grdVouchers.Rows[i];
			if (row.UserData is Voucher item && row.Visible)
			{
				list.Add(item);
			}
		}
		_owner._owner.OnAfterCollect(new LedgerCollectEventArgs
		{
			Viewer = _owner,
			Account = ((grdVouchers.FilterManager.Filters.Count == 0) ? null : _currentInFilterAccount),
			Auxiliary = ((grdVouchers.FilterManager.Filters.Count == 0) ? null : _currentInFilterAuxiliary),
			CollectObject = CollectObjectEnum.Subsidiary,
			StartTime = _owner.StartDate,
			EndTime = _owner.EndDate,
			Source = list,
			IsSourceComeFromMyMark = true,
			IsShowSubsidiaryAllAccountTypeTable = true
		});
	}

	private void PopulateDetails(Voucher voucher)
	{
		IEnumerable<Voucher> vouchers = Ledger.Vouchers.Where((Voucher t) => t.Type == voucher.Type && t.Number == voucher.Number && t.Day.Year == voucher.Day.Year && t.Day.Month == voucher.Day.Month);
		PopulateDetailsImpl(grdDetail, vouchers);
		lblDetailType.Text = "字：" + voucher.Type.Name;
		lblDetailNumber.Text = "号：" + voucher.Number;
		lblDetailDate.Text = "制单日期：" + voucher.Day.ToString("yyyy-MM-dd");
		lblDetailAttachNum.Text = $"附件张数：{voucher.NumAttachments}";
		lblDetailMaker.Text = "制单人：" + voucher.Maker;
		lblDetailBooker.Text = "记账人：" + voucher.Booker;
		lblDetailChecker.Text = "审核人：" + voucher.Checker;
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

	private void PopulateNavTreeImpl()
	{
		C1FlexGridEx grid = Tree;
		grid.BeginUpdate();
		try
		{
			Account searchAccount2 = null;
			bool isAccountExist2;
			VoucherAccountTreeNodeData rootNode2 = BuildAccountTree(searchAccount2, out isAccountExist2);
			AddRoot(rootNode2);
		}
		finally
		{
			grid.EndUpdate();
		}
		void AddChildNode_Account(VoucherAccountTreeNodeData parentAccountData, Node parentNode)
		{
			if (parentAccountData.children == null || parentAccountData.children.Count == 0)
			{
				return;
			}
			foreach (VoucherAccountTreeNodeData child in parentAccountData.children)
			{
				if (child.nodeType == NodeType.Account)
				{
					string text2 = child.account.Code + " " + child.account.Name;
					Node node2 = ((parentNode == null) ? grid.Rows.AddNode(0) : parentNode.AddNode(NodeTypeEnum.LastChild, text2));
					node2.Row[0] = text2;
					node2.Row.UserData = child;
					node2.Image = GetNodeImage(isAccountNode: true, isOpenedState: false);
					AddChildNode_Account(child, node2);
					AddChildNode_AuxItem(child, node2);
				}
			}
		}
		void AddChildNode_AuxItem(VoucherAccountTreeNodeData accountData, Node accountNode)
		{
			if (accountData.children != null && accountData.children.Count != 0)
			{
				List<AuxiliaryClass> allAuxiliaryClass = accountData.GetAllAuxiliaryClass();
				if (allAuxiliaryClass != null && allAuxiliaryClass.Count != 0)
				{
					accountData.inShowingAuxClass = allAuxiliaryClass[0];
					List<AuxiliaryItem> auxiliaryItemByAuxiliaryClass = accountData.GetAuxiliaryItemByAuxiliaryClass(accountData.inShowingAuxClass);
					if (auxiliaryItemByAuxiliaryClass != null && auxiliaryItemByAuxiliaryClass.Count != 0)
					{
						foreach (AuxiliaryItem item in auxiliaryItemByAuxiliaryClass)
						{
							string text = accountData.account.Code + "-" + item.Code + " " + item.Name;
							Node node = accountNode.AddNode(NodeTypeEnum.LastChild, text);
							node.Row[0] = text;
							node.Row.UserData = new VoucherAccountTreeNodeData(item)
							{
								account = accountData.account,
								inShowingAuxClass = accountData.inShowingAuxClass
							};
							node.Image = GetNodeImage(isAccountNode: false, isOpenedState: false);
						}
					}
				}
			}
		}
		void AddRoot(VoucherAccountTreeNodeData rootNode)
		{
			grid.Rows.Count = 0;
			grid.Tree.Column = 0;
			if (rootNode.children != null && rootNode.children.Count != 0)
			{
				AddChildNode_Account(rootNode, null);
				grid.CollapseAll();
			}
		}
		VoucherAccountTreeNodeData BuildAccountTree(Account searchAccount, out bool isAccountExist)
		{
			isAccountExist = false;
			VoucherAccountTreeNodeData voucherAccountTreeNodeData = new VoucherAccountTreeNodeData
			{
				children = new List<VoucherAccountTreeNodeData>()
			};
			Dictionary<Account, VoucherAccountTreeNodeData> dictionary = new Dictionary<Account, VoucherAccountTreeNodeData>();
			foreach (Voucher voucher in Ledger.Vouchers)
			{
				if (voucher.VoucherMark)
				{
					VoucherAccountTreeNodeData voucherAccountTreeNodeData2 = new VoucherAccountTreeNodeData(voucher);
					for (Account account = voucher.Account; account != null; account = account.Parent)
					{
						if (dictionary.TryGetValue(account, out var value))
						{
							if (value.children == null)
							{
								value.children = new List<VoucherAccountTreeNodeData>();
							}
							value.children.Add(voucherAccountTreeNodeData2);
							voucherAccountTreeNodeData2 = null;
							break;
						}
						value = new VoucherAccountTreeNodeData(account)
						{
							children = new List<VoucherAccountTreeNodeData> { voucherAccountTreeNodeData2 }
						};
						dictionary.Add(account, value);
						voucherAccountTreeNodeData2 = value;
					}
					if (voucherAccountTreeNodeData2 != null)
					{
						voucherAccountTreeNodeData.children.Add(voucherAccountTreeNodeData2);
						if (voucherAccountTreeNodeData2.account != null && !dictionary.ContainsKey(voucherAccountTreeNodeData2.account))
						{
							dictionary.Add(voucherAccountTreeNodeData2.account, voucherAccountTreeNodeData2);
						}
					}
				}
			}
			if (searchAccount != null)
			{
				isAccountExist = dictionary.ContainsKey(searchAccount);
			}
			return voucherAccountTreeNodeData;
		}
	}

	private void RefreshNavTreeImpl()
	{
		C1FlexGridEx tree = Tree;
		tree.BeginUpdate();
		try
		{
			for (int num = tree.Rows.Count - 1; num >= tree.Rows.Fixed; num--)
			{
				C1.Win.C1FlexGrid.Row row = tree.Rows[num];
				VoucherAccountTreeNodeData voucherAccountTreeNodeData = row.UserData as VoucherAccountTreeNodeData;
				if (voucherAccountTreeNodeData.nodeType == NodeType.AuxiliaryItem)
				{
					if (row.Node.Parent != null && row.Node.Parent.Row.UserData is VoucherAccountTreeNodeData { children: not null } voucherAccountTreeNodeData2 && voucherAccountTreeNodeData2.children.Count != 0)
					{
						bool flag = false;
						foreach (VoucherAccountTreeNodeData child in voucherAccountTreeNodeData2.children)
						{
							if (child.nodeType != NodeType.Voucher || !child.voucher.VoucherMark)
							{
								continue;
							}
							if (child.voucher.Details != null)
							{
								foreach (AuxiliaryItem detail in child.voucher.Details)
								{
									if (detail == voucherAccountTreeNodeData.auxiliaryItem)
									{
										flag = true;
										break;
									}
								}
							}
							if (flag)
							{
								break;
							}
						}
						if (!flag)
						{
							row.Node.RemoveNode();
						}
					}
				}
				else if (voucherAccountTreeNodeData.nodeType == NodeType.Account && row.Node.Nodes.Length == 0)
				{
					if (voucherAccountTreeNodeData.children == null || voucherAccountTreeNodeData.children.Count == 0)
					{
						row.Node.RemoveNode();
					}
					else
					{
						bool flag2 = false;
						foreach (VoucherAccountTreeNodeData child2 in voucherAccountTreeNodeData.children)
						{
							if (child2.nodeType == NodeType.Voucher && child2.voucher.VoucherMark)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							row.Node.RemoveNode();
						}
					}
				}
			}
		}
		finally
		{
			tree.EndUpdate();
		}
	}

	private void PopulateVouchersImpl(C1FlexGrid grid, IEnumerable<Voucher> vouchers)
	{
		grid.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			grid.Cols.Count = 0;
			grid.Rows.Count = 1;
			grid.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grid.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowMerging = true;
			column.Width = 50;
			column = grid.Cols.Add();
			column.Name = "MyMark";
			column.Caption = "取消关注";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 65;
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.ImageAlign = ImageAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "Date";
			column.Caption = "日期";
			column.DataType = typeof(DateTime);
			column.Format = "yyyy-MM-dd";
			column.AllowMerging = true;
			column.Width = 80;
			column = grid.Cols.Add();
			column.Name = "Type";
			column.Caption = "字";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 30;
			column = grid.Cols.Add();
			column.Name = "Number";
			column.Caption = "号";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 50;
			column = grid.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 200;
			column = grid.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 100;
			column = grid.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 120;
			column = grid.Cols.Add();
			column.Name = "Opposite";
			column.Caption = "对方科目";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 120;
			column = grid.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 80;
			column = grid.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 80;
			column = grid.Cols.Add();
			column.Name = "Maker";
			column.Caption = "制单人";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 60;
			column = grid.Cols.Add();
			column.Name = "Checker";
			column.Caption = "审核人";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 60;
			column = grid.Cols.Add();
			column.Name = "Booker";
			column.Caption = "记账人";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 60;
			grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grid.Rows.Fixed = 1;
			grid.Cols.Fixed = 1;
			int num = 1;
			foreach (Voucher voucher in vouchers)
			{
				C1.Win.C1FlexGrid.Row row = grid.Rows.Add();
				row.UserData = voucher;
				row["Index"] = num++;
				row["Date"] = voucher.Day;
				row["Type"] = voucher.Type.Name;
				row["Number"] = voucher.Number;
				row["Digest"] = voucher.Digest;
				row["Code"] = voucher.GetDisplayAccountCodeWithDetail();
				row["Name"] = voucher.GetDisplayAccountNameWithDetail();
				row["Opposite"] = string.Join(",", voucher.OppositeAccounts.Select((Account o) => o.Name).Distinct());
				row["Debit"] = (voucher.IsDebit ? voucher.Amount : 0m);
				row["Credit"] = (voucher.IsDebit ? 0m : voucher.Amount);
				row["Maker"] = voucher.Maker;
				row["Checker"] = voucher.Checker;
				row["Booker"] = voucher.Booker;
			}
			_owner.StyleRecord.ResumeStyle(grid);
		}
		catch
		{
		}
		finally
		{
			PendingAllEvent = false;
			grid.AllowEditing = false;
			grid.EndUpdate();
		}
	}

	private void PopulateDetailsImpl(C1FlexGrid grid, IEnumerable<Voucher> vouchers)
	{
		grid.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			grid.Cols.Count = 0;
			grid.Rows.Count = 1;
			grid.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grid.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowMerging = true;
			column = grid.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column = grid.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column = grid.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column = grid.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column = grid.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grid.Rows.Fixed = 1;
			grid.Cols.Fixed = 1;
			int num = 1;
			decimal num2 = default(decimal);
			decimal num3 = default(decimal);
			foreach (Voucher voucher in vouchers)
			{
				C1.Win.C1FlexGrid.Row row = grid.Rows.Add();
				row.UserData = voucher;
				row["Index"] = num++;
				row["Digest"] = voucher.Digest;
				row["Code"] = voucher.GetDisplayAccountCodeWithDetail();
				row["Name"] = voucher.GetDisplayAccountNameWithDetail();
				row["Debit"] = (voucher.IsDebit ? voucher.Amount : 0m);
				row["Credit"] = (voucher.IsDebit ? 0m : voucher.Amount);
				num2 += (voucher.IsDebit ? voucher.Amount : 0m);
				num3 += (voucher.IsDebit ? 0m : voucher.Amount);
			}
			C1.Win.C1FlexGrid.Row row2 = grid.Rows.Add();
			row2["Digest"] = "合计";
			row2["Debit"] = num2;
			row2["Credit"] = num3;
			row2.StyleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
			grid.SetCellStyle(row2.Index, "Digest", _csCenter);
			_owner.StyleRecord.ResumeStyle(grid);
		}
		catch
		{
		}
		finally
		{
			PendingAllEvent = false;
			grid.AllowEditing = false;
			grid.EndUpdate();
		}
	}
}
