using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTitleIncreaseColumnWidth : AppCommandButton
{
	public override string Text => "增加列宽";

	public override Image LargeIcon => Resources.IncreaseColumnWidth;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.IncreaseColumnWidth();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.IncreaseColumnWidth();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
