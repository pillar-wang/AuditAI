﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Tile;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.LocalDataStore;
using Leqisoft.UI.Platform.Properties;
// 消除 TreeNode 二义性
using FormsTreeNode = System.Windows.Forms.TreeNode;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 跨项目数据引用配置向导（参照"采账填充"界面风格）
/// </summary>
public class frmCrossProjectRefConfigWizard : Form  // 与 frmTableCollect2 一致
{
    // 缓存的常用字体（避免重复创建 GDI 资源）
    private static readonly Font _fontStepTitle = new Font("Microsoft YaHei", 12f, FontStyle.Bold);
    private static readonly Font _fontNormal = new Font("Microsoft YaHei", 9f);
    private static readonly Font _fontStatus = new Font("Microsoft YaHei", 10f);
    private static readonly Font _fontHint = new Font("Microsoft YaHei", 8.5f);

    private readonly Leqisoft.Model.Project _currentProject;
    private readonly CrossProjectDataRefStore _store;
    private readonly CrossProjectRefAuthProvider _authProvider;
    private CrossProjectDataRef _editingRef;

    // 步骤指示器
    private Panel _pnlStepIndicator;
    private Label[] _lblSteps;

    // 主区域 - C1SplitContainer
    private C1SplitContainer _splitMain;
    private Panel _pnlContent;
    private Panel _pnlButtons;

    // 步骤面板
    private Panel _pnlStep1; // 选择来源项目
    private Panel _pnlStep2; // 选择来源表格
    private Panel _pnlStep3; // 选择填充区域
    private Panel _pnlStep4; // 选择数据源范围
    private Panel _pnlSummary; // 配置摘要

    // 按钮
    private C1Button _btnPrev;
    private C1Button _btnNext;
    private C1Button _btnFinish;
    private C1Button _btnCancel;

    // 选中的配置
    private Guid _selectedProjectId;
    private Id64 _selectedSourceTableId;
    private Id64 _selectedTargetTableId;
    private string _selectedProjectName;
    private string _selectedSourceTableName;
    private string _selectedTargetTableName;
    private C1TileControlEx _tileProjects;
    private C1FlexGridEx _gridTables;  // 树形表格选择器（仿 frmNodeSelector）
    private RefMode _selectedRefMode;
    private string _refConfigJson;
    private Func<bool> _stepValidate; // 当前步骤的验证+生成RefConfig委托

    // 缓存的表格对象（用于查询 Cell ID 等）
    private Leqisoft.Model.Table _targetTable;
    private Leqisoft.Model.Table _sourceTable;

    // Step3/Step4 选择状态（跨步骤传递）
    private int _tgtCol = -1, _tgtRow = -1, _tgtEndCol = -1, _tgtEndRow = -1;
    private string _tgtColName = "";  // 目标列名（跨步骤传递）
    private int _srcCol = -1, _srcRow = -1, _srcEndCol = -1, _srcEndRow = -1;
    private List<int> _formulaCheckedIndices = new List<int>();
    private List<string> _formulaCheckedNames = new List<string>();
    private string _formulaExpr = "";
    private string _refName;
    private bool _autoRefresh = true;
    private int _cacheDuration = 60;
    private List<Leqisoft.DTO.Project> _allProjects = new List<Leqisoft.DTO.Project>();
    private Template _projectTileTemplate;
    private string _defaultValue = "";

    private int _currentStep = 1;
    private const int TOTAL_STEPS = 4;

    public frmCrossProjectRefConfigWizard(Leqisoft.Model.Project currentProject, Id64 targetTableId, CrossProjectDataRef editingRef = null)
    {
        _currentProject = currentProject ?? throw new ArgumentNullException(nameof(currentProject));
        _selectedTargetTableId = targetTableId;
        _store = new CrossProjectDataRefStore(currentProject);
        _authProvider = new CrossProjectRefAuthProvider(currentProject);
        _editingRef = editingRef;

        var targetTableNode = currentProject.GetTableById(targetTableId);
        if (targetTableNode != null)
            _selectedTargetTableName = targetTableNode.TreeNode.Number + " " + targetTableNode.TreeNode.Name;

        InitializeComponent();
        this.Load += async (s, e) =>
        {
            await LoadProjectsAsync();
            if (!IsDisposed) ShowStep(1);
        };
    }

    private void InitializeComponent()
    {
        this.Text = _editingRef != null ? "编辑跨项目数据引用" : "新增跨项目数据引用";
        this.Size = new Size(1100, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = _fontNormal;
        this.MinimumSize = new Size(1000, 700);

        // 顶部步骤指示器
        _pnlStepIndicator = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(240, 240, 240) };
        _lblSteps = new Label[TOTAL_STEPS];
        string[] stepNames = { "选择项目", "选择表格", "填充区域", "数据源范围" };
        for (int i = 0; i < TOTAL_STEPS; i++)
        {
            _lblSteps[i] = new Label
            {
                Text = $"Step {i + 1}\n{stepNames[i]}",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(140, 52),
                Location = new Point(30 + i * 165, 9),
                Font = _fontStatus,
                BackColor = Color.LightGray,
                ForeColor = Color.White
            };
            _pnlStepIndicator.Controls.Add(_lblSteps[i]);
        }
        this.Controls.Add(_pnlStepIndicator);

        // 步骤指示器与内容区之间的分隔线
        var _pnlSeparator = new Panel { Dock = DockStyle.Top, Height = 2, BackColor = Color.FromArgb(200, 200, 200) };
        this.Controls.Add(_pnlSeparator);

        // 主内容区
        _pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        this.Controls.Add(_pnlContent);

        // 底部按钮区（参照"采账填充"风格）
        _pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(240, 240, 240) };
        _btnPrev = new C1Button { Text = "上一步", Location = new Point(10, 10), Size = new Size(90, 30) };
        _btnPrev.Click += (s, e) => ShowStep(_currentStep - 1);
        _btnNext = new C1Button { Text = "下一步", Location = new Point(110, 10), Size = new Size(90, 30) };
        _btnNext.Click += (s, e) =>
        {
            // Step3/Step4 需要先验证选择再进入下一步
            if ((_currentStep == 3 || _currentStep == 4) && _stepValidate != null)
            {
                if (!_stepValidate())
                    return;
            }
            ShowStep(_currentStep + 1);
        };
        _btnFinish = new C1Button { Text = "完成", Location = new Point(210, 10), Size = new Size(90, 30), Visible = false };
        _btnFinish.Click += BtnFinish_Click;
        _btnCancel = new C1Button { Text = "取消", Location = new Point(310, 10), Size = new Size(90, 30), DialogResult = DialogResult.Cancel };

