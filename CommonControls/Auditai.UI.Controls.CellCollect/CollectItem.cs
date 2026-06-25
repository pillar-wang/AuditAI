using System;
using System.Reflection;
using Auditai.Model;
using Auditai.UI.Controls.CollectCell;
using Newtonsoft.Json;

namespace Auditai.UI.Controls.CellCollect;

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

	public Auditai.UI.Controls.CollectCell.Operand GetOperand(Ledger ledger, int auditYear)
	{
		return new Auditai.UI.Controls.CollectCell.Operand(Operation, GetValue(ledger, auditYear));
	}
}
