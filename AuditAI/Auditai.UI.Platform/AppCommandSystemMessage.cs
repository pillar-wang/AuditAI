using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
