using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppGroupMultiDimension : AppCommandGroup
{
	public override string Text => "多维核算";

	public override Image Image => IconRes.CalculateTable;

	public AppGroupMultiDimension()
	{
		base.Commands.Add(AppCommands.MultiDimension);
	}
}
