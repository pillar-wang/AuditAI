using System.Drawing;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class AppCommandParagraphAlign : AppCommandMenu
{
	public override string Text => "段落对齐";

	public override Image LargeImage => Resources.Align;

	public AppCommandParagraphAlign()
		: base(new AppCommandBase[4]
		{
			AppCommands.ParagraphAlignLeft,
			AppCommands.ParagraphAlignRight,
			AppCommands.ParagraphAlignCenter,
			AppCommands.ParagraphAlignJustify
		})
	{
	}
}
