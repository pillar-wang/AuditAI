﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Leqisoft.Model;

namespace LedgerImport;

public class LedgerBuilder3
{
	public static char[] splitChars = new char[16]
	{
		' ', '-', '—', ':', '：', ',', '，', '.', '/', '\\',
		'_', '|', '－', '—', '_', '\uff3f'
	};

	public static char[] prefixChars = new char[16]
	{
		'(', ')', '（', '）', '[', ']', '【', '】', '{', '}',
		'｛', '｝', '<', '>', '＜', '＞'
	};

	public static Ledger EMPTY_LEDGER = null;

	private Ledger _ledger;

	private Dictionary<DataRow, AccData> PreAccountDataAll = new Dictionary<DataRow, AccData>();

	private HashSet<Account> BalanceAccountSet = new HashSet<Account>();

	private HashSet<Account> VoucherAccountSet = new HashSet<Account>();

	private HashSet<Account> AuxiliaryAccountSet = new HashSet<Account>();

	private Dictionary<DataRow, AuxData> PreAuxDataBalance = new Dictionary<DataRow, AuxData>();

	private Dictionary<DataRow, AuxData> PreAuxDataVoucher = new Dictionary<DataRow, AuxData>();

	private Dictionary<string, Dictionary<string, Item>> tempAuxType = new Dictionary<string, Dictionary<string, Item>>();

	public DataSource DataSource { get; set; }

	public event EventHandler<string> ProgressChanged;

	public Ledger GetLedger(string bwb)
	{
		OnProgressChanged("正在准备处理数据");
		Initialize(bwb);
		OnProgressChanged("正在读取科目信息");
		PreBuildAccData();
		BuildAccounts();
		OnProgressChanged("正在读取科目余额");
		PreBuildAuxData();
		BuildBalances();
		OnProgressChanged("正在读取辅助核算");
		BuildAuxBalance();
		OnProgressChanged("正在读取会计凭证");
		BuildVoucher();
		OnProgressChanged("账套数据读取完成");
		Finish();
		return _ledger;
	}

	public Ledger GetAccounts(string bwb = null)
	{
		Initialize(bwb);
		buildAccounts();
		buildBalances();
		return _ledger;
		void buildAccounts()
		{
			DataTable balanceTable2 = DataSource.BalanceTable;
			Dictionary<string, Account> dictionary5 = new Dictionary<string, Account>();
			DataColumn column4 = FindColumn(balanceTable2, "科目代码", throwExceptionWhenNotFound: true);
			DataColumn column5 = FindColumn(balanceTable2, "科目名称", throwExceptionWhenNotFound: true);
			for (int k = 0; k < balanceTable2.Rows.Count; k++)
			{
				DataRow dataRow2 = balanceTable2.Rows[k];
				string text13 = dataRow2[column4]?.ToString()?.Trim();
				string name = dataRow2[column5]?.ToString()?.Trim();
				if (!string.IsNullOrEmpty(text13) && !dictionary5.ContainsKey(text13))
				{
					Account account3 = new Account
					{
						Code = text13,
						Name = name,
						IsDebit = true
					};
					_ledger.Accounts.Add(account3);
					dictionary5.Add(text13, account3);
				}
			}
			foreach (KeyValuePair<DataRow, AccData> item in PreAccountDataAll)
			{
				AccData value2 = item.Value;
				if (!string.IsNullOrEmpty(value2.AccCode) && !dictionary5.ContainsKey(value2.AccCode))
				{
					Account account4 = new Account
					{
						Code = value2.AccCode,
						Name = value2.AccName,
						IsDebit = true
					};
					_ledger.Accounts.Add(account4);
					dictionary5.Add(value2.AccCode, account4);
				}
			}
			if (dictionary5.Count != 0)
			{
				if (AccountHasSplitter(_ledger))
				{
					buildAccountsLevelWithSplitterImpl(_ledger);
				}
				else
				{
					buildAccountsLevelNonSplitterImpl(_ledger);
				}
			}
		}
		void buildAccountsLevelNonSplitterImpl(Ledger ledger)
		{
			IEnumerable<IGrouping<int, Account>> source = from a in ledger.Accounts
				group a by a.Code.Length;
			Dictionary<int, List<Account>> source2 = source.OrderBy((IGrouping<int, Account> g) => g.Key).ToDictionary((IGrouping<int, Account> g) => g.Key, (IGrouping<int, Account> g) => g.ToList());
			List<KeyValuePair<int, List<Account>>> list = source2.ToList();
			bool flag = true;
			for (int num4 = list.Count - 1; num4 > 0; num4--)
			{
				KeyValuePair<int, List<Account>> keyValuePair = list[num4 - 1];
				Dictionary<string, Account> dictionary2 = keyValuePair.Value.ToDictionary((Account a) => a.Code, (Account a) => a);
				foreach (Account item2 in list[num4].Value)
				{
					string key = item2.Code.Substring(0, keyValuePair.Key);
					if (!dictionary2.ContainsKey(key))
					{
						flag = false;
						break;
					}
				}
			}
			char c = '-';
			if (!flag)
			{
				int num5 = 0;
				IEnumerable<string> source3 = ledger.Accounts.Select((Account a) => a.Name);
				char[] array = splitChars;
				foreach (char ch in array)
				{
					int num6 = source3.Sum((string n) => n.Count((char cn) => cn == ch));
					if (num6 > num5)
					{
						c = ch;
						num5 = num6;
					}
				}
			}
			list = source2.OrderBy((KeyValuePair<int, List<Account>> m) => m.Key).ToList();
			for (int num7 = list.Count - 1; num7 > 0; num7--)
			{
				KeyValuePair<int, List<Account>> keyValuePair2 = list[num7 - 1];
				Dictionary<string, Account> dictionary3 = keyValuePair2.Value.ToDictionary((Account a) => a.Code, (Account a) => a);
				foreach (Account item3 in list[num7].Value)
				{
					string parentCode = item3.Code.Substring(0, keyValuePair2.Key);
					Account account2;
					if (dictionary3.ContainsKey(parentCode))
					{
						account2 = dictionary3[parentCode];
						account2.Children.Add(item3);
						item3.Parent = account2;
					}
					else
					{
						string text2 = string.Empty;
						string[] array2 = item3.Name.Split(new char[1] { c }, StringSplitOptions.RemoveEmptyEntries);
						if (array2.Length > num7)
						{
							text2 = ((array2.Length == num7 + 1) ? array2[num7] : string.Join(c.ToString(), array2.Skip(num7)));
						}
						else
						{
							IEnumerable<Account> enumerable = list[num7].Value.Where((Account t) => t.Code.StartsWith(parentCode));
							foreach (Account item4 in enumerable)
							{
								array2 = item4.Name.Split(new char[1] { c }, StringSplitOptions.RemoveEmptyEntries);
								if (array2.Length > num7)
								{
									text2 = ((array2.Length == num7 + 1) ? array2[num7] : string.Join(c.ToString(), array2.Skip(num7)));
									break;
								}
							}
						}
						if (string.IsNullOrEmpty(text2))
						{
							continue;
						}
						account2 = new Account
						{
							Code = parentCode,
							Name = text2,
							IsDebit = true
						};
						_ledger.Accounts.Add(account2);
						account2.Children.Add(item3);
						item3.Parent = account2;
						dictionary3.Add(account2.Code, account2);
						keyValuePair2.Value.Add(account2);
					}
					string text3 = item3.Name.Trim(splitChars);
					string text4 = account2.Name.Trim(splitChars);
					string text5 = null;
					if (text3.StartsWith(text4) && !string.IsNullOrWhiteSpace(text5 = text3.Remove(0, text4.Length)) && splitChars.Contains(text5[0]) && !string.IsNullOrWhiteSpace(text5 = text5.Trim(splitChars)))
					{
						item3.Name = text5;
					}
				}
			}
		}
		void buildAccountsLevelWithSplitterImpl(Ledger ledger)
		{
			IEnumerable<Tuple<Account, string[]>> source4 = ledger.Accounts.Select((Account a) => Tuple.Create(a, RemovePrefixChars(a.Code).Split(splitChars, StringSplitOptions.RemoveEmptyEntries)));
			Dictionary<int, List<Tuple<Account, string[]>>> source5 = (from t in source4
				group t by t.Item2.Length into g
				orderby g.Key
				select g).ToDictionary((IGrouping<int, Tuple<Account, string[]>> g) => g.Key, (IGrouping<int, Tuple<Account, string[]>> g) => g.ToList());
			List<KeyValuePair<int, List<Tuple<Account, string[]>>>> list2 = source5.ToList();
			for (int num8 = list2.Count - 1; num8 > 0; num8--)
			{
				KeyValuePair<int, List<Tuple<Account, string[]>>> keyValuePair3 = list2[num8 - 1];
				Dictionary<string, Account> dictionary4 = keyValuePair3.Value.ToDictionary((Tuple<Account, string[]> a) => a.Item1.Code, (Tuple<Account, string[]> a) => a.Item1);
				foreach (Tuple<Account, string[]> item5 in list2[num8].Value)
				{
					string parentCode2 = item5.Item1.Code.Substring(0, item5.Item1.Code.LastIndexOfAny(splitChars));
					if (dictionary4.TryGetValue(parentCode2, out var value))
					{
						value.Children.Add(item5.Item1);
						item5.Item1.Parent = value;
					}
					else
					{
						string text6 = item5.Item1.Code.TrimEnd(splitChars);
						string text7 = item5.Item2.Last();
						string text8 = item5.Item1.Name.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
						string text9 = item5.Item1.Name.Remove(item5.Item1.Name.Length - text8.Length, text8.Length).TrimEnd(splitChars);
						if (string.IsNullOrEmpty(text9))
						{
							IEnumerable<Tuple<Account, string[]>> enumerable2 = list2[num8].Value.Where((Tuple<Account, string[]> t) => t.Item1.Code.StartsWith(parentCode2));
							foreach (Tuple<Account, string[]> item6 in enumerable2)
							{
								text8 = item6.Item1.Name.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
								text9 = item6.Item1.Name.Remove(item6.Item1.Name.Length - text8.Length, text8.Length).TrimEnd(splitChars);
								if (!string.IsNullOrEmpty(text9))
								{
									break;
								}
							}
						}
						if (string.IsNullOrEmpty(text9))
						{
							continue;
						}
						value = new Account
						{
							Code = parentCode2,
							Name = text9,
							IsDebit = true
						};
						_ledger.Accounts.Add(value);
						value.Children.Add(item5.Item1);
						item5.Item1.Parent = value;
						dictionary4.Add(value.Code, value);
						keyValuePair3.Value.Add(Tuple.Create(value, RemovePrefixChars(value.Code).Split(splitChars, StringSplitOptions.RemoveEmptyEntries)));
					}
					string text10 = item5.Item1.Name.Trim(splitChars);
					string text11 = value.Name.Trim(splitChars);
					string text12 = null;
					if (text10.StartsWith(text11) && !string.IsNullOrWhiteSpace(text12 = text10.Remove(0, text11.Length)) && splitChars.Contains(text12[0]) && !string.IsNullOrWhiteSpace(text12 = text12.Trim(splitChars)))
					{
						item5.Item1.Name = text12;
					}
				}
			}
		}
		void buildBalances()
		{
			DataTable balanceTable = DataSource.BalanceTable;
			DataColumn column = FindColumn(balanceTable, "科目代码", throwExceptionWhenNotFound: true);
			DataColumn dataColumn = FindColumn(balanceTable, "科目名称", throwExceptionWhenNotFound: true);
			DataColumn column2 = FindColumn(balanceTable, "年初借方余额", throwExceptionWhenNotFound: true);
			DataColumn column3 = FindColumn(balanceTable, "年初贷方余额", throwExceptionWhenNotFound: true);
			Dictionary<string, Account> dictionary = _ledger.Accounts.ToDictionary((Account t) => t.Code, (Account t) => t);
			for (int i = 0; i < balanceTable.Rows.Count; i++)
			{
				DataRow dataRow = balanceTable.Rows[i];
				string text = dataRow[column]?.ToString()?.Trim();
				if (!string.IsNullOrEmpty(text) && dictionary.ContainsKey(text))
				{
					Account account = dictionary[text];
					object obj = dataRow[column2];
					object obj2 = dataRow[column3];
					decimal result;
					decimal num = (decimal.TryParse(obj?.ToString(), out result) ? result : 0m);
					decimal result2;
					decimal num2 = (decimal.TryParse(obj2?.ToString(), out result2) ? result2 : 0m);
					decimal num3 = num - num2;
					if (num3 >= 0m)
					{
						account.IsDebit = true;
						account.Balance = num3;
					}
					else
					{
						account.IsDebit = false;
						account.Balance = -num3;
					}
				}
			}
		}
	}

