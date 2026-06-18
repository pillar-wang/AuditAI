using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls.CollectTable;

public class CollectItemShouldSelectFilter
{
	protected List<string> _accountNamePatternList;

	protected HashSet<Voucher> _vouchersSet;

	protected HashSet<Account> _accountSet;

	protected CollectItemShouldSelectFilter()
	{
	}

	public CollectItemShouldSelectFilter(List<string> accountNamePatternList)
	{
		_accountNamePatternList = accountNamePatternList;
	}

	public CollectItemShouldSelectFilter(HashSet<Account> accountSet)
	{
		_accountSet = accountSet;
	}

	public CollectItemShouldSelectFilter(List<Voucher> voucherList)
	{
		_vouchersSet = new HashSet<Voucher>(voucherList);
	}

	public virtual bool IsAccountShouldBeSelected(Account account)
	{
		if (_accountNamePatternList != null)
		{
			string name = account.Name.Trim();
			return _accountNamePatternList.Any((string e) => Regex.IsMatch(name, e));
		}
		if (_accountSet != null)
		{
			return _accountSet.Contains(account);
		}
		return false;
	}

	public virtual bool IsVoucherShouldBeSelected(Voucher voucher)
	{
		if (_vouchersSet == null)
		{
			return false;
		}
		return _vouchersSet.Contains(voucher);
	}
}
