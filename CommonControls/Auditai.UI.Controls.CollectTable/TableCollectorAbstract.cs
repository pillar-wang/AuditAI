using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Auditai.Model;
using Auditai.UI.Controls.SmartCollector;
using Newtonsoft.Json;

namespace Auditai.UI.Controls.CollectTable;

public abstract class TableCollectorAbstract
{
	protected const string CN_PROJECTNAME = "项目";

	protected const string CN_ACCOUNTCODE = "科目代码";

	protected const string CN_ACCOUNTNAME = "科目名称";

	protected const string CN_PRIORDEBIT = "上期借方发生额";

	protected const string CN_PRIORCREDIT = "上期贷方发生额";

	protected const string CN_CURRENT_MONTH_DEBIT = "本月借方发生额";

	protected const string CN_CURRENT_MONTH_CREDIT = "本月贷方发生额";

	protected List<Tuple<DateTime, DateTime, TrialBalanceSheet>> _trialBalanceSheetCache = new List<Tuple<DateTime, DateTime, TrialBalanceSheet>>();

	private bool _cacheNotEmptyAccountSetValid = true;

	private Tuple<Ledger, HashSet<Account>> _cacheNotEmptyAccountSet;

	public CollectObjectEnum CollectObject { get; protected set; }

	public Setting Setting { get; protected set; }

	public IEnumerable<object> Source { get; set; }

	public Dictionary<long, string> Maps { get; set; }

	public Dictionary<long, bool> PositiveValueColumnMaps { get; set; }

	[JsonIgnore]
	public Tuple<DateTime, DateTime> TitlePeriod { get; private set; }

	[JsonIgnore]
	internal bool IsSaveCollectResultByColumnId { get; set; }

	[JsonIgnore]
	public bool IsNeedRunEmptyAccountCheckToSetSelectStatus { get; protected set; }

	public TableCollectorAbstract(Ledger ledger, Table table)
	{
		CollectObject = CollectObjectEnum.Balance;
		Setting = new Setting();
		Setting.Table = table;
		Setting.Ledger = ledger;
		Tuple<DateTime, DateTime> auditYear = DictionarySync.GetAuditYear(table);
		if (auditYear == null)
		{
			throw new InvalidAuditYearException("未在标题区中发现会计截止日或会计期间信息，请在标题区中输入会计截止日或会计期间信息后，再进行采账填充操作！");
		}
		Setting.Start = auditYear.Item1;
		Setting.End = auditYear.Item2;
		TitlePeriod = auditYear;
		Maps = new Dictionary<long, string>();
		PositiveValueColumnMaps = new Dictionary<long, bool>();
	}

	public abstract void ThrowExceptionIfUnExpectAuditYear(int auditYear);

	public abstract void ThrowExceptionIfUnExpectSource();

	public abstract TableCollectResult Collect(int auditYear);

	protected abstract void Intelligence(Table table, string borrowLogic);

	public abstract string Serialize();

	public abstract bool IsAbleUseCurrentSettingToCollectData();

	protected virtual void OnAfterDeSerialize()
	{
	}

	public static bool CanCollect(Table table)
	{
		table.LoadAndReturn();
		string collectSource = table.CollectSource;
		if (!string.IsNullOrWhiteSpace(collectSource))
		{
			try
			{
				ExportArgs exportArgs = JsonConvert.DeserializeObject<ExportArgs>(collectSource);
				bool flag = !string.IsNullOrWhiteSpace(exportArgs.AccountName);
				if ((exportArgs.CollectObject == CollectObjectEnum.Balance && exportArgs.CollectAllCount) || (exportArgs.CollectObject == CollectObjectEnum.Subsidiary && (exportArgs.CollectAllCount || exportArgs.IsOnlyMyMark)))
				{
					flag = true;
				}
				if (exportArgs != null && flag && exportArgs.Mapping.Count > 0)
				{
					if (exportArgs.FillTargetType == CollectFillTargetType.Ticket && table is TicketCollectFillTable)
					{
						return true;
					}
					if (exportArgs.FillTargetType == CollectFillTargetType.Table)
					{
						return true;
					}
				}
			}
			catch (Exception)
			{
			}
		}
		TableCollector tableCollector = DictionarySync.TableCollector;
		string displayValue = table.Title.TitleCell.GetDisplayValue();
		CollectObjectEnum? collectObjectEnum = tableCollector.IntellegenceObject(displayValue);
		if (!collectObjectEnum.HasValue)
		{
			return false;
		}
		if (tableCollector.IntellegenceIsNeedSelectAllAccount(displayValue))
		{
			return true;
		}
		if (tableCollector.IntellegenceIsNeedSelectSomeAccount(displayValue, out var _, out var _))
		{
			return true;
		}
		Tuple<string, List<string>> tuple = tableCollector.IntellegenceLogic(displayValue);
		if (tuple == null)
		{
			return false;
		}
		string item = tuple.Item1;
		switch (collectObjectEnum.Value)
		{
		case CollectObjectEnum.Balance:
			foreach (Column column in table.Columns)
			{
				string text3 = tableCollector.IntelligenceBalanceCol(column.CaptionDisplay, item);
				if (text3 != null)
				{
					return true;
				}
			}
			break;
		case CollectObjectEnum.Subsidiary:
			foreach (Column column2 in table.Columns)
			{
				string text2 = tableCollector.IntelligenceSubsidiaryCol(column2.CaptionDisplay);
				if (text2 != null)
				{
					return true;
				}
			}
			break;
		case CollectObjectEnum.Summary:
			foreach (Column column3 in table.Columns)
			{
				string text = tableCollector.IntelligenceSummaryCol(column3.CaptionDisplay);
				if (text != null)
				{
					return true;
				}
			}
			break;
		}
		return false;
	}

