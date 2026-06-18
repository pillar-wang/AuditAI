using System;

namespace Leqisoft.UI.Controls;

public class FindNextEventArgs : EventArgs
{
	public string FindValue { get; set; }

	public MatchMode MatchMode { get; set; }

	public bool IsMatchCase { get; set; }

	public ScopeMode ScopeMode { get; set; }
}
