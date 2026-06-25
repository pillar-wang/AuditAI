using System;
using Newtonsoft.Json;

namespace Auditai.Model;

[JsonObject]
public class TicketBorder
{
	public int Width { get; set; }

	public TicketBorder Clone()
	{
		return (TicketBorder)MemberwiseClone();
	}

	public TicketBorder MergeWith(TicketBorder rhs)
	{
		return new TicketBorder
		{
			Width = Math.Max(Width, rhs.Width)
		};
	}
}
