﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Leqisoft.DTO;
using Leqisoft.Model;
using Project = Leqisoft.Model.Project;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 跨项目数据引用的配置管理对话框
/// </summary>
public class frmCrossProjectDataRef : C1RibbonForm
{
    private readonly Project _currentProject;
    private readonly Id64 _currentTableId;
    private readonly CrossProjectDataRefStore _store;
    private List<CrossProjectDataRef> _refList;

    // UI 控件
    private C1Label _lblTitle;
    private C1FlexGrid _grid;
    private C1Button _btnAdd;
    private C1Button _btnEdit;
    private C1Button _btnDelete;
    private C1Button _btnToggleEnabled;
    private C1Button _btnRefreshSelected;
    private C1Button _btnRefreshAll;
    private C1Button _btnClose;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentProject">当前项目</param>
    /// <param name="currentTableId">当前表 ID</param>
    public frmCrossProjectDataRef(Project currentProject, Id64 currentTableId)
    {
        _currentProject = currentProject ?? throw new ArgumentNullException(nameof(currentProject));
        _currentTableId = currentTableId;
        _store = new CrossProjectDataRefStore(currentProject);
        _refList = new List<CrossProjectDataRef>();

        InitializeComponent();
        this.Load += FrmCrossProjectDataRef_Load;
    }

