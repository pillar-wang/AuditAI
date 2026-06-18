using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandShowSidebar : AppCommandButton
{
	public override Image SmallIcon => Resources.HideSideToolbar16;

	protected override void Clicked()
	{
		Program.MainForm.ToggleSideToolbar();
	}
}
