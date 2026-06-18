using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class DuplicateValueFilter : FilterBase
{
	public DuplicateValueFilter(int col)
		: base(col)
	{
	}

	public DuplicateValueFilter()
	{
	}

	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		IEnumerable<IGrouping<object, KeyValuePair<int, FilterValue>>> source = from kv in values
			group kv by kv.Value.Value;
		IEnumerable<IGrouping<object, KeyValuePair<int, FilterValue>>> source2 = source.Where((IGrouping<object, KeyValuePair<int, FilterValue>> group) => group.Count() > 1);
		return (from @group in source2
			from kv in @group
			select kv.Key).ToList();
	}
}
