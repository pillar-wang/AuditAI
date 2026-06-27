using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public class frmAuxEdit : Form
{
	private bool _allowFreeInput;
	private bool _allowMultiSelect;
	private bool _attachEvent;
	private string _commentNotice;
	private string _commentValue;
	private string _defaultNotice;
	private string _defaultValue;
	private string _dropNotice;
	private string _dropValue;
	private bool _isTextChanging;
	private AuxEditor _owner;
	private bool _queryValueChanged;
	private bool _queryDefaultValueChanged;
	private bool _queryCommentValueChanged;

	private C1Button btnCancle;
	private C1Button btnConfirm;
	private C1CommandHolder c1CommandHolder1;
	private C1CommandLink c1CommandLink1;
	private C1ContextMenu c1ContextMenu1;
	private C1SplitterPanel c1SplitterPanel2;
	private C1SplitterPanel c1SplitterPanel4;
	private C1CheckBox ckbFreeInput;
	private C1CheckBox ckbMultiSelect;
	private C1Command cmdCopy1;
	private C1Command cmdCopy2;
	private C1Command cmdCopy3;
	private C1Command cmdCut1;
	private C1Command cmdCut2;
	private C1Command cmdCut3;
	private C1Command cmdInputList;
	private C1Command cmdInsertColumn1;
	private C1Command cmdInsertVariable1;
	private C1Command cmdInsertVariable2;
	private C1Command cmdInsertVariable3;
	private C1Command cmdMultiList;
	private C1Command cmdOtherFunc;
	private C1Command cmdPaste1;
	private C1Command cmdPaste2;
	private C1Command cmdPaste3;
	private C1Command cmdTableList;
	private C1Command cmdTreeList;
	private IContainer components;
	private C1SplitContainer ctnCommentInput;
	private C1SplitContainer ctnDefaultInput;
	private C1SplitContainer ctnDock;
	private C1SplitContainer ctnDropInput;
	private C1ContextMenu ctx1;
	private C1ContextMenu ctx2;
	private C1ContextMenu ctx3;
	private C1ContextMenu ctxOtherFunc;
	private C1Label lblFunctionHint;
	private C1CommandLink lnkCopy1;
	private C1CommandLink lnkCopy2;
	private C1CommandLink lnkCopy3;
	private C1CommandLink lnkCut1;
	private C1CommandLink lnkCut2;
	private C1CommandLink lnkCut3;
	private C1CommandLink lnkInputList;
	private C1CommandLink lnkInsertColumn1;
	private C1CommandLink lnkInsertVariable1;
	private C1CommandLink lnkInsertVariable2;
	private C1CommandLink lnkInsertVariable3;
	private C1CommandLink lnkMultiList;
	private C1CommandLink lnkOtherFunc;
	private C1CommandLink lnkPaste1;
	private C1CommandLink lnkPaste2;
	private C1CommandLink lnkPaste3;
	private C1CommandLink lnkTableList;
	private C1CommandLink lnkTreeList;
	private C1SplitterPanel pnlButtons;
	private C1SplitterPanel pnlCombo;
	private C1SplitterPanel pnlFunctionHint;
	private C1SplitterPanel pnlFunctions;
	private C1SplitterPanel pnlInput;
	private C1SplitterPanel pnlInputBox;
	private C1ContextMenu txbDropInputContextMenu;
	private C1ToolBar tbrFunctions;

	public C1DockingTab DockingTab;
	public C1DockingTabPage tabDefault;
	public C1DockingTabPage tabDropList;
	public C1DockingTabPage tabEdit;
	public RichTextBoxEx rtbDropInput;
	public C1TextBoxEx txbCommentInput;
	public C1TextBoxEx txbDefaultInput;

	public frmAuxEdit(AuxEditor owner)
	{
		ctx1 = new C1ContextMenu();
		cmdCopy1 = new C1Command();
		lnkCopy1 = new C1CommandLink();
		cmdCut1 = new C1Command();
		lnkCut1 = new C1CommandLink();
		cmdPaste1 = new C1Command();
		lnkPaste1 = new C1CommandLink();
		cmdInsertVariable1 = new C1Command();
		lnkInsertVariable1 = new C1CommandLink();
		cmdInsertColumn1 = new C1Command();
		lnkInsertColumn1 = new C1CommandLink();
		ctx2 = new C1ContextMenu();
		cmdCopy2 = new C1Command();
		lnkCopy2 = new C1CommandLink();
		cmdCut2 = new C1Command();
		lnkCut2 = new C1CommandLink();
		cmdPaste2 = new C1Command();
		lnkPaste2 = new C1CommandLink();
		cmdInsertVariable2 = new C1Command();
		lnkInsertVariable2 = new C1CommandLink();
		ctx3 = new C1ContextMenu();
		cmdCopy3 = new C1Command();
		lnkCopy3 = new C1CommandLink();
		cmdCut3 = new C1Command();
		lnkCut3 = new C1CommandLink();
		cmdPaste3 = new C1Command();
		lnkPaste3 = new C1CommandLink();
		cmdInsertVariable3 = new C1Command();
		lnkInsertVariable3 = new C1CommandLink();
		cmdTreeList = new C1Command { Text = "TreeList函数" };
		cmdTableList = new C1Command { Text = "TableList函数" };
		cmdMultiList = new C1Command { Text = "MultiList函数" };
		cmdInputList = new C1Command { Text = "InputList函数" };
		cmdOtherFunc = new C1Command { Text = "其他函数" };
		ctxOtherFunc = new C1ContextMenu();
		_owner = owner;
		InitializeComponent();
		Initialize();
	}

	private TextBoxBase ActiveTextBox()
	{
		if (DockingTab.SelectedTab == tabDropList)
		{
			return rtbDropInput;
		}
		if (DockingTab.SelectedTab == tabEdit)
		{
			return txbCommentInput;
		}
		return txbDefaultInput;
	}

	private void AttachEvent()
	{
		if (_attachEvent) return;
		ckbFreeInput.CheckedChanged += ckbFreeInput_CheckedChanged;
		ckbMultiSelect.CheckedChanged += ckbMultiSelect_CheckedChanged;
		rtbDropInput.TextChanged += txbDropInput_TextChanged;
		txbDefaultInput.TextChanged += txbDefaultInput_TextChanged;
		txbCommentInput.TextChanged += txbCommentInput_TextChanged;
		_attachEvent = true;
	}

	private void btnCancle_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		if (!ValidateFormula(out var formula)) return;
		DialogResult = DialogResult.OK;
		Value = formula;
		DefaultValue = txbDefaultInput.Text;
		CommentValue = txbCommentInput.Text;
		Close();
	}

	private void ckbFreeInput_CheckedChanged(object sender, EventArgs e)
	{
		AllowFreeInput = ckbFreeInput.Checked;
		QueryValueChanged = true;
	}

	private void ckbMultiSelect_CheckedChanged(object sender, EventArgs e)
	{
		AllowMultiSelect = ckbMultiSelect.Checked;
		if (AllowMultiSelect)
		{
			AllowFreeInput = false;
		}
		else
		{
			ckbFreeInput.Enabled = true;
		}
		QueryValueChanged = true;
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		try
		{
			TextBoxBase textBoxBase = ActiveTextBox();
			string selectedText = textBoxBase.SelectedText;
			if (string.IsNullOrEmpty(selectedText))
			{
				Clipboard.Clear();
			}
			else
			{
				Clipboard.SetText(selectedText);
			}
		}
		catch
		{
		}
	}

	private void CmdCut_Click(object sender, ClickEventArgs e)
	{
		TextBoxBase textBoxBase = ActiveTextBox();
		try
		{
			string selectedText = textBoxBase.SelectedText;
			if (string.IsNullOrEmpty(selectedText))
			{
				Clipboard.Clear();
			}
			else
			{
				Clipboard.SetText(selectedText);
			}
		}
		catch
		{
		}
		textBoxBase.SelectedText = string.Empty;
	}

	private void CmdInputList_Click(object sender, ClickEventArgs e)
	{
		rtbDropInput.SelectedText = "InputList()";
		rtbDropInput.SelectionStart = rtbDropInput.SelectionStart + rtbDropInput.SelectionLength - 1;
	}

	private void CmdInsertColumn1_Click(object sender, ClickEventArgs e)
	{
		MergeForm form = MergeForm.GetInstance();
		form.Show(Program.MainForm.CurrentProject);
		form.AfterSelected += (s, column) =>
		{
			ActiveTextBox().SelectedText = "{" + column.Table.GetCanonicalName() + "}[" + column.GetUniqueFormulaName() + "]";
			form.Hide();
		};
	}

	private void CmdInsertVariable_Click(object sender, ClickEventArgs e)
	{
		ReferenceEditor referenceEditor = new ReferenceEditor();
		if (referenceEditor.ShowSelect() == DialogResult.OK)
		{
			DataReference selectedReference = referenceEditor.SelectedReference;
			TextBoxBase textBoxBase = ActiveTextBox();
			textBoxBase.SelectedText = textBoxBase.SelectedText + "Var(\"" + selectedReference.Key + "\")";
		}
	}

	private void CmdMultiList_Click(object sender, ClickEventArgs e)
	{
		rtbDropInput.SelectedText = "MultiList()";
		rtbDropInput.SelectionStart = rtbDropInput.SelectionStart + rtbDropInput.SelectionLength - 1;
	}

	private void CmdOtherFunc_Click(object sender, ClickEventArgs e)
	{
		ctxOtherFunc.ShowContextMenu(tbrFunctions, tbrFunctions.PointToClient(Cursor.Position));
	}

	private void CmdPaste_Click(object sender, ClickEventArgs e)
	{
		try
		{
			TextBoxBase textBoxBase = ActiveTextBox();
			string text = Clipboard.GetText();
			textBoxBase.SelectedText = text;
		}
		catch
		{
		}
	}

	private void CmdTableList_Click(object sender, ClickEventArgs e)
	{
		rtbDropInput.SelectedText = "TableList()";
		rtbDropInput.SelectionStart = rtbDropInput.SelectionStart + rtbDropInput.SelectionLength - 1;
	}

	private void CmdTreeList_Click(object sender, ClickEventArgs e)
	{
		rtbDropInput.SelectedText = "TreeList()";
		rtbDropInput.SelectionStart = rtbDropInput.SelectionStart + rtbDropInput.SelectionLength - 1;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void FrmAuxEdit_FormClosing(object sender, FormClosingEventArgs e)
	{
		_owner.IsEditing = false;
		_owner.OnClosed();
	}

	private void FrmAuxEdit_Shown(object sender, EventArgs e)
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		ResetQueryStatus();
		_owner.IsEditing = true;
	}

	public bool AllowFreeInput
	{
		get { return _allowFreeInput; }
		set
		{
			_allowFreeInput = value;
			ckbFreeInput.Checked = value;
		}
	}

	public bool AllowMultiSelect
	{
		get { return _allowMultiSelect; }
		set
		{
			_allowMultiSelect = value;
			ckbMultiSelect.Checked = value;
			if (value)
			{
				ckbFreeInput.Checked = false;
				ckbFreeInput.Enabled = false;
			}
		}
	}

	public string CommentNotice
	{
		get { return _commentNotice; }
		set { _commentNotice = value; }
	}

	public string CommentValue
	{
		get { return _commentValue; }
		set
		{
			_commentValue = value;
			txbCommentInput.Text = CommentValue;
		}
	}

	public string DefaultNotice
	{
		get { return _defaultNotice; }
		set { _defaultNotice = value; }
	}

	public string DefaultValue
	{
		get { return _defaultValue; }
		set
		{
			_defaultValue = value;
			txbDefaultInput.Text = DefaultValue;
		}
	}

	public string Notice
	{
		get { return _dropNotice; }
		set { _dropNotice = value; }
	}

	public bool QueryCommentValueChanged
	{
		get { return _queryCommentValueChanged; }
		set { _queryCommentValueChanged = value; }
	}

	public bool QueryDefaultValueChanged
	{
		get { return _queryDefaultValueChanged; }
		set { _queryDefaultValueChanged = value; }
	}

	public bool QueryValueChanged
	{
		get { return _queryValueChanged; }
		set { _queryValueChanged = value; }
	}

	public string Value
	{
		get { return _dropValue; }
		set
		{
			_dropValue = value;
			rtbDropInput.Text = Value;
		}
	}

	public bool UseWildcard => UseWildcardImpl(rtbDropInput.Text, rtbDropInput.SelectionStart);

	private void Initialize()
	{
		DockingTab.ShowTabs = false;
		rtbDropInput.SelectionChanged += TxbDropInput_SelectionChanged;

		cmdCopy1.Text = "复制";
		cmdCopy1.Click += CmdCopy_Click;
		lnkCopy1.Command = cmdCopy1;
		cmdCut1.Text = "剪切";
		cmdCut1.Click += CmdCut_Click;
		lnkCut1.Command = cmdCut1;
		cmdPaste1.Text = "粘贴";
		cmdPaste1.Click += CmdPaste_Click;
		lnkPaste1.Command = cmdPaste1;
		cmdInsertVariable1.Text = "插入变量";
		cmdInsertVariable1.Click += CmdInsertVariable_Click;
		lnkInsertVariable1.Command = cmdInsertVariable1;
		cmdInsertColumn1.Text = "插入列来源";
		cmdInsertColumn1.Click += CmdInsertColumn1_Click;
		lnkInsertColumn1.Command = cmdInsertColumn1;
		ctx1.CommandLinks.Add(lnkCopy1);
		ctx1.CommandLinks.Add(lnkCut1);
		ctx1.CommandLinks.Add(lnkPaste1);
		ctx1.CommandLinks.Add(lnkInsertVariable1);
		ctx1.CommandLinks.Add(lnkInsertColumn1);

		cmdCopy2.Text = "复制";
		cmdCopy2.Click += CmdCopy_Click;
		lnkCopy2.Command = cmdCopy2;
		cmdCut2.Text = "剪切";
		cmdCut2.Click += CmdCut_Click;
		lnkCut2.Command = cmdCut2;
		cmdPaste2.Text = "粘贴";
		cmdPaste2.Click += CmdPaste_Click;
		lnkPaste2.Command = cmdPaste2;
		cmdInsertVariable2.Text = "插入变量";
		cmdInsertVariable2.Click += CmdInsertVariable_Click;
		lnkInsertVariable2.Command = cmdInsertVariable2;
		ctx2.CommandLinks.Add(lnkCopy2);
		ctx2.CommandLinks.Add(lnkCut2);
		ctx2.CommandLinks.Add(lnkPaste2);
		ctx2.CommandLinks.Add(lnkInsertVariable2);

		cmdCopy3.Text = "复制";
		cmdCopy3.Click += CmdCopy_Click;
		lnkCopy3.Command = cmdCopy3;
		cmdCut3.Text = "剪切";
		cmdCut3.Click += CmdCut_Click;
		lnkCut3.Command = cmdCut3;
		cmdPaste3.Text = "粘贴";
		cmdPaste3.Click += CmdPaste_Click;
		lnkPaste3.Command = cmdPaste3;
		cmdInsertVariable3.Text = "插入变量";
		cmdInsertVariable3.Click += CmdInsertVariable_Click;
		lnkInsertVariable3.Command = cmdInsertVariable3;
		ctx3.CommandLinks.Add(lnkCopy3);
		ctx3.CommandLinks.Add(lnkCut3);
		ctx3.CommandLinks.Add(lnkPaste3);
		ctx3.CommandLinks.Add(lnkInsertVariable3);

		c1CommandHolder1.SetC1ContextMenu(rtbDropInput, ctx1);
		c1CommandHolder1.SetC1ContextMenu(txbCommentInput, ctx2);
		c1CommandHolder1.SetC1ContextMenu(txbDefaultInput, ctx3);
		AttachEvent();
		Shown += FrmAuxEdit_Shown;
		FormClosing += FrmAuxEdit_FormClosing;

		cmdTreeList.Click += CmdTreeList_Click;
		lnkTreeList = new C1CommandLink(cmdTreeList);
		lnkTreeList.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add(lnkTreeList);

		cmdTableList.Click += CmdTableList_Click;
		lnkTableList = new C1CommandLink(cmdTableList);
		lnkTableList.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add(lnkTableList);

		cmdMultiList.Click += CmdMultiList_Click;
		lnkMultiList = new C1CommandLink(cmdMultiList);
		lnkMultiList.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add(lnkMultiList);

		cmdInputList.Click += CmdInputList_Click;
		lnkInputList = new C1CommandLink(cmdInputList);
		lnkInputList.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add(lnkInputList);

		cmdOtherFunc.Click += CmdOtherFunc_Click;
		lnkOtherFunc = new C1CommandLink(cmdOtherFunc);
		lnkOtherFunc.ButtonLook = ButtonLookFlags.Text;
		tbrFunctions.CommandLinks.Add(lnkOtherFunc);

		PopulateOtherFunctions();

		rtbDropInput.KeyDown += TxbDropInput_KeyDown;
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
	}

	private void InitializeComponent()
	{
		components = new Container();
		rtbDropInput = new RichTextBoxEx();
		ctnDock = new C1SplitContainer();
		pnlButtons = new C1SplitterPanel();
		btnCancle = new C1Button();
		btnConfirm = new C1Button();
		pnlInputBox = new C1SplitterPanel();
		DockingTab = new C1DockingTab();
		tabDropList = new C1DockingTabPage();
		ctnDropInput = new C1SplitContainer();
		pnlInput = new C1SplitterPanel();
		ckbFreeInput = new C1CheckBox();
		ckbMultiSelect = new C1CheckBox();
		pnlFunctions = new C1SplitterPanel();
		tbrFunctions = new C1ToolBar();
		c1CommandHolder1 = new C1CommandHolder();
		c1ContextMenu1 = new C1ContextMenu();
		c1CommandLink1 = new C1CommandLink();
		txbDropInputContextMenu = new C1ContextMenu();
		pnlFunctionHint = new C1SplitterPanel();
		lblFunctionHint = new C1Label();
		pnlCombo = new C1SplitterPanel();
		tabEdit = new C1DockingTabPage();
		ctnCommentInput = new C1SplitContainer();
		c1SplitterPanel4 = new C1SplitterPanel();
		txbCommentInput = new C1TextBoxEx();
		tabDefault = new C1DockingTabPage();
		ctnDefaultInput = new C1SplitContainer();
		c1SplitterPanel2 = new C1SplitterPanel();
		txbDefaultInput = new C1TextBoxEx();

		((ISupportInitialize)ctnDock).BeginInit();
		ctnDock.SuspendLayout();
		pnlButtons.SuspendLayout();
		((ISupportInitialize)btnCancle).BeginInit();
		((ISupportInitialize)btnConfirm).BeginInit();
		pnlInputBox.SuspendLayout();
		((ISupportInitialize)DockingTab).BeginInit();
		DockingTab.SuspendLayout();
		tabDropList.SuspendLayout();
		((ISupportInitialize)ctnDropInput).BeginInit();
		ctnDropInput.SuspendLayout();
		pnlInput.SuspendLayout();
		((ISupportInitialize)ckbFreeInput).BeginInit();
		((ISupportInitialize)ckbMultiSelect).BeginInit();
		pnlFunctions.SuspendLayout();
		((ISupportInitialize)c1CommandHolder1).BeginInit();
		pnlFunctionHint.SuspendLayout();
		((ISupportInitialize)lblFunctionHint).BeginInit();
		pnlCombo.SuspendLayout();
		tabEdit.SuspendLayout();
		((ISupportInitialize)ctnCommentInput).BeginInit();
		ctnCommentInput.SuspendLayout();
		c1SplitterPanel4.SuspendLayout();
		((ISupportInitialize)txbCommentInput).BeginInit();
		tabDefault.SuspendLayout();
		((ISupportInitialize)ctnDefaultInput).BeginInit();
		ctnDefaultInput.SuspendLayout();
		c1SplitterPanel2.SuspendLayout();
		((ISupportInitialize)txbDefaultInput).BeginInit();
		SuspendLayout();

		rtbDropInput.BorderStyle = BorderStyle.FixedSingle;
		rtbDropInput.Dock = DockStyle.Fill;
		rtbDropInput.Location = new Point(0, 0);
		rtbDropInput.Margin = new Padding(3, 4, 3, 4);
		rtbDropInput.Name = "rtbDropInput";
		rtbDropInput.Size = new Size(792, 354);
		rtbDropInput.TabIndex = 0;
		rtbDropInput.Text = "";
		rtbDropInput.VScrollPos = 0;

		ctnDock.AutoSizeElement = AutoSizeElement.Both;
		ctnDock.BackColor = Color.FromArgb(164, 195, 235);
		ctnDock.CollapsingAreaColor = Color.FromArgb(221, 231, 238);
		ctnDock.Dock = DockStyle.Fill;
		ctnDock.FixedLineColor = Color.FromArgb(119, 147, 185);
		ctnDock.ForeColor = Color.FromArgb(21, 66, 139);
		ctnDock.HeaderHeight = 27;
		ctnDock.Location = new Point(0, 0);
		ctnDock.Margin = new Padding(3, 4, 3, 4);
		ctnDock.Name = "ctnDock";
		ctnDock.Panels.Add(pnlButtons);
		ctnDock.Panels.Add(pnlInputBox);
		ctnDock.Size = new Size(792, 569);
		ctnDock.SplitterColor = Color.FromArgb(119, 147, 185);
		ctnDock.SplitterWidth = 0;
		ctnDock.TabIndex = 1;
		ctnDock.ToolTipGradient = ToolTipGradient.Blue;

		pnlButtons.Controls.Add(btnCancle);
		pnlButtons.Controls.Add(btnConfirm);
		pnlButtons.Dock = PanelDockStyle.Top;
		pnlButtons.Height = 40;
		pnlButtons.KeepRelativeSize = false;
		pnlButtons.Location = new Point(0, 529);
		pnlButtons.Name = "pnlButtons";
		pnlButtons.Resizable = false;
		pnlButtons.Size = new Size(792, 40);
		pnlButtons.TabIndex = 2;
		pnlButtons.Width = 792;

		btnCancle.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		btnCancle.Location = new Point(699, 7);
		btnCancle.Margin = new Padding(3, 4, 3, 4);
		btnCancle.Name = "btnCancle";
		btnCancle.Size = new Size(70, 26);
		btnCancle.TabIndex = 1;
		btnCancle.Text = "取消";
		btnCancle.UseVisualStyleBackColor = true;
		btnCancle.VisualStyleBaseStyle = (C1.Win.C1Input.VisualStyle)2;
		btnCancle.Click += btnCancle_Click;

		btnConfirm.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		btnConfirm.Location = new Point(596, 7);
		btnConfirm.Margin = new Padding(3, 4, 3, 4);
		btnConfirm.Name = "btnConfirm";
		btnConfirm.Size = new Size(70, 26);
		btnConfirm.TabIndex = 0;
		btnConfirm.Text = "确定";
		btnConfirm.UseVisualStyleBackColor = true;
		btnConfirm.Click += btnConfirm_Click;

		pnlInputBox.Controls.Add(DockingTab);
		pnlInputBox.Height = 528;
		pnlInputBox.Location = new Point(0, 0);
		pnlInputBox.MinHeight = 0;
		pnlInputBox.MinWidth = 52;
		pnlInputBox.Name = "pnlInputBox";
		pnlInputBox.Size = new Size(792, 528);
		pnlInputBox.SizeRatio = 100.0;
		pnlInputBox.TabIndex = 1;
		pnlInputBox.Width = 792;

		DockingTab.BorderStyle = BorderStyle.None;
		DockingTab.Controls.Add(tabDropList);
		DockingTab.Controls.Add(tabEdit);
		DockingTab.Controls.Add(tabDefault);
		DockingTab.Dock = DockStyle.Fill;
		DockingTab.Location = new Point(0, 0);
		DockingTab.Name = "DockingTab";
		DockingTab.Size = new Size(792, 528);
		DockingTab.TabIndex = 1;
		DockingTab.TabsShowFocusCues = false;
		DockingTab.TabsSpacing = 5;
		DockingTab.TabStyle = (TabStyleEnum)6;
		DockingTab.VisualStyle = (C1.Win.C1Command.VisualStyle)0;
		DockingTab.VisualStyleBase = (C1.Win.C1Command.VisualStyle)5;

		tabDropList.Controls.Add(ctnDropInput);
		tabDropList.Location = new Point(0, 27);
		tabDropList.Name = "tabDropList";
		tabDropList.Size = new Size(792, 501);
		tabDropList.TabIndex = 0;
		tabDropList.Text = "下拉列表";

		ctnDropInput.AutoSizeElement = AutoSizeElement.Both;
		ctnDropInput.BackColor = Color.FromArgb(240, 240, 240);
		ctnDropInput.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnDropInput.Dock = DockStyle.Fill;
		ctnDropInput.ForeColor = Color.FromArgb(0, 0, 0);
		ctnDropInput.Location = new Point(0, 0);
		ctnDropInput.Name = "ctnDropInput";
		ctnDropInput.Panels.Add(pnlInput);
		ctnDropInput.Panels.Add(pnlFunctions);
		ctnDropInput.Panels.Add(pnlFunctionHint);
		ctnDropInput.Panels.Add(pnlCombo);
		ctnDropInput.Size = new Size(792, 501);
		ctnDropInput.SplitterWidth = 0;
		ctnDropInput.TabIndex = 0;

		pnlInput.Controls.Add(ckbFreeInput);
		pnlInput.Controls.Add(ckbMultiSelect);
		pnlInput.Dock = PanelDockStyle.Top;
		pnlInput.Height = 40;
		pnlInput.KeepRelativeSize = false;
		pnlInput.Location = new Point(0, 461);
		pnlInput.Name = "pnlInput";
		pnlInput.Resizable = false;
		pnlInput.Size = new Size(792, 40);
		pnlInput.SizeRatio = 11.396;
		pnlInput.TabIndex = 1;

		ckbFreeInput.BackColor = SystemColors.Control;
		ckbFreeInput.BorderStyle = BorderStyle.None;
		ckbFreeInput.ForeColor = SystemColors.ControlText;
		ckbFreeInput.Location = new Point(105, 7);
		ckbFreeInput.Name = "ckbFreeInput";
		ckbFreeInput.Size = new Size(104, 24);
		ckbFreeInput.TabIndex = 3;
		ckbFreeInput.Text = "允许自由输入";
		ckbFreeInput.UseVisualStyleBackColor = true;
		ckbFreeInput.Value = null;

		ckbMultiSelect.BackColor = SystemColors.Control;
		ckbMultiSelect.BorderStyle = BorderStyle.None;
		ckbMultiSelect.ForeColor = SystemColors.ControlText;
		ckbMultiSelect.Location = new Point(12, 7);
		ckbMultiSelect.Name = "ckbMultiSelect";
		ckbMultiSelect.Size = new Size(104, 24);
		ckbMultiSelect.TabIndex = 2;
		ckbMultiSelect.Text = "允许复选";
		ckbMultiSelect.UseVisualStyleBackColor = true;
		ckbMultiSelect.Value = null;

		pnlFunctions.Controls.Add(tbrFunctions);
		pnlFunctions.Height = 24;
		pnlFunctions.KeepRelativeSize = false;
		pnlFunctions.Location = new Point(0, 0);
		pnlFunctions.MinHeight = 24;
		pnlFunctions.Name = "pnlFunctions";
		pnlFunctions.Resizable = false;
		pnlFunctions.Size = new Size(792, 24);
		pnlFunctions.SizeRatio = 7.767;
		pnlFunctions.TabIndex = 3;

		tbrFunctions.AutoSize = false;
		tbrFunctions.CommandHolder = c1CommandHolder1;
		tbrFunctions.Dock = DockStyle.Fill;
		tbrFunctions.Location = new Point(0, 0);
		tbrFunctions.Movable = false;
		tbrFunctions.Name = "tbrFunctions";
		tbrFunctions.Size = new Size(792, 24);
		tbrFunctions.Text = "c1ToolBar1";
		tbrFunctions.VisualStyle = (C1.Win.C1Command.VisualStyle)0;
		tbrFunctions.VisualStyleBase = (C1.Win.C1Command.VisualStyle)1;

		c1CommandHolder1.Commands.Add(c1ContextMenu1);
		c1CommandHolder1.Commands.Add(txbDropInputContextMenu);
		c1CommandHolder1.Owner = this;

		c1ContextMenu1.CommandLinks.AddRange(new C1CommandLink[] { c1CommandLink1 });
		c1ContextMenu1.Name = "c1ContextMenu1";
		c1ContextMenu1.ShortcutText = "";
		c1CommandLink1.Text = "新命令";
		txbDropInputContextMenu.Name = "txbDropInputContextMenu";
		txbDropInputContextMenu.ShortcutText = "";

		pnlFunctionHint.Controls.Add(lblFunctionHint);
		pnlFunctionHint.Dock = PanelDockStyle.Top;
		pnlFunctionHint.Height = 80;
		pnlFunctionHint.KeepRelativeSize = false;
		pnlFunctionHint.Location = new Point(0, 380);
		pnlFunctionHint.Name = "pnlFunctionHint";
		pnlFunctionHint.Resizable = false;
		pnlFunctionHint.Size = new Size(792, 80);
		pnlFunctionHint.SizeRatio = 16.194;
		pnlFunctionHint.TabIndex = 4;

		lblFunctionHint.BorderStyle = BorderStyle.None;
		lblFunctionHint.Dock = DockStyle.Fill;
		lblFunctionHint.Location = new Point(0, 0);
		lblFunctionHint.Name = "lblFunctionHint";
		lblFunctionHint.Size = new Size(792, 80);
		lblFunctionHint.TabIndex = 0;
		lblFunctionHint.Tag = null;
		lblFunctionHint.TextAlign = ContentAlignment.MiddleLeft;

		pnlCombo.Controls.Add(rtbDropInput);
		pnlCombo.Height = 354;
		pnlCombo.Location = new Point(0, 25);
		pnlCombo.Name = "pnlCombo";
		pnlCombo.Size = new Size(792, 354);
		pnlCombo.TabIndex = 2;

		tabEdit.Controls.Add(ctnCommentInput);
		tabEdit.Location = new Point(0, 27);
		tabEdit.Name = "tabEdit";
		tabEdit.Size = new Size(792, 501);
		tabEdit.TabIndex = 2;
		tabEdit.Text = "编辑注释";

		ctnCommentInput.AutoSizeElement = AutoSizeElement.Both;
		ctnCommentInput.BackColor = Color.FromArgb(240, 240, 240);
		ctnCommentInput.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnCommentInput.Dock = DockStyle.Fill;
		ctnCommentInput.ForeColor = Color.FromArgb(0, 0, 0);
		ctnCommentInput.Location = new Point(0, 0);
		ctnCommentInput.Name = "ctnCommentInput";
		ctnCommentInput.Panels.Add(c1SplitterPanel4);
		ctnCommentInput.Size = new Size(792, 501);
		ctnCommentInput.SplitterWidth = 0;
		ctnCommentInput.TabIndex = 1;

		c1SplitterPanel4.Controls.Add(txbCommentInput);
		c1SplitterPanel4.Height = 501;
		c1SplitterPanel4.Location = new Point(0, 0);
		c1SplitterPanel4.Name = "c1SplitterPanel4";
		c1SplitterPanel4.Size = new Size(792, 501);
		c1SplitterPanel4.TabIndex = 1;

		txbCommentInput.Dock = DockStyle.Fill;
		txbCommentInput.Location = new Point(0, 0);
		txbCommentInput.Margin = new Padding(3, 4, 3, 4);
		txbCommentInput.Multiline = true;
		txbCommentInput.Name = "txbCommentInput";
		txbCommentInput.ScrollBars = ScrollBars.Vertical;
		txbCommentInput.Size = new Size(792, 501);
		txbCommentInput.TabIndex = 0;
		txbCommentInput.Tag = null;
		txbCommentInput.TextDetached = true;

		tabDefault.Controls.Add(ctnDefaultInput);
		tabDefault.Location = new Point(0, 27);
		tabDefault.Name = "tabDefault";
		tabDefault.Size = new Size(792, 501);
		tabDefault.TabIndex = 1;
		tabDefault.Text = "默认内容";

		ctnDefaultInput.AutoSizeElement = AutoSizeElement.Both;
		ctnDefaultInput.BackColor = Color.FromArgb(240, 240, 240);
		ctnDefaultInput.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		ctnDefaultInput.Dock = DockStyle.Fill;
		ctnDefaultInput.ForeColor = Color.FromArgb(0, 0, 0);
		ctnDefaultInput.Location = new Point(0, 0);
		ctnDefaultInput.Name = "ctnDefaultInput";
		ctnDefaultInput.Panels.Add(c1SplitterPanel2);
		ctnDefaultInput.Size = new Size(792, 501);
		ctnDefaultInput.SplitterWidth = 0;
		ctnDefaultInput.TabIndex = 1;

		c1SplitterPanel2.Controls.Add(txbDefaultInput);
		c1SplitterPanel2.Height = 501;
		c1SplitterPanel2.Location = new Point(0, 0);
		c1SplitterPanel2.Name = "c1SplitterPanel2";
		c1SplitterPanel2.Size = new Size(792, 501);
		c1SplitterPanel2.TabIndex = 1;

		txbDefaultInput.Dock = DockStyle.Fill;
		txbDefaultInput.Location = new Point(0, 0);
		txbDefaultInput.Margin = new Padding(3, 4, 3, 4);
		txbDefaultInput.Multiline = true;
		txbDefaultInput.Name = "txbDefaultInput";
		txbDefaultInput.ScrollBars = ScrollBars.Vertical;
		txbDefaultInput.Size = new Size(792, 501);
		txbDefaultInput.TabIndex = 0;
		txbDefaultInput.Tag = null;
		txbDefaultInput.TextDetached = true;

		AutoScaleDimensions = new SizeF(7F, 17F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(792, 569);
		Controls.Add(ctnDock);
		Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
		Margin = new Padding(3, 4, 3, 4);
		Name = "frmAuxEdit";
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterScreen;
		Text = "辅助编辑";

		((ISupportInitialize)ctnDock).EndInit();
		ctnDock.ResumeLayout(false);
		pnlButtons.ResumeLayout(false);
		((ISupportInitialize)btnCancle).EndInit();
		((ISupportInitialize)btnConfirm).EndInit();
		pnlInputBox.ResumeLayout(false);
		((ISupportInitialize)DockingTab).EndInit();
		DockingTab.ResumeLayout(false);
		tabDropList.ResumeLayout(false);
		((ISupportInitialize)ctnDropInput).EndInit();
		ctnDropInput.ResumeLayout(false);
		pnlInput.ResumeLayout(false);
		((ISupportInitialize)ckbFreeInput).EndInit();
		((ISupportInitialize)ckbMultiSelect).EndInit();
		pnlFunctions.ResumeLayout(false);
		((ISupportInitialize)c1CommandHolder1).EndInit();
		pnlFunctionHint.ResumeLayout(false);
		((ISupportInitialize)lblFunctionHint).EndInit();
		pnlCombo.ResumeLayout(false);
		tabEdit.ResumeLayout(false);
		((ISupportInitialize)ctnCommentInput).EndInit();
		ctnCommentInput.ResumeLayout(false);
		c1SplitterPanel4.ResumeLayout(false);
		((ISupportInitialize)txbCommentInput).EndInit();
		tabDefault.ResumeLayout(false);
		((ISupportInitialize)ctnDefaultInput).EndInit();
		ctnDefaultInput.ResumeLayout(false);
		c1SplitterPanel2.ResumeLayout(false);
		((ISupportInitialize)txbDefaultInput).EndInit();
		ResumeLayout(false);
	}

	private void PopulateOtherFunctions()
	{
		List<FunctionInfo> publicFunctionInfos = FunctionInfo.PublicFunctionInfos;
		foreach (IGrouping<string, FunctionInfo> grouping in publicFunctionInfos.GroupBy(f => f.Category))
		{
			C1CommandMenu menu = new C1CommandMenu { Text = grouping.Key };
			C1CommandLink link = new C1CommandLink(menu);
			ctxOtherFunc.CommandLinks.Add(link);
			foreach (FunctionInfo f in grouping)
			{
				if (!Program.MainForm.IsAllowShowFunctionInfoAtCurrentView(f, false, false)) continue;
				C1Command cmd = new C1Command { Text = f.Name };
				C1CommandLink lnk = new C1CommandLink(cmd);
				FunctionInfo captured = f;
				cmd.Click += (s, e) =>
				{
					rtbDropInput.SelectedText = captured.Name + "()";
					rtbDropInput.SelectionStart = rtbDropInput.SelectionStart + rtbDropInput.SelectionLength - 1;
				};
				menu.CommandLinks.Add(lnk);
			}
		}
	}

	private void ResetQueryStatus()
	{
		QueryValueChanged = false;
		QueryDefaultValueChanged = false;
		QueryCommentValueChanged = false;
	}

	private void ShowFunctionHint(string text, int pos)
	{
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(text);
			Tuple<string, int> tup = formulaDisplay.GetFuncNameAtPos(pos);
			if (tup.Item1 == null)
			{
				lblFunctionHint.Value = "";
				return;
			}
			FunctionInfo fi = FunctionInfo.AllFunctionInfos.FirstOrDefault(f => string.Equals(f.Name, tup.Item1, StringComparison.OrdinalIgnoreCase));
			if (fi == null || !Program.MainForm.IsAllowShowFunctionInfoAtCurrentView(fi, false, false))
			{
				lblFunctionHint.Value = "";
				return;
			}
			string hint = string.Concat("函数语法：", fi.Name, "(", string.Join(", ", fi.Parameters.Select(p => p.Name)), ")\n");
			hint = string.Concat(hint, "函数功能：", fi.Description, "\n");
			hint = string.Concat(hint, "参数说明：", string.Join("；", fi.Parameters.Select(p => p.Name + "：" + p.Description)));
			lblFunctionHint.Value = hint;
		}
		catch
		{
			lblFunctionHint.Value = "";
		}
	}

	private void txbCommentInput_TextChanged(object sender, EventArgs e)
	{
		QueryCommentValueChanged = true;
	}

	private void txbDefaultInput_TextChanged(object sender, EventArgs e)
	{
		QueryDefaultValueChanged = true;
	}

	private void TxbDropInput_KeyDown(object sender, KeyEventArgs e)
	{
		if (!e.Control || e.KeyCode != Keys.V) return;
		e.Handled = true;
		rtbDropInput.SelectedText = Clipboard.GetText();
	}

	private void TxbDropInput_SelectionChanged(object sender, EventArgs e)
	{
		if (_isTextChanging) return;
		ShowFunctionHint(rtbDropInput.Text, rtbDropInput.SelectionStart);
	}

	private void txbDropInput_TextChanged(object sender, EventArgs e)
	{
		_isTextChanging = true;
		rtbDropInput.SuspendDrawing();
		QueryValueChanged = true;
		int selStart = rtbDropInput.SelectionStart;
		int selLen = rtbDropInput.SelectionLength;
		int vScroll = rtbDropInput.VScrollPos;
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(rtbDropInput.Text);
			rtbDropInput.SelectAll();
			rtbDropInput.SelectionBackColor = Color.Transparent;
			rtbDropInput.SelectionColor = Color.Black;
			foreach (Tuple<int, int, Color> interval in formulaDisplay.GetTokenColorIntervals())
			{
				rtbDropInput.Select(interval.Item1, interval.Item2);
				rtbDropInput.SelectionColor = interval.Item3;
			}
			Tuple<List<FormulaDisplayRef>, Color> refs = formulaDisplay.GetReferences(Program.MainForm.FormulaEditor.Context);
			Program.MainForm.FormulaEditor.RefIntervals = refs.Item1;
			Program.MainForm.FormulaEditor.NextColor = refs.Item2;
			foreach (FormulaDisplayRef r in Program.MainForm.FormulaEditor.RefIntervals)
			{
				foreach (Tuple<int, int> interval in r.Intervals)
				{
					rtbDropInput.Select(interval.Item1, interval.Item2);
					rtbDropInput.SelectionColor = r.Color;
				}
			}
		}
		catch
		{
		}
		finally
		{
			_isTextChanging = false;
			rtbDropInput.Select(selStart, selLen);
			rtbDropInput.VScrollPos = vScroll;
			rtbDropInput.ResumeDrawing();
		}
	}

	private bool UseWildcardImpl(string text, int pos)
	{
		if (text == null) return false;
		char c = text[pos - 1];
		if (c == '=' || c == '>' || c == '<') return true;
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(text);
			Tuple<string, int> tup = formulaDisplay.GetFuncNameAtPos(pos);
			if (string.Equals("Select", tup.Item1, StringComparison.OrdinalIgnoreCase) && tup.Item2 % 2 == 0) return true;
			if (string.Equals("If", tup.Item1, StringComparison.OrdinalIgnoreCase) && tup.Item2 == 0) return true;
		}
		catch
		{
			return false;
		}
		return false;
	}

	private bool ValidateFormula(out string formula)
	{
		formula = string.Empty;
		if (string.IsNullOrWhiteSpace(rtbDropInput.Text)) return true;
		try
		{
			FormulaDisplay formulaDisplay = new FormulaDisplay(rtbDropInput.Text);
			formula = formulaDisplay.ToFormula(Program.MainForm.FormulaEditor.Context);
			return true;
		}
		catch (FormulaParameterCountException)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, "函数参数数量错误", (MessageBoxButtons)0, "", false);
			return false;
		}
		catch (FormulaFunctionNotExistException)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, "函数不存在", (MessageBoxButtons)0, "", false);
			return false;
		}
		catch (FormulaSyntaxException ex)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, "语法错误。\n" + ex.Message, (MessageBoxButtons)0, "", false);
			rtbDropInput.Select(ex.CharPosition, 0);
			rtbDropInput.Focus();
			return false;
		}
		catch (FormulaBadReferenceException)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, "引用不存在", (MessageBoxButtons)0, "", false);
			return false;
		}
		catch (FormulaNotApplicableException ex)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, ex.Message, (MessageBoxButtons)0, "", false);
			return false;
		}
		catch (FormulaBadValueException ex)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, "值范围错误\n" + ex.Message, (MessageBoxButtons)0, "", false);
			return false;
		}
		catch (FormulaTypeMismatchException)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, "参数类型错误", (MessageBoxButtons)0, "", false);
			return false;
		}
		catch (FormulaColumnWildcardNoRowException)
		{
			Auditai.UI.Controls.MessageBox.Show((MessageBoxIcon)0, "公式引用的列没有本表对应的行", (MessageBoxButtons)0, "", false);
			return false;
		}
	}
}
