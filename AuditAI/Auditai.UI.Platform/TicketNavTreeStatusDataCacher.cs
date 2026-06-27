using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using C1.Win.C1FlexGrid;

namespace Auditai.UI.Platform;

public static class TicketNavTreeStatusDataCacher
{
    private static readonly Dictionary<TicketNavTreeProjectID, NavTreePageStatusData> _tickets_data_map = new Dictionary<TicketNavTreeProjectID, NavTreePageStatusData>();

    public class TreeNodeCollapseStatusData
    {
        public string NodeKey;
        public bool IsCollapsed;
    }

    public class NavTreeCollapseStatusData
    {
        public TicketNavTreeID NavTreeId;
        public CollapseStatusChangeEventListener TreeEventListener;
        public List<TreeNodeCollapseStatusData> CollapsedNodeList = new List<TreeNodeCollapseStatusData>();

        public NavTreeCollapseStatusData(TicketNavTreeID navTreeId, NavTreePageStatusData pageStatusData)
        {
            NavTreeId = navTreeId;
            TreeEventListener = new CollapseStatusChangeEventListener(pageStatusData);
        }

        public void SaveNavTressCollapseStatus(TicketNavGrid navTreeGrid)
        {
            TreeEventListener?.RestoreTargetCollapseStatus(navTreeGrid);
            CollapsedNodeList.Clear();
            C1FlexGridEx grid = (C1FlexGridEx)navTreeGrid.View;
            for (int i = 0; i < grid.BodyRowsCount; i++)
            {
                C1.Win.C1FlexGrid.Row row = grid.BodyGetRow(i);
                if (row != null && row.IsNode && !row.Node.Expanded && row.Node.Key is string nodeKey && nodeKey != "->")
                {
                    CollapsedNodeList.Add(new TreeNodeCollapseStatusData { NodeKey = nodeKey, IsCollapsed = true });
                }
            }
            TreeEventListener?.BeginListenTargetCollapseEvent(navTreeGrid);
        }
    }

    public class NavTreeScrollStatusData
    {
        public TicketNavTreeID NavTreeId;
        public NavTreeScrollPosition ScrollPosition = new NavTreeScrollPosition();

        public NavTreeScrollStatusData(TicketNavTreeID navTreeId, NavTreePageStatusData pageStatusData)
        {
            NavTreeId = navTreeId;
        }

        public void SaveScrollPosition(TicketNavGrid navTreeGrid)
        {
            ScrollPosition.Position = new Point(navTreeGrid.View.HorizontalOffset, navTreeGrid.View.VerticalOffset);
        }
    }

    public class NavTreeFilterStatusData
    {
        public TicketNavTreeID NavTreeId;
        public Dictionary<string, string> RecordFilterSetting = new Dictionary<string, string>();

        public NavTreeFilterStatusData(TicketNavTreeID navTreeId)
        {
            NavTreeId = navTreeId;
        }
    }

    public class NavTreePageStatusData
    {
        public int SelectedPageIndex = -1;
        public int SelectedRecordIndex = -1;
        public bool IsSelectRecordByRowId;
        public long SelectedReocrdRowId;
        public bool IsSuspendScrollEvent;
        public bool IsSuspendExpandEvent;
        public TicketRecord SelectedTicketData;
        public List<NavTreeCollapseStatusData> PageCollapseDataList = new List<NavTreeCollapseStatusData>();
        public List<NavTreeScrollStatusData> PageScrollDataList = new List<NavTreeScrollStatusData>();
        public List<NavTreeFilterStatusData> RecordFilterDataList = new List<NavTreeFilterStatusData>();
    }

    public class CollapseStatusChangeEventListener
    {
        private NavTreePageStatusData _pageStatusData;

        public CollapseStatusChangeEventListener(NavTreePageStatusData pageStatusData)
        {
            _pageStatusData = pageStatusData;
        }

        public void RestoreTargetCollapseStatus(TicketNavGrid targetGrid)
        {
            if (targetGrid == null)
                return;

            targetGrid.View.BeginUpdate();
            try
            {
                C1FlexGridEx grid = (C1FlexGridEx)targetGrid.View;
                for (int i = 0; i < grid.BodyRowsCount; i++)
                {
                    C1.Win.C1FlexGrid.Row row = grid.BodyGetRow(i);
                    if (row != null && row.IsNode && row.Node.Key is string nodeKey)
                    {
                        var existing = _pageStatusData.PageCollapseDataList
                            .SelectMany(d => d.CollapsedNodeList)
                            .FirstOrDefault(c => c.NodeKey == nodeKey && c.IsCollapsed);

                        row.Node.Expanded = existing == null;
                    }
                }
            }
            finally
            {
                targetGrid.View.EndUpdate();
            }
        }

