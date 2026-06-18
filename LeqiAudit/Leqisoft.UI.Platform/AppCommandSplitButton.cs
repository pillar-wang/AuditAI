using System.Collections.Generic;
using System.Drawing;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandSplitButton : AppCommandItems
{
	public RibbonSplitButton SplitButton { get; private set; }

	public virtual string Text { get; }

	public virtual Image LargeIcon { get; }

	public virtual Image SmallIcon { get; }

	public sealed override RibbonItem RibbonItem => SplitButton;

	public AppCommandSplitButton(IEnumerable<AppCommandBase> items)
		: base(items)
	{
	}

	protected virtual void Clicked()
	{
	}

	protected virtual void Pressed()
	{
	}

	protected virtual void Unpressed()
	{
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		if (!(SplitButton == null))
		{
			return;
		}
		SplitButton = new RibbonSplitButton
		{
			Text = Text,
			LargeImage = LargeIcon,
			TextImageRelation = ((SmallIcon == null) ? TextImageRelation.ImageAboveText : TextImageRelation.ImageBeforeText),
			ToggleOnClick = (SmallIcon != null),
			SmallImage = SmallIcon
		};
		foreach (AppCommandBase item in _items)
		{
			SplitButton.Items.Add(item.RibbonItem);
		}
		if (SmallIcon == null)
		{
			SplitButton.Click += delegate
			{
				Clicked();
			};
		}
		else
		{
			SplitButton.Click += delegate
			{
				if (SplitButton.Pressed)
				{
					Pressed();
				}
				else
				{
					Unpressed();
				}
			};
		}
		AttachTooltip();
	}
}
