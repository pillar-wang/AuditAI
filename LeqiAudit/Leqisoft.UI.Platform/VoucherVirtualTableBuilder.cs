using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class VoucherVirtualTableBuilder
{
	public enum VoucherVirtualTableColumnIndex
	{
		凭证日期,
		凭证年月,
		凭证字号,
		摘要,
		科目代码,
		科目名称,
		辅助核算类别,
		辅助核算代码,
		辅助核算名称,
		借方发生额,
		贷方发生额
	}

	protected static Dictionary<string, long> _tableColumnNameIdDic;

	protected static Dictionary<long, string> _tableColumnIdNameDic;

	protected static LedgerVirtualTable _emptyTable;

	public static readonly Id64 VoucherVirtualTableId;

	public const string VoucherVirtualTableName = "会计凭证表";

	static VoucherVirtualTableBuilder()
	{
		_tableColumnNameIdDic = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
		_tableColumnIdNameDic = new Dictionary<long, string>();
		VoucherVirtualTableId = new Id64(int.MaxValue, 2147483646);
		Type typeFromHandle = typeof(VoucherVirtualTableColumnIndex);
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
		return new LedgerVirtualTable(VoucherVirtualTableId.Value, rowsCount, 11);
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
		Type typeFromHandle = typeof(VoucherVirtualTableColumnIndex);
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
		switch ((VoucherVirtualTableColumnIndex)columnId)
		{
		case VoucherVirtualTableColumnIndex.凭证日期:
			return typeof(DateTime);
		case VoucherVirtualTableColumnIndex.凭证年月:
			return typeof(DateYearMonth);
		case VoucherVirtualTableColumnIndex.借方发生额:
		case VoucherVirtualTableColumnIndex.贷方发生额:
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

	public static LedgerVirtualTable Build(Ledger ledger)
	{
		try
		{
			return BuildImpl(ledger);
		}
		catch (Exception exception)
		{
			exception.Log("构建会计凭证表的虚拟表格时发生了未预期的异常");
			return null;
		}
	}

	private static string GetVoucherNumberExp(Voucher voucher)
	{
		string text = null;
		string text2 = voucher.Number;
		if (voucher.Type != null)
		{
			text = voucher.Type.Name;
		}
		if (text2 == null)
		{
			text2 = string.Empty;
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			if (!string.IsNullOrWhiteSpace(text2))
			{
				return text + "-" + text2;
			}
			return text;
		}
		return text2;
	}

	protected static LedgerVirtualTable BuildImpl(Ledger ledger)
	{
		int count = ledger.Vouchers.Count;
		LedgerVirtualTable ledgerVirtualTable = CreateVirtualTable(count);
		for (int i = 0; i < count; i++)
		{
			Voucher voucher = ledger.Vouchers[i];
			DateTime day = voucher.Day;
			DateYearMonth dateYearMonth = new DateYearMonth(new DateTime(day.Year, day.Month, 1));
			string voucherNumberExp = GetVoucherNumberExp(voucher);
			string digest = voucher.Digest;
			string displayAccountCodeWithDetail = voucher.GetDisplayAccountCodeWithDetail();
			string displayAccountNameWithDetail = voucher.GetDisplayAccountNameWithDetail();
			string displayAuxiliaryClassName = voucher.GetDisplayAuxiliaryClassName();
			string displayAuxiliaryCode = voucher.GetDisplayAuxiliaryCode();
			string displayAuxiliaryName = voucher.GetDisplayAuxiliaryName();
			decimal num = (voucher.IsDebit ? voucher.Amount : 0m);
			decimal num2 = (voucher.IsDebit ? 0m : voucher.Amount);
			ledgerVirtualTable[i, 0].Value = day;
			ledgerVirtualTable[i, 1].Value = dateYearMonth;
			ledgerVirtualTable[i, 2].Value = voucherNumberExp;
			ledgerVirtualTable[i, 3].Value = digest;
			ledgerVirtualTable[i, 4].Value = displayAccountCodeWithDetail;
			ledgerVirtualTable[i, 5].Value = displayAccountNameWithDetail;
			ledgerVirtualTable[i, 6].Value = displayAuxiliaryClassName;
			ledgerVirtualTable[i, 7].Value = displayAuxiliaryCode;
			ledgerVirtualTable[i, 8].Value = displayAuxiliaryName;
			ledgerVirtualTable[i, 9].Value = (double)num;
			ledgerVirtualTable[i, 10].Value = (double)num2;
		}
		return ledgerVirtualTable;
	}

	private static void DebugOutputVirtualData(VirtualTable virtualTable)
	{
		using StreamWriter streamWriter = new StreamWriter(new FileStream("D:\\out.csv", FileMode.Create, FileAccess.Write));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("凭证日期").Append(",");
		stringBuilder.Append("凭证年月").Append(",");
		stringBuilder.Append("凭证字号").Append(",");
		stringBuilder.Append("摘要").Append(",");
		stringBuilder.Append("科目代码").Append(",");
		stringBuilder.Append("科目名称").Append(",");
		stringBuilder.Append("辅助核算类别").Append(",");
		stringBuilder.Append("辅助核算代码").Append(",");
		stringBuilder.Append("辅助核算名称").Append(",");
		stringBuilder.Append("借方发生额").Append(",");
		stringBuilder.Append("贷方发生额").Append(",");
		streamWriter.WriteLine(stringBuilder.ToString());
		int count = virtualTable.Rows.Count;
		for (int i = 0; i < count; i++)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append(((DateTime)virtualTable[i, 0].Value).ToString("yyyy-MM-dd")).Append(",");
			stringBuilder2.Append(((DateYearMonth)virtualTable[i, 1].Value).Date.ToString("yyyy-MM")).Append(",");
			stringBuilder2.Append(virtualTable[i, 2].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 3].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 4].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 5].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 6].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 7].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 8].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 9].Value.ToString()).Append(",");
			stringBuilder2.Append(virtualTable[i, 10].Value.ToString()).Append(",");
			streamWriter.WriteLine(stringBuilder2.ToString());
		}
	}
}
