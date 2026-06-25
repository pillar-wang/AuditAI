using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTitleUnifyColumnWidth : AppCommandButton
{
	public override string Text => "平均分布列宽";

	public override Image LargeIcon => Resources.UnifyColumnWidth;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.UnifyColumnWidth();
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.UnifyColumnWidth();
			break;
		}
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.EditingTitle || state.ViewKind == MainFormView.EditingFoot;
	}
}
