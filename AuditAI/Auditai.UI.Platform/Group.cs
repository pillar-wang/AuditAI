using System.Collections.Generic;
using System.Linq;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class Group : MemTab
{
	private Dictionary<string, Member> memMap = new Dictionary<string, Member>();

	public bool Exists(string id)
	{
		return memMap.ContainsKey(id);
	}

	public bool Add(Member member)
	{
		if (memMap.ContainsKey(member.Id))
		{
			return false;
		}
		memMap.Add(member.Id, member);
		return true;
	}

	public bool Remove(string id)
	{
		if (memMap.ContainsKey(id))
		{
			memMap.Remove(id);
			return true;
		}
		return false;
	}

	public void Clear()
	{
		memMap.Clear();
	}

	public IEnumerable<Member> Members()
	{
		return memMap.Values;
	}

	public IEnumerable<MemTab> GetSelfAndMembers()
	{
		List<MemTab> list = new List<MemTab>();
		list.Add(this);
		list.AddRange(memMap.Values.Where((Member m) => m.Id != User.Current.Id.ToString()));
		return list;
	}
}
