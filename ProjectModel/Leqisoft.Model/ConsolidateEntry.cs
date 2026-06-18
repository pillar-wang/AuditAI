using System;
using System.Collections.Generic;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

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

}
