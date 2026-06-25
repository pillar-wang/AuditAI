using System.Collections.Generic;

namespace Auditai.Model;

public class ReceivableAgeSheet
{
	public int YearCount { get; set; }

	public Dictionary<Account, ReceivableAgeEntry> Entries { get; } = new Dictionary<Account, ReceivableAgeEntry>();

}
