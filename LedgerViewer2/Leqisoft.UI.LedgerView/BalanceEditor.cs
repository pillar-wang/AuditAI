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
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.CollectTable;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

internal class BalanceEditor : ISetTheme
{
	private enum BalanceDisplayStyleEnum
	{
		HAVE_DIRECTION = 1,
		NONE_DIRECTION
	}

	private LedgerViewer _owner;

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1ContextMenu ctxFixed = new C1ContextMenu();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private C1CommandLink lnkCopy = new C1CommandLink();

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkLastLayer = new C1CommandLink();

	private C1Command cmdLastLayer = new C1Command();

	private C1CommandLink lnkModifyBeginBalance = new C1CommandLink();

	private C1Command cmdModifyBeginBalance = new C1Command();

	private C1CommandLink lnkFillToTable = new C1CommandLink();

	private C1Command cmdFillToTable = new C1Command();

	private C1CommandLink lnkSwitchDisplayStyle = new C1CommandLink();

	private C1Command cmdSwitchDisplayStyle = new C1Command();

	private C1ContextMenu modifyContextMenu = new C1ContextMenu();

	private C1CommandLink lnkModifyCompanyName = new C1CommandLink();

	private C1Command cmdModifyCompanyName = new C1Command();

	private C1CommandLink lnkModifyStartDate = new C1CommandLink();

	private C1Command cmdModifyStartDate = new C1Command();

	private DateTime _startDate;

	private DateTime _endDate;

	private C1SplitterPanel pnlBalanceTitle;

	private C1SplitterPanel pnlBalanceHead;

	private C1SplitterPanel pnlBalanceGrid;

	private C1Button btnBalanceBack;

	private C1Label lblAccountName;

	private C1Label lblCompanyName;

	private C1DateEdit dteStart;

	private C1Label lblPeriod;

	private C1DateEdit dteEnd;

	private C1Label lblCurrency;

	public C1FlexGridEx grdBalance;

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private C1ToolBar toolBar = new C1ToolBar
	{
		Name = "BalanceEditor+toolBar"
	};

	public C1SplitterPanel pnlSidebar;

	private C1CommandLink lnkSidebarModifyBegin = new C1CommandLink();

	private C1Command cmdSidebarModifyBegin = new C1Command();

	private C1Command cmdFillToTableOnToolbar;

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	private TrialBalanceSheet _balanceSheet;

	private bool initializedCaption;

	private bool onlyLastLevelDisplay;

	private int currentDisplayLevel;

	private C1CommandLink lnkAsc = new C1CommandLink();

	private C1Command cmdAsc = new C1Command();

	private C1CommandLink lnkDes = new C1CommandLink();

	private C1Command cmdDes = new C1Command();

	private C1CommandLink lnkNormal = new C1CommandLink();

	private C1Command cmdNormal = new C1Command();

	private BalanceDisplayStyleEnum _currentDisplayStyle = BalanceDisplayStyleEnum.HAVE_DIRECTION;

	private const string TAG_TOTAL_ROW = "tag_total_row";

	public DateTime StartDate
	{
		get
		{
			return _startDate;
		}
		set
		{
			_startDate = value;
			dteStart.ValueChanged -= DatStart_ValueChanged;
			dteStart.Value = value;
			dteStart.ValueChanged += DatStart_ValueChanged;
		}
	}

	public DateTime EndDate
	{
		get
		{
			return _endDate;
		}
		set
		{
			_endDate = value;
			dteEnd.ValueChanged -= DatEnd_ValueChanged;
			dteEnd.Value = value;
			dteEnd.ValueChanged += DatEnd_ValueChanged;
		}
	}

	public bool ShouldShowAuxiliaryNode { get; private set; }

	private Ledger Ledger => _owner.Ledger;

	public bool PendingAllEvent { get; set; }

	public C1SplitContainer View { get; private set; }

	public event EventHandler<Tuple<DateTime, DateTime>> AccountPeriodChanged;

	public void OnAccountPeriodChanged(object sender, Tuple<DateTime, DateTime> e)
	{
		this.AccountPeriodChanged?.Invoke(this, e);
	}

	public BalanceEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitializeComponent();
		BindContextMenu();
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

