using System;
using System.Collections.Generic;

namespace Auditai.Model;

internal class CellValueSearchIndex
{
	protected CellValueSearchIndexListBase[] _typeIndexArr;

	protected List<Cell> _searchList;

	public static List<Cell> EmptyCellList = new List<Cell>();

	public CellValueSearchIndex(List<Cell> searchList)
	{
		_typeIndexArr = new CellValueSearchIndexListBase[20];
		_searchList = searchList;
	}

	public List<Cell> FindEqualValue(Operand value)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		CellValueSearchIndexListBase cellValueSearchIndexListBase = _typeIndexArr[(int)operandType];
		if (cellValueSearchIndexListBase == null)
		{
			cellValueSearchIndexListBase = Build(operandType);
			_typeIndexArr[(int)operandType] = cellValueSearchIndexListBase;
		}
		return cellValueSearchIndexListBase.FindEqualValue(value);
	}

	public List<Cell> FindNotEqualValue(Operand value)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		CellValueSearchIndexListBase cellValueSearchIndexListBase = _typeIndexArr[(int)operandType];
		if (cellValueSearchIndexListBase == null)
		{
			cellValueSearchIndexListBase = Build(operandType);
			_typeIndexArr[(int)operandType] = cellValueSearchIndexListBase;
		}
		return cellValueSearchIndexListBase.FindNotEqualValue(value);
	}

	public List<Cell> FindGreatThanValue(Operand value)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		CellValueSearchIndexListBase cellValueSearchIndexListBase = _typeIndexArr[(int)operandType];
		if (cellValueSearchIndexListBase == null)
		{
			cellValueSearchIndexListBase = Build(operandType);
			_typeIndexArr[(int)operandType] = cellValueSearchIndexListBase;
		}
		return cellValueSearchIndexListBase.FindGreatThanValue(value);
	}

	public List<Cell> FindGreatThanOrEqualValue(Operand value)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		CellValueSearchIndexListBase cellValueSearchIndexListBase = _typeIndexArr[(int)operandType];
		if (cellValueSearchIndexListBase == null)
		{
			cellValueSearchIndexListBase = Build(operandType);
			_typeIndexArr[(int)operandType] = cellValueSearchIndexListBase;
		}
		return cellValueSearchIndexListBase.FindGreatThanOrEqualValue(value);
	}

	public List<Cell> FindLessThanValue(Operand value)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		CellValueSearchIndexListBase cellValueSearchIndexListBase = _typeIndexArr[(int)operandType];
		if (cellValueSearchIndexListBase == null)
		{
			cellValueSearchIndexListBase = Build(operandType);
			_typeIndexArr[(int)operandType] = cellValueSearchIndexListBase;
		}
		return cellValueSearchIndexListBase.FindLessThanValue(value);
	}

	public List<Cell> FindLessThanOrEqualValue(Operand value)
	{
		OperandType operandType = value.OperandType;
		if (operandType == OperandType.CellOperand)
		{
			operandType = ((CellOperand)value).Value.OperandType;
		}
		CellValueSearchIndexListBase cellValueSearchIndexListBase = _typeIndexArr[(int)operandType];
		if (cellValueSearchIndexListBase == null)
		{
			cellValueSearchIndexListBase = Build(operandType);
			_typeIndexArr[(int)operandType] = cellValueSearchIndexListBase;
		}
		return cellValueSearchIndexListBase.FindLessThanOrEqualValue(value);
	}

	private CellValueSearchIndexListBase Build(OperandType searchValueType)
	{
		CellValueSearchIndexListBase cellValueSearchIndexListBase = null;
		cellValueSearchIndexListBase = searchValueType switch
		{
			OperandType.NumberOperand => new NumberOperandSearchIndexList(), 
			OperandType.DateOperand => new DateOperandSearchIndexList(), 
			OperandType.DateYearMonthOperand => new DateYearMonthOperandSearchIndexList(), 
			OperandType.TimeOperand => new TimeOperandSearchIndexList(), 
			OperandType.BoolOperand => new BoolOperandSearchIndexList(), 
			OperandType.StringOperand => new StringOperandSearchIndexList(), 
			_ => throw new NotImplementedException("不支持" + searchValueType.ToString() + "类型的查找."), 
		};
		cellValueSearchIndexListBase.BuildIndexData(_searchList);
		return cellValueSearchIndexListBase;
	}
}
