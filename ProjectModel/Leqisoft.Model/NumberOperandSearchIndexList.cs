using System;
using System.Collections.Generic;

namespace Leqisoft.Model;

internal class NumberOperandSearchIndexList : CellValueSearchIndexListBase
{
	private CellIndexData<double>[] _indexDataArr;

	private int _cellsTotalCount;

	private double IndexValueGenerate(Cell cell)
	{
		try
		{
			return ValueOperand.FromObject(cell.Value).ToNumber().Value;
		}
		catch (Exception)
		{
			return 0.0;
		}
	}

	public override void BuildIndexData(List<Cell> cellList)
	{
		_indexDataArr = BuildIndexData(cellList, IndexValueGenerate, base.IndexCompareDouble);
		_cellsTotalCount = cellList.Count;
		_sourceCellList = cellList;
	}

	public override List<Cell> FindEqualValue(Operand value)
	{
		double value2 = value.ToNumber().Value;
		return BinarySearch_Equal(_indexDataArr, value2, base.IndexCompareDouble);
	}

	public override List<Cell> FindNotEqualValue(Operand value)
	{
		double value2 = value.ToNumber().Value;
		return BinarySearch_NotEqual(_indexDataArr, value2, base.IndexCompareDouble, _cellsTotalCount);
	}

	public override List<Cell> FindGreatThanValue(Operand value)
	{
		double value2 = value.ToNumber().Value;
		return BinarySearch_GreatThan(_indexDataArr, value2, base.IndexCompareDouble, _cellsTotalCount);
	}

	public override List<Cell> FindGreatThanOrEqualValue(Operand value)
	{
		double value2 = value.ToNumber().Value;
		return BinarySearch_GreatThanOrEqual(_indexDataArr, value2, base.IndexCompareDouble, _cellsTotalCount);
	}

	public override List<Cell> FindLessThanValue(Operand value)
	{
		double value2 = value.ToNumber().Value;
		return BinarySearch_LessThan(_indexDataArr, value2, base.IndexCompareDouble, _cellsTotalCount);
	}

	public override List<Cell> FindLessThanOrEqualValue(Operand value)
	{
		double value2 = value.ToNumber().Value;
		return BinarySearch_LessThanOrEqual(_indexDataArr, value2, base.IndexCompareDouble, _cellsTotalCount);
	}
}
