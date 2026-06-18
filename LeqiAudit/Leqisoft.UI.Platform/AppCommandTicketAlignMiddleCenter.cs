using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketAlignMiddleCenter : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignMiddleCenter;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.MiddleCenter);
	}
}
