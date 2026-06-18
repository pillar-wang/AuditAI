using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class EquidistanceFilter : FilterBase
{
	private int value;

	public EquidistanceFilter(int col, int value)
		: base(col)
	{
		this.value = value;
	}

	public EquidistanceFilter()
	{
	}

	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		List<int> list = values.Keys.ToList();
		int num = list.Count / value;
		int num2 = 0;
		List<int> list2 = new List<int>();
		while (list2.Count < value && num2 < list.Count)
		{
			list2.Add(list[num2]);
			num2 += num;
		}
		return list2;
	}
}
