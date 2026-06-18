using System.Collections.Generic;
using System.Linq;

namespace System.Data;

public static class DataSetExtensions
{
	public static IEnumerable<DataRow> AsEnumerable(this DataTable dt)
	{
		return dt.Rows.Cast<DataRow>();
	}

	public static T Field<T>(this DataRow row, string columnName)
	{
		object obj = row[columnName];
		if (!Convert.IsDBNull(obj))
		{
			return (T)obj;
		}
		return default(T);
	}
}
