using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

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
