using System.Collections.Generic;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

internal class VoucherPage
{
	internal int PageNo { get; set; }

	internal int IndexOfVoucher { get; set; }

	internal int PageCountOfVoucher { get; set; }

	internal List<Voucher> Vouchers { get; set; }

	internal VoucherPage()
	{
		Vouchers = new List<Voucher>();
	}
}
