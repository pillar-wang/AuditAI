using System.Drawing;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandToggleButton : AppCommandBase
{
	public RibbonToggleButton ToggleButton { get; private set; }

	public virtual string Text { get; }

	public virtual Image LargeIcon { get; }

	public virtual Image SmallIcon { get; }

	public sealed override RibbonItem RibbonItem => ToggleButton;

	public bool IsPressed
	{
		get
		{
			return ToggleButton.Pressed;
		}
		set
		{
			ToggleButton.Pressed = value;
		}
	}

	public AppCommandToggleButton()
	{
	}

	protected abstract void Pressed();

	protected abstract void Unpressed();

	public override void GenerateRibbonItem()
	{
		if (!(ToggleButton == null))
		{
			return;
		}
		ToggleButton = new RibbonToggleButton
		{
			Text = Text,
			LargeImage = LargeIcon,
			SmallImage = SmallIcon,
			TextImageRelation = ((SmallIcon == null) ? TextImageRelation.ImageAboveText : TextImageRelation.ImageBeforeText)
		};
		ToggleButton.Click += delegate
		{
			if (IsPressed)
			{
				Pressed();
			}
			else
			{
				Unpressed();
			}
		};
		AttachTooltip();
	}
}
