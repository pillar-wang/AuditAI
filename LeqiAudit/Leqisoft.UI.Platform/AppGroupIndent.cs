using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupIndent : AppCommandGroup
{
	public override string Text => "缩进";

	public override Image Image => Resources.UnindentCell;

	public AppGroupIndent()
	{
		base.Commands.Add(AppCommands.Unindent);
		base.Commands.Add(AppCommands.Indent);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table || state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
