namespace Leqisoft.UI.LedgerView;

public class CannotMergeException : LedgerMergeException
{
	public CannotMergeException(string mess)
		: base(mess)
	{
	}
}
