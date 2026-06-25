using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1Tile;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

/// <summary>
/// 跨项目数据引用时的数据选择器对话框
/// 向导三步：选择项目 → 选择表格 → 按 RefMode 选择数据列/区域
/// </summary>
public class frmSelectProjectData : C1RibbonForm
{
    // ---- 状态 ----
    private readonly Guid _currentProjectId;
    private readonly RefMode _mode;
    private int _currentStep; // 0-based

    // ---- 步骤容器 ----
    private Panel _pnlStep1;
    private Panel _pnlStep2;
    private Panel _pnlStep3;
    private C1Label _lblTitle;
    private C1Label _lblStepInfo;

    // ---- 步骤1：项目选择 ----
    private C1TileControlEx _tileControl;
    private readonly List<DTO.Project> _projectList = new List<DTO.Project>();

    // ---- 步骤2：表格选择 ----
    private ListBox _lstTables;
    private readonly List<DTO.Table> _tableList = new List<DTO.Table>();

    // ---- 步骤3：数据选择 ----
    private Panel _pnlCellRef;
    private Panel _pnlColumnRef;
    private Panel _pnlAreaRef;
    private ListBox _lstColumns;       // CellRef: 单选
    private CheckedListBox _clbColumns; // ColumnRef: 多选
    private C1Label _lblAreaStartRow;
    private C1Label _lblAreaEndRow;
    private C1Label _lblAreaStartCol;
    private C1Label _lblAreaEndCol;
    private C1TextBox _txtAreaStartRow;
    private C1TextBox _txtAreaEndRow;
    private C1TextBox _txtAreaStartCol;
    private C1TextBox _txtAreaEndCol;
    private readonly List<DTO.Column> _columnList = new List<DTO.Column>();

    // ---- 导航按钮 ----
    private C1Button _btnPrev;
    private C1Button _btnNext;
    private C1Button _btnOk;
    private C1Button _btnCancel;

    // ---- 选择结果 ----
    /// <summary>选中的项目 ID</summary>
    public Guid SelectedProjectId { get; private set; }

    /// <summary>选中的表格 ID</summary>
    public Id64 SelectedTableId { get; private set; }

    /// <summary>选中的单元格 ID（CellRef 模式）</summary>
    public Id64 SelectedCellId { get; private set; }

    /// <summary>选中的列 ID 列表（ColumnRef 模式）</summary>
    public List<Id64> SelectedColumnIds { get; private set; } = new List<Id64>();

    /// <summary>区域起始行（AreaRef 模式）</summary>
    public int AreaStartRow { get; private set; }

    /// <summary>区域结束行（AreaRef 模式）</summary>
    public int AreaEndRow { get; private set; }

    /// <summary>区域起始列（AreaRef 模式）</summary>
    public int AreaStartCol { get; private set; }

    /// <summary>区域结束列（AreaRef 模式）</summary>
    public int AreaEndCol { get; private set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentProjectId">当前项目 ID，用于排除自身</param>
    /// <param name="mode">引用模式</param>
    public frmSelectProjectData(Guid currentProjectId, RefMode mode)
    {
        _currentProjectId = currentProjectId;
        _mode = mode;
        _currentStep = 0;
        InitializeComponent();
        UpdateStepUI();
    }

    /// <summary>
    /// 填充项目列表（外部注入）
    /// </summary>
    public void LoadProjectList(IEnumerable<DTO.Project> projects)
	{
		_projectList.Clear();
		_projectList.AddRange(projects.Where(p => p.Id != _currentProjectId));

        _tileControl.Groups.Clear();
        var group = new C1.Win.C1Tile.Group();

        foreach (var project in _projectList)
        {
            var tile = new Tile
            {
                Text = project.Name,
                HorizontalSize = 5,
                VerticalSize = 4,
                Tag = project
            };
            tile.BackColor = Color.White;
            group.Tiles.Add(tile);
        }

        _tileControl.Groups.Add(group);
    }

    /// <summary>
    /// 填充表格列表（外部注入）
    /// </summary>
    public void LoadTableList(IEnumerable<DTO.Table> tables)
    {
        _tableList.Clear();
        _tableList.AddRange(tables);

        _lstTables.Items.Clear();
        foreach (var table in _tableList)
        {
            _lstTables.Items.Add(table.Title);
        }
    }

    /// <summary>
    /// 填充列列表（外部注入）
    /// </summary>
    public void LoadColumnList(IEnumerable<DTO.Column> columns)
    {
        _columnList.Clear();
        _columnList.AddRange(columns);

        // CellRef 列表
        _lstColumns.Items.Clear();
        foreach (var col in _columnList)
        {
            _lstColumns.Items.Add(col.Caption);
        }

        // ColumnRef 列表
        _clbColumns.Items.Clear();
        foreach (var col in _columnList)
        {
            _clbColumns.Items.Add(col.Caption, false);
        }
    }

    private void UpdateStepUI()
    {
        // 显示所有步骤面板，仅当前步骤可见
        _pnlStep1.Visible = _currentStep == 0;
        _pnlStep2.Visible = _currentStep == 1;
        _pnlStep3.Visible = _currentStep == 2;

        // 导航按钮状态
        _btnPrev.Enabled = _currentStep > 0;
        _btnNext.Enabled = _currentStep < 2;
        _btnNext.Visible = _currentStep < 2;
        _btnOk.Visible = _currentStep == 2;

        // 标题信息
        var modeName = _mode switch
        {
            RefMode.CellRef => "单元格引用",
            RefMode.ColumnRef => "列引用",
            RefMode.AreaRef => "区域引用",
            RefMode.FormulaCompute => "公式运算",
            _ => "数据引用"
        };

        var stepNames = new[] { "选择项目", "选择表格", "选择数据" };
        _lblStepInfo.Text = $"步骤 {_currentStep + 1}/3：{stepNames[_currentStep]}";
        this.Text = $"跨项目数据引用 - {modeName} - {_lblStepInfo.Text}";
    }

    private void GoToStep(int step)
    {
        if (step < 0 || step > 2) return;
        _currentStep = step;
        UpdateStepUI();
    }

    private void _btnPrev_Click(object sender, EventArgs e)
    {
        GoToStep(_currentStep - 1);
    }

    private void _btnNext_Click(object sender, EventArgs e)
    {
        if (_currentStep == 0)
        {
            // 验证项目是否已选
            if (SelectedProjectId == Guid.Empty)
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一个项目");
                return;
            }
        }
        else if (_currentStep == 1)
        {
            // 验证表格是否已选
            if (SelectedTableId.IsZero())
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一个表格");
                return;
            }
        }

