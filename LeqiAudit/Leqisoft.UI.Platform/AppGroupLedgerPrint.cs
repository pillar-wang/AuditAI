using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupLedgerPrint : AppCommandGroup
{
	public override string Text => "打印输出";

	public override Image Image => Resources.PrintPreview;

	public AppGroupLedgerPrint()
	{
		base.Commands.Add(AppCommands.LedgerPortrait);
		base.Commands.Add(AppCommands.LedgerLandscape);
		base.Commands.Add(AppCommands.LedgerPrintPreview);
		base.Commands.Add(AppCommands.LedgerPrint);
		base.Commands.Add(AppCommands.Export);
	}
}
