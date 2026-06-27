﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.DTO;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class TicketNavGrid : UserControl, ISetTheme
{
    public class NavNode
    {
        private readonly List<NavNode> _children = new List<NavNode>();
        private readonly Dictionary<string, NavNode> _hash = new Dictionary<string, NavNode>();

        public NavNode()
        {
            Children = _children;
        }

        public List<NavNode> Children { get; }
        public Dictionary<string, NavNode> Hash => _hash;
        public Id64 ColId { get; set; }
        public string Text { get; set; }
        public bool IsVirtualNode { get; set; }
        public TicketRecord Record { get; set; }
        public NavNode ParentNode { get; set; }
        public int SortOrderIndex { get; set; }
        public string DisplayStringRealValue { get; set; }
        public int ValidationCheckFailedCount { get; set; }
        public bool IsInShowingVirtualChildNode { get; set; }

        public NavNode AddIfNotExist(string text, out NavNode existingNode)
        {
            if (Hash.TryGetValue(text, out existingNode))
                return null;
            existingNode = new NavNode { Text = text, ParentNode = this };
            Children.Add(existingNode);
            Hash.Add(text, existingNode);
            return existingNode;
        }

        public NavNode AddOrGet(string text)
        {
            if (_hash.TryGetValue(text, out var existing))
                return existing;
            var node = new NavNode { Text = text, ParentNode = this };
            _children.Add(node);
            _hash[text] = node;
            return node;
        }

        public NavNode AddLastCol(string text)
        {
            var node = new NavNode { Text = text, ParentNode = this };
            _children.Add(node);
            return node;
        }

        public NavNode AddLastColWithCache(string text)
        {
            var node = new NavNode { Text = text, ParentNode = this };
            _children.Add(node);
            _hash[text] = node;
            return node;
        }

        public int GetRowCount()
        {
            return _children.Sum(child => child.GetRowCount()) + 1;
        }

        public void SortChildOrderByAsc()
        {
            var sorted = _children.OrderBy(child => child.SortOrderIndex).ToList();
            _children.Clear();
            _children.AddRange(sorted);
        }

        public int GenerateChildSortIndex()
        {
            for (var i = 0; i < _children.Count; i++)
            {
                _children[i].SortOrderIndex = i + 1;
                _children[i].GenerateChildSortIndex();
            }
            return _children.Count;
        }
    }

    public class MouseDragData
    {
        public int SelectRowIndex { get; set; }
        public int MouseDownX { get; set; }
        public int MouseDownY { get; set; }
        public int DragToRowIndex { get; set; }
        public bool IsParentNodeSet { get; set; }
        public object ParentNodeKey { get; set; }
        public object ParentNodeBrotherNodeKey { get; set; }
        public bool IsAllowPutDown { get; set; }
        public bool IsPutToDragRowTop { get; set; }
    }

    public class NaveTreeNodeSortData
    {
        public NavNode NavNode { get; set; }
        public Node GridNode { get; set; }
        public int OrderIndex { get; set; }
        public List<Auditai.Model.Row> DataRowsList { get; set; } = new List<Auditai.Model.Row>();
        public Auditai.Model.Row FirstDataRow { get; set; }
        public string SortCellStringValue { get; set; }
        public ValueOperand SortCellValue { get; set; }
    }

    public class NaveTreeNodeData
    {
        public NavNode NavNode { get; set; }
        public Node GridNode { get; set; }
        public int OrderIndex { get; set; }
    }

    public Auditai.Model.Table Table { get; set; }
    public TicketRecord CurrentTicket { get; set; }
    public C1FlexGridEx Grid { get; }
    public object NavTreeID { get; set; }
    public string NavTreeName { get; set; }
    public dynamic View { get; set; }
    public List<Auditai.Model.Column> Nav { get; set; }
    public TicketTable Ticket { get; set; }
    public object NavSetting { get; set; }
    public bool IsHasFillingFormula { get; set; }
    public bool IsAllowModifyTableRowOrder { get; set; }
    public dynamic Ctx { get; set; }
    public object SelectedRecord { get; set; }

    public event EventHandler RecordSelected;
    public event EventHandler VirtualNodeSelected;

    private readonly SolidBrush _brushHoverBackground;
    private readonly SolidBrush _gridMouseOverMoreMenuIconBrush;
    private int _mouseRow = -1;
    private int _needToSortOrderRow = -1;
    private Color _contentTextDefaultColor;
    private Color _colorPositionLine;
    private bool _isInDragging;
    private bool _isMouseOverMoreMenuIcon;
    private MouseDragData _mouseDragData;
    private bool _isSuspendCollapseEvent;
    private bool _isSuspendSelectChangeEvent;
    private bool _isOnlyRunGridDefaultSelectEventProcess;
    private bool _isNeedFillVirtualNode;
    private bool _shouldSkipBodyAfterRowColChange = true;
    private int _hasFilledVirtualNodeCount;
    private List<NavNode> _needSortNodeList = new List<NavNode>();
    private bool _shouldSkipBodyAfterRowColChange2 = true;

    public List<TicketRecord> RecordList { get; private set; } = new List<TicketRecord>();
    public int SelectedVirtualNodeRowIndex { get; set; } = -1;
    public bool IsSelectedNodeBeVirtualNode { get; set; }
    public List<Tuple<Id64, string>> SelectedVirtualNode { get; set; }
    public bool ShowSortingOrderContextMenu { get; set; } = true;

    private readonly C1Command _cmdExpandAll;
    private readonly C1Command _cmdCollapseAll;
    private readonly C1Command _cmdOrderAsc;
    private readonly C1Command _cmdOrderDesc;
    private readonly C1Command _cmdShowVirtualNode;
    private readonly C1Command _cmdHideVirtualNode;
    private readonly C1CommandLink _lnkExpandAll;
    private readonly C1CommandLink _lnkCollapseAll;
    private readonly C1CommandLink _lnkOrderAsc;
    private readonly C1CommandLink _lnkOrderDesc;
    private readonly C1CommandLink _lnkShowVirtualNode;
    private readonly C1CommandLink _lnkHideVirtualNode;

    public static Color VirtualNodeTextColor { get; set; } = Color.Blue;

    private static Bitmap _menuMoreOperationWhiteImage;

    public TicketNavGrid()
    {
        _brushHoverBackground = new SolidBrush(Color.Transparent);
        _gridMouseOverMoreMenuIconBrush = new SolidBrush(Color.Black);
        _contentTextDefaultColor = Color.Black;
        RecordList = new List<TicketRecord>();
        SelectedVirtualNodeRowIndex = -1;
        IsAllowModifyTableRowOrder = true;
        ShowSortingOrderContextMenu = true;

        Grid = new C1FlexGridEx();
        Grid.Dock = DockStyle.Fill;
        Grid.ExtendLastCol = true;
        Grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
        Grid.AllowEditing = false;
        Grid.SelectionMode = SelectionModeEnum.Row;
        Grid.AllowAddNew = false;
        Grid.AllowDelete = false;
        Grid.AllowDragging = AllowDraggingEnum.None;
        Grid.AllowFiltering = false;
        Grid.AllowFreezing = AllowFreezingEnum.None;
        Grid.AllowMerging = AllowMergingEnum.None;
        Grid.AllowMergingFixed = AllowMergingEnum.None;
        Grid.AllowResizing = AllowResizingEnum.Columns;
        Grid.AllowSorting = AllowSortingEnum.None;
        Grid.FocusRect = FocusRectEnum.None;

        Grid.Rows.Count = 0;
        Grid.Rows.Fixed = 0;
        Grid.Cols.Count = 1;
        Grid.Cols.Fixed = 0;

        Grid.Tree.Column = 0;
        Grid.Rows.DefaultSize = 30;

        Controls.Add(Grid);
        View = Grid;

        ThemeManager.GetInstance().Register(this);

        _cmdExpandAll = new C1Command { Text = "全部展开" };
        _cmdExpandAll.Click += _cmdExpandAll_Click;
        _cmdCollapseAll = new C1Command { Text = "全部收缩" };
        _cmdCollapseAll.Click += _cmdCollapseAll_Click;
        _cmdOrderAsc = new C1Command { Text = "升序排序", Image = ContextResources.ctxAscending };
        _cmdOrderAsc.Click += _cmdOrderAsc_Click;
        _cmdOrderAsc.CommandStateQuery += _cmdOrderAsc_CommandStateQuery;
        _cmdOrderDesc = new C1Command { Text = "降序排序", Image = ContextResources.ctxDescending };
        _cmdOrderDesc.Click += _cmdOrderDesc_Click;
        _cmdOrderDesc.CommandStateQuery += _cmdOrderDesc_CommandStateQuery;
        _cmdShowVirtualNode = new C1Command { Text = "显示预设节点", Image = Resources.ShowNodes16 };
        _cmdShowVirtualNode.Click += _cmdShowVirtualNode_Click;
        _cmdShowVirtualNode.CommandStateQuery += _cmdShowVirtualNode_CommandStateQuery;
        _cmdHideVirtualNode = new C1Command { Text = "隐藏预设节点", Image = Resources.HideNodes16 };
        _cmdHideVirtualNode.Click += _cmdHideVirtualNode_Click;
        _cmdHideVirtualNode.CommandStateQuery += _cmdHideVirtualNode_CommandStateQuery;

        Ctx = new C1ContextMenu();
        Ctx.HideFirstDelimiter = true;
        Ctx.CommandLinks.Add(_lnkExpandAll = new C1CommandLink(_cmdExpandAll) { Delimiter = true });
        Ctx.CommandLinks.Add(_lnkCollapseAll = new C1CommandLink(_cmdCollapseAll));
        Ctx.CommandLinks.Add(_lnkOrderAsc = new C1CommandLink(_cmdOrderAsc) { Delimiter = true });
        Ctx.CommandLinks.Add(_lnkOrderDesc = new C1CommandLink(_cmdOrderDesc));
        Ctx.CommandLinks.Add(_lnkShowVirtualNode = new C1CommandLink(_cmdShowVirtualNode) { Delimiter = true });
        Ctx.CommandLinks.Add(_lnkHideVirtualNode = new C1CommandLink(_cmdHideVirtualNode));

        SetTheme();

        Grid.MouseMove += _grid_MouseMove;
        Grid.MouseLeave += _grid_MouseLeave;
        Grid.MouseDown += _grid_MouseDown;
        Grid.MouseUp += _grid_MouseUp;
        Grid.Paint += _grid_Paint;
        Grid.BodyOwnerDrawCell += _grid_BodyOwnerDrawCell;
        Grid.MouseClick += _grid_MouseClick;
        Grid.BodyAfterRowColChange += _grid_BodyAfterRowColChange;
        Grid.BeforeSelChange += _grid_BeforeSelChange;
        Grid.PaintBackground += _grid_PaintBackground;
        Grid.AfterCollapse += _grid_AfterCollapse;
        Grid.AfterScroll += _grid_AfterScroll;

        Debug.WriteLine($"[TicketNavGrid] Constructor done: Cols.Fixed={Grid.Cols.Fixed}, Cols.Count={Grid.Cols.Count}, Rows.Fixed={Grid.Rows.Fixed}, DrawMode={Grid.DrawMode}");
    }

    public void Populate(TicketRecord ticket)
    {
        CurrentTicket = ticket;
        Populate();
    }

    public void Populate()
    {
        RecordList.Clear();

        var fixedRowsCount = Grid.Rows.Fixed;
        Grid.Rows.Count = fixedRowsCount;
        Grid.Rows.Fixed = fixedRowsCount;

        Grid.BeginUpdate();
        if (Nav != null && Nav.Count > 0)
        {
            var root = MakeTree();
            Grid.Rows.Count = fixedRowsCount + root.GetRowCount() - 1;

            var i = 0;
            var level = 0;
            foreach (var child in root.Children)
            {
                AddNode(child, fixedRowsCount, ref i, ref level);
            }
        }
        Grid.EndUpdate();
    }

    private void AddNode(NavNode node, int fixedRowsCount, ref int i, ref int level)
    {
        var row = Grid.BodyGetRow(i);
        row.IsNode = true;
        var gridNode = row.Node;
        gridNode.Level = level;

        if (node.Record != null)
        {
            RecordList.Add(node.Record);
        }

        gridNode.Key = node;
        Debug.WriteLine($"[TicketNavGrid] AddNode: i={i}, Text='{node.Text}', IsVirtual={node.IsVirtualNode}, Children={node.Children.Count}, UserData after set_Key={row.UserData?.GetType().Name ?? "null"}");

        if (node.IsVirtualNode)
        {
            var range = Grid.GetCellRange(i + fixedRowsCount, 0);
            range.StyleNew.ForeColor = VirtualNodeTextColor;
        }

        i++;

        foreach (var child in node.Children)
        {
            level++;
            AddNode(child, fixedRowsCount, ref i, ref level);
            level--;
        }
    }

    private NavNode MakeTree()
    {
        _isNeedFillVirtualNode = false;
        _needSortNodeList = null;
        var root = new NavNode();

        if (Nav == null || !Nav.Any())
        {
            Debug.WriteLine("[TicketNavGrid] MakeTree: Nav is null or empty");
            return root;
        }

        var hasComboList = GetContainsComboListColumn(Nav);
        var lastNavColumn = Nav.Last();
        var records = Ticket?.Records ?? new List<TicketRecord>();
        Debug.WriteLine($"[TicketNavGrid] MakeTree: Nav.Count={Nav.Count}, hasComboList={(hasComboList != null ? hasComboList.Count.ToString() : "null")}, records.Count={records.Count}");

        foreach (var record in records)
        {
            var firstRow = record.Rows[0];
            var currentNode = root;

            foreach (var column in Nav)
            {
                var cell = firstRow.Table[firstRow.Index, column.Index];
                var value = cell.GetDisplayValue(true);

                if (column == lastNavColumn)
                {
                    if (hasComboList != null)
                    {
                        currentNode = currentNode.AddLastColWithCache(value);
                    }
                    else
                        currentNode = currentNode.AddLastCol(value);

                    currentNode.Record = record;
                    currentNode.ColId = column.Id;
                }
                else
                {
                    currentNode = currentNode.AddOrGet(value);
                    currentNode.ColId = column.Id;
                }

                var realValue = firstRow.Table[firstRow.Index, column.Index].GetDisplayValue(true);
                currentNode.DisplayStringRealValue = realValue;
            }
        }

        if (hasComboList != null)
        {
            Debug.WriteLine($"[TicketNavGrid] MakeTree: before FillVirtualNode, root.Children.Count={root.Children.Count}");
            FillVirtualNode(root, hasComboList);
            Debug.WriteLine($"[TicketNavGrid] MakeTree: after FillVirtualNode, root.Children.Count={root.Children.Count}, _hasFilledVirtualNodeCount={_hasFilledVirtualNodeCount}");
        }

        if (_needSortNodeList != null && _needSortNodeList.Count > 0)
        {
            foreach (var node in _needSortNodeList)
            {
                node.SortChildOrderByAsc();
            }
        }

        return root;
    }

    private void FillVirtualNode(NavNode node, HashSet<Auditai.Model.Column> hasComboList)
    {
        if (Ticket.Table.IsLocked) { Debug.WriteLine("[TicketNavGrid] FillVirtualNode: Table is locked"); return; }
        if (IsHasFillingFormula) { Debug.WriteLine("[TicketNavGrid] FillVirtualNode: IsHasFillingFormula"); return; }

        _isNeedFillVirtualNode = true;
        if (!Ticket.IsAllowShowVirtualNode) { Debug.WriteLine("[TicketNavGrid] FillVirtualNode: IsAllowShowVirtualNode=false"); return; }
        _needSortNodeList = new List<NavNode>();
        Debug.WriteLine($"[TicketNavGrid] FillVirtualNode: enter, Nav.Count={Nav.Count}");

        VirtualTable virtualTable = null;

        for (var i = 0; i < Nav.Count; i++)
        {
            var column = Nav[i];
            if (!hasComboList.Contains(column)) continue;
            if (!TableEditor.IsCurrentUserCanEditColumn(column)) continue;

            var referredColumns = GetReferredSameTableOtherColumns(column);

            if (referredColumns.Count == 0)
            {
                Debug.WriteLine($"[TicketNavGrid] FillVirtualNode: col[{i}]='{column.Caption}' Branch1(no referred), calling GetComboListValue");
                var values = GetComboListValue(column);
                if (values == null) { Debug.WriteLine($"[TicketNavGrid] FillVirtualNode: col[{i}] GetComboListValue returned null"); continue; }
                Debug.WriteLine($"[TicketNavGrid] FillVirtualNode: col[{i}] calling FillVirtualNavNodeValue(values, count={values.Count})");
                FillVirtualNavNodeValue(node, i, values, column);
            }
            else
            {
                if (referredColumns.Any(x => Nav.Contains(x)))
                {
                    Debug.WriteLine($"[TicketNavGrid] FillVirtualNode: col[{i}]='{column.Caption}' Branch2b(evaluator)");
                    var evaluator = GenerateColComboListEvaluator(column);
                    if (evaluator == null) { Debug.WriteLine($"[TicketNavGrid] FillVirtualNode: col[{i}] evaluator=null"); continue; }

                    if (virtualTable == null)
                    {
                        virtualTable = new VirtualTable(1, Ticket.Table.Columns.Count);
                        virtualTable.SetDefaultStyle(Ticket.Table.DefaultStyle);
                    }
                    FillVirtualNavNodeValue(node, i, evaluator, virtualTable, column);
                }
                else
                {
                    Debug.WriteLine($"[TicketNavGrid] FillVirtualNode: col[{i}]='{column.Caption}' Branch2a(no Nav match), calling GetComboListValue");
                    var values = GetComboListValue(column);
                    if (values == null) continue;
                    FillVirtualNavNodeValue(node, i, values, column);
                }
            }
        }
    }

    private void FillVirtualNavNodeValue(NavNode node, int level, List<Tuple<string, string>> values, Auditai.Model.Column column)
    {
        VistNavTree(node, 0);
        void VistNavTree(NavNode parent, int currentLevel)
        {
            if (currentLevel == level)
            {
                if (_needSortNodeList != null)
                {
                    _needSortNodeList.Add(parent);
                }
                int sortIndex = parent.GenerateChildSortIndex();
                sortIndex++;
                parent.IsInShowingVirtualChildNode = true;

                foreach (var value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value.Item1))
                    {
                        NavNode navNode = parent.AddIfNotExist(value.Item1, out var existingNode);
                        if (navNode != null)
                        {
                            navNode.ColId = column.Id;
                            navNode.IsVirtualNode = true;
                            navNode.DisplayStringRealValue = value.Item2;
                            navNode.SortOrderIndex = sortIndex;
                            sortIndex++;
                            _hasFilledVirtualNodeCount++;
                            if (_hasFilledVirtualNodeCount >= 10000)
                            {
                                return;
                            }
                        }
                        else if (existingNode != null)
                        {
                            existingNode.SortOrderIndex = sortIndex;
                            sortIndex++;
                        }
                    }
                }
                return;
            }
            if (currentLevel < level)
            {
                var children = parent.Children;
                if (children == null) return;
                foreach (var child in children)
                {
                    VistNavTree(child, currentLevel + 1);
                    if (_hasFilledVirtualNodeCount >= 10000)
                    {
                        return;
                    }
                }
            }
        }
    }

    private void FillVirtualNavNodeValue(NavNode node, int level, FormulaEvaluator evaluator, VirtualTable virtualTable, Auditai.Model.Column column)
    {
        VistNavTree(node, 0);
        void VistNavTree(NavNode parent, int currentLevel)
        {
            if (currentLevel == level)
            {
                if (_needSortNodeList != null)
                {
                    _needSortNodeList.Add(parent);
                }
                var values = GetComboListValueByVirtualTable(evaluator, virtualTable, column);
                if (values == null) return;
                int sortIndex = parent.GenerateChildSortIndex();
                sortIndex++;
                parent.IsInShowingVirtualChildNode = true;

                foreach (var value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value.Item1))
                    {
                        NavNode navNode = parent.AddIfNotExist(value.Item1, out var existingNode);
                        if (navNode != null)
                        {
                            navNode.ColId = column.Id;
                            navNode.IsVirtualNode = true;
                            navNode.DisplayStringRealValue = value.Item2;
                            navNode.SortOrderIndex = sortIndex;
                            sortIndex++;
                            _hasFilledVirtualNodeCount++;
                            if (_hasFilledVirtualNodeCount >= 10000)
                            {
                                return;
                            }
                        }
                        else if (existingNode != null)
                        {
                            existingNode.SortOrderIndex = sortIndex;
                            sortIndex++;
                        }
                    }
                }
                return;
            }
            if (currentLevel < level)
            {
                var children = parent.Children;
                if (children == null) return;
                foreach (var child in children)
                {
                    var col = Ticket.Table.Columns.GetById(child.ColId);
                    UpdateVirtualTableCellValue(col, child.DisplayStringRealValue);
                    VistNavTree(child, currentLevel + 1);
                    UpdateVirtualTableCellValue(col, null);
                    if (_hasFilledVirtualNodeCount >= 10000)
                    {
                        return;
                    }
                }
            }
        }

        void UpdateVirtualTableCellValue(Auditai.Model.Column col, string cellValue)
        {
            if (col == null) return;
            if (cellValue == null) cellValue = string.Empty;
            var data = Auditai.Model.Cell.ChangeDataTypeImpl(cellValue, col.GetDataType());
            virtualTable.ResetCellInstance(0, col.Index, data);
        }
    }

    public void Reset()
    {
        try
        {
            Grid.BeginUpdate();
            RecordList.Clear();
            SelectedVirtualNodeRowIndex = -1;
            Nav = null;
            SelectedRecord = null;
            IsSelectedNodeBeVirtualNode = false;
            SelectedVirtualNode = null;
            NavSetting = null;
            NavTreeID = null;
            Ticket = null;
            NavTreeName = null;
            IsHasFillingFormula = false;
            _mouseRow = -1;
            _needToSortOrderRow = -1;
            _hasFilledVirtualNodeCount = 0;
            _isSuspendCollapseEvent = false;
            _isInDragging = false;
            _mouseDragData = null;
            _isSuspendSelectChangeEvent = false;
            _isOnlyRunGridDefaultSelectEventProcess = false;
            _isNeedFillVirtualNode = false;
            IsAllowModifyTableRowOrder = true;
            _shouldSkipBodyAfterRowColChange = true;
            Grid.Rows.Count = 0;
            _shouldSkipBodyAfterRowColChange = false;
        }
        catch { }
        finally { Grid.EndUpdate(); }
    }

    private void _grid_MouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            var cursor = Grid.Cursor;
            var isNeedInvalidate = false;
            var hitTest = Grid.HitTest();

            if (_mouseRow != hitTest.Row)
            {
                _mouseRow = hitTest.Row;
                isNeedInvalidate = true;
            }

            // 计算是否进入拖拽状态
            var isInDragging = false;
            if (_mouseDragData != null && !Ticket.Table.IsLocked)
            {
                if (_isInDragging)
                {
                    isInDragging = true;
                }
                else if (Math.Abs(e.X - _mouseDragData.MouseDownX) >= 3 ||
                         Math.Abs(e.Y - _mouseDragData.MouseDownY) >= 3)
                {
                    isInDragging = true;
                }

                if (!IsAllowModifyTableRowOrder)
                {
                    isInDragging = false;
                }
            }
            _isInDragging = isInDragging;

            if (!_isInDragging)
            {
                // 非拖拽：箭头光标 + 检查"更多菜单"图标悬停
                cursor = Cursors.Arrow;

                if (_mouseRow != -1)
                {
                    var iconRect = GetGridRowMoreMenuIconBackgroundRectangle(_mouseRow);
                    var isMouseOverMoreMenuIcon = iconRect.Contains(e.X, e.Y);
                    if (isMouseOverMoreMenuIcon != _isMouseOverMoreMenuIcon)
                    {
                        isNeedInvalidate = true;
                    }
                    _isMouseOverMoreMenuIcon = isMouseOverMoreMenuIcon;
                }
                else
                {
                    _isMouseOverMoreMenuIcon = false;
                }
            }
            else
            {
                // 拖拽中：计算父节点信息 + 判断是否可放置
                isNeedInvalidate = true;

                // 首次进入拖拽时计算父节点和兄弟节点
                if (!_mouseDragData.IsParentNodeSet)
                {
                    _mouseDragData.IsParentNodeSet = true;
                    _mouseDragData.ParentNodeKey = null;
                    _mouseDragData.ParentNodeBrotherNodeKey = null;

                    if (_mouseDragData.SelectRowIndex >= 0 && _mouseDragData.SelectRowIndex < Grid.Rows.Count)
                    {
                        var selectRow = Grid.Rows[_mouseDragData.SelectRowIndex];
                        if (selectRow.Node.Parent != null)
                        {
                            _mouseDragData.ParentNodeKey = selectRow.Node.Parent.Key;

                            var parentRowIndex = selectRow.Node.Parent.Row.Index;
                            var totalRows = Grid.Rows.Count;
                            var level = selectRow.Node.Parent.Row.Node.Level;

                            for (var i = parentRowIndex + 1; i < totalRows; i++)
                            {
                                var row = Grid.Rows[i];
                                if (row.Node.Level > level)
                                {
                                    continue;
                                }
                                if (row.Node.Level != level)
                                {
                                    break;
                                }
                                _mouseDragData.ParentNodeBrotherNodeKey = row.Node.Key;
                                break;
                            }
                        }
                    }
                }

                // 默认禁用拖拽光标
                cursor = TicketInputEditor2.CursorDisableDrag;

                _mouseDragData.DragToRowIndex = hitTest.Row;
                _mouseDragData.IsAllowPutDown = false;

                if (hitTest.Row != -1)
                {
                    var cellRect = Grid.GetCellRect(hitTest.Row, 0);
                    _mouseDragData.IsPutToDragRowTop = e.Y <= cellRect.Y + cellRect.Height / 2;

                    var row = Grid.Rows[hitTest.Row];
                    if (row.IsNode)
                    {
                        var isAllowPutDown = false;

                        // 1. 拖到根节点：仅当源也是根节点时允许
                        if (row.Node.Parent == null && _mouseDragData.ParentNodeKey == null)
                        {
                            isAllowPutDown = true;
                        }
                        // 2. 拖到同父节点下的兄弟节点：允许
                        else if (row.Node.Parent != null && row.Node.Parent.Key == _mouseDragData.ParentNodeKey)
                        {
                            isAllowPutDown = true;
                        }
                        // 3. 拖到源父节点位置且放在下方：允许
                        else if (row.Node.Key == _mouseDragData.ParentNodeKey && !_mouseDragData.IsPutToDragRowTop)
                        {
                            isAllowPutDown = true;
                        }
                        // 4. 拖到源父节点的下一个兄弟节点位置且放在上方：允许
                        else if (row.Node.Key == _mouseDragData.ParentNodeBrotherNodeKey && _mouseDragData.IsPutToDragRowTop)
                        {
                            isAllowPutDown = true;
                        }

                        if (isAllowPutDown)
                        {
                            _mouseDragData.IsAllowPutDown = true;
                            cursor = TicketInputEditor2.CursorInDragging;
                        }
                    }
                }

                _isMouseOverMoreMenuIcon = false;
            }

            if (cursor != Grid.Cursor)
            {
                Grid.Cursor = cursor;
            }

            if (isNeedInvalidate)
            {
                Grid.Invalidate();
            }
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "拖拽表单导航树节点时发生了未预期的异常");
        }
    }

    private void _grid_MouseLeave(object sender, EventArgs e)
    {
        _isMouseOverMoreMenuIcon = false;
        _mouseRow = -1;
        Grid.Invalidate();
    }

    private void _grid_MouseDown(object sender, MouseEventArgs e)
    {
        if (Program.MainForm.CurrentView != MainFormView.TicketInput) return;

        if (_mouseDragData == null)
        {
            _mouseDragData = new MouseDragData();
        }

        var hitTest = Grid.HitTest();
        if (hitTest.Type != HitTestTypeEnum.Cell)
        {
            _mouseDragData.SelectRowIndex = -1;
            _mouseDragData.MouseDownX = e.X;
            _mouseDragData.MouseDownY = e.Y;
            return;
        }

        _mouseDragData.SelectRowIndex = hitTest.Row;
        _mouseDragData.MouseDownX = e.X;
        _mouseDragData.MouseDownY = e.Y;

        if (hitTest.Row < 0 || hitTest.Row >= Grid.Rows.Count)
        {
            _mouseDragData.SelectRowIndex = -1;
            return;
        }

        if (!IsAllowModifyTableRowOrder)
        {
            _mouseDragData = null;
            return;
        }

        var row = Grid.Rows[hitTest.Row];
        var navNode = row.UserData as NavNode;
        if (navNode == null || navNode.ParentNode == null || !navNode.ParentNode.IsInShowingVirtualChildNode)
        {
            _mouseDragData = null;
        }
    }

    private void _grid_MouseUp(object sender, MouseEventArgs e)
    {
        if (_isInDragging)
        {
            _isInDragging = false;
            DoDragDropAction();
            Grid.Invalidate();
        }
        _mouseDragData = null;
    }

    private void _grid_Paint(object sender, PaintEventArgs e)
    {
        // 原始实现仅含 ret，无需绘制逻辑
    }

    private void _grid_PaintBackground(object sender, PaintEventArgs e)
    {
        if (!Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.Picture)) return;

        var grid = Grid;
        var parent = grid.Parent;
        var gridLocation = grid.Location;
        var screenPoint = parent.PointToScreen(gridLocation);
        var mainForm = Program.MainForm;
        var relativePoint = mainForm.PnlMainRelativePosition(screenPoint);
        var bgImage = Theme.CurrentBackgroudImage;

        e.Graphics.DrawImage(bgImage, 0, 0,
            new Rectangle(relativePoint.X, relativePoint.Y, bgImage.Width - relativePoint.X, bgImage.Height - relativePoint.Y),
            GraphicsUnit.Pixel);
    }

    private void _grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
    {
        try
        {
            bool isImageEmpty = false;
            var displayClass = new DisplayClass133_0 { e = e, Parent = this };
            if (_isInDragging && _mouseDragData != null)
            {
                if (_mouseDragData.DragToRowIndex == e.Row)
                {
                    if (_mouseDragData.IsAllowPutDown)
                    {
                        if (_mouseDragData.SelectRowIndex != -1 && _mouseDragData.DragToRowIndex != -1)
                        {
                            var dragToRect = Grid.GetCellRect(_mouseDragData.DragToRowIndex, 0);
                            using (var pen = new Pen(_colorPositionLine, 2))
                            {
                                var x1 = dragToRect.X + 4;
                                var x2 = dragToRect.Right - 4;
                                var y = _mouseDragData.IsPutToDragRowTop
                                    ? dragToRect.Y - 1
                                    : dragToRect.Bottom - 1;
                                // 原始 IL: y 和 yAdjusted 是两个变量，y<0 时都设为 1，x2 不变
                                var yAdjusted = y;
                                if (y < 0)
                                {
                                    y = 1;
                                    yAdjusted = 1;
                                }
                                e.Graphics.DrawLine(pen, x1, y, x2, yAdjusted);
                            }
                        }
                    }
                }
            }
            else if (e.Row == _mouseRow)
            {
                e.Graphics.FillRectangle(_brushHoverBackground, e.Bounds);
            }

            if (!_isInDragging && e.Row == _mouseRow)
            {
                var image = _grid_OwnerDrawCell_g__GetGridRowMoreMenuImage(ref isImageEmpty, ref displayClass);
                e.DrawCell(DrawCellFlags.All);
                e.Handled = true;

                if (_isMouseOverMoreMenuIcon)
                {
                    if (!isImageEmpty)
                    {
                        _gridMouseOverMoreMenuIconBrush.Color = Auditai.UI.Controls.Util.LightColor(e.Style.BackColor, 0.2);
                    }
                    else
                    {
                        _gridMouseOverMoreMenuIconBrush.Color = Auditai.UI.Controls.Util.DarkenColor(Grid.Styles.SelectedColumnHeader.BackColor, 0.1);
                    }
                    e.Graphics.FillRectangle(_gridMouseOverMoreMenuIconBrush, GetGridRowMoreMenuIconBackgroundRectangle(e.Row));
                }

                var point = GetGridRowMoreMenuIconLeftTopPosition(e.Row);
                e.Graphics.DrawImage(image, point);
            }
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, null);
        }
    }

    private Bitmap _grid_OwnerDrawCell_g__GetGridRowMoreMenuImage(ref bool isImageEmpty, ref DisplayClass133_0 displayClass)
    {
        // 原始 IL: <_grid_OwnerDrawCell>g__GetGridRowMoreMenuImage|133_0 (IL=35)
        // 非高亮行直接返回默认图标；高亮行根据主题决定是否使用白色图标
        isImageEmpty = false;
        if (displayClass.e.Style.Name != Grid.Styles.Highlight.Name)
        {
            return Resources.menuMoreOperation;
        }
        if (Theme.SelectedAuditaiTheme.ThemeContext.GridMoreMenuImageIndexOnHighLightRow == GridMoreMenuImageIndex.White)
        {
            if (_menuMoreOperationWhiteImage == null)
            {
                _menuMoreOperationWhiteImage = (Bitmap)new WhiteImageStrategy().ProcessImage(Resources.menuMoreOperation);
            }
            isImageEmpty = true;
            return _menuMoreOperationWhiteImage;
        }
        return Resources.menuMoreOperation;
    }

    private class DisplayClass133_0
    {
        public OwnerDrawCellEventArgs e;
        public TicketNavGrid Parent;
        public bool isImageEmpty;
    }

    private void _grid_BodyOwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
    {
        try
        {
            var row = Grid.BodyGetRow(e.Row);
            var navNode = row.UserData as NavNode;
            if (navNode == null)
            {
                var nodeKey = row.Node?.Key as NavNode;
                Debug.WriteLine($"[TicketNavGrid] BodyOwnerDrawCell: UserData null! e.Row={e.Row}, IsNode={row.IsNode}, Node?.Key.Text='{nodeKey?.Text ?? "null"}', Node={row.Node?.GetType().Name ?? "null"}");
                return;
            }
            Debug.WriteLine($"[TicketNavGrid] BodyOwnerDrawCell: e.Row={e.Row}, Text='{navNode.Text}', IsVirtual={navNode.IsVirtualNode}");

            e.Text = string.IsNullOrEmpty(navNode.Text) ? "(空)" : navNode.Text;

            if (navNode.IsVirtualNode)
            {
                var range = Grid.GetCellRange(e.Row, e.Col);
                range.StyleNew.ForeColor = VirtualNodeTextColor;
            }
            else if (Program.MainForm.ShowHelperTooltip)
            {
                if (Program.MainForm.NodeValidationResults.TryGetValue(Ticket.Table.TreeNode, out var count))
                {
                    if (navNode.ValidationCheckFailedCount > 0)
                    {
                        e.Text = string.Format("{0} ({1})", e.Text, navNode.ValidationCheckFailedCount);
                        var range = Grid.GetCellRange(e.Row, e.Col);
                        range.StyleNew.ForeColor = Color.Red;
                    }
                    else
                    {
                        var range = Grid.GetCellRange(e.Row, e.Col);
                        range.StyleNew.ForeColor = Color.Green;
                    }
                }
                else
                {
                    var range = Grid.GetCellRange(e.Row, e.Col);
                    range.StyleNew.ForeColor = _contentTextDefaultColor;
                }
            }
            else
            {
                var range = Grid.GetCellRange(e.Row, e.Col);
                range.StyleNew.ForeColor = _contentTextDefaultColor;
            }

            if (navNode.Children.Count == 0)
            {
                var existingImage = Grid.GetCellImage(e.Row, e.Col);
                if (existingImage != null)
                {
                    e.Image = existingImage;
                }
                else
                {
                    e.Image = navNode.IsVirtualNode ? Resources.VirtualTicket16 : Resources.Ticket16;
                }
            }
            else if (row.IsNode)
            {
                var existingImage = Grid.GetCellImage(e.Row, e.Col);
                if (existingImage != null)
                {
                    e.Image = existingImage;
                }
                else if (row.Node.Collapsed)
                {
                    e.Image = navNode.IsVirtualNode ? Resources.VirtualTicketNavTreeListCollapsed : Resources.TicketNavTreeListCollapsed;
                }
                else
                {
                    e.Image = navNode.IsVirtualNode ? Resources.VirtualTicketNavTreeListExpanded : Resources.TicketNavTreeListExpanded;
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private void _grid_MouseClick(object sender, MouseEventArgs e)
    {
        if (_isInDragging) return;

        if (e.Button == MouseButtons.Left)
        {
            if (_mouseRow >= 0 && _isMouseOverMoreMenuIcon && !Grid.IsRowIndexOutOfRange(_mouseRow))
            {
                Grid.Row = _mouseRow;
                PrepareToShowCtx(e);
                Ctx.ShowContextMenu(Grid, e.Location);
                return;
            }

            var hitTest = Grid.HitTest();
            if (hitTest.Type == HitTestTypeEnum.Cell)
            {
                var row = Grid.Rows[hitTest.Row];
                if (row.IsNode)
                {
                    row.Node.Collapsed = !row.Node.Collapsed;
                    Program.MainForm.TicketInputEditor.RefreshNavTreeNodeFlickImage();
                }
            }
            return;
        }

        if (e.Button == MouseButtons.Right)
        {
            PrepareToShowCtx(e);
            Ctx.ShowContextMenu(Grid, e.Location);
        }
    }

    private void _grid_BodyAfterRowColChange(object sender, RangeEventArgs e)
    {
        if (_shouldSkipBodyAfterRowColChange) return;

        var rowIndex = Grid.Row;
        if (rowIndex < 0 || rowIndex >= Grid.Rows.Count) return;

        IsSelectedNodeBeVirtualNode = false;
        var row = Grid.BodyGetRow(rowIndex);
        var navNode = row.UserData as NavNode;
        if (navNode == null) return;

        if (navNode.Record != null)
        {
            SelectedRecord = navNode.Record;
            if (_isOnlyRunGridDefaultSelectEventProcess) return;
            RecordSelected?.Invoke(this, EventArgs.Empty);
        }
        else if (navNode.IsVirtualNode)
        {
            IsSelectedNodeBeVirtualNode = true;
            if (navNode.Children == null || navNode.Children.Count > 0) return;

            var list = new List<Tuple<Id64, string>>();
            var parentNode = row.Node;
            while (parentNode != null)
            {
                var key = parentNode.Key as NavNode;
                if (key != null)
                {
                    list.Add(Tuple.Create(key.ColId, key.Text));
                }
                parentNode = parentNode.Parent;
            }
            SelectedVirtualNode = list;
            SelectedVirtualNodeRowIndex = rowIndex;
            if (_isOnlyRunGridDefaultSelectEventProcess) return;
            VirtualNodeSelected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void _grid_BeforeSelChange(object sender, RangeEventArgs e)
    {
        if (_isOnlyRunGridDefaultSelectEventProcess) return;
        if (_isSuspendSelectChangeEvent)
        {
            e.Cancel = true;
            return;
        }
        if (_isInDragging)
        {
            e.Cancel = true;
        }
    }

    private void _grid_AfterCollapse(object sender, RowColEventArgs e)
    {
        if (_isSuspendCollapseEvent) return;
        if (Ticket == null || Ticket.Table == null) return;
        TicketNavTreeStatusDataCacher.ProcessNavTreeCollapseEvent(Ticket.Table.Id, NavTreeID as TicketNavTreeID, this);
    }

    private void _grid_AfterScroll(object sender, RangeEventArgs e)
    {
        if (Ticket == null || Ticket.Table == null) return;
        TicketNavTreeStatusDataCacher.ProcessNavTreeScrollEvent(Ticket.Table.Id, NavTreeID as TicketNavTreeID, this);
    }

    private void _cmdExpandAll_Click(object sender, ClickEventArgs e)
    {
        _isSuspendCollapseEvent = true;
        try
        {
            Grid.ExpandAll();
            Program.MainForm.TicketInputEditor.RefreshNavTreeNodeFlickImage();
        }
        finally
        {
            _isSuspendCollapseEvent = false;
            if (Ticket != null)
            {
                TicketNavTreeStatusDataCacher.ProcessNavTreeCollapseEvent(
                    Ticket.Table.Id, NavTreeID as TicketNavTreeID, this);
            }
        }
    }

    private void _cmdCollapseAll_Click(object sender, ClickEventArgs e)
    {
        CollapseAll();
    }

    private void _cmdOrderAsc_Click(object sender, ClickEventArgs e)
    {
        if (Program.MainForm.CurrentView != MainFormView.TicketInput) return;
        if (_needToSortOrderRow == -1) return;
        Program.MainForm.TicketInputEditor.SaveCurrentSelectdRecordByRowId();
        if (SortNavNodeRecord(_needToSortOrderRow, true))
        {
            Program.MainForm.TicketInputEditor.Populate(false);
        }
    }

    private void _cmdOrderDesc_Click(object sender, ClickEventArgs e)
    {
        if (Program.MainForm.CurrentView != MainFormView.TicketInput) return;
        if (_needToSortOrderRow == -1) return;
        Program.MainForm.TicketInputEditor.SaveCurrentSelectdRecordByRowId();
        if (SortNavNodeRecord(_needToSortOrderRow, false))
        {
            Program.MainForm.TicketInputEditor.Populate(false);
        }
    }

    private void _cmdShowVirtualNode_Click(object sender, ClickEventArgs e)
    {
        Ticket.IsAllowShowVirtualNode = true;
        Ticket.Table.TagTicketDirty(true);

        if (Program.MainForm.CurrentView != MainFormView.TicketInput) return;

        try
        {
            Program.MainForm.TicketInputEditor.SaveCurrentSelectdRecordByRowId();
            Program.MainForm.TicketInputEditor.SuspendDrawing();
            Program.MainForm.TicketInputEditor.Populate(false);
        }
        finally
        {
            Program.MainForm.TicketInputEditor.ResumeDrawing();
        }
    }

    private void _cmdHideVirtualNode_Click(object sender, ClickEventArgs e)
    {
        Ticket.IsAllowShowVirtualNode = false;
        Ticket.Table.TagTicketDirty(true);

        if (Program.MainForm.CurrentView != MainFormView.TicketInput) return;

        try
        {
            Program.MainForm.TicketInputEditor.SaveCurrentSelectdRecordByRowId();
            Program.MainForm.TicketInputEditor.SuspendDrawing();
            Program.MainForm.TicketInputEditor.Populate(false);
        }
        finally
        {
            Program.MainForm.TicketInputEditor.ResumeDrawing();
        }
    }

    private void _cmdOrderAsc_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        // Always visible
    }

    private void _cmdOrderDesc_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        // Always visible
    }

    private void _cmdShowVirtualNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        e.Visible = _isNeedFillVirtualNode && Ticket?.IsAllowShowVirtualNode == true;
    }

    private void _cmdHideVirtualNode_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        e.Visible = _isNeedFillVirtualNode && Ticket?.IsAllowShowVirtualNode != true;
    }

    public void CmdDeleteTicket_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        e.Visible = true;
        e.Enabled = SelectedRecord != null && !IsHasFillingFormula;
    }

    public void CmdAddTicket_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
    {
        e.Visible = true;
        e.Enabled = Table != null;
    }

    public void SetTheme() { SetTheme(null); }
    public void SetTheme(params object[] args)
    {
        try
        {
            Theme.SetCurrentObject(Grid);
            Grid.Styles.Alternate.Clear();
            Grid.Styles.EmptyArea.BackColor = Color.Transparent;
            Grid.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
            Grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
            _brushHoverBackground.Color = Color.FromArgb(100, Theme.SelectedAuditaiTheme.GetBackgroundSolidColor("C1FlexGrid\\Styles\\Highlight\\Background"));
            _colorPositionLine = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("C1FlexGrid\\Styles\\Normal\\Border\\Color");
            _contentTextDefaultColor = Theme.SelectedAuditaiTheme.GetC1Theme().GetColor("BaseThemeProperties\\Styles\\Content\\ForeColor", Color.Black);
            _gridMouseOverMoreMenuIconBrush.Color = Auditai.UI.Controls.Util.DarkenColor(_brushHoverBackground.Color, 0.1);

            if (Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.Picture))
            {
                Grid.Styles.Alternate.BackColor = Color.Transparent;
                Grid.Styles.EmptyArea.BackColor = Color.Transparent;
            }
            else
            {
                Grid.Styles.EmptyArea.BackColor = Color.White;
            }
        }
        catch { }
    }

    public void PrepareToShowCtx(MouseEventArgs e)
    {
        var hitTest = Grid.HitTest();
        var hitType = hitTest.Type;

        if (hitType == HitTestTypeEnum.Cell)
        {
            _needToSortOrderRow = hitTest.Row;
            _cmdOrderAsc.Enabled = true;
            _cmdOrderDesc.Enabled = true;
        }
        else
        {
            _needToSortOrderRow = -1;
            _cmdOrderAsc.Enabled = false;
            _cmdOrderDesc.Enabled = false;
        }

        if (Program.MainForm.CurrentView != MainFormView.TicketInput) return;

        var canModify = false;
        if (IsAllowModifyTableRowOrder)
        {
            if (hitTest.Row >= 0 && hitTest.Row < Grid.Rows.Count)
            {
                var row = Grid.Rows[hitTest.Row];
                var navNode = row.UserData as NavNode;
                if (navNode != null && navNode.ParentNode != null && navNode.ParentNode.IsInShowingVirtualChildNode)
                {
                    canModify = true;
                }
            }
        }

        _cmdOrderAsc.Visible = canModify;
        _cmdOrderDesc.Visible = canModify;

        if (!canModify)
        {
            _cmdOrderAsc.Visible = false;
            _cmdOrderDesc.Visible = false;
        }
    }

    public bool SortNavNodeRecord(int sortLevel, bool isAsc)
    {
        if (sortLevel < 0 || sortLevel >= Grid.Rows.Count)
        {
            return false;
        }
        C1.Win.C1FlexGrid.Row row = Grid.Rows[sortLevel];
        TicketNavGrid.NavNode rowData = row.UserData as TicketNavGrid.NavNode;
        if (rowData == null)
        {
            return false;
        }
        Auditai.Model.Column sortCol = this.Ticket.Table.Columns.GetById(rowData.ColId);
        if (sortCol == null)
        {
            return false;
        }
        Type colDataType = sortCol.GetFormat().GetDataType();
        bool isStringSort = colDataType == typeof(string);
        List<TicketNavGrid.NaveTreeNodeSortData> brotherNodeList = SortNavNodeRecord_g__GetNavTreeNodeBrotherNodes(sortLevel);
        List<TicketNavGrid.NaveTreeNodeSortData> navNodeContainDataRowList = new List<TicketNavGrid.NaveTreeNodeSortData>();
        foreach (TicketNavGrid.NaveTreeNodeSortData nodeData in brotherNodeList)
        {
            List<Auditai.Model.Row> dataRowsList = new List<Auditai.Model.Row>();
            SortNavNodeRecord_g__GetNavNodeAllDataRows(nodeData.NavNode, dataRowsList);
            if (dataRowsList.Count != 0)
            {
                nodeData.DataRowsList = dataRowsList;
                dataRowsList.Sort(delegate(Auditai.Model.Row left, Auditai.Model.Row right)
                {
                    if (left.Index < right.Index)
                    {
                        return -1;
                    }
                    if (left.Index > right.Index)
                    {
                        return 1;
                    }
                    return 0;
                });
                nodeData.FirstDataRow = dataRowsList[0];
                Auditai.Model.Cell cell = this.Ticket.Table.Cells.Get(nodeData.FirstDataRow.Index, sortCol.Index);
                if (isStringSort)
                {
                    nodeData.SortCellStringValue = this.GetNodeDisplayName(cell.GetDisplayValue(true));
                }
                else
                {
                    nodeData.SortCellValue = Auditai.Model.Cell.ChangeToValueOperand(cell.Value, colDataType);
                }
                navNodeContainDataRowList.Add(nodeData);
            }
        }
        if (navNodeContainDataRowList.Count <= 1)
        {
            return false;
        }
        if (isAsc)
        {
            if (isStringSort)
            {
                navNodeContainDataRowList = navNodeContainDataRowList.OrderByCellValue((TicketNavGrid.NaveTreeNodeSortData u) => u.SortCellStringValue).ToList<TicketNavGrid.NaveTreeNodeSortData>();
            }
            else
            {
                navNodeContainDataRowList = navNodeContainDataRowList.OrderByCellValue((TicketNavGrid.NaveTreeNodeSortData u) => u.SortCellValue.Object).ToList<TicketNavGrid.NaveTreeNodeSortData>();
            }
        }
        else if (isStringSort)
        {
            navNodeContainDataRowList = navNodeContainDataRowList.OrderByCellValueDescending((TicketNavGrid.NaveTreeNodeSortData u) => u.SortCellStringValue).ToList<TicketNavGrid.NaveTreeNodeSortData>();
        }
        else
        {
            navNodeContainDataRowList = navNodeContainDataRowList.OrderByCellValueDescending((TicketNavGrid.NaveTreeNodeSortData u) => u.SortCellValue.Object).ToList<TicketNavGrid.NaveTreeNodeSortData>();
        }
        SortNavNodeRecord_g__FirstMove(navNodeContainDataRowList);
        SortNavNodeRecord_g__SecondMove(navNodeContainDataRowList);
        return true;
    }

    // 原始 IL: <SortNavNodeRecord>g__GetNavTreeNodeBrotherNodes|105_4 IL=107
    // 获取与 rowIndex 同级的所有兄弟节点
    // 若为根节点（无 Parent），遍历整个 Grid 所有同 Level 行
    // 否则遍历 Parent.Nodes 数组
    private List<NaveTreeNodeSortData> SortNavNodeRecord_g__GetNavTreeNodeBrotherNodes(int rowIndex)
    {
        var result = new List<NaveTreeNodeSortData>();
        var row = Grid.Rows[rowIndex];
        int level = row.Node.Level;

        if (row.Node.Parent == null)
        {
            int rowsCount = Grid.Rows.Count;
            for (int i = 0; i < rowsCount; i++)
            {
                var r = Grid.Rows[i];
                if (r.Node.Level != level) continue;
                var data = new NaveTreeNodeSortData
                {
                    NavNode = r.Node.Key as NavNode,
                    GridNode = r.Node,
                    OrderIndex = result.Count
                };
                result.Add(data);
            }
        }
        else
        {
            var parent = row.Node.Parent;
            var nodes = parent.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var data = new NaveTreeNodeSortData
                {
                    NavNode = node.Key as NavNode,
                    GridNode = node,
                    OrderIndex = result.Count
                };
                result.Add(data);
            }
        }
        return result;
    }

    // 原始 IL: <SortNavNodeRecord>g__GetNavNodeAllDataRows|105_5 IL=39
    // 递归收集 NavNode 及其子节点的所有数据行（跳过虚拟节点）
    // 若 navNode 有 Record，直接 AddRange 其 Rows；否则递归子节点
    private void SortNavNodeRecord_g__GetNavNodeAllDataRows(NavNode navNode, List<Auditai.Model.Row> dataRows)
    {
        if (navNode == null) return;
        if (navNode.IsVirtualNode) return;
        if (navNode.Record != null)
        {
            dataRows.AddRange(navNode.Record.Rows);
            return;
        }
        if (navNode.Children == null) return;
        foreach (var child in navNode.Children)
        {
            SortNavNodeRecord_g__GetNavNodeAllDataRows(child, dataRows);
        }
    }

    // 原始 IL: <SortNavNodeRecord>g__FirstMove|105_10 IL=42
    // 从排序列表最后一项开始，依次将每项的 FirstDataRow 移动到目标位置
    // Move(sourceIndex, count=1, destinationIndex)
    private void SortNavNodeRecord_g__FirstMove(List<NaveTreeNodeSortData> sortDataList)
    {
        int lastIndex = sortDataList.Count - 1;
        int targetIndex = sortDataList[lastIndex].FirstDataRow.Index;
        for (int i = lastIndex - 1; i >= 0; i--)
        {
            var data = sortDataList[i];
            Ticket.Table.Rows.Move(data.FirstDataRow.Index, 1, targetIndex);
            targetIndex = data.FirstDataRow.Index;
        }
    }

    // 原始 IL: <SortNavNodeRecord>g__SecondMove|105_11 IL=111
    // 将每个节点的 DataRowsList 内的行整理为连续排列
    // 遍历 DataRowsList，若当前行与前一行不连续，则批量移动
    // 最后调用 FormulaEvaluator.ClearCache()
    private void SortNavNodeRecord_g__SecondMove(List<NaveTreeNodeSortData> sortDataList)
    {
        for (int i = 0; i < sortDataList.Count; i++)
        {
            var data = sortDataList[i];
            int count = data.DataRowsList.Count;
            if (count <= 1) continue;

            var firstRow = data.DataRowsList[0];
            int sourceIndex = 1;
            int prevIndex = data.DataRowsList[1].Index;

            for (int j = 2; j < count; j++)
            {
                var currentRow = data.DataRowsList[j];
                if (currentRow.Index != prevIndex + 1)
                {
                    Ticket.Table.Rows.Move(
                        data.DataRowsList[sourceIndex].Index,
                        j - sourceIndex,
                        firstRow.Index + 1);
                    sourceIndex = j;
                    firstRow = data.DataRowsList[j - 1];
                }
                prevIndex = currentRow.Index;
            }

            int remaining = count - sourceIndex;
            if (remaining > 0)
            {
                Ticket.Table.Rows.Move(
                    data.DataRowsList[sourceIndex].Index,
                    remaining,
                    firstRow.Index + 1);
            }
        }
        FormulaEvaluator.ClearCache();
    }

    public void DoDragDropAction()
    {
        if (this._mouseDragData == null || !this._mouseDragData.IsAllowPutDown)
        {
            return;
        }
        if (this._mouseDragData.SelectRowIndex < 0 || this._mouseDragData.SelectRowIndex >= Grid.Rows.Count)
        {
            return;
        }
        if (this._mouseDragData.DragToRowIndex < 0 || this._mouseDragData.DragToRowIndex >= Grid.Rows.Count)
        {
            return;
        }
        Program.MainForm.TicketInputEditor.SuspendDrawing();
        try
        {
            Grid.BeginUpdate();
            List<TicketNavGrid.NaveTreeNodeData> sameLevelNodesList = DoDragDropAction_g__GetNavTreeNodeBrotherNodes(this._mouseDragData.SelectRowIndex);
            if (sameLevelNodesList.Count <= 1)
            {
                return;
            }
            TicketNavGrid.NaveTreeNodeData srcRowData = sameLevelNodesList.FirstOrDefault((TicketNavGrid.NaveTreeNodeData u) => u.GridNode.Row.Index == this._mouseDragData.SelectRowIndex);
            if (srcRowData == null)
            {
                return;
            }
            TicketNavGrid.NaveTreeNodeData dstRowData;
            if (this._mouseDragData.IsPutToDragRowTop)
            {
                dstRowData = sameLevelNodesList.FirstOrDefault((TicketNavGrid.NaveTreeNodeData u) => u.GridNode.Row.Index == this._mouseDragData.DragToRowIndex);
            }
            else
            {
                object key = Grid.Rows[this._mouseDragData.DragToRowIndex].Node.Key;
                Node parent = Grid.Rows[this._mouseDragData.SelectRowIndex].Node.Parent;
                if (key == ((parent != null) ? parent.Key : null))
                {
                    dstRowData = sameLevelNodesList[0];
                }
                else
                {
                    TicketNavGrid.NaveTreeNodeData rowData = sameLevelNodesList.FirstOrDefault((TicketNavGrid.NaveTreeNodeData u) => u.GridNode.Row.Index == this._mouseDragData.DragToRowIndex);
                    if (rowData == null || rowData.OrderIndex + 1 >= sameLevelNodesList.Count)
                    {
                        dstRowData = null;
                    }
                    else
                    {
                        dstRowData = sameLevelNodesList[rowData.OrderIndex + 1];
                    }
                }
            }
            if (dstRowData == null)
            {
                if (srcRowData.OrderIndex == sameLevelNodesList.Count - 1)
                {
                    return;
                }
            }
            else if (dstRowData.OrderIndex == srcRowData.OrderIndex || dstRowData.OrderIndex == srcRowData.OrderIndex + 1)
            {
                return;
            }
            List<Auditai.Model.Row> needMoveRowsList = DoDragDropAction_g__GetRecordRows(this._mouseDragData.SelectRowIndex);
            int beforeWichTableRowIndex;
            if (dstRowData == null)
            {
                beforeWichTableRowIndex = DoDragDropAction_g__GetAfterNavTreeNodeFirstTableDataRowIndex(sameLevelNodesList[sameLevelNodesList.Count - 1].GridNode.Row.Index);
            }
            else
            {
                beforeWichTableRowIndex = DoDragDropAction_g__GetNavTreeNodeFirstTableDataRowIndex(dstRowData.GridNode.Row.Index);
            }
            if (beforeWichTableRowIndex == -1)
            {
                DoDragDropAction_g__MoveTableRowsToTableEnd(needMoveRowsList);
            }
            else
            {
                DoDragDropAction_g__MoveTableRowsBeforeToTableRow(needMoveRowsList, beforeWichTableRowIndex);
            }
            int beforeWhichNavTreeRowIndex = Grid.Rows.Count;
            if (dstRowData == null)
            {
                beforeWhichNavTreeRowIndex = DoDragDropAction_g__GetNavTreeNodeSameLevelNextRowShoudUsedIndex(sameLevelNodesList[sameLevelNodesList.Count - 1].GridNode.Row.Index);
            }
            else
            {
                beforeWhichNavTreeRowIndex = dstRowData.GridNode.Row.Index;
            }
            DoDragDropAction_g__BuildSameNavTreeNodeBeforeRowIndex(srcRowData.GridNode, beforeWhichNavTreeRowIndex);
        }
        finally
        {
            Grid.EndUpdate();
            Program.MainForm.TicketInputEditor.ResumeDrawing();
            TicketNavTreeStatusDataCacher.ResumeProcessNavTreeExpandEvent(this.Ticket.Table.Id, this.NavTreeID as TicketNavTreeID, this);
            Program.MainForm.TicketInputEditor.OnNavTreeCurrentSelectedRecordIndexChanged(this);
        }
    }

    private List<NaveTreeNodeData> DoDragDropAction_g__GetNavTreeNodeBrotherNodes(int rowIndex)
    {
        var result = new List<NaveTreeNodeData>();
        var srcRow = Grid.Rows[rowIndex];
        var srcLevel = srcRow.Node.Level;
        if (srcRow.Node.Parent == null)
        {
            int count = Grid.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                var row = Grid.Rows[i];
                if (row.Node.Level != srcLevel) continue;
                var data = new NaveTreeNodeData
                {
                    NavNode = row.Node.Key as NavNode,
                    GridNode = row.Node,
                    OrderIndex = result.Count
                };
                result.Add(data);
            }
        }
        else
        {
            var parent = srcRow.Node.Parent;
            var nodes = parent.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var data = new NaveTreeNodeData
                {
                    NavNode = node.Key as NavNode,
                    GridNode = node,
                    OrderIndex = result.Count
                };
                result.Add(data);
            }
        }
        return result;
    }

    private List<C1.Win.C1FlexGrid.Row> DoDragDropAction_g__GetNavTreeNodeAndSubNodeAllRows(int rowIndex)
    {
        var result = new List<C1.Win.C1FlexGrid.Row>();
        var srcLevel = Grid.Rows[rowIndex].Node.Level;
        result.Add(Grid.Rows[rowIndex]);
        int count = Grid.Rows.Count;
        for (int i = rowIndex + 1; i < count; i++)
        {
            var row = Grid.Rows[i];
            if (row.Node.Level <= srcLevel) return result;
            result.Add(row);
        }
        return result;
    }

    private int DoDragDropAction_g__GetNavTreeNodeSameLevelNextRowShoudUsedIndex(int lastRowIndex)
    {
        var srcLevel = Grid.Rows[lastRowIndex].Node.Level;
        int count = Grid.Rows.Count;
        for (int i = lastRowIndex + 1; i < count; i++)
        {
            var row = Grid.Rows[i];
            if (row.Node.Level > srcLevel) continue;
            return i;
        }
        return count;
    }

    private int DoDragDropAction_g__GetAfterNavTreeNodeFirstTableDataRowIndex(int gridNodeRowIndex)
    {
        bool inSubNodes = true;
        int count = Grid.Rows.Count;
        var srcLevel = Grid.Rows[gridNodeRowIndex].Node.Level;
        for (int i = gridNodeRowIndex + 1; i < count; i++)
        {
            var row = Grid.Rows[i];
            if (inSubNodes)
            {
                if (row.Node.Level <= srcLevel)
                    inSubNodes = false;
            }
            if (!inSubNodes)
            {
                var navNode = row.Node.Key as NavNode;
                if (navNode != null && navNode.Record != null && navNode.Record.Rows.Count > 0)
                    return navNode.Record.Rows[0].Index;
            }
        }
        return -1;
    }

    private int DoDragDropAction_g__GetNavTreeNodeFirstTableDataRowIndex(int gridNodeRowIndex)
    {
        int count = Grid.Rows.Count;
        for (int i = gridNodeRowIndex; i < count; i++)
        {
            var row = Grid.Rows[i];
            var navNode = row.Node.Key as NavNode;
            if (navNode != null && navNode.Record != null && navNode.Record.Rows.Count > 0)
                return navNode.Record.Rows[0].Index;
        }
        return -1;
    }

    private List<Auditai.Model.Row> DoDragDropAction_g__GetRecordRows(int selectRowIndex)
    {
        var result = new List<Auditai.Model.Row>();
        var srcNode = Grid.Rows[selectRowIndex].Node;
        if (srcNode.Children == 0)
        {
            var navNode = srcNode.Key as NavNode;
            if (navNode != null && navNode.Record != null && navNode.Record.Rows.Count > 0)
                result.AddRange(navNode.Record.Rows);
            result.Sort((a, b) => a.Index.CompareTo(b.Index));
            return result;
        }
        int count = Grid.Rows.Count;
        var srcLevel = srcNode.Level;
        for (int i = selectRowIndex; i < count; i++)
        {
            var row = Grid.Rows[i];
            var navNode = row.Node.Key as NavNode;
            if (navNode != null && navNode.Record != null && navNode.Record.Rows.Count > 0)
                result.AddRange(navNode.Record.Rows);
            if (row.Node.Level <= srcLevel && i != selectRowIndex)
                break;
        }
        if (result.Count > 1)
        {
            result = result.Distinct().ToList();
            result.Sort((a, b) => a.Index.CompareTo(b.Index));
        }
        return result;
    }

    private void DoDragDropAction_g__MoveTableRowsToTableEnd(List<Auditai.Model.Row> rows)
    {
        if (rows.Count == 0) return;
        if (rows.Count == 1)
        {
            var row = rows[0];
            Ticket.Table.Rows.Move(row.Index, 1, Ticket.Table.Rows.Count);
            FormulaEvaluator.ClearCache();
            return;
        }
        int count = rows.Count;
        int sourceIndex = 0;
        int prevIndex = rows[0].Index;
        for (int i = 1; i < count; i++)
        {
            var currentRow = rows[i];
            if (currentRow.Index != prevIndex + 1)
            {
                Ticket.Table.Rows.Move(
                    rows[sourceIndex].Index,
                    i - sourceIndex + 1,
                    Ticket.Table.Rows.Count);
                sourceIndex = i;
            }
            prevIndex = currentRow.Index;
        }
        Ticket.Table.Rows.Move(
            rows[sourceIndex].Index,
            count - sourceIndex,
            Ticket.Table.Rows.Count);
        FormulaEvaluator.ClearCache();
    }

    private void DoDragDropAction_g__MoveTableRowsBeforeToTableRow(List<Auditai.Model.Row> rows, int beforeRowIndex)
    {
        if (rows.Count == 0) return;
        if (rows.Count == 1)
        {
            var row = rows[0];
            Ticket.Table.Rows.Move(row.Index, 1, beforeRowIndex);
            FormulaEvaluator.ClearCache();
            return;
        }
        var targetRow = Ticket.Table.Rows[beforeRowIndex];
        int count = rows.Count;
        int sourceIndex = 0;
        int prevIndex = rows[0].Index;
        for (int i = 1; i < count; i++)
        {
            var currentRow = rows[i];
            if (currentRow.Index != prevIndex - 1)
            {
                Ticket.Table.Rows.Move(
                    rows[sourceIndex].Index,
                    i - sourceIndex,
                    targetRow.Index);
                sourceIndex = i;
            }
            prevIndex = currentRow.Index;
        }
        Ticket.Table.Rows.Move(
            rows[sourceIndex].Index,
            count - sourceIndex,
            targetRow.Index);
        FormulaEvaluator.ClearCache();
    }

    private void DoDragDropAction_g__BuildSameNavTreeNodeBeforeRowIndex(Node gridNode, int beforeRowIndex)
    {
        _isSuspendSelectChangeEvent = true;
        _shouldSkipBodyAfterRowColChange = true;
        try
        {
            var srcRows = DoDragDropAction_g__GetNavTreeNodeAndSubNodeAllRows(gridNode.Row.Index);
            int srcRowCount = srcRows.Count;
            if (beforeRowIndex >= Grid.Rows.Count)
                Grid.Rows.Add(srcRowCount);
            else
                Grid.Rows.InsertRange(beforeRowIndex, srcRowCount);
            TicketNavTreeStatusDataCacher.SuspendProcessNavTreeExpandEvent(Ticket.Table.Id);
            C1.Win.C1FlexGrid.Row firstNewRow = null;
            for (int i = 0; i < srcRowCount; i++)
            {
                var srcRow = srcRows[i];
                var newRow = Grid.Rows[beforeRowIndex + i];
                newRow.IsNode = true;
                newRow.Node.Level = srcRow.Node.Level;
                newRow.Node.Key = srcRow.Node.Key;
                if (firstNewRow == null)
                    firstNewRow = newRow;
            }
            for (int i = 0; i < srcRowCount; i++)
            {
                var srcRow = srcRows[i];
                var newRow = Grid.Rows[beforeRowIndex + i];
                newRow.Node.Collapsed = srcRow.Node.Collapsed;
            }
            var selectedRecord = SelectedRecord;
            Grid.Rows.RemoveRange(gridNode.Row.Index, srcRowCount);
            int newSelectedIndex = -1;
            int newRowCount = Grid.Rows.Count;
            RecordList.Clear();
            for (int i = 0; i < newRowCount; i++)
            {
                var row = Grid.Rows[i];
                var navNode = row.Node.Key as NavNode;
                if (navNode != null && navNode.Record != null && navNode.Record.Rows.Count > 0)
                {
                    RecordList.Add(navNode.Record);
                    if (navNode.Record == selectedRecord)
                        newSelectedIndex = i;
                }
            }
            if (firstNewRow != null)
                newSelectedIndex = firstNewRow.Index;
            _isOnlyRunGridDefaultSelectEventProcess = true;
            Grid.Select(newSelectedIndex, 0);
            SelectedRecord = selectedRecord;
            _isOnlyRunGridDefaultSelectEventProcess = false;
        }
        finally
        {
            _shouldSkipBodyAfterRowColChange = false;
            _isSuspendSelectChangeEvent = false;
            _isOnlyRunGridDefaultSelectEventProcess = false;
        }
    }

    public void FindAndSelectRecord(params object[] args)
    {
        try
        {
            if (args == null || args.Length == 0) return;
            var target = args[0];
            for (var r = 0; r < Grid.BodyRowsCount; r++)
            {
                var row = Grid.BodyGetRow(r);
                var navNode = row.UserData as NavNode;
                if (navNode != null && navNode.Record == target)
                {
                    Grid.Select(r, 0);
                    Grid.Row = r;
                    return;
                }
            }
        }
        catch { }
    }

    public int GetCurrentIndex(params object[] args)
    {
        return Grid.Row >= Grid.Rows.Fixed ? Grid.Row - Grid.Rows.Fixed : 0;
    }

    // 原始 IL: GetTreeNodePath(int rowIndex) IL=67
    // 路径格式: "level0^level1^...^levelN"（从根到目标节点，无前缀）
    // 通过 row.Node.Parent 链向上遍历，使用 "^" 分隔符
    public string GetTreeNodePath(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= Grid.Rows.Count) return string.Empty;
        var row = Grid.Rows[rowIndex];
        var navNode = row.UserData as NavNode;
        if (navNode == null) return string.Empty;

        var node = row.Node;
        var list = new List<string>();
        while (node != null)
        {
            var n = node.Row.UserData as NavNode;
            string name;
            if (n == null) name = "(空)";
            else if (string.IsNullOrEmpty(n.Text)) name = "(空)";
            else name = n.Text;
            list.Add(name);
            node = node.Parent;
        }
        if (list.Count == 0) return string.Empty;
        list.Reverse();
        return string.Join("^", list);
    }

    // 原始 IL: GetTreeNodeOpenPath(Node node) IL=48
    // 路径格式: "TableId^NavTreeName^level0^...^levelN"
    // 通过 node.Key 获取 NavNode，向上遍历 node.Parent 链
    // 最后追加 NavTreeName 和 TableId，反转后用 "^" 连接
    public string GetTreeNodeOpenPath(Node node)
    {
        var list = new List<string>();
        while (node != null)
        {
            var navNode = node.Key as NavNode;
            if (navNode != null)
            {
                var name = string.IsNullOrEmpty(navNode.Text) ? "(空)" : navNode.Text;
                list.Add(name);
            }
            node = node.Parent;
        }
        list.Add(NavTreeName);
        list.Add(Ticket.Table.Id.Value.ToString());
        list.Reverse();
        return string.Join("^", list);
    }

    public void FindAndSelectTreeNodePath(params object[] args)
    {
        // 原始 IL: FindAndSelectTreeNodePath (IL=144)
        // 路径格式: "level0^level1^...^levelN"，从根到目标节点
        // 在 Grid 中查找叶子节点（level == parts.Length-1），再向上匹配父节点链
        if (args == null || args.Length == 0 || !(args[0] is string path)) return;
        if (string.IsNullOrEmpty(path)) return;

        var parts = path.Split(new[] { "^" }, StringSplitOptions.None);
        if (parts.Length == 0) return;

        int foundRowIndex = -1;
        int rowsCount = Grid.Rows.Count;
        int fixedRows = Grid.Rows.Fixed;

        for (int i = fixedRows; i < rowsCount; i++)
        {
            var row = Grid.Rows[i];
            var navNode = row.UserData as NavNode;
            if (navNode == null) continue;

            int level = row.Node.Level;
            if (level >= parts.Length) continue;
            if (level != parts.Length - 1) continue;

            var nodeName = GetNodeName(navNode);
            if (nodeName != parts[level]) continue;

            bool matched = true;
            var parent = row.Node.Parent;
            while (parent != null)
            {
                int parentLevel = parent.Level;
                var parentNode = parent.Row.UserData as NavNode;
                if (parentNode == null) { matched = false; break; }

                var parentName = GetNodeName(parentNode);
                if (parentName != parts[parentLevel]) { matched = false; break; }

                parent = parent.Parent;
            }

            if (matched)
            {
                foundRowIndex = i;
                break;
            }
        }

        if (foundRowIndex == -1) return;

        _shouldSkipBodyAfterRowColChange = true;
        _isOnlyRunGridDefaultSelectEventProcess = true;
        Grid.Select(foundRowIndex, 0);
        _shouldSkipBodyAfterRowColChange = false;
        _isOnlyRunGridDefaultSelectEventProcess = false;
    }

    // 原始 IL: GetRecordNavTreeNodeOpenPath(TicketRecord record) IL=41
    // 遍历 Grid.Rows 查找 navNode.Record == record 的行，调用 GetTreeNodeOpenPath(row.Node)
    public string GetRecordNavTreeNodeOpenPath(TicketRecord record)
    {
        int rowsCount = Grid.Rows.Count;
        int fixedRows = Grid.Rows.Fixed;
        for (int i = fixedRows; i < rowsCount; i++)
        {
            var row = Grid.Rows[i];
            var navNode = row.UserData as NavNode;
            if (navNode == null) continue;
            if (navNode.Record != record) continue;
            return GetTreeNodeOpenPath(row.Node);
        }
        return string.Empty;
    }

    // 原始 IL: IsNavTreeNodeOpenPathMatchedCurrentNavTree(string) IL=48
    // 路径格式: "TableId^NavTreeName^level0^...^levelN"
    // 用 "^" 分割，校验 parts[0] == TableId.ToString() 且 parts[1] == NavTreeName
    public bool IsNavTreeNodeOpenPathMatchedCurrentNavTree(string navTreeNodePath)
    {
        if (string.IsNullOrWhiteSpace(navTreeNodePath)) return false;
        var parts = navTreeNodePath.Split(new[] { "^" }, StringSplitOptions.None);
        if (parts.Length < 2) return false;
        if (parts[0] != Ticket.Table.Id.ToString()) return false;
        if (parts[1] != NavTreeName) return false;
        return true;
    }

    public void ClickToShowRow(params object[] args)
    {
        try
        {
            if (args?.Length > 0 && args[0] is int row && row >= Grid.Rows.Fixed && row < Grid.Rows.Count)
            {
                Grid.Select(row, 0);
                Grid.Row = row;
                Grid.ShowCell(row, 0);
            }
        }
        catch { }
    }

    public void TryToSelectFirstAvailableNode()
    {
        try
        {
            _shouldSkipBodyAfterRowColChange = true;
            Grid.BodySelect(-1, -1);
            _shouldSkipBodyAfterRowColChange = false;

            for (var i = 0; i < Grid.BodyRowsCount; i++)
            {
                var row = Grid.BodyGetRow(i);
                var navNode = row.UserData as NavNode;
                if (navNode != null && navNode.Children.Count == 0)
                {
                    Grid.BodySelect(i, 0);
                    return;
                }
            }
        }
        catch { }
    }

    // 原始 IL: BuildTreeNodeOpenPathAndRowMapper() IL=66
    // 遍历所有行，对有 Record 的节点调用 GetTreeNodeOpenPath(row.Node) 生成路径
    // 路径相同的行归入同一个 List<Row>
    public Dictionary<string, List<C1.Win.C1FlexGrid.Row>> BuildTreeNodeOpenPathAndRowMapper()
    {
        var sw = new Stopwatch();
        sw.Restart();
        sw.Start();

        var result = new Dictionary<string, List<C1.Win.C1FlexGrid.Row>>();
        var rowCount = Grid.Rows.Count;
        var fixedRows = Grid.Rows.Fixed;

        for (var i = fixedRows; i < rowCount; i++)
        {
            var row = Grid.Rows[i];
            var navNode = row.UserData as NavNode;
            if (navNode == null) continue;
            if (navNode.Record == null) continue;

            var path = GetTreeNodeOpenPath(row.Node);
            if (!result.TryGetValue(path, out var list))
            {
                list = new List<C1.Win.C1FlexGrid.Row> { row };
                result.Add(path, list);
            }
            else
            {
                list.Add(row);
            }
        }

        sw.Stop();
        return result;
    }

    // 原始 IL: GetRecordNodePath(TicketRecord record, string separator) IL=74
    // 遍历 Body 行查找 navNode.Record == record，找到后通过 row.Node.Parent 链向上遍历
    // 用传入的 separator 连接（注意：不是"^"，由调用方决定）
    public string GetRecordNodePath(TicketRecord record, string separator)
    {
        Node foundNode = null;
        for (var r = 0; r < Grid.BodyRowsCount; r++)
        {
            var row = Grid.BodyGetRow(r);
            var navNode = row.UserData as NavNode;
            if (navNode == null) continue;
            if (navNode.Record != record) continue;
            foundNode = row.Node;
            break;
        }

        var list = new List<string>();
        while (foundNode != null)
        {
            var n = foundNode.Row.UserData as NavNode;
            string name;
            if (n == null) name = "(空)";
            else if (string.IsNullOrEmpty(n.Text)) name = "(空)";
            else name = n.Text;
            list.Add(name);
            foundNode = foundNode.Parent;
        }
        if (list.Count == 0) return string.Empty;
        list.Reverse();
        return string.Join(separator, list);
    }

    public string GetNodeDisplayName(string text)
    {
        return string.IsNullOrEmpty(text) ? "(空)" : text;
    }

    public string GetNodeName(NavNode node)
    {
        return string.IsNullOrEmpty(node.Text) ? "(空)" : node.Text;
    }

    public List<int> FindRecordRowIndex(TicketRecord record)
    {
        var result = new List<int>();
        try
        {
            for (var r = 0; r < Grid.BodyRowsCount; r++)
            {
                var row = Grid.BodyGetRow(r);
                var navNode = row.UserData as NavNode;
                if (navNode != null && navNode.Record == record)
                {
                    result.Add(r);
                }
            }
        }
        catch { }
        return result;
    }

    public object GetTableRowByOpenPath(params object[] args)
    {
        // 原始 IL: GetTableRowByOpenPath (IL=140)
        // 路径格式: "prefix0^prefix1^level0^level1^...^levelN"
        // 注意 level 索引 = row.Node.Level + 2（前 2 段是表/项目前缀）
        // 在 Grid 中查找叶子节点，向上匹配父节点链，返回匹配的 C1FlexGrid Row
        if (args == null || args.Length == 0 || !(args[0] is string path)) return null;
        int maxSearchRows = args.Length > 1 && args[1] is int maxRows ? maxRows : -1;

        if (string.IsNullOrWhiteSpace(path)) return null;

        var parts = path.Split(new[] { "^" }, StringSplitOptions.None);
        if (parts.Length < 2) return null;

        int rowsCount = Grid.Rows.Count;
        if (maxSearchRows < 0) maxSearchRows = rowsCount;

        int fixedRows = Grid.Rows.Fixed;

        for (int i = fixedRows; i < rowsCount && i <= maxSearchRows; i++)
        {
            var row = Grid.Rows[i];
            var navNode = row.UserData as NavNode;
            if (navNode == null) continue;
            if (navNode.Record == null) continue;

            int level = row.Node.Level + 2;
            if (level >= parts.Length) continue;

            var nodeName = GetNodeName(navNode);
            if (nodeName != parts[level]) continue;

            bool matched = true;
            var parent = row.Node.Parent;
            while (parent != null)
            {
                int parentLevel = parent.Level + 2;
                var parentNode = parent.Row.UserData as NavNode;
                if (parentNode == null) { matched = false; break; }

                var parentName = GetNodeName(parentNode);
                if (parentName != parts[parentLevel]) { matched = false; break; }

                parent = parent.Parent;
            }

            if (matched) return row;
        }

        return null;
    }

    public void SaveNavTreeCollapseStatus()
    {
        var navTreeId = NavTreeID as TicketNavTreeID;
        if (navTreeId != null)
            TicketNavTreeStatusDataCacher.ProcessNavTreeCollapseEvent(Ticket.Table.Id, navTreeId, this);
    }

    public void SaveNavTreeScrollStatus()
    {
        var navTreeId = NavTreeID as TicketNavTreeID;
        if (navTreeId != null)
            TicketNavTreeStatusDataCacher.ProcessNavTreeScrollEvent(Ticket.Table.Id, navTreeId, this);
    }

    public void SelectRowWithoutTriggerSelectEvent(int row)
    {
        try
        {
            _isOnlyRunGridDefaultSelectEventProcess = true;
            Grid.Select(row, 0);
            _isOnlyRunGridDefaultSelectEventProcess = false;
        }
        catch { }
    }

    public void CollapseAll()
    {
        _isSuspendCollapseEvent = true;
        try
        {
            Grid.CollapseAll();
            Program.MainForm.TicketInputEditor.RefreshNavTreeNodeFlickImage();
        }
        finally
        {
            _isSuspendCollapseEvent = false;
            if (Ticket != null)
            {
                TicketNavTreeStatusDataCacher.ProcessNavTreeCollapseEvent(
                    Ticket.Table.Id, NavTreeID as TicketNavTreeID, this);
            }
        }
    }

    public Point GetGridRowMoreMenuIconLeftTopPosition(int row)
    {
        if (Grid.IsRowIndexOutOfRange(row)) return new Point(-100, -100);
        var cellRect = Grid.GetCellRect(row, 0);
        int rightPadding = 25;
        int x = cellRect.X + cellRect.Width - rightPadding;
        int y = cellRect.Y + (cellRect.Height - Resources.menuMoreOperation.Height) / 2;
        return new Point(x, y);
    }

    public Rectangle GetGridRowMoreMenuIconBackgroundRectangle(int row)
    {
        var pos = GetGridRowMoreMenuIconLeftTopPosition(row);
        var marginX = 3;
        var marginY = 3;
        var x = pos.X - marginX;
        var y = pos.Y - marginY;
        var w = Resources.menuMoreOperation.Width + marginX * 2;
        var h = Resources.menuMoreOperation.Height + marginY * 2;
        return new Rectangle(x, y, w, h);
    }

    private HashSet<Auditai.Model.Column> GetContainsComboListColumn(List<Auditai.Model.Column> columns)
    {
        if (columns == null || columns.Count == 0)
            return null;

        var result = new HashSet<Auditai.Model.Column>();
        foreach (var column in columns)
        {
            if (column.GetFormat().HasComboList)
            {
                result.Add(column);
            }
        }
        return result.Count > 0 ? result : null;
    }

    private List<Auditai.Model.Column> GetReferredSameTableOtherColumns(Auditai.Model.Column column)
    {
        // 原始 IL 使用 BFS（Queue + HashSet）递归查找所有引用的同表其他列
        var format = column.GetFormat();
        if (!format.HasComboList)
        {
            return null;
        }

        var queue = new Queue<Auditai.Model.Column>();
        var visited = new HashSet<Auditai.Model.Column>();
        queue.Enqueue(column);
        var result = new List<Auditai.Model.Column>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (visited.Contains(current))
            {
                continue;
            }
            visited.Add(current);

            // 不把输入列本身加入结果
            if (current != column)
            {
                result.Add(current);
            }

            var currentFormat = current.GetFormat();
            if (!currentFormat.HasComboList)
            {
                continue;
            }

            var evaluator = new FormulaEvaluator(currentFormat.ComboList);
            var referredColumns = evaluator.GetReferredTableColumns(column.Table);
            if (referredColumns == null)
            {
                continue;
            }

            foreach (var referredColumn in referredColumns)
            {
                if (!visited.Contains(referredColumn))
                {
                    queue.Enqueue(referredColumn);
                }
            }
        }

        return result;
    }

    private List<Tuple<string, string>> GetComboListValue(Auditai.Model.Column column)
    {
        var comboList = column.GetFormat().ComboList;
        var project = column.Table?.Project;
        if (string.IsNullOrWhiteSpace(comboList) || project == null)
        {
            Debug.WriteLine($"[TicketNavGrid] GetComboListValue: column='{column.Caption}', ComboList='{comboList}', Project is {(project == null ? "null" : "not null")}");
            return null;
        }

        Debug.WriteLine($"[TicketNavGrid] GetComboListValue: column='{column.Caption}', ComboList='{column.GetFormat().ComboList}'");
        try
        {
            var resolver = new FormulaReferenceModelResolver(Table.Project);
            var env = new FormulaEvaluationEnvironment
            {
                Resolver = resolver,
                RowIndex = Grid.BodyRow,
                HostTable = Table,
                RefManager = Table.Project.DataReferenceManager,
                RefEvalContext = new DataReferenceEvaluationContext
                {
                    Project = Table.Project,
                    CurrentTreeNode = Table.TreeNode
                }
            };
            var evaluator = new FormulaEvaluator(column.GetFormat().ComboList)
            {
                Env = env
            };
            var op = evaluator.EvaluateToOperand();
            Debug.WriteLine($"[TicketNavGrid] GetComboListValue: column='{column.Caption}', operand type={op?.GetType().Name ?? "null"}");
            var result = ConvertOperandToNodeDisplayValueList(op, isNavCellCountMoreThanOne: false, out _);
            Debug.WriteLine($"[TicketNavGrid] GetComboListValue: column='{column.Caption}', result count={result?.Count ?? 0}");
            return result;
        }
        catch (FormulaException ex)
        {
            Debug.WriteLine($"[TicketNavGrid] GetComboListValue: column='{column.Caption}', FormulaException: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TicketNavGrid] GetComboListValue: column='{column.Caption}', Exception: {ex.Message}");
            return null;
        }
    }

    private FormulaEvaluator GenerateColComboListEvaluator(Auditai.Model.Column column)
    {
        // 原始 IL: GenerateColComboListEvaluator (IL=54)
        // 使用 column.Table（而非 TicketNavGrid.Table 字段）确保正确性
        var format = column.GetFormat();
        if (string.IsNullOrWhiteSpace(format.ComboList))
            return null;

        var resolver = new FormulaReferenceModelResolver(column.Table.Project);
        var env = new FormulaEvaluationEnvironment
        {
            Resolver = resolver,
            RowIndex = 0,
            HostTable = column.Table,
            RefManager = column.Table.Project.DataReferenceManager,
            RefEvalContext = new DataReferenceEvaluationContext
            {
                Project = column.Table.Project,
                CurrentTreeNode = column.Table.TreeNode
            }
        };
        return new FormulaEvaluator(format.ComboList)
        {
            Env = env
        };
    }

    private List<Tuple<string, string>> GetComboListValueByVirtualTable(FormulaEvaluator evaluator, VirtualTable virtualTable, Auditai.Model.Column column)
    {
        var evalContext = GetEvalContext();
        try
        {
            var op = evaluator.EvaluateOnVirtualTable(evalContext, Ticket.Table.Id);
            if (op is MultiListOperand)
                return null;
            if (op is TreeListOperand treeListOperand)
            {
                var list = new List<Tuple<string, string>>();
                foreach (TreeListNode root in treeListOperand.Roots)
                {
                    AddLeafNode(root, list);
                }
                return list;
            }
            if (op is TableListOperand tableListOperand)
            {
                var rowCount = tableListOperand.DataTable.Rows.Count;
                var colCount = tableListOperand.DataTable.Columns.Count;
                if (colCount == 0)
                    return null;
                var list = new List<Tuple<string, string>>();
                for (var i = 0; i < rowCount; i++)
                {
                    var value = tableListOperand.DataTable.Rows[i][0] as string;
                    if (value != null)
                    {
                        list.Add(Tuple.Create(value, value));
                    }
                }
                return list;
            }
            ValueSetOperand valueSet = null;
            if (op is InputListOperand inputListOperand)
            {
                valueSet = inputListOperand;
            }
            else
            {
                valueSet = op.ToValueSetOrderByRowIndex();
            }
            if (valueSet == null)
                return null;
            return valueSet.Set.Select(t => Tuple.Create(t.Item2.ToString(), t.Item2.ToString())).ToList();
        }
        catch (Exception ex)
        {
            ex.Log("运算下拉框公式时发生了未预期的异常");
            return null;
        }

        VirtualTableEvalContext GetEvalContext()
        {
            var context = new VirtualTableEvalContext
            {
                ResolveTableCell = _ => null,
                ResolveColumn = id =>
                {
                    var col = Ticket.Table.Columns.GetById(id);
                    if (col == null) return null;
                    var cells = new List<Auditai.Model.Cell> { virtualTable.Cells.Get(0, col.Index) };
                    return new VirtualTableColumnOperand(cells, virtualTable);
                },
                ResolveColumnWildcard = (id, rowIndex) =>
                {
                    var col = Ticket.Table.Columns.GetById(id);
                    if (col == null) return null;
                    return virtualTable.Cells.Get(rowIndex, col.Index);
                }
            };
            return context;
        }

        void AddLeafNode(TreeListNode node, List<Tuple<string, string>> list)
        {
            if (node.Children == null || node.Children.Count == 0)
            {
                list.Add(Tuple.Create(node.Text, node.Text));
                return;
            }
            foreach (var child in node.Children)
            {
                AddLeafNode(child, list);
            }
        }
    }

    private List<Tuple<string, string>> ConvertOperandToNodeDisplayValueList(Operand op, bool isNavCellCountMoreThanOne, out TreeListOperand treeListData)
    {
        treeListData = null;
        Debug.WriteLine($"[TicketNavGrid] ConvertOperandToNodeDisplayValueList: op type={op?.GetType().Name ?? "null"}, isNavCellCountMoreThanOne={isNavCellCountMoreThanOne}");
        
        if (op is MultiListOperand)
        {
            Debug.WriteLine("[TicketNavGrid] ConvertOperandToNodeDisplayValueList: op is MultiListOperand, return null");
            return null;
        }
        if (op is TreeListOperand treeListOperand)
        {
            var list = new List<Tuple<string, string>>();
            var upLevelTextQueue = new Queue<string>();
            int rootCount = treeListOperand.Roots?.Count ?? 0;
            Debug.WriteLine($"[TicketNavGrid] ConvertOperandToNodeDisplayValueList: op is TreeListOperand, roots.Count={rootCount}");
            foreach (TreeListNode root in treeListOperand.Roots)
            {
                AddLeafNode(root, list, upLevelTextQueue);
            }
            treeListData = treeListOperand;
            Debug.WriteLine($"[TicketNavGrid] ConvertOperandToNodeDisplayValueList: TreeListOperand result count={list.Count}");
            return list;
        }
        if (op is TableListOperand tableListOperand)
        {
            int count = tableListOperand.DataTable.Rows.Count;
            if (tableListOperand.DataTable.Columns.Count == 0)
            {
                Debug.WriteLine("[TicketNavGrid] ConvertOperandToNodeDisplayValueList: TableListOperand has 0 columns, return null");
                return null;
            }
            int colCount = tableListOperand.DataTable.Columns.Count;
            var list = new List<Tuple<string, string>>();
            for (int i = 0; i < count; i++)
            {
                var stringBuilder = new StringBuilder();
                for (int j = 0; j < colCount; j++)
                {
                    string value = tableListOperand.DataTable.Rows[i][j] as string;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append(" ");
                        }
                        stringBuilder.Append(value);
                    }
                }
                string item = tableListOperand.DataTable.Rows[i][0] as string;
                string item2;
                if (isNavCellCountMoreThanOne)
                {
                    item2 = stringBuilder.ToString();
                }
                else
                {
                    item2 = item;
                }
                list.Add(Tuple.Create(item2, item));
            }
            Debug.WriteLine($"[TicketNavGrid] ConvertOperandToNodeDisplayValueList: TableListOperand result count={list.Count}");
            return list;
        }
        if (op is InputListOperand inputListOperand)
        {
            var result = inputListOperand.Set.Select(tup => Tuple.Create(tup.Item2.ToString(), tup.Item2.ToString())).ToList();
            Debug.WriteLine($"[TicketNavGrid] ConvertOperandToNodeDisplayValueList: InputListOperand result count={result.Count}");
            return result;
        }
        var valueSetResult = op?.ToValueSetOrderByRowIndex()?.Set.Select(tup => Tuple.Create(tup.Item2.ToString(), tup.Item2.ToString())).ToList();
        Debug.WriteLine($"[TicketNavGrid] ConvertOperandToNodeDisplayValueList: ValueSet result count={valueSetResult?.Count ?? 0}");
        return valueSetResult;

        static void AddLeafNode(TreeListNode node, List<Tuple<string, string>> outList, Queue<string> upLevelTextQueue)
        {
            if (node.Children == null || node.Children.Count == 0)
            {
                string fullName = GetFullName(node.Text, upLevelTextQueue);
                outList.Add(Tuple.Create(fullName, node.Text));
            }
            else
            {
                upLevelTextQueue.Enqueue(node.Text);
                foreach (TreeListNode child in node.Children)
                {
                    AddLeafNode(child, outList, upLevelTextQueue);
                }
                upLevelTextQueue.Dequeue();
            }
        }

        static string GetFullName(string currentLevelText, Queue<string> upLevelTextQueue)
        {
            if (upLevelTextQueue.Count == 0)
            {
                return currentLevelText;
            }
            var stringBuilder = new StringBuilder();
            foreach (string item in upLevelTextQueue)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(" ");
                    }
                    stringBuilder.Append(item);
                }
            }
            if (!string.IsNullOrWhiteSpace(currentLevelText))
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(" ");
                }
                stringBuilder.Append(currentLevelText);
            }
            return stringBuilder.ToString();
        }
    }

    public TicketTable GetTicket()
    {
        return Ticket;
    }

    public void SetTicket(TicketTable ticket)
    {
        Ticket = ticket;
    }

    public void SetNav(List<Auditai.Model.Column> nav)
    {
        Nav = nav;
    }

    public void SetNavSetting(TicketNav navSetting)
    {
        NavSetting = navSetting;
    }

    public void SetIsHasFillingFormula(bool value)
    {
        IsHasFillingFormula = value;
    }

    public void SetIsAllowModifyTableRowOrder(bool value)
    {
        IsAllowModifyTableRowOrder = value;
    }

    public void SetSelectedRecord(TicketRecord record)
    {
        SelectedRecord = record;
    }

    public void SetSelectedVirtualNodeRowIndex(int index)
    {
        SelectedVirtualNodeRowIndex = index;
    }

    public void SetShowSortingOrderContextMenu(bool value)
    {
        ShowSortingOrderContextMenu = value;
    }

    public void SetNavTreeID(object navTreeID)
    {
        NavTreeID = navTreeID;
    }

    public void SetTable(Auditai.Model.Table table)
    {
        Table = table;
    }
}
