using Leqisoft.Model;

namespace Leqisoft.UI.Controls.CollectTable;

internal static class ex4
{
	public static decimal GetTotalValue(this SubsidiaryLedgerTotal slt, AnalysisProject ap)
	{
		return ap switch
		{
			AnalysisProject.Balance => slt.Balance, 
			AnalysisProject.Debits => slt.Debit, 
			AnalysisProject.Credits => slt.Credit, 
			_ => 0m, 
		};
	}
}
