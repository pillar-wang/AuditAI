extern alias CrawlerModelAlias;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using LedgerImport;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.LedgerView.Properties;

namespace Auditai.UI.LedgerView;

public class frmImport : C1RibbonForm
{
	private delegate void InvokeDelegate();

	private enum DirectionStyleEnum
	{
		HAVE_DIRECTION,
		NONE_DIRECTION
	}

	private enum VoucherDisplayStyleEnum
	{
		TYPE_NUMBER_SPLIT,
		TYPE_NUMBER_UNIFY
	}

	private enum AuxiliaryDataMode
	{
		MergeBySplitChar,
		TypeMultiColumn
	}

	private enum AuxCodeNameStyleEnum
	{
		CODE_NAME_SPLIT,
		CODE_NAME_UNIFY
	}

	private class SourceConfig
	{
		public VoucherDisplayStyleEnum VoucherDisplayStyle;

		public DirectionStyleEnum BalanceAmountDirectionStyle = DirectionStyleEnum.NONE_DIRECTION;

		public DirectionStyleEnum VoucherAmountDirectionStyle = DirectionStyleEnum.NONE_DIRECTION;

		public DirectionStyleEnum AuxiliaryAmountDirectionStyle = DirectionStyleEnum.NONE_DIRECTION;

		public bool HasAuxiliary;

		public AuxiliaryDataMode AuxiliaryDataMode;

		public AuxCodeNameStyleEnum AuxCodeNameStyle;
	}

	private enum AuxStyle
	{
		None,
		SingleColumn,
		ThreeColumns,
		MultiColumnsSplit,
		MultiColumnsUnsplit
	}

	private const string CN_CODE = "kmdm";

	private const string CN_NAME = "kmmc";

	private const string CN_INDEX = "index";

	private const string CN_DATE = "date";

	private const string CN_TYPE = "type";

	private const string CN_NUMBER = "number";

	private const string CN_DIGEST = "digest";

	private const string CN_DEBIT = "debit";

	private const string CN_CREDIT = "credit";

	private const string CN_AUXTYPE = "auxtype";

	private const string CN_AUXCODE = "auxcode";

	private const string CN_AUXNAME = "auxname";

	private const string CN_AUXDIRE = "auxdire";

	private const string CN_AUXBEGIN = "auxbegin";

	internal const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	private List<ValidateResult2> ValidateResults = new List<ValidateResult2>();

	private readonly C1ContextMenu ctxCellBalance = new C1ContextMenu();

	private readonly C1ContextMenu ctxCellVoucher = new C1ContextMenu();

	private readonly C1ContextMenu ctxCellAux = new C1ContextMenu();

	private readonly C1ContextMenu ctxEmptyBalance = new C1ContextMenu();

	private readonly C1ContextMenu ctxEmptyVoucher = new C1ContextMenu();

	private readonly C1ContextMenu ctxEmptyAux = new C1ContextMenu();

	private readonly C1Command _cmdCopyBalance;

	private readonly C1CommandLink _lnkCopyBalance;

	private readonly C1Command _cmdCopyVoucher;

	private readonly C1CommandLink _lnkCopyVoucher;

	private readonly C1Command _cmdCopyAux;

	private readonly C1CommandLink _lnkCopyAux;

	private readonly C1Command _cmdPasteBalance;

	private readonly C1CommandLink _lnkPasteBalance;

	private readonly C1Command _cmdPasteVoucher;

	private readonly C1CommandLink _lnkPasteVoucher;

	private readonly C1Command _cmdPasteAux;

	private readonly C1CommandLink _lnkPasteAux;

	private readonly C1Command _cmdAppendRowBalance;

	private readonly C1CommandLink _lnkAppendRowBalance;

	private readonly C1Command _cmdAppendRowVoucher;

	private readonly C1CommandLink _lnkAppendRowVoucher;

	private readonly C1Command _cmdAppendRowAux;

	private readonly C1CommandLink _lnkAppendRowAux;

	private C1ContextMenu ctxFixedCol = new C1ContextMenu();

	private C1ContextMenu ctxFixedHeader = new C1ContextMenu();

	private C1CommandLink lnkInsertRow = new C1CommandLink();

	private C1Command cmdInsertRow = new C1Command();

	private C1CommandLink lnkAppendRow = new C1CommandLink();

	private C1Command cmdAppendRow = new C1Command();

	private C1CommandLink lnkDeleteRow = new C1CommandLink();

	private C1Command cmdDeleteRow = new C1Command();

	private C1CommandLink lnkAddAuxColumn = new C1CommandLink();

	private C1Command cmdAddAuxColumn = new C1Command();

	private C1CommandLink lnkDeleteAuxColumn = new C1CommandLink();

	private C1Command cmdDelteAuxColumn = new C1Command();

	private C1CommandLink lnkModifyAuxColumn = new C1CommandLink();

	private C1Command cmdModifyAuxColumn = new C1Command();

	private GridCommandsManager commandManagerBalance;

	private GridCommandsManager commandManagerVoucher;

	private GridCommandsManager commandManagerAuxiliary;

	private C1FlexGridEx HotGrid;

	private readonly C1TextBoxEx _dateEdit = new C1TextBoxEx();

	private readonly RibbonImageProcess ImageProcess = new RibbonImageProcess();

	private bool _hasGenerated;

	private AuxStyle _auxStyle;

	private const string ITEM_NONE_DIRECTION = "借贷分列式";

	private const string ITEM_HAVE_DIRECTION = "借贷标记式";

	private SourceConfig sourceConfig = new SourceConfig();

	private const string TAG_TOTAL = "tag_total";

	private TableFindFactory tableReplaceFactory = new TableFindFactory();

	private bool pendingEditEvent;

	private TooltipBox _tooltip = new TooltipBox
	{
		IsBalloon = true
	};

	private bool displayFilltip;

	private static List<string> AuxStyleDisplay = new List<string> { "单列样式", "三列样式", "多列样式（代码名称分列）", "多列样式（代码名称不分列）" };

#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlDatas;

	private C1DockingTab dockTab;

	private C1DockingTabPage tabBalance;

	private C1FlexGridEx grdBalance;

	private C1DockingTabPage tabVoucher;

	private C1FlexGridEx grdVoucher;

	private C1DockingTabPage tabAuxiliary;

	private C1FlexGridEx grdAuxiliary;

	private C1SplitterPanel pnlTools;

	private C1CommandHolder c1CommandHolder1;

	private C1CommandDock c1CommandDock1;

	private C1ToolBar c1ToolBar1;

	private C1CommandLink lnkGenerate;

	private C1Command cmdGenerate;

	private C1Command cmdFilltip;

	private C1Command cmdValidate;

	private C1CommandLink lnkValidate;

	private C1CommandLink lnkFilltip;

	private C1SplitterPanel pnlInput;

	private C1CheckBox ckbVoucherHasAuxiliary;

	private C1TextBox txtCurrency;

	private C1TextBox txtCompany;

	private C1Label lblCurrency;

	private C1Label lblCompany;

	private C1CheckBox ckbTypeNumSplit;

	private C1CheckBox ckbBalanceNonDirection;

	private C1CheckBox ckbBalanceHasAuxiliary;

	private C1Command cmdHelpDoc;

	private C1CommandLink lnkHelpDoc;

	private C1Command cmdReplace;

	private C1CommandLink lnkReplace;

	private C1CheckBox ckbAuxCodeNameSplit;

	private C1CheckBox ckbAuxMultiColumn;

	private C1DockingTab dockTabInput;

	private C1DockingTabPage tabPageInputBoxBalance;

	private C1DockingTabPage tabPageInputBoxAuxiliary;

	private C1CheckBox ckbAuxiliaryNonDirection;

	private C1DockingTabPage tabPageInputBoxVoucher;

	private C1CheckBox ckbVoucherNonDirection;

	private C1CheckBox ckbAuxPageHasAuxiliary;

	private C1ComboBox cboAuxStyle;

	private Color validateErrorBackColor => UserSet.Config.TableStyle.CheckFailColor;

	public string SavePath { get; private set; }

	public event EventHandler<string> AfterGenerateSuccess;

