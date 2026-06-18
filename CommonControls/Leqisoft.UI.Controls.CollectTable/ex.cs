using System.Linq;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls.CollectTable;

internal static class ex
{
	internal static object GetSummaryValue(this SubsidiaryLedger subsidiaryLedger, string caption, AnalysisProject analysis)
	{
		object result = null;
		switch (caption)
		{
		case "项目":
			result = subsidiaryLedger.Account.Name;
			break;
		case "合计":
			result = subsidiaryLedger.Months.Sum((MonthSubsidiaryLedger msl) => getValue(msl));
			break;
		case "1月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 1));
			break;
		case "2月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 2));
			break;
		case "3月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 3));
			break;
		case "4月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 4));
			break;
		case "5月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 5));
			break;
		case "6月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 6));
			break;
		case "7月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 7));
			break;
		case "8月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 8));
			break;
		case "9月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 9));
			break;
		case "10月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 10));
			break;
		case "11月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 11));
			break;
		case "12月":
			result = getValue(subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == 12));
			break;
		}
		return result;
		decimal getValue(MonthSubsidiaryLedger msl)
		{
			if (msl == null)
			{
				return 0m;
			}
			SubsidiaryLedgerTotal total = msl.Total;
			if (analysis != AnalysisProject.Balance)
			{
				if (analysis != AnalysisProject.Credits)
				{
					return total.Debit;
				}
				return total.Credit;
			}
			return total.Balance;
		}
	}
}
