using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

	// SCS0005 弱随机数：PPS（概率规模抽样）为审计统计抽样，非安全用途，同样需可复现性，故使用 PNRG。
	[SuppressMessage("Security", "SCS0005:Weak random number generator", Justification = "审计统计抽样，非安全用途")]
	protected internal override List<int> Apply(Dictionary<int, FilterValue> values)
	{
		if (values == null || values.Count == 0 || value <= 0)
		{
			return new List<int>();
		}
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
			else if (list.Count > 0)
			{
				// 修复：Next(min, max) 的 max 为不含上界，原 "list.Count - 1" 会选不到最后一个元素，
				// 且当 list 已空时 Next(0,-1) 抛 ArgumentOutOfRangeException。
				int item = list[rdm.Next(0, list.Count)];
				list2.Add(item);
				list.Remove(item);
			}
			else
			{
				// 兜底：Pool 已空却仍未凑够样本数，为避免死循环/崩溃直接退出。
				break;
			}
		}
		return list2;
		decimal randomDecimal(decimal max)
		{
			if (max <= 0m)
			{
				return 0m;
			}
			// 修复：当 max ∈ (0,1)（如单条金额不足 1 元、多条金额累计和 < 1）时，
			// 原 (int)max == 0 会让 rdm.Next(0, 0) 抛 ArgumentOutOfRangeException 直接崩溃。
			// 概率规模抽样在累计和小于 1 时指针必然落在第一条，直接返回 0 即可。
			if (max < 1m)
			{
				return 0m;
			}
			if (max < 2147483647m)
			{
				return rdm.Next(0, (int)max);
			}
			decimal num = max / 2147483647m;
			decimal num2 = max % 2147483647m;
			decimal num3 = 0m;
			// 修复：原 int 累加在 max ≥ ~2.15e9 时溢出为负指针，导致 First(t => t.Value >= pointer)
			// 恒选第一条记录，PPS 抽样完全失真。改用 decimal 累加并保持均匀。
			// 防御：当 max 极大时（如超大金额单元格），num 可达数亿次循环导致界面卡死。
			// 限制迭代上限，超出部分按比例归并在最后一个区间上，确保有界返回。
			decimal iterations = Math.Min(num, 100000m);
			for (int i = 0; (decimal)i < iterations; i++)
			{
				num3 += (decimal)rdm.Next(0, int.MaxValue);
			}
			// 对 (0,1) 的余数区间，用均匀随机小数补齐，避免 (int)num2 == 0 时崩溃。
			num3 += (int)Math.Floor((decimal)rdm.NextDouble() * num2);
			return num3;
		}
	}
}
