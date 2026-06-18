using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Platform;

public class AppCommandLabel : AppCommandBase
{
	public RibbonLabel Label { get; private set; }

	public string Text { get; }

	public sealed override RibbonItem RibbonItem => Label;

	public AppCommandLabel(string text)
	{
		Text = text;
	}

	public override void GenerateRibbonItem()
	{
		if (Label == null)
		{
			Label = new RibbonLabel(Text);
		}
	}
}
