using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using C1.Win.C1FlexGrid;
using Auditai.DTO;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class TableTicketSelectionRangeAndScrollStatusCacher
{
	public class TicketGridStatusCacheData
	{
		public C1.Win.C1FlexGrid.CellRange Selection;

		public Point ScrollPosition;
	}

	private static Dictionary<Id64, TicketGridStatusCacheData> _ticketStatusDataDic = new Dictionary<Id64, TicketGridStatusCacheData>();

	public static void SaveTicketCacheData(Id64 tableId, TicketGridStatusCacheData cacheData)
	{
		_ticketStatusDataDic[tableId] = cacheData;
	}

	public static TicketGridStatusCacheData GetTicketCacheData(Id64 tableId)
	{
		if (!_ticketStatusDataDic.TryGetValue(tableId, out var value))
		{
			return null;
		}
		return value;
	}

	public static void RemoveTable(Auditai.Model.Project project)
	{
		List<Id64> list = _ticketStatusDataDic.Keys.ToList();
		foreach (Id64 item in list)
		{
			if (project.GetNodeById(item) != null)
			{
				_ticketStatusDataDic.Remove(item);
			}
		}
	}

	public static void RemoveTable(Id64 tableId)
	{
		_ticketStatusDataDic.Remove(tableId);
	}

	public static void RemoveProjectHierarchyTreeNode(TreeNodeBase node)
	{
		if (node is TreeTableNode treeTableNode)
		{
			RemoveTable(treeTableNode.Table.Id);
		}
		else
		{
			if (!(node is TreeDirectoryNode treeDirectoryNode))
			{
				return;
			}
			foreach (TreeTableNode item in treeDirectoryNode.GetDescendants().OfType<TreeTableNode>())
			{
				RemoveTable(item.Table.Id);
			}
		}
	}
}
