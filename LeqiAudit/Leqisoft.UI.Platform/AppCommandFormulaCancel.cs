using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandFormulaCancel : AppCommandButton
{
	public override string Text => "取消保存";

	public override Image LargeIcon => Resources.FormulaCancel;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingFormula:
		case MainFormView.TicketFormula:
			Program.MainForm.FormulaEditor.CancelEdit();
			break;
		case MainFormView.EditingValidation:
			Program.MainForm.TableEditor.ValidationEditor.Cancel();
			break;
		}
	}
}
