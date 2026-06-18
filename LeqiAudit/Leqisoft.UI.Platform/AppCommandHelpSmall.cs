using System.Drawing;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandHelpSmall : AppCommandButton
{
	public override Image SmallIcon => Leqisoft.UI.Platform.Properties.Resources.toolHelp16;

	protected override string Tooltip => TipResource.帮助;

	protected override void Clicked()
	{
		Program.MainForm.ShowHelpCenter();
	}
}
