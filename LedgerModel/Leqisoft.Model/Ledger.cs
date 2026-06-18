using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Leqisoft.Model;

public class Ledger
{
	private class TempSubData
	{
		public decimal currentBalance;

		public decimal grandDebit;

		public decimal grandCredit;

		public TempSubData(decimal currentBalance, decimal grandDebit, decimal grandCredit)
		{
			this.currentBalance = currentBalance;
			this.grandDebit = grandDebit;
			this.grandCredit = grandCredit;
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetLevelOrderAccounts_003Ed__33 : IEnumerable<Account>, IEnumerable, IEnumerator<Account>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private Account _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public Ledger _003C_003E4__this;

		private Queue<Account> _003Cq_003E5__2;

		private Account _003CvirtualRoot_003E5__3;

		private Account _003Croot_003E5__4;

		Account IEnumerator<Account>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetLevelOrderAccounts_003Ed__33(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cq_003E5__2 = null;
			_003CvirtualRoot_003E5__3 = null;
			_003Croot_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Ledger ledger = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_009b;
			}
			_003C_003E1__state = -1;
			_003Cq_003E5__2 = new Queue<Account>();
			_003CvirtualRoot_003E5__3 = new Account();
			_003CvirtualRoot_003E5__3.Children.AddRange(ledger.RootAccounts);
			_003Cq_003E5__2.Enqueue(_003CvirtualRoot_003E5__3);
			goto IL_00e2;
			IL_009b:
			foreach (Account child in _003Croot_003E5__4.Children)
			{
				_003Cq_003E5__2.Enqueue(child);
			}
			_003Croot_003E5__4 = null;
			goto IL_00e2;
			IL_00e2:
			if (_003Cq_003E5__2.Count > 0)
			{
				_003Croot_003E5__4 = _003Cq_003E5__2.Dequeue();
				if (_003Croot_003E5__4 != _003CvirtualRoot_003E5__3)
				{
					_003C_003E2__current = _003Croot_003E5__4;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_009b;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Account> IEnumerable<Account>.GetEnumerator()
		{
			_003CGetLevelOrderAccounts_003Ed__33 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetLevelOrderAccounts_003Ed__33(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Account>)this).GetEnumerator();
		}
	}

	internal LedgerDAL DAL { get; private set; }

	public int Year { get; internal set; }

	public string CompanyName { get; internal set; }

	public string LedgerNumber { get; internal set; }

	public DateTime StartDate { get; internal set; }

	public DateTime EndDate { get; internal set; }

	public Currency BaseCurrency { get; internal set; }

	public List<Account> Accounts { get; }

	public IEnumerable<Account> RootAccounts => Accounts.Where((Account a) => a.Parent == null);

	public DateBalance InitialBalance { get; }

	public List<Voucher> Vouchers { get; }

	public List<VoucherType> VoucherTypes { get; }

	public List<AuxiliaryClass> AuxiliaryClasses { get; }

	public List<AuxiliaryItem> AuxiliaryItems { get; }

	public List<Currency> Currencies { get; }

	public IEnumerable<Account> GetLevelOrderAccounts()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetLevelOrderAccounts_003Ed__33(-2)
		{
			_003C_003E4__this = this
		};
	}

	internal Ledger()
	{
		Accounts = new List<Account>();
		Vouchers = new List<Voucher>();
		VoucherTypes = new List<VoucherType>();
		AuxiliaryClasses = new List<AuxiliaryClass>();
		AuxiliaryItems = new List<AuxiliaryItem>();
		Currencies = new List<Currency>();
		InitialBalance = new DateBalance();
	}

	public static Ledger LoadFromFile(string fileName)
	{
		setFileAttribute(fileName);
		LedgerDAL ledgerDAL = new LedgerDAL(fileName);
		Ledger ledger = ledgerDAL.GetLedger();
		ledger.DAL = ledgerDAL;
		return ledger;
	}

	public static List<Tuple<int, string, string>> GetTableData_ItemClass(string fileName)
	{
		setFileAttribute(fileName);
		LedgerDAL ledgerDAL = new LedgerDAL(fileName);
		return ledgerDAL.GetTableData_ItemClass();
	}

	public static void UpdateTableData_ItemClassIdAndCode(string fileName, List<Tuple<int, int, string>> dataList)
	{
		setFileAttribute(fileName);
		LedgerDAL ledgerDAL = new LedgerDAL(fileName);
		ledgerDAL.UpdateTableData_ItemClassIdAndCode(dataList);
	}

	public static void UpdateTableData_ItemClassId(string fileName, List<Tuple<int, int>> dataList)
	{
		setFileAttribute(fileName);
		LedgerDAL ledgerDAL = new LedgerDAL(fileName);
		ledgerDAL.UpdateTableData_ItemClassId(dataList);
	}

	private static void setFileAttribute(string fileName)
	{
		try
		{
			File.SetAttributes(fileName, FileAttributes.Normal);
		}
		catch (Exception)
		{
		}
	}

	public void Save()
	{
		DAL.SaveLedgerIncremental(this);
	}

	public void TotalSave()
	{
		DAL.SaveLedgerTotal(this);
	}

	public void SetStartDate(DateTime date)
	{
		StartDate = date;
	}

	public void SetCompanyName(string company)
	{
		CompanyName = company;
	}

