using System;

namespace Auditai.Model;

public class TicketRangeReference : IEquatable<TicketRangeReference>
{
	public int Row1 { get; set; }

	public int Col1 { get; set; }

	public int Row2 { get; set; }

	public int Col2 { get; set; }

	public override int GetHashCode()
	{
		return Row1.GetHashCode() ^ Col1.GetHashCode() ^ Row2.GetHashCode() ^ Col2.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is TicketRangeReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public override string ToString()
	{
		return $"({Row1}, {Col1})-({Row2}, {Col2})";
	}

	public bool Equals(TicketRangeReference other)
	{
		if (Row1 == other.Row1 && Col1 == other.Col1 && Row2 == other.Row2)
		{
			return Col2 == other.Col2;
		}
		return false;
	}

	public bool Contains(TicketCellReference c)
	{
		if (Row1 <= c.Row && c.Row <= Row2 && Col1 <= c.Col)
		{
			return c.Col <= Col2;
		}
		return false;
	}
}
