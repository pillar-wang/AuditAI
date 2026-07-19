using System.Collections.Generic;

namespace Auditai.Model;

public class InsertColumnsCommand : CommandBase
{
	private Table _table;
	private int _index;
	private int _count;

	// 保存创建的 Column 和 Cell 对象，使 Redo 时复用同一批对象（保持 ID 一致）
	private List<Column> _createdColumns;
	private List<Cell> _createdCells;
	private bool _hasExecuted;

	public InsertColumnsCommand(Table table, int index, int count)
	{
		_table = table;
		_index = index;
		_count = count;
		_createdColumns = new List<Column>();
		_createdCells = new List<Cell>();
		_hasExecuted = false;
	}

	public override void Execute()
	{
		if (!_hasExecuted)
		{
			// 首次执行：通过 Insert 创建新列，然后保存创建的对象引用
			_table.Columns.Insert(_index, _count);
			// 保存新创建的 Column 对象
			for (int i = _index; i < _index + _count && i < _table.Columns.Count; i++)
			{
				_createdColumns.Add(_table.Columns[i]);
			}
			// 保存新创建的 Cell 对象（按行排列，匹配 Cells._list 的顺序）
			for (int i = 0; i < _table.Rows.Count; i++)
			{
				for (int j = _index; j < _index + _count && j < _table.Columns.Count; j++)
				{
					_createdCells.Add(_table[i, j]);
				}
			}
			_hasExecuted = true;
		}
		else
		{
			// Redo：直接重新插入之前创建的对象（保持 ID 一致）
			int rowCount = _table.Rows.Count;
			int currentColCount = _table.Columns.Count;
			_table.Columns._list.InsertRange(_index, _createdColumns);
			_table.Columns.ResetIndex();
			// 从最后一行开始插入 Cell，避免索引偏移
			for (int i = rowCount - 1; i >= 0; i--)
			{
				int insertPos = i * currentColCount + _index;
				int cellStart = i * _count;
				_table.Cells._list.InsertRange(insertPos, _createdCells.GetRange(cellStart, _count));
			}
			// 恢复 Status
			foreach (var col in _createdColumns)
			{
				col.Status = SyncStatus.New;
			}
			foreach (var cell in _createdCells)
			{
				cell.Status = SyncStatus.New;
				cell.NeedSave = true;
			}
			// 清理 Undo 时添加到同步列表的 ID
			foreach (var col in _createdColumns)
			{
				_table.ColumnsToDelete.Remove(col.Id);
				_table.RemovedColumns.Remove(col.Id);
			}
			foreach (var cell in _createdCells)
			{
				_table.CellsToDelete.Remove(cell.Id);
				_table.RemovedCells.Remove(cell.Id);
			}
			_table.Project.FormulaMapDirty = true;
			_table.Ticket.IsCacheExpired = true;
			FormulaEvaluator.ClearCache();
			_table.NeedSave = true;
		}
	}

	public override void Undo()
	{
		int rowCount = _table.Rows.Count;
		int colCount = _table.Columns.Count;

		// 1. 从内部列表移除 Cell（每行移除 _count 个）
		for (int i = rowCount - 1; i >= 0; i--)
		{
			int removePos = i * colCount + _index;
			_table.Cells._list.RemoveRange(removePos, _count);
		}
		// 2. 从内部列表移除 Column
		_table.Columns._list.RemoveRange(_index, _count);
		_table.Columns.ResetIndex();

		// 3. 标记为已删除（用于服务端同步）
		foreach (var col in _createdColumns)
		{
			if (col.HasFormula)
			{
				_table.Project.FormulaManager.RemoveHostObject(_table.Id, col.Id);
			}
			if (col.Status == SyncStatus.New)
			{
				_table.ColumnsToDelete.Add(col.Id);
			}
			else if (col.Status == SyncStatus.Synced)
			{
				_table.RemovedColumns.Add(col.Id);
			}
			col.Status = SyncStatus.LocalDeleted;
		}
		foreach (var cell in _createdCells)
		{
			if (cell.HasFormula)
			{
				_table.Project.FormulaManager.RemoveHostObject(_table.Id, cell.Id);
			}
			if (cell.Status == SyncStatus.New)
			{
				_table.CellsToDelete.Add(cell.Id);
			}
			else if (cell.Status == SyncStatus.Synced)
			{
				_table.RemovedCells.Add(cell.Id);
			}
			cell.Status = SyncStatus.LocalDeleted;
			cell.NeedSave = true;
		}
		_table.Project.FormulaMapDirty = true;
		_table.NeedSave = true;
	}
}
