using System;
using System.Reflection;
using Leqisoft.Model;
using Leqisoft.UI.Controls.CollectCell;
using Newtonsoft.Json;

namespace Leqisoft.UI.Controls.CellCollect;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
[JsonObject("CollectItem")]
public abstract class CollectItem
{
	[JsonProperty(PropertyName = "Operation")]
	public OperateEnum Operation;

	[JsonProperty(PropertyName = "AccountCode")]
	public string AccountCode;

	[JsonProperty(PropertyName = "StartTime")]
	public DateTime StartTime;

	[JsonProperty(PropertyName = "EndTime")]
	public DateTime EndTime;

	[JsonIgnore]
	public string AccountName { get; protected set; }

	public abstract decimal GetValue(Ledger ledger, int auditYear);

	public Leqisoft.UI.Controls.CollectCell.Operand GetOperand(Ledger ledger, int auditYear)
	{
		return new Leqisoft.UI.Controls.CollectCell.Operand(Operation, GetValue(ledger, auditYear));
	}
}
