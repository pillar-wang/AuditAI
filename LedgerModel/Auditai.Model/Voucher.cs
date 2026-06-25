using System;
using System.Collections.Generic;
using System.Linq;

namespace Auditai.Model;

public class Voucher
{
	public const int VOUCHER_DIRTY_ADD = 1;

	public const int VOUCHER_DIRTY_MODIFY = 2;

	public const int VOUCHER_DIRTY_DELETE = -1;

	public int Dirty { get; set; }

	public int Id { get; set; }

	public VoucherType Type { get; set; }

	public string Number { get; set; }

	public DateTime Day { get; set; }

	public string Digest { get; set; }

	public Account Account { get; set; }

	public bool IsDebit { get; set; }

	public bool DirectionToggled { get; set; }

	public bool VoucherMark { get; set; }

	public decimal Amount { get; set; }

	public Currency Currency { get; set; }

	public decimal ForeignAmount { get; set; }

	public double ExchangeRate { get; set; }

	public double Quantity { get; set; }

	public decimal UnitPrice { get; set; }

	public string Checker { get; set; }

	public string Maker { get; set; }

	public string Booker { get; set; }

	public int NumAttachments { get; set; }

	public List<AuxiliaryItem> Details { get; }

	public List<Account> OppositeAccounts { get; } = new List<Account>();


	public Voucher()
	{
		Details = new List<AuxiliaryItem>();
	}

	public override string ToString()
	{
		return string.Format("{0} {1} {2} {3} {4}{5}", Type, Number, Day, Account.Name, IsDebit ? "借" : "贷", Amount);
	}

	public void ToggleDirection()
	{
		Dirty = 2;
		IsDebit = !IsDebit;
		DirectionToggled = !DirectionToggled;
		Amount = -Amount;
	}

	public void ToggleMark()
	{
		Dirty = 2;
		VoucherMark = !VoucherMark;
	}

	public string GetDisplayAccountCodeWithDetail()
	{
		if (Details.Count != 0)
		{
			return string.Join("|", Details.Select((AuxiliaryItem d) => Account.Code + "-" + d.Code));
		}
		return Account.Code;
	}

	public string GetDisplayAccountNameWithDetail()
	{
		if (Details.Count != 0)
		{
			return string.Join("|", Details.Select((AuxiliaryItem d) => Account.GetFullName() + "-" + d.Name));
		}
		return Account.GetFullName();
	}

	public string GetDisplayAuxiliaryClassName()
	{
		if (Details.Count != 0)
		{
			return string.Join("|", Details.Select((AuxiliaryItem d) => d.Class.Name));
		}
		return string.Empty;
	}

	public string GetDisplayAuxiliaryCode()
	{
		if (Details.Count != 0)
		{
			return string.Join("|", Details.Select((AuxiliaryItem d) => d.Code));
		}
		return string.Empty;
	}

	public string GetDisplayAuxiliaryName()
	{
		if (Details.Count != 0)
		{
			return string.Join("|", Details.Select((AuxiliaryItem d) => d.Name));
		}
		return string.Empty;
	}
}
