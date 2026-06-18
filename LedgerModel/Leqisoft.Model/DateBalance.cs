using System;
using System.Collections.Generic;

namespace Leqisoft.Model;

public class DateBalance : Dictionary<Account, AccountBalance>, ICloneable
{
	internal DateBalance()
	{
	}

	internal DateBalance(IDictionary<Account, AccountBalance> dic)
		: base(dic)
	{
	}

	public object Clone()
	{
		DateBalance dateBalance = new DateBalance();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Account, AccountBalance> current = enumerator.Current;
			dateBalance.Add(current.Key, (AccountBalance)current.Value.Clone());
		}
		return dateBalance;
	}

	public DateBalance OnlyCloneKey()
	{
		DateBalance dateBalance = new DateBalance();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<Account, AccountBalance> current = enumerator.Current;
			dateBalance.Add(current.Key, current.Value.OnlyCloneKey());
		}
		return dateBalance;
	}

	public decimal Get(Account account, AuxiliaryItem item)
	{
		if (!TryGetValue(account, out var value))
		{
			return 0m;
		}
		if (!value.ClassBalances.TryGetValue(item.Class, out var value2))
		{
			return 0m;
		}
		if (!value2.ItemBalances.TryGetValue(item, out var value3))
		{
			return 0m;
		}
		return value3;
	}
}
