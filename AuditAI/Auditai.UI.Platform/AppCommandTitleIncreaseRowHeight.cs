using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTitleIncreaseRowHeight : AppCommandButton
{
	public override string Text => "增加行高";

	public override Image LargeIcon => Resources.IncreaseRowHeight;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.IncreaseRowHeight();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.IncreaseRowHeight();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
