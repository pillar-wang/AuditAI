using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandQuit : AppCommandButton
{
	public override string Text => "退出系统";

	public override Image LargeIcon => Resources.Quit;

	protected override void Clicked()
	{
		Program.MainForm.View.Close();
	}
}
