using System;

namespace Auditai.UI.Platform;

public class TicketDesignRowVM
{
	private int _height;

	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = Math.Max(1, value);
		}
	}
}
