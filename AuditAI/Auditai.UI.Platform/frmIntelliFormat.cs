using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class frmIntelliFormat
{
	private readonly C1RibbonForm _form;

	private readonly C1SplitContainer _spl;

	private readonly C1SplitterPanel _pnlButtons;

	private readonly C1SplitterPanel _pnlMain;

	private readonly C1Button _btnOk;

	private readonly C1Button _btnCancel;

	private readonly C1CheckBox _cbxFirstLineIndent2Chars;

	private readonly C1CheckBox _cbxUnindentCenterParagraphs;

	private readonly C1CheckBox _cbxUnifySimilarParagraphsStyle;

	private readonly C1CheckBox _cbxRemoveLeadingWhitespaces;

	private readonly C1CheckBox _cbxRemoveEmptyParagraphs;

	private readonly C1CheckBox _cbxApplyNumbering;

	public IntelliFormatOptions IntelliFormatOptions => new IntelliFormatOptions
	{
		FirstLineIndent2Chars = _cbxFirstLineIndent2Chars.Checked,
		UnindentCenterParagraphs = _cbxUnindentCenterParagraphs.Checked,
		UnifySimilarParagraphsStyle = _cbxUnifySimilarParagraphsStyle.Checked,
		RemoveLeadingWhitespaces = _cbxRemoveLeadingWhitespaces.Checked,
		RemoveEmptyParagraphs = _cbxRemoveEmptyParagraphs.Checked,
		ApplyNumbering = _cbxApplyNumbering.Checked
	};

	public frmIntelliFormat()
	{
		_form = new C1RibbonForm
		{
			StartPosition = FormStartPosition.CenterScreen,
			Size = new Size(300, 300),
			Font = new Font("微软雅黑", 9f),
			Text = "智能排版",
			MaximizeBox = false,
			MinimizeBox = false,
			ShowInTaskbar = false
		};
		_spl = new C1SplitContainer
		{
			Dock = DockStyle.Fill
		};
		_form.Controls.Add(_spl);
		_pnlButtons = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Bottom,
			MinHeight = 0,
			Height = 40,
			KeepRelativeSize = false,
			Resizable = false,
			Collapsible = false
		};
		_spl.Panels.Add(_pnlButtons);
		_btnOk = new C1Button
		{
			Text = "确定",
			Anchor = (AnchorStyles.Top | AnchorStyles.Right),
			Location = new Point(-160, 8)
		};
		_btnOk.Click += _btnOk_Click;
		_pnlButtons.Controls.Add(_btnOk);
		_btnCancel = new C1Button
		{
			Text = "取消",
			Anchor = (AnchorStyles.Top | AnchorStyles.Right),
			Location = new Point(-80, 8)
		};
		_btnCancel.Click += _btnCancel_Click;
		_pnlButtons.Controls.Add(_btnCancel);
		_pnlMain = new C1SplitterPanel
		{
			KeepRelativeSize = false,
			Resizable = false,
			Collapsible = false
		};
		_spl.Panels.Add(_pnlMain);
		_cbxFirstLineIndent2Chars = new C1CheckBox
		{
			Text = "段落首行缩进两个字符",
			AutoSize = true,
			Location = new Point(30, 10),
			Checked = true
		};
		_pnlMain.Controls.Add(_cbxFirstLineIndent2Chars);
		_cbxUnindentCenterParagraphs = new C1CheckBox
		{
			Text = "水平居中段落取消缩进",
			AutoSize = true,
			Location = new Point(30, 40),
			Checked = true
		};
		_pnlMain.Controls.Add(_cbxUnindentCenterParagraphs);
		_cbxUnifySimilarParagraphsStyle = new C1CheckBox
		{
			Text = "相同类型段落统一样式",
			AutoSize = true,
			Location = new Point(30, 70),
			Checked = true
		};
		_cbxApplyNumbering = new C1CheckBox
		{
			Text = "自动重排段落编号",
			AutoSize = true,
			Location = new Point(30, 100),
			Checked = true
		};
		_pnlMain.Controls.Add(_cbxApplyNumbering);
		_pnlMain.Controls.Add(_cbxUnifySimilarParagraphsStyle);
		_cbxRemoveLeadingWhitespaces = new C1CheckBox
		{
			Text = "删除段落首行空格",
			AutoSize = true,
			Location = new Point(30, 130),
			Checked = false
		};
		_pnlMain.Controls.Add(_cbxRemoveLeadingWhitespaces);
		_cbxRemoveEmptyParagraphs = new C1CheckBox
		{
			Text = "删除无内容空段落",
			AutoSize = true,
			Location = new Point(30, 160),
			Checked = false,
			Visible = false
		};
		_pnlMain.Controls.Add(_cbxRemoveEmptyParagraphs);
		Theme.SetCurrentTree(_form);
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.IntelliLayout16);
	}

	public DialogResult ShowDialog()
	{
		return _form.ShowDialog();
	}

	private void _btnCancel_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.Cancel;
	}

	private void _btnOk_Click(object sender, EventArgs e)
	{
		_form.DialogResult = DialogResult.OK;
	}
}
