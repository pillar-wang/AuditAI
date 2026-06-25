using System;
using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class C1TextBoxEx_SupportSelfBorder : C1TextBoxEx
{
	protected Color _borderColor = Color.Black;

	private bool _isInOnPatinFun;

	public Color SelfBorderColor
	{
		get
		{
			return _borderColor;
		}
		set
		{
			_borderColor = value;
			base.BorderColor = _borderColor;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (!_isInOnPatinFun)
		{
			if (base.BorderColor != _borderColor)
			{
				_isInOnPatinFun = true;
				base.BorderColor = _borderColor;
				_isInOnPatinFun = false;
			}
			base.OnPaint(e);
		}
	}

	protected override void OnReadOnlyChanged(EventArgs e)
	{
		base.OnReadOnlyChanged(e);
		if (base.BorderColor != _borderColor)
		{
			base.BorderColor = _borderColor;
		}
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		base.OnEnabledChanged(e);
		if (base.BorderColor != _borderColor)
		{
			base.BorderColor = _borderColor;
		}
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (base.BorderColor != _borderColor)
		{
			base.BorderColor = _borderColor;
		}
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		if (base.BorderColor != _borderColor)
		{
			base.BorderColor = _borderColor;
		}
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		if (base.BorderColor != _borderColor)
		{
			base.BorderColor = _borderColor;
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (base.BorderColor != _borderColor)
		{
			base.BorderColor = _borderColor;
		}
	}
}
