using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class Account
{
	public const int ACCOUNT_DIRTY_ADD = 1;

	public const int ACCOUNT_DIRTY_MODIFY = 2;

	public const int ACCOUNT_DIRTY_DELETE = -1;

	public int Dirty { get; set; }

	public int Id { get; set; }

	public string Code { get; set; }

	public string Name { get; set; }

	public bool IsDebit { get; set; }

	public Account Parent { get; set; }

	public List<Account> Children { get; }

	public List<Account> Descendants => Children.Concat(Children.SelectMany((Account c) => c.Descendants)).ToList();

	public List<Account> DescendantsAndSelf => new Account[1] { this }.Concat(Children).Concat(Children.SelectMany((Account c) => c.DescendantsAndSelf)).ToList();

	public Ledger Ledger { get; set; }

	public IEnumerable<Account> Ancestors
	{
		get
		{
			if (Parent == null)
			{
				return Enumerable.Empty<Account>();
			}
			return new Account[1] { Parent }.Concat(Parent.Ancestors);
		}
	}

	public IEnumerable<Account> AncestorsAndSelf => new Account[1] { this }.Concat(Ancestors);

	public Account()
	{
		Children = new List<Account>();
	}

	public Account(Ledger ledger)
		: this()
	{
		Ledger = ledger;
	}

	public override string ToString()
	{
		return string.Format("{0} {1} {2} {3}", Code, Name, IsDebit ? "借" : "贷", Children.Count);
	}

	public string GetFullName()
	{
		Account account = this;
		string text = account.Name;
		while ((account = account.Parent) != null)
		{
			text = string.Join("-", account.Name, text);
		}
		return text;
	}

	public int GetLevel()
	{
		if (Parent != null)
		{
			return 1 + Parent.GetLevel();
		}
		return 0;
	}
}
