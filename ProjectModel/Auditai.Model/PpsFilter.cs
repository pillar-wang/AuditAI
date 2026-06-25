﻿using System;
using System.Collections.Generic;
using System.Linq;

// PPS 抽样，使用 System.Random 足以满足审计抽样需求
#pragma warning disable SCS0005
namespace Auditai.Model;

public sealed class PpsFilter : SampleCountFilter
{
	public override HashSet<int> Execute(List<FilterValue> data)
	{
		HashSet<int> hashSet = new HashSet<int>();
		Random random = new Random();
		double num = data.Where((FilterValue fv) => fv.DataType == FilterDataType.Number).Sum((FilterValue fv) => fv.Number);
		List<double> list = new List<double>();
		double num2 = 0.0;
		foreach (FilterValue datum in data)
		{
			if (datum.DataType == FilterDataType.Number)
			{
				num2 += datum.Number / num;
			}
			list.Add(num2);
		}
		for (int i = 0; i < base.Count; i++)
		{
			double rd = random.NextDouble();
			hashSet.Add(list.FindIndex((double d) => rd <= d));
		}
		return hashSet;
	}
}
