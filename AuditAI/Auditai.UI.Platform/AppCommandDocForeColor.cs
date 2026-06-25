using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandDocForeColor : AppCommandColorPicker
{
	public override Image Icon => Resources.ForeColor;

	protected override void Clicked(Color color)
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetForeColor(color);
		}
	}
}
