﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

/// <summary>
/// 跨项目数据合并模式
/// </summary>
public enum MergeMode
{
	/// <summary>汇总模式：按分组列合并相同键值的行，数据列求和</summary>
	Aggregate,
	/// <summary>追加模式：直接追加所有行，不合并</summary>
	Append
}

[Serializable]
public class ConsolidateSettings
{
	public List<ConsolidateEntry> Sources { get; set; } = new List<ConsolidateEntry>();


	public List<Id64> GroupDestId { get; set; } = new List<Id64>();


	[JsonIgnore]
	public List<Column> GroupDest { get; set; }

	public List<Id64> AggregateDestId { get; set; } = new List<Id64>();


	[JsonIgnore]
	public List<Column> AggregateDest { get; set; }

	public MergeMode Mode { get; set; } = MergeMode.Aggregate;

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	public static ConsolidateSettings Deserialize(string s)
	{
		return JsonConvert.DeserializeObject<ConsolidateSettings>(s);
	}
}
