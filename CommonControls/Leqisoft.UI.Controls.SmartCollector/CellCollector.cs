﻿﻿﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Leqisoft.UI.Controls.CollectDic;

namespace Leqisoft.UI.Controls.SmartCollector;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class CellCollector
{
	private const string CN_CELL1_TABLENAMES = "tablenames";

	private const string CN_CELL1_CONDITIONCOLS = "conditioncols";

	private const string CN_CELL1_FILLCOLS = "fillcols";

	private const string CN_CELL1_COLLECTOBJECT = "collectobject";

	private const string CN_CELL2_CONDITIONCOLNAME = "conditioncolname";

	private const string CN_CELL2_BALANCEACCOUNTNAME = "balanceaccount";

	private const string CN_CELL2_LENDINGLOGIC = "lendinglogic";

	private const string CN_CELL3_FILLCOLS = "fillcols";

	private const string CN_CELL3_BALANCECOL = "balancecol";

	public ETable CollectObject { get; set; }

	public ETable ConditionColToBalance { get; set; }

	public ETable FillingColToBalance { get; set; }

	public int Version { get; set; }

	public CellCollector()
	{
		CollectObject = new ETable();
		ConditionColToBalance = new ETable();
		FillingColToBalance = new ETable();
	}

	public bool IntelligenceFillingTable(string tableTitle)
	{
		ERow eRow = CollectObject.FindRow("tablenames", (ECell c) => c.AnyMatch(tableTitle));
		if (eRow == null)
		{
			return false;
		}
		return true;
	}

	public bool IntelligenceFillingCol(string colCaption)
	{
		ERow eRow = CollectObject.FindRow("fillcols", (ECell c) => c.AnyMatch(colCaption));
		if (eRow == null)
		{
			return false;
		}
		return true;
	}

	public bool IntelligenceConditionCol(string colCaption)
	{
		ERow eRow = CollectObject.FindRow("conditioncols", (ECell c) => c.AnyMatch(colCaption));
		if (eRow == null)
		{
			return false;
		}
		return true;
	}

	public Tuple<string, List<string>> IntelligenceColToLogic(string conditionColValue)
	{
		ERow eRow = ConditionColToBalance.FindRow("conditioncolname", (ECell c) => c.AnyMatch(conditionColValue));
		if (eRow == null)
		{
			return null;
		}
		string value = eRow["lendinglogic"].Value;
		ECell eCell = eRow["balanceaccount"];
		return Tuple.Create(value, eCell.Values);
	}

	public string IntelligenceColToBalance(string fillingColCaption)
	{
		return FillingColToBalance.FindRow("fillcols", (ECell c) => c.AnyMatch(fillingColCaption))?["balancecol"].Value;
	}

	public bool ContainAccount(string accountName)
	{
		ERow eRow = ConditionColToBalance.FindRow("balanceaccount", (ECell c) => c.AnyMatch(accountName));
		return eRow != null;
	}
}
