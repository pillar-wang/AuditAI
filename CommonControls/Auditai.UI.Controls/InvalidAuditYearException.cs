namespace Auditai.UI.Controls;

public class InvalidAuditYearException : CollectException
{
	public InvalidAuditYearException(string message)
		: base(message)
	{
	}
}
