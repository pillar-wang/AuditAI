using System;
using System.Collections.Generic;
using System.Linq;

namespace Leqisoft.Model;

public class AccountBalance : ICloneable
{
	public decimal Total { get; set; }

	public Dictionary<AuxiliaryClass, ClassBalance> ClassBalances { get; private set; } = new Dictionary<AuxiliaryClass, ClassBalance>();


	public override string ToString()
	{
		return $"{Total} ({ClassBalances.Count})";
	}

	public object Clone()
	{
		AccountBalance accountBalance = (AccountBalance)MemberwiseClone();
		accountBalance.ClassBalances = ClassBalances.ToDictionary((KeyValuePair<AuxiliaryClass, ClassBalance> pair) => pair.Key, (KeyValuePair<AuxiliaryClass, ClassBalance> pair) => (ClassBalance)pair.Value.Clone());
		return accountBalance;
	}

	public AccountBalance OnlyCloneKey()
	{
		AccountBalance accountBalance = (AccountBalance)MemberwiseClone();
		accountBalance.Total = 0m;
		accountBalance.ClassBalances = ClassBalances.ToDictionary((KeyValuePair<AuxiliaryClass, ClassBalance> pair) => pair.Key, (KeyValuePair<AuxiliaryClass, ClassBalance> pair) => pair.Value.OnlyCloneKey());
		return accountBalance;
	}
}
