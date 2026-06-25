using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowFoot : AppCommandToggleButton
{
	public override string Text => "表底签名";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.TableFoot;

	protected override string Tooltip => TipResource.显示设置菜单_表底尾注;

	protected override void Pressed()
	{
		Program.MainForm.TableEditor.ShowFootPane();
	}

	protected override void Unpressed()
	{
		Program.MainForm.TableEditor.HideFootPane();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Visible = state.ViewKind == MainFormView.Table;
	}
}
