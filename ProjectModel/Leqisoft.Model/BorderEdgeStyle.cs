﻿namespace Leqisoft.Model;

/// <summary>
/// 表示单条边框的样式配置（线型 + 磅数）
/// </summary>
public class BorderEdgeStyle
{
	/// <summary>线型</summary>
	public LineStyle LineType { get; set; }

	/// <summary>磅数（以 pt 为单位，0.25pt 为最小单位）</summary>
	public float Weight { get; set; } = 0.5f;

	public BorderEdgeStyle() { }

	public BorderEdgeStyle(LineStyle lineType, float weight)
	{
		LineType = lineType;
		Weight = weight;
	}

	/// <summary>
	/// 将磅数转换为缇（1pt = 20缇）
	/// </summary>
	public int ToTwips()
	{
		if (LineType == LineStyle.None)
			return 0;
		return (int)(Weight * 20f);
	}

	public BorderEdgeStyle Clone()
	{
		return new BorderEdgeStyle(LineType, Weight);
	}
}