	public void IntelligenceShouldSelectedAccounts(Ledger ledger, Table table)
	{
		Setting.CheckCollectItemShouldBeSelectedFilter = null;
		if (CollectObject == CollectObjectEnum.Balance && (Setting.CollectAllAccount || Setting.Account == null))
		{
			TableCollector tableCollector = DictionarySync.TableCollector;
			string displayValue = table.Title.TitleCell.GetDisplayValue();
			if (tableCollector.IntellegenceIsNeedSelectSomeAccount(displayValue, out var _, out var selectFilter))
			{
				Setting.CollectAllAccount = true;
				Setting.CheckCollectItemShouldBeSelectedFilter = selectFilter;
			}
		}
	}

	public void IntelligenceShouldSelectedDetailAccount(Ledger ledger, Table table)
	{
		Setting.IsNeedSelectDetailAccount = false;
		if (CollectObject != CollectObjectEnum.Subsidiary)
		{
			return;
		}
		TableCollector tableCollector = DictionarySync.TableCollector;
		string displayValue = table.Title.TitleCell.GetDisplayValue();
		Tuple<string, List<string>> tuple = tableCollector.IntellegenceLogic(displayValue);
		if (tuple == null)
		{
			return;
		}
		string item = tuple.Item1;
		List<string> accountNamesRegexs = tuple.Item2;
		Account account = ledger.GetLevelOrderAccounts().FirstOrDefault((Account a) => accountNamesRegexs.Any((string r) => Regex.IsMatch(a.Name.Trim(), r)));
		if (account != null)
		{
			Setting.IsNeedSelectDetailAccount = tableCollector.IntellegenceIsNeedSelectDetailAccount(displayValue);
			if (Setting.IsNeedSelectDetailAccount)
			{
				Setting.Account = account;
				Setting.IsOnlyMyMark = false;
			}
		}
	}

