using System;
using System.Collections.Generic;

namespace Leqisoft.Model;

internal class DateYearMonthOperandSearchIndexList : CellValueSearchIndexListBase
{
	private static readonly long MinValue = new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, 1, 0, 0, 0).Ticks;

	private CellIndexData<long>[] _indexDataArr;

	private int _cellsTotalCount;

	private long IndexValueGenerate(Cell cell)
	{
		try
		{
			DateTime date = ValueOperand.FromObject(cell.Value).ToDateYearMonth().Value.Date;
			return new DateTime(date.Year, date.Month, 1, 0, 0, 0).Ticks;
		}
		catch (Exception)
		{
			return MinValue;
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
		DateTime date = value.ToDateYearMonth().Value.Date;
		long ticks = new DateTime(date.Year, date.Month, 1, 0, 0, 0).Ticks;
		return BinarySearch_Equal(_indexDataArr, ticks, base.IndexCompareLong);
	}

	public override List<Cell> FindNotEqualValue(Operand value)
	{
		DateTime date = value.ToDateYearMonth().Value.Date;
		long ticks = new DateTime(date.Year, date.Month, 1, 0, 0, 0).Ticks;
		return BinarySearch_NotEqual(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindGreatThanValue(Operand value)
	{
		DateTime date = value.ToDateYearMonth().Value.Date;
		long ticks = new DateTime(date.Year, date.Month, 1, 0, 0, 0).Ticks;
		return BinarySearch_GreatThan(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindGreatThanOrEqualValue(Operand value)
	{
		DateTime date = value.ToDateYearMonth().Value.Date;
		long ticks = new DateTime(date.Year, date.Month, 1, 0, 0, 0).Ticks;
		return BinarySearch_GreatThanOrEqual(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindLessThanValue(Operand value)
	{
		DateTime date = value.ToDateYearMonth().Value.Date;
		long ticks = new DateTime(date.Year, date.Month, 1, 0, 0, 0).Ticks;
		return BinarySearch_LessThan(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}

	public override List<Cell> FindLessThanOrEqualValue(Operand value)
	{
		DateTime date = value.ToDateYearMonth().Value.Date;
		long ticks = new DateTime(date.Year, date.Month, 1, 0, 0, 0).Ticks;
		return BinarySearch_LessThanOrEqual(_indexDataArr, ticks, base.IndexCompareLong, _cellsTotalCount);
	}
}
