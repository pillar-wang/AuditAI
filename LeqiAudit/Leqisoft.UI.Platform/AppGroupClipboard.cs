using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupClipboard : AppCommandGroup
{
	public override string Text => "剪贴板";

	public override Image Image => Resources.Copy;

	public AppGroupClipboard()
	{
		base.Commands.Add(AppCommands.Copy);
		base.Commands.Add(AppCommands.Cut);
		base.Commands.Add(AppCommands.Paste);
		base.Commands.Add(AppCommands.FormatBrush);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.FormatBrush || state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.EditingNote || state.ViewKind == MainFormView.DocFormatBrush;
	}
}