    private async void FrmCrossProjectDataRef_Load(object sender, EventArgs e)
    {
        try
        {
            await RefreshList();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"初始化失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 刷新网格显示
    /// </summary>
    private async Task RefreshList()
    {
        try
        {
            _refList = await _store.Load(_currentTableId);
            _grid.Rows.Count = 1; // 保留固定行

            for (int i = 0; i < _refList.Count; i++)
            {
                var item = _refList[i];
                _grid.Rows.Add();
                var row = _grid.Rows[i + 1];
                row[0] = i + 1;
                row[1] = item.Name;
                row[2] = item.SourceProjectId.ToString();
                row[3] = item.SourceTableId.Value.ToString();
                row[4] = GetRefModeDisplay(item.RefMode);
                row[5] = item.Enabled ? "启用" : "禁用";
                row[6] = item.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                row[7] = ""; // 最后执行结果（暂空）
                row.UserData = item;
            }

            _grid.AutoSizeCols();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"加载引用列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取引用模式的中文显示
    /// </summary>
    private static string GetRefModeDisplay(RefMode mode)
    {
        return mode switch
        {
            RefMode.CellRef => "单元格",
            RefMode.ColumnRef => "列",
            RefMode.AreaRef => "区域",
            RefMode.FormulaCompute => "公式运算",
            _ => mode.ToString()
        };
    }

    /// <summary>
    /// 弹出新增对话框
    /// </summary>
    private void ShowAddDialog()
    {
        using var dialog = new frmCrossProjectDataRefEditDialog("新增引用", null, _currentTableId);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var newRef = dialog.GetResult();
            newRef.Id = _currentProject.GetNextId();
            newRef.TargetTableId = _currentTableId;
            newRef.CreatedAt = DateTime.Now;
            newRef.UpdatedAt = DateTime.Now;

            _ = SaveAndRefresh(newRef);
        }
    }

    /// <summary>
    /// 弹出编辑对话框
    /// </summary>
    private void ShowEditDialog(CrossProjectDataRef item)
    {
        if (item == null) return;

        using var dialog = new frmCrossProjectDataRefEditDialog("编辑引用", item, _currentTableId);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var updated = dialog.GetResult();
            updated.Id = item.Id;
            updated.TargetTableId = _currentTableId;
            updated.CreatedAt = item.CreatedAt;
            updated.UpdatedAt = DateTime.Now;

            _ = SaveAndRefresh(updated);
        }
    }

    private async Task SaveAndRefresh(CrossProjectDataRef refItem)
    {
        try
        {
            await _store.Save(refItem);
            await RefreshList();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"保存失败: {ex.Message}");
        }
    }

    // ---- 事件处理 ----

    private void _btnAdd_Click(object sender, EventArgs e)
    {
        ShowAddDialog();
    }

    private void _btnEdit_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
            return;
        }
        ShowEditDialog(selected);
    }

    private async void _btnDelete_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
            return;
        }

        var confirm = Leqisoft.UI.Controls.MessageBox.Show(
            MessageBoxIcon.Question,
            $"确定要删除引用 \"{selected.Name}\" 吗？",
            MessageBoxButtons.YesNo,
            "确认删除");

        if (confirm == DialogResult.Yes)
        {
            try
            {
                await _store.Delete(selected.Id);
                await RefreshList();
            }
            catch (Exception ex)
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"删除失败: {ex.Message}");
            }
        }
    }

    private async void _btnToggleEnabled_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
            return;
        }

        selected.Enabled = !selected.Enabled;
        selected.UpdatedAt = DateTime.Now;

        try
        {
            await _store.Save(selected);
            await RefreshList();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"保存失败: {ex.Message}");
        }
    }

    private async void _btnRefreshSelected_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
            return;
        }

        try
        {
            var manager = new CrossProjectDataRefManager(_currentProject);
            var result = await manager.ExecuteRef(selected);
            if (result.Success)
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"刷新完成，影响 {result.AffectedRows} 行");
            }
            else
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, $"刷新失败: {result.ErrorMessage}");
            }
            await RefreshList();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"刷新失败: {ex.Message}");
        }
    }

    private async void _btnRefreshAll_Click(object sender, EventArgs e)
    {
        try
        {
            var manager = new CrossProjectDataRefManager(_currentProject);
            var results = await manager.ExecuteAll(_currentTableId);
            int success = results.Count(r => r.Success);
            int failed = results.Count(r => !r.Success);
            Leqisoft.UI.Controls.MessageBox.Show(
                MessageBoxIcon.None,
                $"全部刷新完成：成功 {success} 个，失败 {failed} 个");
            await RefreshList();
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"刷新失败: {ex.Message}");
        }
    }

    private void _btnClose_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// 获取当前选中的引用
    /// </summary>
    private CrossProjectDataRef GetSelectedRef()
    {
        int sel = _grid.RowSel;
        if (sel > 0 && sel <= _refList.Count)
        {
            return _grid.Rows[sel].UserData as CrossProjectDataRef;
        }
        return null;
    }

    // ---- 初始化组件 ----

    private void InitializeComponent()
    {
        this._lblTitle = new C1Label();
        this._grid = new C1FlexGrid();
        this._btnAdd = new C1Button();
        this._btnEdit = new C1Button();
        this._btnDelete = new C1Button();
        this._btnToggleEnabled = new C1Button();
        this._btnRefreshSelected = new C1Button();
        this._btnRefreshAll = new C1Button();
        this._btnClose = new C1Button();

        //
        // _lblTitle
        //
        this._lblTitle.AutoSize = true;
        this._lblTitle.BackColor = Color.Transparent;
        this._lblTitle.BorderStyle = BorderStyle.None;
        this._lblTitle.Font = new Font("Microsoft YaHei", 12f, FontStyle.Bold);
        this._lblTitle.ForeColor = Color.Black;
        this._lblTitle.Location = new Point(12, 15);
        this._lblTitle.Name = "_lblTitle";
        this._lblTitle.Size = new Size(200, 22);
        this._lblTitle.TabIndex = 0;
        this._lblTitle.Text = "跨项目数据引用管理";
        this._lblTitle.TextDetached = true;

        //
        // _grid
        //
        this._grid.AllowEditing = false;
        this._grid.AllowFiltering = false;
        this._grid.AllowSorting = AllowSortingEnum.None;
        this._grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this._grid.BackColor = Color.White;
        this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle;
        this._grid.Cols.Count = 8;
        this._grid.Cols[0].Caption = "序号";
        this._grid.Cols[0].Width = 45;
        this._grid.Cols[1].Caption = "引用名称";
        this._grid.Cols[1].Width = 150;
        this._grid.Cols[2].Caption = "来源项目";
        this._grid.Cols[2].Width = 230;
        this._grid.Cols[3].Caption = "来源表";
        this._grid.Cols[3].Width = 80;
        this._grid.Cols[4].Caption = "引用模式";
        this._grid.Cols[4].Width = 80;
        this._grid.Cols[5].Caption = "启用状态";
        this._grid.Cols[5].Width = 70;
        this._grid.Cols[6].Caption = "更新时间";
        this._grid.Cols[6].Width = 150;
        this._grid.Cols[7].Caption = "最后执行结果";
        this._grid.Cols[7].Width = 100;
        this._grid.Cols.Fixed = 1;
        this._grid.Location = new Point(12, 50);
        this._grid.Name = "_grid";
        this._grid.Rows.Count = 1;
        this._grid.Rows.Fixed = 1;
        this._grid.Rows.DefaultSize = 24;
        this._grid.SelectionMode = SelectionModeEnum.Row;
        this._grid.Size = new Size(960, 380);
        this._grid.Styles.Normal.Font = new Font("Microsoft YaHei", 9f);
        this._grid.Styles.Fixed.Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold);
        this._grid.TabIndex = 1;

        //
        // _btnAdd
        //
        this._btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this._btnAdd.Font = new Font("Microsoft YaHei", 9f);
        this._btnAdd.Location = new Point(12, 440);
        this._btnAdd.Name = "_btnAdd";
        this._btnAdd.Size = new Size(87, 33);
        this._btnAdd.TabIndex = 2;
        this._btnAdd.Text = "新增";
        this._btnAdd.UseVisualStyleBackColor = true;
        this._btnAdd.Click += _btnAdd_Click;

        //
        // _btnEdit
        //
        this._btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this._btnEdit.Font = new Font("Microsoft YaHei", 9f);
        this._btnEdit.Location = new Point(105, 440);
        this._btnEdit.Name = "_btnEdit";
        this._btnEdit.Size = new Size(87, 33);
        this._btnEdit.TabIndex = 3;
        this._btnEdit.Text = "编辑";
        this._btnEdit.UseVisualStyleBackColor = true;
        this._btnEdit.Click += _btnEdit_Click;

        //
        // _btnDelete
        //
        this._btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this._btnDelete.Font = new Font("Microsoft YaHei", 9f);
        this._btnDelete.Location = new Point(198, 440);
        this._btnDelete.Name = "_btnDelete";
        this._btnDelete.Size = new Size(87, 33);
        this._btnDelete.TabIndex = 4;
        this._btnDelete.Text = "删除";
        this._btnDelete.UseVisualStyleBackColor = true;
        this._btnDelete.Click += _btnDelete_Click;

        //
        // _btnToggleEnabled
        //
        this._btnToggleEnabled.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this._btnToggleEnabled.Font = new Font("Microsoft YaHei", 9f);
        this._btnToggleEnabled.Location = new Point(291, 440);
        this._btnToggleEnabled.Name = "_btnToggleEnabled";
        this._btnToggleEnabled.Size = new Size(87, 33);
        this._btnToggleEnabled.TabIndex = 5;
        this._btnToggleEnabled.Text = "启用/禁用";
        this._btnToggleEnabled.UseVisualStyleBackColor = true;
        this._btnToggleEnabled.Click += _btnToggleEnabled_Click;

        //
        // _btnRefreshSelected
        //
        this._btnRefreshSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this._btnRefreshSelected.Font = new Font("Microsoft YaHei", 9f);
        this._btnRefreshSelected.Location = new Point(384, 440);
        this._btnRefreshSelected.Name = "_btnRefreshSelected";
        this._btnRefreshSelected.Size = new Size(87, 33);
        this._btnRefreshSelected.TabIndex = 6;
        this._btnRefreshSelected.Text = "刷新选定";
        this._btnRefreshSelected.UseVisualStyleBackColor = true;
        this._btnRefreshSelected.Click += _btnRefreshSelected_Click;

        //
        // _btnRefreshAll
        //
        this._btnRefreshAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this._btnRefreshAll.Font = new Font("Microsoft YaHei", 9f);
        this._btnRefreshAll.Location = new Point(477, 440);
        this._btnRefreshAll.Name = "_btnRefreshAll";
        this._btnRefreshAll.Size = new Size(87, 33);
        this._btnRefreshAll.TabIndex = 7;
        this._btnRefreshAll.Text = "刷新所有";
        this._btnRefreshAll.UseVisualStyleBackColor = true;
        this._btnRefreshAll.Click += _btnRefreshAll_Click;

        //
        // _btnClose
        //
        this._btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this._btnClose.Font = new Font("Microsoft YaHei", 9f);
        this._btnClose.Location = new Point(885, 440);
        this._btnClose.Name = "_btnClose";
        this._btnClose.Size = new Size(87, 33);
        this._btnClose.TabIndex = 8;
        this._btnClose.Text = "关闭";
        this._btnClose.UseVisualStyleBackColor = true;
        this._btnClose.Click += _btnClose_Click;

        //
        // frmCrossProjectDataRef
        //
        this.AutoScaleDimensions = new SizeF(7f, 17f);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(984, 485);
        this.Controls.Add(this._lblTitle);
        this.Controls.Add(this._grid);
        this.Controls.Add(this._btnAdd);
        this.Controls.Add(this._btnEdit);
        this.Controls.Add(this._btnDelete);
        this.Controls.Add(this._btnToggleEnabled);
        this.Controls.Add(this._btnRefreshSelected);
        this.Controls.Add(this._btnRefreshAll);
        this.Controls.Add(this._btnClose);
        this.Font = new Font("Microsoft YaHei", 9f);
        this.Name = "frmCrossProjectDataRef";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "跨项目数据引用管理";
    }
}

