using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class TicketInputRowVM
{
	public Row TableRow;

	public TicketTableMixRangeTemplateRow MixTicketDynamicDataRowTemplate;

	public TicketRow TicketRow { get; set; }

	public bool IsNew { get; set; }

	public Row TempRow { get; set; }

	public bool IsDynamicRowTicketDataRow { get; set; }

	public bool IsMixTicketDynamicDataRow { get; set; }

	public bool IsMixTicketFixedDataRow { get; set; }

	public bool IsMixTicketExistWriteTicketFormulaResultToTableRow { get; set; }
}
