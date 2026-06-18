using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public class TicketTableMixRangeDataGroupKeyItem
{
	public Id64 TableColumnId { get; set; }

	public string TableColumnValue { get; set; }

	public override bool Equals(object obj)
	{
		if (obj is TicketTableMixRangeDataGroupKeyItem ticketTableMixRangeDataGroupKeyItem)
		{
			if (TableColumnId != ticketTableMixRangeDataGroupKeyItem.TableColumnId)
			{
				return false;
			}
			if (TableColumnValue != ticketTableMixRangeDataGroupKeyItem.TableColumnValue)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return TableColumnId.GetHashCode() | ((TableColumnValue != null) ? TableColumnValue.GetHashCode() : 0);
	}
}
