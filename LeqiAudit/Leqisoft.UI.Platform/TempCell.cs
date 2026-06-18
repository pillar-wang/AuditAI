namespace Leqisoft.UI.Platform;

public class TempCell
{
	public int Col { get; set; }

	public long CellId { get; set; }

	public string Formula { get; set; }

	public TempCell(int col)
	{
		Col = col;
	}
}