        _pnlButtons.Controls.AddRange(new Control[] { _btnPrev, _btnNext, _btnFinish, _btnCancel });
        this.Controls.Add(_pnlButtons);

        this.CancelButton = _btnCancel;
    }

    private void ShowStep(int step)
    {
        _currentStep = Math.Max(1, Math.Min(step, TOTAL_STEPS));
        // 先 Dispose 所有子控件，释放 GDI 资源（Font/C1FlexGrid 等）
        foreach (Control c in _pnlContent.Controls)
        {
            c.Dispose();
        }
        _pnlContent.Controls.Clear();
        _tileProjects = null;
        _gridTables = null;
        _btnNext.Tag = null; // 清除上一步的验证逻辑
        _stepValidate = null;

        // 更新步骤指示器
        for (int i = 0; i < TOTAL_STEPS; i++)
        {
            _lblSteps[i].BackColor = i + 1 == _currentStep ? Color.FromArgb(0, 120, 215) :
                                     i + 1 < _currentStep ? Color.FromArgb(0, 180, 80) : Color.LightGray;
        }

        _btnPrev.Visible = _currentStep > 1;
        _btnNext.Visible = _currentStep < TOTAL_STEPS;
        _btnFinish.Visible = _currentStep == TOTAL_STEPS;

        switch (_currentStep)
        {
            case 1: ShowStep1(); break;
            case 2: ShowStep2(); break;
            case 3: ShowStep3(); break;
            case 4: ShowStep4(); break;
        }
    }

    private void ShowStep1()
    {
        // Step 1: 选择来源项目 — 仿 FormProjectManage 磁贴模式
        var lbl = new Label { Text = "选择来源项目", Font = _fontStepTitle, Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(5, 0, 0, 5) };
        _pnlContent.Controls.Add(lbl);

        // 模板：参照 FormProjectManage.CreateTileTemplate
        // 结构：顶部图标(Image1) + 底部文字(Text)
        var template = new Template();
        template.Description = "Win32";
        template.Name = "mapImgTemplate";

        // 顶部：项目图标（与 FormProjectManage 一致：50x50, Margin 30）
        var iconPanel = new PanelElement();
        iconPanel.Alignment = ContentAlignment.TopCenter;
        var imgElement = new ImageElement();
        imgElement.AlignmentOfContents = ContentAlignment.TopCenter;
        imgElement.FixedHeight = 50;
        imgElement.FixedWidth = 50;
        imgElement.ImageSelector = ImageSelector.Image1;
        iconPanel.Children.Add(imgElement);
        iconPanel.FixedHeight = 50;
        iconPanel.FixedWidth = 50;
        iconPanel.Margin = new Padding(0, 30, 0, 0);

        // 底部：项目名称（与 FormProjectManage 一致：150x50）
        var textPanel = new PanelElement();
        textPanel.Alignment = ContentAlignment.BottomCenter;
        var nameText = new TextElement();
        nameText.AlignmentOfContents = ContentAlignment.TopCenter;
        nameText.TextTrimming = TextTrimming.EndEllipsis;
        nameText.SingleLine = false;
        nameText.FixedHeight = 50;
        nameText.FixedWidth = 150;
        textPanel.Children.Add(nameText);
        textPanel.FixedHeight = 50;
        textPanel.FixedWidth = 150;

        template.Elements.Add(iconPanel);
        template.Elements.Add(textPanel);

        // C1TileControlEx — 不覆盖默认的 SurfacePadding/GroupPadding（构造函数已设默认值）
        // 默认 SurfacePadding = (15, 10, 15, 5)，默认 GroupPadding = (0, 40, 0, 0)
        _tileProjects = new C1TileControlEx
        {
            Dock = DockStyle.Fill,
            CellWidth = 20,
            CellHeight = 15,
            CellSpacing = 20,
            TileBackColor = Color.Transparent
        };
        _tileProjects.Templates.Add(template);
        _projectTileTemplate = template;

        Theme.SetCurrentTree(_tileProjects);
        _tileProjects.TileBorderColor = Color.Transparent;
        _tileProjects.CustomBorderColor = Theme.SelectedLeqiTheme.ThemeContext.DarkColor;

        // 搜索过滤
        var pnlSearch = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(10, 6, 10, 6)
        };
        var txtSearch = new TextBox
        {
            Width = 250,
            Anchor = AnchorStyles.Left
        };
        var lblSearch = new Label
        {
            Text = "搜索:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };
        lblSearch.Location = new Point(0, 3);
        txtSearch.Location = new Point(lblSearch.Right + 8, 0);
        pnlSearch.Controls.Add(lblSearch);
        pnlSearch.Controls.Add(txtSearch);

        // 构建 Tile 的方法
        txtSearch.TextChanged += (s, e) => BuildTiles(txtSearch.Text);

        // Tile 双击选择 → 进入 Step2
        _tileProjects.DoubleClickTile += (s, e) =>
        {
            if (e?.Tag is Guid projectId)
            {
                _selectedProjectId = projectId;
                _selectedProjectName = e.Text ?? "";
                ShowStep(2);
            }
        };

        // 添加顺序：先 Bottom(Top)，再 Fill，最后 Top（Dock 布局从后往前）
        _pnlContent.Controls.Add(pnlSearch);
        _pnlContent.Controls.Add(_tileProjects);
        _pnlContent.Controls.Add(lbl);

        // 如果项目列表已加载（LoadProjectsAsync 在 ShowStep 之前完成），填充 Tile
        if (_allProjects != null)
            BuildTiles(null);
    }

    private void ShowStep2()
    {
        // Step 2: 选择来源表格 — 仿 frmNodeSelector 树形结构（C1FlexGridEx Tree 模式）
        var lbl = new Label { Text = "选择来源表格", Font = _fontStepTitle, Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(5, 0, 0, 5) };

        // ---- 搜索框（与 Step1 一致，使用 Bottom 布局）----
        var pnlSearch = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(10, 6, 10, 6)
        };
        var btnSearch = new C1Button { Text = "筛选", Dock = DockStyle.Right, Size = new Size(50, 28) };
        var txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = _fontStatus,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 4, 0)
        };
        pnlSearch.Controls.Add(txtSearch);
        pnlSearch.Controls.Add(btnSearch);

        // 包一层容器给 C1FlexGridEx 顶部留间距（C1FlexGridEx 不自带 Padding 边距）
        var pnlGridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 5, 0, 0)
        };

        // C1FlexGridEx 树形选择器（仿 frmNodeSelector.Initialize + Populate）
        _gridTables = new C1FlexGridEx
        {
            Dock = DockStyle.Fill,
            AllowEditing = false,
            AllowSorting = AllowSortingEnum.None,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
            Rows = { DefaultSize = 30 },
            Styles = { Normal = { Border = { Width = 0 } } },
            ExtendLastCol = true
        };
        _gridTables.Tree.Style = TreeStyleFlags.Simple;
        _gridTables.Tree.Column = 0;
        _gridTables.Rows.Count = 1;
        _gridTables.Rows.Fixed = 1;
        _gridTables.Cols.Count = 0;
        _gridTables.Cols.Fixed = 0;

        var colNode = _gridTables.Cols.Add();
        colNode.Caption = "项目文件";
        colNode.Name = "node";
        colNode.AllowEditing = false;
        colNode.TextAlign = TextAlignEnum.LeftCenter;

        // 白背景（不调用 Theme.SetCurrentTree，避免灰色主题覆盖 BackColor）
        _gridTables.BackColor = Color.White;
        _gridTables.Styles.Normal.BackColor = Color.White;
        _gridTables.Styles.Alternate.BackColor = Color.White;
        _gridTables.Styles.EmptyArea.BackColor = Color.White;

        // 加载树（仿 frmNodeSelector.Populate + ReferenceEditor 加载外部项目）
        _gridTables.BeginUpdate();
        try
        {
            _gridTables.Rows.Count = _gridTables.Rows.Fixed;

            // 加载来源项目（仿 ReferenceEditor.cs:552-575）
            Leqisoft.Model.Project project = null;
            if (_selectedProjectId != Guid.Empty)
            {
                try
                {
                    string dbPath = MainForm.GetDbPathByGuid(_selectedProjectId);
                    var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
                    var dto = dal.GetProject();
                    if (dto != null)
                    {
                        project = new Leqisoft.Model.Project
                        {
                            Id = _selectedProjectId,
                            Name = dto.Name,
                            Dal = dal
                        };
                        project.PopulateFieldsFromDto(dto);
                        project.Name = dto.Name;
                        project.Load();
                    }
                }
                catch (Exception ex) { ex.Log(); }
            }
            if (project == null)
                project = _currentProject;

            if (project?.TreeGroups != null)
            {
                foreach (var treeGroup in project.TreeGroups)
                {
                    var groupNode = _gridTables.Rows.AddNode(0);
                    groupNode.Data = treeGroup.Name;
                    groupNode.Image = ContextResources.TreeGroup;

                    foreach (var rootNode in treeGroup.RootNodes)
                    {
                        AddTreeTableNode(groupNode, rootNode);
                    }
                }
            }
            _gridTables.Cols["node"].TextAlign = TextAlignEnum.LeftCenter;
            _gridTables.ExpandAll();
        }
        catch (Exception ex)
        {
            ex.Log();
        }
        finally
        {
            _gridTables.EndUpdate();
        }

        // 选中表格节点（使用 MouseClick 获取准确的点击位置）
        _gridTables.MouseClick += (s, e) =>
        {
            var hit = _gridTables.HitTest(e.Location);
            if (hit.Row >= _gridTables.Rows.Fixed && hit.Row < _gridTables.Rows.Count)
            {
                var userData = _gridTables.Rows[hit.Row].UserData;
                if (userData is TreeTableNode tableNode)
                {
                    _selectedSourceTableId = tableNode.Id;
                    _selectedSourceTableName = tableNode.Number + " " + tableNode.Name;
                }
            }
        };

        // 双击表格节点 → 进入 Step3（使用 MouseDoubleClick 获取准确的点击位置）
        _gridTables.MouseDoubleClick += (s, e) =>
        {
            var hit = _gridTables.HitTest(e.Location);
            if (hit.Row >= _gridTables.Rows.Fixed && hit.Row < _gridTables.Rows.Count)
            {
                var userData = _gridTables.Rows[hit.Row].UserData;
                if (userData is TreeTableNode)
                {
                    ShowStep(3);
                }
            }
        };

        pnlGridContainer.Controls.Add(_gridTables);

        // ---- 搜索过滤逻辑 ----
        Action<string> filterTree = (keyword) =>
        {
            keyword = (keyword ?? "").Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                // 恢复所有节点
                _gridTables.BeginUpdate();
                for (int i = _gridTables.Rows.Fixed; i < _gridTables.Rows.Count; i++)
                {
                    _gridTables.Rows[i].Visible = true;
                    if (_gridTables.Rows[i].IsNode) _gridTables.Rows[i].Node.Collapsed = false;
                }
                _gridTables.EndUpdate();
                return;
            }

            _gridTables.BeginUpdate();
            try
            {
                // 先折叠所有
                for (int i = _gridTables.Rows.Fixed; i < _gridTables.Rows.Count; i++)
                {
                    _gridTables.Rows[i].Visible = false;
                }

                // 显示匹配的节点及其父节点
                for (int i = _gridTables.Rows.Fixed; i < _gridTables.Rows.Count; i++)
                {
                    var row = _gridTables.Rows[i];
                    string text = (_gridTables[i, 0]?.ToString() ?? "").ToLower();
                    if (text.Contains(keyword))
                    {
                        row.Visible = true;
                        // 显示父节点
                        var parent = row.Node?.Parent;
                        while (parent != null)
                        {
                            parent.Row.Visible = true;
                            parent.Collapsed = false;
                            parent = parent.Parent;
                        }
                    }
                }
            }
            finally
            {
                _gridTables.EndUpdate();
            }
        };

        txtSearch.TextChanged += (s, e) => filterTree(txtSearch.Text);
        btnSearch.Click += (s, e) => filterTree(txtSearch.Text);

        // 添加顺序与 Step1 一致：先 pnlSearch(Bottom)，再 Fill，最后 lbl(Top)
        _pnlContent.Controls.Add(pnlSearch);        // Bottom（搜索框）
        _pnlContent.Controls.Add(pnlGridContainer); // Fill（表格容器）
        _pnlContent.Controls.Add(lbl);              // Top（标题）
    }

    // 递归添加树节点到 C1FlexGridEx（仿 frmNodeSelector.Populate.AddDirectoryNode）
    private void AddTreeTableNode(Node parentNode, TreeNodeBase treeNode)
    {
        if (treeNode is TreeTableNode tableNode)
        {
            parentNode.AddNode(NodeTypeEnum.LastChild, tableNode.Number + " " + tableNode.Name, tableNode, Resources.TreeTable);
        }
        else if (treeNode is TreeDirectoryNode dirNode)
        {
            var childNode = parentNode.AddNode(NodeTypeEnum.LastChild, dirNode.Number + " " + dirNode.Name, dirNode, Resources.TreeDir);
            if (dirNode.Children != null)
            {
                foreach (var child in dirNode.Children)
                {
                    AddTreeTableNode(childNode, child);
                }
            }
        }
        else if (treeNode is TreeDocumentNode docNode)
        {
            parentNode.AddNode(NodeTypeEnum.LastChild, docNode.Number + " " + docNode.Name, docNode, Resources.TreeDoc);
        }
    }

    private void ShowStep3()
    {
        try
        {
            var lbl = new Label { Text = "选择填充区域", Font = _fontStepTitle, Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(5, 0, 0, 5) };

            // ---- 填充方式选择 ----
            var pnlMode = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            var lblModeHint = new Label { Text = "填充方式：", Location = new Point(10, 12), AutoSize = true };
            var rbCell = new RadioButton { Text = "单元格", Location = new Point(80, 10), AutoSize = true, Checked = _selectedRefMode == RefMode.CellRef };
            var rbColumn = new RadioButton { Text = "整列", Location = new Point(160, 10), AutoSize = true, Checked = _selectedRefMode == RefMode.ColumnRef };
            var rbArea = new RadioButton { Text = "区域", Location = new Point(230, 10), AutoSize = true, Checked = _selectedRefMode == RefMode.AreaRef };
            var rbFormula = new RadioButton { Text = "公式运算", Location = new Point(300, 10), AutoSize = true, Checked = _selectedRefMode == RefMode.FormulaCompute };
            pnlMode.Controls.AddRange(new Control[] { lblModeHint, rbCell, rbColumn, rbArea, rbFormula });

            // ---- 状态提示 ----
            var pnlStatus = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.FromArgb(245, 245, 245) };
            var lblStatus = new Label
            {
                Text = "在下方表格中拖拽或 Shift+点击 选择目标位置",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            pnlStatus.Controls.Add(lblStatus);

            // ---- 目标表网格（用 LightweightTableEditor，模仿 TableEditor 风格） ----
            var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 0), BackColor = Color.FromArgb(235, 243, 252) };
            var gridTarget = new LightweightTableEditor
            {
                Dock = DockStyle.Fill,
                HighlightColor = Color.FromArgb(0, 120, 215),  // 蓝色高亮（目标表）
                FocusColor = Color.FromArgb(200, 230, 255),
                BackColor = Color.FromArgb(245, 250, 255),      // 浅蓝背景
                CellBackColor = Color.FromArgb(245, 250, 255)   // 浅蓝单元格底色
            };
            LoadTargetTableStructure(gridTarget);
            pnlGrid.Controls.Add(gridTarget);

            // ---- 模式切换 ----
            Action<RefMode> switchMode = (mode) =>
            {
                _selectedRefMode = mode;
                _tgtCol = _tgtRow = _tgtEndCol = _tgtEndRow = -1;
                switch (mode)
                {
                    case RefMode.CellRef:
                        gridTarget.SelectionMode = SelectionModeEnum.Default;
                        lblStatus.Text = "点击选择目标单元格";
                        break;
                    case RefMode.ColumnRef:
                        gridTarget.SelectionMode = SelectionModeEnum.Column;
                        lblStatus.Text = "点击选择目标列";
                        break;
                    case RefMode.AreaRef:
                        gridTarget.SelectionMode = SelectionModeEnum.Default;
                        lblStatus.Text = "拖拽或 Shift+点击 选择目标区域";
                        break;
                    case RefMode.FormulaCompute:
                        gridTarget.SelectionMode = SelectionModeEnum.Column;
                        lblStatus.Text = "点击选择公式结果填入的目标列";
                        break;
                }
                lblStatus.ForeColor = Color.FromArgb(100, 100, 100);
            };
            rbCell.CheckedChanged += (s, e) => { if (rbCell.Checked) switchMode(RefMode.CellRef); };
            rbColumn.CheckedChanged += (s, e) => { if (rbColumn.Checked) switchMode(RefMode.ColumnRef); };
            rbArea.CheckedChanged += (s, e) => { if (rbArea.Checked) switchMode(RefMode.AreaRef); };
            rbFormula.CheckedChanged += (s, e) => { if (rbFormula.Checked) switchMode(RefMode.FormulaCompute); };

            // 始终调用 switchMode 以确保选择模式和状态栏正确初始化
            // （default(RefMode) == CellRef，用 != default 会在 CellRef 模式下漏掉初始化）
            switchMode(_selectedRefMode);

            // ---- 用 SelectionChanged 读取选择区域 ----
            // 直接使用 body 坐标（0-based），与原表格行列索引一致
            gridTarget.SelectionChanged += (s, args) =>
            {
                if (args.StartCol < 0) return;

                // 检查目标单元格是否包含公式（禁止选择有公式的单元格）
                if (_targetTable != null)
                {
                    bool hasFormula = false;
                    for (int r = args.StartRow; r <= args.EndRow && !hasFormula; r++)
                    {
                        for (int c = args.StartCol; c <= args.EndCol && !hasFormula; c++)
                        {
                            if (r < _targetTable.Rows.Count && c < _targetTable.Columns.Count)
                            {
                                var cell = _targetTable[r, c];
                                if (cell != null && (cell.HasFormula || cell.HasColumnFormula()))
                                    hasFormula = true;
                            }
                        }
                    }
                    if (hasFormula)
                    {
                        Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "选中的单元格包含公式，不能作为跨项目引用的目标。\n请选择不含公式的单元格。");
                        _tgtCol = -1;
                        lblStatus.Text = "请重新选择目标位置（不含公式的单元格）";
                        lblStatus.ForeColor = Color.Red;
                        return;
                    }
                }

                // 如果用户在"单元格"模式下拖拽选了区域，自动切换到"区域"模式
                // 注意：必须在设置坐标之前切换，因为 switchMode 会重置坐标
                if (_selectedRefMode == RefMode.CellRef &&
                    (args.StartRow != args.EndRow || args.StartCol != args.EndCol))
                {
                    _selectedRefMode = RefMode.AreaRef;
                    rbCell.Checked = false;
                    rbArea.Checked = true;  // 触发 switchMode，会重置 _tgtCol 等
                }

                // 在自动切换后再设置坐标（避免被 switchMode 重置）
                _tgtRow = args.StartRow;
                _tgtEndRow = args.EndRow;
                _tgtCol = args.StartCol;
                _tgtEndCol = args.EndCol;
                _tgtColName = args.ColName;

                switch (_selectedRefMode)
                {
                    case RefMode.CellRef:
                        lblStatus.Text = $"已选目标：{FormatRange(args.StartRow, args.StartCol, args.EndRow, args.EndCol)}（{args.ColName}）";
                        break;
                    case RefMode.ColumnRef:
                    case RefMode.FormulaCompute:
                        lblStatus.Text = $"已选目标列：{args.ColName}（{ToColumnLetter(args.StartCol)}列）";
                        break;
                    case RefMode.AreaRef:
                        if (args.StartRow == args.EndRow && args.StartCol == args.EndCol)
                            lblStatus.Text = $"已选起点：{FormatRange(args.StartRow, args.StartCol, args.EndRow, args.EndCol)}（拖拽或Shift+点击选终点）";
                        else
                            lblStatus.Text = $"已选目标区域：{FormatRange(args.StartRow, args.StartCol, args.EndRow, args.EndCol)}（{args.EndCol - args.StartCol + 1}列×{args.EndRow - args.StartRow + 1}行）";
                        break;
                }
                lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
            };

            // ---- 验证 ----
            _stepValidate = new Func<bool>(() =>
            {
                if (_tgtCol < 0)
                {
                    Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请先在表格中选择目标位置");
                    return false;
                }
                return true;
            });

            _pnlContent.Controls.Add(pnlGrid);
            _pnlContent.Controls.Add(pnlStatus);
            _pnlContent.Controls.Add(pnlMode);
            _pnlContent.Controls.Add(lbl);
        }
        catch (Exception ex)
        {
            ex.Log();
            var lblError = new Label { Text = $"加载失败：{ex.Message}", Dock = DockStyle.Fill, ForeColor = Color.Red, Padding = new Padding(10) };
            _pnlContent.Controls.Add(lblError);
        }
    }

    private void ShowStep4()
    {
        try
        {
            var lbl = new Label { Text = "选择数据源范围", Font = _fontStepTitle, Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(5, 0, 0, 5) };

            // ---- 状态提示 ----
            var pnlStatus = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.FromArgb(245, 245, 245) };
            var lblStatus = new Label
            {
                Text = _selectedRefMode == RefMode.FormulaCompute ? "勾选参与运算的来源列，并输入公式" : "在下方表格中点击选择数据源范围",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            pnlStatus.Controls.Add(lblStatus);

            // ---- 公式模式：勾选列 + 公式输入 ----
            if (_selectedRefMode == RefMode.FormulaCompute)
            {
                var pnlFormula = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.White, Padding = new Padding(10, 8, 10, 8) };
                var lblColHint = new Label { Text = "参与运算的来源列：", Location = new Point(5, 5), AutoSize = true };
                var chkListCols = new CheckedListBox { Location = new Point(5, 25), Size = new Size(350, 55), CheckOnClick = true };
                var lblExpr = new Label { Text = "公式：", Location = new Point(5, 85), AutoSize = true };
                var txtExpr = new TextBox { Location = new Point(60, 82), Width = 290, Text = _formulaExpr };
                var lblHint = new Label { Text = "用 {0}=第1列, {1}=第2列... 如 {0}+{1}", Location = new Point(60, 105), AutoSize = true, ForeColor = Color.Gray, Font = _fontHint };
                pnlFormula.Controls.AddRange(new Control[] { lblColHint, chkListCols, lblExpr, txtExpr, lblHint });

                // ---- 来源表预览（用 LightweightTableEditor，只读） ----
                var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 0), BackColor = Color.FromArgb(235, 245, 238) };
                var gridSource = new LightweightTableEditor
                {
                    Dock = DockStyle.Fill,
                    HighlightColor = Color.FromArgb(0, 120, 60),
                    FocusColor = Color.FromArgb(200, 255, 200),
                    BackColor = Color.FromArgb(245, 252, 248),      // 浅绿背景
                    CellBackColor = Color.FromArgb(245, 252, 248)   // 浅绿单元格底色
                };
                LoadSourceTableData(gridSource);
                pnlGrid.Controls.Add(gridSource);

                // 从 gridSource 获取列名填充勾选列表
                chkListCols.Items.Clear();
                for (int c = 0; c < gridSource.BodyColsCount; c++)
                {
                    string colName = gridSource.GetColumnName(c);
                    bool isChecked = _formulaCheckedIndices.Contains(c);
                    chkListCols.Items.Add(colName, isChecked);
                }

                // 勾选变更更新状态
                chkListCols.ItemCheck += (s, e) =>
                {
                    BeginInvoke((Action)(() =>
                    {
                        // 步骤切换后控件可能已被 Dispose，需检查
                        if (chkListCols.IsDisposed || lblStatus.IsDisposed) return;

                        _formulaCheckedIndices.Clear();
                        _formulaCheckedNames.Clear();
                        for (int i = 0; i < chkListCols.CheckedItems.Count; i++)
                        {
                            int idx = chkListCols.CheckedIndices[i];
                            _formulaCheckedIndices.Add(idx);
                            _formulaCheckedNames.Add(chkListCols.CheckedItems[i]?.ToString() ?? $"列{idx + 1}");
                        }
                        if (_formulaCheckedIndices.Count > 0)
                        {
                            lblStatus.Text = $"已选 {_formulaCheckedIndices.Count} 列：{string.Join(", ", _formulaCheckedNames)}";
                            lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
                        }
                        else
                        {
                            lblStatus.Text = "请勾选参与运算的来源列";
                            lblStatus.ForeColor = Color.FromArgb(100, 100, 100);
                        }
                    }));
                };

                // 恢复状态
                if (_formulaCheckedIndices.Count > 0)
                {
                    lblStatus.Text = $"已选 {_formulaCheckedIndices.Count} 列：{string.Join(", ", _formulaCheckedNames)}";
                    lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
                }

                // 验证
                _stepValidate = new Func<bool>(() =>
                {
                    _formulaExpr = txtExpr.Text.Trim();
                    if (_formulaCheckedIndices.Count == 0)
                    {
                        Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请勾选参与运算的来源列");
                        return false;
                    }
                    if (string.IsNullOrEmpty(_formulaExpr))
                    {
                        Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请输入公式表达式");
                        return false;
                    }
                    // 保存配置
                    // _tgtCol 已是 body 坐标（0-based），与原表格一致
                    _refConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        Mode = "FormulaCompute",
                        TargetColumnIndex = _tgtCol,
                        TargetColumnName = _tgtColName,
                        SourceColumnIndices = _formulaCheckedIndices,
                        SourceColumnNames = _formulaCheckedNames,
                        FormulaExpression = _formulaExpr
                    });
                    return true;
                });

                _pnlContent.Controls.Add(pnlGrid);
                _pnlContent.Controls.Add(pnlFormula);
                _pnlContent.Controls.Add(pnlStatus);
                _pnlContent.Controls.Add(lbl);
                return;
            }

            // ---- 非公式模式：来源表用 LightweightTableEditor 原生选择 ----
            var pnlGrid2 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 0), BackColor = Color.FromArgb(235, 245, 238) };
            var gridSource2 = new LightweightTableEditor
            {
                Dock = DockStyle.Fill,
                HighlightColor = Color.FromArgb(0, 120, 60),   // 绿色高亮（来源表）
                FocusColor = Color.FromArgb(200, 255, 200),
                BackColor = Color.FromArgb(245, 252, 248),      // 浅绿背景
                CellBackColor = Color.FromArgb(245, 252, 248)   // 浅绿单元格底色
            };
            LoadSourceTableData(gridSource2);
            pnlGrid2.Controls.Add(gridSource2);

            // 根据模式设置选择行为
            switch (_selectedRefMode)
            {
                case RefMode.ColumnRef:
                    gridSource2.SelectionMode = SelectionModeEnum.Column;
                    break;
                default:
                    gridSource2.SelectionMode = SelectionModeEnum.Default;
                    break;
            }

            // ---- 用 SelectionChanged 读取选择区域 ----
            // 直接使用 body 坐标（0-based），与原表格行列索引一致
            gridSource2.SelectionChanged += (s, args) =>
            {
                if (args.StartCol < 0) return;

                _srcRow = args.StartRow;
                _srcEndRow = args.EndRow;
                _srcCol = args.StartCol;
                _srcEndCol = args.EndCol;

                switch (_selectedRefMode)
                {
                    case RefMode.CellRef:
                        lblStatus.Text = $"已选来源：{FormatRange(args.StartRow, args.StartCol, args.EndRow, args.EndCol)}（{args.ColName}）";
                        break;
                    case RefMode.ColumnRef:
                        lblStatus.Text = $"已选来源列：{args.ColName}（{ToColumnLetter(args.StartCol)}列）";
                        break;
                    case RefMode.AreaRef:
                        if (args.StartRow == args.EndRow && args.StartCol == args.EndCol)
                        {
                            lblStatus.Text = $"已选起点：{FormatRange(args.StartRow, args.StartCol, args.EndRow, args.EndCol)}（拖拽或Shift+点击选终点）";
                        }
                        else
                        {
                            // 区域模式：实时检查与 Step3 目标区域的尺寸匹配
                            int srcRows = args.EndRow - args.StartRow + 1;
                            int srcCols = args.EndCol - args.StartCol + 1;
                            int tgtRows = Math.Abs(_tgtEndRow - _tgtRow) + 1;
                            int tgtCols = Math.Abs(_tgtEndCol - _tgtCol) + 1;
                            string srcRange = FormatRange(args.StartRow, args.StartCol, args.EndRow, args.EndCol);
                            string tgtRange = FormatRange(
                                Math.Min(_tgtRow, _tgtEndRow), Math.Min(_tgtCol, _tgtEndCol),
                                Math.Max(_tgtRow, _tgtEndRow), Math.Max(_tgtCol, _tgtEndCol));

                            if (srcRows == tgtRows && srcCols == tgtCols)
                            {
                                lblStatus.Text = $"已选来源区域：{srcRange}（{srcCols}列×{srcRows}行）✓ 与目标区域 {tgtRange} 尺寸一致";
                                lblStatus.ForeColor = Color.FromArgb(0, 120, 60);  // 绿色：匹配
                            }
                            else
                            {
                                lblStatus.Text = $"已选来源区域：{srcRange}（{srcCols}列×{srcRows}行）✗ 与目标区域 {tgtRange}（{tgtCols}列×{tgtRows}行）尺寸不匹配";
                                lblStatus.ForeColor = Color.FromArgb(200, 30, 30);  // 红色：不匹配
                            }
                        }
                        break;
                }
                // 非 AreaRef 不匹配警告时，恢复蓝色
                if (_selectedRefMode != RefMode.AreaRef ||
                    (args.StartRow == args.EndRow && args.StartCol == args.EndCol))
                {
                    lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
                }
            };

            // 验证并保存
            _stepValidate = new Func<bool>(() =>
            {
                if (_srcCol < 0)
                {
                    Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请先选择数据源范围");
                    return false;
                }

                // 所有坐标均为 body 坐标（0-based），与原表格一致
                switch (_selectedRefMode)
                {
                    case RefMode.CellRef:
                        var tgtCell = _targetTable?[_tgtRow, _tgtCol];
                        var srcCell = _sourceTable?[_srcRow, _srcCol];
                        if (tgtCell == null || tgtCell.Id.Value <= 0)
                        {
                            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "无法获取目标单元格信息，请重试");
                            return false;
                        }
                        if (srcCell == null || srcCell.Id.Value <= 0)
                        {
                            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "无法获取来源单元格信息，请重试");
                            return false;
                        }
                        _refConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            TargetCellId = tgtCell.Id.Value,
                            SourceCellId = srcCell.Id.Value,
                            TargetRow = _tgtRow,
                            TargetCol = _tgtCol,
                            SourceRow = _srcRow,
                            SourceCol = _srcCol
                        });
                        break;
                    case RefMode.ColumnRef:
                        _refConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
                        {
                            Mode = "ColumnRef",
                            TargetColumnIndex = _tgtCol,
                            TargetColumnName = _tgtColName,
                            SourceColumnIndex = _srcCol,
                            SourceColumnName = gridSource2.GetColumnName(_srcCol),
                            SourceTotalRows = gridSource2.BodyRowsCount
                        });
                        break;
                    case RefMode.AreaRef:
                        {
                            // 区域模式：检查目标区域与来源区域的尺寸是否匹配
                            int tgtRows = Math.Abs(_tgtEndRow - _tgtRow) + 1;
                            int tgtCols = Math.Abs(_tgtEndCol - _tgtCol) + 1;
                            int srcRows = Math.Abs(_srcEndRow - _srcRow) + 1;
                            int srcCols = Math.Abs(_srcEndCol - _srcCol) + 1;

                            if (srcRows != tgtRows || srcCols != tgtCols)
                            {
                                // 尺寸不匹配时弹出确认对话框，让用户决定是否继续
                                string tgtRange = FormatRange(
                                    Math.Min(_tgtRow, _tgtEndRow), Math.Min(_tgtCol, _tgtEndCol),
                                    Math.Max(_tgtRow, _tgtEndRow), Math.Max(_tgtCol, _tgtEndCol));
                                string srcRange = FormatRange(
                                    Math.Min(_srcRow, _srcEndRow), Math.Min(_srcCol, _srcEndCol),
                                    Math.Max(_srcRow, _srcEndRow), Math.Max(_srcCol, _srcEndCol));

                                string msg = $"目标区域 {tgtRange}（{tgtCols}列×{tgtRows}行）与来源区域 {srcRange}（{srcCols}列×{srcRows}行）尺寸不匹配。\r\n\r\n" +
                                             $"不匹配时将按以下规则填充：\r\n" +
                                             $"• 列数不足：多余的目标列保持不变\r\n" +
                                             $"• 列数多余：仅填充前 {Math.Min(tgtCols, srcCols)} 列\r\n" +
                                             $"• 行数不足：多余的目标行保持不变\r\n" +
                                             $"• 行数多余：仅填充前 {Math.Min(tgtRows, srcRows)} 行\r\n\r\n" +
                                             $"是否继续？";

                                var result = System.Windows.Forms.MessageBox.Show(msg, "区域尺寸不匹配",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (result != DialogResult.Yes)
                                {
                                    return false;
                                }
                            }

                            _refConfigJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
                            {
                                Mode = "AreaRef",
                                TargetStartCol = Math.Min(_tgtCol, _tgtEndCol),
                                TargetEndCol = Math.Max(_tgtCol, _tgtEndCol),
                                TargetStartRow = Math.Min(_tgtRow, _tgtEndRow),
                                TargetEndRow = Math.Max(_tgtRow, _tgtEndRow),
                                SourceStartCol = Math.Min(_srcCol, _srcEndCol),
                                SourceEndCol = Math.Max(_srcCol, _srcEndCol),
                                SourceStartRow = Math.Min(_srcRow, _srcEndRow),
                                SourceEndRow = Math.Max(_srcRow, _srcEndRow)
                            });
                            break;
                        }
                }
                return true;
            });

            _pnlContent.Controls.Add(pnlGrid2);
            _pnlContent.Controls.Add(pnlStatus);
            _pnlContent.Controls.Add(lbl);
        }
        catch (Exception ex)
        {
            ex.Log();
            var lblError = new Label { Text = $"加载失败：{ex.Message}", Dock = DockStyle.Fill, ForeColor = Color.Red, Padding = new Padding(10) };
            _pnlContent.Controls.Add(lblError);
        }
    }

    /// <summary>
    /// 加载来源表格数据到 LightweightTableEditor
    /// </summary>
    private void LoadSourceTableData(LightweightTableEditor editor)
    {
        try
        {
            string dbPath = MainForm.GetDbPathByGuid(_selectedProjectId);
            if (string.IsNullOrEmpty(dbPath) || !System.IO.File.Exists(dbPath))
            {
                editor.ShowError("未找到来源项目数据库");
                return;
            }

            var dal = new ProjectDAL(dbPath);
            var dto = dal.GetProject();
            if (dto == null)
            {
                editor.ShowError("无法读取来源项目");
                return;
            }

            var project = new Leqisoft.Model.Project { Id = dto.Id, Name = dto.Name };
            project.Dal = dal;
            project.PopulateFieldsFromDto(dto);
            project.Load();

            var table = project.GetTableById(_selectedSourceTableId);
            if (table == null)
            {
                editor.ShowError("未找到来源表格");
                return;
            }

            _sourceTable = table;
            LoadTableToEditor(editor, table, 200);
        }
        catch (Exception ex)
        {
            ex.Log();
            editor.ShowError($"加载失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 加载来源表格数据到网格
    /// </summary>
    private void LoadSourceTableData(C1FlexGrid grid)
    {
        try
        {
            // 使用与 Step2 一致的路径获取方式（MainForm.GetDbPathByGuid）
            string dbPath = MainForm.GetDbPathByGuid(_selectedProjectId);
            if (string.IsNullOrEmpty(dbPath) || !System.IO.File.Exists(dbPath))
            {
                grid[0, 0] = "（未找到来源项目数据库）";
                return;
            }

            var dal = new ProjectDAL(dbPath);
            var dto = dal.GetProject();
            if (dto == null)
            {
                grid[0, 0] = "（无法读取来源项目）";
                return;
            }

            var project = new Leqisoft.Model.Project { Id = dto.Id, Name = dto.Name };
            project.Dal = dal;
            project.PopulateFieldsFromDto(dto);
            project.Load();

            var table = project.GetTableById(_selectedSourceTableId);
            if (table == null)
            {
                grid[0, 0] = "（未找到来源表格）";
                return;
            }

            table.LoadAndReturn();

            // 填充列头
            var cols = table.Columns;
            grid.Cols.Count = cols.Count;
            for (int c = 0; c < cols.Count; c++)
            {
                grid[0, c] = cols[c].Caption ?? $"列{c + 1}";
                grid.Cols[c].Width = 100;
            }

            // 填充数据行
            int maxRows = Math.Min(200, table.Rows.Count);
            grid.Rows.Count = maxRows + 1;
            for (int r = 0; r < maxRows; r++)
            {
                for (int c = 0; c < cols.Count; c++)
                {
                    var cell = table[r, c];
                    grid[r + 1, c] = cell?.GetDisplayValue() ?? "";
                }
            }

            if (table.Rows.Count > 200)
            {
                if (grid.Rows.Count < 202)
                    grid.Rows.Count = 202;
                grid[201, 0] = $"... 共 {table.Rows.Count} 行，仅显示前200行";
            }

            grid.AutoSizeCols();
        }
        catch (Exception ex)
        {
            ex.Log();
            grid.Cols.Count = 1;
            grid[0, 0] = $"（加载失败：{ex.Message}）";
        }
    }

    /// <summary>
    /// 加载目标表格结构到 LightweightTableEditor
    /// </summary>
    private void LoadTargetTableStructure(LightweightTableEditor editor)
    {
        try
        {
            var table = _currentProject.GetTableById(_selectedTargetTableId);
            if (table == null)
            {
                editor.ShowError("未找到目标表格");
                return;
            }
            _targetTable = table;
            table.LoadAndReturn();
            LoadTableToEditor(editor, table, 50);
        }
        catch (Exception ex)
        {
            ex.Log();
            editor.ShowError($"加载失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 将 Model.Table 数据加载到 LightweightTableEditor（公共辅助方法）
    /// </summary>
    private static void LoadTableToEditor(LightweightTableEditor editor, Leqisoft.Model.Table table, int maxRows)
    {
        table.LoadAndReturn();
        editor.HideError();
        editor.Grid.BeginUpdate();
        try
        {
            var cols = table.Columns;
            if (cols == null || cols.Count == 0)
            {
                editor.ShowError("表格没有列定义");
                return;
            }
            editor.Grid.Cols.Count = cols.Count + editor.Grid.Cols.Fixed;
            for (int c = 0; c < cols.Count; c++)
            {
                int absCol = c + editor.Grid.Cols.Fixed;
                editor.Grid[0, absCol] = cols[c].Caption ?? $"列{c + 1}";
                editor.Grid.Cols[absCol].Width = 100;
            }

            int rowCount = Math.Min(maxRows, table.Rows.Count);
            editor.Grid.Rows.Count = rowCount + editor.Grid.Rows.Fixed;
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < cols.Count; c++)
                {
                    var cell = table[r, c];
                    editor.Grid[r + editor.Grid.Rows.Fixed, c + editor.Grid.Cols.Fixed] = cell?.GetDisplayValue() ?? "";
                }
            }

            if (table.Rows.Count > maxRows)
            {
                if (editor.Grid.Rows.Count < maxRows + 2)
                    editor.Grid.Rows.Count = maxRows + 2;
                editor.Grid[maxRows + 1, editor.Grid.Cols.Fixed] = $"... 共 {table.Rows.Count} 行，仅显示前 {maxRows} 行";
            }

            editor.Grid.AutoSizeCols();
        }
        finally
        {
            editor.Grid.EndUpdate();
        }
    }

    /// <summary>
    /// 将列索引（0-based）转换为 Excel 列字母（A, B, C... Z, AA, AB...）
    /// </summary>
    private static string ToColumnLetter(int col)
    {
        if (col < 0) return "";
        var sb = new System.Text.StringBuilder();
        col++; // 转为 1-based
        while (col > 0)
        {
            col--; // 0-based 用于计算
            sb.Insert(0, (char)('A' + (col % 26)));
            col /= 26;
        }
        return sb.ToString();
    }

    /// <summary>
    /// 格式化选区为 A1:B2 形式
    /// </summary>
    private static string FormatRange(int startRow, int startCol, int endRow, int endCol)
    {
        string c1 = ToColumnLetter(startCol);
        string c2 = ToColumnLetter(endCol);
        int r1 = startRow + 1; // 转为 1-based
        int r2 = endRow + 1;
        if (startRow == endRow && startCol == endCol)
            return $"{c1}{r1}";
        return $"{c1}{r1}:{c2}{r2}";
    }

    private async void BtnFinish_Click(object sender, EventArgs e)
    {
        try
        {
            // 重要：调用 Step4 的验证+生成 RefConfig 逻辑
            // ShowStep4 时将验证+生成逻辑放在了 _stepValidate 中，
            // 但 Step4 是最后一步，"下一步"隐藏，"完成"显示，所以需要在这里手动调用。
            if (_stepValidate != null)
            {
                if (!_stepValidate())
                    return;
            }

            // 设置默认引用名称
            if (string.IsNullOrEmpty(_refName))
                _refName = $"引用_{_selectedProjectName}_{DateTime.Now:yyyyMMdd}";

            var refItem = new CrossProjectDataRef
            {
                Id = _editingRef?.Id ?? _currentProject.GetNextId(),
                Name = _refName,
                SourceProjectId = _selectedProjectId,
                SourceProjectName = _selectedProjectName,
                SourceTableId = _selectedSourceTableId,
                SourceTableName = _selectedSourceTableName,
                TargetTableId = _selectedTargetTableId,
                TargetTableName = _selectedTargetTableName,
                RefMode = _selectedRefMode,
                RefConfig = _refConfigJson,
                ColumnMapping = null, // 当前版本不支持列映射
                AutoRefresh = _autoRefresh,
                Enabled = true,
                CacheDurationSeconds = _cacheDuration,
                DefaultValue = _defaultValue,
                CreatedAt = _editingRef?.CreatedAt ?? DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _store.Save(refItem);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"保存失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 构建项目 Tile 列表
    /// </summary>
    private void BuildTiles(string filter)
    {
        if (_tileProjects == null) return;
        _tileProjects.ClearSelected();
        _tileProjects.BeginUpdate();
        _tileProjects.Groups.Clear();

        var filtered = _allProjects.Where(p =>
            string.IsNullOrWhiteSpace(filter) ||
            (p.Name ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
            (p.Number ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        var group = new C1.Win.C1Tile.Group { Text = "所有项目" };
        foreach (var proj in filtered)
        {
            var tile = new Tile
            {
                Text = proj.Name,
                Image1 = Program.MainForm.CurrentEdition?.ProjectTileIcon,
                HorizontalSize = 5,
                VerticalSize = 4,
                BackColor = Color.FromArgb(248, 248, 248),
                Template = _projectTileTemplate,
                Tag = proj.Id
            };
            group.Tiles.Add(tile);
        }
        _tileProjects.Groups.Add(group);
        _tileProjects.EndUpdate();
    }

    /// <summary>
    /// 异步加载项目列表
    /// </summary>
    private async Task LoadProjectsAsync()
    {
        try
        {
            var projects = await StorageRouter.GetProjects();
            _allProjects = projects
                .Where(p => p.Type == ProjectType.Project && p.Id != _currentProject.Id)
                .ToList();
            BuildTiles(null);
        }
        catch (Exception ex) { ex.Log(); }
    }

    /// <summary>
    /// 在 frmCrossProjectDataRef 中调用此方法启动向导
    /// </summary>
    public static DialogResult ShowWizard(Leqisoft.Model.Project project, Id64 targetTableId, CrossProjectDataRef editingRef = null)
    {
        using var wizard = new frmCrossProjectRefConfigWizard(project, targetTableId, editingRef);
        return wizard.ShowDialog();
    }
}