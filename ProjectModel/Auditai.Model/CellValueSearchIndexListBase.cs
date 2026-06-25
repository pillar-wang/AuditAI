using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

internal abstract class CellValueSearchIndexListBase
{
	protected delegate T IndexValueGenerateHandle<T>(Cell cell);

	protected class CellIndexData<T>
	{
		public T IndexValue;

		public List<Cell> CellList;
	}

	protected class CellElement<T>
	{
		public T IndexValue;

		public Cell Cell;
	}

	protected List<Cell> _sourceCellList;

	public abstract void BuildIndexData(List<Cell> cellList);

	public abstract List<Cell> FindEqualValue(Operand value);

	public abstract List<Cell> FindNotEqualValue(Operand value);

	public abstract List<Cell> FindGreatThanValue(Operand value);

	public abstract List<Cell> FindGreatThanOrEqualValue(Operand value);

	public abstract List<Cell> FindLessThanValue(Operand value);

	public abstract List<Cell> FindLessThanOrEqualValue(Operand value);

	protected CellIndexData<T>[] BuildIndexData<T>(List<Cell> cellList, IndexValueGenerateHandle<T> indexValueGenerator, Comparison<T> indexValueComparer)
	{
		int count = cellList.Count;
		if (count == 0)
		{
			return new CellIndexData<T>[0];
		}
		CellElement<T>[] array = new CellElement<T>[count];
		for (int i = 0; i < count; i++)
		{
			Cell cell = cellList[i];
			array[i] = new CellElement<T>
			{
				IndexValue = indexValueGenerator(cell),
				Cell = cell
			};
		}
		List<CellIndexData<T>> list = new List<CellIndexData<T>>(count);
		CellIndexData<T>[] array2 = (from u in array
			group u by u.IndexValue into u
			select new CellIndexData<T>
			{
				IndexValue = u.Key,
				CellList = u.Select((CellElement<T> g) => g.Cell).ToList()
			}).ToArray();
		Array.Sort(array2, (CellIndexData<T> left, CellIndexData<T> right) => indexValueComparer(left.IndexValue, right.IndexValue));
		return array2;
	}

	protected List<Cell> BinarySearch_Equal<T>(CellIndexData<T>[] indexDataArr, T searchDataIndexValue, Comparison<T> indexValueComparer)
	{
		int num = 0;
		int num2 = indexDataArr.Length - 1;
		int num3 = -1;
		while (num <= num2)
		{
			int num4 = (num + num2) / 2;
			int num5 = indexValueComparer(indexDataArr[num4].IndexValue, searchDataIndexValue);
			if (num5 == 0)
			{
				num3 = num4;
				break;
			}
			if (num5 > 0)
			{
				num2 = num4 - 1;
			}
			else
			{
				num = num4 + 1;
			}
		}
		if (num3 == -1)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		return indexDataArr[num3].CellList;
	}

	protected List<Cell> BinarySearch_NotEqual<T>(CellIndexData<T>[] indexDataArr, T searchDataIndexValue, Comparison<T> indexValueComparer, int cellsTotalCount)
	{
		int num = 0;
		int num2 = indexDataArr.Length - 1;
		int num3 = -1;
		while (num <= num2)
		{
			int num4 = (num + num2) / 2;
			int num5 = indexValueComparer(indexDataArr[num4].IndexValue, searchDataIndexValue);
			if (num5 == 0)
			{
				num3 = num4;
				break;
			}
			if (num5 > 0)
			{
				num2 = num4 - 1;
			}
			else
			{
				num = num4 + 1;
			}
		}
		if (num3 == -1)
		{
			List<Cell> list = new List<Cell>(cellsTotalCount);
			foreach (CellIndexData<T> cellIndexData in indexDataArr)
			{
				list.AddRange(cellIndexData.CellList);
			}
			return list;
		}
		int capacity = cellsTotalCount - indexDataArr[num3].CellList.Count;
		List<Cell> list2 = new List<Cell>(capacity);
		for (int j = 0; j < num3; j++)
		{
			list2.AddRange(indexDataArr[j].CellList);
		}
		int num6 = indexDataArr.Length;
		for (int k = num3 + 1; k < num6; k++)
		{
			list2.AddRange(indexDataArr[k].CellList);
		}
		return list2;
	}

