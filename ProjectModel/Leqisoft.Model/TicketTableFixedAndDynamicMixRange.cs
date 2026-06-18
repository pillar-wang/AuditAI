using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public class TicketTableFixedAndDynamicMixRange
{
	public List<TicketTableMixRangeDataGroupKey> DataGroupKeyListForFixedDataRow { get; set; }

	public List<TicketTableMixRangeDataGroupKey> DataGroupKeyListForDynamicDataRow { get; set; }

	public List<TicketTableMixRangeTemplateRow> DynamicDataRowTemplateRows { get; set; }
}
