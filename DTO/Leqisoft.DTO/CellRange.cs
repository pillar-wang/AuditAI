namespace Leqisoft.DTO;

public struct CellRange
{
	public int r1;

	public int c1;

	public int r2;

	public int c2;

	public CellRange(int r1, int c1, int r2, int c2)
	{
		this.r1 = r1;
		this.c1 = c1;
		this.r2 = r2;
		this.c2 = c2;
	}

	public override string ToString()
	{
		return $"({r1}, {c1})-({r2}, {c2})";
	}
}
