using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTitleUnifyRowHeight : AppCommandButton
{
	public override string Text => "平均分布行高";

	public override Image LargeIcon => Resources.UnifyRowHeight;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.UnifyRowHeight();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.UnifyRowHeight();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
