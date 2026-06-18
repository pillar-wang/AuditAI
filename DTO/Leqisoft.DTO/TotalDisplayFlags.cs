namespace Leqisoft.DTO;

public static class TotalDisplayFlags
{
	public static TotalFlag AllSum => TotalFlag.MonthSum | TotalFlag.YearSum;

	public static TotalFlag MonthOnly => TotalFlag.MonthSum;

	public static TotalFlag YearOnly => TotalFlag.YearSum;
}
