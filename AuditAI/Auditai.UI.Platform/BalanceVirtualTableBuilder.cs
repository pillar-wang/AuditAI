using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.LedgerView;

namespace Auditai.UI.Platform;

public class BalanceVirtualTableBuilder
{
	public enum BalanceVirtualTableColumnIndex
	{
		年月,
		科目代码,
		科目名称,
		本级科目名称,
		科目级次,
		是否末级,
		辅助核算类别,
		辅助核算代码,
		辅助核算名称,
		期初借方余额,
		期初贷方余额,
		本期借方发生额,
		本期贷方发生额,
		本年累计借方发生额,
		本年累计贷方发生额,
		期末借方余额,
		期末贷方余额,
		期初借方净额,
		期初贷方净额,
		期末借方净额,
		期末贷方净额,
		是否挂接辅助核算,
		一级科目名称,
		年初借方余额,
		年初贷方余额,
		年初借方净额,
		年初贷方净额
	}

	protected class VirtualTableRowKey
	{
		public Account Account;

		public Auditai.Model.AuxiliaryItem AuxiliaryItem;

		public string AccountCode;

		public string AccountFullName;

		public string AccountName;

		public string FirstLevelAccountName;

		public int AccountLevel;

		public bool IsLastLevel;

		public bool IsExistAuxiliary;

		public string AuxiliaryClassName;

		public string AuxiliaryCode;

		public string AuxiliaryName;

		public decimal CurrentYearDebitChangeValue;

		public decimal CurrentYearCreditChangeValue;

		public decimal CurrentYearDebitBalanceStartValue;

		public decimal CurrentYearCreditBalanceStartValue;

		public decimal CurrentYearDebitBalanceStartNetAmount;

		public decimal CurrentYearCreditBalanceStartNetAmount;
	}

	protected class VirtualTableRowData
	{
		public string AccountCode;

		public string AccountFullName;

		public string AccountName;

		public string FirstLevelAccountName;

		public DateYearMonth YearMonth;

		public int AccountLevel;

		public bool IsLastLevel;

		public bool IsExistAuxiliary;

		public string AuxiliaryClassName;

		public string AuxiliaryCode;

		public string AuxiliaryName;

		public decimal DebitStartValue;

		public decimal CreditStartValue;

		public decimal DebitChangeValue;

		public decimal CreditChangeValue;

		public decimal CurrentYearDebitChangeValue;

		public decimal CurrentYearCreditChangeValue;

		public decimal DebitEndValue;

		public decimal CreditEndValue;

		public decimal DebitNetAmountStartValue;

		public decimal CreditNetAmountStartValue;

		public decimal DebitNetAmountEndValue;

		public decimal CreditNetAmountEndValue;

		public decimal CurrentYearDebitBalanceStartValue;

		public decimal CurrentYearCreditBalanceStartValue;

		public decimal CurrentYearDebitBalanceStartNetAmount;

		public decimal CurrentYearCreditBalanceStartNetAmount;
	}

	protected static Dictionary<string, long> _tableColumnNameIdDic;

	protected static Dictionary<long, string> _tableColumnIdNameDic;

	protected static LedgerVirtualTable _emptyTable;

	public static readonly Id64 BalanceVirtualTableId;

	public const string BalanceVirtualTableName = "科目余额表";

	static BalanceVirtualTableBuilder()
	{
		_tableColumnNameIdDic = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
		_tableColumnIdNameDic = new Dictionary<long, string>();
		BalanceVirtualTableId = new Id64(int.MaxValue, int.MaxValue);
		Type typeFromHandle = typeof(BalanceVirtualTableColumnIndex);
		foreach (object value in Enum.GetValues(typeFromHandle))
		{
			string name = Enum.GetName(typeFromHandle, value);
			long num = (int)value;
			_tableColumnNameIdDic[name] = num;
			_tableColumnIdNameDic[num] = name;
		}
	}

