using Auditai.Model;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandPaperDirection : AppCommandMenu
{
	public override string Text => "纸张方向";

	public AppCommandPaperDirection()
		: base(new AppCommandBase[2]
		{
			AppCommands.Portrait,
			AppCommands.Landscape
		})
	{
	}

	public void SelectPaperDirection(Direction direction)
	{
		if (direction == Direction.Vertical)
		{
			base.Menu.LargeImage = Resources.Portrait;
		}
		else
		{
			base.Menu.LargeImage = Resources.Landscape;
		}
	}
}
