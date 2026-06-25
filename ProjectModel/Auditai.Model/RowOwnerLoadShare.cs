using System.Collections.Generic;
using Google.Protobuf;
using Auditai.DTO;

namespace Auditai.Model;

public class RowOwnerLoadShare
{
	private Dictionary<long, HashSet<long>> _dic = new Dictionary<long, HashSet<long>>();

	public void Deserialize(byte[] bin)
	{
		Auditai.DTO.RowOwnerLoadShare rowOwnerLoadShare = Auditai.DTO.RowOwnerLoadShare.Parser.ParseFrom(bin);
		_dic.Clear();
		foreach (RowOwnerLoadShareEntry entry in rowOwnerLoadShare.Entries)
		{
			HashSet<long> hashSet = new HashSet<long>();
			_dic.Add(entry.Creator, hashSet);
			foreach (long item in entry.Shared)
			{
				hashSet.Add(item);
			}
		}
	}

	public byte[] Serialize()
	{
		Auditai.DTO.RowOwnerLoadShare rowOwnerLoadShare = new Auditai.DTO.RowOwnerLoadShare();
		foreach (KeyValuePair<long, HashSet<long>> item in _dic)
		{
			RowOwnerLoadShareEntry rowOwnerLoadShareEntry = new RowOwnerLoadShareEntry();
			rowOwnerLoadShareEntry.Creator = item.Key;
			rowOwnerLoadShareEntry.Shared.AddRange(item.Value);
			rowOwnerLoadShare.Entries.Add(rowOwnerLoadShareEntry);
		}
		return rowOwnerLoadShare.ToByteArray();
	}

	public void Add(long creator, long shared)
	{
		if (!_dic.TryGetValue(creator, out var value))
		{
			value = new HashSet<long>();
			_dic.Add(creator, value);
		}
		value.Add(shared);
	}

	public void Remove(long creator, long shared)
	{
		if (_dic.TryGetValue(creator, out var value))
		{
			value.Remove(shared);
		}
	}

	public bool Exists(long creator, long shared)
	{
		if (_dic.TryGetValue(creator, out var value))
		{
			return value.Contains(shared);
		}
		return false;
	}
}