/// <summary>
/// 简化版新增/编辑引用对话框
/// </summary>
internal class frmCrossProjectDataRefEditDialog : C1RibbonForm
{
    private readonly CrossProjectDataRef _existing;
    private readonly Id64 _currentTableId;

    private TextBox _txtName;
    private TextBox _txtSourceProjectId;
    private TextBox _txtSourceTableId;
    private TextBox _txtTargetTableId;
    private ComboBox _cmbRefMode;
    private TextBox _txtRefConfig;
    private TextBox _txtFilterConfig;
    private TextBox _txtFormulaExpression;
    private TextBox _txtColumnMapping;
    private C1Button _btnOk;
    private C1Button _btnCancel;

    public frmCrossProjectDataRefEditDialog(string title, CrossProjectDataRef existing, Id64 currentTableId)
    {
        _existing = existing;
        _currentTableId = currentTableId;
        InitializeComponent();
        this.Text = title;

        if (_existing != null)
        {
            LoadFromExisting();
        }
        else
        {
            _txtTargetTableId.Text = _currentTableId.Value.ToString();
        }
    }

    private void LoadFromExisting()
    {
        _txtName.Text = _existing.Name;
        _txtSourceProjectId.Text = _existing.SourceProjectId.ToString();
        _txtSourceTableId.Text = _existing.SourceTableId.Value.ToString();
        _txtTargetTableId.Text = _existing.TargetTableId.Value.ToString();
        _cmbRefMode.SelectedItem = GetRefModeDisplay(_existing.RefMode);
        _txtRefConfig.Text = _existing.RefConfig ?? string.Empty;
        _txtFilterConfig.Text = _existing.FilterConfig ?? string.Empty;
        _txtFormulaExpression.Text = _existing.FormulaExpression ?? string.Empty;
        _txtColumnMapping.Text = _existing.ColumnMapping ?? string.Empty;
    }

