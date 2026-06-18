using C1.Win.C1Ribbon;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandFontSelector : AppCommandBase
{
	public FontComboBoxHost FontSelector { get; private set; }

	public sealed override RibbonItem RibbonItem => FontSelector;

	public AppCommandFontSelector()
	{
	}

	protected abstract void FontSelected(FontFamilyEventArgs e);

	public override void GenerateRibbonItem()
	{
		if (FontSelector == null)
		{
			FontSelector = new FontComboBoxHost();
			FontSelector.FontFamilySelected += delegate(object s, FontFamilyEventArgs e)
			{
				FontSelected(e);
			};
			AttachTooltip();
		}
	}
}