	private void Initialize(string bwb)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension("（默认公司名称）");
		_ledger = new Ledger(new LedgerInfo
		{
			LedgerNumber = "（默认账套号）",
			CompanyName = fileNameWithoutExtension,
			Year = 2020
		});
		_ledger.BaseCurrency = new Currency
		{
			Name = bwb
		};
		_ledger.ForeignCurrencies.Add(_ledger.BaseCurrency);
	}

	private void PreBuildAccData()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		DataTable balanceTable = DataSource.BalanceTable;
		if (balanceTable != null)
		{
			DataColumn column = FindColumn(balanceTable, "科目代码", throwExceptionWhenNotFound: true);
			DataColumn column2 = FindColumn(balanceTable, "科目名称", throwExceptionWhenNotFound: true);
			foreach (DataRow row in balanceTable.Rows)
			{
				string text = row[column]?.ToString()?.Trim();
				string text2 = row[column2]?.ToString()?.Trim();
				PreAccountDataAll.Add(row, new AccData(text, text2));
				if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
				{
					if (!dictionary.ContainsKey(text))
					{
						dictionary.Add(text, text2);
					}
					if (!dictionary2.ContainsKey(text2))
					{
						dictionary2.Add(text2, text);
					}
				}
			}
		}
		DataTable auxiliaryTable = DataSource.AuxiliaryTable;
		if (auxiliaryTable != null)
		{
			DataColumn column3 = FindColumn(auxiliaryTable, "科目代码", throwExceptionWhenNotFound: true);
			DataColumn column4 = FindColumn(auxiliaryTable, "科目名称", throwExceptionWhenNotFound: true);
			string text3 = null;
			string text4 = null;
			foreach (DataRow row2 in auxiliaryTable.Rows)
			{
				string text5 = row2[column3]?.ToString()?.Trim();
				text5 = (string.IsNullOrEmpty(text5) ? text3 : text5);
				text3 = text5;
				string text6 = row2[column4]?.ToString()?.Trim();
				text6 = (string.IsNullOrEmpty(text6) ? text4 : text6);
				text4 = text6;
				PreAccountDataAll.Add(row2, new AccData(text5, text6));
				if (!string.IsNullOrEmpty(text5) && !string.IsNullOrEmpty(text6))
				{
					if (!dictionary.ContainsKey(text5))
					{
						dictionary.Add(text5, text6);
					}
					if (!dictionary2.ContainsKey(text6))
					{
						dictionary2.Add(text6, text5);
					}
				}
			}
		}
		DataTable voucherTable = DataSource.VoucherTable;
		if (voucherTable != null)
		{
			DataColumn column5 = FindColumn(voucherTable, "科目代码", throwExceptionWhenNotFound: true);
			DataColumn column6 = FindColumn(voucherTable, "科目名称", throwExceptionWhenNotFound: true);
			foreach (DataRow row3 in voucherTable.Rows)
			{
				string text7 = row3[column5]?.ToString()?.Trim();
				string text8 = row3[column6]?.ToString()?.Trim();
				PreAccountDataAll.Add(row3, new AccData(text7, text8));
				if (!string.IsNullOrEmpty(text7) && !string.IsNullOrEmpty(text8))
				{
					if (!dictionary.ContainsKey(text7))
					{
						dictionary.Add(text7, text8);
					}
					if (!dictionary2.ContainsKey(text8))
					{
						dictionary2.Add(text8, text7);
					}
				}
			}
		}
		foreach (KeyValuePair<DataRow, AccData> item in PreAccountDataAll)
		{
			AccData value = item.Value;
			if (string.IsNullOrEmpty(value.AccCode))
			{
				if (!string.IsNullOrEmpty(value.AccName) && dictionary2.ContainsKey(value.AccName))
				{
					value.AccCode = dictionary2[value.AccName];
				}
			}
			else if (string.IsNullOrEmpty(value.AccName) && !string.IsNullOrEmpty(value.AccCode) && dictionary.ContainsKey(value.AccCode))
			{
				value.AccName = dictionary[value.AccCode];
			}
		}
	}

	private void BuildAccounts()
	{
		DataTable balanceTable = DataSource.BalanceTable;
		DataTable voucherTable = DataSource.VoucherTable;
		Dictionary<string, Account> dictionary = new Dictionary<string, Account>();
		DataColumn col = FindColumn(balanceTable, "科目代码", throwExceptionWhenNotFound: true);
		DataColumn col2 = FindColumn(balanceTable, "科目名称", throwExceptionWhenNotFound: true);
		for (int i = 0; i < balanceTable.Rows.Count; i++)
		{
			DataRow key = balanceTable.Rows[i];
			string accCode = PreAccountDataAll[key].AccCode;
			string accName = PreAccountDataAll[key].AccName;
			if (string.IsNullOrEmpty(accCode))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.BalanceAccountCodeEmpty,
					FailureContext = new FailureContext
					{
						Table = TableEnum.BALANCE,
						RowTag = balanceTable.GetTag(balanceTable.Rows[i]),
						ColTag = balanceTable.GetTag(col)
					}
				};
			}
			if (string.IsNullOrEmpty(accName))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.AccountNameEmpty,
					FailureContext = new FailureContext
					{
						Table = TableEnum.BALANCE,
						RowTag = balanceTable.GetTag(balanceTable.Rows[i]),
						ColTag = balanceTable.GetTag(col2)
					}
				};
			}
			if (dictionary.ContainsKey(accCode))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.RepeatAccountCode,
					FailureContext = new FailureContext
					{
						Table = TableEnum.BALANCE,
						RowTag = balanceTable.GetTag(balanceTable.Rows[i]),
						ColTag = balanceTable.GetTag(col),
						UserData = accCode
					}
				};
			}
			Account account = new Account();
			account.Code = accCode;
			account.Name = accName;
			account.IsDebit = true;
			_ledger.Accounts.Add(account);
			dictionary.Add(accCode, account);
			BalanceAccountSet.Add(account);
		}
		int count = _ledger.Accounts.Count;
		bool flag = false;
		try
		{
			flag = AccountHasSplitter(_ledger);
			if (flag)
			{
				BuildBalanceAccountsLevelWithSplitterImpl(_ledger, (Account a) => true);
			}
			else
			{
				BuildBalanceAccountsLevelNonSplitterImpl(_ledger, (Account a) => true);
			}
		}
		catch (ImportException2 importException) when (importException.FailureReason == FailureReasonEnum.AccountParentCannotCreateBecauseName)
		{
			string parentCode2 = importException.FailureContext.UserData.ToString();
			DataRow dataRow = FindRowByCode(balanceTable, (string code) => code?.StartsWith(parentCode2) ?? false, withPreData: true);
			if (dataRow != null)
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.NotFoundParentAccount,
					FailureContext = new FailureContext
					{
						Table = TableEnum.BALANCE,
						RowTag = balanceTable.GetTag(dataRow),
						ColTag = balanceTable.GetTag(FindColumn(balanceTable, "科目名称", throwExceptionWhenNotFound: false)),
						UserData = parentCode2
					}
				};
			}
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.SpecificMessage,
				FailureContext = new FailureContext
				{
					UserData = "无法生成科目级次信息，请检查科目代码或科目名称是否符合生成规则"
				}
			};
		}
		int count2 = _ledger.Accounts.Count;
		_ledger.Accounts.ForEach(delegate(Account a)
		{
			BalanceAccountSet.Add(a);
		});
		PreBuildAccData2();
		for (int j = count; j < count2; j++)
		{
			Account account2 = _ledger.Accounts[j];
			if (!dictionary.ContainsKey(account2.Code))
			{
				dictionary.Add(account2.Code, account2);
			}
		}
		foreach (KeyValuePair<DataRow, AccData> item in PreAccountDataAll)
		{
			AccData value = item.Value;
			if (!string.IsNullOrEmpty(value.AccCode) && !dictionary.ContainsKey(value.AccCode) && !string.IsNullOrEmpty(value.AccName))
			{
				Account account3 = new Account();
				account3.Code = value.AccCode;
				account3.Name = value.AccName;
				account3.IsDebit = true;
				_ledger.Accounts.Add(account3);
				dictionary.Add(value.AccCode, account3);
				if (item.Key.Table == DataSource.AuxiliaryTable)
				{
					AuxiliaryAccountSet.Add(account3);
				}
				else if (item.Key.Table == DataSource.VoucherTable)
				{
					VoucherAccountSet.Add(account3);
				}
			}
		}
		try
		{
			if (flag)
			{
				BuildBalanceAccountsLevelWithSplitterImpl(_ledger, (Account a) => !BalanceAccountSet.Contains(a));
			}
			else
			{
				IEnumerable<AccData> source = from kv in PreAccountDataAll
					where kv.Key.Table == DataSource.BalanceTable
					select kv.Value;
				List<IGrouping<int, AccData>> list = (from d in source
					group d by d.AccCode.Length into g
					orderby g.Key
					select g).ToList();
				HashSet<int> hashSet = new HashSet<int>(list.Select((IGrouping<int, AccData> g) => g.Key));
				if (list.Count > 0)
				{
					int num = list.Max((IGrouping<int, AccData> g) => g.Key);
					foreach (KeyValuePair<DataRow, AccData> item2 in PreAccountDataAll.Where((KeyValuePair<DataRow, AccData> kv) => kv.Key.Table == DataSource.AuxiliaryTable || kv.Key.Table == DataSource.VoucherTable))
					{
						int length = item2.Value.AccCode.Length;
						if (length < num && !hashSet.Contains(length))
						{
							throw new ImportException2
							{
								FailureReason = FailureReasonEnum.AccCodeNotBeInBalanceRule,
								FailureContext = ((item2.Key.Table == DataSource.AuxiliaryTable) ? new FailureContext
								{
									Table = TableEnum.AUXILIARY,
									RowTag = DataSource.AuxiliaryTable.GetTag(item2.Key),
									ColTag = DataSource.AuxiliaryTable.GetTag(FindColumn(DataSource.AuxiliaryTable, "科目代码", throwExceptionWhenNotFound: false))
								} : new FailureContext
								{
									Table = TableEnum.VOUCHER,
									RowTag = DataSource.VoucherTable.GetTag(item2.Key),
									ColTag = DataSource.VoucherTable.GetTag(FindColumn(DataSource.VoucherTable, "科目代码", throwExceptionWhenNotFound: false))
								})
							};
						}
					}
				}
				BuildBalanceAccountsLevelNonSplitterImpl(_ledger, (Account a) => !BalanceAccountSet.Contains(a));
			}
		}
		catch (ImportException2 importException2) when (importException2.FailureReason == FailureReasonEnum.AccountParentCannotCreateBecauseName)
		{
			string parentCode = importException2.FailureContext.UserData.ToString();
			if (DataSource.VoucherTable != null)
			{
				DataRow dataRow2 = FindRowByCode(DataSource.VoucherTable, (string code) => code?.StartsWith(parentCode) ?? false, withPreData: true);
				if (dataRow2 != null)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AccountParentCannotCreateBecauseName,
						FailureContext = new FailureContext
						{
							Table = TableEnum.VOUCHER,
							RowTag = DataSource.VoucherTable.GetTag(dataRow2),
							ColTag = DataSource.VoucherTable.GetTag(FindColumn(DataSource.VoucherTable, "科目名称", throwExceptionWhenNotFound: false)),
							UserData = parentCode
						}
					};
				}
			}
			if (DataSource.AuxiliaryTable != null)
			{
				DataRow dataRow3 = FindRowByCode(DataSource.AuxiliaryTable, (string code) => code?.StartsWith(parentCode) ?? false, withPreData: true);
				if (dataRow3 != null)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AccountParentCannotCreateBecauseName,
						FailureContext = new FailureContext
						{
							Table = TableEnum.AUXILIARY,
							RowTag = DataSource.AuxiliaryTable.GetTag(dataRow3),
							ColTag = DataSource.AuxiliaryTable.GetTag(FindColumn(DataSource.AuxiliaryTable, "科目名称", throwExceptionWhenNotFound: false)),
							UserData = parentCode
						}
					};
				}
			}
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.SpecificMessage,
				FailureContext = new FailureContext
				{
					UserData = "无法生成科目级次信息，请检查科目代码或科目名称是否符合生成规则"
				}
			};
		}
		if (dictionary.Count == 0)
		{
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.NotFoundAnyAccount
			};
		}
	}

	private DataRow FindRowByCode(DataTable table, Predicate<string> predicate, bool withPreData)
	{
		DataColumn dataColumn = FindColumn(table, "科目代码", throwExceptionWhenNotFound: false);
		if (dataColumn == null)
		{
			return null;
		}
		for (int i = 0; i < table.Rows.Count; i++)
		{
			DataRow dataRow = table.Rows[i];
			string obj = (withPreData ? PreAccountDataAll[dataRow].AccCode : dataRow[dataColumn]?.ToString());
			if (predicate(obj))
			{
				return dataRow;
			}
		}
		return null;
	}

	protected virtual void BuildBalanceAccountsLevelWithSplitterImpl(Ledger ledger, Predicate<Account> predicate)
	{
		IEnumerable<Tuple<Account, string[]>> source = ledger.Accounts.Select((Account a) => Tuple.Create(a, RemovePrefixChars(a.Code).Split(splitChars, StringSplitOptions.RemoveEmptyEntries)));
		Dictionary<int, List<Tuple<Account, string[]>>> source2 = (from t in source
			group t by t.Item2.Length into g
			orderby g.Key
			select g).ToDictionary((IGrouping<int, Tuple<Account, string[]>> g) => g.Key, (IGrouping<int, Tuple<Account, string[]>> g) => g.ToList());
		List<KeyValuePair<int, List<Tuple<Account, string[]>>>> list = source2.ToList();
		for (int num = list.Count - 1; num > 0; num--)
		{
			KeyValuePair<int, List<Tuple<Account, string[]>>> keyValuePair = list[num - 1];
			Dictionary<string, Account> dictionary = keyValuePair.Value.ToDictionary((Tuple<Account, string[]> a) => a.Item1.Code, (Tuple<Account, string[]> a) => a.Item1);
			foreach (Tuple<Account, string[]> item in list[num].Value)
			{
				if (!predicate(item.Item1))
				{
					continue;
				}
				string parentCode = item.Item1.Code.Substring(0, item.Item1.Code.LastIndexOfAny(splitChars));
				if (dictionary.TryGetValue(parentCode, out var value))
				{
					value.Children.Add(item.Item1);
					item.Item1.Parent = value;
				}
				else
				{
					string text = item.Item1.Code.TrimEnd(splitChars);
					string text2 = item.Item2.Last();
					string text3 = item.Item1.Name.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
					string text4 = item.Item1.Name.Remove(item.Item1.Name.Length - text3.Length, text3.Length).TrimEnd(splitChars);
					if (string.IsNullOrEmpty(text4))
					{
						IEnumerable<Tuple<Account, string[]>> enumerable = list[num].Value.Where((Tuple<Account, string[]> t) => t.Item1.Code.StartsWith(parentCode));
						foreach (Tuple<Account, string[]> item2 in enumerable)
						{
							text3 = item2.Item1.Name.Split(splitChars, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
							text4 = item2.Item1.Name.Remove(item2.Item1.Name.Length - text3.Length, text3.Length).TrimEnd(splitChars);
							if (!string.IsNullOrEmpty(text4))
							{
								break;
							}
						}
					}
					if (string.IsNullOrEmpty(text4))
					{
						throw new ImportException2
						{
							FailureReason = FailureReasonEnum.AccountParentCannotCreateBecauseName,
							FailureContext = new FailureContext
							{
								UserData = item.Item1.Code
							}
						};
					}
					value = new Account();
					value.Code = parentCode;
					value.Name = text4;
					value.IsDebit = true;
					value.Children.Add(item.Item1);
					item.Item1.Parent = value;
					_ledger.Accounts.Add(value);
					dictionary.Add(value.Code, value);
					keyValuePair.Value.Add(Tuple.Create(value, RemovePrefixChars(value.Code).Split(splitChars, StringSplitOptions.RemoveEmptyEntries)));
				}
				string text5 = item.Item1.Name.Trim(splitChars);
				string text6 = value.Name.Trim(splitChars);
				string text7 = null;
				if (text5.StartsWith(text6) && !string.IsNullOrWhiteSpace(text7 = text5.Remove(0, text6.Length)) && splitChars.Contains(text7[0]) && !string.IsNullOrWhiteSpace(text7 = text7.Trim(splitChars)))
				{
					item.Item1.Name = text7;
				}
			}
		}
	}

	protected virtual void BuildBalanceAccountsLevelNonSplitterImpl(Ledger ledger, Predicate<Account> predicate)
	{
		IEnumerable<IGrouping<int, Account>> source = from a in ledger.Accounts
			group a by a.Code.Length;
		Dictionary<int, List<Account>> source2 = source.OrderBy((IGrouping<int, Account> g) => g.Key).ToDictionary((IGrouping<int, Account> g) => g.Key, (IGrouping<int, Account> g) => g.ToList());
		List<KeyValuePair<int, List<Account>>> list = source2.ToList();
		bool flag = true;
		for (int num = list.Count - 1; num > 0; num--)
		{
			KeyValuePair<int, List<Account>> keyValuePair = list[num - 1];
			Dictionary<string, Account> dictionary = keyValuePair.Value.ToDictionary((Account a) => a.Code, (Account a) => a);
			foreach (Account item in list[num].Value)
			{
				if (predicate(item))
				{
					string key = item.Code.Substring(0, keyValuePair.Key);
					if (!dictionary.ContainsKey(key))
					{
						flag = false;
						break;
					}
				}
			}
		}
		char c = '-';
		if (!flag)
		{
			int num2 = 0;
			IEnumerable<string> source3 = ledger.Accounts.Select((Account a) => a.Name);
			char[] array = splitChars;
			foreach (char ch in array)
			{
				int num3 = source3.Sum((string n) => n.Count((char cn) => cn == ch));
				if (num3 > num2)
				{
					c = ch;
					num2 = num3;
				}
			}
		}
		for (int num4 = list.Count - 1; num4 > 0; num4--)
		{
			KeyValuePair<int, List<Account>> keyValuePair2 = list[num4 - 1];
			Dictionary<string, Account> dictionary2 = keyValuePair2.Value.ToDictionary((Account a) => a.Code, (Account a) => a);
			foreach (Account item2 in list[num4].Value)
			{
				if (!predicate(item2))
				{
					continue;
				}
				string parentCode = item2.Code.Substring(0, keyValuePair2.Key);
				Account account;
				if (dictionary2.ContainsKey(parentCode))
				{
					account = dictionary2[parentCode];
					account.Children.Add(item2);
					item2.Parent = account;
				}
				else
				{
					string text = string.Empty;
					string[] array2 = item2.Name.Split(new char[1] { c }, StringSplitOptions.RemoveEmptyEntries);
					if (array2.Length > num4)
					{
						text = string.Join(c.ToString(), array2.Take(num4));
					}
					else
					{
						IEnumerable<Account> enumerable = list[num4].Value.Where((Account t) => t.Code.StartsWith(parentCode));
						foreach (Account item3 in enumerable)
						{
							array2 = item3.Name.Split(new char[1] { c }, StringSplitOptions.RemoveEmptyEntries);
							if (array2.Length > num4)
							{
								text = string.Join(c.ToString(), array2.Take(num4));
								break;
							}
						}
					}
					if (string.IsNullOrEmpty(text))
					{
						throw new ImportException2
						{
							FailureReason = FailureReasonEnum.AccountParentCannotCreateBecauseName,
							FailureContext = new FailureContext
							{
								UserData = item2.Code
							}
						};
					}
					account = new Account();
					account.Code = parentCode;
					account.Name = text;
					account.IsDebit = true;
					account.Children.Add(item2);
					item2.Parent = account;
					_ledger.Accounts.Add(account);
					dictionary2.Add(account.Code, account);
					keyValuePair2.Value.Add(account);
				}
				string text2 = item2.Name.Trim(splitChars);
				string text3 = account.Name.Trim(splitChars);
				string text4 = null;
				if (text2.StartsWith(text3) && !string.IsNullOrWhiteSpace(text4 = text2.Remove(0, text3.Length)) && splitChars.Contains(text4[0]) && !string.IsNullOrWhiteSpace(text4 = text4.Trim(splitChars)))
				{
					item2.Name = text4;
				}
			}
		}
	}

	private void PreBuildAccData2()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (Account account in _ledger.Accounts)
		{
			string code = account.Code;
			string fullName = GetFullName(account);
			if (!dictionary.ContainsKey(fullName))
			{
				dictionary.Add(fullName, code);
			}
		}
		IEnumerable<KeyValuePair<DataRow, AccData>> enumerable = PreAccountDataAll.Where((KeyValuePair<DataRow, AccData> kv) => string.IsNullOrEmpty(kv.Value.AccCode) && !string.IsNullOrEmpty(kv.Value.AccName));
		foreach (KeyValuePair<DataRow, AccData> item in enumerable)
		{
			AccData value = item.Value;
			string[] value2 = value.AccName.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
			string key = string.Join("-", value2);
			if (dictionary.ContainsKey(key))
			{
				value.AccCode = dictionary[key];
			}
		}
	}

	private string GetFullName(Account account)
	{
		Account account2 = account;
		string text = account2.Name;
		while ((account2 = account2.Parent) != null)
		{
			text = string.Join("-", account2.Name, text);
		}
		return text;
	}

	private void PreBuildAuxData()
	{
		DataTable auxiliaryTable = DataSource.AuxiliaryTable;
		DataTable voucherTable = DataSource.VoucherTable;
		if (auxiliaryTable == null)
		{
			return;
		}
		DataColumn dataColumn = FindColumn(auxiliaryTable, "辅助核算类别", throwExceptionWhenNotFound: true);
		DataColumn dataColumn2 = FindColumn(auxiliaryTable, "辅助核算代码", throwExceptionWhenNotFound: true);
		DataColumn dataColumn3 = FindColumn(auxiliaryTable, "辅助核算名称", throwExceptionWhenNotFound: true);
		DataColumn dataColumn4 = FindColumn(auxiliaryTable, "科目代码", throwExceptionWhenNotFound: true);
		DataColumn dataColumn5 = FindColumn(voucherTable, "辅助核算类别", throwExceptionWhenNotFound: true);
		DataColumn dataColumn6 = FindColumn(voucherTable, "辅助核算代码", throwExceptionWhenNotFound: true);
		DataColumn dataColumn7 = FindColumn(voucherTable, "辅助核算名称", throwExceptionWhenNotFound: true);
		DataColumn dataColumn8 = FindColumn(voucherTable, "科目代码", throwExceptionWhenNotFound: true);
		Dictionary<string, Tuple<Dictionary<string, string>, Dictionary<string, string>>> dictionary = new Dictionary<string, Tuple<Dictionary<string, string>, Dictionary<string, string>>>();
		Dictionary<string, HashSet<string>> dictionary2 = new Dictionary<string, HashSet<string>>();
		string text = null;
		string text2 = null;
		foreach (DataRow row in auxiliaryTable.Rows)
		{
			string accCode = PreAccountDataAll[row].AccCode;
			string text3 = row[dataColumn]?.ToString()?.Trim();
			accCode = (string.IsNullOrEmpty(accCode) ? text : accCode);
			text = accCode;
			text3 = (string.IsNullOrEmpty(text3) ? text2 : text3);
			text2 = text3;
			string text4 = row[dataColumn2]?.ToString()?.Trim();
			string text5 = row[dataColumn3]?.ToString()?.Trim();
			if (!string.IsNullOrEmpty(text4) || !string.IsNullOrEmpty(text5))
			{
				PreAuxDataBalance.Add(row, new AuxData(accCode, text3, text4, text5));
			}
		}
		text = null;
		text2 = null;
		foreach (DataRow row2 in voucherTable.Rows)
		{
			string accCode2 = PreAccountDataAll[row2].AccCode;
			string text6 = row2[dataColumn5]?.ToString()?.Trim();
			accCode2 = (string.IsNullOrEmpty(accCode2) ? text : accCode2);
			text = accCode2;
			text6 = (string.IsNullOrEmpty(text6) ? text2 : text6);
			text2 = text6;
			string text7 = row2[dataColumn6]?.ToString()?.Trim();
			string text8 = row2[dataColumn7]?.ToString()?.Trim();
			if (!string.IsNullOrEmpty(text7) || !string.IsNullOrEmpty(text8))
			{
				PreAuxDataVoucher.Add(row2, new AuxData(accCode2, text6, text7, text8));
			}
		}
		foreach (KeyValuePair<DataRow, AuxData> item11 in PreAuxDataBalance)
		{
			AuxData value = item11.Value;
			if (string.IsNullOrEmpty(value.AuxType))
			{
				continue;
			}
			if (!dictionary2.ContainsKey(value.AccountCode))
			{
				dictionary2.Add(value.AccountCode, new HashSet<string>());
			}
			HashSet<string> hashSet = dictionary2[value.AccountCode];
			string[] array = value.AuxType.Split('|');
			string[] array2 = array;
			foreach (string text9 in array2)
			{
				if (!dictionary.ContainsKey(text9))
				{
					dictionary.Add(text9, Tuple.Create(new Dictionary<string, string>(), new Dictionary<string, string>()));
				}
				hashSet.Add(text9);
			}
		}
		foreach (KeyValuePair<DataRow, AuxData> item12 in PreAuxDataVoucher)
		{
			AuxData value2 = item12.Value;
			if (string.IsNullOrEmpty(value2.AuxType))
			{
				continue;
			}
			if (!dictionary2.ContainsKey(value2.AccountCode))
			{
				dictionary2.Add(value2.AccountCode, new HashSet<string>());
			}
			HashSet<string> hashSet2 = dictionary2[value2.AccountCode];
			string[] array3 = value2.AuxType.Split('|');
			string[] array4 = array3;
			foreach (string text10 in array4)
			{
				if (!dictionary.ContainsKey(text10))
				{
					dictionary.Add(text10, Tuple.Create(new Dictionary<string, string>(), new Dictionary<string, string>()));
				}
				hashSet2.Add(text10);
			}
		}
		foreach (KeyValuePair<DataRow, AuxData> item13 in PreAuxDataBalance)
		{
			AuxData value3 = item13.Value;
			if (string.IsNullOrEmpty(value3.AuxType))
			{
				if (!dictionary2.ContainsKey(value3.AccountCode) || dictionary2[value3.AccountCode].Count != 1)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
						FailureContext = new FailureContext
						{
							Table = TableEnum.AUXILIARY,
							RowTag = auxiliaryTable.GetTag(item13.Key),
							ColTag = auxiliaryTable.GetTag(dataColumn),
							UserData = "辅助核算数据不正确，请检查辅助核算类别、代码、名称信息是否完整且正确"
						}
					};
				}
				value3.AuxType = dictionary2[value3.AccountCode].First();
			}
		}
		foreach (KeyValuePair<DataRow, AuxData> item14 in PreAuxDataVoucher)
		{
			AuxData value4 = item14.Value;
			if (string.IsNullOrEmpty(value4.AuxType))
			{
				string[] array5 = value4.AuxName.Split('|');
				if (!dictionary2.ContainsKey(value4.AccountCode) || dictionary2[value4.AccountCode].Count != 1)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
						FailureContext = new FailureContext
						{
							Table = TableEnum.VOUCHER,
							RowTag = voucherTable.GetTag(item14.Key),
							ColTag = voucherTable.GetTag(dataColumn5),
							UserData = "辅助核算数据不正确"
						}
					};
				}
				value4.AuxType = dictionary2[value4.AccountCode].First();
			}
		}
		foreach (KeyValuePair<DataRow, AuxData> item15 in PreAuxDataBalance)
		{
			AuxData value5 = item15.Value;
			if (string.IsNullOrEmpty(value5.AuxCode) || string.IsNullOrEmpty(value5.AuxName))
			{
				continue;
			}
			string[] array6 = value5.AuxType.Split('|');
			string[] array7 = value5.AuxCode.Split('|');
			string[] array8 = value5.AuxName.Split('|');
			if (array6.Length == array7.Length)
			{
				if (array7.Length == array8.Length)
				{
					for (int k = 0; k < array7.Length; k++)
					{
						Tuple<Dictionary<string, string>, Dictionary<string, string>> tuple = dictionary[array6[k]];
						Dictionary<string, string> item = tuple.Item1;
						if (!item.ContainsKey(array8[k]))
						{
							item.Add(array8[k], array7[k]);
						}
						Dictionary<string, string> item2 = tuple.Item2;
						if (!item2.ContainsKey(array7[k]))
						{
							item2.Add(array7[k], array8[k]);
						}
					}
				}
				else
				{
					value5.AuxName = null;
				}
			}
			else
			{
				if (array6.Length != array8.Length)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
						FailureContext = new FailureContext
						{
							Table = TableEnum.AUXILIARY,
							RowTag = auxiliaryTable.GetTag(item15.Key),
							ColTag = auxiliaryTable.GetTag(dataColumn2),
							UserData = "辅助核算数据不正确"
						}
					};
				}
				value5.AuxCode = null;
			}
		}
		foreach (KeyValuePair<DataRow, AuxData> item16 in PreAuxDataVoucher)
		{
			AuxData value6 = item16.Value;
			if (string.IsNullOrEmpty(value6.AuxCode) || string.IsNullOrEmpty(value6.AuxName))
			{
				continue;
			}
			string[] array9 = value6.AuxType.Split('|');
			string[] array10 = value6.AuxCode.Split('|');
			string[] array11 = value6.AuxName.Split('|');
			if (array9.Length == array10.Length)
			{
				if (array10.Length == array11.Length)
				{
					for (int l = 0; l < array9.Length; l++)
					{
						Tuple<Dictionary<string, string>, Dictionary<string, string>> tuple2 = dictionary[array9[l]];
						Dictionary<string, string> item3 = tuple2.Item1;
						if (!item3.ContainsKey(array11[l]))
						{
							item3.Add(array11[l], array10[l]);
						}
						Dictionary<string, string> item4 = tuple2.Item2;
						if (!item4.ContainsKey(array10[l]))
						{
							item4.Add(array10[l], array11[l]);
						}
					}
				}
				else
				{
					value6.AuxName = null;
				}
			}
			else
			{
				if (array9.Length != array11.Length)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
						FailureContext = new FailureContext
						{
							Table = TableEnum.VOUCHER,
							RowTag = voucherTable.GetTag(item16.Key),
							ColTag = voucherTable.GetTag(dataColumn6),
							UserData = "辅助核算数据不正确"
						}
					};
				}
				value6.AuxCode = null;
			}
		}
		foreach (KeyValuePair<DataRow, AuxData> item17 in PreAuxDataBalance)
		{
			AuxData value7 = item17.Value;
			if (!string.IsNullOrEmpty(value7.AuxCode) || string.IsNullOrEmpty(value7.AuxName))
			{
				continue;
			}
			string[] array12 = value7.AuxType.Split('|');
			string[] array13 = value7.AuxName.Split('|');
			if (array12.Length == array13.Length)
			{
				string[] array14 = new string[array12.Length];
				for (int m = 0; m < array12.Length; m++)
				{
					Tuple<Dictionary<string, string>, Dictionary<string, string>> tuple3 = dictionary[array12[m]];
					Dictionary<string, string> item5 = tuple3.Item1;
					string text11 = array13[m];
					if (item5.ContainsKey(text11))
					{
						array14[m] = item5[text11];
						continue;
					}
					string empty = string.Empty;
					if (item5.Count == 0)
					{
						empty = "001";
					}
					else
					{
						string value8 = item5.OrderByDescending((KeyValuePair<string, string> kv2) => kv2.Value).First().Value;
						Match match = Regex.Match(value8, "^([a-zA-Z]+)([0-9]+)$");
						if (match.Success)
						{
							string value9 = match.Groups[1].Value;
							long num = long.Parse(match.Groups[2].Value);
							while (item5.Values.Contains(empty = $"{value9}{++num}"))
							{
							}
						}
						else
						{
							int count = item5.Count;
							while (item5.Values.Contains(empty = count++.ToString().PadLeft(3, '0')))
							{
							}
						}
					}
					array14[m] = empty;
					item5.Add(text11, empty);
					Dictionary<string, string> item6 = tuple3.Item2;
					if (!item6.ContainsKey(empty))
					{
						tuple3.Item2.Add(empty, text11);
					}
				}
				value7.AuxCode = string.Join("|", array14);
				continue;
			}
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
				FailureContext = new FailureContext
				{
					Table = TableEnum.AUXILIARY,
					RowTag = auxiliaryTable.GetTag(item17.Key),
					ColTag = auxiliaryTable.GetTag(dataColumn2),
					UserData = "辅助核算数据不正确"
				}
			};
		}
		foreach (KeyValuePair<DataRow, AuxData> item18 in PreAuxDataVoucher)
		{
			AuxData value10 = item18.Value;
			if (!string.IsNullOrEmpty(value10.AuxCode) || string.IsNullOrEmpty(value10.AuxName))
			{
				continue;
			}
			string[] array15 = value10.AuxType.Split('|');
			string[] array16 = value10.AuxName.Split('|');
			if (array15.Length == array16.Length)
			{
				string[] array17 = new string[array15.Length];
				for (int n = 0; n < array15.Length; n++)
				{
					Tuple<Dictionary<string, string>, Dictionary<string, string>> tuple4 = dictionary[array15[n]];
					Dictionary<string, string> item7 = tuple4.Item1;
					string text12 = array16[n];
					if (item7.ContainsKey(text12))
					{
						array17[n] = item7[text12];
						continue;
					}
					string empty2 = string.Empty;
					if (item7.Count == 0)
					{
						empty2 = "001";
					}
					else
					{
						string value11 = item7.OrderByDescending((KeyValuePair<string, string> kv2) => kv2.Value).First().Value;
						Match match2 = Regex.Match(value11, "^([a-zA-Z]+)([0-9]+)$");
						if (match2.Success)
						{
							string value12 = match2.Groups[1].Value;
							long num2 = long.Parse(match2.Groups[2].Value);
							while (item7.Values.Contains(empty2 = $"{value12}{++num2}"))
							{
							}
						}
						else
						{
							int count2 = item7.Count;
							while (item7.Values.Contains(empty2 = count2++.ToString().PadLeft(3, '0')))
							{
							}
						}
					}
					array17[n] = empty2;
					item7.Add(text12, empty2);
					Dictionary<string, string> item8 = tuple4.Item2;
					if (!item8.ContainsKey(empty2))
					{
						item8.Add(empty2, text12);
					}
				}
				value10.AuxCode = string.Join("|", array17);
				continue;
			}
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
				FailureContext = new FailureContext
				{
					Table = TableEnum.VOUCHER,
					RowTag = voucherTable.GetTag(item18.Key),
					ColTag = voucherTable.GetTag(dataColumn6),
					UserData = "辅助核算数据不正确"
				}
			};
		}
		foreach (KeyValuePair<DataRow, AuxData> item19 in PreAuxDataBalance)
		{
			AuxData value13 = item19.Value;
			if (!string.IsNullOrEmpty(value13.AuxName) || string.IsNullOrEmpty(value13.AuxCode))
			{
				continue;
			}
			string[] array18 = value13.AuxType.Split('|');
			string[] array19 = value13.AuxCode.Split('|');
			if (array18.Length == array19.Length)
			{
				string[] array20 = new string[array18.Length];
				for (int num3 = 0; num3 < array18.Length; num3++)
				{
					Tuple<Dictionary<string, string>, Dictionary<string, string>> tuple5 = dictionary[array18[num3]];
					Dictionary<string, string> item9 = tuple5.Item2;
					string key = array19[num3];
					if (item9.ContainsKey(key))
					{
						array20[num3] = item9[key];
						continue;
					}
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
						FailureContext = new FailureContext
						{
							Table = TableEnum.AUXILIARY,
							RowTag = auxiliaryTable.GetTag(item19.Key),
							ColTag = auxiliaryTable.GetTag(dataColumn3),
							UserData = "辅助核算数据不正确"
						}
					};
				}
				value13.AuxName = string.Join("|", array20);
				continue;
			}
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
				FailureContext = new FailureContext
				{
					Table = TableEnum.AUXILIARY,
					RowTag = auxiliaryTable.GetTag(item19.Key),
					ColTag = auxiliaryTable.GetTag(dataColumn3),
					UserData = "辅助核算数据不正确"
				}
			};
		}
		foreach (KeyValuePair<DataRow, AuxData> item20 in PreAuxDataVoucher)
		{
			AuxData value14 = item20.Value;
			if (!string.IsNullOrEmpty(value14.AuxName) || string.IsNullOrEmpty(value14.AuxCode))
			{
				continue;
			}
			string[] array21 = value14.AuxType.Split('|');
			string[] array22 = value14.AuxCode.Split('|');
			if (array21.Length == array22.Length)
			{
				string[] array23 = new string[array21.Length];
				for (int num4 = 0; num4 < array21.Length; num4++)
				{
					Tuple<Dictionary<string, string>, Dictionary<string, string>> tuple6 = dictionary[array21[num4]];
					Dictionary<string, string> item10 = tuple6.Item2;
					string key2 = array22[num4];
					if (item10.ContainsKey(key2))
					{
						array23[num4] = item10[key2];
						continue;
					}
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
						FailureContext = new FailureContext
						{
							Table = TableEnum.VOUCHER,
							RowTag = voucherTable.GetTag(item20.Key),
							ColTag = voucherTable.GetTag(dataColumn7),
							UserData = "辅助核算数据不正确"
						}
					};
				}
				value14.AuxName = string.Join("|", array23);
				continue;
			}
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.AuxiliaryDataNotBeCorrect,
				FailureContext = new FailureContext
				{
					Table = TableEnum.VOUCHER,
					RowTag = voucherTable.GetTag(item20.Key),
					ColTag = voucherTable.GetTag(dataColumn7),
					UserData = "辅助核算数据不正确"
				}
			};
		}
	}

	private void BuildBalances()
	{
		DataTable balanceTable = DataSource.BalanceTable;
		DataColumn dataColumn = FindColumn(balanceTable, "科目代码", throwExceptionWhenNotFound: true);
		DataColumn dataColumn2 = FindColumn(balanceTable, "科目名称", throwExceptionWhenNotFound: true);
		DataColumn dataColumn3 = FindColumn(balanceTable, "年初借方余额", throwExceptionWhenNotFound: true);
		DataColumn column = FindColumn(balanceTable, "年初贷方余额", throwExceptionWhenNotFound: true);
		Dictionary<string, Account> dictionary = _ledger.Accounts.ToDictionary((Account t) => t.Code, (Account t) => t);
		Dictionary<Account, DataRow> balanceAccSet = new Dictionary<Account, DataRow>();
		for (int i = 0; i < balanceTable.Rows.Count; i++)
		{
			DataRow dataRow = balanceTable.Rows[i];
			string accCode = PreAccountDataAll[dataRow].AccCode;
			if (string.IsNullOrEmpty(accCode) || !dictionary.ContainsKey(accCode))
			{
				continue;
			}
			Account account = dictionary[accCode];
			object obj = dataRow[dataColumn3];
			object obj2 = dataRow[column];
			decimal result;
			decimal num = (decimal.TryParse(obj?.ToString(), out result) ? result : 0m);
			decimal result2;
			decimal num2 = (decimal.TryParse(obj2?.ToString(), out result2) ? result2 : 0m);
			account.Balance = num - num2;
			if (!(account.Balance != 0m))
			{
				continue;
			}
			List<Account> source = getAllChildren(account);
			if (source.Any((Account c) => balanceAccSet.ContainsKey(c)))
			{
				continue;
			}
			for (Account parent = account.Parent; parent != null; parent = parent.Parent)
			{
				if (balanceAccSet.ContainsKey(parent))
				{
					balanceAccSet.Remove(parent);
					break;
				}
			}
			balanceAccSet.Add(account, dataRow);
		}
		List<Account> list = new List<Account>();
		list.AddRange(VoucherAccountSet);
		list.AddRange(AuxiliaryAccountSet);
		HashSet<Account> addAccSet = new HashSet<Account>(list);
		foreach (KeyValuePair<Account, DataRow> item in balanceAccSet)
		{
			List<Account> source2 = getAllChildren(item.Key);
			if (source2.Any((Account c) => addAccSet.Contains(c)))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.BalanceNotBeLastLevel,
					FailureContext = new FailureContext
					{
						Table = TableEnum.BALANCE,
						RowTag = balanceTable.GetTag(item.Value),
						ColTag = balanceTable.GetTag(dataColumn3)
					}
				};
			}
		}
		foreach (Account account2 in _ledger.Accounts)
		{
			decimal _balance2 = default(decimal);
			lastLevelSummary(account2, ref _balance2);
			account2.Balance = (account2.IsDebit ? _balance2 : (-_balance2));
		}
		if (_ledger.Accounts.Where((Account a) => a.Children.Count == 0).Sum((Account a) => a.Balance) != 0m)
		{
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.BeginBalanceNotBeZero,
				FailureContext = new FailureContext
				{
					Table = TableEnum.BALANCE
				}
			};
		}
		static List<Account> getAllChildren(Account _acc)
		{
			List<Account> list2 = new List<Account>();
			putAllChildren(_acc, list2);
			return list2;
		}
		static void lastLevelSummary(Account acc, ref decimal _balance)
		{
			if (acc.Children.Count == 0)
			{
				_balance += (acc.IsDebit ? acc.Balance : (-acc.Balance));
				return;
			}
			foreach (Account child in acc.Children)
			{
				lastLevelSummary(child, ref _balance);
			}
		}
		static void putAllChildren(Account _acc2, List<Account> _accs)
		{
			_accs.Add(_acc2);
			foreach (Account child2 in _acc2.Children)
			{
				putAllChildren(child2, _accs);
			}
		}
	}

	private void BuildAuxBalance()
	{
		DataTable auxiliaryTable = DataSource.AuxiliaryTable;
		if (auxiliaryTable == null || auxiliaryTable.Rows.Count == 0)
		{
			return;
		}
		DataColumn col = FindColumn(auxiliaryTable, "科目代码", throwExceptionWhenNotFound: true);
		DataColumn dataColumn = FindColumn(auxiliaryTable, "科目名称", throwExceptionWhenNotFound: true);
		DataColumn column = FindColumn(auxiliaryTable, "年初借方余额", throwExceptionWhenNotFound: true);
		DataColumn column2 = FindColumn(auxiliaryTable, "年初贷方余额", throwExceptionWhenNotFound: true);
		Dictionary<string, Account> dictionary = _ledger.Accounts.ToDictionary((Account t) => t.Code, (Account t) => t);
		string text = null;
		string text2 = null;
		string text3 = null;
		for (int i = 0; i < auxiliaryTable.Rows.Count; i++)
		{
			DataRow key = auxiliaryTable.Rows[i];
			if (!PreAuxDataBalance.ContainsKey(key))
			{
				continue;
			}
			AuxData auxData = PreAuxDataBalance[key];
			string accCode = PreAccountDataAll[key].AccCode;
			string accName = PreAccountDataAll[key].AccName;
			string auxType = auxData.AuxType;
			string auxCode = auxData.AuxCode;
			string auxName = auxData.AuxName;
			accCode = (string.IsNullOrEmpty(accCode) ? text : accCode);
			text = accCode;
			accName = (string.IsNullOrEmpty(accName) ? text2 : accName);
			text2 = accName;
			auxType = (string.IsNullOrEmpty(auxType) ? text3 : auxType);
			text3 = auxType;
			if (string.IsNullOrEmpty(accName) || !dictionary.ContainsKey(accCode))
			{
				continue;
			}
			Account account = dictionary[accCode];
			if (account.Children.Count > 0)
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.AuxiliaryAccountNotBeLastLevel,
					FailureContext = new FailureContext
					{
						Table = TableEnum.AUXILIARY,
						RowTag = auxiliaryTable.GetTag(auxiliaryTable.Rows[i]),
						ColTag = auxiliaryTable.GetTag(col)
					}
				};
			}
			decimal num = default(decimal);
			object obj = auxiliaryTable.Rows[i][column];
			object obj2 = auxiliaryTable.Rows[i][column2];
			num = ((decimal.TryParse(obj?.ToString(), out var result) && result != 0m) ? result : ((!decimal.TryParse(obj2?.ToString(), out var result2) || !(result2 != 0m)) ? default(decimal) : (-result2)));
			string[] array = auxType.Split('|');
			string[] array2 = auxCode.Split('|');
			string[] array3 = auxName.Split('|');
			for (int j = 0; j < array.Length; j++)
			{
				string text4 = array[j];
				string text5 = array2[j];
				string name = array3[j];
				Item item = null;
				if (!tempAuxType.ContainsKey(text4))
				{
					ItemClass itemClass = new ItemClass
					{
						Code = (tempAuxType.Count + 1).ToString().PadLeft(3, '0'),
						Name = text4
					};
					Item item2 = new Item
					{
						Code = text5,
						Name = name,
						ItemClass = itemClass
					};
					_ledger.Items.Add(item2);
					itemClass.Items.Add(item2);
					_ledger.ItemClasses.Add(itemClass);
					tempAuxType.Add(itemClass.Name, new Dictionary<string, Item>());
					tempAuxType[itemClass.Name].Add(item2.Code, item2);
					item = item2;
				}
				else
				{
					Dictionary<string, Item> dictionary2 = tempAuxType[text4];
					if (dictionary2.ContainsKey(text5))
					{
						item = dictionary2[text5];
					}
					else
					{
						Item item3 = new Item
						{
							Code = text5,
							Name = name,
							ItemClass = dictionary2.First().Value.ItemClass
						};
						item3.ItemClass.Items.Add(item3);
						_ledger.Items.Add(item3);
						dictionary2.Add(item3.Code, item3);
						item = item3;
					}
				}
				if (account.ItemBalance == null)
				{
					account.ItemBalance = new Dictionary<Item, ItemBalance>();
				}
				if (!account.ItemBalance.ContainsKey(item))
				{
					account.ItemBalance.Add(item, new ItemBalance());
				}
				account.ItemBalance[item].Balance += num;
			}
		}
	}

	private void BuildVoucher()
	{
		DataTable voucherTable = DataSource.VoucherTable;
		if (voucherTable == null)
		{
			return;
		}
		DataColumn dataColumn = FindColumn(voucherTable, "日期", throwExceptionWhenNotFound: true);
		DataColumn dataColumn2 = FindColumn(voucherTable, "凭证字", throwExceptionWhenNotFound: true);
		DataColumn dataColumn3 = FindColumn(voucherTable, "凭证号", throwExceptionWhenNotFound: true);
		DataColumn col = FindColumn(voucherTable, "科目代码", throwExceptionWhenNotFound: true);
		DataColumn dataColumn4 = FindColumn(voucherTable, "科目名称", throwExceptionWhenNotFound: true);
		DataColumn column = FindColumn(voucherTable, "摘要", throwExceptionWhenNotFound: true);
		DataColumn dataColumn5 = FindColumn(voucherTable, "借方金额", throwExceptionWhenNotFound: true);
		DataColumn dataColumn6 = FindColumn(voucherTable, "贷方金额", throwExceptionWhenNotFound: true);
		DataColumn dataColumn7 = FindColumn(voucherTable, "辅助核算类别", throwExceptionWhenNotFound: false);
		DataColumn dataColumn8 = FindColumn(voucherTable, "辅助核算代码", throwExceptionWhenNotFound: false);
		DataColumn dataColumn9 = FindColumn(voucherTable, "辅助核算名称", throwExceptionWhenNotFound: false);
		decimal num = default(decimal);
		decimal num2 = default(decimal);
		Dictionary<string, Account> dictionary = _ledger.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
		Dictionary<string, Dictionary<string, List<string>>> dictionary2 = _ledger.Accounts.Where((Account a) => a.ItemBalance != null && a.ItemBalance.Count > 0).ToDictionary((Account a) => a.Code, (Account a) => (from i in a.ItemBalance
			group i by i.Key.ItemClass).ToDictionary((IGrouping<ItemClass, KeyValuePair<Item, ItemBalance>> b) => b.Key.Name, (IGrouping<ItemClass, KeyValuePair<Item, ItemBalance>> b) => b.Select((KeyValuePair<Item, ItemBalance> c) => c.Key.Code).ToList()));
		Dictionary<string, VoucherType> dictionary3 = new Dictionary<string, VoucherType>();
		object obj = null;
		object obj2 = null;
		string text = "";
		Dictionary<Voucher, DataRow> dictionary4 = new Dictionary<Voucher, DataRow>();
		int num3 = 0;
		while (num3 < voucherTable.Rows.Count)
		{
			DataRow dataRow = voucherTable.Rows[num3];
			string accCode = PreAccountDataAll[dataRow].AccCode;
			if (string.IsNullOrEmpty(accCode))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.VoucherAccountCodeEmpty,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(dataRow),
						ColTag = voucherTable.GetTag(col)
					}
				};
			}
			if (!dictionary.ContainsKey(accCode))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.VoucherAccountCodeNotFound,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(voucherTable.Rows[num3]),
						ColTag = voucherTable.GetTag(col)
					}
				};
			}
			Account account3 = dictionary[accCode];
			if (account3.Children.Count > 0)
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.VoucherAccountNotBeLastLevel,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(voucherTable.Rows[num3]),
						ColTag = voucherTable.GetTag(col)
					}
				};
			}
			string text2 = voucherTable.Rows[num3][dataColumn2]?.ToString()?.Trim(splitChars);
			text2 = ((!string.IsNullOrEmpty(text2)) ? text2 : obj2?.ToString());
			if (string.IsNullOrEmpty(text2))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.VoucherTypeEmpty,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(voucherTable.Rows[num3]),
						ColTag = voucherTable.GetTag(dataColumn2)
					}
				};
			}
			obj2 = text2;
			if (!dictionary3.ContainsKey(text2))
			{
				VoucherType voucherType = new VoucherType
				{
					Name = text2
				};
				_ledger.VoucherTypes.Add(voucherType);
				dictionary3.Add(text2, voucherType);
			}
			string text3 = (voucherTable.Rows[num3][dataColumn3] ?? "").ToString().Trim();
			if (text3 == string.Empty)
			{
				text3 = text;
			}
			if (text3 == string.Empty)
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.VoucherNumberEmpty,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(voucherTable.Rows[num3]),
						ColTag = voucherTable.GetTag(dataColumn3)
					}
				};
			}
			text = text3;
			string text4 = voucherTable.Rows[num3][dataColumn]?.ToString();
			text4 = ((!string.IsNullOrEmpty(text4)) ? text4 : obj?.ToString());
			DateTime now = DateTime.Now;
			if (DateTime.TryParse(text4, out var result))
			{
				now = result;
			}
			else
			{
				try
				{
					DateTime dateTime = Convert.ToDateTime(text4);
					now = dateTime;
				}
				catch
				{
					goto IL_04fe;
				}
			}
			goto IL_069b;
			IL_04fe:
			try
			{
				string text5 = text4.Split(' ')[0];
				text5 = text5.Replace('年', '-');
				text5 = text5.Replace('月', '-');
				text5 = text5.Replace('日', '-');
				text5 = text5.Replace('号', '-');
				if (text5.EndsWith("-"))
				{
					text5 = text5.Substring(0, text5.Length - 1);
				}
				DateTime dateTime2 = Convert.ToDateTime(text5);
				now = dateTime2;
			}
			catch
			{
				goto IL_0594;
			}
			goto IL_069b;
			IL_069b:
			obj = text4;
			Voucher voucher = new Voucher
			{
				Id = num3 - 1,
				Type = dictionary3[text2],
				Account = dictionary[accCode],
				Number = text3,
				Digest = voucherTable.Rows[num3][column]?.ToString(),
				Day = now
			};
			string text6 = voucherTable.Rows[num3][dataColumn5]?.ToString();
			string text7 = voucherTable.Rows[num3][dataColumn6]?.ToString();
			if (!TryParseDecimal(text6, out var _out))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.VoucherDebitAmountTypeNotCorrect,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(voucherTable.Rows[num3]),
						ColTag = voucherTable.GetTag(dataColumn5)
					}
				};
			}
			decimal num4 = _out;
			if (!TryParseDecimal(text7, out var _out2))
			{
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.VoucherDebitAmountTypeNotCorrect,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(voucherTable.Rows[num3]),
						ColTag = voucherTable.GetTag(dataColumn6)
					}
				};
			}
			decimal num5 = _out2;
			voucher.Amount = ((num4 != 0m) ? num4 : num5);
			voucher.IsDebit = num4 != 0m;
			voucher.Currency = _ledger.BaseCurrency;
			voucher.Foreign = new ForeignRecord
			{
				ExchangeRate = 1.0,
				ForeignAmount = voucher.Amount,
				StandardAmount = voucher.Amount
			};
			if (dataColumn7 != null && dataColumn8 != null && dataColumn9 != null)
			{
				DataRow dataRow2 = voucherTable.Rows[num3];
				if (PreAuxDataVoucher.ContainsKey(dataRow2))
				{
					AuxData auxData = PreAuxDataVoucher[dataRow2];
					string auxType = auxData.AuxType;
					string auxCode = auxData.AuxCode;
					string auxName = auxData.AuxName;
					if (!string.IsNullOrWhiteSpace(auxType) && !string.IsNullOrWhiteSpace(auxCode))
					{
						string[] array = auxType.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
						string[] array2 = auxCode.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
						string[] array3 = auxName.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length != 0 && array2.Length != 0 && array3.Length != 0)
						{
							int num6 = ((array.Length >= array2.Length) ? ((array2.Length < array3.Length) ? array2.Length : array3.Length) : ((array.Length < array3.Length) ? array.Length : array3.Length));
							for (int j = 0; j < num6; j++)
							{
								string className = array[j];
								string text8 = array2[j];
								string name = array3[j];
								if (account3.Balance != 0m && !account3.ItemBalance.Any((KeyValuePair<Item, ItemBalance> kv) => kv.Key.ItemClass.Name == className))
								{
									ImportException2 importException = new ImportException2();
									importException.FailureReason = FailureReasonEnum.VoucherAuxiliaryNotFound;
									importException.FailureContext = new FailureContext
									{
										Table = TableEnum.VOUCHER,
										RowTag = voucherTable.GetTag(dataRow2),
										ColTag = voucherTable.GetTag(col),
										UserData = "(" + voucher.Account.Code + ") 该科目挂接有【" + className + "】类别的辅助核算信息，但在《年初辅助余额表》中未发现该科目【" + className + "】类别的辅助核算余额。请在《年初辅助余额表》中补充该科目的【" + className + "】类别辅助核算余额，或者在《会计凭证库》中删除该科目挂接的【" + className + "】类别辅助核算信息。"
									};
									throw importException;
								}
								Item item = null;
								if (!tempAuxType.ContainsKey(className))
								{
									ItemClass itemClass = new ItemClass
									{
										Code = (tempAuxType.Count + 1).ToString().PadLeft(3, '0'),
										Name = className
									};
									Item item2 = new Item
									{
										Code = text8,
										Name = name,
										ItemClass = itemClass
									};
									itemClass.Items.Add(item2);
									_ledger.ItemClasses.Add(itemClass);
									_ledger.Items.Add(item2);
									tempAuxType.Add(itemClass.Name, new Dictionary<string, Item>());
									tempAuxType[itemClass.Name].Add(item2.Code, item2);
									item = item2;
								}
								else
								{
									Dictionary<string, Item> dictionary5 = tempAuxType[className];
									if (dictionary5.ContainsKey(text8))
									{
										item = dictionary5[text8];
									}
									else
									{
										Item item3 = new Item
										{
											Code = text8,
											Name = name,
											ItemClass = dictionary5.First().Value.ItemClass
										};
										item3.ItemClass.Items.Add(item3);
										_ledger.Items.Add(item3);
										dictionary5.Add(item3.Code, item3);
										item = item3;
									}
								}
								if (dictionary2.ContainsKey(voucher.Account.Code))
								{
									Dictionary<string, List<string>> dictionary6 = dictionary2[voucher.Account.Code];
									if (dictionary6.ContainsKey(className))
									{
										List<string> list = dictionary6[className];
										if (!list.Contains(text8))
										{
											list.Add(text8);
											if (voucher.Account.ItemBalance == null)
											{
												voucher.Account.ItemBalance = new Dictionary<Item, ItemBalance>();
											}
											voucher.Account.ItemBalance.Add(item, new ItemBalance
											{
												Balance = 0m
											});
										}
									}
									else
									{
										List<string> list2 = new List<string>();
										list2.Add(text8);
										dictionary6.Add(className, list2);
										if (voucher.Account.ItemBalance == null)
										{
											voucher.Account.ItemBalance = new Dictionary<Item, ItemBalance>();
										}
										voucher.Account.ItemBalance.Add(item, new ItemBalance
										{
											Balance = 0m
										});
									}
								}
								else
								{
									Dictionary<string, List<string>> dictionary7 = new Dictionary<string, List<string>>();
									List<string> list3 = new List<string>();
									list3.Add(text8);
									dictionary7.Add(className, list3);
									dictionary2.Add(voucher.Account.Code, dictionary7);
									if (voucher.Account.ItemBalance == null)
									{
										voucher.Account.ItemBalance = new Dictionary<Item, ItemBalance>();
									}
									voucher.Account.ItemBalance.Add(item, new ItemBalance
									{
										Balance = 0m
									});
								}
								voucher.Details.Add(item);
							}
						}
					}
				}
			}
			_ledger.Vouchers.Add(voucher);
			dictionary4.Add(voucher, dataRow);
			if (num4 != 0m)
			{
				num += num4;
			}
			else
			{
				num2 += num5;
			}
			num3++;
			continue;
			IL_0594:
			try
			{
				string text9 = text4.Split(' ')[0];
				text9 = text9.Replace('年', '-');
				text9 = text9.Replace('月', '-');
				text9 = text9.Replace('日', '-');
				text9 = text9.Replace('号', '-');
				if (text9.EndsWith("-"))
				{
					text9 = text9.Substring(0, text9.Length - 1);
				}
				string[] formats = new string[2] { "yyyy-MM-dd", "yyyy-M-d" };
				if (DateTime.TryParseExact(text9, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2))
				{
					now = result2;
					goto IL_069b;
				}
			}
			catch
			{
			}
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.VoucherDateTypeNotCorrect,
				FailureContext = new FailureContext
				{
					Table = TableEnum.VOUCHER,
					RowTag = voucherTable.GetTag(voucherTable.Rows[num3]),
					ColTag = voucherTable.GetTag(dataColumn)
				}
			};
		}
		IEnumerable<IGrouping<string, Voucher>> source = from v in _ledger.Vouchers
			group v by v.Day.ToString("yyyyMMdd") + v.Type.Name + v.Number;
		IGrouping<string, Voucher> grouping = source.FirstOrDefault((IGrouping<string, Voucher> g) => g.Sum((Voucher v) => v.IsDebit ? v.Amount : (-v.Amount)) != 0m);
		if (grouping != null)
		{
			Voucher voucher2 = grouping.First();
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.SpecifyVoucherDebitNotEqualsCredit,
				FailureContext = new FailureContext
				{
					Table = TableEnum.VOUCHER,
					UserData = Tuple.Create(grouping.Key, voucher2.Day.ToString("yyyy-MM-dd") + " " + voucher2.Type.Name + voucher2.Number)
				}
			};
		}
		DataTable balanceTable = DataSource.BalanceTable;
		foreach (Account account2 in _ledger.Accounts)
		{
			if (account2.ItemBalance == null || account2.ItemBalance.Count == 0)
			{
				continue;
			}
			IEnumerable<IGrouping<ItemClass, KeyValuePair<Item, ItemBalance>>> enumerable = from i in account2.ItemBalance
				group i by i.Key.ItemClass;
			foreach (IGrouping<ItemClass, KeyValuePair<Item, ItemBalance>> item4 in enumerable)
			{
				decimal value = item4.Sum((KeyValuePair<Item, ItemBalance> c) => c.Value.Balance);
				if (account2.Balance.Equals(value))
				{
					continue;
				}
				DataColumn dataColumn10 = FindColumn(balanceTable, "科目代码", throwExceptionWhenNotFound: true);
				DataColumn col2 = FindColumn(balanceTable, "年初借方余额", throwExceptionWhenNotFound: true);
				DataRow dataRow3 = FindRowByCode(balanceTable, (string code) => code == account2.Code, withPreData: true);
				if (dataRow3 != null)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryBeginBalanceNotEqualsAccountBegin,
						FailureContext = new FailureContext
						{
							Table = TableEnum.BALANCE,
							RowTag = balanceTable.GetTag(dataRow3),
							ColTag = balanceTable.GetTag(col2),
							UserData = account2.Code
						}
					};
				}
				DataTable auxiliaryTable = DataSource.AuxiliaryTable;
				if (auxiliaryTable != null)
				{
					DataColumn dataColumn11 = FindColumn(auxiliaryTable, "科目代码", throwExceptionWhenNotFound: true);
					DataColumn col3 = FindColumn(auxiliaryTable, "年初借方余额", throwExceptionWhenNotFound: true);
					DataRow dataRow4 = FindRowByCode(auxiliaryTable, (string code) => code == account2.Code, withPreData: true);
					if (dataRow4 != null)
					{
						throw new ImportException2
						{
							FailureReason = FailureReasonEnum.AuxiliaryBeginBalanceNotEqualsAccountBegin,
							FailureContext = new FailureContext
							{
								Table = TableEnum.AUXILIARY,
								RowTag = auxiliaryTable.GetTag(dataRow4),
								ColTag = auxiliaryTable.GetTag(col3),
								UserData = account2.Code
							}
						};
					}
				}
				DataColumn col4 = FindColumn(voucherTable, "科目代码", throwExceptionWhenNotFound: true);
				DataRow dataRow5 = FindRowByCode(voucherTable, (string code) => code == account2.Code, withPreData: true);
				if (dataRow5 != null)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.AuxiliaryBeginBalanceNotEqualsAccountBegin,
						FailureContext = new FailureContext
						{
							Table = TableEnum.VOUCHER,
							RowTag = voucherTable.GetTag(dataRow5),
							ColTag = voucherTable.GetTag(col4),
							UserData = account2.Code
						}
					};
				}
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.SpecificMessage,
					FailureContext = new FailureContext
					{
						UserData = "科目" + account2.Code + "的辅助核算期初数与科目期初余额表期初数不相等"
					}
				};
			}
		}
		foreach (Account account in _ledger.Accounts.Where((Account a) => a.ItemBalance != null && a.ItemBalance.Count > 0))
		{
			foreach (IGrouping<ItemClass, KeyValuePair<Item, ItemBalance>> itemBalance in from i in account.ItemBalance
				group i by i.Key.ItemClass)
			{
				IEnumerable<Voucher> source2 = _ledger.Vouchers.Where((Voucher v) => v.Account == account);
				if (source2.All((Voucher v) => v.Details.Any((Item d) => d.ItemClass == itemBalance.Key)))
				{
					continue;
				}
				DataTable auxiliaryTable2 = DataSource.AuxiliaryTable;
				DataRow dataRow6 = DataTableExtensions.AsEnumerable(auxiliaryTable2).FirstOrDefault((DataRow r) => account.Code.Equals(r["kmdm"]) && itemBalance.Key.Name.Equals(r["auxtype"]));
				Voucher voucher3 = source2.First((Voucher v) => v.Details.All((Item d) => d.ItemClass != itemBalance.Key));
				DataRow row = dictionary4[voucher3];
				if (dataRow6 == null)
				{
					throw new ImportException2
					{
						FailureReason = FailureReasonEnum.VoucherNotRelatedAuxiliaryAccount,
						FailureContext = new FailureContext
						{
							Table = TableEnum.VOUCHER,
							RowTag = voucherTable.GetTag(row),
							ColTag = voucherTable.GetTag(col),
							UserData = voucher3.Account.Code
						}
					};
				}
				throw new ImportException2
				{
					FailureReason = FailureReasonEnum.AuxBalanceWithoutVoucherAux,
					FailureContext = new FailureContext
					{
						Table = TableEnum.VOUCHER,
						RowTag = voucherTable.GetTag(row),
						ColTag = voucherTable.GetTag(col),
						UserData = voucher3.Account.Code
					}
				};
			}
		}
	}

	private void Finish()
	{
		_ledger.Accounts = _ledger.Accounts.OrderBy((Account a) => a.Code).ToList();
		_ledger.StartDate = ((_ledger.Vouchers.Count > 0) ? new DateTime(_ledger.Vouchers.Min((Voucher v) => v.Day).Year, 1, 1) : new DateTime(2000, 1, 1));
		_ledger.Year = _ledger.StartDate.Year;
	}

	private bool TryParseInt(string text, out int _out, string message)
	{
		if (string.IsNullOrEmpty(text))
		{
			throw new LedgerImportException(message);
		}
		if (int.TryParse(text, out _out))
		{
			return true;
		}
		throw new LedgerImportException(message);
	}

	private bool TryParseDecimal(string text, out decimal _out)
	{
		if (string.IsNullOrEmpty(text))
		{
			_out = default(decimal);
			return true;
		}
		if (text == "－" || text == "-")
		{
			_out = default(decimal);
			return true;
		}
		if (decimal.TryParse(text, out _out))
		{
			return true;
		}
		return false;
	}

	private bool TryParseDateTime(string text, out DateTime _out, string message)
	{
		if (DateTime.TryParse(text, out _out))
		{
			return true;
		}
		throw new LedgerImportException(message);
	}

	private int FindColumn(DataTable dataTable, Predicate<string> predicate, bool throwExcept = false, string message = null)
	{
		if (dataTable.Rows.Count == 0)
		{
			if (throwExcept)
			{
				throw new LedgerImportException(message);
			}
			return -1;
		}
		if (dataTable.Columns.Count == 0)
		{
			if (throwExcept)
			{
				throw new LedgerImportException(message);
			}
			return -1;
		}
		for (int i = 0; i < dataTable.Columns.Count; i++)
		{
			if (predicate(dataTable.Rows[0][i]?.ToString()))
			{
				return i;
			}
		}
		if (throwExcept)
		{
			throw new LedgerImportException(message);
		}
		return -1;
	}

	private DataColumn FindColumn(DataTable dataTable, string caption, bool throwExceptionWhenNotFound)
	{
		for (int i = 0; i < dataTable.Columns.Count; i++)
		{
			if (dataTable.Columns[i].Caption == caption)
			{
				return dataTable.Columns[i];
			}
		}
		if (throwExceptionWhenNotFound)
		{
			throw new ImportException2
			{
				FailureReason = FailureReasonEnum.ColumnNotFound,
				FailureContext = new FailureContext
				{
					UserData = Tuple.Create(dataTable.TableName, caption)
				}
			};
		}
		return null;
	}

	private int ValidCols(DataTable sheet)
	{
		for (int num = sheet.Columns.Count - 1; num >= 0; num--)
		{
			for (int i = 0; i < sheet.Rows.Count; i++)
			{
				if (!string.IsNullOrEmpty(sheet.Rows[i][num]?.ToString()))
				{
					return num + 1;
				}
			}
		}
		return 0;
	}

	private bool AccountHasSplitter(Ledger ledger)
	{
		if (ledger.Accounts.Count == 0)
		{
			return false;
		}
		int num = ledger.Accounts.Max((Account a) => RemovePrefixChars(a.Code).Split(splitChars, StringSplitOptions.RemoveEmptyEntries).Length);
		return num > 1;
	}

	private string RemovePrefixChars(string code)
	{
		return code.Trim(prefixChars);
	}

	protected void OnProgressChanged(string progress)
	{
		this.ProgressChanged?.Invoke(this, progress);
	}
}
