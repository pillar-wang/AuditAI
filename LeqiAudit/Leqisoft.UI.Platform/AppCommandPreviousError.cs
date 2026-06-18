using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandPreviousError : AppCommandButton
{
	public override string Text => "上一个错误";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.PreviousError;

	protected override string Tooltip => TipResource.上一个错误;

	protected override void Clicked()
	{
		Program.MainForm.PreviousError();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
		RibbonItem.Group.Items[RibbonItem.Group.Items.IndexOf(RibbonItem) - 1].Visible = Visible;
	}
}
