using TXTextControl;

namespace Auditai.UI.Platform;

public class AppCommandHeaderLeft : AppCommandRtfCombo
{
	protected override string PropName1 => "PageHeader";

	protected override string PropName2 => "LeftValue";

	protected override HorizontalAlignment HorizontalAlignment => HorizontalAlignment.Left;
}
