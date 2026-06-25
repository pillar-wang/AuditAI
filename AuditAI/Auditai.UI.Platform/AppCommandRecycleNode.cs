using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandRecycleNode : AppCommandButton
{
	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.RecycleNode;

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