	public static LedgerVirtualTable GetEmtpyTable()
	{
		if (_emptyTable != null)
		{
			return _emptyTable;
		}
		_emptyTable = CreateVirtualTable(0);
		return _emptyTable;
	}

	protected static LedgerVirtualTable CreateVirtualTable(int rowsCount)
	{
		return new LedgerVirtualTable(BalanceVirtualTableId.Value, rowsCount, 27);
	}

	public static bool TryGetColumnName(long columnId, out string columnName)
	{
		return _tableColumnIdNameDic.TryGetValue(columnId, out columnName);
	}

	public static bool TryGetColumnId(string columnName, out long columnId)
	{
		return _tableColumnNameIdDic.TryGetValue(columnName, out columnId);
	}

	public static int GetColumnIndex(string columnName)
	{
		if (!TryGetColumnId(columnName, out var columnId))
		{
			return -1;
		}
		return (int)columnId;
	}

	public static int GetColumnIndex(long columnId)
	{
		int num = (int)columnId;
		if (num < 0 || num >= _tableColumnIdNameDic.Count)
		{
			return -1;
		}
		return num;
	}

	public static bool TryGetColumnIdByColumnIndex(int columnIndex, out long columnId)
	{
		if (columnIndex < 0 || columnIndex >= _tableColumnIdNameDic.Count)
		{
			columnId = -1L;
			return false;
		}
		columnId = columnIndex;
		return true;
	}

	public static List<string> GetAllColumnNames()
	{
		List<string> list = new List<string>();
		Type typeFromHandle = typeof(BalanceVirtualTableColumnIndex);
		foreach (object value in Enum.GetValues(typeFromHandle))
		{
			string name = Enum.GetName(typeFromHandle, value);
			list.Add(name);
		}
		return list;
	}

	public static Type GetColumnDataType(string columnName)
	{
		if (!TryGetColumnId(columnName, out var columnId))
		{
			return null;
		}
		return GetColumnDataType(columnId);
	}

	public static Type GetColumnDataType(long columnId)
	{
		switch ((BalanceVirtualTableColumnIndex)columnId)
		{
		case BalanceVirtualTableColumnIndex.年月:
			return typeof(DateYearMonth);
		case BalanceVirtualTableColumnIndex.是否末级:
		case BalanceVirtualTableColumnIndex.是否挂接辅助核算:
			return typeof(bool);
		case BalanceVirtualTableColumnIndex.期初借方余额:
		case BalanceVirtualTableColumnIndex.期初贷方余额:
		case BalanceVirtualTableColumnIndex.本期借方发生额:
		case BalanceVirtualTableColumnIndex.本期贷方发生额:
		case BalanceVirtualTableColumnIndex.本年累计借方发生额:
		case BalanceVirtualTableColumnIndex.本年累计贷方发生额:
		case BalanceVirtualTableColumnIndex.期末借方余额:
		case BalanceVirtualTableColumnIndex.期末贷方余额:
		case BalanceVirtualTableColumnIndex.期初借方净额:
		case BalanceVirtualTableColumnIndex.期初贷方净额:
		case BalanceVirtualTableColumnIndex.期末借方净额:
		case BalanceVirtualTableColumnIndex.期末贷方净额:
		case BalanceVirtualTableColumnIndex.年初借方余额:
		case BalanceVirtualTableColumnIndex.年初贷方余额:
		case BalanceVirtualTableColumnIndex.年初借方净额:
		case BalanceVirtualTableColumnIndex.年初贷方净额:
			return typeof(double);
		default:
		{
			if (!TryGetColumnName(columnId, out var _))
			{
				return null;
			}
			return typeof(string);
		}
		}
	}

	public static LedgerVirtualTable Build(LedgerViewer ledgerViewer)
	{
		try
		{
			Ledger ledger = ledgerViewer.Ledger;
			return BuildImpl(ledgerViewer);
		}
		catch (Exception exception)
		{
			exception.Log("构建科目余额表的虚拟表格时发生了未预期的异常");
			return null;
		}
	}

