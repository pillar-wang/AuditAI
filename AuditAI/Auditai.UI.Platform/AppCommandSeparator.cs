using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

public class AppCommandSeparator : AppCommandBase
{
	public RibbonSeparator Separator { get; private set; }

	public sealed override RibbonItem RibbonItem => Separator;

	public override void GenerateRibbonItem()
	{
		if (Separator == null)
		{
			Separator = new RibbonSeparator();
		}
	}
}
