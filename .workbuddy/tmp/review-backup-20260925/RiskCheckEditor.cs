using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.C1Excel;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

// 账务数据风险检查主视图。
// 左侧 Tree = 方案列表（根节点 + 各方案）；上部 = 当前方案的检查项管理；下部 = 执行结果。
// 方案持久化经 RiskCheckStore（账套 SQLite 文件），检查项编辑走 frmRiskCheckRuleEditor 对话框。
public class RiskCheckEditor : ISetTheme
{
	private readonly C1Command cmdNewScheme = new C1Command();

	private readonly C1Command cmdRenameScheme = new C1Command();

	private readonly C1Command cmdDeleteScheme = new C1Command();

	private readonly C1Command cmdImportSchemes = new C1Command();

	private readonly C1Command cmdExportSchemes = new C1Command();

	private readonly C1Command cmdRunCheck = new C1Command();

	private readonly C1Command cmdMarkFocus = new C1Command();

	private readonly C1CommandLink lnkNewScheme = new C1CommandLink();

	private readonly C1CommandLink lnkRenameScheme = new C1CommandLink();

	private readonly C1CommandLink lnkDeleteScheme = new C1CommandLink();

	private readonly C1CommandLink lnkImportSchemes = new C1CommandLink();

	private readonly C1CommandLink lnkExportSchemes = new C1CommandLink();

	private readonly C1CommandLink lnkRunCheck = new C1CommandLink();

	private readonly C1CommandLink lnkMarkFocus = new C1CommandLink();

	private readonly C1ContextMenu ctxTree = new C1ContextMenu();

	private readonly C1ContextMenu ctxResults = new C1ContextMenu();

	private C1SplitterPanel pnlTop;

	private C1SplitterPanel pnlBottom;

	private Panel pnlRuleBar;

	private Panel pnlResultBar;

	private Panel pnlRuleFill;

	private Panel pnlResultFill;

	private FlowLayoutPanel flowRuleButtons;

	private FlowLayoutPanel flowResultButtons;

	private Button btnAddRule;

	private Button btnEditRule;

	private Button btnDeleteRule;

	private Button btnRunCheck;

	private Button btnMarkResult;

	private Button btnExportResult;

	private Label lblCount;

	private C1FlexGridEx grdRules;

	private C1FlexGridEx grdResults;

	private C1.Win.C1FlexGrid.CellStyle _headerStyleRules;

	private C1.Win.C1FlexGrid.CellStyle _headerStyleResults;

	private C1.Win.C1FlexGrid.CellStyle _errorStyle;

	private List<RiskCheckScheme> _schemes = new List<RiskCheckScheme>();

	private RiskCheckScheme _currentScheme;

	private List<RiskCheckResult> _lastResults;

	private bool _loadingTree = true;

	private bool _running;

	private readonly LedgerViewer _owner;

	public C1FlexGridEx Tree { get; private set; }

	public Panel View { get; private set; }

	public RiskCheckEditor(LedgerViewer owner)
	{
		_owner = owner;
		InitComponent();
		InitializeMenus();
		LoadSchemes();
		PopulateSchemeTree();
	}

	// 账套文件即 SQLite 数据库文件，方案随账套存储
	private string LedgerDbPath => _owner.CurrentFilePath;

