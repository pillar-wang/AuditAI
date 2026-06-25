using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class FindReplace : C1RibbonForm
{
	public const int WM_SYSCOMMAND = 274;

	public const int SC_MOVE = 61456;

	public const int HTCAPTION = 2;

	private bool IsReplace;

	private Dictionary<string, MatchMode> MatchModeDic = new Dictionary<string, MatchMode>
	{
		{
			"任意位置",
			MatchMode.Any
		},
		{
			"开始位置",
			MatchMode.Start
		},
		{
			"结束位置",
			MatchMode.End
		},
		{
			"完全匹配",
			MatchMode.Exact
		}
	};

	private Dictionary<string, ReplaceMode> ReplaceModeDic = new Dictionary<string, ReplaceMode>
	{
		{
			"匹配内容",
			ReplaceMode.MatchText
		},
		{
			"全部内容",
			ReplaceMode.AllText
		}
	};

	private Dictionary<string, ScopeMode> ScopeModeDic = new Dictionary<string, ScopeMode>
	{
		{
			"当前表格",
			ScopeMode.Current
		},
		{
			"全部表格",
			ScopeMode.Global
		}
	};

	private readonly int formHeight;

	private readonly int ctnAllHeight;

	private readonly int pnlFindHeight;

	private readonly int pnlReplaceHeight;

	private bool _isSuspendTextChangeEvent;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlFindText;

	private C1CheckBox ckbIsMatchCase;

	private C1TextBoxEx txtFindText;

	private C1Label c1Label1;

	private C1Label c1Label2;

	private C1Button btnFindNext;

	private C1Button btnReplace1;

	private C1ComboBox cmbMatchModeList;

	private C1SplitterPanel pnlReplaceText;

	private C1TextBoxEx txtReplaceText;

	private C1Button btnReplaceAll;

	private C1Label c1Label5;

	private C1Label c1Label4;

	private C1ComboBox cmbReplaceModeList;

	private C1Button btnRepalce2;

	internal C1ComboBox cboScope;

	internal C1Label lblScope;

	public event EventHandler<FindNextEventArgs> FindNextHandler;

	public event EventHandler<ReplaceEventArgs> ReplaceHandler;

	[DllImport("user32.dll")]
	public static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

	public FindReplace(TableFindInstance owner)
	{
		InitializeComponent();
		btnFindNext.Enabled = false;
		btnRepalce2.Enabled = false;
		btnReplaceAll.Enabled = false;
		formHeight = base.Height;
		ctnAllHeight = ctnAll.Height;
		pnlFindHeight = pnlFindText.Height;
		pnlReplaceHeight = pnlReplaceText.Height;
		txtReplaceText.TextChanged += TxtReplaceText_TextChanged;
		txtFindText.TextChanged += TxtFindText_TextChanged;
		cmbMatchModeList.ItemsDataSource = MatchModeDic.Keys;
		cmbMatchModeList.DataSource = MatchModeDic.Values;
		cmbMatchModeList.SelectedIndex = 0;
		cmbReplaceModeList.ItemsDataSource = ReplaceModeDic.Keys;
		cmbReplaceModeList.DataSource = ReplaceModeDic.Values;
		cmbReplaceModeList.SelectedIndex = 0;
		cboScope.ItemsDataSource = ScopeModeDic.Keys;
		cboScope.DataSource = ScopeModeDic.Values;
		cboScope.SelectedIndex = 0;
		base.StartPosition = FormStartPosition.Manual;
		base.TopMost = true;
	}

	private void TxtFindText_TextChanged(object sender, EventArgs e)
	{
		RemoveTextBoxNewLineChar(txtFindText);
	}

	private void TxtReplaceText_TextChanged(object sender, EventArgs e)
	{
		RemoveTextBoxNewLineChar(txtReplaceText);
	}

	private void RemoveTextBoxNewLineChar(C1TextBoxEx txtBox)
	{
		if (_isSuspendTextChangeEvent || txtBox.Text.Length <= 0)
		{
			return;
		}
		_isSuspendTextChangeEvent = true;
		try
		{
			txtBox.Text = txtBox.Text.TrimEnd('\r', '\n', '\t');
		}
		finally
		{
			_isSuspendTextChangeEvent = false;
		}
	}

	public void Show(bool IsReplace)
	{
		this.IsReplace = IsReplace;
		pnlFindText.KeepRelativeSize = false;
		pnlReplaceText.KeepRelativeSize = true;
		pnlReplaceText.MinHeight = 0;
		int num = Screen.PrimaryScreen.Bounds.Width / 2 - base.Width / 2;
		int num2 = Screen.PrimaryScreen.Bounds.Height / 2 - base.Height / 2;
		base.DesktopLocation = new Point(num, num2 + 100);
		if (!this.IsReplace)
		{
			Text = "查找替换";
			base.Height = formHeight - pnlReplaceHeight - 10;
			btnFindNext.Enabled = false;
			btnRepalce2.Enabled = false;
			btnReplaceAll.Enabled = false;
			btnReplace1.Visible = true;
			base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Replace);
		}
		else
		{
			btnReplace1_Click(null, null);
			base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Replace);
		}
		txtFindText.Focus();
		Show();
	}

	private void btnFindNext_Click(object sender, EventArgs e)
	{
		this.FindNextHandler?.Invoke(this, new FindNextEventArgs
		{
			FindValue = txtFindText.Text,
			MatchMode = MatchModeDic[cmbMatchModeList.SelectedItem.ToString()],
			IsMatchCase = ckbIsMatchCase.Checked,
			ScopeMode = ScopeModeDic[cboScope.SelectedItem.ToString()]
		});
	}

	private void btnRepalce2_Click(object sender, EventArgs e)
	{
		if (txtFindText.Text.Equals(txtReplaceText.Text))
		{
			MessageBox.Show(MessageBoxIcon.None, "查找替换文本相同");
			return;
		}
		if (txtFindText.Text.Length == 0)
		{
			MessageBox.Show(MessageBoxIcon.None, "查找文本不能为空");
			return;
		}
		this.ReplaceHandler?.Invoke(this, new ReplaceEventArgs
		{
			FindValue = txtFindText.Text,
			MatchMode = MatchModeDic[cmbMatchModeList.SelectedItem.ToString()],
			IsMatchCase = ckbIsMatchCase.Checked,
			ReplaceMode = ReplaceModeDic[cmbReplaceModeList.SelectedItem.ToString()],
			ReplaceValue = txtReplaceText.Text,
			IsReplaceAll = false,
			ScopeMode = ScopeModeDic[cboScope.SelectedItem.ToString()]
		});
	}

	private void btnReplaceAll_Click(object sender, EventArgs e)
	{
		if (txtFindText.Text.Equals(txtReplaceText.Text))
		{
			MessageBox.Show(MessageBoxIcon.None, "查找替换文本相同");
			return;
		}
		if (txtFindText.Text.Length == 0)
		{
			MessageBox.Show(MessageBoxIcon.None, "查找文本不能为空");
			return;
		}
		this.ReplaceHandler?.Invoke(this, new ReplaceEventArgs
		{
			FindValue = txtFindText.Text,
			MatchMode = MatchModeDic[cmbMatchModeList.SelectedItem.ToString()],
			IsMatchCase = ckbIsMatchCase.Checked,
			ReplaceMode = ReplaceModeDic[cmbReplaceModeList.SelectedItem.ToString()],
			ReplaceValue = txtReplaceText.Text,
			IsReplaceAll = true,
			ScopeMode = ScopeModeDic[cboScope.SelectedItem.ToString()]
		});
	}

	private void btnReplace1_Click(object sender, EventArgs e)
	{
		Text = "查找替换";
		base.Height = formHeight;
		ctnAll.Height = ctnAllHeight;
		pnlReplaceText.Visible = true;
		btnReplace1.Visible = false;
	}

	private void FindReplace_MouseDown(object sender, MouseEventArgs e)
	{
		ReleaseCapture();
		SendMessage(base.Handle, 274, 61458, 0);
	}

	private void txtFindText_TextChanged(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(txtFindText.Text))
		{
			btnFindNext.Enabled = false;
			btnRepalce2.Enabled = false;
			btnReplaceAll.Enabled = false;
		}
		else
		{
			btnFindNext.Enabled = true;
			btnRepalce2.Enabled = true;
			btnReplaceAll.Enabled = true;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlFindText = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.cboScope = new C1.Win.C1Input.C1ComboBox();
		this.lblScope = new C1.Win.C1Input.C1Label();
		this.ckbIsMatchCase = new C1.Win.C1Input.C1CheckBox();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.c1Label2 = new C1.Win.C1Input.C1Label();
		this.btnFindNext = new C1.Win.C1Input.C1Button();
		this.btnReplace1 = new C1.Win.C1Input.C1Button();
		this.cmbMatchModeList = new C1.Win.C1Input.C1ComboBox();
		this.pnlReplaceText = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnRepalce2 = new C1.Win.C1Input.C1Button();
		this.btnReplaceAll = new C1.Win.C1Input.C1Button();
		this.c1Label5 = new C1.Win.C1Input.C1Label();
		this.c1Label4 = new C1.Win.C1Input.C1Label();
		this.cmbReplaceModeList = new C1.Win.C1Input.C1ComboBox();
		this.txtFindText = new Auditai.UI.Controls.C1TextBoxEx();
		this.txtReplaceText = new Auditai.UI.Controls.C1TextBoxEx();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlFindText.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cboScope).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblScope).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckbIsMatchCase).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnFindNext).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnReplace1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cmbMatchModeList).BeginInit();
		this.pnlReplaceText.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnRepalce2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnReplaceAll).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label5).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label4).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cmbReplaceModeList).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtFindText).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtReplaceText).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.ctnAll.HeaderHeight = 27;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlFindText);
		this.ctnAll.Panels.Add(this.pnlReplaceText);
		this.ctnAll.Size = new System.Drawing.Size(484, 241);
		this.ctnAll.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlFindText.Controls.Add(this.cboScope);
		this.pnlFindText.Controls.Add(this.lblScope);
		this.pnlFindText.Controls.Add(this.ckbIsMatchCase);
		this.pnlFindText.Controls.Add(this.txtFindText);
		this.pnlFindText.Controls.Add(this.c1Label1);
		this.pnlFindText.Controls.Add(this.c1Label2);
		this.pnlFindText.Controls.Add(this.btnFindNext);
		this.pnlFindText.Controls.Add(this.btnReplace1);
		this.pnlFindText.Controls.Add(this.cmbMatchModeList);
		this.pnlFindText.Height = 140;
		this.pnlFindText.KeepRelativeSize = false;
		this.pnlFindText.Location = new System.Drawing.Point(0, 0);
		this.pnlFindText.MinHeight = 52;
		this.pnlFindText.MinWidth = 52;
		this.pnlFindText.Name = "pnlFindText";
		this.pnlFindText.Resizable = false;
		this.pnlFindText.Size = new System.Drawing.Size(484, 140);
		this.pnlFindText.SizeRatio = 63.636;
		this.pnlFindText.TabIndex = 0;
		this.pnlFindText.Width = 484;
		this.cboScope.AllowSpinLoop = false;
		this.cboScope.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.cboScope.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.cboScope.GapHeight = 0;
		this.cboScope.ImagePadding = new System.Windows.Forms.Padding(0);
		this.cboScope.ItemsDisplayMember = "";
		this.cboScope.ItemsValueMember = "";
		this.cboScope.Location = new System.Drawing.Point(97, 79);
		this.cboScope.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.cboScope.Name = "cboScope";
		this.cboScope.Size = new System.Drawing.Size(117, 29);
		this.cboScope.TabIndex = 8;
		this.cboScope.Tag = null;
		this.cboScope.TextDetached = true;
		this.lblScope.AutoSize = true;
		this.lblScope.BackColor = System.Drawing.Color.Transparent;
		this.lblScope.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblScope.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblScope.ForeColor = System.Drawing.Color.Black;
		this.lblScope.Location = new System.Drawing.Point(14, 81);
		this.lblScope.Name = "lblScope";
		this.lblScope.Size = new System.Drawing.Size(100, 24);
		this.lblScope.TabIndex = 7;
		this.lblScope.Tag = null;
		this.lblScope.Text = "查找范围：";
		this.lblScope.TextDetached = true;
		this.lblScope.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.ckbIsMatchCase.BackColor = System.Drawing.Color.Transparent;
		this.ckbIsMatchCase.BorderColor = System.Drawing.Color.Transparent;
		this.ckbIsMatchCase.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.ckbIsMatchCase.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.ckbIsMatchCase.ForeColor = System.Drawing.Color.Black;
		this.ckbIsMatchCase.Location = new System.Drawing.Point(14, 106);
		this.ckbIsMatchCase.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.ckbIsMatchCase.Name = "ckbIsMatchCase";
		this.ckbIsMatchCase.Padding = new System.Windows.Forms.Padding(1);
		this.ckbIsMatchCase.Size = new System.Drawing.Size(121, 34);
		this.ckbIsMatchCase.TabIndex = 6;
		this.ckbIsMatchCase.Text = "区分大小写";
		this.ckbIsMatchCase.UseVisualStyleBackColor = true;
		this.ckbIsMatchCase.Value = null;
		this.c1Label1.AutoSize = true;
		this.c1Label1.BackColor = System.Drawing.Color.Transparent;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label1.ForeColor = System.Drawing.Color.Black;
		this.c1Label1.Location = new System.Drawing.Point(14, 11);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(100, 24);
		this.c1Label1.TabIndex = 1;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "查找内容：";
		this.c1Label1.TextDetached = true;
		this.c1Label2.AutoSize = true;
		this.c1Label2.BackColor = System.Drawing.Color.Transparent;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label2.ForeColor = System.Drawing.Color.Black;
		this.c1Label2.Location = new System.Drawing.Point(14, 47);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(100, 24);
		this.c1Label2.TabIndex = 2;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "匹配方式：";
		this.c1Label2.TextDetached = true;
		this.c1Label2.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
		this.btnFindNext.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnFindNext.Location = new System.Drawing.Point(374, 4);
		this.btnFindNext.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnFindNext.Name = "btnFindNext";
		this.btnFindNext.Size = new System.Drawing.Size(80, 26);
		this.btnFindNext.TabIndex = 4;
		this.btnFindNext.Text = "查找下一个";
		this.btnFindNext.UseVisualStyleBackColor = true;
		this.btnFindNext.Click += new System.EventHandler(btnFindNext_Click);
		this.btnReplace1.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnReplace1.Location = new System.Drawing.Point(374, 42);
		this.btnReplace1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnReplace1.Name = "btnReplace1";
		this.btnReplace1.Size = new System.Drawing.Size(80, 26);
		this.btnReplace1.TabIndex = 5;
		this.btnReplace1.Text = "替换";
		this.btnReplace1.UseVisualStyleBackColor = true;
		this.btnReplace1.Click += new System.EventHandler(btnReplace1_Click);
		this.cmbMatchModeList.AllowSpinLoop = false;
		this.cmbMatchModeList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.cmbMatchModeList.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.cmbMatchModeList.GapHeight = 0;
		this.cmbMatchModeList.ImagePadding = new System.Windows.Forms.Padding(0);
		this.cmbMatchModeList.ItemsDisplayMember = "";
		this.cmbMatchModeList.ItemsValueMember = "";
		this.cmbMatchModeList.Location = new System.Drawing.Point(97, 44);
		this.cmbMatchModeList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.cmbMatchModeList.Name = "cmbMatchModeList";
		this.cmbMatchModeList.Size = new System.Drawing.Size(117, 29);
		this.cmbMatchModeList.TabIndex = 6;
		this.cmbMatchModeList.Tag = null;
		this.cmbMatchModeList.TextDetached = true;
		this.pnlReplaceText.Controls.Add(this.btnRepalce2);
		this.pnlReplaceText.Controls.Add(this.txtReplaceText);
		this.pnlReplaceText.Controls.Add(this.btnReplaceAll);
		this.pnlReplaceText.Controls.Add(this.c1Label5);
		this.pnlReplaceText.Controls.Add(this.c1Label4);
		this.pnlReplaceText.Controls.Add(this.cmbReplaceModeList);
		this.pnlReplaceText.Height = 100;
		this.pnlReplaceText.Location = new System.Drawing.Point(0, 141);
		this.pnlReplaceText.MinHeight = 52;
		this.pnlReplaceText.MinWidth = 52;
		this.pnlReplaceText.Name = "pnlReplaceText";
		this.pnlReplaceText.Resizable = false;
		this.pnlReplaceText.Size = new System.Drawing.Size(484, 100);
		this.pnlReplaceText.SizeRatio = 100.0;
		this.pnlReplaceText.TabIndex = 1;
		this.pnlReplaceText.Visible = false;
		this.pnlReplaceText.Width = 484;
		this.btnRepalce2.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnRepalce2.Location = new System.Drawing.Point(374, 10);
		this.btnRepalce2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnRepalce2.Name = "btnRepalce2";
		this.btnRepalce2.Size = new System.Drawing.Size(80, 26);
		this.btnRepalce2.TabIndex = 6;
		this.btnRepalce2.Text = "替换";
		this.btnRepalce2.UseVisualStyleBackColor = true;
		this.btnRepalce2.Click += new System.EventHandler(btnRepalce2_Click);
		this.btnReplaceAll.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnReplaceAll.Location = new System.Drawing.Point(374, 48);
		this.btnReplaceAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.btnReplaceAll.Name = "btnReplaceAll";
		this.btnReplaceAll.Size = new System.Drawing.Size(80, 26);
		this.btnReplaceAll.TabIndex = 3;
		this.btnReplaceAll.Text = "全部替换";
		this.btnReplaceAll.UseVisualStyleBackColor = true;
		this.btnReplaceAll.Click += new System.EventHandler(btnReplaceAll_Click);
		this.c1Label5.AutoSize = true;
		this.c1Label5.BackColor = System.Drawing.Color.Transparent;
		this.c1Label5.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label5.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label5.ForeColor = System.Drawing.Color.Black;
		this.c1Label5.Location = new System.Drawing.Point(14, 53);
		this.c1Label5.Name = "c1Label5";
		this.c1Label5.Size = new System.Drawing.Size(100, 24);
		this.c1Label5.TabIndex = 2;
		this.c1Label5.Tag = null;
		this.c1Label5.Text = "替换方式：";
		this.c1Label5.TextDetached = true;
		this.c1Label4.AutoSize = true;
		this.c1Label4.BackColor = System.Drawing.Color.Transparent;
		this.c1Label4.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label4.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.c1Label4.ForeColor = System.Drawing.Color.Black;
		this.c1Label4.Location = new System.Drawing.Point(14, 15);
		this.c1Label4.Name = "c1Label4";
		this.c1Label4.Size = new System.Drawing.Size(82, 24);
		this.c1Label4.TabIndex = 1;
		this.c1Label4.Tag = null;
		this.c1Label4.Text = "替换为：";
		this.c1Label4.TextDetached = true;
		this.cmbReplaceModeList.AllowSpinLoop = false;
		this.cmbReplaceModeList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.cmbReplaceModeList.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.cmbReplaceModeList.GapHeight = 0;
		this.cmbReplaceModeList.ImagePadding = new System.Windows.Forms.Padding(0);
		this.cmbReplaceModeList.ItemsDisplayMember = "";
		this.cmbReplaceModeList.ItemsValueMember = "";
		this.cmbReplaceModeList.Location = new System.Drawing.Point(97, 51);
		this.cmbReplaceModeList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.cmbReplaceModeList.Name = "cmbReplaceModeList";
		this.cmbReplaceModeList.Size = new System.Drawing.Size(117, 29);
		this.cmbReplaceModeList.TabIndex = 0;
		this.cmbReplaceModeList.Tag = null;
		this.cmbReplaceModeList.TextDetached = true;
		this.txtFindText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtFindText.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtFindText.Location = new System.Drawing.Point(97, 7);
		this.txtFindText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtFindText.Name = "txtFindText";
		this.txtFindText.Size = new System.Drawing.Size(257, 29);
		this.txtFindText.TabIndex = 4;
		this.txtFindText.Tag = null;
		this.txtFindText.TextDetached = true;
		this.txtFindText.TextChanged += new System.EventHandler(txtFindText_TextChanged);
		this.txtReplaceText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtReplaceText.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtReplaceText.Location = new System.Drawing.Point(97, 12);
		this.txtReplaceText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.txtReplaceText.Name = "txtReplaceText";
		this.txtReplaceText.Size = new System.Drawing.Size(257, 29);
		this.txtReplaceText.TabIndex = 5;
		this.txtReplaceText.Tag = null;
		this.txtReplaceText.TextDetached = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(11f, 24f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BackgroundColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(484, 241);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FindReplace";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "查找替换";
		base.TopMost = true;
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(FindReplace_MouseDown);
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlFindText.ResumeLayout(false);
		this.pnlFindText.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.cboScope).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblScope).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckbIsMatchCase).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnFindNext).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnReplace1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cmbMatchModeList).EndInit();
		this.pnlReplaceText.ResumeLayout(false);
		this.pnlReplaceText.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.btnRepalce2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnReplaceAll).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label5).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label4).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cmbReplaceModeList).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtFindText).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtReplaceText).EndInit();
		base.ResumeLayout(false);
	}
}
