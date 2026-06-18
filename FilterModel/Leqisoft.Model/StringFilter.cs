using System;
using System.Xml.Serialization;

namespace Leqisoft.Model;

[Serializable]
public abstract class StringFilter : ByIndividualValueFilter
{
	[XmlAttribute]
	public FilterValue Value { get; set; }

	public StringFilter()
	{
	}

	public StringFilter(int col, FilterValue value)
		: base(col)
	{
		Value = value;
	}
}
