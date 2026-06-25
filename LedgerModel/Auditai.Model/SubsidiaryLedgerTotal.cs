namespace Auditai.Model;

public class SubsidiaryLedgerTotal
{
	public decimal Debit { get; internal set; }

	public decimal Credit { get; internal set; }

	public decimal Balance { get; internal set; }

	internal SubsidiaryLedgerTotal()
	{
	}

	public override string ToString()
	{
		return $"借方发生额 {Debit}; 贷方发生额 {Credit}; 余额 {Balance}";
	}
}
