using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandFooterCenter : AppCommandRtfCombo
{
	protected override string PropName1 => "PageFooter";

	protected override string PropName2 => "CenterValue";

	protected override HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Center;
}
