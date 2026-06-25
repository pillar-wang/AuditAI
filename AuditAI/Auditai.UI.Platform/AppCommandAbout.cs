using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandAbout : AppCommandButton
{
	public override string Text => "\u3000关于\u3000";

	public override Image LargeIcon => Auditai.UI.Platform.Properties.Resources.About;

	protected override string Tooltip => TipResource.关于;

	protected override void Clicked()
	{
		Program.MainForm.AboutForm();
	}
}
