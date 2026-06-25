using System.Drawing;
using Auditai.UI.Controls.Properties;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandHelpSmall : AppCommandButton
{
	public override Image SmallIcon => Auditai.UI.Platform.Properties.Resources.toolHelp16;

	protected override string Tooltip => TipResource.帮助;

	protected override void Clicked()
	{
		Program.MainForm.ShowHelpCenter();
	}
}
