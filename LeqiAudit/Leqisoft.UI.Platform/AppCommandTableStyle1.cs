using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTableStyle1 : AppCommandGalleryItem
{
	public override string Text => "样式1";

	public override System.Drawing.Image LargeImage => Resources.TableStyle1;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetBorderStyle(TableBorderStyles.ThickUpDownThinBody);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonSetTableBorder(TableBorderStyles.ThickUpDownThinBody);
			break;
		}
	}
}
