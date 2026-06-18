using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class MinFilter<T> : FilterBase
{
	public MinFilter(int col)
	{
		base.col = col;
	}

	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		object value = values.Min((KeyValuePair<int, FilterValue> t) => t.Value.Value);
		FilterValue minValue = FilterValue.FromObject(value);
		return (from t in values
			where t.Value.Equals(minValue)
			select t.Key).ToList();
	}
}
