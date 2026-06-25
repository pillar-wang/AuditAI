﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using Auditai.UI.Controls;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using Project = Auditai.Model.Project;

namespace Auditai.UI.Platform;

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
    private Panel _pnlButtons;

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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"初始化失败: {ex.Message}");
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

            for (int i = 0; i < filteredList.Count; i++)
            {
                var item = filteredList[i];
                _grid.Rows.Add();
                var row = _grid.Rows[i + 1];
                row[0] = i + 1;           // 序号
                row[1] = item.Name;       // 引用名称

                // 目标表
                row[2] = !string.IsNullOrEmpty(item.TargetTableName) ? item.TargetTableName : GetTargetTableName(item.TargetTableId);

                // 数据来源区域 / 目标区域（从 RefConfig 解析）
                var (targetArea, sourceArea) = ParseRefConfigArea(item);
                row[3] = targetArea;      // 目标区域

                row[4] = !string.IsNullOrEmpty(item.SourceProjectName) ? item.SourceProjectName : await GetProjectNameByIdAsync(item.SourceProjectId);  // 来源项目
                row[5] = !string.IsNullOrEmpty(item.SourceTableName) ? item.SourceTableName : await GetTableNameByIdAsync(item.SourceProjectId, item.SourceTableId);  // 来源表
                row[6] = sourceArea;      // 数据来源区域

                row[7] = GetRefModeDisplay(item.RefMode);  // 引用模式
                row[8] = item.Enabled ? "启用" : "禁用";   // 启用状态
                row[9] = item.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");  // 更新时间

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
            // 确保目标区域和数据来源区域列有足够宽度（AutoSizeCols 可能压缩）
            if (_grid.Cols.Count > 9)
            {
                _grid.Cols[3].Width = Math.Max(_grid.Cols[3].Width, 120);  // 目标区域
                _grid.Cols[6].Width = Math.Max(_grid.Cols[6].Width, 120);  // 数据来源区域
            }
        }
        catch (Exception ex)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"加载引用列表失败: {ex.Message}");
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
                    // 格式：{"TargetCellId": 12345, "SourceCellId": 67890, "TargetRow": r, "TargetCol": c, "SourceRow": r, "SourceCol": c}
                    if (jObj["TargetCellId"] != null)
                    {
                        // 使用行列信息，显示为 A1 格式
                        var tgtRow = jObj["TargetRow"]?.ToObject<int>();
                        var tgtCol = jObj["TargetCol"]?.ToObject<int>();
                        var srcRow = jObj["SourceRow"]?.ToObject<int>();
                        var srcCol = jObj["SourceCol"]?.ToObject<int>();

                        string targetPos = (tgtRow.HasValue && tgtCol.HasValue)
                            ? ToCellRef(tgtRow.Value, tgtCol.Value)
                            : "-";
                        string sourcePos = (srcRow.HasValue && srcCol.HasValue)
                            ? ToCellRef(srcRow.Value, srcCol.Value)
                            : "-";

                        return (targetPos, sourcePos);
                    }
                    return ("-", "-");

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
                        int tsc = jObj["TargetStartCol"].ToObject<int>();
                        int tec = jObj["TargetEndCol"].ToObject<int>();
                        int tsr = jObj["TargetStartRow"].ToObject<int>();
                        int ter = jObj["TargetEndRow"].ToObject<int>();
                        int ssc = jObj["SourceStartCol"].ToObject<int>();
                        int sec = jObj["SourceEndCol"].ToObject<int>();
                        int ssr = jObj["SourceStartRow"].ToObject<int>();
                        int ser = jObj["SourceEndRow"].ToObject<int>();
                        string tgt = tsc == tec && tsr == ter
                            ? ToCellRef(tsr, tsc)
                            : $"{ToCellRef(tsr, tsc)}:{ToCellRef(ter, tec)}";
                        string src = ssc == sec && ssr == ser
                            ? ToCellRef(ssr, ssc)
                            : $"{ToCellRef(ssr, ssc)}:{ToCellRef(ser, sec)}";
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
    /// 将行列索引转为 Excel 风格的单元格引用（如 0,0 → A1）
    /// </summary>
    private static string ToCellRef(int row, int col)
    {
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
            var projects = Task.Run(async () => await Auditai.LocalDataStore.StorageRouter.GetProjects()).GetAwaiter().GetResult();
            var project = projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null)
                return project.Name ?? projectId.ToString();

            string dbPath = MainForm.GetDbPathByGuid(projectId);
            if (!System.IO.File.Exists(dbPath))
                return projectId.ToString();

            var dal = new Auditai.DTO.ProjectDAL(dbPath);
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

            var dal = new Auditai.DTO.ProjectDAL(dbPath);
            var dto = dal.GetProject();
            if (dto == null)
                return tableId.Value.ToString();

            var project = new Auditai.Model.Project
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
            var projects = await Auditai.LocalDataStore.StorageRouter.GetProjects();
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

            var dal = new Auditai.DTO.ProjectDAL(dbPath);
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
        return await Task.Run(() =>
        {
            try
            {
                string dbPath = MainForm.GetDbPathByGuid(projectId);
                if (!System.IO.File.Exists(dbPath))
                    return tableId.Value.ToString();

                var dal = new Auditai.DTO.ProjectDAL(dbPath);
                var dto = dal.GetProject();
                if (dto == null)
                    return tableId.Value.ToString();

                var project = new Auditai.Model.Project
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
        });
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

            var dal = new Auditai.DTO.ProjectDAL(dbPath);
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
            DataRefreshed = true;
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"保存失败: {ex.Message}");
        }
    }

    // ---- 事件处理 ----

    private void _btnEdit_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
            return;
        }
        ShowEditDialog(selected);
    }

    private async void _btnDelete_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
            return;
        }

        var confirm = Auditai.UI.Controls.MessageBox.Show(
            MessageBoxIcon.Question,
            $"确定要删除引用 \"{selected.Name}\" 吗？",
            MessageBoxButtons.YesNo,
            "确认删除");

        if (confirm == DialogResult.Yes)
        {
            try
            {
                await _store.Delete(selected.Id);
                DataRefreshed = true;
                await RefreshList();
            }
            catch (Exception ex)
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"删除失败: {ex.Message}");
            }
        }
    }

    private async void _btnToggleEnabled_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"保存失败: {ex.Message}");
        }
    }

    private async void _btnRefreshSelected_Click(object sender, EventArgs e)
    {
        var selected = GetSelectedRef();
        if (selected == null)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择一条引用记录");
            return;
        }

        try
        {
            var manager = new CrossProjectDataRefManager(_currentProject);
            var result = await manager.ExecuteRef(selected);
            if (result.Success)
            {
                DataRefreshed = true;
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"刷新完成，影响 {result.AffectedRows} 行");
            }
            else
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, $"刷新失败: {result.ErrorMessage}");
            }
            await RefreshList();
        }
        catch (Exception ex)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"刷新失败: {ex.Message}");
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
            Auditai.UI.Controls.MessageBox.Show(
                MessageBoxIcon.None,
                $"全部刷新完成：成功 {success} 个，失败 {failed} 个");
            await RefreshList();
        }
        catch (Exception ex)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"刷新失败: {ex.Message}");
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
                DataRefreshed = true;
                await RefreshList();
            }
        }
        catch (Exception ex)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"新增引用失败: {ex.Message}");
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"打开状态仪表板失败: {ex.Message}");
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Error, $"保存名称失败: {ex.Message}");
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

        // ---- 字体常量 ----
        var fontTitle = new Font("Microsoft YaHei", 11f, FontStyle.Bold);
        var fontNormal = new Font("Microsoft YaHei", 9f);
        var fontBtn = new Font("Microsoft YaHei", 9f);
        var fontBtnBold = new Font("Microsoft YaHei", 9f, FontStyle.Bold);

        // ---- 颜色常量（与向导/主界面一致） ----
        var colorHeaderBg = Color.FromArgb(0, 120, 215);       // 顶部标题栏深蓝
        var colorHeaderFg = Color.White;                        // 顶部标题栏白字
        var colorBtnPrimary = Color.FromArgb(0, 120, 215);      // 主操作按钮蓝
        var colorBtnRefresh = Color.FromArgb(0, 150, 80);       // 刷新按钮绿
        var colorBtnDanger = Color.FromArgb(200, 80, 80);       // 删除按钮红
        var colorBtnNormal = Color.FromArgb(255, 255, 255);     // 普通按钮白
        var colorBtnNormalFg = Color.FromArgb(60, 60, 60);      // 普通按钮深灰字
        var colorBtnBorder = Color.FromArgb(200, 200, 200);     // 按钮边框灰
        var colorBottomBg = Color.FromArgb(248, 249, 251);      // 底部按钮区浅灰
        var colorGridFixedBg = Color.FromArgb(243, 245, 248);   // 表头浅灰
        var colorGridAltBg = Color.FromArgb(248, 249, 251);     // 隔行浅灰

        // ---- 顶部标题栏（深蓝背景 + 白色标题 + 右侧状态筛选） ----
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = colorHeaderBg
        };
        this._lblTitle.AutoSize = false;
        this._lblTitle.BackColor = Color.Transparent;
        this._lblTitle.BorderStyle = BorderStyle.None;
        this._lblTitle.Font = fontTitle;
        this._lblTitle.ForeColor = colorHeaderFg;
        this._lblTitle.Dock = DockStyle.Left;
        this._lblTitle.Size = new Size(300, 48);
        this._lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        this._lblTitle.Padding = new Padding(16, 0, 0, 0);
        this._lblTitle.Text = "跨项目数据引用管理";

        // 状态筛选放标题栏右侧
        this._lblStatusFilter.AutoSize = false;
        this._lblStatusFilter.BackColor = Color.Transparent;
        this._lblStatusFilter.Font = fontNormal;
        this._lblStatusFilter.ForeColor = colorHeaderFg;
        this._lblStatusFilter.Dock = DockStyle.Right;
        this._lblStatusFilter.Size = new Size(70, 48);
        this._lblStatusFilter.TextAlign = ContentAlignment.MiddleRight;
        this._lblStatusFilter.Text = "状态筛选";

        this._cmbStatusFilter.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
        this._cmbStatusFilter.Font = fontNormal;
        this._cmbStatusFilter.Items.AddRange(new object[] { "全部", "正常", "异常", "缓存降级" });
        this._cmbStatusFilter.Dock = DockStyle.Right;
        this._cmbStatusFilter.Size = new Size(110, 48);
        this._cmbStatusFilter.SelectedIndex = 0;
        this._cmbStatusFilter.SelectedIndexChanged += _cmbStatusFilter_SelectedIndexChanged;
        this._cmbStatusFilter.Margin = new Padding(0, 12, 16, 12);

        pnlHeader.Controls.Add(this._cmbStatusFilter);
        pnlHeader.Controls.Add(this._lblStatusFilter);
        pnlHeader.Controls.Add(this._lblTitle);

        // ---- 中间网格区域 ----
        this._grid.AllowEditing = false;
        this._grid.AllowFiltering = false;
        this._grid.AllowSorting = AllowSortingEnum.None;
        this._grid.Dock = DockStyle.Fill;
        this._grid.BackColor = Color.White;
        this._grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
        this._grid.Cols.Count = 10;
        this._grid.Cols[0].Caption = "序号";
        this._grid.Cols[1].Caption = "引用名称";
        this._grid.Cols[1].Width = 150;
        this._grid.Cols[2].Caption = "目标表";
        this._grid.Cols[2].Width = 80;
        this._grid.Cols[3].Caption = "目标区域";
        this._grid.Cols[3].Width = 120;
        this._grid.Cols[4].Caption = "来源项目";
        this._grid.Cols[4].Width = 230;
        this._grid.Cols[5].Caption = "来源表";
        this._grid.Cols[5].Width = 80;
        this._grid.Cols[6].Caption = "数据来源区域";
        this._grid.Cols[6].Width = 120;
        this._grid.Cols[7].Caption = "引用模式";
        this._grid.Cols[7].Width = 80;
        this._grid.Cols[8].Caption = "启用状态";
        this._grid.Cols[8].Width = 70;
        this._grid.Cols[9].Caption = "更新时间";
        this._grid.Cols[9].Width = 150;
        this._grid.Cols.Fixed = 1;
        this._grid.Cols[1].AllowEditing = true;
        this._grid.ExtendLastCol = true;
        this._grid.Rows.Count = 1;
        this._grid.Rows.Fixed = 1;
        this._grid.Rows.DefaultSize = 30;
        this._grid.SelectionMode = SelectionModeEnum.Row;
        this._grid.Styles.Normal.Font = fontNormal;
        this._grid.Styles.Normal.TextAlign = TextAlignEnum.CenterCenter;
        this._grid.Styles.Normal.Border.Style = BorderStyleEnum.Flat;
        this._grid.Styles.Normal.Border.Width = 1;
        this._grid.Styles.Normal.Border.Color = Color.FromArgb(234, 236, 240);
        this._grid.Styles.Fixed.Font = new Font("Microsoft YaHei", 9.5f, FontStyle.Bold);
        this._grid.Styles.Fixed.ForeColor = Color.FromArgb(50, 55, 65);
        this._grid.Styles.Fixed.BackColor = colorGridFixedBg;
        this._grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
        this._grid.Styles.Fixed.Border.Style = BorderStyleEnum.Flat;
        this._grid.Styles.Fixed.Border.Width = 1;
        this._grid.Styles.Fixed.Border.Color = Color.FromArgb(220, 223, 230);
        this._grid.Styles.Alternate.BackColor = colorGridAltBg;
        this._grid.Styles.EmptyArea.BackColor = Color.White;
        this._grid.Styles.Highlight.BackColor = Color.FromArgb(0, 120, 215);
        this._grid.Styles.Highlight.ForeColor = Color.White;
        this._grid.Styles.Focus.BackColor = Color.FromArgb(200, 230, 255);
        this._grid.Styles.Focus.ForeColor = Color.Black;
        this._grid.AfterEdit += _grid_AfterEdit;

        // 网格容器（带 Padding 让网格不贴边）
        var pnlGridContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 8, 12, 4),
            BackColor = Color.White
        };
        pnlGridContainer.Controls.Add(this._grid);

        // ---- 底部按钮区（扁平圆角风，浅灰背景） ----
        _pnlButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = colorBottomBg
        };
        var pnlBorder = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(220, 223, 230)
        };
        _pnlButtons.Controls.Add(pnlBorder);

        // 按钮样式辅助方法（与其他界面一致的 C1Button 扁平风）
        var btnH = 34;
        Action<C1Button, Color, Color, string> styleBtn = (btn, bg, fg, text) => {
            btn.Font = fontBtn;
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = bg;
            btn.ForeColor = fg;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = (bg == colorBtnNormal) ? colorBtnBorder : bg;
            btn.Size = new Size(0, btnH);
            btn.UseVisualStyleBackColor = false;
            btn.Cursor = Cursors.Hand;
        };

        // 创建所有按钮
        this._btnAddNew.TabIndex = 9;
        this._btnAddNew.Click += _btnAddNew_Click;
        styleBtn(this._btnAddNew, colorBtnPrimary, Color.White, "新增引用");
        this._btnAddNew.Font = fontBtnBold;
        this._btnAddNew.Width = 96;
        _pnlButtons.Controls.Add(this._btnAddNew);

        this._btnEdit.TabIndex = 3;
        this._btnEdit.Click += _btnEdit_Click;
        styleBtn(this._btnEdit, colorBtnNormal, colorBtnNormalFg, "编辑");
        this._btnEdit.Width = 64;
        _pnlButtons.Controls.Add(this._btnEdit);

        this._btnDelete.TabIndex = 4;
        this._btnDelete.Click += _btnDelete_Click;
        styleBtn(this._btnDelete, colorBtnDanger, Color.White, "删除");
        this._btnDelete.Width = 64;
        _pnlButtons.Controls.Add(this._btnDelete);

        this._btnToggleEnabled.TabIndex = 5;
        this._btnToggleEnabled.Click += _btnToggleEnabled_Click;
        styleBtn(this._btnToggleEnabled, colorBtnNormal, colorBtnNormalFg, "启用/禁用");
        this._btnToggleEnabled.Width = 80;
        _pnlButtons.Controls.Add(this._btnToggleEnabled);

        this._btnRefreshSelected.TabIndex = 6;
        this._btnRefreshSelected.Click += _btnRefreshSelected_Click;
        styleBtn(this._btnRefreshSelected, colorBtnRefresh, Color.White, "刷新选定");
        this._btnRefreshSelected.Width = 80;
        _pnlButtons.Controls.Add(this._btnRefreshSelected);

        this._btnRefreshAll.TabIndex = 7;
        this._btnRefreshAll.Click += _btnRefreshAll_Click;
        styleBtn(this._btnRefreshAll, colorBtnRefresh, Color.White, "刷新所有");
        this._btnRefreshAll.Width = 80;
        _pnlButtons.Controls.Add(this._btnRefreshAll);

        this._btnDashboard.TabIndex = 10;
        this._btnDashboard.Click += _btnDashboard_Click;
        styleBtn(this._btnDashboard, colorBtnNormal, colorBtnNormalFg, "状态仪表板");
        this._btnDashboard.Width = 96;
        _pnlButtons.Controls.Add(this._btnDashboard);

        this._btnClose.TabIndex = 13;
        this._btnClose.Click += _btnClose_Click;
        styleBtn(this._btnClose, colorBtnNormal, colorBtnNormalFg, "关闭");
        this._btnClose.Width = 72;
        _pnlButtons.Controls.Add(this._btnClose);

        // 按钮布局：Resize 时重新排列
        _pnlButtons.Resize += (s, e) => LayoutButtons();

        // ---- 窗体设置 ----
        this.AutoScaleDimensions = new SizeF(7f, 17f);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1040, 600);
        this.Font = fontNormal;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.MinimumSize = new Size(900, 500);
        this.Name = "frmCrossProjectDataRef";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "跨项目数据引用管理";

        // Dock 布局顺序：先 Bottom，再 Top，最后 Fill
        this.Controls.Add(pnlGridContainer);
        this.Controls.Add(_pnlButtons);
        this.Controls.Add(pnlHeader);

        this.CancelButton = this._btnClose;

        // 窗体 Load 时布局按钮
        this.Load += (s, e) => LayoutButtons();
    }

    /// <summary>
    /// 底部按钮对齐布局：左侧操作按钮从左到右排列，关闭按钮靠右
    /// </summary>
    private void LayoutButtons()
    {
        if (_pnlButtons == null) return;
        var btnH = 34;
        var btnY = (_pnlButtons.Height - btnH) / 2;
        var btnGap = 8;
        var startX = 12;

        // 左侧按钮组
        var leftBtns = new[] { _btnAddNew, _btnEdit, _btnDelete, _btnToggleEnabled,
                                _btnRefreshSelected, _btnRefreshAll, _btnDashboard };
        var x = startX;
        foreach (var btn in leftBtns)
        {
            if (btn == null) continue;
            btn.Location = new Point(x, btnY);
            x += btn.Width + btnGap;
        }

        // 关闭按钮靠右
        if (_btnClose != null)
        {
            _btnClose.Location = new Point(_pnlButtons.Width - _btnClose.Width - 12, btnY);
        }
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
        private readonly Auditai.Model.Project _currentProject;

        // 私有状态
        private Auditai.DTO.Project _selectedProject;
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

        public frmCrossProjectDataRefEditDialog(string title, CrossProjectDataRef existing, Id64 currentTableId, Auditai.Model.Project currentProject)
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
                        var dal = new Auditai.DTO.ProjectDAL(dbPath);
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
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请先选择来源项目");
            return;
        }

        // 加载外部项目的表格节点
        var tableNodes = LoadTableNodesForProject(_selectedProject);
        if (tableNodes == null || tableNodes.Count == 0)
        {
            Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "该项目没有可用的表格");
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
                    Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请选择一个表格");
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
    private static List<TreeTableNode> LoadTableNodesForProject(Auditai.DTO.Project projectDto)
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

        _selectedProject = new Auditai.DTO.Project
        {
            Id = _existing.SourceProjectId,
            Name = projectName
        };
        _txtSourceProject.Text = projectName;
        _txtSourceProject.ReadOnly = true;

        _selectedTableNode = new TreeTableNode { Name = tableName };
        _selectedTableNode.Id = _existing.SourceTableId;
        _selectedTableNode.SetTable(new Auditai.Model.Table());
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
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请输入引用名称");
                return;
            }
            if (this._selectedProject == null)
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请选择来源项目");
                return;
            }
            if (this._selectedTableNode == null)
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "请选择来源表");
                return;
            }
            // 验证 JSON 格式
            if (!string.IsNullOrWhiteSpace(this._txtRefConfig.Text) && !IsValidJson(this._txtRefConfig.Text))
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "引用配置 JSON 格式不正确");
                return;
            }
            if (!string.IsNullOrWhiteSpace(this._txtFilterConfig.Text) && !IsValidJson(this._txtFilterConfig.Text))
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "筛选配置 JSON 格式不正确");
                return;
            }
            if (!string.IsNullOrWhiteSpace(this._txtColumnMapping.Text) && !IsValidJson(this._txtColumnMapping.Text))
            {
                Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "列映射 JSON 格式不正确");
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
