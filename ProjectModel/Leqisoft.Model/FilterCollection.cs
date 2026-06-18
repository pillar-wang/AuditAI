using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public class FilterCollection : IEnumerable<FilterBase>, IEnumerable
{
	private List<FilterBase> _filters = new List<FilterBase>();

	public int Count => _filters.Count;

	public void RemoveAll(Predicate<FilterBase> match)
	{
		_filters.RemoveAll(match);
	}

	public void Clear()
	{
		_filters.Clear();
	}

	public void AddRange(IEnumerable<FilterBase> collection)
	{
		_filters.AddRange(collection);
	}

	public void Add(FilterBase item)
	{
		_filters.Add(item);
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(_filters, FilterBase.SerializerSettings);
	}

	public void Deserialize(string s)
	{
		try
		{
			_filters = JsonConvert.DeserializeObject<List<FilterBase>>(s, FilterBase.SerializerSettings) ?? new List<FilterBase>();
		}
		catch
		{
			_filters = new List<FilterBase>();
		}
	}

	public IEnumerator<FilterBase> GetEnumerator()
	{
		return ((IEnumerable<FilterBase>)_filters).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_filters).GetEnumerator();
	}
}