	public frmImport()
	{
		InitializeComponent();
		base.Shown += FrmImport_Shown;
		ckbTypeNumSplit.Checked = true;
		ckbTypeNumSplit.CheckedChanged += ckbTypeNumSplit_CheckedChanged;
		ckbBalanceNonDirection.Checked = true;
		ckbBalanceNonDirection.CheckedChanged += ckbBalanceNonDirection_CheckedChanged;
		ckbVoucherNonDirection.Checked = true;
		ckbVoucherNonDirection.CheckedChanged += CkbVoucherNonDirection_CheckedChanged;
		ckbAuxiliaryNonDirection.Checked = true;
		ckbAuxiliaryNonDirection.CheckedChanged += CkbAuxiliaryNonDirection_CheckedChanged;
		ckbVoucherHasAuxiliary.Checked = false;
		ckbAuxPageHasAuxiliary.Checked = false;
		ckbBalanceHasAuxiliary.Checked = false;
		ckbVoucherHasAuxiliary.CheckedChanged += ckbVoucherHasAuxiliary_CheckedChanged;
		ckbAuxPageHasAuxiliary.CheckedChanged += CkbAuxPageHasAuxiliary_CheckedChanged;
		ckbBalanceHasAuxiliary.CheckedChanged += CkbBalanceHasAuxiliary_CheckedChanged;
		ckbAuxMultiColumn.Checked = false;
		ckbAuxMultiColumn.Visible = false;
		ckbAuxMultiColumn.CheckedChanged += CkbAuxMultiColumn_CheckedChanged;
		ckbAuxCodeNameSplit.Checked = true;
		ckbAuxCodeNameSplit.Visible = false;
		ckbAuxCodeNameSplit.CheckedChanged += CkbAuxCodeNameSplit_CheckedChanged;
		base.FormClosing += FrmImport_FormClosing;
		txtCurrency.TextDetached = true;
		txtCurrency.Text = "人民币元";
		displayFilltip = true;
		cmdFilltip.Checked = true;
		cmdFilltip.CheckAutoToggle = true;
		if (_dateEdit.EditFormat != null)
		{
			_dateEdit.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
			_dateEdit.EditFormat.CustomFormat = "yyyy-MM-dd";
		}
		InitializeBalance();
		InitializeVoucher();
		InitializeAuxiliary();
		dockTabInput.BorderStyle = BorderStyle.None;
		dockTabInput.ShowTabs = false;
		sourceConfig.BalanceAmountDirectionStyle = DirectionStyleEnum.NONE_DIRECTION;
		sourceConfig.VoucherAmountDirectionStyle = DirectionStyleEnum.NONE_DIRECTION;
		sourceConfig.AuxiliaryAmountDirectionStyle = DirectionStyleEnum.NONE_DIRECTION;
		sourceConfig.VoucherDisplayStyle = VoucherDisplayStyleEnum.TYPE_NUMBER_SPLIT;
		sourceConfig.HasAuxiliary = false;
		HideAuxiliary();
		grdBalance.KeyDown += Grd_KeyDown;
		grdVoucher.KeyDown += Grd_KeyDown;
		grdAuxiliary.KeyDown += Grd_KeyDown;
		grdVoucher.SetupEditor += GrdVoucher_SetupEditor;
		grdBalance.AfterEdit += GrdBalance_AfterEdit;
		grdVoucher.AfterEdit += GrdVoucher_AfterEdit;
		grdBalance.MouseClick += _grid_MouseClick;
		grdVoucher.MouseClick += _grid_MouseClick;
		grdAuxiliary.MouseClick += _grid_MouseClick;
		grdBalance.AfterResizeRow += GrdBalance_AfterResizeRow;
		grdVoucher.AfterResizeRow += GrdVoucher_AfterResizeRow;
		grdAuxiliary.AfterResizeRow += GrdAuxiliary_AfterResizeRow;
		grdBalance.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdBalance.DrawFormBorder(e1.Graphics);
		};
		grdVoucher.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdVoucher.DrawFormBorder(e1.Graphics);
		};
		grdAuxiliary.Paint += delegate(object s1, PaintEventArgs e1)
		{
			grdAuxiliary.DrawFormBorder(e1.Graphics);
		};
		grdBalance.Rows.Count = 20;
		grdVoucher.Rows.Count = 20;
		grdAuxiliary.Rows.Count = 20;
		ReGenerateBalanceTotal();
		ReGenerateVoucherTotal();
		PopulateIndex(grdBalance);
		PopulateIndex(grdVoucher);
		PopulateIndex(grdAuxiliary);
		grdBalance.AutoSizeCol(0);
		AutoSizeCols(grdBalance);
		grdVoucher.AutoSizeCol(0);
		AutoSizeCols(grdVoucher);
		grdAuxiliary.AutoSizeCol(0);
		AutoSizeCols(grdAuxiliary);
		HotGrid = grdBalance;
		dockTab.SelectedTabChanged += DockTab_SelectedTabChanged;
		_cmdCopyBalance = new C1Command
		{
			Text = "复制",
			Image = ContextResources.ctxCopy
		};
		_cmdCopyBalance.Click += _cmdCopyBalance_Click;
		_lnkCopyBalance = new C1CommandLink(_cmdCopyBalance);
		ctxCellBalance.CommandLinks.Add(_lnkCopyBalance);
		_cmdPasteBalance = new C1Command
		{
			Text = "粘贴",
			Image = ContextResources.ctxPaste
		};
		_cmdPasteBalance.Click += _cmdPasteBalance_Click;
		_lnkPasteBalance = new C1CommandLink(_cmdPasteBalance);
		ctxCellBalance.CommandLinks.Add(_lnkPasteBalance);
		ctxCellBalance.CommandLinks.Add(grdBalance.FilterManager.GenLnkFilter());
		ctxCellBalance.CommandLinks.Add(grdBalance.FilterManager.GenLnkSelect());
		ctxCellBalance.CommandLinks.Add(grdBalance.FilterManager.GenLnkCancelCurrentColumn());
		_cmdCopyVoucher = new C1Command
		{
			Text = "复制",
			Image = ContextResources.ctxCopy
		};
		_cmdCopyVoucher.Click += _cmdCopyVoucher_Click;
		_lnkCopyVoucher = new C1CommandLink(_cmdCopyVoucher);
		ctxCellVoucher.CommandLinks.Add(_lnkCopyVoucher);
		_cmdPasteVoucher = new C1Command
		{
			Text = "粘贴",
			Image = ContextResources.ctxPaste
		};
		_cmdPasteVoucher.Click += _cmdPasteVoucher_Click;
		_lnkPasteVoucher = new C1CommandLink(_cmdPasteVoucher);
		ctxCellVoucher.CommandLinks.Add(_lnkPasteVoucher);
		ctxCellVoucher.CommandLinks.Add(grdVoucher.FilterManager.GenLnkFilter());
		ctxCellVoucher.CommandLinks.Add(grdVoucher.FilterManager.GenLnkSelect());
		ctxCellVoucher.CommandLinks.Add(grdVoucher.FilterManager.GenLnkCancelCurrentColumn());
		_cmdCopyAux = new C1Command
		{
			Text = "复制",
			Image = ContextResources.ctxCopy
		};
		_cmdCopyAux.Click += _cmdCopyAux_Click;
		_lnkCopyAux = new C1CommandLink(_cmdCopyAux);
		ctxCellAux.CommandLinks.Add(_lnkCopyAux);
		_cmdPasteAux = new C1Command
		{
			Text = "粘贴",
			Image = ContextResources.ctxPaste
		};
		_cmdPasteAux.Click += _cmdPasteAux_Click;
		_lnkPasteAux = new C1CommandLink(_cmdPasteAux);
		ctxCellAux.CommandLinks.Add(_lnkPasteAux);
		ctxCellAux.CommandLinks.Add(grdAuxiliary.FilterManager.GenLnkFilter());
		ctxCellAux.CommandLinks.Add(grdAuxiliary.FilterManager.GenLnkSelect());
		ctxCellAux.CommandLinks.Add(grdAuxiliary.FilterManager.GenLnkCancelCurrentColumn());
		cmdInsertRow.Text = "插入行...";
		lnkInsertRow.Command = cmdInsertRow;
		cmdInsertRow.Click += CmdInsertRow_Click;
		cmdInsertRow.Image = ContextResources.ctxInsertRow;
		ctxFixedCol.CommandLinks.Add(lnkInsertRow);
		cmdAppendRow.Text = "追加行...";
		lnkAppendRow.Command = cmdAppendRow;
		cmdAppendRow.Click += CmdAppendRow_Click;
		cmdAppendRow.Image = ContextResources.ctxAppendRow;
		ctxFixedCol.CommandLinks.Add(lnkAppendRow);
		cmdDeleteRow.Text = "删除行";
		lnkDeleteRow.Command = cmdDeleteRow;
		cmdDeleteRow.Click += CmdDeleteRow_Click;
		cmdDeleteRow.Image = ContextResources.ctxDeleteRow;
		ctxFixedCol.CommandLinks.Add(lnkDeleteRow);
		_cmdAppendRowBalance = new C1Command
		{
			Text = "追加行...",
			Image = ContextResources.ctxAppendRow
		};
		_cmdAppendRowBalance.Click += _cmdAppendRowBalance_Click;
		_lnkAppendRowBalance = new C1CommandLink(_cmdAppendRowBalance);
		ctxEmptyBalance.CommandLinks.Add(_lnkAppendRowBalance);
		ctxEmptyBalance.CommandLinks.Add(grdBalance.FilterManager.GenLnkCancelAll());
		_cmdAppendRowVoucher = new C1Command
		{
			Text = "追加行...",
			Image = ContextResources.ctxAppendRow
		};
		_cmdAppendRowVoucher.Click += _cmdAppendRowVoucher_Click;
		_lnkAppendRowVoucher = new C1CommandLink(_cmdAppendRowVoucher);
		ctxEmptyVoucher.CommandLinks.Add(_lnkAppendRowVoucher);
		ctxEmptyVoucher.CommandLinks.Add(grdVoucher.FilterManager.GenLnkCancelAll());
		_cmdAppendRowAux = new C1Command
		{
			Text = "追加行...",
			Image = ContextResources.ctxAppendRow
		};
		_cmdAppendRowAux.Click += _cmdAppendRowAux_Click;
		_lnkAppendRowAux = new C1CommandLink(_cmdAppendRowAux);
		ctxEmptyAux.CommandLinks.Add(_lnkAppendRowAux);
		ctxEmptyAux.CommandLinks.Add(grdAuxiliary.FilterManager.GenLnkCancelAll());
		cmdAddAuxColumn.Text = "添加辅助核算类别列";
		lnkAddAuxColumn.Command = cmdAddAuxColumn;
		cmdAddAuxColumn.Click += CmdAddAuxColumn_Click;
		cmdAddAuxColumn.CommandStateQuery += CmdAddAuxColumn_CommandStateQuery;
		cmdModifyAuxColumn.Text = "修改辅助核算类别列";
		lnkModifyAuxColumn.Command = cmdModifyAuxColumn;
		cmdModifyAuxColumn.Click += CmdModifyAuxColumn_Click;
		cmdModifyAuxColumn.CommandStateQuery += CmdModifyAuxColumn_CommandStateQuery;
		cmdDelteAuxColumn.Text = "删除辅助核算类别列";
		lnkDeleteAuxColumn.Command = cmdDelteAuxColumn;
		cmdDelteAuxColumn.Click += CmdDelteAuxColumn_Click;
		cmdDelteAuxColumn.CommandStateQuery += CmdDelteAuxColumn_CommandStateQuery;
		ctxFixedHeader.CommandLinks.Add(lnkAddAuxColumn);
		ctxFixedHeader.CommandLinks.Add(lnkModifyAuxColumn);
		ctxFixedHeader.CommandLinks.Add(lnkDeleteAuxColumn);
		foreach (C1CommandLink commandLink in c1ToolBar1.CommandLinks)
		{
			ImageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		commandManagerBalance = new GridCommandsManager(grdBalance);
		commandManagerVoucher = new GridCommandsManager(grdVoucher);
		commandManagerAuxiliary = new GridCommandsManager(grdAuxiliary);
		cboAuxStyle.DropDownStyle = DropDownStyle.DropDownList;
		cboAuxStyle.Items.AddRange(AuxStyleDisplay);
		cboAuxStyle.SelectedIndex = 1;
		cboAuxStyle.Visible = false;
		cboAuxStyle.SelectedIndexChanged += CboAuxStyle_SelectedIndexChanged;
	}

	private void _cmdAppendRowAux_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("追加行", "请输入追加行数：");
		if (num.HasValue)
		{
			grdAuxiliary.Rows.Add((int)num.Value);
			PopulateIndex(grdAuxiliary);
			grdAuxiliary.AutoSizeCol(0);
		}
	}

	private void _cmdAppendRowVoucher_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("追加行", "请输入追加行数：");
		if (num.HasValue)
		{
			grdVoucher.Rows.Add((int)num.Value);
			ReGenerateVoucherTotal();
			PopulateIndex(grdVoucher);
			grdVoucher.AutoSizeCol(0);
		}
	}

	private void _cmdAppendRowBalance_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("追加行", "请输入追加行数：");
		if (num.HasValue)
		{
			grdBalance.Rows.Add((int)num.Value);
			ReGenerateBalanceTotal();
			PopulateIndex(grdBalance);
			grdBalance.AutoSizeCol(0);
		}
	}

	private async void _cmdPasteAux_Click(object sender, ClickEventArgs e)
	{
		await CmdPaste_Click(grdAuxiliary);
	}

	private void _cmdCopyAux_Click(object sender, ClickEventArgs e)
	{
		grdAuxiliary.Copy();
	}

	private async void _cmdPasteVoucher_Click(object sender, ClickEventArgs e)
	{
		await CmdPaste_Click(grdVoucher);
	}

	private void _cmdCopyVoucher_Click(object sender, ClickEventArgs e)
	{
		grdVoucher.Copy();
	}

	private async void _cmdPasteBalance_Click(object sender, ClickEventArgs e)
	{
		await CmdPaste_Click(grdBalance);
	}

	private void _cmdCopyBalance_Click(object sender, ClickEventArgs e)
	{
		grdBalance.Copy();
	}

	private void CboAuxStyle_SelectedIndexChanged(object sender, EventArgs e)
	{
		_auxStyle = (AuxStyle)(cboAuxStyle.SelectedIndex + 1);
	}

	private void FrmImport_Shown(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.OpenExcelLedger);
	}

	private void CmdModifyAuxColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdVoucher.Col;
		if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.TypeMultiColumn && col > 0 && col < grdVoucher.Cols.Count && grdVoucher.Cols[col].UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
		{
			cmdModifyAuxColumn.Text = "修改" + tuple.Item2 + "类别列";
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdModifyAuxColumn_Click(object sender, ClickEventArgs e)
	{
		int col = grdVoucher.Col;
		if (col <= 0 || col >= grdVoucher.Cols.Count || !(grdVoucher.Cols[col].UserData is Tuple<string, string> tuple) || (!(tuple.Item1 == "auxcode") && !(tuple.Item1 == "auxname")))
		{
			return;
		}
		string text = InputForm.Text("修改辅助核算类别", "请输入辅助核算类别名称：", tuple.Item2);
		if (string.IsNullOrWhiteSpace(text) || !(text != tuple.Item2))
		{
			return;
		}
		for (int i = grdVoucher.Cols.Fixed; i < grdVoucher.Cols.Count; i++)
		{
			C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[i];
			if (!(column.UserData is Tuple<string, string> tuple2) || (!(tuple2.Item1 == "auxcode") && !(tuple2.Item1 == "auxname")) || !(tuple2.Item2 == tuple.Item2))
			{
				continue;
			}
			column.UserData = Tuple.Create(tuple2.Item1, text);
			if (sourceConfig.AuxCodeNameStyle == AuxCodeNameStyleEnum.CODE_NAME_SPLIT)
			{
				if (tuple2.Item1 == "auxcode")
				{
					column.Caption = text + "代码";
				}
				else if (tuple2.Item1 == "auxname")
				{
					column.Caption = text + "名称";
				}
			}
			else if (sourceConfig.AuxCodeNameStyle == AuxCodeNameStyleEnum.CODE_NAME_UNIFY)
			{
				column.Caption = text + "代码及名称";
			}
		}
	}

	private void CmdDelteAuxColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		int col = grdVoucher.Col;
		if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.TypeMultiColumn && col > 0 && col < grdVoucher.Cols.Count && grdVoucher.Cols[col].UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
		{
			cmdDelteAuxColumn.Text = "删除" + tuple.Item2 + "类别列";
			e.Visible = true;
		}
		else
		{
			e.Visible = false;
		}
	}

	private void CmdDelteAuxColumn_Click(object sender, ClickEventArgs e)
	{
		int col = grdVoucher.Col;
		if (col > 0 && col < grdVoucher.Cols.Count && grdVoucher.Cols[col].UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
		{
			DeleteAuxColumn(tuple.Item2);
		}
	}

	private void DeleteAuxColumn(string typeName)
	{
		for (int num = grdVoucher.Cols.Count - 1; num >= grdVoucher.Cols.Fixed; num--)
		{
			if (grdVoucher.Cols[num].UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname") && tuple.Item2 == typeName)
			{
				grdVoucher.Cols.Remove(num);
			}
		}
	}

	private void CkbAuxCodeNameSplit_CheckedChanged(object sender, EventArgs e)
	{
		if (ckbAuxCodeNameSplit.Checked)
		{
			SwitchVoucherAuxCodeName(AuxCodeNameStyleEnum.CODE_NAME_SPLIT);
		}
		else
		{
			SwitchVoucherAuxCodeName(AuxCodeNameStyleEnum.CODE_NAME_UNIFY);
		}
	}

	private void CkbAuxMultiColumn_CheckedChanged(object sender, EventArgs e)
	{
		if (ckbAuxMultiColumn.Checked)
		{
			ckbAuxCodeNameSplit.Visible = true;
			SwitchVoucherAuxStyle(AuxiliaryDataMode.TypeMultiColumn);
		}
		else
		{
			ckbAuxCodeNameSplit.Visible = false;
			SwitchVoucherAuxStyle(AuxiliaryDataMode.MergeBySplitChar);
		}
	}

	private void CmdAddAuxColumn_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.TypeMultiColumn;
	}

	private void CmdAddAuxColumn_Click(object sender, ClickEventArgs e)
	{
		string text = InputForm.Text("添加辅助核算类别", "请输入辅助核算类别名称：");
		if (!string.IsNullOrWhiteSpace(text))
		{
			AppendAuxColumn(text);
		}
	}

	private void AppendAuxColumn(string typeName)
	{
		for (int num = grdVoucher.Cols.Count - 1; num >= grdVoucher.Cols.Fixed; num--)
		{
			if (grdVoucher.Cols[num].UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxname" || tuple.Item1 == "auxcode"))
			{
				InsertAuxColumn(num + 1, typeName);
				return;
			}
		}
		int index = grdVoucher.Cols["debit"].Index;
		InsertAuxColumn(index, typeName);
	}

	private void InsertAuxColumn(int start, string typeName)
	{
		if (sourceConfig.AuxCodeNameStyle == AuxCodeNameStyleEnum.CODE_NAME_SPLIT)
		{
			if (start >= grdVoucher.Cols.Count)
			{
				grdVoucher.Cols.Add(2);
				start = grdVoucher.Cols.Count - 2;
			}
			else
			{
				grdVoucher.Cols.InsertRange(start, 2);
			}
			C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[start++];
			column.Caption = typeName + "代码";
			column.Name = column.Caption;
			column.DataType = typeof(string);
			column.UserData = Tuple.Create("auxcode", typeName);
			C1.Win.C1FlexGrid.Column column2 = grdVoucher.Cols[start++];
			column2.Caption = typeName + "名称";
			column2.Name = column2.Caption;
			column2.DataType = typeof(string);
			column2.UserData = Tuple.Create("auxname", typeName);
		}
		else if (sourceConfig.AuxCodeNameStyle == AuxCodeNameStyleEnum.CODE_NAME_UNIFY)
		{
			if (start >= grdVoucher.Cols.Count)
			{
				grdVoucher.Cols.Add(1);
				start = grdVoucher.Cols.Count - 1;
			}
			else
			{
				grdVoucher.Cols.InsertRange(start, 1);
			}
			C1.Win.C1FlexGrid.Column column3 = grdVoucher.Cols[start++];
			column3.Caption = typeName + "代码及名称";
			column3.DataType = typeof(string);
			column3.UserData = Tuple.Create("auxcode", typeName);
		}
	}

	private void FrmImport_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing && !_hasGenerated && DialogResult.OK != Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "关闭后当前窗体数据将被放弃，确定要关闭吗？", MessageBoxButtons.OKCancel))
		{
			e.Cancel = true;
		}
	}

	private void GrdVoucher_SetupEditor(object sender, RowColEventArgs e)
	{
		object obj = grdVoucher.Rows[e.Row][e.Col];
		if (obj == null)
		{
			_dateEdit.Value = DateTime.Now;
		}
	}

	private void GrdVoucher_AfterResizeRow(object sender, RowColEventArgs e)
	{
		ResizeRow(grdVoucher, grdVoucher.Rows[e.Row].Height);
	}

	private void ResizeRow(C1FlexGrid grid, int height)
	{
		grid.BeginUpdate();
		try
		{
			for (int i = 0; i < grid.Rows.Count; i++)
			{
				grid.Rows[i].Height = height;
			}
		}
		finally
		{
			grid.EndUpdate();
		}
	}

	private void GrdAuxiliary_AfterResizeRow(object sender, RowColEventArgs e)
	{
		ResizeRow(grdAuxiliary, grdAuxiliary.Rows[e.Row].Height);
	}

	private void GrdBalance_AfterResizeRow(object sender, RowColEventArgs e)
	{
		ResizeRow(grdBalance, grdBalance.Rows[e.Row].Height);
	}

	public new DialogResult ShowDialog()
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		return base.ShowDialog();
	}

	public new void Show()
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		base.Show();
	}

	private void SetTheme()
	{
		grdBalance.Styles.Fixed.Border.Color = Color.DarkGray;
		grdVoucher.Styles.Fixed.Border.Color = Color.DarkGray;
		grdAuxiliary.Styles.Fixed.Border.Color = Color.DarkGray;
		AuditaiTheme selectedAuditaiTheme = Auditai.UI.Controls.Theme.SelectedAuditaiTheme;
		if (selectedAuditaiTheme != null && selectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			ImageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			ImageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		ImageProcess.ProcessImage();
		dockTabInput.ShowTabs = false;
		dockTabInput.BorderStyle = BorderStyle.None;
		foreach (C1DockingTabPage tabPage in dockTabInput.TabPages)
		{
			tabPage.BackColor = ((ckbBalanceNonDirection.BackColor.A == 0) ? pnlInput.BackColor : ckbBalanceNonDirection.BackColor);
		}
	}

	private void GrdVoucher_AfterEdit(object sender, RowColEventArgs e)
	{
		if (!pendingEditEvent)
		{
			ReGenerateVoucherTotal();
		}
	}

	private void GrdBalance_AfterEdit(object sender, RowColEventArgs e)
	{
		if (!pendingEditEvent)
		{
			ReGenerateBalanceTotal();
		}
	}

	private void GrdBalance_AppendTotalRow()
	{
		if (grdBalance.Rows.Count <= 20000)
		{
			LedgerBuilder3 ledgerBuilder = new LedgerBuilder3();
			ledgerBuilder.DataSource = new DataSource
			{
				BalanceTable = ConvertFromBalance()
			};
			CrawlerModelAlias::Auditai.Model.Ledger accounts = ledgerBuilder.GetAccounts("人民币");
			decimal num = accounts.Accounts.Where((CrawlerModelAlias::Auditai.Model.Account a) => a.Children.Count == 0 && a.IsDebit).Sum((CrawlerModelAlias::Auditai.Model.Account a) => a.Balance);
			decimal num2 = accounts.Accounts.Where((CrawlerModelAlias::Auditai.Model.Account a) => a.Children.Count == 0 && !a.IsDebit).Sum((CrawlerModelAlias::Auditai.Model.Account a) => a.Balance);
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows.Add();
			row.AllowEditing = false;
			row.UserData = "tag_total";
			row["kmdm"] = "末级科目合计";
			object value = ((sourceConfig.BalanceAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION) ? null : ((object)num));
			object value2 = ((sourceConfig.BalanceAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION) ? null : ((object)num2));
			row["debit"] = value;
			row["credit"] = value2;
			C1.Win.C1FlexGrid.CellStyle cellStyle = grdBalance.Styles.Add("total");
			cellStyle.TextAlign = TextAlignEnum.CenterCenter;
			grdBalance.SetCellStyle(row.Index, "kmdm", cellStyle);
			(row.Style ?? row.StyleNew).BackColor = Color.LightYellow;
		}
	}

	private void GrdBalance_RemoveTotalRow()
	{
		commandManagerBalance?.StartPendding();
		try
		{
			for (int num = grdBalance.Rows.Count - 1; num >= grdBalance.Rows.Fixed; num--)
			{
				C1.Win.C1FlexGrid.Row row = grdBalance.Rows[num];
				if (row.UserData?.ToString() == "tag_total")
				{
					grdBalance.Rows.Remove(row);
					break;
				}
			}
		}
		finally
		{
			commandManagerBalance?.EndPendding();
		}
	}

	private void ReGenerateBalanceTotal()
	{
		commandManagerBalance?.StartPendding();
		try
		{
			grdBalance.BeginUpdate();
			try
			{
				Point point = grdBalance.ScrollPosition;
				GrdBalance_RemoveTotalRow();
				GrdBalance_AppendTotalRow();
				grdBalance.ScrollPosition = point;
			}
			finally
			{
				grdBalance.EndUpdate();
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			commandManagerBalance?.EndPendding();
		}
	}

	private void GrdVoucher_AppendTotalRow()
	{
		decimal num = default(decimal);
		decimal num2 = default(decimal);
		for (int i = grdVoucher.Rows.Fixed; i < grdVoucher.Rows.Count; i++)
		{
			num += (decimal.TryParse(grdVoucher.Rows[i]["debit"]?.ToString(), out var result) ? result : 0m);
			num2 += (decimal.TryParse(grdVoucher.Rows[i]["credit"]?.ToString(), out var result2) ? result2 : 0m);
		}
		C1.Win.C1FlexGrid.Row row = grdVoucher.Rows.Add();
		row.AllowEditing = false;
		row.UserData = "tag_total";
		object value = ((sourceConfig.VoucherAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION) ? null : ((object)num));
		object value2 = ((sourceConfig.VoucherAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION) ? null : ((object)num2));
		row["debit"] = value;
		row["credit"] = value2;
		C1.Win.C1FlexGrid.CellStyle cellStyle = grdVoucher.Styles.Add("total");
		cellStyle.DataType = typeof(string);
		cellStyle.TextAlign = TextAlignEnum.CenterCenter;
		grdVoucher.SetCellStyle(row.Index, "date", cellStyle);
		(row.Style ?? row.StyleNew).BackColor = Color.LightYellow;
		row["date"] = "合计";
	}

	private void GrdVoucher_RemoveTotalRow()
	{
		commandManagerVoucher?.StartPendding();
		try
		{
			for (int num = grdVoucher.Rows.Count - 1; num >= grdVoucher.Rows.Fixed; num--)
			{
				C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[num];
				if (row.UserData?.ToString() == "tag_total")
				{
					grdVoucher.Rows.Remove(row);
					break;
				}
			}
		}
		finally
		{
			commandManagerVoucher?.EndPendding();
		}
	}

	private void ReGenerateVoucherTotal()
	{
		commandManagerVoucher?.StartPendding();
		try
		{
			grdVoucher.BeginUpdate();
			try
			{
				Point point = grdVoucher.ScrollPosition;
				GrdVoucher_RemoveTotalRow();
				GrdVoucher_AppendTotalRow();
				grdVoucher.ScrollPosition = point;
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		finally
		{
			commandManagerVoucher?.EndPendding();
		}
	}

	private void _grid_MouseClick(object sender, MouseEventArgs e)
	{
		if (!(sender is C1FlexGrid c1FlexGrid))
		{
			return;
		}
		if (e.Button == MouseButtons.Left)
		{
			if (c1FlexGrid.Row >= 0 && c1FlexGrid.Col >= 0)
			{
				ShowValidatetip(c1FlexGrid, c1FlexGrid.Row, c1FlexGrid.Col);
			}
			else
			{
				_tooltip.Hide();
			}
		}
		else
		{
			if (e.Button != MouseButtons.Right)
			{
				return;
			}
			_tooltip.Hide();
			switch (c1FlexGrid.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.Cell:
				if (c1FlexGrid == grdBalance)
				{
					ctxCellBalance.ShowContextMenu(c1FlexGrid, e.Location);
				}
				else if (c1FlexGrid == grdVoucher)
				{
					ctxCellVoucher.ShowContextMenu(c1FlexGrid, e.Location);
				}
				else if (c1FlexGrid == grdAuxiliary)
				{
					ctxCellAux.ShowContextMenu(c1FlexGrid, e.Location);
				}
				break;
			case HitTestTypeEnum.None:
				if (c1FlexGrid == grdBalance)
				{
					ctxEmptyBalance.ShowContextMenu(c1FlexGrid, e.Location);
				}
				else if (c1FlexGrid == grdVoucher)
				{
					ctxEmptyVoucher.ShowContextMenu(c1FlexGrid, e.Location);
				}
				else if (c1FlexGrid == grdAuxiliary)
				{
					ctxEmptyAux.ShowContextMenu(c1FlexGrid, e.Location);
				}
				break;
			case HitTestTypeEnum.RowHeader:
				if (c1FlexGrid.MouseRow >= c1FlexGrid.Rows.Fixed)
				{
					ctxFixedCol.ShowContextMenu(c1FlexGrid, e.Location);
				}
				break;
			case HitTestTypeEnum.ColumnHeader:
				if (c1FlexGrid == grdVoucher)
				{
					ctxFixedHeader.ShowContextMenu(c1FlexGrid, e.Location);
				}
				break;
			case HitTestTypeEnum.ColumnResize:
			case HitTestTypeEnum.ColumnFreeze:
				break;
			}
		}
	}

	private void DockTab_SelectedTabChanged(object sender, EventArgs e)
	{
		if (dockTab.SelectedTab == tabBalance)
		{
			HotGrid = grdBalance;
			HotGrid.Focus();
			dockTabInput.SelectedTab = tabPageInputBoxBalance;
			ckbBalanceNonDirection.Checked = sourceConfig.BalanceAmountDirectionStyle == DirectionStyleEnum.NONE_DIRECTION;
		}
		else if (dockTab.SelectedTab == tabVoucher)
		{
			HotGrid = grdVoucher;
			HotGrid.Focus();
			dockTabInput.SelectedTab = tabPageInputBoxVoucher;
			ckbBalanceNonDirection.Checked = sourceConfig.VoucherAmountDirectionStyle == DirectionStyleEnum.NONE_DIRECTION;
		}
		else if (dockTab.SelectedTab == tabAuxiliary)
		{
			HotGrid = grdAuxiliary;
			HotGrid.Focus();
			dockTabInput.SelectedTab = tabPageInputBoxAuxiliary;
			ckbBalanceNonDirection.Checked = sourceConfig.AuxiliaryAmountDirectionStyle == DirectionStyleEnum.NONE_DIRECTION;
		}
		else
		{
			HotGrid = null;
		}
		_tooltip.Hide();
	}

	private void CmdDeleteRow_Click(object sender, ClickEventArgs e)
	{
		if (HotGrid == null)
		{
			return;
		}
		HotGrid.BeginUpdate();
		C1.Win.C1FlexGrid.CellRange selection = HotGrid.Selection;
		for (int num = selection.BottomRow; num >= selection.TopRow; num--)
		{
			if (HotGrid.Rows[num].Visible)
			{
				HotGrid.Rows.Remove(num);
			}
		}
		HotGrid.EndUpdate();
		if (HotGrid == grdBalance)
		{
			ReGenerateBalanceTotal();
		}
		if (HotGrid == grdVoucher)
		{
			ReGenerateVoucherTotal();
		}
		PopulateIndex(HotGrid);
		HotGrid.AutoSizeCol(0);
	}

	private void CmdAppendRow_Click(object sender, ClickEventArgs e)
	{
		decimal? num = InputForm.Numeric("追加行", "请输入追加行数：");
		if (num.HasValue && HotGrid != null)
		{
			HotGrid.Rows.Add((int)num.Value);
			if (HotGrid == grdBalance)
			{
				ReGenerateBalanceTotal();
			}
			if (HotGrid == grdVoucher)
			{
				ReGenerateVoucherTotal();
			}
			PopulateIndex(HotGrid);
			HotGrid.AutoSizeCol(0);
		}
	}

	private void CmdInsertRow_Click(object sender, ClickEventArgs e)
	{
		if (HotGrid == null || HotGrid.MouseRow < HotGrid.Rows.Fixed || HotGrid.Row >= HotGrid.Rows.Count)
		{
			return;
		}
		decimal? num = InputForm.Numeric("插入行", "请输入插入行数：");
		if (!num.HasValue || HotGrid == null)
		{
			return;
		}
		HotGrid.BeginUpdate();
		try
		{
			for (int i = 0; (decimal)i < num.Value; i++)
			{
				HotGrid.Rows.Insert(HotGrid.Row);
			}
		}
		finally
		{
			HotGrid.EndUpdate();
		}
		if (HotGrid == grdBalance)
		{
			ReGenerateBalanceTotal();
		}
		if (HotGrid == grdVoucher)
		{
			ReGenerateVoucherTotal();
		}
		PopulateIndex(HotGrid);
		HotGrid.AutoSizeCol(0);
	}

	private async Task CmdPaste_Click(C1FlexGridEx grid)
	{
		try
		{
			await PasteClipboard(grid);
		}
		catch (Win32Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		catch (Exception exception)
		{
			exception.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "复制粘贴的数据存在异常，请尝试重新复制。");
		}
	}

	private async void Grd_KeyDown(object sender, KeyEventArgs e)
	{
		if (!(sender is C1FlexGrid c1FlexGrid))
		{
			return;
		}
		try
		{
			switch (e.KeyData)
			{
			case Keys.V | Keys.Control:
				try
				{
					await PasteClipboard(c1FlexGrid);
					break;
				}
				catch (Exception exception)
				{
					exception.Log();
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "复制粘贴的数据存在异常，请尝试重新复制。");
					break;
				}
			case Keys.C | Keys.Control:
				c1FlexGrid.Copy();
				break;
			case Keys.X | Keys.Control:
				CutSelection(c1FlexGrid);
				break;
			case Keys.Delete:
				DeleteSelection(c1FlexGrid);
				break;
			case Keys.Z | Keys.Control:
			{
				GridCommandsManager commandsManager2 = GetCommandsManager(c1FlexGrid);
				if (commandsManager2 != null && commandsManager2.CanUndo)
				{
					commandsManager2.Undo();
				}
				if (c1FlexGrid == grdBalance)
				{
					ReGenerateBalanceTotal();
				}
				break;
			}
			case Keys.Y | Keys.Control:
			{
				GridCommandsManager commandsManager = GetCommandsManager(c1FlexGrid);
				if (commandsManager != null && commandsManager.CanRedo)
				{
					commandsManager.Redo();
				}
				break;
			}
			case Keys.F | Keys.Control:
				ShowReplaceForm(replace: false);
				break;
			case Keys.H | Keys.Control:
				ShowReplaceForm(replace: true);
				break;
			}
		}
		catch (Exception exception2)
		{
			exception2.Log();
		}
	}

	private void ShowReplaceForm(bool replace)
	{
		if (HotGrid != null)
		{
			TableFindInstance tableFindInstance = tableReplaceFactory.Get();
			tableFindInstance.HideScopeControls();
			tableFindInstance.FindNextHandler += Frf_FindNextHandler;
			tableFindInstance.ReplaceHandler += Frf_ReplaceHandler;
			if (replace)
			{
				tableFindInstance.ShowReplace();
			}
			else
			{
				tableFindInstance.ShowFind();
			}
		}
	}

	private void Frf_FindNextHandler(object sender, FindNextEventArgs e)
	{
		if (HotGrid == null || HotGrid.BodyRow < 0 || HotGrid.BodyCol < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = HotGrid.Selection;
		if (selection.IsSingleCell)
		{
			int row = HotGrid.Row;
			int col = HotGrid.Col;
			for (int i = row; i < HotGrid.Rows.Count; i++)
			{
				if (!HotGrid.Rows[i].Visible)
				{
					continue;
				}
				for (int j = HotGrid.Cols.Fixed; j < HotGrid.Cols.Count; j++)
				{
					if (HotGrid.Cols[j].Visible && (i != row || j > col))
					{
						string text = HotGrid.GetData(i, j)?.ToString();
						if (text != null && FindImpl(text, e.FindValue, e.MatchMode, e.IsMatchCase))
						{
							HotGrid.Select(i, j);
							return;
						}
					}
				}
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已查找到表格结尾");
			return;
		}
		for (int k = selection.TopRow; k <= selection.BottomRow; k++)
		{
			if (!HotGrid.Rows[k].Visible)
			{
				continue;
			}
			for (int l = selection.LeftCol; l <= selection.RightCol; l++)
			{
				if (HotGrid.Cols[l].Visible)
				{
					string text2 = HotGrid.GetData(k, l)?.ToString();
					if (text2 != null && FindImpl(text2, e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						HotGrid.Select(k, l);
						return;
					}
				}
			}
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选区内无查找结果");
	}

	private void Frf_ReplaceHandler(object sender, ReplaceEventArgs e)
	{
		if (HotGrid == null || HotGrid.BodyRow < 0 || HotGrid.BodyCol < 0)
		{
			return;
		}
		HotGrid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = HotGrid.Selection;
			if (e.IsReplaceAll)
			{
				if (selection.IsSingleCell)
				{
					int num = 0;
					int row = HotGrid.Row;
					int col = HotGrid.Col;
					List<GridCellInfo> list = new List<GridCellInfo>();
					for (int i = row; i < HotGrid.Rows.Count; i++)
					{
						if (!HotGrid.Rows[i].Visible)
						{
							continue;
						}
						for (int j = HotGrid.Cols.Fixed; j < HotGrid.Cols.Count; j++)
						{
							if (!HotGrid.Cols[j].Visible || (i == row && j < col))
							{
								continue;
							}
							object data = HotGrid.GetData(i, j);
							if (data != null && FindImpl(data.ToString(), e.FindValue, e.MatchMode, e.IsMatchCase))
							{
								if (e.ReplaceMode == ReplaceMode.AllText)
								{
									string replaceValue = e.ReplaceValue;
									HotGrid.SetData(i, j, replaceValue);
									list.Add(new GridCellInfo(HotGrid, i, j, data, replaceValue));
								}
								else if (e.ReplaceMode == ReplaceMode.MatchText)
								{
									string text = data.ToString().Replace(e.FindValue, e.ReplaceValue);
									HotGrid.SetData(i, j, text);
									list.Add(new GridCellInfo(HotGrid, i, j, data, text));
								}
								num++;
							}
						}
					}
					if (list.Count > 0)
					{
						GetCommandsManager(HotGrid).NewCommand(new GridBatchCellUpdateValueCommand(list));
					}
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"共替换 {num} 处。");
					return;
				}
				int num2 = 0;
				for (int k = selection.TopRow; k <= selection.BottomRow; k++)
				{
					if (!HotGrid.Rows[k].Visible)
					{
						continue;
					}
					for (int l = selection.LeftCol; l <= selection.RightCol; l++)
					{
						if (!HotGrid.Cols[l].Visible)
						{
							continue;
						}
						string text2 = HotGrid.GetData(k, l)?.ToString();
						if (text2 != null && FindImpl(text2, e.FindValue, e.MatchMode, e.IsMatchCase))
						{
							if (e.ReplaceMode == ReplaceMode.AllText)
							{
								HotGrid.SetData(k, l, e.ReplaceValue);
							}
							else if (e.ReplaceMode == ReplaceMode.MatchText)
							{
								HotGrid.SetData(k, l, text2.Replace(e.FindValue, e.ReplaceValue));
							}
							num2++;
						}
					}
				}
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"共替换 {num2} 处。");
				return;
			}
			if (selection.IsSingleCell)
			{
				int row2 = HotGrid.Row;
				int col2 = HotGrid.Col;
				for (int m = row2; m < HotGrid.Rows.Count; m++)
				{
					if (!HotGrid.Rows[m].Visible)
					{
						continue;
					}
					for (int n = HotGrid.Cols.Fixed; n < HotGrid.Cols.Count; n++)
					{
						if (!HotGrid.Cols[n].Visible || (m == row2 && n <= col2))
						{
							continue;
						}
						object data2 = HotGrid.GetData(m, n);
						if (data2 != null && FindImpl(data2.ToString(), e.FindValue, e.MatchMode, e.IsMatchCase))
						{
							HotGrid.Select(m, n);
							if (e.ReplaceMode == ReplaceMode.AllText)
							{
								string replaceValue2 = e.ReplaceValue;
								HotGrid.SetData(m, n, replaceValue2);
								GetCommandsManager(HotGrid).NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(HotGrid, m, n, data2, replaceValue2)));
							}
							else if (e.ReplaceMode == ReplaceMode.MatchText)
							{
								string text3 = data2.ToString().Replace(e.FindValue, e.ReplaceValue);
								HotGrid.SetData(m, n, text3);
								GetCommandsManager(HotGrid).NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(HotGrid, m, n, data2, text3)));
							}
							return;
						}
					}
				}
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已查找到表格结尾");
				return;
			}
			for (int num3 = selection.TopRow; num3 <= selection.BottomRow; num3++)
			{
				if (!HotGrid.Rows[num3].Visible)
				{
					continue;
				}
				for (int num4 = selection.LeftCol; num4 <= selection.RightCol; num4++)
				{
					if (!HotGrid.Cols[num4].Visible)
					{
						continue;
					}
					object data3 = HotGrid.GetData(num3, num4);
					if (data3 != null && FindImpl(data3.ToString(), e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						HotGrid.Select(num3, num4);
						if (e.ReplaceMode == ReplaceMode.AllText)
						{
							string replaceValue3 = e.ReplaceValue;
							HotGrid.SetData(num3, num4, replaceValue3);
							GetCommandsManager(HotGrid).NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(HotGrid, num3, num4, data3, replaceValue3)));
						}
						else if (e.ReplaceMode == ReplaceMode.MatchText)
						{
							string text4 = data3.ToString().Replace(e.FindValue, e.ReplaceValue);
							HotGrid.SetData(num3, num4, text4);
							GetCommandsManager(HotGrid).NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(HotGrid, num3, num4, data3, text4)));
						}
						return;
					}
				}
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选区内无查找结果");
		}
		finally
		{
			HotGrid.EndUpdate();
		}
	}

	private bool FindImpl(string src, string findWhat, MatchMode mode, bool caseSensitive)
	{
		StringComparison comparisonType = (caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
		return mode switch
		{
			MatchMode.Exact => src.Equals(findWhat, comparisonType), 
			MatchMode.Start => src.StartsWith(findWhat, comparisonType), 
			MatchMode.End => src.EndsWith(findWhat, comparisonType), 
			MatchMode.Any => src.IndexOf(findWhat, comparisonType) >= 0, 
			_ => throw new ArgumentOutOfRangeException("mode"), 
		};
	}

	private void InitializeBalance()
	{
		grdBalance.BeginUpdate();
		try
		{
			grdBalance.Rows.Count = 1;
			grdBalance.Rows.Fixed = 1;
			grdBalance.Cols.Count = 1;
			grdBalance.Cols.Fixed = 1;
			grdBalance.Rows.DefaultSize = 30;
			grdBalance.AllowResizing = AllowResizingEnum.Both;
			C1.Win.C1FlexGrid.Column column = grdBalance.Cols[0];
			column.Name = "index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grdBalance.Cols.Add();
			column.Name = "kmdm";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column = grdBalance.Cols.Add();
			column.Name = "kmmc";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column = grdBalance.Cols.Add();
			column.Name = "debit";
			column.Caption = "年初借方余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdBalance.Cols.Add();
			column.Name = "credit";
			column.Caption = "年初贷方余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
		}
		finally
		{
			grdBalance.EndUpdate();
		}
	}

	private void InitializeVoucher()
	{
		grdVoucher.BeginUpdate();
		try
		{
			grdVoucher.Rows.Count = 1;
			grdVoucher.Rows.Fixed = 1;
			grdVoucher.Cols.Count = 1;
			grdVoucher.Cols.Fixed = 1;
			grdVoucher.Rows.DefaultSize = 30;
			grdVoucher.AllowResizing = AllowResizingEnum.Both;
			C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[0];
			column.Name = "index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grdVoucher.Cols.Add();
			column.Name = "date";
			column.Caption = "日期";
			column.DataType = typeof(DateTime);
			column.Format = "yyyy-MM-dd";
			(column.Style ?? column.StyleNew).Editor = _dateEdit;
			column = grdVoucher.Cols.Add();
			column.Name = "type";
			column.Caption = "凭证字";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "number";
			column.Caption = "凭证号";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "kmdm";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "kmmc";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "auxtype";
			column.Caption = "辅助核算类别";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "auxcode";
			column.Caption = "辅助核算代码";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "auxname";
			column.Caption = "辅助核算名称";
			column.DataType = typeof(string);
			column = grdVoucher.Cols.Add();
			column.Name = "debit";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdVoucher.Cols.Add();
			column.Name = "credit";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void InitializeAuxiliary()
	{
		grdAuxiliary.BeginUpdate();
		try
		{
			grdAuxiliary.Rows.Count = 1;
			grdAuxiliary.Rows.Fixed = 1;
			grdAuxiliary.Cols.Count = 1;
			grdAuxiliary.Cols.Fixed = 1;
			grdAuxiliary.Rows.DefaultSize = 30;
			grdAuxiliary.AllowResizing = AllowResizingEnum.Both;
			C1.Win.C1FlexGrid.Column column = grdAuxiliary.Cols[0];
			column.Name = "index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grdAuxiliary.Cols.Add();
			column.Name = "kmdm";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column = grdAuxiliary.Cols.Add();
			column.Name = "kmmc";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column = grdAuxiliary.Cols.Add();
			column.Name = "auxtype";
			column.Caption = "辅助核算类别";
			column.DataType = typeof(string);
			column = grdAuxiliary.Cols.Add();
			column.Name = "auxcode";
			column.Caption = "辅助核算代码";
			column.DataType = typeof(string);
			column = grdAuxiliary.Cols.Add();
			column.Name = "auxname";
			column.Caption = "辅助核算名称";
			column.DataType = typeof(string);
			column = grdAuxiliary.Cols.Add();
			column.Name = "debit";
			column.Caption = "年初借方余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdAuxiliary.Cols.Add();
			column.Name = "credit";
			column.Caption = "年初贷方余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
		}
		finally
		{
			grdAuxiliary.EndUpdate();
		}
	}

	private void PopulateIndex(C1FlexGrid grid)
	{
		if (grid == null)
		{
			return;
		}
		grid.BeginUpdate();
		try
		{
			GridCommandsManager commandsManager = GetCommandsManager(grid);
			commandsManager?.StartPendding();
			try
			{
				for (int i = grid.Rows.Fixed; i < grid.Rows.Count; i++)
				{
					grid.Cols[0][i] = i.ToString();
				}
			}
			finally
			{
				commandsManager?.EndPendding();
			}
		}
		finally
		{
			grid.EndUpdate();
		}
	}

	private void AutoSizeCols(C1FlexGrid grid)
	{
		grid.AutoSizeCols(0, grid.Cols.Count - 1, 10);
		for (int i = grid.Cols.Fixed; i < grid.Cols.Count; i++)
		{
			if (grid.Cols[i].Width < 120)
			{
				grid.Cols[i].Width = 120;
			}
		}
	}

	private async Task PasteClipboard(C1FlexGrid grid)
	{
		GridCommandsManager commandManager = GetCommandsManager(grid);
		List<GridCellInfo> cellInfos = new List<GridCellInfo>();
		bool oldPendding = pendingEditEvent;
		if (!oldPendding)
		{
			pendingEditEvent = true;
		}
		try
		{
			grid.BeginUpdate();
			try
			{
				if (grid == grdBalance)
				{
					GrdBalance_RemoveTotalRow();
				}
				if (grid == grdVoucher)
				{
					GrdVoucher_RemoveTotalRow();
				}
			}
			finally
			{
				grid.EndUpdate();
			}
			try
			{
				int pasteLastRow = -1;
				ProgressForm<List<List<object>>> progressForm = new ProgressForm<List<List<object>>>(delegate
				{
					Application.DoEvents();
					return new ProgressInfo
					{
						MainCaption = "正在进行粘贴准备，可能时间较长，请耐心等待...",
						MainProgress = ((ClipboardUtil.IsStreamReady && ClipboardUtil.RowsCount > 0) ? ((int)((double)ClipboardUtil.RowsCountAlreadyRead * 100.0 / (double)ClipboardUtil.RowsCount)) : 0)
					};
				}, () => Task.Run(() => ClipboardUtil.GetClipboardAsTable()), TimeSpan.FromSeconds(1.0));
				progressForm.ShowDialog();
				List<List<object>> clipValue = await progressForm.Task;
				grid.BeginUpdate();
				if (clipValue == null)
				{
					return;
				}
				if (clipValue.Count == 1 && clipValue[0].Count == 1 && !grid.Selection.IsSingleCell)
				{
					object item = clipValue[0][0];
					clipValue = new List<List<object>>();
					int num = 0;
					for (int i = grid.Selection.TopRow; i <= grid.Selection.BottomRow; i++)
					{
						if (grid.Rows[i].Visible)
						{
							num++;
						}
					}
					int num2 = 0;
					for (int j = grid.Selection.LeftCol; j <= grid.Selection.RightCol; j++)
					{
						if (grid.Cols[j].Visible)
						{
							num2++;
						}
					}
					for (int k = 0; k < num; k++)
					{
						List<object> list = new List<object>();
						for (int l = 0; l < num2; l++)
						{
							list.Add(item);
						}
						clipValue.Add(list);
					}
				}
				if (clipValue.Count > 5000)
				{
					int currentProgress = 0;
					int totalProgress = clipValue.Count + 2;
					ProgressForm<object> progressForm2 = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
					{
						iProg.Report(new ProgressInfo
						{
							MainCaption = "正在执行粘贴，请稍候...",
							MainProgress = ++currentProgress * 100 / totalProgress
						});
						Application.DoEvents();
						await Task.Delay(100);
						int num5 = clipValue.Count - (grid.Rows.Count - grid.Row);
						if (num5 > 0)
						{
							grid.Rows.Count += num5 + 1;
						}
						int num6 = grid.Row;
						int col2 = grid.Col;
						foreach (List<object> item2 in clipValue)
						{
							int num7 = currentProgress + 1;
							currentProgress = num7;
							if (currentProgress % 10000 == 0)
							{
								iProg.Report(new ProgressInfo
								{
									MainCaption = "正在执行粘贴，请稍候...",
									MainProgress = currentProgress * 100 / totalProgress
								});
							}
							Application.DoEvents();
							for (; num6 < grid.Rows.Count && !grid.Rows[num6].Visible; num6++)
							{
							}
							if (num6 >= grid.Rows.Count)
							{
								grid.Rows.Count++;
							}
							int num8 = col2;
							foreach (object item3 in item2)
							{
								for (; num8 < grid.Cols.Count && !grid.Cols[num8].Visible; num8++)
								{
								}
								if (num8 >= grid.Cols.Count)
								{
									break;
								}
								object obj2 = item3;
								if (grid.Cols[num8].DataType == typeof(decimal))
								{
									decimal result2;
									decimal num9 = (decimal.TryParse(item3?.ToString(), out result2) ? result2 : 0m);
									obj2 = Math.Round(num9, 2, MidpointRounding.AwayFromZero);
								}
								else if (grid.Cols[num8].DataType == typeof(DateTime))
								{
									obj2 = ParseDate(item3);
								}
								object oldValue2 = grid[num6, num8];
								grid.SetData(num6, num8, obj2);
								cellInfos.Add(new GridCellInfo(grid, num6, num8, oldValue2, obj2));
								num8++;
							}
							num6++;
						}
						pasteLastRow = num6;
						iProg.Report(new ProgressInfo
						{
							MainCaption = "正在执行粘贴，请稍候...",
							MainProgress = ++currentProgress * 100 / totalProgress
						});
						Application.DoEvents();
						commandManager.NewCommand(new GridBatchCellUpdateValueCommand(cellInfos));
						PopulateIndex(HotGrid);
						return Task.FromResult(new object());
					});
					progressForm2.ShowDialog();
					await progressForm2.Task;
				}
				else
				{
					int num3 = clipValue.Count - (grid.Rows.Count - grid.Row);
					if (num3 > 0)
					{
						grid.Rows.Count += num3 + 1;
					}
					int m = grid.Row;
					int col = grid.Col;
					foreach (List<object> item4 in clipValue)
					{
						for (; m < grid.Rows.Count && !grid.Rows[m].Visible; m++)
						{
						}
						if (m >= grid.Rows.Count)
						{
							grid.Rows.Count++;
						}
						int n = col;
						foreach (object item5 in item4)
						{
							for (; n < grid.Cols.Count && !grid.Cols[n].Visible; n++)
							{
							}
							if (n < grid.Cols.Count)
							{
								object obj = item5;
								if (grid.Cols[n].DataType == typeof(decimal))
								{
									decimal result;
									decimal num4 = (decimal.TryParse(item5?.ToString(), out result) ? result : 0m);
									obj = Math.Round(num4, 2, MidpointRounding.AwayFromZero);
								}
								else if (grid.Cols[n].DataType == typeof(DateTime))
								{
									obj = ParseDate(item5);
								}
								object oldValue = grid[m, n];
								grid.SetData(m, n, obj);
								cellInfos.Add(new GridCellInfo(grid, m, n, oldValue, obj));
								n++;
								continue;
							}
							break;
						}
						m++;
					}
					pasteLastRow = m;
					commandManager.NewCommand(new GridBatchCellUpdateValueCommand(cellInfos));
					PopulateIndex(HotGrid);
				}
				if (pasteLastRow != -1)
				{
					grid.ShowCell((pasteLastRow < grid.Rows.Count) ? pasteLastRow : (grid.Rows.Count - 1), 1);
				}
			}
			catch (TableModelException ex)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
			}
			finally
			{
				grid.EndUpdate();
			}
			if (grid == grdBalance)
			{
				ReGenerateBalanceTotal();
			}
			if (grid == grdVoucher)
			{
				ReGenerateVoucherTotal();
			}
		}
		finally
		{
			if (!oldPendding)
			{
				pendingEditEvent = false;
			}
		}
	}

	private GridCommandsManager GetCommandsManager(C1FlexGrid grid)
	{
		if (grid == grdBalance)
		{
			return commandManagerBalance;
		}
		if (grid == grdVoucher)
		{
			return commandManagerVoucher;
		}
		if (grid == grdAuxiliary)
		{
			return commandManagerAuxiliary;
		}
		return null;
	}

	private static string GetClipboardText(string clip)
	{
		try
		{
			clip = Clipboard.GetText();
		}
		catch (ExternalException)
		{
			try
			{
				clip = Clipboard.GetText();
			}
			catch (ExternalException)
			{
				clip = Clipboard.GetText();
			}
		}
		return clip;
	}

	private void DeleteSelection(C1FlexGrid grid)
	{
		GridCommandsManager commandsManager = GetCommandsManager(grid);
		List<GridCellInfo> list = new List<GridCellInfo>();
		grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = grid.Selection;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				if (!grid.Rows[i].Visible)
				{
					continue;
				}
				for (int j = selection.LeftCol; j <= selection.RightCol; j++)
				{
					if (grid.Cols[j].Visible)
					{
						object oldValue = grid[i, j];
						grid[i, j] = null;
						list.Add(new GridCellInfo(grid, i, j, oldValue, null));
					}
				}
			}
			commandsManager?.NewCommand(new GridBatchCellUpdateValueCommand(list));
			if (HotGrid == grdBalance)
			{
				ReGenerateBalanceTotal();
			}
			if (HotGrid == grdVoucher)
			{
				ReGenerateVoucherTotal();
			}
		}
		finally
		{
			grid.EndUpdate();
		}
	}

	private void CutSelection(C1FlexGrid grid)
	{
		GridCommandsManager commandsManager = GetCommandsManager(grid);
		List<GridCellInfo> list = new List<GridCellInfo>();
		C1.Win.C1FlexGrid.CellRange selection = grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				object oldValue = grid[i, j];
				list.Add(new GridCellInfo(grid, i, j, oldValue, null));
			}
		}
		commandsManager?.NewCommand(new GridBatchCellUpdateValueCommand(list));
		grid.Copy();
		grid.Selection.Clear(ClearFlags.Content);
		if (HotGrid == grdBalance)
		{
			ReGenerateBalanceTotal();
		}
		if (HotGrid == grdVoucher)
		{
			ReGenerateVoucherTotal();
		}
	}

	private LedgerImport.DataTable ConvertFromBalance()
	{
		LedgerImport.DataTable dataTable = new LedgerImport.DataTable();
		DataColumn dataColumn = dataTable.Columns.Add("kmdm");
		dataColumn.Caption = "科目代码";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdBalance.Cols["kmdm"]);
		dataColumn = dataTable.Columns.Add("kmmc");
		dataColumn.Caption = "科目名称";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdBalance.Cols["kmmc"]);
		dataColumn = dataTable.Columns.Add("debit");
		dataColumn.Caption = "年初借方余额";
		dataColumn.DataType = typeof(decimal);
		dataTable.SetTag(dataColumn, grdBalance.Cols["debit"]);
		dataColumn = dataTable.Columns.Add("credit");
		dataColumn.Caption = "年初贷方余额";
		dataColumn.DataType = typeof(decimal);
		dataTable.SetTag(dataColumn, grdBalance.Cols["credit"]);
		if (sourceConfig.BalanceAmountDirectionStyle == DirectionStyleEnum.NONE_DIRECTION)
		{
			for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
				if (!(row.UserData?.ToString() == "tag_total") && RowHasValue(row))
				{
					DataRow dataRow = dataTable.Rows.Add();
					dataRow["kmdm"] = row["kmdm"] ?? DBNull.Value;
					dataRow["kmmc"] = row["kmmc"] ?? DBNull.Value;
					dataRow["debit"] = row["debit"] ?? DBNull.Value;
					dataRow["credit"] = row["credit"] ?? DBNull.Value;
					dataTable.SetTag(dataRow, row);
				}
			}
		}
		else if (sourceConfig.BalanceAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION)
		{
			for (int j = grdBalance.Rows.Fixed; j < grdBalance.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows[j];
				if (!(row2.UserData?.ToString() == "tag_total") && RowHasValue(row2))
				{
					DataRow dataRow2 = dataTable.Rows.Add();
					dataRow2["kmdm"] = row2["kmdm"] ?? DBNull.Value;
					dataRow2["kmmc"] = row2["kmmc"] ?? DBNull.Value;
					string text = row2["credit"]?.ToString();
					string text2 = row2["debit"]?.ToString();
					decimal result;
					decimal num = (decimal.TryParse(text, out result) ? result : 0m);
					if (num != 0m && (text2 == null || (!text2.Contains("借") && !text2.Contains("贷"))))
					{
						throw new ValidateException
						{
							FailureReason = ValidateErrorTypeEnum.BalanceDirectionUnset,
							Grid = grdBalance,
							Row = row2,
							Col = grdBalance.Cols["debit"]
						};
					}
					bool flag = text2?.Contains("借") ?? false;
					dataRow2["debit"] = (flag ? num : 0m);
					dataRow2["credit"] = (flag ? 0m : num);
					dataTable.SetTag(dataRow2, row2);
				}
			}
		}
		return dataTable;
	}

	private LedgerImport.DataTable ConvertFromVoucher()
	{
		LedgerImport.DataTable dataTable = new LedgerImport.DataTable();
		DataColumn dataColumn = dataTable.Columns.Add("date");
		dataColumn.Caption = "日期";
		dataColumn.DataType = typeof(DateTime);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["date"]);
		dataColumn = dataTable.Columns.Add("type");
		dataColumn.Caption = "凭证字";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["type"]);
		dataColumn = dataTable.Columns.Add("number");
		dataColumn.Caption = "凭证号";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["number"]);
		dataColumn = dataTable.Columns.Add("digest");
		dataColumn.Caption = "摘要";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["digest"]);
		dataColumn = dataTable.Columns.Add("kmdm");
		dataColumn.Caption = "科目代码";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["kmdm"]);
		dataColumn = dataTable.Columns.Add("kmmc");
		dataColumn.Caption = "科目名称";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["kmmc"]);
		if (sourceConfig.HasAuxiliary)
		{
			dataColumn = dataTable.Columns.Add("auxtype");
			dataColumn.Caption = "辅助核算类别";
			dataColumn.DataType = typeof(string);
			dataTable.SetTag(dataColumn, grdVoucher.Cols["auxtype"]);
			dataColumn = dataTable.Columns.Add("auxcode");
			dataColumn.Caption = "辅助核算代码";
			dataColumn.DataType = typeof(string);
			dataTable.SetTag(dataColumn, grdVoucher.Cols["auxcode"]);
			dataColumn = dataTable.Columns.Add("auxname");
			dataColumn.Caption = "辅助核算名称";
			dataColumn.DataType = typeof(string);
			dataTable.SetTag(dataColumn, grdVoucher.Cols["auxname"]);
		}
		dataColumn = dataTable.Columns.Add("debit");
		dataColumn.Caption = "借方金额";
		dataColumn.DataType = typeof(decimal);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["debit"]);
		dataColumn = dataTable.Columns.Add("credit");
		dataColumn.Caption = "贷方金额";
		dataColumn.DataType = typeof(decimal);
		dataTable.SetTag(dataColumn, grdVoucher.Cols["credit"]);
		for (int i = grdVoucher.Rows.Fixed; i < grdVoucher.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[i];
			if (row.UserData?.ToString() == "tag_total" || !RowHasValue(row))
			{
				continue;
			}
			DataRow dataRow = dataTable.Rows.Add();
			dataRow["date"] = row["date"] ?? DBNull.Value;
			dataRow["digest"] = row["digest"] ?? DBNull.Value;
			dataRow["kmdm"] = row["kmdm"] ?? DBNull.Value;
			dataRow["kmmc"] = row["kmmc"] ?? DBNull.Value;
			if (sourceConfig.VoucherDisplayStyle == VoucherDisplayStyleEnum.TYPE_NUMBER_SPLIT)
			{
				dataRow["type"] = row["type"] ?? DBNull.Value;
				dataRow["number"] = row["number"] ?? DBNull.Value;
			}
			else if (sourceConfig.VoucherDisplayStyle == VoucherDisplayStyleEnum.TYPE_NUMBER_UNIFY)
			{
				string typeNumber = row["type"]?.ToString();
				SplitTypeNumber(typeNumber, out var type, out var number);
				dataRow["type"] = type;
				dataRow["number"] = number;
			}
			if (sourceConfig.VoucherAmountDirectionStyle == DirectionStyleEnum.NONE_DIRECTION)
			{
				dataRow["debit"] = row["debit"] ?? DBNull.Value;
				dataRow["credit"] = row["credit"] ?? DBNull.Value;
				dataTable.SetTag(dataRow, row);
			}
			else if (sourceConfig.VoucherAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION)
			{
				bool valueOrDefault = (row["debit"]?.ToString()?.Contains("借")).GetValueOrDefault();
				decimal result;
				decimal num = (decimal.TryParse(row["credit"]?.ToString(), out result) ? result : 0m);
				dataRow["debit"] = (valueOrDefault ? num : 0m);
				dataRow["credit"] = (valueOrDefault ? 0m : num);
				dataTable.SetTag(dataRow, row);
			}
			if (!sourceConfig.HasAuxiliary)
			{
				continue;
			}
			if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.MergeBySplitChar)
			{
				dataRow["auxtype"] = row["auxtype"] ?? DBNull.Value;
				dataRow["auxcode"] = row["auxcode"] ?? DBNull.Value;
				dataRow["auxname"] = row["auxname"] ?? DBNull.Value;
			}
			else
			{
				if (sourceConfig.AuxiliaryDataMode != AuxiliaryDataMode.TypeMultiColumn)
				{
					continue;
				}
				Dictionary<Tuple<string, string>, string> dictionary = new Dictionary<Tuple<string, string>, string>();
				for (int j = grdVoucher.Cols.Fixed; j < grdVoucher.Cols.Count; j++)
				{
					C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[j];
					if (column.UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
					{
						dictionary.Add(tuple, row[column.Index]?.ToString());
					}
				}
				if (sourceConfig.AuxCodeNameStyle == AuxCodeNameStyleEnum.CODE_NAME_SPLIT)
				{
					IEnumerable<IGrouping<string, KeyValuePair<Tuple<string, string>, string>>> enumerable = from kv in dictionary
						group kv by kv.Key.Item2;
					List<string> list = new List<string>();
					List<string> list2 = new List<string>();
					List<string> list3 = new List<string>();
					foreach (IGrouping<string, KeyValuePair<Tuple<string, string>, string>> item in enumerable)
					{
						string value = item.First((KeyValuePair<Tuple<string, string>, string> kv) => kv.Key.Item1 == "auxcode").Value;
						string value2 = item.First((KeyValuePair<Tuple<string, string>, string> kv) => kv.Key.Item1 == "auxname").Value;
						if (!string.IsNullOrEmpty(value) || !string.IsNullOrEmpty(value2))
						{
							list.Add(item.Key);
							list2.Add(value);
							list3.Add(value2);
						}
					}
					if (list.Count > 0)
					{
						dataRow["auxtype"] = string.Join("|", list);
						dataRow["auxcode"] = string.Join("|", list2);
						dataRow["auxname"] = string.Join("|", list3);
					}
				}
				else
				{
					if (sourceConfig.AuxCodeNameStyle != AuxCodeNameStyleEnum.CODE_NAME_UNIFY)
					{
						continue;
					}
					List<string> list4 = new List<string>();
					List<string> list5 = new List<string>();
					List<string> list6 = new List<string>();
					foreach (KeyValuePair<Tuple<string, string>, string> item2 in dictionary)
					{
						string text = item2.Value?.Trim();
						if (text != null)
						{
							string[] array = text.Split(LedgerBuilder3.splitChars, StringSplitOptions.RemoveEmptyEntries);
							if (array.Length > 1)
							{
								list4.Add(item2.Key.Item2);
								string text2 = array[0];
								list5.Add(text2);
								list6.Add((array.Length == 2) ? array[1] : text.Remove(0, text2.Length).Trim(LedgerBuilder3.splitChars));
							}
							else
							{
								list4.Add(item2.Key.Item2);
								list5.Add(string.Empty);
								list6.Add(text);
							}
						}
					}
					if (list4.Count > 0)
					{
						dataRow["auxtype"] = string.Join("|", list4);
						dataRow["auxcode"] = string.Join("|", list5);
						dataRow["auxname"] = string.Join("|", list6);
					}
				}
			}
		}
		return dataTable;
	}

	private LedgerImport.DataTable ConvertFromAuxiliary()
	{
		if (!sourceConfig.HasAuxiliary)
		{
			return null;
		}
		LedgerImport.DataTable dataTable = new LedgerImport.DataTable();
		DataColumn dataColumn = dataTable.Columns.Add("kmdm");
		dataColumn.Caption = "科目代码";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdAuxiliary.Cols["kmdm"]);
		dataColumn = dataTable.Columns.Add("kmmc");
		dataColumn.Caption = "科目名称";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdAuxiliary.Cols["kmmc"]);
		dataColumn = dataTable.Columns.Add("auxtype");
		dataColumn.Caption = "辅助核算类别";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdAuxiliary.Cols["auxtype"]);
		dataColumn = dataTable.Columns.Add("auxcode");
		dataColumn.Caption = "辅助核算代码";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdAuxiliary.Cols["auxcode"]);
		dataColumn = dataTable.Columns.Add("auxname");
		dataColumn.Caption = "辅助核算名称";
		dataColumn.DataType = typeof(string);
		dataTable.SetTag(dataColumn, grdAuxiliary.Cols["auxname"]);
		dataColumn = dataTable.Columns.Add("debit");
		dataColumn.Caption = "年初借方余额";
		dataColumn.DataType = typeof(decimal);
		dataTable.SetTag(dataColumn, grdAuxiliary.Cols["debit"]);
		dataColumn = dataTable.Columns.Add("credit");
		dataColumn.Caption = "年初贷方余额";
		dataColumn.DataType = typeof(decimal);
		dataTable.SetTag(dataColumn, grdAuxiliary.Cols["credit"]);
		if (sourceConfig.AuxiliaryAmountDirectionStyle == DirectionStyleEnum.NONE_DIRECTION)
		{
			for (int i = grdAuxiliary.Rows.Fixed; i < grdAuxiliary.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdAuxiliary.Rows[i];
				if (!(row.UserData?.ToString() == "tag_total") && RowHasValue(row))
				{
					DataRow dataRow = dataTable.Rows.Add();
					dataRow["kmdm"] = row["kmdm"] ?? DBNull.Value;
					dataRow["kmmc"] = row["kmmc"] ?? DBNull.Value;
					dataRow["auxtype"] = row["auxtype"] ?? DBNull.Value;
					dataRow["auxcode"] = row["auxcode"] ?? DBNull.Value;
					dataRow["auxname"] = row["auxname"] ?? DBNull.Value;
					dataRow["debit"] = row["debit"] ?? DBNull.Value;
					dataRow["credit"] = row["credit"] ?? DBNull.Value;
					dataTable.SetTag(dataRow, row);
				}
			}
		}
		else if (sourceConfig.AuxiliaryAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION)
		{
			for (int j = grdAuxiliary.Rows.Fixed; j < grdAuxiliary.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdAuxiliary.Rows[j];
				if (!(row2.UserData?.ToString() == "tag_total") && RowHasValue(row2))
				{
					DataRow dataRow2 = dataTable.Rows.Add();
					dataRow2["kmdm"] = row2["kmdm"] ?? DBNull.Value;
					dataRow2["kmmc"] = row2["kmmc"] ?? DBNull.Value;
					dataRow2["auxtype"] = row2["auxtype"] ?? DBNull.Value;
					dataRow2["auxcode"] = row2["auxcode"] ?? DBNull.Value;
					dataRow2["auxname"] = row2["auxname"] ?? DBNull.Value;
					bool valueOrDefault = (row2["debit"]?.ToString()?.Contains("借")).GetValueOrDefault();
					decimal result;
					decimal num = (decimal.TryParse(row2["credit"]?.ToString(), out result) ? result : 0m);
					dataRow2["debit"] = (valueOrDefault ? num : 0m);
					dataRow2["credit"] = (valueOrDefault ? 0m : num);
					dataTable.SetTag(dataRow2, row2);
				}
			}
		}
		return dataTable;
	}

	private bool RowHasValue(C1.Win.C1FlexGrid.Row row)
	{
		if (row.Index < 0)
		{
			return false;
		}
		C1FlexGridBase grid = row.Grid;
		for (int i = grid.Cols.Fixed; i < grid.Cols.Count; i++)
		{
			object obj = row[i];
			if (obj == null)
			{
				continue;
			}
			C1.Win.C1FlexGrid.Column column = grid.Cols[i];
			if (column.DataType == typeof(string))
			{
				if (!string.IsNullOrWhiteSpace(obj.ToString()))
				{
					return true;
				}
			}
			else if (column.DataType == typeof(DateTime))
			{
				if (!string.IsNullOrWhiteSpace(obj.ToString()))
				{
					return true;
				}
			}
			else if (column.DataType == typeof(int))
			{
				if (int.TryParse(obj.ToString(), out var result) && result != 0)
				{
					return true;
				}
			}
			else if (column.DataType == typeof(int))
			{
				if (decimal.TryParse(obj.ToString(), out var result2) && result2 != 0m)
				{
					return true;
				}
			}
			else if (!string.IsNullOrWhiteSpace(obj.ToString()))
			{
				return true;
			}
		}
		return false;
	}

	private bool CellDefault(C1FlexGrid grid, int r, int c)
	{
		object obj = grid[r, c];
		if (string.IsNullOrEmpty(obj?.ToString()))
		{
			return true;
		}
		if (grid.Cols[c].DataType == typeof(int) && int.TryParse(obj.ToString(), out var result) && result == 0)
		{
			return true;
		}
		if (grid.Cols[c].DataType == typeof(decimal) && decimal.TryParse(obj.ToString(), out var result2) && result2 == 0m)
		{
			return true;
		}
		return false;
	}

	private void ShowValidatetip(Control control, Point point, string title, string body)
	{
		ShowTooltip(control, point, title, new XElement("div", body));
	}

	private void ShowValidatetip(C1FlexGrid grid, int row, int col)
	{
		Rectangle cellRect;
		if (grid == grdBalance)
		{
			cellRect = grdBalance.GetCellRect(row, col);
		}
		else if (grid == grdVoucher)
		{
			cellRect = grdVoucher.GetCellRect(row, col);
		}
		else
		{
			if (grid != grdAuxiliary)
			{
				_tooltip.Hide();
				return;
			}
			cellRect = grdAuxiliary.GetCellRect(row, col);
		}
		Point point = default(Point);
		point.X = cellRect.Right;
		point.Y = (cellRect.Top + cellRect.Bottom) / 2;
		Point point2 = point;
		XElement cellTooltipContent = GetCellTooltipContent(grid, row, col);
		if (cellTooltipContent == null)
		{
			_tooltip.Hide();
			return;
		}
		cellTooltipContent.LastNode.Remove();
		XElement xElement = cellTooltipContent.Element("b");
		xElement?.Remove();
		ShowTooltip(grid, point2, xElement?.Value ?? "", cellTooltipContent);
	}

	private void ShowTooltip(Control control, Point point, string title, XElement body)
	{
		_tooltip.Duration = -1;
		_tooltip.SetText(title, new XElement("div", new XAttribute("style", "color:red"), body).ToString());
		_tooltip.Show(control, point);
	}

	private XElement GetBalanceFilltip(int col)
	{
		XElement xElement = null;
		if (grdVoucher.Cols.Contains("kmdm") && col == grdBalance.Cols["kmdm"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "科目代码中不应含有辅助核算项目的代码。"));
		}
		else if (grdVoucher.Cols.Contains("kmmc") && col == grdBalance.Cols["kmmc"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、本表若填制的是全部级次的科目，科目名称可填制为全路径科目名称，也可仅填制为本级科目名称（如：“银行存款 - 工行存款”或“工行存款”均可）；"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、本表若填制的仅是最末级科目时，科目名称必须填制为全路径科目名称（如：“银行存款 - 工行存款”）。"));
		}
		if (xElement != null)
		{
			xElement.Add(new XElement("hr"));
			xElement.Add(new XElement("span", new XAttribute("style", "color:red"), "注："));
			xElement.Add(new XElement("br"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "本表支持填制全部级次科目，也支持仅填制最末级科目。"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "本表在填制时，对于年初余额为0的科目可不填制。"));
			return xElement;
		}
		return null;
	}

	private XElement GetVoucherFilltip(int col)
	{
		XElement xElement = null;
		if (grdVoucher.Cols.Contains("date") && col == grdVoucher.Cols["date"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "同一张凭证，日期可仅在凭证的首行填列即可。"));
		}
		else if (grdVoucher.Cols.Contains("type") && col == grdVoucher.Cols["type"].Index)
		{
			if (sourceConfig.VoucherDisplayStyle == VoucherDisplayStyleEnum.TYPE_NUMBER_SPLIT)
			{
				xElement = new XElement("div");
				xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "同一张凭证，凭证字可仅在凭证的首行填列即可。"));
			}
			else
			{
				xElement = new XElement("div");
				xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "同一张凭证，凭证字号可仅在凭证的首行填列即可。"));
			}
		}
		else if (grdVoucher.Cols.Contains("number") && col == grdVoucher.Cols["number"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "同一张凭证，凭证号可仅在凭证的首行填列即可。"));
		}
		else if (grdVoucher.Cols.Contains("kmdm") && col == grdVoucher.Cols["kmdm"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、若《年初科目余额表》中的会计科目已涵盖了会计凭证库的所有科目，则本列可不填制，若未全部涵盖，本列应进行填制；"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、若填制，科目代码必须填制为最末级。"));
		}
		else if (grdVoucher.Cols.Contains("kmmc") && col == grdVoucher.Cols["kmmc"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、年初余额为0的科目未在年初科目余额表中填制时，科目名称必须填制为全路径名称（如：“银行存款-工行存款”）；\u00a0"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、年初余额为0的科目已在年初科目余额表中全部填制时，该科目名称列可不填制。"));
		}
		else if (grdVoucher.Cols.Contains("auxtype") && col == grdVoucher.Cols["auxtype"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、对于挂接有辅助核算的科目，辅助核算类别必须填制（如：“客户”、“供应商”、“部门”、“项目”等）；"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、会计科目下挂接有多类别辅助核算的，填制时，辅助核算类别间须加“|”分割符（如：“客户 | 部门”、“供应商|项目”等）。"));
		}
		else if (grdVoucher.Cols.Contains("auxcode") && col == grdVoucher.Cols["auxcode"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、对于挂接有辅助核算的科目，辅助核算代码可填制也可不填制，若不填制，在生成账套时，将自动分配生成辅助核算代码；\u00a0"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、会计科目下挂接有多类别辅助核算的，若进行填制，辅助核算代码间须加“|”分割符（如：“KH001 | BM002”）。"));
		}
		else if (grdVoucher.Cols.Contains("auxname") && col == grdVoucher.Cols["auxname"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、对于挂接有辅助核算的科目，辅助核算名称必须填制（按客户，如：“甲公司”、“乙公司”；按部门，如：“销售一部”、“销售二部”等）；"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、会计科目下挂接有多类别辅助核算的，辅助核算名称间须加“|”分割符（按客户及部门，如：“甲公司 | 销售二部”）。"));
		}
		else if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.TypeMultiColumn && grdVoucher.Cols[col].UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "列头处点击鼠标右键可添加、修改、删除辅助核算类别。"));
		}
		if (xElement != null)
		{
			return xElement;
		}
		return null;
	}

	private XElement GetAuxiliaryFilltip(int col)
	{
		XElement xElement = null;
		if (grdVoucher.Cols.Contains("kmdm") && col == grdAuxiliary.Cols["kmdm"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "科目代码必须为最末级科目，因为辅助核算只能挂接在最末级科目；"));
		}
		else if (grdVoucher.Cols.Contains("kmmc") && col == grdAuxiliary.Cols["kmmc"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "科目名称列可不填制，生成账套时将根据年初科目余额表自动匹配科目名称。"));
		}
		else if (grdVoucher.Cols.Contains("auxtype") && col == grdAuxiliary.Cols["auxtype"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、辅助核算类别必须填制（如：“客户”、“供应商”、“部门”、“项目”等）；"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、科目下挂接多类别辅助核算的，可分行次填制，也可同行次并列填制，辅助核算类别间须加“|”分割符（如：“客户 | 部门”、“供应商 | 项目”等）。"));
		}
		else if (grdVoucher.Cols.Contains("auxcode") && col == grdAuxiliary.Cols["auxcode"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、辅助核算代码可填制，也可不填制，若不填制，在生成账套时，将自动分配生成辅助核算代码；"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、科目下挂接多类别辅助核算的，若进行填制，可分行次填制，也可同行次并列填制，辅助核算代码间须加“|”分割符（如：“KH001 | BM001”、“GYS001 | XM001”等）。"));
		}
		else if (grdVoucher.Cols.Contains("auxname") && col == grdAuxiliary.Cols["auxname"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、辅助核算名称列必须填制；"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、科目下挂接多类别辅助核算的，可分行次填制，也可同行次并列填制，辅助核算名称间须加“|”分割符（如：“甲公司 | 销售一部”、“乙公司 | A项目”等）。"));
		}
		else if (grdVoucher.Cols.Contains("debit") && col == grdAuxiliary.Cols["debit"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、每个科目辅助核算余额合计必须与《年初科目余额表》中该科目的年初余额相等"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、科目存在多类别辅助核算的，该科目的每个类别辅助核算余额合计也必须与《年初科目余额表》中该科目的年初余额相等"));
		}
		else if (grdVoucher.Cols.Contains("credit") && col == grdAuxiliary.Cols["credit"].Index)
		{
			xElement = new XElement("div");
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "1、每个科目辅助核算余额合计必须与《年初科目余额表》中该科目的年初余额相等"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "2、科目存在多类别辅助核算的，该科目的每个类别辅助核算余额合计也必须与《年初科目余额表》中该科目的年初余额相等"));
		}
		if (xElement != null)
		{
			xElement.Add(new XElement("hr"));
			xElement.Add(new XElement("span", new XAttribute("style", "color:red"), "注："));
			xElement.Add(new XElement("br"));
			xElement.Add(new XElement("p", new XAttribute("style", "color:red"), "本表在填制时，对于年初余额为0的辅助核算项可不填制。"));
			return xElement;
		}
		return null;
	}

	private XElement GetCellTooltipContent(C1FlexGrid grid, int row, int col)
	{
		XElement xElement = null;
		if (grid == grdBalance)
		{
			Rectangle cellRect = grdBalance.GetCellRect(row, col);
			if (displayFilltip)
			{
				xElement = GetBalanceFilltip(col);
			}
		}
		else if (grid == grdVoucher)
		{
			Rectangle cellRect = grdVoucher.GetCellRect(row, col);
			if (displayFilltip)
			{
				xElement = GetVoucherFilltip(col);
			}
		}
		else
		{
			if (grid != grdAuxiliary)
			{
				return null;
			}
			Rectangle cellRect = grdAuxiliary.GetCellRect(row, col);
			if (displayFilltip)
			{
				xElement = GetAuxiliaryFilltip(col);
			}
		}
		string text = null;
		ValidateResult2 validateResult = ValidateResults.Find((ValidateResult2 v) => v.C1FlexGrid == grid && v.Row.Index == row && v.Column.Index == col);
		if (validateResult != null)
		{
			text = validateResult.ErrorMessage;
		}
		if (xElement == null && text == null)
		{
			return null;
		}
		XElement xElement2 = new XElement("div");
		if (xElement != null)
		{
			xElement2.Add(new XElement("b", new XAttribute("style", "color:black"), "填表提示"));
			xElement2.Add(xElement.Elements());
			xElement2.Add(new XElement("hr"));
		}
		if (text != null)
		{
			xElement2.Add(new XElement("p", new XElement("b", new XAttribute("style", "color:black;"), "错误提示")));
			xElement2.Add(new XElement("p", text));
			xElement2.Add(new XElement("hr"));
		}
		return xElement2;
	}

	private async Task<bool> ValidateFill(bool save)
	{
		try
		{
			ResetAllObsoleteValidateMark();
			grdBalance.FilterManager.Clear();
			grdVoucher.FilterManager.Clear();
			grdAuxiliary.FilterManager.Clear();
			CrawlerModelAlias::Auditai.Model.Ledger ledger = LedgerBuilder3.EMPTY_LEDGER;
			ProgressForm<object> progressForm = new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProgress)
			{
				int totalProgress = 10;
				int currentProgress = 0;
				LedgerBuilder3 builder = new LedgerBuilder3();
				builder.ProgressChanged += delegate(object s1, string e1)
				{
					iProgress.Report(new ProgressInfo
					{
						MainCaption = e1,
						MainProgress = ++currentProgress * 100 / totalProgress
					});
					Application.DoEvents();
				};
				iProgress.Report(new ProgressInfo
				{
					MainCaption = "正在处理余额表数据",
					MainProgress = ++currentProgress * 100 / totalProgress
				});
				Application.DoEvents();
				await Task.Delay(100);
				DataSource dataSource = new DataSource
				{
					BalanceTable = ConvertFromBalance()
				};
				iProgress.Report(new ProgressInfo
				{
					MainCaption = "正在处理凭证数据",
					MainProgress = ++currentProgress * 100 / totalProgress
				});
				Application.DoEvents();
				dataSource.VoucherTable = ConvertFromVoucher();
				iProgress.Report(new ProgressInfo
				{
					MainCaption = "正在处理辅助核算数据",
					MainProgress = ++currentProgress * 100 / totalProgress
				});
				Application.DoEvents();
				dataSource.AuxiliaryTable = ConvertFromAuxiliary();
				builder.DataSource = dataSource;
				ledger = builder.GetLedger(string.IsNullOrEmpty(txtCurrency.Text) ? "人民币" : txtCurrency.Text);
				iProgress.Report(new ProgressInfo
				{
					MainCaption = "处理完成",
					MainProgress = ++currentProgress * 100 / totalProgress
				});
				Application.DoEvents();
				return Task.FromResult(new object());
			});
			progressForm.ShowDialog();
			await progressForm.Task;
			if (save)
			{
				if (string.IsNullOrEmpty(txtCompany.Text))
				{
					ShowValidatetip(txtCompany, new Point((txtCompany.Right - txtCompany.Left) / 2, 0), "错误提示", "请输入核算单位名称");
					return false;
				}
				if (string.IsNullOrEmpty(txtCurrency.Text))
				{
					ShowValidatetip(txtCurrency, new Point((txtCurrency.Right - txtCurrency.Left) / 2, 0), "错误提示", "请输入本位币");
					return false;
				}
				string text = txtCompany.Text.Trim();
				ledger.CompanyName = text;
				SaveFileDialog saveFileDialog = new SaveFileDialog
				{
					Filter = "DB文件|*.db",
					FileName = text
				};
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					ledger.SaveAsSqlite(saveFileDialog.FileName);
					SavePath = saveFileDialog.FileName;
					return true;
				}
				return false;
			}
			return true;
		}
		catch (ValidateException ex)
		{
			if (ex.FailureReason == ValidateErrorTypeEnum.BalanceDirectionUnset)
			{
				ShowValidatetipIfCanPosition(ex, "期初余额不为0时，必须设置借贷方向");
			}
		}
		catch (ImportException2 importException)
		{
			_ = importException.FailureContext?.RowTag;
			_ = importException.FailureContext?.ColTag;
			switch (importException.FailureReason)
			{
			case FailureReasonEnum.ColumnNotFound:
			{
				Tuple<string, string> tuple = importException.FailureContext.UserData as Tuple<string, string>;
				if (tuple == null)
				{
				}
				break;
			}
			case FailureReasonEnum.BalanceAccountCodeEmpty:
				ShowValidatetipIfCanPosition(importException.FailureContext, "科目代码不能为空");
				break;
			case FailureReasonEnum.VoucherAccountCodeEmpty:
				ShowValidatetipIfCanPosition(importException.FailureContext, "科目代码不能为空");
				break;
			case FailureReasonEnum.AccountNameEmpty:
				ShowValidatetipIfCanPosition(importException.FailureContext, "科目名称不能为空");
				break;
			case FailureReasonEnum.RepeatAccountCode:
				ShowValidatetipIfCanPosition(importException.FailureContext, $"({importException.FailureContext.UserData})科目代码不能重复");
				break;
			case FailureReasonEnum.NotFoundAnyAccount:
				dockTab.SelectedTab = tabBalance;
				if (grdBalance.Rows.Count > 1)
				{
					SetError(grdBalance, 1, grdBalance.Cols["kmdm"].Index, "当前没有科目信息");
				}
				ShowValidatetip(grdBalance, (grdBalance.Rows.Count > 1) ? 1 : 0, 1);
				break;
			case FailureReasonEnum.NotFoundParentAccount:
				ShowValidatetipIfCanPosition(importException.FailureContext, $"（{importException.FailureContext.UserData}）不符合规则的科目代码，未找到其上级科目");
				break;
			case FailureReasonEnum.AccountParentCannotCreateBecauseName:
				ShowValidatetipIfCanPosition(importException.FailureContext, $"（{importException.FailureContext.UserData}）未在《年初科目余额表》中发现该科目，请在《年初科目余额表》中补充添加该科目及其所有上级科目的科目代码和科目名称。");
				break;
			case FailureReasonEnum.AccountLevelCannotCreate:
			{
				int num = int.Parse(importException.FailureContext.UserData?.ToString());
				if (importException.FailureContext.Table == TableEnum.BALANCE)
				{
					C1.Win.C1FlexGrid.Column column = grdBalance.Cols["kmdm"];
					C1.Win.C1FlexGrid.Column column2 = grdBalance.Cols["kmmc"];
					bool flag2 = true;
					for (int l = grdBalance.Rows.Fixed; l < grdBalance.Rows.Count; l++)
					{
						C1.Win.C1FlexGrid.Row row5 = grdBalance.Rows[l];
						if (num == -1 || (row5["kmmc"]?.ToString()?.Split(LedgerBuilder3.splitChars, StringSplitOptions.RemoveEmptyEntries)?.Length).GetValueOrDefault(-1) == num)
						{
							SetError(grdBalance, l, column.Index, "科目代码或科目名称不规范，无法生成科目级次");
							SetError(grdBalance, l, column2.Index, "科目代码或科目名称不规范，无法生成科目级次");
							if (flag2)
							{
								dockTab.SelectedTab = tabBalance;
								ShowValidatetip(grdBalance, l, column.Index);
								flag2 = false;
							}
						}
					}
					if (flag2)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码或科目名称不规范，无法生成科目级次");
					}
				}
				else
				{
					if (importException.FailureContext.Table != TableEnum.VOUCHER)
					{
						break;
					}
					bool flag3 = true;
					C1.Win.C1FlexGrid.Column column3 = grdVoucher.Cols["kmdm"];
					C1.Win.C1FlexGrid.Column column4 = grdVoucher.Cols["kmmc"];
					for (int m = grdVoucher.Rows.Fixed; m < grdVoucher.Rows.Count; m++)
					{
						C1.Win.C1FlexGrid.Row row6 = grdVoucher.Rows[m];
						string[] array = row6["kmmc"]?.ToString()?.Split(LedgerBuilder3.splitChars, StringSplitOptions.RemoveEmptyEntries);
						if (num == -1 || ((array != null) ? array.Length : (-1)) == num)
						{
							SetError(grdVoucher, m, column3.Index, "科目代码或科目名称不规范，无法生成科目级次");
							SetError(grdVoucher, m, column4.Index, "科目代码或科目名称不规范，无法生成科目级次");
							if (flag3)
							{
								dockTab.SelectedTab = tabVoucher;
								ShowValidatetip(grdVoucher, m, column3.Index);
								flag3 = false;
							}
						}
					}
					C1.Win.C1FlexGrid.Column column5 = grdAuxiliary.Cols["kmdm"];
					C1.Win.C1FlexGrid.Column column6 = grdAuxiliary.Cols["kmmc"];
					for (int n = grdAuxiliary.Rows.Fixed; n < grdAuxiliary.Rows.Count; n++)
					{
						C1.Win.C1FlexGrid.Row row7 = grdAuxiliary.Rows[n];
						if (num == -1 || (row7["kmmc"]?.ToString()?.Split(LedgerBuilder3.splitChars, StringSplitOptions.RemoveEmptyEntries)?.Length).GetValueOrDefault(-1) == num)
						{
							SetError(grdAuxiliary, n, column5.Index, "科目代码或科目名称不规范，无法生成科目级次");
							SetError(grdAuxiliary, n, column6.Index, "科目代码或科目名称不规范，无法生成科目级次");
							if (flag3)
							{
								dockTab.SelectedTab = tabAuxiliary;
								ShowValidatetip(grdAuxiliary, n, column5.Index);
								flag3 = false;
							}
						}
					}
					if (flag3)
					{
						Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "科目代码或科目名称不规范，无法生成科目级次");
					}
				}
				break;
			}
			case FailureReasonEnum.BalanceNotBeLastLevel:
			{
				dockTab.SelectedTab = tabBalance;
				C1.Win.C1FlexGrid.Row row4 = importException.FailureContext.RowTag as C1.Win.C1FlexGrid.Row;
				SetError(grdBalance, row4.Index, grdBalance.Cols["kmdm"].Index, "在凭证中存在该科目的子级科目，此处设置期初数不合理");
				SetError(grdBalance, row4.Index, grdBalance.Cols["kmmc"].Index, "在凭证中存在该科目的子级科目，此处设置期初数不合理");
				SetError(grdBalance, row4.Index, grdBalance.Cols["debit"].Index, "在凭证中存在该科目的子级科目，此处设置期初数不合理");
				SetError(grdBalance, row4.Index, grdBalance.Cols["credit"].Index, "在凭证中存在该科目的子级科目，此处设置期初数不合理");
				ShowValidatetipIfCanPosition(importException.FailureContext, "在凭证中存在该科目的子级科目，此处设置期初数不合理");
				break;
			}
			case FailureReasonEnum.BeginBalanceNotBeZero:
				dockTab.SelectedTab = tabBalance;
				grdBalance.BeginUpdate();
				try
				{
					for (int k = grdBalance.Rows.Fixed; k < grdBalance.Rows.Count; k++)
					{
						SetError(grdBalance, k, grdBalance.Cols["debit"].Index, "年初科目余额表借贷不平衡");
						SetError(grdBalance, k, grdBalance.Cols["credit"].Index, "年初科目余额表借贷不平衡");
					}
				}
				finally
				{
					grdBalance.EndUpdate();
				}
				ShowValidatetip(grdBalance, (grdBalance.Rows.Count > 1) ? 1 : 0, grdBalance.Cols["credit"].Index);
				break;
			case FailureReasonEnum.AuxiliaryAccountNotBeLastLevel:
				ShowValidatetipIfCanPosition(importException.FailureContext, "辅助核算期初余额设置只能为末级科目");
				break;
			case FailureReasonEnum.VoucherAccountCodeNotFound:
				ShowValidatetipIfCanPosition(importException.FailureContext, "未在年初科目余额表中发现该科目代码");
				break;
			case FailureReasonEnum.VoucherAccountNotBeLastLevel:
				ShowValidatetipIfCanPosition(importException.FailureContext, "凭证的科目必须为末级科目");
				break;
			case FailureReasonEnum.VoucherTypeEmpty:
				ShowValidatetipIfCanPosition(importException.FailureContext, "凭证字不能为空");
				break;
			case FailureReasonEnum.VoucherNumberEmpty:
				ShowValidatetipIfCanPosition(importException.FailureContext, "凭证号不能为空");
				break;
			case FailureReasonEnum.VoucherDateTypeNotCorrect:
				ShowValidatetipIfCanPosition(importException.FailureContext, "凭证日期格式不正确");
				break;
			case FailureReasonEnum.VoucherDebitAmountTypeNotCorrect:
				ShowValidatetipIfCanPosition(importException.FailureContext, "借方金额格式不正确");
				break;
			case FailureReasonEnum.VoucherCreditAmountTypeNotCorrect:
				ShowValidatetipIfCanPosition(importException.FailureContext, "贷方金额格式不正确");
				break;
			case FailureReasonEnum.SpecifyVoucherDebitNotEqualsCredit:
			{
				string text4 = null;
				string text5 = null;
				string text6 = null;
				string text7 = null;
				bool flag4 = true;
				dockTab.SelectedTab = tabVoucher;
				Tuple<string, string> tuple2 = importException.FailureContext.UserData as Tuple<string, string>;
				for (int num2 = grdVoucher.Rows.Fixed; num2 < grdVoucher.Rows.Count; num2++)
				{
					C1.Win.C1FlexGrid.Row row8 = grdVoucher.Rows[num2];
					if (!RowHasValue(row8) || row8.UserData?.ToString() == "tag_total")
					{
						continue;
					}
					string text8 = null;
					string text9 = row8["date"]?.ToString()?.Trim();
					text9 = (string.IsNullOrEmpty(text9) ? text4 : text9);
					text4 = text9;
					if (!DateTime.TryParse(text9, out var result))
					{
						continue;
					}
					string text10 = result.ToString("yyyyMMdd");
					if (sourceConfig.VoucherDisplayStyle == VoucherDisplayStyleEnum.TYPE_NUMBER_SPLIT)
					{
						string text11 = row8["type"]?.ToString()?.Trim();
						text11 = (string.IsNullOrEmpty(text11) ? text5 : text11);
						text5 = text11;
						string text12 = row8["number"]?.ToString()?.Trim();
						text12 = (string.IsNullOrEmpty(text12) ? text6 : text12);
						text6 = text12;
						text8 = text10 + text11 + text12;
					}
					else
					{
						string text13 = row8["type"]?.ToString()?.Trim();
						if (!string.IsNullOrEmpty(text13))
						{
							SplitTypeNumber(text13, out var type, out var number);
							type = (string.IsNullOrEmpty(type) ? text5 : type);
							text5 = type;
							number = (string.IsNullOrEmpty(number) ? text6 : number);
							text6 = number;
							text8 = text10 + type?.Trim() + number?.Trim();
						}
					}
					text8 = (string.IsNullOrEmpty(text8) ? text7 : text8);
					text7 = text8;
					if (!string.IsNullOrWhiteSpace(text8) && tuple2?.Item1 == text8)
					{
						SetError(grdVoucher, num2, grdVoucher.Cols["debit"].Index, "该凭证借贷金额不平衡");
						SetError(grdVoucher, num2, grdVoucher.Cols["credit"].Index, "该凭证借贷金额不平衡");
						if (flag4)
						{
							flag4 = false;
							grdVoucher.ShowCell(num2, grdVoucher.Cols["debit"].Index);
							C1.Win.C1FlexGrid.Column column7 = ((sourceConfig.VoucherAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION) ? grdVoucher.Cols["credit"] : (CellDefault(grdVoucher, num2, grdVoucher.Cols["debit"].Index) ? grdVoucher.Cols["credit"] : grdVoucher.Cols["debit"]));
							ShowValidatetip(grdVoucher, num2, column7.Index);
						}
					}
				}
				if (flag4)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "凭证" + tuple2.Item2 + "借贷金额不平");
				}
				break;
			}
			case FailureReasonEnum.AuxiliaryBeginBalanceNotEqualsAccountBegin:
			{
				bool flag = false;
				string text2 = importException.FailureContext.UserData?.ToString();
				string text3 = $"({importException.FailureContext.UserData})辅助核算期初与科目期初不同";
				for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
				{
					C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows[i];
					if (row2["kmdm"]?.ToString()?.Trim() == text2)
					{
						flag = true;
						SetError(grdBalance, i, grdBalance.Cols["debit"].Index, text3);
						SetError(grdBalance, i, grdBalance.Cols["credit"].Index, text3);
						break;
					}
				}
				for (int j = grdAuxiliary.Rows.Fixed; j < grdAuxiliary.Rows.Count; j++)
				{
					C1.Win.C1FlexGrid.Row row3 = grdAuxiliary.Rows[j];
					if (row3["kmdm"]?.ToString()?.Trim() == text2)
					{
						flag = true;
						SetError(grdAuxiliary, j, grdAuxiliary.Cols["debit"].Index, text3);
						SetError(grdAuxiliary, j, grdAuxiliary.Cols["credit"].Index, text3);
						break;
					}
				}
				ShowValidatetipIfCanPosition(importException.FailureContext, text3);
				if (!flag)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text3);
				}
				break;
			}
			case FailureReasonEnum.VoucherNotRelatedAuxiliaryAccount:
				ShowValidatetipIfCanPosition(importException.FailureContext, $"({importException.FailureContext.UserData}）该科目在其他会计凭证中挂接有辅助核算，请补充该科目辅助核算或在其他会计凭证中将该科目挂接的辅助核算删除");
				break;
			case FailureReasonEnum.AuxBalanceWithoutVoucherAux:
				ShowValidatetipIfCanPosition(importException.FailureContext, $"({importException.FailureContext.UserData}）该科目在年初辅助余额表中挂接有辅助核算，请补充该科目辅助核算或在年初辅助余额表中将该科目删除");
				break;
			case FailureReasonEnum.VoucherAuxiliaryNotFound:
				ShowValidatetipIfCanPosition(importException.FailureContext, (string)importException.FailureContext.UserData);
				break;
			case FailureReasonEnum.AuxiliaryDataNotBeCorrect:
				if (importException.FailureContext.RowTag is C1.Win.C1FlexGrid.Row row)
				{
					switch (importException.FailureContext.Table)
					{
					case TableEnum.VOUCHER:
						SetError(grdVoucher, row.Index, grdVoucher.Cols["auxtype"].Index, importException.FailureContext.UserData?.ToString());
						SetError(grdVoucher, row.Index, grdVoucher.Cols["auxcode"].Index, importException.FailureContext.UserData?.ToString());
						SetError(grdVoucher, row.Index, grdVoucher.Cols["auxname"].Index, importException.FailureContext.UserData?.ToString());
						break;
					case TableEnum.AUXILIARY:
						SetError(grdAuxiliary, row.Index, grdAuxiliary.Cols["auxtype"].Index, importException.FailureContext.UserData?.ToString());
						SetError(grdAuxiliary, row.Index, grdAuxiliary.Cols["auxcode"].Index, importException.FailureContext.UserData?.ToString());
						SetError(grdAuxiliary, row.Index, grdAuxiliary.Cols["auxname"].Index, importException.FailureContext.UserData?.ToString());
						break;
					}
				}
				ShowValidatetipIfCanPosition(importException.FailureContext, importException.FailureContext.UserData?.ToString());
				break;
			case FailureReasonEnum.AccCodeNotBeInBalanceRule:
				ShowValidatetipIfCanPosition(importException.FailureContext, "辅助核算期初表和凭证表中的科目代码应该与科目余额表中的科目代码级次保持一致或为更长");
				break;
			case FailureReasonEnum.SpecificMessage:
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, importException.FailureContext.UserData?.ToString());
				break;
			}
		}
		catch (LedgerImportException ex2)
		{
			ex2.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
		}
		catch (Exception ex3)
		{
			ex3.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex3.Message);
		}
		return false;
	}

	private void SetError(C1FlexGrid grid, int row, int col, string error)
	{
		C1.Win.C1FlexGrid.CellStyle cellStyle = grid.Styles.Add("redBack");
		cellStyle.BackColor = validateErrorBackColor;
		grid.SetCellStyle(row, col, cellStyle);
		ValidateResults.Add(new ValidateResult2
		{
			C1FlexGrid = grid,
			Row = grid.Rows[row],
			Column = grid.Cols[col],
			ErrorMessage = error
		});
	}

	private void ResetAllObsoleteValidateMark()
	{
		try
		{
			foreach (ValidateResult2 validateResult in ValidateResults)
			{
				C1.Win.C1FlexGrid.Row row = validateResult.Row;
				C1.Win.C1FlexGrid.Column column = validateResult.Column;
				if (row.Index >= 0)
				{
					row.Grid.SetCellStyle(row.Index, column.Name, null);
				}
			}
			ValidateResults = new List<ValidateResult2>();
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}

	private void ShowValidatetipIfCanPosition(FailureContext positionContext, string message)
	{
		if (positionContext != null && positionContext.RowTag is C1.Win.C1FlexGrid.Row { Index: >=0 } row && positionContext.ColTag is C1.Win.C1FlexGrid.Column { Index: >=0 } column)
		{
			switch (positionContext.Table)
			{
			case TableEnum.BALANCE:
				dockTab.SelectedTab = tabBalance;
				grdBalance.ShowCell(row.Index, column.Index);
				SetError(grdBalance, row.Index, column.Index, message);
				ShowValidatetip(grdBalance, row.Index, column.Index);
				break;
			case TableEnum.VOUCHER:
				dockTab.SelectedTab = tabVoucher;
				grdVoucher.ShowCell(row.Index, column.Index);
				SetError(grdVoucher, row.Index, column.Index, message);
				ShowValidatetip(grdVoucher, row.Index, column.Index);
				break;
			case TableEnum.AUXILIARY:
				dockTab.SelectedTab = tabAuxiliary;
				grdAuxiliary.ShowCell(row.Index, column.Index);
				SetError(grdAuxiliary, row.Index, column.Index, message);
				ShowValidatetip(grdAuxiliary, row.Index, column.Index);
				break;
			}
		}
	}

	private void ShowValidatetipIfCanPosition(ValidateException ex, string message)
	{
		if (ex == null)
		{
			return;
		}
		C1FlexGrid grid = ex.Grid;
		if (grid == null)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row = ex.Row;
		if (row == null)
		{
			return;
		}
		C1.Win.C1FlexGrid.Column col = ex.Col;
		if (col != null)
		{
			if (ex.Grid == grdBalance)
			{
				dockTab.SelectedTab = tabBalance;
			}
			else if (ex.Grid == grdVoucher)
			{
				dockTab.SelectedTab = tabVoucher;
			}
			else if (ex.Grid == grdAuxiliary)
			{
				dockTab.SelectedTab = tabAuxiliary;
			}
			grid.ShowCell(row.Index, col.Index);
			SetError(grid, row.Index, col.Index, message);
			ShowValidatetip(grid, row.Index, col.Index);
		}
	}

	private object ParseDate(object value)
	{
		try
		{
			if (value == null)
			{
				return null;
			}
			string input = value.ToString();
			if (DateTime.TryParse(input, out var result))
			{
				return result;
			}
			Match match = Regex.Match(input, "([0-9]{4})([0-9]{2})([0-9]{2})");
			if (match.Success)
			{
				return new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
			}
			return value;
		}
		catch
		{
			return value;
		}
	}

	private async void cmdValidate_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (await ValidateFill(save: false))
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "校验通过！");
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private async void cmdGenerate_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (await ValidateFill(save: true))
			{
				Hide();
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已成功生成账套，生成后的账套文件存储在" + SavePath);
				this.AfterGenerateSuccess?.Invoke(this, SavePath);
				_hasGenerated = true;
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	private void cmdFilltip_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		displayFilltip = e.NewValue;
		if (!displayFilltip)
		{
			_tooltip.Hide();
		}
	}

	private void CkbAuxPageHasAuxiliary_CheckedChanged(object sender, EventArgs e)
	{
		ckbVoucherHasAuxiliary.CheckedChanged -= ckbVoucherHasAuxiliary_CheckedChanged;
		ckbAuxPageHasAuxiliary.CheckedChanged -= CkbAuxPageHasAuxiliary_CheckedChanged;
		ckbBalanceHasAuxiliary.CheckedChanged -= ckbBalanceNonDirection_CheckedChanged;
		try
		{
			bool @checked = ckbAuxPageHasAuxiliary.Checked;
			ckbVoucherHasAuxiliary.Checked = @checked;
			ckbBalanceHasAuxiliary.Checked = @checked;
			try
			{
				grdVoucher.BeginUpdate();
				sourceConfig.HasAuxiliary = @checked;
				if (sourceConfig.HasAuxiliary)
				{
					ShowAuxiliary();
					ckbAuxMultiColumn.Visible = true;
					ckbAuxCodeNameSplit.Visible = ckbAuxMultiColumn.Checked;
				}
				else
				{
					HideAuxiliary();
					ckbAuxMultiColumn.Visible = false;
					ckbAuxCodeNameSplit.Visible = false;
				}
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		finally
		{
			ckbVoucherHasAuxiliary.CheckedChanged += ckbVoucherHasAuxiliary_CheckedChanged;
			ckbAuxPageHasAuxiliary.CheckedChanged += CkbAuxPageHasAuxiliary_CheckedChanged;
			ckbBalanceHasAuxiliary.CheckedChanged += ckbBalanceNonDirection_CheckedChanged;
		}
	}

	private void ckbVoucherHasAuxiliary_CheckedChanged(object sender, EventArgs e)
	{
		ckbVoucherHasAuxiliary.CheckedChanged -= ckbVoucherHasAuxiliary_CheckedChanged;
		ckbAuxPageHasAuxiliary.CheckedChanged -= CkbAuxPageHasAuxiliary_CheckedChanged;
		ckbBalanceHasAuxiliary.CheckedChanged -= ckbBalanceNonDirection_CheckedChanged;
		try
		{
			bool @checked = ckbVoucherHasAuxiliary.Checked;
			ckbAuxPageHasAuxiliary.Checked = @checked;
			ckbBalanceHasAuxiliary.Checked = @checked;
			try
			{
				grdVoucher.BeginUpdate();
				sourceConfig.HasAuxiliary = @checked;
				if (sourceConfig.HasAuxiliary)
				{
					ShowAuxiliary();
					ckbAuxMultiColumn.Visible = true;
					ckbAuxCodeNameSplit.Visible = ckbAuxMultiColumn.Checked;
				}
				else
				{
					HideAuxiliary();
					ckbAuxMultiColumn.Visible = false;
					ckbAuxCodeNameSplit.Visible = false;
				}
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		finally
		{
			ckbVoucherHasAuxiliary.CheckedChanged += ckbVoucherHasAuxiliary_CheckedChanged;
			ckbAuxPageHasAuxiliary.CheckedChanged += CkbAuxPageHasAuxiliary_CheckedChanged;
			ckbBalanceHasAuxiliary.CheckedChanged += ckbBalanceNonDirection_CheckedChanged;
		}
	}

	private void CkbBalanceHasAuxiliary_CheckedChanged(object sender, EventArgs e)
	{
		ckbVoucherHasAuxiliary.CheckedChanged -= ckbVoucherHasAuxiliary_CheckedChanged;
		ckbAuxPageHasAuxiliary.CheckedChanged -= CkbAuxPageHasAuxiliary_CheckedChanged;
		ckbBalanceHasAuxiliary.CheckedChanged -= ckbBalanceNonDirection_CheckedChanged;
		try
		{
			bool @checked = ckbBalanceHasAuxiliary.Checked;
			ckbAuxPageHasAuxiliary.Checked = @checked;
			ckbVoucherHasAuxiliary.Checked = @checked;
			try
			{
				grdVoucher.BeginUpdate();
				sourceConfig.HasAuxiliary = @checked;
				if (sourceConfig.HasAuxiliary)
				{
					ShowAuxiliary();
					ckbAuxMultiColumn.Visible = true;
					ckbAuxCodeNameSplit.Visible = ckbAuxMultiColumn.Checked;
				}
				else
				{
					HideAuxiliary();
					ckbAuxMultiColumn.Visible = false;
					ckbAuxCodeNameSplit.Visible = false;
				}
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		finally
		{
			ckbVoucherHasAuxiliary.CheckedChanged += ckbVoucherHasAuxiliary_CheckedChanged;
			ckbAuxPageHasAuxiliary.CheckedChanged += CkbAuxPageHasAuxiliary_CheckedChanged;
			ckbBalanceHasAuxiliary.CheckedChanged += ckbBalanceNonDirection_CheckedChanged;
		}
	}

	private void ShowAuxiliary()
	{
		if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.TypeMultiColumn)
		{
			for (int i = grdVoucher.Cols.Fixed; i < grdVoucher.Cols.Count; i++)
			{
				C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[i];
				if (column.UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
				{
					column.Move(grdVoucher.Cols["debit"].Index);
					column.AllowEditing = true;
					column.Visible = true;
				}
			}
		}
		else if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.MergeBySplitChar)
		{
			grdVoucher.Cols["auxtype"].Move(grdVoucher.Cols["kmmc"].Index + 1);
			grdVoucher.Cols["auxcode"].Move(grdVoucher.Cols["auxtype"].Index + 1);
			grdVoucher.Cols["auxname"].Move(grdVoucher.Cols["auxcode"].Index + 1);
			grdVoucher.Cols["auxtype"].AllowEditing = true;
			grdVoucher.Cols["auxcode"].AllowEditing = true;
			grdVoucher.Cols["auxname"].AllowEditing = true;
			grdVoucher.Cols["auxtype"].Visible = true;
			grdVoucher.Cols["auxcode"].Visible = true;
			grdVoucher.Cols["auxname"].Visible = true;
		}
		grdAuxiliary.Visible = true;
		tabAuxiliary.TabVisible = true;
	}

	private void HideAuxiliary()
	{
		grdAuxiliary.Visible = false;
		tabAuxiliary.TabVisible = false;
		if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.TypeMultiColumn)
		{
			for (int num = grdVoucher.Cols.Count - 2; num >= grdVoucher.Cols.Fixed; num--)
			{
				C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[num];
				if (column.UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
				{
					column.Visible = false;
					column.AllowEditing = false;
					column.Move(grdVoucher.Cols.Count - 1);
				}
			}
		}
		else if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.MergeBySplitChar)
		{
			grdVoucher.Cols["auxtype"].Visible = false;
			grdVoucher.Cols["auxcode"].Visible = false;
			grdVoucher.Cols["auxname"].Visible = false;
			grdVoucher.Cols["auxtype"].AllowEditing = false;
			grdVoucher.Cols["auxcode"].AllowEditing = false;
			grdVoucher.Cols["auxname"].AllowEditing = false;
			grdVoucher.Cols["auxtype"].Move(grdVoucher.Cols.Count - 1);
			grdVoucher.Cols["auxcode"].Move(grdVoucher.Cols.Count - 1);
			grdVoucher.Cols["auxname"].Move(grdVoucher.Cols.Count - 1);
		}
	}

	private void InsertFixAuxColumn()
	{
		int index = grdVoucher.Cols["debit"].Index;
		grdVoucher.Cols.InsertRange(index, 3);
		C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[index++];
		column.Name = "auxtype";
		column.Caption = "辅助核算类别";
		column.DataType = typeof(string);
		column = grdVoucher.Cols[index++];
		column.Name = "auxcode";
		column.Caption = "辅助核算代码";
		column.DataType = typeof(string);
		column = grdVoucher.Cols[index++];
		column.Name = "auxname";
		column.Caption = "辅助核算名称";
		column.DataType = typeof(string);
	}

	private void RemoveAllAuxColumn()
	{
		for (int num = grdVoucher.Cols.Count - 1; num >= grdVoucher.Cols.Fixed; num--)
		{
			C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[num];
			if (column.Name == "auxtype" || column.Name == "auxcode" || column.Name == "auxname")
			{
				grdVoucher.Cols.Remove(num);
			}
			else if (column.UserData is Tuple<string, string> tuple && (tuple.Item1 == "auxcode" || tuple.Item1 == "auxname"))
			{
				grdVoucher.Cols.Remove(num);
			}
		}
	}

	private void SwitchVoucherAuxCodeName(AuxCodeNameStyleEnum newStyle)
	{
		if (sourceConfig.AuxCodeNameStyle == newStyle)
		{
			return;
		}
		if (sourceConfig.AuxCodeNameStyle == AuxCodeNameStyleEnum.CODE_NAME_SPLIT && newStyle == AuxCodeNameStyleEnum.CODE_NAME_UNIFY)
		{
			grdVoucher.BeginUpdate();
			try
			{
				for (int num = grdVoucher.Cols.Count - 1; num >= grdVoucher.Cols.Fixed; num--)
				{
					C1.Win.C1FlexGrid.Column column = grdVoucher.Cols[num];
					if (column.UserData is Tuple<string, string> tuple)
					{
						if (tuple.Item1 == "auxname")
						{
							grdVoucher.Cols.Remove(num);
						}
						else if (tuple.Item1 == "auxcode")
						{
							column.Caption = tuple.Item2 + "代码及名称";
						}
					}
				}
				sourceConfig.AuxCodeNameStyle = newStyle;
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		if (sourceConfig.AuxCodeNameStyle != AuxCodeNameStyleEnum.CODE_NAME_UNIFY || newStyle != 0)
		{
			return;
		}
		grdVoucher.BeginUpdate();
		try
		{
			for (int num2 = grdVoucher.Cols.Count - 1; num2 >= grdVoucher.Cols.Fixed; num2--)
			{
				C1.Win.C1FlexGrid.Column column2 = grdVoucher.Cols[num2];
				if (column2.UserData is Tuple<string, string> { Item1: "auxcode" } tuple2)
				{
					C1.Win.C1FlexGrid.Column column3 = null;
					column3 = ((num2 + 1 < grdVoucher.Cols.Count) ? grdVoucher.Cols.Insert(num2 + 1) : grdVoucher.Cols.Add());
					column3.UserData = Tuple.Create("auxname", tuple2.Item2);
					column3.Caption = tuple2.Item2 + "名称";
					column3.DataType = typeof(string);
					column2.Caption = tuple2.Item2 + "代码";
				}
			}
			sourceConfig.AuxCodeNameStyle = newStyle;
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void SwitchVoucherAuxStyle(AuxiliaryDataMode newStyle)
	{
		if (sourceConfig.AuxiliaryDataMode == newStyle)
		{
			return;
		}
		if (sourceConfig.AuxiliaryDataMode == AuxiliaryDataMode.MergeBySplitChar && newStyle == AuxiliaryDataMode.TypeMultiColumn)
		{
			grdVoucher.BeginUpdate();
			try
			{
				RemoveAllAuxColumn();
				AppendAuxColumn("部门");
				AppendAuxColumn("项目");
				AppendAuxColumn("往来");
				sourceConfig.AuxiliaryDataMode = newStyle;
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		if (sourceConfig.AuxiliaryDataMode != AuxiliaryDataMode.TypeMultiColumn || newStyle != 0)
		{
			return;
		}
		grdVoucher.BeginUpdate();
		try
		{
			RemoveAllAuxColumn();
			InsertFixAuxColumn();
			sourceConfig.AuxiliaryDataMode = newStyle;
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void SwitchVoucherDisplayStyle(VoucherDisplayStyleEnum newStyle)
	{
		if (sourceConfig.VoucherDisplayStyle == newStyle)
		{
			return;
		}
		if (sourceConfig.VoucherDisplayStyle == VoucherDisplayStyleEnum.TYPE_NUMBER_SPLIT && newStyle == VoucherDisplayStyleEnum.TYPE_NUMBER_UNIFY)
		{
			grdVoucher.BeginUpdate();
			try
			{
				Dictionary<C1.Win.C1FlexGrid.Row, string> dictionary = new Dictionary<C1.Win.C1FlexGrid.Row, string>();
				for (int i = grdVoucher.Rows.Fixed; i < grdVoucher.Rows.Count; i++)
				{
					C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[i];
					if (!(row.UserData?.ToString() == "tag_total") && RowHasValue(row))
					{
						string text = row["type"]?.ToString();
						string text2 = row["number"]?.ToString();
						string empty = string.Empty;
						empty = (string.IsNullOrEmpty(text) ? ((!string.IsNullOrEmpty(text2)) ? text2 : null) : ((!string.IsNullOrEmpty(text2)) ? (text + "-" + text2) : text));
						dictionary.Add(row, empty);
					}
				}
				C1.Win.C1FlexGrid.Column column = grdVoucher.Cols["type"];
				column.Caption = "凭证字号";
				C1.Win.C1FlexGrid.Column column2 = grdVoucher.Cols["number"];
				column2.Visible = false;
				grdVoucher.Cols.Move(column2.Index, grdVoucher.Cols.Count - 1);
				foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, string> item in dictionary)
				{
					C1.Win.C1FlexGrid.Row key = item.Key;
					key["type"] = item.Value;
				}
				sourceConfig.VoucherDisplayStyle = newStyle;
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		if (sourceConfig.VoucherDisplayStyle != VoucherDisplayStyleEnum.TYPE_NUMBER_UNIFY || newStyle != 0)
		{
			return;
		}
		grdVoucher.BeginUpdate();
		try
		{
			Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, string>> dictionary2 = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, string>>();
			for (int j = grdVoucher.Rows.Fixed; j < grdVoucher.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdVoucher.Rows[j];
				if (!(row2.UserData?.ToString() == "tag_total") && RowHasValue(row2))
				{
					object obj = row2["type"];
					if (obj == null)
					{
						dictionary2.Add(row2, Tuple.Create(string.Empty, string.Empty));
						continue;
					}
					string typeNumber = obj.ToString().Trim(LedgerBuilder3.prefixChars);
					SplitTypeNumber(typeNumber, out var type, out var number);
					dictionary2.Add(row2, Tuple.Create(type, number));
				}
			}
			C1.Win.C1FlexGrid.Column column3 = grdVoucher.Cols["type"];
			column3.Caption = "凭证字";
			C1.Win.C1FlexGrid.Column column4 = grdVoucher.Cols["number"];
			grdVoucher.Cols.Move(column4.Index, grdVoucher.Cols["type"].Index + 1);
			column4.Visible = true;
			foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<string, string>> item2 in dictionary2)
			{
				C1.Win.C1FlexGrid.Row key2 = item2.Key;
				key2["type"] = item2.Value.Item1;
				key2["number"] = item2.Value.Item2;
			}
			sourceConfig.VoucherDisplayStyle = newStyle;
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void SplitTypeNumber(string typeNumber, out string type, out string number)
	{
		if (string.IsNullOrWhiteSpace(typeNumber))
		{
			type = string.Empty;
			number = string.Empty;
			return;
		}
		string[] array = typeNumber.Split(LedgerBuilder3.splitChars, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			if (int.TryParse(typeNumber, out var result))
			{
				type = "记";
				number = result.ToString();
				return;
			}
			Match match = Regex.Match(typeNumber, "(.*)([0-9]+)");
			if (match.Success)
			{
				type = match.Groups[1].Value.TrimEnd(LedgerBuilder3.splitChars);
				number = match.Groups[2].Value;
			}
			else
			{
				type = typeNumber;
				number = null;
			}
		}
		else if (array.Length >= 2)
		{
			int num = typeNumber.LastIndexOfAny(LedgerBuilder3.splitChars);
			type = subString(typeNumber, 0, num);
			number = subString(typeNumber, num + 1, typeNumber.Length - num - 1);
		}
		else
		{
			type = typeNumber;
			number = null;
		}
		static string subString(string str, int start, int len)
		{
			if (string.IsNullOrEmpty(str))
			{
				return string.Empty;
			}
			if (start < 0)
			{
				return string.Empty;
			}
			if (start + len > str.Length)
			{
				return string.Empty;
			}
			return str.Substring(start, len);
		}
	}

	private void SwitchBalanceDirectionStyle(DirectionStyleEnum newStyle)
	{
		if (sourceConfig.BalanceAmountDirectionStyle == newStyle)
		{
			return;
		}
		if (sourceConfig.BalanceAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION && newStyle == DirectionStyleEnum.NONE_DIRECTION)
		{
			grdBalance.BeginUpdate();
			try
			{
				Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>> dictionary = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>>();
				for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
				{
					C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
					if (!(row.UserData?.ToString() == "tag_total") && RowHasValue(row))
					{
						string text = row["debit"]?.ToString();
						decimal result;
						decimal num = (decimal.TryParse(row["credit"]?.ToString(), out result) ? result : 0m);
						dictionary.Add(row, Tuple.Create((text != null && text.Contains("借")) ? num : 0m, (text != null && text.Contains("贷")) ? num : 0m));
					}
				}
				C1.Win.C1FlexGrid.Column column = grdBalance.Cols["debit"];
				column.Caption = "年初借方余额";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
				column.ComboList = null;
				C1.Win.C1FlexGrid.Column column2 = grdBalance.Cols["credit"];
				column2.Caption = "年初贷方余额";
				column2.DataType = typeof(decimal);
				column2.Format = "#,0.00;-#,0.00;#";
				column2.ComboList = null;
				foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>> item in dictionary)
				{
					C1.Win.C1FlexGrid.Row key = item.Key;
					key["debit"] = item.Value.Item1;
					key["credit"] = item.Value.Item2;
				}
				sourceConfig.BalanceAmountDirectionStyle = newStyle;
				ReGenerateBalanceTotal();
				return;
			}
			finally
			{
				grdBalance.EndUpdate();
			}
		}
		if (sourceConfig.BalanceAmountDirectionStyle != DirectionStyleEnum.NONE_DIRECTION || newStyle != 0)
		{
			return;
		}
		grdBalance.BeginUpdate();
		try
		{
			Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>> dictionary2 = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>>();
			for (int j = grdBalance.Rows.Fixed; j < grdBalance.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows[j];
				if (!(row2.UserData?.ToString() == "tag_total") && RowHasValue(row2))
				{
					decimal result2;
					decimal num2 = (decimal.TryParse(row2["debit"]?.ToString(), out result2) ? result2 : 0m);
					decimal result3;
					decimal num3 = (decimal.TryParse(row2["credit"]?.ToString(), out result3) ? result3 : 0m);
					if (num2 == 0m && num3 == 0m)
					{
						dictionary2.Add(row2, Tuple.Create("平", 0m));
						continue;
					}
					string text2 = ((!(num2 == 0m)) ? "借" : ((num3 == 0m) ? "平" : "贷"));
					dictionary2.Add(row2, Tuple.Create(text2, (text2 != null && text2.Contains("借")) ? num2 : ((text2 != null && text2.Contains("贷")) ? num3 : 0m)));
				}
			}
			C1.Win.C1FlexGrid.Column column3 = grdBalance.Cols["debit"];
			column3.Caption = "年初余额方向";
			column3.DataType = typeof(string);
			column3.Format = null;
			column3.ComboList = "借|贷|平";
			column3.TextAlign = TextAlignEnum.CenterCenter;
			C1.Win.C1FlexGrid.Column column4 = grdBalance.Cols["credit"];
			column4.Caption = "年初余额";
			column4.DataType = typeof(decimal);
			column4.Format = "#,0.00;-#,0.00;#";
			column4.ComboList = null;
			foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>> item2 in dictionary2)
			{
				C1.Win.C1FlexGrid.Row key2 = item2.Key;
				key2["debit"] = item2.Value.Item1;
				key2["credit"] = item2.Value.Item2;
			}
			sourceConfig.BalanceAmountDirectionStyle = newStyle;
			ReGenerateBalanceTotal();
		}
		finally
		{
			grdBalance.EndUpdate();
		}
	}

	private void SwitchVoucherDirectionStyle(DirectionStyleEnum newStyle)
	{
		if (sourceConfig.VoucherAmountDirectionStyle == newStyle)
		{
			return;
		}
		if (sourceConfig.VoucherAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION && newStyle == DirectionStyleEnum.NONE_DIRECTION)
		{
			grdVoucher.BeginUpdate();
			try
			{
				Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>> dictionary = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>>();
				for (int i = grdVoucher.Rows.Fixed; i < grdVoucher.Rows.Count; i++)
				{
					C1.Win.C1FlexGrid.Row row = grdVoucher.Rows[i];
					if (!(row.UserData?.ToString() == "tag_total") && RowHasValue(row))
					{
						string text = row["debit"]?.ToString();
						decimal result;
						decimal num = (decimal.TryParse(row["credit"]?.ToString(), out result) ? result : 0m);
						dictionary.Add(row, Tuple.Create((text != null && text.Contains("借")) ? num : 0m, (text != null && text.Contains("贷")) ? num : 0m));
					}
				}
				C1.Win.C1FlexGrid.Column column = grdVoucher.Cols["debit"];
				column.Caption = "借方金额";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
				column.ComboList = null;
				C1.Win.C1FlexGrid.Column column2 = grdVoucher.Cols["credit"];
				column2.Caption = "贷方金额";
				column2.DataType = typeof(decimal);
				column2.Format = "#,0.00;-#,0.00;#";
				column2.ComboList = null;
				foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>> item in dictionary)
				{
					C1.Win.C1FlexGrid.Row key = item.Key;
					key["debit"] = item.Value.Item1;
					key["credit"] = item.Value.Item2;
				}
				sourceConfig.VoucherAmountDirectionStyle = newStyle;
				ReGenerateVoucherTotal();
				return;
			}
			finally
			{
				grdVoucher.EndUpdate();
			}
		}
		if (sourceConfig.VoucherAmountDirectionStyle != DirectionStyleEnum.NONE_DIRECTION || newStyle != 0)
		{
			return;
		}
		grdVoucher.BeginUpdate();
		try
		{
			Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>> dictionary2 = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>>();
			for (int j = grdVoucher.Rows.Fixed; j < grdVoucher.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdVoucher.Rows[j];
				if (!(row2.UserData?.ToString() == "tag_total") && RowHasValue(row2))
				{
					decimal result2;
					decimal num2 = (decimal.TryParse(row2["debit"]?.ToString(), out result2) ? result2 : 0m);
					decimal result3;
					decimal num3 = (decimal.TryParse(row2["credit"]?.ToString(), out result3) ? result3 : 0m);
					if (num2 == 0m && num3 == 0m)
					{
						dictionary2.Add(row2, Tuple.Create<string, decimal>(null, 0m));
						continue;
					}
					string text2 = ((!(num2 == 0m)) ? "借" : ((num3 == 0m) ? null : "贷"));
					dictionary2.Add(row2, Tuple.Create(text2, (text2 != null && text2.Contains("借")) ? num2 : ((text2 != null && text2.Contains("贷")) ? num3 : 0m)));
				}
			}
			C1.Win.C1FlexGrid.Column column3 = grdVoucher.Cols["debit"];
			column3.Caption = "方向";
			column3.DataType = typeof(string);
			column3.Format = null;
			column3.ComboList = "借|贷";
			column3.TextAlign = TextAlignEnum.CenterCenter;
			C1.Win.C1FlexGrid.Column column4 = grdVoucher.Cols["credit"];
			column4.Caption = "金额";
			column4.DataType = typeof(decimal);
			column4.Format = "#,0.00;-#,0.00;#";
			column4.ComboList = null;
			foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>> item2 in dictionary2)
			{
				C1.Win.C1FlexGrid.Row key2 = item2.Key;
				key2["debit"] = item2.Value.Item1;
				key2["credit"] = item2.Value.Item2;
			}
			sourceConfig.VoucherAmountDirectionStyle = newStyle;
			ReGenerateVoucherTotal();
		}
		finally
		{
			grdVoucher.EndUpdate();
		}
	}

	private void SwitchAuxiliaryDirectionStyle(DirectionStyleEnum newStyle)
	{
		if (sourceConfig.AuxiliaryAmountDirectionStyle == newStyle)
		{
			return;
		}
		if (sourceConfig.AuxiliaryAmountDirectionStyle == DirectionStyleEnum.HAVE_DIRECTION && newStyle == DirectionStyleEnum.NONE_DIRECTION)
		{
			grdAuxiliary.BeginUpdate();
			try
			{
				Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>> dictionary = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>>();
				for (int i = grdAuxiliary.Rows.Fixed; i < grdAuxiliary.Rows.Count; i++)
				{
					C1.Win.C1FlexGrid.Row row = grdAuxiliary.Rows[i];
					if (!(row.UserData?.ToString() == "tag_total") && RowHasValue(row))
					{
						string text = row["debit"]?.ToString();
						decimal result;
						decimal num = (decimal.TryParse(row["credit"]?.ToString(), out result) ? result : 0m);
						dictionary.Add(row, Tuple.Create((text != null && text.Contains("借")) ? num : 0m, (text != null && text.Contains("贷")) ? num : 0m));
					}
				}
				C1.Win.C1FlexGrid.Column column = grdAuxiliary.Cols["debit"];
				column.Caption = "年初借方余额";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
				column.ComboList = null;
				C1.Win.C1FlexGrid.Column column2 = grdAuxiliary.Cols["credit"];
				column2.Caption = "年初贷方余额";
				column2.DataType = typeof(decimal);
				column2.Format = "#,0.00;-#,0.00;#";
				column2.ComboList = null;
				foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<decimal, decimal>> item in dictionary)
				{
					C1.Win.C1FlexGrid.Row key = item.Key;
					key["debit"] = item.Value.Item1;
					key["credit"] = item.Value.Item2;
				}
			}
			finally
			{
				grdAuxiliary.EndUpdate();
			}
			sourceConfig.AuxiliaryAmountDirectionStyle = newStyle;
		}
		else
		{
			if (sourceConfig.AuxiliaryAmountDirectionStyle != DirectionStyleEnum.NONE_DIRECTION || newStyle != 0)
			{
				return;
			}
			grdAuxiliary.BeginUpdate();
			try
			{
				Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>> dictionary2 = new Dictionary<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>>();
				for (int j = grdAuxiliary.Rows.Fixed; j < grdAuxiliary.Rows.Count; j++)
				{
					C1.Win.C1FlexGrid.Row row2 = grdAuxiliary.Rows[j];
					if (!(row2.UserData?.ToString() == "tag_total") && RowHasValue(row2))
					{
						decimal result2;
						decimal num2 = (decimal.TryParse(row2["debit"]?.ToString(), out result2) ? result2 : 0m);
						decimal result3;
						decimal num3 = (decimal.TryParse(row2["credit"]?.ToString(), out result3) ? result3 : 0m);
						if (num2 == 0m && num3 == 0m)
						{
							dictionary2.Add(row2, Tuple.Create<string, decimal>(null, 0m));
							continue;
						}
						string text2 = ((!(num2 == 0m)) ? "借" : ((num3 == 0m) ? "平" : "贷"));
						dictionary2.Add(row2, Tuple.Create(text2, (text2 != null && text2.Contains("借")) ? num2 : ((text2 != null && text2.Contains("贷")) ? num3 : 0m)));
					}
				}
				C1.Win.C1FlexGrid.Column column3 = grdAuxiliary.Cols["debit"];
				column3.Caption = "年初余额方向";
				column3.DataType = typeof(string);
				column3.Format = null;
				column3.ComboList = "借|贷";
				column3.TextAlign = TextAlignEnum.CenterCenter;
				C1.Win.C1FlexGrid.Column column4 = grdAuxiliary.Cols["credit"];
				column4.Caption = "年初余额";
				column4.DataType = typeof(decimal);
				column4.Format = "#,0.00;-#,0.00;#";
				column4.ComboList = null;
				foreach (KeyValuePair<C1.Win.C1FlexGrid.Row, Tuple<string, decimal>> item2 in dictionary2)
				{
					C1.Win.C1FlexGrid.Row key2 = item2.Key;
					key2["debit"] = item2.Value.Item1;
					key2["credit"] = item2.Value.Item2;
				}
			}
			finally
			{
				grdAuxiliary.EndUpdate();
			}
			sourceConfig.AuxiliaryAmountDirectionStyle = newStyle;
		}
	}

	private void ckbTypeNumSplit_CheckedChanged(object sender, EventArgs e)
	{
		if (ckbTypeNumSplit.Checked)
		{
			SwitchVoucherDisplayStyle(VoucherDisplayStyleEnum.TYPE_NUMBER_SPLIT);
		}
		else
		{
			SwitchVoucherDisplayStyle(VoucherDisplayStyleEnum.TYPE_NUMBER_UNIFY);
		}
	}

	private void ckbBalanceNonDirection_CheckedChanged(object sender, EventArgs e)
	{
		if (ckbBalanceNonDirection.Checked)
		{
			SwitchBalanceDirectionStyle(DirectionStyleEnum.NONE_DIRECTION);
		}
		else
		{
			SwitchBalanceDirectionStyle(DirectionStyleEnum.HAVE_DIRECTION);
		}
	}

	private void CkbVoucherNonDirection_CheckedChanged(object sender, EventArgs e)
	{
		if (ckbVoucherNonDirection.Checked)
		{
			SwitchVoucherDirectionStyle(DirectionStyleEnum.NONE_DIRECTION);
		}
		else
		{
			SwitchVoucherDirectionStyle(DirectionStyleEnum.HAVE_DIRECTION);
		}
	}

	private void CkbAuxiliaryNonDirection_CheckedChanged(object sender, EventArgs e)
	{
		if (ckbAuxiliaryNonDirection.Checked)
		{
			SwitchAuxiliaryDirectionStyle(DirectionStyleEnum.NONE_DIRECTION);
		}
		else
		{
			SwitchAuxiliaryDirectionStyle(DirectionStyleEnum.HAVE_DIRECTION);
		}
	}

	private void cmdHelpDoc_Click(object sender, ClickEventArgs e)
	{
		// 已禁用远程帮助页面
	}

	private void cmdReplace_Click(object sender, ClickEventArgs e)
	{
		if (HotGrid != null)
		{
			ShowReplaceForm(replace: true);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.LedgerView.frmImport));
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlTools = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1CommandDock1 = new C1.Win.C1Command.C1CommandDock();
		this.c1ToolBar1 = new C1.Win.C1Command.C1ToolBar();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		this.lnkFilltip = new C1.Win.C1Command.C1CommandLink();
		this.lnkReplace = new C1.Win.C1Command.C1CommandLink();
		this.lnkValidate = new C1.Win.C1Command.C1CommandLink();
		this.lnkGenerate = new C1.Win.C1Command.C1CommandLink();
		this.lnkHelpDoc = new C1.Win.C1Command.C1CommandLink();
		this.pnlInput = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.dockTabInput = new C1.Win.C1Command.C1DockingTab();
		this.tabPageInputBoxBalance = new C1.Win.C1Command.C1DockingTabPage();
		this.ckbBalanceNonDirection = new C1.Win.C1Input.C1CheckBox();
		this.ckbBalanceHasAuxiliary = new C1.Win.C1Input.C1CheckBox();
		this.tabPageInputBoxAuxiliary = new C1.Win.C1Command.C1DockingTabPage();
		this.ckbAuxPageHasAuxiliary = new C1.Win.C1Input.C1CheckBox();
		this.ckbAuxiliaryNonDirection = new C1.Win.C1Input.C1CheckBox();
		this.tabPageInputBoxVoucher = new C1.Win.C1Command.C1DockingTabPage();
		this.ckbAuxCodeNameSplit = new C1.Win.C1Input.C1CheckBox();
		this.ckbVoucherNonDirection = new C1.Win.C1Input.C1CheckBox();
		this.ckbAuxMultiColumn = new C1.Win.C1Input.C1CheckBox();
		this.ckbTypeNumSplit = new C1.Win.C1Input.C1CheckBox();
		this.ckbVoucherHasAuxiliary = new C1.Win.C1Input.C1CheckBox();
		this.cboAuxStyle = new C1.Win.C1Input.C1ComboBox();
		this.txtCurrency = new C1.Win.C1Input.C1TextBox();
		this.txtCompany = new C1.Win.C1Input.C1TextBox();
		this.lblCurrency = new C1.Win.C1Input.C1Label();
		this.lblCompany = new C1.Win.C1Input.C1Label();
		this.pnlDatas = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.dockTab = new C1.Win.C1Command.C1DockingTab();
		this.tabBalance = new C1.Win.C1Command.C1DockingTabPage();
		this.grdBalance = new Auditai.UI.Controls.C1FlexGridEx();
		this.tabAuxiliary = new C1.Win.C1Command.C1DockingTabPage();
		this.grdAuxiliary = new Auditai.UI.Controls.C1FlexGridEx();
		this.tabVoucher = new C1.Win.C1Command.C1DockingTabPage();
		this.grdVoucher = new Auditai.UI.Controls.C1FlexGridEx();
		this.cmdGenerate = new C1.Win.C1Command.C1Command();
		this.cmdFilltip = new C1.Win.C1Command.C1Command();
		this.cmdValidate = new C1.Win.C1Command.C1Command();
		this.cmdHelpDoc = new C1.Win.C1Command.C1Command();
		this.cmdReplace = new C1.Win.C1Command.C1Command();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlTools.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1CommandDock1).BeginInit();
		this.c1CommandDock1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		this.pnlInput.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dockTabInput).BeginInit();
		this.dockTabInput.SuspendLayout();
		this.tabPageInputBoxBalance.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ckbBalanceNonDirection).BeginInit();
		this.tabPageInputBoxAuxiliary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ckbAuxPageHasAuxiliary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbAuxiliaryNonDirection).BeginInit();
		this.tabPageInputBoxVoucher.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ckbAuxCodeNameSplit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbVoucherNonDirection).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbAuxMultiColumn).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbTypeNumSplit).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbVoucherHasAuxiliary).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cboAuxStyle).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtCurrency).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtCompany).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCurrency).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblCompany).BeginInit();
		this.pnlDatas.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dockTab).BeginInit();
		this.dockTab.SuspendLayout();
		this.tabBalance.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdBalance).BeginInit();
		this.tabAuxiliary.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdAuxiliary).BeginInit();
		this.tabVoucher.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdVoucher).BeginInit();
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
		this.ctnAll.Panels.Add(this.pnlTools);
		this.ctnAll.Panels.Add(this.pnlInput);
		this.ctnAll.Panels.Add(this.pnlDatas);
		this.ctnAll.Size = new System.Drawing.Size(1192, 477);
		this.ctnAll.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 1;
		this.ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlTools.Controls.Add(this.c1CommandDock1);
		this.pnlTools.Height = 63;
		this.pnlTools.KeepRelativeSize = false;
		this.pnlTools.Location = new System.Drawing.Point(0, 0);
		this.pnlTools.Name = "pnlTools";
		this.pnlTools.Resizable = false;
		this.pnlTools.Size = new System.Drawing.Size(1192, 63);
		this.pnlTools.SizeRatio = 13.208;
		this.pnlTools.TabIndex = 1;
		this.c1CommandDock1.Controls.Add(this.c1ToolBar1);
		this.c1CommandDock1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1CommandDock1.Id = 1;
		this.c1CommandDock1.Location = new System.Drawing.Point(0, 0);
		this.c1CommandDock1.Name = "c1CommandDock1";
		this.c1CommandDock1.Size = new System.Drawing.Size(1192, 63);
		this.c1ToolBar1.AccessibleName = "Tool Bar";
		this.c1ToolBar1.AutoSize = false;
		this.c1ToolBar1.Border.Width = 0;
		this.c1ToolBar1.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.c1ToolBar1.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.c1ToolBar1.CommandHolder = this.c1CommandHolder1;
		this.c1ToolBar1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[5] { this.lnkFilltip, this.lnkReplace, this.lnkValidate, this.lnkGenerate, this.lnkHelpDoc });
		this.c1ToolBar1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1ToolBar1.Location = new System.Drawing.Point(0, 0);
		this.c1ToolBar1.MinButtonSize = 42;
		this.c1ToolBar1.Movable = false;
		this.c1ToolBar1.Name = "c1ToolBar1";
		this.c1ToolBar1.Size = new System.Drawing.Size(823, 61);
		this.c1ToolBar1.Text = "c1ToolBar2";
		this.c1ToolBar1.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.c1ToolBar1.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.c1CommandHolder1.Commands.Add(this.cmdGenerate);
		this.c1CommandHolder1.Commands.Add(this.cmdFilltip);
		this.c1CommandHolder1.Commands.Add(this.cmdValidate);
		this.c1CommandHolder1.Commands.Add(this.cmdHelpDoc);
		this.c1CommandHolder1.Commands.Add(this.cmdReplace);
		this.c1CommandHolder1.Owner = this;
		this.lnkFilltip.Command = this.cmdFilltip;
		this.lnkReplace.Command = this.cmdReplace;
		this.lnkReplace.SortOrder = 1;
		this.lnkValidate.Command = this.cmdValidate;
		this.lnkValidate.Delimiter = true;
		this.lnkValidate.SortOrder = 2;
		this.lnkGenerate.Command = this.cmdGenerate;
		this.lnkGenerate.SortOrder = 3;
		this.lnkHelpDoc.Command = this.cmdHelpDoc;
		this.lnkHelpDoc.Delimiter = true;
		this.lnkHelpDoc.SortOrder = 4;
		this.pnlInput.Controls.Add(this.dockTabInput);
		this.pnlInput.Controls.Add(this.txtCurrency);
		this.pnlInput.Controls.Add(this.txtCompany);
		this.pnlInput.Controls.Add(this.lblCurrency);
		this.pnlInput.Controls.Add(this.lblCompany);
		this.pnlInput.Height = 40;
		this.pnlInput.KeepRelativeSize = false;
		this.pnlInput.Location = new System.Drawing.Point(0, 64);
		this.pnlInput.Name = "pnlInput";
		this.pnlInput.Resizable = false;
		this.pnlInput.Size = new System.Drawing.Size(1192, 40);
		this.pnlInput.SizeRatio = 9.709;
		this.pnlInput.TabIndex = 2;
		this.dockTabInput.Alignment = System.Windows.Forms.TabAlignment.Bottom;
		this.dockTabInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dockTabInput.Controls.Add(this.tabPageInputBoxBalance);
		this.dockTabInput.Controls.Add(this.tabPageInputBoxAuxiliary);
		this.dockTabInput.Controls.Add(this.tabPageInputBoxVoucher);
		this.dockTabInput.Location = new System.Drawing.Point(465, 3);
		this.dockTabInput.Name = "dockTabInput";
		this.dockTabInput.Size = new System.Drawing.Size(1131, 60);
		this.dockTabInput.TabIndex = 1;
		this.dockTabInput.TabsSpacing = 5;
		this.dockTabInput.TabStyle = C1.Win.C1Command.TabStyleEnum.Office2007;
		this.dockTabInput.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.dockTabInput.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2007Blue;
		this.tabPageInputBoxBalance.Controls.Add(this.ckbBalanceNonDirection);
		this.tabPageInputBoxBalance.Controls.Add(this.ckbBalanceHasAuxiliary);
		this.tabPageInputBoxBalance.Location = new System.Drawing.Point(0, 0);
		this.tabPageInputBoxBalance.Name = "tabPageInputBoxBalance";
		this.tabPageInputBoxBalance.Size = new System.Drawing.Size(1131, 24);
		this.tabPageInputBoxBalance.TabIndex = 0;
		this.tabPageInputBoxBalance.Text = "Page1";
		this.ckbBalanceNonDirection.BackColor = System.Drawing.Color.Transparent;
		this.ckbBalanceNonDirection.BorderColor = System.Drawing.Color.Transparent;
		this.ckbBalanceNonDirection.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbBalanceNonDirection.ForeColor = System.Drawing.Color.Black;
		this.ckbBalanceNonDirection.Location = new System.Drawing.Point(10, 5);
		this.ckbBalanceNonDirection.Name = "ckbBalanceNonDirection";
		this.ckbBalanceNonDirection.Padding = new System.Windows.Forms.Padding(1);
		this.ckbBalanceNonDirection.Size = new System.Drawing.Size(116, 24);
		this.ckbBalanceNonDirection.TabIndex = 7;
		this.ckbBalanceNonDirection.Text = "借贷金额分列";
		this.ckbBalanceNonDirection.UseVisualStyleBackColor = true;
		this.ckbBalanceNonDirection.Value = null;
		this.ckbBalanceHasAuxiliary.BackColor = System.Drawing.Color.Transparent;
		this.ckbBalanceHasAuxiliary.BorderColor = System.Drawing.Color.Transparent;
		this.ckbBalanceHasAuxiliary.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbBalanceHasAuxiliary.ForeColor = System.Drawing.Color.Black;
		this.ckbBalanceHasAuxiliary.Location = new System.Drawing.Point(130, 5);
		this.ckbBalanceHasAuxiliary.Name = "ckbBalanceHasAuxiliary";
		this.ckbBalanceHasAuxiliary.Padding = new System.Windows.Forms.Padding(1);
		this.ckbBalanceHasAuxiliary.Size = new System.Drawing.Size(116, 24);
		this.ckbBalanceHasAuxiliary.TabIndex = 7;
		this.ckbBalanceHasAuxiliary.Text = "存在辅助核算";
		this.ckbBalanceHasAuxiliary.UseVisualStyleBackColor = true;
		this.ckbBalanceHasAuxiliary.Value = null;
		this.tabPageInputBoxAuxiliary.Controls.Add(this.ckbAuxPageHasAuxiliary);
		this.tabPageInputBoxAuxiliary.Controls.Add(this.ckbAuxiliaryNonDirection);
		this.tabPageInputBoxAuxiliary.Location = new System.Drawing.Point(0, 0);
		this.tabPageInputBoxAuxiliary.Name = "tabPageInputBoxAuxiliary";
		this.tabPageInputBoxAuxiliary.Size = new System.Drawing.Size(1131, 24);
		this.tabPageInputBoxAuxiliary.TabIndex = 1;
		this.tabPageInputBoxAuxiliary.Text = "Page2";
		this.ckbAuxPageHasAuxiliary.BackColor = System.Drawing.Color.Transparent;
		this.ckbAuxPageHasAuxiliary.BorderColor = System.Drawing.Color.Transparent;
		this.ckbAuxPageHasAuxiliary.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbAuxPageHasAuxiliary.ForeColor = System.Drawing.Color.Black;
		this.ckbAuxPageHasAuxiliary.Location = new System.Drawing.Point(130, 5);
		this.ckbAuxPageHasAuxiliary.Name = "ckbAuxPageHasAuxiliary";
		this.ckbAuxPageHasAuxiliary.Padding = new System.Windows.Forms.Padding(1);
		this.ckbAuxPageHasAuxiliary.Size = new System.Drawing.Size(121, 24);
		this.ckbAuxPageHasAuxiliary.TabIndex = 9;
		this.ckbAuxPageHasAuxiliary.Text = "存在辅助核算";
		this.ckbAuxPageHasAuxiliary.UseVisualStyleBackColor = true;
		this.ckbAuxPageHasAuxiliary.Value = null;
		this.ckbAuxiliaryNonDirection.BackColor = System.Drawing.Color.Transparent;
		this.ckbAuxiliaryNonDirection.BorderColor = System.Drawing.Color.Transparent;
		this.ckbAuxiliaryNonDirection.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbAuxiliaryNonDirection.ForeColor = System.Drawing.Color.Black;
		this.ckbAuxiliaryNonDirection.Location = new System.Drawing.Point(10, 5);
		this.ckbAuxiliaryNonDirection.Name = "ckbAuxiliaryNonDirection";
		this.ckbAuxiliaryNonDirection.Padding = new System.Windows.Forms.Padding(1);
		this.ckbAuxiliaryNonDirection.Size = new System.Drawing.Size(116, 24);
		this.ckbAuxiliaryNonDirection.TabIndex = 8;
		this.ckbAuxiliaryNonDirection.Text = "借贷金额分列";
		this.ckbAuxiliaryNonDirection.UseVisualStyleBackColor = true;
		this.ckbAuxiliaryNonDirection.Value = null;
		this.tabPageInputBoxVoucher.Controls.Add(this.ckbAuxCodeNameSplit);
		this.tabPageInputBoxVoucher.Controls.Add(this.ckbVoucherNonDirection);
		this.tabPageInputBoxVoucher.Controls.Add(this.ckbAuxMultiColumn);
		this.tabPageInputBoxVoucher.Controls.Add(this.ckbTypeNumSplit);
		this.tabPageInputBoxVoucher.Controls.Add(this.ckbVoucherHasAuxiliary);
		this.tabPageInputBoxVoucher.Controls.Add(this.cboAuxStyle);
		this.tabPageInputBoxVoucher.Location = new System.Drawing.Point(0, 0);
		this.tabPageInputBoxVoucher.Name = "tabPageInputBoxVoucher";
		this.tabPageInputBoxVoucher.Size = new System.Drawing.Size(1131, 24);
		this.tabPageInputBoxVoucher.TabIndex = 2;
		this.tabPageInputBoxVoucher.Text = "Page3";
		this.ckbAuxCodeNameSplit.BackColor = System.Drawing.Color.Transparent;
		this.ckbAuxCodeNameSplit.BorderColor = System.Drawing.Color.Transparent;
		this.ckbAuxCodeNameSplit.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbAuxCodeNameSplit.ForeColor = System.Drawing.Color.Black;
		this.ckbAuxCodeNameSplit.Location = new System.Drawing.Point(526, 5);
		this.ckbAuxCodeNameSplit.Name = "ckbAuxCodeNameSplit";
		this.ckbAuxCodeNameSplit.Padding = new System.Windows.Forms.Padding(1);
		this.ckbAuxCodeNameSplit.Size = new System.Drawing.Size(176, 24);
		this.ckbAuxCodeNameSplit.TabIndex = 9;
		this.ckbAuxCodeNameSplit.Text = "辅助核算代码及名称分列";
		this.ckbAuxCodeNameSplit.UseVisualStyleBackColor = true;
		this.ckbAuxCodeNameSplit.Value = null;
		this.ckbVoucherNonDirection.BackColor = System.Drawing.Color.Transparent;
		this.ckbVoucherNonDirection.BorderColor = System.Drawing.Color.Transparent;
		this.ckbVoucherNonDirection.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbVoucherNonDirection.ForeColor = System.Drawing.Color.Black;
		this.ckbVoucherNonDirection.Location = new System.Drawing.Point(130, 5);
		this.ckbVoucherNonDirection.Name = "ckbVoucherNonDirection";
		this.ckbVoucherNonDirection.Padding = new System.Windows.Forms.Padding(1);
		this.ckbVoucherNonDirection.Size = new System.Drawing.Size(116, 24);
		this.ckbVoucherNonDirection.TabIndex = 8;
		this.ckbVoucherNonDirection.Text = "借贷金额分列";
		this.ckbVoucherNonDirection.UseVisualStyleBackColor = true;
		this.ckbVoucherNonDirection.Value = null;
		this.ckbAuxMultiColumn.BackColor = System.Drawing.Color.Transparent;
		this.ckbAuxMultiColumn.BorderColor = System.Drawing.Color.Transparent;
		this.ckbAuxMultiColumn.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbAuxMultiColumn.ForeColor = System.Drawing.Color.Black;
		this.ckbAuxMultiColumn.Location = new System.Drawing.Point(384, 5);
		this.ckbAuxMultiColumn.Name = "ckbAuxMultiColumn";
		this.ckbAuxMultiColumn.Padding = new System.Windows.Forms.Padding(1);
		this.ckbAuxMultiColumn.Size = new System.Drawing.Size(134, 24);
		this.ckbAuxMultiColumn.TabIndex = 8;
		this.ckbAuxMultiColumn.Text = "辅助核算类别分列";
		this.ckbAuxMultiColumn.UseVisualStyleBackColor = true;
		this.ckbAuxMultiColumn.Value = null;
		this.ckbTypeNumSplit.BackColor = System.Drawing.Color.Transparent;
		this.ckbTypeNumSplit.BorderColor = System.Drawing.Color.Transparent;
		this.ckbTypeNumSplit.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbTypeNumSplit.ForeColor = System.Drawing.Color.Black;
		this.ckbTypeNumSplit.Location = new System.Drawing.Point(10, 5);
		this.ckbTypeNumSplit.Name = "ckbTypeNumSplit";
		this.ckbTypeNumSplit.Padding = new System.Windows.Forms.Padding(1);
		this.ckbTypeNumSplit.Size = new System.Drawing.Size(116, 24);
		this.ckbTypeNumSplit.TabIndex = 6;
		this.ckbTypeNumSplit.Text = "凭证字号分列";
		this.ckbTypeNumSplit.UseVisualStyleBackColor = true;
		this.ckbTypeNumSplit.Value = null;
		this.ckbVoucherHasAuxiliary.BackColor = System.Drawing.Color.Transparent;
		this.ckbVoucherHasAuxiliary.BorderColor = System.Drawing.Color.Transparent;
		this.ckbVoucherHasAuxiliary.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbVoucherHasAuxiliary.ForeColor = System.Drawing.Color.Black;
		this.ckbVoucherHasAuxiliary.Location = new System.Drawing.Point(257, 5);
		this.ckbVoucherHasAuxiliary.Name = "ckbVoucherHasAuxiliary";
		this.ckbVoucherHasAuxiliary.Padding = new System.Windows.Forms.Padding(1);
		this.ckbVoucherHasAuxiliary.Size = new System.Drawing.Size(121, 24);
		this.ckbVoucherHasAuxiliary.TabIndex = 4;
		this.ckbVoucherHasAuxiliary.Text = "存在辅助核算";
		this.ckbVoucherHasAuxiliary.UseVisualStyleBackColor = true;
		this.ckbVoucherHasAuxiliary.Value = null;
		this.ckbVoucherHasAuxiliary.CheckedChanged += new System.EventHandler(ckbVoucherHasAuxiliary_CheckedChanged);
		this.cboAuxStyle.AllowSpinLoop = false;
		this.cboAuxStyle.BackColor = System.Drawing.Color.FromArgb(234, 242, 251);
		this.cboAuxStyle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.cboAuxStyle.GapHeight = 0;
		this.cboAuxStyle.ImagePadding = new System.Windows.Forms.Padding(0);
		this.cboAuxStyle.ItemsDisplayMember = "";
		this.cboAuxStyle.ItemsValueMember = "";
		this.cboAuxStyle.Location = new System.Drawing.Point(378, 6);
		this.cboAuxStyle.Name = "cboAuxStyle";
		this.cboAuxStyle.Size = new System.Drawing.Size(200, 29);
		this.cboAuxStyle.TabIndex = 9;
		this.cboAuxStyle.Tag = null;
		this.txtCurrency.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCurrency.Location = new System.Drawing.Point(348, 9);
		this.txtCurrency.Name = "txtCurrency";
		this.txtCurrency.Size = new System.Drawing.Size(100, 29);
		this.txtCurrency.TabIndex = 3;
		this.txtCurrency.Tag = null;
		this.txtCurrency.TextDetached = true;
		this.txtCompany.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtCompany.Location = new System.Drawing.Point(85, 9);
		this.txtCompany.Name = "txtCompany";
		this.txtCompany.Size = new System.Drawing.Size(189, 29);
		this.txtCompany.TabIndex = 2;
		this.txtCompany.Tag = null;
		this.txtCompany.TextDetached = true;
		this.lblCurrency.AutoSize = true;
		this.lblCurrency.BackColor = System.Drawing.Color.Transparent;
		this.lblCurrency.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCurrency.ForeColor = System.Drawing.Color.Black;
		this.lblCurrency.Location = new System.Drawing.Point(289, 12);
		this.lblCurrency.Name = "lblCurrency";
		this.lblCurrency.Size = new System.Drawing.Size(100, 24);
		this.lblCurrency.TabIndex = 1;
		this.lblCurrency.Tag = null;
		this.lblCurrency.Text = "金额单位：";
		this.lblCurrency.TextDetached = true;
		this.lblCompany.AutoSize = true;
		this.lblCompany.BackColor = System.Drawing.Color.Transparent;
		this.lblCompany.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblCompany.ForeColor = System.Drawing.Color.Black;
		this.lblCompany.Location = new System.Drawing.Point(3, 11);
		this.lblCompany.Name = "lblCompany";
		this.lblCompany.Size = new System.Drawing.Size(136, 24);
		this.lblCompany.TabIndex = 0;
		this.lblCompany.Tag = null;
		this.lblCompany.Text = "核算单位名称：";
		this.lblCompany.TextDetached = true;
		this.pnlDatas.Controls.Add(this.dockTab);
		this.pnlDatas.Height = 372;
		this.pnlDatas.Location = new System.Drawing.Point(0, 105);
		this.pnlDatas.MinHeight = 52;
		this.pnlDatas.MinWidth = 52;
		this.pnlDatas.Name = "pnlDatas";
		this.pnlDatas.Size = new System.Drawing.Size(1192, 372);
		this.pnlDatas.SizeRatio = 100.0;
		this.pnlDatas.TabIndex = 0;
		this.pnlDatas.Width = 1192;
		this.dockTab.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dockTab.Controls.Add(this.tabBalance);
		this.dockTab.Controls.Add(this.tabAuxiliary);
		this.dockTab.Controls.Add(this.tabVoucher);
		this.dockTab.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dockTab.Location = new System.Drawing.Point(0, 0);
		this.dockTab.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.dockTab.Name = "dockTab";
		this.dockTab.Size = new System.Drawing.Size(1192, 372);
		this.dockTab.TabIndex = 1;
		this.dockTab.TabsShowFocusCues = false;
		this.dockTab.TabsSpacing = 0;
		this.dockTab.TabStyle = C1.Win.C1Command.TabStyleEnum.WindowsXP;
		this.dockTab.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.dockTab.VisualStyleBase = C1.Win.C1Command.VisualStyle.WindowsXP;
		this.tabBalance.Controls.Add(this.grdBalance);
		this.tabBalance.Location = new System.Drawing.Point(0, 36);
		this.tabBalance.Name = "tabBalance";
		this.tabBalance.Size = new System.Drawing.Size(1192, 336);
		this.tabBalance.TabIndex = 0;
		this.tabBalance.Text = "年初科目余额表";
		this.grdBalance.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.grdBalance.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdBalance.ColumnInfo = "10,1,0,0,0,150,Columns:";
		this.grdBalance.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdBalance.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.grdBalance.Location = new System.Drawing.Point(0, 0);
		this.grdBalance.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdBalance.Name = "grdBalance";
		this.grdBalance.Rows.DefaultSize = 30;
		this.grdBalance.Size = new System.Drawing.Size(1192, 336);
		this.grdBalance.TabIndex = 0;
		this.tabAuxiliary.Controls.Add(this.grdAuxiliary);
		this.tabAuxiliary.Location = new System.Drawing.Point(0, 36);
		this.tabAuxiliary.Name = "tabAuxiliary";
		this.tabAuxiliary.Size = new System.Drawing.Size(1192, 336);
		this.tabAuxiliary.TabIndex = 2;
		this.tabAuxiliary.Text = "年初辅助余额表";
		this.grdAuxiliary.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.grdAuxiliary.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdAuxiliary.ColumnInfo = "10,1,0,0,0,150,Columns:";
		this.grdAuxiliary.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdAuxiliary.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.grdAuxiliary.Location = new System.Drawing.Point(0, 0);
		this.grdAuxiliary.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdAuxiliary.Name = "grdAuxiliary";
		this.grdAuxiliary.Rows.DefaultSize = 30;
		this.grdAuxiliary.Size = new System.Drawing.Size(1192, 336);
		this.grdAuxiliary.TabIndex = 1;
		this.tabVoucher.Controls.Add(this.grdVoucher);
		this.tabVoucher.Location = new System.Drawing.Point(0, 36);
		this.tabVoucher.Name = "tabVoucher";
		this.tabVoucher.Size = new System.Drawing.Size(1192, 336);
		this.tabVoucher.TabIndex = 1;
		this.tabVoucher.Text = "会计凭证库";
		this.grdVoucher.AllowSorting = C1.Win.C1FlexGrid.AllowSortingEnum.None;
		this.grdVoucher.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdVoucher.ColumnInfo = "10,1,0,0,0,150,Columns:";
		this.grdVoucher.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdVoucher.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.grdVoucher.Location = new System.Drawing.Point(0, 0);
		this.grdVoucher.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdVoucher.Name = "grdVoucher";
		this.grdVoucher.Rows.DefaultSize = 30;
		this.grdVoucher.Size = new System.Drawing.Size(1192, 336);
		this.grdVoucher.TabIndex = 0;
		this.cmdGenerate.Image = (System.Drawing.Image)resources.GetObject("cmdGenerate.Image");
		this.cmdGenerate.Name = "cmdGenerate";
		this.cmdGenerate.ShortcutText = "";
		this.cmdGenerate.Text = "生成账套";
		this.cmdGenerate.Click += new C1.Win.C1Command.ClickEventHandler(cmdGenerate_Click);
		this.cmdFilltip.CheckAutoToggle = true;
		this.cmdFilltip.Image = (System.Drawing.Image)resources.GetObject("cmdFilltip.Image");
		this.cmdFilltip.Name = "cmdFilltip";
		this.cmdFilltip.ShortcutText = "";
		this.cmdFilltip.Text = "填表提示";
		this.cmdFilltip.CheckedChanged += new C1.Win.C1Command.CheckedChangedEventHandler(cmdFilltip_CheckedChanged);
		this.cmdValidate.Image = (System.Drawing.Image)resources.GetObject("cmdValidate.Image");
		this.cmdValidate.Name = "cmdValidate";
		this.cmdValidate.ShortcutText = "";
		this.cmdValidate.Text = "校验数据";
		this.cmdValidate.Click += new C1.Win.C1Command.ClickEventHandler(cmdValidate_Click);
		this.cmdHelpDoc.Image = Auditai.UI.LedgerView.Properties.Resources.HelpCenter;
		this.cmdHelpDoc.Name = "cmdHelpDoc";
		this.cmdHelpDoc.ShortcutText = "";
		this.cmdHelpDoc.Text = "帮助中心";
		this.cmdHelpDoc.Click += new C1.Win.C1Command.ClickEventHandler(cmdHelpDoc_Click);
		this.cmdReplace.Image = Auditai.UI.LedgerView.Properties.Resources.Replace;
		this.cmdReplace.Name = "cmdReplace";
		this.cmdReplace.ShortcutText = "";
		this.cmdReplace.Text = "查找替换";
		this.cmdReplace.Click += new C1.Win.C1Command.ClickEventHandler(cmdReplace_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(11f, 24f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1192, 477);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmImport";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "账套生成器";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlTools.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1CommandDock1).EndInit();
		this.c1CommandDock1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		this.pnlInput.ResumeLayout(false);
		this.pnlInput.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.dockTabInput).EndInit();
		this.dockTabInput.ResumeLayout(false);
		this.tabPageInputBoxBalance.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ckbBalanceNonDirection).EndInit();
		this.tabPageInputBoxAuxiliary.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ckbAuxPageHasAuxiliary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbAuxiliaryNonDirection).EndInit();
		this.tabPageInputBoxVoucher.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ckbAuxCodeNameSplit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbVoucherNonDirection).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbAuxMultiColumn).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbTypeNumSplit).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbVoucherHasAuxiliary).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cboAuxStyle).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtCurrency).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtCompany).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCurrency).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblCompany).EndInit();
		this.pnlDatas.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dockTab).EndInit();
		this.dockTab.ResumeLayout(false);
		this.tabBalance.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdBalance).EndInit();
		this.tabAuxiliary.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdAuxiliary).EndInit();
		this.tabVoucher.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdVoucher).EndInit();
		base.ResumeLayout(false);
	}
}