        GoToStep(_currentStep + 1);
    }

    private void _btnOk_Click(object sender, EventArgs e)
    {
        // 收集最终数据
        switch (_mode)
        {
            case RefMode.CellRef:
            case RefMode.FormulaCompute:
                if (_lstColumns.SelectedIndex < 0)
                {
                    Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择一个列");
                    return;
                }
                SelectedColumnIds = new List<Id64> { _columnList[_lstColumns.SelectedIndex].Id };
                SelectedCellId = _columnList[_lstColumns.SelectedIndex].Id;
                break;

            case RefMode.ColumnRef:
                {
                    var selected = new List<Id64>();
                    for (int i = 0; i < _clbColumns.Items.Count; i++)
                    {
                        if (_clbColumns.GetItemChecked(i))
                        {
                            selected.Add(_columnList[i].Id);
                        }
                    }
                    if (selected.Count == 0)
                    {
                        Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请至少选择一个列");
                        return;
                    }
                    SelectedColumnIds = selected;
                    break;
                }

            case RefMode.AreaRef:
                {
                    if (!int.TryParse(_txtAreaStartRow.Value?.ToString(), out int sr) ||
                        !int.TryParse(_txtAreaEndRow.Value?.ToString(), out int er) ||
                        !int.TryParse(_txtAreaStartCol.Value?.ToString(), out int sc) ||
                        !int.TryParse(_txtAreaEndCol.Value?.ToString(), out int ec))
                    {
                        Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请输入有效的行号和列号（整数）");
                        return;
                    }
                    if (sr > er || sc > ec || sr < 0 || sc < 0)
                    {
                        Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "起始行列号必须 ≤ 结束行列号，且不能为负数");
                        return;
                    }
                    AreaStartRow = sr;
                    AreaEndRow = er;
                    AreaStartCol = sc;
                    AreaEndCol = ec;
                    break;
                }
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void _btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    private void _tileControl_DoubleClickTile(object sender, Tile e)
    {
        var project = e.Tag as DTO.Project;
        if (project != null)
        {
            SelectedProjectId = project.Id;
            GoToStep(1);
        }
    }

    private void _tileControl_TileClick(object sender, TileEventArgs e)
    {
        var project = e.Tile.Tag as DTO.Project;
        if (project != null)
        {
            SelectedProjectId = project.Id;
        }
    }

    private void _lstTables_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_lstTables.SelectedIndex >= 0 && _lstTables.SelectedIndex < _tableList.Count)
        {
            SelectedTableId = _tableList[_lstTables.SelectedIndex].Id;
        }
    }

    private void _lstTables_DoubleClick(object sender, EventArgs e)
    {
        if (_lstTables.SelectedIndex >= 0)
        {
            SelectedTableId = _tableList[_lstTables.SelectedIndex].Id;
            GoToStep(2);
        }
    }

    private void InitializeComponent()
    {
        // ---- 标题 ----
        _lblTitle = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei", 12f, FontStyle.Bold),
            ForeColor = Color.Black,
            Location = new Point(12, 12),
            Name = "_lblTitle",
            Size = new Size(300, 22),
            Text = "跨项目数据引用",
            TextDetached = true
        };

        _lblStepInfo = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei", 10f),
            ForeColor = Color.Gray,
            Location = new Point(12, 38),
            Name = "_lblStepInfo",
            Size = new Size(200, 19),
            Text = "步骤 1/3：选择项目",
            TextDetached = true
        };

        // ======== 步骤1：项目选择 ========
        _pnlStep1 = new Panel
        {
            Location = new Point(12, 65),
            Size = new Size(760, 430),
            Name = "_pnlStep1"
        };

        _tileControl = new C1TileControlEx
        {
            AllowChecking = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.FromArgb(240, 240, 240),
            CellWidth = 10,
            CellHeight = 10,
            Dock = DockStyle.Fill,
            Name = "_tileControl"
        };
        _tileControl.DoubleClickTile += _tileControl_DoubleClickTile;
        _tileControl.TileClicked += _tileControl_TileClick;
        _pnlStep1.Controls.Add(_tileControl);

        // ======== 步骤2：表格选择 ========
        _pnlStep2 = new Panel
        {
            Location = new Point(12, 65),
            Size = new Size(760, 430),
            Name = "_pnlStep2",
            Visible = false
        };

        var lblTableTitle = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei", 10f),
            ForeColor = Color.Black,
            Location = new Point(0, 5),
            Name = "_lblTableTitle",
            Size = new Size(100, 19),
            Text = "请选择来源表格：",
            TextDetached = true
        };

        _lstTables = new ListBox
        {
            Location = new Point(0, 30),
            Size = new Size(760, 390),
            Name = "_lstTables",
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        _lstTables.SelectedIndexChanged += _lstTables_SelectedIndexChanged;
        _lstTables.DoubleClick += _lstTables_DoubleClick;

        _pnlStep2.Controls.Add(lblTableTitle);
        _pnlStep2.Controls.Add(_lstTables);

        // ======== 步骤3：数据选择 ========
        _pnlStep3 = new Panel
        {
            Location = new Point(12, 65),
            Size = new Size(760, 430),
            Name = "_pnlStep3",
            Visible = false
        };

        // ---- CellRef 面板 ----
        _pnlCellRef = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(760, 430),
            Name = "_pnlCellRef"
        };

        var lblCellRef = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei", 10f),
            ForeColor = Color.Black,
            Location = new Point(0, 5),
            Size = new Size(150, 19),
            Text = "请选择要引用的列：",
            TextDetached = true
        };

        _lstColumns = new ListBox
        {
            Location = new Point(0, 30),
            Size = new Size(760, 390),
            Name = "_lstColumns",
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        _pnlCellRef.Controls.Add(lblCellRef);
        _pnlCellRef.Controls.Add(_lstColumns);

        // ---- ColumnRef 面板 ----
        _pnlColumnRef = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(760, 430),
            Name = "_pnlColumnRef",
            Visible = false
        };

        var lblColumnRef = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei", 10f),
            ForeColor = Color.Black,
            Location = new Point(0, 5),
            Size = new Size(200, 19),
            Text = "请选择要引用的列（可多选）：",
            TextDetached = true
        };

        _clbColumns = new CheckedListBox
        {
            Location = new Point(0, 30),
            Size = new Size(760, 390),
            Name = "_clbColumns",
            CheckOnClick = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        _pnlColumnRef.Controls.Add(lblColumnRef);
        _pnlColumnRef.Controls.Add(_clbColumns);

        // ---- AreaRef 面板 ----
        _pnlAreaRef = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(760, 430),
            Name = "_pnlAreaRef",
            Visible = false
        };

        var lblAreaDesc = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei", 10f, FontStyle.Bold),
            ForeColor = Color.Black,
            Location = new Point(0, 5),
            Size = new Size(200, 19),
            Text = "请输入引用的区域范围：",
            TextDetached = true
        };

        _lblAreaStartRow = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            ForeColor = Color.Black,
            Location = new Point(0, 45),
            Size = new Size(80, 17),
            Text = "起始行号：",
            TextDetached = true
        };

        _txtAreaStartRow = new C1TextBox
        {
            Location = new Point(85, 42),
            Size = new Size(100, 23),
            Name = "_txtAreaStartRow"
        };

        _lblAreaEndRow = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            ForeColor = Color.Black,
            Location = new Point(200, 45),
            Size = new Size(80, 17),
            Text = "结束行号：",
            TextDetached = true
        };

        _txtAreaEndRow = new C1TextBox
        {
            Location = new Point(285, 42),
            Size = new Size(100, 23),
            Name = "_txtAreaEndRow"
        };

        _lblAreaStartCol = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            ForeColor = Color.Black,
            Location = new Point(0, 85),
            Size = new Size(80, 17),
            Text = "起始列号：",
            TextDetached = true
        };

        _txtAreaStartCol = new C1TextBox
        {
            Location = new Point(85, 82),
            Size = new Size(100, 23),
            Name = "_txtAreaStartCol"
        };

        _lblAreaEndCol = new C1Label
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.None,
            ForeColor = Color.Black,
            Location = new Point(200, 85),
            Size = new Size(80, 17),
            Text = "结束列号：",
            TextDetached = true
        };

        _txtAreaEndCol = new C1TextBox
        {
            Location = new Point(285, 82),
            Size = new Size(100, 23),
            Name = "_txtAreaEndCol"
        };

        _pnlAreaRef.Controls.Add(lblAreaDesc);
        _pnlAreaRef.Controls.Add(_lblAreaStartRow);
        _pnlAreaRef.Controls.Add(_txtAreaStartRow);
        _pnlAreaRef.Controls.Add(_lblAreaEndRow);
        _pnlAreaRef.Controls.Add(_txtAreaEndRow);
        _pnlAreaRef.Controls.Add(_lblAreaStartCol);
        _pnlAreaRef.Controls.Add(_txtAreaStartCol);
        _pnlAreaRef.Controls.Add(_lblAreaEndCol);
        _pnlAreaRef.Controls.Add(_txtAreaEndCol);

        // 将各 RefMode 面板加入步骤3
        _pnlStep3.Controls.Add(_pnlCellRef);
        _pnlStep3.Controls.Add(_pnlColumnRef);
        _pnlStep3.Controls.Add(_pnlAreaRef);

        // 根据 RefMode 显示对应的面板
        switch (_mode)
        {
            case RefMode.CellRef:
                _pnlCellRef.Visible = true;
                _pnlColumnRef.Visible = false;
                _pnlAreaRef.Visible = false;
                break;
            case RefMode.ColumnRef:
                _pnlCellRef.Visible = false;
                _pnlColumnRef.Visible = true;
                _pnlAreaRef.Visible = false;
                break;
            case RefMode.AreaRef:
                _pnlCellRef.Visible = false;
                _pnlColumnRef.Visible = false;
                _pnlAreaRef.Visible = true;
                break;
            case RefMode.FormulaCompute:
                _pnlCellRef.Visible = true;
                _pnlColumnRef.Visible = false;
                _pnlAreaRef.Visible = false;
                break;
        }

        // ======== 导航按钮 ========
        _btnPrev = new C1Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Font = new Font("Microsoft YaHei", 9f),
            Location = new Point(508, 520),
            Name = "_btnPrev",
            Size = new Size(87, 33),
            TabIndex = 10,
            Text = "上一步",
            UseVisualStyleBackColor = true
        };
        _btnPrev.Click += _btnPrev_Click;

        _btnNext = new C1Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Font = new Font("Microsoft YaHei", 9f),
            Location = new Point(601, 520),
            Name = "_btnNext",
            Size = new Size(87, 33),
            TabIndex = 11,
            Text = "下一步",
            UseVisualStyleBackColor = true
        };
        _btnNext.Click += _btnNext_Click;

        _btnOk = new C1Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Font = new Font("Microsoft YaHei", 9f),
            Location = new Point(601, 520),
            Name = "_btnOk",
            Size = new Size(87, 33),
            TabIndex = 12,
            Text = "确定",
            UseVisualStyleBackColor = true,
            Visible = false
        };
        _btnOk.Click += _btnOk_Click;

        _btnCancel = new C1Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Font = new Font("Microsoft YaHei", 9f),
            Location = new Point(694, 520),
            Name = "_btnCancel",
            Size = new Size(87, 33),
            TabIndex = 13,
            Text = "取消",
            UseVisualStyleBackColor = true
        };
        _btnCancel.Click += _btnCancel_Click;

        // ======== frmSelectProjectData ========
        this.AutoScaleDimensions = new SizeF(7f, 17f);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(784, 565);
        this.Controls.Add(_lblTitle);
        this.Controls.Add(_lblStepInfo);
        this.Controls.Add(_pnlStep1);
        this.Controls.Add(_pnlStep2);
        this.Controls.Add(_pnlStep3);
        this.Controls.Add(_btnPrev);
        this.Controls.Add(_btnNext);
        this.Controls.Add(_btnOk);
        this.Controls.Add(_btnCancel);
        this.Font = new Font("Microsoft YaHei", 9f);
        this.Name = "frmSelectProjectData";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "跨项目数据引用";
    }
}