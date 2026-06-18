using System;
using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls.SmartCollector;
using Newtonsoft.Json;

namespace Leqisoft.UI.Controls.CollectTable;

public class TableCollectorBalance : TableCollectorAbstract
{
	public AccNameStyleEnum AccNameStyle { get; set; }

	public TableCollectorBalance(Ledger ledger, Leqisoft.Model.Table table)
		: base(ledger, table)
	{
		base.CollectObject = CollectObjectEnum.Balance;
		base.IsNeedRunEmptyAccountCheckToSetSelectStatus = true;
	}

	public override void ThrowExceptionIfUnExpectAuditYear(int auditYear)
	{
	}

	public override void ThrowExceptionIfUnExpectSource()
	{
		if (base.Source == null)
		{
			throw new ArgumentNullException("数据源不能为空");
		}
		if (!base.Source.All((object v) => v is Account || v is Tuple<Account, AuxiliaryItem>))
		{
			throw new ArgumentException("数据源类型不合法！");
		}
	}

	private void FindOutWhichColunmNeedUsePositiveValue()
	{
		base.PositiveValueColumnMaps.Clear();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string value in base.Maps.Values)
		{
			if (!dictionary.ContainsKey(value))
			{
				dictionary.Add(value, null);
			}
		}
		foreach (long key2 in base.Maps.Keys)
		{
			string key = base.Maps[key2];
			if (OppositeCaptionDic.Balance.ContainsKey(key) && dictionary.ContainsKey(key) && dictionary.ContainsKey(OppositeCaptionDic.Balance[key]))
			{
				base.PositiveValueColumnMaps.Add(key2, value: true);
			}
		}
	}

	public override bool IsAbleUseCurrentSettingToCollectData()
	{
		if (!base.Setting.CollectAllAccount && base.Setting.Account == null)
		{
			return false;
		}
		return true;
	}

	public override TableCollectResult Collect(int auditYear)
	{
		ThrowExceptionIfUnExpectAuditYear(auditYear);
		if (base.Source == null)
		{
			base.Source = GetSourceFromSetting();
		}
		ThrowExceptionIfUnExpectSource();
		if (!base.Setting.CollectAllAccount && base.Setting.Account == null)
		{
			throw new InvalidCollectSettingException("采集器没有设置要查询的科目！");
		}
		if (base.Setting.CollectingFilter != null)
		{
			List<object> list = base.Source.ToList();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				object obj = list[num];
				if (obj is Account account)
				{
					if (!base.Setting.CollectingFilter.IsAccountShouldBeSelected(account))
					{
						list.RemoveAt(num);
					}
				}
				else if (obj is Tuple<Account, AuxiliaryItem> tuple && !base.Setting.CollectingFilter.IsAccountShouldBeSelected(tuple.Item1))
				{
					list.RemoveAt(num);
				}
			}
			base.Source = list;
		}
		TableCollectResult result = new TableCollectResult(this);
		if (base.IsSaveCollectResultByColumnId)
		{
			result.ValuesOnColumn = new Dictionary<long, List<object>>();
		}
		else
		{
			FindOutWhichColunmNeedUsePositiveValue();
		}
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(base.Setting.Ledger, base.Setting.Start, base.Setting.End);
		DateTime dateTime = base.Setting.Start.AddYears(-1);
		DateTime dateTime2 = base.Setting.End.AddYears(-1);
		TrialBalanceSheet trialBalanceSheetWithCache2 = GetTrialBalanceSheetWithCache(base.Setting.Ledger, dateTime, dateTime2);
		DateTime end = base.Setting.End;
		DateTime dateTime3 = new DateTime(end.Year, end.Month, 1, 0, 0, 0);
		DateTime dateTime4 = dateTime3.AddMonths(1).AddDays(-1.0);
		dateTime4 = new DateTime(dateTime4.Year, dateTime4.Month, dateTime4.Day, 23, 59, 59);
		TrialBalanceSheet trialBalanceSheetWithCache3 = GetTrialBalanceSheetWithCache(base.Setting.Ledger, dateTime3, dateTime4);
		Leqisoft.Model.Table table = base.Setting.Table;
		Ledger ledger = base.Setting.Ledger;
		Account account2 = base.Setting.Account;
		object auxiliary = base.Setting.Auxiliary;
		int year = ledger.StartDate.Year;
		int year2 = ledger.GetEndDate().Year;
		List<SubsidiaryLedger> subsidiaryLedgerCacheList = base.Source.Select((object u) => (SubsidiaryLedger)null).ToList();
		foreach (KeyValuePair<long, string> map2 in base.Maps)
		{
			KeyValuePair<long, string> map = map2;
			if (!base.IsSaveCollectResultByColumnId && !table.Columns.Any((Leqisoft.Model.Column c) => c.Id == new Id64(map.Key)))
			{
				continue;
			}
			if (map.Value == "上期贷方发生额" || map.Value == "上期借方发生额")
			{
				if (dateTime.Year >= year && dateTime2.Year <= year2)
				{
					PickUpDataForCollectTarget(base.Source, trialBalanceSheetWithCache2, dateTime, dateTime2);
				}
			}
			else if (map.Value == "本月借方发生额" || map.Value == "本月贷方发生额")
			{
				if (dateTime3.Year >= year && dateTime4.Year <= year2)
				{
					PickUpDataForCollectTarget(base.Source, trialBalanceSheetWithCache3, dateTime3, dateTime4);
				}
			}
			else if ((base.Setting.Start.Year >= year && base.Setting.End.Year <= year2) || !(map.Value != "科目名称") || !(map.Value != "科目代码"))
			{
				PickUpDataForCollectTarget(base.Source, trialBalanceSheetWithCache, base.Setting.Start, base.Setting.End);
			}
			void PickUpDataForCollectTarget(IEnumerable<object> collectTarget, TrialBalanceSheet sheet, DateTime startTime, DateTime endTime)
			{
				int num2 = 0;
				List<object> list2 = new List<object>();
				foreach (object item in collectTarget)
				{
					if (item is Account account3)
					{
						list2.Add(sheet.GetBalanceValue(account3, map.Value, AccNameStyle, base.Setting.CollectAllAccount));
					}
					else if (item is Tuple<Account, AuxiliaryItem> tuple2)
					{
						SubsidiaryLedger subsidiaryLedger = subsidiaryLedgerCacheList[num2];
						if (subsidiaryLedger == null)
						{
							subsidiaryLedger = ledger.GetSubsidiaryLedger(tuple2.Item1, startTime, endTime, tuple2.Item2);
							subsidiaryLedgerCacheList[num2] = subsidiaryLedger;
						}
						list2.Add(subsidiaryLedger.GetBalanceValue(tuple2.Item1, tuple2.Item2, map.Value, AccNameStyle, base.Setting.CollectAllAccount));
					}
					else
					{
						list2.Add(null);
					}
					num2++;
				}
				if (!base.IsSaveCollectResultByColumnId)
				{
					if (base.PositiveValueColumnMaps.ContainsKey(map.Key))
					{
						for (int num3 = list2.Count - 1; num3 >= 0; num3--)
						{
							object obj2 = list2[num3];
							if (obj2 != null && obj2 is decimal num4 && num4 < 0m)
							{
								list2[num3] = null;
							}
						}
					}
					result.Values.Add(base.Setting.Table.Columns.GetById(new Id64(map.Key)), list2);
				}
				else
				{
					result.ValuesOnColumn.Add(map.Key, list2);
				}
			}
		}
		return result;
	}

	protected override void Intelligence(Leqisoft.Model.Table table, string borrowLogic)
	{
		base.Maps.Clear();
		base.PositiveValueColumnMaps.Clear();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		TableCollector tableCollector = DictionarySync.TableCollector;
		foreach (Leqisoft.Model.Column column in table.Columns)
		{
			string text = tableCollector.IntelligenceBalanceCol(column.CaptionDisplay, borrowLogic);
			if (text != null && !dictionary.ContainsKey(text))
			{
				base.Maps.Add(column.Id.Value, text);
				dictionary.Add(text, null);
			}
		}
		foreach (long key2 in base.Maps.Keys)
		{
			string key = base.Maps[key2];
			if (OppositeCaptionDic.Balance.TryGetValue(key, out var value) && dictionary.ContainsKey(key) && dictionary.ContainsKey(value))
			{
				base.PositiveValueColumnMaps.Add(key2, value: true);
			}
		}
	}

	public override string Serialize()
	{
		ExportArgs exportArgs = new ExportArgs
		{
			CollectObject = CollectObjectEnum.Balance,
			MonthStart = base.Setting.Start.Month,
			MonthEnd = base.Setting.End.Month,
			Mapping = base.Maps,
			CollectAllCount = base.Setting.CollectAllAccount,
			FillTargetType = base.Setting.FillTargetType
		};
		if (base.Setting.Account != null)
		{
			exportArgs.AccountName = TableCollectorAbstract.GetFullPath(base.Setting.Account);
		}
		return JsonConvert.SerializeObject(exportArgs);
	}

	private List<object> GetSourceFromSetting()
	{
		List<object> list2 = new List<object>();
		TrialBalanceSheet sheet = GetTrialBalanceSheetWithCache(base.Setting.Ledger, base.Setting.Start, base.Setting.End);
		if (base.Setting.CollectAllAccount)
		{
			Filter_AllAccount(list2);
			return list2;
		}
		switch (base.Setting.SubAccountFitlerMode)
		{
		case SubAccountFilterMode.OnlyChildAccountAndAuxiliaryItem:
			Filter_OnlyChildAccountAndAuxItem(base.Setting.Account, list2);
			break;
		case SubAccountFilterMode.AllAboveLevelChild:
			Filter_AllAboveLevelChild(base.Setting.Account, list2, base.Setting.CollectMaxLevel, base.Setting.Account.GetLevel());
			break;
		case SubAccountFilterMode.AllChildAndAuxiliaryItem:
			Filter_AllChildAndAuxiliaryItem(base.Setting.Account, list2);
			break;
		}
		return list2;
		static void Filter_AllAboveLevelChild(Account parent, List<object> list, int aboveLevel, int parentLevel)
		{
			if (parent != null)
			{
				int num = parentLevel + 1;
				if (num > aboveLevel)
				{
					list.Add(parent);
				}
				else
				{
					bool flag3 = false;
					if (parent.Children.Count > 0)
					{
						flag3 = true;
						foreach (Account child in parent.Children)
						{
							Filter_AllAboveLevelChild(child, list, aboveLevel, num);
						}
					}
					if (!flag3 && parentLevel <= aboveLevel)
					{
						list.Add(parent);
					}
				}
			}
		}
		void Filter_AllAccount(List<object> list)
		{
			switch (base.Setting.SubAccountFitlerMode)
			{
			case SubAccountFilterMode.OnlyChildAccountAndAuxiliaryItem:
			{
				foreach (Account item in base.Setting.Ledger.RootAccounts.OrderBy((Account u) => u.Code))
				{
					list.Add(item);
				}
				break;
			}
			case SubAccountFilterMode.AllAboveLevelChild:
			{
				foreach (Account item2 in base.Setting.Ledger.RootAccounts.OrderBy((Account u) => u.Code))
				{
					Filter_AllAboveLevelChild(item2, list, base.Setting.CollectMaxLevel, 0);
				}
				break;
			}
			case SubAccountFilterMode.AllChildAndAuxiliaryItem:
			{
				foreach (Account item3 in base.Setting.Ledger.RootAccounts.OrderBy((Account u) => u.Code))
				{
					Filter_AllChildAndAuxiliaryItem(item3, list);
				}
				break;
			}
			}
		}
		void Filter_AllChildAndAuxiliaryItem(Account parent, List<object> list)
		{
			if (parent != null)
			{
				bool flag4 = false;
				AuxiliaryClass firstOrDefaultAuxiliary2 = TableCollectorAbstract.GetFirstOrDefaultAuxiliary(base.Setting.Ledger, parent, sheet);
				if (firstOrDefaultAuxiliary2 != null)
				{
					ClassBalance classBalance2 = sheet.End[parent].ClassBalances[firstOrDefaultAuxiliary2];
					List<AuxiliaryItem> list4 = (from t in classBalance2.ItemBalances
						orderby t.Key.Code
						select t.Key).ToList();
					if (list4.Count > 0)
					{
						flag4 = true;
						foreach (AuxiliaryItem item4 in list4)
						{
							list.Add(Tuple.Create(parent, item4));
						}
					}
				}
				bool flag5 = false;
				if (parent.Children.Count > 0)
				{
					flag5 = true;
					foreach (Account child2 in parent.Children)
					{
						Filter_AllChildAndAuxiliaryItem(child2, list);
					}
				}
				if (!flag4 && !flag5)
				{
					list.Add(parent);
				}
			}
		}
		void Filter_OnlyChildAccountAndAuxItem(Account parent, List<object> list)
		{
			if (parent != null)
			{
				bool flag = false;
				AuxiliaryClass firstOrDefaultAuxiliary = TableCollectorAbstract.GetFirstOrDefaultAuxiliary(base.Setting.Ledger, parent, sheet);
				if (firstOrDefaultAuxiliary != null)
				{
					ClassBalance classBalance = sheet.End[parent].ClassBalances[firstOrDefaultAuxiliary];
					List<AuxiliaryItem> list3 = (from t in classBalance.ItemBalances
						orderby t.Key.Code
						select t.Key).ToList();
					if (list3.Count > 0)
					{
						flag = true;
						foreach (AuxiliaryItem item5 in list3)
						{
							list.Add(Tuple.Create(parent, item5));
						}
					}
				}
				bool flag2 = false;
				if (parent.Children.Count > 0)
				{
					flag2 = true;
					foreach (Account child3 in parent.Children)
					{
						list.Add(child3);
					}
				}
				if (!flag && !flag2)
				{
					list.Add(parent);
				}
			}
		}
	}
}
