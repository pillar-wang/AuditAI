﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class ReplaceForm : C1RibbonForm
{
	private readonly int formHeight;

	private readonly int ctnAllHeight;

	private readonly int pnlFindHeight;

	private readonly int pnlReplaceHeight;

	private Dictionary<string, ScopeMode> ScopeModeDic = new Dictionary<string, ScopeMode>
	{
		{
			"当前文档",
			ScopeMode.Current
		},
		{
			"全部文档",
			ScopeMode.Global
		}
	};

	private bool IsReplace;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlReplace;

	private C1SplitterPanel pnlFind;

	private C1Button btnDisplayReplace;

	private C1Button btnReplaceAll;

	private C1Button btnNext;

	private C1TextBoxEx txtSearchTarget;

	private C1TextBoxEx txtReplaceBy;

	private C1Label c1Label2;

	private C1Label c1Label1;

	private C1CheckBox ckWholeWord;

	private C1CheckBox ckMatchCase;

	private C1Button btnReplace2;

	private C1Label lblScope;

	private C1ComboBox cboScope;

	public event EventHandler<FindReplaceEventArgs> Find_NextKeyDown;

	public event EventHandler<FindReplaceEventArgs> Replace_NextKeyDown;

	public event EventHandler<FindReplaceEventArgs> Replace_ReplaceKeyDown;

	public event EventHandler<FindReplaceEventArgs> Replace_ReplaceAllKeyDown;

	#region 按钮尺寸规范
	private const int ButtonWidth = 110;
	private const int ButtonHeight = 34;
	private const int ButtonRightMargin = 50;
	/// <summary>按钮圆角半径（统一规范 8px）</summary>
	private const int ButtonRadius = 8;
	#endregion

	#region === 设计令牌（轻盈清新风格，支持主题切换） ===

	private Color Primary;
	private Color PrimaryDark;
	private Color PrimaryLight;
	private Color LineColorDefault;
	private Color TextPrimary;
	private Color Surface0;

	/// <summary>根据当前主题加载颜色令牌：Google 蓝主题走清新蓝色板，其余走默认小清新浅蓝</summary>
	private void LoadThemeColors()
	{
		var theme = Theme.SelectedAuditaiTheme;
		if (theme != null && theme.Name == "auditai_GoogleBlue")
		{
			// 清新蓝色板
			Primary = Color.FromArgb(59, 130, 246);
			PrimaryDark = Color.FromArgb(29, 78, 216);
			PrimaryLight = Color.FromArgb(37, 99, 235);
			LineColorDefault = Color.FromArgb(229, 231, 235);
			TextPrimary = Color.FromArgb(30, 41, 59);
			Surface0 = Color.FromArgb(250, 251, 252);
		}
		else
		{
			// 默认：小清新浅蓝
			Primary = Color.FromArgb(74, 144, 217);
			PrimaryDark = Color.FromArgb(53, 123, 189);
			PrimaryLight = Color.FromArgb(90, 160, 230);
			LineColorDefault = Color.FromArgb(229, 231, 235);
			TextPrimary = Color.FromArgb(30, 41, 59);
			Surface0 = Color.FromArgb(245, 249, 252);
		}
	}

	#endregion

	/// <summary>
	/// 统一按钮样式：主按钮=蓝底白字无边框；次按钮=白底深灰字+浅灰边框；均 Flat + 8px 圆角 Region
	/// </summary>
	private void ApplyButtonStyle(C1Button btn, bool isPrimary)
	{
		btn.FlatStyle = FlatStyle.Flat;
		if (isPrimary)
		{
			// 主按钮：Primary 蓝填充 + 白字（悬停/按下深浅变体）
			btn.BackColor = Primary;
			btn.ForeColor = Color.White;
			btn.FlatAppearance.BorderSize = 0;
			btn.FlatAppearance.MouseDownBackColor = PrimaryDark;
			btn.FlatAppearance.MouseOverBackColor = PrimaryLight;
		}
		else
		{
			// 次按钮：白底深灰字 + 浅灰边框，视觉低调
			btn.BackColor = Color.White;
			btn.ForeColor = TextPrimary;
			btn.FlatAppearance.BorderColor = LineColorDefault;
			btn.FlatAppearance.BorderSize = 1;
			btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(240, 247, 252);
			btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 249, 252);
		}
		// 8px 圆角 Region（四角 AddArc，同 MessageShowBox）
		using (var path = new GraphicsPath())
		{
			int r = ButtonRadius * 2;
			path.AddArc(0, 0, r, r, 180, 90);
			path.AddArc(btn.Width - r, 0, r, r, 270, 90);
			path.AddArc(btn.Width - r, btn.Height - r, r, r, 0, 90);
			path.AddArc(0, btn.Height - r, r, r, 90, 90);
			path.CloseFigure();
			btn.Region = new Region(path);
		}
	}

	/// <summary>统一重设按钮样式（构造末尾调用；主题应用后再次调用防 C1Theme 覆盖，同 UpdateForm.EnsureStylesCorrect）</summary>
	private void RefreshButtonStyles()
	{
		// 查找下一个/全部替换=主动作主按钮；替换/显示替换区=次按钮
		ApplyButtonStyle(btnNext, isPrimary: true);
		ApplyButtonStyle(btnReplaceAll, isPrimary: true);
		ApplyButtonStyle(btnReplace2, isPrimary: false);
		ApplyButtonStyle(btnDisplayReplace, isPrimary: false);
	}

	public ReplaceForm()
	{
		// 先加载主题颜色令牌（须在 InitializeComponent 之前，同 MessageShowBox）
		LoadThemeColors();
		InitializeComponent();
		base.TopMost = true;
		base.StartPosition = FormStartPosition.CenterScreen;
		formHeight = base.Height;
		ctnAllHeight = ctnAll.Height;
		pnlFindHeight = pnlFind.Height;
		pnlReplaceHeight = pnlReplace.Height;
		cboScope.ItemsDataSource = ScopeModeDic.Keys;
		cboScope.DataSource = ScopeModeDic.Values;
		cboScope.SelectedIndex = 0;
		// 统一按钮样式 + 8px 圆角（构造末尾统一应用）
		RefreshButtonStyles();
	}

	public void SetCanReplace(bool canReplace = true)
	{
		btnDisplayReplace.Visible = canReplace;
	}

	public void Show(bool IsReplace)
	{
		this.IsReplace = IsReplace;
		if (this.IsReplace)
		{
			Text = "查找替换";
			base.Height = formHeight;
			ctnAll.Height = ctnAllHeight;
			pnlReplace.Visible = true;
			btnDisplayReplace.Visible = false;
			btnNext.Enabled = true;
			btnReplace2.Enabled = true;
			btnReplaceAll.Enabled = true;
			base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.Replace);
		}
		else
		{
			Text = "查找替换";
			ctnAll.Height = pnlFindHeight;
			base.Height = formHeight - pnlReplaceHeight;
			pnlReplace.Visible = false;
			btnDisplayReplace.Visible = true;
			btnNext.Enabled = false;
			btnReplace2.Enabled = false;
			btnReplaceAll.Enabled = false;
			base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(IconRes.Replace);
		}
		base.ActiveControl = txtSearchTarget;
		Show();
	}

	private SearchFlag GetSearchFlag()
	{
		SearchFlag searchFlag = SearchFlag.None;
		searchFlag |= (ckMatchCase.Checked ? SearchFlag.Case : SearchFlag.None);
		searchFlag |= (ckWholeWord.Checked ? SearchFlag.WholeWord : SearchFlag.None);
		if (cboScope.SelectedIndex == 0)
		{
			searchFlag |= SearchFlag.ScopeCurrent;
		}
		if (cboScope.SelectedIndex == 1)
		{
			searchFlag |= SearchFlag.ScopeGlobal;
		}
		return searchFlag;
	}

	private void btnDisplayReplace_Click(object sender, EventArgs e)
	{
		Text = "查找替换";
		if (base.Height != formHeight)
		{
			base.Height = formHeight;
		}
		ctnAll.Height = ctnAllHeight;
		pnlReplace.Visible = true;
		btnDisplayReplace.Visible = false;
	}

	private void btnNext_Click(object sender, EventArgs e)
	{
		if (IsReplace)
		{
			this.Replace_NextKeyDown?.Invoke(this, new FindReplaceEventArgs(txtSearchTarget.Text, txtReplaceBy.Text, GetSearchFlag()));
			return;
		}
		this.Find_NextKeyDown?.Invoke(this, new FindReplaceEventArgs
		{
			SearchFlag = GetSearchFlag(),
			SearchText = txtSearchTarget.Text
		});
	}

	private void btnReplace_Click(object sender, EventArgs e)
	{
		if (txtReplaceBy.Text.Equals(txtSearchTarget.Text))
		{
			MessageBox.Show(MessageBoxIcon.None, "查找替换文本相同");
		}
		else
		{
			this.Replace_ReplaceKeyDown?.Invoke(this, new FindReplaceEventArgs(txtSearchTarget.Text, txtReplaceBy.Text, GetSearchFlag()));
		}
	}

	private void btnReplaceAll_Click(object sender, EventArgs e)
	{
		if (txtReplaceBy.Text.Equals(txtSearchTarget.Text))
		{
			MessageBox.Show(MessageBoxIcon.None, "查找替换文本相同");
		}
		else
		{
			this.Replace_ReplaceAllKeyDown?.Invoke(this, new FindReplaceEventArgs(txtSearchTarget.Text, txtReplaceBy.Text, GetSearchFlag()));
		}
	}

	private void txtSearchTarget_TextChanged(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(txtSearchTarget.Text))
		{
			btnNext.Enabled = false;
			btnReplace2.Enabled = false;
			btnReplaceAll.Enabled = false;
		}
		else
		{
			btnNext.Enabled = true;
			btnReplace2.Enabled = true;
			btnReplaceAll.Enabled = true;
		}
	}

	public void ShowFind()
	{
		Theme.SetCurrentTree(this);
		// 主题应用后重设按钮样式，防止被 C1Theme 覆盖
		RefreshButtonStyles();
		Show(IsReplace: false);
	}

	public void ShowReplace()
	{
		Theme.SetCurrentTree(this);
		// 主题应用后重设按钮样式，防止被 C1Theme 覆盖
		RefreshButtonStyles();
		Show(IsReplace: true);
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
		this.pnlFind = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.cboScope = new C1.Win.C1Input.C1ComboBox();
		this.lblScope = new C1.Win.C1Input.C1Label();
		this.btnDisplayReplace = new C1.Win.C1Input.C1Button();
		this.btnNext = new C1.Win.C1Input.C1Button();
		this.txtSearchTarget = new Auditai.UI.Controls.C1TextBoxEx();
		this.c1Label1 = new C1.Win.C1Input.C1Label();
		this.ckWholeWord = new C1.Win.C1Input.C1CheckBox();
		this.ckMatchCase = new C1.Win.C1Input.C1CheckBox();
		this.pnlReplace = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.btnReplace2 = new C1.Win.C1Input.C1Button();
		this.btnReplaceAll = new C1.Win.C1Input.C1Button();
		this.c1Label2 = new C1.Win.C1Input.C1Label();
		this.txtReplaceBy = new Auditai.UI.Controls.C1TextBoxEx();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlFind.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.cboScope).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblScope).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnDisplayReplace).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnNext).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtSearchTarget).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckWholeWord).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ckMatchCase).BeginInit();
		this.pnlReplace.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.btnReplace2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.btnReplaceAll).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtReplaceBy).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		// 统一设计语言：旧蓝紫背景 → 浅色背景
		this.ctnAll.BackColor = Surface0;
		this.ctnAll.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		// 旧蓝分隔线 → 统一浅灰边框色
		this.ctnAll.FixedLineColor = LineColorDefault;
		// 旧深蓝前景 → 统一深灰文字色
		this.ctnAll.ForeColor = TextPrimary;
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlFind);
		this.ctnAll.Panels.Add(this.pnlReplace);
		this.ctnAll.Size = new System.Drawing.Size(629, 322);
		// 旧蓝分隔条 → 统一浅灰
		this.ctnAll.SplitterColor = LineColorDefault;
		this.ctnAll.SplitterWidth = 2;
		this.ctnAll.TabIndex = 0;
		this.ctnAll.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlFind.Controls.Add(this.cboScope);
		this.pnlFind.Controls.Add(this.lblScope);
		this.pnlFind.Controls.Add(this.btnDisplayReplace);
		this.pnlFind.Controls.Add(this.btnNext);
		this.pnlFind.Controls.Add(this.txtSearchTarget);
		this.pnlFind.Controls.Add(this.c1Label1);
		this.pnlFind.Controls.Add(this.ckWholeWord);
		this.pnlFind.Controls.Add(this.ckMatchCase);
		this.pnlFind.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑（字号不变）
		this.pnlFind.Height = 182;
		this.pnlFind.KeepRelativeSize = false;
		this.pnlFind.Location = new System.Drawing.Point(0, 0);
		this.pnlFind.Name = "pnlFind";
		this.pnlFind.Resizable = false;
		this.pnlFind.Size = new System.Drawing.Size(629, 182);
		this.pnlFind.SizeRatio = 56.68;
		this.pnlFind.TabIndex = 1;
		this.cboScope.AllowSpinLoop = false;
		this.cboScope.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.cboScope.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		this.cboScope.GapHeight = 0;
		this.cboScope.ImagePadding = new System.Windows.Forms.Padding(0);
		this.cboScope.ItemsDisplayMember = "";
		this.cboScope.ItemsValueMember = "";
		this.cboScope.Location = new System.Drawing.Point(112, 60);
		this.cboScope.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.cboScope.Name = "cboScope";
		this.cboScope.Size = new System.Drawing.Size(152, 38);
		this.cboScope.TabIndex = 14;
		this.cboScope.Tag = null;
		this.cboScope.TextDetached = true;
		this.lblScope.AutoSize = true;
		this.lblScope.BackColor = System.Drawing.Color.Transparent;
		this.lblScope.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblScope.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑
		// 界面文字统一深灰（原 Color.Black）
		this.lblScope.ForeColor = TextPrimary;
		this.lblScope.Location = new System.Drawing.Point(16, 60);
		this.lblScope.Name = "lblScope";
		this.lblScope.Size = new System.Drawing.Size(130, 31);
		this.lblScope.TabIndex = 8;
		this.lblScope.Tag = null;
		this.lblScope.Text = "查找范围：";
		this.lblScope.TextDetached = true;
		// 移除旧主题残留 VisualStyleBaseStyle=Office2007Blue（外观改由统一设计语言/C1 主题接管，删除后不影响使用）
		this.btnDisplayReplace.Location = new System.Drawing.Point(629 - ButtonRightMargin - ButtonWidth, 96);
		this.btnDisplayReplace.Name = "btnDisplayReplace";
		this.btnDisplayReplace.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnDisplayReplace.TabIndex = 13;
		this.btnDisplayReplace.Text = "替换";
		// 去除 UseVisualStyleBackColor，改用统一次按钮样式（RefreshButtonStyles 应用）
		this.btnDisplayReplace.Click += new System.EventHandler(btnDisplayReplace_Click);
		this.btnNext.Location = new System.Drawing.Point(629 - ButtonRightMargin - ButtonWidth, 13);
		this.btnNext.Name = "btnNext";
		this.btnNext.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnNext.TabIndex = 12;
		this.btnNext.Text = "查找下一个";
		// 去除 UseVisualStyleBackColor，改用统一主按钮样式（RefreshButtonStyles 应用）
		this.btnNext.Click += new System.EventHandler(btnNext_Click);
		this.txtSearchTarget.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtSearchTarget.Location = new System.Drawing.Point(112, 16);
		this.txtSearchTarget.Name = "txtSearchTarget";
		this.txtSearchTarget.Size = new System.Drawing.Size(348, 38);
		this.txtSearchTarget.TabIndex = 9;
		this.txtSearchTarget.Tag = null;
		this.txtSearchTarget.TextDetached = true;
		this.txtSearchTarget.TextChanged += new System.EventHandler(txtSearchTarget_TextChanged);
		this.c1Label1.AutoSize = true;
		this.c1Label1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label1.Location = new System.Drawing.Point(16, 20);
		this.c1Label1.Name = "c1Label1";
		this.c1Label1.Size = new System.Drawing.Size(130, 31);
		this.c1Label1.TabIndex = 6;
		this.c1Label1.Tag = null;
		this.c1Label1.Text = "查找目标：";
		this.c1Label1.TextDetached = true;
		this.ckWholeWord.BackColor = System.Drawing.Color.Transparent;
		this.ckWholeWord.BorderColor = System.Drawing.Color.Transparent;
		this.ckWholeWord.BorderStyle = System.Windows.Forms.BorderStyle.None;
		// 界面文字统一深灰（原 Color.Black）
		this.ckWholeWord.ForeColor = TextPrimary;
		this.ckWholeWord.Location = new System.Drawing.Point(16, 135);
		this.ckWholeWord.Name = "ckWholeWord";
		this.ckWholeWord.Padding = new System.Windows.Forms.Padding(1);
		this.ckWholeWord.Size = new System.Drawing.Size(135, 31);
		this.ckWholeWord.TabIndex = 4;
		this.ckWholeWord.Text = "全词匹配";
		this.ckWholeWord.UseVisualStyleBackColor = true;
		this.ckWholeWord.Value = null;
		this.ckMatchCase.BackColor = System.Drawing.Color.Transparent;
		this.ckMatchCase.BorderColor = System.Drawing.Color.Transparent;
		this.ckMatchCase.BorderStyle = System.Windows.Forms.BorderStyle.None;
		// 界面文字统一深灰（原 Color.Black）
		this.ckMatchCase.ForeColor = TextPrimary;
		this.ckMatchCase.Location = new System.Drawing.Point(16, 99);
		this.ckMatchCase.Name = "ckMatchCase";
		this.ckMatchCase.Padding = new System.Windows.Forms.Padding(1);
		this.ckMatchCase.Size = new System.Drawing.Size(135, 31);
		this.ckMatchCase.TabIndex = 3;
		this.ckMatchCase.Text = "区分大小写";
		this.ckMatchCase.UseVisualStyleBackColor = true;
		this.ckMatchCase.Value = null;
		this.pnlReplace.Controls.Add(this.btnReplace2);
		this.pnlReplace.Controls.Add(this.btnReplaceAll);
		this.pnlReplace.Controls.Add(this.c1Label2);
		this.pnlReplace.Controls.Add(this.txtReplaceBy);
		this.pnlReplace.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlReplace.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134); // 字体统一 微软雅黑（字号不变）
		this.pnlReplace.Height = 139;
		this.pnlReplace.Location = new System.Drawing.Point(0, 183);
		this.pnlReplace.MinHeight = 0;
		this.pnlReplace.Name = "pnlReplace";
		this.pnlReplace.Resizable = false;
		this.pnlReplace.Size = new System.Drawing.Size(629, 139);
		this.pnlReplace.SizeRatio = 26.923;
		this.pnlReplace.TabIndex = 0;
		this.btnReplace2.Location = new System.Drawing.Point(629 - ButtonRightMargin - ButtonWidth, 20);
		this.btnReplace2.Name = "btnReplace2";
		this.btnReplace2.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnReplace2.TabIndex = 14;
		this.btnReplace2.Text = "替换";
		// 去除 UseVisualStyleBackColor，改用统一次按钮样式（RefreshButtonStyles 应用）
		this.btnReplace2.Click += new System.EventHandler(btnReplace_Click);
		this.btnReplaceAll.Location = new System.Drawing.Point(629 - ButtonRightMargin - ButtonWidth, 77);
		this.btnReplaceAll.Name = "btnReplaceAll";
		this.btnReplaceAll.Size = new System.Drawing.Size(ButtonWidth, ButtonHeight);
		this.btnReplaceAll.TabIndex = 0;
		this.btnReplaceAll.Text = "全部替换";
		// 去除 UseVisualStyleBackColor，改用统一主按钮样式（RefreshButtonStyles 应用）
		this.btnReplaceAll.Click += new System.EventHandler(btnReplaceAll_Click);
		this.c1Label2.AutoSize = true;
		this.c1Label2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.c1Label2.Location = new System.Drawing.Point(16, 22);
		this.c1Label2.Name = "c1Label2";
		this.c1Label2.Size = new System.Drawing.Size(92, 31);
		this.c1Label2.TabIndex = 7;
		this.c1Label2.Tag = null;
		this.c1Label2.Text = "替换为：";
		this.c1Label2.TextDetached = true;
		this.txtReplaceBy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtReplaceBy.Location = new System.Drawing.Point(112, 22);
		this.txtReplaceBy.Name = "txtReplaceBy";
		this.txtReplaceBy.Size = new System.Drawing.Size(348, 38);
		this.txtReplaceBy.TabIndex = 8;
		this.txtReplaceBy.Tag = null;
		this.txtReplaceBy.TextDetached = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(14f, 31f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(629, 322);
		base.Controls.Add(this.ctnAll);
		this.Font = new System.Drawing.Font("微软雅黑", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(5);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ReplaceForm";
		base.ShowInTaskbar = false;
		this.Text = "替换";
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlFind.ResumeLayout(false);
		this.pnlFind.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.cboScope).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblScope).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnDisplayReplace).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnNext).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtSearchTarget).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckWholeWord).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ckMatchCase).EndInit();
		this.pnlReplace.ResumeLayout(false);
		this.pnlReplace.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.btnReplace2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.btnReplaceAll).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1Label2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtReplaceBy).EndInit();
		base.ResumeLayout(false);
	}
}
