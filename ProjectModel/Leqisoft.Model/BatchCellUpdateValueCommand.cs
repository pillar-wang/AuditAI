using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class BatchCellUpdateValueCommand : CommandBase
{
	private Table _table;

	private List<Tuple<Cell, object, object, bool>> _OldNewValues;

	public bool IsExistManualInputValue { get; set; }

	public BatchCellUpdateValueCommand(Table table, IEnumerable<Tuple<Cell, object>> newValues)
	{
		_table = table;
		_OldNewValues = newValues.Select((Tuple<Cell, object> tup) => Tuple.Create(tup.Item1, tup.Item1.Value, tup.Item2, tup.Item1.IsExistManualInputValue)).ToList();
	}

	public override void Execute()
	{
		_table.BeginBatchUpdateValue();
		foreach (Tuple<Cell, object, object, bool> oldNewValue in _OldNewValues)
		{
			if (oldNewValue.Item1.IsExisting)
			{
				oldNewValue.Item1.UpdateValue(oldNewValue.Item3);
				oldNewValue.Item1.IsExistManualInputValue = IsExistManualInputValue;
			}
		}
		_table.EndBatchUpdateValue();
	}

	public override void Undo()
	{
		_table.BeginBatchUpdateValue();
		foreach (Tuple<Cell, object, object, bool> oldNewValue in _OldNewValues)
		{
			if (oldNewValue.Item1.IsExisting)
			{
				oldNewValue.Item1.UpdateValue(oldNewValue.Item2);
				oldNewValue.Item1.IsExistManualInputValue = oldNewValue.Item4;
			}
		}
		_table.EndBatchUpdateValue();
	}
}