	public static TableCollectorAbstract Intelligence(Ledger ledger, Table table)
	{
		if (ledger == null)
		{
			throw new InvalidCollectSettingException("账套不允许为空");
		}
		if (table == null)
		{
			throw new InvalidCollectSettingException("表格不允许为空");
		}
		TableCollector tableCollector = DictionarySync.TableCollector;
		string displayValue = table.Title.TitleCell.GetDisplayValue();
		CollectObjectEnum? collectObjectEnum = tableCollector.IntellegenceObject(displayValue);
		if (!collectObjectEnum.HasValue)
		{
			return null;
		}
		TableCollectorAbstract tableCollectorAbstract = null;
		switch (collectObjectEnum.Value)
		{
		case CollectObjectEnum.Balance:
			tableCollectorAbstract = new TableCollectorBalance(ledger, table);
			break;
		case CollectObjectEnum.Subsidiary:
			tableCollectorAbstract = new TableCollectorSubsidiary(ledger, table);
			tableCollectorAbstract.Setting.IsOnlyMyMark = true;
			break;
		case CollectObjectEnum.Summary:
			tableCollectorAbstract = new TableCollectorSummary(ledger, table);
			break;
		default:
			return null;
		}
		tableCollectorAbstract.Setting.Start = tableCollectorAbstract.TitlePeriod.Item1;
		tableCollectorAbstract.Setting.End = tableCollectorAbstract.TitlePeriod.Item2;
		tableCollectorAbstract.Setting.CollectAllAccount = tableCollector.IntellegenceIsNeedSelectAllAccount(displayValue);
		if (tableCollectorAbstract.Setting.CollectAllAccount)
		{
			tableCollectorAbstract.Intelligence(table, null);
			return tableCollectorAbstract;
		}
		tableCollectorAbstract.Setting.CheckCollectItemShouldBeSelectedFilter = null;
		if (tableCollector.IntellegenceIsNeedSelectSomeAccount(displayValue, out var borrowLogic, out var selectFilter))
		{
			tableCollectorAbstract.Setting.CollectAllAccount = true;
			tableCollectorAbstract.Setting.CheckCollectItemShouldBeSelectedFilter = selectFilter;
			tableCollectorAbstract.Intelligence(table, borrowLogic);
			return tableCollectorAbstract;
		}
		Tuple<string, List<string>> tuple = tableCollector.IntellegenceLogic(displayValue);
		if (tuple == null)
		{
			return tableCollectorAbstract;
		}
		string item = tuple.Item1;
		List<string> accountNamesRegexs = tuple.Item2;
		Account account = ledger.GetLevelOrderAccounts().FirstOrDefault((Account a) => accountNamesRegexs.Any((string r) => Regex.IsMatch(a.Name.Trim(), r)));
		if (account == null)
		{
			return tableCollectorAbstract;
		}
		Account account2 = null;
		tableCollectorAbstract.Setting.Account = account2 ?? account;
		tableCollectorAbstract.Setting.Auxiliary = GetFirstOrDefaultAuxiliary(ledger, tableCollectorAbstract.Setting.Account);
		tableCollectorAbstract.Intelligence(table, item);
		if (tableCollectorAbstract.CollectObject == CollectObjectEnum.Subsidiary && tableCollectorAbstract.Setting.Account != null)
		{
			tableCollectorAbstract.Setting.IsNeedSelectDetailAccount = tableCollector.IntellegenceIsNeedSelectDetailAccount(displayValue);
			tableCollectorAbstract.Setting.IsOnlyMyMark = false;
		}
		return tableCollectorAbstract;
	}

