using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandQuit : AppCommandButton
{
	public override string Text => "退出系统";

	public override Image LargeIcon => Resources.Quit;

	protected override void Clicked()
	{
		Program.MainForm.View.Close();
	}
}
