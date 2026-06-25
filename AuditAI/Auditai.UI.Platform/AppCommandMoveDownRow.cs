using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMoveDownRow : AppCommandButton
{
	public override string Text => "下移行";

	public override Image LargeIcon => Resources.RowDown;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.MoveDownRows();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
