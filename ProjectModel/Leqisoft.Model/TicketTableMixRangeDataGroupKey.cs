using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public class TicketTableMixRangeDataGroupKey
{
	public int KeyId { get; set; }

	public int TickeRowIndex { get; set; }

	public List<TicketTableMixRangeDataGroupKeyItem> KeyItems { get; set; } = new List<TicketTableMixRangeDataGroupKeyItem>();


	public override int GetHashCode()
	{
		if (KeyItems == null || KeyItems.Count == 0)
		{
			return 0;
		}
		return KeyItems[0].GetHashCode();
	}
}
