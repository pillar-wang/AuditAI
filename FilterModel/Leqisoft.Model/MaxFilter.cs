﻿using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class MaxFilter<T> : FilterBase
{
	public MaxFilter(int col)
	{
		base.col = col;
	}

	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		object value = values.Max((KeyValuePair<int, FilterValue> t) => t.Value.Value);
		FilterValue maxValue = FilterValue.FromObject(value);
		return (from v in values
			where v.Value.Equals(maxValue)
			select v into t
			select t.Key).ToList();
	}
}
