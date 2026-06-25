using System.Collections.Generic;

namespace Auditai.Model;

public class ReceivableAgeEntry
{
	public ReceivableAgeValue Value { get; } = new ReceivableAgeValue();


	public Dictionary<AuxiliaryItem, ReceivableAgeValue> Aux { get; } = new Dictionary<AuxiliaryItem, ReceivableAgeValue>();

}
