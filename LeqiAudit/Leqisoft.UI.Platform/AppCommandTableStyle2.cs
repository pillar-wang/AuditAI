using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTableStyle2 : AppCommandGalleryItem
{
	public override string Text => "样式2";

	public override System.Drawing.Image LargeImage => Resources.TableStyle2;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetBorderStyle(TableBorderStyles.ThickUpDownDashBody);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonSetTableBorder(TableBorderStyles.ThickUpDownDashBody);
			break;
		}
	}
}
