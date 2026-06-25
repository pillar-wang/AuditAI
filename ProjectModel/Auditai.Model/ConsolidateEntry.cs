using System;
using System.Collections.Generic;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

[Serializable]
public class ConsolidateEntry
{
	public Guid ProjectId { get; set; }

	[JsonIgnore]
	public Project Project { get; set; }

	public Id64 TableId { get; set; }

	[JsonIgnore]
	public Table Table { get; set; }

	public List<Id64> GroupSrcId { get; set; }

	[JsonIgnore]
	public List<Column> GroupSrc { get; set; }

	public List<Id64> DataSrcId { get; set; }

	[JsonIgnore]
	public List<Column> DataSrc { get; set; }

	public bool Selected { get; set; } = true;

	/// <summary>持股比例（百分比，默认100%）</summary>
	public decimal OwnershipRatio { get; set; } = 100m;

	/// <summary>合并层级（默认1，数字越小越先合并）</summary>
	public int Level { get; set; } = 1;

	/// <summary>内部交易标识列ID列表</summary>
	public List<Id64> IntercompanyCols { get; set; } = new List<Id64>();

}
