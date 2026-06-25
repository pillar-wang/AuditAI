using System.Drawing;
using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandTableStyle0 : AppCommandGalleryItem
{
	public override string Text => "普通";

	public override System.Drawing.Image LargeImage => Resources.TableStyle0;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetBorderStyle(TableBorderStyles.Grid);
			break;
		case MainFormView.Document:
			Program.MainForm.CurrentDocumentEditor.RibbonSetTableBorder(TableBorderStyles.Grid);
			break;
		}
	}
}
