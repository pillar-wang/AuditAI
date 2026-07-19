using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class ColumnWidthCommand : CommandBase
{
	private Table _table;

	private List<Tuple<Column, int, int>> _oldNewWidths;

	public ColumnWidthCommand(Table table, IEnumerable<Column> columns, int newWidth)
	{
		_table = table;
		_oldNewWidths = columns.Select(c => Tuple.Create(c, c.Width, newWidth)).ToList();
	}

	public override void Execute()
	{
		foreach (var item in _oldNewWidths)
		{
			if (item.Item1.Status != SyncStatus.LocalDeleted)
			{
				item.Item1.UpdateWidth(item.Item3);
			}
		}
	}

	public override void Undo()
	{
		foreach (var item in _oldNewWidths)
		{
			if (item.Item1.Status != SyncStatus.LocalDeleted)
			{
				item.Item1.UpdateWidth(item.Item2);
			}
		}
	}
}
