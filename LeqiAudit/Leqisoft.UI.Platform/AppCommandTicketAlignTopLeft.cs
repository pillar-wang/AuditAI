using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTicketAlignTopLeft : AppCommandButton
{
	public override System.Drawing.Image SmallIcon => Resources.tb_AlignTopLeft;

	protected override void Clicked()
	{
		Program.MainForm.TicketDesignEditor.SetAlign(CellTextAlign.TopLeft);
	}
}
