using System.Collections.Generic;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Platform;

public class AppCommandToolbar : AppCommandItems
{
	public RibbonToolBar ToolBar { get; private set; }

	public sealed override RibbonItem RibbonItem => ToolBar;

	public AppCommandToolbar(IEnumerable<AppCommandBase> items)
		: base(items)
	{
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		if (!(ToolBar == null))
		{
			return;
		}
		ToolBar = new RibbonToolBar();
		foreach (AppCommandBase item in _items)
		{
			ToolBar.Items.Add(item.RibbonItem);
		}
	}
}
