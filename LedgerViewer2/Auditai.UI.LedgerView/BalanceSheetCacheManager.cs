using System;
using System.Collections.Generic;
using System.Threading;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

public class BalanceSheetCacheManager
{
	private Dictionary<DateTime, Dictionary<DateTime, TrialBalanceCache>> cached = new Dictionary<DateTime, Dictionary<DateTime, TrialBalanceCache>>();

	private bool cacheFinished;

	private Ledger _ledger;

	public void Cache(Ledger ledger, DateTime baseDate)
	{
		new Thread((ThreadStart)delegate
		{
			cacheFinished = false;
			try
			{
				cached = new Dictionary<DateTime, Dictionary<DateTime, TrialBalanceCache>>();
				_ledger = ledger;
				DateTime startDate = ledger.StartDate;
				for (int num = ledger.GetEndDate().Year; num > startDate.Year; num--)
				{
					DateTime dateTime = baseDate.CopyToSpecificYear(num);
					DateTime dateTime2 = baseDate.CopyToSpecificYear(num - 1).AddDays(1.0);
					if (dateTime2 >= startDate)
					{
						TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(dateTime2, dateTime);
						cached.Add(dateTime2, new Dictionary<DateTime, TrialBalanceCache>());
						TrialBalanceCache trialBalanceCache = new TrialBalanceCache();
						foreach (KeyValuePair<Account, AccountBalance> item in trialBalanceSheet.Start)
						{
							Account key = item.Key;
							trialBalanceCache.Add(key, Tuple.Create(item.Value.Total, trialBalanceSheet.Debit[key].Total, trialBalanceSheet.Credit[key].Total));
						}
						cached[dateTime2].Add(dateTime, trialBalanceCache);
					}
				}
			}
			catch (Exception exception)
			{
				exception.Log();
				cacheFinished = false;
				return;
			}
			cacheFinished = true;
		}).Start();
	}

	public TrialBalanceCache Get(Ledger ledger, DateTime start, DateTime end)
	{
		if (!cacheFinished)
		{
			return null;
		}
		if (ledger != _ledger)
		{
			return null;
		}
		if (cached.ContainsKey(start))
		{
			Dictionary<DateTime, TrialBalanceCache> dictionary = cached[start];
			if (dictionary.ContainsKey(end))
			{
				return dictionary[end];
			}
		}
		return null;
	}
}
