using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public class LedgerMerge
{
	private HashSet<string> fullInheritChild = new HashSet<string>();

	private bool Check(Ledger first, Ledger second)
	{
		DateTime endDate = first.GetEndDate();
		if (second.StartDate.Year != endDate.Year + 1)
		{
			throw new CannotMergeException("账套不合法！必须为连续账套才可以合并");
		}
		Dictionary<string, Account> dictionary = first.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
		Dictionary<string, Account> dictionary2 = second.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
		TrialBalanceSheet trialBalanceSheet = first.GetTrialBalanceSheet(first.StartDate, first.GetEndDate());
		TrialBalanceSheet trialBalanceSheet2 = second.GetTrialBalanceSheet(second.StartDate, second.GetEndDate());
		foreach (Account rootAccount in first.RootAccounts)
		{
			List<Account> list = new List<Account>();
			GetChildren(rootAccount, list);
			foreach (Account item in list)
			{
				decimal total = trialBalanceSheet.End[item].Total;
				bool flag = dictionary2.ContainsKey(item.Code);
				if (!(total == 0m) || flag)
				{
					if (!flag)
					{
						throw new CannotMergeException("科目不一致，不符合合并条件！");
					}
					Account account = dictionary2[item.Code];
					decimal total2 = trialBalanceSheet2.Start[account].Total;
					total = (item.IsDebit ? total : (-total));
					total2 = (account.IsDebit ? total2 : (-total2));
					if (total != total2)
					{
						throw new CannotMergeException("科目余额不一致，不符合合并条件！");
					}
				}
			}
		}
		fullInheritChild = new HashSet<string>();
		foreach (KeyValuePair<string, Account> item2 in dictionary)
		{
			if (item2.Value.Children.Count > 0 || !dictionary2.ContainsKey(item2.Key))
			{
				continue;
			}
			Account account2 = dictionary2[item2.Key];
			Account account3 = null;
			decimal num = default(decimal);
			int num2 = 0;
			foreach (Account child in account2.Children)
			{
				decimal total3 = trialBalanceSheet2.Start[child].Total;
				total3 = (child.IsDebit ? total3 : (-total3));
				if (total3 != 0m)
				{
					num2++;
					account3 = child;
					num = total3;
				}
			}
			if (num2 == 1)
			{
				decimal total4 = trialBalanceSheet.End[item2.Value].Total;
				total4 = (item2.Value.IsDebit ? total4 : (-total4));
				if (num == total4)
				{
					fullInheritChild.Add(account3.Code);
				}
			}
		}
		IEnumerable<string> enumerable = dictionary2.Keys.Except(dictionary.Keys);
		foreach (string item3 in enumerable)
		{
			if (!fullInheritChild.Contains(item3))
			{
				Account key = dictionary2[item3];
				if (trialBalanceSheet2.Start[key].Total != 0m)
				{
					throw new CannotMergeException("新增科目起初数不为零,不符合合并条件！");
				}
			}
		}
		foreach (AuxiliaryClass auxiliaryClass in second.AuxiliaryClasses)
		{
			if (!first.AuxiliaryClasses.Any((AuxiliaryClass a) => a.Code == auxiliaryClass.Code))
			{
				throw new CannotMergeException("辅助核算类别不一致，不符合合并条件！");
			}
		}
		return true;
		static void GetChildren(Account rootAccount, List<Account> children)
		{
			if (rootAccount.Children.Count == 0)
			{
				children.Add(rootAccount);
				return;
			}
			foreach (Account child2 in rootAccount.Children)
			{
				GetChildren(child2, children);
			}
		}
	}

	public Ledger Merge(Ledger first, Ledger second)
	{
		Check(first, second);
		Dictionary<string, Currency> dictionary = first.Currencies.ToDictionary((Currency c) => c.Name, (Currency c) => c);
		foreach (Currency currency in second.Currencies)
		{
			if (!dictionary.ContainsKey(currency.Name))
			{
				first.Currencies.Add(currency);
				dictionary.Add(currency.Name, currency);
			}
		}
		Dictionary<string, VoucherType> dictionary2 = first.VoucherTypes.ToDictionary((VoucherType t) => t.Name, (VoucherType t) => t);
		foreach (VoucherType voucherType in second.VoucherTypes)
		{
			if (!dictionary2.ContainsKey(voucherType.Name))
			{
				first.VoucherTypes.Add(voucherType);
				dictionary2.Add(voucherType.Name, voucherType);
			}
		}
		Dictionary<string, AuxiliaryClass> dictionary3 = first.AuxiliaryClasses.ToDictionary((AuxiliaryClass c) => c.Code, (AuxiliaryClass c) => c);
		List<AuxiliaryClass> list = new List<AuxiliaryClass>();
		foreach (AuxiliaryClass auxiliaryClass3 in second.AuxiliaryClasses)
		{
			if (!dictionary3.ContainsKey(auxiliaryClass3.Code))
			{
				AuxiliaryClass auxiliaryClass = new AuxiliaryClass();
				auxiliaryClass.Code = auxiliaryClass3.Code;
				auxiliaryClass.Name = auxiliaryClass3.Name;
				first.AuxiliaryClasses.Add(auxiliaryClass);
				list.Add(auxiliaryClass3);
				dictionary3.Add(auxiliaryClass.Code, auxiliaryClass);
			}
		}
		Dictionary<string, AuxiliaryItem> dictionary4 = first.AuxiliaryItems.ToDictionary((AuxiliaryItem i) => i.Class.Code + i.Code, (AuxiliaryItem i) => i);
		List<AuxiliaryItem> list2 = new List<AuxiliaryItem>();
		foreach (AuxiliaryItem auxiliaryItem3 in second.AuxiliaryItems)
		{
			if (!dictionary4.ContainsKey(auxiliaryItem3.Class.Code + auxiliaryItem3.Code))
			{
				AuxiliaryItem auxiliaryItem = new AuxiliaryItem();
				auxiliaryItem.Code = auxiliaryItem3.Code;
				auxiliaryItem.Name = auxiliaryItem3.Name;
				auxiliaryItem.Class = dictionary3[auxiliaryItem3.Class.Code];
				first.AuxiliaryItems.Add(auxiliaryItem);
				list2.Add(auxiliaryItem3);
				dictionary4.Add(auxiliaryItem.Class.Code + auxiliaryItem.Code, auxiliaryItem);
			}
		}
		foreach (AuxiliaryClass item3 in list)
		{
			AuxiliaryClass auxiliaryClass2 = dictionary3[item3.Code];
			foreach (AuxiliaryItem item4 in item3.Items)
			{
				auxiliaryClass2.Items.Add(dictionary4[item4.Class.Code + item4.Code]);
			}
		}
		foreach (AuxiliaryItem item5 in list2)
		{
			AuxiliaryItem auxiliaryItem2 = dictionary4[item5.Class.Code + item5.Code];
			auxiliaryItem2.Class = dictionary3[item5.Class.Code];
		}
		Dictionary<int, List<Account>> ledgerLevelDic = GetLedgerLevelDic(first);
		Dictionary<int, List<Account>> ledgerLevelDic2 = GetLedgerLevelDic(second);
		Dictionary<Account, string> dictionary5 = new Dictionary<Account, string>();
		Dictionary<string, Account> dictionary6 = first.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
		TrialBalanceSheet trialBalanceSheet = second.GetTrialBalanceSheet(second.StartDate, second.GetEndDate());
		for (int num = ledgerLevelDic2.Keys.Max(); num > ledgerLevelDic.Keys.Max(); num--)
		{
			foreach (Account secondAccount2 in ledgerLevelDic2[num])
		{
			Account account = new Account();
			account.Ledger = first;
			account.Code = secondAccount2.Code;
			account.Name = secondAccount2.Name;
			account.IsDebit = secondAccount2.IsDebit;
			foreach (Account child in secondAccount2.Children)
			{
				account.Children.Add(dictionary6[child.Code]);
			}
			first.Accounts.Add(account);
			dictionary6.Add(account.Code, account);
			dictionary5.Add(account, secondAccount2.Parent.Code);
		}
		}
		foreach (KeyValuePair<int, List<Account>> item6 in ledgerLevelDic.OrderByDescending((KeyValuePair<int, List<Account>> i) => i.Key))
		{
			if (!ledgerLevelDic2.ContainsKey(item6.Key))
			{
				continue;
			}
			Dictionary<string, Account> dictionary7 = item6.Value.ToDictionary((Account a) => a.Code, (Account a) => a);
			foreach (Account secondAccount in ledgerLevelDic2[item6.Key])
			{
				if (dictionary7.ContainsKey(secondAccount.Code))
				{
					continue;
				}
				Account account2 = new Account();
				account2.Ledger = first;
				account2.Code = secondAccount.Code;
				account2.Name = secondAccount.Name;
				account2.IsDebit = secondAccount.IsDebit;
				foreach (Account child2 in secondAccount.Children)
			{
				account2.Children.Add(dictionary6[child2.Code]);
			}
			first.Accounts.Add(account2);
				item6.Value.Add(account2);
				dictionary7.Add(account2.Code, account2);
				dictionary6.Add(account2.Code, account2);
				if (secondAccount.Parent != null)
				{
					dictionary5.Add(account2, secondAccount.Parent.Code);
				}
			}
		}
		foreach (KeyValuePair<Account, string> item7 in dictionary5)
		{
			string value = item7.Value;
			Account key = item7.Key;
			if (value == null)
			{
				key.Parent = null;
			}
			else
			{
				key.Parent = dictionary6[value];
			}
		}
		foreach (string item8 in fullInheritChild)
		{
			Account account3 = dictionary6[item8];
			Account parent = account3.Parent;
			if (!first.InitialBalance.TryGetValue(account3, out var value2))
			{
				value2 = new AccountBalance();
				first.InitialBalance.Add(account3, value2);
			}
			value2.Total = first.InitialBalance[parent].Total;
		}
		Dictionary<string, Account> dictionary8 = first.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
		foreach (string item9 in fullInheritChild)
		{
			Account account4 = dictionary6[item9];
			Account accParent = account4.Parent;
			IEnumerable<Voucher> enumerable = first.Vouchers.Where((Voucher v) => v.Account == accParent);
			foreach (Voucher item10 in enumerable)
			{
				item10.Account = account4;
			}
		}
		foreach (Voucher voucher2 in second.Vouchers)
		{
			Voucher voucher = new Voucher();
			voucher.Id = voucher2.Id;
			voucher.Number = voucher2.Number;
			voucher.Day = voucher2.Day;
			voucher.Digest = voucher2.Digest;
			voucher.Amount = voucher2.Amount;
			voucher.Maker = voucher2.Maker;
			voucher.Booker = voucher2.Booker;
			voucher.Checker = voucher2.Checker;
			voucher.IsDebit = voucher2.IsDebit;
			voucher.Quantity = voucher2.Quantity;
			voucher.UnitPrice = voucher2.UnitPrice;
			voucher.NumAttachments = voucher2.NumAttachments;
			voucher.ExchangeRate = voucher2.ExchangeRate;
			voucher.ForeignAmount = voucher2.ForeignAmount;
			voucher.Type = dictionary2[voucher2.Type.Name];
			voucher.Account = dictionary8[voucher2.Account.Code];
			voucher.Currency = dictionary[voucher2.Currency.Name];
			foreach (Account oppositeAccount in voucher2.OppositeAccounts)
			{
				if (dictionary8.ContainsKey(oppositeAccount.Code))
				{
					Account item = dictionary8[oppositeAccount.Code];
					voucher.OppositeAccounts.Add(item);
				}
			}
			foreach (AuxiliaryItem detail in voucher2.Details)
			{
				AuxiliaryItem item2 = dictionary4[detail.Class.Code + detail.Code];
				voucher.Details.Add(item2);
			}
			first.Vouchers.Add(voucher);
		}
		return first;
	}

	private Dictionary<int, List<Account>> GetLedgerLevelDic(Ledger ledger)
	{
		Dictionary<int, List<Account>> ret = new Dictionary<int, List<Account>>();
		foreach (Account rootAccount in ledger.RootAccounts)
		{
			PutAccountLevelDic(rootAccount, 1);
		}
		return ret;
		void PutAccountLevelDic(Account account, int level)
		{
			if (!ret.ContainsKey(level))
			{
				ret.Add(level, new List<Account>());
			}
			ret[level].Add(account);
			foreach (Account child in account.Children)
			{
				PutAccountLevelDic(child, level + 1);
			}
		}
	}
}

