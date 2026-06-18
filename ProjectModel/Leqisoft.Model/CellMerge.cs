using Leqisoft.DTO;

namespace Leqisoft.Model;

public class CellMerge
{
	public Id64 Id { get; set; }

	public int Dirty { get; set; }

	public SyncStatus Status { get; set; }

	public Cell TopLeft { get; set; }

	public Cell BottomRight { get; set; }

	public void SetSynced()
	{
		Status = SyncStatus.Synced;
	}

	public bool ContainsAndNotTopLeft(Cell cell)
	{
		if (Contains(cell))
		{
			return cell != TopLeft;
		}
		return false;
	}

	public bool Contains(Cell cell)
	{
		if (TopLeft.Row.Index <= cell.Row.Index && TopLeft.Column.Index <= cell.Column.Index && cell.Row.Index <= BottomRight.Row.Index)
		{
			return cell.Column.Index <= BottomRight.Column.Index;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return TopLeft.GetHashCode() ^ BottomRight.GetHashCode();
	}
}
