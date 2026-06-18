using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Leqisoft.Model;

[Serializable]
public abstract class FilterBase
{
	[XmlAttribute]
	public FilterRelation relation { get; set; }

	[XmlAttribute]
	public int col { get; set; }

	public FilterBase(int col)
	{
		this.col = col;
	}

	public FilterBase()
	{
	}

	protected internal abstract List<int> Apply(Dictionary<int, FilterValue> values);
}
