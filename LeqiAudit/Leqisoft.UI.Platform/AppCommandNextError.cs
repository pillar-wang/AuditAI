using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandNextError : AppCommandButton
{
	public override string Text => "下一个错误";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.NextError;

	protected override string Tooltip => TipResource.下一个错误;

	protected override void Clicked()
	{
		Program.MainForm.NextError();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
