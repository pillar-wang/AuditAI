namespace Leqisoft.Model;

public class ReceivableAgeValue
{
	public decimal[] Values { get; set; }

	public decimal End { get; set; }

	public bool IsDebit { get; set; }

	public decimal Opposite { get; set; }
}
