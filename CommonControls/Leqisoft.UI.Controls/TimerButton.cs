using System;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Leqisoft.UI.Controls;

public class TimerButton : C1Button
{
	private Timer _timer;

	private int _current;

	public string Format { get; set; } = "(0s)";


	public TimerButton()
	{
		_timer = new Timer
		{
			Interval = 1000
		};
		_timer.Tick += _timer_Tick;
		Text = "获取验证码";
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		_current--;
		if (_current > 0)
		{
			Text = _current.ToString(Format);
			return;
		}
		_timer.Stop();
		Text = "重新获取";
		base.Enabled = true;
	}

	public void Start(int seconds)
	{
		base.Enabled = false;
		_current = seconds;
		Text = _current.ToString(Format);
		_timer.Start();
	}

	public void Reset(string text = "重新获取")
	{
		_timer.Stop();
		Text = text;
		base.Enabled = true;
	}
}
