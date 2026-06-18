using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandSystemMessage : AppCommandButton
{
	public override Image SmallIcon => Resources.SystemMessage;

	protected override void Clicked()
	{
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		Visible = false;
	}
}
