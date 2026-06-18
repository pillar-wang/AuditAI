using System;

namespace Leqisoft.Model;

public class TicketCellReference : IEquatable<TicketCellReference>
{
	public int Row { get; set; }

	public int Col { get; set; }

	public override int GetHashCode()
	{
		return Row.GetHashCode() ^ Col.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is TicketCellReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public override string ToString()
	{
		return $"({Row}, {Col})";
	}

	public bool Equals(TicketCellReference other)
	{
		if (Row == other.Row)
		{
			return Col == other.Col;
		}
		return false;
	}
}
