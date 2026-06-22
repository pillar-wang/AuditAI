﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using Leqisoft.UI.Controls;
using Leqisoft.DTO;
using Leqisoft.LocalDataStore;
using Leqisoft.Model;
using Project = Leqisoft.Model.Project;

namespace Leqisoft.UI.Platform;

/// <summary>
/// 跨项目数据引用的配置管理对话框
/// </summary>
public class frmCrossProjectDataRef : Form
{
    private readonly Project _currentProject;
    private readonly Id64 _currentTableId;
    private readonly CrossProjectDataRefStore _store;
    private List<CrossProjectDataRef> _refList;

    /// <summary>
    /// 是否有引用被刷新过（关闭后由调用方检查，决定是否重新加载表格）
    /// </summary>
    public bool DataRefreshed { get; private set; }

    // 名称查询缓存
    private readonly Dictionary<Guid, string> _projectNameCache = new Dictionary<Guid, string>();
    private readonly Dictionary<(Guid, long), string> _tableNameCache = new Dictionary<(Guid, long), string>();

    // UI 控件
    private Label _lblTitle;
    private C1FlexGridEx _grid;
    private C1Button _btnEdit;
    private C1Button _btnDelete;
    private C1Button _btnToggleEnabled;
    private C1Button _btnRefreshSelected;
    private C1Button _btnRefreshAll;
    private C1Button _btnClose;
    private C1Button _btnAddNew;
    private C1Button _btnDashboard;
    private Label _lblStatusFilter;
    private C1ComboBox _cmbStatusFilter;

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

            // 应用状态筛选
            string filterText = _cmbStatusFilter?.SelectedItem?.ToString() ?? "全部";
            var filteredList = _refList;
            if (filterText != "全部")
            {
                filteredList = _refList.Where(item =>
                {
                    var mark = CrossProjectRefCellStyle.GetMark(item.Id);
                    var status = mark?.Status ?? CrossProjectRefCellStyle.RefStatus.Normal;
                    return filterText switch
                    {
                        "正常" => status == CrossProjectRefCellStyle.RefStatus.Normal,
                        "异常" => status == CrossProjectRefCellStyle.RefStatus.Error || status == CrossProjectRefCellStyle.RefStatus.DefaultValue,
                        "缓存降级" => status == CrossProjectRefCellStyle.RefStatus.CacheFallback,
                        _ => true
                    };
                }).ToList();
            }

            var authProvider = new CrossProjectRefAuthProvider(_currentProject);
            var cache = new CrossProjectRefCache(Leqisoft.Model.User.Current?.Id ?? 1);

            for (int i = 0; i < filteredList.Count; i++)
            {
                var item = filteredList[i];
                _grid.Rows.Add();
                var row = _grid.Rows[i + 1];
                row[0] = i + 1;
                row[1] = item.Name;
                row[2] = !string.IsNullOrEmpty(item.SourceProjectName) ? item.SourceProjectName : await GetProjectNameByIdAsync(item.SourceProjectId);
                row[3] = !string.IsNullOrEmpty(item.SourceTableName) ? item.SourceTableName : await GetTableNameByIdAsync(item.SourceProjectId, item.SourceTableId);
                row[4] = GetRefModeDisplay(item.RefMode);
                row[5] = item.Enabled ? "启用" : "禁用";
                row[6] = item.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                row[7] = ""; // 最后执行结果（暂空）

                // 授权状态
                try
                {
                    bool authorized = authProvider.CheckTableAccess(item.SourceProjectId, _currentProject.Id, item.SourceTableId);
                    row[8] = authorized ? "已授权" : "未授权";
                }
                catch
                {
                    row[8] = "未知";
                }

                // 缓存状态
                try
                {
                    string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data",
                        (Leqisoft.Model.User.Current?.Id ?? 1).ToString(), $"{item.SourceProjectId}.db");
                    var cachedData = cache.GetCachedData(item.Id, dbPath, 60);
                    if (cachedData != null)
                        row[9] = "已缓存";
                    else if (item.LastSourceVersion.HasValue)
                        row[9] = "过期";
                    else
                        row[9] = "未缓存";
                }
                catch
                {
                    row[9] = "未知";
                }

                // 版本号
                row[10] = item.LastSourceVersion?.ToString() ?? "-";

                // 上次验证
                row[11] = item.LastVerifiedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";