	private void InitializeComponent()
	{
		View = new C1SplitContainer();
		pnlBalanceTitle = new C1SplitterPanel();
		pnlBalanceHead = new C1SplitterPanel();
		pnlBalanceGrid = new C1SplitterPanel();
		btnBalanceBack = new C1Button();
		lblCompanyName = new C1Label();
		lblCompanyName.MouseClick += LblCompanyName_MouseClick;
		lblAccountName = new C1Label();
		lblCurrency = new C1Label();
		dteStart = new C1DateEdit();
		lblPeriod = new C1Label();
		dteEnd = new C1DateEdit();
		grdBalance = new C1FlexGridEx();
		grdBalance.Name = "grdBalance";
		btnBalanceBack.Location = new Point(0, -3);
		btnBalanceBack.Size = new Size(30, 30);
		btnBalanceBack.Image = Leqisoft.UI.LedgerView.Properties.Resources.back;
		btnBalanceBack.FlatStyle = FlatStyle.Flat;
		btnBalanceBack.FlatAppearance.BorderSize = 0;
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblAccountName.TextDetached = true;
		lblAccountName.BorderStyle = BorderStyle.None;
		lblAccountName.Dock = DockStyle.Fill;
		lblAccountName.Font = font;
		lblAccountName.Text = "科目余额表";
		lblAccountName.TextAlign = ContentAlignment.MiddleCenter;
		pnlBalanceTitle.BackColor = Color.WhiteSmoke;
		pnlBalanceTitle.HeaderLineColor = Color.Transparent;
		pnlBalanceTitle.Height = 30;
		pnlBalanceTitle.KeepRelativeSize = false;
		pnlBalanceTitle.Location = new Point(0, 0);
		pnlBalanceTitle.MinHeight = 30;
		pnlBalanceTitle.Resizable = false;
		pnlBalanceTitle.Size = new Size(927, 30);
		pnlBalanceTitle.SizeRatio = 4.815;
		pnlBalanceTitle.Controls.Add(btnBalanceBack);
		pnlBalanceTitle.Controls.Add(lblAccountName);
		Font font2 = new Font("Microsoft YaHei", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblCurrency.TextDetached = true;
		lblCurrency.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
		lblCurrency.BorderStyle = BorderStyle.None;
		lblCurrency.Font = font2;
		lblCurrency.Location = new Point(727, 6);
		lblCurrency.Size = new Size(180, 17);
		lblCurrency.Text = "金额单位：";
		lblCurrency.TextAlign = ContentAlignment.MiddleRight;
		lblCompanyName.TextDetached = true;
		lblCompanyName.BorderStyle = BorderStyle.None;
		lblCompanyName.Font = font2;
		lblCompanyName.Location = new Point(3, 3);
		lblCompanyName.Size = new Size(343, 19);
		lblCompanyName.Text = "核算单位：";
		lblCompanyName.TextAlign = ContentAlignment.MiddleLeft;
		dteStart.AllowSpinLoop = false;
		dteStart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
		dteStart.Calendar.DayNameLength = 1;
		dteStart.CustomFormat = "yyyy-MM-dd";
		dteStart.FormatType = FormatTypeEnum.CustomFormat;
		dteStart.ImagePadding = new Padding(0);
		dteStart.Location = new Point(377, 2);
		dteStart.Size = new Size(75, 21);
		dteStart.VisibleButtons = DropDownControlButtonFlags.None;
		dteStart.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
		dteStart.ValueChanged += DatStart_ValueChanged;
		dteStart.DoubleClick += DatStart_DoubleClick;
		dteStart.KeyDown += datStart_KeyDown;
		dteEnd.AllowSpinLoop = false;
		dteEnd.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
		dteEnd.Calendar.DayNameLength = 1;
		dteEnd.CustomFormat = "yyyy-MM-dd";
		dteEnd.FormatType = FormatTypeEnum.CustomFormat;
		dteEnd.ImagePadding = new Padding(0);
		dteEnd.Location = new Point(474, 2);
		dteEnd.Size = new Size(74, 21);
		dteEnd.VisibleButtons = DropDownControlButtonFlags.None;
		dteEnd.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
		dteEnd.ValueChanged += DatEnd_ValueChanged;
		dteEnd.DoubleClick += DatEnd_DoubleClick;
		dteEnd.KeyDown += datEnd_KeyDown;
		lblPeriod.TextDetached = true;
		lblPeriod.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
		lblPeriod.AutoSize = true;
		lblPeriod.Font = font2;
		lblPeriod.BorderStyle = BorderStyle.None;
		lblPeriod.Location = new Point(453, 6);
		lblPeriod.Size = new Size(20, 17);
		lblPeriod.Text = "至";
		pnlBalanceHead.BackColor = Color.WhiteSmoke;
		pnlBalanceHead.HeaderLineColor = Color.Transparent;
		pnlBalanceHead.Height = 25;
		pnlBalanceHead.KeepRelativeSize = false;
		pnlBalanceHead.Location = new Point(0, 31);
		pnlBalanceHead.MinHeight = 25;
		pnlBalanceHead.Resizable = false;
		pnlBalanceHead.Size = new Size(927, 25);
		pnlBalanceHead.SizeRatio = 4.181;
		pnlBalanceHead.Controls.Add(lblCurrency);
		pnlBalanceHead.Controls.Add(lblCompanyName);
		pnlBalanceHead.Controls.Add(dteEnd);
		pnlBalanceHead.Controls.Add(dteStart);
		pnlBalanceHead.Controls.Add(lblPeriod);
		pnlBalanceHead.Paint += delegate(object s1, PaintEventArgs e1)
		{
			e1.Graphics.DrawLine(panelBorderPen, 0, pnlBalanceHead.Height - 1, pnlBalanceHead.Width, pnlBalanceHead.Height - 1);
		};
		grdBalance.AllowEditing = false;
		grdBalance.AllowResizing = AllowResizingEnum.Both;
		grdBalance.AllowSorting = AllowSortingEnum.None;
		grdBalance.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grdBalance.Dock = DockStyle.Fill;
		grdBalance.DrawMode = DrawModeEnum.OwnerDraw;
		grdBalance.Font = font2;
		grdBalance.Rows.DefaultSize = 30;
		grdBalance.Tree.LineColor = Color.DimGray;
		grdBalance.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		Leqisoft.UI.Controls.Theme.SetCurrentObject(grdBalance);
		C1CommandLink lnkShareLedger = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Name = "BalanceEditor+cmdShareLedger";
		c1Command.Text = "分享账套";
		c1Command.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideShareLedger;
		c1Command.Click += delegate
		{
			_owner._owner.OnAfterShare(new LedgerShareEventArgs
			{
				Viewer = _owner,
				Link = lnkShareLedger,
				File = _owner.CurrentFilePath
			});
		};
		lnkShareLedger.Command = c1Command;
		toolBar.CommandLinks.Add(lnkShareLedger);
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "科目级次";
		c1Command2.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideDisplayLevel;
		c1Command2.Click += delegate(object s1, ClickEventArgs e1)
		{
			C1ContextMenu c1ContextMenu = new C1ContextMenu();
			for (int i = 0; i <= _owner.AccountTreeEditor.Tree.Tree.MaximumLevel; i++)
			{
				C1CommandLink c1CommandLink6 = new C1CommandLink();
				C1Command c1Command6 = new C1Command();
				c1Command6.Text = $"显示至{i + 1}级";
				c1Command6.UserData = i + 1;
				c1Command6.Click += CmdLayer_Click;
				c1Command6.CommandStateQuery += CmdLayer_CommandStateQuery;
				c1CommandLink6.Command = c1Command6;
				c1ContextMenu.CommandLinks.Add(c1CommandLink6);
			}
			C1CommandLink c1CommandLink7 = new C1CommandLink();
			C1Command c1Command7 = new C1Command();
			c1Command7.Text = "仅显示末级";
			c1Command7.Click += CmdLastLayer_Click;
			c1Command7.CommandStateQuery += CmdLastLayer_CommandStateQuery;
			c1CommandLink7.Command = c1Command7;
			c1ContextMenu.CommandLinks.Add(c1CommandLink7);
			c1ContextMenu.ShowContextMenu(toolBar, new Point(e1.CallerLink.Bounds.Left, e1.CallerLink.Bounds.Bottom));
		};
		c1CommandLink.Command = c1Command2;
		toolBar.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "切换样式";
		c1Command3.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideSwitchStyle;
		c1Command3.Click += delegate
		{
			if (_currentDisplayStyle == BalanceDisplayStyleEnum.HAVE_DIRECTION)
			{
				SwitchDisplayStyle(BalanceDisplayStyleEnum.NONE_DIRECTION);
				GenerateTotal();
			}
			else if (_currentDisplayStyle == BalanceDisplayStyleEnum.NONE_DIRECTION)
			{
				SwitchDisplayStyle(BalanceDisplayStyleEnum.HAVE_DIRECTION);
				GenerateTotal();
			}
			else
			{
				SwitchDisplayStyle(BalanceDisplayStyleEnum.HAVE_DIRECTION);
				GenerateTotal();
			}
		};
		c1CommandLink2.Command = c1Command3;
		toolBar.CommandLinks.Add(c1CommandLink2);
		cmdSidebarModifyBegin.Text = "修改期初数";
		cmdSidebarModifyBegin.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideModifyBegin;
		cmdSidebarModifyBegin.Click += delegate
		{
			_owner.ModifyBeginBalance((grdBalance.Row < 0) ? null : grdBalance.Rows[grdBalance.Row].UserData);
		};
		lnkSidebarModifyBegin.Command = cmdSidebarModifyBegin;
		toolBar.CommandLinks.Add(lnkSidebarModifyBegin);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "填充至底稿";
		c1Command4.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideFillToTable;
		c1Command4.Click += delegate
		{
			FillToTable();
		};
		c1Command4.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
		{
			e1.Visible = LedgerViewer.IsAuditPlatform;
		};
		c1CommandLink3.Command = c1Command4;
		cmdFillToTableOnToolbar = c1Command4;
		toolBar.CommandLinks.Add(c1CommandLink3);
		C1CommandLink c1CommandLink4 = new C1CommandLink();
		c1CommandLink4.Delimiter = true;
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "隐藏侧边栏";
		c1Command5.Image = Leqisoft.UI.LedgerView.Properties.Resources.sideHideSidebar;
		c1Command5.Click += CmdHideSidebar_Click;
		c1CommandLink4.Command = c1Command5;
		foreach (C1CommandLink commandLink in toolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		C1SplitContainer value = ComponentFactory.BuildSidebar(grdBalance, toolBar, out pnlSidebar);
		pnlBalanceGrid.HeaderLineColor = Color.Transparent;
		pnlBalanceGrid.Height = 573;
		pnlBalanceGrid.Location = new Point(0, 57);
		pnlBalanceGrid.Resizable = false;
		pnlBalanceGrid.Size = new Size(927, 573);
		pnlBalanceGrid.SizeRatio = 100.0;
		pnlBalanceGrid.Controls.Add(value);
		View.AutoSizeElement = AutoSizeElement.Both;
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.Location = new Point(0, 0);
		View.Size = new Size(927, 630);
		View.SplitterWidth = 0;
		View.Panels.Add(pnlBalanceTitle);
		View.Panels.Add(pnlBalanceHead);
		View.Panels.Add(pnlBalanceGrid);
		Leqisoft.UI.Controls.Theme.SetCurrentTree(View);
	}

	private void CmdHideSidebar_Click(object sender, ClickEventArgs e)
	{
		_owner.OnHideSidebarClick();
	}

	private void LblCompanyName_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			modifyContextMenu.ShowContextMenu(lblCompanyName, e.Location);
		}
	}

