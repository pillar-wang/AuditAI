using System;
using System.Drawing;
using System.Threading.Tasks;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandSaveProject : AppCommandButton
{
	public override Image SmallIcon => Leqisoft.UI.Platform.Properties.Resources.SaveProject;

	protected override Func<Task> ClickedTask => () => Program.MainForm.SaveProjects();

	protected override string Tooltip => TipResource.保存Project按钮;
}
