using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandToggleFullscreenSmall : AppCommandToggleButton
{
	public override Image SmallIcon => Leqisoft.UI.Platform.Properties.Resources.Fullscreen_S;

	protected override string Tooltip => TipResource.全屏按钮;

	protected override void Pressed()
	{
		Program.MainForm.Fullscreen();
	}

	protected override void Unpressed()
	{
		Program.MainForm.QuitFullscreen();
	}
}
