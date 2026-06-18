using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandHeaderRight : AppCommandRtfCombo
{
	protected override string PropName1 => "PageHeader";

	protected override string PropName2 => "RightValue";

	protected override HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Right;
}