	private void InitComponent()
	{
		View = new Panel();
		pnlTop = new C1SplitterPanel();
		pnlBottom = new C1SplitterPanel();
		pnlRuleBar = new Panel();
		pnlResultBar = new Panel();
		pnlRuleFill = new Panel();
		pnlResultFill = new Panel();
		flowRuleButtons = new FlowLayoutPanel();
		flowResultButtons = new FlowLayoutPanel();
		lblCount = new Label();
		grdRules = new C1FlexGridEx();
		grdResults = new C1FlexGridEx();
		btnAddRule = MakeButton("新增检查项");
		btnEditRule = MakeButton("编 辑");
		btnDeleteRule = MakeButton("删 除");
		btnRunCheck = MakeButton("执行检查");
		btnMarkResult = MakeButton("标记关注");
		btnExportResult = MakeButton("导出结果");
		btnAddRule.Click += delegate
		{
			AddRule();
		};
		btnEditRule.Click += delegate
		{
			EditSelectedRule();
		};
		btnDeleteRule.Click += delegate
		{
			DeleteSelectedRule();
		};
		btnRunCheck.Click += delegate
		{
			RunCheckCore();
		};
		btnMarkResult.Click += delegate
		{
			MarkSelectedResultsAsFocus();
		};
		btnExportResult.Click += delegate
		{
			ExportResults();
		};
		// —— 上部：检查项管理区 ——
		pnlRuleBar.Dock = DockStyle.Top;
		pnlRuleBar.Height = 38;
		pnlRuleBar.BackColor = Color.White;
		flowRuleButtons.Dock = DockStyle.Left;
		flowRuleButtons.AutoSize = true;
		flowRuleButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		flowRuleButtons.WrapContents = false;
		flowRuleButtons.Padding = new Padding(8, 6, 0, 0);
		btnAddRule.Margin = new Padding(0, 0, 8, 0);
		btnEditRule.Margin = new Padding(0, 0, 8, 0);
		btnDeleteRule.Margin = new Padding(0, 0, 0, 0);
		flowRuleButtons.Controls.Add(btnAddRule);
		flowRuleButtons.Controls.Add(btnEditRule);
		flowRuleButtons.Controls.Add(btnDeleteRule);
		pnlRuleFill.Dock = DockStyle.Fill;
		pnlRuleBar.Controls.Add(pnlRuleFill);
		pnlRuleBar.Controls.Add(flowRuleButtons);
		SetupGrid(grdRules);
		_headerStyleRules = grdRules.Styles.Add("rcHeaderRules");
		_headerStyleRules.TextAlign = TextAlignEnum.CenterCenter;
		C1.Win.C1FlexGrid.Column column = grdRules.Cols.Add();
		column.Caption = "类型";
		column.Name = "type";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.Width = 70;
		column = grdRules.Cols.Add();
		column.Caption = "说明";
		column.Name = "note";
		column.DataType = typeof(string);
		column.Width = 220;
		column = grdRules.Cols.Add();
		column.Caption = "摘要";
		column.Name = "digest";
		column.DataType = typeof(string);
		C1.Win.C1FlexGrid.Row headerRow1 = grdRules.Rows.Add();
		for (int i = 0; i < grdRules.Cols.Count; i++)
		{
			grdRules.SetCellStyle(0, i, _headerStyleRules);
		}
		headerRow1["type"] = "类型";
		headerRow1["note"] = "说明";
		headerRow1["digest"] = "摘要";
		grdRules.Rows.Fixed = 1;
		grdRules.Cols.Fixed = 0;
		pnlTop.Controls.Add(grdRules);
		pnlTop.Controls.Add(pnlRuleBar);
		// —— 下部：结果区 ——
		pnlResultBar.Dock = DockStyle.Top;
		pnlResultBar.Height = 38;
		pnlResultBar.BackColor = Color.White;
		flowResultButtons.Dock = DockStyle.Left;
		flowResultButtons.AutoSize = true;
		flowResultButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		flowResultButtons.WrapContents = false;
		flowResultButtons.Padding = new Padding(8, 6, 0, 0);
		btnRunCheck.Margin = new Padding(0, 0, 8, 0);
		btnMarkResult.Margin = new Padding(0, 0, 8, 0);
		btnExportResult.Margin = new Padding(0, 0, 0, 0);
		flowResultButtons.Controls.Add(btnRunCheck);
		flowResultButtons.Controls.Add(btnMarkResult);
		flowResultButtons.Controls.Add(btnExportResult);
		lblCount.Dock = DockStyle.Right;
		lblCount.AutoSize = true;
		lblCount.Font = new Font("微软雅黑", 9.5f);
		lblCount.ForeColor = Color.FromArgb(30, 41, 59);
		lblCount.BackColor = Color.Transparent;
		lblCount.TextAlign = ContentAlignment.MiddleRight;
		lblCount.Padding = new Padding(0, 0, 12, 0);
		lblCount.Text = "未执行";
		pnlResultFill.Dock = DockStyle.Fill;
		pnlResultBar.Controls.Add(pnlResultFill);
		pnlResultBar.Controls.Add(flowResultButtons);
		pnlResultBar.Controls.Add(lblCount);
		SetupGrid(grdResults);
		_headerStyleResults = grdResults.Styles.Add("rcHeaderResults");
		_headerStyleResults.TextAlign = TextAlignEnum.CenterCenter;
		column = grdResults.Cols.Add();
		column.Caption = "检查项";
		column.Name = "rule";
		column.DataType = typeof(string);
		column.Width = 180;
		column = grdResults.Cols.Add();
		column.Caption = "类型";
		column.Name = "type";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column.Width = 70;
		column = grdResults.Cols.Add();
		column.Caption = "科目代码";
		column.Name = "code";
		column.DataType = typeof(string);
		column.Width = 110;
		column = grdResults.Cols.Add();
		column.Caption = "科目名称";
		column.Name = "name";
		column.DataType = typeof(string);
		column.Width = 140;
		column = grdResults.Cols.Add();
		column.Caption = "命中说明";
		column.Name = "hit";
		column.DataType = typeof(string);
		column.Width = 280;
		column = grdResults.Cols.Add();
		column.Caption = "金额";
		column.Name = "amount";
		column.DataType = typeof(decimal);
		column.Format = "#,0.00;-#,0.00;#";
		column.TextAlign = TextAlignEnum.RightCenter;
		column.Width = 120;
		column = grdResults.Cols.Add();
		column.Caption = "辅助核算明细";
		column.Name = "aux";
		column.DataType = typeof(string);
		column.Width = 200;
		column = grdResults.Cols.Add();
		column.Caption = "凭证信息";
		column.Name = "voucher";
		column.DataType = typeof(string);
		column.Width = 240;
		C1.Win.C1FlexGrid.Row headerRow2 = grdResults.Rows.Add();
		for (int j = 0; j < grdResults.Cols.Count; j++)
		{
			grdResults.SetCellStyle(0, j, _headerStyleResults);
		}
		headerRow2["rule"] = "检查项";
		headerRow2["type"] = "类型";
		headerRow2["code"] = "科目代码";
		headerRow2["name"] = "科目名称";
		headerRow2["hit"] = "命中说明";
		headerRow2["amount"] = "金额";
		headerRow2["aux"] = "辅助核算明细";
		headerRow2["voucher"] = "凭证信息";
		grdResults.Rows.Fixed = 1;
		grdResults.Cols.Fixed = 0;
		grdResults.MouseClick += GrdResults_MouseClick;
		pnlBottom.Controls.Add(grdResults);
		pnlBottom.Controls.Add(pnlResultBar);
		// —— 上下分割 ——
		pnlTop.Height = 320;
		pnlTop.MinHeight = 150;
		pnlTop.KeepRelativeSize = true;
		pnlBottom.Height = 420;
		pnlBottom.MinHeight = 200;
		pnlBottom.KeepRelativeSize = true;
		C1SplitContainer container = new C1SplitContainer();
		container.AutoSizeElement = AutoSizeElement.Both;
		container.BackColor = Color.FromArgb(243, 244, 246);
		container.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		container.Dock = DockStyle.Fill;
		container.ForeColor = Color.FromArgb(0, 0, 0);
		container.Panels.Add(pnlTop);
		container.Panels.Add(pnlBottom);
		container.BringToFront();
		View.Dock = DockStyle.Fill;
		View.BackColor = Color.FromArgb(243, 244, 246);
		View.Controls.Add(container);
		// —— 方案树（挂到 LedgerViewer 左侧） ——
		Tree = GridFactory.Create("tree");
		Tree.Paint += delegate(object s1, PaintEventArgs e1)
		{
			Auditai.UI.Controls.Theme.DrawFormBorder(Tree, e1.Graphics);
		};
		Tree.MouseClick += Tree_MouseClick;
		Tree.DoubleClick += Tree_DoubleClick;
		Tree.AfterRowColChange += Tree_AfterRowColChange;
	}

