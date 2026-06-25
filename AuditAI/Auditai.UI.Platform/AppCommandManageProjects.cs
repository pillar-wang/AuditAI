using System.Drawing;
using Auditai.Model;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandManageProjects : AppCommandButton
{
	public override string Text => StringConstBase.Current.Project + "管理";

	public override System.Drawing.Image LargeIcon => Auditai.UI.Platform.Properties.Resources.ProjectManager;

	protected override string Tooltip => TipResource.Project管理按钮;

	protected override void Clicked()
	{
		Program.MainForm.ManageProjects();
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
	}
}
