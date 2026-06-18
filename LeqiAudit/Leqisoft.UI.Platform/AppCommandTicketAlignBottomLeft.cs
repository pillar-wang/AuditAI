using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketAlignBottomLeft : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignBottomLeft;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.BottomLeft);
	}
}
