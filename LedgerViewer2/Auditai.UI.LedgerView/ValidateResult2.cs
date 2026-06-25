using C1.Win.C1FlexGrid;

namespace Auditai.UI.LedgerView;

public class ValidateResult2
{
	public string ErrorMessage { get; set; }

	public C1FlexGrid C1FlexGrid { get; set; }

	public Row Row { get; set; }

	public Column Column { get; set; }
}
