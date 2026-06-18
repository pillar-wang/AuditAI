using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupValidateDocument : AppCommandGroup
{
	public override string Text => "文档校验";

	public override Image Image => Resources.ValidateDocument;

	public AppGroupValidateDocument()
	{
		base.Commands.Add(AppCommands.ValidateDocument);
		base.Commands.Add(AppCommands.DocPreviousError);
		base.Commands.Add(AppCommands.DocNextError);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind == MainFormView.Document;
	}
}
