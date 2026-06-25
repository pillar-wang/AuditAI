using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandMoveRightColumn : AppCommandButton
{
	public override string Text => "右移列";

	public override Image LargeIcon => Resources.ColumnRight;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.MoveRightColumns();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
