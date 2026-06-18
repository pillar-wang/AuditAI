using System;
using System.Drawing;
using System.Windows.Forms;

namespace Leqisoft.UI.Controls;

public class WinformProgressBarEx : ProgressBar
{
	protected Color _progressBarColor = Color.Green;

	private Timer _timer;

	private bool _isInTrigger;

	private int _ColorChunkDrawLength;

	private int _ColorChunkXOffset;

	private Brush _chunkPaintBrush;

	protected DateTime _scrollStartTime = DateTime.Now;

	public Color ProgressBarColor
	{
		get
		{
			return _progressBarColor;
		}
		set
		{
			if (_chunkPaintBrush != null)
			{
				_chunkPaintBrush.Dispose();
				_chunkPaintBrush = null;
			}
			_progressBarColor = value;
		}
	}

	public int ColorChunkLength { get; set; } = 100;


	public int ColorChunkMoveSpeed { get; set; } = 150;


	public WinformProgressBarEx()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		base.VisibleChanged += WinformProgressBarEx_VisibleChanged;
	}

	private void WinformProgressBarEx_VisibleChanged(object sender, EventArgs e)
	{
		if (_timer == null)
		{
			return;
		}
		if (!base.Visible)
		{
			if (_isInTrigger)
			{
				_timer.Tick -= Timer_Tick;
				_isInTrigger = false;
			}
		}
		else if (!_isInTrigger)
		{
			_timer.Tick += _timer_Tick;
			_isInTrigger = true;
			_scrollStartTime = DateTime.Now;
		}
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		try
		{
			Invalidate();
		}
		catch
		{
		}
	}

	private void UpdateColorChunkDrawData()
	{
		double totalMilliseconds = DateTime.Now.Subtract(_scrollStartTime).TotalMilliseconds;
		int num = (int)((double)ColorChunkMoveSpeed * (totalMilliseconds * 0.0010000000474974513));
		if (num >= base.Size.Width + ColorChunkLength)
		{
			_scrollStartTime = DateTime.Now;
			_ColorChunkDrawLength = 0;
			_ColorChunkXOffset = 0;
		}
		else if (num <= ColorChunkLength)
		{
			_ColorChunkDrawLength = num;
			_ColorChunkXOffset = 0;
		}
		else if (num <= base.Size.Width)
		{
			_ColorChunkDrawLength = ColorChunkLength;
			_ColorChunkXOffset = num - ColorChunkLength;
		}
		else
		{
			_ColorChunkDrawLength = base.Size.Width + ColorChunkLength - num;
			_ColorChunkXOffset = base.Size.Width - _ColorChunkDrawLength;
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (_chunkPaintBrush != null)
		{
			_chunkPaintBrush.Dispose();
			_chunkPaintBrush = null;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (_chunkPaintBrush == null)
		{
			_chunkPaintBrush = new SolidBrush(_progressBarColor);
		}
		UpdateColorChunkDrawData();
		Rectangle rect = new Rectangle(_ColorChunkXOffset, 0, _ColorChunkDrawLength, base.Height);
		e.Graphics.Clear(BackColor);
		e.Graphics.FillRectangle(_chunkPaintBrush, rect);
	}

	public void SetAnimationTrigger(Timer timer)
	{
		if (_timer != null && _isInTrigger)
		{
			_timer.Tick -= Timer_Tick;
			_timer = null;
			_isInTrigger = false;
		}
		_timer = timer;
		if (base.Visible)
		{
			timer.Tick += Timer_Tick;
			_isInTrigger = true;
			_scrollStartTime = DateTime.Now;
		}
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		if (base.Visible)
		{
			Invalidate();
		}
	}
}
