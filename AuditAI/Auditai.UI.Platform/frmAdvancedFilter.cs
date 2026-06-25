using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using Newtonsoft.Json;

namespace Auditai.UI.Platform;

/// <summary>
/// 高级筛选条件配置对话框
/// 用于配置跨项目数据引用时的筛选条件
/// </summary>
public class frmAdvancedFilter : C1RibbonForm
{
    private C1FlexGrid _gridConditions;
    private C1Label _lblPreviewStats;
    private C1FlexGrid _gridPreview;
    private C1Button _btnAddCondition;
    private C1Button _btnDeleteCondition;
    private C1Button _btnClearAll;
    private C1Button _btnPreview;
    private C1Button _btnOk;
    private C1Button _btnCancel;

    private readonly List<DTO.Column> _sourceColumns;
    private readonly List<List<object>> _sampleData;
    private readonly List<string> _operators = new()
    {
        "等于", "不等于", "大于", "小于", "大于等于", "小于等于",
        "区间", "包含", "开头是", "结尾是", "为空", "非空", "为空或零", "非空且非零"
    };

    /// <summary>
    /// 返回配置好的筛选条件 JSON
    /// </summary>
    public string FilterConfigJson { get; private set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sourceColumns">来源表的所有列信息</param>
    /// <param name="sampleData">来源表的前若干行数据样例</param>
    public frmAdvancedFilter(List<DTO.Column> sourceColumns, List<List<object>> sampleData)
    {
        _sourceColumns = sourceColumns ?? new List<DTO.Column>();
        _sampleData = sampleData ?? new List<List<object>>();
        InitializeComponent();
        PopulateColumnNames();
        AddDefaultRow();
        UpdatePreview();
    }

    private void InitializeComponent()
    {
        // 网格：条件编辑器
        _gridConditions = new C1FlexGrid();
        _gridConditions.AllowDragging = AllowDraggingEnum.None;
        _gridConditions.AllowEditing = true;
        _gridConditions.AllowSorting = AllowSortingEnum.None;
        _gridConditions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        _gridConditions.BackColor = Color.White;
        _gridConditions.Cols.Count = 5;
        _gridConditions.Cols[0].Caption = "列名";
        _gridConditions.Cols[0].Width = 120;
        _gridConditions.Cols[1].Caption = "运算符";
        _gridConditions.Cols[1].Width = 90;
        _gridConditions.Cols[2].Caption = "值1";
        _gridConditions.Cols[2].Width = 100;
        _gridConditions.Cols[3].Caption = "值2";
        _gridConditions.Cols[3].Width = 100;
        _gridConditions.Cols[4].Caption = "逻辑关系";
        _gridConditions.Cols[4].Width = 70;
        // 设置运算符和逻辑关系列为下拉选择（管道符分隔）
        _gridConditions.Cols[1].ComboList = string.Join("|", _operators);
        _gridConditions.Cols[4].ComboList = "And|Or";
        _gridConditions.Location = new Point(12, 12);
        _gridConditions.Name = "_gridConditions";
        _gridConditions.Rows.Count = 1;
        _gridConditions.Rows.Fixed = 1;
        _gridConditions.Size = new Size(580, 380);
        _gridConditions.TabIndex = 0;
        _gridConditions.AfterEdit += _gridConditions_AfterEdit;
        _gridConditions.SetupEditor += _gridConditions_SetupEditor;

        // 预览统计标签
        _lblPreviewStats = new C1Label();
        _lblPreviewStats.AutoSize = true;
        _lblPreviewStats.BackColor = Color.Transparent;
        _lblPreviewStats.BorderStyle = BorderStyle.None;
        _lblPreviewStats.Font = new Font("Microsoft YaHei", 9f, FontStyle.Bold);
        _lblPreviewStats.ForeColor = Color.Black;
        _lblPreviewStats.Location = new Point(600, 12);
        _lblPreviewStats.Name = "_lblPreviewStats";
        _lblPreviewStats.Size = new Size(270, 20);
        _lblPreviewStats.TabIndex = 1;
        _lblPreviewStats.Text = "满足条件的行数: 0 行";
        _lblPreviewStats.TextDetached = true;

        // 网格：预览数据
        _gridPreview = new C1FlexGrid();
        _gridPreview.AllowDragging = AllowDraggingEnum.None;
        _gridPreview.AllowEditing = false;
        _gridPreview.AllowSorting = AllowSortingEnum.None;
        _gridPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _gridPreview.BackColor = Color.White;
        _gridPreview.Location = new Point(600, 40);
        _gridPreview.Name = "_gridPreview";
        _gridPreview.Size = new Size(270, 270);
        _gridPreview.TabIndex = 2;

        // 添加条件按钮
        _btnAddCondition = new C1Button();
        _btnAddCondition.Font = new Font("Microsoft YaHei", 9f);
        _btnAddCondition.Location = new Point(12, 400);
        _btnAddCondition.Name = "_btnAddCondition";
        _btnAddCondition.Size = new Size(90, 30);
        _btnAddCondition.TabIndex = 3;
        _btnAddCondition.Text = "添加条件";
        _btnAddCondition.UseVisualStyleBackColor = true;
        _btnAddCondition.Click += _btnAddCondition_Click;

        // 删除条件按钮
        _btnDeleteCondition = new C1Button();
        _btnDeleteCondition.Font = new Font("Microsoft YaHei", 9f);
        _btnDeleteCondition.Location = new Point(108, 400);
        _btnDeleteCondition.Name = "_btnDeleteCondition";
        _btnDeleteCondition.Size = new Size(90, 30);
        _btnDeleteCondition.TabIndex = 4;
        _btnDeleteCondition.Text = "删除条件";
        _btnDeleteCondition.UseVisualStyleBackColor = true;
        _btnDeleteCondition.Click += _btnDeleteCondition_Click;

        // 清除所有条件按钮
        _btnClearAll = new C1Button();
        _btnClearAll.Font = new Font("Microsoft YaHei", 9f);
        _btnClearAll.Location = new Point(204, 400);
        _btnClearAll.Name = "_btnClearAll";
        _btnClearAll.Size = new Size(110, 30);
        _btnClearAll.TabIndex = 5;
        _btnClearAll.Text = "清除所有条件";
        _btnClearAll.UseVisualStyleBackColor = true;
        _btnClearAll.Click += _btnClearAll_Click;

        // 预览按钮
        _btnPreview = new C1Button();
        _btnPreview.Font = new Font("Microsoft YaHei", 9f);
        _btnPreview.Location = new Point(600, 320);
        _btnPreview.Name = "_btnPreview";
        _btnPreview.Size = new Size(90, 30);
        _btnPreview.TabIndex = 6;
        _btnPreview.Text = "预览";
        _btnPreview.UseVisualStyleBackColor = true;
        _btnPreview.Click += _btnPreview_Click;

        // 确定按钮
        _btnOk = new C1Button();
        _btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnOk.Font = new Font("Microsoft YaHei", 9f);
        _btnOk.Location = new Point(696, 530);
        _btnOk.Name = "_btnOk";
        _btnOk.Size = new Size(87, 33);
        _btnOk.TabIndex = 7;
        _btnOk.Text = "确定";
        _btnOk.UseVisualStyleBackColor = true;
        _btnOk.Click += _btnOk_Click;

        // 取消按钮
        _btnCancel = new C1Button();
        _btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _btnCancel.Font = new Font("Microsoft YaHei", 9f);
        _btnCancel.Location = new Point(789, 530);
        _btnCancel.Name = "_btnCancel";
        _btnCancel.Size = new Size(87, 33);
        _btnCancel.TabIndex = 8;
        _btnCancel.Text = "取消";
        _btnCancel.UseVisualStyleBackColor = true;
        _btnCancel.Click += _btnCancel_Click;

        // frmAdvancedFilter
        this.AutoScaleDimensions = new SizeF(7f, 17f);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(890, 575);
        this.Controls.Add(this._gridConditions);
        this.Controls.Add(this._lblPreviewStats);
        this.Controls.Add(this._gridPreview);
        this.Controls.Add(this._btnAddCondition);
        this.Controls.Add(this._btnDeleteCondition);
        this.Controls.Add(this._btnClearAll);
        this.Controls.Add(this._btnPreview);
        this.Controls.Add(this._btnOk);
        this.Controls.Add(this._btnCancel);
        this.Font = new Font("Microsoft YaHei", 9f);
        this.Name = "frmAdvancedFilter";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "高级筛选条件配置";
    }

    private void PopulateColumnNames()
    {
        // 为列名下拉设置 ComboList（管道符分隔列名）
        if (_sourceColumns.Count > 0)
        {
            var names = _sourceColumns.Select(c => c.Caption).Where(n => !string.IsNullOrEmpty(n));
            _gridConditions.Cols[0].ComboList = string.Join("|", names);
        }
    }

    private void AddDefaultRow()
    {
        _gridConditions.Rows.Count = 2;
        _gridConditions.Rows.Fixed = 1;
    }

    private void _gridConditions_AfterEdit(object sender, RowColEventArgs e)
    {
        // 当条件变化时，清空已生成的 JSON 并刷新预览
        FilterConfigJson = null;
        UpdatePreview();
    }

    private void _gridConditions_SetupEditor(object sender, RowColEventArgs e)
    {
        if (e.Col == 0)
        {
            // 列名下拉：填充来源表列名
            var editor = _gridConditions.Editor as C1ComboBox;
            if (editor != null)
            {
                editor.Items.Clear();
                foreach (var col in _sourceColumns)
                {
                    editor.Items.Add(col.Caption);
                }
            }
        }
        else if (e.Col == 1)
        {
            // 运算符下拉
            var editor = _gridConditions.Editor as C1ComboBox;
            if (editor != null)
            {
                editor.Items.Clear();
                foreach (var op in _operators)
                {
                    editor.Items.Add(op);
                }
            }
        }
        else if (e.Col == 4)
        {
            // 逻辑关系下拉
            var editor = _gridConditions.Editor as C1ComboBox;
            if (editor != null)
            {
                editor.Items.Clear();
                editor.Items.Add("And");
                editor.Items.Add("Or");
            }
        }
    }

    private void _btnAddCondition_Click(object sender, EventArgs e)
    {
        int newRowIndex = _gridConditions.Rows.Count;
        _gridConditions.Rows.Count = newRowIndex + 1;
        _gridConditions.Rows[newRowIndex].Height = 22;
    }

    private void _btnDeleteCondition_Click(object sender, EventArgs e)
    {
        if (_gridConditions.Rows.Count <= 2)
            return;

        int row = _gridConditions.RowSel;
        if (row >= 1 && row < _gridConditions.Rows.Count)
        {
            _gridConditions.Rows.Remove(row);
        }
    }

    private void _btnClearAll_Click(object sender, EventArgs e)
    {
        _gridConditions.Rows.Count = 1;
        _gridConditions.Rows.Fixed = 1;
        AddDefaultRow();
        FilterConfigJson = null;
        UpdatePreview();
    }

    private void _btnPreview_Click(object sender, EventArgs e)
    {
        UpdatePreview();
    }

    private void _btnOk_Click(object sender, EventArgs e)
    {
        FilterConfigJson = BuildFilterConfigJson();
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void _btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    /// <summary>
    /// 从网格中读取筛选条件并构建 JSON 字符串
    /// </summary>
    private string BuildFilterConfigJson()
    {
        var conditions = new List<object>();

        for (int i = 1; i < _gridConditions.Rows.Count; i++)
        {
            string columnName = _gridConditions[i, 0] as string;
            string operatorStr = _gridConditions[i, 1] as string;
            string value1 = _gridConditions[i, 2] as string;
            string value2 = _gridConditions[i, 3] as string;
            string logic = _gridConditions[i, 4] as string;

            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(operatorStr))
                continue;

            var condition = new Dictionary<string, object>
            {
                ["Column"] = columnName,
                ["Operator"] = operatorStr,
                ["Value1"] = value1 ?? string.Empty,
                ["Value2"] = value2 ?? string.Empty,
                ["Logic"] = (i < _gridConditions.Rows.Count - 1 && !string.IsNullOrEmpty(logic)) ? logic : ""
            };

            conditions.Add(condition);
        }

        return JsonConvert.SerializeObject(new { Conditions = conditions }, Formatting.Indented);
    }

    /// <summary>
    /// 更新预览区域的统计信息和示例数据
    /// </summary>
    private void UpdatePreview()
    {
        var conditions = ReadConditions();
        int matchedCount = 0;
        var matchedRows = new List<List<object>>();

        foreach (var row in _sampleData)
        {
            if (EvaluateConditions(row, conditions))
            {
                matchedCount++;
                if (matchedRows.Count < 5)
                {
                    matchedRows.Add(row);
                }
            }
        }

        _lblPreviewStats.Text = $"满足条件的行数: {matchedCount} 行";

        // 填充预览网格
        PopulatePreviewGrid(matchedRows);
    }

    private List<ConditionRow> ReadConditions()
    {
        var conditions = new List<ConditionRow>();

        for (int i = 1; i < _gridConditions.Rows.Count; i++)
        {
            string columnName = _gridConditions[i, 0] as string;
            string operatorStr = _gridConditions[i, 1] as string;
            string value1 = _gridConditions[i, 2] as string;
            string value2 = _gridConditions[i, 3] as string;
            string logic = _gridConditions[i, 4] as string;

            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(operatorStr))
                continue;

            conditions.Add(new ConditionRow
            {
                ColumnName = columnName,
                Operator = operatorStr,
                Value1 = value1 ?? string.Empty,
                Value2 = value2 ?? string.Empty,
                Logic = (i < _gridConditions.Rows.Count - 1) ? (logic ?? "And") : "And"
            });
        }

        return conditions;
    }

