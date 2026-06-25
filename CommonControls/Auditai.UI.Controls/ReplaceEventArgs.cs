namespace Auditai.UI.Controls;

public class ReplaceEventArgs : FindNextEventArgs
{
	public ReplaceMode ReplaceMode { get; set; }

	public string ReplaceValue { get; set; }

	public bool IsReplaceAll { get; set; }
}