	private static void SetupGrid(C1FlexGridEx grid)
	{
		grid.AllowEditing = false;
		grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grid.Dock = DockStyle.Fill;
		grid.DrawMode = DrawModeEnum.OwnerDraw;
		grid.Font = new Font("微软雅黑", 9.5f);
		grid.Rows.DefaultSize = 20;
		grid.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		grid.ExtendLastCol = true;
		grid.SelectionMode = SelectionModeEnum.Row;
	}

	private static Button MakeButton(string text)
	{
		Button button = new Button();
		button.Text = text;
		button.Font = new Font("微软雅黑", 9f);
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
		button.FlatAppearance.BorderSize = 1;
		button.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 247, 252);
		button.BackColor = Color.White;
		button.ForeColor = Color.FromArgb(30, 41, 59);
		button.Size = new Size(88, 26);
		button.UseVisualStyleBackColor = false;
		button.Cursor = Cursors.Hand;
		return button;
	}

	private void InitializeMenus()
	{
		cmdNewScheme.Text = "新建方案";
		cmdNewScheme.Click += delegate
		{
			NewScheme();
		};
		lnkNewScheme.Command = cmdNewScheme;
		ctxTree.CommandLinks.Add(lnkNewScheme);
		cmdRenameScheme.Text = "重命名";
		cmdRenameScheme.Click += delegate
		{
			RenameScheme();
		};
		cmdRenameScheme.CommandStateQuery += CmdSchemeOnly_CommandStateQuery;
		lnkRenameScheme.Command = cmdRenameScheme;
		ctxTree.CommandLinks.Add(lnkRenameScheme);
		cmdDeleteScheme.Text = "删除";
		cmdDeleteScheme.Click += delegate
		{
			DeleteScheme();
		};
		cmdDeleteScheme.CommandStateQuery += CmdSchemeOnly_CommandStateQuery;
		lnkDeleteScheme.Command = cmdDeleteScheme;
		ctxTree.CommandLinks.Add(lnkDeleteScheme);
		cmdImportSchemes.Text = "导入方案";
		cmdImportSchemes.Click += delegate
		{
			ImportSchemes();
		};
		lnkImportSchemes.Command = cmdImportSchemes;
		ctxTree.CommandLinks.Add(lnkImportSchemes);
		cmdExportSchemes.Text = "导出方案";
		cmdExportSchemes.Click += delegate
		{
			ExportSchemes();
		};
		cmdExportSchemes.CommandStateQuery += CmdSchemeOnly_CommandStateQuery;
		lnkExportSchemes.Command = cmdExportSchemes;
		ctxTree.CommandLinks.Add(lnkExportSchemes);
		cmdRunCheck.Text = "执行检查";
		cmdRunCheck.Click += delegate
		{
			RunCheckCore();
		};
		cmdRunCheck.CommandStateQuery += CmdSchemeOnly_CommandStateQuery;
		lnkRunCheck.Command = cmdRunCheck;
		ctxTree.CommandLinks.Add(lnkRunCheck);
		cmdMarkFocus.Text = "标记关注";
		cmdMarkFocus.Click += delegate
		{
			MarkSelectedResultsAsFocus();
		};
		lnkMarkFocus.Command = cmdMarkFocus;
		ctxResults.CommandLinks.Add(lnkMarkFocus);
	}

	// 仅选中方案行时显示的菜单项（对齐 ValidateEditor 的 CommandStateQuery 模式）
	private void CmdSchemeOnly_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		if (sender is C1Command command)
		{
			command.Visible = GetSelectedScheme() != null;
		}
	}

	private RiskCheckScheme GetSelectedScheme()
	{
		if (Tree.Row >= 0 && Tree.Row < Tree.Rows.Count)
		{
			return Tree.Rows[Tree.Row].UserData as RiskCheckScheme;
		}
		return null;
	}

	private RiskCheckRule GetSelectedRule()
	{
		if (grdRules.Row >= grdRules.Rows.Fixed && grdRules.Row < grdRules.Rows.Count)
		{
			return grdRules.Rows[grdRules.Row].UserData as RiskCheckRule;
		}
		return null;
	}

	// —— 方案数据 ——
	private void LoadSchemes()
	{
		_schemes = new List<RiskCheckScheme>();
		try
		{
			if (!string.IsNullOrEmpty(LedgerDbPath))
			{
				_schemes = RiskCheckStore.Load(LedgerDbPath) ?? new List<RiskCheckScheme>();
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "风险检查方案加载失败：" + ex.Message);
		}
	}

	private void PopulateSchemeTree()
	{
		_loadingTree = true;
		Tree.BeginUpdate();
		try
		{
			Tree.Rows.Count = 0;
			Tree.Cols.Count = 0;
			Tree.Cols.Add();
			Tree.Rows.DefaultSize = 33;
			Tree.Tree.Column = 0;
			C1.Win.C1FlexGrid.Row rootRow = Tree.Rows.Add();
			rootRow.IsNode = true;
			rootRow.Node.Data = $"风险检查方案（{_schemes.Count}）";
			foreach (RiskCheckScheme scheme in _schemes)
			{
				Node node = rootRow.Node.AddNode(NodeTypeEnum.LastChild, scheme.Name);
				node.Key = scheme;
			}
			Tree.AllowEditing = false;
			Tree.Tree.Show(Tree.Tree.MaximumLevel);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
		finally
		{
			Tree.EndUpdate();
			_loadingTree = false;
		}
		long selectId = _currentScheme?.Id ?? 0L;
		int selectRow = -1;
		for (int i = 0; i < Tree.Rows.Count; i++)
		{
			if (Tree.Rows[i].UserData is RiskCheckScheme scheme && scheme.Id == selectId)
			{
				selectRow = i;
				break;
			}
		}
		if (selectRow < 0)
		{
			for (int j = 0; j < Tree.Rows.Count; j++)
			{
				if (Tree.Rows[j].UserData is RiskCheckScheme)
				{
					selectRow = j;
					break;
				}
			}
		}
		if (selectRow >= 0)
		{
			Tree.Row = selectRow;
		}
		else
		{
			SyncSelectedScheme(force: true);
		}
	}

	private void RefreshSchemeTree()
	{
		PopulateSchemeTree();
	}

	private void Tree_AfterRowColChange(object sender, RangeEventArgs e)
	{
		if (!_loadingTree)
		{
			SyncSelectedScheme(force: false);
		}
	}

	private void SyncSelectedScheme(bool force)
	{
		RiskCheckScheme scheme = GetSelectedScheme();
		if (!force && scheme == _currentScheme)
		{
			return;
		}
		_currentScheme = scheme;
		RefreshRulesGrid();
		if (scheme == null)
		{
			ClearResults();
		}
	}

	private void Tree_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTest = Tree.HitTest(e.Location);
		if (hitTest.Type != HitTestTypeEnum.Cell)
		{
			return;
		}
		if (hitTest.Row >= 0 && hitTest.Row < Tree.Rows.Count && Tree.Rows[hitTest.Row].UserData is RiskCheckScheme && Tree.Row != hitTest.Row)
		{
			Tree.Row = hitTest.Row;
		}
		NativeMenuShim.Show(ctxTree, Tree, e.Location);
	}

	private void Tree_DoubleClick(object sender, EventArgs e)
	{
		if (Tree.MouseRow >= 0 && Tree.MouseRow < Tree.Rows.Count && Tree.Rows[Tree.MouseRow].UserData is RiskCheckScheme)
		{
			RenameScheme();
		}
	}

	// —— 方案管理 ——
	private void NewScheme()
	{
		string name = InputForm.Text("新建方案", "请输入方案名称", "", 220);
		if (name == null)
		{
			return;
		}
		name = name.Trim();
		if (name.Length == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案名称不能为空");
			return;
		}
		RiskCheckScheme scheme = new RiskCheckScheme
		{
			Id = 0,
			Name = name
		};
		try
		{
			RiskCheckStore.Save(LedgerDbPath, scheme);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案保存失败：" + ex.Message);
			return;
		}
		_currentScheme = scheme;
		_schemes.Add(scheme);
		RefreshSchemeTree();
	}

	private void RenameScheme()
	{
		RiskCheckScheme scheme = GetSelectedScheme();
		if (scheme == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在左侧选择一个方案");
			return;
		}
		string name = InputForm.Text("重命名方案", "请输入方案名称", scheme.Name, 220);
		if (name == null)
		{
			return;
		}
		name = name.Trim();
		if (name.Length == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案名称不能为空");
			return;
		}
		scheme.Name = name;
		try
		{
			RiskCheckStore.Save(LedgerDbPath, scheme);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案保存失败：" + ex.Message);
			return;
		}
		RefreshSchemeTree();
	}

	private void DeleteScheme()
	{
		RiskCheckScheme scheme = GetSelectedScheme();
		if (scheme == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在左侧选择一个方案");
			return;
		}
		DialogResult dialogResult = Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, $"确定删除方案「{scheme.Name}」吗？该方案下的全部检查项将一并删除。", MessageBoxButtons.YesNo, "删除方案");
		if (dialogResult != DialogResult.Yes)
		{
			return;
		}
		try
		{
			RiskCheckStore.Delete(LedgerDbPath, scheme.Id);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案删除失败：" + ex.Message);
			return;
		}
		_schemes.Remove(scheme);
		if (_currentScheme == scheme)
		{
			_currentScheme = null;
		}
		RefreshSchemeTree();
	}

	private void ExportSchemes()
	{
		RiskCheckScheme selected = GetSelectedScheme();
		List<RiskCheckScheme> toExport = (selected != null) ? new List<RiskCheckScheme> { selected } : _schemes;
		if (toExport.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前没有可导出的方案");
			return;
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "风险检查方案|*.json",
			DefaultExt = ".json",
			FileName = (selected?.Name ?? "风险检查方案") + ".json"
		};
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			string json = RiskCheckStore.ExportJson(toExport);
			File.WriteAllText(saveFileDialog.FileName, json, Encoding.UTF8);
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案已导出：" + saveFileDialog.FileName);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案导出失败：" + ex.Message);
		}
	}

	private void ImportSchemes()
	{
		if (string.IsNullOrEmpty(LedgerDbPath))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = "风险检查方案|*.json"
		};
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		List<RiskCheckScheme> imported;
		try
		{
			imported = RiskCheckStore.ImportJson(File.ReadAllText(openFileDialog.FileName));
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案文件解析失败：" + ex.Message);
			return;
		}
		imported = (imported ?? new List<RiskCheckScheme>()).Where((RiskCheckScheme s) => s != null && !string.IsNullOrWhiteSpace(s.Name)).ToList();
		if (imported.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案文件中没有有效方案");
			return;
		}
		// 置 0 让 Save 自动分配新 Id，避免顶掉现有方案/检查项
		foreach (RiskCheckScheme scheme in imported)
		{
			scheme.Id = 0;
			foreach (RiskCheckRule rule in scheme.Rules)
			{
				rule.Id = 0;
				rule.SchemeId = 0;
			}
		}
		int count = 0;
		try
		{
			foreach (RiskCheckScheme scheme2 in imported)
			{
				RiskCheckStore.Save(LedgerDbPath, scheme2);
				_schemes.Add(scheme2);
				count++;
			}
		}
		catch (Exception ex2)
		{
			ex2.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"方案导入失败（已导入 {count} 个）：" + ex2.Message);
		}
		_currentScheme = (count > 0 ? _schemes[_schemes.Count - 1] : _currentScheme);
		RefreshSchemeTree();
		if (count > 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"已导入 {count} 个方案。");
		}
	}

	// —— 检查项管理 ——
	private void RefreshRulesGrid()
	{
		grdRules.BeginUpdate();
		try
		{
			grdRules.Rows.Count = grdRules.Rows.Fixed;
			if (_currentScheme?.Rules == null)
			{
				return;
			}
			foreach (RiskCheckRule rule in _currentScheme.Rules)
			{
				C1.Win.C1FlexGrid.Row row = grdRules.Rows.Add();
				row.UserData = rule;
				row["type"] = (rule.RuleType == RiskCheckRule.RULE_TYPE_FORMULA) ? "公式" : "条件";
				row["note"] = rule.Note ?? "";
				row["digest"] = BuildDigest(rule);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
		finally
		{
			grdRules.EndUpdate();
		}
	}

	private void SaveCurrentScheme()
	{
		try
		{
			RiskCheckStore.Save(LedgerDbPath, _currentScheme);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "方案保存失败：" + ex.Message);
		}
		RefreshRulesGrid();
	}

	private void AddRule()
	{
		if (_currentScheme == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在左侧选择一个方案");
			return;
		}
		RiskCheckRule rule = new RiskCheckRule
		{
			RuleType = RiskCheckRule.RULE_TYPE_CONDITION
		};
		using frmRiskCheckRuleEditor frmRiskCheckRuleEditor2 = new frmRiskCheckRuleEditor(rule);
		if (frmRiskCheckRuleEditor2.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		_currentScheme.Rules.Add(rule);
		SaveCurrentScheme();
	}

	private void EditSelectedRule()
	{
		if (_currentScheme == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在左侧选择一个方案");
			return;
		}
		RiskCheckRule rule = GetSelectedRule();
		if (rule == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中一个检查项");
			return;
		}
		using frmRiskCheckRuleEditor frmRiskCheckRuleEditor2 = new frmRiskCheckRuleEditor(rule);
		if (frmRiskCheckRuleEditor2.ShowDialog() == DialogResult.OK)
		{
			SaveCurrentScheme();
		}
	}

	private void DeleteSelectedRule()
	{
		if (_currentScheme == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在左侧选择一个方案");
			return;
		}
		RiskCheckRule rule = GetSelectedRule();
		if (rule == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中一个检查项");
			return;
		}
		DialogResult dialogResult = Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "确定删除选中的检查项吗？", MessageBoxButtons.YesNo, "删除检查项");
		if (dialogResult != DialogResult.Yes)
		{
			return;
		}
		_currentScheme.Rules.Remove(rule);
		SaveCurrentScheme();
	}

	// —— 摘要 ——
	// 条件检查项：把启用的维度拼成短文本，如"代码:1122*|期末:贷≥0|负数辅助"
	private static string BuildDigest(RiskCheckRule rule)
	{
		if (rule.RuleType == RiskCheckRule.RULE_TYPE_FORMULA)
		{
			return Truncate($"{rule.LeftExpr} {OpSymbol(rule.OperatorCode)} {rule.RightExpr}", 60);
		}
		List<string> parts = new List<string>();
		if (!string.IsNullOrWhiteSpace(rule.AccountCodes))
		{
			parts.Add("代码:" + rule.AccountCodes);
		}
		if (!string.IsNullOrWhiteSpace(rule.AccountNames))
		{
			parts.Add("名称:" + rule.AccountNames);
		}
		if (rule.RequireLeaf)
		{
			parts.Add("仅叶子科目");
		}
		if (rule.OpeningEnabled)
		{
			parts.Add($"期初:{DirectionChar(rule.OpeningDirection)}{OpSymbol(rule.OpeningOp)}{FormatValue(rule.OpeningValue)}");
		}
		if (rule.ClosingEnabled)
		{
			parts.Add($"期末:{DirectionChar(rule.ClosingDirection)}{OpSymbol(rule.ClosingOp)}{FormatValue(rule.ClosingValue)}");
		}
		if (rule.DebitEnabled)
		{
			parts.Add((rule.DebitScope == RiskCheckRule.SCOPE_PERIOD_TOTAL ? "本期借方" : "单凭证借方") + $":{OpSymbol(rule.DebitOp)}{FormatValue(rule.DebitValue)}");
		}
		if (rule.CreditEnabled)
		{
			parts.Add((rule.CreditScope == RiskCheckRule.SCOPE_PERIOD_TOTAL ? "本期贷方" : "单凭证贷方") + $":{OpSymbol(rule.CreditOp)}{FormatValue(rule.CreditValue)}");
		}
		if (rule.AuxNegativeEnabled)
		{
			parts.Add((rule.AuxValue != 0m) ? $"负数辅助:{OpSymbol(rule.AuxOp)}{FormatValue(rule.AuxValue)}" : "负数辅助");
		}
		return string.Join("|", parts);
	}

	// Op 编码对齐 ValidationOperator.Code：0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"
	private static string OpSymbol(int op)
	{
		switch (op)
		{
		case 0:
			return "=";
		case 1:
			return ">";
		case 2:
			return ">=";
		case 3:
			return "<";
		case 4:
			return "<=";
		case 5:
			return "<>";
		default:
			return op.ToString();
		}
	}

	private static string DirectionChar(int direction)
	{
		switch (direction)
		{
		case RiskCheckRule.DIRECTION_DEBIT:
			return "借";
		case RiskCheckRule.DIRECTION_CREDIT:
			return "贷";
		default:
			return "";
		}
	}

	private static string FormatValue(decimal value)
	{
		return value.ToString("0.##");
	}

	private static string Truncate(string text, int max)
	{
		if (string.IsNullOrEmpty(text) || text.Length <= max)
		{
			return text ?? "";
		}
		return text.Substring(0, max) + "…";
	}

	// —— 执行检查 ——
	private async void RunCheckCore()
	{
		if (_running)
		{
			return;
		}
		if (_currentScheme == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先在左侧选择一个风险检查方案");
			return;
		}
		if (_owner.Ledger == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		_running = true;
		SetRunning(true);
		lblCount.Text = "检查中…";
		try
		{
			RiskCheckScheme scheme = _currentScheme;
			// 条件规则在后台线程执行（纯内存计算）；公式规则回到 UI 线程执行——其内部经
			// FormulaRuleExecutor 访问虚拟表构建缓存与 LedgerCacheManager（非线程安全），避免并发
			List<RiskCheckResult> results = await Task.Run(() => RiskCheckEngine.RunConditionRules(_owner.Ledger, scheme));
			if (scheme.Rules != null)
			{
				foreach (RiskCheckRule rule in scheme.Rules)
				{
					if (rule != null && rule.RuleType == RiskCheckRule.RULE_TYPE_FORMULA)
					{
						results.AddRange(RiskCheckEngine.RunFormulaRule(_owner.Ledger, rule));
					}
				}
			}
			_lastResults = results ?? new List<RiskCheckResult>();
			FillResults(_lastResults);
			int hits = _lastResults.Count((RiskCheckResult r) => !r.IsError);
			int errors = _lastResults.Count((RiskCheckResult r) => r.IsError);
			lblCount.Text = $"共命中 {hits} 条" + ((errors > 0) ? $"（错误 {errors} 条）" : "");
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "检查执行失败：" + ex.Message);
			lblCount.Text = "未执行";
		}
		finally
		{
			SetRunning(false);
			_running = false;
		}
	}

	private void SetRunning(bool running)
	{
		btnRunCheck.Enabled = !running;
		btnMarkResult.Enabled = !running;
		btnExportResult.Enabled = !running;
		btnAddRule.Enabled = !running;
		btnEditRule.Enabled = !running;
		btnDeleteRule.Enabled = !running;
	}

	private void ClearResults()
	{
		_lastResults = null;
		grdResults.Rows.Count = grdResults.Rows.Fixed;
		lblCount.Text = "未执行";
	}

	private void FillResults(List<RiskCheckResult> results)
	{
		grdResults.BeginUpdate();
		try
		{
			grdResults.Rows.Count = grdResults.Rows.Fixed;
			foreach (RiskCheckResult result in results)
			{
				C1.Win.C1FlexGrid.Row row = grdResults.Rows.Add();
				row.UserData = result;
				string note = result.RuleNote ?? result.Rule?.Note ?? "";
				if (result.IsError)
				{
					row["rule"] = note;
					row["type"] = "错误";
					row["hit"] = result.ErrorMessage ?? "";
					ApplyErrorStyle(row.Index);
				}
				else
				{
					row["rule"] = note;
					row["type"] = (result.RuleType == RiskCheckRule.RULE_TYPE_FORMULA) ? "公式" : "条件";
					row["code"] = result.AccountCode ?? "";
					row["name"] = result.AccountName ?? "";
					row["hit"] = result.HitDescription ?? "";
					row["amount"] = result.Amount;
					row["aux"] = result.AuxDetail ?? "";
					row["voucher"] = result.VoucherInfo ?? "";
				}
			}
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.ToString());
		}
		finally
		{
			grdResults.EndUpdate();
		}
	}

	// 错误行整行标红
	private void ApplyErrorStyle(int rowIndex)
	{
		if (_errorStyle == null)
		{
			_errorStyle = grdResults.Styles.Add("rcError");
			_errorStyle.ForeColor = Color.FromArgb(192, 57, 43);
		}
		for (int c = 0; c < grdResults.Cols.Count; c++)
		{
			grdResults.SetCellStyle(rowIndex, c, _errorStyle);
		}
	}

	// —— 标记关注 ——
	// 结果网格右键菜单：定位到鼠标所在行（不在当前选择范围内时改选该行）后弹出（对齐 Tree_MouseClick 模式）
	private void GrdResults_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		HitTestInfo hitTest = grdResults.HitTest(e.Location);
		if (hitTest.Type != HitTestTypeEnum.Cell || hitTest.Row < grdResults.Rows.Fixed)
		{
			return;
		}
		if ((hitTest.Row < grdResults.Selection.TopRow || hitTest.Row > grdResults.Selection.BottomRow) && grdResults.Row != hitTest.Row)
		{
			grdResults.Row = hitTest.Row;
		}
		NativeMenuShim.Show(ctxResults, grdResults, e.Location);
	}

	// 把选中结果行对应的凭证加入标记关注（VoucherMark/Source=风险检查，程序化标记不走 ToggleMark）；
	// 科目级命中（结果.Voucher 为空且非错误行）无单一凭证，提示穿透明细账后标记具体凭证
	private void MarkSelectedResultsAsFocus()
	{
		if (_lastResults == null || _lastResults.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先执行检查后再标记关注");
			return;
		}
		if (grdResults.BodyRow < 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选中结果行");
			return;
		}
		if (_owner.Ledger == null)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先打开账套文件后操作");
			return;
		}
		int marked = 0;
		int skipped = 0;
		int accountLevel = 0;
		RiskCheckResult firstAccountLevel = null;
		for (int body = grdResults.BodyRow; body <= grdResults.BodyRowSel; body++)
		{
			if (!(grdResults.Rows[body + grdResults.Rows.Fixed].UserData is RiskCheckResult result) || result.IsError)
			{
				continue;
			}
			if (result.Voucher != null)
			{
				if (result.Voucher.VoucherMark)
				{
					skipped++;
					continue;
				}
				result.Voucher.VoucherMark = true;
				result.Voucher.VoucherMarkSource = Voucher.MARK_SOURCE_RISK_CHECK;
				result.Voucher.Dirty = 2;
				marked++;
			}
			else
			{
				accountLevel++;
				if (firstAccountLevel == null)
				{
					firstAccountLevel = result;
				}
			}
		}
		if (marked + skipped == 0)
		{
			// 选中不含凭证级行：全为科目级命中时询问跳转明细账，全为错误行时给出一般提示
			if (firstAccountLevel == null)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选中有效结果行后再标记关注");
				return;
			}
			string accountText = $"{firstAccountLevel.AccountCode} {firstAccountLevel.AccountName}".Trim();
			DialogResult dialogResult = Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Question, "科目级异常无单一凭证可标记，请穿透明细账后标记具体凭证。\n\n是否跳转到科目「" + accountText + "」的明细账？", MessageBoxButtons.YesNo, "标记关注");
			if (dialogResult == DialogResult.Yes)
			{
				Account account = FindAccountByCode(firstAccountLevel.AccountCode);
				if (account == null)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "未找到科目：" + firstAccountLevel.AccountCode);
					return;
				}
				ShowSubsidiary(account);
			}
			return;
		}
		if (marked > 0)
		{
			_owner.Ledger.Save();
		}
		string text = (marked > 0) ? $"已将 {marked} 张凭证加入标记关注（跳过已标记 {skipped} 张）" : $"选中凭证均已加入标记关注（跳过已标记 {skipped} 张）";
		if (accountLevel > 0)
		{
			text += $"\n另有 {accountLevel} 条科目级命中无单一凭证，请穿透明细账后标记具体凭证";
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, text);
	}

	private Account FindAccountByCode(string code)
	{
		if (string.IsNullOrEmpty(code) || _owner.Ledger?.Accounts == null)
		{
			return null;
		}
		return _owner.Ledger.Accounts.FirstOrDefault((Account a) => a != null && a.Code == code);
	}

	// 跳转到指定科目的明细账（对齐 LedgerViewer.TrendencyEditor_GrdTrendTable_DoubleClick 的跳转写法）
	private void ShowSubsidiary(Account account)
	{
		try
		{
			_owner.SubsidiaryEditor.SubStatus = Auditai.DTO.SubOrTotal.Subsidiary;
			_owner.SubsidiaryEditor.PopulateSubsidiarySheet(account, _owner.StartDate, _owner.EndDate);
			_owner.SubsidiaryEditor.UpdateTitle(account);
			if (_owner.SwitchToView(ActiveView.Subsidiary))
			{
				Common.SetTreeCheck(_owner.AccountTreeEditor.Tree, CheckEnum.None);
			}
			_owner.CurrentAccount = account;
			_owner.CurrentAuxiliary = null;
			_owner.AccountTreeEditor.UpdateNodeStatus(account);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
	}

	// —— 导出结果 ——
	private void ExportResults()
	{
		if (_lastResults == null || _lastResults.Count == 0)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先执行检查后再导出结果");
			return;
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "Excel 文件|*.xlsx|HTML 报告|*.html",
			DefaultExt = ".xlsx",
			FileName = (_currentScheme?.Name ?? "风险检查") + "-检查结果"
		};
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			if (Path.GetExtension(saveFileDialog.FileName).Equals(".html", StringComparison.OrdinalIgnoreCase))
			{
				ExportResultsHtml(saveFileDialog.FileName);
			}
			else
			{
				ExportResultsExcel(saveFileDialog.FileName);
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "结果已导出：" + saveFileDialog.FileName);
		}
		catch (Exception ex)
		{
			ex.Log();
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "结果导出失败：" + ex.Message);
		}
	}

	// Excel 导出：复用 LedgerExporter（C1.C1Excel）模式
	private void ExportResultsExcel(string fileName)
	{
		RiskResultExporter riskResultExporter = new RiskResultExporter(_lastResults);
		riskResultExporter.Build();
		riskResultExporter.xlBook.Save(fileName);
	}

	private void ExportResultsHtml(string fileName)
	{
		int hits = _lastResults.Count((RiskCheckResult r) => !r.IsError);
		int errors = _lastResults.Count((RiskCheckResult r) => r.IsError);
		StringBuilder html = new StringBuilder();
		html.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>风险检查报告</title>");
		html.Append("<style>body{font-family:'微软雅黑',sans-serif;margin:24px;color:#222}h1{font-size:20px}h2{font-size:15px;margin-top:24px}");
		html.Append("table{border-collapse:collapse;width:100%;font-size:12px}th,td{border:1px solid #ccc;padding:6px 8px;text-align:left}");
		html.Append("th{background:#f0f4f8}.err{color:#c83c3c}.muted{color:#888;font-size:12px}</style></head><body>");
		html.Append("<h1>风险检查报告</h1>");
		html.Append($"<p class=\"muted\">账套：{HtmlEncode(Path.GetFileNameWithoutExtension(_owner.CurrentFilePath))}　方案：{HtmlEncode(_currentScheme?.Name)}　生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
		html.Append("<h2>共命中 " + hits + " 条" + ((errors > 0) ? $"，错误 {errors} 条" : "") + "</h2>");
		html.Append("<table><tr><th>检查项</th><th>类型</th><th>科目代码</th><th>科目名称</th><th>命中说明</th><th>金额</th><th>辅助核算明细</th><th>凭证信息</th></tr>");
		foreach (RiskCheckResult result in _lastResults)
		{
			string type = result.IsError ? "错误" : ((result.RuleType == RiskCheckRule.RULE_TYPE_FORMULA) ? "公式" : "条件");
			string cls = result.IsError ? " class=\"err\"" : "";
			string amount = result.IsError ? "" : result.Amount.ToString("#,0.00;-#,0.00;#");
			string hit = result.IsError ? (result.ErrorMessage ?? "") : (result.HitDescription ?? "");
			html.Append($"<tr{cls}><td>{HtmlEncode(result.RuleNote ?? result.Rule?.Note ?? "")}</td><td>{type}</td><td>{HtmlEncode(result.AccountCode ?? "")}</td><td>{HtmlEncode(result.AccountName ?? "")}</td><td>{HtmlEncode(hit)}</td><td>{amount}</td><td>{HtmlEncode(result.AuxDetail ?? "")}</td><td>{HtmlEncode(result.VoucherInfo ?? "")}</td></tr>");
		}
		html.Append("</table></body></html>");
		File.WriteAllText(fileName, html.ToString(), Encoding.UTF8);
	}

	// LedgerViewer2 未引用 System.Web，手写转义
	private static string HtmlEncode(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}
		return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
	}

	public void SetTheme()
	{
		Tree.Styles.Alternate.BackColor = Color.Transparent;
		Tree.Styles.Fixed.Border.Width = 0;
		Tree.Styles.Normal.Border.Width = 0;
	}

	// 结果 Excel 导出器（对齐 MarkedVouchersExporter 的 LedgerExporter 用法）
	private sealed class RiskResultExporter : LedgerExporter
	{
		private const int COL_RULE = 0;

		private const int COL_TYPE = 1;

		private const int COL_CODE = 2;

		private const int COL_NAME = 3;

		private const int COL_HIT = 4;

		private const int COL_AMOUNT = 5;

		private const int COL_AUX = 6;

		private const int COL_VOUCHER = 7;

		private readonly List<RiskCheckResult> _results;

		public RiskResultExporter(List<RiskCheckResult> results)
		{
			_results = results;
		}

		public override void Build()
		{
			XLSheet xLSheet = xlBook.Sheets[0];
			string[] headers = new string[8] { "检查项", "类型", "科目代码", "科目名称", "命中说明", "金额", "辅助核算明细", "凭证信息" };
			int[] widths = new int[8] { 180, 70, 110, 140, 320, 110, 200, 260 };
			for (int i = 0; i < headers.Length; i++)
			{
				xLSheet[0, i].SetValue(headers[i], styleHCenter);
				xLSheet.Columns[i].Width = C1XLBook.PixelsToTwips((double)widths[i]);
			}
			int num = 0;
			foreach (RiskCheckResult result in _results)
			{
				num++;
				xLSheet[num, 0].SetValue(result.RuleNote ?? result.Rule?.Note ?? "", styleBorder);
				xLSheet[num, 1].SetValue(result.IsError ? "错误" : ((result.RuleType == RiskCheckRule.RULE_TYPE_FORMULA) ? "公式" : "条件"), styleBorder);
				xLSheet[num, 2].SetValue(result.AccountCode ?? "", styleBorder);
				xLSheet[num, 3].SetValue(result.AccountName ?? "", styleBorder);
				xLSheet[num, 4].SetValue(result.IsError ? (result.ErrorMessage ?? "") : (result.HitDescription ?? ""), styleBorder);
				if (result.IsError)
				{
					xLSheet[num, 5].SetValue("", styleBorder);
				}
				else
				{
					xLSheet[num, 5].SetValue(EmptyIf0(result.Amount), styleMoney);
				}
				xLSheet[num, 6].SetValue(result.AuxDetail ?? "", styleBorder);
				xLSheet[num, 7].SetValue(result.VoucherInfo ?? "", styleBorder);
			}
			foreach (XLRow item in (IEnumerable)xLSheet.Rows)
			{
				item.Height = C1XLBook.PixelsToTwips(30.0);
			}
		}
	}
}
