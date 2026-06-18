using System;
using System.Collections.Generic;
using System.Drawing;
using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class GetNodesListener : DotPlainBaseListener
{
	private const double DPI = 96.0;

	private double _graphHeight;

	public List<Tuple<int, Rectangle>> Nodes { get; } = new List<Tuple<int, Rectangle>>();


	public List<Point[]> Edges { get; } = new List<Point[]>();


	public override void ExitLine([NotNull] DotPlainParser.LineContext context)
	{
		if (context.NODE() != null)
		{
			string text = context.FIELD(0).GetText();
			if (!text.StartsWith("_hidden"))
			{
				double num = double.Parse(context.FIELD(1).GetText());
				double num2 = _graphHeight - double.Parse(context.FIELD(2).GetText());
				double num3 = double.Parse(context.FIELD(3).GetText());
				double num4 = double.Parse(context.FIELD(4).GetText());
				Nodes.Add(Tuple.Create(int.Parse(text), new Rectangle(ToPixel(num - num3 / 2.0), ToPixel(num2 - num4 / 2.0), ToPixel(num3), ToPixel(num4))));
			}
		}
		else if (context.GRAPH() != null)
		{
			_graphHeight = double.Parse(context.FIELD(2).GetText());
		}
		else
		{
			if (context.EDGE() == null)
			{
				return;
			}
			int num5 = int.Parse(context.FIELD(2).GetText());
			string text2 = context.FIELD(num5 * 2 + 3).GetText();
			if (text2 == "solid")
			{
				Point[] array = new Point[num5 / 3 + 1];
				for (int i = 0; i < array.Length; i++)
				{
					double inch = double.Parse(context.FIELD(i * 6 + 3).GetText());
					double inch2 = _graphHeight - double.Parse(context.FIELD(i * 6 + 4).GetText());
					array[i] = new Point(ToPixel(inch), ToPixel(inch2));
				}
				Edges.Add(array);
			}
		}
	}

	private int ToPixel(double inch)
	{
		return Convert.ToInt32(inch * 96.0);
	}
}