    /// <summary>
    /// 获取对话框结果
    /// </summary>
    public CrossProjectDataRef GetResult()
    {
        var result = _existing ?? new CrossProjectDataRef();

        result.Name = _txtName.Text.Trim();
        if (Guid.TryParse(_txtSourceProjectId.Text.Trim(), out Guid sourceProjectId))
            result.SourceProjectId = sourceProjectId;
        if (long.TryParse(_txtSourceTableId.Text.Trim(), out long sourceTableId))
            result.SourceTableId = new Id64(sourceTableId);
        if (long.TryParse(_txtTargetTableId.Text.Trim(), out long targetTableId))
            result.TargetTableId = new Id64(targetTableId);

        result.RefMode = _cmbRefMode.SelectedItem?.ToString() switch
        {
            "单元格" => RefMode.CellRef,
            "列" => RefMode.ColumnRef,
            "区域" => RefMode.AreaRef,
            "公式运算" => RefMode.FormulaCompute,
            _ => RefMode.CellRef
        };

        result.RefConfig = string.IsNullOrWhiteSpace(_txtRefConfig.Text) ? null : _txtRefConfig.Text.Trim();
        result.FilterConfig = string.IsNullOrWhiteSpace(_txtFilterConfig.Text) ? null : _txtFilterConfig.Text.Trim();
        result.FormulaExpression = string.IsNullOrWhiteSpace(_txtFormulaExpression.Text) ? null : _txtFormulaExpression.Text.Trim();
        result.ColumnMapping = string.IsNullOrWhiteSpace(_txtColumnMapping.Text) ? null : _txtColumnMapping.Text.Trim();

        return result;
    }