	public static AuxiliaryClass GetFirstOrDefaultAuxiliary(Ledger ledger, Account account, TrialBalanceSheet sheet = null)
	{
		sheet = sheet ?? ledger.GetTrialBalanceSheet(ledger.StartDate, ledger.GetEndDate());
		if (sheet.End.ContainsKey(account))
		{
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = sheet.End[account].ClassBalances;
			if (classBalances.Count > 0)
			{
				if (classBalances.Any((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => Regex.IsMatch(cb.Key.Name, ".*客户.*")))
				{
					return classBalances.First((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => Regex.IsMatch(cb.Key.Name, ".*客户.*")).Key;
				}
				if (classBalances.Any((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => Regex.IsMatch(cb.Key.Name, ".*供应商.*")))
				{
					return classBalances.First((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => Regex.IsMatch(cb.Key.Name, ".*供应商.*")).Key;
				}
				if (classBalances.Any((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => Regex.IsMatch(cb.Key.Name, ".*项目.*")))
				{
					return classBalances.First((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => Regex.IsMatch(cb.Key.Name, ".*项目.*")).Key;
				}
				int max = classBalances.Max((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => cb.Value.ItemBalances.Count);
				return classBalances.FirstOrDefault((KeyValuePair<AuxiliaryClass, ClassBalance> cb) => cb.Value.ItemBalances.Count == max).Key;
			}
		}
		return null;
	}

	public static TableCollectorAbstract Deserialize(string formula, Ledger ledger, Table table)
	{
		if (ledger == null)
		{
			throw new ArgumentNullException("账套不允许为空");
		}
		if (table == null)
		{
			throw new ArgumentNullException("表格不允许为空");
		}
		if (string.IsNullOrWhiteSpace(formula))
		{
			return null;
		}
		ExportArgs exportArgs = null;
		try
		{
			exportArgs = JsonConvert.DeserializeObject<ExportArgs>(formula);
		}
		catch (Exception)
		{
			return null;
		}
		TableCollectorAbstract tableCollectorAbstract = null;
		switch (exportArgs.CollectObject)
		{
		case CollectObjectEnum.Balance:
			tableCollectorAbstract = new TableCollectorBalance(ledger, table);
			break;
		case CollectObjectEnum.Subsidiary:
			tableCollectorAbstract = new TableCollectorSubsidiary(ledger, table);
			break;
		case CollectObjectEnum.Summary:
			tableCollectorAbstract = new TableCollectorSummary(ledger, table);
			break;
		default:
			return null;
		}
		int year = tableCollectorAbstract.TitlePeriod.Item1.Year;
		tableCollectorAbstract.Setting.Start = new DateTime(year, exportArgs.MonthStart, 1);
		tableCollectorAbstract.Setting.End = new DateTime(year, exportArgs.MonthEnd, DateTime.DaysInMonth(year, exportArgs.MonthEnd)).AddDays(1.0).AddMilliseconds(-1.0);
		tableCollectorAbstract.Setting.Account = ledger.Accounts.Find((Account t) => GetFullPath(t) == exportArgs.AccountName);
		tableCollectorAbstract.Setting.CollectAllAccount = exportArgs.CollectAllCount;
		TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(ledger.StartDate, ledger.GetEndDate());
		if (tableCollectorAbstract.Setting.Account != null && trialBalanceSheet.End.ContainsKey(tableCollectorAbstract.Setting.Account))
		{
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = trialBalanceSheet.End[tableCollectorAbstract.Setting.Account].ClassBalances;
			tableCollectorAbstract.Setting.Auxiliary = classBalances.Keys.FirstOrDefault((AuxiliaryClass t) => t.Name == exportArgs.AuxName);
			tableCollectorAbstract.Setting.Auxiliary = tableCollectorAbstract.Setting.Auxiliary ?? classBalances.Keys.SelectMany((AuxiliaryClass c) => c.Items).FirstOrDefault((AuxiliaryItem item) => item.Name == exportArgs.AuxName);
		}
		tableCollectorAbstract.Maps = exportArgs.Mapping;
		if (tableCollectorAbstract is TableCollectorSummary tableCollectorSummary)
		{
			tableCollectorSummary.AnalysisProject = exportArgs.Analysis;
		}
		tableCollectorAbstract.Setting.FillTargetType = exportArgs.FillTargetType;
		tableCollectorAbstract.Setting.IsOnlyMyMark = exportArgs.IsOnlyMyMark;
		tableCollectorAbstract.OnAfterDeSerialize();
		return tableCollectorAbstract;
	}

	protected static string GetFullPath(Account account)
	{
		return string.Join("-", account.AncestorsAndSelf.Select((Account p) => p.Name));
	}

	protected TrialBalanceSheet GetTrialBalanceSheetWithCache(Ledger ledger, DateTime start, DateTime end)
	{
		Tuple<DateTime, DateTime, TrialBalanceSheet> tuple = _trialBalanceSheetCache.Find((Tuple<DateTime, DateTime, TrialBalanceSheet> tp) => tp.Item1.Equals(start) && tp.Item2.Equals(end));
		if (tuple != null)
		{
			return tuple.Item3;
		}
		TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(start, end);
		_trialBalanceSheetCache.Add(Tuple.Create(start, end, trialBalanceSheet));
		return trialBalanceSheet;
	}

	public bool IsEmptyAccountWithCache(Account account)
	{
		if (!_cacheNotEmptyAccountSetValid || _cacheNotEmptyAccountSet == null || account.Ledger != _cacheNotEmptyAccountSet.Item1)
		{
			HashSet<Account> hashSet = new HashSet<Account>();
			foreach (Voucher voucher in account.Ledger.Vouchers)
			{
				hashSet.Add(voucher.Account);
			}
			_cacheNotEmptyAccountSet = Tuple.Create(account.Ledger, hashSet);
			_cacheNotEmptyAccountSetValid = true;
		}
		if (_cacheNotEmptyAccountSet.Item2.Contains(account))
		{
			return false;
		}
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(account.Ledger, account.Ledger.StartDate, account.Ledger.EndDate);
		if (trialBalanceSheetWithCache.Start[account].Total == 0m && trialBalanceSheetWithCache.Debit[account].Total == 0m && trialBalanceSheetWithCache.Credit[account].Total == 0m)
		{
			return !account.Descendants.Any((Account c) => _cacheNotEmptyAccountSet.Item2.Contains(c));
		}
		return false;
	}
}
