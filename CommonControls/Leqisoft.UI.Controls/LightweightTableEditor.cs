using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;

namespace Leqisoft.UI.Controls
{
    /// <summary>
    /// 轻量级表格编辑器（模仿 TableEditor 外观，不依赖 MainForm）
    /// 适用于需要在对话框/向导中显示表格并选择区域的场景
    /// </summary>
    public class LightweightTableEditor : UserControl
    {
        private readonly C1FlexGridEx _grid;
        private readonly Label _lblError;
        private bool _hasRowHeader = true;
        private readonly Font _rowHeaderFont = new Font("Microsoft YaHei", 9f);

        /// <summary>
        /// 选择区域改变事件
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// 选择区域变更参数
        /// </summary>
        public class SelectionChangedEventArgs : EventArgs
        {
            /// <summary>Body 起始行（0-based），-1 表示未选择</summary>
            public int StartRow { get; set; }
            /// <summary>Body 结束行（0-based），-1 表示未选择</summary>
            public int EndRow { get; set; }
            /// <summary>Body 起始列（0-based），-1 表示未选择</summary>
            public int StartCol { get; set; }
            /// <summary>Body 结束列（0-based），-1 表示未选择</summary>
            public int EndCol { get; set; }
            /// <summary>列名（取首列）</summary>
            public string ColName { get; set; } = "";
            /// <summary>终点列名（区域选择时有效）</summary>
            public string ColName2 { get; set; } = "";
        }

        /// <summary>
        /// 底层 C1FlexGridEx 控件（直接访问以支持高级需求）
        /// </summary>
        public C1FlexGridEx Grid => _grid;

        /// <summary>
        /// 是否显示行号列
        /// </summary>
        public bool ShowRowHeader
        {
            get => _hasRowHeader;
            set
            {
                _hasRowHeader = value;
                _grid.Cols.Fixed = value ? 1 : 0;
            }
        }

        /// <summary>
        /// 选择模式
        /// </summary>
        public SelectionModeEnum SelectionMode
        {
            get => _grid.SelectionMode;
            set => _grid.SelectionMode = value;
        }

        /// <summary>
        /// 高亮颜色（选区背景色）
        /// </summary>
        public Color HighlightColor
        {
            get => _grid.Styles.Highlight.BackColor;
            set
            {
                _grid.Styles.Highlight.BackColor = value;
                // 根据背景色亮度自动选择前景色（浅色背景用黑字，深色背景用白字）
                _grid.Styles.Highlight.ForeColor = GetContrastColor(value);
            }
        }

        /// <summary>
        /// 焦点颜色（当前单元格背景色）
        /// </summary>
        public Color FocusColor
        {
            get => _grid.Styles.Focus.BackColor;
            set
            {
                _grid.Styles.Focus.BackColor = value;
                _grid.Styles.Focus.ForeColor = GetContrastColor(value);
            }
        }

        /// <summary>
        /// 数据单元格背景色（整个表格的底色，区别于控件背景色 BackColor）
        /// 设置后同步更新 Normal、EmptyArea 和 Alternate 样式
        /// </summary>
        public Color CellBackColor
        {
            get => _grid.Styles.Normal.BackColor;
            set
            {
                _grid.Styles.Normal.BackColor = value;
                _grid.Styles.EmptyArea.BackColor = value;
                // 隔行变色：基于单元格底色稍深一点，保持视觉层次
                _grid.Styles.Alternate.BackColor = Color.FromArgb(
                    Math.Max(0, value.R - 7),
                    Math.Max(0, value.G - 7),
                    Math.Max(0, value.B - 7));
            }
        }

        /// <summary>
        /// 根据背景色亮度计算对比色（黑或白）
        /// </summary>
        private static Color GetContrastColor(Color backColor)
        {
            // 使用 ITU-R BT.601 标准计算亮度
            double luminance = 0.299 * backColor.R + 0.587 * backColor.G + 0.114 * backColor.B;
            return luminance > 140 ? Color.Black : Color.White;
        }

