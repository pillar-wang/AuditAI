using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandPreviousError : AppCommandButton
{
	public override string Text => "上一个错误";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.PreviousError;

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
