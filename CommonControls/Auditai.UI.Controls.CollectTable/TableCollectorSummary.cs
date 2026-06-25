using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls.SmartCollector;
using Newtonsoft.Json;

namespace Auditai.UI.Controls.CollectTable;

public class TableCollectorSummary : TableCollectorAbstract
{
	internal AnalysisProject AnalysisProject { get; set; }

	internal TableCollectorSummary(Ledger ledger, Auditai.Model.Table table)
		: base(ledger, table)
	{
		base.CollectObject = CollectObjectEnum.Summary;
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

	public override bool IsAbleUseCurrentSettingToCollectData()
	{
		if (base.Setting.Account == null)
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
		TableCollectResult tableCollectResult = new TableCollectResult(this);
		if (base.Setting.Account == null)
		{
			throw new InvalidCollectSettingException("采集器没有设置要查询的科目！");
		}
		if (base.IsSaveCollectResultByColumnId)
		{
			tableCollectResult.ValuesOnColumn = new Dictionary<long, List<object>>();
		}
		List<object> list = base.Source.ToList();
		Dictionary<SubsidiaryLedger, Tuple<string, string>> dictionary = new Dictionary<SubsidiaryLedger, Tuple<string, string>>();
		foreach (object item in list)
		{
			if (!(item is Account account))
			{
				if (item is Tuple<Account, AuxiliaryItem> tuple)
				{
					dictionary.Add(base.Setting.Ledger.GetSubsidiaryLedger(tuple.Item1, base.Setting.Start, base.Setting.End, tuple.Item2), Tuple.Create(tuple.Item1.Code + "-" + tuple.Item2.Code, tuple.Item2.Name));
				}
			}
			else
			{
				dictionary.Add(base.Setting.Ledger.GetSubsidiaryLedger(account, base.Setting.Start, base.Setting.End), Tuple.Create(account.Code, account.Name));
			}
		}
		int year = base.Setting.Ledger.StartDate.Year;
		int year2 = base.Setting.Ledger.GetEndDate().Year;
		foreach (KeyValuePair<long, string> map in base.Maps)
		{
			Auditai.Model.Column column = null;
			if (!base.IsSaveCollectResultByColumnId)
			{
				column = base.Setting.Table.Columns.GetById(new Id64(map.Key));
				if (column == null)
				{
					continue;
				}
			}
			List<object> list2 = new List<object>();
			if (map.Value == "科目代码")
			{
				foreach (KeyValuePair<SubsidiaryLedger, Tuple<string, string>> item2 in dictionary)
				{
					list2.Add(item2.Value.Item1);
				}
			}
			else if (map.Value == "科目名称")
			{
				foreach (KeyValuePair<SubsidiaryLedger, Tuple<string, string>> item3 in dictionary)
				{
					list2.Add(item3.Value.Item2);
				}
			}
			else if (base.Setting.Start.Year < year || base.Setting.End.Year > year2)
			{
				list2 = null;
			}
			else
			{
				foreach (KeyValuePair<SubsidiaryLedger, Tuple<string, string>> item4 in dictionary)
				{
					object summaryValue = item4.Key.GetSummaryValue(map.Value, AnalysisProject);
					list2.Add(summaryValue);
				}
			}
			if (list2 != null)
			{
				if (!base.IsSaveCollectResultByColumnId)
				{
					tableCollectResult.Values.Add(column, list2);
				}
				else
				{
					tableCollectResult.ValuesOnColumn.Add(map.Key, list2);
				}
			}
		}
		return tableCollectResult;
	}

	protected override void Intelligence(Auditai.Model.Table table, string borrowLogic)
	{
		base.Maps.Clear();
		TableCollector tableCollector = DictionarySync.TableCollector;
		foreach (Auditai.Model.Column column in table.Columns)
		{
			string text = tableCollector.IntelligenceSummaryCol(column.CaptionDisplay);
			if (text != null)
			{
				if (text == "项目")
				{
					text = "科目名称";
				}
				base.Maps.Add(column.Id.Value, text);
			}
		}
		if (!(borrowLogic == "借增贷减"))
		{
			if (borrowLogic == "借减贷增")
			{
				AnalysisProject = AnalysisProject.Credits;
			}
		}
		else
		{
			AnalysisProject = AnalysisProject.Debits;
		}
	}

	public override string Serialize()
	{
		if (base.Setting.Account == null)
		{
			return null;
		}
		ExportArgs exportArgs = new ExportArgs
		{
			CollectObject = CollectObjectEnum.Summary,
			MonthStart = base.Setting.Start.Month,
			MonthEnd = base.Setting.End.Month,
			Mapping = base.Maps,
			Analysis = AnalysisProject,
			FillTargetType = base.Setting.FillTargetType
		};
		exportArgs.AccountName = TableCollectorAbstract.GetFullPath(base.Setting.Account);
		return JsonConvert.SerializeObject(exportArgs);
	}

	protected override void OnAfterDeSerialize()
	{
		List<long> list = new List<long>();
		foreach (long item in base.Maps.Keys.ToList())
		{
			string text = base.Maps[item];
			if (text == "项目")
			{
				base.Maps[item] = "科目名称";
			}
			else if (text == "合计")
			{
				list.Add(item);
			}
		}
		foreach (long item2 in list)
		{
			base.Maps.Remove(item2);
		}
	}

	private List<object> GetSourceFromSetting()
	{
		List<object> list2 = new List<object>();
		TrialBalanceSheet sheet = GetTrialBalanceSheetWithCache(base.Setting.Ledger, base.Setting.Start, base.Setting.End);
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
						foreach (AuxiliaryItem item in list4)
						{
							list.Add(Tuple.Create(parent, item));
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
						foreach (AuxiliaryItem item2 in list3)
						{
							list.Add(Tuple.Create(parent, item2));
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
