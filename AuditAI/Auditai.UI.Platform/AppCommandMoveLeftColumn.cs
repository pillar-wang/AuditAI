using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMoveLeftColumn : AppCommandButton
{
	public override string Text => "左移列";

	public override Image LargeIcon => Resources.ColumnLeft;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.MoveLeftColumns();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
