using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTitleDecreaseColumnWidth : AppCommandButton
{
	public override string Text => "减少列宽";

	public override Image LargeIcon => Resources.DecreaseColumnWidth;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.DecreaseColumnWidth();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.DecreaseColumnWidth();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
