using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandRemoveNodes : AppCommandButton
{
	public override string Text => "批量删除文件";

	public override Image LargeIcon => Resources.RemoveNodes;

	protected override void Clicked()
	{
		Program.MainForm.RemoveNodes();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		Enabled = !SoftwareLicenseManager.IsAddFileOutOfLicenseLimit();
		if (!SoftwareLicenseManager.IsAllowShowAddOrDeleteFileButton())
		{
			Visible = false;
		}
	}
}
