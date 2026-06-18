using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandForeColor : AppCommandColorPicker
{
	public override Image Icon => Resources.ForeColor;

	protected override void Clicked(Color color)
	{
		Program.MainForm.TableEditor.SetForeColor(color);
	}
}
