using System.Collections.Generic;
using System.Linq;
using C1.Win.C1Command;

namespace Auditai.UI.LedgerView;

public static class ContexMenuEx
{
	internal static void ShowAll(this C1ContextMenu contex)
	{
		foreach (C1CommandLink commandLink in contex.CommandLinks)
		{
			commandLink.Command.Visible = true;
		}
	}

	internal static void OnlyShow(this C1ContextMenu contex, IEnumerable<C1CommandLink> lnks)
	{
		foreach (C1CommandLink commandLink in contex.CommandLinks)
		{
			commandLink.Command.Visible = false;
		}
		foreach (C1CommandLink lnk in lnks)
		{
			lnk.Command.Visible = true;
		}
	}

	internal static void OnlyShow(this C1ContextMenu contex, params C1CommandLink[] lnks)
	{
		contex.OnlyShow(lnks.AsEnumerable());
	}

	internal static void HideLinks(this C1ContextMenu _contex, IEnumerable<C1CommandLink> _lnks)
	{
		foreach (C1CommandLink _lnk in _lnks)
		{
			if (_contex.CommandLinks.Contains(_lnk))
			{
				_lnk.Command.Visible = false;
			}
		}
	}

	internal static void HideLinks(this C1ContextMenu contex, params C1CommandLink[] lnks)
	{
		contex.HideLinks(lnks.AsEnumerable());
	}
}
