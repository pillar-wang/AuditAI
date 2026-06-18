using System;
using System.Drawing;

namespace CrawlerForm;

public class Circle
{
	private const float PI = 3.14f;

	private int X;

	private int Y;

	private float Radius;

	public Circle(int x, int y, float radius)
	{
		X = x;
		Y = y;
		Radius = radius;
	}

	public Point GetAnglePoint(int angle)
	{
		Point result = default(Point);
		result.X = (int)((double)X + (double)Radius * Math.Cos((double)angle * Math.PI / 180.0));
		result.Y = (int)((double)Y + (double)Radius * Math.Sin((double)angle * Math.PI / 180.0));
		return result;
	}
}