	public void PopulateBalanceSheet(Ledger ledger, bool displayEmpty, int level = 0)
	{
		ShouldShowAuxiliaryNode = level == 0 || level == _owner.AccountTreeEditor.Tree.Tree.MaximumLevel + 1;
		onlyLastLevelDisplay = false;
		currentDisplayLevel = level;
		grdBalance.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			InitializeCaption();
			grdBalance.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grdBalance.Rows.Fixed = 1;
			grdBalance.Cols.Fixed = 1;
			grdBalance.Tree.Column = 1;
			TrialBalanceSheet sheet = _owner.CacheManager.GetTrialBalanceSheetWithCache(ledger, StartDate, EndDate);
			_balanceSheet = sheet;
			foreach (Account item in ledger.RootAccounts.OrderBy((Account a) => a.Code))
			{
				AddChildren(item, null);
			}
			BalanceDisplayStyleEnum currentDisplayStyle = _currentDisplayStyle;
			_currentDisplayStyle = BalanceDisplayStyleEnum.HAVE_DIRECTION;
			SwitchDisplayStyle(currentDisplayStyle);
			if (ShouldShowAuxiliaryNode)
			{
				UpdateAuxiliaryByTree(_owner.AccountTreeEditor.Tree, level);
			}
			PopulateIndex(grdBalance);
			if (level == 0)
			{
				grdBalance.Tree.Show(0);
				currentDisplayLevel = grdBalance.Tree.MaximumLevel;
			}
			_owner.StyleRecord.ResumeStyle(grdBalance);
			GenerateTotal();
			void AddChildren(Account account, Node parentNode)
			{
				if ((level != 0 && parentNode != null && parentNode.Level >= level - 1) || (!displayEmpty && _owner.CacheManager.IsEmptyAccountWithCache(account)))
				{
					return;
				}
				Node parentNode2 = addNode(sheet, account, parentNode);
				foreach (Account child in account.Children)
				{
					AddChildren(child, parentNode2);
				}
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
			grdBalance.EndUpdate();
		}
		UpdateTitle(ledger);
		Node addNode(TrialBalanceSheet _sheet, Account _account, Node _parentNode)
		{
			Node node = ((_parentNode != null) ? _parentNode.AddNode(NodeTypeEnum.LastChild, _account.Code) : grdBalance.Rows.AddNode(0));
			node.Data = _account.Code;
			node.Key = _account;
			node.Row["Code"] = _account.Code;
			node.Row["Name"] = " ".PadLeft(node.Level * 4) + _account.Name;
			node.Row["BeginDC"] = Common.GetDCChar(_account.IsDebit, _sheet.Start[_account].Total);
			node.Row["BeginBalance"] = Math.Abs(_sheet.Start[_account].Total);
			node.Row["Debit"] = (_sheet.Debit.ContainsKey(_account) ? ((object)_sheet.Debit[_account].Total) : null);
			node.Row["Credit"] = (_sheet.Credit.ContainsKey(_account) ? ((object)_sheet.Credit[_account].Total) : null);
			node.Row["EndDC"] = Common.GetDCChar(_account.IsDebit, _sheet.End[_account].Total);
			node.Row["EndBalance"] = Math.Abs(_sheet.End[_account].Total);
			return node;
		}
	}

