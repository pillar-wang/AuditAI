using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class RowCollection : IEnumerable<Row>, IEnumerable
{
	internal readonly List<Row> _list = new List<Row>();

	private readonly Table _table;

	public int Count => _list.Count;

	public Row this[int index] => _list[index];

	public int DefaultHeight => UserSet.Config.TableStyle.TableRowHeight;

	public void Append(int count)
	{
		Insert(_list.Count, count);
	}

	public void Insert(int index, int count)
	{
		List<CellStyle> list = null;
		if (index == _list.Count || _list[index].Role == RowRole.Header || _list[index].Role == RowRole.Fixed || _list[index].Role == RowRole.Total || _list[index].Role == RowRole.Subtotal)
		{
			for (int num = index - 1; num >= 0; num--)
			{
				RowRole role = _list[num].Role;
				if (role == RowRole.Normal || role == RowRole.Among || role == RowRole.Minus)
				{
					list = (from c in _table.Rows[num].GetCells()
						select c.Style).ToList();
					break;
				}
			}
		}
		else
		{
			for (int i = index; i < _list.Count; i++)
			{
				RowRole role2 = _list[i].Role;
				if (role2 == RowRole.Normal || role2 == RowRole.Among || role2 == RowRole.Minus)
				{
					list = (from c in _table.Rows[i].GetCells()
						select c.Style).ToList();
					break;
				}
			}
		}
		List<Row> list2 = new List<Row>(count);
		for (int j = 0; j < count; j++)
		{
			Row row = new Row
			{
				Id = Project.Current.GetNextId(),
				NeedSave = true,
				Index = index + j,
				Table = _table,
				Visible = true,
				Creator = User.Current.Id
			};
			if (_list.Count == 0)
			{
				row.Height = DefaultHeight;
			}
			else if ((index == _list.Count || _list[index].Role == RowRole.Header || _list[index].Role == RowRole.Fixed) && index != 0)
			{
				row.Height = _list[index - 1].Height;
			}
			else
			{
				row.Height = _list[index].Height;
			}
			list2.Add(row);
		}
		_list.InsertRange(index, list2);
		ResetIndex();
		List<Cell> list3 = new List<Cell>(count * _table.Columns.Count);
		for (int k = 0; k < count; k++)
		{
			for (int l = 0; l < _table.Columns.Count; l++)
			{
				Cell cell = _table.MakeNewCell();
				cell.Row = list2[k];
				cell.Column = _table.Columns[l];
				if (list != null)
				{
					cell.Style = list[l];
				}
				list3.Add(cell);
			}
		}
		_table.Cells._list.InsertRange(index * _table.Columns.Count, list3);
		_table.NeedSave = true;
		_table.Ticket.IsCacheExpired = true;
		FormulaEvaluator.ClearCache();
	}

	public void Move(int index, int count, int newIndex)
	{
		if (newIndex < index || newIndex > index + count)
		{
			List<Row> range = _list.GetRange(index, count);
			_list.RemoveRange(index, count);
			_list.InsertRange((newIndex < index) ? newIndex : (newIndex - count), range);
			ResetIndex();
			_table.Cells.MoveRows(index, count, newIndex);
			_table.NeedSave = true;
			_table.RemoveInvalidMerges();
			_table.Ticket.IsCacheExpired = true;
		}
	}

	public void Reorder(int startIndex, List<int> pickupOrder)
	{
		_table.NeedSave |= NeedSave();
		List<Row> list = new List<Row>(pickupOrder.Count);
		List<Cell> list2 = new List<Cell>(pickupOrder.Count * _table.Columns.Count);
		foreach (int item in pickupOrder)
		{
			list.Add(this[item]);
			list2.AddRange(_table.Cells._list.GetRange(_table.Cells.GetCollectionIndex(this[item].Index, 0), _table.Columns.Count));
		}
		_list.RemoveRange(startIndex, pickupOrder.Count);
		_list.InsertRange(startIndex, list);
		_table.Cells._list.RemoveRange(startIndex * _table.Columns.Count, pickupOrder.Count * _table.Columns.Count);
		_table.Cells._list.InsertRange(startIndex * _table.Columns.Count, list2);
		ResetIndex();
		_table.Ticket.IsCacheExpired = true;
		bool NeedSave()
		{
			if (pickupOrder.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < pickupOrder.Count; i++)
			{
				if (pickupOrder[i] != startIndex + i)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void Remove(int index, int count)
	{
		if (index >= _table.Rows.Count)
		{
			return;
		}
		for (int i = index; i < index + count; i++)
		{
			for (int j = 0; j < _table.Columns.Count; j++)
			{
				Cell cell = _table[i, j];
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
				if (cell.HasFormula)
				{
					_table.Project.FormulaManager.RemoveHostObject(_table.Id, cell.Id);
				}
				CellMerge cellMerge = _table.MergedCells.FirstOrDefault((CellMerge m) => m.TopLeft == cell || m.BottomRight == cell);
				if (cellMerge != null)
				{
					_table.RemoveMerge(cellMerge);
				}
			}
			Row row = _list[i];
			row.NeedSave = true;
			if (row.Status == SyncStatus.New)
			{
				_table.RowsToDelete.Add(row.Id);
			}
			else if (row.Status == SyncStatus.Synced)
			{
				_table.RemovedRows.Add(row.Id);
			}
			row.Status = SyncStatus.LocalDeleted;
			_table.HeaderRowCache.Remove(row);
		}
		_table.Cells._list.RemoveRange(_table.Cells.GetCollectionIndex(index, 0), count * _table.Columns.Count);
		_list.RemoveRange(index, count);
		ResetIndex();
		_table.Project.FormulaMapDirty = true;
		_table.NeedSave = true;
		_table.Ticket.IsCacheExpired = true;
		FormulaEvaluator.ClearCache();
	}

	public IEnumerator<Row> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	internal void ResetIndex()
	{
		for (int i = 0; i < _list.Count; i++)
		{
			_list[i].Index = i;
		}
	}

	internal RowCollection(Table table)
	{
		_table = table;
	}

	internal void Clear()
	{
		_list.Clear();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
