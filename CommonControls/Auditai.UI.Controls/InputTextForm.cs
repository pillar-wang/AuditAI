using System;
using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class InputTextForm
{
	private InputTextFormImpl _form;

	private Action<string> _continuation;

	public event EventHandler<TextEventArgs> Closed;

	public static void Show(Point location, Size size, string defaultText, Action<string> continuation = null)
	{
		InputTextForm inputTextForm = new InputTextForm
		{
			_continuation = continuation,
			_form = new InputTextFormImpl
			{
				Location = location,
				Size = size,
				txtInput = 
				{
					Value = defaultText
				}
			}
		};
		inputTextForm._form.FormClosed += inputTextForm.Form_FormClosed;
		inputTextForm._form.Deactivate += inputTextForm.Form_Deactivate;
		inputTextForm._form.Show();
	}

	private void Form_Deactivate(object sender, EventArgs e)
	{
		_form.Close();
	}

	private void Form_FormClosed(object sender, FormClosedEventArgs e)
	{
		if (_continuation == null)
		{
			this.Closed?.Invoke(this, new TextEventArgs(_form.txtInput.Text));
		}
		else
		{
			_continuation(_form.txtInput.Text);
		}
	}
}
