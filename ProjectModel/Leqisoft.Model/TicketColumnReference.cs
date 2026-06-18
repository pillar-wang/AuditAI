using System;

namespace Leqisoft.Model;

public class TicketColumnReference : IEquatable<TicketColumnReference>
{
	public int Col { get; set; }

	public override int GetHashCode()
	{
		return Col.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is TicketColumnReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public override string ToString()
	{
		return Col.ToString();
	}

	public bool Equals(TicketColumnReference other)
	{
		return Col == other.Col;
	}

	public bool Contains(TicketCellReference c, int row1, int row2)
	{
		if (Col == c.Col && row1 <= c.Row)
		{
			return c.Row <= row2;
		}
		return false;
	}
}