	protected List<Cell> BinarySearch_GreatThan<T>(CellIndexData<T>[] indexDataArr, T searchDataIndexValue, Comparison<T> indexValueComparer, int cellsTotalCount)
	{
		if (indexDataArr.Length == 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		int num = 0;
		int num2 = indexDataArr.Length - 1;
		if (indexValueComparer(searchDataIndexValue, indexDataArr[num2].IndexValue) >= 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		if (indexValueComparer(searchDataIndexValue, indexDataArr[0].IndexValue) < 0)
		{
			return _sourceCellList;
		}
		int num3 = -1;
		while (num <= num2)
		{
			int num4 = (num + num2) / 2;
			int num5 = indexValueComparer(indexDataArr[num4].IndexValue, searchDataIndexValue);
			if (num5 == 0)
			{
				num3 = num4 + 1;
				break;
			}
			if (num5 > 0)
			{
				num3 = num4;
				num2 = num4 - 1;
			}
			else
			{
				num = num4 + 1;
			}
		}
		if (num3 == -1)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		List<Cell> list = new List<Cell>(cellsTotalCount);
		int num6 = indexDataArr.Length;
		for (int i = num3; i < num6; i++)
		{
			list.AddRange(indexDataArr[i].CellList);
		}
		return list;
	}

	protected List<Cell> BinarySearch_GreatThanOrEqual<T>(CellIndexData<T>[] indexDataArr, T searchDataIndexValue, Comparison<T> indexValueComparer, int cellsTotalCount)
	{
		if (indexDataArr.Length == 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		int num = 0;
		int num2 = indexDataArr.Length - 1;
		int num3 = indexValueComparer(searchDataIndexValue, indexDataArr[num2].IndexValue);
		if (num3 > 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		if (num3 == 0)
		{
			return indexDataArr[num2].CellList;
		}
		if (indexValueComparer(searchDataIndexValue, indexDataArr[0].IndexValue) <= 0)
		{
			return _sourceCellList;
		}
		int num4 = -1;
		while (num <= num2)
		{
			int num5 = (num + num2) / 2;
			int num6 = indexValueComparer(indexDataArr[num5].IndexValue, searchDataIndexValue);
			if (num6 == 0)
			{
				num4 = num5;
				break;
			}
			if (num6 > 0)
			{
				num4 = num5;
				num2 = num5 - 1;
			}
			else
			{
				num = num5 + 1;
			}
		}
		if (num4 == -1)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		List<Cell> list = new List<Cell>(cellsTotalCount);
		int num7 = indexDataArr.Length;
		for (int i = num4; i < num7; i++)
		{
			list.AddRange(indexDataArr[i].CellList);
		}
		return list;
	}

	protected List<Cell> BinarySearch_LessThan<T>(CellIndexData<T>[] indexDataArr, T searchDataIndexValue, Comparison<T> indexValueComparer, int cellsTotalCount)
	{
		if (indexDataArr.Length == 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		int num = 0;
		int num2 = indexDataArr.Length - 1;
		if (indexValueComparer(searchDataIndexValue, indexDataArr[num2].IndexValue) > 0)
		{
			return _sourceCellList;
		}
		if (indexValueComparer(searchDataIndexValue, indexDataArr[0].IndexValue) <= 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		int num3 = -1;
		while (num <= num2)
		{
			int num4 = (num + num2) / 2;
			int num5 = indexValueComparer(indexDataArr[num4].IndexValue, searchDataIndexValue);
			if (num5 == 0)
			{
				num3 = num4 - 1;
				break;
			}
			if (num5 > 0)
			{
				num2 = num4 - 1;
				continue;
			}
			num3 = num4;
			num = num4 + 1;
		}
		if (num3 == -1)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		List<Cell> list = new List<Cell>(cellsTotalCount);
		for (int i = 0; i <= num3; i++)
		{
			list.AddRange(indexDataArr[i].CellList);
		}
		return list;
	}

	protected List<Cell> BinarySearch_LessThanOrEqual<T>(CellIndexData<T>[] indexDataArr, T searchDataIndexValue, Comparison<T> indexValueComparer, int cellsTotalCount)
	{
		if (indexDataArr.Length == 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		int num = 0;
		int num2 = indexDataArr.Length - 1;
		if (indexValueComparer(searchDataIndexValue, indexDataArr[num2].IndexValue) >= 0)
		{
			return _sourceCellList;
		}
		if (indexValueComparer(searchDataIndexValue, indexDataArr[0].IndexValue) < 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		int num3 = -1;
		while (num <= num2)
		{
			int num4 = (num + num2) / 2;
			int num5 = indexValueComparer(indexDataArr[num4].IndexValue, searchDataIndexValue);
			if (num5 == 0)
			{
				num3 = num4;
				break;
			}
			if (num5 > 0)
			{
				num2 = num4 - 1;
				continue;
			}
			num3 = num4;
			num = num4 + 1;
		}
		if (num3 == -1)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		List<Cell> list = new List<Cell>(cellsTotalCount);
		for (int i = 0; i <= num3; i++)
		{
			list.AddRange(indexDataArr[i].CellList);
		}
		return list;
	}

	protected int IndexCompareDouble(double left, double right)
	{
		if (left < right)
		{
			return -1;
		}
		if (left == right)
		{
			return 0;
		}
		return 1;
	}

	protected int IndexCompareLong(long left, long right)
	{
		if (left < right)
		{
			return -1;
		}
		if (left == right)
		{
			return 0;
		}
		return 1;
	}
}
