using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public abstract class LedgerBuilder
{
	public const int MaxProgress = 11;

	private Ledger _ledger;

	protected LedgerInfo ledgerInfo;

	protected string database;

	protected LSDb _db;

	protected virtual void Initialize(Ledger ledger, LedgerInfo info)
	{
		ledger.CompanyName = info.CompanyName;
		ledger.LedgerNumber = info.LedgerNumber;
		ledger.StartDate = new DateTime(info.Year, 1, 1);
		_db = LSDb.FromDatabaseInfo(info.DbInfo);
		_db.Database = database;
	}

	protected abstract void BuildCurrencies(Ledger ledger);

	protected abstract void BuildVoucherTypes(Ledger ledger);

	protected abstract void BuildAuxiliaries(Ledger ledger);

	protected abstract void BuildAccounts(Ledger ledger);

	protected virtual void BuildAccountsLevel(Ledger ledger)
	{
		BuildAccountsLevelStatic(ledger);
	}

	public static void BuildAccountsLevelStatic(Ledger ledger)
	{
		IEnumerable<IGrouping<int, Account>> source = from a in ledger.Accounts
			group a by a.Code.Length;
		List<IGrouping<int, Account>> list = source.OrderBy((IGrouping<int, Account> g) => g.Key).ToList();
		for (int num = list.Count - 1; num > 0; num--)
		{
			IGrouping<int, Account> grouping = list[num - 1];
			Dictionary<string, Account> dictionary = grouping.ToDictionary((Account a) => a.Code, (Account a) => a);
			foreach (Account item in list[num])
			{
				string key = item.Code.Substring(0, grouping.Key);
				if (dictionary.ContainsKey(key))
				{
					Account account = dictionary[key];
					account.Children.Add(item);
					item.Parent = account;
					continue;
				}
				throw new Exception("非法的科目代码，未找到父科目 科目代码：" + item.Code + " 科目名称：" + item.Name);
			}
		}
	}

	protected abstract void BuildVouchers(Ledger ledger);

	protected virtual void BuildAuxiliariesRelation(Ledger ledger)
	{
	}

	protected virtual void BuildFinally(Ledger ledger)
	{
	}

	public Ledger GetLedger(LedgerInfo info, string database)
	{
		ledgerInfo = info;
		this.database = database;
		info.ReportProgress("正在创建账套");
		_ledger = new Ledger(info);
		info.ReportProgress("正在初始化账套");
		Initialize(_ledger, info);
		info.ReportProgress("正在获取货币类型");
		BuildCurrencies(_ledger);
		info.ReportProgress("正在获取凭证类型");
		BuildVoucherTypes(_ledger);
		info.ReportProgress("正在获取辅助核算");
		BuildAuxiliaries(_ledger);
		info.ReportProgress("正在获取科目信息");
		BuildAccounts(_ledger);
		info.ReportProgress("正在建立科目级次");
		BuildAccountsLevel(_ledger);
		info.ReportProgress("正在获取凭证列表");
		BuildVouchers(_ledger);
		info.ReportProgress("正在挂接辅助核算");
		BuildAuxiliariesRelation(_ledger);
		info.ReportProgress("正在完成采集账套");
		BuildFinally(_ledger);
		info.ReportProgress("正在创建账套");
		return _ledger;
	}
}