	protected static LedgerVirtualTable BuildImpl(LedgerViewer ledgerViewer)
	{
		Ledger ledger = ledgerViewer.Ledger;
		DateTime startDate = ledger.StartDate;
		DateTime dateTime = new DateTime(ledger.GetEndDate().Year, 12, 31);
		List<VirtualTableRowKey> list = GenerateAccountList(ledger, ledgerViewer);
		List<VirtualTableRowData> list2 = new List<VirtualTableRowData>(list.Count * 12);
		foreach (VirtualTableRowKey item in list)
		{
			item.CurrentYearDebitChangeValue = default(decimal);
			item.CurrentYearCreditChangeValue = default(decimal);
			item.CurrentYearDebitBalanceStartValue = default(decimal);
			item.CurrentYearDebitBalanceStartNetAmount = default(decimal);
			item.CurrentYearCreditBalanceStartValue = default(decimal);
			item.CurrentYearCreditBalanceStartNetAmount = default(decimal);
		}
		DateTime dateTime2 = startDate;
		int year = dateTime2.Year;
		while (dateTime2 <= dateTime)
		{
			DateTime dateTime3 = dateTime2.AddMonths(1);
			DateTime dateTime4 = new DateTime(dateTime3.Year, dateTime3.Month, 1);
			DateTime dateTime5 = dateTime4.AddDays(-1.0);
			if (dateTime5 > dateTime)
			{
				dateTime5 = dateTime;
			}
			TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(dateTime2, dateTime5);
			foreach (VirtualTableRowKey item2 in list)
			{
				VirtualTableRowData virtualTableRowData = new VirtualTableRowData();
				virtualTableRowData.AccountCode = item2.AccountCode;
				virtualTableRowData.AccountFullName = item2.AccountFullName;
				virtualTableRowData.FirstLevelAccountName = item2.FirstLevelAccountName;
				virtualTableRowData.AccountName = item2.AccountName;
				virtualTableRowData.AccountLevel = item2.AccountLevel;
				virtualTableRowData.IsLastLevel = item2.IsLastLevel;
				virtualTableRowData.IsExistAuxiliary = item2.IsExistAuxiliary;
				virtualTableRowData.AuxiliaryClassName = item2.AuxiliaryClassName;
				virtualTableRowData.AuxiliaryCode = item2.AuxiliaryCode;
				virtualTableRowData.AuxiliaryName = item2.AuxiliaryName;
				virtualTableRowData.YearMonth = new DateYearMonth(new DateTime(dateTime2.Year, dateTime2.Month, 1));
				list2.Add(virtualTableRowData);
				if (item2.AuxiliaryItem == null)
				{
					if (trialBalanceSheet.Start.TryGetValue(item2.Account, out var value))
					{
						string balanceDCChar = LedgerViewer.GetBalanceDCChar(item2.Account, value.Total);
						if (balanceDCChar == "借")
						{
							virtualTableRowData.DebitStartValue = Math.Abs(value.Total);
						}
						else if (balanceDCChar == "贷")
						{
							virtualTableRowData.CreditStartValue = Math.Abs(value.Total);
						}
						virtualTableRowData.DebitNetAmountStartValue = virtualTableRowData.DebitStartValue - virtualTableRowData.CreditStartValue;
						virtualTableRowData.CreditNetAmountStartValue = virtualTableRowData.CreditStartValue - virtualTableRowData.DebitStartValue;
					}
					if (trialBalanceSheet.End.TryGetValue(item2.Account, out var value2))
					{
						string balanceDCChar2 = LedgerViewer.GetBalanceDCChar(item2.Account, value2.Total);
						if (balanceDCChar2 == "借")
						{
							virtualTableRowData.DebitEndValue = Math.Abs(value2.Total);
						}
						else if (balanceDCChar2 == "贷")
						{
							virtualTableRowData.CreditEndValue = Math.Abs(value2.Total);
						}
						virtualTableRowData.DebitNetAmountEndValue = virtualTableRowData.DebitEndValue - virtualTableRowData.CreditEndValue;
						virtualTableRowData.CreditNetAmountEndValue = virtualTableRowData.CreditEndValue - virtualTableRowData.DebitEndValue;
					}
					if (trialBalanceSheet.Debit.TryGetValue(item2.Account, out var value3))
					{
						virtualTableRowData.DebitChangeValue = value3.Total;
					}
					if (trialBalanceSheet.Credit.TryGetValue(item2.Account, out var value4))
					{
						virtualTableRowData.CreditChangeValue = value4.Total;
					}
					item2.CurrentYearDebitChangeValue += virtualTableRowData.DebitChangeValue;
					virtualTableRowData.CurrentYearDebitChangeValue = item2.CurrentYearDebitChangeValue;
					item2.CurrentYearCreditChangeValue += virtualTableRowData.CreditChangeValue;
					virtualTableRowData.CurrentYearCreditChangeValue = item2.CurrentYearCreditChangeValue;
				}
				else
				{
					decimal num = trialBalanceSheet.Start.Get(item2.Account, item2.AuxiliaryItem);
					string balanceDCChar3 = LedgerViewer.GetBalanceDCChar(item2.Account, num);
					if (balanceDCChar3 == "借")
					{
						virtualTableRowData.DebitStartValue = Math.Abs(num);
					}
					else if (balanceDCChar3 == "贷")
					{
						virtualTableRowData.CreditStartValue = Math.Abs(num);
					}
					virtualTableRowData.DebitNetAmountStartValue = virtualTableRowData.DebitStartValue - virtualTableRowData.CreditStartValue;
					virtualTableRowData.CreditNetAmountStartValue = virtualTableRowData.CreditStartValue - virtualTableRowData.DebitStartValue;
					decimal num2 = trialBalanceSheet.End.Get(item2.Account, item2.AuxiliaryItem);
					string balanceDCChar4 = LedgerViewer.GetBalanceDCChar(item2.Account, num2);
					if (balanceDCChar4 == "借")
					{
						virtualTableRowData.DebitEndValue = Math.Abs(num2);
					}
					else if (balanceDCChar4 == "贷")
					{
						virtualTableRowData.CreditEndValue = Math.Abs(num2);
					}
					virtualTableRowData.DebitNetAmountEndValue = virtualTableRowData.DebitEndValue - virtualTableRowData.CreditEndValue;
					virtualTableRowData.CreditNetAmountEndValue = virtualTableRowData.CreditEndValue - virtualTableRowData.DebitEndValue;
					virtualTableRowData.DebitChangeValue = trialBalanceSheet.Debit.Get(item2.Account, item2.AuxiliaryItem);
					virtualTableRowData.CreditChangeValue = trialBalanceSheet.Credit.Get(item2.Account, item2.AuxiliaryItem);
					item2.CurrentYearDebitChangeValue += virtualTableRowData.DebitChangeValue;
					virtualTableRowData.CurrentYearDebitChangeValue = item2.CurrentYearDebitChangeValue;
					item2.CurrentYearCreditChangeValue += virtualTableRowData.CreditChangeValue;
					virtualTableRowData.CurrentYearCreditChangeValue = item2.CurrentYearCreditChangeValue;
				}
				if (dateTime2.Month == 1)
				{
					virtualTableRowData.CurrentYearDebitBalanceStartValue = virtualTableRowData.DebitStartValue;
					virtualTableRowData.CurrentYearCreditBalanceStartValue = virtualTableRowData.CreditStartValue;
					virtualTableRowData.CurrentYearDebitBalanceStartNetAmount = virtualTableRowData.DebitNetAmountStartValue;
					virtualTableRowData.CurrentYearCreditBalanceStartNetAmount = virtualTableRowData.CreditNetAmountStartValue;
					item2.CurrentYearDebitBalanceStartValue = virtualTableRowData.CurrentYearDebitBalanceStartValue;
					item2.CurrentYearCreditBalanceStartValue = virtualTableRowData.CurrentYearCreditBalanceStartValue;
					item2.CurrentYearDebitBalanceStartNetAmount = virtualTableRowData.CurrentYearDebitBalanceStartNetAmount;
					item2.CurrentYearCreditBalanceStartNetAmount = virtualTableRowData.CurrentYearCreditBalanceStartNetAmount;
				}
				else
				{
					virtualTableRowData.CurrentYearDebitBalanceStartValue = item2.CurrentYearDebitBalanceStartValue;
					virtualTableRowData.CurrentYearCreditBalanceStartValue = item2.CurrentYearCreditBalanceStartValue;
					virtualTableRowData.CurrentYearDebitBalanceStartNetAmount = item2.CurrentYearDebitBalanceStartNetAmount;
					virtualTableRowData.CurrentYearCreditBalanceStartNetAmount = item2.CurrentYearCreditBalanceStartNetAmount;
				}
			}
			dateTime2 = dateTime4;
			if (dateTime2.Year == year)
			{
				continue;
			}
			year = dateTime2.Year;
			foreach (VirtualTableRowKey item3 in list)
			{
				item3.CurrentYearDebitChangeValue = default(decimal);
				item3.CurrentYearCreditChangeValue = default(decimal);
				item3.CurrentYearDebitBalanceStartValue = default(decimal);
				item3.CurrentYearDebitBalanceStartNetAmount = default(decimal);
				item3.CurrentYearCreditBalanceStartValue = default(decimal);
				item3.CurrentYearCreditBalanceStartNetAmount = default(decimal);
			}
		}
		LedgerVirtualTable ledgerVirtualTable = CreateVirtualTable(list2.Count);
		for (int i = 0; i < list2.Count; i++)
		{
			VirtualTableRowData virtualTableRowData2 = list2[i];
			ledgerVirtualTable[i, 0].Value = virtualTableRowData2.YearMonth;
			ledgerVirtualTable[i, 1].Value = ((virtualTableRowData2.AccountCode == null) ? string.Empty : virtualTableRowData2.AccountCode);
			ledgerVirtualTable[i, 22].Value = ((virtualTableRowData2.AccountFullName == null) ? string.Empty : virtualTableRowData2.FirstLevelAccountName);
			ledgerVirtualTable[i, 2].Value = ((virtualTableRowData2.AccountFullName == null) ? string.Empty : virtualTableRowData2.AccountFullName);
			ledgerVirtualTable[i, 3].Value = ((virtualTableRowData2.AccountName == null) ? string.Empty : virtualTableRowData2.AccountName);
			ledgerVirtualTable[i, 4].Value = (double)virtualTableRowData2.AccountLevel;
			ledgerVirtualTable[i, 5].Value = virtualTableRowData2.IsLastLevel;
			ledgerVirtualTable[i, 21].Value = virtualTableRowData2.IsExistAuxiliary;
			ledgerVirtualTable[i, 6].Value = ((virtualTableRowData2.AuxiliaryClassName == null) ? string.Empty : virtualTableRowData2.AuxiliaryClassName);
			ledgerVirtualTable[i, 7].Value = ((virtualTableRowData2.AuxiliaryCode == null) ? string.Empty : virtualTableRowData2.AuxiliaryCode);
			ledgerVirtualTable[i, 8].Value = ((virtualTableRowData2.AuxiliaryName == null) ? string.Empty : virtualTableRowData2.AuxiliaryName);
			ledgerVirtualTable[i, 9].Value = (double)virtualTableRowData2.DebitStartValue;
			ledgerVirtualTable[i, 10].Value = (double)virtualTableRowData2.CreditStartValue;
			ledgerVirtualTable[i, 11].Value = (double)virtualTableRowData2.DebitChangeValue;
			ledgerVirtualTable[i, 12].Value = (double)virtualTableRowData2.CreditChangeValue;
			ledgerVirtualTable[i, 13].Value = (double)virtualTableRowData2.CurrentYearDebitChangeValue;
			ledgerVirtualTable[i, 14].Value = (double)virtualTableRowData2.CurrentYearCreditChangeValue;
			ledgerVirtualTable[i, 15].Value = (double)virtualTableRowData2.DebitEndValue;
			ledgerVirtualTable[i, 16].Value = (double)virtualTableRowData2.CreditEndValue;
			ledgerVirtualTable[i, 17].Value = (double)virtualTableRowData2.DebitNetAmountStartValue;
			ledgerVirtualTable[i, 18].Value = (double)virtualTableRowData2.CreditNetAmountStartValue;
			ledgerVirtualTable[i, 19].Value = (double)virtualTableRowData2.DebitNetAmountEndValue;
			ledgerVirtualTable[i, 20].Value = (double)virtualTableRowData2.CreditNetAmountEndValue;
			ledgerVirtualTable[i, 23].Value = (double)virtualTableRowData2.CurrentYearDebitBalanceStartValue;
			ledgerVirtualTable[i, 25].Value = (double)virtualTableRowData2.CurrentYearDebitBalanceStartNetAmount;
			ledgerVirtualTable[i, 24].Value = (double)virtualTableRowData2.CurrentYearCreditBalanceStartValue;
			ledgerVirtualTable[i, 26].Value = (double)virtualTableRowData2.CurrentYearCreditBalanceStartNetAmount;
		}
		return ledgerVirtualTable;
	}

