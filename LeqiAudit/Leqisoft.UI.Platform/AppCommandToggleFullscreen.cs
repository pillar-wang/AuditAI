using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandToggleFullscreen : AppCommandToggleButton
{
	public override string Text => "全屏显示";

	public override Image LargeIcon => Resources.Fullscreen;

	protected override void Pressed()
	{
		Program.MainForm.Fullscreen();
	}

	protected override void Unpressed()
	{
		Program.MainForm.QuitFullscreen();
	}
}
