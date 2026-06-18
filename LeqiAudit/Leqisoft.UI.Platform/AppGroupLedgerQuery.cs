using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupLedgerQuery : AppCommandGroup
{
	public override string Text => "账务查询";

	public override Image Image => Resources.AccountBalance;

	public AppGroupLedgerQuery()
	{
		base.Commands.Add(AppCommands.BalanceSheet);
		base.Commands.Add(AppCommands.MonthSummary);
		base.Commands.Add(AppCommands.GeneralLedger);
		base.Commands.Add(AppCommands.SubsidiaryLedger);
		base.Commands.Add(AppCommands.Vouchers);
		base.Commands.Add(AppCommands.MyFavorites);
	}
}
