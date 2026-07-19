using System.Linq;

namespace Auditai.Model;

public class UnmergeCellsCommand : CommandBase
{
	private Table _table;
	private int _row;
	private int _col;
	private CellMerge _removedMerge;
	private SyncStatus _removedMergeStatus;
	private bool _hasExecuted;

	public UnmergeCellsCommand(Table table, int row, int col)
	{
		_table = table;
		_row = row;
		_col = col;
		_hasExecuted = false;
	}

	public override void Execute()
	{
		if (!_hasExecuted)
		{
			_removedMerge = _table.MergedCells.FirstOrDefault(c =>
				c.TopLeft.Row.Index == _row && c.TopLeft.Column.Index == _col);
			if (_removedMerge != null)
			{
				_removedMergeStatus = _removedMerge.Status;
			}
			_table.UnmergeCells(_row, _col);
			_hasExecuted = true;
		}
		else
		{
			// Redo：直接移除合并（不创建新对象）
			if (_removedMerge != null && _table.MergedCells.Contains(_removedMerge))
			{
				_table.MergedCells.Remove(_removedMerge);
				_table.RemovedMerges.Add(_removedMerge.Id);
			}
		}
		_table.NeedSave = true;
	}

	public override void Undo()
	{
		if (_removedMerge != null)
		{
			// 恢复合并对象和原始 Status
			_removedMerge.Status = _removedMergeStatus;
			_table.MergedCells.Add(_removedMerge);
			// 清理 RemovedMerges（移除 Execute 时添加的 ID）
			_table.RemovedMerges.Remove(_removedMerge.Id);
			_table.MergesToDelete.Remove(_removedMerge.Id);
			_table.NeedSave = true;
		}
	}
}
