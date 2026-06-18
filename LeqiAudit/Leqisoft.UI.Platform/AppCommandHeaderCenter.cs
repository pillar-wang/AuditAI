using TXTextControl;

namespace Leqisoft.UI.Platform;

public class AppCommandHeaderCenter : AppCommandRtfCombo
{
	protected override string PropName1 => "PageHeader";

	protected override string PropName2 => "CenterValue";

	protected override HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Center;
}