                // 目标区域 / 数据来源区域（从 RefConfig 解析）
                var (targetArea, sourceArea) = ParseRefConfigArea(item);
                row[12] = targetArea;
                row[13] = sourceArea;

                row.UserData = item;

                // 行颜色编码
                try
                {
                    var mark = CrossProjectRefCellStyle.GetMark(item.Id);
                    if (mark != null)
                    {
                        row.StyleNew.BackColor = mark.Status switch
                        {
                            CrossProjectRefCellStyle.RefStatus.CacheFallback => Color.FromArgb(255, 255, 200),
                            CrossProjectRefCellStyle.RefStatus.DefaultValue => Color.FromArgb(255, 220, 220),
                            CrossProjectRefCellStyle.RefStatus.Error => Color.FromArgb(255, 220, 220),
                            _ => Color.White
                        };
                    }
                }
                catch
                {
                    // 忽略样式设置错误
                }
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
            RefMode.ColumnRef => "整列",
            RefMode.AreaRef => "区域",
            RefMode.FormulaCompute => "公式运算",
            _ => mode.ToString()
        };
    }

    /// <summary>
    /// 从 RefConfig JSON 中解析出目标区域和来源区域的文本描述
    /// 用于在列表中显示
    /// </summary>
    private static (string targetArea, string sourceArea) ParseRefConfigArea(CrossProjectDataRef item)
    {
        if (string.IsNullOrWhiteSpace(item.RefConfig))
            return ("-", "-");

        try
        {
            var jObj = Newtonsoft.Json.Linq.JObject.Parse(item.RefConfig);

            // 根据 RefMode 解析不同格式
            switch (item.RefMode)
            {
                case RefMode.CellRef:
                    // 新格式：{"TargetCellId": 12345, "SourceCellId": 67890}
                    if (jObj["TargetCellId"] != null)
                        return ("单元格 ID:" + (long)jObj["TargetCellId"],
                                "单元格 ID:" + (jObj["SourceCellId"] != null ? ((long)jObj["SourceCellId"]).ToString() : "0"));
                    // 旧格式索引
                    return (FormatAreaDesc(jObj, "TargetColumnIndex", "TargetRowIndex"),
                            FormatAreaDesc(jObj, "SourceColumnIndex", "SourceRowIndex"));

                case RefMode.ColumnRef:
                    // 预期格式：{"TargetColumnIndex": n, "TargetColumnName": "xxx", "SourceColumnIndex": n, "SourceColumnName": "xxx"}
                    if (jObj["TargetColumnName"] != null)
                        return ($"列:{(string)jObj["TargetColumnName"]}",
                                $"列:{(string)(jObj["SourceColumnName"] ?? "?")}");
                    return ("-", "-");

                case RefMode.AreaRef:
                    // 格式：{"TargetStartCol": n, "TargetEndCol": n, "TargetStartRow": n, "TargetEndRow": n,
                    //        "SourceStartCol": n, "SourceEndCol": n, "SourceStartRow": n, "SourceEndRow": n}
                    if (jObj["TargetStartCol"] != null)
                    {
                        string tgt = $"{jObj["TargetStartCol"]}-{jObj["TargetEndCol"]}列 × {jObj["TargetStartRow"]}-{jObj["TargetEndRow"]}行";
                        string src = $"{jObj["SourceStartCol"]}-{jObj["SourceEndCol"]}列 × {jObj["SourceStartRow"]}-{jObj["SourceEndRow"]}行";
                        return (tgt, src);
                    }
                    return ("-", "-");

                case RefMode.FormulaCompute:
                    // 格式：{"TargetColumnIndex": n, "TargetColumnName": "xxx", "SourceColumnNames": [...]}
                    if (jObj["TargetColumnName"] != null)
                    {
                        string tgt = $"列:{(string)jObj["TargetColumnName"]}";
                        var srcCols = jObj["SourceColumnNames"];
                        string src = srcCols != null ? string.Join(",", srcCols.Values<string>()) : "-";
                        return (tgt, src);
                    }
                    return ("-", "-");

                default:
                    return ("-", "-");
            }
        }
        catch
        {
            return ("-", "-");
        }
    }

    /// <summary>
    /// 从 JObject 中读取列索引和行索引，格式化为列字母+行号描述
    /// </summary>
    private static string FormatAreaDesc(Newtonsoft.Json.Linq.JObject jObj, string colKey, string rowKey)
    {
        if (jObj[colKey] == null || jObj[rowKey] == null)
            return "-";
        int col = (int)jObj[colKey];
        int row = (int)jObj[rowKey];
        // 将列索引转为 Excel 风格字母 (A, B, C...)
        string colLetter = "";
        int tmp = col + 1;
        while (tmp > 0)
        {
            tmp--;
            colLetter = (char)('A' + tmp % 26) + colLetter;
            tmp /= 26;
        }
        return $"{colLetter}{row + 1}";
    }

    /// <summary>
    /// 通过项目ID获取项目名称（同步版本，用于非 UI 线程调用）
    /// </summary>
    internal static string GetProjectNameById(Guid projectId)
    {
        try
        {
            var projects = Task.Run(async () => await Leqisoft.LocalDataStore.StorageRouter.GetProjects()).GetAwaiter().GetResult();
            var project = projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null)
                return project.Name ?? projectId.ToString();

            string dbPath = MainForm.GetDbPathByGuid(projectId);
            if (!System.IO.File.Exists(dbPath))
                return projectId.ToString();

            var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
            var projectDto = dal.GetProject();
            return projectDto?.Name ?? projectId.ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取项目名称失败: {ex.Message}");
            return projectId.ToString();
        }
    }

    /// <summary>
    /// 通过项目ID和表ID获取表格名称（同步版本，用于非 UI 线程调用）
    /// </summary>
    internal static string GetTableNameById(Guid projectId, Id64 tableId)
    {
        try
        {
            string dbPath = MainForm.GetDbPathByGuid(projectId);
            if (!System.IO.File.Exists(dbPath))
                return tableId.Value.ToString();

            var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
            var dto = dal.GetProject();
            if (dto == null)
                return tableId.Value.ToString();

            var project = new Leqisoft.Model.Project
            {
                Id = projectId,
                Name = dto.Name,
                Dal = dal
            };
            project.PopulateFieldsFromDto(dto);
            project.Load();

            var tableNode = project.GetAllTableNodes().FirstOrDefault(n => n.Id == tableId);
            if (tableNode != null)
                return tableNode.Number + " " + tableNode.Name;

            var tableDto = dal.GetTable(tableId);
            return tableDto?.Title ?? tableId.Value.ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取表名称失败: {ex.Message}");
            return tableId.Value.ToString();
        }
    }

    /// <summary>
    /// 通过项目ID异步获取项目名称（带缓存）
    /// </summary>
    private async Task<string> GetProjectNameByIdAsync(Guid projectId)
    {
        if (_projectNameCache.TryGetValue(projectId, out string cachedName))
            return cachedName;

        try
        {
            var projects = await Leqisoft.LocalDataStore.StorageRouter.GetProjects();
            var project = projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null)
            {
                _projectNameCache[projectId] = project.Name ?? projectId.ToString();
                return _projectNameCache[projectId];
            }

            string dbPath = MainForm.GetDbPathByGuid(projectId);
            if (!System.IO.File.Exists(dbPath))
            {
                _projectNameCache[projectId] = projectId.ToString();
                return _projectNameCache[projectId];
            }

            var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
            var projectDto = dal.GetProject();
            _projectNameCache[projectId] = projectDto?.Name ?? projectId.ToString();
            return _projectNameCache[projectId];
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取项目名称失败: {ex.Message}");
            return projectId.ToString();
        }
    }

    /// <summary>
    /// 通过项目ID和表ID异步获取表格名称（带缓存）
    /// </summary>
    private async Task<string> GetTableNameByIdAsync(Guid projectId, Id64 tableId)
    {
        var key = (projectId, tableId.Value);
        if (!_tableNameCache.TryGetValue(key, out string cachedName))
        {
            cachedName = await LoadTableNameFromDbAsync(projectId, tableId);
            _tableNameCache[key] = cachedName;
        }
        return cachedName;
    }

    /// <summary>
    /// 从数据库加载表格名称
    /// </summary>
    private async Task<string> LoadTableNameFromDbAsync(Guid projectId, Id64 tableId)
    {
        try
        {
            string dbPath = MainForm.GetDbPathByGuid(projectId);
            if (!System.IO.File.Exists(dbPath))
                return tableId.Value.ToString();

            var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
            var dto = dal.GetProject();
            if (dto == null)
                return tableId.Value.ToString();

            var project = new Leqisoft.Model.Project
            {
                Id = projectId,
                Name = dto.Name,
                Dal = dal
            };
            project.PopulateFieldsFromDto(dto);
            project.Load();

            var tableNode = project.GetAllTableNodes().FirstOrDefault(n => n.Id == tableId);
            if (tableNode != null)
                return tableNode.Number + " " + tableNode.Name;

            var tableDto = dal.GetTable(tableId);
            return tableDto?.Title ?? tableId.Value.ToString();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"获取表名称失败: {ex.Message}");
            return tableId.Value.ToString();
        }
    }

    /// <summary>
    /// 获取当前项目中目标表的名称
    /// </summary>
    private string GetTargetTableName(Id64 tableId)
    {
        try
        {
            var table = _currentProject.GetTableById(tableId);
            if (table != null)
                return table.TreeNode?.Name ?? tableId.Value.ToString();
            
            string dbPath = MainForm.GetDbPathByGuid(_currentProject.Id);
            if (!System.IO.File.Exists(dbPath))
                return tableId.Value.ToString();

            var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
            var tableDto = dal.GetTable(tableId);
            return tableDto?.Title ?? tableId.Value.ToString();
        }
        catch
        {
            return tableId.Value.ToString();
        }
    }

    /// <summary>
    /// 弹出编辑对话框（使用向导）
    /// </summary>
    private void ShowEditDialog(CrossProjectDataRef item)
    {
        if (item == null) return;

        var result = frmCrossProjectRefConfigWizard.ShowWizard(_currentProject, _currentTableId, item);
        if (result == DialogResult.OK)
        {
            // 向导内部已自行保存
            _ = RefreshList();
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
                DataRefreshed = true;
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
            int success = results.Results.Count(r => r.Success);
            int failed = results.Results.Count(r => !r.Success);
            if (success > 0) DataRefreshed = true;
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

    private async void _btnAddNew_Click(object sender, EventArgs e)
    {
        try
        {
            var result = frmCrossProjectRefConfigWizard.ShowWizard(_currentProject, _currentTableId, null);
            if (result == DialogResult.OK)
            {
                await RefreshList();
            }
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"新增引用失败: {ex.Message}");
        }
    }

    private void _btnDashboard_Click(object sender, EventArgs e)
    {
        try
        {
            frmCrossProjectRefStatus.ShowDashboard(_currentProject);
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"打开状态仪表板失败: {ex.Message}");
        }
    }

    private async void _cmbStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        await RefreshList();
    }

    private async void _grid_AfterEdit(object sender, C1.Win.C1FlexGrid.RowColEventArgs e)
    {
        if (e.Col != 1) return; // 仅处理引用名称列

        try
        {
            int rowIdx = e.Row;
            if (rowIdx > 0 && rowIdx <= _grid.Rows.Count - 1)
            {
                var item = _grid.Rows[rowIdx].UserData as CrossProjectDataRef;
                if (item != null)
                {
                    var newName = _grid.Rows[rowIdx][1]?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(newName) && newName != item.Name)
                    {
                        item.Name = newName;
                        item.UpdatedAt = DateTime.Now;
                        await _store.Save(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"保存名称失败: {ex.Message}");
        }
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
        this._lblTitle = new Label();
        this._grid = new C1FlexGridEx();
        this._btnEdit = new C1Button();
        this._btnDelete = new C1Button();
        this._btnToggleEnabled = new C1Button();
        this._btnRefreshSelected = new C1Button();
        this._btnRefreshAll = new C1Button();
        this._btnClose = new C1Button();
        this._btnAddNew = new C1Button();
        this._btnDashboard = new C1Button();
        
        this._lblStatusFilter = new Label();
        this._cmbStatusFilter = new C1ComboBox();

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

        //
        // _grid
        //
        this._grid.AllowEditing = false;
        this._grid.AllowFiltering = false;
        this._grid.AllowSorting = AllowSortingEnum.None;
        this._grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this._grid.BackColor = Color.White;
        this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.FixedSingle;
        this._grid.Cols.Count = 14;
        this._grid.Cols[0].Caption = "序号";
        this._grid.Cols[0].AllowSorting = false;
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
        this._grid.Cols[8].Caption = "授权状态";
        this._grid.Cols[8].Width = 80;
        this._grid.Cols[9].Caption = "缓存状态";
        this._grid.Cols[9].Width = 80;
        this._grid.Cols[10].Caption = "版本号";
        this._grid.Cols[10].Width = 70;
        this._grid.Cols[11].Caption = "上次验证";
        this._grid.Cols[11].Width = 150;
        this._grid.Cols[12].Caption = "目标区域";
        this._grid.Cols[12].Width = 120;
        this._grid.Cols[13].Caption = "数据来源区域";
        this._grid.Cols[13].Width = 120;
        this._grid.Cols.Fixed = 1;
        this._grid.Cols[1].AllowEditing = true;
        this._grid.ExtendLastCol = true;
        this._grid.Location = new Point(12, 50);
        this._grid.Name = "_grid";
        this._grid.Rows.Count = 1;
        this._grid.Rows.Fixed = 1;
        this._grid.Rows.DefaultSize = 30;
        this._grid.SelectionMode = SelectionModeEnum.Row;
        this._grid.Size = new Size(960, 380);
        this._grid.Styles.Normal.Font = new Font("Microsoft YaHei", 10f);
        this._grid.Styles.Normal.TextAlign = TextAlignEnum.CenterCenter;
        this._grid.Styles.Fixed.Font = new Font("Microsoft YaHei", 10f, FontStyle.Bold);
        this._grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
        this._grid.Styles.EmptyArea.BackColor = Color.White;
        this._grid.TabIndex = 1;
        this._grid.AfterEdit += _grid_AfterEdit;

        //
        // 底部按钮面板
        //
        var pnlButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            BackColor = Color.FromArgb(245, 245, 245)
        };
        var pnlBorder = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(220, 220, 220)
        };
        pnlButtons.Controls.Add(pnlBorder);

        // 左侧操作按钮组
        var btnX = 12;
        var btnY = 11;
        var btnW = 87;
        var btnH = 33;
        var btnGap = 6;

        Action<C1Button> layoutLeft = (btn) => {
            btn.Font = new Font("Microsoft YaHei", 9f);
            btn.Location = new Point(btnX, btnY);
            btn.Size = new Size(btnW, btnH);
            btn.UseVisualStyleBackColor = true;
            pnlButtons.Controls.Add(btn);
            btnX += btnW + btnGap;
        };

        this._btnAddNew.Text = "新增引用";
        this._btnAddNew.TabIndex = 9;
        this._btnAddNew.Click += _btnAddNew_Click;
        layoutLeft(_btnAddNew);

        this._btnEdit.Text = "编辑";
        this._btnEdit.TabIndex = 3;
        this._btnEdit.Click += _btnEdit_Click;
        layoutLeft(_btnEdit);

        this._btnDelete.Text = "删除";
        this._btnDelete.TabIndex = 4;
        this._btnDelete.Click += _btnDelete_Click;
        layoutLeft(_btnDelete);

        this._btnToggleEnabled.Text = "启用/禁用";
        this._btnToggleEnabled.TabIndex = 5;
        this._btnToggleEnabled.Click += _btnToggleEnabled_Click;
        layoutLeft(_btnToggleEnabled);

        this._btnRefreshSelected.Text = "刷新选定";
        this._btnRefreshSelected.TabIndex = 6;
        this._btnRefreshSelected.Click += _btnRefreshSelected_Click;
        layoutLeft(_btnRefreshSelected);

        this._btnRefreshAll.Text = "刷新所有";
        this._btnRefreshAll.TabIndex = 7;
        this._btnRefreshAll.Click += _btnRefreshAll_Click;
        layoutLeft(_btnRefreshAll);

        this._btnDashboard.Text = "状态仪表板";
        this._btnDashboard.TabIndex = 10;
        this._btnDashboard.Click += _btnDashboard_Click;
        layoutLeft(_btnDashboard);

        // 右侧关闭按钮
        this._btnClose.Text = "关闭";
        this._btnClose.Anchor = AnchorStyles.Right;
        this._btnClose.Font = new Font("Microsoft YaHei", 9f);
        this._btnClose.Location = new Point(885, 11);
        this._btnClose.Size = new Size(btnW, btnH);
        this._btnClose.UseVisualStyleBackColor = true;
        this._btnClose.TabIndex = 13;
        this._btnClose.Click += _btnClose_Click;
        pnlButtons.Controls.Add(_btnClose);

        // 状态筛选 ComboBox 改到右上角标题行

        //
        // _lblStatusFilter
        //
        this._lblStatusFilter.AutoSize = true;
        this._lblStatusFilter.BackColor = Color.Transparent;
        this._lblStatusFilter.Font = new Font("Microsoft YaHei", 9f);
        this._lblStatusFilter.Location = new Point(820, 17);
        this._lblStatusFilter.Name = "_lblStatusFilter";
        this._lblStatusFilter.Size = new Size(56, 17);
        this._lblStatusFilter.TabIndex = 14;
        this._lblStatusFilter.Text = "状态筛选:";

        //
        // _cmbStatusFilter
        //
        this._cmbStatusFilter.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
        this._cmbStatusFilter.Font = new Font("Microsoft YaHei", 9f);
        this._cmbStatusFilter.Items.AddRange(new object[] { "全部", "正常", "异常", "缓存降级" });
        this._cmbStatusFilter.Location = new Point(876, 14);
        this._cmbStatusFilter.Name = "_cmbStatusFilter";
        this._cmbStatusFilter.Size = new Size(96, 24);
        this._cmbStatusFilter.TabIndex = 15;
        this._cmbStatusFilter.SelectedIndex = 0;
        this._cmbStatusFilter.SelectedIndexChanged += _cmbStatusFilter_SelectedIndexChanged;

        //
        // frmCrossProjectDataRef
        //
        this.AutoScaleDimensions = new SizeF(7f, 17f);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(984, 530);
        this.Controls.Add(this._lblTitle);
        this.Controls.Add(this._grid);
        this.Controls.Add(pnlButtons);
        this.Controls.Add(this._lblStatusFilter);
        this.Controls.Add(this._cmbStatusFilter);
        this.Font = new Font("Microsoft YaHei", 9f);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "frmCrossProjectDataRef";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "跨项目数据引用管理";
    }
}

