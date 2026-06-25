using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupFormatBrush : AppCommandGroup
{
	public override string Text => "格式和样式刷";

	public override Image Image => Resources.FormatPainter;

	public AppGroupFormatBrush()
	{
		base.Commands.Add(AppCommands.FormatBrush);
		base.Commands.Add(AppCommands.TableStyleBrush);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.FormatBrush;
	}
}
