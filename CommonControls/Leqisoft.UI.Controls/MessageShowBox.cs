﻿﻿﻿﻿﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class MessageShowBox : C1RibbonForm
{
	private readonly MessageBoxButtons _buttonType;

	private readonly MessageBoxIcon _boxIcon;

	private string _message = string.Empty;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer ctnAll;

	private C1SplitterPanel pnlImage;

	private C1PictureBox MessageIcon;

	private C1SplitterPanel pnlButton;

	private C1SplitterPanel pnlContent;

	private C1TextBoxEx txtNotice;

	private C1Label lblNotice;

	public MessageShowBox(MessageBoxButtons buttonType, MessageBoxIcon boxIcon)
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
		txtNotice.BackColor = Color.FromArgb(241, 241, 241);
		ctnAll.BorderColor = Color.Blue;
		base.TopMost = true;
		base.HScroll = true;
		_buttonType = buttonType;
		_boxIcon = boxIcon;
		base.FormClosing += MessageShowBox_FormClosing;
		switch (_boxIcon)
		{
		case MessageBoxIcon.Asterisk:
			Text = "提示";
			MessageIcon.BackgroundImage = Resource1.提示;
			break;
		case MessageBoxIcon.Question:
			Text = "询问";
			MessageIcon.BackgroundImage = Resource1.问号;
			break;
		case MessageBoxIcon.Exclamation:
			Text = "警告";
			MessageIcon.BackgroundImage = Resource1.警告;
			break;
		case MessageBoxIcon.Hand:
			Text = "错误";
			MessageIcon.BackgroundImage = Resource1.错误;
			break;
		default:
			Text = "提示";
			MessageIcon.BackgroundImage = Resource1.提示;
			break;
		}
		switch (_buttonType)
		{
		case MessageBoxButtons.OK:
			InitOkView();
			break;
		case MessageBoxButtons.OKCancel:
			InitOkCancelView();
			break;
		case MessageBoxButtons.YesNo:
			InitYesNoView();
			break;
		case MessageBoxButtons.YesNoCancel:
			InitYesNoCancelView();
			break;
		case MessageBoxButtons.AbortRetryIgnore:
			break;
		}
	}

	internal void SetMessage(string message)
	{
		_message = message;
		txtNotice.TextDetached = true;
		txtNotice.Text = message;
		txtNotice.ReadOnly = true;
		txtNotice.ScrollBars = ((message.Length > 200) ? ScrollBars.Vertical : ScrollBars.None);
		lblNotice.TextDetached = true;
		lblNotice.Text = message;
	}

	public new DialogResult ShowDialog()
	{
		ShowScrollView(_message.Length > 150);
		Theme.SetCurrentTree(this);
		StandardView();
		return base.ShowDialog();
	}

	private C1Button NewButton(string text)
	{
		return new C1Button
		{
			Text = text,
			Width = 70,
			Height = 26,
			Font = new Font("微软雅黑", 9f),
			Anchor = (AnchorStyles.Bottom | AnchorStyles.Right)
		};
	}

	private void AnchorPosition1(C1SplitterPanel panel, Control control)
	{
		panel.Controls.Add(control);
		control.Top = (panel.Height - 26) / 2;
		control.Left = panel.Width - 300;
	}

	private void AnchorPosition2(C1SplitterPanel panel, Control control)
	{
		panel.Controls.Add(control);
		control.Top = (panel.Height - 26) / 2;
		control.Left = panel.Width - 200;
	}

	private void AnchorPosition3(C1SplitterPanel panel, Control control)
	{
		panel.Controls.Add(control);
		control.Top = (panel.Height - 26) / 2;
		control.Left = panel.Width - 100;
	}

	private void InitYesNoView()
	{
		C1Button c1Button = NewButton("是");
		C1Button c1Button2 = NewButton("否");
		AnchorPosition2(pnlButton, c1Button);
		AnchorPosition3(pnlButton, c1Button2);
		c1Button.Click += delegate
		{
			base.DialogResult = DialogResult.Yes;
			Close();
		};
		c1Button2.Click += delegate
		{
			base.DialogResult = DialogResult.No;
			Close();
		};
		c1Button.Focus();
	}

	private void InitYesNoCancelView()
	{
		C1Button c1Button = NewButton("是");
		C1Button c1Button2 = NewButton("否");
		C1Button c1Button3 = NewButton("取消");
		AnchorPosition1(pnlButton, c1Button);
		AnchorPosition2(pnlButton, c1Button2);
		AnchorPosition3(pnlButton, c1Button3);
		c1Button.Click += delegate
		{
			base.DialogResult = DialogResult.Yes;
			Close();
		};
		c1Button2.Click += delegate
		{
			base.DialogResult = DialogResult.No;
			Close();
		};
		c1Button3.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		};
		c1Button.Focus();
	}

	private void InitOkView()
	{
		C1Button c1Button = NewButton("确定");
		AnchorPosition3(pnlButton, c1Button);
		c1Button.Click += delegate
		{
			base.DialogResult = DialogResult.OK;
			Close();
		};
		c1Button.Focus();
	}

	private void MessageShowBox_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.DialogResult == DialogResult.None)
		{
			base.DialogResult = DialogResult.Cancel;
		}
	}

	private void InitOkCancelView()
	{
		C1Button c1Button = NewButton("确定");
		C1Button c1Button2 = NewButton("取消");
		AnchorPosition2(pnlButton, c1Button);
		AnchorPosition3(pnlButton, c1Button2);
		c1Button.Click += delegate
		{
			base.DialogResult = DialogResult.OK;
			Close();
		};
		c1Button2.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		};
		c1Button.Focus();
	}

	private void ShowScrollView(bool scroll)
	{
		if (scroll)
		{
			lblNotice.Hide();
			txtNotice.Show();
			txtNotice.BringToFront();
		}
		else
		{
			txtNotice.Hide();
			lblNotice.Show();
			lblNotice.BringToFront();
		}
	}

	private void StandardView()
	{
		lblNotice.Font = new Font("微软雅黑", 9f);
		txtNotice.Font = new Font("微软雅黑", 9f);
		txtNotice.BorderStyle = BorderStyle.None;
		ctnAll.SplitterWidth = 0;
		pnlImage.BorderWidth = 0;
		pnlContent.BorderWidth = 0;
	}

	private void txtNotice_BorderStyleChanged(object sender, EventArgs e)
	{
		if (txtNotice.BorderStyle != 0)
		{
			txtNotice.BorderColor = Color.Red;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Leqisoft.UI.Controls.MessageShowBox));
		this.ctnAll = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlButton = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.pnlImage = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.MessageIcon = new C1.Win.C1Input.C1PictureBox();
		this.pnlContent = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.lblNotice = new C1.Win.C1Input.C1Label();
		this.txtNotice = new Leqisoft.UI.Controls.C1TextBoxEx();
		((System.ComponentModel.ISupportInitialize)this.ctnAll).BeginInit();
		this.ctnAll.SuspendLayout();
		this.pnlImage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.MessageIcon).BeginInit();
		this.pnlContent.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.lblNotice).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtNotice).BeginInit();
		base.SuspendLayout();
		this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.ctnAll.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.ctnAll.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.ctnAll.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ctnAll.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.ctnAll.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.ctnAll.Location = new System.Drawing.Point(0, 0);
		this.ctnAll.Name = "ctnAll";
		this.ctnAll.Panels.Add(this.pnlButton);
		this.ctnAll.Panels.Add(this.pnlImage);
		this.ctnAll.Panels.Add(this.pnlContent);
		this.ctnAll.Size = new System.Drawing.Size(512, 189);
		this.ctnAll.SplitterWidth = 0;
		this.ctnAll.TabIndex = 0;
		this.pnlButton.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Bottom;
		this.pnlButton.Height = 50;
		this.pnlButton.KeepRelativeSize = false;
		this.pnlButton.Location = new System.Drawing.Point(0, 139);
		this.pnlButton.MinHeight = 50;
		this.pnlButton.MinWidth = 41;
		this.pnlButton.Name = "pnlButton";
		this.pnlButton.Resizable = false;
		this.pnlButton.Size = new System.Drawing.Size(512, 50);
		this.pnlButton.SizeRatio = 26.596;
		this.pnlButton.TabIndex = 1;
		this.pnlButton.Width = 512;
		this.pnlImage.Controls.Add(this.MessageIcon);
		this.pnlImage.Dock = C1.Win.C1SplitContainer.PanelDockStyle.Left;
		this.pnlImage.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.pnlImage.Height = 138;
		this.pnlImage.KeepRelativeSize = false;
		this.pnlImage.Location = new System.Drawing.Point(0, 0);
		this.pnlImage.MinHeight = 41;
		this.pnlImage.MinWidth = 92;
		this.pnlImage.Name = "pnlImage";
		this.pnlImage.Size = new System.Drawing.Size(92, 138);
		this.pnlImage.SizeRatio = 18.0;
		this.pnlImage.TabIndex = 0;
		this.pnlImage.Width = 92;
		this.MessageIcon.BackColor = System.Drawing.Color.Transparent;
		this.MessageIcon.BackgroundImage = (System.Drawing.Image)resources.GetObject("MessageIcon.BackgroundImage");
		this.MessageIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.MessageIcon.Dock = System.Windows.Forms.DockStyle.Fill;
		this.MessageIcon.Location = new System.Drawing.Point(0, 0);
		this.MessageIcon.Name = "MessageIcon";
		this.MessageIcon.Size = new System.Drawing.Size(92, 138);
		this.MessageIcon.TabIndex = 0;
		this.MessageIcon.TabStop = false;
		this.pnlContent.Controls.Add(this.lblNotice);
		this.pnlContent.Controls.Add(this.txtNotice);
		this.pnlContent.Height = 138;
		this.pnlContent.Location = new System.Drawing.Point(92, 0);
		this.pnlContent.Name = "pnlContent";
		this.pnlContent.Size = new System.Drawing.Size(420, 138);
		this.pnlContent.TabIndex = 2;
		this.lblNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblNotice.Dock = System.Windows.Forms.DockStyle.Left;
		this.lblNotice.Location = new System.Drawing.Point(0, 0);
		this.lblNotice.Name = "lblNotice";
		this.lblNotice.Size = new System.Drawing.Size(392, 138);
		this.lblNotice.TabIndex = 1;
		this.lblNotice.Tag = null;
		this.lblNotice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.txtNotice.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtNotice.Location = new System.Drawing.Point(0, 0);
		this.txtNotice.Multiline = true;
		this.txtNotice.Name = "txtNotice";
		this.txtNotice.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
		this.txtNotice.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtNotice.Size = new System.Drawing.Size(420, 138);
		this.txtNotice.TabIndex = 0;
		this.txtNotice.Tag = null;
		this.txtNotice.TextDetached = true;
		this.txtNotice.VerticalAlign = C1.Win.C1Input.VerticalAlignEnum.Middle;
		this.txtNotice.BorderStyleChanged += new System.EventHandler(txtNotice_BorderStyleChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(512, 189);
		base.Controls.Add(this.ctnAll);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(300, 150);
		base.Name = "MessageShowBox";
		base.ShowInTaskbar = false;
		this.Text = "MessageShowBox";
		base.TopMost = true;
		((System.ComponentModel.ISupportInitialize)this.ctnAll).EndInit();
		this.ctnAll.ResumeLayout(false);
		this.pnlImage.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.MessageIcon).EndInit();
		this.pnlContent.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.lblNotice).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtNotice).EndInit();
		base.ResumeLayout(false);
	}
}
