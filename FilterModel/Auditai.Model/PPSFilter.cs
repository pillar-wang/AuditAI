using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class PPSFilter : FilterBase
{
	private int value { get; set; }

	private Dictionary<int, object> dic { get; set; }

	public PPSFilter(int col, int value)
		: base(col)
	{
		this.value = value;
	}

	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		decimal sum = default(decimal);
		List<int> list = values.Keys.ToList();
		decimal result;
		Dictionary<int, decimal> source = values.ToDictionary((KeyValuePair<int, FilterValue> t) => t.Key, (KeyValuePair<int, FilterValue> v) => decimal.TryParse(v.Value.ToString(), out result) ? (sum += result) : (sum += 1m));
		Random rdm = new Random();
		List<int> list2 = new List<int>();
		while (list2.Count < value && list2.Count < values.Count)
		{
			decimal pointer = randomDecimal(sum);
			int key = source.First((KeyValuePair<int, decimal> t) => Convert.ToDecimal(t.Value) >= pointer).Key;
			if (!list2.Contains(key))
			{
				list2.Add(key);
				list.Remove(key);
			}
			else
			{
				int item = list[rdm.Next(0, list.Count - 1)];
				list2.Add(item);
				list.Remove(item);
			}
		}
		return list2;
		decimal randomDecimal(decimal max)
		{
			if (max < 2147483647m)
			{
				return rdm.Next(0, (int)max);
			}
			decimal num = max / 2147483647m;
			decimal num2 = max % 2147483647m;
			int num3 = 0;
			for (int i = 0; (decimal)i < num; i++)
			{
				num3 += rdm.Next(0, int.MaxValue);
			}
			num3 += rdm.Next(0, (int)num2);
			return num3;
		}
	}
}