    private static string GetRefModeDisplay(RefMode mode)
    {
        return mode switch
        {
            RefMode.CellRef => "单元格",
            RefMode.ColumnRef => "列",
            RefMode.AreaRef => "区域",
            RefMode.FormulaCompute => "公式运算",
            _ => mode.ToString()
        };
    }

    private void InitializeComponent()
    {
        this._txtName = new TextBox();
        this._txtSourceProjectId = new TextBox();
        this._txtSourceTableId = new TextBox();
        this._txtTargetTableId = new TextBox();
        this._cmbRefMode = new ComboBox();
        this._txtRefConfig = new TextBox();
        this._txtFilterConfig = new TextBox();
        this._txtFormulaExpression = new TextBox();
        this._txtColumnMapping = new TextBox();
        this._btnOk = new C1Button();
        this._btnCancel = new C1Button();

        var lblName = new Label { Text = "引用名称：", Location = new Point(12, 15), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblSourceProjectId = new Label { Text = "来源项目 ID：", Location = new Point(12, 48), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblSourceTableId = new Label { Text = "来源表 ID：", Location = new Point(12, 81), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblTargetTableId = new Label { Text = "目标表 ID：", Location = new Point(12, 114), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblRefMode = new Label { Text = "引用模式：", Location = new Point(12, 147), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblRefConfig = new Label { Text = "引用配置 JSON：", Location = new Point(12, 180), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblFilterConfig = new Label { Text = "筛选配置 JSON：", Location = new Point(12, 213), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblFormulaExpression = new Label { Text = "公式表达式：", Location = new Point(12, 246), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblColumnMapping = new Label { Text = "列映射 JSON：", Location = new Point(12, 279), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };

        //
        // _txtName
        //
        this._txtName.Font = new Font("Microsoft YaHei", 9f);
        this._txtName.Location = new Point(118, 12);
        this._txtName.Name = "_txtName";
        this._txtName.Size = new Size(350, 24);
        this._txtName.TabIndex = 0;

        //
        // _txtSourceProjectId
        //
        this._txtSourceProjectId.Font = new Font("Microsoft YaHei", 9f);
        this._txtSourceProjectId.Location = new Point(118, 45);
        this._txtSourceProjectId.Name = "_txtSourceProjectId";
        this._txtSourceProjectId.Size = new Size(350, 24);
        this._txtSourceProjectId.TabIndex = 1;

        //
        // _txtSourceTableId
        //
        this._txtSourceTableId.Font = new Font("Microsoft YaHei", 9f);
        this._txtSourceTableId.Location = new Point(118, 78);
        this._txtSourceTableId.Name = "_txtSourceTableId";
        this._txtSourceTableId.Size = new Size(350, 24);
        this._txtSourceTableId.TabIndex = 2;

        //
        // _txtTargetTableId
        //
        this._txtTargetTableId.Font = new Font("Microsoft YaHei", 9f);
        this._txtTargetTableId.Location = new Point(118, 111);
        this._txtTargetTableId.Name = "_txtTargetTableId";
        this._txtTargetTableId.ReadOnly = true;
        this._txtTargetTableId.Size = new Size(350, 24);
        this._txtTargetTableId.TabIndex = 3;

        //
        // _cmbRefMode
        //
        this._cmbRefMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this._cmbRefMode.Font = new Font("Microsoft YaHei", 9f);
        this._cmbRefMode.Items.AddRange(new object[] { "单元格", "列", "区域", "公式运算" });
        this._cmbRefMode.Location = new Point(118, 144);
        this._cmbRefMode.Name = "_cmbRefMode";
        this._cmbRefMode.Size = new Size(350, 24);
        this._cmbRefMode.TabIndex = 4;
        this._cmbRefMode.SelectedIndex = 0;

        //
        // _txtRefConfig
        //
        this._txtRefConfig.Font = new Font("Microsoft YaHei", 9f);
        this._txtRefConfig.Location = new Point(118, 177);
        this._txtRefConfig.Name = "_txtRefConfig";
        this._txtRefConfig.Size = new Size(350, 24);
        this._txtRefConfig.TabIndex = 5;

        //
        // _txtFilterConfig
        //
        this._txtFilterConfig.Font = new Font("Microsoft YaHei", 9f);
        this._txtFilterConfig.Location = new Point(118, 210);
        this._txtFilterConfig.Name = "_txtFilterConfig";
        this._txtFilterConfig.Size = new Size(350, 24);
        this._txtFilterConfig.TabIndex = 6;

        //
        // _txtFormulaExpression
        //
        this._txtFormulaExpression.Font = new Font("Microsoft YaHei", 9f);
        this._txtFormulaExpression.Location = new Point(118, 243);
        this._txtFormulaExpression.Name = "_txtFormulaExpression";
        this._txtFormulaExpression.Size = new Size(350, 24);
        this._txtFormulaExpression.TabIndex = 7;

        //
        // _txtColumnMapping
        //
        this._txtColumnMapping.Font = new Font("Microsoft YaHei", 9f);
        this._txtColumnMapping.Location = new Point(118, 276);
        this._txtColumnMapping.Name = "_txtColumnMapping";
        this._txtColumnMapping.Size = new Size(350, 24);
        this._txtColumnMapping.TabIndex = 8;

        //
        // _btnOk
        //
        this._btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this._btnOk.Font = new Font("Microsoft YaHei", 9f);
        this._btnOk.Location = new Point(291, 315);
        this._btnOk.Name = "_btnOk";
        this._btnOk.Size = new Size(87, 33);
        this._btnOk.TabIndex = 9;
        this._btnOk.Text = "确定";
        this._btnOk.UseVisualStyleBackColor = true;
        this._btnOk.Click += (s, e) =>
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(this._txtName.Text))
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请输入引用名称");
                return;
            }
            if (!Guid.TryParse(this._txtSourceProjectId.Text.Trim(), out _))
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "来源项目 ID 格式不正确，请输入有效的 GUID");
                return;
            }
            if (!long.TryParse(this._txtSourceTableId.Text.Trim(), out _))
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "来源表 ID 必须是数字");
                return;
            }
            // 验证 JSON 格式
            if (!string.IsNullOrWhiteSpace(this._txtRefConfig.Text) && !IsValidJson(this._txtRefConfig.Text))
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "引用配置 JSON 格式不正确");
                return;
            }
            if (!string.IsNullOrWhiteSpace(this._txtFilterConfig.Text) && !IsValidJson(this._txtFilterConfig.Text))
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "筛选配置 JSON 格式不正确");
                return;
            }
            if (!string.IsNullOrWhiteSpace(this._txtColumnMapping.Text) && !IsValidJson(this._txtColumnMapping.Text))
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "列映射 JSON 格式不正确");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        };

        //
        // _btnCancel
        //
        this._btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this._btnCancel.Font = new Font("Microsoft YaHei", 9f);
        this._btnCancel.Location = new Point(384, 315);
        this._btnCancel.Name = "_btnCancel";
        this._btnCancel.Size = new Size(87, 33);
        this._btnCancel.TabIndex = 10;
        this._btnCancel.Text = "取消";
        this._btnCancel.UseVisualStyleBackColor = true;
        this._btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        //
        // frmCrossProjectDataRefEditDialog
        //
        this.AutoScaleDimensions = new SizeF(7f, 17f);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(484, 360);
        this.Controls.AddRange(new Control[]
        {
            lblName, this._txtName,
            lblSourceProjectId, this._txtSourceProjectId,
            lblSourceTableId, this._txtSourceTableId,
            lblTargetTableId, this._txtTargetTableId,
            lblRefMode, this._cmbRefMode,
            lblRefConfig, this._txtRefConfig,
            lblFilterConfig, this._txtFilterConfig,
            lblFormulaExpression, this._txtFormulaExpression,
            lblColumnMapping, this._txtColumnMapping,
            this._btnOk, this._btnCancel
        });
        this.Font = new Font("Microsoft YaHei", 9f);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "frmCrossProjectDataRefEditDialog";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "新增引用";
    }

    /// <summary>
    /// 验证字符串是否为有效的 JSON
    /// </summary>
    private static bool IsValidJson(string str)
    {
        str = str.Trim();
        if (string.IsNullOrEmpty(str))
            return true;
        try
        {
            var obj = Newtonsoft.Json.Linq.JToken.Parse(str);
            return true;
        }
        catch
        {
            return false;
        }
    }
}