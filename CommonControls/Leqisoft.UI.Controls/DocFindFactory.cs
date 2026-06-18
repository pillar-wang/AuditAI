namespace Leqisoft.UI.Controls;

public class DocFindFactory
{
	private DocFindInstance instance = new DocFindInstance();

	public DocFindInstance Get()
	{
		instance.UpdateForm();
		return instance;
	}
}
