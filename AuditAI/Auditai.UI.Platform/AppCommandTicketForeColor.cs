using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketForeColor : AppCommandColorPicker
{
	public override Image Icon => Resources.ForeColor;

	protected override void Clicked(Color color)
	{
		Program.MainForm.TicketDesignEditor.SetForeColor(color);
	}
}
