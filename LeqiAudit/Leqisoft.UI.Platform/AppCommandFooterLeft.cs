using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandFooterLeft : AppCommandRtfCombo
{
	protected override string PropName1 => "PageFooter";

	protected override string PropName2 => "LeftValue";

	protected override HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Left;
}
