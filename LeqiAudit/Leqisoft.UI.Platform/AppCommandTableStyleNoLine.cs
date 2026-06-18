using System.Drawing;
using Leqisoft.Model;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandTableStyleNoLine : AppCommandGalleryItem
{
	public override string Text => "无线";

	public override System.Drawing.Image LargeImage => Resources.TableStyleNoLine;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetBorderStyle(TableBorderStyles.NoLine);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonSetTableBorder(TableBorderStyles.NoLine);
			break;
		}
	}
}
