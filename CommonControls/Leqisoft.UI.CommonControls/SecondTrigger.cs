using System;
using System.Windows.Forms;

namespace Leqisoft.UI.CommonControls;

public class SecondTrigger
{
	public static bool Display { get; private set; }

	public static Timer Trigger { get; }

	static SecondTrigger()
	{
		Display = true;
		Trigger = new Timer();
		Trigger.Interval = 500;
		Trigger.Tick += Trigger_Tick;
		Trigger.Start();
	}

	private static void Trigger_Tick(object sender, EventArgs e)
	{
		Display = !Display;
	}
}
