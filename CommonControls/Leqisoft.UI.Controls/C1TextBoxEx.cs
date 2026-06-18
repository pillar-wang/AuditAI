using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Win.C1Input;

namespace Leqisoft.UI.Controls;

public class C1TextBoxEx : C1TextBox
{
	private bool _handlePosition;

	protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
	{
		base.OnPreviewKeyDown(e);
		if (e.KeyData == (Keys.V | Keys.Control))
		{
			try
			{
				SelectedText = Clipboard.GetText();
			}
			catch (ExternalException)
			{
			}
			_handlePosition = true;
		}
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		base.OnKeyPress(e);
		if (_handlePosition)
		{
			e.Handled = true;
			SelectedText = SelectedText ?? "";
			_handlePosition = false;
		}
	}
}
