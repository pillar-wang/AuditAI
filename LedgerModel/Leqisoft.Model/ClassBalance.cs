using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class ClassBalance : ICloneable
{
	public decimal Total { get; set; }

	public Dictionary<AuxiliaryItem, decimal> ItemBalances { get; private set; } = new Dictionary<AuxiliaryItem, decimal>();


	public object Clone()
	{
		ClassBalance classBalance = (ClassBalance)MemberwiseClone();
		classBalance.ItemBalances = new Dictionary<AuxiliaryItem, decimal>(ItemBalances);
		return classBalance;
	}

	public ClassBalance OnlyCloneKey()
	{
		ClassBalance classBalance = (ClassBalance)MemberwiseClone();
		classBalance.Total = 0m;
		classBalance.ItemBalances = ItemBalances.ToDictionary((KeyValuePair<AuxiliaryItem, decimal> pair) => pair.Key, (KeyValuePair<AuxiliaryItem, decimal> pair) => 0m);
		return classBalance;
	}

	public override string ToString()
	{
		return $"{Total} ({ItemBalances.Count})";
	}
}
