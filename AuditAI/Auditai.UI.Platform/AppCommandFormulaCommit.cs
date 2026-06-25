using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandFormulaCommit : AppCommandButton
{
	public override string Text => "保存公式";

	public override Image LargeIcon => Resources.FormulaCommit;

	protected override void Clicked()
	{
		switch (Program.MainForm.State.ViewKind)
		{
		case MainFormView.EditingFormula:
		case MainFormView.TicketFormula:
			Program.MainForm.FormulaEditor.Commit();
			break;
		case MainFormView.EditingValidation:
			Program.MainForm.TableEditor.ValidationEditor.Commit();
			break;
		}
	}
}
