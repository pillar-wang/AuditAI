namespace Auditai.UI.Controls;

public class InvalidCollectSettingException : CollectException
{
	public InvalidCollectSettingException(string message)
		: base(message)
	{
	}
}
