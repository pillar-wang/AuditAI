﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Auditai.Model;

[Serializable]
public class C1FlexGridFilters
{
	public List<FilterBase> filters { get; set; }

	public int ResultCount { get; set; }

	public C1FlexGridFilters()
	{
		filters = new List<FilterBase>();
	}

	public void Clear()
	{
		filters.Clear();
	}

	public C1FlexGridFilters First(FilterBase filter)
	{
		filters.Clear();
		filters.Add(filter);
		return this;
	}

	public C1FlexGridFilters And(FilterBase filter)
	{
		filter.relation = FilterRelation.And;
		filters.Add(filter);
		return this;
	}

	public C1FlexGridFilters Or(FilterBase filter)
	{
		filter.relation = FilterRelation.Or;
		filters.Add(filter);
		return this;
	}

	public bool IsExtract(FilterBase filter)
	{
		if (!(filter.GetType() == typeof(RandomFilter)) && !(filter.GetType() == typeof(EquidistanceFilter)))
		{
			return filter.GetType() == typeof(PPSFilter);
		}
		return true;
	}

	internal List<int> Apply(List<Dictionary<int, FilterValue>> source)
	{
		List<int> result = source[0].Keys.ToList();
		if (filters.Count == 0)
		{
			ResultCount = result.Count;
			return result;
		}
		if (filters.First().relation == FilterRelation.Or)
		{
			result.Clear();
		}
		foreach (FilterBase filter in filters)
		{
			if (filter.col >= source.Count)
			{
				continue;
			}
			if (filter.relation == FilterRelation.And)
			{
				if (IsExtract(filter))
				{
					Dictionary<int, FilterValue> values = source[filter.col].Where((KeyValuePair<int, FilterValue> t) => result.Contains(t.Key)).ToDictionary((KeyValuePair<int, FilterValue> t) => t.Key, (KeyValuePair<int, FilterValue> t) => t.Value);
					result = filter.Apply(values);
				}
				else
				{
					List<int> second = filter.Apply(source[filter.col]);
					result = result.Intersect(second).ToList();
				}
			}
			else
			{
				if (filter.relation != FilterRelation.Or)
				{
					continue;
				}
				if (IsExtract(filter))
				{
					Dictionary<int, FilterValue> values2 = source[filter.col].Where((KeyValuePair<int, FilterValue> t) => result.Contains(t.Key)).ToDictionary((KeyValuePair<int, FilterValue> t) => t.Key, (KeyValuePair<int, FilterValue> t) => t.Value);
					result = filter.Apply(values2);
				}
				else
				{
					List<int> second2 = filter.Apply(source[filter.col]);
					result = result.Union(second2).ToList();
				}
			}
		}
		ResultCount = result.Count;
		return result;
	}

	public string Serialize()
	{
#pragma warning disable SCS0028 // 需要多态序列化
		return JsonConvert.SerializeObject(filters, Formatting.Indented, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		});
#pragma warning restore SCS0028
	}

	public void Deserialize(string SerializeData)
	{
#pragma warning disable SCS0028 // 需要多态反序列化
		filters = JsonConvert.DeserializeObject<List<FilterBase>>(SerializeData, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		});
#pragma warning restore SCS0028
	}
}
