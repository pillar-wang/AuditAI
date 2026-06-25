using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTicketBackColor : AppCommandColorPicker
{
	public override Image Icon => Resources.BackColor;

	protected override void Clicked(Color color)
	{
		Program.MainForm.TicketDesignEditor.SetBackColor(color);
	}
}
