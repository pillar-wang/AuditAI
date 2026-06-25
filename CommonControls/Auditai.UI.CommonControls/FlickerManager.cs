using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.UI.CommonControls;

public class FlickerManager
{
	private Dictionary<object, AbstractFlickerProxy> _maps;

	public FlickerManager()
	{
		_maps = new Dictionary<object, AbstractFlickerProxy>();
	}

	public int Count()
	{
		return _maps.Count;
	}

	public bool Add(object key, AbstractFlickerProxy twinkleProxy)
	{
		if (key == null)
		{
			return false;
		}
		if (_maps.ContainsKey(key))
		{
			_maps[key].Stop();
			_maps.Remove(key);
		}
		_maps.Add(key, twinkleProxy);
		return true;
	}

	public bool Remove(object key)
	{
		if (key == null)
		{
			return false;
		}
		if (_maps.ContainsKey(key))
		{
			_maps[key].Stop();
			_maps.Remove(key);
			return true;
		}
		return false;
	}

	public void Clear()
	{
		foreach (KeyValuePair<object, AbstractFlickerProxy> map in _maps)
		{
			map.Value.Stop();
		}
		_maps.Clear();
	}

	public void Dispose()
	{
		foreach (KeyValuePair<object, AbstractFlickerProxy> map in _maps)
		{
			map.Value.Stop();
			map.Value.Dispose();
		}
		_maps.Clear();
	}

	public bool Start(object key)
	{
		if (key == null)
		{
			return false;
		}
		if (_maps.ContainsKey(key))
		{
			_maps[key].Start();
			return true;
		}
		return false;
	}

	public bool Stop(object key)
	{
		if (key == null)
		{
			return false;
		}
		if (_maps.ContainsKey(key))
		{
			_maps[key].Stop();
			return true;
		}
		return false;
	}

	public AbstractFlickerProxy Get(object key)
	{
		if (key == null)
		{
			return null;
		}
		if (_maps.ContainsKey(key))
		{
			return _maps[key];
		}
		return null;
	}

	public bool Any(Predicate<AbstractFlickerProxy> predicate)
	{
		return _maps.Values.Any((AbstractFlickerProxy m) => predicate(m));
	}

	public bool Contains(object key)
	{
		return _maps.ContainsKey(key);
	}
}
