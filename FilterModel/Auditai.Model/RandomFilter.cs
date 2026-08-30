using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

	// SCS0005 弱随机数：此处为审计统计抽样（随机等概率选取 n 条记录），
	// 并非安全场景（无保密/防预测需求），且需满足可复现的抽样记录，故使用 PNRG 即可。
	[SuppressMessage("Security", "SCS0005:Weak random number generator", Justification = "审计统计抽样，非安全用途")]
	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		List<int> result = values.Keys.ToList();
		if (value <= 0)
		{
			return new List<int>();
		}
		if (value >= result.Count)
		{
			return result;
		}
		Random random = new Random();
		List<int> pool = result.ToList();
		List<int> list = new List<int>();
		while (list.Count < value)
		{
			// 修复：Next(min, max) 的 max 为不含区间的上界，原 "list2.Count - 1" 导致
			// 永远选不到最后一个元素，且当 Count==1 时生成 Next(0,-1) 抛 ArgumentOutOfRangeException。
			int index = random.Next(0, pool.Count);
			list.Add(pool[index]);
			pool.RemoveAt(index);
		}
		return list;
	}
}
