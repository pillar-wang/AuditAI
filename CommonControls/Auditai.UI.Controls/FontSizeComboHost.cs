using System;
using System.Windows.Forms;
using C1.Win.C1Ribbon;

namespace Auditai.UI.Controls;

public class FontSizeComboHost : RibbonControlHost
{
	private const int MIN_FONT_SIZE = 1;

	private const int MAX_FONT_SIZE = 409;

	private FontSizeComboBox _hostee;

	public event EventHandler<FontSizeEventArgs> FontSizeSelected;

	public FontSizeComboHost()
		: this(new FontSizeComboBox())
	{
	}

	public void SelectFontSize(float fs)
	{
		fs = ToNearestHalf(fs);
		int num = Array.FindIndex(FontSize.FontSizes, (FontSize a) => a.Value == fs);
		_hostee.SelectedIndex = num;
		if (num == -1)
		{
			_hostee.Text = fs.ToString();
		}
	}

	private FontSizeComboHost(FontSizeComboBox hostee)
		: base(hostee)
	{
		_hostee = hostee;
		_hostee.KeyDown += _hostee_KeyDown;
		_hostee.SelectionChangeCommitted += _hostee_SelectionChangeCommitted;
	}

	public static float ToNearestHalf(float f)
	{
		return (float)Math.Round(f * 2f, MidpointRounding.AwayFromZero) / 2f;
	}

	private void _hostee_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			if (float.TryParse(_hostee.Text, out var result) && result >= 1f && result <= 409f)
			{
				this.FontSizeSelected?.Invoke(this, new FontSizeEventArgs(ToNearestHalf(result)));
			}
			else
			{
				MessageBox.Show(MessageBoxIcon.None, $"请输入{1}~{409}之间的值");
			}
		}
	}

	private void _hostee_SelectionChangeCommitted(object sender, EventArgs e)
	{
		this.FontSizeSelected?.Invoke(this, new FontSizeEventArgs(_hostee.SelectedFontSize.Value));
	}
}
