using System;
using System.Collections.Generic;
using C1.Win.C1Command;

namespace Leqisoft.UI.Platform;

public static class C1ContexEx
{
	public static C1CommandMenu FromDic(Dictionary<int, Tuple<string, object>> menuList, Action<int, object> action)
	{
		C1CommandMenu c1CommandMenu = new C1CommandMenu();
		foreach (KeyValuePair<int, Tuple<string, object>> link in menuList)
		{
			C1CommandLink c1CommandLink = new C1CommandLink();
			C1Command c1Command = new C1Command
			{
				Text = link.Value.Item1
			};
			c1Command.Click += delegate
			{
				action(link.Key, link.Value.Item2);
			};
			c1CommandLink.Command = c1Command;
			c1CommandMenu.CommandLinks.Add(c1CommandLink);
		}
		return c1CommandMenu;
	}
}
