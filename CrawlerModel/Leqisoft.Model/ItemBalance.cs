using System.Collections.Generic;

namespace Leqisoft.Model;

public class ItemBalance
{
	public decimal Balance { get; set; }

	public double Quantity { get; set; }

	public double UnitPrice { get; set; }

	public Dictionary<Currency, ForeignRecord> Foreign { get; set; } = new Dictionary<Currency, ForeignRecord>();

}
