using System.Collections.Generic;
using System.Data;

namespace LedgerImport;

public class DataTable : System.Data.DataTable
{
	private Dictionary<DataRow, object> rowsTagDic = new Dictionary<DataRow, object>();

	private Dictionary<DataColumn, object> colsTagDic = new Dictionary<DataColumn, object>();

	public void SetTag(DataRow row, object tag)
	{
		if (rowsTagDic.ContainsKey(row))
		{
			rowsTagDic[row] = tag;
		}
		else
		{
			rowsTagDic.Add(row, tag);
		}
	}

	public object GetTag(DataRow row)
	{
		if (rowsTagDic.ContainsKey(row))
		{
			return rowsTagDic[row];
		}
		return null;
	}

	public void SetTag(DataColumn col, object tag)
	{
		if (colsTagDic.ContainsKey(col))
		{
			colsTagDic[col] = tag;
		}
		else
		{
			colsTagDic.Add(col, tag);
		}
	}

	public object GetTag(DataColumn col)
	{
		if (colsTagDic.ContainsKey(col))
		{
			return colsTagDic[col];
		}
		return null;
	}
}
