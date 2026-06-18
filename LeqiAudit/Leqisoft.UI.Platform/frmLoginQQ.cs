﻿﻿﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using CefSharp;
using CefSharp.WinForms;
using Leqisoft.DTO;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;
using Leqisoft.Util;

namespace Leqisoft.UI.Platform;

internal class frmLoginQQ : C1RibbonForm
{
	private const string CODE = "code";

	private const string STATE = "state";

	private const string AppID = "101433004";

	private const string callback = "about:blank";

	private IContainer components;

	private C1Button btnClose;

	public Tuple<UserToken, User> resultMsg { get; set; }

	private void frmQQLogin_Paint(object sender, PaintEventArgs e)
	{
		Pen pen = new Pen(Color.FromArgb(0, 195, 245), 1f);
		e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
	}

	public frmLoginQQ()
	{
		InitializeComponent();
		// Cef.Initialize 未执行，不创建 ChromiumWebBrowser 实例
		// 避免析构时 Cef.RemoveDisposable() 因 Cef 运行时未初始化而崩溃
		base.Shown += FrmLoginQQ_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.Load += FrmQQLogin_Load;
		Theme.SetCurrentTree(this);
	}

	private void _browser_AddressChanged(object sender, AddressChangedEventArgs e)
	{
		Invoke((Action)async delegate
		{
			if (e.Address.StartsWith("http://leqisoft.com/logintemp.html"))
			{
				Uri url = new Uri(e.Address);
				if (url.TryGetValue("code", out var value) && url.TryGetValue("state", out var value2))
				{
					try
					{
						Tuple<UserToken, User> tuple = await WebApiClient.QQLogin(value, value2);
						resultMsg = tuple;
						await Task.Delay(1000);
					}
					catch (HttpRequestException ex)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
					}
					catch (TimeoutException ex2)
					{
						Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
					}
					finally
					{
						Close();
					}
				}
			}
		});
	}

	private void FrmLoginQQ_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.qq_img);
	}

	private void FrmQQLogin_Load(object sender, EventArgs e)
	{
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
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
		this.btnClose = new C1.Win.C1Input.C1Button();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		base.SuspendLayout();
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.close2;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Location = new System.Drawing.Point(771, -1);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(25, 25);
		this.btnClose.TabIndex = 2;
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Visible = false;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(796, 493);
		base.Controls.Add(this.btnClose);
		base.Name = "frmLoginQQ";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "QQ登录";
		base.Paint += new System.Windows.Forms.PaintEventHandler(frmQQLogin_Paint);
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		base.ResumeLayout(false);
	}
}
