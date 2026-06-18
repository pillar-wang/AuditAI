using System;
using System.Windows.Forms;

namespace Leqisoft.UI.Platform;

public class LazyExcute
{
	private Timer _trigger = new Timer();

	private bool _continue;

	private int _interval = 150;

	public Action Action { get; set; }

	public LazyExcute()
	{
		_trigger.Interval = _interval;
		_trigger.Tick += Trigger_Tick;
	}

	public void SetAction(Action action)
	{
		Action = action;
	}

	public void SetInterval(int interval)
	{
		_interval = interval;
		_trigger.Interval = interval;
	}

	public void Excute()
	{
		if (_trigger.Enabled)
		{
			_continue = true;
		}
		else
		{
			_trigger.Start();
		}
	}

	private void Trigger_Tick(object sender, EventArgs e)
	{
		if (_continue)
		{
			_continue = false;
			return;
		}
		_trigger.Enabled = false;
		Action();
	}
}
