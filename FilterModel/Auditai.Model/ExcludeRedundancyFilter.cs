using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class ExcludeRedundancyFilter : FilterBase
{
	public ExcludeRedundancyFilter(int col)
		: base(col)
	{
	}

	public ExcludeRedundancyFilter()
	{
	}

	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		IEnumerable<IGrouping<object, KeyValuePair<int, FilterValue>>> source = from kv in values
			group kv by kv.Value.Value;
		return source.Select((IGrouping<object, KeyValuePair<int, FilterValue>> group) => group.First().Key).ToList();
	}
}
