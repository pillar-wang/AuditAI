﻿﻿﻿using System.Reflection;
using Leqisoft.UI.Controls.CollectDic;

namespace Leqisoft.UI.Controls;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class LedgerValidator
{
	public int Version { get; set; }

	public ETable AccountPositive { get; set; }

	public ETable AccountReverse { get; set; }

	public ETable BalanceValidate { get; set; }

	public ETable VoucherPositive { get; set; }

	public ETable VoucherReverse { get; set; }

	public LedgerValidator()
	{
		AccountPositive = new ETable();
		AccountReverse = new ETable();
		BalanceValidate = new ETable();
		VoucherPositive = new ETable();
		VoucherReverse = new ETable();
	}
}
