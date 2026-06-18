namespace Leqisoft.Model;

public static class TableBorderStyles
{
	private static TableBorderStyle[] BorderStyles = new TableBorderStyle[5] { Grid, ThickUpDownDashBody, ThickUpDownThinBody, ThickBorderThinBody, NoLine };

	public static TableBorderStyle Grid { get; } = new TableBorderStyle
	{
		UpDownLine = LineStyle.Thin,
		LeftRightLine = LineStyle.Thin,
		SecondLine = LineStyle.Thin,
		BodyLine = LineStyle.Thin,
		InternalNumber = 0
	};


	public static TableBorderStyle ThickUpDownDashBody { get; } = new TableBorderStyle
	{
		UpDownLine = LineStyle.Thick,
		LeftRightLine = LineStyle.None,
		SecondLine = LineStyle.Thick,
		BodyLine = LineStyle.None,
		InternalNumber = 1
	};


	public static TableBorderStyle ThickUpDownThinBody { get; } = new TableBorderStyle
	{
		UpDownLine = LineStyle.Thick,
		LeftRightLine = LineStyle.None,
		SecondLine = LineStyle.Thin,
		BodyLine = LineStyle.Thin,
		InternalNumber = 2
	};


	public static TableBorderStyle ThickBorderThinBody { get; } = new TableBorderStyle
	{
		UpDownLine = LineStyle.Thick,
		LeftRightLine = LineStyle.Thick,
		SecondLine = LineStyle.Thin,
		BodyLine = LineStyle.Thin,
		InternalNumber = 3
	};


	public static TableBorderStyle NoLine { get; } = new TableBorderStyle
	{
		UpDownLine = LineStyle.None,
		LeftRightLine = LineStyle.None,
		SecondLine = LineStyle.None,
		BodyLine = LineStyle.None,
		InternalNumber = 4
	};


	public static TableBorderStyle Test { get; } = new TableBorderStyle
	{
		UpDownLine = LineStyle.Thin,
		LeftRightLine = LineStyle.Thin,
		SecondLine = LineStyle.Thin,
		BodyLine = LineStyle.Thin
	};


	public static TableBorderStyle FromNumber(int number)
	{
		if (number == Custom)
			return CreateCustom();
		if (number >= 0 && number < BorderStyles.Length)
		{
			return BorderStyles[number];
		}
		return BorderStyles[0];
	}

	/// <summary>自定义样式的编号常量</summary>
	public const int Custom = 6;

	/// <summary>
	/// 创建一个自定义样式的实例（所有边框默认为细线 0.5pt）
	/// </summary>
	public static TableBorderStyle CreateCustom()
	{
		return new TableBorderStyle
		{
			IsCustomStyle = true,
			InternalNumber = Custom,
			UpDownLine = LineStyle.Thin,
			LeftRightLine = LineStyle.None,
			BodyLine = LineStyle.Thin,
			SecondLine = LineStyle.Thin
		};
	}
}
