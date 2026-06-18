using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public class TicketMerge
{
	public int TopRow { get; set; }

	public int LeftColumn { get; set; }

	public int BottomRow { get; set; }

	public int RightColumn { get; set; }

	public bool Contains(int row, int col)
	{
		if (TopRow <= row && row <= BottomRow && LeftColumn <= col)
		{
			return col <= RightColumn;
		}
		return false;
	}

	public bool IntersectsWith(int topRow, int leftCol, int bottomRow, int rightCol)
	{
		bool flag = bottomRow >= TopRow && topRow <= BottomRow;
		bool flag2 = rightCol >= LeftColumn && leftCol <= RightColumn;
		return flag && flag2;
	}

	public bool IntersectsWith(TicketMerge ticketMerge)
	{
		return IntersectsWith(ticketMerge.TopRow, ticketMerge.LeftColumn, ticketMerge.BottomRow, ticketMerge.RightColumn);
	}

	public override string ToString()
	{
		return $"({TopRow}, {LeftColumn})-({BottomRow}, {RightColumn})";
	}
}
