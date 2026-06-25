using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public class LedgerCacheManager
{
	private bool _cacheValid = true;

	private bool _cacheTrialBalanceSheet1Valid = true;

	private Tuple<Ledger, TrialBalanceSheet> _cacheTrialBalanceSheet1;

	private bool _cacheTrialBalanceSheet2Valid = true;

	private Tuple<Ledger, DateTime, DateTime, TrialBalanceSheet> _cacheTrialBalanceSheet2;

	private bool _cacheSubsidiaryLedger1Valid = true;

	private Tuple<Account, AuxiliaryClass, DateTime, DateTime, Dictionary<object, SubsidiaryLedger>> _cacheSubsidiaryLedger1;

	private bool _cacheSubsidiaryLedger2Valid = true;

	private Dictionary<Account, Tuple<DateTime, DateTime, Dictionary<object, SubsidiaryLedger>>> _cacheSubsidiaryLedger2;

	private bool _cacheNotEmptyAccountSetValid = true;

	private Tuple<Ledger, HashSet<Account>> _cacheNotEmptyAccountSet;

	private bool _cacheNotEmptyAuxiliaryItemSetValid = true;

	private Tuple<Ledger, HashSet<Tuple<Account, AuxiliaryItem>>> _cacheNotEmptyAuxiliaryItemSet;

	public bool CacheValid
	{
		get
		{
			return _cacheValid;
		}
		set
		{
			_cacheValid = value;
			if (!_cacheValid)
			{
				_cacheTrialBalanceSheet1Valid = false;
				_cacheTrialBalanceSheet2Valid = false;
				_cacheNotEmptyAccountSetValid = false;
				_cacheSubsidiaryLedger1Valid = false;
				_cacheSubsidiaryLedger2Valid = false;
				_cacheNotEmptyAuxiliaryItemSetValid = false;
				_cacheTrialBalanceSheet1 = null;
				_cacheTrialBalanceSheet2 = null;
				_cacheNotEmptyAccountSet = null;
				_cacheSubsidiaryLedger1 = null;
				_cacheSubsidiaryLedger2 = null;
				_cacheNotEmptyAuxiliaryItemSet = null;
			}
		}
	}

	public TrialBalanceSheet GetTrialBalanceSheetWithCache(Ledger ledger, bool useCache = true)
	{
		if (useCache && _cacheTrialBalanceSheet1Valid && _cacheTrialBalanceSheet1 != null && _cacheTrialBalanceSheet1.Item1 == ledger)
		{
			return _cacheTrialBalanceSheet1.Item2;
		}
		DateTime endDate = ledger.GetEndDate();
		TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(ledger.StartDate, endDate);
		_cacheTrialBalanceSheet1 = Tuple.Create(ledger, trialBalanceSheet);
		_cacheTrialBalanceSheet1Valid = true;
		if (_cacheTrialBalanceSheet2 == null || _cacheTrialBalanceSheet2.Item1 != _cacheTrialBalanceSheet1.Item1)
		{
			_cacheTrialBalanceSheet2 = Tuple.Create(ledger, ledger.StartDate, endDate, trialBalanceSheet);
			_cacheTrialBalanceSheet2Valid = true;
		}
		return trialBalanceSheet;
	}

	public TrialBalanceSheet GetTrialBalanceSheetWithCache(Ledger ledger, DateTime start, DateTime end, bool useCache = true)
	{
		if (useCache && _cacheTrialBalanceSheet2Valid)
		{
			if (_cacheTrialBalanceSheet2 != null && _cacheTrialBalanceSheet2.Item1 == ledger && _cacheTrialBalanceSheet2.Item2.Equals(start) && _cacheTrialBalanceSheet2.Item3.Equals(end))
			{
				return _cacheTrialBalanceSheet2.Item4;
			}
			DateTime endDate = _cacheTrialBalanceSheet1.Item1.GetEndDate();
			if (_cacheTrialBalanceSheet2 != null && _cacheTrialBalanceSheet1 != null && _cacheTrialBalanceSheet2.Item1 == _cacheTrialBalanceSheet1.Item1 && start.Equals(_cacheTrialBalanceSheet1.Item1.StartDate) && end.Equals(endDate))
			{
				_cacheTrialBalanceSheet2 = Tuple.Create(ledger, start, end, _cacheTrialBalanceSheet1.Item2);
				_cacheTrialBalanceSheet2Valid = true;
				return _cacheTrialBalanceSheet2.Item4;
			}
		}
		TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(start, end);
		_cacheTrialBalanceSheet2 = Tuple.Create(ledger, start, end, trialBalanceSheet);
		_cacheTrialBalanceSheet2Valid = true;
		DateTime endDate2 = ledger.GetEndDate();
		if (start.Equals(ledger.StartDate) && end.Equals(endDate2))
		{
			_cacheTrialBalanceSheet1 = Tuple.Create(ledger, trialBalanceSheet);
			_cacheTrialBalanceSheet1Valid = true;
		}
		return _cacheTrialBalanceSheet2.Item4;
	}

	internal AccountBalance GetAccountBalanceWithCache(Ledger ledger, Account account)
	{
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(ledger);
		return trialBalanceSheetWithCache.End[account];
	}

	internal SubsidiaryLedger GetSubsidiaryLedgerWithCache(Account account, DateTime start, DateTime end, AuxiliaryClass auxliaryClass)
	{
		bool flag = _cacheSubsidiaryLedger1Valid && _cacheSubsidiaryLedger1 != null && _cacheSubsidiaryLedger1.Item1 == account && _cacheSubsidiaryLedger1.Item2 == auxliaryClass;
		bool flag2 = _cacheSubsidiaryLedger1 != null && _cacheSubsidiaryLedger1.Item3.Equals(start) && _cacheSubsidiaryLedger1.Item4.Equals(end);
		if (flag && flag2)
		{
			if (_cacheSubsidiaryLedger1.Item5.ContainsKey(auxliaryClass))
			{
				return _cacheSubsidiaryLedger1.Item5[auxliaryClass];
			}
			return account.Ledger.GetSubsidiaryLedger(account, start, end, auxliaryClass);
		}
		_cacheSubsidiaryLedger1 = Tuple.Create(account, auxliaryClass, start, end, account.Ledger.GetAuxSubsidiaryLedgerDic(account, start, end, (Voucher v) => v.Details.Any((AuxiliaryItem d) => d.Class == auxliaryClass)));
		_cacheSubsidiaryLedger1Valid = true;
		if (_cacheSubsidiaryLedger1.Item5.ContainsKey(auxliaryClass))
		{
			return _cacheSubsidiaryLedger1.Item5[auxliaryClass];
		}
		return account.Ledger.GetSubsidiaryLedger(account, start, end, auxliaryClass);
	}

	internal SubsidiaryLedger GetSubsidiaryLedgerWithCache(Account account, DateTime start, DateTime end, AuxiliaryItem auxiliaryItem)
	{
		if (_cacheSubsidiaryLedger2Valid && _cacheSubsidiaryLedger2 != null && _cacheSubsidiaryLedger2.ContainsKey(account) && end.Equals(_cacheSubsidiaryLedger2[account].Item2) && start.Equals(_cacheSubsidiaryLedger2[account].Item1))
		{
			if (_cacheSubsidiaryLedger2[account].Item3.ContainsKey(auxiliaryItem))
			{
				return _cacheSubsidiaryLedger2[account].Item3[auxiliaryItem];
			}
			return account.Ledger.GetEmptySubsidiaryLedger(account, start, end, auxiliaryItem);
		}
		_cacheSubsidiaryLedger2 = account.Ledger.GetAuxSubsidiaryLedgerDic2(account, start, end, (Voucher v) => true).ToDictionary((KeyValuePair<Account, Dictionary<object, SubsidiaryLedger>> kv) => kv.Key, (KeyValuePair<Account, Dictionary<object, SubsidiaryLedger>> kv) => Tuple.Create(start, end, kv.Value));
		_cacheSubsidiaryLedger2Valid = true;
		if (_cacheSubsidiaryLedger2.ContainsKey(account) && _cacheSubsidiaryLedger2[account].Item3.ContainsKey(auxiliaryItem))
		{
			return _cacheSubsidiaryLedger2[account].Item3[auxiliaryItem];
		}
		return account.Ledger.GetEmptySubsidiaryLedger(account, start, end, auxiliaryItem);
	}

	public bool IsEmptyAccountWithCache(Account account)
	{
		if (!_cacheNotEmptyAccountSetValid || _cacheNotEmptyAccountSet == null || account.Ledger != _cacheNotEmptyAccountSet.Item1)
		{
			HashSet<Account> hashSet = new HashSet<Account>();
			foreach (Voucher voucher in account.Ledger.Vouchers)
			{
				hashSet.Add(voucher.Account);
			}
			_cacheNotEmptyAccountSet = Tuple.Create(account.Ledger, hashSet);
			_cacheNotEmptyAccountSetValid = true;
		}
		if (_cacheNotEmptyAccountSet.Item2.Contains(account))
		{
			return false;
		}
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(account.Ledger);
		if (trialBalanceSheetWithCache.Start[account].Total == 0m && trialBalanceSheetWithCache.Debit[account].Total == 0m && trialBalanceSheetWithCache.Credit[account].Total == 0m)
		{
			return !account.Descendants.Any((Account c) => _cacheNotEmptyAccountSet.Item2.Contains(c));
		}
		return false;
	}

	public bool IsEmptyAuxiliaryItemWithCache(Account account, AuxiliaryItem auxItem)
	{
		if (!_cacheNotEmptyAuxiliaryItemSetValid || _cacheNotEmptyAuxiliaryItemSet == null || account.Ledger != _cacheNotEmptyAuxiliaryItemSet.Item1)
		{
			HashSet<Tuple<Account, AuxiliaryItem>> hashSet = new HashSet<Tuple<Account, AuxiliaryItem>>();
			foreach (Voucher voucher in account.Ledger.Vouchers)
			{
				foreach (AuxiliaryItem detail in voucher.Details)
				{
					hashSet.Add(Tuple.Create(voucher.Account, detail));
				}
				_cacheNotEmptyAuxiliaryItemSet = Tuple.Create(account.Ledger, hashSet);
				_cacheNotEmptyAuxiliaryItemSetValid = true;
			}
		}
		if (_cacheNotEmptyAuxiliaryItemSet.Item2.Contains(Tuple.Create(account, auxItem)))
		{
			return false;
		}
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(account.Ledger);
		List<ClassBalance> list = new List<ClassBalance>();
		list.AddRange(trialBalanceSheetWithCache.Start[account].ClassBalances.Values);
		list.AddRange(trialBalanceSheetWithCache.Debit[account].ClassBalances.Values);
		list.AddRange(trialBalanceSheetWithCache.Credit[account].ClassBalances.Values);
		foreach (ClassBalance item in list)
		{
			if (item.ItemBalances.TryGetValue(auxItem, out var value) && value != 0m)
			{
				return false;
			}
		}
		return true;
	}
}
