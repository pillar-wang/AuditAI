using System.Linq;

namespace Auditai.UI.LedgerView;

public class AgeSheet
{
	public int MinYear => AgeAmountSheet.Keys.Min();

	public int MaxYear => AgeAmountSheet.Keys.Max();

	public AgeAmountSheet AgeAmountSheet { get; set; }

	public AgeAmount AgeBegin { get; set; }

	public bool IsDebit { get; set; } = true;


	public AgeSheet()
	{
		AgeBegin = new AgeAmount();
		AgeAmountSheet = new AgeAmountSheet();
	}
}
