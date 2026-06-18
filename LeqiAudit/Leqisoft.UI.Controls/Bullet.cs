using System;
using System.Drawing;

namespace Leqisoft.UI.Controls;

public class Bullet
{
	private static Random _rng = new Random();

	private DateTime _createTime = DateTime.Now;

	private double _speed;

	public Image Image { get; set; }

	public string Text { get; set; }

	public double Y { get; set; }

	public Bullet(string text, Image image = null)
	{
		Text = text;
		Image = image;
		_speed = 150.0;
		Y = _rng.Next(10, 200);
	}

	public double GetX()
	{
		return (DateTime.Now - _createTime).TotalSeconds * _speed;
	}
}
