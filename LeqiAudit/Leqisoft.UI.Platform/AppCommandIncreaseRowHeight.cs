using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandIncreaseRowHeight : AppCommandButton
{
	public override string Text => "增加行高";

	public override Image LargeIcon => Resources.IncreaseRowHeight;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.RibbonIncreaseRowHeightClicked();
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonIncreaseRowHeightClicked();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Document;
	}
}
