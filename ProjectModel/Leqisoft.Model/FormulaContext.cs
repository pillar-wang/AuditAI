namespace Leqisoft.Model;

public class FormulaContext
{
	public Project Project { get; set; }

	public Table Table { get; set; }

	public Column Column { get; set; }

	public Cell Cell { get; set; }

	public TableTitleCell TitleOrFoot { get; set; }

	public int TitleOrFootRow { get; set; }

	public int TitleOrFootCol { get; set; }

	public TicketDesignCellVM TicketCell { get; set; }

	public int DataRowStart { get; set; }

	public int DataRowCount { get; set; }

	public TicketTable Ticket { get; set; }

	public int EditingFlagOnTicketDesign { get; set; }

	public FormulaContextKind Kind { get; set; }

	public ILegderVirtualTableSetting LegderVirtualTableSetting { get; set; }

	public FormulaContext Clone()
	{
		return (FormulaContext)MemberwiseClone();
	}
}
