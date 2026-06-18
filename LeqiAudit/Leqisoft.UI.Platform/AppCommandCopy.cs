using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandCopy : AppCommandButton
{
	public override Image LargeIcon => Resources.Copy;

	public override string Text => "复制";

	protected override void Clicked()
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.Copy();
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind != MainFormView.DocFormatBrush;
	}
}
