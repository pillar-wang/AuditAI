namespace Leqisoft.DTO;

public static class SubDisplayFlags
{
	public static TotalFlag AllSumAndData => TotalFlag.MonthSum | TotalFlag.YearSum | TotalFlag.Data;

	public static TotalFlag MonthAndData => TotalFlag.MonthSum | TotalFlag.Data;

	public static TotalFlag YearAndData => TotalFlag.YearSum | TotalFlag.Data;

	public static TotalFlag DataOnly => TotalFlag.Data;
}
