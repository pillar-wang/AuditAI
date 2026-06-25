using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandSyncProject : AppCommandButton
{
	public override string Text => "同步数据";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.SyncProject;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.SyncProjects();
	};

	protected override string Tooltip => TipResource.Ribbon菜单_Project管理菜单_同步Project按钮;

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
	}
}
