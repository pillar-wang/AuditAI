using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandDocBackColor : AppCommandColorPicker
{
	public override Image Icon => Resources.BackColor;

	protected override void Clicked(Color color)
	{
		MainFormView viewKind = Program.MainForm.State.ViewKind;
		if (viewKind == MainFormView.Document)
		{
			Program.MainForm.CurrentDocumentEditor.SetBackColor(color);
		}
	}
}
