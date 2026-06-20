﻿using System;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public class ConsolidateAttributes
{
	public ConsolidateRole Role { get; set; }

	public Guid ProjectId { get; set; }

	public Id64 TableId { get; set; }

	public Id64 ColumnId { get; set; }

	/// <summary>持股比例（百分比）</summary>
	public decimal OwnershipRatio { get; set; } = 100m;

	public bool ShouldSerializeProjectId()
	{
		return Role == ConsolidateRole.Data;
	}

	public bool ShouldSerializeTableId()
	{
		return Role == ConsolidateRole.Data;
	}

	public bool ShouldSerializeColumnId()
	{
		return Role == ConsolidateRole.Data;
	}

	public bool ShouldSerializeOwnershipRatio()
	{
		return Role == ConsolidateRole.Data;
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	public static ConsolidateAttributes Deserialize(string s)
	{
		if (!string.IsNullOrWhiteSpace(s))
		{
			return JsonConvert.DeserializeObject<ConsolidateAttributes>(s);
		}
		return null;
	}
}
