using System.Collections.Generic;
using Newtonsoft.Json;

namespace Auditai.Model;

[JsonObject]
public class TicketTableMixRangeTemplateRow
{
	public int TemplateId { get; set; }

	public int RefTicketTableRowIndex { get; set; }

	public int BottomBorderRefTicketTableRowIndex { get; set; }

	public int DataGroupKeyId { get; set; } = -1;


	public List<long> TicketColumnIdList { get; set; }

	public List<TicketMerge> Merges { get; set; }

	public int GetTicketTableRowsCount()
	{
		return BottomBorderRefTicketTableRowIndex - RefTicketTableRowIndex + 1;
	}
}
