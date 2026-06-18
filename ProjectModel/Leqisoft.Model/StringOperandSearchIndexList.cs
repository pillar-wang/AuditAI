using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Leqisoft.Model;

internal class StringOperandSearchIndexList : CellValueSearchIndexListBase
{
	protected class StringElementCountIndexData
	{
		public string[] ElementsList;

		public Cell Cell;
	}

	#pragma warning disable 0649
	protected class CollectionElement
	{
		public int IndexValue;

		public int CollectionIndex;
	}
#pragma warning restore 0649

	private CellIndexData<string>[] _indexDataForEqualOrNotEqual_StringTypeCell;

	private List<Cell> _indexDataForEqualOrNotEqual_NotStringTypeCell;

	private int _cellsTotalCountForEqualOrNotEqual_StringTypeCell;

	private int _cellsTotalCountForEqualOrNotEqual_NotStringTypeCell;

	private List<Cell> _notEmptyCellList;

	private List<Cell> _emptyStringCellList;

	private StringElementCountIndexData[] _indexDataForGreat;

	private StringElementCountIndexData[] _indexDataForLess;

	private static string[] EmptyStringArray = new string[0];

	public override void BuildIndexData(List<Cell> cellList)
	{
		_sourceCellList = cellList;
	}

	protected void BuildIndexData_ForEqualOrNotEqual()
	{
		List<Cell> sourceCellList = _sourceCellList;
		_indexDataForEqualOrNotEqual_NotStringTypeCell = new List<Cell>(sourceCellList.Count);
		List<CellElement<string>> list = new List<CellElement<string>>(sourceCellList.Count);
		foreach (Cell item in sourceCellList)
		{
			if (item.Value is string text)
			{
				list.Add(new CellElement<string>
				{
					IndexValue = text.Trim(),
					Cell = item
				});
			}
			else
			{
				_indexDataForEqualOrNotEqual_NotStringTypeCell.Add(item);
			}
		}
		_indexDataForEqualOrNotEqual_StringTypeCell = (from c in list
			group c by c.IndexValue into g
			select new CellIndexData<string>
			{
				IndexValue = g.Key,
				CellList = g.Select((CellElement<string> u) => u.Cell).ToList()
			}).ToArray();
		Array.Sort(_indexDataForEqualOrNotEqual_StringTypeCell, delegate(CellIndexData<string> left, CellIndexData<string> right)
		{
			int length = left.IndexValue.Length;
			int length2 = right.IndexValue.Length;
			if (length < length2)
			{
				return -1;
			}
			return (length > length2) ? 1 : left.IndexValue.CompareTo(right.IndexValue);
		});
		_cellsTotalCountForEqualOrNotEqual_NotStringTypeCell = _indexDataForEqualOrNotEqual_NotStringTypeCell.Count;
		_cellsTotalCountForEqualOrNotEqual_StringTypeCell = sourceCellList.Count - _indexDataForEqualOrNotEqual_NotStringTypeCell.Count;
	}

	protected void BuildIndexData_ForNotEmptyCell()
	{
		List<Cell> sourceCellList = _sourceCellList;
		List<Cell> list = new List<Cell>(sourceCellList.Count);
		foreach (Cell item in sourceCellList)
		{
			if (item.Value is string text)
			{
				if (text.Trim() != "")
				{
					list.Add(item);
				}
			}
			else
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			_notEmptyCellList = CellValueSearchIndex.EmptyCellList;
			return;
		}
		list.TrimExcess();
		_notEmptyCellList = list;
	}

	protected void BuildIndexData_ForEmptyStringCell()
	{
		List<Cell> sourceCellList = _sourceCellList;
		List<Cell> list = new List<Cell>(sourceCellList.Count);
		foreach (Cell item in sourceCellList)
		{
			if (item.Value is string text && text.Trim() == "")
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			_emptyStringCellList = CellValueSearchIndex.EmptyCellList;
			return;
		}
		list.TrimExcess();
		_emptyStringCellList = list;
	}

