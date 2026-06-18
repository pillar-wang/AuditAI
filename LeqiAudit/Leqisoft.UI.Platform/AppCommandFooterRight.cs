using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandFooterRight : AppCommandRtfCombo
{
	protected override string PropName1 => "PageFooter";

	protected override string PropName2 => "RightValue";

	protected override HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Right;
}
