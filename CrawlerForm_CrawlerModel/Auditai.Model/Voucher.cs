using System;
using System.Collections.Generic;

namespace Auditai.Model;

public class Voucher
{
	public long Id { get; set; }

	public VoucherType Type { get; set; }

	public string Number { get; set; }

	public DateTime Day { get; set; }

	public string Digest { get; set; }

	public Account Account { get; set; }

	public decimal Amount { get; set; }

	public Currency Currency { get; set; }

	public ForeignRecord Foreign { get; set; }

	public bool IsDebit { get; set; }

	public List<Item> Details { get; set; } = new List<Item>();


	public double Quantity { get; set; }

	public double UnitPrice { get; set; }

	public string Checker { get; set; }

	public string Maker { get; set; }

	public string Booker { get; set; }

	public int NumAttachments { get; set; }

	public override string ToString()
	{
		return $"{Day} {Type}-{Number} {Digest}";
	}

	public override bool Equals(object obj)
	{
		return obj is Voucher && Id == ((Voucher)obj).Id;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}
