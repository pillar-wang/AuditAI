using System.Collections.Generic;
using Auditai.Model;
using Auditai.UI.Controls;
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
			if (!IsStandardAccount(rootAccount))
			{
				list.Add(rootAccount);
			}
		}
		return list;
	}

	public bool Validate(Account account)
	{
		return IsStandardAccount(account);
	}

	/// <summary>
	/// 标准科目判定：
	/// 1. 标准字典 code 命中 → 标准；
	/// 2. 未命中 → 回退采账别名（CellCollectDic）名称匹配；
	/// 3. 仍不命中 → 非标准。
	/// 标准字典 code 集合为空（未加载/加载失败）时保持原有名称匹配行为。
	/// </summary>
	private bool IsStandardAccount(Account account)
	{
		// 1. 标准字典 code 命中 → 标准
		HashSet<string> standardCodes = DictionarySync.StandardAccountCodes;
		if (standardCodes != null && standardCodes.Count > 0 && !string.IsNullOrWhiteSpace(account.Code) && standardCodes.Contains(account.Code.Trim()))
		{
			return true;
		}
		// 2. code 未命中 → 回退采账别名正则匹配科目名称；3. 仍不命中 → 非标准
		return cellCollector.ContainAccount(account.Name);
	}
}
