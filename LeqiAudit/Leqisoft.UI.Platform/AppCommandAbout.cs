using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandAbout : AppCommandButton
{
	public override string Text => "\u3000关于\u3000";

	public override Image LargeIcon => Leqisoft.UI.Platform.Properties.Resources.About;

	protected override string Tooltip => TipResource.关于;

	protected override void Clicked()
	{
		Program.MainForm.AboutForm();
	}
}
