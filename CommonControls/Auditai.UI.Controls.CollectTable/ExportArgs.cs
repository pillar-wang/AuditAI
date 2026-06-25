using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Auditai.UI.Controls.CollectTable;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
[JsonObject("ExportArgs")]
public class ExportArgs
{
	[JsonProperty(PropertyName = "CollectAllAccount")]
	public bool CollectAllCount;

	[JsonProperty(PropertyName = "FillTargetType")]
	public CollectFillTargetType FillTargetType;

	[JsonProperty(PropertyName = "IsOnlyMyMark")]
	public bool IsOnlyMyMark;

	[JsonProperty(PropertyName = "ExportObject")]
	public CollectObjectEnum CollectObject { get; set; }

	[JsonProperty(PropertyName = "MonthStart")]
	public int MonthStart { get; set; }

	[JsonProperty(PropertyName = "MonthEnd")]
	public int MonthEnd { get; set; }

	[JsonProperty(PropertyName = "AccountName")]
	public string AccountName { get; set; }

	[JsonProperty(PropertyName = "AuxName")]
	public string AuxName { get; set; }

	[JsonProperty(PropertyName = "Mapping")]
	public Dictionary<long, string> Mapping { get; set; }

	[JsonProperty(PropertyName = "Analysis")]
	public AnalysisProject Analysis { get; set; }

	public ExportArgs()
	{
		Mapping = new Dictionary<long, string>();
	}

	public static ExportArgs Parse(string tableCollectSettings)
	{
		return JsonConvert.DeserializeObject<ExportArgs>(tableCollectSettings);
	}
}
