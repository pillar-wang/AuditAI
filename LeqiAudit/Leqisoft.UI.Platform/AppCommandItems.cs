using System.Collections.Generic;

namespace Leqisoft.UI.Platform;

public abstract class AppCommandItems : AppCommandBase
{
	protected IEnumerable<AppCommandBase> _items;

	public AppCommandItems(IEnumerable<AppCommandBase> items)
	{
		_items = items;
	}

	public override void OnAppStateChanged(AppState state)
	{
		base.OnAppStateChanged(state);
		foreach (AppCommandBase item in _items)
		{
			item.OnAppStateChanged(state);
		}
	}

	public override void GenerateRibbonItem()
	{
		foreach (AppCommandBase item in _items)
		{
			item.GenerateRibbonItem();
		}
	}
}
