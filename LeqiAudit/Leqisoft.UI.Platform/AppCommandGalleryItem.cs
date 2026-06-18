using System.Drawing;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandGalleryItem : AppCommandBase
{
	public RibbonGalleryItem GalleryItem { get; private set; }

	public virtual string Text { get; }

	public virtual Image LargeImage { get; }

	public sealed override RibbonItem RibbonItem => GalleryItem;

	public bool IsSelected => GalleryItem.Selected;

	public AppCommandGalleryItem()
	{
	}

	protected abstract void Clicked();

	public override void GenerateRibbonItem()
	{
		if (GalleryItem == null)
		{
			GalleryItem = new RibbonGalleryItem
			{
				Text = Text,
				LargeImage = LargeImage
			};
			GalleryItem.Click += delegate
			{
				Clicked();
			};
			AttachTooltip();
		}
	}

	public void Select()
	{
		GalleryItem.Selected = true;
	}
}
