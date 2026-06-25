using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandDocPreviousError : AppCommandButton
{
	public override string Text => "上一个错误";

	public override Image LargeIcon => Resources.PreviousError;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.PreviousError();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}
