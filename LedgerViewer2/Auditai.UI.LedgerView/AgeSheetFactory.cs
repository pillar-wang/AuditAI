using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.Model;
using Auditai.UI.Controls;

namespace Auditai.UI.LedgerView;

public class AgeSheetFactory
{
	private readonly Ledger _ledger;

	private readonly LedgerAgingEditor _owner;

	public AgeSheetFactory(LedgerAgingEditor owner, Ledger ledger)
	{
		_owner = owner;
		_ledger = ledger;
	}

	public AgeSheet GetAgeSheet(Account account, DateTime baseDate)
	{
		return GetAgeSheetImpl(account, null, baseDate);
	}

	public AgeSheet GetAgeSheet(Account account, AuxiliaryClass auxiliaryClass, DateTime baseDate)
	{
		return GetAgeSheetImpl(account, auxiliaryClass, baseDate);
	}

	public AgeSheet GetAgeSheet(Account account, AuxiliaryItem auxiliaryItem, DateTime baseDate)
	{
		return GetAgeSheetImpl(account, auxiliaryItem, baseDate);
	}

	private AgeSheet GetAgeSheetImpl(Account account, object auxiliary, DateTime baseDate)
	{
		int num = baseDate.Year;
		DateTime startDate = _ledger.StartDate;
		DateTime endDate = _ledger.GetEndDate();
		if (baseDate.Year < startDate.Year)
		{
			return new AgeSheet
			{
				IsDebit = account.IsDebit
			};
		}
		if (baseDate.Year > endDate.Year)
		{
			num = endDate.Year + 1;
		}
		AgeAmount ageAmount = new AgeAmount();
		Dictionary<int, decimal> dictionary = new Dictionary<int, decimal>();
		Dictionary<int, decimal> dictionary2 = new Dictionary<int, decimal>();
		int num2 = num;
		while (num2 >= startDate.Year)
		{
			DateTime end = baseDate.CopyToSpecificYear(num2);
			DateTime dateTime = baseDate.CopyToSpecificYear(num2 - 1).AddDays(1.0);
			if (dateTime >= startDate)
			{
				Tuple<decimal, decimal, decimal> tuple = null;
				tuple = ((auxiliary is AuxiliaryClass auxiliaryClass) ? GetValue(account, auxiliaryClass, dateTime, end) : ((!(auxiliary is AuxiliaryItem auxiliaryItem)) ? GetValue(account, dateTime, end) : GetValue(account, auxiliaryItem, dateTime, end)));
				dictionary.Add(num2, tuple.Item2);
				dictionary2.Add(num2, tuple.Item3);
				if (new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).AddYears(-1) < startDate)
				{
					ageAmount = new AgeAmount
					{
						IsDebit = account.IsDebit,
						Amount = tuple.Item1
					};
					break;
				}
				num2--;
				continue;
			}
			DateTime start = new DateTime(end.Year, end.Month, end.Day).AddDays(1.0);
			DateTime end2 = new DateTime(end.Year, end.Month, end.Day).AddYears(1);
			Tuple<decimal, decimal, decimal> tuple2 = null;
			tuple2 = ((auxiliary is AuxiliaryClass auxiliaryClass2) ? GetValue(account, auxiliaryClass2, start, end2) : ((!(auxiliary is AuxiliaryItem auxiliaryItem2)) ? GetValue(account, start, end2) : GetValue(account, auxiliaryItem2, start, end2)));
			ageAmount = new AgeAmount
			{
				IsDebit = account.IsDebit,
				Amount = tuple2.Item1
			};
			break;
		}
		if (ageAmount.Amount < 0m)
		{
			ageAmount.IsDebit = !ageAmount.IsDebit;
			ageAmount.Amount = -ageAmount.Amount;
		}
		return GetAgeSheetImpl_0(ageAmount, dictionary, dictionary2);
	}

	private AgeSheet GetAgeSheetImpl_0(AgeAmount startBalance, Dictionary<int, decimal> debitBalance, Dictionary<int, decimal> creditBalance)
	{
		AgeSheet ageSheet = new AgeSheet();
		AgeAmountSheet ageAmountSheet = new AgeAmountSheet();
		foreach (KeyValuePair<int, decimal> item in debitBalance.OrderBy((KeyValuePair<int, decimal> d) => d.Key))
		{
			ageAmountSheet.Add(item.Key, new AgeAmount());
		}
		decimal num = default(decimal);
		foreach (KeyValuePair<int, decimal> item2 in debitBalance)
		{
			num += item2.Value;
		}
		decimal num2 = default(decimal);
		foreach (KeyValuePair<int, decimal> item3 in creditBalance)
		{
			num2 += item3.Value;
		}
		if (startBalance.IsDebit)
		{
			num += startBalance.Amount;
		}
		else
		{
			num2 += startBalance.Amount;
		}
		AgeAmount ageAmount = new AgeAmount
		{
			IsDebit = (num - num2 >= 0m),
			Amount = ((num - num2 >= 0m) ? num2 : num)
		};
		ageSheet.IsDebit = ageAmount.IsDebit;
		if (ageAmount.IsDebit)
		{
			if (startBalance.IsDebit)
			{
				if (startBalance.Amount >= ageAmount.Amount)
				{
					startBalance.Amount -= ageAmount.Amount;
					ageAmount.Amount = 0m;
				}
				else
				{
					ageAmount.Amount -= startBalance.Amount;
					startBalance.Amount = 0m;
				}
			}
			else
			{
				startBalance.Amount = 0m;
			}
		}
		else if (startBalance.IsDebit)
		{
			startBalance.Amount = 0m;
		}
		else if (startBalance.Amount >= ageAmount.Amount)
		{
			startBalance.Amount -= ageAmount.Amount;
			ageAmount.Amount = 0m;
		}
		else
		{
			ageAmount.Amount -= startBalance.Amount;
			startBalance.Amount = 0m;
		}
		foreach (KeyValuePair<int, decimal> item4 in debitBalance.OrderBy((KeyValuePair<int, decimal> y) => y.Key))
		{
			AgeAmount ageAmount2 = ageAmountSheet[item4.Key];
			if (ageAmount.IsDebit)
			{
				decimal value = item4.Value;
				if (value > ageAmount.Amount)
				{
					ageAmountSheet[item4.Key] = new AgeAmount
					{
						Amount = value - ageAmount.Amount,
						IsDebit = true
					};
					ageAmount.Amount = 0m;
				}
				else
				{
					ageAmountSheet[item4.Key] = new AgeAmount
					{
						Amount = 0m,
						IsDebit = true
					};
					ageAmount.Amount -= value;
				}
			}
			else
			{
				decimal num3 = creditBalance[item4.Key];
				if (num3 > ageAmount.Amount)
				{
					ageAmountSheet[item4.Key] = new AgeAmount
					{
						Amount = num3 - ageAmount.Amount,
						IsDebit = false
					};
					ageAmount.Amount = 0m;
				}
				else
				{
					ageAmountSheet[item4.Key] = new AgeAmount
					{
						Amount = 0m,
						IsDebit = false
					};
					ageAmount.Amount -= num3;
				}
			}
		}
		ageSheet.AgeBegin = startBalance;
		ageSheet.AgeAmountSheet = ageAmountSheet;
		return ageSheet;
	}

	private Tuple<decimal, decimal, decimal> GetValue_0(SubsidiaryLedger subsidiaryLedger)
	{
		SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
		decimal item = subsidiaryLedgerTotal?.Debit ?? 0m;
		decimal item2 = subsidiaryLedgerTotal?.Credit ?? 0m;
		return Tuple.Create(subsidiaryLedger.BeginBalance, item, item2);
	}

	private Tuple<decimal, decimal, decimal> GetValue(Account account, DateTime start, DateTime end)
	{
		TrialBalanceCache trialBalanceCache = _owner.SheetCacheManager.Get(_ledger, start, end);
		if (trialBalanceCache != null && trialBalanceCache.ContainsKey(account))
		{
			return _Sanitize(trialBalanceCache[account]);
		}
		SubsidiaryLedger subsidiaryLedger = _ledger.GetSubsidiaryLedger(account, start, end);
		return _Sanitize(GetValue_0(subsidiaryLedger));
		static Tuple<decimal, decimal, decimal> _Sanitize(Tuple<decimal, decimal, decimal> ret)
		{
			decimal num = default(decimal);
			decimal num2 = default(decimal);
			num = ret.Item2;
			num2 = ret.Item3;
			if (ret.Item2 < 0m)
			{
				num -= ret.Item2;
				num2 -= ret.Item2;
			}
			if (ret.Item3 < 0m)
			{
				num -= ret.Item3;
				num2 -= ret.Item3;
			}
			return Tuple.Create(ret.Item1, num, num2);
		}
	}

	private Tuple<decimal, decimal, decimal> GetValue(Account account, AuxiliaryClass auxiliaryClass, DateTime start, DateTime end)
	{
		SubsidiaryLedger subsidiaryLedger = _ledger.GetSubsidiaryLedger(account, start, end, auxiliaryClass);
		return GetValue_0(subsidiaryLedger);
	}

	private Tuple<decimal, decimal, decimal> GetValue(Account account, AuxiliaryItem auxiliaryItem, DateTime start, DateTime end)
	{
		SubsidiaryLedger subsidiaryLedger = _ledger.GetSubsidiaryLedger(account, start, end, auxiliaryItem);
		return GetValue_0(subsidiaryLedger);
	}
}
