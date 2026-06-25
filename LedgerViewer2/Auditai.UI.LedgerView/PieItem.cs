using System.Collections.Generic;
using System.Reflection;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

[Obfuscation(Exclude = true)]
public class PieItem
{
	public string Name { get; set; }

	public decimal Value { get; set; }

	public string Display { get; set; }

	public Account Account { get; set; }

	public List<PieItem> ChildItems { get; set; }

	public PieItem()
	{
		ChildItems = new List<PieItem>();
		Account = new Account();
	}
}
