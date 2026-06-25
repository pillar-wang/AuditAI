using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupPrintScale : AppCommandGroup
{
	public override string Text => "打印缩放";

	public override Image Image => Resources.PrintZoom;

	public AppGroupPrintScale()
	{
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[2]
		{
			AppCommands.ScalePageWidth,
			AppCommands.WidthScale
		}));
		base.Commands.Add(new AppCommandToolbar(new AppCommandBase[2]
		{
			AppCommands.ScalePageHeight,
			AppCommands.HeightScale
		}));
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.TablePreview || state.ViewKind == MainFormView.TicketInput || state.ViewKind == MainFormView.TicketPrint;
		bool enabled = !AppCommands.ScalePageWidth.IsChecked && !AppCommands.ScalePageHeight.IsChecked;
		AppCommands.WidthScale.Enabled = enabled;
		AppCommands.HeightScale.Enabled = enabled;
	}
}
