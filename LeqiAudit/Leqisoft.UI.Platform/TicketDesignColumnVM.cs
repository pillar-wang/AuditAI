using System;

namespace Leqisoft.UI.Platform;

public class TicketDesignColumnVM
{
	private int _width;

	public bool IsHiddenColumn;

	public int Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = Math.Max(1, value);
		}
	}
}
