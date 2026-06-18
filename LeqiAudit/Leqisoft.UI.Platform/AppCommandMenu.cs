using System.Collections.Generic;
using System.Drawing;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandMenu : AppCommandItems
{
	public RibbonMenu Menu { get; private set; }

	public virtual string Text { get; }

	public virtual Image LargeImage { get; }

	public sealed override RibbonItem RibbonItem => Menu;

	public AppCommandMenu(IEnumerable<AppCommandBase> items)
		: base(items)
	{
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		if (!(Menu == null))
		{
			return;
		}
		Menu = new RibbonMenu
		{
			Text = Text,
			LargeImage = LargeImage,
			TextImageRelation = TextImageRelation.ImageAboveText
		};
		foreach (AppCommandBase item in _items)
		{
			Menu.Items.Add(item.RibbonItem);
		}
		AttachTooltip();
	}
}
