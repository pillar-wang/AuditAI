using System;
using C1.Win.C1Ribbon;

namespace Leqisoft.UI.Controls;

public class RibbonNumericBoxEx : RibbonNumericBox
{
	private bool _isUserChange = true;

	public new decimal Value
	{
		get
		{
			return base.Value;
		}
		set
		{
			_isUserChange = false;
			base.Value = value;
			_isUserChange = true;
		}
	}

	protected override void OnValueChanged(EventArgs e)
	{
		if (_isUserChange)
		{
			base.OnValueChanged(e);
		}
	}
}
