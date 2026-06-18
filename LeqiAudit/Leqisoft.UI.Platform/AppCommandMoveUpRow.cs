using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandMoveUpRow : AppCommandButton
{
	public override string Text => "上移行";

	public override Image LargeIcon => Resources.RowUp;

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.MoveUpRows();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
