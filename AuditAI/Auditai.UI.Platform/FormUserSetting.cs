using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1InputPanel;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class FormUserSetting
{
	private const string DefaultFontFamily = "微软雅黑";

	private readonly int DefaultHeight = 26;

	private readonly Dictionary<object, string> TotalDisplay = new Dictionary<object, string>
	{
		[TotalDisplayFlags.AllSum] = "显示合计累计",
		[TotalDisplayFlags.MonthOnly] = "仅显示合计",
		[TotalDisplayFlags.YearOnly] = "仅显示累计"
	};

	private readonly Dictionary<object, string> SubDisplay = new Dictionary<object, string>
	{
		[SubDisplayFlags.AllSumAndData] = "显示合计累计",
		[SubDisplayFlags.MonthAndData] = "仅显示合计",
		[SubDisplayFlags.YearAndData] = "仅显示累计",
		[SubDisplayFlags.DataOnly] = "不显示合计累计"
	};

	private readonly Dictionary<object, string> BalanceTo = new Dictionary<object, string>
	{
		[SubOrTotal.Total] = "总账",
		[SubOrTotal.Subsidiary] = "明细账"
	};

	private readonly Dictionary<object, string> AlignDic = new Dictionary<object, string>
	{
		[SignAlign.Left] = "居左位置",
		[SignAlign.Center] = "居中位置",
		[SignAlign.Right] = "居右位置"
	};

	private readonly Dictionary<object, string> LocationDic = new Dictionary<object, string>
	{
		["1"] = "副标题第1行",
		["2"] = "副标题第2行",
		["3"] = "副标题第3行"
	};

	private readonly Dictionary<object, string> ParaSpace = new Dictionary<object, string>
	{
		["1"] = "1倍行距",
		["1.5"] = "1.5倍行距",
		["2"] = "2倍行距"
	};

	private readonly C1RibbonForm form;

	private readonly C1SplitterPanel pnlBottomBtn = new C1SplitterPanel();

	private readonly C1SplitterPanel pnlDockingTab = new C1SplitterPanel();

	private readonly C1SplitContainer ctnSetting = new C1SplitContainer();

	private readonly C1Button btnConfirm;

	private readonly C1Button btnCancel;

	private readonly C1DockingTab DockingTab;

	private readonly C1DockingTabPage BasicSettingPage;

	private readonly C1DockingTabPage MenuSettingPage;

	private readonly C1DockingTabPage TableSettingPage;

	private readonly C1DockingTabPage LedgerSettingPage;

	private readonly C1DockingTabPage DocSettingPage;

	private readonly C1DockingTabPage SignSettingPage;

	private readonly C1DockingTabPage CollectSettingPage;

	private C1InputPanel TableSettingPanel;

	private readonly Lazy<C1DockingTabPage> lzBasicTabPage;

	private readonly Lazy<C1DockingTabPage> lzMenuTabPage;

	private readonly Lazy<C1DockingTabPage> lzTableTabPage;

	private readonly Lazy<C1DockingTabPage> lzLedgerTabPage;

	private readonly Lazy<C1DockingTabPage> lzDocTabPage;

	private readonly Lazy<C1DockingTabPage> lzSignatureTabPage;

	private readonly Lazy<C1DockingTabPage> lzCollectTabPage;

	private InputRadioButton radioTreeStyle;

	private InputRadioButton radioChartStyle;

	private InputCheckBox tblTitleSameName;

	private InputCheckBox tblAppendRowAutomatic;

	private InputCheckBox tblSubtotalAutomatic;

	private InputNumericBox tblFormulaRefreshRows;

	private InputCheckBox tblSelectionStats;

	private InputCheckBox tblSpellCheck;

	private InputCheckBox tblHideFootRow;

	private InputCheckBox muOptionTabHide;

	private InputCheckBox muDisplayGuide;

	private InputNumericBox muRecentProjects;

	private InputFontControl lgFontFamily;

	private InputNumericBox lgRowHeight;

	private InputNumericBox lgFontSize;

	private InputColorControl lgFontColor;

	private InputComboBox lgTotalDispaly;

	private InputComboBox lgSubDisplay;

	private InputComboBox lgBalanceTo;

	private InputCheckBox lgEnableLedger;

	private InputLabel markLable;

	private InputFontControl tblTitleFamilyName;

	private InputNumericBox tblTitleFontSize;

	private InputColorControl tblTitleForeColor;

	private InputNumericBox tblTitleHeight;

	private InputFontControl tblSubTitleFamilyName;

	private InputNumericBox tblSubTitleFontSize;

	private InputColorControl tblSubTitleForeColor;

	private InputNumericBox tblSubTitleHeight;

	private InputNumericBox tblSubTitleRows;

	private InputFontControl tblFontFamily;

	private InputNumericBox tblFontSize;

	private InputColorControl tblForeColor;

	private InputNumericBox tblDefaultRows;

	private InputNumericBox tblDefaultCols;

	private InputNumericBox tblDefaultHight;

	private InputColorControl tblFormalaAreaBackColor;

	private InputColorControl tblLockAreaBackColor;

	private InputColorControl tblValidatePassColor;

	private InputColorControl tblValidateFailColor;

	private InputColorControl tblTotalRowBackColor;

	private InputColorControl tblJianxiangBackColor;

	private InputColorControl tblQizhongBackColor;

	private InputColorControl tblFixedBackColor;

	private InputFontControl docFontFamily;

	private InputNumericBox docFontSize;

	private InputColorControl docForeColor;

	private InputComboBox docInterSegmentSpacing;

	private InputNumericBox docPreSegmentSpacing;

	private InputNumericBox docPostSegmentSpacing;

	private InputNumericBox docFirstLineIndent;

	private InputTextBox sgCompilateFormat;

	private InputComboBox sgCompilateRow;

	private InputComboBox sgCompilateAlign;

	private InputTextBox sgCheckFormat;

	private InputComboBox sgCheckRow;

	private InputComboBox sgCheckAlign;

	public InputNumericBox clExtractScale;

	public InputNumericBox clExtractMax;

	public InputNumericBox clExtractMin;

	private bool _isLoadStatus = true;

	private int lastRowsNum;

	public UserConfig UserConfig => UserSet.Config;

	public FormUserSetting()
	{
		form = FormFactory.Create();
		form.MinimizeBox = false;
		form.MaximizeBox = false;
		form.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Settings);
		form.Size = new Size(750, 630);
		form.AcceptButton = btnConfirm;
		form.Text = "系统设置";
		form.ShowIcon = true;
		form.ShowInTaskbar = false;
		form.FormBorderStyle = FormBorderStyle.Sizable;
		lzBasicTabPage = new Lazy<C1DockingTabPage>(InitBasicSetting);
		lzMenuTabPage = new Lazy<C1DockingTabPage>(InitMenuSetting);
		lzTableTabPage = new Lazy<C1DockingTabPage>(InitTableSetting);
		lzLedgerTabPage = new Lazy<C1DockingTabPage>(InitBookSetting);
		lzDocTabPage = new Lazy<C1DockingTabPage>(InitDocSetting);
		lzSignatureTabPage = new Lazy<C1DockingTabPage>(InitSignatureSetting);
		lzCollectTabPage = new Lazy<C1DockingTabPage>(InitCollectSetting);
		DockingTab = new C1DockingTab
		{
			TextDirection = TabTextDirectionEnum.Horizontal,
			BorderStyle = BorderStyle.FixedSingle,
			Alignment = TabAlignment.Left,
			Dock = DockStyle.Fill,
			ShowToolTips = true,
			Size = new Size(800, 350),
			TabsSpacing = 10,
			ShowTabList = false,
			Font = new Font("微软雅黑", 10f),
			TabsShowFocusCues = false,
			Indent = 0
		};
		try
		{
			BasicSettingPage = new C1DockingTabPage();
			BasicSettingPage.Text = "基本设置";
			BasicSettingPage.Font = new Font("微软雅黑", 9f);
		}
		catch (NullReferenceException)
		{
		}
		try
		{
			MenuSettingPage = new C1DockingTabPage();
			MenuSettingPage.Text = "菜单设置";
			MenuSettingPage.Font = new Font("微软雅黑", 9f);
		}
		catch (NullReferenceException)
		{
		}
		try
		{
			TableSettingPage = new C1DockingTabPage();
			TableSettingPage.Text = "表格样式";
			TableSettingPage.Font = new Font("微软雅黑", 9f);
		}
		catch (NullReferenceException)
		{
		}
		try
		{
			LedgerSettingPage = new C1DockingTabPage();
			LedgerSettingPage.Text = "账套设置";
			LedgerSettingPage.Font = new Font("微软雅黑", 9f);
		}
		catch (NullReferenceException)
		{
		}
		try
		{
			DocSettingPage = new C1DockingTabPage();
			DocSettingPage.Text = "文档设置";
			DocSettingPage.Font = new Font("微软雅黑", 9f);
		}
		catch (NullReferenceException)
		{
		}
		try
		{
			SignSettingPage = new C1DockingTabPage();
			SignSettingPage.Text = "签名设置";
			SignSettingPage.Font = new Font("微软雅黑", 9f);
		}
		catch (NullReferenceException)
		{
		}
		try
		{
			CollectSettingPage = new C1DockingTabPage();
			CollectSettingPage.Text = "智能填充";
			CollectSettingPage.Font = new Font("微软雅黑", 9f);
		}
		catch (NullReferenceException)
		{
		}
		DockingTab.SelectedTabChanged += C1DockingTab1_SelectedTabChanged;
		DockingTab.Controls.Add(lzBasicTabPage.Value);
		DockingTab.Controls.Add(LedgerSettingPage);
		DockingTab.Controls.Add(TableSettingPage);
		DockingTab.Controls.Add(DocSettingPage);
		btnConfirm = new C1Button
		{
			Location = new Point(550, 8),
			Font = new Font("微软雅黑", 9f),
			Size = new Size(70, 26),
			Text = "确定"
		};
		btnCancel = new C1Button
		{
			Location = new Point(650, 8),
			Font = new Font("微软雅黑", 9f),
			Size = new Size(70, 26),
			Text = "取消"
		};
		btnConfirm.Click += btnConfirm_Click;
		btnCancel.Click += btnCancel_Click;
		ctnSetting.Dock = DockStyle.Fill;
		ctnSetting.SplitterWidth = 0;
		pnlDockingTab.SizeRatio = 95.0;
		pnlDockingTab.Resizable = false;
		DockingTab.Dock = DockStyle.Fill;
		pnlDockingTab.Controls.Add(DockingTab);
		ctnSetting.Panels.Add(pnlDockingTab);
		pnlBottomBtn.KeepRelativeSize = false;
		pnlBottomBtn.Resizable = false;
		pnlBottomBtn.SizeRatio = 100.0;
		pnlBottomBtn.Controls.Add(new C1InputPanel
		{
			Dock = DockStyle.Fill
		});
		pnlBottomBtn.Controls.Add(btnCancel);
		pnlBottomBtn.Controls.Add(btnConfirm);
		ctnSetting.Panels.Add(pnlBottomBtn);
		btnConfirm.BringToFront();
		btnCancel.BringToFront();
		form.Controls.Add(ctnSetting);
		ctnSetting.SplitterWidth = 0;
		ctnSetting.SplitterColor = Color.Transparent;
		radioTreeStyle.Checked = UserConfig.NavigateViewType == NavigateViewType.Tree;
		radioChartStyle.Checked = UserConfig.NavigateViewType == NavigateViewType.Chart;
		tblTitleSameName.Checked = UserConfig.IsTitleFitTableName;
		tblAppendRowAutomatic.Checked = UserConfig.AutoRowAdd;
		tblSubtotalAutomatic.Checked = UserConfig.AutoAreaMerge;
		tblFormulaRefreshRows.Value = UserConfig.RowsApplyFormulaAuto;
		tblSelectionStats.Checked = UserConfig.SelectionStatsEnabled;
		tblSpellCheck.Checked = UserConfig.AutoSpellCheck;
	}

	public DialogResult ShowDialog()
	{
		Auditai.UI.Controls.Theme.SetCurrentTree(form);
		ctnSetting.SplitterWidth = 0;
		ctnSetting.SplitterColor = Color.Transparent;
		ctnSetting.BorderWidth = 0;
		pnlDockingTab.BorderWidth = 0;
		pnlBottomBtn.BorderWidth = 0;
		DockingTab.BorderStyle = BorderStyle.FixedSingle;
		DockingTab.TabsSpacing = 10;
		DockingTab.Indent = 0;
		LedgerSettingPage.TabVisible = SoftwareLicenseManager.IsLedgerModuleEnable();
		return form.ShowDialog();
	}

	private void btnConfirm_Click(object sender, EventArgs e)
	{
		if (lzBasicTabPage.IsValueCreated)
		{
			if (radioTreeStyle.Checked)
			{
				UserConfig.NavigateViewType = NavigateViewType.Tree;
			}
			else if (radioChartStyle.Checked)
			{
				UserConfig.NavigateViewType = NavigateViewType.Chart;
			}
			else
			{
				UserConfig.NavigateViewType = NavigateViewType.Tree;
			}
			UserConfig.IsTitleFitTableName = tblTitleSameName.Checked;
			UserConfig.AutoRowAdd = tblAppendRowAutomatic.Checked;
			UserConfig.AutoAreaMerge = tblSubtotalAutomatic.Checked;
			UserConfig.RowsApplyFormulaAuto = (int)tblFormulaRefreshRows.Value;
			UserConfig.SelectionStatsEnabled = tblSelectionStats.Checked;
			UserConfig.AutoSpellCheck = tblSpellCheck.Checked;
			UserConfig.HideFootRow = tblHideFootRow.Checked;
		}
		if (lzMenuTabPage.IsValueCreated)
		{
			UserConfig.HideTab = muOptionTabHide.Checked;
			UserConfig.GuidDisplay = muDisplayGuide.Checked;
			UserConfig.RecentProjectCount = (int)muRecentProjects.Value;
		}
		if (lzLedgerTabPage.IsValueCreated)
		{
			UserConfig.BooksStyle.FontStyle.FontFamily = lgFontFamily.Text;
			UserConfig.BooksStyle.FontStyle.FontSize = (float)lgFontSize.Value;
			UserConfig.BooksStyle.FontStyle.FontColor = Color.Black;
			UserConfig.BooksStyle.BooksRowHeight = (int)lgRowHeight.Value;
			UserConfig.BooksStyle.TotalDisplay = (TotalFlag)lgTotalDispaly.SelectedOption.Tag;
			UserConfig.BooksStyle.SubDisplay = (TotalFlag)lgSubDisplay.SelectedOption.Tag;
			UserConfig.BooksStyle.BalanceTo = (SubOrTotal)lgBalanceTo.SelectedOption.Tag;
		}
		if (lzTableTabPage.IsValueCreated)
		{
			UserConfig.TableStyle.TitleStyle.FontFamily = tblTitleFamilyName.Text;
			UserConfig.TableStyle.TitleStyle.FontSize = (float)tblTitleFontSize.Value;
			UserConfig.TableStyle.TitleStyle.FontColor = (tblTitleForeColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.MainTitleHeight = (int)tblTitleHeight.Value;
			UserConfig.TableStyle.SubTitleStyle.FontFamily = tblSubTitleFamilyName.Text;
			UserConfig.TableStyle.SubTitleStyle.FontSize = (float)tblSubTitleFontSize.Value;
			UserConfig.TableStyle.SubTitleStyle.FontColor = (tblSubTitleForeColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.SubTitleHeight = (int)tblSubTitleHeight.Value;
			UserConfig.TableStyle.SubTitleRows = (int)tblSubTitleRows.Value;
			UserConfig.TableStyle.SubTitleContent.Clear();
			int num = TableSettingPanel.Items.IndexOf(markLable);
			for (int i = 0; (decimal)i < tblSubTitleRows.Value; i++)
			{
				Tuple<string, string, string> item = Tuple.Create(TableSettingPanel.Items[num += 2].Text.Trim(), TableSettingPanel.Items[num += 2].Text.Trim(), TableSettingPanel.Items[num += 2].Text.Trim());
				UserConfig.TableStyle.SubTitleContent.Add(item);
			}
			UserConfig.TableStyle.FontStyle.FontFamily = tblFontFamily.Text;
			UserConfig.TableStyle.FontStyle.FontSize = (float)tblFontSize.Value;
			UserConfig.TableStyle.FontStyle.FontColor = (tblForeColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.TableRows = (int)tblDefaultRows.Value;
			UserConfig.TableStyle.TableCols = (int)tblDefaultCols.Value;
			UserConfig.TableStyle.TableRowHeight = (int)tblDefaultHight.Value;
			UserConfig.TableStyle.FormalaColor = (tblFormalaAreaBackColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.LockAreaColor = (tblLockAreaBackColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.CheckPassColor = (tblValidatePassColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.CheckFailColor = (tblValidateFailColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.RowTotalColor = (tblTotalRowBackColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.RowMinusColor = (tblJianxiangBackColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.RowAmongColor = (tblQizhongBackColor.Control as C1ColorPicker).Color;
			UserConfig.TableStyle.RowFixedColor = (tblFixedBackColor.Control as C1ColorPicker).Color;
		}
		if (lzDocTabPage.IsValueCreated)
		{
			UserConfig.DocStyle.FontStyle.FontFamily = docFontFamily.Text;
			UserConfig.DocStyle.FontStyle.FontSize = (float)docFontSize.Value;
			UserConfig.DocStyle.FontStyle.FontColor = (docForeColor.Control as C1ColorPicker).Color;
			UserConfig.DocStyle.ParagraphSpace = docInterSegmentSpacing.SelectedOption.Tag.ToString();
			UserConfig.DocStyle.BeforeParagraph = (int)docPreSegmentSpacing.Value;
			UserConfig.DocStyle.AfterParagraph = (int)docPostSegmentSpacing.Value;
			UserConfig.DocStyle.FirstRowIndent = (int)docFirstLineIndent.Value;
		}
		if (lzSignatureTabPage.IsValueCreated)
		{
			UserConfig.SignatureStyle.SignatureFormat = sgCompilateFormat.Text;
			UserConfig.SignatureStyle.SignatureRow = sgCompilateRow.SelectedOption.Tag.ToString();
			UserConfig.SignatureStyle.SignatureAlign = (SignAlign)sgCompilateAlign.SelectedOption.Tag;
			UserConfig.SignatureStyle.ReviewSignFormat = sgCheckFormat.Text;
			UserConfig.SignatureStyle.ReviewSignRow = sgCheckRow.SelectedOption.Tag.ToString();
			UserConfig.SignatureStyle.ReviewSignAlign = (SignAlign)sgCheckAlign.SelectedOption.Tag;
		}
		if (lzCollectTabPage.IsValueCreated)
		{
			UserConfig.CollectSetting.DefaultExtract = (double)clExtractScale.Value;
			UserConfig.CollectSetting.MaxExtract = (int)clExtractMax.Value;
			UserConfig.CollectSetting.MinExtract = (int)clExtractMin.Value;
		}
		form.DialogResult = DialogResult.OK;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		form.DialogResult = DialogResult.Cancel;
	}

	private void InputSubRows_ValueChanged(object sender, EventArgs e)
	{
		if (_isLoadStatus)
		{
			return;
		}
		int num = TableSettingPanel.Items.IndexOf(markLable) + 1;
		int num2 = (int)tblSubTitleRows.Value;
		if (num2 < 0)
		{
			return;
		}
		if (num2 < lastRowsNum)
		{
			int index = num + 6 * num2;
			int num3 = 6 * (lastRowsNum - num2);
			for (int i = 0; i < num3; i++)
			{
				TableSettingPanel.Items.RemoveAt(index);
			}
		}
		else
		{
			int num4 = num + lastRowsNum * 6;
			for (int j = 0; j < num2 - lastRowsNum; j++)
			{
				InputTextBox inputTextBox = MakeInputBox(100, BreakType.Column);
				InputTextBox inputTextBox2 = MakeInputBox(100, BreakType.Column);
				InputTextBox inputTextBox3 = MakeInputBox(100, BreakType.Group);
				inputTextBox.MaxLength = 100;
				inputTextBox2.MaxLength = 100;
				inputTextBox3.MaxLength = 100;
				TableSettingPanel.Items.Insert(num4++, MakeInputLabel("副标题（左）", 90));
				TableSettingPanel.Items.Insert(num4++, inputTextBox);
				TableSettingPanel.Items.Insert(num4++, MakeInputLabel("副标题（中）", 90));
				TableSettingPanel.Items.Insert(num4++, inputTextBox2);
				TableSettingPanel.Items.Insert(num4++, MakeInputLabel("副标题（右）", 90));
				TableSettingPanel.Items.Insert(num4++, inputTextBox3);
			}
		}
		lastRowsNum = num2;
	}

	private void LgEnableLedger_Click(object sender, EventArgs e)
	{
	}

	private C1DockingTabPage InitBasicSetting()
	{
		C1InputPanel c1InputPanel = MakeInputPanel();
		radioTreeStyle = new InputRadioButton();
		radioTreeStyle.GroupName = "navigatestyle";
		radioTreeStyle.Text = "文件树导航样式";
		radioChartStyle = new InputRadioButton();
		radioChartStyle.GroupName = "navigatestyle";
		radioChartStyle.Text = "流程图导航样式";
		c1InputPanel.Items.Add(MakeGroupHeader("自由表格"));
		c1InputPanel.Items.Add(tblTitleSameName = MakeCheckBox("主标题与表名一致", BreakType.Row));
		c1InputPanel.Items.Add(tblAppendRowAutomatic = MakeCheckBox("末行回车自动增行", BreakType.Row));
		c1InputPanel.Items.Add(tblSubtotalAutomatic = MakeCheckBox("选区自动合计", BreakType.Row));
		c1InputPanel.Items.Add(tblHideFootRow = MakeCheckBox("隐藏表尾行", BreakType.Row));
		tblHideFootRow.Checked = UserConfig.HideFootRow;
		InputLabel inputLabel = null;
		c1InputPanel.Items.Add(inputLabel = MakeInputLabel("可自动刷新运算的表格总行数", 160));
		inputLabel.HorizontalAlign = InputContentAlignment.Near;
		c1InputPanel.Items.Add(tblFormulaRefreshRows = MakeInputNum(100, BreakType.Row));
		tblFormulaRefreshRows.Maximum = 10000m;
		c1InputPanel.Items.Add(MakeGroupHeader("自由文档"));
		c1InputPanel.Items.Add(tblSelectionStats = MakeCheckBox("选区统计", BreakType.Row));
		c1InputPanel.Items.Add(tblSpellCheck = MakeCheckBox("输入拼写检查", BreakType.Row));
		tblSpellCheck.Enabled = false;
		BasicSettingPage.Controls.Add(c1InputPanel);
		return BasicSettingPage;
	}

	private C1DockingTabPage InitMenuSetting()
	{
		C1InputPanel c1InputPanel = MakeInputPanel();
		c1InputPanel.Items.Add(MakeGroupHeader("菜单设置"));
		c1InputPanel.Items.Add(muOptionTabHide = MakeCheckBox("隐藏选项卡", 150, BreakType.Row));
		c1InputPanel.Items.Add(muDisplayGuide = MakeCheckBox("显示引导提示", 150, BreakType.Row));
		c1InputPanel.Items.Add(MakeInputLabel("最近打开" + StringConstBase.Current.Project + "数"));
		c1InputPanel.Items.Add(muRecentProjects = MakeInputNum(100, BreakType.Row));
		MenuSettingPage.Controls.Add(c1InputPanel);
		muOptionTabHide.Checked = UserConfig.HideTab;
		muDisplayGuide.Checked = UserConfig.GuidDisplay;
		muRecentProjects.Value = UserConfig.RecentProjectCount;
		return MenuSettingPage;
	}

	private C1DockingTabPage InitBookSetting()
	{
		C1InputPanel c1InputPanel = MakeInputPanel();
		int width = 80;
		int width2 = 100;
		c1InputPanel.Items.Add(MakeGroupHeader("账簿样式"));
		c1InputPanel.Items.Add(MakeInputLabel("默认字体", width));
		c1InputPanel.Items.Add(lgFontFamily = MakeInputFont(width2, BreakType.Column));
		c1InputPanel.Items.Add(MakeInputLabel("默认表格行高", width));
		c1InputPanel.Items.Add(lgRowHeight = MakeInputNum(width2, BreakType.Column));
		c1InputPanel.Items.Add(MakeInputLabel("默认字号", width));
		c1InputPanel.Items.Add(lgFontSize = MakeInputNum(width2, BreakType.Group));
		int width3 = 110;
		int width4 = 130;
		c1InputPanel.Items.Add(MakeGroupHeader("操作个性"));
		c1InputPanel.Items.Add(MakeInputLabel("总账默认显示选项", width3));
		c1InputPanel.Items.Add(lgTotalDispaly = MakeComboBox(1, TotalDisplay, width4));
		c1InputPanel.Items.Add(MakeInputLabel("明细账默认显示选项", width3));
		c1InputPanel.Items.Add(lgSubDisplay = MakeComboBox(3, SubDisplay, width4));
		c1InputPanel.Items.Add(MakeInputLabel("科目余额表跳转至", width3));
		c1InputPanel.Items.Add(lgBalanceTo = MakeComboBox(1, BalanceTo, width4));
		lgRowHeight.Minimum = 10m;
		lgRowHeight.Maximum = 2000m;
		lgFontSize.Minimum = 5m;
		lgFontSize.Maximum = 72m;
		LedgerSettingPage.Controls.Add(c1InputPanel);
		BooksStyle booksStyle = UserConfig.BooksStyle;
		lgFontFamily.Text = booksStyle.FontStyle.FontFamily;
		lgFontSize.Value = (decimal)booksStyle.FontStyle.FontSize;
		lgRowHeight.Value = booksStyle.BooksRowHeight;
		if (TotalDisplay.ContainsKey(booksStyle.TotalDisplay))
		{
			lgTotalDispaly.Text = TotalDisplay[booksStyle.TotalDisplay];
		}
		if (SubDisplay.ContainsKey(booksStyle.SubDisplay))
		{
			lgSubDisplay.Text = SubDisplay[booksStyle.SubDisplay];
		}
		if (BalanceTo.ContainsKey(booksStyle.BalanceTo))
		{
			lgBalanceTo.Text = BalanceTo[booksStyle.BalanceTo];
		}
		return LedgerSettingPage;
	}

	private C1DockingTabPage InitDocSetting()
	{
		C1InputPanel c1InputPanel = MakeInputPanel();
		int width = 80;
		int width2 = 100;
		c1InputPanel.Items.Add(MakeGroupHeader("文档样式"));
		c1InputPanel.Items.Add(MakeInputLabel("默认字体", width));
		c1InputPanel.Items.Add(docFontFamily = MakeInputFont(width2, BreakType.Column));
		c1InputPanel.Items.Add(MakeInputLabel("默认字体颜色", width));
		c1InputPanel.Items.Add(docForeColor = MakeInputColor(width2, BreakType.Column));
		c1InputPanel.Items.Add(MakeInputLabel("默认字号", width));
		c1InputPanel.Items.Add(docFontSize = MakeInputNum(width2, BreakType.Group));
		c1InputPanel.Items.Add(MakeInputLabel("默认段内行距", width));
		c1InputPanel.Items.Add(docInterSegmentSpacing = MakeComboBox(1, ParaSpace, 100));
		docInterSegmentSpacing.Break = BreakType.Column;
		c1InputPanel.Items.Add(MakeInputLabel("默认段前行距", width));
		c1InputPanel.Items.Add(docPreSegmentSpacing = MakeInputNum(width2, BreakType.Column));
		c1InputPanel.Items.Add(MakeInputLabel("默认段后行距", width));
		c1InputPanel.Items.Add(docPostSegmentSpacing = MakeInputNum(width2, BreakType.Group));
		c1InputPanel.Items.Add(MakeInputLabel("默认首行缩进", width));
		c1InputPanel.Items.Add(docFirstLineIndent = MakeInputNum(width2, BreakType.Group));
		docFontSize.Minimum = 5m;
		docFontSize.Maximum = 72m;
		docPreSegmentSpacing.Minimum = 0m;
		docPreSegmentSpacing.Maximum = 5000m;
		docPostSegmentSpacing.Minimum = 0m;
		docPostSegmentSpacing.Maximum = 5000m;
		docFirstLineIndent.Minimum = 0m;
		docFirstLineIndent.Maximum = 100m;
		DocSettingPage.Controls.Add(c1InputPanel);
		DocStyle docStyle = UserConfig.DocStyle;
		docFontFamily.Text = docStyle.FontStyle.FontFamily;
		docFontSize.Value = (decimal)docStyle.FontStyle.FontSize;
		(docForeColor.Control as C1ColorPicker).Color = docStyle.FontStyle.FontColor;
		if (ParaSpace.ContainsKey(docStyle.ParagraphSpace.ToString()))
		{
			docInterSegmentSpacing.Text = ParaSpace[docStyle.ParagraphSpace.ToString()];
		}
		docPreSegmentSpacing.Value = docStyle.BeforeParagraph;
		docPostSegmentSpacing.Value = docStyle.AfterParagraph;
		docFirstLineIndent.Value = docStyle.FirstRowIndent;
		return DocSettingPage;
	}

	private C1DockingTabPage InitSignatureSetting()
	{
		C1InputPanel c1InputPanel = MakeInputPanel();
		c1InputPanel.Items.Add(MakeGroupHeader("设置签名"));
		c1InputPanel.Items.Add(MakeInputLabel("默认编制签名样式", 100));
		c1InputPanel.Items.Add(sgCompilateFormat = MakeInputBox(200, BreakType.Column));
		c1InputPanel.Items.Add(MakeInputLabel("签名位置", 52));
		c1InputPanel.Items.Add(sgCompilateRow = MakeComboBox(0, LocationDic, 100));
		sgCompilateRow.Break = BreakType.Column;
		c1InputPanel.Items.Add(sgCompilateAlign = MakeComboBox(2, AlignDic, 100));
		sgCompilateAlign.Break = BreakType.Group;
		c1InputPanel.Items.Add(MakeInputLabel("默认复核签名样式", 100));
		c1InputPanel.Items.Add(sgCheckFormat = MakeInputBox(200, BreakType.Column));
		c1InputPanel.Items.Add(MakeInputLabel("签名位置", 52));
		c1InputPanel.Items.Add(sgCheckRow = MakeComboBox(1, LocationDic, 100));
		sgCheckRow.Break = BreakType.Column;
		c1InputPanel.Items.Add(sgCheckAlign = MakeComboBox(2, AlignDic, 100));
		sgCheckAlign.Break = BreakType.Group;
		sgCompilateFormat.Width = 270;
		sgCheckFormat.Width = 270;
		sgCompilateFormat.HorizontalAlign = InputContentAlignment.NotSet;
		sgCheckFormat.HorizontalAlign = InputContentAlignment.NotSet;
		SignSettingPage.Controls.Add(c1InputPanel);
		SignatureStyle signatureStyle = UserConfig.SignatureStyle;
		sgCompilateFormat.Text = signatureStyle.SignatureFormat;
		if (LocationDic.ContainsKey(signatureStyle.SignatureRow))
		{
			sgCompilateRow.Text = LocationDic[signatureStyle.SignatureRow];
		}
		if (AlignDic.ContainsKey(signatureStyle.SignatureAlign))
		{
			sgCompilateAlign.Text = AlignDic[signatureStyle.SignatureAlign];
		}
		sgCheckFormat.Text = signatureStyle.ReviewSignFormat;
		if (LocationDic.ContainsKey(signatureStyle.ReviewSignRow))
		{
			sgCheckRow.Text = LocationDic[signatureStyle.ReviewSignRow];
		}
		if (AlignDic.ContainsKey(signatureStyle.ReviewSignAlign))
		{
			sgCheckAlign.Text = AlignDic[signatureStyle.ReviewSignAlign];
		}
		return SignSettingPage;
	}

	private C1DockingTabPage InitCollectSetting()
	{
		C1InputPanel c1InputPanel = MakeInputPanel();
		c1InputPanel.Items.Add(MakeGroupHeader("批量采账填充"));
		c1InputPanel.Items.Add(MakeInputLabel("默认抽凭样本比例", 120));
		c1InputPanel.Items.Add(clExtractScale = MakeInputNum(200, BreakType.Row));
		c1InputPanel.Items.Add(MakeInputLabel("默认抽凭最小样本量", 120));
		c1InputPanel.Items.Add(clExtractMin = MakeInputNum(200, BreakType.Row));
		c1InputPanel.Items.Add(MakeInputLabel("默认抽凭最大样本量", 120));
		c1InputPanel.Items.Add(clExtractMax = MakeInputNum(200, BreakType.Row));
		CollectSettingPage.Controls.Add(c1InputPanel);
		clExtractScale.StandardFormat = InputNumberFormat.Percent;
		clExtractMax.Maximum = 100000m;
		clExtractMin.Maximum = 100000m;
		clExtractScale.Minimum = 0m;
		clExtractScale.Value = (decimal)UserConfig.CollectSetting.DefaultExtract;
		clExtractMax.Value = UserConfig.CollectSetting.MaxExtract;
		clExtractMin.Value = UserConfig.CollectSetting.MinExtract;
		return CollectSettingPage;
	}

	private C1DockingTabPage InitTableSetting()
	{
		TableSettingPanel = new C1InputPanel
		{
			AutoSizeElement = AutoSizeElement.Both,
			Font = new Font("微软雅黑", 9f),
			ChildSpacing = new Size(4, 10),
			Dock = DockStyle.Fill
		};
		int width = 90;
		int width2 = 100;
		TableSettingPanel.Items.Add(MakeGroupHeader("主标题样式"));
		TableSettingPanel.Items.Add(MakeInputLabel("默认主标题字体", width));
		TableSettingPanel.Items.Add(tblTitleFamilyName = MakeInputFont(width2, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认主标题字号", width));
		TableSettingPanel.Items.Add(tblTitleFontSize = MakeInputNum(width2, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认主标题颜色", width));
		TableSettingPanel.Items.Add(tblTitleForeColor = MakeInputColor(width2, BreakType.Group));
		TableSettingPanel.Items.Add(MakeInputLabel("默认主标题行高", width));
		TableSettingPanel.Items.Add(tblTitleHeight = MakeInputNum(width2, BreakType.Column));
		int width3 = 90;
		int width4 = 100;
		TableSettingPanel.Items.Add(MakeGroupHeader("副标题样式"));
		TableSettingPanel.Items.Add(MakeInputLabel("默认副标题字体", width3));
		TableSettingPanel.Items.Add(tblSubTitleFamilyName = MakeInputFont(width4, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认副标题字号", width3));
		TableSettingPanel.Items.Add(tblSubTitleFontSize = MakeInputNum(width4, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认副标题颜色", width3));
		TableSettingPanel.Items.Add(tblSubTitleForeColor = MakeInputColor(width4, BreakType.Group));
		TableSettingPanel.Items.Add(MakeInputLabel("默认副标题行高", width3));
		TableSettingPanel.Items.Add(tblSubTitleHeight = MakeInputNum(width4, BreakType.Row));
		TableSettingPanel.Items.Add(MakeInputLabel("默认副标题行数", width3));
		TableSettingPanel.Items.Add(tblSubTitleRows = MakeInputNum(width4, BreakType.Column));
		markLable = new InputLabel
		{
			Text = string.Empty,
			Break = BreakType.Group
		};
		TableSettingPanel.Items.Add(markLable);
		int width5 = 90;
		int width6 = 100;
		TableSettingPanel.Items.Add(MakeGroupHeader("表体样式"));
		TableSettingPanel.Items.Add(MakeInputLabel("默认表体字体", width5));
		TableSettingPanel.Items.Add(tblFontFamily = MakeInputFont(width6, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认表格行数", width5));
		TableSettingPanel.Items.Add(tblDefaultRows = MakeInputNum(width6, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认表体字号", width5));
		TableSettingPanel.Items.Add(tblFontSize = MakeInputNum(width6, BreakType.Group));
		TableSettingPanel.Items.Add(MakeInputLabel("默认表格列数", width5));
		TableSettingPanel.Items.Add(tblDefaultCols = MakeInputNum(width6, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认表体颜色", width5));
		TableSettingPanel.Items.Add(tblForeColor = MakeInputColor(width6, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("默认表格行高", width5));
		TableSettingPanel.Items.Add(tblDefaultHight = MakeInputNum(width6, BreakType.Group));
		int width7 = 90;
		int width8 = 100;
		TableSettingPanel.Items.Add(MakeGroupHeader("其他默认设置"));
		TableSettingPanel.Items.Add(MakeInputLabel("公式区域背景色", width7));
		TableSettingPanel.Items.Add(tblFormalaAreaBackColor = MakeInputColor(width8, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("锁定区域背景色", width7));
		TableSettingPanel.Items.Add(tblLockAreaBackColor = MakeInputColor(width8, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("校验正确背景色", width7));
		TableSettingPanel.Items.Add(tblValidatePassColor = MakeInputColor(width8, BreakType.Group));
		TableSettingPanel.Items.Add(MakeInputLabel("校验错误背景色", width7));
		TableSettingPanel.Items.Add(tblValidateFailColor = MakeInputColor(width8, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("合计类行背景色", width7));
		TableSettingPanel.Items.Add(tblTotalRowBackColor = MakeInputColor(width8, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("减项类行背景色", width7));
		TableSettingPanel.Items.Add(tblJianxiangBackColor = MakeInputColor(width8, BreakType.Group));
		TableSettingPanel.Items.Add(MakeInputLabel("其中类行背景色", width7));
		TableSettingPanel.Items.Add(tblQizhongBackColor = MakeInputColor(width8, BreakType.Column));
		TableSettingPanel.Items.Add(MakeInputLabel("固定类行背景色", width7));
		TableSettingPanel.Items.Add(tblFixedBackColor = MakeInputColor(width8, BreakType.Column));
		tblTitleHeight.Minimum = 10m;
		tblTitleHeight.Maximum = 2000m;
		tblSubTitleHeight.Minimum = 10m;
		tblSubTitleHeight.Maximum = 2000m;
		tblDefaultHight.Minimum = 10m;
		tblDefaultHight.Maximum = 2000m;
		tblTitleFontSize.Minimum = 5m;
		tblTitleFontSize.Maximum = 72m;
		tblSubTitleFontSize.Minimum = 5m;
		tblSubTitleFontSize.Maximum = 72m;
		tblFontSize.Minimum = 5m;
		tblFontSize.Maximum = 72m;
		tblSubTitleRows.Minimum = 0m;
		tblSubTitleRows.Maximum = 10m;
		TableSettingPage.Controls.Add(TableSettingPanel);
		tblSubTitleRows.ValueChanged += InputSubRows_ValueChanged;
		try
		{
			tblTitleFamilyName.Text = UserConfig.TableStyle.TitleStyle.FontFamily;
			tblTitleFontSize.Value = (decimal)UserConfig.TableStyle.TitleStyle.FontSize;
			(tblTitleForeColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.TitleStyle.FontColor;
			tblTitleHeight.Value = UserConfig.TableStyle.MainTitleHeight;
		}
		catch
		{
		}
		try
		{
			tblSubTitleFamilyName.Text = UserConfig.TableStyle.SubTitleStyle.FontFamily;
			tblSubTitleFontSize.Value = (decimal)UserConfig.TableStyle.SubTitleStyle.FontSize;
			(tblSubTitleForeColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.SubTitleStyle.FontColor;
			tblSubTitleHeight.Value = UserConfig.TableStyle.SubTitleHeight;
		}
		catch
		{
		}
		try
		{
			tblFontFamily.Text = UserConfig.TableStyle.FontStyle.FontFamily;
			tblFontSize.Value = (decimal)UserConfig.TableStyle.FontStyle.FontSize;
			(tblForeColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.FontStyle.FontColor;
		}
		catch
		{
		}
		try
		{
			tblDefaultRows.Value = UserConfig.TableStyle.TableRows;
			tblDefaultCols.Value = UserConfig.TableStyle.TableCols;
			tblDefaultHight.Value = UserConfig.TableStyle.TableRowHeight;
		}
		catch
		{
		}
		try
		{
			tblSubTitleRows.Value = UserConfig.TableStyle.SubTitleRows;
			lastRowsNum = UserConfig.TableStyle.SubTitleRows;
			int num = TableSettingPanel.Items.IndexOf(markLable);
			for (int i = 0; i < UserConfig.TableStyle.SubTitleRows; i++)
			{
				TableSettingPanel.Items.Insert(++num, MakeInputLabel("副标题（左）", 90));
				TableSettingPanel.Items.Insert(++num, MakeInputBox(100, BreakType.Column, UserConfig.TableStyle.SubTitleContent[i].Item1));
				TableSettingPanel.Items.Insert(++num, MakeInputLabel("副标题（中）", 90));
				TableSettingPanel.Items.Insert(++num, MakeInputBox(100, BreakType.Column, UserConfig.TableStyle.SubTitleContent[i].Item2));
				TableSettingPanel.Items.Insert(++num, MakeInputLabel("副标题（右）", 90));
				TableSettingPanel.Items.Insert(++num, MakeInputBox(100, BreakType.Group, UserConfig.TableStyle.SubTitleContent[i].Item3));
			}
		}
		catch
		{
		}
		try
		{
			(tblLockAreaBackColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.LockAreaColor;
			(tblFormalaAreaBackColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.FormalaColor;
			(tblValidatePassColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.CheckPassColor;
			(tblValidateFailColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.CheckFailColor;
			(tblTotalRowBackColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.RowTotalColor;
			(tblJianxiangBackColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.RowMinusColor;
			(tblQizhongBackColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.RowAmongColor;
			(tblFixedBackColor.Control as C1ColorPicker).Color = UserConfig.TableStyle.RowFixedColor;
		}
		catch
		{
		}
		_isLoadStatus = false;
		return null;
	}

	private void C1DockingTab1_SelectedTabChanged(object sender, EventArgs e)
	{
		bool flag = true;
		if (DockingTab.SelectedTab == MenuSettingPage)
		{
			flag = lzMenuTabPage.IsValueCreated;
			C1DockingTabPage value = lzMenuTabPage.Value;
		}
		else if (DockingTab.SelectedTab == LedgerSettingPage)
		{
			flag = lzLedgerTabPage.IsValueCreated;
			C1DockingTabPage value2 = lzLedgerTabPage.Value;
		}
		else if (DockingTab.SelectedTab == TableSettingPage)
		{
			flag = lzTableTabPage.IsValueCreated;
			C1DockingTabPage value3 = lzTableTabPage.Value;
		}
		else if (DockingTab.SelectedTab == DocSettingPage)
		{
			flag = lzDocTabPage.IsValueCreated;
			C1DockingTabPage value4 = lzDocTabPage.Value;
		}
		else if (DockingTab.SelectedTab == SignSettingPage)
		{
			flag = lzSignatureTabPage.IsValueCreated;
			C1DockingTabPage value5 = lzSignatureTabPage.Value;
		}
		else if (DockingTab.SelectedTab == CollectSettingPage)
		{
			flag = lzCollectTabPage.IsValueCreated;
			C1DockingTabPage value6 = lzCollectTabPage.Value;
		}
		if (!flag)
		{
			Auditai.UI.Controls.Theme.SetCurrentTree(form);
		}
		DockingTabSetting();
	}

	private void DockingTabSetting()
	{
		DockingTab.TabsSpacing = 10;
		DockingTab.BorderStyle = BorderStyle.FixedSingle;
		DockingTab.Indent = 0;
	}

	private C1InputPanel MakeInputPanel()
	{
		return new C1InputPanel
		{
			AutoSizeElement = AutoSizeElement.Both,
			Font = new Font("微软雅黑", 9f),
			ChildSpacing = new Size(2, 10),
			Dock = DockStyle.Fill,
			BorderColor = Color.Transparent,
			BorderThickness = 0
		};
	}

	private InputCheckBox MakeCheckBox(string text, int width, BreakType breakType = BreakType.Column)
	{
		return new InputCheckBox
		{
			Height = DefaultHeight,
			Text = text,
			Width = width,
			Break = breakType,
			Font = new Font("微软雅黑", 9f),
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center
		};
	}

	private InputCheckBox MakeCheckBox(string text, BreakType breakType = BreakType.Column)
	{
		return new InputCheckBox
		{
			Height = DefaultHeight,
			Text = text,
			Break = breakType,
			Font = new Font("微软雅黑", 9f),
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center
		};
	}

	private InputComboBox MakeComboBox(int selectedIndex, Dictionary<object, string> items, int width)
	{
		InputComboBox inputComboBox = new InputComboBox
		{
			DropDownStyle = InputComboBoxStyle.DropDownList,
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center,
			Font = new Font("微软雅黑", 9f),
			Width = width,
			Height = DefaultHeight
		};
		foreach (KeyValuePair<object, string> item in items)
		{
			inputComboBox.Items.Add(new InputOption
			{
				Text = item.Value,
				Tag = item.Key
			});
		}
		inputComboBox.SelectedIndex = 0;
		return inputComboBox;
	}

	private InputLabel MakeInputLabel(string text, int width = -1)
	{
		InputLabel inputLabel = new InputLabel
		{
			HorizontalAlign = InputContentAlignment.Far,
			VerticalAlign = InputContentAlignment.Center,
			Font = new Font("微软雅黑", 9f),
			Text = text,
			Height = DefaultHeight
		};
		if (width > 0)
		{
			inputLabel.Width = width;
		}
		return inputLabel;
	}

	private InputGroupHeader MakeGroupHeader(string text, bool collapsed = false)
	{
		return new InputGroupHeader
		{
			Font = new Font("微软雅黑", 9f),
			Collapsible = true,
			Collapsed = collapsed,
			Text = text,
			Height = DefaultHeight
		};
	}

	private InputNumericBox MakeInputNum(int width, BreakType breakType)
	{
		return new InputNumericBox
		{
			Height = DefaultHeight,
			Width = width,
			Break = breakType,
			Font = new Font("微软雅黑", 9f),
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center
		};
	}

	private InputTextBox MakeInputBox(int width, BreakType breakType)
	{
		return new InputTextBox
		{
			Height = DefaultHeight,
			Width = width,
			Break = breakType,
			Font = new Font("微软雅黑", 9f),
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center,
			Text = string.Empty
		};
	}

	private InputTextBox MakeInputBox(int width, BreakType breakType, string text)
	{
		return new InputTextBox
		{
			Height = DefaultHeight,
			Width = width,
			Break = breakType,
			Font = new Font("微软雅黑", 9f),
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center,
			Text = text
		};
	}

	private InputFontControl MakeInputFont(int width, BreakType breakType)
	{
		return new InputFontControl
		{
			Height = DefaultHeight,
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center,
			Font = new Font("微软雅黑", 9f),
			Break = breakType,
			Width = width
		};
	}

	private InputColorControl MakeInputColor(int width, BreakType breakType)
	{
		return new InputColorControl
		{
			Height = DefaultHeight,
			HorizontalAlign = InputContentAlignment.Near,
			VerticalAlign = InputContentAlignment.Center,
			Font = new Font("微软雅黑", 9f),
			Break = breakType,
			Width = width
		};
	}
}