	protected void BuildIndexData_ForGreat()
	{
		List<Cell> sourceCellList = _sourceCellList;
		int count = sourceCellList.Count;
		_indexDataForGreat = new StringElementCountIndexData[count];
		for (int i = 0; i < count; i++)
		{
			StringElementCountIndexData stringElementCountIndexData = new StringElementCountIndexData();
			_indexDataForGreat[i] = stringElementCountIndexData;
			if ((stringElementCountIndexData.Cell = sourceCellList[i]).Value is string text)
			{
				stringElementCountIndexData.ElementsList = text.Split(StringOperand._splitter, StringSplitOptions.RemoveEmptyEntries);
				for (int num = stringElementCountIndexData.ElementsList.Length - 1; num >= 0; num--)
				{
					stringElementCountIndexData.ElementsList[num] = stringElementCountIndexData.ElementsList[num].Trim();
				}
			}
			else
			{
				stringElementCountIndexData.ElementsList = EmptyStringArray;
			}
		}
	}

	protected void BuildIndexData_ForLess()
	{
		List<Cell> sourceCellList = _sourceCellList;
		int count = sourceCellList.Count;
		_indexDataForLess = new StringElementCountIndexData[count];
		for (int i = 0; i < count; i++)
		{
			StringElementCountIndexData stringElementCountIndexData = new StringElementCountIndexData();
			_indexDataForLess[i] = stringElementCountIndexData;
			if ((stringElementCountIndexData.Cell = sourceCellList[i]).Value is string text)
			{
				stringElementCountIndexData.ElementsList = text.Split(StringOperand._splitter, StringSplitOptions.RemoveEmptyEntries);
				for (int num = stringElementCountIndexData.ElementsList.Length - 1; num >= 0; num--)
				{
					stringElementCountIndexData.ElementsList[num] = StringOperand.GetRegxExpression(stringElementCountIndexData.ElementsList[num].Trim());
				}
			}
			else
			{
				stringElementCountIndexData.ElementsList = EmptyStringArray;
			}
		}
	}

	public override List<Cell> FindEqualValue(Operand value)
	{
		if (_indexDataForEqualOrNotEqual_StringTypeCell == null)
		{
			BuildIndexData_ForEqualOrNotEqual();
		}
		StringOperand stringOperand = value.ToStringOp();
		string text = stringOperand.Value.Trim();
		int length = text.Length;
		if (ContainsWildcardChar(text))
		{
			return FindEqual_ContainsWildcardChar(text);
		}
		return BinarySearch_Equal(_indexDataForEqualOrNotEqual_StringTypeCell, text, delegate(string left, string right)
		{
			int length2 = left.Length;
			int length3 = right.Length;
			if (length2 < length3)
			{
				return -1;
			}
			return (length2 > length3) ? 1 : left.CompareTo(right);
		});
	}

	public override List<Cell> FindNotEqualValue(Operand value)
	{
		StringOperand stringOperand = value.ToStringOp();
		string text = stringOperand.Value.Trim();
		int length = text.Length;
		if (length == 0)
		{
			return FindNotEqualEmptyString();
		}
		if (_indexDataForEqualOrNotEqual_StringTypeCell == null)
		{
			BuildIndexData_ForEqualOrNotEqual();
		}
		if (ContainsWildcardChar(text))
		{
			return FindNotEqual_ContainsWildcardChar(text);
		}
		int num = _indexDataForEqualOrNotEqual_StringTypeCell.Length;
		List<Cell> list = new List<Cell>(_sourceCellList.Count);
		for (int i = 0; i < num; i++)
		{
			CellIndexData<string> cellIndexData = _indexDataForEqualOrNotEqual_StringTypeCell[i];
			if (cellIndexData.IndexValue.Length != length)
			{
				list.AddRange(cellIndexData.CellList);
			}
			else if (cellIndexData.IndexValue != text)
			{
				list.AddRange(cellIndexData.CellList);
			}
		}
		return list;
	}

