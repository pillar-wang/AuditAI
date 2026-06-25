namespace Auditai.UI.LedgerView;

public class CannotMergeException : LedgerMergeException
{
	public CannotMergeException(string mess)
		: base(mess)
	{
	}
}
