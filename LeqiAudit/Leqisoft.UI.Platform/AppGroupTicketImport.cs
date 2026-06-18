namespace Leqisoft.UI.Platform;

public class AppGroupTicketImport : AppCommandGroup
{
	public override string Text => "导入";

	public AppGroupTicketImport()
	{
		base.Commands.Add(AppCommands.TicketImportExcel);
		base.Commands.Add(AppCommands.TicketImportTable);
	}
}
