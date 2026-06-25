using System;
using System.Collections.Generic;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.LedgerView;

namespace Auditai.UI.Platform;

public class LedgerVirtualTableUtils
{
	protected static Dictionary<Ledger, LedgerVirtualTable> _balanceVirtualTableDic = new Dictionary<Ledger, LedgerVirtualTable>();

	protected static Dictionary<Ledger, LedgerVirtualTable> _voucherVirtualTableDic = new Dictionary<Ledger, LedgerVirtualTable>();

	public static void CacheLegerBalanceVirtualTable(Ledger ledger, LedgerVirtualTable virtualTable)
	{
		_balanceVirtualTableDic[ledger] = virtualTable;
	}

	public static void CacheLegerVoucherVirtualTable(Ledger ledger, LedgerVirtualTable virtualTable)
	{
		_voucherVirtualTableDic[ledger] = virtualTable;
	}

	public static LedgerVirtualTable GetBalanceVirtualTable(LedgerViewer ledgerViewer)
	{
		try
		{
			Ledger ledger = ledgerViewer.Ledger;
			if (_balanceVirtualTableDic.TryGetValue(ledger, out var value))
			{
				return value;
			}
			value = BalanceVirtualTableBuilder.Build(ledgerViewer);
			_balanceVirtualTableDic[ledger] = value;
			return value;
		}
		catch (Exception exception)
		{
			exception.Log("科目余额表的虚拟表格构建失败");
			return null;
		}
	}

	public static LedgerVirtualTable GetVoucherVirtualTable(Ledger ledger)
	{
		try
		{
			if (_voucherVirtualTableDic.TryGetValue(ledger, out var value))
			{
				return value;
			}
			value = VoucherVirtualTableBuilder.Build(ledger);
			_voucherVirtualTableDic[ledger] = value;
			return value;
		}
		catch (Exception exception)
		{
			exception.Log("会计凭证表的虚拟表格构建失败");
			return null;
		}
	}

	public static void ClearLederVirtualTable(Ledger ledger)
	{
		_balanceVirtualTableDic.Remove(ledger);
		_voucherVirtualTableDic.Remove(ledger);
	}
}