	public void PopulateBalanceSheetSpecLevel(Ledger ledger, bool displayEmpty, int level)
	{
		onlyLastLevelDisplay = false;
		bool pendingAllEvent = PendingAllEvent;
		if (!pendingAllEvent)
		{
			PendingAllEvent = true;
		}
		try
		{
			InitializeCaption();
			grdBalance.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grdBalance.Rows.Fixed = 1;
			grdBalance.Cols.Fixed = 1;
			grdBalance.Tree.Column = -1;
			TrialBalanceSheet sheet = _owner.CacheManager.GetTrialBalanceSheetWithCache(ledger, StartDate, EndDate);
			foreach (Account item in ledger.RootAccounts.OrderBy((Account a) => a.Code))
			{
				AddChildren(item, 0);
			}
			BalanceDisplayStyleEnum currentDisplayStyle = _currentDisplayStyle;
			_currentDisplayStyle = BalanceDisplayStyleEnum.HAVE_DIRECTION;
			SwitchDisplayStyle(currentDisplayStyle);
			PopulateIndex(grdBalance);
			_owner.StyleRecord.ResumeStyle(grdBalance);
			GenerateTotal();
			void AddChildren(Account account, int layer)
			{
				if ((displayEmpty || !_owner.CacheManager.IsEmptyAccountWithCache(account)) && layer <= level - 1)
				{
					if (layer != level - 1 && account.Children.Count != 0)
					{
						foreach (Account child in account.Children)
						{
							AddChildren(child, layer + 1);
						}
						return;
					}
					addRow(sheet, account, layer);
				}
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
		}
		UpdateTitle(ledger);
		C1.Win.C1FlexGrid.Row addRow(TrialBalanceSheet _sheet, Account _account, int _layer)
		{
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows.Add();
			row.UserData = _account;
			row["Code"] = _account.Code;
			row["Name"] = " ".PadLeft(_layer * 4) + _account.Name;
			row["BeginDC"] = Common.GetDCChar(_account.IsDebit, _sheet.Start[_account].Total);
			row["BeginBalance"] = Math.Abs(_sheet.Start[_account].Total);
			row["Debit"] = (_sheet.Debit.ContainsKey(_account) ? ((object)_sheet.Debit[_account].Total) : null);
			row["Credit"] = (_sheet.Credit.ContainsKey(_account) ? ((object)_sheet.Credit[_account].Total) : null);
			row["EndDC"] = Common.GetDCChar(_account.IsDebit, _sheet.End[_account].Total);
			row["EndBalance"] = Math.Abs(_sheet.End[_account].Total);
			return row;
		}
	}

	public void UpdateAccountData(Account account)
	{
		UpdateAccount(account);
		while (account.Parent != null)
		{
			account = account.Parent;
			UpdateAccount(account);
		}
	}

	private void UpdateAccount(Account account)
	{
		TrialBalanceSheet trialBalanceSheetWithCache = _owner.CacheManager.GetTrialBalanceSheetWithCache(Ledger);
		for (int i = 0; i < grdBalance.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
			if (!(row.UserData is Account account2) || account2 != account)
			{
				continue;
			}
			row["Code"] = account.Code;
			row["Name"] = " ".PadLeft(row.Node.Level * 4) + account.Name;
			row["BeginDC"] = Common.GetDCChar(account.IsDebit, trialBalanceSheetWithCache.Start[account].Total);
			row["BeginBalance"] = Math.Abs(trialBalanceSheetWithCache.Start[account].Total);
			row["Debit"] = (trialBalanceSheetWithCache.Debit.ContainsKey(account) ? ((object)trialBalanceSheetWithCache.Debit[account].Total) : null);
			row["Credit"] = (trialBalanceSheetWithCache.Credit.ContainsKey(account) ? ((object)trialBalanceSheetWithCache.Credit[account].Total) : null);
			row["EndDC"] = Common.GetDCChar(account.IsDebit, trialBalanceSheetWithCache.End[account].Total);
			row["EndBalance"] = Math.Abs(trialBalanceSheetWithCache.End[account].Total);
			Node[] nodes = row.Node.Nodes;
			foreach (Node node in nodes)
			{
				if (node.Key is Tuple<Account, AuxiliaryItem> tuple && tuple.Item1 == account2)
				{
					Node node2 = node;
					AuxiliaryItem item = tuple.Item2;
					SubsidiaryLedger subsidiaryLedger = Ledger.GetSubsidiaryLedger(account, StartDate, EndDate, item);
					SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
					node2.Data = account.Code + "-" + item.Code;
					node2.Key = Tuple.Create(account, item);
					node2.Row["Code"] = account.Code + "-" + item.Code;
					node2.Row["Name"] = (onlyLastLevelDisplay ? (account.Name + "－" + item.Name) : item.Name);
					node2.Row["Debit"] = subsidiaryLedgerTotal?.Debit ?? 0m;
					node2.Row["Credit"] = subsidiaryLedgerTotal?.Credit ?? 0m;
					decimal num = subsidiaryLedgerTotal?.Balance ?? subsidiaryLedger.BeginBalance;
					if (_currentDisplayStyle == BalanceDisplayStyleEnum.HAVE_DIRECTION)
					{
						node2.Row["BeginDC"] = Common.GetDCChar(account.IsDebit, subsidiaryLedger.BeginBalance);
						node2.Row["BeginBalance"] = Math.Abs(subsidiaryLedger.BeginBalance);
						node2.Row["EndDC"] = Common.GetDCChar(account.IsDebit, num);
						node2.Row["EndBalance"] = Math.Abs(num);
						continue;
					}
					object dCChar = Common.GetDCChar(account.IsDebit, subsidiaryLedger.BeginBalance);
					decimal num2 = Math.Abs(subsidiaryLedger.BeginBalance);
					object dCChar2 = Common.GetDCChar(account.IsDebit, num);
					decimal num3 = Math.Abs(num);
					node2.Row["BeginDC"] = ((dCChar?.ToString() == "借") ? num2 : 0m);
					node2.Row["BeginBalance"] = ((dCChar?.ToString() == "贷") ? num2 : 0m);
					node2.Row["EndDC"] = ((dCChar2?.ToString() == "借") ? num : 0m);
					node2.Row["EndBalance"] = ((dCChar2?.ToString() == "贷") ? num3 : 0m);
				}
			}
			break;
		}
	}

	private void PopulateLastLevelSheet(Ledger ledger, bool displayEmpty)
	{
		ShouldShowAuxiliaryNode = false;
		onlyLastLevelDisplay = true;
		grdBalance.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			InitializeCaption();
			grdBalance.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grdBalance.Rows.Fixed = 1;
			grdBalance.Cols.Fixed = 1;
			grdBalance.Tree.Column = 1;
			TrialBalanceSheet sheet = ledger.GetTrialBalanceSheet(StartDate, EndDate);
			Dictionary<Account, AuxiliaryClass> expandingAux = _owner.AccountTreeEditor.GetAccountExpandingAuxClass();
			Dictionary<object, string> list2 = new Dictionary<object, string>();
			foreach (Account item in ledger.RootAccounts.OrderBy((Account a) => a.Code))
			{
				string empty = string.Empty;
				getLastLevelAccounts(item, empty, ref list2);
			}
			int rowIndex = 1;
			foreach (KeyValuePair<object, string> item2 in list2)
			{
				Account account2 = null;
				AuxiliaryItem auxiliaryItem = null;
				object key = item2.Key;
				if (!(key is Account account3))
				{
					if (!(key is Tuple<Account, AuxiliaryItem> tuple))
					{
						continue;
					}
					account2 = tuple.Item1;
					auxiliaryItem = tuple.Item2;
				}
				else
				{
					account2 = account3;
				}
				if ((displayEmpty || !_owner.CacheManager.IsEmptyAccountWithCache(account2)) && (displayEmpty || auxiliaryItem == null || !_owner.CacheManager.IsEmptyAuxiliaryItemWithCache(account2, auxiliaryItem)))
				{
					if (auxiliaryItem == null)
					{
						addRow(account2, item2.Value);
					}
					else
					{
						addRow2(account2, auxiliaryItem, item2.Value);
					}
				}
			}
			BalanceDisplayStyleEnum currentDisplayStyle = _currentDisplayStyle;
			_currentDisplayStyle = BalanceDisplayStyleEnum.HAVE_DIRECTION;
			SwitchDisplayStyle(currentDisplayStyle);
			UpdateAuxiliaryByTree(_owner.AccountTreeEditor.Tree, -1);
			PopulateIndex(grdBalance);
			_owner.StyleRecord.ResumeStyle(grdBalance);
			GenerateTotal();
			UpdateTitle(ledger);
			grdBalance.Tree.Show(0);
			void addRow(Account account, string name)
			{
				Node node2 = grdBalance.Rows.AddNode(0);
				node2.Data = account.Code;
				node2.Key = account;
				node2.Row["Index"] = rowIndex++;
				node2.Row["Code"] = account.Code;
				node2.Row["Name"] = name;
				node2.Row["BeginDC"] = Common.GetDCChar(account.IsDebit, sheet.Start[account].Total);
				node2.Row["BeginBalance"] = Math.Abs(sheet.Start[account].Total);
				node2.Row["Debit"] = (sheet.Debit.ContainsKey(account) ? ((object)sheet.Debit[account].Total) : null);
				node2.Row["Credit"] = (sheet.Credit.ContainsKey(account) ? ((object)sheet.Credit[account].Total) : null);
				node2.Row["EndDC"] = Common.GetDCChar(account.IsDebit, sheet.End[account].Total);
				node2.Row["EndBalance"] = Math.Abs(sheet.End[account].Total);
			}
			void getLastLevelAccounts(Account account, string lastName, ref Dictionary<object, string> list)
			{
				Dictionary<AuxiliaryClass, ClassBalance> classBalances = sheet.End[account].ClassBalances;
				if (classBalances.Count > 0)
				{
					if (expandingAux.ContainsKey(account))
					{
						foreach (KeyValuePair<AuxiliaryItem, decimal> itemBalance in classBalances[expandingAux[account]].ItemBalances)
						{
							string text = string.Join("-", lastName, account.Name, itemBalance.Key.Name);
							list.Add(Tuple.Create(account, itemBalance.Key), text.TrimStart().TrimStart('-'));
						}
						return;
					}
					{
						foreach (KeyValuePair<AuxiliaryItem, decimal> itemBalance2 in classBalances[TableCollectorAbstract.GetFirstOrDefaultAuxiliary(ledger, account, sheet)].ItemBalances)
						{
							string text2 = string.Join("-", lastName, account.Name, itemBalance2.Key.Name);
							list.Add(Tuple.Create(account, itemBalance2.Key), text2.TrimStart().TrimStart('-'));
						}
						return;
					}
				}
				if (account.Children.Count == 0)
				{
					lastName = string.Join("-", lastName, account.Name);
					list.Add(account, lastName.TrimStart().TrimStart('-'));
					return;
				}
				lastName = string.Join("-", lastName, account.Name);
				foreach (Account child in account.Children)
				{
					getLastLevelAccounts(child, lastName, ref list);
				}
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
			grdBalance.EndUpdate();
		}
		void addRow2(Account account, AuxiliaryItem item, string name)
		{
			SubsidiaryLedger subsidiaryLedger = Ledger.GetSubsidiaryLedger(account, StartDate, EndDate, item);
			SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
			Node node = grdBalance.Rows.AddNode(0);
			node.Data = account.Code + "-" + item.Code;
			node.Key = Tuple.Create(account, item);
			node.Row["Code"] = account.Code + "-" + item.Code;
			node.Row["Name"] = name;
			node.Row["BeginDC"] = Common.GetDCChar(account.IsDebit, subsidiaryLedger.BeginBalance);
			node.Row["BeginBalance"] = Math.Abs(subsidiaryLedger.BeginBalance);
			node.Row["Debit"] = subsidiaryLedgerTotal?.Debit ?? 0m;
			node.Row["Credit"] = subsidiaryLedgerTotal?.Credit ?? 0m;
			decimal num = subsidiaryLedgerTotal?.Balance ?? subsidiaryLedger.BeginBalance;
			node.Row["EndDC"] = Common.GetDCChar(account.IsDebit, num);
			node.Row["EndBalance"] = Math.Abs(num);
		}
	}

	private void InitializeCaption()
	{
		if (!initializedCaption)
		{
			grdBalance.Cols.Count = 0;
			grdBalance.Rows.Count = 1;
			grdBalance.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.Column column = grdBalance.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(int);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 50;
			column = grdBalance.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 100;
			column = grdBalance.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column.AllowMerging = true;
			column.Width = 200;
			column = grdBalance.Cols.Add();
			column.Name = "BeginDC";
			column.Caption = "期初余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 100;
			column = grdBalance.Cols.Add();
			column.Name = "BeginBalance";
			column.Caption = "期初余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.Width = 100;
			column.Sort = SortFlags.None;
			column = grdBalance.Cols.Add();
			column.Name = "Debit";
			column.Caption = "借方发生额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.Width = 100;
			column.Sort = SortFlags.None;
			column = grdBalance.Cols.Add();
			column.Name = "Credit";
			column.Caption = "贷方发生额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.Width = 100;
			column.Sort = SortFlags.None;
			column = grdBalance.Cols.Add();
			column.Name = "EndDC";
			column.Caption = "期末余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.Width = 100;
			column = grdBalance.Cols.Add();
			column.Name = "EndBalance";
			column.Caption = "期末余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column.Width = 100;
			column.Sort = SortFlags.None;
			initializedCaption = true;
		}
		else
		{
			grdBalance.Rows.Count = 1;
			C1.Win.C1FlexGrid.Column column2 = grdBalance.Cols["Index"];
			column2.Caption = "序号";
			column2.DataType = typeof(int);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2 = grdBalance.Cols["Code"];
			column2.Caption = "科目代码";
			column2.DataType = typeof(string);
			column2.AllowMerging = true;
			column2 = grdBalance.Cols["Name"];
			column2.Caption = "科目名称";
			column2.DataType = typeof(string);
			column2.AllowMerging = true;
			column2 = grdBalance.Cols["BeginDC"];
			column2.Caption = "期初余额方向";
			column2.DataType = typeof(string);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2 = grdBalance.Cols["BeginBalance"];
			column2.Caption = "期初余额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.Sort = SortFlags.None;
			column2 = grdBalance.Cols["Debit"];
			column2.Caption = "借方发生额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.Sort = SortFlags.None;
			column2 = grdBalance.Cols["Credit"];
			column2.Caption = "贷方发生额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.Sort = SortFlags.None;
			column2 = grdBalance.Cols["EndDC"];
			column2.Caption = "期末余额方向";
			column2.DataType = typeof(string);
			column2.TextAlign = TextAlignEnum.CenterCenter;
			column2 = grdBalance.Cols["EndBalance"];
			column2.Caption = "期末余额";
			column2.DataType = typeof(decimal);
			column2.Format = "#,0.00;-#,0.00;#";
			column2.Sort = SortFlags.None;
		}
	}

	public void PopulateIndex(C1FlexGrid grid)
	{
		if (grid.Cols.Contains("Index"))
		{
			int num = 1;
			for (int i = grid.Rows.Fixed; i < grid.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grid.Rows[i];
				row["Index"] = num++;
			}
		}
	}

	public void SelectRow(object userdata)
	{
		if (userdata != null)
		{
			C1.Win.C1FlexGrid.Row row = Common.FindRow(grdBalance, userdata);
			if (row != null)
			{
				grdBalance.Row = row.Index;
			}
		}
	}

	public void FillToTable()
	{
		Account account = null;
		object auxiliary = null;
		int row = grdBalance.Row;
		if (row >= grdBalance.Rows.Fixed && row < grdBalance.Rows.Count)
		{
			object userData = grdBalance.Rows[row].UserData;
			if (!(userData is Account account2))
			{
				if (!(userData is Tuple<Account, AuxiliaryClass> tuple))
				{
					if (!(userData is Tuple<Account, AuxiliaryItem> tuple2))
					{
						return;
					}
					account = tuple2.Item1;
					auxiliary = tuple2.Item2;
				}
				else
				{
					account = tuple.Item1;
					auxiliary = tuple.Item2;
				}
			}
			else
			{
				account = account2;
			}
		}
		List<object> source = null;
		if (grdBalance.FilterManager.Filters.Count > 0)
		{
			HashSet<object> hashSet = new HashSet<object>();
			for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows[i];
				if (!row2.Visible)
				{
					continue;
				}
				Account account3 = null;
				object userData2 = row2.UserData;
				if (!(userData2 is Account account4))
				{
					if (!(userData2 is Tuple<Account, AuxiliaryClass> tuple3))
					{
						if (userData2 is Tuple<Account, AuxiliaryItem> tuple4)
						{
							account3 = tuple4.Item1;
						}
					}
					else
					{
						account3 = tuple3.Item1;
					}
				}
				else
				{
					account3 = account4;
				}
				if (account3 != null)
				{
					for (Account account5 = account3; account5 != null; account5 = account5.Parent)
					{
						hashSet.Add(account5);
					}
				}
			}
			source = hashSet.ToList();
		}
		_owner._owner.OnAfterCollect(new LedgerCollectEventArgs
		{
			Viewer = _owner,
			Account = account,
			Auxiliary = auxiliary,
			CollectObject = CollectObjectEnum.Balance,
			StartTime = StartDate,
			EndTime = EndDate,
			Source = source,
			IsShowBalanceAllAccountTypeTable = true
		});
	}

