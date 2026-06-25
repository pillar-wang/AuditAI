using Auditai.Model;

namespace Auditai.UI.Platform;

public class AppCommandDataFormatComma : AppCommandButton
{
	public override string Text => "1,234.56";

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.Table:
			Program.MainForm.TableEditor.SetDataFormatNumeric(DataFormatType.Comma);
			break;
		case MainFormView.EditingTitle:
			Program.MainForm.TableEditor.TitleEditor.SetDataFormatNumeric(DataFormatType.Comma);
			break;
		case MainFormView.EditingFoot:
			Program.MainForm.TableEditor.FootEditor.SetDataFormatNumeric(DataFormatType.Comma);
			break;
		}
	}
}
