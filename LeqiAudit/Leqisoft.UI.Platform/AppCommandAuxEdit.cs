using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAuxEdit : AppCommandButton
{
	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ComboList;

	public override string Text => "下拉列表";

	protected override string Tooltip => TipResource.辅助编辑;

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		if (!SoftwareLicenseManager.IsAllowModifyTableStruct())
		{
			Visible = false;
		}
		else
		{
			Visible = true;
		}
	}

	protected override void Clicked()
	{
		Program.MainForm.TableEditor.SetComboListDialog();
	}
}