        public LightweightTableEditor()
        {
            // ---- 初始化网格（参照 TableEditor.Initialize） ----
            _grid = new C1FlexGridEx
            {
                Dock = DockStyle.Fill,
                AllowEditing = false,
                AllowSorting = AllowSortingEnum.None,
                AllowDragging = AllowDraggingEnum.None,
                AllowMerging = AllowMergingEnum.None,
                AllowResizing = AllowResizingEnum.Rows,
                BackColor = Color.White,
                BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None,
                ExtendLastCol = true,
                HighLight = HighLightEnum.Always,
                ScrollOptions = ScrollFlags.None,
                VisualStyle = VisualStyle.Custom,
                AutoClipboard = false
            };

            // ---- 样式设置（参照 TableEditor.SetTheme） ----
            var borderColor = Color.FromArgb(218, 220, 224);  // 浅灰网格线

            // Normal 样式（数据单元格）
            _grid.Styles.Normal.BackColor = Color.White;
            _grid.Styles.Normal.Font = new Font("Microsoft YaHei", 9.5f);
            _grid.Styles.Normal.Border.Style = BorderStyleEnum.Flat;
            _grid.Styles.Normal.Border.Width = 1;
            _grid.Styles.Normal.Border.Color = borderColor;
            _grid.Styles.Normal.Border.Direction = BorderDirEnum.Both;
            _grid.Styles.Normal.TextAlign = TextAlignEnum.LeftCenter;
            _grid.Styles.Normal.Margins = new System.Drawing.Printing.Margins(4, 4, 0, 0);

            // Alternate 样式（隔行变色）
            _grid.Styles.Alternate.BackColor = Color.FromArgb(248, 249, 251);
            _grid.Styles.Alternate.Font = new Font("Microsoft YaHei", 9.5f);
            _grid.Styles.Alternate.Border.Style = BorderStyleEnum.Flat;
            _grid.Styles.Alternate.Border.Width = 1;
            _grid.Styles.Alternate.Border.Color = borderColor;
            _grid.Styles.Alternate.Border.Direction = BorderDirEnum.Both;

            // Fixed 样式（列头/行号）
            _grid.Styles.Fixed.BackColor = Color.FromArgb(243, 245, 248);
            _grid.Styles.Fixed.Font = new Font("Microsoft YaHei", 9.5f, FontStyle.Bold);
            _grid.Styles.Fixed.ForeColor = Color.FromArgb(50, 55, 65);
            _grid.Styles.Fixed.Border.Style = BorderStyleEnum.Flat;
            _grid.Styles.Fixed.Border.Width = 1;
            _grid.Styles.Fixed.Border.Color = Color.FromArgb(200, 203, 210);
            _grid.Styles.Fixed.Border.Direction = BorderDirEnum.Both;
            _grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;

            // EmptyArea 样式（参照 TableEditor.SetTheme）
            _grid.Styles.EmptyArea.BackColor = Color.White;
            _grid.Styles.EmptyArea.Border.Style = BorderStyleEnum.None;

            // 选区高亮（蓝色 - 目标表默认色）
            _grid.Styles.Highlight.BackColor = Color.FromArgb(0, 120, 215);
            _grid.Styles.Highlight.ForeColor = Color.White;

            // 焦点单元格
            _grid.Styles.Focus.BackColor = Color.FromArgb(200, 230, 255);
            _grid.Styles.Focus.ForeColor = Color.Black;

            // 行列头
            _grid.Rows.DefaultSize = 28;
            _grid.Cols.DefaultSize = 100;
            _grid.Rows.Fixed = 1;
            _grid.Cols.Fixed = 1; // 行号列
            _grid.Cols[0].Width = 50; // 行号列宽度

            // 选择改变事件
            _grid.AfterSelChange += (s, e) =>
            {
                var sel = _grid.Selection;
                int r1 = sel.TopRow, r2 = sel.BottomRow;
                int c1 = sel.LeftCol, c2 = sel.RightCol;

                // 转换为 body 坐标
                int bodyR1 = r1 - _grid.Rows.Fixed;
                int bodyR2 = r2 - _grid.Rows.Fixed;
                int bodyC1 = c1 - _grid.Cols.Fixed;
                int bodyC2 = c2 - _grid.Cols.Fixed;

                // 忽略固定行/列的选择
                if (bodyR1 < 0 || bodyC1 < 0)
                {
                    SelectionChanged?.Invoke(this, new SelectionChangedEventArgs
                    {
                        StartRow = -1, EndRow = -1, StartCol = -1, EndCol = -1
                    });
                    return;
                }

                // 获取列名
                string colName = GetColumnName(bodyC1);
                string colName2 = GetColumnName(bodyC2);

                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs
                {
                    StartRow = bodyR1,
                    EndRow = bodyR2,
                    StartCol = bodyC1,
                    EndCol = bodyC2,
                    ColName = colName,
                    ColName2 = colName2
                });
            };

            // 绘制行号（参照 TableEditor 的行号列风格）
            _grid.OwnerDrawCell += (s, e) =>
            {
                if (_hasRowHeader && e.Col == 0 && e.Row >= _grid.Rows.Fixed)
                {
                    e.Text = (e.Row - _grid.Rows.Fixed + 1).ToString();
                    e.Style.BackColor = Color.FromArgb(243, 245, 248);
                    e.Style.ForeColor = Color.FromArgb(120, 125, 135);
                    e.Style.TextAlign = TextAlignEnum.CenterCenter;
                    e.Style.Font = _rowHeaderFont;
                    e.Style.Border.Style = BorderStyleEnum.Flat;
                    e.Style.Border.Width = 1;
                    e.Style.Border.Color = Color.FromArgb(200, 203, 210);
                    e.Style.Border.Direction = BorderDirEnum.Both;
                }
            };

            Controls.Add(_grid);