        public void BeginListenTargetCollapseEvent(TicketNavGrid targetGrid)
        {
            // The original code attaches to the grid's collapse event.
            // Since C1FlexGrid doesn't have a direct collapse event,
            // this is handled through the AfterRowColChange or similar events.
            // We track state through SaveNavTressCollapseStatus instead.
        }
    }

    public static string GenerateTicketNavTreeID(Id64 tableId, TicketNavGrid navGrid)
    {
        string text = "->";
        if (navGrid != null)
        {
            text = string.Join("->", from c in navGrid.Nav select c.Id.ToString());
        }
        return Program.MainForm.CurrentProject.Id.ToString() + "_" + tableId.ToString() + "_" + text;
    }

    public static TicketNavTreeID GenerateTicketNavTreeID(int pageIndex, List<Auditai.Model.Column> nav)
    {
        TicketNavTreeID ticketNavTreeID = new TicketNavTreeID();
        ticketNavTreeID.PageIndex = pageIndex;
        ticketNavTreeID.ColumnsKey = string.Join("->", from c in nav select c.Id.ToString());
        return ticketNavTreeID;
    }

    public static TicketNavTreeID GenerateTicketNavTreeID(int pageIndex, TicketNavTreeID srcNavTreeId)
    {
        TicketNavTreeID ticketNavTreeID = new TicketNavTreeID();
        ticketNavTreeID.PageIndex = pageIndex;
        ticketNavTreeID.ColumnsKey = srcNavTreeId.ColumnsKey;
        return ticketNavTreeID;
    }

    public static void ProcessNavTreeCollapseEvent(Id64 ticketId, TicketNavTreeID navTreeId, TicketNavGrid navTreeGrid)
    {
        if (navTreeId == null)
            return;

        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
        {
            value = new NavTreePageStatusData();
            _tickets_data_map.Add(key, value);
        }

        NavTreeCollapseStatusData collapseData = null;
        for (int num = value.PageCollapseDataList.Count - 1; num >= 0; num--)
        {
            if (value.PageCollapseDataList[num].NavTreeId == navTreeId)
            {
                collapseData = value.PageCollapseDataList[num];
                break;
            }
        }
        if (collapseData == null)
        {
            collapseData = new NavTreeCollapseStatusData(navTreeId, value);
            value.PageCollapseDataList.Add(collapseData);
        }
        collapseData.SaveNavTressCollapseStatus(navTreeGrid);
    }

    public static void RestoreNavTreeCollapseStatus(Id64 ticketId, TicketNavTreeID navTreeId, TicketNavGrid restoreTarget)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
        {
            if (restoreTarget.View.BodyRowsCount > 20)
                restoreTarget.CollapseAll();
            return;
        }

        var list = value.PageCollapseDataList.Where(u => u.NavTreeId == navTreeId).ToList();
        if (list.Count == 0)
        {
            if (restoreTarget.View.BodyRowsCount > 20)
                restoreTarget.CollapseAll();
            return;
        }

