using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupReference : AppCommandGroup
{
	public override string Text => "数据引用";

	public override Image Image => Resources.Intelliref;

	public AppGroupReference()
	{
		base.Commands.Add(AppCommands.RefreshRefTable);
		base.Commands.Add(AppCommands.RefreshDocument);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document;
	}
}
