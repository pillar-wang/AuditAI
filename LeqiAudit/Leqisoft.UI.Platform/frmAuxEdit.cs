using System.Windows.Forms;
using TXTextControl;

namespace Leqisoft.UI.Platform;

public class frmAuxEdit : Form
{
	public frmAuxEdit(AuxEditor editor)
	{
	}

	public bool AllowFreeInput { get; set; }
	public bool AllowMultiSelect { get; set; }
	public string Value { get; set; }
	public bool QueryValueChanged { get; set; }
	public string CommentValue { get; set; }
	public bool QueryCommentValueChanged { get; set; }
	public string Notice { get; set; }
	public string DefaultNotice { get; set; }
	public string CommentNotice { get; set; }
	public object DockingTab { get; set; }
	public object tabDropList { get; set; }
	public object tabEdit { get; set; }
	public TextControl rtbDropInput { get; set; }
	public bool UseWildcard { get; set; }
}