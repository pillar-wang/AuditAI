namespace Leqisoft.Model;

public class SubsidiaryLedgerEntry
{
	public Voucher Voucher { get; internal set; }

	public decimal Balance { get; internal set; }

	public override string ToString()
	{
		return $"{Voucher} 余额{Balance}";
	}

	public SubsidiaryLedgerEntry()
	{
	}

	public SubsidiaryLedgerEntry(Voucher voucher, decimal balance)
	{
		Voucher = voucher;
		Balance = balance;
	}
}
