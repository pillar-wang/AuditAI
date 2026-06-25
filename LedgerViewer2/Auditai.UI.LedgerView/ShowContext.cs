using System;

namespace Auditai.UI.LedgerView;

public class ShowContext
{
	public string Type { get; private set; }

	public DateTime Date { get; private set; }

	public ShowContext(string type, DateTime date)
	{
		Type = type;
		Date = date;
	}
}
