using Auditai.Model;

namespace Auditai.UI.Controls.CollectTable;

public class CollectItemShouldSelectFilter_ExcludeEmptyAccount : CollectItemShouldSelectFilter
{
	protected TableCollectorAbstract _collector;

	protected CollectItemShouldSelectFilter _checkAccountShouldSelectFilter;

	public CollectItemShouldSelectFilter_ExcludeEmptyAccount(TableCollectorAbstract collector)
	{
		_collector = collector;
	}

	public CollectItemShouldSelectFilter_ExcludeEmptyAccount(TableCollectorAbstract collector, CollectItemShouldSelectFilter accountShouldSelectFilter)
	{
		_collector = collector;
		_checkAccountShouldSelectFilter = accountShouldSelectFilter;
	}

	public override bool IsAccountShouldBeSelected(Account account)
	{
		if (_checkAccountShouldSelectFilter != null && !_checkAccountShouldSelectFilter.IsAccountShouldBeSelected(account))
		{
			return false;
		}
		if (_collector.IsEmptyAccountWithCache(account))
		{
			return false;
		}
		return true;
	}
}
