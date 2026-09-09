using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Auditai.EmojiResource;

namespace Auditai.UI.Controls;

public class BulletForm : Form
{
	private static Font _font = new Font("微软雅黑", 15f, FontStyle.Bold);

	private List<Bullet> _bullets = new List<Bullet>();

	private Timer _twinkleTimer = new Timer
	{
		Interval = 500
	};

	private bool _imageDisplay = true;

	private Timer _repaintTimer = new Timer
	{
		// 修复：原 Interval=1ms（约 1000 帧/秒），每帧全窗体重绘 + ClearType 抗锯齿文本，
		// CPU 占用极高且 GDI 画刷分配速率远超回收。降至 33ms（约 30fps）对弹幕动画足够流畅。
		Interval = 33
	};

	private Form _owner;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 32;
			return createParams;
		}
	}

	public BulletForm(Form owner)
	{
		_owner = owner;
		base.Owner = _owner;
		base.TopMost = true;
		base.ShowInTaskbar = false;
		BackColor = Color.White;
		base.TransparencyKey = Color.White;
		base.FormBorderStyle = FormBorderStyle.None;
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		DoubleBuffered = true;
		Show();
		base.Paint += _form_Paint;
		_repaintTimer.Tick += _timer_Tick;
		_twinkleTimer.Tick += _twinkleTimer_Tick;
		_owner.Paint += _owner_Paint;
		_repaintTimer.Start();
	}

	private void _twinkleTimer_Tick(object sender, EventArgs e)
	{
		_imageDisplay = !_imageDisplay;
	}

	public void Launch(Bullet bullet)
	{
		Bullet bullet2 = _bullets.LastOrDefault();
		if (bullet2 == null || bullet2.Y >= 500.0)
		{
			bullet.Y = 50.0;
		}
		else
		{
			bullet.Y = bullet2.Y + 30.0;
		}
		_bullets.Add(bullet);
		if (!_repaintTimer.Enabled)
		{
			_repaintTimer.Start();
		}
	}

	private void _form_Paint(object sender, PaintEventArgs e)
	{
		if (!_twinkleTimer.Enabled)
		{
			_twinkleTimer.Enabled = true;
		}
		base.Location = _owner.Location;
		base.Width = _owner.Width;
		base.Height = _owner.Height;
		_bullets.RemoveAll((Bullet b) => b.GetX() > (double)base.Width);
		if (_bullets.Count == 0)
		{
			_repaintTimer.Stop();
			return;
		}
		foreach (Bullet bullet in _bullets)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			DrawBullet(e.Graphics, _font, new PointF((float)bullet.GetX(), (float)bullet.Y), bullet);
		}
	}

	private void DrawBullet(Graphics graphics, Font _font, PointF begin, Bullet bullet)
	{
		float num = begin.X;
		float num2 = begin.Y;
		Dictionary<string, Image> images = EmojiLib.GetImages();
		string[] array = Regex.Split(bullet.Text, "\\[:([^\\[\\]]+)\\]");
		if (bullet.Image != null)
		{
			graphics.DrawImage(bullet.Image, num, num2, 32f, 32f);
			num += 32f;
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (images.ContainsKey(text))
			{
				graphics.DrawImage(images[text], num, num2 + 2f, 23f, 23f);
				num += 23f;
			}
			else
			{
				// 修复：原实现每次 DrawString 都 new SolidBrush 且从不释放 → GDI 画刷句柄持续泄漏。
				// 改用 using 立即释放；若画笔颜色为透明则无需绘制。
				using (SolidBrush brush = new SolidBrush(Theme.SelectedAuditaiTheme.ThemeContext.BulletColor))
				{
					if (brush.Color.A != 0)
					{
						graphics.DrawString(text, _font, brush, num, num2, StringFormat.GenericDefault);
					}
				}
				num += (float)(int)graphics.MeasureString(text, _font).Width;
			}
		}
	}

	private void _owner_Paint(object sender, PaintEventArgs e)
	{
		Invalidate();
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		Invalidate();
	}
}
