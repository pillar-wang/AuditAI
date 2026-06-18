namespace Leqisoft.DTO;

public static class TeamLevelExtensions
{
	public static string ToFriendlyString(this TeamLevel tl)
	{
		return tl switch
		{
			TeamLevel.Standard => "标准版", 
			TeamLevel.Professional => "专业版", 
			TeamLevel.Ultimate => "旗舰版", 
			_ => "", 
		};
	}
}
