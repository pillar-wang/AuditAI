using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class RowHeightCommand : CommandBase
{
	private Table _table;

	private List<Tuple<Row, int, int>> _oldNewHeights;

	public RowHeightCommand(Table table, IEnumerable<Row> rows, int newHeight)
	{
		_table = table;
		_oldNewHeights = rows.Select(r => Tuple.Create(r, r.Height, newHeight)).ToList();
	}

	public override void Execute()
	{
		foreach (var item in _oldNewHeights)
		{
			if (item.Item1.IsExisting)
			{
				item.Item1.UpdateHeight(item.Item3);
			}
		}
	}

	public override void Undo()
	{
		foreach (var item in _oldNewHeights)
		{
			if (item.Item1.IsExisting)
			{
				item.Item1.UpdateHeight(item.Item2);
			}
		}
	}
}
