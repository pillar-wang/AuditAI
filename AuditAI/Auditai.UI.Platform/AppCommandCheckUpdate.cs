using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandCheckUpdate : AppCommandButton
{
	public override string Text => "检查更新";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.CheckUpdate;

	protected override Func<Task> ClickedTask => async delegate
	{
		await Program.MainForm.CheckUpdate();
	};

	protected override string Tooltip => TipResource.检查更新;
}
