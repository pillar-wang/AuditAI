using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Leqisoft.Model;
using Leqisoft.UI.Controls.CellCollect;
using Leqisoft.UI.Controls.SmartCollector;

namespace Leqisoft.UI.Controls.CollectCell;

public class CollectorManager
{
	public CollectManager Collector { get; set; }

	public Ledger Ledger { get; set; }

	public Table Table { get; set; }

	public Tuple<DateTime, DateTime> TitlePeriod { get; private set; }

	public CollectorManager(Ledger ledger, Table table, Tuple<DateTime, DateTime> initPeriod)
	{
		Collector = new CollectManager();
		Ledger = ledger;
		Table = table;
		TitlePeriod = initPeriod;
	}

	public void LoadFormula(string formula)
	{
		Collector = new CollectManager();
		if (string.IsNullOrWhiteSpace(formula))
		{
			return;
		}
		try
		{
			Collector = CollectManager.Parse(formula);
		}
		catch (Exception)
		{
			Collector = new CollectManager();
		}
	}

	public void Intelligence(int row, int col)
	{
		Collector = IntelligenceImpl(row, col);
		Collector = Collector ?? new CollectManager();
	}

	public void Apply(out decimal? value, out string formula)
	{
		formula = Collector.Serialize();
		value = Collector.GetValue(Ledger, TitlePeriod.Item1.Year);
		formula = ((!value.HasValue) ? null : formula);
	}

	private CollectManager IntelligenceImpl(int row, int col)
	{
		CollectManager collectManager = new CollectManager();
		CellCollector cellCollector = DictionarySync.CellCollector;
		string displayValue = Table.Title.TitleCell.GetDisplayValue();
		if (!cellCollector.IntelligenceFillingTable(displayValue))
		{
			return null;
		}
		string captionDisplay = Table.Columns[col].CaptionDisplay;
		if (!cellCollector.IntelligenceFillingCol(captionDisplay))
		{
			return null;
		}
		string text = string.Empty;
		for (int num = col - 1; num >= 0; num--)
		{
			string captionDisplay2 = Table.Columns[num].CaptionDisplay;
			if (cellCollector.IntelligenceConditionCol(captionDisplay2))
			{
				text = Table[row, num].Value?.ToString();
				break;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string conditionColValue = formatParse(text);
		Tuple<string, List<string>> tuple = cellCollector.IntelligenceColToLogic(conditionColValue);
		if (tuple == null)
		{
			return null;
		}
		BorrowLogicEnum borrowLogicEnum = ((tuple.Item1 == "借增贷减") ? BorrowLogicEnum.BorrowGrow : ((!(tuple.Item1 == "借减贷增")) ? BorrowLogicEnum.None : BorrowLogicEnum.LoanGrow));
		List<string> conditions = new List<string>();
		foreach (string balanceName in tuple.Item2)
		{
			Account account = Ledger.Accounts.Find((Account t) => t.Parent == null && Regex.IsMatch(formatParse(t.Name), formatParse(balanceName)));
			if (account == null)
			{
				continue;
			}
			string text2 = cellCollector.IntelligenceColToBalance(captionDisplay);
			if (text2 == null)
			{
				continue;
			}
			AmountEnum amountEnum = AmountEnum.CreditAmount;
			switch (text2)
			{
			case "期初余额":
				amountEnum = ((borrowLogicEnum == BorrowLogicEnum.BorrowGrow) ? AmountEnum.DebitBegin : AmountEnum.CreditBegin);
				break;
			case "期末余额":
				amountEnum = ((borrowLogicEnum == BorrowLogicEnum.BorrowGrow) ? AmountEnum.DebitBalance : AmountEnum.CreditBalance);
				break;
			case "本期借方发生额":
			case "本期贷方发生额":
				amountEnum = ((borrowLogicEnum != BorrowLogicEnum.BorrowGrow) ? AmountEnum.CreditAmount : AmountEnum.DebitAmount);
				break;
			case "上期借方发生额":
			case "上期贷方发生额":
				amountEnum = ((borrowLogicEnum == BorrowLogicEnum.BorrowGrow) ? AmountEnum.PreDebitAmount : AmountEnum.PreCreditAmount);
				break;
			default:
				continue;
			}
			IEnumerable<Account> enumerable = account.Descendants.Where((Account t) => conditions.Contains(t.Code));
			foreach (Account des in enumerable)
			{
				conditions.RemoveAll((string t) => t.Equals(des.Code));
			}
			if (!account.Ancestors.Any((Account t) => conditions.Contains(t.Code)))
			{
				BalanceItem item = new BalanceItem
				{
					StartTime = TitlePeriod.Item1,
					EndTime = TitlePeriod.Item2,
					Operation = OperateEnum.Add,
					AccountCode = account.Code,
					AmountEnum = amountEnum
				};
				collectManager.CollectItems.Add(item);
				conditions.Add(account.Code);
			}
		}
		return collectManager;
	}

	public static string formatParse(string str)
	{
		return str.Replace(" ", "").Replace("它", "他").Replace("帐", "账")
			.Replace("：", ":")
			.Replace("，", ",")
			.Replace("（", "(")
			.Replace("）", ")")
			.Replace("“", "\"")
			.Replace("\n", "")
			.Replace("\r", "")
			.Replace("”", "\"")
			.Replace("一、", "")
			.Replace("二、", "")
			.Replace("三、", "")
			.Replace("四、", "")
			.Replace("五、", "")
			.Replace("六、", "")
			.Replace("七、", "")
			.Replace("八、", "")
			.Replace("九、", "")
			.Replace("十、", "")
			.Replace("加:", "")
			.Replace("减:", "")
			.Replace("其中:", "");
	}
}
