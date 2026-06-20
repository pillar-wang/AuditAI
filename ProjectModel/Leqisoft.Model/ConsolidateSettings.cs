﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

/// <summary>
/// 合并方式
/// </summary>
public enum MergeMode
{
	/// <summary>分组汇总合并：按合并维度合并相同键值的行，合并金额求和</summary>
	Aggregate,
	/// <summary>逐行追加合并：直接追加所有行，不合并</summary>
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

	/// <summary>合并报表名称</summary>
	public string ConsolidationName { get; set; } = string.Empty;

	/// <summary>是否显示工作底稿明细</summary>
	public bool ShowDetail { get; set; } = true;

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	public static ConsolidateSettings Deserialize(string s)
	{
		return JsonConvert.DeserializeObject<ConsolidateSettings>(s);
	}
}
