using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandShowSidebar : AppCommandButton
{
	public override Image SmallIcon => IconRes.HideSideToolbar16;

	protected override void Clicked()
	{
		Program.MainForm.ToggleSideToolbar();
	}
}
