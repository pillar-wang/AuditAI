using System;
using System.Drawing;
using System.Threading.Tasks;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandSaveProject : AppCommandButton
{
	public override Image SmallIcon => Auditai.UI.Platform.Properties.Resources.SaveProject;

	protected override Func<Task> ClickedTask => () => Program.MainForm.SaveProjects();

	protected override string Tooltip => TipResource.保存Project按钮;
}
