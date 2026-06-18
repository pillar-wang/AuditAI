using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketForeColor : AppCommandColorPicker
{
	public override Image Icon => Resources.ForeColor;

	protected override void Clicked(Color color)
	{
		Program.MainForm.TicketDesignEditor.SetForeColor(color);
	}
}
