using System;
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
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Controls.Properties;
using Auditai.UI.LedgerView.Properties;

namespace Auditai.UI.LedgerView;

internal class SubsidiaryEditor : ISetTheme
{
	private const string DATETIMEFORMATE = "yyyy-MM-dd";

	private const string ROW_BEGINBALANCE = "BeginBalance";

	private const string MONTHTOTAL = "MonthTotal";

	private const string YEARTOTAL = "YearTotal";

	private LedgerViewer _owner;

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkCopy = new C1CommandLink();

	private C1Command cmdTotal_AllTotal = new C1Command();

	private C1CommandLink lnkTotal_AllTotal = new C1CommandLink();

	private C1Command cmdTotal_MonthOnly = new C1Command();

	private C1CommandLink lnkTotal_MonthOnly = new C1CommandLink();

	private C1Command cmdTotal_YearOnly = new C1Command();

	private C1CommandLink lnkTotal_YearOnly = new C1CommandLink();

	private C1Command cmdSub_AllTotal = new C1Command();

	private C1CommandLink lnkSub_AllTotal = new C1CommandLink();

	private C1Command cmdSub_MonthOnly = new C1Command();

	private C1CommandLink lnkSub_MonthOnly = new C1CommandLink();

	private C1Command cmdSub_YearOnly = new C1Command();

	private C1CommandLink lnkSub_YearOnly = new C1CommandLink();

	private C1Command cmdSubNoTotal = new C1Command();

	private C1CommandLink lnkSub_NoTotal = new C1CommandLink();

	private C1Command cmdDirectionChange = new C1Command();

	private C1CommandLink lnkDirectionChange = new C1CommandLink();

	private C1Command cmdDirectionReduce = new C1Command();

	private C1CommandLink lnkDirectionReduce = new C1CommandLink();

	private C1Command cmdMakeMark = new C1Command();

	private C1CommandLink lnkMakeMark = new C1CommandLink();

	private C1Command cmdCancelMark = new C1Command();

	private C1CommandLink lnkCancelMark = new C1CommandLink();

	private C1Command cmdColHide = new C1Command();

	private C1CommandLink lnkColHide = new C1CommandLink();

	private C1Command cmdCancelHide = new C1Command();

	private C1CommandLink lnkCancelHide = new C1CommandLink();

	private C1CommandLink lnkAsc = new C1CommandLink();

	private C1Command cmdAsc = new C1Command();

	private C1CommandLink lnkDes = new C1CommandLink();

	private C1Command cmdDes = new C1Command();

	private C1CommandLink lnkNormal = new C1CommandLink();

	private C1Command cmdNormal = new C1Command();

	private C1Command cmdCopy2 = new C1Command();

	private C1CommandLink lnkCopy2 = new C1CommandLink();

	private C1Command cmdDirectionChange2 = new C1Command();

	private C1CommandLink lnkDirectionChange2 = new C1CommandLink();

	private C1Command cmdDirectionReduce2 = new C1Command();

	private C1CommandLink lnkDirectionReduce2 = new C1CommandLink();

	private C1Command cmdMakeMark2 = new C1Command();

	private C1CommandLink lnkMakeMark2 = new C1CommandLink();

	private C1Command cmdCancelMark2 = new C1Command();

	private C1CommandLink lnkCancelMark2 = new C1CommandLink();

	private C1Command cmdColHide2 = new C1Command();

	private C1CommandLink lnkColHide2 = new C1CommandLink();

	private C1Command cmdCancelHide2 = new C1Command();

	private C1CommandLink lnkCancelHide2 = new C1CommandLink();

	private C1Command cmdModifyBalance = new C1Command();

	private C1CommandLink lnkModifyBalance = new C1CommandLink();

	private C1Command cmdFillToTable = new C1Command();

	private C1CommandLink lnkFillToTable = new C1CommandLink();

	public object currentUserData;

	private C1SplitterPanel pnlSubsidiaryTitle;

	private C1SplitterPanel pnlSubsidiaryHead;

	private C1SplitterPanel pnlSubsidiaryGrid;

	private C1SplitterPanel pnlSubsidiaryVoucher;

	private C1SplitterPanel pnlSubsidiayFoot;

	private C1SplitContainer ctnVoucher;

	private C1SplitterPanel pnlVoucherTitle;

	private C1SplitterPanel pnlVoucherHead;

	private C1SplitterPanel pnlVoucherGrid;

	private C1SplitterPanel pnlVoucherFoot;

	public C1Button btnSubsidiaryBack;

	public C1Label lblSubsidiaryTitle;

	private C1Label lblAccountName;

	private C1Label lblSubStartDate;

	private C1Label lblPeriod;

	private C1Label lblSubEndDate;

	private C1Label lblSubCurrency;

	public C1FlexGridEx grdSubsidiary;

	private C1Label lblVoucherTitle;

	private C1Label lblVoucherType;

	private C1Label lblVoucherNumber;

	private C1Label lblVoucherDate;

	private C1Label lblNumAttachments;

	public C1FlexGridEx grdVoucher;

	private C1Label lblChecker;

	private C1Label lblMaker;

	private C1Label lblBooker;

	private C1DockingTab SubDockingTab;

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1ToolBar toolBar = new C1ToolBar();

	private C1SplitterPanel pnlSidebar;

	private C1Button btnCloseVoucher = new C1Button();

	private C1CommandLink lnkSidebarDirectionChange = new C1CommandLink();

	private C1Command cmdSidebarDirectionChange = new C1Command();

	private C1CommandLink lnkSidebarMarkVoucher = new C1CommandLink();

	private C1Command cmdSidebarMarkVoucher = new C1Command();

	private C1CommandLink lnkSidebarModifyBegin = new C1CommandLink();

	private C1Command cmdSidebarModifyBegin = new C1Command();

	private C1CommandLink lnkSidebarModifyVoucher = new C1CommandLink();

	private C1Command cmdSidebarModifyVoucher = new C1Command();

	private C1ContextMenu ctxSidebarTotalSummary = new C1ContextMenu();

	private C1Command cmdFillToTableOnToolbar;

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	private bool addVisit = true;

	private bool initializedSubsidiaryCaption;

	private bool initializedVoucherCaption;

	public bool _voucherVisible;

	private C1ContextMenu ctxSubFixed = new C1ContextMenu();

	private C1ContextMenu ctxSubEmpty = new C1ContextMenu();

	private C1ContextMenu ctxSubCell = new C1ContextMenu();

	private C1ContextMenu ctxVouCell = new C1ContextMenu();

	private C1ContextMenu ctxVouFixed = new C1ContextMenu();

	private C1ContextMenu ctxVouEmpty = new C1ContextMenu();

	private Ledger Ledger => _owner.Ledger;

	private DateTime StartDate => _owner.StartDate;

	private DateTime EndDate => _owner.EndDate;

	public BooksStyle BooksStyle => UserSet.Config.BooksStyle;

	public bool PendingAllEvent { get; set; }

	public SubOrTotal SubStatus { get; set; }

	public TotalFlag SubDisplay { get; set; }

	public TotalFlag TotalDisplay { get; set; }

	public TotalFlag TotalFlagDisplay
	{
		get
		{
			if (SubStatus != SubOrTotal.Total)
			{
				return SubDisplay;
			}
			return TotalDisplay;
		}
	}

	public C1SplitContainer View { get; private set; }

