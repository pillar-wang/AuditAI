using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Leqisoft.Model;
using Leqisoft.UI.Controls.CollectCell;
using Leqisoft.UI.Controls.SmartCollector;
using Newtonsoft.Json;

namespace Leqisoft.UI.Controls.CellCollect;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class CollectManager
{
	[JsonProperty(PropertyName = "CollectObject")]
	public CollectObjectEnum CollectObject { get; set; }

	[JsonProperty(PropertyName = "Values")]
	public List<CollectItem> CollectItems { get; set; }

	public CollectManager()
	{
		CollectObject = CollectObjectEnum.Balance;
		CollectItems = new List<CollectItem>();
	}

	public decimal? GetValue(Ledger ledger, int auditYear)
	{
		if (CollectItems.Count > 0)
		{
			CollectItem collectItem = CollectItems.First();
			Leqisoft.UI.Controls.CollectCell.Operand operand = null;
			operand = collectItem.GetOperand(ledger, auditYear);
			operand = operand.First();
			foreach (CollectItem item in CollectItems.Skip(1))
			{
				Leqisoft.UI.Controls.CollectCell.Operand operand2 = null;
				operand2 = item.GetOperand(ledger, auditYear);
				operand = operand.With(operand2);
			}
			return operand.Value;
		}
		return null;
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Objects
		});
	}

	public static CollectManager Parse(string json)
	{
		return JsonConvert.DeserializeObject<CollectManager>(json, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Objects
		});
	}

	public static bool CanCollect(Table table)
	{
		table.LoadAndReturn();
		foreach (Cell cell in table.Cells)
		{
			if (!string.IsNullOrWhiteSpace(cell.CollectSource))
			{
				return true;
			}
		}
		if (table.Columns.Count < 2)
		{
			return false;
		}
		CellCollector cellCollector = DictionarySync.CellCollector;
		string displayValue = table.Title.TitleCell.GetDisplayValue();
		if (!cellCollector.IntelligenceFillingTable(displayValue))
		{
			return false;
		}
		for (int i = 0; i < table.Columns.Count; i++)
		{
			string captionDisplay = table.Columns[i].CaptionDisplay;
			if (!cellCollector.IntelligenceFillingCol(captionDisplay))
			{
				continue;
			}
			for (int num = i - 1; num >= 0; num--)
			{
				string captionDisplay2 = table.Columns[num].CaptionDisplay;
				if (cellCollector.IntelligenceConditionCol(captionDisplay2))
				{
					return true;
				}
			}
		}
		return false;
	}
}
