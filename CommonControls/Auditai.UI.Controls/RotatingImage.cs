using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class RotatingImage : Control, ISupportInitialize
{
	private float _rotationAngle;

	private float _speed = 1f;

	protected Timer _timer;

	private readonly Stopwatch _frameStopwatch = new Stopwatch();

	private IContainer components;

	public Image Image { get; set; }

	public RotatingImage()
	{
		DoubleBuffered = true;
		InitializeComponent();
	}

	public void StartRotate(int frameDeltaTime, float rotateSpeed)
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer.Dispose();
			_timer = null;
		}
		_speed = rotateSpeed;
		_timer = new Timer();
		_timer.Interval = frameDeltaTime;
		_timer.Tick += _timer_Tick;
		_timer.Start();
		_frameStopwatch.Restart();
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		float num = (float)_frameStopwatch.Elapsed.TotalSeconds;
		_rotationAngle = (_rotationAngle + _speed * num) % 360f;
		_frameStopwatch.Restart();
		Invalidate();
	}

	public void StopRotate()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer.Dispose();
			_timer = null;
			_frameStopwatch.Stop();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Image image = Image;
		if (image != null)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
			PointF point = new PointF((float)base.ClientSize.Width / 2f, (float)base.ClientSize.Height / 2f);
			using (Matrix matrix = new Matrix())
			{
				matrix.RotateAt(_rotationAngle, point);
				e.Graphics.Transform = matrix;
			}
			RectangleF rect = new RectangleF(point.X - (float)image.Width / 2f, point.Y - (float)image.Height / 2f, image.Width, image.Height);
			e.Graphics.DrawImage(image, rect);
			e.Graphics.ResetTransform();
		}
	}

	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}
}
