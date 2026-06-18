using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupPrintPreview : AppCommandGroup
{
	public override string Text => "打印";

	public override Image Image => Resources.PrintPreview;

	public AppGroupPrintPreview()
	{
		base.Commands.Add(AppCommands.TablePrintPreview);
		base.Commands.Add(AppCommands.TablePrint);
		base.Commands.Add(AppCommands.FileBatchPrint);
	}
}