	public override List<Cell> FindGreatThanValue(Operand value)
	{
		StringOperand stringOperand = value.ToStringOp();
		string text = stringOperand.Value.Trim();
		if (text.Length == 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		if (_indexDataForGreat == null)
		{
			BuildIndexData_ForGreat();
		}
		string[] array = text.Split(StringOperand._splitter, StringSplitOptions.RemoveEmptyEntries);
		int num = array.Length;
		string[] array2 = new string[num];
		for (int i = 0; i < num; i++)
		{
			string str = array[i].Trim();
			array2[i] = StringOperand.GetRegxExpression(str);
		}
		List<Cell> list = new List<Cell>(_sourceCellList.Count);
		for (int j = 0; j < _indexDataForGreat.Length; j++)
		{
			StringElementCountIndexData stringElementCountIndexData = _indexDataForGreat[j];
			if (IsSrcContainsDstAllReg(stringElementCountIndexData.ElementsList, array2))
			{
				list.Add(stringElementCountIndexData.Cell);
			}
		}
		return list;
	}

	public override List<Cell> FindGreatThanOrEqualValue(Operand value)
	{
		return FindGreatThanValue(value);
	}

	public override List<Cell> FindLessThanValue(Operand value)
	{
		StringOperand stringOperand = value.ToStringOp();
		string text = stringOperand.Value.Trim();
		if (text.Length == 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		if (_indexDataForLess == null)
		{
			BuildIndexData_ForLess();
		}
		string[] array = text.Split(StringOperand._splitter, StringSplitOptions.RemoveEmptyEntries);
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			array[i] = array[i].Trim();
		}
		List<Cell> list = new List<Cell>(_sourceCellList.Count);
		for (int j = 0; j < _indexDataForLess.Length; j++)
		{
			StringElementCountIndexData stringElementCountIndexData = _indexDataForLess[j];
			if (IsSrcContainsDstAllReg(array, stringElementCountIndexData.ElementsList))
			{
				list.Add(stringElementCountIndexData.Cell);
			}
		}
		return list;
	}

	public override List<Cell> FindLessThanOrEqualValue(Operand value)
	{
		return FindLessThanValue(value);
	}

	private List<Cell> FindNotEqualEmptyString()
	{
		if (_notEmptyCellList != null)
		{
			return _notEmptyCellList;
		}
		if (_indexDataForEqualOrNotEqual_StringTypeCell == null)
		{
			BuildIndexData_ForNotEmptyCell();
			return _notEmptyCellList;
		}
		if (_indexDataForEqualOrNotEqual_StringTypeCell.Length == 0)
		{
			return _indexDataForEqualOrNotEqual_NotStringTypeCell;
		}
		int num = 0;
		int num2 = 0;
		if (_indexDataForEqualOrNotEqual_StringTypeCell[0].IndexValue.Length == 0)
		{
			num = 1;
			num2 = _indexDataForEqualOrNotEqual_StringTypeCell[0].CellList.Count;
		}
		List<Cell> list = new List<Cell>(_cellsTotalCountForEqualOrNotEqual_NotStringTypeCell + _cellsTotalCountForEqualOrNotEqual_StringTypeCell - num2);
		for (int i = num; i < _indexDataForEqualOrNotEqual_StringTypeCell.Length; i++)
		{
			list.AddRange(_indexDataForEqualOrNotEqual_StringTypeCell[i].CellList);
		}
		list.AddRange(_indexDataForEqualOrNotEqual_NotStringTypeCell);
		return list;
	}

	private List<Cell> FindEqualEmptyString()
	{
		if (_emptyStringCellList != null)
		{
			return _emptyStringCellList;
		}
		if (_indexDataForEqualOrNotEqual_StringTypeCell == null)
		{
			BuildIndexData_ForEmptyStringCell();
			return _emptyStringCellList;
		}
		if (_indexDataForEqualOrNotEqual_StringTypeCell.Length == 0)
		{
			return CellValueSearchIndex.EmptyCellList;
		}
		if (_indexDataForEqualOrNotEqual_StringTypeCell[0].IndexValue.Length == 0)
		{
			return _indexDataForEqualOrNotEqual_StringTypeCell[0].CellList;
		}
		return CellValueSearchIndex.EmptyCellList;
	}

	private static int GetCharCountExceptWildcardChar(string str)
	{
		int num = 0;
		int length = str.Length;
		for (int i = 0; i < length; i++)
		{
			char c = str[i];
			if (c != '*' && c != '?')
			{
				num++;
			}
		}
		return num;
	}

	private List<Cell> FindEqual_ContainsWildcardChar(string searchString)
	{
		List<Cell> list = new List<Cell>(_sourceCellList.Count);
		if (searchString.Length == 1 && searchString[0] == '*')
		{
			for (int i = 0; i < _indexDataForEqualOrNotEqual_StringTypeCell.Length; i++)
			{
				list.AddRange(_indexDataForEqualOrNotEqual_StringTypeCell[i].CellList);
			}
			return list;
		}
		string regxExpression = StringOperand.GetRegxExpression(searchString);
		int charCountExceptWildcardChar = GetCharCountExceptWildcardChar(searchString);
		for (int j = 0; j < _indexDataForEqualOrNotEqual_StringTypeCell.Length; j++)
		{
			CellIndexData<string> cellIndexData = _indexDataForEqualOrNotEqual_StringTypeCell[j];
			if (cellIndexData.IndexValue.Length >= charCountExceptWildcardChar && Regex.IsMatch(cellIndexData.IndexValue, regxExpression))
			{
				list.AddRange(cellIndexData.CellList);
			}
		}
		return list;
	}

	private List<Cell> FindNotEqual_ContainsWildcardChar(string searchString)
	{
		if (searchString.Length == 1 && searchString[0] == '*')
		{
			return _indexDataForEqualOrNotEqual_NotStringTypeCell;
		}
		string regxExpression = StringOperand.GetRegxExpression(searchString);
		int charCountExceptWildcardChar = GetCharCountExceptWildcardChar(searchString);
		List<Cell> list = new List<Cell>(_sourceCellList.Count);
		if (_indexDataForEqualOrNotEqual_NotStringTypeCell.Count > 0)
		{
			list.AddRange(_indexDataForEqualOrNotEqual_NotStringTypeCell);
		}
		for (int i = 0; i < _indexDataForEqualOrNotEqual_StringTypeCell.Length; i++)
		{
			CellIndexData<string> cellIndexData = _indexDataForEqualOrNotEqual_StringTypeCell[i];
			if (cellIndexData.IndexValue.Length < charCountExceptWildcardChar)
			{
				list.AddRange(cellIndexData.CellList);
			}
			else if (!Regex.IsMatch(cellIndexData.IndexValue, regxExpression))
			{
				list.AddRange(cellIndexData.CellList);
			}
		}
		return list;
	}

	private bool IsSrcContainsDstAllReg(string[] sourceArr, string[] regArr)
	{
		if (sourceArr.Length == 0 || regArr.Length == 0)
		{
			return false;
		}
		for (int num = regArr.Length - 1; num >= 0; num--)
		{
			string pattern = regArr[num];
			bool flag = false;
			for (int num2 = sourceArr.Length - 1; num2 >= 0; num2--)
			{
				if (Regex.IsMatch(sourceArr[num2], pattern))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	protected static bool ContainsWildcardChar(string strValue)
	{
		for (int num = strValue.Length - 1; num >= 0; num--)
		{
			char c = strValue[num];
			if (c == '*' || c == '?')
			{
				return true;
			}
		}
		return false;
	}
}
