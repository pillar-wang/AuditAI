using System;
using System.Xml.Serialization;

namespace Auditai.Model;

[Serializable]
public abstract class ComparisonFilter<T> : ByIndividualValueFilter where T : IComparable<T>
{
	[XmlAttribute]
	public T Value { get; set; }

	public ComparisonFilter(int col, T value)
		: base(col)
	{
		Value = value;
	}

	public ComparisonFilter()
	{
	}
}