	public SubsidiaryEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitComponent();
		BindSubContexMenu();
		BindVoucherContexMenu();
		Initialize();
	}

	public void ShowSideToolbar()
	{
		pnlSidebar?.Show();
	}

	public void HideSideToolbar()
	{
		pnlSidebar?.Hide();
	}

	public void ShowFillToTable(bool isShow)
	{
		cmdFillToTableOnToolbar.Visible = isShow;
		cmdFillToTable.Visible = isShow;
	}

	private void InitComponent()
	{
		View = new C1SplitContainer();
		pnlSubsidiaryTitle = new C1SplitterPanel();
		pnlSubsidiaryHead = new C1SplitterPanel();
		pnlSubsidiaryGrid = new C1SplitterPanel();
		pnlSubsidiaryVoucher = new C1SplitterPanel();
		pnlSubsidiayFoot = new C1SplitterPanel();
		ctnVoucher = new C1SplitContainer();
		pnlVoucherTitle = new C1SplitterPanel();
		pnlVoucherHead = new C1SplitterPanel();
		pnlVoucherGrid = new C1SplitterPanel();
		pnlVoucherFoot = new C1SplitterPanel();
		btnSubsidiaryBack = new C1Button();
		lblSubsidiaryTitle = new C1Label();
		lblAccountName = new C1Label();
		lblSubStartDate = new C1Label();
		lblPeriod = new C1Label();
		lblSubEndDate = new C1Label();
		lblSubCurrency = new C1Label();
		grdSubsidiary = new C1FlexGridEx();
		grdSubsidiary.Name = "grdSubsidiary";
		lblVoucherTitle = new C1Label();
		lblVoucherType = new C1Label();
		lblVoucherNumber = new C1Label();
		lblVoucherDate = new C1Label();
		lblNumAttachments = new C1Label();
		grdVoucher = new C1FlexGridEx();
		grdVoucher.Name = "grdVoucher";
		lblChecker = new C1Label();
		lblMaker = new C1Label();
		lblBooker = new C1Label();
		SubDockingTab = new C1DockingTab();
		btnSubsidiaryBack.Location = new Point(0, -3);
		btnSubsidiaryBack.Size = new Size(30, 30);
		btnSubsidiaryBack.Image = Auditai.UI.LedgerView.Properties.Resources.back;
		btnSubsidiaryBack.FlatStyle = FlatStyle.Flat;
		btnSubsidiaryBack.FlatAppearance.BorderSize = 0;
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblSubsidiaryTitle.TextDetached = true;
		lblSubsidiaryTitle.BorderStyle = BorderStyle.None;
		lblSubsidiaryTitle.Dock = DockStyle.Fill;
		lblSubsidiaryTitle.Font = font;
		lblSubsidiaryTitle.Text = "明细账";
		lblSubsidiaryTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlSubsidiaryTitle.Height = 30;
		pnlSubsidiaryTitle.KeepRelativeSize = false;
		pnlSubsidiaryTitle.Location = new Point(0, 0);
		pnlSubsidiaryTitle.MinHeight = 30;
		pnlSubsidiaryTitle.Resizable = false;
		pnlSubsidiaryTitle.Size = new Size(927, 30);
		pnlSubsidiaryTitle.SizeRatio = 5.025;
		pnlSubsidiaryTitle.Controls.Add(btnSubsidiaryBack);
		pnlSubsidiaryTitle.Controls.Add(lblSubsidiaryTitle);
		Font font2 = new Font("Microsoft YaHei", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblAccountName.TextDetached = true;
		lblAccountName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		lblAccountName.BorderStyle = BorderStyle.None;
		lblAccountName.Font = font2;
		lblAccountName.Location = new Point(3, 4);
		lblAccountName.Size = new Size(367, 17);
		lblAccountName.Text = "科目名称：";
		lblAccountName.TextAlign = ContentAlignment.MiddleLeft;
		lblSubStartDate.TextDetached = true;
		lblSubStartDate.Anchor = AnchorStyles.Top;
		lblSubStartDate.BorderStyle = BorderStyle.None;
		lblSubStartDate.Location = new Point(373, 4);
		lblSubStartDate.Size = new Size(75, 18);
		lblSubStartDate.Text = "2000-01-01";
		lblSubStartDate.TextAlign = ContentAlignment.MiddleRight;
		lblSubEndDate.TextDetached = true;
		lblSubEndDate.Anchor = AnchorStyles.Top;
		lblSubEndDate.BorderStyle = BorderStyle.None;
		lblSubEndDate.Location = new Point(476, 4);
		lblSubEndDate.Size = new Size(76, 18);
		lblSubEndDate.Text = "2000-01-01";
		lblSubEndDate.TextAlign = ContentAlignment.MiddleLeft;
		lblPeriod.TextDetached = true;
		lblPeriod.Anchor = AnchorStyles.Top;
		lblPeriod.BorderStyle = BorderStyle.None;
		lblPeriod.Location = new Point(452, 4);
		lblPeriod.Size = new Size(20, 18);
		lblPeriod.Text = "至";
		lblPeriod.TextAlign = ContentAlignment.MiddleCenter;
		lblSubCurrency.TextDetached = true;
		lblSubCurrency.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		lblSubCurrency.BorderStyle = BorderStyle.None;
		lblSubCurrency.Font = font2;
		lblSubCurrency.Location = new Point(727, 6);
		lblSubCurrency.Size = new Size(180, 17);
		lblSubCurrency.Text = "金额单位：";
		lblSubCurrency.TextAlign = ContentAlignment.MiddleRight;
		pnlSubsidiaryHead.Height = 25;
		pnlSubsidiaryHead.KeepRelativeSize = false;
		pnlSubsidiaryHead.Location = new Point(0, 31);
		pnlSubsidiaryHead.MinHeight = 25;
		pnlSubsidiaryHead.Resizable = false;
		pnlSubsidiaryHead.Size = new Size(927, 25);
		pnlSubsidiaryHead.SizeRatio = 2.0;
		pnlSubsidiaryHead.Controls.Add(lblSubEndDate);
		pnlSubsidiaryHead.Controls.Add(lblPeriod);
		pnlSubsidiaryHead.Controls.Add(lblSubStartDate);
		pnlSubsidiaryHead.Controls.Add(lblSubCurrency);
		pnlSubsidiaryHead.Controls.Add(lblAccountName);
		pnlSubsidiaryHead.Paint += delegate(object s1, PaintEventArgs e1)
		{
			e1.Graphics.DrawLine(panelBorderPen, 0, pnlSubsidiaryHead.Height - 1, pnlSubsidiaryHead.Width, pnlSubsidiaryHead.Height - 1);
		};
		grdSubsidiary.AllowEditing = false;
		grdSubsidiary.AllowResizing = AllowResizingEnum.Both;
		grdSubsidiary.AllowSorting = AllowSortingEnum.None;
		grdSubsidiary.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdSubsidiary.Dock = DockStyle.Fill;
		grdSubsidiary.DrawMode = DrawModeEnum.OwnerDraw;
		grdSubsidiary.Font = font2;
		grdSubsidiary.Rows.DefaultSize = 30;
		grdSubsidiary.Tree.LineColor = Color.DimGray;
		grdSubsidiary.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		grdSubsidiary.MouseDoubleClick += GrdSubsidiary_MouseDoubleClick;
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "显示合计累计";
		c1Command.Click += CmdSidebarAllSum_Click;
		c1CommandLink2.Command = c1Command;
		ctxSidebarTotalSummary.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "仅显示合计";
		c1Command2.Click += CmdSidebarMonthOnly_Click;
		c1CommandLink3.Command = c1Command2;
		ctxSidebarTotalSummary.CommandLinks.Add(c1CommandLink3);
		C1CommandLink c1CommandLink4 = new C1CommandLink();
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "仅显示累计";
		c1Command3.Click += CmdSidebarYearOnly_Click;
		c1CommandLink4.Command = c1Command3;
		ctxSidebarTotalSummary.CommandLinks.Add(c1CommandLink4);
		C1CommandLink c1CommandLink5 = new C1CommandLink();
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "不显示合计累计";
		c1Command4.CommandStateQuery += CmdSidebarOnlyData_CommandStateQuery;
		c1Command4.Click += CmdSidebarOnlyData_Click;
		c1CommandLink5.Command = c1Command4;
		ctxSidebarTotalSummary.CommandLinks.Add(c1CommandLink5);
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "合计累计";
		c1Command5.Image = Auditai.UI.LedgerView.Properties.Resources.sideTotalSummary;
		c1Command5.Click += CmdTotalSummary_Click;
		c1CommandLink.Command = c1Command5;
		toolBar.CommandLinks.Add(c1CommandLink);
		cmdSidebarDirectionChange.Text = "方向调整";
		cmdSidebarDirectionChange.Image = Auditai.UI.LedgerView.Properties.Resources.sideDirectionChange;
		cmdSidebarDirectionChange.UserData = grdSubsidiary;
		cmdSidebarDirectionChange.Click += delegate(object s1, ClickEventArgs e1)
		{
			if (grdSubsidiary.Row >= grdSubsidiary.Rows.Fixed && grdSubsidiary.Rows[grdSubsidiary.Row].UserData is Voucher voucher3)
			{
				if (voucher3.DirectionToggled)
				{
					_owner.DirectionReduce_Click(s1, e1);
				}
				else
				{
					_owner.DirectionChange_Click(s1, e1);
				}
			}
			else
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择方向调整的凭证行");
			}
		};
		lnkSidebarDirectionChange.Command = cmdSidebarDirectionChange;
		toolBar.CommandLinks.Add(lnkSidebarDirectionChange);
		cmdSidebarMarkVoucher.Text = "标记关注";
		cmdSidebarMarkVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.sideMarkVoucher;
		cmdSidebarMarkVoucher.UserData = grdSubsidiary;
		cmdSidebarMarkVoucher.Click += delegate(object s1, ClickEventArgs e1)
		{
			if (grdSubsidiary.Row >= grdSubsidiary.Rows.Fixed && grdSubsidiary.Rows[grdSubsidiary.Row].UserData is Voucher voucher2)
			{
				if (voucher2.VoucherMark)
				{
					CancelMarkImpl(s1, e1);
				}
				else
				{
					MakeMarkImpl(s1, e1);
				}
			}
			else
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择标记关注的凭证行");
			}
		};
		lnkSidebarMarkVoucher.Command = cmdSidebarMarkVoucher;
		toolBar.CommandLinks.Add(lnkSidebarMarkVoucher);
		cmdSidebarModifyBegin.Text = "修改期初数";
		cmdSidebarModifyBegin.Image = Auditai.UI.LedgerView.Properties.Resources.sideModifyBegin;
		cmdSidebarModifyBegin.Click += delegate
		{
			_owner.ModifyBeginBalance(_owner.CurrentAccount);
		};
		lnkSidebarModifyBegin.Command = cmdSidebarModifyBegin;
		toolBar.CommandLinks.Add(lnkSidebarModifyBegin);
		cmdSidebarModifyVoucher.Text = "修改凭证";
		cmdSidebarModifyVoucher.Image = Auditai.UI.LedgerView.Properties.Resources.sideModifyVoucher;
		cmdSidebarModifyVoucher.Click += async delegate
		{
			if (grdSubsidiary.Row >= grdSubsidiary.Rows.Fixed && grdSubsidiary.Rows[grdSubsidiary.Row].UserData is Voucher voucher)
			{
				await _owner.ModifyVoucher(voucher);
			}
			else
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择修改凭证的凭证行");
			}
		};
		lnkSidebarModifyVoucher.Command = cmdSidebarModifyVoucher;
		toolBar.CommandLinks.Add(lnkSidebarModifyVoucher);
		C1CommandLink c1CommandLink6 = new C1CommandLink();
		C1Command c1Command6 = new C1Command();
		c1Command6.Text = "填充至底稿";
		c1Command6.Image = Auditai.UI.LedgerView.Properties.Resources.sideFillToTable;
		c1Command6.Click += delegate
		{
			FillToTable();
		};
		c1Command6.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
		{
			e1.Visible = LedgerViewer.IsAuditPlatform;
		};
		c1CommandLink6.Command = c1Command6;
		cmdFillToTableOnToolbar = c1Command6;
		toolBar.CommandLinks.Add(c1CommandLink6);
		C1CommandLink c1CommandLink7 = new C1CommandLink();
		c1CommandLink7.Delimiter = true;
		C1Command c1Command7 = new C1Command();
		c1Command7.Text = "隐藏侧边栏";
		c1Command7.Image = Auditai.UI.LedgerView.Properties.Resources.sideHideSidebar;
		c1Command7.Click += CmdHideSidebar_Click;
		c1CommandLink7.Command = c1Command7;
		foreach (C1CommandLink commandLink in toolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		C1SplitContainer value = ComponentFactory.BuildSidebar(grdSubsidiary, toolBar, out pnlSidebar);
		pnlSubsidiaryGrid.HeaderLineColor = Color.Transparent;
		pnlSubsidiaryGrid.HeaderTextAlign = PanelTextAlign.Center;
		pnlSubsidiaryGrid.Height = 327;
		pnlSubsidiaryGrid.Location = new Point(0, 57);
		pnlSubsidiaryGrid.Size = new Size(927, 327);
		pnlSubsidiaryGrid.SizeRatio = 59.74;
		pnlSubsidiaryGrid.Controls.Add(value);
		lblVoucherTitle.TextDetached = true;
		lblVoucherTitle.Anchor = AnchorStyles.Top;
		lblVoucherTitle.AutoSize = true;
		lblVoucherTitle.BorderStyle = BorderStyle.None;
		lblVoucherTitle.Font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblVoucherTitle.Location = new Point(417, 0);
		lblVoucherTitle.Size = new Size(74, 25);
		lblVoucherTitle.Text = "记账凭证";
		lblVoucherTitle.TextAlign = ContentAlignment.MiddleCenter;
		btnCloseVoucher.Size = new Size(25, 25);
		btnCloseVoucher.Image = Auditai.UI.Controls.Properties.Resources.tileClose;
		btnCloseVoucher.ImageAlign = ContentAlignment.MiddleCenter;
		btnCloseVoucher.Dock = DockStyle.Right;
		btnCloseVoucher.FlatStyle = FlatStyle.Flat;
		btnCloseVoucher.FlatAppearance.BorderSize = 0;
		btnCloseVoucher.MouseEnter += delegate
		{
			btnCloseVoucher.Image = Auditai.UI.Controls.Properties.Resources.tileCloseSlide;
		};
		btnCloseVoucher.MouseLeave += delegate
		{
			btnCloseVoucher.Image = Auditai.UI.Controls.Properties.Resources.tileClose;
		};
		btnCloseVoucher.Click += delegate
		{
			ShowVoucher(visible: false);
		};
		btnCloseVoucher.MouseDown += delegate
		{
			btnCloseVoucher.Image = Auditai.UI.Controls.Properties.Resources.tileCloseDown;
		};
		btnCloseVoucher.MouseUp += delegate
		{
			btnCloseVoucher.Image = Auditai.UI.Controls.Properties.Resources.tileCloseSlide;
		};
		pnlVoucherTitle.Height = 25;
		pnlVoucherTitle.KeepRelativeSize = false;
		pnlVoucherTitle.Location = new Point(0, 0);
		pnlVoucherTitle.MinHeight = 25;
		pnlVoucherTitle.Resizable = false;
		pnlVoucherTitle.Size = new Size(927, 20);
		pnlVoucherTitle.SizeRatio = 10.204;
		pnlVoucherTitle.Controls.Add(btnCloseVoucher);
		pnlVoucherTitle.Controls.Add(lblVoucherTitle);
		lblVoucherType.TextDetached = true;
		lblVoucherType.AutoSize = true;
		lblVoucherType.BorderStyle = BorderStyle.None;
		lblVoucherType.Location = new Point(9, 1);
		lblVoucherType.Size = new Size(32, 17);
		lblVoucherType.Text = "字：";
		lblVoucherType.TextAlign = ContentAlignment.MiddleLeft;
		lblVoucherNumber.TextDetached = true;
		lblVoucherNumber.AutoSize = true;
		lblVoucherNumber.BorderStyle = BorderStyle.None;
		lblVoucherNumber.Location = new Point(92, 1);
		lblVoucherNumber.Margin = new Padding(10, 0, 3, 0);
		lblVoucherNumber.Size = new Size(32, 17);
		lblVoucherNumber.Text = "号：";
		lblVoucherNumber.TextAlign = ContentAlignment.MiddleLeft;
		lblVoucherDate.TextDetached = true;
		lblVoucherDate.Anchor = AnchorStyles.Top;
		lblVoucherDate.BorderStyle = BorderStyle.None;
		lblVoucherDate.Location = new Point(383, 1);
		lblVoucherDate.Size = new Size(150, 17);
		lblVoucherDate.Text = "制单日期：0000-00-00";
		lblVoucherDate.TextAlign = ContentAlignment.MiddleCenter;
		lblNumAttachments.TextDetached = true;
		lblNumAttachments.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		lblNumAttachments.BorderStyle = BorderStyle.None;
		lblNumAttachments.ImageAlign = ContentAlignment.MiddleRight;
		lblNumAttachments.Location = new Point(727, 1);
		lblNumAttachments.Size = new Size(180, 17);
		lblNumAttachments.Text = "附件张数：";
		lblNumAttachments.TextAlign = ContentAlignment.MiddleRight;
		pnlVoucherHead.HeaderTextAlign = PanelTextAlign.Center;
		pnlVoucherHead.Height = 20;
		pnlVoucherHead.KeepRelativeSize = false;
		pnlVoucherHead.Location = new Point(0, 21);
		pnlVoucherHead.Resizable = false;
		pnlVoucherHead.Size = new Size(927, 20);
		pnlVoucherHead.SizeRatio = 2.0;
		pnlVoucherHead.Width = 927;
		pnlVoucherHead.Controls.Add(lblVoucherType);
		pnlVoucherHead.Controls.Add(lblVoucherNumber);
		pnlVoucherHead.Controls.Add(lblVoucherDate);
		pnlVoucherHead.Controls.Add(lblNumAttachments);
		grdVoucher.AllowEditing = false;
		grdVoucher.AllowResizing = AllowResizingEnum.Both;
		grdVoucher.AllowSorting = AllowSortingEnum.None;
		grdVoucher.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdVoucher.Dock = DockStyle.Fill;
		grdVoucher.DrawMode = DrawModeEnum.OwnerDraw;
		grdVoucher.Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		grdVoucher.Location = new Point(0, 0);
		grdVoucher.Rows.DefaultSize = 20;
		grdVoucher.Size = new Size(927, 126);
		grdVoucher.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		pnlVoucherGrid.Height = 126;
		pnlVoucherGrid.Location = new Point(0, 42);
		pnlVoucherGrid.Size = new Size(927, 126);
		pnlVoucherGrid.SizeRatio = 95.0;
		pnlVoucherGrid.Controls.Add(grdVoucher);
		lblChecker.TextDetached = true;
		lblChecker.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
		lblChecker.BorderStyle = BorderStyle.None;
		lblChecker.ImageAlign = ContentAlignment.MiddleRight;
		lblChecker.Location = new Point(727, 7);
		lblChecker.Size = new Size(180, 17);
		lblChecker.Text = "审核人：";
		lblChecker.TextAlign = ContentAlignment.MiddleRight;
		lblMaker.TextDetached = true;
		lblMaker.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
		lblMaker.BorderStyle = BorderStyle.None;
		lblMaker.Location = new Point(9, 7);
		lblMaker.Size = new Size(150, 17);
		lblMaker.Text = "制单人：";
		lblMaker.TextAlign = ContentAlignment.MiddleLeft;
		lblBooker.TextDetached = true;
		lblBooker.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
		lblBooker.BorderStyle = BorderStyle.None;
		lblBooker.Location = new Point(383, 7);
		lblBooker.Size = new Size(150, 17);
		lblBooker.Text = "记账人：";
		lblBooker.TextAlign = ContentAlignment.MiddleCenter;
		pnlVoucherFoot.Dock = PanelDockStyle.Bottom;
		pnlVoucherFoot.Height = 30;
		pnlVoucherFoot.KeepRelativeSize = false;
		pnlVoucherFoot.Location = new Point(0, 169);
		pnlVoucherFoot.MinHeight = 30;
		pnlVoucherFoot.MinWidth = 30;
		pnlVoucherFoot.Resizable = false;
		pnlVoucherFoot.Size = new Size(927, 30);
		pnlVoucherFoot.SizeRatio = 100.0;
		pnlVoucherFoot.Controls.Add(lblChecker);
		pnlVoucherFoot.Controls.Add(lblMaker);
		pnlVoucherFoot.Controls.Add(lblBooker);
		ctnVoucher.AutoSizeElement = AutoSizeElement.Both;
		ctnVoucher.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnVoucher.Dock = DockStyle.Fill;
		ctnVoucher.Location = new Point(0, 0);
		ctnVoucher.Size = new Size(927, 199);
		ctnVoucher.SplitterWidth = 2;
		ctnVoucher.Panels.Add(pnlVoucherFoot);
		ctnVoucher.Panels.Add(pnlVoucherTitle);
		ctnVoucher.Panels.Add(pnlVoucherHead);
		ctnVoucher.Panels.Add(pnlVoucherGrid);
		SubDockingTab.BorderStyle = BorderStyle.None;
		SubDockingTab.CanCloseTabs = true;
		SubDockingTab.CanMoveTabs = true;
		SubDockingTab.CloseBox = CloseBoxPositionEnum.AllPages;
		SubDockingTab.Dock = DockStyle.Fill;
		SubDockingTab.KeepClosedPages = false;
		SubDockingTab.Location = new Point(0, 0);
		SubDockingTab.Size = new Size(927, 23);
		SubDockingTab.TabsShowFocusCues = false;
		SubDockingTab.TabsSpacing = 0;
		SubDockingTab.ItemSize = new Size(0, 0);
		pnlSubsidiayFoot.Dock = PanelDockStyle.Bottom;
		pnlSubsidiayFoot.Height = 23;
		pnlSubsidiayFoot.Location = new Point(0, 607);
		pnlSubsidiayFoot.MinHeight = 23;
		pnlSubsidiayFoot.Resizable = false;
		pnlSubsidiayFoot.Size = new Size(927, 23);
		pnlSubsidiayFoot.SizeRatio = 3.657;
		pnlSubsidiayFoot.Controls.Add(SubDockingTab);
		pnlSubsidiaryVoucher.Font = new Font("Microsoft YaHei", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		pnlSubsidiaryVoucher.HeaderLineColor = Color.Transparent;
		pnlSubsidiaryVoucher.HeaderTextAlign = PanelTextAlign.Center;
		pnlSubsidiaryVoucher.Height = 220;
		pnlSubsidiaryVoucher.Location = new Point(0, 407);
		pnlSubsidiaryVoucher.Size = new Size(927, 199);
		pnlSubsidiaryVoucher.SizeRatio = 100.0;
		pnlSubsidiaryVoucher.Controls.Add(ctnVoucher);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.BackColor = Color.FromArgb(240, 240, 240);
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.ForeColor = Color.FromArgb(0, 0, 0);
		View.Location = new Point(0, 0);
		View.Size = new Size(927, 630);
		View.SplitterWidth = 2;
		View.Panels.Add(pnlSubsidiayFoot);
		View.Panels.Add(pnlSubsidiaryTitle);
		View.Panels.Add(pnlSubsidiaryHead);
		View.Panels.Add(pnlSubsidiaryGrid);
		View.Panels.Add(pnlSubsidiaryVoucher);
		_owner._owner.AfterOpenLedger += _owner_AfterOpenLedger;
	}

	private void GrdSubsidiary_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		int mouseRow = grdSubsidiary.MouseRow;
		int mouseCol = grdSubsidiary.MouseCol;
		if (mouseRow >= 0 && mouseRow < grdSubsidiary.Rows.Fixed && (mouseCol == grdSubsidiary.Cols["Credit"].Index || mouseCol == grdSubsidiary.Cols["Debit"].Index || mouseCol == grdSubsidiary.Cols["Balance"].Index))
		{
			C1.Win.C1FlexGrid.Column column = grdSubsidiary.Cols[mouseCol];
			switch (column.Sort)
			{
			case SortFlags.None:
				column.Sort = SortFlags.Descending;
				break;
			case SortFlags.Ascending:
				column.Sort = SortFlags.None;
				break;
			case SortFlags.Descending:
				column.Sort = SortFlags.Ascending;
				break;
			}
			SortColumn(column.Sort, mouseCol);
		}
	}

	private void SortColumn(SortFlags sortFlags, int mouseCol)
	{
		Point scrollPosition = grdSubsidiary.ScrollPosition;
		grdSubsidiary.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Column column = grdSubsidiary.Cols[mouseCol];
			column.Sort = sortFlags;
			grdSubsidiary.AllowSorting = AllowSortingEnum.SingleColumn;
			column.AllowSorting = true;
			grdSubsidiary.Sort(column.Sort, mouseCol);
			if (column.Sort == SortFlags.None)
			{
				C1.Win.C1FlexGrid.Column column2 = grdSubsidiary.Cols["Index"];
				grdSubsidiary.Sort(SortFlags.Ascending, column2.Index);
				grdSubsidiary.Sort(SortFlags.None, column2.Index);
			}
			grdSubsidiary.AllowSorting = AllowSortingEnum.None;
			if (grdSubsidiary.Rows.Count > grdSubsidiary.Rows.Fixed)
			{
				grdSubsidiary.Select(grdSubsidiary.Rows.Fixed, column.Index, grdSubsidiary.Rows.Count - 1, column.Index);
			}
		}
		finally
		{
			grdSubsidiary.ScrollPosition = scrollPosition;
			grdSubsidiary.EndUpdate();
		}
	}

	private void CmdTotalSummary_Click(object sender, ClickEventArgs e)
	{
		ctxSidebarTotalSummary.ShowContextMenu(e.CallerLink.Owner as C1ToolBar, new Point(e.CallerLink.Bounds.Left, e.CallerLink.Bounds.Bottom));
	}

	private void CmdSidebarAllSum_Click(object sender, ClickEventArgs e)
	{
		if (SubStatus == SubOrTotal.Total)
		{
			TotalDisplay = TotalDisplayFlags.AllSum;
			_owner.SelectTotal();
		}
		else
		{
			SubDisplay = SubDisplayFlags.AllSumAndData;
			_owner.SelectSub();
		}
	}

	private void CmdSidebarMonthOnly_Click(object sender, ClickEventArgs e)
	{
		if (SubStatus == SubOrTotal.Total)
		{
			TotalDisplay = TotalDisplayFlags.MonthOnly;
			_owner.SelectTotal();
		}
		else
		{
			SubDisplay = SubDisplayFlags.MonthAndData;
			_owner.SelectSub();
		}
	}

	private void CmdSidebarYearOnly_Click(object sender, ClickEventArgs e)
	{
		if (SubStatus == SubOrTotal.Total)
		{
			TotalDisplay = TotalDisplayFlags.YearOnly;
			_owner.SelectTotal();
		}
		else
		{
			SubDisplay = SubDisplayFlags.YearAndData;
			_owner.SelectSub();
		}
	}

	private void CmdSidebarOnlyData_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = SubStatus == SubOrTotal.Subsidiary;
	}

	private void CmdSidebarOnlyData_Click(object sender, ClickEventArgs e)
	{
		SubDisplay = SubDisplayFlags.DataOnly;
		_owner.SelectSub();
	}

	private void CmdHideSidebar_Click(object sender, ClickEventArgs e)
	{
		_owner.OnHideSidebarClick();
	}

	public void SetTitle(string startDate, string endDate)
	{
		lblSubStartDate.Text = startDate;
		lblSubEndDate.Text = endDate;
	}

	public void AddVisitHistory(object userdata)
	{
		if (!addVisit)
		{
			return;
		}
		string empty = string.Empty;
		if (!(userdata is Account account))
		{
			if (!(userdata is Tuple<Account, AuxiliaryItem> tuple))
			{
				return;
			}
			empty = Common.GetFullNameWithCode(tuple.Item1, tuple.Item2);
		}
		else
		{
			empty = Common.GetFullNameWithCode(account);
		}
		C1DockingTabPage c1DockingTabPage = new C1DockingTabPage
		{
			Name = empty,
			Text = empty,
			Tag = userdata,
			Font = new Font("微软雅黑", 9f),
			ForeColor = Color.Black
		};
		c1DockingTabPage.TabClick += delegate(object s1, EventArgs e1)
		{
			try
			{
				addVisit = false;
				C1DockingTabPage c1DockingTabPage3 = s1 as C1DockingTabPage;
				UpdateValueFromUserData(c1DockingTabPage3.Tag);
			}
			catch
			{
			}
			finally
			{
				addVisit = true;
			}
		};
		foreach (C1DockingTabPage tabPage in SubDockingTab.TabPages)
		{
			if (tabPage.Name == c1DockingTabPage.Name)
			{
				SubDockingTab.SelectedTab = tabPage;
				return;
			}
		}
		SubDockingTab.TabPages.Add(c1DockingTabPage);
		if (SubDockingTab.TabPages.Count > 10)
		{
			SubDockingTab.TabPages.RemoveAt(0);
		}
		SubDockingTab.SelectedIndex = c1DockingTabPage.TabIndex;
	}

	public void UpdateValueFromUserData(object userData)
	{
		if (!(userData is Account account))
		{
			if (userData is Tuple<Account, AuxiliaryItem> tuple)
			{
				PopulateSubsidiarySheet(tuple.Item1, _owner.StartDate, _owner.EndDate, tuple.Item2);
				UpdateTitle(tuple.Item1, tuple.Item2);
				_owner.CurrentAccount = tuple.Item1;
				_owner.CurrentAuxiliary = tuple.Item2;
				_owner.AccountTreeEditor.UpdateNodeStatus(tuple);
			}
		}
		else
		{
			PopulateSubsidiarySheet(account, _owner.StartDate, _owner.EndDate);
			UpdateTitle(account);
			_owner.CurrentAccount = account;
			_owner.CurrentAuxiliary = null;
			_owner.AccountTreeEditor.UpdateNodeStatus(account);
		}
	}

	public void PopulateSubsidiarySheet(Account account, DateTime startDate, DateTime endDate, AuxiliaryItem auxiliaryItem = null)
	{
		grdSubsidiary.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			InitializeSubsidiaryCaption(account);
			grdSubsidiary.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grdSubsidiary.Rows.Fixed = 1;
			grdSubsidiary.Cols.Fixed = 1;
			int num = 1;
			SubsidiaryLedger subsidiaryLedger = ((auxiliaryItem != null) ? Ledger.GetSubsidiaryLedger(account, startDate, endDate, auxiliaryItem) : Ledger.GetSubsidiaryLedger(account, startDate, endDate));
			if (subsidiaryLedger.BeginBalance != 0m)
			{
				C1.Win.C1FlexGrid.Row row = grdSubsidiary.Rows.Add();
				row.UserData = "BeginBalance";
				row["Index"] = num++;
				row["Balance"] = Math.Abs(subsidiaryLedger.BeginBalance);
				row["DC"] = Common.GetDCChar(account.IsDebit, subsidiaryLedger.BeginBalance);
				row["Digest"] = "期初余额";
			}
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdVoucher.Styles.Add("center");
			cellStyle.TextAlign = TextAlignEnum.CenterCenter;
			cellStyle.DataType = typeof(string);
			if (SubStatus == SubOrTotal.Subsidiary)
			{
				grdSubsidiary.Cols["MyMark"].Visible = true;
				grdSubsidiary.Cols["Date"].Visible = true;
				grdSubsidiary.Cols["Type"].Visible = true;
				grdSubsidiary.Cols["Number"].Visible = true;
				grdSubsidiary.Cols["Opposite"].Visible = true;
				int index = grdSubsidiary.Cols["MyMark"].Index;
				foreach (MonthSubsidiaryLedger item in from d in subsidiaryLedger.Months
					orderby d.Year, d.Month
					select d)
				{
					if (TotalFlagDisplay.HasFlag(TotalFlag.Data))
					{
						IOrderedEnumerable<SubsidiaryLedgerEntry> orderedEnumerable = item.Entries.OrderBy((SubsidiaryLedgerEntry t) => t.Voucher.Day).ThenBy((SubsidiaryLedgerEntry s) => s.Voucher.Type.Name).ThenBy((SubsidiaryLedgerEntry m) => m.Voucher.Number, StringNumberComparer.Instance);
						foreach (SubsidiaryLedgerEntry item2 in orderedEnumerable)
						{
							C1.Win.C1FlexGrid.Row row2 = grdSubsidiary.Rows.Add();
							row2.UserData = item2.Voucher;
							row2["Index"] = num++;
							row2["Date"] = item2.Voucher.Day;
							row2["Type"] = item2.Voucher.Type.Name;
							row2["Number"] = item2.Voucher.Number;
							row2["Digest"] = item2.Voucher.Digest;
							row2["Debit"] = (item2.Voucher.IsDebit ? item2.Voucher.Amount : 0m);
							row2["Credit"] = (item2.Voucher.IsDebit ? 0m : item2.Voucher.Amount);
							row2["DC"] = Common.GetDCChar(account.IsDebit, item2.Balance);
							row2["Balance"] = Math.Abs(item2.Balance);
							row2["Opposite"] = string.Join(",", item2.Voucher.OppositeAccounts.Select((Account t) => t.Name).Distinct());
							grdSubsidiary.SetCellCheck(row2.Index, index, item2.Voucher.VoucherMark ? CheckEnum.Checked : CheckEnum.Unchecked);
							if (item2.Voucher.VoucherMark)
							{
								row2.StyleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
								row2.StyleNew.ForeColor = Common.MarkForeColor;
							}
						}
					}
					if (TotalFlagDisplay.HasFlag(TotalFlag.MonthSum))
					{
						C1.Win.C1FlexGrid.Row row3 = grdSubsidiary.Rows.Add();
						row3.UserData = "MonthTotal";
						row3["Index"] = num++;
						row3["Digest"] = $"本月合计（{item.Year}年{item.Month}月）";
						row3["Debit"] = item.Total.Debit;
						row3["Credit"] = item.Total.Credit;
						row3["DC"] = Common.GetDCChar(account.IsDebit, item.Total.Balance);
						row3["Balance"] = Math.Abs(item.Total.Balance);
						row3.StyleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
						grdSubsidiary.SetCellStyle(row3.Index, "Digest", cellStyle);
						grdSubsidiary.SetCellCheck(row3.Index, index, CheckEnum.None);
					}
					if (TotalFlagDisplay.HasFlag(TotalFlag.YearSum))
					{
						C1.Win.C1FlexGrid.Row row4 = grdSubsidiary.Rows.Add();
						row4.UserData = "YearTotal";
						row4["Index"] = num++;
						row4["Digest"] = $"本年累计（{item.Year}年{item.Month}月）";
						row4["Debit"] = item.GrandTotal.Debit;
						row4["Credit"] = item.GrandTotal.Credit;
						row4["DC"] = Common.GetDCChar(account.IsDebit, item.GrandTotal.Balance);
						row4["Balance"] = Math.Abs(item.GrandTotal.Balance);
						row4.StyleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
						grdSubsidiary.SetCellStyle(row4.Index, "Digest", cellStyle);
						grdSubsidiary.SetCellCheck(row4.Index, index, CheckEnum.None);
					}
				}
			}
			else if (SubStatus == SubOrTotal.Total)
			{
				grdSubsidiary.Cols["MyMark"].Visible = false;
				grdSubsidiary.Cols["Date"].Visible = false;
				grdSubsidiary.Cols["Type"].Visible = false;
				grdSubsidiary.Cols["Number"].Visible = false;
				grdSubsidiary.Cols["Opposite"].Visible = false;
				decimal num2 = subsidiaryLedger.BeginBalance;
				decimal num3 = subsidiaryLedger.BeginBalance;
				IOrderedEnumerable<IGrouping<int, MonthSubsidiaryLedger>> orderedEnumerable2 = from m in subsidiaryLedger.Months
					group m by m.Year into m
					orderby m.Key
					select m;
				foreach (IGrouping<int, MonthSubsidiaryLedger> item3 in orderedEnumerable2)
				{
					Dictionary<int, MonthSubsidiaryLedger> dictionary = item3.ToDictionary((MonthSubsidiaryLedger m) => m.Month, (MonthSubsidiaryLedger m) => m);
					for (int i = 1; i <= 12; i++)
					{
						MonthSubsidiaryLedger monthSubsidiaryLedger = (dictionary.ContainsKey(i) ? dictionary[i] : null);
						if (TotalFlagDisplay.HasFlag(TotalFlag.MonthSum))
						{
							decimal num4 = monthSubsidiaryLedger?.Total.Balance ?? num2;
							num2 = num4;
							C1.Win.C1FlexGrid.Row row5 = grdSubsidiary.Rows.Add();
							row5.UserData = "MonthTotal";
							row5["Index"] = num++;
							row5["Digest"] = $"本月合计（{item3.Key}年{i}月）";
							row5["Debit"] = monthSubsidiaryLedger?.Total.Debit ?? 0m;
							row5["Credit"] = monthSubsidiaryLedger?.Total.Credit ?? 0m;
							row5["DC"] = Common.GetDCChar(account.IsDebit, num4);
							row5["Balance"] = Math.Abs(num4);
							row5.StyleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
							grdSubsidiary.SetCellStyle(row5.Index, "Digest", cellStyle);
						}
						if (TotalFlagDisplay.HasFlag(TotalFlag.YearSum))
						{
							decimal num5 = monthSubsidiaryLedger?.GrandTotal.Balance ?? num3;
							num3 = num5;
							C1.Win.C1FlexGrid.Row row6 = grdSubsidiary.Rows.Add();
							row6.UserData = "YearTotal";
							row6["Index"] = num++;
							row6["Digest"] = $"本年累计（{item3.Key}年{i}月）";
							row6["Debit"] = monthSubsidiaryLedger?.GrandTotal.Debit ?? 0m;
							row6["Credit"] = monthSubsidiaryLedger?.GrandTotal.Credit ?? 0m;
							row6["DC"] = Common.GetDCChar(account.IsDebit, num5);
							row6["Balance"] = Math.Abs(num5);
							row6.StyleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
							grdSubsidiary.SetCellStyle(row6.Index, "Digest", cellStyle);
						}
					}
				}
			}
			PopulateBottomVoucher();
			_owner.StyleRecord.ResumeStyle(grdSubsidiary);
			_owner.StyleRecord.ResumeStyle(grdVoucher);
			if (SubStatus == SubOrTotal.Total)
			{
				ShowVoucher(visible: false);
			}
		}
		catch
		{
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			grdSubsidiary.EndUpdate();
		}
	}

	private void InitializeSubsidiaryCaption(Account account)
	{
		if (!initializedSubsidiaryCaption)
		{
			grdSubsidiary.Cols.Count = 0;
			grdSubsidiary.Rows.Count = 1;
			grdSubsidiary.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grdSubsidiary.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(int);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 50;
			column = grdSubsidiary.Cols.Add();
			column.Name = "MyMark";
			column.Caption = "标记关注";
			column.DataType = typeof(bool);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 65;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Date";
			column.Caption = "日期";
			column.DataType = typeof(DateTime);
			column.Format = "yyyy-MM-dd";
			column.AllowMerging = true;
			column.Width = 80;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Type";
			column.Caption = "字";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowMerging = true;
			column.Width = 30;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Number";
			column.Caption = "号";
			column.UserData = account;
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowMerging = true;
			column.Width = 50;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 220;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Opposite";
			column.Caption = "对方科目";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 120;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 100;
			column.Sort = SortFlags.None;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 100;
			column.Sort = SortFlags.None;
			column = grdSubsidiary.Cols.Add();
			column.Name = "DC";
			column.Caption = "方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowMerging = true;
			column.Width = 50;
			column = grdSubsidiary.Cols.Add();
			column.Name = "Balance";
			column.Caption = "余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 100;
			column.Sort = SortFlags.None;
			initializedSubsidiaryCaption = true;
		}
		else
		{
			grdSubsidiary.Rows.Count = 1;
			C1.Win.C1FlexGrid.Column column2 = grdSubsidiary.Cols["Index"];
			column2.Caption = "序号";
			column2.DataType = typeof(int);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2 = grdSubsidiary.Cols["MyMark"];
			column2.Caption = "标记关注";
			column2.DataType = typeof(bool);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2 = grdSubsidiary.Cols["Date"];
			column2.Caption = "日期";
			column2.DataType = typeof(DateTime);
			column2.Format = "yyyy-MM-dd";
			column2.AllowMerging = true;
			column2 = grdSubsidiary.Cols["Type"];
			column2.Caption = "字";
			column2.DataType = typeof(string);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2.AllowMerging = true;
			column2 = grdSubsidiary.Cols["Number"];
			column2.Caption = "号";
			column2.UserData = account;
			column2.DataType = typeof(string);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2.AllowMerging = true;
			column2 = grdSubsidiary.Cols["Digest"];
			column2.Caption = "摘要";
			column2.DataType = typeof(string);
			column2.AllowMerging = true;
			column2 = grdSubsidiary.Cols["Opposite"];
			column2.Caption = "对方科目";
			column2.DataType = typeof(string);
			column2.AllowMerging = true;
			column2 = grdSubsidiary.Cols["Debit"];
			column2.Caption = "借方金额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.AllowMerging = true;
			column2.Sort = SortFlags.None;
			column2 = grdSubsidiary.Cols["Credit"];
			column2.Caption = "贷方金额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.AllowMerging = true;
			column2.Sort = SortFlags.None;
			column2 = grdSubsidiary.Cols["DC"];
			column2.Caption = "方向";
			column2.DataType = typeof(string);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2.AllowMerging = true;
			column2 = grdSubsidiary.Cols["Balance"];
			column2.Caption = "余额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.AllowMerging = true;
			column2.Sort = SortFlags.None;
		}
	}

	public void PopulateVouchers(IEnumerable<Voucher> vouchers)
	{
		grdVoucher.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			InitializeVoucherCaption();
			grdVoucher.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grdVoucher.Rows.Fixed = 1;
			grdVoucher.Cols.Fixed = 1;
			int num = 1;
			decimal num2 = default(decimal);
			decimal num3 = default(decimal);
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdVoucher.Styles.Add("center");
			cellStyle.TextAlign = TextAlignEnum.CenterCenter;
			cellStyle.DataType = typeof(string);
			foreach (Voucher voucher in vouchers)
			{
				C1.Win.C1FlexGrid.Row row = grdVoucher.Rows.Add();
				row.UserData = voucher;
				row["Index"] = num++;
				row["Digest"] = voucher.Digest;
				row["Code"] = voucher.GetDisplayAccountCodeWithDetail();
				row["Name"] = voucher.GetDisplayAccountNameWithDetail();
				row["Debit"] = (voucher.IsDebit ? voucher.Amount : 0m);
				row["Credit"] = (voucher.IsDebit ? 0m : voucher.Amount);
				num2 += (voucher.IsDebit ? voucher.Amount : 0m);
				num3 += (voucher.IsDebit ? 0m : voucher.Amount);
				if (voucher.VoucherMark)
				{
					row.StyleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
					row.StyleNew.ForeColor = Common.MarkForeColor;
				}
			}
			C1.Win.C1FlexGrid.Row row2 = grdVoucher.Rows.Add();
			row2["Digest"] = "合计";
			row2["Debit"] = num2;
			row2["Credit"] = num3;
			row2.StyleNew.BackColor = UserSet.Config.TableStyle.FormalaColor;
			grdVoucher.SetCellStyle(row2.Index, "Digest", cellStyle);
			SetVoucherHeader(vouchers.FirstOrDefault());
			_owner.StyleRecord.ResumeStyle(grdVoucher);
		}
		catch
		{
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			grdVoucher.EndUpdate();
		}
	}

	private void InitializeVoucherCaption()
	{
		if (!initializedVoucherCaption)
		{
			grdVoucher.Cols.Count = 0;
			grdVoucher.Rows.Count = 1;
			grdVoucher.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grdVoucher.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(int);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowMerging = true;
			column.Width = 50;
			column = grdVoucher.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 220;
			column = grdVoucher.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 100;
			column = grdVoucher.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 200;
			column = grdVoucher.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 100;
			column = grdVoucher.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.AllowMerging = true;
			column.Width = 100;
			initializedVoucherCaption = true;
		}
		else
		{
			grdVoucher.Rows.Count = 1;
			C1.Win.C1FlexGrid.Column column2 = grdVoucher.Cols["Index"];
			column2.Caption = "序号";
			column2.DataType = typeof(int);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2.AllowMerging = true;
			column2 = grdVoucher.Cols["Digest"];
			column2.Caption = "摘要";
			column2.DataType = typeof(string);
			column2.AllowMerging = true;
			column2 = grdVoucher.Cols["Code"];
			column2.Caption = "科目代码";
			column2.DataType = typeof(string);
			column2.AllowMerging = true;
			column2 = grdVoucher.Cols["Name"];
			column2.Caption = "科目名称";
			column2.DataType = typeof(string);
			column2.AllowMerging = true;
			column2 = grdVoucher.Cols["Debit"];
			column2.Caption = "借方金额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.AllowMerging = true;
			column2 = grdVoucher.Cols["Credit"];
			column2.Caption = "贷方金额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.AllowMerging = true;
		}
	}

	public void ShowVoucher(bool visible)
	{
		_voucherVisible = visible;
		pnlSubsidiaryVoucher.Visible = visible;
	}

	public void UpdateTitle(Account account)
	{
		lblAccountName.Text = "科目名称：" + Common.GetFullNameWithCode(account);
		lblSubCurrency.Text = "金额单位：" + (Ledger.BaseCurrency?.Name ?? "人民币元");
		lblSubsidiaryTitle.Text = ((SubStatus == SubOrTotal.Total) ? "总账" : "明细账");
	}

	public void UpdateTitle(Account account, AuxiliaryClass auxiliaryClass)
	{
		lblAccountName.Text = "科目名称：" + Common.GetFullNameWithCode(account) + "    辅助核算类别：（" + auxiliaryClass.Code + "）" + auxiliaryClass.Name;
		lblSubCurrency.Text = "金额单位：" + (Ledger.BaseCurrency?.Name ?? "人民币元");
		lblSubsidiaryTitle.Text = ((SubStatus == SubOrTotal.Total) ? "总账" : "明细账");
	}

	public void UpdateTitle(Account account, AuxiliaryItem auxiliaryItem)
	{
		lblAccountName.Text = "科目名称：" + Common.GetFullNameWithCode(account, auxiliaryItem);
		lblSubCurrency.Text = "金额单位：" + (Ledger.BaseCurrency?.Name ?? "人民币元");
		lblSubsidiaryTitle.Text = ((SubStatus == SubOrTotal.Total) ? "总账" : "明细账");
	}

	private void _grid_Click(object sender, EventArgs e)
	{
		if (!_voucherVisible || grdSubsidiary.Row < grdSubsidiary.Rows.Fixed)
		{
			return;
		}
		object userData = grdSubsidiary.Rows[grdSubsidiary.Row].UserData;
		Voucher voucher = userData as Voucher;
		if (voucher != null)
		{
			IEnumerable<Voucher> vouchers = Ledger.Vouchers.Where((Voucher t) => t.Type == voucher.Type && t.Number == voucher.Number && t.Day.Year == voucher.Day.Year && t.Day.Month == voucher.Day.Month);
			PopulateVouchers(vouchers);
		}
	}

	private void _grid_DoubleClick(object sender, EventArgs e)
	{
		try
		{
			int mouseRow = grdSubsidiary.MouseRow;
			int mouseCol = grdSubsidiary.MouseCol;
			if (mouseRow < grdSubsidiary.Rows.Fixed || mouseCol < grdSubsidiary.Cols.Fixed)
			{
				return;
			}
			switch (SubStatus)
			{
			case SubOrTotal.Total:
				ShowVoucher(visible: false);
				SubStatus = SubOrTotal.Subsidiary;
				PopulateSubsidiarySheet(_owner.CurrentAccount, StartDate, EndDate);
				UpdateTitle(_owner.CurrentAccount);
				break;
			case SubOrTotal.Subsidiary:
			{
				object userData = grdSubsidiary.Rows[grdSubsidiary.Row].UserData;
				Voucher voucher = userData as Voucher;
				if (voucher != null)
				{
					ShowVoucher(visible: true);
					IEnumerable<Voucher> vouchers = Ledger.Vouchers.Where((Voucher t) => t.Type == voucher.Type && t.Number == voucher.Number && t.Day.Year == voucher.Day.Year && t.Day.Month == voucher.Day.Month);
					PopulateVouchers(vouchers);
				}
				break;
			}
			}
		}
		catch
		{
		}
	}

	private void _grid_AfterResizeRow(object sender, RowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			_owner.StyleRecord.RecordHeight(sender as C1FlexGridEx, e.Row);
		}
	}

	private void _grid_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordWidth(c1FlexGridEx.Name, c1FlexGridEx.Cols[e.Col].Name, c1FlexGridEx.Cols[e.Col].Width);
		}
	}

	private void _grid_AfterDragColumn(object sender, DragRowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordOrder(c1FlexGridEx.Name, from C1.Win.C1FlexGrid.Column t in c1FlexGridEx.Cols
				select t.Name);
		}
	}

	private void btnBack_Click(object sender, EventArgs e)
	{
		switch (SubStatus)
		{
		case SubOrTotal.Total:
			if (_owner.SwitchToView(ActiveView.Balance))
			{
				Common.SetTreeCheck(_owner.AccountTreeEditor.Tree, CheckEnum.None);
			}
			break;
		case SubOrTotal.Subsidiary:
			if (BooksStyle.BalanceTo == SubOrTotal.Total)
			{
				SubStatus = SubOrTotal.Total;
				if (_owner.SwitchToView(ActiveView.Subsidiary))
				{
					Common.SetTreeCheck(_owner.AccountTreeEditor.Tree, CheckEnum.None);
				}
				PopulateSubsidiarySheet(_owner.CurrentAccount, StartDate, EndDate);
				UpdateTitle(_owner.CurrentAccount);
			}
			else if (_owner.SwitchToView(ActiveView.Balance))
			{
				Common.SetTreeCheck(_owner.AccountTreeEditor.Tree, CheckEnum.None);
			}
			break;
		}
	}

	private void _grdVoucher_DoubleClick(object sender, EventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[grdVoucher.Row];
		if (row.UserData is Voucher voucher)
		{
			if (voucher.Details.Count == 0)
			{
				PopulateSubsidiarySheet(voucher.Account, StartDate, EndDate);
				UpdateTitle(voucher.Account);
				_owner.CurrentAccount = voucher.Account;
				_owner.CurrentAuxiliary = null;
				_owner.AccountTreeEditor.UpdateNodeStatus(voucher.Account);
			}
			else
			{
				AuxiliaryItem auxiliaryItem = voucher.Details[0];
				PopulateSubsidiarySheet(voucher.Account, StartDate, EndDate, auxiliaryItem);
				UpdateTitle(voucher.Account, auxiliaryItem);
				_owner.CurrentAccount = voucher.Account;
				_owner.CurrentAuxiliary = auxiliaryItem;
				_owner.AccountTreeEditor.UpdateNodeStatus(Tuple.Create(voucher.Account, auxiliaryItem));
			}
		}
	}

	private void _grdVoucher_AfterResizeRow(object sender, RowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			_owner.StyleRecord.RecordHeight(sender as C1FlexGridEx, e.Row);
		}
	}

	private void _grdVoucher_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordWidth(c1FlexGridEx.Name, c1FlexGridEx.Cols[e.Col].Name, c1FlexGridEx.Cols[e.Col].Width);
		}
	}

	private void _grdVoucher_AfterDragColumn(object sender, DragRowColEventArgs e)
	{
		if (!PendingAllEvent)
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordOrder(c1FlexGridEx.Name, from C1.Win.C1FlexGrid.Column t in c1FlexGridEx.Cols
				select t.Name);
		}
	}

	private void CmdSubNoTotal_Click(object sender, ClickEventArgs e)
	{
		SubDisplay = SubDisplayFlags.DataOnly;
		_owner.SelectSub();
	}

	private void CmdSub_YearOnly_Click(object sender, ClickEventArgs e)
	{
		SubDisplay = SubDisplayFlags.YearAndData;
		_owner.SelectSub();
	}

	private void CmdSub_MonthOnly_Click(object sender, ClickEventArgs e)
	{
		SubDisplay = SubDisplayFlags.MonthAndData;
		_owner.SelectSub();
	}

	private void CmdSub_AllTotal_Click(object sender, ClickEventArgs e)
	{
		SubDisplay = SubDisplayFlags.AllSumAndData;
		_owner.SelectSub();
	}

	private void CmdTotal_YearOnly_Click(object sender, ClickEventArgs e)
	{
		TotalDisplay = TotalDisplayFlags.YearOnly;
		_owner.SelectTotal();
	}

	private void CmdTotal_MonthOnly_Click(object sender, ClickEventArgs e)
	{
		TotalDisplay = TotalDisplayFlags.MonthOnly;
		_owner.SelectTotal();
	}

	private void CmdTotal_AllTotal_Click(object sender, ClickEventArgs e)
	{
		TotalDisplay = TotalDisplayFlags.AllSum;
		_owner.SelectTotal();
	}

	private void _owner_AfterOpenLedger(object sender, LedgerEventArgs e)
	{
		lblSubStartDate.Text = _owner.StartDate.ToString("yyyy-MM-dd");
		lblSubEndDate.Text = _owner.EndDate.ToString("yyyy-MM-dd");
		SubDockingTab.TabPages.Clear();
	}

	private void CmdFillToTable_Click(object sender, ClickEventArgs e)
	{
		FillToTable();
	}

	private void Initialize()
	{
		grdSubsidiary.DrawMode = DrawModeEnum.OwnerDraw;
		grdSubsidiary.OwnerDrawCell += GrdSubsidiary_OwnerDrawCell;
		grdSubsidiary.Click += _grid_Click;
		grdSubsidiary.RowColChange += _grid_Click;
		grdSubsidiary.DoubleClick += _grid_DoubleClick;
		grdSubsidiary.BodySelectionChanged += GrdSubsidiary_BodySelectionChanged;
		grdSubsidiary.BeforeMouseDown += GrdSubsidiary_BeforeMouseDown;
		grdSubsidiary.KeyDown += GrdSubsidiary_KeyDown;
		grdSubsidiary.AfterResizeRow += _grid_AfterResizeRow;
		grdSubsidiary.AfterResizeColumn += _grid_AfterResizeColumn;
		grdSubsidiary.AfterDragColumn += _grid_AfterDragColumn;
		btnSubsidiaryBack.Click += btnBack_Click;
		grdVoucher.DoubleClick += _grdVoucher_DoubleClick;
		grdVoucher.AfterResizeRow += _grdVoucher_AfterResizeRow;
		grdVoucher.AfterResizeColumn += _grdVoucher_AfterResizeColumn;
		grdVoucher.AfterDragColumn += _grdVoucher_AfterDragColumn;
		grdVoucher.KeyDown += GrdVoucher_KeyDown;
		SubStatus = BooksStyle.BalanceTo;
		SubDisplay = BooksStyle.SubDisplay;
		TotalDisplay = BooksStyle.TotalDisplay;
		grdVoucher.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdVoucher.DrawFormBorder(e1.Graphics);
		};
	}

	private void GrdVoucher_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyData = e.KeyData;
		if (keyData == Keys.Space)
		{
			CheckCellBox(grdVoucher);
		}
	}

	private void GrdSubsidiary_KeyDown(object sender, KeyEventArgs e)
	{
		Keys keyData = e.KeyData;
		if (keyData == Keys.Space)
		{
			CheckCellBox(grdSubsidiary);
		}
	}

	private void CheckCellBox(C1FlexGridEx grid)
	{
		C1.Win.C1FlexGrid.CellRange selection = grid.Selection;
		int num = -1;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			if (i >= grid.Rows.Fixed && grid.Rows[i].UserData is Voucher)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			C1Command c1Command = new C1Command();
			c1Command.UserData = grid;
			Voucher voucher2 = grid.Rows[num].UserData as Voucher;
			if (voucher2.VoucherMark)
			{
				CancelMarkImpl(c1Command, ClickEventArgs.Empty);
			}
			else
			{
				MakeMarkImpl(c1Command, ClickEventArgs.Empty);
			}
		}
	}

	private void GrdSubsidiary_BeforeMouseDown(object sender, BeforeMouseDownEventArgs e)
	{
		HitTestInfo hitTestInfo = grdSubsidiary.HitTest(e.X, e.Y);
		if (hitTestInfo.Column != grdSubsidiary.Cols["MyMark"].Index || hitTestInfo.Row < grdSubsidiary.Rows.Fixed)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellStyle cellStyleDisplay = grdSubsidiary.GetCellStyleDisplay(hitTestInfo.Row, hitTestInfo.Column);
		Rectangle cellRect = grdSubsidiary.GetCellRect(hitTestInfo.Row, hitTestInfo.Column);
		if (!cellStyleDisplay.GetImageRectangle(cellRect, grdSubsidiary.Glyphs[GlyphEnum.Checked]).Contains(hitTestInfo.Point))
		{
			return;
		}
		e.Cancel = true;
		bool flag = grdSubsidiary.GetCellCheck(hitTestInfo.Row, hitTestInfo.Column) != CheckEnum.Checked;
		CheckEnum check = (flag ? CheckEnum.Checked : CheckEnum.Unchecked);
		grdSubsidiary.BeginUpdate();
		try
		{
			if (grdSubsidiary.Selection.ContainsRow(hitTestInfo.Row))
			{
				if (flag)
				{
					_owner.MakeMarkRows(grdSubsidiary.Selection, grdSubsidiary);
				}
				else
				{
					_owner.MarkCancelRows(grdSubsidiary.Selection, grdSubsidiary);
				}
				C1.Win.C1FlexGrid.CellRange selection = grdSubsidiary.Selection;
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					grdSubsidiary.SetCellCheck(i, hitTestInfo.Column, check);
				}
			}
			else
			{
				if (flag)
				{
					_owner.MakeMarkRow(hitTestInfo.Row, grdSubsidiary);
				}
				else
				{
					_owner.MarkCancelRow(hitTestInfo.Row, grdSubsidiary);
				}
				grdSubsidiary.SetCellCheck(hitTestInfo.Row, hitTestInfo.Column, check);
			}
		}
		finally
		{
			grdSubsidiary.EndUpdate();
		}
		RefreshVouchersGridBackground();
	}

	private void GrdSubsidiary_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col == grdSubsidiary.Cols["MyMark"].Index && grdSubsidiary.Rows[e.Row].UserData is string)
		{
			e.Handled = true;
			e.DrawCell(DrawCellFlags.Background | DrawCellFlags.Border);
		}
	}

	private void MakeMarkImpl(object sender, ClickEventArgs e)
	{
		if (!(sender is C1Command { UserData: C1FlexGridEx userData }))
		{
			return;
		}
		userData.BeginUpdate();
		try
		{
			_owner.MakeMark_Click(sender, e);
			if (userData == grdSubsidiary)
			{
				int index = userData.Cols["MyMark"].Index;
				C1.Win.C1FlexGrid.CellRange selection = userData.Selection;
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					C1.Win.C1FlexGrid.Row row = userData.Rows[i];
					if (row.UserData is Voucher voucher)
					{
						userData.SetCellCheck(i, index, voucher.VoucherMark ? CheckEnum.Checked : CheckEnum.Unchecked);
					}
				}
				RefreshVouchersGridBackground();
			}
			else if (userData == grdVoucher)
			{
				RefreshSubsidiaryGridBackground();
			}
		}
		finally
		{
			userData.EndUpdate();
		}
	}

	private void CancelMarkImpl(object sender, ClickEventArgs e)
	{
		if (!(sender is C1Command { UserData: C1FlexGridEx userData }))
		{
			return;
		}
		userData.BeginUpdate();
		try
		{
			_owner.MarkCancel_Click(sender, e);
			if (userData == grdSubsidiary)
			{
				int index = userData.Cols["MyMark"].Index;
				C1.Win.C1FlexGrid.CellRange selection = userData.Selection;
				for (int i = selection.TopRow; i <= selection.BottomRow; i++)
				{
					C1.Win.C1FlexGrid.Row row = userData.Rows[i];
					if (row.UserData is Voucher voucher)
					{
						userData.SetCellCheck(i, index, voucher.VoucherMark ? CheckEnum.Checked : CheckEnum.Unchecked);
					}
				}
				RefreshVouchersGridBackground();
			}
			else if (userData == grdVoucher)
			{
				RefreshSubsidiaryGridBackground();
			}
		}
		finally
		{
			userData.EndUpdate();
		}
	}

	private void RefreshSubsidiaryGridBackground()
	{
		grdSubsidiary.BeginUpdate();
		try
		{
			int count = grdSubsidiary.Rows.Count;
			int index = grdSubsidiary.Cols["MyMark"].Index;
			for (int i = grdSubsidiary.Rows.Fixed; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdSubsidiary.Rows[i];
				if (!(row.UserData is Voucher voucher))
				{
					continue;
				}
				bool flag = grdSubsidiary.GetCellCheck(i, index) == CheckEnum.Checked;
				if (voucher.VoucherMark != flag)
				{
					grdSubsidiary.SetCellCheck(i, index, voucher.VoucherMark ? CheckEnum.Checked : CheckEnum.Unchecked);
					if (voucher.VoucherMark)
					{
						row.StyleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
						row.StyleNew.ForeColor = Common.MarkForeColor;
					}
					else
					{
						row.StyleNew.BackColor = Color.White;
						row.StyleNew.ForeColor = grdVoucher.ForeColor;
					}
				}
			}
		}
		finally
		{
			grdSubsidiary.EndUpdate();
		}
	}

	private void RefreshVouchersGridBackground()
	{
		if (!_voucherVisible)
		{
			return;
		}
		grdVoucher.BeginUpdate();
		try
		{
			int count = grdVoucher.Rows.Count;
			for (int i = grdVoucher.Rows.Fixed; i < count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[i];
				if (row.UserData is Voucher voucher)
				{
					if (voucher.VoucherMark)
					{
						row.StyleNew.BackColor = UserSet.Config.TableStyle.CheckFailColor;
						row.StyleNew.ForeColor = Common.MarkForeColor;
					}
					else
					{
						row.StyleNew.BackColor = Color.White;
						row.StyleNew.ForeColor = grdVoucher.ForeColor;
					}
				}
			}
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void GrdSubsidiary_BodySelectionChanged(object sender, EventArgs e)
	{
		int row = grdSubsidiary.Row;
		if (row < grdSubsidiary.Rows.Fixed)
		{
			cmdSidebarDirectionChange.Visible = false;
			cmdSidebarMarkVoucher.Visible = false;
			cmdSidebarModifyBegin.Visible = false;
			cmdSidebarModifyVoucher.Visible = false;
			return;
		}
		C1.Win.C1FlexGrid.Row row2 = grdSubsidiary.Rows[row];
		if (row2.UserData is Voucher voucher)
		{
			cmdSidebarDirectionChange.Visible = true;
			cmdSidebarMarkVoucher.Visible = true;
			cmdSidebarModifyBegin.Visible = false;
			cmdSidebarModifyVoucher.Visible = true;
			cmdSidebarDirectionChange.Text = (voucher.DirectionToggled ? "方向还原" : "方向调整");
			cmdSidebarMarkVoucher.Text = (voucher.VoucherMark ? "取消关注" : "标记关注");
		}
		else if (row2.UserData?.ToString() == "BeginBalance")
		{
			cmdSidebarDirectionChange.Visible = false;
			cmdSidebarMarkVoucher.Visible = false;
			cmdSidebarModifyBegin.Visible = true;
			cmdSidebarModifyVoucher.Visible = false;
		}
		else
		{
			cmdSidebarDirectionChange.Visible = false;
			cmdSidebarMarkVoucher.Visible = false;
			cmdSidebarModifyBegin.Visible = false;
			cmdSidebarModifyVoucher.Visible = false;
		}
	}

	private void BindSubContexMenu()
	{
		try
		{
			cmdDes.Text = "降序排列";
			cmdDes.Image = ContextResources.ctxDescending;
			cmdDes.Click += CmdDesSort_Click;
			cmdDes.CommandStateQuery += CmdDesSort_CommandStateQuery;
			lnkDes.Command = cmdDes;
			cmdAsc.Text = "升序排列";
			cmdAsc.Image = ContextResources.ctxAscending;
			cmdAsc.Click += CmdAscSort_Click;
			cmdAsc.CommandStateQuery += CmdAscSort_CommandStateQuery;
			lnkAsc.Command = cmdAsc;
			cmdNormal.Text = "取消排序";
			cmdNormal.Click += CmdNoneSort_Click;
			cmdNormal.CommandStateQuery += CmdNoneSort_CommandStateQuery;
			lnkNormal.Command = cmdNormal;
			ctxSubFixed.HideFirstDelimiter = true;
			ctxSubFixed.CommandLinks.Add(lnkDes);
			ctxSubFixed.CommandLinks.Add(lnkAsc);
			ctxSubFixed.CommandLinks.Add(lnkNormal);
			cmdColHide.Text = "隐藏本列";
			cmdColHide.UserData = grdSubsidiary;
			cmdColHide.Click += _owner.ColHide_Click;
			lnkColHide.Command = cmdColHide;
			lnkColHide.Delimiter = true;
			ctxSubFixed.CommandLinks.Add(lnkColHide);
			cmdCancelHide.Text = "取消隐藏";
			cmdCancelHide.UserData = grdSubsidiary;
			cmdCancelHide.Click += CmdCancelHide_Click;
			lnkCancelHide.Command = cmdCancelHide;
			ctxSubFixed.CommandLinks.Add(lnkCancelHide);
			cmdCopy.Text = "复制";
			cmdCopy.Image = ContextResources.ctxCopy;
			cmdCopy.Click += delegate
			{
				Common.SetSelectionToClipboard(grdSubsidiary);
			};
			lnkCopy.Command = cmdCopy;
			ctxSubCell.CommandLinks.Add(lnkCopy);
			ctxSubCell.CommandLinks.Add(grdSubsidiary.FilterManager.GenLnkFilter());
			ctxSubCell.CommandLinks.Add(grdSubsidiary.FilterManager.GenLnkSample());
			ctxSubCell.CommandLinks.Add(grdSubsidiary.FilterManager.GenLnkSelect());
			ctxSubCell.CommandLinks.Add(grdSubsidiary.FilterManager.GenLnkCancelCurrentColumn());
			cmdDirectionChange.Text = "方向调整";
			cmdDirectionChange.UserData = grdSubsidiary;
			cmdDirectionChange.Image = ContextResources.ctxDirectionChange;
			cmdDirectionChange.Click += _owner.DirectionChange_Click;
			lnkDirectionChange.Command = cmdDirectionChange;
			lnkDirectionChange.Delimiter = true;
			ctxSubCell.CommandLinks.Add(lnkDirectionChange);
			cmdDirectionReduce.Text = "方向还原";
			cmdDirectionReduce.UserData = grdSubsidiary;
			cmdDirectionReduce.Click += _owner.DirectionReduce_Click;
			lnkDirectionReduce.Command = cmdDirectionReduce;
			ctxSubCell.CommandLinks.Add(lnkDirectionReduce);
			cmdMakeMark.Text = "标记关注";
			cmdMakeMark.UserData = grdSubsidiary;
			cmdMakeMark.Image = ContextResources.ctxMakeMark;
			cmdMakeMark.Click += MakeMarkImpl;
			lnkMakeMark.Command = cmdMakeMark;
			ctxSubCell.CommandLinks.Add(lnkMakeMark);
			cmdCancelMark.Text = "取消关注";
			cmdCancelMark.UserData = grdSubsidiary;
			cmdCancelMark.Click += CancelMarkImpl;
			lnkCancelMark.Command = cmdCancelMark;
			ctxSubCell.CommandLinks.Add(lnkCancelMark);
			cmdTotal_AllTotal.Text = "显示合计累计";
			cmdTotal_AllTotal.Click += CmdTotal_AllTotal_Click;
			lnkTotal_AllTotal.Command = cmdTotal_AllTotal;
			lnkTotal_AllTotal.Delimiter = true;
			ctxSubCell.CommandLinks.Add(lnkTotal_AllTotal);
			cmdTotal_MonthOnly.Text = "仅显示合计";
			cmdTotal_MonthOnly.Click += CmdTotal_MonthOnly_Click;
			lnkTotal_MonthOnly.Command = cmdTotal_MonthOnly;
			ctxSubCell.CommandLinks.Add(lnkTotal_MonthOnly);
			cmdTotal_YearOnly.Text = "仅显示累计";
			cmdTotal_YearOnly.Click += CmdTotal_YearOnly_Click;
			lnkTotal_YearOnly.Command = cmdTotal_YearOnly;
			ctxSubCell.CommandLinks.Add(lnkTotal_YearOnly);
			cmdSub_AllTotal.Text = "显示合计累计";
			cmdSub_AllTotal.Click += CmdSub_AllTotal_Click;
			lnkSub_AllTotal.Command = cmdSub_AllTotal;
			lnkSub_AllTotal.Delimiter = true;
			ctxSubCell.CommandLinks.Add(lnkSub_AllTotal);
			cmdSub_MonthOnly.Text = "仅显示合计";
			cmdSub_MonthOnly.Click += CmdSub_MonthOnly_Click;
			lnkSub_MonthOnly.Command = cmdSub_MonthOnly;
			ctxSubCell.CommandLinks.Add(lnkSub_MonthOnly);
			cmdSub_YearOnly.Text = "仅显示累计";
			cmdSub_YearOnly.Click += CmdSub_YearOnly_Click;
			lnkSub_YearOnly.Command = cmdSub_YearOnly;
			ctxSubCell.CommandLinks.Add(lnkSub_YearOnly);
			cmdSubNoTotal.Text = "不显示合计累计";
			cmdSubNoTotal.Click += CmdSubNoTotal_Click;
			lnkSub_NoTotal.Command = cmdSubNoTotal;
			ctxSubCell.CommandLinks.Add(lnkSub_NoTotal);
			cmdModifyBalance.Text = "修改期初数";
			cmdModifyBalance.Image = ContextResources.modifyLedger;
			cmdModifyBalance.CommandStateQuery += CmdModifyBalance_CommandStateQuery;
			cmdModifyBalance.Click += CmdModifyBalance_Click;
			lnkModifyBalance.Command = cmdModifyBalance;
			lnkModifyBalance.Delimiter = true;
			ctxSubCell.CommandLinks.Add(lnkModifyBalance);
			C1CommandLink c1CommandLink = new C1CommandLink();
			C1Command c1Command = new C1Command();
			c1Command.Text = "修改凭证";
			c1Command.Image = ContextResources.modifyLedger;
			c1Command.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				int row = grdSubsidiary.Row;
				e1.Visible = row >= grdSubsidiary.Rows.Fixed && row < grdSubsidiary.Rows.Count && grdSubsidiary.Rows[row].UserData is Voucher;
			};
			c1Command.Click += async delegate
			{
				if (grdSubsidiary.Row > 0 && grdSubsidiary.Rows[grdSubsidiary.Row].UserData is Voucher voucher2)
				{
					await _owner.ModifyVoucher(voucher2);
				}
			};
			c1CommandLink.Command = c1Command;
			c1CommandLink.Delimiter = true;
			ctxSubCell.CommandLinks.Add(c1CommandLink);
			cmdFillToTable.Text = "填充至底稿";
			cmdFillToTable.Image = ContextResources.ctxFillToTable;
			cmdFillToTable.Click += CmdFillToTable_Click;
			cmdFillToTable.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				e1.Visible = LedgerViewer.IsAuditPlatform;
			};
			lnkFillToTable.Command = cmdFillToTable;
			lnkFillToTable.Delimiter = true;
			ctxSubCell.CommandLinks.Add(lnkFillToTable);
			ctxSubCell.Popup += delegate
			{
				bool visible = cmdFillToTable.Visible;
				ctxSubCell.ShowAll();
				cmdFillToTable.Visible = visible;
				List<int> skipRows = new List<int>();
				grdSubsidiary.SkipRows = skipRows;
				if (grdSubsidiary.MouseRow >= 0 && grdSubsidiary.MouseRow < grdSubsidiary.Rows.Fixed)
				{
					ctxSubCell.OnlyShow(lnkColHide, lnkCancelHide);
				}
				else
				{
					ctxSubCell.HideLinks(lnkColHide, lnkCancelHide);
					if (SubStatus == SubOrTotal.Total)
					{
						ctxSubCell.HideLinks(lnkSub_AllTotal, lnkSub_MonthOnly, lnkSub_YearOnly, lnkSub_NoTotal);
						ctxSubCell.HideLinks(lnkDirectionChange, lnkDirectionReduce, lnkMakeMark, lnkCancelMark, lnkColHide, lnkCancelHide);
					}
					else
					{
						ctxSubCell.HideLinks(lnkTotal_AllTotal, lnkTotal_MonthOnly, lnkTotal_YearOnly);
					}
					if (grdSubsidiary.MouseRow >= 0)
					{
						object userData = grdSubsidiary.Rows[grdSubsidiary.MouseRow].UserData;
						bool flag = grdSubsidiary.Selection.r2 - grdSubsidiary.Selection.r1 == 0;
						if (grdSubsidiary.Row >= grdSubsidiary.Rows.Fixed && flag && userData is Voucher voucher)
						{
							if (voucher.DirectionToggled)
							{
								ctxSubCell.HideLinks(lnkDirectionChange);
							}
							else
							{
								ctxSubCell.HideLinks(lnkDirectionReduce);
							}
							if (voucher.VoucherMark)
							{
								ctxSubCell.HideLinks(lnkMakeMark);
							}
							else
							{
								ctxSubCell.HideLinks(lnkCancelMark);
							}
						}
						if (grdSubsidiary.MouseRow >= 0 && "BeginBalance".Equals(userData))
						{
							ctxSubCell.HideLinks(lnkDirectionChange, lnkDirectionReduce);
						}
						if ((grdSubsidiary.MouseRow >= 0 && "MonthTotal".Equals(userData)) || "YearTotal".Equals(userData))
						{
							ctxSubCell.HideLinks(lnkDirectionChange, lnkDirectionReduce);
						}
					}
				}
			};
			ctxSubEmpty.CommandLinks.Add(grdSubsidiary.FilterManager.GenLnkCancelAll());
			grdSubsidiary.MouseClick += GrdSubsidiary_MouseClick;
		}
		catch
		{
		}
	}

	private void CmdCancelHide_Click(object sender, ClickEventArgs e)
	{
		grdSubsidiary.BeginUpdate();
		try
		{
			_owner.CancelHide_Click(sender, e);
			if (SubStatus == SubOrTotal.Total)
			{
				grdSubsidiary.Cols["Date"].Visible = false;
				grdSubsidiary.Cols["Type"].Visible = false;
				grdSubsidiary.Cols["Number"].Visible = false;
				grdSubsidiary.Cols["Opposite"].Visible = false;
			}
		}
		finally
		{
			grdSubsidiary.EndUpdate();
		}
	}

	private void CmdNoneSort_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdSubsidiary.Col;
		e.Visible = col >= 0 && (col == grdSubsidiary.Cols["Debit"].Index || col == grdSubsidiary.Cols["Credit"].Index || col == grdSubsidiary.Cols["Balance"].Index);
	}

	private void CmdNoneSort_Click(object sender, ClickEventArgs e)
	{
		SortColumn(SortFlags.None, grdSubsidiary.Col);
	}

	private void CmdAscSort_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdSubsidiary.Col;
		e.Visible = col >= 0 && (col == grdSubsidiary.Cols["Debit"].Index || col == grdSubsidiary.Cols["Credit"].Index || col == grdSubsidiary.Cols["Balance"].Index);
	}

	private void CmdAscSort_Click(object sender, ClickEventArgs e)
	{
		SortColumn(SortFlags.Ascending, grdSubsidiary.Col);
	}

	private void CmdDesSort_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdSubsidiary.Col;
		e.Visible = col >= 0 && (col == grdSubsidiary.Cols["Debit"].Index || col == grdSubsidiary.Cols["Credit"].Index || col == grdSubsidiary.Cols["Balance"].Index);
	}

	private void CmdDesSort_Click(object sender, ClickEventArgs e)
	{
		SortColumn(SortFlags.Descending, grdSubsidiary.Col);
	}

	private void GrdSubsidiary_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdSubsidiary.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxSubFixed.ShowContextMenu(grdSubsidiary, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxSubEmpty.ShowContextMenu(grdSubsidiary, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxSubCell.ShowContextMenu(grdSubsidiary, e.Location);
				break;
			}
		}
	}

	private void CmdModifyBalance_Click(object sender, ClickEventArgs e)
	{
		_owner.ModifyBeginBalance(_owner.CurrentAccount);
	}

	private void CmdModifyBalance_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int mouseRow = grdSubsidiary.MouseRow;
		e.Visible = mouseRow >= grdSubsidiary.Rows.Fixed && grdSubsidiary.Rows[mouseRow].UserData?.ToString() == "BeginBalance";
	}

	private void BindVoucherContexMenu()
	{
		try
		{
			cmdCopy2.Text = "复制";
			cmdCopy2.Image = ContextResources.ctxCopy;
			cmdCopy2.Click += delegate
			{
				Common.SetSelectionToClipboard(grdVoucher);
			};
			lnkCopy2.Command = cmdCopy2;
			ctxVouCell.CommandLinks.Add(lnkCopy2);
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkFilter());
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkSample());
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkSelect());
			ctxVouCell.CommandLinks.Add(grdVoucher.FilterManager.GenLnkCancelCurrentColumn());
			cmdDirectionChange2.Text = "方向调整";
			cmdDirectionChange2.UserData = grdVoucher;
			cmdDirectionChange2.Image = ContextResources.ctxDirectionChange;
			cmdDirectionChange2.Click += _owner.DirectionChange_Click;
			lnkDirectionChange2.Command = cmdDirectionChange2;
			lnkDirectionChange2.Delimiter = true;
			ctxVouCell.CommandLinks.Add(lnkDirectionChange2);
			cmdDirectionReduce2.Text = "方向还原";
			cmdDirectionReduce2.UserData = grdVoucher;
			cmdDirectionReduce2.Click += _owner.DirectionReduce_Click;
			lnkDirectionReduce2.Command = cmdDirectionReduce2;
			ctxVouCell.CommandLinks.Add(lnkDirectionReduce2);
			cmdMakeMark2.Text = "标记关注";
			cmdMakeMark2.UserData = grdVoucher;
			cmdMakeMark2.Image = ContextResources.ctxMakeMark;
			cmdMakeMark2.Click += MakeMarkImpl;
			lnkMakeMark2.Command = cmdMakeMark2;
			ctxVouCell.CommandLinks.Add(lnkMakeMark2);
			cmdCancelMark2.Text = "取消关注";
			cmdCancelMark2.UserData = grdVoucher;
			cmdCancelMark2.Click += CancelMarkImpl;
			lnkCancelMark2.Command = cmdCancelMark2;
			ctxVouCell.CommandLinks.Add(lnkCancelMark2);
			C1CommandLink c1CommandLink = new C1CommandLink();
			C1Command c1Command = new C1Command();
			c1Command.Text = "修改凭证";
			c1Command.Image = ContextResources.modifyLedger;
			c1Command.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
			{
				int row = grdVoucher.Row;
				e1.Visible = row >= grdVoucher.Rows.Fixed && row < grdVoucher.Rows.Count && grdVoucher.Rows[row].UserData is Voucher;
			};
			c1Command.Click += async delegate
			{
				await _owner.ModifyVoucher(grdVoucher.Rows[grdVoucher.Row].UserData as Voucher);
			};
			c1CommandLink.Command = c1Command;
			c1CommandLink.Delimiter = true;
			ctxVouCell.CommandLinks.Add(c1CommandLink);
			ctxVouCell.Popup += delegate
			{
				ctxVouCell.ShowAll();
				if (grdVoucher.MouseRow >= 0 && grdVoucher.MouseRow < grdVoucher.Rows.Fixed)
				{
					ctxVouCell.OnlyShow(lnkColHide2, lnkCancelHide2);
				}
				else
				{
					ctxVouCell.HideLinks(lnkColHide2, lnkCancelHide2);
					bool flag = grdVoucher.Selection.r2 - grdVoucher.Selection.r1 == 0;
					if (grdVoucher.Row >= grdVoucher.Rows.Fixed && flag && grdVoucher.Rows[grdVoucher.Row].UserData is Voucher voucher)
					{
						if (voucher.DirectionToggled)
						{
							ctxVouCell.HideLinks(lnkDirectionChange2);
						}
						else
						{
							ctxVouCell.HideLinks(lnkDirectionReduce2);
						}
						if (voucher.VoucherMark)
						{
							ctxVouCell.HideLinks(lnkMakeMark2);
						}
						else
						{
							ctxVouCell.HideLinks(lnkCancelMark2);
						}
					}
				}
			};
			ctxVouEmpty.CommandLinks.Add(grdVoucher.FilterManager.GenLnkCancelAll());
			ctxVouFixed.HideFirstDelimiter = true;
			cmdColHide2.Text = "隐藏本列";
			cmdColHide2.UserData = grdVoucher;
			cmdColHide2.Click += _owner.ColHide_Click;
			lnkColHide2.Command = cmdColHide2;
			ctxVouFixed.CommandLinks.Add(lnkColHide2);
			cmdCancelHide2.Text = "取消隐藏";
			cmdCancelHide2.UserData = grdVoucher;
			cmdCancelHide2.Click += _owner.CancelHide_Click;
			lnkCancelHide2.Command = cmdCancelHide2;
			ctxVouFixed.CommandLinks.Add(lnkCancelHide2);
			grdVoucher.MouseClick += GrdVoucher_MouseClick;
		}
		catch
		{
		}
	}

	private void GrdVoucher_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdVoucher.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxVouFixed.ShowContextMenu(grdVoucher, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxVouEmpty.ShowContextMenu(grdVoucher, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxVouCell.ShowContextMenu(grdVoucher, e.Location);
				break;
			}
		}
	}

	private void SetVoucherHeader(Voucher voucher)
	{
		if (voucher != null)
		{
			lblVoucherType.Text = $"字：{voucher.Type}";
			lblVoucherNumber.Text = "号： " + voucher.Number;
			lblVoucherDate.Text = "制单日期：" + voucher.Day.ToString("yyyy-MM-dd");
			lblNumAttachments.Text = $"附件张数：{voucher.NumAttachments} ";
			lblMaker.Text = "制单人：" + voucher.Maker;
			lblBooker.Text = "记账人：" + voucher.Booker;
			lblChecker.Text = "审核人：" + voucher.Checker;
		}
	}

	public void FillToTable()
	{
		if (_owner.CurrentAccount == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前科目为空！");
			return;
		}
		List<object> list = new List<object>();
		for (int i = grdSubsidiary.Rows.Fixed; i < grdSubsidiary.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = grdSubsidiary.Rows[i];
			if (row.Visible && row.UserData is Voucher { VoucherMark: not false } voucher)
			{
				list.Add(voucher);
			}
		}
		_owner._owner.OnAfterCollect(new LedgerCollectEventArgs
		{
			Viewer = _owner,
			Account = _owner.CurrentAccount,
			Auxiliary = null,
			CollectObject = ((SubStatus == SubOrTotal.Subsidiary) ? CollectObjectEnum.Subsidiary : CollectObjectEnum.Summary),
			StartTime = StartDate,
			EndTime = EndDate,
			Source = list,
			IsShowSubsidiaryAllAccountTypeTable = (SubStatus == SubOrTotal.Subsidiary)
		});
	}

	private void PopulateBottomVoucher()
	{
		if (grdSubsidiary.Rows.Count <= grdSubsidiary.Rows.Fixed)
		{
			ShowVoucher(visible: false);
			return;
		}
		C1.Win.C1FlexGrid.Row row = grdSubsidiary.Rows[grdSubsidiary.Rows.Fixed];
		object userData = row.UserData;
		Voucher voucher = userData as Voucher;
		if (voucher != null)
		{
			ShowVoucher(visible: true);
			PopulateVouchers(Ledger.Vouchers.Where((Voucher v) => v.Type == voucher.Type && v.Number == voucher.Number && v.Day.Year == voucher.Day.Year && v.Day.Month == voucher.Day.Month));
		}
		else
		{
			ShowVoucher(visible: false);
		}
	}

	public void SetTheme()
	{
		btnSubsidiaryBack.BackColor = Color.Transparent;
		btnSubsidiaryBack.FlatStyle = FlatStyle.Flat;
		btnSubsidiaryBack.FlatAppearance.BorderSize = 0;
		btnSubsidiaryBack.FlatAppearance.MouseOverBackColor = Color.LightGray;
		btnCloseVoucher.BackColor = Color.Transparent;
		btnCloseVoucher.FlatStyle = FlatStyle.Flat;
		btnCloseVoucher.FlatAppearance.BorderSize = 0;
		btnCloseVoucher.FlatAppearance.MouseDownBackColor = Color.LightGray;
		grdSubsidiary.Styles.Fixed.Border.Color = Color.DarkGray;
		grdVoucher.Styles.Fixed.Border.Color = Color.DarkGray;
		if (Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
	}

	public void AttachTooltip(TooltipManager tooltipManager)
	{
		attachTooltip(grdVoucher, TipInfo.Parse(TipResource.会计凭证区域_指明细账的下方));
		TipInfo subsidiary = TipInfo.Parse(TipResource.明细账区域);
		TipInfo total = TipInfo.Parse(TipResource.总账区域);
		grdSubsidiary.MouseMove += delegate(object s1, MouseEventArgs e1)
		{
			if (tooltipManager.ShouldDisplay)
			{
				TipInfo tipInfo = ((SubStatus == SubOrTotal.Subsidiary) ? subsidiary : total);
				tooltipManager.Show(tipInfo, grdSubsidiary, e1.X, e1.Y);
			}
		};
		grdSubsidiary.MouseLeave += delegate
		{
			tooltipManager.Hide();
		};
		void attachTooltip(Component component, TipInfo text)
		{
			Control control = component as Control;
			if (control != null)
			{
				control.MouseMove += delegate(object s1, MouseEventArgs e1)
				{
					if (tooltipManager.ShouldDisplay)
					{
						tooltipManager.Show(text, control, e1.X, e1.Y);
					}
				};
				control.MouseLeave += delegate
				{
					tooltipManager.Hide();
				};
			}
			tooltipManager.Attach(component, text);
		}
	}
}
