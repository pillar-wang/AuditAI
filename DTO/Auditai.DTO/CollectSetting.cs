using Newtonsoft.Json;

namespace Auditai.DTO;

public class CollectSetting
{
	[JsonProperty(PropertyName = "DefaultExtract")]
	public double DefaultExtract { get; set; } = 0.1;


	[JsonProperty(PropertyName = "MaxExtract")]
	public int MaxExtract { get; set; } = 80;


	[JsonProperty(PropertyName = "MinExtract")]
	public int MinExtract { get; set; } = 10;

}
