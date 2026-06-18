using System.Drawing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppGroupExport : AppCommandGroup
{
	public override string Text => "导出";

	public override Image Image => Resources.ExportExcel;

	public AppGroupExport()
	{
		base.Commands.Add(AppCommands.TableExportXlsx);
		base.Commands.Add(AppCommands.ExportDocx);
		base.Commands.Add(AppCommands.ExportImage);
		base.Commands.Add(AppCommands.ExportPdf);
		base.Commands.Add(AppCommands.FileBatchExport);
	}
}
