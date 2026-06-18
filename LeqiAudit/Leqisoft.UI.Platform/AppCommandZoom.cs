using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandZoom : AppCommandMenu
{
	public override string Text => "缩放比例";

	public override Image LargeImage => Resources.Scale;

	public AppCommandZoom()
		: base(new AppCommandBase[11]
		{
			AppCommands.Zoom25,
			AppCommands.Zoom50,
			AppCommands.Zoom75,
			AppCommands.Zoom100,
			AppCommands.Zoom125,
			AppCommands.Zoom200,
			AppCommands.Zoom400,
			new AppCommandSeparator(),
			AppCommands.ZoomWholePage,
			AppCommands.ZoomPageWidth,
			AppCommands.ZoomTextWidth
		})
	{
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview || state.ViewKind == MainFormView.Image || state.ViewKind == MainFormView.ImagePreview || state.ViewKind == MainFormView.Pdf || state.ViewKind == MainFormView.PdfPreview;
		RibbonItem.Group.Items[RibbonItem.Group.Items.IndexOf(RibbonItem) - 1].Visible = Visible;
	}
}
