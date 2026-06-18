using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupApplyRange : AppCommandGroup
{
	public override string Text => "应用范围";

	public override Image Image => Resources.DocPrintSettingSelection;

	public AppGroupApplyRange()
	{
		base.Commands.Add(AppCommands.ApplySelection);
		base.Commands.Add(AppCommands.ApplyDocument);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
