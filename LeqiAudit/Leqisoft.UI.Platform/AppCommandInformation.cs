using System;
using System.Drawing;
using System.Xml.Linq;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandInformation : AppCommandButton
{
	private TooltipBox _ttp;

	public override Image SmallIcon => Resources.Infomation;

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
		Visible = false;
	}

	public void ShowInformation(string title, string body, int duration = -1)
	{
		Visible = true;
		base.Button.Ribbon.Refresh();
		Rectangle itemBounds = base.Button.Ribbon.GetItemBounds(base.Button);
		_ttp = new TooltipBox
		{
			IsBalloon = true
		};
		_ttp.Duration = duration;
		_ttp.DurationElapsed = delegate
		{
			HideInformation();
		};
		_ttp.SetText(title, new XElement("div", new XAttribute("style", "color:red"), body).ToString());
		_ttp.Show(base.Button.Ribbon, new Point(itemBounds.Left + itemBounds.Width / 2, itemBounds.Bottom));
	}

	public void ShowInformation(Action<TooltipBox> setting, int duration = -1)
	{
		Visible = true;
		base.Button.Ribbon.Refresh();
		Rectangle itemBounds = base.Button.Ribbon.GetItemBounds(base.Button);
		_ttp = new TooltipBox
		{
			IsBalloon = true
		};
		setting(_ttp);
		_ttp.Duration = duration;
		_ttp.DurationElapsed = delegate
		{
			HideInformation();
		};
		_ttp.Show(base.Button.Ribbon, new Point(itemBounds.Left + itemBounds.Width / 2, itemBounds.Bottom));
	}

	public void HideInformation()
	{
		Visible = false;
		_ttp?.Hide();
	}
}
