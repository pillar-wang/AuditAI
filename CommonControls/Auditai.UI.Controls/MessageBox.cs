using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class MessageBox
{
	public static DialogResult Show(MessageBoxIcon icon = MessageBoxIcon.None, string text = "", MessageBoxButtons buttons = MessageBoxButtons.OK, string title = "", bool scroll = false)
	{
		MessageShowBox messageShowBox = new MessageShowBox(buttons, icon);
		if (!string.IsNullOrWhiteSpace(title))
		{
			messageShowBox.Text = title;
		}
		messageShowBox.SetMessage(text);
		return messageShowBox.ShowDialog();
	}

	public static DialogResult UpdateForm(MessageBoxIcon icon, MessageBoxButtons buttons, string text)
	{
		UpdateForm updateForm = new UpdateForm(icon, buttons, text);
		return updateForm.ShowDialog();
	}
}
