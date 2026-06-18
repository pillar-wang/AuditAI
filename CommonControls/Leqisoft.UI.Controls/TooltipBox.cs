using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using C1.Win.C1SuperTooltip;

namespace Leqisoft.UI.Controls;

public class TooltipBox
{
	private const int FORM_WIDTH = 300;

	private const string TEMPLATE_CLOSEFORM_TITLEBODY = "<body style='font-family:Microsoft YaHei;font-size:12;color:Black;'>\r\n                            <table style='width:{2}px;'>\r\n                                <tr style='font-weight:Bold;'>\r\n                                    <td>{0}</td>\r\n                                    <td style='text-align:right;width:50px;'>\r\n                                        <a href='closehref' style='text-align:right;text-decoration:none;'>×</a>\r\n                                        <a href='decreasewidth' style='text-align:right; text-decoration:none;'>&lt;</a>\r\n                                        <a href='increasewidth' style='text-align:right; text-decoration:none;'>&gt;</a>\r\n                                    </td>\r\n                                </tr>\r\n                                <tr><td colspan='2'><parm><hr noshade size=1 color=Gray></parm></td></tr>\r\n                                <tr><td colspan='2' ><div>{1}</div></td></tr>\r\n                            </table>\r\n                      </body>";

	private const string TEMPLATE_CLOSEFORM_ONLYBODY = "<body style='font-family:Microsoft YaHei;font-size:12'>\r\n                        <div width='{1}px' align='right'>\r\n                            <a href='closehref' style='text-align:right; text-decoration:none;'>×</a>\r\n                            <a href='decreasewidth' style='text-align:right; text-decoration:none;'>&lt;</a>\r\n                            <a href='increasewidth' style='text-align:right; text-decoration:none;'>&gt;</a>\r\n                        </div>\r\n                        <div style='text-align:justify;'>{0}</div>\r\n                      </body>";

	private const string TEMPLATE_NORMAL_TITLEBODY = "<div style='width:{2}px;color:Black;font-family:Microsoft YaHei;font-size:12'>\r\n                        <div style='font-weight:Bold'>{0}</div>\r\n                        <parm><hr noshade size=1 color=Gray></parm>\r\n                        <div style='text-align:justify;'>{1}</div>\r\n                      </div>";

	private const string TEMPLATE_NORMAL_ONLYBODY = "<div style='width:{1}px;color:Black;font-family:Microsoft YaHei;font-size:12'>\r\n                        <div style='text-align:justify;'>{0}</div>\r\n                      </div>";

	private Dictionary<string, object> _tagDic = new Dictionary<string, object>();

	private Random rdm = new Random();

	private string _htmlBody = string.Empty;

	private C1SuperTooltip _superTooltip = new C1SuperTooltip
	{
		HitTestVisible = true,
		UseFading = false
	};

	private Timer _timer;

	private string _title;

	private string _content;

	private bool _canclose;

	private Control _control;

	private Point _point;

	private static Color _gradientColor = Color.AliceBlue;

	private static Image _gradientBackImage;

	public double Opacity { get; set; } = 0.8;


	public int Duration { get; set; }

	public int Width { get; set; } = 300;


	public bool IsBalloon { get; set; }

	public Action DurationElapsed { get; set; }

	private static Image GradientBackImage
	{
		get
		{
			Color gradientColor = Theme.SelectedLeqiTheme.ThemeContext.GradientColor;
			if (_gradientBackImage == null || !_gradientColor.Equals(gradientColor))
			{
				_gradientColor = gradientColor;
				_gradientBackImage = GradientImage(gradientColor, Color.White);
			}
			return _gradientBackImage;
		}
		set
		{
			_gradientBackImage = value;
		}
	}

	public event EventHandler<object> LinkClicked;

	public event EventHandler CloseClick;

	public TooltipBox()
	{
		_superTooltip.LinkClicked += delegate(object s, C1SuperLabelLinkClickedEventArgs e)
		{
			if (e.HRef == "closehref")
			{
				this.CloseClick?.Invoke(this, EventArgs.Empty);
			}
			else if (e.HRef == "increasewidth")
			{
				IncreaseWidth(150);
			}
			else if (e.HRef == "decreasewidth")
			{
				IncreaseWidth(-150);
			}
			else
			{
				object e2 = null;
				if (_tagDic.ContainsKey(e.HRef))
				{
					e2 = _tagDic[e.HRef];
				}
				this.LinkClicked?.Invoke(e.HRef, e2);
			}
		};
		_superTooltip.RoundedCorners = true;
	}