	public DateBalance GetDebits(DateTime start, DateTime end)
	{
		DateBalance ret = CreateDateBalance();
		IEnumerable<Voucher> enumerable = Vouchers.Where((Voucher v) => v.IsDebit && start.Date <= v.Day.Date && v.Day.Date <= end.Date);
		foreach (Voucher item in enumerable)
		{
			ret[item.Account].Total += item.Amount;
			foreach (AuxiliaryItem detail in item.Details)
			{
				if (!ret[item.Account].ClassBalances.TryGetValue(detail.Class, out var value))
				{
					value = new ClassBalance();
					ret[item.Account].ClassBalances.Add(detail.Class, value);
				}
				value.Total += item.Amount;
				if (!value.ItemBalances.ContainsKey(detail))
				{
					value.ItemBalances.Add(detail, 0m);
				}
				value.ItemBalances[detail] += item.Amount;
			}
		}
		Dictionary<Account, decimal> accountSelfValueDic = ret.Keys.ToDictionary((Account item) => item, (Account item) => ret[item].Total);
		foreach (Account item2 in ret.Keys.Where((Account a) => a.Children.Count > 0))
		{
			ret[item2].Total = GetAllSubAccountTotal(item2);
		}
		return ret;
		decimal GetAllSubAccountTotal(Account parentAccount)
		{
			if (parentAccount.Children.Count == 0)
			{
				return accountSelfValueDic[parentAccount];
			}
			decimal result = default(decimal);
			if (accountSelfValueDic.TryGetValue(parentAccount, out var value2))
			{
				result = value2;
			}
			foreach (Account child in parentAccount.Children)
			{
				result += GetAllSubAccountTotal(child);
			}
			return result;
		}
	}

	public DateBalance GetCredits(DateTime start, DateTime end)
	{
		DateBalance ret = CreateDateBalance();
		IEnumerable<Voucher> enumerable = Vouchers.Where((Voucher v) => !v.IsDebit && start.Date <= v.Day.Date && v.Day.Date <= end.Date);
		foreach (Voucher item in enumerable)
		{
			ret[item.Account].Total += item.Amount;
			foreach (AuxiliaryItem detail in item.Details)
			{
				if (!ret[item.Account].ClassBalances.TryGetValue(detail.Class, out var value))
				{
					value = new ClassBalance();
					ret[item.Account].ClassBalances.Add(detail.Class, value);
				}
				value.Total += item.Amount;
				if (!value.ItemBalances.ContainsKey(detail))
				{
					value.ItemBalances.Add(detail, 0m);
				}
				value.ItemBalances[detail] += item.Amount;
			}
		}
		Dictionary<Account, decimal> accountSelfValueDic = ret.Keys.ToDictionary((Account item) => item, (Account item) => ret[item].Total);
		foreach (Account item2 in ret.Keys.Where((Account a) => a.Children.Count > 0))
		{
			ret[item2].Total = GetAllSubAccountTotal(item2);
		}
		return ret;
		decimal GetAllSubAccountTotal(Account parentAccount)
		{
			if (parentAccount.Children.Count == 0)
			{
				return accountSelfValueDic[parentAccount];
			}
			decimal result = default(decimal);
			if (accountSelfValueDic.TryGetValue(parentAccount, out var value2))
			{
				result = value2;
			}
			foreach (Account child in parentAccount.Children)
			{
				result += GetAllSubAccountTotal(child);
			}
			return result;
		}
	}

