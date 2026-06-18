using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketBackColor : AppCommandColorPicker
{
	public override Image Icon => Resources.BackColor;

	protected override void Clicked(Color color)
	{
		Program.MainForm.TicketDesignEditor.SetBackColor(color);
	}
}