	private void UpdateAuxiliaryByTree(C1FlexGridEx accountTree, int level)
	{
		if (level == -1)
		{
			return;
		}
		Dictionary<Account, AuxiliaryClass> accountExpandingAuxClass = _owner.AccountTreeEditor.GetAccountExpandingAuxClass();
		if (accountExpandingAuxClass.Count <= 0)
		{
			return;
		}
		grdBalance.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			foreach (KeyValuePair<Account, AuxiliaryClass> item in accountExpandingAuxClass)
			{
				AppendAuxiliaryItems(item.Key, item.Value, level);
			}
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			grdBalance.EndUpdate();
		}
	}

	public void RemoveAuxiliaryItems(Account account, AuxiliaryClass auxiliaryClass = null)
	{
		Node node = null;
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)grdBalance.Rows)
		{
			if (item.UserData is Account account2 && account2 == account)
			{
				node = item.Node;
				break;
			}
		}
		if (node == null)
		{
			return;
		}
		List<Node> list = new List<Node>();
		Node[] nodes = node.Nodes;
		foreach (Node node2 in nodes)
		{
			if (node2.Key is Tuple<Account, AuxiliaryItem> tuple && (auxiliaryClass == null || tuple.Item2.Class == auxiliaryClass))
			{
				list.Add(node2);
			}
		}
		grdBalance.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			foreach (Node item2 in list)
			{
				item2.RemoveNode();
			}
			PopulateIndex(grdBalance);
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			grdBalance.EndUpdate();
		}
	}

	public void AppendAuxiliaryItems(Account account, AuxiliaryClass auxiliaryClass, int level = 0)
	{
		Node node = grdBalance.Rows[1].Node;
		foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)grdBalance.Rows)
		{
			if (item.UserData is Account account2 && account2 == account)
			{
				node = item.Node;
				break;
			}
		}
		if (node == null)
		{
			return;
		}
		grdBalance.BeginUpdate();
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = _owner.CacheManager.GetAccountBalanceWithCache(Ledger, account).ClassBalances;
			foreach (KeyValuePair<AuxiliaryItem, decimal> item2 in classBalances[auxiliaryClass].ItemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
			{
				addNode(node, account, item2.Key);
			}
		}
		finally
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = false;
			}
			grdBalance.EndUpdate();
		}
		void addNode(Node _parentNode, Account _account, AuxiliaryItem _auxiliaryItem)
		{
			if ((_owner.IsDisplayEmptyAccount() || !_owner.IsEmptyAuxiliaryItem(account, _auxiliaryItem)) && (level <= 0 || _parentNode == null || _parentNode.Level < level - 1))
			{
				Node node2 = ((_parentNode != null) ? _parentNode.AddNode(NodeTypeEnum.LastChild, string.Empty) : grdBalance.Rows.AddNode(0));
				node2.Data = _account.Code + "-" + _auxiliaryItem.Code;
				node2.Key = Tuple.Create(account, _auxiliaryItem);
				node2.Row["Code"] = _account.Code + "-" + _auxiliaryItem.Code;
				node2.Row["Name"] = new string(' ', node2.Level * 4) + (onlyLastLevelDisplay ? (_account.Name + "－" + _auxiliaryItem.Name) : _auxiliaryItem.Name);
				node2.Row["Debit"] = _balanceSheet.Debit.Get(_account, _auxiliaryItem);
				node2.Row["Credit"] = _balanceSheet.Credit.Get(_account, _auxiliaryItem);
				decimal num = _balanceSheet.Start.Get(_account, _auxiliaryItem);
				decimal num2 = _balanceSheet.End.Get(_account, _auxiliaryItem);
				if (_currentDisplayStyle == BalanceDisplayStyleEnum.HAVE_DIRECTION)
				{
					node2.Row["BeginDC"] = Common.GetDCChar(account.IsDebit, num);
					node2.Row["BeginBalance"] = Math.Abs(num);
					node2.Row["EndDC"] = Common.GetDCChar(account.IsDebit, num2);
					node2.Row["EndBalance"] = Math.Abs(num2);
				}
				else
				{
					object dCChar = Common.GetDCChar(account.IsDebit, num);
					decimal num3 = Math.Abs(num);
					object dCChar2 = Common.GetDCChar(account.IsDebit, num2);
					decimal num4 = Math.Abs(num2);
					node2.Row["BeginDC"] = ((dCChar?.ToString() == "借") ? num3 : 0m);
					node2.Row["BeginBalance"] = ((dCChar?.ToString() == "贷") ? num3 : 0m);
					node2.Row["EndDC"] = ((dCChar2?.ToString() == "借") ? num2 : 0m);
					node2.Row["EndBalance"] = ((dCChar2?.ToString() == "贷") ? num4 : 0m);
				}
				node2.Expanded = true;
				(node2.Row.Style ?? node2.Row.StyleNew).ForeColor = Color.Purple;
			}
		}
	}

	public void SetTheme()
	{
		btnBalanceBack.BackColor = Color.Transparent;
		btnBalanceBack.FlatStyle = FlatStyle.Flat;
		btnBalanceBack.FlatAppearance.BorderSize = 0;
		btnBalanceBack.FlatAppearance.MouseOverBackColor = Color.LightGray;
		grdBalance.Styles.Fixed.Border.Color = Color.DarkGray;
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

	public void AttachTooltip(TooltipManager tooltipManager)
	{
		attachTooltip(grdBalance, TipInfo.Parse(TipResource.科目余额表区域));
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

	public void Dispose()
	{
		_balanceSheet = null;
		grdBalance.Clear(ClearFlags.UserData);
	}

	private bool TryParseDateTime(string text, out DateTime value)
	{
		if (DateTime.TryParse(text, out value))
		{
			return true;
		}
		try
		{
			DateTime dateTime = Convert.ToDateTime(text);
			value = dateTime;
			return true;
		}
		catch
		{
		}
		value = default(DateTime);
		return false;
	}

	private void DatStart_ValueChanged(object sender, EventArgs e)
	{
		if (TryParseDateTime(dteStart.Text, out var value) && TryParseDateTime(dteEnd.Text, out var value2))
		{
			StartDate = value;
			EndDate = value2;
			OnAccountPeriodChanged(this, Tuple.Create(value, value2));
			PopulateBalanceSheet(Ledger, _owner.AccountTreeEditor.DisplayEmptyAccount);
		}
	}

	private void DatEnd_ValueChanged(object sender, EventArgs e)
	{
		if (TryParseDateTime(dteStart.Text, out var value) && TryParseDateTime(dteEnd.Text, out var value2))
		{
			StartDate = value;
			EndDate = value2.AddDays(1.0).AddMilliseconds(-1.0);
			OnAccountPeriodChanged(this, Tuple.Create(value, value2));
			PopulateBalanceSheet(Ledger, _owner.AccountTreeEditor.DisplayEmptyAccount);
		}
	}

	private void DatStart_DoubleClick(object sender, EventArgs e)
	{
		Leqisoft.UI.Controls.Theme.SetCurrentObject(dteStart);
		dteStart.OpenDropDown();
	}

	private void DatEnd_DoubleClick(object sender, EventArgs e)
	{
		Leqisoft.UI.Controls.Theme.SetCurrentObject(dteEnd);
		dteEnd.OpenDropDown();
	}

	private void datStart_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			dteStart.Value = dteStart.Text;
		}
	}

	private void datEnd_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			dteEnd.Value = dteEnd.Text;
		}
	}

	private void _grid_AfterResizeRow(object sender, RowColEventArgs e)
	{
		if (PendingAllEvent)
		{
			return;
		}
		try
		{
			_owner.StyleRecord.RecordHeight(sender as C1FlexGridEx, e.Row);
		}
		catch
		{
		}
	}

	private void _grid_AfterResizeColumn(object sender, RowColEventArgs e)
	{
		if (PendingAllEvent)
		{
			return;
		}
		bool pendingAllEvent = PendingAllEvent;
		try
		{
			if (!pendingAllEvent)
			{
				PendingAllEvent = true;
			}
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordWidth(c1FlexGridEx.Name, c1FlexGridEx.Cols[e.Col].Name, c1FlexGridEx.Cols[e.Col].Width);
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
		}
	}

	private void _grid_AfterDragColumn(object sender, DragRowColEventArgs e)
	{
		if (PendingAllEvent)
		{
			return;
		}
		try
		{
			C1FlexGridEx c1FlexGridEx = sender as C1FlexGridEx;
			_owner.StyleRecord.RecordOrder(c1FlexGridEx.Name, from C1.Win.C1FlexGrid.Column t in c1FlexGridEx.Cols
				select t.Name);
		}
		catch
		{
		}
	}

	private void btnBalanceBack_Click(object sender, EventArgs e)
	{
		grdBalance.BeginUpdate();
		try
		{
			foreach (C1.Win.C1FlexGrid.Row item in (IEnumerable)grdBalance.Rows)
			{
				item.Visible = true;
			}
			grdBalance.FilterManager.Clear();
		}
		finally
		{
			grdBalance.EndUpdate();
		}
	}

	private void CtxMenu_Popup(object sender, EventArgs e)
	{
		ctxCell.CommandLinks.Clear();
		ctxCell.CommandLinks.Add(lnkCopy);
		ctxCell.CommandLinks.Add(grdBalance.FilterManager.GenLnkFilter());
		ctxCell.CommandLinks.Add(grdBalance.FilterManager.GenLnkSample());
		ctxCell.CommandLinks.Add(grdBalance.FilterManager.GenLnkSelect());
		ctxCell.CommandLinks.Add(grdBalance.FilterManager.GenLnkCancelCurrentColumn());
		for (int i = 0; i <= _owner.AccountTreeEditor.Tree.Tree.MaximumLevel; i++)
		{
			C1Command c1Command = new C1Command();
			c1Command.Text = $"显示至{i + 1}级";
			c1Command.UserData = i + 1;
			c1Command.Click += CmdLayer_Click;
			c1Command.CommandStateQuery += CmdLayer_CommandStateQuery;
			C1CommandLink c1CommandLink = new C1CommandLink();
			if (i == 0)
			{
				c1CommandLink.Delimiter = true;
			}
			c1CommandLink.Command = c1Command;
			ctxCell.CommandLinks.Add(c1CommandLink);
		}
		ctxCell.CommandLinks.Add(lnkLastLayer);
		lnkSwitchDisplayStyle.Delimiter = true;
		ctxCell.CommandLinks.Add(lnkSwitchDisplayStyle);
		ctxCell.CommandLinks.Add(lnkModifyBeginBalance);
		lnkFillToTable.Delimiter = true;
		lnkModifyBeginBalance.Delimiter = true;
		ctxCell.CommandLinks.Add(lnkFillToTable);
	}

	private void CmdCopy_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = grdBalance.Row >= grdBalance.Rows.Fixed && grdBalance.Col >= grdBalance.Cols.Fixed;
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Common.SetSelectionToClipboard(grdBalance);
	}

	private void CmdLastLayer_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = grdBalance.Row >= grdBalance.Rows.Fixed && grdBalance.Col >= grdBalance.Cols.Fixed;
	}

	private void CmdLastLayer_Click(object sender, ClickEventArgs e)
	{
		PopulateLastLevelSheet(Ledger, _owner.AccountTreeEditor.DisplayEmptyAccount);
	}

	private void CmdLayer_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = grdBalance.Row >= grdBalance.Rows.Fixed && grdBalance.Col >= grdBalance.Cols.Fixed;
	}

	private void CmdLayer_Click(object sender, ClickEventArgs e)
	{
		int level = Convert.ToInt32(((C1Command)sender).UserData);
		PopulateBalanceSheet(Ledger, _owner.AccountTreeEditor.DisplayEmptyAccount, level);
	}

	private void CmdFillToTable_Click(object sender, ClickEventArgs e)
	{
		FillToTable();
	}

	private void Initialize()
	{
		btnBalanceBack.Click += btnBalanceBack_Click;
		grdBalance.AfterResizeRow += _grid_AfterResizeRow;
		grdBalance.AfterDragColumn += _grid_AfterDragColumn;
		grdBalance.AfterResizeColumn += _grid_AfterResizeColumn;
		grdBalance.MouseDoubleClick += GrdBalance_MouseDoubleClick;
	}

	private void GrdBalance_MouseDoubleClick(object sender, MouseEventArgs e)
	{
	}

	private void SortColumn(SortFlags sortFlags, int mouseCol)
	{
		grdBalance.BeginUpdate();
		Point scrollPosition = grdBalance.ScrollPosition;
		try
		{
			PopulateBalanceSheetSpecLevel(Ledger, _owner.AccountTreeEditor.DisplayEmptyAccount, currentDisplayLevel);
			C1.Win.C1FlexGrid.Column column = grdBalance.Cols[mouseCol];
			column.Sort = sortFlags;
			grdBalance.AllowSorting = AllowSortingEnum.SingleColumn;
			column.AllowSorting = true;
			grdBalance.Sort(column.Sort, mouseCol);
			PopulateIndex(grdBalance);
			grdBalance.AllowSorting = AllowSortingEnum.None;
			if (grdBalance.Rows.Count > grdBalance.Rows.Fixed)
			{
				grdBalance.Select(grdBalance.Rows.Fixed, column.Index, grdBalance.Rows.Count - 1, column.Index);
			}
		}
		finally
		{
			grdBalance.ScrollPosition = scrollPosition;
			grdBalance.EndUpdate();
		}
	}

	private void BindContextMenu()
	{
		cmdCopy.Text = "复制";
		cmdCopy.Image = ContextResources.ctxCopy;
		cmdCopy.Click += CmdCopy_Click;
		cmdCopy.CommandStateQuery += CmdCopy_CommandStateQuery;
		lnkCopy.Command = cmdCopy;
		cmdLastLayer.Text = "仅显示末级";
		cmdLastLayer.Click += CmdLastLayer_Click;
		cmdLastLayer.CommandStateQuery += CmdLastLayer_CommandStateQuery;
		lnkLastLayer.Command = cmdLastLayer;
		cmdSwitchDisplayStyle.Text = "切换显示样式";
		cmdSwitchDisplayStyle.Image = ContextResources.ctxSwitchDisplay;
		cmdSwitchDisplayStyle.Click += CmdSwitchDisplayStyle_Click;
		lnkSwitchDisplayStyle.Command = cmdSwitchDisplayStyle;
		cmdModifyBeginBalance.Text = "修改期初数";
		cmdModifyBeginBalance.Image = ContextResources.modifyLedger;
		cmdModifyBeginBalance.CommandStateQuery += CmdModifyBeginBalance_CommandStateQuery;
		cmdModifyBeginBalance.Click += CmdModifyBeginBalance_Click;
		lnkModifyBeginBalance.Command = cmdModifyBeginBalance;
		cmdFillToTable.Text = "填充至底稿";
		cmdFillToTable.Image = ContextResources.ctxFillToTable;
		cmdFillToTable.Click += CmdFillToTable_Click;
		cmdFillToTable.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
		{
			e1.Visible = LedgerViewer.IsAuditPlatform;
		};
		lnkFillToTable.Command = cmdFillToTable;
		ctxCell.Popup += CtxMenu_Popup;
		grdBalance.MouseClick += GrdBalance_MouseClick;
		grdBalance.BodySelectionChanged += GrdBalance_BodySelectionChanged;
		cmdModifyCompanyName.Text = "修改核算单位名称";
		cmdModifyCompanyName.Click += delegate
		{
			string text = InputForm.Text("修改核算单位名称", "请输入核算单位名称：", Ledger.CompanyName, 200);
			if (text != null && text != lblCompanyName.Text)
			{
				Ledger.SetCompanyName(text);
				Ledger.Save();
				UpdateTitle(Ledger);
			}
		};
		lnkModifyCompanyName.Command = cmdModifyCompanyName;
		modifyContextMenu.CommandLinks.Add(lnkModifyCompanyName);
		cmdModifyStartDate.Text = "修改账套起始日期";
		cmdModifyStartDate.Click += delegate
		{
			DateTime? dateTime = InputForm.DateInput("修改账套起始日期", "请输入账套起始日期：", Ledger.StartDate);
			if (dateTime.HasValue)
			{
				DateTime value = dateTime.Value;
				if (Ledger.Vouchers.Count > 0)
				{
					DateTime value2 = Ledger.Vouchers.Min((Voucher v) => v.Day);
					if (value.CompareTo(value2) > 0)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账套起始日期的设置必须小于凭证的最小凭证日期");
						return;
					}
				}
				Ledger.SetStartDate(value);
				Ledger.Save();
				StartDate = Ledger.StartDate;
			}
		};
		lnkModifyStartDate.Command = cmdModifyStartDate;
		modifyContextMenu.CommandLinks.Add(lnkModifyStartDate);
		cmdDes.Text = "降序排列";
		cmdDes.Image = ContextResources.ctxDescending;
		cmdDes.Click += CmdDes_Click;
		cmdDes.CommandStateQuery += CmdDes_CommandStateQuery;
		lnkDes.Command = cmdDes;
		cmdAsc.Text = "升序排列";
		cmdAsc.Image = ContextResources.ctxAscending;
		cmdAsc.Click += CmdAsc_Click;
		cmdAsc.CommandStateQuery += CmdAsc_CommandStateQuery;
		lnkAsc.Command = cmdAsc;
		cmdNormal.Text = "取消排序";
		cmdNormal.Click += CmdNormal_Click;
		cmdNormal.CommandStateQuery += CmdNormal_CommandStateQuery;
		lnkNormal.Command = cmdNormal;
		ctxFixed.HideFirstDelimiter = true;
		ctxFixed.CommandLinks.Add(lnkDes);
		ctxFixed.CommandLinks.Add(lnkAsc);
		ctxFixed.CommandLinks.Add(lnkNormal);
		C1Command c1Command = new C1Command();
		c1Command.Text = "隐藏本列";
		c1Command.UserData = grdBalance;
		c1Command.Click += _owner.ColHide_Click;
		C1CommandLink c1CommandLink = new C1CommandLink();
		c1CommandLink.Command = c1Command;
		c1CommandLink.Delimiter = true;
		ctxFixed.CommandLinks.Add(c1CommandLink);
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "取消隐藏";
		c1Command2.UserData = grdBalance;
		c1Command2.Click += _owner.CancelHide_Click;
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		c1CommandLink2.Command = c1Command2;
		ctxFixed.CommandLinks.Add(c1CommandLink2);
		ctxEmpty.CommandLinks.Add(grdBalance.FilterManager.GenLnkCancelAll());
	}

	private void CmdNormal_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdBalance.Col;
		e.Visible = col >= 0 && (col == grdBalance.Cols["Debit"].Index || col == grdBalance.Cols["Credit"].Index || col == grdBalance.Cols["EndBalance"].Index || col == grdBalance.Cols["BeginBalance"].Index);
	}

	private void CmdNormal_Click(object sender, ClickEventArgs e)
	{
		SortColumn(SortFlags.None, grdBalance.Col);
	}

	private void CmdAsc_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdBalance.Col;
		e.Visible = col >= 0 && (col == grdBalance.Cols["Debit"].Index || col == grdBalance.Cols["Credit"].Index || col == grdBalance.Cols["EndBalance"].Index || col == grdBalance.Cols["BeginBalance"].Index);
	}

	private void CmdAsc_Click(object sender, ClickEventArgs e)
	{
		SortColumn(SortFlags.Ascending, grdBalance.Col);
	}

	private void CmdDes_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdBalance.Col;
		e.Visible = col >= 0 && (col == grdBalance.Cols["Debit"].Index || col == grdBalance.Cols["Credit"].Index || col == grdBalance.Cols["EndBalance"].Index || col == grdBalance.Cols["BeginBalance"].Index);
	}

	private void CmdDes_Click(object sender, ClickEventArgs e)
	{
		SortColumn(SortFlags.Descending, grdBalance.Col);
	}

	private void GrdBalance_BodySelectionChanged(object sender, EventArgs e)
	{
		int col = grdBalance.Col;
		if (col < grdBalance.Cols.Fixed)
		{
			cmdSidebarModifyBegin.Visible = false;
			return;
		}
		C1.Win.C1FlexGrid.Column column = grdBalance.Cols[col];
		if (column.Name == "BeginDC" || column.Name == "BeginBalance")
		{
			cmdSidebarModifyBegin.Visible = true;
		}
		else
		{
			cmdSidebarModifyBegin.Visible = false;
		}
	}

	private void GrdBalance_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grdBalance.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxFixed.ShowContextMenu(grdBalance, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(grdBalance, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxCell.ShowContextMenu(grdBalance, e.Location);
				break;
			}
		}
	}

	private void CmdModifyBeginBalance_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int mouseCol = grdBalance.MouseCol;
		e.Visible = mouseCol == grdBalance.Cols["BeginDC"].Index || mouseCol == grdBalance.Cols["BeginBalance"].Index;
	}

	private void CmdModifyBeginBalance_Click(object sender, ClickEventArgs e)
	{
		_owner.ModifyBeginBalance((grdBalance.Row < 0) ? null : grdBalance.Rows[grdBalance.Row].UserData);
	}

	private void CmdSwitchDisplayStyle_Click(object sender, ClickEventArgs e)
	{
		grdBalance.BeginUpdate();
		try
		{
			if (_currentDisplayStyle == BalanceDisplayStyleEnum.HAVE_DIRECTION)
			{
				SwitchDisplayStyle(BalanceDisplayStyleEnum.NONE_DIRECTION);
				GenerateTotal();
			}
			else if (_currentDisplayStyle == BalanceDisplayStyleEnum.NONE_DIRECTION)
			{
				SwitchDisplayStyle(BalanceDisplayStyleEnum.HAVE_DIRECTION);
				GenerateTotal();
			}
		}
		finally
		{
			grdBalance.EndUpdate();
		}
	}

	private void UpdateTitle(Ledger ledger)
	{
		lblCompanyName.Text = "核算单位：" + ledger.CompanyName;
		lblCurrency.Text = "金额单位：" + (ledger.BaseCurrency?.Name ?? "人民币元");
	}

	private void SwitchDisplayStyle(BalanceDisplayStyleEnum newStyle)
	{
		if (_currentDisplayStyle == newStyle)
		{
			return;
		}
		if (_currentDisplayStyle == BalanceDisplayStyleEnum.HAVE_DIRECTION && newStyle == BalanceDisplayStyleEnum.NONE_DIRECTION)
		{
			Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal, decimal, decimal>> dictionary = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal, decimal, decimal>>();
			for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
				string text = row["BeginDC"]?.ToString();
				decimal result;
				decimal num = (decimal.TryParse(row["BeginBalance"]?.ToString(), out result) ? result : 0m);
				string text2 = row["EndDC"]?.ToString();
				decimal result2;
				decimal num2 = (decimal.TryParse(row["EndBalance"]?.ToString(), out result2) ? result2 : 0m);
				dictionary.Add(row, Tuple.Create((text == "借") ? num : 0m, (text == "贷") ? num : 0m, (text2 == "借") ? num2 : 0m, (text2 == "贷") ? num2 : 0m));
			}
			C1.Win.C1FlexGrid.Column column = grdBalance.Cols["BeginDC"];
			column.Caption = "期初借方余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			C1.Win.C1FlexGrid.Column column2 = grdBalance.Cols["BeginBalance"];
			column2.Caption = "期初贷方余额";
			C1.Win.C1FlexGrid.Column column3 = grdBalance.Cols["EndDC"];
			column3.Caption = "期末借方余额";
			column3.DataType = typeof(decimal);
			column3.Format = "#,0.00;-#,0.00;#";
			C1.Win.C1FlexGrid.Column column4 = grdBalance.Cols["EndBalance"];
			column4.Caption = "期末贷方余额";
			foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal, decimal, decimal>> item in dictionary)
			{
				C1.Win.C1FlexGrid.Row key = item.Key;
				key["BeginDC"] = item.Value.Item1;
				key["BeginBalance"] = item.Value.Item2;
				key["EndDC"] = item.Value.Item3;
				key["EndBalance"] = item.Value.Item4;
			}
			_currentDisplayStyle = newStyle;
		}
		else
		{
			if (_currentDisplayStyle != BalanceDisplayStyleEnum.NONE_DIRECTION || newStyle != BalanceDisplayStyleEnum.HAVE_DIRECTION)
			{
				return;
			}
			Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal, string, decimal>> dictionary2 = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal, string, decimal>>();
			for (int j = grdBalance.Rows.Fixed; j < grdBalance.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows[j];
				decimal result3;
				decimal num3 = (decimal.TryParse(row2["BeginDC"]?.ToString(), out result3) ? result3 : 0m);
				decimal result4;
				decimal num4 = (decimal.TryParse(row2["BeginBalance"]?.ToString(), out result4) ? result4 : 0m);
				string text3 = ((num3 == 0m) ? ((num4 == 0m) ? "平" : "贷") : "借");
				decimal result5;
				decimal num5 = (decimal.TryParse(row2["EndDC"]?.ToString(), out result5) ? result5 : 0m);
				decimal result6;
				decimal num6 = (decimal.TryParse(row2["EndBalance"]?.ToString(), out result6) ? result6 : 0m);
				string text4 = ((num5 == 0m) ? ((num6 == 0m) ? "平" : "贷") : "借");
				dictionary2.Add(row2, Tuple.Create(text3, (text3 == "借") ? num3 : ((text3 == "贷") ? num4 : 0m), text4, (text4 == "借") ? num5 : ((text4 == "贷") ? num6 : 0m)));
			}
			C1.Win.C1FlexGrid.Column column5 = grdBalance.Cols["BeginDC"];
			column5.Caption = "期初余额方向";
			column5.DataType = typeof(string);
			column5.Format = null;
			column5.TextAlign = TextAlignEnum.CenterCenter;
			C1.Win.C1FlexGrid.Column column6 = grdBalance.Cols["BeginBalance"];
			column6.Caption = "期初余额";
			C1.Win.C1FlexGrid.Column column7 = grdBalance.Cols["EndDC"];
			column7.Caption = "期末余额方向";
			column7.DataType = typeof(string);
			column7.Format = null;
			column7.TextAlign = TextAlignEnum.CenterCenter;
			C1.Win.C1FlexGrid.Column column8 = grdBalance.Cols["EndBalance"];
			column8.Caption = "期末余额";
			foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<string, decimal, string, decimal>> item2 in dictionary2)
			{
				C1.Win.C1FlexGrid.Row key2 = item2.Key;
				key2["BeginDC"] = item2.Value.Item1;
				key2["BeginBalance"] = item2.Value.Item2;
				key2["EndDC"] = item2.Value.Item3;
				key2["EndBalance"] = item2.Value.Item4;
			}
			_currentDisplayStyle = newStyle;
		}
	}

	public void GenerateTotal()
	{
		Point scrollPosition = grdBalance.ScrollPosition;
		try
		{
			RemoveTotal();
			if (grdBalance.Tree.MaximumLevel > 0)
			{
				return;
			}
			C1.Win.C1FlexGrid.Row row = GetTotalRow();
			if (row == null)
			{
				row = grdBalance.Rows.AddNode(0).Row;
				row.UserData = "tag_total_row";
				(row.Style ?? row.StyleNew).BackColor = Color.LightYellow;
				C1.Win.C1FlexGrid.CellStyle cellStyle = grdBalance.Styles.Add("textCenterStyle");
				cellStyle.TextAlign = TextAlignEnum.CenterCenter;
				grdBalance.SetCellStyle(row.Index, grdBalance.Cols["Code"].Index, cellStyle);
			}
			decimal num = default(decimal);
			decimal num2 = default(decimal);
			decimal num3 = default(decimal);
			decimal num4 = default(decimal);
			decimal num5 = default(decimal);
			decimal num6 = default(decimal);
			for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows[i];
				num += (decimal.TryParse(row2["Debit"]?.ToString(), out var result) ? result : 0m);
				num2 += (decimal.TryParse(row2["Credit"]?.ToString(), out var result2) ? result2 : 0m);
				if (_currentDisplayStyle == BalanceDisplayStyleEnum.NONE_DIRECTION)
				{
					num5 += (decimal.TryParse(row2["EndDC"]?.ToString(), out var result3) ? result3 : 0m);
					num6 += (decimal.TryParse(row2["EndBalance"]?.ToString(), out var result4) ? result4 : 0m);
					num3 += (decimal.TryParse(row2["BeginDC"]?.ToString(), out var result5) ? result5 : 0m);
					num4 += (decimal.TryParse(row2["BeginBalance"]?.ToString(), out var result6) ? result6 : 0m);
				}
			}
			row["Code"] = "合计";
			row["Debit"] = num;
			row["Credit"] = num2;
			if (_currentDisplayStyle == BalanceDisplayStyleEnum.NONE_DIRECTION)
			{
				row["BeginDC"] = num3;
				row["BeginBalance"] = num4;
				row["EndDC"] = num5;
				row["EndBalance"] = num6;
			}
			else
			{
				row["BeginDC"] = null;
				row["BeginBalance"] = null;
				row["EndDC"] = null;
				row["EndBalance"] = null;
			}
		}
		finally
		{
			grdBalance.ScrollPosition = scrollPosition;
		}
	}

	public void RemoveTotal()
	{
		for (int num = grdBalance.Rows.Count - 1; num >= grdBalance.Rows.Fixed; num--)
		{
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows[num];
			if ("tag_total_row".Equals(row.UserData))
			{
				grdBalance.Rows.Remove(row);
				break;
			}
		}
	}

	private C1.Win.C1FlexGrid.Row GetTotalRow()
	{
		for (int num = grdBalance.Rows.Count - 1; num >= grdBalance.Rows.Fixed; num--)
		{
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows[num];
			if ("tag_total_row".Equals(row.UserData))
			{
				return row;
			}
		}
		return null;
	}
}
