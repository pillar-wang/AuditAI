using System;
using System.Drawing;
using System.Windows.Forms;

namespace Leqisoft.UI.Controls;

public class PictureBoxEx : PictureBox
{
	private float _zoomFactor = 1f;

	private PointF _center;

	public float ZoomFactor
	{
		get
		{
			return _zoomFactor;
		}
		set
		{
			_zoomFactor = value;
			if (_zoomFactor < 0f)
			{
				_zoomFactor = 0f;
			}
			Refresh();
		}
	}

	public PointF Center
	{
		get
		{
			return _center;
		}
		set
		{
			_center = value;
			Refresh();
		}
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		if (base.Image != null)
		{
			ImageAnimator.UpdateFrames(base.Image);
			float num = (float)base.Image.Width * ZoomFactor;
			float num2 = (float)base.Image.Height * ZoomFactor;
			pe.Graphics.DrawImage(base.Image, (int)((float)base.Width * Center.X - num / 2f), (int)((float)base.Height * Center.Y - num2 / 2f), (int)num, (int)num2);
		}
	}

	private void AnimateImage(object sender, EventArgs e)
	{
		Invalidate();
	}
}
