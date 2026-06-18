using System.Collections.Generic;
using Google.Protobuf;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class RowOwnerLoadShare
{
	private Dictionary<long, HashSet<long>> _dic = new Dictionary<long, HashSet<long>>();

	public void Deserialize(byte[] bin)
	{
		Leqisoft.DTO.RowOwnerLoadShare rowOwnerLoadShare = Leqisoft.DTO.RowOwnerLoadShare.Parser.ParseFrom(bin);
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
		Leqisoft.DTO.RowOwnerLoadShare rowOwnerLoadShare = new Leqisoft.DTO.RowOwnerLoadShare();
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