	private DateBalance GetDateBalance(DateBalance initial, DateTime start, DateTime end)
	{
		DateBalance dateBalance = (DateBalance)initial.Clone();
		DateBalance debits = GetDebits(start, end);
		DateBalance credits = GetCredits(start, end);
		foreach (KeyValuePair<Account, AccountBalance> item in debits)
		{
			AccountBalance accountBalance = dateBalance[item.Key];
			if (item.Key.IsDebit)
			{
				accountBalance.Total += item.Value.Total;
			}
			else
			{
				accountBalance.Total -= item.Value.Total;
			}
			foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in item.Value.ClassBalances)
			{
				if (!accountBalance.ClassBalances.TryGetValue(classBalance.Key, out var value))
				{
					value = new ClassBalance();
					accountBalance.ClassBalances.Add(classBalance.Key, value);
				}
				if (item.Key.IsDebit)
				{
					value.Total += classBalance.Value.Total;
				}
				else
				{
					value.Total -= classBalance.Value.Total;
				}
				foreach (KeyValuePair<AuxiliaryItem, decimal> itemBalance in classBalance.Value.ItemBalances)
				{
					if (!value.ItemBalances.ContainsKey(itemBalance.Key))
					{
						value.ItemBalances.Add(itemBalance.Key, 0m);
					}
					if (item.Key.IsDebit)
					{
						value.ItemBalances[itemBalance.Key] += itemBalance.Value;
					}
					else
					{
						value.ItemBalances[itemBalance.Key] -= itemBalance.Value;
					}
				}
			}
		}
		foreach (KeyValuePair<Account, AccountBalance> item2 in credits)
		{
			AccountBalance accountBalance2 = dateBalance[item2.Key];
			if (item2.Key.IsDebit)
			{
				accountBalance2.Total -= item2.Value.Total;
			}
			else
			{
				accountBalance2.Total += item2.Value.Total;
			}
			foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance2 in item2.Value.ClassBalances)
			{
				if (!accountBalance2.ClassBalances.TryGetValue(classBalance2.Key, out var value2))
				{
					value2 = new ClassBalance();
					accountBalance2.ClassBalances.Add(classBalance2.Key, value2);
				}
				if (item2.Key.IsDebit)
				{
					value2.Total -= classBalance2.Value.Total;
				}
				else
				{
					value2.Total += classBalance2.Value.Total;
				}
				foreach (KeyValuePair<AuxiliaryItem, decimal> itemBalance2 in classBalance2.Value.ItemBalances)
				{
					if (!value2.ItemBalances.ContainsKey(itemBalance2.Key))
					{
						value2.ItemBalances.Add(itemBalance2.Key, 0m);
					}
					if (item2.Key.IsDebit)
					{
						value2.ItemBalances[itemBalance2.Key] -= itemBalance2.Value;
					}
					else
					{
						value2.ItemBalances[itemBalance2.Key] += itemBalance2.Value;
					}
				}
			}
		}
		return dateBalance;
	}

	private DateBalance GetDateBalance(DateTime date)
	{
		return GetDateBalance(InitialBalance, StartDate, date);
	}

	public TrialBalanceSheet GetTrialBalanceSheet(DateTime start, DateTime end)
	{
		TrialBalanceSheet trialBalanceSheet = new TrialBalanceSheet();
		if (start < StartDate)
		{
			trialBalanceSheet.Start = InitialBalance.OnlyCloneKey();
			trialBalanceSheet.Debit = GetDebits(start, end);
			trialBalanceSheet.Credit = GetCredits(start, end);
			if (end < StartDate)
			{
				trialBalanceSheet.End = InitialBalance.OnlyCloneKey();
			}
			else
			{
				trialBalanceSheet.End = GetDateBalance(end);
			}
			return trialBalanceSheet;
		}
		trialBalanceSheet.Start = GetDateBalance(start - TimeSpan.FromDays(1.0));
		trialBalanceSheet.Debit = GetDebits(start, end);
		trialBalanceSheet.Credit = GetCredits(start, end);
		trialBalanceSheet.End = GetDateBalance(trialBalanceSheet.Start, start, end);
		return trialBalanceSheet;
	}

	public SubsidiaryLedger GetSubsidiaryLedger(Account account, DateTime start, DateTime end)
	{
		return GetSubsidiaryLedgerImpl(account, start, end, (Voucher v) => true, GetBalance(account, start));
	}

	public SubsidiaryLedger GetSubsidiaryLedger(Account account, DateTime start, DateTime end, AuxiliaryClass auxClass)
	{
		return GetSubsidiaryLedgerImpl(account, start, end, (Voucher v) => v.Details.Any((AuxiliaryItem i) => i.Class == auxClass), GetBalance(account, start, auxClass));
	}

	public SubsidiaryLedger GetSubsidiaryLedger(Account account, DateTime start, DateTime end, AuxiliaryItem auxItem)
	{
		return GetSubsidiaryLedgerImpl(account, start, end, (Voucher v) => v.Details.Any((AuxiliaryItem i) => i == auxItem), GetBalance(account, start, auxItem));
	}

	private SubsidiaryLedger GetSubsidiaryLedgerImpl(Account account, DateTime start, DateTime end, Func<Voucher, bool> filter, decimal beginBalance)
	{
		List<Account> accounts = account.DescendantsAndSelf;
		SubsidiaryLedger subsidiaryLedger = new SubsidiaryLedger();
		subsidiaryLedger.Account = account;
		subsidiaryLedger.Start = start;
		subsidiaryLedger.End = end;
		subsidiaryLedger.BeginBalance = beginBalance;
		decimal beginBalance2 = subsidiaryLedger.BeginBalance;
		decimal debit = default(decimal);
		decimal credit = default(decimal);
		foreach (IGrouping<Tuple<int, int>, Voucher> item in from v in Vouchers.Where((Voucher v) => start.Date <= v.Day.Date && v.Day.Date <= end.Date && accounts.Contains(v.Account)).Where(filter)
			group v by Tuple.Create(v.Day.Year, v.Day.Month) into t
			orderby t.Key.Item1, t.Key.Item2
			select t)
		{
			MonthSubsidiaryLedger monthSubsidiaryLedger = new MonthSubsidiaryLedger(item.Key.Item1, item.Key.Item2);
			foreach (Voucher item2 in (from t in item
				orderby t.Day, t.Type.Name
				select t).ThenBy((Voucher t) => t.Number, StringNumberComparer.Instance))
			{
				if (item2.IsDebit)
				{
					monthSubsidiaryLedger.Total.Debit += item2.Amount;
				}
				else
				{
					monthSubsidiaryLedger.Total.Credit += item2.Amount;
				}
				beginBalance2 += ((account.IsDebit == item2.IsDebit) ? item2.Amount : (-item2.Amount));
				monthSubsidiaryLedger.Entries.Add(new SubsidiaryLedgerEntry
				{
					Voucher = item2,
					Balance = beginBalance2
				});
			}
			monthSubsidiaryLedger.Total.Balance = beginBalance2;
			debit += monthSubsidiaryLedger.Total.Debit;
			credit += monthSubsidiaryLedger.Total.Credit;
			monthSubsidiaryLedger.GrandTotal.Debit = debit;
			monthSubsidiaryLedger.GrandTotal.Credit = credit;
			monthSubsidiaryLedger.GrandTotal.Balance = beginBalance2;
			subsidiaryLedger.Months.Add(monthSubsidiaryLedger);
		}
		return subsidiaryLedger;
	}

	public Dictionary<object, SubsidiaryLedger> GetAuxSubsidiaryLedgerDic(Account account, DateTime start, DateTime end, Predicate<Voucher> predicate)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		HashSet<Account> accounts = new HashSet<Account>(account.DescendantsAndSelf);
		Dictionary<object, TempSubData> dictionary = new Dictionary<object, TempSubData>();
		Dictionary<object, SubsidiaryLedger> dictionary2 = new Dictionary<object, SubsidiaryLedger>();
		foreach (IGrouping<Tuple<int, int>, Voucher> item in from v in Vouchers
			where v.Details.Count > 0 && accounts.Contains(v.Account) && start.Date <= v.Day.Date && v.Day.Date <= end.Date && predicate(v)
			group v by Tuple.Create(v.Day.Year, v.Day.Month) into t
			orderby t.Key.Item1, t.Key.Item2
			select t)
		{
			Dictionary<object, MonthSubsidiaryLedger> dictionary3 = new Dictionary<object, MonthSubsidiaryLedger>();
			foreach (Voucher item2 in (from t in item
				orderby t.Day, t.Type.Name
				select t).ThenBy((Voucher t) => t.Number, StringNumberComparer.Instance))
			{
				foreach (IGrouping<AuxiliaryClass, AuxiliaryItem> item3 in from v in item2.Details
					group v by v.Class)
				{
					if (!dictionary.ContainsKey(item3.Key))
					{
						decimal balance = GetBalance(account, start, item3.Key);
						SubsidiaryLedger subsidiaryLedger = new SubsidiaryLedger();
						subsidiaryLedger.Account = account;
						subsidiaryLedger.Start = start;
						subsidiaryLedger.End = end;
						subsidiaryLedger.BeginBalance = balance;
						dictionary2.Add(item3.Key, subsidiaryLedger);
						dictionary.Add(item3.Key, new TempSubData(balance, 0m, 0m));
					}
					if (!dictionary3.ContainsKey(item3.Key))
					{
						dictionary3.Add(item3.Key, new MonthSubsidiaryLedger(item.Key.Item1, item.Key.Item2));
					}
					MonthSubsidiaryLedger monthSubsidiaryLedger = dictionary3[item3.Key];
					if (item2.IsDebit)
					{
						monthSubsidiaryLedger.Total.Debit += item2.Amount;
					}
					else
					{
						monthSubsidiaryLedger.Total.Credit += item2.Amount;
					}
					TempSubData tempSubData = dictionary[item3.Key];
					tempSubData.currentBalance += ((account.IsDebit == item2.IsDebit) ? item2.Amount : (-item2.Amount));
					monthSubsidiaryLedger.Entries.Add(new SubsidiaryLedgerEntry
					{
						Voucher = item2,
						Balance = tempSubData.currentBalance
					});
					foreach (AuxiliaryItem item4 in item3)
					{
						if (!dictionary.ContainsKey(item4))
						{
							decimal balance2 = GetBalance(account, start, item4);
							SubsidiaryLedger subsidiaryLedger2 = new SubsidiaryLedger();
							subsidiaryLedger2.Account = account;
							subsidiaryLedger2.Start = start;
							subsidiaryLedger2.End = end;
							subsidiaryLedger2.BeginBalance = balance2;
							dictionary2.Add(item4, subsidiaryLedger2);
							dictionary.Add(item4, new TempSubData(balance2, 0m, 0m));
						}
						if (!dictionary3.ContainsKey(item4))
						{
							dictionary3.Add(item4, new MonthSubsidiaryLedger(item.Key.Item1, item.Key.Item2));
						}
						MonthSubsidiaryLedger monthSubsidiaryLedger2 = dictionary3[item4];
						if (item2.IsDebit)
						{
							monthSubsidiaryLedger2.Total.Debit += item2.Amount;
						}
						else
						{
							monthSubsidiaryLedger2.Total.Credit += item2.Amount;
						}
						TempSubData tempSubData2 = dictionary[item4];
						tempSubData2.currentBalance += ((account.IsDebit == item2.IsDebit) ? item2.Amount : (-item2.Amount));
						monthSubsidiaryLedger2.Entries.Add(new SubsidiaryLedgerEntry
						{
							Voucher = item2,
							Balance = tempSubData2.currentBalance
						});
					}
				}
			}
			foreach (KeyValuePair<object, MonthSubsidiaryLedger> item5 in dictionary3)
			{
				MonthSubsidiaryLedger value = item5.Value;
				TempSubData tempSubData3 = dictionary[item5.Key];
				tempSubData3.grandDebit += value.Total.Debit;
				tempSubData3.grandCredit += value.Total.Credit;
				value.Total.Balance = tempSubData3.currentBalance;
				value.GrandTotal.Debit = tempSubData3.grandDebit;
				value.GrandTotal.Credit = tempSubData3.grandCredit;
				value.GrandTotal.Balance = tempSubData3.currentBalance;
				dictionary2[item5.Key].Months.Add(value);
			}
		}
		return dictionary2;
	}

	public Dictionary<Account, Dictionary<object, SubsidiaryLedger>> GetAuxSubsidiaryLedgerDic2(Account account, DateTime start, DateTime end, Predicate<Voucher> predicate)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Dictionary<Account, Dictionary<object, Tuple<SubsidiaryLedger, TempSubData>>> dictionary = new Dictionary<Account, Dictionary<object, Tuple<SubsidiaryLedger, TempSubData>>>();
		foreach (IGrouping<Tuple<int, int>, Voucher> item4 in from v in Vouchers
			where v.Details.Count > 0 && start.Date <= v.Day.Date && v.Day.Date <= end.Date && predicate(v)
			group v by Tuple.Create(v.Day.Year, v.Day.Month) into t
			orderby t.Key.Item1, t.Key.Item2
			select t)
		{
			Dictionary<Account, Dictionary<object, MonthSubsidiaryLedger>> dictionary2 = new Dictionary<Account, Dictionary<object, MonthSubsidiaryLedger>>();
			foreach (Voucher item5 in (from t in item4
				orderby t.Day, t.Type.Name
				select t).ThenBy((Voucher t) => t.Number, StringNumberComparer.Instance))
			{
				if (!dictionary.ContainsKey(item5.Account))
				{
					dictionary.Add(item5.Account, new Dictionary<object, Tuple<SubsidiaryLedger, TempSubData>>());
				}
				if (!dictionary2.ContainsKey(item5.Account))
				{
					dictionary2.Add(item5.Account, new Dictionary<object, MonthSubsidiaryLedger>());
				}
				Dictionary<object, Tuple<SubsidiaryLedger, TempSubData>> dictionary3 = dictionary[item5.Account];
				foreach (IGrouping<AuxiliaryClass, AuxiliaryItem> item6 in from v in item5.Details
					group v by v.Class)
				{
					if (!dictionary3.ContainsKey(item6.Key))
					{
						decimal balance = GetBalance(item5.Account, start, item6.Key);
						SubsidiaryLedger subsidiaryLedger = new SubsidiaryLedger();
						subsidiaryLedger.Account = item5.Account;
						subsidiaryLedger.Start = start;
						subsidiaryLedger.End = end;
						subsidiaryLedger.BeginBalance = balance;
						dictionary3.Add(item6.Key, Tuple.Create(subsidiaryLedger, new TempSubData(balance, 0m, 0m)));
					}
					Dictionary<object, MonthSubsidiaryLedger> dictionary4 = dictionary2[item5.Account];
					if (!dictionary4.ContainsKey(item6.Key))
					{
						dictionary4.Add(item6.Key, new MonthSubsidiaryLedger(item4.Key.Item1, item4.Key.Item2));
					}
					MonthSubsidiaryLedger monthSubsidiaryLedger = dictionary4[item6.Key];
					if (item5.IsDebit)
					{
						monthSubsidiaryLedger.Total.Debit += item5.Amount;
					}
					else
					{
						monthSubsidiaryLedger.Total.Credit += item5.Amount;
					}
					TempSubData item = dictionary3[item6.Key].Item2;
					item.currentBalance += ((item5.Account.IsDebit == item5.IsDebit) ? item5.Amount : (-item5.Amount));
					monthSubsidiaryLedger.Entries.Add(new SubsidiaryLedgerEntry
					{
						Voucher = item5,
						Balance = item.currentBalance
					});
					foreach (AuxiliaryItem item7 in item6)
					{
						if (!dictionary3.ContainsKey(item7))
						{
							decimal balance2 = GetBalance(item5.Account, start, item7);
							SubsidiaryLedger subsidiaryLedger2 = new SubsidiaryLedger();
							subsidiaryLedger2.Account = item5.Account;
							subsidiaryLedger2.Start = start;
							subsidiaryLedger2.End = end;
							subsidiaryLedger2.BeginBalance = balance2;
							dictionary3.Add(item7, Tuple.Create(subsidiaryLedger2, new TempSubData(balance2, 0m, 0m)));
						}
						if (!dictionary4.ContainsKey(item7))
						{
							dictionary4.Add(item7, new MonthSubsidiaryLedger(item4.Key.Item1, item4.Key.Item2));
						}
						MonthSubsidiaryLedger monthSubsidiaryLedger2 = dictionary4[item7];
						if (item5.IsDebit)
						{
							monthSubsidiaryLedger2.Total.Debit += item5.Amount;
						}
						else
						{
							monthSubsidiaryLedger2.Total.Credit += item5.Amount;
						}
						TempSubData item2 = dictionary3[item7].Item2;
						item2.currentBalance += ((item5.Account.IsDebit == item5.IsDebit) ? item5.Amount : (-item5.Amount));
						monthSubsidiaryLedger2.Entries.Add(new SubsidiaryLedgerEntry
						{
							Voucher = item5,
							Balance = item2.currentBalance
						});
					}
				}
			}
			foreach (KeyValuePair<Account, Dictionary<object, MonthSubsidiaryLedger>> item8 in dictionary2)
			{
				Dictionary<object, MonthSubsidiaryLedger> value = item8.Value;
				Dictionary<object, Tuple<SubsidiaryLedger, TempSubData>> dictionary5 = dictionary[item8.Key];
				foreach (KeyValuePair<object, MonthSubsidiaryLedger> item9 in value)
				{
					MonthSubsidiaryLedger value2 = item9.Value;
					Tuple<SubsidiaryLedger, TempSubData> tuple = dictionary5[item9.Key];
					TempSubData item3 = tuple.Item2;
					item3.grandDebit += value2.Total.Debit;
					item3.grandCredit += value2.Total.Credit;
					value2.Total.Balance = item3.currentBalance;
					value2.GrandTotal.Debit = item3.grandDebit;
					value2.GrandTotal.Credit = item3.grandCredit;
					value2.GrandTotal.Balance = item3.currentBalance;
					tuple.Item1.Months.Add(value2);
				}
			}
		}
		return dictionary.ToDictionary((KeyValuePair<Account, Dictionary<object, Tuple<SubsidiaryLedger, TempSubData>>> kv) => kv.Key, (KeyValuePair<Account, Dictionary<object, Tuple<SubsidiaryLedger, TempSubData>>> kv) => kv.Value.ToDictionary((KeyValuePair<object, Tuple<SubsidiaryLedger, TempSubData>> kv2) => kv2.Key, (KeyValuePair<object, Tuple<SubsidiaryLedger, TempSubData>> kv2) => kv2.Value.Item1));
	}

	public SubsidiaryLedger GetEmptySubsidiaryLedger(Account account, DateTime start, DateTime end, AuxiliaryItem auxiliaryItem)
	{
		decimal balance = GetBalance(account, start, auxiliaryItem);
		SubsidiaryLedger subsidiaryLedger = new SubsidiaryLedger();
		subsidiaryLedger.Account = account;
		subsidiaryLedger.Start = start;
		subsidiaryLedger.End = end;
		subsidiaryLedger.BeginBalance = balance;
		return subsidiaryLedger;
	}

	public ReceivableAgeSheet GetReceivableAgeSheet(DateTime end)
	{
		TrialBalanceSheet trialBalanceSheet = GetTrialBalanceSheet(StartDate, end);
		ReceivableAgeSheet receivableAgeSheet = new ReceivableAgeSheet();
		receivableAgeSheet.YearCount = Math.Min(6, GetYearDiff(StartDate.AddDays(-1.0), end));
		foreach (KeyValuePair<Account, AccountBalance> item in trialBalanceSheet.End)
		{
			ReceivableAgeEntry receivableAgeEntry = new ReceivableAgeEntry();
			receivableAgeEntry.Value.End = Math.Abs(item.Value.Total);
			bool isDebit = item.Key.IsDebit;
			receivableAgeEntry.Value.IsDebit = ((item.Value.Total >= 0m) ? isDebit : (!isDebit));
			receivableAgeEntry.Value.Values = new decimal[receivableAgeSheet.YearCount];
			decimal total = trialBalanceSheet.Start[item.Key].Total;
			bool flag = ((total >= 0m) ? isDebit : (!isDebit));
			total = Math.Abs(total);
			if (flag == receivableAgeEntry.Value.IsDebit)
			{
				receivableAgeEntry.Value.Values[receivableAgeSheet.YearCount - 1] = total;
			}
			else
			{
				receivableAgeEntry.Value.Opposite = total;
			}
			receivableAgeSheet.Entries.Add(item.Key, receivableAgeEntry);
			foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in item.Value.ClassBalances)
			{
				foreach (KeyValuePair<AuxiliaryItem, decimal> itemBalance in classBalance.Value.ItemBalances)
				{
					ReceivableAgeValue receivableAgeValue = new ReceivableAgeValue();
					receivableAgeValue.Values = new decimal[receivableAgeSheet.YearCount];
					ReceivableAgeValue receivableAgeValue2 = receivableAgeValue;
					receivableAgeValue2.End = Math.Abs(itemBalance.Value);
					receivableAgeValue2.IsDebit = ((itemBalance.Value >= 0m) ? isDebit : (!isDebit));
					decimal value = default(decimal);
					if (trialBalanceSheet.Start[item.Key].ClassBalances.TryGetValue(classBalance.Key, out var value2))
					{
						value2.ItemBalances.TryGetValue(itemBalance.Key, out value);
					}
					bool flag2 = ((value >= 0m) ? isDebit : (!isDebit));
					value = Math.Abs(value);
					if (flag2 == receivableAgeValue2.IsDebit)
					{
						receivableAgeValue2.Values[receivableAgeSheet.YearCount - 1] = value;
					}
					else
					{
						receivableAgeValue2.Opposite = value;
					}
					receivableAgeEntry.Aux.Add(itemBalance.Key, receivableAgeValue2);
				}
			}
		}
		List<Voucher> list = Vouchers.Where((Voucher v) => v.Day.Date <= end.Date).ToList();
		foreach (Voucher item2 in list)
		{
			bool flag3 = ((item2.Amount >= 0m) ? item2.IsDebit : (!item2.IsDebit));
			decimal num = Math.Abs(item2.Amount);
			int num2 = Math.Min(6, GetYearDiff(item2.Day, end)) - 1;
			foreach (Account item3 in item2.Account.AncestorsAndSelf)
			{
				if (flag3 == receivableAgeSheet.Entries[item3].Value.IsDebit)
				{
					receivableAgeSheet.Entries[item3].Value.Values[num2] += num;
				}
				else
				{
					receivableAgeSheet.Entries[item3].Value.Opposite += num;
				}
			}
			foreach (AuxiliaryItem detail in item2.Details)
			{
				ReceivableAgeValue receivableAgeValue3 = receivableAgeSheet.Entries[item2.Account].Aux[detail];
				if (flag3 == receivableAgeValue3.IsDebit)
				{
					receivableAgeValue3.Values[num2] += num;
				}
				else
				{
					receivableAgeValue3.Opposite += num;
				}
			}
		}
		foreach (KeyValuePair<Account, ReceivableAgeEntry> entry in receivableAgeSheet.Entries)
		{
			ReceivableAgeEntry value3 = entry.Value;
			int num3 = receivableAgeSheet.YearCount - 1;
			while (num3 >= 0 && !(value3.Value.Opposite == 0m))
			{
				if (value3.Value.Opposite < value3.Value.Values[num3])
				{
					value3.Value.Values[num3] -= value3.Value.Opposite;
					value3.Value.Opposite = 0m;
				}
				else
				{
					value3.Value.Opposite -= value3.Value.Values[num3];
					value3.Value.Values[num3] = default(decimal);
				}
				num3--;
			}
			foreach (KeyValuePair<AuxiliaryItem, ReceivableAgeValue> item4 in value3.Aux)
			{
				ReceivableAgeValue value4 = item4.Value;
				int num4 = receivableAgeSheet.YearCount - 1;
				while (num4 >= 0 && !(value4.Opposite == 0m))
				{
					if (value4.Opposite < value4.Values[num4])
					{
						value4.Values[num4] -= value4.Opposite;
						value4.Opposite = 0m;
					}
					else
					{
						value4.Opposite -= value4.Values[num4];
						value4.Values[num4] = default(decimal);
					}
					num4--;
				}
			}
		}
		return receivableAgeSheet;
	}

	private static int GetYearDiff(DateTime start, DateTime end)
	{
		int num = ((end.Month > start.Month || (end.Month == start.Month && end.Day >= start.Day)) ? 1 : 0);
		return end.Year - start.Year + num;
	}

	private decimal GetBalance(Account account, DateTime date)
	{
		List<Account> accounts = account.DescendantsAndSelf;
		decimal num = Vouchers.Where((Voucher v) => v.Day < date && accounts.Contains(v.Account)).Sum((Voucher v) => (!v.IsDebit) ? (-v.Amount) : v.Amount);
		return InitialBalance[account].Total + (account.IsDebit ? num : (-num));
	}

	private decimal GetBalance(Account account, DateTime date, AuxiliaryClass auxClass)
	{
		ClassBalance value;
		decimal num = (InitialBalance[account].ClassBalances.TryGetValue(auxClass, out value) ? value.Total : 0m);
		decimal num2 = Vouchers.Where((Voucher v) => v.Day < date && account == v.Account && v.Details.Any((AuxiliaryItem d) => d.Class == auxClass)).Sum((Voucher v) => (!v.IsDebit) ? (-v.Amount) : v.Amount);
		return num + (account.IsDebit ? num2 : (-num2));
	}

	private decimal GetBalance(Account account, DateTime date, AuxiliaryItem auxItem)
	{
		decimal num = default(decimal);
		if (InitialBalance[account].ClassBalances.TryGetValue(auxItem.Class, out var value) && value.ItemBalances.TryGetValue(auxItem, out var value2))
		{
			num = value2;
		}
		decimal num2 = Vouchers.Where((Voucher v) => v.Day < date && account == v.Account && v.Details.Contains(auxItem)).Sum((Voucher v) => (!v.IsDebit) ? (-v.Amount) : v.Amount);
		return num + (account.IsDebit ? num2 : (-num2));
	}

	public DateTime GetEndDate()
	{
		if (Vouchers.Count == 0)
		{
			return new DateTime(StartDate.Year, StartDate.Month, DateTime.DaysInMonth(StartDate.Year, StartDate.Month)).AddDays(1.0).AddMilliseconds(-1.0);
		}
		DateTime dateTime = Vouchers.Max((Voucher v) => v.Day);
		return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month)).AddDays(1.0).AddMilliseconds(-1.0);
	}

	public List<Tuple<DateTime, decimal>> GetBalanceByDay(Account account, DateTime start, DateTime end, object Aux = null)
	{
		List<Tuple<DateTime, decimal>> list = new List<Tuple<DateTime, decimal>>();
		SubsidiaryLedger subsidiaryLedger = null;
		if (Aux == null)
		{
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end);
		}
		else if (Aux.GetType() == typeof(AuxiliaryClass))
		{
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end, (AuxiliaryClass)Aux);
		}
		else
		{
			if (!(Aux.GetType() == typeof(AuxiliaryItem)))
			{
				return list;
			}
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end, (AuxiliaryItem)Aux);
		}
		List<DateTime> list2 = new List<DateTime>();
		foreach (MonthSubsidiaryLedger month in subsidiaryLedger.Months)
		{
			foreach (SubsidiaryLedgerEntry entry in month.Entries)
			{
				if (list2.Contains(entry.Voucher.Day))
				{
					list[list2.IndexOf(entry.Voucher.Day)] = Tuple.Create(entry.Voucher.Day, entry.Balance);
					continue;
				}
				list.Add(Tuple.Create(entry.Voucher.Day, entry.Balance));
				list2.Add(entry.Voucher.Day);
			}
		}
		return list.OrderBy((Tuple<DateTime, decimal> a) => a.Item1).ToList();
	}

	public List<Tuple<DateTime, decimal>> GetCreditsByDay(Account account, DateTime start, DateTime end, object Aux = null)
	{
		List<Tuple<DateTime, decimal>> list = new List<Tuple<DateTime, decimal>>();
		SubsidiaryLedger subsidiaryLedger = null;
		if (Aux == null)
		{
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end);
		}
		else if (Aux.GetType() == typeof(AuxiliaryClass))
		{
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end, (AuxiliaryClass)Aux);
		}
		else
		{
			if (!(Aux.GetType() == typeof(AuxiliaryItem)))
			{
				return list;
			}
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end, (AuxiliaryItem)Aux);
		}
		List<DateTime> list2 = new List<DateTime>();
		foreach (MonthSubsidiaryLedger month in subsidiaryLedger.Months)
		{
			foreach (SubsidiaryLedgerEntry entry in month.Entries)
			{
				if (!entry.Voucher.IsDebit && entry.Voucher.Amount > 0m)
				{
					if (list2.Contains(entry.Voucher.Day))
					{
						list[list2.IndexOf(entry.Voucher.Day)] = Tuple.Create(entry.Voucher.Day, list[list2.IndexOf(entry.Voucher.Day)].Item2 + entry.Voucher.Amount);
						continue;
					}
					list.Add(Tuple.Create(entry.Voucher.Day, entry.Voucher.Amount));
					list2.Add(entry.Voucher.Day);
				}
			}
		}
		return list.OrderBy((Tuple<DateTime, decimal> a) => a.Item1).ToList();
	}

	public List<Tuple<DateTime, decimal>> GetDebitsByDay(Account account, DateTime start, DateTime end, object Aux = null)
	{
		List<Tuple<DateTime, decimal>> list = new List<Tuple<DateTime, decimal>>();
		SubsidiaryLedger subsidiaryLedger = null;
		if (Aux == null)
		{
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end);
		}
		else if (Aux.GetType() == typeof(AuxiliaryClass))
		{
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end, (AuxiliaryClass)Aux);
		}
		else
		{
			if (!(Aux.GetType() == typeof(AuxiliaryItem)))
			{
				return list;
			}
			subsidiaryLedger = GetSubsidiaryLedger(account, start, end, (AuxiliaryItem)Aux);
		}
		List<DateTime> list2 = new List<DateTime>();
		foreach (MonthSubsidiaryLedger month in subsidiaryLedger.Months)
		{
			foreach (SubsidiaryLedgerEntry entry in month.Entries)
			{
				if (entry.Voucher.IsDebit && entry.Voucher.Amount > 0m)
				{
					if (list2.Contains(entry.Voucher.Day))
					{
						list[list2.IndexOf(entry.Voucher.Day)] = Tuple.Create(entry.Voucher.Day, list[list2.IndexOf(entry.Voucher.Day)].Item2 + entry.Voucher.Amount);
						continue;
					}
					list.Add(Tuple.Create(entry.Voucher.Day, entry.Voucher.Amount));
					list2.Add(entry.Voucher.Day);
				}
			}
		}
		return list.OrderBy((Tuple<DateTime, decimal> a) => a.Item1).ToList();
	}

	private DateBalance CreateDateBalance()
	{
		return new DateBalance(Accounts.ToDictionary((Account a) => a, (Account a) => new AccountBalance()));
	}

	public List<Account> GetOppositeAccount(Voucher voucher)
	{
		IEnumerable<Voucher> enumerable = Vouchers.Where((Voucher t) => t.Type.Name.Equals(voucher.Type.Name) && t.Number.Equals(voucher.Number) && t.Day.Equals(voucher.Day));
		if (enumerable.Count() <= 1)
		{
			return new List<Account>();
		}
		HashSet<Voucher> hashSet = new HashSet<Voucher>();
		HashSet<Voucher> hashSet2 = new HashSet<Voucher>();
		bool flag = false;
		bool flag2 = isDebit(enumerable.First());
		foreach (Voucher item in enumerable)
		{
			if (isDebit(item) == flag2)
			{
				if (flag)
				{
					if (hashSet.Sum((Voucher v) => v.Amount) == hashSet2.Sum((Voucher v) => v.Amount))
					{
						if (hashSet.Contains(voucher))
						{
							return hashSet2.Select((Voucher v) => topParent(v.Account)).Distinct().ToList();
						}
						if (hashSet2.Contains(voucher))
						{
							return hashSet.Select((Voucher v) => topParent(v.Account)).Distinct().ToList();
						}
						hashSet = new HashSet<Voucher>();
						hashSet2 = new HashSet<Voucher>();
						hashSet.Add(item);
						flag = false;
					}
					else
					{
						hashSet.Add(item);
						flag = false;
					}
				}
				else
				{
					hashSet.Add(item);
				}
			}
			else
			{
				flag = true;
				hashSet2.Add(item);
			}
		}
		if (hashSet.Count > 0)
		{
			if (hashSet.Contains(voucher))
			{
				return hashSet2.Select((Voucher v) => topParent(v.Account)).Distinct().ToList();
			}
			if (hashSet2.Contains(voucher))
			{
				return hashSet.Select((Voucher v) => topParent(v.Account)).Distinct().ToList();
			}
		}
		bool voucherDirection = isDebit(voucher);
		return (from v in enumerable
			where isDebit(v) != voucherDirection
			select topParent(v.Account)).Distinct().ToList();
		static bool isDebit(Voucher cv)
		{
			if (!cv.IsDebit || !(cv.Amount >= 0m))
			{
				if (!cv.IsDebit)
				{
					return cv.Amount < 0m;
				}
				return false;
			}
			return true;
		}
		static Account topParent(Account ac)
		{
			while (ac.Parent != null)
			{
				ac = ac.Parent;
			}
			return ac;
		}
	}
}
