using System.Collections.Generic;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Controls.CollectDic;

namespace Auditai.UI.LedgerView;

public class AccountValidate : AbstractValidate<Account>
{
	protected const string CN_AP_NAME = "accountPositiveName";

	protected const string CN_AP_TIP = "accountPositiveTip";

	protected const string CN_AR_NAME = "accountReverseName";

	protected const string CN_AR_TIP = "accountReverseTip";

	public AccountValidate(IEnumerable<Account> accounts)
	{
		_validateItems = accounts;
	}

	protected override bool TryValidate(Account account, out ValidateResult result)
	{
		result = null;
		ERow eRow = DictionarySync.LedgerValidator.AccountPositive.FindRow("accountPositiveName", (ECell c) => c.AnyEquals(account.Name));
		if (eRow != null)
		{
			result = new ValidateResult
			{
				Key = account,
				Tip = eRow["accountPositiveTip"].Value
			};
			return true;
		}
		ERow eRow2 = DictionarySync.LedgerValidator.AccountReverse.FindRow("accountReverseName", (ECell c) => c.AnyMatch(account.Name));
		if (eRow2 == null)
		{
			result = new ValidateResult
			{
				Key = account,
				Tip = DictionarySync.LedgerValidator.AccountReverse.Rows[0]["accountReverseTip"].Value
			};
			return true;
		}
		return false;
	}
}
