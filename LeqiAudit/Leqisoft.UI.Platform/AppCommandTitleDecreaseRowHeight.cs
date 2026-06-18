using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTitleDecreaseRowHeight : AppCommandButton
{
	public override string Text => "减少行高";

	public override Image LargeIcon => Resources.DecreaseRowHeight;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.DecreaseRowHeight();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.DecreaseRowHeight();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