/// <summary>
/// 新增/编辑引用对话框
/// 使用弹出窗口选择项目和表格，而非直接输入 ID
/// </summary>
internal class frmCrossProjectDataRefEditDialog : Form
    {
        private readonly CrossProjectDataRef _existing;
        private readonly Id64 _currentTableId;
        private readonly Leqisoft.Model.Project _currentProject;

        // 私有状态
        private Leqisoft.DTO.Project _selectedProject;
        private TreeTableNode _selectedTableNode;

        // UI 控件
        private TextBox _txtName;
        private TextBox _txtSourceProject;          // 显示选中的项目名
        private C1Button _btnSelectProject;           // 点击弹出项目选择
        private TextBox _txtSourceTable;            // 显示选中的表名
        private C1Button _btnSelectTable;             // 点击弹出表格选择
        private TextBox _txtTargetTableId;
        private C1ComboBox _cmbRefMode;
        private TextBox _txtRefConfig;
        private TextBox _txtFilterConfig;
        private TextBox _txtFormulaExpression;
        private TextBox _txtColumnMapping;
        private C1Button _btnOk;
        private C1Button _btnCancel;

        public frmCrossProjectDataRefEditDialog(string title, CrossProjectDataRef existing, Id64 currentTableId, Leqisoft.Model.Project currentProject)
        {
            _existing = existing;
            _currentTableId = currentTableId;
            _currentProject = currentProject;
            InitializeComponent();
            this.Text = title;

            _cmbRefMode.Items.AddRange(new object[] { "单元格", "整列", "区域", "公式运算" });
            _cmbRefMode.SelectedIndex = 0;

            if (_existing != null)
            {
                LoadFromExisting();
            }
            else
            {
                _txtTargetTableId.Text = GetTargetTableName(_currentTableId);
            }
        }

        private string GetTargetTableName(Id64 tableId)
        {
            try
            {
                var table = _currentProject?.GetTableById(tableId);
                if (table != null)
                    return table.TreeNode?.Name ?? tableId.Value.ToString();
                
                if (_currentProject != null)
                {
                    string dbPath = MainForm.GetDbPathByGuid(_currentProject.Id);
                    if (System.IO.File.Exists(dbPath))
                    {
                        var dal = new Leqisoft.DTO.ProjectDAL(dbPath);
                        var tableDto = dal.GetTable(tableId);
                        return tableDto?.Title ?? tableId.Value.ToString();
                    }
                }
                return tableId.Value.ToString();
            }
            catch
            {
                return tableId.Value.ToString();
            }
        }

    /// <summary>
    /// 点击"选择项目"按钮 — 弹出 frmSelectProject 对话���
    /// </summary>
    private void _btnSelectProject_Click(object sender, EventArgs e)
    {
        using (var frm = new frmSelectProject(Program.MainForm.CurrentProject.Id))
        {
            if (frm.ShowDialog(this) == DialogResult.OK && frm.SelectedProject != null)
            {
                _selectedProject = frm.SelectedProject;
                _txtSourceProject.Text = $"{_selectedProject.Name} ({_selectedProject.Number})";

                // 清空之前选择的表
                _selectedTableNode = null;
                _txtSourceTable.Clear();
            }
        }
    }

    /// <summary>
    /// 点击"选择表"按钮 — 加载外部项目的表格树，弹出 TreeView 选择
    /// </summary>
    private void _btnSelectTable_Click(object sender, EventArgs e)
    {
        if (_selectedProject == null)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择来源项目");
            return;
        }

        // 加载外部项目的表格节点
        var tableNodes = LoadTableNodesForProject(_selectedProject);
        if (tableNodes == null || tableNodes.Count == 0)
        {
            Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该项目没有可用的表格");
            return;
        }

        // 弹出 TreeView 选择窗口
        using (var tableForm = new Form())
        {
            tableForm.Text = "选择来源表 - " + _selectedProject.Name;
            tableForm.Size = new Size(450, 420);
            tableForm.StartPosition = FormStartPosition.CenterParent;
            tableForm.MinimizeBox = false;
            tableForm.MaximizeBox = false;
            tableForm.FormBorderStyle = FormBorderStyle.FixedDialog;

            var treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.Font = new Font("Microsoft YaHei", 9f);
            tableForm.Controls.Add(treeView);

            var btnOk = new Button();
            btnOk.Text = "选择此表格";
            btnOk.Dock = DockStyle.Bottom;
            btnOk.Height = 36;
            btnOk.Font = new Font("Microsoft YaHei", 9f);
            btnOk.Click += (s, ev) =>
            {
                if (treeView.SelectedNode?.Tag is TreeTableNode tableNode)
                {
                    _selectedTableNode = tableNode;
                    _txtSourceTable.Text = tableNode.Name;
                    tableForm.DialogResult = DialogResult.OK;
                    tableForm.Close();
                }
                else
                {
                    Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择一个表格");
                }
            };
            tableForm.Controls.Add(btnOk);

            // 填充树
            foreach (var node in tableNodes)
            {
                var tn = treeView.Nodes.Add(node.Name);
                tn.Tag = node;
            }

            // 自动展开所有节点
            treeView.ExpandAll();

            tableForm.ShowDialog(this);
        }
    }

    /// <summary>
    /// 打开外部项目并获取所有表格节点
    /// </summary>
    private static List<TreeTableNode> LoadTableNodesForProject(Leqisoft.DTO.Project projectDto)
    {
        try
        {
            string dbPath = MainForm.GetDbPathByGuid(projectDto.Id);
            if (!System.IO.File.Exists(dbPath))
                return new List<TreeTableNode>();

            var dal = new ProjectDAL(dbPath);
            var dto = dal.GetProject();
            if (dto == null)
                return new List<TreeTableNode>();

            var project = new Project
            {
                Id = dto.Id,
                Name = dto.Name
            };
            project.Dal = dal;
            project.PopulateFieldsFromDto(dto);
            project.Load();

            var nodes = project.GetAllTableNodes();
            return nodes?.ToList() ?? new List<TreeTableNode>();
        }
        catch
        {
            return new List<TreeTableNode>();
        }
    }

    private void LoadFromExisting()
    {
        _txtName.Text = _existing.Name;

        // 编辑模式下用已有数据显示，获取项目名称和表格名称
        string projectName = frmCrossProjectDataRef.GetProjectNameById(_existing.SourceProjectId);
        string tableName = frmCrossProjectDataRef.GetTableNameById(_existing.SourceProjectId, _existing.SourceTableId);

        _selectedProject = new Leqisoft.DTO.Project
        {
            Id = _existing.SourceProjectId,
            Name = projectName
        };
        _txtSourceProject.Text = projectName;
        _txtSourceProject.ReadOnly = true;

        _selectedTableNode = new TreeTableNode { Name = tableName };
        _selectedTableNode.Id = _existing.SourceTableId;
        _selectedTableNode.SetTable(new Leqisoft.Model.Table());
        _txtSourceTable.Text = tableName;
        _txtSourceTable.ReadOnly = true;

        // 编辑模式下禁用选择按钮
        _btnSelectProject.Enabled = false;
        _btnSelectTable.Enabled = false;

        _txtTargetTableId.Text = !string.IsNullOrEmpty(_existing.TargetTableName) ? _existing.TargetTableName : GetTargetTableName(_existing.TargetTableId);
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

        if (_selectedProject != null)
            result.SourceProjectId = _selectedProject.Id;

        if (_selectedTableNode != null)
            result.SourceTableId = _selectedTableNode.Table.Id;

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
        this._txtSourceProject = new TextBox();
        this._btnSelectProject = new C1Button();
        this._txtSourceTable = new TextBox();
        this._btnSelectTable = new C1Button();
        this._txtTargetTableId = new TextBox();
        this._cmbRefMode = new C1ComboBox();
        this._txtRefConfig = new TextBox();
        this._txtFilterConfig = new TextBox();
        this._txtFormulaExpression = new TextBox();
        this._txtColumnMapping = new TextBox();
        this._btnOk = new C1Button();
        this._btnCancel = new C1Button();

        var lblName = new Label { Text = "引用名称：", Location = new Point(12, 15), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblSourceProject = new Label { Text = "来源项目：", Location = new Point(12, 48), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
        var lblSourceTable = new Label { Text = "来源表：", Location = new Point(12, 81), Size = new Size(100, 24), Font = new Font("Microsoft YaHei", 9f) };
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
        // _txtSourceProject
        //
        this._txtSourceProject.Font = new Font("Microsoft YaHei", 9f);
        this._txtSourceProject.Location = new Point(118, 45);
        this._txtSourceProject.Name = "_txtSourceProject";
        this._txtSourceProject.ReadOnly = true;
        this._txtSourceProject.Size = new Size(260, 24);
        this._txtSourceProject.TabIndex = 1;
        this._txtSourceProject.Text = "(点击右侧按钮选择)";

        //
        // _btnSelectProject
        //
        this._btnSelectProject.Font = new Font("Microsoft YaHei", 9f);
        this._btnSelectProject.Location = new Point(382, 44);
        this._btnSelectProject.Name = "_btnSelectProject";
        this._btnSelectProject.Size = new Size(90, 26);
        this._btnSelectProject.TabIndex = 12;
        this._btnSelectProject.Text = "选择...";
        this._btnSelectProject.Click += _btnSelectProject_Click;

        //
        // _txtSourceTable
        //
        this._txtSourceTable.Font = new Font("Microsoft YaHei", 9f);
        this._txtSourceTable.Location = new Point(118, 78);
        this._txtSourceTable.Name = "_txtSourceTable";
        this._txtSourceTable.ReadOnly = true;
        this._txtSourceTable.Size = new Size(260, 24);
        this._txtSourceTable.TabIndex = 2;
        this._txtSourceTable.Text = "(请先选择来源项目)";

        //
        // _btnSelectTable
        //
        this._btnSelectTable.Font = new Font("Microsoft YaHei", 9f);
        this._btnSelectTable.Location = new Point(382, 77);
        this._btnSelectTable.Name = "_btnSelectTable";
        this._btnSelectTable.Size = new Size(90, 26);
        this._btnSelectTable.TabIndex = 13;
        this._btnSelectTable.Text = "选择...";
        this._btnSelectTable.Click += _btnSelectTable_Click;

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
        this._cmbRefMode.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
        this._cmbRefMode.Font = new Font("Microsoft YaHei", 9f);
        this._cmbRefMode.Location = new Point(118, 144);
        this._cmbRefMode.Name = "_cmbRefMode";
        this._cmbRefMode.Size = new Size(350, 24);
        this._cmbRefMode.TabIndex = 4;

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
            if (this._selectedProject == null)
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请选择来源项目");
                return;
            }
            if (this._selectedTableNode == null)
            {
                Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请选择来源表");
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
            lblSourceProject, this._txtSourceProject,
            this._btnSelectProject,
            lblSourceTable, this._txtSourceTable,
            this._btnSelectTable,
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
