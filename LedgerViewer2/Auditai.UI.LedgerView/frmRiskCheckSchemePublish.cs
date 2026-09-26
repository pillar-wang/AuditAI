using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.LedgerView;

/// <summary>
/// 风险检查方案上传参数收集对话框：只收集名称/备注/是否覆盖，不做任何网络调用。
/// 目标库由调用方通过 systemLibrary 传入，仅用于标题与提示文案。
/// </summary>
public class frmRiskCheckSchemePublish : Form
{
	private readonly TextBox _txtName;

	private readonly TextBox _txtNote;

	private readonly CheckBox _chkOverwrite;

	private readonly Button _btnOk;

	/// <summary>方案名称（去空白）</summary>
	public string SchemeName => _txtName.Text.Trim();

	/// <summary>方案备注（去空白）</summary>
	public string SchemeNote => _txtNote.Text.Trim();

	/// <summary>是否覆盖同名云端方案</summary>
	public bool Overwrite => _chkOverwrite.Checked;

	public frmRiskCheckSchemePublish(string defaultName, string defaultNote, string sourceLedgerName, bool systemLibrary)
	{
		base.Font = new Font("微软雅黑", 9f);
		Text = systemLibrary ? "上传为系统方案" : "上传到团队库";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		BackColor = Color.White;
		ClientSize = new Size(520, 330);

		Label lblName = new Label
		{
			Text = "方案名称：",
			Location = new Point(20, 24),
			AutoSize = true
		};
		_txtName = new TextBox
		{
			Location = new Point(100, 21),
			Size = new Size(390, 23),
			MaxLength = 100,
			Text = defaultName ?? string.Empty
		};
		Label lblNote = new Label
		{
			Text = "备注：",
			Location = new Point(20, 59),
			AutoSize = true
		};
		_txtNote = new TextBox
		{
			Location = new Point(100, 56),
			Size = new Size(390, 56),
			MaxLength = 200,
			Multiline = true,
			Text = defaultNote ?? string.Empty
		};
		Label lblSource = new Label
		{
			Text = "来源账套：" + (string.IsNullOrWhiteSpace(sourceLedgerName) ? "—" : sourceLedgerName),
			Location = new Point(100, 118),
			Size = new Size(390, 18),
			ForeColor = Color.FromArgb(100, 116, 139)
		};
		_chkOverwrite = new CheckBox
		{
			Text = "覆盖同名云端方案",
			Location = new Point(100, 142),
			AutoSize = true
		};
		Label lblTarget = new Label
		{
			Text = systemLibrary
				? "目标库：系统库（所有团队可见、只读）"
				: "目标库：团队库（仅本团队可见）",
			Location = new Point(20, 180),
			Size = new Size(480, 40),
			ForeColor = Color.FromArgb(100, 116, 139)
		};

		// 取消贴右（右缘距窗体 50），确定在左侧间距 12
		_btnOk = new Button
		{
			Text = "确定",
			Location = new Point(ClientSize.Width - 50 - 110 - 12 - 110, ClientSize.Height - 40 - 12),
			Size = new Size(110, 30),
			Enabled = !string.IsNullOrWhiteSpace(defaultName)
		};
		_btnOk.Click += delegate
		{
			OnOk();
		};
		Button btnCancel = new Button
		{
			Text = "取消",
			Location = new Point(ClientSize.Width - 50 - 110, ClientSize.Height - 40 - 12),
			Size = new Size(110, 30)
		};
		btnCancel.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		};
		_txtName.TextChanged += delegate
		{
			_btnOk.Enabled = !string.IsNullOrWhiteSpace(_txtName.Text);
		};

		base.Controls.Add(lblName);
		base.Controls.Add(_txtName);
		base.Controls.Add(lblNote);
		base.Controls.Add(_txtNote);
		base.Controls.Add(lblSource);
		base.Controls.Add(_chkOverwrite);
		base.Controls.Add(lblTarget);
		base.Controls.Add(_btnOk);
		base.Controls.Add(btnCancel);
		base.AcceptButton = _btnOk;
		base.CancelButton = btnCancel;

		Auditai.UI.Controls.Theme.SetCurrentTree(this);
	}

	private void OnOk()
	{
		if (string.IsNullOrWhiteSpace(_txtName.Text))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.Warning, "方案名称不能为空", MessageBoxButtons.OK, Text);
			return;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}
}
