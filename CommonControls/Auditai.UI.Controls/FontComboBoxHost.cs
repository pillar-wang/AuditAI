using System;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Controls;

public class FontComboBoxHost : RibbonControlHost
{
	private FontComboBox hostee;

	public event EventHandler<FontFamilyEventArgs> FontFamilySelected;

	public FontComboBoxHost()
		: this(new FontComboBox())
	{
	}

	public void SelectFontFamily(string ff)
	{
		hostee.SelectedItem = ff;
	}

	private FontComboBoxHost(FontComboBox hostee)
		: base(hostee)
	{
		this.hostee = hostee;
		hostee.SelectionChangeCommitted += Hostee_SelectionChangeCommitted;
	}

	private void Hostee_SelectionChangeCommitted(object sender, EventArgs e)
	{
		this.FontFamilySelected?.Invoke(this, new FontFamilyEventArgs(hostee.SelectedFontFamily));
	}
}