	private static void DebugOutputVirtualData(VirtualTable virtualTable)
	{
		using StreamWriter streamWriter = new StreamWriter(new FileStream("D:\\out.csv", FileMode.Create, FileAccess.Write));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("年月").Append(",");
		stringBuilder.Append("科目代码").Append(",");
		stringBuilder.Append("科目全称").Append(",");
		stringBuilder.Append("科目名称").Append(",");
		stringBuilder.Append("辅助核算类").Append(",");
		stringBuilder.Append("辅助核算代码").Append(",");
		stringBuilder.Append("辅助核算名称").Append(",");
		stringBuilder.Append("科目级次").Append(",");
		stringBuilder.Append("是否末级").Append(",");
		stringBuilder.Append("是否挂接辅助核算").Append(",");
		stringBuilder.Append("期初借方余额").Append(",");
		stringBuilder.Append("本期借方发生额").Append(",");
		stringBuilder.Append("期末借方余额").Append(",");
		stringBuilder.Append("期初贷方余额").Append(",");
		stringBuilder.Append("本期贷方发生额").Append(",");
		stringBuilder.Append("期末贷方余额").Append(",");
		stringBuilder.Append("本年累计借方发生额").Append(",");
		stringBuilder.Append("本年累计贷方发生额").Append(",");
		stringBuilder.Append("期初借方净额").Append(",");
		stringBuilder.Append("期初贷方净额").Append(",");
		stringBuilder.Append("期末借方净额").Append(",");
		stringBuilder.Append("期末贷方净额").Append(",");
		streamWriter.WriteLine(stringBuilder.ToString());
		int count = virtualTable.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append(((DateYearMonth)virtualTable[i, 0].Value).Date.ToString("yyyy-MM")).Append(",");
			stringBuilder2.Append(virtualTable[i, 1].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 2].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 3].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 6].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 7].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 8].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 4].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 5].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 21].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 9].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 11].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 15].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 10].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 12].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 16].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 13].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 14].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 17].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 18].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 19].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 20].Value.ToString()).Append(",");
			streamWriter.WriteLine(stringBuilder2.ToString());
		}
	}

	private static string GetFirstLevelAccountName(Account account)
	{
		Account account2 = account;
		while (account2.Parent != null)
		{
			account2 = account2.Parent;
		}
		return account2.Name;
	}

	protected static List<VirtualTableRowKey> GenerateAccountList(Ledger ledger, LedgerViewer ledgerViewer)
	{
		bool isShowEmtpyAccount = ledgerViewer.IsDisplayEmptyAccount();
		List<VirtualTableRowKey> rowDataList = new List<VirtualTableRowKey>();
		TrialBalanceSheet balanceSheet = ledger.GetTrialBalanceSheet(ledger.StartDate, ledger.EndDate);
		foreach (Account item in ledger.RootAccounts.OrderBy((Account a) => a.Code))
		{
			AddChildAccount(item);
		}
		return rowDataList;
		void AddAuxiliaryItem(Account account, VirtualTableRowKey parentAccountRowData)
		{
			Dictionary<string, AuxiliaryItem> dictionary = new Dictionary<string, AuxiliaryItem>();
			GetAccountAuxiliaryItem(account, balanceSheet.Start, dictionary);
			GetAccountAuxiliaryItem(account, balanceSheet.Debit, dictionary);
			GetAccountAuxiliaryItem(account, balanceSheet.Credit, dictionary);
			List<AuxiliaryItem> list = dictionary.Values.ToList();
			if (list.Count == 0)
			{
				return;
			}
			list.Sort((AuxiliaryItem left, AuxiliaryItem right) => left.Code.CompareTo(right.Code));
			foreach (AuxiliaryItem item2 in list)
			{
				if (isShowEmtpyAccount || !ledgerViewer.IsEmptyAuxiliaryItem(account, (dynamic)item2))
				{
					VirtualTableRowKey virtualTableRowKey = new VirtualTableRowKey();
					rowDataList.Add(virtualTableRowKey);
					parentAccountRowData.IsLastLevel = false;
					parentAccountRowData.IsExistAuxiliary = true;
					virtualTableRowKey.Account = account;
					virtualTableRowKey.AccountCode = string.Join("-", account.Code, item2.Code);
					virtualTableRowKey.AccountName = item2.Name;
					virtualTableRowKey.AccountFullName = string.Join("-", account.GetFullName(), item2.Name);
					virtualTableRowKey.AccountLevel = parentAccountRowData.AccountLevel + 1;
					virtualTableRowKey.IsLastLevel = true;
					virtualTableRowKey.AuxiliaryItem = (dynamic)item2;
					virtualTableRowKey.AuxiliaryClassName = item2.ClassName;
					virtualTableRowKey.AuxiliaryCode = item2.Code;
					virtualTableRowKey.AuxiliaryName = item2.Name;
					virtualTableRowKey.FirstLevelAccountName = GetFirstLevelAccountName(account);
				}
			}
		}
		void AddChildAccount(Account account)
		{
			if (isShowEmtpyAccount || !ledgerViewer.IsEmptyAccount(account))
			{
				VirtualTableRowKey virtualTableRowKey2 = new VirtualTableRowKey();
				rowDataList.Add(virtualTableRowKey2);
				virtualTableRowKey2.Account = account;
				virtualTableRowKey2.AccountCode = account.Code;
				virtualTableRowKey2.AccountName = account.Name;
				virtualTableRowKey2.AccountFullName = account.GetFullName();
				virtualTableRowKey2.AccountLevel = account.GetLevel() + 1;
				virtualTableRowKey2.IsLastLevel = account.Children.Count == 0;
				virtualTableRowKey2.FirstLevelAccountName = GetFirstLevelAccountName(account);
				foreach (Account child in account.Children)
				{
					AddChildAccount(child);
				}
				AddAuxiliaryItem(account, virtualTableRowKey2);
			}
		}
		static void GetAccountAuxiliaryItem(Account account, DateBalance accountBlance, Dictionary<string, AuxiliaryItem> auxItemDic)
		{
			if (!accountBlance.TryGetValue(account, out var value) || value.ClassBalances.Count == 0)
			{
				return;
			}
			foreach (ClassBalance value2 in value.ClassBalances.Values)
			{
				foreach (Auditai.Model.AuxiliaryItem key2 in value2.ItemBalances.Keys)
				{
					string key = string.Join("-", key2.Class.Code, key2.Code);
					if (!auxItemDic.ContainsKey(key))
					{
						auxItemDic.Add(key, (dynamic)key2);
					}
				}
			}
		}
	}
}
