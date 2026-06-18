using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupDocumentLock : AppCommandGroup
{
	public override string Text => "文档锁定";

	public override Image Image => Resources.ToggleDocLock;

	public AppGroupDocumentLock()
	{
		base.Commands.Add(AppCommands.DocumentLock);
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		base.Visible = state.ViewKind != MainFormView.EditingNote && state.ViewKind != MainFormView.DocFormatBrush;
	}
}
