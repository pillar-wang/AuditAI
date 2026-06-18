namespace Leqisoft.UI.Platform;

public class AppCommandTest : AppCommandButton
{
	public override string Text => "test";

	protected override void Clicked()
	{
		Program.MainForm.Test();
	}
}
