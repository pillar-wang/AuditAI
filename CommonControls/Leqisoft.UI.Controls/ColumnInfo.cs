using System;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls;

[Serializable]
public class ColumnInfo
{
	public string Id { get; set; }

	public string Caption { get; set; }

	public FilterDataType DataType { get; set; }

	public string DataTypeFormatString { get; set; }
}
