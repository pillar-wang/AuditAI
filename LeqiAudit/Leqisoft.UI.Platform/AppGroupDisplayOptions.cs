using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupDisplayOptions : AppCommandGroup
{
	public override string Text => "显示选项";

	public override Image Image => Resources.IndexNumber;

	public AppGroupDisplayOptions()
	{
		base.Commands.Add(AppCommands.PageView);
		base.Commands.Add(AppCommands.DraftView);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.ShowParagraphMarkers);
		base.Commands.Add(AppCommands.ShowHorizontalRuler);
		base.Commands.Add(AppCommands.ShowVerticalRuler);
		base.Commands.Add(AppCommands.ShowDocumentNavigator);
		base.Commands.Add(new AppCommandSeparator());
		base.Commands.Add(AppCommands.Zoom);
		base.Commands.Add(AppCommands.ShowNodeNumber);
		base.Commands.Add(AppCommands.ShowFormula);
		base.Commands.Add(AppCommands.ShowValidation);
		base.Commands.Add(AppCommands.ShowTooltip);
		base.Commands.Add(AppCommands.ToggleFullscreen);
	}
}
