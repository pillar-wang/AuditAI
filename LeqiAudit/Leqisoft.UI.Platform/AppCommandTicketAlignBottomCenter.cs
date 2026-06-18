using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketAlignBottomCenter : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.AlignBottomCenter;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.BottomCenter);
	}
}
