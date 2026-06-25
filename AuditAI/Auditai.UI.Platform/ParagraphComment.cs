using System.Windows.Forms;

namespace Auditai.UI.Platform;

public class ParagraphComment
{
	private frmParagraphComment _form = new frmParagraphComment();

	public bool Changed => _form._changed;

	public string Text
	{
		get
		{
			return _form.txbCommentInput.Text;
		}
		set
		{
			_form._settingText = true;
			_form.txbCommentInput.Text = value;
			_form._settingText = false;
		}
	}

	public DialogResult ShowDialog()
	{
		return _form.ShowDialog();
	}
}
