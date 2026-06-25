using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandBack : AppCommandButton
{
	public override Image SmallIcon => Resources.back;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		Enabled = false;
	}

	protected override void Clicked()
	{
		Program.MainForm.Back();
	}
}
