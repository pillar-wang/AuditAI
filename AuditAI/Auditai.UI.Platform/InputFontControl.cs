using C1.Win.C1Input;
using C1.Win.C1InputPanel;

namespace Auditai.UI.Platform;

public class InputFontControl : InputControlHost
{
	public InputFontControl()
		: base(new C1FontPicker())
	{
	}
}
