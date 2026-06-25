using System;
using Auditai.Model;

namespace Auditai.UI.Controls;

[Serializable]
public class ColumnInfo
{
	public string Id { get; set; }

	public string Caption { get; set; }

	public FilterDataType DataType { get; set; }

	public string DataTypeFormatString { get; set; }
}
