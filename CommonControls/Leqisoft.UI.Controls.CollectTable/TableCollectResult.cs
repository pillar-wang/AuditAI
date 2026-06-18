using System.Collections.Generic;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls.CollectTable;

public class TableCollectResult
{
	public TableCollectorAbstract TableCollector { get; set; }

	public Dictionary<Column, List<object>> Values { get; set; }

	internal Dictionary<long, List<object>> ValuesOnColumn { get; set; }

	public TableCollectResult(TableCollectorAbstract tableCollector)
	{
		TableCollector = tableCollector;
		Values = new Dictionary<Column, List<object>>();
	}
}
