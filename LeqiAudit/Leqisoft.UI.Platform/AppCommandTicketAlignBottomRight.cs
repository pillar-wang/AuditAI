using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketAlignBottomRight : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignBottomRight;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.BottomRight);
	}
}
