using System.Collections.Generic;
using System.Data;
using System.Linq;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.CollectDic;

namespace Leqisoft.UI.LedgerView;

public class BalanceValidate : AbstractValidate<Account>
{
	protected const string CN_BA_INDEX = "balanceIndex";

	protected const string CN_BA_NAME = "balanceName";

	protected const string CN_BA_COMP = "balanceCompare";

	protected const string CN_BA_TIP = "BalanceTip";

	private DateBalance dateBalance;

	private LedgerViewer _owner;

	public BalanceValidate(LedgerViewer owner, IEnumerable<Account> accounts)
	{
		_owner = owner;
		_validateItems = accounts;
		Account account = accounts.FirstOrDefault();
		if (account != null)
		{
			Ledger ledger = account.Ledger;
			dateBalance = _owner.CacheManager.GetTrialBalanceSheetWithCache(ledger).End;
		}
	}

	protected override bool TryValidate(Account account, out ValidateResult result)
	{
		result = null;
		ERow eRow = DictionarySync.LedgerValidator.BalanceValidate.FindRow("balanceName", (ECell c) => c.AnyEquals(account.Name));
		if (eRow == null)
		{
			return false;
		}
		decimal total = dateBalance[account].Total;
		string value = eRow["balanceCompare"].Value;
		string expression = $"{total}{value}";
		string value2 = new DataTable().Compute(expression, null).ToString();
		if (bool.TryParse(value2, out var result2) && result2)
		{
			result = new ValidateResult
			{
				Key = account,
				Tip = eRow["BalanceTip"].Value
			};
			return true;
		}
		return false;
	}
}
