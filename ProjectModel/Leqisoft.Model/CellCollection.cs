using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class CellCollection : IEnumerable<Cell>, IEnumerable
{
	private readonly Table _table;

	internal readonly List<Cell> _list = new List<Cell>();

	public int Count => _list.Count;

	internal Cell this[int index] => _list[index];

	public Cell Get(int row, int col)
	{
		int collectionIndex = GetCollectionIndex(row, col);
		if (collectionIndex < 0 || collectionIndex >= _list.Count)
			return null;
		return _list[collectionIndex];
	}

	public Cell GetById(Id64 id)
	{
		return _list.FirstOrDefault((Cell c) => c.Id == id);
	}

	public IEnumerator<Cell> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	internal CellCollection(Table table)
	{
		_table = table;
	}

	internal int GetCollectionIndex(int row, int col)
	{
		return row * _table.Columns.Count + col;
	}

	internal void Clear()
	{
		_list.Clear();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal void MoveRows(int rowIndex, int rowCount, int newRowIndex)
	{
		List<Cell> range = _list.GetRange(GetCollectionIndex(rowIndex, 0), rowCount * _table.Columns.Count);
		_list.RemoveRange(GetCollectionIndex(rowIndex, 0), rowCount * _table.Columns.Count);
		_list.InsertRange(GetCollectionIndex((newRowIndex < rowIndex) ? newRowIndex : (newRowIndex - rowCount), 0), range);
	}

	internal void MoveColumns(int colIndex, int colCount, int newColIndex)
	{
		for (int i = 0; i < _table.Rows.Count; i++)
		{
			List<Cell> range = _list.GetRange(GetCollectionIndex(i, colIndex), colCount);
			if (newColIndex < colIndex)
			{
				for (int num = colIndex - 1; num >= newColIndex; num--)
				{
					_list[GetCollectionIndex(i, num + colCount)] = _list[GetCollectionIndex(i, num)];
				}
				for (int j = 0; j < range.Count; j++)
				{
					_list[GetCollectionIndex(i, newColIndex + j)] = range[j];
				}
			}
			else
			{
				for (int k = colIndex + colCount; k < newColIndex; k++)
				{
					_list[GetCollectionIndex(i, k - colCount)] = _list[GetCollectionIndex(i, k)];
				}
				for (int l = 0; l < range.Count; l++)
				{
					_list[GetCollectionIndex(i, newColIndex - colCount + l)] = range[l];
				}
			}
		}
	}

	public Cell GetByCaption(string caption)
	{
		return _list.Where((Cell c) => c.Row.Role == RowRole.Header).FirstOrDefault((Cell c) => c.GetUniqueFormulaName() == caption);
	}
}
