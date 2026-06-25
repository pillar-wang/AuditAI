using System.Collections.Generic;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public static class TreeNodeStateCache
{
	private static Dictionary<Id64, TreeNodeCacheState> _cache = new Dictionary<Id64, TreeNodeCacheState>();

	public static void Set(Id64 id, TreeNodeCacheState value)
	{
		_cache[id] = value;
	}

	public static bool Contains(Id64 id)
	{
		return _cache.ContainsKey(id);
	}

	public static TreeNodeCacheState Get(Id64 id)
	{
		return _cache[id];
	}
}
