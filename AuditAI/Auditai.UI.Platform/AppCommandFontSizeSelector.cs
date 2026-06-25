using C1.Win.C1Ribbon;
using Auditai.UI.Controls;

namespace Auditai.UI.Platform;

public abstract class AppCommandFontSizeSelector : AppCommandBase
{
	public FontSizeComboHost FontSizeSelector { get; private set; }

	public sealed override RibbonItem RibbonItem => FontSizeSelector;

	public AppCommandFontSizeSelector()
	{
	}

	protected abstract void FontSizeSelected(FontSizeEventArgs e);

	public override void GenerateRibbonItem()
	{
		if (FontSizeSelector == null)
		{
			FontSizeSelector = new FontSizeComboHost();
			FontSizeSelector.FontSizeSelected += delegate(object s, FontSizeEventArgs e)
			{
				FontSizeSelected(e);
			};
			AttachTooltip();
		}
	}
}