        value.IsSuspendExpandEvent = true;
        try
        {
            list[0].TreeEventListener.RestoreTargetCollapseStatus(restoreTarget);
        }
        finally
        {
            value.IsSuspendExpandEvent = false;
        }
    }

    public static void ProcessNavTreeScrollEvent(Id64 ticketId, TicketNavTreeID navTreeId, TicketNavGrid navTreeGrid)
    {
        if (navTreeId == null)
            return;

        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
        {
            value = new NavTreePageStatusData();
            _tickets_data_map.Add(key, value);
        }
        if (value.IsSuspendScrollEvent)
            return;

        NavTreeScrollStatusData scrollData = null;
        for (int num = value.PageScrollDataList.Count - 1; num >= 0; num--)
        {
            if (value.PageScrollDataList[num].NavTreeId == navTreeId)
            {
                scrollData = value.PageScrollDataList[num];
                break;
            }
        }
        if (scrollData == null)
        {
            scrollData = new NavTreeScrollStatusData(navTreeId, value);
            value.PageScrollDataList.Add(scrollData);
        }
        scrollData.SaveScrollPosition(navTreeGrid);
    }

    public static NavTreeScrollPosition GetNavTreeLastScrollPosition(Id64 ticketId, TicketNavTreeID navTreeId)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
            return null;

        var list = value.PageScrollDataList.Where(u => u.NavTreeId == navTreeId).ToList();
        if (list.Count == 0)
            return null;

        return list[0].ScrollPosition;
    }

    public static void SuspendProcessNavTreeScrollEvent(Id64 ticketId)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (_tickets_data_map.TryGetValue(key, out var value))
            value.IsSuspendScrollEvent = true;
    }

    public static void ResumeProcessNavTreeScrollEvent(Id64 ticketId)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (_tickets_data_map.TryGetValue(key, out var value))
            value.IsSuspendScrollEvent = false;
    }

    public static void SuspendProcessNavTreeExpandEvent(Id64 ticketId)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (_tickets_data_map.TryGetValue(key, out var value))
            value.IsSuspendExpandEvent = true;
    }

    public static void ResumeProcessNavTreeExpandEvent(Id64 ticketId, TicketNavTreeID navTreeId, TicketNavGrid navTreeGrid)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (_tickets_data_map.TryGetValue(key, out var value))
        {
            value.IsSuspendExpandEvent = false;
            ProcessNavTreeCollapseEvent(ticketId, navTreeId, navTreeGrid);
        }
    }

    public static void SaveNavTreePageSelectedIndex(Id64 ticketId, int selectedIndex)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (_tickets_data_map.TryGetValue(key, out var value))
            value.SelectedPageIndex = selectedIndex;
    }

    public static void SaveNavTreeSelectedRecordIndex(Id64 ticketId, int recordIndex, bool isSelectByRowId = false, long rowId = 0L)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (_tickets_data_map.TryGetValue(key, out var value))
        {
            value.SelectedRecordIndex = recordIndex;
            value.IsSelectRecordByRowId = isSelectByRowId;
            value.SelectedReocrdRowId = rowId;
        }
    }

    public static void SaveNavTreeSelectedRecordData(Id64 ticketId, TicketRecord ticketRecord)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
        {
            value = new NavTreePageStatusData();
            _tickets_data_map.Add(key, value);
        }
        value.SelectedTicketData = ticketRecord;
    }

    public static TicketRecord GetNavTreeLastSelectedRecordData(Id64 ticketId)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
            return null;
        return value.SelectedTicketData;
    }

    public static int GetLastSelectedNavTreePageIndex(Id64 ticketId, int maxValue)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
            return -1;

        value.SelectedPageIndex = Math.Min(value.SelectedPageIndex, maxValue);
        return value.SelectedPageIndex;
    }

    public static int GetLastSelectedNavTreeRecordIndex(Id64 ticketId, out bool isSelectByRowId, out long rowId)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
        {
            isSelectByRowId = false;
            rowId = 0L;
            return -1;
        }
        isSelectByRowId = value.IsSelectRecordByRowId;
        rowId = value.SelectedReocrdRowId;
        return value.SelectedRecordIndex;
    }

    public static void ResetNavTreeIDWithRemovePageIndex(Id64 ticketId, int willRemovePageIndex)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
            return;

        for (int num = value.PageCollapseDataList.Count - 1; num >= 0; num--)
        {
            if (value.PageCollapseDataList[num].NavTreeId.PageIndex == willRemovePageIndex)
                value.PageCollapseDataList.RemoveAt(num);
        }
        for (int num2 = value.PageCollapseDataList.Count - 1; num2 >= 0; num2--)
        {
            value.PageCollapseDataList[num2].NavTreeId.PageIndex--;
        }

        for (int num3 = value.PageScrollDataList.Count - 1; num3 >= 0; num3--)
        {
            if (value.PageScrollDataList[num3].NavTreeId.PageIndex == willRemovePageIndex)
                value.PageScrollDataList.RemoveAt(num3);
        }
        for (int num4 = value.PageScrollDataList.Count - 1; num4 >= 0; num4--)
        {
            value.PageScrollDataList[num4].NavTreeId.PageIndex--;
        }

        for (int num5 = value.RecordFilterDataList.Count - 1; num5 >= 0; num5--)
        {
            if (value.RecordFilterDataList[num5].NavTreeId.PageIndex == willRemovePageIndex)
                value.RecordFilterDataList.RemoveAt(num5);
        }
        for (int num6 = value.RecordFilterDataList.Count - 1; num6 >= 0; num6--)
        {
            value.RecordFilterDataList[num6].NavTreeId.PageIndex--;
        }
    }

    public static void RemoveNavTreeByPageIndex(Id64 ticketId, int pageIndex)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
            return;

        for (int num = value.PageCollapseDataList.Count - 1; num >= 0; num--)
        {
            if (value.PageCollapseDataList[num].NavTreeId.PageIndex == pageIndex)
                value.PageCollapseDataList.RemoveAt(num);
        }
        for (int num2 = value.PageScrollDataList.Count - 1; num2 >= 0; num2--)
        {
            if (value.PageScrollDataList[num2].NavTreeId.PageIndex == pageIndex)
                value.PageScrollDataList.RemoveAt(num2);
        }
        for (int num3 = value.RecordFilterDataList.Count - 1; num3 >= 0; num3--)
        {
            if (value.RecordFilterDataList[num3].NavTreeId.PageIndex == pageIndex)
                value.RecordFilterDataList.RemoveAt(num3);
        }
    }

    public static void RemoveTicket(Id64 ticketId)
    {
        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        _tickets_data_map.Remove(key);
    }

    public static void RemoveTicket(Auditai.Model.Project project)
    {
        var list = _tickets_data_map.Keys.ToList();
        foreach (var item in list)
        {
            if (item.ProjectId == project.Id)
                _tickets_data_map.Remove(item);
        }
    }

    public static void RemoveProjectHierarchyTreeNode(TreeNodeBase node)
    {
        if (node is TreeTableNode treeTableNode)
            RemoveTicket(treeTableNode.Table.Id);
        else if (node is TreeDirectoryNode treeDirectoryNode)
        {
            foreach (var item in treeDirectoryNode.GetDescendants().OfType<TreeTableNode>())
                RemoveTicket(item.Table.Id);
        }
    }

    public static void ExpandTicketRecordParentNode(TicketNavGrid target, TicketRecord record)
    {
        if (record == null)
            return;

        var list = target.FindRecordRowIndex(record);
        if (list.Count == 0)
            return;

        target.View.BeginUpdate();
        try
        {
            foreach (int item in list)
            {
                C1.Win.C1FlexGrid.Row row = target.View.BodyGetRow(item);
                if (row.IsNode)
                {
                    for (Node parent = row.Node.Parent; parent != null; parent = parent.Parent)
                        parent.Expanded = true;
                }
            }
        }
        finally
        {
            target.View.EndUpdate();
        }
    }

    public static void SaveNavTreeRecordFilterSetting(Id64 ticketId, TicketNavGrid navTree, TicketRecord ticketRecord, string filterSetting)
    {
        string text = "->";
        TicketNavTreeID navTreeId = new TicketNavTreeID { PageIndex = -1, ColumnsKey = "->" };

        if (navTree != null && ticketRecord != null)
        {
            text = navTree.GetRecordNodePath(ticketRecord, "->");
            navTreeId = (TicketNavTreeID)navTree.NavTreeID;
        }

        if (!string.IsNullOrEmpty(text))
        {
            TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
            if (!_tickets_data_map.TryGetValue(key, out var value))
            {
                value = new NavTreePageStatusData();
                _tickets_data_map.Add(key, value);
            }

            var list = value.RecordFilterDataList.Where(u => u.NavTreeId == navTreeId).ToList();
            NavTreeFilterStatusData filterData;
            if (list.Count == 0)
            {
                filterData = new NavTreeFilterStatusData(navTreeId);
                value.RecordFilterDataList.Add(filterData);
            }
            else
            {
                filterData = list[0];
            }

            filterData.RecordFilterSetting.Remove(text);
            if (!string.IsNullOrWhiteSpace(filterSetting))
                filterData.RecordFilterSetting.Add(text, filterSetting);
        }
    }

    public static string GetNavTreeRecordFilterSetting(Id64 ticketId, TicketNavGrid navTree, TicketRecord ticketRecord)
    {
        string text = "->";
        TicketNavTreeID navTreeId = new TicketNavTreeID { PageIndex = -1, ColumnsKey = "->" };

        if (navTree != null && ticketRecord != null)
        {
            text = navTree.GetRecordNodePath(ticketRecord, "->");
            navTreeId = (TicketNavTreeID)navTree.NavTreeID;
        }

        if (string.IsNullOrEmpty(text))
            return null;

        TicketNavTreeProjectID key = new TicketNavTreeProjectID(Program.MainForm.CurrentProject.Id, ticketId);
        if (!_tickets_data_map.TryGetValue(key, out var value))
            return null;

        var list = value.RecordFilterDataList.Where(u => u.NavTreeId == navTreeId).ToList();
        if (list.Count == 0)
            return null;

        if (list[0].RecordFilterSetting.TryGetValue(text, out var result))
            return result;
        return null;
    }
}