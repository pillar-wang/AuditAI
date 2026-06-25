using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Auditai.UI.LedgerView;

[JsonObject("GridStyle")]
public class GridStyle
{
	[JsonProperty("Name")]
	public string Name { get; set; }

	[JsonProperty("ColStyleCollection")]
	public List<ColStyle> ColStyleCollection { get; set; }

	public ColStyle this[string name] => ColStyleCollection.FirstOrDefault((ColStyle t) => t.Name == name);

	[JsonProperty("ColOrder")]
	public List<string> ColOrder { get; set; }

	public GridStyle(string name)
	{
		Name = name;
		ColStyleCollection = new List<ColStyle>();
		ColOrder = new List<string>();
	}
}