            // 错误提示标签
            _lblError = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.Red,
                Padding = new Padding(10),
                Visible = false,
                Font = new Font("Microsoft YaHei", 10f)
            };
            Controls.Add(_lblError);
        }

        /// <summary>
        /// 获取列名（body 列索引）
        /// </summary>
        public string GetColumnName(int bodyCol)
        {
            if (bodyCol < 0) return "";
            int absCol = bodyCol + _grid.Cols.Fixed;
            if (absCol >= _grid.Cols.Count) return $"列{bodyCol + 1}";
            return _grid[0, absCol]?.ToString() ?? $"列{bodyCol + 1}";
        }

        /// <summary>
        /// 设置列头
        /// </summary>
        public void SetColumns(string[] columnNames)
        {
            _grid.BeginUpdate();
            try
            {
                _grid.Cols.Count = columnNames.Length + (_hasRowHeader ? 1 : 0);
                for (int i = 0; i < columnNames.Length; i++)
                {
                    int absCol = i + _grid.Cols.Fixed;
                    _grid[0, absCol] = columnNames[i];
                    _grid.Cols[absCol].Width = 100;
                }
                _grid.Rows.Count = _grid.Rows.Fixed;
            }
            finally
            {
                _grid.EndUpdate();
            }
        }

        /// <summary>
        /// 设置单元格数据
        /// </summary>
        public void SetCell(int bodyRow, int bodyCol, string value)
        {
            int absRow = bodyRow + _grid.Rows.Fixed;
            int absCol = bodyCol + _grid.Cols.Fixed;
            if (absRow >= _grid.Rows.Count || absCol >= _grid.Cols.Count) return;
            _grid[absRow, absCol] = value;
        }

        /// <summary>
        /// 设置行数
        /// </summary>
        public void SetRowCount(int count)
        {
            _grid.Rows.Count = count + _grid.Rows.Fixed;
        }

        /// <summary>
        /// 批量加载数据（二维数组，第一行作为列头）
        /// </summary>
        public void LoadData(string[,] data, int headerRowCount = 1)
        {
            if (data == null)
            {
                ShowError("数据为空");
                return;
            }

            try
            {
                HideError();
                _grid.BeginUpdate();

                int totalRows = data.GetLength(0);
                int totalCols = data.GetLength(1);

                // 设置列
                _grid.Cols.Count = totalCols + (_hasRowHeader ? 1 : 0);

                // 计算数据行数（总行数减去列头行数，最小为0）
                int dataRows = Math.Max(0, totalRows - headerRowCount);
                _grid.Rows.Count = dataRows + _grid.Rows.Fixed;

                // 填充列头（取第一行）
                if (headerRowCount > 0 && totalRows > 0)
                {
                    for (int c = 0; c < totalCols; c++)
                    {
                        int absCol = c + _grid.Cols.Fixed;
                        _grid[0, absCol] = data[0, c] ?? $"列{c + 1}";
                        _grid.Cols[absCol].Width = 100;
                    }
                }

                // 填充数据行
                int dataStartRow = headerRowCount;
                int gridStartRow = _grid.Rows.Fixed;
                for (int r = dataStartRow; r < totalRows; r++)
                {
                    int gridRow = gridStartRow + (r - dataStartRow);
                    if (gridRow >= _grid.Rows.Count) break;
                    for (int c = 0; c < totalCols; c++)
                    {
                        int absCol = c + _grid.Cols.Fixed;
                        _grid[gridRow, absCol] = data[r, c] ?? "";
                    }
                }

                _grid.AutoSizeCols();
            }
            catch (Exception ex)
            {
                ex.Log();
                ShowError($"加载失败：{ex.Message}");
            }
            finally
            {
                _grid.EndUpdate();
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            _grid.Rows.Count = _grid.Rows.Fixed;
            _grid.Cols.Count = _grid.Cols.Fixed;
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        public void ShowError(string message)
        {
            _grid.Visible = false;
            _lblError.Text = message;
            _lblError.Visible = true;
        }

        /// <summary>
        /// 隐藏错误，显示网格
        /// </summary>
        public void HideError()
        {
            _lblError.Visible = false;
            _grid.Visible = true;
        }

        /// <summary>
        /// 获取当前选择的 body 区域
        /// </summary>
        public CellRange GetBodySelection()
        {
            return _grid.BodySelection;
        }

        /// <summary>
        /// 选择指定 body 单元格
        /// </summary>
        public void SelectCell(int bodyRow, int bodyCol)
        {
            _grid.BodySelect(bodyRow, bodyCol);
        }

        /// <summary>
        /// 选择指定 body 区域
        /// </summary>
        public void SelectRange(int bodyRow1, int bodyCol1, int bodyRow2, int bodyCol2)
        {
            _grid.BodySelect(bodyRow1, bodyCol1, bodyRow2, bodyCol2);
        }

        /// <summary>
        /// 获取列数（不含行号列）
        /// </summary>
        public int BodyColsCount => _grid.BodyColsCount;

        /// <summary>
        /// 获取行数（不含列头行）
        /// </summary>
        public int BodyRowsCount => _grid.BodyRowsCount;

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _rowHeaderFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
