using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowFoot : AppCommandToggleButton
{
	public override string Text => "表底签名";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.TableFoot;

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
