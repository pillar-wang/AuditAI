using System.Collections.Generic;

namespace Leqisoft.Model;

public class Account
{
	public string Code { get; set; }

	public string Name { get; set; }

	public bool IsDebit { get; set; }

	public decimal Balance { get; set; }

	public Dictionary<Currency, ForeignRecord> Foreign { get; set; } = new Dictionary<Currency, ForeignRecord>();


	public Dictionary<Item, ItemBalance> ItemBalance { get; set; } = new Dictionary<Item, ItemBalance>();


	public double Quantity { get; set; }

	public double UnitPrice { get; set; }

	public Account Parent { get; set; }

	public List<Account> Children { get; set; } = new List<Account>();


	public bool IsAncestorOf(Account account)
	{
		if (account.Parent == null)
		{
			return false;
		}
		if (account.Parent == this)
		{
			return true;
		}
		return IsAncestorOf(account.Parent);
	}

	public bool IsAncestorOfOrSelf(Account account)
	{
		if (account != this)
		{
			return IsAncestorOf(account);
		}
		return true;
	}

	public override string ToString()
	{
		return Code + " " + Name;
	}
}
