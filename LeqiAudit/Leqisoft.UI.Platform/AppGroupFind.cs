using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupFind : AppCommandGroup
{
	public override string Text => "查找替换";

	public override Image Image => Resources.Find;

	public AppGroupFind()
	{
		base.Commands.Add(AppCommands.Find);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Table;
	}
}
