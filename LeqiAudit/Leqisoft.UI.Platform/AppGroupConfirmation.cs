using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupConfirmation : AppCommandGroup
{
	public override string Text => "函证设置";

	public override Image Image => Resources.ConfirmationGenerate;

	public AppGroupConfirmation()
	{
		base.Commands.Add(AppCommands.ConfirmationSetting);
		base.Commands.Add(AppCommands.GenerateConfirmation);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document;
	}
}
