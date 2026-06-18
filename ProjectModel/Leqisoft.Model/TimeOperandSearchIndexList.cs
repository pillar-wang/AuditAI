using System;
using System.Collections.Generic;

namespace Leqisoft.Model;

internal class TimeOperandSearchIndexList : CellValueSearchIndexListBase
{
	private CellIndexData<long>[] _indexDataArr;

	private int _cellsTotalCount;

	private long IndexValueGenerate(Cell cell)
	{
		try
		{
			return ValueOperand.FromObject(cell.Value).ToTime().Value.Ticks;
		}
		catch (Exception)
		{
			return TimeSpan.MinValue.Ticks;
		}
	}

	public override void BuildIndexData(List<Cell> cellList)
	{
		_indexDataArr = BuildIndexData(cellList, IndexValueGenerate, base.IndexCompareLong);
		_cellsTotalCount = cellList.Count;
		_sourceCellList = cellList;
	}

	public override List<Cell> FindEqualValue(Operand value)
	{
		long ticks = value.ToDate().Value.Ticks;
		return BinarySearch_Equal(_indexDataArr, ticks, base.IndexCompareLong);
	}

	public override List<Cell> FindNotEqualValue(Operand value)
	{
		long ticks = value.ToDate().Value.Ticks;
		return BinarySearch_NotEqual(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindGreatThanValue(Operand value)
	{
		long ticks = value.ToDate().Value.Ticks;
		return BinarySearch_GreatThan(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindGreatThanOrEqualValue(Operand value)
	{
		long ticks = value.ToDate().Value.Ticks;
		return BinarySearch_GreatThanOrEqual(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindLessThanValue(Operand value)
	{
		long ticks = value.ToDate().Value.Ticks;
		return BinarySearch_LessThan(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindLessThanOrEqualValue(Operand value)
	{
		long ticks = value.ToDate().Value.Ticks;
		return BinarySearch_LessThanOrEqual(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}
}