	public void SetText(string title, string content, bool canClose = false)
	{
		_title = title;
		_content = content;
		_canclose = canClose;
		_htmlBody = GetHtmlContent(title, content, canClose);
	}

	public void IncreaseWidth(int step)
	{
		Width += step;
		if (Width < 300)
		{
			Width = 300;
		}
		if (Width > Screen.PrimaryScreen.Bounds.Width - 50)
		{
			Width = Screen.PrimaryScreen.Bounds.Width - 50;
		}
		SetText(_title, _content, _canclose);
		if (_timer != null)
		{
			_timer.Tick -= _timer_Tick;
			_timer.Stop();
			_timer.Dispose();
		}
		Show(_control, _point);
	}

	private string GetHtmlContent(string title, string content, bool canClose)
	{
		string empty = string.Empty;
		if (canClose)
		{
			if (string.IsNullOrEmpty(title))
			{
				return string.Format("<body style='font-family:Microsoft YaHei;font-size:12'>\r\n                        <div width='{1}px' align='right'>\r\n                            <a href='closehref' style='text-align:right; text-decoration:none;'>×</a>\r\n                            <a href='decreasewidth' style='text-align:right; text-decoration:none;'>&lt;</a>\r\n                            <a href='increasewidth' style='text-align:right; text-decoration:none;'>&gt;</a>\r\n                        </div>\r\n                        <div style='text-align:justify;'>{0}</div>\r\n                      </body>", content, Width);
			}
			return string.Format("<body style='font-family:Microsoft YaHei;font-size:12;color:Black;'>\r\n                            <table style='width:{2}px;'>\r\n                                <tr style='font-weight:Bold;'>\r\n                                    <td>{0}</td>\r\n                                    <td style='text-align:right;width:50px;'>\r\n                                        <a href='closehref' style='text-align:right;text-decoration:none;'>×</a>\r\n                                        <a href='decreasewidth' style='text-align:right; text-decoration:none;'>&lt;</a>\r\n                                        <a href='increasewidth' style='text-align:right; text-decoration:none;'>&gt;</a>\r\n                                    </td>\r\n                                </tr>\r\n                                <tr><td colspan='2'><parm><hr noshade size=1 color=Gray></parm></td></tr>\r\n                                <tr><td colspan='2' ><div>{1}</div></td></tr>\r\n                            </table>\r\n                      </body>", title, content, Width);
		}
		if (string.IsNullOrEmpty(title))
		{
			return string.Format("<div style='width:{1}px;color:Black;font-family:Microsoft YaHei;font-size:12'>\r\n                        <div style='text-align:justify;'>{0}</div>\r\n                      </div>", content, Width);
		}
		return string.Format("<div style='width:{2}px;color:Black;font-family:Microsoft YaHei;font-size:12'>\r\n                        <div style='font-weight:Bold'>{0}</div>\r\n                        <parm><hr noshade size=1 color=Gray></parm>\r\n                        <div style='text-align:justify;'>{1}</div>\r\n                      </div>", title, content, Width);
	}

	public void SetTagDic(IDictionary<string, object> dic)
	{
		_tagDic = new Dictionary<string, object>(dic);
		_tagDic.Add("closehref", string.Empty);
	}

	public void Show(Control control, Point point)
	{
		_control = control;
		_point = point;
		_superTooltip.Opacity = Opacity;
		_superTooltip.IsBalloon = IsBalloon;
		_superTooltip.BackgroundImage = GradientBackImage;
		_superTooltip.Show(_htmlBody, control, point.X, point.Y, Duration);
		if (Duration > 0)
		{
			_timer = new Timer
			{
				Enabled = true,
				Interval = Duration
			};
			_timer.Tick += _timer_Tick;
		}
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		_timer.Tick -= _timer_Tick;
		_timer.Stop();
		_timer.Dispose();
		DurationElapsed?.Invoke();
	}

	public void Hide()
	{
		_superTooltip.Hide();
	}

	public void Close()
	{
		_superTooltip.Hide();
		_superTooltip.Dispose();
	}

	private static Image GradientImage(Color start, Color end)
	{
		Bitmap bitmap = new Bitmap(50, 150);
		Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		LinearGradientBrush brush = new LinearGradientBrush(rect, start, end, LinearGradientMode.Vertical);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.FillRectangle(brush, rect);
		return bitmap;
	}
}
