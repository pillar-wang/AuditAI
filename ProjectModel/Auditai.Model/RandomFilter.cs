﻿using System;
using System.Collections.Generic;

// 随机抽样，使用 System.Random 足以满足审计抽样需求
#pragma warning disable SCS0005
namespace Auditai.Model;

public sealed class RandomFilter : SampleCountFilter
{
	public override HashSet<int> Execute(List<FilterValue> data)
	{
		if (base.Count > data.Count)
		{
			base.Count = data.Count;
		}
		Random random = new Random();
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < base.Count; i++)
		{
			int item;
			do
			{
				item = random.Next(0, data.Count);
			}
			while (hashSet.Contains(item));
			hashSet.Add(item);
		}
		return hashSet;
	}
}
