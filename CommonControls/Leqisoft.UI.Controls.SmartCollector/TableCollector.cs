using System;
using System.Collections.Generic;
using System.Reflection;
using Leqisoft.UI.Controls.CollectDic;
using Leqisoft.UI.Controls.CollectTable;

namespace Leqisoft.UI.Controls.SmartCollector;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class TableCollector
{
	protected const string CN_TABLE1_COLLECTOBJECT = "collectobject";

	protected const string CN_TABLE1_TABLENAMES = "tablenames";

	protected const string CN_TABLE2_ACCOUNTNAME = "accountName";

	protected const string CN_TABLE2_LENDINGLOGIC = "lendingLogic";

	protected const string CN_TABLE3_BALANCECOLS = "balancecols";

	protected const string CN_TABLE3_LENDINGLOGIC = "lendinglogic";

	protected const string CN_TABLE3_TABLECOLS = "tablecols";

	protected const string CN_TABLE4_SUBSIDIARYCOLS = "subsidiarycols";

	protected const string CN_TABLE4_TABLECOLS = "tablecols";

	protected const string CN_TABLE5_MONTHCOLS = "monthcols";

	protected const string CN_TABLE5_TABLECOLS = "tablecols";

	protected const string CN_TABLE6_TABLENAMES = "tablenames";

	protected const string CN_TABLE6_ACCOUNTNAME = "accountName";

	protected const string CN_TABLE6_LENDINGLOGIC = "lendinglogic";

	public ETable TableTitleToCollectObjectMap { get; set; } = new ETable();


	public ETable AccountNameToBorrowLogicMap { get; set; } = new ETable();


	public ETable TableColToBalanceColMap { get; set; } = new ETable();


	public ETable TableColToSubsidiaryColMap { get; set; } = new ETable();


	public ETable TableColToSummaryColMap { get; set; } = new ETable();


	public ETable TableTitleToSelectAllAccountMap { get; set; } = new ETable();


	public ETable TableTitleToSelectSomeAccountMap { get; set; } = new ETable();


	public ETable TableTitleToSelectDetailAccountMap { get; set; } = new ETable();


	public int Version { get; set; }

	public CollectObjectEnum? IntellegenceObject(string tableTitle)
	{
		ERow eRow = TableTitleToCollectObjectMap.FindRow("tablenames", (ECell c) => c.AnyMatch(tableTitle));
		if (eRow == null)
		{
			return null;
		}
		return eRow["collectobject"].Value switch
		{
			"科目余额表" => CollectObjectEnum.Balance, 
			"明细账" => CollectObjectEnum.Subsidiary, 
			"月度汇总表" => CollectObjectEnum.Summary, 
			"科目余额表_全部科目" => CollectObjectEnum.Balance, 
			"明细账_明细科目识别" => CollectObjectEnum.Subsidiary, 
			"明细账_全部科目" => CollectObjectEnum.Subsidiary, 
			_ => null, 
		};
	}

	public bool IntellegenceIsNeedSelectAllAccount(string tableTitle)
	{
		ERow eRow = TableTitleToSelectAllAccountMap.FindRow("tablenames", (ECell c) => c.AnyMatch(tableTitle));
		if (eRow == null)
		{
			return false;
		}
		return true;
	}

	public bool IntellegenceIsNeedSelectSomeAccount(string tableTitle, out string borrowLogic, out CollectItemShouldSelectFilter selectFilter)
	{
		ERow eRow = TableTitleToSelectSomeAccountMap.FindRow("tablenames", (ECell c) => c.AnyMatch(tableTitle));
		if (eRow == null)
		{
			borrowLogic = null;
			selectFilter = null;
			return false;
		}
		selectFilter = new CollectItemShouldSelectFilter(eRow["accountName"].Values);
		borrowLogic = eRow["lendinglogic"].Value;
		return true;
	}

	public bool IntellegenceIsNeedSelectDetailAccount(string tableTitle)
	{
		ERow eRow = TableTitleToSelectDetailAccountMap.FindRow("tablenames", (ECell c) => c.AnyMatch(tableTitle));
		if (eRow == null)
		{
			return false;
		}
		return true;
	}

	public Tuple<string, List<string>> IntellegenceLogic(string tableTitle)
	{
		ERow eRow = AccountNameToBorrowLogicMap.FindRow("accountName", (ECell c) => c.AnyMatch(tableTitle));
		if (eRow == null)
		{
			return null;
		}
		string value = eRow["lendingLogic"].Value;
		ECell eCell = eRow["accountName"];
		return Tuple.Create(value, eCell.Values);
	}

	public string IntelligenceBalanceCol(string colCaption, string borrowLogic)
	{
		if (borrowLogic == null)
		{
			return TableColToBalanceColMap.FindRow((ERow r) => r["tablecols"].AnyMatch(colCaption))?["balancecols"].Value;
		}
		ERow eRow = TableColToBalanceColMap.FindRow((ERow r) => r["tablecols"].AnyMatch(colCaption) && r["lendinglogic"].AnyEquals(borrowLogic));
		if (eRow == null)
		{
			eRow = TableColToBalanceColMap.FindRow((ERow r) => r["tablecols"].AnyMatch(colCaption) && r["lendinglogic"].AnyMatch(string.Empty));
		}
		return eRow?["balancecols"].Value;
	}

	public string IntelligenceSubsidiaryCol(string colCaption)
	{
		return TableColToSubsidiaryColMap.FindRow("tablecols", (ECell c) => c.AnyMatch(colCaption))?["subsidiarycols"].Value;
	}

	public string IntelligenceSummaryCol(string colCaption)
	{
		return TableColToSummaryColMap.FindRow("tablecols", (ECell c) => c.AnyMatch(colCaption))?["monthcols"].Value;
	}
}
