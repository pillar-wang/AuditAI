namespace Leqisoft.UI.Controls;

public class TableFindFactory
{
	private TableFindInstance form = new TableFindInstance();

	public TableFindInstance Get()
	{
		form.UpdateForm();
		return form;
	}
}
