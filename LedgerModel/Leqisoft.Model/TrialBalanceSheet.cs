namespace Leqisoft.Model;

public class TrialBalanceSheet
{
	public DateBalance Start { get; internal set; }

	public DateBalance Debit { get; internal set; }

	public DateBalance Credit { get; internal set; }

	public DateBalance End { get; internal set; }

	internal TrialBalanceSheet()
	{
	}
}
