using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandRecycleNode : AppCommandButton
{
	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.RecycleNode;

	public override string Text => "回收文件";

	protected override string Tooltip => TipResource.回收文件功能说明;

	protected override void Clicked()
	{
		Program.MainForm.ProjectHierarchy.RecycleNode();
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
