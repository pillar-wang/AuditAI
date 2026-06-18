using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandDocNextError : AppCommandButton
{
	public override string Text => "下一个错误";

	public override Image LargeIcon => Resources.NextError;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.NextError();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}
