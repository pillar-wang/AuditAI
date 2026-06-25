using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class RandomFilter : FilterBase
{
	private int value;

	public RandomFilter(int col, int value)
		: base(col)
	{
		this.value = value;
	}

	public RandomFilter()
	{
	}

	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		List<int> result = values.Keys.ToList();
		List<int> list = new List<int>();
		if (value == 0)
		{
			return list;
		}
		if (value > values.Count)
		{
			return result;
		}
		Random random = new Random();
		List<int> list2 = values.Keys.ToList();
		while (list.Count < value)
		{
			int index = random.Next(0, list2.Count - 1);
			list.Add(list2[index]);
			list2.RemoveAt(index);
		}
		return list;
	}
}
