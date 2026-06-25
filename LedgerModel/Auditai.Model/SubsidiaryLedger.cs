using System;
using System.Collections.Generic;

namespace Auditai.Model;

public class SubsidiaryLedger
{
	public Account Account { get; internal set; }

	public DateTime Start { get; internal set; }

	public DateTime End { get; internal set; }

	public decimal BeginBalance { get; internal set; }

	public List<MonthSubsidiaryLedger> Months { get; }

	internal SubsidiaryLedger()
	{
		Months = new List<MonthSubsidiaryLedger>();
	}
}
