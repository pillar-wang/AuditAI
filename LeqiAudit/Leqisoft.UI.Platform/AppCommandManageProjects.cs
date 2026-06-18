using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandManageProjects : AppCommandButton
{
	public override string Text => StringConstBase.Current.Project + "管理";

	public override System.Drawing.Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.ProjectManager;

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
