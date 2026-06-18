using C1.Win.C1Input;
using C1.Win.C1InputPanel;

namespace Leqisoft.UI.Platform;

public class InputColorControl : InputControlHost
{
	public InputColorControl()
		: base(new C1ColorPicker
		{
			ShowModalButton = false
		})
	{
	}
}
