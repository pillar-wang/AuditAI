using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class FontSizeComboBox : ComboBox
{
	public FontSize SelectedFontSize => (FontSize)base.SelectedValue;

	public FontSizeComboBox()
	{
		BindingContext = new BindingContext();
		base.DataSource = FontSize.FontSizes;
		base.DisplayMember = "Text";
	}
}
