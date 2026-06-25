using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandToggleFullscreenSmall : AppCommandToggleButton
{
	public override Image SmallIcon => Auditai.UI.Platform.Properties.Resources.Fullscreen_S;

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