    private bool EvaluateConditions(List<object> row, List<ConditionRow> conditions)
    {
        if (conditions.Count == 0)
            return true;

        // 获取列名到索引的映射
        var columnIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < _sourceColumns.Count; i++)
        {
            if (!columnIndexMap.ContainsKey(_sourceColumns[i].Caption))
            {
                columnIndexMap[_sourceColumns[i].Caption] = i;
            }
        }

        bool? currentResult = null;
        string pendingLogic = "And";

        foreach (var condition in conditions)
        {
            bool condResult;

            if (!columnIndexMap.TryGetValue(condition.ColumnName, out int colIndex) || colIndex >= row.Count)
            {
                condResult = false;
            }
            else
            {
                object cellValue = row[colIndex];
                condResult = EvaluateSingleCondition(cellValue, condition);
            }

            if (currentResult == null)
            {
                currentResult = condResult;
            }
            else
            {
                currentResult = pendingLogic.Equals("Or", StringComparison.OrdinalIgnoreCase)
                    ? (currentResult.Value || condResult)
                    : (currentResult.Value && condResult);
            }

            pendingLogic = condition.Logic;
        }

        return currentResult ?? true;
    }

    private bool EvaluateSingleCondition(object cellValue, ConditionRow condition)
    {
        string strValue = cellValue?.ToString()?.Trim() ?? string.Empty;
        string op = condition.Operator;

        switch (op)
        {
            case "等于":
                return string.Equals(strValue, condition.Value1.Trim(), StringComparison.OrdinalIgnoreCase);

            case "不等于":
                return !string.Equals(strValue, condition.Value1.Trim(), StringComparison.OrdinalIgnoreCase);

            case "大于":
                return TryParseNumber(strValue, out var nv) && TryParseNumber(condition.Value1, out var cv) && nv > cv;

            case "小于":
                return TryParseNumber(strValue, out var nv2) && TryParseNumber(condition.Value1, out var cv2) && nv2 < cv2;

            case "大于等于":
                return TryParseNumber(strValue, out var nv3) && TryParseNumber(condition.Value1, out var cv3) && nv3 >= cv3;

            case "小于等于":
                return TryParseNumber(strValue, out var nv4) && TryParseNumber(condition.Value1, out var cv4) && nv4 <= cv4;

            case "区间":
                return TryParseNumber(strValue, out var nv5)
                    && TryParseNumber(condition.Value1, out var cv5)
                    && TryParseNumber(condition.Value2, out var cv6)
                    && nv5 >= cv5 && nv5 <= cv6;

            case "包含":
                if (string.IsNullOrWhiteSpace(condition.Value1)) return false;
                return strValue.IndexOf(condition.Value1.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;

            case "开头是":
                if (string.IsNullOrWhiteSpace(condition.Value1)) return false;
                return strValue.StartsWith(condition.Value1.Trim(), StringComparison.OrdinalIgnoreCase);

            case "结尾是":
                if (string.IsNullOrWhiteSpace(condition.Value1)) return false;
                return strValue.EndsWith(condition.Value1.Trim(), StringComparison.OrdinalIgnoreCase);

            case "为空":
                return string.IsNullOrEmpty(strValue);

            case "非空":
                return !string.IsNullOrEmpty(strValue);

            case "为空或零":
                return string.IsNullOrEmpty(strValue) || strValue == "0";

            case "非空且非零":
                return !string.IsNullOrEmpty(strValue) && strValue != "0";

            default:
                return true;
        }
    }

    private static bool TryParseNumber(string s, out double result)
    {
        return double.TryParse(s, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out result);
    }

    private void PopulatePreviewGrid(List<List<object>> matchedRows)
    {
        _gridPreview.Rows.Count = 1;
        _gridPreview.Rows.Fixed = 1;
        _gridPreview.Cols.Count = _sourceColumns.Count;

        // 设置列标题
        for (int i = 0; i < _sourceColumns.Count; i++)
        {
            _gridPreview.Cols[i].Caption = _sourceColumns[i].Caption;
            _gridPreview.Cols[i].Width = 80;
        }

        // 填充数据行
        for (int r = 0; r < matchedRows.Count; r++)
        {
            _gridPreview.Rows.Count = r + 2;
            for (int c = 0; c < matchedRows[r].Count && c < _sourceColumns.Count; c++)
            {
                _gridPreview[r + 1, c] = matchedRows[r][c];
            }
        }
    }

    /// <summary>
    /// 内部条件行模型
    /// </summary>
    private class ConditionRow
    {
        public string ColumnName { get; set; }
        public string Operator { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Logic { get; set; }
    }
}