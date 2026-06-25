using System.Collections.Generic;
using Auditai.Model;
using Auditai.UI.Controls.SmartCollector;

namespace Auditai.UI.LedgerView;

public class ValidateAccount
{
	private CellCollector cellCollector;

	public ValidateAccount(CellCollector cellCollector)
	{
		this.cellCollector = cellCollector;
	}

	public List<Account> Validate(Ledger ledger)
	{
		List<Account> list = new List<Account>();
		foreach (Account rootAccount in ledger.RootAccounts)
		{
			if (!cellCollector.ContainAccount(rootAccount.Name))
			{
				list.Add(rootAccount);
			}
		}
		return list;
	}

	public bool Validate(Account account)
	{
		return cellCollector.ContainAccount(account.Name);
	}
}
