using System.Collections.Generic;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Platform;

public abstract class AppCommandGallery : AppCommandItems
{
	public RibbonGallery Gallery { get; private set; }

	public sealed override RibbonItem RibbonItem => Gallery;

	public AppCommandGallery(IEnumerable<AppCommandBase> items)
		: base(items)
	{
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		if (!(Gallery == null))
		{
			return;
		}
		Gallery = new RibbonGallery();
		foreach (AppCommandBase item in _items)
		{
			Gallery.Items.Add(item.RibbonItem);
		}
		Gallery.VisibleItems = Gallery.Items.Count;
		AttachTooltip();
	}
}
