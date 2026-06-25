using System.Drawing;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

public abstract class AppCommandColorPicker : AppCommandBase
{
	private bool _inClick;

	public RibbonColorPicker ColorPicker { get; private set; }

	public virtual Image Icon { get; }

	public sealed override RibbonItem RibbonItem => ColorPicker;

	public AppCommandColorPicker()
	{
	}

	protected abstract void Clicked(Color color);

	public override void GenerateRibbonItem()
	{
		if (!(ColorPicker == null))
		{
			return;
		}
		ColorPicker = new RibbonColorPicker
		{
			SmallImage = Icon
		};
		ColorPicker.Click += delegate
		{
			if (!_inClick)
			{
				_inClick = true;
				Clicked(ColorPicker.Color);
				_inClick = false;
			}
		};
		ColorPicker.SelectedColorChanged += delegate
		{
			Clicked(ColorPicker.Color);
		};
		AttachTooltip();
	}
}
