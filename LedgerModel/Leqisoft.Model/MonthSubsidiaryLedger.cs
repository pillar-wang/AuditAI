using System.Collections.Generic;

namespace Leqisoft.Model;

public class MonthSubsidiaryLedger
{
	public int Year { get; }

	public int Month { get; }

	public List<SubsidiaryLedgerEntry> Entries { get; }

	public SubsidiaryLedgerTotal Total { get; }

	public SubsidiaryLedgerTotal GrandTotal { get; }

	internal MonthSubsidiaryLedger(int year, int month)
	{
		Year = year;
		Month = month;
		Entries = new List<SubsidiaryLedgerEntry>();
		Total = new SubsidiaryLedgerTotal();
		GrandTotal = new SubsidiaryLedgerTotal();
	}

	public override string ToString()
	{
		return $"{Year}年{Month}月；{Entries.Count}笔凭证";
	}
}
