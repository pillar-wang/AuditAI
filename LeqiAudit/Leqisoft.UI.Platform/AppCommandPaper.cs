using System.Drawing.Printing;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class AppCommandPaper : AppCommandMenu
{
	public override string Text => "纸张大小";

	public AppCommandPaper()
		: base(new AppCommandBase[5]
		{
			AppCommands.PaperA4,
			AppCommands.PaperB4,
			AppCommands.PaperA3,
			AppCommands.PaperB5,
			AppCommands.PaperCustom
		})
	{
	}

	public override void GenerateRibbonItem()
	{
		base.GenerateRibbonItem();
	}

	public void SelectPaper(PaperKind kind)
	{
		switch (kind)
		{
		case PaperKind.A4:
			base.Menu.LargeImage = Resources.A4;
			break;
		case PaperKind.B4:
			base.Menu.LargeImage = Resources.pt_B4;
			break;
		case PaperKind.A3:
			base.Menu.LargeImage = Resources.A3;
			break;
		case PaperKind.B5:
			base.Menu.LargeImage = Resources.pt_B5;
			break;
		case PaperKind.Custom:
			base.Menu.LargeImage = Resources.PaperCustom;
			break;
		}
	}
}
