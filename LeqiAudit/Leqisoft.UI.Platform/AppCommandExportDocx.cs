using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandExportDocx : AppCommandButton
{
	public override string Text => "Word文件";

	public override Image LargeIcon => Resources.ExportDocx;

	protected override void Clicked()
	{
		Program.MainForm.CurrentDocumentEditor.ExportDocumentDialog();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document || state.ViewKind == MainFormView.DocumentPreview;
	}
}
