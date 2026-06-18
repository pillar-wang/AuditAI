namespace LedgerImport;

public class FailureContext
{
	public TableEnum Table { get; set; }

	public object RowTag { get; set; }

	public object ColTag { get; set; }

	public object UserData { get; set; }
}
