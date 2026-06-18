using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.CollectDic;

namespace Leqisoft.UI.LedgerView;

public class VoucherValidate : AbstractValidate<Voucher>
{
	protected const string CN_VP_INDEX = "voucherPositiveIndex";

	protected const string CN_VP_NAME = "voucherPositiveName";

	protected const string CN_VP_DIRECTION = "voucherPositiveDirection";

	protected const string CN_VP_OPPOSITE = "voucherPositiveOpposite";

	protected const string CN_VP_TIP = "voucherPositiveTip";

	protected const string CN_VR_INDEX = "voucherReverseIndex";

	protected const string CN_VR_NAME = "voucherReverseName";

	protected const string CN_VR_DIRECTION = "voucherReverseDirection";

	protected const string CN_VR_OPPOSITE = "voucherReverseOpposite";

	protected const string CN_VR_TIP = "voucherReverseTip";

	public VoucherValidate(IEnumerable<Voucher> accounts)
	{
		_validateItems = accounts;
	}

	protected override bool TryValidate(Voucher voucher, out ValidateResult result)
	{
		result = null;
		string dc = (voucher.Account.IsDebit ? "借" : "贷");
		ERow eRow = DictionarySync.LedgerValidator.VoucherPositive.FindRow((ERow row) => row["voucherPositiveName"].Value == voucher.Account.Name && row["voucherPositiveDirection"].Value == dc && voucher.OppositeAccounts.Any((Account op) => op.Name == row["voucherPositiveOpposite"].Value));
		if (eRow != null)
		{
			result = new ValidateResult
			{
				Key = voucher,
				Tip = eRow["voucherPositiveTip"].Value
			};
			return true;
		}
		eRow = DictionarySync.LedgerValidator.VoucherReverse.FindRow((ERow row) => row["voucherReverseName"].Value == voucher.Account.Name && row["voucherReverseDirection"].Value == dc && voucher.OppositeAccounts.All((Account a) => !row["voucherReverseOpposite"].Values.Any((string op) => Regex.IsMatch(a.Name, op))));
		if (eRow != null)
		{
			result = new ValidateResult
			{
				Key = voucher,
				Tip = eRow["voucherReverseTip"].Value
			};
			return true;
		}
		return false;
	}
}
