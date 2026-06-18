using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
